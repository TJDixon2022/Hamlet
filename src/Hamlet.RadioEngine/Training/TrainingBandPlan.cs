using Hamlet.RadioEngine.Bands;
using Hamlet.RadioEngine.Explore;

namespace Hamlet.RadioEngine.Training;

/// <summary>
/// Decides what is on the air for practice, and where.
/// </summary>
/// <remarks>
/// <para>THE RULE THIS FILE EXISTS FOR (HM-DEC-026): a newcomer practicing on
/// the training radio must be learning the real band, not a convenient
/// fiction. So no frequency is written down here. Every signal is placed
/// inside a neighborhood that <see cref="NeighborhoodPlan"/> already
/// describes, and its mode is the mode that neighborhood is documented to
/// host — CW in the CW segments, FT8 in FT8 city, voice up in the phone
/// segment.</para>
/// <para>Reading the editorial map rather than duplicating its numbers is the
/// point. A second copy of the band plan would drift from the first, and the
/// day it drifted the app would be teaching a band that does not exist while
/// the map beside it said otherwise (§0).</para>
/// </remarks>
public static class TrainingBandPlan
{
    /// <summary>
    /// Which modes a neighborhood hosts, read from the map's own labels.
    /// </summary>
    /// <param name="hood">The neighborhood.</param>
    /// <returns>Modes plausibly heard there; empty for open space.</returns>
    /// <remarks>
    /// <para>Keyed on the map's short label, because those come from the cited
    /// data file and are the identifiers the map itself draws (HM-DEC-054).
    /// Practising on a band whose digital blocks were in the wrong place would
    /// teach the wrong band, which is the one thing this class must not do.</para>
    /// <para>The synthesiser has five voices and the map names more modes than
    /// that, so a block whose mode has no synthesis is placed with the nearest
    /// honest one. FT4 and JS8 sound like FT8 to an ear and on a waterfall,
    /// which is exactly what is being taught here.</para>
    /// </remarks>
    public static IReadOnlyList<TrainingMode> ModesFor(Neighborhood hood)
    {
        ArgumentNullException.ThrowIfNull(hood);

        var label = hood.ShortName.Trim().ToUpperInvariant();
        var name = hood.Name.Trim().ToUpperInvariant();

        // Bursts in a rhythm: the three that draw falling rain.
        if (label is "FT8" or "FT4" or "JS8" || name.Contains("FT8", StringComparison.Ordinal))
        {
            return new[] { TrainingMode.Ft8 };
        }

        if (label == "RTTY" || name.Contains("RTTY", StringComparison.Ordinal))
        {
            return new[] { TrainingMode.Rtty };
        }

        if (label == "PSK31" || name.Contains("PSK31", StringComparison.Ordinal))
        {
            return new[] { TrainingMode.Psk31 };
        }

        if (label is "RTTY+" || name.Contains("DIGITAL", StringComparison.Ordinal))
        {
            return new[] { TrainingMode.Rtty, TrainingMode.Psk31 };
        }

        if (label.StartsWith("SSB", StringComparison.Ordinal)
            || label == "AM"
            || name.Contains("PHONE", StringComparison.Ordinal)
            || name.Contains("RAGCHEW", StringComparison.Ordinal))
        {
            return new[] { TrainingMode.Ssb };
        }

        if (label.StartsWith("CW", StringComparison.Ordinal)
            || label == "QRP"
            || name.Contains("CW", StringComparison.Ordinal))
        {
            return new[] { TrainingMode.Cw };
        }

        // Open ground, the beacon block and the automatic stations: nothing the
        // synthesiser can honestly put there. A band with no empty space on it
        // would be its own kind of lie.
        return Array.Empty<TrainingMode>();
    }

