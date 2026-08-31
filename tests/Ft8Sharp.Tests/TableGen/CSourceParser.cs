using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Ft8Sharp.Tests.TableGen;

/// <summary>
/// A refusal from the converter, always carrying the C identifier it refused over.
/// </summary>
/// <remarks>
/// Every failure this converter can have names the table it happened on. A parser that
/// goes quiet about a table it did not find is the failure mode that would poison every
/// later step: the emitted file would simply be missing a table, the build would be
/// green, and the fault would surface four stages away as a decoder that never decodes.
/// <para>
/// <b>No message here carries a table value.</b> Positions, counts, widths and
/// identifiers are metadata; the bytes have exactly one route into this repository and
/// an exception message is not it.
/// </para>
/// </remarks>
public sealed class TableConversionException : Exception
{
    public TableConversionException(string identifier, string what)
        : base($"{identifier}: {what}")
        => Identifier = identifier;

    /// <summary>The C identifier the refusal is about.</summary>
    public string Identifier { get; }
}

/// <summary>One C array as it was found in the source: its shape, and its flattened values.</summary>
/// <param name="Identifier">The C identifier, exactly as upstream spells it.</param>
/// <param name="DeclaredDimensions">The bracketed dimension text as written, macros unresolved.</param>
/// <param name="Shape">The dimensions as the initialiser's own structure gives them.</param>
/// <param name="Values">Every element, flattened row-major.</param>
public sealed record ParsedTable(
    string Identifier,
    string DeclaredDimensions,
    IReadOnlyList<int> Shape,
    byte[] Values)
{
    public int ElementCount => Values.Length;

    /// <summary>The width of one row, or the whole table where it has only one dimension.</summary>
    public int RowWidth => Shape.Count == 2 ? Shape[1] : Values.Length;
}

/// <summary>
/// Reads C source well enough to lift a <c>uint8_t</c> table out of it, and no further.
/// </summary>
/// <remarks>
/// This is not a C compiler and does not pretend to be one. It handles what
/// <c>ft8/constants.c</c> actually contains — nested brace initialisers, block and line
/// comments, hexadecimal and decimal literals, integer suffixes and trailing commas —
/// and refuses loudly, by name, at anything else. It lives in the test assembly rather
/// than in <c>Ft8Sharp</c> because a parser of C source has no business shipping inside
/// a published decoder, and because <c>dotnet run</c> is not available in this loop, so
/// a test is the only executable surface a converter can have.
/// </remarks>
public static class CSourceParser
{
    /// <summary>
    /// Finds one array definition by identifier and returns its shape and its values.
    /// </summary>
    /// <exception cref="TableConversionException">
    /// The identifier is absent, its initialiser is unbalanced or ragged, or a literal in
    /// it is not an integer that fits <c>uint8_t</c>.
    /// </exception>
    public static ParsedTable ParseArray(string source, string identifier)
    {
        var text = StripCommentsAndLiterals(source);

        var definition = new Regex(
            @"\b" + Regex.Escape(identifier) + @"\s*(?<dims>(?:\[[^\]]*\]\s*)+)=\s*\{",
            RegexOptions.Compiled);

        var match = definition.Match(text);
        if (!match.Success)
        {
            throw new TableConversionException(
                identifier,
                "no array definition with this identifier was found in the source. The converter "
                + "will not guess, substitute or carry on without it — a table silently missing "
                + "from the emitted file is the one fault that would not show up until the "
                + "decoder did not decode.");
        }

        var open = match.Index + match.Length - 1;
        var cursor = open;
        var root = ParseGroup(text, ref cursor, identifier);

        var shape = ShapeOf(root, identifier);
        var values = Flatten(root, identifier);

        var declared = Regex.Replace(match.Groups["dims"].Value.Trim(), @"\s+", string.Empty);
        return new ParsedTable(identifier, declared, shape, values);
    }

    /// <summary>
    /// The bracketed dimensions as written, one string per dimension, macros unresolved.
    /// </summary>
    public static IReadOnlyList<string> DeclaredDimensionTerms(ParsedTable table)
        => Regex.Matches(table.DeclaredDimensions, @"\[(?<d>[^\]]*)\]")
            .Select(m => m.Groups["d"].Value.Trim())
            .ToList();

