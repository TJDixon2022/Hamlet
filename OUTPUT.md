UNIT:       045 — complete at task 4 of 4 — 2026-08-28 21:19
PHASE GOAL: Readable CW on the operator's screen — eighty percent of a strong signal read correctly, first time.
UNIT GOAL:  Port the reference decoder's acquisition and gating into the engine and measure it head to head against the shipped path.
ADVANCED:   no — the reference was ported faithfully and lost the head to head, so it ships off; the shipped decode path is unchanged.
NUMBER:     shipped 12 of 12 adjudicated readings -> reference 1 of 12.
DRIFT:      2 consecutive units without advance  (was 1)

## 1. What Claude did

**Complete: all four tasks, and the answer to the question this unit was
commissioned to ask is no.** Task 4 was not dropped.

Development computer, prompt claimed `PROJECT: Hamlet`, branch `main`, version
`1.12.6` unchanged. **Nothing here is evidence about the radio**: no radio was
connected, and every number comes from WAV files already in the tree.

### Task 1 — the port

`CwReferenceDecoder` carries the whole chain, function for function, each name
kept so the two can be read side by side. **Every line number in the work order's
table is correct against the tree** — `mute_mask:46`, `acquire_tone:62`,
`fine_envelope:77`, `two_means:105`, `gate:113`, `deglitch:135`, `runs:145`,
`fit_clock:163`, `well_separated:193`, `classify_gaps:212`, `decode:245`. The
tree's `cwdecoder.py` is the one committed at `351784a` and nothing in it differs
from the order's description.

**The port agrees with its source on all forty-four captures.** Run side by side:
the same acquired pitch, the same decision about whether a clock fits, the same
gate contrast, the same character count, and **the same transcript character for
character on 44 of 44**. The only differences anywhere are the fitted dah on
`cw-2026-08-22-032012` (422 against 423) and the fitted dit on
`cw-2026-08-28-005218` (142 against 143), each under a millisecond, which is
floating-point accumulation order rather than behaviour.

Both of the reference's load-bearing comments came across verbatim: that the
2.5–3.8 ratio band **refused `cw-2026-08-17-134712`'s real 4.24-dit fist**, and
that `well_separated` was tried as a *replacement* for the band and measured at
five decibels dropping fast-working from 58 % to nothing, so both are kept.

**Where the engine already had the thing, the engine's was used**, which is the
order's own instruction and is tokens back. `MorseAlphabet.Lookup` is the table
and `MorseAlphabet.Unreadable` is the placeholder, so the port keeps no second
copy of either. The reference renders its tainted and its unknown characters as
the same glyph, so folding them onto Hamlet's own mark loses nothing.

**One mismatch in the order, and it matters because it was a stated test.** The
order asks that *"`acquire_tone` finds 429 Hz on `cw-2026-08-28-004844`"*. It
cannot: the grid is 300 to 900 in steps of 25, so the answer is always a multiple
of 25, and on that capture it is **425**. The 430 the reference prints is a
different quantity — the median of the fine tracker's seven offsets over the loud
hops. Both are now pinned by test.

Ten tests, all green.

### Task 2 — the head to head

**The reference returns one of the twelve adjudicated readings. The shipped path
returns twelve.**

| capture | adjudicated anchor | shipped | reference |
|---|---|---|---|
| `cw-2026-08-17-013347` | `VA3VRR` | yes | **yes** |
| `cw-2026-08-17-134712` | `N4` | yes | no |
| `cw-2026-08-18-003758` | `MP/4 QNIK` | yes | no |
| `cw-2026-08-24-012403` | `DE KD0UN KD0UN K` | yes | no |
| `cw-2026-08-18-004507` | `N HANDLING THIS MESSAG` | yes | no |
| `cw-2026-08-22-031838` | `, AND` | yes | no |
| `cw-2026-08-22-031905` | `DICTED 10.7` | yes | no |
| `cw-2026-08-22-031948` | `110, AND 110 WITH A MEAN OF 117` | yes | no |
| `cw-2026-08-22-032012` | `R OTHER WEBSITES MENTI` | yes | no |
| `cw-2026-08-22-032050` | `ULLETIN CAN BE FO` | yes | no |
| `cw-2026-08-22-032113` | `INT` | yes | no |
| `cw-2026-08-22-032129` | `OPAGATION` | yes | no |

