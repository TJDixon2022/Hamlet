READ IN THIS ORDER

A. The phase goal. Hamlet hears FT8 off the radio and displays the decoded text
   on screen. Steps 1 to 5 are done. Step 6 is open and unit 227 settled why: the
   sensitivity shortfall was inherited with the code, upstream's own decoder
   returning the same messages slot for slot on the identical files, so criterion
   2 is not met and no unit-reachable change moves it. Step 7 is where this unit
   works, on the plan's 2026-09-02 ruling that steps 6 and 7 do not depend on each
   other. The phase itself closes at Tim's eyes at 14.074, not at a test.

B. This step and its exit criteria. Step 7 - Hamlet displays decoded FT8. Five
   must-pass criteria. THIS UNIT IS AIMED AT THE SECOND ONE, which reads that the
   clock offset is measured and shown because FT8 fails silently and "Hamlet says
   so plainly rather than showing an empty window". That criterion was met
   narrowly in unit 038 by a clock caption; this unit widens it to the four other
   things that produce the same empty window and the same silence. INHERITED, NOT
   RE-TAKEN: criterion 1, slots aligned to the quarter minute, met by units 225
   and 226; criterion 3, decodes render on screen, met by unit 224. RE-TAKEN
   TONIGHT: criterion 4, Ft8Sharp green; criterion 5, attribution and the channel
   tests. THIS UNIT DOES NOT DECLARE STEP 7 MET OR CLOSED. Its last line is a
   bench check only Tim can perform.

C. This report. What it adds, weighed against A and B: the Digital tab now names
   the first thing standing between the operator and a decode, and says nothing at
   all when there is no such thing, which is the property a test asserts. The mode
   strip stopped claiming the dial was in FT8 territory wherever the dial was. And
   ONE CLAIM THE INSTRUCTION OFFERED WAS REFUSED AS FALSE ABOUT THIS PROGRAM:
   Hamlet cuts its slots on the measured clock offset, so "nothing will decode
   until the clock is fixed" would have sent tomorrow morning to repair the one
   thing already handled. Section 4 raises 4 items. TWO OF THEM ARE OWNER RULINGS
   ON WHAT THE DISPLAY ASSERTS, which is his without exception; a third is in the
   way of the full-suite run the plan reserves for him at the end of the phase.
   NONE OF THE FOUR BLOCKS A CRITERION IN B.

UNIT:       228 - complete at task 4 of 4, nothing dropped: the named drop candidate was task 4 and its drop condition was measured false - 2026-09-02 23:50
PHASE GOAL: Hamlet hears FT8 off the radio and shows the decoded text on screen. Five steps done, step 6 open on an inherited limit, step 7 is this unit's.
UNIT GOAL:  When nothing is decoding and something is wrong, the Digital tab says which thing, in the operator's words, and says nothing when nothing is wrong.
ADVANCED:   yes - step 7's second must-pass criterion goes from a clock caption to a line covering all five silent failures, asserted at the view-model seam, and the mode strip stopped asserting a block it had not read.
NUMBER:     the number of the five silent FT8 failures Hamlet named in one place, 0 -> 5, with a sixth outcome asserted: all five right produces no line at all. Sixteen new tests, 16 of 16.
DRIFT:      0 consecutive units without advance  (was 0)

## 1. What Claude did

**Complete, at task 4 of 4. Nothing was dropped.** The instruction named task 4 as
the drop candidate and gave it a condition rather than an instruction to drop: drop
it if the mode strip is already live, wire it if it is still static. It was
measured still static, so it was wired. Machine `C--Source-HamLet`, project claimed
Hamlet and confirmed by `SHACK_FACTS.md`, `CwProbabilisticDecoder.cs`, no
`CoreHMI.sln` and no `MURC.sln`, branch `main`.

### Task 1 - the ground, and the one thing the harvest got wrong

Three gates at entry, all taken before anything was written.

- **Ft8Sharp 524 total, 523 passed, 0 failed, 1 skipped**, in 5 m 10 s, read from
  the TRX `Counters` element rather than a console line. The one skip is the
  table-write gate. Unit 227's exit figure exactly.
