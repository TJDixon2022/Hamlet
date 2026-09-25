# Work instruction 435 - the search reaches the speeds people send at

**Seed under `--seed`.** On 14.0475 at 09:56 Eastern, W1AW code practice, the decoder read
nothing in 44 minutes and said why itself: *40 WPM won out of 8 to 40, 0.0 better than
silence, at the top of the search: the sender may be faster than Hamlet can look.* The
independent sweep measured a 27 ms median key-down - about 44 WPM. **The sender was outside
the range.** This unit raises it. Three tasks, drop from the back.

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

**HM-DEC-155.** No suite. Only this unit's names and `docs\carry-forward-tests.txt`, run as
its top comment says. **Never background and poll.** One type per invocation, each with its
own `timeout`. The captures type is 51 rows and runs about 120 s; **a wider search makes every
decode slower, so give it 900 s and report what it actually took.**

**The report's four top-level headings are exactly these, character for character:**

```
## 1. What Claude did
## 2. What the owner should expect
## 3. What you should see
## 4. What's blocking us
```

**The `UNIT:` line carries no parentheses**, and no `&`, `|`, `<`, `>`, `^`.
**`ADVANCES` reads exactly `step 7 criterion 1`.** **WHY cites a line of the plan.**
**Write `output.md` at the root before the session ends.**
**Nothing in section 4 halts this phase.** Park it and go on.

## 2. The tool facts

Apostrophes in quoted heredocs break; doubled backslashes collapse; `;` is refused; `rm` is
refused; Python cannot run here; `-m` more than once for a multi-line commit. A bare
`git worktree`, `git checkout` and `git show` are refused at the prompt. Multi-step commands
go into `.run-unit\unit435-<name>.sh` and run with `sh`.

## 3. Asks still outstanding

Carried per HM-DEC-139, verbatim in section 4. **None is this unit's to answer.**

---

## 4. Why this unit exists

```
PHASE GOAL: Hamlet reads a CQ call correctly.
UNIT GOAL:  Signals sent faster than 40 WPM stop reading as nothing.
ADVANCES:   step 7 criterion 1
DRIFT:      0
```

**The evidence, from `cw-2026-09-24-135641.txt`:**

- `reading  40 WPM won out of 8 to 40, 0.0 better than silence per hop against a gate of 1
  (AT THE TOP OF THE SEARCH: the sender may be faster than Hamlet can look)`
- `keying  no keying at 625 Hz: 308 rises above the threshold, median 27 ms, 7 dB swing`
- `inThis  0 characters emitted` - and 0 in the 44 minutes before it.
- 14.0475 MHz at 13:56 UTC is **09:56 Eastern, W1AW code practice**, whose Fast Code sessions
  open at 35 WPM and work down. A 27 ms median key-down is about **44 WPM**.

**The change is the range, not the method.** The speed search runs 8 to 40 and the sender was
past the end of it. **Nothing else about the decoder is touched.**

**What it may cost.** A wider search is more hypotheses per hop, so every decode gets slower,
and a wider search can also let a wrong speed win on a signal that reads correctly today.
**Both are measured here**, and the second is what 7.2 exists for.

---

## 5. Verify this instruction against the tree

Check, report any mismatch, repair nothing:

- **Where the range 8 to 40 is written** - the constant or constants, and whether the same
  numbers appear in more than one place. Name every one.
- Whether the search steps in whole WPM or otherwise, and how many hypotheses a hop carries
  today.
- Whether `cw-2026-09-24-135641.wav` is in `tests\fixtures\cw\captured\unadjudicated`. **If it
  is not, say so** - task 2 then measures on the fixtures alone and the report says which.
- The keyed totals at HEAD, all keyed over 565, and the captures type's wall time.
- Whether anything else reads the range - a sidecar line, a sheet caption, a test - so a
  changed range does not leave a sentence behind saying 8 to 40 (§0.0).

## 6. Rulings in force - do not re-argue

`PHASE_PLAN.md` R59 to R76, §3 and §6.

**7.2 is the cost test and it is not negotiable**: the total edit count over all keyed
recordings does not rise, no named floor breaks, the three adjudicated readings are unchanged
or move onto their own adjudicated text, no capture row's above-bar count falls, **and the
captures type's wall time is reported before and after**.
**R71** the floors count at or above raw span 13.0.
**§0.0** if a sentence anywhere says the search runs 8 to 40, it says what it now runs.
**§0.2** nothing that keys or transmits is touched. **§12.6** repair nothing on the way past.
**HM-DEC-091**, **HM-DEC-155**, **HM-DEC-165**, **FACT-004**, **FACT-006**.

No decision record is needed: the criterion is already the owner's.

## 7. Status cadence

As the header says.

---

## 8. The tasks

### Task 0 - the record

`PHASE_OUTCOME.md` gets its `## UNIT 435 - STEP 7` entry from the decision block at the foot of
this file. `PHASE_STATUS.md` names unit 435 and `CURRENT_STEP: 7`. Patch-bump
`Directory.Build.props`. **Entry round:** both carry-forward lines, the three floor tests with
the captures type's wall time, the keyed totals, the named floors, recorded as the numbers to
beat.

