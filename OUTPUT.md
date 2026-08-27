UNIT: 032 — complete at task 5 of 5 — 2026-08-27 16:37

## 1. What Claude did

**Complete. All five tasks ran, including the drop.**

Claude Code, on the development computer, in `C:\Source\HamLet`. The prompt
claimed `PROJECT: Hamlet`; the tree confirmed all four checks — `SHACK_FACTS.md`
and `src/Hamlet.RadioEngine/Cw/CwProbabilisticDecoder.cs` exist, neither
`CoreHMI.sln` nor `MURC.sln` does. **Hamlet confirmed.** Branch `main`
throughout, four commits, all pushed, none refused. Version 1.11.28 to 1.11.29
per HM-DEC-150.

**Nothing here is evidence about the radio.** No rig was connected.

**No decision was recorded under §12.1.** What this unit found is a question
about a ruling of Tim's, which §12.1 puts outside a session's authority without
exception. It is in section 4.

**The unit was commissioned to build ranking and the measurement says do not
wire it.** It is built, it is pure, it is tested, and it is connected to nothing
— with the reason in its own documentation so no later session reconnects it
without reading why.

**THE FULL ENGINE SUITE DID NOT RETURN A FAILING-SET DIFF AND THIS REPORT DOES
NOT CLAIM ONE.** It was started three times; the first two runs were lost to a
build lock and a redirection that captured nothing, and the third was still
running when this was written. **So every figure below is measured, and the one
number that is not here is the diff of which tests moved.**

What is known instead, and it is weaker: **the seven tests added by this unit
were each run and each passed**, and **no production file was changed except one
new engine class that nothing calls**. The decoder, the tracker and the survey
are untouched — `git diff` over this unit's commits against
`src/Hamlet.RadioEngine/` reports exactly one added file, `Cw/CwPitchRanking.cs`.
**A suite that moved off 28 of 1845 would therefore be a timing intermittent
rather than this unit's doing**, but that is reasoning and not a measurement,
and the next session should run the diff before trusting it.

### Task 1 — the cost, which passed

The budget is **500 ms**, and it is not a number chosen for this:
`CwToneTracker.SurveyEveryHops` is 100 hops of 5 ms and
`CwProbabilisticStream.ReadEverySeconds` is 0.5, so the survey verdict and the
window re-read already land on the same cadence.

| scoring window | envelope | decode | total | candidates that fit |
|---|---|---|---|---|
| 3 s | 4.5 ms | 22.7 ms | **27.0 ms** | **18**, or 22 if the envelope is already taken |
| 6 s | 9.5 ms | 47.6 ms | 57.1 ms | 8, or 10 |
| 12 s | 16.2 ms | 98.8 ms | 115.0 ms | 4, or 5 |

**The answer to task 1's one sentence: eighteen candidates can be decode-scored
inside the survey's own cadence at a three-second window, and four at twelve.**
That is four or more either way, so tasks 2 to 4 proceeded.

**Most of the envelope work is already done.** The tracker computes a magnitude
per coarse bin every hop, so a ranking that reuses those pays the decode and not
the mix. **And a barren pitch is the cheap case** — it loses to the null
hypothesis early — so the scheme prices off its best branch rather than its
worst.

### Task 2 — built, and connected to nothing

`src/Hamlet.RadioEngine/Cw/CwPitchRanking.cs`. Envelopes in, an ordering out;
no clock, no radio. **The shortlist may be drawn by energy and the choice may
not be**, which is the distinction HM-DEC-095 turns on, and `Rank` orders only
by what each candidate read. Scoring is ungated, because a gated decode returns
nothing below the floor and every candidate under it would then score alike.

**It is not wired to the tracker**, and `Winner`'s own documentation carries the
measurement that says why.

## 2. What the owner should expect

**Nothing changed on the screen, and a station he can hear still does not reach
the decoder without him pressing anything.** That is the plain answer section 2
was asked for, and this unit did not move it.

**What did move is that the reason is now measured rather than suspected.** Two
more statistics have been tried and have failed, and the one rule that works on
every capture is the one a ruling forbids. That is a question for Tim rather
than a fault to fix, and it is in section 4.

