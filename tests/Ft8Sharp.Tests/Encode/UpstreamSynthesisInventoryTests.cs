using System.Text.RegularExpressions;
using Ft8Sharp.Encode;
using Xunit;
using Xunit.Abstractions;

namespace Ft8Sharp.Tests.Encode;

/// <summary>
/// The sanctioned read of the pinned clone for unit 212: what upstream's generator does to turn
/// seventy-nine channel symbols into audio, and what the WAV it writes actually is.
/// </summary>
/// <remarks>
/// <para>
/// <b>Read it before porting it, and leave behind something that fails loudly if a re-pin changes
/// it.</b> Every shape this file asserts is a shape the synthesizer in <c>src/Ft8Sharp/Encode/</c>
/// was written against. If upstream's synthesis is ever re-pinned to a different one, this goes red
/// beside the comparison rather than the comparison drifting quietly.
/// </para>
/// <para>
/// <b>Shapes and counts, never values.</b> The same discipline as
/// <see cref="ReferenceCloneMessageInventoryTests"/>: identifiers, presences, counts and structural
/// facts are printed; the smoothing parameter, the amplitude scale, the padding and the sample-rate
/// default are not. Those live in the port where the port needs them and nowhere else. The
/// protocol's published facts — seventy-nine symbols, eight tones, the symbol rate — come from the
/// QEX paper the NOTICE already cites and are free.
/// </para>
/// <para>
/// <b>Absent is a skip.</b> Nothing from the clone is committed, so a fresh clone stays green.
/// </para>
/// </remarks>
public class UpstreamSynthesisInventoryTests
{
    private readonly ITestOutputHelper _output;

    public UpstreamSynthesisInventoryTests(ITestOutputHelper output) => _output = output;

    /// <summary>The files unit 212 is licensed to read, and no others.</summary>
    private static readonly string[] SynthesisSources =
    {
        @"demo\gen_ft8.c", @"common\wave.c", @"common\wave.h", @"ft8\constants.h",
    };

    /// <summary>
    /// Task 2, question 1 and 2: is the clone here, is the generator built, and is the reference
    /// DECODER built? The third is criterion 3's literal instrument and this is the only thing in
    /// the tree that asks for it by name.
    /// </summary>
    [RequiresReferenceCloneFact]
    public void TheGeneratorIsBuiltAndWhetherTheReferenceDecoderIsTooIsRecorded()
    {
        var location = RequireReachableClone();
        _output.WriteLine($"clone                   : {location}");

        var generator = Path.Combine(location, @"build\gen_ft8.exe");
        var decoder = Path.Combine(location, @"build\decode_ft8.exe");

        _output.WriteLine($"build\\gen_ft8.exe       : {Describe(generator)}");
        _output.WriteLine($"build\\decode_ft8.exe    : {Describe(decoder)}");

        // NOT asserted either way. The decoder's absence is criterion 3's answer, not a defect, and
        // a unit may not build it — the permission scope has no rule for a compiler run and that is
        // owner-class. Its arrival on a later machine must not turn this red.
        _output.WriteLine(
            File.Exists(decoder)
                ? "criterion 3's literal instrument IS on this machine."
                : "criterion 3's literal instrument is NOT on this machine, so criterion 3 cannot be "
                  + "met on its own terms by any unit; the owner's script builds only the generator.");

        Assert.True(
            File.Exists(generator),
            $"the clone is here but nothing is built at {generator}; the comparison has no oracle.");
    }

