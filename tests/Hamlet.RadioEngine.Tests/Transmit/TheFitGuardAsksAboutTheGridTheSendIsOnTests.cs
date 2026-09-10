using System.Globalization;
using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Licensing;
using Hamlet.RadioEngine.Tests.Rig;
using Hamlet.RadioEngine.Transmit;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Transmit;

/// <summary>
/// **The fit guard follows the grid the transmission is on.** Work instruction
/// 293, task 3 - the arithmetic half of step 4's criterion 3.
/// </summary>
/// <remarks>
/// <para>**THE BREAKAGE THESE CATCH** (`CLAUDE.md`: a unit may not add a test
/// without naming it): **an FT4 transmission that is measured against a
/// fifteen-second slot.** `Ft8TransmitSequence.Sendable` tested
/// <c>Ft8Slots.SlotSeconds</c> in four places, so 7.0 s of an FT4 slot was compared
/// with 12.64 s of FT8 tones and accepted - the transmission would have keyed, run
/// past the boundary into the next slot, been decodable by nobody, and the log
/// would have said `Sent`. **And its mirror image**: an FT4 transmission refused
/// with a sentence that told the operator an FT8 transmission needs 12.64 s, which
/// is the §0.0 fault in the one place he is being told why nothing went out.</para>
/// <para>**NOTHING HERE OPENS A DEVICE OR A PORT** (`SHACK_FACTS.md` FACT-004).
/// <see cref="FakeSerialPort"/> and <see cref="FakeTransmitAudioSink"/>.</para>
/// </remarks>
public sealed class TheFitGuardAsksAboutTheGridTheSendIsOnTests
{
    /// <summary>A boundary in the middle of a minute.</summary>
    private static readonly DateTime Boundary =
        new(2026, 9, 9, 18, 0, 0, DateTimeKind.Utc);

    /// <summary>What the operator asks to send.</summary>
    private const string Message = "W1ABC KC3QIS FN00";

    /// <summary>Where in the slot the application starts a transmission.</summary>
    private const double HalfASecond = 0.5;

    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where every measured figure is printed.</param>
    public TheFitGuardAsksAboutTheGridTheSendIsOnTests(ITestOutputHelper output)
        => _output = output;

    /// <summary>
    /// **FT4 tones fit an FT4 slot, and FT8 tones do not.**
    /// </summary>
    /// <remarks>
    /// **AND THE FIRST OF THOSE HOLDS ON EITHER ANSWER TO THE OPEN QUESTION.** The
    /// 0.5 s offset leaves 7.0 s of a 7.5 s slot; FT4's occupancy is 5.04 s on
    /// upstream's constant and 4.48 s on the other reading, and 7.0 s holds both.
    /// **Nothing here settles that and nothing here needs it settled.**
    /// </remarks>
    [Fact]
    public async Task AnFt4TransmissionFitsAnFt4SlotAndAnFt8OneDoesNot()
    {
        var (sequence, port, sink) = Sequence();

        var ft4 = Ft4Composer.ComposeSignal(Message);
        var ft8 = Ft8Composer.ComposeSignal(Message);

        Assert.True(ft4.Composed, ft4.Explanation);
        Assert.True(ft8.Composed, ft8.Explanation);

        var left = SlotGrid.Ft4.SlotSeconds - HalfASecond;

        _output.WriteLine("FT4 grid         : " + SlotGrid.Ft4.Describe());
        _output.WriteLine("start into slot  : " + HalfASecond + " s");
        _output.WriteLine("which leaves     : " + left.ToString("0.###", CultureInfo.InvariantCulture) + " s");
        _output.WriteLine("FT4 audio        : "
            + ft4.Transmission!.SlotSeconds.ToString("0.###", CultureInfo.InvariantCulture) + " s");
        _output.WriteLine("FT8 audio        : "
            + ft8.Transmission!.SlotSeconds.ToString("0.###", CultureInfo.InvariantCulture) + " s");
        _output.WriteLine("fits on 5.04     : " + (left >= 5.04));
        _output.WriteLine("fits on 4.48     : " + (left >= 4.48));

        var good = await sequence.RunAsync(On(SlotGrid.Ft4, ft4.Transmission!));

        _output.WriteLine(string.Empty);
        _output.WriteLine("FT4 on FT4       : " + good.Outcome + " - \"" + good.Reason + "\"");

        Assert.True(good.AudioWentOut, good.Reason);
        Assert.True(good.Keyed);
        Assert.Equal(ft4.Transmission!.Samples.Length, sink.SamplesHandedOver);

        // ---- AND THE OTHER WAY ROUND, ON A CLEAN PORT --------------------------
        var (second, otherPort, otherSink) = Sequence();
        var bad = await second.RunAsync(On(SlotGrid.Ft4, ft8.Transmission!));

        _output.WriteLine("FT8 on FT4       : " + bad.Outcome + " - \"" + bad.Reason + "\"");
        _output.WriteLine("frames on wire   : " + otherPort.Written.Length);
        _output.WriteLine("times sink played: " + otherSink.TimesCalled);

        Assert.Equal(Ft8TransmitOutcome.RefusedAsUnsendable, bad.Outcome);
        Assert.False(bad.Keyed);

        // **A REFUSAL KEYS NOTHING.** Not a key-on followed by an abort.
        Assert.Empty(otherPort.Written);
        Assert.Equal(0, otherSink.TimesCalled);
        Assert.Null(bad.Abort);

        _output.WriteLine("wire after refusal: " + port.Written.Length
            + " frames on the run that sent, " + otherPort.Written.Length
            + " on the run that refused");
    }

