using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using Forms = System.Windows.Forms;

namespace Gatitos;

internal sealed class OverlayWindow : Window
{
    private readonly SpriteCatalog catalog = new();
    private readonly CatSurface surface;
    private readonly Stopwatch clock = new();
    private readonly Forms.NotifyIcon tray;
    private readonly Forms.ToolStripMenuItem pauseItem = new("Pausar");
    private readonly Forms.ToolStripMenuItem hideItem = new("Ocultar");
    private readonly List<Forms.ToolStripMenuItem> countItems = new();
    private readonly Measurement? measurement;
    private double lastTime;
    private double nextFrameAt;
    private bool paused;
    private bool hidden;
    private Window? panel;
    private TextBlock? panelStatus;
    private Button? panelPause, panelHide;
    public int CatCount { get; private set; }
    public long Frames => surface.Frames;
    public long Steps { get; private set; }
    public int Activations { get; private set; }
    public nint Handle { get; private set; }
    public bool Hidden => hidden;

    public OverlayWindow(Options options)
    {
        Title = "Gatitos — compañía de escritorio";
        WindowStyle = WindowStyle.None; ResizeMode = ResizeMode.NoResize;
        AllowsTransparency = true; Background = Brushes.Transparent;
        ShowInTaskbar = false; ShowActivated = false; Topmost = true;
        Focusable = false; IsHitTestVisible = false;
        surface = new CatSurface(catalog); Content = surface;
        Place(); SetCount(options.Cats); paused = options.Paused;
        tray = new Forms.NotifyIcon { Icon = System.Drawing.SystemIcons.Information, Text = "Gatitos — clic derecho para opciones" };
        var menu = new Forms.ContextMenuStrip();
        foreach (int count in new[] { 1, 4, 8 })
        {
            var item = new Forms.ToolStripMenuItem(count == 1 ? "1 gato" : $"{count} gatos") { Tag = count, Checked = count == CatCount };
            item.Click += (_, _) => SetCount(count); countItems.Add(item); menu.Items.Add(item);
        }
        menu.Items.Add(new Forms.ToolStripSeparator()); menu.Items.Add(pauseItem); menu.Items.Add(hideItem);
        menu.Items.Add("Controles", null, (_, _) => OpenPanel());
        menu.Items.Add("Salir", null, (_, _) => Close()); tray.ContextMenuStrip = menu;
        pauseItem.Click += (_, _) => { paused = !paused; UpdateAnimation(); };
        hideItem.Click += (_, _) => SetHidden(!hidden);
        tray.DoubleClick += (_, _) => SetHidden(!hidden);
        Activated += (_, _) => Activations++;
        SourceInitialized += (_, _) =>
        {
            Handle = new WindowInteropHelper(this).Handle;
            Native.MakePassive(Handle);
            HwndSource.FromHwnd(Handle)!.AddHook(FilterMessages);
        };
        SystemParameters.StaticPropertyChanged += OnDisplayChanged;
        if (options.Output != null) measurement = new Measurement(this, options.Output);
        Loaded += (_, _) => { tray.Visible = true; UpdateAnimation(); measurement?.Start(); if (options.Panel) OpenPanel(); };
        Closed += (_, _) =>
        {
            CompositionTarget.Rendering -= OnRendering; measurement?.Dispose();
            SystemParameters.StaticPropertyChanged -= OnDisplayChanged;
            tray.Visible = false; tray.Dispose();
        };
    }

    private void Place()
    {
        var work = SystemParameters.WorkArea;
        Left = work.Left; Width = work.Width; Height = 165; Top = work.Bottom - Height;
    }
    private void OnDisplayChanged(object? sender, PropertyChangedEventArgs args) { if (args.PropertyName == nameof(SystemParameters.WorkArea)) Place(); }

    private static nint FilterMessages(nint hwnd, int msg, nint wParam, nint lParam, ref bool handled)
    {
        if (msg == 0x21) { handled = true; return 3; } // MA_NOACTIVATE
        if (msg == 0x84) { handled = true; return -1; } // HTTRANSPARENT
        return 0;
    }

    private void SetCount(int count)
    {
        if (!Options.ValidCount(count)) throw new ArgumentOutOfRangeException(nameof(count));
        catalog.Load(count); CatCount = count;
        surface.Cats = Enumerable.Range(0, count).Select(i => new CatMotion(i, count, Width)).ToArray();
        foreach (var item in countItems) item.Checked = (int)item.Tag! == count;
        surface.InvalidateVisual();
        UpdatePanel();
    }

    public void SetHidden(bool value)
    {
        hidden = value;
        if (hidden) Hide(); else Show();
        UpdateAnimation();
    }

    private void UpdateAnimation()
    {
        pauseItem.Text = paused ? "Reanudar" : "Pausar"; hideItem.Text = hidden ? "Mostrar" : "Ocultar";
        CompositionTarget.Rendering -= OnRendering; clock.Restart(); lastTime = 0; nextFrameAt = 0;
        if (!paused && !hidden) CompositionTarget.Rendering += OnRendering;
        UpdatePanel();
    }

