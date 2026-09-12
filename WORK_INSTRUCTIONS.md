# Work instruction 330 - maintenance round one, and the achievements page

**Seed under `--seed`, one iteration. Tim is away for two hours; this runs unattended.**
Six things he saw on 2026-09-12, discussed and ruled, all screen. **Seven tasks. Drop
from the back. Every task commits on its own, so what lands, lands.** The watchdog kills
at twelve minutes of silence: **write `PROJECT_STATUS.md` before every `dotnet` command
and after every task, without exception.**

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
top comment says. Never background and poll. **Status before every `dotnet` command.**

## 2. The tool fact

Apostrophes in quoted heredocs break; doubled backslashes collapse; `;` is refused; `rm`
is refused; `-m` more than once for a multi-line commit; `validate-output.bat` has been
refused four units running - try once, then check the seven rules by hand.

## 3. Asks still outstanding

Carried per HM-DEC-139 from unit 328's queue, **verbatim in section 4**. Touched here:
the `MainWindow.axaml` comment that still calls the mark a filled disc (task 1 corrects
it in passing); the four files to delete by hand (listed again in section 2).

---

## 4. Why this unit exists

```
PHASE GOAL: Hamlet works PSK31 the way it works FT8.
UNIT GOAL:  Five things on the screen that are wrong or wasted, fixed; and the
            achievements page rebuilt as eight badges with scores.
ADVANCES:   No step. Step 6 is Tim's.
DRIFT:      carried.
```

**Tim, 2026-09-12, with screenshots:**

1. *"The icon in the tray is tiny and hard to notice."* The tray quill beside the `28`
   count is a green sliver a few pixels tall.
2. *"The right of globe text is not aligned or even, make it nice. It's a jumbled mess."*
   The card's column reads `3,000 miles · south / Grid FI07 / His time: 09:21 by the sun /
   2 messages  show the messag / New country` - ragged, one line truncated, and *show the
   messages* appears twice on the card.
3. *"The achievement indicator popup is not pretty or interesting."* It reads `LA8ENA /
   Norway · a new country · you have worked 3 in Europe / A QSO first. The card comes
   with it.` Three lines of gray text in a box.
4. *"The green zone is wasting a lot of real estate."* Under the neighborhood map:
   `14.074 MHz · yours to use / Your General license covers digital modes here. /
   97.305(c)(3)(ix)` - three lines, a full-width green panel, and it says the same thing
   every time.
5. *"The message we put up about slots after a transmit is wrong. We transmitted that
   slot so nothing could be heard."* The readiness line said *one slot decoded, and
   nothing on the band looked like FT8 at all* about the slot Hamlet was transmitting in.
6. *"I want to rework the achievements dialog. The opening dialog page should be a list
   of badges indicating type of achievements. Nice badges, real nice."* And, after
   discussion: **scores by difficulty** - *"we're moving away from the achievements all
   count the same… getting all continents is hard… 10 million is hard… the points don't
   matter"* - a **running total on the page**, and a new kind, **Total Miles**.
   *"I really didn't know 14.070 was 20 meters until recently"* - which is task 4.

---

## 5. Verify this instruction against the tree

**Names from units 300-328's reports.** Also: the achievements screen as it stands -
`AchievementScreen`, `AchievementChallenges.For`, `AchievementLog`, the card face, the six-row
window, `TheAchievementsScreenTests` and its two reds; `GridPath` for the distance; the
tray mark from units 300-303. Check; **report every mismatch; do not repair this
instruction; do not stop over a mismatch** unless a task is impossible.

- The tray: the quill mark from units 300-303 (~27 px ruled, `#3B6D11`), the count
  badge, what sizes the mark today; `Unit300SizesTests`, `Unit303OptionBTests` and their
  contradiction (unit 328 item).
- The conversation card's right column as unit 327 built it: the six facts, their
  bindings, the truncation, the two *show the messages* links.
- The quill popup as unit 327 built it: `nudge_opened`, the counter and door texts,
  its container - the same as the map popup from unit 310.
- The green zone under the neighborhood map: `… MHz · yours to use`, the license line,
  the citation; `DigitalModeChip`, the current sub-mode label; the *heard just now* dots
  and their count; the *best bet now* band pill.
- The readiness line after a transmit: *one slot decoded, and nothing on the band looked
  like FT8 at all*, and the other line that already exists - *21:56:15 UTC was yours -
  Hamlet was transmitting and did not listen* (seen 2026-09-11); the slot bookkeeping
  that knows a slot was the operator's.
- `VoiceTests`; `BindingHealthTests.TheMainWindowBindsWithoutOneComplaint`.

---

## 6. Rulings in force

**`PHASE_PLAN.md` §R1-§R19**, unchanged. **§R12** own tests; **§R14** nothing beyond the
criterion; **§R16** two forms of the mark; **§R19** American.

