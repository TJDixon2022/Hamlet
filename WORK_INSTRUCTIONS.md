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

# Work instruction 025 — the cuts, not the letters

**ISSUED: 2026-08-27. A fresh order, not an amendment.**

**Four tasks; task 4 is the drop.** This unit implements option A of
`DEV_ANALYSIS_2026-08-27.md` §4, which is in this zip and is committed by task 1.

The standing goal governs every part: **he hears CW, Hamlet must decode it,
eighty percent of the time.**

## Why this unit exists

**The unit's number: `ATEEKEND`.**

On `cw-2026-08-25-021410` — 18.2 words a minute, machine-grade fist — the gap
classes measure **53 / 221 / 913 ms**, which is 0.81u / 3.36u / 13.9u with wide
dead zones between them. Perfectly separable. **And the cutter still split `W`
into `A T E`.** The rule fails when the information is perfect.

At the other end, `cw-2026-08-25-013637` at 30.6 words a minute has element and
character gaps **four milliseconds apart** — 24 and 28 ms. `AB OVE`, `BREE Z E`.
**No per-gap threshold can work there even in principle.**

**The letters are already right. The cuts are wrong.** `USEDTOUSEAFIRM`,
`OUTOFALT`, `TTHINKING`, `FLENX` — every one is correct elements, mis-segmented.
And the shack-side analysis states the thing that makes this the unit:
**the element-to-character decision has not been touched since the corpus began,
and it is now the only stage that hasn't.** Everything around it moved this week
— the squelch, the screen, admission, the operator's assertion — and the quality
of the characters Hamlet emits did not.

**One correction the analysis does not carry, so the session does not confuse
the two.** Unit 1.11.9 built and measured a *validity term* added to the
existing cutter's path score, and settled it at a safe weight of nought: at the
only weight that helped anything, it ate `VA3VRR`. **That is not this.** This is
a dynamic program over element boundaries with duration-fit terms, which unit
1.11.9's own report named as the untried alternative. Do not treat the earlier
measurement as a verdict on this one.

## Verify this instruction against the tree

**Nothing here describes the tree.** Check every claim and report mismatches,
including where the work succeeded anyway. Every unit since 1.11.17 disproved
part of its own order's premise and was right to.

**Fixtures this unit needs may be absent.** `021410`, `013637`, `011447`,
`011514`, `011112`–`011617`, `021825`. **Task 1 checks and says which are
present.** Any acceptance line naming an absent fixture is reported as
unmeasurable rather than quietly dropped.

**Expected state: 28 failing of 1841 in the engine as the stable set; 503 of 503
in the app. Seven timing intermittents exist and four fired in the last
session.** Do not chase any of them; diff which tests moved and never trust a
total.

**A mismatch to check and report:** the analysis asks for the keying sweep to be
hidden behind a debug flag, but unit 1.11.12 shipped `AppSettings.ShowKeyingSweep`
defaulting off. **Say which is true in the tree today** — either the setting was
turned back on, or the analysis is reading older captures.

**`DECISIONS.md` still has no record for HM-DEC-096–133, 136, 141, 150, nor
Tim's rulings of 2026-08-25/26/27.** **`CLAUDE_CODE.md` is at version 1.4.**

## Rulings in force

**Tim's ruling, 2026-08-27, by adopting this unit (flagged for veto in the
delivery):** option A of the analysis — **the joint decoder replaces per-gap
thresholding**, as specified in its HOW section. Option B is rejected because it
rebuilds half the machinery and then rebuilds it again; option C is the status
quo.

**Tim's ruling, same date: the joint decoder ships behind a setting.**
`AppSettings.UseJointDecoder`. **Default on if every floor and every anchor is
green; default off, shipped anyway, with the measurement reported, if they are
not.** The operator is at the radio tonight and a switch he can throw is worth
more than a change he cannot compare against.

**The §0.0 guard, and it is not negotiable.** **No language model. No
letter-frequency prior. No dictionary. No word list.** The only knowledge
admitted is the Morse table and the fitted clock. A decoder with an English
prior invents plausible words from marginal audio, which is the confident lie
this project exists to prevent. **The validity term stays small against the
timing terms, and `cw-2026-08-25-021825` — noise — must still yield blocks
rather than letters. If the guard and the acceptance conflict, the guard wins
and the unit ships less.**

**HM-DEC-120 is untouched and upstream.** The squelch, the gate and the silence
property are not part of this unit. Both silence controls emit nothing, checked
and stated.

