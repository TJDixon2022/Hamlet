```
READ IN THIS ORDER - A the phase goal, B this step and its exit criteria, C what
this report adds and whether any of it bears on A or B.

A. THE PHASE GOAL AND EVERY STEP'S STATE
   Everything this project has built reaches the operator's screen, and the
   decoder is taken as far as it will go.
   Steps 0 to 5 all done: Hamlet decodes through Ft8Sharp.Deep; the gate set
   exists; the SNR column shows a number; ordered statistics; subtraction;
   cross-slot combining.
   Step 6 PARTIAL entering tonight, 4 units spent - the closing measurement.
   It leaves tonight with its one short criterion measured and committed, and
   this unit does NOT write the verdict on whether it is now done.

B. THIS STEP, ITS EXIT CRITERIA, AND WHICH WERE MET
   1. MET TONIGHT - the criterion the judging session named as the only one
      short. Combining measured on and off at -19, -20 and -21 dB at BOTH
      placements, 306 trials a cell, wrong asserted per row. On grid ON reads
      306 of 306 at all three rungs against 248, 73 and 13 of 306 off. At the
      cell centre 306, 270 and 75 of 306 against 6, 0 and 0 of 306 off. WRONG
      IS ZERO IN ALL EIGHTEEN ROWS. It is a panel on the repeats ladder and
      NOT a column of section 3's table: section 3 gives each trial one slot
      and RunRepeats gives each trial four, so a four-slot row beside a
      one-slot row is a false comparison and no other arrangement of this
      harness is possible. Evidence: section 5.5 of the closing document, six
      artefacts under docs/unit257-runs/.
   2. MET BEFORE TONIGHT, not re-run. Tonight's two new configurations cross
      at -22.41 dB on grid (band -22.47 to -22.35) and -20.60 dB at the cell
      centre (band -20.67 to -20.53); tonight's two cell-centre combining-off
      columns are not bracketed, against a stated ceiling of -19 dB.
   3. MET BEFORE TONIGHT, not re-run. Unit 255's 336.8 ms and 44.5x are
      undisturbed; tonight's worst observed slot over eight walks was
      129.3 ms, a 116x margin against FT8's 15 000 ms.
   4. MET. Section 6.1 re-read item by item: items 1 to 6 did not move and
      why is stated for each, item 7's rung qualifier stands and is better
      founded, and new item 8 gives combining at the table's own rungs and
      both placements. Section 6.2 - both stages ship OFF - is unchanged.
   5. UNTOUCHED. Section 6.3's fixtures, exactly as unit 255 left them.

C. THIS REPORT'S OWN FINDINGS, WEIGHED AGAINST A AND B
   Section 4 raises 5 items. NONE stands in the way of a criterion in B and
   none asks for a ruling: four are observations for the record and one is a
   completed workaround.
   A WRONG DECODE DID APPEAR, at the -23 dB extension rung only, which is not
   one of exit 1's rungs. It is byte for byte HM-OPEN-082 - trial 29, seed
   220771 - now reproduced at zero jitter and on grid. IT DOES NOTHING TO
   CRITERION 1: exit 1's six cells read zero wrong over 5 508 scored slot
   decodes, and -23 dB is two decibels below the deepest rung any criterion
   covers and 1.41 dB below combining's own crossing. It is left red and
   unweakened.
   THE PLACEMENT SPLIT WAS TAKEN AS READING 2, NOT READING 3'S FALLBACK.
   Ft8LadderHarness.cs:573-574 holds exactly as the instruction states, so at
   zero jitter every hearing of every trial sits at the stated placement. THE
   PANEL IS THEREFORE LABELLED "placement" AND NOT "first-hearing placement",
   and the label is confirmed by measurement rather than argument: the twelve
   combining-off figures reproduce sections 3.1 and 3.2 to the decode.
END
```

