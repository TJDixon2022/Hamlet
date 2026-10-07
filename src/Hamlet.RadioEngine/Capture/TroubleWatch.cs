using System.Globalization;
using Hamlet.RadioEngine.Cw;

namespace Hamlet.RadioEngine.Capture;

/// <summary>Which trouble fired (work instruction 549, task 2).</summary>
public enum TroubleKind
{
    /// <summary>Four or more senders held for ten seconds.</summary>
    SendersHeld,

    /// <summary>Any audio lost before the decode.</summary>
    AudioLost,

    /// <summary>A burst of stray letters from the printed sender.</summary>
    StrayLetters,
}

/// <summary>A trigger that fired.</summary>
/// <param name="Kind">Which.</param>
/// <param name="AtUtc">When.</param>
/// <param name="Token">A stable word for the folder and the telemetry: <c>senders</c>, <c>audio-lost</c>, <c>stray-letters</c>.</param>
/// <param name="Detail">What it saw, in words, for the sheet.</param>
public sealed record TroubleFired(TroubleKind Kind, DateTime AtUtc, string Token, string Detail);

/// <summary>
/// **TROUBLE, NOTICED WHERE IT HAPPENS** (work instruction 549, task 2): three triggers, each with a ten-minute cooldown.
/// </summary>
/// <remarks>
/// <para>**FOUR OR MORE SENDERS HELD FOR TEN SECONDS.** While W1AW is on, the owner's telemetry of 2026-10-07 showed Hamlet
/// holding four to nine senders at once, never under two, where at other hours it holds nought to two and a replay of a
/// W1AW recording holds one. There is one station. Four is past anything an ordinary band gave that day, and ten seconds is
/// longer than a burst of noise forms a sender and is forgotten.</para>
/// <para>**ANY AUDIO LOST.** The bench reproduced W1AW's junk by losing one 50 ms chunk a second (work instruction 548), so a
/// single lost millisecond is worth five minutes of the audio around it.</para>
/// <para>**A BURST OF STRAY LETTERS: SIX OR MORE LETTERS OF ONE OR TWO ELEMENTS, EACH PRINTED ON ITS OWN, WITHIN TEN
/// SECONDS.** The order's figure was six letters of one or two elements in ten seconds, and measured on text it fires on
/// clean copy (<c>WhatJunkLooksLikeTests</c>): E, T, I, A, N and M are half of English, so plain English sent at 18 words a
/// minute prints up to fifteen of them in ten seconds and a bulletin's text nineteen; six of them in a row comes up in
/// English and in Q-code exchanges as often as in the junk. What clean copy never does is print them alone: the only
/// one-letter words are A and I, and English, a bulletin's text and a Q-code exchange print at most two lone ones in any ten
/// seconds at 18, 25 or 35 words a minute. The owner's word was *stray* letters, the `E I S A N` kind, so six alone in ten
/// seconds is three times anything clean text does.</para>
/// <para>**TEN MINUTES' COOLDOWN EACH**, the web session's figure: a capture holds the five minutes before its trigger, so
/// ten minutes leaves the next one, should it fire, holding audio the first did not.</para>
/// <para>Letters arrive on the chain's thread and the rest on the app's; one lock covers both.</para>
/// </remarks>
public sealed class TroubleWatch
{
    /// <summary>How many senders held at once is trouble.</summary>
    public const int SendersAtOnce = 4;

    /// <summary>For how long they must be held.</summary>
    public static readonly TimeSpan SendersFor = TimeSpan.FromSeconds(10);

    /// <summary>How many stray letters is a burst.</summary>
    public const int StrayLetters = 6;

    /// <summary>Within how long.</summary>
    public static readonly TimeSpan StrayWithin = TimeSpan.FromSeconds(10);

    /// <summary>How long each trigger waits after it fires.</summary>
    public static readonly TimeSpan Cooldown = TimeSpan.FromMinutes(10);

    private readonly object _gate = new();
    private readonly Dictionary<TroubleKind, DateTime> _fired = new();
    private readonly Queue<DateTime> _stray = new();

