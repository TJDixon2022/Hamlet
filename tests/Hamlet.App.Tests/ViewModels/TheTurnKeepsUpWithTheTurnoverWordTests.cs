using System;
using System.Collections.Generic;
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
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.ViewModels;

/// <summary>
/// Work instruction 367 task 5: **criterion 4.8 - the turn indicator changes within one block of
/// the other station's turnover word** (decision BS).
/// </summary>
/// <remarks>
/// <para>**THIS IS A MEASUREMENT AND THE NUMBER IS THE DELIVERABLE.** Nothing was invented to have
/// something to point at, and **nothing gives the turn a clock**: `Psk31Turn.Read` still reads a
/// list of messages and a carrier-present fact, and nothing in it or above it consults a time
/// (unit 366's measurement, unchanged).</para>
/// <para>**THE LAG IS AN AUDIO LAG, NOT A WALL LAG.** What is measured is the audio between the last
/// accepted character of the turnover word and the audio at which the card's turn reading changed,
/// both read off the same feed of quarter-second pieces into the real tick. So the number is a
/// property of the mode and the splitter, not of the machine the test ran on.</para>
/// <para>**AT 16/500 IT IS THE MODE AUTHOR'S OWN AUDIO**, hashed against the manifest first, with the
/// operator set to the author's other station so that his lines are addressed to the operator and a
/// card exists to read a turn off. **At 8/250 it is Hamlet's own modulator**, in memory, because the
/// shipped 8/250 file is a CQ to anybody and opens no card; that modulator's thirty loopbacks read
/// back identical (unit 365) and the RSID burst in front of it is its own.</para>
/// <para>**NOTHING HERE TRANSMITS** (§0.2): no wire, no sound card and no armed send are given to the
/// panel at all.</para>
/// </remarks>
public sealed class TheTurnKeepsUpWithTheTurnoverWordTests
{
    /// <summary>20 m, where the cited table gives an Olivia calling center and a dial.</summary>
    private const long DialOn20m = 14_071_500;

    private const string Mode = "Olivia";

    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the lag table is printed.</param>
    public TheTurnKeepsUpWithTheTurnoverWordTests(ITestOutputHelper output) => _output = output;

    /// <summary>
    /// **4.8 at 16/500, on the mode author's own four-line QSO: the card's turn changes within one
    /// block of his turnover word.**
    /// </summary>
    [Fact]
    public void AtSixteenFiveHundredTheTurnChangesInsideOneBlock()
    {
        var measured = Measure(
            "16/500",
            Fixture("olivia-16-500-qso-rsid.wav"),
            mine: "KC3QIS",
            his: "W1AW");

        // **ONE BLOCK OR LESS IS MET** (decision BS). Never loosened: the bound is the block.
        Assert.True(
            measured.LagSeconds <= measured.BlockSeconds,
            $"the turn lagged {measured.LagSeconds:0.000} s, more than one block of {measured.BlockSeconds:0.000} s");
    }

    /// <summary>
    /// **4.8 at 8/250, on a send Hamlet's own modulator made: the card's turn changes within one
    /// block of his turnover word.**
    /// </summary>
    [Fact]
    public void AtEightTwoFiftyTheTurnChangesInsideOneBlock()
    {
        var format = OliviaData.Current.Format!;
        const string his = "N1XYZ";
        const string mine = "K1ABC";

        var line = Psk31Macros.Answer(mine, his) + "\n";
        var composed = OliviaModulator.Compose(line, "8/250", 1500, 12000, 0.5f);
        var tail = Noise(
            (int)(4 * format.SymbolsPerBlock * format.Variants.Max(v => v.SymbolSeconds) * composed.SampleRate),
            0.0005);

        _output.WriteLine($"8/250, made by Hamlet's own modulator in memory: \"{line.TrimEnd()}\"");

        var measured = Measure(
            "8/250",
            new MonoAudio(composed.SampleRate, composed.Samples.Concat(tail).ToArray()),
            mine,
            his);

        Assert.True(
            measured.LagSeconds <= measured.BlockSeconds,
            $"the turn lagged {measured.LagSeconds:0.000} s, more than one block of {measured.BlockSeconds:0.000} s");
    }

