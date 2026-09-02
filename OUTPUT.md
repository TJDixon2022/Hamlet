READ IN THIS ORDER

A. **The phase goal.** Hamlet hears FT8 off the radio and displays the decoded
   text on screen. Tonight is step 5 of 7 — a found signal becomes a message.
B. **This step and its exit criteria.** Step 5 has five must-pass criteria: a
   corrupted codeword within the code's correcting power is recovered and one
   beyond it fails honestly; a candidate failing CRC is never returned as a
   decode; **no decode that is present and recoverable is lost**, as the owner
   amended it on 2026-09-02; `Ft8Sharp` tests green; attribution clean from
   `2828ab6` with the channel tests green.
C. **What this report adds, and whether it bears on A or B.** It bears directly
   on B and it closes it: all five must-pass criteria were measured tonight and
   all five are met, so **step 5 is closed**. It bears on A only by unblocking
   it — step 6 can now start, and step 6 is the verdict on steps 1 through 5.
   Nothing tonight is a sensitivity measurement. This report **raises 2 items**
   in section 4, neither of them a request for a ruling.

UNIT:       220 — complete at task 6 of 6 — 2026-09-02 14:25
PHASE GOAL: Hamlet hears FT8 off the radio and puts the decoded text on screen.
UNIT GOAL:  Re-take step 5's five criteria under criterion 3's amended wording, with the control group proved first, and close the step or name what holds it.
ADVANCED:   yes — step 5's criterion 3 is met and the step closes, the first step to move in five units.
NUMBER:     recoverable-and-lost, 0 of 169 (criterion 3's new gate, met) | raw reference-WAV count 760 of 1298 -> 760 of 1298, unchanged and no longer the gate
DRIFT:      0 consecutive units without advance  (was 4 — inferred, see below)

## 1. What Claude did

**Step 5 is closed. All five must-pass criteria were measured tonight and all
five are met.** Complete at task 6 of 6. Machine `QUIVERFULL`, project claimed
Hamlet and confirmed by the gate, branch `main`.

**The gate passed on all four checks:** `SHACK_FACTS.md` present,
`src/Hamlet.RadioEngine/Cw/CwProbabilisticDecoder.cs` present, `CoreHMI.sln`
absent, `MURC.sln` absent.

**This was a re-take, not a hunt.** Unit 219 had already looked, one signal at a
time, and found nothing recoverable being thrown away. Tonight re-measured that
under the criterion the owner rewrote to match it, with the control group
underneath it, and with the old count printed beside the new one.

### The five criteria, each with what was measured

**Criterion 1 — a corrupted codeword within the correcting power is recovered;
one beyond it fails honestly. MET.** Carried by the suite and not re-argued, but
read for its numbers: **18 000 trials** over a bit-flip sweep, every trial
recovered up to **k = 6**, recovery reached **zero at k = 17**, and **wrong
messages returned over the whole sweep: 0**. Beyond the correcting power at
**k = 44**, **400 of 400 returned nothing** and **0 returned a wrong message** —
which is the second half of the criterion in its own words. Three trials reached
a wrong codeword at all, against CRC-14's own undetected-error floor of 1 in
16 384.

**Criterion 2 — a candidate failing CRC is never returned as a decode. MET, and
taken harder than any unit has taken it.**

- **5096 genuine codewords with altered checksum bits**, and the sharp part is
  that **parity was fully satisfied on all 5096** — every one is a codeword the
  LDPC gate cannot fault. **0 returned anything at all.**
- **5000 random ratio arrays**: 0 messages returned, the closest trial still **2
  of 83 checks unsatisfied**.
- **51 wrong-checksum transmissions through the full slot decoder**: candidates
  and parity in every case, **0 returned**, every one refused `ChecksumFailed`
  at 0 checks unsatisfied.
- **An empty slot**: 0 candidates, 0 returned.
- **51 transmissions at −30 dB**: **603 candidates**, 0 reaching parity, **0
  wrong text**.
- And the one this unit added: **10 quiet neighbourhoods swept at 600 points
  each — 6000 alignment points through the full gate — 0 messages and 0 true
  codewords.**

**Criterion 3 — no decode that is present and recoverable is lost. MET.** The
count is in section 3 because it is what the unit was commissioned to produce.

**Criteria 4 and 5 — `Ft8Sharp` green, attribution clean, channels green. MET.**
`Ft8Sharp` **502 total, 501 passed, 0 failed, 1 skipped** in 1 m 39 s, read from
the TRX `Counters` element and not a console line; the one skip is
`Ft8TableGenerationTests.RewriteTheCheckedInTablesFile`, the table write gate.
Library rebuilt `--no-incremental` at **0 warnings, 0 errors**, with **0
`PackageReference` and 0 `ProjectReference` elements** in `Ft8Sharp.csproj`.
**183 paths** from `2828ab6` and **0** under `src/Hamlet.` or `tests/Hamlet.`.
Channels **55** and **9**, both green.

### The control group ran first and could have stopped the night

**An instrument that has never failed is not an instrument**, so this was taken
before anything was claimed, and all three checks reproduced unit 219 figure for
figure.

- **It finds what is there.** 12 of 12 lines the untold path already matched
  found a decoding alignment. Mean best agreement **170.2 of 174**, lowest 156,
  mean best sync score 24.6, and **all twelve decoded at bin offset zero** from
  the centre the list's own frequency put them at.
- **It refuses what is not there.** 10 quiet neighbourhoods, 600 points each,
  **0 messages and 0 codewords**. Best-of-600 agreement on empty air runs **106
  to 115**, mean 110.1 — so the B bound of **130**, fixed before the run, still
  sits 15 above the highest the null ever reached.
- **It agrees with the existing instrument.** 12 of 12 equal against
  `Ft8MissAccountingTests`' separately written nearest-candidate reading, 0
  differing. Two implementations agreeing, not one agreeing with itself.

The neighbourhood is unchanged and is the search's own: block offsets **−10 to
19**, both time sub-offsets, **two bins either side** (about 15.6 Hz), both
frequency sub-offsets — **600 points per line**. The decode rule was stated
before the run: the 20 best-agreeing points **plus every point the search itself
kept**, so the sweep can never miss a decode the untold path could have had.

### What I did to the record

`OPEN_ISSUES.md`: **`HM-OPEN-065`, the reference decoder, is recorded by name as
task 5 required and is not resolved by the closure.** It gains a unit-220
section saying in terms that step 5 closed with it **recorded rather than met**
under the plan's 2026-09-01 ruling; that step 5 *did* want it, and named the one
question nothing on this machine can answer — whether the 96 of 169 lines absent
to *this* receiver are present to the pin; that **step 6 will want it too**, for
a different reason; and that it stays `tim`'s because a compiler run is
owner-class under `ARBITER.md` §6.

**`HM-OPEN-066` is not closed, and its `blocks:` line changed with the reason
written into the entry** rather than left to be inferred: it was raised against
criterion 3's *old* wording, the owner replaced that wording, and the new one is
met — so it blocks nothing and is now a **regression witness rather than a
gate**. It gains a unit-220 section with the reproduced control group, the
reproduced split, and the fifth identical reproduction of 760 of 1298.

**Versions bumped as directed:** `Ft8Sharp` **0.10.3 → 0.10.4** under HM-DEC-152
with the reason written into the props file itself, including the line that
**nothing in 0.10.4 is a step 6 result**; root **1.12.26 → 1.12.27** under
HM-DEC-150. Both gates re-run after both bumps: 502/501/0/1, channels 55 and 9,
library 0 warnings 0 errors, attribution still 183 and 0.

**What I did not write, deliberately.** `PHASE_STATUS.md`'s `STEP:` lines and
`PHASE_OUTCOME.md` belong to the launcher under the session prompt and
`outcome-append.bat`. Step 5's closure is **declared** in this section and in
the decision block, and the launcher moves the card. I touched only the
`WORK_INSTRUCTION:` line.

### Task 6 was dropped whole, and it is the named drop candidate

**Task 6 — the entry step 6 inherits — was dropped whole, and I am saying so.**
The instruction named it the drop candidate and said *"Dropped whole, and say
that you dropped it"* with no condition attached, so it was dropped **as
directed** and not on a sizing judgement of my own. **No task other than the
named candidate was dropped.**

**And the drop was verified costless rather than assumed costless**, which is
the part worth having. All three things task 6 would have written into
`porting-notes.md` are already in that file: unit 218's **measured sensitivity
and its ladder table** at line 2638, the **calibrated noise convention with its
arithmetic** and the eighteen-slot noise floor in the same section, and the
**SNR-column disagreement recorded as `HM-OPEN-066`** at lines 2789, 2793, 2813
and 2932. So step 6's first unit inherits it from the file, not only from the
reports.

### The validator was refused again, for the tenth unit running

**`tools\arbiter\validate-output.bat` could not be run, and this is reported as a
refusal rather than routed around.** Five spellings were attempted and every one
was denied: `cmd //c "tools\arbiter\validate-output.bat output.md"`,
`tools/arbiter/validate-output.bat output.md`,
`./tools/arbiter/validate-output.bat output.md`,
`cmd.exe /c tools\arbiter\validate-output.bat output.md`, and the same with an
absolute path to the report.

**So all six rules were checked by hand against the script's own source**, with
the line numbers, and all six pass:

| rule | source | checked | result |
|---|---|---|---|
| 1 — a parseable `UNIT:` line above section 1 | line 97 | `UNIT:` found within the first 60 lines | **ok** |
| 2 — the four `## ` sections, in order, exact names | lines 110–114 | the four found and nothing else | **ok** |
| 3 — no fifth top-level section | line 117 | exactly 4 `^## ` lines; `###` is ignored by the script's own printed reading at lines 34–47 | **ok** |
| 4 — section 4 present even when empty | line 126 | `## 4. What's blocking us` at line start, plain ASCII apostrophe confirmed by byte dump | **ok** |
| 5 — section 3 non-empty | line 138 | **53** non-blank lines between `## 3.` and `## 4.` | **ok** |
| 6 — the ordering block above `UNIT:` | line 168 | `READ IN THIS ORDER` present, one `A.`, one `B.`, one `C.`, and `raises 2 items` matching its `raises \d+ item` | **ok** |

**I cannot claim exit 0**, because the script did not run. I can claim that the
rules it holds were applied by hand and that none of them fails.

### Decisions I made for myself, reproduced in full

**That the unit is numbered 220.** `WORK_INSTRUCTIONS.md` carries **no `# Work
instruction <n> - <title>` heading at all**, which the session prompt says to
read the number and title from. I took 220 from 219 being the last committed
unit, and wrote `PHASE_STATUS.md`'s `WORK_INSTRUCTION:` line from the unit goal.
Reported rather than repaired.

**That two of the instruction's quoted figures disagree with the tree, and both
are explained rather than smoothed.** It quotes **496 / 495 / 0 / 1** tests and
**180** attribution paths. Those are unit 219's *entry* figures. Unit 219 added
six tests and three test files, so its exit — and tonight's entry — is **502 /
501 / 0 / 1** and **183 paths**. Nothing is wrong; the instruction was written
from 219's opening rather than its close.

**That the drift count is inferred and the header line says so.** §8 has a
session take the count from §4.2's block, and **this instruction carries no such
block** — it is a seed written by the web session. `output.md` is also
git-ignored here, so the prior report cannot be read back. I took **4** from the
instruction's own decision block, which states that *four units worked step 5
and three worked that criterion and the count never moved*: units 216, 217, 218
and 219. **It is evidenced to that sentence and to nothing stronger.**

**That `HM-OPEN-066`'s `blocks:` line had to change.** Leaving it reading *blocks
step 5 criterion 3* after the owner replaced that criterion and the replacement
was met would have left the next reader with a step recorded closed and an open
issue recorded blocking it. I changed the line and wrote the reason into the
entry rather than silently editing it.

## 2. What the owner should expect

**Step 5 is closed and the dashboard should move to step 6** once the launcher
writes the card from the outcome file. **This is the first time in five units
the phase has moved a step.**

**What will look wrong but is not.**

**The number that has held this phase up did not change, and the step closed
anyway.** Criterion 3 still reads **760 of 1298** against the reference WAVs —
the same figure units 216, 217, 218 and 219 all measured, identical column for
column. That is not a step closing on a number that failed. **The owner replaced
the criterion on 2026-09-02** because the old one measured somebody's expected
list rather than this decoder, and could not be met by any decoder that
de-duplicates the way upstream does. The new criterion asks whether anything
recoverable is being thrown away, and the answer is **nothing**. The 760 is
carried forward as a regression witness so a real fall in it stays visible.

**`HM-OPEN-066` is still open with a closed step above it.** Deliberate. The
issue is about a number worth understanding, not about a gate; its `blocks:`
line now says so.

**`HM-OPEN-065` is still open and step 5 closed without it.** Also deliberate,
and licensed by the plan's own 2026-09-01 ruling — an unmet *nice-to-pass*
criterion does not hold a step open provided it is recorded by name. It is
recorded by name. **It is the first thing step 6 will want if step 6's number
falls short**, and building `decode_ft8.exe` is still yours.

**Nothing in the library changed.** No library file was touched, no threshold
moved, no dependency taken, no divergence added. The decoder behaves exactly as
it did yesterday. **A version bumped twice with no behaviour change is correct
here** — it is a patch precisely because an operator would see no difference.

**Nothing tonight is a step 6 result** and none of it may be counted toward one.
Step 6 wants a reproducible curve, a verdict against the published figure, and
graceful degradation. This unit took none of the three and claims none.

## 3. What you should see

**The number this unit was commissioned to produce, as a count and not a
proportion:**

> **Recoverable-and-lost: ZERO. Out of 169 matchable missed expected lines at
> −5 dB or better.**
>
> **Criterion 3 is met if and only if that count is zero, and it is zero.**

**No visible change in the application.** Nothing an operator could see moved
tonight, and the version bumps say so by being patches. What changed is that the
step gating every remaining step in this phase is now closed.

### The evidence under that zero

**The two populations, re-measured rather than inherited, both reproducing unit
219 exactly:**

| population | lines | A present & recoverable | B present, not recoverable | C not present |
|---|---|---|---|---|
| at 0 dB or better | 78 | **5** | 35 | 38 |
| −5.0 up to but not including 0.0 dB | 91 | **0** | 33 | 58 |
| **together** | **169** | **5** | **68** | **96** |

**And the reading that decides what those 5 mean.** Every one of the five is an
**expected line the list itself carries twice**. The untold path *did* return
that text for that file and then de-duplicated it by **upstream's own payload
rule**. So:

> **Outcome-A lines that are not a repeated expected line: 0, in both
> populations.**

**Not one of the 169 is a transmission this library could have recovered and
threw away.** The search kept the decoding point in all five, so nothing was
lost at the search either.

**The cost of the measurement:** **101 400 alignment points**, **3703 belief
propagations**, and **0 lines met divergence 22's passband refusal**.

**The bound is shown rather than asserted.** In the 78 it fell in a **gap** —
highest C **129**, lowest B **132**. In the 91 it fell **adjacent** — highest C
**129**, lowest B **130** — which is printed here rather than hidden, because a
bound cutting through a cluster should be read with more suspicion than one
falling in a gap. Both sit above the quiet-air null's ceiling of 115.

### The raw count alongside, and it is not the gate

| column | tonight | units 216–219 |
|---|---|---|
| matched of expected | **760 of 1298** (58.6%) | 760 of 1298 |
| against the representable ceiling of 1157 | **65.7%** | 65.7% |
| candidates / parity / checksum / text / unique | 7803 / 2733 / 2733 / 2263 / 783 | identical |
| missed | 538 | 538 |
| **returned but on no list** | **23** | 23 |
| files measured / skipped for sample rate | 60 / 0 | 60 / 0 |

**Identical column for column for the fifth unit running. No regression.** A
fifth reproduction is what makes this a witness rather than an anecdote.

### The control group, which is the reason to believe any of the above

| check | result |
|---|---|
| finds what is there | **12 of 12** found a decoding alignment, mean **170.2 of 174**, all at **bin offset zero** |
| refuses what is not there | 10 quiet neighbourhoods, **0 messages, 0 codewords** over 6000 points |
| agrees with the existing instrument | **12 of 12 equal**, 0 differing |

**A sweep hit is evidence a transmission is present and it is never a decode.**
It is not a match, it did not move the 760, and it was added to no total
anywhere.

## 4. What's blocking us

**Nothing blocks step 5, which is closed. Two items, neither a request for a
ruling.**

### 1. `WORK_INSTRUCTIONS.md` has no `# Work instruction <n> - <title>` heading

**Reported, not repaired.** The session prompt says to take the
`WORK_INSTRUCTION:` line from that heading, and the file does not carry one —
its top-level headings are *Why this unit exists*, *Verify this instruction
against the tree*, *Rulings in force*, *Status cadence*, *Tasks*, *Parked* and
*What not to do*. This is a seed instruction written by the web session rather
than by the arbiter, so the heading the arbiter's own template supplies is
absent.

**What I did instead:** took **220** from 219 being the last committed unit, and
wrote the title from the unit goal. **What it would take:** either the seed
template gains the heading, or the prompt's rule gains a fallback. **It bears on
neither A nor B** and cost the unit nothing.

### 2. `HM-OPEN-065` is what step 6 will want first, and only the owner can build it

**Not a new item and not re-raised** — it is recorded by name in `OPEN_ISSUES.md`
per task 5, and this is the pointer rather than the raising.

**Why it matters now rather than later.** Unit 219 established, and unit 220
reproduced, that **96 of 169** matchable strong misses are not present *as far
as this receiver can see*. The one question left is whether they are present as
far as **the pin** can see. If step 6's sensitivity number falls short, the first
thing worth knowing is whether this library's synthesized signal is what it
believes it is — and only `decode_ft8.exe` reading one answers that.

**What it would take:** `decode_ft8.exe` built from the pinned clone at
`C:\Source\ft8_lib`, which carries `demo/decode_ft8.c` with the Makefile naming
it as a target. **A unit may not build it** — a compiler run is owner-class under
`ARBITER.md` §6. **It bears on A**, by way of step 6.

### Not raised, deliberately

The reference decoder as a *question* (`HM-OPEN-065`), the 82 placeholder
messages, and the two-decibel question are all already in front of the owner and
the instruction forbids re-raising them. The candidate limit and the minimum
sync score are parked; unit 216 answered sweeping them no, and tonight only
*read* what the search assigned at the true alignments and proposed nothing.

```
ARBITER-DECISION
STEP: 5
APPROACH: re-take step 5's five criteria under criterion 3's amended wording - no decode that is present and recoverable is lost - with the control group taken first and the raw reference-WAV count reported alongside as a regression witness rather than as the gate
MOVE: continue
WHY: All five must-pass criteria of step 5 were measured tonight and all five are met, so the step closes and step 6 becomes reachable for the first time in this phase. Criterion 3 under its 2026-09-02 wording returned zero recoverable-and-lost out of 169 matchable missed lines at -5 dB or better, with every outcome-A line proving to be an expected line the list carries twice and de-duplicated by upstream's own payload rule. The control group was taken before anything was claimed and reproduced unit 219 figure for figure, so the instrument was watched working before it was believed. The raw count did not move - 760 of 1298 for the fifth unit running, identical column for column - which is reported as a regression witness and was explicitly not the gate. Nothing in the library changed, no threshold moved, and no fix was licensed or needed.
STATE: done
DECIDED: That step 5 is closed rather than held open, because all five must-pass criteria have a measurement taken tonight and the plan's 2026-09-01 ruling says an unmet nice-to-pass criterion does not hold a step open provided it is recorded by name. That HM-OPEN-065 is recorded by name and not resolved by the closure, gaining a unit-220 section that says step 5 closed with it recorded rather than met and that step 6 will want it first if step 6's number falls short. That HM-OPEN-066's blocks: line had to change and the reason had to be written into the entry rather than the line quietly edited, because leaving it reading blocks step 5 criterion 3 after the owner replaced that criterion would have left a closed step with an open issue recorded as blocking it; it is a regression witness now and is not closed. That task 6 was dropped whole because the instruction directed it in those words with no condition attached, and that the drop was VERIFIED costless rather than assumed costless - all three things it would have written are already in porting-notes.md at named line numbers. That PHASE_STATUS.md's STEP: lines and PHASE_OUTCOME.md were not written, because they are the launcher's under the session prompt, so the closure is declared in section 1 and here and the launcher moves the card. That two of the instruction's quoted figures - 496/495/0/1 and 180 attribution paths - disagree with the tree because they are unit 219's entry rather than its exit, and are reported as explained rather than smoothed. That the unit is numbered 220 by inference, because WORK_INSTRUCTIONS.md carries no work-instruction heading at all for the number to be read from.
LICENCE: PHASE_PLAN.md's step 5 section and its five exit criteria, with criterion 3 as amended by the owner on 2026-09-02; the plan's 2026-09-01 ruling on when a step is done, which is what lets step 5 close on its must-pass criteria with HM-OPEN-065 recorded rather than met; the plan's ruling that every unmet criterion is recorded in OPEN_ISSUES.md by name and that recorded is not dropped, which task 5 discharges; the plan's ruling on what a unit runs, which licenses Ft8Sharp plus attribution plus the channel tests and forbids the full suite - the full Hamlet suite was not run; the plan's ruling that reference WAVs are never committed and are read from the clone by a test that skips when absent, and the clone was present so nothing skipped but the table write gate; the plan's ruling that no threshold moves and that a fix must be a fidelity fix against the pin, and no library file was touched; CLAUDE.md 0.2, restated because the encoder ran again for the true codewords and nothing it produced reached a device, a stream, a port or a file; CLAUDE.md 12.1, which is why the 82 placeholder messages are parked untouched; HM-DEC-152 for Ft8Sharp 0.10.4 and HM-DEC-150 for the root 1.12.27.
ACCOMPLISHED: Five units have held step 5 open on a count that turned out to be measuring somebody else's list rather than this decoder. Tonight that count was measured a fifth time and did not move, and the step closed anyway - because the owner rewrote the criterion to ask the question that matters, and the answer to it is zero. Nothing recoverable is being thrown away, across all 169 matchable missed lines at -5 dB or better, and the instrument that says so was watched answering correctly on twelve known answers and refusing correctly on ten stretches of empty air before it was asked anything unknown. Criterion 2 was taken harder than any unit has taken it: 5096 genuine codewords whose parity the LDPC gate cannot fault, every one refused at the checksum, plus 5000 random ratio arrays and 6000 quiet-air alignment points, and not one message came back that nobody sent. The phase now has a decoder proved to turn found signals into messages without inventing any, and the next unit can start asking the only question left that matters - how deaf it is against the published figure.
ADVANCES: Step 5, closed. All five must-pass criteria met, criterion 3 under its 2026-09-02 wording at zero recoverable-and-lost out of 169. Step 6 is now reachable and is the verdict on steps 1 through 5.
END-ARBITER-DECISION
```
