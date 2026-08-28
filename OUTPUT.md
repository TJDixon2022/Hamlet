UNIT: 036 — stopped at task 1 of 5 — 2026-08-27 22:31

## 1. What Claude did

**Stopped at task 1, by the order's own guard. Tasks 3, 4 and 5 were done
anyway; task 2 is moot.**

Claude Code, on the development computer, in `C:\Source\HamLet`. The prompt
claimed `PROJECT: Hamlet`; the tree confirmed all four checks — `SHACK_FACTS.md`
and `src/Hamlet.RadioEngine/Cw/CwProbabilisticDecoder.cs` exist, neither
`CoreHMI.sln` nor `MURC.sln` does. **Hamlet confirmed.** Branch `main`
throughout, three commits, all pushed, none refused. Version 1.11.33 to 1.11.34
per HM-DEC-150.

**Nothing here is evidence about the radio.** No rig was connected.

**No decision was recorded under §12.1.** Section 4 carries what needs a ruling.

**The refusal did not ship, and the order is why.** It says: *"Tim ruled the
trade knowing it was five tests and part of six captures; if the true cost is
materially larger than that, stop and report rather than shipping."* **The true
cost is eleven tests, and six of them are a different kind of loss from the five
he weighed.**

**Task 2 is moot rather than dropped.** It re-expresses the anchors the refusal
breaks. No anchor is broken, because the refusal is not in.

### Where the order and the tree agree, and where they do not

- **All seven captures of 2026-08-28 are in the tree**, as stated. Confirmed.
- **`UseJointDecoder` and `ShowKeyingSweep` both ship false.** Confirmed,
  untouched.
- **`CLAUDE_CODE.md` is at 1.6 with twelve sections.** Confirmed; this report
  follows its §8.
- **The engine total is not 1852.** The order carries unit 1.11.33's figure. The
  measured total tonight is **1901**, because units 1.11.32 and 1.11.33 added
  tests after that run. The *failing* set was byte-identical at 28 before this
  unit's change, which is the number that matters.

## 2. What the owner should expect

**On a frequency where nothing is happening, the terminal still shows letters.
The fix is measured and not shipped, and this is the second time it has been
handed back — for a different reason than the first.**

Last time it was the five anchors, and you ruled that trade. **This time the
measurement found six more tests that were never part of the trade**, and one of
them is not an anchor at all: **tuning onto a station already sending now returns
nothing.**

**What did change on the screen:** one line on the capture sheet. `reading` now
says it covers the last twelve seconds alone rather than the whole recording.
Nothing else moved.

| | baseline | end |
|---|---|---|
| engine | 28 failing, byte-identical by name | **28 failing — the refusal is reverted** |
| app | 509 of 509 | **509 of 509** |

**With the refusal in, the run was 1901 tests, 1862 passed, 39 failed, 21.4
minutes.** That run is the measurement this unit exists to report; the tree is
back to 28.

## 3. What you should see

### Task 3 leads, as the order requires — how much junk the refusal does not reach

**Measured from the record, on every capture carrying both figures: the pitch
Hamlet admitted against the independent keying sweep, which shares nothing with
the tracker.**

| capture | admitted | sweep says | apart | emitted |
|---|---|---|---|---|
| `cw-2026-08-25-021825` | 395 | 600 | **205 Hz** | 63 |
| `cw-2026-08-23-001520` | 600 | 400 | **200 Hz** | 21 |
| `cw-2026-08-28-005158` | 750 | 600 | **150 Hz** | 69 |
| `cw-2026-08-28-005218` | 775 | 625 | **150 Hz** | 55 |
| `cw-2026-08-28-005243` | 775 | 625 | **150 Hz** | 73 |
| `cw-2026-08-26-125941` | 300 | 400 | **100 Hz** | 0 |
| `cw-2026-08-25-021629` | 510 | 600 | **90 Hz** | 77 |
| `cw-2026-08-28-004844` **good** | 430 | 375 | **55 Hz** | 41 |
| `cw-2026-08-25-012823` | 450 | 500 | **50 Hz** | 39 |
| `cw-2026-08-28-004902` **good** | 425 | 475 | **50 Hz** | 43 |
| `cw-2026-08-23-001952` | 525 | 475 | **50 Hz** | 50 |

**Eleven captures of the twenty-eight carrying both figures disagree by more than
25 Hz, up to 205.** Roughly four in ten.

**Two cautions, and they matter.** `cw-2026-08-28-004844` is one of the good
captures — it reads `TUES AUG 25` — and it disagrees by 55 Hz, **so disagreement
alone does not mean junk.** And the sweep is not a trustworthy referee: unit
1.11.31 measured it saying `no keying` on `cw-2026-08-24-012403`, which holds an
adjudicated callsign. **The right figure would be the strongest keyed bin
measured through Hamlet's own envelope**, and the sidecars carry that
(`competing`) on only three captures — all three of tonight's phantoms, all
saying **575 Hz** against admitted pitches of 750 and 775.

