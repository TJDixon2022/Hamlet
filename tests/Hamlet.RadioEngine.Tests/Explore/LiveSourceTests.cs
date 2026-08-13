using System.Net.Http;
using Hamlet.RadioEngine.Explore;
using Xunit;

namespace Hamlet.RadioEngine.Tests.Explore;

/// <summary>
/// The live activity sources (HM-DEC-024), driven from captured responses.
/// </summary>
/// <remarks>
/// The POTA and SOTA payloads below are real records taken from those services
/// on 2026-08-13, trimmed to the fields Hamlet reads. Using a capture rather
/// than the live endpoint is what makes these tests mean something: they fail
/// when Hamlet's parsing breaks, and not when somebody's server is down (§5).
/// </remarks>
public sealed class LiveSourceTests
{
    private const string PotaJson = """
    [
      {"spotId":55089238,"activator":"VE3CMI","frequency":"14247.0","mode":"SSB",
       "reference":"CA-5599","parkName":null,"spotTime":"2026-08-13T14:54:07",
       "spotter":"K4OTT","comments":"55 in EM74xw 73s K4OTT","source":"POTACAT",
       "invalid":null,"name":"Conestogo Lake Conservation Area",
       "locationDesc":"CA-ON","grid4":"EN93","latitude":43.6699,"longitude":-80.7206},
      {"spotId":55089239,"activator":"K3ABC","frequency":"7032.0","mode":"CW",
       "reference":"US-0001","spotTime":"2026-08-13T14:55:00","spotter":"W3XYZ",
       "comments":"calling at 13wpm","invalid":null,"name":"Ohiopyle State Park",
       "locationDesc":"US-PA","grid4":"FN00"},
      {"spotId":55089240,"activator":"W9GONE","frequency":"7040.0","mode":"CW",
       "reference":"US-0002","spotTime":"2026-08-13T14:50:00","spotter":"W9AAA",
       "comments":"QRT thanks for the contacts","invalid":null,
       "name":"Somewhere","locationDesc":"US-IL"},
      {"spotId":55089241,"activator":"N0BAD","frequency":"7045.0","mode":"CW",
       "reference":"US-0003","spotTime":"2026-08-13T14:52:00","spotter":"N0AAA",
       "comments":"","invalid":"flagged","name":"Bad Spot","locationDesc":"US-MO"}
    ]
    """;

    private const string SotaJson = """
    [
      {"id":9999999999999999,"userID":null,"timeStamp":"2026-08-13T15:12:07",
       "comments":"This API endpoint is deprecated and will be removed before August 31, 2026.",
       "callsign":"DEPRECATED","associationCode":"DEPRECATED","summitCode":"DEPRECATED",
       "activatorCallsign":"DEPRECATED","activatorName":null,"frequency":"","mode":"",
       "summitDetails":"DEPRECATED","highlightColor":null},
      {"id":369724,"userID":100611,"timeStamp":"2026-08-13T15:12:15","comments":null,
       "callsign":"AF5TT","associationCode":"W5N","summitCode":"SI-010",
       "activatorCallsign":"AF5TT","activatorName":"Thomas","frequency":"14.059",
       "mode":"CW","summitDetails":"Palomas Peak, 2647m, 8 points","highlightColor":null},
      {"id":369723,"userID":7310,"timeStamp":"2026-08-13T15:11:06","comments":"",
       "callsign":"AC1Z","associationCode":"W1","summitCode":"HA-052",
       "activatorCallsign":"AC1Z","activatorName":"Bob","frequency":"10.11",
       "mode":"CW","summitDetails":"Stewarts Peak, 564m, 1 points","highlightColor":null}
    ]
    """;

    private static readonly DateTime Now = new(2026, 8, 13, 15, 0, 0, DateTimeKind.Utc);

    private static ActivityContext Pittsburgh => new()
    {
        BandLowHz = 7_000_000,
        BandHighHz = 7_300_000,
        HomeDistrict = 3,
        HomeInNorthAmerica = true,
    };

