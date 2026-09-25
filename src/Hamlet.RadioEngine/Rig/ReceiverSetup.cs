using Hamlet.RadioEngine.Civ;
using Hamlet.RadioEngine.Explore;

namespace Hamlet.RadioEngine.Rig;

/// <summary>What became of one receiver condition on one tune-in.</summary>
public enum ConditionOutcome
{
    /// <summary>The radio was already set that way, so nothing was sent.</summary>
    AlreadyRight,

    /// <summary>Hamlet changed it, and the radio confirmed the new value.</summary>
    Changed,

    /// <summary>
    /// The operator has moved this himself since Hamlet last set it, so it is
    /// his (HM-DEC-056).
    /// </summary>
    LeftToTheOperator,

    /// <summary>
    /// The radio would not say what it was, so nothing was changed.
    /// </summary>
    /// <remarks>
    /// **NOT KNOWING IS NOT A LICENCE TO WRITE.** Without a reading Hamlet
    /// cannot tell an operator who set this deliberately from a radio nobody has
    /// touched, and the operator's own hand wins. Silence is a stop, exactly as
    /// it is for the scanner (§0.2.1).
    /// </remarks>
    NotRead,

    /// <summary>
    /// It was written and the radio did not confirm the new value, so what it is
    /// now is unknown rather than assumed.
    /// </summary>
    NotConfirmed,

    /// <summary>
    /// Stated so the operator can be told, and not written: either §4 carries no
    /// command for it, or the value itself has not been established (§12.4).
    /// </summary>
    SpokenOnly,
}

/// <summary>One condition, and what happened to it.</summary>
/// <param name="Condition">The condition.</param>
/// <param name="Outcome">What happened.</param>
/// <param name="WasText">What the radio said before, or null where unread.</param>
/// <param name="NowText">What it says after, or null where unread.</param>
/// <param name="NowAtUtc">
/// When <paramref name="NowText"/> was read, or null where it was not (work
/// instruction 411: a read-back is stated with its time).
/// </param>
public sealed record ConditionResult(
    ReceiverCondition Condition,
    ConditionOutcome Outcome,
    string? WasText = null,
    string? NowText = null,
    DateTime? NowAtUtc = null);

/// <summary>What Hamlet last set, so it can tell its own hand from the operator's.</summary>
/// <param name="LastSet">
/// Field to the value a tune-in last left it at: confirmed after Hamlet's write, or
/// read already there. On the scale the field is read on.
/// </param>
/// <remarks>
/// <para>**THIS IS HM-DEC-056'S RULE FOR THE MODE, APPLIED TO THE REST OF THE
/// RECEIVE SIDE.** Somebody who reaches over and switches the noise blanker on
/// has said something, and an app that switches it off again the next time he
/// changes frequency is arguing with him about his own radio.</para>
/// <para>**IT REMEMBERS WHAT A TUNE-IN LEFT RIGHT, WRITTEN OR FOUND** (HM-DEC-174,
/// work instruction 419). It used to be a memory of writes only, so a control the
/// first tune-in found already right left nothing for the operator's change to
/// disagree with, and the next tune-in wrote over his hand: Tim set the preamp off
/// and Hamlet turned it back on. A field no tune-in has left right is still not one
/// Hamlet may claim was taken from it, and the first tune-in sets it.</para>
/// <para>**IT HOLDS UNTIL THE BAND CHANGES**, which calls <see cref="Rearmed"/>.</para>
/// </remarks>
public sealed record ReceiverSetupMemory(IReadOnlyDictionary<RigField, int> LastSet)
{
    /// <summary>Nothing set yet.</summary>
    public static ReceiverSetupMemory Empty { get; } =
        new(new Dictionary<RigField, int>());

    /// <summary>Remember a value a tune-in left right.</summary>
    /// <param name="field">What was set.</param>
    /// <param name="value">What it was set to.</param>
    /// <returns>The memory carrying it.</returns>
    public ReceiverSetupMemory Remember(RigField field, int value)
    {
        var next = new Dictionary<RigField, int>(LastSet) { [field] = value };
        return new ReceiverSetupMemory(next);
    }

