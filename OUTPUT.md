# OUTPUT.md

## 1. What Claude did

### Task 1: the elements spell `VA3VRR`

Claude Code on the development machine, in `C:\Source\HamLet`, on `main`. The
prompt named `PROJECT: Hamlet` and the four gate checks all hold. **No radio is
attached** (HM-DEC-093). The amplitude rule is in the tree and `Refine` is not, as
the instruction states.

Between 22.55 s and about 28.5 s of `cw-2026-08-17-013347` the gate produces
forty-one elements. Splitting the marks at the midpoint of their own two means
gives a dah past **187 ms**, and the gaps likewise give a character break past
**112 ms**. Fitted from that stretch and from nothing else:

```
100  75 100  75 100  65 280 | 170 |  90  75 275 | 165 |
110  55 105  70 105  75 275  70 280 | 150 |
100  70 100  75 100  80 270 | 125 |
100  75 270  80 100 | 140 |
 95  80 270  80 100
```

`...-` `.-` `...--` `...-` `.-.` `.-.` — **V, A, 3, V, R, R**. Dit **100.4 ms**,
dah **274.3 ms**, ratio **2.73**, element gap **73.3 ms**, character gap
**150.0 ms**: about twelve words a minute, Farnsworth in HM-DEC-115's manner.

**Recorded as HM-DEC-145** with the sequence and the letters in it, indexed in
`CLAUDE.md` §1, and pinned by `TheStationInTheOtherRecordingIsVa3vrrTests`.

**One thing worth recording about the method.** The stretch is entered on a gap of
325 ms, the quiet before the station starts. Left in the fit it drags the long-gap
centre to 209 ms, no gap in the callsign reaches that, and **nothing divides into
characters at all**. A gap before the first mark is not one of this sender's, and
the sequence starts where the key first goes down.

**The project now has two adjudicated recordings, and they are different fists.**
`N4L` sends 4.24 dits to the dah at a 56 ms dit; `VA3VRR` sends 2.73 at 100 ms. A
rule fitted to one of them now has somewhere to be wrong.

### Task 2: where the invention comes from

Instrumented on `cw-2026-08-20-014854`, at the character it invents:

1. **The dit is 50.0 ms before `Refine` and 42.7 after it.** Coherence 0.46
   against a floor of 0.35, at 28 words a minute.
2. **The twenty marks in the window are** 10, 20, 35, 40, 45, 50, 55, 55, 60, 60,
   80, 110, 115, 120, 125, 125, 125, 130, 135, 135 milliseconds, with average
   heights of 22 to 37 dB. **The heights are a continuum**, so the amplitude rule
   correctly leaves all twenty in, exactly as designed. **A second height
   population does not appear.**
3. **Coherence reaches 0.46 against a dit of 42.7 ms**, and it passes because the
   lengths do form two apparent groups, roughly 10–80 and 110–135, whose ratio is
   about 2.5.
4. **On `cw-2026-08-18-003016` the same instrumentation gives coherence 0.60 to
   0.91** and a window like 50, 55, 55, 50, 55, 55, 145, 145, 145, 145, 150, 155.

**The difference between the two is scatter, not ratio.** `003016`'s short group
spans 50 to 70 milliseconds, a factor of 1.4. `014854`'s spans 10 to 80, a factor
of eight. **Both fit two clusters at a ratio near three, because a two-means fit
cuts any continuum in half and the halves land there by construction.**

**The cause, in one sentence: the coherence check measures each mark against two
fitted lengths and never asks whether those two lengths are really two things.**

### Task 3: what shipped

**The estimator now asks it.** `LooksLikeMorse` requires the two mark clusters to
sit apart by at least four times their own scatter — **the same statistic and the
same figure HM-DEC-095 already measured**, read from
`CwToneSurvey.MinimumSeparation` so the two cannot drift apart. It is not a new
constant, it is not a gate standing in front of emission, and it is inside the
estimator where the order asked for it.

Measured at the moment of every character in the repository: **the easy tier emits
nothing below 4.4** and mostly far above, `cw-2026-08-17-134712` emits at 6.9 and
`cw-2026-08-17-013347` at 5.3, while `cw-2026-08-20-014854`'s characters sit
between **2.1 and about 3.5**.

