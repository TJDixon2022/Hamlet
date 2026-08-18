# 1. What Claude did

**STATE.** Development computer, `C:\Source\HamLet`, Claude Code surface (§9.5).
**Branch: `main`, and nowhere else** (§9.5.1). The prompt claimed `PROJECT: Hamlet`
and the tree confirms it: `CLAUDE.md`'s header reads `Project: Hamlet`, the
solution is `Hamlet.sln`, the namespaces are `Hamlet.*`. Gate passed. **Nothing in
this report is evidence about the radio** (HM-DEC-093): every number comes from a
fixture, a generated signal, or one of the two off-air recordings decoded here.

**Nothing was recorded under §12.1.** Two questions came up and both are in
section 4.

**All five phases completed. Nothing was dropped.** No transmit work of any kind.

## Phase 1 — a refining retune keeps the settled window (HM-DEC-123) — **built**

**The criterion is measured and it is the survey's own grid.** Every tracker move
in the corpus was traced with its from-and-to pitch:

| recording | the moves | what they are |
|---|---|---|
| `cw-2026-08-17-013347` | cold to 625, then 625→600, 600→625 | one station at ~615, the survey settling between two bins |
| `cw-2026-08-18-004507` | 500→525, 525→500 | one station at ~505, the same |
| `two-station` | 625→**725**, then 725→700, 700→725 | the caller at 615 **handing over to the answerer at 730**, then settling |

**Every move within one station is exactly one coarse bin, twenty-five hertz, and
the one genuine station change is a hundred. There is nothing in between to choose
from.** `ConfirmWithinHz` already carried that number for the neighbouring
question — whether two consecutive surveys are the same signal — and its own note
already called it "a station drifting or the survey preferring its neighbor,
rather than a different signal". The distinction did not need inventing, only
reading.

`CwToneTracker.Follows` counts moves to a different station; the decoder acts on
those and does nothing at all on a refinement. A tracker that has not yet reported
a pitch has nothing to refine, so its first move is a follow. The measurement is
against the bank the tracker listens through rather than the pitch it last
reported: the fine bank answers a few hertz outside its own centre — 730 through a
bank centred at 725 — and measuring from the report would make one bin read as one
and a bit.

**Acceptance, half met.**
`TheSettledPassNoLongerStopsShortOfTheCallsign` **passes**: the settled pass reads
`■■■ ■■VA3VRR` where it read `■■■ ■`. The bulletin gains `DOT` and the two-station
fixture gains `W1XYZ K`. **Nothing that passed regressed.**

**`ASignalAtTheWrongPitchIsStillFound(400)` does not pass, and the reason is that
this entry's own diagnosis was wrong.** Measured by disabling every reset
outright: the decode is unchanged, still `■■ ■■■ ■ K DE W1AW K` with the `CQ`
missing. **The retunes never cost that case anything through the settled window.**
What they cost is where the tracker went — to 575 hertz, for about half a second,
straight through the `CQ`. And 575 is the station's own image:

```
keyed 400  dit 77  dah 213  sep  8.7  lift 64.0  keyedDb -21.9
keyed 575  dit 83  dah 220  sep 30.8  lift 26.3  keyedDb -56.5
```

Same dit, same dah, **thirty-five decibels quieter, and clustering three times
more cleanly than the station itself.** It is an artifact of a fixture with no band
in it — that test generates its audio with no noise at all, so between the elements
there is digital silence and the sidelobe is a hard-limited replica with nothing to
bury it. With noise added it disappears:

| noise | moves | decode |
|---|---|---|
| 0 | 3 | `■■ ■■■ ■ K DE W1AW K` |
| 0.01 | **1** | `T■ ■■■ ■Q DE W1AW K` |
| 0.03 | **1** | **`V VVV VVV CQ DE W1AW K`** |
| 0.06 | **1** | **`V VVV VVV CQ DE W1AW K`** |

**Not fixed.** Giving the test a band would turn it green, and changing a fixture
to turn a test green is the one move §12.5 exists to stop a session making on its
own authority. Section 4.

**And the change held back was not needed.** HM-DEC-123 held back stopping the
streaming segmentation from gating the tracker, on the ground that it might be a
symptom that dissolves. It dissolved: see phase 2.