**Rejected already, do not revisit:** the validity term on the existing cutter
(1.11.9, safe weight nought); gap-cluster retuning (clusters merge at 30 WPM —
`013637` is the proof); six admission axis families; the envelope as the
survey's input; per-pass scoring; agreement between fitted units; gating on
`spanLlr` (inverted on strong signals — 004808 soup at 8,000–29,000 against real
letters at 41–437).

## Status cadence

Per §4.5: after each task, before the next, update `PROJECT_STATUS.md` —
`STATE`, `TASK: n of m`, `BALL`, `UPDATED` from the clock, `NOTE` saying what is
moving. Same every ten minutes while a task runs. **This is the largest single
change to the decode path in the project's history; the cadence is how Tim knows
it is progressing rather than stuck.**

## The tasks

### Task 1 — the floors green, and the four fixtures on the record

Commit `DEV_ANALYSIS_2026-08-27.md` to the repository root.

Check for `021410`, `013637`, `011447`, `011514`, `011112`–`011617` and
`021825`; commit and floor any that are present in this zip or already in the
tree, and **name any that are absent**.

**Record what each of the four named failures reads today**, verbatim, so the
after is comparable: `021410`'s `ATEEKEND`, `TTHINKING`, `FLENX`; `013637`'s
`AB OVE`, `BREE Z E`; `011447`'s `USEDTOUSEAFIRM`; `011514`'s `OUTOFALT`.

**Every floor and every anchor green before the first edit**, diffed rather than
totalled. If anything is red that is not a known intermittent, **stop and report
it** — this unit must not begin on an unstable tree.

### Task 2 — the joint decoder

Implement the analysis's HOW exactly:

- **State**: position between elements. **Transition**: emit character C
  spanning elements i..j, allowed only if the mark pattern of i..j is C's
  pattern.
- **Transition cost**: the sum of duration-fit terms — each mark against 1u or
  3u, each internal gap against 1u, the boundary gap against 3u for a character
  or 7u for a word — as log-likelihoods around the fitted clock, with a per-fist
  spread learned from the recent stream. Plus a **small** flat validity term,
  and a matching cost for the `■` hypothesis **so an unreadable span loses to a
  block rather than to an invented letter**.
- **Streaming**: finalise with a lag of about two characters, emitting on
  traceback agreement, at the pipeline's existing cadence.
- **Nothing already settled is retracted.** §0.0: the display does not un-say
  things.

Behind `AppSettings.UseJointDecoder`, per the ruling.

**Acceptance, and the floors are the judge:**

- `021410`: `ATEEKEND` → `WEEKEND`, `TTHINKING` → `THINKING`, `FLENX` → `FLEX`;
- `013637`: `AB OVE` → `ABOVE`, `BREE Z E` → `BREEZE`;
- `011447`: `USEDTOUSEAFIRM` → `USED TO USE A FIRM`;
- **every floor and every anchor the same or better** — the two rag-chew
  evenings, the W1AW seven, `KD0UN`, the synthetic file;
- **`021825` still yields blocks, not letters**;
- both silence controls silent; chunk invariance intact.

**If the acceptance is met, the setting defaults on. If it is not, the setting
defaults off and ships anyway, with a per-fixture table of what improved and
what regressed.** Shipping it off is not a failure; shipping it on while an
anchor is red is.

### Task 3 — the constrained margin, which falls out of the table

The analysis §2 measured margins of 0.1–3.4 on right answers and 0.0–1.9 on
wrong ones — no separation — because second-best is free to re-segment, so
there is always a trivially different alternative.

**Constrain it: second-best is the best path forced to a different character
over the same span and the same element boundaries.** Task 2's dynamic program
produces exactly this as a by-product.

Log it beside the existing figures, **clamped** — the sheet has printed
`6:27306879.3` and `■:-1876275.2`. **Report its distribution**, correct
characters against pileup characters, on the logged corpus. The analysis's own
target is an order of magnitude of separation. **Measure and report; change no
behaviour on it.**

### Task 4 — the three one-sentence items *(the drop candidate)*

1. **After a Clear, `textCovers` still says "everything read since the decoder
   started listening"** while the text starts at the clear. Say "since the
   transcript was cleared at hh:mm:ss".
2. **The keying sweep** — per the mismatch above, ensure it is behind its
   setting and that the setting is off.
