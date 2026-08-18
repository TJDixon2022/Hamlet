# 1. What Claude did

**STATE.** Development computer, `C:\Source\HamLet`, Claude Code surface (§9.5).
**Branch: `main`, and nowhere else** (§9.5.1). The prompt claimed `PROJECT: Hamlet`
and the tree confirms it: `CLAUDE.md`'s header reads `Project: Hamlet`, the
solution is `Hamlet.sln`, the namespaces are `Hamlet.*`. Gate passed. **Nothing in
this report is evidence about the radio** (HM-DEC-093): every number comes from a
fixture, a generated signal, or one of the two off-air recordings decoded here.

**Nothing was recorded under §12.1.** Two questions came up and both are in
section 4.

**All five phases completed. Nothing was dropped.** No transmit work of any kind,
and nothing was built toward auto-CQ or toward HM-DEC-123's work order.

## Phase 1, two analysis windows during acquisition (HM-DEC-122) — built, measured, **not live**

Built exactly as ruled and it does not survive its own measurement, so it is not
in the tree. The full trace is HM-OPEN-030; the short version is three
measurements.

**The candidates name flukes for several seconds.** On a clean signal at 640 Hz,
eighteen decibels over the noise, the twenty millisecond candidate's answer across
the first ten survey reads runs 325, 325, 325, 550, 550, 725, 725, 725, 650, 650
hertz. This is the fault the tracker's own two-agreeing-surveys rule exists to
prevent (HM-DEC-095), and the candidates were not subject to it.

**Settling on the first answer meets the ruling's acceptance and breaks §0.0.**
Tuned onto mid-transmission with no run-up, 25, 28, 30 and 35 words a minute go
from 0.67, 0.63, 0.70, 0.63 of the message to 0.79, 0.79, 0.95, 0.89 — and
`NothingIsEmittedAnywhereBelowTheFloor` fails, with 2.8% of what comes back below
the refusal floor never having been sent, where HM-DEC-120 measured zero at every
level. `ASignalAtTheWrongPitchIsStillFound(875)` fails outright. **So a short
window taken early does not only cost sensitivity**, which is the premise the
ruling's tie-break rests on.

**Requiring the clock to belong to the confirmed station fixes both regressions
and leaves nothing behind.** With that gate, every cell of the matrix — nine
speeds, five ratios, with and without a run-up — is identical to the unmodified
decoder. The window cap on its own is likewise a no-op: it never binds.

**And where the gate does fire in time, it costs the only real recording seven
characters.** On the ARRL bulletin the analysis window settles at twenty
milliseconds and the settled pass falls from `JJ AOT NET ■I ECH STAAION HAND■
AHIS MESAGE P` to `T■E ECH STAAION HAND■ AHIS MESAGE P`, **36 of 47 to 29 of 47**.
Isolated by disabling the settle alone, which returns it character for character.
**Only the short candidate yields a clock there**, so the tie-break is not even in
play: the fifty millisecond window smears a 57 ms dit badly enough to fail the
cluster test while being the better window to read through, and the forty
millisecond window the ruling removes from consideration is better than either.

What shipped from this phase is the measurement. `CwAcquisitionWindowTests` pins
the bare fast end, the same fist with a run-up, and the slow end.

## Phase 2, the fixture generator's caret (HM-DEC-124) — fixed

The caret had a branch of its own whose first act was to add a gap edge, assuming
a mark was in progress to separate from. At the head of a word there is not, so
that edge closed a mark that never opened. Modelled independently before the code
was touched, and the model reproduces `EV N0CALL IR` exactly, which is what both
Hamlet and the reference read off the audio.

The caret now changes one thing and nothing else: which gap separates the letters.
The separate branch is gone.

**HM-DEC-101's gate was re-run over the whole set after every regeneration.**

| fixture | reference before | after |
|---|---|---|
| prosigns-easy | 75% `EV N0CALL IR <SK>` | **100% `<BT> N0CALL <AR> <SK>`** |
| prosigns-working | 75% | 83% |
| prosigns-edge | 83% | 83% |

Everything else is unmoved and **no fixture is held out**. Three `.wav` files and
three sidecars changed and nothing else did.

