using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Hamlet.App.Settings;
using Hamlet.App.ViewModels;
using Hamlet.RadioEngine.Contacts;
using Hamlet.RadioEngine.Licensing;
using Hamlet.RadioEngine.Psk31;
using Hamlet.RadioEngine.Telemetry;
using Hamlet.RadioEngine.Transmit;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.ViewModels;

/// <summary>
/// Work instruction 385 task 3, criteria 9.3 and 9.1: **Log is on a conversation card from the
/// moment the card exists, and a card the operator takes off his screen says so in the record.**
/// </summary>
/// <remarks>
/// <para>**R44, FROM THE OWNER'S OWN CONTACT**: there was no way to log the man he worked, because
/// Log waited for the exchange to finish in the exact shape Hamlet expects - Hamlet's confirmation
/// and his certain goodbye. **A half exchange is still a contact he made.**</para>
/// <para>**NOTHING IS INVENTED TO FILL A FIELD** (§0.0, ruling 2 item 2). What Hamlet observed is
/// logged; what it did not observe stays absent. That was already true of
/// `ContactLogEntryForStation`, and this asserts it rather than rebuilding it (R14).</para>
/// <para>**AND LOG NEVER GOES ON A RECEIPT** (Tim, 2026-09-11, and §R8): a call to anybody is not
/// a conversation, and the existing guard stays exactly as it is.</para>
/// <para>**COMPUTED, NOT SEEN**, and nothing here is evidence about the radio (FACT-004).</para>
/// </remarks>
public sealed class TheCardOffersLogAndAnXTests : IDisposable
{
    private const string Mine = "KC3QIS";
    private const string Him = "W1ABC";

    /// <summary>His over: addressed to the operator, handing the turn back, RST in it.</summary>
    private const string HisOver = Mine + " de " + Him + " GM TNX CALL UR 599 599 BTU " + Mine + " de " + Him + " K\n";

    private readonly ITestOutputHelper _output;
    private readonly string _folder = Path.Combine(Path.GetTempPath(), "hamlet-log-" + Guid.NewGuid().ToString("N"));

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the card and the fields are printed.</param>
    public TheCardOffersLogAndAnXTests(ITestOutputHelper output)
    {
        _output = output;
        Directory.CreateDirectory(_folder);
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        try
        {
            Directory.Delete(_folder, true);
        }
        catch (IOException)
        {
        }
    }

    /// <summary>**9.3: Log is on his card the moment the card exists, half an exchange in.**</summary>
    [Fact]
    public void LogIsOnTheCardFromTheMomentThereIsSomebodyToLog()
    {
        var model = Panel(out var telemetry);

        model.ShowPsk31ChannelsForTests(new[] { new Psk31Channel(5, 1500, 10.0, HisOver) });

        var card = Assert.Single(model.DigitalCards);

        _output.WriteLine($"card {card.Callsign}: ShowsLogLink {card.ShowsLogLink}, label [{card.LogLabel}], "
            + $"complete {card.ShowsLogLink}");

        // **THE EXCHANGE IS NOT FINISHED** - he has said nothing like a goodbye and Hamlet has
        // sent no confirmation - and Log is there anyway.
        Assert.Equal(Him, card.Callsign);
        Assert.True(card.ShowsLogLink);
        Assert.False(card.IsCallToAnyone);

        telemetry.Dispose();
    }

    /// <summary>**And it is never on a receipt** (Tim, 2026-09-11; §R8).</summary>
    [Fact]
    public void LogIsNeverOnAReceipt()
    {
        var model = Panel(out var telemetry);

        Cq(model);

        var receipt = Assert.Single(model.DigitalCards, c => c.IsCallToAnyone);

        _output.WriteLine($"receipt {receipt.Callsign}: ShowsLogLink {receipt.ShowsLogLink}, "
            + $"ActionKind {receipt.ActionKind}");

        Assert.False(receipt.ShowsLogLink);

        telemetry.Dispose();
    }

    /// <summary>
    /// **9.3's second half: the log entry is what was exchanged, and nothing is invented.**
    /// </summary>
    /// <remarks>
    /// **ASSERTED, NOT REBUILT** (R14). `ContactLogEntryForStation` already takes its report from
    /// the mode's own parser and leaves every unobserved field null; this pins that, so a later
    /// unit cannot quietly start filling one in.
    /// </remarks>
    [Fact]
    public void TheLogEntryCarriesWhatPassedAndInventsNothing()
    {
        var model = Panel(out var telemetry);

        model.ShowPsk31ChannelsForTests(new[] { new Psk31Channel(5, 1500, 10.0, HisOver) });

        var entry = model.ContactLogEntryForStation(Him, 14_070_000);

        Assert.NotNull(entry);

        _output.WriteLine($"call {entry!.Call}, mode {entry.Mode}/{entry.Submode}, band {entry.Band}, "
            + $"rstRx [{entry.RstReceived}] rstTx [{entry.RstSent}] grid [{entry.GridSquare}]");

        // **WHAT HE SENT IS THERE**: the report came out of his own over.
        Assert.Equal(Him, entry.Call);
        Assert.Equal("599", entry.RstReceived);
        Assert.Equal("PSK31", entry.Submode);

        // **AND WHAT WAS NOT OBSERVED IS ABSENT, NOT GUESSED** (§0.0): Hamlet has sent him no
        // report, he has put no grid on the air, and neither is filled in.
        Assert.Null(entry.RstSent);
        Assert.Null(entry.GridSquare);

        telemetry.Dispose();
    }

