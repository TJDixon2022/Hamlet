using Ft8Sharp.Tests.TableGen;

namespace Ft8Sharp.Tests;

/// <summary>
/// Unit 289's measurement sink: the numbers a test measured, written where a person can read them.
/// </summary>
/// <remarks>
/// <para>
/// <b>Why a file and not only the test log.</b> HM-DEC-155 recorded that the console logs on this
/// machine are UTF-16 and that a reader treating them as UTF-8 reports nothing at all — four
/// consecutive reports carried no totals for exactly that reason. A measurement that cannot be read
/// back is a measurement nobody has, so every figure this unit reports is written to
/// <c>artifacts/unit289/</c> as well as to the test output. <c>.gitignore</c> already excludes
/// <c>artifacts/</c>, so nothing measured here can reach a commit by accident.
/// </para>
/// <para>
/// <b>It records and it never asserts.</b> Nothing in this type can make a test pass or fail. The
/// assertions live in the tests beside it, where a reader looking for what was actually required
/// will find them.
/// </para>
/// </remarks>
internal sealed class Unit289Report
{
    /// <summary>
    /// Paths this process has already started writing.
    /// </summary>
    /// <remarks>
    /// <b>xUnit builds a fresh instance of a test class per test method</b>, so several tests of one
    /// class each construct one of these against the same path. Truncating in the constructor would
    /// leave the file holding whichever method happened to run last, which is how a measurement
    /// quietly stops being a measurement. The file is emptied once per process and appended to
    /// thereafter.
    /// </remarks>
    private static readonly HashSet<string> Started = new(StringComparer.OrdinalIgnoreCase);

    private readonly string _path;
    private readonly Xunit.Abstractions.ITestOutputHelper? _output;

    public Unit289Report(string name, Xunit.Abstractions.ITestOutputHelper? output = null)
    {
        _output = output;
        var folder = Path.Combine(RepositoryTree.Root, "artifacts", "unit289");
        Directory.CreateDirectory(folder);
        _path = Path.Combine(folder, name + ".txt");

        lock (Started)
        {
            if (Started.Add(_path))
            {
                File.WriteAllText(_path, string.Empty);
            }
        }
    }

    /// <summary>Both channels at once — the test log, and a file that can be read afterwards.</summary>
    /// <remarks>
    /// <b>Appended line by line rather than buffered</b>, so a run that throws part way through still
    /// leaves everything it had measured up to the throw.
    /// </remarks>
    public void WriteLine(string text = "")
    {
        _output?.WriteLine(text);
        lock (Started)
        {
            File.AppendAllText(_path, text + Environment.NewLine);
        }
    }
}
