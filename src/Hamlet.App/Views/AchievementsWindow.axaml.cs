using Avalonia.Controls;

namespace Hamlet.App.Views;

/// <summary>
/// What he has done: the belt, and the first contact in each mode.
/// </summary>
/// <remarks>
/// <para>**IT READS AND IT NEVER WRITES** (Tim's ruling, 2026-09-08, and unit
/// 278's before it). There is no handler here at all, which is the point: nothing
/// in this window can touch a record, and nothing in it reaches a send path, so
/// there is no code path to get either of those wrong.</para>
/// <para>**BESIDE `My contacts…` RATHER THAN UNDER HELP.** Help is *how do I use
/// this*; this is his own operating record, which is the same kind of thing as his
/// log and belongs next to it.</para>
/// </remarks>
public partial class AchievementsWindow : Window
{
    /// <summary>Creates the window.</summary>
    public AchievementsWindow()
    {
        InitializeComponent();
    }
}
