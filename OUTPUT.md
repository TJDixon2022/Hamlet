# Work instruction 017 — the dimming, the plan in the tree, and the margin read

## 1. What Claude did

Claude Code, on the development computer, in `C:\Source\HamLet`. The prompt
claimed `PROJECT: Hamlet`; the tree confirmed it — `SHACK_FACTS.md` and
`src/Hamlet.RadioEngine/Cw/CwProbabilisticDecoder.cs` exist, neither
`CoreHMI.sln` nor `MURC.sln` does, `CLAUDE.md`'s header says Hamlet and the
solution is `Hamlet.sln`. Branch `main` throughout, three commits, all pushed,
none refused. Version 1.11.13 to 1.11.14 per HM-DEC-150.

**Nothing here is evidence about the radio.** No rig was connected.

**No decision was recorded under §12.1.** Section 4 carries what needs a ruling.

**All three tasks ran. Nothing was dropped.**

### Where the instruction and the tree disagree

- **The three captures of `2026-08-26` are still absent** — `004808`, `004900`
  and `004952` are nowhere in the tree. Asked a fourth time. No fallback work,
  as the order directs.
- **The engine baseline was 28 failing of 1831 and the app 497 of 497**, exactly
  as stated.
- **There is a fifth intermittent, not four.**
  `Rig.ScopeOutputWriteTests.ConfirmedNeedsTheReadbackToAgree` failed once in a
  full run and passed three times alone. It is a rig timing test and this unit
  touched no engine code, so it cannot be this unit's doing — but the order's
  count of four is now five.
- **`ISSUED: 2026-08-26` is present and it did its job**: this session could tell
  a fresh order from an amendment by reading one line rather than diffing the
  file against its own last commit. First time in this project's history.

### Task 1 — the plan is in the tree

`BUILD_SESSION_2026-08-25.md` is committed at the repository root. Two units
implemented it from quotation and its numbers could not be checked against the
instrument that produced them, which is exactly where unit 1.11.12's central
measurement went wrong.

**And having it lets that failure be named precisely rather than by inference.**
The plan's duty figures — 18–24 % invented, 36–47 % readable, 55 %+ overlap —
are **whole-capture** figures over a swept pitch. Unit 1.11.12 measured a
**rolling three-second** duty at the tracked pitch, which is the only duty a
live squelch can have, and got a different quantity. The two diverge hardest
exactly where the squelch was aimed: a whole-capture duty of 18 % is eight
seconds of real keying plus twenty-two seconds of silence, and silence measured
through a narrow filter reads about half rather than nought — so the rolling
median lands near 42 % while the whole-file figure says 18 %. The plan's own
`021629` numbers, 24–31 % at the station bin, match the rolling measurement
closely. **It was never a wrong plan; it was a plan whose axis does not survive
being computed the way the fix would have to compute it.**

### Task 2 — the dimming, shipped

Tim's ruling of 2026-08-26 at its narrowest. Characters before the most recent
`CwTranscript.RecentCharacters` recede toward the surface; the recent stretch
stays bright; everything is selectable and nothing is deleted.

Measured on seven hundred and twenty characters: **240 bright, 480 receded, 720
still on the screen**.

**It is a blend toward the surface and not an opacity**, so each ink keeps its
own hue — a placeholder is still amber when it is old and an uncertain character
still the dimmer green. §0.6 forbids colour carrying meaning alone, and receding
must not quietly become a fourth confidence state; a test holds the three apart
and holds receded ink distinct from the surface, because text nobody can read is
deleted in everything but name.

**The boundary falls at a run rather than inside one**, so a little more than 240
characters stay bright. Splitting a run would rewrite text the operator may be
part-way through selecting, for a boundary nobody can see.

**Nothing is verified by hit-testing**, per unit 1.11.13's rule. What is asserted
is the ink each character is actually drawn with. The terminal gained an internal
`Draw` so a test need not wait on its timer.

