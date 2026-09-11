using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text.Json;
using Hamlet.RadioEngine.Psk31;

namespace Hamlet.RadioEngine.Tests.Psk31;

/// <summary>
/// `assets/fixtures/psk31-transcripts/corpus.json`, read exactly as written.
/// </summary>
/// <remarks>
/// <para>**ONE READER FOR THE TWO TEST CLASSES THAT USE IT**, so the parser's test and the
/// splitter's test cannot come to read the same file two ways.</para>
/// <para>**NOTHING IS CORRECTED ON THE WAY IN.** The corpus disagrees with itself on
/// `05-garbled` - `unknown_rate_expected` 0.2 against two of five lines at `certain: false` -
/// and both numbers are exposed as they stand for the test to print side by side.</para>
/// <para>**WRITTEN, NOT RECORDED.** Every line was typed by the web thread.</para>
/// </remarks>
internal sealed class Psk31Corpus
{
    /// <summary>What the corpus writes where the text does not say.</summary>
    public const string Unknown = "UNKNOWN";

    private Psk31Corpus(string operatorCall, IReadOnlyList<Psk31CorpusTranscript> transcripts, string sha256)
    {
        Operator = operatorCall;
        Transcripts = transcripts;
        Sha256 = sha256;
    }

    /// <summary>The operator's callsign, from the corpus's own `operator` field.</summary>
    public string Operator { get; }

    /// <summary>The transcripts, in file order.</summary>
    public IReadOnlyList<Psk31CorpusTranscript> Transcripts { get; }

    /// <summary>Every line of every transcript, in file order.</summary>
    public IReadOnlyList<Psk31CorpusLine> Lines => Transcripts.SelectMany(t => t.Lines).ToList();

    /// <summary>The file's SHA-256, lower-case hex. Recorded, not checked - there is no manifest.</summary>
    public string Sha256 { get; }

    /// <summary>Read the file.</summary>
    public static Psk31Corpus Load()
    {
        var path = Path.Combine(Root(), "assets", "fixtures", "psk31-transcripts", "corpus.json");
        var bytes = File.ReadAllBytes(path);

        using var document = JsonDocument.Parse(bytes);

        var root = document.RootElement;
        var transcripts = new List<Psk31CorpusTranscript>();

        foreach (var transcript in root.GetProperty("transcripts").EnumerateArray())
        {
            var name = transcript.GetProperty("name").GetString() ?? "";
            var texts = transcript.GetProperty("lines").EnumerateArray().Select(l => l.GetString() ?? "").ToList();
            var expected = transcript.GetProperty("expected").EnumerateArray().ToList();
            var lines = new List<Psk31CorpusLine>();

            for (var i = 0; i < texts.Count; i++)
            {
                var e = expected[i];

                lines.Add(new Psk31CorpusLine(
                    name,
                    i + 1,
                    texts[i],
                    e.GetProperty("frm").GetString() ?? "",
                    e.GetProperty("to").GetString() ?? "",
                    e.GetProperty("kind").GetString() ?? "",
                    e.GetProperty("turnover").GetBoolean(),
                    e.GetProperty("certain").GetBoolean(),
                    e.TryGetProperty("rst", out var rst) ? rst.GetString() : null,
                    e.TryGetProperty("grid", out var grid) ? grid.GetString() : null));
            }

            transcripts.Add(new Psk31CorpusTranscript(
                name, lines, transcript.GetProperty("unknown_rate_expected").GetDouble()));
        }

        return new Psk31Corpus(
            root.GetProperty("operator").GetString() ?? "",
            transcripts,
            Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant());
    }

    /// <summary>The repository root, found by walking up to `Hamlet.sln`.</summary>
    public static string Root()
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

/// <summary>One transcript of the corpus.</summary>
internal sealed record Psk31CorpusTranscript(
    string Name, IReadOnlyList<Psk31CorpusLine> Lines, double UnknownRateExpected);

/// <summary>One line of the corpus and what a correct parser must conclude from it.</summary>
internal sealed record Psk31CorpusLine(
    string Transcript, int Number, string Text,
    string Frm, string To, string Kind, bool Turnover, bool Certain,
    string? Rst, string? Grid)
{
    /// <summary>The speaker, or null where the corpus says UNKNOWN.</summary>
    public string? Speaker => Frm == Psk31Corpus.Unknown ? null : Frm;

    /// <summary>The addressee, or null where the corpus says UNKNOWN.</summary>
    public string? Addressee => To == Psk31Corpus.Unknown ? null : To;

    /// <summary>The kind, as the parser's enum.</summary>
    public Psk31LineKind ExpectedKind => Enum.Parse<Psk31LineKind>(Kind, ignoreCase: true);

    /// <summary>Where the line is, for a message.</summary>
    public string Where => Transcript + " line " + Number;
}
