# Work instruction 302 — the moving string, and a name for the station

**READ IN THIS ORDER.** The instruction expected one moving string. **There were
four**, and only two of them were strings.

A. **The phase goal — FT4 works exactly the way FT8 does.** This unit advanced no
   step of it. It made the character ceiling measurable and gave a US station's card
   a name and a town.

B. **Step 4 and its exit criteria** — pressing FT4 tunes and decodes; the panel, the
   conversation, the ring, the filters, the tooltips, the ledger and the right-click
   menu all working unchanged; one click, one transmission; a whole exchange from one
   right click at the bench. **None was measured tonight.** Step 4 stays `partial`,
   and its second criterion now describes a card that names a US operator.

C. **The report last, and section 4 raises 3 items.** Two asks are closed by this
   unit. Nothing blocks the next one.

```
UNIT:       302 — complete at task 7 of 7, none dropped — 2026-09-10 09:13
PHASE GOAL: FT4 works exactly the way FT8 does.
UNIT GOAL:  The Digital tab's character count holds still, and a US station's card
            says who and where he is.
ADVANCED:   no — no phase step moved. Step 4 stays partial: nothing tuned, nothing
            transmitted, no exchange run at the bench.
NUMBER:     the Digital tab's character count, measured twice
            BEFORE: 1234 then 1233, thirty-seven seconds apart. Across nights it
            had read 1281, 1293, 1259 — never the same figure twice.
            AFTER:  1194 then 1194, and 1193 then 1193 for the working state,
            with the countdown reading 13 and then 11 in between. Reproduced
            across repeated class runs and from a second test class.
DRIFT:      6 consecutive units without advance  (was 5, carried from unit 301)
```

---

## 1. What Claude did

**Complete. Seven tasks of seven, none dropped, including the named drop candidate.**
Development machine, prompt claimed `PROJECT: Hamlet`, branch `main`, three commits,
all pushed. Root version 1.12.264 to **1.12.265**, bumped once. **No file under
`src/Ft8Sharp/` was touched**, and nothing here transmits.

**Nothing in this report is evidence about the radio.** Nothing was tuned, nothing
was transmitted, and no audio was captured.

**Nothing was recorded to `DECISIONS.md`.** The four judgements made on this session's
authority are in `PHASE_OUTCOME.md`'s entry and are listed below.

**One defect in the order, reported and not repaired** (§9.6): the file carries
`PROJECT:` but **no `ISSUED:` line**. It is plainly this unit's — it is the only order
in the tree and its heading carries 302 — so the work proceeded.

### Task 1 — the moving string, found by standing the application up

**Not searched for.** The probe realizes the Digital tab, writes down every string
with the control that owns it, and two runs are diffed. **Thirty-seven seconds apart
it found the first one immediately.**

Following each to what composes it gave four causes, not one:

| # | What moves | What makes it move |
|---|---|---|
| 1 | **The slot countdown**, `10` then `3` | `Ft8Turn.CountText` off `DateTime.UtcNow`. 1 to 3 characters |
| 2 | **The card's relative age** | `Ft8ContactCard.Ago` against a fixture whose slot was fixed and whose *now* was not |
| 3 | **The waterfall summary**, `not listening yet` | `ReconnectOnStartup` ships on, so a background connect starts the spectrum |
| 4 | **The clock-offset line** | `QueryTheClockAsync` runs a **live SNTP network call** |

**Number 2 is worse than a string changing width.** The seeded card aged out of the
list entirely, and the empty-state prose it had been hiding came back: **the total
rose 16 while the block count fell 10.**

**Number 3 is why both Digital rows fell by exactly 17 together** on some class runs
and not others. **Number 4 is why the first window built in a process measured 1240
and every later one 1194** — the time server having replied in between.

**Unit 284's rule held exactly.** All four are composed at run time; a source search
would have come back empty and read as clean.

### Task 2 — the ceiling holds still

**Two of the four belong on screen and neither moved behind a hover.** The countdown
is Tim's own ask — *nobody has responded, but I want to know how long I have till the
next transmit cycle* — and how stale a card is, is why its time line exists. **Both
are live measurements, not prose, and the ceiling exists to stop prose growing.** So
the sweep counts each at its widest reading, derived and stated: countdown **3**, time
line **28**, clock line **47**. A readout that grows past its stated width **fails
loudly** rather than being quietly clamped.