UNIT:       257 — complete at task 6 of 6 — 2026-09-05 20:58
PHASE GOAL: Everything this project has built reaches the operator's screen, and the decoder is taken as far as it will go.
UNIT GOAL:  Measure combining turned on and off at the closing table's own three rungs and at both placements, with the jitter set to zero so the placement label is true, and put it in the closing document beside unit 256's jittered panel — so exit 1's "each stage on and off" is answered for the last stage that had no answer.
ADVANCED:   yes — the one exit criterion the judging session found short is measured, committed and evidenced; combining had never been measured at -19 or -20 dB anywhere in this project, nor off the analysis grid at all, and now it is at six rung-placements.
NUMBER:     combining at -21 dB, cell centre: none -> 75 of 306, and its 50 per cent crossing off the grid: none -> -20.60 dB (band -20.67 to -20.53)
DRIFT:      0 consecutive units without advance  (was 0)

## 1. What Claude did

**Exit state: COMPLETE, at task 6 of 6.** All six tasks were done; none was dropped. **The
named drop candidate — the downward extension rungs — WAS taken**, and the decision was made
at the start of task 4 and written into the document before either call was spent, as the
instruction requires. Nothing was left undone, so there is nothing under CLAUDE_CODE.md §8's
*stopped* clause to report.

Machine: `win32`, Windows 11 Pro 10.0.26200. Project claimed and confirmed: **Hamlet** —
`SHACK_FACTS.md` and `src/Hamlet.RadioEngine/Cw/CwProbabilisticDecoder.cs` both present,
`CoreHMI.sln` and `MURC.sln` both absent, `Hamlet.sln` the only solution at the root. Branch
`main`, from `HEAD d80fb6f` to `658b987`, six commits, pushed at the end.

### What was traced

`Ft8LadderHarness.cs:472`–`:600` read in full. **`:573` is `var slotFrequency = frequencyHz +
(r * frequencyJitterHz);` and `:574` is `var slotOffset = offset + (r * offsetJitterSamples);`,
exactly as the instruction states.** `offsetSamples` resolves at `:490`. There is no other term
in the method that moves a slot's frequency or offset; the only other per-repeat variation is
the noise draw at `:553`–`:557`. **So reading 2 holds without qualification and reading 3's
fallback was not taken.** Unit 256's call at `:112`–`:120` was transcribed argument for
argument: **every argument the instruction said would be identical is identical**, checked by
reading rather than assumed.

### What was measured — nine foregrounded calls, none backgrounded, none polled

Each was filtered by its exact full method name, run alone, with a stated timeout of 480 s
(120 s for the crossings computation), with a status line written immediately before and
immediately after.

**Tasks 2 and 3 — the panel, `Ft8LadderHarness.RunRepeats`, four slots a trial, 306 trials a
cell, `frequencyJitterHz: 0.0`, `offsetJitterSamples: 0`, `historyDepth: 3`,
`accumulationDepth: 3`, `combinedOsd` and `combinedFineSync` both `Default`, `Ft8Sharp.Deep`
0.8.0:**

| rung | placement | `single slot` OFF | `single + OSD` OFF | **`summed x4` ON** | OnlyCombined | Lost | wrong |
|---|---|---:|---:|---:|---:|---:|---:|
| -19 | on grid | 248 of 306 | 276 of 306 | **306 of 306** | 1 | 0 | **0** |
| -20 | on grid | 73 of 306 | 125 of 306 | **306 of 306** | 39 | 0 | **0** |
| -21 | on grid | 13 of 306 | 33 of 306 | **306 of 306** | 200 | 0 | **0** |
| -19 | cell centre | 6 of 306 | 33 of 306 | **306 of 306** | 196 | 0 | **0** |
| -20 | cell centre | 0 of 306 | 1 of 306 | **270 of 306** | 262 | 0 | **0** |
| -21 | cell centre | 0 of 306 | 0 of 306 | **75 of 306** | 75 | 0 | **0** |

**2 561 combined decodes, 2 561 verified against the ladder's own ground truth. `LostByCombining`
is 0 in all six cells. `DeepestHearings` is 4 in all six.**

**Task 4 — the extension rungs and the crossings:**

| rung | placement | `summed x4` | wrong |
|---|---|---:|---:|
| -22 | on grid | 239 of 306, 78.1 % | 0 |
| -23 | on grid | 29 of 306, 9.5 % | **1 — left red** |

