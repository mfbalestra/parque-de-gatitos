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
    private readonly Measurement? measurement;
    private double lastTime;
    private readonly FramePacer pacer;
    private bool paused;
    private bool hidden;
    private SettingsPanel? panel;
    private readonly ParkCycle cycle;
    private readonly ParkScene parkScene = new();
    private bool expanded;
    private int statusSecond = -1;
    public string Scene => cycle.Phase.ToString();
    public int Visits => cycle.Visits;
    public int CatCount { get; private set; }
    public int TargetFps => pacer.Target;
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
        var saved = ParkSettings.Load();
        var settings = saved with { Cats = options.Cats ?? saved.Cats, Fps = options.Fps ?? saved.Fps };
        cycle = new ParkCycle(settings); pacer = new FramePacer(settings.Fps);
        surface = new CatSurface(catalog, cycle, parkScene); Content = surface;
        Place(); SetCount(settings.Cats); paused = options.Paused;
        tray = new Forms.NotifyIcon { Icon = System.Drawing.SystemIcons.Information, Text = "Gatitos — clic derecho para opciones" };
        var menu = new Forms.ContextMenuStrip();
        menu.Items.Add("Controles", null, (_, _) => OpenPanel());
        menu.Items.Add(new Forms.ToolStripSeparator()); menu.Items.Add(pauseItem); menu.Items.Add(hideItem);
        menu.Items.Add("Ir al parque ahora", null, (_, _) => VisitPark());
        menu.Items.Add("Volver al escritorio", null, (_, _) => ReturnToDesktop());
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
        if (options.Output != null) measurement = new Measurement(this, options.Output, options.ParkNow);
        Loaded += (_, _) => { tray.Visible = true; UpdateAnimation(); if (options.ParkNow) VisitPark(); measurement?.Start(); if (options.Panel) OpenPanel(); };
        Closed += (_, _) =>
        {
            CompositionTarget.Rendering -= OnRendering; measurement?.Dispose();
            SystemParameters.StaticPropertyChanged -= OnDisplayChanged;
            tray.Visible = false; tray.Dispose(); panel?.Close();
        };
    }

    private void Place()
    {
        var work = SystemParameters.WorkArea;
        Left = work.Left; Width = work.Width; Height = cycle.Expanded ? work.Height : 165; Top = work.Bottom - Height;
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
        if (cycle.Expanded || count == CatCount) return;
        catalog.Load(count); CatCount = count;
        surface.Cats = Enumerable.Range(0, count).Select(i => new CatMotion(i, count, Width)).ToArray();
        surface.InvalidateVisual();
        UpdatePanel();
    }

    private void VisitPark()
    {
        cycle.Visit(surface.Cats, Width); SyncScene();
    }
    private void ReturnToDesktop() { cycle.Return(); SyncScene(); }
    private void SyncScene()
    {
        if (expanded != cycle.Expanded)
        {
            expanded = cycle.Expanded;
            if (expanded) parkScene.Load(); else parkScene.Release();
            Place();
        }
        surface.InvalidateVisual(); UpdatePanel();
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
        CompositionTarget.Rendering -= OnRendering; clock.Restart(); lastTime = 0; pacer.Reset();
        if (!paused && !hidden) CompositionTarget.Rendering += OnRendering;
        UpdatePanel();
    }

    private void OpenPanel()
    {
        if (panel != null) { if (panel.WindowState == WindowState.Minimized) panel.WindowState = WindowState.Normal; panel.Activate(); return; }
        panel = new SettingsPanel(cycle.Settings, SaveSettings,
            () => { paused = !paused; UpdateAnimation(); }, () => SetHidden(!hidden), VisitPark, ReturnToDesktop, Close);
        panel.Closed += (_, _) => panel = null;
        UpdatePanel(); panel.Show();
    }

    private string? SaveSettings(ParkSettings settings)
    {
        if (!settings.Valid) return "Revisá la cantidad, la fluidez y los rangos de minutos.";
        if (cycle.Expanded && settings.Cats != CatCount) return "La cantidad se cambia al volver al escritorio. Las preferencias aplicadas siguen iguales.";
        try
        {
            // Prepare fallible assets before writing. A failed write changes no applied preference.
            CatMotion[]? replacement = null;
            if (settings.Cats != CatCount)
            {
                catalog.Load(settings.Cats);
                replacement = Enumerable.Range(0, settings.Cats).Select(i => new CatMotion(i, settings.Cats, Width)).ToArray();
            }
            settings.Save();
            cycle.Configure(settings);
            if (replacement != null) { surface.Cats = replacement; CatCount = settings.Cats; surface.InvalidateVisual(); }
            if (pacer.Target != settings.Fps) pacer.Configure(settings.Fps);
            UpdatePanel(); return null;
        }
        catch (Exception error) when (error is System.IO.IOException or UnauthorizedAccessException)
        { return "No se pudieron guardar las preferencias. Revisá el permiso de escritura y el espacio en la carpeta de la aplicación y volvé a guardar. Las preferencias aplicadas siguen iguales."; }
    }

    private void UpdatePanel()
    {
        panel?.Refresh(cycle.Settings, cycle.Expanded, paused, hidden, cycle.Status);
    }

    private void OnRendering(object? sender, EventArgs args)
    {
        if (args is not RenderingEventArgs render || !pacer.ShouldRender(render.RenderingTime.TotalSeconds)) return;
        double now = clock.Elapsed.TotalSeconds;
        double elapsed = now - lastTime; lastTime = now;
        // A long gap means suspension; do not turn it into a surprise visit on resume.
        if (elapsed > 2) elapsed = 0;
        while (elapsed > 0)
        {
            double dt = Math.Min(.1, elapsed);
            cycle.Advance(dt, surface.Cats, Width, SystemParameters.WorkArea.Height); elapsed -= dt;
        }
        if (expanded != cycle.Expanded) SyncScene();
        if ((int)now != statusSecond) { statusSecond = (int)now; UpdatePanel(); }
        Steps++; surface.InvalidateVisual();
    }
}

internal sealed class CatSurface : FrameworkElement
{
    private readonly SpriteCatalog catalog;
    private readonly ParkCycle cycle;
    private readonly ParkScene park;
    private readonly RadialGradientBrush shadow = new(Color.FromArgb(45, 24, 30, 34), Colors.Transparent);
    public CatMotion[] Cats { get; set; } = Array.Empty<CatMotion>();
    public long Frames { get; private set; }
    public CatSurface(SpriteCatalog catalog, ParkCycle cycle, ParkScene park)
    {
        this.catalog = catalog; this.cycle = cycle; this.park = park; shadow.Freeze();
        RenderOptions.SetBitmapScalingMode(this, BitmapScalingMode.LowQuality);
    }
    protected override void OnRender(DrawingContext dc)
    {
        Frames++;
        if (cycle.Expanded) park.Draw(dc, ActualWidth, ActualHeight, cycle.Opacity, cycle.SceneTime);
        foreach (var cat in Cats.OrderByDescending(c => c.Depth))
        {
            var sprite = catalog.Get(cat.Coat, cat.Pose);
            double feet = ActualHeight - 7 - cat.Depth;
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
