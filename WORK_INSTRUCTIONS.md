# Work instruction 506 - achievements worth opening: overall progress, unlocking, and the moment

**Hand run. One unit, outside the CW phase.** The achievements window is correct and plain. This
unit makes it show overall progress, show what is locked and what opens it, and say so at the
moment something is earned. **What is earned, counted and scored does not change.** This unit
changes how it is drawn and adds one panel to the main window.

**The picture is in the tree**, three files the owner approved on 2026-09-30:

- `assets\achievements-look\opening-page.html`
- `assets\achievements-look\countries-page.html`
- `assets\achievements-look\unlock-moment.html`

**Read all three before task 1.** Layout, hierarchy and color are the spec. **Every number, date
and callsign in them is a sample**; the application's come from the log and from his points file.

Verify by building `Hamlet.sln` with warnings as errors and running the app carry-forward line
before the first change and after the last. **The owner's look at the window is the test.**

---

## 0. The project gate

```
STOP. Verify the project before reading any further.

PROJECT: Hamlet

Check the repository root:
  MUST EXIST:      SHACK_FACTS.md
  MUST EXIST:      src\Hamlet.RadioEngine\Cw\CwProbabilisticDecoder.cs
  MUST EXIST:      CW_REQUIREMENTS.md
  MUST NOT EXIST:  CoreHMI.sln
  MUST NOT EXIST:  MURC.sln
  root             C:\Source\HamLet

If all five are not as stated, refuse: reply with only the path you are in,
which checks failed, and "wrong project - nothing done."

If all five hold, say "Hamlet confirmed" and continue.
```

---

## 1. Rules

- Take `SESSION.lock` through `tools\arbiter\lock.bat take`, release it at the end. Write nothing
  to `RUN_LEDGER.md`. Touch nothing under `tools\arbiter\`.
- **This unit is not a step of the CW phase.** Touch no copy of `PHASE_PLAN.md`, `PHASE_OUTCOME.md`
  or `PHASE_STATUS.md`. Tick nothing. `PROJECT_STATUS.md` follows the cadence in the prompt and
  names 506; unit 505's wrapper `.run-unit\unit505-status.sh` shows how.
- One `dotnet test` invocation per line, filtered, with a `timeout`. Never background and poll.
  Never run `Hamlet.App.Tests` unfiltered. The app line loses names to the dispatcher loop; re-run
  once, count neither way.
- Apostrophes in quoted heredocs break; `;`, `rm` and `git rm` are refused; Python cannot run
  here; `-m` more than once for a multi-line commit. Scripts go in `.run-unit\unit506-<name>.sh`
  and are not committed.
- **A file that has to go cannot be deleted here.** Empty it to one comment saying which unit
  retired it and why, and list it once in section 2 for the owner to delete by hand.
- Nothing that keys or transmits. Nothing written to the radio. Nothing written to the log. **No
  package. No font file. No image asset.**
- **Under `src\Hamlet.RadioEngine\`: additive, read-only members on the achievements scoring types
  only**, where the page needs a number they already compute and do not expose. Nothing else there.
- The tree is dirty. **Commit only what this unit changed**, by path. The three picture files and
  this file arrived by zip and are committed with task 0.
- American spelling. `output.md` at the root, four headings exactly: `## 1. What Claude did`,
  `## 2. What the owner should expect`, `## 3. What you should see`, `## 4. What's blocking us`.

---

## 2. The owner's ruling

**His words, 2026-09-30, after unit 505:**

> *"I like the way maps are popping up when you ask for them, but overall, the achievements isn't
> very visually stunning. It doesn't attract me to go look. This is one of the ways we're going to
> help people to get use of the radio, is by wanting to get these achievements. We need to show
> overall progress. We need unlocking. We need just ways to make it visually attractive and
> stunning."*

**And on the picture:** *"Okay, write it up. The whole thing. It looks good."*

**Three things he asked for, and where each lands:**

- **Overall progress** - the standing panel: his rank in a ring, his points, how far to the next
  rank.
- **Unlocking** - a kind he has nothing in is drawn locked and says what opens it; the next rank
  is drawn locked and says where it opens; and the main window says so the moment a contact earns
  something.
- **Attractive** - the picture.

**What stands, and the picture was drawn inside it:**

- `ACHIEVEMENTS_PHILOSOPHY.md` §2, no wall of blanks. **Locked is one step ahead only**: one
  locked rank, never the ladder; a locked kind only where nothing in it is earned.