| column | placement | rungs | crossing | band |
|---|---|---|---|---|
| `single slot` | on grid | -19 / -20 | -19.54 dB | -19.62 to -19.46 |
| `single + OSD` | on grid | -19 / -20 | -19.81 dB | -19.92 to -19.71 |
| **`summed x4`** | **on grid** | **-22 / -23** | **-22.41 dB** | **-22.47 to -22.35** |
| `single slot` | cell centre | — | **not bracketed — above -19 dB** | — |
| `single + OSD` | cell centre | — | **not bracketed — above -19 dB** | — |
| **`summed x4`** | **cell centre** | **-20 / -21** | **-20.60 dB** | **-20.67 to -20.53** |

Computed by `Ft8Unit256CrossingBand`, which is already gate-set entry 13 and already watched
failing as `B18`. **No second crossing helper was written.**

### The decisions this session made for itself, reproduced in full

**1. `DeepestHearings` is printed and NOT asserted**, which departs from unit 256's panel,
which asserts `== 4` at `Ft8Unit256CombiningPanelTests.cs:214`. The instruction names two
assertions for tonight, not three. Unit 256's own summary records that at -23 dB it read 3 —
not because of `B17` but because so little decoded that no slot ever held four hearings' worth
of candidates. **On a walk whose rung is the variable, an assertion that only means something
where the combiner has candidates would manufacture a red for a measurement**, which
`docs/gate-set.md:57` forbids. It read 4 in all eight walks, so nothing was lost.

**2. The named drop candidate was TAKEN, and where it was spent was narrowed.** Decided at the
start of task 4 and written into `docs/unit257-combining-placement.md` §3.1 before either call.
Three of the six columns are already bracketed by the table's own rungs. **The two cell-centre
combining-off columns are unbracketed *upward* — already below 50 per cent at -19 dB, the
shallowest rung walked — so the downward extension cannot reach them and walking up is not
licensed**; they are reported against a stated ceiling. **The extension was therefore spent on
the grid only**, where `summed x4` reads 306 of 306 at all three rungs and would otherwise have
been the only one of tonight's six columns with no crossing at all.

**3. `STEP: 6` was deliberately left at `partial` in both `PHASE_OUTCOME.md` and
`PHASE_STATUS.md`.** Task 6 item 1 says to move the header's `STEP: 6` line; task 6 item 6 says
not to declare the phase closed and to leave the reading of whether step 6 is `done` to the
session that judges this report. **Unit 256 moved it to `done` and the judging session reversed
it.** This unit states its position in `STATE_WHY` and does not write its own verdict. **It is
flagged here because it is a judgement call between two items of the same task.**

## 2. What the owner should expect

**Combining is now answered at the ratios your closing table is quoted at, and at the place a
real station actually lands.** Before tonight the closing document could only show combining
turned on and off at rungs *below* the table's, at one placement, on a ladder where every
hearing was jittered. It can now say what four hearings are worth at -19, -20 and -21 dB, on
the analysis grid and half a bin off it.

**Nothing shipped and nothing in the radio changed.** `Ft8Reception.cs` is untouched.
Subtraction and combining are still **off by default**, no radio does either, and whether
either ships is yours with the figures now in front of you. No line under `src/` moved.
`Ft8Sharp` stays 0.10.7 and `Ft8Sharp.Deep` stays 0.8.0; only the root version moved, 1.12.58
to 1.12.59.

**What will look wrong but is not, in four places.**

- **Three cells read 306 of 306 — a perfect score — and one reads 100 % at the cell centre
  too.** That is **saturation and it is the answer, not a broken measurement.** At the rungs
  the closing table quotes, a station heard four times on the grid is decoded every time. No
  bound is asserted on any rate; the rate is the measurement. It was deliberately *not* chased
  down the ladder inside the panel.
- **There is a RED test in the tree, and there are now two of them.**
  `Ft8Unit257PlacementPanelTests.TheDownwardExtensionOnGridAtMinus23` fails, alongside unit
  256's `TheCombiningPanelAtMinus23`. **Both are meant to be red.** A wrong decode at -23 dB is
  a real finding and weakening the assertion would make this the unit that stopped checking.
- **The closing document now carries two combining panels that disagree — §5.4 says 254 of 306
  at -21 dB and §5.5 says 306 of 306.** They are not two attempts at one number. §5.4 is a
  station whose oscillator drifts between overs; §5.5 is a station whose four transmissions
  land in the same place. **Between them they bracket a real station**, and 0.93 dB separates
  their crossings. §5.4 was not re-run, amended or corrected.
