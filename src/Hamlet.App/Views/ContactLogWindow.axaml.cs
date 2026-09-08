using Avalonia.Controls;

namespace Hamlet.App.Views;

/// <summary>
/// His own contacts, which the application had been writing and never showing.
/// </summary>
/// <remarks>
/// <para>**IT READS AND IT NEVER WRITES** (work instruction 278). There is no
/// handler here at all, which is the point: nothing in this window can edit,
/// delete or reorder a record, so there is no code path to get that wrong.</para>
/// <para>The companion to "What Hamlet decided" and "What the radio is doing".
/// Those two answer what the application and the radio are doing right now; this
/// one answers what he has actually done on the air.</para>
/// </remarks>
public partial class ContactLogWindow : Window
{
    /// <summary>Creates the window.</summary>
    public ContactLogWindow()
    {
        InitializeComponent();
    }
}
