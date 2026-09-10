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
        => (await AskAsync(pcUtcNow, server).ConfigureAwait(false)).Offset;

    /// <summary>
    /// **Ask a time server, and say what happened either way.**
    /// </summary>
    /// <param name="pcUtcNow">What this machine believes the time is.</param>
    /// <param name="server">The server to ask, or null for the pool.</param>
    /// <returns>The offset where one was measured, and always the reason.</returns>
    /// <remarks>
    /// <para>**SIX WAYS TO FAIL AND ONE ANSWER** (work instruction 303 task 1). Every
    /// path below used to return `ClockOffset.Unknown` and nothing else, so a name
    /// that would not resolve, a packet that never came back, a reply too short and a
    /// timestamp that would not parse were **indistinguishable from each other and
    /// from never having asked at all.**</para>
    /// <para>**THAT IS WHAT COST THE TWENTY MINUTES.** The screen said Hamlet had not
    /// been able to check the clock, which is a sentence about a failure, while the
    /// record held nothing whatever - and *asked and failed* and *never asked* are
    /// different problems with different fixes.</para>
    /// <para>**THE OFFSET IS UNCHANGED.** This returns exactly what `QueryAsync`
    /// returned; what is added is the reason beside it, for the caller to record.
    /// </para>
    /// </remarks>
    public static async Task<ClockAnswer> AskAsync(
        DateTime pcUtcNow, string? server = null)
    {
        var asked = server ?? DefaultServer;

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
                return ClockAnswer.Failed(
                    asked, "short_reply",
                    "the reply was " + reply.Buffer.Length
                    + " bytes and an SNTP reply is 48");
            }

            var serverUtc = TransmitTimestamp(reply.Buffer);

            if (serverUtc is not { } theirs)
            {
                return ClockAnswer.Failed(
                    asked, "unreadable_timestamp",
                    "the reply carried no transmit timestamp");
            }

            // The reply describes the moment the server sent it, which is about
            // half a round trip before it arrived here.
            var roundTrip = received - sent;
            var hereWhenTheySent = received - (roundTrip / 2);

            return ClockAnswer.Measured(
                asked,
                new ClockOffset(
                    (theirs - hereWhenTheySent).TotalSeconds, received),
                roundTrip);
        }
        catch (SocketException error)
        {
            // **THE ONE THAT NAMES ITSELF.** A name that will not resolve, a refused
            // port and a network that is not there all arrive here, and the socket
            // error code is what tells them apart - so it is recorded rather than
            // thrown away.
            return ClockAnswer.Failed(
                asked, "socket_" + error.SocketErrorCode, error.Message);
        }
        catch (TimeoutException)
        {
            return ClockAnswer.Failed(
                asked, "timeout",
                "nothing came back within " + TimeoutMilliseconds + " ms");
        }
        catch (ObjectDisposedException)
        {
            return ClockAnswer.Failed(
                asked, "disposed", "the socket was closed while asking");
        }
        catch (InvalidOperationException error)
        {
            return ClockAnswer.Failed(asked, "invalid_operation", error.Message);
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

/// <summary>What one clock query did, whether or not it measured anything.</summary>
/// <param name="Server">Which server was asked.</param>
/// <param name="Offset">The offset, or <see cref="ClockOffset.Unknown"/>.</param>
/// <param name="Reason">
/// A **stable machine token** for what happened: `measured`, `timeout`,
/// `short_reply`, `unreadable_timestamp`, `disposed`, `invalid_operation`, or
/// `socket_` and the socket error code.
/// </param>
/// <param name="Detail">The same thing in words, for a person reading the file.</param>
/// <param name="RoundTrip">How long the exchange took, where it completed.</param>
/// <remarks>
/// <para>**A STABLE TOKEN AND A SENTENCE, WHICH IS §8.1'S OWN SHAPE.** The token is
/// what a comparison across sessions is made on; the sentence is what a person reads.
/// A display string alone gets reworded the next time somebody improves the copy and
/// takes every comparison with it.</para>
/// <para>**NOTHING HERE IS PERSONAL** (§2.1). A time server's hostname is not
/// personal and neither is an offset.</para>
/// </remarks>
public sealed record ClockAnswer(
    string Server,
    ClockOffset Offset,
    string Reason,
    string Detail,
    TimeSpan? RoundTrip = null)
{
    /// <summary>True where a server answered and the offset is real.</summary>
    public bool DidMeasure => Offset.IsKnown;

    /// <summary>A query that worked.</summary>
    /// <param name="server">Which server answered.</param>
    /// <param name="offset">What it measured.</param>
    /// <param name="roundTrip">How long the exchange took.</param>
    /// <returns>The answer.</returns>
    public static ClockAnswer Measured(
        string server, ClockOffset offset, TimeSpan roundTrip)
        => new(
            server,
            offset,
            "measured",
            "the server answered in "
                + roundTrip.TotalMilliseconds.ToString("0")
                + " ms",
            roundTrip);

    /// <summary>A query that did not work, and why.</summary>
    /// <param name="server">Which server was asked.</param>
    /// <param name="reason">The stable token.</param>
    /// <param name="detail">The same thing in words.</param>
    /// <returns>The answer.</returns>
    public static ClockAnswer Failed(string server, string reason, string detail)
        => new(server, ClockOffset.Unknown, reason, detail);
}
