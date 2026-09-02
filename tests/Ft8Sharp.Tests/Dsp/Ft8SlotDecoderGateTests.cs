using Ft8Sharp;
using Ft8Sharp.Dsp;
using Ft8Sharp.Encode;
using Ft8Sharp.Ldpc;
using Ft8Sharp.Message;
using Ft8Sharp.Tests.Encode;
using Xunit;
using Xunit.Abstractions;

namespace Ft8Sharp.Tests.Dsp;

/// <summary>
/// <b>Step 5, criterion 2: a candidate failing CRC is never returned as a decode, however tempting
/// the partial — re-taken IN THE CANDIDATE SENSE.</b>
/// </summary>
/// <remarks>
/// <para>
/// <b>What changed since unit 215.</b> That unit met this criterion <em>at the codeword entry
/// point</em> and said so in those words, because extraction did not exist and nothing that had ever
/// been near a radio could reach the gate: its 5096 wrong-checksum codewords were arrays of ratios
/// built in the test project. <b>A candidate exists now.</b> Everything below goes in as
/// <em>audio</em>, is analysed into a waterfall, is found by the search without being told where it
/// is, is extracted, normalised and handed to the gate — the whole path — and the question is how
/// many messages came out the other end.
/// </para>
/// <para>
/// <b>The third of the four is the one the criterion is actually about.</b> A genuine transmission,
/// synthesized from a codeword whose checksum was made wrong <em>before</em> the parity bits were
/// computed. So the sync tones are real, the candidate is real, belief propagation converges on a
/// perfectly valid member of the code in almost no iterations with zero unsatisfied checks — and only
/// the checksum knows. That is the tempting partial, and it is what HM-DEC-009 exists for.
/// </para>
/// <para><b>Every number is printed before anything is asserted about it.</b></para>
/// </remarks>
public class Ft8SlotDecoderGateTests
{
    private const int Rate = Ft8WaterfallGeometry.DefaultSampleRate;

    private readonly ITestOutputHelper _output;

    public Ft8SlotDecoderGateTests(ITestOutputHelper output) => _output = output;

    /// <summary>ONE. An empty slot. Candidates, and messages, which must be none.</summary>
    [Fact]
    public void AnEmptySlotReturnsNoMessages()
    {
        var result = new Ft8SlotDecoder().Decode(SearchFixture.EmptySlot(Rate));

        _output.WriteLine($"  candidates:         {result.CandidateCount}");
        _output.WriteLine($"  parity satisfied:   {result.ParitySatisfiedCount}");
        _output.WriteLine($"  checksum passed:    {result.ChecksumPassedCount}");
        _output.WriteLine($"  became text:        {result.BecameTextCount}");
        _output.WriteLine($"  MESSAGES RETURNED:  {result.Messages.Count}");

        Assert.Empty(result.Messages);
    }

    /// <summary>
    /// TWO. Seeded Gaussian noise alone, with no transmission under it, over twenty slots at twenty
    /// stated seeds. <b>The search will find candidates in noise; it is supposed to. The question is
    /// how many became text.</b>
    /// </summary>
    [Fact]
    public void NoiseAloneReturnsNoMessagesOverTwentySlots()
    {
        const int Slots = 20;
        const int FirstSeed = 216_601;
        const double Amplitude = 0.02;

        var decoder = new Ft8SlotDecoder();
        var length = SearchFixture.EmptySlot(Rate).Length;

        var candidates = 0;
        var parity = 0;
        var checksum = 0;
        var text = 0;
        var messages = 0;
        var bestScore = int.MinValue;
        var closest = int.MaxValue;

        _output.WriteLine($"{"seed",8} {"cand",5} {"top score",10} {"par",4} {"crc",4} {"txt",4} {"messages",9}");

        for (var slot = 0; slot < Slots; slot++)
        {
            var seed = FirstSeed + slot;
            var noise = new GaussianNoise(seed).Block(length, Amplitude);
            var result = decoder.Decode(noise);

            var top = result.CandidateCount == 0 ? 0 : new Ft8SyncSearch()
                .Find(new Ft8Monitor().Analyse(noise))[0].Score;

            candidates += result.CandidateCount;
            parity += result.ParitySatisfiedCount;
            checksum += result.ChecksumPassedCount;
            text += result.BecameTextCount;
            messages += result.Messages.Count;
            bestScore = Math.Max(bestScore, top);
            closest = Math.Min(closest, result.ParitySatisfiedCount == 0 ? closest : 0);

            _output.WriteLine(
                $"{seed,8} {result.CandidateCount,5} {top,10} {result.ParitySatisfiedCount,4} "
                + $"{result.ChecksumPassedCount,4} {result.BecameTextCount,4} {result.Messages.Count,9}");

            foreach (var message in result.Messages)
            {
                _output.WriteLine($"           returned: {message.Text}");
            }
        }

        _output.WriteLine(string.Empty);
        _output.WriteLine($"  slots:                        {Slots}");
        _output.WriteLine($"  noise amplitude (rms):        {Amplitude}");
        _output.WriteLine($"  candidates found in noise:    {candidates}");
        _output.WriteLine($"  best sync score in any slot:  {bestScore}");
        _output.WriteLine($"  of those, parity satisfied:   {parity}");
        _output.WriteLine($"  of those, checksum passed:    {checksum}");
        _output.WriteLine($"  of those, became text:        {text}");
        _output.WriteLine($"  MESSAGES RETURNED:            {messages} out of {candidates} candidates");
        _output.WriteLine(string.Empty);
        _output.WriteLine("  The search finding candidates in noise is correct and expected. The gate");
        _output.WriteLine("  is what stands between a candidate and a message on a screen.");

        Assert.True(candidates > 0, "the search should find candidates in noise; that is not the question.");
        Assert.Equal(0, messages);
    }