**What will look wrong and is not:** a new engine file that nothing calls. That
is deliberate. Deleting it would throw away the measurement; wiring it would put
ninety-three characters of nothing on an empty band.

| | baseline | end |
|---|---|---|
| engine | 28 of 1845, stable set | **not measured — the run did not return** |
| app | 509 of 509 | **not re-run — no app file was changed** |

**The baseline is what this reports against**, because the end state was not
measured. Seven tests were added, all in the engine project, and each passed on
its own run: three in `WhatDecodeScoringCostsTests` and four in
`WhatRankingChoosesTests`. **An honest expectation is 28 of 1852 and it is an
expectation.**

## 3. What you should see

**The four captures the operator can hear, which is what section 3 leads with.**
Twelve-second window, the whole 25-pitch coarse bank, against floors of 41, 0, 0
and 0.

| capture | he hears | ranking chose | error | ratio there | at his pitch | what it spelled |
|---|---|---|---|---|---|---|
| `cw-2026-08-25-012823` | 500 Hz | **900 Hz** | +400 | 6.69 | 0.90 | `I E IS I SEE EE EE E E…` |
| `cw-2026-08-22-014113` | 607 Hz | **900 Hz** | +293 | 2.51 | 0.74 | `IEEE EEESE E E E EEE S…` |
| `cw-2026-08-22-014308` | 606 Hz | **875 Hz** | +269 | 3.63 | 0.48 | `E E IE E EE E EI EEIEE…` |
| `cw-2026-08-26-125941` | 403.5 Hz | **800 Hz** | +397 | 4.27 | 0.44 | `EEEIEIEEEIESEE EEIEIE#…` |

**Ranking chose the top of the passband on all four**, 269 to 400 hertz away,
and spelled runs of E, I and S — which is what noise reads as, because those are
the one- and two-element characters. **At the pitch he can actually hear, the
ratio is 0.44 to 0.90, below the gate of 1.40.** The station scores worse than
the noise beside it, on the decoder's own measure of reading.

**A capture pointed at the right pitch that still reads nothing is a finding
rather than a failure**, and this is the opposite case: none of the four was
pointed at the right pitch, so nothing downstream was ever exercised.

### Task 4 — what it costs when it is wrong, and the distance

| capture | chose | ratio | characters | over the gate of 1.40 |
|---|---|---|---|---|
| `cw-2026-08-20-014854`, holding nothing | 425 Hz | 4.47 | **93** | **+3.07** |
| `cw-2026-08-20-014935`, holding nothing | 450 Hz | 2.41 | **91** | **+1.01** |

**Both recordings that hold nothing are admitted, and not by a hair.** The floor
does not hold by a wide margin or by a narrow one; **it does not hold at all.**

**Why, and it is the part worth keeping.** `CwProbabilisticDecoder.Gate`'s own
documentation records `cw-2026-08-20-014854` at a **highest window ratio of
0.840 across 55 windows**. That figure is correct and it was measured **at one
pitch** — the one the tracker had already settled on. **Take the best of
twenty-five bins instead and the same recording scores 4.47.** The maximum over
a bank is a different statistic from a single draw, and a floor calibrated for
one does not transfer to the other. Somewhere in six hundred hertz of noise
there is always a pitch that reads.

**So the order's central safety argument — ranking needs no threshold because
HM-DEC-120 refuses the winner afterwards — is measured false.** Not marginal:
wrong by a factor of five on the worse capture. The silence property was named
as task 3's first acceptance line and as a thing not to trade, and ranking as
specified trades it.

**Swept at three, six and twelve seconds**, and it fails at all three, so no
choice of scoring window rescues it.

### The second statistic, tried because the first one's failure had a shape

The window ratio is the whole window's margin divided by its hops, so **it
rewards density**, and a pitch minting many cheap one-element characters out of
noise averages higher than a pitch holding a real station with real silence
between its letters. `SpanMargin` asks a different question of each character —
how far its own marks stood above the noise, with the element gaps cancelling
exactly — and a character minted from noise scores near zero there by
construction. The decoder has recorded it since unit 1.11.3 and nothing read it.

