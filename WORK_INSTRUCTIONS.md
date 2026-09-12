# Work instruction 332 - the achievements page clicks in, the green zone shows the world's clock, and the tray gets a feather

**Seed under `--seed`, `--max-iterations 999`.** Three things Tim ruled on 2026-09-12 after
seeing unit 331's screen. All screen. Carried repair before step 6, which is his.
**Six tasks. Drop from the back. Every task commits on its own.**

**Status.** `tools/status.sh` reads the clock; **use it for every write and never compose
a time**. Three units running have written made-up timestamps; unit 331 wrote fourteen.
If the helper cannot run, read the clock with `date` and paste it. **The watchdog may
still be the twelve-minute file rule when this runs** - write status before every
`dotnet` command, after every commit and after every task.

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
top comment says. Never background and poll. Status as above.

## 2. The tool fact

Apostrophes in quoted heredocs break; doubled backslashes collapse; `;` is refused; `rm`
is refused; Python cannot run here; `-m` more than once for a multi-line commit.

## 3. Asks still outstanding

Carried per HM-DEC-139 from unit 331's queue, **verbatim in section 4**. Touched here:
the 1400 px split (task 2 renders the page at 1400 and 1920 and reports both); the
`Digital` versus `Data` word - **Tim has not overruled; `Digital` stands**; the five files
to delete by hand (listed again in section 2); States scoring zero (unchanged; raised).

---

## 4. Why this unit exists

```
PHASE GOAL: Hamlet works PSK31 the way it works FT8.
UNIT GOAL:  The achievements page is eight badges and nothing else, and a badge
            clicks in to its category. The green zone is the world with the
            night side drawn and Tim's grid on it. The tray mark is a feather.
ADVANCES:   No step. Step 6 is Tim's.
DRIFT:      carried.
```

**Tim, 2026-09-12, on unit 331's achievements window:** *"This is wrong. It should open
like this, but not scroll. When you click on a category, that category replaces it. It's
a click-into system. Also the text is not fitting."*

**On the green zone:** *"I said this was wasted real estate on the right so you just put
more on the left."* Then, on the card's map: *"We ought to use something like this with a
dark/light indicator to show where the band can reach right now. Looking for contacts in
eastern Europe at 2:00 pm EST is not likely."* Then: *"without the orphan dots, maybe just
a dot at my grid"*, *"a little darker in the dead zone"*.

**On the tray mark at 27 px:** the row-scale vane enlarged is *"the eye of Sauron with an
infection."* *"Just make it look attractive."*

`assets/` carries three mockups Tim has seen and approved the direction of - the
achievements opening page, the green zone, and the day-night map at 2 pm EDT with only
his dot. **They are for reading. Nothing in `src/` references them. Everything is drawn
as vector paths in the app.**

---

## 5. Verify this instruction against the tree

**Names from units 300-331's reports.** Check; **report every mismatch; do not repair this
instruction; do not stop over a mismatch** unless a task is impossible.

- The achievements window as 331 left it: 820 × 720, one scroller, the badge page on top
  and the old card view, the contact belt, *Your best*, the file path and the *cannot work
  CW, PSK31 and Voice yet* line below. The badge control, its text bindings, where text
  clips. `data/achievements/achievement-points.json` and the `%AppData%` seed.
- The green zone as 331 left it: two lines, left-aligned, full width; the band strip
  data - the cited rows under `data/bands/` for 20 m's segments; the band pills and their
  bars; the *heard just now* dots; `OperatorLocation.FromGrid`, `FlatWorldMap.Relief` and
  the placement, `Ft8GlobeControl` and how the card draws the map.
- The tray mark: `AchievementMarkControl` at 27 px, the vane path, the count badge beside
  it; the row-scale mark (6 × 12 px) **which is not touched**.
- Tests: `TheAchievementsPageTests`, `TheGreenZoneTests`, the tray size tests,
  `BindingHealthTests`, `VoiceTests`.

---

## 6. Rulings in force

**`PHASE_PLAN.md` §R1-§R19.** **§R12** own tests; **§R14** nothing beyond the criterion;
**§R16** two quills; **§R19** American.

**Tim, 2026-09-12**, quoted in section 4. **Ruling C** on the opening page: every kind
shown, the nearest unearned card in each, nothing beyond it. **Scores by difficulty from
the file**; **the running total on the page**. **`Digital`** is the family word.

