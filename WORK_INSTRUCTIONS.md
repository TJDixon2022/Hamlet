# Work instruction 266 - the record is honest, and the row knows where the contact stands

```
STOP. Verify the project before reading any further.

PROJECT: Hamlet

Check the repository root:
  MUST EXIST:      SHACK_FACTS.md
  MUST EXIST:      src/Hamlet.RadioEngine/Cw/CwProbabilisticDecoder.cs
  MUST NOT EXIST:  CoreHMI.sln
  MUST NOT EXIST:  MURC.sln

If all four are not as stated, you are in the wrong repository.
REFUSE. Do not read the rest of this file, do not summarise it, do not
adapt it to whatever project you are actually in, and change nothing.
Reply with only: the path you are in, which checks failed, and
"wrong project - nothing done."

If all four hold, say "Hamlet confirmed" and continue.
```

---

## THE TWO RULES THAT KILLED SESSIONS

**Tim's rulings of 2026-09-05. Not this unit's to weigh.**

**1. A unit runs no test suite.** **A unit may run only the unit test it
constructs in that work instruction**, filtered by exact name, in the foreground,
with a stated timeout of a few minutes. **An unfiltered `dotnet test` on any
project is forbidden.**

**2. Never background a command and poll for it.** Sessions were killed by the
watchdog on 2026-09-05 and 2026-09-06, each sitting in
`until grep -q "exited with code" ...; do sleep 15; done` with a `900000` ms
timeout. **The watchdog fires after twelve minutes with no status write.**

`dotnet build` is allowed, foregrounded, with a timeout.

---

## THE TOOL RULE

**This session's shell may refuse calls.** A refused shell call is a signal to
reach for the other tool, not to stop. **The file-editing tools have been
unaffected throughout.** Record every refusal verbatim. **Nothing in this unit
halts the loop.**

**This shell collapses a doubled backslash inside a quoted heredoc.** Use the
file-editing tools for anything with escapes in it.

---

## Why this unit exists

**Eight units advanced real work and closed nothing.** The stop button took 8.4
seconds of audio off the air mid-transmission. The loopback read 3 of 3 messages
back as themselves. Transmit level went from full scale to -12.04 dBFS. **And
steps 2, 3, 4 and 5 all read `partial`.**

**Every one of those steps carried a criterion only Tim's radio could answer.** A
unit on a machine with no radio did everything reachable, deferred the rest, and
the step stayed open. **The plan has been re-cut so its steps can finish**, and
this unit's first job is to make the record say what actually happened.

Its second job is step A - **the last piece step B's menu needs before a
right-click can offer the operator anything.**

```
PHASE GOAL:   Hamlet works stations on the air.
UNIT GOAL:    The old steps are closed at what they reached, each unit appears
              once in the record, and every decoded row says where its contact
              stands.
ADVANCES:     steps 0 and A
```

---

## Verify this instruction against the tree

**Nothing here describes the tree.** Check every claim and report mismatches.
Report them; do not repair the instruction.

- `PHASE_PLAN.md` at the root is the **re-cut** send phase with steps 0, A, B, C,
  D, E, and `docs/phase-send-run/` holds the previous cut. **If the root plan
  still has numbered steps 0 to 6, `install-phase.bat` was not run** - say so
  first, skip task 4, carry on with the rest.
- **`docs/phase-send-run/PHASE_OUTCOME.md` records each unit twice** - `UNIT 262 -
  STEP 3` and `UNIT 5 - STEP 3` are the same unit with near-identical text. **Find
  where `outcome-append.bat` takes its unit number from.**
- **Unit 264 walked a whole exchange through the application** - a CQ heard, two
  right-click sends, a report and a sign-off, and the row read `complete, 0 slots`.
  **Find what of that survives in the tree before building anything.**
- The four row states this unit must show are **waiting on him**, **your move**,
  **complete**, **gone quiet**.
- Root version after work instruction 265, which pushed at `9fcfdf2`. **Read it;
  do not assume.**

Known reds, inherited, **never chased**:
`CwAdjudicationTests.ASpeedChangeInRealisticAudio`; the 51 CW cases in
`docs/unit239-failing-set.txt`; the `Ft8Sharp.Deep.Tests` whole-type-list
tripwire.

---

## Rulings in force

Transcribed from `PHASE_PLAN.md`. **Not to be re-argued.**

**A bench step's criteria are all satisfiable on a machine with no radio.**
**A bench step never defers a criterion to Tim.** If one turns out to need the
radio, **move it to step D or E and record the move** - do not leave it open.

**A step has at most four criteria.**

**A contact is never closed by the app.** `73` is politeness; complete means the
exchange has what a QSO needs. **Hamlet is not the radio police.**

**Nothing is forbidden.** Rows are never hidden or closed.

**One click, one transmission.** **The abort is not to be weakened, made
conditional, or routed around.**

**The dummy load is withdrawn in full.** Do not reference it, propose it, or add a
compensating control in its place.

**`Ft8Sharp` is a faithful MIT port and nothing changes a line of it.**

**§0.1** - the engine is not told that tabs exist. **§12.1** - nothing interprets
a message; a row's state is bookkeeping over which messages passed, not meaning.

---

## Status cadence

After each task, before starting the next, update `PROJECT_STATUS.md` per
`CLAUDE.md` - `STATE`, `TASK: n of m`, `BALL`, `UPDATED` from the clock, and
`NOTE` saying what is moving inside the task. The same every ten minutes while a
task is running. **Use the file-editing tools if the shell refuses.**

---

## Tasks

### Task 1 - the old steps close at what they reached

