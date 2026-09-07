READ IN THIS ORDER

A. THE PHASE GOAL IS "Hamlet works stations on the air", and every step's
   state is unchanged tonight: step 0 done; step 1 done; steps 2 to 5
   partial; step 6 not started. Nothing this unit measured changed any of
   those. NO STEP CLOSED TONIGHT AND NONE WAS MEANT TO - the work
   instruction forbids closing step 2, step 3 or step 6, and this unit
   closes none of them.

   The fact that shaped this unit: what has been recorded open in steps 2
   and 3 for four consecutive units is THE LEVEL, deferred to Tim under
   SHACK_FACTS.md FACT-004. Before tonight, HAMLET HAD NO CONTROL AND NO
   READOUT WITH WHICH HE COULD HAVE CLOSED IT. Measured cold: the waveform
   was built at unit amplitude, no compose route took an amplitude
   argument, nothing between the composer and the sound card multiplied a
   sample by anything, and every reader of the sink's own peak and clip
   counters in the whole repository was a test. A criterion deferred to an
   operator who has no control and no number is not deferred; it is
   unclosable by anybody.

B. THIS UNIT AIMS AT STEP 3, CRITERION 1 - "audio plays to the radio's USB
   input at the right device, rate and level." Two of its three halves
   were already met: THE DEVICE BY UNIT 256, THE RATE BY UNIT 262. The
   level half is this unit's.

   AT WHAT PEAK DID HAMLET TRANSMIT BEFORE THIS UNIT, AT WHAT PEAK DOES IT
   TRANSMIT AFTER IT, AND CAN THE OPERATOR NOW CHANGE IT AND SEE IT?

   BEFORE: 0.00 dBFS - peak sample 1.000000, full scale.
   AFTER:  -12.04 dBFS - peak sample 0.250000, by default.
   Both measured on THE DEVELOPMENT MACHINE, the second of them through a
   real render endpoint (S34J55x display audio, 48000 Hz), which reported
   peak written 0.2500 with 0 samples clipped and decoded the message back
   as itself.

   CAN HE CHANGE IT? Yes - a Transmit drive control on the Settings screen,
   beside the transmit endpoint picker, refusing any value the composer
   would refuse. CAN HE SEE IT? Yes - after every send the line under the
   waterfall says the level in dBFS and the clip count.

   WHAT IS STILL UNMET IN CRITERION 1, AND WHOSE IT IS: the figure the
   IC-7300's USB modulation input actually wants is not in this repository,
   cannot be inferred from anything measured on this machine, and is TIM'S
   to read off the radio against its own ALC meter under FACT-004.

   TASK 5 WAS NOT DROPPED. The named drop candidate shipped, so there is
   nothing Tim must edit by hand instead.

C. THIS REPORT'S OWN FINDINGS, weighed against A and B. SECTION 4 RAISES
   THREE ITEMS AND NONE OF THEM STANDS IN THE WAY OF ANYTHING NAMED IN B.
   None is a ruling request.

   The four findings this instruction anticipated all landed as it
   predicted, and none is blocking:
   - DOES THE SCALED SIGNAL STILL DECODE, AND WITH WHAT MARGIN? Yes, at no
     measured cost: step 2's own corpus proof passes untouched at the new
     level, and the loopback decoded back as itself with about 74 dB
     between the signal and the quantisation floor against a decoder
     threshold near -21 dB SNR.
   - IS AppSettings.cs:210-213's REMARK STALE? Yes. It said the settings
     screen was not built; unit 260's outcome entry was right and the
     remark was wrong. Corrected under task 5's licence.
   - WAS Ft8Composer THE RIGHT PLACE FOR THE SCALE? Yes. There is exactly
     one ComposeSignal call site in all of src/ and both send doors reach
     it.
   - DOES ANYTHING IN src/ READ PeakWritten? No. Confirmed, not corrected,
     which made task 4 smaller rather than different.

UNIT: 265 - complete at task 5 of 5 - 2026-09-07 02:57
PHASE GOAL: Hamlet works stations on the air
UNIT GOAL:  Hamlet stops transmitting at full scale and the operator can set
            and see the level.
