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

# Work instruction 011 — nothing stops it decoding silence; make something stop it

**This unit implements the shack-side analysis of the 2026-08-25 session
(`ANALYSIS-2026-08-25-session.md`), at Tim's direction: "I need the successes to
be significant and the failures to be rare."** Seven tasks; task 7 is the drop.

## Why this unit exists

**The unit's number: one law, measured thirteen times.**

Thirteen captures on 2026-08-25, 0115–0219 UTC, all 40 m, all −13.3 dBFS, tone
locked within a few hertz on twelve of thirteen. Sorted by keying duty, the
outcome sorts itself:

```
38–47 % duty  ->  readable, 0–8 unsure          (10 captures)
24 % duty     ->  real content buried in 48 noise characters
18 % duty     ->  8 seconds of station, 22 seconds of invented text
```

**The decoder's remaining failure is not decoding signals — it is that nothing
stops it decoding silence.** On a rag chew the silence is short and the damage
invisible; on a calling frequency the silence is most of the file and the output
is mostly invented. Measured on `021825`: real characters score
`T:6234, T:4798, T:765, K:712`; the noise scores `E:0.9, E:1.8, I:4.1`. The
number that separates them is already printed in every sidecar and nothing acts
on it.

**What is proven fixed and must be protected while fixing this:** tone at a
median error of 5.3 Hz (a week ago it was a 25 Hz grid reporting 300 on a 499.9
carrier); speed within 2 WPM from 18 to 31 whenever the tone is within ~12 Hz;
rag chews read end to end, twice at 0 unsure; and on `013303` Hamlet beat the
independent analysis chain for the first time. **`013520` (59 characters, 1
unsure) and `013303` must never read worse than they read that night.**

## What is and is not in this delivery

**Nine captures are in this zip** at
`tests/fixtures/cw/captured/unadjudicated/`: the remaining six W1AW bulletin
captures (`031838`, `031948`, `032012`, `032050`, `032113`, `032129`) and the
three 2026-08-23 pileups (`001831`, `001952`, `002016`), with sidecars, plus
`W1AW-ARLP034-PROPOSED-TRUTH.md` — **a proposal, not an adjudication; no test
treats it as truth until Tim rules.**

**The thirteen captures of 2026-08-25 are NOT in this zip.** They were never
delivered to the session that wrote this instruction. **Task 1 checks the tree
for them; Tim may have copied them in.** Every task below names what it does if
they are present and what it falls back to if they are not.

## Verify this instruction against the tree

**Nothing here describes the tree.** Check every claim and report mismatches,
including where the work succeeded anyway.

**Known state after unit 1.11.7: 32 failing of 1608 in the engine; 1 failing of
483 in the app, the known flaky one.** Two engine failures are the accepted cost
(`clean-12wpm`, `clean-18wpm`, exact digital silence, HM-OPEN-018); do not fix
them. **`ARecordingWithNoStationInItSaysNothing(014854)` is green and must stay
green.**

**`■` is HM-DEC-048's unresolved placeholder — the gate working, not failing.**
Unit 1.11.7 established this and the shack analysis still reads `■:-93.7` as an
emitted letter. **Where the analysis and the tree disagree about what `■` means,
the tree is right**; what remains true either way is that letters like `E:0.9`
pass a margin of nought, and that is what task 2 changes.

**`DECISIONS.md` still has no record for HM-DEC-096–133, 136, 141 or 150.**
HM-DEC-095 and HM-DEC-127 govern task 4's subsystem; unit 1.11.6's report
section 1 carries their transcription — read it there.

**`CLAUDE_CODE.md` says four report sections; its version line still reads 1.3.**
Read the file's own section count.

## Rulings in force

**Tim's direction of 2026-08-25, quoted:** *"Use that analysis and the attached
WAV files … I need the successes to be significant and the failures to be
rare."* This unit implements the analysis's fixes 1, 2, 4 and parts of 5, in its
stated order.

