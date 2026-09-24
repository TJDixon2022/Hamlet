# Work instruction 411 - the screen stops saying what is not so

**Seed under `--seed`.** The first unit of step 6. It makes the RF gain banner state what
the radio actually reported, and it makes the capture sidecar's three contradictory
sentences either true or honestly silent. **Five tasks, drop from the back.**

**Status.** `sh tools/status.sh`, real clock, after every commit and every task, and
immediately before every `dotnet test`. **Write files as UTF-8.**

---

## 0. The project gate

```
STOP. Verify the project before reading any further.

PROJECT: Hamlet

Check the repository root:
  MUST EXIST:      SHACK_FACTS.md
  MUST EXIST:      src\Hamlet.RadioEngine\Cw\CwProbabilisticDecoder.cs
  MUST NOT EXIST:  CoreHMI.sln
  MUST NOT EXIST:  MURC.sln
  root             C:\Source\HamLet

If all four are not as stated, you are in the wrong repository.
REFUSE. Do not read the rest of this file, do not summarise it, do not
adapt it to whatever project you are actually in, and change nothing.
Reply with only: the path you are in, which checks failed, and
"wrong project - nothing done."

If all four hold, say "Hamlet confirmed" and continue.
```

---

## 1. The rules that killed sessions

**HM-DEC-155.** No suite. Only this unit's names and `docs\carry-forward-tests.txt`, run
as its top comment says. **Never background and poll.** One type per invocation, each with
its own `timeout`.

**The app line loses a different name to the headless dispatcher loop most runs.** A loss
before any assertion is re-run once and counted neither way. This unit works in the app
project, so expect it.

**The report's four top-level headings are exactly these, character for character:**

```
## 1. What Claude did
## 2. What the owner should expect
## 3. What you should see
## 4. What's blocking us
```

**No other wording and no fifth top-level heading.** Unit 410 wrote *What Tim should
expect*, `validate-output.bat` rule 2 refused the report, and the loop halted at stop 7
with the unit's work complete and unjudged. Section 2 is still written for Tim; its
**heading** says *the owner*.

**The `UNIT:` line carries no parentheses**, and no `&`, `|`, `<`, `>`, `^`.

## 2. The tool facts

Apostrophes in quoted heredocs break; doubled backslashes collapse; `;` is refused; `rm`
is refused; Python cannot run here; `-m` more than once for a multi-line commit. A bare
`git worktree`, `git checkout` and `git show` are refused at the prompt. Multi-step
commands go into `.run-unit\unit411-<name>.sh` and run with `sh`. Unit 410's runner
scripts can be copied under this unit's name, as 410 copied 409's.

## 3. Asks still outstanding

Carried per HM-DEC-139, verbatim in section 4. Unit 410 parked two items about which
recordings count toward the baseline and whether step 3 is framed right; **both are step
0's and step 3's, not this unit's**, and they stay parked. Under R54 anything that blocks
no criterion here goes to `docs\phase-correctness\PARKED.md`.

---

## 4. Why this unit exists

```
PHASE GOAL: Hamlet reads a CQ call correctly.
UNIT GOAL:  The RF gain banner and the capture sidecar stop stating as
            unknown or as measured what the tree shows is otherwise.
ADVANCES:   step 6 criterion 1
DRIFT:      0 - the first unit of the step
```

**Tim, at the radio, 2026-09-23.** He photographed a banner reading *"I asked for the RF
gain to be 100% and the radio did not confirm it, so I do not know where it is now."* In
the radio-state dialog in the same session, **RF gain 100%, read back over `CI-V 14 02`,
24 seconds earlier.** The confirmation path and the read-back path are not talking, and
the product is telling the operator it does not know a fact it holds. That is §0.0 in
reverse, and it is the sentence he trusts the app by.

**Three more of the same kind**, from `cw-2026-09-23-173723.txt`:

- `tonePeak 25.8` followed by *"not a figure about this recording"* - a number printed in a
  per-capture sidecar that is not about the capture.
- `elementHz not measured  (the decoder in this build does not say where each element began
  and ended...)` printed directly under `elements 169 seen, 169 resolved`.
- `keying no keying at 575 Hz, 69 ms key down, 16 dB swing, 98 key-downs` - no keying, and
  98 key-downs, in one sentence. The August analysis flagged this one and it still stands.

**This unit does not change what the radio does or what the decoder decides.** It changes
what the app says about them, and only where the tree shows the saying is wrong.

