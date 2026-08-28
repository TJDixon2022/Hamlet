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

# Work instruction 027 — the phantom characters

**ISSUED: 2026-08-27. A fresh order, not an amendment.**

**Four tasks; task 4 is the drop.** The operator has been chasing phantom
characters for weeks. **This unit ends them, and the evidence to do it was
already in the sheets.**

## Why this unit exists

**The unit's number: minus sixty-eight thousand, five hundred and sixty-two.**

`cw-2026-08-28-005158.txt`, from tonight, reads:

```
reading  17 WPM won out of 8 to 40, -68562.4 better than silence per hop
         against a gate of 1
inThis   69 characters emitted, 36 unsure
```

**A window scoring minus sixty-eight thousand against a gate of one emitted
sixty-nine characters.** Whatever the gate is doing, it is not refusing.

**And the pitch was nowhere near the station.** Measured outside Hamlet on the
three captures of 7.068 MHz, the station sits at **599.3 Hz** in all three.
Hamlet tracked **750, 775 and 775 Hz**, where the spectrum measures **58 to 75
decibels below** the real signal. It read 197 characters out of that.

**Everything needed to refuse was already in the sheet and none of it was
used:**

| the sheet said | on `005158` | on `005218` | on `005243` |
|---|---|---|---|
| `reading` … against a gate of 1 | **−68562.4** | 86.7 | 158.4 |
| `tonePeak`, held and decaying | 65.3 | **45.8** | **20.3** |
| `competing`, loudest in band | **575 Hz** | **575 Hz** | **575 Hz** |
| `keying`, independent sweep | **600 Hz** | **625 Hz** | **625 Hz** |
| `toneHz`, what was decoded | 750 | 775 | 775 |
| `decoderWpm` | **withdrawn** | **withdrawn** | 17 |

**Two instruments that share nothing with the tracker both pointed at 575–625
while the tracker sat at 775.** The held peak decayed from 65 to 20 dB, meaning
nothing refreshed it for the whole stretch. The clock was withdrawn as
un-acquired. **Characters kept coming.**

**And one capture is worse, because the sheet names the fault itself.**
`cw-2026-08-28-005051.txt`:

```
unkeyed  YES  (252 characters reached the screen from a pitch chosen by the
         loudest bin in the band, with no keying admitted here. This is the
         sheet to send back)
```

**Hamlet knew it had admitted no keying, knew the pitch was a fallback, and
printed 252 characters anyway.**

**Set against a working case from the same evening, thirty minutes earlier**,
so this is not a claim that the decoder cannot read: `cw-2026-08-28-004844`,
with the attenuator in and no overload, read `TUES AUG 25`, `NR 230 CK 7`,
`WED AUG 26`, `W7GB QRU 88`, `BRUCE`, `<BT>`, `<AR>` — **confirmed
independently from the audio at 429.2 Hz, around 85–90 % correct on the settled
stretch.** The decoder works when it is pointed at a station. **The phantoms
come from printing when it is not.**

## Verify this instruction against the tree

**Nothing here describes the tree.** Check every claim and report mismatches,
including where the work succeeded anyway.

**Seven captures from tonight are in this zip** at
`tests/fixtures/cw/captured/unadjudicated/`, with sidecars and
`cases-2026-08-27.txt`. Three are the good case, four are the phantom case.

**Expected state: 28 failing of 1841 in the engine as the stable set; 503 of
503 in the app. Seven timing intermittents.** Do not chase any; diff which
tests moved and never trust a total.

**`AppSettings.UseJointDecoder` ships false and stays false.**
**`AppSettings.ShowKeyingSweep` ships false and stays false.**

**`DECISIONS.md` still has no record for HM-DEC-096–133, 136, 141, 150, nor
Tim's rulings of 2026-08-25/26/27.** **`CLAUDE_CODE.md` is at version 1.4.**

## Rulings in force

**Tim's ruling, 2026-08-27, by adopting this unit (flagged for veto in the
delivery):**

