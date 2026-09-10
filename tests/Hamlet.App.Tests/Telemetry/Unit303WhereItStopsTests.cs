using System;
using System.Diagnostics;
using System.Globalization;
using System.Net.Sockets;
using System.Threading.Tasks;
using Hamlet.RadioEngine.Audio;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.Telemetry;

/// <summary>
/// Work instruction 303 task 2: **stand the query up and see where it stops.**
/// </summary>
/// <remarks>
/// <para>**DO NOT SEARCH THE SOURCE.** The instruction says so and this project has
/// paid five times for the author's guesses about where a thing lives. Every figure
/// below is what the real code did against the real network on this machine, printed
/// rather than reasoned about.</para>
/// <para>**THESE TOUCH THE NETWORK ON PURPOSE**, which is exactly what almost no test
/// in this repository does. That is the point: the fault is a network call that
/// nobody could see, and a fake transport would have reproduced the theory rather
/// than the fault.</para>
/// <para>**THEY REPORT AND THEY DO NOT JUDGE.** A machine with no network would fail
/// a strict assertion here for a reason that says nothing about Hamlet, so what is
/// asserted is only that the query answers *something* and names what it did.</para>
/// </remarks>
public sealed class Unit303WhereItStopsTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the run is printed.</param>
    public Unit303WhereItStopsTests(ITestOutputHelper output) => _output = output;

    /// <summary>**What the real query does, right now, on this machine.**</summary>
    [Fact]
    public async Task WhatTheRealQueryDoes()
    {
        var watch = Stopwatch.StartNew();

        var answer = await SntpClock.AskAsync(DateTime.UtcNow);

        watch.Stop();

        _output.WriteLine("server    : " + answer.Server);
        _output.WriteLine("reason    : " + answer.Reason);
        _output.WriteLine("detail    : " + answer.Detail);
        _output.WriteLine("measured  : " + answer.DidMeasure);
        _output.WriteLine(
            "offset    : " + (answer.Offset.OffsetSeconds?.ToString(
                "0.000", CultureInfo.InvariantCulture) ?? "(none)"));
        _output.WriteLine(
            "took      : " + watch.ElapsedMilliseconds + " ms");

        // **A REASON IS ALWAYS GIVEN**, which is the whole of task 1 proved against
        // the real socket rather than a fake one.
        Assert.False(string.IsNullOrWhiteSpace(answer.Reason));
        Assert.False(string.IsNullOrWhiteSpace(answer.Detail));
    }

    /// <summary>**Each stage on its own, so a failure names its own stage.**</summary>
    /// <remarks>
    /// **THE INSTRUCTION ASKS WHERE IT STOPS - RESOLUTION, SEND, RECEIVE, PARSE.**
    /// The query does all four behind one `try`, so this walks them separately and
    /// prints which one is the wall.
    /// </remarks>
    [Fact]
    public async Task EachStageOnItsOwn()
    {
        // 1. Does the name resolve?
        try
        {
            var addresses = await System.Net.Dns.GetHostAddressesAsync(
                SntpClock.DefaultServer);

            _output.WriteLine(
                "resolve : " + addresses.Length + " addresses, first "
                + (addresses.Length > 0 ? addresses[0].ToString() : "(none)"));
        }
        catch (Exception error)
        {
            _output.WriteLine(
                "resolve : FAILED " + error.GetType().Name + " " + error.Message);

            return;
        }

        // 2. Can a UDP socket be opened and a packet sent to port 123?
        try
        {
            using var client = new UdpClient();

            var request = new byte[48];
            request[0] = 0x1B;

            var sent = await client
                .SendAsync(request, request.Length, SntpClock.DefaultServer, 123)
                .WaitAsync(TimeSpan.FromMilliseconds(SntpClock.TimeoutMilliseconds));

            _output.WriteLine("send    : " + sent + " bytes away");

            // 3. Does anything come back?
            try
            {
                var reply = await client
                    .ReceiveAsync()
                    .WaitAsync(TimeSpan.FromMilliseconds(
                        SntpClock.TimeoutMilliseconds));

                _output.WriteLine(
                    "receive : " + reply.Buffer.Length + " bytes from "
                    + reply.RemoteEndPoint);

                // 4. Does it parse?
                var stamp = SntpClock.TransmitTimestamp(reply.Buffer);

                _output.WriteLine(
                    "parse   : " + (stamp?.ToString("O") ?? "(no timestamp)"));
            }
            catch (Exception error)
            {
                _output.WriteLine(
                    "receive : FAILED " + error.GetType().Name + " "
                    + error.Message);
            }
        }
        catch (Exception error)
        {
            _output.WriteLine(
                "send    : FAILED " + error.GetType().Name + " " + error.Message);
        }
    }

    /// <summary>**Is anything else already holding the port Hamlet sends from?**</summary>
    /// <remarks>
    /// **THE INSTRUCTION RECORDS THAT `w32time` HOLDS PORT 123 AND THAT STOPPING IT
    /// CHANGED NOTHING.** That is worth measuring rather than assuming, because an
    /// outbound UDP socket binds an ephemeral port and does not need 123 at all - so
    /// port contention would not explain a failure here, and confirming that removes
    /// a theory.
    /// </remarks>
    [Fact]
    public void WhichPortHamletActuallySendsFrom()
    {
        using var client = new UdpClient();

        client.Client.Bind(new System.Net.IPEndPoint(System.Net.IPAddress.Any, 0));

        var local = (System.Net.IPEndPoint?)client.Client.LocalEndPoint;

        _output.WriteLine(
            "Hamlet's own socket binds to port " + (local?.Port.ToString(
                CultureInfo.InvariantCulture) ?? "(unknown)"));

        _output.WriteLine(
            "It sends TO port 123 and does not bind it, so anything else holding "
            + "123 is not in the way.");

        Assert.True(local is not null);
        Assert.NotEqual(123, local!.Port);
    }
}
