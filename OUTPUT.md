# OUTPUT.md

## 1. What Claude did

### Task 1: the twenty marks ending with `N4L`

Claude Code on the development machine, in `C:\Source\HamLet`, on `main`. The
prompt named `PROJECT: Hamlet` and the four gate checks all hold. **No radio is
attached, and nothing here needed one** (HM-DEC-093). **Nothing in `src/`
changed**, which is this unit's whole point. Last session's three-way fit and
`Refine` change are confirmed absent from the tree.

`cw-2026-08-17-134712`, pitch **500 Hz** chosen by sweeping rather than taken from
the decoder, envelope floor **−44.6 dB**. Heights are above that floor.

| start | length | median | peak | one of `N4L`? |
|---|---|---|---|---|
| 20.21 s | 35 ms | 13.3 dB | 17.2 dB | no |
| 20.30 | 15 | 9.2 | 13.5 | no |
| 20.37 | 20 | 12.8 | 18.5 | no |
| 20.54 | 5 | 13.8 | 13.8 | no |
| 20.57 | 40 | 13.4 | 18.9 | no |
| 20.73 | 15 | 8.1 | 13.5 | no |
| 20.80 | 15 | 8.2 | 12.5 | no |
| 20.84 | 25 | 13.7 | 16.8 | no |
| 21.06 | 45 | 14.2 | 20.5 | no |
| **21.23** | **225** | **24.7** | **25.0** | **yes** |
| 21.48 | 55 | 24.6 | 24.9 | yes |
| 21.72 | 55 | 24.6 | 24.9 | yes |
| 21.81 | 55 | 24.4 | 24.9 | yes |
| 21.91 | 60 | 24.6 | 24.9 | yes |
| 22.01 | 55 | 24.7 | 25.0 | yes |
| 22.09 | 245 | 24.7 | 25.0 | yes |
| 22.49 | 60 | 24.4 | 25.0 | yes |
| 22.57 | 245 | 24.6 | 24.9 | yes |
| 22.86 | 55 | 24.6 | 24.9 | yes |
| 22.95 | 55 | 24.4 | 24.9 | yes |

**The statistic is the median of the envelope inside the mark, with the peak
beside it.** A keyed mark has a plateau, and the median is that plateau's own
height, defended against the rising and falling edges the smoother rounds off. The
peak is reported too because a mark shorter than the smoother's ten-millisecond
window never reaches its plateau at all, so a median alone depresses every short
mark whatever it is. That confound is real and the two columns are what let it be
read.

**The envelope shares no code with the gate** (§12.5). The gate's own threshold
decision is what put these marks where they are, so measuring their height with the
gate's machinery would be asking the instrument to grade itself. Quadrature
mixdown, a 10 ms boxcar, sampled every millisecond.

**They separate, and by a lot.**

- Station: 11 marks, 55 to 245 ms, heights **24.4 to 24.7 dB** on the median.
- Not the station: 9 marks, 5 to 45 ms, heights **8.1 to 14.2 dB**.
- **Gap on the median: 10.1 dB. Gap on the peak: 4.4 dB.**

**One correction to how the window was labelled, because it nearly reversed the
answer.** HM-DEC-144 records the elements as running 21.45 s to 23.01 s, and those
are the moments each element *ended*. `N4L`'s opening dah is 225 ms long and
*begins* at 21.23. Labelled by its start it came out as not the station's, sitting
at 24.7 dB among the chatter, and the separation read as an overlap of −0.3 dB. It
is the station's, and the table above is keyed on the end time.

### Task 2: the same measurement where it should fail

**`tightfist-easy`**, pitch 675 Hz swept, floor −51.3 dB, 38 marks.

**From 0.97 s onward there is one amplitude population and nothing else.** Every
mark from there to the end, dits of 90 to 100 ms and dahs of 270 to 275, sits
between **21.9 and 24.0 dB** above the floor. **The short marks are at full signal
exactly as predicted**, and there is no low group for a height-based rule to catch.
The seven marks before 0.97 s sit at −0.2 to 6.6 dB; those are the fixture's
opening, before the station starts.

**A mismatch with the instruction, reported rather than repaired.** It describes
this fixture's short population as *merged elements*. It is not: the short marks
here are the dits, at 90 to 100 ms, and the merged pairs are the long ones at 270
to 275. Last session's report said the same thing. **The prediction under test is
unaffected** and is confirmed either way, because what was being asked is whether
real keyed marks sit on the plateau, and they do.

**`cw-2026-08-18-004507`**, the control, pitch 500 Hz swept, floor −45.6 dB, 49
marks in the first ten seconds. Its real elements sit at **25.8 to 26.1 dB**, and
two things in it matter:

- **Length and amplitude are genuinely independent.** There are 15 and 20
  millisecond marks at 25.8 to 26.0 dB, which is full signal. A short mark is not
  automatically chatter, and amplitude is carrying information that length does
  not.
- **Some long marks have low medians and high peaks**: 90, 160, 180, 205 and 275
  millisecond marks with medians of 11 to 15 dB and peaks of 18 to 26. Those are
  marks the gate held open across a dip. **A rule reading the median alone would
  throw them away**, and that is the first thing a discriminator would have to
  answer for.
- One genuine sliver, 15 ms at 2.80 s, median −2.1 dB, peak 9.7 — far below
  everything else.