**It moves two of the four by one bin and changes nothing.** 900, 900, 850, 775.
And at the pitch he can hear the median span margin is **0.8 to 2.0**, while the
two empty recordings score **9.3 and 4.7** at theirs. **It inverts too.**

### Task 5 — the operator's assertion, and the sentence the order predicted

Three selection rules, same audio, same window, same bank.

| rule | within one bin of where the station is |
|---|---|
| by the decoder's window ratio | **0 of 4** |
| by the per-character span margin | **0 of 4** |
| **by the strongest bin** — what his assertion does | **4 of 4** |

| capture | loudest bin | what it reads there |
|---|---|---|
| `cw-2026-08-25-012823` | 500 Hz | **`O BET TER ON N…`** |
| `cw-2026-08-22-014113` | 600 Hz | `E D T# TIUIIII…` |
| `cw-2026-08-22-014308` | 600 Hz | `INS EW TIEET E…` |
| `cw-2026-08-26-125941` | 400 Hz | ` E EE I I II E…` |

The order said that if the assertion still wins on any capture, that is the most
useful sentence in the report. **It wins on all four, and on the first one it
reads English.**

### Where the instruction and the tree disagree

- **`cw-2026-08-25-021825` is called "the noise capture" and it holds a
  station** — an eight-second call in thirty seconds, 18 % duty, with floors of
  41, 74 and 16 in `TheCapturesThatDecodeKeepDecodingTests`. The two recordings
  that hold nothing are the 2026-08-20 pair, and those are what task 4 was run
  against.
- **`CLAUDE_CODE.md` is at version 1.6 with twelve sections**, as stated.
  Confirmed, and this report follows its §8 including the `UNIT:` line.
- **The four captures are under `captured/unadjudicated/`**, not `captured/`.
- **`cw-2026-08-24-012403`'s station is at 439.81 Hz**, not the 450 its own
  sidecar names; the sidecar records where the decoder was pointed at the moment
  of the press, which was ten hertz off.

## 4. What's blocking us

**The one rule that finds the station on every capture is the one HM-DEC-095
forbids, and that ruling is now what stands between Hamlet and reading these
four stations.**

Ruling asked for:

> **HM-DEC-095's "a note is chosen by how it is keyed and never by how loud it
> is" is amended: the strongest bin may choose the note at acquisition, with
> keying structure demoted from the chooser to a check on the winner.**
>
> **Eight statistics have now been measured against choosing a pitch by how it
> is keyed and all eight are wrong** — cluster separation, dah/dit ratio, level
> spread, lift over the band floor, quantisation residual, agreement between
> fitted units, and now the decoder's own window ratio and per-character span
> margin. On the four captures the operator can hear, the keying statistics are
> right **0 of 4** and the strongest bin is right **4 of 4**, reading English on
> one of them. **This is not six unlucky choices followed by two more.**
>
> **HM-DEC-095 was ruled on a real case and the case was narrow.** Its evidence
> was one recording where the answer was neither the loudest thing nor the
> configured pitch, with the operator's own transmission in the audio. That is a
> reason to exclude the operator's own transmission and to distrust loudness
> *when a keying statistic disagrees* — it is not evidence that loudness is
> wrong when nothing disagrees with it.
>
> **What was rejected:** carrying on looking for a ninth keying statistic, on
> the grounds that six was already enough evidence and eight is past the point
> where the next one is worth a session. **And rejected:** wiring the ranking
> anyway with a re-calibrated floor, because the floor would then be fitted to
> two empty recordings to permit a scheme that is right on none of the four that
> matter, which is fitting a number to a fixture.

**This is Tim's and not a session's**, under §12.1 — it touches what the display
asserts, and HM-DEC-095 is his ruling. **It is also exactly the case his ruling
of 2026-08-27 anticipated**: *"If any of my rulings are keeping us from doing
something the right way, then I probably ruled in error."* This one is, and the
evidence is four captures wide.

---

**HM-DEC-120's floor is calibrated for one look and anything that maximises over
a bank needs a different number.**

