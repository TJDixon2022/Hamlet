using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text.Json;
using Hamlet.App.Settings;
using Hamlet.App.ViewModels;
using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Contacts;
using Hamlet.RadioEngine.Olivia;
using Hamlet.RadioEngine.Psk31;
using Hamlet.RadioEngine.Rsid;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.ViewModels;

/// <summary>
/// Work instruction 367 task 1: **the trace, before the move is built** - items 1 to 7.
/// </summary>
/// <remarks>
/// <para>**THIS IS A MEASUREMENT, NOT AN ASSERTION**, the shape of `Unit366Trace`. It asserts
/// nothing at all, so it cannot become a wall, and it is on neither carry-forward line.</para>
/// <para>**THE CODE-LOCATION ITEMS ARE READ FROM THE TREE**, by name and line, rather than
/// transcribed from the instruction: a line that has moved reads as moved.</para>
/// <para>**IT IS IN THE APP PROJECT BECAUSE THE MOVE'S PATH IS**: the card, the offer, the two
/// send fields and the turn all live in `MainWindowViewModel`, and item 6's lag is measured
/// through the real tick rather than through the engine alone.</para>
/// <para>**COMPUTED, NOT SEEN**, and nothing here is evidence about the radio (FACT-004).</para>
/// </remarks>
public sealed class Unit367Trace
{
    /// <summary>20 m, where the cited table gives an Olivia calling center and a dial.</summary>
    private const long DialOn20m = 14_071_500;

    /// <summary>The station whose card the trace watches, in the mode author's own audio.</summary>
    private const string His = "W1AW";

    /// <summary>The operator, for item 6 only: the author's other station, so his lines are addressed to him.</summary>
    private const string MineInTheFixture = "KC3QIS";

    /// <summary>The operator elsewhere: neither station of any Olivia fixture.</summary>
    private const string Mine = "K1ABC";

    private const string Mode = "Olivia";

    /// <summary>R29's amount, for the arithmetic only. Nothing here moves anything.</summary>
    private const double UpHz = 500;

    private readonly ITestOutputHelper _output;

    /// <summary>Creates the trace.</summary>
    /// <param name="output">Where the readings are printed.</param>
    public Unit367Trace(ITestOutputHelper output) => _output = output;

    /// <summary>Items 1, 2, 3, 5 and 7: every line of `src/` the move would have to reach.</summary>
    [Fact]
    public void TheSitesTheMoveWouldHaveToReach()
    {
        var src = Path.Combine(Root(), "src");

        var wanted = new (string Item, string[] Patterns)[]
        {
            ("1 the card's life", ["_psk31Cards", "Ft8ContactCard.ForPsk31(", "class Psk31CardState", "ClearedAtMessages"]),
            ("1 what ends a card", ["_psk31Cards.Clear()", "DigitalCards.Remove(", "ForgetOlivia()", "ForgetPsk31()"]),
            ("1 the channel a card is read from", ["state.ChannelId", "ShowPsk31Cards(", "_psk31EndedReadings", "ResumePsk31Row("]),
            ("2 where a send goes and at which variant", ["_psk31SendAtHz", "_oliviaSendVariant", "Psk31OffsetOf(", "OliviaVariantOf("]),
            ("2 the offset the send path chooses", ["OliviaCallingOffsetHz()", "ClearSpotForTheCall()", "OliviaCentersHeard()"]),
            ("3 the offer and the one click", ["Psk31Offer.For(", "ActionFor(", "CardActionAsync(", "Psk31ActionLabel(", "Ft8CardActionKind"]),
            ("5 the RSID detections", ["_rsidDetector.Feed(", "RsidEvents.Heard(", "FoundByRsid", "channel.Variant"]),
            ("6 the turn", ["Psk31Turn.Read(", "Splitter.Pending"]),
            ("7 gated on PSK31 alone", ["IsPsk31Chosen"]),
            ("the fence: nothing here is touched", ["CivConstants.PttOn)", "_armedSend.Arm("]),
        };

        foreach (var (item, patterns) in wanted)
        {
            foreach (var pattern in patterns)
            {
                var found = Sites(src, pattern);

                if (found.Count == 0)
                {
                    _output.WriteLine($"({item}) {pattern}: NO CODE LINE IN src/");
                    continue;
                }

                foreach (var line in found)
                {
                    _output.WriteLine($"({item}) {line}");
                }
            }
        }
    }

