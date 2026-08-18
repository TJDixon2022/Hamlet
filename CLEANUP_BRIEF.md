**PROJECT: Hamlet**

# Work order: the survey stops abandoning stations, then the operator gets his hands on it

Six phases. Reported per §12.2: four sections, **written to `OUTPUT.md` at the
repository root, overwriting it**, and printed to the session as well. **Name
the branch in section 1** (§9.5.1 — `main`, and nowhere else).

**Read first:** `CLAUDE.md` (§0.0, §0.2.1, §0.5, §12, §12.5),
`SESSION_PROTOCOL.md`, the previous `OUTPUT.md`, `OPEN_ISSUES.md`,
`DECISIONS.md`.

**New rulings: HM-DEC-127 and HM-DEC-128.** Phases 1 and 2 are the first; phase
3 records the second.

**Phases 4 and 5 are new feature work**, the first in several orders. They come
from the operator using the app rather than from the record.

## Standing instruction

A phase needing a ruling records the question in `OUTPUT.md` section 4 and
continues. §12.1 unchanged. **No transmit work of any kind** — auto-CQ is
HM-DEC-098 and is the next work order, not this one.

The suite stands at 1832 tests, five failing:
`ASignalAtTheWrongPitchIsStillFound(400)`,
`ClearingTheTranscriptLeavesTheDecoderAlone`,
`TheBulletinDecodesToItsAnswerKey`,
`TheEasyTierIsReadWhole(prosigns-easy)`,
`TheEasyTierIsReadWhole(tightfist-easy)`.

**Three of the five belong to phase 1.**

---

## Phase 1 — a confirmed station is not abandoned for a candidate far below it (HM-DEC-127)

The survey's bin choice is behind three failures:

| Where | Signal | Survey chose | Result |
|---|---|---|---|
| `ASignalAtTheWrongPitchIsStillFound(400)` | 400 Hz | 575 Hz, **35 dB down** | the `CQ` is lost |
| `prosigns-easy` with the ruled run-up | 615 Hz | 675 Hz | nothing decoded at all |
| `two-station` from cold | 615 Hz | 625, 600, 625 | three moves before settling |

In the first, the chosen bin carries **the same dit and the same dah as the
station being read, thirty-five decibels quieter, and clusters three times more
cleanly** — separation 30.8 against 8.7. It is the station's own image. The
survey ranks candidates by how far they stand over the band beside them, so 400
wins easily and the move only happens on reads where the 400 bin fails to score
at all and the image is the only candidate left. It then satisfies the
two-agreeing-surveys rule by itself.

**HM-DEC-095 is not in tension with this and is not amended.** It settled that a
note is chosen by how it is keyed and never by how loud it is, on an
**empty-handed** survey, where loudness picked a carrier over a station. **It
never settled whether a candidate may take the tracker away from a station it has
already confirmed.** The answer to that is not "prefer the louder" but "do not
abandon what you have for something far below it."

Build it as a floor on the relative level of a displacing candidate, not as a
preference for loudness. **Measure the separation on every recording in the
corpus and state the number you chose and why** — a threshold pulled from the air
is the thing this project keeps catching.

**Every recording will be re-measured against this.** Report the corpus before
and after, character for character, including the ones that do not move.

## Phase 2 — the fixture gets a band (HM-DEC-127, second half)

`ASignalAtTheWrongPitchIsStillFound` generates its audio with **no noise at
all**, so between the elements there is digital silence and the image is a
hard-limited replica with nothing to bury it. Every fixture under
`tests/fixtures/cw/receiver` was rebuilt with a shaped band for exactly this
reason (HM-OPEN-018); **this one was missed.**

Measured last session:

| noise | moves | decode |
|---|---|---|
| 0 | 3 | `■■ ■■■ ■ K DE W1AW K` |
| 0.01 | 1 | `T■ ■■■ ■Q DE W1AW K` |
| 0.03 | **1** | **`V VVV VVV CQ DE W1AW K`** |
| 0.06 | **1** | **`V VVV VVV CQ DE W1AW K`** |

