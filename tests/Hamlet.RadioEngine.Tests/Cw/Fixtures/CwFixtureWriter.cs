using Hamlet.RadioEngine.Audio;

namespace Hamlet.RadioEngine.Tests.Cw.Fixtures;

/// <summary>
/// Writes the rebuilt fixtures to disk (HM-OPEN-018 phase 1).
/// </summary>
/// <remarks>
/// **REGENERABLE, WHICH IS WHAT MAKES A FIXTURE ARGUABLE.** Nothing here is
/// recorded and nothing is hand-tuned, so anybody who thinks a fixture is wrong
/// can change its recipe and rebuild it rather than having to take it on trust.
/// The old off-air captures cannot be regenerated at all, which is exactly why
/// they are kept as evidence rather than used as the whole suite (HM-DEC-091).
/// </remarks>
public static class CwFixtureWriter
{
    /// <summary>Build every fixture and write it beside its sidecar.</summary>
    /// <param name="folder">Where to write, or null for the standard place.</param>
    /// <returns>What was written, in order.</returns>
    public static IReadOnlyList<string> WriteAll(string? folder = null)
    {
        var into = folder ?? CwFixtureCatalogue.Folder;

        Directory.CreateDirectory(into);

        var written = new List<string>();

        foreach (var recipe in CwFixtureCatalogue.All)
        {
            var (audio, sidecar) = CwFixtureGenerator.Generate(recipe);

            var wav = Path.Combine(into, recipe.Name + ".wav");
            var notes = Path.Combine(into, recipe.Name + ".txt");

            // **THE GATE'S VERDICT SURVIVES REGENERATION, AND IS BOUND TO THE
            // FILE IT JUDGED.** Rewriting the sidecar used to delete the reference
            // score outright, so running the generator silently disarmed the gate
            // and every fixture went back to being unjudged without anybody
            // saying so. Carrying it across is only safe because it carries the
            // byte count of the file it was measured on, which the commit test
            // checks: a regenerated fixture whose content changed invalidates its
            // own score loudly instead of keeping a verdict about a file that no
            // longer exists.
            var carried = File.Exists(notes)
                ? File.ReadAllLines(notes)
                    .Where(l => l.StartsWith("reference", StringComparison.Ordinal)
                        || l.StartsWith("scoredBytes", StringComparison.Ordinal))
                    .ToList()
                : new List<string>();

            WavAudio.Write(wav, audio);

            // Newline is fixed rather than the platform's, so a fixture built on
            // one machine is byte-identical to the same fixture built on another.
            var text = sidecar.Replace("\r\n", "\n", StringComparison.Ordinal);

            if (carried.Count > 0)
            {
                text = text.TrimEnd() + "\n" + string.Join("\n", carried) + "\n";
            }

            File.WriteAllText(notes, text);

            written.Add(recipe.Name);
        }

        return written;
    }
}
