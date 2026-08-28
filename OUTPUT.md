UNIT: 038 — stopped at task 4 of 8 — 2026-08-28 11:58

## 1. What Claude did

**Stopped. Tasks 1, 2, 3 and 4 are done; tasks 5, 6, 7 and 8 are blocked by one
finding, and it is task 1's.**

Claude Code, on the development computer, in `C:\Source\HamLet`. The prompt
claimed `PROJECT: Hamlet`; the tree confirmed all four checks — `SHACK_FACTS.md`
and `src/Hamlet.RadioEngine/Cw/CwProbabilisticDecoder.cs` exist, neither
`CoreHMI.sln` nor `MURC.sln` does. **Hamlet confirmed.** Branch `main`
throughout, four commits, all pushed, none refused. Version 1.12.0 to 1.12.1.

**Nothing here is evidence about the radio.** No rig was connected. **Nothing
transmitted and no rig setting was written** (§0.2).

**No decision was recorded under §12.1.** Section 4 carries what needs a ruling,
including the closing text for HM-OPEN-004 the order asks for.

### Why it stopped, and it is the answer the order asked for in task 1

**There is no C toolchain on this machine, so `ft8_lib` cannot be built here.**
Tim's ruling makes this a finding rather than a failure, and the order is
explicit that the report is worth more than a half-working interop layer. What
was tried:

- **Visual Studio 18 Insiders is installed and `cl.exe` for x64 exists**, at
  `VC/Tools/MSVC/14.51.36231/bin/Hostx64/x64`.
- **The install is partial in exactly the way that matters.** That toolset has
  **no `include` directory and no `lib` directory**. There is **no Windows SDK
  `Include` or `Lib`** anywhere under `Windows Kits`. And
  `VC/Auxiliary/Build` holds `vcvars64.bat` **without the `vcvarsall.bat` it
  calls**, so the environment cannot be set up at all.
- **A probe build was written and run**: a four-line C file to a DLL. It fails
  first at `cl` not resolving, and would fail next at `stdio.h`.
- **No other compiler is present** — no `gcc`, `clang`, `cc`, MinGW, LLVM, or
  `zig`.
- **`github.com` is reachable** (HTTP 200), so fetching the source is not the
  obstacle. **Building it is.**

`winget` is present, so a toolchain could be installed — **that is a change to
the operator's machine and not a session's to make**, and the order says do not
improvise.

**Tasks 6, 7 and 8 all sit downstream of a decoder.** Task 6 hands slots to one,
task 7 puts its output on screen, task 8 records its decodes in a sidecar. **A
slot cutter with nothing to hand slots to, and a decoded-text panel with nothing
to decode, would be scaffolding measured against nothing** — and the order's own
prohibition against scoring anything on synthetic audio is what makes that
worthless rather than merely premature.

## 2. What the owner should expect

**On the Digital tab, the waterfall now draws the radio's own audio.** It is a
real FFT of the codec stream, the same audio the CW decoder listens on, and with
no radio connected it still draws its own honest empty state.

**The decoded text panel is unchanged and still shows four placeholder rows**,
because no decoder exists to fill it. **That is the gap this unit could not
close**, and section 4 says what would.

**The slot grid is not drawn.** Its arithmetic is built and tested; what it needs
is the clock, and a grid at a guessed boundary is the one place a wrong picture
looks exactly like a right one.

**The clock offset is shown on the mode strip, beside unit 037's static status
rather than in place of it.** It reads `clock not checked yet, so slots cannot be
cut` until a query answers, goes amber past half a second, and says how old the
measurement is in words.

| | before | after |
|---|---|---|
| app | 509 of 509 | **509 of 509** |
| engine | 28 failing, byte-identical | **not re-run — see below** |

**The engine gained code and was not re-run whole.** The four new test classes
pass — 3, 10 and the spectrum trio — and no existing engine file was modified;
everything added is new files. **A full engine run is owed and is not in hand.**

## 3. What you should see

### The measurement the unit turns on: 2.93 Hz bins over 0.341 seconds

FT8 spaces its eight tones **6.25 Hz** apart. The transform window is chosen from
that and the numbers are stated rather than assumed:

| sample rate | window | covers | bin width | bins per FT8 tone |
|---|---|---|---|---|
| 8 kHz | 2048 | 0.256 s | **3.91 Hz** | 1.6 |
| 12 kHz | 4096 | 0.341 s | 2.93 Hz | 2.1 |
| 48 kHz | 16384 | 0.341 s | **2.93 Hz** | 2.1 |

**The window scales with the sample rate** so a fixture at 8 kHz and a radio at
48 kHz both see about a third of a second. **A wider window would resolve better
and blur the fifteen-second slot edges the grid exists to show**, and those edges
are what make FT8 recognisable at a glance.

**Determinism is asserted, not assumed.** The same fixture produces byte-identical
frames twice, **and produces them identically at chunk sizes of 4096 and 997** —
which is the property that matters, because a live source hands over whatever the
driver gives it and a picture that depended on buffer size would differ between
the radio and a replay of its own recording. 348 frames from a 30-second capture.

**A 1500 Hz tone lands at 1500 Hz.**

### Two things I got wrong and corrected on measurement

