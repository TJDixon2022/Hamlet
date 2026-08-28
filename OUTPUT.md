UNIT:       043 — stopped at task 2 of 5 — 2026-08-28 18:59
PHASE GOAL: Readable CW on the operator's screen — eighty percent of a strong signal read correctly, first time.
UNIT GOAL:  Choose the mixdown pitch by which candidate decodes best, rather than by a statistic on an intermediate signal.
ADVANCED:   yes — pitch acquisition goes from 1 of 44 captures to 34 of 44 as a measurement; no decoder path was changed, so it is not on the operator's screen yet.
NUMBER:     one in eight -> 34 of 44 measured offline; on the operator's screen, unchanged.
DRIFT:      unknown — the work order's block carried no DRIFT line, so there is no count to carry or increment.

## 1. What Claude did

**Stopped at task 2 of 5, with task 2's question answered and answered
positively.** Tasks 3, 4 and 5 were not built. **That is not the drop the order
named** — task 5 was the drop candidate — so it is a sizing decision, and the
reasons are in section 4 as two rulings rather than buried here.

Development computer, prompt claimed `PROJECT: Hamlet`, branch `main`, version
`1.12.5` -> `1.12.6`. **Nothing in this report is evidence about the radio**: no
radio was connected, and every number comes from WAV files already in the tree.

### Task 1 — the trace

**What one window at one pitch costs, measured with `tools/Hamlet.PitchRank cost`
rather than estimated.** A 12-second window at 48 kHz is 2400 hops:

| | 12 s | 6 s | 4 s | 3 s | 2 s |
|---|---|---|---|---|---|
| quadrature mixdown | 10.4 ms | 4.2 ms | 2.6 ms | 2.0 ms | 1.6 ms |
| decode, speed grid 8–40 | 39.2 ms | 18.7 ms | 13.0 ms | 9.5 ms | 6.2 ms |
| **one candidate** | **49.6 ms** | 22.9 ms | 15.6 ms | 11.5 ms | 7.8 ms |
| sweep of 25 candidates | 1240 ms | 572 ms | 390 ms | 287 ms | 195 ms |
| share of one core at the shipped cadence | **248 %** | 114 % | 78 % | 57 % | 39 % |

**The sweep does not fit at the window and cadence that ship, and that is reported
rather than traded away.** Cost is linear in window length, so the trade is real
but it is a trade, and section 4 asks for it rather than taking it.

**The candidate count in the tree is 25, not the "about thirty-three" the order
states.** `CwToneTracker.MinimumToneHz` is 300 and `MaximumToneHz` 900
(`CwToneTracker.cs:125,128`), stepped by `CoarseSpacingHz` = 25 (`:138`).

**The fixed-pitch harness is already callable per candidate, so the unit is mostly
plumbing — as the order hoped.** Three public entry points, none of them new:

- `CwProbabilisticDecoder.Decode(MonoAudio, double toneHz)` — `CwProbabilisticDecoder.cs:639`
- `CwProbabilisticDecoder.Decode(IReadOnlyList<double> envelope, double toneHz)` — `:652`
- `CwDecoder.AssertAt(double toneHz)` — `CwDecoder.cs:515`, which holds the mixdown
  at a stated pitch through the whole shipped path.

`CwProbabilisticStream.ReadAgain(audio, toneHz)` (`CwProbabilisticStream.cs:475`)
already re-mixes held audio at a new pitch and `AudioTap` already keeps thirty
seconds, so even the replay a ranking pass would want is built.

**Where the mixdown pitch enters is one line**: `CwDecoder.Step`,
`CwDecoder.cs:753` — the operator's lock, else the last measured pitch, else the
tracker's. `CwProbabilisticStream.Process` (`:275`) mixes each sample at it;
`Read` (`:665`) passes it into the decode. **What else consumes it matters for
task 3**: `CwDecodeReport.ToneHz` comes from `_tracker.ToneHz`
(`CwDecoder.cs:279`), not from the mixdown, so a ranked pitch driving the mixdown
would not by itself reach the capture sheet, the duty line or the panel
(`MainWindowViewModel.cs:4735–4784`, `:3689`). The sidecar the order asks task 3 to
write would have to be fed from the ranking rather than from the tracker.

