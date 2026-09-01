using Ft8Sharp.Message;
using Xunit;

namespace Ft8Sharp.Tests.Encode;

/// <summary>
/// Real messages, one set, shared by the geometry assertions, the independent second
/// implementation and the parity re-take — so that all three stand on the same corpus and a type
/// covered by one is covered by all of them.
/// </summary>
/// <remarks>
/// <para>
/// <b>These are messages, not payloads.</b> Step 3's first exit criterion says <em>known
/// payloads</em>, and the honest reading is the whole chain exercised end to end from text: pack,
/// CRC, 91-bit payload, codeword. A basis vector is not a message and step 1 already proved the
/// tables over all of them; this is the other reading and it is taken deliberately rather than
/// inherited.
/// </para>
/// <para>
/// <b>Every type this library builds is here.</b> Standard messages with reports, grids and the
/// lettered CQ forms; free text; telemetry; the non-standard callsign type, both with the companion
/// spelled out and with it hashed through a warm cache; and a standard message carrying a callsign
/// too long for its own field, which therefore travels as a hash.
/// </para>
/// <para>
/// <b>Which entries carry a hash, and why there are now two kinds of them.</b> Unit 208 left this
/// debt: a comparison covering only standard messages with basecalls in them passes whatever the
/// hash does, because no hash will have been on the wire. Unit 211 ran upstream's generator for the
/// first time and found that the two sides do not agree on what the words <c>PJ4/K1ABC W9XYZ</c>
/// mean. We pack them as the non-standard-callsign type with a twelve-bit hash; upstream packs a
/// <em>standard</em> message with a twenty-two-bit hash in the 28-bit callsign field. Both are
/// hashes on the wire and they are different wire formats, so:
/// </para>
/// <list type="bullet">
/// <item>the non-standard hashed-companion entry has <b>no text form</b>, exactly as telemetry has
/// none — no string makes upstream produce that message, and giving it one would compare two
/// different messages and call the difference a defect;</item>
/// <item>the <c>StandardHashed</c> entries <b>do</b> have one, and they are the leg that settles the
/// debt: a callsign is on the wire as a hash on both sides at once, so a wrong hash function moves
/// the bytes.</item>
/// </list>
/// <para>
/// <b>Widened by unit 211 and still fast.</b> Unit 209 built fourteen; the comparison against
/// upstream's own tones is worth roughly what it covers, so this is several times that across the
/// same kinds. Every entry costs one process launch on the upstream side, so the size is bounded by
/// the project still returning in about a minute rather than by how many could be written.
/// </para>
/// </remarks>
internal static class EncodeCorpus
{
    /// <summary>One message of the corpus: what it is, and the 77 bits it packs to.</summary>
    /// <param name="Text">
    /// The same message as upstream's generator takes it on its command line, or null where there
    /// is no text form of it. Added by unit 210: the comparison against upstream's own tones has to
    /// hand it a message, and the generator's only input is a string.
    /// </param>
    internal sealed record Entry(
        string Label,
        string Kind,
        byte[] Message,
        bool CarriesHashedCallsign,
        string? Text = null);

    /// <summary>The callsign the hashed entries put on the wire as a hash rather than in full.</summary>
    internal const string HashedCompanion = "PJ4/K1ABC";

