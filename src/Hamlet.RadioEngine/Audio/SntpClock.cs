using System.Net.Sockets;

namespace Hamlet.RadioEngine.Audio;

/// <summary>
/// One SNTP query, to find out how far the PC clock is from UTC.
/// </summary>
/// <remarks>
/// <para>**IT MEASURES AND IT NEVER CORRECTS ANYTHING** (Tim's ruling of
/// 2026-08-28). No clock is set, no internal clock is disciplined, and nothing
/// in the application uses a notion of time other than the machine's own. What
/// this produces is a number to display and to cut slots against.</para>
/// <para>**A FAILED QUERY LEAVES THE OFFSET UNKNOWN, NOT ZERO** (HM-DEC-009).
/// Unknown means slots are not cut and the tab says why, which is the honest
/// state; assuming zero would produce trimmed files that are quietly wrong and
/// unscoreable against WSJT-X.</para>
/// <para>**NEVER THROWS** (§8). It is called from a UI path on a timer, and
/// logging or timing that can crash the application is worse than none. Every
/// failure — no network, a refusing server, a malformed reply — comes back as
/// <see cref="ClockOffset.Unknown"/>.</para>
/// </remarks>
public static class SntpClock
{
    /// <summary>Where the query goes.</summary>
    /// <remarks>
    /// **THE POOL, NOT A VENDOR'S OWN SERVER.** `pool.ntp.org` is the
    /// volunteer-run rotation the NTP project points ordinary clients at, and
    /// using it is the polite default this project already commits to elsewhere
    /// for somebody else's service (HM-DEC-024).
    /// </remarks>
    public const string DefaultServer = "pool.ntp.org";

    /// <summary>How long to wait for an answer.</summary>
    public const int TimeoutMilliseconds = 3000;

    /// <summary>Ask once, and say what came back.</summary>
    /// <param name="pcUtcNow">What the machine believed when the call was made.</param>
    /// <param name="server">The server, or null for the default.</param>
    /// <returns>The offset, or <see cref="ClockOffset.Unknown"/>.</returns>
    /// <remarks>
    /// <para>**THE ROUND TRIP IS HALVED, WHICH IS THE STANDARD ESTIMATE AND IS
    /// NOT EXACT.** SNTP assumes the path is symmetric; on a bad connection it
    /// is not, and the error is a fraction of the round trip. At the half-second
    /// threshold this matters only on a link far worse than one that could run
    /// this application at all.</para>
    /// </remarks>
    public static async Task<ClockOffset> QueryAsync(
        DateTime pcUtcNow, string? server = null)
    {
        try
        {
            using var client = new UdpClient();
            client.Client.ReceiveTimeout = TimeoutMilliseconds;
            client.Client.SendTimeout = TimeoutMilliseconds;

            // Mode 3 (client), version 4, leap indicator 0.
            var request = new byte[48];
            request[0] = 0x1B;

            var sent = DateTime.UtcNow;

            await client
                .SendAsync(request, request.Length, server ?? DefaultServer, 123)
                .WaitAsync(TimeSpan.FromMilliseconds(TimeoutMilliseconds))
                .ConfigureAwait(false);

            var reply = await client
                .ReceiveAsync()
                .WaitAsync(TimeSpan.FromMilliseconds(TimeoutMilliseconds))
                .ConfigureAwait(false);

            var received = DateTime.UtcNow;

            if (reply.Buffer.Length < 48)
            {
                return ClockOffset.Unknown;
            }

            var serverUtc = TransmitTimestamp(reply.Buffer);

            if (serverUtc is not { } theirs)
            {
                return ClockOffset.Unknown;
            }

            // The reply describes the moment the server sent it, which is about
            // half a round trip before it arrived here.
            var roundTrip = received - sent;
            var hereWhenTheySent = received - (roundTrip / 2);

            return new ClockOffset(
                (theirs - hereWhenTheySent).TotalSeconds, received);
        }
        catch (SocketException)
        {
            return ClockOffset.Unknown;
        }
        catch (TimeoutException)
        {
            return ClockOffset.Unknown;
        }
        catch (ObjectDisposedException)
        {
            return ClockOffset.Unknown;
        }
        catch (InvalidOperationException)
        {
            return ClockOffset.Unknown;
        }
    }

    /// <summary>The transmit timestamp out of an SNTP reply.</summary>
    /// <param name="reply">At least 48 bytes.</param>
    /// <returns>The moment, or null when the reply says nothing usable.</returns>
    /// <remarks>
    /// <para>Bytes 40 to 47: seconds since 1900 and a binary fraction, both big
    /// endian. **Public so it can be tested without a network** — a parser
    /// reachable only through a socket is a parser nobody checks.</para>
    /// <para>**A ZERO TIMESTAMP IS A REFUSAL, NOT A DATE IN 1900.** Kiss-of-death
    /// replies and unsynchronised servers send it, and reading it as a time would
    /// report the clock as more than a century out.
    /// </para>
    /// </remarks>
    public static DateTime? TransmitTimestamp(ReadOnlySpan<byte> reply)
    {
        if (reply.Length < 48)
        {
            return null;
        }

        ulong seconds = 0;
        ulong fraction = 0;

        for (var i = 40; i < 44; i++)
        {
            seconds = (seconds << 8) | reply[i];
        }

        for (var i = 44; i < 48; i++)
        {
            fraction = (fraction << 8) | reply[i];
        }

        if (seconds == 0 && fraction == 0)
        {
            return null;
        }

        var milliseconds =
            (seconds * 1000.0) + (fraction * 1000.0 / 0x100000000L);

        return new DateTime(1900, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            .AddMilliseconds(milliseconds);
    }
}
