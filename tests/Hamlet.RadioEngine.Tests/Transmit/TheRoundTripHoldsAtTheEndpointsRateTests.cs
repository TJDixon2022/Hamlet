using Ft8Sharp.Dsp;
using Ft8Sharp.Encode;
using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Transmit;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Transmit;

/// <summary>
/// **A signal composed at the endpoint's rate is still FT8.** Work instruction
/// 262, task 4.
/// </summary>
/// <remarks>
/// <para>**THE RATE IS WHAT IS UNPROVED HERE, NOT THE CORPUS.** Step 2 already
/// proved more than a hundred messages at the decoder's own 12000 Hz in
/// <see cref="HamletsOwnDecoderReadsBackWhatHamletComposedTests"/>. What has
/// never been measured is whether the same messages survive being built at the
/// rate the send path now actually uses - so the corpus is held to a dozen and
/// the rate is varied instead.</para>
/// <para>**IT REUSES THAT TEST'S CONSTRUCTION RATHER THAN WRITING A SECOND
/// ENCODER** (step 2, criterion 3): <c>Ft8Composer.Compose</c> for the padded slot
/// a decoder is handed, <c>Ft8SlotDecoder.Decode</c> for the reading, and
/// <c>Ft8Transmission.ReadsBackAs</c> for what counts as the same message.</para>
/// <para>**THE RESAMPLE IS THE RECEIVE PATH'S OWN AND IS NOT A CONCESSION.**
/// <c>Ft8SlotDecoder</c> reads at 12000 Hz; a transmission that goes out at 48000
/// reaches any receiver through a capture endpoint and
/// <c>Ft8Resample.ToFt8Rate</c>, which is exactly what
/// <c>Ft8Reception</c> does at line 492 on every slot Hamlet hears. Decoding a
/// 48000 Hz array directly would measure something no receiver ever does.</para>
/// <para>**NOTHING HERE OPENS A DEVICE, KEYS ANYTHING OR READS A CLOCK.**</para>
/// </remarks>
public sealed class TheRoundTripHoldsAtTheEndpointsRateTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the table is printed.</param>
    public TheRoundTripHoldsAtTheEndpointsRateTests(ITestOutputHelper output)
        => _output = output;

    /// <summary>
    /// The rates this is measured at, and why each one is in the list.
    /// </summary>
    /// <remarks>
    /// **12000** is the decoder's own and the rate step 2 proved the corpus at, so
    /// it is the control - a difference at another rate is only meaningful against
    /// it. **48000** is what all four of this machine's active render endpoints
    /// declare (task 1, question 5) and is required by task 4 whether or not this
    /// machine has such an endpoint. **44100** is the other rate ordinary consumer
    /// hardware declares; it is not on this machine and the operator's machine is
    /// not this one.
    /// </remarks>
    public static TheoryData<int> Rates => new() { 12_000, 44_100, 48_000 };

    /// <summary>How long an FT8 signal is, whatever it is built at.</summary>
    private const double SignalSeconds = 12.64;

    /// <summary>
    /// **Every message in the corpus reads back as itself at every rate, and the
    /// signal is 12.64 s at all of them.**
    /// </summary>
    [Theory]
    [MemberData(nameof(Rates))]
    public void AtThisRateTheCorpusReadsBackAndTheSignalIsStillTwelvePointSixFourSeconds(int rate)
    {
        Assert.True(
            Ft8Composer.RateIsUsable(rate, out var whyNot), whyNot);

        var decoder = new Ft8SlotDecoder();
        var corpus = Corpus();

        var readBack = 0;
        var hashed = new List<string>();
        var failures = new List<string>();

        _output.WriteLine($"RATE: {rate} Hz");
        _output.WriteLine(string.Empty);
        _output.WriteLine($"{"message",-22} {"signal samples",15} {"seconds",9}  read back");

        for (var i = 0; i < corpus.Count; i++)
        {
            var text = corpus[i];

            // Spread across the decoder's 200-3000 Hz search window, deterministic
            // and identical at every rate so the rate is the only thing varying.
            var baseHz = 300.0f + (i * 137 % 2200);

            // THE SIGNAL, which is what goes on the air and what task 4 measures
            // the duration of.
            var signal = Ft8Composer.ComposeSignal(text, rate, baseHz);

            Assert.True(signal.Composed, text + ": " + signal.Explanation);

            var transmission = signal.Transmission!;
            var seconds = transmission.SlotSeconds;

            // THE PADDED SLOT, which is what a decoder is handed.
            var slot = Ft8Composer.Compose(text, rate, baseHz);

            Assert.True(slot.Composed, text + ": " + slot.Explanation);

            var heard = slot.Transmission!;

            // WHAT ANY RECEIVER DOES: onto the FT8 grid before decoding.
            var atDecoderRate = rate == Ft8Resample.TargetSampleRate
                ? heard.Samples
                : Ft8Resample.Resample(heard.Samples, rate, Ft8Resample.TargetSampleRate);

            var texts = decoder.Decode(atDecoderRate).Texts;
            var itself = texts.Contains(heard.ReadsBackAs, StringComparer.Ordinal);

            if (itself)
            {
                readBack++;
            }
            else if (heard.CarriesHashedCallsign)
            {
                // **NEVER COUNTED AS A PASS.** A callsign on the wire as a hash
                // resolves only where the full call was heard in the same slot,
                // and a slot carrying one message never has it. That is a fact
                // about FT8 and not about the rate - which is why the assertion
                // below is that the answer is the same at every rate.
                hashed.Add(text);
            }
            else
            {
                failures.Add(
                    $"\"{text}\" - sent as \"{heard.ReadsBackAs}\" at {baseHz:F0} Hz, decoder "
                    + $"returned {(texts.Count == 0 ? "nothing" : "\"" + string.Join("\", \"", texts) + "\"")}");
            }

            _output.WriteLine(
                $"{text,-22} {transmission.Samples.Length,15} {seconds,9:F4}  "
                + (itself ? "yes" : heard.CarriesHashedCallsign ? "hashed" : "NO"));

            // **THE DURATION IS THE SAME AT EVERY RATE OR IT IS A DEFECT.** Not
            // rounded: a rate that changes how long a transmission lasts would put
            // it in somebody else's slot.
            Assert.Equal(SignalSeconds, seconds, 6);
            Assert.Equal(
                (int)Math.Round(SignalSeconds * rate), transmission.Samples.Length);
        }

        _output.WriteLine(string.Empty);
        _output.WriteLine($"TRIED                 : {corpus.Count}");
        _output.WriteLine($"READ BACK IDENTICALLY : {readBack}");
        _output.WriteLine($"on the air as a hash  : {hashed.Count}"
            + (hashed.Count == 0 ? string.Empty : " - " + string.Join(", ", hashed)));
        _output.WriteLine($"FAILED                : {failures.Count}");

        foreach (var line in failures)
        {
            _output.WriteLine("  " + line);
        }

        Assert.Empty(failures);
    }

    /// <summary>
    /// **The rate changes nothing about which messages read back.**
    /// </summary>
    /// <remarks>
    /// The assertion the theory above cannot make on its own: each case sees one
    /// rate, so none of them can say the answers agree. A message that read back
    /// at 12000 and not at 48000 would be a defect the send path introduced, and a
    /// message that read back only at 48000 would mean the two rates are not
    /// carrying the same signal.
    /// </remarks>
    [Fact]
    public void TheSameMessagesReadBackAtEveryRate()
    {
        var corpus = Corpus();
        var byRate = new Dictionary<int, List<string>>();

        foreach (var rate in new[] { 12_000, 44_100, 48_000 })
        {
            var decoder = new Ft8SlotDecoder();
            var read = new List<string>();

            for (var i = 0; i < corpus.Count; i++)
            {
                var baseHz = 300.0f + (i * 137 % 2200);
                var slot = Ft8Composer.Compose(corpus[i], rate, baseHz);
                var heard = slot.Transmission!;

                var atDecoderRate = rate == Ft8Resample.TargetSampleRate
                    ? heard.Samples
                    : Ft8Resample.Resample(heard.Samples, rate, Ft8Resample.TargetSampleRate);

                if (decoder.Decode(atDecoderRate).Texts
                    .Contains(heard.ReadsBackAs, StringComparer.Ordinal))
                {
                    read.Add(corpus[i]);
                }
            }

            byRate[rate] = read;

            _output.WriteLine($"{rate,6} Hz : {read.Count} of {corpus.Count} read back");
        }

        var reference = byRate[12_000];

        _output.WriteLine(string.Empty);
        _output.WriteLine("read back at 12000: " + string.Join(", ", reference));

        Assert.Equal(reference, byRate[44_100]);
        Assert.Equal(reference, byRate[48_000]);
    }

    /// <summary>
    /// A dozen messages: what a band carries, with a compound callsign, a grid, a
    /// report and an <c>RR73</c> among them.
    /// </summary>
    /// <remarks>
    /// **SMALL ON PURPOSE.** Step 2 proved the corpus; this proves the rate, and a
    /// hundred messages at three rates would be three hundred slots of synthesis
    /// and decoding to answer a question a dozen answers.
    /// </remarks>
    private static IReadOnlyList<string> Corpus() =>
    [
        "CQ KC3QIS FN00",           // a grid
        "CQ DX KC3QIS FN00",
        "CQ TEST KC3QIS FN00",
        "W1ABC KC3QIS FN00",        // a grid, addressed
        "W1ABC KC3QIS -10",         // a report
        "W1ABC KC3QIS R-08",
        "W1ABC KC3QIS +03",
        "W1ABC KC3QIS RRR",
        "W1ABC KC3QIS RR73",        // the RR73
        "W1ABC KC3QIS 73",
        "CQ W1ABC/P FN42",          // a compound callsign
        "W1ABC/P KC3QIS -05",
    ];
}