### Task 4 — where the tracker was, and the decay says how long

| capture | captured at | held peak | elapsed | lost |
|---|---|---|---|---|
| `cw-2026-08-28-005158` | 00:51:58 | 65.3 dB | — | — |
| `cw-2026-08-28-005218` | 00:52:18 | 45.8 dB | **20 s** | **19.5 dB** |
| `cw-2026-08-28-005243` | 00:52:43 | 20.3 dB | **25 s** | **25.5 dB** |

**`CwDecoder.cs:101`**: `SnrDecayDbPerHop = 0.005`, and measurements arrive two
hundred times a second — **exactly 1.0 dB per second**. **`CwDecoder.cs:1090`**:
the held figure rises only when the sustained median exceeds it, and otherwise
subtracts the decay.

**So 20 seconds cost 19.5 dB and 25 seconds cost 25.5 dB. That is pure decay to
within half a decibel in both intervals: the held peak was refreshed exactly
zero times in forty-five seconds.**

**The tracked bin at 750–775 Hz never once produced a measurement above its own
decaying held value across that whole stretch — and 197 characters came out of
it.** The sheet recorded the emptiness of that bin in a number nobody read, on
three consecutive sheets, while the terminal filled with letters.

`toneHz` on all three says *"measured from the keying the survey admitted"*, so
**the survey did admit keying at 750–775.** That is why the no-keying refusal
barely touches `005218` — two characters. **The residue is not a gap in the
refusal; it is admission being wrong**, and that is what tasks 3 and 4 were asked
to size.

### Task 1 — the refusal reproduces, and then costs more than was ruled on

**The table in the order reproduces exactly**, which is the first thing to say:

| capture | letters | blocks |
|---|---|---|
| `cw-2026-08-28-005158` | **1** | 59 |
| `cw-2026-08-28-005243` | **0** | 54 |
| `cw-2026-08-28-005051` | 13 | 17 |
| `cw-2026-08-28-004844` good | 41 | 2 |
| `cw-2026-08-28-004902` good | 45 | 2 |
| `cw-2026-08-28-004915` good | 35 | 7 |

**Eleven tests go red, not five, and they are two different kinds.**

**The five Tim weighed** — `N4L` on `cw-2026-08-17-134712` (HM-DEC-144), and
four W1AW bulletin readings of 2026-08-22 (`031905`, `032050`, `032113`,
`032129`).

**The six he did not:**

| test | what it loses |
|---|---|
| `AFastFistIsReadWithoutARunUp(30)` | **0.00 of the message**, against a bar of 0.79 |
| `AFastFistIsReadWithoutARunUp(35)` | the same shape |
| `TheSameFistWithARunUpDoesNot(28)` | — |
| `TheSameFistWithARunUpDoesNot(30)` | — |
| `TheSlowEndReadsTheMessage(10, 18 dB)` | 0.47 of the message |
| `TheSlowEndReadsTheMessage(10, 3 dB)` | — |

**These are clean synthesized signals, not captures, and the first one is the
case that changes the answer.** `AFastFistIsReadWithoutARunUp` is *tuned onto
mid-transmission* — landing on a station already sending, which is what happens
every time the dial moves. **Under the refusal it returns nothing at all** until
the survey admits a pitch.

**So the trade ruled on was five anchors read from an unmeasured pitch. The trade
actually on offer is those five, plus the ability to read a station you tune onto
rather than wait for.**

### Task 5 — the sheet's two spans, and the wording for approval

Implemented. **The wording is yours under §12.1 and is not treated as settled:**

> `reading    17 WPM won out of 8 to 40, -68562.4 better than silence per hop
> against a gate of 1  (this is the last 12 second window alone, at the moment
> of the press, and not the whole recording)`

## 4. What's blocking us

**The refusal costs the ability to read a station you tune onto, and that was not
in the trade you ruled.**

Ruling asked for:

> **The no-keying refusal ships, accepting that tuning onto a station
> mid-transmission returns nothing until the survey admits a pitch** — or it
> ships only where the survey has been given time to admit and has not.
>
> **Measured tonight**: eleven tests, not five. The five anchors are as you
> ruled. The six others are clean synthesized signals, and
> `AFastFistIsReadWithoutARunUp` at 30 words a minute comes back **0.00 of the
> message** where it read 0.79 before. **That test is the dial being turned onto
> somebody already sending**, which is most of how an operator finds a station.
>
> **What was rejected:** shipping without saying so, since the order's own guard
> requires stopping when the cost is materially larger; and narrowing the refusal
> on my own judgement, since any narrowing is a new rule about when Hamlet may
> speak and that is §12.1's territory.

