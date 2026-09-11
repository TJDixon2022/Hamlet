# Work instruction 312 - PSK31 exists as a mode

**READ IN THIS ORDER.**

A. **The phase goal - Hamlet works PSK31 the way it works FT8.** A third digital mode
   with the same two cards, the same one-click exchange, the same log and the same
   achievements, on a modem Hamlet builds itself.

B. **Step 0 and its exit criteria** - pressing PSK31 tunes to 14.070 USB-D; the panel
   names the mode; the log offers the mode; `BindingHealthTests` green; the mode is in
   the Digital family and draws with text colour only. Nice: the map's PSK31 ribbon
   lights when the tab is selected. **Every one of those is met, including the
   nice-to-pass**, and step 0 is recorded `done`. **Nothing was measured at a radio.**

C. **The report last, and section 4 raises 4 items** on top of a carried queue of eleven.

```
UNIT:       312 - complete at task 4 of 4, none dropped - 2026-09-11 01:52
PHASE GOAL: Hamlet works PSK31 the way it works FT8 - the same cards, the same one
            click, the same log, on a modem this project writes itself.
UNIT GOAL:  Step 0. Tell the whole application that PSK31 exists, take the radio to
            the cited watering hole when it is pressed, and say plainly that nothing
            can be read there yet.
ADVANCED:   yes - step 0, wholly. It is the first unit of the phase.
NUMBER:     version 1.12.274 -> 1.13.0; blocks picked out on the map 0 -> 1 of 13;
            the record's mode field on a PSK31 press Ft8 -> PSK31
DRIFT:      0 consecutive units without advance  (new phase)
```

**Every appearance claim in this report is computed, not seen.** Nothing in this
repository can look at a picture. What is asserted is a view-model value, a lookup, a
telemetry file and a rule the render asks - never that any of it looked right on your
screen. **And nothing here is evidence about the radio**: this is the development
computer, it has none, and no frequency was written and no audio was heard.

## 1. What Claude did

**The gate passed on all four checks.** `SHACK_FACTS.md` present,
`src\Hamlet.RadioEngine\Cw\CwProbabilisticDecoder.cs` present, no `CoreHMI.sln`, no
`MURC.sln`, root at `C:\Source\HamLet`. The prompt said `PROJECT: Hamlet` and so does
`PROJECT_CARD.md`. Branch **`main`**, five commits, each pushed before the next task
started.

**The three questions, one line each.**

- **Did unit 311 run? No.** Nothing in git history mentions it, `PHASE_OUTCOME.md` has
  no entry for it, and neither the zoom cap nor the For You scroll is in the tree. The
  last commit before this unit is unit 310's report. **Reported and not folded in**, as
  the instruction requires.
- **The version: 1.12.274 → 1.13.0.** The minor bumped for the new phase and the patch
  reset, per `PHASE_PLAN.md` §R7. `PROJECT_STATUS.md` carries no version field, so
  nothing there made it ambiguous. **One reading is worth stating**: HM-DEC-150 taken
  strictly says a phase bumps the minor and resets the patch to zero *and* each unit
  bumps the patch, which would make this unit 1.13.1. The instruction says bump and
  reset; §R7 says the first PSK31 unit lands on the new minor. I followed the
  instruction.
- **The cited band row says the PSK31 activity centre is 14.070 000 MHz exactly.** The
  20 m row in `data/bands/us-neighborhoods.json` is `lowHz 14070000`, `highHz 14074000`,
  `jumpHz 14070000`, family `digital`, cite `podxs070`. **The block is 4 kHz wide, not
  the 2.5 kHz §R6 describes** - see the mismatches below.

### Task 1 - the trace

The three phase files at the root are the PSK31 phase's, set 2026-09-11, seven steps all
`not started`; the FT4 phase's `PHASE_OUTCOME.md` was replaced, and its content is in git
history. `UNIT 312` is appended under step 0.

**The carry-forward list ran first, before anything changed: 19 named types, 106 of 106
green.** Nothing on it is red, so every red in this unit is this unit's.

**How FT4 was added, which task 2 copies** - and the finding is that PSK31 was mostly
added with it:

| Seam | Where | PSK31 before this unit |
| --- | --- | --- |
| The mode strip | `DigitalModeChip.cs:61`, `Labels` | **already there**, one of the four |
| The press | `MainWindowViewModel.cs:1109` `ChooseDigitalModeAsync` | **already generic** |
| The frequency | `DigitalCallingFrequencies.Find(band, label)` reading the cited rows | **already found** |
| USB-D | `ModeFollowPlan.cs:219` lists PSK31 with FT8, FT4, JS8 and RTTY | **already written** |
| The log | `ContactModes.cs:147`, `MODE=PSK` + `SUBMODE=PSK31`, ADIF 3.1.4 cited | **already there**, unit 287 |
| The decoder and grid | `DigitalMode` (engine), two members, and `DigitalModeFor` | **mapped onto `Ft8`** |
| The panel line | `DigitalIdleText.ModeStripFor(grid)` | **talked about slots** |
| The record's mode field | `_digitalMode.ToString()` | **said `Ft8`** |
| The map ribbon | `NeighborhoodMapControl.Render` | **no ribbon lit, for any mode** |

**So the fault was not a missing mode, it was a dishonest one.** Pressing PSK31 tuned
correctly to 14.070 and then ran FT8's slot grid and FT8's decoder on it, under a line
reading *nothing on this frequency yet. Slots here run 15 seconds, so give it a slot or
two before deciding the band is empty* - a sentence about FT8, on a mode with no slots,
telling the operator to wait for something that is never coming (§0.0, HM-DEC-092).

**`PHASE_PLAN.md` §1 and §R1, restated as asked.**

**§1 in my own words:** FT8 hands Hamlet a protocol - fixed slots, thirteen characters, a
grammar with six moves - so *whose turn it is* can be read straight off the wire. PSK31
hands it a conversation: two people typing at each other at 31 baud, in whatever words
they like, with no acknowledgement primitive and no fixed length. The convention most
operators follow looks like a QSO, but it is a convention and not a protocol, so Hamlet
will sometimes be unable to tell where an exchange has got to. **That is why the honest
display has a state called unknown**, and why "exactly like FT8" can be promised at the
product layer - the same cards, the same one click, the same log - and never at the wire.

**§R1 in my own words:** how much Hamlet is allowed to assert about an exchange it can
only half read, and the answer splits by consequence. **Anything that could put a signal
on the air is strict**: a macro is offered for one click only where the parser is certain
whose turn it is, because a wrong guess there transmits into somebody else's over. **What
the card merely displays is permissive**, because a card saying *unknown* through most of
a real QSO reads as broken - but a state that was inferred is visibly marked as inferred,
and not by colour alone.

**`SlotClock`** is bound in `MainWindow.axaml` on the digital card face and is untouched;
step 4 owns replacing it for this mode. **It is not shown for PSK31 today** because the
card panel it lives on is built from decoded rows and PSK31 produces none.

### Task 2 - the seam

`CanDecode` now names the two modes that have a decoder, rather than `DigitalModeFor`'s
answer being read as one. The strip line for a mode with no decoder says so by name. The
record carries the label the operator pressed as its own `state_changed` event, so a file
from a PSK31 evening can be told from an FT8 one - it could not before, because the mode
field is `DigitalMode`, which has two members and answered `Ft8` for both labels it does
not carry.

**Measured: the press goes to the cited row on all four bands that have one** - 3.580,
7.070, 10.130 and 14.070 - so no constant in code can satisfy the test.

**What was not built, deliberately:** no decoder, no modulator, no parser, no card, no
macro, no turn indicator, no achievement, no RST field, no ADIF submode added (the
submode was already there from unit 287 and was not touched). **The engine was not told a
tab exists** (§0.1): `DigitalMode` keeps its two members, and its own remarks say why a
third for a mode nothing can decode would assert a capability the application does not
have.

### Task 3 - the reference, pinned

`fldigi` cloned to `C:\Source\fldigi`, outside the tree, **never committed**, pinned at
**`61b97f4133c488063f3de1795c894d22d5032e8a`** - Version 4.1.23, 2022-06-23. **GPL-3**,
read from the clone's own `COPYING` rather than recalled, so it is compatible with
Hamlet's own licence. `docs/psk31-reference.md` records all of it, the rule that it is
read and never ported wholesale, and the two files at that commit that carry the varicode
table with the header recording the chain from Martinez through gmfsk to fldigi.
**Nothing was read from it in this unit.**

