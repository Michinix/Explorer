using System;
using System.IO;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using Explorer.Models;
using Explorer.Services;

namespace Explorer.ViewModels;

public partial class FileOperationsViewModel(
    NavigationService navigation,
    ClipboardService clipboard,
    DataGridViewModel dataGrid) : ViewModelBase
{
    public ClipboardService Clipboard { get; } = clipboard;

    [RelayCommand]
    private void CreateFile()
    {
        dataGrid.AddDraft(false);
    }

    [RelayCommand]
    private void CreateFolder()
    {
        dataGrid.AddDraft(true);
    }

    [RelayCommand]
    private void Rename()
    {
        if (dataGrid.SelectedEntry is not { } entry) return;

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
                dataGrid.Remove(entry);
                return;
            }

            try
            {
                var path = Path.Combine(navigation.CurrentPath, name);

                if (entry.IsDirectory)
                    await FileSystemService.CreateDirectoryAsync(path);
                else
                    await FileSystemService.CreateFileAsync(path);

                await dataGrid.LoadEntriesAsync();
            }
            catch (Exception)
            {
                dataGrid.Remove(entry);
                // TODO Add Toast
            }

            return;
        }

        if (string.IsNullOrWhiteSpace(name) || name == entry.Name) return;

        try
        {
            await FileSystemService.RenameAsync(entry, name);
            await dataGrid.LoadEntriesAsync();
        }
        catch (Exception)
        {
            // TODO Add Toast
        }
    }

    [RelayCommand]
    private void CancelRename(FileSystemEntry entry)
    {
        entry.IsEditing = false;

        if (entry.IsNew)
            dataGrid.Remove(entry);
    }

    [RelayCommand]
    private void Copy()
    {
        Clipboard.Copy(dataGrid.SelectionTargets);
    }

    [RelayCommand]
    private void Cut()
    {
        Clipboard.Cut(dataGrid.SelectionTargets);
    }

    [RelayCommand]
    private async Task Paste()
    {
        try
        {
            await Clipboard.PasteAsync(navigation.CurrentPath);
            await dataGrid.LoadEntriesAsync();
        }
        catch (Exception)
        {
            // TODO Add Toast
        }
    }

    [RelayCommand]
    private async Task DeleteSelected()
    {
        var targets = dataGrid.SelectionTargets;
        if (targets.Count == 0) return;

        try
        {
            await FileSystemService.DeleteEntriesAsync(targets);
            await dataGrid.LoadEntriesAsync();
        }
        catch (Exception)
        {
            // TODO Add Toast
        }
    }
}