### Task 3: whether it survives a weaker station, measured

The separation above is one recording at one signal-to-noise ratio, and a margin
quoted from a single strong recording is the shape of claim this project has been
caught by before. So the recording was buried in band-shaped noise, a few decibels
at a time, and the same two groups measured again at each step across the same
span.

| noise added | quietest station mark | loudest other | gap |
|---|---|---|---|
| none | 24.6 dB | 14.5 dB | **10.1 dB** |
| −20 dB | 23.5 | 14.2 | **9.3** |
| −14 dB | 21.8 | 13.0 | **8.7** |
| −10 dB | 19.7 | 12.8 | **6.9** |
| −6 dB | 16.9 | 10.5 | **6.5** |

**The gap narrows and never closes.** Across that sweep the station's own marks
fall 7.7 dB and the chatter falls 4.0 dB, so the two come together slowly rather
than crossing. **The naive extrapolation was wrong**: subtracting ten decibels from
the station alone predicts the gap vanishing, and it does not, because the gate's
threshold follows the signal down and the noise it then admits is drawn from lower
amplitudes too.

**So it survives a station ten decibels weaker**, with about seven decibels of
margin left, on this recording.

## 2. What Tim should expect

**Yes. Amplitude separates chatter from real elements, by about ten decibels on
the recording where the answer is known, and the gap is still about seven decibels
after ten decibels of added noise.**

**Nothing was built and nothing in `src/` changed.** What is in the tree is the
measurement itself, `MarkAmplitudeTests`, so the numbers above can be re-run rather
than quoted. It asserts almost nothing on purpose: it reports heights and
classifies no mark, because a threshold is what four rulings have been spent
closing and this unit is not the place for a fifth.

**Build clean, no warnings. 2,083 tests, four failing, and they are the four
expected:**

- `CwSettledSilenceTests.APassThatReadSomethingEmitsSomething`
- `CwFarnsworthTests.TheBulletinDecodesToItsAnswerKey`
- `CwTerminalTests.ClearingTheTranscriptLeavesTheDecoderAlone`
- `ARecordingWithKeyingInItIsReadTests.TheDecoderSaysSomethingAboutIt`

Seven tests were added, all measurement, all passing.

**What a discriminator on this would look like, and it is not built.** It would
compare each mark's height against the heights of the marks around it rather than
against any fixed level: the fitted plateau of a window of marks is a quantity the
signal supplies, and a mark sitting many decibels below its own neighbours is not
one of them. **Two things would have to be answered before it could be trusted.**
The first is `004507`'s long marks with low medians and high peaks, which are real
elements the gate stretched across a dip and which a median-only rule would
discard. The second is that this rests on one station with a known callsign: a
second recording with adjudicated ground truth would double the evidence, and there
is exactly one.

## 3. What we should do next

- **`MedianOfShortCluster`, with the three-way fit, in one unit.** It was named two
  sessions ago as the line the callsign dies on, and this measurement says the
  information needed to fix it exists. That is the next unit and it now has a
  candidate behind it rather than a hope.
- Decide whether the discriminator reads the median, the peak or both. `004507`'s
  stretched marks are the case that decides it and they are already in the tree.
- Keep a second recording with a readable callsign when one is heard. All of this
  rests on `N4L`.
- The keying meter's thresholds are still unscored against an evening's roster.

## 4. What's blocking us

Nothing blocks the next unit.

**One ask, new this session.**

> **Amplitude is admitted as a candidate discriminator and a rule built on it must
> be relative rather than absolute, comparing each mark to the marks around it and
> never to a fixed number of decibels.**
>
> Measured on `cw-2026-08-17-134712`, where HM-DEC-144 settles which marks are the
> station's: its eleven elements sit 24.4 to 24.7 dB above the envelope floor and
> the nine chatter slivers sit at 8.1 to 14.2, a gap of 10.1 dB. On
> `tightfist-easy`, where every mark is real, there is one population at 21.9 to
> 24.0 dB and no low group at all. **The two recordings look different, which is
> what the candidate needed to show.**
>
> **The rule must be relative because the floor moves.** Across ten decibels of
> added noise the station falls from 24.6 to 19.7 dB above the floor while the
> chatter falls from 14.5 to 12.8; the gap holds at about seven decibels but
> neither figure stays put, so any fixed height would be wrong at one end of that
> range or the other. **That is the sixth instance of the error class four rulings
> have gone on closing**, and it is worth naming before the unit that builds this
> rather than after.
>
> **Rejected: treating this as settled enough to build on without the second
> question answered.** On `cw-2026-08-18-004507` there are real elements of 90 to
> 275 ms whose envelope median sits at 11 to 15 dB because the gate held them open
> across a dip, and their peaks are 18 to 26. A rule reading the median alone
> discards them. **Also rejected: reading the peak alone**, which on the callsign
> window gives a gap of 4.4 dB rather than 10.1 and is the weaker of the two
> statistics.

### Asks still outstanding

- **Amplitude as a relative discriminator, and which statistic it reads.** First
  made 2026-08-20, this session. Waiting on Tim, or on the unit that builds
  `MedianOfShortCluster`. Nothing is in the tree but the measurement.
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

**One item leaves the queue.** How to tell a merged element from chatter, asked
last session: amplitude does it on the evidence available, which is what this unit
was commissioned to find out.
