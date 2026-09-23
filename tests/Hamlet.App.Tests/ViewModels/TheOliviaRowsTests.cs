using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using Hamlet.App.Settings;
using Hamlet.App.ViewModels;
using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Contacts;
using Hamlet.RadioEngine.Olivia;
using Hamlet.RadioEngine.Telemetry;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.ViewModels;

/// <summary>
/// Work instruction 364 task 2: **Olivia channels are drawn as rows through the PSK31 row path**
/// (step 3 criteria 3.1's rows half and 3.5, decisions AE, AF, AG, AH, AK and AN).
/// </summary>
/// <remarks>
/// <para>**FED AS THE APP IS FED** (decision AH): each mode author's file, hash-checked, handed to
/// the real tick through the audio tap a quarter-second at a time, as `ThePsk31HearsEveryoneTests`
/// feeds PSK31. Nothing tells the listener where a station is, which variant it is or when it
/// starts; the manifest only checks what came out.</para>
/// <para>**THE AIR DOES NOT STOP WHEN THE FILE DOES.** The app never flushes the listener, so
/// a block reaches its channel only as later audio pushes it through (`Unit364Trace`: the worst
/// block reached its channel 2.92 blocks after it ended). Each file is followed by
/// <see cref="TailBlocks"/> of the slowest variant's blocks of seeded white noise at
/// <see cref="TailRms"/> - far below the signal, and not silence, because a noise measure of
/// exactly nothing is not a band.</para>
/// <para>**COMPUTED, NOT SEEN**, and nothing here is evidence about the radio (FACT-004).</para>
/// </remarks>
public sealed class TheOliviaRowsTests : IDisposable
{
    /// <summary>The slowest variant's blocks of noise fed after each file.</summary>
    private const int TailBlocks = 4;

    /// <summary>The tail's level, as a root-mean-square sample value.</summary>
    private const double TailRms = 0.001;

    /// <summary>The operator's callsign: neither station's in any Olivia fixture.</summary>
    private const string OwnCall = "K1ABC";

    private const string Mode = "Olivia";

