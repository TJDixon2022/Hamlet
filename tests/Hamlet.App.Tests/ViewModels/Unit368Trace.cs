using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using Hamlet.App.Settings;
using Hamlet.App.ViewModels;
using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Contacts;
using Hamlet.RadioEngine.Olivia;
using Hamlet.RadioEngine.Rsid;
using Hamlet.RadioEngine.Tests.Psk31;
using Hamlet.RadioEngine.Transmit;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.ViewModels;

/// <summary>
/// Work instruction 368: **step 5's entry at task 0, and the trace at task 1** - decision CE and
/// items 1 to 7.
/// </summary>
/// <remarks>
/// <para>**THIS IS A MEASUREMENT, NOT AN ASSERTION**, the shape of `Unit367Trace`. It is on neither
/// carry-forward line.</para>
/// <para>**THE ENTRY IS MEASURED BEFORE ANY LINE OF `src/` CHANGES** (decision CE). The exchange is
/// a loopback on the development machine and no part of it is evidence about the radio
/// (FACT-004).</para>
/// <para>**HAMLET'S HALF IS READ OFF THE SAMPLES THE SOUND CARD WAS HANDED**, through the detector
/// and the demodulator, told nothing but what the burst in front of them announces (decision I) -
/// which is the loopback units 365 to 367 proved, run once more here so the entry is measured
/// rather than inherited.</para>
/// </remarks>
public sealed class Unit368Trace : IDisposable
{
    /// <summary>20 m, inside a General class data segment, where the cited table gives an Olivia spot.</summary>
    private const long DialOn20m = 14_071_500;

    /// <summary>The other station of the corpus's textbook exchange, the one that ends on 73.</summary>
    private const string His = "W1AW";

    /// <summary>The transcript worked to 73: his CQ, his report, his 73 and SK.</summary>
    private const string Textbook = "01-textbook";

    private const string Mode = "Olivia";

    private readonly ITestOutputHelper _output;
    private readonly string _folder;
    private readonly string _wasFolder;

    /// <summary>Creates the trace and redirects the data folder.</summary>
    /// <param name="output">Where the readings are printed.</param>
    public Unit368Trace(ITestOutputHelper output)
    {
        _output = output;
        _wasFolder = SettingsStore.DataFolder;
        _folder = Path.Combine(Path.GetTempPath(), "hamlet-unit368-" + Guid.NewGuid().ToString("N"));

        Directory.CreateDirectory(_folder);
        SettingsStore.DataFolder = _folder;
    }

    /// <summary>The variant the calling spot is worked at, from the cited table and never from here.</summary>
    private static string CallingVariant => OliviaCallingTable.CallingVariant;

    /// <summary>Puts the real folder back.</summary>
    public void Dispose()
    {
        SettingsStore.DataFolder = _wasFolder;

        try
        {
            Directory.Delete(_folder, recursive: true);
        }
        catch (IOException)
        {
        }
    }

