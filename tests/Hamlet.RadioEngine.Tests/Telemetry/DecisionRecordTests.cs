using System.Text.Json;
using Hamlet.RadioEngine.Civ;
using Hamlet.RadioEngine.Cw;
using Hamlet.RadioEngine.Rig;
using Hamlet.RadioEngine.Telemetry;
using Xunit;

namespace Hamlet.RadioEngine.Tests.Telemetry;

/// <summary>
/// Telemetry as a decision record rather than a list of completions
/// (HM-DEC-077).
/// </summary>
/// <remarks>
/// The organizing fault these exist to fix, in one sentence: Hamlet logged what
/// it did and never what it decided. A refusal is an outcome and a failure is an
/// outcome, and both are more useful than a success nobody ever has to diagnose.
/// </remarks>
public sealed class DecisionRecordTests
{
    private static readonly DateTime Now = new(2026, 8, 15, 21, 0, 0, DateTimeKind.Utc);

    private static RigCapabilities Radio { get; } = new(
        "IC-7300", HasSpectrumScope: true, HasBuiltInCwKeyer: true,
        HasUsbAudio: true, CanTransmit: true, new[] { "40 m" });

    /// <summary>Tonight's exact reading: break-in full, not transmitting, CW.</summary>
    private static RigState TonightsState() => RigState.Empty.With(new[]
    {
        RigValue.Known(RigField.Mode, (int)CivMode.Cw, "CW", Now, "CI-V 04"),
        RigValue.Known(RigField.BreakIn, 2, "full", Now, "CI-V 16 47"),
        RigValue.Known(RigField.TransmitStatus, 0, "receiving", Now, "CI-V 1C 00"),
    });

    // ---- The state that must pass ---------------------------------------

    /// <remarks>
    /// Proves HM-DEC-077 against the exact reading from the evening this was
    /// written: break-in full, transmitting false, mode CW. **That state must
    /// produce a ready verdict**, and it must produce an event saying so, since
    /// the whole failure was that nothing said anything.
    /// </remarks>
    [Fact]
    public void TonightsReadingIsReadyAndSaysSo()
    {
        var readiness = TransmitReadiness.Check(
            connected: true, Radio, TonightsState(), Now);

        Assert.True(readiness.MaySend);
        Assert.Equal(CwReadyState.Ready, readiness.State);
        Assert.Equal(OutcomeEvent.Ok, readiness.Reason);

        var body = readiness.AsEvent();

        Assert.Equal(Outcome.Proceeded, body.Outcome);
        Assert.Equal(TelemetryLevel.Info, body.Level);

        // Everything it looked at travels with the verdict, including what
        // passed: a record of only the failing condition cannot tell "break-in
        // was read as on" from "break-in was never reached".
        var fields = body.DeterminedBy.Select(d => d.Field).ToList();

        Assert.Contains(nameof(RigField.BreakIn), fields);
        Assert.Contains(nameof(RigField.Mode), fields);
        Assert.Contains(nameof(RigField.TransmitStatus), fields);

        var breakIn = body.DeterminedBy.Single(d => d.Field == nameof(RigField.BreakIn));

        Assert.Equal(RadioEngine.Telemetry.DeterminedBy.Read, breakIn.Provenance);
        Assert.Equal(2, breakIn.Value);
    }

    // ---- Unknown and off must never look the same ------------------------

    /// <remarks>
    /// Proves HM-DEC-077: refusing on unknown is correct per HM-DEC-050 and
    /// refusing on off is something the operator can walk across the room and
    /// fix. A file that conflates them is worth nothing on the evening it is
    /// needed, and they used to produce one state and one sentence.
    /// </remarks>
    [Fact]
    public void UnknownAndOffAreDifferentInEveryWayThatMatters()
    {
        var off = TransmitReadiness.Check(
            true, Radio,
            TonightsState().With(
                RigValue.Known(RigField.BreakIn, 0, "off", Now, "CI-V 16 47")),
            Now);

        var unread = TransmitReadiness.Check(
            true, Radio,
            RigState.Empty.With(new[]
            {
                RigValue.Known(RigField.Mode, (int)CivMode.Cw, "CW", Now, "CI-V 04"),
                RigValue.Known(
                    RigField.TransmitStatus, 0, "receiving", Now, "CI-V 1C 00"),
            }),
            Now);

        Assert.False(off.MaySend);
        Assert.False(unread.MaySend);

        // Different state, different token, different sentence.
        Assert.NotEqual(off.State, unread.State);
        Assert.Equal("break_in_off", off.Reason);
        Assert.Equal("break_in_unknown", unread.Reason);
        Assert.NotEqual(off.Detail, unread.Detail);

        // And different provenance in the written record.
        var offRow = off.AsEvent().DeterminedBy
            .Single(d => d.Field == nameof(RigField.BreakIn));
        var unreadRow = unread.AsEvent().DeterminedBy
            .Single(d => d.Field == nameof(RigField.BreakIn));

        Assert.Equal(RadioEngine.Telemetry.DeterminedBy.Read, offRow.Provenance);
        Assert.Equal(0, offRow.Value);

        Assert.Equal(RadioEngine.Telemetry.DeterminedBy.Unknown, unreadRow.Provenance);
        Assert.Null(unreadRow.Value);
    }

