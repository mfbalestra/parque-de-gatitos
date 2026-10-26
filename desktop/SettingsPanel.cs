using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Controls;

namespace Gatitos;

// A normal, resizable WPF window. The passive overlay never owns keyboard focus.
internal sealed class SettingsPanel : Window
{
    private readonly TextBlock activity = Text(), schedule = Text(), dirty = Text(), message = Text();
    private readonly TextBlock countHelp = Text(), companionError = Text(), stayError = Text();
    private readonly TextBox companion = Input("CompanionMinutes"), stay = Input("StayMinutes");
    private readonly List<RadioButton> counts = new(), rates = new();
    private readonly Button pause, hide, visit, back;
    private ParkSettings applied;
    private bool refreshing, expanded;
    private readonly Func<ParkSettings, string?> save;

    public SettingsPanel(ParkSettings settings, Func<ParkSettings, string?> save,
        Action togglePause, Action toggleVisibility, Action visitPark, Action returnToDesktop, Action quit)
    {
        applied = settings; this.save = save; refreshing = true;
        Title = "Controles de gatitos"; Width = 520; Height = Math.Min(840, SystemParameters.WorkArea.Height);
        MinWidth = 340; MinHeight = 360; ResizeMode = ResizeMode.CanResize; Topmost = true;
        WindowStartupLocation = WindowStartupLocation.CenterScreen;
        FontFamily = SystemFonts.MessageFontFamily; FontSize = 14;
        Background = SystemColors.WindowBrush; Foreground = SystemColors.WindowTextBrush;
        var layout = new StackPanel { Margin = new Thickness(22) };
        layout.Children.Add(new TextBlock { Text = "Gatitos", FontSize = 24, FontWeight = FontWeights.SemiBold });
        activity.FontSize = 17; activity.FontWeight = FontWeights.SemiBold; activity.Margin = new Thickness(0, 10, 0, 4);
        AutomationProperties.SetAutomationId(activity, "ActivityStatus"); layout.Children.Add(activity);
        AutomationProperties.SetAutomationId(schedule, "ScheduleStatus"); layout.Children.Add(schedule);

        var actions = new WrapPanel();
        pause = ActionButton("_Pausar", "Pause", togglePause); actions.Children.Add(pause);
        hide = ActionButton("_Ocultar", "Visibility", toggleVisibility); actions.Children.Add(hide);
        visit = ActionButton("_Ir al parque ahora", "VisitPark", visitPark); actions.Children.Add(visit);
        back = ActionButton("_Volver al escritorio", "ReturnDesktop", returnToDesktop); actions.Children.Add(back);
        layout.Children.Add(Group("Acciones inmediatas", actions));

        var movement = new StackPanel();
        movement.Children.Add(Choices("Cantidad de gatos", "Cats", new[] { 1, 4, 8 }, settings.Cats, counts,
            n => n == 1 ? "1 gato" : $"{n} gatos"));
        countHelp.Margin = new Thickness(0, 2, 0, 12); movement.Children.Add(countHelp);
        movement.Children.Add(Choices("Fluidez", "Fps", new[] { 30, 60 }, settings.Fps, rates, n => $"{n} FPS"));
        movement.Children.Add(Text("30 FPS dibuja con menor frecuencia. 60 FPS ofrece más actualizaciones, según la pantalla y el equipo."));
        layout.Children.Add(Group("Gatos y movimiento", movement));

        var durations = new StackPanel();
        var fields = new Grid();
        fields.ColumnDefinitions.Add(new ColumnDefinition()); fields.ColumnDefinitions.Add(new ColumnDefinition());
        fields.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        fields.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        var companionField = new StackPanel(); var stayField = new StackPanel();
        AddField(companionField, "_Compañía: minutos entre visitas (1–240)", companion, companionError);
        AddField(stayField, "_Estadía: minutos en el parque (1–60)", stay, stayError);
        fields.Children.Add(companionField); fields.Children.Add(stayField);
        fields.SizeChanged += (_, _) =>
        {
            bool sideBySide = fields.ActualWidth >= 400;
            Grid.SetColumnSpan(companionField, sideBySide ? 1 : 2);
            Grid.SetColumnSpan(stayField, sideBySide ? 1 : 2);
            Grid.SetColumn(stayField, sideBySide ? 1 : 0); Grid.SetRow(stayField, sideBySide ? 0 : 1);
            companionField.Margin = new Thickness(0, 0, sideBySide ? 8 : 0, 0);
            stayField.Margin = new Thickness(sideBySide ? 8 : 0, 0, 0, 0);
        };
        durations.Children.Add(fields);
        durations.Children.Add(Text("Los tiempos se aplican al ciclo actual y conservan el tiempo ya transcurrido."));
        layout.Children.Add(Group("Visitas al parque", durations));
        companion.Text = settings.CompanionMinutes.ToString(CultureInfo.CurrentCulture);
        stay.Text = settings.StayMinutes.ToString(CultureInfo.CurrentCulture);
        companion.TextChanged += (_, _) => Edited(); stay.TextChanged += (_, _) => Edited();

        var guidance = Text("Cerrar esta ventana deja los gatos activos. Pausar u ocultar detiene también el reloj del parque; los clics pasan a través del parque y los gatos.");
        guidance.Margin = new Thickness(0, 18, 0, 0); layout.Children.Add(guidance);

        // The scroll region yields height first: saving, feedback and exit remain reachable.
        var footer = new StackPanel();
        dirty.FontWeight = FontWeights.SemiBold; dirty.Margin = new Thickness(0, 0, 0, 4);
        AutomationProperties.SetAutomationId(dirty, "PendingChanges"); footer.Children.Add(dirty);
        var commit = ActionButton("_Guardar preferencias", "SavePreferences", Save);
        commit.HorizontalAlignment = HorizontalAlignment.Left; footer.Children.Add(commit);
        message.Margin = new Thickness(0, 4, 0, 0);
        AutomationProperties.SetAutomationId(message, "SettingsMessage");
        AutomationProperties.SetLiveSetting(message, AutomationLiveSetting.Polite); footer.Children.Add(message);
        footer.Children.Add(new Separator { Margin = new Thickness(0, 10, 0, 6) });
        var exit = ActionButton("_Salir de los gatitos", "Exit", quit); exit.Margin = new Thickness(0);
        exit.HorizontalAlignment = HorizontalAlignment.Left; footer.Children.Add(exit);
        var frame = new Grid(); frame.RowDefinitions.Add(new RowDefinition());
        frame.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        frame.Children.Add(new ScrollViewer { Content = layout, VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled, CanContentScroll = false });
        var fixedFooter = new Border { Child = footer, Padding = new Thickness(22, 12, 22, 12),
            BorderThickness = new Thickness(0, 1, 0, 0), BorderBrush = SystemColors.ActiveBorderBrush };
        Grid.SetRow(fixedFooter, 1); frame.Children.Add(fixedFooter); Content = frame;
        refreshing = false; UpdateDirty();
    }

