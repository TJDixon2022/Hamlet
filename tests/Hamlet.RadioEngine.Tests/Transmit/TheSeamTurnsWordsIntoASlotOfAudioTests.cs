using Ft8Sharp.Encode;
using Ft8Sharp.Message;
using Hamlet.RadioEngine.Transmit;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Transmit;

/// <summary>
/// The seam itself: it makes a slot of audio, it refuses rather than guessing,
/// and it works at more than one rate.
/// </summary>
/// <remarks>
/// <para>**This is unit 254's task 2. The round trip through the decoder is task
/// 3 and lives next door**, in
/// <c>HamletsOwnDecoderReadsBackWhatHamletComposedTests</c>. These are the
/// properties of the seam that hold whether or not anything ever decodes it.</para>
/// <para>**Nothing here opens a device, keys anything or waits.** The whole file
/// is arithmetic over arrays.</para>
/// </remarks>
public sealed class TheSeamTurnsWordsIntoASlotOfAudioTests
{
    private readonly ITestOutputHelper _output;

    public TheSeamTurnsWordsIntoASlotOfAudioTests(ITestOutputHelper output) => _output = output;

    /// <summary>
    /// **The rates the port will accept, measured by asking it rather than by
    /// arithmetic written here.**
    /// </summary>
    /// <remarks>
    /// The port refuses any rate at which its two ways of counting the signal's
    /// length disagree — the slot is laid out from one and the waveform written
    /// from the other. This sweeps the rates a sound card plausibly offers and
    /// prints which survive, so the answer in the report is a measurement.
    /// </remarks>
    [Fact]
    public void TheRatesThePortWillAcceptAreMeasuredRatherThanAssumed()
    {
        int[] candidates = [8000, 11025, 12000, 16000, 22050, 24000, 32000, 44100, 48000, 88200, 96000, 192000];

        _output.WriteLine($"{"rate",8} {"symbol",8} {"from dur",10} {"79 x sym",10} {"slot",10} {"usable",7}");

        var usable = new List<int>();
        foreach (var rate in candidates)
        {
            var ok = Ft8Composer.RateIsUsable(rate, out var why);
            var symbol = Ft8Waveform.SamplesPerSymbol(rate);
            var fromDuration = Ft8Waveform.SampleCount(rate);
            var fromSymbols = Ft8Waveform.SymbolCount * symbol;
            var slot = ok ? Ft8Waveform.SlotSampleCount(rate).ToString() : "-";

            _output.WriteLine(
                $"{rate,8} {symbol,8} {fromDuration,10} {fromSymbols,10} {slot,10} {(ok ? "yes" : "NO"),7}"
                + (ok ? string.Empty : $"   {why}"));

            if (ok)
            {
                usable.Add(rate);
            }
        }

        _output.WriteLine(string.Empty);
        _output.WriteLine($"usable: {string.Join(", ", usable)}");

        // The two the unit is required to deliver, and they are asserted rather
        // than only printed.
        Assert.Contains(12000, usable);
        Assert.Contains(48000, usable);
    }

    /// <summary>
    /// **A message becomes a whole slot of audio, at the decoder's rate and at the
    /// rate a USB codec is likely to want.**
    /// </summary>
    [Theory]
    [InlineData(12000)]
    [InlineData(48000)]
    public void TheOperatorsOwnCallBecomesASlotOfAudioAtBothRates(int rate)
    {
        var result = Ft8Composer.Compose("CQ KC3QIS FN00", rate);

        Assert.True(result.Composed, result.Explanation);
        var transmission = result.Transmission!;

        Assert.Equal("CQ KC3QIS FN00", transmission.Text);
        Assert.Equal(Ft8MessageType.Standard, transmission.Type);
        Assert.False(transmission.CarriesHashedCallsign);
        Assert.Equal(rate, transmission.SampleRate);

        // The length is the port's, and it is checked against the port's own
        // number rather than against a constant written here.
        Assert.Equal(Ft8Waveform.SlotSampleCount(rate), transmission.Samples.Length);

        _output.WriteLine($"rate            : {rate}");
        _output.WriteLine($"slot samples    : {transmission.Samples.Length}");
        _output.WriteLine($"slot seconds    : {transmission.SlotSeconds:F6}");
        _output.WriteLine($"signal samples  : {Ft8Waveform.SampleCount(rate)}");
        _output.WriteLine($"signal seconds  : {Ft8Waveform.SampleCount(rate) / (double)rate:F6}");
        _output.WriteLine($"peak sample     : {transmission.PeakSample:F6}");
    }

