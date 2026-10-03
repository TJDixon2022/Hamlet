using System.Text;

namespace Hamlet.RadioEngine.Cw;

/// <summary>
/// **THE READER IS A LOOKUP TABLE** (work instruction 532, HM-DEC-236): it takes one sender's stream of dots, dashes,
/// letter ends and word ends from the gate (<see cref="CwSenderGate"/>) and looks each letter up in the Morse table.
/// </summary>
/// <remarks>
/// <para>**THE OWNER, 2026-10-02**: *"If you do shape right, we should be able to pass the decoder nothing but a pattern
/// that says dash dash dot dot space dash dot dot space dot dot dot dash. And it doesn't have to look at anything. It
/// should just take our pattern and turn it into characters. Trivial."*</para>
/// <para>**WHAT IT KEEPS, AND NOTHING ELSE.** The Morse table and its prosigns (<see cref="MorseAlphabet"/>), the
/// placeholder for a pattern the table does not hold, printed-stays-printed - a character is raised once and never
/// revised - and handing each character on as one event. Every decision about the marks is made in the gate, and
/// <c>TheReaderIsALookupTable</c> fails if any of it comes back here.</para>
/// </remarks>
public sealed class CwRunReader
{
    // The dots and dashes since the last letter end.
    private readonly StringBuilder _letter = new();

    /// <summary>Raised once for each character, a space at a word end, in the order of the stream, and never revised.</summary>
    public event Action<CwCharacter>? CharacterRead;

    /// <summary>Raised with each letter, beside <see cref="CharacterRead"/>, with the marks the gate read it from.</summary>
    /// <remarks>The evidence travels with the letter (§0.0.1).</remarks>
    public event Action<CwCharacter, IReadOnlyList<CwMark>>? RunRead;

    /// <summary>Take the gate's next symbols and raise what they spell.</summary>
    /// <param name="symbols">Dots, dashes, letter ends and word ends, in order.</param>
    public void Take(IEnumerable<CwSymbol> symbols)
    {
        ArgumentNullException.ThrowIfNull(symbols);

        foreach (var symbol in symbols)
        {
            switch (symbol.Kind)
            {
                case CwSymbolKind.Dot:
                    _letter.Append('.');
                    break;

                case CwSymbolKind.Dash:
                    _letter.Append('-');
                    break;

                case CwSymbolKind.WordEnd:
                    CharacterRead?.Invoke(symbol.Reading!);
                    break;

                case CwSymbolKind.LetterEnd:
                    Letter(symbol);
                    break;
            }
        }
    }

    // The letter the dots and dashes since the last letter end spell, looked up; the placeholder where the table holds none.
    private void Letter(CwSymbol end)
    {
        var pattern = _letter.ToString();
        var text = MorseAlphabet.Lookup(pattern);
        var reading = end.Reading!;
        var character = reading with
        {
            Text = text ?? MorseAlphabet.Unreadable,
            Pattern = pattern,
            Confidence = text is null ? CwConfidence.Unreadable : reading.Confidence,
        };

        _letter.Clear();
        CharacterRead?.Invoke(character);
        RunRead?.Invoke(character, end.Marks!);
    }
}