    /// <summary>
    /// Where step 3's third exit criterion actually stands, written down where it cannot be
    /// mistaken for something else.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>The criterion says: audio synthesis produces a signal the reference decoder decodes.</b>
    /// The reference decoder is <c>decode_ft8</c>, a different program from the generator and a
    /// materially larger build — it pulls in the FFT. The owner's script builds the generator and
    /// only the generator. <b>A unit cannot build it</b>: the permission scope has no rule under
    /// which a unit runs a compiler or a batch file, which is owner-class under <c>ARBITER.md</c> §6
    /// and has been a standing note with the owner since unit 210.
    /// </para>
    /// <para>
    /// <b>So on this machine criterion 3 is NOT MET ON ITS OWN TERMS, and unit 212 does not claim
    /// it.</b> What was taken instead is two things, and neither of them is the criterion:
    /// </para>
    /// <list type="number">
    /// <item><see cref="Ft8WaveformComparisonTests"/> — every sample of fifty-one messages held
    /// against the WAV upstream's own generator writes for the same message. If our samples are
    /// upstream's samples then our audio <em>is</em> the audio every FT8 decoder already decodes,
    /// which is a stronger statement about the synthesizer than one successful decode on every
    /// point but the one below.</item>
    /// <item><see cref="Ft8WaveformTests.EverySymbolOfEveryMessageIsRecoveredBackOutOfTheWaveform"/>
    /// — the frequency measured back out of our own samples and all seventy-nine symbols recovered
    /// from it, which unlike the comparison runs on a machine with no clone.</item>
    /// </list>
    /// <para>
    /// <b>The one thing neither of them shows.</b> Nothing has demodulated this waveform — not this
    /// project, not upstream, not anybody. No candidate search has run over it, no soft symbol has
    /// been formed from it, no belief propagation has read one. That is steps 4 and 5 and this unit
    /// claims none of it.
    /// </para>
    /// <para>
    /// <b>This test passes either way and asserts nothing about the outcome</b>, because the
    /// decoder's arrival on a later machine must close the criterion rather than turn this red.
    /// </para>
    /// </remarks>
    [RequiresReferenceCloneFact]
    public void WhereCriterionThreeStandsOnThisMachineIsStatedRatherThanImplied()
    {
        var decoder = Path.Combine(RequireReachableClone(), @"build\decode_ft8.exe");
        var present = File.Exists(decoder);

        _output.WriteLine("criterion 3: audio synthesis produces a signal the reference decoder decodes.");
        _output.WriteLine($"the reference decoder   : {decoder}");
        _output.WriteLine($"built on this machine   : {present}");

        if (present)
        {
            _output.WriteLine(
                "CRITERION 3 CAN BE MET ON ITS OWN TERMS on this machine: run the decoder against a "
                + "WAV this library's synthesizer wrote and report what it decoded.");
            return;
        }

        _output.WriteLine("CRITERION 3 IS NOT MET ON ITS OWN TERMS.");
        _output.WriteLine("    the reference decoder is not built here;");
        _output.WriteLine("    building it needs a compiler run, for which the permission scope has");
        _output.WriteLine("    no rule — owner-class under ARBITER.md section 6, a standing note");
        _output.WriteLine("    with the owner since unit 210;");
        _output.WriteLine("    what was taken instead is sample-level agreement with upstream's own");
        _output.WriteLine("    WAV, plus tone recovery out of our own waveform;");
        _output.WriteLine("    and the one thing neither shows is that NO DEMODULATOR HAS BEEN RUN");
        _output.WriteLine("    AGAINST THIS WAVEFORM BY ANYBODY. That is steps 4 and 5.");
    }