**Two things were re-tested rather than taken on trust, per §12.5.** The run-up
exclusion on the prosigns fixture rested on a reason that no longer holds — a
correctly rendered `^BT` is `-...-`, whose marks are the same two lengths as
`VVV` — so it was measured again: the reference reads the run-up version at 100%
and Hamlet emits a single placeholder, so the exclusion stands and what stands
behind it is now a decoder finding (HM-OPEN-031). And `exchange-easy` was
re-checked after the fix rather than investigated separately, as instructed: **it
is not the same defect.** That fixture has no caret in it.

## Phase 3, the bulletin re-measured — **unmoved at 36 of 47**

Reported before anything else was touched, as required.

```
got    'JJ AOT NET ■I ECH STAAION HAND■ AHIS MESAGE P'
wanted 'AT ARRL DOT NET <BT> EACH STATION HANDLING THIS MESSAGE P'
36 characters against 47
```

Character for character what it read last session. The caret was a generator fault
and touched no real audio, and phase 1 was held back, so nothing that shipped
could have moved it and nothing did. **That is the finding the work order asked
for.** `T` is still read as `A` in `STATION` and in `THIS`, unchanged.

Phase 4 then moved it, after the measurement was taken: `OT NET ■I ECH STAAION
HAND■ AHIS MESAGE P`. **Three characters shorter and the same number correct** —
what went was the invented `JJ` and one wrong `A`. Nothing was tuned to this
recording.

## Phase 4, `ClearingTheTranscriptLeavesTheDecoderAlone` — diagnosed and fixed

**The test is not about clearing.** Every assertion about the decoder surviving a
clear passes and always did. It fails on the decode afterwards, and the decode
read `■ B■AW K` where `CQ DE W1AW K` was sent. `DE` read as `B` is `-..` and `.`
run together: a character gap read as an element gap. The same substitution is
what `exchange-easy` had been failing on for sessions.

**The cause, traced to the line.** `CwGapFit` carries the note "one
implementation, read by both passes, because two copies of a classifier is two
classifiers", and there were two. The settled pass used it; the streaming
estimator had its own, which split the gaps in two and then split the long half
again. **A two-way split of three heaps lands wherever the window's mixture puts
it.** On `exchange-easy` — textbook spacing at twelve words a minute, element gaps
100 ms, character 295, word 695, three heaps a hand could separate — the first cut
wandered from 189 to 414 milliseconds across one message, and wherever a couple of
word gaps crowded into the twenty-gap window it converged on the split between
*character and word* rather than between *element and character*.

The streaming estimator now reads `CwGapFit`. Two guards were needed and both were
measured rather than reasoned:

- **The element class has to be the crowded one**, tested at the call site rather
  than inside the fit. The gate flaps at the onset of the very first mark and
  leaves gaps of 25, 35 and 65 milliseconds behind; while those sit in the window
  the fit gives them a class of their own and reads every real element gap as a
  character gap. Putting the same test inside `CwGapFit`, where the settled pass
  would see it, costs the callsign on `cw-2026-08-17-013347`.
- **A lone gap far above everything else is a pause and not a class**, tested
  inside the fit because it is about the data. A looping signal pauses two seconds
  between repeats and that one silence took the whole top class: word gaps of 680
  then shared a class with character gaps of 290 and every space between words
  disappeared.

**What it bought:**

| | before | after |
|---|---|---|
| `exchange-easy` | `VVCQCQBN0CALLN0CALLK` | **reads whole** |
| bare fist, 25 / 28 / 30 / 35 wpm | 0.67 / 0.63 / 0.70 / 0.63 | **0.89 / 0.89 / 0.89 / 0.88** |
| slow end with a run-up | 0.89 | **0.95 to 1.00** |
| `ClearingTheTranscript…` | `■ B■AW K` | `■ DE W1AW K` |
| bulletin, settled | 36 correct, `JJ` invented | 36 correct, nothing invented |
| `013347`, streaming | `■   ■<SK>3VRR` | `■    ■VA3VRR` |
| `013347`, settled | `■■■ ■■VA3VRR` | `■■■ ■` |

