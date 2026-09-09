using Ft8Sharp.Encode;
using Ft8Sharp.Message;

namespace Hamlet.RadioEngine.Transmit;

/// <summary>Assembles one message's channel symbols.</summary>
/// <param name="message">The 77 bits, as ten bytes.</param>
/// <returns>The symbols, one tone index each.</returns>
/// <remarks>
/// **A DELEGATE RATHER THAN A <c>Func</c> BECAUSE THE PAYLOAD IS A SPAN.** A
/// <see cref="ReadOnlySpan{T}"/> cannot be a generic argument, so the two port
/// entry points this stands in front of - <c>Ft8SymbolEncoder.Encode</c> and
/// <c>Ft4SymbolEncoder.Encode</c> - need a declared shape to be named by.
/// </remarks>
internal delegate byte[] SymbolAssembly(ReadOnlySpan<byte> message);

/// <summary>Turns channel symbols into audio.</summary>
/// <param name="symbols">The tone indices.</param>
/// <param name="sampleRate">Samples per second.</param>
/// <param name="baseFrequency">The audio frequency of tone 0.</param>
/// <returns>The samples, in the range -1 to +1.</returns>
internal delegate float[] Synthesis(
    ReadOnlySpan<byte> symbols, int sampleRate, float baseFrequency);

/// <summary>
/// **One mode's modulation, named once: its two port calls and its four
/// constants.**
/// </summary>
/// <param name="ModeName">
/// What the mode is called in a sentence the operator reads. **The only thing in
/// this record that is not the port's.**
/// </param>
/// <param name="DefaultSampleRate">The rate the mode is decoded at.</param>
/// <param name="DefaultBaseFrequency">Where tone 0 sits unless another is asked for.</param>
/// <param name="SymbolCount">How many channel symbols one transmission carries.</param>
/// <param name="ToneCount">How many tones the modulation has.</param>
/// <param name="ToneSpacingHz">How far apart they sit.</param>
/// <param name="SamplesPerSymbol">How long one symbol is, at a rate.</param>
/// <param name="SampleCount">How long the whole signal is, at a rate.</param>
/// <param name="Assemble">The port's symbol assembly.</param>
/// <param name="SynthesiseSignal">The port's bare signal - what goes on the air.</param>
/// <param name="SynthesiseSlot">The port's padded slot - what a decoder reads.</param>
/// <remarks>
/// <para>**EVERY FIELD IS READ OUT OF <c>Ft8Sharp</c> AND NONE IS WRITTEN DOWN
/// HERE.** <see cref="Ft8"/> and <see cref="Ft4"/> are two lists of the port's own
/// members; there is no tone table, no symbol period and no length arithmetic in
/// this assembly, and a number that disagreed with the port would have to be typed
/// in first.</para>
/// <para>**IT DESCRIBES THE MODULATION AND NOTHING ELSE.** It carries no slot
/// length, no occupancy and no boundary: where a transmission sits in time is the
/// grid's question (<c>SlotGrid</c>) and is asked by
/// <see cref="Ft8TransmitSequence"/>, not here. In particular it does not carry
/// the figure the open 4.48-against-5.04 question is about, so nothing about
/// composing a message can depend on how that is settled.</para>
/// </remarks>
internal sealed record ComposeGeometry(
    string ModeName,
    int DefaultSampleRate,
    float DefaultBaseFrequency,
    int SymbolCount,
    int ToneCount,
    float ToneSpacingHz,
    Func<int, int> SamplesPerSymbol,
    Func<int, int> SampleCount,
    SymbolAssembly Assemble,
    Synthesis SynthesiseSignal,
    Synthesis SynthesiseSlot)
{
    /// <summary>FT8's modulation: 79 symbols, 8 tones at 6.25 Hz.</summary>
    public static ComposeGeometry Ft8 { get; } = new(
        "FT8",
        Ft8Waveform.DefaultSampleRate,
        Ft8Waveform.DefaultBaseFrequency,
        Ft8Waveform.SymbolCount,
        Ft8Waveform.ToneCount,
        Ft8Waveform.ToneSpacingHz,
        Ft8Waveform.SamplesPerSymbol,
        Ft8Waveform.SampleCount,
        Ft8SymbolEncoder.Encode,
        Ft8Waveform.Synthesize,
        Ft8Waveform.SynthesizeSlot);

    /// <summary>FT4's modulation: 105 symbols, 4 tones at 20.833 Hz.</summary>
    /// <remarks>
    /// **UNIT 289 BUILT AND NAILED BOTH OF THESE TO <c>gen_ft8 -ft4</c>, SYMBOL FOR
    /// SYMBOL AND SAMPLE FOR SAMPLE.** They are called and nothing in them is
    /// changed - `PHASE_PLAN.md`'s standing ruling that <c>Ft8Sharp</c> remains a
    /// faithful MIT port.
    /// </remarks>
    public static ComposeGeometry Ft4 { get; } = new(
        "FT4",
        Ft4Waveform.DefaultSampleRate,
        Ft4Waveform.DefaultBaseFrequency,
        Ft4Waveform.SymbolCount,
        Ft4Waveform.ToneCount,
        Ft4Waveform.ToneSpacingHz,
        Ft4Waveform.SamplesPerSymbol,
        Ft4Waveform.SampleCount,
        Ft4SymbolEncoder.Encode,
        Ft4Waveform.Synthesize,
        Ft4Waveform.SynthesizeSlot);
}