3. **`competing: none found` appears in every sidecar of the week**, including
   files with eight admitted tones and a station 2.4 dB from the tracked one.
   **Either report what the survey saw or drop the field** — a field that always
   says the same thing is worse than no field.

**Dropped whole if time runs out, and the report says so.**

## Parked — do not touch, do not raise

Admission and the six dead axis families; the survey; the squelch and the gate;
the operator's assertion path; the meter's rebuild; the integrator width; the
whole-file second pass; `001520`'s quadrillions and `013347`'s 17.2 million;
the reference and port integrator difference; the short-character bias; the
Avalonia offset; `CHANGELOG.md`; the seven intermittents; HM-OPEN-057;
HM-OPEN-059; **the panel beyond task 4's items.**

A parked item that blocks a task is raised once, and says it was parked.

## What not to do

Standing prohibitions are `CLAUDE.md`'s and are not retyped. Unit-specific:

- **No language model, no letter-frequency prior, no dictionary, no word list.**
  The guard outranks the acceptance.
- **Do not default the setting on with any floor or anchor red.**
- **Do not retract settled text.**
- **Do not touch the squelch, the gate, admission, or the silence property.**
- **Do not tune the validity term upward to reach an acceptance line** — if the
  timing terms cannot do it, report that they cannot.
- **Do not chase an intermittent.**
- **Floors only rise; anchors stay green; chunk invariance holds.**

## Committing, pushing, reporting

Commit and push each task before starting the next; name the branch; a refused
push is reported as refused, with the reason.

Report per `CLAUDE_CODE.md` §8 — read the file's own section count — to
`output.md` at the repository root, overwritten and printed.

**Section 3 leads with the four named failures, before and after, verbatim.**
**Section 2 says whether the setting ships on or off, and what the operator
should expect to see differently at the radio tonight.**

### Asks still outstanding

Carried forward verbatim per HM-DEC-139 and HM-DEC-140. **Eighteen inbound. The
oldest is open since 2026-08-14.**

1. **The sweep's `invented` column counts substitutions, not invented
   characters.**
2. **Whether the refill guard should apply to the first fill at all.**
3. **`ANNUNCIATOR.md` renamed `PHASE` to `TASK` while HM-DEC-150 makes `PHASE`
   match the version's minor.**
4. **`DECISIONS.md` has no record for HM-DEC-096–133, 136, 141 or 150 — nor for
   Tim's rulings of 2026-08-25/26/27, including the two this unit acts under.**
5. **The tone tracker** — six axis families measured; the question is a design
   one and the operator's assertion is the way round it meanwhile.
6. **The integrator width** — settled at 45 Hz, with the sharp-peak caveat.
7. **The guard's gap is two to one**, calibrated on two empty captures.
8. **A boxcar's nulls made two of five swept offsets pathological best cases.**
9. **Two stations closer than 125 Hz are not named** — the operator's item five,
   and `competing` is task 4's third item.
10. **The keying meter** — 17 contradictions, including `no keying` on both of
    the readable captures of 2026-08-27.
11. **HM-OPEN-057** (2026-08-22) and **HM-OPEN-007** (2026-08-14).
12. **The gate opens on everything, including two empty recordings** (1.11.18).
13. **An asserted pitch does not relax the decoder's own gate** — `014113` is
    pointed within seven hertz of its station and still emits nothing.
    **Tim's, without exception; still unruled.**
14. **The quantisation statistic's unit search is biased to its own lower
    bound** (1.11.21).
15. **`spanLlr` inverts on strong signals** — do not gate on it.
16. **Raw scores still need clamping** — task 3 clamps the new one.
17. **Seven timing intermittents, four fired in one session.** A full-run total
    is unreadable; worth its own small unit.
18. **The mark and gap classifiers do not share a unit** — a forced-unit sweep
    across 8–44 WPM cannot reproduce Hamlet's signature with any single unit.
    **Task 2 is the fix if the joint decoder lands.**

Still open: **the lock's mixed help**; **three fixtures at accepted cost**;
**the reference and port integrator difference**; **an unmeasured pitch costs
`N4L`**; **the six-hertz window disagreement**; **the short-character bias**;
**the Avalonia geometry offset**; **`CHANGELOG.md` at 1.9.0 against 1.11.21**;
**the whole-file second pass**; **the squelch has no axis**; **the three morning
captures of 2026-08-26**.

**If you finish every task, stop and report. Do not start the next unit.**
