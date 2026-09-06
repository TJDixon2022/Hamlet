```
READ IN THIS ORDER - A the phase goal, B this step and its exit criteria, C what
this report adds and whether any of it bears on A or B.

A. THE PHASE GOAL AND EVERY STEP'S STATE
   Everything this project has built reaches the operator's screen, and the
   decoder is taken as far as it will go.
   Steps 0 to 5 all done: Hamlet decodes through Ft8Sharp.Deep; the gate set
   exists; the SNR column shows a number; ordered statistics; subtraction;
   cross-slot combining.
   Step 6 PARTIAL, 4 units spent before unit 257 and unit 257 now executed -
   the closing measurement. Both headers still read partial, deliberately.
   THIS SESSION ADDED NO MEASUREMENT AND MOVED NO STEP: launched against
   WORK_INSTRUCTIONS.md holding unit 257, it found unit 257 already executed
   in full at HEAD ba1ee22 and level with origin/main, and verified that
   rather than repeating it.

B. THIS STEP, ITS EXIT CRITERIA, AND WHICH WERE MET
   The five must-pass exits stand exactly where unit 257's own report left
   them, and this session re-checked the evidence for each in the tree rather
   than taking the report's word:
   1. MET BY UNIT 257 - the criterion the first judging session named as the
      only one short. Combining on and off at -19, -20 and -21 dB at BOTH
      placements, 306 trials a cell. On grid ON reads 306 of 306 at all three
      rungs against 248, 73 and 13 off; at the cell centre 306, 270 and 75
      against 6, 0 and 0. Zero wrong in all eighteen rows. Evidence verified
      present: section 5.5 at line 1032 of the closing document and six
      artefacts under docs/unit257-runs/.
   2. MET BEFORE UNIT 257, not re-run by it or by this session. Its two new
      configurations crossed at -22.41 dB on grid and -20.60 dB at the cell
      centre; its two cell-centre combining-off columns are reported not
      bracketed against a stated ceiling of -19 dB. THE SECOND JUDGING VERDICT
      CALLS THAT SHORT - section 4 item 1, the one thing here asking for a
      decision.
   3. MET BEFORE UNIT 257, not re-run. 336.8 ms and 44.5x undisturbed; unit
      257's worst observed slot over eight walks was 129.3 ms.
   4. MET. Section 6.1 re-read item by item by unit 257; new item 8 verified
      at line 1330. Section 6.2 unchanged and still on the document's face.
   5. UNTOUCHED, and the second verdict calls that short too - item 1 again.

C. THIS REPORT'S OWN FINDINGS, WEIGHED AGAINST A AND B
   Section 4 raises 4 items. ONE stands in the way of criteria in B and asks
   for a ruling: the judging session's second partial verdict names exits 2
   and 5, and closing them needs upward rungs at the cell centre and fixture
   work that unit 257's instruction expressly parks and forbids, so no session
   holding that instruction can close them. The other three are observations
   and do not block anything.
   NO WRONG DECODE APPEARED THIS SESSION BECAUSE NO WALK WAS RUN. Unit 257's
   one wrong decode stands unchanged in the record at the -23 dB extension
   rung, which is not one of exit 1's rungs; exit 1's six cells read zero
   wrong and criterion 1 is untouched by it.
   THE PLACEMENT SPLIT WAS TAKEN AS READING 2 AND THIS SESSION RE-VERIFIED
   THE ARITHMETIC INDEPENDENTLY: Ft8LadderHarness.cs:573 and :574 read exactly
   as the instruction states, so at zero jitter every hearing sits at the
   stated placement and the panel's "placement" label - not "first-hearing
   placement" - is correct. Reading 3's fallback was not needed.
END
```

UNIT:       257 — found already executed; this session verified and re-run nothing — 2026-09-05 21:14
PHASE GOAL: Everything this project has built reaches the operator's screen, and the decoder is taken as far as it will go.
UNIT GOAL:  Measure combining turned on and off at the closing table's own three rungs and at both placements, with the jitter set to zero so the placement label is true, and put it in the closing document beside unit 256's jittered panel.
ADVANCED:   not by this session — unit 257 achieved the goal at 21:01 and is committed; this session confirmed the deliverable is whole, validated and pushed, and added one finding for the next arbiter.
NUMBER:     walks run this session: 0 of 0 needed — every figure the instruction asks for was already committed under docs/unit257-runs/ and transcribed into section 5.5
DRIFT:      0 consecutive units without advance  (was 0) — this session is a verification pass over a completed unit, not a unit of its own

