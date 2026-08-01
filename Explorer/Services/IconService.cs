using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Threading;
using System.Threading.Tasks;
using Bitmap = Avalonia.Media.Imaging.Bitmap;

namespace Explorer.Services;

public static class IconService
{
	private const int ThumbnailWidth = 400;

	private const int ShilJumbo = 0x4;
	private const int IldTransparent = 0x1;
	private const uint FileAttributeNormal = 0x80;
	private const uint ShgfiIcon = 0x100;
	private const uint ShgfiSysIconIndex = 0x4000;
	private const uint ShgfiUseFileAttributes = 0x10;

	private static readonly string[] ImageExtensions =
		[".png", ".jpg", ".jpeg", ".bmp", ".gif", ".webp", ".ico", ".tif", ".tiff"];

	private static readonly ConcurrentDictionary<string, Task<Bitmap?>> Cache = new();
	private static readonly ConcurrentDictionary<string, Task<Bitmap?>> ThumbnailCache = new();
	private static readonly BlockingCollection<Action> WorkQueue = new();

	static IconService()
	{
		if (!OperatingSystem.IsWindows()) return;

		var worker = new Thread(RunWorker) { IsBackground = true };
		worker.SetApartmentState(ApartmentState.STA);
		worker.Start();
	}

	private static void RunWorker()
	{
		foreach (var work in WorkQueue.GetConsumingEnumerable())
			work();
	}

	public static Task<Bitmap?> GetIconAsync(string path)
	{
		if (Array.IndexOf(ImageExtensions, Path.GetExtension(path).ToLowerInvariant()) >= 0)
			return GetThumbnailAsync(path);

		if (!OperatingSystem.IsWindows())
			return Task.FromResult<Bitmap?>(null);

		var key = Path.GetExtension(path).ToLowerInvariant();

		return Cache.GetOrAdd(key, _ =>
		{
			var tcs = new TaskCompletionSource<Bitmap?>();

			WorkQueue.Add(() =>
			{
				try
				{
					tcs.SetResult(OperatingSystem.IsWindows() ? ExtractIcon(path) : null);
				}
				catch (Exception ex)
				{
					Debug.WriteLine(ex.Message);
					tcs.SetResult(null);
				}
			});

			return tcs.Task;
		});
	}

	private static Task<Bitmap?> GetThumbnailAsync(string path)
	{
		return ThumbnailCache.GetOrAdd(path, p => Task.Run(() =>
		{
			try
			{
				using var stream = File.OpenRead(p);
				return (Bitmap?)Bitmap.DecodeToWidth(stream, ThumbnailWidth);
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message);
				return null;
			}
		}));
	}

	[SupportedOSPlatform("windows")]
	private static Bitmap? ExtractIcon(string path)
	{
		var probe = "file" + Path.GetExtension(path);

		var hIcon = TryGetJumboIcon(probe) ?? TryGetLargeIcon(probe);
		if (hIcon is null || hIcon.Value == IntPtr.Zero)
			return null;

		try
		{
			using var icon = Icon.FromHandle(hIcon.Value);
			using var drawingBitmap = icon.ToBitmap();
			using var stream = new MemoryStream();
			drawingBitmap.Save(stream, ImageFormat.Png);
			stream.Position = 0;
			return new Bitmap(stream);
		}
		catch (Exception ex)
		{
			Debug.WriteLine(ex.Message);
			return null;
		}
		finally
		{
			DestroyIcon(hIcon.Value);
		}
	}

	[SupportedOSPlatform("windows")]
	private static IntPtr? TryGetJumboIcon(string probe)
	{
		try
		{
			var info = new ShFileInfo();
			var result = SHGetFileInfo(probe, FileAttributeNormal, ref info, (uint)Marshal.SizeOf<ShFileInfo>(),
				ShgfiSysIconIndex | ShgfiUseFileAttributes);

			if (result == IntPtr.Zero)
				return null;

			var iid = new Guid("46EB5926-582E-4017-9FDF-E8998DAA0950");
			var hr = SHGetImageList(ShilJumbo, ref iid, out var imageList);
			if (hr != 0 || imageList is null)
				return null;

			try
			{
				var hr2 = imageList.GetIcon(info.iIcon, IldTransparent, out var hIcon);
				return hr2 == 0 && hIcon != IntPtr.Zero ? hIcon : null;
			}
			finally
			{
				Marshal.ReleaseComObject(imageList);
			}
		}
		catch (Exception ex)
		{
			Debug.WriteLine(ex.Message);
			return null;
		}
	}

	[SupportedOSPlatform("windows")]
	private static IntPtr? TryGetLargeIcon(string probe)
	{
		var info = new ShFileInfo();
		var result = SHGetFileInfo(probe, FileAttributeNormal, ref info, (uint)Marshal.SizeOf<ShFileInfo>(),
			ShgfiIcon | ShgfiUseFileAttributes);

		return result != IntPtr.Zero && info.hIcon != IntPtr.Zero ? info.hIcon : null;
	}

	[DllImport("shell32.dll", CharSet = CharSet.Unicode)]
	private static extern IntPtr SHGetFileInfo(string pszPath, uint dwFileAttributes,
		ref ShFileInfo psfi, uint cbFileInfo, uint uFlags);

	[DllImport("shell32.dll", EntryPoint = "#727")]
	private static extern int SHGetImageList(int iImageList, ref Guid riid,
		[MarshalAs(UnmanagedType.Interface)] out IImageList? ppv);

	[DllImport("user32.dll")]
	private static extern bool DestroyIcon(IntPtr hIcon);

	[StructLayout(LayoutKind.Sequential)]
	private struct ShFileInfo
	{
		public IntPtr hIcon;
		public int iIcon;
		public uint dwAttributes;

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
		public string szDisplayName;

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
		public string szTypeName;
	}

	[ComImport]
	[Guid("46EB5926-582E-4017-9FDF-E8998DAA0950")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	private interface IImageList
	{
		int Add(IntPtr hbmImage, IntPtr hbmMask, ref int pi);
		int ReplaceIcon(int i, IntPtr hicon, ref int pi);
		int SetOverlayImage(int iImage, int iOverlay);
		int Replace(int i, IntPtr hbmImage, IntPtr hbmMask);
		int AddMasked(IntPtr hbmImage, int crMask, ref int pi);
		int Draw(IntPtr pimldp);
		int Remove(int i);
		int GetIcon(int i, int flags, out IntPtr picon);
	}
}