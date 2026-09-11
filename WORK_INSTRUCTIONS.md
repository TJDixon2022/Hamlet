# Work instruction 315 - hear everyone

**The seed unit of the PSK31 phase loop.** It executes as iteration 1 under `--seed`;
the arbiter authors every unit after it from `PHASE_PLAN.md`. Steps 0 and 1 are `done`
(units 312, 314). This unit aims at **step 2**.

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

## 1. The two rules that killed sessions

Both HM-DEC-155, Tim, 2026-09-05.

**1. A unit runs no test suite.** Only this unit's own test names, filtered by exact
name, foregrounded, with a stated timeout:

```
timeout 480 dotnet test <project> --filter "FullyQualifiedName~TypeName.MethodName"
```

Plus `docs\carry-forward-tests.txt`, the same way. **Known reds are never on it** -
section 10 lists them. An unfiltered run finds them and tells you nothing.

**2. Never background a command and poll it.** The watchdog fires at **twelve minutes
with no status write.** A demodulator over a 66-second 8 kHz fixture runs in well under
a second; one that takes longer is a finding about the code.

---

## 2. The tool fact

**The shell breaks on an apostrophe inside a quoted heredoc and collapses a doubled
backslash.** Write *do not*; write single backslashes; check what landed on disk.

---

## 3. Asks still outstanding

Carried per HM-DEC-139. **All of these come back in section 4, verbatim where
unresolved.**

1. **Does the transmission record ask the radio whether it keyed?** Unit 303's proposal,
   still Tim's. PSK31's continuous carrier makes it sharper.
2. **Nothing in this repository can look at a picture.** `Avalonia.Headless.Skia` is
   Tim's to add (§0.4).
3. **Three inherited reds, never chased** - two in `TheAchievementsScreenTests`, one in
   `TheFitGuardAsksAboutTheGridTheSendIsOnTests` (engine).
4. **`Ft8ContactCard.Closing`** is uncalled and left standing.
5. **`Ft8GlobePlot`'s unused framing constants.** Report; leave standing.
6. **The licence of `assets/world-flat-relief.png` is unknown.** Raise; do not resolve.
7. **The FT4 phase's run files were never archived under `docs\`.** The web thread
   overwrote them at the root on 2026-09-11 instead of running `install-phase.bat`. They
   are in git history. **Task 1 recovers them into `docs\phase-ft4-run\`.**
8. **The door sentence is a placeholder.** Carry.
9. **Acknowledgement indicators.** Named by Tim, not yet defined. Step 4's turn indicator
   may be what he meant; do not assume it is.
10. **Card ordering under scroll, and its root** - `DigitalCards.Clear()` then new cards
    every slot, per-card state lost. Not this phase's. Carry.
11. **The 2 px map outline** and **the 2.0x popup zoom cap** are sessions' numbers.
12. **Version numbering** - x.y.0 or x.y.1 for a phase's first unit under HM-DEC-150.
13. **The squelch threshold 0.90** is unit 314's number, `Psk31Demodulator.SquelchQuality`.
14. **The listening offset 1000 Hz** is unit 314's starting place - **this unit replaces
    it with a search**, which closes the item if task 3 lands.
15. **Real off-air PSK31 audio** - only Tim can record it. Two or three minutes on 14.070
    with a few signals on it, any recorder, any rate, WAV, into
    `assets\fixtures\captured\`. Every number in this phase is synthetic until then.
16. **One conversation card is taller than the panel** (293 px in 220 px). Not broken.

---

## 4. Why this unit exists

```
PHASE GOAL: Hamlet works PSK31 the way it works FT8.
UNIT GOAL:  Step 2 - hear everyone. Every PSK31 signal in the passband found and
            decoded at once, one row per signal in the decoded-text list, with
            its offset, its strength and its text as it arrives.
