# 1. What Claude did

**STATE.** Development computer, `C:\Source\HamLet`, Claude Code surface (§9.5).
**Branch: `main`, and nowhere else.** The prompt claimed `PROJECT: Hamlet` and the
tree confirms it: `CLAUDE.md`'s header reads `Project: Hamlet`, the solution is
`Hamlet.sln`, the namespaces are `Hamlet.*`. Gate passed. **Nothing in this report
is evidence about the radio** (HM-DEC-093): every number comes from a fixture, a
generated signal, or one off-air recording decoded here.

**Nothing was recorded under §12.1.** Four questions came up and all four are in
section 4.

**All five phases completed. Nothing was dropped.**

**No transmit work of any kind was done and nothing was built toward auto-CQ.**

## Phase 1, the floor is 14 (HM-DEC-120)

One line, and the sweep behind it. `RefusalFloorDb` moves from seventeen to
fourteen, superseding HM-DEC-117's interim.

Seventeen was arithmetic from the offset between the broadband ratio a fixture is
generated at and what the decoder reads inside its own tone filter, and it was
expected to bite at HM-DEC-097's zero decibel line. **It bit at about five**, so
four decibels of reach were given away for nothing.

Re-swept at fourteen, and the table is what the ruling predicted:

```
 SNR    correct   invented
  5 dB     1.00       none
  4 dB     1.00       none
  3 dB     1.00       none
  2 dB     0.97       none
  1 dB     0.81       none
 -1 dB   silence      none
```

**Nothing invented at any level, the whole message down to three decibels, and
silence from minus one.** The four decibels are back and the property HM-DEC-097
exists for is intact. The test that pinned seventeen now pins fourteen and says
why it moved.

## Phase 2, the detector's edges characterised (HM-DEC-119's commission)

Hamlet's own `CwToneTracker` and `CwGate`, wired as the decoder wires them, over
synthesized marks of exactly known length. **The measurement is the deliverable
and nothing in the decoder was changed.**

| wpm | true mark | gate reads | start err | end err | length err |
|---|---|---|---|---|---|
| 12 | 100 ms | 106.5 | +28.8 | +35.3 | **+6.5** |
| 12 | 300 ms | 305.6 | +29.4 | +35.0 | **+5.6** |
| 25 | 48 ms | 51.3 | +32.5 | +35.7 | **+3.3** |
| 25 | 144 ms | 147.5 | +30.2 | +33.7 | **+3.5** |
| 30 | 40 ms | 43.3 | +32.2 | +35.5 | **+3.3** |
| 30 | 120 ms | 125.0 | +30.0 | +35.0 | **+5.0** |

**Both edges are late by about thirty to thirty-six milliseconds**, which is three
quarters of the forty millisecond window and is group delay: it is the same at both
ends, so it cancels for a length. What survives is three to seven milliseconds,
**under one hop, and it does not grow as the marks shorten.** The gate is unbiased,
which is what HM-DEC-119 already ruled, now confirmed through the instrument that
matters. The gaps mirror it exactly, short by four to six milliseconds at every
speed, because a mark and the silence after it are complementary.

**The window is where the answer is, and it is not an edge correction.** Told ten
words a minute the tracker uses a fifty millisecond window and thirty words a
minute collapses outright: the window is longer than the dit, runs merge, and the
start error jumps from thirty milliseconds to a hundred and seventy. Told
twenty-five or more it uses twenty, and **both speeds then read better than at the
forty it acquires with**. At 25 the dah reads 144.4 against a true 144, and at 30
the dit reads 38.5 against a true 40.

That is the clue in the brief resolved: 25 decoded exactly and 30 collapsed
because of which window each was running, not because of anything at the edges.

One correction to my own probe, recorded because it looked exactly like a finding:
pairing detected runs to true marks by overlap silently dropped every dit at 25 and
30, because a short mark's own midpoint falls before the delayed run representing
it. The gate sees them. The probe pairs in order now and says so in-code.

**The recommendation is in section 4.**

## Phase 3, the six short fixtures rebuilt (§12.5)

**Thirteen failures become five.**

The finding it rests on: **a fixture shorter than the detector's acquisition tests
acquisition rather than decoding.** Every message here is under five seconds, so
the part under test was competing for the seconds the decoder needed to find it at
all, and the characters lost were always the opening ones.

