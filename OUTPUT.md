UNIT:       052 — the window where somebody is keying — 2026-08-30
PHASE GOAL: 85% correct CW, precision before yield.
UNIT GOAL:  Measure admission and pitch over the stretch where somebody is actually keying, rather than over a window that is mostly silence.
ADVANCED:   **no, and the reason is that the premise did not survive measurement.** Both changes this unit was built to make were measured and neither buys anything. Nothing regressed: precision holds at 0.888.
NUMBER:     **precision 0.888, yield 0.745, substitutions 16** — unchanged, by construction. No decoder change shipped.
DRIFT:      1 consecutive unit without advance (was 0).

## 1. What Claude did

**Hamlet confirmed.** All four gate checks verified against the tree before the
order was read, `Hamlet.sln` and `CLAUDE.md`'s header corroborating. Branch
`main`, every task committed and pushed, every push succeeded. **Nothing here is
evidence about the radio.**

**Five of six tasks. Task 6 is dropped and it is the named drop candidate.** Two
of the five ended in "measured, and not adopted", which is the honest outcome and
not a failure to try.

**The order's central premise does not hold, and that is this unit's main
finding.**

### Task 1 — how many captures can test this at all

**Three of twelve, and effectively one.**

| capture | present | longest | duty whole | duty in window | **gap** |
|---|---|---|---|---|---|
| `134712` | 23.3% | 8.0 s | 23.3% | 75.0% | **+51.7** |
| `012403` | 53.3% | 15.0 s | 53.3% | 93.3% | **+40.0** |
| `003758` | 73.3% | 15.0 s | 73.3% | 93.3% | **+20.0** |
| `031905` | 90.0% | 15.0 s | 90.0% | 100.0% | +10.0 |
| `013347` | 93.3% | 27.0 s | 93.3% | 100.0% | +6.7 |
| the other seven | 96.7–100% | 30.0 s | — | — | **+0.0** |

**Seven captures have no gap at all** — the station is present throughout, so they
cannot demonstrate a window change in either direction. Of the three that can,
**`012403` and `003758` already read at 1.000 precision and have no headroom**, so
the only capture that could show an improvement is `134712`, which is the retired
`N4L`. **One capture. Any result from this corpus is worth exactly that much, and
that is stated rather than discovered afterwards.**

**The instrument took three attempts and all three are recorded**, because each
failure says something about the corpus. Six decibels above the envelope's median
marked six captures at 0.0% present — on a continuously-keyed bulletin the median
sits inside the signal. Six decibels above the quietest tenth of one-second blocks
marked eight at 0.0%, for a sharper reason: **a station present throughout leaves
no quiet reference inside its own recording**, so every relative rule calls it
absent. The test that works is absolute — a second of Morse swings 15–25 dB
between key-down and key-up, noise sits still — with the separator at 18 dB read
off the measured distribution (quiet blocks 11–15, keyed blocks 20–30) rather than
guessed. It shares no code with admission.

### Task 2 — the four W1AW anchors re-expressed

Retired, not deleted and not lowered, in the same form `N4L` was, with one shared
reason so the four cannot drift apart. **Each carries what its capture still
reads**, so the re-expression carries its own evidence: `031905` reads back to
`DICTED 10.7`, `032050` reads its last few seconds, `032129` still gives
`…ON FORECAST BUAELETIN ARLP034`. Suite green, 13 of 13.

### Task 3 — **the premise is false and the change was not built**

**The admission window is already three seconds and it already slides.**

`CwToneSurvey`'s constructor takes `seconds = 3.0`, and `CwToneTracker` builds
both the coarse and the fine survey without overriding it. **Measured: thirty
seconds of hops leaves 3.00 seconds of history**, and `presentFraction` is counted
over what is in the ring. **There is no whole-recording duty anywhere on the
admission path.**

