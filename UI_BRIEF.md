**PROJECT: Hamlet**

# Work order: give the scanner a face, put the decoder on screen, and clear the visible faults

Seven phases. **This exceeds §12.3's five or six deliberately**, as the last one
did: Tim would rather run long than shepherd short. Phase 7 is the drop
candidate and is written so that dropping it costs nothing.

Reported per §12.2: four sections, **written to `OUTPUT.md` at the repository
root, overwriting it**, and printed to the session as well.

**Read first:** `CLAUDE.md` (§0.0, §0.2.1, §0.5, §0.6, §0.7, §4, §12),
`SESSION_PROTOCOL.md`, `DECODER_AND_SCANNER_BRIEF.md`, the previous
`OUTPUT.md`, `OPEN_ISSUES.md`, `DECISIONS.md`.

**This is the UI work order the last session was told not to start.** Almost
everything here is verified by Tim's eyes at the screen rather than by a test,
which is why it was held until he was back. Where a phase can be proved by
test, prove it by test anyway.

## Standing instruction

A phase needing a ruling records the question in `OUTPUT.md` section 4 and
continues to the next phase. §12.1 is unchanged: anything touching §0.0, §0.2
or what the display asserts is Tim's without exception.

**No transmit work of any kind.** Auto-CQ is HM-DEC-098 and is not in this work
order; build nothing toward it.

---

## Phase 1 — the scanner's face, and the stop control §0.2.1 requires

The engine is finished, tested, and **reachable from no view model and no
command**, which was the right safe state: §0.2.1 requires an always-visible
stop and until one exists no scan may start.

Build:

- **An always-visible stop control while a scan runs.** Not a menu item, not a
  window close button — the report already rejected that, and it was right: a
  close button is a way to leave the room while the radio goes on being tuned
  by a process that has just lost its screen. It stays on screen wherever the
  operator is in the app.
- **A line saying plainly that Hamlet is moving the dial**, in §0.7's voice.
  The operator must never wonder why his frequency changed.
- **The ranked candidates with their verdicts and confidence.** Phase 7 of the
  last order made the confidence travel into the sentence — a call assembled
  from dim letters reads "not at all sure" and a solid one reads "sure". Carry
  that through to the screen rather than re-deriving it.
- **A callsign-shaped token stops the scan and is never printed as a callsign**
  (HM-DEC-073). The verdict carries no name.
- **A dwell that found nothing still reports where it was and what it heard.**
  Silence about a bin the scan visited is the collapsed-panel failure in §0.5:
  hiding detail is fine, hiding information is not.

Practical test for this phase: could the operator walk away mid-scan, come
back, and be unable to tell where his radio had been left or why? If yes, the
phase is not done (§0.2.1).

## Phase 2 — the band-plan file the operator is supposed to edit

`ScanSegments.WriteDefaultIfMissing` is called by nothing, so
`%AppData%\Hamlet\scan-segments.json` does not exist and there is nothing to
edit.

- Wire it into first run.
- Put a way to open it in the Settings window.
- A file that cannot be read is **refused loudly and never quietly replaced
  with the default** — the engine already does this; make sure the operator
  sees the refusal rather than a silent fallback.
- Call `BandScanner.RestoreHomeAsync` on connect, so a scan the app died during
  puts the dial back. The note is written before the first tune for exactly
  this case and nothing reads it yet.

## Phase 3 — the waterfall counter row hides when healthy (HM-DEC-093)

Ruled: **option B.** The row currently reads `4147 parts in, 4147 read, 376
sweeps drawn` permanently, and the waterfall works, so it is furniture.

- Hidden while parts arrive, are read, and sweeps draw.
- **Shown when any stage is wrong**: no parts arriving; parts arriving but none
  read; parts read but no sweeps completing; frames stopped for some seconds.
- Numbers remain available on demand.
- HM-DEC-093's property survives intact: **"band is quiet" and "nothing has
  ever arrived" must stay visually distinct.** The row is what distinguishes
  them, so it appears in the second case.

**And verify the header's word `receiving` is driven by frames actually
arriving in the last second or two**, not by the scope source being connected.
If it is driven by connection state it said "receiving" throughout the weeks
when 2,740 parts were being discarded, which is §0.0 broken by a single word.

## Phase 4 — widgets reflow rather than clip

Ruled: **option D.** Content is being cut at the right edge — the level meter's
sentence, the amber tuning hint — and a clipped sentence is §0.0 broken,
because the operator can only half read it and cannot tell there was more.

