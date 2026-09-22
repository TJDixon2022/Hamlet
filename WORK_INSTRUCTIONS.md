# Work instruction 387 - the four things Tim saw on his own screen, and the first of them is a thing Hamlet used to have

**Step 10 of the hardening phase, and it has never had a unit.** R46 was ruled by Tim on
**2026-09-22** - this morning - and the plan gained the step at `6d4caef5` a few hours ago.
Seven tasks. **This is the first unit in nine to write application code under `src` since
unit 385**, and it is the last step in this phase a unit can move.

**The phase is called *Hamlet holds what it has*, and item (a) is the phase goal in one
sentence.** Tim had favorites. The star on the rig display, the saved dial and mode, the list
he could get back to a clear CW spot from. **They are gone from his screen and the machinery
is still in the tree** - `FavoritesViewModel`, `SavedFavorite`, `TuneToFavoriteCommand`,
`PersistFavorites` and a settings round-trip test that loads a favorite out of a file. So the
first question of the night is not *how do we build favorites*; it is **what took them off the
screen, which unit did it, and on which line** - and the criterion asks for that answer by
name before it asks for anything to be restored.

**The step you were handed was step 0. This unit is step 10, and section 4 says why in
numbers.** In one line: 0.1 is a completed negative that two arbiters and one judging session
have now been over, its remedy is a change to the plan's own wording and therefore Tim's, and
work instruction 386 section 6 ruling 1 closed it to units of this phase. **Nothing in step 0
is left for a unit to reach. Step 10 has five criteria and no unit has ever opened it.**

**Status.** `sh tools/status.sh`, real clock, after every commit and every task. **The `sh` is
part of the command - read section 2 before you type it.**

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

*All four were checked against the tree at authoring time and all four hold: `SHACK_FACTS.md`
and `src\Hamlet.RadioEngine\Cw\CwProbabilisticDecoder.cs` are present, neither `CoreHMI.sln`
nor `MURC.sln` exists at the root, and the root is `C:\Source\HamLet`.*

---

## 1. The rules that killed sessions

**HM-DEC-155.** No suite; only `docs\carry-forward-tests.txt` as its own top comment says -
**two invocations, one build each**, status written immediately before each. Never background
and poll. **Unit 323 ran one `dotnet test` per name - forty-one builds - wrote one status
line, and the watchdog killed it at twelve minutes of silence.** It had done nothing wrong
except obey the shape of the file.

**Write a status line immediately before every test invocation**, naming the task and what is
running. Silence is what kills a unit.

**The app invocation is the flaky one and you must expect it.** The headless
`InvalidProgramException: You've caused dispatcher loop` has now been counted across eleven
units: **46 full app invocations over units 375 to 385, of which 17 were lost to it and 2 were
red on an assertion** (unit 386 task 1), and four more lost in unit 386's own ten. It kills at
about **1 ms, before any assertion**, moves between names, and **has never once touched the
engine invocation**. **It is not a red; it is a lost invocation. Re-run it once, record both
attempts, and do not chase it.** The discriminator is measured and it is in the tree: *about
1 ms and before any assertion* is the session fault; **anything that ran and disagreed is a
red and is yours.**

**A loop goes in a script file** (`;` is refused in a compound command). `Bash(sh:*)` is
granted, so `sh .run-unit/unit387-<name>.sh` works. **Write the script with the editor.**

---

## 2. The tool facts

- **The status line is `sh tools/status.sh ...`, with the `sh`.** The granted rule is
  `Bash(sh tools/status.sh:*)` and **a permission rule is a literal prefix match over the
  whole command string**. `tools/status.sh ...` and `./tools/status.sh ...` are both refused;
  unit 384 lost three calls to exactly that.
- **`git status` with no `-C`.** The granted rule is `Bash(git status:*)`. You are already
  standing in the root. `git -C /c/Source/HamLet status ...` is refused.
- **`git log`, `git log -S`, `git log -L`, `git show`, `git diff` and `git rev-parse` are
  granted, and tonight they are an instrument and not a convenience** - criterion 10.1 is
  answered with them. Use them; do not name a commit from memory or from a code comment alone.
- **Shell output redirection (`>`) is refused to every path**, including paths under
  `.run-unit/`. Write files with the editor. **A test run's output is read from the console**,
  or piped to `grep` in the same command.
- **A compound command with `;`, `&&` or a second operation is refused**, and so is
  `cd X && Y`.
- **Apostrophes in quoted heredocs break; doubled backslashes collapse; `rm` is refused; `-m`
  more than once for a multi-line commit.** Append to a file with the editor.
- **Write this repository's files as UTF-8.** A PowerShell `>` redirect writes UTF-16 and the
  launcher cannot read it. **`PHASE_OUTCOME.md` is BOM+CRLF - edit it with the editor and do
  not normalise it.**
- **`validate-output.bat` is at `tools/arbiter/validate-output.bat`**, and the shape is
  forward slashes, a leading `./`, one command, no `cd` in front and no `cmd /c` around it.
  **Run it before you say the report is written.** If it answers *This command requires
  approval*, that is the permission mode and not the syntax: hand-check the rules against the
  script's own header, say in section 4 that what you did was a hand-check and not a run, and
  move on. Seven units running have hit this.

---

## 3. Asks still outstanding

Carried per HM-DEC-139, **verbatim in section 4**.

**The queue stands at fifty-three and you answer none of them** - the forty-seven unit 386
carried plus **unit 386's own six**. Unit 386's report is committed and readable at the root
(`output.md` at `283c6fa6`); its section 4 is the list. **Read it; do not reconstruct the
queue from memory.**

Four of them touch tonight and each gets one line in your section 4:

- **Unit 386's item 3 - a stale expected test count beat an instruction, and the measurement
  won.** Instruction 386 said a green app invocation was 226 of 226; the measured total was
  **245**. Section 5 below says 245 and 150. **Count what you get and say so. If your number
  disagrees with section 5, your number wins and it is a section 4 item, not a reason to
  stop.**