The order's diagnosis — *"duty and swing computed over the whole recording"* —
describes the capture sheet, which reports what a file looked like at the moment
somebody pressed the button. **Those are the 39% duty and 19 dB swing the order
quotes, and they describe the recording rather than the decision.**

So task 3 would have replaced a three-second sliding window with a window chosen
inside three seconds. That is not the fault described, and it could not be
verified as the fix for it. **Building it would have been the unverifiable
admission change unit 051 correctly declined**, which the order itself names as
the standard.

### Tasks 4 and 5 — measured, not adopted, and one correction to make

`CwSpectralPeak.FindOverLoudestStretch` is built and **is not wired in.**

| capture | whole file | loudest 8 s | loudest 4 s | spread |
|---|---|---|---|---|
| `134712` | 501.16 | 501.04 | 500.99 | **0.16** |
| `013347` | 613.64 | 613.67 | 613.62 | 0.05 |
| `003758` | 498.82 | 498.91 | 498.75 | 0.15 |
| `012403` | 439.76 | 439.76 | 439.76 | 0.01 |
| the eight others | — | — | — | 0.03–0.09 |

**The window changes the answer by at most 0.16 Hz against a residual of 1.1.** So
task 5's answer is **no: `N4L` does not come back**, and the anchor stays as unit
051 re-expressed it.

**AND THIS WITHDRAWS SOMETHING I STATED CONFIDENTLY IN UNIT 051'S REPORT.** That
report concluded the peak's error *"is neither bias nor floor but duty"*. **It is
not duty.** The ±1.25 Hz outliers in that sweep were sparse messages only a few
seconds long, so the transform had almost nothing to average — **I read a
file-length artifact as a duty-cycle effect**, and the first version of this
unit's test reproduced the same mistake, returning identical numbers to three
decimals because a four-second stretch of a four-second file is the whole file.

The better test is the shape the real capture actually has: a seven-second burst
inside thirty seconds of noise. **Measured that way across five carriers and three
speeds, the peak is accurate to 0.023 Hz over the whole file, and the loudest
stretch matches it to a thousandth.** Duty does not explain the 1.1 Hz and neither
does file length.

**What that leaves is the possibility that the station on `134712` is not at
500.09 at all.** Every window agrees on about 501.1, and the eight 08-22 captures
read 500.02–500.10 on the same instrument. That is section 4's ask.

### Task 6 — **dropped whole, and this says so**

The named drop candidate. Re-measuring seven confidence quantities is measure-only
and would have been feasible; it is dropped because **its stated premise is that
the corpus has changed since they were measured, and the useful comparison is
against a corpus that has settled.** Precision moved 0.858 → 0.888 in unit 051 and
this unit changed nothing, so the numbers would be re-taken again the moment
anything lands. It is a better first task for a unit that ships a change than a
last task for one that ships none.

**No decision was recorded under §12.1.**

## 2. What Tim should expect

**Nothing on screen has changed. Precision is 0.888 and yield 0.745, exactly as
unit 051 left them.** No decoder change shipped, by design: both candidate changes
were measured and neither earned its place.

**What will look wrong but is not:**

- **`CwSpectralPeak.FindOverLoudestStretch` exists and nothing calls it.** Measured
  and not adopted, kept with its numbers — the same pattern as
  `CwUnitEstimator.Threshold` from unit 051.
- **The four W1AW anchors and `N4L` are all marked retired.** They print what they
  read and what would bring them back.
- **Task 6 has no commit.** Dropped.

**Build clean, no new warnings.** Version unchanged at 1.12.7 — the bump question
is still unruled and I have not guessed a third time.

| suite | result |
|---|---|
| `TheAdjudicatedReadingsKeepReadingTests` | **13 passing, 0 failing** |
| `TheSurveyAlreadyUsesAShortWindowTests` | 2 passing |
| `IsTheHertzABiasOrAFloorTests` | 3 passing |
| `TheSilencePropertyIsLockedTests` | not re-run — **no decoder change shipped this unit**, and it was green in unit 051 against this same code |