**The run-up length was measured, not picked.** Swept from nothing to eight groups
of `VVV` at four speeds: 25 words a minute fails bare and passes from one group
onward; 18 and 30 pass at every length; **12 passes at nought and one, then fails
from two onward and never recovers.** One group is the least that satisfies the
acquisition and the most that leaves twelve alone. Choosing more would have buried
a real decoder fault, and that fault is in section 4 instead.

A signal off frequency needs longer to find than one on it, so the wrong-pitch
tests get three groups, measured the same way: 500 and 750 Hz need three, 875 needs
one, and **400 never passes at any length**, which is phase 5's subject and is left
failing rather than hidden.

**On the disk fixtures the run-up goes on the easy tier and nowhere else.** Applying
it to all three tiers was tried and measured: the easy tiers held, and two working
tiers fell through the reference gate, `coverage-working` from 52% to 8% and
`tightfist-working` from 73% to 36%. Rebuilding a fixture that was not failing is
churn that invalidates a reference score for nothing (§12.6).

And **not on `prosigns`**, which a run-up breaks outright: the tone survey stops
finding the tone at all and the decode goes empty. A prosign is one long unbroken
symbol, so `VVV` in front gives the mark-length clustering that separates keying
from a carrier (HM-DEC-095) one smear rather than two groups.

**HM-DEC-101's gate was re-run after every regeneration and reports zero fixtures
the reference cannot read.** Scores now on disk:

| fixture | reference | fixture | reference |
|---|---|---|---|
| coverage-easy | 96% | coverage-working | 52% |
| exchange-easy | 100% | exchange-working | 53% |
| fast-easy | 100% | fast-working | 58% |
| prosigns-easy | 75% | prosigns-working | 75% |
| tightfist-easy | 100% | tightfist-working | 73% |

Every working tier is back to the score it had before. **No fixture was retired.**

**One hold-out adjudicated individually with its reason recorded**, per §12.5:
`ClearingTheScreenLeavesTheDecoderAloneOnRealisticAudio` asserted the tone was
identical before and after, and began failing when the longer run-up let the
tracker refine 625 Hz down to the 600 actually sent. It now allows one bin, because
forbidding refinement is not what that test is about.

**The `IR` for `AR` fault was chased separately as instructed, and it is not a
decoder fault at all.** It is in section 4 and it is the most interesting thing
this session found.

## Phase 4, the path behind HM-DEC-116 (HM-DEC-121)

Traced on the capture that fails, with adoption applied and then reverted.

| | retunes | settles on | settled pass reads |
|---|---|---|---|
| without adoption | **1** | 610 Hz | `■■■ ■■VA3VRR` |
| with adoption | **3** | 625 Hz | `■■■ ■` |

**The standing hypothesis is wrong.** The settled pass does not take the dit hint
from the estimator in a way adoption can move: `Recompute()` derives the dit from
the mark clusters and `ShortestGap()`, and reads none of the gap cuts. Adoption
cannot reach it that way.

**The path runs through the tone tracker.** The decoder sets `MidCharacter` from the
streaming pass's own segmentation, and the tracker reads it to decide when a held
retune may be released. Adopted gap classes change where characters divide, which
changes when the pattern is empty, which changes when the tracker is allowed to
move. **And every tracker switch calls `_settled.Reset()`**, because a switch means
somebody else started transmitting, so two extra retunes on a thirty-second capture
is enough to lose the callsign. The settled pass is also fed the envelope measured
at whatever pitch the tracker is on, so a different retune history is different
audio.

**Reported and not shipped**, as the phase required. HM-DEC-116 stays blocked.
Recorded as HM-OPEN-027.

## Phase 5, the 400 Hz tracker

**The tracker always finds the pitch.** Told to start at 600 and given a signal at
400, it reports 400 at the end of every run. What differs is how many steps it takes
to get there.

One signal at 400 Hz, varying only where the tracker was told to start:

| told | retunes | decode |
|---|---|---|
| 300 Hz | 3 | broken |
| 350 Hz | **1** | good |
| 400 Hz | **1** | perfect |
| 500 Hz | 3 | broken |
| 550 Hz | 3 | broken |
| 600 Hz | 3 | broken |
| 700 Hz | **1** | good |
| 900 Hz | **1** | good |

