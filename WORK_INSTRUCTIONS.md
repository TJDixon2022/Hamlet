# Work instruction 484 - the bar stays put, and silence is empty

**Hand run. One unit.** Two fixes the owner reported at the radio. No recording, no copied
telemetry, no data-driven test. **The owner's report after he rebuilds is the test.**

**The owner's ruling, 2026-09-28, R96:** *"You spend too much time testing against stuff that we
don't need. I don't want all those tests against data in our library. These are useless and
pointless. I will report back. That's the only testing you need for the most part."*

---

## 0. The project gate

```
STOP. Verify the project before reading any further.

PROJECT: Hamlet

Check the repository root:
  MUST EXIST:      SHACK_FACTS.md
  MUST EXIST:      src\Hamlet.RadioEngine\Cw\CwProbabilisticDecoder.cs
  MUST EXIST:      CW_REQUIREMENTS.md
  MUST NOT EXIST:  CoreHMI.sln
  MUST NOT EXIST:  MURC.sln
  root             C:\Source\HamLet

If all five are not as stated, refuse: reply with only the path you are in,
which checks failed, and "wrong project - nothing done."

If all five hold, say "Hamlet confirmed" and continue.
```

---

## 1. Rules

- **R96: no test against recorded audio, no copied telemetry, no fixture, no floor.** Verify by
  building `Hamlet.sln` with warnings as errors and running the app carry-forward line so nothing
  that worked breaks. That is all. The owner verifies the fixes at the radio.
- Take `SESSION.lock` through `tools\arbiter\lock.bat take`, release it at the end. Write nothing
  to `RUN_LEDGER.md`. Touch nothing under `tools\arbiter\`. Tick nothing in `PHASE_PLAN.md`.
- One `dotnet test` invocation per line, filtered, with a `timeout`. Never background and poll.
  The app line loses names to the dispatcher loop; re-run once, count neither way.
- Nothing that keys or transmits. Nothing that writes to the radio.
- Report in `output.md`, four headings exactly: `## 1. What Claude did`, `## 2. What the owner
  should expect`, `## 3. What you should see`, `## 4. What's blocking us`.

---

## 2. Fix one - silence is empty

**What the owner sees now:** the scope draws the level trace all the time, so the noise draws a
line across the panel whether anyone is keying or not. Unit 478's scope was blank when nothing
was keying, and the owner liked that. Unit 480 fixed a blank panel by drawing the level
continuously, and brought the noise back.

**The fix:** the trace and the bars draw **only while the detector says keying.** When it says no
keying, the scope is empty - no trace, no noise line. The *tone · mixing* words stay. The letters
over the bars stay.

Change only what `CwScopeControl` draws. The detector is not touched.

## 3. Fix two - the top bar does not swap when the dial moves

**What the owner sees now:** in CW, turning the radio's dial into the data block makes the top of
the window rearrange - the map and the neighborhood panel swap places. Choosing the same
frequency from the app does not. Unit 481's test drove frequency and mode together in one step
and found no difference, but the real radio does not arrive in one step: the dial sends the new
frequency first, still in CW; Hamlet sees the data block and writes USB-D; the radio then
announces the mode. The window is drawn between those, half updated.

**The fix, and it does not need the frame sequence to be right:** **the top row's arrangement
must not depend on the mode, the frequency, or how much text the neighborhood panel carries.**
Read the XAML and the view model and find what can move the map or resize the panel: a column
width that follows content, a panel that grows with an extra paragraph, a visibility that
collapses a column, or a layout rule keyed to the mode. Make the columns fixed by window width
alone, and make the neighborhood panel's text fit its space - wrap and clip, or scroll - instead
of growing. Unit 389's sun-map rule (393 x 214, dropping back below 1400 wide or when the
strayed-frequency line shows) stays as it is; it is keyed to width and a warning line, not to
the mode.

**Also:** the sentence *"Your General license covers Morse here"* must not appear on a data-block
frequency. Find where it is written and make it follow the block the frequency is in, on every
path.

Name, in the report, exactly what could move the layout before and why it cannot now.

---

## 4. Record

- `PHASE_OUTCOME.md`, both copies: `## UNIT 484 - STEP 11`, one paragraph, what changed.
- `PHASE_STATUS.md`, both copies: names 484.
- Patch-bump `Directory.Build.props`.
- **Append R96 to the rulings section of both `PHASE_PLAN.md` copies**, in the owner's words
  above, as a paragraph. **Touch no checkbox.**
- `DECISIONS.md`, newest first, one short entry, HM-DEC-189, headline *The owner's report at the
  radio is the test*, quoting him.

---

## 5. Report

`output.md`, four headings. Section 2 is for the owner, in plain words:

- rebuild;
- on the CW tab with nobody keying, the scope should be empty; when someone keys, the trace and
  bars appear with the letters over them;
- sit on a CW frequency, turn the dial into the data block, and the top of the window should not
  move.

Section 1 says what was changed, file by file, and that the build and the app line are green.
Section 4: anything left, in a line each.