    /// <summary>
    /// **And the turn still reads no clock** (decision BS, R14): the one place a turn is decided is
    /// handed a list of messages, a carrier-present fact and a callsign, and nothing else.
    /// </summary>
    /// <remarks>
    /// **THE WAY 4.8 WOULD BE CHEATED IS BY GIVING THE TURN A TIMER**, so the count of
    /// `Psk31Turn.Read` call sites in `src/` and the absence of any clock inside it are the guard
    /// beside the number.
    /// </remarks>
    [Fact]
    public void TheTurnIsReadOffTheParseAndConsultsNoClock()
    {
        var src = Path.Combine(Root(), "src");
        var engine = File.ReadAllText(Path.Combine(src, "Hamlet.RadioEngine", "Psk31", "Psk31Turn.cs"));
        var readers = new List<string>();

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
                    && !trimmed.StartsWith("///", StringComparison.Ordinal)
                    && lines[i].Contains("Psk31Turn.Read(", StringComparison.Ordinal))
                {
                    readers.Add($"{Path.GetRelativePath(src, path)}:{i + 1}: {trimmed}");
                }
            }
        }

        foreach (var reader in readers)
        {
            _output.WriteLine("a turn is decided at " + reader);
        }

        Assert.Single(readers);

        foreach (var clock in new[] { "DateTime", "Stopwatch", "Environment.TickCount", "Seconds" })
        {
            Assert.DoesNotContain(clock, engine, StringComparison.Ordinal);
        }
    }

    /// <summary>What one variant's feed measured.</summary>
    /// <param name="BlockSeconds">One block of that variant.</param>
    /// <param name="LagSeconds">The audio from the turnover word's last character to the turn changing.</param>
    private readonly record struct Lag(double BlockSeconds, double LagSeconds);

    /// <summary>
    /// Feed one recording to the real tick and measure the lag from the turnover word to the turn.
    /// </summary>
    private Lag Measure(string variant, MonoAudio audio, string mine, string his)
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

            steps.Add(((at + take) / (double)rate, text.Length, card?.TurnWord ?? ""));
        }

        var final = model.OliviaChannelsForTests
            .Select(c => c.Text)
            .OrderByDescending(t => t.Length)
            .FirstOrDefault() ?? "";

        _output.WriteLine(
            $"{variant}: one block is {format.SymbolsPerBlock} symbols of {v.SymbolSeconds:0.000} s = "
            + $"{block:0.000} s, carrying {v.BitsPerSymbol} characters");
        _output.WriteLine(
            $"{variant}: {audio.Samples.Length / (double)rate:0.0} s of audio fed a quarter-second at a "
            + $"time, {final.Length} characters read");

        Assert.NotEqual(0, final.Length);

        // When each character index first became available, from the feed steps.
        var arrived = new double[final.Length];

        for (var i = 0; i < final.Length; i++)
        {
            arrived[i] = steps.Where(s => s.Length > i).Select(s => s.Seconds).First();
        }

        // **THE FIRST MESSAGE THAT HANDS THE TURN TO THE OPERATOR**, found by the same splitter the
        // panel uses, and the character that closed it - which is the whitespace after the turnover
        // word, the splitter's own rule (unit 364 item 6).
        var splitter = new Psk31MessageSplitter(mine);
        var closedAt = -1;
        var turnoverAt = -1;

        for (var i = 0; i < final.Length; i++)
        {
            if (splitter.Add(final[i]) is not { } message)
            {
                continue;
            }

            _output.WriteLine(
                $"{variant}: \"{message.Text}\" closed at character {i}; its turnover word's last "
                + $"character arrived at {arrived[i - 1]:0.000} s and the character that closed it at "
                + $"{arrived[i]:0.000} s");

            if (closedAt < 0
                && message.Exchange is { IsCertain: true, IsForOperator: true, HandsOver: true }
                && !Ft8MessageSplit.IsSameStation(message.Exchange.Speaker, mine))
            {
                closedAt = i;
                turnoverAt = i - 1;
            }
        }

        Assert.True(closedAt > 0, variant + ": no message handed the turn to the operator");

        // When the card's own reading changed to *your turn*.
        var changedAt = steps
            .Where(s => s.Turn.StartsWith("Your turn", StringComparison.Ordinal)
                && s.Seconds >= arrived[turnoverAt])
            .Select(s => s.Seconds)
            .DefaultIfEmpty(double.NaN)
            .First();

        Assert.False(double.IsNaN(changedAt), variant + ": the card's turn never became your turn");

        var lag = changedAt - arrived[turnoverAt];

        _output.WriteLine(
            $"{variant}: the turnover word's last accepted character arrived at {arrived[turnoverAt]:0.000} s; "
            + $"the card's turn reading changed at {changedAt:0.000} s; "
            + $"LAG {lag:0.000} s = {lag / block:0.000} blocks, against one block of {block:0.000} s");

        var was = "";

        foreach (var step in steps.Where(s => s.Turn.Length > 0))
        {
            if (step.Turn == was)
            {
                continue;
            }

            was = step.Turn;

            _output.WriteLine($"{variant}: at {step.Seconds:0.000} s, {step.Length} characters, the card says \"{step.Turn}\"");
        }

        return new Lag(block, lag);
    }

    /// <summary>A panel on 20 m with the Olivia tab pressed, the tap running, and no transmit path.</summary>
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

    /// <summary>Seeded white Gaussian noise, the tail unit 364 feeds after a file.</summary>
    private static float[] Noise(int count, double rms)
    {
        var random = new Random(367);
        var samples = new float[count];

        for (var i = 0; i < count; i++)
        {
            var u1 = 1.0 - random.NextDouble();
            var u2 = random.NextDouble();

            samples[i] = (float)(rms * Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Cos(2.0 * Math.PI * u2));
        }

        return samples;
    }

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