**The port is not what lost, and that is the important half.** `cwdecoder.py`
produces the same output on the same files. On `cw-2026-08-18-004507` it acquires
**700 Hz** for a station the sheet measured at 500 and reads
`E E TETEEE TE ETIT NE ETEEIE`. On `cw-2026-08-24-012403` it acquires 440
**correctly**, fits a plausible 59/187 ms clock, and still reads
`EEIEIEETE■ETTTITIEETT…`. So the failure is not acquisition alone: on that capture
everything upstream is right and the segmentation still produces nothing.

**Where the reference wins, it wins clearly.** On `cw-2026-08-17-013347` it reads
`■ ■ ■ ■ ■ ■ M VRR VA3VRR` in sixteen characters where the shipped path needs
fifty-nine to reach the same callsign. On `cw-2026-08-28-004844` it reads
`K I L O T U E S A U G 2 5 K C 9 U C Q R ET 8 8 <BT> B R U C E <AR> N R 2 3 0 C`,
the cleanest reading of that net anything in this repository has produced. On
`cw-2026-08-28-004902` it gets `NR 2 3 0` and `W 7 G B` where the shipped path
gets the callsign and loses the number.

**The four phantoms**: the reference silences one of four. `005158` refuses
outright — a tone at 595 Hz and no clock fits. `005051`, `005218` and `005243`
still emit 23, 14 and 19 characters, against the shipped path's 30, 53 and 54.
Fewer, and not none.

**The two silence controls**: one of two. `cw-2026-08-20-014935` refuses, which is
exactly the structural refusal the port was made for. **`cw-2026-08-20-014854` does
not**: a clock fits, and eighteen characters come out of a capture this suite has
always called HOLDS NOTHING — `■ ■■I M YOY■KB A NB ■A IM` — where the shipped path
emits none.

**Across the corpus the reference refuses outright on 11 of 44.**

### Task 3 — the setting stays off

`AppSettings.UseReferenceDecoder`, default **false**. Every one of the four
acceptance lines fails:

- the four phantoms emit no letters — **fails**, three of four still emit;
- all twelve adjudicated anchors still read — **fails**, one of twelve;
- both silence controls silent — **fails**, one of two;
- the reference picks the reading pitch on more captures — **fails**, one against
  twelve.

**No constant was tuned to try to pass a line.** It is a port, and the order is
right that a tuned port is a seventh invention.

### Task 4 — cost

Measured on thirty seconds of `cw-2026-08-28-004844`:

| | whole file | share of one core |
|---|---|---|
| `acquire_tone` alone | 312 ms | **1.0 %** |
| the whole reference chain | 401 ms | **1.3 %** |
| the shipped path | 1840 ms | **6.1 %** |

**The reference is about five times cheaper than what ships.** `acquire_tone` is a
25 ms Goertzel over 25 bins at a 10 ms hop, against the 1240 ms a sweep of 25 full
decodes cost in unit 1.12.6 — **a factor of four hundred.** Cost was never why
this was not adopted. Measure only; nothing changed on its account.

No decision was recorded under §12.1.

## 2. What the owner should expect

**With the setting on, a dead frequency does not show nothing, and a station you
can hear does not reliably get picked.** One of the two dead-air captures fills
with eighteen characters, three of the four junk captures still emit, and eleven
of the twelve readings you have adjudicated stop coming back. **So it is off, and
nothing you see tonight is different from this morning.**

What is now true of the tree:

- `CwReferenceDecoder` is in the engine, a faithful port with ten tests, agreeing
  with `cwdecoder.py` transcript for transcript on all forty-four captures.
- `AppSettings.UseReferenceDecoder` exists and is **false**. Two tests go red if
  that changes.
- `tools/Hamlet.PitchRank` gained `reference`, `headtohead` and `refcost`, so
  every figure above is one command from being re-measured.
- The shipped decode path was not touched at all.

**What will look wrong but is not:**

