using System.Net;
using Hamlet.RadioEngine.Bands;
using Hamlet.RadioEngine.Licensing;
using Hamlet.RadioEngine.Rig;
using Hamlet.RadioEngine.Tests.Explore;
using Xunit;

namespace Hamlet.RadioEngine.Tests.Licensing;

/// <summary>
/// The callsign lookup (HM-DEC-028), the transmit guard rail (HM-DEC-029) and
/// the rig capabilities seam (HM-DEC-030).
/// </summary>
public sealed class LookupAndGuardTests
{
    /// <summary>A real callook.info response, trimmed to what Hamlet reads.</summary>
    private const string ValidJson = """
    {
      "status": "VALID",
      "type": "PERSON",
      "current": { "callsign": "KC3QIS", "operClass": "GENERAL" },
      "name": "A NAME THE PARSER MUST IGNORE",
      "address": { "line1": "A STREET ADDRESS", "line2": "A TOWN" },
      "location": { "gridsquare": "FN00dj" },
      "otherInfo": { "grantDate": "10/26/2020" }
    }
    """;

    private const string UnknownJson = """
    { "status": "INVALID", "current": { "callsign": "", "operClass": "" } }
    """;

    private static readonly DateTime Now = new(2026, 8, 13, 12, 0, 0, DateTimeKind.Utc);

    /// <remarks>
    /// Proves the operator class is read from a real response shape, with the
    /// provenance the profile stores beside it.
    /// </remarks>
    [Fact]
    public async Task Lookup_ReadsTheOperatorClass()
    {
        using var handler = new StubHttp(ValidJson);
        using var lookup = new CallookCallsignLookup("0.1", "KC3QIS", handler, () => Now);

        var result = await lookup.LookupAsync("kc3qis");

        Assert.NotNull(result);
        Assert.Equal(LicenseClass.General, result!.Class);
        Assert.Equal("KC3QIS", result.Callsign);
        Assert.Equal("callook.info", result.SourceName);
        Assert.Equal(Now, result.RetrievedUtc);
    }

    /// <remarks>
    /// Proves the parser takes the class and leaves the rest. The response
    /// carries the licensee's name and street address; Hamlet has no use for
    /// them and does not read them, so there is nothing to leak later.
    /// </remarks>
    [Fact]
    public async Task Lookup_ReadsNothingButTheClass()
    {
        using var handler = new StubHttp(ValidJson);
        using var lookup = new CallookCallsignLookup("0.1", "KC3QIS", handler, () => Now);

        var result = await lookup.LookupAsync("KC3QIS");

        // The result type has nowhere to put a name or an address, which is
        // the point: the restraint is in the shape, not in a convention.
        var properties = typeof(CallsignLookupResult)
            .GetProperties()
            .Select(p => p.Name)
            .ToList();

        Assert.Equal(
            new[] { "Callsign", "Class", "SourceName", "RetrievedUtc" }.OrderBy(x => x),
            properties.OrderBy(x => x));
        Assert.NotNull(result);
    }

    /// <remarks>
    /// Proves the request identifies Hamlet, its version and the operator, as
    /// HM-DEC-024 requires of every outside service.
    /// </remarks>
    [Fact]
    public async Task Lookup_IntroducesItself()
    {
        using var handler = new StubHttp(ValidJson);
        using var lookup = new CallookCallsignLookup("0.1", "KC3QIS", handler, () => Now);

        await lookup.LookupAsync("KC3QIS");

        var agent = handler.Requests.Single().Headers.UserAgent.ToString();
        Assert.Contains("Hamlet/0.1", agent, StringComparison.Ordinal);
        Assert.Contains("KC3QIS", agent, StringComparison.Ordinal);
    }

    /// <remarks>
    /// Proves "the service does not know this callsign" is an answer, not a
    /// failure — the caller must stop rather than retry forever.
    /// </remarks>
    [Fact]
    public async Task Lookup_ReturnsNullForAnUnknownCallsign()
    {
        using var handler = new StubHttp(UnknownJson);
        using var lookup = new CallookCallsignLookup("0.1", "KC3QIS", handler, () => Now);

        Assert.Null(await lookup.LookupAsync("XX0XXX"));
    }

    /// <remarks>
    /// Proves a transport failure surfaces as an exception, so the caller can
    /// tell "try later" from "no such license" and fall down the ladder
    /// without blocking anything.
    /// </remarks>
    [Fact]
    public async Task Lookup_ThrowsOnTransportFailure()
    {
        using var handler = new StubHttp("nope", HttpStatusCode.ServiceUnavailable);
        using var lookup = new CallookCallsignLookup("0.1", "KC3QIS", handler, () => Now);

        await Assert.ThrowsAnyAsync<HttpRequestException>(() => lookup.LookupAsync("KC3QIS"));
    }

    /// <remarks>
    /// Proves an unrecognized class string yields Unknown rather than the
    /// nearest guess. The whole feature exists to state privileges correctly
    /// (HM-DEC-009).
    /// </remarks>
    [Theory]
    [InlineData("GENERAL", LicenseClass.General)]
    [InlineData("Technician", LicenseClass.Technician)]
    [InlineData("AMATEUR EXTRA", LicenseClass.Extra)]
    [InlineData("EXTRA", LicenseClass.Extra)]
    [InlineData("ADVANCED", LicenseClass.Advanced)]
    [InlineData("NOVICE", LicenseClass.Novice)]
    [InlineData("SOMETHING NEW", LicenseClass.Unknown)]
    [InlineData("", LicenseClass.Unknown)]
    [InlineData(null, LicenseClass.Unknown)]
    public void Lookup_MapsClassesOrAdmitsItCannot(string? raw, LicenseClass expected)
        => Assert.Equal(expected, CallookCallsignLookup.ParseClass(raw));

