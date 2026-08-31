UNIT:       053 — find where the good reads broke, and lock them — 2026-08-30
PHASE GOAL: 85% correct CW on a capture where the pitch is right, precision before yield.
UNIT GOAL:  Bisect the regression the operator reports, lock the clean reads, and cost the fix without building it.
ADVANCED:   **yes — the regression is found and named, and the reason the score could not see it is measured.** No fix built; the order forbids it and the band is open.
NUMBER:     **`cw-2026-08-17-013347` went from 55 named characters and 3 blocks to 9 named and 49 blocks at `95a5e06`, and precision did not move from 1.000.**
DRIFT:      0 consecutive units without advance (was 1).

## 1. What Claude did

**Hamlet confirmed.** All four gate checks verified against the tree before the
order was read. Branch `main`, every task committed and pushed, every push
succeeded. **Nothing here is evidence about the radio.**

**Five of six tasks. Task 6 is dropped and it is the named drop candidate.** No
fix was built, as the order requires.

### Task 1 — the bisect, and the answer is not a broken capture

A harness using only `WavAudio`, `CwDecoder`, `Process`, `Flush` and
`CharacterSettled` — the parts unchanged across the whole range — so it compiles
at every commit. It prints text only; **scoring happens outside, so the alignment
is the same one at every commit** rather than whatever the tree held that day.

**Forty-one commits, unit 048's first to head. Two transitions.**

| commit | unit | subject | effect on the three clean captures |
|---|---|---|---|
| `71b4f04`…`ade5253` | 048–050 | posterior, temperature, the 26 read, the dwell | **none** — all three identical throughout |
| **`efcd524`** | 050 | the spectral peak | `012403` gains `J ■■`, `013347` moves two characters. **All three truths survive.** |
| `b27b4b8`…`2147294` | 050–051 | benches, map, diagnostics | **none** |
| **`95a5e06`** | 051 | **the squelch** | **`013347`: 55 named → 9, 3 blocks → 49.** Truth survives. |
| `b7147b1`…`2c1a42c` | 051–053 | thresholds, anchors, reports | **none** |

**By the order's literal test, no capture ever stopped reading.** `KD0UN`,
`AA4MP/4` and `VA3VRR` all still contain their truth at head, and precision is
still 1.000 for all three.

**And the operator is still right.** On `013347` the screen went from **5% blocks
to 84% blocks** in one commit. **A screen of 84% blocks is what garbage looks
like.**

**The reason the score is blind to it is the shape of the measurement.** That
capture's adjudicated truth is `VA3VRR` — six characters. Six of six stayed
correct. **Everything else the decoder emits is outside the alignment entirely: it
is neither correct nor a substitution, so turning fifty characters into blocks
cost exactly nothing in the number.** That is how precision rose 0.858 → 0.888
across the same window in which the operator's experience got worse. **The two
facts were compatible, and the order was right to suspect it.**

### Task 2 — a floor an average cannot hide

Two halves, and **only the second would have caught what task 1 found**, which is
said in the test rather than left to be discovered.

- **The half the order asks for** — a capture reading at 1.000 keeps containing
  its truth. **This passes at `95a5e06` and at every commit either side.** Kept
  because a capture that stops containing its callsign at all is a different and
  worse failure nothing else watches.
- **The half with teeth** — named characters against blocks, per capture.

| capture | named | blocks | blocks |
|---|---|---|---|
| `KD0UN` `012403` | 20 | 4 | 17% |
| `AA4MP/4` `003758` | 42 | 19 | 31% |
| `VA3VRR` `013347` | **9** | **49** | **84%** |

**The floors are measured at head and deliberately not the pre-squelch numbers.**
Setting `013347` back to 55 would assert that the squelch must be reverted, which
is Tim's ruling and not a test's (§12.5, and this unit may not build a fix).

**The third test earned its place immediately**: the first floors I wrote were
counted off a console line rather than measured through the harness — 24, 55 and 9
against the true 20, 42 and 9 — **and it caught them.**

### Task 3 — the peak against a second signal, and this is the mechanism

**Synthetic, and it says so.** Two real captures summed, not two generated tones.
**Every capture in the tree has one dominant station, so the corpus cannot see
this at all** — which is exactly why the suspect survived unit 050.

| second signal, relative | **the peak** | **the old tracker** |
|---|---|---|
| −30 to −6 dB | holds 613 | holds 613 |
| **−3 dB** | **switches to 440** | holds 613 |
| 0 dB | 440 | holds 613 |
| **+6 dB** | 440 | **switches to 440** |

