using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Hamlet.App.Settings;
using Hamlet.App.ViewModels;
using Hamlet.RadioEngine.Contacts;
using Hamlet.RadioEngine.Explore;
using Hamlet.RadioEngine.Olivia;
using Hamlet.RadioEngine.Psk31;
using Hamlet.RadioEngine.Telemetry;
using Hamlet.RadioEngine.Tests.Psk31;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.ViewModels;

/// <summary>
/// Work instruction 364 task 3: **an Olivia row is read exactly as a PSK31 row is** - the parser
/// (step 3 criterion 3.2) and the CQ filter, worked-fade, `EntityOf` with its `CQ` guard, the quill
/// and the hover (3.3), with no change to their code.
/// </summary>
/// <remarks>
/// <para>**IN THE APP PROJECT, BECAUSE THE SEAMS ARE THERE**: an Olivia row is made by
/// <c>ShowOliviaChannelsForTests</c> and a PSK31 row by <c>ShowPsk31ChannelsForTests</c>, each the
/// path its tick takes from the channel onwards. The corpus is text, not audio, so it is fed at the
/// row (decision AI).</para>
/// <para>**EVERY COMPARISON IS OLIVIA ROW AGAINST PSK31 ROW, SAME TEXT.** A difference is the
/// finding; nothing here is loosened to make one agree.</para>
/// <para>**COMPUTED, NOT SEEN** (FACT-004).</para>
/// </remarks>
public sealed class TheOliviaRowsReadLikePsk31Tests : IDisposable
{
    private const string CqText = "CQ CQ CQ de TI2ABC TI2ABC K\n";

    private readonly ITestOutputHelper _output;
    private readonly string _folder;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the comparisons are printed.</param>
    public TheOliviaRowsReadLikePsk31Tests(ITestOutputHelper output)
    {
        _output = output;
        _folder = Path.Combine(Path.GetTempPath(), "hamlet-olivia-reads-" + Guid.NewGuid().ToString("N"));

        Directory.CreateDirectory(_folder);
    }

    /// <summary>Removes the telemetry folder.</summary>
    public void Dispose()
    {
        try
        {
            Directory.Delete(_folder, true);
        }
        catch (IOException)
        {
            // A left-over temp folder is not a test failure.
        }
    }

    /// <summary>
    /// **3.2, decision AI: every transcript in `corpus.json`, fed a line at a time through an Olivia
    /// row and through a PSK31 row, gets the same verdicts - the row's reading after every line, every
    /// field, and every line the parser returned a verdict for - with zero differences.**
    /// </summary>
    [Fact]
    public void TheCorpusThroughOliviaRowsGetsTheSameVerdictsAsThroughPsk31Rows()
    {
        var corpus = Psk31Corpus.Load();
        var differences = new List<string>();
        var readings = 0;
        var parsed = 0;

        _output.WriteLine($"corpus.json sha256 {corpus.Sha256}, {corpus.Transcripts.Count} transcripts, {corpus.Lines.Count} lines, operator {corpus.Operator}");

        foreach (var transcript in corpus.Transcripts)
        {
            var oliviaFolder = Path.Combine(_folder, "olivia-" + transcript.Name);
            var psk31Folder = Path.Combine(_folder, "psk31-" + transcript.Name);

            Directory.CreateDirectory(oliviaFolder);
            Directory.CreateDirectory(psk31Folder);

            var oliviaReadings = new List<string>();
            var psk31Readings = new List<string>();

            using (var oliviaTelemetry = new JsonlTelemetry(oliviaFolder, "olivia", _ => true))
            using (var psk31Telemetry = new JsonlTelemetry(psk31Folder, "psk31", _ => true))
            {
                var olivia = Panel(corpus.Operator, "Olivia", oliviaTelemetry);
                var psk31 = Panel(corpus.Operator, "PSK31", psk31Telemetry);

                for (var n = 1; n <= transcript.Lines.Count; n++)
                {
                    var text = string.Concat(transcript.Lines.Take(n).Select(l => l.Text + "\n"));

                    olivia.ShowOliviaChannelsForTests(new[] { new OliviaChannel(1, "16/500", 1000, OliviaListener.FoundByRsid, 0, text, n, 0, false) });
                    psk31.ShowPsk31ChannelsForTests(new[] { new Psk31Channel(1, 1000, 10, text) });

                    oliviaReadings.Add(JsonSerializer.Serialize(TextRow(olivia).Reading));
                    psk31Readings.Add(JsonSerializer.Serialize(TextRow(psk31).Reading));
                }
            }

            for (var n = 0; n < oliviaReadings.Count; n++)
            {
                readings++;

                if (oliviaReadings[n] != psk31Readings[n])
                {
                    differences.Add($"{transcript.Name} after line {n + 1}: olivia {oliviaReadings[n]} / psk31 {psk31Readings[n]}");
                }
            }

            var oliviaVerdicts = Verdicts(oliviaFolder);
            var psk31Verdicts = Verdicts(psk31Folder);

            parsed += psk31Verdicts.Count;

            if (!oliviaVerdicts.SequenceEqual(psk31Verdicts))
            {
                differences.Add($"{transcript.Name}: olivia verdicts [{string.Join("; ", oliviaVerdicts)}] / psk31 [{string.Join("; ", psk31Verdicts)}]");
            }
        }

        _output.WriteLine($"transcripts {corpus.Transcripts.Count}; row readings compared {readings}; parser verdicts compared {parsed}; differences {differences.Count}");

        foreach (var difference in differences)
        {
            _output.WriteLine("DIFFERENCE " + difference);
        }

        Assert.True(parsed > 0, "the corpus must give the parser lines to rule on");
        Assert.Empty(differences);
    }