**The nearest narrowing, offered but not built:** hold the refusal only until the
survey has had its first chance to admit — the phantoms ran for forty-five
seconds with nothing refreshing the peak, while a fresh tune-in has had no chance
at all. **That distinction is available in the tree** (the held peak's age is the
measurement task 4 just made) and it would need your ruling on what "had a
chance" means.

---

**Admission is admitting keying at 750–775 Hz where the station is at 599.3.**

Tasks 3 and 4 sized it: eleven captures in twenty-eight disagree with the
independent sweep by more than 25 Hz, and on tonight's three the held peak was
refreshed **zero times in forty-five seconds** while 197 characters came out.
**The refusal cannot reach any of it, because admission said yes.**

*Not proposed, because the order parks the tracker and admission:* the next unit
is the one that asks why a bin with no refreshed peak for forty-five seconds
keeps its admission.

### Asks still outstanding

Carried forward per HM-DEC-139 and HM-DEC-140. **Thirty-one inbound after this
unit. The oldest is open since 2026-08-14.**

1. **The sweep's `invented` column counts substitutions, not invented
   characters.**
2. **Whether the refill guard should apply to the first fill at all.**
3. **`ANNUNCIATOR.md` renamed `PHASE` to `TASK` while HM-DEC-150 makes `PHASE`
   match the version's minor.**
4. **`DECISIONS.md` has no record for HM-DEC-096–133, 136, 141 or 150.**
5. **The integrator width** — settled at 45 Hz, with the sharp-peak caveat.
6. **A boxcar's nulls made two of five swept offsets pathological best cases.**
7. **Two stations closer than 125 Hz are not named** — the operator's item five.
8. **The keying meter** — its measurement found a station its verdict denied,
   and **task 3 shows why that matters: it is the only referee available and it
   is not trustworthy.**
9. **HM-OPEN-057** (2026-08-22) and **HM-OPEN-007** (2026-08-14).
10. **The joint cutter cannot find word gaps on a compressed fist** (1.11.22).
11. **The constrained margin is bounded and still does not separate** (1.11.22).
12. **Four fixtures are absent and five acceptance lines were unmeasurable**
    (1.11.22).
13. **HM-DEC-086's supersession needs a record** (1.11.25).
14. **The phrasebook's arrival and the absent-widget news are gone** (1.11.25).
15. **The recent-places row has no home** (1.11.26), three options costed.
16. **The owned-property list has no enforcement of staying current** (1.11.27).
17. **A test resolved an ambiguous control by accident** (1.11.27).
18. **Nothing checks that deleting a surface is not deleting a capability**
    (1.11.28).
19. **The scanner and the calling cycle are attached to the rig with no
    control** (1.11.28), and one of them transmits.
20. **Thirteen dead `DataTemplate` blocks nothing can distinguish from live
    ones** (1.11.28).
21. **Whether every constructed view model should be reachable from a binding**
    (1.11.28).
22. **`013347` returns a likelihood ratio of 17.2 million**, with `001520`'s
    quadrillions. Parked, raised once.
23. **The empty corpus is blocked by the keying meter, with proof** (1.11.31).
24. **An offline sweep over a bank is not what the application runs**
    (`CwPitchRanking`'s lesson, for `DECISIONS.md`).
25. **The two captures have no ground truth and cannot be told from an empty
    band** (1.11.32).
26. **The emission floor was calibrated against a model that over-credits
    keying** (1.11.32).
27. **`cwdecoder.py` does not read what two orders say it reads** (1.11.32).
28. **The refusal against `N4L`** (1.11.33) — **ruled, and superseded by item
    29: the cost is larger than the ruling was given.**
29. **The refusal also costs reading a station you tune onto**, above — **the one
    this unit ends on.**
30. **Admission admits a pitch 150 Hz off the station and holds it for
    forty-five seconds without a refresh**, above.
31. **The `reading` line's new span wording needs approval**, above.

New this unit: **the refusal's true cost**, above; **admission's forty-five
silent seconds**, above; **the span wording**, above.

Closed this unit: **the refusal's cost per test** — eleven, named individually.
**How far admission strays** — eleven captures of twenty-eight by more than 25
Hz, up to 205. **Why the tracker held 750–775** — the held peak decayed at
exactly 1 dB per second across both gaps, so it was never refreshed at all.
**The sheet's two spans** — labelled, wording awaiting approval.

Still open: **the lock's mixed help**; **three fixtures at accepted cost**; **the
six-hertz window disagreement**; **`CHANGELOG.md` at 1.9.0 against 1.11.34**;
**the squelch has no axis**; **the three morning captures of 2026-08-26**;
**seven timing intermittents**.