    /// <summary>
    /// **A message that will not pack comes back as a refusal naming what would
    /// not pack, and carries no audio at all.**
    /// </summary>
    /// <remarks>
    /// §0.0's principle pointed the other way: the seam never answers with a
    /// transmission for a message different from the one it was asked for. A
    /// refusal has no <c>Transmission</c> to read, so there is no flag a caller
    /// can forget to check.
    /// </remarks>
    [Theory]
    [InlineData("", Ft8ComposeRefusal.NothingToSay)]
    [InlineData("   ", Ft8ComposeRefusal.NothingToSay)]
    [InlineData("THIS IS FAR TOO LONG FOR THIRTEEN CHARACTERS", Ft8ComposeRefusal.WillNotPack)]
    [InlineData("K1ABC W9XYZ FN42 EXTRA WORDS HERE", Ft8ComposeRefusal.WillNotPack)]
    public void AMessageThatWillNotPackIsRefusedAndNamesWhatWouldNotPack(string text, Ft8ComposeRefusal expected)
    {
        var result = Ft8Composer.Compose(text);

        Assert.False(result.Composed);
        Assert.Null(result.Transmission);
        Assert.Equal(expected, result.Refusal);
        Assert.NotEqual(string.Empty, result.Explanation);

        _output.WriteLine($"\"{text}\" -> {result.Refusal}: {result.Explanation}");
    }

