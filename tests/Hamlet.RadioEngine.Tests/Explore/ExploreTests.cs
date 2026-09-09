using Hamlet.RadioEngine.Bands;
using Hamlet.RadioEngine.Explore;
using Xunit;

namespace Hamlet.RadioEngine.Tests.Explore;

public sealed class ExploreTests
{
    /// <remarks>
    /// Proves: 40 m neighborhoods tile the band, in order, first edge to last
    /// edge, and every jump spot lands inside its own neighborhood. A map with
    /// gaps or teleporting doors is worse than no map.
    /// <para>Each block ends one hertz below the next so no frequency belongs
    /// to two of them, which is what lets the card under the map give a single
    /// answer for where the dial is pointing (HM-DEC-054).</para>
    /// </remarks>
    [Fact]
    public void FortyMeterNeighborhoods_TileTheBand()
    {
        var band = HfBands.Bands.First(b => b.Name == "40 m");
        var hoods = NeighborhoodPlan.ForBand(band);

        Assert.Equal(band.LowHz, hoods[0].LowHz);
        Assert.Equal(band.HighHz, hoods[^1].HighHz);

        for (var i = 1; i < hoods.Count; i++)
        {
            Assert.Equal(hoods[i - 1].HighHz + 1, hoods[i].LowHz);
        }

        Assert.All(hoods, h => Assert.True(h.Contains(h.JumpHz), h.Name));

        // Single-valued: no frequency in the band answers to two blocks.
        foreach (var hz in new long[]
                 { 7_000_000, 7_030_000, 7_074_000, 7_078_000, 7_125_000, 7_300_000 })
        {
            Assert.Single(hoods, h => h.Contains(hz));
        }
    }

    /// <remarks>Proves: every band gets a truthful map — never empty, always
    /// within the band — even before its editorial map is written.</remarks>
    [Fact]
    public void EveryBand_GetsAnHonestMap()
    {
        foreach (var band in HfBands.Bands)
        {
            var hoods = NeighborhoodPlan.ForBand(band);
            Assert.NotEmpty(hoods);
            Assert.All(hoods, h =>
            {
                Assert.InRange(h.LowHz, band.LowHz, band.HighHz);
                Assert.InRange(h.HighHz, band.LowHz, band.HighHz);
                Assert.False(string.IsNullOrWhiteSpace(h.Blurb));
            });
        }
    }

    /// <remarks>Proves: every field-guide entry that claims a 40 m home
    /// actually points inside 40 m, so "take me where it lives" never
    /// teleports out of band.</remarks>
    [Fact]
    public void ModeGuide_HomesAreReal()
    {
        var band = HfBands.Bands.First(b => b.Name == "40 m");
        foreach (var mode in ModeGuide.Modes.Where(m => m.LivesAt40mHz is not null))
        {
            Assert.InRange(mode.LivesAt40mHz!.Value, band.LowHz, band.HighHz);
        }
    }

