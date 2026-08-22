# OUTPUT.md

## 1. What Claude did

**Elements per character has not moved. Nothing shipped that changes what is on
the screen, and this is that sentence in the first line rather than a character
count instead.**

| recording | elements | characters | **per character** | speed chosen |
|---|---|---|---|---|
| `cw-2026-08-17-013347` | 134 | 94 | **1.43** | 8 |
| `cw-2026-08-17-013622` | 135 | 75 | **1.80** | 30 |
| `cw-2026-08-17-134712` | 39 | 25 | **1.56** | 16 |
| `cw-2026-08-18-004507` | 110 | 47 | 2.34 | 24 |
| `cw-2026-08-18-003016` | 163 | 49 | 3.33 | 28 |
| `cw-2026-08-18-003126` | 143 | 52 | 2.75 | 30 |
| `cw-2026-08-18-003758` | 117 | 46 | 2.54 | 12 |
| `cw-2026-08-20-014854`, `-014935` | 0 | 0 | — silent | — |
| sensitivity fixture, 18 / 15 / 12 dB | 24 | 9 | 2.67 | **12**, on a fixture sending at 18 |

**Before and after are the same table**, because all three candidate changes were
measured and all three failed on evidence. The three are set out below with their
numbers.

**Three near three, three well under it.** The worst are the two oldest captures
and `134712`, at 1.43, 1.56 and 1.80 — and those are the recordings the operator
describes as a page of E, T and I. The order's figure of 1.54 could not be
checked, because **`cw-2026-08-22-014113.wav` and `ANALYSIS-cw-2026-08-22-014113.md`
are still not in the tree.** Nothing by either name exists anywhere in the
repository, so every figure in the order taken from that analysis — the 62 ms
unit, the 19 words a minute, the gap clusters at 50, 120–180 and 410/495 ms, the
`20 elements, 13 characters` — **went unchecked.** The table above is measured
here instead.

Claude Code on the development computer, `C:\Source\HamLet`, on `main`. Gate
verified against the tree: `Hamlet.sln` and `CwProbabilisticStream.cs` present, no
`CoreHMI.sln`, no `src\CoreHMI`, `PROJECT_CARD.md` says Hamlet. **No radio was
connected and nothing here is evidence about the radio** (HM-DEC-093). Nothing was
recorded under §12.1.

### Task 2 — where a character gap is decided

**1. What decides it.** `CwProbabilisticDecoder.DecodeAt`, in
`src/Hamlet.RadioEngine/Cw/CwProbabilisticDecoder.cs`. Five kinds are tried over
every span: a dit at one unit, a dah at three, the gap inside a character at one,
the gap between characters at three, the gap between words at seven. Each span
scores the audio's own key-down or key-up log-likelihood over it, minus a Gaussian
penalty on how far the span sits from the kind's expected length:

    off = (span − want) / max(want × 0.35, 1)      score = evidence − 0.5 × off²

**The penalty's width is a share of the kind's own expected length.** So the gap
inside a character is allowed 0.35 units of scatter and the gap between characters
is allowed 1.05, three times as much, because it expects three times as long.

**That puts the crossover in the wrong place, and it is arithmetic rather than an
opinion.** A gap of two units — exactly between the two — costs 4.08 as an element
gap and 0.45 as a character gap. The two costs are equal at **one and a half
units**, not at two. **Every gap longer than one and a half dits is called a
character gap**, and the evidence term is identical for both readings, so nothing
argues back. That is the mechanism behind characters breaking into single
elements.

The same asymmetry sets the dit-or-dah crossover at one and a half units, where it
happens to help, because a real fist's dahs run long (HM-DEC-144 measured 2.73
dits to the dah, HM-DEC-145 4.24).

**2. What a hand-sent fist gives.** The analysis's gap clusters could not be
checked without its audio. What is in the tree says the same thing from the other
side: the two adjudicated captures were measured at **2.73 and 4.24 dits to the
dah** (HM-DEC-144, HM-DEC-145) rather than the model's three, and HM-DEC-115
measured a real bulletin's gaps at 40, 240 and 500 ms against a 57 ms dit — **0.7,
4.2 and 8.8 units against a model expecting 1, 3 and 7.** The model expects a
textbook fist and operators do not send one.

**3. Imposing the measured speed.** A seam was added so a recording can be read at
one imposed speed rather than searching the grid. On `cw-2026-08-18-004507`, whole
recording, offline:

| imposed | 8 | 10 | 13 | 16 | 19 | 22 | 25 | 28 | 32 |
|---|---|---|---|---|---|---|---|---|---|
| likelihood | 27.5 | 32.0 | 32.4 | 32.4 | 32.4 | 32.4 | 32.4 | 32.4 | 32.3 |
| per character | 3.13 | 2.44 | 2.44 | 2.44 | 2.46 | 2.38 | 2.38 | 2.33 | 2.37 |

**The objective is flat from eleven words a minute to thirty-two**, 32.3 to 32.4,
and elements per character sits between 2.33 and 2.50 across the whole of it.
**Imposing the right speed does not bring it to three.**

**So the answer to the question this unit turns on is: the gap model, not the
grid.** The grid is not innocent of everything — a flat objective means the speed
Hamlet reports is nearly arbitrary, which is its own defect — but it is not what
breaks the characters.

### Tasks 3 and 4 — three changes built, measured, and none shipped

**A step of one word a minute.** Built. **It reads worse and it breaks
HM-DEC-120.** With a flat objective, more hypotheses is more ways to be wrong: the
sensitivity fixture, which sends at eighteen, was won by **nine**, and the sweep
began inventing **0.22 of the message at eighteen decibels where it had invented
nothing**. Cost 22.7 per cent of real time against 13.5 for the current grid, so
CPU was never the obstacle. **The order's own stop condition applies and the step
stays at two.**

**Scatter as a share of the dit rather than of the segment.** Built. It moves both
crossovers to two units, where the durations actually cross. **It costs five of the
seven recordings their text**, because a real fist's dahs arrive at two to two and
a half units and then read as dits: `AT ARRL DOT NET` became `IE ISSHSSE`, and
`003016` fell from 49 characters to 18 at 8.67 elements per character.

**Scoring the ratio rather than the difference** — `off = ln(span/want) / 0.35` —
which puts both crossovers at the geometric mean, 1.73 units, and rests on timing
error being multiplicative, which is what a hand does. **This one reads better in
several places**: `2 MOVIES A DAY` where it read `2 IOVI ES`, `EACH` as one word,
`N4LQ K` kept intact on the capture HM-DEC-144 adjudicated as `N4L`, and `VRR VA`
appearing on the capture HM-DEC-145 adjudicated as `VA3VRR`. Elements per
character did not move in aggregate — 1.42, 1.88, 1.63, 2.34, 3.55, 2.71, 3.10 —
and **it breaks `TheProbabilisticDecoderTests.ItReadsWhatTheReferenceReads`**,
which is the only thing anchoring this decoder to `reference_decoder.py`, an
implementation somebody else can check.

**That last one is the ask.** It is the change most likely to be right and it
cannot be made without deciding to diverge from the reference, which is a decision
about what the display asserts (§12.1).

### Task 5 — the corpus

**Unchanged, every recording character for character**, since nothing shipped:
`004507` `E AT ARRL DOT NET <BT> E ACH STATION HANDLING ET HIS M E S S A G E P E`;
`003016` `E ■I KPA1■IS<HH> ■NK <BT> STILLHVEMY ETO 91B E TT JETST VFB TUBE LIN`;
`003126` `E S 5 IWATTCH ATL E<AS>T 2 IOVI ES A DAY WID X■ WHY N■TT E E , WESTERNS
, E`; `003758` `E ■HES EHEHSE AA■IH/5■IS E E E EAN EANQNI<HH>SK  E E E E E E EIIE`;
`013347`, `013622` and `134712` as in the table above; `014854` and `014935`
silent, offline and streamed.

The sweep is unchanged at every level: 1.00 right and 0.00 invented from eighteen
decibels down to twelve, 0.06 wrong at eleven, 0.19 at three, 0.33 at zero,
silence below minus five. **28 failing, the same 28 by name.**

### Task 6 — the version

**`Directory.Build.props` moved 1.10.6 to 1.10.7.**

### What is in the tree from this unit

One diagnostic seam: `CwProbabilisticDecoder.Decode` takes an optional imposed
speed, which nothing in the application passes, so that a measurement can separate
a speed the grid cannot reach from a gap model that breaks characters wherever the
speed lands. And the two rejected models are written into the comments beside the
constant they would have changed, with what each cost.

### The rulings, checked

Every ruling this order cites says what the order says it says. **HM-DEC-048 and
HM-DEC-108 are the ones that bear on the answer**: a doubtful call lowers
confidence and nothing raises it, and a gap of two units is precisely the doubtful
call this model resolves silently toward the longer reading.

### The inbound asks queue

Every id it names is `status: open` in `OPEN_ISSUES.md`. Nothing on it is closed
and nothing open and relevant is missing.

## 2. What Tim should expect

**Every recording reads exactly what it read this morning, so he will not see more
CW tonight.**

