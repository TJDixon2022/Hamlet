UNIT: 027 — stopped at task 2 of 4 — 2026-08-27 21:09

## 1. What Claude did

**Stopped. Tasks 1 and 2 done; task 3 is moot and task 4, the drop, was not
reached.**

Claude Code, on the development computer, in `C:\Source\HamLet`. The prompt
claimed `PROJECT: Hamlet`; the tree confirmed all four checks — `SHACK_FACTS.md`
and `src/Hamlet.RadioEngine/Cw/CwProbabilisticDecoder.cs` exist, neither
`CoreHMI.sln` nor `MURC.sln` does. **Hamlet confirmed.** Branch `main`
throughout, three commits, all pushed, none refused. Version 1.11.32 to 1.11.33
per HM-DEC-150.

**Nothing here is evidence about the radio.** No rig was connected.

**No decision was recorded under §12.1.** Section 4 carries what needs a ruling,
and this unit ends on one.

**Nothing shipped from task 2, by the order's own rule.** The refusal works and
costs adjudicated anchors, and the order says that means ship the other two —
which are already-enforced and unshippable respectively.

**Task 3 is moot rather than dropped.** It asks the terminal to say why it is
quiet *when a refusal in task 2 is holding*. No refusal holds, so there is
nothing to explain and a line saying so would be describing behaviour the
application does not have.

### Where the order and the tree disagree

- **The expected state is stale.** The order says 28 failing of **1841** in the
  engine and **503 of 503** in the app, with `CLAUDE_CODE.md` at **1.4**. The
  tree is **1852** engine tests with 28 failing, **509** app tests, and
  `CLAUDE_CODE.md` at **1.6 with twelve sections**. The order also numbers itself
  **027** while the tree's last report is unit 035. **The captures and the
  `unkeyed` line it cites are from tonight**, so the order is new and its
  header block is copied from an older one.
- **`AppSettings.UseJointDecoder` and `ShowKeyingSweep` both ship false**, as
  stated. Confirmed, untouched.

## 2. What the owner should expect

**Nothing on the screen has changed, and the phantoms are still there.** The fix
was built, it works, and it is handed back rather than shipped — because it
takes `N4L` with it, and this project already ruled that trade to be yours.

**What did change is that the phantoms are now explained.** The gate is not
broken. It never was.

| | baseline | end |
|---|---|---|
| engine | 28 of 1852, byte-identical by name | **anchors and silence controls re-run green; no full run** |
| app | 509 of 509 | **not re-run — no app file was changed** |

**No engine source file was changed.** The refusal was written into
`CwDecoder`, measured, and reverted with `git checkout`; what remains is the
measurement in tests.

## 3. What you should see

### Task 1 — the gate is firing, and the sheet is what lied

**Every one of the seven captures streamed through a real decoder: zero
characters settled while the standing window was below the gate.**

| capture | characters | settled under the gate | lowest ratio behind one |
|---|---|---|---|
| `004844` good | 190 | **0** | 22.33 |
| `004902` good | 175 | **0** | 19.16 |
| `004915` good | 154 | **0** | 5.48 |
| `005051` phantom | 114 | **0** | 1.48 |
| `005158` phantom | 58 | **0** | 1.49 |
| `005218` phantom | 101 | **0** | 6.17 |
| `005243` phantom | 158 | **0** | 3.34 |

**So the −68562.4 is not a window that emitted.** It is `_probabilistic.Last` —
**the score of the final window at the moment of the press** — printed on a sheet
beside `inThis`, a character count accumulated across the whole recording. The
two figures are about different things and the sheet puts them three lines apart.

**That is HM-DEC-091's `tonePeak` fault arriving in a second field**, and it is
what sent this order after a gate that was working. The phantoms cleared the
gate honestly, at 1.48 and 1.49 against 1.40 — **by a hair, but above it.**

### Task 2 — the three refusals, measured

**Refusal 2, window score below the gate: already enforced.** Task 1 is the
proof. Nothing to build.

**Refusal 3, clock withdrawn: measured and not shippable.** Counted at the moment
each character settles, it would block **26, 38 and 25** characters on `004844`,
`004902` and `004915` — the three captures from earlier that read a real
bulletin. That is the good case paying for the bad one, which the order forbids.

**Refusal 1, no keying admitted: works.** Wired at the emit seam as **blocks
rather than deletions**, so no character position is lost and only the assertion
goes:

| capture | letters | blocks | what it spells |
|---|---|---|---|
| `004844` good | 41 | 2 | `K IL O TUES AU G 2 5 K C 9 E T…` |
| `004902` good | 45 | 2 | `TTEL <BT> BRU C E <AR> NRE…` |
| `004915` good | 35 | 7 | `■ ■ ■■L A WED AUG 2 6 W 7 G B…` |
| `005051` phantom | 13 | 17 | mostly blocks |
| **`005158` phantom** | **1** | **59** | **almost entirely blocks** |
| `005218` phantom | 40 | 13 | still letters |
| **`005243` phantom** | **0** | **54** | **no letters at all** |

**`005158` goes from sixty characters to one letter. `005243` goes to none.** And
the good captures keep `TUES AUG 25`, `WED AUG 26`, `W7GB` and `BRUCE`.

**`005218` is barely touched** — 2 characters — because its pitch *was* measured.
The refusal reaches what the survey admits nothing on, and on that capture the
survey admitted something.

