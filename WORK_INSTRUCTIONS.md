# Work instruction 480 - the letter sits over the bars that made it

**Hand run. One unit.** Paste the prompt at the foot into Claude Code. No loop, no STOP.

The CW tab shows a blank panel and the letters `T T` where the middle picture should be.
This unit makes the trace and the bars draw from live audio, and sets each decoded letter
over the bars it came from. Three tasks.

**The owner's words, 2026-09-28, R94:** *"I want to see the flat oscilloscope shapes with the
letter over top. I need a visual of dit and dash."* And: *"This is a training tool. Over time
I would start to recognize those patterns."*

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

## 1. Rules, short

**R88 - the corpus is banned.** No task reads a recording, runs a floor, the engine line, a
metric or the keyed set. Entry and exit: the app carry-forward line and the types this unit
touches.

**HM-DEC-155.** No suite. Named types only, one per invocation, own `timeout`. Never
background and poll. The app line loses names to the dispatcher loop; re-run once, count
neither way.

Apostrophes in quoted heredocs break; doubled backslashes collapse; `;` is refused; `rm` is
refused; Python cannot run here; `-m` more than once for a multi-line commit. Scripts go in
`.run-unit\unit480-<name>.sh`, run with `sh`.

**Hand run:** take `SESSION.lock` through `tools\arbiter\lock.bat take`, release it at the
end, write nothing to `RUN_LEDGER.md`, touch nothing under `tools\arbiter\`. **Tick nothing
in `PHASE_PLAN.md`**; the owner's eye is the judge.

**The four report headings, exactly:** `## 1. What Claude did`, `## 2. What the owner should
expect`, `## 3. What you should see`, `## 4. What's blocking us`. `UNIT:` line without brackets.
**Line C of the ordering block names how many items section 4 raises.**

---

## 2. Why this unit exists

```
PHASE GOAL: Hamlet meets the CW requirements.
UNIT GOAL:  On the live CW tab, the trace and the bars draw from the audio,
            and each letter is drawn over the bars it came from.
```

**What the owner sees now**, from his screenshot at 18:30 UTC on 14.0685: the scope panel is
blank - no trace, no bars - and beneath it, drawn large, the letters `T T`, with *"no keying ·
mixing 525 Hz"* on two lines. Unit 478 proved its drawing headless on a driven detector and
never on the live tab. Whatever the live tab feeds `CwScopeControl`, the result is text where
the shapes should be.

**What he wants**, and the picture he approved: the level trace with flat tops where the key
is down; a filled bar under every mark, dit short and dah long; and **above each group of bars,
the letter the decoder made of them**, drawn when the character settles. A gap between letters
is empty. Junk is bars with no letter, or a letter over bars that do not look like Morse - and
both are things to press *You're an idiot* on. This is a training tool: he will learn the
shapes.

---

## 3. Verify against the tree

- **Why the scope is blank on the live tab.** `CwScopeControl.Lines(frame)` draws from a
  `CwScopeFrame`; find what builds the frame on the live path (`CwHearingViewModel.Observe`,
  the decode tick, `CwEnvelopeDetector.History()`) and **whether the frame it gets on the
  live tab carries any hops at all.** 478's headless test drove the detector directly; the
  live tab may be handing it an empty history, or feeding it once a second where it needs the
  hop rate, or the control may not be laid out with a height. **Name the cause; do not guess.**
- **Where the `T T` comes from.** Something under the scope is rendering settled characters
  large. Find it - it may be 478's *tone · mixing* line's neighbour, or a leftover from 474 -
  and name it.
- **Where a settled character's time span is known.** `CharacterSettled` on
  `CwProbabilisticStream`, `CwCharacter`, and whether a character carries the hop or the
  sample it began and ended on. **If it does not, the smallest change that gives it one** -
  a start and end hop on the settled character - is this unit's, and it is the only engine
  change allowed, named in the report.
- `CwTranscript` and `CwTerminalControl`: the letters already reach the terminal; this unit
  draws them a second time, over the scope, and does not change the terminal.

## 4. Rulings in force

`PHASE_PLAN.md` R77 to R94 and §6. **R88** the corpus is banned. **R92** the scope is the
middle picture. **R94** the letter over the bars. **§0.0** the letter is drawn over the bars
the decoder actually used, at the time it actually settled - never nudged to look right.
**§0.6** words on hover for every drawn thing. **§0.2** nothing that keys or transmits.
**HM-DEC-155, HM-DEC-165, FACT-006.**

