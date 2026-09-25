using Hamlet.App.ViewModels;
using Xunit;

namespace Hamlet.App.Tests.ViewModels;

/// <summary>
/// Work instruction 426 task 2, criterion 7.8: inside a CW block that owns the front end,
/// the overload sentence says where the preamp is and why, now that Hamlet turns it off on
/// an overload after the tune-in as well as at it (HM-DEC-179).
/// </summary>
/// <remarks>
/// <para>**WHAT TASK 1 FOUND.** With the band overloading after the tune-in, the sentence
/// said *the preamp and the attenuator are set by this mode when you tune in, so Hamlet is
/// not asking you to change them here*, while the preamp read on and the row asked for
/// off. With the preamp followed live that is no longer the whole account: the preamp is
/// set when the front end overloads too, and the attenuator still only at the tune-in.</para>
/// <para>The overload itself is still said, word for word: nothing true is taken out.</para>
/// </remarks>
public sealed class TheOverloadSentenceSaysWhereThePreampIsTests
{
    private const string Overload =
        "The radio says its front end is overloading, which means the signal coming in is "
        + "stronger than the receiver can handle and everything in the passband is being "
        + "squashed together. Nothing will decode until that stops.";

    /// <summary>Preamp off in an owned block: it is off, as the manual has it, and why.</summary>
    [Fact]
    public void WithThePreampOffItSaysItIsOffAsTheManualHasIt()
    {
        var sentence = MainWindowViewModel.OverflowAdviceFor(
            overloading: true, preampIsOn: false, frontEndOwned: true, attenuatorIsIn: false);

        Assert.StartsWith(Overload, sentence, StringComparison.Ordinal);
        Assert.Contains("The preamp is off", sentence, StringComparison.Ordinal);
        Assert.Contains("manual", sentence, StringComparison.Ordinal);
        Assert.DoesNotContain("The preamp and the attenuator are set by this mode when you tune in", sentence, StringComparison.Ordinal);
        Assert.DoesNotContain("P.AMP/ATT", sentence, StringComparison.Ordinal);
    }

    /// <summary>
    /// Preamp on in an owned block: it says Hamlet turns it off itself once the overload
    /// holds, unless he has set it himself, and still asks him for nothing.
    /// </summary>
    [Fact]
    public void WithThePreampOnItSaysHamletTurnsItOffUnlessItIsHis()
    {
        var sentence = MainWindowViewModel.OverflowAdviceFor(
            overloading: true, preampIsOn: true, frontEndOwned: true, attenuatorIsIn: false);

        Assert.StartsWith(Overload, sentence, StringComparison.Ordinal);
        Assert.Contains("turns it off", sentence, StringComparison.Ordinal);
        Assert.Contains("yourself", sentence, StringComparison.Ordinal);
        Assert.DoesNotContain("P.AMP/ATT", sentence, StringComparison.Ordinal);
    }
}
