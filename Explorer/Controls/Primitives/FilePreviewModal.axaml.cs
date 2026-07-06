using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using Explorer.Models;

namespace Explorer.Controls.Primitives;

public partial class FilePreviewModal : UserControl
{
    private static readonly string[] ImageExtensions = ["JPG", "JPEG", "PNG", "GIF", "BMP", "WEBP"];

    public static readonly StyledProperty<bool> IsOpenProperty =
        AvaloniaProperty.Register<FilePreviewModal, bool>(
            nameof(IsOpen), defaultBindingMode: BindingMode.TwoWay);

    public static readonly StyledProperty<FileSystemEntry?> EntryProperty =
        AvaloniaProperty.Register<FilePreviewModal, FileSystemEntry?>(nameof(Entry));

    private readonly Image _previewImage;

    public FilePreviewModal()
    {
        InitializeComponent();
        _previewImage = this.FindControl<Image>("PreviewImage")!;
    }

    public bool IsOpen
    {
        get => GetValue(IsOpenProperty);
        set => SetValue(IsOpenProperty, value);
    }

    public FileSystemEntry? Entry
    {
        get => GetValue(EntryProperty);
        set => SetValue(EntryProperty, value);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == IsOpenProperty && IsOpen)
            Focus();

        if (change.Property == EntryProperty || change.Property == IsOpenProperty)
            UpdatePreview();
    }

    private void UpdatePreview()
    {
        Bitmap? bitmap = null;

        if (Entry is { IsDirectory: false } entry &&
            ImageExtensions.Contains(entry.Type, StringComparer.OrdinalIgnoreCase))
        {
            try
            {
                bitmap = new Bitmap(entry.FullPath);
            }
            catch (Exception)
            {
                bitmap = null;
            }
        }

        _previewImage.Source = bitmap;
        _previewImage.IsVisible = bitmap is not null;
    }

    private void OnOverlayPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        IsOpen = false;
    }

    private void OnCardPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        e.Handled = true;
    }

    private void OnCloseClicked(object? sender, RoutedEventArgs e)
    {
        IsOpen = false;
    }

    private void OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key != Key.Escape && e.Key != Key.Space) return;

        IsOpen = false;
        e.Handled = true;
    }
}