# Work instruction 337 - the main window is the mockup

**Seed of the screen phase, under `--seed`.** Step 0 of `PHASE_PLAN.md`. The arbiter
authors steps 1 and 2 after it; step 3 is Tim's. **Five tasks.**

**Status.** `tools/status.sh` reads the clock; use it for every write. The watchdog now
polls the process; it kills only a session whose process tree accrues no CPU for ten
minutes. Write status before every `dotnet` command and after every task anyway - it is
what Tim reads.

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

Carried per HM-DEC-139 from unit 336's queue, **verbatim in section 4**. This unit touches
the 1400 split (R26 supersedes R24's mechanism; the outcome stands) and the card's facts
beside the map.

---

## 4. Why this unit exists

```
PHASE GOAL: The screen, done right.
UNIT GOAL:  Step 0 - the main window laid out as assets/main-screen-mockup.png:
            one short top row about where you are, the working panels given
            the height, at 1920 and at 1400.
ADVANCES:   Step 0, wholly.
DRIFT:      0. A new phase starts the count.
```

**Tim, 2026-09-12, on unit 334's window** (`assets/screenshot-334-green-zone.png` if
present; otherwise his words): the green zone grew to two thirds of the window; the
waterfall, the decoded list and For You were squeezed into the bottom third; a third of
the window under the rig display stood empty. *"Mock up what Hamlet main screen will look
like if you do this right."* The mockup is the ruling. **Read `PHASE_PLAN.md` R26 in
full; it is this step's outcomes, and it prescribes no mechanism.**

**What the mockup shows, in numbers at 1920 × 810:**

