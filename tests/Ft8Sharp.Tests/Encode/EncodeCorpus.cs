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
/// lettered CQ forms; free text; telemetry; and the non-standard callsign type, both with the
/// companion spelled out and with it hashed through a warm cache. The last of those is unit 208's
/// carried-forward item: a corpus with no hash on the wire passes whatever the hash does.
/// </para>
/// <para>
/// <b>Small on purpose.</b> A fast inner loop is a property of this phase; these run in
/// milliseconds and the whole project still returns in seconds.
/// </para>
/// </remarks>
internal static class EncodeCorpus
{
    /// <summary>One message of the corpus: what it is, and the 77 bits it packs to.</summary>
    internal sealed record Entry(string Label, string Kind, byte[] Message, bool CarriesHashedCallsign);

    /// <summary>The callsign the hashed entries put on the wire as a hash rather than in full.</summary>
    internal const string HashedCompanion = "PJ4/K1ABC";

    /// <summary>
    /// Builds the corpus. A cache is created per call and never shared, so nothing here depends on
    /// the order the tests run in.
    /// </summary>
    internal static IReadOnlyList<Entry> Build()
    {
        var entries = new List<Entry>();

        void Standard(string label, string to, string de, string extra)
        {
            var message = new byte[Ft8Payload.MessageBytes];
            var result = Ft8StandardMessage.TryPack(to, de, extra, message);
            Assert.True(
                result == Ft8PackResult.Ok,
                $"the corpus entry '{label}' did not pack as a standard message: {result}");
            entries.Add(new Entry(label, "standard", message, false));
        }

        void FreeText(string label, string text)
        {
            var message = new byte[Ft8Payload.MessageBytes];
            var result = Ft8FreeText.TryPackText(text, message);
            Assert.True(
                result == Ft8PackResult.Ok,
                $"the corpus entry '{label}' did not pack as free text: {result}");
            entries.Add(new Entry(label, "free text", message, false));
        }

        void Telemetry(string label, byte[] bytes)
        {
            var message = new byte[Ft8Payload.MessageBytes];
            var result = Ft8FreeText.TryPackTelemetry(bytes, message);
            Assert.True(
                result == Ft8PackResult.Ok,
                $"the corpus entry '{label}' did not pack as telemetry: {result}");
            entries.Add(new Entry(label, "telemetry", message, false));
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
            entries.Add(new Entry(label, hashed ? "non-standard, hashed companion" : "non-standard", message, hashed));
        }

        // Standard: the forms a band actually carries.
        Standard("CQ with a grid", "CQ", "K1ABC", "FN42");
        Standard("CQ with no grid", "CQ", "W9XYZ", string.Empty);
        Standard("a signal report", "K1ABC", "W9XYZ", "-11");
        Standard("a report acknowledged", "K1ABC", "W9XYZ", "R-09");
        Standard("roger roger", "K1ABC", "W9XYZ", "RRR");
        Standard("seventy three", "K1ABC", "W9XYZ", "73");
        Standard("a lettered CQ", "CQ DX", "K1ABC", "FN42");
        Standard("a compound call", "CQ", "K1ABC/R", "FN42");

        // Free text and telemetry.
        FreeText("free text, full width", "TNX BOB 73 GL");
        FreeText("free text, short", "HELLO");
        Telemetry("telemetry, alternating", new byte[] { 0x12, 0x34, 0x56, 0x78, 0x9A, 0xBC, 0xDE, 0xF0, 0x12 });
        Telemetry("telemetry, minimum", new byte[9]);

        // Non-standard callsigns, with the companion spelled out and with it hashed.
        Nonstandard("non-standard, call in full", "CQ", "PJ4/K1ABC", string.Empty, hashed: false);
        Nonstandard("non-standard, hashed companion", "PJ4/K1ABC", "W9XYZ", string.Empty, hashed: true);

        return entries;
    }
}