**The peak yields nine decibels sooner than the tracker did**, because the tracker
held a pitch once locked and the peak re-measures from scratch and takes the
loudest bin every time.

**And it walks between them inside one recording.** At −3 dB it sits on 440 for
seconds 8–21 and jumps to 613 for 22–30 — one switch across 23 readings. **That is
the shape that matters**: a switch between files is survivable, a pitch that moves
mid-recording decodes one part at one station and the rest at another, with
nonsense across the join.

### Task 4 — what would restore the good reads, costed and not built

| option | what it does | cost | risk |
|---|---|---|---|
| **A. Hysteresis on the peak** *(recommended)* | Hold the current pitch unless a competitor beats it by a stated margin for a stated time | One measurement to size the margin — the sweep above says **6 dB holds and 3 dB does not**, so the margin sits between. Corpus unchanged: every capture has one station, so hysteresis never fires there | **Low.** Restores what the tracker had without restoring the tracker's 100–200 Hz errors, which unit 050 measured |
| **B. Revert the squelch** (`95a5e06`) | `013347` returns to 55 named characters | **Precision 0.888 → 0.858**, and 61 invented characters return on an empty band | **High.** Reopens the §0.0 violation unit 051 closed |
| **C. Revert the spectral peak** (`efcd524`) | — | **Precision back to 0.766**, four captures abandoned 100–200 Hz from their station | **High**, and it is not the cause: `efcd524` moved two characters |
| **D. Nothing** | — | The bisect found a real 5%→84% change on one capture | Leaves it |

**Recommendation: A.** It is the only option that addresses what task 3 measured
without giving back what units 050 and 051 bought. **It is not built, and the
sizing of the margin and the hold time is a measurement the next unit takes.**

**One thing option A does not fix**, and it should be said: **the squelch is why
`013347` shows 84% blocks, and hysteresis will not change that.** Those two are
separate findings — task 3's mechanism explains a *live* evening with two signals
in the passband; task 1's finding explains a capture with one. **Tim may want both
addressed, and only A is costed here because only A is what task 3's measurement
supports.**

### Task 5 — the fading, measured for the next unit

| capture | ripple p50 | ripple p90 | dominant | named |
|---|---|---|---|---|
| `013347` | **99%** | 100% | 11.0 Hz | **9** |
| `134712` | 79% | 83% | 12.0 Hz | **0** |
| `031948` | 73% | 77% | 11.0 Hz | 31 |
| `003758` | 72% | 78% | 13.5 Hz | 42 |
| … | … | … | … | … |
| `004507` | 63% | 71% | 12.5 Hz | 48 |
| `012403` | **60%** | 67% | 15.5 Hz | 20 |

**The order predicted captures reading at 1.000 would sit at the bottom, and
`013347` does not** — it ripples hardest of all while reading at 1.000 precision
on a six-character truth. **Same divergence, reached by a third route.**

**Dominant modulation is 9.5–15.5 Hz**, not the 7/37/53 Hz quoted for the
2026-08-29 evening — those captures are not in the tree, so that is a different
set of recordings rather than a contradiction.

**The number the next unit needs, and it is not encouraging.** At 9.5–15.5 Hz **one
dropout lasts 32–53 ms**. A dit is **43 ms at 28 WPM and 60 ms at 20**. **The
dropout and the dit are the same order**, so a hold-over sized to bridge the fade
would bridge a real inter-element gap as well. **A naive hold-over cannot work**,
and that is the finding.

**The periodogram was wrong first time and is recorded rather than quietly
fixed**: it searched from 3 Hz and returned 4.5–6 Hz on every capture, which over a
120 ms run is less than one cycle — it was measuring the shape of the dah.

### Task 6 — **dropped whole**

The named drop candidate. Dropped because **task 1 showed the corpus score is the
wrong instrument for this question**, and re-measuring seven correlations against
that same score would produce seven numbers of the kind this unit just established
cannot see a regression. It wants doing against the named-character measure task 2
built, which is a different task from the one the order wrote.

**No decision was recorded under §12.1.**

## 2. What Tim should expect

**Plainly: yes, there was a regression, and it is `95a5e06`, the squelch.** On
`cw-2026-08-17-013347` it took the screen from 5% blocks to 84% in one commit.
**Nothing in the score moved, because that capture's truth is six characters and
all six survived.**

**Nothing is changed in this unit.** The decoder behaves exactly as it did this
morning — precision 0.888, yield 0.745. The order forbids building a fix while the
band is open, and I have not.