    /// <summary>
    /// **Both refusal sentences name the grid's own numbers, and neither is typed
    /// in.**
    /// </summary>
    /// <remarks>
    /// <para>**IT ASSERTS THAT THE SENTENCE CARRIES WHAT THE GRID SAYS, NOT WHAT
    /// THE GRID SAYS.** The 4.48-against-5.04 figure is the owner's question and
    /// `Ft8Sharp.Ft4Timing.OccupancySeconds` is the one place it lives; a literal
    /// here would make settling it cost two edits instead of one and would let the
    /// two disagree in between.</para>
    /// <para>**AND NEITHER SENTENCE SAYS FT8 ON AN FT4 SEND.** The name travels
    /// with the two numbers on <see cref="SlotGrid"/> for exactly that reason.</para>
    /// </remarks>
    [Fact]
    public async Task NeitherRefusalSentenceNamesTheOtherModesNumbers()
    {
        var (sequence, _, _) = Sequence();

        // The signal alone, started so late in the slot that it will not fit.
        var ft4 = Ft4Composer.ComposeSignal(Message);
        var tooLate = await sequence.RunAsync(
            On(SlotGrid.Ft4, ft4.Transmission!, startSecondsIntoSlot: 3.0));

        // The padded slot, which is what a decoder reads and never what goes out.
        var (second, _, _) = Sequence();
        var padded = Ft4Composer.Compose(Message);
        var paddedRun = await second.RunAsync(On(SlotGrid.Ft4, padded.Transmission!));

        _output.WriteLine("does not fit     : " + tooLate.Reason);
        _output.WriteLine("padded slot      : " + paddedRun.Reason);
        _output.WriteLine(string.Empty);
        _output.WriteLine("the grid says    : " + SlotGrid.Ft4.Describe()
            + ", named " + SlotGrid.Ft4.Name);

        var occupancy =
            SlotGrid.Ft4.TransmissionSeconds.ToString("0.##", CultureInfo.InvariantCulture);

        foreach (var reason in new[] { tooLate.Reason, paddedRun.Reason })
        {
            Assert.Equal(Ft8TransmitOutcome.RefusedAsUnsendable, tooLate.Outcome);
            Assert.Contains(SlotGrid.Ft4.Name, reason, StringComparison.Ordinal);
            Assert.Contains(occupancy, reason, StringComparison.Ordinal);
            Assert.DoesNotContain(SlotGrid.Ft8.Name, reason, StringComparison.Ordinal);
            Assert.DoesNotContain(
                SlotGrid.Ft8.TransmissionSeconds.ToString("0.##", CultureInfo.InvariantCulture),
                reason,
                StringComparison.Ordinal);
        }

        Assert.Equal(Ft8TransmitOutcome.RefusedAsUnsendable, paddedRun.Outcome);
        Assert.Contains("padded slot", paddedRun.Reason, StringComparison.Ordinal);
    }