- **Unit 386's items 5 and 6 - the launcher fault twice over, and 0.1's cut-down.** Both are
  logged to the owner. **Neither is yours. Do not re-record either.**
- **The `RULES_AT` id split, now for the ninth unit running.** `PROJECT_STATUS.md` reads
  `HM-DEC-165 (2026-09-19)`; `CLAUDE.md` section 1 holds `CPS-DEC-0165`. The reload names it
  first among the disagreements. `tools\status.sh` writes that field as a literal and **you
  may not edit `tools\`**. Report it. Do not repair it.
- **Unit 383's inherited reds**: `TheOliviaMoveUpTests.ItIsNotOfferedOnAGuessedYourTurn`,
  `ViewTestsActThroughControlsTests.NoViewTestWritesAPropertyAControlOwns` and
  `TheOliviaSendTests.StopMidPlayAbortsAnOliviaSend`. **None is on the carry-forward list**, so
  none can fail a round. Tonight's work is near the second of them - a view test that writes a
  property a control owns is exactly the trap in task 3. **Do not go hunting them. If your own
  work makes one green, say so and prove it; if your own work makes one red, that one is
  yours.**

---

## 4. Why this unit exists

```
PHASE GOAL: Hamlet holds what it has - the hardening phase, screen, record and test
            work banked in the PSK31 and Olivia threads that needs neither the radio
            nor the owner, judged by tests that ran and, at the end, by Tim at his
            window.
UNIT GOAL:  Give Tim back the four things he lost or never had on his own screen on
            2026-09-22 - favorites, with the unit and the line that took them named
            before anything is rebuilt; a right-click menu on every decoded row and
            not only on the rows that named a station; the sun map filling the band
            it sits in; and no invented characters from a carrier the blind search
            found but the decoder cannot read.
ADVANCES:   step 10, criteria 10.1, 10.2, 10.3 and 10.5, with 10.4 the named drop
            candidate.