**The other two were fixture nondeterminism** and are pinned: the card's clock, and
the clock offset stated rather than raced.

**All four MainWindow rows re-set** from figures reproduced across repeated runs:
CW **426**, Digital **1194**, Digital working **1193**, Voice **468**. They are lower
than before because with the offset stated the strip carries the unmeasured line
rather than a measured one.

### Tasks 3 to 6 — callook, and a name on the card

**No second client.** `CallookCallsignLookup` has been how Settings resolved the
operator's own class, coordinates and grid since 2026-08-14. `StationDirectory` is a
cache and a parser over that same one, handed in rather than built.

**The parser's own restraint was narrowed, not undone.** That file has declined the
whole name-and-address block since it was written, with the reason stated in its
remarks. Somebody has now asked — for two fields. **The street is still named nowhere
in this application.**

### Task 7 — the outcome entry

Filed as **`UNIT 302 - STEP 4`** with nothing renumbered by hand.

### The four things decided on this session's authority

1. **The two real live readouts stay on screen** and the ceiling bounds what it counts.
2. **The two fixture races are pinned in the fixture**, not worked around in the sweep.
3. **The name replaces the country on a card** rather than joining it.
4. **The street address is still read nowhere.**

---

## 2. What the owner should expect

**A US station's card says who he is.** `W7PP · Richard, Sun City AZ · 1,900 miles`
where it used to say `United States`. **Everyone else's card reads exactly as it did**
— `VP2MAA · Montserrat · 1,900 miles`, unchanged — because callook only holds US
licences and silence is the right answer rather than a gap.

**It never waits for the network.** The card is drawn from what is already known and
the answer arrives on its own; if the lookup is slow, or fails, or you are offline,
the card reads as it does today. No spinner, no error, no gap where a name would be.

**Two red tests are green** for the first time in three units.

**What will look wrong and is not:**

- **The character ceilings all got smaller.** CW 650→550, Digital 1250→1300 with a
  lower measured figure, Voice 650→600. The surfaces genuinely say less, because the
  sweep now states the clock's condition instead of racing a time server for it.
- **`TheAchievementsScreenTests` has 2 red.** Measured with this unit's changes
  stashed: **the same 2 were red before it started.** Inherited, not caused, and left
  alone under §12.6.

**Build:** succeeded, 0 warnings, 0 errors. **Tests:** 21 constructed in this
instruction across four classes, **all green**, each filtered by exact name and
foregrounded. **No suite was run** (HM-DEC-155). Gates re-run: `BindingHealthTests`
green, `HowMuchTheApplicationSaysTests` 5 of 5 green.

**Pushed:** three commits to `main`. Nothing of this unit's is uncommitted.

---

## 3. What you should see

### 1. The moving string, quoted, and what makes it move

```
run a at 2026-09-10 08:45:33   total 1234   blocks 80
run b at 2026-09-10 08:46:10   total 1233   blocks 80

ONLY IN THE FIRST RUN  (1)
  - 0002  DigitalContactCards  |10|
ONLY IN THE SECOND RUN (1)
  + 0001  DigitalContactCards  |3|
```

**That is the slot countdown**, `TurnRingCount` → `Ft8Turn.CountText`, composed in
`RefreshTurn` from `DateTime.UtcNow`. Seconds to the next slot boundary.

The other three, each quoted from the diff at the moment it moved:

```
  - |clock is 1.68 s slow, checked just now|      ClockOffset.Describe, off a live
  + |clock is 1.67 s slow, checked just now|      SNTP call

  - |21:41:30 UTC · 39 hours ago|                 Ft8ContactCard.Ago, against a
                                                  fixture slot fixed at 2026-09-08

     |not listening yet|                          DigitalWaterfallSummary, present
                                                  only until a background reconnect
                                                  starts the spectrum
```

### 2. Two measurements of the same tree, matching

```
MainWindow — Digital tab           1194  then  1194   same
      live readouts, first  : TurnRingCountText="13"  CardTimeLineText="…2 minutes ago"
      live readouts, second : TurnRingCountText="11"  CardTimeLineText="…2 minutes ago"
MainWindow — Digital tab, working  1193  then  1193   same
MainWindow — CW tab                 426  then   426   same
```