    /// <summary>
    /// **3.3, the CQ filter: with the CQ toggle on, each row lands where the PSK31 row with the same
    /// text lands** - a CQ, a finished line to somebody else, a line to the operator, and a line with
    /// no turnover yet.
    /// </summary>
    /// <remarks>
    /// **WHAT THE FILTER DOES TO A TEXT ROW IS UNIT 337'S RULE, AND IT IS KEPT**: `WantsRow` holds back
    /// no PSK31 row for the CQ toggle - the squelch is the only gate on one - and `IsForHim` sends a
    /// line to the operator to his side. The instruction expected a non-CQ row to be dropped; on a
    /// PSK31 row it is not, so the Olivia row is held to the PSK31 row's outcome, whatever it is.
    /// </remarks>
    [Fact]
    public void TheCqFilterPutsEachOliviaRowWhereItPutsThePsk31Row()
    {
        var texts = new (int Id, double Hz, string Text)[]
        {
            (1, 1000, CqText),
            (2, 1500, "W9ZZZ de TI2ABC UR RST 599 599 W9ZZZ de TI2ABC K\n"),
            (3, 2000, "KC3QIS de TI2ABC UR RST 599 599 KC3QIS de TI2ABC K\n"),
            (4, 2500, "W9ZZZ de TI2ABC UR RST 599 BK\n"),
        };

        var outcomes = new Dictionary<string, List<string>>();

        foreach (var mode in new[] { "PSK31", "Olivia" })
        {
            var model = Panel("KC3QIS", mode, null);

            model.ShowsCqOnly = true;
            Show(model, mode, texts);

            outcomes[mode] = texts.Select(t =>
            {
                var row = model.DigitalDecodes.Single(r => r.Message == t.Text);
                var side = model.DigitalMineDecodes.Contains(row) ? "his side"
                    : model.DigitalVisibleDecodes.Contains(row) ? "shown" : "held back";

                return $"addressee [{row.Addressee}] {side}";
            }).ToList();

            _output.WriteLine($"{mode,-6}: {string.Join("; ", outcomes[mode])}");

            Assert.Contains(model.DigitalDecodes.Single(r => r.Message == CqText), model.DigitalVisibleDecodes);
            Assert.All(model.DigitalDecodes, r => Assert.Equal(mode == "Olivia", r.HasVariant));
        }

        Assert.Equal(outcomes["PSK31"], outcomes["Olivia"]);
    }

