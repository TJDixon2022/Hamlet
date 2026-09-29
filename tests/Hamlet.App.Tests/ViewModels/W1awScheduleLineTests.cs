using System;
using Hamlet.App.Settings;
using Hamlet.App.ViewModels;
using Hamlet.RadioEngine.Bands;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.ViewModels;

/// <summary>
/// The line and the dot beside the W1AW button: what W1AW's schedule has on now, or next, in the
/// operator's own clock (work instruction 497).
/// </summary>
/// <remarks>
/// **NO TIME HERE IS COMPUTED WITH A FIXED OFFSET.** Each moment is made in named US Central and
/// converted to UTC through the time zone database, and read back in the operator's named zone.
/// The schedule is the one the build carries, from <c>data/bands/w1aw-morse.json</c>.
/// </remarks>
public sealed class W1awScheduleLineTests
{
    private static readonly TimeZoneInfo Central = W1awMorseFrequencies.FindZone("America/Chicago");

    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the line is printed.</param>
    public W1awScheduleLineTests(ITestOutputHelper output) => _output = output;

    private static DateTime CentralToUtc(int year, int month, int day, int hour, int minute)
        => TimeZoneInfo.ConvertTimeToUtc(new DateTime(year, month, day, hour, minute, 0, DateTimeKind.Unspecified), Central);

    private MainWindowViewModel At(DateTime utc, TimeZoneInfo zone)
    {
        var model = new MainWindowViewModel(new AppSettings { ReconnectOnStartup = false }, null);

        model.UpdateW1awSchedule(utc, zone);
        _output.WriteLine($"{utc:u} in {zone.Id}: `{model.W1awScheduleLine}`, dot {(model.W1awScheduledNow ? "filled" : "hollow")} `{model.W1awScheduleWord}`");

        return model;
    }

    /// <remarks>
    /// Inside a bulletin - Tuesday 6 October 2026, 7:30 PM Central - the line says it is sending now,
    /// the kind, the speed and the end in the operator's clock, here Eastern, and the dot is filled
    /// and says scheduled.
    /// </remarks>
    [Fact]
    public void InsideABulletin()
    {
        var eastern = W1awMorseFrequencies.FindZone("America/New_York");
        var utc = CentralToUtc(2026, 10, 6, 19, 30);
        var end = TimeZoneInfo.ConvertTimeFromUtc(CentralToUtc(2026, 10, 6, 20, 0), eastern);
        var model = At(utc, eastern);

        Assert.Equal($"Sending now: code bulletin, 18 WPM, until {end:h:mm tt}", model.W1awScheduleLine);
        Assert.True(model.W1awScheduledNow);
        Assert.Equal("scheduled", model.W1awScheduleWord);
    }

    /// <remarks>
    /// At a quiet time - Wednesday 7 October 2026, 8:38 PM Central - the line names the next run and
    /// the minutes to it, and the dot is hollow and says quiet.
    /// </remarks>
    [Fact]
    public void AtAQuietTime()
    {
        var model = At(CentralToUtc(2026, 10, 7, 20, 38), Central);

        Assert.Equal("Next: code practice, fast, 9:00 PM - in 22 minutes", model.W1awScheduleLine);
        Assert.False(model.W1awScheduledNow);
        Assert.Equal("quiet", model.W1awScheduleWord);
    }

    /// <remarks>
    /// Past the last run of a day - Tuesday 6 October 2026, 11:30 PM Central - the line names
    /// tomorrow's first run.
    /// </remarks>
    [Fact]
    public void PastTheLastRunOfTheDay()
    {
        var model = At(CentralToUtc(2026, 10, 6, 23, 30), Central);

        Assert.Equal("Next: code practice, slow, tomorrow 8:00 AM", model.W1awScheduleLine);
        Assert.False(model.W1awScheduledNow);
    }

    /// <remarks>
    /// The same 7 PM Central bulletin starts at a different hour of UTC in October and in December,
    /// because Central keeps daylight saving: a baked-in offset would be an hour wrong in one of them.
    /// </remarks>
    [Fact]
    public void DaylightSavingMovesTheRunInUtc()
    {
        var october = W1awMorseFrequencies.Default.At(CentralToUtc(2026, 10, 6, 19, 30));
        var december = W1awMorseFrequencies.Default.At(CentralToUtc(2026, 12, 1, 19, 30));

        _output.WriteLine($"7 PM Central bulletin starts {october.StartUtc:HH:mm} UTC in October, {december.StartUtc:HH:mm} UTC in December");

        Assert.True(october.Scheduled && december.Scheduled);
        Assert.Equal(1, december.StartUtc.Hour - october.StartUtc.Hour);
    }
}
