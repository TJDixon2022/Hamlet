using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Cw.Fixtures;

/// <summary>
/// The committed synthetic CQ calls are what their recipes build, and each key file
/// says what its recipe says (work instruction 414, task 2; PHASE_PLAN.md 1.2).
/// </summary>
/// <remarks>
/// <para>**AN EXACT KEY IS ONLY EXACT WHILE THE FILE IS THE ONE THE RECIPE MAKES.**
/// The key is known because the generator knows what it sent; a WAV that has
/// drifted from its recipe would carry a key for audio nobody generated.</para>
/// <para>The set is written by <see cref="WriteTheSetWhenAsked"/> with
/// `HAMLET_WRITE_SYNTHETIC_CQ=1` in the environment, and by nothing else.</para>
/// </remarks>
public sealed class TheSyntheticCqRebuildsTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the comparison is reported.</param>
    public TheSyntheticCqRebuildsTests(ITestOutputHelper output)
        => _output = output;

    /// <summary>Every case's name.</summary>
    public static TheoryData<string> Names
    {
        get
        {
            var data = new TheoryData<string>();

            foreach (var recipe in SyntheticCq.All)
            {
                data.Add(recipe.Name);
            }

            return data;
        }
    }

    /// <remarks>
    /// Proves 1.2: the WAV on disk is byte for byte what the recipe in its key file
    /// builds, and the key file is the one that recipe writes.
    /// </remarks>
    [Theory]
    [MemberData(nameof(Names))]
    public void TheCommittedCaseIsWhatItsRecipeBuilds(string name)
    {
        var recipe = SyntheticCq.All.Single(r => r.Name == name);
        var wav = Path.Combine(SyntheticCq.Folder, name + ".wav");
        var key = Path.Combine(SyntheticCq.Folder, name + ".key.md");

        Assert.True(File.Exists(wav), $"{name}.wav has not been written.");
        Assert.True(File.Exists(key), $"{name}.key.md has not been written.");

        var expected = SyntheticCq.Bytes(recipe);
        var committed = File.ReadAllBytes(wav);

        _output.WriteLine($"{name}: {committed.Length} bytes committed, {expected.Length} built");

        Assert.Equal(expected.Length, committed.Length);

        var first = Enumerable.Range(0, expected.Length).FirstOrDefault(i => expected[i] != committed[i], -1);

        Assert.True(first < 0, $"{name} differs from its recipe at byte {first}.");

        var keyText = File.ReadAllText(key).Replace("\r\n", "\n", StringComparison.Ordinal);

        Assert.Equal(SyntheticCq.KeyFile(recipe), keyText);
    }

    /// <remarks>Writes the set when asked to, and otherwise says it was not asked.</remarks>
    [Fact]
    public void WriteTheSetWhenAsked()
    {
        if (Environment.GetEnvironmentVariable("HAMLET_WRITE_SYNTHETIC_CQ") != "1")
        {
            _output.WriteLine("not asked: HAMLET_WRITE_SYNTHETIC_CQ is not 1, nothing written");
            return;
        }

        foreach (var recipe in SyntheticCq.All)
        {
            SyntheticCq.Write(recipe);

            var bytes = new FileInfo(Path.Combine(SyntheticCq.Folder, recipe.Name + ".wav")).Length;

            _output.WriteLine($"written | {recipe.Name} | {bytes} bytes");
        }
    }
}
