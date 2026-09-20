# Work instruction 368 - the Olivia contact goes in the log, and the mode's records appear

**Step 5 of `PHASE_PLAN.md`, authored by the arbiter.** Step 4 is **done, 8 of 8**: Hamlet's own
modulator at all three variants, every send announced by its RSID, the CQ on the cited calling spot,
a reply at the row's own variant and center, the cap and the patience scaled by the variant, Stop,
the power offer and the ALC, and unit 367's one click that takes the QSO up 500 Hz and widens it.
**Step 5 has had no unit.** It stands at **0 of 4**.

**This unit is step 5's first.** Hamlet hears Olivia, reads it, answers it and moves it; what it
cannot do is **log it**. Today an Olivia contact reaches the log as `MODE=PSK31`, because the one
question *what mode was this contact made in* answers PSK31 for any station with a card and Olivia
rows use PSK31's cards. **Seven tasks are not wanted here; six are: 0 to 5. Task 5 (5.4, the only
nice-to-pass in the step) is the drop candidate.**

**Status.** `tools/status.sh`, real clock, after every commit and every task, and immediately
before each `dotnet test` invocation.

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

The arbiter checked all four against the tree on 2026-09-19 after unit 367's report commit, and they
held: both files are present, neither solution exists, and the root is `C:\Source\HamLet`.

---

## 1. The rules that killed sessions

**HM-DEC-155.** No suite. Run only this unit's names, plus `docs\carry-forward-tests.txt` the
way its top comment says: **two invocations, one build each**, with a status write immediately
before each. Never background and poll.

**The margins.** At the end of unit 367 the app invocation took **2 m 18 s for 189 tests** and the
engine invocation **4 m 45 s for 146**, against a 480 s timeout - about 195 s of margin on the
engine. **Nothing this unit adds goes on the engine carry-forward line.** One app name may join it
(decision CG). Report the seconds of both invocations every time they run.

**HM-DEC-165 - a mode that works stays working.** The list carries a send guard and a read guard for
every working mode; Olivia has three rows - read, modulate and send. **A red after that was green
before is a regression.** Name it in sections 1 and 4, and **the repair is this unit's next task
before any new criterion**. This unit edits code that **every mode's log record and every badge on
the achievements screen goes through**, so an FT8, FT4, CW, Voice or PSK31 record that changes shape
is a regression even where no test names it: say what you changed and what it did to the other five.

**CPU is measured alone.** Every class this unit adds that asserts or reports CPU goes in the
non-parallel `CpuMeasuredAlone` collection.

## 2. The tool facts, as units 359 to 367 measured them

- Apostrophes in quoted heredocs break, and doubled backslashes collapse. **Units 366 and 367 wrote
  every commit message into a file under `.run-unit\` with the editor and passed it with
  `git commit -F`**, which worked. Do the same.
- A `sed` substitution with backslashes in the pattern matched nothing and reported nothing. Use the
  editor. A `sed` insertion between an XML doc comment and its member fails the build (`CS1572`),
  because warnings are errors.
- **`>` redirection is refused anywhere under the root**, including into `.run-unit\`, as is
  `sed -i` on a source file and `echo` into a file. Use the file editor.
- **Python did not run for units 365, 366 or 367** (`python -c` and `python file.py` needed
  approval), though it ran for 361 and 362. **Do not build a task on it.**
- `rm` is refused. A `for` loop over a variable was refused as *simple_expansion*. Do not depend
  on `;`.
- A `grep` pattern containing `\|` was read as several operations and refused; **`grep -E` with the
  same alternation ran.** `grep -o` with a quantifier and `grep -v` in a pipe need approval.
- **`tools\arbiter\validate-output.bat` has not run for three units.** If it will not run, check the
  report's shape by hand against the rules the script prints - the ordering block with A, B and C
  and C's item count; the `UNIT:` line above section 1; the four `##` sections in order with their
  exact names and no fifth; section 3 non-empty; `###` nested under them and nowhere else - and say
  in section 4 that you did.
- These needed approval, which a headless session cannot give: `mkdir`, `cp`, `mv`, `tee`,
  `powershell.exe`, `jq`, `awk`, `git restore --source`, `git checkout <rev> -- <file>`,
  `git stash push`, `git config`, and command substitution. A command that includes one is refused
  whole. **Keep `Passed!|Failed!` in every filter** so the summary line survives.
- `sh tools/status.sh` alone, or joined by `&&` to `git` and `dotnet test`, ran. `sed -n N,Mp` and
  `tail -n +N | md5sum` in a pipe after `git show` ran.
- Anything outside `C:\Source\HamLet` cannot be listed or read. **And nothing reaches the network**
  (`TheTestsStayOffTheNetworkTests`), which is why decision CC exists.
- Status words: `STATE: EXECUTING`, `BALL: code` (`CLAUDE.md` §13.1). `tools/status.sh` writes
  `RULES_AT: HM-DEC-165 (2026-09-19)` and reads `WORK_INSTRUCTION` from `PHASE_STATUS.md`, which
  says 367 until you set it.

## 3. Asks still outstanding

Carried per HM-DEC-139. **The last report is `output.md` at `HEAD` (`c395e6b8`), unit 367's, 1827
lines.** Its `## 4. What's blocking us` heading is at **line 615**. **Carry everything from line 616
to the end verbatim** - its five items and the queue it carries from unit 366, which carries 365's,
the FT8 unit's, 364's, 363's and the older ones. Recover the bytes whichever way the harness allows;
units 366 and 367 kept `output.md` from `HEAD` in place and replaced only the lines above the queue
with the editor, and that path works and needs no refused command.

**Check the carried copy.** `git show c395e6b8:output.md | tail -n +616 | md5sum` gives
`8064c626e456793ae6426e797839ef16`. The same `md5sum` over your carried block must give the same
hash. **Report both.**

**Mark nothing in place** except what section 9 says this instruction answers.

---

## 4. Why this unit exists

**This is unit 368, the first unit of step 5 and the eleventh of the phase.** Step 4 closed at 8 of
8 on unit 367 and **a done step is closed** (§6). Step 5 stands at **0 of 4** with three must-passes
- 5.1, 5.2, 5.3 - and one nice-to-pass, 5.4.