No decision record: R94 is a display ruling under R90 to R92, already recorded.

---

## 5. The tasks

### Task 0 - the record and the entry round

`PHASE_OUTCOME.md` gets `## UNIT 480 - STEP 12` from the block at the foot. `PHASE_STATUS.md`
names 480. Patch-bump. Entry round: the app carry-forward line.

### Task 1 - the scope draws live

Fix the cause found in §3 so that, on the live CW tab, the trace and the bars draw from the
audio the decoder is hearing, at the hop rate or at least 20 frames a second. **Remove the
large `T T` rendering.** Keep the *tone · mixing* words.

**Watch it fail first**: a headless test that builds the CW tab, feeds the *live* path -
`CwDecoder` given synthetic hops of a keyed tone, not a driven detector - and asserts the scope
frame it renders carries hops and bars. Red at HEAD if the live path is what is broken; if the
test is green at HEAD, the cause is elsewhere and the report says where.

### Task 2 - the letter over the bars

When a character settles, draw it above the span of bars it rests on: centred over the group,
in the same row as the scope, drawn once, and scrolling left with the trace. The span comes
from the character's start and end hop; if the tree does not carry those, add them to the
settled character in the engine and say so.

- A placeholder settles as its placeholder glyph, not a letter.
- A prosign settles as its bracketed name.
- A word gap draws nothing.
- Hover over a letter: *"the decoder made this letter from the bars beneath it"*.

**Watch it fail first**, headless, on the live path with a synthetic `CQ` at 20 WPM: red when
no letters are drawn over the scope; green when `C` sits over four bars and `Q` over four, at
the hops where they settled, and nothing sits over the gap between.

### Task 3 - the exit round

`Hamlet.sln` builds with warnings as errors. The app carry-forward line. Every type touched.
`src\Hamlet.RadioEngine\Cw` diff against entry: nothing, or the one start-and-end-hop change
named in §3, and nothing else. Transmit files print nothing against `7e209cb4`. **No recording
was read.**

---

## 6. Do not

- Do not change the detector, the meter, the tracker or the decoder's decisions. Only what is
  drawn, and at most the span a settled character carries.
- Do not draw a letter anywhere but over the bars that made it, at the time it settled.
- Do not read, run or measure against any recording.
- Do not touch the terminal, the buttons or the verdict row.
- Do not touch what keys or transmits.
- **No unfiltered `dotnet test`. Never background and poll. Never compose a timestamp.**

## 7. Report

`output.md` at the root, four headings exactly.

```
READ IN THIS ORDER.

A. Why the scope was blank on the live tab, and what the owner sees now.
B. Step 12: the trace and bars live, the letters over the bars.
C. The rest. Section 4 raises <n> items, none blocking. No recording was read.
```

```
UNIT:       480 - <complete|stopped> at task N of 3, <dropped or none dropped> - <date time>
            hand run, outside the loop
PHASE GOAL: <in your own words>
UNIT GOAL:  <in your own words>
NUMBER:     scope frames on the live tab: 0 -> <n> per second; letters drawn over bars: yes; recordings read: 0
```

**Section 2 tells the owner:** rebuild, tune a station, and watch the bars appear as you hear
dits and dahs, and the letter land over each group when it settles.

---

```
ARBITER-DECISION
STEP: 12
APPROACH: find why the live CW tab hands the scope an empty frame and fix it so the trace and bars draw from live audio, remove the large T T rendering, and draw each settled character over the span of bars it rests on
MOVE: continue
WHY: PHASE_PLAN.md step 12 criterion 12.2 asks that the CW tab draw the last four seconds as an oscilloscope with marks as bars, fed fast enough that the owner sees dits, and R94 rules that the letter the decoder made sits over the bars it came from
STATE: partial
DECIDED: the exact placement and size of the letter over its bars, and the frame rate, are the author's, overrulable
LICENCE: PHASE_PLAN.md R88, R90, R91, R92, R94, section 6, step 12; CLAUDE.md 0.0, 0.2 and 0.6; HM-DEC-155; FACT-006
ACCOMPLISHED: the owner watches the shapes he hears become the letters Hamlet prints, and learns the patterns by seeing them
ADVANCES: step 12 criterion 2
END-ARBITER-DECISION
```
