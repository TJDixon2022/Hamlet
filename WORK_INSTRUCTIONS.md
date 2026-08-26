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

# Work instruction 017 — the dimming, the plan in the tree, and the margin read

**ISSUED: 2026-08-26. Replaces instruction 016, which was never run, and the amended 015, which was. Per CLAUDE_CODE.md §9.6 this line is how a session tells a fresh order from an amendment; its absence from every prior order was the web session's standing omission and is corrected from this unit on.**

**Small unit. Three tasks; task 3 is the drop.** The band row shipped in unit
1.11.13 and is not here.

## Why this unit exists

**The unit's number: one hundred stale characters above three correct ones.**
The transcript still buries good copy — the night's own account is a screen
whose first hundred characters were soup decoded two minutes earlier, sitting
bright above three correctly-read callsign tokens. The separator-and-dim design
of unit 1.11.12 was defined against a squelch that measured out of existence;
that unit's report proposes the mechanism that needs no squelch, and **Tim has
ruled it by adopting this unit.**

Second: unit 1.11.13's report names the pattern behind four measured-and-not-
shipped features — every one proposed from a measurement taken through a
different instrument than the one it would run in, with the shack's plan never
in the tree to audit. **`BUILD_SESSION_2026-08-25.md` is in this zip.** Banking
it closes that gap for this plan and sets the precedent for the next one.

Third: unit 1.11.12 logs `marginLlr` — the candidate axis for the squelch's
successor — and nothing has read it yet. The drop task takes its first
distribution so tomorrow's bound comes from data.

## Verify this instruction against the tree

**Nothing here describes the tree.** Check every claim and report mismatches,
including where the work succeeded anyway.

**Known state after unit 1.11.13: 28 failing of 1831 in the engine, identical
set through two units; 497 of 497 in the app.** Three accepted-cost silence
fixtures; four known intermittents. Twelve success tests green; anchors govern
where they cover; floors elsewhere; element floors everywhere; silence
absolute; chunk invariance corpus-wide. **All of that must be true at the end
of this unit too — nothing here touches the decoder.**

**`BUILD_SESSION_2026-08-25.md` is in this zip at the repository root.** Two
units implemented it from quotation because it was never delivered; commit it
so its numbers can be audited.

**The three captures `cw-2026-08-26-004808/-004900/-004952` may now be in the
tree** — Tim has been asked a fourth time. If present, bank them with floors and
the roles unit 015 assigned; if absent, one line in section 4, no fallback
work.

**`DECISIONS.md` has no record for HM-DEC-096–133, 136, 141, 150, nor Tim's
rulings of 2026-08-25/26.** `CLAUDE_CODE.md` says four report sections; its
version line reads 1.3 — read the file's own section count.

## Rulings in force

**Tim's dimming ruling, 2026-08-26, via unit 1.11.12's section-4 ask which he
adopted with this unit:** everything before the most recent stretch renders
dimmed, using the notion of "recent" the tree already has
(`CwTranscript.RecentCharacters`, 240). Selectable, nothing deleted, the eye
lands on current copy. **Narrowest reading; no separator, no timestamps, no
other transcript change.**

**HM-DEC-120 and every decode guard: untouched by construction.** This unit
changes no engine code. The engine suite's failure set must be byte-identical
at the end, and that identity is the proof.

**Rejected already, do not revisit:** the squelch on duty or fist-ratio axes
(measured dead in 1.11.12); headless-only verification of panel work — unit
1.11.13's working rule stands: **assert the geometry that causes the fault,
never that a point reaches a control**, because the headless offset is still
unexplained.

## Status cadence

Per §4.5: after each task, before the next, update `PROJECT_STATUS.md` —
`STATE`, `TASK: n of m`, `BALL`, `UPDATED` from the clock, `NOTE` saying what
is moving. Same every ten minutes while a task runs. **Unit 1.11.12's session
slipped this twice on long measurements; this unit has no long measurements
and no excuse.**

## The tasks

### Task 1 — bank the plan, check for the captures

Commit `BUILD_SESSION_2026-08-25.md` from this zip to the repository root, so
the plan two units implemented from quotation can finally be audited against
the tree. If the three `2026-08-26` captures are present, bank and floor them
with unit 015's roles (`004808` overlap, `004900` control with `WB8SC` /
`SKSK` / `KE8P`, `004952` honest-unsure); if absent, one line in section 4 and
nothing else. Build and run; record the green baseline.