    public void Refresh(ParkSettings settings, bool isExpanded, bool paused, bool hidden, string status)
    {
        applied = settings; expanded = isExpanded;
        activity.Text = $"{settings.Cats} gato{(settings.Cats == 1 ? "" : "s")} {(expanded ? "en el parque" : "en el escritorio")} · {(hidden ? (paused ? "ocultos y en pausa" : "ocultos") : paused ? "en pausa" : "activos")}";
        schedule.Text = status + (paused || hidden ? " · reloj detenido" : "");
        pause.Content = paused ? "_Reanudar" : "_Pausar"; hide.Content = hidden ? "_Mostrar" : "_Ocultar";
        // Keep Visit enabled while leaving: the existing cycle queues the requested next visit.
        back.IsEnabled = expanded;
        refreshing = true;
        if (expanded && Selected(counts) != settings.Cats)
        {
            Select(counts, settings.Cats);
            Feedback("La cantidad pendiente no se aplicó: comenzó la visita al parque. Podés volver a elegirla al regresar.");
        }
        foreach (var option in counts) option.IsEnabled = !expanded;
        refreshing = false;
        countHelp.Text = expanded ? "La cantidad no se puede cambiar mientras el parque está abierto. La fluidez y los tiempos sí se pueden guardar."
            : "La cantidad cambia al guardar preferencias, mientras están en el escritorio.";
        UpdateDirty();
    }

