using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using Hamlet.RadioEngine.Civ;
using Hamlet.RadioEngine.Transport;
using Xunit;

namespace Hamlet.RadioEngine.Tests.Rig;

/// <summary>
/// Work instruction 253, task 4. The transmit abort, watched to fire, before
/// anything in this repository can key a transmitter.
/// </summary>
/// <remarks>
/// <para>**THERE IS NO CALLER.** <see cref="TransmitAbort"/> is exercised here
/// against a fake CI-V transport and nowhere else in the tree. Step 1 of the
/// phase says the abort is proven before the thing it aborts exists, and this
/// file is that proof.</para>
/// <para>The breakages these cases would have caught are B19 to B22 in
/// <c>docs/breakage-record.md</c>. B19 and B20 are defects that were in the tree
/// when this unit opened; B21 and B22 are tripwires and are marked as such.</para>
/// </remarks>
public sealed class TheAbortFiresFromEveryStateTests
{
    /// <summary>
    /// The stated bound. Two frames at a seam that copies bytes into a list;
    /// anything approaching this is a scheduler, a lock or a wait that has no
    /// business on this path.
    /// </summary>
    private const int TimeBoundMs = 50;

    private static readonly byte[] ExpectedCwStop =
        { 0xFE, 0xFE, 0x94, 0xE0, 0x17, 0xFF, 0xFD };

    private static readonly byte[] ExpectedPttOff =
        { 0xFE, 0xFE, 0x94, 0xE0, 0x1C, 0x00, 0x00, 0xFD };

    // ---- the four states a transmission can be in ------------------------

    [Fact]
    public void ItFiresWhenTheTransmissionIsAboutToKey()
        => BothFramesGoOutFrom(Transmission.AboutToKey);

    [Fact]
    public void ItFiresWhileTheRadioIsKeying()
        => BothFramesGoOutFrom(Transmission.Keying);

    [Fact]
    public void ItFiresMidTransmission()
        => BothFramesGoOutFrom(Transmission.MidTransmission);

    [Fact]
    public void ItFiresWhileWaitingToUnkey()
        => BothFramesGoOutFrom(Transmission.WaitingToUnkey);

    /// <summary>
    /// The abort does not know what the transmission was doing and cannot be
    /// made to care. The state is carried by the transport so the assertion can
    /// say which one it fired from rather than reasoning that it would have.
    /// </summary>
    private static void BothFramesGoOutFrom(Transmission state)
    {
        using var port = new AbortTransport { State = state };

        var record = TransmitAbort.Fire(port);

        Assert.True(record.CwStop.Written);
        Assert.True(record.PttOff.Written);
        Assert.Equal(2, port.Writes.Count);
        Assert.Equal(ExpectedCwStop, port.Writes[0].Bytes);
        Assert.Equal(ExpectedPttOff, port.Writes[1].Bytes);
        Assert.All(port.Writes, w => Assert.Equal(state, w.State));
    }

    // ---- exactly what goes on the wire -----------------------------------

    [Fact]
    public void TheTwoFramesAreTheStopCodeAndPttOff()
    {
        using var port = new AbortTransport();

        var record = TransmitAbort.Fire(port);

        Assert.Equal(ExpectedCwStop, record.CwStop.Frame.ToWireBytes());
        Assert.Equal(ExpectedPttOff, record.PttOff.Frame.ToWireBytes());
        Assert.Equal(CivConstants.CmdSendCwMessage, record.CwStop.Frame.Command);
        Assert.Equal(CivConstants.CwStopByte, record.CwStop.Frame.Data[0]);
        Assert.Equal(CivConstants.CmdTransceiverControl, record.PttOff.Frame.Command);
        Assert.Equal(CivConstants.SubPtt, record.PttOff.Frame.Data[0]);
        Assert.Equal(CivConstants.PttOff, record.PttOff.Frame.Data[1]);
    }

    // ---- the transport in every state of ruin ----------------------------

    [Fact]
    public void ItFiresWhenTheTransportIsDead()
    {
        var port = new AbortTransport();
        port.Dispose();

        var record = TransmitAbort.Fire(port);

        // Both were reached for. Neither landed, and the record says which.
        Assert.False(record.CwStop.Written);
        Assert.False(record.PttOff.Written);
        Assert.False(record.AnythingReachedTheRadio);
        Assert.Equal(2, port.Attempts);
        Assert.NotNull(record.CwStop.Failure);
        Assert.NotNull(record.PttOff.Failure);
    }

    [Fact]
    public void ItFiresWhenThePortIsGone()
    {
        using var port = new AbortTransport();
        port.Close();
        port.ThrowOn = _ => new IOException("The port is gone (USB pulled).");

        var record = TransmitAbort.Fire(port);

        Assert.Equal(2, port.Attempts);
        Assert.False(record.AnythingReachedTheRadio);
        Assert.Contains("gone", record.CwStop.Failure, StringComparison.Ordinal);
        Assert.Contains("gone", record.PttOff.Failure, StringComparison.Ordinal);
    }