ADVANCES:   Step 2, wholly if tasks 2-4 land. The exit criteria moved are the
            two-signal, four-signal and noise-only rows, the retire rule, the
            search, and real-time.
DRIFT:      0 carried from unit 314.
```

**A, in the author's words.** A third digital mode that is Hamlet's - the same two cards,
the same one click, the same log and achievements - on a modem this project writes.

**B, in the author's words.** Step 1 listens at one spot. Real PSK31 is a ribbon of
signals stacked up the waterfall about 100 Hz apart, and an operator chooses from among
them. Step 2 finds every carrier, runs a demodulator on each, and puts each in the list
FT8 already uses. **Rows are text only** - no callsign parsed, nothing clickable, nothing
a station yet. That is step 3.

**What exists to build on.** `Psk31Demodulator` (unit 314): one channel at an offset,
squelch on keying shape, AFC that tracks only while the squelch is open, proved at
0.0000 on every synthetic QSO fixture and zero characters on noise. The FT8 decoded-text
list and its row type. `Psk31Listening.OffsetHz = 1000` - the fixed spot this unit
replaces.

---

## 5. Verify this instruction against the tree

**Everything named below comes from units 312-314's reports.** Check it; **report every
mismatch; do not repair this instruction; do not stop over a mismatch** unless a task is
impossible, in which case say which and why.

- `Psk31Demodulator`, `Psk31Demodulator.SquelchQuality`, `Psk31Listening.OffsetHz`,
  `Varicode`; the audio source the FT8 decoder is fed from; how the PSK31 text row is
  replaced in place (unit 314's workaround for the card-rebuild root).
- The FT8 decoded-text list: its row type, how a row carries offset and strength, and
  how the `CQ` filter selects. **PSK31 rows go into the same list.**
- `assets\fixtures\` - the seven fixtures and `manifest.json` from unit 314, and from
  this delivery `psk31-four-signals.wav`, `psk31-two-signals-100hz-apart.wav`,
  `manifest-step2.json`, and `psk31-transcripts\corpus.json` (step 3's, not this
  unit's).
- `docs\carry-forward-tests.txt`; `docs\psk31-reference.md`.
- `PROJECT_CARD.md`'s `PHASE` and `PHASE_SET` - **probably still the FT4 phase's**,
  because the phase was never installed through `install-phase.bat`. Task 1 fixes it.
- Tests: `ThePsk31DemodulatorTests`, `ThePsk31PanelHearsTests`, `ThePsk31TabIsInertTests`,
  `TheVaricodeTests`, `BindingHealthTests.TheMainWindowBindsWithoutOneComplaint`.

**Expected failures:** none inherited. Every new test is watched failing first.

---

## 6. Rulings in force

**Do not re-argue any of these.**

**`PHASE_PLAN.md` at the root - read it in full.** §1 PSK31's contact is a conversation,
not a protocol. §3 no slot clock, RST not dB, continuous carrier, characters not messages,
no fixed length. §R1 strict on transmit, permissive on display, guesses marked. §R3 the
parser's vocabulary - **step 3's, not this unit's**. §R5 fldigi read, never ported. §R6
the activity is 14.070-14.074 as the cited row says. §R8 the receipt carries no Log. §R9
a character the demodulator was not sure of is not shown. **§6 Branching** - the calls
already made: a missed ceiling is `partial`, never a loosened test; a package is
`MOVE: stop`; missing off-air audio is raised, not stopped for; the card-rebuild root is
worked around, not chased.

**§0.0 / HM-DEC-092** - never present a guess as a decode; a picture binds as hard as a
sentence. **A row that appears is a row Hamlet is sure is a carrier.**

**§0.1** - the engine is never told that tabs exist. The search takes samples and yields
carriers; it does not know it is behind a tab.

**§0.2** - one click, one transmission. **Nothing in this unit transmits or adds a click
that could.** The PSK31 tab stays inert for sending (unit 314, `ThePsk31TabIsInertTests`).

**§0.4** - a package is Tim's. **No FFT library, no DSP package.** The engine has what FT8
and CW use.

**§2.1** - nothing personal in telemetry.

**HM-DEC-054** - the band row is cited data. **HM-DEC-155** - section 1.
**HM-DEC-139** - the asks queue.

**Tim, 2026-09-06 - the dummy load is withdrawn in full.** No compensating control.

**FACT-004** - a dev-machine result is an indication, never a finding. **FACT-006** -
this machine has no radio. **Every fixture is synthetic.**

---

## 7. Status cadence

`PROJECT_STATUS.md` per `CLAUDE.md` §13: **after every task, and at least every ten
minutes.** Write the status **before** starting a test run.

---

## 8. The tasks

Five. Each names the test to watch failing first and one drop candidate. **Drop from the
back.**

### Task 1 - the phase opens properly, and the trace. No production changes except the card.

**1a. Entry criteria, checked by running rather than reading.**
`ThePsk31DemodulatorTests` filtered: the clean fixture at 0.0000 and the noise-only
fixture at zero characters. **If either is not so, step 2 has no ground; report it and
stop after this task.**

**1b. The phase files.** `PHASE_STATUS.md` line 1 names *Hamlet works PSK31 the way it
works FT8* and its `STEP:` lines are the seven PSK31 steps with 0 and 1 `done`. Report
what they say. **Do not edit them** - they are the launcher's.

**1c. `PROJECT_CARD.md`.** Set `PHASE` and `PHASE_SET` to this phase's, as
`install-phase.bat` says is the seed unit's job. Report before and after.

**1d. `DECISIONS.md`.** Record the ruling that set this phase, as Tim's: *2026-09-11,
"let's finish implementing PSK31"*, superseding the FT4 phase at his word *"FT8 and FT4
seem pretty solid"* the same day. **This is the one write to `DECISIONS.md` a seed unit
makes; §12.1 permits recording a ruling the owner gave.** Nothing else goes in.

**1e. Recover the FT4 phase's run.** Its three files were at the root until the commit
that installed the PSK31 plan on 2026-09-11. `git log --oneline -- PHASE_OUTCOME.md`
finds the last FT4 commit; `git show <hash>:PHASE_OUTCOME.md > docs\phase-ft4-run\PHASE_OUTCOME.md`,
and the same for `PHASE_PLAN.md` and `PHASE_STATUS.md`. **Report the hash and the byte
count of each, and that `docs\phase-ft4-run\PHASE_OUTCOME.md` carries `UNIT 300` through
`UNIT 310`.** If it does not, say what it carries; do not fabricate.

**1f. Trace.** With file and line: the audio source and its sample rate; how the FT8 list
takes a row; how unit 314's text row is replaced in place; what `Psk31Listening.OffsetHz`
feeds. Run `docs\carry-forward-tests.txt` filtered before anything changes.

**Test watched failing first:** none.
**Drop candidate:** none. **Not droppable.**

---

### Task 2 - the carrier search

`Psk31CarrierSearch`, engine, §0.1. Fed the passband as samples, it yields **the offsets
of every PSK31 carrier present**, each with a strength, updated as audio arrives.

**How it finds them is the unit's to describe**, not this instruction's to dictate.
What it must be: **a measurement across the passband, not a fixed list of offsets**; able
to tell a 31 Hz PSK31 carrier from a wider signal or a noise peak - the keying shape
unit 314's squelch measures is one honest way; stable enough that a carrier does not
flicker in and out of the list between updates; and cheap enough for task 4's real-time
number. **State the rule and its numbers in one place.**

**Test watched failing first:** `ThePsk31CarrierSearchTests`, engine. Watch it fail, then
green:

1. the two-signal fixture yields carriers at 1000 and 1500 Hz, within 5 Hz, and no others
2. the four-signal fixture yields carriers at 700, 1100, 1600 and 2200 Hz, within 5 Hz,
   and no others - the 1600 Hz one drifting +8 Hz over the file and still one carrier
3. the noise-only fixture yields **no carriers**
4. the clean single-signal fixture yields exactly one
5. strengths are ordered as the fixtures were made: on the four-signal file, 1600 > 700 >
   1100 > 2200

**Drop candidate:** assertion 5.

---

### Task 3 - a demodulator per carrier, into the list

For every carrier the search yields, a `Psk31Demodulator` at that offset - created when
the carrier appears, **retired when it goes**. Each feeds **one row in the same
decoded-text list FT8 uses**, carrying offset, strength, and text as characters arrive.
`Psk31Listening.OffsetHz` is replaced by the search; **report what became of it.**

**The retire rule is a must-pass and its timing is stated.** `manifest-step2.json` records
what happens without one: the author's unsquelched reference read all four carriers at
0.0000 while they were on, and 0.125, 0.386, 0.183 after they stopped, because it kept
emitting garbage from the empty channel. A carrier that stops is retired from the list,
not left as a ghost. **State how long after the last character, and why that number.**

**Rows are text only.** No callsign parsed, no row clickable, no CQ filter behaviour
beyond empty. **Re-run `ThePsk31TabIsInertTests` after this task.**

**Test watched failing first:** `ThePsk31HearsEveryoneTests`, app. Watch it fail, then
green:

1. the two-signal fixture yields two rows, each decoding its own text at or under 0.05,
   with none of the other's text in it
2. the four-signal fixture yields four rows, each at or under 0.10 **over the span its
   carrier was on**
3. the noise-only fixture yields zero rows
4. a carrier that stops is retired within the stated time, and the retire rule and its
   number live in one named place
5. every fixture's SHA-256 matches its manifest before use
6. no row is clickable; `ThePsk31TabIsInertTests` still green

**Drop candidate:** assertion 2's four-signal ceiling - keep the four rows, report the
CERs with no ceiling, mark the step `partial`.

---

### Task 4 - real time, measured

**The search and every demodulator together must keep up with the audio.** Measure it:
the four-signal fixture, 66.6 seconds of audio, processed end to end as a stream - **how
many seconds of CPU on this machine**, reported as a number, and the ratio. Under 1.0 is
the must-pass. State what dominates the cost.

**Test watched failing first:** extend `ThePsk31HearsEveryoneTests` by one: the ratio is
under 1.0 and is reported.

**Drop candidate:** none in this task; it is one assertion. If it is red, **report the
number and mark the step `partial`** rather than dropping the measurement.

---

### Task 5 - two carriers 100 Hz apart

**Drop candidate: this whole task.** Step 2's nice-to-pass.

`psk31-two-signals-100hz-apart.wav` - equal level, 1000 and 1100 Hz. Both found, both
decoded at or under 0.05. The author's reference reads them at 0.0000 and 0.0083.

**Test watched failing first:** extend `ThePsk31CarrierSearchTests` and
`ThePsk31HearsEveryoneTests` by one each.

**Drop candidate:** the whole task.

---

## 9. Parked

- **Step 3** - the parser, callsigns, states, the CQ filter, clickable rows, the nudge on
  PSK31 rows. `assets\fixtures\psk31-transcripts\corpus.json` is its fixture and is not
  read by this unit.
- **Step 4** - the modulator, macros, cards, turn indicator. **The transmit chain is not
  touched.**
- **Step 5** - RST, achievements.
- **Real off-air audio.** Raised, not done.
- **The card-rebuild root.** Worked around for the PSK31 rows as unit 314 did.
- **Acknowledgement indicators.** Build nothing.
- **Reading anything from fldigi** beyond the varicode table already cited.
- **`Avalonia.Headless.Skia` or any other package.**

---

## 10. What not to do

- **No unfiltered `dotnet test`.** **Never background and poll.**
- **Do not touch anything that keys the transmitter.** The chain
  `cq_pressed → … → Played` stays as it is.
- **The dummy load is withdrawn in full.** No compensating control.
- **Do not show a character the demodulator was not sure of**, and **do not show a row
  the search is not sure is a carrier.**
- **Do not use a fixed list of offsets as the search.**
- **Do not add or vendor a package.**
- **Do not hard-code the passband width or 14.070.**
- **Do not edit `PHASE_PLAN.md`, `PHASE_STATUS.md` or `PHASE_OUTCOME.md`** - the launcher
  and the arbiter own them.
- **Do not claim an appearance from computation and call it seen.**
- **Do not invent a ruling id.**
- **Do not chase these known reds:** `CwAdjudicationTests.ASpeedChangeInRealisticAudio`;
  the 51 CW cases in `docs\unit239-failing-set.txt`; the `Ft8Sharp.Deep.Tests`
  whole-type-list tripwire; `HM-OPEN-088`'s ten; the two in
  `TheAchievementsScreenTests`; the one in `TheFitGuardAsksAboutTheGridTheSendIsOnTests`.
- **Do not repair this instruction.** Report mismatches; keep working.

---

## 11. Committing and pushing

Commit per task, push at the end. Patch-bump the version in task 1. Nothing left
uncommitted; say so in section 1.

---

## 12. Reporting

`output.md` at the repository root. **Canonical headings:** `## 1. What Claude did`,
`## 2. What the owner should expect`, `## 3. What you should see`,
`## 4. What's blocking us`.

