using System.Globalization;
using Ft8Sharp.Tests.Encode;
using Ft8Sharp.Tests.Fixtures;

namespace Ft8FixtureMaker;

/// <summary>
/// <b>The command Tim runs at the shack. One capture in, one committed fixture out, no editing
/// afterwards.</b>
/// </summary>
/// <remarks>
/// <para>
/// <b>HALF OF THIS IS UNEXERCISED AND THAT IS SAID OUT LOUD RATHER THAN LEFT TO BE DISCOVERED.</b>
/// The hashing, the row parsing, the fixture writing and both loud refusals are unit-tested on the
/// development machine against committed decode text. <b>Starting WSJT-X's decoder and getting real
/// rows back is not, because there is no WSJT-X here and no unit may assume one.</b> Tim's first run
/// is what exercises that half. If it refuses a line WSJT-X really prints, the refusal carries the
/// line verbatim and the parser is corrected against a real sample.
/// </para>
/// <para>
/// <b>Nothing is substituted for WSJT-X.</b> Not <c>decode_ft8.exe</c>, which is <c>ft8_lib</c> and
/// is the thing this project measures itself against, and not Hamlet's own decoder, which would make
/// the fixture a measurement of Hamlet against Hamlet.
/// </para>
/// </remarks>
internal static class Program
{
    private static int Main(string[] args)
    {
        if (args.Length == 0 || args[0] is "-h" or "--help")
        {
            Usage();
            return args.Length == 0 ? 2 : 0;
        }

        try
        {
            return Make(args);
        }
        catch (Ft8FixtureException refused)
        {
            // The whole design is that this is loud and specific. Nothing is written, and the reason
            // names the fixture, the capture and what was wrong.
            Console.Error.WriteLine();
            Console.Error.WriteLine("REFUSED. No fixture was written.");
            Console.Error.WriteLine();
            Console.Error.WriteLine(refused.Message);
            return 1;
        }
    }

    private static int Make(string[] args)
    {
        var capture = Path.GetFullPath(args[0]);
        string? decoder = null;
        string? arguments = null;
        string? utc = null;

        for (var i = 1; i < args.Length - 1; i++)
        {
            switch (args[i])
            {
                case "--decoder":
                    decoder = args[++i];
                    break;
                case "--arguments":
                    arguments = args[++i];
                    break;
                case "--utc":
                    utc = args[++i];
                    break;
                default:
                    Console.Error.WriteLine($"Unknown option \"{args[i]}\".");
                    Usage();
                    return 2;
            }
        }

        if (!File.Exists(capture))
        {
            Console.Error.WriteLine($"REFUSED. There is no capture at {capture}.");
            return 1;
        }

        // The rate comes from the capture's own fmt chunk rather than from an assumption, because the
        // scorer refuses a rate the decode path was not built for and a guessed one would turn a
        // wrong file into a sensitivity result.
        var rate = WavFile.Read(capture).SampleRate;

        var when = utc ?? UtcFor(capture);
        Console.WriteLine($"capture   {capture}");
        Console.WriteLine($"rate      {rate} Hz");
        Console.WriteLine($"utc       {when}");

        var written = Ft8FixtureGenerator.Run(
            capture,
            when,
            rate,
            decoder,
            arguments,
            new Ft8FixtureGenerator.RealLookup(),
            Console.WriteLine);

        Console.WriteLine();
        Console.WriteLine($"WROTE     {written}");
        Console.WriteLine();
        Console.WriteLine("Commit the .wav and the .fixture.txt together. Nothing else is needed and");
        Console.WriteLine("nothing in either file is meant to be edited by hand.");
        return 0;
    }

    /// <summary>
    /// When the capture was taken, from its name if the name carries it and from the file's own
    /// timestamp otherwise.
    /// </summary>
    /// <remarks>
    /// <b>Always UTC.</b> The shack is UTC-04:00 and the CW manifest already records one evening's
    /// captures reading as two dates; a local time in this field would do the same thing to every FT8
    /// fixture. <c>--utc</c> overrides both, because Tim knows when he recorded it and this does not.
    /// </remarks>
    private static string UtcFor(string capture)
    {
        var stem = Path.GetFileNameWithoutExtension(capture);
        var parts = stem.Split('-');

        // <anything>-YYYY-MM-DD-HHMMSS, which is the shape CW's captures already use.
        if (parts.Length >= 4
            && DateTimeOffset.TryParseExact(
                string.Join('-', parts[^4..]),
                "yyyy-MM-dd-HHmmss",
                CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                out var named))
        {
            return named.ToString("yyyy-MM-dd'T'HH:mm:ss'Z'", CultureInfo.InvariantCulture);
        }

        return File.GetLastWriteTimeUtc(capture)
            .ToString("yyyy-MM-dd'T'HH:mm:ss'Z'", CultureInfo.InvariantCulture);
    }

    private static void Usage()
    {
        Console.WriteLine("Ft8FixtureMaker - one capture in, one committed fixture out.");
        Console.WriteLine();
        Console.WriteLine("  dotnet run --project tools/Ft8FixtureMaker -- <capture.wav> [options]");
        Console.WriteLine();
        Console.WriteLine("  --decoder <path>    WSJT-X's decoder. Otherwise "
            + $"{Ft8FixtureGenerator.DecoderVariable}, then the standard");
        Console.WriteLine("                      install locations. Never substituted for.");
        Console.WriteLine("  --arguments <text>  What to pass it. Default: -8 \"<capture>\".");
        Console.WriteLine("  --utc <stamp>       yyyy-MM-ddTHH:mm:ssZ. Otherwise read from the file");
        Console.WriteLine("                      name, then from the file's own timestamp.");
        Console.WriteLine();
        Console.WriteLine("The fixture lands beside the capture with the same stem and the extension");
        Console.WriteLine($"{Ft8CaptureFixture.Extension}. Commit both. It is written whole or not at all.");
    }
}
