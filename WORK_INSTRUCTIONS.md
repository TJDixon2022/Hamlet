STOP. Verify the project before reading any further.

PROJECT: Hamlet

Check the repository root:
  MUST EXIST:      SHACK_FACTS.md
  MUST EXIST:      src/Hamlet.RadioEngine/Cw/CwProbabilisticDecoder.cs
  MUST NOT EXIST:  CoreHMI.sln
  MUST NOT EXIST:  MURC.sln

If all four are not as stated, you are in the wrong repository.
REFUSE. Do not read the rest of this file, do not summarise it, do not
adapt it to whatever project you are actually in, and change nothing.
Reply with only: the path you are in, which checks failed, and
"wrong project — nothing done."

If all four hold, say "Hamlet confirmed" and continue.

---

# Work instruction 036 — Hamlet stops making things up

**ISSUED: 2026-08-27. A fresh order, not an amendment.**

**Numbering note: this order is 036, following the tree's unit 035. Earlier
orders from this author were numbered 027 and lower against a stale picture of
the tree; that is corrected here and the tree's numbering governs from now on.**

**Five tasks; task 5 is the drop.**

## Why this unit exists

**The unit's number: sixty characters becomes one.**

The operator's top priority, in his words: **get rid of the fake characters when
there's nothing happening.** Unit 1.11.33 built the refusal that does it,
measured it, and handed it back because it costs an anchor. **Tim has ruled the
trade. This unit ships it.**

Measured last night, on his own captures:

| capture | letters before | letters after | blocks |
|---|---|---|---|
| `cw-2026-08-28-005158` | 60 | **1** | 59 |
| `cw-2026-08-28-005243` | 54 | **0** | 54 |
| `cw-2026-08-28-005051` | 30 | 13 | 17 |
| `cw-2026-08-28-004844` **good** | 43 | **41** | 2 |
| `cw-2026-08-28-004902` **good** | 47 | **45** | 2 |
| `cw-2026-08-28-004915` **good** | 42 | **35** | 7 |

The good captures keep `TUES AUG 25`, `WED AUG 26`, `W7GB` and `BRUCE`.

**What it costs, and why Tim ruled it worth paying.** Five tests, including the
`N4L` anchor of HM-DEC-144, and part of six captures' text. `CwDecoder`'s own
comment, written before any of this, records the reason the anchor is not what it
appears to be:

> that recording's fallback bank centre is 500.0 and its station sits at
> **500.09**, so the callsign was only ever read because an unmeasured number
> happened to land on it.

**`N4L` is a correct reading obtained exactly the way the phantoms are
obtained.** An anchor that passes by luck has been certifying a mechanism that
produces junk everywhere else.

**What this does not fix, stated plainly so the screen does not surprise
anyone.** `cw-2026-08-28-005218` loses only two characters, because on that
capture the survey *did* admit a pitch — at 775 Hz, while the station measures
at **599.3 Hz**. **Junk from a wrongly admitted pitch survives this unit.**
Tasks 3 and 4 attack that residue.

## Verify this instruction against the tree

**Nothing here describes the tree.** Check every claim and report mismatches,
including where the work succeeded anyway. **This author's picture of the tree
has been stale by several units; unit 1.11.33 caught it. Trust the tree over
this order everywhere they differ, and list the differences.**

**Expected state, from unit 1.11.33: 28 failing of 1852 in the engine; 509 in
the app; `CLAUDE_CODE.md` at version 1.6 with twelve report sections.** **Read
the file's own section count and follow it**, not this order's assumptions.

**Seven captures of 2026-08-28 should already be in the tree** from unit
1.11.33's task 1. Confirm; if any are missing, say which.

**`AppSettings.UseJointDecoder` and `ShowKeyingSweep` both ship false and stay
false.**

## Rulings in force

**Tim's ruling, 2026-08-27, on the trade unit 1.11.33 handed back:**

> **Ship the refusal. Hamlet stops printing letters from a pitch the survey
> admitted no keying at, and `N4L` becomes blocks.** The phantoms are the
> priority. `N4L` was never read from a measurement; it returns when admission
> can find that station honestly.

**Rejected with it, and not to be revisited:** the clock-withdrawn refusal —
measured at 26, 38 and 25 characters off the three good captures, which is the
good case paying for the bad; raising the gate — unit 1.11.33 proved the gate
fires correctly on all seven captures, and `−68562.4` was a one-window snapshot
printed beside a thirty-second total, not a window that emitted.