**One retune decodes and three does not**, and it is not distance: starting three
hundred hertz above works while starting one hundred above does not. The cliff the
test finds at 400 is an artifact of its 600 Hz start, and the same test passes at
425 because 425 is 175 hertz away rather than 200.

**Two investigations converged on one cause**, which is worth saying plainly: phase
4 traced a decode failure to extra retunes and phase 5 traced a different decode
failure to extra retunes. Recorded as HM-OPEN-028. **Not fixed**, because the cause
is characterised but the mechanism producing three retunes is still a hypothesis,
and the brief said to fix only what is unambiguous.

# 2. What Tim should expect

- **Build succeeds, no warnings.**
- **1817 tests, 5 failing.** 1397 of 1401 pass in the engine, 415 of 416 in the app.
- **Thirteen failures became five, and nothing regressed.** The five are
  `ASignalAtTheWrongPitchIsStillFound(400)`, `ClearingTheTranscriptLeavesTheDecoderAlone`,
  `TheBulletinDecodesToItsAnswerKey`, `TheEasyTierIsReadWhole(exchange-easy)` and
  `TheEasyTierIsReadWhole(prosigns-easy)`.
- **What will look wrong and is not.** Two of the five are a known baseline that is
  meant to stay red until the fixture generator is fixed. `prosigns-easy` and
  `exchange-easy` both fail because **the fixture is wrong, not the decoder**, and
  section 4 has the proof. `TheBulletinDecodesToItsAnswerKey` is the long-standing
  bar on a real recording. `ASignalAtTheWrongPitchIsStillFound(400)` is left failing
  on purpose rather than hidden behind a longer run-up.
- **What is different at the radio.** The decoder now refuses four decibels lower
  than it did, which is the only change an operator would notice. Everything else
  this session was measurement, fixtures and records.
- **What did not change and might have been expected to.** No edge correction was
  applied to the detector, because the measurement says the edges are not the
  problem. HM-DEC-116 was not shipped. The 400 Hz failure was not fixed.
- **Nothing is tuned to the off-air recording.** No decoder parameter was moved to
  suit `cw-2026-08-17-013347` or any other capture.
- **Six commits, pushed to `main`.** Nothing local, no branches.

# 3. What we should do next

- Rule on the analysis window, section 4 item one. It is phase 2's commission
  answered and it is the one that unblocks thirty words a minute.
- Rule on the tracker retunes, section 4 item two. One ruling settles HM-OPEN-027
  and HM-OPEN-028 together, which is two of the five remaining failures.
- Fix the fixture generator's caret, section 4 item three, as its own work order
  with the gate re-run and each hold-out adjudicated. It clears the other two
  remaining failures and it is small.
- Re-measure the bulletin afterwards. It is the only failure none of the above
  touches, and it should be looked at against a fixture set that is known good.
- Still outstanding and untouched: `cw-2026-08-18-003758` is not on the machine
  (HM-OPEN-026).

# 4. What's blocking us

---
date: 2026-08-18
refs: CLAUDE.md §0.0, §12.1; HM-DEC-119
---

**The analysis window follows the speed estimate, and no edge correction is applied
to the detector.**

This is HM-DEC-119's commission answered. The three candidates it named were a
shorter edge window, sub-hop interpolation, or nothing at all. **The measurement
says nothing at all, on the edges.** The gate's length error is three to seven
milliseconds, under one hop, and it does not grow as the marks shorten. There is
no bias there worth correcting, and correcting one that is not there is what took
the suite from thirteen failures to twenty-nine last session.

**The window is a different matter and it is where the speeds live or die.** The
tracker picks its window from the speed it currently believes. Told ten words a
minute it uses fifty milliseconds, and at thirty words a minute that window is
longer than the dit: runs merge and the start error goes from thirty milliseconds
to a hundred and seventy. Told twenty-five or more it uses twenty, and both 25 and
30 then read better than they do at the forty it acquires with. The dah at 25 reads
144.4 against a true 144, and the dit at 30 reads 38.5 against a true 40.

So the fault is that the window is chosen from an estimate that is wrong exactly
when it matters most, at acquisition, and a decoder that has not yet found the
speed is running the window least able to find it.

Rejected: a shorter edge window, which corrects a bias the measurement says is not
there. Rejected: sub-hop interpolation, for the same reason and at a higher price.
Rejected: changing the window here. It changes acquisition behaviour on every
signal including real ones, and §12.1 puts anything governing what the display
asserts with you without exception.