## Phase 2 — HM-DEC-116, re-attempted and **not shipped**

**The chain HM-DEC-121 traced is genuinely broken.** With adoption applied,
`cw-2026-08-17-013347` shows **three moves and one follow, identical to adoption
off**, where the whole of that diagnosis was that adoption turned one retune into
three. `MidCharacter` no longer costs anything, because a refinement no longer
resets anything. That is HM-DEC-123's own prediction confirmed, and it is why the
held-back change should stay held back.

**The new path is direct and it is about the classes themselves.** Adoption now
changes only where the streaming pass divides characters, and on the two
recordings where it fires the settled pass's classes are the worse of the two fits:

| | adoption off | adoption on |
|---|---|---|
| `013347` settled | `■■■ ■■VA3VRR` | `■■■ ■■VA3VRR` |
| **`013347` streamed** | **`■    ■VA3VRR`** | `■    ■■■■R` |
| `004507` settled | `NL DOT NET ■I ECH STAAION HAND■ AHIS MESAGE P` | unchanged |
| **`two-station` settled** | **`L DE W1XYZ K`** | `ATD■VTXYZ` |
| `ClearingTheTranscript…` | fails at `■ DE W1AW K` | **passes** |

Everything else in the corpus is unchanged, character for character. So the trade
is one synthetic looping training signal against the streaming pass losing the
callsign on the only real capture that carries one. **A real capture outranks a
synthetic one** (HM-DEC-091) and the work order said not to ship it if it still
costs one. It does. It is not shipped and nothing of it is left in the tree.

**And the ruling's premise has dissolved underneath it.** HM-DEC-116 says the
streaming pass "uses dit multiples only until those classes exist", which was true
when it was ruled and stopped being true when the estimator got the real fitter
last session. Read literally against today's code — adopt only where the estimator
has no fit of its own — it was measured and **it is a no-op on every recording
here**, because wherever the settled pass has classes the streaming pass already
has its own. The full form overrides a working local fit with a worse global one;
the narrow form never fires. Section 4.

## Phase 3 — the bulletin, **28 correct becomes 30**

Measured before anything in phase 4 was touched.

```
got    'NL DOT NET ■I ECH STAAION HAND■ AHIS MESAGE P'
wanted 'AT ARRL DOT NET <BT> EACH STATION HANDLING THIS MESSAGE P'
36 characters against 47
```

Aligned against the key rather than counted, which is the honest measure:

| | before phase 1 | after |
|---|---|---|
| settled | `OT NET ■I ECH STAAION HAND■ AHIS MESAGE P` | `NL DOT NET ■I ECH STAAION HAND■ AHIS MESAGE P` |
| correct | 28 | **30** |
| wrong | 4 | 5 |
| invented | 1 | 1 |

Of 44 characters sent. **The `D` of `DOT` came back**, lost since the recording was
committed, and `NL` arrived at the head where nothing was sent. The cause is phase
1: two of this capture's three moves are the survey settling between 500 and 525
hertz on one station, and the settled window is no longer thrown away for them.

**`T` is still read as `A` in `STATION` and in `THIS`**, unmoved by any of the four
measurements taken of this recording. Nothing was tuned to it.

## Phase 4 — the last two bar failures, **attributed, not fixed**

Checked whether they share a cause before treating them separately, as asked.
**They do not, and one of them shares a cause with the 400 hertz test instead.**

**`tightfist-easy` is a timing veto on one gap.** The character is the first `S` of
the first `TEST`; its pattern is `...`, which is correct, and the same pattern
reads as `S` four seconds later.

```
'■' Unreadable score 0.11 snr 28.6 dB pat '...' at 5.31s
clarities [0.97, 0.11, 0.92, 0.49, 0.89, 0.97]
dit 93 ms, mark boundary 137 ms, element gap boundary 96 ms, 20 marks
```

**Twenty-eight decibels over the noise, so the signal is not the question**: one
timing measurement inside the character scores 0.11 against an element-gap
boundary of 96 milliseconds, on a fist whose element gaps are 80. Every other
measurement in the same character is between 0.49 and 0.97. A placeholder is the
honest output for a measurement that marginal, which makes this a question about
the boundary and not about the veto.