    private void Save()
    {
        bool validCompanion = int.TryParse(companion.Text.Trim(), out int companionMinutes) && ParkSettings.ValidCompanion(companionMinutes);
        bool validStay = int.TryParse(stay.Text.Trim(), out int stayMinutes) && ParkSettings.ValidStay(stayMinutes);
        companionError.Text = validCompanion ? "" : "Ingresá minutos enteros entre 1 y 240.";
        stayError.Text = validStay ? "" : "Ingresá minutos enteros entre 1 y 60.";
        if (!validCompanion || !validStay)
        {
            Feedback("Revisá los campos indicados. Las preferencias aplicadas siguen iguales.");
            var invalid = validCompanion ? stay : companion;
            invalid.Focus();
            // Include the neighbouring field error, not just the input's rectangle.
            ((FrameworkElement)invalid.Parent).BringIntoView(); return;
        }
        // Read the live lock again; no stale disabled radio can enter a saved preference.
        var draft = new ParkSettings(companionMinutes, stayMinutes, expanded ? applied.Cats : Selected(counts), Selected(rates));
        string? error = save(draft);
        if (error != null) { Feedback(error); return; }
        applied = draft; UpdateDirty(); Feedback("Preferencias guardadas. Los tiempos se aplican al ciclo actual.");
    }

    private void Edited()
    {
        if (refreshing) return;
        companionError.Text = ""; stayError.Text = ""; message.Text = ""; UpdateDirty();
    }
    private void UpdateDirty()
    {
        bool changed = Selected(counts) != applied.Cats || Selected(rates) != applied.Fps
            || !int.TryParse(companion.Text.Trim(), out int c) || c != applied.CompanionMinutes
            || !int.TryParse(stay.Text.Trim(), out int s) || s != applied.StayMinutes;
        dirty.Text = changed ? "Cambios sin guardar" : "Preferencias aplicadas en esta sesión";
    }
    private void Feedback(string value)
    {
        message.Text = value;
        UIElementAutomationPeer.FromElement(message)?.RaiseAutomationEvent(AutomationEvents.LiveRegionChanged);
    }
    private static int Selected(List<RadioButton> choices) => (int)choices.First(x => x.IsChecked == true).Tag;
    private static void Select(List<RadioButton> choices, int value) { foreach (var option in choices) option.IsChecked = (int)option.Tag == value; }
    private FrameworkElement Choices(string label, string name, int[] values, int selected, List<RadioButton> list, Func<int, string> caption)
    {
        var stack = new StackPanel(); stack.Children.Add(Text(label));
        var row = new WrapPanel();
        foreach (int value in values)
        {
            var radio = new RadioButton { Content = caption(value), GroupName = name, Tag = value, IsChecked = value == selected,
                MinHeight = 44, MinWidth = 96, VerticalContentAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 0, 8, 0) };
            AutomationProperties.SetAutomationId(radio, name + value); AutomationProperties.SetHelpText(radio, label);
            radio.Checked += (_, _) => Edited(); list.Add(radio); row.Children.Add(radio);
        }
        stack.Children.Add(row); return stack;
    }
    private static void AddField(Panel parent, string caption, TextBox input, TextBlock error)
    {
        var label = new Label { Content = new AccessText { Text = caption, TextWrapping = TextWrapping.Wrap }, Target = input,
            Padding = new Thickness(0, 4, 0, 4) };
        AutomationProperties.SetLabeledBy(input, label); parent.Children.Add(label);
        parent.Children.Add(input); error.Margin = new Thickness(0, 3, 0, 8); parent.Children.Add(error);
    }
    private static TextBox Input(string id)
    {
        var input = new TextBox { MinHeight = 44, VerticalContentAlignment = VerticalAlignment.Center, Padding = new Thickness(8, 4, 8, 4) };
        AutomationProperties.SetAutomationId(input, id); return input;
    }
    private static TextBlock Text(string value = "") => new() { Text = value, TextWrapping = TextWrapping.Wrap };
    private static GroupBox Group(string title, UIElement content) => new() { Header = title, Content = content,
        Margin = new Thickness(0, 18, 0, 0), Padding = new Thickness(12) };
    private static Button ActionButton(string caption, string id, Action action)
    {
        var button = new Button { Content = caption, MinHeight = 44, MinWidth = 182, Padding = new Thickness(12, 5, 12, 5), Margin = new Thickness(0, 4, 8, 4) };
        AutomationProperties.SetAutomationId(button, id); button.Click += (_, _) => action(); return button;
    }
}