### Task 3 — the margin's first distribution, and it does not separate

`marginLlr` has been logged since unit 1.11.12 and nothing had read it. Read
across all thirty-six captures, 1,583 characters, split by whether a character
falls inside its recording's adjudicated anchor:

| | n | P10 | median | P90 | max |
|---|---|---|---|---|---|
| **inside an anchor** | 131 | 0.449 | **1.793** | 5.676 | 18.1 |
| **everything else** | 1,452 | 0.230 | **1.570** | 37.289 | 2.98 × 10⁸ |

**The two distributions sit on top of each other.** The anchors' median is
1.79 and everything else's is 1.57 — a fifth of a unit apart on a quantity that
runs to hundreds of millions. Swept as a floor:

| floor | anchor characters kept | everything else kept |
|---|---|---|
| ≥ 0.5 | 87 % | 78 % |
| ≥ 1 | 72 % | 63 % |
| ≥ 2 | 46 % | 41 % |
| ≥ 5 | **13 %** | **22 %** |
| ≥ 25 | 0 % | 11 % |

**At every useful floor it cuts correct copy about as fast as soup, and past
five it cuts correct copy faster.** It is not a weak axis; at the top end it is
an inverted one.

**And it inherits the scale problem it was meant to escape.** On
`cw-2026-08-17-013347` the margin reaches 2.98 × 10⁸ and on `013622` 1.17 × 10⁸,
because `best − second` is a difference of *path* scores and a path score
carries each recording's own noise estimate — the same reason `spanLlr` is
incomparable across recordings. The clamp added in 1.11.12 is why the sheet stays
readable; it does not make the quantity comparable.

Measured and reported only. Nothing changed.

### The suite

| | baseline | end |
|---|---|---|
| engine | 28 failing of 1831 | **28 failing of 1831** |
| app | 497 passing | **501 passing, 0 failing** |

**No engine file was touched** — the diff is two app controls and two test files
— which is the real proof that the decode cannot have moved. One full run showed
29 rather than 28; the extra is the fifth intermittent named above, which passes
alone.

## 2. What Tim sees at the radio

**The screen finally lands the eye on what is being read now.** Everything older
than the last couple of hundred characters sits back into the page. Current copy
is the brightest thing on the instrument, which is what was missing on the night
the transcript showed a hundred characters of two-minute-old soup above three
correctly-read callsigns.

**Nothing is lost.** History is dimmer, not gone: still there, still selectable,
still readable if he wants to go back and check what was said. Trimming is a
separate thing and still happens only at four thousand characters.

**The confidence colours still mean what they meant.** A placeholder is amber
whether it is current or old, and an uncertain character is the dimmer green
either way. Receding changes how far forward text sits and nothing else.

**And nothing about the decode changed** — same failure set, no engine file
touched.

**What will look wrong and is not:**

- **A little more than 240 characters stay bright.** The boundary falls between
  runs rather than inside one, deliberately.
- **The keying sweep is still absent from the terminal** and the squelch still
  does not exist, both from earlier units. A quiet frequency still produces soup;
  what changed today is only that yesterday's soup no longer competes with it.

## 3. What you should see

**The transcript**, measured on 720 settled characters:

| | |
|---|---|
| bright, current copy | **240** |
| receded, history | **480** |
| on the screen | **720 of 720 — nothing deleted** |
| receded inks distinct from each other | **3 of 3** |
| receded ink equal to the surface | **none** |
| a receded run after a bright one | **none** |
| engine failure set | **28 of 1831, unchanged** |

**The margin's first distribution**: inside an anchor, median 1.793 (P10 0.449,
P90 5.676); everything else, median 1.570 (P10 0.230, P90 37.289). **They
overlap**, and past a floor of five the quantity keeps soup at 22 % while keeping
correct copy at 13 %.

## 4. What's blocking us

**The margin is not the squelch's successor, and the reason rules out a family
of candidates rather than one.**

