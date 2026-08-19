using System.Text.Json;
using Hamlet.App.Settings;
using Hamlet.App.Telemetry;
using Hamlet.RadioEngine.Rig;
using Hamlet.RadioEngine.Telemetry;
using Xunit;

namespace Hamlet.App.Tests.Telemetry;

/// <summary>
/// `scope_output_requested` does not contradict itself.
/// </summary>
/// <remarks>
/// <para>**IT LOGGED `outcome: failed` BESIDE `reason: confirmed`, WITH
/// `unansweredCommands: 0`, WHILE 2,748 SCOPE FRAMES WERE ARRIVING.** Two fields
/// in one event saying opposite things about a write that plainly worked. The
/// cause was mine: the caller was moved to the stable token
/// (`RigWriteResult.Reason`) and this comparison was left on the enum's name, so
/// "confirmed" never matched "Confirmed" and every outcome fell through to
/// failed.</para>
/// <para>**A STABLE TOKEN EXISTS SO COMPARISONS SURVIVE REWORDING** (HM-DEC-077),
/// and it only works if the comparison uses it. Proved here rather than assumed,
/// which is what the work order asked for: the repair is a one-word change and
/// nothing was checking it.</para>
/// </remarks>
public sealed class ScopeRequestReportsTruthfullyTests : IDisposable
{
    private readonly string _folder = Path.Combine(
        Path.GetTempPath(), "hamlet-tests", Guid.NewGuid().ToString("N"));

    public void Dispose()
    {
        try
        {
            if (Directory.Exists(_folder))
            {
                Directory.Delete(_folder, recursive: true);
            }
        }
        catch (IOException)
        {
            // A leftover temp folder is not a test failure.
        }
    }

    private IReadOnlyList<JsonElement> Written(string reason)
    {
        var settings = new AppSettings();

        using (var telemetry = new JsonlTelemetry(
                   _folder, "test", settings.IsTelemetryEnabled, 50L * 1024 * 1024))
        {
            AppEvents.ScopeOutputRequested(telemetry, reason, 115_200, unanswered: 0);
        }

        return Directory.GetFiles(_folder, "*.jsonl")
            .SelectMany(File.ReadAllLines)
            .Where(l => l.Contains("scope_output_requested", StringComparison.Ordinal))
            .Select(l => JsonDocument.Parse(l).RootElement.Clone())
            .ToList();
    }

    private static string Field(JsonElement e, string name)
    {
        if (e.TryGetProperty(name, out var direct))
        {
            return direct.ToString();
        }

        foreach (var p in e.EnumerateObject())
        {
            if (p.Value.ValueKind == JsonValueKind.Object
                && p.Value.TryGetProperty(name, out var nested))
            {
                return nested.ToString();
            }
        }

        return "";
    }

    /// <remarks>
    /// Proves the repair: the token a confirmed write actually carries is the one
    /// that produces `proceeded`. This is the exact pair that contradicted itself.
    /// </remarks>
    [Fact]
    public void AConfirmedWriteIsReportedAsProceeded()
    {
        var confirmed = RigWriteResult.Confirmed("27 11").Reason;

        var line = Assert.Single(Written(confirmed));

        Assert.Equal("proceeded", Field(line, "outcome"));
        Assert.Equal(confirmed, Field(line, "reason"));
    }

    /// <remarks>
    /// Proves the other side stayed honest: silence is a failure and says which
    /// kind, so the ladder's first rung is still readable (HM-OPEN-042).
    /// </remarks>
    [Fact]
    public void SilenceIsReportedAsFailedAndSaysWhy()
    {
        var line = Assert.Single(Written(RigWriteResult.NoAnswer("27 11").Reason));

        Assert.Equal("failed", Field(line, "outcome"));
        Assert.Equal("no_answer", Field(line, "reason"));
    }

    /// <remarks>
    /// **THE ONE THAT WOULD HAVE CAUGHT IT.** Every outcome a write can have is
    /// swept, and the two fields are never allowed to disagree: `proceeded` only
    /// with the confirmed token, `failed` only without it. A future rename of the
    /// enum cannot bring the contradiction back without failing here.
    /// </remarks>
    [Fact]
    public void OutcomeAndReasonNeverContradictEachOther()
    {
        foreach (var outcome in Enum.GetValues<RigWriteOutcome>())
        {
            var reason = new RigWriteResult(outcome, "", "27 11").Reason;
            var line = Assert.Single(Written(reason));

            var reported = Field(line, "outcome");

            Assert.Equal(reason, Field(line, "reason"));
            Assert.Equal(
                outcome == RigWriteOutcome.Confirmed ? "proceeded" : "failed",
                reported);

            Directory.Delete(_folder, recursive: true);
        }
    }
}
