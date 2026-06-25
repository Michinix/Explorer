using System;
using System.IO;
using CommunityToolkit.Mvvm.Input;
using Explorer.Services;

namespace Explorer.ViewModels;

public partial class AsideLeftViewModel(NavigationService navigation) : ViewModelBase
{
    [RelayCommand]
    private void NavigateToDesktop()
    {
        navigation.NavigateTo(Environment.GetFolderPath(Environment.SpecialFolder.Desktop));
    }

    [RelayCommand]
    private void NavigateToDownloads()
    {
        var downloadsPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads"
        );
        navigation.NavigateTo(downloadsPath);
    }

    [RelayCommand]
    private void NavigateToDocuments()
    {
        navigation.NavigateTo(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));
    }

    [RelayCommand]
    private void NavigateToPictures()
    {
        navigation.NavigateTo(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures));
    }
}