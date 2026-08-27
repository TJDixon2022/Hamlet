UNIT: 033 — stopped at task 2 of 9 — 2026-08-27 17:13

## 1. What Claude did

**Stopped, at task 2 of 9. Tasks 3 to 9 were not done and the reason is below.**

Claude Code, on the development computer, in `C:\Source\HamLet`. The prompt
claimed `PROJECT: Hamlet`; the tree confirmed all four checks — `SHACK_FACTS.md`
and `src/Hamlet.RadioEngine/Cw/CwProbabilisticDecoder.cs` exist, neither
`CoreHMI.sln` nor `MURC.sln` does. **Hamlet confirmed.** Branch `main`
throughout, three commits, all pushed, none refused. Version 1.11.29 to 1.11.30
per HM-DEC-150.

**Nothing here is evidence about the radio.** No rig was connected.

**No decision was recorded under §12.1.** Section 4 carries what needs a ruling.

### Why it stopped, and what the next unit inherits

**Task 1's baseline contradicted the order's diagnosis, and tasks 3 to 7 are
built on that diagnosis.** Continuing into them would have been building a
mechanism for a fault the measurement says is not there. **The stop is a sizing
decision I made and the owner did not** — the order named task 9 as its drop and
I dropped seven — so §8 requires it reported as one, and this is it.

**What the order expected:** an empty band filling with `E space E space I`,
caused by acquisition picking the wrong pitch, cured by the strongest bin
choosing and a channel hold squelching the rest.

**What the tree measures:**

| capture | pointed at | emits |
|---|---|---|
| `cw-2026-08-20-014854`, holds nothing | 600 Hz | **0 characters** |
| `cw-2026-08-20-014935`, holds nothing | 825 Hz | **0 characters** |

**Both recordings that hold nothing already emit nothing through the real
chain.** Task 7's target of nought is met on all the empty audio that exists, and
was met before this unit touched anything. **The 93 characters unit 1.11.29
reported were `CwPitchRanking` sweeping the bank offline** — a component built
deliberately disconnected, which the application does not run. That figure is
correct about the component and was carried into this order as though it
described the app.

**And the four the operator can hear are mostly pointed correctly already:**

| capture | he hears | pointed at | error | emits |
|---|---|---|---|---|
| `cw-2026-08-25-012823` | 500 Hz | 450.0 | **−50** | 41 characters of `E TTE TTN TN T IT K` |
| `cw-2026-08-22-014113` | 607 Hz | 600.0 | −7 | **0** |
| `cw-2026-08-22-014308` | 606 Hz | 575.0 | −31 | **0** |
| `cw-2026-08-26-125941` | 403.5 Hz | 400.0 | −4 | **0** |

**Three of the four are within 31 Hz, two within 7, and they emit nothing
anyway.** So what refuses them is not acquisition — it is the emission floor,
which this unit is forbidden to move and rightly so. Unit 1.11.29 measured those
same three at window ratios of 0.44 to 0.90 against a floor of 1.40.

**Tasks 3, 4, 5 and 7 are aimed at a channel that is not leaking**, and task 6's
first acceptance line is already satisfied. Building a two-threshold hold and
calibrating an acquisition floor against a two-recording corpus, to cure junk the
measured chain does not produce, is work whose result nobody could interpret.

### Task 1 — done, and it is the most useful thing here

**The engine baseline is measured rather than expected for the first time in two
units: 28 failing of 1852, and the failing set is byte-identical by name to the
stable list.** Nothing outside the known 28 is red, so this unit began on a known
tree.

**The capture inventory the order asked for is written to
`CAPTURE_INVENTORY.md`**: 12 recordings hold an adjudicated station, 19 hold a
station nobody has ruled on, and **2 hold nothing**.

**Two is the whole third list, and that is a finding rather than an omission.**
The order asks for the acquisition floor to be measured against every capture
holding no adjudicated station, on the grounds that everything since has rested
on the 2026-08-20 pair. **The grounds are right and the corpus does not exist.**
Deciding a recording holds nothing is adjudication and it is Tim's; doing it from
the decoder's own output would be circular, because the four captures he can hear
read as runs of `E` and `I` too. **Three recordings were left unclassified for
exactly that reason rather than guessed into the empty list.**

### Task 2 — built, and it moved two of four