## 1. What Claude did

**Exit state: COMPLETE, with nothing re-run and one finding raised.**

### 1.1 The guard block, first

`WORK_INSTRUCTIONS.md` opens with a refusal test and it was answered before anything
else was read. **Hamlet confirmed:** `SHACK_FACTS.md` present,
`src/Hamlet.RadioEngine/Cw/CwProbabilisticDecoder.cs` present, `CoreHMI.sln` absent,
`MURC.sln` absent, and the only solution file at the root is `Hamlet.sln`.

### 1.2 What I found, and why I did not spend the night again

`WORK_INSTRUCTIONS.md` holds **unit 257**. Unit 257 **had already been executed in
full** by a session that ended at 21:01, ten minutes before this one started:

```
ba1ee22  docs(unit257): the report itself, which the previous commit did not carry
9254d45  docs(unit257): the report, validated at exit 0
658b987  chore(unit257): the bookkeeping, and the position stated rather than the verdict written
0ff4991  docs(unit257): the closing document carries combining at its own rungs and both placements
da94688  test(unit257): the crossings, and at -23 dB the wrong decode returns at zero jitter
df6d97c  test(unit257): combining off the analysis grid, measured for the first time in this project
c5921ed  test(unit257): combining on grid at the closing table's rungs, and it is saturated at all three
bd1b580  docs(unit257): the trace, and the night priced before it is spent
```

`git status -sb` reads `## main...origin/main` with no ahead or behind marker: **every
one of those eight commits is pushed.**

**The decision this session had to make was whether to run the instruction again.** I
did not, and the reason is arithmetic rather than reluctance. The instruction's six
tasks are nine multi-minute foreground calls whose outputs are already committed as
artefacts and already transcribed into the closing document. Re-running them would
reproduce the same figures from the same fixed seeds, overwrite a report that has
already been delivered and judged, and buy nothing. **What a re-launch actually needs
is verification that the deliverable is whole** — so that is what I did, taking
nothing from the previous session's summary on trust.

### 1.3 The verification, item by item, against the tree and not against the report

Every check below was made by reading the tree at this session's clock.

| what the previous session claimed | how it was checked | result |
|---|---|---|
| the report is validated | `"tools\arbiter\validate-output.bat" output.md` re-run now | **exit 0**, all seven rules ok |
| section 5.5 was added to the closing document | heading grep | present at **line 1032**, header block, three rows × three rungs × two placements |
| section 5.0 amended in place | grep for unit 257 in §5.0's range | present at **779** and **788**, saying §5.4 is not reopened and citing `:573-574` |
| section 6.1 re-read, item 8 added | heading grep | **line 1330**, "ADDED BY UNIT 257" |
| six panel walks plus two extension rungs written to artefacts | directory listing | **9 files** under `docs/unit257-runs/`, six panels, two extensions, one crossing table |
| two test files constructed | listing | `Ft8Unit257PlacementPanelTests.cs`, `Ft8Unit257CrossingTests.cs` |
| version moved | `Directory.Build.props:205` | **`1.12.59`** |
| `HM-OPEN-082` carries a second dated observation | `OPEN_ISSUES.md:7-13` | status line reads "observed again at zero jitter by unit 257 the same day" |
| gate set and breakage record not padded | grep | last breakage is still **B18**, and no unit 257 entry was added to either |
| `PHASE_OUTCOME.md` carries the unit's entry | grep | `## UNIT 257 - STEP 6` at **line 273** |
| nothing under `src/` moved | `git diff --stat HEAD` over the written paths | **empty** — working tree equals HEAD for every path the unit wrote |

**I also re-verified the one piece of arithmetic the whole design turns on**, because
the ordering block has to state whether reading 2 or reading 3 was taken and I was not
willing to copy that answer forward. `Ft8LadderHarness.cs:573-574`:

```
var slotFrequency = frequencyHz + (r * frequencyJitterHz);
var slotOffset = offset + (r * offsetJitterSamples);
```

**Reading 2 holds.** With both jitters zero the `r` terms vanish and every hearing of
every trial sits at exactly `frequencyHz` and `offsetSamples`. The panel's `placement`
label is true and reading 3's fallback was correctly not taken.