DRIFT:      none.
```

**The count today**, from `PHASE_OUTCOME.md`, `PHASE_PLAN.md` at `6d4caef5` and the reload of
2026-09-22 10:10.

| Step | State | Units spent | Where it stands |
|---|---|---|---|
| 0 | `partial` | 369, 371's carried repair | 0.2 to 0.5 certified met at `PHASE_OUTCOME.md:41`. **0.1 cut down by instruction 386, logged to the owner, closed to units** |
| 1 | **`done`** | 372, 373, 374 | judging session `done` |
| 2 | **`done`** | 375, 377, 384 (one task), 386 | **closed last night** - judging session `done` at `PHASE_OUTCOME.md:525`, all five of 2.1 to 2.4 and 2.6 ticked |
| 3 | **`done`** | 380, 381 | judging session `done` |
| 4 | `partial` | 382, 383 | 4.1, 4.2 met. 4.3 **cut down** by instruction 384, logged to the owner |
| 5 | `not started` | 0 | Tim's own, and it ends the run |
| 6 | **`done`** | 376 | closed by R42 at the measured floor. **10.3 supersedes its 6.3** |
| 7 | `partial` | 378, 383 | 7.1, 7.3, 7.4, 7.5 met. 7.2 met on unit 383's own account, **unjudged** |
| 8 | **`done`** | 379 | judging session `done` |
| 9 | ungraded | 385 | all five criteria's work **landed at `e5e4bee0`**; no judging session has read it; 9.1 to 9.5 unticked |
| 10 | `not started` | **0** | **R46, ruled this morning. Five criteria. This unit** |

**Step 10's entry, checked:** *step 9 done.* Section 6 ruling 1 rules it open and says why.

### Why step 10, when the step handed to this arbiter was step 0

**Because step 0 holds nothing a unit can reach, and I checked that against the record rather
than inheriting it.** Criterion 0.1 asks for *the commit and line between 1.13.30 and 1.13.48
that dropped the transmit device on load*. Three things have been tried:

| # | What was tried | What it hit |
|---|---|---|
| A | Unit 369 searched the 119 commits of `681d45c8..ec4b466e` over the four settings paths | **zero** commits; a completed negative, and the line that did drop it named and measured as predating the window |
| B | Work instruction 372 section 6 ruled 0.1 met on that negative | the judging session returned `partial`: the report *"names none"* |
| C | Work instruction 386 section 6 re-measured it wider - **any** path in the repository whose name contains `settings` | **zero** again; cut down, 0.1 never ticked by a unit of this phase, remedy logged to the owner |

**A fourth attempt is either A again or B again, and `ARBITER.md` section 4 says the step ends
there.** It does. **I am not re-opening it and no task tonight searches that window.**

**One thing I will log rather than chase**, because the next reader should not have to find it
twice: every search so far has been over *settings* paths, and a saved device name can also be
lost at the point where it is **resolved against the devices the OS enumerates** - a rename, an
index-for-name change, a default fallback - in files that carry no `settings` in their names.
**Nobody has searched that shape.** It is not tonight's work: it would spend a night on a
report-only criterion whose remedy is a wording change that belongs to Tim, while step 10's
five criteria sit at zero units. **It is written here for him to decide, and that is all.**

Everything else open is answered, the owner's, or waiting on a grading pass that runs *after* a
unit rather than before it: 4.3 cut down and logged; 5.1 Tim at his window; 7.2 and all of step
9 need a judging session, not a unit. **Step 10 is the only place a unit can move this phase
tonight, and it happens to be the step the owner ruled this morning.**

### What this unit is worth, in the owner's terms

**He had favorites and now he does not.** On a phase called *Hamlet holds what it has*, that is
the sharpest thing on the list, and 10.1 is deliberately written with the archaeology first:
*the report names the unit and the line*. **Hamlet is not allowed to quietly lose a feature and
then quietly grow it back** - the record says who took it and when, and then it comes back.

And three more things off his own screen: a right-click that does nothing on the rows the
parser could not read a callsign out of - **which are exactly the rows he most needs a Capture
on**; a sun map sitting at two thirds of the height of the band it lives in, since a criterion
was written *keeps its size* and read as *stays 134 px*; and a row on 14.072 at 13:37 UTC
reading `4/500 sending Hk7DYYYzfzYXTDYY...`, **which is Hamlet telling him a station sent
characters that no station sent**.

---

## 5. Verify this instruction against the tree

Every line below was read at authoring time, at the file and line named. **Report every
mismatch in section 4 and in section 1 of your report; repair nothing but this unit's.** Line
numbers move under an edit - if one is off by a few, the name is the thing that matters; say so
and go on. **Where your measurement disagrees with mine, yours wins and it is a finding.**

### The record and the versions

`Directory.Build.props` line 1196 reads `<Version>1.13.73</Version>`. `PHASE_STATUS.md` names
this phase and reads `CURRENT_STEP: 0` and `WORK_INSTRUCTION: 386`, both stale. `HEAD` is
`6d4caef5`. `PHASE_PLAN.md` at `6d4caef5` leaves exactly **fifteen** criteria unticked: 0.1,
4.3, 5.1, 7.2, 9.1 to 9.5 and **10.1 to 10.5**; 32 are ticked.

### What a round should come back as

Unit 386's exit round, both invocations green on the first attempt: **app 245 of 245, engine
150 of 150**. The app line (`docs/carry-forward-tests.txt` line 7) carried **59** filter terms
at authoring time and the engine line (line 9) **25**; both begin `timeout 480 dotnet test`.
**They are used unedited.** 245 and 150 are the numbers your entry round must match or beat,
and your exit round must match or beat your entry round plus this unit's own new guards.

### For 10.1 - favorites, and the machinery that is still there

**The feature is not deleted. Something took it off the screen.** What is in the tree:

| Where | What is there |
|---|---|
| `src/Hamlet.App/Settings/AppSettings.cs:70` | `public List<SavedFavorite> Favorites { get; set; } = new();` |
| `AppSettings.cs:1075` | `SavedFavorite`, with a note at 1097 that a field is *defaulted rather than required, so every favorite saved before notes* still loads |
| `tests/.../TheSettingsSurviveAnUpgradeTests.cs:88` and `:539` | a settings fixture carrying a `"Favorites"` array, and `Assert.Single(loaded.Favorites)` - **the loader keeps them today** |
| `src/Hamlet.App/ViewModels/MainWindowViewModel.cs:~9790-9840` | the save path, `AppEvents.FavoriteSaved`, `TuneToFavorite` with `AppEvents.FavoriteTuned`, `ManageFavorites` opening `Views.FavoritesWindow`, and `PersistFavorites` |
| `src/Hamlet.App/Views/MainWindow.axaml:2739` and `:2774` | a **Radio menu** `Favorites` submenu gated on `HasFavorites`, and `Manage favorites…` |
| `MainWindow.axaml:3066-3068` | the rig display's three bindings: `IsFavorite`, `FavoriteLabel`, `ToggleFavoriteCommand` |
| `src/Hamlet.App/Controls/RigDisplayControl.cs:327` | `DrawStar`, *the star, inside the black, where it is the most findable thing here (HM-DEC-070)* |
| `RigDisplayControl.cs:~338` | **the bail**: `if (room < glyph.WidthIncludingTrailingWhitespace) { _starRect = default; return; }` - the star is dropped **and its hit rectangle zeroed** when the strip runs short |
| `MainWindow.axaml:3277-3292` | the favorites strip's own comment: *which favorite you landed on at the left, and the way to any of them at the right* - then **THE RECENT-PLACES ROW IS NOT HERE ANY MORE** (Tim's ruling of 2026-08-27) and **IT IS NOT DELETED**, pointing at `ABANDONED_WIDGETS.md` |

**Two candidate answers and you measure which is true**, rather than believing either:

1. **The strip was removed** on 2026-08-27 with the recent-places row, taking the *way in* with
   it, and the star has been alone on the rig display since.
2. **Unit 376 shortened the rig display** for step 6 (commit `1c187df6`, *the band comes down*)
   and the bail at `RigDisplayControl.cs:~338` now fires at Tim's widths, so the star is not
   drawn **and cannot be clicked**, silently, with no sentence anywhere.

**If it is (2) it is a regression this phase's own step 6 caused, and that is a finding worth
the night on its own.** Measure `_starRect` and the drawn glyph at **1920, 1400 and 1100×780**
before you change anything.

### For 10.2 - the menu that is not offered

| Where | What is there |
|---|---|
| `src/Hamlet.App/Views/MainWindow.axaml:~4955` | the row `Grid` with `ContextRequested="OnDecodedRowContextRequested"` |
| `src/Hamlet.App/Views/MainWindow.axaml.cs:448` | the handler |
| `MainWindow.axaml.cs:466` | `vm.OpenPsk31CardCommand.Execute(row)` - **the right-click already makes a card** (R29), before the menu is built |
| `MainWindow.axaml.cs:470-477` | *"**HANDLED EITHER WAY.** A row with no station has no menu"* - the flyout is `null` and nothing opens |
| `MainWindow.axaml.cs:503` | `SendFlyoutFor`, whose summary reads *"The flyout, or null where the row names no station"* |
| `MainWindow.axaml.cs:518` | *"**NOTHING IS GREYED, HIDDEN, SORTED AWAY OR DISABLED** (ruled 2026-09-06). An entry with no command is a note ... and a note cannot be hit"* |
| `MainWindow.axaml.cs:~586` | `Log this contact...`, added only where `vm.CanLogRow(row)` |
| `MainWindowViewModel.cs:3684` | `Psk31CaptureIdle = "Capture 2 minutes"` - **Capture exists as a panel press, not as a row item** |

**So today: a row the parser read a station out of gets a menu; a row it did not gets nothing
at all.** That is precisely R46(b)'s complaint, and section 6 ruling 2(a) says what to do about
the 2026-09-06 rule it collides with. **Measure the ratio on a fixture first** - how many rows
of how many offer a menu - the way unit 378 measured 2 of 7 before it built anything.

### For 10.3 - the sun map and the band

| Where | What is there |
|---|---|
| `MainWindow.axaml:996` | `<ctl:GrayLineMapControl x:Name="GreenZoneGrayLine" Height="134" ...>` - **hard-coded** |
| `MainWindow.axaml:1002` | `GreenZoneClockCaption`, `where the sun is · you` |
| `MainWindow.axaml:985-990` | the comment: *the height is the unit's choice, marked as its own: 134 px, the mockup's* |
| `tests/.../Unit376TheTopBandTests.cs:542,545` | `SunMapWidth = 246`, `SunMapHeight = 134` |
| `Unit376TheTopBandTests.cs:423` | `TheSunMapIsTheSizeItWasAndStillCarriesHisGrid` - asserts 246 × 134 within 0.5 px and calls the map *not a source of pixels (6.3)* |
| the carry-forward list, line 7 | carries **`Unit376TheTopBandTests.TheTopBandIsOneShortRowAndThePanelsAreTallerByTheDifference` only** - not the whole type |

**Read that last row twice.** The name that asserts 134 px is **not** on the carry-forward
list; the name that asserts the band's height **is**. So 10.3's work cannot be done by
loosening a carry-forward guard, and it must not raise the band: **6.1's ceiling of 220 px at
1920 and at 1400 is untouched and is the constraint.** R42 measured the band at **214** with a
floor of **197**. Section 6 ruling 2(b) rules the 134 px assertion superseded and rewritten,
not loosened.

### For 10.4 - the carrier that showed characters nobody sent

| Where | What is there |
|---|---|
| `MainWindowViewModel.cs:2571` | `internal const string HeardNotReadableYet = "heard, not readable yet";` - **the sentence already exists** |
| `src/Hamlet.App/ViewModels/DigitalDecodeRow.cs:1007` | *a carrier with a shut squelch carries* **heard, not readable yet** |
| `src/Hamlet.RadioEngine/Psk31/Psk31CarrierSearch.cs:222` | §0.0 and **R9**: *the row stays, dimmed, saying* heard, not readable yet |
| `src/Hamlet.RadioEngine/Psk31/Psk31Listener.cs:106` and `:317` | the same state, named, on the PSK31 side |
| the carry-forward list, line 9 | `TheOliviaBlindSearchTests` and `TheOliviaDemodulatorTests.NoiseGivesNoCharacters` |

**The sentence, the rule and the state all exist; what R46(d) says is missing is the gate
between the Olivia block decoder's own confidence and the characters a blind-found row
shows.** `NoiseGivesNoCharacters` is the guard nearest it and it is on the list, so **whatever
you do here must leave that name green**. Tim's row was `4/500 sending Hk7DYYYzfzYXTDYY...` on
14.072 at 13:37 UTC.

### What will be red before you start

`TheOliviaMoveUpTests.ItIsNotOfferedOnAGuessedYourTurn`,
`ViewTestsActThroughControlsTests.NoViewTestWritesAPropertyAControlOwns` and
`TheOliviaSendTests.StopMidPlayAbortsAnOliviaSend` - **none on the carry-forward list**, so
none can enter a round. Plus the inherited known-red block at `docs/carry-forward-tests.txt`
line 152, which is likewise never on the list.

---

## 6. Rulings in force

**Transcribed in full. Do not re-argue any of them.** Two are mine, author's and overrulable
under R31, which allows two a unit. The rest are the plan's and the project's.

### The first is mine: step 10's entry is open, and step 9 is not touched to open it

**Author's and overrulable.**

Step 10's entry reads *step 9 done*, and step 9 is **ungraded** - unit 385's report has never
been read by a judging session, so `PHASE_PLAN.md` leaves 9.1 to 9.5 unticked and the outcome
header still says `not started`. **I rule the entry open**, on evidence rather than on
impatience:

1. **The work landed.** Unit 385 committed 9.2's gate, display and hold, 9.3's Log, and 9.4's
   and 9.5's guards across `6d192b98`, `3659f03a`, `d86d4ccd`, `91fe9b9f`, `b69319f8`,
   `45ba1ea9` and `ad3b70ab`, finishing at `e5e4bee0`.
2. **Its four new guards have since been proved.** `TheCarrierHoldsTheButtonsTests`,
   `TheCardOffersLogAndAnXTests`, `TheTurnMovesOnEveryHandBackTests` and
   `TheFourAreOnOliviaCardsTooTests` joined the app command line and then ran in **twelve
   invocations across unit 386's five counted rounds over a tree frozen at `a91d6ed6`, with not
   one red on an assertion in the whole night.**
3. **The grading pass runs after a unit, never before one.** A phase that waits for it before
   opening the next step waits forever on nights when no unit runs.

**What follows from this ruling, and it is the whole of what follows:** step 10 is worked
tonight. **9.1 to 9.5 stay unticked, are not re-proved, are not re-measured and are not
ticked by this unit**, exactly as work instruction 384 left step 7 and instruction 386 left
step 9. You do not read unit 385's code to judge it. If a judging session later returns
`partial` on it, nothing tonight becomes a lie, because **nothing tonight rests on step 9's
criteria being met** - only on its code being in the tree and green, which the frozen-tree
soak already proved.

### The second is mine: where R46 supersedes a rule already in the tree, and where it does not

**Author's and overrulable.** `PHASE_PLAN.md` section 6: **the later ruling wins.** R46 is
Tim's, dated **2026-09-22**, and it is later than both rules below. **But a supersession is
narrow: it wins exactly where it speaks and nowhere else.**

**(a) 10.2 against the 2026-09-06 *nothing is greyed* rule.** The tree says, at
`MainWindow.axaml.cs:518`, *NOTHING IS GREYED, HIDDEN, SORTED AWAY OR DISABLED (ruled
2026-09-06)*, and the mechanism it gave instead is the `Note` - an entry with no command that
cannot be hit. R46(b) says *lines that need his callsign are disabled and say why*. **R46(b)
wins, for those lines only:**

1. **A line that cannot be sent because Hamlet does not know his callsign is disabled and
   carries a word saying so.** Grey is this project's reserved signal for a control that
   genuinely cannot be used (§0.5.1, HM-DEC-087), and this is that case.
2. **Every other line keeps the 2026-09-06 rule and the `Note` mechanism.** A complete contact
   still offers `73`; a repeat is not sorted away; nothing that can be sent today becomes
   unsendable tonight. **If your change makes any sendable line unsendable, you have gone too
   far and it is a section 4 item.**
3. **The menu opens on every decoded row** - callsign read or not, live or ended, PSK31,
   Olivia, FT8 or FT4 - **and `Capture` and *make a card anyway* are on it always**, which
   means on the rows that name nobody too. `OpenPsk31CardCommand` already fires on the
   right-click at `MainWindow.axaml.cs:466`; **an item that says what that press did is not a
   second mechanism, and you do not build one.**

**(b) 10.3 against 6.3.** R46(c) names its own supersession in terms: *the sun map takes the
top band's full height - unit 376 kept it at 246 x 134 on the author's wording of 6.3, which is
superseded.* **So `TheSunMapIsTheSizeItWasAndStillCarriesHisGrid` is rewritten under R12 to
assert R46(c)'s rule instead of 6.3's number, and that is not loosening a test** - the thing it
asserted has been replaced by the owner, which is the one case where rewriting an assertion is
honest. **What is not superseded: 6.1's 220 px ceiling, the dot, and the caption.** The band
may not grow by one pixel to make room, the operator's grid marker stays, and
`GreenZoneClockCaption` stays. **If the map cannot fill the band without the band growing past
220, stop at what fits, report the two numbers, and say 10.3 is partial** - `PHASE_PLAN.md`
section 6: a must-pass missed by a little ships, reports, `partial`, moves on.

### The plan's own, which tonight is built on

**R46 - Tim, 2026-09-22, four things on his screen.** *(a) **Favorites vanished.** Saved
frequencies - the star on the rig display - so a clear, productive CW spot can be returned to.
Tim had them; a unit since removed them or their list, and the record must say which before
they are rebuilt; the saved frequencies may still be in his settings file. (b) A right-click on
any decoded row opens the menu, whether or not the parser read a callsign; lines that need his
callsign are disabled and say why; Capture and* make a card anyway *are always on it. (c) The
sun map takes the top band's full height - unit 376 kept it at 246 x 134 on the author's
wording of 6.3, which is superseded. (d) A carrier the blind search found shows text only when
the mode's block decoding is confident (R9); the row* `4/500 sending Hk7DYYYzfzYXTDYY...` *on
14.072 at 13:37 UTC was the fault.*

**R9**, which R46(d) cites: a row Hamlet cannot read says so rather than showing what it
guessed.

**R11** nothing at the radio. **R12** a session rewrites its own tests. **R13** telemetry on
every stage. **R14** nothing beyond the criterion. **R19** American. **R31** unattended -
criteria by id, a done step closed, the owner's step ends the run, **two rulings a unit**.
**HM-DEC-018 §2.1** nothing personal in an event. **§0.0** a sentence on the screen is a claim;
a refusal says the true reason.

**`PHASE_PLAN.md` section 6, the branching rules that bear on tonight:** *Three stops only:
keying, transmit or the radio's safety; money past the budget; a fact the product states to the
operator about the radio, a contact or a send. A hint, a label, a number, a layout, a test's
shape, a mechanism arithmetic will not allow: **decide, mark author's, continue**. The later
ruling wins. A done step is closed. A must-pass missed by a little: ship, report, `partial`,
move on. **Never loosen a test.***

**And the one that will tempt you to halt, settled here so you do not:** 10.4 changes what
Hamlet says about a station, which is the third stop's shape. **It is not a stop, because the
owner has already ruled it** - R46(d) says exactly what the row must show and when. Build it.
**What would be a stop is going further than R46(d) says** - suppressing something Hamlet
states today that R46(d) does not cover. If you find yourself there, that is `MOVE: stop` in
your section 4 and you leave 10.4 where it stands.

---

## 7. Status cadence

`sh tools/status.sh`, real clock, **after every commit, after every task, and immediately
before every test invocation**. Name the task and what is running. **The watchdog kills at
twelve minutes of silence and the app invocation alone runs about 2 m 35 s.**

---

## 8. The tasks

**Seven tasks. The trace is task 1 and it builds nothing under `src`. The named drop candidate
is task 5 - criterion 10.4 - and its measurement, item 4 of the trace, goes with it.**

**The order is deliberate: the two criteria that answer *what did we lose and when* come
first, because the phase is called `Hamlet holds what it has`.**

### Task 0 - the record and the entry run

Version **1.13.73 -> 1.13.74** with its line in the version log. `PHASE_STATUS.md` to **step
10 and unit 387**.

**Transcribe, do not judge:** the judging session's verdict on unit 386 at
`PHASE_OUTCOME.md:525-526` returned `STATE_AFTER: done` on step 2 - **another session's word,
not yours and not a criticism of unit 386** - so `PHASE_STATUS.md`'s `STEP: 2` line may carry
the word `done`. **`PHASE_PLAN.md` ticks nothing at this task.** Step 9's line is left exactly
where unit 385 and 386 left it, with one sentence carrying section 6 ruling 1: the work landed
at `e5e4bee0`, it is ungraded, 9.1 to 9.5 stay unticked, and step 10's entry is ruled open on
the landed and proved code.

Append the `## UNIT 387 - STEP 10` entry to `PHASE_OUTCOME.md` in the house shape, carrying
this instruction's `STEP`, `ADVANCED`, `APPROACH`, `MOVE`, `WHY`, `DECIDED`, `LICENCE`,
`COST`, `ACCOMPLISHED` and an `ENTRY:` line with the entry round's real numbers.

