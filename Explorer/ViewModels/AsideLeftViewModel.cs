using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Explorer.Models;
using Explorer.Services;

namespace Explorer.ViewModels;

public partial class AsideLeftViewModel : ViewModelBase
{
    private readonly NavigationService _navigation;

    private readonly string _homePath = NavigationService.HomePath;
    private readonly string _desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
    private readonly string _documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
    private readonly string _picturesPath = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);

    private readonly string _downloadsPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");

    [ObservableProperty] private ICollection<DriveItem> _drives = [];

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsHomeActive), nameof(IsDesktopActive), nameof(IsDownloadsActive),
        nameof(IsDocumentsActive), nameof(IsPicturesActive))]
    private string _currentPath;

    public AsideLeftViewModel(NavigationService navigation)
    {
        _navigation = navigation;
        _currentPath = navigation.CurrentPath;

        WeakReferenceMessenger.Default.Register<CurrentPathChangedMessage>(this, (_, message) =>
            CurrentPath = message.NewPath);

        _ = LoadDrivesAsync();
    }

    public bool IsHomeActive => string.Equals(CurrentPath, _homePath, StringComparison.OrdinalIgnoreCase);
    public bool IsDesktopActive => IsUnder(CurrentPath, _desktopPath);
    public bool IsDownloadsActive => IsUnder(CurrentPath, _downloadsPath);
    public bool IsDocumentsActive => IsUnder(CurrentPath, _documentsPath);
    public bool IsPicturesActive => IsUnder(CurrentPath, _picturesPath);

    private static bool IsUnder(string current, string basePath)
    {
        if (string.IsNullOrEmpty(current) || string.IsNullOrEmpty(basePath))
            return false;

        var separators = new[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar };
        var normalizedCurrent = current.TrimEnd(separators);
        var normalizedBase = basePath.TrimEnd(separators);

        if (string.Equals(normalizedCurrent, normalizedBase, StringComparison.OrdinalIgnoreCase))
            return true;

        return normalizedCurrent.StartsWith(normalizedBase + Path.DirectorySeparatorChar,
            StringComparison.OrdinalIgnoreCase);
    }

    partial void OnCurrentPathChanged(string value)
    {
        UpdateDrivesActive();
    }

    private void UpdateDrivesActive()
    {
        var quickAccessActive = IsHomeActive || IsDesktopActive || IsDownloadsActive || IsDocumentsActive ||
                                 IsPicturesActive;

        foreach (var drive in Drives)
            drive.IsActive = !quickAccessActive && IsUnder(CurrentPath, drive.FullPath);
    }

    private async Task LoadDrivesAsync()
    {
        Drives = await FileSystemService.GetDrivesAsync();
        UpdateDrivesActive();
    }

    [RelayCommand]
    private void NavigateToDrive(string path)
    {
        _navigation.NavigateTo(path);
    }

    [RelayCommand]
    private void NavigateToHome()
    {
        _navigation.NavigateTo(_homePath);
    }

    [RelayCommand]
    private void NavigateToDesktop()
    {
        _navigation.NavigateTo(_desktopPath);
    }

    [RelayCommand]
    private void NavigateToDocuments()
    {
        _navigation.NavigateTo(_documentsPath);
    }

    [RelayCommand]
    private void NavigateToPictures()
    {
        _navigation.NavigateTo(_picturesPath);
    }

    [RelayCommand]
    private void NavigateToDownloads()
    {
        _navigation.NavigateTo(_downloadsPath);
    }
}