    /// <remarks>
    /// Proves HM-DEC-024: POTA's real field names and units are read correctly
    /// — frequency arrives in kilohertz as a string, and a park activation
    /// becomes a spot that knows it is one.
    /// </remarks>
    [Fact]
    public async Task Pota_ReadsRealFields()
    {
        using var handler = new StubHttp(PotaJson);
        using var source = new PotaActivitySource("0.1", "KC3QIS", handler, () => Now);
        source.SetContext(Pittsburgh);

        var spots = await source.GetSpotsAsync();
        var ohiopyle = spots.Single(s => s.DxCall == "K3ABC");

        Assert.Equal(7_032_000, ohiopyle.FrequencyHz);
        Assert.Equal("CW", ohiopyle.Mode);
        Assert.True(ohiopyle.IsActivation);
        Assert.Equal(SpotCallType.Cq, ohiopyle.CallType);
        Assert.Equal("US-0001", ohiopyle.Reference);
        Assert.Equal("US-PA", ohiopyle.PlaceLabel);
        Assert.Equal(13, ohiopyle.Wpm);
        Assert.Contains("Ohiopyle State Park", ohiopyle.Story, StringComparison.Ordinal);
    }

    /// <remarks>
    /// Proves the prime directive at the source: a station that has announced
    /// QRT has packed up, and a spot POTA has flagged invalid is not a spot.
    /// Sending a newcomer to an empty frequency is the failure this whole
    /// feature exists to prevent (HM-DEC-009).
    /// </remarks>
    [Fact]
    public async Task Pota_DropsQrtAndInvalidSpots()
    {
        using var handler = new StubHttp(PotaJson);
        using var source = new PotaActivitySource("0.1", "KC3QIS", handler, () => Now);
        source.SetContext(Pittsburgh);

        var spots = await source.GetSpotsAsync();

        Assert.DoesNotContain(spots, s => s.DxCall == "W9GONE");
        Assert.DoesNotContain(spots, s => s.DxCall == "N0BAD");
    }

    /// <remarks>
    /// Proves POTA spots are kept for every band, not just the one on screen —
    /// the whole-spectrum view the band-conditions line needs to be able to
    /// say "nothing here, try 40 m" (HM-DEC-025).
    /// </remarks>
    [Fact]
    public async Task Pota_KeepsOtherBandsForTheConditionsLine()
    {
        using var handler = new StubHttp(PotaJson);
        using var source = new PotaActivitySource("0.1", "KC3QIS", handler, () => Now);
        source.SetContext(Pittsburgh);

        var spots = await source.GetSpotsAsync();

        Assert.Contains(spots, s => s.FrequencyHz == 14_247_000);
    }

    /// <remarks>
    /// Proves the politeness rule of HM-DEC-024: every request names the app,
    /// its version, the project URL and the operator, so a service admin can
    /// find out who is calling them.
    /// </remarks>
    [Fact]
    public async Task Pota_IntroducesItself()
    {
        using var handler = new StubHttp(PotaJson);
        using var source = new PotaActivitySource("0.1", "KC3QIS", handler, () => Now);
        source.SetContext(Pittsburgh);

        await source.GetSpotsAsync();

        var agent = handler.Requests.Single().Headers.UserAgent.ToString();
        Assert.Contains("Hamlet/0.1", agent, StringComparison.Ordinal);
        Assert.Contains("github.com/TJDixon2022/Hamlet", agent, StringComparison.Ordinal);
        Assert.Contains("KC3QIS", agent, StringComparison.Ordinal);
    }

    /// <remarks>
    /// Proves the self-imposed rate floor of HM-DEC-024: asking again inside
    /// the minimum interval is answered from cache and costs the service
    /// nothing. POTA publishes no rate card, so Hamlet supplies its own.
    /// </remarks>
    [Fact]
    public async Task Pota_DoesNotPollFasterThanItsFloor()
    {
        using var handler = new StubHttp(PotaJson);
        using var source = new PotaActivitySource("0.1", "KC3QIS", handler, () => Now);
        source.SetContext(Pittsburgh);

        await source.GetSpotsAsync();
        await source.GetSpotsAsync();
        await source.GetSpotsAsync();

        Assert.Single(handler.Requests);
    }

