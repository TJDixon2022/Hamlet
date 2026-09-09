using Ft8Sharp.Encode;
using Ft8Sharp.Message;
using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Contacts;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Audio;

/// <summary>
/// **Work instruction 294 task 5 — the ratio reaches the row, and the menu offers
/// five shapes on FT4.**
/// </summary>
/// <remarks>
/// <para>**WHAT THIS CLOSES.** Step 4's criterion 2 names seven surfaces and asks
/// that they work on FT4 as they do on FT8. Two did not: the ledger, closed by task
/// 2, and the right-click menu — which offered three shapes on FT4 against five on
/// FT8, and the two missing ones were `report` and `roger and report`, **because no
/// FT4 row had a measured ratio**. That reaches past this step: step 6 is *he
/// answers a CQ on FT4 and completes an exchange*, a conventional exchange carries a
/// signal report each way, and until now Hamlet could not offer one at all.</para>
/// <para>**THE FIGURE IS GATED AND THE GATE WAS OPENED BY MEASUREMENT.**
/// `Ft4Unit294SnrAgreementTests` measured the estimator at five rungs FT4 decodes
/// at: **0.58 dB mean absolute error and 1.41 dB at the 95th percentile over 970
/// messages**, against a 2 dB threshold written down before the run. Had it not
/// cleared, this file would not exist and the menu would still offer three.</para>
/// <para>**A ROW WITH NO MEASUREMENT STILL OFFERS THREE AND SAYS WHY.** Null means
/// not observed. A message whose symbols could not be packed back out of its own
/// text gets no figure, and a row in that position behaves exactly as an FT8 row in
/// the same position does — the report-bearing messages absent with the reason said
/// out loud, rather than a number being invented. **A report goes on the air and
/// into another operator's log**, which is why the null case is asserted here and
/// not assumed.</para>
/// <para>**THE BREAKAGE THESE CATCH:** the menu offering a report shape whose text
/// carries a number the row never showed — the two disagreeing, with the operator
/// reading one and the band receiving the other.</para>
/// <para>**NOTHING HERE REACHES A TRANSMITTER** (`CLAUDE.md` §0.2) and no capture
/// device is opened. The audio is synthesized by the port's own FT4 waveform.</para>
/// </remarks>
public sealed class Unit294AnFt4RowCarriesAMeasuredReportTests(ITestOutputHelper output)
{
    /// <summary>What a sound card delivers.</summary>
    private const int DeviceRate = 12_000;

    /// <summary>Where the synthesized transmission is put, in the passband.</summary>
    private const float PlacedAtHz = 1240.0f;

    /// <summary>
    /// The moment the recording ended, by a clock measured at no drift. The recording
    /// runs from 14:22:30 to 14:22:45, and FT4's boundaries fall every 7.5 s, so two
    /// whole FT4 slots fit.
    /// </summary>
    private static readonly DateTime EndedAt =
        new(2026, 9, 2, 14, 22, 45, DateTimeKind.Utc);

    private static ClockOffset Measured =>
        new(0, new DateTime(2026, 9, 2, 14, 20, 0, DateTimeKind.Utc));

    /// <summary>The message the synthesized station sends.</summary>
    private const string Text = "CQ W9GAP EM12";

