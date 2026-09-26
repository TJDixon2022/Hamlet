# Work instruction 454 - the second decoder reads beside ours

**Seed under `--seed`.** Section M of `CW_REQUIREMENTS.md` v1.1 rules that two decoders must
read the same audio. This unit is the first two rows of it: HM-REQ-122, fldigi's CW receive modem
ported faithfully as the second decoder, and HM-REQ-123, both decoders scored on every recording
through the same metrics before either votes. **The output is a parity table and a list of what
the second decoder does that ours does not.** Four tasks, drop from the back.

---

## 0. The project gate

```
STOP. Verify the project before reading any further.

PROJECT: Hamlet

Check the repository root:
  MUST EXIST:      SHACK_FACTS.md
  MUST EXIST:      src\Hamlet.RadioEngine\Cw\CwProbabilisticDecoder.cs
  MUST EXIST:      CW_REQUIREMENTS.md
  MUST EXIST:      CW_SPEC.md
  MUST NOT EXIST:  CoreHMI.sln
  MUST NOT EXIST:  MURC.sln
  root             C:\Source\HamLet

If all six are not as stated, refuse: reply with only the path you are in,
which checks failed, and "wrong project - nothing done."

If all six hold, say "Hamlet confirmed" and continue.
```

---

## 1. Rules, short

**HM-DEC-155.** No suite. Named types only, one per invocation, each with its own `timeout`.
Captures 600 s. **A run of every recording through two decoders will be slow; give it 900 s
and report what it took.** Never background and poll.

Apostrophes in quoted heredocs break; doubled backslashes collapse; `;` is refused; `rm` is
refused; Python cannot run here; `-m` more than once for a multi-line commit. Scripts go in
`.run-unit\unit454-<name>.sh`, run with `sh`.

**The four report headings, exactly:** `## 1. What Claude did`, `## 2. What the owner should
expect`, `## 3. What you should see`, `## 4. What's blocking us`. `UNIT:` line without brackets.
`ADVANCES: step 9 criterion 2`. WHY cites the plan.

**A CW question is answered from the documents and the second decoder's source, never raised to
the owner** (R85). One line in section 4 records the reading; the work goes on.

---

## 2. Why this unit exists

```
PHASE GOAL: Hamlet meets the CW requirements.
UNIT GOAL:  fldigi's CW decoder reads every recording beside ours, on the
            same metrics, and the report says where it wins and why.
ADVANCES:   step 9 criterion 2
```

**Read `CW_REQUIREMENTS.md` v1.1 - section M in particular - and `CW_SPEC.md` first. They win
over this instruction.** If the document at the root has no section M, stop and say so.

**Tim, 2026-09-26:** *"Why aren't we using something similar to Skimmer? Why don't we borrow
known technology? We should have done this months ago."* Ruled as R84.

**Where the source is.** The w1hkj/fldigi repository on GitHub is the SourceForge mirror.
The CW receive modem is `src/cw_rtty/cw.cxx` with its header `src/include/cw.h`, GPL-3,
copyright Dave Freese W1HKJ and Mauri Niininen AG1LE, adapted from gmfsk (Tomi Manninen
OH2BNS, Lawrence Glaister VE7IT). It depends on a handful of DSP helpers in the same tree -
the FFT filter, the moving average, the sliding FFT - which must come with it. **Hamlet is
GPL-3, so the port is legal; the attribution is kept in the ported file's header with the
upstream commit hash.**

**What we are and are not doing.** The port is the **second decoder** of HM-REQ-120, and it
will one day vote - **but not in this unit**. HM-REQ-123 says it is scored beside ours before
it votes, and HM-REQ-124 says it votes only once its confidence is calibrated. Neither exists
yet. So in this unit it is faithful to fldigi (122), not improved, and not wired into the app's
decode path. **Our decoder stays ours**; a technique taken from the second goes into ours as a
change judged under R78 (129).

**Where we stand, so the comparison has context.** Confidently-wrong letters on the 23 keyed
recordings: 67 on Thursday, 33 now, after the overnight run's four kept changes. Right letters
359 to 403. The W1AW bulletin `032129` reads `PGOPAGATION FORECAST BUAELETIN ARLP034`. A
mature decoder reads that line clean. **That gap is what this unit measures.**

---

## 3. Verify against the tree

- Whether `git clone` of a public GitHub repository is permitted from the session. **If it is
  refused, that is a denial: record it, and instead ask for the four files to be placed under
  `.run-unit\fldigi\` by the owner** - section 4, one line - and stop at task 1 with the
  request. Do not route around a denial.
- `CwScorer`, `CwMetrics`, and how a decoder is driven hop by hop through `CwDecodeHarness`;
  the second decoder must be driven the same way so the comparison is fair.
- The keyed recordings and the synthetic set, their keys and kinds.
- The audio format the harness delivers - sample rate, mono, float - and what fldigi's modem
  expects (its internal rate, its block size), so the port's input stage is named.

## 4. Rulings in force

`PHASE_PLAN.md` R77 to R85 and §6, and **section M of `CW_REQUIREMENTS.md`**, which governs.
**HM-REQ-122** faithful port, receive only, attribution kept. **HM-REQ-123** scored before it
votes. **HM-REQ-129** a technique goes into ours; the second stays as ported. **R78** the keep rule. **R80** no bookkeeping. **R72** no word prior - and
**if fldigi's modem carries any dictionary or word logic, it is left out of the port and the
omission is named**. **§0.2** nothing that keys or transmits; **the port is receive only** -
fldigi's `cw.cxx` also transmits, and every transmit path is left out. **HM-DEC-155,
HM-DEC-165, FACT-004.**

---

## 5. The tasks

### Task 0 - the record and the entry numbers

`PHASE_OUTCOME.md` gets `## UNIT 454 - STEP 9` from the block at the foot. `PHASE_STATUS.md`
names 454 and `CURRENT_STEP: 9`. Patch-bump. Entry round: both carry-forward lines, the floor
tests, the four metrics at HEAD.