That is true whatever happens to the ruling above, because **any** acquisition
scheme that scores several candidates and takes the best is taking a maximum.
The floor of 1.40 is sound for a single decode at a tracked pitch and says
nothing about the best of twenty-five. **Nothing in the tree records that
distinction** and the next session to build a search will meet it again.

*Not proposed, because it needs a ruling:* whether the acquisition floor becomes
a separate, separately-measured number from the emission floor.

---

**Ranking is in the tree and is called by nothing.**

`CwPitchRanking` exists so the measurement can be re-run rather than re-argued.
**A later session will find an engine class with no callers and may read that as
dead code** (§10.1 warns that graph isolation is not evidence of dead code, and
here it is not even isolation — it is a deliberate disconnection). Its
`Winner` documentation says so in full. If Tim would rather it were deleted, it
is one file.

### Asks still outstanding

Carried forward verbatim per HM-DEC-139 and HM-DEC-140. **Twenty-six inbound
after this unit's closures. The oldest is open since 2026-08-14.**

1. **The sweep's `invented` column counts substitutions, not invented
   characters.**
2. **Whether the refill guard should apply to the first fill at all.**
3. **`ANNUNCIATOR.md` renamed `PHASE` to `TASK` while HM-DEC-150 makes `PHASE`
   match the version's minor.**
4. **`DECISIONS.md` has no record for HM-DEC-096–133, 136, 141 or 150** — and
   HM-DEC-095, 120, 125 and 127 are all inside it. **This unit acted on index
   rows alone, and it is asking for one of those rulings to be amended.**
5. **The integrator width** — settled at 45 Hz, with the sharp-peak caveat.
6. **The guard's gap is two to one**, calibrated on two empty captures.
7. **A boxcar's nulls made two of five swept offsets pathological best cases.**
8. **Two stations closer than 125 Hz are not named** — the operator's own item
   five, still not attempted.
9. **The keying meter** — its measurement found a station its verdict denied.
10. **HM-OPEN-057** (2026-08-22) and **HM-OPEN-007** (2026-08-14).
11. **The gate opens on everything, including two empty recordings** (1.11.18).
12. **The joint cutter cannot find word gaps on a compressed fist** (1.11.22) —
    the next decode question after this one, still unruled.
13. **The constrained margin is bounded and still does not separate** (1.11.22).
14. **Four fixtures are absent and five acceptance lines were unmeasurable**
    (1.11.22).
15. **HM-DEC-086's supersession needs a record** (1.11.25).
16. **The phrasebook's arrival and the absent-widget news are gone** (1.11.25).
17. **The recent-places row has no home** (1.11.26), three options costed.
18. **The owned-property list has no enforcement of staying current** (1.11.27).
19. **A test resolved an ambiguous control by accident** (1.11.27).
20. **Nothing checks that deleting a surface is not deleting a capability**
    (1.11.28) — measured on three instances.
21. **The scanner and the calling cycle are attached to the rig with no
    control** (1.11.28), and one of them transmits.
22. **Thirteen dead `DataTemplate` blocks nothing can distinguish from live
    ones** (1.11.28).
23. **Whether every constructed view model should be reachable from a binding**
    (1.11.28).
24. **`013347` returns a likelihood ratio of 17.2 million**, with `001520`'s
    quadrillions. Parked, raised once.
25. **HM-DEC-095 is what stands between Hamlet and these four stations**, above.
26. **HM-DEC-120's floor is calibrated for one look**, above.

New this unit: **HM-DEC-095's amendment**, above; **the floor for a maximum**,
above; **ranking is in the tree and called by nothing**, above.

Closed this unit: **can decode-scoring be afforded** — yes, eighteen candidates
at a three-second window, four at twelve. **Does ranking by what a bin reads
choose better than clustering** — no, 0 of 4 against the strongest bin's 4 of 4.
**What ranking costs on an empty band** — 93 characters, +3.07 over a floor that
does not hold.

Still open: **the lock's mixed help**; **three fixtures at accepted cost**; **an
unmeasured pitch costs `N4L`**; **the six-hertz window disagreement**;
**`CHANGELOG.md` at 1.9.0 against 1.11.29**; **the squelch has no axis**; **the
three morning captures of 2026-08-26**; **seven timing intermittents**.