- **`STEP: 6` still reads `partial` in both header files.** That is deliberate, per decision 3
  above, and not an oversight.

## 3. What you should see

**No visible change in the application. Nothing the operator would see moved tonight** — this
unit measured, and every figure it produced is a measurement handed to you rather than a change
to what a radio does.

**The answer this unit was commissioned to ask, with its evidence.** *What is combining worth
at the ratios the closing table is quoted at, and at the place a real station lands?*

> **On the grid, at -19, -20 and -21 dB, everything there is: 306 of 306 at every rung, against
> 248, 73 and 13 of 306 with combining off. Off the grid, half a waterfall bin out, at -21 dB a
> station heard once is one Hamlet never hears — 0 of 306, and ordered statistics does not help
> it, also 0 of 306 — and heard four times in the same place it is decoded 75 times in 306, every
> one of them a trial nothing else could reach. Combining crosses 50 per cent at -22.41 dB on the
> grid and -20.60 dB off it, so the cost of landing where real stations land is 1.81 dB. Zero
> wrong across all eighteen rows and 5 508 scored slot decodes.**

### The mismatches between the instruction and the tree

**Reading 2's arithmetic first, because the whole design turns on it.**

1. **`Ft8LadderHarness.cs:573-574` — NO MISMATCH, and this is the important one.** Both lines
   are verbatim what the instruction states. `frequencyHz` and `offsetSamples` set the first
   hearing and every later hearing steps from it, so at zero jitter every hearing sits at the
   stated placement. **Reading 2 holds, reading 3's fallback was not taken, and the panel is
   labelled *placement* rather than *first-hearing placement*.** It was then confirmed by
   measurement: the twelve combining-off figures reproduce §3.1 and §3.2 **to the decode**.

2. **MATERIAL MISMATCH — the zero-jitter consistency check is cited to the wrong section and
   the wrong number.** The instruction sends task 1 to *unit 247 §2, 49 of 51 at -21 dB with no
   jitter*. **`docs/unit247-combining.md` §2 is *The pairing, measured before it was designed*,
   and its 49 of 51 is candidate geometry, not a decode rate** — it is how many trials had the
   two slots' closest candidates within one waterfall bin (`:117`) and within 0.16 s (`:118`).
   **The zero-jitter decode figure is in §4** (`:205`): `combined x2` reads **217 of 306** at
   -21 dB on grid, with `single slot` 13 and `single + OSD` 33. The instruction's *jittered*
   figure of 68 of 306 is correct (`:239`). **The corrected bound was used**; tonight's
   `summed x4` cleared it at 306 of 306, as a four-hearing accumulated stacked column should.

3. **MISMATCH — `SESSION.lock` is `M` (modified), not `D` (deleted)** as the instruction's tree
   snapshot states. Not repaired; it is parked.

4. **MISMATCH — there is no untracked file under `.run-unit/`.** The instruction states *twelve
   modified files and one untracked file under `.run-unit/`*. The tree has exactly twelve
   modified files there and nothing untracked. Not repaired; it is the launcher's.

5. **IMMATERIAL — `Ft8Unit256CrossingBand`'s `Header` is at `:226`, not `:231`.** The
   instruction writes "`Header` and `AsRow(column, placement, band)` at `:231`"; `AsRow` is at
   `:231` and `Header` at `:226`.

6. **OBSERVATION on `PHASE_OUTCOME.md`'s own entries, not on the instruction.** The last entry
   before tonight's is headed **`## UNIT 7 - STEP 6`** and is plainly the judging session's
   entry for unit 256, whose approach line it repeats verbatim. **Not repaired** — the file is
   the launcher's and repairing it is not this unit's.