    /// <summary>
    /// Item 1: what happens to a station's card when his signal moves to a new center and variant.
    /// </summary>
    /// <remarks>
    /// **THE QUESTION DECISION BR TURNS ON.** It is answered by driving the panel: a card is opened
    /// from a channel at the calling center at 8/250, that channel is ended and a new one opened
    /// 500 Hz up at 16/500, and what is left on the panel is printed.
    /// </remarks>
    [Fact]
    public void WhatBecomesOfTheCardWhenHisSignalMoves()
    {
        var model = Listening(Mine);
        var at = CallingOffsetOn(model);

        model.ShowOliviaChannelsForTests(
        [
            Channel(11, "8/250", at, Mine + " de " + His + " " + His + " K\n"),
        ]);

        Print("opened at the calling center", model);

        // **HIS CHANNEL ENDS AND A NEW ONE OPENS 500 Hz UP AT 16/500**, which is what the listener
        // does when a station announces a different variant at a different place: a place is the
        // same place only within half the narrower bandwidth (OliviaListener's remarks).
        model.ShowOliviaChannelsForTests(
        [
            Channel(11, "8/250", at, Mine + " de " + His + " " + His + " K\n", ended: true),
        ]);

        Print("his 8/250 channel ended", model);

        model.ShowOliviaChannelsForTests(
        [
            Channel(12, "16/500", at + UpHz, ""),
        ]);

        Print("a new channel 500 Hz up at 16/500, nothing read on it yet", model);

        model.ShowOliviaChannelsForTests(
        [
            Channel(12, "16/500", at + UpHz, Mine + " de " + His + " R R here we go K\n"),
        ]);

        Print("and his first 16/500 message", model);
    }

    /// <summary>Item 4: whether the band's Olivia calling offset is reachable where the card is built.</summary>
    /// <remarks>
    /// **AND THE TRAP IN IT.** `OliviaCallingOffsetHz` is the send path's question - *where may a
    /// call to anyone go* - and it answers null where a station being read is within
    /// `Psk31ClearSpot.ClearHz` of the calling center, which is exactly the case during a QSO on
    /// the calling center. So the offer's *are we on the calling center* test cannot be that
    /// method; it has to be the table's own arithmetic.
    /// </remarks>
    [Fact]
    public void WhetherTheCardKnowsItIsOnTheCallingCenter()
    {
        var table = OliviaData.Current.Calling!;

        _output.WriteLine(
            $"(4) the passband Hamlet listens across: {Psk31CarrierSearch.PassbandLowHz} to "
            + $"{Psk31CarrierSearch.PassbandHighHz} Hz; the calling window Psk31ClearSpot allows a call to "
            + $"anyone in: {Psk31ClearSpot.LowestCallHz} to {Psk31ClearSpot.HighestCallHz} Hz, clear by "
            + $"{Psk31ClearSpot.ClearHz} Hz");

        var format = OliviaData.Current.Format!;
        var wide = format.Variant("16/500")!;
        var half = (wide.Tones - 1) * wide.ToneSpacingHz / 2.0;

        _output.WriteLine(
            $"(4) 16/500 occupies about +/-{half:0.0} Hz of its center, so the highest center wholly inside "
            + $"the listening passband is {Psk31CarrierSearch.PassbandHighHz - half:0.0} Hz and the highest "
            + $"calling offset the move may be offered at is {Psk31CarrierSearch.PassbandHighHz - half - UpHz:0.0} Hz");

        _output.WriteLine("(4) band | calling center | dial the tab sets | calling offset | move to | inside the passband");

        foreach (var row in table.Rows)
        {
            var dial = table.DialHzFor(row);

            if (dial is not { } hz)
            {
                _output.WriteLine($"(4) {row.Band} | {row.CenterHz} | the table gives no dial | - | - | -");
                continue;
            }

            var offset = row.CenterHz - (double)hz;
            var moved = offset + UpHz;
            var fits = moved - half >= Psk31CarrierSearch.PassbandLowHz
                && moved + half <= Psk31CarrierSearch.PassbandHighHz;

            _output.WriteLine(
                $"(4) {row.Band} ({row.Variant}) | {row.CenterHz} | {hz} | {offset:0} Hz | {moved:0} Hz | "
                + $"{(fits ? "yes" : "NO")}");
        }

        // What the send path's own question answers, with and without a station sitting on the spot.
        var model = Listening(Mine);
        var at = CallingOffsetOn(model);

        _output.WriteLine($"(4) the table's arithmetic on this panel: calling offset {at:0} Hz at the tab's own dial");
        _output.WriteLine($"(4) OliviaCallingOffsetHz with nothing being read: {Say(model.OliviaCallingOffsetForTests)}");

        model.ShowOliviaChannelsForTests([Channel(21, "8/250", at, Mine + " de " + His + " " + His + " K\n")]);

        _output.WriteLine(
            $"(4) OliviaCallingOffsetHz with him on the calling center: {Say(model.OliviaCallingOffsetForTests)} "
            + "- null, because a station within 150 Hz makes the spot unclear for a *call*, which is the "
            + "very situation the move is offered in");
    }

