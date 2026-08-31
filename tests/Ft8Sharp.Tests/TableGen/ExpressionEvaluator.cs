namespace Ft8Sharp.Tests.TableGen;

/// <summary>
/// Evaluates the arithmetic a C header uses for a dimension macro, and nothing more.
/// </summary>
/// <remarks>
/// <c>FTX_LDPC_K_BYTES</c> is not a number in <c>constants.h</c>; it is arithmetic over
/// another macro. Without this, the cross-check against the header would silently degrade
/// to "could not resolve" on exactly the dimension most worth corroborating. What it
/// handles is integer literals, macro names already resolved, the four arithmetic
/// operators, remainder, unary sign and parentheses. Anything else — a shift, a cast, a
/// sizeof — returns false, and the caller reports the macro as unresolved rather than
/// inventing a value for it.
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

    private static void SkipWhitespace(string text, ref int i)
    {
        while (i < text.Length && char.IsWhiteSpace(text[i]))
        {
            i++;
        }
    }
}
