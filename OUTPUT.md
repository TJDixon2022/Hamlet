UNIT:       045 — stopped at task 8 of 8 — 2026-08-29 10:26
PHASE GOAL: Readable CW on the operator's screen — reading one mode at ninety-nine percent, measured against what was actually sent.
UNIT GOAL:  Build the score this project has never had, then make the first structural change the evidence supports.
ADVANCED:   yes — the phase goal is a percentage and this unit produced the first one: yield 0.763, precision 0.761 over 384 adjudicated characters.
NUMBER:     none -> yield 0.763, precision 0.761.
DRIFT:      0 consecutive units without advance  (was 4)

## 1. What Claude did

**Stopped at task 8 of 8.** Tasks 1, 3, 4, 6 and 7 landed. **Task 2 stopped as its
own instruction requires. Task 5 was not done and task 8 was not done.**

**Task 5 is blocked on audio and task 8 is the drop.** Task 5's acceptance is
stated entirely on `cw-2026-08-29-030850` and `-031024`, which are not in the
tree, so there is no way to show the change does what it is for or that it costs
nothing. **This is the drop the order named plus one blocked task**, not a sizing
decision taken here.

Development computer, prompt claimed `PROJECT: Hamlet`, branch `main`, version
`1.12.6` unchanged. **Nothing here is evidence about the radio**: no radio was
connected.

### Task 1 — where the tree is

**Both suites:** app **519 passing, 0 failing**. Engine **28 failing of 1984**,
excluding the five tests of `TheGateHasItsOwnWindowNowTests`, which crash the host
rather than failing (HM-OPEN-061). The failing set is byte-identical to unit 043's
baseline.

**And unit 044's twenty-ninth failure was confirmed as an intermittent.**
`ARigWhoseReadLoopIsStuckStillDisconnects` failed in that unit's whole-suite run,
passed on its own, and **does not appear in this run at all**. The total rose 1976
to 1984 with the eight scoring tests added here.

**What unit 044 landed: no product code at all.** Its `git diff` over `src/` is
empty. Its task 2 measured the no-clock refusal and **shipped nothing**, because
blocking output while `SpeedIsReacquiring` costs 356 of the 599 characters
carrying the twelve adjudicated readings — including every character of `N4L` and
of `DICTED 10.7` — against unit 036's bar of 2, 2 and 7 blocks. **So the refusal
this order asks about is not in force**, and every number below is measured
without it.

**The hard decisions in the chain, in order, with what each discards:**

| stage | where | commits | can anything revise it |
|---|---|---|---|
| pitch admitted | `CwToneTracker`, `ConfirmWithinSurveys = 2`, `:287` | one bin, after two agreeing surveys | no — the survey's other candidates are gone |
| pitch committed to the mixer | `CwDecoder.Step`, `:753` | one pitch per hop, lock → last measured → bank | no — the envelope is mixed at it and the audio is not kept per candidate |
| speed | `CwProbabilisticDecoder.Decode`, `:798` | **one WPM out of the grid, keeping only `bestScore`** | no — losing hypotheses are discarded inside the loop |
| element and character | the Viterbi inside `DecodeAt` | one path | no — one path is carried, not many |
| emission | `CwProbabilisticStream.Character`, `:730` | `CwConfidence.High` for every pattern the alphabet knows | no |

**The order's account of this is correct and the tree is worse than it says in one
place:** at the emit seam **no confidence judgement is made at all**. The only
thing that can mark a character unsure is the alphabet failing to recognise its
pattern. The single refusal that exists is `CwProbabilisticDecoder.Marked`,
`:1513`, which blocks a character whose `SpanMargin` is under 1.0.

**The integrator width is a constant.** `IntegratorBandwidthHz = 45.0`,
`:415`, fixed at construction (`CwDecoder.cs:145`) and **scaled by nothing**. The
order's claim holds.

**Averaging on the pitch before it is committed: there is confirmation, not
averaging.** A candidate must be admitted by two agreeing surveys before the
tracker will follow it. That is a stability requirement over two half-second
surveys; it is not an average of the peak estimate, and nothing smooths the
number that reaches the mixer.

### Task 2 — the answer key: stopped, for two reasons

**The order says to obtain the bulletin and stop if it cannot be got. It could
not, and there is a second reason it should not be vendored even if it could.**

The network is available and the ARRL archive was reached. `ARLP035` is listed for
2026-08-28, and `ARLP034` for 2026-08-21. **The fetch declined to reproduce the
bulletin verbatim on fair-use grounds**, returning only a summary — flux mean
117.1, planetary A index from 12 declining to 5. **A summary is not an answer
key**, and the order forbids reconstructing the text from memory or from Hamlet's
own output.