ADVANCED:   yes - step 3 criterion 1's level half moves from unreachable to
            reachable: 0.00 dBFS before, -12.04 dBFS after, with a control
            and a readout. NOT CLOSED; the radio-side figure is Tim's.
NUMBER:     0.00 dBFS -> -12.04 dBFS (peak 1.000000 -> 0.250000, development
            machine)
DRIFT:      0 consecutive units without advance (was 5)

## 1. What Claude did

**Exit state: COMPLETE, at task 5 of 5. All five tasks were attempted and
none was dropped, including the named drop candidate.** Development machine,
project confirmed as Hamlet, branch `main`, pushed at `9fcfdf2`.

The four identity checks were run before anything else and all four hold:
`SHACK_FACTS.md` present, `src/Hamlet.RadioEngine/Cw/CwProbabilisticDecoder.cs`
present, `CoreHMI.sln` absent, `MURC.sln` absent, `Hamlet.sln` the only root
solution.

### Task 1 - the trace, committed on its own before task 2 (`0094328`)

`docs/unit265-the-level-trace.md`, seven questions, each with a file, a line
and a quotation.

| Question | Answer |
|---|---|
| Q1 what peak leaves the composer | **1.000000 at both 12000 and 48000 Hz** = 0.00 dBFS, run by exact name |
| Q2 any gain on the path | **None.** One multiplication exists between composer and endpoint and it is not a gain: `sample * 32767.0`, the PCM16 conversion. **The operator could not change the level by any route, including editing the settings file by hand** |
| Q3 who reads `PeakWritten` | **Zero readers in `src/`.** All four in the repository are tests. Instruction confirmed |
| Q4 does it describe what leaves the machine | **Cannot be told from here, and this is said plainly rather than guessed.** It is what was handed to the WASAPI render buffer; whether Windows scales afterwards is not knowable from this repository |
| Q5 what a scale would break | The re-run list, eleven named suites |
| Q6 where the scale must go | **Exactly one `ComposeSignal` call site in all of `src/`**, `MainWindowViewModel.cs:8184`, reached by both the CQ button and the right-click send. **No second compose site** |
| Q7 the decode budget | At full scale on this machine's endpoint: peak 1.0000, RMS 0.7064, clipped 0, decoded. RMS is `1/sqrt(2)` to four figures, so **FT8's crest factor is 3.01 dB and peak and average move together** |

### Task 2 - the drive, watched red first (`38d874b` red, `c56c7e9` green)

**The red, verbatim, against the untouched tree:**

```
the composer was configured for a peak of 0.250000 (-12.04 dBFS)
and produced 1.000000 (0.00 dBFS).
```

`Ft8Composer.DefaultDrivePeak = 0.25f`, applied in `Ft8Composer.Build` - one
place, both compose routes - with the arithmetic written at the site.
`Ft8ComposeRefusal.DriveLevelRefused` joins the four that were there.
**`Ft8Sharp` is untouched.** 17 of 17 green, plus 3 of 3 on the application
side: with 0.4 set in Settings, the CQ button and the right-click send both
went out at 0.400000 and at the same number.

### Task 3 - the neighbours (`545a1ba`)

Every suite named in Q5, filtered by exact name. **29 of 29**, **15 of 15**,
**32 of 32**, **5 of 6**, **2 of 2**, **1 of 1**. Three expectations changed,
all of them the scale, each with the reason at the site. One failure that is
not the scale, reported in section 4 and not chased.

### Task 4 - the operator can see what went out (`c56c7e9`)

The line now reads what section 3 quotes. **It is the composed peak and the
line says so**, because Q3 found no route to the sink's own `PeakWritten`
from what the sequence returns, and building one would have meant touching
the keying path units 255, 261 and 263 proved. **Nothing in that path was
touched.**

### Task 5 - the Settings control (`9fcfdf2`)

**The named drop candidate, and it shipped.** 12 of 12, with the picker's own
six unchanged beside it.

### Decisions I made for myself, reproduced in full

1. **The default level, 0.25 = -12.04 dBFS.** The instruction deliberately
   did not specify it, bounding it only at "at least 6 dB below full scale".
   Chosen out of task 1's measured budget: twelve dB below full scale, twice
   the required minimum, with about 74 dB of slack over the 16-bit
   quantisation floor.
