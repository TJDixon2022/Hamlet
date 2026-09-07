using Hamlet.RadioEngine.Audio;

namespace Hamlet.RadioEngine.Tests.Audio;

/// <summary>
/// Which render endpoint the tests that make sound will make it on.
/// </summary>
/// <remarks>
/// <para>**CHOSEN, NOT DEFAULTED TO** (work instruction 256, task 1). These tests
/// play 12.64 seconds of FT8 tones out of a real computer that somebody owns and
/// may be asleep near, so the endpoint is picked deliberately and named in the
/// output every time.</para>
/// <para>**A DISPLAY-AUDIO ENDPOINT IS PREFERRED** because it is a monitor's audio
/// path rather than the machine's speakers, and because it is not the default -
/// which means a sink that quietly fell back to the default would show up as a
/// different name in the report rather than as a passing test.</para>
/// <para>**IT DOES NOT DECIDE WHETHER A TEST PASSES.** It reports what it chose
/// and why; a machine with no render endpoint at all returns null and the caller
/// says so (SHACK_FACTS.md FACT-004).</para>
/// </remarks>
internal static class RenderChoice
{
    /// <summary>The endpoint these tests will play to, and why it was chosen.</summary>
    /// <param name="why">In words, for the report.</param>
    /// <returns>The endpoint, or null where the machine has none.</returns>
    public static RenderEndpoint? Preferred(out string why)
    {
        var endpoints = WasapiTransmitSink.Endpoints();

        if (endpoints.Count == 0)
        {
            why = "this machine has no active render endpoint at all";

            return null;
        }

        var quiet = endpoints.FirstOrDefault(
            e => e.Name.Contains("Display Audio", StringComparison.OrdinalIgnoreCase));

        if (quiet is not null)
        {
            why = "a display-audio endpoint - a monitor's audio path rather than the "
                + $"machine's speakers - chosen from {endpoints.Count} active render endpoints, "
                + $"and it is {(quiet.IsDefault ? "also" : "not")} the default";

            return quiet;
        }

        var chosen = endpoints.FirstOrDefault(e => !e.IsDefault) ?? endpoints[0];

        why = "no display-audio endpoint on this machine, so "
            + $"{(chosen.IsDefault ? "the default" : "the first endpoint that is not the default")} "
            + $"was taken from {endpoints.Count} active render endpoints - THIS MAY BE AUDIBLE";

        return chosen;
    }

    /// <summary>A tone at a given amplitude, for tests that need real audio.</summary>
    /// <param name="seconds">How long.</param>
    /// <param name="sampleRate">At what rate.</param>
    /// <param name="hertz">At what pitch.</param>
    /// <param name="amplitude">How loud, as a fraction of full scale.</param>
    /// <returns>The samples.</returns>
    public static float[] Tone(double seconds, int sampleRate, double hertz, double amplitude)
    {
        var samples = new float[(int)Math.Round(seconds * sampleRate)];

        for (var i = 0; i < samples.Length; i++)
        {
            samples[i] = (float)(amplitude * Math.Sin(2.0 * Math.PI * hertz * i / sampleRate));
        }

        return samples;
    }
}