**HM-DEC-120's property is tightened, never loosened.** It has meant nothing is
emitted on audio holding no signal; it now also means **no letters from a pitch
nobody judged to be a station.** Both silence controls stay silent.

**HM-DEC-009 is the principle**: a value that could not be measured says so.

**HM-DEC-144 is amended, not deleted** — task 2.

## Status cadence

Per §4.5: after each task, before the next, update `PROJECT_STATUS.md` —
`STATE`, `TASK: n of m`, `BALL`, `UPDATED` from the clock, `NOTE` saying what is
moving. Same every ten minutes while a task runs.

## The tasks

### Task 1 — ship the refusal

Restore unit 1.11.33's task 2 refusal — **no keying admitted, no letters** —
wired at the emit seam as **blocks rather than deletions**, so no character
position is lost and only the assertion goes.

**Report the cost per test before declaring the task done**, not after: name
every test that goes red and what it loses. Tim ruled the trade knowing it was
five tests and part of six captures; **if the true cost is materially larger
than that, stop and report rather than shipping.**

**Acceptance:** the numbers in the table above reproduce; both silence controls
silent; chunk invariance intact; **no capture emits a letter from an unmeasured
pitch anywhere in the corpus** — asserted by test, across every capture, not
only the seven.

Build and run; record the baseline by diffing which tests fail.

### Task 2 — re-express the anchors the refusal breaks

An anchor that goes red must not simply be deleted; the corpus loses its memory
that way.

For **`N4L` on `cw-2026-08-17-134712`**, and for each of the other four:
**re-express the test as what the recording now honestly produces**, with the
reason in the test itself — that the callsign was read from a bank centre of
500.0 against a station at 500.09, that it is retired as a reading anchor, and
**that it returns as an anchor when admission finds that station honestly.**

**Write the amendment to HM-DEC-144 into the report's decision section for Tim
to enter** — the session does not mint decision ids.

**Any capture whose text is partly lost keeps a floor at what it now reads**, so
the loss is recorded and can only be recovered, never repeated.

### Task 3 — the residue, measured

`005218` keeps 40 letters because the survey admitted 775 Hz while the station
measures 599.3 Hz — confirmed outside Hamlet, on the audio, in all three of the
7.068 MHz captures.

**Measure, across every capture in the corpus: how often is a pitch admitted
that is more than 25 Hz from the strongest keyed thing in the band?** Report per
capture: the admitted pitch, the strongest keyed bin, the difference, and the
characters emitted at the admitted pitch.

**This is the size of what the refusal does not reach.** Measure only; change
nothing.

### Task 4 — where the tracker was, and why

Unit 1.11.33's task 4 was the drop and was not reached; its task 1 leaves a
running start — all seven captures already stream through a real decoder with
per-capture state reported.

On `005158`, `005218` and `005243`: **why does the tracker hold 750–775 Hz when
the independent keying sweep says 600–625, `competing` says the loudest thing is
575, and the audio measures the station at 599.3?** Include what the held peak's
decay — 65.3, then 45.8, then 20.3 dB — says about how long it had been since
anything refreshed it.

**Measure and report with file and line. Change nothing.** The next unit is
built from this.

### Task 5 — the sheet's two spans *(the drop candidate)*

`reading … −68562.4 better than silence per hop` is `_probabilistic.Last`, one
window at the instant of the press. `inThis 69 characters emitted` covers thirty
seconds. **Three lines apart on the same sheet, with nothing saying they are
about different spans** — and this author wrote a whole unit from reading them
together.

**Make `reading` say which window it is about**, the way `tonePeak` and the
running totals were made to after HM-DEC-091. **The sheet's wording is Tim's**
(§12.1), so: implement the span label, and **put the exact new wording in the
report for his approval** rather than treating it as settled.

**Dropped whole if time runs out, and the report says so.**

## Parked — do not touch, do not raise

