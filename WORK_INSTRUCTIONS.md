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

# Work instruction 031 — give the operator his send button back

**ISSUED: 2026-08-27. A fresh order, not an amendment.**

**Five tasks; task 5 is the drop. This unit restores a capability that this
project removed by mistake.**

## Why this unit exists

**The unit's number: hundreds of transmissions, and no button.**

**Tim has sent with Hamlet hundreds of times.** The send button worked. It keyed
the radio. It has been in the application for dozens of builds.

**It was removed by a chain of orders written from a wrong premise, all mine.**
Instructions 026, 027 and 028 each carried *"do not wire Send to the
transmitter"*, citing §0.2 and HM-DEC-098 as though the interlock work were still
ahead of the project — when it had been done, ruled and used. Unit 1.11.25 then
deleted the Send widget from the catalogue on that same order, and preserved its
own description in `ABANDONED_WIDGETS.md`:
**"What you could say next, written out in full, with a button that sends it."**

**The button was in that sentence and the sentence was the record.** Nothing in
those three orders checked whether the capability already existed before
forbidding it.

**What is in the tree today** is what unit 1.11.24 shipped: a title, a text box,
four buttons reading CQ, RST, 73 and Clear, and a paragraph saying nothing leaves
the radio. **The transmit path itself is almost certainly still present and
still tested** — unit 1.11.25 reported the engine behind every abandoned widget
compiles and runs, unreachable only from the screen.

**This is reconnection, not construction.**

## Verify this instruction against the tree

**Nothing here describes the tree.** Check every claim against the files and
report any mismatch, including where the work succeeded anyway. **This order was
written from an operator's memory and from a deleted widget's description, not
from the code. Task 1 exists because of that.**

**Expected state: 28 failing of 1841 in the engine as the stable set; 507 of 507
in the app.** Seven timing intermittents exist. Do not chase any; diff which
tests moved and never trust a total.

