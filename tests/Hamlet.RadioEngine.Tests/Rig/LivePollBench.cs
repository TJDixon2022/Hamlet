using Hamlet.RadioEngine.Explore;
using Hamlet.RadioEngine.Rig;

namespace Hamlet.RadioEngine.Tests.Rig;

/// <summary>
/// A tune-in followed by the live poll, for work instruction 426: the real
/// <see cref="ReceiverSetup"/> against <see cref="ScriptedRadio"/>, then the radio read the
/// way the live poll reads it and each reading handed to what the app does with a poll.
/// </summary>
/// <remarks>
/// <para>**WHAT THE APP DOES WITH A POLL IS <see cref="OnPollAsync"/>, AND AT HEAD IT IS
/// NOTHING THAT WRITES.** `MainWindowViewModel.ApplyRigState` reads `Overflow` into
/// `FrontEndIsOverloading` (MainWindowViewModel.cs:10983-10987) and nothing acts on it.</para>
/// <para>**EVERY RESULT HERE IS AN INDICATION, NOT A MEASUREMENT OF THE RADIO** (FACT-006).</para>
/// </remarks>
internal static class LivePollBench
{
    /// <summary>One tune-in and the polls after it.</summary>
    internal sealed class Run
    {
        /// <summary>Hamlet's rig.</summary>
        public required Ic7300Rig Rig { get; init; }

        /// <summary>The radio.</summary>
        public required ScriptedRadio Radio { get; init; }

        /// <summary>What the block states.</summary>
        public required IReadOnlyList<ReceiverCondition> Conditions { get; init; }

        /// <summary>What the tune-in did, as the app holds it in `LastReceiverSetup`.</summary>
        public IReadOnlyList<ConditionResult> Results { get; set; } = Array.Empty<ConditionResult>();

        /// <summary>What Hamlet last set.</summary>
        public ReceiverSetupMemory Memory { get; set; } = ReceiverSetupMemory.Empty;

        /// <summary>Everything narrated after the tune-in.</summary>
        public List<string> Narrated { get; } = new();
    }

    /// <summary>
    /// A radio left as an operator might have left it, tuned in with the front end quiet.
    /// </summary>
    /// <param name="hz">The dial.</param>
    /// <returns>The run, with the tune-in's writes still on the radio's record.</returns>
    public static async Task<Run> TuneInQuietAsync(long hz)
    {
        var block = ModeEntryBench.BlockAt(hz);
        var fromBlock = ReceiverConditions.ForBlock(block);
        var conditions = fromBlock.Count > 0 ? fromBlock : ReceiverConditions.ForMode("CW");

        var radio = ModeEntryBench.AsLeft(hz, data: false);
        radio.Transmitting = false;
        var rig = await ModeEntryBench.ConnectAsync(radio);
        var run = new Run { Rig = rig, Radio = radio, Conditions = conditions };

        var (results, memory) = await ReceiverSetup.ApplyAsync(rig, conditions, ReceiverSetupMemory.Empty);
        run.Results = results;
        run.Memory = memory;
        return run;
    }

    /// <summary>What the app does with one poll.</summary>
    /// <param name="run">The run.</param>
    /// <param name="polled">What the poll read.</param>
    /// <returns>When it is done.</returns>
    public static Task OnPollAsync(Run run, RigState polled)
    {
        _ = run;
        _ = polled;
        return Task.CompletedTask;
    }

    /// <summary>One live poll: the radio read, then handed to <see cref="OnPollAsync"/>.</summary>
    /// <param name="run">The run.</param>
    /// <returns>What the poll read.</returns>
    public static async Task<RigState> PollAsync(Run run)
    {
        var state = await ModeEntryBench.ReadAllAsync(run.Rig);
        state = state.With(await run.Rig.ReadAsync(RigField.TransmitStatus, state));
        await OnPollAsync(run, state);
        return state;
    }

    /// <summary>Several polls in a row.</summary>
    /// <param name="run">The run.</param>
    /// <param name="count">How many.</param>
    /// <returns>What the last one read.</returns>
    public static async Task<RigState> PollAsync(Run run, int count)
    {
        var state = RigState.Empty;
        for (var i = 0; i < count; i++)
        {
            state = await PollAsync(run);
        }

        return state;
    }

    /// <summary>The preamp writes the radio has taken, in order.</summary>
    /// <param name="radio">The radio.</param>
    /// <returns>The values written.</returns>
    public static IReadOnlyList<int> PreampWrites(ScriptedRadio radio)
        => ModeEntryBench.Writes(radio).Where(w => w.Field == RigField.Preamp).Select(w => w.Value).ToList();
}