**And §2.1 forbids the vendoring the order asks for.** *No third-party proprietary
material* in this repository, and HM-DEC-049's precedent is explicit: a source
whose terms forbid redistribution is **cited and never committed**, which is why
the Icom manual is page-cited and absent. The order asks for the bulletin to be
vendored "the same way the band plan carries its citation", but the band plan's
practice for restricted sources is citation without the text. **This is raised in
section 4 rather than decided here.**

**A better answer key was already in the tree and costs nothing.** The twelve
readings Tim has adjudicated on his own recordings — `VA3VRR`, `N4L`,
`AA4MP/4 QNIK`, `DE KD0UN KD0UN K`, the ARRL bulletin fragment, and seven spans of
the 2026-08-22 propagation bulletin — are real air, ruled by him, already committed
with their decision ids, and carry no third-party rights question at all. **The
score below is measured against those.**

**One finding worth having: the 2026-08-22 captures are `ARLP034`.** One of them
reads `2026 PROPAGATION FORECAST BULLETIN ARLP034`, and the archive confirms
ARLP034 is dated 2026-08-21. So the corpus already held a bulletin capture with a
published counterpart; the missing 03:08 captures are not the first.

### Tasks 3 and 4 — the first score this project has ever had

`CwAccuracy` scores a decode against truth. Semi-global alignment, so the
adjudicated fragment is matched against the best-fitting stretch and the rest of
the transmission is not counted as error. **A block is a deletion, not a
substitution** — the trade unit 036 was ruled on. Two numbers, never reported
apart. Eight tests, each checkable by hand.

**The baseline, over 384 truth characters:**

| capture | truth | yield | precision | correct | subs | ins | dels |
|---|---|---|---|---|---|---|---|
| `cw-2026-08-17-013347` | 6 | **1.000** | 1.000 | 6 | 0 | 0 | 0 |
| `cw-2026-08-17-134712` | 3 | **1.000** | 0.750 | 3 | 0 | 1 | 0 |
| `cw-2026-08-18-003758` | 12 | **1.000** | 1.000 | 12 | 0 | 0 | 0 |
| `cw-2026-08-24-012403` | 16 | **1.000** | 1.000 | 16 | 0 | 0 | 0 |
| `cw-2026-08-22-031948` | 36 | 0.944 | 0.944 | 34 | 2 | 0 | 0 |
| `cw-2026-08-18-004507` | 57 | 0.930 | 0.828 | 53 | 3 | 8 | 1 |
| `cw-2026-08-22-032012` | 51 | 0.922 | 0.797 | 47 | 3 | 9 | 1 |
| `cw-2026-08-22-032113` | 28 | 0.821 | 0.719 | 23 | 4 | 5 | 1 |
| `cw-2026-08-22-031905` | 39 | 0.692 | 0.643 | 27 | 10 | 5 | 2 |
| `cw-2026-08-22-032050` | 59 | 0.678 | 0.800 | 40 | 7 | 3 | 12 |
| `cw-2026-08-22-032129` | 42 | 0.452 | 0.500 | 19 | 19 | 0 | 4 |
| `cw-2026-08-22-031838` | 35 | 0.371 | 0.500 | 13 | 13 | 0 | 9 |
| **corpus** | **384** | **0.763** | **0.761** | 293 | 61 | 31 | 30 |

**Reported plainly: seventy-six percent, both ways.** The phase goal is
ninety-nine. Four short captures read perfectly; the long bulletin spans are where
it falls apart, and the worst two are almost entirely substitutions — 19 and 13
wrong letters against 4 and 9 blocks. **Hamlet is not refusing on those. It is
guessing and missing.**

### Task 6 — the fit figure does not measure quality

**Correlation with accuracy across the twelve scored captures:**

| | |
|---|---|
| fit against **yield** | **−0.179** |
| fit against **precision** | **−0.203** |

**Negative on both.** The order states the consequence itself: *if the correlation
is negative or absent, the metric is not measuring quality and every decision that
consumes it is unsafe.* It is negative.

Sorted by fit, the point is plainer than the coefficient:

| fit | yield | capture |
|---|---|---|
| 1.22 | 0.371 | `031838` |
| 1.57 | **1.000** | `012403` |
| 4.00 | **1.000** | `013347` |
| 5.94 | **1.000** | `003758` |
| 12.60 | **1.000** | `134712` |
| 13.31 | 0.692 | `031905` |
| **25.85** | 0.678 | `032050` |

**The highest-scoring capture in the corpus reads 68 % and the second-lowest reads
100 %.**

