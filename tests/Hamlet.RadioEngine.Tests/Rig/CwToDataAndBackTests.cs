using Hamlet.RadioEngine.Bands;
using Hamlet.RadioEngine.Civ;
using Hamlet.RadioEngine.Explore;
using Hamlet.RadioEngine.Rig;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Rig;

/// <summary>
/// Switching between Morse and the digital block leaves the radio correctly set,
/// in both directions, repeatedly.
/// </summary>
/// <remarks>
/// <para>**TASK 1 OF WORK INSTRUCTION 042, AND THE OPERATOR SAID IT PLAINLY: "I
/// do not want to touch the radio."** He was told three times in one afternoon
/// to press buttons on the front of it. This is the half of that which is about
/// the mode.</para>
/// <para>**THE CAUSE WAS NOT WHERE IT LOOKED.** He forced a re-read, the display
/// corrected itself, and that reads as staleness. It was not: the automation
/// remembers the last write it made and declined to write again. Nothing ever
/// cleared that memory, so once USB-D had been established at 14.074 the
/// automation would never establish it there again, however far the radio
/// wandered afterwards. His re-read fixed the display and would not have fixed
/// the next tune-in.</para>
/// <para>**THE RADIO HERE HOLDS ITS OWN STATE** (<see cref="ScriptedRadio"/>).
/// A port replaying a script cannot fail this test, because the script is the
/// answer; a radio that remembers what it was last told can.</para>
/// </remarks>
public sealed class CwToDataAndBackTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the round trips are printed.</param>
    public CwToDataAndBackTests(ITestOutputHelper output) => _output = output;

    private const long Ft8Hz = 14_074_000;

    /// <summary>
    /// The mode-follow loop as <c>FollowTheMapAsync</c> runs it, with a real rig
    /// on the other end.
    /// </summary>
    /// <remarks>
    /// The view model owns the timer, the tab strip and the narration; what it
    /// owns of this is three lines — decide, write, remember — and those three
    /// are here against the same plan and the same rig the application uses.
    /// </remarks>
    private sealed class Follower
    {
        private readonly Ic7300Rig _rig;
        private readonly Neighborhood? _here;

        public Follower(Ic7300Rig rig, Neighborhood? here)
        {
            _rig = rig;
            _here = here;
            State = ModeFollowState.Armed(true);

            _rig.ValuesReported += (_, e) => Ledger = Ledger.With(e.Values.ToArray());
        }

        /// <summary>What Hamlet believes about the radio.</summary>
        public RigState Ledger { get; private set; } = RigState.Empty;

        /// <summary>The automation memory.</summary>
        public ModeFollowState State { get; private set; }

        /// <summary>How many times a write actually went out.</summary>
        public int Writes { get; private set; }

        /// <summary>Take a fresh reading, the way a poll sweep would.</summary>
        public async Task SweepAsync()
        {
            Ledger = Ledger.With(
                (await _rig.ReadAsync(RigField.Mode, Ledger)).ToArray());
        }

        /// <summary>Arrive somewhere: a tab press, or the dial settling.</summary>
        public async Task TuneInAsync()
        {
            var decision = ModeFollowPlan.Decide(
                State, Ledger.Mode, Ledger.DataVariant,
                ModeFollowPlan.TargetFor(_here), Ft8Hz, workingCw: false);

            if (!decision.Write)
            {
                return;
            }

            Writes++;

            var result = await _rig.SetModeAsync(
                decision.Mode, decision.DataMode,
                _here?.PassbandHz is not null ? CivWrites.WidestFilterSlot : null);

            if (result.Worked)
            {
                State = State.Done(Ft8Hz, decision.Mode, decision.DataMode);
            }
        }
    }

    private static Neighborhood Ft8City()
        => NeighborhoodPlan
            .ForBand(HfBands.Bands.First(b => b.Name == "20 m"))
            .First(n => n.Name == "FT8 city");

    private static async Task<(ScriptedRadio Radio, Ic7300Rig Rig)> ConnectAsync()
    {
        var radio = new ScriptedRadio { FrequencyHz = Ft8Hz };
        var rig = new Ic7300Rig(radio);

        Assert.True(await rig.ConnectAsync());
        return (radio, rig);
    }

    /// <remarks>
    /// <para>**THE OPERATOR IN CW AT 14.074, PRESSING THE DIGITAL TAB.** The
    /// acceptance in the order, exactly: he is left in USB-D with a filter wide
    /// enough for the block, and it is **confirmed by readback** rather than by
    /// the acknowledgement to the write.</para>
    /// <para>The readback is what the assertions read, so a Hamlet that folded
    /// its own request into the ledger and called that a measurement would pass
    /// the first three and fail the last: the width is a number no
    /// acknowledgement carries.</para>
    /// </remarks>
    [Fact]
    public async Task ArrivingOnTheDigitalBlockLeavesTheRadioAbleToHearIt()
    {
        var (radio, rig) = await ConnectAsync();
        using var _ = rig;

        var follow = new Follower(rig, Ft8City());
        await follow.SweepAsync();

        _output.WriteLine(
            $"  before: {radio.Mode} data={radio.DataMode} "
            + $"FIL{radio.FilterSlot} {radio.PassbandHz} Hz");

        Assert.Equal(CivMode.Cw, radio.Mode);
        Assert.Equal(ScriptedRadio.NarrowHz, radio.PassbandHz);

        await follow.TuneInAsync();

        _output.WriteLine(
            $"  after:  {radio.Mode} data={radio.DataMode} "
            + $"FIL{radio.FilterSlot} {radio.PassbandHz} Hz");

        // The radio itself.
        Assert.Equal(CivMode.Usb, radio.Mode);
        Assert.True(radio.DataMode);
        Assert.Equal(ScriptedRadio.WideHz, radio.PassbandHz);

        // **AND THE LEDGER AGREES, FROM A READING RATHER THAN FROM THE REQUEST.**
        // The width is the one that proves it: nothing acknowledged a number of
        // hertz, so a ledger that has one has been told it by the radio.
        Assert.Equal(CivMode.Usb, follow.Ledger.Mode);
        Assert.True(follow.Ledger.DataVariant);

        var width = follow.Ledger[RigField.FilterBandwidth];
        _output.WriteLine($"  ledger: {width.Text} via {width.Source}");

        Assert.True(width.IsKnown, "the passband was not read back after the write");
        Assert.Equal(ScriptedRadio.WideHz, width.Number);
        Assert.Contains("1A 03", width.Source, StringComparison.Ordinal);

        // The block can be heard, judged by the map row rather than by a figure
        // written down here.
        Assert.True(Ft8City().PassbandIsWideEnough(width.Number));
    }

    /// <remarks>
    /// <para>**THE DEFECT, AS A TEST.** Hamlet establishes USB-D; the operator
    /// reaches over and turns the mode knob to CW; he presses the Digital tab
    /// again. Before this task, the automation remembered its own earlier write
    /// and did nothing, and he was left in CW under a three-kilohertz block.
    /// </para>
    /// <para>**THE RADIO DOES NOT VOLUNTEER THE KNOB TURN**, because his does
    /// not (HM-DEC-138). So Hamlet learns of it by asking, which is the sweep.
    /// </para>
    /// </remarks>
    [Fact]
    public async Task TheRadioLeavingTheModeHamletSetIsWrittenAgain()
    {
        var (radio, rig) = await ConnectAsync();
        using var _ = rig;

        var follow = new Follower(rig, Ft8City());
        await follow.SweepAsync();
        await follow.TuneInAsync();

        Assert.Equal(CivMode.Usb, radio.Mode);
        Assert.Equal(1, follow.Writes);

        // His hand, on the mode knob, silently.
        radio.OperatorTurnsTheModeKnob(CivMode.Cw, dataMode: false, filterSlot: 2);
        await follow.SweepAsync();

        _output.WriteLine(
            $"  he turns the knob: {radio.Mode} FIL{radio.FilterSlot} "
            + $"{radio.PassbandHz} Hz, ledger says {follow.Ledger.Mode}");

        await follow.TuneInAsync();

        _output.WriteLine(
            $"  presses Digital:   {radio.Mode} data={radio.DataMode} "
            + $"FIL{radio.FilterSlot} {radio.PassbandHz} Hz");

        Assert.Equal(2, follow.Writes);
        Assert.Equal(CivMode.Usb, radio.Mode);
        Assert.True(radio.DataMode);
        Assert.Equal(ScriptedRadio.WideHz, radio.PassbandHz);
    }

    /// <remarks>
    /// <para>**TEN ROUND TRIPS, AND THE ORDER ASKS FOR NO DRIFT.** Each one is
    /// the operator working Morse for a while, then going back to the digital
    /// block: he sets CW himself, and the tune-in has to bring the radio back.
    /// </para>
    /// <para>**AND THE COUNT IS ASSERTED AS WELL AS THE STATE**, because the
    /// two failures available here are opposite ones. Writing nothing leaves him
    /// in CW under an FT8 block, which is the defect. Writing on every tick with
    /// nothing changing is HM-OPEN-041, an evening of eighteen mode writes with
    /// the dial standing still. **Ten arrivals, ten writes, and not an
    /// eleventh.**</para>
    /// </remarks>
    [Fact]
    public async Task TenRoundTripsDoNotDrift()
    {
        var (radio, rig) = await ConnectAsync();
        using var _ = rig;

        var follow = new Follower(rig, Ft8City());

        for (var trip = 1; trip <= 10; trip++)
        {
            // Morse, by his own hand.
            radio.OperatorTurnsTheModeKnob(CivMode.Cw, dataMode: false, filterSlot: 2);
            await follow.SweepAsync();

            // Back to the block.
            await follow.TuneInAsync();

            // And the tab is pressed again without anything having changed,
            // which must cost nothing.
            await follow.TuneInAsync();

            _output.WriteLine(
                $"  trip {trip,2}: {radio.Mode} data={radio.DataMode} "
                + $"FIL{radio.FilterSlot} {radio.PassbandHz} Hz, "
                + $"{follow.Writes} writes so far");

            Assert.Equal(CivMode.Usb, radio.Mode);
            Assert.True(radio.DataMode);
            Assert.Equal(ScriptedRadio.WideHz, radio.PassbandHz);
            Assert.Equal(trip, follow.Writes);
        }

        // The ledger has not drifted away from the radio either.
        Assert.Equal(CivMode.Usb, follow.Ledger.Mode);
        Assert.True(follow.Ledger.DataVariant);
        Assert.Equal(
            ScriptedRadio.WideHz, follow.Ledger[RigField.FilterBandwidth].Number);
    }

    /// <remarks>
    /// <para>**THE GUARD THIS NARROWS IS STILL DOING ITS JOB.** HM-OPEN-041 was
    /// a write loop: a field that reads back unknown looks exactly like a radio
    /// nobody has set, so the automation wrote the same command over and over.
    /// </para>
    /// <para>Here the ledger knows the mode and has never been told the data
    /// flag. Nothing contradicts the memory of the write, so nothing is written
    /// — twenty arrivals, one write.</para>
    /// </remarks>
    [Fact]
    public async Task AnUnreadFlagDoesNotStartTheWriteLoopAgain()
    {
        var (radio, rig) = await ConnectAsync();
        using var _ = rig;

        var follow = new Follower(rig, Ft8City());
        await follow.TuneInAsync();

        Assert.Equal(1, follow.Writes);

        for (var tick = 0; tick < 20; tick++)
        {
            // Command 04 answers the mode and the filter and says nothing about
            // the data variant, which is the reading that used to look like a
            // radio nobody had set.
            await follow.SweepAsync();
            await follow.TuneInAsync();
        }

        _output.WriteLine(
            $"  twenty arrivals later: {follow.Writes} write(s), "
            + $"radio {radio.Mode} data={radio.DataMode}");

        Assert.Equal(1, follow.Writes);
        Assert.Equal(1, radio.ModeWrites);
    }
}