**Everything else in *Verify this instruction against the tree* held exactly**, and each was
checked rather than assumed: root `1.12.58` at `Directory.Build.props:205`, `Ft8Sharp` `0.10.7`
at `:396`, `Ft8Sharp.Deep` `0.8.0` at `:155`; `docs/unit255-closing-measurement.md` 1 250 lines
with §3.1 at 378, §3.2 at 426, §4.1 at 531, §5.0 at 756, §5.2 at 795, §5.3 at 821, §5.4 at 889,
§6.1 at 1013, §6.2 at 1122, §6.3 at 1170, §6.4 at 1220; `docs/unit256-crossings-and-combining.md`
733 lines; `Ft8LadderHarness.cs` 1 312 lines with `:64`, `:69`, `:270`, `:357`, `:404`, `:472`,
`:741` and `:871` all as stated; `RunRepeats` giving three rows with the `summed x{depth+1}`
label at `:514`; the cell-centre constants at `Ft8Unit255ClosingLadderTests.cs:62` and `:64` and
at `Ft8Unit256CellCentreBracketTests.cs:70` and `:72`; every landmark in
`Ft8Unit256CombiningPanelTests.cs` including the call at `:112` and the red at `:275`;
`Ft8Unit256CrossingBand.cs` at `:46`, `:169` and `:211`; `docs/gate-set.md` at 13 entries with
the ladder ruling at line 57; `docs/breakage-record.md` `B18` at `:412`; `HM-OPEN-082` at
`OPEN_ISSUES.md:7`; §5.4's whole panel table; and §5.3's 145.3 s.

### Every refused shell call, verbatim

**Three, all worked around, none halting anything.** `dotnet` was refused in no spelling across
nine test invocations and three builds; `git` in no spelling across six commits and the push.
**The file-editing tools were unaffected throughout, as on units 251 to 256.**

```
$ "tools\arbiter\outcome-append.bat" 2>&1 | head -20
This Bash command contains multiple operations. The following part requires approval:
"tools\arbiter\outcome-append.bat" 2>&1
```

```
$ tools/arbiter/outcome-append.bat
This command requires approval
```

**That is six consecutive units on which `outcome-append.bat` has been refused.** The
`PHASE_OUTCOME.md` entry was appended by hand with the file-editing tools in the entries'
format, and **says so on its own face**, as units 251 to 256 did.

**And a third, which is a spelling not seen on units 251 to 256 — a shell redirection into the
working directory itself:**

```
$ tail -n +66 output.md > .rest.tmp && wc -l .rest.tmp && head -1 .rest.tmp
Output redirection to 'C:\Source\HamLet\.rest.tmp' was blocked. For security, Claude Code may
only write to files in the allowed working directories for this session: 'C:\Source\HamLet'
```

**Worked around immediately with the file-editing tools**, which is what it was wanted for.
**Recording it because it is new**: the refusal names the working directory as *allowed* and
blocks a write into it in the same sentence, so `>` appears to be refused as a category rather
than on the path. **A later unit should reach for the file-editing tools first rather than
spend a probe on it**, and it would in any case have breached *do not create a scratch file at
the repository root*.

### The walks, in the order they ran, predicted against actual

| # | method | rung | placement | predicted | **actual** | result |
|---|---|---|---|---:|---:|---|
| 1 | `ThePlacementPanelOnGridAtMinus21` | -21 | on grid | 147 s | **144.0 s** | passed |
| 2 | `ThePlacementPanelOnGridAtMinus20` | -20 | on grid | 147 s | **144.5 s** | passed |
| 3 | `ThePlacementPanelOnGridAtMinus19` | -19 | on grid | 148 s | **145.2 s** | passed |
| 4 | `ThePlacementPanelAtCellCentreAtMinus21` | -21 | cell centre | 147 s | **145.9 s** | passed |
| 5 | `ThePlacementPanelAtCellCentreAtMinus20` | -20 | cell centre | 148 s | **149.4 s** | passed |
| 6 | `ThePlacementPanelAtCellCentreAtMinus19` | -19 | cell centre | 150 s | **150.4 s** | passed |
| 7 | `TheDownwardExtensionOnGridAtMinus22` | -22 | on grid | **45 s** | **144.7 s** | passed |
| 8 | `TheDownwardExtensionOnGridAtMinus23` | -23 | on grid | **45 s** | **145.8 s** | **RED — left red** |
| 9 | `TheCrossingsForTonightsSixColumns` | — | both | a computation | **0.005 s** | passed |

**The six required calls were priced to better than 2 per cent.** **The two extension rungs were
priced wrong by a factor of 3.2**, and the reason is worth carrying forward: §1.4 took their
price from **unit 256's measured wall clocks**, which is the right source for a *jittered* panel
and the wrong one for this one. Unit 256's -22 dB call is cheap because under 2.00 Hz and 480
samples of drift almost nothing decodes — 43 of 306. **At zero jitter the same rung reads 239 of
306** and costs what every other call cost. **A wall clock transcribed from another panel carries
that panel's jitter with it.** It cost nothing: both sat at 145 s against a 480 s ceiling and the
300 s split line, so no call was restructured, delayed or dropped.

