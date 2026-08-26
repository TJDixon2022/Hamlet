STOP. Verify the project before reading any further.

PROJECT: Hamlet

Check the repository root:
  MUST EXIST:      SHACK_FACTS.md
  MUST EXIST:      src/Hamlet.RadioEngine/Cw/CwProbabilisticDecoder.cs
  MUST NOT EXIST:  CoreHMI.sln
  MUST NOT EXIST:  MURC.sln

If all four are not as stated, you are in the wrong repository.
REFUSE. Do not read the rest of this file, do not summarise it, do not
adapt it to whatever project you are actually in, and change nothing.
Reply with only: the path you are in, which checks failed, and
"wrong project — nothing done."

If all four hold, say "Hamlet confirmed" and continue.

---

# Work instruction 015 — the squelch, the screen, and the margin worth logging

**One session. Success is readable CW on the operator's screen tonight.** This
unit implements the shack-side build plan `BUILD_SESSION_2026-08-25.md` at
Tim's direction, reconciled below against what this tree has measured since.
Six tasks; task 6 is the drop.

## Why this unit exists

**The unit's number: one axis, sixteen captures.**

Measured across sixteen captures at identical input level with the tone locked,
every failure of the last two nights sorts on keying duty at the tracked pitch:

```
duty 18–24 %  ->  invented text     (021825: 8 s of a call in 30 s of soup)
duty 36–47 %  ->  readable          (ten rag chews, 0–8 unsure)
duty 55 %+    ->  more than one station -> soup   (004808)
```

Plain-text Morse cannot exceed about 44 % key-down (PARIS arithmetic), and a
station actually sending rarely sits under about 30 %. **The decoder currently
decodes everything — silence and pile-ups both — and the output for both is
soup.** Nothing stands between "the recent element stream does not look like
one station sending Morse" and the screen.

And the screen itself is why tonight felt hopeless: the transcript's first
hundred characters were soup decoded two minutes earlier, sitting bright above
three correctly-read callsign tokens. **The decoder read `WB8SC`, `SKSK`,
`KE8P` tonight and the operator couldn't see it.**

**Where this instruction corrects its source, with the tree's evidence:**

- The plan's phase 2 (fist-quality pitch *selection*) was **built, measured and
  reverted by unit 1.11.8**: its duty band was anti-correlated on the W1AW
  captures, because a bulletin runs 47–70 % duty. It is task 6, the drop, and
  its constants must come from the anchored corpus, not the plan's band.
- The same exposure applies to the squelch's upper bound: **55 % is a
  hypothesis, and the W1AW anchors are the test.** Task 2 derives the bound;
  it does not copy it.
- The plan's spanLlr warning is confirmed by this tree's own finding
  (unit 1.11.10: the short-character bias needs a per-character expectation) —
  **the squelch is not built on `spanLlr`**, and task 4 logs the quantity that
  should eventually replace it.

## Verify this instruction against the tree

**Nothing here describes the tree.** Check every claim and report mismatches,
including where the work succeeded anyway.

**Known state after unit 1.11.11: 28 failing of 1831 in the engine, 487 of 487
in the app** (three accepted-cost silence fixtures; four known intermittents).
**Twelve success tests; anchors govern where they cover (Tim's ruling,
2026-08-25); count floors stand elsewhere; element floors stand everywhere.
`ARecordingWithNoStationInItSaysNothing(014854)` green and staying green.
Chunk-size invariance across five sizes and both entry points is asserted
corpus-wide and survives every task here.**

**Three captures this unit wants may not be in the tree**:
`cw-2026-08-26-004808`, `-004900`, `-004952`. Tim was asked to copy them to
`tests/fixtures/cw/captured/unadjudicated/`. Task 1 checks; every dependent
step names its fallback.

**`DECISIONS.md` has no record for HM-DEC-096–133, 136, 141, 150, nor Tim's
five rulings of 2026-08-25.** Read HM-DEC-095/127's index-row transcriptions in
unit 1.11.6's report.

