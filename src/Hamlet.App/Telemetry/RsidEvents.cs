using Hamlet.RadioEngine.Olivia;
using Hamlet.RadioEngine.Rsid;
using Hamlet.RadioEngine.Telemetry;

namespace Hamlet.App.Telemetry;

/// <summary>
/// **What Hamlet writes when it hears an RSID burst and when it sends one.**
/// </summary>
/// <remarks>
/// <para>**`PHASE_PLAN.md` CRITERION 1.6, AND PSK31 PLAN §R13.** Every stage a step adds writes
/// an event a person can diagnose that stage from: a burst heard says which code, where and how
/// well; a burst sent says which code and where.</para>
/// <para>**NOTHING PERSONAL, AND THERE IS NOTHING PERSONAL TO HAVE** (HM-DEC-018, §2.1). An RSID
/// burst carries a mode code and nothing else. **No callsign, no text** - the tests scan the
/// written lines for both.</para>
/// <para>**AND NOTHING HERE ACTS ON WHAT IT WRITES** (§0.2, the arbiter's decision B). A burst
/// heard changes no mode, variant, tab or dial and sends nothing.</para>
/// </remarks>
public static class RsidEvents
{
    /// <summary>A burst was heard.</summary>
    /// <param name="telemetry">Sink, or null.</param>
    /// <param name="heard">What the detector read.</param>
    public static void Heard(ITelemetry? telemetry, RsidDetection heard)
    {
        ArgumentNullException.ThrowIfNull(heard);

        telemetry?.Write(
            TelemetryCategory.Decode,
            "rsid_heard",
            new Dictionary<string, object?>(StringComparer.Ordinal)
            {
                ["code"] = heard.Code,
                ["mode"] = heard.Mode,
                ["variant"] = heard.Variant,
                ["centerHz"] = Math.Round(heard.CenterHz, 1),
                ["quality"] = Math.Round(heard.Quality, 3),
                ["tonesRight"] = heard.TonesRight,
            });
    }

    /// <summary>A send went out beginning with its burst.</summary>
    /// <param name="telemetry">Sink, or null.</param>
    /// <param name="code">The code the burst names.</param>
    /// <param name="centerHz">Where the burst was centered.</param>
    /// <remarks>
    /// **THE MODE AND VARIANT ARE READ BACK FROM THE FILE BY THE CODE**, the same way a burst
    /// heard is named, so a line sent and a line heard for the same burst say the same words.
    /// </remarks>
    public static void Sent(ITelemetry? telemetry, int code, double centerHz)
    {
        var name = OliviaData.Current.Rsid is { } codes ? RsidBurst.NameFor(codes, code) ?? "" : "";

        telemetry?.Write(
            TelemetryCategory.Transmit,
            "rsid_sent",
            new Dictionary<string, object?>(StringComparer.Ordinal)
            {
                ["code"] = code,
                ["mode"] = RsidCodes.ModeOf(name),
                ["variant"] = RsidCodes.VariantOf(name),
                ["centerHz"] = Math.Round(centerHz, 1),
            });
    }
}