    /// <summary>
    /// **AN FT4 ROW CARRIES A MEASURED RATIO, END TO END THROUGH THE READER.**
    /// </summary>
    /// <remarks>
    /// The whole path the operator's press runs: audio in, <c>Ft8Reader.Read</c> with
    /// <c>DigitalMode.Ft4</c>, out to <c>Ft8Decode.SignalToNoiseDb</c>. Before this
    /// unit every FT4 decode carried null here and the slot census carried
    /// <c>Ft8SlotSnrs.None</c>. **The delivered ratio is commanded, so the figure is
    /// checked against a known answer rather than against itself.**
    /// </remarks>
    [Fact]
    public void AnFt4DecodeCarriesARatioAndTheSlotCarriesItsSpread()
    {
        const double commanded = -8.0;
        var heard = ReadOneFt4Slot(commanded, out var delivered);

        var decode = Assert.Single(heard.Decodes, d => d.Message == Text);
        var slot = Assert.Single(heard.Slots, s => s.SignalToNoise.Measured > 0);

        output.WriteLine("AN FT4 SLOT THROUGH Ft8Reader.Read, mode Ft4");
        output.WriteLine($"  message              \"{decode.Message}\"");
        output.WriteLine($"  delivered            {delivered:F2} dB in 2500 Hz");
        output.WriteLine($"  SignalToNoiseDb      {decode.SignalToNoiseDb:F2} dB");
        output.WriteLine($"  the snr cell         {FormatLikeTheRow(decode.SignalToNoiseDb)}");
        output.WriteLine($"  the slot's spread    {slot.SignalToNoise}");
        output.WriteLine(string.Empty);
        output.WriteLine("  before unit 294 the cell was the dash and the spread was");
        output.WriteLine("  Ft8SlotSnrs.None, on every FT4 row without exception.");

        // A NUMBER, NOT A NULL, and not a NaN dressed as one.
        Assert.NotNull(decode.SignalToNoiseDb);
        Assert.False(double.IsNaN(decode.SignalToNoiseDb!.Value));

        // AND IT IS THE RIGHT NUMBER. The bound is the ladder's own measured 95th
        // percentile, 1.41 dB, rounded up to 1.5 - not a tolerance chosen to make one
        // trial pass, and looser than the mean error by more than a factor of two.
        Assert.True(
            Math.Abs(decode.SignalToNoiseDb.Value - delivered) <= 1.5,
            $"the row says {decode.SignalToNoiseDb.Value:F2} dB and {delivered:F2} dB was "
            + "delivered, which is outside the ladder's measured 95th percentile.");

        // The census carries the spread, which is what telemetry is handed.
        Assert.True(slot.SignalToNoise.IsMeasured);
        Assert.Equal(1, slot.SignalToNoise.Measured);
        Assert.NotNull(slot.SignalToNoise.WeakestDb);
        Assert.NotNull(slot.SignalToNoise.StrongestDb);
    }

    /// <summary>
    /// **THE RIGHT-CLICK MENU OFFERS FIVE SHAPES ON FT4, THE SAME FIVE AS ON FT8.**
    /// </summary>
    /// <remarks>
    /// **Both counts are asserted even though they are now equal**, which is the point:
    /// a later change that took a shape off one path would pass a test that only
    /// checked the other. The FT4 report is the one the reader above measured, put
    /// through the same whole-decibel rounding the `snr` cell uses, so <b>the menu
    /// cannot offer a number the row never showed</b>.
    /// </remarks>
    [Fact]
    public void TheFt4MenuOffersTheSameFiveShapesTheFt8MenuOffers()
    {
        const double commanded = -8.0;
        var heard = ReadOneFt4Slot(commanded, out _);
        var decode = Assert.Single(heard.Decodes, d => d.Message == Text);

        // THE ROW'S OWN CELL, AND THE MENU IS GIVEN WHAT THE ROW SHOWS. This is the
        // arithmetic MainWindowViewModel.MeasuredReport does on DigitalDecodeRow.Snr,
        // reproduced here because the engine cannot see the view model - and doing it
        // any other way would be the second copy that lets the two disagree.
        var report = (int)Math.Round(
            decode.SignalToNoiseDb!.Value, MidpointRounding.AwayFromZero);

        var ledger = new Ft8ContactLedger("KC3QIS");
        ledger.RecordHeard("KC3QIS W9GAP EM12", EndedAt);
        var record = ledger.For("W9GAP")!;

        var ft4 = Ft8SendOptions.For(record, "KC3QIS", "FN00", report);
        var ft8 = Ft8SendOptions.For(record, "KC3QIS", "FN00", -12);

        output.WriteLine("THE RIGHT-CLICK MENU AFTER UNIT 294");
        output.WriteLine($"  on an FT4 row   {ft4.Options.Count} shapes: "
            + string.Join(", ", ft4.Options.Select(o => o.Label)));
        output.WriteLine($"  on an FT8 row   {ft8.Options.Count} shapes: "
            + string.Join(", ", ft8.Options.Select(o => o.Label)));
        output.WriteLine(string.Empty);
        output.WriteLine("  the FT4 report shape carries: "
            + ft4.Options.Single(o => o.Shape == Ft8SendShape.Report).Text);
        output.WriteLine("  and the row's own snr cell says: "
            + FormatLikeTheRow(decode.SignalToNoiseDb));

        // BOTH COUNTS, EVEN THOUGH THEY ARE NOW EQUAL.
        Assert.Equal(5, ft4.Options.Count);
        Assert.Equal(5, ft8.Options.Count);

        Assert.Contains(ft4.Options, o => o.Shape == Ft8SendShape.Report);
        Assert.Contains(ft4.Options, o => o.Shape == Ft8SendShape.RogerAndReport);

        // THE SAME FIVE SHAPES IN THE SAME ORDER, which is the exchange's order.
        Assert.Equal(
            ft8.Options.Select(o => o.Shape).ToArray(),
            ft4.Options.Select(o => o.Shape).ToArray());

        // NOTHING IS ABSENT, so nothing has to be explained away.
        Assert.Empty(ft4.Absent);

        // THE MENU'S NUMBER IS THE ROW'S NUMBER. The report text carries the same
        // whole decibels the snr cell shows, so the operator and the band cannot be
        // reading different figures.
        Assert.Contains(
            Ft8SendOptions.Report(report),
            ft4.Options.Single(o => o.Shape == Ft8SendShape.Report).Text,
            StringComparison.Ordinal);
        Assert.Equal(
            FormatLikeTheRow(decode.SignalToNoiseDb),
            report.ToString("+0;-0;+0", System.Globalization.CultureInfo.InvariantCulture));
    }

