# Work instruction 010 — bank the evening, then fix the six things that break it

## 1. What Claude did

Claude Code on the development computer, `C:\Source\HamLet`. The prompt claimed
`PROJECT: Hamlet` and so does `WORK_INSTRUCTIONS.md`; the tree confirms it —
`SHACK_FACTS.md` and `src/Hamlet.RadioEngine/Cw/CwProbabilisticDecoder.cs` exist,
neither `CoreHMI.sln` nor `MURC.sln` does, the solution is `Hamlet.sln`, and
`PROJECT_CARD.md` names Hamlet. **Branch `main`**, per §9.5.1. Seven tasks:
**task 1 could not run, tasks 2 through 6 ran, task 7 was dropped.** Every push
succeeded; none was refused.

**Nothing in this report is evidence about the radio.** No rig was connected.

**Nothing was recorded to `DECISIONS.md`.**

**Report shape**: `CLAUDE_CODE.md` §8 says **four** sections; its version line
still reads 1.3. Followed the section count. Tenth consecutive unit naming it.

### Task 1 — the evening is unbanked

**The nine captures are not in the tree.** `cw-2026-08-25-011552` through
`-013520` are not under `tests/fixtures/cw/captured/`, not elsewhere in the
working tree, and not in git history. Section 4 leads with it, as instructed.

**Green baseline: 33 failing of 1607 in the engine, 480 of 481 in the app** —
matching the instruction, with the app's one being the flaky test.

### Task 2 — it was the badge, and there is a second one

**Which of the two candidates it was: the badge.** `SelectBand`
(`MainWindowViewModel.cs:4590`) is unconditional — it always sets the band and
the frequency — so the stale-state hypothesis is ruled out.

**The badge caused two faults from one line.** It was `Grid.Row="0"` above the
card, so a band carrying it had its card pushed down while every other card
stayed at the top. And it is centred on a card whose width follows the
wavelength (HM-DEC-141), so on a narrow card the badge is **wider than the thing
it labels** and hangs over its neighbours, in front of them, taking their clicks.

It now shares the card's cell, sits over the top of it, and is
`IsHitTestVisible="False"` — a click on a label is a click meant for the control
underneath. The reason it carried in its own tooltip moved to the card's,
because a thing that cannot be hovered cannot explain itself.

**A second obstruction of the same shape turned up while testing and is not the
badge**: at the headless default width, the rig readout covers `10 m` entirely.
At 1400 wide all seven cards answer their own clicks. Outside the ruling I was
given, so logged as **HM-OPEN-060** rather than changed.

### Task 3 — the gate is running, and `■` is it working

**Answered the first question first, because it decides the size of the task.**
The margin is wired: `CwProbabilisticDecoder.cs:1151`,
`character.SpanMargin >= CharacterMargin`.

**`■:-93.4` is the gate catching that character, not one that passed it.** `■` is
the unresolved placeholder — HM-DEC-048's third confidence state — so a sheet
reading `■:-93.4` is Hamlet marking a character whose span silence explains
better. The instruction's premise reads the sheet backwards.

**So the redesign is not needed in the form written**, and both normalisations
were measured anyway because the instruction asks for them:

| elements | raw median | per element | per keying unit |
|---|---|---|---|
| 1 (`E`, `T`) | 9.9 | 9.9 | 9.7 |
| 2 | 215.8 | 107.9 | 61.0 |
| 3 | 420.8 | 140.3 | 93.6 |
| 4 | 697.4 | 174.4 | 115.0 |
| 5 | 928.6 | 185.7 | 104.2 |

**Neither normalisation makes one-element characters comparable.** Per keying
unit is the better divisor — a dah is three dits and carries three dits'
evidence — and it still leaves `E` and `T` at about a tenth of everything else.
**If a positive margin is ever ruled, neither divisor on its own is enough.**

Nothing shipped: the constant is Tim's and its motivating fault dissolved.
33 of 749 characters sit below the margin today, 4.4 %.

### Task 4 — the elements were never counted

`cw-2026-08-25-012748` is not in the tree either, so this was diagnosed from the
code, with file and line as asked.

**The elements are not lost downstream. They were never counted.**
`_elementsResolved` increments only inside the `CharacterSettled` handler at
`CwDecoder.cs:129`, and it adds **the emitted character's own pattern length**,
not marks in the audio. The same field goes out as both `ElementsSeen` and
`ElementsResolved` at `:247` and `:248`.