> **Hamlet does not print letters it cannot stand behind.** Where the survey has
> admitted no keying, where the window's own score is below the gate, or where
> the clock has been withdrawn as un-acquired, **the terminal shows nothing or
> shows blocks — never letters.** The sheets already carry `unkeyed YES`,
> `toneHz NOT MEASURED`, a negative `reading`, and `decoderWpm withdrawn`.
> **Every one of those conditions was known at the moment the characters were
> printed, and every one was ignored.**

**HM-DEC-120's property is extended, not traded.** It has always meant nothing
is emitted on audio holding no signal. **It now also means nothing is emitted
from a pitch nobody judged to be a station.** Both silence controls stay silent;
this is stricter, never looser.

**HM-DEC-009 is the principle**: a value that could not be measured says so.
A pitch that was not measured must not produce letters that imply it was.

**Rejected already, do not revisit:** raising the gate's value (it is not being
enforced — find out why before touching the number); the six admission axis
families; the joint decoder's word gaps; the operator's assertion path.

## Status cadence

Per §4.5: after each task, before the next, update `PROJECT_STATUS.md` —
`STATE`, `TASK: n of m`, `BALL`, `UPDATED` from the clock, `NOTE` saying what is
moving. Same every ten minutes while a task runs.

## The tasks

### Task 1 — bank tonight, and find out why the gate did not fire

Commit the seven captures and `cases-2026-08-27.txt`. Floor them at what they
read today — **including the phantoms, so the reduction is visible as a
reduction.**

**Then answer one question before changing anything: how did
`cw-2026-08-28-005158` emit sixty-nine characters with a window score of
−68562.4 against a gate of 1?** Trace it with file and line. Possibilities
worth checking first: the gate is applied to a different quantity than the one
printed; it is applied per window but characters settle from a window that
already passed; the score is computed after emission; or the streaming path
does not consult it at all.

**Report the mechanism. It decides the shape of task 2.**

Build and run; record the baseline by diffing which tests fail.

### Task 2 — the three refusals

Under the ruling, and each independently:

1. **No keying admitted → no letters.** The condition behind `unkeyed YES` and
   `toneHz NOT MEASURED` already exists; it must reach the emit decision.
2. **Window score below the gate → no letters**, per whatever task 1 found.
3. **Clock withdrawn → no letters** while it is withdrawn.

**Blocks or nothing, never letters** (§0.0: a marked unknown outranks a wrong
letter).

**Acceptance:**

- `005051` emits **no letters** — it emitted 252;
- `005158`, `005218`, `005243` emit **no letters** — they emitted 197 between
  them;
- **`004844`, `004902`, `004915` are unchanged, character for character** —
  the good case must not pay for this;
- all twelve adjudicated anchors green; every floor held; both silence controls
  silent; chunk invariance intact.

**If any refusal costs an anchor, ship the other two and report which and
why.**

### Task 3 — the terminal says why it is quiet

When a refusal in task 2 is holding, **the terminal says so in the plain
language it already uses elsewhere** — that Hamlet can hear something and
cannot make letters of it, or that nothing here has been judged a station.
**One line, in the existing prose area, using the existing wording style. No
new panel, no new control, nothing else on the screen changes.**

The operator's complaint is not that the screen is empty. It is that the screen
lies. **A quiet screen that explains itself is the deliverable.**

### Task 4 — the tracker is at 775 while two instruments say 600 *(the drop)*

**Measure only.** On `005158`, `005218` and `005243`, report why the tracker
holds 750–775 Hz when the independent sweep says 600–625, `competing` says the
loudest thing is 575, and the audio measures the station at **599.3 Hz** in all
three. Include what the held peak's decay from 65.3 to 20.3 dB means about how
long it had been since anything refreshed it.

**No change.** Dropped whole if time runs out, and the report says so.

## Parked — do not touch, do not raise

Admission and the six axis families; the joint decoder; the constrained margin;
the meter's rebuild; the integrator width; the whole-file second pass;
`001520`'s quadrillions and `013347`'s 17.2 million; the reference and port
difference; the short-character bias; the Avalonia offset; `CHANGELOG.md`; the
seven intermittents; HM-OPEN-057; HM-OPEN-059; **the layout work of
instruction 026**; **the panel beyond task 3's one line.**