    /// <summary>
    /// Whether the operator has moved this since Hamlet last set it.
    /// </summary>
    /// <param name="field">The control.</param>
    /// <param name="reading">What the radio says now.</param>
    /// <returns>True where it is his.</returns>
    public bool MovedByHandSince(RigField field, int reading)
        => LastSet.TryGetValue(field, out var mine) && mine != reading;

    /// <summary>The operator changed the mode himself, so nothing is remembered.</summary>
    /// <returns>An empty memory.</returns>
    /// <remarks>
    /// A band change re-arms the mode automation (HM-DEC-056), and the same
    /// reasoning holds here: somebody who suspended it on one band almost
    /// certainly did not mean to switch it off forever.
    /// </remarks>
    public ReceiverSetupMemory Rearmed() => Empty;
}

/// <summary>
/// Where the preamp's overload follow stands between one tune-in and the next (HM-DEC-179).
/// </summary>
/// <param name="Overloading">The last `Overflow` reading counted, or null where none is.</param>
/// <param name="Readings">How many readings in a row have said that.</param>
/// <param name="OffForOverload">Hamlet turned the preamp off for an overload and has not put it back.</param>
/// <param name="SinceRestored">
/// Readings since Hamlet put the band's value back, or null where it has not.
/// </param>
/// <param name="Stopped">
/// Nothing more is followed until the next tune-in: his hand moved it, a write went
/// unconfirmed, or the overload came back once the preamp was back on.
/// </param>
/// <remarks>
/// **IT STARTS AFRESH AT EVERY TUNE-IN**, as the memory's claim on a field does at every
/// band change: somebody who took the preamp over on one block did not mean it forever.
/// </remarks>
public sealed record PreampFollow(
    bool? Overloading, int Readings, bool OffForOverload, int? SinceRestored, bool Stopped)
{
    /// <summary>A follow nothing has happened to yet.</summary>
    public static PreampFollow Fresh { get; } = new(null, 0, false, null, false);
}

/// <summary>What one live reading did to the preamp's follow.</summary>
/// <param name="Follow">Where the follow stands now.</param>
/// <param name="Memory">What Hamlet last set, with any follow write remembered.</param>
/// <param name="Results">
/// The tune-in's results with the preamp's replaced by what the follow found, or the same
/// list where nothing changed.
/// </param>
/// <param name="Said">What to narrate, or "" where nothing happened worth saying.</param>
public sealed record PreampFollowStep(
    PreampFollow Follow,
    ReceiverSetupMemory Memory,
    IReadOnlyList<ConditionResult> Results,
    string Said);