    /// <remarks>
    /// Proves the guard permits what the license covers and refuses what it
    /// does not — the two cases it exists for.
    /// </remarks>
    [Fact]
    public void Guard_PermitsAndDeniesCorrectly()
    {
        var guard = new TransmitGuard();

        var ok = guard.Check(LicenseClass.General, 7_030_000, TransmitMode.Cw, true);
        Assert.True(ok.MayTransmit);
        Assert.False(ok.WasOverridden);

        var no = guard.Check(LicenseClass.Technician, 7_200_000, TransmitMode.Phone, true);
        Assert.False(no.MayTransmit);
        Assert.NotEmpty(no.Reason);
        Assert.NotEmpty(no.Citation);
    }

    /// <remarks>
    /// Proves the override lets a deliberate operator through and records
    /// that it was used. Their license, their call — Hamlet's job is to make
    /// the decision conscious, not to make it for them.
    /// </remarks>
    [Fact]
    public void Guard_OverrideLetsThroughAndSaysSo()
    {
        var decision = new TransmitGuard()
            .Check(LicenseClass.Technician, 7_200_000, TransmitMode.Phone, guardEnabled: false);

        Assert.True(decision.MayTransmit);
        Assert.True(decision.WasOverridden);
        Assert.NotEmpty(decision.Reason);
    }

    /// <remarks>
    /// Proves an unknown class does not block transmitting. Hamlet has no
    /// business refusing to key a radio because a lookup service was down —
    /// it says what it does not know and gets out of the way.
    /// </remarks>
    [Fact]
    public void Guard_DoesNotBlockOnAnUnknownClass()
    {
        var decision = new TransmitGuard()
            .Check(LicenseClass.Unknown, 7_030_000, TransmitMode.Cw, guardEnabled: true);

        Assert.True(decision.MayTransmit);
        Assert.False(decision.WasOverridden);
        Assert.Contains("does not know", decision.Reason, StringComparison.OrdinalIgnoreCase);
    }

    /// <remarks>
    /// Proves the guard governs transmitting only. It is never consulted for
    /// tuning or receiving, and the clearest way to keep that true is that it
    /// takes a mode to transmit and answers one question — there is no
    /// "may I listen here" to call by mistake (HM-DEC-029).
    /// </remarks>
    [Fact]
    public void Guard_HasNoOpinionAboutListening()
    {
        var methods = typeof(TransmitGuard)
            .GetMethods(System.Reflection.BindingFlags.Public
                        | System.Reflection.BindingFlags.Instance
                        | System.Reflection.BindingFlags.DeclaredOnly)
            .Select(m => m.Name)
            .ToList();

        Assert.Equal(new[] { "Check" }, methods);
    }

    /// <remarks>
    /// Proves each rig reports its own capabilities rather than inheriting
    /// the IC-7300's (HM-DEC-030). The training radio has a scope because the
    /// synthesiser is one, and cannot transmit because there is nothing there
    /// to transmit with.
    /// </remarks>
    [Fact]
    public void Rigs_ReportTheirOwnCapabilities()
    {
        var training = new TrainingRig().Capabilities;
        var real = new Ic7300Rig(new Tests.Rig.FakeSerialPort()).Capabilities;

        Assert.True(training.HasSpectrumScope);
        Assert.False(training.CanTransmit);
        Assert.False(training.HasBuiltInCwKeyer);

        Assert.True(real.HasSpectrumScope);
        Assert.True(real.CanTransmit);
        Assert.True(real.HasBuiltInCwKeyer);
        Assert.True(real.HasUsbAudio);

        Assert.True(real.Supports("40 m"));
        Assert.False(real.Supports("2 m"));
    }

    /// <remarks>
    /// Proves the unknown capability set claims nothing. A radio that has not
    /// said what it can do must not inherit features by default — that is the
    /// assumption the whole record exists to remove.
    /// </remarks>
    [Fact]
    public void UnknownCapabilities_ClaimNothing()
    {
        var caps = RigCapabilities.Unknown;

        Assert.False(caps.HasSpectrumScope);
        Assert.False(caps.HasBuiltInCwKeyer);
        Assert.False(caps.HasUsbAudio);
        Assert.False(caps.CanTransmit);
        Assert.Empty(caps.SupportedBandNames);
        Assert.False(caps.Supports("40 m"));
    }

    /// <remarks>
    /// Proves capabilities are reported, not configured — the same shape as
    /// IsSimulated and for the same reason: a radio is the only thing that
    /// knows what it is.
    /// </remarks>
    [Fact]
    public void Capabilities_CannotBeSetOnARig()
    {
        foreach (var type in new[] { typeof(IRig), typeof(TrainingRig), typeof(Ic7300Rig) })
        {
            var property = type.GetProperty("Capabilities");

            Assert.True(property is not null, $"{type.Name} should declare Capabilities");
            Assert.False(property!.CanWrite, $"{type.Name}.Capabilities must not be settable");
        }
    }

    /// <remarks>
    /// Proves the band names a rig claims are the band plan's own, so a
    /// capability check and a band button cannot disagree about what "40 m"
    /// is called.
    /// </remarks>
    [Fact]
    public void Capabilities_UseTheBandPlansNames()
    {
        var caps = new TrainingRig().Capabilities;

        Assert.All(
            BandPlan.Bands,
            b => Assert.True(caps.Supports(b.Name), $"{b.Name} should be supported"));
    }
}
