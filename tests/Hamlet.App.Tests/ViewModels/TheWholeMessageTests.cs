using System;
using System.Collections.Generic;
using System.Linq;
using Hamlet.App.Settings;
using Hamlet.App.ViewModels;
using Hamlet.RadioEngine.Contacts;
using Hamlet.RadioEngine.Psk31;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.ViewModels;

/// <summary>
/// Work instruction 357 task 4: **the whole message, on hover and on a click.**
/// </summary>
/// <remarks>
/// <para>**TIM, 2026-09-14**: *"I should be able to hover and see a whole message, not just
/// cut off."* The message cell is one line in a column as wide as it is, so a station who
/// typed three sentences showed the first few words and there was no way to read the rest.
/// </para>
/// <para>**IT IS THE ROW'S OWN TEXT** (§0.0). No re-wording, no summary and no ellipsis: the
/// characters the demodulator emitted, with the station and the time above them so a box
/// opened an hour later says whose words these were.</para>
/// <para>**COMPUTED, NOT SEEN.** These read view-model strings and a bool; whether the hover
/// draws is `BindingHealthTests`' question.</para>
/// </remarks>
public sealed class TheWholeMessageTests
{
    private const double HisHz = 1234;

    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the text is printed.</param>
    public TheWholeMessageTests(ITestOutputHelper output) => _output = output;

    /// <summary>**A row whose text runs past the column carries all of it, and its station and time.**</summary>
    [Fact]
    public void ARowCarriesItsWholeTextWithTheStationAndTheTime()
    {
        var model = Panel();

        var said =
            "KC3QIS de EI4GNB good evening Tim and thanks for the call, you are 589 here "
            + "in Dublin with a little QSB, the rig is an IC-7300 into a wire up about "
            + "thirty feet, and it has been a quiet evening on the band so far K\n";

        // **A SECOND LINE FROM THE SAME STATION, SO THE FIRST IS A FINISHED MESSAGE.** The
        // splitter cuts on the turnover that follows a callsign (unit 316), and until
        // something arrives after it the text is still pending and the row names no speaker.
        model.ShowPsk31ChannelsForTests(
            new[]
            {
                new Psk31Channel(
                    1, HisHz, 10.0, said + "KC3QIS de EI4GNB back to you Tim K\n"),
            });

        var row = Assert.Single(model.DigitalDecodes);

        _output.WriteLine("cell  : " + row.Unsplit);
        _output.WriteLine("whole : " + row.WholeMessage.Replace("\n", " | ", StringComparison.Ordinal));

        Assert.True(row.HasWholeMessage);

        // **EVERY CHARACTER OF IT**, and the tail is the part a one-line cell loses.
        Assert.Contains("quiet evening on the band so far K", row.WholeMessage, StringComparison.Ordinal);
        Assert.Contains(said.Trim(), row.WholeMessage, StringComparison.Ordinal);

        // **AND WHEN**, always.
        Assert.Contains(row.Utc, row.WholeMessage, StringComparison.Ordinal);

        // **AND WHOSE WORDS THEY WERE, WHERE THE PARSER NAMED SOMEBODY** (§0.0). The head
        // carries the row's own `Sender` and never a callsign read out of the text by this
        // box: a station Hamlet has not named is not named here either. **On this text the
        // parser names nobody**, which is reported in section 4 rather than worked around.
        if (row.Sender.Length > 0)
        {
            Assert.StartsWith(row.Sender, row.WholeMessage, StringComparison.Ordinal);
        }
        else
        {
            Assert.StartsWith(row.Utc, row.WholeMessage, StringComparison.Ordinal);
        }
    }

    /// <summary>**A click opens the box and the X closes it.**</summary>
    [Fact]
    public void AClickOpensTheBoxAndTheXClosesIt()
    {
        var model = Panel();

        model.ShowPsk31ChannelsForTests(
            new[] { new Psk31Channel(1, HisHz, 10.0, "F4DIA DE EI4GNB EI4GNB K\n") });

        var row = Assert.Single(model.DigitalDecodes);

        Assert.False(row.WholeMessageIsOpen);

        row.OpenTheWholeMessageCommand.Execute(null);

        _output.WriteLine("open after the click: " + row.WholeMessageIsOpen);

        Assert.True(row.WholeMessageIsOpen);

        row.CloseTheWholeMessageCommand.Execute(null);

        _output.WriteLine("open after the X    : " + row.WholeMessageIsOpen);

        Assert.False(row.WholeMessageIsOpen);
    }

    /// <summary>**An ended row still carries its whole text and still opens.**</summary>
    /// <remarks>
    /// **UNIT 355 KEPT THE TEXT ON AN ENDED ROW**, and this is what that is for: the station
    /// whose carrier went is the one whose words he most wants to read back.
    /// </remarks>
    [Fact]
    public void AnEndedRowStillCarriesItAndStillOpens()
    {
        var model = Panel();

        model.ShowPsk31ChannelsForTests(
            new[] { new Psk31Channel(1, HisHz, 10.0, "F4DIA DE EI4GNB UR 599 599 K\n") });

        // **HIS CARRIER GOES.**
        model.ShowPsk31ChannelsForTests(Array.Empty<Psk31Channel>());

        var row = Assert.Single(model.DigitalDecodes);

        _output.WriteLine("ended row: " + row.EndedWord + "  [" + row.Unsplit + "]");

        Assert.True(row.HasWholeMessage);
        Assert.Contains("UR 599 599 K", row.WholeMessage, StringComparison.Ordinal);

        row.OpenTheWholeMessageCommand.Execute(null);

        Assert.True(row.WholeMessageIsOpen);
    }

    /// <summary>**A row with nothing read yet offers no box.**</summary>
    /// <remarks>
    /// **A CONTROL THAT LOOKS LIVE AND IS NOT IS THE FAULT §0.5.1 EXISTS FOR.** A held
    /// carrier with a shut squelch has a dimmed row and no words, so there is nothing to
    /// open and nothing offers to.
    /// </remarks>
    [Fact]
    public void ARowWithNothingReadYetOffersNoBox()
    {
        var model = Panel();

        model.ShowPsk31ChannelsForTests(
            new[] { new Psk31Channel(1, HisHz, 10.0, "", Readable: false) });

        var row = Assert.Single(model.DigitalDecodes);

        _output.WriteLine("dimmed row: [" + row.Message + "], whole [" + row.WholeMessage + "]");

        Assert.True(row.HeardNotReadable);
        Assert.False(row.HasWholeMessage);

        // **AND THE COMMAND CANNOT OPEN IT EITHER**, so a stray press does nothing.
        row.OpenTheWholeMessageCommand.Execute(null);

        Assert.False(row.WholeMessageIsOpen);
    }

    private static MainWindowViewModel Panel()
    {
        var settings = new AppSettings { ReconnectOnStartup = false };

        settings.Operator.Callsign = "KC3QIS";
        settings.Operator.GridSquare = "FN00DJ";

        var model = new MainWindowViewModel(settings, null)
        {
            OperatingMode = "Digital",
            DigitalNewestFirst = false,
        };

        model.UseWorkedBeforeForTests(
            new Dictionary<string, AdifContact>(StringComparer.OrdinalIgnoreCase));

        model.ChooseDigitalModeCommand.Execute("PSK31");

        return model;
    }
}
