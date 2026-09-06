using Ft8Sharp.Encode;
using Ft8Sharp.Message;

namespace Hamlet.RadioEngine.Transmit;

/// <summary>Why a message did not become audio.</summary>
/// <remarks>
/// **Every value names what would not pack, and none of them is a fallback.** The
/// prime directive (§0.0) says never present a guess as a decode; the same
/// principle pointing the other way is the whole of this file. A message Hamlet
/// cannot say exactly comes back as one of these, never as audio for some other
/// message that happened to pack.
/// </remarks>
public enum Ft8ComposeRefusal
{
    /// <summary>Nothing was refused.</summary>
    None,

    /// <summary>There were no words to say.</summary>
    NothingToSay,

    /// <summary>
    /// The words pack into no message type this library can write, or they pack
    /// into one that does not read back as the same words.
    /// </summary>
    WillNotPack,

    /// <summary>
    /// The sample rate is one at which the port's signal length and slot layout
    /// disagree, so the audio would sit at the wrong offset inside the slot.
    /// </summary>
    /// <remarks>See <c>Ft8Waveform.RequireConsistentGeometry</c>.</remarks>
    SampleRateRefused,

    /// <summary>
    /// The base frequency would put a tone at or below DC, or at or above the
    /// Nyquist limit where it would alias into the channel as a different tone.
    /// </summary>
    BaseFrequencyRefused,
}

/// <summary>
/// One FT8 slot of audio, and what it says.
/// </summary>
/// <remarks>
/// <para>**Two texts, because for one kind of message they differ and hiding
/// that would be the fault this seam exists to prevent.** <see cref="Text"/> is
/// what the operator asked to send. <see cref="ReadsBackAs"/> is what the message
/// layer produces when the packed bits are unpacked again — the same call the
/// receive path's decoder ends in. <see cref="Ft8Composer"/> refuses to return
/// this record at all unless the two are identical **or** differ only by the angle
/// brackets the port puts round a callsign it recovered from a hash.</para>
/// <para>**<see cref="Samples"/> is whatever the route that made it produces, and
/// there are two.** <see cref="Ft8Composer.Compose"/> gives a whole slot -
/// silence, signal, silence - from <c>Ft8Waveform.SynthesizeSlot</c>, which is
/// what a decoder is handed. <see cref="Ft8Composer.ComposeSignal"/> gives the
/// signal alone, with no padding at either end, from <c>Ft8Waveform.Synthesize</c>
/// - which is what goes out on the air, because on the air the silence is time
/// rather than samples. Where the signal sits inside the padded slot is the
/// port's placement and is measured rather than asserted here.</para>
/// </remarks>
/// <param name="Text">The message, as the operator asked for it.</param>
/// <param name="ReadsBackAs">
/// The same message unpacked back out of its own bits. Equal to <see cref="Text"/>
/// except where a callsign travels as a hash, when it wears angle brackets.
/// </param>
/// <param name="Type">Which of FT8's message types carries it.</param>
/// <param name="Samples">The audio, in the range -1 to +1.</param>
/// <param name="SampleRate">Samples per second.</param>
/// <param name="BaseFrequencyHz">The audio frequency of tone 0.</param>
/// <param name="CarriesHashedCallsign">
/// True where a callsign travels as a hash rather than in full. **Measured, not
/// guessed**: the message was packed once without a callsign cache and refused
/// for want of one, and once with a cache and accepted. A receiver can only put a
/// name to that hash if it heard the full callsign in the same slot — the cache
/// in <c>Ft8SlotDecoder.Decode</c> is created per slot and dropped when the call
/// returns — so a transmission with this set **will not read back as itself on its
/// own**, and that is a fact about FT8 rather than about this seam.
/// </param>
public sealed record Ft8Transmission(
    string Text,
    string ReadsBackAs,
    Ft8MessageType Type,
    float[] Samples,
    int SampleRate,
    float BaseFrequencyHz,
    bool CarriesHashedCallsign)
{
    /// <summary>
    /// How long the audio is, in seconds, from the array and the rate.
    /// </summary>
    /// <remarks>
    /// 15 s for <see cref="Ft8Composer.Compose"/>'s padded slot and 12.64 s for
    /// <see cref="Ft8Composer.ComposeSignal"/>'s bare signal. **Measured off the
    /// array rather than stored**, so it cannot disagree with the samples it
    /// describes.
    /// </remarks>
    public double SlotSeconds => Samples.Length / (double)SampleRate;

    /// <summary>The largest absolute sample in the slot.</summary>
    /// <remarks>
    /// Measured over the array every time it is asked for rather than stored, so
    /// it cannot disagree with the samples it describes.
    /// </remarks>
    public float PeakSample
    {
        get
        {
            var peak = 0.0f;
            foreach (var sample in Samples)
            {
                var magnitude = Math.Abs(sample);
                if (magnitude > peak)
                {
                    peak = magnitude;
                }
            }

            return peak;
        }
    }
}

