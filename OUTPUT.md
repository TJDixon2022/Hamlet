# OUTPUT.md

## 1. What Claude did

### Task 1: seeding the dit works, and it works on the fist that has been unreadable

Claude Code on the development machine, in `C:\Source\HamLet`, on `main`. The
prompt named `PROJECT: Hamlet` and the four gate checks all hold: `Hamlet.sln`
and `src\Hamlet.RadioEngine\Cw\CwGate.cs` exist, `CoreHMI.sln` and `src\CoreHMI`
do not. **No radio is attached** (HM-DEC-093), so nothing below is evidence about
the radio.

| audio | true dit | today | seeded | what came out |
|---|---|---|---|---|
| **`farnsworth-heavy`** | 56 ms | 3 of 11 | **10 of 11** | `AL K` becomes **`Q DE N0CALL K`** |
| `farnsworth-light` | 100 ms | 9 of 11 | 9 of 11 | `DE N0CALL K`, unchanged character for character |
| `cw-2026-08-17-134712` (`N4L`) | 56.3 ms | nothing | nothing | one unresolved placeholder, no character |
| `cw-2026-08-17-013347` (`VA3VRR`) | 100.4 ms | 8 | 8 | `VA3VRR`, unchanged character for character |

**The count is of `CQ DE N0CALL K`'s eleven non-space characters, in order.** The
earlier reports' denominator of twelve counted a space; the numerators reproduce
exactly, so 3 and 9 are the same 3 and 9.

**The heavy fist is the answer to task 1's question and it is a large one.**
Three of eleven to ten of eleven, and what those seven characters are is the
whole callsign plus the front of the call. Only the opening `C` is still lost.

**What the estimator does with the seed afterwards: it holds, and on the audio
where the fit is sound it never engages at all.** On `farnsworth-heavy` the dit
sits at 57.1 ms for the entire message against a true 56, with no spread at all,
where without the figure it sits at 46.6 and wanders between 42.3 and 47.1. On
`farnsworth-light` and on `cw-2026-08-17-013347` the seeded figure is never used:
the fitted dit is the estimator's own, 94.7 and 95.9, and the transcripts are
identical to the unseeded ones character for character. On
`cw-2026-08-17-134712` it wanders, 15.5 to 114.3, because the seed engages only
in the windows where the fit has gone wrong in the one identifiable way described
below, and on a real off-air recording that is some windows and not others.

### What the seed is, and the two shapes that were measured and rejected

**It is one number and one place.** `CwSpeedEstimator.Seed` takes words a minute,
refuses anything outside what people send rather than clamping it, and the dit it
implies stands in for the fitted one **only where the fit describes a fist nobody
has**: where the sender's own dah measures less than two dits or more than five,
which is the band the estimator already uses to decide whether a fist is
plausible. That is precisely the collapse the work order describes. On
`farnsworth-heavy` the fit reads the dit at 46.6 and then calls the sender's
238 ms dah 5.1 dits, off the end of the band, at which point `MeasureCoherence`
falls back to a textbook three this sender demonstrably does not send and scores
every mark against a length nobody keyed.

**Rejected, measured: using the seed wherever coherence dips.** That was the
first shape and it is worse. Coherence sags for a window or two in the ordinary
course of a message on a sender the estimator reads perfectly well, so the dit
flips between the fitted value and a round number, which is the jitter HM-DEC-095
built hysteresis to stop. `farnsworth-light` put `ETE TTET` in front of a message
it had previously been silent about, which is the direction §0.0 forbids.

**Rejected, measured: retiring the seed once the fit is coherent.** Clean to
state and it removes the entire gain, because on `farnsworth-heavy` the fit does
cross the coherence floor early while still describing a fist nobody has.

**Rejected, measured: letting the tracker pick its analysis window from the
seeded speed.** The reasoning was that the operator's figure beats no figure at
all before there is a fit. It added nothing to `farnsworth-heavy`, which was
already whole, and it **cost `cw-2026-08-17-013347` its callsign**, `VA3VRR`
coming apart into `VRR A3VRR`. What the operator supplies is a dit, and it is now
used as a dit and nowhere else.

### Task 2: the acceptance was not met, and the mechanism named is not available

**`Refine`'s window was confined to the fitted element-gap class exactly as the
order specifies, measured, and reverted.** `farnsworth-light`'s dit went the
wrong way, 95.0 to 87.1 against a required 95 to 105, and its count fell from 9
of 11 to 6.