The joint decoder; the constrained margin; the meter's rebuild; the integrator
width; the whole-file second pass; `001520`'s quadrillions and `013347`'s 17.2
million; the reference and port difference; the short-character bias;
`CHANGELOG.md`; the intermittents; HM-OPEN-057; HM-OPEN-059; **the layout work
of instruction 026 and everything in the 1.11.25–1.11.32 stream** — the
phrasebook, the recent-places row, the owned-property list, the dead
`DataTemplate` blocks, the scanner and calling cycle. **This unit is the
phantoms and nothing else.**

A parked item that blocks a task is raised once, and says it was parked.

## What not to do

Standing prohibitions are `CLAUDE.md`'s and are not retyped. Unit-specific:

- **Do not ship the clock-withdrawn refusal.** Measured dead.
- **Do not raise or lower the gate.** It works.
- **Do not delete an anchor.** Re-express it with its reason.
- **Do not let a good capture lose more than unit 1.11.33 measured** — 2, 2 and
  7 blocks on `004844`, `004902`, `004915`.
- **Do not change the tracker, admission, or the panel** — tasks 3 and 4
  measure; they do not fix.
- **Do not trade the silence property.** This unit only tightens it.

## Committing, pushing, reporting

Commit and push each task before starting the next; name the branch; a refused
push is reported as refused, with the reason.

Report per `CLAUDE_CODE.md` §8 — **read the file's own section count; it is at
version 1.6 with twelve sections** — to `output.md` at the repository root,
overwritten and printed.

**The section that says what the owner should expect leads with this: on a
frequency where nothing is happening, the terminal now shows blocks rather than
letters, and the count of letters lost on the three good captures is two, two
and seven.** **The section that reports measurements leads with task 3's
number — how much junk the refusal does not reach.**

### Asks still outstanding

Carried forward per HM-DEC-139 and HM-DEC-140, from unit 1.11.33's list of
thirty, with this unit's changes marked.

1. **The sweep's `invented` column counts substitutions, not invented
   characters.**
2. **Whether the refill guard should apply to the first fill at all.**
3. **`ANNUNCIATOR.md` renamed `PHASE` to `TASK` while HM-DEC-150 makes `PHASE`
   match the version's minor.**
4. **`DECISIONS.md` has no record for HM-DEC-096–133, 136, 141 or 150** — and
   now also needs **HM-DEC-144's amendment** from task 2.
5. **The integrator width** — settled at 45 Hz, with the sharp-peak caveat.
6. **A boxcar's nulls made two of five swept offsets pathological best cases.**
7. **Two stations closer than 125 Hz are not named.**
8. **The keying meter** — on four captures of 2026-08-28 its measurement was
   right where the decoder was wrong.
9. **HM-OPEN-057** (2026-08-22) and **HM-OPEN-007** (2026-08-14).
10. **The joint cutter cannot find word gaps on a compressed fist.**
11. **The constrained margin is bounded and still does not separate.**
12. **Four fixtures are absent and five acceptance lines were unmeasurable.**
13. **HM-DEC-086's supersession needs a record.**
14. **The phrasebook's arrival and the absent-widget news are gone.**
15. **The recent-places row has no home**, three options costed.
16. **The owned-property list has no enforcement of staying current.**
17. **A test resolved an ambiguous control by accident.**
18. **Nothing checks that deleting a surface is not deleting a capability.**
19. **The scanner and the calling cycle are attached to the rig with no
    control**, and one of them transmits.
20. **Thirteen dead `DataTemplate` blocks nothing can distinguish from live
    ones.**
21. **Whether every constructed view model should be reachable from a
    binding.**
22. **`013347` returns a likelihood ratio of 17.2 million**, with `001520`'s
    quadrillions.
23. **The empty corpus is blocked by the keying meter, with proof.**
24. **An offline sweep over a bank is not what the application runs.**
25. **The two captures have no ground truth and cannot be told from an empty
    band.**
26. **The emission floor was calibrated against a model that over-credits
    keying.**
27. **`cwdecoder.py` does not read what two orders say it reads.**
28. **The sheet prints a last-window score beside a whole-recording count** —
    **task 5 acts on it, and the wording is Tim's.**
29. **A pitch can be admitted far from the strongest keyed thing in the band** —
    **new; task 3 measures it, and it is what the refusal does not reach.**
30. **Why the tracker holds 750–775 while two instruments say 600–625** —
    **task 4 measures it.**

**Closed by this unit if it lands: `N4L` against the phantoms** — ruled and
shipped.

**If you finish every task, stop and report. Do not start the next unit.**
