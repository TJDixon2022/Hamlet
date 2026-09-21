using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.Json;
using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Olivia;
using Hamlet.RadioEngine.Psk31;
using Hamlet.RadioEngine.Transmit;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Olivia;

/// <summary>
/// Work instruction 365: **Hamlet's own Olivia signal, read back by Hamlet's own ears** (step 4
/// criterion 4.1).
/// </summary>
/// <remarks>
/// <para>**THE DEMODULATOR IS NOT TOUCHED** (§10). A modulator that loops back only because the
/// demodulator was changed has proved nothing, so the reader here is the one the author's fixtures
/// proved.</para>
/// <para>**COMPUTED, NOT SEEN**, and nothing here is evidence about the radio (FACT-004).</para>
/// </remarks>
public sealed class TheOliviaModulatorTests
{
    /// <summary>The rate the demodulator reads at, and the author's fixtures were made at.</summary>
    private const int Rate = 8000;

    /// <summary>The author's fixture center (`manifest.json`), and one 500 Hz above it (decision AR).</summary>
    private static readonly double[] Centers = [1000, 1500];

    /// <summary>The typed line: at least 60 characters, framed as the app frames it (decision AR).</summary>
    private const string TypedLine = "Thanks for the call, running 100 watts to a dipole at 30 feet here in PA";

    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where each reading is printed.</param>
    public TheOliviaModulatorTests(ITestOutputHelper output) => _output = output;

    private static OliviaFormat Format
        => OliviaData.Current.Format ?? throw new InvalidOperationException(OliviaData.Current.Problem);

    /// <summary>The four PSK31 macros and one typed line, framed with the test fields (decision AR).</summary>
    internal static (string Name, string Text)[] Texts()
    {
        Assert.True(TypedLine.Length >= 60, $"the typed line is {TypedLine.Length} characters");

        return
        [
            .. Unit365Trace.Macros(),
            ("typed", Psk31Macros.Typed(Unit365Trace.His, Unit365Trace.Mine, TypedLine)),
        ];
    }

    /// <summary>
    /// **4.1, first half: each macro and a typed line, modulated by Hamlet at each variant and at
    /// two centers, decodes through Hamlet's demodulator to exactly the text that went in.**
    /// </summary>
    /// <param name="variant">The variant.</param>
    /// <remarks>
    /// **THE VARIANT AND THE CENTER COME FROM THE DETECTOR, NEVER FROM THE TEST** (decision AT,
    /// carrying decision I forward). The text is composed with its burst in front, the detector
    /// reads the burst off that audio, and what it says is what the demodulator is built with; the
    /// variant the loop asked for only checks the detection.
    /// </remarks>
    /// <remarks>
    /// **ALL SEVEN SINCE UNIT 377** (`PHASE_PLAN.md` R41, criterion 2.2). It was 8/250, 16/500 and
    /// 32/1000 - the three whose RSID code carried a tone sequence, so the three
    /// <see cref="OliviaModulator.Compose"/> would make at all. The one RSID file carries a
    /// sequence for every code, and R41 makes THIS NAME the gate: a variant is marked
    /// `proved_by_loopback` in `data/olivia/format.json` only because these cases came back
    /// identical, and a variant that fails here stays false and stays off the air. **Nothing else
    /// about the name's shape moved** - the variant and the center are still read off the burst by
    /// the detector and never taken from the test (decision AT), and identical still means
    /// character for character after `Unify` with no tolerance of any kind.
    /// </remarks>
    [Theory]
    [InlineData("4/250")]
    [InlineData("4/500")]
    [InlineData("8/250")]
    [InlineData("8/500")]
    [InlineData("16/500")]
    [InlineData("16/1000")]
    [InlineData("32/1000")]
    public void EachMacroAndATypedLineComeBackIdentical(string variant)
    {
        var identical = 0;
        var texts = Texts();

        foreach (var center in Centers)
        {
            foreach (var (name, text) in texts)
            {
                var composed = OliviaModulator.Compose(text, variant, center, Rate, Ft8Composer.DefaultDrivePeak);
                var announced = Announcement(composed);

                Assert.Equal(variant, announced.Variant);
                Assert.InRange(announced.CenterHz, center - 5, center + 5);

                var samples = composed.Samples;
                var demodulator = new OliviaDemodulator(Format, Format.Variant(announced.Variant)!, announced.CenterHz, Rate);
                var before = Process.GetCurrentProcess().TotalProcessorTime;
                var decoding = demodulator.Decode(new MonoAudio(Rate, samples), BurstEnd(announced));
                var cpu = (Process.GetCurrentProcess().TotalProcessorTime - before).TotalSeconds;
                var same = string.Equals(Unify(decoding.Text), Unify(text), StringComparison.Ordinal);

                identical += same ? 1 : 0;

                _output.WriteLine(
                    $"{variant} at {center:0} Hz, {name}: {text.Length} characters in, {decoding.CharactersOut} out, "
                    + $"{(same ? "identical" : "DIFFERENT")}; announced {announced.Name} ({announced.Code}) at "
                    + $"{announced.CenterHz:0.00} Hz, {announced.TonesRight} tones right; {samples.Length / (double)Rate:0.000} s of audio, "
                    + $"blocks {decoding.BlocksDecoded} decoded, {decoding.BlocksRejected} rejected; decode cpu {cpu:0.000} s");

                if (!same)
                {
                    _output.WriteLine("  sent : " + JsonSerializer.Serialize(text));
                    _output.WriteLine("  read : " + JsonSerializer.Serialize(decoding.Text));
                }
            }
        }

        _output.WriteLine($"{variant}: {identical} of {texts.Length * Centers.Length} identical");

        Assert.Equal(texts.Length * Centers.Length, identical);
    }