    /// <summary>**3.3, worked-fade: a worked station's Olivia row fades to the PSK31 row's 0.55.**</summary>
    [Fact]
    public void WorkedFadeDimsAnOliviaRowAsItDimsAPsk31Row()
    {
        var worked = new Dictionary<string, AdifContact>(StringComparer.OrdinalIgnoreCase)
        {
            ["TI2ABC"] = new AdifContact { Call = "TI2ABC", StartedUtc = new DateTime(2026, 9, 7, 1, 0, 0, DateTimeKind.Utc) },
        };

        var rows = new[] { "PSK31", "Olivia" }.Select(mode =>
        {
            var model = Panel("KC3QIS", mode, null, worked);

            Show(model, mode, (1, 1000, CqText));

            return TextRow(model);
        }).ToList();

        _output.WriteLine($"PSK31 opacity {rows[0].RowOpacity} [{rows[0].WorkedTip}]; Olivia opacity {rows[1].RowOpacity} [{rows[1].WorkedTip}]");

        Assert.Equal(0.55, rows[1].RowOpacity);
        Assert.Equal(rows[0].RowOpacity, rows[1].RowOpacity);
        Assert.Equal(rows[0].WorkedTip, rows[1].WorkedTip);
    }

    /// <summary>
    /// **3.3, `EntityOf` and its `CQ` guard: the Olivia row's station resolves to the same entity and
    /// hover as the PSK31 row's, and its addressee `CQ` is refused as a callsign on both.**
    /// </summary>
    [Fact]
    public void EntityOfResolvesTheStationAndRefusesCqOnBoth()
    {
        var rows = new[] { "PSK31", "Olivia" }.Select(mode =>
        {
            var model = Panel("KC3QIS", mode, null);

            Show(model, mode, (1, 1000, CqText));

            return TextRow(model);
        }).ToList();

        foreach (var row in rows)
        {
            _output.WriteLine($"variant [{row.Variant}]: sender {row.Sender} -> {DxccPrefixes.EntityOf(row.Sender)}; addressee {row.Addressee} -> {DxccPrefixes.EntityOf(row.Addressee) ?? "(refused)"}; hover [{row.SenderHelp}] [{row.AddresseeHelp}]");
        }

        Assert.Equal("TI2ABC", rows[1].Sender);
        Assert.NotNull(DxccPrefixes.EntityOf(rows[1].Sender));
        Assert.Equal(DxccPrefixes.EntityOf(rows[0].Sender), DxccPrefixes.EntityOf(rows[1].Sender));
        Assert.Equal(rows[0].SenderHelp, rows[1].SenderHelp);
        Assert.Equal("CQ", rows[1].Addressee);
        Assert.Null(DxccPrefixes.EntityOf(rows[1].Addressee));
        Assert.Null(DxccPrefixes.EntityOf(rows[0].Addressee));
        Assert.Equal(rows[0].AddresseeHelp, rows[1].AddresseeHelp);
    }

    /// <summary>**3.3, the quill: a station that would open something is marked the same on both rows.**</summary>
    [Fact]
    public void TheQuillMarksAnOliviaRowAsItMarksAPsk31Row()
    {
        var rows = new[] { "PSK31", "Olivia" }.Select(mode =>
        {
            var model = Panel("KC3QIS", mode, null, nudges: Log("W9ZZZ"));

            Show(model, mode, (1, 1000, CqText));

            return TextRow(model);
        }).ToList();

        foreach (var row in rows)
        {
            _output.WriteLine($"variant [{row.Variant}]: nudge {row.Nudge} [{row.NudgeTip}], lift {row.RowLift}");
        }

        Assert.NotEqual(NudgeKind.None, rows[0].Nudge);
        Assert.Equal(rows[0].Nudge, rows[1].Nudge);
        Assert.Equal(rows[0].NudgeTip, rows[1].NudgeTip);
        Assert.Equal(rows[0].IsNudged, rows[1].IsNudged);
        Assert.Equal(rows[0].RowLift, rows[1].RowLift);
    }

