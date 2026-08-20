# OUTPUT.md

## 1. What Claude did

### Task 1: the de-glitch is not the cause, and bypassing it makes things worse

Claude Code on the development machine, in `C:\Source\HamLet`, on `main`. The
prompt named `PROJECT: Hamlet` and the four gate checks all hold. **No radio is
attached** (HM-DEC-093). **Nothing in `src/`, the tests or the reference changed.**

The gate's mark lengths on generated audio, with the vote window clamped to a
single measurement so no median runs at all:

| fixture | true dit | **with de-glitch** | **without** | true dah | with | without |
|---|---|---|---|---|---|---|
| `exchange-easy` | 100.0 | 102.8 (+3%) | **88.3 (−12%)** | 300.0 | 300.2 | 301.7 |
| `coverage-easy` | 100.0 | 101.4 (+1%) | 100.8 (+1%) | 300.0 | 303.0 | 301.0 |
| `farnsworth-light` | 100.0 | 102.6 (+3%) | 100.5 (0%) | 274.0 | 280.2 | 279.8 |
| **`fast-easy`** | 48.0 | 45.3 (−6%) | **42.3 (−12%)** | 144.0 | 145.9 | 145.6 |
| **`farnsworth-heavy`** | 56.0 | 48.9 (−13%) | **45.3 (−19%)** | 238.0 | 243.1 | 234.5 |

**The de-glitch is holding short marks together, not eating them.** Take it away
and `farnsworth-heavy` goes from thirteen per cent short to nineteen, `fast-easy`
from six to twelve, and `exchange-easy` from three per cent long to twelve short.

**So `ShortestVote` is not the mechanism.** It was unparked on the strength of a
suspicion, the suspicion was testable, and it is wrong. **It stays at 5, now on
measured evidence rather than on a park**, and task 2 did not run.

**The widths, as numbers rather than as an argument:**

| fixture | dit in hops | dah in hops | analysis window |
|---|---|---|---|
| `exchange-easy`, `coverage-easy`, `farnsworth-light` | 20.0 | 54.8–60.0 | **50 ms** |
| `farnsworth-heavy` | **11.2** | 47.6 | **20 ms** |
| `fast-easy` | **9.6** | 28.8 | **20 ms** |

**And that kills the obvious follow-on suspect too.** HM-DEC-119's own record
offers a rounded-top explanation — "a Goertzel window rounds the top of a short
mark, so its width six decibels below its own apex is far less than its base". But
the tracker runs a **20 ms** window on the two fixtures that read short and a
**50 ms** window on the three that do not. **The window is narrower where the error
is worse**, which is the wrong way round for that story.

**What the error is not**, from the same table: not a fixed number of
milliseconds, since a true 56 loses 7.1 and a true 48 loses 2.7; not a general
mark bias, since the dahs are within two per cent everywhere; and not the median
filter, since removing it doubles the loss.

### Task 3: HM-DEC-119 is corrected, and that is the finding worth keeping

Recorded as **HM-DEC-146**, superseding HM-DEC-119 on this point and nothing else,
with the table in the entry and indexed in `CLAUDE.md` §1.

That ruling says the gate is "accurate to within one hop at every speed" and that a
mark reads long by nought to ten per cent. **It is true at a hundred milliseconds
and false at fifty-six.**

**It matters more than the figure does.** HM-DEC-119 was measured at one speed and
generalised to every speed, and the generalisation became the premise of four
sessions of work: if a mark reads long and the next gap reads short by the same
amount, averaging them cancels the error, which is the entire argument for
`Refine`'s averaging. **On a fifty-six millisecond dit the mark reads short and the
gap reads true, so there is nothing to cancel.** A ruling that is right about the
audio it was taken from and wrong about the rest is the most expensive kind,
because everything downstream cites it rather than re-measuring.

### The naming correction, carried forward as instructed

`Refine` is the method at `CwTiming.cs:1151`, called at line 649, and it is in the
tree and always has been. **What has been proposed and withdrawn four times is its
removal.** This report says "`Refine`'s removal" where that is meant.

## 2. What Tim should expect

### Task 4: the callsign, and the numbers have not moved

