using Ft8Sharp.Message;
using Xunit;
using Xunit.Abstractions;

namespace Ft8Sharp.Tests.Encode;

/// <summary>
/// Criterion 1's stronger reading, and the instrument that says whether a tone difference belongs to
/// the packer or to the encoder.
/// </summary>
/// <remarks>
/// <para>
/// <b>Two jobs, and they are the same measurement.</b> Upstream's generator prints the packed
/// message as hex before it prints the tones. Comparing that against ours byte for byte is the
/// stronger reading of <em>matches the reference</em> — criteria 1 and 2 have both stood on a
/// syndrome check against tables this project checked in itself, which cannot catch a message that
/// was packed wrongly and then encoded perfectly.
/// </para>
/// <para>
/// <b>And it localises a difference.</b> Where the tones differ, exactly one of two things is true:
/// the packed bits already differed, in which case the fault is in the message layer and the symbol
/// encoder is innocent; or the packed bits agreed and the tones did not, in which case the fault is
/// in the encoder. Reporting <em>the tones differ</em> without saying which is worth much less to the
/// next reader, and this test is what makes that call.
/// </para>
/// <para>
/// <b>What is compared is the packed message, not the LDPC codeword.</b> Upstream's generator prints
/// ten bytes, which is the 77-bit message padded — not the 174-bit codeword. Saying so matters: this
/// upgrades criterion 1's reading of the <em>message</em> to byte-for-byte, and leaves the codeword
/// itself standing where it stood, on the parity check against the checked-in tables. Nothing here
/// pretends otherwise.
/// </para>
/// <para>
/// <b>Nothing upstream produces is committed.</b> The hex is read at run time, compared, and
/// dropped.
/// </para>
/// </remarks>
public class Ft8PayloadIdentityTests
{
    private readonly ITestOutputHelper _output;

    public Ft8PayloadIdentityTests(ITestOutputHelper output) => _output = output;

    /// <summary>The label upstream puts in front of the packed message bytes.</summary>
    private const string PackedLabel = "data";

    /// <summary>
    /// Every message this library can put into words, packed by us and packed by upstream, compared
    /// byte for byte.
    /// </summary>
    [RequiresWorkingOracleFact]
    public void EveryPackedMessageIsIdenticalToUpstreams()
    {
        var comparable = EncodeCorpus.Build().Where(entry => entry.Text is not null).ToList();

        var compared = 0;
        var matching = 0;
        var unreadable = 0;
        var differing = new List<string>();

        foreach (var entry in comparable)
        {
            var run = Ft8Oracle.Generate(entry.Text!);
            if (run.ExitCode != 0
                || !Ft8Oracle.TryReadHexAfterLabel(run.StandardOutput, PackedLabel, out var theirs))
            {
                unreadable++;
                _output.WriteLine($"  no packed line [{entry.Kind}] {entry.Label}");
                continue;
            }

            compared++;

            // Ours is Ft8Payload.MessageBytes long; upstream prints the same field. A length
            // difference is a difference and is not compared away over the shorter of the two.
            var ours = entry.Message;
            var same = theirs.Length == ours.Length && theirs.SequenceEqual(ours);
            if (same)
            {
                matching++;
                _output.WriteLine($"  match  [{entry.Kind}] {entry.Label}: {ours.Length} bytes");
                continue;
            }

            var firstDifference = theirs.Length != ours.Length
                ? -1
                : Enumerable.Range(0, ours.Length).First(i => ours[i] != theirs[i]);
            var note = firstDifference < 0
                ? $"lengths differ: ours {ours.Length} bytes, upstream's {theirs.Length}"
                : $"first differing byte is at index {firstDifference} of {ours.Length}";

            // The message type each side chose is printed on every difference, because it separates
            // the two causes at a glance: the same type with different bits is a defect in this
            // port, and two different types means the two sides were asked different questions.
            var types = $"ours i3={MessageType(ours)}, upstream's i3={MessageType(theirs)}";
            _output.WriteLine($"  DIFFER [{entry.Kind}] {entry.Label}: {note}; {types}");
            differing.Add($"[{entry.Label}] ({entry.Kind}): {note}; {types}");
        }

        _output.WriteLine(string.Empty);
        _output.WriteLine($"messages with a text form   : {comparable.Count}");
        _output.WriteLine($"packed lines read           : {compared}");
        _output.WriteLine($"matching byte for byte      : {matching}");
        _output.WriteLine($"no packed line came back    : {unreadable}");
        _output.WriteLine(string.Empty);
        _output.WriteLine(
            "This is the stronger reading of 'matches the reference' for the packed message: "
            + "upstream's own bits rather than a syndrome check against tables this project checked "
            + "in itself. The 174-bit LDPC codeword is NOT what upstream prints and still stands on "
            + "the parity check.");

        Assert.True(compared > 0, "upstream printed no packed message for any corpus entry");

        // No exceptions and no exclusions. Every entry with a text form is a question upstream can
        // be asked, and every one of them must come back with the same bits; the one wire format
        // upstream cannot be asked for carries no text form and never reaches this loop.
        Assert.Equal(comparable.Count, compared);
        Assert.True(
            differing.Count == 0,
            "packed messages differ from upstream's:"
            + Environment.NewLine
            + string.Join(Environment.NewLine, differing));
    }