    /// <summary>
    /// Task 2, question 5: the synthesis itself, read out of the pin as structure rather than as
    /// values, so that the port can be authored against it and a re-pin cannot move it silently.
    /// </summary>
    [RequiresReferenceCloneFact]
    public void UpstreamsSynthesisHasTheShapeThePortWasWrittenAgainst()
    {
        var location = RequireReachableClone();
        _output.WriteLine($"clone                   : {location}");
        _output.WriteLine("SHAPES ONLY — no constant of upstream's is printed here, by ruling.");

        foreach (var relative in SynthesisSources)
        {
            var path = Path.Combine(location, relative);
            Assert.True(File.Exists(path), $"{relative} is not in the pin; the port was written against it.");
            var text = File.ReadAllText(path);
            _output.WriteLine(
                $"{relative,-20}: present, {new FileInfo(path).Length} bytes, {CountLines(text)} lines");
        }

        var generatorSource = File.ReadAllText(Path.Combine(location, @"demo\gen_ft8.c"));

        // The generator carries its own synthesis rather than calling a library one — which is why
        // the port reads demo/ at all, and why there is no ft8/ file to point porting-notes at for
        // the waveform. Counted, not quoted.
        var synthesisFunctions = FunctionNames(generatorSource).ToList();
        _output.WriteLine($"functions in demo\\gen_ft8.c : {synthesisFunctions.Count}");
        foreach (var name in synthesisFunctions)
        {
            _output.WriteLine($"    {name}");
        }

        // The structural facts the port depends on and which a plausible reading gets wrong. Each
        // is asserted as a presence in the source, never as the value it holds.

        // One phase accumulator, updated from its own previous value once per sample and wrapped
        // rather than reset. This asserts the exact shape, not merely a mention of the variable: an
        // earlier draft accepted any for-loop that named the accumulator, which a port that
        // restarted phase at every symbol would have satisfied just as well.
        AssertShape(
            generatorSource,
            @"phi\s*=\s*fmodf\s*\(\s*phi\s*\+",
            "phase is ACCUMULATED across symbol boundaries rather than restarted per symbol",
            "a port that restarts phase produces the right length, the right frequencies and the "
            + "wrong shape, and nothing about the length or the tones would catch it");

        AssertShape(
            generatorSource,
            @"\bgfsk_pulse\b",
            "the pulse is a Gaussian-filtered frequency-shift pulse, named as such in the source",
            "the pulse shape and its smoothing parameter are what the sample comparison is most "
            + "sensitive to after the phase");

        AssertShape(
            generatorSource,
            @"\bsymbol_bt\b",
            "the smoothing parameter is a named parameter of the synthesis rather than a literal",
            "it is one of the two the port must carry rather than infer");

        AssertShape(
            generatorSource,
            @"\bsymbol_period\b",
            "the symbol timing is a named parameter of the synthesis rather than a literal",
            "it is the other one, and it sets both the length and the tone spacing");

        // The two shapes at the ends of the signal. Both are easy to leave out of a port, and
        // neither shows up in a length check or a tone recovery — they would show up in the sample
        // comparison as a difference confined to the ends, which is one of the shapes task 4 reads.
        AssertShape(
            generatorSource,
            @"[Dd]ummy symbol",
            "dummy symbols repeating the first and last tones extend the pulse past both ends",
            "without them the first and last symbols are shaped by a truncated filter and the ends "
            + "of the waveform differ");

        AssertShape(
            generatorSource,
            @"\bn_ramp\b",
            "there is an envelope ramp over part of the first and last symbol",
            "without it the transmission begins and ends on a step");

        // Where the float actually becomes an int16 — in the WAV writer, not in the generator.
        // Corrected against myself: this pair first ran against demo/gen_ft8.c, which hands floats
        // to save_wav and never sees a sixteen-bit sample, so it went red against a pin that was
        // perfectly correct. The conversion the comparison depends on is here.
        var conversionSource = File.ReadAllText(Path.Combine(location, @"common\wave.c"));
        AssertShape(
            conversionSource,
            @"\bint16_t\b",
            "the samples reach the file as int16, converted in the WAV writer",
            "the comparison against upstream's WAV is in int16 counts and needs upstream to be "
            + "producing them by its own rounding rather than ours");

        AssertShape(
            conversionSource,
            @"\(int\)\s*\(\s*0\.5\s*\+",
            "the rounding is a half added before a truncation, not a rounding function",
            "that rounding is not symmetric about zero and is not any of the framework's rounding "
            + "modes; getting it wrong costs one count on roughly half the samples of the file");

        // Whether the base frequency can be set from the command line decides whether the
        // comparison may choose one or must take upstream's default. Reported as a yes/no.
        var takesFrequencyArgument = Regex.IsMatch(
            generatorSource,
            @"argv\s*\[[^\]]*\]|getopt|atof|strtod",
            RegexOptions.None);
        _output.WriteLine($"generator parses its own arguments : {takesFrequencyArgument}");

        var waveSource = File.ReadAllText(Path.Combine(location, @"common\wave.c"));
        var waveFunctions = FunctionNames(waveSource).ToList();
        _output.WriteLine($"functions in common\\wave.c : {waveFunctions.Count}");
        foreach (var name in waveFunctions)
        {
            _output.WriteLine($"    {name}");
        }

        // The WAV writer's own shape: one canonical PCM header, mono, sixteen-bit, with the four
        // tags written a character at a time rather than as string literals — which is why these
        // patterns look for the character list and not for the word.
        AssertShape(waveSource, CharacterList("RIFF"), "the file is tagged RIFF", "WavFile checks it");
        AssertShape(waveSource, CharacterList("WAVE"), "the form is tagged WAVE", "WavFile checks it");
        AssertShape(waveSource, CharacterList("fmt "), "there is a fmt chunk", "WavFile checks it");
        AssertShape(waveSource, CharacterList("data"), "there is a data chunk", "WavFile checks it");
    }