---
date: 2026-08-18
refs: CLAUDE.md §0.0, §12.1; HM-DEC-116; HM-DEC-121; HM-DEC-096
---

**A tracker retune stops destroying the settled window, or the segmentation stops
steering the tracker. One of the two, and the choice is yours.**

Two independent investigations reached the same place. HM-OPEN-027: adopting the
settled pass's gap classes changes where characters divide, which changes
`MidCharacter`, which changes when the tracker may release a held retune, so one
retune becomes three and the callsign is lost. HM-OPEN-028: a signal at 400 Hz
found from a starting pitch one to two hundred hertz away also takes three retunes
instead of one, and is also unreadable. **One retune decodes and three does not, in
both cases, for two entirely different reasons.**

The cost is paid in one line: every tracker switch calls `_settled.Reset()`.

Two directions, and they are not equivalent:

**Stop the segmentation steering the tracker.** The streaming pass's idea of where
a character divides currently gates when the tracker may move. That is a
provisional judgement governing a measurement, which is backwards. It would settle
HM-OPEN-027 directly and leave HM-OPEN-028 alone.

**Stop a switch resetting the settled window unconditionally.** This is the one
that would settle both, and it acts against HM-DEC-096 phase 3, which reset on a
switch because a switch usually does mean a different station started transmitting.
Distinguishing a retune that found the same station more precisely from one that
followed a new station is a real question and not a refactor.

Rejected: shipping HM-DEC-116 on this session's judgement, which the phase forbade
by name. Rejected: fixing the 400 Hz case, because the mechanism producing three
retunes is characterised but not proven, and the brief said to fix only what is
unambiguous.

---
date: 2026-08-18
refs: CLAUDE.md §12.5; HM-DEC-101
---

**The fixture generator's caret is fixed, the prosign fixtures are regenerated
through HM-DEC-101's gate, and each hold-out is adjudicated individually.**

**Hamlet reads `IR` where `AR` was sent because the fixture sent `IR`.** Chased on
the instruction to chase it, and it is §12.5's exact pattern: a fixture built from
a misunderstanding, with the decoder taking the blame for months.

Measured off `prosigns-easy.wav` itself, against the intended `^BT N0CALL ^AR ^SK`:

| word | intended | rendered as | reads |
|---|---|---|---|
| `^BT` | `-...-` | dit, character gap, dit dit dit dah | `EV` |
| `^AR` | `.-.-.` | dit dit, character gap, dit dah dit | `IR` |
| `^SK` | `...-.-` | correct | `SK` |

**`EV` and `IR` are exactly what the reference implementation reads**, which is the
confirmation rather than a coincidence: two independent decoders agree, and they
are both right. The audio genuinely says `EV` and `IR`.

The path is arithmetic. `KeyEdges` opens with a single unpaired edge at the message
start. The caret's join branch begins by adding a gap edge, which assumes a mark is
in progress to separate from; at the head of a word there is not, so that edge
closes a mark that never opened. **A phantom hundred-millisecond dit, and every
edge after it on the opposite parity.** The dah that should open `BT` becomes a
three-hundred-millisecond gap and the element gaps become marks. Predicted against
the measurement, the model is exact at all nine edges of `^BT`.

`^SK` survives because its two letters carry six elements rather than five, and the
branch's trailing-gap removal restores the parity the opening edge broke. **An
even-length prosign renders correctly and an odd-length one does not**, which is
why this has looked like an intermittent decoder fault.

`exchange-easy` is very likely the same defect and should be re-checked after the
fix rather than investigated separately.

Rejected: fixing it in this session. It changes what three fixtures assert, so it
needs the gate re-run and every affected hold-out adjudicated with a recorded
reason, which is the discipline phase 3 was held to and is not a tail on another
work unit. Rejected: leaving it recorded as a decoder failure, which is what it has
been until now.

---
date: 2026-08-18
refs: CLAUDE.md §12.5; HM-OPEN-026
---

**`cw-2026-08-18-003758` is still not on the machine.**

Unchanged from last session and untouched by this one. It is named in the fixture
records and the file is absent, so anything asserted about it is unverifiable.
Either it is supplied, or the reference to it is removed so the fixture set stops
naming evidence that does not exist.