**The reason is that `Refine`'s premise is what is wrong, not its window.** It
averages the mark-derived dit with the mean element gap because a mark measured
against a threshold partway up its edge reads long by the same amount the gap
after it reads short, so the mean of the two is the truth. **That holds only
where the element gap is the dit**, and on a Farnsworth fist it is not:
HM-DEC-115 measured exactly this and HM-DEC-145 puts this sender's element gap at
73 ms against a dit of 100. Confining the average to gaps that really are element
gaps therefore averages 100 with 73 and lands at 86.5, which is what came out to
within a millisecond. What was helping before was the accident of character gaps
of 150 ms being inside the 200 ms window and pulling the mean back up.

**And there is no version of it that works from inside `Refine`.** The measured
mark is the true mark plus the edge bias and the measured gap is the true gap
minus it, so with two unknowns and one equation the dit cannot be separated from
the element gap without one of them being known already. `Refine` is untouched
and byte-identical.

### Task 3: the control

On the CW terminal, under the transcript: a checkbox reading **"I can hear the
speed"**, off, with a minus, a figure and a plus beside it that step one word a
minute at a time and are disabled until it is on. It is **not persisted**, so
every fresh start of Hamlet has it off. Under it, a line that says which of the
two speeds produced what is on the screen, in three states — off and silent, set
and waiting because Hamlet's own fit looks like a real fist, and in use because
the fit did not.

**The sidecar gains a `copySpeed` line and the roster a `seed` column**, both
saying `not set` where he was not helping, so tomorrow's rows say which of the
two speeds each piece of text came from.

**No decision was recorded under §12.1.** The ruling that this be built is Tim's,
in the work order; the control changes what the display asserts, which §12.1 puts
outside what a session may record for itself. It needs an entry and an id, and
that ask is in section 4.

## 2. What Tim should expect

### At the rig tonight, in three sentences

**Leave it alone until a station beats you.** When one is clearly sending and the
transcript is empty or starts several characters in, tick "I can hear the speed",
set the figure to roughly what you are hearing — the arrows step one word a
minute and you can work them without looking — and watch the line under the
transcript: if it says Hamlet is reading at your figure, your number is being
used, and if it says your figure is set and waiting, Hamlet's own fit already
looks like a real fist and the problem tonight is something else. **When the
figure is right you should see the transcript start at the beginning of the call
instead of part-way into it**, and when it is wrong you will see the same
placeholders and strangers as before rather than a plausible wrong callsign,
because nothing about this changes what counts as a resolved character.

**Does a callsign at the front of a call survive with the speed set? On the
heavy fist, yes.** `CQ DE N0CALL K` came back as `Q DE N0CALL K` with the speed
set, against `AL K` without it. On the lighter fist it already survived and is
unchanged. **On the real `N4L` recording it still does not**: that file produces
one unresolved placeholder and no characters, seeded or not.

### What is now true, and what will look wrong and is not

The control is additive. **With it off, every number is identical to this
morning's**, which was proved by running the whole solution before and after:
`cw-2026-08-20-014854` and `-014935` still produce nothing, the capture floors
all hold, `VA3VRR` still reads, and the easy tier is whole apart from the one
fixture already red.

Build clean, no warnings. **2,129 tests, five failing, and they are the five
expected:**

- `CwSettledSilenceTests.APassThatReadSomethingEmitsSomething`
- `CwFarnsworthTests.TheBulletinDecodesToItsAnswerKey`
- `CwTerminalTests.ClearingTheTranscriptLeavesTheDecoderAlone`
- `ARecordingWithKeyingInItIsReadTests.TheDecoderSaysSomethingAboutIt`
- `TheToneIsFoundInRealisticAudio(farnsworth-heavy)`

Eleven tests were added, all green, in
`TheOperatorCanSayHowFastItIsTests` — including the one that matters most, which
proves that **no speed you type in makes the recording with no keying in it
speak**. The guard that keeps it quiet is how far the two mark lengths sit apart
counted in their own scatter, and that is a ratio with no dit in it, so the seed
cannot move it.

**What will look wrong and is not, one:** two rig tests failed once each across
four full runs of the solution and passed immediately when re-run on their own —
`RigReadTests.EachSettingParsesToTheManualsOwnWords` with the squelch row, and
`ModeFollowTests.ARefusedWriteLeavesTheModeUnknown`. Nothing in the rig code was
touched this session. Named as `HM-OPEN-055` and left (§12.6).

**What will look wrong and is not, two:** the roster's `seed` column sits between
`wpm` and `chars`. A roster file already started today and appended to after this
build would have rows of two different widths. Tonight's evening file is new, so
this only bites if you have already pressed the case button today on this build.

Pushed to `main`.

## 3. What we should do next