**Nothing shipped, so both answers stand from last session.** On the lighter fist
the callsign survives: `farnsworth-light` loses `CQ ` and starts at the fourth
character, so `CQ CQ DE N4L K` comes through from `CQ DE` onward. **On the heavier
fist it does not**: nine characters go and reading begins inside the callsign.

`farnsworth-light` is still **9 of 12**, `farnsworth-heavy` still **3 of 12**, and
**`cw-2026-08-17-134712` still emits nothing.**

**What you have instead is a suspect eliminated and a ruling corrected.** The
de-glitch has been the named suspect for two sessions and it is not the cause —
it is the only thing keeping the heavy fist's dit from reading nineteen per cent
short instead of thirteen. `ShortestVote` can go back on the shelf with a
measurement behind it rather than a park.

**Build clean, no warnings. 2,117 tests, five failing, and they are the five
expected:**

- `CwSettledSilenceTests.APassThatReadSomethingEmitsSomething`
- `CwFarnsworthTests.TheBulletinDecodesToItsAnswerKey`
- `CwTerminalTests.ClearingTheTranscriptLeavesTheDecoderAlone`
- `ARecordingWithKeyingInItIsReadTests.TheDecoderSaysSomethingAboutIt`
- `TheToneIsFoundInRealisticAudio(farnsworth-heavy)`

No test was added or changed and the bulletin's count is untouched.

## 3. What we should do next

- **Find what actually shortens a short mark**, with the two suspects that have
  been eliminated written down so the next session does not re-try them: the
  de-glitch makes it worse, and the analysis window is narrower where the error is
  worse. **What is left is the envelope's own shape between the hop grid and the
  threshold**, and `farnsworth-heavy` is fifteen seconds of noise-free audio with
  every edge known, so it is fully observable.
- **The light fist's estimator window is still the other mechanism** and it is
  cheap: `Refine` averages every gap under twice the mark-derived dit, and whether
  a sender's character gaps fall inside that span depends on the sender's own
  ratio. On `farnsworth-light` that is 150 against 200 and it costs seven and a
  half milliseconds. **The gap classes are already fitted a few lines away.**
- **Consider taking the light fist first.** It is one line, the mechanism is
  understood, and it would put a second fixture inside five per cent — where the
  heavy fist now needs a mechanism nobody has named.

## 4. What's blocking us

Nothing blocks the next unit. **The heavy fist has no named mechanism left.**

**One ask, new this session.**

> **The short-mark error is not the de-glitch and not the analysis window, and the
> next unit should be told both rather than re-testing them.**
>
> Bypassing the de-glitch entirely takes `farnsworth-heavy` from thirteen per cent
> short to **nineteen**, `fast-easy` from six to **twelve**, and `exchange-easy`
> from three per cent long to twelve short. It is holding short marks together.
> `ShortestVote` stays at 5 and can be re-parked with a measurement behind it.
>
> The rounded-top explanation HM-DEC-119 offers does not fit either: the tracker
> runs a **20 ms** analysis window on the two fixtures that read short and **50 ms**
> on the three that do not, so **the window is narrower where the error is worse**.
>
> **What is left is between the hop grid and the threshold**: a 56 ms mark is 11.2
> hops and a 300 ms dah is 60, and the error is neither a fixed fraction nor a fixed
> number of milliseconds — a true 56 loses 7.1 ms and a true 48 loses 2.7.
>
> **Rejected: moving `ShortestVote` anyway** because it changes the number in the
> right direction. It changes it by making the gate slower to notice a mark, which
> is not the same as measuring one correctly, and it would bury a mechanism nobody
> has found. **Also rejected: correcting the estimator for the gate's short
> reading**, which is a constant applied downstream of the measurement that produced
> it and leaves the gate still reporting a length the audio does not contain.

### Asks still outstanding

- **What shortens a short mark**, with the de-glitch and the analysis window both
  eliminated. First made 2026-08-20, this session.
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

**One item leaves the queue.** Whether the gate's measurement of a short mark may
be opened: it was, `ShortestVote` was tested and cleared, and HM-DEC-119 is
corrected as HM-DEC-146.