/// <summary>What became of one attempt to say something.</summary>
/// <remarks>
/// **The two states are not the same shape and cannot be confused.** A refusal
/// carries no transmission and a transmission carries no refusal, so there is no
/// way for a caller to read audio off a refused message by forgetting to check a
/// flag.
/// </remarks>
public readonly struct Ft8ComposeResult
{
    private Ft8ComposeResult(Ft8Transmission? transmission, Ft8ComposeRefusal refusal, string explanation)
    {
        Transmission = transmission;
        Refusal = refusal;
        Explanation = explanation;
    }

    /// <summary>The slot of audio, or null where the message was refused.</summary>
    public Ft8Transmission? Transmission { get; }

    /// <summary>Why it was refused, or <see cref="Ft8ComposeRefusal.None"/>.</summary>
    public Ft8ComposeRefusal Refusal { get; }

    /// <summary>What would not pack, in words. Empty on success.</summary>
    public string Explanation { get; }

    /// <summary>True where there is audio to play.</summary>
    public bool Composed => Transmission is not null;

    /// <summary>A composed transmission.</summary>
    internal static Ft8ComposeResult Ok(Ft8Transmission transmission) =>
        new(transmission, Ft8ComposeRefusal.None, string.Empty);

    /// <summary>A refusal, with what it was about.</summary>
    internal static Ft8ComposeResult No(Ft8ComposeRefusal refusal, string explanation) =>
        new(null, refusal, explanation);
}

/// <summary>
/// The one seam between what the operator wants to say and the audio of one FT8
/// slot.
/// </summary>
/// <remarks>
/// <para>**IT REUSES THE PORT FOR EVERY STEP AND IMPLEMENTS NONE OF THEM.**
/// Packing is <c>Ft8StandardMessage.TryPack</c>,
/// <c>Ft8NonstandardMessage.TryPack</c> and <c>Ft8FreeText.TryPackText</c>;
/// symbol assembly is <c>Ft8SymbolEncoder.Encode</c>; synthesis is
/// <c>Ft8Waveform.SynthesizeSlot</c>. **There is no Costas array here, no tone
/// table, no phase accumulator, no Gaussian and no symbol assembly of any kind**,
/// and there never will be — <c>Ft8Sharp</c> is a faithful MIT port and a second
/// encoder beside it would be a second thing to drift.</para>
/// <para>**IT KEYS NOTHING AND OPENS NOTHING.** No audio device, no thread, no
/// timer, no clock, no file, no serial port. It names no rig type, no PTT, no
/// CI-V command and not <c>TransmitAbort</c>. It takes words and returns an array
/// of floats, and that is the whole of what it does. Playing them into a radio is
/// step 3's and it will go through the abort (§0.2), which is built and proven
/// and correctly has no caller yet.</para>
/// <para>**IT REFUSES RATHER THAN GUESSING, AND THE REFUSAL IS PROVED BY A ROUND
/// TRIP THROUGH THE MESSAGE LAYER.** Every candidate packing is unpacked again
/// through <c>Ft8MessageDecoder</c> — the same call the receive path ends in — and
/// is accepted only if the text that comes back is character-for-character the
/// text that went in. That is what makes *it never returns audio for a message
/// different from the one asked for* a measured property rather than an intention:
/// a packing that quietly rounds a report, drops a suffix or loses a grid does not
/// reproduce its own words and is refused with the rest.</para>
/// <para>**WHICH ALSO SETTLES THE MESSAGE TYPE WITHOUT INTERPRETING ANYTHING**
/// (§12.1). The words are split into candidate field arrangements — the shapes
/// FT8's own message types admit, no more — and each is offered to each packer in
/// turn. Nothing here reads meaning into a message; it tries the arrangements the
/// format allows and keeps the one that survives its own round trip.</para>
/// </remarks>
public static class Ft8Composer
{
    /// <summary>The rate the decoder reads at, and this seam's default.</summary>
    /// <remarks>
    /// <c>Ft8WaterfallGeometry.SampleRate</c>'s value, taken from the port's own
    /// synthesiser rather than written again here.
    /// </remarks>
    public const int DefaultSampleRate = Ft8Waveform.DefaultSampleRate;