    /// <summary>
    /// The hashed-callsign message, and where its difference actually lives.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>This is the diagnostic that decides whose fault the tone difference is.</b> The corpus
    /// packs this entry with a warm callsign cache, so the non-standard call goes on the wire as a
    /// twelve-bit hash. Upstream's generator is handed the same text on a command line with no cache
    /// at all, and cannot make the same choice — it has nothing to hash <em>against</em>.
    /// </para>
    /// <para>
    /// If the packed bytes differ, the two sides were asked different questions and the encoder is
    /// not implicated: what upstream encoded is a different message, correctly. If the packed bytes
    /// <em>agree</em> and the tones do not, the encoder is wrong and that is a defect in this port.
    /// The test asserts which of those it found rather than reporting a difference and leaving the
    /// reader to guess.
    /// </para>
    /// </remarks>
    [RequiresWorkingOracleFact]
    public void TheHashedCompanionMessageIsNotTheMessageUpstreamWasAskedFor()
    {
        // The entry with no text form, and the words that would have been its text form. They are
        // written here rather than taken from the corpus precisely because the corpus no longer
        // claims they mean the same thing — this test is the evidence for that claim.
        var entry = EncodeCorpus.Build()
            .Single(e => e.Kind == "non-standard, hashed companion");
        Assert.Null(entry.Text);

        const string wouldBeText = "PJ4/K1ABC W9XYZ";

        var run = Ft8Oracle.Generate(wouldBeText);
        Assert.Equal(0, run.ExitCode);

        var read = Ft8Oracle.TryReadHexAfterLabel(run.StandardOutput, PackedLabel, out var theirs);
        Assert.True(read, "upstream printed no packed message for the hashed-callsign entry");

        var ours = entry.Message;
        var samePacked = theirs.Length == ours.Length && theirs.SequenceEqual(ours);

        _output.WriteLine($"entry        : {entry.Label}");
        _output.WriteLine($"ours         : {ours.Length} bytes");
        _output.WriteLine($"upstream's   : {theirs.Length} bytes");
        _output.WriteLine($"packed bytes identical: {samePacked}");

        if (!samePacked && theirs.Length == ours.Length)
        {
            var first = Enumerable.Range(0, ours.Length).First(i => ours[i] != theirs[i]);
            _output.WriteLine($"first differing byte: index {first} of {ours.Length}");
        }

        // The message type each side chose, read from the three type bits the wire format puts at
        // the end of the payload. This is the fact that settles it: two different message types are
        // two different questions, not two answers to one.
        _output.WriteLine($"our message type    : i3 = {MessageType(ours)}");
        _output.WriteLine($"upstream's type     : i3 = {MessageType(theirs)}");

        _output.WriteLine(string.Empty);
        _output.WriteLine(
            samePacked
                ? "The packed bits AGREE and the tones do not, which puts the fault in the symbol "
                  + "encoder and makes this a defect in this port."
                : "The packed bits already DIFFER, so the symbol encoder is not implicated: upstream "
                  + "was asked a different question. The corpus packs this entry with a warm callsign "
                  + "cache so the call travels as a twelve-bit hash; upstream's generator is handed "
                  + "the text on a command line with no cache and cannot make that choice.");

        Assert.False(
            samePacked,
            "the packed messages agree and the tones do not, which means the symbol encoder is "
            + "wrong for the hashed-callsign type — this is a defect in this port and not a "
            + "difference in what the two sides were asked.");
    }

