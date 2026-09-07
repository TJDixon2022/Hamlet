using Hamlet.App.Settings;
using Hamlet.App.ViewModels;
using Hamlet.RadioEngine.Contacts;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.ViewModels;

/// <summary>
/// **The click itself: Hamlet refuses to send what nobody can read, and says so
/// in the Send area** (work instruction 272, task 2).
/// </summary>
/// <remarks>
/// <para>**THIS FILE IS BUILT AND NOT RUN.** Work instruction 272 forbids running
/// <c>Hamlet.App.Tests</c> - the rule is Tim's ruling of 2026-09-05 about what a
/// unit may run and it is not this unit's to weigh - so the gate is measured on
/// the real armed send, the real sequence and the real wire by
/// <c>Hamlet.RadioEngine.Tests/Transmit/HamletDoesNotKeyWhatNobodyCanReadTests</c>,
/// which may be run and was. What this file adds is the join: that the refusal is
/// reached from <c>SendMessageCommand.Execute</c>, which is where the operator's
/// click lands.</para>
/// <para>**IT NEVER OPENS A WINDOW, A PORT OR A DEVICE.** No radio has ever been
/// attached to this machine (SHACK_FACTS.md FACT-004), so the ordinary state of
/// this command is refusal - and what is asserted here is **which** refusal, and
/// that the read-back one comes first, because it is true whether or not a radio
/// is connected.</para>
/// </remarks>
public sealed class TheSendPathRefusesWhatNobodyCanReadTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the Send area line is quoted.</param>
    public TheSendPathRefusesWhatNobodyCanReadTests(ITestOutputHelper output)
        => _output = output;

    /// <summary>
    /// **The exact string that went out on a live antenna, clicked: nothing is
    /// armed and the Send area says what the encoder made of it.**
    /// </summary>
    [Fact]
    public void TheStringThatWentOutOnALiveAntennaIsRefusedFromTheClick()
    {
        var panel = Panel();

        panel.SendMessageCommand.Execute("VP2MAA KC3QIS FN00DJ");

        _output.WriteLine(panel.DigitalSendLine);

        Assert.StartsWith(
            "Hamlet composed \"VP2MAA KC3QIS FN00DJ\" and sent nothing: ",
            panel.DigitalSendLine, StringComparison.Ordinal);

        // WHAT THE ENCODER MADE OF IT, quoted to him.
        Assert.Contains(
            "<VP2MAA KC3QIS> FN00DJ", panel.DigitalSendLine, StringComparison.Ordinal);

        // AND IT IS THE READ-BACK REFUSAL, NOT THE NO-RADIO ONE. The message is
        // unreadable whether or not a radio is connected, so it is answered first.
        Assert.DoesNotContain(
            "no radio is connected", panel.DigitalSendLine, StringComparison.Ordinal);
    }

    /// <summary>
    /// **A compound prefix from the right-click menu is refused the same way.**
    /// </summary>
    [Theory]
    [InlineData("VP2M/K1ABC KC3QIS FN00", "<VP2M/K1ABC> KC3QIS FN00")]
    [InlineData("SV9/PA3EXX KC3QIS -12", "<SV9/PA3EXX> KC3QIS -12")]
    public void ACompoundPrefixIsRefusedFromTheClick(string clicked, string encodedAs)
    {
        var panel = Panel();

        panel.SendMessageCommand.Execute(clicked);

        _output.WriteLine(panel.DigitalSendLine);

        Assert.StartsWith(
            "Hamlet composed \"" + clicked + "\" and sent nothing: ",
            panel.DigitalSendLine, StringComparison.Ordinal);
        Assert.Contains(encodedAs, panel.DigitalSendLine, StringComparison.Ordinal);
    }

    /// <summary>
    /// **A good message goes past the guard and stops for the ordinary reason** -
    /// no radio on this machine - which is how the gate is shown to be a gate and
    /// not a dead path.
    /// </summary>
    [Theory]
    [InlineData("CQ KC3QIS FN00")]
    [InlineData("VP2MAA KC3QIS FN00")]
    [InlineData("K1ABC/P KC3QIS -12")]
    public void AMessageThatReadsBackAsItselfGoesPastTheGuard(string clicked)
    {
        var panel = Panel();

        panel.SendMessageCommand.Execute(clicked);

        _output.WriteLine(panel.DigitalSendLine);

        Assert.Contains(clicked, panel.DigitalSendLine, StringComparison.Ordinal);
        Assert.Contains(
            "no radio is connected", panel.DigitalSendLine, StringComparison.Ordinal);
        Assert.DoesNotContain("hash", panel.DigitalSendLine, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// **The menu toward a station every message is refused for still offers all
    /// five.** Nothing is forbidden in the menu.
    /// </summary>
    [Fact]
    public void TheMenuIsUntouchedByTheGuard()
    {
        var panel = Panel();
        var settings = new AppSettings();

        var ledger = new Ft8ContactLedger("KC3QIS");
        ledger.RecordHeard(
            "CQ VP2M/K1ABC FK52", new DateTime(2026, 9, 7, 17, 12, 30, DateTimeKind.Utc));

        var record = ledger.For("VP2M/K1ABC");

        Assert.NotNull(record);

        var menu = Ft8SendOptions.For(record!, "KC3QIS", "FN00DJ", -12);

        foreach (var option in menu.Options)
        {
            _output.WriteLine(option.Label + " | " + option.Text);
        }

        Assert.Equal(5, menu.Options.Count);
        Assert.NotNull(panel);
        Assert.NotNull(settings);
    }

    private static MainWindowViewModel Panel()
    {
        var settings = new AppSettings();

        settings.Operator.Callsign = "KC3QIS";
        settings.Operator.GridSquare = "FN00DJ";

        return new MainWindowViewModel(settings, null);
    }
}