**What is new is a test that would have caught it**, and it is checked separately
from the average so it cannot be traded against one.

**What will look wrong but is not:**

- **`013347` is pinned at 9 named characters and 84% blocks.** That locks in
  today rather than blessing it; the floor may rise and may not fall.
- **`tools/Hamlet.Bisect` is untracked and stays that way.** It exists to survive
  `git checkout` of the whole tree, which is how the bisect ran.
- **Task 6 has no commit.** Dropped.

**Build clean, no new warnings.** Version unchanged at 1.12.7 — still unruled and
I have not guessed a fourth time.

| suite | result |
|---|---|
| `TheCleanReadsStayCleanTests` | **7 passing, 0 failing** — new this unit |
| `ThePeakAgainstASecondSignalTests` | 3 passing |
| `TheSilencePropertyIsLockedTests` | not re-run — **no decoder change shipped**, and it was green in unit 051 against this same code |
| corpus score | **0.888 / 0.745**, unchanged |

## 3. What we should do next

1. **Rule on task 4's option A** — hysteresis on the peak. It is costed above and
   the sizing measurement is one sweep.
2. **Rule on the squelch and `013347` separately.** Option A does not touch it, and
   84% blocks on a capture whose station is plainly there is its own question.
3. **Then the fading**, with task 5's numbers: the dropout and the dit are the same
   length, so the next unit starts from "a hold-over cannot work" rather than
   discovering it.

## 4. What's blocking us

Nothing is blocked. **One ruling is waiting and the band is open, which is why
this unit built nothing.**

> **Hysteresis on the spectral peak: hold the current pitch unless a competitor
> exceeds it by a stated margin for a stated time.**
>
> Measured this unit on two real captures summed: **the peak abandons the station
> it is reading once a second signal comes within 3 dB below it, and holds at
> 6 dB below.** The tone tracker it replaced held to 0 dB and yielded at +6 — **nine
> decibels more resistant** — because it held a pitch once locked. **And the peak
> walks between two stations inside a single recording**, which decodes one part at
> one station and the rest at another.
>
> **Every capture in the corpus has one dominant station, so none of this is
> visible in the score**, and a real evening on forty metres is not like that.
>
> **Rejected: reverting the peak.** It is not the cause — `efcd524` moved two
> characters — and reverting costs 0.766 precision and four captures abandoned
> 100–200 Hz from their station.
> **Rejected: restoring the tracker.** Its hold is the part worth having; its
> pitch errors are what unit 050 removed.
> **Rejected: building it this unit.** The order forbids it and the band is open.
> **What this session could not settle** is the margin and the hold time. The
> sweep brackets the margin between 3 and 6 dB and the hold time is unmeasured.

### Asks still outstanding

Carried forward per HM-DEC-139 and HM-DEC-140.

1. **Hysteresis on the peak** — raised above, 2026-08-30.
2. **The squelch and `013347`'s 84% blocks** — raised above, 2026-08-30. Separate
   from the above and not fixed by it.
3. **The `134712` carrier** — 2026-08-30, unit 052. Tim ruled the 500.09 unsourced
   and rejected spending another unit; carried only because the recording now reads
   **zero** named characters, which is new information since that ruling.
4. **The guard narrowing** — 2026-08-29, unit 051.
5. **The version bump** — 2026-08-29. `Directory.Build.props` still says 1.12.7.
6. **The filter byte against HM-DEC-149** — **HM-OPEN-062**, unruled.
7. **The evidence term's unbounded scale** (unit 049).
8. **The answer key's licensing.** **This unit makes it sharper**: a six-character
   truth on a fifty-eight-character emission is why the score could not see the
   regression.
9. **The mode and filter's place in the owned-settings contract** — unit 047.
10. **What the digital rows state for the five settings they are silent on.**
11. **The pedestal ranking is measured at 34 of 44 and unbuilt.**
12. **A dial move's threshold is provisional at 500 Hz.**
13. **The transcript break's wording.**
14. **Whether `CwPitch` should follow an admitted station.**
15. **`DECISIONS.md` has no record for HM-DEC-096–133, 136, 141 or 150.**
16. **The `reading` line's span wording needs approval.**
17. **Two stations closer than 125 Hz are not named.**
18. **HM-OPEN-057** (2026-08-22) and **HM-OPEN-007** (2026-08-14).
19. **Nothing checks that deleting a surface is not deleting a capability.**
20. **The test host crashes** in both suites — **HM-OPEN-063**.
21. **`PROJECT_CARD.md` has no phase field.**