/// <summary>
/// **The one seam between what the operator wants to say and the audio of one
/// slot, for both slotted digital modes.**
/// </summary>
/// <remarks>
/// <para>**THE PACKING IS SHARED AND STAYS SHARED** (work instruction 293 task 2).
/// FT4 carries FT8's 77-bit payload - <c>PHASE_PLAN.md</c> says so and unit 288
/// measured it - so <see cref="PackAsItself"/>, the three-pass hash ordering and
/// the round trip through <c>Ft8MessageDecoder</c> are **the same code and not a
/// copy of it**. A second packer beside the first is a second thing to drift, which
/// is the argument the port itself rests on.</para>
/// <para>**WHAT DIFFERS BETWEEN THE TWO MODES IS <see cref="ComposeGeometry"/> AND
/// NOTHING ELSE**, and that is four constants and three port calls. It is the same
/// size of difference <c>Ft8Composer</c>'s own two routes have between them - one
/// call, and nothing else.</para>
/// <para>**IT KEYS NOTHING AND OPENS NOTHING.** No audio device, no thread, no
/// timer, no clock, no file, no serial port. It names no rig type, no PTT, no CI-V
/// command and not <c>TransmitAbort</c>. It takes words and returns an array of
/// floats.</para>
/// <para>**AND IT PUTS NO TRANSMISSION LENGTH IN A SENTENCE.** Every refusal below
/// is about a rate, a frequency, a drive level or a packing; none of them names how
/// long a transmission is, because the FT4 figure is an open question with the
/// owner and a sentence stating either answer would be settling it.</para>
/// </remarks>
internal static class DigitalComposer
{
    /// <summary>Both routes, for either mode, which differ in one call.</summary>
    /// <param name="geometry">Which mode's modulation.</param>
    /// <param name="text">The message, in the operator's own words.</param>
    /// <param name="sampleRate">Samples per second.</param>
    /// <param name="baseFrequencyHz">The audio frequency of tone 0.</param>
    /// <param name="drivePeak">The peak amplitude to build at.</param>
    /// <param name="wholeSlot">True for the padded slot, false for the signal alone.</param>
    /// <remarks>
    /// **THIS IS THE ONE PLACE THE TRANSMIT DRIVE IS APPLIED, AND IT IS APPLIED
    /// HERE RATHER THAN IN THE SINK ON PURPOSE.**
    /// <see cref="Ft8Transmission.PeakSample"/> is measured off the array every
    /// time it is asked for, so a transmission scaled at compose time **carries
    /// the peak it will actually play at**. A gain applied further down would
    /// hand <c>Ft8TransmitSequence</c> an object claiming a peak its own samples
    /// do not have - the same class of lie unit 256 caught in
    /// <c>SamplesPlayed</c>. It is also the only place available: the synthesis
    /// itself is <c>Ft8Waveform.cs:199</c> and <c>Ft4Waveform.cs:194</c>, and
    /// <c>Ft8Sharp</c> is a faithful MIT port that nothing in this phase changes a
    /// line of. **One level, one place, for both modes.**
    /// </remarks>
    internal static Ft8ComposeResult Build(
        ComposeGeometry geometry,
        string? text,
        int sampleRate,
        float baseFrequencyHz,
        float drivePeak,
        bool wholeSlot)
    {
        var wanted = Normalise(text);
        if (wanted.Length == 0)
        {
            return Ft8ComposeResult.No(
                Ft8ComposeRefusal.NothingToSay,
                "there were no words to send. A transmission carries a message and this one is empty.");
        }

        if (!RateIsUsable(geometry, sampleRate, out var rateExplanation))
        {
            return Ft8ComposeResult.No(Ft8ComposeRefusal.SampleRateRefused, rateExplanation);
        }

        if (!BaseFrequencyIsUsable(geometry, baseFrequencyHz, sampleRate, out var baseExplanation))
        {
            return Ft8ComposeResult.No(Ft8ComposeRefusal.BaseFrequencyRefused, baseExplanation);
        }

        if (!DriveIsUsable(drivePeak, out var driveExplanation))
        {
            return Ft8ComposeResult.No(Ft8ComposeRefusal.DriveLevelRefused, driveExplanation);
        }

        var packing = PackAsItself(wanted, out var packExplanation);
        if (packing is null)
        {
            return Ft8ComposeResult.No(Ft8ComposeRefusal.WillNotPack, packExplanation);
        }

        // From here down every step is the port's. Nothing below chooses a tone,
        // places a synchronisation block or advances a phase.
        var symbols = geometry.Assemble(packing.Bits);
        var audio = wholeSlot
            ? geometry.SynthesiseSlot(symbols, sampleRate, baseFrequencyHz)
            : geometry.SynthesiseSignal(symbols, sampleRate, baseFrequencyHz);

        // THE DRIVE, APPLIED ONCE, AFTER THE PORT HAS RETURNED AND BEFORE THE
        // ARRAY IS HANDED TO ANYBODY. The port builds a sine of unit amplitude and
        // stays that way; the level is this assembly's. Multiplying in place is
        // safe because the array was made by the port for this call and has no
        // other owner.
        for (var i = 0; i < audio.Length; i++)
        {
            audio[i] *= drivePeak;
        }

        return Ft8ComposeResult.Ok(new Ft8Transmission(
            wanted,
            packing.ReadsBackAs,
            packing.Type,
            audio,
            sampleRate,
            baseFrequencyHz,
            packing.CarriesHashedCallsign));
    }