    /// <summary>
    /// Task 2, question 6: does the generator still survive a real message, and which image
    /// answered — the original or unit 211's proven patched copy?
    /// </summary>
    /// <remarks>
    /// Deliberately <see cref="RequiresOracleFactAttribute"/> rather than the working-oracle gate:
    /// this test's whole subject is whether it works, so gating it on working would make it vanish
    /// exactly when it has something to say.
    /// </remarks>
    [RequiresOracleFact]
    public void TheGeneratorStillSurvivesARealMessageAndWritesAWav()
    {
        var (state, detail) = Ft8Oracle.ProbeUsability();
        _output.WriteLine($"usability               : {state}");
        _output.WriteLine($"detail                  : {detail}");
        _output.WriteLine($"answering image         : {Ft8Oracle.ResolvedExecutablePath}");
        _output.WriteLine(
            $"is a patched copy       : {Ft8Oracle.AnsweringImageIsAPatchedCopy}");

        var kept = Ft8Oracle.GenerateKeepingWav("CQ K1ABC FN42");
        try
        {
            _output.WriteLine($"exit code               : {kept.Run.ExitCode} (0x{kept.Run.ExitCode:X8})");
            _output.WriteLine($"wav written             : {kept.Run.WavBytes >= 0}");
            _output.WriteLine($"wav bytes               : {kept.Run.WavBytes}");

            Assert.Equal(0, kept.Run.ExitCode);
            Assert.True(kept.Run.WavBytes > 0, "the generator exited zero and wrote no WAV.");

            // Question 3: the file's own header, read out of the file, and question 4: where the
            // signal sits inside it. Both are read from the source and then checked against the
            // file, so a disagreement between the two would be loud rather than assumed away.
            var wav = WavFile.Read(kept.WavPath);
            _output.WriteLine($"sample rate             : {wav.SampleRate} Hz");
            _output.WriteLine($"bit depth               : {wav.BitsPerSample} bits");
            _output.WriteLine($"channels                : {wav.ChannelCount}");
            _output.WriteLine($"header length           : {wav.HeaderBytes} bytes");
            _output.WriteLine($"total samples           : {wav.Samples.Length}");

            var rate = wav.SampleRate;
            var lead = Ft8Waveform.PaddingSampleCount(rate);
            var signal = Ft8Waveform.SampleCount(rate);
            _output.WriteLine($"samples of silence lead : {lead}");
            _output.WriteLine($"samples of signal       : {signal}");
            _output.WriteLine($"samples of silence tail : {wav.Samples.Length - lead - signal}");
            _output.WriteLine($"file bytes the port implies : {WavFile.CanonicalHeaderBytes + (2 * Ft8Waveform.SlotSampleCount(rate))}");
            _output.WriteLine(
                $"source and file agree   : {Ft8Waveform.SlotSampleCount(rate) == wav.Samples.Length}");

            Assert.Equal(1, wav.ChannelCount);
            Assert.Equal(16, wav.BitsPerSample);
            Assert.Equal(Ft8Waveform.DefaultSampleRate, wav.SampleRate);
            Assert.Equal(WavFile.CanonicalHeaderBytes, wav.HeaderBytes);

            // The one that matters: the layout the port computed from the pin's own timing is the
            // layout of the file the pin's own program wrote. This is the alignment task 4 uses,
            // and it is READ rather than searched for.
            Assert.Equal(Ft8Waveform.SlotSampleCount(rate), wav.Samples.Length);
            Assert.Equal(
                WavFile.CanonicalHeaderBytes + (2 * Ft8Waveform.SlotSampleCount(rate)),
                (int)kept.Run.WavBytes);

            // And the silence really is silence, which is what says the padding is padding rather
            // than an offset that happens to arithmetically fit.
            for (var i = 0; i < lead; i++)
            {
                Assert.Equal(0, wav.Samples[i]);
                Assert.Equal(0, wav.Samples[wav.Samples.Length - 1 - i]);
            }
        }
        finally
        {
            WavFile.DeleteQuietly(kept.WavPath);
        }
    }

