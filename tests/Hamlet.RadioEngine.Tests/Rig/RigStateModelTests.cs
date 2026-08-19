using Hamlet.RadioEngine.Civ;
using Hamlet.RadioEngine.Rig;
using Xunit;

namespace Hamlet.RadioEngine.Tests.Rig;

/// <summary>
/// The rig state model: unknown is a state rather than a number, and every
/// documented read cites the manual (HM-DEC-050, HM-DEC-049).
/// </summary>
public sealed class RigStateModelTests
{
    private static readonly DateTime Now = new(2026, 8, 15, 12, 0, 0, DateTimeKind.Utc);

    /// <remarks>
    /// THE RULE THIS MODEL EXISTS FOR (§0.0, HM-DEC-009). A field never read
    /// answers unknown rather than zero. An S-meter is where this would be
    /// easiest to slip, because a needle at rest looks exactly like a
    /// measurement of a quiet band.
    /// </remarks>
    [Fact]
    public void AFieldNeverReadIsUnknownAndNotZero()
    {
        var state = RigState.Empty;

        foreach (var field in Enum.GetValues<RigField>())
        {
            var value = state[field];

            Assert.Equal(RigValueState.Unknown, value.State);
            Assert.Null(value.Number);
            Assert.Null(value.AtUtc);
        }

        Assert.Null(state.SMeterFraction);
        Assert.Null(state.Mode);
        Assert.Null(state.FilterBandwidthHz);
        Assert.False(state.IsTransmitting);
    }

    /// <remarks>
    /// Proves the three ways of not knowing stay apart. "This radio has no
    /// spectrum scope" is a different fact from "nobody has asked yet", which is
    /// different again from "the manual documents no command", and a UI that
    /// collapsed them would either keep waiting for something that will never
    /// arrive or blame the radio for a gap in Hamlet.
    /// </remarks>
    [Fact]
    public void UnknownUnsupportedAndUndocumentedAreThreeDifferentThings()
    {
        var unknown = RigValue.Unknown(RigField.Agc);
        var unsupported = RigValue.Unsupported(RigField.Agc, "training radio");
        var undocumented = RigValue.Undocumented(RigField.Vfo, "no read in the command table");

        Assert.Equal(RigValueState.Unknown, unknown.State);
        Assert.Equal(RigValueState.Unsupported, unsupported.State);
        Assert.Equal(RigValueState.Undocumented, undocumented.State);

        Assert.All(
            new[] { unknown, unsupported, undocumented },
            v =>
            {
                Assert.False(v.IsKnown);
                Assert.Null(v.Number);
                Assert.NotEmpty(v.Text);
            });

        // And each says something different, so the screen can too.
        Assert.Equal(3, new[] { unknown.Text, unsupported.Text, undocumented.Text }
            .Distinct().Count());
    }

    /// <remarks>
    /// Proves a fresh reading and a stale one are distinguishable. A number read
    /// four minutes ago is a fact about four minutes ago, and showing it as
    /// current is the same failure as inventing it, only harder to spot.
    /// </remarks>
    [Fact]
    public void AStaleReadingIsDistinguishableFromAFreshOne()
    {
        var fresh = RigValue.Known(RigField.SMeter, 60, "S5", Now.AddSeconds(-1), "CI-V 15 02");
        var old = RigValue.Known(RigField.SMeter, 60, "S5", Now.AddMinutes(-4), "CI-V 15 02");
        var window = TimeSpan.FromSeconds(3);

        Assert.False(fresh.IsStale(Now, window));
        Assert.True(old.IsStale(Now, window));

        Assert.Equal(TimeSpan.FromSeconds(1), fresh.Age(Now));
        Assert.Equal(TimeSpan.FromMinutes(4), old.Age(Now));

        // Something never read has no age at all, which is not the same as
        // being very old.
        Assert.Null(RigValue.Unknown(RigField.SMeter).Age(Now));
        Assert.False(RigValue.Unknown(RigField.SMeter).IsStale(Now, window));
    }