/// <summary>
/// Sets what would otherwise stop the operator hearing the block he has just
/// tuned into, and nothing else.
/// </summary>
/// <remarks>
/// <para>**THE OPERATOR STATES AN INTENT AND THE SETTINGS ARE THE CONSEQUENCE**
/// (HM-DEC-050, HM-DEC-084). There is no row of switches here and there is not
/// going to be one. He says *I want to work FT8 here* by tuning there, and what
/// follows is what has to be true for that to work.</para>
/// <para>**ONCE PER TUNE-IN, THEN HANDS OFF.** No timer, no re-assertion, no
/// fighting the knob. Arriving somewhere new is an explicit act and
/// re-establishing what is needed to hear the block is part of arriving; doing
/// it again two seconds later is an app that will not let go.</para>
/// <para>**WITH ONE EXCEPTION, THE PREAMP ON AN OVERLOAD** (HM-DEC-179,
/// <see cref="FollowOverloadAsync"/>): the radio's manual turns the preamp off while
/// strong signals overload the front end, and a band can start doing that after he
/// has tuned in. Nothing else is followed, and his hand still wins.</para>
/// <para>**AND IT CHANGES ONLY WHAT WOULD GET IN THE WAY.** A control already
/// correct is not written and is not narrated. Setting the whole family every
/// time would override deliberate, skilled choices the operator made for reasons
/// Hamlet cannot see.</para>
/// </remarks>
public static class ReceiverSetup
{
    /// <summary>
    /// Apply the conditions a block states, reading before writing.
    /// </summary>
    /// <param name="rig">The radio.</param>
    /// <param name="conditions">What the block states.</param>
    /// <param name="memory">What Hamlet last set.</param>
    /// <param name="cancellationToken">Cancellation.</param>
    /// <returns>What happened to each, and the memory to carry forward.</returns>
    /// <remarks>
    /// **EVERY CONDITION PRODUCES A RESULT, INCLUDING THE ONES THAT DID
    /// NOTHING.** A control that was already right and one Hamlet could not read
    /// are different facts, and the difference is the whole of §0.0.1: the first
    /// says the radio is fine and the second says nobody knows.
    /// </remarks>
    public static async Task<(IReadOnlyList<ConditionResult> Results, ReceiverSetupMemory Memory)>
        ApplyAsync(
            IRig rig,
            IReadOnlyList<ReceiverCondition> conditions,
            ReceiverSetupMemory memory,
            CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(rig);
        ArgumentNullException.ThrowIfNull(conditions);
        ArgumentNullException.ThrowIfNull(memory);

        var results = new List<ConditionResult>();

        foreach (var condition in conditions)
        {
            if (!condition.CanBeWritten)
            {
                results.Add(new ConditionResult(condition, ConditionOutcome.SpokenOnly));
                continue;
            }

            var field = condition.Field!.Value;
            var wanted = condition.Wanted!.Value;

            // **A RULE IS RESOLVED AGAINST A READING, OR IT IS NOT WRITTEN**
            // (Tim's ruling of 2026-08-29). The attenuator is off unless the
            // front end reads overloading and the preamp follows the frequency
            // and the overload flag (HM-DEC-177); both were wrong in opposite
            // directions on one evening while
            // Hamlet held the reading that decides them. Where the reading is
            // unknown the row is spoken and no byte goes out, because a rule
            // applied without its input is a constant wearing a rule's clothes.
            if (condition.IsConditional)
            {
                var resolved = await ResolveAsync(
                    rig, condition, cancellationToken).ConfigureAwait(false);

                if (resolved is null)
                {
                    results.Add(new ConditionResult(
                        condition, ConditionOutcome.NotRead));
                    continue;
                }

                wanted = resolved.Value;
            }

            var before = (await rig
                .ReadAsync(field, RigState.Empty, cancellationToken)
                .ConfigureAwait(false))
                .FirstOrDefault(v => v.Field == field);

            if (before is not { IsKnown: true, Number: { } reading })
            {
                results.Add(new ConditionResult(condition, ConditionOutcome.NotRead));
                continue;
            }

            var now = (int)reading;

            // **ONE SCALE, THE READ'S** (R65, HM-DEC-172, work instruction 419).
            // The RF gain row asks for 255 and the read says 100 percent; compared
            // raw they never met, so a radio already at full was written on every
            // CW tune-in. The write still carries the row's own value; only the
            // comparisons and the memory are on the scale the radio reports.
            var wantedAsRead = CivDecode.OnReadScale(field, wanted);

            if (now == wantedAsRead)
            {
                // **FOUND RIGHT IS REMEMBERED AS WELL AS SET RIGHT** (HM-DEC-174,
                // work instruction 419). With only writes remembered, a preamp
                // found at 1 left nothing for the operator's own change to disagree
                // with, and the next tune-in wrote his off back to 1.
                memory = memory.Remember(field, wantedAsRead);

                results.Add(new ConditionResult(
                    condition, ConditionOutcome.AlreadyRight, before.Text, before.Text));
                continue;
            }

            if (memory.MovedByHandSince(field, now))
            {
                results.Add(new ConditionResult(
                    condition, ConditionOutcome.LeftToTheOperator, before.Text, before.Text));
                continue;
            }

            var write = CivWrites.All.FirstOrDefault(w => w.Field == field);

            if (write is null)
            {
                // The condition claimed to be writable and the table has no
                // command. That is a defect in the data rather than in the
                // radio, and it says so rather than inventing a byte (§4).
                results.Add(new ConditionResult(condition, ConditionOutcome.SpokenOnly));
                continue;
            }

            var result = await rig
                .SetSettingAsync(write, wanted, cancellationToken)
                .ConfigureAwait(false);

            if (!result.Worked)
            {
                // **A READ-BACK THE RIG TOOK IS CARRIED, NOT DROPPED** (work
                // instruction 411, HM-DEC-170). The outcome is unchanged; what
                // changes is that the sentence can say what the radio said.
                results.Add(new ConditionResult(
                    condition, ConditionOutcome.NotConfirmed, before.Text,
                    result.ReadBack?.Text, result.ReadBack?.AtUtc));
                continue;
            }

            // **THE ACKNOWLEDGEMENT IS NOT THE VALUE** (HM-DEC-084: read before
            // write, read back after, and unknown stays unknown). `FB` is the
            // radio saying it accepted the frame, not that the setting now holds
            // what was asked for. This recorded `condition.WantedText` as the
            // value afterwards, which is the write asserting its own success —
            // exactly the shape §0.0 forbids, on the surface built to prove what
            // Hamlet did.
            var after = (await rig
                .ReadAsync(field, RigState.Empty, cancellationToken)
                .ConfigureAwait(false))
                .FirstOrDefault(v => v.Field == field);

            if (after is not { IsKnown: true, Number: { } settled })
            {
                // The write was acknowledged and the value cannot be read, so
                // what the setting holds is unknown rather than assumed.
                results.Add(new ConditionResult(
                    condition, ConditionOutcome.NotConfirmed, before.Text));
                continue;
            }

            if ((int)settled != wantedAsRead)
            {
                // The radio took the frame and did something else with it, which
                // is a different fact from a refused write and is worth its own
                // line in the record.
                results.Add(new ConditionResult(
                    condition, ConditionOutcome.NotConfirmed, before.Text,
                    after.Text, after.AtUtc));
                continue;
            }

            memory = memory.Remember(field, wantedAsRead);

            results.Add(new ConditionResult(
                condition, ConditionOutcome.Changed, before.Text, after.Text));
        }

        return (results, memory);
    }