    /// <summary>
    /// THREE, AND THIS IS THE ONE THE CRITERION IS ABOUT. Genuine transmissions whose checksum was
    /// made wrong before the parity bits were computed, synthesized into audio and put through the
    /// whole path. <b>The sync is real, the candidate is real, the codeword is a genuine member of
    /// the code, and only the checksum knows.</b>
    /// </summary>
    [Fact]
    public void ATransmissionCarryingAWrongChecksumReturnsNothing()
    {
        var decoder = new Ft8SlotDecoder();
        var corpus = EncodeCorpus.Build().Where(e => !e.CarriesHashedCallsign).ToArray();

        var tried = 0;
        var returnedSomething = 0;
        var candidates = 0;
        var parity = 0;
        var checksum = 0;
        var text = 0;
        var minIterations = int.MaxValue;
        var unsatisfiedAtParity = new List<int>();

        _output.WriteLine($"{"message",-32} {"bit",4} {"cand",5} {"par",4} {"crc",4} {"txt",4} {"returned",9}");

        for (var i = 0; i < corpus.Length; i++)
        {
            var entry = corpus[i];

            // Rotate which message bit is altered, so this is not one bit position measured many
            // times. The bit is inside the 77-bit message, so the checksum that travels with it is
            // the checksum of the message BEFORE the alteration.
            var alteredBit = (i * 7) % Ft8Payload.MessageBits;
            var symbols = WrongChecksumSymbols(entry.Message, alteredBit);
            var slot = SearchFixture.EmptySlot(Rate);
            var signal = Ft8Waveform.Synthesize(symbols, Rate, 1000.0f + (i % 5 * 137.0f));
            signal.CopyTo(slot, 0);

            var result = decoder.Decode(slot);

            tried++;
            candidates += result.CandidateCount;
            parity += result.ParitySatisfiedCount;
            checksum += result.ChecksumPassedCount;
            text += result.BecameTextCount;
            if (result.Messages.Count > 0)
            {
                returnedSomething++;
            }

            // Proof that the codeword really is a valid member of the code, and that belief
            // propagation finds it: the same ratios through the gate, with the correction's own
            // report of how many checks were unsatisfied.
            var waterfall = new Ft8Monitor().Analyse(slot);
            var best = new Ft8SyncSearch().Find(waterfall)[0];
            var ratios = new float[Ft8SoftSymbols.RatioCount];
            Ft8SoftSymbols.Extract(waterfall, best, ratios);
            Ft8SoftSymbols.Normalise(ratios);
            var gate = Ft8CodewordDecoder.Decode(ratios);
            unsatisfiedAtParity.Add(gate.Correction.UnsatisfiedChecks);
            minIterations = Math.Min(minIterations, gate.Correction.Iterations);

            _output.WriteLine(
                $"{entry.Label,-32} {alteredBit,4} {result.CandidateCount,5} "
                + $"{result.ParitySatisfiedCount,4} {result.ChecksumPassedCount,4} "
                + $"{result.BecameTextCount,4} {result.Messages.Count,9}"
                + $"   gate: {gate.Status}, {gate.Correction.UnsatisfiedChecks} checks unsatisfied");

            Assert.Empty(result.Messages);
        }

        _output.WriteLine(string.Empty);
        _output.WriteLine($"  transmissions tried:              {tried}");
        _output.WriteLine($"  candidates over all of them:      {candidates}");
        _output.WriteLine($"  of those, parity satisfied:       {parity}");
        _output.WriteLine($"  of those, CHECKSUM PASSED:        {checksum}");
        _output.WriteLine($"  of those, became text:            {text}");
        _output.WriteLine($"  TRANSMISSIONS THAT RETURNED ANYTHING AT ALL: {returnedSomething} of {tried}");
        _output.WriteLine(string.Empty);
        _output.WriteLine($"  at the best candidate, unsatisfied checks: "
            + $"min {unsatisfiedAtParity.Min()}, max {unsatisfiedAtParity.Max()}");
        _output.WriteLine($"  fewest iterations any of them needed:      {minIterations}");
        _output.WriteLine("  Zero unsatisfied checks is the point: EVERY ONE OF THESE IS A GENUINE");
        _output.WriteLine("  CODEWORD that belief propagation accepts. The parity gate has nothing to");
        _output.WriteLine("  object to. Only the checksum knows, and it is the checksum that refuses.");

        Assert.Equal(0, returnedSomething);
        Assert.Equal(0, text);
    }

