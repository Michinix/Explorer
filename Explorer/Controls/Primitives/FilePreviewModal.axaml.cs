using System;
using System.IO;
using System.Threading.Tasks;
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
    private const int DecodeWidth = 1200;
    private static readonly string[] ImageExtensions = ["JPG", "JPEG", "PNG", "GIF", "BMP", "WEBP"];

    public static readonly StyledProperty<bool> IsOpenProperty =
        AvaloniaProperty.Register<FilePreviewModal, bool>(
            nameof(IsOpen), defaultBindingMode: BindingMode.TwoWay);

    public static readonly StyledProperty<FileSystemEntry?> EntryProperty =
        AvaloniaProperty.Register<FilePreviewModal, FileSystemEntry?>(nameof(Entry));

    private readonly Image _previewImage;

    private int _previewToken;
    private IInputElement? _previouslyFocused;

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

        if (change.Property == IsOpenProperty)
        {
            if (IsOpen)
            {
                _previouslyFocused = TopLevel.GetTopLevel(this)?.FocusManager.GetFocusedElement();
            }
            else
            {
                _previouslyFocused?.Focus();
                _previouslyFocused = null;
            }
        }

        if (change.Property == EntryProperty || change.Property == IsOpenProperty)
            UpdatePreview();
    }

    private async void UpdatePreview()
    {
        var token = ++_previewToken;

        _previewImage.Source = null;
        _previewImage.IsVisible = false;

        if (!IsOpen ||
            Entry is not { IsDirectory: false } entry ||
            !ImageExtensions.Contains(entry.Type, StringComparer.OrdinalIgnoreCase))
            return;

        var path = entry.FullPath;

        var bitmap = await Task.Run(() =>
        {
            try
            {
                using var stream = File.OpenRead(path);
                return Bitmap.DecodeToWidth(stream, DecodeWidth);
            }
            catch (Exception)
            {
                return null;
            }
        });

        if (token != _previewToken)
        {
            bitmap?.Dispose();
            return;
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