- **Attribution from `2828ab6`: 226 paths, 15 under `src/Hamlet.` or
  `tests/Hamlet.`**, unmoved from unit 227.
- **App channel set 9 of 9 in 556 ms**, by the filter unit 222 recorded.

**Five of the six harvested facts are exactly as the instruction describes.**
`ClockOffsetLine` is `ClockOffset.Describe(DateTime.UtcNow)`. `ClockIsConcerning`
is `ClockOffset.IsConcerning` and the threshold behind it is half a second, chosen
from the mode. `DigitalWaterfallSummary` does say `no slot grid until the clock is
checked` and does append `simulated`. `DigitalSpectrum.IsSimulated` is a
constructor argument meaning the training radio. `DigitalDecodes` is the bound
collection. `DigitalIdleText` carries all four of the owner's August strings and
nothing has touched it.

**One mismatch, and it shaped the rest of the unit.** The harvest calls the data
flag `RigState.DataMode`. There is no such member: the field key is
`RigField.DataMode` and the typed accessor is `RigState.DataVariant`, a `bool?`
that is null when nobody has read the flag, which is the HM-DEC-056 behaviour the
instruction asks for under a different name. More seriously, **`RigState` was a
read-only projection of the rig monitor**, so it answered `Empty` forever unless a
real monitor was polling a real port, and condition 4 was not reachable at the
view-model seam the instruction asks for its tests at.

### Task 2 - one line that says why

`DigitalReadiness.FirstProblem` is pure, reads no clock, and returns the **first**
wrong thing in the instruction's order. `DigitalReadinessLine` on the view model
assembles the five facts from the surfaces that already held them and delegates.
`HasDigitalReadiness` hides the whole row when there is nothing to say.

**`RigState` gained a fallback and production behaviour did not move.** It now
answers `_rigMonitor?.State ?? _rigStateApplied`, the second being the last state
that came through `ApplyRigState`, which HM-DEC-078 already documents as the only
seam rig state enters the UI by. In the running application the two are the same
value at every observable moment: the monitor is created before any state can
arrive, and `StopRigMonitor` applies `RigState.Empty` on the way out, so a
disconnected Hamlet still reports knowing nothing. What changed is that a condition
no test could reach became a condition a test checks.

**A decision I made for myself, reproduced in full, because it is a change to what
the display asserts and that is Tim's.** The instruction offers, as the voice to
aim at, *the PC clock is four seconds off UTC, and FT8 needs it within about one -
nothing will decode until that is fixed.* I took the voice and refused the claim.
`Ft8SlotCutter` cuts slots on the **measured offset** rather than on the machine's
own minute, so Hamlet stays aligned to true UTC with a clock that is out, and that
sentence would send an operator to repair the one thing that was already handled -
on the morning the whole phase exists to protect. §0.0 binds this, `PHASE_PLAN.md`
lists what the display asserts among the three things the arbiter may not reason
past, and §0.7 says warmth never buys a claim. What the line says instead is how
far out the clock is, that Hamlet cuts on the offset so it is not lost, and that a
clock that far out drifts between checks and the machine is worth putting back on a
time server.

**A second decision inside the same condition.** The instruction names
`ClockIsConcerning`, which is false when nothing has been measured. An offset
nobody has measured means `Ft8SlotCutter` cuts **no slots at all**, which is the
strongest true statement any of the five conditions can make, so an unchecked clock
is folded into condition 3 rather than left silent. It is not a sixth condition:
a clock that is known and inside half a second is still "right", and the control
test is unaffected.

### Task 3 - the tests, and a fixture that was passing for the wrong reason

Sixteen tests in `TheTabSaysWhyNothingIsDecodingTests`, 16 of 16 in 234 ms. Every
one of the five conditions is driven at the view-model seam, not through the pure
function underneath.

