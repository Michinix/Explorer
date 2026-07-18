using System;
using System.Diagnostics;
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
    
    public bool CanModifySelection => dataGrid.HasSelectionTargets;
    public bool CanRenameSelection => dataGrid.SelectionTargets.Count == 1;

    public void NotifySelectionChanged()
    {
        RenameCommand.NotifyCanExecuteChanged();
        CopyCommand.NotifyCanExecuteChanged();
        CutCommand.NotifyCanExecuteChanged();
        DeleteSelectedCommand.NotifyCanExecuteChanged();
    }

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

    [RelayCommand(CanExecute = nameof(CanRenameSelection))]
    private void Rename()
    {
        if (dataGrid.SelectionTargets is not [{ } entry]) return;

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
            }

            return;
        }

        if (string.IsNullOrWhiteSpace(name) || name == entry.Name) return;

        try
        {
            await FileSystemService.RenameAsync(entry, name);
            await dataGrid.LoadEntriesAsync();
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
            dataGrid.Remove(entry);
    }

    [RelayCommand(CanExecute = nameof(CanModifySelection))]
    private void Copy()
    {
        Clipboard.Copy(dataGrid.SelectionTargets);
    }

    [RelayCommand(CanExecute = nameof(CanModifySelection))]
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
        catch (Exception ex)
        {
            Debug.WriteLine(ex.Message);
        }
    }

    [RelayCommand(CanExecute = nameof(CanModifySelection))]
    private async Task DeleteSelected()
    {
        var targets = dataGrid.SelectionTargets;
        if (targets.Count == 0) return;

        try
        {
            await FileSystemService.DeleteEntriesAsync(targets);
            await dataGrid.LoadEntriesAsync();
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.Message);
        }
    }
}