**§0.0** - the night side is computed from the sun and the clock, which is fact; **band
openness is a forecast Hamlet has no source for and never draws.** A rule of thumb is
stated in words as a rule of thumb. **§0.5** family color is text only. **§0.6** color is
never the only carrier. **`ACHIEVEMENTS_PHILOSOPHY.md` §2** no wall of blanks; **§3.1**
inside a category nothing shows until something adjacent is earned; **§4** worked, never
confirmed.

**HM-DEC-155**, **HM-DEC-139**, **FACT-004**, **FACT-006**, **the dummy load withdrawn.**

## 7. Status cadence

As the header says. `tools/status.sh`, real clock, before every `dotnet`, after every
commit, after every task.

---

## 8. The tasks

### Task 0 - housekeeping

Append `UNIT 332` to `PHASE_OUTCOME.md` under step 6; patch-bump; run the carry-forward
list. **Empty and comment** the five files that cannot be deleted if any still has
content; list them once in section 2. Remove the line *Hamlet cannot work CW, PSK31 and
Voice yet* wherever it lives - it is false on two of three.

**Drop candidate:** none.

### Task 1 - the achievements page is eight badges, and a badge clicks in

**The page is the badges and nothing else.** Title, the running total line, eight badges
in two rows of four, the one-line legend. **No scroller. Nothing below.** The old card
view, the contact belt, *Your best* and the file path are **gone from this page**; the
file path moves to the settings screen if it is wanted anywhere.

**Click a badge and the category replaces the page** in the same window and frame:

- a **back control** top-left - `‹ All achievements` - returning to the eight
- the category's emblem, name and color band across the top; its score, level and gap
  beside them; its meaning line under
- **the cards**: earned cards as the screen draws them today, then **the nearest unearned
  one**, nothing beyond it (§3.1); every card shows its points
- **Continents** opens to **seven continent badges** in the same shape, each with its own
  emblem, and each clicks in to its countries with the same back control
- **Total Miles** opens to its tiers with the running sum and a bar to the next

**Text fits, everywhere.** No badge, card or line clips or wraps a word. Measure the
longest string in each slot - *Your first PSK31 contact*, *A first outside North America*,
*0 pts · unranked · 10 to Bronze* - and size the slot to it, or shorten the string and
say which. `VoiceTests` runs.

**Telemetry:** `achievements_opened`, `achievement_category_opened` (kind only),
`achievement_category_closed`.

**Test watched failing first:** `TheAchievementsPageClicksInTests`, app: the page has no
scroller and no content below the legend; clicking a badge replaces the page with its
category; the back control returns; inside, earned cards then one unearned; Continents
opens to seven and each to its countries; no string in any slot is clipped at the
window's size, asserted by measuring; `BindingHealthTests`, `VoiceTests`.

**Drop candidate:** the continent-to-countries second level. Keep the first level.

### Task 2 - the green zone is the world's clock

**The whole width, three regions, drawn where a picture says it** - `assets/green-zone-mockup.png`
is the shape; the map replaces the band strip's place as the centerpiece:

**Left - where you are.** The band in the largest text on the panel, in family color;
the frequency beside it; under it the mode and *yours to use*; under that, small, the
license phrase and citation on one line.

**Center - the world with its night side.** `FlatWorldMap.Relief` as the card draws it,
at the panel's height, with **the night side darkened** - the solar terminator computed
from the clock and the date, a six-degree twilight fade so the gray line reads as a line -
and **one marker: the operator's grid**. No station dots, no paths. It updates with the
clock. `assets/grayline-1400edt.png` is what it looks like at 2 pm EDT on 2026-09-12 with
the darkness Tim chose. **Under the map, one line of rule of thumb, stated as such**: *20 m
and up want daylight along the path; 40 m and down want dark; the gray edge is where
both happen* - and nothing about whether the band is open, because Hamlet does not know.

**Right - what is worth doing now.** The band pills as they are, the one you are on
filled with *you are on it* under it; under them a **sparkline of stations heard here
over the last minute** with the count. Under those, the band strip from the earlier
mockup - 20 m's segments from the cited rows with the dial marked - **if it fits at the
panel's height**; if not, drop the strip and say so.

**Telemetry:** `green_zone_rendered` once per session with the panel's dimensions and
which regions rendered; `terminator_computed` with the subsolar longitude and declination
and nothing else.

**Test watched failing first:** `TheGreenZoneTests`, rewritten: the band is the largest
text and in family color; the map region renders with a terminator whose subsolar
longitude matches the hand-computed value for a fixed clock within 1°; exactly one
marker, at the operator's grid; the rule-of-thumb line is present and contains no claim
of openness; the pills mark the current band; the sparkline reflects the heard count;
the panel uses at least 90% of its width; `BindingHealthTests`, `VoiceTests`.