    /// <summary>
    /// **9.1: the X takes his card off the screen, records the press and the leaving, and deletes
    /// nothing.**
    /// </summary>
    [Fact]
    public void TheXTakesTheCardOffTheScreenAndRecordsIt()
    {
        var model = Panel(out var telemetry);

        model.ShowPsk31ChannelsForTests(new[] { new Psk31Channel(5, 1500, 10.0, HisOver) });

        var card = Assert.Single(model.DigitalCards);

        model.ClearCardCommand.Execute(card.Callsign);

        _output.WriteLine($"after the X: cards {model.DigitalCards.Count}, rows {model.DigitalDecodes.Count}, "
            + $"ledger {(model.LedgerForTests?.For(Him) is null ? "gone" : "still there")}");

        // **OFF THE SCREEN.**
        Assert.Empty(model.DigitalCards);

        // **AND NOTHING ELSE IS TOUCHED** (ruling 2 item 4): his row stays, and the ledger still
        // holds what passed, so the contact can still be logged from the row.
        Assert.Contains(model.DigitalDecodes, r => r.IsTextOnly);
        Assert.NotNull(model.LedgerForTests!.For(Him));

        model.FlushOnScreenForTests();
        telemetry.Dispose();

        // **THE PRESS IS IN THE RECORD**, through the writer that already carries card presses.
        var press = Assert.Single(
            Events("operator_action"),
            e => e.GetProperty("action").GetString() == "card_cleared");

        _output.WriteLine("press : " + press);

        // **AND THE CARD'S LEAVING IS IN IT TOO**, with `dismissed` as the reason - unit 380's
        // writer, which the PSK31 branch of the press did not reach before this unit.
        var left = Assert.Single(
            Events("on_screen"),
            e => e.GetProperty("kind").GetString() == "card"
                && e.GetProperty("state").GetString() == "removed"
                && e.GetProperty("by").GetString() == "dismissed");

        _output.WriteLine("left  : " + left);

        NothingPersonal();
    }

    /// <summary>**And a receipt goes on one press too**, which is R2's own rule.</summary>
    [Fact]
    public void TheXTakesAReceiptOffTheScreenOnOnePress()
    {
        var model = Panel(out var telemetry);

        Cq(model);

        var receipt = Assert.Single(model.DigitalCards, c => c.IsCallToAnyone);

        model.ClearCardCommand.Execute(receipt.Callsign);

        _output.WriteLine($"after the X on a receipt: cards {model.DigitalCards.Count}");

        Assert.DoesNotContain(model.DigitalCards, c => c.IsCallToAnyone);

        telemetry.Dispose();
    }

    /// <summary>One CQ press, over the fakes, so there is a receipt on the panel.</summary>
    private static void Cq(MainWindowViewModel model)
    {
        model.SendCallToAnyoneCommand.Execute(null);

        for (var tries = 0; tries < 200 && model.HasSomethingToStop; tries++)
        {
            System.Threading.Thread.Sleep(10);
        }
    }

    private List<JsonElement> Events(string name)
        => Directory.GetFiles(_folder, "*.jsonl")
            .SelectMany(File.ReadAllLines)
            .Select(line => JsonDocument.Parse(line).RootElement)
            .Where(e => e.GetProperty("event").GetString() == name)
            .Select(e => e.GetProperty("data").Clone())
            .ToList();

    private void NothingPersonal()
    {
        foreach (var line in Directory.GetFiles(_folder, "*.jsonl").SelectMany(File.ReadAllLines))
        {
            Assert.DoesNotContain(Him, line, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain(Mine, line, StringComparison.OrdinalIgnoreCase);
        }
    }

    private MainWindowViewModel Panel(out JsonlTelemetry telemetry)
    {
        var settings = new AppSettings { ReconnectOnStartup = false };

        settings.Operator.Callsign = Mine;
        settings.Operator.GridSquare = "FN00";
        settings.Operator.OperatorName = "Tester";
        settings.Operator.Location = "Erie PA";
        settings.Operator.LicenseClass = LicenseClass.General;

        telemetry = new JsonlTelemetry(_folder, "385", _ => true);

        var model = new MainWindowViewModel(settings, telemetry)
        {
            OperatingMode = "Digital",
            DigitalNewestFirst = false,
        };

        model.UseWorkedBeforeForTests(new Dictionary<string, AdifContact>(StringComparer.OrdinalIgnoreCase));
        model.ChooseDigitalModeCommand.Execute("PSK31");
        model.SelectedBand = model.Bands.First(b => b.Band.LowHz <= 14_070_000 && b.Band.HighHz >= 14_070_000);
        model.FrequencyHz = 14_070_000;

        var port = new FakePort();

        model.UseRigPortForTests(port);
        model.UseArmedSendForTests(new Ft8ArmedSend(new Ft8TransmitSequence(port, new FakeSink(), guard: null, telemetry)));

        return model;
    }
}
