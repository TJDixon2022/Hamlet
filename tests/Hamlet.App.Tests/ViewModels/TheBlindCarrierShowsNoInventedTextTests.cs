using System;
using System.Collections.Generic;
using System.Linq;
using Hamlet.App.Settings;
using Hamlet.App.ViewModels;
using Hamlet.RadioEngine.Contacts;
using Hamlet.RadioEngine.Olivia;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.ViewModels;

/// <summary>
/// **Criterion 10.4: a carrier the blind search found shows characters only from blocks the
/// decoder is confident about** - R46(d) and R9, work instruction 387 task 5.
/// </summary>
/// <remarks>
/// <para>**THE FAULT THIS IS FOR.** Tim's screen on 2026-09-22 carried a row on 14.072 at
/// 13:37 UTC reading `4/500 sending Hk7DYYYzfzYXTDYY...`, **which is Hamlet telling him a station
/// sent characters that no station sent.**</para>
/// <para>**THE ROW STAYS AND NOTHING IS INVENTED** (§0.0, and work instruction 387 section 10).
/// What changes is what the row says: `heard, not readable yet`, the sentence that has been at
/// `MainWindowViewModel.HeardNotReadableYet` since work instruction 324 and which
/// `Psk31CarrierSearch` §0.0 and R9 already name. **No row is hidden and no character is
/// invented.**</para>
/// <para>**NOTHING IS KEYED AND NO PORT IS OPENED** (FACT-004). Every channel here is a record
/// handed to the view model in memory.</para>
/// </remarks>
public sealed class TheBlindCarrierShowsNoInventedTextTests
{
    /// <summary>Tim's row, character for character, as his screen carried it.</summary>
    private const string WhatNobodySent = "Hk7DYYYzfzYXTDYY";

    /// <summary>The station on the rows that are genuinely being read.</summary>
    private const string His = "W1AW";

    private readonly ITestOutputHelper _output;

    /// <summary>Creates the guard.</summary>
    /// <param name="output">Where every row is printed.</param>
    public TheBlindCarrierShowsNoInventedTextTests(ITestOutputHelper output) => _output = output;

    /// <summary>
    /// **The 13:37 UTC shape, replayed: the 4/500 row shows no text at all.**
    /// </summary>
    /// <remarks>
    /// One block through and eleven thrown away is what a wrong row does, and it is what the
    /// blind search's own remark at `OliviaBlindSearch.ConfirmBlocks` warns about in those words.
    /// </remarks>
    [Fact]
    public void TheThirteenThirtySevenRowShowsNoTextAndSaysWhatItIs()
    {
        var model = Panel();

        model.ShowOliviaChannelsForTests(new[]
        {
            new OliviaChannel(1, "4/500", 1500, OliviaListener.FoundBlind, 0, WhatNobodySent, 1, 11, false),
        });

        var row = Assert.Single(model.DigitalDecodes, r => r.IsTextOnly);

        _output.WriteLine("the 4/500 row reads: [" + row.Message + "], variant " + row.Variant);

        // **NOT ONE OF THE CHARACTERS NOBODY SENT.**
        Assert.DoesNotContain(WhatNobodySent, row.Message, StringComparison.Ordinal);

        // **AND IT SAYS WHAT IT IS, RATHER THAN BEING BLANK OR BEING GONE** (§0.0).
        Assert.Equal(MainWindowViewModel.HeardNotReadableYet, row.Message);
        Assert.True(row.HeardNotReadable);
        Assert.Equal("4/500", row.Variant);
        Assert.Equal("1500", row.Hz);
    }

    /// <summary>
    /// **A blind-found carrier that is genuinely being read shows every character it read.**
    /// </summary>
    /// <remarks>
    /// This is the half that stops the gate from being a way of showing nothing: a carrier whose
    /// blocks mostly decode is a station, and Hamlet says what it said.
    /// </remarks>
    [Fact]
    public void ABlindFoundCarrierThatIsBeingReadShowsWhatItRead()
    {
        var model = Panel();
        var said = "CQ CQ CQ de " + His + " " + His + " pse K\n";

        model.ShowOliviaChannelsForTests(new[]
        {
            new OliviaChannel(1, "8/250", 1000, OliviaListener.FoundBlind, 0, said, 9, 1, false),
        });

        var row = Assert.Single(model.DigitalDecodes, r => r.IsTextOnly);

        _output.WriteLine("the 8/250 row reads: [" + Short(row.Message) + "]");

        Assert.Contains(His, row.Message, StringComparison.Ordinal);
        Assert.False(row.HeardNotReadable);
    }

    /// <summary>
    /// **A station that announced itself with an RSID is not gated at all** - it said which
    /// variant it is, so there is nothing to be unsure about.
    /// </summary>
    /// <remarks>
    /// R46(d) names the blind search, and a supersession wins exactly where it speaks. The same
    /// one-block-in-twelve channel that is withheld when the search found it is shown when the
    /// station announced it, which is the whole of the difference.
    /// </remarks>
    [Fact]
    public void AnRsidAnnouncedChannelIsNotGatedAndShowsWhatItReadOnTheFirstBlock()
    {
        var model = Panel();

        model.ShowOliviaChannelsForTests(new[]
        {
            new OliviaChannel(1, "4/500", 1500, OliviaListener.FoundByRsid, 0, "CQ de " + His, 1, 11, false),
        });

        var row = Assert.Single(model.DigitalDecodes, r => r.IsTextOnly);

        _output.WriteLine("the RSID-announced 4/500 row reads: [" + Short(row.Message) + "]");

        Assert.Contains(His, row.Message, StringComparison.Ordinal);
        Assert.False(row.HeardNotReadable);
    }