**Confirming the order's analysis: yes, the figure compares the chosen reading
against silence**, and it is `(bestScore − nothingAtAll) / hops`
(`CwProbabilisticDecoder.cs:812`). **The term that grows is the noise scale, not
the element count.** Both `keyUp` and `keyDown` carry `−e²/2σ²`
(`LogLikelihoods`, `:973`); the null hypothesis pays it on every loud hop and the
best path avoids it wherever it calls a mark, so the difference — the whole figure
— **scales as the square of level over the estimated noise floor, unbounded.**
Unit 044 measured its range on real captures at −18.12 to 121.88, and it has
produced 5,521,967, 17.2 million and quadrillions on degenerate bins.

**Does the decoder compute a second-best? No.** `Decode`'s speed loop keeps only
`bestScore` (`:798`), the Viterbi inside it carries one path, and the string
`secondBest` does not occur in the file. **That is the finding the order predicted:
a pipeline that keeps one path has nothing to compare against except silence.**

Changed nothing, as instructed.

### Task 7 — the width, swept against truth for the first time

**The width has been swept before and never against an answer key** — the
reasoning behind 45 Hz argued from character counts and E-shares, which are
proxies for the thing now measurable.

| width | yield | precision | subs | ins | dels |
|---|---|---|---|---|---|
| 20 Hz | 0.708 | 0.747 | 66 | 26 | 46 |
| 25 Hz | 0.747 | 0.811 | 55 | 12 | 42 |
| 30 Hz | 0.745 | 0.784 | 61 | 18 | 37 |
| **35 Hz** | **0.771** | **0.811** | 51 | 18 | 37 |
| 40 Hz | 0.703 | 0.734 | 81 | 17 | 33 |
| **45 Hz (shipped)** | 0.763 | 0.761 | 61 | 31 | 30 |
| 55 Hz | 0.622 | 0.681 | 83 | 29 | 62 |
| 70 Hz | 0.518 | 0.626 | 104 | 15 | 81 |

**35 Hz leads on both numbers and nothing ships on that.** 40 Hz sits between the
two best rows and is the worst of the mid-range: **this is not a curve with a
maximum in it, it is noise on twelve captures.** Picking 35 off it would be tuning
a constant until the corpus reads better, which the order forbids in as many
words. The eight-tenths of a point of yield is three characters.

**What the sweep does establish is the wide end.** 55 and 70 Hz are clearly worse
than anything at or below 45, by margins far outside the noise — 0.622 and 0.518
against 0.763. So widening is ruled out even if narrowing is not settled.

**And the speed-scaled width was not built, deliberately.** Fitting a second
parameter on evidence that cannot choose the first would be the same error twice.
Section 4 says what would settle it.

No decision was recorded under §12.1.

## 2. What the owner should expect

**Hamlet now scores itself against what was actually sent, and the number is
seventy-six percent — yield 0.763, precision 0.761, over 384 characters you have
adjudicated.** The goal is ninety-nine. Nothing on the screen changed this unit;
what changed is that there is now an instrument that can tell whether the next
change helps.

What is now true of the tree:

- `CwAccuracy` scores a decode against truth, with eight hand-checkable tests. It
  is pure, deterministic, and takes no clock and no network.
- `tools/Hamlet.PitchRank` gained `score` and `width`, so the baseline table and
  the width sweep are each one command.
- **No decoder behaviour changed.** No constant moved.

**What will look wrong but is not:**

- **The score is against your twelve adjudicated readings, not the ARRL
  bulletin.** The bulletin could not be obtained verbatim and should not be
  vendored anyway — section 4.
- **The engine baseline is still 28 failing**, and no product code changed to move
  it. The twenty-ninth that unit 044 saw did not recur, which settles it as the
  intermittent that report called it.
- **35 Hz appears to beat the shipped 45 Hz and was not adopted.** The sweep is
  non-monotonic; that is the reason, and it is in task 7.
- **Task 5 did nothing.** Its acceptance is stated only on captures that are not
  here.
- **The full engine suite has no clean run.** The host crash is wider than
  HM-OPEN-061 first recorded, which was logged against that issue in unit 043.

## 3. What you should see

**Seventy-six percent. Yield 0.763, precision 0.761, over 384 characters of your
own adjudicated readings.** That is the first accuracy figure this project has
produced, and the phase goal is ninety-nine.

The shape of the failure is worth as much as the number. **Four captures read
perfectly and the long bulletin spans fall apart**, and the two worst are almost
entirely substitutions — 19 wrong letters against 4 blocks on `032129`, 13 against
9 on `031838`. **Hamlet is not refusing on those. It is guessing and missing**,
which is the failure §0.0 exists to prevent, now visible as a number for the first
time.

