using Hamlet.RadioEngine.Bands;
using Hamlet.RadioEngine.Licensing;
using Xunit;

namespace Hamlet.RadioEngine.Tests.Licensing;

/// <summary>
/// The Part 97 privileges data and the join over it (HM-DEC-029).
/// </summary>
/// <remarks>
/// These assertions are checked against 47 CFR 97.301 and 97.305 as read from
/// eCFR on 2026-08-13, not against anybody's recollection. Being wrong here
/// means telling somebody they may transmit where they may not, which is the
/// one place in Hamlet where a confident error has legal consequences.
/// </remarks>
public sealed class PrivilegeTests
{
    private static readonly PrivilegePlan Plan = new();

    private static CwBand Band(string name)
        => HfBands.Bands.First(b => b.Name == name);

    /// <remarks>
    /// Proves the shipped data file loads and carries its citations. If the
    /// embedded resource ever goes missing, Hamlet must fail loudly rather
    /// than quietly answering from nothing.
    /// </remarks>
    [Fact]
    public void Data_LoadsWithItsCitations()
    {
        var data = PrivilegeData.Current;

        Assert.NotEmpty(data.ClassBands);
        Assert.NotEmpty(data.EmissionRanges);
        Assert.Contains(data.Authorities, s => s.Id == "97.301");
        Assert.Contains(data.Authorities, s => s.Id == "97.305");
        Assert.Equal("2026-08-13", data.RetrievedUtc);

        // The ARRL chart is named, and named as convenience rather than law.
        var arrl = data.Sources.Single(s => s.Id == "arrl-chart");
        Assert.Equal("convenience", arrl.Authority);
    }

