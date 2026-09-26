using Hamlet.App.Settings;
using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Cw;
using Hamlet.RadioEngine.Training;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.Cw;

/// <summary>
/// HM-REQ-072: where one pattern has both a punctuation name and a prosign name,
/// the decoder emits one symbol and the terminal names it per its setting.
/// Verification row 072 (work instruction 455, task 2; PHASE_PLAN.md 6.2).
/// </summary>
/// <remarks>
/// <para>**THE TWO PATTERNS ARE `CW_SPEC.md` 6.2's**, `-...-` for `=` and `BT`
/// and `.-.-.` for `+` and `AR`, one symbol each with a naming choice and not two
/// entries. Each is sent as one run in `prosigns-18wpm`'s shape, 18 wpm over the
/// same quiet band, exact by construction.</para>
/// <para>**NAMING, NOT A CLAIM ABOUT THE SIGNAL.** The sound is the same whichever
/// name the sender had in mind, so the decoder's symbol is one and the terminal
/// chooses what to call it.</para>
/// </remarks>
public sealed class TheTwoNamedPatternIsNamedAsTheTerminalIsSetTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where each send's reading is printed.</param>
    public TheTwoNamedPatternIsNamedAsTheTerminalIsSetTests(ITestOutputHelper output)
        => _output = output;

    /// <summary>The two patterns, with their prosign name.</summary>
    public static TheoryData<string, string> Patterns { get; } = new()
    {
        { "-...-", "BT" },
        { ".-.-.", "AR" },
    };

    /// <summary>Each send of a two-named pattern settles as one character per time it was sent.</summary>
    /// <param name="pattern">The pattern.</param>
    /// <param name="prosign">Its prosign name, which is how it is sent.</param>
    [Theory]
    [MemberData(nameof(Patterns))]
    public void EachSendIsOneSymbol(string pattern, string prosign)
    {
        var settled = Settle(prosign);
        var letters = settled.Where(c => !c.IsWordGap).ToList();

        _output.WriteLine($"HM-REQ-072 | {pattern} | read `{string.Concat(settled.Select(c => c.Text))}`");

        // W1AW DE K2ABC, the pattern, R TU, the pattern: sixteen symbols, the
        // pattern at the twelfth and the sixteenth and nowhere split.
        Assert.Equal(16, letters.Count);
        Assert.Equal(pattern, letters[11].Pattern);
        Assert.Equal(pattern, letters[15].Pattern);
    }

    /// <summary>The terminal has a setting that chooses the name of the two-named patterns.</summary>
    [Fact]
    public void TheTerminalHasASettingThatNamesThem()
    {
        var setting = typeof(AppSettings).GetProperties()
            .SingleOrDefault(p => p.PropertyType.IsEnum && p.PropertyType.Name == "CwProsignNaming");

        _output.WriteLine($"HM-REQ-072 | setting | {setting?.Name ?? "none"}");

        Assert.True(
            setting is not null,
            "HM-REQ-072 not met: no terminal setting names -...- and .-.-.; the terminal shows <BT> and <AR> and nothing else");
    }

    private static IReadOnlyList<CwCharacter> Settle(string prosign)
    {
        var audio = CwSignal.Generate(new CwSignalRequest(
            $"W1AW DE K2ABC ^{prosign} R TU ^{prosign}", WordsPerMinute: 18, NoiseAmplitude: 0.02));
        var decoder = new CwDecoder(audio.SampleRate, CwSignal.DefaultToneHz);
        var settled = new List<CwCharacter>();

        decoder.CharacterSettled += settled.Add;

        using var source = new BufferedAudioSource(audio);
        decoder.Listen(source);
        source.PumpAll();
        decoder.Flush();

        return settled;
    }
}
