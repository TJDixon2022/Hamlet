using System.Reflection;

namespace Ft8Sharp;

/// <summary>
/// A handle on this assembly, so that a caller can reach it without naming a
/// protocol type that does not exist yet.
/// </summary>
/// <remarks>
/// The boundary test needs some type inside Ft8Sharp to start from when it walks
/// <see cref="Assembly.GetReferencedAssemblies"/>. Nothing of the FT8 protocol is
/// built yet — no tables, no CRC, no LDPC, no DSP — so this is the whole of the
/// library for now, and it is deliberately the least interesting type that can do
/// the job.
/// </remarks>
public static class Ft8SharpAssembly
{
    /// <summary>The assembly Ft8Sharp compiles to.</summary>
    public static Assembly Assembly => typeof(Ft8SharpAssembly).Assembly;

    /// <summary>The library's name, as it is meant to be published.</summary>
    public const string Name = "Ft8Sharp";
}