**Baseline, by diffing which tests fail: 28 failing, 1930 passing, 1958 total,
21 m 36 s.** Matches the order's stated 28 exactly, and the same 28 fail now.

### Task 2 — the ranking

**The decoder's score as it stands is not comparable between two pitches, and the
reason is that it is scale invariant.** `LikelihoodRatio` is a per-hop
log-likelihood of the best reading against "this is all noise", and both the noise
scale and the keyed level are estimated from the very envelope being scored
(`CwProbabilisticDecoder.LogLikelihoods`, `:973`). A bin where the receiver's
filter has already thrown everything away has almost no noise in it, so the wobble
that is left is scored against a tiny sigma and looks like the clearest keying in
the band. **The quietest bin wins.**

Measured through `CwDecoder.AssertAt`, the shipped path with the pitch held from
the first sample, on 15 captures before the run was stopped: the winner sat at
875–900 Hz on 10 of 15, it outscored the pitch that actually reads on **15 of 15**,
and it matched the station on 1. On `cw-2026-08-28-004844` the winner at 875 Hz
scored 312.62 and read `E E EE E EEEE E EEE EE E EEE E EE E E EE E E E E`, against
29.84 at the pitch that reads the net.

**So task 1's fourth question is answered no, and task 2's charge was then to find
what is comparable. It did.**

**Stand every candidate's envelope on one noise floor measured across the whole
band, and the same score ranks correctly.** Each envelope is combined in power with
a single pedestal — the loudest per-bin floor in the band — which is what each bin
would look like if the receiver's floor were flat. A bin holding nothing goes flat
against that pedestal and scores near nothing. A bin holding a keyed station keeps
its marks well above it and keeps its structure.

**Same window, same decoder, same 44 captures. The only change is the pedestal:**

| | matches the station |
|---|---|
| ranking by the score as it stands | **1 of 44** |
| ranking with a common noise floor | **34 of 44** |

**And the winners read, which is what makes 34 credible rather than circular.** The
pitch on the capture sheet is Hamlet's own tracker output, not independent truth,
so the text is the evidence:

| capture | what the winner reads |
|---|---|
| `cw-2026-08-17-013347` | `E W#VA3#R E` — the adjudicated `VA3VRR` |
| `cw-2026-08-17-134712` | `LQ E N4LQ # E` — the adjudicated `N4L` |
| `cw-2026-08-18-004507` | `H AN D L I NG T HIS M E S S A G E PE` — the adjudicated bulletin |
| `cw-2026-08-24-012403` | `EEQ DE NED0UN KD0UN K` — the adjudicated `KD0UN` |
| `cw-2026-08-28-004902` | `WED AU G 2 6 W 7 G B QRU M` — the net, with `W7GB` |
| `cw-2026-08-28-004844` | `# <BT> BRU C E <AR> NR 2 3 0 CE` — the net, with `BRUCE` |
| `cw-2026-08-22-032012` | `SES OR OTHER WEBSITES MENTI` |
| `cw-2026-08-25-013402` | `D NOT SURE - BUT ANY WAY VY NICE` |

**The four phantoms behave the way the order predicted they should.** They are the
captures where the winner misses the sheet's pitch and reads nothing: `005158`,
`005218` and `005243` all pick 600 Hz rather than the 750–775 Hz Hamlet used, and
what they read there is junk. Their winning scores are 5.28, 3.07 and 5.13 against
15.71, 12.32 and 10.15 for the three good ones on the same night. **A winner whose
own score is poor is exactly the shape task 4 was written for**, and there is now a
measured gap to put a floor in — though where it goes is a number nobody has swept
yet, and unit 1.11.33's finding that no fixed threshold separates the corpus still
stands until somebody re-measures it in these units.

