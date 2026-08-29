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
public sealed record ConditionResult(
    ReceiverCondition Condition,
    ConditionOutcome Outcome,
    string? WasText = null,
    string? NowText = null);

/// <summary>What Hamlet last set, so it can tell its own hand from the operator's.</summary>
/// <param name="LastSet">Field to the value Hamlet last confirmed setting it to.</param>
/// <remarks>
/// <para>**THIS IS HM-DEC-056'S RULE FOR THE MODE, APPLIED TO THE REST OF THE
/// RECEIVE SIDE.** Somebody who reaches over and switches the noise blanker on
/// has said something, and an app that switches it off again the next time he
/// changes frequency is arguing with him about his own radio.</para>
/// <para>**AND IT IS A MEMORY OF WRITES, NOT OF READINGS.** A field Hamlet never
/// set is not one it may claim to have had taken away from it: with no memory
/// there is nothing to disagree with, and the first tune-in sets it.</para>
/// </remarks>
public sealed record ReceiverSetupMemory(IReadOnlyDictionary<RigField, int> LastSet)
{
    /// <summary>Nothing set yet.</summary>
    public static ReceiverSetupMemory Empty { get; } =
        new(new Dictionary<RigField, int>());

    /// <summary>Remember a confirmed write.</summary>
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
            // front end reads overloading and the preamp is off at 40 m and
            // below; both were wrong in opposite directions on one evening while
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

            if (now == wanted)
            {
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
                results.Add(new ConditionResult(
                    condition, ConditionOutcome.NotConfirmed, before.Text));
                continue;
            }

            memory = memory.Remember(field, wanted);

            results.Add(new ConditionResult(
                condition, ConditionOutcome.Changed, before.Text, condition.WantedText));
        }

        return (results, memory);
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
    /// <para>**`band`: the preamp follows the frequency.** Off at 40 m and below,
    /// where the noise arriving at the antenna is already louder than anything
    /// the receiver adds and gain raises both together for nothing; on above,
    /// where the band goes quiet enough that the receiver is the limit. The
    /// boundary is 10 MHz, which is the top of 30 m and the bottom of the range
    /// where that changes.</para>
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
                var reading = (await rig
                    .ReadAsync(RigField.Frequency, RigState.Empty, cancellationToken)
                    .ConfigureAwait(false))
                    .FirstOrDefault(v => v.Field == RigField.Frequency);

                if (reading is not { IsKnown: true, Number: { } hz })
                {
                    return null;
                }

                return hz > 10_000_000 ? 1 : 0;
            }

            default:
                // A condition nobody has taught it is not a licence to write the
                // stated constant.
                return null;
        }
    }
}
