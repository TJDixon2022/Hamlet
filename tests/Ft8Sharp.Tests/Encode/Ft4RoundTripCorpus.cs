using Ft8Sharp.Message;
using Xunit;

namespace Ft8Sharp.Tests.Encode;

/// <summary>
/// The messages unit 289's round trip sends: <b>a hundred and more, across the forms a band
/// actually carries</b> — compound callsigns, grids, reports and <c>RR73</c>.
/// </summary>
/// <remarks>
/// <para>
/// <b>Beside <see cref="EncodeCorpus"/> rather than instead of it.</b> That corpus is sized by the
/// clock: every entry there costs a process launch on the upstream side, and it is deliberately kept
/// at the size that still returns in about a minute. This one costs no process at all — it is
/// encoded and decoded inside one process — so it is sized by what step 1 asks for instead.
/// </para>
/// <para>
/// <b>Every entry carries the text it was composed from, and that is what the round trip is held
/// to.</b> A message is composed from fields, packed, and the composed string is the ground truth
/// the decoder's output has to equal. Nothing here takes the decoder's own reading as the answer.
/// </para>
/// <para>
/// <b>The compound callsigns are spelled out in full on the wire</b> — the non-standard-callsign
/// type with the companion written rather than hashed. A callsign that travels as a hash cannot
/// round trip inside one slot by construction: the hash resolves only against a cache that has heard
/// the call in full earlier, and a slot carrying one transmission has heard nothing. That is a
/// property of FT8 and FT4 rather than of this library, and putting a hashed entry in here would
/// manufacture a miss and report it as a decoder fault.
/// </para>
/// </remarks>
internal static class Ft4RoundTripCorpus
{
    /// <summary>One message to be sent: what it is, its text, and the 77 bits it packs to.</summary>
    internal sealed record Entry(string Label, string Kind, string Text, byte[] Message);

    private static readonly string[] Callsigns =
    [
        "K1ABC", "W9XYZ", "G4ABC", "JA1ABC", "VK2ABC",
        "PY2ABC", "KA1A", "K10ABC", "VE3XYZ", "ZL2ABC",
    ];

    private static readonly string[] Grids =
    [
        "FN42", "IO91", "PM95", "QF56", "GG66",
        "EM12", "JJ55", "AA00", "RR99", "FN31",
    ];

    /// <summary>Builds the corpus. Nothing here is cached, so no test depends on another's order.</summary>
    internal static IReadOnlyList<Entry> Build()
    {
        var entries = new List<Entry>();

        static string Spoken(string to, string de, string extra) =>
            string.Join(' ', new[] { to, de, extra }.Where(part => part.Length > 0));

        void Standard(string kind, string to, string de, string extra)
        {
            var message = new byte[Ft8Payload.MessageBytes];
            var result = Ft8StandardMessage.TryPack(to, de, extra, message);
            Assert.True(
                result == Ft8PackResult.Ok,
                $"'{Spoken(to, de, extra)}' did not pack as a standard message: {result}");
            entries.Add(new Entry(Spoken(to, de, extra), kind, Spoken(to, de, extra), message));
        }

        void Nonstandard(string kind, string to, string de, string extra)
        {
            var message = new byte[Ft8Payload.MessageBytes];
            var result = Ft8NonstandardMessage.TryPack(to, de, extra, null, message);
            Assert.True(
                result == Ft8PackResult.Ok,
                $"'{Spoken(to, de, extra)}' did not pack as a non-standard callsign message: {result}");
            entries.Add(new Entry(Spoken(to, de, extra), kind, Spoken(to, de, extra), message));
        }

        void FreeText(string kind, string text)
        {
            var message = new byte[Ft8Payload.MessageBytes];
            var result = Ft8FreeText.TryPackText(text, message);
            Assert.True(result == Ft8PackResult.Ok, $"'{text}' did not pack as free text: {result}");
            entries.Add(new Entry(text, kind, text, message));
        }

        // Calling CQ, one per callsign, one per grid — including the grid field's own corners.
        for (var i = 0; i < Callsigns.Length; i++)
        {
            Standard("CQ with a grid", "CQ", Callsigns[i], Grids[i]);
        }

        // The directed and lettered forms, which pack into the addressed-call field as tokens.
        for (var i = 0; i < Callsigns.Length; i++)
        {
            Standard("a directed CQ", "CQ DX", Callsigns[i], Grids[(i + 1) % Grids.Length]);
        }

        // Signal reports across the range the field carries, both signs and both ends, and the same
        // set again acknowledged with an R.
        string[] reports =
        [
            "-24", "-21", "-18", "-15", "-12", "-09", "-06", "-03",
            "+00", "+03", "+06", "+09", "+12", "+15",
        ];

        for (var i = 0; i < reports.Length; i++)
        {
            var to = Callsigns[i % Callsigns.Length];
            var de = Callsigns[(i + 3) % Callsigns.Length];
            Standard("a signal report", to, de, reports[i]);
            Standard("a report acknowledged", to, de, "R" + reports[i]);
        }

        // The tokens an exchange ends on. RR73 is named in step 1's criterion by itself.
        string[] endings = ["RRR", "RR73", "73", string.Empty];
        foreach (var ending in endings)
        {
            for (var i = 0; i < Callsigns.Length; i++)
            {
                Standard(
                    ending.Length == 0 ? "nothing at all" : ending,
                    Callsigns[i],
                    Callsigns[(i + 5) % Callsigns.Length],
                    ending);
            }
        }

        // Compound callsigns, spelled out in full on the wire.
        Nonstandard("a compound callsign", "CQ", "PJ4/K1ABC", string.Empty);
        Nonstandard("a compound callsign", "CQ", "VP2E/K1ABC", string.Empty);
        Nonstandard("a compound callsign", "CQ", "W4/G4ABC", string.Empty);
        Nonstandard("a compound callsign", "CQ", "VK9/JA1ABC", string.Empty);
        Nonstandard("a compound callsign", "CQ", "3D2/W9XYZ", string.Empty);
        Nonstandard("a compound callsign", "CQ", "K1ABC/VE3", string.Empty);

        // And the suffixed forms a standard message carries in its own field.
        Standard("a portable suffix", "CQ", "K1ABC/R", "FN42");
        Standard("a portable suffix", "K1ABC", "W9XYZ/R", "RRR");
        Standard("a portable suffix", "CQ", "G4ABC/P", "IO91");
        Standard("a portable suffix", "JA1ABC", "VK2ABC/R", "-05");

        // Free text, at both ends of its own alphabet and its own length.
        FreeText("free text", "TNX BOB 73 GL");
        FreeText("free text", "HELLO");
        FreeText("free text", "ABCDEFGHIJKLM");
        FreeText("free text", "0123456789");
        FreeText("free text", "A");
        FreeText("free text", "TU 73 GL OM");
        FreeText("free text", "A B C D E F G");
        FreeText("free text", "HW CPY OM");

        return entries;
    }
}