    /// <summary>
    /// Checks the dimensions the source declares against the shape the initialiser actually
    /// has, resolving macro dimensions from <paramref name="macros"/>.
    /// </summary>
    /// <param name="unresolved">
    /// Dimension terms that are neither an integer literal nor a macro this pass could
    /// evaluate. Those are reported, not failed — a header that will not parse is a gap in
    /// the corroboration, where a header that parses and disagrees is a contradiction.
    /// </param>
    /// <exception cref="TableConversionException">
    /// The source declares a different number of dimensions than the initialiser has, or a
    /// dimension that resolves to a number the initialiser contradicts.
    /// </exception>
    public static void CrossCheckDimensions(
        ParsedTable table,
        IReadOnlyDictionary<string, long> macros,
        List<string> unresolved)
    {
        var terms = DeclaredDimensionTerms(table);
        if (terms.Count != table.Shape.Count)
        {
            throw new TableConversionException(
                table.Identifier,
                $"the source declares {terms.Count} dimension(s) as '{table.DeclaredDimensions}' but "
                + $"its initialiser is {table.Shape.Count}-dimensional.");
        }

        for (var i = 0; i < terms.Count; i++)
        {
            var term = terms[i];
            if (term.Length == 0)
            {
                // An unsized leading dimension — legal C, and the initialiser is the authority.
                continue;
            }

            long declared;
            if (TryParseIntegerLiteral(term, out var literal))
            {
                declared = literal;
            }
            else if (macros.TryGetValue(term, out var macro))
            {
                declared = macro;
            }
            else
            {
                unresolved.Add($"{table.Identifier}[{i}] = {term}");
                continue;
            }

            if (declared != table.Shape[i])
            {
                throw new TableConversionException(
                    table.Identifier,
                    $"dimension {i} is declared as '{term}', which resolves to {declared}, but the "
                    + $"initialiser has {table.Shape[i]} there. The header and the source disagree "
                    + "about this table's geometry, and that is a contradiction rather than a "
                    + "preference — one of the two is not the file this port thinks it is.");
            }
        }
    }

    /// <summary>
    /// Every object-like <c>#define</c> in a header whose body evaluates to an integer.
    /// </summary>
    /// <remarks>
    /// Function-like macros are skipped — the regex requires whitespace between the name and
    /// the body, which <c>#define MAX(a,b)</c> does not have. Bodies that will not evaluate
    /// are left out rather than guessed at, and the caller reports what was missing.
    /// </remarks>
    public static IReadOnlyDictionary<string, long> ParseIntegerMacros(string headerSource)
    {
        var text = StripCommentsAndLiterals(headerSource);
        text = Regex.Replace(text, @"\\\r?\n", " ");

        var raw = new List<KeyValuePair<string, string>>();
        foreach (Match m in Regex.Matches(
                     text,
                     @"^[ \t]*#[ \t]*define[ \t]+(?<name>[A-Za-z_][A-Za-z0-9_]*)[ \t]+(?<body>[^\r\n]*)$",
                     RegexOptions.Multiline))
        {
            raw.Add(new KeyValuePair<string, string>(m.Groups["name"].Value, m.Groups["body"].Value.Trim()));
        }

        var resolved = new Dictionary<string, long>(StringComparer.Ordinal);

        // Several passes, because a macro may be defined in terms of one further down the file.
        // Bounded by the count so a circular definition ends the loop rather than the process.
        for (var pass = 0; pass <= raw.Count; pass++)
        {
            var progress = false;
            foreach (var (name, body) in raw)
            {
                if (resolved.ContainsKey(name))
                {
                    continue;
                }

                if (ExpressionEvaluator.TryEvaluate(body, resolved, out var value))
                {
                    resolved[name] = value;
                    progress = true;
                }
            }

            if (!progress)
            {
                break;
            }
        }

        return resolved;
    }

    private sealed class Node
    {
        public List<Node>? Children { get; init; }

        public long? Value { get; init; }
    }

    private static Node ParseGroup(string text, ref int i, string identifier)
    {
        // text[i] is the opening brace.
        i++;
        var children = new List<Node>();
        while (true)
        {
            while (i < text.Length && char.IsWhiteSpace(text[i]))
            {
                i++;
            }

            if (i >= text.Length)
            {
                throw new TableConversionException(
                    identifier,
                    "the initialiser runs to the end of the file without a closing brace.");
            }

            if (text[i] == '}')
            {
                i++;
                return new Node { Children = children };
            }

            if (text[i] == ',')
            {
                // Also swallows a trailing comma before the closing brace.
                i++;
                continue;
            }

            if (text[i] == '{')
            {
                children.Add(ParseGroup(text, ref i, identifier));
                continue;
            }

            var start = i;
            while (i < text.Length
                   && text[i] != ','
                   && text[i] != '{'
                   && text[i] != '}'
                   && !char.IsWhiteSpace(text[i]))
            {
                i++;
            }

            var token = text[start..i];
            if (!TryParseIntegerLiteral(token, out var value))
            {
                throw new TableConversionException(
                    identifier,
                    $"the literal at flat position {CountLeaves(children)} could not be read as an "
                    + "integer. This converter handles decimal and hexadecimal literals with the "
                    + "usual integer suffixes and nothing else; an expression here needs the parser "
                    + "widened rather than the value transcribed.");
            }

            children.Add(new Node { Value = value });
        }
    }

