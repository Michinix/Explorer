using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Explorer.Services;

public static class ExternalToolsService
{
	private static readonly Lazy<bool> VsCodeDetected = new(DetectVsCode);

	public static bool IsVsCodeAvailable => VsCodeDetected.Value;

	public static Task OpenTerminalAsync(string path)
	{
		return Task.Run(() =>
		{
			try
			{
				if (OperatingSystem.IsWindows())
				{
					if (TryStart("wt.exe", $"-d \"{path}\"", null)) return;
					TryStart("cmd.exe", null, path);
				}
				else if (OperatingSystem.IsMacOS())
				{
					TryStart("open", $"-a Terminal \"{path}\"", null);
				}
				else
				{
					TryStart("x-terminal-emulator", null, path);
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message);
			}
		});
	}

	public static Task OpenInVsCodeAsync(string path)
	{
		return Task.Run(() =>
		{
			try
			{
				if (OperatingSystem.IsWindows())
				{
					var info = new ProcessStartInfo("cmd.exe")
					{
						Arguments = $"/c code \"{path}\"",
						UseShellExecute = false,
						CreateNoWindow = true,
						WindowStyle = ProcessWindowStyle.Hidden
					};
					Process.Start(info);
				}
				else
				{
					var info = new ProcessStartInfo("code")
					{
						Arguments = $"\"{path}\"",
						UseShellExecute = false,
						CreateNoWindow = true
					};
					Process.Start(info);
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message);
			}
		});
	}

	private static bool TryStart(string fileName, string? arguments, string? workingDirectory)
	{
		try
		{
			var info = new ProcessStartInfo(fileName)
			{
				Arguments = arguments ?? string.Empty,
				UseShellExecute = true
			};

			if (!string.IsNullOrEmpty(workingDirectory))
				info.WorkingDirectory = workingDirectory;

			Process.Start(info);
			return true;
		}
		catch (Exception ex)
		{
			Debug.WriteLine(ex.Message);
			return false;
		}
	}

	private static bool DetectVsCode()
	{
		var pathEnv = Environment.GetEnvironmentVariable("PATH") ?? string.Empty;
		var exeName = OperatingSystem.IsWindows() ? "code.cmd" : "code";
		var separator = OperatingSystem.IsWindows() ? ';' : ':';

		return pathEnv.Split(separator, StringSplitOptions.RemoveEmptyEntries)
			.Any(dir =>
			{
				try
				{
					return File.Exists(Path.Combine(dir, exeName));
				}
				catch
				{
					return false;
				}
			});
	}
}