    private DateTime? _sendersSince;
    private double _lostMs;
    private int _wordLetters;
    private DateTime? _loneShortAt;

    /// <summary>A character the terminal printed, as it arrives.</summary>
    /// <param name="character">The character, or a word gap.</param>
    /// <param name="atUtc">When it arrived.</param>
    /// <remarks>
    /// A letter is alone once the word gap after it arrives with nothing else in its word; the gate sends the word gap when
    /// the next word begins, so a lone letter is counted then, at the moment it was itself printed.
    /// </remarks>
    public void Character(CwCharacter character, DateTime atUtc)
    {
        ArgumentNullException.ThrowIfNull(character);

        lock (_gate)
        {
            if (character.IsWordGap)
            {
                if (_wordLetters == 1 && _loneShortAt is { } at)
                {
                    _stray.Enqueue(at);
                }

                _wordLetters = 0;
                _loneShortAt = null;
                return;
            }

            _wordLetters++;

            var elements = character.Pattern.Count(c => c is '.' or '-');

            _loneShortAt = _wordLetters == 1 && !character.IsUnreadable && elements is 1 or 2 ? atUtc : null;
        }
    }

    /// <summary>Look, once a second, while trouble may be saved; what fires is returned and starts its cooldown.</summary>
    /// <param name="nowUtc">The moment.</param>
    /// <param name="sendersHeld">How many senders the gate holds.</param>
    /// <param name="audioLostMs">Audio lost since listening began, in milliseconds.</param>
    /// <returns>The triggers that fired.</returns>
    public IReadOnlyList<TroubleFired> Observe(DateTime nowUtc, int sendersHeld, double audioLostMs)
    {
        var fired = new List<TroubleFired>();

        lock (_gate)
        {
            if (sendersHeld >= SendersAtOnce)
            {
                _sendersSince ??= nowUtc;

                if (nowUtc - _sendersSince.Value >= SendersFor)
                {
                    Fire(fired, TroubleKind.SendersHeld, nowUtc, "senders", string.Format(
                        CultureInfo.InvariantCulture,
                        "four or more senders held for ten seconds  ({0} held now, since {1:HH:mm:ss} UTC)",
                        sendersHeld,
                        _sendersSince.Value));
                }
            }
            else
            {
                _sendersSince = null;
            }

            if (audioLostMs > _lostMs)
            {
                Fire(fired, TroubleKind.AudioLost, nowUtc, "audio-lost", string.Format(
                    CultureInfo.InvariantCulture,
                    "audio lost before the decode  ({0:0} ms more since the last look, {1:0} ms since listening began)",
                    audioLostMs - _lostMs,
                    audioLostMs));
            }

            _lostMs = Math.Max(_lostMs, audioLostMs);

            while (_stray.Count > 0 && nowUtc - _stray.Peek() > StrayWithin)
            {
                _stray.Dequeue();
            }

            if (_stray.Count >= StrayLetters)
            {
                Fire(fired, TroubleKind.StrayLetters, nowUtc, "stray-letters", string.Format(
                    CultureInfo.InvariantCulture,
                    "a burst of stray letters  ({0} letters of one or two elements, each printed on its own, within ten seconds)",
                    _stray.Count));
                _stray.Clear();
            }
        }

        return fired;
    }

    /// <summary>
    /// While trouble may not be saved - scanning, not in CW - nothing builds up: a run of senders starts again, the lost
    /// count is taken as seen, and stray letters are forgotten.
    /// </summary>
    /// <param name="audioLostMs">Audio lost since listening began.</param>
    public void Pause(double audioLostMs)
    {
        lock (_gate)
        {
            _sendersSince = null;
            _lostMs = Math.Max(_lostMs, audioLostMs);
            _stray.Clear();
            _wordLetters = 0;
            _loneShortAt = null;
        }
    }

    private void Fire(List<TroubleFired> fired, TroubleKind kind, DateTime nowUtc, string token, string detail)
    {
        if (_fired.TryGetValue(kind, out var last) && nowUtc - last < Cooldown)
        {
            return;
        }

        _fired[kind] = nowUtc;
        fired.Add(new TroubleFired(kind, nowUtc, token, detail));
    }
}