Where nothing has ever been confirmed, the tracker now commits to the loudest bin
in the band rather than to whatever the fine bank is centred on. `ToneHz` gained
a middle rung: keying still wins, the chosen bin sits under it, the bank centre
stays the bottom.

**The choice is recorded as a choice.** `CwPitchChoice` names four provenances —
not chosen, keying, strongest bin, operator assertion — and `HasMeasuredPitch`
keeps its old meaning, because a loud bin is where to point the filter and is not
evidence that anybody is sending (§0.0). The report carries it, **which is task
8's ask arriving early because the diagnosis needed it.**

**The acceptance is 2 of 4, not 4 of 4, and the mechanism is not what fell
short:**

- `cw-2026-08-25-012823` confirms **keying at 450 Hz** and emits 41 characters
  while the station sits at 500. Keying is still the chooser there, because a
  confirmed pitch outranks a chosen one and HM-DEC-127 protects it.
- `cw-2026-08-22-014308` is chosen by the strongest bin at **575**, one coarse
  bin below the 600 unit 1.11.29 measured over a twelve-second tail. The survey's
  strongest is taken over its own three-second rolling history, so the two
  numbers are measurements of different things — worth knowing before anybody
  treats 1.11.29's table as what the tracker sees.

**An inversion letting the strongest bin outrank a confirmed keying candidate was
written, measured, found inert, and removed** rather than left looking live.
Making it bite means re-ordering acquisition against HM-DEC-127, which is a
ruling and not a session's to take.

## 2. What the owner should expect

**A station he can hear still does not reach the decoder without him pressing
anything.** Two of the four are now pointed within 25 Hz and all four still emit
nothing or nonsense. **This unit did not move that.**

**A dead frequency does stay quiet — and it did before this unit too.** Both
recordings holding nothing emit zero characters through the real decoder. **If he
is watching an empty frequency fill with `E space E space I` tonight, nothing in
this repository reproduces it**, and closing that gap is the single most useful
thing the next unit could do. Section 4 asks for the capture.

**What will look wrong and is not:** the capture sheet can now say
`StrongestBin` where the pitch was never measured. That is the honest answer and
it is new; it does not mean a station was found.

| | baseline | end |
|---|---|---|
| engine | 28 of 1852, byte-identical by name | **the 28, plus one timing intermittent** |
| app | 509 of 509 | **not re-run — no app file was changed** |

**One test moved and it is not this unit's:**
`TheStateMonitorDoesNotHoldUpADisconnect`, which **passes three runs out of three
in isolation**. It is a rig-disconnect timing test and the three files this unit
changed are the tracker's acquisition rung, a new enum and one field on the
report — nothing on that path. It is one of the seven intermittents the order
says to diff and not chase, and this is the diff.

**The run it was found in was killed before it printed a total**, so the count
above is the failing set by name and not a total. A second full run was started
after the report was written; if it disagrees, the disagreement is the answer and
not this paragraph.

No decoder file and no app file was touched.

## 3. What you should see

**The two numbers section 3 was told to lead with.**

**The pitch chosen on each of the four**, against 500, 607, 606 and 403.5:
**450.0, 600.0, 575.0, 400.0** — errors of −50, −7, −31 and −4. **Two of four
inside 25 Hz**, against the 4 of 4 the order set.

**Characters emitted from audio holding no station: nought.** Both recordings,
before this unit and after. That is the whole empty corpus that exists.

**Nothing else changed on screen.** No app file was altered.

**Tasks 3, 4, 5, 6, 7 and 9 were not done.** Task 8 was done incidentally,
because the diagnosis needed the provenance on the sheet.

## 4. What's blocking us

**The junk the operator is watching is not reproduced by anything in this
repository, and until it is, no unit can fix it.**

Ruling asked for — this unblocks the most:

> **A capture is taken of the empty frequency that is filling with characters.**
> Every recording in the tree that a record says holds nothing emits **zero**
> characters through the real decoder, before and after this unit. The 93- and
> 91-character figures that motivated work instruction 033 came from
> `CwPitchRanking` sweeping the coarse bank offline — a component built
> disconnected, which the application does not run.
>
> **So either the fault lives in a state no capture holds** — a tracker that has
> confirmed and lost a station, a QSY, a long session — **or it is on a band
> quieter or noisier than the two recordings of 2026-08-20.** Both are testable
> and neither can be guessed at.
>
> **What was rejected:** building the channel hold anyway. It is a sound
> mechanism and I could not have told whether it worked, because there is no
> audio in the tree on which the current code misbehaves.

