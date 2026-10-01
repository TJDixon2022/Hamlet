# Work instruction 515 - the shape is found wherever it appears

**Hand run. One unit.** The detector has had a watched bin since unit 476: one pitch it looks at
for its keying verdict and its blocks, and a chain of rules - unit 496, 507, 514 - for deciding
which pitch. Every pointing fault this week came from that bin being somewhere the station was
not. **This unit removes the watched bin.** Every bin, every hop, the same shape test; wherever a
rectangle appears, that is a mark, and its pitch is the row it was found in.

**No test against a recording, a fixture, a floor or copied telemetry** (R96). A headless test
driving synthetic hops written in the test itself is allowed; nothing read from disk. Verify by
building `Hamlet.sln` with warnings as errors and running the app carry-forward line. **Every
existing reading case reads exactly as at HEAD**, or the report says what changed and why.

**Numbering.** This is unit 515, ruling HM-DEC-219. If taken, use the next free and say so.

---

## 0. The project gate

```
STOP. Verify the project before reading any further.

PROJECT: Hamlet

Check the repository root:
  MUST EXIST:      SHACK_FACTS.md
  MUST EXIST:      src\Hamlet.RadioEngine\Cw\CwRunReader.cs
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
- Apostrophes in quoted heredocs break; `;` is refused; Python cannot run here; `-m` more than
  once for a multi-line commit. Scripts go in `.run-unit\unit515-<name>.sh`, not committed.
- Nothing that keys or transmits. Nothing written to the radio.
- `output.md` at the root, four headings exactly: `## 1. What Claude did`, `## 2. What the owner
  should expect`, `## 3. What you should see`, `## 4. What's blocking us`.

---

## 2. The owner's ruling, and what it ends

**His words, 2026-09-30, R114:**

> *"I'm wondering why we're so focused on pitch. Pitch almost doesn't matter. It's shape. If you
> can identify height, flat top, period, then you know it's a dot or a dash. The pitch doesn't
> matter."* And: *"You keep talking about 350, 400, 500, 600. Those are pitches. I just care
> about shape."*

**What the detector does now.** `CwEnvelopeDetector` keeps a **watched bin** - one pitch - and
measures its keying verdict, its light, its scope blocks and `MarksLast4s` there. Which pitch it
watches has been decided by, in turn: the survey (476), the station's own bin (496), the printed
sender (507's `Follow(PrintingHz)`), and the meter when the reader has nobody (514). **Every
pointing fault this week** - the detector on 600 while the meter had the station at 500, `tone
600 heard · decoding at 666`, keying at 350 while printing at 500 - **was that one bin being
somewhere the station was not.** The marks themselves were already found in every bin (490); the
pattern gate already takes candidates from every bin (507). **Only the detector's own verdict,
the light and the scope still think one pitch at a time.**

**The rule.** Height, flat top, period, sharp ends: that is a dot or a dash. **It is tested in every
bin on every hop.** Wherever it appears, that is a mark; its pitch is the row it was found in. No
watched bin. Nothing to point. Nothing to follow.

---

## 3. The change

### One - no watched bin

In `CwEnvelopeDetector`:

- **The keying verdict is: does any bin hold a sequence that stands** (the pattern gate's own
  test). Not: does the watched bin pair bars.
- **`Follow(pitch)`, `Pointed`, `WatchedHz`, and the choice of a watched bin go.** Unit 514's
  follow-the-meter wire, unit 507's follow-the-reader, unit 496's station's-own-bin selection for
  the *verdict* - all retire. The 496 peak walk that puts a **mark** on its lobe's peak stays; that
  is attribution, not pointing.
- **The reading's pitch is the pitch of the sequence that stands**, or of the loudest standing
  sequence where several do. NaN when none.
- **`MarksLast4s`** is the marks that stood in the last four seconds, at any pitch - or, when a
  sender is being printed, at that sender's pitch, as unit 507 left it.

### Two - the light and the scope read the standing sequences

- **The light** is lit when any sequence stands.
- **The scope's blocks** are the marks that stood for the sender being printed - unit 511's kept
  list, unchanged in rule, now fed from the standing sequences rather than from a watched bin.
- **The verdict row** loses `trackerHz`, `trackerHasPitch` and `trackerHasKeying` - they describe
  a bin nobody watches - and keeps `scopePitchHz`, `marks4s`, `mixingHz`. The key-set test is
  updated and says why.

### Three - the meter and the tracker stop steering anything

`CwKeyingMeter` and `CwToneTracker` no longer feed the detector, the light or the scope. **Leave
them in the tree** - unit 512's cleanup is still to run - but nothing on the screen's path reads
them. Say in the report what still constructs them and what still reads them, so 512 can take
them out.

### Four - what must not change

`CwRunReader` (units 500 to 513), the pattern gate's rules, the mark rules of 492, 496, 497, 498,
507 and 511, the terminal, the one layout, the tab-is-the-mode, the preamp, the buttons.

**Watch it fail first**, synthetic hops written in the test:

1. **The call at 20 WPM at 425, 500, 600, 700 and 775 Hz**, through a 500 Hz passband centred on
   600, **with no pointer and no follow**. Each reads whole, and the reading's pitch is the
   station's. **Red today at every pitch but 600**, or say so with the text.
2. **A station that changes pitch mid-transmission** - drifts from 500 to 560 Hz over the call, as
   a hand VFO does. Reads whole; the reading's pitch follows.
3. **Two stations 200 Hz apart**, as unit 507: the loud one prints whole, both stand.
4. **Every existing case reads exactly as at HEAD**: the calls at every speed, the Farnsworth
   cases, the speed change, the fists, the bursts, the hesitation, `TEST DE W1AW K`, `DE DE`, the
   lone and stray marks, both noise tests, the strength table, unit 511's quiet dit and dah.
5. **The verdict row on a driven station**: `scopePitchHz` equals the printed pitch; no tracker
   fields.

---

## 4. Record

- `PHASE_OUTCOME.md`, both copies: `## UNIT 515 - STEP 12`, one paragraph.
- `PHASE_STATUS.md`, both copies: names 515.
- Patch-bump `Directory.Build.props`.
- `CLAUDE.md` §1 index row.
- **Append R114 to the rulings section of both `PHASE_PLAN.md` copies**, in the owner's words
  above. **Touch no checkbox.**
- `DECISIONS.md`, newest first, **HM-DEC-219**, headline *The shape is found wherever it appears;
  the watched bin retires*, quoting him, and naming every pointing rule this supersedes: 476's
  survey choice, 496's verdict bin, 507's follow-the-reader, 514's follow-the-meter.

---

## 5. Report

Section 2, for the owner, in plain words:

- rebuild;
- **a station at any pitch inside the filter is found the moment it keys**, because the shape is
  looked for everywhere at once; nothing has to be pointed or to catch up;
- the light, the blocks and the row describe whatever is standing;
- nothing about how letters are read changed;
- **if a station still reads nothing, the pitch table in section 3 is what to report against.**

Section 1: what changed, file by file; what the meter and tracker still touch, for 512; and that
the build and the app line are green. **Section 3: the pitch table at the top, the drifting
station, then the existing cases.** Section 4: anything left, a line each.
