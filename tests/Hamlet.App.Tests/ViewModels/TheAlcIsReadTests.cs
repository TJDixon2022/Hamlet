using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Hamlet.App.Settings;
using Hamlet.App.ViewModels;
using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Civ;
using Hamlet.RadioEngine.Contacts;
using Hamlet.RadioEngine.Rig;
using Hamlet.RadioEngine.Telemetry;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.ViewModels;

/// <summary>
/// Work instruction 324 task 4b: **the ALC has a read.**
/// </summary>
/// <remarks>
/// <para>**UNIT 323 BUILT THE SENTENCE AND THERE WAS NOTHING TO FILL IT** (its item 47).
/// `RigField.Alc` existed, no `CivRead` anywhere in the tree asked the radio for it, so
/// `RigStateMonitor` never held a value and the sentence could only ever fire from a
/// reading a test handed in. **It is in the manual**: section 19's command table, beside
/// `15 11` (power meter) and `15 12` (SWR) which Hamlet already reads, is `15 13`, *Read
/// ALC meter level*, `00 00` = minimum to `01 20` = maximum. BCD, so the scale is 0 to
/// 120.</para>
/// <para>**AND WHAT THE MANUAL DOES NOT GIVE IS NOT INVENTED** (§0.0, §4). The command
/// table says nothing about where the ALC zone ends on that scale; for data modes the
/// manual says only *adjust the device's output level within the ALC zone*. Unit 323's
/// figure of 128 was half of the 0-255 range the transmit power **setting** uses, which is
/// not this meter's range. It is gone, no second number replaces it, and the threshold is
/// an ask on the owner.</para>
/// <para>**NO RADIO ON THIS MACHINE** (FACT-006). The read is proved as far as the poll
/// and stops there: what is asserted is that the command exists, that it is asked for only
/// while a send is running, that a reply decodes on the manual's scale, and that with no
/// radio the record says `measured: false` rather than a comfortable zero.</para>
/// </remarks>
public sealed class TheAlcIsReadTests : IDisposable
{
    private readonly ITestOutputHelper _output;
    private readonly string _folder;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the command and the record are printed.</param>
    public TheAlcIsReadTests(ITestOutputHelper output)
    {
        _output = output;

        _folder = Path.Combine(
            Path.GetTempPath(), "hamlet-alc-" + Guid.NewGuid().ToString("N"));

        Directory.CreateDirectory(_folder);
    }

    /// <summary>Removes the telemetry folder.</summary>
    public void Dispose()
    {
        try
        {
            Directory.Delete(_folder, true);
        }
        catch (IOException)
        {
            // A left-over temp folder is not a test failure.
        }
    }

    /// <summary>**Assertion 1: the command is the manual's, and it is cited.**</summary>
    [Fact]
    public void TheCommandIsTheOneInTheManualsTable()
    {
        var read = CivReads.Alc;

        _output.WriteLine(
            "command : " + read.Label + "  page " + read.Page + "  " + read.Note);

        Assert.Equal(RigField.Alc, read.Field);
        Assert.Equal(0x15, read.Command);
        Assert.Equal(new byte[] { 0x13 }, read.SubCommand.ToArray());
        Assert.False(string.IsNullOrWhiteSpace(read.Page));

        // **AND IT IS ON THE LIST THE DIAGNOSTICS SCREEN AND THE SWEEP READ FROM**, or it
        // would be a command nothing ever issues.
        Assert.Contains(CivReads.All, r => r.Field == RigField.Alc);
    }

    /// <summary>**Assertion 2: the reply decodes from BCD on the 0 to 120 scale.**</summary>
    /// <remarks>
    /// **THE THREE POINTS THE TABLE GIVES, AND ONE THAT IS NOT A NUMBER AT ALL.** `00 00`
    /// is the bottom, `01 20` the top, and a byte whose nibbles are not decimal digits is
    /// not a reading of zero - it is an unreadable reply, and the value stays unknown
    /// (§0.0).
    /// </remarks>
    [Theory]
    [InlineData(0x00, 0x00, 0)]
    [InlineData(0x00, 0x60, 60)]
    [InlineData(0x01, 0x20, 120)]
    public void TheReplyDecodesOnTheManualsScale(int high, int low, int expected)
    {
        var value = Decode((byte)high, (byte)low);

        _output.WriteLine(((byte)high).ToString("X2", CultureInfo.InvariantCulture) + " "
            + ((byte)low).ToString("X2", CultureInfo.InvariantCulture) + " -> "
            + value.Number + "  \"" + value.Text + "\"");

        Assert.True(value.IsKnown);
        Assert.Equal(expected, value.Number);
        Assert.Equal(120, CivAlc.FullScale);
    }

    /// <summary>**And a reply that is not BCD leaves the value unknown.**</summary>
    [Fact]
    public void AReplyThatIsNotBcdIsNotAReadingOfZero()
    {
        var value = Decode(0x0F, 0xFF);

        Assert.False(value.IsKnown);
    }

