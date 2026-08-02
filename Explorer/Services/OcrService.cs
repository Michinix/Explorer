using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using RapidOcrNet;

namespace Explorer.Services;

public sealed class OcrService
{
	private const int SnippetRadius = 40;
	private const int ThreadsPerEngine = 2;

	public static readonly int Degree = Math.Clamp(Environment.ProcessorCount / 2, 1, 4);

	private static readonly string ModelsDirectory = Path.Combine(AppContext.BaseDirectory, "models", "v5");

	private readonly ConcurrentBag<RapidOcr> _engines = [];
	private readonly SemaphoreSlim _gate = new(Degree, Degree);
	private readonly OcrCacheStore _store;

	public OcrService(OcrCacheStore store)
	{
		_store = store;
	}

	public async Task<string> ExtractTextAsync(string filePath, CancellationToken token)
	{
		long length;
		long ticks;

		try
		{
			var info = new FileInfo(filePath);

			if (!info.Exists)
			{
				_store.Remove(filePath);
				return string.Empty;
			}

			length = info.Length;
			ticks = info.LastWriteTimeUtc.Ticks;
		}
		catch (Exception ex)
		{
			Debug.WriteLine(ex.Message);
			return string.Empty;
		}

		var cached = _store.TryGet(filePath);

		if (cached is not null &&
		    cached.Length == length &&
		    cached.LastWriteTicks == ticks)
			return cached.Text;

		var text = await RecognizeAsync(filePath, token);

		_store.Set(filePath, length, ticks, text);

		return text;
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
}