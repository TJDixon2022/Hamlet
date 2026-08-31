using Xunit;
using Xunit.Abstractions;

namespace Ft8Sharp.Tests.Ldpc;

/// <summary>
/// Says where <see cref="Ft8Sharp.Ldpc.LdpcEncoder"/> came from, against the clone rather
/// than in a sentence.
/// </summary>
/// <remarks>
/// <para>
/// The phase plan calls this work a port, and a port's provenance is the most
/// licence-sensitive thing about it. <c>porting-notes.md</c> records that the generator
/// multiply was taken from <c>ft8/encode.c</c>, function <c>encode174</c>, at the pin; this
/// test is that same claim made checkable, so a reader does not have to take a notes file's
/// word for which file was read.
/// </para>
/// <para>
/// <b>Shapes only.</b> Presence, size and line count. Not one line of upstream source
/// reaches the transcript, and the tables live in a different file that this one never
/// opens.
/// </para>
/// <para>
/// Skips when the clone is absent, like every other test that needs reference material --
/// the clone is never committed and a fresh checkout must stay green without it.
/// </para>
/// </remarks>
public class UpstreamEncoderProvenanceTests
{
    private readonly ITestOutputHelper _output;

    public UpstreamEncoderProvenanceTests(ITestOutputHelper output) => _output = output;

    [RequiresReferenceCloneFact]
    public void TheEncoderWasPortedFromFt8EncodeDotCAtThePin()
    {
        var head = ReferenceClone.ResolveHead(ReferenceClone.Location, out var howRead);
        var encode = Path.Combine(ReferenceClone.Location, @"ft8\encode.c");

        _output.WriteLine($"clone      : {ReferenceClone.Location}");
        _output.WriteLine($"HEAD       : {(head.Length == 0 ? "(unreadable)" : head)}  (via {howRead})");
        _output.WriteLine($"pin        : {ReferenceClone.PinnedCommit}");
        _output.WriteLine($"ported from: ft8/encode.c, function encode174");
        _output.WriteLine(File.Exists(encode)
            ? $"encode.c   : present, {new FileInfo(encode).Length} bytes"
            : "encode.c   : ABSENT");

        Assert.True(
            string.Equals(head, ReferenceClone.PinnedCommit, StringComparison.OrdinalIgnoreCase),
            $"The clone is at '{(head.Length == 0 ? "(unreadable)" : head)}' and the pin is "
            + $"'{ReferenceClone.PinnedCommit}', so provenance cannot be recorded against it.");

        Assert.True(
            File.Exists(encode),
            $"{encode} is not there. LdpcEncoder claims to be a port of encode174 from that file, "
            + "and a provenance claim that cannot be checked against the pin is not a provenance "
            + "claim.");
    }
}
