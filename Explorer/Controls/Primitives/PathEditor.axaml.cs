using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Explorer.Models;
using Explorer.Services;
using Ursa.Controls;

namespace Explorer.Controls.Primitives;

public partial class PathEditor : UserControl
{
    public static readonly StyledProperty<string?> TextProperty =
        AvaloniaProperty.Register<PathEditor, string?>(
            nameof(Text), defaultBindingMode: BindingMode.TwoWay);

    public static readonly StyledProperty<ICommand?> SubmitCommandProperty =
        AvaloniaProperty.Register<PathEditor, ICommand?>(nameof(SubmitCommand));

    public static readonly StyledProperty<ICommand?> RevertCommandProperty =
        AvaloniaProperty.Register<PathEditor, ICommand?>(nameof(RevertCommand));

    public static readonly StyledProperty<ICommand?> NavigateCommandProperty =
        AvaloniaProperty.Register<PathEditor, ICommand?>(nameof(NavigateCommand));

    public static readonly StyledProperty<bool> IsEditingProperty =
        AvaloniaProperty.Register<PathEditor, bool>(nameof(IsEditing));

    private bool _pressedOnSegment;

    public PathEditor()
    {
        InitializeComponent();
        RebuildSegments();

        AddHandler(PointerPressedEvent, OnTunnelPointerPressed, RoutingStrategies.Tunnel);
    }

    public ObservableCollection<PathSegment> Segments { get; } = [];

    public string? Text
    {
        get => GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public ICommand? SubmitCommand
    {
        get => GetValue(SubmitCommandProperty);
        set => SetValue(SubmitCommandProperty, value);
    }

    public ICommand? RevertCommand
    {
        get => GetValue(RevertCommandProperty);
        set => SetValue(RevertCommandProperty, value);
    }

    public ICommand? NavigateCommand
    {
        get => GetValue(NavigateCommandProperty);
        set => SetValue(NavigateCommandProperty, value);
    }

    public bool IsEditing
    {
        get => GetValue(IsEditingProperty);
        set => SetValue(IsEditingProperty, value);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == TextProperty)
            RebuildSegments();
    }

    private void RebuildSegments()
    {
        Segments.Clear();

        if (string.IsNullOrWhiteSpace(Text))
            return;

        if (string.Equals(Text, NavigationService.HomePath, StringComparison.OrdinalIgnoreCase))
        {
            Segments.Add(new PathSegment("Accueil", NavigationService.HomePath, true));
            return;
        }

        try
        {
            var chain = new List<DirectoryInfo>();
            for (DirectoryInfo? dir = new(Text); dir is not null; dir = dir.Parent)
                chain.Add(dir);
            chain.Reverse();

            for (var i = 0; i < chain.Count; i++)
            {
                var dir = chain[i];
                var name = string.IsNullOrEmpty(dir.Name) ? dir.FullName : dir.Name;
                Segments.Add(new PathSegment(name, dir.FullName, i == 0));
            }
        }
        catch
        {
        }
    }

    private void OnTunnelPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        _pressedOnSegment = e.Source is Visual source &&
                            source.GetSelfAndVisualAncestors().Any(v => v is BreadcrumbItem);
    }

    private void OnBreadcrumbPressed(object? sender, PointerPressedEventArgs e)
    {
        if (_pressedOnSegment)
            return;

        if (string.Equals(Text, NavigationService.HomePath, StringComparison.OrdinalIgnoreCase))
            return;

        IsEditing = true;
        Dispatcher.UIThread.Post(() =>
        {
            InputBox.Focus();
            InputBox.SelectAll();
        });
    }

    private void OnClearClick(object? sender, RoutedEventArgs e)
    {
        Text = string.Empty;
        InputBox.Focus();
    }

    private void OnInputKeyDown(object? sender, KeyEventArgs e)
    {
        switch (e.Key)
        {
            case Key.Enter:
                if (SubmitCommand?.CanExecute(null) == true)
                    SubmitCommand.Execute(null);
                IsEditing = false;
                e.Handled = true;
                break;

            case Key.Escape:
                if (RevertCommand?.CanExecute(null) == true)
                    RevertCommand.Execute(null);
                IsEditing = false;
                e.Handled = true;
                break;
        }
    }

    private void OnInputLostFocus(object? sender, RoutedEventArgs e)
    {
        if (RevertCommand?.CanExecute(null) == true)
            RevertCommand.Execute(null);

        IsEditing = false;
    }
}