Build clean, no warnings, version 1.10.7, **28 failing, the same 28 by name.**

**What will look wrong and is not:** nothing changed in the app at all. The three
things that would have changed it are measured above and each one fails on
evidence the operator would notice first — one silences most of the corpus, one
starts inventing at eighteen decibels, and one breaks the decoder's agreement with
the reference implementation it was ported from.

## 3. What we should do next

- **Rule on the ratio model**, in section 4. It is the one candidate that reads
  better on real captures, including two whose callsigns are adjudicated, and the
  question is whether Hamlet may stop matching `reference_decoder.py`.
- **The reference itself may be worth re-reading on this point.** If the reference
  scores the difference rather than the ratio, then the crossover at one and a half
  units is inherited rather than chosen, and that is worth knowing before anybody
  diverges from it.
- **The likelihood is flat in speed from eleven words a minute upward**, which
  means the speed on screen is nearly arbitrary on real audio. That is its own
  defect and it is not the one this unit was aimed at.
- **`003758` and `003016` are still short of their pre-removal strings.**

## 4. What's blocking us

Nothing blocks the next unit. One ask, and one thing that is needed before this
order can be executed as written.

> **The decoder scores how far a segment strays from its expected length as a
> ratio rather than as a difference, and stops matching `reference_decoder.py`.**
>
> The penalty is currently `(span − want) / (want × 0.35)`, so a character gap gets
> three times the scatter of an element gap and a word gap seven times. **The two
> costs cross at one and a half units instead of two**, which calls every gap
> longer than one and a half dits a character gap and breaks letters into single
> elements. That is arithmetic, not a hypothesis: at two units the element gap
> costs 4.08 and the character gap 0.45.
>
> **Scoring `ln(span / want) / 0.35` puts both crossovers at 1.73 units** and rests
> on a property of hands rather than of textbooks: timing error is multiplicative,
> so a sender who runs a fifth long runs a fifth long on dits, dahs and gaps alike.
> **Measured, it reads better on real captures** — `2 MOVIES A DAY`, `EACH`, `N4LQ
> K` on the capture adjudicated as `N4L`, `VRR VA` on the one adjudicated as
> `VA3VRR` — **and it leaves elements per character where it was**, which is the
> honest half of the report.
>
> **The cost is the anchor.** `ItReadsWhatTheReferenceReads` proves this decoder
> reads what the Python reference reads, character for character, and that is the
> only external check on a port of somebody else's algorithm. Changing the penalty
> ends it.
>
> **Rejected: the dit-scaled scatter**, which moves the crossovers to two units and
> costs five of seven recordings their text, because a real fist's dahs then read
> as dits. **Rejected: a finer speed grid**, which invents 0.22 of the message at
> eighteen decibels where nothing was invented, because the objective is flat in
> speed and more candidates is more ways to be wrong.

> **The evidence this order is built on is not in the repository.**
>
> `cw-2026-08-22-014113.wav` and `ANALYSIS-cw-2026-08-22-014113.md` are both
> absent, for the second unit running. Every figure quoted from that analysis went
> unchecked: the 62 ms unit, the nineteen words a minute, the gap clusters, and the
> `20 elements, 13 characters` the unit was named after. **The table in section 1
> is measured from what is here instead**, and it agrees with the order's direction
> — three of seven recordings sit between 1.43 and 1.80 elements per character —
> without confirming any of its numbers.

### Asks still outstanding

Carried per HM-DEC-139, verbatim until ruled.

- Whether a sender change can be decided by pitch distance at all — measured dead.
- Whether the window clear comes back on.
- The advice line asserting a cause the app can disprove.
- The sidecar asserting two incompatible things about one span.
- Whether the sidecar's `text` should include the leading edge.
- `cw-2026-08-22-014113.wav` and its analysis are not in the tree.
- The captures from the evenings of the 20th and 21st are not in the tree.
- Thirty seconds since the last character, for mode-follow's guard.
- Whether `RfGain`'s hundred per cent is a defect or the right answer.
- The likelihood gate at 15.0.
- The keying meter's provisional thresholds.
- `FollowSpeed` has no supplier.
- The mark-and-gap witness behind HM-DEC-144 and HM-DEC-145.
- HM-OPEN-052, HM-OPEN-053, HM-OPEN-054, HM-DEC-130, HM-DEC-098, HM-OPEN-033,
  HM-OPEN-007.
- **Whether the length penalty becomes a ratio**, first made today, above.
- **The likelihood is flat in speed above eleven words a minute**, first made
  today, so the speed Hamlet reports on real audio is nearly arbitrary.