- §3.1, earned and then the one nearest unearned and nothing beyond it.
- §3.7, a count is `8 worked`, **never `8 of 340`**. A bar runs to the next level of his file, and
  says the level's count, never the size of the world.
- R19, the door: amber, a ring, and the words. **Color is never the only carrier** (§0.6).
- HM-DEC-209, unit 505: a category is a list and the map opens on a click. The row press and the
  popup are not rebuilt.
- The points file is his. **No number on this page is written in code**; ranks, rank names and
  levels are read from it, and where it cannot be read the page says so and shows no scores.

---

## 3. Verify this instruction against the tree

Read from a harvest of `57b9759` and from unit 505's report; the tree after 505 was not harvested.
**Check each; report every mismatch in section 1; do not repair this instruction; do not stop over
a mismatch** unless a task is impossible.

- `src\Hamlet.App\Views\AchievementsWindow.axaml`, 1040 by 720, opened modally from
  `MainWindowViewModel.OpenAchievements` off the Tools menu item `Achievements…`.
- The opening page is the `StackPanel` named `AchievementsPage`: `AchievementsPageTitle`,
  `AchievementsPageSubtitle`, `AchievementsTotalLine` bound to `Page.TotalLine`,
  `AchievementsPointsProblem`, the `ItemsControl` `AchievementsBadges` on `HmBadgeTemplate`, and
  `AchievementsLegend`.