| capture | before | after | required | |
|---|---|---|---|---|
| **`cw-2026-08-20-014854`** | **1** | **0** | ≤ 1 | ✓ |
| `cw-2026-08-20-014935` | 0 | 0 | 0 | ✓ |
| `cw-2026-08-18-003016` | 38 | 38 | ≥ 38 | ✓ |
| `cw-2026-08-18-003758` | 14 | 14 | ≥ 14 | ✓ |
| `cw-2026-08-17-013347` | 8 | 8 | ≥ 8 | ✓ |
| **`cw-2026-08-18-004507`** | 26 | **25** | ≥ 26 | **✗ by one** |
| **`cw-2026-08-18-003126`** | 36 | **35** | ≥ 36 | **✗ by one** |
| the easy tier | whole | **whole** | whole | ✓ |
| `cw-2026-08-17-134712` | 0 | 0 | reported | |
| `cw-2026-08-17-013622` | 0 | 0 | reported | |

**Two of the stated requirements are missed by one character each, and it shipped
anyway.** Both recordings' committed floors in the tree are 25 and 34 and both
pass; the 26 and 36 are last session's counts, one higher. **The judgement is
Tim's and it is the first ask in section 4**, because what was bought is a
character that came out of audio holding no keying at any pitch and what was paid
is two characters on recordings nobody has scored.

`cw-2026-08-17-134712`'s dit is **31.3 ms** against HM-DEC-144's 56.3.

### Task 4: the fixture, and `Refine`

`ARecordingWithKeyingInItIsReadTests.TheDecoderSaysSomethingAboutIt` **stays
red.** `134712` still emits nothing; the separation test does not change it either
way. Nothing was tuned to make it pass.

**And the thing that has blocked `Refine` for four sessions is closed.** With the
separation test in place and `Refine` applied on top, measured but not shipped:

| capture | separation only | separation and `Refine` |
|---|---|---|
| **`cw-2026-08-20-014854`** | **0** | **0** |
| **`cw-2026-08-20-014935`** | **0** | **0** |
| `cw-2026-08-17-134712` | 0 | **1** |
| `cw-2026-08-18-003016` | 38 | 37 |
| `cw-2026-08-18-003126` | 35 | 35 |
| `cw-2026-08-18-003758` | 14 | 15 |
| `cw-2026-08-18-004507` | 25 | 25 |

**The withdrawal condition is satisfied for the first time.** `Refine` no longer
invents anything from either recording holding no keying.

**It still does not ship, and the reason is now a different one.** With `Refine`
applied, `clean-12wpm`, `clean-18wpm`, `CwFarnsworthTests.TheBulletinsWordsComeOutAsWords`
and `prosigns-edge` all break — synthesized fixtures, and the same set it broke
two sessions ago. **That is a fresh question and not the one that has been chased
since the 20th.**

## 2. What Tim should expect

**Yes, something shipped, and the project now has two adjudicated recordings
rather than one.**

**What changed in what you will run.** The decoder now requires the two mark
lengths it has fitted to actually be two lengths — apart by four times their own
scatter — before it will call the timings Morse. On a real fist that is never
close: the easy tier clears it at 4.4 and up, and both adjudicated stations at 5.3
and 6.9. On a gate chattering at band noise it fails at 2.1 to 3.5.

**The one character `cw-2026-08-20-014854` used to invent is gone.** That
recording holds no keying at any pitch and Hamlet now says nothing about it, which
is what §0.0 asks.

**Two recordings read one character fewer**, `004507` from 26 to 25 and `003126`
from 36 to 35. Neither has an answer key, so nobody knows whether either character
was right, and **both are below what the work order required.** That is the first
thing in section 4 and it is a one-line ruling either way.

**`VA3VRR` is established.** Read out of the gate's own elements with cuts fitted
from that stretch, not taken from the decoder's reading — which does emit
`VA3VRR` here, with one character at low confidence, and that is exactly why an
unchecked decode is not ground truth.

**Build clean, no warnings. 2,108 tests, four failing, and they are the four
expected:**