A parked item that blocks a task is raised once, and says it was parked.

## What not to do

Standing prohibitions are `CLAUDE.md`'s and are not retyped. Unit-specific:

- **Do not change the gate's value.** Find out why it is not enforced.
- **Do not let the good captures lose a character.**
- **Do not add a panel or a control.** Task 3 is one line of existing prose.
- **Do not touch admission, the tracker's rules, or the joint decoder.**
- **Do not trade the silence property** — this unit only tightens it.
- **Floors only rise; anchors stay green; chunk invariance holds.**

## Committing, pushing, reporting

Commit and push each task before starting the next; name the branch; a refused
push is reported as refused, with the reason.

Report per `CLAUDE_CODE.md` §8 — read the file's own section count — to
`output.md` at the repository root, overwritten and printed.

**Section 3 leads with two numbers: characters emitted by the four phantom
captures, before and after; and characters emitted by the three good captures,
before and after.** Section 2 says plainly what the operator sees at the radio
when Hamlet has nothing it can stand behind.

### Asks still outstanding

Carried forward verbatim per HM-DEC-139 and HM-DEC-140. **Twenty inbound. The
oldest is open since 2026-08-14.**

1. **The sweep's `invented` column counts substitutions, not invented
   characters.**
2. **Whether the refill guard should apply to the first fill at all.**
3. **`ANNUNCIATOR.md` renamed `PHASE` to `TASK` while HM-DEC-150 makes `PHASE`
   match the version's minor.**
4. **`DECISIONS.md` has no record for HM-DEC-096–133, 136, 141 or 150 — nor for
   Tim's rulings of 2026-08-25/26/27, including the one this unit acts under.**
5. **The tone tracker** — six axis families measured; task 4 measures a live
   case where it sat 175 Hz off a station two other instruments found.
6. **The integrator width** — settled at 45 Hz, with the sharp-peak caveat.
7. **The guard's gap is two to one**, calibrated on two empty captures.
8. **A boxcar's nulls made two of five swept offsets pathological best cases.**
9. **Two stations closer than 125 Hz are not named** — but **`competing` now
   reports the loudest thing and its duty, and on four captures tonight it was
   right where the tracker was wrong.**
10. **The keying meter** — behind its setting, off, and **on three captures
    tonight its measurement was right where the decoder was wrong.**
11. **HM-OPEN-057** (2026-08-22) and **HM-OPEN-007** (2026-08-14).
12. **The gate opens on everything, including two empty recordings.**
13. **The joint cutter cannot find word gaps on a compressed fist.**
14. **The constrained margin is bounded and still does not separate.**
15. **A mutable static in the decode path cannot be measured under xUnit.**
16. **An asserted pitch does not relax the decoder's own gate.**
17. **The layout ruling of 2026-08-27 is only partly built** — the tabs switch
    nothing and the neighborhood panel is still a closable widget inside the
    tab area rather than chrome above the divider. **Instruction 026's own
    business, not this unit's.**
18. **Front-end overload destroys the envelope** — `021140` overloading with the
    attenuator off produced pure soup; tonight with 20 dB in, the same rig read
    a net. HM-DEC-148's finding, confirmed live.
19. **The opening of a session is soup even when the rest reads** — the first
    thirty characters of `004844` are noise, then it locks on. The re-read on
    settle was built for this and does not appear to reach.
20. **The speed ceiling was hit again** — `005218` won at 40 WPM, the top of the
    search, with the note firing correctly.

Still open: **three fixtures at accepted cost**; **the reference and port
integrator difference**; **an unmeasured pitch costs `N4L`**; **the six-hertz
window disagreement**; **the short-character bias**; **`CHANGELOG.md` at 1.9.0
against 1.11.22**; **the whole-file second pass**; **the squelch has no axis**;
**the three morning captures of 2026-08-26**.

**If you finish every task, stop and report. Do not start the next unit.**