**Drop candidate:** none.

### Task 1 - raise it (7.1)

Watch a test fail first: a fact over `cw-2026-09-24-135641.wav` asserting that named characters
are emitted. **It is red today** - the capture reads nothing. Quote the red.

Then raise the top of the range. **How far is the author's**, and the reason goes in DECIDED:
take it from what the corpus and the sweep measure, not from a round number. The sweep's 27 ms
median on this capture is the evidence in hand; W1AW's fast sessions open at 35 WPM, and
ordinary hams work into the 40s. **Do not raise it so far that the search carries hypotheses
nobody sends at** - every extra one costs time on every hop of every decode.

If the range is written in more than one place, **change every one**, and say so.

**Drop candidate:** none.

### Task 2 - what it cost (7.2)

Run all four of 7.2's tests and print every one as a number:

1. total edits over all keyed recordings, before and after;
2. all 13 named floors;
3. the three adjudicated readings, quoted;
4. all 51 capture rows' above-bar counts, **and the captures type's wall time before and
   after**.

**Kept only if the capture reads something and nothing on that list gets worse.** If the wider
search costs a row, report the row and the number, take the change out, and say what range
would have held - **do not narrow it silently to pass.**

**And print what the 13:56 capture now reads**, whole, beside `nothing read`. That is what the
owner is waiting to see.

**Drop candidate:** none.

### Task 3 - the exit round

Both carry-forward lines, the three floor tests, the keyed totals, and every type touched.
`git diff` over the transmit files against `7e209cb4` prints nothing. **State the captures
type's wall time at exit** - if the decode got materially slower, that is a finding for the
report, not a reason to take the change out by itself.

---

## 9. Parked - do not touch, do not raise

- **`CwToneTracker` and the pitch instrument.** 4.1 and 4.6's, and not this unit's.
- **3.6 the stray letters, 7.3 and 7.4 acquisition, 6.5 the dead button, 7.8 the attenuator.**
- **The 75 Hz pitch gap** on this same capture - real, and step 4's. **Do not chase it here**;
  if the capture still reads nothing after the range is raised, that is the finding, and say
  the pitch gap is the likely reason.
- **Unit 427's six findings, P27, P43, P45 to P48.**

## 10. What not to do

- **Do not change the search's method**, only its range.
- **Do not narrow the range silently to make a floor pass.**
- **Do not leave a sentence anywhere saying 8 to 40.**
- **Do not lower an above-bar count on any row.**
- **Do not touch the tracker, the tone survey, or what keys or transmits.**
- **Do not halt for a question.** Park it.
- **Write `output.md` before the session ends.**
- **No unfiltered `dotnet test`. Never background and poll. Never compose a timestamp.**
- **Report mismatches; repair nothing. American spelling. UTF-8. The four headings exactly.**

## 11. Committing and pushing

Commit per task. Push at the end and say whether it succeeded.

---

## 12. Reporting

`output.md` at the root, the four headings exactly as section 1 gives them.

```
READ IN THIS ORDER.

A. What cw-2026-09-24-135641 reads now, beside "nothing read".
B. Step 7: 7.1 met or not, 7.2's four tests as numbers with the wall time.
C. The rest. Section 4 raises <n> items, none blocking.
```

```
UNIT:       435 - <complete|stopped> at task N of 3, <dropped or none dropped> - <date time>
PHASE GOAL: <in your own words>
UNIT GOAL:  <in your own words>
ADVANCED:   yes | no - <why, on the line>
NUMBER:     cw-2026-09-24-135641: 0 named characters -> <n>; search range 8 to 40 -> 8 to <n>
DRIFT:      <0 if a criterion moved>
```

**Section 3 leads with the text the 13:56 capture now gives.**

**Section 2 tells the owner in one paragraph** what he will now read on a fast sender, and what
it cost in decode time.

---

```
ARBITER-DECISION
STEP: 7
APPROACH: raise the top of the speed search past 40 WPM, chosen from what the corpus and the independent sweep measure, and judge it on the 13:56 capture reading something against the keyed totals, the floors, the adjudicated readings and the decode time
MOVE: continue
WHY: PHASE_PLAN.md step 7 criterion 7.1 asks that the range's top be raised past 40 WPM, chosen from what the corpus and the independent sweep measure rather than from a round number, and that cw-2026-09-24-135641 - which read nothing in 44 minutes at about 44 WPM - emit named characters
STATE: partial
DECIDED: how far the range is raised, and the per-type timeouts, are the author's, overrulable
LICENCE: PHASE_PLAN.md criteria 7.1 and 7.2, section 6; HM-DEC-091; HM-DEC-155; HM-DEC-165; CLAUDE.md 0.0 and 0.2; FACT-004
ACCOMPLISHED: a sender faster than Hamlet could look at is read instead of producing nothing at all
ADVANCES: step 7 criterion 1
END-ARBITER-DECISION
```
