using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Media;

namespace Hamlet.App.Controls;

/// <summary>
/// Which color family a panel belongs to (HM-DEC-012): amber is tuning,
/// blue is spectrum, green is decode, slate is everything else.
/// </summary>
public enum PanelFamily
{
    /// <summary>Neutral — no family claim.</summary>
    Slate,

    /// <summary>Tuning.</summary>
    Amber,

    /// <summary>Spectrum.</summary>
    Blue,

    /// <summary>Decode.</summary>
    Green,
}

/// <summary>
/// A panel with a clickable header that collapses its content (HM-DEC-021,
/// CLAUDE.md §0.5). Every panel in Hamlet is one of these; the rig display is
/// the single deliberate exception.
/// </summary>
/// <remarks>
/// <para>The header carries chevron + title on the left in the panel's family
/// color — as TEXT color only. The header bar is never filled with the
/// family color: seven filled bars stacked down a window read as a stripe
/// pattern, not as structure.</para>
/// <para><see cref="Summary"/> is right-aligned and survives collapse. That is
/// the whole point: collapsing hides detail, never information, so a shut
/// panel still says "7 spots · updated 30s ago" rather than going silent.</para>
/// </remarks>
public sealed class CollapsiblePanel : ContentControl
{
    /// <summary>Panel title, shown in the family color.</summary>
    public static readonly StyledProperty<string> TitleProperty =
        AvaloniaProperty.Register<CollapsiblePanel, string>(nameof(Title), "");

    /// <summary>The line that survives collapse.</summary>
    public static readonly StyledProperty<string> SummaryProperty =
        AvaloniaProperty.Register<CollapsiblePanel, string>(nameof(Summary), "");

    /// <summary>Color family for the header text and the panel edge.</summary>
    public static readonly StyledProperty<PanelFamily> FamilyProperty =
        AvaloniaProperty.Register<CollapsiblePanel, PanelFamily>(nameof(Family));

    /// <summary>Whether the content is showing. Two-way by default so the
    /// ViewModel can persist it without ceremony at every use site.</summary>
    public static readonly StyledProperty<bool> IsExpandedProperty =
        AvaloniaProperty.Register<CollapsiblePanel, bool>(
            nameof(IsExpanded), true, defaultBindingMode: BindingMode.TwoWay);

    /// <summary>The family text brush, resolved from <see cref="Family"/>.</summary>
    public static readonly StyledProperty<IBrush?> FamilyBrushProperty =
        AvaloniaProperty.Register<CollapsiblePanel, IBrush?>(nameof(FamilyBrush));

    /// <summary>Brush for the summary text. Defaults to muted; a panel whose
    /// summary carries a warning (a stale feed) overrides it.</summary>
    public static readonly StyledProperty<IBrush?> SummaryBrushProperty =
        AvaloniaProperty.Register<CollapsiblePanel, IBrush?>(
            nameof(SummaryBrush), new SolidColorBrush(Color.Parse("#6E6E66")));

    /// <summary>The chevron glyph for the current state.</summary>
    public static readonly StyledProperty<string> ChevronProperty =
        AvaloniaProperty.Register<CollapsiblePanel, string>(nameof(Chevron), "▾");

    private Button? _header;

    static CollapsiblePanel()
    {
        FamilyProperty.Changed.AddClassHandler<CollapsiblePanel>((c, _) => c.ApplyFamily());
        IsExpandedProperty.Changed.AddClassHandler<CollapsiblePanel>((c, _) => c.ApplyChevron());
    }

    /// <summary>Creates the panel with its family colors applied.</summary>
    public CollapsiblePanel()
    {
        ApplyFamily();
        ApplyChevron();
    }

    /// <summary>Panel title, shown in the family color.</summary>
    public string Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    /// <summary>The line that survives collapse — never decoration, always
    /// the fact the operator would otherwise lose.</summary>
    public string Summary
    {
        get => GetValue(SummaryProperty);
        set => SetValue(SummaryProperty, value);
    }

    /// <summary>Color family for the header text and the panel edge.</summary>
    public PanelFamily Family
    {
        get => GetValue(FamilyProperty);
        set => SetValue(FamilyProperty, value);
    }

    /// <summary>Whether the content is showing.</summary>
    public bool IsExpanded
    {
        get => GetValue(IsExpandedProperty);
        set => SetValue(IsExpandedProperty, value);
    }

    /// <summary>The family text brush, resolved from <see cref="Family"/>.</summary>
    public IBrush? FamilyBrush
    {
        get => GetValue(FamilyBrushProperty);
        set => SetValue(FamilyBrushProperty, value);
    }

    /// <summary>Brush for the summary text.</summary>
    public IBrush? SummaryBrush
    {
        get => GetValue(SummaryBrushProperty);
        set => SetValue(SummaryBrushProperty, value);
    }

    /// <summary>The chevron glyph for the current state.</summary>
    public string Chevron
    {
        get => GetValue(ChevronProperty);
        set => SetValue(ChevronProperty, value);
    }

    /// <inheritdoc/>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        if (_header is not null)
        {
            _header.Click -= OnHeaderClick;
        }

        _header = e.NameScope.Find<Button>("PART_Header");
        if (_header is not null)
        {
            _header.Click += OnHeaderClick;
        }
    }

    private void OnHeaderClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        => SetCurrentValue(IsExpandedProperty, !IsExpanded);

    /// <summary>Chevron glyph, and a <c>:collapsed</c> pseudo-class so a shut
    /// panel stops claiming the height its content wanted.</summary>
    private void ApplyChevron()
    {
        SetCurrentValue(ChevronProperty, IsExpanded ? "▾" : "▸");
        PseudoClasses.Set(":collapsed", !IsExpanded);
    }

    /// <summary>
    /// Family color is applied as header text and panel edge only. Panel
    /// bodies stay white per HM-DEC-012, white panels on warm paper, which is
    /// also what keeps a column of collapsed headers from striping.
    /// </summary>
    /// <remarks>
    /// The values come from <see cref="PanelPalette"/> rather than from hex
    /// literals here. They used to live in this method, which is why the
    /// Settings window could not use the same families without becoming a
    /// second copy of them (HM-DEC-044).
    /// </remarks>
    private void ApplyFamily()
    {
        var colors = PanelPalette.For(Family);

        SetCurrentValue(FamilyBrushProperty, colors.TitleBrush);
        SetCurrentValue(BorderBrushProperty, colors.EdgeBrush);
    }
}
