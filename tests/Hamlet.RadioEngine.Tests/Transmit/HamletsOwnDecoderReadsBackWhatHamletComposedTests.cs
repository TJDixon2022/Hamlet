using System.Diagnostics;
using Ft8Sharp.Dsp;
using Ft8Sharp.Encode;
using Hamlet.RadioEngine.Transmit;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Transmit;

/// <summary>
/// **The whole of step 2's central criterion.** Text in through the seam, into
/// audio, into <c>Ft8SlotDecoder</c>, and the same message comes back — over more
/// than a hundred messages, by category, with every failure named and its text
/// quoted.
/// </summary>
/// <remarks>
/// <para>**The receive path is the oracle** (`PHASE_PLAN.md`). If Hamlet cannot
/// read its own transmission, nothing else in this phase is worth building — so
/// the assertion is not *did something decode* but *did the message that went in
/// come back out as itself*.</para>
/// <para>**The corpus is built here rather than borrowed.**
/// <c>Ft8Sharp.Tests.Encode.EncodeCorpus</c> is internal to another assembly, is a
/// corpus of packed bytes rather than of words, and carries entries with no text
/// form at all. Its *choice* of messages is the record of what a band carries and
/// this corpus is built to the same shape from the categories `PHASE_PLAN.md`
/// names.</para>
/// <para>**Nothing here opens a device, keys anything, or waits on a clock.** A
/// stopwatch is read to report how long the run took and decides nothing.</para>
/// </remarks>
public sealed class HamletsOwnDecoderReadsBackWhatHamletComposedTests
{
    private readonly ITestOutputHelper _output;

    public HamletsOwnDecoderReadsBackWhatHamletComposedTests(ITestOutputHelper output) =>
        _output = output;

    /// <summary>The rate the decoder reads at.</summary>
    private const int Rate = Ft8Waveform.DefaultSampleRate;

    /// <summary>One message of the corpus and what kind of thing it is.</summary>
    private sealed record Entry(string Category, string Text);

    /// <summary>
    /// **THE ONE. Every message in the corpus goes out as audio and comes back as
    /// itself.**
    /// </summary>
    [Fact]
    public void EveryMessageHamletComposesComesBackFromItsOwnDecoderAsItself()
    {
        var corpus = Corpus();
        var decoder = new Ft8SlotDecoder();
        var clock = Stopwatch.StartNew();

        var byCategory = new Dictionary<string, (int Tried, int Composed, int ReadBack)>(StringComparer.Ordinal);
        var failures = new List<string>();
        var conditional = new List<string>();

        for (var i = 0; i < corpus.Count; i++)
        {
            var entry = corpus[i];
            var tally = byCategory.TryGetValue(entry.Category, out var t)
                ? t
                : (Tried: 0, Composed: 0, ReadBack: 0);
            tally.Tried++;

            // Spread across the decoder's own 200-3000 Hz search window, so this is
            // not one frequency measured a hundred times. Deterministic.
            var baseHz = 300.0f + (i * 137 % 2200);

            var composed = Ft8Composer.Compose(entry.Text, Rate, baseHz);
            if (!composed.Composed)
            {
                failures.Add(
                    $"[{entry.Category}] \"{entry.Text}\" — would not compose: {composed.Refusal}. "
                    + composed.Explanation);
                byCategory[entry.Category] = tally;
                continue;
            }

            tally.Composed++;
            var transmission = composed.Transmission!;
            var texts = decoder.Decode(transmission.Samples).Texts;

            if (texts.Contains(transmission.ReadsBackAs, StringComparer.Ordinal))
            {
                tally.ReadBack++;
            }
            else if (transmission.CarriesHashedCallsign)
            {
                // Counted separately and never as a pass. A callsign on the wire as
                // a hash resolves only where the full call was heard in the same
                // slot, and a slot holding one transmission never has it.
                conditional.Add(
                    $"[{entry.Category}] \"{entry.Text}\" — on the air as a hash; the bits say "
                    + $"\"{transmission.ReadsBackAs}\" and a slot carrying only this message returned "
                    + $"{(texts.Count == 0 ? "nothing" : "\"" + string.Join("\", \"", texts) + "\"")}.");
            }
            else
            {
                failures.Add(
                    $"[{entry.Category}] \"{entry.Text}\" — sent as \"{transmission.ReadsBackAs}\" at "
                    + $"{baseHz:F0} Hz, decoder returned "
                    + $"{(texts.Count == 0 ? "nothing" : "\"" + string.Join("\", \"", texts) + "\"")}.");
            }

