using System;
using System.Collections.Generic;
using System.Linq;
using Hamlet.App.Settings;
using Hamlet.App.ViewModels;
using Hamlet.RadioEngine.Contacts;
using Hamlet.RadioEngine.Licensing;
using Hamlet.RadioEngine.Psk31;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.ViewModels;

/// <summary>
/// Work instruction 371 task 2: **a row that spoke to you is never hidden by the CQ filter.**
/// </summary>
/// <remarks>
/// <para>**THE FILTER HIDES ROWS THAT ARE NEITHER A CQ NOR FOR HIM.** On 2026-09-20 a station
/// answered Tim's CQ, was on the screen for seventeen seconds, stopped, and went - his row had
/// ended, and an ended row that is not a CQ was not on the list he was looking at.</para>
/// <para>**CERTAIN OR NOT, LIVE OR ENDED.** A guess about who spoke is still a station speaking
/// to him, and the row carries the words it already carries (§0.0).</para>
/// <para>**COMPUTED, NOT SEEN**, and nothing here is evidence about the radio (FACT-004).</para>
/// </remarks>
public sealed class TheRowForYouIsNeverHiddenTests
{
    private const string Mine = "KC3QIS";
    private const string Him = "W1AW";
    private const string Somebody = "K9XP";

    /// <summary>His answer to the operator, uncertain because a symbol is welded into the report.</summary>
    private const string ToTheOperator = Mine + " de " + Him + " GM TIM UR 5#9 BTU " + Mine + " de " + Him + " K\n";

    /// <summary>The same shape, addressed to somebody else entirely.</summary>
    private const string ToSomebodyElse = Somebody + " de " + Him + " GM UR 5#9 BTU " + Somebody + " de " + Him + " K\n";

    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the lists are printed.</param>
    public TheRowForYouIsNeverHiddenTests(ITestOutputHelper output) => _output = output;

    /// <summary>**With the CQ filter on, his row is shown live and after his carrier goes.**</summary>
    /// <param name="ended">Whether his carrier has gone.</param>
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ARowAddressedToTheOperatorIsShownUnderTheCqFilter(bool ended)
    {
        var model = Panel();

        model.ShowPsk31ChannelsForTests(new[] { new Psk31Channel(5, 1733, 10.0, ToTheOperator) });

        if (ended)
        {
            // **HIS CARRIER GOES**, which is what happened at 22:20:43 in the record.
            model.ShowPsk31ChannelsForTests(Array.Empty<Psk31Channel>());
        }

        Print(model, ended ? "ended" : "live");

        var row = Assert.Single(model.DigitalDecodes, r => r.Message.Contains(Him, StringComparison.Ordinal));

        Assert.Equal(ended, row.Ended);
        Assert.True(Shown(model, row), "the row that spoke to the operator is on no list he is looking at");
    }

    /// <summary>
    /// **Two stations answering him at once: one is the conversation, the other is on the waiting
    /// strip, and neither is off the screen.**
    /// </summary>
    /// <remarks>
    /// **THIS IS THE ONE WAY A ROW FOR HIM COULD GO UNSEEN.** The right-hand side carries the
    /// conversation with one station; everything else addressed to him is a name and a count on
    /// the waiting strip, one click away (unit 314's rule), and the strip is asserted here rather
    /// than assumed.
    /// </remarks>
    [Fact]
    public void TwoStationsAnsweringHimAreBothOnTheScreen()
    {
        var model = Panel();

        model.ShowPsk31ChannelsForTests(new[]
        {
            new Psk31Channel(5, 1733, 10.0, ToTheOperator),
            new Psk31Channel(7, 1600, 10.0, Mine + " de " + Somebody + " GM UR 5#9 BTU " + Mine + " de " + Somebody + " K\n"),
        });

        Print(model, "two answering him");

        foreach (var row in model.DigitalDecodes)
        {
            Assert.True(Shown(model, row), "a station who spoke to the operator is on no list: " + row.Message);
        }
    }