**The view-test rule is in force** (Tim's ruling, 2026-08-27, unit 1.11.27): a
view-level test acts through the control — it presses the button, it does not
set the property the button would have set. **The guard's named-property list
gains any property this unit gives a control**, and the report says which.

**`CLAUDE_CODE.md` is at version 1.6.** Read its own section count.

**`DECISIONS.md` still has no record for HM-DEC-096–133, 136, 141, 150, nor
Tim's rulings of 2026-08-25/26/27.** **HM-DEC-098 is inside that unreadable
range and this unit turns on it** — see task 1.

## Rulings in force

**Tim's ruling, 2026-08-27, in his words:** *"I've sent with it hundreds of
times. It worked great."* **The send button is restored.** It keys the radio, as
it did before this project removed it.

**HM-DEC-098 and §0.2 are not being overridden — they are being read
correctly.** They govern *bringing a transmit path into existence*: every
interlock watched firing into a dummy load, including the link pulled mid-cycle.
**That work was done and ruled before these three orders were written.** A path
already built, ruled and used for hundreds of transmissions is not brought into
existence again by reconnecting a button to it.

**But this unit does not assume that on my word.** Task 1 finds the ruling and
the interlocks in the tree and reports them. **If the transmit path is not there,
or is there without a ruling authorising it, this unit stops at task 1 and says
so** — see below.

**Untouched:** the workspace boundary and tab strip as unit 1.11.26 left them;
the band plan, neighborhood, radio and divider; every decoder behaviour; the
view-test rule and the resource check unit 1.11.27 added.

**Rejected already, do not revisit:** bringing back the canvas, the tray or the
preset bar; placeholder text in Digital or Voice; asserting a control's own
`IsVisible` where effective visibility is what the operator sees.

## Status cadence

Per §4.5: after each task, before the next, update `PROJECT_STATUS.md` —
`STATE`, `TASK: n of m`, `BALL`, `UPDATED` from the clock, `NOTE` saying what is
moving. Same every ten minutes while a task runs.

## The tasks

### Task 1 — find the transmit path and the ruling that authorised it

**Before changing anything**, report with file and line:

1. **The transmit path.** What keys the radio — the view model command, the
   service, the CAT or keying call at the bottom of it. Whether it compiles,
   whether it is tested, and what those tests assert.
2. **The interlocks.** What refuses to transmit and on what condition. **Name
   each one**, and say whether a test covers it.
3. **The ruling.** Find HM-DEC-098 and any decision authorising the transmit
   path, in `DECISIONS.md` or in `CLAUDE.md`'s index rows. **Transcribe what you
   find.** The full records for HM-DEC-096–133 are missing from this tree, so an
   index row may be all there is — say which you had.
4. **What the button used to be.** Search the history for the Send widget's own
   button and report what it called, so the reconnection restores the same route
   rather than inventing a new one.

**Then decide, and say which:**

- **The path exists, is tested, and a ruling authorises it** → tasks 2 to 4
  proceed.
- **The path exists but no ruling can be found** → **build tasks 2 and 3, and
  leave the button disabled with a line saying the ruling could not be located.**
  Report exactly what you searched. **Do not key the radio on an unlocatable
  ruling.**
- **The path is not there** → **stop. Report what is missing.** The unit becomes
  the interlock work, which is a different unit done at the rig.

Build and run; record the baseline by diffing which tests fail.

### Task 2 — the button

**A send button in the Send panel, wired to the transmit path task 1 found.**
Same route the old one used.

**It is a permanent part of the Send panel**, like Send and Receive are permanent
parts of the CW workspace — not a widget, not removable, not closable.

**Clear is an action, not a message, and is coloured differently from the
macros** — Tim's ruling of 2026-08-27. **Send and Clear sit at the top beside the
title**, so what transmits is visually separate from what composes.

### Task 3 — the panel says what it means

Replace the paragraph about nothing leaving the radio — which will no longer be
true — with **what each control actually does**, in the plain terms Tim asked
for:

- **CQ** — says you are looking for a conversation, and sends your callsign.
- **RST** — sends a signal report: how well you are hearing the other station.
- **73** — signs off. It means best wishes and it is how a contact ends.
- **Clear** — empties the line. Nothing is transmitted.
- **Send** — transmits what is on the line.

**Write them in the application's own voice**, as the neighborhood map's own
lines are written. The wording above is the meaning, not the copy.

### Task 4 — the interlocks are proven, not assumed

**Tests, acting through the button per the view-test rule**, that each interlock
task 1 named still refuses.

**Report every interlock and whether a test now covers it.** An interlock with no
test is named in section 4 as one, not quietly counted as covered.

**Nothing in this unit is verified by transmitting.** No rig is connected on the
development computer, and the tests assert the refusals, not the emission.
**Tim verifies at the rig.**

### Task 5 — what else the deletion took *(the drop candidate)*

`ABANDONED_WIDGETS.md` lists fifteen widgets deleted by unit 1.11.25 on my
orders. **The Send button was inside one of those descriptions and nobody noticed
until Tim did.**

**Read the other fourteen and report any that describe a working capability
rather than a display** — "Call CQ on a cycle" calls and listens between rounds;
the scanner works down the band and stops where somebody is calling. **Report
only. Restore nothing.** Which come back, and in what order, is Tim's.

**Dropped whole if time runs out, and the report says so.**

## Parked — do not touch, do not raise

Every decoder question: admission, the six axis families, the gate, the squelch,
the joint decoder, the constrained margin, the tracker, the meter, the integrator
width, the whole-file second pass, `001520`'s quadrillions, `013347`'s 17.2
million. Also: the recent-places row's home; HM-DEC-086's supersession record;
the owned-property list's maintenance; the ambiguous-control test class;
`CHANGELOG.md`; the seven intermittents; the Avalonia geometry offset;
HM-OPEN-057; HM-OPEN-059; **the band plan, the neighborhood map, the radio panel
and the divider.**

A parked item that blocks a task is raised once, and says it was parked.

## What not to do

Standing prohibitions are `CLAUDE.md`'s and are not retyped. Unit-specific:

- **Do not touch any decoder file.** The byte-identical failing set is the claim.
- **Do not key the radio from a test**, and do not add a code path that could.
- **Do not proceed past task 1 if the transmit path is absent.**
- **Do not enable the button if no ruling authorising the path can be located** —
  build it disabled and say so.
- **Do not restore any other widget.** Task 5 reports.
- **Do not set a view-model property in a view test where a control exists.**

## Committing, pushing, reporting

Commit and push each task before starting the next; name the branch; a refused
push is reported as refused, with the reason.

Report per `CLAUDE_CODE.md` §8 — read the file's own section count — to
`output.md` at the repository root, overwritten and printed.

**Section 2 leads with whether Tim has his send button back, and what he should
check at the rig before trusting it.** **Section 3 leads with task 1's findings:
the transmit path, every interlock, and the ruling — with file and line.**

### Asks still outstanding

Carried forward verbatim per HM-DEC-139 and HM-DEC-140. **Twenty-three inbound.
The oldest is open since 2026-08-14.**

1. **The sweep's `invented` column counts substitutions, not invented
   characters.**
2. **Whether the refill guard should apply to the first fill at all.**
3. **`ANNUNCIATOR.md` renamed `PHASE` to `TASK` while HM-DEC-150 makes `PHASE`
   match the version's minor.**
4. **`DECISIONS.md` has no record for HM-DEC-096–133, 136, 141 or 150** — and
   **HM-DEC-098, which authorises the transmit path this unit reconnects, is
   inside that range.** The cost of the missing records is now a capability
   nobody can look up.
5. **The tone tracker** — six axis families measured; the question is a design
   one.
6. **The integrator width** — settled at 45 Hz, with the sharp-peak caveat.
7. **The guard's gap is two to one**, calibrated on two empty captures.
8. **A boxcar's nulls made two of five swept offsets pathological best cases.**
9. **Two stations closer than 125 Hz are not named** — the operator's own item
   five, still not attempted.
10. **The keying meter** — its measurement found a station its verdict denied.
11. **HM-OPEN-057** (2026-08-22) and **HM-OPEN-007** (2026-08-14).
12. **The gate opens on everything, including two empty recordings** (1.11.18).
13. **The joint cutter cannot find word gaps on a compressed fist** (1.11.22) —
    the next decode question, still unruled.
14. **The constrained margin is bounded and still does not separate** (1.11.22).
15. **Four fixtures are absent and five acceptance lines were unmeasurable**
    (1.11.22).
16. **HM-DEC-086's supersession needs a record** (1.11.25).
17. **The phrasebook's arrival and the absent-widget news are gone** (1.11.25).
18. **Engine code behind the abandoned widgets is unreachable** (1.11.25) —
    **this unit reconnects one of them and reports which others matter.**
19. **The recent-places row has no home** (1.11.26), three options costed.
20. **The owned-property list has no enforcement of staying current** (1.11.27).
21. **A test resolved an ambiguous control by accident** (1.11.27).
22. **A deleted widget's description was the only record of a working
    capability**, and it took the operator to notice. **Nothing checks that a
    deletion is not removing something in use.**
23. **`013347` returns a likelihood ratio of 17.2 million**, with `001520`'s
    quadrillions. Parked, raised once.

Still open: **three fixtures at accepted cost**; **the reference and port
integrator difference**; **an unmeasured pitch costs `N4L`**; **the six-hertz
window disagreement**; **the short-character bias**; **the Avalonia geometry
offset**; **`CHANGELOG.md` at 1.9.0 against 1.11.27**; **the whole-file second
pass**; **the squelch has no axis**; **the three morning captures of
2026-08-26**.

**If you finish every task, stop and report. Do not start the next unit.**