**The countdown read 13 and then 11 between the two sweeps.** The surface moved and
the figure did not, which is the property the ceiling needs. The second measurement is
taken **after a readout has really changed**, and with other surfaces built in
between, because that is the condition the sweep actually runs in.

### 3. A US card and a non-US card, quoted

```
US station     : W7PP   · Richard, Sun City AZ · 1,900 miles
everybody else : VP2MAA · Montserrat           · 1,900 miles

nothing asked yet : W7PP · United States · 1,900 miles
with no network   : W7PP · United States · 1,900 miles
```

**What the live service actually returns**, read on 2026-09-10 rather than assumed:

```
W7PP    "status":"VALID"    name "RICHARD R HALE"   line2 "SUN CITY, AZ 85373"
VP2MAA  {"status": "INVALID"}                       and nothing else at all
```

**The cost, measured.** A lookup takes **0.12 to 0.15 s**. A busy slot of 14 messages
naming 3 stations, seen over 4 slots — **56 messages — makes 3 requests**, one per
distinct callsign, with one in flight per callsign. **The answer that there is nothing
to know is cached too**, or a non-US callsign would be asked about every slot for
ever. **A transport failure is deliberately not cached**: the network being down says
nothing about a callsign, so the next ask is allowed to succeed. **The cache is in
memory and does not survive a restart**, so a hundred distinct callsigns in an evening
is a hundred requests that evening and a hundred again tomorrow.

### 4. The resolve rate, and whether the state ask is closed

```
callsigns asked about : 10
a state               : 1   W7PP
country only          : 9   W3YNI, W1ABC, KC3QIS, K4XYZ, VP2MAA,
                            IK4LZH, EI4GNB, VA3VRR, N4L
```

**It narrows the ask; it does not close it, and the shape of what is left is the
point.** Hamlet can now say Arizona **for a US callsign and for nothing else**. The
figure above is against a fake that resolves one callsign, so it measures the shape of
the answer rather than the service's coverage — but the shape is the whole finding:
**every callsign outside the United States gets a country and nothing below it, for
ever, because callook holds only US licences.**

That is correct behaviour under the standing rule rather than a shortfall. **The ask
is closed for the case it was raised about** — units 297, 298 and 299 each hit *the
United States* with no way to say which state — **and it stays open for everywhere
else**, where the honest answer is that Hamlet does not know and says nothing.

---

## 4. What's blocking us

**Nothing blocks the next unit.**

### 1. `TheAchievementsScreenTests` has two red, inherited

Measured with this unit's changes stashed: `WsprIsNotAFirstAnybodyCanEarnAndTheCardSaysSo`
and `TheWindowDrawsEverySixRows` were **already red before this unit started**. They
are not in `docs/unit239-failing-set.txt` and no order names them, so they are an
inherited red nobody has recorded. Left alone under §12.6.

### 2. Two ±2 wobbles remain on other surfaces

`SettingsWindow` reads 1628 or 1626 and `AboutWindow` 726 or 725 across runs. **Both
are far under their ceilings and neither is a Digital row**, so neither was chased.
Named here so the next unit to touch the ceiling knows they exist.

### 3. `Ft8Sharp` did not move

No file under `src/Ft8Sharp/` was read, edited or built.

### Asks still outstanding

Carried verbatim per HM-DEC-139.

1. **Nothing in this repository can look at a picture.** *First made 2026-09-09, unit
   300.* Five units running have now reported every appearance claim as computed
   rather than seen. Real pixels want `Avalonia.Headless.Skia`, and **a package is a
   dependency decision rather than a session's** (§0.4). **Waiting on:** your ruling.
   **Where it sits:** nowhere — no package has been added.

2. **The map image.** *First made 2026-09-09, unit 301.* The projection is built and
   proved against independent trigonometry; what is missing is the picture and its
   three numbers. **Waiting on:** a file from you, not a session. **Where it sits:**
   `assets/azimuthal-map.md` carries what is needed and the five steps to finish.

**And two that are dropped rather than carried.**

**`HM-OPEN-089` is closed by task 1**, four causes found and measured, two ceiling rows
green. The entry in `OPEN_ISSUES.md` is marked `closed: 2026-09-10` and carries all
four causes, so the next reader gets the finding rather than the question.

**The US state ask is closed for the case it was raised about**, and section 3 says
plainly where it stays open.