    private void OpenPanel()
    {
        if (panel != null) { panel.Activate(); return; }
        panel = new Window { Title = "Controles de gatitos", Width = 360, Height = 410, ResizeMode = ResizeMode.NoResize,
            WindowStartupLocation = WindowStartupLocation.CenterScreen };
        var layout = new StackPanel { Margin = new Thickness(22) };
        panelStatus = new TextBlock { FontSize = 18, Margin = new Thickness(0, 0, 0, 12) }; layout.Children.Add(panelStatus);
        foreach (int count in new[] { 1, 4, 8 })
        { var choice = new Button { Content = count == 1 ? "1 gato" : $"{count} gatos", Height = 34, Margin = new Thickness(0, 3, 0, 3) }; choice.Click += (_, _) => SetCount(count); layout.Children.Add(choice); }
        panelPause = new Button { Height = 34, Margin = new Thickness(0, 8, 0, 3) };
        panelPause.Click += (_, _) => { paused = !paused; UpdateAnimation(); }; layout.Children.Add(panelPause);
        panelHide = new Button { Height = 34, Margin = new Thickness(0, 3, 0, 3) };
        panelHide.Click += (_, _) => SetHidden(!hidden); layout.Children.Add(panelHide);
        var quit = new Button { Content = "Salir de los gatitos", Height = 34, Margin = new Thickness(0, 3, 0, 8) }; quit.Click += (_, _) => Close(); layout.Children.Add(quit);
        layout.Children.Add(new TextBlock { Text = "Cerrar este panel deja los gatos activos.\nTambién tenés estas opciones en la bandeja.", TextWrapping = TextWrapping.Wrap });
        panel.Content = layout; panel.Closed += (_, _) => { panel = null; panelStatus = null; };
        UpdatePanel(); panel.Show();
    }

    private void UpdatePanel()
    {
        if (panelStatus == null) return;
        panelStatus.Text = $"{CatCount} gato{(CatCount == 1 ? "" : "s")} · {(hidden ? "ocultos" : paused ? "en pausa" : "en movimiento")}";
        panelPause!.Content = paused ? "Reanudar" : "Pausar";
        panelHide!.Content = hidden ? "Mostrar" : "Ocultar";
    }

    private void OnRendering(object? sender, EventArgs args)
    {
        double now = clock.Elapsed.TotalSeconds;
        if (now < nextFrameAt) return;
        nextFrameAt = (Math.Floor(now * 30) + 1) / 30;
        double dt = Math.Min(.1, now - lastTime); lastTime = now;
        foreach (var cat in surface.Cats) cat.Advance(dt, Width);
        Steps++; surface.InvalidateVisual();
    }
}

internal sealed class CatSurface : FrameworkElement
{
    private readonly SpriteCatalog catalog;
    private readonly RadialGradientBrush shadow = new(Color.FromArgb(45, 24, 30, 34), Colors.Transparent);
    public CatMotion[] Cats { get; set; } = Array.Empty<CatMotion>();
    public long Frames { get; private set; }
    public CatSurface(SpriteCatalog catalog)
    {
        this.catalog = catalog; shadow.Freeze();
        RenderOptions.SetBitmapScalingMode(this, BitmapScalingMode.LowQuality);
    }
    protected override void OnRender(DrawingContext dc)
    {
        Frames++;
        foreach (var cat in Cats)
        {
            var sprite = catalog.Get(cat.Coat, cat.Pose);
            double feet = ActualHeight - 7;
            dc.DrawEllipse(shadow, null, new Point(cat.X, feet), 32 - cat.Height / 8, 5);
            dc.PushTransform(new TranslateTransform(cat.X, feet - cat.Height));
            dc.PushTransform(new ScaleTransform(cat.Direction == sprite.Facing ? 1 : -1, cat.Squash));
            dc.PushTransform(new TranslateTransform(-sprite.Width * sprite.AnchorX, -sprite.Height * sprite.AnchorY));
            dc.PushTransform(new ScaleTransform(sprite.Width / sprite.PixelWidth, sprite.Height / sprite.PixelHeight));
            dc.DrawDrawing(sprite.Drawing);
            dc.Pop(); dc.Pop(); dc.Pop(); dc.Pop();
        }
    }
}

internal static class Native
{
    public const long PassiveStyles = 0x00080000L | 0x00000020L | 0x08000000L | 0x00000080L;
    [DllImport("user32.dll", EntryPoint = "GetWindowLongPtrW", SetLastError = true)]
    public static extern nint GetWindowLongPtr(nint window, int index);
    [DllImport("user32.dll", EntryPoint = "SetWindowLongPtrW", SetLastError = true)]
    private static extern nint SetWindowLongPtr(nint window, int index, nint value);
    [DllImport("user32.dll")] public static extern nint GetForegroundWindow();
    public static void MakePassive(nint window)
    {
        long styles = GetWindowLongPtr(window, -20).ToInt64();
        Marshal.SetLastPInvokeError(0);
        SetWindowLongPtr(window, -20, (nint)((styles | PassiveStyles) & ~0x00040000L));
        if (Marshal.GetLastWin32Error() != 0) throw new Win32Exception(Marshal.GetLastWin32Error());
        if ((GetWindowLongPtr(window, -20).ToInt64() & PassiveStyles) != PassiveStyles)
            throw new InvalidOperationException("Windows no aplicó el modo pasivo.");
    }
}
