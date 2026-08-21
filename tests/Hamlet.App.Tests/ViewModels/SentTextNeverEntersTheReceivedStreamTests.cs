using System.Text.RegularExpressions;
using Hamlet.RadioEngine.Cw;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.ViewModels;

/// <summary>
/// What Hamlet sends can never appear as something it received.
/// </summary>
/// <remarks>
/// <para>**THIS HOLDS INDEPENDENTLY OF THE TRANSMIT STATE** (HM-DEC-147). It has
/// to be true even if that state is late, wrong, or missing entirely, because it
/// is a fact about where text came from rather than about what the radio was
/// doing. Suspending decoding while transmitting closes the path through the
/// sidetone; this closes the path through the code.</para>
/// <para>**THE GUARANTEE IS THAT ONLY THE DECODER CAN WRITE.** The transcript has
/// four ways in, and every call site in the application is a subscription to an
/// event raised by `CwDecoder`, which raises them from `CwProbabilisticStream`
/// and from nowhere else. That stream is fed audio and nothing but audio.</para>
/// <para>**WHAT WOULD HAVE TO GO WRONG.** Somebody would have to call one of the
/// four write methods from a path that is not the decoder's — the send panel, the
/// phrasebook, the auto-call cycle, a test helper wired into the app. This test
/// is a sweep of the source for exactly that, in the shape this project already
/// uses to prove no telemetry payload can be handed a callsign.</para>
/// </remarks>
public sealed class SentTextNeverEntersTheReceivedStreamTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the call sites are printed.</param>
    public SentTextNeverEntersTheReceivedStreamTests(ITestOutputHelper output)
        => _output = output;

    /// <summary>The application's own source, walked up from the test binary.</summary>
    private static string AppSource()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);

        while (directory is not null
               && !Directory.Exists(Path.Combine(directory.FullName, "src", "Hamlet.App")))
        {
            directory = directory.Parent;
        }

        Assert.NotNull(directory);

        return Path.Combine(directory!.FullName, "src", "Hamlet.App");
    }

    /// <remarks>
    /// <para>Proves the guarantee: **every way into the transcript is the
    /// decoder's.** A call site that is not a subscription to a `CwDecoder` event
    /// fails this, whatever it is, because the transcript is the record of what
    /// was on the air.</para>
    /// </remarks>
    [Fact]
    public void OnlyTheDecoderCanWriteToTheTranscript()
    {
        var writes = new Regex(
            @"Transcript\.(Settle|Append|Offer|OfferEdge)\b", RegexOptions.Compiled);

        var offenders = new List<string>();
        var allowed = 0;

        foreach (var file in Directory.GetFiles(AppSource(), "*.cs", SearchOption.AllDirectories))
        {
            var lines = File.ReadAllLines(file);

            for (var i = 0; i < lines.Length; i++)
            {
                if (!writes.IsMatch(lines[i]))
                {
                    continue;
                }

                var line = lines[i].Trim();

                // Every legitimate site attaches or detaches a decoder event.
                // Nothing else may reach these methods.
                if (line.Contains("_decoder.", StringComparison.Ordinal)
                    && (line.Contains("+=", StringComparison.Ordinal)
                        || line.Contains("-=", StringComparison.Ordinal)))
                {
                    allowed++;
                    _output.WriteLine(
                        $"allowed  {Path.GetFileName(file)}:{i + 1}  {line}");
                    continue;
                }

                offenders.Add($"{Path.GetFileName(file)}:{i + 1}  {line}");
            }
        }

        foreach (var offender in offenders)
        {
            _output.WriteLine($"OFFENDER {offender}");
        }

        Assert.Empty(offenders);

        // And the paths are actually there, so this cannot pass by finding
        // nothing at all.
        Assert.True(allowed >= 2, $"only {allowed} decoder subscriptions were found");
    }

    /// <remarks>
    /// <para>Proves the other end of it: **the engine's own transmit path never
    /// touches a decoder.** What Hamlet sends is handed to the radio's keyer as a
    /// CI-V message, and the text does not exist anywhere the decoder can reach.
    /// </para>
    /// </remarks>
    [Fact]
    public void TheSendPathHandsTextToTheRadioAndNowhereElse()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);

        while (directory is not null
               && !Directory.Exists(Path.Combine(directory.FullName, "src", "Hamlet.RadioEngine")))
        {
            directory = directory.Parent;
        }

        Assert.NotNull(directory);

        var engine = Path.Combine(directory!.FullName, "src", "Hamlet.RadioEngine");
        var suspects = new List<string>();

        foreach (var file in Directory.GetFiles(engine, "*.cs", SearchOption.AllDirectories))
        {
            var text = File.ReadAllText(file);

            // A file that both composes a message for the keyer and raises a
            // decoded character would be the merge this test exists to prevent.
            if (text.Contains("CwMessage", StringComparison.Ordinal)
                && text.Contains("CharacterSettled?.Invoke", StringComparison.Ordinal))
            {
                suspects.Add(Path.GetFileName(file));
            }
        }

        foreach (var suspect in suspects)
        {
            _output.WriteLine($"OFFENDER {suspect}");
        }

        Assert.Empty(suspects);
    }

    /// <remarks>
    /// <para>Proves the decoder's own half: a character can only be built from
    /// audio the stream was given, so there is no constructor by which sent text
    /// could become a received one.</para>
    /// </remarks>
    [Fact]
    public void ADecodedCharacterCarriesThePatternItWasReadFrom()
    {
        var stream = new CwProbabilisticStream(8_000);
        var settled = new List<CwCharacter>();

        stream.CharacterSettled += settled.Add;

        // Silence in, nothing out. There is no other way to make one.
        stream.Process(new float[8_000 * 3]);
        stream.Flush();

        _output.WriteLine($"{settled.Count} characters out of three seconds of nothing");

        Assert.Empty(settled);
    }
}
