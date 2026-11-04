using System.Reflection;

namespace Hamlet.RadioEngine.Cw;

/// <summary>
/// **THE WORDS OF THE AIR** (work instruction 595, under HM-DEC-296, *Hamlet may know words*): whether a word the reader laid is a word.
/// It judges whether a decode reads as text and nothing else: no letter is changed, added or removed because of a word.
/// The Q-codes, prosigns and abbreviations are listed here; the common English and the first names are embedded from <c>data\cw</c>, each file
/// naming its source on its first line; the callsign, RST and number are shapes, matched by pattern.
/// </summary>
public static class CwVocabulary
{
    private static readonly string[] QCodes = ["QTH", "QRL", "QRM", "QRN", "QSB", "QSL", "QSO", "QRZ", "QRS", "QRQ", "QRP", "QRT", "QRV", "QRX", "QSY", "QSK", "QSP", "QTR"];

    private static readonly string[] Prosigns = ["<BT>", "<AR>", "<SK>", "<KN>", "<AS>", "<KA>"];

    private static readonly string[] Abbreviations =
    [
        "CQ", "DE", "K", "KN", "AR", "SK", "BK", "TU", "TNX", "TKS", "FER", "FB", "U", "UR", "URS", "RST", "RPRT", "ES", "HR", "HW",
        "CPY", "CPI", "AGN", "PSE", "PLS", "OM", "YL", "XYL", "NR", "NAME", "WX", "TEMP", "ANT", "RIG", "PWR", "WATTS", "GA", "GM", "GE", "GN", "GB", "CUL", "CUAGN",
        "73", "88", "DX", "TEST", "VY", "NW", "BTU", "SRI", "SORRY", "OK", "R", "RR", "WID", "HPE", "SN", "SOON", "HV", "HVE", "CONDX", "ABT", "ABOUT", "MNI",
        "MANY", "DIPOLE", "VERT", "YAGI", "KEY", "BUG", "STN", "MOBILE", "WPM",
    ];

    private static readonly string[] States =
    [
        "AL", "AK", "AZ", "AR", "CA", "CO", "CT", "DE", "DC", "FL", "GA", "HI", "ID", "IL", "IN", "IA", "KS", "KY", "LA", "ME", "MD", "MA", "MI", "MN", "MS", "MO",
        "MT", "NE", "NV", "NH", "NJ", "NM", "NY", "NC", "ND", "OH", "OK", "OR", "PA", "RI", "SC", "SD", "TN", "TX", "UT", "VT", "VA", "WA", "WV", "WI", "WY",
    ];

    // The only English of one or two letters; every other short word is an abbreviation or a state's code (section 2).
    private static readonly string[] FunctionWords = ["A", "I", "AN", "IN", "ON", "AT", "TO", "OF", "IS", "IT", "BE", "BY", "DO", "GO", "HE", "ME", "MY", "NO", "OR", "SO", "UP", "US", "WE", "AM", "AS", "IF"];

    private static readonly HashSet<string> Listed = Build();

    /// <summary>How many words the list holds; the shapes are beside it.</summary>
    public static int Size => Listed.Count;

    /// <summary>A word is recognized if it is on the list or matches a shape: a Q-code may carry its <c>?</c>.</summary>
    public static bool IsWord(string word)
        => word.Length > 0 && (Listed.Contains(word) || IsQuestion(word) || IsCallsign(word) || IsRst(word) || IsNumber(word));

    /// <summary>One or two letters, a digit, one to three letters, with an optional suffix of a slash and up to three letters or digits, such as <c>/M</c>, <c>/QRP</c> or <c>/4</c>.</summary>
    public static bool IsCallsign(string word)
    {
        var slash = word.IndexOf('/', StringComparison.Ordinal);
        var body = slash < 0 ? word : word[..slash];

        if (slash >= 0)
        {
            var suffix = word[(slash + 1)..];

            if (suffix.Length is < 1 or > 3 || !suffix.All(IsLetterOrDigit))
            {
                return false;
            }
        }

        var k = 0;

        while (k < body.Length && IsLetter(body[k]))
        {
            k++;
        }

        if (k is < 1 or > 2 || k >= body.Length || !IsDigit(body[k]))
        {
            return false;
        }

        var tail = body.Length - k - 1;

        return tail is >= 1 and <= 3 && body[(k + 1)..].All(IsLetter);
    }

    /// <summary><c>5</c> and two digits, where <c>N</c> may stand for a nine and <c>T</c> for a nought: <c>579</c>, <c>5NN</c>, <c>5N9</c>.</summary>
    public static bool IsRst(string word)
        => word.Length == 3 && word[0] == '5' && word.Skip(1).All(c => IsDigit(c) || c is 'N' or 'T');

    /// <summary>One to four digits; a ZIP's first part or a year is just a number.</summary>
    public static bool IsNumber(string word)
        => word.Length is >= 1 and <= 4 && word.All(IsDigit);

    private static bool IsQuestion(string word)
        => word.Length == 4 && word[3] == '?' && QCodes.Contains(word[..3], StringComparer.Ordinal);

    private static bool IsLetter(char c) => c is >= 'A' and <= 'Z';

    private static bool IsDigit(char c) => c is >= '0' and <= '9';

    private static bool IsLetterOrDigit(char c) => IsLetter(c) || IsDigit(c);

    private static HashSet<string> Build()
    {
        var set = new HashSet<string>(StringComparer.Ordinal);

        set.UnionWith(QCodes);
        set.UnionWith(Prosigns);
        set.UnionWith(Abbreviations);
        set.UnionWith(States);
        set.UnionWith(FunctionWords);
        set.UnionWith(Embedded("words-english.txt"));
        set.UnionWith(Embedded("words-names.txt"));

        return set;
    }

    private static IEnumerable<string> Embedded(string file)
    {
        using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Hamlet.RadioEngine.Data.Cw." + file)
            ?? throw new InvalidOperationException("The embedded vocabulary " + file + " is missing.");
        using var reader = new StreamReader(stream);
        var words = new List<string>();

        while (reader.ReadLine() is { } line)
        {
            if (line.Length > 0 && line[0] != '#')
            {
                words.Add(line.Trim());
            }
        }

        return words;
    }
}