### Task 1 - the port (9.1)

Fetch the source into `.run-unit\fldigi\` (not committed) and port the **receive** path of
`cw.cxx` to C# as `src\Hamlet.RadioEngine\Cw\Second\FldigiCwDecoder.cs`, plus whatever DSP
helpers it needs under the same folder. **Faithful**: same filter, same tracking, same
threshold logic, same element and gap decisions, same speed tracking. Every constant kept.
Every function ported gets the upstream name in a comment. **The file header carries the GPL-3
notice, the authors, and the upstream commit.**

**Left out, and named in the report:** transmit, the GUI hooks, and any word or dictionary
logic (R72). If fldigi's modem has more than one decoder mode - it has had a legacy decoder
and a newer one over the years - **port the one the current source runs by default**, and
name it.

**Prove it decodes at all** with a test on one synthetic case whose key is exact: the port
emits the right text, or the report says how far off and why. **Do not tune the port to pass.**
If it reads the synthetic wrong, that is either a porting error to find or a fact about fldigi
to record; say which.

### Task 2 - the parity table (9.2)

Drive both decoders through the same harness over **every keyed recording and the synthetic
set**. Score both with `CwScorer` and `CwMetrics`. Write `docs\phase-requirements\parity.md`:
per recording and per condition, MET-CER-SURE, MET-INVENTED, sure-and-right coverage and
MET-WBE, ours beside the second decoder, key kind beside each. **Totals at the bottom.**

**The second decoder has no sure/dim/placeholder classes** - fldigi prints what it decodes,
and HM-REQ-124's confidence is a later unit's. Say how its output was mapped for MET-CER-SURE
and coverage, apply the same mapping consistently, and record it in `parity.md` as 9.2 asks.

### Task 3 - where it wins, and why (9.3, 9.5)

For every recording where the second decoder beats ours on MET-CER-SURE, print both texts, then
**read the second decoder's source for that stretch** and name what it does differently: how it
found the tone, how it measured the unit, how it decided a gap was a space, when it emitted.
**From the source, not from guesswork.** Group the wins by cause.

Where the second decoder produces text on a recording ours reads as nothing, say so by name (9.5).

Where ours beats the second decoder, say that too - it matters for what not to take.

**Drop candidate:** whole task, with the parity table stated as the unit's product.

### Task 4 - the exit round

Both carry-forward lines, the floor tests, the four metrics, every type touched. `git diff`
over `src\Hamlet.RadioEngine\Cw` outside `Second\` prints nothing - **ours is unchanged
this unit**. The transmit files print nothing against `7e209cb4`. Report the decode time of
each decoder over the set.

---

## 6. Do not

- Do not improve the second decoder. Faithful (HM-REQ-122), or the comparison is worthless.
- Do not wire it into the app's decode path. It votes after 123 and 124, not before.
- Do not port transmit, GUI, or word logic.
- Do not change our decoder in this unit. 9.4 is the next unit's.
- Do not route around a `git clone` denial; ask for the files.
- Do not spend a task on the record.
- Do not raise a CW question to the owner. Read the source, read the spec, decide, record.
- **No unfiltered `dotnet test`. Never background and poll. Never compose a timestamp.**

## 7. Report

`output.md` at the root, four headings exactly.

```
READ IN THIS ORDER.

A. The parity totals: ours beside fldigi on the four metrics, keyed
   recordings and synthetic set.
B. Step 9: 9.1 ported and proved, 9.2 the table, 9.3 the wins named by cause,
   9.5 the recordings it reads that we do not.
C. The rest, in as few lines as it takes.
```

```
UNIT:       454 - <complete|stopped> at task N of 4, <dropped or none dropped> - <date time>
PHASE GOAL: <in your own words>
UNIT GOAL:  <in your own words>
NUMBER:     MET-CER-SURE ours <n> of <n>, fldigi <n> of <n>; recordings fldigi wins <n>, ours wins <n>, tie <n>
```

**Section 3 leads with the totals, then the largest win-cause with one recording's two texts.**

**Section 2, one paragraph:** how far ours is from a known decoder, in the operator's terms,
and the one thing the second decoder does that we should take first.

---

```
ARBITER-DECISION
STEP: 9
APPROACH: port fldigi's CW receive modem faithfully as the second decoder of section M with its GPL-3 attribution, drive every keyed and synthetic recording through both decoders on the same harness and metrics, table the parity per HM-REQ-123, and name from the second decoder's source what it does differently where it wins
MOVE: continue
WHY: PHASE_PLAN.md step 9 criterion 9.2, carrying HM-REQ-123, asks that both decoders read every keyed recording and the synthetic set through the same harness and be scored through the same scorer and metrics, tabled per recording and per condition in parity.md, before either votes
STATE: not started
DECIDED: which of fldigi's decoder modes is ported, how its unclassed output is mapped to sure for the metrics, and the per-type timeouts are the author's, overrulable
LICENCE: CW_REQUIREMENTS.md section M, HM-REQ-120 122 123 129; PHASE_PLAN.md R78, R80, R84, R85, section 6; R72; HM-DEC-155; CLAUDE.md 0.2 and 12.5; FACT-004; GPL-3
ACCOMPLISHED: Hamlet knows, in numbers, how far its CW decoder stands from a known one, and which technique to take first
ADVANCES: step 9 criterion 2
END-ARBITER-DECISION
```