    /// <summary>Where tone 0 sits in the audio passband unless another is asked for.</summary>
    public const float DefaultBaseFrequencyHz = Ft8Waveform.DefaultBaseFrequency;

    /// <summary>
    /// Turns what the operator wants to say into one slot of audio.
    /// </summary>
    /// <param name="text">
    /// The message in the operator's own words — <c>CQ KC3QIS FN00</c>. Trimmed,
    /// upper-cased and reduced to single spaces before anything else happens;
    /// that is a normalisation of whitespace and case, not a reading of meaning.
    /// </param>
    /// <param name="sampleRate">
    /// Samples per second. Defaults to the decoder's <see cref="DefaultSampleRate"/>.
    /// **The port refuses any rate at which a channel symbol is not a whole number
    /// of samples**, because the slot is laid out from one length and the signal
    /// written from another; those rates come back as
    /// <see cref="Ft8ComposeRefusal.SampleRateRefused"/> rather than as an
    /// exception.
    /// </param>
    /// <param name="baseFrequencyHz">The audio frequency of tone 0.</param>
    /// <returns>A slot of audio, or a refusal naming what would not pack.</returns>
    public static Ft8ComposeResult Compose(
        string? text,
        int sampleRate = DefaultSampleRate,
        float baseFrequencyHz = DefaultBaseFrequencyHz)
        => Build(text, sampleRate, baseFrequencyHz, wholeSlot: true);

    /// <summary>
    /// Turns what the operator wants to say into the signal alone - the 12.64 s
    /// of tones, with no silence at either end.
    /// </summary>
    /// <param name="text">The message, in the operator's own words.</param>
    /// <param name="sampleRate">Samples per second.</param>
    /// <param name="baseFrequencyHz">The audio frequency of tone 0.</param>
    /// <returns>The signal, or a refusal naming what would not pack.</returns>
    /// <remarks>
    /// <para>**THIS IS THE ROUTE THAT GOES ON THE AIR, AND
    /// <see cref="Compose"/> IS THE ROUTE THAT GOES INTO A DECODER.** They differ
    /// in one call - <c>Ft8Waveform.Synthesize</c> here,
    /// <c>Ft8Waveform.SynthesizeSlot</c> there - and in nothing else. Every
    /// packing decision, every refusal and every round trip is shared, so the two
    /// cannot come to different answers about what a message says.</para>
    /// <para>**WHY THE PADDED SLOT IS THE WRONG THING TO PLAY.** The padding
    /// centres the signal, putting 1.180 s of silence at each end. On the air the
    /// silence before a transmission is not samples, it is time: the operator's
    /// audio path starts when the sequence hands it something, so playing the
    /// padded slot would key the radio and send 1.180 s of nothing, and the tones
    /// would start about seven tenths of a second later than every other station
    /// on the band. **Where the transmission starts is the caller's to state**
    /// and it is stated to <c>Ft8TransmitSequence</c> as a figure, not built into
    /// an array.</para>
    /// <para>**AND THE PADDING IS NOT TRIMMED OFF AN ARRAY TO GET HERE.** The
    /// port hands the signal over directly; trimming would be this seam deciding
    /// where a signal begins, which is the port's arithmetic and not this file's
    /// (work instruction 255, task 4).</para>
    /// </remarks>
    public static Ft8ComposeResult ComposeSignal(
        string? text,
        int sampleRate = DefaultSampleRate,
        float baseFrequencyHz = DefaultBaseFrequencyHz)
        => Build(text, sampleRate, baseFrequencyHz, wholeSlot: false);