**`prosigns-easy` loses its opening to acquisition, and the ruled remedy makes it
worse.** Its first character arrives at 7.44 seconds on a message running about
four and a half. Re-measured with a run-up after the caret fix and after phase 1
rather than taken from the record: the tracker makes two moves, **settles at 675
hertz on a fixture sitting at 615**, and emits nothing at all.

**Which is the third sighting of one mechanism** — the coarse survey choosing a bin
that holds no station. Section 4.

## Phase 5 — the record housekeeping — done, not dropped

**HM-OPEN-026 closed** (HM-DEC-126), with the gap it leaves recorded beside it:
this suite has **no regression test for a success at all.** Every ratchet in it is
a ratchet on a failure getting less bad, so nothing in it can tell a repair from a
coincidence.

**HM-OPEN-030 closed** (HM-DEC-125). Swept: no candidate survey, no candidate
window constant, no clock-proved flag and no window-change counter remains
anywhere in `src`. `CwAcquisitionWindowTests` survives — it is measurement rather
than mechanism — and still pins all three figures, the bare fast end, the same fist
with a run-up, and the slow end.

**HM-OPEN-024**: `TheStopFrameIsCommand17CarryingFf` did not fail once across six
full runs this session, and a third test flaked once instead. All three are still
intermittent rather than any becoming reliable, and they share nothing but running
under xunit's parallel collections.

# 2. What Tim should expect

- **Build succeeds, no warnings.**
- **1832 tests, 5 failing.** 1412 of 1416 in the engine, 415 of 416 in the app.
  Three tests are new, all of them pinning the retune distinction.
- **`TheSettledPassNoLongerStopsShortOfTheCallsign` is green.** Six failures
  become five and nothing regressed.
- **The failing five, named:**
  - `ASignalAtTheWrongPitchIsStillFound(400)` — **still red, and its cause has
    changed**: it was never about the settled reset. Section 4.
  - `ClearingTheTranscriptLeavesTheDecoderAlone` — reads `■ DE W1AW K` against
    `CQ DE W1AW K`. It would pass with HM-DEC-116 shipped, which is why that is a
    ruling and not a session's call.
  - `TheBulletinDecodesToItsAnswerKey` — the long-standing bar, now 30 correct
    of 44 where it was 28.
  - `TheEasyTierIsReadWhole(prosigns-easy)` and `(tightfist-easy)` — both
    attributed in phase 4.
- **What will look wrong and is not.** The count went six to five, not six to
  three: two of phase 1's three named failures are answered and the third turned
  out not to belong to it. HM-DEC-116 is ratified and not in the tree, deliberately
  and on the work order's own instruction. Nothing of HM-DEC-122 remains.
- **What is different at the radio.** A station whose pitch the survey settles
  between two bins — which is most of them, since nobody sits exactly on a
  twenty-five hertz grid — no longer costs the settled transcript its window twice
  while being found. On the one capture that carries a callsign, that is the
  difference between four placeholders and `VA3VRR`.
- **Nothing is tuned to any recording.** No decoder parameter was moved to suit
  `cw-2026-08-17-013347` or `cw-2026-08-18-004507`.
- **Five commits, pushed to `main`.** Nothing local, no branches. The first carries
  the uncommitted `CLAUDE.md` and `CLEANUP_BRIEF.md` that were in the working tree
  when the session opened.

# 3. What we should do next

- Rule on the survey's bin choice, section 4 item one. It is now behind three
  separate failures and it is the largest single thing left.
- Rule on HM-DEC-116, section 4 item two — supersede or keep blocked. It is
  ratified and not in the tree, which is the state the record likes least.
- Then `tightfist-easy`'s boundary, which is one gap and one number.
- Re-measure the bulletin after either of the above; it has moved on three
  consecutive sessions.

# 4. What's blocking us

---
date: 2026-08-18
refs: CLAUDE.md §0.0, §12.1, §12.5; HM-DEC-095; HM-OPEN-028; HM-OPEN-033
---

**A keying candidate may not displace a station already being read when it is far
weaker, and the fixture that exposed this gets a band in it.**

Two halves of one finding, and the second is the smaller.

**The survey's bin choice is now behind three separate failures:**