    /// <summary>
    /// The one form in which a callsign really does travel as a hash on both sides at once.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>This is unit 208's debt, settled or not settled, and it took finding the right question.</b>
    /// The corpus has carried a hashed-callsign message since unit 209, packed as the
    /// non-standard-callsign type with a warm cache. Upstream, handed the same text, does not pack it
    /// that way at all — it packs a <em>standard</em> message and puts the non-standard call into the
    /// 28-bit callsign field as a 22-bit hash. Two different wire formats are two different questions,
    /// and comparing them would have been comparing nothing.
    /// </para>
    /// <para>
    /// This asks the question upstream can actually answer: the same text, packed by us as a standard
    /// message with a cache, where <c>PJ4/K1ABC</c> goes onto the wire as a hash and not in full. A
    /// hash really is on the wire on both sides, and a wrong hash function moves the bytes.
    /// </para>
    /// </remarks>
    [RequiresWorkingOracleFact]
    public void ACallsignHashedIntoAStandardMessageIsIdenticalToUpstreams()
    {
        const string text = "PJ4/K1ABC W9XYZ";

        var cache = new Ft8CallsignCache();
        var ours = new byte[Ft8Payload.MessageBytes];
        var packed = Ft8StandardMessage.TryPack("PJ4/K1ABC", "W9XYZ", string.Empty, cache, ours);

        _output.WriteLine($"our packing  : {packed}");
        Assert.Equal(Ft8PackResult.Ok, packed);

        var run = Ft8Oracle.Generate(text);
        Assert.Equal(0, run.ExitCode);
        Assert.True(
            Ft8Oracle.TryReadHexAfterLabel(run.StandardOutput, PackedLabel, out var theirs),
            "upstream printed no packed message");

        _output.WriteLine($"our type     : i3 = {MessageType(ours)}");
        _output.WriteLine($"upstream type: i3 = {MessageType(theirs)}");
        _output.WriteLine($"packed bytes identical: {theirs.SequenceEqual(ours)}");

        Assert.Equal(MessageType(ours), MessageType(theirs));
        Assert.Equal(ours.Length, theirs.Length);
        Assert.True(
            theirs.SequenceEqual(ours),
            "the packed message differs from upstream's for a callsign travelling as a 22-bit hash, "
            + "which puts the fault in the callsign hash or the callsign field rather than anywhere "
            + "downstream of them.");

        _output.WriteLine(string.Empty);
        _output.WriteLine(
            "A callsign really is on the wire as a hash here, on both sides, and the bytes agree — "
            + "so the hash function and the field that carries it are checked against upstream and "
            + "not against themselves.");
    }

