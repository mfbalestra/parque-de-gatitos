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
    private TextBox? companionInput, stayInput;
    private TextBlock? scheduleStatus, settingsMessage;
    private readonly List<Button> countButtons = new();
    private readonly ParkCycle cycle;
    private readonly ParkScene parkScene = new();
    private bool expanded;
    private int statusSecond = -1;
    public string Scene => cycle.Phase.ToString();
    public int Visits => cycle.Visits;
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
        cycle = new ParkCycle(ParkSettings.Load());
        surface = new CatSurface(catalog, cycle, parkScene); Content = surface;
        Place(); SetCount(options.Cats); paused = options.Paused;
        tray = new Forms.NotifyIcon { Icon = System.Drawing.SystemIcons.Information, Text = "Gatitos — clic derecho para opciones" };
        var menu = new Forms.ContextMenuStrip();
        foreach (int count in new[] { 1, 4, 8 })
        {
            var item = new Forms.ToolStripMenuItem(count == 1 ? "1 gato" : $"{count} gatos") { Tag = count, Checked = count == CatCount };
            item.Click += (_, _) => SetCount(count); countItems.Add(item); menu.Items.Add(item);
        }
        menu.Items.Add(new Forms.ToolStripSeparator()); menu.Items.Add(pauseItem); menu.Items.Add(hideItem);
        menu.Items.Add("Ir al parque ahora", null, (_, _) => VisitPark());
        menu.Items.Add("Volver al escritorio", null, (_, _) => ReturnToDesktop());
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
        if (cycle.Expanded) return;
        catalog.Load(count); CatCount = count;
        surface.Cats = Enumerable.Range(0, count).Select(i => new CatMotion(i, count, Width)).ToArray();
        foreach (var item in countItems) item.Checked = (int)item.Tag! == count;
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
        foreach (var item in countItems) item.Enabled = !expanded;
        foreach (var button in countButtons) button.IsEnabled = !expanded;
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
        CompositionTarget.Rendering -= OnRendering; clock.Restart(); lastTime = 0; nextFrameAt = 0;
        if (!paused && !hidden) CompositionTarget.Rendering += OnRendering;
        UpdatePanel();
    }

    private void OpenPanel()
    {
        if (panel != null) { panel.Activate(); return; }
        panel = new Window { Title = "Controles de gatitos", Width = 420, Height = 710, ResizeMode = ResizeMode.CanMinimize, Topmost = true,
            WindowStartupLocation = WindowStartupLocation.CenterScreen };
        var layout = new StackPanel { Margin = new Thickness(22) };
        panelStatus = new TextBlock { FontSize = 18, Margin = new Thickness(0, 0, 0, 12) }; layout.Children.Add(panelStatus);
        foreach (int count in new[] { 1, 4, 8 })
        { var choice = new Button { Content = count == 1 ? "1 gato" : $"{count} gatos", Height = 34, Margin = new Thickness(0, 3, 0, 3), IsEnabled = !expanded }; choice.Click += (_, _) => SetCount(count); layout.Children.Add(choice); countButtons.Add(choice); }
        panelPause = new Button { Height = 34, Margin = new Thickness(0, 8, 0, 3) };
        panelPause.Click += (_, _) => { paused = !paused; UpdateAnimation(); }; layout.Children.Add(panelPause);
        panelHide = new Button { Height = 34, Margin = new Thickness(0, 3, 0, 3) };
        panelHide.Click += (_, _) => SetHidden(!hidden); layout.Children.Add(panelHide);
        scheduleStatus = new TextBlock { TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 12, 0, 8), FontWeight = FontWeights.SemiBold }; layout.Children.Add(scheduleStatus);
        var visit = new Button { Content = "Ir al parque ahora", Height = 34 }; visit.Click += (_, _) => VisitPark(); layout.Children.Add(visit);
        var back = new Button { Content = "Volver al escritorio", Height = 34, Margin = new Thickness(0, 4, 0, 8) }; back.Click += (_, _) => ReturnToDesktop(); layout.Children.Add(back);
        companionInput = new TextBox { Text = cycle.Settings.CompanionMinutes.ToString(), Height = 28 };
        stayInput = new TextBox { Text = cycle.Settings.StayMinutes.ToString(), Height = 28 };
        layout.Children.Add(new Label { Content = "_Compañía: minutos entre visitas (1–240)", Target = companionInput }); layout.Children.Add(companionInput);
        layout.Children.Add(new Label { Content = "_Estadía: minutos en el parque (1–60)", Target = stayInput }); layout.Children.Add(stayInput);
        var apply = new Button { Content = "Guardar tiempos", Height = 32, Margin = new Thickness(0, 8, 0, 2) }; apply.Click += (_, _) => SaveSettings(); layout.Children.Add(apply);
        settingsMessage = new TextBlock { TextWrapping = TextWrapping.Wrap, MinHeight = 34 }; layout.Children.Add(settingsMessage);
        var quit = new Button { Content = "Salir de los gatitos", Height = 34, Margin = new Thickness(0, 3, 0, 8) }; quit.Click += (_, _) => Close(); layout.Children.Add(quit);
        layout.Children.Add(new TextBlock { Text = "El parque tapa los programas, pero los clics pasan. Pausar u ocultar detiene también el reloj. La cantidad se cambia en el escritorio.", TextWrapping = TextWrapping.Wrap });
        panel.Content = new ScrollViewer { Content = layout, VerticalScrollBarVisibility = ScrollBarVisibility.Auto };
        panel.Closed += (_, _) => { panel = null; panelStatus = null; scheduleStatus = null; countButtons.Clear(); };
        UpdatePanel(); panel.Show();
    }

    private void SaveSettings()
    {
        if (!int.TryParse(companionInput!.Text.Trim(), out int companion) || !int.TryParse(stayInput!.Text.Trim(), out int stay) || !new ParkSettings(companion, stay).Valid)
        { settingsMessage!.Text = "Usá minutos enteros: compañía de 1 a 240 y estadía de 1 a 60."; return; }
        try
        {
            var settings = new ParkSettings(companion, stay); settings.Save(); cycle.Configure(settings);
            settingsMessage!.Text = "Tiempos guardados. Se aplican al ciclo actual."; UpdatePanel();
        }
        catch (Exception error) when (error is System.IO.IOException or UnauthorizedAccessException)
        { settingsMessage!.Text = "No se pudieron guardar los tiempos. Usá una carpeta con permiso de escritura."; }
    }

    private void UpdatePanel()
    {
        if (panelStatus == null) return;
        panelStatus.Text = $"{CatCount} gato{(CatCount == 1 ? "" : "s")} · {(hidden ? "ocultos" : paused ? "en pausa" : "en movimiento")}";
        panelPause!.Content = paused ? "Reanudar" : "Pausar";
        panelHide!.Content = hidden ? "Mostrar" : "Ocultar";
        if (scheduleStatus != null) scheduleStatus.Text = cycle.Status;
    }

    private void OnRendering(object? sender, EventArgs args)
    {
        double now = clock.Elapsed.TotalSeconds;
        if (now < nextFrameAt) return;
        nextFrameAt = (Math.Floor(now * 30) + 1) / 30;
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