---

## 5. Verify this instruction against the tree

Check, report any mismatch, repair nothing:

- The banner's text, and the file it lives in. Search `did not confirm` across `src`.
- Where the RF gain read-back is held, and whether the banner's code can reach it: the
  radio-state dialog prints `RfGain 100%` with `CI-V 14 02` and an age, so something holds
  it.
- The sidecar writer, and the three lines named above, with their current wording.
- Whether `tonePeak` is a held-and-decaying figure across captures rather than a per-capture
  measurement, and where it is computed.
- Whether the keying verdict comes from `CwKeyingMeter`'s four-part test, and which of the
  four parts fails on 17:37 - unit 409 recorded that the swing figure runs low when the
  meter is asked of a whole 30 s file.
- `PHASE_STATUS.md` reads step 0 done and `CURRENT_STEP 1`; the plan now carries step 6.

## 6. Rulings in force

`PHASE_PLAN.md` R59 to R62, §3 and §6, read before task 1.

**R62** step 6 joins this phase and depends on nothing.
**§6** a screen criterion is met by **making a sentence true, never by deleting the sentence
and saying nothing**, and never by changing a radio setting to match a claim.
**CLAUDE.md §0.0** never state as known what is not known, and never state as unknown what
is known. **§0.2** nothing that keys or transmits is touched - the RF gain banner is read
and written, the transmit path is not. **§0.6** color is never the sole carrier.
**HM-DEC-155**, **HM-DEC-165**, **FACT-004**, **FACT-006**.

**Record this in `DECISIONS.md`, newest first, and one row at the top of `CLAUDE.md` §1's
table dated 2026-09-23, headline **The screen work joins the correctness phase as step 6**,
ref HM-DEC-170:**

```
---
id: HM-DEC-170
date: 2026-09-23
refs: PHASE_PLAN.md R62 step 6, OPEN_ISSUES.md, work instruction 411 task 0, CLAUDE.md 0.0, HM-OPEN-087
---

**The screen work joins the correctness phase as step 6, and it depends on nothing.**
Tim, 2026-09-23.

**What he found at the radio.** An RF gain banner stating the radio did not confirm a value
the radio-state dialog showed read back 24 seconds earlier; the window reflowing when he
tunes outside his privileges; no hover text on any control; three sentences in the capture
sidecar that contradict their own neighbors; a dead button on the CW tab.

**Why it is a step of this phase rather than a phase of its own.** It depends on nothing, so
the arbiter has a place to route whenever the CW work stalls, and the loop keeps moving. Its
sentence is CLAUDE.md 0.0, which binds in any phase: never state as known what is not known,
and never state as unknown what is known.

**What it is not.** It changes what the operator reads, never what the radio does. A
criterion is met by making a sentence true, never by deleting the sentence.

**Whose words are whose.** The ruling is Tim's; the wording is work instruction 411 task 0's
record of it. Rejected: a new phase holding both the CW and screen work; the screen in its
own phase afterward.
```

## 7. Status cadence

As the header says. `NOTE` says what is moving inside the task.

---

## 8. The tasks

### Task 0 - the record

`PHASE_OUTCOME.md` gets its `## UNIT 411 - STEP 6` entry in the existing shape from the
decision block at the foot of this file. `PHASE_STATUS.md` names unit 411 and
`CURRENT_STEP: 6`. Patch-bump `Directory.Build.props`. `DECISIONS.md` HM-DEC-170 and the
`CLAUDE.md` row. **Entry round:** both carry-forward lines and the three floor tests, every
number recorded.

**Drop candidate:** none.

### Task 1 - the RF gain banner (6.1)

Trace first: print, from a test that asserts nothing, what the banner's code can see about
RF gain at the moment it writes that sentence, and what the read-back holds. **Then watch a
test fail**: with a read-back held for RF gain, the banner must state the value and when it
was read, and must not say the radio did not confirm.

The sentence's wording is the author's and goes in the report. It states the value, and how
long ago it was read, in the same voice as the rest of the app. **When no read-back is held
the old sentence stands unchanged** - that case is true and is not this unit's to touch.

**Drop candidate:** none. This is the sentence he photographed.

### Task 2 - the sidecar's three sentences (6.2)

Each gets a test watched failing on a saved capture that shows the contradiction, then a
change:

- **`tonePeak`.** Either print a figure that is about this recording, or do not print it in a
  per-capture sidecar. The author chooses and says why.
