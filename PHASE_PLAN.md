PHASE: Hamlet reads a CQ call correctly
PHASE_SET: 2026-09-23
DESCRIPTION: The restore phase put the decoder back and stopped it printing what it does not believe, but nothing in the tree ever measured whether the text is right. This phase builds that measurement - edit distance against a key, over a scored region - gets enough keys to work with without asking Tim to read Morse, carries an unsure-per-real guard so the decoder cannot score well by going quiet, and then attacks the fault the first measurement found: the letters are right and the word boundaries are wrong. Judged by a correctness number and, at the end, by Tim at the radio.
STEP: 0 | The number exists - a scorer that measures edit distance against a key over a scored region, every keyed recording in the tree scored, and the numbers tabled as the phase's baseline.
STEP: 1 | There are enough keys - synthetic CQ calls at known speeds and signal strengths with exact keys by construction, plus a written rule for inferring a key from a CQ call on the air, and every one of them scored.
STEP: 2 | The number cannot be gamed - unsure characters per named character carried beside every correctness number, and a named floor on how much of each keyed recording is read at all.
STEP: 3 | The spacing is repaired - the fault the baseline names, where letters are right and word boundaries wrong, attacked on the correctness number with nothing kept that costs a named floor or an anchor.
STEP: 4 | The pitch judge is worth trusting - an instrument whose resolution is finer than the tolerance it judges, the pitch table re-run with it, and the tracker question answered on that table.
STEP: 5 | Tim at the radio - CW on 20 m or 40 m, text on the CW tab that reads as what was sent, and he says it read.
STEP: 6 | The screen stops saying what is not so - every sentence the app states about the radio or a signal is true or says it does not know, the window keeps its arrangement outside his privileges, and every control tells him what it does.
STEP: 7 | The decoder hears what is there - the speed search reaches the speeds stations actually send at, the first minutes of a session read like the rest of it, and the receiver is set correctly for the mode and stays set.

---

# The correctness phase - the reasoning under the step list

**Set 2026-09-23 by the web thread on Tim's ruling.** He chose this phase's outcome before
the restore phase closed: *"Hamlet reads a CQ call correctly."* The yardstick is edit
distance against keys inferred from CQ calls, which he can collect any evening without
reading Morse, with unsure-per-real carried beside it as a guard.

## §1 What this phase is

The restore phase, *CW decodes again*, is archived at `docs/phase-cw-run/` with its 29
loop criteria ticked and **5.1 open - Tim's verdict, which is his to give when he has
looked**. It is not carried here as debt. What it leaves behind:

- A decoder that reads, at 97 s over the capture set where it once took 1995 s.
- A CW read guard on the carry-forward line, so nothing breaks silently again.
- Floors that count **named** characters, with placeholders separate (R57 there).
- An emission gate that prints nothing below `CharacterMargin`: placeholders across the
  37 captures fell 299 to 28 with no named character lost.
- **One number, and it is this phase's starting point**: the bench reads 46 named
  characters on `cw-2026-09-23-173723` and stands **29 edits** from its inferred key over
  the scored region. That capture reads `CQ CQ CQ DEW B 6 RE D W B` where `CQ CQ CQ DE
  WB6RED WB6RED` was sent. **Every letter is right. The spaces are wrong.**