**`CLAUDE_CODE.md` says four report sections; its version line reads 1.3.**
Read the file's own section count.

## Rulings in force

**Tim's direction, tonight, quoted:** *"Use it to develop a single work unit
that has a shot at giving the significant improvement in CW."* The shack plan's
phases 1, 3, 4 and 5 are tasks 2–5 under it; its phase 2 is task 6.

**Scope supersession, named so it is not silently contradicted:** earlier units
recorded *"rejected: treating duty as a station test"* from 1.11.8's revert.
That rejection was about **pitch selection** and its constants. Tonight's
direction adopts duty for **emission holding**, with the constants derived
against the anchors — the W1AW bulletins at 47–70 % duty must keep reading, or
the bound is wrong and widens rather than ships.

**Tim's display ruling, via the adopted plan (§0.0 makes the screen his; the
plan is his side's and he directed its use):** the three terminal changes of
task 3 and the debug-flag hiding of task 5, **narrowest reading, nothing else
on the panel**. The "Hold this pitch" button stays exactly as it is.

**HM-DEC-120.** Nothing emitted on audio holding no signal; no Keying window
claimed on it — absolute, all four empty captures, live path. The squelch may
only *strengthen* this: below-band duty holds emission entirely.

**Floors and anchors are the judge of every task**: `013520`, `013303` and (if
present) `004900`'s three tokens byte-identical; all twelve success tests
green; `004952`'s honest 58-of-106 unsure floored as honest, not "improved" by
squelching it silent — a capture the gate holds closed emits *less*, and its
floor is re-expressed as the gate's documented effect with before/after
recorded, per the anchors-outrank-counts ruling where an anchor covers and
with the loss on the record where none does.

**Rejected already, do not revisit:** gating on silence-nulled `spanLlr`
(measured inverted 100:1 on 004808); the joint character cutter (settled at a
safe weight of nought); gap-cluster retuning (clusters merge at 30 WPM); the
four re-read trigger variants; lowering the meter's swing bar; locking to
`CwPitch`.

## Status cadence

Per §4.5: after each task, before the next, update `PROJECT_STATUS.md` —
`STATE`, `TASK: n of m`, `BALL`, `UPDATED` from the clock, `NOTE` saying what
is moving. Same every ten minutes while a task runs.

## The tasks

### Task 1 — bank tonight's three, and their roles

If present, commit `004808`, `004900`, `004952` with sidecars and floor them:
**004808** is the overlap fixture and the spanLlr-inversion proof; **004900**
is the control — `WB8SC`, `SKSK`, `KE8P` must survive everything below;
**004952** is honest behaviour (58 of 106 unsure at S2, 40 WPM at the top of
the search) that must stay honest. **If absent, say so first in section 4 and
fall back**: overlap side from `004808`-absent means the upper bound is derived
from the W1AW anchors alone and marked provisional-on-weaker-evidence; the
control role falls to `013520`/`013303` and the twelve anchors.

Build and run; record the green baseline.

### Task 2 — the emission squelch

A rolling test over the last ~3 s at the tracked pitch, from quantities the
decoder already computes. **Emission holds when the recent stream does not look
like one station sending Morse; it resumes when it does.** Forward-acting only:
characters resolved before the gate closed stay.

1. **Local duty in a band.** Lower bound near 25 % (below: silence — hold, emit
   nothing). **Upper bound derived, not copied**: it must sit above every W1AW
   anchor capture's measured duty and below `004808`'s (or, absent that file,
   above the anchors with the margin reported). Above it: overlap — hold, and
   mark the stretch with a single `■`.
2. **Fist sanity** on the last ~12 resolved marks: dah/dit ratio in [2.2, 4.0],
   dit in [25, 160] ms. Real fists measured 2.6–3.4 all week; the mush measured
   4.6–5.8 and 12.9.
3. **~2 s of passing before resuming** — a word gap at 20 WPM is 420 ms and
   cannot false-trigger it. Trigger arithmetic in hops, so chunk invariance
   holds.