    /// <summary>
    /// FOUR. A transmission at a signal-to-noise ratio far below anything that decodes. <b>Not
    /// <em>did it decode</em> — did it ever return the wrong text.</b>
    /// </summary>
    [Fact]
    public void ATransmissionFarBelowTheDecodableLevelNeverReturnsTheWrongText()
    {
        const double RequestedSnr = -30.0;

        var decoder = new Ft8SlotDecoder();
        var corpus = EncodeCorpus.Build().Where(e => !e.CarriesHashedCallsign).ToArray();
        var noise = new GaussianNoise(seed: 216_604);

        var tried = 0;
        var decoded = 0;
        var wrong = 0;
        var candidates = 0;
        var parity = 0;
        var delivered = new List<double>();

        _output.WriteLine($"{"message",-32} {"delivered dB",13} {"cand",5} {"par",4} {"txt",4}  returned");

        for (var i = 0; i < corpus.Length; i++)
        {
            var entry = corpus[i];
            var frequency = 1000.0 + (i % 7 * 211.0);

            var (clean, _) = SearchFixture.OneSignal(Rate, entry, frequency, 0);
            var signalPower = SearchFixture.TransmissionPower(Rate, entry, frequency);
            var sigma = SignalToNoise.NoiseAmplitudeFor(signalPower, RequestedSnr, Rate);
            var mixed = SearchFixture.AddNoise(clean, noise, sigma, out var noisePower);
            delivered.Add(SignalToNoise.DecibelsFor(signalPower, noisePower, Rate));

            var result = decoder.Decode(mixed);
            var expected = Ft8MessageDecoder.Decode(entry.Message).Text;

            tried++;
            candidates += result.CandidateCount;
            parity += result.ParitySatisfiedCount;

            foreach (var message in result.Messages)
            {
                if (string.Equals(message.Text, expected, StringComparison.Ordinal))
                {
                    decoded++;
                }
                else
                {
                    wrong++;
                    _output.WriteLine($"    WRONG TEXT: [{message.Text}] where [{expected}] was sent");
                }
            }

            _output.WriteLine(
                $"{entry.Label,-32} {delivered[^1],13:F3} {result.CandidateCount,5} "
                + $"{result.ParitySatisfiedCount,4} {result.BecameTextCount,4}  "
                + $"{(result.Messages.Count == 0 ? "nothing" : string.Join(" | ", result.Texts))}");
        }

        _output.WriteLine(string.Empty);
        _output.WriteLine($"  transmissions:              {tried}");
        _output.WriteLine($"  requested ratio:            {RequestedSnr:F1} dB");
        _output.WriteLine($"  delivered, worst to best:   {delivered.Min():F3} to {delivered.Max():F3} dB");
        _output.WriteLine($"  candidates found:           {candidates}");
        _output.WriteLine($"  of those, parity satisfied: {parity}");
        _output.WriteLine($"  decoded correctly:          {decoded} of {tried}");
        _output.WriteLine($"  WRONG TEXT RETURNED:        {wrong} out of {tried}");
        _output.WriteLine(string.Empty);
        _output.WriteLine("  This is NOT a sensitivity measurement and the ratio is not compared with");
        _output.WriteLine("  any published figure. Whether anything decodes here is not the question.");
        _output.WriteLine("  The question is whether anything WRONG came back, and the answer is a count.");

        Assert.Equal(0, wrong);
    }