    /// <summary>The fields the last tune-in's mode states a condition for.</summary>
    /// <param name="results">What the last tune-in did, or empty where none ran.</param>
    /// <returns>The fields this setup owns until the next tune-in.</returns>
    /// <remarks>
    /// <para>**ONE OWNER PER FIELD** (HM-DEC-174, work instruction 419). A field the
    /// mode states a condition for is decided here, whether the row is written or
    /// only spoken, and no other voice asks the operator to change it: the preamp
    /// was being set by this class, asked for by the advice and objected to by the
    /// observations, all at once.</para>
    /// <para>A row with no field, the scope span, owns nothing, because there is
    /// nothing to change.</para>
    /// </remarks>
    public static IReadOnlySet<RigField> Owns(IEnumerable<ConditionResult> results)
    {
        ArgumentNullException.ThrowIfNull(results);

        return results
            .Select(r => r.Condition.Field)
            .OfType<RigField>()
            .ToHashSet();
    }

    /// <summary>
    /// Consecutive live readings of overloading before the preamp goes off.
    /// </summary>
    /// <remarks>
    /// **FROM THE MEASURED CADENCE** (work instruction 426): the live pass brings a fresh
    /// `Overflow` reading every 252.9 ms median against the scripted radio, plus about
    /// 14 ms of wire a pass on the IC-7300 at 19200 baud, so four readings is about one
    /// second. Long enough that a single loud burst does not flip a switch on the radio,
    /// short enough that he is not left listening to a squashed passband.
    /// </remarks>
    public const int OverloadHoldReadings = 4;

    /// <summary>
    /// Consecutive live readings of no overload before the band's value goes back.
    /// </summary>
    /// <remarks>
    /// Twenty readings, about five seconds at the same cadence: slower back than off,
    /// because an overload that comes and goes costs him the passband each time it
    /// returns. It is also the relapse window: if the overload comes back and holds
    /// within this many readings of the preamp going back on, the preamp goes off and
    /// stays off until the next tune-in.
    /// </remarks>
    public const int ClearHoldReadings = 20;