**Proof before merging, from the plan verbatim**: `021825` shrinks from 63
characters to roughly its 8-second call window; `021629`'s `559 559 IN MI MI`
survives; the control tokens survive; `013520`/`013303` byte-identical — those
files never leave the pass band, so **if any floor case changes, the window is
wrong: widen it, don't ship.** Empty captures: still nothing, now doubly held.

### Task 3 — the screen stops burying good copy *(display, under the ruling)*

1. When the squelch has held ~10 s, insert a **timestamped separator rule**
   into the transcript instead of nothing.
2. Everything before the most recent separator renders **dimmed** — selectable,
   nothing deleted; the eye lands on current copy.
3. **The `no keying here` advice block retires whenever the tone panel is
   showing** — two panels currently assert a clear tone and nothing-there
   simultaneously, 50 Hz apart, and send the operator to the radio for a
   decoder condition.
4. **The band row, per Tim's ruling of 2026-08-26, in his words:** the order
   from the top is the Hamlet title, the dad-humor-of-the-day block, **then the
   band row** — and today the row sits far below that. Restore the ruled order.
   Two clipping faults ride with it, observed on the real window tonight:
   **`10 m` is cut on its right** — the operator sees `10 n` and not the whole
   `m` — and **the `best bet now` badge shows only its bottom five percent**,
   which is the shape of an overlay extending above its host's top edge inside
   a parent that clips. All seven cards fully visible including `10 m`'s whole
   label, and the badge fully visible, at the application's default width and
   the operator's working width. HM-DEC-141's wavelength-proportioned card
   widths are meaning and are not shrunk to make room — make room around them,
   and if some width genuinely cannot fit all seven, report what width and
   stop rather than inventing policy.

   **Unit 1.11.9 verified this same area green by headless hit-testing while
   logging an unexplained headless-versus-real geometry offset — and the real
   window shows the faults anyway. Do not verify by headless hit-tests alone
   this time**: assert the visual-tree order (title, humor, band row), assert
   `10 m`'s right render-edge inside the window bounds and the badge's top
   inside its clipping ancestor's bounds, at both widths, and say which
   ancestor was clipping the badge.

Nothing else on the panel changes.

### Task 4 — log the margin that will replace the silence null; change nothing

For every emitted character, compute and log
`marginLlr = LLR(best) − LLR(second-best)` over the same span, beside
`spanLlr`, sidecar and jsonl only. **Clamp both to sane bounds** — the
`6:27306879.3` family is the overflow again. The inversion this exists to fix,
for the record: on `004808` the E-soup scored 8003–29261 against silence while
the plausible tail scored 41–437; against a second-best null, an E carved from
continuous tone scores near nought. **No behaviour change; tomorrow sets
thresholds from real distributions.**

### Task 5 — the keying sweep goes behind a debug flag *(display, under the ruling)*

Wrong on 14 of 20 against independent measurement, and this tree has since
measured its calibration inside an overlap (unit 1.11.10). **Hide the panel
behind a debug flag; keep computing to the sidecar.** Removing a lying
instrument is one line; rebuilding it is not tonight's work.

### Task 6 — fist-quality pitch selection *(the drop candidate)*

