using System.IO;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Explorer.ViewModels;

namespace Explorer.Views;

public partial class HomeView : UserControl
{
    public HomeView()
    {
        InitializeComponent();
        this.AttachedToVisualTree += OnAttachedToVisualTree;
    }

    private void OnAttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
    {
        if (DataContext is HomeViewModel viewModel)
        {
            viewModel.Initialize();
        }

        // Add double-click handler for directories
        var directoriesListBox = this.FindControl<ListBox>("DirectoriesListBox");
        if (directoriesListBox != null)
        {
            directoriesListBox.DoubleTapped += DirectoriesListBox_DoubleTapped;
        }
    }

    private void DirectoriesListBox_DoubleTapped(object? sender, TappedEventArgs e)
    {
        if (sender is ListBox listBox && listBox.SelectedItem is DirectoryInfo directory)
        {
            if (DataContext is HomeViewModel viewModel)
            {
                viewModel.NavigateToDirectoryCommand.Execute(directory);
            }
        }
    }
}