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
    /// <summary>
    /// A real callook.info response for KC3QIS, captured from the live service
    /// on 2026-08-13 with only the name and street address replaced.
    /// </summary>
    /// <remarks>
    /// The coordinates and the <c>gridsquare</c> field are verbatim, which is
    /// what makes them worth keeping: callook's own answer is "FN00dj", so
    /// Hamlet's derivation of the locator from the coordinates has something
    /// independent to agree with (HM-DEC-037).
    /// </remarks>
    private const string ValidJson = """
    {
      "status": "VALID",
      "type": "PERSON",
      "current": { "callsign": "KC3QIS", "operClass": "GENERAL" },
      "name": "A NAME THE PARSER MUST IGNORE",
      "address": { "line1": "A STREET ADDRESS", "line2": "A TOWN" },
      "location": {
        "latitude": "40.3782746",
        "longitude": "-79.7081649",
        "gridsquare": "FN00dj"
      },
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
    /// <para>Proves the parser takes the class and the coordinates and leaves
    /// the rest. The response carries the licensee's name and street address;
    /// Hamlet has no use for them and does not read them, so there is nothing
    /// to leak later.</para>
    /// <para>The coordinates were added deliberately for the grid square and
    /// the solar clock (HM-DEC-037), so the list below grew by exactly one
    /// name. That is the point of pinning it: widening this type is a decision
    /// somebody has to make on purpose, and a test that had to be edited is a
    /// decision that was made.</para>
    /// </remarks>
    [Fact]
    public async Task Lookup_ReadsNothingButTheClassAndTheCoordinates()
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
            new[] { "Callsign", "Class", "SourceName", "RetrievedUtc", "Location" }
                .OrderBy(x => x),
            properties.OrderBy(x => x));

        Assert.NotNull(result);
        Assert.DoesNotContain(
            properties, p => p.Contains("Name", StringComparison.OrdinalIgnoreCase)
                             && p != "SourceName");
        Assert.DoesNotContain(
            properties, p => p.Contains("Address", StringComparison.OrdinalIgnoreCase));
    }

    /// <remarks>
    /// Proves the coordinates are read, and that the locator Hamlet derives
    /// from them matches the one callook computed independently — the two
    /// arrive by different routes and agree, which is the check that would
    /// catch a hemisphere sign error in <c>ToGrid</c> (HM-DEC-037).
    /// </remarks>
    [Fact]
    public async Task Lookup_ReadsCoordinatesThatAgreeWithTheServicesOwnGrid()
    {
        using var handler = new StubHttp(ValidJson);
        using var lookup = new CallookCallsignLookup("0.1", "KC3QIS", handler, () => Now);

        var result = await lookup.LookupAsync("KC3QIS");

        Assert.NotNull(result!.Location);
        Assert.Equal(40.3782746, result.Location!.Value.Latitude, 6);
        Assert.Equal(-79.7081649, result.Location.Value.Longitude, 6);

        // callook's own answer for this callsign is "FN00dj".
        Assert.Equal(
            "FN00DJ", RadioEngine.Explore.OperatorLocation.ToGrid(result.Location.Value));
    }

    /// <remarks>
    /// Proves a response with no location block, a half-filled one, or a pair
    /// of zeros yields no coordinates at all. Null Island is in the Gulf of
    /// Guinea, and a profile quietly placed there would put every band card and
    /// every distance wrong while looking entirely confident (HM-DEC-009).
    /// </remarks>
    [Theory]
    [InlineData("""{ "status": "VALID", "current": { "callsign": "K1AA", "operClass": "EXTRA" } }""")]
    [InlineData("""{ "status": "VALID", "current": { "callsign": "K1AA", "operClass": "EXTRA" }, "location": {} }""")]
    [InlineData("""{ "status": "VALID", "current": { "callsign": "K1AA", "operClass": "EXTRA" }, "location": { "latitude": "41.2" } }""")]
    [InlineData("""{ "status": "VALID", "current": { "callsign": "K1AA", "operClass": "EXTRA" }, "location": { "latitude": "0", "longitude": "0" } }""")]
    [InlineData("""{ "status": "VALID", "current": { "callsign": "K1AA", "operClass": "EXTRA" }, "location": { "latitude": "", "longitude": "" } }""")]
    [InlineData("""{ "status": "VALID", "current": { "callsign": "K1AA", "operClass": "EXTRA" }, "location": { "latitude": "999", "longitude": "12" } }""")]
    public async Task Lookup_WithoutUsableCoordinates_ReportsNone(string json)
    {
        using var handler = new StubHttp(json);
        using var lookup = new CallookCallsignLookup("0.1", "K1AA", handler, () => Now);

        var result = await lookup.LookupAsync("K1AA");

        Assert.NotNull(result);
        Assert.Null(result!.Location);
        Assert.Equal(LicenseClass.Extra, result.Class);
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
            HfBands.Bands,
            b => Assert.True(caps.Supports(b.Name), $"{b.Name} should be supported"));
    }
}
