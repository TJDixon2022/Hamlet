# Work instruction 253 - the dummy load is gone, and the abort is watched to fire

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

## THE TWO RULES THAT KILLED THREE SESSIONS

**Tim's rulings of 2026-09-05. Not this unit's to weigh.**

**1. A unit runs no test suite.** **A unit may run only the unit test it
constructs in that work instruction**, filtered by exact name, in the foreground,
with a stated timeout of a few minutes. **An unfiltered `dotnet test` on any
project is forbidden.**

**2. Never background a command and poll for it.** Three sessions were killed by
the watchdog on 2026-09-05, at 33 to 38 minutes, each sitting in
`until grep -q "exited with code" ...; do sleep 15; done` with a `900000` ms
timeout. **The watchdog fires after twelve minutes with no status write.**

`dotnet build` is allowed, foregrounded, with a timeout.

---

## THE TOOL RULE

**This session's shell may refuse calls.** A refused shell call is a signal to
reach for the other tool, not to stop. **The file-editing tools have been
unaffected throughout.** Record every refusal verbatim. **Nothing in this unit
halts the loop.**

**One tool fact unit 252 paid for twice:** this shell collapses a doubled
backslash inside a quoted heredoc, so a Python string written there loses its
escaping. Use the file-editing tools for anything with escapes in it.

---

## Why this unit exists

**Hamlet reads FT8 as well as anything and cannot answer.** Fourteen messages
from one slot on 14.074, five continents, down to -21 dB. Cross-slot combining
reads 252 of 306 at -21 dB against the port's 13, zero wrong. **The operator
watches the band and cannot say a word into it.**

**This is the first unit of the transmit phase and it builds nothing that
transmits.** It does two things, in order: it takes a withdrawn rule out of the
tree so no later session halts on it, and it proves the abort **before any code
exists that could need one.**

```
PHASE GOAL:   Hamlet works stations on the air.
UNIT GOAL:    No dummy-load requirement survives in a live document, and a
              same-thread no-await abort has been watched to fire from every
              state, with nothing yet built that can key a transmitter.
ADVANCES:     steps 0 and 1
```

---

## Verify this instruction against the tree

**Nothing here describes the tree.** Check every claim and report mismatches.
Report them; do not repair the instruction.

- `CLAUDE.md` §0.2 currently opens *Development transmit testing is into a dummy
  load, not an antenna, until the feature is proven (HM-DEC-008)*, and carries a
  paragraph citing HM-DEC-098 about an automated cycle exercised into a dummy load
  only. **The replacement text is delivered beside this instruction as
  `CLAUDE_0_2_REPLACEMENT.md`.**
- §0.2.1 on tuning writes follows it and is **not** this unit's to touch.
- `src/Hamlet.RadioEngine/Civ/` holds `CivWrites.cs`, `CivFrame.cs`,
  `CivConstants.cs` and the transport. **Find what already exists for PTT and for
  CI-V `0x17` before writing anything.**
- `AppSettings` carries the operator's callsign `KC3QIS`, grid `FN00DJ`, licence
  class General, and a toggle *Only let me transmit where my license allows*,
  currently On, whose own text says *There is no transmit path yet*.
- Root version after work instruction 252. **Read it; do not assume.**

Known reds, inherited, **never chased**:
`CwAdjudicationTests.ASpeedChangeInRealisticAudio`; the 51 CW cases in
`docs/unit239-failing-set.txt`; the `Ft8Sharp.Deep.Tests` whole-type-list
tripwire.

---

## Rulings in force

**Tim's, 2026-09-06:**

- **Tim operates a licensed station on an antenna and Hamlet transmits on the
  air.** **HM-DEC-008 and HM-DEC-098 are withdrawn in full.** The dummy load is
  **not a stage, not a fallback, and not to be referenced.** **Do not propose it,
  do not treat its absence as a risk to be mitigated, and do not add a
  compensating control in its place.**
- **One click, one message.** Hamlet transmits because the operator clicked, and
  for no other reason.
- **Right-click sends immediately, in the next slot, with no confirmation.**

**Standing, and unaffected by the above:**

- **Every path that keys the transmitter has a same-thread, no-await abort** -
  CI-V `0x17` with `0xFF`, PTT off as the fallback.
- **Hamlet never transmits outside the operator's licence privileges.**
- **§0.0.** Never present a guess as a decode.
- **§0.1.** The engine is not told that tabs exist.
- **`Ft8Sharp` is a faithful MIT port and nothing changes a line of it.**

---

## Status cadence

After each task, before starting the next, update `PROJECT_STATUS.md` per
`CLAUDE.md` - `STATE`, `TASK: n of m`, `BALL`, `UPDATED` from the clock, and
`NOTE` saying what is moving inside the task. The same every ten minutes while a
task is running. **Use the file-editing tools if the shell refuses.**

---

## Tasks

### Task 1 - §0.2 is rewritten

- Replace `CLAUDE.md` §0.2 with the text in `CLAUDE_0_2_REPLACEMENT.md`, exactly
  as delivered. **§0.2.1 and everything after it is untouched.**