**Then the entry round:** both command lines, unedited, one build each, status before each.
**Expect app 245 of 245 and engine 150 of 150.** A lost app attempt is re-run once. **If a name
goes red on an assertion, it is inherited, it is not yours, and it is named in the report and
worked around - it does not stop the night.**

**Commit.**

### Task 1 - the trace: measure all four before you build one

**No file under `src` changes at this task.** Four items. **Every number in your report's
section 3 that describes *before* comes from here.**

1. **Favorites, and this is 10.1's own criterion.** Three questions, answered with `git` and
   with a headless measurement, not from a comment:
   - **Does the star draw, and does it answer a click, at 1920, 1400 and 1100×780?** Realize
     the window as `Unit376TheTopBandTests`/`TheTopRowTests` do, read
     `RigDisplayControl`'s drawn star and its `_starRect`, and say at which widths the bail at
     `RigDisplayControl.cs:~338` fires. **A zeroed `_starRect` means the star cannot be
     clicked even if something is drawn.**
   - **Which unit and which line took favorites off the screen?** Use `git log -S` and
     `git log -L` over `MainWindow.axaml`'s favorites strip region and over
     `RigDisplayControl.cs`'s `DrawStar`, and over `ABANDONED_WIDGETS.md`. **Name the commit,
     the unit, the date and the line, and quote the line.** If the honest answer is *two
     things happened and here they both are*, say that - **10.1 asks for the truth, not for a
     single culprit.**
   - **Do the saved frequencies survive in the settings file?** Answer it from the loader and
     from `TheSettingsSurviveAnUpgradeTests`' own fixture - **round-trip a reconstructed file
     carrying favorites through today's loader and say what came back.** **Do not open, copy or
     quote Tim's own settings file into the record**: HM-DEC-018 §2.1, nothing personal. A
     count and a yes or no is the answer; his callsign, his grid and his frequencies are not.
