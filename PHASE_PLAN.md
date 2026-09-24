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
- [ ] 1.1 The fixture generator produces CQ calls of the standard form at a named set of speeds and signal strengths, each with its exact key written beside it, and each is read back by the scorer at HEAD and tabled.
- [ ] 1.2 The generated set spans at least three speeds and at least three signal strengths, and the report states the recipe for every case so another unit can rebuild them.
- [ ] 1.3 `docs/phase-correctness/inferring-a-key.md` states the rule for inferring a key from a CQ call on the air: what may be inferred, what may not, and how the scored region is chosen, with `cw-2026-09-23-173723` worked as the example.
- [ ] 1.4 The synthetic cases carry a written statement of what they do not prove (§12.5), and no synthetic case is ever the sole evidence for keeping a change.
- [ ] 1.5 The three floor tests and both carry-forward lines are green at exit.
- [ ] 1.6 Every capture of the locked-on run - `cw-2026-09-24-004108` onward - carries an inferred key built by differencing consecutive sidecar transcripts, with its scored region named, ambiguous stretches left unscored, and each key file stating that it is inferred and how; each is scored by `CwScorer` and tabled beside the baseline (R61, R63).

**Depends on:** step 0.

## Step 2 - The number cannot be gamed

**Delivers:** R59's guard.

**Entry:** step 0 done.

**Exit:**
- [ ] 2.1 Every correctness number reported by the scorer carries unsure characters per named character for the same region, and the baseline table is re-issued with that column.
- [ ] 2.2 A named floor per keyed recording states how many named characters must be read at all, set from the baseline, and a change that drops below it is a regression whatever its edit count.
- [ ] 2.3 The guard is watched working: a deliberate change that suppresses most output is measured, shown to improve edits while breaking the floor of 2.2, and taken back out in the same unit.
- [ ] 2.4 The three floor tests and both carry-forward lines are green at exit.
- [ ] 2.5 The thirteen captures `cw-2026-09-24-003901` through `-004550` are tracked in the tree and carry a named-character floor and an element floor each, measured once at HEAD and printed old beside new, and they are run by the capture floor test with the other rows; no existing row is retired or lowered (R63).

**Depends on:** step 0. Independent of step 1: when one blocks the arbiter works the other.

## Step 3 - The spacing is repaired

**Delivers:** the first real improvement in what Tim reads.

**Entry:** steps 0 and 2 done, the baseline and the guard in force.

**Exit:**
- [ ] 3.1 The dominant error kind named in 0.3 is traced to a named line or property in `src/Hamlet.RadioEngine/Cw`, printed by a fact that asserts nothing, before any change is built.
- [ ] 3.2 Each change is built in its own commit and kept only if the total edit count over all keyed recordings falls, no named floor from 2.2 is broken, the three adjudicated readings are unchanged character for character, and no capture row's named count falls; a change that fails any of those goes back out in the next commit and the report says so.
- [ ] 3.3 The edit count on `cw-2026-09-23-173723` over its scored region is reported before and after every kept change, and the phase's running total is in `docs/phase-correctness/baseline.md`.
- [ ] 3.4 After three consecutive units with no kept change, the trace and the measurements are written to `PARKED.md` and the step closes partial rather than holding the loop.
- [ ] 3.5 The three floor tests and both carry-forward lines are green at the exit of every commit of the step.

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
- [ ] 6.2 Every sentence the capture sidecar states about a signal is true of that capture or says plainly that it is not measured: `tonePeak` is a figure about this recording or is not printed as one, `elementHz` does not report nothing measured while the line above it resolves elements, and the `keying` line does not say no keying at a pitch in the same breath as counting key-downs there; each is watched failing first on a saved capture that shows the contradiction.
- [ ] 6.7 `tonePeak` in a per-capture sidecar is a figure measured over that recording and is labeled as such, watched failing first against the held-and-decaying figure, with the cost at the moment of capture measured and stated (R63).
- [ ] 6.3 The window keeps its arrangement when the operator tunes outside his privileges: at a frequency his license does not cover, the map, the neighborhood panel and the radio panel occupy the same columns as at a frequency it does, proved by a headless test that measures the panels' placement at both frequencies; the words and the color of the panel still change.
- [ ] 6.4 Every control on the CW tab and the band row carries hover text saying what it does - Send, Clear, CQ, RST, 73, the band buttons, the connect or disconnect button, the save star and the circled question marks - proved by a test that names each control and fails when one has none.
- [ ] 6.5 The CW tab's *Have a look* button either does what its words promise or is not on screen, and the report says which and why (HM-OPEN-087).
- [x] 6.6 The three floor tests and both carry-forward lines are green at exit, and nothing is red that was green at entry.

**Depends on:** nothing. Independent of every other step: when the CW work blocks, the arbiter works this.

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

- **2026-09-24.** R63 tonight's thirteen banked as the benchmark, older rows kept, tonePeak
  measured over its own recording; R64 the loop moves rather than halting. New criteria 1.6
  the keys by differencing, 2.5 the floors, 6.7 tonePeak.
- **2026-09-23, night.** R62: step 6, the screen work, depending on nothing so the loop
  always has somewhere to route; §6 gains the exact four report headings after unit 410's
  report was refused over one word.
- **2026-09-23.** Written from the interview: R59 to R61; six steps; the baseline taken
  from `cw-2026-09-23-173723` at 29 edits over 46 named characters against an inferred key.