    /// <summary>
    /// Follow the front end's overload flag with the preamp, between tune-ins (HM-DEC-179).
    /// </summary>
    /// <param name="rig">The radio.</param>
    /// <param name="polled">What the live poll read, fresh: one call per `Overflow` reading.</param>
    /// <param name="lastSetup">What the last tune-in did, as the app holds it.</param>
    /// <param name="memory">What Hamlet last set.</param>
    /// <param name="follow">Where the follow stands.</param>
    /// <param name="cancellationToken">Cancellation.</param>
    /// <returns>Where it stands now, and anything to narrate.</returns>
    /// <remarks>
    /// <para>**THE ONE WRITE HM-DEC-179 LICENSES OUTSIDE A TUNE-IN, AND ITS FIVE
    /// LIMITS.** The preamp field only; triggered only by the polled `Overflow` flag;
    /// only while the last tune-in's block states the preamp with an overload rule;
    /// never while the radio is transmitting, or while it will not say whether it is;
    /// and never after his hand has moved the preamp since Hamlet last set it, which
    /// stops the follow until the next tune-in.</para>
    /// <para>**OFF ON AN OVERLOAD THAT HOLDS, BACK ON ONE THAT HAS CLEARED**, each once:
    /// <see cref="OverloadHoldReadings"/> readings of overloading write off, and
    /// <see cref="ClearHoldReadings"/> of quiet write the band's own value back. An
    /// overload that returns within <see cref="ClearHoldReadings"/> of the preamp going
    /// back on puts it off until the next tune-in, because a preamp that brings the
    /// overload back is the preamp the manual says to leave off.</para>
    /// <para>**READ BEFORE WRITE, READ BACK AFTER** (HM-DEC-084), exactly as at the
    /// tune-in, and a value already right is not rewritten.</para>
    /// </remarks>
    public static async Task<PreampFollowStep> FollowOverloadAsync(
        IRig rig,
        RigState polled,
        IReadOnlyList<ConditionResult> lastSetup,
        ReceiverSetupMemory memory,
        PreampFollow follow,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(rig);
        ArgumentNullException.ThrowIfNull(polled);
        ArgumentNullException.ThrowIfNull(lastSetup);
        ArgumentNullException.ThrowIfNull(memory);
        ArgumentNullException.ThrowIfNull(follow);

        var unchanged = new PreampFollowStep(follow, memory, lastSetup, "");

        if (follow.Stopped)
        {
            return unchanged;
        }

        var index = -1;
        for (var i = 0; i < lastSetup.Count; i++)
        {
            if (lastSetup[i].Condition is { Field: RigField.Preamp, WhenOverloading: not null })
            {
                index = i;
                break;
            }
        }

        // **ONLY WHILE THE BLOCK OWNS IT.** A block that does not state the preamp, or
        // states it without an overload rule, is not one Hamlet follows in.
        if (index < 0)
        {
            return unchanged;
        }

        var tuneIn = lastSetup[index];
        var condition = tuneIn.Condition;

        // **ONLY WHAT THE TUNE-IN LEFT RIGHT.** A preamp the tune-in found in his hand,
        // could not read or could not confirm is not one Hamlet has a value for to follow.
        // A follow write replaces this result with its own, and one that finds his hand
        // or goes unconfirmed stops the follow, so this reads the tune-in's word only.
        if (tuneIn.Outcome is not (ConditionOutcome.AlreadyRight or ConditionOutcome.Changed))
        {
            return unchanged with { Follow = follow with { Stopped = true } };
        }

        // **NEVER WHILE TRANSMITTING**, and a transmit flag nobody has read is not a
        // licence either. The hold starts again once it is receiving.
        if (polled[RigField.TransmitStatus] is not { IsKnown: true, Number: 0 })
        {
            return unchanged with { Follow = follow with { Overloading = null, Readings = 0 } };
        }

        if (polled[RigField.Overflow] is not { IsKnown: true, Number: { } flag })
        {
            return unchanged with { Follow = follow with { Overloading = null, Readings = 0 } };
        }

        var overloading = flag > 0;
        var next = follow with
        {
            Overloading = overloading,
            Readings = follow.Overloading == overloading ? follow.Readings + 1 : 1,
            SinceRestored = follow.SinceRestored + (follow.SinceRestored is null ? 0 : 1),
        };

        if (overloading && !next.OffForOverload && next.Readings >= OverloadHoldReadings)
        {
            var relapse = next.SinceRestored is { } since && since <= ClearHoldReadings;

            return await WriteFollowAsync(
                rig, lastSetup, index, memory, next,
                next with { OffForOverload = true, SinceRestored = null, Stopped = relapse },
                condition.WhenOverloading!.Value,
                relapse
                    ? "I turned the preamp off again because the radio's front end started overloading "
                      + "again once it was back on, and the radio's manual has the preamp off with strong "
                      + "signals. I am leaving it off until you next tune in."
                    : "I turned the preamp off because the radio says its front end is overloading, and "
                      + "the radio's manual has the preamp off with strong signals.",
                cancellationToken).ConfigureAwait(false);
        }

        if (!overloading && next.OffForOverload && next.Readings >= ClearHoldReadings)
        {
            // The band's own value, from the row's stretches at the dial the poll read.
            // A dial the poll did not read, or one in no stretch, is not a licence to guess.
            if (polled[RigField.Frequency] is not { IsKnown: true, Number: { } hz }
                || condition.Bands.FirstOrDefault(b => b.Contains((long)hz)) is not { } stretch)
            {
                return unchanged with { Follow = next };
            }

            return await WriteFollowAsync(
                rig, lastSetup, index, memory, next,
                next with { OffForOverload = false, SinceRestored = 0 },
                stretch.Wanted,
                $"I set the preamp back to preamp {stretch.Wanted} because the radio's front end has "
                + "stopped overloading, and that is what the radio's manual gives for this band.",
                cancellationToken).ConfigureAwait(false);
        }

        return unchanged with { Follow = next };
    }