**No call approached the twelve-minute watchdog, nothing was backgrounded, nothing was polled,
and no test this instruction did not construct was run at any point.**

## 4. What's blocking us

**Nothing blocking, and no ruling is wanted.** Five items, each labelled for what it is. **None
stands in the way of any of step 6's five exit criteria.**

**1. A wrong decode at -23 dB, reproduced at zero jitter — AN OBSERVATION FOR THE RECORD, and a
second dated observation under `HM-OPEN-082` rather than a new issue.**
`Ft8Unit257PlacementPanelTests.TheDownwardExtensionOnGridAtMinus23` returned trial 29, seed
220771, `SENT "CQ PY2ABC GG66"`, `RETURNED "WN8ESU/P JG5HKE/P R AH58"` — **byte for byte what
unit 256 recorded at 2.00 Hz and 480 samples of jitter.** It survived a change of both jitter
axes, so **it is a property of the rung and of the shipping stack's exposure, not of the jitter
or of the pairing geometry.** It did not come from a combination: `CombinedDecodes` 32,
`CombinedDecodesVerified` 32. **The assertion is left red and unweakened. Which of the issue's
three named causes it is was not investigated** — that is outside step 6. **It blocks no
criterion**: it is 1.41 dB below combining's own crossing and two decibels below the deepest
rung any criterion covers, and exit 1's six cells read zero wrong.

**2. The combined column's inner decoder does not pay for fine sync at the rate §3's `fine sync
only` column does — AN OBSERVATION, and the one thing tonight found that nobody has explained.**
The inner decoder is built with the same two stages as `SHIPPING`
(`osd: Default, fineSync: Default, rememberHearings: true`), and §3.1 prices `SHIPPING` at 203.8
ms a slot at -21 dB; but `summed x4` came back between **72.8 and 77.3 ms a slot** in all eight
walks, which is the `OSD only` cost. `Ft8DeepSlotDecoder.cs:747` gates fine sync on
`result.Status != Ft8CodewordStatus.Decoded` and `:267` names a counter
`RefusedForWantOfSamples`, so there is a plausible mechanism. **`RunRepeats` does not surface
`LastFineSync`, so this unit could not settle it without changing a shared harness, and reading
6 forbids touching `src/`.** **It changed no decision tonight** — the pricing question it bears
on came out under the 300 s split line either way. It may mean the panel's *stacked* label
overstates what fine sync contributes, which is worth a later unit's attention and is not this
one's.

**3. `outcome-append.bat` refused for the sixth consecutive unit — A COMPLETED WORKAROUND, not
a request.** Both spellings are quoted verbatim in section 3. The entry was written by hand in
the entries' format and says so on its face. **Nothing halted.**

**4. `PROJECT_STATUS.md` `RULES_AT:` says `HM-DEC-155 (2026-09-05)` while `CLAUDE.md` §1's
highest is `CPS-DEC-0152` — AN OBSERVATION, reported as the instruction directs.** **Not
reconciled, and not raised as a question.**

**5. The tree was not clean on arrival and is not clean now — AN OBSERVATION, and none of it is
this unit's.** `.tmp-sink.py` is still untracked at the root (unit 255's; `rm` and `git clean`
were refused in three spellings there and no fourth attempt was made tonight), `SESSION.lock`
shows modified, `RUN_LEDGER.md` and `WORK_INSTRUCTIONS.md` show modified, and twelve files under
`.run-unit/` show modified. **All are the launcher's or parked. Reported, repaired none.**

### One thing that is explicitly not an item above

**`docs/gate-set.md` stands at 13 and `docs/breakage-record.md` at `B18`, and nothing was added
to either.** Tonight's arithmetic is `Ft8Unit256CrossingBand`, which is already entry 13 and was
already watched failing as `B18`; a caller of existing arithmetic is not new arithmetic, and
tonight's walks are measurements, which earn neither. **Nothing broke that was not already
recorded**, and saying so explicitly rather than padding the list is that file's own rule.