    /// <summary>
    /// A pattern matching a C initialiser that spells a tag out one character at a time.
    /// </summary>
    private static string CharacterList(string tag) =>
        string.Join(@"\s*,\s*", tag.Select(c => $"'{Regex.Escape(c.ToString())}'"));

    private void AssertShape(string source, string pattern, string what, string why)
    {
        var present = Regex.IsMatch(source, pattern, RegexOptions.None);
        _output.WriteLine($"    {(present ? "yes" : "NO "),-4} {what}");
        Assert.True(present, $"the pin no longer shows that {what} — {why}.");
    }

    private static string Describe(string path) =>
        File.Exists(path) ? $"present, {new FileInfo(path).Length} bytes" : "ABSENT";

    private string RequireReachableClone()
    {
        if (ReferenceClone.Probe(out var detail) == ReferenceClone.Reach.PresentButUnreadable)
        {
            Assert.Fail(
                $"{ReferenceClone.Location} exists but the test process could not read it: {detail}. "
                + "There is no other route to the pinned source, so nothing can be ported tonight.");
        }

        return ReferenceClone.Location;
    }

    private static int CountLines(string text) => text.Split('\n').Length;

    private static IEnumerable<string> FunctionNames(string text) =>
        Regex.Matches(
                text,
                @"^[A-Za-z_][A-Za-z0-9_ \t\*]*?\b([A-Za-z_][A-Za-z0-9_]*)\s*\([^;{]*\)\s*\{",
                RegexOptions.Multiline)
            .Select(m => m.Groups[1].Value)
            .Where(name => name is not ("if" or "for" or "while" or "switch" or "return" or "sizeof"))
            .Distinct();

    /// <summary>
    /// Prints the synthesis source so it can be ported, and only when explicitly asked to.
    /// </summary>
    /// <remarks>
    /// Off by default, keyed on its own variable, exactly as
    /// <c>ReferenceCloneMessageInventoryTests.EmitMessageSourceForPorting</c> is. A port has to read
    /// the functions it ports; a probe that printed third-party source on every run would put it
    /// into every transcript for the rest of the project's life. Nothing it prints reaches a
    /// committed file.
    /// </remarks>
    [RequiresReferenceCloneFact]
    public void EmitSynthesisSourceForPorting()
    {
        if (Environment.GetEnvironmentVariable("FT8_SYNTH_SOURCE_DUMP") != "1")
        {
            _output.WriteLine(
                "Not asked. Set FT8_SYNTH_SOURCE_DUMP=1 on the run to emit the source for porting.");
            return;
        }

        var location = RequireReachableClone();
        var only = Environment.GetEnvironmentVariable("FT8_SYNTH_SOURCE_FILE");
        var wanted = SynthesisSources.AsEnumerable();
        if (only is { Length: > 0 })
        {
            wanted = wanted.Where(r => r.Contains(only, StringComparison.OrdinalIgnoreCase));
        }

        foreach (var relative in wanted)
        {
            var path = Path.Combine(location, relative);
            _output.WriteLine($"===== {relative} =====");
            _output.WriteLine(File.Exists(path) ? File.ReadAllText(path) : "(absent)");
        }
    }
}
