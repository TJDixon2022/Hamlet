using Hamlet.App.ViewModels;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.ViewModels;

/// <summary>
/// Work instruction 419 task 2 item 2, criterion 7.5: the panel's overload
/// sentence does not ask the operator to change a preamp the mode's tune-in set.
/// </summary>
/// <remarks>
/// <para>**THE COMPLAINT TIM HEARD.** The CW row sets the preamp to 1 above 40 m,
/// and while the front end reads overloading the panel then told him to press
/// P.AMP/ATT until the preamp reads off - the setup and the panel deciding one
/// knob in opposite directions within a second. Task 1's table printed it at
/// 14.050 overloading.</para>
/// <para>**SCOPED, NOT DELETED** (HM-DEC-148 stands). Where no tune-in owns the
/// preamp the sentence names the button word for word as before.</para>
/// </remarks>
public sealed class TheOverloadSentenceLeavesTheModesFieldsTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the sentences are printed.</param>
    public TheOverloadSentenceLeavesTheModesFieldsTests(ITestOutputHelper output)
        => _output = output;

    /// <summary>
    /// With the preamp owned by the mode, the sentence still says the front end is
    /// overloading and does not ask him to press P.AMP/ATT.
    /// </summary>
    /// <param name="preampIsOn">Whether the preamp reads on.</param>
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void OwnedItStatesTheOverloadAndAsksNothingOfTheKnob(bool preampIsOn)
    {
        var said = MainWindowViewModel.OverflowAdviceFor(true, preampIsOn, frontEndOwned: true);

        _output.WriteLine(said);

        Assert.Contains("front end is overloading", said, StringComparison.Ordinal);
        Assert.DoesNotContain("P.AMP/ATT", said, StringComparison.Ordinal);
    }

    /// <summary>Not owned, the button is named as before.</summary>
    /// <param name="preampIsOn">Whether the preamp reads on.</param>
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void NotOwnedTheButtonIsStillNamed(bool preampIsOn)
    {
        Assert.Contains(
            "P.AMP/ATT",
            MainWindowViewModel.OverflowAdviceFor(true, preampIsOn),
            StringComparison.Ordinal);
    }
}