**One in eight and 34 of 44 are not the same measurement, and the report should not
pretend they are.** One in eight is the operator's figure for the live application;
34 of 44 is an offline ranking over one window per capture. The like-for-like
comparison is the one in the table above: 1 of 44 against 34 of 44, same window,
same decoder, one change.

**What the order got wrong, and it matters because the unit rests on it.** The
order's evidence table gives the phantoms as 1.48, 1.49 and 3.34 against 36.3 and
28.3, "a factor of six to twenty-four". The sidecars in the tree do not say that:

| capture | order says | the sheet in the tree says |
|---|---|---|
| `cw-2026-08-28-004844` | 36.3 | 36.3 |
| `cw-2026-08-28-004902` | 28.3 | 28.3 |
| `cw-2026-08-28-005051` | 1.48 | **7.6** |
| `cw-2026-08-28-005158` | 1.49 | **−68562.4** |
| `cw-2026-08-28-005243` | 3.34 | **158.4** |

A phantom scores 158.4 where a real net scores 36.3, so the separation the unit was
commissioned on is not in the sheets as quoted. **The pedestal supplies the
separation the order expected to find already there**, which is the better outcome,
but the table should not go into the next order unchecked.

No decision was recorded under §12.1. Nothing here was a session's to settle.

## 2. What the owner should expect

**Nothing about the application changed.** No engine file, no view, no view model.
The decoder reads what it read this morning and the terminal behaves as it did. The
twelve adjudicated anchors are green because nothing went near them.

New in the tree is one measurement tool, `tools/Hamlet.PitchRank`, added to
`Hamlet.sln` beside `Hamlet.ScopeCheck`. It reads WAV files and prints numbers;
nothing calls it and it keys nothing. `pitch-rank cost` gives the cost table,
`pitch-rank shipped` the ranking through the shipped path, `pitch-rank pedestal`
the comparison that produced 1 of 44 against 34 of 44.

**What will look wrong but is not:**

- **28 engine tests still fail.** That is the stable baseline the order named,
  unmoved. `CwAcquisitionWindowTests.AFastFistIsReadWithoutARunUp(25, 0.79)` is
  among them — task 5's test, and task 5 was not reached.
- **`Directory.Build.props` moved 1.12.5 to 1.12.6** for a unit that ships no
  product change. HM-DEC-150 counts work units rather than grading them.
- **The app suite has no result here.** It was still running when the session's
  background work was stopped, so the order's 509 is unverified rather than
  confirmed. No app code was touched.
- **The shipped-path ranking covers 15 captures, not 44.** That run was killed
  part way. Its 15 are unanimous and the pedestal run that replaced it covers all
  44, so nothing turns on the missing 29.
- **The pedestal measurement is one-shot, not streaming.** It reads one window per
  capture. The two paths are already known to disagree — `CwProbabilisticDecoder`'s
  own remarks record a case where they track 650 Hz and 500 Hz on the same file —
  so **34 of 44 is not yet a claim about what the terminal would do.**
- **`eng-final.txt` and `app-final.txt` sit modified in the working tree.** They
  came from the previous session's commit `cf81849` and were left alone (§12.6).

## 3. What you should see

**Nothing yet. On a frequency where nothing is happening the terminal still fills
with letters, and on a frequency where a station is sending Hamlet lands on it
exactly as often as it did**, because no decoder path was changed.

What the evening bought is the reason the last six units failed, and it is short
enough to carry: **the decoder's score never measures anything absolute.** It asks
how much better a two-state reading explains this envelope than noise does, and it
takes its idea of noise from the same envelope. Give it a bin the receiver's filter
has already emptied and it finds almost no noise there, so the small wobble left
over looks like the cleanest keying in the band. That is why every winner sat at
the very edge of the search and read a page of single dits — those are the quietest
bins, and a dit is the shortest thing the decoder can spell.

Stand every bin on the same noise floor before scoring it and the same number
starts pointing at stations instead: **1 capture in 44 becomes 34**, and what it
reads at those pitches is `VA3VRR`, `N4L`, `KD0UN`, `HANDLING THIS MESSAGE`,
`W7GB` and `BRUCE`. Six admission statistics asked whether a bin was a station.
This asks the same question the seventh did and finally asks it in units that mean
the same thing at 400 hertz as at 900.