    /// <summary>
    /// **A ROW WITH NO MEASUREMENT STILL OFFERS THREE, AND SAYS WHY.**
    /// </summary>
    /// <remarks>
    /// Null means not observed. A message whose symbols could not be recovered has no
    /// figure, and a row in that position must behave exactly as an FT8 row in the same
    /// position does. **This is the case that keeps the estimator honest**: without it
    /// a later unit could make the report unconditional by substituting a floor, and
    /// every test above would still pass.
    /// </remarks>
    [Fact]
    public void AnFt4RowWithNoMeasurementStillOffersThreeAndSaysWhy()
    {
        var ledger = new Ft8ContactLedger("KC3QIS");
        ledger.RecordHeard("KC3QIS W9GAP EM12", EndedAt);
        var record = ledger.For("W9GAP")!;

        var none = Ft8SendOptions.For(record, "KC3QIS", "FN00", reportDecibels: null);

        output.WriteLine("AN FT4 ROW WHOSE RATIO COULD NOT BE TAKEN");
        output.WriteLine($"  {none.Options.Count} shapes: "
            + string.Join(", ", none.Options.Select(o => o.Label)));
        foreach (var line in none.Absent)
        {
            output.WriteLine("  absent: " + line);
        }

        Assert.Equal(3, none.Options.Count);
        Assert.DoesNotContain(none.Options, o => o.Shape == Ft8SendShape.Report);
        Assert.DoesNotContain(none.Options, o => o.Shape == Ft8SendShape.RogerAndReport);

        // SAID OUT LOUD, not left blank.
        Assert.Contains(
            none.Absent,
            line => line.Contains("no signal report has been measured", StringComparison.Ordinal));

        // AND THE CELL IS THE DASH RATHER THAN A FLOOR.
        Assert.Equal("—", FormatLikeTheRow(null));
        Assert.Equal("—", FormatLikeTheRow(double.NaN));
    }

    /// <summary>
    /// **TWO NEW MENU ENTRIES ARE TWO NEW WAYS TO CLICK AND NOT A SECOND WAY TO
    /// TRANSMIT.**
    /// </summary>
    /// <remarks>
    /// The standing ruling is *one click, one transmission* — Hamlet transmits because
    /// the operator clicked, never on a timer, never on a decode, never to continue a
    /// contact. <see cref="Ft8SendOptions"/> composes text and reaches nothing: it has
    /// no field, no event and no way to arm anything, so putting two more shapes on the
    /// menu adds no route to a transmitter. **The one caller of `Ft8ArmedSend.Arm` in
    /// `src/` is `MainWindowViewModel.cs:10627` and this unit added none**, counted by
    /// grep over every `.cs` file under `src/` and reported.
    /// </remarks>
    [Fact]
    public void TheSendOptionsCannotReachATransmitterAtAll()
    {
        var type = typeof(Ft8SendOptions);

        output.WriteLine("EVERY PUBLIC MEMBER OF Ft8SendOptions");
        foreach (var member in type.GetMembers().Where(m => m.DeclaringType == type))
        {
            output.WriteLine("  " + member.Name);
        }

        // Nothing on it is an instance member at all - it is a static composer - so
        // there is no state for a transmission to be armed in.
        Assert.True(type.IsAbstract && type.IsSealed, "Ft8SendOptions is a static class");

        // And no member of it names anything that keys a radio.
        string[] forbidden = ["Arm", "Send", "Transmit", "Key", "Ptt", "Start"];

        foreach (var member in type.GetMembers().Where(m => m.DeclaringType == type))
        {
            Assert.DoesNotContain(
                forbidden,
                word => member.Name.Contains(word, StringComparison.OrdinalIgnoreCase));
        }
    }

