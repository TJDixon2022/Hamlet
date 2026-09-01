namespace Ft8Sharp.Tests.TableGen;

/// <summary>
/// Evaluates the arithmetic a C header uses for a dimension macro, and nothing more.
/// </summary>
/// <remarks>
/// <c>FTX_LDPC_K_BYTES</c> is not a number in <c>constants.h</c>; it is arithmetic over
/// another macro. Without this, the cross-check against the header would silently degrade
/// to "could not resolve" on exactly the dimension most worth corroborating. What it
/// handles is integer literals, macro names already resolved, the four arithmetic
/// operators, remainder, unary sign, parentheses, and a cast to one of the fixed-width
/// integer types C spells in <c>stdint.h</c>. Anything else — a shift, a sizeof — returns
/// false, and the caller reports the macro as unresolved rather than inventing a value
/// for it.
/// <para>
/// <b>The cast was added for the message layer</b> (unit 206). Upstream writes its CRC
/// scalars as a cast literal, and without this the provenance test would have had to
/// resolve them with a second parser — the thing this file exists to avoid. A cast to an
/// unsigned type is applied rather than ignored, so a macro that truncates in C truncates
/// here too; assuming a cast is a no-op is how a checker silently stops checking.
/// </para>
/// </remarks>
internal static class ExpressionEvaluator
{
    public static bool TryEvaluate(string expression, IReadOnlyDictionary<string, long> symbols, out long value)
    {
        value = 0;
        var cursor = 0;
        if (!TryExpression(expression, symbols, ref cursor, out var result))
        {
            return false;
        }

        SkipWhitespace(expression, ref cursor);
        if (cursor != expression.Length)
        {
            return false;
        }

        value = result;
        return true;
    }

    private static bool TryExpression(
        string text,
        IReadOnlyDictionary<string, long> symbols,
        ref int i,
        out long value)
    {
        if (!TryTerm(text, symbols, ref i, out value))
        {
            return false;
        }

        while (true)
        {
            SkipWhitespace(text, ref i);
            if (i >= text.Length || (text[i] != '+' && text[i] != '-'))
            {
                return true;
            }

            var op = text[i++];
            if (!TryTerm(text, symbols, ref i, out var right))
            {
                return false;
            }

            value = op == '+' ? value + right : value - right;
        }
    }

    private static bool TryTerm(
        string text,
        IReadOnlyDictionary<string, long> symbols,
        ref int i,
        out long value)
    {
        if (!TryUnary(text, symbols, ref i, out value))
        {
            return false;
        }

        while (true)
        {
            SkipWhitespace(text, ref i);
            if (i >= text.Length || (text[i] != '*' && text[i] != '/' && text[i] != '%'))
            {
                return true;
            }

            var op = text[i++];
            if (!TryUnary(text, symbols, ref i, out var right))
            {
                return false;
            }

            if (op != '*' && right == 0)
            {
                return false;
            }

            value = op switch
            {
                '*' => value * right,
                '/' => value / right,
                _ => value % right,
            };
        }
    }

    private static bool TryUnary(
        string text,
        IReadOnlyDictionary<string, long> symbols,
        ref int i,
        out long value)
    {
        SkipWhitespace(text, ref i);
        if (i < text.Length && (text[i] == '-' || text[i] == '+'))
        {
            var negate = text[i] == '-';
            i++;
            if (!TryUnary(text, symbols, ref i, out value))
            {
                return false;
            }

            if (negate)
            {
                value = -value;
            }

            return true;
        }

        return TryPrimary(text, symbols, ref i, out value);
    }

    private static bool TryPrimary(
        string text,
        IReadOnlyDictionary<string, long> symbols,
        ref int i,
        out long value)
    {
        value = 0;
        SkipWhitespace(text, ref i);
        if (i >= text.Length)
        {
            return false;
        }

        if (TryCast(text, symbols, ref i, out value))
        {
            return true;
        }

        if (text[i] == '(')
        {
            i++;
            if (!TryExpression(text, symbols, ref i, out value))
            {
                return false;
            }

            SkipWhitespace(text, ref i);
            if (i >= text.Length || text[i] != ')')
            {
                return false;
            }

            i++;
            return true;
        }

        var start = i;
        if (char.IsLetter(text[i]) || text[i] == '_')
        {
            while (i < text.Length && (char.IsLetterOrDigit(text[i]) || text[i] == '_'))
            {
                i++;
            }

            return symbols.TryGetValue(text[start..i], out value);
        }

        if (!char.IsDigit(text[i]))
        {
            return false;
        }

        while (i < text.Length && (char.IsLetterOrDigit(text[i]) || text[i] == '_'))
        {
            i++;
        }

        return CSourceParser.TryParseIntegerLiteral(text[start..i], out value);
    }

    /// <summary>
    /// The casts this evaluator will apply, with the width each one keeps. Named explicitly
    /// rather than pattern-matched on a trailing <c>_t</c>: a type this file has never heard
    /// of is a macro it should refuse, not one it should guess the width of.
    /// </summary>
    private static readonly Dictionary<string, int> UnsignedCastWidths = new(StringComparer.Ordinal)
    {
        ["uint8_t"] = 8,
        ["uint16_t"] = 16,
        ["uint32_t"] = 32,
    };

    /// <summary>Signed casts, which are accepted and applied as no-ops at these widths.</summary>
    private static readonly HashSet<string> SignedCasts = new(StringComparer.Ordinal)
    {
        "int8_t",
        "int16_t",
        "int32_t",
        "int64_t",
        "int",
        "long",
    };

    /// <summary>
    /// Reads <c>(type)operand</c> if that is what is here, and leaves the cursor untouched if
    /// it is not — <c>(FTX_LDPC_K + 7)</c> must still parse as a parenthesised expression.
    /// </summary>
    private static bool TryCast(
        string text,
        IReadOnlyDictionary<string, long> symbols,
        ref int i,
        out long value)
    {
        value = 0;
        var save = i;

        if (text[i] != '(')
        {
            return false;
        }

        var cursor = i + 1;
        SkipWhitespace(text, ref cursor);
        var start = cursor;
        while (cursor < text.Length && (char.IsLetterOrDigit(text[cursor]) || text[cursor] == '_'))
        {
            cursor++;
        }

        var name = text[start..cursor];
        SkipWhitespace(text, ref cursor);
        if (name.Length == 0 || cursor >= text.Length || text[cursor] != ')')
        {
            i = save;
            return false;
        }

        var unsignedCast = UnsignedCastWidths.TryGetValue(name, out var width);
        if (!unsignedCast && !SignedCasts.Contains(name))
        {
            i = save;
            return false;
        }

        cursor++;
        if (!TryUnary(text, symbols, ref cursor, out var operand))
        {
            i = save;
            return false;
        }

        // An unsigned cast in C wraps. Applying it keeps the evaluator honest about a macro
        // whose written value does not fit the type it is cast to.
        if (unsignedCast)
        {
            operand = width == 32
                ? (long)(uint)operand
                : operand & ((1L << width) - 1);
        }

        i = cursor;
        value = operand;
        return true;
    }

    private static void SkipWhitespace(string text, ref int i)
    {
        while (i < text.Length && char.IsWhiteSpace(text[i]))
        {
            i++;
        }
    }
}