    /// <summary>
    /// True when a neighborhood is the fast end of the CW segment, where
    /// contest and DX operators run at speed.
    /// </summary>
    /// <param name="hood">The neighborhood.</param>
    /// <returns>True for the fast lane.</returns>
    public static bool IsFastCw(Neighborhood hood)
        => hood.ShortName.Trim().Equals("CW DX", StringComparison.OrdinalIgnoreCase)
           || hood.Name.Contains("fast lane", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Build the signal set for a band.
    /// </summary>
    /// <param name="band">The band on screen.</param>
    /// <param name="seed">Seed; the same seed always yields the same band.</param>
    /// <param name="callsign">Callsign the CW stations call as, for flavour.</param>
    /// <returns>Signals placed inside real neighborhoods.</returns>
    public static IReadOnlyList<SyntheticSignal> ForBand(
        CwBand band, int seed, string? callsign = null)
    {
        var signals = new List<SyntheticSignal>();
        var hoods = NeighborhoodPlan.ForBand(band);
        var slot = 0;

        foreach (var hood in hoods)
        {
            var modes = ModesFor(hood);
            if (modes.Count == 0)
            {
                continue;
            }

            foreach (var mode in modes)
            {
                var count = CountFor(mode, hood);

                for (var i = 0; i < count; i++)
                {
                    slot++;
                    var signal = Place(hood, mode, seed, slot, callsign);
                    if (signal is not null)
                    {
                        signals.Add(signal);
                    }
                }
            }
        }

        return signals;
    }

    /// <summary>
    /// How many of a mode to put in one neighborhood.
    /// </summary>
    /// <remarks>
    /// FT8 city gets a crowd because that is the truthful picture — it is the
    /// busiest three kilohertz in radio and the reason the waterfall looks
    /// like rain. The quieter modes get one or two, so a newcomer can pick
    /// out a single signal and study it.
    /// </remarks>
    private static int CountFor(TrainingMode mode, Neighborhood hood)
    {
        var widthHz = hood.HighHz - hood.LowHz;

        return mode switch
        {
            TrainingMode.Ft8 => 7,
            TrainingMode.Cw => widthHz > 30_000 ? 4 : 3,
            TrainingMode.Ssb => widthHz > 60_000 ? 3 : 2,
            TrainingMode.Rtty => 1,
            TrainingMode.Psk31 => 2,
            _ => 1,
        };
    }

    private static SyntheticSignal? Place(
        Neighborhood hood, TrainingMode mode, int seed, int slot, string? callsign)
    {
        var probe = new SyntheticSignal(mode, hood.LowHz, 1.0);
        var margin = probe.WidthHz;
        var low = hood.LowHz + margin;
        var high = hood.HighHz - margin;

        if (high <= low)
        {
            return null;
        }

        // Deterministic placement across the neighborhood's own span, then
        // snapped to 100 Hz so the numbers read like real spots.
        var position = Hash01(seed, slot, (int)(hood.LowHz / 1000));
        var hz = low + (long)(position * (high - low));
        hz = hz / 100 * 100;

        var strengthRoll = Hash01(seed, slot * 7, 11);
        var strength = 0.35 + (0.55 * strengthRoll);

        var wpm = mode == TrainingMode.Cw
            ? (IsFastCw(hood)
                ? 25 + (int)(Hash01(seed, slot * 13, 29) * 10)
                : 10 + (int)(Hash01(seed, slot * 13, 29) * 10))
            : 18;

        var fadeSeconds = 18.0 + (Hash01(seed, slot * 3, 71) * 40.0);

        return new SyntheticSignal(
            mode,
            hz,
            strength,
            wpm,
            MorseCode.CqCall(CallFor(seed, slot, callsign)),
            Hash01(seed, slot * 5, 53),
            TimeSpan.FromSeconds(fadeSeconds));
    }

    /// <summary>
    /// A plausible callsign for a synthetic station.
    /// </summary>
    /// <remarks>
    /// Never the operator's own — hearing your own callsign called on a
    /// training band would be a genuinely confusing thing to teach. The
    /// operator's callsign is used only to keep the set stable per operator.
    /// </remarks>
    private static string CallFor(int seed, int slot, string? callsign)
    {
        var prefixes = new[] { "W", "K", "N", "AA", "KC", "WB" };
        var suffixes = new[]
        {
            "ABC", "QRP", "XYZ", "DXR", "MNO", "TUV", "JKL", "PQR", "GHI", "STU",
        };

        var p = prefixes[(int)(Hash01(seed, slot, 101) * prefixes.Length) % prefixes.Length];
        var d = (int)(Hash01(seed, slot, 103) * 10) % 10;
        var s = suffixes[(int)(Hash01(seed, slot, 107) * suffixes.Length) % suffixes.Length];

        var call = $"{p}{d}{s}";

        // Vanishingly unlikely, but a training band that called the operator
        // would be teaching a lie about who is on the air.
        return string.Equals(call, callsign?.Trim(), StringComparison.OrdinalIgnoreCase)
            ? call + "X"
            : call;
    }

    private static double Hash01(int seed, int a, int b)
    {
        unchecked
        {
            var h = (uint)seed;
            h = (h ^ (uint)a) * 2654435761u;
            h ^= h >> 15;
            h = (h ^ (uint)b) * 2246822519u;
            h ^= h >> 13;
            h *= 3266489917u;
            h ^= h >> 16;
            return h / 4294967296.0;
        }
    }
}
