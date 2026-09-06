namespace Hamlet.App.ViewModels;

/// <summary>Which decoded messages the operator wants to stand out.</summary>
/// <remarks>
/// **THREE, AND THEY ARE TIM'S** (ruling of 2026-09-05). The set is not this
/// code's to extend.
/// </remarks>
public enum DecodedFilter
{
    /// <summary>Everything, which is where a fresh settings file starts.</summary>
    Everything,

    /// <summary>Only messages addressed to anyone.</summary>
    CqOnly,

    /// <summary>Only messages addressed to the operator.</summary>
    Mine,
}

/// <summary>
/// Whether a decoded row is one the current filter is interested in.
/// </summary>
/// <remarks>
/// <para>**THE PREDICATE IS OVER THE TO-FIELD AND NOTHING ELSE.**
/// `DigitalDecodeRow` already splits a message into addressee, sender and
/// payload, so this reads a field the row has rather than parsing the message a
/// second time. A second parser is a second answer waiting to disagree.</para>
/// <para>**A ROW WITH NO THREE FIELDS IS NOT A MATCH AND IS NOT AN ERROR.** Free
/// text and telemetry have no addressee, so they cannot be addressed to anyone in
/// particular and cannot be addressed to the operator. Under `Everything` they
/// are shown like everything else.</para>
/// <para>**IT NEVER HIDES A ROW.** The answer here decides whether a row is drawn
/// dimmed, and dimming is the whole of what a filter does on this panel — see
/// `MainWindowViewModel.ApplyDecodedFilter`.</para>
/// </remarks>
public static class DecodedFilterRule
{
    /// <summary>Whether a row is one this filter wants.</summary>
    /// <param name="filter">The filter.</param>
    /// <param name="addressee">The row's to-field, which may be "".</param>
    /// <param name="mine">The operator's callsign, or null where it is unknown.</param>
    /// <returns>True when the row is one the filter is interested in.</returns>
    /// <remarks>
    /// **AN UNKNOWN CALLSIGN MAKES `Mine` MATCH EVERYTHING, NOT NOTHING.** Tim's
    /// ruling: `mine` is still offered where Hamlet does not know his callsign,
    /// and it says it has nothing to match on rather than silently showing an
    /// empty band. Returning false for every row would be exactly the silent
    /// nothing the ruling forbids, and the operator would read a busy evening as
    /// a dead one.
    /// </remarks>
    public static bool Wants(DecodedFilter filter, string? addressee, string? mine)
    {
        var to = addressee?.Trim() ?? "";

        return filter switch
        {
            DecodedFilter.CqOnly => IsCallToAnyone(to),
            DecodedFilter.Mine => !HasSomethingToMatchOn(mine)
                                  || string.Equals(
                                      to, mine!.Trim(),
                                      StringComparison.OrdinalIgnoreCase),
            _ => true,
        };
    }

    /// <summary>Whether the app knows a callsign to match `mine` against.</summary>
    /// <param name="mine">The operator's callsign from settings.</param>
    /// <returns>True when there is something to compare a to-field with.</returns>
    public static bool HasSomethingToMatchOn(string? mine)
        => !string.IsNullOrWhiteSpace(mine);

    /// <summary>
    /// A to-field that is a call to anyone: `CQ`, and `CQ DX` or `CQ EU`.
    /// </summary>
    /// <param name="to">The to-field.</param>
    /// <returns>True when it is a call to anyone.</returns>
    /// <remarks>
    /// **CQ IN ANY OF ITS FORMS**, which is the ruling's own wording.
    /// `Ft8Vocabulary.Split` joins `CQ DX` into one addressee already, because
    /// the call and its direction are one field in two words.
    /// </remarks>
    public static bool IsCallToAnyone(string? to)
    {
        var text = to?.Trim() ?? "";

        return string.Equals(text, "CQ", StringComparison.OrdinalIgnoreCase)
               || text.StartsWith("CQ ", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>What a filter is called on a control.</summary>
    /// <param name="filter">The filter.</param>
    /// <returns>The label.</returns>
    public static string Label(DecodedFilter filter) => filter switch
    {
        DecodedFilter.CqOnly => "CQ only",
        DecodedFilter.Mine => "mine",
        _ => "everything",
    };
}