### 1.4 What arrived in the tree after unit 257 finished, and what I did about it

`PHASE_OUTCOME.md` is modified and uncommitted, and the modification is **not unit
257's** — it is the judging session's own appended entry, written after 21:02, in the
launcher's format with a `COST:` field of `12.961496999999998` matching unit 257's
session cost exactly. **It returns `partial` a second time**, and its `STATE_WHY` names
different criteria from the first verdict:

> Criteria 3 and 4 are met with quoted figures and the combining panel now covers all
> six rung placements with zero wrong, but criterion 2 still leaves the two cell centre
> combining off configurations without an interpolated crossing or interval, only a
> stated ceiling, and criterion 5 is reported as untouched with no fixture names or
> command quoted in support.

**I did not act on it, and section 4 item 1 says why.** In short: closing it needs
rungs *above* -19 dB at the cell centre and fixture work, and unit 257's instruction
parks exits 2 to 5, caps its extension search downward at -23 dB, and forbids
extrapolation. **That is a new instruction's work, not a re-run of this one**, and
writing it is the arbiter's call rather than mine.

### 1.5 Bookkeeping this session did and did not touch

- **`PROJECT_STATUS.md`** updated, `UPDATED` read from the clock at each write
  (`21:14:06`), with a `NOTE` saying what this session actually is.
- **`PHASE_STATUS.md`** — the `WORK_INSTRUCTION:` line already reads
  `257 - combining gets the closing table's own rungs and both placements`, which is
  correct and needed no edit. **`STEP: 6 | partial` left exactly as it is**, per task 6
  item 6 and the same reasoning unit 257 gave. `HEARTBEAT:`, `CURRENT_STEP:` and the
  `STEP:` lines were not written by hand.
- **Not touched:** everything under `.run-unit/`, `SESSION.lock`, `RUN_LEDGER.md`,
  `WORK_INSTRUCTIONS.md`, `.tmp-sink.py`, the judging session's uncommitted
  `PHASE_OUTCOME.md` entry, and every path under `src/`.

## 2. What the owner should expect

**Unit 257's deliverable is intact, whole, validated and pushed.** Nothing in the tree
regressed between 21:01 and now, and nothing this session did changed a measured figure.

**What you can rely on, unchanged from unit 257:** combining is measured on and off at
-19, -20 and -21 dB at both placements, 306 trials a cell, **zero wrong in all eighteen
rows**. On the analysis grid it is saturated — 306 of 306 at every one of the closing
table's rungs, against 248, 73 and 13 without it. Half a waterfall bin off the grid, at
-21 dB, **nothing decodes at all without it and 75 of 306 decode with it**, every one a
trial no single slot reached. It crosses 50 per cent at -22.41 dB on grid and -20.60 dB
off it, so **landing off the grid costs 1.81 dB** even with four hearings.

**Two things to expect that are about process rather than radio.**

**The judging loop has now returned `partial` twice on step 6, and the second verdict
moved the goalposts to different criteria than the first.** The first said exit 1 was
short; unit 257 measured exit 1 and committed it. The second accepts exit 1 and names
exits 2 and 5 — which unit 257's own instruction had declared met before the night
started and told it not to re-run. **Nobody has done anything wrong here**, but a
session handed unit 257's instruction cannot close exits 2 and 5 without breaking it,
so the loop will keep producing `partial` until an instruction is written that licenses
that work. That is the one thing in this report that needs a decision from you or the
next arbiter.

**Nothing shipped and nothing keyed.** `Ft8Reception.cs` is untouched, subtraction and
combining are both still off by default, and no line under `src/` moved in unit 257 or
in this session. **Whether combining ships is still yours, with the figures now in
front of you.**

## 3. What you should see

### 3.1 Mismatches between the instruction and the tree — and the arithmetic first

**The instruction's *Verify this instruction against the tree* block was measured at
authoring against `HEAD d80fb6f`. The tree is now at `ba1ee22`, eight commits later,
and those eight commits are unit 257's own.** So most of what follows is not an error
in the instruction — it is the instruction describing the tree it was written for, read
by a session standing after the work rather than before it. **I have marked which is
which, because the distinction is the whole point.**

