using System.Text;
using Hamlet.RadioEngine.Cw;

namespace Hamlet.App.ViewModels;

/// <summary>
/// What has been decoded so far, on its way from the audio thread to the
/// screen.
/// </summary>
/// <remarks>
/// <para>A pipe rather than a document. Characters are appended from whichever
/// thread the audio arrived on and drained by the terminal on the UI thread at
/// its own pace, which is the same shape the waterfall uses and for the same
/// reason (HM-DEC-006): at high speed a decode produces about forty characters
/// a second, and pushing each one through a property change would be forty
/// layout passes a second for text nobody can read that fast anyway.</para>
/// <para>The ViewModel holds this and the control subscribes to it directly,
/// which is the arrangement HM-DEC-006 already settled for spectrum. Binding a
/// growing string would rebuild and re-measure the whole transcript on every
/// character.</para>
/// <para>Nothing here interprets anything. It carries characters exactly as the
/// decoder produced them, confidence and evidence intact, because the moment
/// something in the middle started tidying the text up the screen would stop
/// being what was heard (§0.0).</para>
/// </remarks>
public sealed class CwTranscript
{
    /// <summary>How much history the transcript keeps.</summary>
    /// <remarks>
    /// A few thousand characters is a long over and several exchanges, which is
    /// as far back as anybody scrolls. Keeping everything would grow without
    /// limit through an evening left running.
    /// </remarks>
    public const int MaximumCharacters = 4_000;

    /// <summary>
    /// How far back the callsign resolver can look (HM-DEC-073).
    /// </summary>
    /// <remarks>
    /// A couple of hundred characters is several overs at any speed, which is
    /// far more than a callsign needs and short enough that the resolver never
    /// reaches back into a previous contact. The drained queue cannot serve
    /// this, because the control consumes it as it draws.
    /// </remarks>
    public const int RecentCharacters = 240;

    private readonly object _gate = new();
    private readonly Queue<CwCharacter> _pending = new();
    private readonly Queue<CwCharacter> _recent = new();
    private readonly StringBuilder _text = new();

    private int _version;

    /// <summary>How many characters have been decoded since the last clear.</summary>
    public int CharacterCount
    {
        get
        {
            lock (_gate)
            {
                return _text.Length;
            }
        }
    }

    /// <summary>
    /// Increments whenever the transcript is cleared, so a reader can tell a
    /// clear from a quiet moment.
    /// </summary>
    public int Version
    {
        get
        {
            lock (_gate)
            {
                return _version;
            }
        }
    }

    /// <summary>True when nothing has been decoded yet.</summary>
    public bool IsEmpty => CharacterCount == 0;

    /// <summary>How many characters the leading edge may run ahead by.</summary>
    /// <remarks>
    /// A hundred and twenty is about ten seconds at a middling speed, which is
    /// far more than the settled pass ever runs behind by. It exists so that a
    /// settled pass which never catches up cannot grow the tip without limit,
    /// and reaching it is a fault rather than a working state.
    /// </remarks>
    public const int LongestTip = 120;

    private readonly Queue<CwCharacter> _tip = new();

    private DateTime _settledThrough = DateTime.MinValue;

    /// <summary>
    /// Offer a reading from the leading edge (HM-DEC-096).
    /// </summary>
    /// <param name="character">What the streaming pass read.</param>
    /// <remarks>
    /// <para>**THE TIP IS WHAT THE SETTLED PASS HAS NOT REACHED YET, AND IT IS
    /// NOT THE TRANSCRIPT.** A provisional reading is right far more often than
    /// not and it is never final, so showing one as though it were is §0.0
    /// broken by omission however good a guess it usually is. It waits here
    /// until the second pass overtakes it and then goes away, replaced by
    /// whatever that pass made of the same audio.</para>
    /// <para>**EXCEPT WHEN NOTHING IS COMING BEHIND IT.** Where the settled pass
    /// has refused, the engine stamps the character
    /// <see cref="CwReadingStage.Unstable"/>, and then waiting is waiting
    /// forever: the reading is committed to the transcript at once and carries
    /// the mark saying nothing confirmed it. Losing the text entirely would be
    /// worse than showing it marked, and the moment somebody answers a call is
    /// the worst possible moment for the live feed to go dark.</para>
    /// </remarks>
    public void Offer(CwCharacter character)
    {
        ArgumentNullException.ThrowIfNull(character);

        if (character.IsUnstable)
        {
            Append(character);
            return;
        }

        lock (_gate)
        {
            _tip.Enqueue(character);

            while (_tip.Count > LongestTip)
            {
                _tip.Dequeue();
            }
        }
    }