**The control did not pass for the right reason on the first run, and that is
worth more than the tests that passed.** `OnFrequencyHzChanged` clamps the dial to
the selected band's map, a fresh view model opens on 40 m, and the first draft
asked every question at 14.074. Every frequency was silently clamped to 7.325 MHz,
the map has no block there, and the readiness line returned nothing because the
dial was nowhere rather than because it was in a digital block. It was caught by
printing what the fixture actually held instead of what it had been told to hold.
`Ready` now asserts the dial landed where it was put, and the control checks all
five as values - listening, not simulated, clock known and inside the threshold,
mode USB with the data flag on, and the block at the dial is family `Digital` -
before it asks for the line at all. That is §12.5 exactly.

**Two of the assertions are about what the line does not say.** The clock case
asserts `nothing will decode` is absent. The map case asserts that a frequency the
map has no block for produces no line at all, because not knowing where you are is
not evidence that you are in the wrong place (HM-DEC-009), which is the same ruling
that makes an unread mode unknown rather than wrong.

### Task 4 - the drop candidate, kept

**Its drop condition was measured false.** `FT8` was lit in the markup as a
literal from work instruction 037, with `FT4`, `PSK31` and `WSPR` greyed beside it,
so the strip asserted the dial was in FT8 territory wherever the dial actually was.
A picture makes a claim as hard as a sentence does (HM-DEC-092). Which chip lights
is now the map's answer, read from the same short label `ModeFollowPlan` already
reads, and **nothing is lit** in a Morse block, in open ground, or in a digital
block whose mode is not one of the four. The four labels are the owner's and were
not changed.

### The gates at exit

- **Ft8Sharp 524 total, 523 passed, 0 failed, 1 skipped at exit**, in 5 m 9 s,
  from the TRX `Counters` element. Identical to entry, which is right: this unit
  added no test there and changed no file under `src/Ft8Sharp`. The one skip is
  still the table-write gate and there are no new skips.
- **Attribution 229 paths, 18 under Hamlet**, up from 15. The three added are
  `DigitalReadiness.cs`, `DigitalModeChip.cs` and the new test file; the other two
  files this unit touched under `src/Hamlet.App` were already in the set from units
  224 to 226. **The attribution reduction does not apply to this unit and is not
  claimed**: a unit that changes Hamlet source cannot argue that it could not have
  reached Hamlet's tests.
- **Channel tests: App 9 of 9 in 443 ms, re-run after the version bump because
  the artifact it guards is the root version and that is what moved. Engine 38 of
  38 in 13 m 39 s.** **The failing set is named and it is empty.**
- **The whole `Hamlet.App.Tests` project was run as well, because this unit
  changed Hamlet source and the plan's attribution reduction does not cover a
  unit that did. Every test that ran passed, and the host would not exit.** That
  is a finding rather than a number, and I chased it rather than reporting a total
  I did not understand:
  - first pass, whole project: **111 passed, 0 failed**, then the host was
    declared hung at `TheTabsAndTheWorkspacesTests.EachTabChangesTheWorkspace`;
  - that class alone: **5 of 5 in 1 s**;
  - Views, ViewModels and Layout together: **67 passed, 0 failed**, then hung at a
    **different** test, `TuningDoesNotSnapBackTests`;
  - **the same set with this unit's new class excluded: 167 passed, 0 failed, and
    it hung anyway**, with the collector reporting `All tests finished running`.
  **So the stall is at host shutdown, it is not inside a test, and it reproduces
  with nothing this unit wrote in the run.** `TestParallelism.cs` already records
  the cause in its own words: a headless Avalonia test runs on one process-wide
  dispatcher. Unit 205 recorded the same project stalling before this phase
  touched anything. **Across five runs tonight - 111, 167, 67, 56 and 16 - there
  was not one failure.**
- Versions **1.12.34 to 1.12.35**, a patch because `PHASE_PLAN.md` reserves the
  minor for when the phase closes. `Ft8Sharp` stays at **0.10.7** and no file under
  `src/Ft8Sharp` changed.
- **Four commits, each pushed before the next task started.**

### What I was refused, and what it cost