    /// <summary>
    /// **4.1, second half: Hamlet's signal measures like the mode author's**, by the one measuring
    /// code for both (decision AS), within tolerances stated before measuring.
    /// </summary>
    /// <param name="file">The author's clean fixture for the variant.</param>
    /// <remarks>
    /// <para>**LIKE FOR LIKE.** Hamlet composes the fixture's own text, from `manifest.json`, at the
    /// variant and center the fixture's burst announces, at the fixture's 8000 Hz, and
    /// <see cref="OliviaSignalMeasure"/> measures both recordings.</para>
    /// <para>**THE TOLERANCES ARE THE AUTHOR'S NUMBERS AND ARE NOT LOOSENED** (decision AS): tone
    /// spacing and symbol rate within 0.5%, the 99% bandwidth within 5%, the preamble within one
    /// symbol.</para>
    /// </remarks>
    [Theory]
    [InlineData("olivia-8-250-cq-rsid.wav")]
    [InlineData("olivia-16-500-qso-rsid.wav")]
    [InlineData("olivia-32-1000-qso-rsid.wav")]
    public void TheSignalMatchesTheAuthorsAudio(string file)
    {
        var fixture = OliviaFixtures.Load(file);
        var authorAudio = WavAudio.Read(fixture.Path);
        var author = OliviaSignalMeasure.Measure(authorAudio);
        var heard = Hamlet.RadioEngine.Rsid.RsidDetector.Detect(
            OliviaData.Current.Rsid!, authorAudio, Psk31CarrierSearch.PassbandLowHz, Psk31CarrierSearch.PassbandHighHz);
        var announced = Assert.Single(heard);
        var variant = Format.Variant(announced.Variant)!;
        var composed = OliviaModulator.Compose(fixture.Text, variant.Name, fixture.CenterHz, authorAudio.SampleRate, Ft8Composer.DefaultDrivePeak);
        var hamlet = OliviaSignalMeasure.Measure(new MonoAudio(composed.SampleRate, composed.Samples));

        var rows = new (string Quantity, double Author, double Hamlet, double Limit, string LimitText)[]
        {
            ("tone spacing Hz", author.ToneSpacingHz, hamlet.ToneSpacingHz, 0.005 * author.ToneSpacingHz, "0.5%"),
            ("symbol rate Hz", author.SymbolRateHz, hamlet.SymbolRateHz, 0.005 * author.SymbolRateHz, "0.5%"),
            ("99% bandwidth Hz", author.OccupiedBandwidthHz, hamlet.OccupiedBandwidthHz, 0.05 * author.OccupiedBandwidthHz, "5%"),
            ("preamble s", author.PreambleSeconds, hamlet.PreambleSeconds, variant.SymbolSeconds, "one symbol"),
        };

        _output.WriteLine($"{variant.Name}: text of {fixture.Text.Length} characters at {fixture.CenterHz:0} Hz, {authorAudio.SampleRate} Hz; "
            + $"tones found: author {author.Tones}, Hamlet {hamlet.Tones}; burst: author {author.BurstCode} at {author.BurstCenterHz:0.00} Hz, "
            + $"Hamlet {hamlet.BurstCode} at {hamlet.BurstCenterHz:0.00} Hz; data: author {author.DataSeconds:0.000} s, Hamlet {hamlet.DataSeconds:0.000} s");
        _output.WriteLine("variant | quantity | author | Hamlet | difference | tolerance");

        foreach (var (quantity, a, h, limit, limitText) in rows)
        {
            _output.WriteLine($"{variant.Name} | {quantity} | {a:0.0000} | {h:0.0000} | {h - a:+0.0000;-0.0000;0.0000} ({(h - a) / a * 100:+0.00;-0.00;0.00}%) | {limitText} ({limit:0.0000})");
        }

        Assert.Equal(author.Tones, hamlet.Tones);

        foreach (var (quantity, a, h, limit, _) in rows)
        {
            Assert.True(Math.Abs(h - a) <= limit, $"{variant.Name} {quantity}: author {a:0.0000}, Hamlet {h:0.0000}, over {limit:0.0000}");
        }
    }