- Header and band pills: unchanged, to about y = 100.
- **Top row, y 110 to 296.** Left, the neighborhood card 14-1000 px wide: the band strip
  with legend and the *you · FT8* marker on rows 1-2; the green block 28-720 × 210-282
  with `20 m` at 16 pt bold, the frequency/mode/*yours to use* line, the license line
  small, the rule-of-thumb line small, and heard-just-now with count and sparkline at its
  right; **the world clock 246 × 134 at the card's right end** with the operator's dot
  only and *where the sun is · you* under it. Right, the rig display 1012-1488, the same
  height, with the frequency at 40 pt, the S-meter, the scroll-wheel hint, and **a line
  for transmit drive and the RF power offer** under it.
- **Tabs at y 310; the working card from y 336 to 770.** Inside: waterfall 28-640,
  decoded text 652-1068, For You 1080-1474, **all from y 376 to 758**. The conversation
  card's facts sit beside its map at 190 px.
- Status bar with the feather and the count, unchanged.

**These are the mockup's numbers, not a specification.** The outcomes in R26 are what
must hold; the unit measures the real controls and chooses, marks its choices as its
own, and reports what it built at both widths.

---

## 5. Verify this instruction against the tree

- `PHASE_STATUS.md` line 1 names *The screen, done right* with four steps. **If not, stop
  and say so.** `docs\phase-maintenance-run\` holds the maintenance phase's files.
- The neighborhood card, the green zone as unit 334 left it (map filling the height),
  the world clock control and its terminator, the rig display panel and where drive and
  the power offer live today (the digital tab's send area), the tabs, the working card,
  the three panels and the bar above them, the split unit 336 left at `383,*`, the
  conversation card's map row and facts table.
- `TheGreenZoneTests`, `ThePanelsMakeRoomTests`, `TheCardsRightColumnTests`,
  `ThePanelScrollsTests`, `TheFilterIsAlwaysThereTests`, `BindingHealthTests`, `VoiceTests`.
- `PROJECT_CARD.md`'s `PHASE` and `PHASE_SET`.

**Report every mismatch; repair nothing.**

## 6. Rulings in force

**`PHASE_PLAN.md` R26 and §6.** The PSK31 and maintenance rulings stand. **§0.0** every
appearance claim is computed and says so. **§0.5**, **§0.6**. **HM-DEC-155**,
**HM-DEC-139**, **FACT-004**, **FACT-006**. **The arbiter and this session stop for three
things only**; layout is not one of them - measure, choose, mark, continue.

## 7. Status cadence

As the header says.

---

## 8. The tasks

### Task 0 - the phase opens

Append `UNIT 337` to `PHASE_OUTCOME.md` under step 0. Patch-bump. Set `PROJECT_CARD.md`'s
`PHASE` and `PHASE_SET`. Record in `DECISIONS.md`, as Tim's ruling of 2026-09-12: the
screen phase set on the mockup, replacing the maintenance phase, which is archived with
step 2 partial. Run the carry-forward list, status first.

**Drop candidate:** none.

### Task 1 - the top row

The neighborhood card and the rig display become one band of the mockup's height, and
the working card below gets the rest. **The green block moves inside the neighborhood
card** under the strip and legend, compact, with the band as its largest text, the
count and sparkline at its right. **The world clock moves into the card's right end** at
the mockup's size, one dot. **Drive and the power offer move under the rig's S-meter**
and leave the send area; the send area keeps CQ and Stop. Measure and report: the top
row's height, the working card's height, at 1920.

**Test watched failing first:** `TheTopRowTests`, app: the top row's height at 1920 is
within 10% of 190 px; the working card takes the rest to the status bar; the green block
is inside the neighborhood card with the band as its largest text; the world clock is in
the card at its right with one marker; drive and the power offer are in the rig panel and
not in the send area; `BindingHealthTests`, `VoiceTests`.

**Drop candidate:** the sparkline. Keep the count.

### Task 2 - the working panels

Waterfall, decoded text and For You are equal height, full from the mode chips to the
status bar. The decoded list's width is what its longest line needs and no wider -
measure the longest FT8 line at the list's font; For You takes the rest. At 1920 the
conversation card's facts sit beside its map at the map's width. Report the split as
pixels and as a fraction at 1920.

**Test watched failing first:** `TheWorkingPanelsTests`, app: the three panels share one
top and one bottom; the decoded list's width equals its stated need within 10 px; at 1920
the card's facts are beside the map; no callsign is clipped; `ThePanelScrollsTests`,
`TheFilterIsAlwaysThereTests`, `TheCardsRightColumnTests`.

**Drop candidate:** none.

### Task 3 - 1400

Render at 1400. **The same shape holds**: top row short, working panels the rest, no
callsign clipped. Where the card's facts cannot sit beside the map, they go under it.
**Choose the rule, mark it as the unit's, report the numbers.** Nothing is prescribed;
R24's *fraction* is superseded by R26's outcomes.

**Test watched failing first:** extend `TheWorkingPanelsTests`: at 1400 the working
panels are at least half the height below the band pills; no callsign is clipped; the
facts are beside or under by the stated rule.

**Drop candidate:** none.

### Task 4 - stand it up at both widths and describe it

**Drop candidate: this whole task.** Stand the app up at 1920 and 1400 and describe each
region in section 3, computed, beside the mockup's numbers, so Tim knows what to look
for.

---

## 9. Parked

- **Steps 1 and 2.** The arbiter authors them.
- **Anything touching the radio, a decoder, a parser or the transmit chain.**
- **Any package.**

## 10. What not to do

- **No unfiltered `dotnet test`. Never background and poll. Never compose a timestamp.**
- **Do not let the top row grow past the mockup's proportion.** The working panels are
  the window.
- **Do not put station dots or paths on the world clock.**
- **Do not stop on a layout choice.** Choose, mark, continue.
- **No image assets. No package. Report mismatches; repair nothing. Write American.**

## 11. Committing and pushing

Commit per task; push at the end.

---

## 12. Reporting

`output.md` at the root. **Canonical headings:** `## 1. What Claude did`, `## 2. What the
owner should expect`, `## 3. What you should see`, `## 4. What's blocking us`.

```
READ IN THIS ORDER.

A. The phase goal - the screen, done right. Step 0 <state after this unit>, 1-3
   not started.
B. Step 0 and its six must-pass - each met or not, with the number.
C. The report last, and section 4 raises N items on top of the carried queue.
```

```
UNIT:       337 - <complete|stopped> at task N of 5 - <date time>
PHASE GOAL: <restated in your own words>
UNIT GOAL:  <restated in your own words>
ADVANCED:   <yes|no>
NUMBER:     top row height <before> -> <after> px; working panels height
            <before> -> <after> px at 1920; the same at 1400
DRIFT:      0
```

**Section 3 puts the mockup's numbers beside the built numbers, region by region, at
both widths.** **Every appearance claim is computed, not seen. Say so once.**

---

```
ARBITER-DECISION
STEP: 0
APPROACH: lay the main window out as the approved mockup - a short top row holding the neighborhood card with the green block and the world clock and a rig display of the same height with drive and power under it, the working panels full height below the tabs, at 1920 and 1400 - measuring the real controls and choosing the mechanism
MOVE: continue
WHY: step 0 depends on nothing; the owner drew the outcome and ruled it; the plan prescribes no mechanism, so nothing here is a stop
STATE: not started
DECIDED: every mechanism - the split, the 1400 rule, the exact heights - is the unit's, marked as its own, overrulable
LICENCE: PHASE_PLAN.md R26, section 6; the PSK31 and maintenance rulings; CLAUDE.md 0.0, 0.5, 0.6
ACCOMPLISHED: the main window looks like the mockup Tim approved, and the working panels have the window again
ADVANCES: step 0 - all six must-pass
END-ARBITER-DECISION
```
