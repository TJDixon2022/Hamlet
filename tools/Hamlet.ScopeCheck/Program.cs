using System.Diagnostics;
using Hamlet.RadioEngine.Civ;
using Hamlet.RadioEngine.Rig;
using Hamlet.RadioEngine.Transport;

namespace Hamlet.ScopeCheck;

/// <summary>
/// Six numbers about the scope path, measured on a connected radio
/// (HM-DEC-093).
/// </summary>
/// <remarks>
/// <para>**THE WATERFALL HAS BEEN REPORTED WORKING THREE TIMES AND HAS NEVER
/// DRAWN A PIXEL FROM A RADIO.** Every one of those reports was true of tests
/// and synthetic sources and false of the instrument, and none of them could
/// have been checked, because nothing counted anything. This exists so the next
/// claim carries a number.</para>
/// <para>**IT CHANGES NOTHING ABOUT THE RADIO EXCEPT THE ONE SETTING IT IS
/// TESTING**, and it puts that back. It reads, it asks for the wave output, it
/// listens, and it prints. Nothing here keys a transmitter (§0.2).</para>
/// <para>**AND IT ADVISES NOTHING.** Per FACT-001 the radio's CI-V USB port and
/// baud rate are ground truth and have been for days. This verifies them
/// silently by wire and reports a mismatch as a finding about the link or the
/// code, never as an errand.</para>
/// </remarks>
internal static class Program
{
    /// <summary>How long to listen for waveform frames.</summary>
    private static readonly TimeSpan ListenFor = TimeSpan.FromSeconds(8);

    private static async Task<int> Main(string[] args)
    {
        Console.WriteLine("Hamlet scope check");
        Console.WriteLine("==================");
        Console.WriteLine();

        var port = args.FirstOrDefault(a => !a.StartsWith('-'));

        if (port is null)
        {
            var found = System.IO.Ports.SerialPort.GetPortNames();

            Console.WriteLine("Give it the radio's port, for example:");
            Console.WriteLine("    scope-check COM3");
            Console.WriteLine();
            Console.WriteLine(found.Length == 0
                ? "This machine reports no serial ports at all."
                : "This machine reports: " + string.Join(", ", found));

            return 2;
        }

        using var serial = new SystemSerialPort(port);
        using var rig = new Ic7300Rig(serial);

        Console.WriteLine($"Opening {port}...");

        if (!await rig.ConnectAsync().ConfigureAwait(false))
        {
            Console.WriteLine($"  Could not open {port}. Nothing else can be measured.");
            return 1;
        }

        Console.WriteLine($"  open at {rig.Link.BaudRate} baud");
        Console.WriteLine();

        using var spectrum = new RigSpectrumSource(rig);
        var delivered = 0L;
        spectrum.FrameReady += (in Hamlet.RadioEngine.Training.SpectrumFrame _)
            => Interlocked.Increment(ref delivered);
        spectrum.Start();

        // ---- 1. the CI-V USB port, read rather than asked about -----------
        var usbPort = await ReadAsync(rig, CivReads.CivUsbPort).ConfigureAwait(false);

        Console.WriteLine("1. CI-V USB port (1A 05 0074, p. 19-5)");
        Console.WriteLine($"     {Describe(usbPort, v => v == 1 ? "01 Unlink from [REMOTE]" : $"{v:00} Link to [REMOTE]")}");

        // The link's own rate, which needs no radio to answer for it.
        var fast = rig.Link.BaudRate >= CivLinkHealth.ScopeOutputBaudRate;
        Console.WriteLine($"     host rate {rig.Link.BaudRate}"
            + (fast ? " (fast enough)" : " (TOO SLOW for 27 11)"));
        Console.WriteLine();

        // ---- 2. the write, attempted -------------------------------------
        Console.WriteLine("2. Asking for wave output (27 11 = 01)");

        var before = await ReadAsync(rig, CivReads.ScopeOutput).ConfigureAwait(false);
        var unansweredBefore = rig.Link.Unanswered;
        var outcome = "not attempted";

        try
        {
            var result = await rig.SetSettingAsync(CivWrites.ScopeOutput, 1)
                .ConfigureAwait(false);

            outcome = result.Outcome.ToString();
        }
        catch (Exception ex)
        {
            outcome = "threw: " + ex.GetType().Name;
        }

        Console.WriteLine($"     outcome {outcome}");
        Console.WriteLine(
            $"     unanswered commands {rig.Link.Unanswered} "
            + $"(was {unansweredBefore}), sent {rig.Link.Sent}, "
            + $"answered {rig.Link.Answered}");
        Console.WriteLine();

        // ---- 3. the readback ---------------------------------------------
        var after = await ReadAsync(rig, CivReads.ScopeOutput).ConfigureAwait(false);
        var scopeOn = await ReadAsync(rig, CivReads.ScopeOn).ConfigureAwait(false);

        Console.WriteLine("3. Readback");
        Console.WriteLine($"     27 11 before  {Describe(before, OnOff)}");
        Console.WriteLine($"     27 11 after   {Describe(after, OnOff)}");
        Console.WriteLine($"     27 10 scope   {Describe(scopeOn, OnOff)}");
        Console.WriteLine();

        // ---- 4, 5, 6. what actually arrives -------------------------------
        Console.WriteLine($"Listening {ListenFor.TotalSeconds:0} seconds for 27 00 frames...");

        var watch = Stopwatch.StartNew();
        while (watch.Elapsed < ListenFor)
        {
            await Task.Delay(250).ConfigureAwait(false);
        }

        Console.WriteLine();
        Console.WriteLine("4. Parts received off the wire");
        Console.WriteLine($"     {spectrum.PartsReceived}");
        Console.WriteLine();
        Console.WriteLine("5. Parsed / rejected");
        Console.WriteLine($"     parsed   {spectrum.PartsParsed}");
        Console.WriteLine($"     rejected {spectrum.PartsRejected}");
        Console.WriteLine($"     first rejection: "
            + (spectrum.FirstRejection.Length == 0 ? "none" : spectrum.FirstRejection));
        Console.WriteLine();
        Console.WriteLine("6. Sweeps completed / delivered");
        Console.WriteLine($"     completed {spectrum.SweepCount}, dropped {spectrum.DroppedCount}");
        Console.WriteLine($"     delivered {Interlocked.Read(ref delivered)}");
        Console.WriteLine();
        Console.WriteLine("     (drawn is only observable in the app, on the waterfall itself)");
        Console.WriteLine();

        // ---- the address of the bug --------------------------------------
        Console.WriteLine("First zero in the chain:");
        Console.WriteLine("     " + FirstZero(usbPort, after, spectrum, delivered));
        Console.WriteLine();

        // ---- put it back --------------------------------------------------
        if (before.IsKnown && before.Number is { } was && (int)was != 1)
        {
            Console.WriteLine($"Putting 27 11 back to {(int)was} as it was found.");

            try
            {
                await rig.SetSettingAsync(CivWrites.ScopeOutput, (int)was)
                    .ConfigureAwait(false);
            }
            catch (Exception)
            {
                Console.WriteLine("  could not put it back.");
            }
        }

        spectrum.Stop();
        await rig.DisconnectAsync().ConfigureAwait(false);

        return 0;
    }