    /// <remarks>
    /// Proves HM-DEC-077: every provenance the model can produce survives into
    /// the written bag as itself. Unknown never becomes zero (HM-DEC-050), and
    /// stale carries its age rather than passing as fresh.
    /// </remarks>
    [Fact]
    public void EveryProvenanceSurvivesIntoTheRecord()
    {
        var fresh = DeterminedBy.From(
            RigValue.Known(RigField.BreakIn, 2, "full", Now, "CI-V 16 47"),
            Now, TimeSpan.FromMinutes(1));

        var stale = DeterminedBy.From(
            RigValue.Known(RigField.BreakIn, 2, "full", Now, "CI-V 16 47"),
            Now.AddMinutes(5), TimeSpan.FromMinutes(1));

        var unknown = DeterminedBy.From(RigValue.Unknown(RigField.BreakIn), Now);

        var unsupported = DeterminedBy.From(
            RigValue.Unsupported(RigField.ScopeOn, "capabilities"), Now);

        Assert.Equal(RadioEngine.Telemetry.DeterminedBy.Read, fresh.Provenance);
        Assert.Equal(RadioEngine.Telemetry.DeterminedBy.Stale, stale.Provenance);
        Assert.Equal(RadioEngine.Telemetry.DeterminedBy.Unknown, unknown.Provenance);
        Assert.Equal(
            RadioEngine.Telemetry.DeterminedBy.Unsupported, unsupported.Provenance);

        // Stale carries how stale.
        Assert.NotNull(stale.AgeSeconds);
        Assert.True(stale.AgeSeconds > 250);

        // Unknown carries no number at all, rather than a zero.
        Assert.Null(unknown.ToBag().GetValueOrDefault("value"));
        Assert.Null(unsupported.ToBag().GetValueOrDefault("value"));
    }

    // ---- The written shape ----------------------------------------------

    /// <remarks>
    /// Proves HM-DEC-077: the event round-trips through JSON with its outcome,
    /// its reason and everything that determined it intact. A record that
    /// serializes into something unreadable is a record nobody can act on.
    /// </remarks>
    [Fact]
    public void TheOutcomeShapeRoundTripsThroughJson()
    {
        var readiness = TransmitReadiness.Check(
            true, Radio,
            TonightsState().With(
                RigValue.Known(RigField.BreakIn, 0, "off", Now, "CI-V 16 47")),
            Now);

        var bag = readiness.AsEvent().ToBag(
            new Dictionary<string, object?> { ["trigger"] = "recomputed" });

        using var document = JsonDocument.Parse(JsonSerializer.Serialize(bag));
        var root = document.RootElement;

        Assert.Equal("refused", root.GetProperty("outcome").GetString());
        Assert.Equal("break_in_off", root.GetProperty("reason").GetString());
        Assert.Equal("recomputed", root.GetProperty("trigger").GetString());

        var rows = root.GetProperty("determinedBy").EnumerateArray().ToList();

        Assert.NotEmpty(rows);

        var breakIn = rows.Single(
            r => r.GetProperty("field").GetString() == nameof(RigField.BreakIn));

        Assert.Equal("read", breakIn.GetProperty("provenance").GetString());
        Assert.Equal(0, breakIn.GetProperty("value").GetDouble());
    }

