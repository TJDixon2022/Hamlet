namespace Hamlet.RadioEngine.Civ;

/// <summary>
/// One parsed CI-V frame: FE FE [to] [from] [cmd] [data...] FD (p. 19-2).
/// </summary>
/// <param name="To">Destination address.</param>
/// <param name="From">Source address.</param>
/// <param name="Command">Command byte (first byte after the addresses).</param>
/// <param name="Data">Everything between the command byte and the terminator.
/// May be empty.</param>
public sealed record CivFrame(byte To, byte From, byte Command, byte[] Data)
{
    /// <summary>Serialize this frame to wire bytes, preamble through terminator.</summary>
    public byte[] ToWireBytes()
    {
        var wire = new byte[5 + Data.Length + 1];
        wire[0] = CivConstants.Preamble;
        wire[1] = CivConstants.Preamble;
        wire[2] = To;
        wire[3] = From;
        wire[4] = Command;
        Data.CopyTo(wire, 5);
        wire[^1] = CivConstants.EndOfMessage;
        return wire;
    }

    /// <summary>Hex dump for the CI-V log — §0.0.1 requires wire traffic to be
    /// recordable verbatim.</summary>
    public override string ToString()
        => $"to=0x{To:X2} from=0x{From:X2} cmd=0x{Command:X2} data=[{Convert.ToHexString(Data)}]";
}
