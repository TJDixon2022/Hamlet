# OUTPUT.md

## 1. What Claude did

### Task 1: the light and heavy fists lose the dit at different steps, so task 2 did not run

Claude Code on the development machine, in `C:\Source\HamLet`, on `main`. The
prompt named `PROJECT: Hamlet` and the four gate checks all hold. **No radio is
attached** (HM-DEC-093). **Nothing changed anywhere: not `src/`, not the tests, not
the reference.**

Every mark and gap the gate reports on generated audio with no noise in it, against
what was generated:

| fixture | true dit | **gate reads** | true dah | gate reads | true element gap | gate reads | fitted dit |
|---|---|---|---|---|---|---|---|
| `coverage-easy` | 100.0 | **101.4 (+1%)** | 300 | 303.0 (+1%) | 100 | 96.7 (−3%) | 99.5 |
| `exchange-easy` | 100.0 | **102.8 (+3%)** | 300 | — | 100 | 96.8 (−3%) | 99.5 |
| `farnsworth-light` | 100.0 | **102.6 (+3%)** | 274 | 280.2 (+2%) | 73 | 67.1 (−8%) | **95.0** |
| `fast-easy` | 48.0 | **45.3 (−6%)** | 144 | 145.9 (+1%) | 48 | 48.5 (+1%) | 48.0 |
| `farnsworth-heavy` | 56.0 | **48.9 (−13%)** | 238 | 243.1 (+2%) | 36 | 36.3 (+1%) | **47.0** |

**They lose it in different places, and that is the finding.**

**`farnsworth-light` loses it all in the estimator.** The gate hands over 102.6 for
a true 100 — long, as expected — and the fit takes it to **95.0**. Seven and a half
milliseconds go between the gate and `DitSamples`. `Refine` averages the
mark-derived dit with the mean of every gap under twice it, and twice 100 is 200,
so this sender's **150 ms character gaps are inside that window along with its 73 ms
element gaps**. The average lands low.

**`farnsworth-heavy` loses most of it before the estimator sees anything.** The
gate hands over **48.9 for a true 56** — short by thirteen per cent — and the fit
takes it only to 47.0. Seven of the twelve milliseconds are gone at the gate, and
`Refine` costs about two more; its window is twice 48.9, so this sender's 165 ms
character gaps are outside it and only the 36 ms element gaps are averaged in.

**One sentence each, since that is what was asked.** The light fist's dit is short
because `Refine` averages the sender's character gaps into it. The heavy fist's dit
is short because the gate reports its dits thirteen per cent short before anything
is fitted at all.

**So the order's instruction applies: stop and report.** Task 2 did not run.

### The ruling this unit was built on does not hold at short dits

The order carries HM-DEC-119 forward as a ruling in force: *the gate reads 100–110
ms for a true 100 at every speed; a mark is long by nought to ten per cent, not
short.* It asked for that to be confirmed on this audio or reported as not holding.

**It holds at 100 milliseconds and it does not hold below.**

| true dit | what the gate reads |
|---|---|
| 100 ms | 101.4, 102.8, 102.6 — **long by 1 to 3%**, as ruled |
| 48 ms | **45.3 — short by 6%** |
| 56 ms | **48.9 — short by 13%** |

**The dahs are long by 1 to 2% at every length**, so this is not the gate being
wrong about marks in general. It is short marks specifically. The analysis hop is
5 ms and `CwGate.ShortestVote` is five measurements, so a 56 ms dit is eleven hops
being median-filtered by a window of five, while a 300 ms dah is sixty. **Naming
the exact gate mechanism would mean touching the gate, and `ShortestVote` is parked
at 5**, so it is measured here and left.

**That matters beyond this unit**, because "the mark reads long, never short" has
been load-bearing in four sessions of reasoning about `Refine`.

### Two rows in that table are not clean, and are marked

`prosigns-easy` and `tightfist-easy` were measured too and are left out of the
table above. Both send prosigns or run their elements together, so a bucket split
at the midpoint of dit and dah catches merged pairs as well as dits and the numbers
are not comparable. `prosigns-easy` reads its "dits" at 92.2 for a true 100 and
`tightfist-easy` at 101.6 for a true 94; **neither figure means what the others
mean** and neither is used above.

### A naming mismatch worth recording once

The order states that **`Refine` is not in the tree**. The method `Refine` is at
`CwTiming.cs:1151` and is called at line 649; it has been there throughout and it
is the mark-and-gap average. What has been proposed and withdrawn four times is its
**removal**, and the orders have been using the name `Refine` for that removal.

It matters here because task 1's answer is partly about `Refine` itself: on the
light fist it is the whole of the loss.

## 2. What Tim should expect

