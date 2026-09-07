using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Transmit;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Audio;

/// <summary>
/// What rate every render endpoint on this machine declares, and whether FT8 can
/// be built at it.
/// </summary>
/// <remarks>
/// <para>**IT IS THE TRACE, NOT THE VERDICT** (work instruction 262, task 1
/// question 5). The shell on this machine refused both spellings of the
/// enumeration script, and the instruction's own fallback is a filtered test that
/// prints the same table. That is all this is: an enumeration and two calls into
/// <see cref="Ft8Composer"/>, printed.</para>
/// <para>**IT OPENS NOTHING AND PLAYS NOTHING.**
/// <see cref="WasapiTransmitSink.Endpoints()"/> reads the mix format each active
/// render endpoint declares; no client is initialised, no buffer is written and
/// no sound is made. It is the same enumeration the settings page performs every
/// time it is opened.</para>
/// <para>**AND IT SAYS NOTHING ABOUT THE RADIO** (`SHACK_FACTS.md` FACT-004). No
/// radio has ever been attached to this machine, so nothing printed here is the
/// IC-7300's USB codec and nothing about that codec can be inferred from it. What
/// this measures is this computer.</para>
/// <para>**IT ASSERTS A CONTRACT AND NOT A MACHINE.** A machine with no render
/// endpoint at all is a normal machine and the assertion holds vacuously there;
/// what is asserted is that <see cref="Ft8Composer.RateIsUsable"/> answers in
/// words either way, so an operator whose sound card cannot carry FT8 can be told
/// why rather than left with a silent refusal.</para>
/// </remarks>
public sealed class WhatThisMachinesRenderEndpointsDeclareTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the table is printed.</param>
    public WhatThisMachinesRenderEndpointsDeclareTests(ITestOutputHelper output)
        => _output = output;

    /// <summary>
    /// Every active render endpoint, its declared rate, and whether FT8 can be
    /// composed at that rate.
    /// </summary>
    [Fact]
    public void EveryRenderEndpointsRateIsAnsweredInWordsEitherWay()
    {
        var endpoints = WasapiTransmitSink.Endpoints();

        _output.WriteLine($"COUNT: {endpoints.Count} active render endpoints on this machine");

        if (endpoints.Count == 0)
        {
            _output.WriteLine(
                "This machine has no active render endpoint. That is a fact about this "
                + "machine and not a failure (unit 256).");
        }

        foreach (var endpoint in endpoints)
        {
            _output.WriteLine(string.Empty);
            _output.WriteLine($"ID:       {endpoint.Id}");
            _output.WriteLine($"NAME:     {endpoint.Name}");
            _output.WriteLine($"DEFAULT:  {endpoint.IsDefault}");
            _output.WriteLine($"RATE:     {endpoint.SampleRate} Hz");
            _output.WriteLine($"CHANNELS: {endpoint.Channels}");
            _output.WriteLine($"BITS:     {endpoint.BitsPerSample}");
            _output.WriteLine($"ENCODING: {endpoint.Encoding}");

            var rateUsable = Ft8Composer.RateIsUsable(endpoint.SampleRate, out var rateWhy);
            _output.WriteLine($"RateIsUsable({endpoint.SampleRate}): {rateUsable}");
            if (!rateUsable)
            {
                _output.WriteLine($"  REFUSAL: {rateWhy}");
            }

            var baseUsable = Ft8Composer.BaseFrequencyIsUsable(
                Ft8Composer.DefaultBaseFrequencyHz, endpoint.SampleRate, out var baseWhy);
            _output.WriteLine(
                $"BaseFrequencyIsUsable({Ft8Composer.DefaultBaseFrequencyHz} Hz at "
                + $"{endpoint.SampleRate}): {baseUsable}");
            if (!baseUsable)
            {
                _output.WriteLine($"  REFUSAL: {baseWhy}");
            }

            // THE CONTRACT: whichever way it answers, it answers in words. A
            // refusal with an empty explanation is what strands an operator at
            // two in the morning.
            Assert.True(
                rateUsable ? rateWhy.Length == 0 : rateWhy.Length > 0,
                $"endpoint '{endpoint.Name}' at {endpoint.SampleRate} Hz was refused with no words");
        }
    }

    /// <summary>
    /// The rates ordinary hardware and the IC-7300's USB codec declare, against
    /// the rate the application composes at today.
    /// </summary>
    /// <remarks>
    /// **48000 IS HERE WHETHER OR NOT THIS MACHINE HAS SUCH AN ENDPOINT.** It is
    /// the shared-mode mix rate of ordinary hardware, and the send path has to
    /// work at it on a machine this repository will never run on.
    /// </remarks>
    [Fact]
    public void TheRatesOrdinaryHardwareDeclaresAreAllUsable()
    {
        foreach (var rate in new[] { 8000, 11025, 12000, 16000, 22050, 24000, 44100, 48000, 96000, 192000 })
        {
            var usable = Ft8Composer.RateIsUsable(rate, out var why);
            _output.WriteLine(
                $"{rate,7} Hz  RateIsUsable={usable}{(usable ? string.Empty : "  " + why)}");
        }

        Assert.True(Ft8Composer.RateIsUsable(48000, out _));
    }

    /// <summary>
    /// What the composer says about a rate at which FT8 cannot be built, in the
    /// words it says it in.
    /// </summary>
    /// <remarks>
    /// **NO ENDPOINT ON THIS MACHINE DECLARES ONE**, so the wording an operator
    /// would read is quoted from a constructed rate rather than from hardware
    /// that is not here. A channel symbol is 0.16 s, so a rate whose product with
    /// 0.16 is not a whole number is one the port refuses.
    /// </remarks>
    [Fact]
    public void ARateFt8CannotBeBuiltAtIsRefusedInWords()
    {
        foreach (var rate in new[] { 0, 8001, 37999 })
        {
            var usable = Ft8Composer.RateIsUsable(rate, out var why);
            _output.WriteLine($"{rate,7} Hz  RateIsUsable={usable}");
            _output.WriteLine($"          {why}");

            Assert.False(usable);
            Assert.NotEmpty(why);
        }
    }
}