    /// <summary>
    /// One FT4 slot of synthesized audio, read through the reader the tab runs.
    /// </summary>
    /// <param name="commandedDecibels">The ratio to deliver, in 2500 Hz.</param>
    /// <param name="delivered">What was actually delivered, measured from the noise drawn.</param>
    private static Ft8Reception ReadOneFt4Slot(double commandedDecibels, out double delivered)
    {
        var message = new byte[Ft8Payload.MessageBytes];
        Assert.Equal(
            Ft8PackResult.Ok,
            Ft8StandardMessage.TryPack("CQ", "W9GAP", "EM12", message));

        var symbols = Ft4SymbolEncoder.Encode(message);
        var signal = Ft4Waveform.Synthesize(symbols, DeviceRate, PlacedAtHz);

        // THE RECORDING IS FIFTEEN SECONDS ENDING ON AN FT4 BOUNDARY, so the cutter
        // finds two whole 7.5 s slots and the transmission is inside the second of
        // them, at the padding the waveform's own slot layout uses.
        var samples = new float[DeviceRate * 15];
        var lead = (DeviceRate * 15 / 2) + Ft4Waveform.PaddingSampleCount(DeviceRate);
        signal.CopyTo(samples.AsSpan(lead));

        // The noise is drawn once and measured, so the delivered ratio is a fact about
        // this buffer rather than about the sigma that was asked for.
        var signalPower = MeanSquare(signal);
        var sigma = NoiseAmplitudeFor(signalPower, commandedDecibels, DeviceRate);
        var random = new Random(294);
        var noisePower = 0.0;

        for (var i = 0; i < samples.Length; i++)
        {
            var draw = Gaussian(random) * sigma;
            noisePower += draw * draw;
            samples[i] = (float)(samples[i] + draw);
        }

        noisePower /= samples.Length;
        delivered = DecibelsFor(signalPower, noisePower, DeviceRate);

        return Ft8Reader.Read(
            new MonoAudio(DeviceRate, samples),
            EndedAt,
            Measured,
            mode: DigitalMode.Ft4);
    }

    /// <summary>What <c>DigitalDecodeRow.FormatSnr</c> does, which the engine cannot see.</summary>
    /// <remarks>
    /// **The view model's own arithmetic, reproduced rather than reached for**, because
    /// `Hamlet.RadioEngine` does not reference `Hamlet.App` and must not. The App tests
    /// hold the real one; what this checks is that the number reaching the row is one
    /// the cell can render as a signed whole decibel rather than a dash.
    /// </remarks>
    private static string FormatLikeTheRow(double? decibels)
    {
        if (decibels is not { } measured || double.IsNaN(measured) || double.IsInfinity(measured))
        {
            return "—";
        }

        return ((int)Math.Round(measured, MidpointRounding.AwayFromZero))
            .ToString("+0;-0;+0", System.Globalization.CultureInfo.InvariantCulture);
    }

    private static double MeanSquare(ReadOnlySpan<float> samples)
    {
        var sum = 0.0;
        foreach (var sample in samples)
        {
            sum += (double)sample * sample;
        }

        return sum / samples.Length;
    }

    /// <summary>The same arithmetic <c>SignalToNoise</c> in the port's tests does.</summary>
    private static double NoiseAmplitudeFor(double signalPower, double decibels, int sampleRate)
    {
        var noiseInReference = signalPower / Math.Pow(10.0, decibels / 10.0);
        return Math.Sqrt(noiseInReference * (sampleRate / 2.0) / 2500.0);
    }

    private static double DecibelsFor(double signalPower, double totalNoisePower, int sampleRate)
        => 10.0 * Math.Log10(
            signalPower / (totalNoisePower * 2500.0 / (sampleRate / 2.0)));

    /// <summary>One standard normal, Box-Muller in its polar form.</summary>
    private static double Gaussian(Random random)
    {
        double u, v, s;
        do
        {
            u = (random.NextDouble() * 2) - 1;
            v = (random.NextDouble() * 2) - 1;
            s = (u * u) + (v * v);
        }
        while (s >= 1.0 || s == 0.0);

        return u * Math.Sqrt(-2.0 * Math.Log(s) / s);
    }
}
