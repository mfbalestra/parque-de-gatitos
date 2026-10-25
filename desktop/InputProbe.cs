using System;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Gatitos;

internal sealed class InputProbe : Window
{
    private int clicks, keys, activations, deactivations;
    private Point lastClick;
    private readonly TextBlock status = new() { FontSize = 24, Margin = new Thickness(24) };
    private readonly TextBox editor = new() { FontSize = 28, AcceptsReturn = true, TextWrapping = TextWrapping.Wrap,
        VerticalContentAlignment = VerticalAlignment.Bottom, Padding = new Thickness(30), Background = Brushes.WhiteSmoke };
    private readonly string output;

    public InputProbe(string? output)
    {
        this.output = output!;
        Title = "Prueba de convivencia — texto de ejemplo"; WindowState = WindowState.Maximized;
        var layout = new DockPanel(); DockPanel.SetDock(status, Dock.Top); layout.Children.Add(status); layout.Children.Add(editor); Content = layout;
        PreviewMouseDown += (_, e) => { clicks++; lastClick = e.GetPosition(this); Save(); };
        PreviewKeyDown += (_, _) => { keys++; Save(); };
        Activated += (_, _) => { activations++; Save(); };
        Deactivated += (_, _) => { deactivations++; Save(); };
        editor.TextChanged += (_, _) => Save();
        Loaded += (_, _) => Save();
    }

    private void Save()
    {
        status.Text = $"Prueba local · Clics: {clicks} · Teclas: {keys} · Activaciones: {activations}\nHacé clic a través de un gato y escribí aquí.";
        File.WriteAllText(output, JsonSerializer.Serialize(new { clicks, keys, activations, deactivations,
            lastClick = new { x = lastClick.X, y = lastClick.Y }, text = editor.Text, active = IsActive }, new JsonSerializerOptions { WriteIndented = true }));
    }
}
