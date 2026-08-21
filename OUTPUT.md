# OUTPUT.md

## 1. What Claude did

### Task 1: what is trustworthy and what is not

Claude Code on the development machine, in `C:\Source\HamLet`, on `main`. The
prompt named `PROJECT: Hamlet`, all four gate checks hold, and **no radio is
attached** (HM-DEC-093), so every test drives rig state directly.

**`Overflow` is the one that is read often enough to be useful, and the one
nothing is known about.** It is `15 07`, cited to Full Manual 19-3, `00=clear,
01=overloading`. It is a **`Live`** field: asked for every **250 ms** and marked
stale after **1.5 s**, which is fast enough to catch a condition arriving. What is
**not** known is how it behaves normally — it has been seen reading `overloading`
exactly once, on the recording that prompted this unit, and nothing establishes
whether it flickers on strong signals, latches, or clears promptly. **That is
stated rather than assumed**, and it is why the message is written to stand while
the condition holds rather than to announce itself.

**`RfGain` is not trustworthy and is not on the panel.** It is `14 02`,
`00 00=minimum to 02 55=maximum`, a **`Session`** field read on connect and every
**30 s**. Tim has watched it report 100 per cent with the knob at noon. **It is
left off entirely** and nothing advises on it.

**The leading explanation is that the read is right and the interpretation is
wrong.** The IC-7300's RF/SQL knob can be configured as squelch only
(`1A 05 0025`, §4), and in that position the RF gain is held at maximum inside the
radio whatever the knob is doing. **Hamlet has a write for that setting and no
read**, so it cannot currently tell. Reading it would settle this, and that is a
later unit.

| field | command | rate | stale after | never read |
|---|---|---|---|---|
| `Overflow` | `15 07` | **Live**, 250 ms | 1.5 s | `unknown` |
| `Preamp` | `16 02` | Session, 30 s | 2 min | `unknown` |
| `Attenuator` | `11` | Session, 30 s | 2 min | `unknown` |
| `RfGain` | `14 02` | Session, 30 s | 2 min | not displayed at all |

A field never answered is `RigValue.Unknown`, whose text is the word `unknown` and
whose source reads `not read yet`. **Nothing defaults to off.**

### Task 2: the front end is on the panel

A chip beside the filter width and the speed, in the terminal's header row —
**where he is already looking while tuning**, not in a diagnostics screen he would
have to go and find. It reads `preamp 1 · attenuator off`, or
`preamp unknown · attenuator unknown` before either has been answered.

**When the radio says the front end is overloading the chip goes amber and leads
with it**: `overloading · preamp 1`.

**The transcript does not move.** The chip lives in the existing horizontal header
row and is always present — the overloading and the ordinary version are two
borders, one visible at a time, so nothing is added or removed from the layout.
The advice below the transcript goes into the fixed-height advisory region built
last unit, which swaps its contents rather than growing. `BindingHealthTests`
passes, so every binding in the rebuilt window resolves.

### Task 3: the advice names the button

With the preamp on:

> The radio says its front end is overloading, which means the signal coming in is
> stronger than the receiver can handle and everything in the passband is being
> squashed together. Nothing will decode until that stops. Press P.AMP/ATT on the
> front of the radio until the preamp reads off.

With the preamp already off:

> The radio says its front end is overloading, and the preamp is already off, so
> the next thing to try is the attenuator. Hold P.AMP/ATT for a moment to bring it
> in. A strong band in daylight can do this on its own.

It sits second in the advisory order, behind only the transmit notice, because it
is the one condition that stops the band being readable at all and it is one press
from being fixed. **It says it once and lets it stand**: the sentence does not
change while the condition holds, so nothing blinks at him four times a second.

### Task 4: the tests

Six in `TheFrontEndIsOnThePanelTests`, all green:

| | result |
|---|---|
| overflow asserted shows the message, clearing removes it | asserted, and the message names `P.AMP/ATT` |
| the preamp first, the attenuator only after it | preamp 1 and 2 name the preamp and not the attenuator; preamp off names the attenuator |
| a setting never read says unknown | `preamp unknown · attenuator unknown`, and never the word `off` |
| nothing advises on RF gain | asserted on every branch |
| **nothing this unit added writes to the radio** | a sweep of every `.cs` under `src/Hamlet.App` for a write of `Preamp`, `Attenuator` or `RfGain` finds none |

**HM-DEC-120 still holds**, run rather than assumed: twenty-two tests covering
`NothingIsEmittedAnywhereBelowTheFloor`, both recordings holding no keying, the
whole probabilistic decoder suite and the transmit-suspension suite — all pass.
**The decoder was not touched.**

### Task 5: the ruling is HM-DEC-148

