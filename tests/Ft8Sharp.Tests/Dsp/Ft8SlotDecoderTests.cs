using System.Reflection;
using Ft8Sharp.Dsp;
using Ft8Sharp.Ldpc;
using Ft8Sharp.Message;
using Ft8Sharp.Tests.Encode;
using Xunit;
using Xunit.Abstractions;

namespace Ft8Sharp.Tests.Dsp;

/// <summary>
/// <b>The first text this library has ever taken out of audio</b>, measured on audio it made itself.
/// </summary>
/// <remarks>
/// <para>
/// <b>This is the localiser, and that is why it comes before the reference recordings.</b> A signal
/// placed at a known frequency and a known offset either decodes or it does not; if it does not, the
/// alignment is wrong and it is found out here rather than inside somebody else's off-air recording
/// where it could not be told apart from fading, interference or a message this library cannot read.
/// </para>
/// <para>
/// <b>Nothing is handed the answer.</b> The path takes samples and returns text. The truth appears
/// only in the assertion, after the code has answered, and
/// <see cref="ThePathsSignatureHasNowhereToPutAnAnswer"/> asserts by reflection that no parameter of
/// any public entry point is named for a message, a frequency, a time, an offset or a truth.
/// </para>
/// <para>
/// <b>Every number is printed before anything is asserted about it.</b>
/// </para>
/// </remarks>
public class Ft8SlotDecoderTests
{
    private const int Rate = Ft8WaterfallGeometry.DefaultSampleRate;

    /// <summary>The waterfall's tone spacing, for placing signals off a bin centre.</summary>
    private const double BinHz = 6.25;

    private readonly ITestOutputHelper _output;

    public Ft8SlotDecoderTests(ITestOutputHelper output) => _output = output;

    /// <summary>
    /// <b>The prohibition, one layer up again.</b> Asserted by reflection rather than by reading.
    /// </summary>
    [Fact]
    public void ThePathsSignatureHasNowhereToPutAnAnswer()
    {
        var forbidden = new[]
        {
            "freq", "hertz", "hz", "time", "offset", "expect", "hint", "truth", "text", "codeword",
        };

        foreach (var method in typeof(Ft8SlotDecoder).GetMethods(BindingFlags.Public | BindingFlags.Instance)
                     .Where(m => m.DeclaringType == typeof(Ft8SlotDecoder)))
        {
            foreach (var parameter in method.GetParameters())
            {
                var name = parameter.Name!.ToLowerInvariant();
                foreach (var word in forbidden)
                {
                    Assert.False(
                        name.Contains(word, StringComparison.Ordinal),
                        $"Ft8SlotDecoder.{method.Name} takes a parameter called '{parameter.Name}'.");
                }
            }

            _output.WriteLine($"  {method.Name}({string.Join(", ", method.GetParameters().Select(p => p.Name))})");
        }

        _output.WriteLine(string.Empty);
        _output.WriteLine("  Decode takes samples, or a waterfall. There is nowhere to pass a truth.");
    }