**That direction supersedes the margin-at-nought for emission — flagged, because
margin-at-nought was itself Tim's ruling (unit 1.11.3).** Task 2 introduces a
positive, normalised emission threshold. **The mechanism is the analysis's; the
constant is measured, provisional, and re-measured against the synthetic corpus,
the W1AW captures and `012403` before it hardens — the analysis's own caution,
verbatim.** If Tim vetoes, the constant returns to nought in one line.

**HM-DEC-120.** The property: nothing is emitted on audio holding no signal.
Both empty captures emit nothing, checked and stated at every task touching the
signal path. **`Gate` stands at 1.40 in a gap of 0.840 to 1.684 — do not consume
it, do not retune it.** Task 2 works *behind* the guard, on characters, not on
the window.

**HM-DEC-095 / HM-DEC-127** (per unit 1.11.6's transcription): a note is chosen
by how it is keyed and never by how loud it is; a confirmed station is not
abandoned for a candidate far below it. **Task 4 adds a keying-quality test,
which is 095's own principle extended — it must not move a confirmed station.**

**Rejected already, do not revisit:** locking to `CwPitch`; widening the guard
above 1.684; reverting the corrected scale; quantising pitch to a bin; tuning a
threshold to make a red test green; regenerating a fixture to justify a code
change in the same session; **more cluster tuning on the gap cutter — the
analysis proves the clusters merge at 30 WPM (`013637`: element and character
gaps four milliseconds apart).**

**PROPOSAL, not ruled — §4.4:** the "Hold this pitch" button stays exactly as it
is; the keying sweep's removal from the screen is display and Tim's; the W1AW
adjudication is Tim's. **Nothing on the panel changes in this unit.**

## Status cadence

Named here as well as in the prompt, per §4.5. After each task, before starting
the next, update `PROJECT_STATUS.md` per `CLAUDE.md` — `STATE`, `TASK: n of m`,
`BALL`, `UPDATED` from the clock, and `NOTE` saying what is moving inside the
task. The same every ten minutes while a task runs.

## The tasks

### Task 1 — bank everything that can be banked

1. Commit the nine captures from this zip. The six W1AW files complete the seven
   (`031905` is already in the tree). The proposed-truth file goes in beside
   them **marked exactly as it is marked — a proposal**.
2. **Search the tree for the thirteen 2026-08-25 captures** (`011552` through
   `021825`). If present, commit and bank them. **If absent, section 4 leads
   with it again**, and the fallbacks below apply.
3. Extend the harness: for **every** capture in the tree, write today's
   character, unsure and element counts in as **floors** (`>=`, never equality).
   Floors only ever rise. Where the 2026-08-25 files exist: `013520` (59
   characters, 1 unsure, 157 elements) is the reference, `013303` the
   beat-the-chain case, `012823` the negative control — **a change that improves
   `013520` while regressing `012823` has traded one failure for another and the
   harness must say so.**

Build and run the suite; record counts as the green baseline.

### Task 2 — the gate: stop emitting what the evidence says is absent

Characters below a **normalised** span-LLR threshold are suppressed — to
nothing, or to `■` where an element genuinely sounded. §0.0 ranks a marked
unknown above a wrong letter. Letters like `E:0.9` on a silent frequency stop
reaching the screen.

- **Normalise per element first** (the analysis's instruction; medians 40 / 225 /
  254 / 446 / 812 for 1–5 elements). Unit 1.11.7 measured that neither
  per-element nor per-keying-unit makes one-element characters comparable —
  **so measure the threshold's effect on E and T separately and report it**; if
  a single threshold cannot both keep real E's in `MEET` and kill soup E's, say
  so with the numbers rather than shipping a compromise silently.
- **Decisive fixtures if the 2026-08-25 files are present:** `021825` (nearly
  all noise — the gate must remove `E:0.9`-class characters and keep
  `T:6234 … K:712`), `021629` (buried `559 559 IN MI MI` must survive whole),
  `013010` and `013520` (controls — **not one real word lost**).
- **Fallback if absent:** soup side from `013622` (55 characters, no station
  adjudicated) and the pileups; control side from `012403`
  (`DE KD0UN KD0UN K`), `004507`, and the W1AW captures against their floors.
  **The constant is then marked provisional-on-weaker-evidence in its
  doc-comment**, in so many words.
- Both empty captures still emit nothing; `014854`'s test stays green.

### Task 3 — the clock eats only evidence

The speed estimator currently updates from everything, so silence drags it:
32 WPM hypothesised on a 22.5 WPM station (`012823`), 10 WPM on a 17.9 WPM
station fitted to the gaps between an 8-second transmission (`021825`).

**Let the clock update only from spans that pass task 2's gate.** What it
reports and when it withholds (HM-OPEN-022) is untouched — this changes what it
eats, not what it says. Verify: the corpus's reported speeds do not regress, and
on the two named captures (if present) the hypothesis no longer lands at 32 or
10.

### Task 4 — pick the pitch by fist quality, not energy alone

The survey already admits keying per bin. **Add a keying-quality score to the
bin choice**: dit:dah ratio inside 2.4–3.6 and duty inside 18–55 %, measured on
data the survey already computes. On `021629` this separates the real station
(485–540 Hz: ratio 2.7–3.0, duty 24–31 %) from a mush of neighbours within
2.4 dB (545–620 Hz: ratio 4+, duty 62–76 %), and it would have prevented
`012823`'s 50 Hz miss — the only tone failure of that night.

**This extends HM-DEC-095's principle and must not violate HM-DEC-127**: a
confirmed station is not abandoned. The quality score chooses *among candidates
for acquisition*; it does not displace a station being read. Verify on the
corpus that no capture's chosen pitch regresses; test the separation on a
generated two-tone fixture where truth is known, and on `021629` if present.

### Task 5 — duty in the sidecar, and why `competing` finds nothing

1. **Duty belongs in the sidecar.** It predicted every outcome of 2026-08-25, it
   is one number, and the envelope it comes from already exists. Add it,
   measured over the capture, to a tenth of a percent.
2. **`competing: none found` appeared in all thirteen sidecars**, including
   `021629` where 545–620 Hz carries energy within 2.4 dB of the tracked
   station. Diagnose what `competing` measures and why it misses what the
   spectrum shows, with file and line. **Diagnose only — fixing it may touch
   the two-stations-closer-than-125-Hz ask, which is unruled.** Use the pileup
   captures (`001952`, `002016`) if the 2026-08-25 files are absent.

### Task 6 — the joint cutter: an options table, no code

The boundary cutter is the dominant residual on good signals and the analysis
brackets it from both ends: at 18 WPM (`021410`) the gap classes are perfectly
separable (0.81u / 3.36u / 13.9u) and the cutter still cut inside characters —
a decision-rule fault; at 30 WPM (`013637`) element and character gaps are four
milliseconds apart and **no per-gap rule can work**. The analysis's candidate: a
small dynamic program scoring cut/no-cut jointly over a short window against
character validity and the fitted clock.

**Write the options table** — HM-DEC-010's form, at least the joint DP, an
improved local rule, and do-nothing, with costs and what each threatens — into
the report's section 4 for Tim's ruling. **Build nothing.**

### Task 7 — the keying sweep *(the drop candidate)*

Now wrong on **14 of 20** captures, including `no keying at 550 Hz` on `021410`
(37 characters emitted, 38 % duty) and `no keying at 600 Hz` on `021825` while
the station sat at 394. Its 4–7 ms key-down medians are arithmetically
impossible for Morse. Fix its two structural faults — the impossible medians
and the 25 Hz bin straddle — against whatever fixtures are in the tree.
**Its removal from the screen is display and Tim's; do not remove it.**

**This is the drop candidate. Dropped whole, and the report says so.**

## Parked — do not touch, do not raise

- **Fix 3 itself** — task 6 writes its options table; nothing is built.
- **The panel**: the sweep's visibility, the "Hold this pitch" button, the lock
  disagreement display, HM-OPEN-060.
- **Refusing an unmeasured pitch costs `N4L`** — needs a ruling.
- **`014113`/`014308` envelope smear and the integrator width** (45 vs 30 Hz).
- **The guard's two-to-one gap; `001520`'s quadrillions; the reference
  decoder's boxcar; `ElementsSeen`/`ElementsResolved`; `CwUnitEstimator.Runs`;
  the withdrawn six-hertz refinement; HM-OPEN-057, HM-OPEN-059.**

A parked item that blocks a task is raised once, and says it was parked.

## What not to do

Standing prohibitions are `CLAUDE.md`'s and are not retyped. Unit-specific:

- **Do not lose one real word from `013520`, `013303` or `013010`** (if present)
  or from `012403`, `004507`, the adjudicated callsigns and the W1AW floors
  (regardless). The gate exists to remove soup, and a gate that eats real words
  has failed its own fixtures.
- **Do not lower a floor. Do not let `012823` (if present) get worse.**
- **Do not treat the proposed W1AW truth as truth.**
- **Do not touch the panel, the guard, or the tracker's displacement rules.**
- **Do not tune gap clusters** — ruled out above, with the measurement.
- **Do not trade the silence property.**

## Committing, pushing, reporting

Commit and push each task before starting the next. The report names the branch
and states whether each push succeeded; a refused push is reported as refused,
with the reason.

Report per `CLAUDE_CODE.md` §8 — read the file's own section count — to
`output.md` at the repository root, overwritten and printed.

**Section 3 leads with the gate's effect, per capture: characters suppressed,
split into soup removed and real words touched — where "real" is an adjudicated
callsign, a banked floor, or a W1AW proposed line, each labelled as which.**
Section 2 says plainly what Tim will see on a quiet frequency now versus last
night.

### Asks still outstanding

Carried forward verbatim per HM-DEC-139 and HM-DEC-140. **Eleven inbound, none
ruled, the oldest open since 2026-08-14. Thirteen consecutive units have now
worked beside rulings they cannot read.**

1. **The sweep's `invented` column counts substitutions, not invented
   characters.**
2. **Whether the refill guard should apply to the first fill at all.**
3. **`ANNUNCIATOR.md` renamed `PHASE` to `TASK` while HM-DEC-150 makes `PHASE`
   match the version's minor.**
4. **`DECISIONS.md` has no record for HM-DEC-096–133, 136, 141 or 150.**
5. **The tone tracker** — narrowed by the hold; the 2026-08-25 table shows the
   hold is what made the decoder work.
6. **Whether the integrator ships at 45 Hz or 30 Hz** — bears on `014113` and
   `014308`.
7. **The guard's gap is two to one**, calibrated on two empty captures.
8. **A boxcar's nulls made two of five swept offsets pathological best cases.**
9. **Two stations closer than 125 Hz are not named** — task 5's diagnosis bears
   on it.
10. **The keying witness is wrong more often than right** — now 6 against 14.
11. **HM-OPEN-057** (2026-08-22) and **HM-OPEN-007** (2026-08-14).

Still open from earlier units: **the lock helping sometimes and hurting
sometimes**; **the "Hold this pitch" button**; **two clean fixtures at accepted
cost**; **`001520`'s quadrillions**; **the reference/port integrator
difference**; **`CLAUDE_CODE.md`'s version line**; **an unmeasured pitch costs
`N4L`**; **`014113`/`014308`'s second mechanism**; **the six-hertz window
disagreement**; **HM-OPEN-060**; **the short-character bias needs a
per-character expectation**; **the W1AW seven await Tim's adjudication — the
proposal is now in the tree**.

New from the shack analysis, not covered above: **the joint cutter's options
table** (task 6 writes it); **`competing` finds nothing it should find** (task 5
diagnoses it); **thirteen 2026-08-25 captures may still be unbanked** (task 1
settles it or says so).

**If you finish every task, stop and report. Do not start the next unit.**
