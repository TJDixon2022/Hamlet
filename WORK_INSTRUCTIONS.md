# Work instruction 328 - the mark is a quill again, and step 5 is done on the record

**Seed under `--seed`, one iteration, called under step 5.** Two things. **First, the
mark**: unit 327 made it an 18 px filled disc, and Tim's word on it was *"so so so so so
ugly."* He chose, from three drawn treatments, **a thin quill in the gutter** - the tray's
vane at row scale, hairline, not filled. **Second, the record**: unit 326 built step 5 in
full while called under step 4, so the judge never graded step 5 and `PHASE_STATUS.md`
still reads `STEP: 5 | not started`. This unit reruns step 5's four must-pass and
appends under step 5. Four tasks.

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

**HM-DEC-155.** No suite; only the names below and `docs\carry-forward-tests.txt` as its
top comment says. Never background and poll. **Status before every `dotnet` command.**

## 2. The tool fact

Apostrophes in quoted heredocs break; doubled backslashes collapse; `;` is refused; `rm`
is refused; `-m` more than once for a multi-line commit message.

## 3. Asks still outstanding

Carried per HM-DEC-139 from unit 327's queue, **verbatim in section 4**. None touched
here. The four files that need deleting by hand are Tim's and are listed again in
section 2.

---

## 4. Why this unit exists

```
PHASE GOAL: Hamlet works PSK31 the way it works FT8.
UNIT GOAL:  The achievement mark on a list row is the quill, thin, in the gutter -
            green for a counter, orange with a ring for a door - and the record
            for step 5 matches the tree.
ADVANCES:   Step 5, from not started to done, on tests that already exist. The
            mark is carried repair.
DRIFT:      carried.
```

Step 5's four must-pass, from `PHASE_PLAN.md`:

1. A PSK31 contact logs with an RST field, and the FT8 dB field is not reused for it.
2. The ADIF export carries `MODE=PSK` and `SUBMODE=PSK31`.
3. Before the first PSK31 contact no PSK31 card is visible on the achievements screen;
   after it, the mode's records appear.
4. The two inherited reds in `TheAchievementsScreenTests` are not made worse.

Unit 326 met all four and named its tests; unit 327 added `GRIDSQUARE` to the same log
entry. **This unit runs those tests and writes the result under step 5.**

---

## 5. Verify this instruction against the tree

- `AchievementMarkControl.cs` as unit 327 left it: the 18 px filled disc, the ring and
  the turn for a door, the press that opens the popup, the colors (green `#3B6D11`, the
  orange hex 327 chose). The tray quill from units 300-303 - its vane path - which this
  mark is drawn from.
- `TheMarkIsSeenAndClickedTests` (unit 327) - **rewritten under §R12** for the new shape;
  the popup assertions unchanged.
- `ThePsk31LogsWithRstTests` (unit 326, extended by 327), the ADIF record test that
  compares whole FT8 and FT4 records byte for byte, the PSK31 records test on the
  achievements screen, and `TheAchievementsScreenTests` with its two known reds.
- `PHASE_STATUS.md` line `STEP: 5 | not started`; `PHASE_OUTCOME.md` with unit 327 under
  step 6.

**Report every mismatch; repair nothing.**

---

## 6. Rulings in force

**`PHASE_PLAN.md` §R1-§R19**, unchanged. **§R16** two forms, no cap. **§R12** the
session rewrites its own test for the new shape. **§R14** no tests beyond the
criterion. **§0.6** color never the only carrier - the ring is the door's shape.
**Tim, 2026-09-12:** treatment A - *thin quill in the gutter, ring for a door*.
**HM-DEC-155**, **HM-DEC-139**, **FACT-004**, **FACT-006**.

## 7. Status cadence

Before every `dotnet` command, and after every task.

---

## 8. The tasks

### Task 1 - append under step 5, and the carry-forward list

Append `UNIT 328` to `PHASE_OUTCOME.md` **under step 5**. Patch-bump. Run the carry-forward
list as its comment says. Report the count.

**Drop candidate:** none.

### Task 2 - the mark is a thin quill in the gutter