    /// <summary>
    /// Builds the corpus. A cache is created per call and never shared, so nothing here depends on
    /// the order the tests run in.
    /// </summary>
    internal static IReadOnlyList<Entry> Build()
    {
        var entries = new List<Entry>();

        // The text form is assembled from the same fields the packer is given, so the string handed
        // to upstream and the bits handed to our encoder cannot drift apart by a typo in one of
        // them. Two sources for one message is how a comparison quietly stops comparing.
        static string Spoken(string to, string de, string extra) =>
            string.Join(' ', new[] { to, de, extra }.Where(part => part.Length > 0));

        void Standard(string label, string to, string de, string extra)
        {
            var message = new byte[Ft8Payload.MessageBytes];
            var result = Ft8StandardMessage.TryPack(to, de, extra, message);
            Assert.True(
                result == Ft8PackResult.Ok,
                $"the corpus entry '{label}' did not pack as a standard message: {result}");
            entries.Add(new Entry(label, "standard", message, false, Spoken(to, de, extra)));
        }

        void FreeText(string label, string text)
        {
            var message = new byte[Ft8Payload.MessageBytes];
            var result = Ft8FreeText.TryPackText(text, message);
            Assert.True(
                result == Ft8PackResult.Ok,
                $"the corpus entry '{label}' did not pack as free text: {result}");
            entries.Add(new Entry(label, "free text", message, false, text));
        }

        void Telemetry(string label, byte[] bytes)
        {
            var message = new byte[Ft8Payload.MessageBytes];
            var result = Ft8FreeText.TryPackTelemetry(bytes, message);
            Assert.True(
                result == Ft8PackResult.Ok,
                $"the corpus entry '{label}' did not pack as telemetry: {result}");

            // No text form on purpose. Telemetry is nine bytes, not a sentence, and upstream's
            // generator takes only a string — so this type is reachable by our encoder and not by
            // the comparison, and the report says so rather than letting it look covered.
            entries.Add(new Entry(label, "telemetry", message, false, Text: null));
        }

        // A standard message carrying a callsign that will not fit the 28-bit field, so it goes onto
        // the wire as a 22-bit hash instead. THIS is the form in which a hash is really on the wire
        // AND upstream can be asked the same question — see the remarks on the corpus below.
        void StandardHashed(string label, string to, string de, string extra)
        {
            var message = new byte[Ft8Payload.MessageBytes];
            var cache = new Ft8CallsignCache();
            var result = Ft8StandardMessage.TryPack(to, de, extra, cache, message);
            Assert.True(
                result == Ft8PackResult.Ok,
                $"the corpus entry '{label}' did not pack as a standard message with a hash: {result}");
            entries.Add(new Entry(
                label,
                "standard, callsign hashed",
                message,
                CarriesHashedCallsign: true,
                Spoken(to, de, extra)));
        }

        void Nonstandard(string label, string to, string de, string extra, bool hashed)
        {
            var message = new byte[Ft8Payload.MessageBytes];

            // The hashed entries need a cache holding the companion, because that is the whole
            // point of them: the call goes on the wire as 12 bits and the receiver resolves it.
            Ft8CallsignCache? cache = null;
            if (hashed)
            {
                cache = new Ft8CallsignCache();
                cache.Save(HashedCompanion);
            }

            var result = Ft8NonstandardMessage.TryPack(to, de, extra, cache, message);
            Assert.True(
                result == Ft8PackResult.Ok,
                $"the corpus entry '{label}' did not pack as a non-standard callsign message: {result}");
            entries.Add(new Entry(
                label,
                hashed ? "non-standard, hashed companion" : "non-standard",
                message,
                hashed,

                // The hashed-companion form has NO text upstream's generator can be asked for, and
                // giving it one is worse than giving it none. Unit 210 wrote the text in good faith,
                // never having been able to run the generator; unit 211 ran it and found that the
                // same words come back from upstream as a STANDARD message with the non-standard
                // call hashed into its 28-bit field — a different wire format entirely. Comparing
                // the two would have been comparing two different messages and calling the
                // difference a defect. The hash is still covered against upstream, by the
                // StandardHashed entry below, which is the form upstream can actually be asked for.
                hashed ? null : Spoken(to, de, extra)));
        }

        // Standard: the forms a band actually carries. Widened by unit 211 from unit 209's eight,
        // because criterion 2's worth is proportional to its cover — a hundred messages across the
        // kinds says far more than fourteen, and every one of them is a fresh 77-bit payload walking
        // the whole chain. Bounded by the clock rather than by ambition: the whole project still
        // returns in about a minute and this is what keeps it there.
        Standard("CQ with a grid", "CQ", "K1ABC", "FN42");
        Standard("CQ with no grid", "CQ", "W9XYZ", string.Empty);
        Standard("a signal report", "K1ABC", "W9XYZ", "-11");
        Standard("a report acknowledged", "K1ABC", "W9XYZ", "R-09");
        Standard("roger roger", "K1ABC", "W9XYZ", "RRR");
        Standard("seventy three", "K1ABC", "W9XYZ", "73");
        Standard("a lettered CQ", "CQ DX", "K1ABC", "FN42");
        Standard("a compound call", "CQ", "K1ABC/R", "FN42");

        // Every grid field the four-character form admits at its corners and in its middle, because
        // the grid packs as a base-18/base-10 number and an off-by-one there moves one tone.
        Standard("grid at the origin", "CQ", "K1ABC", "AA00");
        Standard("grid at the far corner", "CQ", "K1ABC", "RR99");
        Standard("grid in the middle", "CQ", "K1ABC", "JJ55");
        Standard("a southern grid", "CQ", "VK3ABC", "QF22");

        // Signal reports across the range the field carries, including both signs and both ends.
        Standard("a strong report", "K1ABC", "W9XYZ", "+20");
        Standard("a weak report", "K1ABC", "W9XYZ", "-20");
        Standard("a zero report", "K1ABC", "W9XYZ", "+00");
        Standard("the weakest report", "K1ABC", "W9XYZ", "-30");
        Standard("the strongest report", "K1ABC", "W9XYZ", "+30");
        Standard("a roger at zero", "K1ABC", "W9XYZ", "R+00");
        Standard("a roger, strong", "K1ABC", "W9XYZ", "R+15");
        Standard("a roger, weak", "K1ABC", "W9XYZ", "R-15");

        // The tokens the extra field admits beyond a grid or a report.
        Standard("RR73", "K1ABC", "W9XYZ", "RR73");
        Standard("nothing at all", "K1ABC", "W9XYZ", string.Empty);

        // Callsign shapes: the field packs a call as a fixed six-character pattern, and the shapes
        // differ in which of the six positions carry what. One of each.
        Standard("a two-by-three call", "CQ", "KA1ABC", "FN42");
        Standard("a one-by-three call", "CQ", "K1ABC", "EM12");
        Standard("a two-by-one call", "CQ", "KA1A", "FN31");
        Standard("a call with two digits", "CQ", "K10ABC", "FN42");
        Standard("a European call", "CQ", "G4ABC", "IO91");
        Standard("a Japanese call", "CQ", "JA1ABC", "PM95");
        Standard("an Australian call", "CQ", "VK2ABC", "QF56");
        Standard("a Brazilian call", "CQ", "PY2ABC", "GG66");
        Standard("both calls unusual", "JA1ABC", "VK2XYZ", "-05");
        Standard("a portable suffix", "K1ABC", "W9XYZ/R", "RRR");

        // The lettered and directed CQ forms, which pack into the addressed-call field as tokens
        // rather than as callsigns.
        Standard("CQ DX", "CQ DX", "W9XYZ", "EM12");
        Standard("a numbered CQ", "CQ 123", "K1ABC", "FN42");
        Standard("a two-letter CQ", "CQ EU", "G4ABC", "IO91");
        Standard("a QRZ", "QRZ", "K1ABC", "FN42");
        Standard("a directed reply", "CQ DX", "K1ABC", string.Empty);

        // Free text: the seventy-one bits pack thirteen characters from an alphabet of forty-two, so
        // the ends of that alphabet and the ends of that length are where a wrong base shows.
        FreeText("free text, full width", "TNX BOB 73 GL");
        FreeText("free text, short", "HELLO");
        FreeText("free text, thirteen characters", "ABCDEFGHIJKLM");
        FreeText("free text, digits", "0123456789");
        FreeText("free text, one character", "A");
        FreeText("free text, punctuation", "TU 73 GL OM");
        FreeText("free text, spaces", "A B C D E F G");
        FreeText("free text, an aside", "HW CPY OM");

        // A free-text entry must be a string upstream would ALSO choose free text for, and that is
        // not a formality. Our API names the type — TryPackText packs free text because it was
        // asked to — while upstream's generator is handed a string and picks the type itself. Unit
        // 211 tried "K1ABC RR73 X" here and upstream packed it as a STANDARD message: the two sides
        // were being asked different questions and the tones differed by 50 of 79 for a reason that
        // had nothing to do with either encoder. The entry was replaced rather than excused, and the
        // finding is worth more than the entry was.

        // Telemetry: nine bytes, no text form, and therefore reachable by our encoder and not by the
        // comparison. Said here rather than left for a reader to work out.
        Telemetry("telemetry, alternating", new byte[] { 0x12, 0x34, 0x56, 0x78, 0x9A, 0xBC, 0xDE, 0xF0, 0x12 });
        Telemetry("telemetry, minimum", new byte[9]);
        Telemetry("telemetry, maximum", [0x0F, .. Enumerable.Repeat((byte)0xFF, 8)]);
        Telemetry("telemetry, walking bit", new byte[] { 0x00, 0x01, 0x02, 0x04, 0x08, 0x10, 0x20, 0x40, 0x80 });

        // Non-standard callsigns, with the companion spelled out and with it hashed.
        Nonstandard("non-standard, call in full", "CQ", "PJ4/K1ABC", string.Empty, hashed: false);
        Nonstandard("non-standard, another prefix", "CQ", "VP2E/K1ABC", string.Empty, hashed: false);
        Nonstandard("non-standard, hashed companion", "PJ4/K1ABC", "W9XYZ", string.Empty, hashed: true);

        // And the form in which a callsign really does travel as a hash AND upstream can be asked
        // the same question. Unit 208's carried-forward debt lives or dies on these.
        StandardHashed("a hashed call, addressed", "PJ4/K1ABC", "W9XYZ", string.Empty);
        StandardHashed("a hashed call, transmitting", "W9XYZ", "PJ4/K1ABC", string.Empty);
        StandardHashed("a hashed call, with a report", "PJ4/K1ABC", "W9XYZ", "-11");
        StandardHashed("a hashed call, another prefix", "VP2E/K1ABC", "W9XYZ", "RRR");

        return entries;
    }
}
