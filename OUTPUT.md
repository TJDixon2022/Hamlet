# OUTPUT.md

## 1. What Claude did

### Task 1: the named cause

Claude Code on the development machine, in `C:\Source\HamLet`, on `main`. The
prompt named `PROJECT: Hamlet`, all four gate checks hold, and **no radio is
attached** (HM-DEC-093), so every test drives state directly.

**`ModeFollowPlan.cs:228`, added 2026-08-18 as the fix for the sixty-six-second
incident:**

```csharp
if (workingCw && target.Mode != CivMode.Cw)
{
    return ModeFollowDecision.Nothing;
}
```

fed by `MainWindowViewModel.cs:4966`:

```csharp
var workingCw = IsDecoding || IsInsideCwSegment;
```

**`IsDecoding` is true from the moment the decoder starts listening until it
stops.** It is set once at `3244` and cleared at `3299`; it means the decoder is
switched on and says nothing about whether anybody is sending. So `workingCw` was
**permanently true** whenever audio was connected, and **every target that was not
CW was refused, forever.** At 14.243 MHz the map says upper sideband, the target
was USB, and the decision was `Nothing`.

**The write was attempted and refused, not never attempted.** Both tuning paths
reach it:

| path | route |
|---|---|
| the dial | the radio's own report → `ApplyRigFrequency` (`6058` broadcast, `3032` poll) → `FrequencyHz` → `OnFrequencyHzChanged` → `ScheduleModeFollow()` at `4748` |
| the band map | a tune command → `ApplyRigFrequency` at `4624` → the same property → the same call |
| a band button | `OnSelectedBandChanged` → `Rearmed()` → `ScheduleModeFollow()` at `2562` |

**Every guard, and which one was stopping it:**

| guard | where | added for | firing? |
|---|---|---|---|
| `Enabled` off | `ScheduleModeFollow` | the operator's setting, ships on | no |
| `Suspended` | `NoticeOperatorModeChange` | HM-DEC-056, the operator's own hand | no — and `_settingModeOurselves` correctly stops Hamlet's own write suspending it |
| no rig / not connected | `ScheduleModeFollow` | — | no |
| **`workingCw`** | `Decide:228` | **2026-08-18** | **yes, always** |
| already in that mode | `Decide:237` | HM-DEC-050 | no |
| already done here | `Decide:249` | HM-OPEN-041, the snap-back | no |

**What the 18th's fix changed** is exactly this: closing the snap-back added the
`DoneAtHz` memory *and* this guard, and the guard was handed evidence that is
always true. The snap-back memory is sound and untouched.

### Task 1, second half: what the previous unit actually did

**The panel work was done, it does render, and it is in the right place.** The
chip is inside `widget.terminal` in the terminal's own header row beside the
filter width, not in the diagnostics dialog.

**What was wrong is that the readings were unlabelled.** The chip took
`RigValue.Text` straight from the radio, and the radio's own word for both the
preamp and the attenuator when they are off is the single word `off` — so it read
`off · off`, which says two things are off and nothing about which two. **A
reading nobody can interpret is the same failure as a reading nobody can find**,
and from the operator's chair those are indistinguishable.

### Task 2: it follows again

`workingCw` now asks two things that are actually about Morse: **the dial inside a
CW segment, or a character having come through in the last half minute.** The
guard itself is unchanged and the snap-back memory is untouched.

Half a minute because an exchange has gaps of several seconds between overs and a
slow sender leaves long ones inside a message, and because a station that finished
five minutes ago should not still be pinning the mode. **The clock is not seeded
when listening starts** — a decoder that has just been switched on has read
nothing, and treating that as somebody working Morse is the defect itself.

**Only the mode is written**, and it says so: `Switched to USB, which is how voice
is worked up here.`

### Task 3: the front end says which setting is which

The labels are now composed from the radio's number rather than taken from its
word:

| preamp | attenuator | chip |
|---|---|---|
| off | off | `preamp off · att off` |
| 1 | off | `preamp 1 · att off` |
| 2 | 20 dB | `preamp 2 · att 20 dB` |
| never read | never read | `preamp unknown · att unknown` |

**`on` never appears**, because preamp 1 and preamp 2 are different settings on
this radio and an operator judging his own front end needs to know which. When the
radio reports overload the chip goes amber and leads with it —
`overloading · preamp 1` — and the advice below the transcript names **P.AMP/ATT**,
mentioning the attenuator only once the preamp is already off. `RfGain` is not
shown and nothing advises on it.

### Task 4: the tests

Twelve in `ModeFollowsTheMapAgainTests` plus the six from the previous unit, all
green:

| | result |
|---|---|
| tuning into the phone portion writes the mode | `Switched to USB, ...`, mode `Usb`, data variant off |
| both tuning paths reach mode-follow | asserted in `OnFrequencyHzChanged` and `OnSelectedBandChanged` |
| a snap-back does not write again | second decision at the same frequency declines |
| a mode he set by hand is not overwritten | and a band change re-arms it |
| **nothing but the mode is ever written** | a bounded sweep of the 74-line follow path finds `SetModeAsync` and none of `SetFrequencyAsync`, `SetFilter`, `SetPower`, `SetGain`, `SetPreamp`, `SetAttenuator`, `WriteAsync` |
| every front-end reading carries its own name | four cases, and `· on` never appears |
| a setting never read says unknown | `preamp unknown`, `att unknown` |
| the front end is on the terminal panel | `FrontEndText` found inside `widget.terminal` |

**HM-DEC-120 still holds**, run rather than assumed: sixteen tests covering
`NothingIsEmittedAnywhereBelowTheFloor`, both recordings holding no keying and the
whole probabilistic decoder suite — all pass. **The decoder was not touched.**

`BindingHealthTests` and `VoiceTests` both pass.

### Task 5: the ruling is HM-DEC-149

**Established from both places, as instructed.** `DECISIONS.md` holds 001–095 then
134–148; `CLAUDE.md` §1 holds index rows up to 148; a sweep of every `.md`, `.cs`
and `.axaml` in the tree finds nothing above 148. **149 was free** and is recorded
with an index row at the top of §1, which `DecisionLogOrderTests` passes on.

The front-end half was already recorded yesterday as **HM-DEC-148**; 149 records
mode-follow.

## 2. What Tim should expect

**Tune to 14.243 now and the radio goes to USB, with the status line saying
"Switched to USB, which is how voice is worked up here."** — unless you are
inside a CW segment or a character has come through in the last thirty seconds, in
which case Hamlet leaves your radio alone exactly as it has since the 18th.

**And the front end on the terminal reads, verbatim:** `preamp off · att off`, or
`preamp 1 · att off`, or `preamp unknown · att unknown` before the radio has
answered — and `overloading · preamp 1` in amber when the front end is being
overdriven.

### The failing-test set, before and after

**55 before, 55 after, and the sets are identical.** No test that was passing now
fails and none that was failing now passes. Eighteen tests were added across this
unit and the last, all green.

Build clean, no warnings. Pushed to `main`.

### What to watch for

- **Mode-follow will not fire while a station is being copied**, for thirty
  seconds after the last character. That is the guard doing its job, not a
  failure.
- **It still stands down entirely once you set a mode by hand**, until the next
  band change, and it says so when it does.

## 3. What we should do next

- **Watch mode-follow across an evening.** Thirty seconds is reasoned from how an
  exchange sounds and has never been measured against one.
- **Read `1A 05 0025`**, which would settle whether `RfGain`'s hundred per cent is
  a defect or the right answer for a knob configured as squelch only.
- **The fifty dead tests**, which are the only thing between this project and a
  suite whose count means something.

## 4. What's blocking us

Nothing blocks the next unit.

**One ask, new this session.**

> **Thirty seconds since the last character is reasoned, not measured.**
>
> `workingCw` now means the dial is in a CW segment or a character arrived within
> the last thirty seconds. The figure comes from how a contact sounds — gaps of
> several seconds between overs, longer ones inside a slow sender's message — and
> nothing in this repository measures how long a real exchange goes quiet.
>
> **The failure it would cause is asymmetric and worth naming.** Too short and
> Hamlet changes mode in the middle of a contact, which is the sixty-six-second
> incident again. Too long and it declines to follow the map for a while after a
> station stops, which costs nothing but a manual mode change.
>
> **One evening's captures would settle it**: the roster already records when each
> case was marked, and the gap between characters inside a capture is measurable
> from the recordings themselves. Not a session's to pick, because it is a number
> that decides when the app moves his radio.

### Asks still outstanding

- **Thirty seconds since the last character, for mode-follow's guard.** First made
  2026-08-21, this session. Waiting on one evening's captures.
- **Whether `RfGain`'s hundred per cent is a defect or the right answer.** First
  made 2026-08-21. Waiting on Tim, and on one read of `1A 05 0025`.
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
  interlock watched to fire into the dummy load. **This unit did not transmit and
  touched no interlock.**
- **HM-OPEN-033, the cold-start bin choice and `prosigns-easy`.** First made
  2026-08-18; HM-DEC-129 scheduled it rather than closing it.
- **HM-OPEN-007.** Open and unruled since 2026-08-14. Waiting on Tim.

**Nothing leaves the queue this session.**