    /// <summary>
    /// <b>THE FIRST NUMBER: the corpus, one transmission per slot, through the whole path.</b> How
    /// many came back as exactly the message that went in, out of how many — and how many came back
    /// as something else, which is the number that matters most.
    /// </summary>
    /// <remarks>
    /// <b>The five that do not come back are counted by agreeing about the refusal rather than being
    /// excused.</b> Five of the corpus name a station by a hash, and a hash cannot be read unless the
    /// cache has heard that call in the same slot — which, in a slot holding one transmission, it
    /// has not. So the path's verdict on those five is held against what step 2's own decoder makes
    /// of the same 77 bits, and the two have to agree.
    /// </remarks>
    [Fact]
    public void EveryMessageOfTheCorpusComesBackAsItself()
    {
        var decoder = new Ft8SlotDecoder();
        var corpus = EncodeCorpus.Build();

        var frequencies = new[] { 1000.0, 1000.0 + (BinHz / 4), 1000.0 + (BinHz / 2), 1500.0 + (BinHz * 0.75) };
        var offsets = new[] { 0, 960 * 3, 1920 * 2, 5000 };

        var returned = 0;
        var wrong = 0;
        var refusedAndStepTwoAgrees = 0;
        var candidates = 0;
        var parity = 0;
        var checksum = 0;
        var text = 0;
        var unique = 0;

        _output.WriteLine($"{"message",-32} {"cand",5} {"par",4} {"crc",4} {"txt",4} {"uniq",5}  verdict");

        for (var i = 0; i < corpus.Count; i++)
        {
            var entry = corpus[i];
            var (slot, _) = SearchFixture.OneSignal(
                Rate, entry, frequencies[i % frequencies.Length], offsets[i % offsets.Length]);

            var result = decoder.Decode(slot);

            candidates += result.CandidateCount;
            parity += result.ParitySatisfiedCount;
            checksum += result.ChecksumPassedCount;
            text += result.BecameTextCount;
            unique += result.Messages.Count;

            // What step 2 makes of the same 77 bits, with no cache — the same position the path is
            // in for a slot holding one transmission.
            var stepTwo = Ft8MessageDecoder.Decode(entry.Message);

            string verdict;
            if (result.Texts.Contains(stepTwo.Text, StringComparer.Ordinal) && stepTwo.Decoded)
            {
                returned++;
                verdict = $"OK  {stepTwo.Text}";
            }
            else if (!stepTwo.Decoded && result.Messages.Count == 0)
            {
                refusedAndStepTwoAgrees++;
                verdict = $"refused, and so does step 2: {stepTwo.Status}";
            }
            else
            {
                wrong++;
                verdict = $"WRONG: returned [{string.Join(" | ", result.Texts)}] for [{stepTwo.Text}]";
            }

            _output.WriteLine(
                $"{entry.Label,-32} {result.CandidateCount,5} {result.ParitySatisfiedCount,4} "
                + $"{result.ChecksumPassedCount,4} {result.BecameTextCount,4} {result.Messages.Count,5}  {verdict}");

            // NOTHING WRONG COMES BACK, ever, from any slot.
            foreach (var message in result.Messages)
            {
                Assert.Equal(Ft8CodewordStatus.Decoded, message.Result.Status);
                Assert.NotEqual(string.Empty, message.Text);
            }
        }

        _output.WriteLine(string.Empty);
        _output.WriteLine($"  messages put in:                          {corpus.Count}");
        _output.WriteLine($"  came back as themselves:                  {returned}");
        _output.WriteLine($"  refused, and step 2 refuses them too:     {refusedAndStepTwoAgrees}");
        _output.WriteLine($"  WRONG MESSAGES RETURNED:                  {wrong} out of {corpus.Count}");
        _output.WriteLine(string.Empty);
        _output.WriteLine($"  candidates over all {corpus.Count} slots:            {candidates}");
        _output.WriteLine($"  of those, parity satisfied:               {parity}");
        _output.WriteLine($"  of those, checksum passed:                {checksum}");
        _output.WriteLine($"  of those, became text:                    {text}");
        _output.WriteLine($"  unique after de-duplication:              {unique}");
        _output.WriteLine(string.Empty);
        _output.WriteLine("  THE NUMBERS ARE PRINTED BEFORE ANY BOUND IS ASSERTED ON THEM.");

        Assert.Equal(0, wrong);
        Assert.Equal(corpus.Count, returned + refusedAndStepTwoAgrees);
    }

    /// <summary>
    /// <b>The same corpus at deliberate time and frequency offsets</b>, including a fractional bin
    /// and a fractional symbol, because a real signal is never on a bin centre and never starts on a
    /// block boundary.
    /// </summary>
    [Fact]
    public void TheRateHoldsAtEveryOffsetOnAndOffTheGrid()
    {
        var decoder = new Ft8SlotDecoder();
        var corpus = EncodeCorpus.Build().Where(e => !e.CarriesHashedCallsign).Take(12).ToArray();

        var frequencies = new (string What, double Hz)[]
        {
            ("on a bin centre", 1000.0),
            ("a quarter bin up", 1000.0 + (BinHz / 4)),
            ("exactly half a bin up", 1000.0 + (BinHz / 2)),
            ("three quarters of a bin up", 1000.0 + (BinHz * 0.75)),
        };

        var offsets = new (string What, int Samples)[]
        {
            ("on the block grid", 0),
            ("three whole blocks", 1920 * 3),
            ("five sub-blocks, off the block grid", 960 * 5),
            ("half a symbol, off both grids", 960),
            ("5000 samples, off both grids", 5000),
            ("12345 samples, off both grids", 12345),
        };

        var totalReturned = 0;
        var totalTried = 0;
        var totalWrong = 0;

        _output.WriteLine($"{"frequency",-30} {"offset",-38} {"rate",8}  wrong");

        foreach (var (frequencyWhat, hz) in frequencies)
        {
            foreach (var (offsetWhat, samples) in offsets)
            {
                var returned = 0;
                var wrong = 0;

                foreach (var entry in corpus)
                {
                    var (slot, _) = SearchFixture.OneSignal(Rate, entry, hz, samples);
                    var texts = decoder.Decode(slot).Texts;
                    var expected = Ft8MessageDecoder.Decode(entry.Message).Text;

                    if (texts.Contains(expected, StringComparer.Ordinal))
                    {
                        returned++;
                    }

                    wrong += texts.Count(t => !string.Equals(t, expected, StringComparison.Ordinal));
                }

                totalReturned += returned;
                totalWrong += wrong;
                totalTried += corpus.Length;

                _output.WriteLine(
                    $"{frequencyWhat,-30} {offsetWhat,-38} {returned,3}/{corpus.Length,-4} {wrong,6}");
            }
        }

        _output.WriteLine(string.Empty);
        _output.WriteLine($"  decoded:                {totalReturned} of {totalTried}");
        _output.WriteLine($"  WRONG MESSAGES RETURNED: {totalWrong} out of {totalTried}");
        _output.WriteLine("  THE NUMBERS ARE PRINTED BEFORE ANY BOUND IS ASSERTED ON THEM.");

        Assert.Equal(0, totalWrong);
        Assert.Equal(totalTried, totalReturned);
    }

