**PROJECT: Hamlet**

# Work order: two windows, one caret, and the bulletin re-measured

Five phases. Reported per §12.2: four sections, **written to `OUTPUT.md` at the
repository root, overwriting it**, and printed to the session as well. **Name
the branch in section 1** (§9.5.1 — `main`, and nowhere else).

**Read first:** `CLAUDE.md` (§0.0, §12, §12.5), `SESSION_PROTOCOL.md`, the
previous `OUTPUT.md`, `OPEN_ISSUES.md`, `DECISIONS.md`.

**New rulings: HM-DEC-122, 123, 124.** All three answer questions the last
session raised with measurements underneath them. **HM-DEC-123 is a separate
work order and is not in this one** — do not begin it.

## Standing instruction

A phase needing a ruling records the question in `OUTPUT.md` section 4 and
continues. §12.1 unchanged. **No transmit work of any kind.**

The suite stands at 1817 tests, five failing:
`ASignalAtTheWrongPitchIsStillFound(400)`,
`ClearingTheTranscriptLeavesTheDecoderAlone`,
`TheBulletinDecodesToItsAnswerKey`, `TheEasyTierIsReadWhole(exchange-easy)`,
`TheEasyTierIsReadWhole(prosigns-easy)`. **Three of the five should fall in this
order.** The 400 Hz one belongs to HM-DEC-123 and stays red.

---

## Phase 1 — two analysis windows during acquisition (HM-DEC-122)

**The edges are not the problem and no edge correction is applied.** Both edges
are late by 30–36 ms, which is group delay: identical at both ends, so it
cancels for a length. What survives is 3–7 ms, under one hop, and it does not
grow as marks shorten.

**The window is the fault.** The tracker picks it from the speed it currently
believes:

| Told | Window | At 30 wpm |
|---|---|---|
| 10 wpm | 50 ms | longer than the dit — runs merge, start error 30 ms → 170 ms |
| 25 wpm or more | 20 ms | reads better than the 40 ms it acquires with |

At 25 the dah reads 144.4 against a true 144; at 30 the dit reads 38.5 against a
true 40. **A decoder that has not yet found the speed is running the window
least able to find it.**

Build it:

- During acquisition, run **20 ms and 50 ms in parallel**.
- **Keep whichever yields a valid clock** — a clean dit-or-dah cluster inside
  the 2.5–3.8 ratio band, which is already the test and needs no new judgement.
- If both yield one, prefer the shorter: the failure is asymmetric — too long
  merges runs and destroys the signal, too short only costs sensitivity.
- If neither does, emit nothing. §0.0 already prefers silence.
- Once locked, the window follows the proved speed as it does now.

Acquiring short alone was **rejected**: it trades weak-and-slow reach, this
project's best-proven capability, for speeds it cannot yet read. Iterating from
the locked speed was **rejected** because at 30 wpm there is no lock to iterate
from.

Acceptance: 30 wpm decodes rather than collapsing, and **nothing at 10–12 wpm
gets worse.** Measure the slow end explicitly and report it — that is the thing
this ruling risks and the thing it was chosen to protect.

## Phase 2 — the fixture generator's caret (HM-DEC-124)

**Hamlet reads `IR` where `AR` was sent because the fixture sends `IR`.** §12.5's
own pattern, with the decoder blamed for months.

`KeyEdges` opens with a single unpaired edge at the message start. The caret's
join branch begins by adding a gap edge, which assumes a mark is in progress to
separate from; at the head of a word there is not, so that edge **closes a mark
that never opened**. A phantom 100 ms dit, and every edge after it on the
opposite parity — the dah that should open `BT` becomes a 300 ms gap and the
element gaps become marks.

`^SK` survives because six elements restore the parity that five break, so **an
even-length prosign renders correctly and an odd-length one does not**, which is
why this has looked intermittent.

The model predicts all nine edges of `^BT` exactly. The reference implementation
reads `EV` and `IR` too: two independent decoders agree and both are right.

- Fix the join branch so a caret at the head of a word does not emit an opening
  gap edge.
- Regenerate the affected fixtures.
- **Re-run HM-DEC-101's gate after every regeneration.** A fixture the reference
  cannot read is a bad fixture.
- **Adjudicate every hold-out individually with its reason recorded** (§12.5).
  No wholesale retirement. This is the discipline phase 3 of the last session
  was held to and it held.
- **Re-check `exchange-easy` after the fix rather than investigating it
  separately** — it is very likely the same defect, and if it is, two of the
  five failures clear in one move.

## Phase 3 — re-measure the bulletin against a known-good fixture set

`cw-2026-08-18-004507` stands at **36 characters against 45**. Every remaining
error is character-level: `JJ` extra and `TARRLD` lost to acquisition, `BT`
unresolved, **`T` read as `A` twice** in `STATION` and `THIS`, and letters
dropped from `EACH`, `MESSAGE` and `HANDLING`.

Re-measure after phases 1 and 2 and **report the number before touching
anything.** The spaces have been right since the Farnsworth fix; what is left
belongs to the clock, and phase 1 is the first thing to touch the clock since.

`T` read as `A` is a dah read as a dit followed by a dah — a spurious leading
dit, which is a mark boundary in the wrong place or an edge caught early. If
phase 1 moves it, say by how much. If it does not, that is the finding.

**Do not tune anything to this recording.** A decoder fitted to one capture has
learned one station.

## Phase 4 — `ClearingTheTranscriptLeavesTheDecoderAlone`

The app failure, unchanged across three sessions and untouched by any ruling.
Diagnose it and say whether the fault is the app's, the decoder's, or the
fixture's, in the manner of the phase 5 adjudications.

Fix it only if the cause is unambiguous. If it is not, report the path.

## Phase 5 — DROP THIS ONE IF SHORT OF ROOM

Housekeeping the record has accumulated:

- **HM-OPEN-026**: `cw-2026-08-18-003758` is named in the fixture records and the
  file is not on the machine, so anything asserted about it is unverifiable.
  **Either it is supplied or the reference is removed** — the fixture set must
  not name evidence that does not exist. Tim has not supplied it across three
  sessions; recommend removal and say so, but do not decide it.
- **HM-OPEN-025**: the `"save"` commits. Recorded, not chased. Confirm it is
  still only cosmetic.
- Confirm HM-OPEN-027 and HM-OPEN-028 are both recorded as belonging to
  HM-DEC-123's separate work order, so the next session does not re-derive them.

If dropped, say so.

---

**If every phase completes, stop and report. Do not start HM-DEC-123's work
order, and build nothing toward auto-CQ.**

## Definition of done

30 wpm decodes and the slow end is measured and unharmed. The caret is fixed,
the fixtures regenerated, gated and adjudicated, and `prosigns-easy` and
probably `exchange-easy` are green. The bulletin's distance from its answer key
is a reported number taken against a fixture set that is finally known good.

**Everything here is provable on the development computer against fixtures, and
none of it is evidence about the radio** (HM-DEC-093).