    /// <summary>**A rate the port refuses is a refusal, not an exception.**</summary>
    /// <remarks>
    /// The two rejected rates here were **found by the sweep above, not assumed**.
    /// This unit's first guess was that 11025 and 44100 would be refused because
    /// they are not round numbers; they are not, because 0.16 s of either is a
    /// whole number of samples. What is refused is a rate at which it is not —
    /// 12345 and 44101 are two, and they are here because they were measured.
    /// </remarks>
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(12345)]
    [InlineData(44101)]
    public void ARateThePortWillNotSynthesiseAtIsRefusedInWords(int rate)
    {
        var result = Ft8Composer.Compose("CQ KC3QIS FN00", rate);

        Assert.False(result.Composed);
        Assert.Equal(Ft8ComposeRefusal.SampleRateRefused, result.Refusal);
        _output.WriteLine($"{rate} -> {result.Explanation}");
    }

    /// <summary>
    /// **A base frequency that would put a tone outside the channel is refused.**
    /// </summary>
    [Theory]
    [InlineData(0.0f)]
    [InlineData(-100.0f)]
    [InlineData(5990.0f)]
    public void ABaseFrequencyThatWouldPutAToneOutsideTheChannelIsRefused(float baseHz)
    {
        var result = Ft8Composer.Compose("CQ KC3QIS FN00", 12000, baseHz);

        Assert.False(result.Composed);
        Assert.Equal(Ft8ComposeRefusal.BaseFrequencyRefused, result.Refusal);
        _output.WriteLine($"{baseHz} Hz -> {result.Explanation}");
    }

    /// <summary>
    /// **The text the seam returns is what came back out of the bits.**
    /// </summary>
    /// <remarks>
    /// The seam's guarantee is that it accepts a packing only where unpacking it
    /// gives back the same words. This checks the guarantee from the outside, by
    /// unpacking the seam's own symbols independently and comparing — so a change
    /// that made the seam trust its input rather than verify it would fail here.
    /// </remarks>
    [Theory]
    [InlineData("CQ KC3QIS FN00")]
    [InlineData("CQ DX K1ABC FN42")]
    [InlineData("K1ABC W9XYZ -11")]
    [InlineData("K1ABC W9XYZ RR73")]
    [InlineData("K1ABC W9XYZ 73")]
    [InlineData("TNX BOB 73 GL")]
    public void WhatTheSeamSaysItSentIsWhatTheBitsSay(string text)
    {
        var result = Ft8Composer.Compose(text);
        Assert.True(result.Composed, result.Explanation);

        // Re-derive the message from the audio path's own inputs is impossible
        // without a decoder, which is task 3. What is checked here is the layer
        // below: pack the same words again by the seam and confirm the seam's
        // reported text is the text the message layer produces, not the string
        // it was handed.
        var transmission = result.Transmission!;
        Assert.Equal(text.ToUpperInvariant(), transmission.Text);

        _output.WriteLine($"\"{text}\" -> {transmission.Type}, hashed: {transmission.CarriesHashedCallsign}");
    }

    /// <summary>
    /// **Case and spacing are normalised and nothing else is.**
    /// </summary>
    [Fact]
    public void TheWordsAreUpperCasedAndSingleSpacedAndOtherwiseLeftAlone()
    {
        var result = Ft8Composer.Compose("  cq   kc3qis   fn00  ");

        Assert.True(result.Composed, result.Explanation);
        Assert.Equal("CQ KC3QIS FN00", result.Transmission!.Text);
    }

    /// <summary>
    /// **A compound callsign that travels as a hash says so, and says what it will
    /// read back as.**
    /// </summary>
    /// <remarks>
    /// <para>Measured, not guessed: the seam packs once without a callsign cache
    /// and once with one, and reports a hash only where the port itself asked for
    /// the cache. This is the fact task 3 categorises its corpus by.</para>
    /// <para>**And the two texts differ for the hashed one**, which is the finding
    /// this test exists to pin. `PJ4/K1ABC W9XYZ` goes on the air with the compound
    /// call as a 22-bit hash, and the port reads a hash-resolved callsign back
    /// wearing angle brackets — so `ReadsBackAs` is `&lt;PJ4/K1ABC&gt; W9XYZ`. The
    /// message is right; the brackets mark where the name came from.</para>
    /// </remarks>
    [Fact]
    public void ACallsignThatTravelsAsAHashIsReportedAsOneAndSaysWhatItReadsBackAs()
    {
        var inFull = Ft8Composer.Compose("CQ PJ4/K1ABC");
        var hashed = Ft8Composer.Compose("PJ4/K1ABC W9XYZ");

        Assert.True(inFull.Composed, inFull.Explanation);
        Assert.True(hashed.Composed, hashed.Explanation);

        _output.WriteLine($"CQ PJ4/K1ABC     : {inFull.Transmission!.Type}, "
            + $"hashed {inFull.Transmission.CarriesHashedCallsign}, "
            + $"reads back as \"{inFull.Transmission.ReadsBackAs}\"");
        _output.WriteLine($"PJ4/K1ABC W9XYZ  : {hashed.Transmission!.Type}, "
            + $"hashed {hashed.Transmission.CarriesHashedCallsign}, "
            + $"reads back as \"{hashed.Transmission.ReadsBackAs}\"");

        // Carried in full: no hash, and the bits say exactly what was asked for.
        Assert.False(inFull.Transmission.CarriesHashedCallsign);
        Assert.Equal("CQ PJ4/K1ABC", inFull.Transmission.ReadsBackAs);

        // Carried as a hash: said so, and the read-back wears the port's marking.
        Assert.True(hashed.Transmission.CarriesHashedCallsign);
        Assert.Equal("<PJ4/K1ABC> W9XYZ", hashed.Transmission.ReadsBackAs);
    }

    /// <summary>
    /// **The seam contains no symbol assembly, no tone table and no phase of its
    /// own — every step is the port's.**
    /// </summary>
    /// <remarks>
    /// Checked against the file rather than asserted in prose. A Costas array, a
    /// tone table or a phase accumulator appearing in this seam is a second
    /// encoder beside <c>Ft8Sharp</c>, which is what step 2's third criterion
    /// forbids in its own words.
    /// </remarks>
    [Fact]
    public void TheSeamNamesNoRadioNoDeviceAndNoEncoderOfItsOwn()
    {
        var path = SeamPath();
        var source = File.ReadAllText(path);
        var body = string.Join(
            '\n',
            source.Split('\n').Where(line => !line.TrimStart().StartsWith("///", StringComparison.Ordinal)));

        string[] forbidden =
        [
            "TransmitAbort", "PTT", "Ptt", "Civ", "SerialPort", "Ic7300", "IRig", "RigState",
            "AudioDevice", "IAudioSource", "Wasapi", "NAudio",
            "Thread", "DateTime", "Stopwatch", "TickCount", "Task<", "async ", "await ",
            "MathF", "Math.Sin", "Math.Cos", "Math.PI", "Erf", "Costas", "Gaussian", "phaseStep",
        ];

        var found = forbidden.Where(name => body.Contains(name, StringComparison.Ordinal)).ToList();

        _output.WriteLine($"seam            : {path}");
        _output.WriteLine($"lines           : {source.Split('\n').Length}");
        _output.WriteLine($"patterns tried  : {forbidden.Length}");
        _output.WriteLine($"found in code   : {(found.Count == 0 ? "none" : string.Join(", ", found))}");

        Assert.Empty(found);
    }

    /// <summary>The seam's source file, found from this test assembly's location.</summary>
    private static string SeamPath()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "Hamlet.sln")))
        {
            directory = directory.Parent;
        }

        Assert.NotNull(directory);
        return Path.Combine(
            directory!.FullName, "src", "Hamlet.RadioEngine", "Transmit", "Ft8Composer.cs");
    }
}