    /// <summary>Item 5: which of the two carries the code and the center, and which decision BQ should read.</summary>
    [Fact]
    public void WhereARsidDetectionLandsToday()
    {
        var codes = OliviaData.Current.Rsid!;

        _output.WriteLine(
            "(5) RsidDetection carries: " + string.Join(", ",
                typeof(RsidDetection).GetProperties().Select(p => p.Name + " " + p.PropertyType.Name)));

        _output.WriteLine(
            "(5) OliviaChannel carries: " + string.Join(", ",
                typeof(OliviaChannel).GetProperties().Select(p => p.Name)));

        foreach (var variant in new[] { "8/250", "16/500", "32/1000" })
        {
            var name = OliviaModulator.AnnouncedAs(variant);

            _output.WriteLine($"(5) {variant} is announced as {name}, code {Say(codes.CodeOf(name))}");
        }

        // What a burst Hamlet makes at 16/500 reads back as, told nothing (decision I) - the same
        // read the follow check would make of the other station's announcement.
        var composed = OliviaModulator.Compose("R R K", "16/500", 2000, 12000, 0.5f);
        var audio = new MonoAudio(composed.SampleRate, composed.Samples);
        var heard = RsidDetector.Detect(
            codes, audio, Psk31CarrierSearch.PassbandLowHz, Psk31CarrierSearch.PassbandHighHz);

        foreach (var one in heard)
        {
            _output.WriteLine(
                $"(5) a 16/500 send at 2000 Hz reads back as code {one.Code} \"{one.Name}\" at "
                + $"{one.CenterHz:0.00} Hz, {Math.Abs(one.CenterHz - 2000):0.00} Hz of error, "
                + $"quality {one.Quality:0.000}, starting {one.StartSeconds:0.000} s in");
        }
    }