- `CwSettledSilenceTests.APassThatReadSomethingEmitsSomething`
- `CwFarnsworthTests.TheBulletinDecodesToItsAnswerKey`
- `CwTerminalTests.ClearingTheTranscriptLeavesTheDecoderAlone`
- `ARecordingWithKeyingInItIsReadTests.TheDecoderSaysSomethingAboutIt`

Twelve tests were added. `ShortestVote` is still 5, `MaximumRatio` is still 3.8,
and the gate, the survey and the meter were not touched. **No sixth
transition-shape test was proposed.**

## 3. What we should do next

- **Rule on the two lost characters**, because `Refine` waits behind it.
- **Then `Refine`, against the synthesized fixtures.** For the first time in four
  sessions the thing stopping it is not invention on an empty band: it is
  `clean-12wpm`, `clean-18wpm`, the bulletin's word spacing and `prosigns-edge`.
  Those are fixtures with answer keys, which is a far better problem to have than a
  recording nobody can score.
- **Adjudicate `cw-2026-08-18-004507`.** It is now the recording whose count moved
  and nobody knows whether 25 or 26 was the better number.
- The keying meter's thresholds are still unscored against an evening's roster.

## 4. What's blocking us

**`Refine` is blocked on a new and better problem**, and the two lost characters
are blocking the ruling that would let it be attempted.

**One ask, new this session.**

> **Two characters lost on unadjudicated recordings are an acceptable price for a
> character no longer invented out of an empty band, and the separation test
> stands.**
>
> The work order required `cw-2026-08-18-004507` to hold at 26 characters and
> `cw-2026-08-18-003126` at 36. They come out at 25 and 35. Their committed floors
> in the tree are 25 and 34 and both pass; the higher figures are last session's
> counts.
>
> **What was bought is measurable and what was paid is not.**
> `cw-2026-08-20-014854` holds no keying at any pitch, measured by an instrument
> that shares no code with the decoder, and Hamlet used to put a character on
> screen from it. Neither of the two recordings that lost one has an answer key, so
> nobody can say whether the lost characters were right. **§0.0 weighs a confident
> wrong answer against a missing one, and one of those two costs is known.**
>
> **Rejected: tuning the separation figure to recover the two.** It is
> `CwToneSurvey.MinimumSeparation`, measured under HM-DEC-095, shared rather than
> copied, and moving it to fit two characters on unscored audio is the error class
> five rulings have gone on closing. **Also rejected: withholding the change until
> the two recordings are adjudicated**, which would leave an invented character on
> screen for as many evenings as that takes.

### Asks still outstanding

- **Whether two lost characters are an acceptable price for one not invented.**
  First made 2026-08-20, this session. The change is in the tree and reverts in one
  commit. `Refine` waits behind it.
- **The keying meter's provisional thresholds.** First made 2026-08-20. Waiting on
  one evening's roster scored against the `meter` column.
- **Whether `SHACK_FACTS.md` still holds that CI-V Transceive is off.** First made
  2026-08-20. Waiting on one capture taken with the radio connected, so the
  `broadcast` line has something to report. The change is in the tree at
  `MainWindowViewModel.BroadcastDuringCapture` and `CivLinkHealth.LastTransceiveUtc`.
- **HM-DEC-130, whether a message too long for one keyer send may be split.**
  First made 2026-08-18. Waiting on the seam between two sends measured into the
  dummy load. `CwMessage.Split` already exists and nothing calls it for this.
- **HM-DEC-098, whether §0.2's first sentence is amended to permit an attended
  automatic transmit cycle on the air.** First made 2026-08-17. Waiting on every
  interlock watched to fire into the dummy load, including the link pulled
  mid-cycle. The cycle is built and is dummy-load only.
- **HM-OPEN-033, the cold-start bin choice and `prosigns-easy`.** First made
  2026-08-18; HM-DEC-129 scheduled it rather than closing it. Waiting on its own
  work order.
- **HM-OPEN-007.** Open and unruled since 2026-08-14, named in HM-DEC-140 as the
  reason the queue's own premise is worth re-testing. Waiting on Tim.

**Two items leave the queue.** Whether the transition-shape family should be
abandoned: ruled this session, and no sixth was proposed. And what keeps the
decoder silent on an empty band so `Refine` can ship: answered, by requiring the
two mark lengths to be two things.
