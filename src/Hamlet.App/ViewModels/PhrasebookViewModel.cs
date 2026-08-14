using Hamlet.RadioEngine.Cw;
using Hamlet.RadioEngine.Explore;

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

/// <summary>One family chip, with its live count (HM-DEC-061).</summary>
/// <remarks>
/// THE COUNT SHOWS EVEN WHEN THE FAMILY IS SWITCHED OFF, which is the teaching
/// part. Somebody who filters to Morse and still sees forty-one voice stations
/// learns the band is full of people they could talk to; a filtered-out family
/// that went silent would teach the opposite.
/// </remarks>
public sealed class FamilyChipViewModel
{
    /// <summary>Wraps one chip for the panel.</summary>
    /// <param name="chip">The chip.</param>
    public FamilyChipViewModel(FamilyChip chip)
    {
        Family = chip.Family;
        Label = chip.Label;
        IsOn = chip.IsOn;
        Count = chip.Count;
        Name = chip.Family.ToString();
    }

    /// <summary>Which family.</summary>
    public ModeFamily Family { get; }

    /// <summary>The family's enum name, for the command parameter.</summary>
    public string Name { get; }

    /// <summary>What the chip says.</summary>
    public string Label { get; }

    /// <summary>Whether this family is being shown.</summary>
    public bool IsOn { get; }

    /// <summary>How many there are, on or off.</summary>
    public int Count { get; }

    /// <summary>The chip's face, e.g. "Morse 12".</summary>
    public string Text => $"{Label} {Count}";
}