## 4. What's blocking us

Two rulings, the first blocking the more work.

> **The ranking stands every candidate on one noise floor measured across the band,
> and the mixdown pitch is the winner of that ranking.**
>
> The score as it stands cannot compare two pitches, because the noise scale and
> the keyed level are both taken from the envelope being scored, so the emptiest
> bin in the band wins and reads a page of dits — measured at 1 of 44 captures, and
> 15 of 15 through the shipped path where the winner outscored the pitch that
> actually reads. Combining every candidate's envelope in power with a single
> band-wide floor takes the same ranking to 34 of 44, and the winners read six
> adjudicated or corroborated readings.
>
> **Rejected: a seventh admission statistic.** This is not one. It changes the
> units the existing score is measured in; it asks no new question about a bin.
> **Rejected: shipping it on this measurement alone.** It is one window per
> capture through the whole-file path, and this repository has already been bitten
> by the whole-file and streaming paths disagreeing about which note they are on.
> **What it needs before it drives anything is the same sweep through
> `CwProbabilisticStream`.**
> **Rejected: taking the pedestal as obviously right.** The loudest per-bin floor
> in the band is one choice of common floor and nobody has swept the alternatives.
> The 34 is evidence the approach works, not that this constant is the best one.

> **What the pitch sweep is allowed to cost, given that it does not fit as it
> stands.**
>
> A sweep of 25 candidates over the 12-second window is 1240 ms, which is 248 % of
> one core at the shipped half-second cadence. It fits only by shortening the
> ranking window, lengthening the cadence, or both: a 4-second window swept every
> two seconds is about 20 % of one core, which is arithmetic from the measured
> table rather than a measurement of its own.
>
> **This is handed back rather than taken** because the ruling in force says in as
> many words that thinning the band or lengthening the cadence to make the compute
> fit is a measurement to report and not a silent trade. **A shorter ranking window
> is not free**: HM-DEC-120's own reasoning is that a short window does not merely
> read less, it reads confidently and wrongly, and `CwProbabilisticStream`'s refill
> guard exists for that. Ranking is not emission, so it may be a different case —
> but that is the argument, and it is the operator's to accept.
> **Rejected: ranking only at acquisition and then holding.** It would cost far
> less and it is probably right, but it changes what happens when a second station
> starts up, and that is a behaviour question rather than a compute one.

### Asks still outstanding

Carried forward per HM-DEC-139 and HM-DEC-140, from unit 1.11.34's list.

1. **The refusal costs reading a station you tune onto** — first raised 2026-08-27.
   Waiting on: task 5 of this unit, which was not reached. The test is
   `CwAcquisitionWindowTests.AFastFistIsReadWithoutARunUp(25, 0.79)`, red in the
   baseline and untouched this session.
2. **Admission admits a pitch 150 Hz off the station and holds it for forty-five
   seconds without a refresh** — the held peak decayed at exactly 1 dB per second
   across both gaps, so it was never refreshed at all. Waiting on: Tim's ruling.
3. **The `reading` line's new span wording needs approval.** Waiting on: Tim's
   ruling.
4. **`DECISIONS.md` has no record for HM-DEC-096–133, 136, 141 or 150.** Waiting
   on: Tim's ruling on whether these are written retrospectively.
5. **Two stations closer than 125 Hz are not named** — the operator's item five.
   Waiting on: Tim's ruling.
6. **HM-OPEN-057** (2026-08-22) and **HM-OPEN-007** (2026-08-14). Both open, both
   owned by Tim.
7. **Nothing checks that deleting a surface is not deleting a capability** — the
   operator has since found the favourites list gone. Parked, and named as the next
   unit unless he says otherwise.
8. **A capture sheet carries a score of −68562.4** (`cw-2026-08-28-005158`). Raised
   here for the first time. Waiting on: Tim's ruling on whether that is a display
   defect worth a unit. It is a number the operator can read and nothing beside it
   says what it means.