So *2 characters from 6 elements* says two characters were emitted whose dits
and dahs totalled six. Comparing that with 113 marks found independently
compares what was emitted against what was sent. What is left of the question is
why only two characters emitted, and that is the window guard — unmeasurable
without the file.

### Task 5 — one voice

The advisory already returns only its first non-empty line. What sat beneath it
was the keying meter's own block, visible whenever decoding. It now waits while
the advisory has something to say, and speaks when the advisory is silent.

**The meter is not retired and must not be.** It is the one instrument sharing
nothing with the decoder, and on `cw-2026-08-22-012823` it found the right
frequency while the decoder took the wrong one.

### Task 6 — the ceiling

`FastestWpm` was 32 while its own remarks argued for forty — HM-OPEN-058, logged
2026-08-23 and parked in every unit since. Raised to **40**, and the capture
sheet now says when the winning speed sits at either end of the search.

### Task 7 — dropped

**Dropped whole.** Its acceptance is specified as *"Fix it against the nine
fixtures banked in task 1"*, and task 1 could not bank them. Fixing the sweep
against the eleven captures that *are* here would be fixing it against different
evidence from the one the instruction names, and the sweep's faults were
measured on the nine.

Task 5 removes the on-screen harm in the meantime, exactly as the instruction
anticipated.

## 2. What Tim should expect

**Can the operator click the 40 m button?**

# **Yes**

All seven band cards answer a click on their own centre, verified by hit-testing
the real window headless at the application's own width. `40 m`, `30 m` and
`20 m` — the three the badge was covering — all reach their own card.

**One caveat, and it is a different fault**: at a *narrow* window the rig readout
covers `10 m`. Logged as HM-OPEN-060, not fixed, because it is outside the
ruling I was given.

**What else changed at the radio:**

- **The keying meter's block no longer argues with the advisory above it.** When
  the advisory has something to say, the block waits its turn.
- **The capture sheet says when the speed is at the edge of the search** — *"AT
  THE TOP OF THE SEARCH: the sender may be faster than Hamlet can look"*. It
  would have said that on both of the 2026-08-25 captures that reported 32.
- **Speeds up to 40 words a minute are now inside the grid**, where the ceiling
  was 32.
- **The best-bet badge no longer explains itself on hover** — its reason moved
  into the band card's own tooltip, because a label that takes no clicks takes
  no hovers either.

**What will look wrong and is not:**

- **32 failing of 1608 in the engine, 482 of 483 in the app.** The engine is
  **down one** from the 33 baseline and nothing new broke.
- **Raising the ceiling turned `AFastFistIsReadWithoutARunUp(35 wpm)` green**,
  red since unit 002. That fist was outside the grid rather than beyond the
  decoder.
- **The app's one failure is the known flaky test.** Two tests were added.
- The two accepted-cost fixtures and the flaky rig test are untouched, as
  instructed.

## 3. What you should see

**The evening's nine captures scored against their banked floors, before and
after this unit:**

# **not measurable — the evening is unbanked**

The nine captures of 2026-08-25 are not in the repository, so there are no
floors, no before, and no after. **The 59 characters at 32 WPM, the capture where
Hamlet beat the independent chain, and the negative control that separates a real
improvement from a trade are all unprotected**, and a later unit can lose any of
them without anything failing.

**What can be shown instead, from the eleven captures that are here**, is that
this unit changed no decode at all. The engine's failing set went from 33 to 32,
the one that changed went green, and **no capture's output moved**:

| | baseline | after |
|---|---|---|
| engine tests failing | 33 | **32** |
| app tests failing | 1 (flaky) | 1 (flaky) |
| newly red | — | **none** |
| newly green | — | `AFastFistIsReadWithoutARunUp(35)` |

Tasks 2, 5 and 6 touch the panel and the sidecar; task 6's ceiling is the only
engine change, and it can only add speed hypotheses above 32 that nothing in the
corpus reaches. **Both empty captures still emit nothing** and
`ARecordingWithNoStationInItSaysNothing(014854)` is still green.

**The three findings that came out of the measurements:**

1. **`■` is the gate working.** The premise that a ruled margin was being
   bypassed is a misreading of the placeholder.
2. **Neither proposed normalisation fixes the short-character bias** — `E` and
   `T` stay at a tenth of everything else under both.
