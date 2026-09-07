using System.Globalization;
using Ft8Sharp.Deep;
using Hamlet.RadioEngine.Contacts;
using Hamlet.RadioEngine.Transmit;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Contacts;

/// <summary>
/// **A whole six-message exchange, read slot by slot, walking through all four
/// states in the order they happen.**
/// </summary>
/// <remarks>
/// <para>**THE BREAKAGE THIS WOULD HAVE CAUGHT.** Every state had a case of its
/// own and no case read one station through all of them, so **the transitions
/// were untested**: a row that reached a state and stuck there would have passed
/// every test in the tree. The one that matters most is the way back out of gone
/// quiet - a station falls silent, crosses the threshold, then comes back, and
/// the row has to stop saying *gone quiet, 4 slots* and start saying *your move*
/// on the strength of one decode. Nothing asserted that before this.</para>
/// <para>**AND THE ORDER IS THE ASSERTION, NOT THE SET.** Reading four states
/// somewhere in twelve slots is not a walk. This reads the row at every slot from
/// 0 to 11, prints the whole column, and asserts the state at each one.</para>
/// <para>**DERIVED FROM A RECORDED SCENE AND NOT FROM THE AIR.** The messages are
/// composed by <c>Ft8Composer</c>, decoded back through the
/// <c>Ft8DeepSlotDecoder</c> Hamlet actually runs, and the decoder's own answer is
/// what the ledger is fed. It is synthesized and it is not a capture - it lives in
/// <c>tests/fixtures/ft8/scenes/</c>, carries no <c>provenance: wsjtx</c>, and may
/// never be scored against the decoder's accuracy.</para>
/// <para>**THE COMPOSE-AND-DECODE HELPERS BELOW ARE A SECOND COPY OF
/// <see cref="TheBandSceneIsWhatHamletsDecoderReadTests"/>'S** and they are
/// deliberately not shared with it. Sharing would mean editing that test, and a
/// unit may run only the tests it constructs, so this one could not be run to
/// prove the refactor safe. **The two are worth joining by a unit that can run
/// both.**</para>
/// </remarks>
public sealed class TheExchangeWalksThroughTheFourStatesTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the fixture.</summary>
    /// <param name="output">Where the walk is printed.</param>
    public TheExchangeWalksThroughTheFourStatesTests(ITestOutputHelper output) =>
        _output = output;

    /// <summary>Where the committed walk scene lives, from the repository root.</summary>
    public const string RelativePath =
        "tests/fixtures/ft8/scenes/unit266-four-state-walk.corpus.txt";

    /// <summary>The rate the composer and the decoder share.</summary>
    private const int Rate = Ft8Composer.DefaultSampleRate;

    /// <summary>The peak a slot is scaled to, so nothing clips.</summary>
    private const float Peak = 0.8f;

    /// <summary>The committed walk scene's full path on this machine.</summary>
    private static string PathInTree => Path.Combine(
        Ft8SceneCorpus.Root(), RelativePath.Replace('/', Path.DirectorySeparatorChar));

    /// <summary>
    /// **THE ONE.** The row is read at every slot of the exchange and says the
    /// four things in the order they happen.
    /// </summary>
    [Fact]
    public void AWholeSixMessageExchangeWalksThroughTheFourStatesInOrder()
    {
        // WHAT THE ROW SAYS AT EVERY SLOT, WRITTEN DOWN BEFORE IT IS READ. The
        // count is part of it: a state without its slot count is half an answer,
        // and the counts are what make the silence visible.
        (int Slot, string Reads)[] expected =
        [
            (0, "your move, 0 slots"),
            (1, "waiting on him, 0 slots"),
            (2, "waiting on him, 1 slot"),
            (3, "waiting on him, 2 slots"),
            (4, "gone quiet, 4 slots"),
            (5, "gone quiet, 5 slots"),
            (6, "gone quiet, 6 slots"),
            (7, "gone quiet, 7 slots"),
            (8, "your move, 0 slots"),
            (9, "waiting on him, 0 slots"),
            (10, "complete, 0 slots"),
            (11, "complete, 0 slots"),
        ];

        var corpus = Ft8SceneCorpus.Read(PathInTree);

        _output.WriteLine(
            $"the exchange between {Ft8FourStateWalkScene.OperatorCallsign} and "
            + $"{Ft8FourStateWalkScene.Station}, six messages over "
            + $"{Ft8FourStateWalkScene.SlotCount} slots");
        _output.WriteLine(
            $"gone quiet after {Ft8ContactStates.GoneQuietAfterSlots} slots = "
            + $"{Ft8ContactStates.GoneQuietAfterSeconds:F0} s (a choice, not a "
            + "specification)");
        _output.WriteLine(string.Empty);
        _output.WriteLine($"{"slot",4}  {"what passed",-22}  the row reads");

        var seen = new List<Ft8ContactState>();

        foreach (var (slot, reads) in expected)
        {
            // **FED ONLY AS FAR AS THE SLOT BEING READ.** A ledger fed the whole
            // scene and then read at an earlier moment already knows what has not
            // happened yet, and every row would be right for the wrong reason.
            var ledger = FedThroughSlot(corpus, slot);
            var record = ledger.For(Ft8FourStateWalkScene.Station);

            Assert.NotNull(record);

            var row = Ft8ContactStates.Read(record!, corpus.SlotUtc(slot));

            var passed = corpus.Lines.Where(line => line.Slot == slot)
                .Select(line => line.Message)
                .DefaultIfEmpty("-")
                .First();

            _output.WriteLine(
                $"{slot,4}  {passed,-22}  {row.Text}");

            Assert.Equal(reads, row.Text);

            if (seen.Count == 0 || seen[^1] != row.State)
            {
                seen.Add(row.State);
            }
        }

        // THE ORDER, AS ONE STATEMENT. Six turns of state across twelve slots,
        // and all four of the words appear in it.
        Assert.Equal(
            new[]
            {
                Ft8ContactState.YourMove,
                Ft8ContactState.WaitingOnHim,
                Ft8ContactState.GoneQuiet,
                Ft8ContactState.YourMove,
                Ft8ContactState.WaitingOnHim,
                Ft8ContactState.Complete,
            },
            seen);

        Assert.Equal(4, seen.Distinct().Count());

        // AND THE EXCHANGE REALLY IS SIX MESSAGES, three each way.
        var whole = FedThroughSlot(corpus, Ft8FourStateWalkScene.SlotCount);
        var w9gap = whole.For(Ft8FourStateWalkScene.Station)!;

        Assert.Equal(3, w9gap.Heard.Count);
        Assert.Equal(3, w9gap.Sent.Count);

        // His CQ is heard and is not addressed to the operator, so it never reads
        // as an answer to anything the operator sent.
        Assert.Equal(2, w9gap.HeardToUs.Count);

        // COMPLETE ON THE ACKNOWLEDGEMENT AT SLOT 10, BEFORE THE 73 AT SLOT 11.
        Assert.True(Ft8ContactStates.IsComplete(w9gap));
        Assert.Equal("W9GAP KC3QIS 73", w9gap.Sent[^1].Message);
    }

    /// <summary>
    /// **The committed walk scene is what the decoder returned for it**, and not
    /// what the script says was sent.
    /// </summary>
    /// <remarks>
    /// The same breakage <see cref="TheBandSceneIsWhatHamletsDecoderReadTests"/>
    /// exists against, one scene over: a corpus written from the script would look
    /// identical, would make the walk above pass, and would be evidence about a
    /// text file rather than about anything Hamlet can do.
    /// </remarks>
    [Fact]
    public void TheCommittedWalkSceneIsWhatHamletsDecoderReturned()
    {
        var read = DecodeTheScene(out var lost);

        _output.WriteLine($"signals composed     : {Ft8FourStateWalkScene.Signals.Count}");
        _output.WriteLine($"decodes returned     : {read.Count}");
        _output.WriteLine($"composed but not read: {lost.Count}");
        _output.WriteLine(string.Empty);

        foreach (var line in read)
        {
            _output.WriteLine($"  {line.Slot,2} | {line.FrequencyHz,5} | {line.Message}");
        }

        if (lost.Count > 0)
        {
            _output.WriteLine(string.Empty);
            _output.WriteLine("COMPOSED BUT NOT READ BACK - every one named:");

            foreach (var line in lost)
            {
                _output.WriteLine("  " + line);
            }
        }

        // EVERY ONE OF THE SIX HAS TO SURVIVE. This scene is one exchange, so a
        // lost message is not a gap in the evidence - it is the evidence.
        Assert.Empty(lost);

        var text = Ft8SceneCorpus.Write(
            Ft8FourStateWalkScene.OperatorCallsign,
            Ft8FourStateWalkScene.SlotZeroUtc,
            read,
            nameof(TheExchangeWalksThroughTheFourStatesTests));

        var path = PathInTree;

        if (!File.Exists(path))
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            File.WriteAllText(path, text);

            Assert.Fail(
                "there was no committed walk scene, so this run wrote one to "
                + RelativePath + ". Read it, commit it, and run this again - from "
                + "here on it is verified rather than written.");
        }

        var committed = File.ReadAllText(path)
            .Replace("\r\n", "\n", StringComparison.Ordinal);

        Assert.Equal(text, committed);
    }

    /// <summary>Feeds a ledger the scene as far as one slot and no further.</summary>
    /// <param name="corpus">The recorded scene.</param>
    /// <param name="throughSlot">The last slot to feed.</param>
    /// <returns>The ledger.</returns>
    /// <remarks>
    /// **TWO DOORS, THE SAME TWO THE APPLICATION USES.** A line whose sender is
    /// the operator goes to <c>RecordSent</c> and every other line to
    /// <c>RecordHeard</c> - a station does not decode its own transmission, it
    /// says what it sent.
    /// </remarks>
    private static Ft8ContactLedger FedThroughSlot(Ft8SceneCorpus corpus, int throughSlot)
    {
        var ledger = new Ft8ContactLedger(corpus.OperatorCallsign);

        foreach (var line in corpus.Lines)
        {
            if (line.Slot > throughSlot)
            {
                continue;
            }

            var fields = Ft8MessageSplit.Split(line.Message);
            var utc = corpus.SlotUtc(line.Slot);

            var mine = fields is not null && string.Equals(
                fields.From, corpus.OperatorCallsign, StringComparison.OrdinalIgnoreCase);

            if (mine)
            {
                ledger.RecordSent(line.Message, utc);
            }
            else
            {
                ledger.RecordHeard(line.Message, utc);
            }
        }

        return ledger;
    }

    /// <summary>Composes the whole scene and decodes each slot exactly once.</summary>
    /// <param name="lost">Filled with every composed message that did not come back.</param>
    /// <returns>The decodes, in slot order and then in the decoder's own order.</returns>
    private static IReadOnlyList<Ft8SceneLine> DecodeTheScene(out IReadOnlyList<string> lost)
    {
        // **THE DECODER HAMLET ACTUALLY RUNS** (Tim's ruling, 2026-09-05), built
        // exactly as Ft8Reader builds it. A corpus read back by a decoder the
        // application does not use would be evidence about a decoder nobody uses.
        var decoder = new Ft8DeepSlotDecoder(
            osd: Ft8DeepOsdSettings.Default,
            fineSync: Ft8DeepFineSyncSettings.Default);

        var read = new List<Ft8SceneLine>();
        var missing = new List<string>();

        for (var slot = 0; slot < Ft8FourStateWalkScene.SlotCount; slot++)
        {
            var here = Ft8FourStateWalkScene.Signals.Where(s => s.Slot == slot).ToList();

            if (here.Count == 0)
            {
                continue;
            }

            var result = decoder.Decode(Sum(here));
            var texts = new List<string>();

            foreach (var message in result.Messages)
            {
                var hz = (int)Math.Round(
                    message.FrequencyHz(decoder.Geometry), MidpointRounding.AwayFromZero);

                read.Add(new Ft8SceneLine(slot, hz, message.Text));
                texts.Add(message.Text);
            }

            foreach (var signal in here)
            {
                if (!texts.Contains(signal.Text, StringComparer.Ordinal))
                {
                    missing.Add(string.Create(
                        CultureInfo.InvariantCulture,
                        $"slot {signal.Slot}, {signal.BaseFrequencyHz:F0} Hz: \"{signal.Text}\""));
                }
            }
        }

        lost = missing;

        return read;
    }

    /// <summary>Composes one slot's signals and sums them without clipping.</summary>
    /// <param name="here">The signals in this slot.</param>
    /// <returns>One slot of audio at <see cref="Rate"/>.</returns>
    /// <exception cref="InvalidOperationException">A message would not compose.</exception>
    private static float[] Sum(IReadOnlyList<Ft8SceneSignal> here)
    {
        float[]? summed = null;

        foreach (var signal in here)
        {
            var composed = Ft8Composer.Compose(signal.Text, Rate, signal.BaseFrequencyHz);

            if (!composed.Composed)
            {
                throw new InvalidOperationException(
                    $"\"{signal.Text}\" would not compose: {composed.Refusal}. "
                    + composed.Explanation);
            }

            var samples = composed.Transmission!.Samples;

            summed ??= new float[samples.Length];

            for (var i = 0; i < summed.Length; i++)
            {
                summed[i] += samples[i];
            }
        }

        var loudest = 0.0f;

        foreach (var sample in summed!)
        {
            loudest = Math.Max(loudest, Math.Abs(sample));
        }

        if (loudest > 0.0f)
        {
            var scale = Peak / loudest;

            for (var i = 0; i < summed.Length; i++)
            {
                summed[i] *= scale;
            }
        }

        return summed;
    }
}
