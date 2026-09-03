using System.Text;
using Xunit;
using Xunit.Abstractions;

namespace Ft8Sharp.Tests.Encode;

/// <summary>
/// <b>Unit 227 task 1: the ground, measured rather than assumed.</b>
/// </summary>
/// <remarks>
/// <para>
/// <b>Why this is a test and not a shell command.</b> Every session of this phase has been refused
/// permission to list <c>C:\Source\ft8_lib</c> from a shell, and unit 227's was refused again. The
/// sanctioned route to the pin has been a test process since unit 210 — <see cref="ReferenceClone"/>
/// and <see cref="Ft8Oracle"/> both read it at run time and both skip when it is absent. Reading a
/// file is not the thing unit 209 refused; <b>that was routing a compiler through a test process to
/// dodge a refused shell command, and nothing here compiles anything.</b>
/// </para>
/// <para>
/// <b>Nothing here asserts a result.</b> These report. A machine without the clone skips them and a
/// machine with the clone prints what it found, because the whole point of task 1 is to find out
/// whether the instruction's premises are true rather than to confirm them.
/// </para>
/// </remarks>
public class Unit227GroundTests(ITestOutputHelper output)
{
    private readonly ITestOutputHelper _output = output;

    /// <summary>
    /// <b>Question 1.</b> Do the two oracle binaries exist, and if so how big are they and when were
    /// they last written?
    /// </summary>
    [RequiresReferenceCloneFact]
    public void WhatIsBuiltInThePin()
    {
        _output.WriteLine("UNIT 227 TASK 1b QUESTION 1 — what is built in the pin");
        _output.WriteLine($"  clone : {ReferenceClone.Location}");
        _output.WriteLine(string.Empty);

        Report("generator", Ft8Oracle.ExecutablePath);
        Report("decoder", Ft8Decoder.ExecutablePath);

        void Report(string what, string path)
        {
            if (!File.Exists(path))
            {
                _output.WriteLine($"  {what,-10}: ABSENT at {path}");
                return;
            }

            var info = new FileInfo(path);
            _output.WriteLine(
                $"  {what,-10}: {info.Length} bytes, last written "
                + $"{info.LastWriteTime:yyyy-MM-dd HH:mm:ss} local — {path}");
        }
    }

    /// <summary>
    /// <b>Question 3.</b> What does upstream's decoder expect of a WAV, and what does it search?
    /// </summary>
    /// <remarks>
    /// <b>The lines are quoted from the pin and the pin is not modified.</b> They are read, printed
    /// into a test log that is never committed, and dropped. What goes into the report is the
    /// numbers — a sample rate, a search band — and not upstream's source.
    /// </remarks>
    [RequiresReferenceCloneFact]
    public void WhatUpstreamsDecoderExpectsOfAWav()
    {
        _output.WriteLine("UNIT 227 TASK 1b QUESTION 3 — what upstream's decoder expects and searches");
        _output.WriteLine(string.Empty);

        Quote(
            Path.Combine(ReferenceClone.Location, "demo", "decode_ft8.c"),
            "sample_rate",
            "num_samples",
            "min_freq",
            "max_freq",
            "f_min",
            "f_max",
            "printf",
            "kFreq_osr",
            "kTime_osr",
            "kMax_decoded");

        Quote(
            Path.Combine(ReferenceClone.Location, "demo", "decode_ft8.c"),
            "slot_period",
            "is_ft8");

        // save_wav and load_wav in full: what upstream writes and what it refuses is fifty lines,
        // and reading them by needle would be reading the answer the needles were chosen for.
        // The write half matters as much as the read half, because unit 227's harness has to
        // quantise a float slot the way upstream's own generator does or the two decoders would
        // be reading a file neither of them would have written.
        Span(Path.Combine(ReferenceClone.Location, "common", "wave.c"), 10, 68);
        Span(Path.Combine(ReferenceClone.Location, "common", "wave.c"), 69, 133);

        void Quote(string path, params string[] needles)
        {
            _output.WriteLine($"  --- {path} ---");
            if (!File.Exists(path))
            {
                _output.WriteLine("      ABSENT");
                _output.WriteLine(string.Empty);
                return;
            }

            var lines = File.ReadAllLines(path);
            _output.WriteLine($"      {lines.Length} lines");
            for (var i = 0; i < lines.Length; i++)
            {
                if (needles.Any(n => lines[i].Contains(n, StringComparison.Ordinal)))
                {
                    _output.WriteLine($"      {i + 1,5}: {lines[i].TrimEnd()}");
                }
            }

            _output.WriteLine(string.Empty);
        }

        void Span(string path, int firstLine, int lastLine)
        {
            _output.WriteLine($"  --- {path} lines {firstLine}-{lastLine} ---");
            if (!File.Exists(path))
            {
                _output.WriteLine("      ABSENT");
                _output.WriteLine(string.Empty);
                return;
            }

            var lines = File.ReadAllLines(path);
            for (var i = firstLine - 1; i < Math.Min(lastLine, lines.Length); i++)
            {
                _output.WriteLine($"      {i + 1,5}: {lines[i].TrimEnd()}");
            }

            _output.WriteLine(string.Empty);
        }
    }
}