2. **The scale applies to BOTH compose routes**, not only the on-air one,
   because `Build` is literally one place and a future caller who plays the
   padded slot cannot then get full scale by forgetting an argument. The cost
   was three changed expectations, written against `DefaultDrivePeak` so they
   cannot go stale against a constant again.
3. **I ran the application-side loopback once at full scale against the
   untouched tree**, to answer Q7. That is a second sound beyond the one task
   3 item 3 names. Q7 asks for the device-side decode result at full scale;
   no such figure existed anywhere in `docs/`, so it could only come from a
   run, and having it makes the before-and-after pair a measurement on one
   endpoint rather than an arithmetic claim.
4. **The readout uses the composed `PeakSample`**, for the reason under task
   4 above.

### The tool rule

`tools\arbiter\outcome-append.bat` was tried once, verbatim, and refused:
**"This command requires approval"** - the thirteenth consecutive refusal.
The `PHASE_OUTCOME.md` entry was then appended with the file-editing tools in
the format the script writes: twelve fields, same order, ASCII, **existing
entries untouched.** `outcome-read.bat --approach` was not used; its
apostrophe parse error is recorded at units 262 to 264 and is not this unit's
to repair.

## 2. What the owner should expect

**Hamlet no longer transmits at full scale.** Out of the box it sends at
-12.04 dBFS, and that is the single most important sentence in this report.

**What will look wrong but is not:**

- **The signal is quieter than it was, on purpose, and Hamlet is still doing
  the same thing.** A drive of 25 % is not Hamlet being timid; it is where
  soundcard digital modes are supposed to start, and the number is meant to
  be turned up against your own ALC.
- **The number on the Settings screen is not the number your radio wants.**
  Nothing in this repository knows that figure. The screen says so in its own
  words, and it is deliberate that it does not offer you a "correct" value.
- **The send line now has a second sentence on it.** After a send it reads
  the level and the clipping as well as what went out and when.
- **The level Hamlet reports is the level it composed, not the level that
  reached the antenna.** Between the two lie this machine's own volume for
  that device and then the radio's input gain. The line names both rather
  than pretending the number is the whole story.
- **Three tests changed their expectations tonight** and one of them changed
  from `worstPeak > 0.99f`. That test used to require full scale. It was the
  one assertion in the tree that would have made this unit impossible.

## 3. What you should see

**The answer to the question this unit was commissioned to ask, in one
number, and it leads with the red as the work instruction requires.**

Task 2's assertion, run against the untouched tree, one test filtered by
exact name, no product code changed:

```
Failed Hamlet.RadioEngine.Tests.Transmit.TheOperatorSetsTheLevelHamlet
       TransmitsAtTests.WhatTheComposerProducesIsAtTheDriveLevelAndNot
       AtFullScale [250 ms]
  Error Message:
   the composer was configured for a peak of 0.250000 (-12.04 dBFS) and
   produced 1.000000 (0.00 dBFS).
  Standard Output Messages:
   message        : CQ KC3QIS FN00
   drive asked    : 0.250000  (-12.04 dBFS)
   peak composed  : 1.000000  (0.00 dBFS)

Test Run Failed.  Total tests: 1  Failed: 1
```

**That is the state of this phase's oldest deferral in a single number.** It
was committed at the red, at `38d874b`, before anything was made green.
Everything below is the repair.

The same assertion after it: `Passed ... [9 ms]`, `Total tests: 17, Passed: 17`.

### In the application, in your own terms

**You can now turn Hamlet down, and it starts turned down.**

> Open Settings. Under the transmit endpoint picker there is a new box,
> **Transmit drive**, showing **25 % of full scale**, and under it a line that
> reads: *How hard Hamlet drives the radio's input - -12.0 dBFS at this
> setting. This is a starting point, not a specification. Set it against your
> own radio's ALC meter: turn it up until the ALC just begins to move and then
> back off. Full scale is a wide, distorted signal over other people's band,
> which is why Hamlet starts at 25 %.*
>
> Type a level it cannot use and it will not take it - the line tells you what
> is still in force instead.