**The clone needed one exclusion.** `flarq_doxygen/user_src_doc/aux/ARQ2.pdf` cannot be
written on Windows, because `aux` is a reserved device name, so a plain `git clone`
reports `error: invalid path` and leaves an empty working tree. It is sparse - `src`,
`COPYING`, `README` - and the note says so, because the next person to clone it meets the
same error.

### Task 4 - the ribbon, taken rather than dropped

The map now picks out the block the operator asked for: an outline in the family's own
ink and a heavier label. **Not by colour** - every block is already filled from its
family, so a hue here would be a second language over the top of HM-DEC-032's, and a
reader who cannot separate two fills would be told nothing (§0.6). Measured on 20 m with
PSK31 chosen: **one block of thirteen picked out, PSK31 at 14.070.**

**Nothing was recorded in `DECISIONS.md`.** The instruction forbids it (§12.1).

### Four mismatches with the instruction, reported and not repaired

1. **No ribbon lit before this unit, for any mode.** The instruction says the PSK31
   ribbon should light *the way the FT8 ribbon lights at 14.074*. The map filled every
   block from its family and picked out none of them. Task 4 therefore **built the
   mechanism rather than copying one**, and it serves FT8 and FT4 in the same change.
2. **§R6 says the ribbon of signals runs from about 14.0700 to 14.0725. The cited row
   says 14.070 to 14.074.** The row is the source of record (HM-DEC-054) and is what the
   code reads; §R6's figure is not in the tree anywhere. The map's own tiling trims the
   block to 14.073999 so it does not overlap the FT8 block that starts at 14.074.
3. **The instruction's *do not add an ADIF submode* was already moot.** `ContactModes`
   has carried `MODE=PSK` + `SUBMODE=PSK31` since unit 287, cited to ADIF 3.1.4. Nothing
   was added and nothing was removed.
4. **The instruction carries its `Asks still outstanding` queue and is not defective**
   (§9.6) - noted because the previous unit's order was not, and the rule only works if
   the compliant case is confirmed as well as the missing one.

### Tests, all filtered by exact name, foregrounded, 480 s timeout

**No suite was run. No `dotnet test` on a whole project, and nothing was backgrounded or
polled** (HM-DEC-155).

| Test type | Result |
| --- | --- |
| `ThePsk31SeamTests` (new, tasks 2 and 4) | **7 of 7**, watched failing first at three of six |
| `ThePsk31ReferenceIsPinnedTests` (new, engine, task 3) | **2 of 2**, watched failing first |
| The carry-forward list, 19 types, run before anything changed | **106 of 106** |
| `BindingHealthTests.TheMainWindowBindsWithoutOneComplaint` | 1 of 1, re-run after each markup change |
| `VoiceTests` | 3 of 3, run because this unit adds copy the operator reads |
| `TheReadinessHoverTests`, `TheSlotClockTests` | 4 of 4, 3 of 3 |

**Nothing new is red.** The inherited reds in section 10 were not run and were not
chased.

## 2. What the owner should expect

**The build is clean** - zero warnings, zero errors, `TreatWarningsAsErrors` on.

**Press PSK31 and the radio goes to the PSK31 block of the band you are on.** On 20 m
that is 14.070 000 MHz; on 40 m, 7.070; on 30 m, 10.130; on 80 m, 3.580. The block asks
for USB with the data flag, which is USB-D, through the same mode-follow path FT8 uses.

**The panel then tells you it cannot read it.** It does not sit there looking like a
band that has gone quiet, and it does not talk about slots, because this mode has none.

**The map picks out the block you chose.** One outline, one heavier label, on the block
for the mode you pressed. This is new for FT8 and FT4 as well.

**What will look wrong and is not.** Nothing appears in the decoded table on PSK31: there
is no decoder, that is step 1, and the panel says so in words. The PSK31 block on the map
ends at 14.073 999 rather than 14.074: that is the map's own tiling keeping it off the
FT8 block, not a wrong row.

**What I cannot promise.** That any of it looks right, and anything at all about the
radio. No frequency was written, no audio was heard, and every claim above is computed.

**Pushed to `main`**, five commits, nothing uncommitted:
`c8da770`, `c5c0c96`, `fbce700`, `f00f645` and this report.

## 3. What you should see

**The PSK31 panel, word for word.** This is the line on the mode strip with PSK31
selected and nothing decoded:

```
the radio is on the PSK31 calling frequency and Hamlet cannot read PSK31 yet,
so nothing will appear below. You can still hear it, and it sounds like a
warble you could almost hum.
```