    /// <summary>
    /// <b>The same, in seeded noise, at a delivered signal-to-noise ratio that is measured rather
    /// than requested.</b>
    /// </summary>
    /// <remarks>
    /// <b>The ratio is stated and is NOT compared with the published sensitivity figure.</b> That is
    /// step 6's measurement and this is not it — this asks only whether the path survives noise at
    /// all, at a ratio unit 214's search was already measured at.
    /// </remarks>
    [Fact]
    public void TheCorpusComesBackInSeededNoiseAtAMeasuredRatio()
    {
        const double RequestedSnr = -10.0;

        var decoder = new Ft8SlotDecoder();
        var corpus = EncodeCorpus.Build().Where(e => !e.CarriesHashedCallsign).ToArray();
        var noise = new GaussianNoise(seed: 216_004);

        var frequencies = new[] { 1000.0, 1000.0 + (BinHz / 2), 1000.0 + (BinHz / 4) };
        var offsets = new[] { 0, 1920 * 3, 960 * 5, 5000, 12345 };

        var returned = 0;
        var wrong = 0;
        var delivered = new List<double>();

        _output.WriteLine($"{"message",-32} {"delivered dB",13} {"cand",5} {"par",4} {"crc",4} {"txt",4}  verdict");

        for (var i = 0; i < corpus.Length; i++)
        {
            var entry = corpus[i];
            var frequency = frequencies[i % frequencies.Length];
            var offset = offsets[i % offsets.Length];

            var (clean, _) = SearchFixture.OneSignal(Rate, entry, frequency, offset);
            var signalPower = SearchFixture.TransmissionPower(Rate, entry, frequency);
            var sigma = SignalToNoise.NoiseAmplitudeFor(signalPower, RequestedSnr, Rate);
            var mixed = SearchFixture.AddNoise(clean, noise, sigma, out var noisePower);
            var ratio = SignalToNoise.DecibelsFor(signalPower, noisePower, Rate);
            delivered.Add(ratio);

            var result = decoder.Decode(mixed);
            var expected = Ft8MessageDecoder.Decode(entry.Message).Text;
            var got = result.Texts.Contains(expected, StringComparer.Ordinal);

            if (got)
            {
                returned++;
            }

            var extra = result.Texts.Count(t => !string.Equals(t, expected, StringComparison.Ordinal));
            wrong += extra;

            _output.WriteLine(
                $"{entry.Label,-32} {ratio,13:F3} {result.CandidateCount,5} "
                + $"{result.ParitySatisfiedCount,4} {result.ChecksumPassedCount,4} "
                + $"{result.BecameTextCount,4}  {(got ? "OK" : "MISSED")}{(extra > 0 ? $" +{extra} WRONG" : string.Empty)}");
        }

        _output.WriteLine(string.Empty);
        _output.WriteLine($"  requested ratio:          {RequestedSnr:F1} dB");
        _output.WriteLine($"  delivered, worst:         {delivered.Min():F3} dB");
        _output.WriteLine($"  delivered, best:          {delivered.Max():F3} dB");
        _output.WriteLine($"  came back as themselves:  {returned} of {corpus.Length}");
        _output.WriteLine($"  WRONG MESSAGES RETURNED:  {wrong} out of {corpus.Length}");
        _output.WriteLine(string.Empty);
        _output.WriteLine("  This ratio is STATED and is NOT compared with any published sensitivity");
        _output.WriteLine("  figure. That is step 6's measurement and this is not it.");

        Assert.Equal(0, wrong);
        Assert.Equal(corpus.Length, returned);
    }