    /// <remarks>
    /// Proves the User-Agent degrades honestly: with no callsign set it names
    /// the app and its URL and simply omits the operator, rather than sending
    /// a placeholder that would misidentify whoever is calling.
    /// </remarks>
    [Fact]
    public void UserAgent_OmitsAnAbsentCallsign()
    {
        var withCall = HamletIdentity.UserAgent("0.1", "kc3qis");
        var without = HamletIdentity.UserAgent("0.1", "  ");

        Assert.Equal("Hamlet/0.1 (+https://github.com/TJDixon2022/Hamlet; KC3QIS)", withCall);
        Assert.Equal("Hamlet/0.1 (+https://github.com/TJDixon2022/Hamlet)", without);
    }

    /// <remarks>
    /// Proves SOTA's frequencies are read as megahertz — the unit differs from
    /// POTA's kilohertz, and getting it wrong would put every summit spot a
    /// thousand times off frequency.
    /// </remarks>
    [Fact]
    public async Task Sota_ReadsMegahertzAndSummitNames()
    {
        using var handler = new StubHttp(SotaJson);
        using var source = new SotaActivitySource("0.1", "KC3QIS", handler, () => Now);
        source.SetContext(Pittsburgh);

        var spots = await source.GetSpotsAsync();
        var palomas = spots.Single(s => s.DxCall == "AF5TT");

        Assert.Equal(14_059_000, palomas.FrequencyHz);
        Assert.Equal("W5N/SI-010", palomas.Reference);
        Assert.True(palomas.IsActivation);
        Assert.Contains("Palomas Peak", palomas.Story, StringComparison.Ordinal);
        Assert.Contains("Thomas", palomas.Story, StringComparison.Ordinal);
    }

    /// <remarks>
    /// Proves the deprecation sentinel never becomes a spot. SOTA's endpoint
    /// answers with a record whose callsign is the literal "DEPRECATED",
    /// carrying an API notice in the comments field; rendering that as a
    /// station on the air would be the prime directive broken by a parser
    /// (HM-DEC-009).
    /// </remarks>
    [Fact]
    public async Task Sota_DropsTheDeprecationSentinel()
    {
        using var handler = new StubHttp(SotaJson);
        using var source = new SotaActivitySource("0.1", "KC3QIS", handler, () => Now);
        source.SetContext(Pittsburgh);

        var spots = await source.GetSpotsAsync();

        Assert.DoesNotContain(spots, s => s.DxCall == "DEPRECATED");
        Assert.DoesNotContain(
            spots, s => s.Story.Contains("deprecated", StringComparison.OrdinalIgnoreCase));
        Assert.Equal(2, spots.Count);
    }

    /// <remarks>
    /// Proves SOTA ships switched off, and that the switch is a licence
    /// decision rather than a technical one: the API's terms require the
    /// developer to be registered with the SOTA Reflector's API-consumers
    /// group and to have had AI-written software approved. Hamlet does not
    /// enter into that on Tim's behalf (HM-DEC-024).
    /// </remarks>
    [Fact]
    public void Sota_ShipsDisabledWithAStatedReason()
    {
        Assert.NotEmpty(SotaActivitySource.DisabledReason);
        Assert.Contains(
            "API-consumers", SotaActivitySource.DisabledReason, StringComparison.Ordinal);
    }

    /// <remarks>
    /// Proves a source that throws does not take the refresh down with it —
    /// the aggregate is where that is handled, and the source is free to fail
    /// loudly.
    /// </remarks>
    [Fact]
    public async Task Pota_PropagatesTransportFailure()
    {
        using var handler = new StubHttp("nope", System.Net.HttpStatusCode.ServiceUnavailable);
        using var source = new PotaActivitySource("0.1", "KC3QIS", handler, () => Now);
        source.SetContext(Pittsburgh);

        await Assert.ThrowsAnyAsync<HttpRequestException>(() => source.GetSpotsAsync());
    }
}
