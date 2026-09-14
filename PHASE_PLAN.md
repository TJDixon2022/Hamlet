PHASE: The screen, done right
PHASE_SET: 2026-09-12
DESCRIPTION: The main window laid out as the approved mockup - one short top row about where you are, the working panels given the height - and the achievements category pages as trading cards. Screen only. Judged by Tim at his window.
STEP: 0 | The main window is the mockup - the top row 190 px with the neighborhood card carrying the green block and the world clock, the rig display the same height with drive and power under it, the working panels full height below the tabs, at 1920 and at 1400.
STEP: 1 | Every achievements category page is trading cards - the contact that earned each card with its path map, distance, band, mode and date; the next card carrying who is calling now from an unworked place. All eight kinds.
STEP: 2 | What the last phase left - the States wording, the Modes test, the undeletable files listed once, the record's small reds, the points file's documentation checked against the file.
STEP: 3 | Tim looks at every page at his window size and says it passed.

---

# The screen, done right - the reasoning under the step list

**Set 2026-09-12 by the web thread on Tim's instruction**, replacing the maintenance
phase of the same day, which is archived at `docs/phase-maintenance-run/` with its steps
0 and 1 done and step 2 partial. That phase's plan told the arbiter a mechanism for the
layout that arithmetic would not allow, and its step 0 grew the green zone until the
working panels lost the window. **This plan states outcomes and carries a picture.**

`assets/main-screen-mockup.png` is the approved main window. `assets/category-page-countries.png`
is the approved category page. `assets/achievements-opening-mockup.png` is the opening
page, already built by unit 332 and unchanged.

## §1 What this phase is

Screen only. Nothing here touches the radio, a decoder, a parser, the transmit chain or
the log's content. **A step's exit is what is on the screen, asserted by computation and
described in words, then Tim's eyes.** Every appearance claim is computed, not seen, and
says so.

## §2 What stands

Every ruling of the PSK31 phase (R1-R20, `docs/phase-psk31-run/PHASE_PLAN.md`) and of the
maintenance phase (R21-R25, `docs/phase-maintenance-run/PHASE_PLAN.md`) stands. **R12** a
session rewrites its own tests and never asks; **R13** telemetry on every new stage;
**R14** tests prove criteria and nothing beyond; **R16** the two quills; **R19** American;
**R22** the category pages as trading cards; **R23** States from `STATE`, rank names from
the points file. `ACHIEVEMENTS_PHILOSOPHY.md` §2, §3.1 as bent by Ruling C, §3.5, §4.
`CLAUDE.md` §0.0, §0.5, §0.6. **HM-DEC-155**, **HM-DEC-139**, **FACT-004**, **FACT-006**.

**The arbiter stops for three things only** (Tim, 2026-09-12, now in `ARBITER.md`):
keying, transmit or the radio's safety; money past the budget; a decision that changes
what the product promises the operator. On everything else it takes its own
recommendation, marks it author's and overrulable, applies it, and continues. **A later
ruling of Tim's beats an earlier line of this plan; the arbiter proceeds.**

## §R Rulings, Tim, 2026-09-12

**R26 - the main window.** Tim, on unit 334's screen: the green zone had grown to two
thirds of the window and the working panels were squeezed into the bottom third. The
mockup answers it, and its outcomes are the rulings:

- **The top row is one band, about 190 px tall at 1920**, and the working panels below
  the tabs take the rest of the window. At no window size do the working panels get less
  than half the height below the band pills.
- **The neighborhood card carries three things**: the band strip with the legend and the
  *you · mode* marker; **the green block** - the band in the largest text, the frequency,
  mode and *yours to use*, the license line small, the rule-of-thumb line small, and
  heard-just-now with its count and sparkline; and **the world's clock** at its right end,
  about 246 px wide, with the operator's dot only.
- **The rig display is the same height as the neighborhood card**, and carries under the
  frequency and S-meter the transmit drive and the RF power offer, so no empty column
  stands under it.
- **The band pills stay where they are** and are not repeated anywhere.
- **Below the tabs**: waterfall, decoded text, For You, all the same height, full to the
  status bar. The decoded list is as wide as its longest line needs and no wider; For You
  takes the rest, wide enough that the card's facts sit beside its map.
- **At 1400**: the same shape; where the card's facts cannot sit beside the map they go
  under it; no callsign is ever clipped in the decoded list. **No mechanism is prescribed;
  the unit measures and chooses, marks the choice as its own, and reports the numbers at
  both widths.**

**R28 - Stop is always pressable and never grey.** Tim, 2026-09-14, asked which of two
ways Stop should behave and answering *"A, the way it has been"*: the control stays live at
every instant, including when nothing is keyed, rather than being disabled until there is
something to stop. A press with nothing running costs nothing; a grey Stop on the evening it
is needed costs the transmission. Recorded by unit 356 task 0 from his own words, and
building nothing: the behavior is already this.