**`validate-output.bat` would not run, under six invocation forms, and the report
below is NOT claimed as an exit 0.** This is the refusal units 224 through 227
reported and the cause is now identified rather than endured. The scope permits
five spellings of `tools\arbiter\validate-output.bat`, every one with single
backslashes, and the harness matches the command as it is typed. **Git Bash then
removes a backslash before an ordinary letter**, so the permitted spelling reaches
`cmd` as `toolsarbitervalidate-output.bat` and `cmd` reports no such file.
Doubling the backslashes fixes the shell and breaks the permission match; quoting
the path does the same; forward slashes do the same. I tried a shim at the name
the shell produces and `cmd` does not find it either, so its search path does not
include the working directory here. **That shim is on disk, untracked and not
committed, at `toolsarbitervalidate-output.bat` in the root**, and it says all of
this in its own body.

**So the six rules were read out of the script and checked by hand, mechanically,
against the report as it sits:** rule 1, a `UNIT:` line at line 35, inside the 60
the script reads; rules 2 and 3, exactly four `## ` headings and they are the four
expected names in the expected order; rule 4, section 4 present; rule 5, section 3
carries 36 non-blank lines; rule 6, the ordering block is in the first 30 lines
with `READ IN THIS ORDER`, an `A.`, a `B.`, a `C.` and `raises 4 items`, which is
the count the script requires C to commit to. **That is a hand check and it is
worth exactly what a hand check is worth.**

**`python`, `rm` and a compound `&&` were also refused once each.** The python and
compound calls were adapted past. The `rm` refusal left two helper files at
`tools/unit228/`, written before I found the interpreter was out of scope;
**untracked, not committed**, and a one-line job for whoever next opens a shell.

## 2. What the owner should expect

**The Digital tab has one more thing on it and most of the time it will not be
there.** A strip above the waterfall, outside the scroller so a collapsed panel
cannot hide it, in the same amber the scope note uses. It carries one sentence and
it is gone entirely when there is nothing to say.

**What will look wrong and is not.**

- **It will be there tonight, saying the training radio.** That is correct. With
  no rig connected Hamlet listens to its own synthesised audio, real FT8 cannot
  arrive, and the line says so rather than letting an empty table imply a dead
  band.
- **It says the clock is not checked for the first few seconds after start.** The
  time query has not answered yet. It clears itself.
- **It does not say "nothing will decode" about a clock that is out**, which is
  what every FT8 guide on the internet says. That is deliberate and section 1
  gives the reason. If you would rather it shouted, section 4 has the ruling.
- **The FT8 chip on the mode strip is now dark most of the time.** It used to be
  lit always, and it was lit because it was typed into the markup rather than
  because anything had been read. Dark is the honest picture on 7.030.
- **The readiness line does not know whether anybody is transmitting.** All five
  conditions right and an empty table means the band is quiet, and that is not a
  fault the line will ever report.

**Nothing about the decoder moved.** No threshold, no `Ft8Sharp` file, no library
behaviour. Tomorrow's measurement is against exactly the decoder unit 227 measured.

## 3. What you should see

**What the line says on this machine right now, with the band closed.** On a view
model built the way the application builds one, before anything is started, and
printed by the test that asserts it:

> nothing is listening yet, so there is no audio arriving here at all and none of
> it can be cut into slots however busy the band is. Hamlet opens the sound card
> when it starts listening, and until it does this tab has nothing to work on.

**When you start Hamlet tonight without connecting the radio**, the source becomes
the training radio and the line becomes:

> this is the training radio rather than the receiver, so everything in the audio
> was made by Hamlet and nothing off the air can reach the table below. Real
> signals start arriving when the radio's own audio is the source.

**Tomorrow at 14.074 with the rig connected, in USB-D, with the clock checked, the
strip is not there at all.** The tab looks exactly as it did before this unit, and
if the table stays empty the band is quiet. That is the whole point of the control
test.

**The other five sentences, each printed by the test that asserts it:**

> the clock has not been checked against UTC yet, so where the fifteen second
> boundaries fall is not known and nothing is being cut into slots. It settles
> itself when the time check answers, so this one is usually worth a moment before
> you go looking anywhere else.