    private readonly ITestOutputHelper _output;
    private readonly string _folder;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where each feed is printed.</param>
    public TheOliviaRowsTests(ITestOutputHelper output)
    {
        _output = output;
        _folder = Path.Combine(Path.GetTempPath(), "hamlet-olivia-rows-" + Guid.NewGuid().ToString("N"));

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

    private static OliviaFormat Format => OliviaData.Current.Format!;

    /// <summary>
    /// **3.1, rows half, decision AH: the two-signal file through the app yields exactly two rows -
    /// 8/250 within 5 Hz of 1000 and 16/500 within 5 Hz of 2000, each showing its variant, each
    /// reading its own half of the manifest text at CER 0.05 or under, the other station's
    /// callsign in neither - and never more than two rows at any tick.**
    /// </summary>
    [Fact]
    public void TheTwoSignalFileIsTwoRowsEachWithItsVariantAndItsOwnText()
    {
        var (audio, text) = Fixture("olivia-two-signals-rsid.wav");
        var model = Listening(null);
        var fed = Feed(model, audio);
        var halves = text.Split(" | ");

        // The manifest's note: the first half is the 8/250 station's. The check, never the input.
        var expected = new[] { ("8/250", 1000.0, halves[0], halves[1]), ("16/500", 2000.0, halves[1], halves[0]) };
        var rows = model.DigitalDecodes.ToList();

        Print(rows);
        _output.WriteLine($"most rows at any tick {fed.MostRows}");

        Assert.Equal(2, rows.Count);
        Assert.Equal(2, fed.MostRows);

        foreach (var (variant, centerHz, own, other) in expected)
        {
            var row = Assert.Single(rows, r => r.Variant == variant);
            var cer = ErrorRate(row.Message, own);
            var otherCall = CallsignOf(other);

            _output.WriteLine($"{variant}: CER {cer:0.0000} against its own half; {otherCall} appears {Count(row.Message, otherCall)} times");

            Assert.True(row.IsTextOnly);
            Assert.True(row.HasVariant);
            Assert.InRange(Hz(row), centerHz - 5, centerHz + 5);
            Assert.True(cer <= 0.05, $"{variant}: CER {cer:0.0000} over 0.05");
            Assert.DoesNotContain(otherCall, row.Message, StringComparison.Ordinal);
        }
    }

    /// <summary>**The noise-only file through the same path: no row and no character.**</summary>
    [Fact]
    public void TheNoiseOnlyFileIsNoRowAndNoCharacter()
    {
        var (audio, _) = Fixture("olivia-noise-only-30s.wav");
        var model = Listening(null);
        var fed = Feed(model, audio);

        _output.WriteLine($"most rows at any tick {fed.MostRows}, characters on any row {fed.MostCharacters}");

        Assert.Equal(0, fed.MostRows);
        Assert.Equal(0, fed.MostCharacters);
        Assert.Empty(model.DigitalDecodes);
    }

    /// <summary>**The no-RSID 8/250 file through the same path: one row, 8/250, found blind.**</summary>
    [Fact]
    public void TheNoRsidFileIsOneRowFoundBlind()
    {
        var (audio, text) = Fixture("olivia-8-250-qso-norsid.wav");
        var folder = Path.Combine(_folder, "blind");

        Directory.CreateDirectory(folder);

        DigitalDecodeRow row;

        using (var telemetry = new JsonlTelemetry(folder, "blind", _ => true))
        {
            var model = Listening(telemetry);
            var fed = Feed(model, audio);

            Print(model.DigitalDecodes);
            _output.WriteLine($"most rows at any tick {fed.MostRows}");

            Assert.Equal(1, fed.MostRows);

            row = Assert.Single(model.DigitalDecodes);
        }

        var appeared = Lines(folder).Select(Parse).Where(e => e.Event == "psk31_carrier_appeared").ToList();

        _output.WriteLine($"CER {ErrorRate(row.Message, text):0.0000}");

        Assert.Equal("8/250", row.Variant);
        Assert.InRange(Hz(row), 995, 1005);

        var found = Assert.Single(appeared);

        Assert.Equal("blind", found.Data.GetProperty("found").GetString());
    }

    /// <summary>
    /// **3.5, decision AK: on the two-signal file every row event the PSK31 path writes is written
    /// for the Olivia rows - the same names - with `mode: olivia` and the variant on each row's
    /// event, and no decoded text, no callsign and nothing of the operator's in any of them.**
    /// </summary>
    [Fact]
    public void TheRowEventsSayOliviaAndTheVariantAndCarryNoTextOrCallsign()
    {
        var (audio, text) = Fixture("olivia-two-signals-rsid.wav");
        var folder = Path.Combine(_folder, "events");

        Directory.CreateDirectory(folder);

        using (var telemetry = new JsonlTelemetry(folder, "events", _ => true))
        {
            var model = Listening(telemetry);

            Feed(model, audio);

            // **THE FIXTURE'S TWO CQs NEVER FINISH A LINE**: each ends `pse K` with nothing after it,
            // and the splitter closes a message on the character after a turnover. So the parse's
            // event is driven by one more channel through the tick's own seam, carrying a line that
            // finishes - the same path, with the variant, from the mapping on.
            model.ShowOliviaChannelsForTests(model.OliviaChannelsForTests.Append(
                new OliviaChannel(99, "16/500", 1500, OliviaListener.FoundByRsid, 0, "CQ CQ CQ de W1AW W1AW W1AW pse K\n", 9, 0, false)).ToList());

            // **LEAVING THE TAB IS A ROW EVENT TOO**: the stop, and each row ended with its words.
            model.ChooseDigitalModeCommand.Execute("FT8");
        }

        var lines = Lines(folder);
        var rowEvents = lines.Select(Parse).Where(e => e.Event.StartsWith("psk31_", StringComparison.Ordinal)).ToList();

        foreach (var e in rowEvents.GroupBy(e => e.Event))
        {
            _output.WriteLine($"{e.Key} x{e.Count()}: {e.First().Data.GetRawText()}");
        }

        var names = rowEvents.Select(e => e.Event).ToHashSet();

        foreach (var name in new[]
                 {
                     "psk31_carrier_appeared", "psk31_squelch", "psk31_reading",
                     "psk31_line_parsed", "psk31_carrier_retired", "psk31_row_ended",
                 })
        {
            Assert.Contains(name, names);
        }

        // **THE LISTENER'S START AND STOP ARE ITS OWN**, not PSK31's: `psk31_listening_started` means a
        // PSK31 listener started, and none did.
        Assert.DoesNotContain("psk31_listening_started", names);
        Assert.Single(lines.Select(Parse), e => e.Event == "olivia_listening_started");
        Assert.Single(lines.Select(Parse), e => e.Event == "olivia_listening_stopped");

        var appeared = rowEvents.Where(e => e.Event == "psk31_carrier_appeared").ToList();

        Assert.Equal(
            new[] { "16/500", "8/250" },
            appeared.Select(e => e.Data.GetProperty("variant").GetString()).OrderBy(v => v, StringComparer.Ordinal));

        foreach (var e in rowEvents)
        {
            Assert.Equal("olivia", e.Data.GetProperty("mode").GetString());

            if (e.Event is not ("psk31_listening_started" or "psk31_listening_stopped" or "psk31_audio_level"))
            {
                Assert.Contains(e.Data.GetProperty("variant").GetString(), new[] { "8/250", "16/500" });
            }
        }

        // **NO TEXT AND NO CALLSIGN** (HM-DEC-018): not either station, not any word either sent,
        // not the added line's station, and not the operator's own call.
        var words = text.Split(' ', '\n', '|').Where(w => w.Length >= 3 && w.Any(char.IsLetter)).Append(OwnCall).Append("W1AW").ToList();

        foreach (var line in lines)
        {
            foreach (var word in words)
            {
                Assert.DoesNotContain(word, line, StringComparison.Ordinal);
            }
        }
    }

    /// <summary>
    /// **With the two rows present under Olivia, and a card open, nothing is composed until a
    /// control is pressed, and every press that composes is announced at its own row's variant.**
    /// </summary>
    /// <remarks>
    /// <para>**REWRITTEN BY WORK INSTRUCTION 366 UNDER PSK31 PLAN §R12 (decision BI).** It was unit
    /// 364's decision AN test: every send control refused, four refusals at least, and no line of
    /// the record held `composed`, `armed` or `ptt`. **All of that guarded the shut mode gate**,
    /// which this unit opened, so keeping it would have made this class the wall that stopped the
    /// unit told to open the door.</para>
    /// <para>**WHAT IT GUARDS NOW IS WHAT STILL HOLDS**: no line of the record before the first
    /// press; one composition per press and no more, which is §R10's one click one transmission;
    /// each composition announced at the variant of the row it was addressed to, never at a variant
    /// anything here chose; and no second keying path, counted in `src/`. **Nothing keys**, because
    /// this panel has no armed send at all - the presses stop at `no_transmit_path`, which is the
    /// honest state of a bench with no radio and leaves the keying itself to
    /// `TheOliviaSendTests`.</para>
    /// </remarks>
    [Fact]
    public async System.Threading.Tasks.Task WithRowsPresentNothingIsComposedUntilAPressAndEachPressCarriesItsRowsVariant()
    {
        var (audio, _) = Fixture("olivia-two-signals-rsid.wav");
        var folder = Path.Combine(_folder, "refuse");

        Directory.CreateDirectory(folder);

        var presses = new List<(string Control, string Line)>();

        using (var telemetry = new JsonlTelemetry(folder, "refuse", _ => true))
        {
            var model = Listening(telemetry);

            Feed(model, audio);

            Assert.Equal(2, model.DigitalDecodes.Count(r => r.HasVariant));

            // **TWO MORE ROWS, THROUGH THE SAME SEAM THE TICK USES**: a certain message addressed to
            // the operator, so a card opens, and a CQ that finishes its line, so a click has a CQ to
            // answer. No Olivia fixture carries either: the file's two CQs end `pse K` with nothing
            // after, so neither ever finishes a line.
            var calling = new OliviaChannel(
                98, "8/250", 1500, OliviaListener.FoundByRsid, 0,
                OwnCall + " de W1AW GA TNX FER CALL UR RST 599 599 HW? " + OwnCall + " de W1AW K\n", 5, 0, false);
            var cq = new OliviaChannel(
                99, "16/500", 2500, OliviaListener.FoundByRsid, 0, "CQ CQ CQ de N1XYZ N1XYZ N1XYZ pse K\n", 9, 0, false);

            model.ShowOliviaChannelsForTests(model.OliviaChannelsForTests.Append(calling).Append(cq).ToList());

            var card = Assert.Single(model.DigitalCards);

            _output.WriteLine($"card: {card.Callsign}, offered {card.Offered}, action {card.ActionKind}");

            // **NOTHING HAS BEEN PRESSED YET, AND NOTHING HAS BEEN COMPOSED.** Two fixture
            // stations read, two more channels drawn, a card opened - and not one line of the
            // transmit or send-stage record, because a transmission takes a click (§0.2).
            var before = Lines(folder);

            Assert.DoesNotContain(before, l => l.Contains("psk31_send_composed", StringComparison.Ordinal));
            Assert.DoesNotContain(before, l => l.Contains("send_stage", StringComparison.Ordinal));

            foreach (var row in model.DigitalDecodes.Where(r => r.HasVariant).ToList())
            {
                model.DigitalSendLine = "";
                model.AnswerPsk31Command.Execute(row);
                presses.Add(("answer " + row.Variant, model.DigitalSendLine));

                model.DigitalSendLine = "";
                model.OpenPsk31CardCommand.Execute(row);
                presses.Add(("open card " + row.Variant, model.DigitalSendLine));
            }

            model.DigitalSendLine = "";
            model.SendCallToAnyoneCommand.Execute(null);
            presses.Add(("CQ", model.DigitalSendLine));

            model.DigitalSendLine = "";
            await model.CardActionCommand.ExecuteAsync(card);
            presses.Add(("card " + card.ActionKind, model.DigitalSendLine));

            model.DigitalSendLine = "";
            card.TypedText = "tnx fer the call";
            model.SendTypedPsk31Command.Execute(card);
            presses.Add(("typed line", model.DigitalSendLine));
        }

        foreach (var (control, line) in presses)
        {
            _output.WriteLine($"{control,-18}: {line}");
        }

        var lines = Lines(folder);
        var transmit = lines.Select(Parse).Where(e => e.Category == "transmit").ToList();

        foreach (var e in transmit)
        {
            _output.WriteLine("transmit: " + e.Event + " " + e.Data.GetRawText());
        }

        // **EVERY PRESS THAT REACHED THE DOOR WENT THROUGH IT AND STOPPED AT THE BENCH.** There is
        // no armed send on this panel, so each composition is followed by `no_transmit_path`, and
        // the operator is told which half is missing rather than left with a silent button.
        Assert.All(
            presses.Where(p => p.Line.Length > 0),
            p => Assert.Contains("sent nothing", p.Line, StringComparison.Ordinal));

        // **THE CLICKS, NOT THE STAGES.** The transmit category carries both now that a press goes
        // through the door, and only the operator's own actions name one.
        var actions = transmit
            .Where(e => e.Event == "operator_action")
            .Select(e => e.Data.GetProperty("action").GetString())
            .ToList();

        var composed = lines.Select(Parse).Where(e => e.Event == "psk31_send_composed").ToList();

        foreach (var e in composed)
        {
            _output.WriteLine("composed: " + e.Data.GetRawText());
        }

        // **ONE COMPOSITION PER PRESS THAT GOT PAST THE GATE, AND NOT ONE MORE** (§R10).
        Assert.Equal(actions.Count(a => a == "send_requested"), composed.Count);
        Assert.True(composed.Count >= 4, "the CQ press, the answer to the finished CQ, the card's button and the typed line must each reach the composer");

        // **EACH AT ITS OWN ROW'S VARIANT, AND THE CQ AT THE CALLING VARIANT** (R27, decision BA).
        // The variants that appear are the ones the two rows announced and the one the cited table
        // says calls start on, and nothing else.
        var variants = composed.Select(e => e.Data.GetProperty("variant").GetString()).Distinct().Order().ToList();

        _output.WriteLine("variants composed at: " + string.Join(", ", variants));

        Assert.All(composed, e => Assert.Equal("olivia", e.Data.GetProperty("mode").GetString()));
        Assert.Subset(new HashSet<string?> { "8/250", "16/500", OliviaCallingTable.CallingVariant }, variants.ToHashSet());
        Assert.Contains(OliviaCallingTable.CallingVariant, variants);

        // **AND NOTHING KEYED, BECAUSE NOTHING WAS ARMED.**
        foreach (var word in new[] { "transmission", "ptt", "keyed", "armed", "rsid_sent" })
        {
            Assert.All(lines, l => Assert.DoesNotContain(word, l, StringComparison.OrdinalIgnoreCase));
        }
    }

    /// <summary>
    /// **Decision AE, both ways: under Olivia the PSK31 listener is fed nothing, and under PSK31 the
    /// Olivia listener is fed nothing.**
    /// </summary>
    [Fact]
    public void EachTabFeedsOnlyItsOwnListener()
    {
        var (audio, _) = Fixture("olivia-8-250-cq-rsid.wav");
        var olivia = Listening(null);

        Feed(olivia, audio);

        _output.WriteLine($"under Olivia: Olivia listener {olivia.OliviaSamplesHeardForTests} samples, PSK31 listener {olivia.Psk31SamplesHeardForTests}");

        Assert.True(olivia.OliviaSamplesHeardForTests > 0);
        Assert.Equal(0, olivia.Psk31SamplesHeardForTests);

        var psk31 = Listening(null, "PSK31");

        Feed(psk31, audio);

        _output.WriteLine($"under PSK31: Olivia listener {psk31.OliviaSamplesHeardForTests} samples, PSK31 listener {psk31.Psk31SamplesHeardForTests}");

        Assert.Equal(0, psk31.OliviaSamplesHeardForTests);
        Assert.True(psk31.Psk31SamplesHeardForTests > 0);
        Assert.DoesNotContain(psk31.DigitalDecodes, r => r.HasVariant);
    }

    /// <summary>
    /// **3.4, decision AJ: fed the two-signal file and then quiet for longer than the longest window,
    /// each row is retired within its own variant's window after its last accepted block - the 16/500
    /// row, whose station stops first, while the 8/250 row is still open - and stays on the list
    /// marked ended, with its text, its center not moving after it ends; the retire event carries the
    /// variant, the window and the factor, and no text.**
    /// </summary>
    /// <remarks>
    /// The quiet is <see cref="TailRms"/> seeded noise, as every tail here is: the longest window
    /// plus <see cref="TailBlocks"/> of the slowest variant's blocks.
    /// </remarks>
    [Fact]
    public void EachRowIsRetiredWithinItsWindowAndStaysListedAsEnded()
    {
        var (audio, text) = Fixture("olivia-two-signals-rsid.wav");
        var timing = OliviaData.Current.Timing!;
        var folder = Path.Combine(_folder, "retire");
        var rate = audio.SampleRate;
        var block = new Dictionary<string, double>();

        foreach (var v in Format.Variants)
        {
            block[v.Name] = Format.SymbolsPerBlock * v.SymbolSeconds;
        }

        var longest = new[] { "8/250", "16/500" }.Max(timing.RetireWindowSeconds);
        var quiet = Noise((int)((longest + (TailBlocks * block["8/250"])) * rate), TailRms);
        var all = audio.Samples.Concat(quiet).ToArray();
        var endedAt = new Dictionary<string, double>();
        var hzAtEnd = new Dictionary<string, string>();
        var centerAtEnd = new Dictionary<string, double>();
        var textAtEnd = new Dictionary<string, string>();
        MainWindowViewModel model;

        Directory.CreateDirectory(folder);

        using (var telemetry = new JsonlTelemetry(folder, "retire", _ => true))
        {
            model = Listening(telemetry);

            var piece = rate / 4;

            for (var at = 0; at < all.Length; at += piece)
            {
                var count = Math.Min(piece, all.Length - at);

                model.TapForTests!.Take(all.AsSpan(at, count), rate);
                model.LookForASlotForTests();

                var now = (at + count) / (double)rate;

                foreach (var row in model.DigitalDecodes.Where(r => r.HasVariant && r.Ended && !endedAt.ContainsKey(r.Variant)))
                {
                    endedAt[row.Variant] = now;
                    hzAtEnd[row.Variant] = row.Hz;
                    textAtEnd[row.Variant] = row.Message;
                    centerAtEnd[row.Variant] = model.OliviaChannelsForTests.Single(c => c.Variant == row.Variant).CenterHz;

                    // **THE OTHER ROW, AT THIS TICK**, for the order the stations stopped in.
                    _output.WriteLine($"{row.Variant} ended at {now:0.00} s; rows still open: "
                        + string.Join(", ", model.DigitalDecodes.Where(r => !r.Ended).Select(r => r.Variant)));
                }
            }
        }

        var events = Lines(folder).Select(Parse).ToList();
        var halves = text.Split(" | ");

        Print(model.DigitalDecodes);

        foreach (var (variant, own) in new[] { ("8/250", halves[0]), ("16/500", halves[1]) })
        {
            var lastEnd = events
                .Where(e => e.Event == "olivia_block"
                    && e.Data.GetProperty("variant").GetString() == variant
                    && e.Data.GetProperty("accepted").GetBoolean())
                .Select(e => e.Data.GetProperty("atSeconds").GetDouble() + block[variant])
                .Max();
            var window = timing.RetireWindowSeconds(variant);
            var row = Assert.Single(model.DigitalDecodes, r => r.Variant == variant);
            var channel = model.OliviaChannelsForTests.Single(c => c.Variant == variant);

            _output.WriteLine(
                $"{variant}: last accepted block ends {lastEnd:0.000} s; window {window:0.000} s ({timing.RetireAfterCharacters} x "
                + $"{timing.SecondsPerCharacter[variant]}); ended at {(endedAt.TryGetValue(variant, out var t) ? t : double.NaN):0.000} s, "
                + $"{t - lastEnd:0.000} s after it; center at the end {centerAtEnd.GetValueOrDefault(variant):0.00}, now {channel.CenterHz:0.00}; "
                + $"row Hz {hzAtEnd.GetValueOrDefault(variant)} then {row.Hz}");

            Assert.True(endedAt.ContainsKey(variant), variant + " was never retired");
            Assert.InRange(endedAt[variant] - lastEnd, window, window + 0.25 + 1e-6);
            Assert.True(row.Ended);
            Assert.True(channel.Retired);
            Assert.Equal(textAtEnd[variant], row.Message);
            Assert.True(ErrorRate(row.Message, own) <= 0.05);
            Assert.Equal(hzAtEnd[variant], row.Hz);
            Assert.Equal(centerAtEnd[variant], channel.CenterHz);
        }

        Assert.True(endedAt["16/500"] < endedAt["8/250"], "the 16/500 station stops first and must be ended first");

        var retires = events
            .Where(e => e.Event == "psk31_carrier_retired" && e.Data.GetProperty("reason").GetString() == "SignalGone")
            .ToList();

        foreach (var e in retires)
        {
            _output.WriteLine("retire event: " + e.Data.GetRawText());
        }

        Assert.Equal(2, retires.Count);

        foreach (var e in retires)
        {
            var variant = e.Data.GetProperty("variant").GetString()!;

            Assert.Equal("olivia", e.Data.GetProperty("mode").GetString());
            Assert.Equal(Math.Round(timing.RetireWindowSeconds(variant), 3), e.Data.GetProperty("windowSeconds").GetDouble());
            Assert.Equal(timing.RetireAfterCharacters, e.Data.GetProperty("retireFactor").GetInt32());

            foreach (var word in text.Split(' ', '|').Where(w => w.Length >= 3 && w.Any(char.IsLetter)))
            {
                Assert.DoesNotContain(word, e.Data.GetRawText(), StringComparison.Ordinal);
            }
        }
    }

    private void Print(IEnumerable<DigitalDecodeRow> rows)
    {
        foreach (var row in rows)
        {
            _output.WriteLine(
                $"row: {row.Variant,-7} at {row.Hz} Hz, snr [{row.Snr}], ended {row.Ended}, sender [{row.Sender}] "
                + JsonSerializer.Serialize(row.Message));
        }
    }

    /// <summary>Hand a recording and its tail to the tick in quarter-second pieces, as a sound card would.</summary>
    private static Fed Feed(MainWindowViewModel model, MonoAudio audio)
    {
        var rate = audio.SampleRate;
        var tail = Noise((int)(TailBlocks * Format.SymbolsPerBlock * Format.Variants.Max(v => v.SymbolSeconds) * rate), TailRms);
        var all = audio.Samples.Concat(tail).ToArray();
        var piece = rate / 4;
        var most = 0;
        var characters = 0;

        for (var at = 0; at < all.Length; at += piece)
        {
            model.TapForTests!.Take(all.AsSpan(at, Math.Min(piece, all.Length - at)), rate);
            model.LookForASlotForTests();

            most = Math.Max(most, model.DigitalDecodes.Count);
            characters = Math.Max(characters, model.DigitalDecodes.Select(r => r.HeardNotReadable ? 0 : r.Message.Length).DefaultIfEmpty(0).Max());
        }

        return new Fed(most, characters);
    }

    /// <summary>Seeded white Gaussian noise.</summary>
    private static float[] Noise(int count, double rms)
    {
        var random = new Random(364);
        var samples = new float[count];

        for (var i = 0; i < count; i++)
        {
            var u1 = 1.0 - random.NextDouble();
            var u2 = random.NextDouble();

            samples[i] = (float)(rms * Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Cos(2.0 * Math.PI * u2));
        }

        return samples;
    }

    private static MainWindowViewModel Listening(JsonlTelemetry? telemetry, string mode = Mode)
    {
        var settings = new AppSettings { ReconnectOnStartup = false };

        settings.Operator.Callsign = OwnCall;
        settings.Operator.GridSquare = "FN42";

        // **A NAME AND A PLACE, SO A CARD'S REPORT HAS TEXT AND ITS BUTTON EXISTS** (PSK31's rule in
        // `MacroTextFor`): without them there is no Report to press, and decision AN's card press
        // would test nothing.
        settings.Operator.OperatorName = "Pat";
        settings.Operator.Location = "Boston MA";

        var model = new MainWindowViewModel(settings, telemetry)
        {
            OperatingMode = "Digital",
            TapForTests = new AudioTap(),
        };

        model.UseWorkedBeforeForTests(new Dictionary<string, AdifContact>());
        model.ChooseDigitalModeCommand.Execute(mode);

        return model;
    }

    /// <summary>A fixture from the mode author's set, after its hash has been checked against the manifest.</summary>
    private static (MonoAudio Audio, string Text) Fixture(string file)
    {
        var folder = Path.Combine(Root(), "assets", "fixtures", "olivia");

        using var manifest = JsonDocument.Parse(File.ReadAllText(Path.Combine(folder, "manifest.json")));

        var entry = manifest.RootElement.EnumerateArray().Single(e => e.GetProperty("file").GetString() == file);
        var path = Path.Combine(folder, file);

        Assert.Equal(
            entry.GetProperty("sha256").GetString(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(File.ReadAllBytes(path))).ToLowerInvariant());

        return (WavAudio.Read(path), entry.GetProperty("text").GetString() ?? "");
    }

    /// <summary>Decision J's character error rate: Levenshtein over the expected text's length, line endings unified.</summary>
    private static double ErrorRate(string got, string want)
    {
        var a = got.Replace("\r\n", "\n", StringComparison.Ordinal).Replace('\r', '\n');
        var b = want.Replace("\r\n", "\n", StringComparison.Ordinal).Replace('\r', '\n');
        var previous = Enumerable.Range(0, b.Length + 1).ToArray();
        var current = new int[b.Length + 1];

        for (var i = 1; i <= a.Length; i++)
        {
            current[0] = i;

            for (var j = 1; j <= b.Length; j++)
            {
                current[j] = Math.Min(Math.Min(current[j - 1] + 1, previous[j] + 1), previous[j - 1] + (a[i - 1] == b[j - 1] ? 0 : 1));
            }

            (previous, current) = (current, previous);
        }

        return b.Length == 0 ? a.Length : (double)previous[b.Length] / b.Length;
    }

    private static string CallsignOf(string half)
    {
        var words = half.Split(' ');
        var at = Array.IndexOf(words, "de");

        return at >= 0 && at + 1 < words.Length ? words[at + 1] : "";
    }

    private static int Count(string text, string word)
        => word.Length == 0 ? 0 : (text.Length - text.Replace(word, "", StringComparison.Ordinal).Length) / word.Length;

    private static double Hz(DigitalDecodeRow row)
        => double.TryParse(row.Hz, NumberStyles.Float, CultureInfo.InvariantCulture, out var hz) ? hz : double.NaN;

    // **READ BESIDE THE WRITER, NOT AGAINST IT.** The telemetry thread appends with its handle
    // open for writing; File.ReadAllLines asks for FileShare.Read, which that handle cannot grant,
    // so a read mid-append threw IOException on a busy machine (unit 392). ReadWrite admits it.
    private static List<string> Lines(string folder)
        => Directory.GetFiles(folder, "*.jsonl").SelectMany(ReadBesideTheWriter).ToList();

    private static List<string> ReadBesideTheWriter(string path)
    {
        using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        using var reader = new StreamReader(stream);

        var lines = new List<string>();

        while (reader.ReadLine() is { } line)
        {
            lines.Add(line);
        }

        return lines;
    }

    private static Written Parse(string line)
    {
        var root = JsonDocument.Parse(line).RootElement;

        return new Written(root.GetProperty("category").GetString() ?? "", root.GetProperty("event").GetString() ?? "", root.GetProperty("data").Clone());
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

    private sealed record Fed(int MostRows, int MostCharacters);

    private sealed record Written(string Category, string Event, JsonElement Data);
}
