# Work instruction 334 - the green zone finishes, and the leftovers go

**Seed of the maintenance phase, under `--seed`.** Step 0 of `PHASE_PLAN.md`. The
arbiter authors steps 1 and 2 after it; step 3 is Tim's. **Four tasks, each small.**

**Status.** `tools/status.sh` reads the clock; use it for every write. Before every
`dotnet` command, after every commit, after every task. Never compose a time.

---

## 0. The project gate

```
SHACK_FACTS.md                                          must exist
src\Hamlet.RadioEngine\Cw\CwProbabilisticDecoder.cs     must exist
CoreHMI.sln                                             must not exist
MURC.sln                                                must not exist
root                                                    C:\Source\HamLet
```

**If any of the four is wrong, stop and say so in `output.md` section 4. Write nothing
else.** The refusal text: *This is not Hamlet. Nothing was changed.*

---

## 1. The rules that killed sessions

**HM-DEC-155.** No suite; only this unit's names and `docs\carry-forward-tests.txt` as its
top comment says. Never background and poll.

## 2. The tool fact

Apostrophes in quoted heredocs break; doubled backslashes collapse; `;` is refused; `rm`
is refused; Python cannot run here; `-m` more than once for a multi-line commit.

## 3. Asks still outstanding

Carried per HM-DEC-139 from unit 333's queue, **verbatim in section 4**. This unit
touches: the five files that cannot be deleted (listed once in section 2 for Tim); the
stale *cannot work* line (removed here).

---

## 4. Why this unit exists

```
PHASE GOAL: The screen says what is true and looks like someone meant it.
UNIT GOAL:  Step 0 - the green zone without the band pills, the map wider by
            their space, the stale line gone, the leftover files listed.
ADVANCES:   Step 0, wholly.
DRIFT:      0. A new phase starts the count.
```

**Read `PHASE_PLAN.md` at the root in full.** §R21 is this step. §R22-§R25 are the
steps after it. §6 carries the later-ruling-wins rule and the watchdog rule.

**Tim, 2026-09-12, on unit 332's green zone:** *"Too redundant. We don't need the repeat
of the band list on the green. Maybe make the map bigger."*

---

## 5. Verify this instruction against the tree

- `PHASE_STATUS.md` line 1 names *The screen says what is true and looks like someone
  meant it* and carries four steps. **If it is still the PSK31 phase, stop and say so** -
  `install-phase.bat` did not run.
- `docs\phase-psk31-run\` holds the PSK31 phase's three files.
- The green zone as unit 332 left it: left block, center map with the terminator and
  the operator's dot, right block with the band pills, the *you are on it* line, the
  sparkline and count, the rule-of-thumb line under the map. `TheGreenZoneTests`.
- The line *Hamlet cannot work CW, PSK31 and Voice yet* - where it lives, if 332 did not
  already remove it.
- The files units 322-332 could not delete: `commit-msg-326.txt`,
  `toolsarbitervalidate-output.bat`, `tools\arbiter\unit323-append.bat`, `.py`,
  `tools\cut-header-action.py`, and any other emptied-with-a-comment file.
- `PROJECT_CARD.md`'s `PHASE` and `PHASE_SET`.

**Report every mismatch; repair nothing.**

## 6. Rulings in force

**`PHASE_PLAN.md` §R21-§R25 and §6.** The PSK31 plan's R1-R20 stand from
`docs\phase-psk31-run\PHASE_PLAN.md`. **§0.0** every appearance claim is computed and
says so. **§0.5**, **§0.6**. **HM-DEC-155**, **HM-DEC-139**, **FACT-004**, **FACT-006**.

## 7. Status cadence

As the header says.

---

## 8. The tasks

### Task 0 - the phase opens

Append `UNIT 334` to `PHASE_OUTCOME.md` under step 0. Patch-bump. Set `PROJECT_CARD.md`'s
`PHASE` and `PHASE_SET` to this phase's. Record in `DECISIONS.md`, as Tim's ruling of
2026-09-12: the maintenance phase set - *"do the cleanup stuff I've been talking about.
We're calling this a maintenance phase"* - and that the PSK31 phase is archived with its
step 6 open, to be closed by him at the radio. Run the carry-forward list, status first.

**Drop candidate:** none.

### Task 1 - the band pills come off the green zone

Remove the pills, their bars and the *you are on it* line from the green zone's right
block - they are the row above the panel. **The map widens by the space they held**;
state the width before and after. The heard-just-now count stays on the right as a
number with its sparkline if it still fits, the number alone if not. The rule-of-thumb
line stays under the map. The left block is unchanged.

**Test watched failing first:** `TheGreenZoneTests`, rewritten under R12: no band pill in
the green zone; the map's width grew by the stated number; the left block, the count and
the rule-of-thumb line present; the panel uses at least 90% of its width;
`BindingHealthTests`, `VoiceTests`.

**Drop candidate:** the sparkline. Keep the number.

### Task 2 - the stale line

*Hamlet cannot work CW, PSK31 and Voice yet* exists nowhere in `src/` or in any
operator-facing string. If 332 removed it, assert that and move on.

**Test watched failing first:** extend `VoiceTests` by one string check.

**Drop candidate:** none.

### Task 3 - the leftovers, listed once

Every file a session emptied because it could not delete it: **one list in section 2 of
the report**, full paths, for Tim to remove by hand. Any of them not yet emptied is
emptied now with a one-line comment.

**Test watched failing first:** none.
**Drop candidate:** none.

---

## 9. Parked

- **Steps 1 and 2.** The arbiter authors them from the plan.
- **Anything touching the radio, a decoder, a parser or the transmit chain.**
- **Any package.**

## 10. What not to do

- **No unfiltered `dotnet test`. Never background and poll. Never compose a timestamp.**
- **Do not touch the radio side.** Screen only.
- **Do not put station dots or paths on the green zone map.**
- **No image assets. No package. Report mismatches; repair nothing. Write American.**

## 11. Committing and pushing

Commit per task; push at the end.

---

## 12. Reporting

`output.md` at the root. **Canonical headings:** `## 1. What Claude did`, `## 2. What the
owner should expect`, `## 3. What you should see`, `## 4. What's blocking us`.

```
READ IN THIS ORDER.

A. The phase goal - the screen says what is true and looks like someone meant it.
   Step 0 <state after this unit>, 1-3 not started.
B. Step 0 and its five must-pass - each met or not, with the number.
C. The report last, and section 4 raises N items on top of the carried queue.
```

```
UNIT:       334 - <complete|stopped> at task N of 4 - <date time>
PHASE GOAL: <restated in your own words>
UNIT GOAL:  <restated in your own words>
ADVANCED:   <yes|no>
NUMBER:     green zone map width <before> -> <after> px
DRIFT:      0
```

**Section 2 carries the one list of files for Tim to delete.** **Every appearance claim
is computed, not seen. Say so once.**

---

```
ARBITER-DECISION
STEP: 0
APPROACH: remove the band pills from the green zone and widen the map by their space, remove the stale cannot-work line, list the undeletable files once, and open the phase in the card and the decisions file
MOVE: continue
WHY: step 0 depends on nothing; every part of it is a ruling already made on the owner's own screen
STATE: not started
DECIDED: nothing beyond the plan; the map's new width is measured, not chosen
LICENCE: PHASE_PLAN.md R21, section 6; docs/phase-psk31-run/PHASE_PLAN.md R12, R14, R19; CLAUDE.md 0.0, 0.5, 0.6
ACCOMPLISHED: the green zone no longer repeats the band row and the map is the size Tim asked for
ADVANCES: step 0 - all five must-pass
END-ARBITER-DECISION
```