**Tim on the mark, 2026-09-10 and 11**: *"a tiny dot lost in the sea of the tray"*; a
subtle mark is invisible to him. **Units 300-303** ruled the tray mark ~27 px in decode
green; **what is on screen is not 27 px.**

**§0.0** - a line on the screen is a claim; *nothing on the band looked like FT8* about a
slot Hamlet did not listen to is a false one. **§0.5 / HM-DEC-012** - family color is
text only. **§0.6** - color never the only carrier. **`ACHIEVEMENTS_PHILOSOPHY.md` §3.1**
a door is never named; **§3.5** the teaching is the product - the popup is where the
teaching lives; **§4** worked, never confirmed, nothing shames.

**§0.2** - nothing here touches what keys. **HM-DEC-155**, **HM-DEC-139**, **FACT-004**,
**FACT-006**, **the dummy load withdrawn in full.**

## 7. Status cadence

Before every `dotnet` command, and after every task.

---

## 8. The tasks

Five. Each names the test to watch failing first and one drop candidate. **Drop from the
back.**

### Task 1 - the tray mark is the size that was ruled

Append `UNIT 330` to `PHASE_OUTCOME.md` under step 6 as carried repair; patch-bump; run
the carry-forward list.

The tray quill renders at the height units 300-303 ruled - **about 27 px, matching the
count badge's height**, green `#3B6D11`, not a sliver. Measure what it is today and
report before and after. **Resolve the `Unit300SizesTests` / `Unit303OptionBTests`
contradiction under §R12** by making both say what Tim ruled and what the screen shows.
Correct the `MainWindow.axaml` comment that still says *filled disc*.

**Test watched failing first:** the two size tests, reconciled, watched red then green;
`BindingHealthTests`.

**Drop candidate:** none.

### Task 2 - the card's right column is a table

Beside the map: **a two-column grid, labels in one column, values in the other, every
row the same height, nothing truncated, nothing wrapped.** Labels right-aligned in a
muted weight, values left-aligned in the card's text color. Rows:

```
Distance   3,000 miles · south
Grid       FI07
His time   09:21 by the sun
Messages   2 · show them
Achieves   New country
```

*Show the messages* appears **once**, here. The link on the card's top row is removed.
The `Achieves` row carries the mark's kind and opens the task 3 popup; **absent when he
earns nothing**, not blank. A fact Hamlet does not have is an absent row, not a dash.
Width: the column takes what the map leaves and never pushes the map narrower.

**Test watched failing first:** `TheCardsRightColumnTests`, rewritten under §R12: five
rows at equal height; no text clipped at the card's width; one *show* link on the whole
card; an absent fact is an absent row; `VoiceTests`, `BindingHealthTests`.

**Drop candidate:** the right-aligned labels. Keep the grid.

### Task 3 - the popup is worth opening

The quill popup becomes **a preview of the card he would earn**, not three gray lines.
Layout, top to bottom, in the same container as the map popup:

- **The card itself**, drawn as the achievements screen draws an earned card - the same
  shape, the same face - with **the entity's name on it** for a counter (`Norway`) and
  **no name** for a door (a card face with the ring and *a new area*); rendered as it
  will look when earned, not dimmed.
- **One line of teaching** (§3.5): for a counter, what it counts toward - *your 4th
  country in Europe · 40 to go* using the cited entity table's count for the continent;
  for a door, *the first contact in a new area opens every country in it* - naming none.
- **One line of fact**: distance and direction, and his local time, from the card's
  column.
- **The closing line**: *A QSO first. The card comes with it.*

Colors from the palette the app has; the entity name in the card's own text; no image
assets. Counts say **worked**, never confirmed. **Report a description of the rendered
popup in section 3, computed, not seen.**

**Test watched failing first:** `TheQuillPopupTests`, rewritten: the popup contains a
card face; a counter's face names the entity and the teaching line carries a count and
a remainder from the cited table; a door's face names nothing and the teaching line
names nothing; the fact line matches the card's column; *confirmed* appears nowhere;
`nudge_opened` fires with the kind only.

**Drop candidate:** the *to go* remainder. Keep the count.

### Task 4 - the green zone earns its space

The panel under the neighborhood map stops being three lines that never change. **One
line of license, and then what is true right now:**

```
14.074 MHz · yours to use · General covers digital here · 97.305(c)(3)(ix)
20 m · 14.074 MHz · Data · FT8 · best bet now: 20 m ✓ · heard just now: 31 stations
```