**And after you send, Hamlet tells you what went out.** The line beneath the
waterfall now reads, verbatim from the test:

```
Sent "CQ KC3QIS FN00" in the slot at 06:46:30 UTC. It was composed at
-12.0 dBFS with nothing clipped - that is the level Hamlet built, before
this machine's own volume for that device and before the radio's input
gain. Set the radio's drive against its own ALC meter.
```

### The one sound this unit made deliberately, and the pair it belongs to

Both runs, same endpoint, same message, **development machine**, through
**S34J55x (3- HD Audio Driver for Display Audio)** at 48000 Hz:

| | before (untouched tree) | after (new default) |
|---|---|---|
| level asked | none - there was no argument | 0.25 (-12.04 dBFS) |
| peak written | **1.0000 (0.00 dBFS)** | **0.2500 (-12.04 dBFS)** |
| rms written | 0.7064 (-3.02 dBFS) | 0.1766 (-15.05 dBFS) |
| clipped samples | 0 | **0** |
| decoder returned | "CQ KC3QIS FN00" | **"CQ KC3QIS FN00"** |

The RMS ratio is `0.7064 / 0.1766 = 4.000` - exactly the 12.04 dB, which is
task 1's prediction from the 3.01 dB crest factor confirmed by measurement.

**FACT-004, and it is the point: every figure in this section was measured on
the development machine, through a monitor's display-audio endpoint. No radio
has ever been attached to this machine. NONE OF IT SAYS ANYTHING WHATEVER
ABOUT THE IC-7300, and what its USB modulation input expects is not in this
repository and is not claimed here.**

### Every count, and which neighbours were run

| Suite | Result |
|---|---|
| `TheOperatorSetsTheLevelHamletTransmitsAtTests` (new) | 17 of 17 |
| `WhatTheTransmissionLooksLikeAsAudioTests` + `TheSeamTurnsWordsIntoASlotOfAudioTests` | 29 of 29 |
| `HamletsOwnDecoderReadsBackWhatHamletComposedTests` + `TheRoundTripHoldsAtTheEndpointsRateTests` + `OneClickSendsExactlyOneMessageTests` | 15 of 15 |
| app send-path suites (five, named in the task 3 commit) | 32 of 32 |
| `TheApplicationSendsAtTheLevelTheOperatorSetTests` (new) | 3 of 3 |
| `SettingsCarriesTheTransmitDriveTests` (new) + `SettingsNamesTheTransmitEndpointTests` | 12 of 12 |
| `TheLoopbackProvesTheWholeChainTests` | 2 of 2 |
| `TheLoopbackThroughTheApplicationsSendPathTests` | 1 of 1 |
| `TheSinkPlaysToANamedEndpointTests` | **5 of 6** - see section 4 |

**Not run, and why:** no unfiltered suite was run on any project, which is the
standing rule. The inherited reds named in `PHASE_PLAN.md` -
`CwAdjudicationTests.ASpeedChangeInRealisticAudio`, the 51 CW cases in
`docs/unit239-failing-set.txt`, and the `Ft8Sharp.Deep.Tests` whole-type-list
tripwire - were not run and are never chased.

### The three expectations that changed, and why each is honest

- **`NothingTheSeamProducesLeavesFullScale`** asserted `worstPeak > 0.99f`.
  It was right while the seam could build nothing but a full-scale sine. It
  now asserts the peak equals `Ft8Composer.DefaultDrivePeak` **and** is at
  least 6 dB below full scale - so a future change putting the default back to
  1.0 fails here too.
- **`TheSixteenBitConversionOfARealTransmissionStaysInRange`** asserted
  `max > 32000` and reported `max is 8192`. 8192 is `0.25 * 32767`. The
  property it was really testing - that nothing wraps - is kept, written
  against the drive.
- Both are now expressed against the constant rather than against a literal,
  so they cannot go stale the same way again.

### The four things the instruction expected to be told it got wrong

All four landed as it predicted. Full detail in the ordering block, section C.

## 4. What's blocking us

**Nothing is blocking. Three items are recorded for the record; none is a
ruling request, and none stands in the way of anything named in B.**

