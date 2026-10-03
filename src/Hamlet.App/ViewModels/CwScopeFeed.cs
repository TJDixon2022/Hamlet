using Hamlet.RadioEngine.Cw;

namespace Hamlet.App.ViewModels;

/// <summary>
/// **WHAT THE LIVE CW TAB HANDS THE SCOPE** (work instruction 480, the letter over the bars):
/// the envelope detector's last four seconds and the characters the decoder settled, put on one
/// clock and made into the frame the scope draws.
/// </summary>
/// <remarks>
/// <para>**THE SCOPE TICK'S OWN WORK, MOVED HERE UNCHANGED** so a headless test can drive the
/// path the tab runs - the training radio's audio into a real decoder and a real detector - and
/// read the frame the tab would have drawn. The main view model calls <see cref="Tick"/> from
/// its twenty-a-second scope timer and <see cref="Settle"/> from the decoder's settled event.</para>
/// <para>**IT DECIDES NOTHING**: it copies what the detector holds and what the decoder settled.</para>
/// </remarks>
public sealed class CwScopeFeed
{
    private readonly CwTrainingGraph _graph = new();

    /// <summary>The graph's eight seconds, for a test to read.</summary>
    public CwTrainingGraph Graph => _graph;

    /// <summary>One scope tick: what the detector holds now, as the frame the scope draws.</summary>
    /// <param name="envelope">The detector.</param>
    /// <param name="reading">Its reading at this tick.</param>
    /// <param name="mixingHz">The pitch the decoder is mixing at, or NaN.</param>
    /// <param name="previous">The frame the scope drew last, or null.</param>
    /// <param name="scopeQuiet">In CW, the radio's scope is quiet and the detector sweeps.</param>
    /// <param name="nowUtc">Now: the right-hand edge.</param>
    /// <returns>The frame.</returns>
    public CwScopeFrame Tick(
        CwEnvelopeDetector envelope,
        CwEnvelopeReading reading,
        double mixingHz,
        CwScopeFrame? previous,
        bool scopeQuiet,
        DateTime nowUtc)
    {
        ArgumentNullException.ThrowIfNull(envelope);

        var history = envelope.History();

        // **THE BLOCKS ARE THE MARKS THAT STOOD AT THE PITCH BEING PRINTED, KEPT ONCE** (work
        // instruction 511, task 1, HM-DEC-215). Every mark still inside the window at that pitch is
        // offered each tick, so a sender's first marks, which stood before the reader printed it, are
        // drawn the moment it prints; the graph keeps each by its sequence and lets time alone take it.
        // Nobody printed, nothing is drawn: noise stands marks now and then and prints nothing.
        if (double.IsFinite(mixingHz))
        {
            var batch = envelope.MarksSince(0);

            foreach (var mark in batch.Marks)
            {
                if (Math.Abs(mark.PitchHz - mixingHz) <= CwSenderGate.PitchToleranceHz
                    && batch.HeardSeconds - mark.ToSeconds <= CwTrainingGraph.WindowSeconds)
                {
                    _graph.Stand(mark, batch.HeardSeconds, nowUtc);
                }
            }
        }

        _graph.Trim(nowUtc);

        return CwScopeFrame.From(history, envelope.HopMs, reading, mixingHz, previous, scopeQuiet) with
        {
            Training = _graph.Frame(nowUtc),
        };
    }

    /// <summary>A character the decoder settled, over the span it gave it. Called on the audio thread.</summary>
    /// <param name="character">The character.</param>
    /// <param name="heard">How much audio the decoder had heard when it settled.</param>
    /// <param name="nowUtc">Now.</param>
    public void Settle(CwCharacter character, TimeSpan heard, DateTime nowUtc)
        => _graph.Settle(character, heard, nowUtc);
}