### And it costs anchors, which the tree predicted in a comment

Five tests fail with it in, including **`N4L` on `cw-2026-08-17-134712`**
(HM-DEC-144). `CwDecoder` already carried this, written before tonight:

> Refusing to decode until the survey admits a candidate was built and measured,
> **and it costs `N4L`** on `cw-2026-08-17-134712` along with six other captures'
> text. The reason is worth keeping: that recording's fallback bank centre is
> 500.0 and its station sits at 500.09, **so the callsign was only ever read
> because an unmeasured number happened to land on it. Honesty and that callsign
> are in tension and the ruling is Tim's.**

**The order's ruling and the order's acceptance are on opposite sides of that
tension.** The ruling says never letters from a pitch nobody judged a station;
the acceptance says all twelve anchors green. **`N4L` is a letter from a pitch
nobody judged a station.** The order's tie-break — ship the other two — leaves
nothing to ship, so nothing shipped.

## 4. What's blocking us

**The refusal that ends the phantoms is the one that costs `N4L`, and that trade
has been waiting for you since it was first measured.**

Ruling asked for:

> **Hamlet stops printing letters from a pitch the survey admitted no keying at,
> and `N4L` becomes blocks.** Measured tonight: it takes
> `cw-2026-08-28-005158` from sixty characters to one letter and `005243` to
> none, while the three captures that read a bulletin keep it at a cost of two,
> two and seven blocks. It costs five tests, including the `N4L` anchor of
> HM-DEC-144 and part of six captures' text.
>
> **`N4L` was never read from a measurement.** `CwDecoder`'s own comment records
> why: the fallback bank centre is 500.0, the station sits at 500.09, and the
> callsign came back because an unmeasured number happened to land on it. **It
> is a correct reading obtained the way the phantoms are obtained.**
>
> **What was rejected:** the clock-withdrawn refusal, measured at 26, 38 and 25
> characters off the three good captures; and raising the gate, which the order
> forbids and which task 1 shows would be aimed at a mechanism that is working.

**This is the whole unit.** Everything else is measured and settled.

---

**The capture sheet prints a last-window score beside a whole-recording count.**

`reading … −68562.4 better than silence per hop` is `_probabilistic.Last`, one
window, taken at the instant of the press. `inThis 69 characters emitted` covers
thirty seconds. **Three lines apart on the same sheet, nothing says they are
about different spans**, and this order was written from reading them together.

*Not proposed, because §12.1 puts the sheet's assertions outside a session:*
whether `reading` says which window it is about, the way `tonePeak` and the
running totals were made to after HM-DEC-091.

---

**Task 4 was not reached, and it is the drop.**

Why the tracker holds 750–775 Hz while two independent instruments say 600–625
is unmeasured by this unit. **Task 1's trace gives the next unit a running
start**: it streams all seven captures through a real decoder and already reports
per-capture state.

### Asks still outstanding

Carried forward verbatim per HM-DEC-139 and HM-DEC-140. **Thirty inbound after
this unit. The oldest is open since 2026-08-14.**

1. **The sweep's `invented` column counts substitutions, not invented
   characters.**
2. **Whether the refill guard should apply to the first fill at all.**
3. **`ANNUNCIATOR.md` renamed `PHASE` to `TASK` while HM-DEC-150 makes `PHASE`
   match the version's minor.**
4. **`DECISIONS.md` has no record for HM-DEC-096–133, 136, 141 or 150.**
5. **The integrator width** — settled at 45 Hz, with the sharp-peak caveat.
6. **A boxcar's nulls made two of five swept offsets pathological best cases.**
7. **Two stations closer than 125 Hz are not named** — the operator's item five.
8. **The keying meter** — its measurement found a station its verdict denied.
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
23. **No capture reproduced the junk the operator was watching** (1.11.30) —
    **closed tonight: seven captures arrived and four reproduce it.**
24. **The empty corpus is blocked by the keying meter, with proof** (1.11.31).
25. **An offline sweep over a bank is not what the application runs**
    (`CwPitchRanking`'s lesson, for `DECISIONS.md`).
26. **The two captures have no ground truth and cannot be told from an empty
    band** (1.11.32).
27. **The emission floor was calibrated against a model that over-credits
    keying** (1.11.32).
28. **`cwdecoder.py` does not read what two orders say it reads** (1.11.32).
29. **`N4L` against the phantoms**, above — **the one this unit ends on.**
30. **The sheet prints a last-window score beside a whole-recording count**,
    above.

New this unit: **the `N4L` trade**, above; **the sheet's two spans**, above.

Closed this unit: **why the gate did not fire** — it did fire, on all seven
captures, and the −68562.4 was a snapshot read as a summary. **What each refusal
costs** — 26/38/25 on the good captures for the clock one, five anchors for the
no-keying one, nothing to build for the gate one. **A capture that reproduces the
phantoms** — four of them.

Still open: **the lock's mixed help**; **three fixtures at accepted cost**; **an
unmeasured pitch costs `N4L`** — **now measured exactly**; **the six-hertz window
disagreement**; **`CHANGELOG.md` at 1.9.0 against 1.11.33**; **the squelch has no
axis**; **the three morning captures of 2026-08-26**; **seven timing
intermittents**.
