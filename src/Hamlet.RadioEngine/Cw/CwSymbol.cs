namespace Hamlet.RadioEngine.Cw;

/// <summary>What one symbol of the gate's stream is (work instruction 532, HM-DEC-236).</summary>
public enum CwSymbolKind
{
    /// <summary>A dit.</summary>
    Dot,

    /// <summary>A dah.</summary>
    Dash,

    /// <summary>The letter the dots and dashes since the last letter end make has ended.</summary>
    LetterEnd,

    /// <summary>A word has ended: a space.</summary>
    WordEnd,
}

/// <summary>
/// **ONE SYMBOL OF ONE SENDER'S STREAM** (work instruction 532, HM-DEC-236): what the gate hands the reader. The owner,
/// 2026-10-02: *"If you do shape right, we should be able to pass the decoder nothing but a pattern that says dash dash
/// dot dot space dash dot dot space dot dot dot dash."*
/// </summary>
/// <param name="Kind">Dot, dash, letter end or word end.</param>
/// <remarks>
/// A letter end and a word end carry the gate's reading of everything but the letter itself - how sure the gate is of its
/// dots and dashes, the signal, the speed and the time - and a letter end carries the marks it was read from (§0.0.1). The
/// reader fills in the letter from its table and nothing else.
/// </remarks>
public sealed record CwSymbol(CwSymbolKind Kind)
{
    /// <summary>On a letter end or a word end, the gate's reading of everything but the letter.</summary>
    public CwCharacter? Reading { get; init; }

    /// <summary>On a letter end, the marks the letter was read from.</summary>
    public IReadOnlyList<CwMark>? Marks { get; init; }
}