    /// <remarks>
    /// Proves HM-DEC-077: levels start meaning something. Everything was info,
    /// so nothing could be found by scanning and a reconnect nobody asked for
    /// was logged identically to a healthy one.
    /// </remarks>
    [Theory]
    [InlineData(Outcome.Proceeded, TelemetryLevel.Info)]
    [InlineData(Outcome.Refused, TelemetryLevel.Warn)]
    [InlineData(Outcome.Degraded, TelemetryLevel.Warn)]
    [InlineData(Outcome.Failed, TelemetryLevel.Error)]
    public void AnOutcomeCarriesTheLevelItDeserves(Outcome outcome, TelemetryLevel level)
        => Assert.Equal(
            level, new OutcomeEvent(outcome, "whatever", Array.Empty<DeterminedBy>()).Level);

    // ---- The rig state travels -------------------------------------------

    /// <remarks>
    /// Proves HM-DEC-077: a full snapshot carries every value with its
    /// provenance. The model held thirty-one values and not one appeared in
    /// telemetry, which is why break-in could only be learned by photographing
    /// a window.
    /// </remarks>
    [Fact]
    public void AFullSnapshotCarriesEveryValueWithItsProvenance()
    {
        var bag = RigSnapshot.Full(TonightsState(), Now);
        var rows = (IReadOnlyList<IReadOnlyDictionary<string, object?>>)bag["rig"]!;

        Assert.True(rows.Count > 20, $"only {rows.Count} fields in the snapshot");
        Assert.Equal(3, bag["rigKnownCount"]);

        var breakIn = rows.Single(
            r => (string?)r["field"] == nameof(RigField.BreakIn));

        Assert.Equal("read", breakIn["provenance"]);

        // Everything unread says unknown and carries no number.
        var unread = rows.Where(r => (string?)r["provenance"] == "unknown").ToList();

        Assert.NotEmpty(unread);
        Assert.All(unread, r => Assert.False(r.ContainsKey("value")));
    }

    /// <remarks>
    /// Proves HM-DEC-077: the heartbeat is a delta so a quiet session has a
    /// spine without thirty-one rows a minute burying the events worth finding.
    /// Ageing is not a change: everything ages every second, and treating that
    /// as a change would make every delta a full snapshot.
    /// </remarks>
    [Fact]
    public void TheHeartbeatReportsOnlyWhatChanged()
    {
        var before = TonightsState();
        var after = before.With(
            RigValue.Known(RigField.BreakIn, 0, "off", Now.AddMinutes(1), "CI-V 16 47"));

        var delta = RigSnapshot.Delta(before, after, Now.AddMinutes(1));
        var changed = (IReadOnlyList<IReadOnlyDictionary<string, object?>>)
            delta["rigChanged"]!;

        Assert.Single(changed);
        Assert.Equal(nameof(RigField.BreakIn), changed[0]["field"]);

        // Nothing changed but the clock: nothing is reported.
        var quiet = RigSnapshot.Delta(before, before, Now.AddMinutes(5));

        Assert.Empty((IReadOnlyList<IReadOnlyDictionary<string, object?>>)
            quiet["rigChanged"]!);

        // And with nothing to compare against, the first one is a full picture.
        Assert.True(RigSnapshot.Delta(null, after, Now).ContainsKey("rig"));
    }

    // ---- Nothing identifying can enter ------------------------------------

    /// <remarks>
    /// Proves HM-DEC-018 holds where the record grew most. This work expands
    /// what is logged more than anything before it, so the boundary is proved
    /// rather than assumed: a readiness event names preconditions and carries
    /// numbers, and there is nowhere in the shape for a callsign or the text
    /// being sent.
    /// </remarks>
    [Fact]
    public void AReadinessEventCannotCarryAnythingIdentifying()
    {
        var bag = TransmitReadiness
            .Check(true, Radio, TonightsState(), Now)
            .AsEvent()
            .ToBag();

        var written = JsonSerializer.Serialize(bag);

        foreach (var forbidden in new[]
                 { "KC3QIS", "W1AW", "CQ CQ", "Pittsburgh", "Timothy", "EN90" })
        {
            Assert.DoesNotContain(forbidden, written, StringComparison.OrdinalIgnoreCase);
        }

        // The shape has no member that could hold one, which is the real proof.
        var readiness = TransmitReadiness.Check(true, Radio, TonightsState(), Now);

        Assert.All(
            readiness.AsEvent().DeterminedBy,
            d => Assert.True(
                Enum.TryParse<RigField>(d.Field, out _)
                || d.Field is "connected" or "canTransmit",
                $"'{d.Field}' is not a rig field or a known fact"));
    }
}