    // One follow write: read, his hand first, a value already right left alone, write,
    // read back. The preamp's result in the tune-in's list is replaced so every voice
    // that reads the list reads what the radio holds now.
    private static async Task<PreampFollowStep> WriteFollowAsync(
        IRig rig,
        IReadOnlyList<ConditionResult> lastSetup,
        int index,
        ReceiverSetupMemory memory,
        PreampFollow notWritten,
        PreampFollow next,
        int wanted,
        string said,
        CancellationToken cancellationToken)
    {
        var condition = lastSetup[index].Condition;

        IReadOnlyList<ConditionResult> With(ConditionResult result)
        {
            var list = lastSetup.ToList();
            list[index] = result;
            return list;
        }

        var before = (await rig
            .ReadAsync(RigField.Preamp, RigState.Empty, cancellationToken)
            .ConfigureAwait(false))
            .FirstOrDefault(v => v.Field == RigField.Preamp);

        if (before is not { IsKnown: true, Number: { } reading })
        {
            // Not knowing is not a licence to write; the follow tries again on the next
            // reading that calls for it.
            return new PreampFollowStep(notWritten, memory, lastSetup, "");
        }

        var now = (int)reading;

        // **HIS HAND WINS** (HM-DEC-056, HM-DEC-179): moved since Hamlet last set it, so it
        // is his until the next tune-in, and he is told so in the setup's own words.
        if (memory.MovedByHandSince(RigField.Preamp, now))
        {
            var his = new ConditionResult(
                condition, ConditionOutcome.LeftToTheOperator, before.Text, before.Text);

            return new PreampFollowStep(
                notWritten with { Stopped = true }, memory, With(his),
                ReceiverSetupVoice.Say(new[] { his }));
        }

        if (now == wanted)
        {
            return new PreampFollowStep(
                next, memory.Remember(RigField.Preamp, now),
                With(new ConditionResult(condition, ConditionOutcome.AlreadyRight, before.Text, before.Text, before.AtUtc)),
                "");
        }

        var write = CivWrites.All.First(w => w.Field == RigField.Preamp);
        var result = await rig
            .SetSettingAsync(write, wanted, cancellationToken)
            .ConfigureAwait(false);

        var after = result.Worked
            ? (await rig
                .ReadAsync(RigField.Preamp, RigState.Empty, cancellationToken)
                .ConfigureAwait(false))
                .FirstOrDefault(v => v.Field == RigField.Preamp)
            : result.ReadBack;

        if (!result.Worked || after is not { IsKnown: true, Number: { } settled } || (int)settled != wanted)
        {
            // One write, not a retry loop: what the radio holds is said as unconfirmed,
            // and the follow stops until the next tune-in.
            var unconfirmed = new ConditionResult(
                condition, ConditionOutcome.NotConfirmed, before.Text,
                after?.IsKnown == true ? after.Text : null, after?.IsKnown == true ? after.AtUtc : null);

            return new PreampFollowStep(
                next with { Stopped = true }, memory, With(unconfirmed),
                ReceiverSetupVoice.Say(new[] { unconfirmed }));
        }

        return new PreampFollowStep(
            next, memory.Remember(RigField.Preamp, wanted),
            With(new ConditionResult(condition, ConditionOutcome.Changed, before.Text, after.Text, after.AtUtc)),
            said);
    }