**And FT8's line, unchanged, for contrast:**

```
nothing on this frequency yet. Slots here run 15 seconds, so give it a slot or
two before deciding the band is empty.
```

**The tune line under the mode strip**, with nothing connected:

```
Nothing is connected, so the dial has not moved. PSK31 on 20 m is 14.070000 MHz
when a radio is.
```

**The map, on 20 m.** Thirteen blocks, one of them outlined and its label a shade
heavier:

```
  CW DX   14.000 000 - 14.024 999
  CW      14.025 000 - 14.059 999
  QRP     14.060 000 - 14.069 999
* PSK31   14.070 000 - 14.073 999      <- outlined, label heavier
  FT8     14.074 000 - 14.076 999
  JS8     14.078 000 - 14.079 999
  FT4     14.080 000 - 14.082 999
  RTTY    14.083 000 - 14.094 999
  ...
```

**One thing to check that I cannot.** Whether a two-pixel outline actually reads as
emphasis on a forty-four pixel map at your screen's scale. It is the one number in this
unit that was chosen rather than derived, and section 4 raises it.

## 4. What's blocking us

Nothing blocks step 1. Four items want your ruling.

1. **Unit 311 never ran, and its work is still outstanding.** The zoom cap and the For
   You scroll were delivered on 2026-09-11 and are not in the tree. **They are not folded
   in here**, per the instruction. They need a unit of their own or an explicit drop.

2. **`PHASE_PLAN.md` §R6 disagrees with the cited band row.** §R6 says the PSK31 ribbon
   runs from about 14.0700 to 14.0725; the row says 14.070 to 14.074. The code reads the
   row (HM-DEC-054) and §R6's figure is nowhere in the tree. Either the row is right and
   §R6 is prose to be corrected, or the row wants re-citing - and a band row is cited
   data, so that is not a session's to change.

3. **The outline width, 2 px, is a number I chose.** Everything else on the map derives
   from the band edges or the palette. A hairline vanishes against a filled block and
   anything heavier starts reading as a boundary of its own, but I cannot see the result.

4. **Version 1.13.0 or 1.13.1?** HM-DEC-150 read strictly gives the phase the minor with
   the patch reset to zero *and* each unit a patch bump, which makes the first unit of a
   phase 1.13.1. The instruction and §R7 say this unit lands on the new minor. I followed
   the instruction; one word settles which reading stands for the rest of the phase.

### Asks still outstanding

Carried per HM-DEC-139, verbatim where unresolved.

1. **Does the transmission record ask the radio whether it keyed?** Unit 303's proposal,
   still Tim's: `Played` stays a statement about what the audio path did; a second,
   separate fact says what the radio did, read from `1C 00` and `15 11`, which Hamlet
   already polls four times a second; **unknown** where the radio does not answer.
   **PSK31 sharpens it**: a continuous carrier that did not key is a long silence, not a
   missed slot.
2. **Nothing in this repository can look at a picture.** Real pixels want
   `Avalonia.Headless.Skia`, and **a package is Tim's, not a session's** (§0.4). This
   unit adds a third thing asserted from a rule rather than from a screen.
3. **Three inherited reds, never chased.** Two in `TheAchievementsScreenTests` -
   `WsprIsNotAFirstAnybodyCanEarnAndTheCardSaysSo` and `TheWindowDrawsEverySixRows` -
   and one in `TheFitGuardAsksAboutTheGridTheSendIsOnTests`, in the engine test project.
4. **Where the explanatory hover wording lives, if anywhere.** `Ft8ContactCard.Closing`
   is uncalled and left standing.
5. **`Ft8GlobePlot`'s unused framing constants.** Report; leave standing.
6. **The licence of `assets/world-flat-relief.png` is unknown.** See
   `assets/PROVENANCE.md`. Raise; do not resolve.
7. **The door sentence is a placeholder.** Carried.
8. **Acknowledgement indicators.** Named by Tim, not yet defined. **`PHASE_PLAN.md` §3.1
   builds a turn indicator in step 4, which may be what he meant. It is not assumed to
   be.**
9. **Card ordering under scroll.** Raised twice, unruled.
10. **The popup's size** - 720 by 400, a number unit 310 chose.
11. **Unit 311 may or may not have run.** **Answered by this unit: it did not.** It is
    carried as item 1 of the new asks above and drops off this queue.