The plan's phase 2, previously built and reverted by 1.11.8 — **the revert's
measurement governs**: constants from the anchored corpus, never the plan's
band. A candidate bin passing task 2's derived window beats any bin failing it
regardless of energy (HM-DEC-095's own principle — keying, not loudness);
among passers, the strongest; ±1-bin hysteresis so a steady station does not
hop (013402→013637 drifted 525↔540); the chosen bin's (ratio, dit, duty)
logged beside `toneHz`. **Acceptance:** `004952` chooses 510 over the 6 WPM
noise at 400 (if present); no anchored capture's chosen pitch regresses; the
1.11.11 lead is measured — `031905` toward 499.8 instead of 300, `032113`
toward 499.8 instead of 650. **Any anchor red: revert whole, keep the table.**
Dropped whole if time runs out, and the report says so.

## Parked — do not touch, do not raise

- **The confirmation rule** (consecutive-surveys; the intermittent-station
  finding) — its ruling ask stands from 1.11.11; task 6 may relieve it and must
  not modify it.
- **The meter's rebuild**, the joint cutter, the whole-file second pass for
  late-pitch captures, the integrator width, `014113`/`014308`'s smear,
  `001520`'s quadrillions, the reference/port integrator difference, the
  six-hertz window disagreement, the unmeasured-pitch-costs-`N4L` ruling, the
  Avalonia geometry offset, `CHANGELOG.md`, HM-OPEN-057, HM-OPEN-059, the four
  intermittents.

A parked item that blocks a task is raised once, and says it was parked.

## What not to do

Standing prohibitions are `CLAUDE.md`'s and are not retyped. Unit-specific:

- **Do not gate on `spanLlr`.** Measured inverted 100:1.
- **Do not copy the 55 % bound.** Derive it against the W1AW anchors.
- **Do not let the squelch touch anything already resolved.** Forward only.
- **Do not change the panel beyond tasks 3 and 5's narrowest reading.**
- **Do not redesign the cutter, retune clusters, or touch the tone
  interpolation** beyond task 6's selection logic.
- **Floors only rise; anchors stay green; silence is absolute; chunk
  invariance holds.**

## Committing, pushing, reporting

Commit and push each task before starting the next; name the branch; a refused
push is reported as refused, with the reason.

Report per `CLAUDE_CODE.md` §8 — read the file's own section count — to
`output.md` at the repository root, overwritten and printed.

**Section 3 leads with the squelch's before/after on `021825` and the survival
list — the control tokens, `021629`'s exchange, the byte-identical floors.**
Section 2 says plainly what Tim sees at the radio tonight: a quiet frequency
that stays quiet on screen, current copy bright over dimmed history, one
instrument that no longer argues with another, and the band row back in its
ruled place — title, humor, bands — with `10 m` whole and the badge whole.

### Asks still outstanding

Carried forward verbatim per HM-DEC-139 and HM-DEC-140. **Thirteen inbound.
The oldest is open since 2026-08-14.**

1. **The sweep's `invented` column counts substitutions, not invented
   characters.**
2. **Whether the refill guard should apply to the first fill at all.**
3. **`ANNUNCIATOR.md` renamed `PHASE` to `TASK` while HM-DEC-150 makes
   `PHASE` match the version's minor.**
4. **`DECISIONS.md` has no record for HM-DEC-096–133, 136, 141 or 150 — nor
   for Tim's five rulings of 2026-08-25, plus tonight's adoption of the build
   plan.**
5. **The tone tracker** — the confirmation rule's ruling ask stands; task 6
   measures the selection half.
6. **The integrator width** — bears on `014113`/`014308`.
7. **The guard's gap is two to one**, calibrated on two empty captures.
8. **A boxcar's nulls made two of five swept offsets pathological best
   cases.**
9. **Two stations closer than 125 Hz are not named.**
10. **The keying meter** — task 5 hides it; the rebuild is its own unit.
11. **HM-OPEN-057** (2026-08-22) and **HM-OPEN-007** (2026-08-14).

Still open: **the lock's mixed help**; **the "Hold this pitch" button**;
**three fixtures at accepted cost**; **`001520`'s quadrillions**; **the
reference/port integrator difference**; **`CLAUDE_CODE.md`'s version line**;
**an unmeasured pitch costs `N4L`**; **`014113`/`014308`'s second mechanism**;
**the six-hertz window disagreement**; **the short-character bias** (task 4
logs its replacement quantity); **the Avalonia geometry offset** — task 3 works around it and its cause is still unfound;
**`CHANGELOG.md` at 1.9.0**; **four intermittents**; **the whole-file second
pass**; **the confirmation rule cannot admit an intermittent station**.

**If you finish every task, stop and report. Do not start the next unit.**