- In `docs/phase-send-run/PHASE_STATUS.md` and its `PHASE_OUTCOME.md` header,
  **record the old steps 2 and 3 as `done`**, with the figures that closed them:
  the loopback at 3 of 3 messages, the level at -12.04 dBFS, the device by unit
  256 and the rate by unit 262.
- **Say in the same file what was open in them and where it went** - the level
  Tim's own radio wants, which is now step D of the re-cut plan.
- **Do not alter any entry body.** The entries are the record of what units did;
  only the step-state header changes, and a line is added saying why.
- Report the old steps 4 and 5's states and what of their work survives, since
  step A and step B inherit it.

### Task 2 - each unit appears in the record once

- **Find where `outcome-append.bat` takes its unit number from** and why the same
  unit lands twice - once by its work-instruction number and once by the loop's
  iteration number.
- **Fix it**, so a unit appears once.
- **Leave the existing duplicates in the archive.** Rewriting history is worse
  than a labelled duplicate. **Name them in `docs/phase-send-run/PHASE_OUTCOME.md`
  so a reader knows to read them as one unit each.**
- Test, watched failing first: the same unit appended twice under both numbering
  routes produces one entry.

### Task 3 - the row knows where the contact stands

**This is the goal task.**

- **A per-station contact ledger** built from decoded slots: which messages passed
  each way and how many slots ago.
- **Four states, shown on the row**: **waiting on him** - you sent, nothing back;
  **your move** - he sent, a reply fits; **complete** - the exchange has what a
  QSO needs; **gone quiet** - nothing heard for a while. **Each with its slot
  count.**
- **Complete does not wait for `73`.** Both callsigns, both grids or reports, both
  acknowledgements. **A station that never signs off still made a contact.**
- **Nothing is closed, hidden or forbidden.** A complete contact still shows; a
  gone-quiet one still shows. **The app reports; it does not rule.**
- **A station working three others at once reads as gaps, not a fault.** Prove it
  against a recorded multi-slot scene - unit 264 composed one with Hamlet's own
  encoder and read it back with its own decoder. **Reuse that if it survives.**
- **Derived from recorded captures, not from the air.**

Tests, watched failing first, each run alone by exact name:

- a whole six-message exchange walks through the four states in order
- an exchange with **no `73` from either side** still reads complete
- a station interleaved with two others reads as gaps and never as a fault
- a station absent for several slots reads gone quiet **with the right count**

### Task 4 - the phase's bookkeeping

**File edits only. No shell needed.**

- `PROJECT_CARD.md` gains the `PHASE` and `PHASE_SET` from `PHASE_STATUS.md`'s
  header. **Changed only by ruling** (§13.3); the ruling is Tim's approval of
  `PHASE_PLAN.md` on 2026-09-07.
- Append that ruling to `DECISIONS.md`, next id in sequence, naming the re-cut and
  **why**: four steps sat partial for eight units because each held a criterion no
  bench machine could satisfy.
- Append this unit's entry to `PHASE_OUTCOME.md` through the **fixed**
  `outcome-append.bat`. **If the shell refuses, append with the file-editing tools
  in the format the existing entries use** and say so.

### Task 5 - what step B will need from this

**Named drop candidate.**

Given the ledger, **report for each of the four states which messages would be
valid to send** - not built, only named, with the rule that decides each. Step B
builds the menu from it and **the expected one is highlighted while none are
forbidden**, so this is the list it works from.

**Do not build the menu.** Do not add a send path. Name the mapping and stop.

---

## Parked - do not touch, do not raise

- **The CQ button and the right-click menu.** Step B.
- **The whole-chain dry run.** Step C.
- **The drive level and working a station.** Steps D and E, Tim's.
- **Automatic sequencing.** Out of this phase.
- **Anything in `src/Ft8Sharp/`.**
- The OSD re-encoding count, `ReusableWindow`, `ProcessDelayForTests`, the tap's
  owner, the waterfall's first row, unit 237's Extensible conclusion, work
  instruction 231's four tree items, `validate-output.bat`'s permitted-spellings
  bug, the 101.33 ms pulse above 6 kHz, the CW decoder and its inherited reds.

---

## What not to do

- **Do not build a send path, a menu, or a CQ button.**
- **Do not close a contact, hide a row, or forbid anything.**
- **Do not require `73` for complete.**
- **Do not rewrite archived entry bodies.**
- **Do not defer a criterion to Tim.** If one needs the radio, move it to step D
  or E and say so.
- **Do not reference a dummy load.**
- **Do not touch `src/Ft8Sharp/`.**
- **Do not run a test suite.** Only the test you just wrote, filtered,
  foregrounded, with a timeout.
- **Do not background a command and poll for it.**
- **Do not run `Hamlet.App.Tests`.**
- **Do not ship a placeholder token in a reported number.**

---

## Committing and pushing

Commit and push each task before starting the next. Bump the root version's patch
by one from whatever work instruction 265 left. **`Ft8Sharp` does not move.**

---

## Reporting

`output.md` at the repository root, overwritten, four sections per
`CLAUDE_CODE.md` §8 with the header block above section 1.

**Section 3 leads with three things:**

1. **A row's state, quoted**, at each of the four states, as the operator would
   read it off the screen.
2. **An exchange with no `73` reading complete**, quoted - the case the ruling
   exists for.
3. **The duplicate-entry fix**: where the second number came from, and the test
   that proves one entry.

**Section 2 says what changes on his screen** - every decoded row now says where
its contact stands, and nothing is hidden or closed.

Write `output.md`, then stop. Do not start the next unit.
