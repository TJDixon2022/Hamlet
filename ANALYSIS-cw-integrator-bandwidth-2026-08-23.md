# What each integrator width is worth, 2026-08-23

Every figure is read at a fixed pitch, with the tone tracker out of
the path. The tracker is measurably the largest source of error in
this decoder and it would swamp a filter's contribution entirely; what
this table measures is the filter.

The production default is **45 Hz**
(Hann). It is a constant and this
sweep does not move it: a mutable static the whole suite shares is a
way for one test to change another test's numbers without either
saying so.

Regenerate with:

```
dotnet test tests/Hamlet.RadioEngine.Tests --filter FullyQualifiedName~TheIntegratorBandwidthTable
```

## What each width is, in samples and in time

At 48 kHz. The length is what costs: an integrator longer than a dit
rounds the top of every short mark.

| width | samples | spans | dit at 18 wpm | dit at 30 wpm |
|---|---|---|---|---|
| 60 Hz | 1201 | 25.0 ms | 38 % of it | 63 % of it |
| 45 Hz | 1601 | 33.4 ms | 50 % of it | 83 % of it |
| 30 Hz | 2401 | 50.0 ms | 75 % of it | 125 % of it |
| 20 Hz | 3601 | 75.0 ms | 113 % of it | 188 % of it |

## Rejection: two senders in one passband

The wanted station's eleven characters, read against a competing
station at each offset and level. **The ordered grid saturates**: every
width reads the message whole at every offset from 40 Hz out and every
level down from equal, so it discriminates nothing. Harder rows are
added below it — closer, and louder than the wanted station — because a
table where every cell is perfect measures nothing about the filter.

| offset | level | 60 Hz | 45 Hz | 30 Hz | 20 Hz |
|---|---|---|---|---|---|
| 40 Hz | +0 dB | 0/11, 0 made up | 11/11, 0 made up | 11/11, 0 made up | 11/11, 0 made up |
| 40 Hz | -6 dB | 0/11, 0 made up | 11/11, 0 made up | 11/11, 0 made up | 11/11, 0 made up |
| 40 Hz | -12 dB | 11/11, 0 made up | 11/11, 0 made up | 11/11, 0 made up | 11/11, 0 made up |
| 80 Hz | +0 dB | 11/11, 0 made up | 11/11, 0 made up | 11/11, 0 made up | 11/11, 0 made up |
| 80 Hz | -6 dB | 11/11, 0 made up | 11/11, 0 made up | 11/11, 0 made up | 11/11, 0 made up |
| 80 Hz | -12 dB | 11/11, 0 made up | 11/11, 0 made up | 11/11, 0 made up | 11/11, 0 made up |
| 120 Hz | +0 dB | 11/11, 0 made up | 11/11, 0 made up | 11/11, 0 made up | 11/11, 0 made up |
| 120 Hz | -6 dB | 11/11, 0 made up | 11/11, 0 made up | 11/11, 0 made up | 11/11, 0 made up |
| 120 Hz | -12 dB | 11/11, 0 made up | 11/11, 0 made up | 11/11, 0 made up | 11/11, 0 made up |
| 200 Hz | +0 dB | 11/11, 0 made up | 11/11, 0 made up | 11/11, 0 made up | 11/11, 0 made up |
| 200 Hz | -6 dB | 11/11, 0 made up | 11/11, 0 made up | 11/11, 0 made up | 11/11, 0 made up |
| 200 Hz | -12 dB | 11/11, 0 made up | 11/11, 0 made up | 11/11, 0 made up | 11/11, 0 made up |
| 300 Hz | +0 dB | 11/11, 0 made up | 11/11, 0 made up | 11/11, 0 made up | 11/11, 0 made up |
| 300 Hz | -6 dB | 11/11, 0 made up | 11/11, 0 made up | 11/11, 0 made up | 11/11, 0 made up |
| 300 Hz | -12 dB | 11/11, 0 made up | 11/11, 0 made up | 11/11, 0 made up | 11/11, 0 made up |
| 30 Hz | +0 dB | 0/11, 0 made up | 9/11, 0 made up | 10/11, 0 made up | 11/11, 0 made up |
| 30 Hz | +6 dB | 0/11, 0 made up | 0/11, 0 made up | 9/11, 0 made up | 10/11, 0 made up |
| 20 Hz | +0 dB | 0/11, 0 made up | 9/11, 0 made up | 9/11, 0 made up | 10/11, 0 made up |
| 20 Hz | +6 dB | 4/11, 2 made up | 5/11, 2 made up | 5/11, 1 made up | 9/11, 0 made up |
| 15 Hz | +0 dB | 0/11, 0 made up | 5/11, 1 made up | 6/11, 0 made up | 7/11, 0 made up |
| 10 Hz | +0 dB | 0/11, 0 made up | 5/11, 0 made up | 5/11, 2 made up | 6/11, 0 made up |

## The cost in sensitivity

`CQ DE W1AW K` at 18 words a minute, one seed per level,
read at a fixed pitch so the figures are about the filter. `invented`
is `CwMatchKind.Invented`.

