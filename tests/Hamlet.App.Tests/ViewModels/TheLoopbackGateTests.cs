using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Hamlet.RadioEngine.Olivia;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.ViewModels;

/// <summary>
/// Work instruction 377 task 2: **R41's gate - a variant goes on the air only once Hamlet has read
/// it back off its own audio, and the gate stands in front of the one site where audio becomes a
/// transmission** (criterion 2.2, section 6 ruling 2).
/// </summary>
/// <remarks>
/// <para>**THE ENGINE KEEPS COMPOSING AND THE APP IS THE AIR.**
/// <c>OliviaModulator.Compose</c> is a library call and the loopback proof itself goes through it,
/// so gating the engine would make the proof impossible to write without a bypass - and a bypass
/// is the hole. **So the claim that there is exactly one place in the application where Olivia
/// audio is composed has to be asserted and not assumed**, which is what the first name here
/// does: a second call appearing anywhere under `src/Hamlet.App` fails it.</para>
/// <para>**COMPUTED, NOT SEEN**, and nothing here is evidence about the radio (FACT-004). This
/// type reads the tree; it opens no port, enumerates no device and keys nothing.</para>
/// </remarks>
public sealed class TheLoopbackGateTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the readings are printed.</param>
    public TheLoopbackGateTests(ITestOutputHelper output) => _output = output;

    /// <summary>
    /// **There is exactly one call to <c>OliviaModulator.Compose</c> under `src/Hamlet.App`, and
    /// the unproved-variant refusal stands in front of it in the same method.**
    /// </summary>
    [Fact]
    public void OneComposeCallInTheWholeApplicationAndTheGateStandsInFrontOfIt()
    {
        var root = RepoRoot();
        var app = Path.Combine(root, "src", "Hamlet.App");
        var calls = new List<(string File, int Line, string Text)>();
        var scanned = 0;

        foreach (var each in Directory.EnumerateFiles(app, "*.cs", SearchOption.AllDirectories))
        {
            if (each.Contains(Path.DirectorySeparatorChar + "bin" + Path.DirectorySeparatorChar, StringComparison.Ordinal)
                || each.Contains(Path.DirectorySeparatorChar + "obj" + Path.DirectorySeparatorChar, StringComparison.Ordinal))
            {
                continue;
            }

            scanned++;

            var lines = File.ReadAllLines(each);

            for (var i = 0; i < lines.Length; i++)
            {
                var trimmed = lines[i].TrimStart();

                // **A COMMENT IS NOT A CALL.** This type's own reason for existing is written in
                // comments beside the gate, and a comment naming the composer must not read as a
                // second composer.
                if (trimmed.StartsWith("//", StringComparison.Ordinal)
                    || trimmed.StartsWith("*", StringComparison.Ordinal))
                {
                    continue;
                }

                if (lines[i].Contains("OliviaModulator.Compose(", StringComparison.Ordinal))
                {
                    calls.Add((Path.GetRelativePath(root, each), i + 1, trimmed));
                }
            }
        }

        _output.WriteLine($"scanned {scanned} .cs files under src/Hamlet.App");

        foreach (var (file, line, text) in calls)
        {
            _output.WriteLine($"  {file}:{line}  {text}");
        }

        // **ONE SITE, AND IF A SECOND EVER APPEARS THIS IS WHERE IT IS CAUGHT** (section 6 ruling
        // 2 item 3). The gate below guards this call and only this call.
        var only = Assert.Single(calls);

        Assert.EndsWith("MainWindowViewModel.cs", only.File, StringComparison.Ordinal);

        // **AND THE GATE IS IN FRONT OF IT**, in the same file, asserted as arithmetic on the line
        // numbers rather than as a line number typed here, so ordinary edits above it do not
        // falsify it and moving the gate below the composer does.
        var body = File.ReadAllLines(Path.Combine(root, only.File));
        var refusal = LineOf(body, "\"variant_not_proved\"");
        var sentence = LineOf(body, "it has not proved to itself that it can read back Olivia");
        var announcement = LineOf(body, "\"no_announcement\"");

        _output.WriteLine($"  no_announcement refusal at line {announcement}");
        _output.WriteLine($"  variant_not_proved refusal at line {refusal}, its sentence at line {sentence}");
        _output.WriteLine($"  OliviaModulator.Compose at line {only.Line}");

        Assert.True(refusal > 0, "the variant_not_proved refusal is not in the send path");
        Assert.True(sentence > 0, "the unproved-variant sentence is not in the send path");
        Assert.True(refusal < only.Line, $"the gate is at line {refusal} and the composer at {only.Line}: the gate must come first");
        Assert.True(sentence < only.Line, $"the sentence is at line {sentence} and the composer at {only.Line}");
        Assert.True(announcement < refusal, "the no-announcement refusal reads the file's own problem and speaks first");
    }

    /// <summary>
    /// **The format table carries `proved_by_loopback` on every variant row, and the engine reads
    /// it; absent or false means not proved.**
    /// </summary>
    [Fact]
    public void EveryVariantRowSaysWhetherItIsProvedAndAbsentMeansNotProved()
    {
        var format = OliviaData.Current.Format;

        Assert.NotNull(format);

        foreach (var variant in format!.Variants)
        {
            _output.WriteLine($"  {variant.Name,-8} proved_by_loopback {variant.ProvedByLoopback}");
        }

        var proved = format.Variants.Where(v => v.ProvedByLoopback).Select(v => v.Name).ToArray();
        var not = format.Variants.Where(v => !v.ProvedByLoopback).Select(v => v.Name).ToArray();

        _output.WriteLine($"proved     : {(proved.Length == 0 ? "(none)" : string.Join(", ", proved))}");
        _output.WriteLine($"NOT proved : {(not.Length == 0 ? "(none)" : string.Join(", ", not))}");

        // **THE SEVEN THE MODE HAS**, so a row quietly lost is caught here too.
        Assert.Equal(7, format.Variants.Count);

        // **ABSENT MEANS NOT PROVED**, proved against the parse rather than trusted: a row with
        // the key taken out comes back unproved, and so does one whose value is not the literal
        // true. **A file Hamlet cannot read must not become a file that permits everything.**
        var text = File.ReadAllText(Path.Combine(RepoRoot(), "data", "olivia", "format.json"));

        foreach (var (what, edited) in new[]
        {
            ("the key removed", text.Replace(", \"proved_by_loopback\": true", "", StringComparison.Ordinal)),
            ("the value a string", text.Replace("\"proved_by_loopback\": true", "\"proved_by_loopback\": \"yes\"", StringComparison.Ordinal)),
            ("the value a number", text.Replace("\"proved_by_loopback\": true", "\"proved_by_loopback\": 1", StringComparison.Ordinal)),
        })
        {
            var read = OliviaFormat.Parse(edited);
            var stillProved = read.Variants.Where(v => v.ProvedByLoopback).Select(v => v.Name).ToArray();

            _output.WriteLine($"  with {what,-20}: proved {(stillProved.Length == 0 ? "(none)" : string.Join(", ", stillProved))}");

            Assert.Empty(stillProved);

            // **AND THE FILE IS STILL READ**, because an unreadable flag refuses the variant and
            // does not fail the format: every other row keeps its numbers.
            Assert.Equal(format.Variants.Count, read.Variants.Count);
        }
    }

    private static int LineOf(IReadOnlyList<string> lines, string needle)
    {
        for (var i = 0; i < lines.Count; i++)
        {
            var trimmed = lines[i].TrimStart();

            if (trimmed.StartsWith("//", StringComparison.Ordinal) || trimmed.StartsWith("*", StringComparison.Ordinal))
            {
                continue;
            }

            if (lines[i].Contains(needle, StringComparison.Ordinal))
            {
                return i + 1;
            }
        }

        return 0;
    }

    private static string RepoRoot()
    {
        var at = new DirectoryInfo(AppContext.BaseDirectory);

        while (at is not null && !File.Exists(Path.Combine(at.FullName, "Hamlet.sln")))
        {
            at = at.Parent;
        }

        return at?.FullName
            ?? throw new InvalidOperationException("no Hamlet.sln above the test binary");
    }
}