    /// <summary>
    /// <b>Deterministic, and asserted on the messages and their order rather than on a count.</b>
    /// Two runs agreeing on <em>how many</em> while disagreeing on <em>which</em> is exactly the
    /// failure a count hides.
    /// </summary>
    [Fact]
    public void TheSameSamplesGiveTheSameMessagesInTheSameOrder()
    {
        var corpus = EncodeCorpus.Build();
        var (slot, _) = SearchFixture.ManySignals(Rate, corpus, 6, 500.0, 300.0, BinHz);

        var comparisons = 0;
        var first = new Ft8SlotDecoder().Decode(slot);

        _output.WriteLine($"  run 1 returned {first.Messages.Count} messages:");
        foreach (var message in first.Messages)
        {
            _output.WriteLine($"    {message.Text}");
        }

        // A fresh decoder, the same decoder used twice, and a decoder given the waterfall rather
        // than the samples — all three must give the same list, element for element.
        var reused = new Ft8SlotDecoder();
        var runs = new[]
        {
            new Ft8SlotDecoder().Decode(slot),
            reused.Decode(slot),
            reused.Decode(slot),
            new Ft8SlotDecoder().Decode(new Ft8Monitor().Analyse(slot)),
        };

        foreach (var run in runs)
        {
            Assert.Equal(first.Messages.Count, run.Messages.Count);
            for (var i = 0; i < first.Messages.Count; i++)
            {
                Assert.Equal(first.Messages[i].Text, run.Messages[i].Text);
                Assert.Equal(first.Messages[i].Candidate, run.Messages[i].Candidate);
                comparisons += 2;
            }

            Assert.Equal(first.CandidateCount, run.CandidateCount);
            Assert.Equal(first.ParitySatisfiedCount, run.ParitySatisfiedCount);
            Assert.Equal(first.ChecksumPassedCount, run.ChecksumPassedCount);
            Assert.Equal(first.BecameTextCount, run.BecameTextCount);
            Assert.Equal(first.DuplicateCount, run.DuplicateCount);
            comparisons += 5;
        }

        _output.WriteLine(string.Empty);
        _output.WriteLine($"  {runs.Length + 1} runs, {comparisons} VALUE comparisons, all equal -");
        _output.WriteLine("  on the text AND the candidate at each position, never on a count.");
        Assert.True(comparisons > 0);
    }

    /// <summary>
    /// The de-duplicator, watched doing its job. <b>A strong transmission produces several
    /// candidates and every one of them decodes</b>, and the list holds each message once.
    /// </summary>
    [Fact]
    public void OneTransmissionProducesManyDecodesAndExactlyOneMessage()
    {
        var entry = EncodeCorpus.Build().First(e => !e.CarriesHashedCallsign);
        var (slot, _) = SearchFixture.OneSignal(Rate, entry, 1000.0, 0);
        var result = new Ft8SlotDecoder().Decode(slot);

        _output.WriteLine($"  candidates:             {result.CandidateCount}");
        _output.WriteLine($"  parity satisfied:       {result.ParitySatisfiedCount}");
        _output.WriteLine($"  checksum passed:        {result.ChecksumPassedCount}");
        _output.WriteLine($"  became text:            {result.BecameTextCount}");
        _output.WriteLine($"  duplicates suppressed:  {result.DuplicateCount}");
        _output.WriteLine($"  unique messages:        {result.Messages.Count}");
        foreach (var text in result.Texts)
        {
            _output.WriteLine($"    {text}");
        }

        Assert.Single(result.Messages);
        Assert.Equal(result.BecameTextCount, result.Messages.Count + result.DuplicateCount);
        Assert.True(
            result.DuplicateCount > 0,
            "a strong single transmission should produce more than one decoding candidate.");
    }

    /// <summary>The path's own refusals, watched refusing.</summary>
    [Fact]
    public void ThePathRefusesWhatItCannotDo()
    {
        var refusals = new List<(string What, string Message)>();

        void Refuses(string what, Action action)
        {
            var thrown = Assert.ThrowsAny<ArgumentException>(action);
            refusals.Add((what, thrown.Message.Split('\n')[0]));
        }

        Refuses("a negative message limit", () => new Ft8SlotDecoder(messageLimit: -1));
        Refuses("a negative iteration count", () => new Ft8SlotDecoder(maxIterations: -1));
        Refuses("a null waterfall", () => new Ft8SlotDecoder().Decode((Ft8Waterfall)null!));
        Refuses("audio shorter than one block", () => new Ft8SlotDecoder().Decode(new float[1919]));

        _output.WriteLine($"{"what",-32} refusal");
        foreach (var (what, message) in refusals)
        {
            _output.WriteLine($"{what,-32} {message}");
        }

        // A message limit of zero runs the whole path and returns nothing, rather than refusing.
        var entry = EncodeCorpus.Build().First(e => !e.CarriesHashedCallsign);
        var (slot, _) = SearchFixture.OneSignal(Rate, entry, 1000.0, 0);
        var capped = new Ft8SlotDecoder(messageLimit: 0).Decode(slot);
        Assert.Empty(capped.Messages);
        Assert.True(capped.BecameTextCount > 0);
        _output.WriteLine(string.Empty);
        _output.WriteLine($"  a limit of 0 runs the path - {capped.BecameTextCount} decodes - and returns 0 messages.");
    }
}