- `AchievementBadge` carries `Kind`, `Name`, `Meaning`, `Emblem`, `Band` (the kind's color),
  `NextCard`, `NextIsDoor`, `Standing`, `Score`, `ScoreLine`, `Card`.
- `AchievementBadgePage` carries `Badges`, `Scores`, `TotalLine`, `Problem`. `Scores` exposes
  `Total`, `RankName`, `ToNextRank`, `NextRankName`.
- The kinds' colors are `#A8811A` Hall of Fame, `#2A7A94` Continents, `#A33333` Countries,
  `#2C4C9B` States, `#2F6B3A` Grids, `#6B4C9A` Total Miles, `#8A5A1E` Bands, `#3E4650` Modes.
- The points file, `data\achievements\achievement-points.json`: `ranks` is eight running totals
  starting 25, 100, 250, 500; `rank_names` is empty; `levels` gives each kind three or four counts.
- `AchievementsViewModel` carries `Calling`, a `CqSnapshot`, and `BestBet`.
- From unit 505: the item template `HmCategoryItemTemplate`, the `category-row` button on
  `OpenTheMapCommand`, `AchievementCategory.DrawnCards`, `OpenedMapHeading`, the test class
  `TheCategoryPagesAreListsTests`, and the category band with `AchievementsLevelBar`.
- The newest decision is `HM-DEC-209`. The version is 1.13.192.

---

## 4. The tasks

### Task 0 - before anything changes

Commit this file and the three pictures. Run the app carry-forward line. Then, filtered, one line
each: `TheCategoryPagesAreListsTests`, `TheAchievementsPageClicksInTests`,
`TheAchievementsPageTests`, `TheAchievementsScreenTests`, `TheMapOpensTests`. **Report each green
or red by name. A red here is inherited and is not chased.**

### Task 1 - the standing panel

The left third of the opening page, as `opening-page.html` draws it: a dark panel, `#14202B`, with

- **the ring**: the fraction of the way from where his rank began to where the next begins, a
  thick arc, light green `#9AD05F` on `#2B3B49`. Inside it the rank and his points. **Where his
  file names the rank, the name is drawn; where it names none, `Rank` and the number**, as
  `TotalLine` does today;
- **the gap in words** under it: `88 points to Rank 4.` At the top rank, say so and draw the ring
  full;
- **three facts**: contacts, countries, farthest in miles;
- **just unlocked**: the most recent thing he earned, by the date of the contact that earned it -
  its seal, its name, the station and distance, its points. **It is a button and opens that path
  in the popup.** With nothing earned the panel says what the first contact earns, and no seal.

The ring is a control drawn in code or a shape the framework already has. **No package.**

The window opens at **1280 by 860**, the author's default from the picture, overrulable.

**Where the points file cannot be read:** the panel says `Problem` in words, draws no ring and no
number, and the rest of the page still draws.

**Test watched failing first**, a new `TheAchievementsStandingTests`: the ring's fraction is the
file's arithmetic on three logs - empty, the twelve-contact fixture, and one a point short of a
rank; the rank name follows the file; an unreadable file draws no number; the just-unlocked button
opens the popup with that contact's path.

**Drop candidate:** the three facts.

### Task 2 - the rank trail and the eight tiles

**The trail**, one line across the top of the right side: each rank he has passed, checked; the
one he holds, marked `you are here`; **the next one, locked, a padlock and where it opens, in the form `opens at 500 points`**;
and nothing after it. A solid line joins what is done, a dashed line leads to the locked one.
**More than four passed ranks: draw the last three and say how many came before in words.**

**The tiles**, four across and two down, replacing `HmBadgeTemplate`'s badge. Each is still the
button that opens its kind. An opened kind carries:

- a header in the kind's color with its emblem, its name, and **its level in words** - `Bronze`,
  `Silver`, `Gold`, `Platinum` - or no chip below the first level;
- the count, large, and what it counts: `8` `countries worked`;
- **a bar to the next level** and the gap in words: `2 more to Silver`. At the top level the bar
  is full and says so;
- **the next line**: `Next:` and `NextCard`. Where `NextIsDoor` is true the box has the amber ring
  and says `Opens a set:`.

**A kind with nothing earned is drawn locked**: a dashed border, the muted ground, a padlock and
the word `Locked`, and `To open it:` with what the first one takes. **It is still a button** and
opens the kind's page, which shows its next panel.

White on `#A8811A` does not reach 4.5 to 1. **The Hall of Fame header uses `#8A6A10`**; the kind's
color elsewhere is unchanged.

**The legend line `AchievementsLegend` goes** if the tiles say in words everything it explained;
say which in the report.

**Test watched failing first**, in `TheAchievementsPageTests` or a new class: eight tiles, each
opening its kind; a level chip that follows the file's `levels`; the bar's fraction on the fixture;
a locked tile for every kind with nothing earned and for no other; the trail draws exactly one
locked rank; **no string `of 340` or any denominator of the world appears**; no string clips at
1280, 1400 and 1920 wide.

**Drop candidate:** none.

### Task 3 - within reach right now

The dark strip along the bottom of the opening page: up to three stations from `Calling` that
would earn him something, each with its callsign, its place, and what it earns in words. A door
has the amber ring.

- **It says where the list came from and when**, if the snapshot carries its time. If it does not,
  say only what the snapshot can say, and report it.
- **No callers: the strip says so in a line** and does not draw empty boxes.
- **It sends nothing and tunes nothing.** It is a list to read.

**Test watched failing first:** three callers in a snapshot draw three; one that opens a continent
is ringed and says so; an empty snapshot draws the one line.

**Drop candidate:** the whole task.

### Task 4 - the category page, dressed

`countries-page.html`. Unit 505's list, row press and popup stand. What changes:

- **The header band**: the emblem large, the name, the level chip, the standing line, and the
  level bar with its gap in words at the right, where `AchievementsLevelBar` is today.
- **Each row gets its seal at the left**: a round double-ruled ring in the kind's color, turned a
  few degrees, holding the short code of what was earned - a country's prefix, a state's two
  letters, a grid's four characters, a band's number, a mode's name where it fits. **Which code
  each kind shows, and where it comes from, is the author's**, stated in the report.
- **The next panel moves to a column at the right**, dashed and muted with a padlock and
  `Next stamp`, its callers listed under it, the door ringed with its words.
- **`Your reach` under it**: farthest, and newest. Both from the cards already built.
- The rows, newest first. **The order is the view's**; `Cards` is untouched, as in 505.

Continents and Total Miles take the same header and seals. **A continent's row still opens its
countries, and its `map` button still opens the path.**

**Test watched failing first:** in `TheCategoryPagesAreListsTests`, every earned row carries a
seal with a code that is not empty; the next panel is beside the list and not in it; farthest is
the greatest distance among the cards; every 505 test there still passes, and any whose substance
changed is named in the report with why.

**Drop candidate:** `Your reach`.

### Task 5 - the moment something unlocks

`unlock-moment.html`. **When a contact is written to the log and it earns something he did not
have**, the main window shows a panel: `Unlocked`, the seal, the name, the station and place and
distance and band and mode in one sentence, the points added, and the rank bar with the part just
gained in the lighter green.

- **Find the one place a contact is written to the log** and name it in the report. Compare what
  is earned before the write and after. **If every mode does not pass through one place, say so,
  wire the ones that do, and list the ones that do not.**
- **It never takes the keyboard and never covers the conversation card's controls.** It is a panel
  over the main window, not a dialog. It closes on its own button, on a click outside it, and
  **the moment a transmission starts**.
- **Two buttons**: `See where <place> is`, which opens the achievements window with that path in
  the popup, and `Keep going`.
- **More than one thing earned by one contact: one panel**, the largest by points named, and the
  others in a line under it.
- **Crossing a rank says the new rank** in the panel.
- A points file that cannot be read: no panel.
- **It writes nothing** to the log, the radio or the settings.

**Test watched failing first**, a new `TheUnlockMomentTests`: a logged contact that earns a new
country shows the panel with that country and its points; one that earns nothing shows nothing;
one that crosses a rank says the rank; a transmission starting closes it; the panel holds no
keyboard focus.

**Drop candidate:** the whole task. **If the log has no single write point, drop it, say so, and
leave nothing half-wired.**

### Task 6 - three widths, the old tests, the comments

- Stand the achievements window up at 1280, 1400 and 1920 wide and **describe, computed**, the
  opening page, Countries, Continents and Total Miles at each: what is drawn, what clips if
  anything.
- Every existing test that pinned the old badge or the old opening page is carried in substance
  or retired. **Name each with which happened and why.**
- The comments in `AchievementsWindow.axaml` that describe the old opening page are rewritten to
  cite this unit and HM-DEC-210.

**Drop candidate:** the three-width description.

---

## 5. Record

- `DECISIONS.md`, newest first, **HM-DEC-210**, headline *The achievements window shows overall
  progress, what is locked, and the moment it opens*, quoting him from section 2, and saying whose
  words are whose: the ruling and the approval of the picture are Tim's; the picture, the window
  size, the seal's code, `#8A6A10`, and how the unlock panel closes are the author's under work
  instruction 506, and overrulable.
- The index row for it in `CLAUDE.md` §1, in the shape of the rows above it. **Do not fill
  189 to 208.**
- Patch-bump `Directory.Build.props`.
- **No phase file.** See section 1.

---

## 6. What not to do

- **Do not change what is earned, counted or scored**, or any number in his points file.
- **Do not draw the ladder.** One locked rank. One locked thing per kind.
- **Do not write a denominator of the world.**
- **Do not draw a map at rest** anywhere in the achievements window. Unit 505's zero stands.
- **Do not ship a font or an image.** The picture's serif display face is not in the application;
  use the application's own faces at the picture's sizes and weights, and say so.
- **Do not let the unlock panel take focus, block a control, or outlive the start of a
  transmission.**
- **Do not use color alone** for locked, next, door or level. Each has its word.
- **Do not clip or wrap a word.**
- **Report mismatches; repair nothing else on the way past** (`CLAUDE.md` §12.6).

---

## 7. Committing and pushing

Commit per task on `main`, by path, each with its red quoted. Push at the end and say whether it
succeeded.

---

## 8. Report

`output.md` at the root, above the four headings:

```
READ IN THIS ORDER.

A. Whether the opening page shows the ring, the trail, eight tiles and the strip, and which kinds
   draw locked on the fixture.
B. Whether a logged contact shows the unlock panel, and where the log's write point is.
C. The rest. Section 4 raises <n> items, none blocking.
```

```
UNIT:       506 - <complete|stopped> at task N of 6, <dropped or none dropped> - <date time>
UNIT GOAL:  <in your own words>
NUMBER:     tiles drawn: <n> of 8, locked: <n>; unlock panel on a new country: <shown|not built>
```

Section 2, for the owner, in plain words:

- rebuild;
- Tools, Achievements: the ring and your rank at the left, the trail with the next rank locked,
  eight tiles, who is within reach along the bottom;
- a kind you have nothing in is locked and says what opens it;
- click a tile, then a row: the map, as before;
- log a contact that earns something: the panel comes up in the main window;
- where it differs from the picture, and why;
- any file emptied for him to delete by hand.

Section 1: what changed, file by file; every mismatch against section 3; the old tests and what
became of each; the log's write point; that the build and the app line are green. **Section 3: the
three widths, computed.** Section 4: anything left, a line each.