**Drop candidate:** the sparkline. Keep the count as a number.

### Task 3 - the tray mark is a feather

At 27 px the row-scale vane enlarged is an almond with a slit. **Draw a quill for this
size**: a curved shaft from nib to tip, barbs on both sides with a notch or two, a nib
at the base - a feather anyone would name as one, in `#3B6D11`, the count badge beside
it as now. Vector path, no image. **The row-scale mark (6 × 12 px) is not touched.**

**Test watched failing first:** the tray size test extended: the 27 px mark is a path
with more than one subpath (shaft and barbs), not the row vane scaled; drawn height 27;
`BindingHealthTests`.

**Drop candidate:** none.

### Task 4 - the card's map keeps its dots; the night side is optional there

Tim ruled the green zone map carries only his dot. **The card's map is unchanged** - both
markers and the path. **If the terminator layer from task 2 can be reused there at no
cost, add it under the path and say so; if it costs anything, leave the card alone.**

**Test watched failing first:** none new; `TheGlobeOnTheCardFaceTests` still green.

**Drop candidate:** the whole task.

### Task 5 - render at two widths and report

Stand the app up at 1400 and at 1920 and describe, computed, what the achievements page,
a category page, the green zone and the tray look like at each. Say what clips, if
anything, and where the 1400 split lands.

**Drop candidate:** the whole task.

---

## 9. Parked

- **Rank names.** Tim's.
- **States from the ADIF `STATE` field.** Raised; not here.
- **Step 6.** Tim's.
- **Any change to what keys. Any package.**

## 10. What not to do

- **No unfiltered `dotnet test`. Never background and poll. Never compose a timestamp.**
- **Do not scroll the achievements page.** Click in, click back.
- **Do not draw band openness.** The sun is fact; propagation is not.
- **Do not put station dots or paths on the green zone map.**
- **Do not scale the row vane to 27 px.** Draw a feather.
- **Do not clip or wrap a word anywhere on the achievements page.**
- **Do not add image assets.** Vector.
- **Do not touch what keys. No package. No edit to the phase files beyond the append.
  Report mismatches; repair nothing. Write American.**

## 11. Committing and pushing

Commit per task; `-m` more than once for multi-line; push at the end.

---

## 12. Reporting

`output.md` at the root. **Canonical headings:** `## 1. What Claude did`, `## 2. What the
owner should expect`, `## 3. What you should see`, `## 4. What's blocking us`.

```
READ IN THIS ORDER.

A. The phase goal - Hamlet works PSK31 the way it works FT8. Steps 0 to 5 done,
   6 waits on Tim - unchanged by this unit.
B. No step criterion moves; carried repair on the screen step 6 is judged from.
C. The report last, and section 4 raises N items on top of the carried queue.
```

```
UNIT:       332 - <complete|stopped> at task N of 6, <which dropped> - <date time>
PHASE GOAL: <restated in your own words>
UNIT GOAL:  <restated in your own words>
ADVANCED:   no
NUMBER:     achievements page scroll height <before> -> 0; green zone width used
            <before>% -> <after>%; longest clipped string <before> -> none
DRIFT:      <n> consecutive units without advance
```

**Section 3 describes the achievements page, one category page, the green zone at 2 pm
and at 10 pm, and the tray, computed, in enough words that Tim knows what to look for.**

---

```
ARBITER-DECISION
STEP: 6
APPROACH: make the achievements page eight badges with no scroller and a click-in category view with a back control and text that fits; make the green zone the world map with its computed night side and the operator's dot, the band big on the left and the pills and heard count on the right; draw the tray mark as a feather
MOVE: continue
WHY: three rulings from the owner on unit 331's screen, none touching a step criterion; step 6 is his
STATE: blocked
DECIDED: the category page layout, the twilight fade width, the rule-of-thumb wording and the feather path are the author's shapes marked for the owner; the terminator arithmetic is the unit's and is asserted against a hand-computed value
LICENCE: PHASE_PLAN.md R12, R14, R16, R19; Tim 2026-09-12 as quoted; CLAUDE.md 0.0, 0.5, 0.6; ACHIEVEMENTS_PHILOSOPHY.md 2, 3.1, 4
ACCOMPLISHED: the achievements window is a page of badges Tim can click into and back out of, the green zone shows him where the sun is and where he is, and the tray mark is a feather rather than an almond
ADVANCES: none - carried repair
END-ARBITER-DECISION
```