    /// <summary>
    /// **Step 5's entry, decision CE: an Olivia conversation carried to `73` on both halves of one
    /// card, with the card open at the end.**
    /// </summary>
    /// <remarks>
    /// **HIS HALF IS THE CORPUS'S TEXTBOOK TRANSCRIPT AND HAMLET'S HALF IS HAMLET'S OWN MACROS**, so
    /// the `73` is on both sides: his last line closes `73 73 ... SK` and `Psk31Macros.Confirm`
    /// closes the same way. Neither half is typed here.
    /// </remarks>
    [Fact]
    public void TheEntryLoopbackExchangeReaches73()
    {
        var clock = Stopwatch.StartNew();
        var corpus = Psk31Corpus.Load();
        var model = Listening(corpus.Operator);
        var (sink, _) = Arm(model);
        var calling = CallingOffsetOn(model);

        var his = corpus.Transcripts
            .Single(t => t.Name == Textbook)
            .Lines
            .Where(l => string.Equals(l.Frm, His, StringComparison.OrdinalIgnoreCase))
            .Select(l => l.Text + "\n")
            .ToList();

        _output.WriteLine(
            $"(entry) operator {corpus.Operator}, station {His}, dial {DialOn20m} Hz, Olivia calling "
            + $"offset {calling:0} Hz at {CallingVariant}; his half is {Textbook}, {his.Count} lines");

        var heard = "";
        var mine = new List<string>();

        for (var over = 0; over < his.Count; over++)
        {
            heard += his[over];

            model.ShowOliviaChannelsForTests([Channel(68, CallingVariant, calling, heard)]);

            var card = TheCard(model);

            _output.WriteLine(
                $"(entry) over {over + 1}: he sent \"{his[over].TrimEnd()}\"");

            _output.WriteLine(
                $"(entry) over {over + 1}: card {(card is null ? "not open yet" : "open")}, turn "
                + $"\"{card?.TurnWord}\", offered {card?.OfferedMacro}, action {card?.ActionKind} "
                + $"\"{card?.ActionLabel}\", variant \"{card?.OliviaVariant}\", log link {card?.ShowsLogLink}");

            if (over == 0)
            {
                var row = model.DigitalDecodes.FirstOrDefault(
                    r => string.Equals(r.Sender, His, StringComparison.OrdinalIgnoreCase));

                if (row is null)
                {
                    _output.WriteLine("(entry) NO ROW FROM HIM - the exchange cannot be opened");
                    return;
                }

                model.AnswerPsk31Command.Execute(row);
            }
            else if (card is not null)
            {
                model.CardActionCommand.Execute(card);
            }

            Settle(model);

            var sent = ReadBack(sink, "over " + (over + 1));

            if (sent.Length > 0)
            {
                mine.Add(sent);
            }
        }

        var atTheEnd = TheCard(model);

        _output.WriteLine(
            $"(entry) at the end: card {(atTheEnd is null ? "gone" : "open")}, turn \"{atTheEnd?.TurnWord}\", "
            + $"action {atTheEnd?.ActionKind} \"{atTheEnd?.ActionLabel}\", log link {atTheEnd?.ShowsLogLink}, "
            + $"variant \"{atTheEnd?.OliviaVariant}\"");

        var hisHalf = string.Concat(his);
        var myHalf = string.Join(" ", mine);

        _output.WriteLine(
            $"(entry) his half reaches 73: {hisHalf.Contains("73", StringComparison.Ordinal)}; Hamlet's "
            + $"half, as it came back off the sound card, reaches 73: "
            + $"{myHalf.Contains("73", StringComparison.Ordinal)}");

        // **WHETHER THE LOG IS REACHABLE FROM HERE, BY BOTH DOORS**: the card's own Log link, which
        // is what a keyboard-mode contact is logged from, and `CanLogRow`, which the instruction
        // names. Reported, not repaired.
        var newest = model.DigitalDecodes
            .LastOrDefault(r => string.Equals(r.Sender, His, StringComparison.OrdinalIgnoreCase));

        _output.WriteLine(
            $"(entry) newest row from him: {(newest is null ? "none" : "IsTextOnly " + newest.IsTextOnly)}; "
            + $"CanLogRow {(newest is null ? "n/a" : model.CanLogRow(newest).ToString())}; the card's log "
            + $"link {atTheEnd?.ShowsLogLink}");

        var entry = model.ContactLogEntryForStation(His, DialOn20m);

        _output.WriteLine(
            entry is null
                ? "(entry) ContactLogEntryForStation gives no record"
                : "(entry) the record it gives today:\n" + AdifLog.Record(entry));

        _output.WriteLine($"(entry) the whole exchange took {clock.Elapsed.TotalSeconds:0.0} s");
    }