    /// <remarks>
    /// Proves the diagnostics screen lists every field including the ones
    /// nothing is known about. A screen that showed only what it had would leave
    /// somebody wondering whether a missing row means "not read" or "not a
    /// thing", which is the question the screen exists to answer.
    /// </remarks>
    [Fact]
    public void EveryFieldAppearsInTheListEvenWhenNothingIsKnown()
    {
        var all = RigState.Empty.All();

        Assert.Equal(Enum.GetValues<RigField>().Length, all.Count);
        Assert.All(all, v => Assert.Equal(RigValueState.Unknown, v.State));
    }

    /// <remarks>
    /// Proves a snapshot handed to a reader cannot change underneath them. The
    /// poll loop runs on a background thread and the UI reads on its own.
    /// </remarks>
    [Fact]
    public void AStateSnapshotIsNotChangedByALaterUpdate()
    {
        var first = RigState.Empty.With(
            RigValue.Known(RigField.SMeter, 60, "S5", Now, "CI-V 15 02"));

        var second = first.With(
            RigValue.Known(RigField.SMeter, 120, "S9", Now, "CI-V 15 02"));

        Assert.Equal(60, first[RigField.SMeter].Number);
        Assert.Equal(120, second[RigField.SMeter].Number);
        Assert.NotSame(first, second);
    }

    /// <remarks>
    /// EVERY READ CITES THE MANUAL (HM-DEC-049, §4). A command byte carried from
    /// memory is a byte nobody can check, and this project has already been
    /// wrong about one: the CW pitch was recorded as sub-command 08, which is
    /// the outer Twin PBT position, because a two-column page had been flattened
    /// and the description landed against the wrong row.
    /// </remarks>
    [Fact]
    public void EveryDocumentedReadCarriesItsManualPage()
    {
        Assert.NotEmpty(CivReads.All);

        Assert.All(CivReads.All, read =>
        {
            // **OR IT SAYS IN SO MANY WORDS THAT IT IS NOT CITED YET** (§12.4).
            // One row is in that state and it names the open issue that holds the
            // question: the transceive setting, whose sub-command came from a work
            // order rather than from a column-aware read of `A7292-4EX-6`. A page
            // number nobody had read would be worse than a marker, because it
            // would be indistinguishable from the thirty rows that were.
            // `CitationTests` proves the marker names a live open item, so this
            // shape cannot become the easy way out of a citation.
            Assert.Matches(
                @"^(\d+-\d+(, \d+-\d+)*|uncited \(HM-OPEN-\d{3}\))$",
                read.Page);
            Assert.NotEmpty(read.Note);
            Assert.NotEmpty(read.Label);
        });
    }

    /// <remarks>
    /// THE CORRECTION, named so it cannot come back. Sub-command 08 is the outer
    /// Twin PBT position and 09 is the CW pitch (p. 19-3). Issuing 08 with a
    /// payload would move somebody's passband while trying to read a pitch.
    /// </remarks>
    [Fact]
    public void TheCwPitchReadIsSubCommandNine()
    {
        Assert.Equal(0x14, CivReads.CwPitch.Command);
        Assert.Equal(new byte[] { 0x09 }, CivReads.CwPitch.SubCommand);
        Assert.DoesNotContain(
            CivReads.All, r => r.Command == 0x14 && r.SubCommand is [0x08]);
    }

    /// <remarks>
    /// Proves no two reads collide. Two entries with the same command and
    /// sub-command would make responses ambiguous, and the model would fill one
    /// field with another field's value.
    /// </remarks>
    [Fact]
    public void NoTwoReadsSendTheSameBytes()
    {
        var keys = CivReads.All
            .Select(r => $"{r.Command:X2}:{Convert.ToHexString(r.SubCommand)}")
            .ToList();

        Assert.Equal(keys.Count, keys.Distinct().Count());
    }

    /// <remarks>
    /// Proves the gaps are recorded rather than guessed at (§4). The command
    /// table has 07 00 and 07 01 to select a VFO and nothing that asks which is
    /// selected, so the model says so instead of assuming A.
    /// </remarks>
    [Fact]
    public void AFieldWithNoDocumentedCommandIsRecordedAsSuch()
    {
        Assert.Contains(RigField.Vfo, CivReads.Undocumented.Keys);
        Assert.Null(CivReads.For(RigField.Vfo));
        Assert.NotEmpty(CivReads.Undocumented[RigField.Vfo]);
    }
}