    /// <summary>
    /// **A row addressed to somebody else is still on the decoded list, and that is R9's rule and
    /// not this unit's.**
    /// </summary>
    /// <remarks>
    /// **THE CQ TOGGLE DOES NOT HOLD BACK A PSK31 ROW AT ALL** (work instruction 337 task 1, PSK31
    /// plan §R9): a PSK31 row has an addressee only once a turnover has been read, so on
    /// 2026-09-12 a carrier that emitted 262 characters and no turnover was held off the list all
    /// evening. This asserts the tree as it is, so that a later unit that starts hiding PSK31 rows
    /// has to answer for that case rather than discover it on an evening.
    /// </remarks>
    [Fact]
    public void ARowAddressedToSomebodyElseIsStillOnTheDecodedList()
    {
        var model = Panel();

        model.ShowPsk31ChannelsForTests(new[] { new Psk31Channel(6, 1500, 10.0, ToSomebodyElse) });

        Print(model, "for somebody else");

        var row = Assert.Single(model.DigitalDecodes, r => r.Message.Contains(Somebody, StringComparison.Ordinal));

        Assert.Contains(row, model.DigitalVisibleDecodes);
        Assert.DoesNotContain(row, model.DigitalMineDecodes);
    }

    /// <summary>**With the filter off, both are shown.**</summary>
    [Fact]
    public void WithTheFilterOffBothAreShown()
    {
        var model = Panel();

        model.ShowsCqOnly = false;
        model.ShowPsk31ChannelsForTests(new[]
        {
            new Psk31Channel(5, 1733, 10.0, ToTheOperator),
            new Psk31Channel(6, 1500, 10.0, ToSomebodyElse),
        });

        Print(model, "filter off");

        Assert.All(model.DigitalDecodes, row => Assert.True(Shown(model, row), row.Message));
    }

    /// <summary>Whether a row is on a list the operator can see.</summary>
    private static bool Shown(MainWindowViewModel model, DigitalDecodeRow row)
        => model.DigitalVisibleDecodes.Contains(row)
            || model.DigitalMineDecodes.Contains(row)
            || model.DigitalWaiting.Any(w => string.Equals(w.Callsign, row.Sender, StringComparison.OrdinalIgnoreCase));

    private void Print(MainWindowViewModel model, string what)
    {
        _output.WriteLine($"-- {what}: CQ filter {model.ShowsCqOnly}, hidden {model.DigitalHiddenCount}");
        _output.WriteLine("   decoded : " + string.Join(" | ", model.DigitalDecodes.Select(r => Short(r))));
        _output.WriteLine("   visible : " + string.Join(" | ", model.DigitalVisibleDecodes.Select(r => Short(r))));
        _output.WriteLine("   mine    : " + string.Join(" | ", model.DigitalMineDecodes.Select(r => Short(r))));
        _output.WriteLine("   waiting : " + string.Join(" | ", model.DigitalWaiting.Select(w => w.Callsign)));
    }

    private static string Short(DigitalDecodeRow row)
        => (row.Ended ? "[ended] " : "") + (row.Message.Length > 34 ? row.Message[..34] : row.Message).Replace("\n", " ", StringComparison.Ordinal);

    private static MainWindowViewModel Panel()
    {
        var settings = new AppSettings { ReconnectOnStartup = false };

        settings.Operator.Callsign = Mine;
        settings.Operator.GridSquare = "FN00";
        settings.Operator.LicenseClass = LicenseClass.General;

        var model = new MainWindowViewModel(settings, null)
        {
            OperatingMode = "Digital",
            DigitalNewestFirst = false,
        };

        model.UseWorkedBeforeForTests(new Dictionary<string, AdifContact>(StringComparer.OrdinalIgnoreCase));
        model.ChooseDigitalModeCommand.Execute("PSK31");

        // **THE FILTER IS ON**, which is the state the operator was in.
        model.ShowsCqOnly = true;

        return model;
    }
}