    /// <summary>
    /// **What was read is never taken back off the screen**, so the gate is one-way.
    /// </summary>
    /// <remarks>
    /// A fade that makes the next blocks fail must not turn a sentence back into
    /// `heard, not readable yet`; that rule is already this row path's and the gate keeps it.
    /// </remarks>
    [Fact]
    public void OnceItHasBeenConfidentAFadeDoesNotTakeTheWordsBack()
    {
        var model = Panel();
        var said = "CQ CQ de " + His + " K\n";

        // First: confident, and the words are on the screen.
        model.ShowOliviaChannelsForTests(new[]
        {
            new OliviaChannel(1, "8/250", 1000, OliviaListener.FoundBlind, 0, said, 6, 1, false),
        });

        Assert.Contains(
            His,
            model.DigitalDecodes.Single(r => r.IsTextOnly).Message,
            StringComparison.Ordinal);

        // Then a fade: the same channel, now refusing far more blocks than it reads.
        model.ShowOliviaChannelsForTests(new[]
        {
            new OliviaChannel(1, "8/250", 1000, OliviaListener.FoundBlind, 0, said, 6, 40, false),
        });

        var row = model.DigitalDecodes.Single(r => r.IsTextOnly);

        _output.WriteLine("after the fade the row still reads: [" + Short(row.Message) + "]");

        Assert.Contains(His, row.Message, StringComparison.Ordinal);
        Assert.False(row.HeardNotReadable);
    }

    /// <summary>
    /// **The gate is the engine's own numbers and the boundary is asserted, not assumed.**
    /// </summary>
    /// <remarks>
    /// One block is refused because <c>OliviaBlindSearch.ConfirmBlocks</c> is 2 and its own
    /// remark says why it is not one; two blocks against two rejected is shown, because more read
    /// than refused is the rule and equal is not fewer.
    /// </remarks>
    [Theory]
    [InlineData(1, 0, false, "one block is not a reading, whatever the rest did")]
    [InlineData(1, 11, false, "Tim's own row")]
    [InlineData(2, 3, false, "more thrown away than read")]
    [InlineData(2, 2, true, "as many read as refused, which is not fewer")]
    [InlineData(2, 0, true, "two clean blocks and nothing refused")]
    [InlineData(12, 11, true, "a busy carrier that is mostly being read")]
    public void TheBoundaryIsWhereTheEnginesOwnNumbersPutIt(
        int decoded, int rejected, bool shown, string why)
    {
        var model = Panel();
        var said = "de " + His + " K\n";

        model.ShowOliviaChannelsForTests(new[]
        {
            new OliviaChannel(1, "8/250", 1000, OliviaListener.FoundBlind, 0, said, decoded, rejected, false),
        });

        var row = model.DigitalDecodes.Single(r => r.IsTextOnly);

        _output.WriteLine(
            decoded + " read, " + rejected + " refused -> ["
            + Short(row.Message) + "]   (" + why + ")");

        Assert.Equal(shown, row.Message.Contains(His, StringComparison.Ordinal));
        Assert.Equal(!shown, row.HeardNotReadable);

        // **NOTHING IS EVER PARTLY SHOWN.** A row either carries what the channel read or it
        // carries the sentence; a half-built gate that shows text sometimes is the thing work
        // instruction 387 task 5 says is not acceptable.
        Assert.True(
            row.Message == MainWindowViewModel.HeardNotReadableYet
            || row.Message.Contains(said.Trim(), StringComparison.Ordinal),
            "the row reads [" + row.Message + "], which is neither the sentence nor what the "
            + "channel read.");
    }

    // -------------------------------------------------------------------------------------

    private static MainWindowViewModel Panel()
    {
        var settings = new AppSettings { ReconnectOnStartup = false };

        settings.Operator.Callsign = "KC3QIS";
        settings.Operator.GridSquare = "FN00DJ";
        settings.Operator.OperatorName = "Tim";
        settings.Operator.Location = "Pennsylvania";
        settings.Operator.LicenseClass = Hamlet.RadioEngine.Licensing.LicenseClass.General;

        var model = new MainWindowViewModel(settings, null)
        {
            OperatingMode = "Digital",
            DigitalNewestFirst = false,
        };

        model.UseWorkedBeforeForTests(new Dictionary<string, AdifContact>(StringComparer.OrdinalIgnoreCase));
        model.ChooseDigitalModeCommand.Execute("Olivia");

        return model;
    }

    private static string Short(string? text)
    {
        var one = (text ?? "").Replace("\n", " / ", StringComparison.Ordinal).Trim();

        return one.Length <= 60 ? one : one[..57] + "...";
    }
}
