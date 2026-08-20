# OUTPUT.md

## 1. What Claude did

### Task 3's table, which is what this unit is judged on

Claude Code on the development machine, in `C:\Source\HamLet`, on `main`. The
prompt named `PROJECT: Hamlet` and the four gate checks all hold. **No radio is
attached, and nothing here needed one** (HM-DEC-093).

Both changes were built and measured four ways. Characters emitted, every capture:

| capture | today | `Refine` only | ratio only | both |
|---|---|---|---|---|
| `cw-2026-08-17-013347` | 8 | 8 | 8 | 8 |
| `cw-2026-08-17-013622` | 0 | 0 | 0 | 0 |
| **`cw-2026-08-17-134712`** | **0** | **0** | **0** | **0** |
| `cw-2026-08-18-004507` | 25 | 25 | 25 | 25 |
| `cw-2026-08-18-003016` | 38 | **43** | 38 | **42** |
| `cw-2026-08-18-003126` | 34 | **35** | 34 | **35** |
| `cw-2026-08-18-003758` | 14 | 14 | 14 | 14 |
| **`cw-2026-08-20-014854`** | 1 | **5** | 1 | **5** |
| **`cw-2026-08-20-014935`** | **0** | 0 | 0 | **3** |

**Which change did what.**

**The ratio change moves no character count anywhere.** Not one capture, in either
direction. What it moves is the survey's keyed-hop count: on `134712` from 0 to
200, on `004507` from 2,294 to 2,494, on `003016` from 3,294 to 3,694. It changes
what the survey is willing to call keying and changes nothing that reaches a
screen.

**`Refine` buys five characters on `003016` and one on `003126`, and costs four on
a recording with no keying in it.** The two together put three placeholders on
`cw-2026-08-20-014935`, which has never produced a character in this project's
history.

**Neither change, nor both, produces anything from the recording this whole line
of work is about.**

### Both changes are withdrawn, and here is the measurement that withdrew them

**Neither is in the tree.** This session ships no change to the decoder. The work
order's own preconditions are what withdrew them, and both failures are measured
rather than argued.

**`Refine` fails HM-DEC-048 as the order states it: "Neither may make it more
willing to guess."** Taking the dit from the marks alone is right, and the
reasoning holds up: `Refine` averaged an accurate mark-derived dit with the
sender's element gaps, HM-DEC-119 measured that the mark carries no error to
cancel, and HM-DEC-115 measured that a real fist's element gap is genuinely
shorter than its dit. On the synthetic fist the change does exactly what it should,
taking the estimated dit from 45 ms to 55 and the fitted dah from 5.2 dits to 4.27.

**And on the air it makes the decoder read `U EE ■ ■` out of a recording an
independent instrument says contains no keying at any pitch.** `cw-2026-08-20-014854`
goes from one character to five, all five marked unsure, and the hops on which
`LooksLikeMorse` is true go from 8 to 469. Its estimated dit goes from 42.7 ms to
90.0, and a longer dit makes gate-chopped noise land nearer the two fitted centers.
**HM-DEC-090 already ruled on marking as a substitute for silence**: seventeen
hundred characters once came out of half a minute of band noise, every one marked,
and marking them was not enough.

**The ratio change fails on its own premise, and the instruction's account of it
is wrong.** Widening `MaximumRatio` from 3.8 to 5.0 does **not** let the survey
find the fist on `134712`. With the bound widened, the two candidates it accepts on
that recording are:

```
625 Hz  dit 31.3 ms  dah 150.0 ms  ratio 4.80  separation 6.4
600 Hz  dit 37.0 ms  dah 160.0 ms  ratio 4.32  separation 4.2
```

**Neither is the station.** The gate and the keying meter both put the fist at
500 Hz with a 55 ms dit and a 235 ms dah. The 500 Hz bin is still rejected with the
bound at five, so **`MaximumRatio` was never what rejected it** — something else in
the survey did, and finding out which is HM-OPEN-054's ground.

**What the widened bound does admit is a phantom.** On `cw-2026-08-20-014935`, the
recording with no keying in it, the survey accepts `875 Hz dit 58.4 dah 263.3 ratio
4.51 separation 6.0` and claims keying on 200 hops.

**And it breaks eight tests, one of which is written on this very recording.**
`CwToneSurveyTests.ACarrierNeverConvincesTheTrackerItIsAStation` reads
`cw-2026-08-17-134712.wav` and asserts the tracker never claims keying in it,
because **HM-DEC-095 characterised the strong signal in that recording as a
carrier**. Also failing: `NoKeyingIsClaimedWhereNoneWasFound`,
`FindingTheToneIsNotClaimingSomebodyIsSending`, and tone-finding on
`prosigns-easy`, `prosigns-edge` and `tightfist-easy`.