| generated | 60 Hz | 45 Hz | 30 Hz | 20 Hz |
|---|---|---|---|---|
| 18 dB | 9/9, 0 made up | 9/9, 0 made up | 9/9, 0 made up | 9/9, 0 made up |
| 11 dB | 9/9, 0 made up | 9/9, 0 made up | 9/9, 0 made up | 9/9, 0 made up |
| 3 dB | 9/9, 0 made up | 9/9, 0 made up | 9/9, 0 made up | 9/9, 0 made up |
| 0 dB | 0/9, 0 made up | 0/9, 0 made up | 9/9, 0 made up | 9/9, 0 made up |

## The cost to a fast fist

A narrower filter responds more slowly, and at 30 words a minute a dit
is 40 ms. This is the column that decides the trade, because the
rejection column has nothing left to buy.

| speed | 60 Hz | 45 Hz | 30 Hz | 20 Hz |
|---|---|---|---|---|
| 18 wpm | 9/9, 0 made up | 9/9, 0 made up | 9/9, 0 made up | 9/9, 0 made up |
| 25 wpm | 9/9, 0 made up | 9/9, 0 made up | 9/9, 0 made up | 9/9, 0 made up |
| 30 wpm | 9/9, 0 made up | 9/9, 0 made up | 9/9, 0 made up | 9/9, 0 made up |
| 35 wpm | 9/9, 0 made up | 9/9, 0 made up | 9/9, 0 made up | 9/9, 0 made up |

## The cost to the gate's own margin

`Gate = 15` sits in the space between what an empty band scores and
what a station scores. **Narrowing the filter was measured to move
it**, so the margin is a cost column: a width that reads better and
leaves the empty band sitting on the gate has spent the one property
that has never been traded (HM-DEC-120).

| recording | holds | 60 Hz | 45 Hz | 30 Hz | 20 Hz |
|---|---|---|---|---|---|
| `cw-2026-08-20-014854` | nothing | 0.5 | 0.6 | 0.8 | 0.7 |
| `cw-2026-08-20-014935` | nothing | 0.1 | 0.1 | 0.1 | 0.2 |
| `cw-2026-08-18-004507` | a station | 6.1 | 7.0 | 8.2 | 8.6 |
| `cw-2026-08-17-013347` | a station | 19010365.9 | 17235760.0 | 10694850.9 | 1700091.2 |

The gate is **15**. A row holding
nothing must stay well under it and a row holding a station well over.

## The corpus

Characters emitted and E-share on the real captures, read at a fixed
pitch. No answer key exists for most of these, so what is shown is how
much comes out and how much of it is the letter `E`.

| recording | 60 Hz | 45 Hz | 30 Hz | 20 Hz |
|---|---|---|---|---|
| `cw-2026-08-17-013347` | 101 chars, E 44 % | 81 chars, E 63 % | 86 chars, E 59 % | 98 chars, E 46 % |
| `cw-2026-08-17-134712` | 0 chars, E 0 % | 0 chars, E 0 % | 0 chars, E 0 % | 0 chars, E 0 % |
| `cw-2026-08-18-004507` | 0 chars, E 0 % | 0 chars, E 0 % | 0 chars, E 0 % | 0 chars, E 0 % |
| `cw-2026-08-18-003758` | 0 chars, E 0 % | 0 chars, E 0 % | 0 chars, E 0 % | 0 chars, E 0 % |

## What was chosen, and why

**Forty-five hertz, which is where matching the boxcar's own main lobe
lands.**

**On the grid this unit was asked to sweep, nothing discriminates.**
Every width reads the wanted station whole at every offset from 40 Hz
out and every level down from equal. The ordered measurement returns a
tie, and a tie is an answer.

**The rows that do discriminate were added here, and that is exactly
why they did not decide it.** Below about 30 Hz of separation the
narrower filters win outright, and 30 Hz would buy the 30-and-20-hertz
cases at no measured cost to a fast fist at all. But those rows are
this session's invention, no ruling sanctions them, and fitting a
production constant to a fixture the same session wrote is the shape of
the failure §12.5 exists to stop.

**What narrowing costs, measured:**

- **the gate's margin, which is the binding one.** The empty band on
  `cw-2026-08-20-014854` climbs 6.6, 8.0, 9.3, 10.0 against a gate of
  15. Silence holds at every width, so HM-DEC-120's property is not
  traded, but the room under the gate goes from 8.4 to 5.0.
- **the corpus.** `cw-2026-08-17-013347` reads 82, 83, 79 and 49
  characters as the filter narrows, and its E-share rises from 45 % to
  53 %. Twenty hertz is plainly worse there.
- **sensitivity: nothing measured**, down to 0 dB at every width.
- **a fast fist: nothing measured**, up to 35 words a minute at every
  width, including a 75 ms integrator on a 34 ms dit. The segmental
  decoder scores a span rather than thresholding a level, so a smeared
  envelope loses contrast and keeps its timing. That is a real property
  of this architecture and it was not assumed.

**Thirty hertz is the live alternative and the choice between them is a
trade rather than a deduction.** It buys close-in rejection that 45 Hz
does not have, for 1.3 dB of gate margin and four characters on one
capture. A trade is not a session's to make (§12.1), so it is named
here and handed back.

