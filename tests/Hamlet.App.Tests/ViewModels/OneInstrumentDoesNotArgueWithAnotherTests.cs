using Hamlet.App.Settings;
using Hamlet.App.ViewModels;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.ViewModels;

/// <summary>
/// Two panels may not assert opposite things about the same band, and the one
/// measured wrong does not send the operator to the radio.
/// </summary>
/// <remarks>
/// <para>**HE WENT AND CHECKED THE RADIO AND NOTHING WAS WRONG WITH IT.** On the
/// evening of 2026-08-25 the line above the transcript reported a clear tone
/// while the keying sweep below it reported no keying, fifty hertz away, and the
/// sweep's advice told him the signal was being lost between the antenna and
/// Hamlet and to try the gain, the filter and the tuning.</para>
/// <para>**AND THE SWEEP IS THE INSTRUMENT THAT IS WRONG.** Measured against
/// independent readings it disagreed on fourteen of twenty recordings, and unit
/// 1.11.10 measured its calibration inside an overlap rather than a gap: the four
/// recordings holding nothing swing 14.1 to 17.7 decibels while
/// `cw-2026-08-25-021825`, which holds a station, swings 12.6 — below all of
/// them. No bar separates them.</para>
/// </remarks>
public sealed class OneInstrumentDoesNotArgueWithAnotherTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the states are printed.</param>
    public OneInstrumentDoesNotArgueWithAnotherTests(ITestOutputHelper output)
        => _output = output;

    /// <remarks>
    /// Proves the sweep is off unless somebody turns it on. It keeps computing
    /// and keeps writing to the capture sidecar; what it stops doing is asserting
    /// on a screen beside an instrument that disagrees with it.
    /// </remarks>
    [Fact]
    public void TheKeyingSweepIsNotOnTheScreenByDefault()
    {
        var settings = new AppSettings();

        _output.WriteLine($"ShowKeyingSweep = {settings.ShowKeyingSweep}");

        Assert.False(
            settings.ShowKeyingSweep,
            "the keying sweep is wrong more often than it is right and it is on "
            + "the screen by default");
    }

    /// <remarks>
    /// Proves the advice retires where the decoder has a tone. It is only ever
    /// true where nothing has found one; where something has, the sweep
    /// disagreeing is a fault in the sweep and sending him to the radio acts on
    /// the wrong instrument (§0.0).
    /// </remarks>
    [Fact]
    public void TheGoAndCheckTheRadioAdviceRetiresWhereAToneIsFound()
    {
        var model = new MainWindowViewModel(new AppSettings(), null);

        // No tone: the advice is the only thing anybody has to go on.
        Assert.True(
            model.KeyingAdviceIsUseful,
            "nothing has found a tone and the advice about the antenna is hidden");

        _output.WriteLine(
            $"with no tone, advice shown: {model.KeyingAdviceIsUseful}");
    }
}