    /// <summary>
    /// Take a reading from the settled pass, which is what the transcript keeps.
    /// </summary>
    /// <param name="character">What the second pass read.</param>
    /// <remarks>
    /// Everything at or before this character's own moment leaves the tip,
    /// because the second pass has now spoken about that audio and its answer is
    /// the one that stands (HM-DEC-096).
    /// </remarks>
    public void Settle(CwCharacter character)
    {
        ArgumentNullException.ThrowIfNull(character);

        Append(character);

        lock (_gate)
        {
            _settledThrough = DateTime.UnixEpoch + character.At;

            while (_tip.Count > 0
                   && DateTime.UnixEpoch + _tip.Peek().At <= _settledThrough)
            {
                _tip.Dequeue();
            }
        }
    }

    /// <summary>
    /// What the leading edge is reading that the settled pass has not reached.
    /// </summary>
    public string TipText
    {
        get
        {
            lock (_gate)
            {
                return _tip.Count == 0
                    ? ""
                    : string.Concat(_tip.Select(c => c.Text));
            }
        }
    }

    /// <summary>True when the leading edge is ahead of the settled pass.</summary>
    public bool HasTip
    {
        get
        {
            lock (_gate)
            {
                return _tip.Count > 0;
            }
        }
    }

    /// <summary>Add a character. Safe from any thread.</summary>
    /// <param name="character">The character.</param>
    public void Append(CwCharacter character)
    {
        lock (_gate)
        {
            _pending.Enqueue(character);
            _recent.Enqueue(character);
            _text.Append(character.Text);

            while (_recent.Count > RecentCharacters)
            {
                _recent.Dequeue();
            }

            if (_text.Length > MaximumCharacters * 2)
            {
                _text.Remove(0, _text.Length - MaximumCharacters);
            }
        }
    }

    /// <summary>
    /// Hand over everything appended since the last call.
    /// </summary>
    /// <param name="into">Destination, appended to.</param>
    /// <returns>How many characters were handed over.</returns>
    public int Drain(List<CwCharacter> into)
    {
        ArgumentNullException.ThrowIfNull(into);

        lock (_gate)
        {
            var count = _pending.Count;

            while (_pending.Count > 0)
            {
                into.Add(_pending.Dequeue());
            }

            return count;
        }
    }

    /// <summary>
    /// The last few characters, for the callsign resolver (HM-DEC-073).
    /// </summary>
    /// <returns>A copy, oldest first, safe to read from any thread.</returns>
    /// <remarks>
    /// A copy rather than the queue, because the resolver runs on the UI thread
    /// and the decoder appends from the audio thread. Handing out the live
    /// collection would be a race that shows up as a crash on the one evening
    /// somebody is actually using it.
    /// </remarks>
    public IReadOnlyList<CwCharacter> Recent()
    {
        lock (_gate)
        {
            return _recent.ToArray();
        }
    }

    /// <summary>Throw everything away and start again.</summary>
    public void Clear()
    {
        lock (_gate)
        {
            _pending.Clear();
            _recent.Clear();
            _text.Clear();
            _tip.Clear();
            _settledThrough = DateTime.MinValue;
            _version++;
        }
    }

    /// <summary>
    /// The transcript as plain text, confidence discarded.
    /// </summary>
    /// <remarks>
    /// For the panel summary and for anything that wants the words without the
    /// marking. The unreadable placeholders stay in it, because taking them out
    /// would produce a tidier line that says something different from what was
    /// heard.
    /// </remarks>
    public string PlainText
    {
        get
        {
            lock (_gate)
            {
                return _text.ToString();
            }
        }
    }

    /// <summary>The last few characters, for a collapsed panel's summary.</summary>
    /// <param name="count">How many characters to take.</param>
    /// <returns>The tail of the transcript.</returns>
    public string Tail(int count)
    {
        lock (_gate)
        {
            var take = Math.Min(count, _text.Length);
            return _text.ToString(_text.Length - take, take).TrimStart();
        }
    }
}