    /// <summary>
    /// **THE CONTROL: both FT8 refusal sentences are byte-identical to what they
    /// were before this unit.**
    /// </summary>
    /// <remarks>
    /// **THE EXPECTED STRINGS ARE LITERALS ON PURPOSE.** They are what this tree
    /// produced at HEAD `9a81d4d`, character for character. A comparison assembled
    /// from the same constants the code reads would move with the code and would
    /// pin nothing at all.
    /// </remarks>
    [Fact]
    public async Task BothFt8RefusalSentencesAreWhereTheyWereBeforeThisUnit()
    {
        var (sequence, _, _) = Sequence();
        var ft8 = Ft8Composer.ComposeSignal(Message);
        var tooLate = await sequence.RunAsync(
            On(SlotGrid.Ft8, ft8.Transmission!, startSecondsIntoSlot: 3.0));

        var (second, _, _) = Sequence();
        var padded = Ft8Composer.Compose(Message);
        var paddedRun = await second.RunAsync(On(SlotGrid.Ft8, padded.Transmission!));

        _output.WriteLine("does not fit     : " + tooLate.Reason);
        _output.WriteLine("padded slot      : " + paddedRun.Reason);

        Assert.Equal(
            "starting 3 s into the slot leaves 12 s of it, and an FT8 transmission needs "
            + "12.64 s. It would run into the next slot.",
            tooLate.Reason);

        Assert.Equal(
            "this is 15 s of audio and only 14.5 s of the slot is left after 0.5 s. An FT8 "
            + "transmission is 12.64 s of tones with no silence on either end - a padded slot "
            + "is what a decoder reads, not what goes on the air.",
            paddedRun.Reason);
    }

    /// <summary>
    /// **A send that was given no grid is on FT8's, which is what every caller that
    /// existed before this unit got.**
    /// </summary>
    /// <remarks>
    /// The same rule <c>DigitalModes.Grid()</c> already states: anything that is not
    /// FT4 is FT8's grid. **It is the status quo written down, not a guess**, and it
    /// is why no existing send path had to change to keep working.
    /// </remarks>
    [Fact]
    public void ASendWithNoGridStatedIsOnFt8s()
    {
        var ft8 = Ft8Composer.ComposeSignal(Message);

        var send = new OperatorSend(
            ft8.Transmission!, 14_074_000, LicenseClass.General, true, Boundary, HalfASecond);

        _output.WriteLine("grid when none is stated: " + send.Grid.Describe()
            + ", named " + send.Grid.Name);

        Assert.Equal(SlotGrid.Ft8, send.Grid);
    }

    /// <summary>
    /// **`Ft8ArmedSend.Arm` still has exactly one caller in `src/`.**
    /// </summary>
    /// <remarks>
    /// <para>**THE ONE-CLICK RULE, COUNTED RATHER THAN ASSERTED IN PROSE** (§0.2,
    /// and `PHASE_PLAN.md`'s standing ruling). This unit threaded a grid through the
    /// arm and the drive, and **the drive is the line that would be tempting to let
    /// arm something**: it now reads the armed send to find out which grid to
    /// compute a boundary on. It reads it. It cannot write it.</para>
    /// <para>**IT IS COUNTED BY READING THE TREE**, so a second call site appearing
    /// anywhere under `src/` fails this - including one in a file nobody thought to
    /// look at. FT4's slots are half as long and the temptation to continue a
    /// contact is twice as strong, which is why the count is a test rather than a
    /// remark.</para>
    /// </remarks>
    [Fact]
    public void ArmHasExactlyOneCallerInSrc()
    {
        var source = Path.Combine(
            TheUnkeyHappensWhateverGoesWrongTests.RepositoryRoot(), "src");

        var callers = new List<string>();

        foreach (var file in Directory.EnumerateFiles(source, "*.cs", SearchOption.AllDirectories))
        {
            var body = TheUnkeyHappensWhateverGoesWrongTests.CodeOnly(File.ReadAllText(file));
            var lines = body.Split('\n');

            for (var i = 0; i < lines.Length; i++)
            {
                if (lines[i].Contains(".Arm(", StringComparison.Ordinal))
                {
                    callers.Add(
                        Path.GetRelativePath(source, file).Replace('\\', '/')
                        + ":" + (i + 1) + "  " + lines[i].Trim());
                }
            }
        }

        foreach (var caller in callers)
        {
            _output.WriteLine("calls Arm : " + caller);
        }

        _output.WriteLine("callers of Ft8ArmedSend.Arm in src/ : " + callers.Count);

        var only = Assert.Single(callers);

        Assert.Contains("MainWindowViewModel.cs", only, StringComparison.Ordinal);
    }

    // -------------------------------------------------------------------------

    /// <summary>One sequence over a fake wire and a fake card.</summary>
    private static (Ft8TransmitSequence Sequence, FakeSerialPort Port, FakeTransmitAudioSink Sink)
        Sequence()
    {
        var port = new FakeSerialPort();
        var sink = new FakeTransmitAudioSink();

        return (new Ft8TransmitSequence(port, sink), port, sink);
    }

    /// <summary>One send, on a stated grid.</summary>
    private static OperatorSend On(
        SlotGrid grid,
        Ft8Transmission transmission,
        double startSecondsIntoSlot = HalfASecond)
        => new(
            transmission,
            14_080_000,
            LicenseClass.General,
            true,
            Boundary,
            startSecondsIntoSlot)
        {
            Grid = grid,
        };
}