The phase goal is one sentence with four verbs: Hamlet **hears** Olivia, **reads** it, **answers**
it, **logs** it. Three of the four are built and proved. **The fourth is this step**, and without it
the evening a beginner spends on Olivia leaves nothing behind: no record, no export another logger
or an award program will take, and no sign on the achievements screen that the mode was ever worked.

**And the defect is not that logging is missing - it is that it is wrong.**
`MainWindowViewModel.Psk31ContactWith` answers `ContactModes.Named("PSK31")` for any station that
has a card, and since unit 364 Olivia's rows and cards **are** PSK31's cards. So an Olivia contact
logged today says `MODE=PSK31`, which is a false statement about a contact in a file the operator
keeps forever (§0.0). That is what 5.1 is for, and it is why 5.1 is the first task after the trace.

```
PHASE GOAL: Hamlet works Olivia the way it works PSK31 - hears it, reads it, answers it,
            logs it - with the variant taken from the signal's own announcement and never
            picked by the operator.
UNIT GOAL:  An Olivia contact reaches the log as an Olivia contact. The mode is read off
            the conversation and not off the tab, the RST and the grid come the way PSK31's
            do, the export says MODE=OLIVIA with the variant as SUBMODE through one object
            that cannot set one without the other, and the achievements screen - which
            named no Olivia card before the first contact - counts the mode and reveals its
            records after it, with the two inherited reds in TheAchievementsScreenTests no
            worse than they were.
ADVANCES:   step 5 criteria 5.1 (tasks 2 and 3, must-pass), 5.2 (task 4, must-pass) and
            5.3 (task 4, must-pass). 5.4 (task 5, nice-to-pass) is the drop candidate and
            is expected not met on this machine - see decision CC.
DRIFT:      0 - unit 367 met 4.5 and 4.8 and closed step 4.
```

**Read `PHASE_PLAN.md` at the root in full.** Most of all: step 5 and its entry, R27, R28, R30,
R31, §3.2 and §6.

---

## 5. Verify this instruction against the tree

**Nothing here describes the tree.** Check every claim against the files and report any
mismatch. **Report it; do not repair the instruction.** Mismatches go in the report even where
the work succeeded.

What this instruction believes, from the reload of 2026-09-19 22:29 and the arbiter's own reading
after it:

- **HEAD is `c395e6b8`**, unit 367's report commit. Version **1.13.55** in `Directory.Build.props`
  (line 899).
- **Where a contact becomes a record.** `MainWindowViewModel.ContactLogEntryForStation` at about
  **:15224** calls `Ft8ContactLogEntry.For` at **:15243** with an `Ft8StationConditions` built at
  **:15246** - the dial, the band, **the mode**, and the operator's grid. The mode argument is
  `Psk31ContactWith(who) ?? _digitalMode.Contact()` (about **:15276**).
- **The defect this unit repairs.** `Psk31ContactWith` (**:15340**) is one line:
  `_psk31Cards.ContainsKey(station) ? ContactModes.Named("PSK31") : null`. **It cannot tell an
  Olivia conversation from a PSK31 one**, though the card can: `Ft8ContactCard.OliviaVariant`
  (`Ft8ContactCard.cs:319`) and `IsOlivia` (**:322**), set from the channel's variant.
- **The RST and the grid already come from the conversation.** `Psk31ReportsFor` (**:15354**) and
  `Psk31GridFor` (about **:15316**) both read `_psk31Cards` and `_psk31Readings`, which Olivia rows
  fill, so **5.1's *with RST and grid* may already hold for Olivia** - measure it in the trace and
  say so rather than rebuilding it.
- **The mode table.** `ContactModes` (`src\Hamlet.RadioEngine\Contacts\ContactModes.cs`):
  `Cite` at **:133** is *ADIF Specification 3.1.4, released 2022-12-06, https://www.adif.org/314/
  ADIF_314.htm, retrieved 2026-09-08*; `Six` at **:138**; **`Logged` at :172**, which is `Six` plus
  `new ContactMode("Olivia", new[] { "OLIVIA" }, null, true)` - **`OLIVIA` as the ADIF mode and a
  null submode**; `Named` at **:179** searches `Logged`. Its own remarks say step 5 is where the
  submode and the counting arrive, and that the spelling was taken from `PHASE_PLAN.md` and **not
  re-read from the ADIF page**.
- **The pair cannot come apart today, and must not after.** `Ft8ContactLogEntry.For` sets
  `Mode = adifMode` and `Submode = adifMode is null ? null : mode!.AdifSubmode` - both projections
  of one `ContactMode`. `AdifLog` writes `MODE` at **:349** and `SUBMODE` at **:356**, reads them
  back at **:561-:562**, and `IsRstMode` at **:593** asks `ContactModes.Named("PSK31")` whether the
  pair is PSK31, which is how a record gets `RST_SENT` rather than a decibel report.
- **The achievements count `Six`, not `Logged`.** `AchievementLog`'s contact mapping (about
  **:314**) is `Mode: ContactModes.Six.FirstOrDefault(m => m.Matches(contact.Mode,
  contact.Submode))`, so **a record saying `MODE=OLIVIA` maps to a null mode and is invisible to
  every mode-counting badge.** `AchievementScores.WorkableModes` (**:311**) is
  `ContactModes.Six.Count(m => m.IsContactMode)` and is **5**.
- **The firsts.** `AchievementScores.FirstsEarned` (**:200**) adds `first_psk31` at **:223** from
  `log.Modes`; `AchievementBadges.Firsts` (**:277**) pairs the keys with the screen's words -
  `("first_psk31", "A PSK31 contact")`. The keys come from
  `data\achievements\achievement-points.json`, whose own header says *Edit freely; Hamlet reads this
  file at startup and never hard-codes a value*, and which has `"first_psk31": 15` at **:125**.
- **The Modes badge and its cards.** `AchievementBadges` builds the badge at about **:338** with the
  words *five modes to work* composed from `WorkableModes`; `AchievementCategory.ModesFor`
  (**:666**) makes a card per worked mode from `log.Modes` and a card per **unworked** mode from
  `ContactModes.Six.Where(m => m.IsContactMode && !log.Modes.Contains(...))` (**:676-:677**).
- **The classes this unit will stand beside.** App: `TheLogSaysTheModeItWasMadeInTests`,
  `ThePsk31LogsWithRstTests`, `TheLogShowsBothGridsTests`, `TheBadgesCountContactsTests`,
  `TheLogDialogAndTheWorkedMarkTests`, `TheAchievementsScreenTests`, `TheAchievementsPageTests`.
  Engine: `ThePsk31AdifTests`, `TheAdifLogRoundTripsTests`.