**The ordering block first - `validate-output.bat` refuses a report without it:**

```
A. The phase goal - Hamlet works PSK31 the way it works FT8. Steps 0 and 1 done,
   2 <state after this unit>, 3-6 not started.
B. Step 2 and its exit criteria - <each of the six must-pass and two nice-to-pass,
   met or not met, with the number>.
C. The report last, and section 4 raises N items on top of a carried queue of
   sixteen; <whether any is in the way of a criterion in B>.
```

Then the header:

```
UNIT:       315 - <complete|stopped> at task N of 5, <which dropped> - <date time>
PHASE GOAL: <restated in your own words>
UNIT GOAL:  <restated in your own words>
ADVANCED:   <yes|no, and which step>
NUMBER:     <what moved, before -> after>
DRIFT:      <n> consecutive units without advance  (was 0)
```

**Section 1 must carry a table: every fixture, each carrier found, each row's CER on its
span, the reference's CER, and the real-time ratio.** And the search rule and the retire
rule, each in one sentence with its number.

**Section 3 must say what Tim will see when he presses PSK31 now** - several rows, each
with an offset and text scrolling in - **and what he must not expect**: no clicking, no
sending, no callsigns picked out. **Every appearance claim is computed, not seen. Say so
once.**

---

```
ARBITER-DECISION
STEP: 2
APPROACH: find every PSK31 carrier across the passband by measurement and run one demodulator per carrier into the FT8 decoded-text list
MOVE: continue
WHY: Steps 0 and 1 are done and proved; step 2 is the next in the pipeline and its ground is the single-channel demodulator unit 314 built. Nothing blocks it.
STATE: not started
DECIDED: the search rule and the retire rule are the unit's to choose and state, within the plan's requirement that both be measurements with a stated number; and the seed unit records the phase-setting ruling in DECISIONS.md and recovers the FT4 run files, because the phase was installed without install-phase.bat
LICENCE: PHASE_PLAN.md step 2 entry and exit criteria, §6 branching, HM-DEC-155; install-phase.bat's stated seed-unit duties; CLAUDE.md §12.1 for recording a ruling the owner gave
ACCOMPLISHED: pressing PSK31 shows every station on the band as its own row of text, not one spot
ADVANCES: step 2 - the two-signal, four-signal and noise-only rows, the retire rule, the search-by-measurement, and the real-time ratio
END-ARBITER-DECISION
```