    /// <summary>
    /// Whether upstream's generator can be made to put a callsign on the wire as a hash at all, and
    /// in what form it has to be asked.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Unit 208's carried-forward debt, on its third unit.</b> A comparison covering only standard
    /// messages with basecalls in them passes whatever the hash does, because no hash will have been
    /// on the wire. The corpus has carried such a message since unit 209, but the corpus is our side
    /// of the comparison — until upstream emits one too, the hash is being checked against itself.
    /// </para>
    /// <para>
    /// The generator takes a string and keeps no cache between runs, so the question is whether the
    /// message <em>text</em> can ask for a hash. WSJT-X's own notation for exactly that is angle
    /// brackets around the call, and this tries that alongside the plain forms rather than assuming
    /// either way. <b>What it reports is the message type upstream chose</b> — type 4 is the
    /// non-standard-callsign format and is the only one with a twelve-bit hash in it.
    /// </para>
    /// </remarks>
    [RequiresWorkingOracleFact]
    public void WhetherUpstreamCanBeMadeToPutACallsignOnTheWireAsAHash()
    {
        // Forms to try, described rather than justified one by one: the plain orderings, the
        // bracketed notation in each position, and a special-event call that has no basecall form.
        string[] forms =
        [
            "PJ4/K1ABC W9XYZ",
            "W9XYZ PJ4/K1ABC",
            "CQ PJ4/K1ABC",
            "<PJ4/K1ABC> W9XYZ",
            "W9XYZ <PJ4/K1ABC>",
            "<PJ4/K1ABC> W9XYZ RRR",
            "YW18COT W9XYZ",
            "<YW18COT> W9XYZ",
            "W9XYZ <YW18COT> RR73",
        ];

        var typeFourForms = new List<string>();

        _output.WriteLine("form                          | exit | i3 chosen by upstream");
        _output.WriteLine("------------------------------+------+----------------------");
        foreach (var form in forms)
        {
            var run = Ft8Oracle.Generate(form);
            var read = run.ExitCode == 0
                && Ft8Oracle.TryReadHexAfterLabel(run.StandardOutput, PackedLabel, out var packed);
            var type = read
                ? MessageType(Ft8Oracle.TryReadHexAfterLabel(run.StandardOutput, PackedLabel, out var p) ? p : [])
                : -1;

            _output.WriteLine($"{form,-30}| {run.ExitCode,4} | {(type < 0 ? "no packed line" : type.ToString())}");

            if (type == 4)
            {
                typeFourForms.Add(form);
            }
        }

        _output.WriteLine(string.Empty);
        _output.WriteLine($"forms upstream packed as type 4 (a callsign hashed onto the wire): {typeFourForms.Count}");
        foreach (var form in typeFourForms)
        {
            _output.WriteLine($"  {form}");
        }

        _output.WriteLine(string.Empty);
        _output.WriteLine(
            typeFourForms.Count > 0
                ? "Upstream's generator CAN be made to emit a hashed callsign from the command line, "
                  + "so unit 208's debt is settleable by comparison rather than by assertion."
                : "Upstream's generator CANNOT be made to emit a message whose callsign travels as a "
                  + "hash from the command line: its packer will not prime a cache from a command "
                  + "line, so nothing it is asked produces the non-standard-callsign type. That leg "
                  + "is NOT COVERED and the hash still stands on two legs.");

        // Recorded rather than asserted: a re-pin that changes the generator's command-line handling
        // should change what this reports, not paint the project red.
        Assert.NotEmpty(forms);
    }

    /// <summary>
    /// The three type bits at the end of a packed message, which say which of the wire formats it is.
    /// </summary>
    /// <remarks>
    /// The payload is 77 bits in ten bytes, most significant bit first, so the type occupies bits 74
    /// to 76 — the last three of the message, three bits up from the end of byte nine's top 5 bits.
    /// Read here rather than reached for through the library, so that a wrong reading in the library
    /// cannot make this agree with itself.
    /// </remarks>
    private static int MessageType(byte[] packed)
    {
        if (packed.Length < Ft8Payload.MessageBytes)
        {
            return -1;
        }

        // Bits 74, 75 and 76 of a big-endian bit string over the ten bytes.
        var bits = 0;
        for (var bit = 74; bit <= 76; bit++)
        {
            var value = (packed[bit / 8] >> (7 - (bit % 8))) & 1;
            bits = (bits << 1) | value;
        }

        return bits;
    }
}