- **`elementHz`.** It may not say nothing was measured while the line above resolves
  elements. Either report what the elements say, or word it so both sentences are true of
  the same run.
- **`keying`.** It may not say no keying at a pitch and then count key-downs at that pitch.
  Unit 409 found the swing figure runs low when the meter is asked of a whole file rather
  than its own six-second window; if that is the cause, the honest sentence names what it
  measured over what span. **Do not change the meter's verdict to make the sentence agree** -
  that is changing the instrument to fit the words.

**Drop candidate:** `tonePeak`, last of the three. `keying` first, it is the oldest and the
loudest.

### Task 3 - the sidecar is re-read (6.2)

Regenerate a sidecar from a saved capture through the same writer, and print all three lines
in the report, before and after, so the change is visible as text rather than as a diff.
**No capture in the tree is edited**; a sidecar generated for this test goes under
`.run-unit\`.

**Drop candidate:** whole task, if the clock is short.

### Task 4 - the exit round

Both carry-forward lines, the three floor tests, and every type touched. `git diff` over the
transmit files named in the restore phase's §3 against `7e209cb4` prints nothing. Nothing
under `src\Hamlet.RadioEngine\Cw` that decides a character is changed, and the report says
so with the diff.

---

## 9. Parked - do not touch, do not raise

- **6.3 the window reflow, 6.4 hover text, 6.5 the dead button.** Later units of this step.
- **Steps 1, 2, 3 and 4.** The CW work; the arbiter routes there on its own.
- **The decoder.** Nothing in `Cw` that decides a character changes in this unit.
- **The keying meter's verdict rule.** Word the sentence; do not retune the meter.
- **Unit 410's two parked items.** Step 0's and step 3's.

## 10. What not to do

- **Do not delete a sentence to meet a criterion.** Make it true, or make it honestly say it
  does not know.
- **Do not change a radio setting** to make a claim about the radio true.
- **Do not touch what keys or transmits.**
- **Do not change the decoder's verdicts** to make a sidecar line agree with its neighbor.
- **Do not edit a saved capture or its sidecar in the tree.**
- **No unfiltered `dotnet test`. Never background and poll. Never compose a timestamp.**
- **Report mismatches; repair nothing. American spelling. UTF-8.**
- **The four report headings are exactly as section 1 gives them.**

## 11. Committing and pushing

Commit per task. Push at the end and say whether it succeeded.

---

## 12. Reporting

`output.md` at the root. **The four headings exactly as section 1 gives them.**

```
READ IN THIS ORDER.

A. The banner's old sentence and its new one, quoted.
B. Step 6's criteria: 6.1 the banner, 6.2 the sidecar's three sentences,
   6.6 the exit round. 6.3, 6.4 and 6.5 not started.
C. The rest. Section 4 raises <n> items.
```

```
UNIT:       411 - <complete|stopped> at task N of 4, <dropped or none dropped> - <date time>
PHASE GOAL: <in your own words>
UNIT GOAL:  <in your own words>
ADVANCED:   yes | no - <why, on the line>
NUMBER:     sentences that contradicted the tree: 4 -> <n>
DRIFT:      <0 if a criterion moved>
```

**Section 2 tells the owner in one paragraph** what he will now read where he read the RF
gain banner, and what has not changed: the radio, the decoder, the text on the CW tab.

---

```
ARBITER-DECISION
STEP: 6
APPROACH: make the RF gain banner state what the radio actually reported when a read-back is held, and make the capture sidecar's tonePeak, elementHz and keying lines either true of the capture or honestly silent, each watched failing first
MOVE: continue
WHY: PHASE_PLAN.md step 6 criterion 6.1 asks that the banner state the value and when it was read whenever a read-back for RF gain is held, and that the did-not-confirm wording appear only when none is
STATE: not started
DECIDED: the banner's new wording, which of the three sidecar lines is repaired and which is reworded, and the per-type timeouts are the author's, overrulable
LICENCE: PHASE_PLAN.md R62, section 3, section 6; HM-DEC-170; CLAUDE.md 0.0, 0.2 and 0.6; HM-DEC-155; HM-DEC-139; FACT-004
ACCOMPLISHED: the two places Tim looks when he is deciding whether to trust the app stop saying what the tree shows is not so
ADVANCES: step 6 criterion 1
END-ARBITER-DECISION
```
