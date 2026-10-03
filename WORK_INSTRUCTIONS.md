# Work instruction 529 - look at a sender through a window that fits it

**Hand run. One unit, three tasks, commit per task. Task 1 is the unit.**

**R88 stays lifted for `tests\fixtures\cw\captured\cw-2026-10-02-200157.wav` alone** (the owner,
2026-10-02). No other recording is read. Synthetic hops written in a test are allowed. Verify by
building `Hamlet.sln` with warnings as errors and running the app carry-forward line. **Every
existing reading case reads as at HEAD or better**, or the report says what changed and why.

**Numbering.** This is unit 529, ruling HM-DEC-233. If taken, use the next free and say so.

---

## 0. The project gate

```
STOP. Verify the project before reading any further.

PROJECT: Hamlet

Check the repository root:
  MUST EXIST:      SHACK_FACTS.md
  MUST EXIST:      src\Hamlet.RadioEngine\Cw\CwPatternGate.cs
  MUST EXIST:      tests\fixtures\cw\captured\cw-2026-10-02-200157.wav
  MUST NOT EXIST:  CoreHMI.sln
  MUST NOT EXIST:  MURC.sln
  root             C:\Source\HamLet

If all five are not as stated, refuse: reply with only the path you are in,
which checks failed, and "wrong project - nothing done."

If all five hold, say "Hamlet confirmed" and continue.
```

---

## 1. Rules

- Take `SESSION.lock` through `tools\arbiter\lock.bat take`, release it at the end. Write nothing
  to `RUN_LEDGER.md`. Touch nothing under `tools\arbiter\`. Tick nothing in `PHASE_PLAN.md`.
- One `dotnet test` invocation per line, filtered, with a `timeout`. Never background and poll.
- Apostrophes in quoted heredocs break; `;` is refused; Python cannot run here; `-m` more than
  once for a multi-line commit. Scripts go in `.run-unit\unit529-<name>.sh`, not committed.
- Nothing that keys or transmits. Nothing written to the radio.
- **Nothing is tuned to the recording.** Every figure has a reason from what a keyed tone or a hand
  does; the recording shows the fault, it does not set the figure.
- `output.md` at the root, four headings exactly: `## 1. What Claude did`, `## 2. What the owner
  should expect`, `## 3. What you should see`, `## 4. What's blocking us`.

---

## 2. Why

**Unit 528:** the `T` of `BEST` and the `7` after the pause break the detector's flatness test,
because *"a 210 ms top wobbling 3 to 4 dB at 13 dB contrast breaks the run."*

**The web session measured the same dah from the same WAV and its top wobbles 0.4 dB.** The
difference is the window:

| | how the envelope was taken | top wobble |
|---|---|---|
| web session | mixed at exactly **662.8 Hz**, 4th-order low-pass at **60 Hz**, 5 ms hops | **0.4 dB** standard deviation |
| Hamlet | a 25 Hz bin at **650 or 675**, a **10 ms** window - about 100 Hz of noise bandwidth - with the tone between bins | 3 to 4 dB |

**The flatness test is right; the measurement is noisy.** Hamlet looks at every signal through a
window wide enough for a 45 WPM dit and centred on a grid, not on the tone. Every per-hop shape
test - flatness, edges, the settle rule, the fit - sits on that measurement.

---

## 3. Task 1 - a standing sender is measured through its own window

**Once a sender stands, its pitch and its dit are known.** From then on, **its envelope is taken at
its own pitch through a filter that fits its dit**:

- **mixed at the sender's pitch** - the centroid unit 522 computes, to a fraction of a bin - not at
  a grid bin;
- **low-passed at a cutoff from its dit** - wide enough to keep a key's edges, narrow enough to shut
  out the noise beside it. **The figure is the author's**, stated with its reason; the web
  session's 60 Hz on a 75 ms dit is one point, not the rule;
- **causal**, as a live receiver must be - its delay stated and taken off every mark's times;
- **followed as the sender's pitch and dit move**, as unit 524 already follows a standing sender
  on its own bin.

**Its marks are found on that envelope** - flatness, edges, settle, length - with the same tests as
now. **Before a sender stands, nothing changes**: the per-bin path finds it, as today.

**Measure first, and report it:** the `T` of `BEST` (11.48 to 11.69 s) - its top's wobble in dB on
Hamlet's bin today and on the new envelope. **And the seven marks of fault 3**, the `7` after the
pause (14.9 to 17.0 s): candidates found before and after.

**Watch it fail first:**

1. **`TheOwnersRecordingReads`**: letters `FERCHAT<BT>BEST7V73<SK>KC4ZGPDEWA`, spaces ignored.
   **Red today.** Report the text before and after.
2. **The strength table** with the bench's 1 dB AGC and the filter: 8, 10, 12, 16, 24 dB. A
   matched window should move the floor down; report it.
3. **Unit 528's between-bin cases** at 612.5, 637.5 and 662.5 Hz, still whole.
4. **Noise, 30 s and 3 minutes:** nothing stands, nothing prints. The new window applies only to a
   sender that already stands, so noise should be untouched - say whether it is.

## 4. Task 2 - a pause is not a word

**Unit 528's table:** the sender's gap clusters were mis-measured. A **2.3-second pause** was taken
as the only word gap, pulling the word line to 1.6 s; two early gaps were taken as a whole
cluster. So retiring the five-dit floor broke six cases.

- **A gap longer than three of the sender's word gaps is a pause** - the sender stopping, not
  spacing - and is not counted in its word cluster. The figure is the author's, with its reason.
- **A cluster is trusted only from a stated number of gaps**; before that, the floor and √(7/3) of
  the letter centre stand, as now.
- **Then retire the five-dit floor - only if every case reads as at HEAD or better**, including the
  six unit 528 listed (the drifting hand's `BROWN FOX` and `2024 AND`, the straight key's
  `DE N0CALL`) and `KC4ZGP` on the recording. If any of those reads worse, the floor stays and the
  report names the case.

## 5. Task 3 - what is left on the recording

After tasks 1 and 2, re-run the recording and report each of these: **the `7` before `V`** read as
`M S` (a 120 ms gap inside a letter); **the `G` of `GP`** read as `M` (its dit lost); **the weak
25-35-25 WPM row** at 1 dB AGC, red at HEAD, printing 81 of 194. **Fix any whose cause is plain
from task 1's envelope; report the rest.** Drop candidate.

---

## 6. Record

- `PHASE_OUTCOME.md`, both copies: `## UNIT 529 - STEP 12`, one paragraph.
- `PHASE_STATUS.md`, both copies: names 529.
- Patch-bump `Directory.Build.props`.
- `CLAUDE.md` §1 index row.
- `DECISIONS.md`, newest first, **HM-DEC-233**, headline *A standing sender is measured through a
  window that fits it*, naming the 0.4 against 3 to 4 dB measurement, and whether the five-dit
  floor was retired.
- **Touch no checkbox in `PHASE_PLAN.md`**, and add no ruling.

---

## 7. Report

Section 2, for the owner, in plain words: rebuild; what the recording reads now beside what was
sent; that once Hamlet has a station it looks at it through a window that fits it, so a
wobbling-looking dah stays whole and a weak one stands further out of the noise; whether a long
letter gap still splits a callsign. Section 1: per task, what was found and changed, the cutoff
and its reason, the filter's delay. **Section 3: the wobble measurement first, then the recording's
text, then the strength table, then the existing cases.** Section 4: anything left, a line each.
