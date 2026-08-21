# OUTPUT.md

## 1. What Claude did

### Task 1: one of the three behaviours is explained, and two cannot be

Claude Code on the development machine, in `C:\Source\HamLet`, on `main`. The
prompt named `PROJECT: Hamlet`, all four gate checks hold, and **no radio is
attached** (HM-DEC-093).

**The six captures are not in the tree, for the second work order running.**
`tests\fixtures\cw\captured\unadjudicated\` holds five files, the newest from the
20th being `cw-2026-08-20-014935`. Nothing named `005902`, `010133`, `010244`,
`010336`, `015834` or `020033` exists anywhere in the repository or on this
machine. `da656bb updates`, the commit made an hour before this order, added three
`ANNUNCIATOR.md.bak` files, three `CLAUDE_CODE.md.bak` files, `get-push.bat` and a
generated batch script, and no audio.

**So behaviours one and two cannot be answered.** Nothing here can say what
`010336` had that the four failures did not.

### Behaviour three is answered, on a recording that is here

**A ratio of 3.94 means the low cluster is not dits.** Measured through
`KeyingEnvelope` — the same instrument, the same envelope, the same fixed twenty
millisecond floor the order's table used — on `cw-2026-08-17-013347`, whose fist
this project has read by hand at dit 100.4 ms, dah 274.3, ratio 2.73
(HM-DEC-145):

| floor | dit | dah | ratio | runs | clusters apart |
|---|---|---|---|---|---|
| fixed 20 ms | **65.7** | 268.1 | **4.08** | 69 | 6.5 |
| half the fitted unit | **87.7** | 268.1 | **3.06** | 48 | 13.8 |
| read by hand (HM-DEC-145) | 100.4 | 274.3 | 2.73 | — | — |

**The dah is right to within three per cent and the dit is thirty-five per cent
short.** Lifting the floor from a fixed twenty milliseconds to half the fitted
unit, and letting it settle, excludes twenty-one more runs, leaves the dah
untouched, and takes the ratio from 4.08 to 3.06 while the two clusters move more
than twice as far apart in their own scatter.

**The fixed twenty is the cause and not the cure.** At fourteen words a minute
half a dit is fifty milliseconds, so a floor at twenty admits everything between
twenty and fifty into the cluster that is supposed to be dits. `020033`'s fitted
dit of 40.3 against an evening otherwise measuring 83 to 91 is the same shape.

**And it does nothing where there is nothing to do.** On `004507`, `003016` and
`003126` it excludes one run each and moves the ratio by three hundredths. **On
the two recordings holding no keying at any pitch it excludes none at all**, so it
cannot tidy an empty band into a fist (HM-DEC-090).

### Where the trace contradicts the instruction

**The correlation the unit is built on does not hold in this repository.** Runs
under twenty milliseconds, against the ratio the fit returns, through the same
instrument:

| recording | runs under 20 ms | fitted ratio |
|---|---|---|
| `013347` | 262 | **4.08** |
| `013622` | 1046 | 4.76 |
| `014935` (no station) | **1160** | **2.87** |
| `014854` (no station) | 339 | 2.86 |
| `134712` | 149 | 3.59 |
| `003016` | 81 | 2.88 |
| `003758` | 71 | 3.24 |
| `004507` | 63 | 2.79 |
| `003126` | 62 | 2.92 |

**The recording with more than four times as many short runs as any station-bearing
one fits the cleanest ratio of the set.** The same holds one layer down, counting
the marks that actually reach the tracker: `014854` and `014935` carry 13 and 14
per cent under twenty milliseconds and never lock, while `003758` carries 21 per
cent and reads fourteen characters. **The count is not the mechanism.** What the
floor is set to, relative to the unit being fitted, is.

**And "a ratio far from three is not to be trusted" conflicts with HM-DEC-144.**
That ruling adjudicated `N4L` by hand at a dah of 4.24 dits, on the air, from a
real station. A fitted ratio is a thing to look at and it is not a verdict, so it
was not made one.

### Questions two, three and five

**Two: both, and the key-up half is the contaminating one.** The dit comes first
from a two-means fit over key-down lengths, which is exactly what task 2 asks for
and is already there. Then `Refine` averages that with the mean of every key-up
gap shorter than twice it. On `013347` the key-down fit alone gives **100.0 ms**
against a hand-read 100.4; `Refine` turns it into 87.0.

**Three: in two places out of three.** Gaps are clustered from the sender's own
sending and never from multiples (HM-DEC-115). The dah is fitted per sender but
only inside two to five dits, and **outside that band it falls back to a textbook
three**. And `Refine`'s averaging assumes the element gap equals the dit, a 1:1
assumption that is false for every Farnsworth fist this project has measured.

**Five: from the gate, they survive the de-glitch, and every one of them reaches
the tracker.** `CwDecoder.OnMarkEnded` hands the estimator every mark that was not
truncated, at any length. Nothing between the gate and the estimator has ever
asked how long a mark is; the only thing ever set aside is a mark that was too
**quiet** (HM-DEC-144), and that needs the heights to fall into two separated
groups first.

**`ShortestVote` is not the mechanism and is untouched** (HM-OPEN-053). Its note
claims a median over five measurements removes any run shorter than three, which
is true of an isolated run and false of alternating chatter: three of the five can
be down without any two being adjacent. On `cw-2026-08-17-013622`, single
five-millisecond marks reach the estimator six times and two-measurement marks
nine times, with the vote window at five throughout.

### What was built

**Task 2's safe half, and only that half.** The fit's own quality is now measured
and reported and **nothing reads it to decide anything** (§0.0.1):
`CwSpeedEstimator.FittedDahDits`, the sender's dah in the sender's own fitted
dits, and `MarksBelowHalfADit`, counted and deliberately not excluded. The sidecar
gains a `clockFit` line and the roster a `fit` column carrying both, plus how far
the two clusters stand apart and how many marks the amplitude rule set aside.

**Task 2's exclusion was not built.** It was measured three ways inside the
decoder last session and every version that helped also made the decoder invent
characters on audio holding nothing, which breaks HM-DEC-120. That ask is still
outstanding and this session adds evidence to it rather than settling it.

**Task 3's second half.** `decoderWpm` said `not tracking` for four different
things: no tone located, a tone with no resolved character, a clock being
re-acquired, and a settled pass with no clock. It now says which, and names the
rolling estimate beside it, so a panel reading 29 and a file reading nothing is
legible as the guard withdrawing a number rather than two instruments disagreeing.
**Both the sidecar and the roster now take the speed from the decoder at the
moment of the press** rather than from the polled snapshot the header happens to
be holding, so every figure on a sheet comes from one instant (HM-DEC-091).

**Task 4, not dropped.** The two tone figures are labelled where they appear:
`toneHz` says it is the pitch the decoder is following, refined from where it
started, and the `keying` line says it is an independent sweep of 400 to 1200 Hz
in 25 Hz steps over the last six seconds sharing nothing with the decoder. They
differ by up to 250 Hz on the same file in this repository, and where they do, the
decoder is reading one pitch while something better keyed sits at another.

**No decision was recorded under §12.1.**

## 2. What Tim should expect

### Does a station at 14 words a minute now read at a speed you did not set?

**No, and nothing this session changed what the decoder does with audio at all** —
the decode path is byte-identical, and the change that would have altered it was
measured last session in three shapes that each either invented characters on an
empty band or delivered nothing.

### What is different on the sheets

Every capture sidecar now carries a `clockFit` line, and every roster row a `fit`
column, saying what the fit behind the speed looked like: the dah in fitted dits,
how far the two mark clusters stood apart, how many marks were under half a dit
and how many were set aside as too quiet. **On the evening of the 20th that line
would have separated the four captures that produced no speed from the one that
produced a speed out of a fit whose dah measured nearly four dits.** A row with no
fit behind it says `not fitted` rather than leaving a blank cell.

And a sidecar that cannot give a speed now says which of four things stopped it.

### What will look wrong and is not

**The order's expected-red list is two short**, as it was last time. The tree has
been at five since before either session:

- `CwSettledSilenceTests.APassThatReadSomethingEmitsSomething`
- `CwFarnsworthTests.TheBulletinDecodesToItsAnswerKey`
- `CwTerminalTests.ClearingTheTranscriptLeavesTheDecoderAlone`
- `ARecordingWithKeyingInItIsReadTests.TheDecoderSaysSomethingAboutIt`
- `TheToneIsFoundInRealisticAudio(farnsworth-heavy)`

**2,146 tests, five failing, and they are those five.** Build clean, no warnings.
Ten tests added, all green.

**The roster gains a `fit` column** between `seed` and `chars`. A roster file
started before this build and appended to after it would have rows of two
different widths, which is the third column added in two days.

Pushed to `main`.

## 3. What we should do next

- **Get the six recordings into the tree.** Two consecutive work orders have been
  written from them and neither could check a figure. They are the only sample this
  project has of one fist across several recordings with a mixture of failures and
  successes.
- **Rule on the element floor**, which is now one question with a measurement
  behind it rather than a guess: whether a fit may exclude runs below a share of
  the unit it is fitting. The evidence is in section 4 and it cuts two ways
  depending on which instrument it is applied to.
- **The lock is lost near the end of almost every real recording** — 25.0, 25.7 and
  27.5 seconds on thirty-second captures — and that is where the callsign usually
  is. Noticed while tracing, twice now, and still not investigated.

## 4. What's blocking us

**The six recordings are missing**, which blocked parts 1, 4 and most of 6 of task
1 and left everything else to be measured against different audio.

**Two asks, both new this session.**

> **May a clock fit exclude runs below a share of the unit it is fitting, and if
> so, in which instrument?**
>
> The measurement is unambiguous and it is on the one recording whose fist is
> adjudicated. Through `KeyingEnvelope`, with its fixed twenty millisecond floor,
> `cw-2026-08-17-013347` fits at dit 65.7, dah 268.1, **ratio 4.08**. With the
> floor taken at half the fitted unit and allowed to settle it fits at dit 87.7,
> the same dah, **ratio 3.06**, against a hand-read 100.4 and 2.73 (HM-DEC-145).
> It excludes one run from each recording whose fit is already sound and **none at
> all from either recording holding no keying**, so on this instrument it cannot
> flatter an empty band.
>
> **The same idea inside the decoder is a different question and has already been
> measured badly.** Three shapes were built last session; the ones that raised
> character counts also made the sensitivity sweep invent six to eight per cent of
> what it emits at minus three to minus five decibels, which is HM-DEC-120's
> property. **So the ask is narrow**: whether `KeyingEnvelope.ShortestElementMs`,
> which is the keying meter's floor and not the decoder's, may become a share of
> the unit. It would change what the meter reports and what pitch it chooses, and
> the meter is the independent witness, so it is not a session's to change.

> **The six recordings from the evening of the 20th are not in the tree.**
>
> The order says they are committed under
> `tests\fixtures\cw\captured\unadjudicated\`. That folder holds five files and
> the newest from the 20th is `cw-2026-08-20-014935`. `da656bb`, the commit made
> an hour before the order, added backup copies of two governance files and two
> batch scripts and no audio. This is the second order in a row built on them.

### Asks still outstanding

- **Whether a clock fit may exclude runs below a share of its own unit, and in
  which instrument.** First made 2026-08-20, this session. Waiting on Tim.
  Nothing is in the tree; the measurement is in
  `ARatioFarFromThreeIsAContaminatedFitTests`.
- **The six recordings from the evening of the 20th.** First made 2026-08-20,
  this session. Waiting on the files. Supersedes the same ask about five of them
  made earlier the same evening.
- **Whether a mark too short to be an element may be set aside before the decoder's
  clock is fitted, at the cost of HM-DEC-120's zero-invention property.** First
  made 2026-08-20. Waiting on Tim. Nothing is in the tree.
- **Whether the unit may still be averaged with key-up gaps.** First made
  2026-08-20. Waiting on Tim. Removing `Refine` puts `013347` at 100.0 ms against
  a hand-read 100.4 and turns thirteen tests red.
- **The speed control needs an entry in `DECISIONS.md` and an id.** First made
  2026-08-20. Waiting on Tim. The code is on `main`.
- **The keying meter's provisional thresholds**, now including
  `CwKeyingThresholds.ConfidentSwingDb` at 20 dB. First made 2026-08-20. Waiting
  on one evening's roster scored against the `meter` column.
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

**One item leaves the queue.** Whether `SHACK_FACTS.md` still holds that CI-V
Transceive is off: the order records 1,284 of 28,113 frames as the radio
announcing something, which is the measurement that ask was waiting on, and the
order parks it as Tim's ruling.