Line one is the existing three lines on one line, in the same words. Line two is **live**,
and **the band comes first** - Tim: *"I really didn't know 14.070 was 20 meters"* - so the
frequency and its band name are joined in one place: the band; the frequency; the family
and sub-mode under the tab (`Data · FT8`, `Data · PSK31`, `CW`, `Voice · SSB`); the *best
bet now* band the pills already compute, **with a check when it is the band he is on**,
and in the family color as a nudge when it is not - clicking it tunes there the way the
pill does; the count of stations heard in the last minute here, which the *heard just
now* dots already know. **A third line only when true**: the strayed-segment nudge -
*PSK31 lives at 14.070; you are at 14.074* - from the cited band rows. **Nothing invented;
every value already exists on the screen.** The panel is shorter by a line when nothing is
strayed. Family color as text for the mode word (§0.5).

**Test watched failing first:** `TheGreenZoneTests`, app: line one carries the frequency,
the license phrase and the citation; line two opens with the band name for the dial
frequency from the cited rows, then carries the sub-mode, the best-bet band with the check
when it matches and the nudge when it does not, and the heard count, each changing when
its source does; the strayed line appears only when the sub-mode's segment does not
contain the dial; the panel is shorter than before by a stated number of pixels;
`VoiceTests`, `BindingHealthTests`.

**Drop candidate:** the heard count. Keep band, mode and best bet.

### Task 5 - the slot that was yours says so

After the operator transmits, the readiness line for that slot **never** says *nothing on
the band looked like FT8*. It says what the other line already knows: *`14:37:30` UTC was
yours - Hamlet was transmitting and did not listen.* One line, one truth, and the
*one slot decoded* count does not count a slot that was not listened to. The same for
FT4, and for PSK31's unslotted send - *Hamlet was transmitting from 14:37:30 for 11 s and
did not listen.*

**Test watched failing first:** `TheSlotThatWasYoursTests`, app: after a send, the
readiness line names the slot as the operator's and does not claim the band was empty;
the decoded-slot count excludes it; FT4 and PSK31 forms; `VoiceTests`.

**Drop candidate:** the PSK31 form. Keep FT8 and FT4.

### Task 6 - the achievements page is eight badges with scores

**The opening page.** `assets/achievements-opening-mockup.png` is Tim's approved shape
(*"much better"*), drawn at mockup speed; **the session draws it properly as vector
paths in the app, no image assets.** Eight kinds, each a badge:

| kind | color band | emblem | what it counts |
| --- | --- | --- | --- |
| Hall of Fame | gold | a trophy | firsts that happen once |
| Continents | teal-blue | the Americas silhouette | a first in each of 7 |
| Countries | red | three flags on poles - **Hamlet's emblem, not real flags** | one card per DXCC entity |
| States | blue | a star | the 50 plus DC |
| Grids | green | a 3×3 grid with one square filled | 4-character squares |
| Total Miles | purple | a globe with an arc | grid to grid, added up |
| Bands | brown | three waves of different lengths | a first on each |
| Modes | slate | `·-`, two tones, `PSK` | FT8 · FT4 · PSK31 · CW · SSB |

Each badge: its name and one-line meaning on the color band with the emblem; below it
**the one card he could earn next in that kind** (§3.1 bent by ruling C, 2026-09-12: the
nearest unearned card in each kind is visible on night one; nothing beyond it), with the
orange ring for a door and the green quill for a counter; and in the corner **the kind's
score, its level, and the gap to the next level** - `Countries · 35 pts · Bronze · 3 to
Silver`. Text sized to fit, never clipped.

**The running total, on the page, always.** Under the title: **`Total 145 pts · Rank 3 ·
105 to Rank 4`**. Ranks are numbered; names are Tim's later.

**The points come from a data file, never from code.** `assets/data/achievement-points.json`
ships with this unit and goes to `data/achievements/achievement-points.json` in the tree.
It carries every value Tim set - per-card points, milestones, specials, the *all* bonus,
the Total Miles tiers, the ranks, the level thresholds - and an `_about` line saying it
is his to edit. **Hamlet reads it at startup; a missing or malformed file means scores
are shown as absent, not zero, with a line saying the file could not be read.**

**Inside a badge** - click it - the kind's cards as the screen draws them today, under
§3.1 as before: earned cards, then the nearest unearned, nothing else; a first contact
in a new area reveals that area's set. The Continents badge opens to seven continent
badges with their own emblems. **Every card shows its points.** `ACHIEVEMENTS_PHILOSOPHY.md`
§4 - counts say worked, never confirmed, nothing shames - and **a rank never goes down.**

**Telemetry (§R13):** `achievement_points_loaded` (count of kinds, file hash),
`achievement_score_changed` (kind, delta, total after) - no callsign, no entity name.

**Test watched failing first:** `TheAchievementsPageTests`, app. Watch it fail, then
green: eight badges render on an empty log, each with exactly one *next* card; the
scores, levels and total are computed from the shipped file and match a hand-computed
fixture log of twelve contacts (the unit writes the fixture and the expected numbers
into the test, showing the arithmetic); the total and next rank appear under the title;
a missing points file yields absent scores and the sentence; the two inherited reds in
`TheAchievementsScreenTests` are not made worse; `BindingHealthTests`, `VoiceTests`.