3. **The sidecar's element count has never counted marks**, so the 6-against-113
   discrepancy is two different quantities.

## 4. What's blocking us

---

**The evening of 2026-08-25 is unbanked and can still be lost.**

`cw-2026-08-25-011552` through `-013520` — nine captures with their sidecars —
are not in the tree. Task 1 exists because that evening is the first time Hamlet
read a rag chew end to end and beat the independent chain on a capture, and
**nothing in the repository protects any of it.**

**What is needed**: the nine WAVs and their `.txt` sidecars dropped into
`tests/fixtures/cw/captured/unadjudicated/`. The harness to score them and write
the floors is a short task once the audio is here.

**And task 7 is blocked behind the same gap**, because its acceptance is defined
against those nine.

---

**A second obstruction covers the last band card on a narrow window.**

At the headless default width, a hit test at `10 m`'s own centre reaches a
`Border` belonging to the strip rather than the card. At 1400 wide it does not.
The band row sits in a star column beside the readout's auto column, and the
cards do not shrink because their widths carry the wavelength ratio, which is the
meaning rather than the size (HM-DEC-141).

Logged as **HM-OPEN-060**. What the row should do when it runs out of room is a
display question and therefore Tim's.

---

**Task 3's redesign was aimed at a fault that does not exist, and the real
question it raises is unanswered.**

The gate runs and `■` is its output. But the measurement it prompted stands on
its own: a flat *positive* margin would punish short characters, and **neither
normalisation the instruction proposed fixes that**. Per keying unit is better
than per element and both leave `E` and `T` an order of magnitude below.

So if a positive margin is ever wanted, it needs something neither divisor
supplies — a per-character expectation rather than a per-character division.
Nothing was shipped and the ruled margin of nought stands.

---

**The best-bet badge can no longer be hovered, and its tooltip moved.**

Making it take no clicks also makes it take no hovers. Its reason now appears at
the end of the band card's own tooltip. That is a small change to what the panel
says and the panel is Tim's; it is flagged rather than assumed.

### Asks still outstanding

Carried forward verbatim per HM-DEC-139 and HM-DEC-140. **Eleven inbound, none
ruled, the oldest open since 2026-08-14. Twelve consecutive units have now worked
beside rulings they cannot read.**

1. **The sweep's `invented` column counts substitutions, not invented
   characters.**
2. **Whether the refill guard should apply to the first fill at all.**
3. **`ANNUNCIATOR.md` renamed `PHASE` to `TASK` while HM-DEC-150 makes `PHASE`
   match the version's minor.**
4. **`DECISIONS.md` has no record for HM-DEC-096–133, 136, 141 or 150.**
5. **The tone tracker** — narrowed by unit 1.11.6's hold, not closed.
6. **Whether the integrator ships at 45 Hz or 30 Hz** — bears on `014113` and
   `014308`.
7. **The guard's gap is two to one**, calibrated on two empty captures.
8. **A boxcar's nulls made two of five swept offsets pathological best cases.**
9. **Two stations closer than 125 Hz are not named and the operator is not told
   they are not named.**
10. **The keying witness is wrong more often than right** — 6 agreed, 11
    contradicted. *Task 7 dropped; task 5 removed the on-screen harm.*
11. **HM-OPEN-057** (2026-08-22) and **HM-OPEN-007** (2026-08-14).

Still open from earlier units: **the lock helping sometimes and hurting sometimes
with nothing telling the operator which**; **the "Hold this pitch" button added
against instruction**; **two clean fixtures dropped from 9 of 9 for containing
exact digital silence**; **`001520` scoring in the quadrillions**; **the port and
its reference differing by an integrator**; **`CLAUDE_CODE.md` changing its report
contract without moving its version line**; **refusing an unmeasured pitch costs
`N4L`**; **a second mechanism silences `014113` and `014308`**; **two pitch
measurements disagree by six hertz depending on the window**; **seven adjudicable
W1AW fixtures are unadjudicated.**

New from this unit: **the evening of 2026-08-25 is unbanked**; **HM-OPEN-060, the
readout covering the last band card on a narrow window**; **neither normalisation
fixes the short-character bias**; **the badge's tooltip moved to the card**.

**Build 1.11.7**, confirmed in `Directory.Build.props`, up from 1.11.6.