    /// <summary>
    /// **Every field-guide home is the convention data's own number.**
    /// </summary>
    /// <remarks>
    /// <para>Work instruction 291 task 6. **A WRONG FREQUENCY IN A FIELD GUIDE IS
    /// THE SAME CLASS OF FAULT AS A WRONG ADIF TAG NAME**: the operator acts on
    /// it, and nothing on the screen tells him it was typed from memory. §0
    /// requires data that can be generated from a source of truth to come from
    /// it, and §0.2.1 forbids a frequency asserted from a model's memory.</para>
    /// <para>**THE BREAKAGE THIS CATCHES.** `ModeGuide` writes its 40 m homes as
    /// literals, the same way it has since the guide was built, and nothing tied
    /// them to `data/bands/us-neighborhoods.json` — which is the file the map
    /// draws from and the *take me there* press tunes to. A literal one digit out
    /// would send the operator to an empty stretch of band while the map beside
    /// it drew the block correctly, and `ModeGuide_HomesAreReal` above would not
    /// notice, because it only asks whether the number is inside 40 m at
    /// all.</para>
    /// <para>**IT COVERS THE ROW THIS UNIT ADDED AND THE FIVE THAT WERE THERE**,
    /// and running it over the five turned up **two inherited disagreements that
    /// this unit did not cause and does not repair**. RTTY's field-guide home is
    /// 7.062 MHz and the data says 7.040; PSK31's is 7.065 and the data says
    /// 7.070. **A frequency change needs a citation** (§0, §0.2.1), and which of
    /// the two sources is right is not a session's to decide — the same reason
    /// the frequency table's missing 30 m and 17 m rows are still missing. Both
    /// are named in unit 291's report and left standing.</para>
    /// <para>**SO THE KNOWN PAIR IS ASSERTED AS A SET RATHER THAN SKIPPED.** A
    /// `continue` past a disagreement is how a defect becomes a convention. If
    /// either is ever settled this test goes red and points at the entry that
    /// settled it; if a third appears, it goes red for that too.</para>
    /// </remarks>
    [Fact]
    public void ModeGuide_HomesAreTheConventionDatasOwnNumbers()
    {
        var checkedRows = 0;
        var disagreed = new List<string>();

        foreach (var mode in ModeGuide.Modes.Where(m => m.LivesAt40mHz is not null))
        {
            var block = DigitalCallingFrequencies.Find("40 m", mode.Name);

            // **ONLY THE DIGITAL ROWS HAVE A BLOCK TO CHECK AGAINST**, because
            // that is the only table in the tree carrying a mode's dial. CW and
            // SSB are left to `ModeGuide_HomesAreReal` above, and that is said
            // here rather than passed over in silence.
            if (block is null)
            {
                continue;
            }

            checkedRows++;

            if (block.JumpHz != mode.LivesAt40mHz!.Value)
            {
                disagreed.Add(
                    mode.Name + ": guide " + mode.LivesAt40mHz.Value
                    + ", data " + block.JumpHz);
            }
        }

        // FT8, FT4, RTTY and PSK31 all have 40 m rows in the data.
        Assert.True(
            checkedRows >= 4,
            "only " + checkedRows + " field-guide homes were checked against the data");

        // **THE TWO INHERITED DISAGREEMENTS, NAMED AND NOT REPAIRED.** Reported
        // by unit 291 task 6; which source is right needs a citation and a
        // ruling, and neither is a session's to supply.
        Assert.Equal(
            new[]
            {
                "RTTY: guide 7062000, data 7040000",
                "PSK31: guide 7065000, data 7070000",
            },
            disagreed.ToArray());

        // **AND THE ROW THIS UNIT ADDED AGREES**, named rather than left to the
        // loop, since a guide with no FT4 row at all is what it fixes.
        var ft4 = ModeGuide.Modes.Single(m => m.Name == "FT4");

        Assert.Equal(ModeFamily.Digital, ft4.Family);
        Assert.Equal(
            DigitalCallingFrequencies.Find("40 m", "FT4")!.JumpHz,
            ft4.LivesAt40mHz!.Value);

        // **AND IT SAYS NOTHING ABOUT HOW LONG A SLOT IS.** The transmission
        // figure is with Tim and a guide stating either number would be
        // answering the question.
        foreach (var forbidden in new[]
                 {
                     "4.48", "5.04", "7.5", "seven and a half", "second",
                 })
        {
            Assert.DoesNotContain(forbidden, ft4.Why, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain(forbidden, ft4.Sound, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain(forbidden, ft4.Tagline, StringComparison.OrdinalIgnoreCase);
        }
    }

    /// <remarks>Proves: fixture spots carry the honesty fields — a source
    /// label and a timestamp — and tune targets that land inside a known
    /// band. The prime directive applies to sample data too.</remarks>
    [Fact]
    public async Task FakeSpots_AreHonestAndTunable()
    {
        var spots = await new FakeActivitySource().GetSpotsAsync();

        Assert.NotEmpty(spots);
        Assert.All(spots, s =>
        {
            Assert.Equal("sample", s.Source);
            Assert.False(string.IsNullOrWhiteSpace(s.Story));
            Assert.NotNull(HfBands.BandFor(s.FrequencyHz));
            Assert.True(s.HeardAtUtc <= DateTime.UtcNow);
        });
    }

    /// <remarks>Proves HM-DEC-020: the fixture feed moves. Ages are recomputed
    /// every call, and within a handful of calls the set of spots differs — so
    /// the auto-refresh path's new-arrival handling is actually exercisable
    /// instead of only appearing once live feeds land behind this seam.</remarks>
    [Fact]
    public async Task FakeSpots_ChangeBetweenCalls()
    {
        var source = new FakeActivitySource();
        var seen = new HashSet<string>();
        var stories = new List<HashSet<string>>();

        for (var i = 0; i < 6; i++)
        {
            var spots = await source.GetSpotsAsync();
            var set = spots.Select(s => s.Story).ToHashSet();
            stories.Add(set);
            seen.UnionWith(set);
        }

        // More distinct spots were reported than any single call returned.
        Assert.True(seen.Count > stories[0].Count,
            $"{seen.Count} distinct spots over 6 calls, {stories[0].Count} per call");

        // And at least one call differed from the first.
        Assert.Contains(stories, s => !s.SetEquals(stories[0]));
    }

    /// <remarks>Proves the prime directive holds through the variation: every
    /// spot from every call is still labeled sample and still lands in a real
    /// band.</remarks>
    [Fact]
    public async Task FakeSpots_StayHonestAcrossRotations()
    {
        var source = new FakeActivitySource();

        for (var i = 0; i < 12; i++)
        {
            var spots = await source.GetSpotsAsync();
            Assert.All(spots, s =>
            {
                Assert.Equal("sample", s.Source);
                Assert.NotNull(HfBands.BandFor(s.FrequencyHz));
            });
        }
    }
}