**Established from both places, as instructed.** `DECISIONS.md` holds 001–095 then
134–147; `CLAUDE.md` §1 holds index rows up to 147; a sweep of every `.md`, `.cs`
and `.axaml` in the tree finds nothing above 147. **148 was free** and is recorded
in `DECISIONS.md` with an index row at the top of §1, which `DecisionLogOrderTests`
and `VoiceTests` both pass on.

## 2. What Tim should expect

**Next time the front end overloads, the chip beside the filter width turns amber
and reads `overloading · preamp 1`, and the message under the transcript tells you
to press P.AMP/ATT until the preamp reads off** — instead of the answer sitting in
a capture sidecar you read the following day.

And when it is not overloading, that same chip quietly says what the preamp and
attenuator are set to, or says `unknown` if the radio has not answered yet.

### The failing-test set, before and after

**55 before, 58 after. Nothing that was failing now passes. Three are new, and all
three are `HM-OPEN-055`:**

- `RigReadTests.AReadThatTimesOutMarksTheValueUnknownWithoutThrowing`
- `RigReadTests.TheSMeterParsesAgainstTheManualsAnchors(high: 0, low: 0, expected: "S0")`
- `RigReadTests.TheSMeterParsesAgainstTheManualsAnchors(high: 2, low: 65, expected: "S9+60")`

**All 31 tests in that class pass when the class is run on its own**, immediately
afterwards. Nothing in this unit touches rig reading, the S-meter or timeouts, and
`RigReadTests` is one of the four classes `HM-OPEN-055` already names as flaking
under a full-solution run. **Reported rather than chased**, as the instruction
says.

Six tests added, all green. Build clean, no warnings. Pushed to `main`.

### What is deliberately absent

- **`RfGain` is not shown and not advised on.** See section 1.
- **Nothing writes to the radio.** Not the preamp, not the attenuator, not as a
  fallback. There is no button to press in Hamlet; the button is on the radio.

## 3. What we should do next

- **Read `1A 05 0025`**, the RF/SQL knob's function. One read settles whether
  `RfGain`'s 100 per cent is a defect or the correct answer for a knob configured
  as squelch only, and it is the only thing standing between that figure and the
  panel.
- **Watch `Overflow` over an evening.** It has been seen asserted once. Whether it
  flickers, latches or clears promptly decides whether the message needs any
  hysteresis at all.
- **The mode-follow regression**, which is parked here and is its own unit.

## 4. What's blocking us

Nothing blocks the next unit.

**One ask, new this session.**

> **Whether `RfGain`'s hundred per cent is a defect or the right answer, and
> whether Hamlet should read `1A 05 0025` to find out.**
>
> The IC-7300's RF/SQL knob can be configured as squelch only (§4,
> `1A 05 0025`), and in that position the RF gain really is held at maximum
> whatever the knob is doing — so a read of 100 per cent with the knob at noon
> would be **correct**, and the fault would be Hamlet showing a number without
> saying which world it is in. **Hamlet has a write for that setting and no
> read**, so it cannot currently tell.
>
> Adding the read is a small thing. What it is not a session's to decide is
> whether `RfGain` then goes on the panel, because that is a number the operator
> has already watched contradict his own radio, and putting it back rests on a
> theory rather than on a measurement until somebody looks at the menu.

### Asks still outstanding

- **Whether `RfGain`'s hundred per cent is a defect or the right answer.** First
  made 2026-08-21, this session. Waiting on Tim, and on one read of `1A 05 0025`.
- **The copy-speed control: make it live, or remove it.** First made 2026-08-21.
  Waiting on Tim. It is inert; the new decoder reads no seed.
- **The likelihood gate at 15.0 wants an evening's captures scored against it.**
  First made 2026-08-21. Waiting on one evening at the rig.
- **Three recordings named in an earlier instruction are not in the tree**
  (`cw-2026-08-21-015834`, `-020033`, `-015432`). First made 2026-08-20. Waiting
  on the files.
- **The keying meter's provisional thresholds**, including
  `CwKeyingThresholds.ConfidentSwingDb` at 20 dB. First made 2026-08-20. Waiting
  on one evening's roster scored against the `meter` column.
- **HM-DEC-130, whether a message too long for one keyer send may be split.**
  First made 2026-08-18. Waiting on the seam measured into the dummy load.
- **HM-DEC-098, whether §0.2's first sentence is amended to permit an attended
  automatic transmit cycle on the air.** First made 2026-08-17. Waiting on every
  interlock watched to fire into the dummy load. **This unit wrote nothing to the
  radio and touched no interlock.**
- **HM-OPEN-033, the cold-start bin choice and `prosigns-easy`.** First made
  2026-08-18; HM-DEC-129 scheduled it rather than closing it.
- **HM-OPEN-007.** Open and unruled since 2026-08-14. Waiting on Tim.

**Nothing leaves the queue this session.**