2. **The row menu.** On a fixture with a mix of rows - station read, no station read, live,
   ended, PSK31 and Olivia - count **how many of how many offer a menu today** and what each
   menu holds. Name where `SendFlyoutFor` returns null. **Say whether the right-click's card
   (`MainWindow.axaml.cs:466`) fires on a row that names nobody**, because if it already does,
   *make a card anyway* is a label on a thing that exists rather than a new mechanism.
3. **The sun map and the band.** At **1920 and 1400**: the band's measured height, the map's
   measured width and height, the caption's height, and **how many pixels of the band the map
   is not using**. That difference is 10.3's whole target and it should be stated before a
   pixel moves.
4. **The blind-found carrier - THE DROP CANDIDATE'S MEASUREMENT.** From the engine's own Olivia
   fixtures: **does the block decoder expose a per-block confidence at all, and does anything
   between it and the row consult it?** Name the type and the member, or say plainly that no
   such number crosses the seam today. **If item 4 says the confidence does not exist and would
   have to be invented, say so and drop task 5 - that is the drop and it is a legitimate
   outcome, not a failure.**

**Commit** (the trace's record only - no source file moves).

### Task 2 - 10.1, favorites back

**The archaeology from task 1 goes in the report and in the commit message.** Then:

- **The star saves the dial and the mode with a name** - the command exists; make it reachable
  and drawn at every one of unit 354's nine sizes, or say which sizes it cannot be drawn at and
  why the bail is right there.
- **The list opens from the rig display.** Today it opens from the Radio menu only
  (`MainWindow.axaml:2739`, `:2774`). **R46(a) says the rig display.** The menu stays - two
  ways in is not a defect - but the rig display gets one.
- **One click tunes.** `TuneToFavoriteCommand` exists at `MainWindowViewModel.cs:~9805` with
  its own telemetry; wire it, do not rewrite it (R14).
- **It persists across an upgrade under step 0's loader.** `TheSettingsSurviveAnUpgradeTests`
  is on the carry-forward list and already round-trips a favorite. **Grow it rather than
  writing a second one**, under R12, so a favorite saved tonight and a favorite saved before
  this unit both load.
- **The events.** `FavoriteSaved` and `FavoriteTuned` already exist and carry a band name.
  **Nothing personal goes in a new one** (HM-DEC-018 §2.1), and `CallsignPrivacyTests` is on
  the list - if you add a writer, grow its walk **in the same commit**.

**This unit's new guards go on `docs/carry-forward-tests.txt` in the same commit as the code
they guard**, as every unit since 380 has done.

**Commit.**

### Task 3 - 10.3, the sun map fills its band

The map's height stops being a constant and becomes the band's. **The band does not grow** -
6.1's 220 px at 1920 and at 1400, from R42's measured 214 with a floor of 197. The dot and
`GreenZoneClockCaption` are kept. **`TheSunMapIsTheSizeItWasAndStillCarriesHisGrid` is
rewritten under R12 and section 6 ruling 2(b)** to assert R46(c)'s rule; it asserts the new
rule at both widths and it keeps every assertion about the grid marker that it has today.

**`Unit376TheTopBandTests.TheTopBandIsOneShortRowAndThePanelsAreTallerByTheDifference` is on
the carry-forward list and must be green when you finish.** Measure the nine sizes after the
change and report them against unit 381's `483, 71, 92, 163, 171, 267, 483, 460, 860`.

**Watch the trap in section 3's fourth ask**: a view test that writes a property a control owns
is already red once in this tree. **Act through the control, measure the bounds.**

**Commit.**

### Task 4 - 10.2, a menu on every row

Under section 6 ruling 2(a). Every decoded row opens a menu on a right-click; the lines needing
his callsign are disabled and **say why in a word**; `Capture` and *make a card anyway* are on
every one of them. **The view model decides and the view draws** - that is already this file's
rule at `MainWindow.axaml.cs:~516`, so the new lines come out of the view model's own answer
and a test can read the same list Tim sees.

**Nothing here arms a transmission.** `Capture` is a capture and *make a card anyway* opens a
card; every send on this panel still goes through one click on a named button.

**Commit.**

### Task 5 - 10.4, no characters from blocks the decoder cannot read - THE DROP CANDIDATE

Under R46(d) and R9. A blind-found Olivia or PSK31 carrier shows characters **only** from
blocks the decoder reports confident; otherwise the row reads *heard, not readable yet*, which
is a sentence that already exists at `MainWindowViewModel.cs:2571`. **Replay the 13:37 UTC
shape as a fixture and assert the `4/500` row shows no text.**

`TheOliviaBlindSearchTests` and `TheOliviaDemodulatorTests.NoiseGivesNoCharacters` are on the
engine command line and **must both be green when you finish**.

**This is the named drop candidate.** If the night is short, or if task 1 item 4 found that no
confidence number crosses the seam, **drop this task, say in one line that you dropped it and
why, and spend what is left on the exit round and the report.** 10.4 then stays unticked and
step 10 is `partial`. **That is a permitted outcome. A half-built confidence gate that shows
text sometimes is not.**

### Task 6 - the exit run, the record and the report

The exit round of both invocations, one build each, status before each, its numbers against
task 0's **name for name**. Then `PHASE_PLAN.md`: **tick only the criteria tonight actually
earned**, each with tonight's own numbers written beside it in the plan's own text. **Tick
nothing of step 9, nothing of step 0, nothing of step 4 and nothing of step 7.** Then
`PHASE_STATUS.md`'s `STEP: 10` line rewritten to what tonight measured, then `output.md`, then
`validate-output.bat`, then push.

**Leave the state word for step 10 at `partial` unless every one of 10.1 to 10.5 is met** -
and say plainly that closing a step is a judging session's and not a unit's.

---

## 9. Parked - do not touch, do not raise

- **Criterion 0.1, `src/Hamlet.App/Settings/` as an archaeology subject, and the
  1.13.30-to-1.13.48 commit window.** Work instruction 386 section 6 ruling 1. **You may and
  will touch the settings *loader path* for 10.1's persistence** - that is a different thing
  from searching that window, and section 4's note about the device-resolution shape is logged
  to the owner, not work.
- **`docs/RADIO_SHEET.md`, `TheRadioSheetQuotesTheScreenTests` and every operator-facing string
  the sheet quotes.** 4.3 is cut down and logged. **If a string you change tonight is quoted by
  that sheet, that is a section 4 item and the answer is: do not change the string.**
- **7.2.** It needs a judging session, not a unit.
- **Step 9's code and criteria.** Section 6 ruling 1.
- **The dispatcher loop's cause.** Nobody's criterion. Record every occurrence; chase none.
- **`tools\`, including the `RULES_AT` split.**
- **The two false outcome rows** - the Olivia phase's `## UNIT 2 - STEP 1` and this phase's
  `## UNIT 7 - STEP 2`. Both corrected by appending; **both stay exactly as they are.**

---

## 10. What not to do

- **Do not touch a send path, a modulator, an `Arm` site, a `PttOn` site or anything that
  keys.** Nothing in step 10 needs one. `PHASE_PLAN.md` section 6: anything that would change
  what goes on the air is `MOVE: stop`.
- **Do not loosen a carry-forward test to make tonight's work fit.** The one assertion being
  rewritten is named in section 6 ruling 2(b), it is not on the list, and it is rewritten
  because the owner replaced what it asserted - **not because it was in the way.**
- **Do not let the band grow past 220 px** to fit the map (6.1, R42).
- **Do not make a sendable line unsendable** with 10.2's disabling (ruling 2(a) item 2).
- **Do not invent a single character on a row** to meet 10.4, and do not meet it by hiding a
  row. The row stays; what it says changes.
- **Do not put anything personal in an event** - no callsign, no grid, no word of decoded text,
  no frequency from Tim's own file (HM-DEC-018 §2.1). `CallsignPrivacyTests` is on the list and
  its walk grows **in the same commit** as any new writer.
- **Do not name a commit for 10.1 from a code comment.** A comment is a claim; `git log -S` is
  evidence. Quote what you ran.
- **Do not open, copy or paste Tim's own settings file into the record.**
- **Do not run a suite.** HM-DEC-155: two invocations, one build each.
- **Do not tick a criterion in `PHASE_PLAN.md` that tonight did not earn**, and do not tick
  0.1, 4.3, 7.2 or any of 9.1 to 9.5 at all.
- **Do not call a red environmental because you would like it to be.** About 1 ms and before
  any assertion is the session fault; anything that ran and disagreed is a red.

---

## 11. Committing and pushing

**One commit per task**, with these exceptions: **a carry-forward line edit goes in the same
commit as the name it is about**, a privacy-walk growth goes in the same commit as the writer
it covers, and task 2 may take a second commit if the archaeology and the restoration are
cleaner apart - **the archaeology commit first, because the record comes before the rebuild.**
**Push once, at the end.**

---

## 12. Reporting

`output.md` at the root. **Canonical headings:** `## 1. What Claude did`, `## 2. What the owner
should expect`, `## 3. What you should see`, `## 4. What's blocking us`.

**The ordering block first. `validate-output.bat` refuses a report without it.** Fill every
line from what you measured - a line that is the same every unit is furniture.

```
READ IN THIS ORDER.

A. The phase goal - Hamlet holds what it has. Steps 1, 2, 3, 6 and 8 done, step 2
   closed last night by a judging session; step 0 partial with 0.1 cut down and
   closed to units; step 4 partial with 4.3 logged to the owner; step 7 partial on
   an unjudged 7.2; step 9's work landed at e5e4bee0 and ungraded; step 5 Tim's own
   and it ends the run. Step 10 is the only step a unit can move and this is the
   first unit ever spent on it.
B. Step 10 - what Tim saw on 2026-09-22, R46. 10.1 <met|not>: the unit and line
   that took favorites off the screen are <name them, with the commit>, the saved
   frequencies <do|do not> survive the loader, and favorites are back by <what>.
   10.2 <met|not>: <n> of <n> fixture rows open a menu where <n> of <n> did.
   10.3 <met|not>: the sun map <w> x <h> where it was 246 x 134, in a band of
   <h> px against 6.1's ceiling of 220. 10.4 <met|not|dropped, and why>.
   10.5 <met|not>: app <n of n>, engine <n of n>.
C. The report last. Section 4 raises <N> items on top of the carried fifty-three,
   and <none of them is | item <k> is> in the way of a criterion in B. Say in one
   line whether what took favorites away was this phase's own step 6 or an earlier
   ruling of Tim's - that is the night's finding and 10.1 asks for it by name.
```

```
UNIT:       387 - <complete|stopped> at task N of 7 - <date time>
PHASE GOAL: <restated in your own words>
UNIT GOAL:  <restated in your own words>
ADVANCED:   step 10, criteria <the ids you actually moved>
NUMBER:     decoded rows that open a right-click menu: <n> of <n> -> <n> of <n>;
            and the sun map's share of its band: <n>% -> <n>%
DRIFT:      none
```

**Section 3 must lead with 10.1's archaeology, in a table**: the commit, its short hash, the
unit, the date, the file and line, and **the line quoted**. Under it, the `git` command you ran
and what it returned. **That table is the criterion** - a sentence saying *a unit removed them*
is not. Then the star's measured state at 1920, 1400 and 1100×780, before and after. Then the
menu counts before and after. Then the map and band numbers at both widths against 220. Then
10.4 or the one line saying it was dropped and why. Then the entry and exit rounds, name for
name.

**Section 2 tells Tim in one paragraph what is different, in his terms**: not that a criterion
was met, but that **the stars he saved frequencies with are back on the radio face and the list
opens from there, and the record says exactly which change took them away and when** - and that
a right-click now gives him something on every line he can see, including the ones Hamlet could
not read a callsign out of, which are the lines he most wants a capture from. Every claim
computed, not seen (FACT-004), and one line saying no port was opened, nothing was enumerated
and nothing was keyed.

---

```
ARBITER-DECISION
STEP: 10
APPROACH: name the commit and line that dropped the favorites star and put favorites back on the rig display, open the right-click menu on every decoded row, give the sun map the top band's full height, and show no text from a blind-found carrier until its blocks decode
MOVE: continue
WHY: step 10 is R46, ruled by the owner this morning off his own screen, and it is the only step left in this phase whose criteria a unit can reach - 0.1 is a completed negative closed to units, 4.3 and 5.1 are the owner's, and 7.2 and step 9 need a grading pass rather than a night; and 10.1 is the phase goal's own sentence, because Hamlet did not hold what it had.
STATE: not started
DECIDED: author's and overrulable, two rulings. (1) Step 10's entry - which reads step 9 done - is OPEN, on evidence and not impatience: unit 385's five criteria's work landed across seven commits ending at e5e4bee0, and its four new guards then ran in twelve carry-forward invocations over a tree frozen at a91d6ed6 with not one red on an assertion, while the judging session that would grade it runs after a unit and never before one; 9.1 to 9.5 nevertheless stay unticked, are not re-proved and are not ticked by this unit. (2) Where R46 supersedes a rule already in the tree it wins narrowly and only where it speaks: 10.2's disabling covers the lines that need his callsign and nothing else, so the 2026-09-06 nothing-is-greyed rule and its Note mechanism stand for every other line and no sendable line becomes unsendable; and 10.3 supersedes 6.3's 134 px by R46(c)'s own words, so TheSunMapIsTheSizeItWasAndStillCarriesHisGrid is rewritten under R12 to assert the new rule - which is not loosening a test, because the owner replaced what it asserted - while 6.1's 220 px ceiling, the dot and the caption are untouched. Also decided and not a ruling: step 0 is left closed exactly as work instruction 386 ruled it, with one note logged to the owner rather than worked - every search of that window has been over settings paths, and a saved device can also be lost where its name is resolved against the devices the OS enumerates, which nobody has searched.
LICENCE: PHASE_PLAN.md R46, which is step 10 entire, and R9 which R46(d) cites; PHASE_PLAN.md section 6 - the later ruling wins, a layout number and a test's shape are the arbiter's, a must-pass missed by a little ships partial, never loosen a test; R12 a session rewrites its own tests; R14 nothing beyond the criterion; R31 two rulings a unit; ARBITER.md section 4, whose reading returned NOT FOUND on tonight's approach line, and whose loop test I judge to be FAILED for criterion 0.1 - approach A searched the window, approach B ruled the negative sufficient and was rejected by a judging session, approach C re-measured it wider and cut it down, and a fourth is A or B again; ARBITER.md section 3's cut it down, already applied to 0.1 by instruction 386 and not reopened; ARBITER.md section 6, which makes the choice of step the arbiter's and keeps a change to the plan's wording with the owner; HM-DEC-018 section 2.1, HM-DEC-087, HM-DEC-139, HM-DEC-155, HM-DEC-165; CLAUDE.md 0.0, 0.2, 0.5.1; FACT-004
ACCOMPLISHED: Tim gets back the stars he saved frequencies with - on the radio face where he had them, opening his list from there, one click to tune, surviving the next upgrade - and the record says which change took them away and when, before a line of it was rebuilt. A right-click gives him something on every decoded line, including the ones Hamlet could not read a callsign out of, which are the lines he most wants a capture from. The sun map fills the band it sits in instead of two thirds of it. And a carrier Hamlet found but cannot read says so, instead of showing him characters no station sent.
ADVANCES: step 10 criterion 1, step 10 criterion 2, step 10 criterion 3 and step 10 criterion 5, with step 10 criterion 4 the named drop candidate.
END-ARBITER-DECISION
```