            byCategory[entry.Category] = tally;
        }

        clock.Stop();

        var tried = corpus.Count;
        var composedCount = byCategory.Values.Sum(v => v.Composed);
        var readBack = byCategory.Values.Sum(v => v.ReadBack);

        _output.WriteLine($"{"category",-34} {"tried",6} {"composed",9} {"read back",10}");
        foreach (var (category, counts) in byCategory.OrderBy(pair => pair.Key, StringComparer.Ordinal))
        {
            _output.WriteLine(
                $"{category,-34} {counts.Tried,6} {counts.Composed,9} {counts.ReadBack,10}");
        }

        _output.WriteLine(string.Empty);
        _output.WriteLine($"TRIED                          : {tried}");
        _output.WriteLine($"COMPOSED                       : {composedCount}");
        _output.WriteLine($"READ BACK IDENTICALLY          : {readBack} of {tried}");
        _output.WriteLine($"read back only under conditions: {conditional.Count}");
        _output.WriteLine($"failed                         : {failures.Count}");
        _output.WriteLine($"elapsed                        : {clock.Elapsed.TotalSeconds:F2} s "
            + $"({clock.Elapsed.TotalMilliseconds / tried:F1} ms a message)");

        if (conditional.Count > 0)
        {
            _output.WriteLine(string.Empty);
            _output.WriteLine("READ BACK ONLY UNDER CONDITIONS — every one named:");
            foreach (var line in conditional)
            {
                _output.WriteLine("  " + line);
            }
        }

        if (failures.Count > 0)
        {
            _output.WriteLine(string.Empty);
            _output.WriteLine("FAILURES — every one named with its message text:");
            foreach (var line in failures)
            {
                _output.WriteLine("  " + line);
            }
        }

        Assert.True(tried >= 100, $"the corpus is {tried} messages and the criterion asks for 100.");
        Assert.Empty(failures);

        // Every message that is not on the air as a hash read back as itself. The
        // hashed ones are the named category and their condition is proved by
        // AHashedCallsignReadsBackOnlyWhenTheFullCallIsInTheSameSlot below.
        Assert.Equal(composedCount - conditional.Count, readBack);
    }

    /// <summary>
    /// **The condition, proved rather than asserted.** A callsign that travels as a
    /// hash reads back when — and only when — the full call is in the same slot.
    /// </summary>
    /// <remarks>
    /// <c>Ft8SlotDecoder.Decode</c> builds its callsign cache per slot and drops it
    /// when the call returns, so this is a fact about FT8 and about that scope, not
    /// a defect in the seam. Two transmissions are summed into one slot at
    /// different frequencies, exactly as two stations on a band would arrive.
    /// </remarks>
    [Fact]
    public void AHashedCallsignReadsBackOnlyWhenTheFullCallIsInTheSameSlot()
    {
        var hashed = Ft8Composer.Compose("PJ4/K1ABC W9XYZ", Rate, 1000.0f);
        var inFull = Ft8Composer.Compose("CQ PJ4/K1ABC", Rate, 1600.0f);

        Assert.True(hashed.Composed, hashed.Explanation);
        Assert.True(inFull.Composed, inFull.Explanation);
        Assert.True(hashed.Transmission!.CarriesHashedCallsign);
        Assert.False(inFull.Transmission!.CarriesHashedCallsign);

        var decoder = new Ft8SlotDecoder();

        var alone = decoder.Decode(hashed.Transmission.Samples).Texts;
        _output.WriteLine($"the hashed message alone : "
            + $"{(alone.Count == 0 ? "nothing" : string.Join(" | ", alone))}");

        var together = new float[hashed.Transmission.Samples.Length];
        for (var i = 0; i < together.Length; i++)
        {
            // Halved so the sum cannot leave full scale. Two stations in one slot.
            together[i] = (hashed.Transmission.Samples[i] + inFull.Transmission.Samples[i]) / 2.0f;
        }

        var withTheFullCall = decoder.Decode(together).Texts;
        _output.WriteLine($"with the full call too   : {string.Join(" | ", withTheFullCall)}");

        Assert.DoesNotContain(hashed.Transmission.ReadsBackAs, alone, StringComparer.Ordinal);
        Assert.Contains(hashed.Transmission.ReadsBackAs, withTheFullCall, StringComparer.Ordinal);
        Assert.Contains(inFull.Transmission.ReadsBackAs, withTheFullCall, StringComparer.Ordinal);
    }

    /// <summary>
    /// **WATCHED TO FAIL. The breakage this round trip would have caught is the one
    /// it is here catching.**
    /// </summary>
    /// <remarks>
    /// <para>The breakage is **a base frequency outside the decoder's own search
    /// window**. `Ft8WaterfallGeometry` searches 200 Hz to 3000 Hz; the synthesiser
    /// will happily put a transmission at 3500 Hz, which is a perfectly good FT8
    /// signal that this decoder will never look at. It is the breakage that most
    /// resembles a plausible mistake — a transmit frequency taken from the wrong
    /// place — and nothing about the samples, their length, their level or their
    /// phase is wrong.</para>
    /// <para>Before this file went green, the corpus above was run at 3500 Hz and
    /// went red on every message. This pins that.</para>
    /// </remarks>
    [Fact]
    public void ATransmissionOutsideTheDecodersSearchWindowIsNotReadBack()
    {
        const float OutsideTheWindow = 3500.0f;

        Assert.True(OutsideTheWindow > Ft8WaterfallGeometry.DefaultMaxFrequencyHz);

        var decoder = new Ft8SlotDecoder();
        string[] messages = ["CQ KC3QIS FN00", "K1ABC W9XYZ -11", "K1ABC W9XYZ RR73"];

        foreach (var text in messages)
        {
            var inside = Ft8Composer.Compose(text, Rate, 1000.0f);
            var outside = Ft8Composer.Compose(text, Rate, OutsideTheWindow);

            Assert.True(inside.Composed, inside.Explanation);
            Assert.True(outside.Composed, outside.Explanation);

            var readInside = decoder.Decode(inside.Transmission!.Samples).Texts;
            var readOutside = decoder.Decode(outside.Transmission!.Samples).Texts;

            _output.WriteLine($"\"{text}\"");
            _output.WriteLine($"   at 1000 Hz : {(readInside.Count == 0 ? "nothing" : string.Join(" | ", readInside))}");
            _output.WriteLine($"   at 3500 Hz : {(readOutside.Count == 0 ? "nothing" : string.Join(" | ", readOutside))}");

            Assert.Contains(text, readInside, StringComparer.Ordinal);
            Assert.DoesNotContain(text, readOutside, StringComparer.Ordinal);
        }
    }

    /// <summary>
    /// The corpus: what a band actually carries, across the categories
    /// `PHASE_PLAN.md` names.
    /// </summary>
    /// <remarks>
    /// **Not padded.** Every entry is a distinct 77-bit payload and the categories
    /// are sized by what each one is worth covering, not by what makes a round
    /// number. The compound callsigns are here in all three of their forms —
    /// suffixed, carried in full, and carried as a hash — because the third is the
    /// one that behaves differently and dropping it would make the count a lie.
    /// </remarks>
    private static IReadOnlyList<Entry> Corpus()
    {
        var entries = new List<Entry>();
        void Add(string category, params string[] texts)
        {
            foreach (var text in texts)
            {
                entries.Add(new Entry(category, text));
            }
        }

        // The operator's own, first, because it is the one that has to work.
        Add("the operator's own CQ", "CQ KC3QIS FN00");

        Add(
            "CQ with a grid",
            "CQ K1ABC FN42",
            "CQ W9XYZ EM12",
            "CQ G4ABC IO91",
            "CQ JA1ABC PM95",
            "CQ VK2ABC QF56",
            "CQ PY2ABC GG66",
            "CQ DL1ABC JO31",
            "CQ F5ABC JN18",
            "CQ EA3ABC JN01",
            "CQ SM0ABC JO99",
            "CQ OH2ABC KP20",
            "CQ ZL1ABC RF73",
            "CQ ZS1ABC JF96",
            "CQ LU1ABC GF05",
            "CQ VE3ABC FN03",
            "CQ K1ABC AA00",
            "CQ K1ABC RR99",
            "CQ K1ABC JJ55",
            "CQ KA1A FN31",
            "CQ K10ABC FN42");

        Add(
            "CQ with no grid",
            "CQ K1ABC",
            "CQ W9XYZ",
            "CQ G4ABC",
            "CQ JA1ABC",
            "CQ VK2ABC",
            "CQ KC3QIS");

        Add(
            "a directed or lettered CQ",
            "CQ DX K1ABC FN42",
            "CQ DX W9XYZ EM12",
            "CQ EU G4ABC IO91",
            "CQ NA K1ABC FN42",
            "CQ 123 K1ABC FN42",
            "CQ DX KC3QIS FN00",
            "QRZ K1ABC FN42",
            "CQ DX K1ABC");

        Add(
            "a signal report",
            "K1ABC W9XYZ -01",
            "K1ABC W9XYZ -05",
            "K1ABC W9XYZ -09",
            "K1ABC W9XYZ -11",
            "K1ABC W9XYZ -15",
            "K1ABC W9XYZ -20",
            "K1ABC W9XYZ -24",
            "K1ABC W9XYZ -30",
            "K1ABC W9XYZ +00",
            "K1ABC W9XYZ +05",
            "K1ABC W9XYZ +10",
            "K1ABC W9XYZ +15",
            "K1ABC W9XYZ +20",
            "K1ABC W9XYZ +30",
            "KC3QIS G4ABC -13",
            "G4ABC KC3QIS -07");

        Add(
            "a report acknowledged",
            "K1ABC W9XYZ R-01",
            "K1ABC W9XYZ R-09",
            "K1ABC W9XYZ R-15",
            "K1ABC W9XYZ R-24",
            "K1ABC W9XYZ R+00",
            "K1ABC W9XYZ R+10",
            "K1ABC W9XYZ R+20",
            "KC3QIS G4ABC R-13");

        Add(
            "a grid in the exchange",
            "K1ABC W9XYZ FN42",
            "W9XYZ K1ABC EM12",
            "G4ABC JA1ABC IO91",
            "JA1ABC G4ABC PM95",
            "VK2ABC ZL1ABC QF56",
            "KC3QIS K1ABC FN00",
            "K1ABC KC3QIS FN42",
            "DL1ABC F5ABC JO31",
            "F5ABC DL1ABC JN18",
            "EA3ABC SM0ABC JN01",
            "SM0ABC EA3ABC JO99",
            "OH2ABC LA1ABC KP20");

        Add(
            "RRR",
            "K1ABC W9XYZ RRR",
            "W9XYZ K1ABC RRR",
            "KC3QIS G4ABC RRR",
            "G4ABC KC3QIS RRR",
            "JA1ABC VK2ABC RRR");

        Add(
            "RR73",
            "K1ABC W9XYZ RR73",
            "W9XYZ K1ABC RR73",
            "KC3QIS G4ABC RR73",
            "G4ABC KC3QIS RR73",
            "JA1ABC VK2ABC RR73",
            "DL1ABC F5ABC RR73",
            "ZL1ABC VK2ABC RR73");

        Add(
            "73",
            "K1ABC W9XYZ 73",
            "W9XYZ K1ABC 73",
            "KC3QIS G4ABC 73",
            "G4ABC KC3QIS 73",
            "JA1ABC VK2ABC 73",
            "PY2ABC LU1ABC 73",
            "ZS1ABC G4ABC 73");

        Add(
            "no third field at all",
            "K1ABC W9XYZ",
            "W9XYZ K1ABC",
            "KC3QIS G4ABC",
            "G4ABC KC3QIS");

        // Compound callsigns, in all three of the forms FT8 has for them.
        Add(
            "a compound call, suffixed",
            "CQ K1ABC/P FN42",
            "CQ K1ABC/R FN42",
            "CQ G4ABC/P IO91",
            "K1ABC W9XYZ/R RRR",
            "K1ABC W9XYZ/R -11",
            "W9XYZ/R K1ABC 73",
            "CQ KC3QIS/R FN00");

        Add(
            "a compound call, carried in full",
            "CQ PJ4/K1ABC",
            "CQ VP2E/K1ABC",
            "CQ ZF2/G4ABC",
            "CQ 3D2/W9XYZ",
            "CQ FR/DL1ABC");

        Add(
            "a compound call, carried as a hash",
            "PJ4/K1ABC W9XYZ",
            "VP2E/K1ABC W9XYZ",
            "PJ4/K1ABC W9XYZ -11",
            "PJ4/K1ABC W9XYZ RRR",
            "W9XYZ PJ4/K1ABC");

        Add(
            "free text",
            "TNX BOB 73 GL",
            "HW CPY OM",
            "GL IN TEST",
            "ABCDEFGHIJKLM",
            "TU 73 GL OM",
            "ANT WORK OK");

        return entries;
    }
}