Replace the filled disc with **the tray's quill vane at row scale**: a hairline stroke,
**about 12 px tall, 1.5 px wide, not filled**, sitting in the gutter left of the time.
**Counter: green `#3B6D11`, the vane alone, still. Door: orange, the vane inside a
hairline ring, turning** - the ring is the shape difference (§0.6); the turn stays as
327 built it. A worked, faded station: no mark. **The press and the popup are unchanged.**
Report the before and after: disc 18 px filled versus vane 12 px hairline, with the
orange hex.

**Test watched failing first:** `TheMarkIsSeenAndClickedTests`, rewritten for the shape:
the mark is a stroked path, not a filled ellipse; its height is about 12 px; a counter has
no ring; a door has the ring and turns; a worked row has none; pressing opens the popup as
before; `BindingHealthTests` green.

**Drop candidate:** the turn. Keep the ring.

### Task 3 - step 5's four must-pass, each run and quoted

Run, filtered, status first, each of the four tests or types that prove the four criteria
above. **For each: the test name, green or red, and the value it asserts** - the RST as
logged, the ADIF `MODE`/`SUBMODE` pair as printed, the count of PSK31 cards before and
after the first contact, and `TheAchievementsScreenTests`' red count before and after
(expected 2 and 2). Also the one 327 added: `GRIDSQUARE` present when read, absent when
not.

**No new test. No production change.** If any of the four is red, **report it and stop**;
that is a finding about unit 326, not this unit's to fix.

**Drop candidate:** none.

### Task 4 - the report

Written so the state judge, which reads `PHASE_PLAN.md`'s step 5 section and this
report, can see each criterion met by a named test that ran in this session.

**Drop candidate:** none.

---

## 9. Parked

Everything but the mark's shape. Step 6 is Tim's.

## 10. What not to do

- **No production change beyond `AchievementMarkControl` and its test. No filled shape.
  No new test beyond the rewrite. No unfiltered run.** No package. No edit to the phase
  files beyond the append. Report mismatches; repair nothing. Write American.

## 11. Committing and pushing

One commit for the version and the append; push.

---

## 12. Reporting

`output.md` at the root. **Canonical headings:** `## 1. What Claude did`, `## 2. What the
owner should expect`, `## 3. What you should see`, `## 4. What's blocking us`.

```
READ IN THIS ORDER.

A. The phase goal - Hamlet works PSK31 the way it works FT8. Steps 0 to 4 done,
   5 done after this unit on tests that ran tonight, 6 not started.
B. Step 5's four must-pass - each met, the test named, the value quoted.
C. The report last, and section 4 carries the queue unchanged.
```

```
UNIT:       328 - <complete|stopped> at task N of 4 - <date time>
PHASE GOAL: <restated in your own words>
UNIT GOAL:  <restated in your own words>
ADVANCED:   yes - step 5 to done
NUMBER:     step 5 must-pass 4 of 4, by test; mark 18 px filled disc -> 12 px hairline quill
DRIFT:      <n> consecutive units without advance
```

**Section 2 tells Tim what the mark looks like now, that nothing else on his screen
changed, and repeats the four files to delete by hand.**

---

```
ARBITER-DECISION
STEP: 5
APPROACH: replace the filled disc with the tray quill vane at row scale - green alone for a counter, orange in a ring for a door - and rerun step 5's four must-pass so the record matches the tree
MOVE: continue
WHY: unit 326 built step 5 while called under step 4, so the judge never graded step 5; the work is done and green and the record says not started; nothing else in the phase is between step 5 and Tim's verdict
STATE: not started
DECIDED: the vane's exact size at row scale is the unit's to state from the tray's path; the test is rewritten for the shape under R12
LICENCE: PHASE_PLAN.md step 5 entry and exit; R12, R14, R16; CLAUDE.md 0.6; Tim 2026-09-12 treatment A; HM-DEC-155
ACCOMPLISHED: the mark on Tim's list is the quill he already knows from the tray, thin and in the gutter, and the phase record says step 5 is done, so the only step left is Tim at the radio
ADVANCES: step 5 - all four must-pass, evidenced by tests run this session
END-ARBITER-DECISION
```
