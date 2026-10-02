# Work instruction 528 - read the recording the owner made

**Hand run. One unit.** The owner recorded thirty seconds of a real QSO on 7.0549 MHz at 20:01 UTC
on 2026-10-02 and Hamlet misread four letters of it. **The signal is easy**: a plain fixed-threshold
decoder reads it almost whole. Every error is Hamlet's own. This unit reads that recording right.

**R88 is lifted for this one file**, by the owner, 2026-10-02: he made the recording for this and
said yes. **No other recording is read.** A headless test may also drive synthetic hops written in
the test itself. Verify by building `Hamlet.sln` with warnings as errors and running the app
carry-forward line.

**Numbering.** This is unit 528, ruling HM-DEC-232. If taken, use the next free and say so.

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

**If the WAV is missing, that is the failing check**: the owner places it, with its `.txt`, before
this runs.

---

## 1. Rules

- Take `SESSION.lock` through `tools\arbiter\lock.bat take`, release it at the end. Write nothing
  to `RUN_LEDGER.md`. Touch nothing under `tools\arbiter\`. Tick nothing in `PHASE_PLAN.md`.
- One `dotnet test` invocation per line, filtered, with a `timeout`. Never background and poll.
- Apostrophes in quoted heredocs break; `;` is refused; Python cannot run here; `-m` more than
  once for a multi-line commit. Scripts go in `.run-unit\unit528-<name>.sh`, not committed.
- Nothing that keys or transmits. Nothing written to the radio.
- **Nothing is tuned to this recording.** Every change has a reason from what a keyed tone or a
  hand does; the recording shows the fault, it does not set the figure.
- `output.md` at the root, four headings exactly: `## 1. What Claude did`, `## 2. What the owner
  should expect`, `## 3. What you should see`, `## 4. What's blocking us`.

---

## 2. What is in the recording, measured

Measured by the web session from the WAV, mixed at the true pitch through a 60 Hz low-pass,
5 ms hops:

| | |
|---|---|
| pitch | **662.8 Hz** - halfway between the 650 and 675 bins. The sheet says `toneHz 650` and `keying at 675` |
| dits | 75 to 90 ms (16 WPM) |
| dahs | 205 to 215 ms |
| gaps inside letters | 70 to 95 ms |
| gaps between letters | **120 to 430 ms** (2 to 5.7 dits) |
| gaps between words | 580 ms and longer (7.7 dits up) |
| AGC overshoot at key-down | median **0.65 dB**, 90% under 1.3, worst 2.1 |
| flatness of a top | 0.4 dB standard deviation |
| contrast, mark to gap | 13 dB at that bandwidth |
| noise | 18 blips of 5 to 10 ms, no mark-length noise |

**The elements, as sent**, with every gap over 105 ms in milliseconds:

```
[150] ..-. [285] . [170] .-. [925] -.-. [580] .... [290] .- [390] - [430] -...- [2300]
-... [270] . [255] ... [300] - [670] --... [360] ...- [1230] --... [290] ...-- [965]
...-.- [830] -.- [330] -.-. [365] ....- [420] --.. [405] --. [285] .--. [595] -.. [200]
. [215] .-- [180] .-
```

The `--...` after `BEST` has one 120 ms gap inside it; it is a 7.

**What was sent, spaced by the sender's own gaps:**

```
FER CHAT<BT> BEST 7V 73 <SK> KC4ZGP DEWA
```

**What Hamlet read:** `FER CHET<BT> BESE7V E ■ <SK> KC4 Z GP DEWA`. Four faults:

1. `CHAT` → `CHET`: the A's 210 ms dah read as a dit.
2. `BEST` → `BESE`: the T's 210 ms dah read as a dit.
3. `7V 73` → `7V E ■`: the 73 after a 1.23 s pause is lost.
4. `KC4ZGP` → `KC4 Z GP`: letter gaps of 405 to 430 ms are past five dits (375 ms), and **unit 525's
   five-dit floor** made them words.

---

## 3. The tasks

### Task 1 - read it, and find each fault before changing anything

Read the WAV through the live path - detector, pattern gate and reader wired as the app wires
them, at the radio's state from the `.txt`: CW, FIL2 500 Hz, pitch 600, AGC FAST. Report the
text. **Then, element by element, beside the list above:** what Hamlet found - start, length,
pitch, which bin, dit or dah - and **for each of the four faults, where it came from**: the
detector, the pattern gate or the reader, and the line that did it.

**The likely cause of faults 1 and 2 is the pitch**: 662.8 Hz splits its energy between two bins,
and a dah may be cut or attributed to the wrong one. **Unit 521's between-bin sweep passed without
AGC or the filter**; this is that case with both. Measure; do not assume.

### Task 2 - fix the dahs read as dits

Fix what task 1 found. **And add the synthetic case unit 521 lacked**: a clean sender at 612.5,
637.5 and 662.5 Hz - halfway between bins - through the 500 Hz filter on 600, with a 1 dB AGC
overshoot. Every dah a dah.

### Task 3 - the word gap is the sender's own

**The owner, 2026-10-02:** yes to replacing the five-dit floor with the sender's own gaps. This
sender's letter gaps reach 5.7 dits and his word gaps start at 7.7; a fixed five dits splits his
callsign. **The word line is drawn between the sender's own letter and word clusters**, as units
501 and 525 measure them; where only the letter cluster shows, √(7/3) of the letter centre. **The
five-dit floor retires.** Say what the line comes out at for this sender, and that it falls
between 430 and 580 ms.

### Task 4 - the 73 after a pause

Find why the `--... ...--` after a 1.23 s silence is lost: the sender released, the first marks
after a pause not standing, or the banking. Fix it if the cause is clear; otherwise report it.
**Drop candidate.**

### Task 5 - bench AGC is what the air measured

Unit 527 made the shared synthetic sender apply a **3 dB** AGC overshoot by default. **The air
measured 0.65 dB typical and 2.1 worst.** The default becomes **1 dB**; the 2 and 3 dB rows stay as
named stress cases. Report every reading case that changes.

---

## 4. The test

**`TheOwnersRecordingReads`** reads the WAV and asserts:

- **the letters, spaces ignored:** `FERCHAT<BT>BEST7V73<SK>KC4ZGPDEWA`;
- **no space inside `KC4ZGP`**;
- the text printed in full in the report.

It is the one test that reads a recording, and its remark says the owner lifted R88 for it.

**Every existing case reads as at HEAD or better**; where one changes, say so with its text.
Noise prints nothing.

---

## 5. Record

- `PHASE_OUTCOME.md`, both copies: `## UNIT 528 - STEP 12`, one paragraph.
- `PHASE_STATUS.md`, both copies: names 528.
- Patch-bump `Directory.Build.props`.
- `CLAUDE.md` §1 index row.
- `DECISIONS.md`, newest first, **HM-DEC-232**, headline *The owner's recording reads; the word
  gap is the sender's own*, naming the R88 exception for this one file, the five-dit floor's
  retirement at the owner's word, and the bench AGC correction.
- **Touch no checkbox in `PHASE_PLAN.md`**, and add no ruling.

---

## 6. Report

Section 2, for the owner, in plain words: rebuild; what the recording now reads, beside what was
sent; a callsign is no longer split by a long letter gap; a station between two bins reads its
dahs. Section 1: per fault, where it came from and what changed. **Section 3: the recording's
text first, then the element table, then the between-bin cases, then the existing cases.**
Section 4: anything left, a line each.