**So Task 2 lands on parked ground after all, by a route the order did not
anticipate.** The order separated "how wide a fist counts as Morse" from "how the
survey tells keying from a carrier" and permitted the first. **On this recording
they are the same question**: the ruling in the tree says that signal is a carrier,
the keying meter and the gate say there is a 55 ms fist in it, and widening the
fist band is precisely the act of overturning the first with the second. The order
says to stop if the work reaches for the distinguisher. It reached.

### Task 1, what `Refine` was changed to and why

`Refine` averaged the mark-derived dit with the mean of the sender's short gaps, on
the premise that a mark measured at a threshold reads long by the same amount the
gap after it reads short, so the two errors cancel. **Both halves of that premise
have already been measured false in this repository**: HM-DEC-119 put marks of
known length through Hamlet's own detector and found the gate reads 100 to 110 ms
for a true 100 at every speed, so the mark carries nothing to cancel, and
HM-DEC-115 measured a real fist off the air whose element gap is genuinely shorter
than its dit, 40 ms against 57, because that is how people send. The average was
therefore an accurate number blended with a short one and it came out short. The
replacement takes the dit from the marks alone, which is the estimate HM-DEC-119
measured as accurate, and it is fitted from the signal rather than being another
constant: it is the median of the fitted short-mark cluster, the same fit
`ClassifyMark` already cuts on.

**The dit, before and after, for all nine captures:**

| capture | dit today | dit with `Refine` changed |
|---|---|---|
| `cw-2026-08-17-013347` | 87.0 ms | 100.0 ms |
| `cw-2026-08-17-013622` | 21.6 | 25.0 |
| `cw-2026-08-17-134712` | 24.5 | 25.0 |
| `cw-2026-08-18-004507` | 54.2 | 60.0 |
| `cw-2026-08-18-003016` | 47.9 | 55.0 |
| `cw-2026-08-18-003126` | 49.8 | 50.0 |
| `cw-2026-08-18-003758` | 34.3 | 40.0 |
| `cw-2026-08-20-014854` | 42.7 | 90.0 |
| `cw-2026-08-20-014935` | 40.9 | 105.0 |

**A mismatch against the instruction, reported rather than filled in.** It asks for
the fitted dah in dits for all nine as well. That figure is an internal of the mark
fit with no reader on it, and building one for a change that was then withdrawn was
not worth the session. **The one place it was measured is the synthetic fist**,
where it goes from 5.2 dits to 4.27, which is the whole mechanism the change exists
to correct. The dit column above is the same quantity one step earlier and it is
measured for all nine.

`TheDitComesOutShortWhenTheGapIsShorterThanIt` **still asserts 40 to 50 ms and still
records 45**, because the change that would have moved it is not in the tree. It was
not deleted and it was not weakened.

### Task 4, the fixture

`ARecordingWithKeyingInItIsReadTests.TheDecoderSaysSomethingAboutIt` **stays red**,
in all four columns. Neither change was adjusted to make it pass.

**Where it dies now is where it died before, and the third cause is now visible.**
It still dies at `LooksLikeMorse`, which is false on every hop in every column. But
with `Refine` changed, `134712`'s estimated dit reads **25.0 ms** while the fist in
it sends 55, and the tracked pitch moves to 525 Hz. The reason is arithmetic rather
than mysterious: **the fist occupies about six seconds of a thirty-second
recording**, and the speed estimator's window is the last twenty marks. The other
twenty-four seconds are gate-chopped noise, they supply most of those twenty marks,
and so neither the dit nor the two mark clusters ever describe the station. Last
session showed the same audio cut to the four cleanest seconds produces a character.

**A decoder that reads a station occupying a fifth of its window is a different
piece of work from either change measured tonight**, and it is the honest next
question.

### Task 5, the two from the 19th

Run in all four columns. **Today: 1 and 0. With `Refine`: 5 and 0. With the ratio:
1 and 0. With both: 5 and 3.** The five read `U EE ■ ■` and the three read `■■■`.

**That is invention and it is reported as invention.** The keying meter sweeps both
recordings 400 to 1200 Hz and finds medians of five and seven milliseconds in every
window, against 44 to 57 on the four that decoded. There is nothing being keyed in
them, so the honest output is silence.

`NothingIsReadFromAudioWithNoKeyingTests` is added and pins it: neither press may
produce more than it does today, and the same independent instrument re-measures
both recordings in the tree rather than being quoted. **The single character on
`-014854` predates all of tonight and is allowed for rather than approved of**, and
the test says so on its face.

## 2. What Tim should expect