| where | signal | survey chose | result |
|---|---|---|---|
| `ASignalAtTheWrongPitchIsStillFound(400)` | 400 Hz | 575 Hz, **35 dB down** | the `CQ` is lost |
| `prosigns-easy` with the ruled run-up | 615 Hz | 675 Hz | nothing decoded at all |
| `two-station`, from cold | 615 Hz | 625, 600, 625 | three moves before it settles |

In the first the chosen bin carries **the same dit and the same dah as the station
being read, thirty-five decibels quieter, and clusters three times more cleanly**
— separation 30.8 against 8.7. It is the station's own image. The survey ranks
candidates by how far they stand over the band beside them and 400 wins that
easily, so the move only happens on the reads where the 400 bin fails to score at
all and the image is the only candidate left. It then satisfies the
two-agreeing-surveys rule by itself.

HM-DEC-095 settled that **a note is chosen by how it is keyed and never by how
loud it is**, and that ruling was about which of several signals to read on an
empty-handed survey — where loudness picked a carrier over a station, which is the
fault it exists to prevent. **What it did not settle is whether a candidate may
take the tracker away from a station it is already reading and confirmed**, and
that is a different question with a different answer available: not "prefer the
louder" but "do not abandon what you have for something thirty-five decibels
below it".

That change decides what the display asserts, so it is yours (§12.1).

**And the second half is the fixture.** `ASignalAtTheWrongPitchIsStillFound`
generates its audio with **no noise at all**, so between the elements there is
digital silence and the image is a hard-limited replica with nothing to bury it.
Every fixture under `tests/fixtures/cw/receiver` was rebuilt with a shaped band in
it for exactly this reason (HM-OPEN-018), and this test never was. With any band
in it the excursion disappears and the message reads whole at 0.03 and 0.06.

Rejected: giving it a band on this session's authority. §12.5's own rule is that
changing a fixture to turn a test green is the move a session may not make alone,
and this one would also quietly change what the test asserts — from "found at 400
hertz in silence" to "found at 400 hertz in a band". The second is the better
test and it is still not a session's to swap in.

Rejected: fixing the survey here. It is one line of judgement about what the
tracker may abandon, and every recording in the repository would be re-measured
against it.

---
date: 2026-08-18
refs: CLAUDE.md §0.0; HM-DEC-116; HM-DEC-121; HM-DEC-091; HM-OPEN-027; HM-OPEN-032
---

**HM-DEC-116 is superseded rather than blocked: its premise dissolved when the
streaming pass got the real gap fitter.**

The ruling says the streaming pass "adopts the settled pass's fitted gap classes
for the current sender, **and uses dit multiples only until those classes exist**".
The second clause was true when it was ruled. It stopped being true last session,
when the streaming estimator stopped carrying its own two-way classifier and
started reading `CwGapFit` — the same fitter the settled pass uses (HM-OPEN-032).
The choice the ruling was making is no longer available: it is not "fitted classes
against dit multiples" but "the settled pass's fit against the streaming pass's
own", which nobody has ruled on.

Measured both ways this session, on top of phase 1:

- **The full form costs a real capture.** The streaming pass on
  `cw-2026-08-17-013347` falls from `■    ■VA3VRR` to `■    ■■■■R`, and the
  two-station settled text falls from `L DE W1XYZ K` to `ATD■VTXYZ`. What it buys
  is one synthetic looping training signal. HM-DEC-091 decides that.
- **The narrow form, read literally, never fires.** Adopting only where the
  estimator has no fit of its own leaves every recording in the corpus unchanged,
  character for character, because wherever the settled pass has classes the
  streaming pass already has its own.

**And the reason HM-DEC-121 blocked it is gone.** That ruling blocked HM-DEC-116
pending a trace of the coupling, and the coupling was adoption moving
`MidCharacter`, turning one retune into three, and resetting the settled window.
With phase 1 in place, adoption produces **three moves and one follow, exactly as
without it**. The chain is broken at the last link, as HM-DEC-123 predicted. What
remains is a straightforward question of which fit is better, and on this evidence
it is the streaming pass's own.

So the honest state is not "blocked pending phase 1" — phase 1 happened — but
"answered, and the answer is no". Whether that is a supersede is yours.

Rejected: shipping it. The work order forbade it by name if a real capture still
paid, and one does. Rejected: leaving it recorded as blocked, which would keep a
ratified ruling waiting on a condition that has already been met and answered.