- **The engine baseline is still 28 failing.** Nothing this unit did went near
  them.
- **The reference reads `cw-2026-08-28-004844` better than Hamlet does**, and it
  is still switched off. That capture is what the argument for the reference
  rested on, and it does not generalise: the same chain loses eleven readings.
- **The full engine suite has no result here.**
  `TheGateHasItsOwnWindowNowTests` crashes the test host — **HM-OPEN-061**, raised
  last unit and reproduced there on the pre-044 tree. What ran: 32 engine tests
  across the port, the twelve anchors and the pedestal ranking, all green; 2 app
  tests green.
- **`cwdecoder.py` itself is unchanged.** The order said not to improve the
  reference while porting it, and nothing in it was edited.

## 3. What you should see

**On how many of the forty-four does the reference pick the pitch that reads?
Measured against the only ground truth there is — the twelve readings somebody has
adjudicated — the reference returns one and the shipped path returns twelve.**

The table is in section 1. The short version is that the reference is not a better
decoder than the one that ships. It is a **different** one: better on two captures
and much worse on ten.

**What it is genuinely better at is saying nothing.** It refuses outright on
eleven of forty-four, and every one of those refusals is structural — no clock
fits, so nothing runs — with no threshold anybody chose. That is the property six
units of admission statistics were built to get and never got.

It is not enough on its own, though, and the same corpus says so: one of the two
dead-air captures still produces a clock and eighteen characters. So `fit_clock`'s
refusal is a real mechanism and not yet a complete answer to your first question.

**And it costs a fifth of what the current path costs**, so if any part of it is
ever wanted, affordability is not the obstacle.

**The argument this retires** is that the reference reads these captures and
Hamlet has been reinventing it. It reads two of them well. On your own adjudicated
set it reads one in twelve.

## 4. What's blocking us

One ruling, and it is about what to take from a decoder that lost.

> **The reference's structural refusal is worth taking on its own, and the rest of
> it is not.**
>
> `fit_clock` returning nothing when the marks do not form two lengths refuses
> eleven of forty-four captures outright with no threshold anybody chose, which is
> the property six families of admission statistic were built for and never
> reached. It is separable from the rest of the chain: it reads mark lengths, and
> the shipped path already has mark lengths.
>
> **Rejected: adopting the reference whole.** Measured this unit at one of twelve
> adjudicated readings against twelve.
> **Rejected: taking its acquisition.** It picks 700 Hz for a station at 500 on
> `cw-2026-08-18-004507`, and on `cw-2026-08-24-012403` it picks the right pitch
> and reads junk anyway, so acquisition is not where its advantage lies either.
> **Rejected: treating `fit_clock` as a finished answer to the phantoms.**
> `cw-2026-08-20-014854` holds nothing, fits a clock, and yields eighteen
> characters. The refusal is real and it is not sufficient.
> **What is not yet decided** is whether a refusal that fires on eleven of
> forty-four is worth what it costs on the other thirty-three, and that is a
> measurement rather than a judgement: the same head-to-head table, with only the
> clock refusal grafted onto the shipped path.

### Asks still outstanding

Carried forward per HM-DEC-139 and HM-DEC-140.

1. **The pedestal ranking is measured at 34 of 44 and unbuilt.** The order said it
   becomes its own unit **if the reference loses**. It lost.
2. **The ranking's ten misses are unexamined.**
3. **`DECISIONS.md` has no record for HM-DEC-096–133, 136, 141 or 150.**
4. **The `reading` line's span wording needs approval.**
5. **Two stations closer than 125 Hz are not named.**
6. **HM-OPEN-057** (2026-08-22) and **HM-OPEN-007** (2026-08-14).
7. **Nothing checks that deleting a surface is not deleting a capability** — the
   favourites list is gone and the operator found it by hand.
8. **A capture sheet carries a score of −68562.4** (`cw-2026-08-28-005158`), first
   raised in unit 1.12.6 and unruled.
9. **`TheGateHasItsOwnWindowNowTests` crashes the test host** (**HM-OPEN-061**),
   so full-suite acceptance is assembled from batches. Owned by Claude, not
   waiting on a ruling.