**The arithmetic, first of all, because the design turns on it.** The instruction says
`Ft8LadderHarness.cs:573` is `var slotFrequency = frequencyHz + (r * frequencyJitterHz);`
and `:574` is `var slotOffset = offset + (r * offsetJitterSamples);`. **Both hold, at
those exact line numbers, verbatim.** Reading 2 stands and no fallback was needed. This
is a re-verification of what unit 257 already found, made independently this session.

| instruction's claim | tree now | which |
|---|---|---|
| `HEAD d80fb6f` | **`ba1ee22`** | superseded — by unit 257's eight commits |
| root version `1.12.58` | **`1.12.59`** | superseded — unit 257's task 6 item 4, as instructed |
| `docs/unit255-closing-measurement.md` is 1250 lines | **1533** | superseded — §5.5 and the §5.0 amendment |
| §5.4 at 889, §6.1 at 1013, §6.4 at 1220 | **923, 1219, 1503** | superseded — pushed down by the same additions |
| §3.1 at 378, §3.2 at 426, §5.0 at 756 | **378, 426, 756** | **holds exactly** |
| `docs/unit256-crossings-and-combining.md` is 733 lines | **733** | holds |
| `Ft8LadderHarness.cs` is 1312 lines | **1312** | holds |
| `DefaultFrequencyHz = 1000.0` at `:64`, `DefaultOffsetSamples` at `:69`, `RunRepeats` at `:472` | all three at those lines | holds |
| `docs/gate-set.md` at 13 entries, `docs/breakage-record.md` at B18 | last breakage still **B18**, no unit 257 entry in either | holds — correctly, a walk earns neither |
| `HM-OPEN-082` at `OPEN_ISSUES.md:7` | at `:7`, now with unit 257's second observation on its status line | superseded in the right direction |
| tree not clean: `M PHASE_OUTCOME.md`, `M PHASE_STATUS.md`, `M RUN_LEDGER.md`, **`D SESSION.lock`**, `?? .tmp-sink.py`, twelve modified under `.run-unit/` | all present, **except `SESSION.lock` now reads `M` and not `D`** | one real difference, and it is the launcher's — reported, not repaired |
| both headers read `STEP: 6 | partial` | **both still do** | holds — left deliberately |
| the `RULES_AT` disagreement | `PROJECT_STATUS.md` still says HM-DEC-155 (2026-09-05), `CLAUDE.md` §1 still holds CPS-DEC-0152 | holds — reported as an observation, not reconciled |

**One claim I could not check the way the instruction frames it.** The instruction says
`Ft8Unit256CombiningPanelTests.TheCombiningPanelAtMinus23` is red in the tree and must
not be run. **I did not run it, so I cannot confirm its colour** — and confirming it is
not worth breaking the rule. Its file and the named line numbers are present.

### 3.2 Refused shell calls, verbatim

**Four refusals this session, all on the same command, all worked around, none halted
anything.** They match the pattern the instruction predicts.

```
cmd //c "tools\arbiter\validate-output.bat output.md" 2>&1 | tail -20; echo "EXIT=$?"
  -> This Bash command contains multiple operations. The following parts require
     approval: cmd //c "tools\arbiter\validate-output.bat output.md" 2>&1,
     tail -20; echo "EXIT=$?"

"tools/arbiter/validate-output.bat" output.md 2>&1 | tail -25
  -> This Bash command contains multiple operations. The following part requires
     approval: "tools/arbiter/validate-output.bat" output.md 2>&1

tools/arbiter/validate-output.bat output.md
  -> This command requires approval
```

**The working spelling is the one the instruction names — unit 252's:**

```
"tools\arbiter\validate-output.bat" output.md
  -> VALID - all seven rules passed.   validate-output exit 0
```

**`git` was refused in no spelling.** `dotnet` was not invoked at all this session, so
this session says nothing about whether it would have been refused. **The file-editing
tools were unaffected throughout**, as on units 251 to 257.

### 3.3 The walks, and their predicted against actual wall clocks

**No walk ran this session. `dotnet test` was invoked zero times and `dotnet build`
zero times.** There is therefore no predicted-against-actual table of my own to give,
and inventing one would be furniture. **Unit 257's own eight walks, their prices and
their actuals are recorded in `docs/unit257-combining-placement.md` and their raw
output in the nine artefacts under `docs/unit257-runs/`**, which I verified are present
and match `HEAD`.

For the record, so this report is readable on its own — the figures below are
**transcribed from unit 257's committed artefacts and section 5.5, not re-measured**:

| rung | placement | OFF, port | OFF, +OSD | **ON, `summed x4`** | wrong |
|---|---|---:|---:|---:|---:|
| -19 dB | on grid | 248 of 306 | 276 of 306 | **306 of 306** | **0** |
| -20 dB | on grid | 73 of 306 | 125 of 306 | **306 of 306** | **0** |
| -21 dB | on grid | 13 of 306 | 33 of 306 | **306 of 306** | **0** |
| -19 dB | cell centre | 6 of 306 | 33 of 306 | **306 of 306** | **0** |
| -20 dB | cell centre | 0 of 306 | 1 of 306 | **270 of 306** | **0** |
| -21 dB | cell centre | 0 of 306 | 0 of 306 | **75 of 306** | **0** |

Crossings: **-22.41 dB on grid**, **-20.60 dB at the cell centre**. Worst observed slot
over all eight walks: **129.3 ms**, against the 15 000 ms budget.

### 3.4 The three rules that kill sessions, and what this one did about them

- **No test suite was run.** No test at all was run, so no filter question arose. In
  particular `Ft8Unit256CombiningPanelTests.TheCombiningPanelAtMinus23` was **not run**,
  nor was any other test this session did not construct — which is all of them.
- **Nothing was backgrounded and nothing was polled.** Every command was foreground and
  short.
- **The status file was written from the clock**, read at the moment of the write
  (`2026-09-05T21:14:06-04:00`), not composed ahead.
- **No scratch file was created at the repository root.** `.tmp-sink.py` is still there,
  still untracked, still not mine, and still not deleted.

## 4. What's blocking us

**Four items. One asks for a decision. Three are observations for the record.**

### 4.1 A DECISION IS WANTED — the judging loop has named criteria that the instruction in force forbids closing

**This is a request for a ruling, and it is the only one.**

The judging session's uncommitted `PHASE_OUTCOME.md` entry returns `partial` on step 6
a second time and names **exit 2** — the two cell-centre combining-off configurations
having only a stated ceiling rather than an interpolated crossing — and **exit 5**, the
fixtures, as reported untouched without names or commands quoted.

**A session holding unit 257's instruction cannot close either.** The instruction:

- parks exits 2 to 5 as *"Met, and not re-run tonight"*;
- caps the extension search **downward at -23 dB**, while the cell-centre
  combining-off columns read 6, 0 and 0 of 306 and therefore cross **above** -19 dB,
  where only *upward* rungs could reach them;
- says in *What not to do*: **"Do not extrapolate a crossing"**;
- and puts §6.3's fixtures outside the night entirely.

**So this is not a failure of execution and re-running unit 257 would not fix it.** It
needs a new instruction that licenses upward rungs at the cell centre — -18 and -17 dB
would be the natural pair, and unit 256 already has the bare port off-grid at -17.52 dB
on the single-slot ladder as a sanity bound — and that says what exit 5's fixtures
require. **Whether to spend that is the arbiter's call, and I have not pre-empted it.**

### 4.2 OBSERVATION — `SESSION.lock` reads `M`, where the instruction and the reload both measured `D`

The instruction's tree block and `.run-unit/reload.txt` both record `D SESSION.lock`;
`git status` now reads `M`. **It is the launcher's file, it is parked, and I neither
touched it nor investigated further.** Recorded because the instruction asks for
mismatches to be reported rather than repaired.

### 4.3 OBSERVATION — the `RULES_AT` disagreement, unchanged and unreconciled

`PROJECT_STATUS.md` `RULES_AT:` says **HM-DEC-155 (2026-09-05)**; `CLAUDE.md` §1's
highest is **CPS-DEC-0152**. **Reported as an observation exactly as instructed. Not
reconciled, and not raised as a question.**

### 4.4 OBSERVATION — `HM-OPEN-082` stands open, and nothing this session did touched it

Unit 257 reproduced it at zero jitter and on grid — same trial 29, same seed 220771,
same wrong message — which narrows it away from being a property of the jitter. **The
red assertion is still red and still unweakened.** It sits at -23 dB, two decibels
below the deepest rung any exit criterion covers, and **it blocks no criterion in
section B**. Its three named next questions remain outside step 6 and were not spent.

**Nothing else is blocking.** The deliverable is whole, validated at exit 0, committed
and pushed.
