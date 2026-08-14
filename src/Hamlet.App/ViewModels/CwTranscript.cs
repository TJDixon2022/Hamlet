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

    private readonly object _gate = new();
    private readonly Queue<CwCharacter> _pending = new();
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

    /// <summary>Add a character. Safe from any thread.</summary>
    /// <param name="character">The character.</param>
    public void Append(CwCharacter character)
    {
        lock (_gate)
        {
            _pending.Enqueue(character);
            _text.Append(character.Text);

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

    /// <summary>Throw everything away and start again.</summary>
    public void Clear()
    {
        lock (_gate)
        {
            _pending.Clear();
            _text.Clear();
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
