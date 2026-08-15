using Hamlet.RadioEngine.Civ;
using Hamlet.RadioEngine.Cw;
using Hamlet.RadioEngine.Rig;
using Xunit;

namespace Hamlet.App.Tests.ViewModels;

/// <summary>
/// The notice about the back of the radio is gone (HM-DEC-083).
/// </summary>
/// <remarks>
/// <para>HM-DEC-081 retired it on evidence, which was the right shape and still
/// one screen of standing prose too many. Tim asked for it to go and it has
/// gone.</para>
/// <para>What answers the question it was gesturing at is the chain report,
/// which says what the power meter and the SWR meter actually read during the
/// send. **A sentence with a number in it beats a paragraph admitting
/// ignorance**, and that is the whole trade.</para>
/// </remarks>
public sealed class DummyLoadNoticeTests
{
    private static readonly DateTime Now = new(2026, 8, 15, 20, 0, 0, DateTimeKind.Utc);

    private static RigState State() => RigState.Empty.With(new[]
    {
        RigValue.Known(RigField.Mode, (int)CivMode.Cw, "CW", Now, "CI-V 04"),
    });

    /// <remarks>
    /// Proves HM-DEC-083: it is not shown, ever, in any state. A test rather
    /// than an absence, so somebody reinstating it has to argue with this.
    /// </remarks>
    [Fact]
    public void TheNoticeIsNeverShown()
    {
        foreach (var state in new[]
                 {
                     RigState.Empty,
                     State(),
                     State().With(
                         RigValue.Known(RigField.RfPower, 5, "5%", Now, "CI-V 14 0A")),
                     State().With(
                         RigValue.Known(RigField.RfPower, 99, "99%", Now, "CI-V 14 0A")),
                 })
        {
            var notes = TransmitNotes.For(state);

            Assert.DoesNotContain(
                notes, n => n.Contains("back of the radio", StringComparison.Ordinal));
            Assert.DoesNotContain(
                notes, n => n.Contains("dummy load", StringComparison.OrdinalIgnoreCase));
        }
    }

    /// <remarks>
    /// Proves HM-DEC-083: removing it took nothing else with it. The power line
    /// is a consequence of a value Hamlet read and still earns its place.
    /// </remarks>
    [Fact]
    public void RemovingItLeavesTheMeasuredNotesAlone()
    {
        var quiet = State().With(
            RigValue.Known(RigField.RfPower, 5, "5%", Now, "CI-V 14 0A"));

        var notes = TransmitNotes.For(quiet);

        Assert.Single(notes);
        Assert.Contains("percent of its range", notes[0], StringComparison.Ordinal);

        // And an ordinary radio says nothing at all rather than something bland.
        Assert.Empty(TransmitNotes.For(State()));
    }
}