    /// <remarks>
    /// Proves the unknowns are carried loud (CLAUDE.md §4). 60 m, VHF/UHF and
    /// power limits are genuinely absent, and a file that stayed silent about
    /// that would read as complete.
    /// </remarks>
    [Fact]
    public void Data_DeclaresWhatItDoesNotCover()
    {
        var topics = PrivilegeData.Current.Unknowns.Select(u => u.Topic).ToList();

        Assert.Contains(topics, t => t.Contains("60 m", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(topics, t => t.Contains("VHF", StringComparison.OrdinalIgnoreCase));
        Assert.All(
            PrivilegeData.Current.Unknowns,
            u => Assert.False(string.IsNullOrWhiteSpace(u.Reason)));
    }

    /// <remarks>
    /// Proves the 40 m class edges match 97.301 exactly. These four numbers
    /// are the ones the band map draws and the guard rail enforces.
    /// </remarks>
    [Theory]
    // Extra reaches the bottom of 40 m; nobody else does (97.301(b)).
    [InlineData(LicenseClass.Extra, 7_010_000, true)]
    [InlineData(LicenseClass.Advanced, 7_010_000, false)]
    [InlineData(LicenseClass.General, 7_010_000, false)]
    [InlineData(LicenseClass.Technician, 7_010_000, false)]
    // From 7.025 everyone with HF has CW (97.301(c),(d),(e)).
    [InlineData(LicenseClass.General, 7_030_000, true)]
    [InlineData(LicenseClass.Technician, 7_030_000, true)]
    // Technician stops at 7.125; General has a gap to 7.175 (97.301(d),(e)).
    [InlineData(LicenseClass.Technician, 7_150_000, false)]
    [InlineData(LicenseClass.General, 7_150_000, false)]
    [InlineData(LicenseClass.Advanced, 7_150_000, true)]
    [InlineData(LicenseClass.Extra, 7_150_000, true)]
    // The phone segment: General is back in from 7.175.
    [InlineData(LicenseClass.General, 7_200_000, true)]
    [InlineData(LicenseClass.Technician, 7_200_000, false)]
    public void FortyMeters_ClassEdgesMatchTheRegulation(
        LicenseClass cls, long hz, bool mayTransmit)
        => Assert.Equal(mayTransmit, Plan.MayTransmitAnyMode(cls, hz));

    /// <remarks>
    /// Proves 97.305(a): CW rides on the class frequency table alone, so a
    /// General may send Morse anywhere their license reaches — including up in
    /// the phone segment, where it surprises people.
    /// </remarks>
    [Fact]
    public void Cw_IsAllowedWhereverTheClassMayTransmit()
    {
        var verdict = Plan.Evaluate(LicenseClass.General, 7_200_000, TransmitMode.Cw);

        Assert.True(verdict.MayTransmit);
        Assert.Equal("97.305(a)", verdict.Citation);
    }

    /// <remarks>
    /// Proves 97.307(f)(9): a Technician's HF privileges are Morse only. This
    /// is the restriction most often got wrong, and getting it wrong the
    /// generous way would put a beginner on the air illegally.
    /// </remarks>
    [Fact]
    public void Technician_OnFortyMeters_IsMorseOnly()
    {
        Assert.True(
            Plan.Evaluate(LicenseClass.Technician, 7_030_000, TransmitMode.Cw).MayTransmit);

        // Digital IS authorised here for everyone else — 97.305(c)(3)(iv) —
        // so a Technician being refused is the class restriction speaking.
        Assert.True(
            Plan.Evaluate(LicenseClass.General, 7_030_000, TransmitMode.Data).MayTransmit);

        var tech = Plan.Evaluate(LicenseClass.Technician, 7_030_000, TransmitMode.Data);
        Assert.False(tech.MayTransmit);
        Assert.Equal("97.307(f)(9)", tech.Citation);
        Assert.Contains("Morse", tech.Explanation, StringComparison.OrdinalIgnoreCase);
    }

    /// <remarks>
    /// Proves the two refusals are told apart. Voice at 7.030 is refused
    /// because voice is not authorised there for ANY class, which is a
    /// different fact from a Technician's class restriction — and an operator
    /// deciding whether to upgrade needs to know which one is stopping them.
    /// </remarks>
    [Fact]
    public void Refusals_DistinguishTheModeFromTheLicense()
    {
        var voice = Plan.Evaluate(LicenseClass.Technician, 7_030_000, TransmitMode.Phone);

        Assert.False(voice.MayTransmit);
        Assert.Equal("97.305(c)", voice.Citation);
        Assert.Contains("any class", voice.Explanation, StringComparison.OrdinalIgnoreCase);

        // No class could work voice here, so there is nobody to upgrade to.
        Assert.Equal(LicenseClass.Unknown, voice.LowestClassThatCould);
    }

    /// <remarks>
    /// Proves 97.307(f)(10): a Technician does get voice on 10 m between
    /// 28.3 and 28.5 — their one HF phone privilege, and worth being right
    /// about in the encouraging direction too.
    /// </remarks>
    [Fact]
    public void Technician_GetsVoiceOnTenMeters()
    {
        Assert.True(
            Plan.Evaluate(LicenseClass.Technician, 28_400_000, TransmitMode.Phone)
                .MayTransmit);

        // But not below 28.3, where the band is data and CW.
        Assert.False(
            Plan.Evaluate(LicenseClass.Technician, 28_100_000, TransmitMode.Phone)
                .MayTransmit);
    }

    /// <remarks>
    /// Proves 97.307(f)(11) is applied conservatively. The 7.075–7.100 phone
    /// segment is only for stations west of 130° W or south of 20° N, which
    /// excludes the contiguous US; Hamlet does not assume the operator
    /// qualifies and says which rule decided it.
    /// </remarks>
    [Fact]
    public void FortyMeterPhone_BelowSevenOneTwoFive_IsRefusedWithItsReason()
    {
        var verdict = Plan.Evaluate(LicenseClass.Extra, 7_080_000, TransmitMode.Phone);

        Assert.False(verdict.MayTransmit);
        Assert.Equal("97.307(f)(11)", verdict.Citation);
        Assert.Contains("contiguous US", verdict.Explanation, StringComparison.OrdinalIgnoreCase);
    }

    /// <remarks>
    /// Proves voice is refused down in the CW/data end for the right reason —
    /// the mode is wrong for the segment, not the license.
    /// </remarks>
    [Fact]
    public void Voice_InTheCwSegment_IsRefusedAsAModeProblem()
    {
        var verdict = Plan.Evaluate(LicenseClass.Extra, 7_030_000, TransmitMode.Phone);

        Assert.False(verdict.MayTransmit);
        Assert.Equal(PrivilegeStatus.ModeNotAuthorised, verdict.Status);
    }

    /// <remarks>
    /// Proves an unknown class yields no claim at all — not a permissive
    /// default and not a restrictive one. This is HM-DEC-009 at the one point
    /// where guessing has legal consequences.
    /// </remarks>
    [Fact]
    public void UnknownClass_ClaimsNothing()
    {
        var verdict = Plan.Evaluate(LicenseClass.Unknown, 7_030_000, TransmitMode.Cw);

        Assert.Equal(PrivilegeStatus.Unknown, verdict.Status);
        Assert.False(verdict.MayTransmit);
        Assert.Contains("does not know", verdict.Explanation, StringComparison.OrdinalIgnoreCase);
    }

    /// <remarks>
    /// Proves an unknown class produces NO overlay spans. The band map reads
    /// this list, so an empty list is what makes "draw nothing rather than
    /// guess" structural rather than a rule the control has to remember.
    /// </remarks>
    [Fact]
    public void UnknownClass_ProducesNoSpansToDraw()
        => Assert.Empty(Plan.SpansFor(Band("40 m"), LicenseClass.Unknown));

    /// <remarks>
    /// Proves the three classes produce genuinely different pictures of 40 m —
    /// the thing the operator will see change when they set their class.
    /// </remarks>
    [Fact]
    public void Technician_General_And_Extra_SeeDifferentFortyMeters()
    {
        var band = Band("40 m");

        var tech = Plan.SpansFor(band, LicenseClass.Technician);
        var gen = Plan.SpansFor(band, LicenseClass.General);
        var extra = Plan.SpansFor(band, LicenseClass.Extra);

        Assert.NotEmpty(tech);
        Assert.NotEmpty(gen);
        Assert.NotEmpty(extra);

        // Extra covers the whole band; the others do not.
        Assert.Single(extra);
        Assert.True(extra[0].MayTransmit);

        // Coverage strictly increases with privilege.
        var techCover = Plan.CoverageOf(band, LicenseClass.Technician);
        var genCover = Plan.CoverageOf(band, LicenseClass.General);
        var extraCover = Plan.CoverageOf(band, LicenseClass.Extra);

        Assert.True(techCover < genCover, $"tech {techCover:P0} < general {genCover:P0}");
        Assert.True(genCover < extraCover, $"general {genCover:P0} < extra {extraCover:P0}");
        Assert.Equal(1.0, extraCover, 6);
    }

    /// <remarks>
    /// Proves the spans tile the band with no gaps or overlaps. The map paints
    /// straight from them, so a hole would be an undrawn stretch of band and
    /// an overlap would be double-hatched.
    /// </remarks>
    [Theory]
    [InlineData("80 m")]
    [InlineData("40 m")]
    [InlineData("20 m")]
    [InlineData("15 m")]
    [InlineData("10 m")]
    public void Spans_TileTheBandExactly(string bandName)
    {
        var band = Band(bandName);

        foreach (var cls in new[]
                 {
                     LicenseClass.Technician, LicenseClass.General,
                     LicenseClass.Advanced, LicenseClass.Extra,
                 })
        {
            var spans = Plan.SpansFor(band, cls);

            Assert.NotEmpty(spans);
            Assert.Equal(band.LowHz, spans[0].LowHz);
            Assert.Equal(band.HighHz, spans[^1].HighHz);

            for (var i = 1; i < spans.Count; i++)
            {
                Assert.Equal(spans[i - 1].HighHz, spans[i].LowHz);
                Assert.NotEqual(spans[i - 1].MayTransmit, spans[i].MayTransmit);
            }
        }
    }

    /// <remarks>
    /// Proves a span's verdict agrees with a direct evaluation at any point
    /// inside it. The map and the status line must never disagree about the
    /// same frequency — they are two renderings of one answer (HM-DEC-029).
    /// </remarks>
    [Fact]
    public void Spans_AgreeWithPointEvaluation()
    {
        var band = Band("40 m");

        foreach (var cls in new[]
                 { LicenseClass.Technician, LicenseClass.General, LicenseClass.Extra })
        {
            foreach (var span in Plan.SpansFor(band, cls))
            {
                foreach (var probe in new[]
                         {
                             span.LowHz + 100,
                             span.LowHz + ((span.HighHz - span.LowHz) / 2),
                             span.HighHz - 100,
                         })
                {
                    Assert.Equal(span.MayTransmit, Plan.MayTransmitAnyMode(cls, probe));
                }
            }
        }
    }

    /// <remarks>
    /// Proves the upgrade ladder points somewhere real, which is what turns a
    /// restriction into motivation rather than a scolding.
    /// </remarks>
    [Fact]
    public void UpgradePath_PointsUpwardAndStopsAtExtra()
    {
        Assert.Equal(LicenseClass.General, Plan.UpgradeFrom(LicenseClass.Technician)!.Next);
        Assert.Equal(LicenseClass.Extra, Plan.UpgradeFrom(LicenseClass.General)!.Next);
        Assert.Null(Plan.UpgradeFrom(LicenseClass.Extra));
        Assert.Null(Plan.UpgradeFrom(LicenseClass.Unknown));
    }

    /// <remarks>
    /// Proves the "what would it unlock" answer is real: naming the lowest
    /// class that could work a frequency is what the upgrade panel quotes.
    /// </remarks>
    [Fact]
    public void LowestClassFor_NamesWhoCouldWorkIt()
    {
        Assert.Equal(
            LicenseClass.Extra, Plan.LowestClassFor(7_010_000, TransmitMode.Cw));
        Assert.Equal(
            LicenseClass.General, Plan.LowestClassFor(7_200_000, TransmitMode.Phone));
        Assert.Equal(
            LicenseClass.Novice, Plan.LowestClassFor(7_030_000, TransmitMode.Cw));
    }

    /// <remarks>
    /// Proves determinism (§5): no clock, no state, same answer every time.
    /// </remarks>
    [Fact]
    public void Evaluation_IsDeterministic()
    {
        var a = Plan.Evaluate(LicenseClass.General, 7_150_000, TransmitMode.Phone);
        var b = Plan.Evaluate(LicenseClass.General, 7_150_000, TransmitMode.Phone);

        Assert.Equal(a, b);
    }
}