- **Determine first whether the clipping is widget-level or the canvas clipping
  its children.** The fix lands in a different place and this has not been
  established.
- Text wraps and controls reflow as a widget narrows. Prose is connected speech
  (§0.7) and must never be cut mid-sentence.
- **A minimum width below which a widget refuses to shrink**, so reflow cannot
  collapse a level bar into two pixels.
- A horizontal scrollbar was rejected: a summary scrolled off the edge fails
  §0.5's test.

Check the same pass against the favorites strip, where `EG90GL on 14.028 —
reported by a spot feed, not heard by Hamlet` appears inside what should be the
favorites dropdown while the word "favorites" sits in a separate box to its
right. **Report what you find; do not guess at the intent.** Either the
dropdown is populated from the wrong source, or the label has detached, or
both, and nobody has yet established which.

## Phase 5 — the two-stage decode becomes visible

`CharacterSettled`, `CwReadingStage` and `Revisions` exist and are tested, and
**no UI renders any of it**, so the entire two-stage design is invisible.

- One line of text with a **live provisional tip that firms into settled text
  behind it**, the tip visually distinct.
- **The provisional tip keeps running and is visibly marked unstable** during a
  refusal — clock loss, tracker switch. The moment someone answers is the worst
  moment for the live feed to go dark.
- **The speed field goes blank while the clock re-acquires** and says it is
  re-acquiring (HM-OPEN-022, phase 1 of the last order). `SpeedIsReacquiring`
  exists so a surface can explain the blank rather than merely showing one.
- A **speed change and a tracker switch are annotated** on the settled line.
  Operationally both mean somebody else started transmitting.
- **The ceiling announces itself when it binds** — at slow speeds the settled
  fit is short and the display says so rather than concealing it.
- The revision log is in-memory and exportable; give the export a way in.

Confidence display follows HM-DEC-108 now: three measurements, worst wins.
Nothing on screen may raise a confidence.

## Phase 6 — the stale rig-state block

Capture sidecars show the header at 7.030 MHz / 40 m while the rig block eleven
lines below says 14.055 MHz, and one showed `Frequency` marked stale at sixty
seconds while every neighbouring field was twenty-seven seconds old. **That row
is on a different refresh path from the rest and has now produced four separate
faults.**

Capture headers come from the rig model, never from configuration. One source
of truth.

**Then check what else consumed the wrong value.** RBN is filtered to band
(HM-DEC-024) and the skimmer watch (HM-DEC-075) reports who heard the operator.
If the band was resolved from the stale field, the skimmer watch may have been
filtered to a band he was not transmitting on — which would make an empty
"nobody heard you" panel a defect rather than an answer. Establish whether that
is so; do not assume it either way.

## Phase 7 — DROP THIS ONE IF SHORT OF ROOM. `BandPlan` migration, HM-OPEN-005.

`BandPlan.Bands` carries seven bands of frequency literals marked
`[extrapolated]`, and its own comment says the numbers are carried from general
knowledge and not source marked. §0.2.1 forbids frequencies asserted from a
model's memory, so the scanner was built around it rather than on it: its
defaults come from `data/bands/us-neighborhoods.json`, cited to `cfr-97.305`,
`arrl-conop` and `qrp-arci`.

That leaves **two band plans in the tree, one cited and one not**, which is the
state §0 exists to prevent, and the uncited one has the friendlier name.

- **Verify the cited data column-aware against its sources first** (§4), before
  anything re-points to it.
- **If `us-neighborhoods.json` does not cover what `BandPlan`'s callers need,
  record the gap and stop.** Do not extrapolate to fill it — extrapolation is
  the defect being removed.
- If it does cover them, re-point the callers and delete `BandPlan`.
- Raise HM-OPEN-005's severity either way: it is now load bearing for a feature
  that moves the operator's dial.

**A half-migrated band plan is worse than two whole ones.** If there is not room
to finish, drop it and say so.

---

**If every phase completes, stop and report. Do not start any other work unit,
and build nothing toward auto-CQ.**

## Definition of done

The scanner can be started and stopped from the running app, with its stop
always on screen and the dial restored on every exit route. The band-plan file
exists and can be opened from Settings. The waterfall's counter row is gone
when healthy and present when not. Nothing clips. The provisional tip, the
settled text, the blank speed field and the annotations are all on screen.
Capture sidecars agree with the rig.

**Tim verifies at the screen, on the ham computer, against the real radio.**
Everything in this order is provable on the development computer against the
simulator, and **none of that is evidence about the radio** (HM-DEC-093).