Task 3's numbers are in section 1. The hoped-for property was that a letter
carved out of continuous tone has a second-best reading that fits about as well,
so its margin collapses while a real letter's does not. Whatever truth that has
is swamped: `best − second` is a difference of path scores and a path score
carries each recording's own noise estimate, so the quantity is no more
comparable across recordings than `spanLlr` was — it reaches 2.98 × 10⁸ on one
capture and 1.8 on another.

**What that suggests, and it is a suggestion rather than a proposal**: a usable
axis probably has to be *dimensionless by construction* — a ratio of two things
measured through the same noise estimate, so the estimate cancels — rather than a
difference of two log-likelihoods. `marginLlr / spanLlr` is the obvious such
ratio and it is one line to log beside the two already there.

*Rejected: setting a bound from these numbers anyway.* At every floor it costs
anchor characters at least as fast as soup.

---

**Three captures have now been asked for four times.**

`cw-2026-08-26-004808`, `-004900` and `-004952`. `004808` is the overlap fixture
and the stated proof of the spanLlr inversion; `004900` is the control carrying
`WB8SC`, `SKSK` and `KE8P` — the three tokens the dimming shipped today exists to
make visible; `004952` is the honest-unsure case. Without them the squelch's
successor has no overlap fixture at all, and today's dimming cannot be checked
against the very screen that motivated it.

---

**A fifth intermittent, and the count in the orders is now stale.**

`Rig.ScopeOutputWriteTests.ConfirmedNeedsTheReadbackToAgree` joins the four
already known. It failed once in a full run and passed three times alone, and
this unit touched no engine code. Five tests that fail on timing rather than on
behaviour is enough that a full-run count of 28 is no longer a number anyone can
read without checking which tests moved.

### Asks still outstanding

Carried forward verbatim per HM-DEC-139 and HM-DEC-140. **Thirteen inbound. The
oldest is open since 2026-08-14.**

1. **The sweep's `invented` column counts substitutions, not invented
   characters.**
2. **Whether the refill guard should apply to the first fill at all.**
3. **`ANNUNCIATOR.md` renamed `PHASE` to `TASK` while HM-DEC-150 makes `PHASE`
   match the version's minor.**
4. **`DECISIONS.md` has no record for HM-DEC-096–133, 136, 141 or 150 — nor for
   Tim's rulings of 2026-08-25 and 2026-08-26.**
5. **The tone tracker** — the confirmation-rule ask stands; fist-quality
   selection is unmeasured.
6. **The integrator width** — bears on `014113`/`014308`.
7. **The guard's gap is two to one**, calibrated on two empty captures.
8. **A boxcar's nulls made two of five swept offsets pathological best cases.**
9. **Two stations closer than 125 Hz are not named.**
10. **The keying meter** — hidden behind a setting; the rebuild is its own unit.
11. **HM-OPEN-057** (2026-08-22) and **HM-OPEN-007** (2026-08-14).

New this unit: **the margin does not separate and the reason rules out
differences of log-likelihoods generally**, above; **the three captures of
2026-08-26, asked a fourth time**, above; **a fifth intermittent**, above.

Closed this unit: **the shack plan is in the tree** and its duty axis is now
explained rather than merely contradicted. **The transcript's dimming**, which
had no trigger that existed until Tim's ruling gave it one.

Still open: **the lock's mixed help**; **the "Hold this pitch" button**; **three
fixtures at accepted cost**; **`001520`'s quadrillions**; **the reference/port
integrator difference**; **`CLAUDE_CODE.md`'s version line**; **an unmeasured
pitch costs `N4L`**; **`014113`/`014308`'s second mechanism**; **the six-hertz
window disagreement**; **the short-character bias**; **the Avalonia geometry
offset, still unexplained**; **`CHANGELOG.md` at 1.9.0 against 1.11.14**; **the
whole-file second pass**; **the confirmation rule cannot admit an intermittent
station**; **the squelch has no axis**.