    /// <summary>
    /// Item 6, and 4.8's number: the lag from the turnover word to the card's turn reading changing.
    /// </summary>
    /// <remarks>
    /// <para>**THE MODE AUTHOR'S OWN AUDIO, AT 16/500**, fed to the real tick a quarter-second at a
    /// time. The operator is `KC3QIS` here and only here, so the author's other station is speaking
    /// to him and a card exists to read a turn off.</para>
    /// <para>**AND AT 8/250 FROM HAMLET'S OWN MODULATOR**, in memory, because the shipped 8/250
    /// file is a CQ to anybody and opens no card. The modulator's thirty loopbacks read back
    /// identical (unit 365), and the RSID burst in front of it is its own.</para>
    /// <para>**NOTHING HERE IS LOOSENED AND NOTHING IS ADDED TO THE TURN** (decision BS, R14).</para>
    /// </remarks>
    [Fact]
    public void TheLagFromTheTurnoverWordToTheTurnReading()
    {
        var format = OliviaData.Current.Format!;

        foreach (var variant in new[] { "8/250", "16/500" })
        {
            var v = format.Variant(variant)!;
            var block = format.SymbolsPerBlock * v.SymbolSeconds;

            _output.WriteLine(
                $"(6) {variant}: one block is {format.SymbolsPerBlock} symbols of {v.SymbolSeconds:0.000} s "
                + $"= {block:0.000} s, carrying {v.BitsPerSymbol} characters");
        }

        Measure("16/500", Fixture("olivia-16-500-qso-rsid.wav"), MineInTheFixture, His);

        var line = Psk31Macros.Answer(Mine, "N1XYZ") + "\n";
        var composed = OliviaModulator.Compose(line, "8/250", 1500, 12000, 0.5f);

        _output.WriteLine($"(6) 8/250, made by Hamlet's own modulator in memory: \"{line.TrimEnd()}\"");

        // **A TAIL OF NOISE AFTER IT**, as unit 364 feeds one, so the reader finishes its last
        // block rather than being cut off mid-block by the end of the buffer.
        var tail = Noise((int)(4 * format.SymbolsPerBlock * format.Variants.Max(v => v.SymbolSeconds) * composed.SampleRate), 0.001);

        Measure("8/250", new MonoAudio(composed.SampleRate, composed.Samples.Concat(tail).ToArray()), Mine, "N1XYZ");
    }

    /// <summary>Feed one recording to the real tick and print when each character and each turn arrived.</summary>
    private void Measure(string variant, MonoAudio audio, string mine, string his)
    {
        var format = OliviaData.Current.Format!;
        var v = format.Variant(variant)!;
        var block = format.SymbolsPerBlock * v.SymbolSeconds;
        var model = Listening(mine);
        var steps = new List<(double Seconds, int Length, string Turn)>();
        var rate = audio.SampleRate;
        var piece = rate / 4;

        for (var at = 0; at < audio.Samples.Length; at += piece)
        {
            var take = Math.Min(piece, audio.Samples.Length - at);

            model.TapForTests!.Take(audio.Samples.AsSpan(at, take), rate);
            model.LookForASlotForTests();

            var text = model.OliviaChannelsForTests
                .Where(c => !c.Ended)
                .Select(c => c.Text)
                .OrderByDescending(t => t.Length)
                .FirstOrDefault() ?? "";

            var card = model.DigitalCards.FirstOrDefault(
                c => string.Equals(c.Callsign, his, StringComparison.OrdinalIgnoreCase));

            steps.Add(((at + take) / (double)rate, text.Length, card?.TurnWord ?? "no card"));
        }

        var final = model.OliviaChannelsForTests
            .Select(c => c.Text)
            .OrderByDescending(t => t.Length)
            .FirstOrDefault() ?? "";

        _output.WriteLine(
            $"(6) {variant}: {audio.Samples.Length / (double)rate:0.0} s of audio, {final.Length} characters read, "
            + $"one block {block:0.000} s");

        if (final.Length == 0)
        {
            _output.WriteLine($"(6) {variant}: NOTHING WAS READ - no lag to measure");
            return;
        }

        // When each character index first became available, from the feed steps.
        var arrived = new double[final.Length];

        for (var i = 0; i < final.Length; i++)
        {
            arrived[i] = steps.Where(s => s.Length > i).Select(s => s.Seconds).DefaultIfEmpty(double.NaN).First();
        }

        // Every message the splitter closes, and the character that closed it.
        var splitter = new Psk31MessageSplitter(mine);

        for (var i = 0; i < final.Length; i++)
        {
            if (splitter.Add(final[i]) is not { } message)
            {
                continue;
            }

            var lag = arrived[i] - arrived[i - 1];

            _output.WriteLine(
                $"(6) {variant}: \"{message.Text}\" closed by the character at {i} "
                + $"({Show(final[i])}); its turnover word's last character arrived at {arrived[i - 1]:0.000} s, "
                + $"the character that closed the message at {arrived[i]:0.000} s, lag {lag:0.000} s "
                + $"= {lag / block:0.000} blocks");
        }

        string was = "";

        foreach (var step in steps)
        {
            if (step.Turn == was)
            {
                continue;
            }

            was = step.Turn;

            _output.WriteLine($"(6) {variant}: at {step.Seconds:0.000} s, {step.Length} characters, the card says \"{step.Turn}\"");
        }
    }

