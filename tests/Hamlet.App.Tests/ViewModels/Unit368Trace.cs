using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
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

    /// <summary>
    /// Items 1, 2 and 3: **what an Olivia contact logs as today, and where the variant is when it
    /// does.**
    /// </summary>
    /// <remarks>
    /// **THE SAME EXCHANGE AS THE ENTRY**, worked to 73 and then asked for its record. The reports
    /// and the grid are read off whatever the shared card dictionaries answer, which is item 2's
    /// question, and nothing here is asserted - it is printed so that task 2 builds what is missing
    /// and rebuilds nothing that already works (R14).
    /// </remarks>
    [Fact]
    public void WhatAnOliviaContactLogsAsToday()
    {
        var corpus = Psk31Corpus.Load();
        var model = Listening(corpus.Operator);

        Arm(model);
        Work(model, corpus);

        var card = TheCard(model);
        var entry = model.ContactLogEntryForStation(His, DialOn20m);

        if (entry is null)
        {
            _output.WriteLine("(1) ContactLogEntryForStation gives NO RECORD for an Olivia station");
            return;
        }

        _output.WriteLine("(1) the whole record an Olivia contact logs as today:");
        _output.WriteLine($"(1)   CALL        {Say(entry.Call)}");
        _output.WriteLine($"(1)   MODE        {Say(entry.Mode)}");
        _output.WriteLine($"(1)   SUBMODE     {Say(entry.Submode)}");
        _output.WriteLine($"(1)   RST_SENT    {Say(entry.RstSent)}   (the RST field)");
        _output.WriteLine($"(1)   RST_RCVD    {Say(entry.RstReceived)}   (the RST field)");
        _output.WriteLine($"(1)   report sent {Say(entry.ReportSent)}   (the decibel field)");
        _output.WriteLine($"(1)   report rcvd {Say(entry.ReportReceived)}   (the decibel field)");
        _output.WriteLine($"(1)   GRIDSQUARE  {Say(entry.GridSquare)}");
        _output.WriteLine($"(1)   MY_GRID     {Say(entry.MyGridSquare)}");
        _output.WriteLine($"(1)   BAND        {Say(entry.Band)}");
        _output.WriteLine($"(1)   FREQ        {(entry.FrequencyMhz is { } mhz ? mhz.ToString("0.000000", CultureInfo.InvariantCulture) : "(none)")}");
        _output.WriteLine($"(1)   TIME_ON     {Say(entry.StartedUtc?.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture))}");
        _output.WriteLine($"(1)   TIME_OFF    {Say(entry.EndedUtc?.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture))}");
        _output.WriteLine("(1) as ADIF:");
        _output.WriteLine(AdifLog.Record(entry));

        foreach (var line in Sites(Path.Combine(Root(), "src"), "ContactModes.Named(\"PSK31\")"))
        {
            _output.WriteLine("(1) the line that decided the mode: " + line);
        }

        // Item 2: whether 5.1's *with RST and grid* already holds, read off the record rather than
        // off the private methods that filled it.
        _output.WriteLine(
            $"(2) Psk31ReportsFor answered for an Olivia station: sent {Say(entry.RstSent)}, received "
            + $"{Say(entry.RstReceived)}; Psk31GridFor answered {Say(entry.GridSquare)}. Both read "
            + "_psk31Cards and _psk31Readings, which an Olivia row fills.");

        // Item 3: where the variant is at logging time, on a conversation that never moved.
        _output.WriteLine(
            $"(3) on a QSO that stayed put: the card's OliviaVariant \"{card?.OliviaVariant}\", the "
            + $"channel's next-send variant \"{model.OliviaNextSendForTests(His).Variant}\", the row's "
            + $"variant \"{model.DigitalDecodes.LastOrDefault(r => string.Equals(r.Sender, His, StringComparison.OrdinalIgnoreCase))?.Variant}\"");
    }

    /// <summary>Item 3, decision BW's own case: a QSO that moved from 8/250 to 16/500.</summary>
    /// <remarks>
    /// **UNIT 367'S ONE CLICK, USED AND NOT RE-ARGUED** (step 4 is closed). What is measured is only
    /// which of the three readings the log could take after the move, and what each one says.
    /// </remarks>
    [Fact]
    public void WhereTheVariantIsAfterTheMove()
    {
        var corpus = Psk31Corpus.Load();
        var model = Listening(corpus.Operator);

        Arm(model);

        var calling = CallingOffsetOn(model);
        var his = corpus.Transcripts.Single(t => t.Name == Textbook).Lines
            .Where(l => string.Equals(l.Frm, His, StringComparison.OrdinalIgnoreCase))
            .Select(l => l.Text + "\n")
            .ToList();

        // His CQ and Hamlet's answer, then his certain report - which is the certain over the move
        // is offered on.
        model.ShowOliviaChannelsForTests([Channel(68, CallingVariant, calling, his[0])]);

        var row = model.DigitalDecodes.FirstOrDefault(
            r => string.Equals(r.Sender, His, StringComparison.OrdinalIgnoreCase));

        if (row is null)
        {
            _output.WriteLine("(3) NO ROW FROM HIM - the move cannot be reached");
            return;
        }

        model.AnswerPsk31Command.Execute(row);
        Settle(model);

        model.ShowOliviaChannelsForTests([Channel(68, CallingVariant, calling, his[0] + his[1])]);

        var card = TheCard(model);

        _output.WriteLine(
            $"(3) before the move: card variant \"{card?.OliviaVariant}\", move offered "
            + $"{card?.HasOliviaMove}, next send {Say(model.OliviaNextSendForTests(His).Variant)}");

        if (card is null || !card.HasOliviaMove)
        {
            _output.WriteLine("(3) the move is not offered here, so BW's case cannot be driven in this trace");
            return;
        }

        model.OliviaMoveUpCommand.Execute(card);
        Settle(model);

        var moved = TheCard(model);

        _output.WriteLine(
            $"(3) after the one click: card variant \"{moved?.OliviaVariant}\", next send "
            + $"{Say(model.OliviaNextSendForTests(His).Variant)} at "
            + $"{model.OliviaNextSendForTests(His).AtHz?.ToString("0", CultureInfo.InvariantCulture) ?? "(none)"} Hz; "
            + $"the row he was read on still says \"{model.DigitalDecodes.LastOrDefault(r => string.Equals(r.Sender, His, StringComparison.OrdinalIgnoreCase))?.Variant}\"");

        var entry = model.ContactLogEntryForStation(His, DialOn20m);

        _output.WriteLine(
            entry is null
                ? "(3) and the record after the move: NO RECORD"
                : $"(3) and the record after the move: MODE {Say(entry.Mode)}, SUBMODE {Say(entry.Submode)}");
    }

    /// <summary>Item 4: what the achievements do with a `MODE=OLIVIA` record today.</summary>
    [Fact]
    public void WhatTheAchievementsDoWithAnOliviaRecordToday()
    {
        var olivia = new AdifContact
        {
            Call = His,
            StationCallsign = "KC3QIS",
            Band = "20m",
            Mode = "OLIVIA",
            Submode = null,
            RstSent = "599",
            RstReceived = "599",
            GridSquare = "FN31",
            MyGridSquare = "FN00DJ",
            StartedUtc = new DateTime(2026, 9, 19, 21, 0, 0, DateTimeKind.Utc),
            EndedUtc = new DateTime(2026, 9, 19, 21, 12, 0, DateTimeKind.Utc),
        };

        var records = new[] { new AdifLogRecord(olivia, [], true) };
        var log = new AchievementLog(records, "FN00DJ");

        _output.WriteLine($"(4) a log of one MODE=OLIVIA record: {log.Count} contacts");
        _output.WriteLine($"(4) AchievementLog.Modes: [{string.Join(", ", log.Modes)}]");
        _output.WriteLine($"(4) AchievementScores.WorkableModes: {AchievementScores.WorkableModes}");
        _output.WriteLine($"(4) FirstsEarned: [{string.Join(", ", AchievementScores.FirstsEarned(log))}]");
        _output.WriteLine(
            "(4) AchievementBadgePage.Firsts keys: ["
            + string.Join(", ", AchievementBadgePage.Firsts.Select(f => f.Key + " = \"" + f.Said + "\"")) + "]");

        var points = AchievementPoints.Parse(AchievementPoints.Shipped());
        var page = new AchievementBadgePage(log, points);
        var badge = page.Badges.FirstOrDefault(b => b.Name == "Modes");

        _output.WriteLine(
            badge is null
                ? "(4) there is no Modes badge on the page"
                : $"(4) the Modes badge: words \"{badge.Meaning}\", standing \"{badge.Standing}\", "
                  + $"next \"{badge.NextCard}\", score \"{badge.ScoreLine}\"");

        var screen = new AchievementsViewModel(records, "FN00DJ", points);

        foreach (var row in screen.Firsts)
        {
            _output.WriteLine(
                $"(4) the firsts row for {row.Name} ({row.AdifSpelling}): {row.State}, earned {row.Earned}");
        }

        var category = AchievementCategory.For(AchievementKinds.Modes, page);

        if (category is null)
        {
            _output.WriteLine("(4) AchievementCategory.ModesFor draws no page");
        }
        else
        {
            _output.WriteLine($"(4) the Modes category band line: \"{category.BandLine}\"");

            foreach (var card in category.Cards)
            {
                _output.WriteLine($"(4)   card \"{card.Title}\" earned {card.Earned}");
            }
        }

        _output.WriteLine(
            "(4) every line of src that reads ContactModes.Six, which decision BY would have read "
            + "ContactModes.Logged:");

        foreach (var line in Sites(Path.Combine(Root(), "src"), "ContactModes.Six"))
        {
            _output.WriteLine("(4)   " + line);
        }

        _output.WriteLine("(4) and the badge's words, wherever a count of modes is typed in src:");

        foreach (var line in Sites(Path.Combine(Root(), "src"), "modes to work"))
        {
            _output.WriteLine("(4)   " + line);
        }
    }

    /// <summary>
    /// Items 6 and 7: which tests assert the count of modes, and where the ADIF spelling can be
    /// checked from inside the tree.
    /// </summary>
    [Fact]
    public void WhatGuardsTheCountAndWhereTheSpellingIsCited()
    {
        var tests = Path.Combine(Root(), "tests");

        _output.WriteLine("(6) every test line that names the count of modes, the badge's words or the table:");

        foreach (var needle in new[] { "WorkableModes", "ContactModes.Six", "modes to work" })
        {
            foreach (var line in Sites(tests, needle))
            {
                _output.WriteLine($"(6)   [{needle}] {line}");
            }
        }

        _output.WriteLine($"(7) ContactModes.Cite: {ContactModes.Cite}");
        _output.WriteLine(
            "(7) ContactModes.Logged, as the table carries it today: ["
            + string.Join("; ", ContactModes.Logged.Select(m => m.Name + " -> " + m.AdifSpelling)) + "]");

        var vendor = Path.Combine(Root(), "data", "vendor");

        _output.WriteLine(
            "(7) data/vendor holds: "
            + (Directory.Exists(vendor)
                ? string.Join(", ", Directory.GetFileSystemEntries(vendor).Select(Path.GetFileName))
                : "(no such folder)")
            + " - no copy of the ADIF enumeration is vendored, so a test may assert only what the "
            + "tree itself carries: ContactModes.Cite's text and the spellings in ContactModes.");

        var format = OliviaData.Current.Format!;

        _output.WriteLine(
            "(7) the variant names the format file already holds: ["
            + string.Join(", ", format.Variants.Select(v => v.Name)) + "]");
    }

    /// <summary>Works the textbook exchange to 73, a press at a time.</summary>
    private static void Work(MainWindowViewModel model, Psk31Corpus corpus)
    {
        var calling = CallingOffsetOn(model);
        var his = corpus.Transcripts.Single(t => t.Name == Textbook).Lines
            .Where(l => string.Equals(l.Frm, His, StringComparison.OrdinalIgnoreCase))
            .Select(l => l.Text + "\n")
            .ToList();

        var heard = "";

        for (var over = 0; over < his.Count; over++)
        {
            heard += his[over];

            model.ShowOliviaChannelsForTests([Channel(68, CallingVariant, calling, heard)]);

            var card = TheCard(model);

            if (over == 0)
            {
                var row = model.DigitalDecodes.FirstOrDefault(
                    r => string.Equals(r.Sender, His, StringComparison.OrdinalIgnoreCase));

                if (row is null)
                {
                    return;
                }

                model.AnswerPsk31Command.Execute(row);
            }
            else if (card is not null)
            {
                model.CardActionCommand.Execute(card);
            }

            Settle(model);
        }
    }

    /// <summary>Every code line under a folder holding this text, a comment not being one.</summary>
    private static List<string> Sites(string folder, string needle)
    {
        var found = new List<string>();

        foreach (var path in Directory.EnumerateFiles(folder, "*.cs", SearchOption.AllDirectories)
                     .Where(p => !p.Contains(Path.DirectorySeparatorChar + "obj" + Path.DirectorySeparatorChar, StringComparison.Ordinal)
                                 && !p.Contains(Path.DirectorySeparatorChar + "bin" + Path.DirectorySeparatorChar, StringComparison.Ordinal))
                     .OrderBy(p => p, StringComparer.Ordinal))
        {
            var lines = File.ReadAllLines(path);

            for (var i = 0; i < lines.Length; i++)
            {
                var trimmed = lines[i].TrimStart();

                if (!trimmed.StartsWith("//", StringComparison.Ordinal)
                    && !trimmed.StartsWith("*", StringComparison.Ordinal)
                    && !trimmed.StartsWith("///", StringComparison.Ordinal)
                    && lines[i].Contains(needle, StringComparison.Ordinal))
                {
                    found.Add($"{Path.GetRelativePath(folder, path)}:{i + 1}: {trimmed}");
                }
            }
        }

        return found;
    }

    private static string Root()
    {
        var at = new DirectoryInfo(AppContext.BaseDirectory);

        while (at is not null && !File.Exists(Path.Combine(at.FullName, "Hamlet.sln")))
        {
            at = at.Parent;
        }

        return at?.FullName ?? throw new InvalidOperationException("no Hamlet.sln above the test binary");
    }

    private static string Say(string? value)
        => string.IsNullOrEmpty(value) ? "(none)" : value;

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