    private static int CountLeaves(List<Node> nodes)
        => nodes.Sum(n => n.Value.HasValue ? 1 : CountLeaves(n.Children!));

    private static IReadOnlyList<int> ShapeOf(Node root, string identifier)
    {
        var children = root.Children!;
        if (children.Count == 0)
        {
            throw new TableConversionException(identifier, "the initialiser is empty.");
        }

        if (children.All(c => c.Value.HasValue))
        {
            return new[] { children.Count };
        }

        if (children.All(c => c.Children is not null))
        {
            if (children.Any(c => c.Children!.Any(g => !g.Value.HasValue)))
            {
                throw new TableConversionException(
                    identifier,
                    "the initialiser nests more than two deep. Nothing in this port's six tables "
                    + "does, so the file is not what the converter was written against.");
            }

            var widths = children.Select(c => c.Children!.Count).Distinct().ToList();
            if (widths.Count != 1)
            {
                throw new TableConversionException(
                    identifier,
                    $"its rows are ragged — {widths.Count} distinct row widths across "
                    + $"{children.Count} rows. A table whose rows are not all one width cannot be "
                    + "flattened behind a stride constant.");
            }

            return new[] { children.Count, widths[0] };
        }

        throw new TableConversionException(
            identifier,
            "the initialiser mixes bare values and nested rows at its top level.");
    }

    private static byte[] Flatten(Node root, string identifier)
    {
        var values = new List<byte>();
        Walk(root);
        return values.ToArray();

        void Walk(Node node)
        {
            if (node.Children is not null)
            {
                foreach (var child in node.Children)
                {
                    Walk(child);
                }

                return;
            }

            var value = node.Value!.Value;
            if (value is < 0 or > 255)
            {
                throw new TableConversionException(
                    identifier,
                    $"the element at flat position {values.Count} does not fit uint8_t — it falls "
                    + "outside 0..255. Upstream declares this table as uint8_t, so either the file "
                    + "is not the pinned one or the parser has mis-read its structure.");
            }

            values.Add((byte)value);
        }
    }

    internal static bool TryParseIntegerLiteral(string token, out long value)
    {
        value = 0;
        var t = token.Trim();
        if (t.Length == 0)
        {
            return false;
        }

        var negative = false;
        if (t[0] == '+')
        {
            t = t[1..];
        }
        else if (t[0] == '-')
        {
            negative = true;
            t = t[1..];
        }

        t = t.TrimEnd('u', 'U', 'l', 'L');
        if (t.Length == 0)
        {
            return false;
        }

        bool ok;
        if (t.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
        {
            ok = long.TryParse(t[2..], NumberStyles.HexNumber, CultureInfo.InvariantCulture, out value);
        }
        else if (t.StartsWith("0b", StringComparison.OrdinalIgnoreCase))
        {
            ok = TryParseBinary(t[2..], out value);
        }
        else
        {
            ok = long.TryParse(t, NumberStyles.None, CultureInfo.InvariantCulture, out value);
        }

        if (ok && negative)
        {
            value = -value;
        }

        return ok;
    }

    private static bool TryParseBinary(string digits, out long value)
    {
        value = 0;
        if (digits.Length == 0)
        {
            return false;
        }

        foreach (var c in digits)
        {
            if (c is not ('0' or '1'))
            {
                return false;
            }

            value = (value << 1) | (long)(c - '0');
        }

        return true;
    }

    /// <summary>
    /// Blanks comments and string and character literals so that a brace or a comma inside one
    /// is not read as structure. Newlines are preserved so nothing shifts onto another line.
    /// </summary>
    internal static string StripCommentsAndLiterals(string source)
    {
        var text = new StringBuilder(source.Length);
        for (var i = 0; i < source.Length; i++)
        {
            if (source[i] == '/' && i + 1 < source.Length && source[i + 1] == '/')
            {
                while (i < source.Length && source[i] != '\n')
                {
                    i++;
                }

                text.Append('\n');
                continue;
            }

            if (source[i] == '/' && i + 1 < source.Length && source[i + 1] == '*')
            {
                i += 2;
                while (i + 1 < source.Length && !(source[i] == '*' && source[i + 1] == '/'))
                {
                    text.Append(source[i] == '\n' ? '\n' : ' ');
                    i++;
                }

                i++;
                text.Append(' ');
                continue;
            }

            if (source[i] == '"' || source[i] == '\'')
            {
                var quote = source[i];
                i++;
                while (i < source.Length && source[i] != quote)
                {
                    i += source[i] == '\\' ? 2 : 1;
                }

                text.Append('0');
                continue;
            }

            text.Append(source[i]);
        }

        return text.ToString();
    }
}