**R27 - what the last phase left.** Unit 336's open items: the States wording (*worked*,
never *confirmed*, and the count says what it counts); the Modes test; the files sessions
cannot delete, listed once for Tim; the two `Views` reds and any other small red in the
record; the points file's comment block checked key by key against the file. Small, and
one step.

## §4 The steps

Each step verifies its own ground. Exit criteria are tiered. **A step's exit is its own
assertions plus the carry-forward list, run as its own comment says** - not the whole
suite.

## Step 0 - The main window is the mockup

**Delivers:** R26.

**Entry:** the tree is Hamlet's; `PHASE_STATUS.md` names this phase.

**Exit:**
- At 1920, the top row (neighborhood card and rig display) is about 190 px tall and the
  working panels below the tabs take the rest; the numbers reported. *must-pass*
- The neighborhood card carries the band strip, the green block and the world clock as
  R26 says; the green block's band is its largest text; the clock carries one dot.
  *must-pass*
- The rig display is the neighborhood card's height and carries drive and the power
  offer under the S-meter. *must-pass*
- Waterfall, decoded text and For You are equal height, full to the status bar; the
  card's facts sit beside its map at 1920. *must-pass*
- At 1400 the same shape holds, no callsign is clipped, and the facts go beside or under
  by the unit's stated rule; the numbers reported at both widths. *must-pass*
- `BindingHealthTests`, `VoiceTests` and the carry-forward list green. *must-pass*
- The band pills' *best bet now* is joined to the green block by the check as before.
  *nice-to-pass*

**Depends on:** nothing.

## Step 1 - Every category page is trading cards

**Delivers:** R22, as `assets/category-page-countries.png`, on all eight kinds.

**Entry:** step 0 done; `TheAchievementsPageClicksInTests` green, checked first.

**Exit:**
- A category page has the color band with count, score, level and a bar to the next
  level. *must-pass*
- An earned card carries the entity, callsign, grid, a path map cropped to the two
  stations, distance, band, mode, date and points, from the log entry that earned it.
  *must-pass*
- The next card names what it wants and, when anyone on the CQ list would earn it, lists
  them with distance; otherwise says no one is calling from there now. *must-pass*
- All eight kinds render this way with their own content per R22; Continents opens to
  seven badges and each to its countries. *must-pass*
- No string clips or wraps a word, measured at 1400 and 1920. *must-pass*
- No card is a white rectangle. *must-pass*
- `achievement_category_opened` carries the kind and the count of cards. *must-pass*
- A card's map opens in the popup on click. *nice-to-pass*

**Depends on:** step 0.

## Step 2 - What the last phase left

**Delivers:** R27.

**Entry:** step 1 done.

**Exit:**
- The States badge and its cards say *worked*; no *confirmed* anywhere. *must-pass*
- The Modes test unit 336 named is green or named as a known red with why. *must-pass*
- The undeletable files are listed once in the report. *must-pass*
- The two `Views` reds are green or named as known reds with why. *must-pass*
- The points file's comment block names every key in the file and no key it lacks.
  *must-pass*

**Depends on:** step 1.

## Step 3 - Tim looks

**Delivers:** Tim opens every page at his window size and says it passed.

**Entry:** step 2 done.

**Exit:**
- Tim says it passed. *must-pass* No script can evaluate this.

**Depends on:** step 2.

## §5 Dependencies

Step 0 depends on nothing. Steps 1-3 are one pipeline.

## §6 Branching

- **The arbiter stops for three things only**; on everything else it decides, marks,
  applies, continues.
- **A later ruling of Tim's contradicts a line of this plan.** The later ruling wins.
- **A must-pass is missed by a little.** Ship, report, `partial`, move on. Never loosen a
  test.
- **A string will not fit.** Shorten and say which, or widen; never clip.
- **A fact for a card is missing from the log.** Show what is there; no dash; a map with
  no grid is no map.
- **Anything touches the radio, a decoder, a parser or the transmit chain.** `MOVE: stop`.
- **A package is needed.** `MOVE: stop`.
- **A file must be deleted.** Empty it, comment it, list it.
- **Status.** `tools/status.sh` reads the clock; never compose a time. The watchdog now
  polls the process; a session is killed only when its process tree accrues no CPU for
  ten minutes.

## §7 Carried

Every open ask from unit 336's queue, verbatim in every unit. Plus: real flags on
earned country cards - undecided, Tim's; the PSK31 phase's step 6 - Tim's, at the
radio; the recording of real PSK31 audio; the id-scheme split; the map bitmap's license.
