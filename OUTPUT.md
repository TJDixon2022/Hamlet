# OUTPUT.md

## 1. What Claude did

### Task 2's mechanism, and there are two of them, so task 3 did not run

Claude Code on the development machine, in `C:\Source\HamLet`, on `main`. The
prompt named `PROJECT: Hamlet` and the four gate checks all hold. **No radio is
attached** (HM-DEC-093). **No decoder code changed.**

Traced character by character from the first mark, on all four:

| audio | true dit | `LooksLikeMorse` first true | first within 5% | final dit |
|---|---|---|---|---|
| `farnsworth-light` | 100 ms | **mark 16**, coherence 0.40 | never (ends at −5%) | 95.0 |
| `cw-2026-08-17-013347` | 100.4 | **mark 12**, coherence 0.42 | **mark 16** | 87.0 |
| `farnsworth-heavy` | 56 | **never** | never | 47.0 |
| `cw-2026-08-17-134712` | 56.3 | never | never | 31.3 |

**The two light fists lose their opening to a warm-up.** Coherence starts at
nought and climbs as the window fills with the sender's own marks: on
`farnsworth-light` it runs 0.00, 0.01, 0.10, 0.18, 0.25, 0.30, 0.33 and crosses
its 0.35 floor at mark 16. Nothing is emitted before that, and beneath it
`IsReady` will not answer at all under twelve marks. On `CQ DE N0CALL K` the first
twelve marks are exactly `C`, `Q` and `D`, which is precisely what goes missing.
**The line is `LooksLikeMorse`'s `Coherence >= MinimumCoherence`, floored by
`IsReady`'s `_markCount >= MinimumMarks`.**

**The two heavy fists lose it to something else entirely, and it is not a warm-up
at all.** On `farnsworth-heavy` coherence is **0.00 flat from mark 5 onward**
while the separation runs 8 to 22 — the marks are cleanly two lengths, so this is
not a smear. The dit reads 44 to 47 against a true 56, so the fitted dah comes out
at 238 over 44, which is **5.4 dits**, outside the two-to-five band
`MeasureCoherence` will fit to. It falls back to the textbook three, every dah
then scores 2.4 dits of error, and **coherence cannot rise at all while the dit is
that short**. On `N4L` the same arithmetic is worse: a dit of 31.3 puts the dah at
7.6 dits.

**So the openings are not lost four ways, but they are lost two ways**, and the
order says to stop and report rather than repair two things at once. **Task 3 did
not run.**

- **Light fists**: the estimate is climbing and the coherence floor has not been
  reached. Time fixes it.
- **Heavy fists**: the estimate is short enough to push the fitted dah out of the
  band, and time does not fix it — `farnsworth-heavy` never crosses at all in
  fifteen seconds.

**One sentence for each, since that is what was asked.** The light fist loses its
opening because coherence has to climb from nothing and cannot answer before
twelve marks. The heavy fist loses its opening because a dit twenty per cent short
makes its dah look like five and a half dits, and `MeasureCoherence` abandons the
fitted dah past five and scores every dah against three instead.

### Task 1: the reference now reads a 4.25-dit fist

`fit_clock` refused any clock whose dah-to-dit ratio fell outside 2.5 to 3.8.
**That band is now an early acceptance rather than the only one**: a clock inside
it is taken as before, and a clock outside it is taken only if the two mark
clusters are separated by four times their own scatter, which is HM-DEC-095's
statistic rather than a wider constant.

**It can only add acceptances**, which is what keeps every other score fixed.

| fixture | before | after |
|---|---|---|
| **`farnsworth-heavy`** | **0% BAD FIXTURE** | **100% ok** |
| every other fixture | — | **unchanged** |

**Two things were tried and measured before that shape was settled on.** Replacing
the band outright took `fast-working` from 58% to nothing, because at five
decibels the marks scatter enough that the separation test refuses a clock the
band would have taken. And allowing the widened path in the slow-fist re-read took
`farnsworth-light` from 100% to 73%, because the narrower second pass produced a
clock the band would have rejected and the first one was better. **The refit is a
refinement, and a refinement that changes the answer is not one**, so the widening
is confined to acquisition.

`farnsworth-heavy` is out of `NotYetAdmissible`, which is now empty again.

### And admitting it found a third thing

**`TheToneIsFoundInRealisticAudio(farnsworth-heavy)` is a new red.** The fixture is
generated at 615 Hz and the decoder settles on **575**, forty hertz away and
outside the twenty-five the test allows. It reads three characters from there.

