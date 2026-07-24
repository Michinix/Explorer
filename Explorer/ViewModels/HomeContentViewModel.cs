using System;
using System.Collections.Generic;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Explorer.Models;
using Explorer.Services;

namespace Explorer.ViewModels;

public partial class HomeContentViewModel : ViewModelBase
{
    private readonly NavigationService _navigation;

    [ObservableProperty] private bool _isActive;
    [ObservableProperty] private IReadOnlyList<QuickAccessItem> _quickAccess = [];

    public HomeContentViewModel(NavigationService navigation)
    {
        _navigation = navigation;

        QuickAccess = BuildQuickAccess();
        IsActive = string.Equals(navigation.CurrentPath, NavigationService.HomePath, StringComparison.OrdinalIgnoreCase);

        WeakReferenceMessenger.Default.Register<CurrentPathChangedMessage>(this, (_, message) =>
            IsActive = string.Equals(message.NewPath, NavigationService.HomePath, StringComparison.OrdinalIgnoreCase));
    }

    private static IReadOnlyList<QuickAccessItem> BuildQuickAccess()
    {
        var userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

        return
        [
            new QuickAccessItem("Bureau", "/Assets/Icons/Desktop.svg", "path { stroke: white }",
                Environment.GetFolderPath(Environment.SpecialFolder.Desktop)),
            new QuickAccessItem("Téléchargements", "/Assets/Icons/Download.svg", "path { fill: white }",
                Path.Combine(userProfile, "Downloads")),
            new QuickAccessItem("Documents", "/Assets/Icons/Document.svg", "path { stroke: white }",
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)),
            new QuickAccessItem("Images", "/Assets/Icons/Picture.svg", "path { stroke: white }",
                Environment.GetFolderPath(Environment.SpecialFolder.MyPictures))
        ];
    }

    [RelayCommand]
    private void OpenQuickAccess(string path)
    {
        _navigation.NavigateTo(path);
    }
}
