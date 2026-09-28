# Work instruction 487 - a letter needs blocks, printed stays printed, one layout

**Hand run. One unit.** Three things the owner reported at the radio on build 1.13.173.
**No test against a recording, a fixture, a floor or copied telemetry** (R96). A headless test
driving synthetic hops written in the test itself is allowed; nothing read from disk. Verify by
building `Hamlet.sln` with warnings as errors and running the app carry-forward line. **The
owner's report at the radio is the test.**

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

- Take `SESSION.lock` through `tools\arbiter\lock.bat take`, release it at the end. Write nothing
  to `RUN_LEDGER.md`. Touch nothing under `tools\arbiter\`. Tick nothing in `PHASE_PLAN.md`.
- One `dotnet test` invocation per line, filtered, with a `timeout`. Never background and poll.
  The app line loses names to the dispatcher loop; re-run once, count neither way.
- Apostrophes in quoted heredocs break; `;`, `rm` and `git rm` are refused; Python cannot run
  here; `-m` more than once for a multi-line commit. Scripts go in `.run-unit\unit487-<name>.sh`.
  **Do not commit scratch logs or copies of source files under `.run-unit\`** - unit 486 did.
- Nothing that keys or transmits. Nothing written to the radio.
- `output.md` at the root, four headings exactly: `## 1. What Claude did`, `## 2. What the owner
  should expect`, `## 3. What you should see`, `## 4. What's blocking us`.

---

## 2. What the owner reported

**One: junk letters, still, in quantity.** *"We should get no letters unless we have a
flat-topped signal with a duration that matches CW. This new way of identifying things should
eliminate bad characters."*

**Why, from unit 486's own report.** Its gate asks two questions: is the detector keying **now**,
and was this character heard in the stretch that is keying now. **It never asks whether the
character's own elements were blocks the detector called.** So the decoder still reads a
continuous mix and emits whatever spells a letter; anything falling inside an open window gets
through, including letters built from noise between the real marks. The scope already refuses to
draw a letter with no block beneath it (unit 485). **The same rule has never been applied to
emitting.**

**Two: printed text disappears.** *"If you put a character on the screen, don't make it
disappear. It seems like the system is going in and out of detection, and when it goes out, it
erases the scroll. If you put something up, leave it."*

**Why.** Unit 486 was told to drop characters in flight when keying goes false, and implemented
it as *"the terminal's provisional tip is cleared in the same moment."* Clearing the tip takes
text that was already on the screen back off it.

**Three: one layout.** *"Here's the rule. There's only one layout. The layout that we use for CW
is the layout we use everywhere. It doesn't change. That's a rule."*

---

## 3. Change one - a letter needs blocks

**A character reaches the transcript, the leading edge or the scope only if the detector called
blocks for the elements it was made of.** No blocks, no letter. This is the scope's drawing rule
from unit 485, applied to emitting.

- Each element of a character is matched to a block the detector called at the pitch the decoder
  was mixing at, within the element's own span.
- **If any element of a character has no block, the character is not emitted at all** - not as a
  letter, not as a placeholder.
- The existing gate stays: keying must be true now, and the character heard in the open stretch.
  This is a third condition, not a replacement.
- **How an element is matched to a block** - the tolerance in milliseconds between an element's
  span and a block's - is the author's, stated in the report with its reason, and derived from
  the hop length, not from any recording.

**The decoder's own decisions are not changed** - not the lattice, not the unit estimator, not
the emission gate. Only whether what it decided is let out.

**Watch it fail first**, headless, with synthetic hops written in the test: a keyed call with a
stretch of noise inside the open window, loud enough that the ungated decoder reads letters from
it. Red while letters from the noise stretch reach a surface; green when only the letters whose
elements have blocks do.

**Say in the report** how many characters the test's call produced before and after, so the owner
knows what the rule costs on a clean signal.

## 4. Change two - printed stays printed

**Once a character is on the screen it stays there.** The transcript only ever grows.

- When keying goes false, characters **not yet shown** are dropped. Characters already shown are
  never removed, re-rendered away, or cleared with the tip.
- Whatever unit 486 clears on the falling edge must distinguish the two: the provisional tip that
  has not been seen, and text that has. **Only the unseen part is cleared.**
- The same holds for the scope: blocks and letters scroll off the left with time, never blink out
  because the detector let go.
- A `Clear` press by the owner still clears everything. That is his action, not the detector's.

**Watch it fail first**, headless: a call, then silence, and assert the transcript's text after
the silence contains every character it held during the call. Red while anything is removed.

## 5. Change three - one layout, everywhere

**The CW layout is the layout, at every frequency, in every mode, at every window width.** The
band row across the top, the neighborhood panel on the left, the map to its right, the rig
display on the right - the arrangement of the owner's screenshot at 14.069.2 in CW.

- **Nothing about the arrangement depends on the mode, the frequency, the block the frequency
  falls in, how much text the neighborhood panel carries, or the window width.** The panel's text
  fits the space it has - wrap, clip or scroll - and never resizes its column.
- **At 14.070.0 exactly**, the first hertz of the PSK31 block, the layout is identical to
  14.069.2 and 14.076.0. The owner's three screenshots show 14.070.0 alone rearranging.
- **A strayed frequency, a licence warning or any extra line does not move a column.** It appears
  inside the panel's own space.
- **Unit 389's width rule is superseded** - the rule that moved the sun map to the band's left
  edge above 1400 px, and dropped it back below. The map keeps one place. Say in the report which
  of 389's tests had to change, and change no assertion that is not about placement.

**Watch it fail first**, headless: build the window at two widths and at three frequencies -
14.069.2 CW, 14.070.0 USB-D, 14.076.0 USB-D - and assert every panel's position and size are
identical across all six. Red at HEAD, with the report naming what differs.

---

## 6. Record

- `PHASE_OUTCOME.md`, both copies: `## UNIT 487 - STEP 12`, one paragraph.
- `PHASE_STATUS.md`, both copies: names 487.
- Patch-bump `Directory.Build.props`.
- **Append to the rulings section of both `PHASE_PLAN.md` copies**, as paragraphs, no checkbox
  touched:
  - **R99** - a letter needs blocks, in the owner's words above;
  - **R100** - printed stays printed, in his words;
  - **R101** - one layout everywhere, in his words, naming that it supersedes unit 389's width
    rule.
- `DECISIONS.md`, newest first, **HM-DEC-192**, headline *A letter needs blocks, printed stays
  printed, and there is one layout*, quoting him on each, and naming that R101 supersedes 389's
  width rule.

---

## 7. Report

Section 2, for the owner, in plain words:

- rebuild;
- on a band with no CW: nothing on the scope and nothing in the terminal;
- on a station: blocks, letters over them, and **no letters that do not sit over blocks**;
- text once printed never vanishes, whatever the detector does;
- the window looks the same at 14.069, at 14.070, at 14.076, in CW and in data, narrow and wide.

Section 1: what changed, file by file, the element-to-block tolerance and its reason, and that
the build and the app line are green. Section 3: the before-and-after character counts from
change one's test. Section 4: anything left, a line each.
