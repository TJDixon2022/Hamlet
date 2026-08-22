# tools/reference-decoder

`reference_decoder.py` is a working probabilistic CW decoder in ~120 lines of
Python. **It is reference material. It is not on Hamlet's decode path and
nothing in `src/` may call it.**

It is here because three work orders in a row were written from recordings and
measurements that no session could open, and a description of an algorithm in a
work order is not something a session can check its output against. This is.

## What it demonstrates

Hamlet thresholds the envelope into hard key-down/key-up runs, fits the speed by
clustering those run lengths, and selects its analysis window from the fitted
speed. That is a loop with positive feedback. Measured: senders working near
fourteen words a minute fitted at 22 to 56, and eight of nine recordings sat at
75 Hz when they wanted 30.

This decoder has no threshold, so it has no loop. Per-sample log-likelihoods of
key-down and key-up are carried forward as numbers; speed is an outer hypothesis
the audio chooses between; and element boundaries and character boundaries are
decided together by dynamic programming rather than one gap at a time.

## What it produced

On the four recordings in `tests/fixtures/cw/captured/` holding a station, and
the two holding none:

| recording | ratio | speed found | text |
|---|---|---|---|
| `003016` | 24.2 | 22 WPM | `I= HADA KPA15TT ITWAS JUNK = ESTILL HVE MY ETO 91B TT JUST VFB TUBELIN` |
| `003126` | 30.9 | 28 WPM | `A OM = I WATCH AT LEAST 2 MOVIES A DAY WID X# WHY NOT ... WESTERNS` |
| `003758` | 39.2 | 16 WPM | `KIS QRL TU ... AA4MP/4 QNIK` |
| `004507` | 32.5 | 18 WPM | `E JJ AT ARRL DOT NET = EACH STATION HANDLING THIS MESSAGE PE` |
| `014854` | 6.1 | — | nothing |
| `014935` | 2.8 | — | nothing |

No seed was given. No speed was set by an operator. The decoder found 22, 28, 16
and 18 words a minute on its own.

**The likelihood ratio separates without overlap** — 24 to 39 with a station, 3
to 6 without. Any gate between 10 and 20 reads every station and stays silent on
every empty band. HM-DEC-120 is not traded for the character counts; silence
falls out of modelling the null hypothesis explicitly, so it competes.

## The length penalty is scored as a ratio (2026-08-22)

Originally the penalty on a segment straying from its expected length was
`(span - want) / (want * 0.35)` — a difference, scaled by the kind's own expected
length. **That gives a character gap three times an element gap's slack and a word
gap seven times**, so the two costs cross at **1.5 units instead of 2**. Every gap
longer than one and a half dits was called a character gap, and the evidence term
is identical for both readings, so nothing argued back. **That is what shattered
letters into single elements.**

At a gap of exactly two units the element reading cost 4.08 and the character
reading 0.45 — nine to one for the wrong answer.

It is now `ln(span / want) / 0.35`, which puts both crossovers at the geometric
mean, **1.73 units**, and rests on a property of hands rather than of textbooks:
timing error is multiplicative, so a sender who runs a fifth long runs a fifth
long on dits, dahs and gaps alike.

Measured in the app with the same change: `2 MOVIES A DAY` where it read
`2 IOVI ES`, `EACH` as one word, **`N4LQ K` on the capture HM-DEC-144 adjudicated
as `N4L`**, and **`VRR VA` on the one HM-DEC-145 adjudicated as `VA3VRR`**. Here:
`EACH STATION HANDLING THIS MESSAG E PE` where it read `E ACH ... ME SSAG E PE`.

**Elements per character did not move in aggregate.** It reads better in specific
measurable places and the headline number stayed put. Both halves are true.

## Where this comes from

E. L. Bell, 1977, *Optimal Bayesian estimation of the state of a probabilistically
mapped memory-conditional Markov process with application to manual morse
decoding*. Bell's decoder carries about twenty parallel paths, each with its own
letter state, element duration and speed, scored by Kalman recursions against a
tracked noise power estimate, over a trellis with about a second of decision
delay so later evidence can revise an earlier letter.

`ag1le/morse-wip` is that algorithm transcribed to C++, GPL-3.0, about 5,400
lines. CW Skimmer is Bayesian by its author's own account. The common thread
across every serious implementation is that **none of them threshold.**

This file is the three load-bearing ideas at a size that fits in Hamlet.

## What is not demonstrated, and must not be assumed

- **Streaming.** This runs offline over whole thirty-second files. A live
  terminal needs a sliding window and a decision delay. Nobody has measured
  what that costs.
- **Cost.** The speed search is an outer loop over twelve hypotheses. Fine per
  file; unmeasured per second in a running app.
- **Correctness of the text.** None of these recordings has an adjudicated
  answer key. The table above is what this decoder emitted, not truth (§12.5).
  `ETO 91B`, `ARRL DOT NET` and `AA4MP/4` are anchors that look right; they are
  not a key and no session may write one.
- **`GATE = 15.0` is provisional.** It sits in a gap between 6 and 24 measured
  on six recordings. It wants an evening's captures scored against it.

## Running it

    python reference_decoder.py <wavfile> [more wavfiles]

Requires numpy. Any sample rate; it resamples by decimation internally.