    /// <summary>
    /// Whether the port will synthesise at this rate, asked of the port rather
    /// than decided here.
    /// </summary>
    /// <remarks>
    /// **This is the port's own consistency condition, read out of the port's own
    /// two length functions.** <c>SampleCount</c> sizes the slot from the
    /// transmission's duration and the synthesis writes
    /// <c>SymbolCount * SamplesPerSymbol</c>; where those two disagree the signal
    /// lands at the wrong offset, and the port refuses. Asking the same question
    /// here, with the same two calls, means this seam and the port cannot come to
    /// different answers - **and it is asked of the mode's own two functions**, so
    /// a rate FT8 can be built at and FT4 cannot is refused for FT4.
    /// </remarks>
    internal static bool RateIsUsable(
        ComposeGeometry geometry, int sampleRate, out string explanation)
    {
        if (sampleRate <= 0)
        {
            explanation =
                $"{sampleRate} samples per second is not a sample rate. Audio cannot be synthesised "
                + "at zero or fewer.";
            return false;
        }

        var fromDuration = geometry.SampleCount(sampleRate);
        var fromSymbols = geometry.SymbolCount * geometry.SamplesPerSymbol(sampleRate);
        if (fromDuration != fromSymbols)
        {
            explanation =
                $"at {sampleRate} samples per second the signal is {fromDuration} samples long measured "
                + $"from the transmission's duration and {fromSymbols} measured as "
                + $"{geometry.SymbolCount} symbols of {geometry.SamplesPerSymbol(sampleRate)}. "
                + "The slot is laid out from the first and written from the second, so every sample "
                + "after the signal starts would be at the wrong offset. Use a rate at which a symbol "
                + $"is a whole number of samples — {geometry.DefaultSampleRate} is the one "
                + $"{geometry.ModeName} is decoded at.";
            return false;
        }

        explanation = string.Empty;
        return true;
    }