- Append a `DECISIONS.md` entry, next id in sequence, **in Tim's name, dated
  2026-09-06**, superseding HM-DEC-008 and HM-DEC-098, recording his reasoning:
  a licensed operator, on bands he is licensed for, with a live antenna.
- **The abort requirement and the one-click rule survive verbatim.** Check the
  replacement text carries both and say so.

### Task 2 - every other trace of it, found

- **Search the whole tree** for the dummy load and for HM-DEC-008 and HM-DEC-098.
  Report every hit with file and line.
- **Correct live documents** - `ARBITER.md`, `PHASE_CONTROL.md`,
  `ft8sharp-spec.md`, `CLAUDE_CODE.md`, anything a session reads.
- **Leave archived phase records alone and name them.** `docs/phase-*-run/` and
  old `output.md` copies are history. **Rewriting history is worse than a stale
  archive**, and a reader who finds the old rule there will also find the
  superseding decision in `DECISIONS.md`.
- Say in the report how many hits were live and how many archived.

### Task 3 - what already exists for keying

**Reading only.**

- Every place `Hamlet.RadioEngine` can key or unkey a transmitter today, with
  file and line. CI-V `0x1C 00`, `0x17`, PTT, anything in `CivWrites`.
- **What the CW side already does about aborting**, if anything.
- The transport's behaviour when the port is gone, the radio does not answer, or
  a write throws.
- The licence gate's current shape and where a send path would have to pass
  through it.
- **Say what is missing** for step 1's abort, rather than assuming it is nothing.

### Task 4 - the abort, built and watched to fire

**This is the goal task.**

- A same-thread, **no-await** abort: CI-V `0x17` with `0xFF`, **PTT off as the
  fallback**, both attempted, neither depending on the other succeeding.
- **No `await` anywhere on the abort path**, asserted by a test that would fail if
  one were added.
- **It cannot be disabled, deferred, or made conditional.** No flag turns it off.
- Tests, watched failing first, against a fake CI-V transport:
  - fires from **about to key**, **keying**, **mid-transmission** and **waiting to
    unkey**
  - fires when **the transport is dead**, when **the port is gone**, and when
    **the radio does not answer** - the fallback exercised, not reasoned about
  - fires when the CI-V write itself **throws**
  - **completes without awaiting anything**, and within a stated time bound
- **Nothing in this unit can key a transmitter.** The abort is built against a
  fake transport and there is no caller. Say so plainly in the report.

### Task 5 - the record

**File edits only.**

- `PROJECT_CARD.md` gains the new `PHASE` and `PHASE_SET` from `PHASE_STATUS.md`.
  **Changed only by ruling** (§13.3); the ruling is Tim's approval of
  `PHASE_PLAN.md` on 2026-09-06.
- Append that ruling to `DECISIONS.md`.
- Append this unit's entry to `PHASE_OUTCOME.md` through
  `tools\arbiter\outcome-append.bat`. **If the shell refuses, append with the
  file-editing tools in the format the existing entries use** and say so.

---

## Parked - do not touch, do not raise

- **Generating FT8 audio.** Step 2.
- **Playing audio to the radio, keying it, the loopback.** Step 3.
- **Contact state and the four row states.** Step 4.
- **The right-click menu and the CQ button.** Step 5.
- **Automatic sequencing.** Deliberately out of this phase.
- **Logging**, FT4/PSK31/WSPR transmit, CW send.
- **Anything in `src/Ft8Sharp/`.**
- The OSD re-encoding count, `ReusableWindow`, `ProcessDelayForTests`, the tap's
  owner, the waterfall's first row, unit 237's Extensible conclusion, work
  instruction 231's four tree items, `validate-output.bat`'s permitted-spellings
  bug, the 101.33 ms pulse above 6 kHz, the CW decoder and its inherited reds.

---

## What not to do

- **Do not reference, propose, or design around a dummy load.** It is withdrawn.
- **Do not add a compensating control in its place.** No confirmation dialog, no
  power limit, no test mode. Tim ruled the transmit path goes on the air.
- **Do not write anything that can key a transmitter in this unit.**
- **Do not put an `await` on the abort path.**
- **Do not rewrite archived phase records.**
- **Do not run a test suite.** Only the test you just wrote, filtered,
  foregrounded, with a timeout.
- **Do not background a command and poll for it.**
- **Do not touch `src/Ft8Sharp/`** or `CLAUDE.md` §0.2.1.
- **Do not run `Hamlet.App.Tests`.**
- **Do not ship a placeholder token in a reported number.**

---

## Committing and pushing

Commit and push each task before starting the next. Bump the root version's patch
by one from whatever work instruction 252 left. **`Ft8Sharp` does not move.**

---

## Reporting

`output.md` at the repository root, overwritten, four sections per
`CLAUDE_CODE.md` §8 with the header block above section 1.

**Section 3 leads with three things:**

1. **The dummy load's hit count** - how many live documents carried it, how many
   archives were left alone, and confirmation that `CLAUDE.md` carries none.
2. **The abort's test list**, each state it was watched to fire from, and the
   time bound it completed within.
3. **Confirmation that nothing in this unit can key a transmitter**, and what step
   2 must build before anything can.

Write `output.md`, then stop. Do not start the next unit.