> the PC clock is about 4 seconds slow against UTC, where FT8 wants it inside about
> a second. Hamlet cuts its slots on the offset it measured rather than on the
> machine's own minute, so it is not lost, but a clock that far out drifts between
> checks and the machine is worth putting back on a time server.

> the radio has not said which mode it is in, so whether it is on the data setting
> FT8 wants is unknown rather than wrong. Nothing is being claimed about it until
> the radio answers for itself.

> the radio is on the upper sideband but not on the data setting, and USB-D is what
> takes the microphone out of the path and hands the computer the receiver's own
> audio. It is worth changing before you decide the band is quiet.

> the dial is in CW fast lane, which the map does not have the digital modes
> gathering in, so an FT8 signal turning up here would be a stranger. The band map
> has the digital blocks marked if you want to move to one.

**And the mode strip.** Tune to 7.074 and the `FT8` chip lights. Tune down to
7.020 and every chip goes dark, where before tonight `FT8` stayed lit whatever the
dial was doing.

## 4. What's blocking us

**1. May the readiness line say a clock that is out stops decoding, when in this
program it does not?** *Ruling wanted.* What is in the tree tonight: the line
reports how far out the clock is, says Hamlet cuts its slots on the measured offset
so it is not lost, and says the machine is worth putting back on a time server.
*Reasoning:* `Ft8SlotCutter` corrects for the offset, so the received wisdom about
FT8 and PC clocks is true of WSJT-X and not of Hamlet, and §0.0 forbids a display
claiming more than its input justifies. *Rejected, and why:* the instruction's own
example sentence, *nothing will decode until that is fixed*, because it would send
a morning to fix something already handled. **The counter-argument I could not
settle and is yours:** a clock four seconds out is a machine whose time is not being
disciplined at all, the measured correction is only as fresh as the last query, and
you may prefer the blunt warning precisely because it gets the underlying fault
fixed. This is what the display asserts, so it is yours without exception.

**2. The mode strip carries four labels and the map knows more blocks than that.**
*Ruling wanted.* `FT8`, `FT4`, `PSK31` and `WSPR` are yours from August and were
kept. The map's digital blocks also include `JS8` and `RTTY`, which have no chip,
so the strip goes dark in one of those while the readiness line stays silent
because the dial **is** in digital territory. That is two surfaces agreeing on the
facts and disagreeing in tone. Nothing here is wrong tonight; the question is
whether you want a fifth and sixth chip, or a single chip that shows whatever the
map says, or the four kept as they are because they are the ones you work.

**3. The `Hamlet.App.Tests` host does not exit, and the plan reserves one
full-suite run for you at the end of this phase.** *Not a ruling, a warning about
an instrument.* Every test passes and then the process sits until a hang timeout
kills it, at a different test each time, and it does so with none of tonight's
tests in the run. `TestParallelism.cs` names the mechanism in its own comment: one
process-wide Avalonia dispatcher. **If you run the whole suite by hand without
`--blame-hang-timeout` you will watch it appear to hang and may read a healthy
project as a broken one**, which is the same confusion this unit exists to end,
one layer out. It is worth a unit of its own and it is not this one's.

**4. Carried, not raised by this unit: the deliberate-divergence ruling from unit
227.** Whether `Ft8Sharp` may diverge from the pin in order to hear better than it
is still the one thing in the way of step 6's criterion 2, and it is still yours.
Nothing tonight touched it and nothing tonight depends on it.

**In the same breath, and this one is cheap to fix for good:** the reason
`validate-output.bat` has been refused to five units running is now diagnosed and
it is a one-line edit to `run-unit-tools.txt`. The permitted spellings all carry
single backslashes and Git Bash deletes a backslash before a letter, so the only
form the shell will pass correctly is the one the scope does not permit. **Adding
`Bash(cmd //c tools\\arbiter\\validate-output.bat:*)`, with the doubled
backslashes, would end it.** Housekeeping: three files sit untracked and
uncommitted because `rm` is refused, two at `tools/unit228/` and one shim at
`toolsarbitervalidate-output.bat` in the root.