## 3. What we should do next

**Task 1's table is the number: three captures of twelve have any presence gap,
and only one of those has headroom to improve. This corpus can barely test a
window change at all.** Task 4's peak table follows: every window agrees to within
0.16 Hz on every capture.

1. **Rule on the `134712` carrier** (section 4). It decides whether `N4L` is a
   decoder problem or a bookkeeping one, and three units have now spent effort on
   it.
2. **Get captures with partial presence into the tree.** Not the 08-30 ones, which
   the order says are not coming — any recording where a station appears partway
   through. Eleven of twelve captures here are keyed throughout, which is why this
   unit could not test its own subject.
3. **Then the joint decoder**, which the order parks as the unit after this one and
   whose evidence is untouched by anything here.

## 4. What's blocking us

One ask, and it is small and cheap to settle.

> **The station on `cw-2026-08-17-134712` is at about 501.1 Hz, not 500.09, and
> the figure that retired `N4L` should be checked before another unit is spent on
> it.**
>
> `CwDecoder.cs` records that the old tracker's fallback bank centre of 500.0
> "landed within a tenth of a hertz of a station at 500.09". **Nothing in this
> repository says where that 500.09 came from.** It is not in HM-DEC-144, which
> records the element timings and the callsign and no carrier frequency.
>
> Measured this unit, three ways: the peak reads **501.16** over the whole file,
> **501.04** over the loudest eight seconds and **500.99** over the loudest four.
> **They agree with each other far better than any of them agrees with 500.09.**
> On the same instrument the eight 08-22 captures read 500.02 to 500.10.
>
> And the peak is not the suspect: on synthetic keying at a known 500.09, at
> three speeds, with and without noise, and as a seven-second burst inside thirty
> seconds of band noise, **it reads within 0.023 Hz every time.**
>
> **Rejected: changing the decoder to make `N4L` return.** The order forbids it and
> it would be fitting the instrument to one recording.
> **Rejected: another window.** Three were measured and they agree to 0.16 Hz.
> **What this session could not settle** is what the station's carrier actually is,
> because that needs an instrument independent of both the peak and the tracker,
> and the only adjudicated fact about the recording is its element timing.

### Asks still outstanding

Carried forward per HM-DEC-139 and HM-DEC-140.

1. **The `134712` carrier** — raised above, 2026-08-30.
2. **Captures with partial presence.** Eleven of twelve in the tree are keyed
   throughout. **The 08-29 and 08-30 captures are confirmed not coming** and are
   dropped from this queue accordingly.
3. **The guard narrowing** — 2026-08-29, unit 051. In the tree at
   `tests/Hamlet.RadioEngine.Tests/Rig/RigStateModelTests.cs`.
4. **The version bump** — 2026-08-29. `Directory.Build.props` still says 1.12.7.
5. **The filter byte against HM-DEC-149** — **HM-OPEN-062**, unruled.
6. **The evidence term's unbounded scale** (unit 049).
7. **The answer key's licensing.**
8. **The mode and filter's place in the owned-settings contract** — unit 047.
9. **What the digital rows state for the five settings they are silent on.**
10. **The pedestal ranking is measured at 34 of 44 and unbuilt.**
11. **A dial move's threshold is provisional at 500 Hz.**
12. **The transcript break's wording.**
13. **Whether `CwPitch` should follow an admitted station.**
14. **`DECISIONS.md` has no record for HM-DEC-096–133, 136, 141 or 150.**
15. **The `reading` line's span wording needs approval.**
16. **Two stations closer than 125 Hz are not named.**
17. **HM-OPEN-057** (2026-08-22) and **HM-OPEN-007** (2026-08-14).
18. **Nothing checks that deleting a surface is not deleting a capability.**
19. **The test host crashes** in both suites — **HM-OPEN-063**. Owned by Claude.
20. **`PROJECT_CARD.md` has no phase field.**