- Three adjudicated anchors, and a pitch table whose judge admits 5 of 38 cases and misses
  a known tone by 60 to 85 Hz (that phase's 3.8, ticked with the doubt recorded).

**What this phase does not do.** Nothing that keys or transmits. No scanner, no screen
work - the RF gain sentence, the window reflow and hover text are `OPEN_ISSUES.md`'s and a
later phase's. No weak-signal work: the captures in hand are strong signals read wrongly,
and that is the fault to fix first.

## §2 What is the same

Every ruling of the phases before stands and is not restated. The ones this phase leans
on: **R11** nothing at the radio; **R12** a session rewrites its own tests; **R14** tests
prove criteria and nothing beyond; **R19** American; **R45** the form of a criterion and of
ADVANCES; **R54** a ruling is wanted only when a criterion of the step in hand needs one,
and anything else goes to `docs/phase-correctness/PARKED.md`; **R57** a floor counts named
characters, not placeholders. **CLAUDE.md §0.0** never present a guess as a decode;
**§0.2** transmit safety; **§12.5** a fixture built from the same misunderstanding as the
code proves nothing. **HM-DEC-091** a change that reads one recording and costs another is
not a fix. **HM-DEC-103** a fixture retires by ruling. **HM-DEC-155** no suite.
**HM-DEC-165** nothing red that was green before. **HM-DEC-168** the floors count named
characters. **FACT-004** dev results are indications. **FACT-006** no radio on the dev
machine.

## §R Rulings, Tim, 2026-09-23

**R59 - the phase, and its yardstick.** Ruled A of three: *Hamlet reads a CQ call
correctly*, measured by edit distance against inferred keys from CQ calls, with
`PHASE_GOAL.md`'s 80 percent on the scored region as the target. Rejected: W1AW bulletins
as the yardstick (an exact key, but it depends on a schedule and a band that cooperates -
it becomes the confirming measurement whenever a bulletin lands cleanly); unsure-per-real
alone (it measures silence, not correctness, and a decoder that prints almost nothing
scores perfectly - so it is carried as the guard instead, in step 2).

**R60 - the loop runs while he is away.** *"Build me a unit that can run. Always moving
forward."* Consequence: every criterion below is a named test or a named number a report
can carry; nothing but 5.1 waits on him; and no step needs a capture he has not yet made.

**R61 - a key is inferred unless it was transcribed.** Nobody in this project reads Morse.
A key inferred from the fixed form of a CQ call is evidence, and it is labeled *inferred*
every time a number is reported against it (§0.0, FACT-004). **A synthetic key is exact**,
because the generator knows what it sent. **An inferred key is never written for audio
nobody could read**: the unscored stretch of a recording stays unscored.

**R62 - Tim, 2026-09-23, night: the screen work joins this phase as step 6.** Ruled A of
three. He listed five things at the radio: the RF gain banner saying the radio did not
confirm while the radio-state dialog showed it read back 24 seconds earlier; the window
reflowing when he tunes outside his privileges; no hover text on any control; three
sentences in the capture sidecar that contradict their own neighbors; and a dead button on
the CW tab. **Step 6 depends on nothing**, so the arbiter always has a place to route when
the CW work stalls, which is why it is here rather than in a phase of its own. Its sentence
is CLAUDE.md §0.0 - never present a guess as a decode, and never state as known what is not
known - which binds in any phase. Rejected: a new phase holding both (an interview before
anything runs, and step 0's ticks archived); the screen in its own phase after this one (the
screen stays wrong for days and the CW work has nowhere to route).

**R63 - Tim, 2026-09-24: tonight's thirteen captures are the benchmark, and tonePeak is
measured over the recording it is printed beside.** He worked 7.052 MHz from 00:39 to
00:46 UTC and the decoder read a whole QSO: 625 characters, 11 unsure, real sentences, the
callsigns `KA2GJV` and `AA3SB` clean and repeated. *"This should be our minimum benchmark
and future iterations should run against that. I don't want to go backwards."* So: the
thirteen captures in `tests/fixtures/cw/captured/unadjudicated/cw-2026-09-24-*` are banked
as floors at what they produced tonight, and the locked-on run gets inferred keys. **The
older captures are not retired** - they are the only guard against a change that reads
tonight's signal better and August's worse, and a fixture retires by ruling anyway
(HM-DEC-103). On `tonePeak`, ruled (a): a figure measured over the recording the sidecar
is about, labeled as such. HM-DEC-091 protects the held peak other things were built on;
it does not require the per-capture sheet to print that particular figure. Rejected: not
printing it in the sidecar; leaving it with its contradicting caption.

**R64 - Tim, 2026-09-24: the night runs itself, and the loop moves when it sticks.**
*"If it gets stuck one place, it can continue... This just stopping and saying I can't go
any further is getting old."* Five criteria below depend on nothing but the banking: the
keys, the spacing, the reflow, the hover text, the dead button. **When a unit cannot
advance the criterion it was authored for, the arbiter authors the next one against a
different criterion rather than halting**, and step 3's three-failure rule stands so the
spacing cannot eat the night. Order of preference when nothing is blocked: **the spacing
first**, then the screen.

**R65 - Tim, 2026-09-24: the RF gain scale is licensed, and nothing carried ever halts the
loop.** Two parts, and the second governs every unit of this phase.

**One: the RF gain ask is answered.** `ReceiverSetup` and `Ic7300Rig.SetSettingAsync` may
compare the RF gain on a single scale - the condition in percent, or the read-back in raw
units - so that a gain already where it should be is recognized as such. **The change makes
the app write less to the radio, not more**, and nothing about keying, transmitting or
power is touched. Unit 411's item 1 is answered and leaves the carried list (HM-DEC-139).

**Two: a carried ask never stops the loop.** A stop 3 is legitimate only when **a criterion
of the step the unit is working cannot be met without a ruling on one of the three**.
Everything else - a carried ask, a question a report raises in section 4, a finding a unit
noticed on the way past, an item already in `PARKED.md` - is parked and the loop goes on,
however squarely it touches keying, transmit, money or a product fact. **A unit does not
carry an ask forward into its own report as blocking unless the criterion it was authored
for is the one that cannot be met.** This was already R54's intent; R65 states it as a rule
the arbiter applies to the stop itself.

**Three: no unit halts for want of work.** When the criterion a unit was authored for
cannot be advanced, the arbiter authors the next unit against a different open criterion
(R64). Six are open and independent: 1.1 to 1.5 the synthetic keys, 3.1 to 3.5 the spacing,
4.1 to 4.4 the pitch judge, 6.3 the reflow, 6.4 the hover text, 6.5 the dead button, 6.7
tonePeak. **Preference when nothing is blocked: the spacing first, then the screen.**

**R66 - Tim, 2026-09-24: a reading that moves onto its own adjudicated text has not been
damaged.** Unit 415 built a space-only relabel that moved no letter, no element and no
placeholder on any of the 51 capture rows, held all 13 named floors identical, and took all
keyed recordings from **217 edits to 185** over 565 characters and the ten bench recordings
from **60 to 36** over 156. It was taken out on 3.2's third test alone, because on
`cw-2026-08-17-134712` the reading changed from `N4 ` to `N4L` - which is exactly the text
HM-DEC-144 adjudicated, 1 edit to 0. **The third test exists so that a change cannot buy
total edits by damaging a reading somebody ruled on; a reading that becomes the ruled text
has not been damaged.** 3.2's third test is amended to read: *unchanged character for
character, or changed to exactly its own adjudicated text*. The amendment is stated before
the re-apply, not after it: **any other change to an adjudicated reading still fails the
test**, and a report that invokes this clause must print the reading before and after so the
owner can see which happened. Rejected: leaving the test as written and losing the change;
narrowing the relabel until `134712` does not move, which would choose the boundary from a
score rather than a trace.

**R67 - Tim, 2026-09-24: entering a mode sets the radio correctly and it stays set.** *"We're
supposed to automatically set the right radio settings when we enter CW mode and when we
enter data mode, and we're not doing it."* He reports Hamlet putting the preamp to stage 1 or
2 in CW, complaining that it is not off, and turning it back on after he sets it off by hand.
The tree shows why: `data/bands/mode-receiver-conditions.json` states the CW preamp as
`"wanted": 1`, a single value, with `wantedText` reading *preamp 1 above 40 m, off at 40 m
and below* - a band rule that the written value does not carry, and that only a comment in
`ReceiverSetup.cs` knows. **And three components decide the preamp independently**:
`ReceiverSetup` writes it on tune-in, `ReceiveAdvice.Preamp` tells the operator to switch it
on whenever it reads off, and `RigObservations.AttenuatorAndPreampTogether` objects when it
is on. On top of that, unit 411 found `ReceiverSetup` filing every write unconfirmed for a
scale mismatch, so nothing is ever recorded as already correct and the operator's hand never
sticks (HM-DEC-056). **This is a requirement broken, not a preference.** It is criterion 7.5
and it ranks above the decode work in this step.

**R68 - Tim, 2026-09-24: the decoder's deafness gets its own step.** Two faults have been
parked repeatedly because no criterion owned them. **The speed ceiling**: on
`cw-2026-09-24-135641`, 14.0475 MHz at 09:56 Eastern - W1AW code practice, whose Fast Code
sessions open at 35 WPM - the search reported *40 WPM won out of 8 to 40, 0.0 better than
silence, at the top of the search: the sender may be faster than Hamlet can look*, while the
independent sweep measured a 27 ms median key-down, about 44 WPM. Nothing was read in 44
minutes. **Acquisition**: the first two minutes of the 7.052 session read `E ET E E` before
the decoder locks, then it reads a whole QSO. Both are now step 7.

**R69 - Tim, 2026-09-24: the stray letters get a criterion in step 3.** After unit 416's two
kept changes, what is left on 17:37 is 10 letters added and 4 wrong of 19 edits - the stray
`E`, `T` and `<BT>` between real words. A single dit reads as `E` and a single dah as `T`, so
any fragment comes out as a confident character and the emission gate passes it. It is the
largest measured decode error left and it is criterion 3.6.

**R70 - Tim, 2026-09-24: entering Morse in any CW-family block is entering CW mode.** Unit
419 met every clause of 7.5 - rewrites of values already correct 1 to 0, contradicting voices
6 to 0, the operator's hand standing, the preamp's band rule carried by the value written -
and 7.5 stayed open on one hole: the `CW DX` and `QRP` blocks state no receiver conditions,
so entering Morse there writes nothing at all. **7.030 MHz, the 40 m frequency where the
preamp rule says off, is the first hertz of the QRP block**, so a radio left at preamp 1
there stays at preamp 1 against the row's own text. All three blocks carry `family: cw`.
**The CW conditions apply to every block of the CW family**, by `sameAs` lines in
`data/bands/mode-receiver-conditions.json` or whatever the file's schema allows, and the
conditions themselves are not changed. Rejected: leaving those blocks silent and closing 7.5
partial, which leaves the reported fault live on 40 m.

**R71 - Tim, 2026-09-24: a floor counts characters the decoder was confident about.** Unit
420 raised P17: a floor counts named characters and only rises, and a stray `E` is a named
character, so every change that removes junk reads as a floor lowered and 3.6 cannot pass its
own guard. This is the restore phase's R57 trap one level down. R57 freed the placeholders,
which are the decoder admitting it does not know; **a stray letter is the decoder being
confident and wrong**, and the two cannot be told apart by the character, only by its span.
On `cw-2026-09-24-153202` the real letters stand at 100 to 1200 and the four stray `E`s at
4.9, 5.0, 14.0 and 6.8. **So a floor counts only characters whose span is at or above a
stated bar**; all 51 rows are re-measured once, in one commit, with the bar named and the old
count, the above-bar count and the below-bar count printed per row; a row that falls solely
because below-bar characters were removed is not a floor lowered, and a row whose above-bar
count falls is a regression. **The bar is chosen from the trace, never from which changes it
would let through**, and the three adjudicated readings unchanged are the independent check
that nothing real was lost. Rejected: exempting the floors from 3.6 altogether, which would
leave the 40 unkeyed captures unguarded during exactly the work most likely to lose
characters; leaving P17 and closing 3.6 unmet.

## §3 What is different from the phases before it

This phase scores text for the first time, so two things bind every unit:

1. **A correctness number is always reported with three parts**: edits, the length of the
   scored region, and whether the key is exact or inferred. *"29 edits over 46 characters
   against an inferred key"*, never *"63 percent"* alone.
2. **A change is kept only on the correctness number**, never on a test turning green by
   itself. The named floors and the three anchors are the cost check, exactly as in the
   restore phase.

## §4 The steps

Exit criteria carry ids `N.k`; met is `[x]`; R45 gives the form. A step's exit is its own
assertions, the three floor tests, and `docs/carry-forward-tests.txt` run as its comment
says - never the whole suite.

## Step 0 - The number exists

**Delivers:** R59's yardstick, and the baseline everything else is measured against.

**Entry:** `PHASE_STATUS.md` names this phase; the tree is Hamlet's.

**Exit:**
- [x] 0.1 A scorer in the test project measures edit distance between a decode and a key over a scored region the key file names, reports edits, scored length and whether the key is exact or inferred, and is watched failing first on a case whose answer is known by construction.
- [x] 0.2 Every recording in the tree that has a key - `cw-2026-09-23-173723` and the three adjudicated anchors - is scored at HEAD and tabled with its three parts, and the table is the phase's baseline in `docs/phase-correctness/baseline.md`.
- [x] 0.3 The report names, from the baseline, what kind of error dominates: characters wrong, characters missing, characters added, or word boundaries misplaced, counted per case rather than asserted.
- [x] 0.4 The three floor tests and both carry-forward lines are green at exit, and nothing is red that was green at entry.

**Depends on:** nothing.

## Step 1 - There are enough keys

**Delivers:** cases to work on without waiting for Tim at the radio.

**Entry:** step 0 done.

**Exit:**
- [x] 1.1 The fixture generator produces CQ calls of the standard form at a named set of speeds and signal strengths, each with its exact key written beside it, and each is read back by the scorer at HEAD and tabled.
- [x] 1.2 The generated set spans at least three speeds and at least three signal strengths, and the report states the recipe for every case so another unit can rebuild them.
- [x] 1.3 `docs/phase-correctness/inferring-a-key.md` states the rule for inferring a key from a CQ call on the air: what may be inferred, what may not, and how the scored region is chosen, with `cw-2026-09-23-173723` worked as the example.
- [x] 1.4 The synthetic cases carry a written statement of what they do not prove (§12.5), and no synthetic case is ever the sole evidence for keeping a change.
- [x] 1.5 The three floor tests and both carry-forward lines are green at exit.
- [x] 1.6 Every capture of the locked-on run - `cw-2026-09-24-004108` onward - carries an inferred key built by differencing consecutive sidecar transcripts, with its scored region named, ambiguous stretches left unscored, and each key file stating that it is inferred and how; each is scored by `CwScorer` and tabled beside the baseline (R61, R63).

**Depends on:** step 0.

## Step 2 - The number cannot be gamed

**Delivers:** R59's guard.

**Entry:** step 0 done.

**Exit:**
- [x] 2.1 Every correctness number reported by the scorer carries unsure characters per named character for the same region, and the baseline table is re-issued with that column.
- [x] 2.2 A named floor per keyed recording states how many named characters must be read at all, set from the baseline, and a change that drops below it is a regression whatever its edit count.
- [x] 2.3 The guard is watched working: a deliberate change that suppresses most output is measured, shown to improve edits while breaking the floor of 2.2, and taken back out in the same unit.
- [x] 2.4 The three floor tests and both carry-forward lines are green at exit.
- [x] 2.5 The thirteen captures `cw-2026-09-24-003901` through `-004550` are tracked in the tree and carry a named-character floor and an element floor each, measured once at HEAD and printed old beside new, and they are run by the capture floor test with the other rows; no existing row is retired or lowered (R63).

**Depends on:** step 0. Independent of step 1: when one blocks the arbiter works the other.

## Step 3 - The spacing is repaired

**Delivers:** the first real improvement in what Tim reads.

**Entry:** steps 0 and 2 done, the baseline and the guard in force.

**Exit:**
- [x] 3.1 The dominant error kind named in 0.3 is traced to a named line or property in `src/Hamlet.RadioEngine/Cw`, printed by a fact that asserts nothing, before any change is built.
- [x] 3.2 Each change is built in its own commit and kept only if the total edit count over all keyed recordings falls, no named floor from 2.2 is broken, the three adjudicated readings are unchanged character for character or changed to exactly their own adjudicated text (R66), with any reading that moves printed before and after in the report, and no capture row's named count falls; a change that fails any of those goes back out in the next commit and the report says so.
- [x] 3.3 The edit count on `cw-2026-09-23-173723` over its scored region is reported before and after every kept change, and the phase's running total is in `docs/phase-correctness/baseline.md`.
- [ ] 3.4 After three consecutive units with no kept change, the trace and the measurements are written to `PARKED.md` and the step closes partial rather than holding the loop.
- [ ] 3.6 The stray single-element characters are attacked: the trace names, per keyed recording, every added or wrong character whose decode rests on one element and what score admitted it, printed by a fact that asserts nothing; then each change is judged under 3.2's four tests, and the total edit count over all keyed recordings falls, with the count of added letters reported before and after (R69).
- [x] 3.7 All 51 capture rows are re-measured once under R71 in a single commit, with the span bar named and the old count, the above-bar count and the below-bar count printed per row; the bar's choice is justified from 3.6's trace and not from which changes it admits; the three adjudicated readings and the keyed totals are run at that commit and are unchanged or better, and the report states plainly that no character above the bar was lost.
- [x] 3.5 The three floor tests and both carry-forward lines are green at the exit of every commit of the step.

**Depends on:** steps 0 and 2.

## Step 4 - The pitch judge is worth trusting

**Delivers:** the question the restore phase's 3.8 could not answer.

**Entry:** step 0 done.

**Exit:**
- [ ] 4.1 A pitch instrument whose resolution is finer than 25 Hz is built and shown, on the synthetic cases whose tone is known by construction, to land within one of its own bins of the truth - the restore phase's sweep missed those by 60 to 85 Hz.
- [ ] 4.2 The 38-case pitch table is re-run with it, and the report states how many cases it admits as single-sender and how many are more than one of its bins apart.
- [ ] 4.3 Every case the new table calls apart is either repaired in `CwToneTracker` under the keep rule of 3.2, or listed with its measurement and parked; the report says which and why.
- [ ] 4.4 The three floor tests and both carry-forward lines are green at exit.

**Depends on:** step 0. Independent of steps 1, 2 and 3.

## Step 5 - Tim at the radio

**Entry:** steps 3 and 4 done or partial.

**Exit:**
- [ ] 5.1 Tim, at the radio, on CW on 20 m or 40 m, sees text on the CW tab that reads as what was sent, and says it read. No script can evaluate this.   *owner's verdict*

**Depends on:** steps 3 and 4.

## Step 6 - The screen stops saying what is not so

**Delivers:** R62. What Tim reads while he is deciding whether to trust the app.

**Entry:** `PHASE_STATUS.md` names this phase; the tree is Hamlet's.

**Exit:**
- [x] 6.1 The RF gain banner states what the radio actually reported: when a read-back for RF gain is held, the banner says the value and when it was read, and the "did not confirm, so I do not know where it is now" wording appears only when no read-back is held; watched failing first against a held read-back, and the report quotes both sentences.
- [x] 6.2 Every sentence the capture sidecar states about a signal is true of that capture or says plainly that it is not measured: `tonePeak` is a figure about this recording or is not printed as one, `elementHz` does not report nothing measured while the line above it resolves elements, and the `keying` line does not say no keying at a pitch in the same breath as counting key-downs there; each is watched failing first on a saved capture that shows the contradiction.
- [x] 6.7 `tonePeak` in a per-capture sidecar is a figure measured over that recording and is labeled as such, watched failing first against the held-and-decaying figure, with the cost at the moment of capture measured and stated (R63).
- [x] 6.8 The RF gain condition and its read-back are compared on one scale under R65, so a gain already at the wanted value is recognized rather than written and filed unconfirmed; watched failing first against a radio already at that value, with the report stating what is written to the radio before and after and showing that no new byte is sent when the value already matches.
- [ ] 6.3 The window keeps its arrangement when the operator tunes outside his privileges: at a frequency his license does not cover, the map, the neighborhood panel and the radio panel occupy the same columns as at a frequency it does, proved by a headless test that measures the panels' placement at both frequencies; the words and the color of the panel still change.
- [x] 6.4 Every control on the CW tab and the band row carries hover text saying what it does - Send, Clear, CQ, RST, 73, the band buttons, the connect or disconnect button, the save star and the circled question marks - proved by a test that names each control and fails when one has none.
- [ ] 6.5 The CW tab's *Have a look* button either does what its words promise or is not on screen, and the report says which and why (HM-OPEN-087).
- [x] 6.6 The three floor tests and both carry-forward lines are green at exit, and nothing is red that was green at entry.

**Depends on:** nothing. Independent of every other step: when the CW work blocks, the arbiter works this.

## Step 7 - The decoder hears what is there

**Delivers:** R67 and R68. What the decoder cannot hear at all, and the receiver it listens
through.

**Entry:** `PHASE_STATUS.md` names this phase; the tree is Hamlet's.

**Exit:**
- [ ] 7.1 The speed search reaches the speeds stations send at: the range's top is raised past 40 WPM, chosen from what the corpus and the independent sweep measure rather than from a round number, and `cw-2026-09-24-135641` - which read nothing in 44 minutes at about 44 WPM - emits named characters, with the winning speed and the margin over silence reported.
- [ ] 7.2 The wider search costs nothing already held: the total edit count over all keyed recordings does not rise, no named floor breaks, the three adjudicated readings are unchanged or move onto their own adjudicated text, no capture row's named count falls, and the captures type's wall time is reported before and after.
- [ ] 7.3 The acquisition failure is traced: for the 7.052 session's opening, a fact that asserts nothing prints what the decoder was doing through the stretch that read `E ET E E` - the speed it held, the pitch it mixed at, the unit it estimated, and the scores it admitted characters on - beside the same figures from the stretch after it locked, and the report names the line or property that differs.
- [ ] 7.4 A change against what 7.3 names is judged under 3.2's four tests, and the named characters read in the opening 60 seconds of `cw-2026-09-24-003901` and `-003919` are reported before and after; after three consecutive units with no kept change the trace goes to `PARKED.md` and the criterion closes partial.
- [x] 7.5 Entering CW mode and entering data mode each set the receiver correctly and it stays set: every condition the mode states is written once when the radio is not already at it and not written when it is; the band rule in a condition's own text is carried by what is written, not by a comment; exactly one component decides each field, and no other component asks the operator to change a field the setup has just set; the operator's own change is not overwritten by a later tune-in of the same mode (HM-DEC-056); and the report tables every field for CW and for data mode - what was asked, what the radio answered, whether it was written, and which component owns it (R67).
- [x] 7.7 Every block of the CW family states the CW conditions (R70): entering Morse in the `CW DX` and `QRP` blocks sets the receiver exactly as the plain CW block does, watched failing first at 7.030 MHz where the preamp rule says off, with the field table printed for one frequency in each of the three blocks and the count of blocks that state conditions reported before and after; the conditions themselves are unchanged, and a condition marked unconfirmed is still not written.
- [ ] 7.6 The three floor tests and both carry-forward lines are green at exit, and nothing is red that was green at entry.

**Depends on:** nothing. Independent of every other step.

## §5 Dependencies

Step 0 depends on nothing and everything depends on it. **Step 6 depends on nothing either
and is independent of every other step.** Steps 1, 2 and 4 depend on step 0 and on nothing
else, so with step 6 there are four places to route when one blocks. Step 3 waits on 0
and 2. Step 5 waits on 3 and 4. When step 0 blocks there is nowhere to route - work it or
halt.

## §6 Branching

- **Three stops only**: keying, transmit or the radio's safety; money past the budget; a
  fact the product states to the operator about a signal, a station or a send. A test's
  shape, a threshold, a recipe, a filter, a timeout: decide, mark author's, continue.
- **The later ruling wins. A done step is closed. Every remaining step Tim's: halt.**
- **7.5 changes what is written to the radio, and that is its point** (R67). It may make the
  app write *less* - a value already correct is not rewritten - and it may carry a band rule
  the condition's own text already states. **It may not change what a condition asks for**,
  and nothing that keys or transmits is touched.
- **Removing a below-bar character is not lowering a floor** (R71), once 3.7 has set the bar.
  A row whose above-bar count falls is a regression and the change goes back out. Until 3.7
  is met, every named character counts as it does today.
- **An adjudicated reading may move only onto its own adjudicated text** (R66). Any other
  movement fails 3.2's third test, and a report invoking the clause prints the reading
  before and after.
- **A stop 3 is legitimate only when a criterion of the step being worked cannot be met
  without the ruling** (R65). A carried ask, a section 4 question, a `PARKED.md` item or a
  finding noticed on the way past **never halts the loop**, whatever it touches. The
  arbiter parks it and authors the next unit.
- **When a unit cannot advance its criterion, the next unit is authored against a different
  open criterion** (R64, R65). The loop halts only at stop 1, when nothing but the owner's
  verdict is left.
- **A stop is for the work, not for a mention** (R54). A carried ask or a finding that
  touches one of the three but blocks no criterion goes to `PARKED.md` and the loop goes on.
- **A run lost before any assertion** - the test host crash inside `Cw` (HM-OPEN-063) or the
  headless dispatcher loop - counts neither way and is re-run once.
- **A correctness number is never reported alone** (§3.1). A report that gives a percentage
  without edits, scored length and the key's kind is incomplete and the arbiter says so.
- **No key may be invented for audio nobody could read** (R61). If the scored region is
  unclear, score less rather than guess more.
- **A named floor from 2.2 is never lowered**, and a capture row's named count is never
  lowered. Placeholders are free (R57).
- **The generator is never the sole evidence** for keeping a change (1.4, §12.5).
- **Step 6 changes what the operator reads, never what the radio does.** A screen criterion
  is met by making a sentence true, never by deleting the sentence and saying nothing, and
  never by changing a radio setting to match a claim.
- **A report's four headings are exactly** `## 1. What Claude did`, `## 2. What the owner
  should expect`, `## 3. What you should see`, `## 4. What's blocking us`. No other wording,
  no fifth top-level heading. Unit 410's report was refused for writing *What Tim should
  expect*, and the loop halted at stop 7 with the unit's work complete.
- **A unit that cannot advance its own criterion does not halt the phase** (R64). It reports
  what it measured, and the arbiter authors the next unit against a different criterion.
  Preference when nothing is blocked: step 3 first, then step 6.
- **Tonight's thirteen floors are never lowered and the older rows are never retired** (R63).
- **Anything would change what keys or transmits.** `MOVE: stop`.
- **A package is needed.** `MOVE: stop`.
- **A CW test costs more than 300 s**: it never goes on a carry-forward line and is run
  alone with its own timeout.

## §7 Carried

The restore phase's 5.1, Tim's, at `docs/phase-cw-run/`; `#15`, `#43`, `#44` and `#42`,
parked there with their measurements; `PHASE_GOAL.md`'s 80 percent, which this phase
measures toward but does not promise; W1AW as an exact-key confirmation whenever a bulletin
lands; the scanner; the screen findings in `OPEN_ISSUES.md` - the RF gain sentence, the
window reflow, hover text; `tonePeak`, `elementHz` and the `keying` line's wording;
HM-OPEN-063 and HM-OPEN-070.

## §8 Revision record

- **2026-09-24, after unit 420.** R71: a floor counts characters at or above a stated span
  bar, so the stray-letter work can proceed; criterion 3.7 re-measures the 51 rows once.
- **2026-09-24, after unit 419.** R70: the CW conditions apply to every CW-family block, as
  criterion 7.7, so 7.5's last hole closes and 7.030 behaves.
- **2026-09-24, after unit 418.** R67 entering a mode sets the receiver correctly and it
  stays set, as criterion 7.5; R68 step 7 for the speed ceiling and acquisition; R69 the
  stray single-element letters as criterion 3.6.
- **2026-09-24, after unit 415.** R66: 3.2's third test allows a reading to move onto its
  own adjudicated text, so unit 415's narrowed relabel `b68be0dd` - 185 over 565, no letter
  moved - can be re-applied.
- **2026-09-24, night.** R65: the RF gain scale licensed as criterion 6.8; a carried ask,
  a section 4 question or a parked item never halts the loop, and a stop 3 is legitimate
  only when the criterion being worked needs the ruling. Written after unit 412 completed
  its work and the loop halted at stop 3 on an ask 412 had carried rather than on anything
  blocking a criterion.
- **2026-09-24.** R63 tonight's thirteen banked as the benchmark, older rows kept, tonePeak
  measured over its own recording; R64 the loop moves rather than halting. New criteria 1.6
  the keys by differencing, 2.5 the floors, 6.7 tonePeak.
- **2026-09-23, night.** R62: step 6, the screen work, depending on nothing so the loop
  always has somewhere to route; §6 gains the exact four report headings after unit 410's
  report was refused over one word.
- **2026-09-23.** Written from the interview: R59 to R61; six steps; the baseline taken
  from `cw-2026-09-23-173723` at 29 edits over 46 named characters against an inferred key.
