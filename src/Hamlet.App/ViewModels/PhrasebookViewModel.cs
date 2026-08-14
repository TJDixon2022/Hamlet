using Hamlet.RadioEngine.Cw;

namespace Hamlet.App.ViewModels;

/// <summary>One column of the phrasebook.</summary>
/// <param name="Heading">What the column is called.</param>
/// <param name="Note">The sentence at the head of it, or "".</param>
/// <param name="Phrases">Its phrases.</param>
/// <param name="IsForNewcomers">
/// True for the column about being new, which the panel draws differently
/// because it is the one somebody needs and would never ask for.
/// </param>
public sealed record PhraseColumn(
    string Heading, string Note, IReadOnlyList<CwPhrase> Phrases, bool IsForNewcomers);

/// <summary>
/// The phrases people actually send, in columns (HM-DEC-059).
/// </summary>
/// <remarks>
/// A view over <see cref="CwPhrasebook"/> and nothing more. The words and the
/// groupings are the engine's, so the panel cannot drift away from the book or
/// quietly acquire a phrase of its own.
/// </remarks>
public sealed class PhrasebookViewModel
{
    /// <summary>Builds the columns from the book.</summary>
    public PhrasebookViewModel()
        => Columns = Enum.GetValues<PhraseKind>()
            .Select(kind => new PhraseColumn(
                CwPhrasebook.Heading(kind),
                kind == PhraseKind.NewOperator ? CwPhrasebook.NewOperatorNote : "",
                CwPhrasebook.OfKind(kind),
                kind == PhraseKind.NewOperator))
            .ToList();

    /// <summary>The columns, in order.</summary>
    public IReadOnlyList<PhraseColumn> Columns { get; }

    /// <summary>What a shut panel says about itself (§0.5).</summary>
    public string Summary => CwPhrasebook.Summary();
}