**And the second finding is the one that makes the rest untrustworthy: the fit
figure correlates negatively with accuracy — −0.179 against yield, −0.203 against
precision.** The highest-scoring capture in the corpus reads 68 % and the
second-lowest reads 100 %. Your order's own words apply: the metric is not
measuring quality, and every decision that consumes it is unsafe. It compares the
chosen reading against silence, and what actually drives it is the square of the
signal level over the estimated noise floor, with no bound.

**The decoder computes no second-best at all** — one speed survives the grid, one
path survives the Viterbi. So there is nothing in the tree to compare the winning
interpretation against except nothing at all, which is exactly the architectural
point your analysis makes.

## 4. What's blocking us

Three rulings, most-blocking first.

> **The answer key is the adjudicated corpus, and a published bulletin is cited
> rather than vendored.**
>
> The ARRL text could not be obtained verbatim, and §2.1 forbids third-party
> proprietary material in this repository while HM-DEC-049 has restricted sources
> cited and never committed — which is why the Icom manual is page-cited and
> absent. **The order asks for vendoring under `data/truth/` and that conflicts
> with both.** Meanwhile the twelve readings you have adjudicated are real air,
> already committed, and carry no rights question; the score above is measured on
> them.
>
> **Rejected: reconstructing the bulletin from memory or from Hamlet's output.**
> The order forbids it and it would make the decoder its own answer key.
> **Rejected: scoring only the fragments as pass or fail.** That is what the
> anchors already do and it produces no percentage.
> **What would change it:** a statement that ARRL bulletins may be redistributed,
> or a truth file holding *your* transcription of your own recordings, which has
> no rights question at all and would extend the corpus well past 384 characters.

> **Whether the fit figure is replaced, and with what.**
>
> It correlates **−0.179** with yield and **−0.203** with precision. It gates
> emission at 1.40, it is printed on every capture sheet as `better than silence
> per hop`, and the character floor at 1.0 is expressed in its units. **Every one
> of those rests on a quantity now measured not to track correctness.**
>
> **Rejected: changing it in this unit.** The order says measure only and it is
> right — a metric change moves every gate that consumes it and needs the score in
> hand, which now exists.
> **Rejected: the odds against the second-best, as stated.** Not because it is
> wrong but because **the decoder computes no second-best**, so that quantity does
> not exist yet and creating it means keeping more than one path — which is task
> 8's redesign, not a metric swap.
> **What is available cheaply:** the runner-up *speed* hypothesis. The grid is
> already evaluated at every WPM and all but the winner discarded at `:798`;
> keeping the second-highest costs one variable and would give a first
> best-against-rival figure without touching the architecture.

> **The integrator width, and whether the corpus can settle it.**
>
> Swept against truth: 35 Hz reads 0.771/0.811 and the shipped 45 Hz reads
> 0.763/0.761, but 40 Hz between them reads 0.703/0.734. **Twelve captures cannot
> resolve a difference of three characters.** The wide end is settled — 55 and 70
> Hz are far worse and widening is ruled out.
>
> **Rejected: adopting 35 Hz.** It is the best row in a non-monotonic sweep, which
> is the definition of fitting noise.
> **Rejected: scaling the width with the committed speed now.** A second parameter
> fitted on evidence that cannot choose the first.
> **What would settle it:** more truth. The same sweep over a corpus three times
> this size would separate 35 from 45 or show they are the same.

### Asks still outstanding

Carried forward per HM-DEC-139 and HM-DEC-140.

1. **The eight 2026-08-29 captures are not in the tree**, blocking a third
   consecutive unit — task 5 here, tasks 2/3/4/6 of unit 044, tasks 2/3/4/6 of
   unit 043.
2. **The fit figure does not track correctness** — measured here, raised above.
3. **The answer key's licensing** — raised above.
4. **The pedestal ranking is measured at 34 of 44 and unbuilt.**
5. **A dial move's threshold is provisional at 500 Hz**, shipped in unit 043 with
   three candidates costed.
6. **The transcript break's wording** — proposed in unit 043's report, unruled.
7. **The attenuator's condition on a live overflow reading**, and whether `CwPitch`
   should follow an admitted station.
8. **`DECISIONS.md` has no record for HM-DEC-096–133, 136, 141 or 150.**
9. **The `reading` line's span wording needs approval.**
10. **Two stations closer than 125 Hz are not named.**
11. **HM-OPEN-057** (2026-08-22) and **HM-OPEN-007** (2026-08-14).
12. **Nothing checks that deleting a surface is not deleting a capability.**
13. **The engine test host crashes**, and not only on the class HM-OPEN-061 names.
    Owned by Claude, not waiting on a ruling.
14. **A second intermittent**, `ARigWhoseReadLoopIsStuckStillDisconnects`, seen in
    unit 044 and **not reproduced here**, which confirms it is intermittent rather
    than a regression.
