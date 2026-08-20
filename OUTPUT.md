# OUTPUT.md

## 1. What Claude did

### Task 1: the separation figures, and they do not separate

Claude Code on the development machine, in `C:\Source\HamLet`, on `main`. The
prompt named `PROJECT: Hamlet` and the four gate checks all hold. **No radio is
attached** (HM-DEC-093). The amplitude rule from last session is in the tree and
`Refine` is not, as the instruction states.

**What was fitted.** For a candidate interval, every transition is turned into an
angle by how far through the interval it falls, and the length of the mean of those
angles is taken. One means every edge lands at the same point in the cycle; nought
means they are spread evenly around it. The interval is swept from 15 to 250
milliseconds in half-millisecond steps and the best kept. **Nothing is taken from
the speed estimator, no interval is assumed, and nothing is asked about
characters** — only the times the signal turned on and off.

**On synthesized audio it works beautifully.** Every easy-tier fixture finds its
own dit to within a millisecond:

| fixture | agreement | interval found | true dit |
|---|---|---|---|
| `coverage-easy` | 0.954 | 100 ms | 100 |
| `exchange-easy` | 0.938 | 100 | 100 |
| `fast-easy` | 0.832 | 48 | 48 |
| `prosigns-easy` | 0.754 | 100 | 100 |
| `tightfist-easy` | 0.702 | 88 | 88 |

**On real audio, across a whole recording, it says nothing at all.**

| capture | whole-file agreement | best window | window median |
|---|---|---|---|
| `cw-2026-08-17-013347` | 0.254 | 0.660 | 0.483 |
| `cw-2026-08-17-013622` | 0.110 | 0.485 | 0.344 |
| **`cw-2026-08-17-134712`** (holds `N4L`) | **0.177** | 0.471 | 0.364 |
| `cw-2026-08-18-004507` | 0.242 | 0.597 | 0.450 |
| `cw-2026-08-18-003016` | 0.148 | 0.618 | 0.457 |
| `cw-2026-08-18-003126` | 0.160 | 0.677 | 0.471 |
| `cw-2026-08-18-003758` | 0.166 | 0.731 | 0.360 |
| **`cw-2026-08-20-014854`** (holds nothing) | **0.122** | 0.437 | 0.335 |
| **`cw-2026-08-20-014935`** (holds nothing) | **0.116** | 0.471 | 0.336 |
| **`134712`'s callsign window**, 21.45–23.01 s | **0.677 at 48 ms** | 0.773 | 0.760 |

**The recording with a proved station in it scores 0.177 and the recording with
nothing in it scores 0.116.** A recording holding a station is mostly band noise
too, so a figure taken over the whole of one measures the noise.

**The callsign window itself scores 0.677 at 48 ms**, which is the candidate
working exactly as hoped on the four seconds where the answer is known, and it is
not enough, because a gate does not get to choose its window.

**At the moment of emission, which is where a gate would stand, the two overlap.**

| | lowest | tenth percentile | median |
|---|---|---|---|
| `cw-2026-08-18-003016`, real | **0.389** | 0.542 | 0.673 |
| `cw-2026-08-18-004507`, real | 0.488 | 0.515 | 0.600 |
| `cw-2026-08-18-003758`, real | 0.534 | 0.565 | 0.704 |
| `cw-2026-08-17-013347`, real | 0.597 | 0.633 | 0.688 |
| **`cw-2026-08-20-014854`, invented** | **0.470** | — | 0.470 |

**A real character comes out at 0.389 and an invented one at 0.470.** No line
drawn on this statistic keeps the first and rejects the second.

**And in the configuration the gate exists to enable it is worse.** With `Refine`
applied — the change this gate was commissioned to unblock — `cw-2026-08-20-014854`
invents **nine** characters at agreements from **0.456 to 0.533**, while
`prosigns-easy` emits a real one at **0.493** and `tightfist-easy` at **0.497**.
HM-DEC-114 makes those pass or fail. **A gate low enough to keep the easy tier
whole admits everything the empty band invents.**

**So the candidate is dead, and tasks 2, 3 and 4 did not run**, as Task 1
instructs. Nothing was built on it.

**One prediction of the instruction confirmed and one refined.** Keying is periodic
and noise is not, and that is true: it is why the easy tier scores 0.70 to 0.95 and
synthesized noise produces no transitions to fit at all. What the instruction did
not anticipate is that **a real off-air recording is not the keyed case**. It is
noise with a station in it, and the gate has to run over both at once.

**A second candidate was tried and rejected on the same evidence.** Rather than
the agreement itself, how far the best interval's peak stands above the background
of all the intervals tried — a keyed signal should give a tall isolated peak and
noise a flat curve, and it is relative rather than a level. Measured, it separates
worse: `134712`'s callsign window reaches a prominence of 4.22 against 4.08 for
`014854` and 4.34 for `013622`, and every real capture sits between 2.7 and 8.9
alongside them.