**The noise floor tracked the minimum bin.** On a clean tone the minimum is
numerical zero, around −240 dB, so the whole picture saturated and the 1500 Hz
tone read as **1289 Hz**. It is the twenty-fifth percentile of the visible span
now — the band between the signals, which is what a floor is.

**And my own test bar was wrong.** It demanded a bin narrower than *half* the
tone spacing and failed 8 kHz at 3.91 Hz. That was a bar I wrote as though it
were derived and it was not: two tones separate once the bin is narrower than
their spacing. Corrected to 6.25 Hz, with the reasoning in the test.

### The clock

| offset | reads |
|---|---|
| never measured | `clock not checked yet, so slots cannot be cut` |
| 0.00 s, just now | `clock matches UTC, checked just now` |
| 0.12 s, 40 min ago | `clock is 0.12 s slow, checked 40 minutes ago` |
| −0.50 s | `clock is 0.50 s fast, checked just now` — **amber** |

**Half a second, and the number comes from the mode**: FT8 packs 12.64 s of tones
into a 15-second slot, so there is about 2.3 s of slack, and half a second is
where the operator should be told while margin remains rather than after decodes
have started failing.

**Unknown is never zero.** A clock nobody has checked and a clock checked at no
drift are different facts, and only the second permits slots to be cut. **A reply
timestamp of zero is read as a refusal**, not as a date in 1900 — read as a time
it would report the clock as a century out.

The slot arithmetic is pure and tested without a network or a wall clock: a PC
time of 14:22:47 with a clock 2 s slow is really 14:22:49, its slot began at
14:22:45, and it is 4.0 s in.

### What task 1 found about the tree

- **`IAudioSource.SamplesReady` is an ordinary multicast event**, so the second
  consumer subscribes freely. **The CW audio path was not restructured and not
  touched.**
- **`ISpectrumSource` and `SpectrumFrame` already had the exact shape task 2
  needed** — bins as bytes 0–255, a low and high hertz, a timestamp, and
  `IsSimulated` carried on the source itself.
- **The engine had no FFT, deliberately.** `CLAUDE.md` §6 records that as an
  answer with a stated reopening condition — *"if phase 3 needs a wideband
  transform in software"* — and 450 bins across 2800 Hz is that condition. Forty
  lines, no dependency added.
- **Unit 037 created no digital view model**; the panels bind the main one.

### Where the order and the tree differ

- **Everything the order carried from 037's report was accurate.** The three
  sibling workspaces, `CollapsiblePanel`'s members including `HeaderAction`,
  `WaterfallControl`'s properties and its honest empty state, `ModePalette`'s
  brushes, the capture command's binding, and `DigitalIdleText` — all confirmed.
- **The app baseline is 509**, confirmed before any change.

## 4. What's blocking us

**The machine cannot compile C, and that decides the phase's direction.**

Closing text for **HM-OPEN-004**, for you to enter — the session mints no id:

> **`ft8_lib` is wrapped rather than shelled out to, and the wrap cannot be built
> on this machine.** Visual Studio 18 Insiders provides `cl.exe` for x64 with no
> CRT headers, no libs, no Windows SDK and no `vcvarsall.bat`; no other C
> compiler is installed. `github.com` is reachable, so the source is obtainable
> and only the build is blocked.
>
> **This is the condition Tim's own ruling named**: the wrapper is abandoned if
> it costs more than a session or two of fighting native binaries, and it has
> cost a toolchain that does not exist. **Two ways forward, and the choice is
> his** — install a C toolchain, which `winget` can do but a session may not; or
> **begin the C# implementation directly**, which is what he said he believed
> best at the outset.
>
> **What was rejected:** downloading a prebuilt native DLL, which the order
> forbids at any convenience; and installing a compiler unasked, which is a
> change to his machine.

**The C# route is not blocked by anything.** The FFT this unit added is the first
piece of it: an FT8 decoder needs a transform, a slot clock and a waterfall, and
**all three now exist and are tested.**

---

**A full engine run is owed.**

The engine gained four files and four test classes and was not re-run whole; the
new tests pass and no existing engine file was modified. **The failing count
should still be the stable 28 and that is an expectation, not a measurement.**

---

**Tasks 6, 7 and 8 were not attempted and that was a judgement.**

They are downstream of a decoder. Cutting slots to hand to nothing, and wiring a
capture press to record decodes that do not exist, would be scaffolding no
measurement could judge — and the order forbids scoring anything on synthetic
audio, which is all that would have been available. **If you would rather have
the slot cutter standing alone, it is a short unit.**

### Asks still outstanding

**Carried forward per HM-DEC-139 and HM-DEC-140, and deliberately not restated.**
The order parks the whole CW stream and the carried asks, and says *both halves
are required: do not touch them, and do not raise them.* **The thirty-one asks
from unit 1.11.34's list stand unchanged**, none touched by this unit.

**New this unit:**

1. **`ft8_lib` cannot be built here**, above — the HM-OPEN-004 closing text and
   the wrap-or-write choice.
2. **A full engine run is owed**, above.
3. **The slot cutter without a decoder**, above — offered, not taken.

**Closed this unit:** whether a second consumer can ride the audio tap — yes,
without restructuring anything. Whether the engine needs an FFT — yes, and §6's
own reopening condition names this case. What bin width FT8 needs — 2.93 Hz at
48 kHz, stated and tested.
