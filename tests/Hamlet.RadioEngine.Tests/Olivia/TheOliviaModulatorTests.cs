using System;
using System.Diagnostics;
using System.Text.Json;
using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Olivia;
using Hamlet.RadioEngine.Psk31;
using Hamlet.RadioEngine.Transmit;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Olivia;

/// <summary>
/// Work instruction 365: **Hamlet's own Olivia signal, read back by Hamlet's own ears** (step 4
/// criterion 4.1).
/// </summary>
/// <remarks>
/// <para>**THE DEMODULATOR IS NOT TOUCHED** (§10). A modulator that loops back only because the
/// demodulator was changed has proved nothing, so the reader here is the one the author's fixtures
/// proved.</para>
/// <para>**COMPUTED, NOT SEEN**, and nothing here is evidence about the radio (FACT-004).</para>
/// </remarks>
public sealed class TheOliviaModulatorTests
{
    /// <summary>The rate the demodulator reads at, and the author's fixtures were made at.</summary>
    private const int Rate = 8000;

    /// <summary>The author's fixture center (`manifest.json`), and one 500 Hz above it (decision AR).</summary>
    private static readonly double[] Centers = [1000, 1500];

    /// <summary>The typed line: at least 60 characters, framed as the app frames it (decision AR).</summary>
    private const string TypedLine = "Thanks for the call, running 100 watts to a dipole at 30 feet here in PA";

    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where each reading is printed.</param>
    public TheOliviaModulatorTests(ITestOutputHelper output) => _output = output;

    private static OliviaFormat Format
        => OliviaData.Current.Format ?? throw new InvalidOperationException(OliviaData.Current.Problem);

    /// <summary>The four PSK31 macros and one typed line, framed with the test fields (decision AR).</summary>
    internal static (string Name, string Text)[] Texts()
    {
        Assert.True(TypedLine.Length >= 60, $"the typed line is {TypedLine.Length} characters");

        return
        [
            .. Unit365Trace.Macros(),
            ("typed", Psk31Macros.Typed(Unit365Trace.His, Unit365Trace.Mine, TypedLine)),
        ];
    }

    /// <summary>
    /// **4.1, first half: each macro and a typed line, modulated by Hamlet at each variant and at
    /// two centers, decodes through Hamlet's demodulator to exactly the text that went in.**
    /// </summary>
    /// <param name="variant">The variant.</param>
    /// <remarks>
    /// **UNTIL TASK 3 COMPOSES THE BURST, THE VARIANT AND CENTER ARE THE CALL'S** (work instruction
    /// 365 task 1, noted as it says to). Task 3 returns this to decision AT's form.
    /// </remarks>
    [Theory]
    [InlineData("8/250")]
    [InlineData("16/500")]
    [InlineData("32/1000")]
    public void EachMacroAndATypedLineComeBackIdentical(string variant)
    {
        var identical = 0;
        var texts = Texts();

        foreach (var center in Centers)
        {
            foreach (var (name, text) in texts)
            {
                var samples = OliviaModulator.Modulate(text, variant, center, Rate, Ft8Composer.DefaultDrivePeak);
                var demodulator = new OliviaDemodulator(Format, Format.Variant(variant)!, center, Rate);
                var before = Process.GetCurrentProcess().TotalProcessorTime;
                var decoding = demodulator.Decode(new MonoAudio(Rate, samples), 0);
                var cpu = (Process.GetCurrentProcess().TotalProcessorTime - before).TotalSeconds;
                var same = string.Equals(Unify(decoding.Text), Unify(text), StringComparison.Ordinal);

                identical += same ? 1 : 0;

                _output.WriteLine(
                    $"{variant} at {center:0} Hz, {name}: {text.Length} characters in, {decoding.CharactersOut} out, "
                    + $"{(same ? "identical" : "DIFFERENT")}; {samples.Length / (double)Rate:0.000} s of audio, "
                    + $"blocks {decoding.BlocksDecoded} decoded, {decoding.BlocksRejected} rejected; decode cpu {cpu:0.000} s");

                if (!same)
                {
                    _output.WriteLine("  sent : " + JsonSerializer.Serialize(text));
                    _output.WriteLine("  read : " + JsonSerializer.Serialize(decoding.Text));
                }
            }
        }

        _output.WriteLine($"{variant}: {identical} of {texts.Length * Centers.Length} identical");

        Assert.Equal(texts.Length * Centers.Length, identical);
    }

    /// <summary>Decision J's line-ending unification: CR LF and a lone CR become LF.</summary>
    private static string Unify(string text)
        => text.Replace("\r\n", "\n", StringComparison.Ordinal).Replace('\r', '\n');
}
