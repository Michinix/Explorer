using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Threading;
using System.Threading.Tasks;

namespace Explorer.Services;

public static class RecycleBinService
{
	private const uint FoDelete = 0x0003;

	private const ushort FofSilent = 0x0004;
	private const ushort FofNoConfirmation = 0x0010;
	private const ushort FofAllowUndo = 0x0040;
	private const ushort FofNoErrorUi = 0x0400;
	private const ushort FofWantNukeWarning = 0x4000;

	private static readonly BlockingCollection<Action> WorkQueue = new();

	static RecycleBinService()
	{
		if (!OperatingSystem.IsWindows()) return;

		var worker = new Thread(RunWorker) { IsBackground = true };
		worker.SetApartmentState(ApartmentState.STA);
		worker.Start();
	}

	public static Task DeleteAsync(IEnumerable<string> paths)
	{
		var targets = paths.Where(p => !string.IsNullOrWhiteSpace(p)).ToArray();

		if (targets.Length == 0)
			return Task.CompletedTask;

		if (!OperatingSystem.IsWindows())
			return Task.Run(() => DeletePermanently(targets));

		var tcs = new TaskCompletionSource();

		WorkQueue.Add(() =>
		{
			try
			{
				if (!OperatingSystem.IsWindows() || !SendToRecycleBin(targets))
					DeletePermanently(targets);
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message);
			}
			finally
			{
				tcs.SetResult();
			}
		});

		return tcs.Task;
	}

	private static void RunWorker()
	{
		foreach (var work in WorkQueue.GetConsumingEnumerable())
			work();
	}

	[SupportedOSPlatform("windows")]
	private static bool SendToRecycleBin(string[] paths)
	{
		var operation = new ShFileOpStruct
		{
			wFunc = FoDelete,
			pFrom = string.Join('\0', paths) + "\0\0",
			fFlags = FofAllowUndo | FofNoConfirmation | FofNoErrorUi | FofSilent | FofWantNukeWarning
		};

		var result = SHFileOperation(ref operation);

		if (result != 0)
			Debug.WriteLine($"SHFileOperation failed with code {result}.");

		return result == 0 && !operation.fAnyOperationsAborted;
	}

	private static void DeletePermanently(string[] paths)
	{
		foreach (var path in paths)
			try
			{
				if (Directory.Exists(path))
					Directory.Delete(path, true);
				else
					File.Delete(path);
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message);
			}
	}

	[DllImport("shell32.dll", CharSet = CharSet.Unicode)]
	private static extern int SHFileOperation(ref ShFileOpStruct fileOp);

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	private struct ShFileOpStruct
	{
		public IntPtr hwnd;
		public uint wFunc;
		public string pFrom;
		public string pTo;
		public ushort fFlags;
		public bool fAnyOperationsAborted;
		public IntPtr hNameMappings;
		public string lpszProgressTitle;
	}
}