---

**The empty-capture corpus is two recordings and cannot honestly be widened.**

Task 5 asks for an acquisition floor measured against every capture holding no
adjudicated station. **That corpus does not exist, and building it is
adjudication.** Synthesized noise is known-empty by construction and is a
legitimate lower bound, but it is weaker evidence than a recording (HM-DEC-091):
real band noise carries carriers and splatter that white noise does not.

*Not proposed, because it needs a ruling:* whether Tim adjudicates a handful of
the nineteen unadjudicated recordings as holding nothing, which would give the
floor something to be measured against.

---

**Letting the strongest bin outrank a confirmed keying candidate is a ruling.**

Tim's ruling demotes keying from chooser to check. On `cw-2026-08-25-012823`
keying confirms at 450 and emits 41 characters of junk while the station is at
500 — **exactly the case the demotion is aimed at** — and the demotion does not
reach it, because a confirmed pitch outranks a chosen one.

*Not proposed:* inverting that priority at acquisition. It is one condition, it
was written and measured inert where it sits, and making it bite means deciding
how the amendment interacts with HM-DEC-127. That is the interaction §12.1 keeps
away from a session.

### Asks still outstanding

Carried forward verbatim per HM-DEC-139 and HM-DEC-140. **Twenty-seven inbound
after this unit. The oldest is open since 2026-08-14.**

1. **The sweep's `invented` column counts substitutions, not invented
   characters.**
2. **Whether the refill guard should apply to the first fill at all.**
3. **`ANNUNCIATOR.md` renamed `PHASE` to `TASK` while HM-DEC-150 makes `PHASE`
   match the version's minor.**
4. **`DECISIONS.md` has no record for HM-DEC-096–133, 136, 141 or 150** — and
   HM-DEC-095, 120, 125 and 127 are all inside it. **This unit amended one of
   them from an index row alone.**
5. **The integrator width** — settled at 45 Hz, with the sharp-peak caveat.
6. **A boxcar's nulls made two of five swept offsets pathological best cases.**
7. **Two stations closer than 125 Hz are not named** — the operator's own item
   five, still not attempted.
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
    (1.11.28) — measured on three instances.
19. **The scanner and the calling cycle are attached to the rig with no
    control** (1.11.28), and one of them transmits.
20. **Thirteen dead `DataTemplate` blocks nothing can distinguish from live
    ones** (1.11.28).
21. **Whether every constructed view model should be reachable from a binding**
    (1.11.28).
22. **`013347` returns a likelihood ratio of 17.2 million**, with `001520`'s
    quadrillions. Parked, raised once.
23. **HM-DEC-095 is what stands between Hamlet and these four stations**
    (1.11.29) — **amended by Tim 2026-08-27 and implemented this unit; it moved
    two of four, and the fourth needs the HM-DEC-127 interaction ruled.**
24. **HM-DEC-120's floor is calibrated for one look** (1.11.29) — **now ruled: a
    separate acquisition floor. Not built, because there is nothing to measure it
    against.**
25. **`CwPitchRanking` is in the tree and called by nothing** (1.11.29) — **task
    3 was to decide its fate and task 3 was not reached. It stays.**
26. **No capture reproduces the junk the operator is watching**, above.
27. **The empty-capture corpus cannot honestly be widened**, above.

New this unit: **the missing capture**, above; **the empty corpus**, above; **the
HM-DEC-127 interaction**, above.

Closed this unit: **the engine baseline** — 28 of 1852, measured and
byte-identical by name, after two units of expectation. **What each recording
holds** — written to `CAPTURE_INVENTORY.md`. **Whether the application puts junk
on an empty band** — it does not, on all the empty audio that exists.

Still open: **the lock's mixed help**; **three fixtures at accepted cost**; **an
unmeasured pitch costs `N4L`**; **the six-hertz window disagreement**;
**`CHANGELOG.md` at 1.9.0 against 1.11.30**; **the squelch has no axis**; **the
three morning captures of 2026-08-26**; **seven timing intermittents**.