    /// <summary>
    /// **Assertion 3: the poll asks for it only while something is sending.**
    /// </summary>
    /// <remarks>
    /// **THE BUS IS SHARED AND THE ANSWER IS MEANINGLESS AT REST** (HM-DEC-050). ALC is the
    /// radio holding back a signal it is being given too much of; a resting transmitter is
    /// holding nothing back, so asking four times a second buys a reading of nothing.
    /// </remarks>
    [Fact]
    public async Task ThePollAsksForTheAlcOnlyWhileSomethingIsSending()
    {
        var rig = new RecordingRig();

        using var monitor = new RigStateMonitor(rig, (_, _) => Task.CompletedTask);

        monitor.Start();

        await WaitFor(() => monitor.ReadCount > 60);

        var atRest = rig.Count(RigField.Alc);

        _output.WriteLine("at rest, after " + monitor.ReadCount + " reads: "
            + atRest + " ALC read(s)");

        Assert.Equal(0, atRest);

        monitor.WantsAlc = true;

        await WaitFor(() => rig.Count(RigField.Alc) > 0);

        var sending = rig.Count(RigField.Alc);

        monitor.WantsAlc = false;

        await Task.Delay(50);

        var settled = rig.Count(RigField.Alc);

        await Task.Delay(200);

        monitor.Stop();

        _output.WriteLine("while sending: " + sending + " ALC read(s); after: "
            + (rig.Count(RigField.Alc) - settled) + " more");

        Assert.True(sending > 0);
        Assert.Equal(settled, rig.Count(RigField.Alc));
    }

    /// <summary>
    /// **Assertion 4: with no radio the reading is absent and the record says so.**
    /// </summary>
    /// <remarks>
    /// **ABSENT IS NOT ZERO AND IT IS NOT *FINE*** (§0.0). On this machine there is no rig
    /// at all, and what is written down is that nothing was measured - which is the finding
    /// rather than a silence somebody downstream has to infer.
    /// </remarks>
    [Fact]
    public void WithNoRadioTheReadingIsAbsentAndTheRecordSaysSo()
    {
        var lines = Read(model => model.ReadTheAlcForTests());
        var alc = Events(lines, "psk31_send_alc").Single();

        _output.WriteLine(alc.ToString());

        Assert.False(alc.GetProperty("data").GetProperty("measured").GetBoolean());
        Assert.Equal(
            JsonValueKind.Null, alc.GetProperty("data").GetProperty("alc").ValueKind);
        Assert.False(alc.GetProperty("data").GetProperty("pastTheZone").GetBoolean());
    }

    /// <summary>
    /// **Assertion 5: the zone is not invented, and the record says it was not judged.**
    /// </summary>
    /// <remarks>
    /// **THE READING IS REPORTED AND NOT JUDGED** (§0.0, §4). Unit 323 compared it with
    /// 128, which was half of the transmit power setting's 0-255 range and had nothing to
    /// do with this meter. The scale is the manual's 0 to 120; where the ALC zone ends on
    /// that scale is not in the manual, so `zone` is absent, `judged` is false, and the
    /// sentence tells the operator what was read and asks him to look at the bar on the
    /// radio - which is a thing he can do and Hamlet cannot.
    /// </remarks>
    [Fact]
    public void TheZoneIsNotInventedAndTheSentenceSaysWhatWasReadRatherThanJudgingIt()
    {
        Assert.Equal(CivAlc.FullScale, MainWindowViewModel.Psk31AlcScaleTop);

        var lines = Read(model =>
        {
            // **`Psk31AlcZone` BECAME `Psk31AlcReference` UNDER §R15** (Tim,
            // 2026-09-11, work instruction 325 task 6). The static property was
            // always null and this assertion said *nothing is invented*; the
            // replacement is an instance property that is null until an FT8 send
            // has been observed, and this asserts the same fact in the new shape.
            // **Nothing about this test's subject has changed**: with no reference,
            // the reading is reported and no verdict is passed.
            Assert.Null(model.Psk31AlcReference);

            model.Psk31AlcForTests = RigValue.Known(
                RigField.Alc, 96, CivAlc.Describe(96), DateTime.UtcNow, "15 13");

            model.ReadTheAlcForTests();

            _output.WriteLine("sentence: " + model.Psk31AlcLine);

            Assert.True(model.HasPsk31AlcLine);
            Assert.Contains("96", model.Psk31AlcLine, StringComparison.Ordinal);
            Assert.Contains("120", model.Psk31AlcLine, StringComparison.Ordinal);

            // **AND IT CLAIMS NOTHING ABOUT WHETHER 96 IS TOO MUCH.**
            Assert.DoesNotContain(
                "driven harder", model.Psk31AlcLine, StringComparison.OrdinalIgnoreCase);
        });

        var data = Events(lines, "psk31_send_alc").Single().GetProperty("data");

        Assert.True(data.GetProperty("measured").GetBoolean());
        Assert.Equal(96, data.GetProperty("alc").GetDouble());
        Assert.Equal(CivAlc.FullScale, data.GetProperty("scaleTop").GetDouble());
        Assert.False(data.GetProperty("judged").GetBoolean());
        Assert.Equal(JsonValueKind.Null, data.GetProperty("zone").ValueKind);
        Assert.False(data.GetProperty("pastTheZone").GetBoolean());
    }