    /// <summary>
    /// The 79 channel symbols of a transmission whose 77-bit message was altered <em>after</em> its
    /// checksum was computed — so the codeword is a genuine member of the code and its checksum is
    /// somebody else's.
    /// </summary>
    /// <remarks>
    /// <b>The layout is checked against the library's own encoder rather than trusted.</b> Laying a
    /// codeword out across the data symbols is <see cref="Ft8SymbolEncoder"/>'s job and its
    /// implementation is private, so this reproduces it — and <see cref="TheLayoutUsedHereIsTheOne"/>
    /// asserts that reproducing it with an <em>unaltered</em> payload gives exactly what
    /// <c>Ft8SymbolEncoder.Encode</c> gives, for the whole corpus. A fixture that quietly laid the
    /// bits out differently would be measuring nothing.
    /// </remarks>
    private static byte[] WrongChecksumSymbols(ReadOnlySpan<byte> message, int alteredBit)
    {
        Span<byte> payload = stackalloc byte[Ft8Payload.PayloadBytes];
        Ft8Payload.Create(message, payload);

        if (alteredBit >= 0)
        {
            // Inside the 77 message bits, so the 14 checksum bits that follow are now the checksum
            // of a message that is not the one being sent.
            payload[alteredBit / 8] ^= (byte)(0x80u >> (alteredBit % 8));
        }

        var codeword = new byte[LdpcEncoder.CodewordBytes];
        LdpcEncoder.Encode(payload, codeword);

        return LayOut(codeword);
    }

    /// <summary>Upstream's layout: three bits per data symbol, through the Gray map, with the
    /// Costas blocks dropped in and consuming no codeword bit.</summary>
    private static byte[] LayOut(ReadOnlySpan<byte> codeword)
    {
        var symbols = new byte[Ft8SymbolEncoder.SymbolCount];
        var costas = Ft8Tables.Ft8CostasPattern;
        var gray = Ft8Tables.Ft8GrayMap;

        var bit = 0;
        for (var symbol = 0; symbol < symbols.Length; symbol++)
        {
            if (Ft8SymbolEncoder.IsSyncSymbol(symbol))
            {
                symbols[symbol] = costas[symbol % Ft8SymbolEncoder.SyncBlockOffset];
                continue;
            }

            var value = 0;
            for (var b = 0; b < Ft8SymbolEncoder.BitsPerSymbol; b++)
            {
                var set = (codeword[bit / 8] >> (7 - (bit % 8))) & 1;
                value = (value << 1) | set;
                bit++;
            }

            symbols[symbol] = gray[value];
        }

        return symbols;
    }

    /// <summary>
    /// The fixture's layout is the library's, for every message of the corpus. <b>Without this the
    /// wrong-checksum test would prove nothing</b>, because a fixture that laid the bits out
    /// differently would produce a transmission that fails for a reason nobody intended.
    /// </summary>
    [Fact]
    public void TheLayoutUsedHereIsTheOne()
    {
        var corpus = EncodeCorpus.Build();
        var comparisons = 0;

        foreach (var entry in corpus)
        {
            var mine = WrongChecksumSymbols(entry.Message, -1);
            var theirs = Ft8SymbolEncoder.Encode(entry.Message);

            Assert.Equal(theirs.Length, mine.Length);
            for (var i = 0; i < theirs.Length; i++)
            {
                Assert.Equal(theirs[i], mine[i]);
                comparisons++;
            }
        }

        _output.WriteLine($"  {corpus.Count} messages, {comparisons} SYMBOL comparisons, all equal.");
        _output.WriteLine("  So the only thing wrong with a wrong-checksum transmission is its checksum.");
    }
}