**No. The decoder does not read `cw-2026-08-17-134712`, and nothing shipped
tonight.** Both candidate changes were built, measured four ways and withdrawn, and
the tree is where it was this morning apart from four new tests.

**What you have instead is the measurement**, and it is worth more than either
change would have been:

- **The ratio change was aimed at the wrong thing.** `MaximumRatio` is not what
  rejects that station. Widened, the survey finds two other candidates at 600 and
  625 Hz and still not the 500 Hz fist, and it manufactures a station on a
  recording with nothing in it.
- **`Refine` is right in principle and cannot ship as it stands.** It reads five
  characters out of an empty band.
- **A third cause is now named.** The fist is about six seconds of a thirty-second
  recording and the speed estimator looks at the last twenty marks, so the
  twenty-four seconds of noise own the estimate.
- **There is a contradiction in the tree that has to be settled by a person.**
  `CwToneSurveyTests.ACarrierNeverConvincesTheTrackerItIsAStation` is written on
  this exact recording and asserts, on HM-DEC-095's authority, that the strong
  signal in it is a carrier. The keying meter and the decoder's own gate both read
  a 55 ms dit and a 235 ms dah in it. **Those cannot both be right**, and which one
  is wrong decides whether the last three sessions were chasing a defect or a
  ghost.

**Build clean, no warnings. 2,073 tests, four failing, and they are the four
expected:**

- `CwSettledSilenceTests.APassThatReadSomethingEmitsSomething`
- `CwFarnsworthTests.TheBulletinDecodesToItsAnswerKey`
- `CwTerminalTests.ClearingTheTranscriptLeavesTheDecoderAlone`
- `ARecordingWithKeyingInItIsReadTests.TheDecoderSaysSomethingAboutIt`

Four tests were added and all pass. Nothing in the gate, the settled pass or the
keying meter was touched, and `ShortestVote` is still 5.

Committed and pushed to `main`.

## 3. What we should do next

- **Settle the carrier question first.** It is the one thing everything else waits
  on, it needs your ear rather than another session's measurement, and three
  sessions have now been spent on a recording the repository's own ruling says
  holds no station.
- If it is a station: `Refine` with a guard that keeps empty bands silent, and the
  short-window problem, in that order. Both are named and measured.
- If it is a carrier: `ARecordingWithKeyingInItIsReadTests` is asserting the wrong
  thing and should be reversed, and the keying meter's thresholds need looking at,
  because it scored this recording higher than the four that decoded.
- The ratio band stays at 3.8 either way. Nothing measured tonight supports moving
  it and one measurement says it is holding a carrier out.

## 4. What's blocking us

**One thing blocks the next unit, and it is the ask below.**

> **Whether `cw-2026-08-17-134712` contains a station or a carrier is Tim's ear,
> and no further work on it happens until he has listened to it.**
>
> The repository holds both answers and they are incompatible. HM-DEC-095
> characterised the strong signal in this recording as a carrier and
> `ACarrierNeverConvincesTheTrackerItIsAStation` asserts it, reading this file by
> name. Two sessions later the keying meter, which shares no code with the decoder,
> scored one of its six-second windows 0.37 at 500 Hz with a 54 ms element, the
> highest score it has measured on any recording here, higher than the four that
> decoded. Last session the decoder's own gate read that same stretch as dits of
> 55 ms, dahs of 235 and element gaps of 35, and the two instruments agreed on the
> element length to within a millisecond.
>
> **Two instruments agreeing is strong and it is not a person hearing Morse.** The
> file is thirty seconds long and the stretch in question is four of them.
>
> **Rejected: settling it by measurement.** Three sessions have tried and produced
> two instruments that agree with each other and disagree with a ruling. A fourth
> would produce a third instrument. **Also rejected: proceeding on the meter's
> word** and reversing the carrier test, because that is a session overturning a
> ruling on §0.0 ground, which §12.1 puts outside what a session may record.

### Asks still outstanding

- **Whether `cw-2026-08-17-134712` holds a station or a carrier.** First made
  2026-08-20, this session. Waiting on Tim listening to four seconds of a WAV in
  the tree. **It blocks the coherence work, the `Refine` work and the fixture
  test.**
- **The keying meter's provisional thresholds.** First made 2026-08-20. Waiting on
  one evening's roster scored against the `meter` column. **The ask above bears on
  this one**: if that recording is a carrier, the meter scored a carrier higher
  than four real stations.
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

**One item leaves the queue.** `CwToneSurvey.MaximumRatio` was ruled workable this
session and has been worked: it was widened, measured, found not to be the cause,
found to admit a phantom on an empty recording, and reverted. It needs no further
ruling.
