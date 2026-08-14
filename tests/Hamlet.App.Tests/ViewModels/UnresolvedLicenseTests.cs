using Hamlet.App.ViewModels;
using Hamlet.RadioEngine.Cw;
using Hamlet.RadioEngine.Licensing;
using Hamlet.RadioEngine.Rig;
using Xunit;

namespace Hamlet.App.Tests.ViewModels;

/// <summary>
/// What the send panel says when Hamlet does not know the operator's license
/// class (HM-DEC-065, confirming HM-DEC-029).
/// </summary>
/// <remarks>
/// The rule is warn and label, never block. These tests hold both halves,
/// because the tempting mistake in a program with legal consequences on one
/// side of it is to refuse and call that safety. Refusing to key somebody's own
/// radio because a lookup service did not answer teaches them something false
/// about their own license.
/// </remarks>
public sealed class UnresolvedLicenseTests
{
    private static TransmitContext Context(LicenseClass cls)
        => new(cls, 7_030_000, GuardEnabled: true, Connected: false, null, RigState.Empty);

    /// <remarks>Proves HM-DEC-065: an unresolved class shows the label.</remarks>
    [Fact]
    public void AnUnresolvedClassIsSaidPlainlyBesideTheButtons()
    {
        var panel = new CwTransmitViewModel(() => Context(LicenseClass.Unknown));

        panel.Refresh();

        Assert.True(panel.LicenseUnresolved);
    }

    /// <remarks>Proves HM-DEC-065: a class Hamlet knows draws no label at all,
    /// so the note is a statement of what is missing rather than a standing
    /// caution everybody learns to look past.</remarks>
    [Theory]
    [InlineData(LicenseClass.Technician)]
    [InlineData(LicenseClass.General)]
    [InlineData(LicenseClass.Extra)]
    public void AKnownClassSaysNothing(LicenseClass cls)
    {
        var panel = new CwTransmitViewModel(() => Context(cls));

        panel.Refresh();

        Assert.False(panel.LicenseUnresolved);
    }

    /// <remarks>
    /// Proves HM-DEC-065 confirms HM-DEC-029 rather than amending it: the guard
    /// still permits on an unknown class, and it says what it does not know. If
    /// somebody ever "tightened" this into a refusal, this fails.
    /// </remarks>
    [Fact]
    public void TheGuardStillPermitsOnAnUnknownClass()
    {
        var decision = new TransmitGuard().Check(
            LicenseClass.Unknown, 7_030_000, TransmitMode.Cw, guardEnabled: true);

        Assert.True(decision.MayTransmit);
        Assert.False(decision.WasOverridden);
        Assert.NotEqual("", decision.Reason);
    }

    /// <remarks>
    /// Proves the label is about Hamlet rather than about the operator (§0.7).
    /// It says what Hamlet does not know and leaves the license to the person
    /// who holds it, so nothing in it tells anybody what they are obliged to do.
    /// </remarks>
    [Fact]
    public void TheLabelDoesNotScold()
    {
        var note = CwTransmitViewModel.UnresolvedLicenseNote.ToLowerInvariant();

        foreach (var scold in new[]
                 { "you must", "you should", "you need to", "be careful", "make sure" })
        {
            Assert.False(note.Contains(scold, StringComparison.Ordinal),
                $"the label says '{scold}'");
        }

        Assert.Contains("does not know", note, StringComparison.Ordinal);
    }
}