**Giving it a band changes what it asserts** — from *found at 400 Hz in silence*
to *found at 400 Hz in a band* — and the second is the better test. Say so in the
test's own name or summary, so nobody reads the change as a bar being moved.

**Do this after phase 1, not before**, so the two effects are separable: phase 1
should reduce the moves on its own, and the band should reduce them further. If
phase 1 alone turns this green, say so.

## Phase 3 — record HM-DEC-128 and remove what it closes

**HM-DEC-116 is superseded, not blocked.** Its premise dissolved when the
streaming estimator began reading `CwGapFit`: the choice it was making — fitted
classes against dit multiples — no longer exists, and the real question is the
settled pass's global fit against the streaming pass's local one, which today the
streaming pass wins.

Confirm nothing of the adoption mechanism remains in the tree, as was done for
HM-DEC-122. Mark HM-OPEN-027 and HM-OPEN-032 closed against it, with their
reasons.

## Phase 4 — scan results are click-and-go

**A list of stations is a report; the operator wants a destination.**

Tapping a scan result tunes there. The plumbing largely exists: the waterfall's
click-to-tune already works and **§0.2.1 already governs Hamlet moving the dial**
— never while transmitting, the starting frequency restored on every exit route,
inside the operator's own configured segment, aborting on a touched dial or an
unanswered read.

- A result row carries enough to make the tune unambiguous — the frequency it was
  heard on, not the bin it was ranked in.
- **Tapping a result tunes and stops the scan.** A hunting operator wants to
  listen to what he just found, and a scan that carries on moving the dial out
  from under him while he is reading is §0.2.1's own practical test failing.
- Returning from a result restores the dial as any other exit does.
- **The verdict and its confidence travel to the row** — a stop on a
  0.3-confidence maybe-CQ must look different from a clean one, and the
  confidence already travels into the sentence.
- **A callsign-shaped token is never printed as a callsign** (HM-DEC-073). The
  row says what was heard, not who.

## Phase 5 — a favourite carries a note

A favourite says where. It should say **why**.

- A free-text note, captured **at save time and editable afterwards**. Captured
  only at save time and a blank box gets left blank; editable only afterwards
  and it never gets written.
- Short — a line, not a paragraph. It belongs in the strip beside the name
  (HM-DEC-070) and must not push the dropdown out of shape; §0.7 governs it as
  prose and phase 4 of the UI order already forbids clipping a sentence.
- An empty note is a real state and renders as nothing at all, not as a
  placeholder pretending to be one.

## Phase 6 — DROP THIS ONE IF SHORT OF ROOM

`tightfist-easy`, which is one gap and one number. The character is the first `S`
of the first `TEST`; its pattern is `...`, which is correct, and the same pattern
reads as `S` four seconds later.

```
'■' Unreadable score 0.11 snr 28.6 dB pat '...' at 5.31s
clarities [0.97, 0.11, 0.92, 0.49, 0.89, 0.97]
dit 93 ms, mark boundary 137 ms, element gap boundary 96 ms, 20 marks
```

Twenty-eight decibels over the noise, **so the signal is not the question**: one
timing measurement inside the character scores 0.11 against an element-gap
boundary of 96 ms, on a fist whose element gaps are 80. Every other measurement
in the same character is between 0.49 and 0.97.

**A placeholder is the honest output for a measurement that marginal**, so this is
a question about where the boundary sits and not about the veto. If the boundary
is wrong, say why with the corpus behind it. **If it is right, this test is
asserting something the decoder should not be asked to do** — say that instead
and leave it red.

If dropped, say so.

---

**If every phase completes, stop and report. Do not start the auto-CQ work order,
and build nothing toward it.**

## Definition of done

The survey no longer abandons a confirmed station, with the threshold stated as a
measurement and the whole corpus re-measured around it. The 400 Hz fixture has a
band and says so. Scan results tune. A favourite carries a note.

**Everything here is provable on the development computer against fixtures and the
training radio, and none of it is evidence about the radio** (HM-DEC-093). Tim
verifies phases 4 and 5 at the screen on COM3.
