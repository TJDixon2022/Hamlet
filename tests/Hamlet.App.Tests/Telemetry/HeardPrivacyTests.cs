using System.Reflection;
using Hamlet.App.Telemetry;
using Hamlet.RadioEngine.Explore;
using Xunit;

namespace Hamlet.App.Tests.Telemetry;

/// <summary>
/// "Did anybody hear me" cannot put a callsign into telemetry (HM-DEC-075,
/// HM-DEC-018).
/// </summary>
/// <remarks>
/// <para>THIS FEATURE IS BUILT ENTIRELY OUT OF CALLSIGNS: the operator's own,
/// which is how a report is recognized as his, and the receivers', which is who
/// heard him. HM-DEC-018's rule is that the profile and the telemetry payload
/// builder stay unable to see each other, and a feature this shaped is exactly
/// where that rule would be broken by accident.</para>
/// <para>So the proof is structural rather than a sweep of strings.
/// <see cref="AppEvents"/> cannot accept anything from this feature, which means
/// no future event can carry one without the type system objecting first.</para>
/// </remarks>
public sealed class HeardPrivacyTests
{
    /// <remarks>
    /// Proves HM-DEC-075 and HM-DEC-018: no telemetry event can be handed a
    /// report or a summary, so a receiver's callsign has no route into the
    /// record even if somebody later adds an event for this feature.
    /// </remarks>
    [Fact]
    public void NoTelemetryEventCanBeHandedAHeardReport()
    {
        var forbidden = new[]
        {
            typeof(HeardReport), typeof(HeardSummary), typeof(HeardState),
        };

        var methods = typeof(AppEvents)
            .GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly);

        Assert.NotEmpty(methods);

        foreach (var method in methods)
        {
            foreach (var parameter in method.GetParameters())
            {
                Assert.DoesNotContain(parameter.ParameterType, forbidden);
            }
        }
    }

    /// <remarks>
    /// Proves HM-DEC-018 still holds where this feature lives: the payload
    /// builder cannot reach the operator profile, so the callsign that makes a
    /// report "his" cannot travel with anything it writes.
    /// </remarks>
    [Fact]
    public void TheTelemetryBuilderStillCannotSeeTheProfile()
    {
        var reachable = typeof(AppEvents)
            .GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .SelectMany(m => m.GetParameters())
            .Select(p => p.ParameterType)
            .Distinct()
            .ToList();

        Assert.DoesNotContain(typeof(Hamlet.App.Settings.OperatorProfile), reachable);
        Assert.DoesNotContain(typeof(Hamlet.App.Settings.AppSettings), reachable);
    }
}