### What is in the tree

`ACarrierClockDoesNotSeparateTests` keeps the measurement, so the finding is
reproducible rather than remembered. It asserts what was measured: that the fit
works on clean audio, that it says nothing over a whole real recording, and that a
real character and an invented one come out on the wrong side of each other.
**Nothing in `src` reads it and no `src` file changed this session.**

## 2. What Tim should expect

**Nothing shipped, and no, the decoder still does not read
`cw-2026-08-17-134712`.** What you are running tonight is exactly what you were
running after the last session: the amplitude rule, and no `Refine`.

**What you have instead is a candidate eliminated with numbers.** Three ideas have
now been measured against HM-OPEN-054 and all three are gone: the tone survey's
verdict, which let a carrier recording produce 33 characters; the ratio band, which
would discard `N4L`; and now the transition clock, which cannot be set anywhere
that keeps the easy tier and rejects an empty band.

**The one encouraging number.** `134712`'s callsign window fits a clock at 48 ms
with an agreement of 0.677, the highest of any real window measured. **The
information is in the audio.** What is missing is a way to ask for it that does not
also have to survive the twenty-six seconds of band noise either side.

**Build clean, no warnings. 2,096 tests, four failing, and they are the four
expected:**

- `CwSettledSilenceTests.APassThatReadSomethingEmitsSomething`
- `CwFarnsworthTests.TheBulletinDecodesToItsAnswerKey`
- `CwTerminalTests.ClearingTheTranscriptLeavesTheDecoderAlone`
- `ARecordingWithKeyingInItIsReadTests.TheDecoderSaysSomethingAboutIt`

Seven tests were added, all measurement, all passing. The easy tier is whole.
`ShortestVote` is still 5, `MaximumRatio` is still 3.8, and the gate, the survey
and the meter were not touched.

**The boundary was not crossed.** Nothing here fitted anything to characters, asked
which character a mark belonged to, or distinguished element gaps from character
gaps. The clock was fitted to the on and off times and nothing else.

## 3. What we should do next

- **Look at where the question is being asked, not at what is being asked.** Every
  candidate so far has been a test applied to a rolling window that spans both the
  station and the noise around it. `134712`'s callsign window scores 0.677 and its
  whole recording scores 0.177, and that difference is not about the statistic.
- **The obvious remaining idea is the one already ruled out, and it may deserve
  re-examining on new terms.** The keying meter separates these recordings
  cleanly, is in the tree, and is forbidden to the decoder for a good reason. What
  is not forbidden is asking whether the *principle* it uses — a window chosen for
  the measurement rather than inherited from the decoder's own state — is what the
  other candidates have been missing.
- **Adjudicate `cw-2026-08-18-004507`.** It went 25 to 26 characters last session
  and nobody knows whether either number is any good.
- Keep a second recording with a readable callsign when one is heard. `N4L` is
  still the only ground truth in nine real recordings.

## 4. What's blocking us

**`Refine` is still blocked and HM-OPEN-054 is still open.** Three candidates are
gone and the ruling that closed it this session has not survived measurement.

**One ask, new this session.**

> **The transition clock is rejected as a keying test, and the reason is where the
> question is asked rather than what is asked.**
>
> Fitted to `cw-2026-08-17-134712`'s callsign window the clock finds 48 ms and the
> edges agree at 0.677, the strongest figure of any real window measured, and
> HM-DEC-144 puts the true dit at 56.3. **The idea works on the audio it was
> designed for.** Fitted across the whole of that recording it scores 0.177 against
> 0.116 for a recording holding nothing, because twenty-six of its thirty seconds
> are band noise and the statistic measures them.
>
> **A gate has to run where the decoder runs**, on a rolling window that contains
> whatever is passing at the time. At the moment of emission a real character comes
> out at 0.389 and an invented one at 0.470, and with `Refine` applied the easy
> tier emits at 0.493 while an empty band invents up to 0.533. There is nowhere to
> draw the line.
>
> **Rejected: gating on the best window rather than the current one.** That is a
> decoder deciding it may speak because it heard something well several seconds
> ago, which is the shape of claim §0.0 exists to prevent. **Also rejected: the
> peak's prominence**, which was measured on the same recordings and separates
> worse, putting `134712`'s own callsign window at 4.22 against 4.08 for a
> recording with nothing in it.

### Asks still outstanding

- **What keeps the decoder silent on an empty band, so `Refine` can ship.** First
  made 2026-08-20. Three candidates measured and rejected: the survey's verdict,
  the ratio band, and now the transition clock. `Refine` is not in the tree.
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
