using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Explorer.Models;
using Explorer.Services;

namespace Explorer.ViewModels;

public partial class AsideLeftViewModel : ViewModelBase
{
    private readonly FileSystemService _fileSystemService;
    private readonly NavigationService _navigation;

    [ObservableProperty] private ICollection<DriveItem> _drives = [];

    public AsideLeftViewModel(NavigationService navigation, FileSystemService fileSystemService)
    {
        _navigation = navigation;
        _fileSystemService = fileSystemService;

        _ = LoadDrivesAsync();
    }

    private async Task LoadDrivesAsync()
    {
        Drives = await _fileSystemService.GetDrivesAsync();
    }

    [RelayCommand]
    private void NavigateToDrive(string path)
    {
        _navigation.NavigateTo(path);
    }

    [RelayCommand]
    private void NavigateToDesktop()
    {
        _navigation.NavigateTo(Environment.GetFolderPath(Environment.SpecialFolder.Desktop));
    }

    [RelayCommand]
    private void NavigateToDocuments()
    {
        _navigation.NavigateTo(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));
    }

    [RelayCommand]
    private void NavigateToPictures()
    {
        _navigation.NavigateTo(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures));
    }

    [RelayCommand]
    private void NavigateToDownloads()
    {
        _navigation.NavigateTo(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            "Downloads"));
    }
}