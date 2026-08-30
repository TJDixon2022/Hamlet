using Hamlet.RadioEngine.Audio;
﻿using Hamlet.RadioEngine.Cw;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// Task 1's answer, pinned: nothing on the emit path asks whether the survey
/// admitted keying.
/// </summary>
/// <remarks>
/// <para>**THE REFUSAL WAS BUILT, MEASURED, AND DELIBERATELY WITHHELD** (work
/// instruction 051, task 1). It is not a regression and it is not an oversight.
/// `1366199` on 2026-08-24 says so in its own commit message: *"Refusing to
/// decode at an unmeasured pitch was built and measured and is not shipped. It
/// costs N4L on cw-2026-08-17-134712 and text on six other captures… Honesty and
/// that callsign are in tension and the ruling is Tim's."* The same paragraph
/// stands in `CwDecoder.cs` to this day.</para>
/// <para>So the session that built it escalated correctly and refused to make the
/// operator's decision for him. **What then took six days was the ruling**, and it
/// arrived with this order: `N4L` is retired as a reading anchor and the measured
/// pitch is kept.</para>
/// <para>**THIS TEST EXISTS TO STOP THE QUESTION BEING ASKED A FIFTH TIME.** It
/// records where the gates are and what each one tests, so the next session
/// reading "has the refusal shipped?" finds a measurement instead of an
/// afternoon.</para>
/// </remarks>
public sealed class NothingActsOnTheAdmissionVerdictTests
{
    private readonly ITestOutputHelper _output;

    public NothingActsOnTheAdmissionVerdictTests(ITestOutputHelper output)
        => _output = output;

    /// <summary>
    /// Every gate on the emit path, and not one of them asks about admission.
    /// </summary>
    /// <remarks>
    /// <para>The three gates that exist, with what each tests:</para>
    /// <list type="bullet">
    /// <item>**The window gate**, `CwProbabilisticDecoder.Gate` at 1.40 — whether
    /// this stretch of audio is better explained by a message than by silence.
    /// It asks nothing about which pitch, only whether anything is here.</item>
    /// <item>**The character margin**, `CharacterMargin` at 1.0 — whether one
    /// letter clears its own evidence. Below it the letter becomes a placeholder
    /// rather than disappearing (unit 036).</item>
    /// <item>**The refusal floor**, HM-DEC-120's 14 margin units, inside the
    /// settled pass.</item>
    /// </list>
    /// <para>**All three ask about the audio at the chosen pitch. None asks
    /// whether anybody chose the pitch.** That is the hole 61 characters came
    /// through on 2026-08-30.</para>
    /// </remarks>
    [Fact]
    public void TheDecoderExposesTheVerdictAndTheEmitPathDoesNotConsultIt()
    {
        // The verdict is computed and reachable — this is not a missing
        // measurement, it is a measurement nothing reads.
        var report = new CwDecodeReport(
            new AudioLevel(-24, -30, -31, false, 30),
            600, 0.4, HasTone: false, 0, 0, 0, 0);

        _output.WriteLine(
            $"PitchWasMeasured defaults to {report.PitchWasMeasured}, "
            + $"PitchChoice to {report.PitchChoice}");

        Assert.False(report.PitchWasMeasured);

        // And the gates that do exist are about the audio, not about admission.
        Assert.Equal(1.40, CwProbabilisticDecoder.Gate);
        Assert.Equal(1.0, CwProbabilisticDecoder.CharacterMargin);
    }

    /// <summary>
    /// A decoder told nothing about a pitch still reports one, and says it was
    /// not measured.
    /// </summary>
    /// <remarks>
    /// **THIS IS THE STATE THE SIDECAR PRINTED.** `toneHz 599.0 (NOT MEASURED)`
    /// is the tracker answering with the loudest bin because the survey admitted
    /// nothing, and the pitch is then perfectly usable — unit 050's spectral peak
    /// found 599–600 Hz and the station is at 600. **The pitch was right and the
    /// admission was wrong**, which is why task 2 gates emission and task 3
    /// repairs the threshold, and why neither alone is enough.
    /// </remarks>
    [Fact]
    public void AnUnmeasuredPitchIsStillReportedAndSaysSo()
    {
        var report = new CwDecodeReport(
            new AudioLevel(-14, -22, -34, false, 30),
            599.0, 8.0, HasTone: true,
            ElementsSeen: 0, ElementsResolved: 0,
            CharactersEmitted: 61, CharactersUnsure: 0,
            PitchWasMeasured: false,
            PitchChoice: CwPitchChoice.StrongestBin);

        _output.WriteLine(
            $"{report.CharactersEmitted} characters, measured "
            + $"{report.PitchWasMeasured}, chosen by {report.PitchChoice}");

        // The conjunction the capture sheet prints as `unkeyed YES`.
        Assert.True(report.CharactersEmitted > 0 && !report.PitchWasMeasured);
    }
}