    /// <summary>What the detector and the demodulator make of the audio the sink was handed.</summary>
    /// <param name="sink">The fake sound card.</param>
    /// <param name="when">Which over this was, for the printed line.</param>
    /// <returns>The text that came back, or an empty string where nothing did.</returns>
    private string ReadBack(FakeSink sink, string when)
    {
        if (sink.LastSamples is null || sink.LastSamples.Length == 0)
        {
            _output.WriteLine($"(entry) {when}: NOTHING REACHED THE SOUND CARD");
            return "";
        }

        var codes = OliviaData.Current.Rsid!;
        var rate = sink.RateAskedFor;
        var audio = new MonoAudio(rate, sink.LastSamples);
        var heard = RsidDetector.Detect(codes, audio, 200, (rate / 2.0) - 200);

        if (heard.Count == 0)
        {
            _output.WriteLine($"(entry) {when}: the send carried no announcement the detector could read");
            return "";
        }

        var announced = heard[0];
        var variant = RsidCodes.VariantOf(announced.Name);

        if (variant is null)
        {
            _output.WriteLine($"(entry) {when}: the announcement \"{announced.Name}\" names no Olivia variant");
            return "";
        }

        var format = OliviaData.Current.Format!;
        var demodulator = new OliviaDemodulator(format, format.Variant(variant)!, announced.CenterHz, rate);
        var burstEnd = announced.StartSeconds + (codes.Symbols / codes.SymbolRateHz);
        var decoding = demodulator.Decode(audio, burstEnd);

        _output.WriteLine(
            $"(entry) {when}: Hamlet's send read back - announced {announced.Name} (code {announced.Code}) "
            + $"at {announced.CenterHz:0.00} Hz, {audio.Samples.Length / (double)rate:0.0} s of audio, "
            + $"{decoding.CharactersOut} characters out of {decoding.BlocksDecoded} blocks");

        _output.WriteLine($"(entry) {when}: read " + JsonSerializer.Serialize(decoding.Text));

        return decoding.Text;
    }

    /// <summary>The one card on the panel for the station being worked, or null.</summary>
    private static Ft8ContactCard? TheCard(MainWindowViewModel model)
        => model.DigitalCards.FirstOrDefault(
            c => string.Equals(c.Callsign, His, StringComparison.OrdinalIgnoreCase));

    /// <summary>An Olivia channel as the listener would list it.</summary>
    private static OliviaChannel Channel(int id, string variant, double centerHz, string text)
        => new(id, variant, centerHz, OliviaListener.FoundByRsid, 0, text, text.Length > 0 ? 9 : 0, 0, false, centerHz);

    /// <summary>Where the band's Olivia calling center sits in the passband, by the table's own arithmetic.</summary>
    private static double CallingOffsetOn(MainWindowViewModel model)
    {
        var row = OliviaData.Current.Calling!.CallingRowFor(model.SelectedBand.Band.Name)!;

        return row.CenterHz - (double)model.FrequencyHz;
    }

    /// <summary>A panel on 20 m with the Olivia tab pressed and the tap running.</summary>
    private static MainWindowViewModel Listening(string mine)
    {
        var settings = new AppSettings { ReconnectOnStartup = false };

        settings.Operator.Callsign = mine;
        settings.Operator.GridSquare = "FN00DJ";
        settings.Operator.OperatorName = "Tim";
        settings.Operator.Location = "Trafford PA";
        settings.Operator.LicenseClass = Hamlet.RadioEngine.Licensing.LicenseClass.General;

        var model = new MainWindowViewModel(settings, null)
        {
            OperatingMode = "Digital",
            DigitalNewestFirst = false,
            TapForTests = new AudioTap(),
        };

        model.SelectedBand = model.Bands.First(b => b.Band.LowHz <= DialOn20m && b.Band.HighHz >= DialOn20m);
        model.FrequencyHz = DialOn20m;
        model.UseWorkedBeforeForTests(new Dictionary<string, AdifContact>(StringComparer.OrdinalIgnoreCase));
        model.ChooseDigitalModeCommand.Execute(Mode);

        return model;
    }

    /// <summary>Give the panel the fake wire and the fake sound card, and the one armed send.</summary>
    private static (FakeSink Sink, FakePort Port) Arm(MainWindowViewModel model)
    {
        var port = new FakePort();
        var sink = new FakeSink();

        model.UseRigPortForTests(port);
        model.UseArmedSendForTests(new Ft8ArmedSend(new Ft8TransmitSequence(port, sink)));

        return (sink, port);
    }

    /// <summary>Wait for the send that the press started, and run what it posted to the UI thread.</summary>
    private static void Settle(MainWindowViewModel model)
    {
        for (var tries = 0; tries < 2000 && model.HasSomethingToStop; tries++)
        {
            System.Threading.Thread.Sleep(10);
        }

        Avalonia.Threading.Dispatcher.UIThread.RunJobs();
    }
}
