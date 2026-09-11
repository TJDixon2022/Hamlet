using System.Text;

namespace Hamlet.RadioEngine.Psk31;

/// <summary>One complete message out of a channel's text, and what it says.</summary>
/// <param name="Text">What arrived between two turnovers, trimmed.</param>
/// <param name="Exchange">The parse of it (<see cref="Psk31ExchangeParser.Read"/>).</param>
public sealed record Psk31Message(string Text, Psk31Exchange Exchange);

/// <summary>Cuts one channel's characters into messages as they arrive.</summary>
/// <remarks>
/// <para>**§3.4: A MESSAGE FOR THE PARSER IS WHAT ARRIVED BETWEEN TWO TURNOVERS**, not what
/// arrived in a slot. PSK31 has no slots and no fixed length.</para>
/// <para>**THE RULE IS <see cref="SplitRule"/>**, and the reason for each part of it:</para>
/// <list type="bullet">
/// <item>**A WORD, NOT A LETTER.** `K` ends a message and `K` is also the first letter of
/// `KC3QIS` and the last of `OK`, so nothing is decided until whitespace says the word is
/// finished. A `K` with nothing after it yet is not a turnover; it is a character.</item>
/// <item>**ONLY AFTER A SIGN.** A station closes its over with the calls - `de W1AW K`,
/// `de W1AW W1AW pse K` - and a turnover word anywhere else is prose: `BTU` in the middle of a
/// report, `SK` in *I will SK the rig*, `OVER` in *over to you*. One word is allowed between
/// the calls and the turnover because operators put one there.</item>
/// <item>**`73` DOES NOT SPLIT.** It is a closer and it turns up in the middle of the last over
/// (*understood, 73 VE3XN de KC3QIS SK*), where splitting on it would cut the over in two. The
/// `SK` or `CL` after it is what ends the message.</item>
/// <item>**A TAIL IS NOT A MESSAGE.** `SK SK` and `K K` are one ending sent twice; the second
/// arrives after the message is out and starts nothing.</item>
/// <item>**ONLY A CLEAN CALLSIGN MAKES A SIGN.** A damaged one does not split, so that message
/// runs into the next, and the damage keeps the merged message uncertain - merging can never
/// make anything more certain than what it holds.</item>
/// </list>
/// <para>**ONE CHANNEL, ONE SPLITTER**, fed in the order the demodulator read. It knows nothing
/// about rows, tabs or settings (§0.1).</para>
/// </remarks>
public sealed class Psk31MessageSplitter
{
    /// <summary>The split rule, in one sentence, for a reader and for the report.</summary>
    public const string SplitRule =
        "A message ends at the whitespace after K, KN, BTU, OVER, SK or CL when that word follows "
        + "de, one or more clean callsigns and at most one other word - so K inside OK, and SK, BTU "
        + "or OVER in the middle of a sentence, never split, 73 on its own never splits, and "
        + "turnover words that arrive straight after a message are its tail and start nothing.";

    private readonly string? _operatorCallsign;
    private readonly StringBuilder _pending = new();
    private readonly StringBuilder _word = new();
    private readonly List<string> _words = new();
    private bool _inTail;

    /// <summary>Creates a splitter for one channel.</summary>
    /// <param name="operatorCallsign">The operator's own callsign, handed to the parser.</param>
    public Psk31MessageSplitter(string? operatorCallsign) => _operatorCallsign = operatorCallsign;

    /// <summary>Everything that has arrived since the last complete message.</summary>
    public string Pending => _pending.ToString();

    /// <summary>Take one character.</summary>
    /// <param name="character">The next character the channel read.</param>
    /// <returns>A message, where this character completed one; otherwise null.</returns>
    public Psk31Message? Add(char character)
    {
        if (!char.IsWhiteSpace(character))
        {
            _word.Append(character);

            if (!_inTail)
            {
                _pending.Append(character);
            }

            return null;
        }

        if (_word.Length == 0)
        {
            if (!_inTail && _pending.Length > 0)
            {
                _pending.Append(character);
            }

            return null;
        }

        var raw = _word.ToString();
        var word = Psk31ExchangeParser.Normalise(raw);

        _word.Clear();

        if (_inTail)
        {
            if (IsSplitWord(word))
            {
                return null;
            }

            // **THE TAIL IS OVER AND THIS WORD STARTS THE NEXT MESSAGE.**
            _inTail = false;
            _pending.Append(raw);
        }

        if (word.Length > 0)
        {
            _words.Add(word);
        }

        if (IsSplitWord(word) && FollowsASign())
        {
            var text = _pending.ToString().Trim();

            _pending.Clear();
            _words.Clear();
            _inTail = true;

            return new Psk31Message(text, Psk31ExchangeParser.Read(text, _operatorCallsign));
        }

        _pending.Append(character);

        return null;
    }

    private static bool IsSplitWord(string word)
        => Psk31ExchangeParser.TurnoverWords.Contains(word) || word is "SK" or "CL";

    /// <summary>True where the word just added closes `de CALL [CALL...] [one word]`.</summary>
    private bool FollowsASign()
    {
        var at = _words.Count - 2;

        if (at >= 0 && _words[at] != "DE" && !Psk31ExchangeParser.IsCleanCallsign(_words[at]))
        {
            at--;
        }

        var calls = 0;

        while (at >= 0 && Psk31ExchangeParser.IsCleanCallsign(_words[at]))
        {
            calls++;
            at--;
        }

        return calls > 0 && at >= 0 && _words[at] == "DE";
    }
}