- **The lists.** `docs\carry-forward-tests.txt` carries the per-mode guard table with Olivia's three
  rows and names **`TheAchievementsScreenTests.WsprIsNotAFirstAnybodyCanEarnAndTheCardSaysSo` and
  `TheAchievementsScreenTests.TheWindowDrawsEverySixRows` as known reds that are never on the list**
  - those are 5.3's two. `docs\chain-guarding-tests.txt` holds 17 engine and 11 app classes.
- **Carry-forward at the end of unit 367:** app **189 of 189** (2 m 18 s), engine **146 of 146**
  (4 m 45 s). Chain-guarding: engine **98 of 110** (47 s), app **48 of 63** (33 s), the same
  twenty-seven inherited reds by name before and after. `PttOn` write lines **1**, `Arm(` call
  lines **2**.
- `assets\fixtures\captured\` holds only `README.md`. **No real Olivia audio is in the tree.**

**Expected mismatches and reds, already known. Do not rediscover them as new:**

- The reload's disagreement: `PROJECT_STATUS.md` `RULES_AT` says HM-DEC-165 (2026-09-19) and
  `CLAUDE.md` §1 holds CPS-DEC-0165. That is the carried id-scheme split. **Do not repair it.**
- **`PHASE_STATUS.md` says `CURRENT_STEP: 3` with step 3 `partial` and step 4 `done`**, which
  understates the tree by a whole step. **Decision CF tells this unit to correct it**, and it is the
  one file of the eight this unit may write. `PHASE_PLAN.md`'s own checkboxes are **not** edited.
- **`PHASE_PLAN.md` leaves 1.5, 1.7 and every one of 4.1 to 4.8 unchecked** though units 359 to 367
  met them. **Reported, not edited** (§10).
- **`PHASE_OUTCOME.md` carries two step 4 entries whose decision letters clash** (AO-AY twice). The
  file is append-only: **do not edit any earlier entry.** This instruction's letters start at
  **BU** because unit 367's ran to BT.
- R27 and R29 name `data/rsid-codes.json` and `data/olivia-calling.json`. The tree has
  `data/rsid/` and `data/bands/`.
- **Untracked in the root and under `.run-unit\`:** `.u365-head.md`, `.u365-report.py` and units
  366's and 367's `commit-*.txt` scratch, left because `rm` is refused. **Do not commit them and do
  not build on them.**
- Red and not on either list, as they were before unit 367:
  `TheOperatorCanStopItTests.TheStopAddedNoNewRouteToATransmission`,
  `TheTopRowTests.TheBestBetPillAndTheGreenBlockNameTheSameBandOnTheWindow`, and
  `WhereTheTransmissionStartsAndWhatTheRecordSaysTests.ATransmitRecordCannotCarryTheMessageOrTheCallsignInIt`.
- **The twenty-seven chain-guarding reds are inherited and carried as ask 35.** That list holds
  known reds on purpose. Compare by name, do not chase.
- Known headless flakes: `ThePsk31OfferTests.TheOfferIsOneButtonAndItIsTheOneTheEngineNamed`,
  `ThePsk31ConversationCardTests.NoSlotClockUnderPsk31AndFt8AndFt4StillShowIt` (unit 367 item 4),
  `ThePowerIsOfferedTests.TheOfferRendersAtHalfAndNothingMirrorsTheUsbModLevel` and
  `TheTestsStayOffTheNetworkTests.The354LayoutReadsTheSameNumbersTwiceRunning`. Rerun a red that
  passes alone up to three times and say which run the number came from.
- Uncommitted at authoring: `PHASE_OUTCOME.md`, `PHASE_STATUS.md`, `RUN_LEDGER.md`, `.run-unit\`,
  and this file.

## 6. Rulings in force

Transcribed from the owner's documents. **Do not re-argue them, and do not re-argue what they
rejected.**

**PHASE_PLAN.md R27 - RSID, both ways, always, Hamlet-wide.** *Every keyboard-mode transmission
Hamlet sends begins with the RSID burst naming its mode and variant - Olivia, and PSK31
retroactively. Hamlet listens for RSID across the passband and when one arrives sets the mode,
the variant and the offset itself. **The operator never picks a variant.** A carrier that never
announced itself gets the blind search of step 2 as a fallback. The tables and codes are
`data/rsid-codes.json`, ported from fldigi's `rsid.cxx` (GPL-3).*

**PHASE_PLAN.md R28 - Identical to PSK31 above the modem.** *The same receipt, conversation card,
Answer, Report, Confirm on certainty, the typed line framed with the callsigns and the hand-back,
the same parser. The mode chip says Olivia, **the row says the variant**, the calling spot is
Olivia's. Timing rules scale with the variant (§3.2).*

**PHASE_PLAN.md R30 - Synthetic fixtures first.** *The fixtures under `assets/fixtures/olivia/` were
made from the mode author's own transmitter and are independent of anything Hamlet thinks Olivia is.*
**Hash them before use and write no new WAV under `assets\`.**

**PHASE_PLAN.md R31 - This phase runs unattended.** *Progress is counted in criteria by id. A
done step is closed. The owner's step ends the run. Two rulings per unit at most. A question
about layout, wording, a number or a mechanism is the arbiter's to answer, mark and continue.*

**PHASE_PLAN.md step 5, the criteria this unit works, verbatim:**
- *5.1 An Olivia contact logs with RST and grid; the ADIF carries `MODE=OLIVIA` and the variant
  submode.* **must-pass**
- *5.2 Before the first Olivia contact no earned Olivia card is visible; after it the mode's records
  appear; the Modes badge counts it.* **must-pass**
- *5.3 The two inherited reds in `TheAchievementsScreenTests` are not made worse.* **must-pass**
- *5.4 The export imports cleanly into one named logger.* **nice-to-pass**

**PHASE_PLAN.md step 5's entry, verbatim:** *step 4 done; a loopback exchange reaches 73, checked
first.* **That check is task 0's and it happens before anything is built** (decision CE).

**PHASE_PLAN.md §6, the lines that bind this unit.**
- *A mode that works stays working - HM-DEC-165, Tim, 2026-09-19. No unit is complete, whatever its
  own criteria say, if a mode that reached the air before it does not reach the air after it, or a
  mode that read the air before it reads less. The carry-forward list carries a send guard and a
  read guard per working mode; a red after is a regression, named, and the next unit's first task
  is the repair.*
- *The arbiter stops for three things only: keying, transmit or the radio's safety; money past the
  budget; a decision that changes what the product promises the operator - a fact stated about the
  radio, a contact or a send. A hint, a label, a number, a layout, a mechanism arithmetic will not
  allow: the arbiter decides, marks it author's and overrulable, and continues.*
- *A later ruling of Tim's contradicts a line of this plan. The later ruling wins.*
- *A must-pass ceiling is missed by a little. Ship, report the number, `partial`, move on. Never
  loosen a test.*
- *A done step is closed. Only Tim reopens it.* **Step 4 is closed. Do not reopen 4.5 or 4.8.**
- *A package is needed. `MOVE: stop`.* **This is why 5.4 is decision CC and not an install.**
- *Anything touches the transmit chain beyond adding an audio generator and an RSID prefix behind
  the one sequence. `MOVE: stop`.*
- *A file must be deleted. Empty it, comment it, list it.*

**PSK31 plan §R1 - the certainty gate.** A card exists only where the parser was **certain** a
station addressed the operator. **That card is this unit's evidence that a contact happened in a
mode**, and no looser reading replaces it.

**PSK31 plan §R11 - nothing at the radio.** Nothing in this unit goes near the rig.

**PSK31 plan §R12 - a session fixes its own tests.** *A test a session wrote while a door was shut,
that later blocks the unit told to open the door, is the session's to rewrite in its own commit so
it guards the rule and not the shut door - and that is not a ruling, not an ask, and not a stop.*
**A test asserting that the achievements count five modes is exactly such a test.**

**PSK31 plan §R13 - telemetry is a must-pass on every remaining step.** *Every stage a step adds
writes an event in the `psk31` category that lets a person diagnose that stage from the file alone,
proved by assertion against a fixture, with nothing personal in it (HM-DEC-018, §2.1).* **Logging is
a stage: `Psk31Events.ContactLogged` (about `MainWindowViewModel.cs:15160`) carries the mode and, for
Olivia, the variant - and no callsign and no text.**

**PSK31 plan §R14 - eyes on the prize.** *A test exists to prove an exit criterion. A unit writes
the tests its criteria need and no others.*

**CLAUDE.md §0.0 - honesty.** A sentence on the screen says what Hamlet actually knows. **A log
record is the longest-lived sentence Hamlet writes.** A contact made on Olivia that says `PSK31`,
a submode guessed where no variant was measured, or a badge claiming a mode was worked when the
record says nothing of the kind, are each the fault this step exists to prevent.

**CLAUDE.md §0.2 - Transmit safety, absolute.** **Nothing in this unit keys anything.** If a task
finds itself on the transmit path, it is in the wrong place.

**FACT-004.** *There are two computers, and only one of them has a radio on it.* Nothing measured
here is evidence about the radio. **No Olivia signal from Hamlet has ever been on the air, and no
Olivia contact in the log is a contact anybody made** - every record this unit writes is composed on
the development machine from a loopback exchange. **The first real one will be Tim's, at step 6.**

**HM-DEC-139**, **HM-DEC-155**, **HM-DEC-165**, as in sections 1 and 3.

**Standing decisions of earlier instructions, still in force:** H (format facts from
`data\olivia\format.json` with `pj_mfsk.h` citations - transcription, never a port), **I (the variant
and the center come from the detector or the row, never from the test)**, AF (one row path), AJ (the
retire window), unit 365's AP to AY (the modulator), unit 366's **AZ (one door, and the forbidden
list)**, BA, BB, BC, BD, BF, BH, and unit 367's **BK** (the move is the audio center, not the dial),
**BL**, **BN**, **BO**, **BP**, **BQ**, **BR** (the card survives the move). **AZ's forbidden list
binds this unit exactly as it bound units 366 and 367**, and this unit has no business near it.

**The arbiter's own decisions for this unit.** Author's, overrulable, not rulings. They continue
unit 367's letters, which ran to BT.

- **BU. The mode is read off the conversation and never off the tab.** `Psk31ContactWith` answers
  **Olivia** where the station's card is an Olivia card - `Ft8ContactCard.IsOlivia` /
  `OliviaVariant`, filled from the channel the parser was certain about - and **PSK31** where it is
  not. That is the same rule unit 326 wrote for PSK31 in that method's own remarks - *a station
  worked on PSK31 is a PSK31 contact whatever the operator has since pressed* - applied to the mode
  that now shares the cards. *Rejected:* asking which tab is selected, which logs the screen's
  present state rather than what passed; and a second dictionary of Olivia cards, which is a second
  place for the same fact to live and disagree.
- **BV. `MODE` and `SUBMODE` travel as one object, and the variant is inside it.** `ContactModes`
  gains a lookup that returns the Olivia entry **for a named variant** with `AdifMode = "OLIVIA"`
  and `AdifSubmode` the ADIF spelling of that variant - `OLIVIA 8/250`, `OLIVIA 16/500`,
  `OLIVIA 32/1000` - built from the variant names the format file already holds, so there is **no
  assignment anywhere that can set one without the other** (`Ft8ContactLogEntry.For`'s own rule).
  `ContactModes.Logged`'s existing null-submode entry stays what a record with no variant matches.
  **The spelling is checked against `ContactModes.Cite` as a reading carried forward** and **the
  tests do not reach the network**; say in the report that it is a citation and not a fetch.
- **BW. Which variant is logged when the QSO moved.** **The one the conversation ended at** - the
  card's current variant - because unit 367's one click means a contact can start at 8/250 and
  finish at 16/500, and what the operator read on the card at the end is what his log should say.
  **Prove it on the move's own case** and report the record. *Rejected:* the variant the contact
  opened at, which the card no longer shows; and two submodes on one record, which ADIF has no room
  for.
- **BX. An unknown variant logs the mode and no submode.** Where no card variant is readable the
  record says `MODE=OLIVIA` with **`SUBMODE` absent**, never a guessed one (§0.0) - which is what
  `ContactModes.Logged`'s own remark already provides for.
- **BY. The achievements count what the log can write.** Where the achievement code reads
  `ContactModes.Six` to decide what a mode is - `AchievementLog`'s mapping and
  `AchievementScores.WorkableModes` - it reads **`ContactModes.Logged`**, so a record saying
  `MODE=OLIVIA` maps to a mode, `WorkableModes` becomes **6**, and the Modes badge's words and its
  *all modes* bonus follow from that count **and are not typed as a number anywhere**. *Rejected:*
  putting Olivia inside `Six`, whose name and remarks are the operator's own list; and leaving the
  count at five, which leaves a badge asking for five of six and an *all modes* bonus payable
  without the mode this phase built.
- **BZ. What *no earned Olivia card* means, and what is allowed before the first contact.** 5.2 is
  asserted as: with a log holding no Olivia record, **no earned card, no first and no badge count
  names Olivia anywhere on the screen**; with one Olivia record, the Modes card for Olivia is earned,
  the first is earned, and the badge's worked count rises by one. **An *unworked* Olivia row is
  allowed before the first contact** and is what every unworked mode gets - it is an invitation, not
  a claim that he worked it. If a test shows that row reading as earned, that is the fault 5.2 names.
- **CA. The Hall of Fame gains `first_olivia`, worth 15.** The key goes in
  `data\achievements\achievement-points.json` beside `first_psk31`, whose value is 15, because
  Olivia is the other keyboard mode and the file's own header says the values are arbitrary by
  ruling and free to edit; the words in `AchievementBadges.Firsts` are **`An Olivia contact`**, in
  the shortened form unit 332 set. **The number is the arbiter's and overrulable**; say it in the
  report so Tim can change one line.
- **CB. 5.3 is measured, not assumed.** Run `TheAchievementsScreenTests` **whole, by name, before any
  change and again after task 4**, and report **which names failed and with what message** each time.
  *Not made worse* means: the same two names fail, for the same reason, and **no third name in that
  class goes red**. A third is a regression and its repair is the next task (§1).
- **CC. 5.4 is not reachable on this machine, and it is not faked.** No third-party logger is in the
  tree, **a package is a `MOVE: stop`** (§6) and the tests stay off the network, so nothing here can
  import anything into anything. **What the unit does instead**, and it is not 5.4: export the
  record, **re-read it through Hamlet's own `AdifLog` reader** with `MODE`, `SUBMODE`, `RST_SENT`,
  `RST_RCVD` and `GRIDSQUARE` intact, and check the two spellings against `ContactModes.Cite`'s
  enumeration as carried in the tree. **Then report 5.4 as not met**, nice-to-pass, with that reason
  in one sentence, and carry it to step 6 where Tim is at a machine with a logger on it. **Never
  write that a logger read the file.** A round trip through Hamlet's own reader proves the file is
  self-consistent and proves nothing at all about Log4OM, N1MM or LoTW.
- **CD. Nothing in this unit goes near the transmit chain.** Decision AZ's forbidden list binds
  unchanged, `PttOn` stays **1** code line and `Arm(` stays **2** call lines, counted and reported at
  the end. This unit reads a conversation that has already happened. **Needing a line on that list is
  `MOVE: stop`.**
- **CE. The entry - a loopback exchange reaches 73.** At task 0, before anything is built: an Olivia
  conversation carried to **`73`** on both halves of one card, Hamlet's own modulated sends read back
  through its own demodulator and listener as units 365 to 367 proved they are, with the card open
  and `CanLogRow` true at the end. **Report how it was reached and how long it took.** If it cannot
  be reached without new source, **that build is task 1's business and the entry is reported as
  reached by what task 1 added** - not waved through, and not declared open on an exchange that
  stopped at the report.
- **CF. `PHASE_STATUS.md` is the unit's to correct, and never the arbiter's.** Unit 367's section 4
  item 5 asks who may fix the launcher's file. **The arbiter may not**: `ARBITER.md` §5 lets it write
  `WORK_INSTRUCTIONS.md` and nothing else. **The unit may, and this one does**, as unit 365's
  decision AO already did: at task 0 set `CURRENT_STEP: 5`, step 3 and step 4 to `done`, and
  `WORK_INSTRUCTION` to 368, **each citing `PHASE_PLAN.md` §8's revision of 2026-09-19 evening and
  units 363 to 367's reports in the commit message**. `PHASE_PLAN.md`'s own checkboxes stay
  unedited - they are the owner's file (§10).
- **CG. The chain and the lists** (HM-DEC-165). Run `docs\chain-guarding-tests.txt`'s 17 engine and
  11 app classes by exact name, foreground, **before any change and again after task 4**, comparing
  reds by name. Run `docs\carry-forward-tests.txt`, both invocations, before the first change and
  after the last. **One name joins the app line and none joins the engine line**: Olivia's log
  guard, the test that an Olivia contact logs as Olivia with its submode. Say what the app
  invocation cost afterwards.

## 7. Status cadence

`tools/status.sh`, real clock, after every commit and every task, and immediately before each
`dotnet test` invocation.

---

## 8. The tasks

### Task 0 - the unit opens

- **Step 5's entry, first.** Step 4 done: decision CG's before-run of the chain-guarding list, reds
  by name and seconds, then `ThePsk31CqGoesOutTests`, `TheFt8AndFt4SendsAreByteIdenticalTests` and
  `TheOliviaSendTests.AnOliviaCqReachesTheAir` by name. **Then decision CE's loopback exchange to
  `73`.** If a class that guards a working mode's send or read is red, **stop and report it** - the
  entry is not open and the repair is task 1.
- Hash all nine Olivia fixtures against `manifest.json`, 9 of 9.
- **Correct `PHASE_STATUS.md` per decision CF**, and report what it said before you did.
- Append `UNIT 368 - STEP 5` to `PHASE_OUTCOME.md`, at the end, in the shape of the `UNIT 367`
  entry. **Touch no earlier entry.**
- Patch-bump the version by one, from 1.13.55.
- Run the carry-forward list, **both invocations**, before any change, with the seconds and the
  per-mode guards' states.
- Commit `PHASE_OUTCOME.md`, `PHASE_STATUS.md` and `WORK_INSTRUCTIONS.md` as they stand. **Do not
  commit** `SESSION.lock`, `RUN_LEDGER.md`, `.u365-head.md`, `.u365-report.py` or anything under
  `.run-unit\`.

**Drop candidate:** none.

### Task 1 - the trace, before anything is built

**Say what you find rather than confirming this list.** Report it in section 1 before task 2 writes
a line. Put it in `Unit368Trace`, a class that asserts nothing, in the shape of `Unit367Trace`, not
on either carry-forward line.

1. **What an Olivia contact logs as today.** Take the station from task 0's exchange and call
   `ContactLogEntryForStation`. **Quote the whole record**: `MODE`, `SUBMODE`, `RST_SENT`,
   `RST_RCVD`, `GRIDSQUARE`, `BAND`, `FREQ`, times. Name the line that decided the mode.
2. **Whether 5.1's *with RST and grid* already holds for Olivia.** `Psk31ReportsFor` and
   `Psk31GridFor` read the shared card dictionaries - say whether they answer for an Olivia station
   and what they answered. **If they already work, say so and build nothing there** (R14).
3. **Where the variant is at logging time.** The card's `OliviaVariant`, the channel's, the row's -
   which of them is readable from `ContactLogEntryForStation`, and **what each says for a station
   that moved from 8/250 to 16/500 by unit 367's one click** (decision BW).
4. **What the achievements do with `MODE=OLIVIA` today.** Feed a log holding one such record and
   report: `AchievementLog.Modes`, the Modes badge's worked count and words, what
   `AchievementCategory.ModesFor` draws, and what `FirstsEarned` returns. **Name every line that
   reads `ContactModes.Six`** and would have to read `Logged` under decision BY.
5. **What `TheAchievementsScreenTests` does right now**, whole class, by name: passed, failed, and
   **the failure message of each red** (decision CB). This is 5.3's before-number.
6. **Which existing tests assert the count of modes, the badge's words, or the *all modes* bonus**,
   by name and line - the ones decision BY will make fail, and which of them are §R12 rewrites
   rather than regressions.
7. **Where the ADIF submode spelling can be checked from inside the tree**: what `ContactModes.Cite`
   points at, whether any copy of the enumeration is vendored under `data\`, and **what a test may
   assert without a network call**.

**Drop candidate:** none. A trace is never dropped.

### Task 2 - an Olivia contact logs as an Olivia contact, with RST and grid (5.1, first half)

Build to decisions BU, BW and BX. The new app class is `TheOliviaLogsAsOliviaTests`.

**Tests watched failing first:**

- **The station from a loopback Olivia exchange logs with `MODE=OLIVIA`**, not PSK31, and **the RST
  each way and the grid are the ones the card showed** - the same readings, asserted against the
  conversation and not against a literal.
- **A PSK31 station in the same session still logs `MODE=PSK31`**, and an FT8 and an FT4 contact are
  untouched. **This is the regression assertion for decision BU** and it is not optional.
- **A station whose card carries no variant logs the mode with no submode** (BX), absent rather than
  guessed.
- **Telemetry** (R13): `Psk31Events.ContactLogged` for an Olivia contact carries the mode and the
  variant, **no callsign and no text**; the privacy walk green.

**Drop candidate:** none. It is the first half of the step's first must-pass.

### Task 3 - the export says `MODE=OLIVIA` with the variant as `SUBMODE` (5.1, second half)

Build to decisions BV and BW.

**Tests watched failing first:**

- **The exported ADIF carries the pair** - `MODE=OLIVIA` and `SUBMODE=OLIVIA 16/500` - **quoted
  verbatim in the report**, for a contact that ended at 16/500, and `OLIVIA 8/250` for one that
  ended there.
- **The pair cannot be set apart**: there is no path that writes `MODE` without the submode coming
  from the same `ContactMode`, proved the way `Ft8ContactLogEntry`'s remark asks.
- **The move's own case** (BW): a contact that opened at 8/250 and moved to 16/500 exports the
  variant it ended at, with the reason in the report.
- **A round trip**: the file re-read by `AdifLog`'s own reader gives back the same mode, submode,
  RST, grid, band and times. **Say plainly that this is Hamlet reading Hamlet and is not 5.4.**
- **The spelling against the citation** (BV, CC): the two strings checked against
  `ContactModes.Cite`'s enumeration as carried in the tree, **with no network call**.
- **`ThePsk31AdifTests` and `TheAdifLogRoundTripsTests` stay green and unedited**, or, where one
  guarded a door this unit opens, §R12 applies - rewrite it in its own commit and name the
  assertions that changed.

**Drop candidate:** none.

### Task 4 - the mode's records appear, and the two reds are no worse (5.2 and 5.3)

Build to decisions BY, BZ, CA and CB.

**Tests watched failing first**, in `TheOliviaRecordsAppearTests`:

- **Before**: with a log holding no Olivia record, **no earned card, no first and no badge count
  names Olivia** - each its own assertion. An unworked Modes row is allowed (BZ).
- **After**: with one Olivia record in the log, the Modes card for Olivia is earned, `first_olivia`
  is earned and reads **`An Olivia contact`**, and **the Modes badge's worked count rises by one**.
- **The badge's target is composed, not typed**: `WorkableModes` reads 6 and the words follow it;
  **no literal five or six anywhere in the change**.
- **The other five modes are unharmed**: an FT8-and-PSK31 log scores exactly as it did before this
  unit, asserted against the numbers task 1 recorded.
- **5.3** (CB): `TheAchievementsScreenTests` whole, by name, **with each red's message**, compared
  against task 1 item 5. **The same two, for the same reason, and no third.** If a third goes red,
  its repair is the next task before anything else.

Then run decision CG's after-run of the chain-guarding list. **Give the seconds and compare every
red by name against task 0.**

**Drop candidate:** none. 5.2 and 5.3 are both must-pass.

### Task 5 - what 5.4 can honestly reach, and what it cannot

Build to decision CC. **The deliverable is a measurement and a plain sentence, not a claim.**

- The exported file's full text for one Olivia contact, **quoted in the report**.
- Every field checked against the ADIF citation's own requirements as they are carried in the tree:
  the mode, the submode, the RST tags for an RST mode (`AdifLog.IsRstMode`), the band, the
  frequency in MHz, the times.
- **Then say, in one sentence and without hedging, that 5.4 is not met**: no third-party logger is on
  this machine, a package would be a `MOVE: stop`, and the tests reach no network - so nothing here
  has imported the file into anything. **Carry it to step 6**, where Tim has a logger.

Run the carry-forward list, both invocations, at the end of this task whether or not it is built,
and count `PttOn` and `Arm(`.

**Drop candidate: this whole task.** Drop it whole and say so plainly. 5.4 is the step's only
nice-to-pass; with it dropped, step 5 is 3 of 4 with **every must-pass met**, which is what §6 calls
done on its must-passes. **Do not drop it half-built, and do not drop it to save time for anything
except finishing 5.1, 5.2 and 5.3.**

---

## 9. Parked - do not touch, do not raise

- **Step 6 entirely.** Tim at the radio is Tim's, and no test can stand in for it.
- **Every criterion of step 4.** The step is closed (§6). 4.5's move and 4.8's lag are **used** by
  this unit and are not re-argued, re-measured or improved.
- **Unit 367's move-button label** - *Move up 500 Hz and switch to 16/500*. The unit answered it,
  marked it overrulable, and it stands. **Logged, not chased.**
- **R27's across-tab switch**: an RSID heard under PSK31 or FT8 still switches nothing.
- **The compound-callsign parser** (unit 366 item 1): a line whose callsign carries a slash opens no
  card. **If it blocks a test, use a plain callsign and say so.**
- **The `variant` field on `TransmitRecord`** (unit 366 item 3): still forbidden by AZ. This unit
  does not need it - the variant it logs comes from the card.
- **`Psk31ClearSpot`'s margin against Olivia's width** (unit 366 item 4) and **`OliviaCallingOffsetHz`
  answering null on a busy calling spot** (unit 367 item 2). Both are findings about the send path.
- **The wider `Psk31`-named sweep** (unit 366 item 6): bounded to what the log and the achievements
  actually read. Do not sweep the rest.
- **`PHASE_PLAN.md`, `PHASE_OUTCOME.md`'s earlier entries, `tools\`, `.run-unit\` and
  `RUN_LEDGER.md`.** Reported, not edited. `PHASE_STATUS.md` is the one exception and decision CF
  bounds it exactly.
- **Carried from earlier:** the FT8 unit's retry at the press and its starter card, `longestSeconds`
  on the typed line's record, the *60 s of text* rounding, the silence after the burst not being a
  field, codes 72 to 75 and the fldigi commit pin, the dial 1500 Hz below the center, the three
  off-list reds, the id-scheme split, HM-OPEN-090, and the screen phase's open asks. **Carried in
  section 4, not worked.**

## 10. What not to do

- **No unfiltered `dotnet test`. Never background and poll. Never compose a timestamp.**
- **Nothing keys anything**, and nothing in this unit belongs on the transmit path (CD). **Do not
  touch any line on decision AZ's forbidden list**, and do not add a second keying path or a second
  `Arm(` site for any reason. The answer to needing one is a stop, not an edit.
- **Do not invent a submode.** A variant Hamlet did not measure is an absent `SUBMODE` (BX).
- **Do not write a mode from the tab.** The conversation says what the contact was made in (BU).
- **Do not claim a logger imported anything** (CC). No network, no package, no install.
- **Do not hard-code the number of modes**, the badge's words, or a point value in source - the
  count comes from the table and the value comes from the points file (BY, CA).
- **Do not edit a fixture, `manifest.json`, `corpus.json`, or `PHASE_PLAN.md`, and write no WAV
  under `assets\`.**
- **Do not edit an existing test to make a change pass.** If one guards a rule this unit changes by
  ruling - the count of modes, the badge's words - §R12 applies: rewrite it in its own commit and
  name the assertions that changed. **Never loosen 5.3's comparison**, and never mark a red as
  expected to make a run look clean.
- **Nothing this unit adds goes on the engine carry-forward line** (CG).
- **Report mismatches. Repair nothing outside the task. Write American.**

## 11. Committing and pushing

Commit per task, and push after each, on `main`. Write each commit message into a file under
`.run-unit\` with the editor and pass it with `git commit -F`. Never commit `SESSION.lock`,
`.run-unit\`, `RUN_LEDGER.md`, `.u365-head.md` or `.u365-report.py`. The report names the branch and
whether every push succeeded.

---

## 12. Reporting

Write `output.md` at the root, then stop. **Every exit writes it**: complete, stopped, or with
task 5 dropped. **Canonical headings:** `## 1. What Claude did`, `## 2. What the owner should
expect`, `## 3. What you should see`, `## 4. What's blocking us`.

**First, the ordering block. `validate-output.bat` refuses a report without it.**

```
READ IN THIS ORDER.

A. The phase goal - Hamlet works Olivia the way it works PSK31: hears it,
   reads it, answers it, logs it. Steps 0 to 4 done, step 4 closed at 8
   of 8 by unit 367; step 5 was 0 of 4 at the start of this unit and is
   N of 4 after it; step 6 is Tim's. Say plainly whether step 5 is done,
   done on its must-passes with 5.4 not met, or partial - and which of
   the four verbs in the goal is still unproved.
B. Step 5's criteria this unit worked. Entry: step 4 done and the
   loopback exchange reached 73 <yes|no, and how>. 5.1: the Olivia
   contact logged MODE=<...> SUBMODE=<...>, RST sent <...> received
   <...>, grid <...>; a PSK31 contact in the same session still logged
   MODE=PSK31 <yes|no>; the record quoted. 5.2: before the first Olivia
   record, earned Olivia cards N (0); after it, the Modes card earned
   <yes|no>, first_olivia earned <yes|no>, the badge's worked count
   <before> -> <after> of <target>. 5.3: TheAchievementsScreenTests
   before N passed / N failed, after N passed / N failed, the failing
   names listed both times and the third-red count N (0). 5.4:
   <not met, and why | dropped>. Met: <list by id>. Not met: <list,
   with the reason>. Entry: chain-guarding reds before N, after N, new
   N (0).
C. The report last: section 4 raises N items on top of the carried queue;
   say whether any stands in the way of 5.1, 5.2 or 5.3 - in particular
   whether any working mode's send or read guard went red after green
   (HM-DEC-165), whether any of the other five modes' log records or
   badge scores changed, whether the PttOn or Arm( count moved (1 and 2),
   whether anything needed a line on decision AZ's forbidden list (stop
   material), and the engine and app carry-forward seconds against the
   480 s timeout.
```

**Then the header:**

```
UNIT:       368 - <complete|stopped> at task N of 5, <task 5 built|dropped> - <date time>
PHASE GOAL: <restated in your own words>
UNIT GOAL:  <restated in your own words>
ADVANCED:   <yes|no>
NUMBER:     step 5 criteria met 0 of 4 -> N of 4; must-passes left in step 5 3 -> N
DRIFT:      0
```

**Then a criterion table, 5.1 to 5.4**, each with its state and this unit's own numbers, and a line
saying which of the three must-passes remain.

**Section 3 leads with the operator's evening, from the 73 to the log window to the achievements
screen.** He works an Olivia station - the one that moved up 500 Hz and widened - the exchange
reaches 73, he presses Log, and what he saves says Olivia and says which Olivia. Then he opens the
achievements screen and the mode is there, where before the contact it was not. Then give:

- **the record, field by field**, as it stands in the log and as it goes out in the ADIF - quoted
- the before-and-after of the achievements screen: earned Olivia cards, the firsts, the Modes
  badge's worked count and its words, and the other five modes' scores unchanged
- **`TheAchievementsScreenTests`, by name, before and after**, with each red's message - 5.3's proof
- the round trip through Hamlet's own reader, and **the sentence saying that is not 5.4**
- the chain-guarding and carry-forward runs before and after, by name for every red
- the `PttOn` and `Arm(` counts before and after

**Every figure is computed, not seen. Nothing here is evidence about the radio** (FACT-004). In
particular, **the contact in this report is a loopback exchange on the development machine, not a
station anybody worked**, and the first Olivia contact in Tim's real log will be the one he makes at
step 6.

---

```
ARBITER-DECISION
STEP: 5
APPROACH: an Olivia contact logs with RST and grid and exports as MODE OLIVIA with the variant as SUBMODE, and the Olivia records are revealed by the first Olivia contact
MOVE: continue
WHY: Step 4 closed at 8 of 8 on unit 367 and a done step is closed, so step 5 is next in a one-way pipeline and stands at 0 of 4 with no unit spent on it. The loop test finds no logging, no export and no achievements approach in any entry, so this is the first unit on the step and not a retry. The work is also a repair of a false statement: Psk31ContactWith answers PSK31 for any station with a card, and Olivia now shares those cards, so an Olivia contact logged today says MODE=PSK31 in a file the operator keeps forever.
STATE: not started
DECIDED: author's, overrulable - (BU) the mode is read off the conversation and never off the tab: Psk31ContactWith answers Olivia where the card is an Olivia card by its own variant, and PSK31 where it is not, with FT8, FT4 and PSK31 asserted unchanged. (BV) MODE and SUBMODE travel as one ContactMode carrying the variant, so no path can set one without the other; the spelling is checked against ContactModes.Cite as a reading carried forward and no test reaches the network. (BW) where the QSO moved by unit 367's one click, the variant logged is the one the conversation ended at, because that is what the card showed. (BX) an unreadable variant logs MODE=OLIVIA with SUBMODE absent, never guessed. (BY) the achievement code reads ContactModes.Logged where it reads Six, so WorkableModes becomes 6 and the Modes badge's words and its all-modes bonus follow from the count with no literal in source. (BZ) 5.2 is asserted as no earned card, no first and no badge count naming Olivia before the first record, and all three after; an unworked Modes row is allowed before it. (CA) the Hall of Fame gains first_olivia at 15, the value of first_psk31, in the owner's points file, said on screen as An Olivia contact - the number is the arbiter's and overrulable. (CB) 5.3 is measured: TheAchievementsScreenTests runs whole before and after with every red's message reported, not made worse means the same two for the same reason and no third, and a third is a regression whose repair is the next task. (CC) 5.4 is not reachable on this machine and is not faked - no logger, no package, no network - so the unit round-trips the export through Hamlet's own reader and checks the spelling against the citation, then reports 5.4 not met with that reason and carries it to step 6. (CD) nothing in this unit goes near the transmit chain; AZ's forbidden list binds unchanged and PttOn stays 1 and Arm( stays 2. (CE) the step's entry - a loopback exchange reaching 73 - is proved at task 0 before anything is built, and if it needs new source that build is task 1's and is reported as such. (CF) unit 367's section 4 item 5 is answered: PHASE_STATUS.md is not the arbiter's to correct, because ARBITER.md section 5 lets it write only WORK_INSTRUCTIONS.md, and it is the unit's - task 0 sets CURRENT_STEP 5, steps 3 and 4 done and WORK_INSTRUCTION 368, citing PHASE_PLAN.md section 8 and units 363 to 367; PHASE_PLAN.md's own checkboxes stay unedited. (CG) the chain-guarding list runs before any change and after task 4, the carry-forward list before the first change and after the last, reds compared by name; one app name joins the carry-forward line as Olivia's log guard and nothing joins the engine line. Task 5 (5.4) is the drop candidate; tasks 0 to 4 have none. Unit 367's section 4 asked for one ruling and it is answered by CF; its items 1 to 4 are findings the unit already acted on and are logged, not chased. The letters start at BU because unit 367's ran to BT.
LICENCE: PHASE_PLAN.md step 5 criteria 5.1, 5.2, 5.3 and 5.4 and its entry, R27, R28, R30, R31, section 3.2, and section 6 (a done step is closed; a package is needed; never loosen a test; a must-pass ceiling missed by a little; a number, a label or a mechanism is the arbiter's; HM-DEC-165); PSK31 plan R1, R11, R12, R13, R14; CLAUDE.md 0.0 and 0.2; FACT-004; HM-DEC-018, HM-DEC-139, HM-DEC-155, HM-DEC-165; ARBITER.md sections 2, 4, 5 and 6
ACCOMPLISHED: The evening leaves something behind. A station worked on Olivia goes into the operator's log as an Olivia contact - not as PSK31, which is what Hamlet writes today - with the report and the grid the card showed him and the variant the QSO ended at, and the export says MODE=OLIVIA with that variant as its submode, which is the pair every other logger and every award program reads. Open the achievements screen and the mode is there: a first he has earned, a card among the modes, and a badge that counts Olivia as one of the modes there are to work - where before his first Olivia contact nothing on that screen claimed he had worked it.
ADVANCES: step 5 criteria 5.1 (tasks 2 and 3, must-pass), 5.2 (task 4, must-pass) and 5.3 (task 4, must-pass). With all three met, step 5 has every must-pass met and only 5.4 - which needs a logger this machine does not have - stands between the phase and step 6, Tim's own.
END-ARBITER-DECISION
```
