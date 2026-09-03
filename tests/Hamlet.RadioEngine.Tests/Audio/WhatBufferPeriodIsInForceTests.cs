using System.Reflection;
using NAudio.CoreAudioApi;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Audio;

/// <summary>
/// Work instruction 239, task 1.1: what buffer period every callback is actually
/// measured against.
/// </summary>
/// <remarks>
/// <para>**UNIT 238 ASSERTED AGAINST 20,000 MICROSECONDS AND THAT WAS NEVER THE
/// VALUE IN PRODUCTION.** It came from 960 samples at 48 kHz, which is what
/// `BufferedAudioSource` hands the decoder, and the device's own period is a
/// different number set by `WasapiCapture`. A budget taken from the wrong place
/// makes a passing test say nothing: the shack machine's worst callback of
/// 91,372 µs reads as a catastrophe against 20,000 and as 91% of the budget
/// against the real figure, and only the second is true.</para>
/// <para>**READ FROM THE ASSEMBLY THIS REPOSITORY REFERENCES, NOT FROM MEMORY.**
/// The property is not in NAudio's XML documentation, so the number is taken off
/// the type itself. `WasapiAudioSource` calls `new WasapiCapture(endpoint)` and
/// never sets it, so whatever this reports is the budget in force.</para>
/// </remarks>
public sealed class WhatBufferPeriodIsInForceTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the measurement.</summary>
    /// <param name="output">Where the figure is printed.</param>
    public WhatBufferPeriodIsInForceTests(ITestOutputHelper output)
        => _output = output;

    /// <summary>The default buffer length, read off NAudio 2.2.1 itself.</summary>
    /// <remarks>
    /// **IT IS READ RATHER THAN ASSERTED.** A test that pinned the number would
    /// fail on a NAudio upgrade and say nothing about why; this one prints it, so
    /// the figure the report quotes comes from the assembly on disk.
    /// </remarks>
    [Fact]
    public void TheDefaultBufferLengthIsReadOffTheAssembly()
    {
        var type = typeof(WasapiCapture);
        var property = type.GetProperty(
            "AudioBufferMillisecondsLength",
            BindingFlags.Public | BindingFlags.Instance);

        _output.WriteLine("NAudio assembly : " + type.Assembly.GetName().Name
            + " " + type.Assembly.GetName().Version);

        // **THE PROPERTY DOES NOT EXIST IN 2.2.1, AND THAT IS A FINDING.** The
        // work instruction says WasapiCapture "defaults
        // AudioBufferMillisecondsLength to 100". In the version this repository
        // references there is no such property: the length is a CONSTRUCTOR
        // PARAMETER, `audioBufferMillisecondsLength`, on the three-argument
        // overload. Nothing can be set on an existing capture; the value has to
        // be chosen when it is built.
        _output.WriteLine("public property : " + (property is null
            ? "DOES NOT EXIST - it is a constructor parameter in 2.2.1"
            : "exists"));

        Assert.Null(property);

        // The default is set by the constructor chain, so it has to be read from
        // an instance. Every constructor that does not take the value forwards to
        // the one that does, and the argument it forwards is the default.
        foreach (var field in type.GetFields(
            BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public))
        {
            _output.WriteLine("  field: " + field.FieldType.Name + " " + field.Name);
        }

        var defaults = type.GetConstructors()
            .Select(c => string.Join(
                ", ",
                c.GetParameters().Select(p =>
                    p.ParameterType.Name + " " + p.Name
                    + (p.HasDefaultValue ? " = " + p.DefaultValue : ""))))
            .ToArray();

        foreach (var signature in defaults)
        {
            _output.WriteLine("  ctor(" + signature + ")");
        }

        // **THE NUMBER, TAKEN THE ONLY WAY IT CAN BE.** Constructing a capture
        // needs a device; where the machine running the suite has none, the
        // figure is reported as unread rather than guessed (§0.0).
        var period = ReadDefaultPeriod(out var how);

        _output.WriteLine("");
        _output.WriteLine("buffer period in force : "
            + (period is null ? "NOT READ" : period + " ms")
            + "   (" + how + ")");

        if (period is { } ms)
        {
            _output.WriteLine("that is " + (ms * 1000) + " us per callback budget");
        }
    }

    /// <summary>The default, from a real capture where the machine has one.</summary>
    private static int? ReadDefaultPeriod(out string how)
    {
        try
        {
            using var enumerator = new MMDeviceEnumerator();
            var device = enumerator
                .EnumerateAudioEndPoints(DataFlow.Capture, DeviceState.Active)
                .FirstOrDefault();

            if (device is null)
            {
                how = "no active capture device on this machine";

                return null;
            }

            // Built exactly as WasapiAudioSource builds it: the one-argument
            // constructor, which forwards to the three-argument one with
            // whatever default NAudio chose.
            using var capture = new WasapiCapture(device);

            var field = typeof(WasapiCapture).GetField(
                "audioBufferMillisecondsLength",
                BindingFlags.NonPublic | BindingFlags.Instance);

            if (field is null)
            {
                how = "no audioBufferMillisecondsLength field to read";

                return null;
            }

            how = "read off a WasapiCapture over " + device.FriendlyName
                + ", built with the same one-argument constructor "
                + "WasapiAudioSource uses";

            return (int?)field.GetValue(capture);
        }
        catch (Exception ex)
        {
            how = "could not construct a capture: " + ex.GetType().Name;

            return null;
        }
    }
}