    [Fact]
    public void ItFiresWhenTheRadioDoesNotAnswer()
    {
        // The transport takes the bytes and the radio says nothing back, ever.
        // The abort reads nothing, so there is nothing to time out on: it must
        // put both frames out and return.
        using var port = new AbortTransport { AnswersNothing = true };
        var clock = Stopwatch.StartNew();

        var record = TransmitAbort.Fire(port);

        clock.Stop();
        Assert.True(record.CwStop.Written);
        Assert.True(record.PttOff.Written);
        Assert.True(
            clock.ElapsedMilliseconds < TimeBoundMs,
            $"waited {clock.ElapsedMilliseconds} ms on a radio that never answers");
    }

    [Fact]
    public void ItFiresWhenTheCivWriteItselfThrows()
    {
        using var port = new AbortTransport();
        port.ThrowOn = _ => new InvalidOperationException("scripted write failure");

        // Nothing propagates. An abort that can throw is not an abort.
        var record = TransmitAbort.Fire(port);

        Assert.Equal(2, port.Attempts);
        Assert.False(record.AnythingReachedTheRadio);
    }

    // ---- the fallback, exercised rather than reasoned about ---------------

    [Fact]
    public void ThePttOffGoesOutWhenTheStopFrameThrows()
    {
        using var port = new AbortTransport();
        port.ThrowOn = command =>
            command == CivConstants.CmdSendCwMessage
                ? new IOException("the keyer command was refused")
                : null;

        var record = TransmitAbort.Fire(port);

        Assert.False(record.CwStop.Written);
        Assert.True(record.PttOff.Written);
        Assert.True(record.AnythingReachedTheRadio);
        Assert.Single(port.Landed);
        Assert.Equal(ExpectedPttOff, port.Landed[0]);
    }

    [Fact]
    public void TheStopFrameGoesOutWhenThePttOffThrows()
    {
        using var port = new AbortTransport();
        port.ThrowOn = command =>
            command == CivConstants.CmdTransceiverControl
                ? new IOException("the transceiver command was refused")
                : null;

        var record = TransmitAbort.Fire(port);

        Assert.True(record.CwStop.Written);
        Assert.False(record.PttOff.Written);
        Assert.True(record.AnythingReachedTheRadio);
        Assert.Single(port.Landed);
        Assert.Equal(ExpectedCwStop, port.Landed[0]);
    }

    // ---- the properties a later session could quietly remove -------------

    /// <summary>
    /// **THIS IS THE TEST THAT FAILS IF SOMEBODY PUTS A WAIT ON THE ABORT.**
    /// </summary>
    /// <remarks>
    /// Two instruments, because either alone is escapable. Reflection catches a
    /// compiler-generated state machine and a task-shaped return; the source
    /// scan catches the keyword itself, including in a helper that reflection
    /// would have to be told about. Comment lines are stripped first, or this
    /// file's own prose about the rule would break the rule.
    /// </remarks>
    [Fact]
    public void NothingOnTheAbortPathWaitsForAnything()
    {
        var type = typeof(TransmitAbort);

        foreach (var method in type.GetMethods(
            BindingFlags.Public | BindingFlags.NonPublic
            | BindingFlags.Static | BindingFlags.Instance | BindingFlags.DeclaredOnly))
        {
            Assert.Null(method.GetCustomAttribute<AsyncStateMachineAttribute>());
            Assert.False(
                typeof(Task).IsAssignableFrom(method.ReturnType),
                $"{method.Name} returns a task");
            Assert.False(
                method.ReturnType == typeof(ValueTask)
                || (method.ReturnType.IsGenericType
                    && method.ReturnType.GetGenericTypeDefinition() == typeof(ValueTask<>)),
                $"{method.Name} returns a value task");
        }

        foreach (var nested in type.GetNestedTypes(
            BindingFlags.Public | BindingFlags.NonPublic))
        {
            Assert.False(
                typeof(IAsyncStateMachine).IsAssignableFrom(nested),
                $"{nested.Name} is a generated state machine");
        }

        // The seam it writes through returns void. That is a compile-time fact
        // and it is asserted so that widening it shows up here.
        var seam = typeof(ISerialPort).GetMethod(
            "Write", new[] { typeof(ReadOnlySpan<byte>) });
        Assert.NotNull(seam);
        Assert.Equal(typeof(void), seam.ReturnType);

        var code = StrippedOfComments(AbortSource());
        Assert.DoesNotContain("await ", code, StringComparison.Ordinal);
        Assert.DoesNotContain("async ", code, StringComparison.Ordinal);
        Assert.DoesNotContain("Task", code, StringComparison.Ordinal);
        Assert.DoesNotContain(".Wait(", code, StringComparison.Ordinal);
        Assert.DoesNotContain(".Result", code, StringComparison.Ordinal);
        Assert.DoesNotContain("GetAwaiter", code, StringComparison.Ordinal);
    }