- **Score an evening with the control**, because the roster's `seed` column now
  makes it answerable: how often did his figure get used, and on those rows did
  the `read` column say yes. That is the first measurement this project has that
  separates "the decoder cannot fit this fist" from "the decoder cannot hear this
  station".
- **The real `N4L` recording is still the hard case and it is now better
  isolated.** Its fitted dit with the figure set wanders between 15.5 and 114.3,
  so the seed engages in some windows and not others, which means the mark
  clusters themselves are being contaminated on real audio in a way the generated
  fixture does not reproduce. That is a different question from the short-mark
  one and it is now separable from it.
- **`Refine` needs its own ruling**, on the evidence above rather than on another
  attempt to tune it. Its correction is exactly right for a textbook fist and
  exactly wrong for a Farnsworth one, and this project's two adjudicated fists are
  both Farnsworth.

## 4. What's blocking us

Nothing blocks the next unit.

**Two asks, both new this session.**

> **The speed control needs an entry in `DECISIONS.md` and an id.**
>
> It was ruled in the work order of 2026-08-20 and it is built, measured and
> pushed. §9.5 says a decision that is not in `DECISIONS.md` is not made, and
> §12.1 puts anything touching what the display asserts outside what a session may
> record for itself, so this one comes back rather than being written here. What
> the entry has to carry is the shape that survived measurement, because three
> other shapes did not: the figure stands in for the fitted dit **only where the
> sender's own dah measures outside two to five dits**, it is never used to pick
> the tracker's analysis window, and it is not persisted across a restart.
>
> **Rejected, and worth recording as rejected**: using the figure whenever
> coherence dips, which put invented characters in front of a message
> `farnsworth-light` had been silent about; retiring it once the fit is coherent,
> which removes the whole gain; and letting the tracker follow it, which cost
> `cw-2026-08-17-013347` its callsign for no gain anywhere.

> **`Refine`'s correction is right for a textbook fist and wrong for a Farnsworth
> one, and the question is what to do about that rather than how to tune it.**
>
> Task 2's acceptance was not met and the mechanism it named is not available.
> Confining the average to gaps the fit says are element gaps takes
> `farnsworth-light` from 95.0 ms to 87.1 and from 9 of 11 to 6, because this
> sender's element gap is genuinely 73 ms against a dit of 100 (HM-DEC-115,
> HM-DEC-145). The averaging assumes those two are the same length. What was
> helping was the accident that 150 ms character gaps fell inside a 200 ms window.
>
> There is no repair inside the method: the measured mark is the true mark plus
> the edge bias and the measured gap is the true gap minus it, which is two
> unknowns and one equation. **Its removal has been proposed and withdrawn four
> times and is parked, so nothing was done.** What is new is that there is now a
> measurement saying why the obvious middle course does not work.

### Asks still outstanding

- **The speed control needs an entry in `DECISIONS.md` and an id.** First made
  2026-08-20, this session. Waiting on Tim. The code is on `main`.
- **What to do about `Refine`, whose correction is wrong for a Farnsworth fist.**
  First made 2026-08-20, this session. Waiting on Tim. Nothing is changed;
  `Refine` is byte-identical.
- **What shortens a short mark**, with the de-glitch and the analysis window both
  eliminated. First made 2026-08-20. Waiting on a session of its own. The seed
  works around it and does not answer it.
- **The keying meter's provisional thresholds.** First made 2026-08-20. Waiting on
  one evening's roster scored against the `meter` column.
- **Whether `SHACK_FACTS.md` still holds that CI-V Transceive is off.** First made
  2026-08-20. Waiting on one capture taken with the radio connected, so the
  `broadcast` line has something to report. The change is in the tree at
  `MainWindowViewModel.BroadcastDuringCapture` and `CivLinkHealth.LastTransceiveUtc`.
- **HM-DEC-130, whether a message too long for one keyer send may be split.**
  First made 2026-08-18. Waiting on the seam between two sends measured into the
  dummy load. `CwMessage.Split` already exists and nothing calls it for this.
- **HM-DEC-098, whether §0.2's first sentence is amended to permit an attended
  automatic transmit cycle on the air.** First made 2026-08-17. Waiting on every
  interlock watched to fire into the dummy load, including the link pulled
  mid-cycle. The cycle is built and is dummy-load only.
- **HM-OPEN-033, the cold-start bin choice and `prosigns-easy`.** First made
  2026-08-18; HM-DEC-129 scheduled it rather than closing it. Waiting on its own
  work order.
- **HM-OPEN-007.** Open and unruled since 2026-08-14, named in HM-DEC-140 as the
  reason the queue's own premise is worth re-testing. Waiting on Tim.

**Nothing leaves the queue this session.**