That is the fixture doing exactly what it was built for, and it is consistent with
everything above: `CwToneSurvey.MaximumRatio` is 3.8, this fist sends 4.25, so the
survey never calls it keyed and the tracker points at the loudest thing rather than
at a confirmed station.

## 2. What Tim should expect

### Task 4: whether the callsign survives

**On the lighter fist, yes. On the heavier one, no.**

`farnsworth-light` starts reading at the fourth character of `CQ DE N0CALL K` and
loses `CQ ` — **two characters and the space**. On a call of `CQ CQ DE N4L K` that
means it begins reading at `CQ DE`, and **the callsign survives whole**.

`farnsworth-heavy` starts reading at the tenth character and loses `CQ DE N0C` —
**nine characters**. On the same call it would begin somewhere inside the callsign
and **the callsign would not survive**.

**So a heavy fist calling CQ is still lost, and that is the number this phase is
about.** Nine characters of warm-up on a fourteen-character call leaves nothing
worth having.

**Nothing shipped to the decoder** and the two counts are where they were: 9 of 12
and 3 of 12.

**Build clean, no warnings. 2,117 tests, five failing.** Four are the expected:

- `CwSettledSilenceTests.APassThatReadSomethingEmitsSomething`
- `CwFarnsworthTests.TheBulletinDecodesToItsAnswerKey`
- `CwTerminalTests.ClearingTheTranscriptLeavesTheDecoderAlone`
- `ARecordingWithKeyingInItIsReadTests.TheDecoderSaysSomethingAboutIt`

**The fifth is new and is the point of admitting the fixture**:
`TheToneIsFoundInRealisticAudio(farnsworth-heavy)`, tone found at 575 Hz against a
generated 615. It was not there yesterday because the fixture was held out, and it
is a real defect rather than a broken test.

Nothing else moved. `ShortestVote` is still 5, `MaximumRatio` still 3.8,
`MinimumSeparation` untouched, no fixture re-cut, `Refine` still absent.

## 3. What we should do next

- **The heavy-fist mechanism first**, because it is the one that loses callsigns
  and because it is not a warm-up: a decoder that never crosses in fifteen seconds
  will never cross. The dit reads short, the fitted dah leaves the band, and
  coherence is pinned at nought. **Two adjudicated recordings and a generated
  fixture all show it**, which is the most evidence any defect in this project has
  ever had.
- **The light-fist warm-up second.** Twelve marks before the estimator will answer
  is three characters of a call, and that is a design figure rather than a defect.
  Whether it can be shortened without emitting an unresolved character is a real
  question.
- **The 575 Hz tone on the heavy fixture** is the third thread and probably the
  same root: the survey's ratio band refuses a 4.25 fist, so nothing is ever
  confirmed and the tracker follows loudness.
- Adjudicate `cw-2026-08-18-004507` when there is an evening for it.

## 4. What's blocking us

Nothing blocks the next unit.

**One ask, new this session.**

> **The heavy fist's opening is not lost to a warm-up and must not be worked on as
> one. `MeasureCoherence` abandons a fitted dah past five dits, and a short dit
> puts every heavy fist past it.**
>
> On `farnsworth-heavy` the marks are cleanly two lengths from mark five onward,
> separated by eight to twenty-two times their own scatter. What fails is that the
> dit reads 44 to 47 against a true 56, so the dah reads 5.4 dits, outside the band
> `MeasureCoherence` will fit to. It falls back to the textbook three and every dah
> scores 2.4 dits of error, so **coherence is pinned at nought for the whole
> recording** and no amount of further sending moves it. `N4L` is worse at 7.6
> dits.
>
> **That is circular and worth naming as such.** The dah looks implausible because
> the dit is short; the dit stays short because coherence never rises to let the
> estimate settle. The light fists escape it only because their dah at 2.7 dits
> stays inside the band even when the dit is fifteen per cent out.
>
> **Rejected: widening the five-dit bound.** It was measured and set as the point
> past which a long mark is a carrier, a fade or a key held down, and moving it to
> admit a fist is the error class six rulings have gone on closing. **Also
> rejected: treating this as the same problem as the light fists**, which would
> put one fix on two mechanisms and make the next session's evidence unreadable.

### Asks still outstanding

- **How a heavy fist escapes the circle** between a short dit and an out-of-band
  dah. First made 2026-08-20, this session. Nothing is in the tree and three
  pieces of audio show it.
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

**One item leaves the queue.** Whether the reference should be fixed to read a
4.25-dit fist: ruled this session, done, and `farnsworth-heavy` now scores 100%
with no other score moved.