    /// <summary>Both routes, which differ in one call.</summary>
    /// <param name="text">The message, in the operator's own words.</param>
    /// <param name="sampleRate">Samples per second.</param>
    /// <param name="baseFrequencyHz">The audio frequency of tone 0.</param>
    /// <param name="wholeSlot">
    /// True for the padded 15 s slot, false for the 12.64 s signal alone.
    /// </param>
    private static Ft8ComposeResult Build(
        string? text,
        int sampleRate,
        float baseFrequencyHz,
        bool wholeSlot)
    {
        var wanted = Normalise(text);
        if (wanted.Length == 0)
        {
            return Ft8ComposeResult.No(
                Ft8ComposeRefusal.NothingToSay,
                "there were no words to send. A transmission carries a message and this one is empty.");
        }

        if (!RateIsUsable(sampleRate, out var rateExplanation))
        {
            return Ft8ComposeResult.No(Ft8ComposeRefusal.SampleRateRefused, rateExplanation);
        }

        if (!BaseFrequencyIsUsable(baseFrequencyHz, sampleRate, out var baseExplanation))
        {
            return Ft8ComposeResult.No(Ft8ComposeRefusal.BaseFrequencyRefused, baseExplanation);
        }

        var packing = PackAsItself(wanted, out var packExplanation);
        if (packing is null)
        {
            return Ft8ComposeResult.No(Ft8ComposeRefusal.WillNotPack, packExplanation);
        }

        // From here down every step is the port's. Nothing below chooses a tone,
        // places a synchronisation block or advances a phase.
        var symbols = Ft8SymbolEncoder.Encode(packing.Bits);
        var audio = wholeSlot
            ? Ft8Waveform.SynthesizeSlot(symbols, sampleRate, baseFrequencyHz)
            : Ft8Waveform.Synthesize(symbols, sampleRate, baseFrequencyHz);

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
    /// two length functions.** <c>Ft8Waveform.SampleCount</c> sizes the slot from
    /// the transmission's duration and the synthesis writes
    /// <c>SymbolCount * SamplesPerSymbol</c>; where those two disagree the signal
    /// lands at the wrong offset, and the port refuses. Asking the same question
    /// here, with the same two calls, means this seam and the port cannot come to
    /// different answers.
    /// </remarks>
    public static bool RateIsUsable(int sampleRate, out string explanation)
    {
        if (sampleRate <= 0)
        {
            explanation =
                $"{sampleRate} samples per second is not a sample rate. Audio cannot be synthesised "
                + "at zero or fewer.";
            return false;
        }

        var fromDuration = Ft8Waveform.SampleCount(sampleRate);
        var fromSymbols = Ft8Waveform.SymbolCount * Ft8Waveform.SamplesPerSymbol(sampleRate);
        if (fromDuration != fromSymbols)
        {
            explanation =
                $"at {sampleRate} samples per second the signal is {fromDuration} samples long measured "
                + $"from the transmission's duration and {fromSymbols} measured as "
                + $"{Ft8Waveform.SymbolCount} symbols of {Ft8Waveform.SamplesPerSymbol(sampleRate)}. "
                + "The slot is laid out from the first and written from the second, so every sample "
                + "after the signal starts would be at the wrong offset. Use a rate at which a symbol "
                + $"is a whole number of samples — {DefaultSampleRate} is the one FT8 is decoded at.";
            return false;
        }

        explanation = string.Empty;
        return true;
    }

    /// <summary>Whether every tone would fit in the channel at this rate.</summary>
    public static bool BaseFrequencyIsUsable(float baseFrequencyHz, int sampleRate, out string explanation)
    {
        if (float.IsNaN(baseFrequencyHz) || baseFrequencyHz <= 0.0f)
        {
            explanation =
                $"{baseFrequencyHz} Hz puts tone 0 at or below DC, where it is not a tone at all.";
            return false;
        }

        var top = baseFrequencyHz + ((Ft8Waveform.ToneCount - 1) * Ft8Waveform.ToneSpacingHz);
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
    /// Packs the words, and accepts a packing only if unpacking it again gives
    /// back the same words.
    /// </summary>
    /// <remarks>
    /// <para>**The round trip is the whole of the refusal.** Every candidate is
    /// offered to the packers and then read back through <c>Ft8MessageDecoder</c>,
    /// which is the call the receive path itself ends in. A packing that succeeds
    /// but does not reproduce its own text is discarded exactly like one that
    /// failed — audio for a message the operator did not ask for is the fault this
    /// method exists to prevent.</para>
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
