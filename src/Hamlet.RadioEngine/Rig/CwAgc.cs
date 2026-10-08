using Hamlet.RadioEngine.Civ;
using Hamlet.RadioEngine.Explore;

namespace Hamlet.RadioEngine.Rig;

/// <summary>What the operator chose for the radio's AGC on the CW tab (work instruction 564, HM-DEC-268).</summary>
public enum CwAgcChoice
{
    /// <summary>SLOW, the default: the gain sets on the station's peak and holds through the gaps.</summary>
    Slow,

    /// <summary>MID.</summary>
    Mid,

    /// <summary>FAST, what Hamlet set before work instruction 564.</summary>
    Fast,

    /// <summary>Hamlet writes nothing to the AGC.</summary>
    LeaveAlone,
}

/// <summary>What Hamlet changed the AGC from and to on entering CW, so it can be put back on leaving.</summary>
/// <param name="Before">The value the radio had before Hamlet wrote, on the read's scale.</param>
/// <param name="Set">The value Hamlet set, on the same scale.</param>
public sealed record CwAgcHold(int Before, int Set);

/// <summary>
/// **HAMLET SETS AGC FOR CW, AS IT SETS THE PREAMP** (work instruction 564, HM-DEC-268). The owner, 2026-10-08, asked how to
/// change the radio's AGC: *"I have no idea how to do that."* The CW row of the receiver conditions sets it once on entering
/// CW, through command `16 12` (`00`=off, `01`=FAST, `02`=MID, `03`=SLOW; IC-7300 Full Manual `A7292-4EX-6`, p. 19-3), and
/// his hand wins as it does for every row. This holds what the choice changes and what leaving CW puts back.
/// </summary>
public static class CwAgc
{
    /// <summary>The value command `16 12` takes for FAST.</summary>
    public const int Fast = 1;

    /// <summary>The value for MID.</summary>
    public const int Mid = 2;

    /// <summary>The value for SLOW.</summary>
    public const int Slow = 3;

    /// <summary>The value a choice writes, or null where it writes nothing.</summary>
    /// <param name="choice">The choice.</param>
    /// <returns>The value on `16 12`'s scale, or null.</returns>
    public static int? Wanted(CwAgcChoice choice) => choice switch
    {
        CwAgcChoice.Slow => Slow,
        CwAgcChoice.Mid => Mid,
        CwAgcChoice.Fast => Fast,
        _ => null,
    };

    /// <summary>A value in the radio's own word.</summary>
    /// <param name="value">The value on `16 12`'s scale.</param>
    /// <returns>OFF, FAST, MID or SLOW, or the number where it is none of them.</returns>
    public static string Words(int value) => value switch
    {
        0 => "OFF",
        Fast => "FAST",
        Mid => "MID",
        Slow => "SLOW",
        _ => value.ToString(System.Globalization.CultureInfo.InvariantCulture),
    };

    /// <summary>The CW row with its AGC condition set to the choice, or taken out where the choice is to leave it alone.</summary>
    /// <param name="conditions">The CW row, as the data file states it.</param>
    /// <param name="choice">The operator's choice.</param>
    /// <returns>The conditions to apply.</returns>
    public static IReadOnlyList<ReceiverCondition> Apply(IReadOnlyList<ReceiverCondition> conditions, CwAgcChoice choice)
    {
        ArgumentNullException.ThrowIfNull(conditions);

        if (Wanted(choice) is not { } wanted)
        {
            return conditions.Where(c => c.Field != RigField.Agc).ToList();
        }

        return conditions
            .Select(c => c.Field == RigField.Agc ? c with { Wanted = wanted, WantedText = Words(wanted).ToLowerInvariant() } : c)
            .ToList();
    }

    /// <summary>What the tune-in changed, to be put back on leaving CW, or null where it changed nothing.</summary>
    /// <param name="results">The tune-in's results.</param>
    /// <returns>The hold, or null.</returns>
    public static CwAgcHold? HoldFrom(IEnumerable<ConditionResult> results)
    {
        ArgumentNullException.ThrowIfNull(results);

        return results.FirstOrDefault(r => r.Condition.Field == RigField.Agc && r.Outcome == ConditionOutcome.Changed) is { WasNumber: { } before, Condition.Wanted: { } set }
            ? new CwAgcHold(before, set)
            : null;
    }

    /// <summary>
    /// Who the AGC the radio reports now belongs to, in words for the capture sheet and the telemetry row.
    /// </summary>
    /// <param name="reading">What the radio reports now, or null where unread.</param>
    /// <param name="inCw">Whether the CW tab is the mode.</param>
    /// <param name="choice">The operator's choice.</param>
    /// <param name="hamletLeft">What the last CW tune-in left the AGC at, set or found, or null where it left nothing.</param>
    /// <param name="hamletWrote">Whether that tune-in wrote it.</param>
    /// <returns>A short phrase.</returns>
    public static string SetBy(int? reading, bool inCw, CwAgcChoice choice, int? hamletLeft, bool hamletWrote)
    {
        if (reading is null)
        {
            return "unknown, the radio has not said";
        }

        if (!inCw)
        {
            return "not Hamlet's, the CW tab is not the mode";
        }

        if (choice == CwAgcChoice.LeaveAlone)
        {
            return "the radio's own, Hamlet is set to leave it alone";
        }

        if (hamletLeft is not { } left)
        {
            return "not yet set by Hamlet";
        }

        if (reading != left)
        {
            return "set by hand";
        }

        return hamletWrote ? "set by Hamlet" : "already so when Hamlet looked";
    }

    /// <summary>
    /// Puts back the AGC the radio had before Hamlet changed it on entering CW, unless his hand has moved it since.
    /// </summary>
    /// <param name="rig">The radio.</param>
    /// <param name="hold">What Hamlet changed it from and to.</param>
    /// <param name="cancellationToken">Cancellation.</param>
    /// <returns>A sentence for the story line, or "" where nothing was written.</returns>
    /// <remarks>
    /// **READ BEFORE WRITE, READ BACK AFTER** (HM-DEC-084). Where the radio does not say what it is, nothing is written: not
    /// knowing is not a licence to write. Where it reads anything but what Hamlet set, it is his and it stays.
    /// </remarks>
    public static async Task<string> RestoreAsync(IRig rig, CwAgcHold hold, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(rig);
        ArgumentNullException.ThrowIfNull(hold);

        var now = (await rig.ReadAsync(RigField.Agc, RigState.Empty, cancellationToken).ConfigureAwait(false))
            .FirstOrDefault(v => v.Field == RigField.Agc);

        if (now is not { IsKnown: true, Number: { } reading } || (int)reading != hold.Set)
        {
            return "";
        }

        var result = await rig.SetSettingAsync(CivWrites.Agc, hold.Before, cancellationToken).ConfigureAwait(false);

        if (!result.Worked)
        {
            return $"Hamlet could not put the AGC back to {Words(hold.Before)}; the radio did not confirm it.";
        }

        return $"AGC put back to {Words(hold.Before)}, as it was before CW.";
    }
}

/// <summary>The operator's own choices about the receive side, handed to the engine with a tab's conditions (work instruction 564).</summary>
/// <param name="CwAgc">What Hamlet sets the AGC to on the CW tab.</param>
public sealed record ReceiverChoices(CwAgcChoice CwAgc);