### Task 2 — dim everything but the most recent stretch

Implement the dimming ruling: characters before the most recent
`RecentCharacters` stretch render dimmed; current copy bright; everything
selectable; nothing deleted; no other transcript change. Prove no decode
change: engine failure set byte-identical.

### Task 3 — the margin's first distribution *(the drop candidate)*

Unit 1.11.12 logs `marginLlr` for every character and nothing reads it yet.
Report its distribution across the corpus — per capture, P10/median/P90, split
by characters inside adjudicated anchors against everything else — so
tomorrow's squelch-replacement bound comes from data. **Measure and report
only; no behaviour change. Dropped whole if time runs out, and the report says
so.**

## Parked — do not touch, do not raise

Everything not named above, and in particular: the squelch's replacement (waits
on task 4's distribution and tonight's captures), the confirmation rule, the
meter's rebuild, the whole-file second pass, the joint cutter, the integrator
width, `014113`/`014308`, `001520`'s quadrillions, the reference/port
difference, the six-hertz disagreement, the short-character bias, the
`CHANGELOG.md`, the four intermittents, HM-OPEN-057, HM-OPEN-059.

A parked item that blocks a task is raised once, and says it was parked.

## What not to do

Standing prohibitions are `CLAUDE.md`'s and are not retyped. Unit-specific:

- **Do not touch engine code.** The byte-identical failure set is the claim.
- **Do not verify any panel work by headless hit-tests alone** — geometry
  assertions only, per 1.11.13's rule.
- **Do not change the transcript beyond the dimming ruling's narrowest
  reading.**
- **Do not re-litigate the squelch.**

## Committing, pushing, reporting

Commit and push each task before starting the next; name the branch; a refused
push is reported as refused, with the reason.

Report per `CLAUDE_CODE.md` §8 — read the file's own section count — to
`output.md` at the repository root, overwritten and printed.

**Section 3 leads with the transcript: current copy bright, history dimmed,
nothing deleted, engine byte-identical — and task 3's margin distribution if it
ran.** Section 2 says plainly what Tim sees: the screen finally landing the eye
on what is being read now.

### Asks still outstanding

Carried forward verbatim per HM-DEC-139 and HM-DEC-140. **Thirteen inbound.
The oldest is open since 2026-08-14.**

1. **The sweep's `invented` column counts substitutions, not invented
   characters.**
2. **Whether the refill guard should apply to the first fill at all.**
3. **`ANNUNCIATOR.md` renamed `PHASE` to `TASK` while HM-DEC-150 makes
   `PHASE` match the version's minor.**
4. **`DECISIONS.md` has no record for HM-DEC-096–133, 136, 141 or 150 — nor
   for Tim's rulings of 2026-08-25 and 2026-08-26.**
5. **The tone tracker** — the confirmation-rule ask stands; fist-quality
   selection is unmeasured (015's task 6 was dropped).
6. **The integrator width** — bears on `014113`/`014308`.
7. **The guard's gap is two to one**, calibrated on two empty captures.
8. **A boxcar's nulls made two of five swept offsets pathological best
   cases.**
9. **Two stations closer than 125 Hz are not named.**
10. **The keying meter** — hidden behind a setting; the rebuild is its own
    unit.
11. **HM-OPEN-057** (2026-08-22) and **HM-OPEN-007** (2026-08-14).

New and standing: **the squelch needs an axis — the logged margin is the
successor candidate** (task 3 measures it); **the three captures of 2026-08-26
were never delivered** (task 1 checks a fourth time); **the headless geometry
offset is still unexplained and hid three visible faults behind a green test**
(1.11.13); **prior orders carried no `ISSUED:` line** (corrected from this
unit).

Still open: **the lock's mixed help**; **the "Hold this pitch" button**;
**three fixtures at accepted cost**; **`001520`'s quadrillions**; **the
reference/port integrator difference**; **`CLAUDE_CODE.md`'s version line**;
**an unmeasured pitch costs `N4L`**; **`014113`/`014308`'s second mechanism**;
**the six-hertz window disagreement**; **the short-character bias**; **the
Avalonia geometry offset — task 2 must not trust it**; **`CHANGELOG.md` at
1.9.0**; **four intermittents**; **the whole-file second pass**; **the
confirmation rule cannot admit an intermittent station**.

**If you finish every task, stop and report. Do not start the next unit.**