    /// <summary>What a conditional row wants right now, or null if it cannot say.</summary>
    /// <param name="rig">The radio.</param>
    /// <param name="condition">The row.</param>
    /// <param name="cancellationToken">Cancellation.</param>
    /// <returns>The value to write, or null where the reading is unknown.</returns>
    /// <remarks>
    /// <para>**`overflow`: the attenuator follows the front end's own flag.** Off
    /// unless the radio says it is overloading, in which case 20 dB. On
    /// 2026-08-29 it sat at 20 dB while a station faded S4 to S1 to nothing, and
    /// later sat off while the front end read overloading at S9 plus 10.</para>
    /// <para>**`band`: the preamp follows the frequency, and the overload flag
    /// before it** (HM-DEC-177, work instruction 424). The stretches and their
    /// values are the row's, from the radio's manual (`IC-7300_ENG_FM_12b` page
    /// 4-3): preamp 1 from 1.8 to 29.999 MHz, preamp 2 at 50 MHz, off while the
    /// front end reads overloading. This replaced a 10 MHz literal that turned the
    /// preamp off across the low bands where Icom specifies it on.</para>
    /// <para>**NULL IS AN ANSWER AND IT MEANS NOTHING IS WRITTEN** (§0.0).</para>
    /// </remarks>
    private static async Task<int?> ResolveAsync(
        IRig rig, ReceiverCondition condition, CancellationToken cancellationToken)
    {
        switch (condition.Condition)
        {
            case "overflow":
            {
                var reading = (await rig
                    .ReadAsync(RigField.Overflow, RigState.Empty, cancellationToken)
                    .ConfigureAwait(false))
                    .FirstOrDefault(v => v.Field == RigField.Overflow);

                if (reading is not { IsKnown: true })
                {
                    return null;
                }

                // 20 dB where the front end says it is overloading, off where it
                // does not. The value is the decibels themselves (§4, `11`).
                return reading.Number is > 0 ? 20 : 0;
            }

            case "band":
            {
                // **OVERLOAD FIRST, WHERE THE ROW SAYS IT DECIDES** (HM-DEC-177). The
                // manual turns the preamp off with strong signals whatever the band,
                // so the flag is read before the dial, and a flag the radio will not
                // give means the rule has no input and nothing is written.
                if (condition.WhenOverloading is { } whenOverloading)
                {
                    var overflow = (await rig
                        .ReadAsync(RigField.Overflow, RigState.Empty, cancellationToken)
                        .ConfigureAwait(false))
                        .FirstOrDefault(v => v.Field == RigField.Overflow);

                    if (overflow is not { IsKnown: true })
                    {
                        return null;
                    }

                    if (overflow.Number is > 0)
                    {
                        return whenOverloading;
                    }
                }

                var reading = (await rig
                    .ReadAsync(RigField.Frequency, RigState.Empty, cancellationToken)
                    .ConfigureAwait(false))
                    .FirstOrDefault(v => v.Field == RigField.Frequency);

                if (reading is not { IsKnown: true, Number: { } hz })
                {
                    return null;
                }

                // A frequency in none of the row's stretches is one the row does not
                // speak for, which is not a licence to write its headline value.
                return condition.Bands.FirstOrDefault(b => b.Contains((long)hz))?.Wanted;
            }

            default:
                // A condition nobody has taught it is not a licence to write the
                // stated constant.
                return null;
        }
    }
}