**Drop candidate:** the inside-a-badge view. Keep the opening page, the scores and the
total; report that clicking a badge opens the old screen.

### Task 7 - Total Miles

A new kind. **Every logged contact with a grid on both ends contributes its great-circle
distance from the operator's grid to the station's**, using the path Hamlet already
computes for the card. **A contact without a grid contributes nothing** - never an
estimate from the country. Tiers from the points file: 50K, 100K, 250K, 500K, 1M, 2M, 3M,
5M, 10M. The badge shows `0 mi so far` on an empty log and the running sum thereafter;
its *next* card is the lowest unearned tier. Earning a tier is an achievement like any
other - the tray mark, the card, the points.

**Test watched failing first:** `TheTotalMilesTests`, engine and app: a fixture log of
five contacts with grids sums to the hand-computed miles within 1%; a contact without a
grid adds zero; crossing 50,000 earns the tier once and never again; the badge's *next*
is the lowest unearned tier.

**Drop candidate:** the whole task. Report the badge as present with no cards behind it.

---

## 9. Parked

- **Rank names.** Tim's.
- **Real flags on earned country cards.** A separate decision; not here.
- **Step 6.** Tim's.
- **Any change to what keys. Any package.**

## 10. What not to do

- **No unfiltered `dotnet test`. Never background and poll. Status before every `dotnet`
  command.**
- **Do not invent a fact for the card, the popup or the green zone.** Absent, not dashed.
- **Do not name a door's area.** **Do not write *confirmed*.**
- **Do not make the tray mark subtle.** 27 px was ruled.
- **Do not add image assets.** Vector, from the palette. The mockup PNG is for reading,
  not for shipping; **nothing in `src/` references it.**
- **Do not hard-code a point value.** The file is the source.
- **Do not draw a real flag.** Hamlet's emblem only.
- **Do not touch anything that keys.** No package. No edit to the phase files beyond the
  append. Report mismatches; repair nothing. **Write American.**

## 11. Committing and pushing

Commit per task; `-m` more than once for multi-line; push at the end.

---

## 12. Reporting

`output.md` at the root. **Canonical headings:** `## 1. What Claude did`, `## 2. What the
owner should expect`, `## 3. What you should see`, `## 4. What's blocking us`.

```
READ IN THIS ORDER.

A. The phase goal - Hamlet works PSK31 the way it works FT8. Steps 0 to 4 done,
   5 partial on a nice-to-pass, 6 not started - unchanged by this unit.
B. No step criterion moves; carried repair.
C. The report last, and section 4 raises N items on top of the carried queue.
```

```
UNIT:       330 - <complete|stopped> at task N of 7, <which dropped> - <date time>
PHASE GOAL: <restated in your own words>
UNIT GOAL:  <restated in your own words>
ADVANCED:   no
NUMBER:     tray mark <before> -> <after> px; green zone height <before> -> <after> px;
            achievement kinds on the page 0 -> 8; total points on the fixture log <n>
DRIFT:      <n> consecutive units without advance
```

**Section 3 must describe each of the seven as it now renders, computed, in enough words
that Tim knows what to look for - and print the fixture log's scores kind by kind so he
can check the arithmetic against his file.** **Section 2 repeats the four files to delete by hand.**

---

```
ARBITER-DECISION
STEP: 6
APPROACH: size the tray mark as ruled, make the card's column a table, turn the quill popup into a preview of the card he would earn, compress the green zone to a license line plus a live band-first line with the best-bet check, make the after-transmit line say the slot was the operator's, and rebuild the achievements page as eight iconed badges with scores from a data file, a running total, and Total Miles
MOVE: continue
WHY: six things the owner saw and ruled on with the phase at his verdict, none touching a step criterion; he is away for two hours and asked for one unit that runs unattended, so every task commits on its own and drops from the back
STATE: not started
DECIDED: the green zone's second line, the popup's card preview, the badge emblems and colors, and the level thresholds are the author's shapes marked for the owner; the points are the owner's file; the size tests are reconciled under R12
LICENCE: PHASE_PLAN.md R12, R14, R16, R19; CLAUDE.md 0.0, 0.5, 0.6; ACHIEVEMENTS_PHILOSOPHY.md 3.1, 3.5, 4; units 300-303 on the tray mark
ACCOMPLISHED: the tray mark can be seen, the card reads as a table, the popup shows the card he would earn, the green zone tells him what band he is on and whether it is the best bet, Hamlet stops claiming the band was empty in a slot it did not listen to, and the achievements page is eight badges with scores and a running total he can edit from a file
ADVANCES: none - carried repair
END-ARBITER-DECISION
```