    /// <summary>**3.3, the hover: the whole message, with its station above it, the same on both rows.**</summary>
    [Fact]
    public void TheHoverShowsTheWholeMessageOnBoth()
    {
        const string long3 = "CQ CQ CQ de TI2ABC TI2ABC K\nW9ZZZ de TI2ABC GM OM TNX FER CALL UR RST 599 599 NAME JUAN QTH SAN JOSE HW? W9ZZZ de TI2ABC K\n";

        var rows = new[] { "PSK31", "Olivia" }.Select(mode =>
        {
            var model = Panel("KC3QIS", mode, null);

            Show(model, mode, (1, 1000, long3));

            return TextRow(model);
        }).ToList();

        foreach (var row in rows)
        {
            _output.WriteLine($"variant [{row.Variant}]: has whole message {row.HasWholeMessage}: {JsonSerializer.Serialize(row.WholeMessage)}");
        }

        Assert.True(rows[1].HasWholeMessage);
        Assert.Equal(rows[0].HasWholeMessage, rows[1].HasWholeMessage);

        // **THE HEAD NAMES THE TIME THE ROW WENT UP**, which is the wall clock and may differ by a
        // second between the two panels; the station and every character below it may not.
        Assert.Equal(Body(rows[0].WholeMessage), Body(rows[1].WholeMessage));
        Assert.StartsWith(rows[1].Sender + "  ·  ", rows[1].WholeMessage, StringComparison.Ordinal);

        rows[1].OpenTheWholeMessage();

        Assert.True(rows[1].WholeMessageIsOpen);
    }

    private static string Body(string whole) => whole[(whole.IndexOf('\n', StringComparison.Ordinal) + 1)..];

    private static void Show(MainWindowViewModel model, string mode, params (int Id, double Hz, string Text)[] channels)
    {
        if (mode == "Olivia")
        {
            model.ShowOliviaChannelsForTests(channels
                .Select(c => new OliviaChannel(c.Id, "8/250", c.Hz, OliviaListener.FoundByRsid, 0, c.Text, 9, 0, false))
                .ToList());
        }
        else
        {
            model.ShowPsk31ChannelsForTests(channels.Select(c => new Psk31Channel(c.Id, c.Hz, 10, c.Text)).ToList());
        }
    }

    private static DigitalDecodeRow TextRow(MainWindowViewModel model) => model.DigitalDecodes.Single(r => r.IsTextOnly);

    /// <summary>Every `psk31_line_parsed` verdict written, without the mode and variant an Olivia row adds.</summary>
    private static List<string> Verdicts(string folder)
        => Directory.GetFiles(folder, "*.jsonl")
            .SelectMany(File.ReadAllLines)
            .Select(l => JsonDocument.Parse(l).RootElement)
            .Where(e => e.GetProperty("event").GetString() == "psk31_line_parsed")
            .Select(e => string.Join(
                ",",
                e.GetProperty("data").EnumerateObject()
                    .Where(p => p.Name is not ("mode" or "variant"))
                    .Select(p => p.Name + "=" + p.Value.GetRawText())))
            .ToList();

    private static MainWindowViewModel Panel(
        string operatorCall, string mode, JsonlTelemetry? telemetry,
        Dictionary<string, AdifContact>? worked = null, NudgeSet? nudges = null)
    {
        var settings = new AppSettings { ReconnectOnStartup = false };

        settings.Operator.Callsign = operatorCall;
        settings.Operator.GridSquare = "FN00DJ";

        var model = new MainWindowViewModel(settings, telemetry)
        {
            OperatingMode = "Digital",
            DigitalNewestFirst = false,
        };

        model.UseWorkedBeforeForTests(worked ?? new Dictionary<string, AdifContact>(StringComparer.OrdinalIgnoreCase));
        model.UseNudgeSetForTests(nudges ?? Log());
        model.ChooseDigitalModeCommand.Execute(mode);

        return model;
    }

    /// <summary>A position in which exactly these stations have been worked, from the cited table.</summary>
    private static NudgeSet Log(params string[] workedCallsigns)
    {
        var entities = workedCallsigns
            .Select(DxccPrefixes.EntityOf)
            .Where(e => !string.IsNullOrWhiteSpace(e))
            .ToList();

        return new NudgeSet(entities, entities.Select(DxccContinents.Of));
    }
}