    /// <summary>Seeded white Gaussian noise, the tail unit 364 feeds after a file.</summary>
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

    /// <summary>What is on the panel now, printed.</summary>
    private void Print(string when, MainWindowViewModel model)
    {
        _output.WriteLine($"(1) {when}:");

        foreach (var card in model.DigitalCards)
        {
            _output.WriteLine(
                $"(1)   card {card.Callsign}: turn \"{card.TurnWord}\", offered {card.OfferedMacro}, "
                + $"variant \"{card.OliviaVariant}\", action {card.ActionKind} \"{card.ActionLabel}\", "
                + $"log link {card.ShowsLogLink}");
        }

        if (model.DigitalCards.Count == 0)
        {
            _output.WriteLine("(1)   no cards");
        }

        foreach (var row in model.DigitalDecodes)
        {
            _output.WriteLine(
                $"(1)   row at {row.Hz} Hz, variant \"{row.Variant}\", ended {row.Ended}, "
                + $"message \"{row.Message.Replace("\n", "\\n", StringComparison.Ordinal)}\"");
        }
    }

    /// <summary>An Olivia channel as the listener would list it.</summary>
    private static OliviaChannel Channel(int id, string variant, double centerHz, string text, bool ended = false)
        => new(id, variant, centerHz, OliviaListener.FoundByRsid, 0, text, text.Length > 0 ? 9 : 0, 0, ended, centerHz);

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
        settings.Operator.GridSquare = "FN42";
        settings.Operator.OperatorName = "Pat";
        settings.Operator.Location = "Boston MA";
        settings.Operator.LicenseClass = Hamlet.RadioEngine.Licensing.LicenseClass.General;

        var model = new MainWindowViewModel(settings, null)
        {
            OperatingMode = "Digital",
            TapForTests = new AudioTap(),
        };

        model.SelectedBand = model.Bands.First(b => b.Band.LowHz <= DialOn20m && b.Band.HighHz >= DialOn20m);
        model.FrequencyHz = DialOn20m;
        model.UseWorkedBeforeForTests(new Dictionary<string, AdifContact>(StringComparer.OrdinalIgnoreCase));
        model.ChooseDigitalModeCommand.Execute(Mode);

        return model;
    }

    private static string Show(char character)
        => char.IsWhiteSpace(character)
            ? "whitespace U+" + ((int)character).ToString("X4", CultureInfo.InvariantCulture)
            : character.ToString();

    private static string Say(double? value)
        => value is { } hz ? hz.ToString("0.0", CultureInfo.InvariantCulture) + " Hz" : "null";

    private static string Say(int? value)
        => value is { } code ? code.ToString(CultureInfo.InvariantCulture) : "none";

    /// <summary>A fixture from the mode author's set, after its hash has been checked against the manifest.</summary>
    private static MonoAudio Fixture(string file)
    {
        var folder = Path.Combine(Root(), "assets", "fixtures", "olivia");

        using var manifest = JsonDocument.Parse(File.ReadAllText(Path.Combine(folder, "manifest.json")));

        var entry = manifest.RootElement.EnumerateArray().Single(e => e.GetProperty("file").GetString() == file);
        var path = Path.Combine(folder, file);

        Assert.Equal(
            entry.GetProperty("sha256").GetString(),
            Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path))).ToLowerInvariant());

        return WavAudio.Read(path);
    }

    /// <summary>Every code line of `src/` holding this text, a comment not being one.</summary>
    private static List<string> Sites(string src, string needle)
    {
        var found = new List<string>();

        foreach (var path in Directory.EnumerateFiles(src, "*.cs", SearchOption.AllDirectories)
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
                    && lines[i].Contains(needle, StringComparison.Ordinal))
                {
                    found.Add($"{Path.GetRelativePath(src, path)}:{i + 1}: {trimmed}");
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
}