    /// <summary>
    /// No flag turns it off. The moment somebody adds one, this fails.
    /// </summary>
    [Fact]
    public void NoFlagCanTurnTheAbortOff()
    {
        var type = typeof(TransmitAbort);

        // A static class holds no instance anybody could configure.
        Assert.True(type.IsAbstract && type.IsSealed, "TransmitAbort is not static");

        Assert.Empty(type.GetFields(
            BindingFlags.Public | BindingFlags.NonPublic
            | BindingFlags.Static | BindingFlags.DeclaredOnly));

        Assert.Empty(type.GetProperties(
            BindingFlags.Public | BindingFlags.NonPublic
            | BindingFlags.Static | BindingFlags.DeclaredOnly));

        foreach (var method in type.GetMethods(
            BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly))
        {
            foreach (var parameter in method.GetParameters())
            {
                Assert.False(
                    parameter.ParameterType == typeof(bool)
                    || parameter.ParameterType == typeof(bool?),
                    $"{method.Name} takes a switch called {parameter.Name}");
            }
        }

        var code = StrippedOfComments(AbortSource());
        Assert.DoesNotContain("enabled", code, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("if (", code, StringComparison.Ordinal);
    }

    [Fact]
    public void TheAbortCompletesWithinItsTimeBound()
    {
        using var port = new AbortTransport();

        // Once through first, so the measurement is not of the JIT.
        TransmitAbort.Fire(port);
        port.Reset();

        var clock = Stopwatch.StartNew();
        var record = TransmitAbort.Fire(port);
        clock.Stop();

        Assert.True(record.AnythingReachedTheRadio);
        Assert.True(
            clock.Elapsed.TotalMilliseconds < TimeBoundMs,
            $"the abort took {clock.Elapsed.TotalMilliseconds:F3} ms, "
            + $"bound is {TimeBoundMs} ms");
    }

    // ---- reading the abort's own source ----------------------------------

    private static string AbortSource()
    {
        var at = new DirectoryInfo(AppContext.BaseDirectory);
        while (at is not null && !File.Exists(Path.Combine(at.FullName, "Hamlet.sln")))
        {
            at = at.Parent;
        }

        var root = at?.FullName
            ?? throw new InvalidOperationException("no Hamlet.sln above the test binary");

        var path = Path.Combine(
            root, "src", "Hamlet.RadioEngine", "Civ", "TransmitAbort.cs");

        Assert.True(File.Exists(path), $"the abort's source is not at {path}");
        return File.ReadAllText(path);
    }

    private static string StrippedOfComments(string source)
    {
        var kept = source
            .Split('\n')
            .Where(line =>
            {
                var trimmed = line.TrimStart();
                return !trimmed.StartsWith("//", StringComparison.Ordinal)
                    && !trimmed.StartsWith("*", StringComparison.Ordinal)
                    && !trimmed.StartsWith("/*", StringComparison.Ordinal);
            });

        return string.Join('\n', kept);
    }

    // ---- the fake CI-V transport -----------------------------------------

    private enum Transmission
    {
        AboutToKey,
        Keying,
        MidTransmission,
        WaitingToUnkey,
    }

    private sealed record WriteRecord(byte[] Bytes, Transmission State);

    /// <summary>
    /// A CI-V transport that can be dead, gone, silent, or refuse a particular
    /// command, and that remembers what the transmission was doing when each
    /// write arrived.
    /// </summary>
    private sealed class AbortTransport : ISerialPort
    {
        private bool _disposed;

        public string PortName => "COM-ABORT";

        public int BaudRate => 115_200;

        public bool IsOpen { get; private set; } = true;

        /// <summary>What the transmission was doing when the abort came.</summary>
        public Transmission State { get; init; } = Transmission.MidTransmission;

        /// <summary>The radio takes the bytes and never says anything back.</summary>
        public bool AnswersNothing { get; init; }

        /// <summary>Given a command byte, what the write should throw, if
        /// anything.</summary>
        public Func<byte, Exception?>? ThrowOn { get; set; }

        /// <summary>Every write that was reached for, landed or not.</summary>
        public int Attempts { get; private set; }

        /// <summary>Every write that landed, with the state it landed in.</summary>
        public List<WriteRecord> Writes { get; } = new();

        /// <summary>The bytes of every write that landed.</summary>
        public List<byte[]> Landed => Writes.Select(w => w.Bytes).ToList();

        public void Reset()
        {
            Attempts = 0;
            Writes.Clear();
        }

        public void Open() => IsOpen = true;

        public void Close() => IsOpen = false;

        public void Write(ReadOnlySpan<byte> buffer)
        {
            Attempts++;

            ObjectDisposedException.ThrowIf(_disposed, this);

            var bytes = buffer.ToArray();
            var command = bytes.Length > 4 ? bytes[4] : (byte)0x00;

            var scripted = ThrowOn?.Invoke(command);
            if (scripted is not null)
            {
                throw scripted;
            }

            Writes.Add(new WriteRecord(bytes, State));
        }

        public ValueTask WriteAsync(
            ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken)
            => throw new InvalidOperationException(
                "the abort path does not use the asynchronous seam");

        public ValueTask<int> ReadAsync(
            Memory<byte> buffer, CancellationToken cancellationToken)
            => AnswersNothing
                ? new ValueTask<int>(new TaskCompletionSource<int>().Task)
                : new ValueTask<int>(0);

        public void Dispose()
        {
            _disposed = true;
            IsOpen = false;
        }
    }
}