    /// <summary>
    /// **4.2 at the engine: every composed send begins with the burst naming its own variant**
    /// (decision AU), read back by <see cref="Hamlet.RadioEngine.Rsid.RsidDetector"/> off the audio.
    /// </summary>
    /// <param name="variant">The variant.</param>
    /// <remarks>
    /// **THE ENGINE HALF, AND IT IS SAID AS THAT.** *Every send* is proved when a send exists; the
    /// application's gate still refuses Olivia, so what is proved here is every transmission this
    /// engine composes.
    /// </remarks>
    [Theory]
    [InlineData("8/250")]
    [InlineData("16/500")]
    [InlineData("32/1000")]
    public void EverySendBeginsWithItsRsid(string variant)
    {
        var codes = OliviaData.Current.Rsid!;

        foreach (var center in Centers)
        {
            var composed = OliviaModulator.Compose(
                Psk31Macros.Cq(Unit365Trace.Mine), variant, center, Rate, Ft8Composer.DefaultDrivePeak);
            var announced = Announcement(composed);
            var burstSamples = Hamlet.RadioEngine.Rsid.RsidBurst.LengthInSamples(codes, Rate);

            _output.WriteLine(
                $"{variant} at {center:0} Hz: announced {announced.Name} ({announced.Code}) at {announced.CenterHz:0.00} Hz, "
                + $"{announced.TonesRight} of {codes.Symbols} tones right, quality {announced.Quality:0.000}; "
                + $"record says announced {composed.Announced} code {composed.AnnouncedCode}; "
                + $"announcement {composed.AnnouncementSeconds:0.000} s of {composed.Seconds:0.000} s, "
                + $"text {composed.TextSeconds:0.000} s against a cap of {composed.Cap:0.000} s; fit {composed.Fit}");

            // The burst is the variant's own, where the composer said it was, and read as that variant.
            Assert.Equal(OliviaModulator.AnnouncedAs(variant), announced.Name);
            Assert.Equal(codes.CodeOf(OliviaModulator.AnnouncedAs(variant)), announced.Code);
            Assert.Equal(variant, announced.Variant);
            Assert.InRange(announced.CenterHz, center - 5, center + 5);
            Assert.Equal(codes.Symbols, announced.TonesRight);

            // And the record says a send was announced, with the code and that burst's own length.
            Assert.True(composed.Announced);
            Assert.Equal(announced.Code, composed.AnnouncedCode);
            Assert.Equal(burstSamples, composed.AnnouncementSamples);
            Assert.Equal(UnslottedMode.Olivia, composed.Mode);
            Assert.Equal(UnslottedFit.Fits, composed.Fit);
        }
    }

    /// <summary>What the detector reads off the front of a composed transmission.</summary>
    /// <remarks>
    /// **ONLY THE FRONT IS FED**, because that is where a burst is and the detector's cost is the
    /// audio's length: the burst, its pause, and a second more, then the stream is closed.
    /// </remarks>
    private static Hamlet.RadioEngine.Rsid.RsidDetection Announcement(UnslottedTransmission composed)
    {
        var codes = OliviaData.Current.Rsid!;
        var detector = new Hamlet.RadioEngine.Rsid.RsidDetector(
            codes, composed.SampleRate, Psk31CarrierSearch.PassbandLowHz, Psk31CarrierSearch.PassbandHighHz);
        var leading = Math.Min(
            composed.Samples.Length,
            (2 * Hamlet.RadioEngine.Rsid.RsidBurst.LengthInSamples(codes, composed.SampleRate)) + composed.SampleRate);
        var heard = new List<Hamlet.RadioEngine.Rsid.RsidDetection>(detector.Feed(composed.Samples.AsSpan(0, leading)));

        heard.AddRange(detector.Flush());

        return Assert.Single(heard);
    }

    /// <summary>Where the burst's tones end, which is where the reading begins.</summary>
    private static double BurstEnd(Hamlet.RadioEngine.Rsid.RsidDetection heard)
        => heard.StartSeconds + (OliviaData.Current.Rsid!.Symbols / OliviaData.Current.Rsid!.SymbolRateHz);

    /// <summary>Decision J's line-ending unification: CR LF and a lone CR become LF.</summary>
    private static string Unify(string text)
        => text.Replace("\r\n", "\n", StringComparison.Ordinal).Replace('\r', '\n');
}