**And what it cost, both recorded and adjudicated individually** (HM-OPEN-032).
`TheSettledPassNoLongerStopsShortOfTheCallsign` is red: changing where the
streaming pass divides characters moves `MidCharacter`, one retune becomes three,
`_settled.Reset()` runs twice more and the window is thrown away before it reaches
the callsign. **That is HM-OPEN-027's coupling exactly and HM-DEC-123 is the
ratified fix for it**, in its own work order, which this session did not begin.
The callsign did not leave the screen; it moved to the other pass, and the reading
it moved from was `<SK>` where `VA` was sent. `tightfist-easy` gains one
placeholder, which is not the outlier trim — tested with the trim disabled and
unchanged.

**Why it shipped with those two red.** `DE` read as `B` is a wrong character at
full confidence, in the two commonest letters on the band, at every speed a
beginner will meet and at every ratio down to three decibels. What replaces it is
a placeholder in one pass on one recording, and a placeholder asserts nothing.
§0.0 decides that one way.

## Phase 5, the record housekeeping — done, not dropped

**HM-OPEN-026.** Swept the whole tree for `cw-2026-08-18-003758`. It appears in
its own open issue, in the previous `OUTPUT.md`, and in the work order that asked
about it. **No test, no sidecar, no catalogue entry and no assertion refers to
it**, so nothing in the fixture set rests on a file that is absent and the
property the work order wanted is already true. **The recommendation is to close
it** — four sessions, no file — and it is a recommendation rather than a decision,
because closing it costs the project the regression test for a success it has
never had.

**HM-OPEN-025.** Confirmed still cosmetic. `20c8ae5` is the only one-word `save`
in the whole log, its diff is five files and 430 insertions of that session's own
work, and all of this session's commits kept the messages they were written with.

**HM-OPEN-027 and HM-OPEN-028** now say on their face that HM-DEC-123 rules them
and that the work is its own order, so the next session reads it rather than
deriving it a third time.

# 2. What Tim should expect

- **Build succeeds, no warnings.**
- **1829 tests, 6 failing.** 1408 of 1413 in the engine, 415 of 416 in the app.
  Twelve tests are new, all of them measurement.
- **The failing six, named:**
  - `ASignalAtTheWrongPitchIsStillFound(400)` — unchanged, belongs to HM-DEC-123.
  - `ClearingTheTranscriptLeavesTheDecoderAlone` — much improved and still red:
    `■ DE W1AW K` against `CQ DE W1AW K`, where it read `■ B■AW K` before.
  - `TheBulletinDecodesToItsAnswerKey` — the long-standing bar on a real recording.
  - `TheEasyTierIsReadWhole(prosigns-easy)` — **the same test, a different cause**:
    the prosigns now read correctly and the four opening characters are lost to
    acquisition on the one easy-tier fixture that cannot carry a run-up.
  - `TheSettledPassNoLongerStopsShortOfTheCallsign` — **new**, section 1 phase 4.
  - `TheEasyTierIsReadWhole(tightfist-easy)` — **new**, one placeholder.
- **`TheEasyTierIsReadWhole(exchange-easy)` is green** for the first time since
  HM-DEC-114 turned it into a pass-or-fail.
- **What will look wrong and is not.** Two red tests are new and the count went
  from five to six; both are recorded with their reasons and neither was tidied
  away by moving a bar. HM-DEC-122 is ratified and is not in the tree, which is
  deliberate and is section 4's first item. The prosigns fixtures' `.wav` files
  changed, which is the caret fix and is what took the reference from 75% to 100%.
- **What is different at the radio.** `DE` no longer reads as `B`, which is the
  one change an operator would notice immediately, and a station tuned onto
  mid-transmission at twenty-five words a minute or faster now arrives at about
  nine characters in ten rather than six.
- **One intermittent seen once and recorded.** `TheStopFrameIsCommand17CarryingFf`
  failed in one full run and passed alone and in the two runs either side. Named
  in HM-OPEN-024 beside the other intermittent and not chased (§12.6).
- **Nothing is tuned to any recording.** No decoder parameter was moved to suit
  `cw-2026-08-18-004507` or `cw-2026-08-17-013347`.
- **Six commits, pushed to `main`.** Nothing local, no branches. The first of them
  carries the uncommitted `CLAUDE.md` and `CLEANUP_BRIEF.md` changes that were in
  the working tree when the session opened.

# 3. What we should do next

- Rule on HM-DEC-122, section 4 item one. It is ratified and not live, and that is
  the one thing in the tree that does not match the record.