    /// <summary>One reply from `15 13`, through the decoder the link uses.</summary>
    private static RigValue Decode(byte high, byte low)
        => CivDecode.Values(
            CivReads.Alc,
            new byte[] { high, low },
            DateTime.UtcNow,
            mode: null,
            filterName: null).Single();

    /// <summary>Run something against a panel with the record on, and read the file.</summary>
    private List<string> Read(Action<MainWindowViewModel> what)
    {
        using (var telemetry = new JsonlTelemetry(_folder, "324", _ => true))
        {
            var settings = new AppSettings { ReconnectOnStartup = false };

            settings.Operator.Callsign = "KC3QIS";
            settings.Operator.GridSquare = "FN00DJ";

            var model = new MainWindowViewModel(settings, telemetry)
            {
                OperatingMode = "Digital",
                TapForTests = new AudioTap(),
                ClockOffset = new ClockOffset(0.033, DateTime.UtcNow),
            };

            model.UseWorkedBeforeForTests(new Dictionary<string, AdifContact>());

            what(model);
        }

        return Directory.GetFiles(_folder, "*.jsonl").SelectMany(File.ReadAllLines).ToList();
    }

    private static List<JsonElement> Events(IEnumerable<string> lines, string name)
        => lines
            .Where(l => l.Contains("\"" + name + "\"", StringComparison.Ordinal))
            .Select(l => JsonDocument.Parse(l).RootElement)
            .Where(e => e.GetProperty("event").GetString() == name)
            .ToList();

    private static async Task WaitFor(Func<bool> condition)
    {
        var deadline = DateTime.UtcNow.AddSeconds(10);

        while (!condition() && DateTime.UtcNow < deadline)
        {
            await Task.Delay(5).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// A rig that answers instantly and remembers which field it was asked for.
    /// </summary>
    /// <remarks>
    /// **HAND-ROLLED RATHER THAN A MOCKING FRAMEWORK** (§6), and the same shape as
    /// `RigPollingTests.CountingRig`. What it exists to catch is a command on the bus at a
    /// moment nothing should be asking for it.
    /// </remarks>
    private sealed class RecordingRig : IRig
    {
        private readonly object _gate = new();
        private readonly Dictionary<RigField, int> _asked = new();

        public bool IsConnected => true;

        public bool IsSimulated => false;

        public RigCapabilities Capabilities { get; } = new(
            "Recording radio", false, false, false, false, Array.Empty<string>());

        public event EventHandler<FrequencyChangedEventArgs>? FrequencyChanged;

        public event EventHandler<RigValuesReportedEventArgs>? ValuesReported;

        public int Count(RigField field)
        {
            lock (_gate)
            {
                return _asked.GetValueOrDefault(field);
            }
        }

        public Task<bool> ConnectAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(true);

        public Task DisconnectAsync() => Task.CompletedTask;

        public Task<long> GetFrequencyHzAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(14_070_000L);

        public Task SetFrequencyHzAsync(
            long frequencyHz, CancellationToken cancellationToken = default)
        {
            FrequencyChanged?.Invoke(this, new FrequencyChangedEventArgs(frequencyHz));

            return Task.CompletedTask;
        }

        /// <summary>This one exists to count reads, so it never keys.</summary>
        public Task<bool> SendCwAsync(
            string message, CancellationToken cancellationToken = default)
            => Task.FromResult(false);

        /// <summary>Nothing is ever keyed here, so there is nothing to stop.</summary>
        public void AbortCw()
        {
        }

        /// <summary>This one exists to count reads, so it writes nothing.</summary>
        public Task<RigWriteResult> SetSettingAsync(
            CivWrite write, int value, CancellationToken cancellationToken = default)
            => Task.FromResult(RigWriteResult.NotSupported("recording radio"));

        /// <summary>This one exists to count reads, so it writes nothing.</summary>
        public Task<RigWriteResult> SetModeAsync(
            CivMode mode,
            bool dataMode,
            byte? filterSlot = null,
            CancellationToken cancellationToken = default)
            => Task.FromResult(RigWriteResult.NotSupported("recording radio"));

        public Task<IReadOnlyList<RigValue>> ReadAsync(
            RigField field, RigState context, CancellationToken cancellationToken = default)
        {
            lock (_gate)
            {
                _asked[field] = _asked.GetValueOrDefault(field) + 1;
            }

            IReadOnlyList<RigValue> values = new[]
            {
                RigValue.Known(field, 1, "scripted", DateTime.UtcNow, "test"),
            };

            ValuesReported?.Invoke(this, new RigValuesReportedEventArgs(Array.Empty<RigValue>()));

            return Task.FromResult(values);
        }

        public void Dispose()
        {
        }
    }
}