    /// <summary>Whether every tone would fit in the channel at this rate.</summary>
    /// <remarks>
    /// **THE ARITHMETIC IS THE MODE'S OWN AND IT IS NOT THE SAME ARITHMETIC.** FT8
    /// has eight tones at 6.25 Hz and FT4 has four at 20.833, so the top tone sits
    /// 43.75 Hz above tone 0 on one and 62.5 Hz above it on the other. Both figures
    /// come out of <see cref="ComposeGeometry"/>, which reads them off the port.
    /// </remarks>
    internal static bool BaseFrequencyIsUsable(
        ComposeGeometry geometry, float baseFrequencyHz, int sampleRate, out string explanation)
    {
        if (float.IsNaN(baseFrequencyHz) || baseFrequencyHz <= 0.0f)
        {
            explanation =
                $"{baseFrequencyHz} Hz puts tone 0 at or below DC, where it is not a tone at all.";
            return false;
        }

        var top = baseFrequencyHz + ((geometry.ToneCount - 1) * geometry.ToneSpacingHz);
        var nyquist = sampleRate / 2.0f;
        if (top >= nyquist)
        {
            explanation =
                $"with tone 0 at {baseFrequencyHz} Hz the top tone would sit at {top} Hz, at or above "
                + $"the {nyquist} Hz Nyquist limit of a {sampleRate} Hz rate, where it would alias down "
                + "into the channel as a different tone instead of being synthesised.";
            return false;
        }

        explanation = string.Empty;
        return true;
    }

    /// <summary>
    /// Whether a transmit drive level is a peak amplitude audio can be built at.
    /// </summary>
    /// <remarks>
    /// **Refused with a sentence rather than an exception**, because every other
    /// refusal on this path comes back as something the operator reads. **It is one
    /// method for both modes and it is not parameterised on the geometry**, because
    /// how loud a transmission is has nothing to do with how many tones it has: the
    /// two ends are zero and full scale on either mode.
    /// </remarks>
    internal static bool DriveIsUsable(float drivePeak, out string explanation)
    {
        if (float.IsNaN(drivePeak))
        {
            explanation =
                "the transmit drive level is not a number, so there is no level to build the "
                + "transmission at.";
            return false;
        }

        if (drivePeak <= 0.0f)
        {
            explanation =
                $"a transmit drive level of {drivePeak} is not a level. At or below zero there is "
                + "no transmission to send, only a keyed transmitter sending silence. Set a peak "
                + $"amplitude above 0 and no more than 1 - {Ft8Composer.DefaultDrivePeak} is "
                + $"{20.0 * Math.Log10(Ft8Composer.DefaultDrivePeak):0.##} dBFS and is where Hamlet starts.";
            return false;
        }

        if (drivePeak > 1.0f)
        {
            explanation =
                $"a transmit drive level of {drivePeak} is above full scale. Every sample past the "
                + "rail would be clamped on the way out and counted as a clip, which is distortion "
                + "on the band rather than a louder signal. Set a peak amplitude above 0 and no "
                + $"more than 1 - {Ft8Composer.DefaultDrivePeak} is "
                + $"{20.0 * Math.Log10(Ft8Composer.DefaultDrivePeak):0.##} dBFS and is where Hamlet starts.";
            return false;
        }

        explanation = string.Empty;
        return true;
    }