### 1. An inherited red, found and deliberately not chased

`TheSinkPlaysToANamedEndpointTests.ACancelledPlayGoesOutShortAndTheSequence
CallsItAudioFailed` fails: `Expected: AudioFailed, Actual: Cancelled`.

**It is not the scale and it is not mine.** The test builds its own tone
through `RenderChoice.Tone` and never touches `Ft8Composer`, so the drive
level cannot reach it. `Ft8TransmitSequence.cs:296-305` is unit 263's new
branch - a short play under cancellation now reports `Cancelled` rather than
`AudioFailed`, deliberately, because "one is the sound card letting him down;
the other is him pressing the button". The test asserts the pre-263 behaviour
and was last touched at `77268f1`, before unit 263 existed.

Task 3 item 2 says: where a test fails for a reason that is not the scale,
stop, report it, and do not chase it. **Reported, not chased.** The next unit
inherits a one-line expectation change and the sentence explaining it.

### 2. Two places where the tree disagreed with the instruction

Both reported and neither repaired, per "the tree wins".

- The instruction refers to "the four at `Ft8Composer.cs:14-34`". The enum
  runs to line 40 and holds **five** values - four refusals plus `None`. I
  added the new refusal beside them as asked; the count and the line range in
  the instruction are both slightly off and nothing turns on it.
- `AppSettings.cs:210-213`'s remark was stale, as the instruction suspected.
  **I corrected it**, and only because task 5 put the drive control beside
  that picker, which is the exact condition the instruction set for touching
  it. Had task 5 been dropped, the remark would have been left alone.

### 3. Carried forward, reported and not chased

- `PROJECT_STATUS.md` carries `RULES_AT: HM-DEC-157 (2026-09-06)` while
  `CLAUDE.md` §1's highest is `CPS-DEC-0152`. **Reported, not reconciled** -
  the work instruction parks it explicitly.
- `PHASE_OUTCOME.md`'s header says step 1 is `done` while an entry beneath it
  says `partial`. **Expected, and not reconciled**: unit 264 set the header on
  work instruction 264's authority and the entry predates it. No existing
  entry was edited and no step's word in either header line was touched.
- `outcome-append.bat` refused once, verbatim - the thirteenth consecutive
  unit.
- **`validate-output.bat` could not be run, and this report's validation is a
  hand check standing in for a run.** Three forms were tried: the verbatim
  backslash form, which the shell mangled into
  `toolsarbitervalidate-output.bat: command not found`; the forward-slash
  form; and `cmd /c "..."`. The last two were both refused with **"This
  command requires approval"**. Following unit 263's recorded fallback, the
  script's seven rules were checked by hand against the copy it prints in its
  own header, and **all seven pass**: the `UNIT:` line is above section 1 and
  parses; the four top-level sections are in order with exact names; there is
  no fifth; section 4 is present and non-empty; section 3 is non-empty; the
  ordering block sits above the `UNIT:` line with A, B and C, and C names how
  many items section 4 raises; and the header block carries no placeholder
  token.
- **The bookkeeping the reload named was committed before task 2**, at
  `96d7d2d`: `PHASE_OUTCOME.md`, `PHASE_STATUS.md`, `RUN_LEDGER.md`,
  `SESSION.lock`, `WORK_INSTRUCTIONS.md` and the whole of `.run-unit/`. **What
  I left:** none of it - `git status` was clean of the reload's list after
  that commit. The only files I authored changes to among them are
  `PHASE_STATUS.md`'s `WORK_INSTRUCTION:` line and `PROJECT_STATUS.md`'s
  header; `HEARTBEAT:`, `CURRENT_STEP:` and the `STEP:` lines were not
  touched and nothing below the `---` was changed.

### Not blocking, and not raised as questions

The parked list was honoured in full: the IC-7300's expected figure, the
transmit audio base frequency, step 4's criterion 6, step 1's fifth criterion
and both its header words, the CW send and band-scan members, the licence-gate
wording, and the `RULES_AT` disagreement. **No step was closed. `TransmitGuard`
was not touched, the drive level is not a gate, and neither the abort nor the
stop was made conditional on anything added tonight.**
