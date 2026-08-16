using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.VisualTree;
using Hamlet.App.ViewModels;

namespace Hamlet.App.Controls;

/// <summary>
/// Shows whatever a widget shows, found by its name (HM-DEC-086).
/// </summary>
/// <remarks>
/// <para>**THE PANELS DID NOT CHANGE WHEN THEY BECAME WIDGETS.** Each one is the
/// same markup it always was, moved into a template keyed by its widget id, and
/// what it binds against is still the main view model. So none of the bindings
/// inside thirteen panels had to be rewritten to gain a position, and none of
/// them can have been rewritten wrongly.</para>
/// <para>The lookup goes up the resource tree, which means the templates live
/// beside the window that hosts them rather than in the application's global
/// resources, where they would be loaded whether or not anything wanted them.
/// </para>
/// </remarks>
public sealed class WidgetBody : ContentControl
{
    /// <summary>The prefix every widget template is keyed under.</summary>
    public const string KeyPrefix = "widget.";

    /// <summary>The resource key for one widget's body.</summary>
    /// <param name="id">The widget's stable name.</param>
    /// <returns>The key its template is stored under.</returns>
    public static string Key(string id) => KeyPrefix + id;

    /// <inheritdoc />
    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);
        Resolve();
    }

    /// <inheritdoc />
    protected override void OnAttachedToVisualTree(
        Avalonia.VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        Resolve();
    }

    private void Resolve()
    {
        if (DataContext is not WidgetViewModel widget)
        {
            return;
        }

        Content = widget.Body;

        // A WIDGET WITH NO TEMPLATE SHOWS NOTHING RATHER THAN SOMETHING WRONG.
        // It cannot happen with the catalog and the templates in step, and if it
        // ever does, an empty frame the operator can close beats a stack trace
        // or a box saying the name of a thing they never asked about (§0.0).
        if (this.TryFindResource(Key(widget.Id), out var found)
            && found is IDataTemplate template)
        {
            ContentTemplate = template;
        }

        // The panel inside is built from that template, so it does not exist
        // until the layout pass after this one.
        Avalonia.Threading.Dispatcher.UIThread.Post(
            Follow, Avalonia.Threading.DispatcherPriority.Loaded);
    }

    /// <summary>
    /// Follow the panel's own collapse, so the widget shrinks with it
    /// (HM-DEC-086).
    /// </summary>
    /// <remarks>
    /// <para>**A COLLAPSED WIDGET USED TO LEAVE AN EMPTY BOX.** In a column a
    /// panel that shut handed its space to the panel below it (HM-DEC-021), and
    /// on a canvas the frame kept the height it was given, so collapsing
    /// something left a rectangle of nothing where it used to be. That is
    /// §0.5 broken by the surface rather than by the panel.</para>
    /// <para>**The panel still owns the state.** It goes on persisting whether it
    /// is open, per panel, in `settings.json`. This only watches it, so there is
    /// one answer to the question and the frame follows it (§0).</para>
    /// </remarks>
    private void Follow()
    {
        if (DataContext is not WidgetViewModel widget)
        {
            return;
        }

        var panel = this.GetVisualDescendants().OfType<CollapsiblePanel>().FirstOrDefault();

        if (panel is null)
        {
            return;
        }

        widget.IsExpanded = panel.IsExpanded;

        panel.PropertyChanged += (_, e) =>
        {
            if (e.Property == CollapsiblePanel.IsExpandedProperty
                && DataContext is WidgetViewModel current)
            {
                current.IsExpanded = panel.IsExpanded;
            }
        };
    }
}