    /// <summary>What to look at first, which is where the chain first breaks.</summary>
    private static string FirstZero(
        RigValue usbPort, RigValue output, RigSpectrumSource spectrum, long delivered)
    {
        if (!usbPort.IsKnown)
        {
            return "1A 05 0074 did not answer. The link is not carrying reads, "
                + "so nothing below it means anything.";
        }

        if (usbPort.Number is not 1)
        {
            return "1A 05 0074 reads Link to [REMOTE]. That contradicts FACT-001, "
                + "so it is a finding about this reading rather than an errand: "
                + "either the sub-command is wrong or the link is answering for "
                + "something else.";
        }

        if (!output.IsKnown || output.Number is not 1)
        {
            return "27 11 does not read back as on after the write. The radio is "
                + "not accepting the setting, and both documented conditions on it "
                + "are satisfied.";
        }

        if (spectrum.PartsReceived == 0)
        {
            return "Wave output reads on and no 27 00 parts arrived at all. "
                + "The question is between the radio and the frame reader.";
        }

        if (spectrum.PartsParsed == 0)
        {
            return "Parts arrived and none parsed. The 11-part shape on the wire "
                + "is not what the parser expects: " + spectrum.FirstRejection;
        }

        if (spectrum.SweepCount == 0)
        {
            return "Parts parsed and no sweep ever completed. Reassembly is "
                + $"losing parts: {spectrum.DroppedCount} sweeps dropped.";
        }

        if (delivered == 0)
        {
            return "Sweeps completed and none were delivered. The event is not "
                + "reaching its subscriber.";
        }

        return "Nothing is zero. Frames are arriving, parsing, completing and "
            + "being delivered, so anything still wrong is in the drawing.";
    }

    private static string OnOff(double v) => v == 1 ? "01 on" : $"{(int)v:00} off";

    private static string Describe(RigValue value, Func<double, string> say)
        => value switch
        {
            { IsKnown: true, Number: { } n } => say(n),
            { State: RigValueState.Unsupported } => "the radio says it has no such setting",
            _ => "UNREAD — no answer",
        };

    private static async Task<RigValue> ReadAsync(Ic7300Rig rig, CivRead read)
    {
        try
        {
            var values = await rig.ReadAsync(read.Field, RigState.Empty)
                .ConfigureAwait(false);

            return values.Count > 0 ? values[0] : RigValue.Unknown(read.Field);
        }
        catch (Exception)
        {
            return RigValue.Unknown(read.Field);
        }
    }
}
