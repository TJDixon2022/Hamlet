# Work instruction 508 - a letter on the scroll stays on the scroll

**Hand run. One small unit. Display only.** The scroll draws a letter over its blocks and then
loses it on the next frame. The terminal has had printed-stays-printed since unit 487. The scroll
gets the same rule, from the same event.

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
  here; `-m` more than once for a multi-line commit. Scripts go in `.run-unit\unit508-<name>.sh`
  and are not committed.
- Nothing that keys or transmits. Nothing written to the radio.
- **Nothing in the engine changes.** `src\Hamlet.RadioEngine\Cw` prints nothing against entry.
- `output.md` at the root, four headings exactly: `## 1. What Claude did`, `## 2. What the owner
  should expect`, `## 3. What you should see`, `## 4. What's blocking us`.

---

## 2. What the owner sees

**His screen, 2026-09-30, W1AW fast code practice at 40 WPM, reading.** The terminal holds
`APPEARS IN THE SEPTEMBER 202DI IDUE OF QST ON PAGE 28. PRACTICE AT 35 TEM0 25 20 15`. The scroll
beneath shows a dozen groups of blocks and **three letters over them** - `A`, `N T`, `M`. Every
other group that became a letter in the terminal shows no letter on the scroll. And the letters
that do show **blink in and out**. His words: *"I don't like how the scrolling letters seem to
blink in and out depending on your confidence."*

**It is not confidence. It is redraw.** `CwScopeControl` rebuilds its letters every frame from the
reader's current state. Three things since unit 498 make the reader's state move after a letter
has been drawn: the banking holds `E` and `T` until confirmed (498), the pattern gate holds marks
until five stand (507), and unprinted runs are re-split when a sender qualifies (498). Each is
right for the terminal, where printed-stays-printed (R100) is enforced at the transcript. **The
scroll has no such rule**, so a letter the reader reconsiders vanishes, and a letter settled
between two frames may never draw at all.

---

## 3. The change

**The scroll obeys R100 exactly as the terminal does, from the same event.**

- **Every letter the terminal prints is drawn on the scroll, once, at the moment it prints**, over
  the blocks it was read from. The source is the event the transcript already settles from - one
  decoder, one truth (R106) - not a rebuild from the reader's state.
- **A letter drawn stays drawn until it scrolls off the left.** Nothing removes it: not a redraw,
  not the reader reconsidering, not a run being re-split. The blocks under it stay with it.
- **A letter is never drawn that the terminal did not print.** If the terminal shows `APPEARS`, the
  scroll shows `A P P E A R S` over the seven groups that made it, and nothing else.
- **Word gaps** draw nothing, as now.
- **The owner's `Clear`** clears the scroll with the terminal, as now.
- **Blocks are unchanged**: they come from the marks that stood, as unit 507 left them.

**Where the scroll keeps its letters:** a list the control owns, appended on the terminal's event
and trimmed only as time carries entries off the left edge. Say in the report what the frame was
rebuilt from before and what it reads from now.

**Watch it fail first**, headless, on the live path with synthetic hops:

1. **`CQ CQ DE N0CALL N0CALL K` at 20 WPM.** After the call, the scroll's letters are exactly the
   terminal's letters, in order, each over its own blocks. **Red today if any letter the terminal
   printed is missing from the scroll.**
2. **The same, sampled frame by frame:** once a letter appears on the scroll it is present on every
   later frame until it scrolls off. **Red today if any letter disappears and reappears, or
   disappears before the edge.**
3. **`TEST DE W1AW K`:** the banked `T` and `E` appear on the scroll at the moment they appear in the
   terminal, over their own blocks, and stay.
4. **The Farnsworth 5 WPM call:** letters that release late release onto the scroll late, in place,
   and stay.
5. **Loud noise:** no letter on the scroll, as none in the terminal.

---

## 4. Record

- `PHASE_OUTCOME.md`, both copies: `## UNIT 508 - STEP 12`, one paragraph.
- `PHASE_STATUS.md`, both copies: names 508.
- Patch-bump `Directory.Build.props`.
- `DECISIONS.md`, newest first, **HM-DEC-211**, headline *A letter on the scroll stays on the
  scroll*, naming that the scroll had been rebuilding its letters every frame while the terminal
  kept its own, and that both now read one event.
- **Touch no checkbox in `PHASE_PLAN.md`**, and add no ruling - R100 already covers it.

---

## 5. Report

Section 2, for the owner, in plain words:

- rebuild;
- every letter the terminal prints appears over its blocks on the scroll at the same moment, and
  stays there until it slides off the left;
- no letter blinks, and no group that became a letter is left bare;
- nothing about detection or decoding changed.

Section 1: what changed, file by file, what the frame read from before and now, and that the
build, the app line and the engine diff are clean. Section 3: the five cases. Section 4: anything
left, a line each.
