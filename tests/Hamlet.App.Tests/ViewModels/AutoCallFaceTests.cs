using Hamlet.App.ViewModels;
using Hamlet.RadioEngine.Cw;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.ViewModels;

/// <summary>
/// What the calling cycle puts on screen, and what it refuses (phase 4, §0.2).
/// </summary>
/// <remarks>
/// <para>**NOTHING HERE IS CONNECTED TO A RADIO, WHICH IS WHY IT CAN BE A TEST.**
/// Most of this phase is verified at the screen into a dummy load, which is
/// HM-DEC-098's own requirement and Tim's to do. What can be proved by test is
/// the words and the refusals: whether arming is a separate act from starting,
/// whether the facts consent is given against are shown, and whether the two
/// things that move and key the radio refuse to run together.</para>
/// </remarks>
public sealed class AutoCallFaceTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the lines are printed.</param>
    public AutoCallFaceTests(ITestOutputHelper output) => _output = output;

    private const string Call = "CQ CQ DE W1AW W1AW K";

    /// <remarks>
    /// <para>**ARM IS A DISTINCT STEP FROM START AND STARTING IS NOT OFFERED
    /// UNTIL IT HAS HAPPENED.** What the operator consents to is a transmission
    /// repeating under his callsign while he may not be watching, and consent
    /// given by pressing a button whose state he is inferring is not consent.
    /// </para>
    /// </remarks>
    [Fact]
    public void StartingIsNotOfferedUntilItHasBeenArmed()
    {
        var auto = new AutoCallViewModel(_ => { }) { Message = Call };

        // No radio, so nothing may be armed either — but the point here is the
        // order: start is never offered ahead of arm.
        Assert.False(auto.CanStart);
        Assert.False(auto.IsArmed);

        _output.WriteLine($"armed {auto.IsArmed}, can start {auto.CanStart}");
    }

    /// <remarks>
    /// <para>**THE FACTS CONSENT IS GIVEN AGAINST** (phase 4). It says what will
    /// go out and how often, and where it does not know something it says that
    /// rather than leaving a gap the operator would fill in himself.</para>
    /// </remarks>
    [Fact]
    public void ItSaysWhatWillGoOutAndHowOftenBeforeAnythingIsArmed()
    {
        var auto = new AutoCallViewModel(_ => { })
        {
            Message = Call,
            IntervalSeconds = 30,
            MaxRounds = 10,
        };

        _output.WriteLine(auto.WillSendLine);
        _output.WriteLine(auto.RoundsLine);
        _output.WriteLine(auto.BreakInLine);
        _output.WriteLine(auto.PowerLine);
        _output.WriteLine(auto.ReadyLine);

        Assert.Contains(Call, auto.WillSendLine, StringComparison.Ordinal);
        Assert.Contains("10 times", auto.RoundsLine, StringComparison.Ordinal);
        Assert.Contains("30 seconds", auto.RoundsLine, StringComparison.Ordinal);

        // **WHAT IT HAS NOT READ, IT SAYS IT HAS NOT READ** (§0.0). Nothing is
        // connected, so the frequency, break-in and the power are all unknown and
        // each says so in its own words rather than showing a zero.
        Assert.Contains("has not read", auto.WillSendLine, StringComparison.Ordinal);
        Assert.Contains("not read break-in", auto.BreakInLine, StringComparison.Ordinal);
        Assert.Contains("has not read", auto.PowerLine, StringComparison.Ordinal);
    }

    /// <remarks>
    /// <para>**BREAK-IN OFF IS SAID BEFORE ARMING, NOT DISCOVERED AFTER.** With
    /// it off, command 17 gives a correct frame, a correct acknowledgement and
    /// total silence, which looks exactly like success (Full Manual p. 19-7,
    /// footnote 2). Nothing connected is a third state again, and all three read
    /// differently.</para>
    /// </remarks>
    [Fact]
    public void BreakInIsSaidRatherThanAssumed()
    {
        var auto = new AutoCallViewModel(_ => { });

        _output.WriteLine(auto.BreakInLine);

        Assert.Contains("Nothing is connected", auto.BreakInLine, StringComparison.Ordinal);
    }

    /// <remarks>
    /// <para>**A MESSAGE THE KEYER CANNOT TAKE IS REFUSED WHERE HE CAN SEE IT.**
    /// The refusal is on the panel beside the box, so a message too long fails
    /// while he is typing it rather than arriving on the air cut short.</para>
    /// </remarks>
    [Fact]
    public void AMessageTooLongIsRefusedOnThePanel()
    {
        var auto = new AutoCallViewModel(_ => { })
        {
            Message = "CQ CQ CQ DE KC3QIS KC3QIS KC3QIS PSE K",
        };

        _output.WriteLine(auto.MessageRefusal);

        Assert.True(auto.HasMessageRefusal);
        Assert.False(auto.CanArm);
        Assert.Contains("30", auto.MessageRefusal, StringComparison.Ordinal);
    }

    /// <remarks>
    /// **AND AN EMPTY MESSAGE IS REFUSED RATHER THAN FILLED IN.** No session may
    /// invent the content of a transmission that goes out under his callsign, so
    /// there is no default and the refusal says as much.
    /// </remarks>
    [Fact]
    public void AnEmptyMessageIsRefusedAndNothingIsWrittenForHim()
    {
        var auto = new AutoCallViewModel(_ => { });

        _output.WriteLine(auto.MessageRefusal);

        Assert.True(auto.HasMessageRefusal);
        Assert.Contains("does not write one for you", auto.MessageRefusal,
            StringComparison.Ordinal);
    }

    /// <remarks>
    /// <para>**MUTUALLY EXCLUSIVE WITH THE SCANNER, REFUSED AT ARM AND SAID OUT
    /// LOUD** (HM-DEC-098). The scanner moves the dial and this transmits on it,
    /// so running both means transmitting mid-tune on a frequency neither
    /// component believes it is on. The refusal names the reason rather than
    /// leaving a control greyed out (HM-DEC-087).</para>
    /// </remarks>
    [Fact]
    public void ArmingIsRefusedWhileTheScannerHasTheDial()
    {
        var said = "";
        var auto = new AutoCallViewModel(line => said = line, () => true)
        {
            Message = Call,
        };

        auto.ArmCommand.Execute(null);

        _output.WriteLine($"refusal: {auto.Refusal}");
        _output.WriteLine($"said: {said}");

        Assert.False(auto.IsArmed);
        Assert.Contains("scanner", auto.Refusal, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("scanner", said, StringComparison.OrdinalIgnoreCase);
    }

    /// <remarks>
    /// <para>**AND THE OTHER DIRECTION, WHICH IS THE HALF THAT IS EASY TO
    /// FORGET.** Refusing in only one direction leaves whichever the operator
    /// pressed second to win, which is the concurrency the ruling forbids.</para>
    /// </remarks>
    [Fact]
    public async Task TheScannerIsRefusedWhileACycleIsTransmitting()
    {
        var said = "";
        var scan = new ScanViewModel(
            line => said = line, transmitting: () => true);

        await scan.StartCommand.ExecuteAsync(null);

        _output.WriteLine($"refusal: {scan.Refusal}");

        Assert.False(scan.IsScanning);
        Assert.Contains("transmitting", scan.Refusal, StringComparison.OrdinalIgnoreCase);
    }

    /// <remarks>
    /// **THE STOP IS SAFE WITH NOTHING RUNNING AND NO RADIO** (§0.2). It is
    /// wired to Escape and to a control in the pinned strip, both of which exist
    /// before any cycle does, so pressing either while nothing is happening has to
    /// be a call that returns rather than a crash.
    /// </remarks>
    [Fact]
    public void TheStopIsSafeWithNothingRunning()
    {
        var auto = new AutoCallViewModel(_ => { });

        auto.StopNow();
        auto.StopCommand.Execute(null);
        auto.StopNow();

        Assert.False(auto.IsCalling);
        Assert.False(auto.IsArmed);
    }

    /// <remarks>
    /// **DISARMING TAKES THE CONSENT BACK.** An armed cycle that could not be
    /// disarmed without starting it would make arming the point of no return,
    /// which is the opposite of what a separate arming step is for.
    /// </remarks>
    [Fact]
    public void DisarmingTakesTheConsentBack()
    {
        var auto = new AutoCallViewModel(_ => { }) { Message = Call };

        auto.DisarmCommand.Execute(null);

        Assert.False(auto.IsArmed);
        Assert.False(auto.CanStart);
    }

    /// <remarks>
    /// Proves §0.5: **a collapsed panel still carries its news.** A shut panel
    /// that went silent about a transmitter running is the prime directive broken
    /// by omission, and this is the one panel where that matters most.
    /// </remarks>
    [Fact]
    public void ACollapsedPanelStillSaysWhetherItIsTransmitting()
    {
        var auto = new AutoCallViewModel(_ => { }) { Message = Call };

        _output.WriteLine($"idle: {auto.Summary}");

        Assert.Equal("not armed", auto.Summary);
    }

    /// <remarks>
    /// Proves the defaults reach the screen as the ruling set them: thirty
    /// seconds a round and ten rounds, which is about five minutes of calling.
    /// </remarks>
    [Fact]
    public void TheRuledDefaultsAreWhatThePanelOpensWith()
    {
        var auto = new AutoCallViewModel(_ => { });

        Assert.Equal(30, auto.IntervalSeconds);
        Assert.Equal(10, auto.MaxRounds);

        _output.WriteLine(auto.RoundsLine);

        Assert.Contains("5 minutes", auto.RoundsLine, StringComparison.Ordinal);
    }

    /// <remarks>
    /// <para>**THE MESSAGE IS NEVER PRE-FILLED, AND THIS IS THE CHECK THAT SAYS
    /// SO.** A default here would be Hamlet putting a callsign on the air that
    /// nobody typed, and it is the one thing in this feature that could not be
    /// undone afterwards.</para>
    /// </remarks>
    [Fact]
    public void ThePanelOpensWithNoMessageAtAll()
    {
        var auto = new AutoCallViewModel(_ => { });

        Assert.Equal("", auto.Message);
        Assert.Equal("", CwMessage.Clean(auto.Message));
    }
}
