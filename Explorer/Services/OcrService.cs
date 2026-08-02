using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Explorer.Models;
using RapidOcrNet;

namespace Explorer.Services;

public sealed class OcrService
{
	private const int SnippetRadius = 40;
	private const int ThreadsPerEngine = 2;

	public static readonly int Degree = Math.Clamp(Environment.ProcessorCount / 2, 1, 4);

	private static readonly string CachePath = Path.Combine(
		Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Explorer", "ocr-cache.json");

	private static readonly string ModelsDirectory = Path.Combine(AppContext.BaseDirectory, "models", "v5");

	private static readonly JsonSerializerOptions JsonOptions = new()
	{
		PropertyNameCaseInsensitive = true,
		WriteIndented = true
	};

	private readonly ConcurrentDictionary<string, OcrCacheEntry> _cache =
		new(StringComparer.OrdinalIgnoreCase);

	private readonly ConcurrentBag<RapidOcr> _engines = [];
	private readonly SemaphoreSlim _gate = new(Degree, Degree);

	private int _pendingWrites;

	public OcrService()
	{
		foreach (var entry in LoadCache())
			_cache[entry.FullPath] = entry;
	}

	public async Task<string> ExtractTextAsync(string filePath, CancellationToken token)
	{
		long length;
		long ticks;

		try
		{
			var info = new FileInfo(filePath);
			if (!info.Exists)
				return string.Empty;

			length = info.Length;
			ticks = info.LastWriteTimeUtc.Ticks;
		}
		catch (Exception ex)
		{
			Debug.WriteLine(ex.Message);
			return string.Empty;
		}

		if (_cache.TryGetValue(filePath, out var cached) &&
		    cached.Length == length &&
		    cached.LastWriteTicks == ticks)
			return cached.Text;

		var text = await RecognizeAsync(filePath, token);

		_cache[filePath] = new OcrCacheEntry(filePath, length, ticks, text);
		Interlocked.Increment(ref _pendingWrites);

		return text;
	}

	public void SaveCache()
	{
		if (Interlocked.Exchange(ref _pendingWrites, 0) == 0)
			return;

		try
		{
			Directory.CreateDirectory(Path.GetDirectoryName(CachePath)!);
			File.WriteAllText(CachePath, JsonSerializer.Serialize(_cache.Values.ToArray(), JsonOptions));
		}
		catch (Exception ex)
		{
			Debug.WriteLine(ex.Message);
		}
	}

	public static bool Matches(string text, string term, out string snippet)
	{
		snippet = string.Empty;

		if (string.IsNullOrEmpty(text) || string.IsNullOrWhiteSpace(term))
			return false;

		var index = CultureInfo.CurrentCulture.CompareInfo.IndexOf(
			text.AsSpan(), term.AsSpan(),
			CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace,
			out var matchLength);

		if (index < 0)
			return false;

		snippet = BuildSnippet(text, index, matchLength);
		return true;
	}

	private async Task<string> RecognizeAsync(string filePath, CancellationToken token)
	{
		await _gate.WaitAsync(token);

		try
		{
			return await Task.Run(() =>
			{
				RapidOcr? engine = null;

				try
				{
					if (!_engines.TryTake(out engine))
						engine = CreateEngine();

					return engine.Detect(filePath, RapidOcrOptions.Default).StrRes ?? string.Empty;
				}
				catch (Exception ex)
				{
					Debug.WriteLine(ex.Message);
					return string.Empty;
				}
				finally
				{
					if (engine is not null)
						_engines.Add(engine);
				}
			}, token);
		}
		finally
		{
			_gate.Release();
		}
	}

	private static RapidOcr CreateEngine()
	{
		var engine = new RapidOcr();

		engine.InitModels(
			Path.Combine(ModelsDirectory, "ch_PP-OCRv5_mobile_det.onnx"),
			Path.Combine(ModelsDirectory, "ch_PP-LCNet_x0_25_textline_ori_cls_mobile.onnx"),
			Path.Combine(ModelsDirectory, "latin_PP-OCRv5_rec_mobile_infer.onnx"),
			Path.Combine(ModelsDirectory, "ppocrv5_latin_dict.txt"),
			ThreadsPerEngine);

		return engine;
	}

	private static string BuildSnippet(string text, int index, int matchLength)
	{
		var start = Math.Max(0, index - SnippetRadius);
		var end = Math.Min(text.Length, index + matchLength + SnippetRadius);

		var window = string.Join(' ', text[start..end]
			.Split([' ', '\r', '\n', '\t'], StringSplitOptions.RemoveEmptyEntries));

		var prefix = start > 0 ? "… " : string.Empty;
		var suffix = end < text.Length ? " …" : string.Empty;

		return $"{prefix}{window}{suffix}";
	}

	private static OcrCacheEntry[] LoadCache()
	{
		try
		{
			if (!File.Exists(CachePath))
				return [];

			return JsonSerializer.Deserialize<OcrCacheEntry[]>(File.ReadAllText(CachePath), JsonOptions) ?? [];
		}
		catch (Exception ex)
		{
			Debug.WriteLine(ex.Message);
			return [];
		}
	}
}