- Run HM-DEC-123's work order. It is now blocking three separate things: the
  400 Hz test, HM-DEC-116, and the settled pass on `cw-2026-08-17-013347`.
- Then re-measure the bulletin again. Phase 4 moved it for the first time in three
  sessions and HM-DEC-123 will move it again.
- `tightfist-easy`'s placeholder and `prosigns-easy`'s four opening characters, in
  that order. Both are small and both are HM-DEC-114's bar.
- Decide HM-OPEN-026 either way, so the question stops being asked.

# 4. What's blocking us

---
date: 2026-08-18
refs: CLAUDE.md §0.0, §12.1; HM-DEC-122; HM-DEC-091; HM-DEC-095; HM-DEC-120; HM-OPEN-030
---

**HM-DEC-122 cannot be implemented as written without breaking either §0.0 or the
only real recording this project can measure against, and it is not live.**

Built as ruled: two coarse surveys during acquisition, one over twenty
milliseconds and one over fifty, fed from the same ring buffer on the same survey
grid, each asked the question the ruling names, the shorter preferred where both
answer. The diagnosis behind the ruling is sound and the remedy does not follow
from it.

Three measurements, all in HM-OPEN-030 with their tables:

- **Taking the first answer the candidates give meets the acceptance and invents
  characters.** The fast end tuned onto mid-transmission goes from about two
  thirds of the message to about nine tenths, and 2.8% of what comes back below
  the refusal floor was never sent, where HM-DEC-120 measured zero at every level.
  The candidates answer 325, 550 and 725 hertz for a signal at 640 across their
  first several reads; the tracker's own rule against exactly that is two agreeing
  surveys, and the candidates were not subject to it.
- **Gating the settle on the tracker having confirmed where the keying is removes
  both regressions and every gain with them.** Every cell of the matrix is then
  identical to the unmodified decoder, at nine speeds and five ratios, with and
  without a run-up.
- **And where it does fire in time it costs the bulletin seven characters**, 36 of
  47 down to 29, because the analysis window settles at twenty milliseconds.
  **Only the short candidate yields a clock there**, so the tie-break never runs:
  fifty milliseconds smears a 57 ms dit badly enough to fail the cluster test
  while being the better window to read through, and forty, which the ruling
  removes from consideration, is better than either. That is HM-DEC-095's own
  table restated: twenty milliseconds loses half a callsign the same recording
  gives up whole at forty.

So the window that yields the cleanest clock is not the window that reads the
signal best, and on real audio those are different windows.

Three directions, and the choice is yours:

- **Change what the candidates are judged on.** Score each candidate by its own
  speed estimator at the tracked pitch rather than by the survey's per-bin scan.
  That is a measurement of reading rather than of clustering, and a fluke at 725
  hertz cannot fool it.
- **Put forty milliseconds in as a third candidate**, so the choice includes the
  window the evidence prefers.
- **Take the figures as the target and leave the mechanism alone.** Phase 4 moved
  the bare fast end from 0.63 to 0.70 up to 0.88 to 0.89 without touching
  acquisition at all, which is most of what HM-DEC-122 was ruled to buy.

Rejected: shipping it as ruled. HM-DEC-121 is three entries old and says a ruling
that breaks a real decode is marked blocked rather than left live, and HM-DEC-113
says you run `main` against your radio. Rejected: shipping the ungated form, which
§0.0 forbids without a second opinion being needed.

---
date: 2026-08-18
refs: CLAUDE.md §12.5; HM-OPEN-026; HM-DEC-091
---

**`cw-2026-08-18-003758` is closed as unobtainable, or it is supplied.**

Asked across four sessions and the file has not appeared. Re-checked this session
against the concern the work order raised: **nothing in the fixture set names it.**
No test, no sidecar, no catalogue entry, no assertion. It appears in its own open
issue, in the previous report, and in the work order. So the fixture records are
not naming evidence that does not exist, and what is left is a question with no
work behind it.

The cost of closing is real and is why this is not a session's to decide: it is
the recording on which Hamlet read `DE AA4MP/4 QNIK` correctly and somebody
confirmed it independently, and **this suite has no regression test for a success
at all.** Every ratchet it holds is a ratchet on a failure getting less bad. If
the file turns up it is committed and the entry reopens.

Rejected: leaving it open a fifth time, which is what the last three sessions did
and which produces one more paragraph a session and no decision.