    /// <summary>
    /// Packs the words, and accepts a packing only if unpacking it again gives
    /// back the same words.
    /// </summary>
    /// <remarks>
    /// <para>**THE ROUND TRIP IS THE WHOLE OF THE REFUSAL.** Every candidate is
    /// offered to the packers and then read back through <c>Ft8MessageDecoder</c>,
    /// which is the call the receive path itself ends in. A packing that succeeds
    /// but does not reproduce its own text is discarded exactly like one that
    /// failed — audio for a message the operator did not ask for is the fault this
    /// method exists to prevent.</para>
    /// <para>**AND IT IS THE SAME METHOD FOR FT4, NOT A COPY OF IT** (work
    /// instruction 293 task 2). The 77-bit message layer is shared between the two
    /// modes; a second packer beside this one would be a second thing to drift, and
    /// the drift would be a message that says one thing on FT8 and another on
    /// FT4.</para>
    /// <para>**THREE PASSES, AND THE ORDER IS A RULING ABOUT HASHES RATHER THAN A
    /// PREFERENCE.** A message that puts a callsign on the wire as a hash can be
    /// read back only by a receiver that heard the full call in the same slot; a
    /// message that carries everything in full can be read by anybody. So anything
    /// sayable without a hash is said without one: first the structured types with
    /// no callsign cache at all, then free text, and only then the structured types
    /// with a cache.</para>
    /// <para>**The unit that wrote this had the order wrong and the corpus caught
    /// it.** With hashing allowed in the first pass, `GL IN TEST` packed as a
    /// standard message whose two callsign fields were the hashes of `GL IN` and
    /// `TEST` — nonsense on the air that rendered back as the right words, which is
    /// precisely the failure §0.0's principle forbids in the other direction. It is
    /// now free text, because free text is offered before anything is hashed.</para>
    /// </remarks>
    private static Packing? PackAsItself(string wanted, out string explanation)
    {
        var refusals = new List<string>();
        var words = wanted.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        // Pass one: everything carried in full.
        var whole = TryStructured(words, wanted, refusals, allowHashing: false);
        if (whole is not null)
        {
            explanation = string.Empty;
            return whole;
        }

        // Pass two: free text, which carries thirteen characters and hashes
        // nothing.
        var freeText = TryRoute(
            (_, buffer) => Ft8FreeText.TryPackText(wanted, buffer),
            wanted,
            Ft8MessageType.FreeText,
            refusals,
            "free text",
            allowHashing: false);
        if (freeText is not null)
        {
            explanation = string.Empty;
            return freeText;
        }

        // Pass three: a callsign on the wire as a hash, which is the only way to
        // say some things and is reported as what it is.
        var hashed = TryStructured(words, wanted, refusals, allowHashing: true);
        if (hashed is not null)
        {
            explanation = string.Empty;
            return hashed;
        }

        explanation =
            $"\"{wanted}\" is not a message this library can put on the air. What was tried, and what "
            + "each one said: " + string.Join("; ", refusals) + ".";
        return null;
    }

    /// <summary>
    /// Offers the structured message types every field arrangement these words
    /// admit.
    /// </summary>
    /// <remarks>
    /// Standard before non-standard: it is most of what a band carries, and it is
    /// the form that puts two whole callsigns on the wire.
    /// </remarks>
    private static Packing? TryStructured(
        string[] words, string wanted, List<string> refusals, bool allowHashing)
    {
        var pass = allowHashing ? "hashed" : "in full";

        foreach (var (to, de, extra) in FieldArrangements(words))
        {
            var standard = TryRoute(
                (cache, buffer) => Ft8StandardMessage.TryPack(to, de, extra, cache, buffer),
                wanted,
                Ft8MessageType.Standard,
                refusals,
                $"standard, {pass}, \"{to}\" / \"{de}\" / \"{extra}\"",
                allowHashing);
            if (standard is not null)
            {
                return standard;
            }

            var nonstandard = TryRoute(
                (cache, buffer) => Ft8NonstandardMessage.TryPack(to, de, extra, cache, buffer),
                wanted,
                Ft8MessageType.NonstandardCallsign,
                refusals,
                $"non-standard callsign, {pass}, \"{to}\" / \"{de}\" / \"{extra}\"",
                allowHashing);
            if (nonstandard is not null)
            {
                return nonstandard;
            }
        }

        return null;
    }

    /// <summary>
    /// Offers one packing route and keeps it only if it reads back as itself.
    /// </summary>
    /// <remarks>
    /// **Whether a callsign is on the wire as a hash is measured rather than
    /// guessed.** A message packed without a cache carries every callsign in full
    /// by construction; one that refuses for want of a cache has a callsign that
    /// can only travel as a hash. That is read off the port's own refusal rather
    /// than inferred from the shape of a callsign — and on the pass where hashing
    /// is not allowed, that refusal is simply the end of the route.
    /// </remarks>
    private static Packing? TryRoute(
        Func<Ft8CallsignCache?, byte[], Ft8PackResult> pack,
        string wanted,
        Ft8MessageType type,
        List<string> refusals,
        string label,
        bool allowHashing)
    {
        var buffer = new byte[Ft8Payload.MessageBytes];

        var result = pack(null, buffer);
        if (result is Ft8PackResult.FirstCallRequiresHashCache or Ft8PackResult.SecondCallRequiresHashCache)
        {
            if (!allowHashing)
            {
                refusals.Add($"{label} needs a callsign on the wire as a hash: {result}");
                return null;
            }

            var cache = new Ft8CallsignCache();
            result = pack(cache, buffer);
            if (result != Ft8PackResult.Ok)
            {
                refusals.Add($"{label} would not pack even with a callsign cache: {result}");
                return null;
            }

            if (!ReadsBackAsItself(buffer, cache, wanted, allowHashMarking: true, out var hashedText))
            {
                refusals.Add(
                    $"{label} packed with a callsign cache but read back as \"{hashedText}\"");
                return null;
            }

            return new Packing(buffer, type, CarriesHashedCallsign: true, hashedText);
        }

        if (result != Ft8PackResult.Ok)
        {
            refusals.Add($"{label} would not pack: {result}");
            return null;
        }

        if (!ReadsBackAsItself(buffer, null, wanted, allowHashMarking: false, out var text))
        {
            refusals.Add($"{label} packed but read back as \"{text}\"");
            return null;
        }

        return new Packing(buffer, type, CarriesHashedCallsign: false, text);
    }

