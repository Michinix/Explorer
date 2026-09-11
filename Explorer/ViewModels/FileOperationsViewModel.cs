using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Explorer.Models;
using Explorer.Services;

namespace Explorer.ViewModels;

public partial class FileOperationsViewModel(
	NavigationService navigation,
	ClipboardService clipboard,
	FileBrowserViewModel fileBrowser,
	SettingsService settings) : ViewModelBase
{
	public ClipboardService Clipboard { get; } = clipboard;

	public bool CanModifySelection => fileBrowser.HasSelectionTargets;
	public bool CanRenameSelection => fileBrowser.SelectionTargets.Count == 1;

	public void NotifySelectionChanged()
	{
		RenameCommand.NotifyCanExecuteChanged();
		CopyCommand.NotifyCanExecuteChanged();
		CutCommand.NotifyCanExecuteChanged();
		MoveToCommand.NotifyCanExecuteChanged();
		DeleteSelectedCommand.NotifyCanExecuteChanged();
	}

	[RelayCommand]
	private void CreateFile()
	{
		fileBrowser.AddDraft(false);
	}

	[RelayCommand]
	private void CreateFolder()
	{
		fileBrowser.AddDraft(true);
	}

	[RelayCommand(CanExecute = nameof(CanRenameSelection))]
	private void Rename()
	{
		if (fileBrowser.SelectionTargets is not [{ } entry]) return;

		entry.EditableName = entry.Name;
		entry.IsEditing = true;
	}

	[RelayCommand]
	private async Task CommitRename(FileSystemEntry entry)
	{
		entry.IsEditing = false;
		var name = entry.EditableName.Trim();

		if (entry.IsNew)
		{
			if (string.IsNullOrWhiteSpace(name))
			{
				fileBrowser.Remove(entry);
				return;
			}

			try
			{
				var path = Path.Combine(navigation.CurrentPath, name);

				if (entry.IsDirectory)
					await FileSystemService.CreateDirectoryAsync(path);
				else
					await FileSystemService.CreateFileAsync(path);

				await fileBrowser.LoadEntriesAsync();
			}
			catch (Exception)
			{
				fileBrowser.Remove(entry);
			}

			return;
		}

		if (string.IsNullOrWhiteSpace(name)) return;

		if (!entry.IsDirectory && !name.Contains('.'))
			name += Path.GetExtension(entry.Name);

		if (name == entry.Name) return;

		try
		{
			await FileSystemService.RenameAsync(entry, name);
			await fileBrowser.LoadEntriesAsync();
		}
		catch (Exception ex)
		{
			Debug.WriteLine(ex.Message);
		}
	}

	[RelayCommand]
	private void CancelRename(FileSystemEntry entry)
	{
		entry.IsEditing = false;

		if (entry.IsNew)
			fileBrowser.Remove(entry);
	}

	[RelayCommand(CanExecute = nameof(CanModifySelection))]
	private void Copy()
	{
		Clipboard.Copy(fileBrowser.SelectionTargets);
	}

	[RelayCommand(CanExecute = nameof(CanModifySelection))]
	private void Cut()
	{
		Clipboard.Cut(fileBrowser.SelectionTargets);
	}

	[RelayCommand]
	private async Task Paste()
	{
		try
		{
			await Clipboard.PasteAsync(navigation.CurrentPath);
			await fileBrowser.LoadEntriesAsync();
		}
		catch (Exception ex)
		{
			Debug.WriteLine(ex.Message);
		}
	}

	[RelayCommand(CanExecute = nameof(CanModifySelection))]
	private void MoveTo()
	{
		var targets = fileBrowser.SelectionTargets;
		if (targets.Count == 0) return;

		WeakReferenceMessenger.Default.Send(new MoveRequestedMessage(targets, navigation.CurrentPath));
	}

	public async Task MoveEntriesAsync(IReadOnlyList<FileSystemEntry> entries, string destination)
	{
		try
		{
			await FileSystemService.MoveEntriesAsync(entries, destination);
			await fileBrowser.LoadEntriesAsync();
		}
		catch (Exception ex)
		{
			Debug.WriteLine(ex.Message);
		}
	}

	[RelayCommand]
	private void PinFolder(FileSystemEntry entry)
	{
		if (!entry.IsDirectory) return;

		settings.TogglePinned(entry.Name, entry.FullPath);
	}

	[RelayCommand(CanExecute = nameof(CanModifySelection))]
	private async Task DeleteSelected()
	{
		var targets = fileBrowser.SelectionTargets;
		if (targets.Count == 0) return;

		try
		{
			await FileSystemService.DeleteEntriesAsync(targets);
			await fileBrowser.LoadEntriesAsync();
		}
		catch (Exception ex)
		{
			Debug.WriteLine(ex.Message);
		}
	}
}