### Task 4: whether the callsign survives, and the numbers have not moved

**Nothing shipped, so both answers are last session's.** On the lighter fist the
callsign survives: it loses `CQ ` and starts reading at the fourth character, so
`CQ CQ DE N4L K` comes through from `CQ DE` onward. **On the heavier fist it does
not**: nine characters go, and reading begins somewhere inside the callsign.

`farnsworth-light` is still 9 of 12 and `farnsworth-heavy` still 3 of 12.

**What you have instead is the reason the heavy fist is different**, and it is not
where this week's reasoning has been looking. Its dit is not dragged down by the
estimator. **It arrives at the estimator already thirteen per cent short**, from a
gate whose measurement of a 56 ms mark is not the same quality as its measurement
of a 100 ms one.

**Build clean, no warnings. 2,117 tests, five failing, and they are the five
expected:**

- `CwSettledSilenceTests.APassThatReadSomethingEmitsSomething`
- `CwFarnsworthTests.TheBulletinDecodesToItsAnswerKey`
- `CwTerminalTests.ClearingTheTranscriptLeavesTheDecoderAlone`
- `ARecordingWithKeyingInItIsReadTests.TheDecoderSaysSomethingAboutIt`
- `TheToneIsFoundInRealisticAudio(farnsworth-heavy)`

### Task 3: the two threads, reported and not worked on

**The tone still settles at 575 Hz on a fixture generated at 615**, unchanged,
because nothing shipped. The order anticipated this correctly: even with the dit
right, a 4.25 fist is still past `MaximumRatio`'s 3.8, so the survey would still
never confirm it and the tracker would still follow loudness.

**The light fist's warm-up is unchanged too**: `LooksLikeMorse` first goes true at
**mark 16** on `farnsworth-light` and **mark 12** on `cw-2026-08-17-013347`.

## 3. What we should do next

- **The gate's measurement of a short mark**, because that is where the heavy
  fist's twelve milliseconds actually go. It needs a ruling first: `ShortestVote`
  has been held at 5 for eight units and this is the first measurement that points
  at it rather than past it.
- **`Refine`'s window, separately**, because it is the light fist's whole problem
  and a different fix. Twice the mark-derived dit is an arbitrary span, and whether
  it admits a sender's character gaps depends on that sender's ratio: 150 against
  200 for the light fist, 165 against 112 for the heavy one. **The gap classes are
  already fitted a few lines away.**
- **Do not do both in one unit.** They are two mechanisms and one session of
  evidence apiece is what has made this week's findings readable.
- Adjudicate `cw-2026-08-18-004507` when there is an evening for it.

## 4. What's blocking us

**The heavy fist is blocked on a ruling about the gate**, which every order this
week has parked.

**One ask, new this session.**

> **HM-DEC-119 is narrowed: the gate reads a mark long by nought to three per cent
> at a hundred milliseconds and short by six to thirteen at forty-eight and
> fifty-six. It is not "long at every speed", and the heavy fist's short dit is the
> gate's rather than the estimator's.**
>
> Measured on generated audio with no noise in it and a dit known to the
> millisecond. A true 100 reads 101.4, 102.8 and 102.6 across three fixtures. A
> true 48 reads 45.3 and a true 56 reads 48.9. **The dahs are long by one to two
> per cent at every length**, so this is short marks specifically rather than the
> gate being wrong about marks.
>
> **That relocates the heavy fist's defect.** Of the twelve milliseconds between a
> true 56 and a fitted 47, seven are gone before the estimator is handed anything.
> Four sessions have looked for it in the fit.
>
> **The obvious suspect is parked and this session did not touch it.** The hop is
> 5 ms and `CwGate.ShortestVote` is five measurements, so a 56 ms dit is eleven
> hops through a five-wide median while a 300 ms dah is sixty. **HM-OPEN-053 has
> held `ShortestVote` at 5 for eight units and every order has restated it**, so
> this is handed back rather than tried.
>
> **Rejected: correcting the estimator for the gate's short reading.** That is a
> constant applied downstream of the measurement that produced it, which is the
> error class six rulings have gone on closing, and it would leave the gate still
> reporting a length the audio does not contain. **Also rejected: fixing `Refine`
> and calling the heavy fist done**, because `Refine` accounts for two of its twelve
> milliseconds and the fitted dah would still sit past five dits.

### Asks still outstanding

- **Whether the gate's measurement of a short mark may be opened**, and
  HM-DEC-119 narrowed to the length it was measured at. First made 2026-08-20,
  this session. Nothing is in the tree.
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

**One item leaves the queue.** How a heavy fist escapes the circle between a short
dit and an out-of-band dah: measured this session, and the answer is that most of
the short dit is the gate's rather than the fit's.
