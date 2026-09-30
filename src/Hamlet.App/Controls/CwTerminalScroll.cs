namespace Hamlet.App.Controls;

/// <summary>
/// The CW terminal's scroll bar: what it says on hover (work instruction numbered 509, run as unit
/// 510, task 1, HM-DEC-214).
/// </summary>
/// <remarks>
/// <para>**TIM, 2026-09-30**: the terminal fills and the oldest text goes out of reach. It sat in a
/// scroll viewer all along, but Fluent's bar is a hairline that hides itself until the pointer
/// finds it, so the history looked gone. The bar now stays shown whenever the text is taller than
/// the box, and says on hover what it does (§0.6).</para>
/// <para>**THE FOLLOWING IS <see cref="CwTerminalControl"/>'S**, unchanged: it follows new text
/// while the view is within forty pixels of the bottom and holds still otherwise.</para>
/// </remarks>
public static class CwTerminalScroll
{
    /// <summary>What the bar says on hover.</summary>
    public const string BarTip =
        "Scroll up to read what has gone by. While you are up there the terminal holds still, "
        + "and once you scroll back to the bottom it follows the new text again.";
}