    /// <summary>
    /// Unpacks a packed message and asks whether it says what it was asked to say.
    /// </summary>
    /// <param name="packed">The 77 bits, as ten bytes.</param>
    /// <param name="cache">The cache the message was packed with, or null.</param>
    /// <param name="wanted">The words the operator asked for.</param>
    /// <param name="readsBackAs">What the message layer made of the bits.</param>
    /// <param name="allowHashMarking">
    /// True only where the message was packed with a callsign cache, and therefore
    /// carries a callsign as a hash. **The port marks a callsign it recovered from
    /// a hash by putting it in angle brackets** — <c>Ft8CallsignField.Bracket</c>,
    /// and the convention is written out at
    /// <c>Ft8NonstandardMessage.TryPack</c>'s remarks: the brackets say *this name
    /// came from a hash rather than off the wire*. They are a marking on the
    /// reading, not a difference in the message, so a hashed message is allowed to
    /// come back wearing them. Everything else must match character for character,
    /// and the call inside the brackets still has to be the call that was asked
    /// for — which it can only be if it is the one this seam hashed.
    /// </param>
    private static bool ReadsBackAsItself(
        byte[] packed,
        Ft8CallsignCache? cache,
        string wanted,
        bool allowHashMarking,
        out string readsBackAs)
    {
        var read = Ft8MessageDecoder.Decode(packed, cache);
        readsBackAs = read.Text;

        if (!read.Decoded)
        {
            return false;
        }

        if (string.Equals(read.Text, wanted, StringComparison.Ordinal))
        {
            return true;
        }

        return allowHashMarking
            && string.Equals(
                read.Text.Replace("<", string.Empty, StringComparison.Ordinal)
                    .Replace(">", string.Empty, StringComparison.Ordinal),
                wanted,
                StringComparison.Ordinal);
    }

    /// <summary>One packing that survived its own round trip.</summary>
    private sealed record Packing(
        byte[] Bits, Ft8MessageType Type, bool CarriesHashedCallsign, string ReadsBackAs);

    /// <summary>
    /// The ways FT8's own message types can carry these words across three fields.
    /// </summary>
    /// <remarks>
    /// **These are arrangements, not readings.** A standard or non-standard
    /// message is an addressed station, a transmitting station and one more thing;
    /// the addressed station is one word except in the lettered and numbered
    /// general-call forms, where it is two. So two words admit one arrangement,
    /// three admit two, and four admit one. Nothing here decides which is right —
    /// the round trip does, by refusing every arrangement that does not reproduce
    /// the words it was built from.
    /// </remarks>
    private static IEnumerable<(string To, string De, string Extra)> FieldArrangements(string[] words)
    {
        switch (words.Length)
        {
            case 2:
                yield return (words[0], words[1], string.Empty);
                break;

            case 3:
                yield return (words[0], words[1], words[2]);
                yield return ($"{words[0]} {words[1]}", words[2], string.Empty);
                break;

            case 4:
                yield return ($"{words[0]} {words[1]}", words[2], words[3]);
                break;
        }
    }

    /// <summary>
    /// Trimmed, upper-cased, single-spaced — and nothing else.
    /// </summary>
    /// <remarks>
    /// FT8's fields hold no lower case, so upper-casing is what the format does
    /// rather than a liberty taken with the operator's words. **Nothing else is
    /// changed**: no callsign is corrected, no grid completed, no report signed,
    /// and no word is added or removed. A message that is wrong is transmitted
    /// wrong or refused, never quietly fixed.
    /// </remarks>
    private static string Normalise(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return string.Empty;
        }

        return string.Join(
            ' ',
            text.ToUpperInvariant().Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
    }
}
