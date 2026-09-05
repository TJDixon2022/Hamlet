using System.Globalization;
using Ft8Sharp.Tests.Dsp;

namespace Ft8Sharp.Tests.Fixtures;

/// <summary>
/// <b>Turns the decode lines WSJT-X prints into fixture rows. Strict, and loud about anything it
/// does not recognise.</b>
/// </summary>
/// <remarks>
/// <para>
/// <b>WHERE THE UNDERSTANDING OF THIS FORMAT CAME FROM, stated in full because a parser that guesses
/// is how a wrong number reaches a report.</b> One place in this tree documents the shape:
/// <see cref="ReferenceRecording"/>'s header quotes upstream <c>ft8_lib</c>'s own print format
/// verbatim as <c>"%02d%02d%02d %+05.1f %+4.2f %4.0f ~  %s\n"</c> - that is <c>HHMMSS</c>, SNR, dt,
/// frequency, a tilde, then the message. <c>ft8_lib</c> prints in that shape because it imitates
/// WSJT-X's own display, which is a published, observable output. <b>No WSJT-X source was read and
/// none may be</b>; nothing under <c>ft4_ft8_public/</c> was read.
/// </para>
/// <para>
/// <b>What follows from admitting that.</b> This is knowledge of one program's output transcribed
/// from a second program that imitates it. That is good enough to write a parser against and
/// <b>not</b> good enough to write a lenient one against. So: it accepts exactly that shape and
/// <b>refuses everything else by name, with the offending line verbatim</b>. It never skips a line it
/// does not understand, because a fixture short by an unknown number of rows is a fixture nothing
/// downstream can tell is wrong.
/// </para>
/// <para>
/// <b>Tim's first real run is what settles this.</b> If a line WSJT-X actually prints is refused
/// here, the refusal carries the line and the parser is corrected against a real sample - which is
/// the right way round, and is the only honest position available on a machine that has never seen
/// the program.
/// </para>
/// </remarks>
internal static class WsjtxDecodeLines
{
    /// <summary>The shape this parser accepts, in one line, for every message that has to quote it.</summary>
    internal const string Shape = "HHMMSS  snr  dt  freqHz  ~  message";

    /// <summary>
    /// <b>Every line, in order, as fixture rows.</b> Blank lines are skipped and nothing else is.
    /// </summary>
    /// <param name="text">Whatever the decoder wrote, whole.</param>
    /// <param name="source">Named in every refusal, so a message says which output it is about.</param>
    /// <exception cref="Ft8FixtureException">
    /// A line that is not blank and is not the accepted shape, or a field that is not a number, or a
    /// message that is empty.
    /// </exception>
    internal static IReadOnlyList<Ft8FixtureRow> Parse(string text, string source)
    {
        var rows = new List<Ft8FixtureRow>();
        var lines = text.Replace("\r\n", "\n").Split('\n');

        for (var i = 0; i < lines.Length; i++)
        {
            var line = lines[i].Trim();
            if (line.Length == 0)
            {
                continue;
            }

            rows.Add(ParseLine(line, i + 1, source));
        }

        return rows;
    }

    private static Ft8FixtureRow ParseLine(string line, int number, string source)
    {
        // Six fields, message last and unsplit because it carries spaces.
        var parts = line.Split((char[]?)null, 6, StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length < 6)
        {
            throw Refuse(
                source,
                number,
                line,
                $"it has {parts.Length} fields and the shape this parser accepts has six: {Shape}.");
        }

        if (parts[0].Length != 6 || !parts[0].All(char.IsAsciiDigit))
        {
            throw Refuse(
                source,
                number,
                line,
                $"its first field is \"{parts[0]}\" and the shape this parser accepts starts with a "
                + "six-digit HHMMSS.");
        }

        if (!string.Equals(parts[4], "~", StringComparison.Ordinal))
        {
            throw Refuse(
                source,
                number,
                line,
                $"its fifth field is \"{parts[4]}\" and the shape this parser accepts has a single "
                + "tilde there, separating the numbers from the message.");
        }

        var snr = Number(parts[1], "snr", source, number, line);
        var dt = Number(parts[2], "dt", source, number, line);
        var hz = Number(parts[3], "frequency", source, number, line);

        // The same normalisation ReferenceRecordings applies to upstream's own expected lists, called
        // and not re-implemented: trim, cut at the first run of two or more spaces, nothing else.
        var message = ReferenceRecording.Normalise(parts[5]);
        if (message.Length == 0)
        {
            throw Refuse(source, number, line, "its message is empty once normalised.");
        }

        return new Ft8FixtureRow(snr, dt, hz, message);
    }

    private static double Number(string token, string field, string source, int number, string line)
    {
        if (double.TryParse(token, NumberStyles.Float, CultureInfo.InvariantCulture, out var value))
        {
            return value;
        }

        throw Refuse(source, number, line, $"its {field} field reads \"{token}\" and is not a number.");
    }

    private static Ft8FixtureException Refuse(string source, int number, string line, string why) =>
        new(
            source,
            "(not yet known)",
            $"line {number} of the decoder's output is \"{line}\", and {why} THIS PARSER REFUSES "
            + "RATHER THAN SKIPS. A line dropped here would leave the fixture short by a row and "
            + "nothing downstream could tell. If WSJT-X really prints this line, send it back and the "
            + "parser is corrected against it - see WsjtxDecodeLines' remarks on where the shape came "
            + "from.");
}
