# Work instruction 006 — the noise scale, and the guard that can go once it is fixed

## 1. What Claude did

Claude Code on the development computer, `C:\Source\HamLet`. The prompt claimed
`PROJECT: Hamlet` and so does `WORK_INSTRUCTIONS.md`; the tree confirms it —
`SHACK_FACTS.md` and `src/Hamlet.RadioEngine/Cw/CwProbabilisticDecoder.cs` exist,
neither `CoreHMI.sln` nor `MURC.sln` does, the solution is `Hamlet.sln`, and
`PROJECT_CARD.md` names Hamlet. **Branch `main`**, per §9.5.1. Four tasks, all
four worked; task 4 was not dropped but was satisfied by re-running unit
1.11.3's harness rather than building a second one, and that is said here rather
than claimed as new work. Every push succeeded; none was refused.

**Nothing in this report is evidence about the radio.** No rig was connected.

**Nothing was recorded to `DECISIONS.md`.**

### The shape conflict, and a real defect in how it is signalled

`CLAUDE_CODE.md` is **version 1.3** and §8 now says **four** sections. Followed.
`SESSION_PROTOCOL.md` §12.2 still says three. Seventh consecutive unit naming it.

**But the version line did not move when the contract did.** A backup this
session's own tooling took at 10:25 today —
`CLAUDE_CODE.md.bak-20260824-102543` — reads *"Version 1.3 … **Five** sections"*,
and the current file reads *"Version 1.3 … **Four** sections"*. §0 of that file
says the version line "is the only handle, and it depends on somebody comparing."
A session that compared version lines would have concluded nothing changed and
written five sections.

### Task 1 — the captures, the premise, and the contradiction

The three captures are committed at
`tests/fixtures/cw/captured/unadjudicated/`. They added three theory cases to
the suite, all passing.

**The three figures, reproduced in-tree. Hamlet disagrees with all three, and
two of the disagreements matter.**

| capture | instruction said | Hamlet says |
|---|---|---|
| `012403`, 20–30 s, 20 WPM @ 439.81 | ratio ≈ 11.3, `DE KD0UN KD0UN K` | **13.94**, and still refused by a gate of 15 |
| `031905` @ 499.9 | ratio ≈ 17.5, **soup** | **25.84**, and **not soup**: `PREDICTED 10.7 CENTIMETER FLAX IS 125` |
| `001520` @ 600 | ratio in the billions | **13,950,103,585**, and it reads `KC3QIS` |

**`031905` is a propagation bulletin, not soup** — unit 1.11.3's per-character
gate had already cleaned it. **`001520` is the operator's own callsign**, not
garbage. Both characterisations in the instruction are out of date because the
previous unit's change fixed them.

**The pitch claim holds and is worth the emphasis.** The same ten seconds scores
13.94 at 439.81 Hz, 12.44 at 450, and **10.36 at the radio's own `CwPitch` of
600**. The sidecar records `CwPitch 600 Hz` beside a station at 439.81. **The
radio's CW pitch and the station's pitch are unrelated**, confirmed in-tree.

**The contradiction in unit 1.11.3 resolves, and both numbers were right about
different things.** `134712`'s **4.64** is the whole-file offline read. Its
**35.8** was the *last* streaming window, which that unit's corpus table printed
in a column headed `window` as though it described the run. Across the run:
56 reads, median **2.54**, max 35.86, and **15 of 55 clear the gate**, emitting
28 characters. So the guard was suppressing about three quarters of that
recording's windows, not all of them — the corpus table's column was mislabelled
and the premise table was correct.

**The trace.** `LogLikelihoods` has exactly one caller,
`CwProbabilisticDecoder.cs:527`, inside `Decode`. Its outputs feed the
all-key-up total, `DecodeAt`, the window ratio, and every character's span
ratio. **Two thresholds are expressed in its units**: `Gate = 15` (`cs:155`,
applied `cs:573`, shown on the panel at `MainWindowViewModel.cs:4307`), and
`CharacterMargin = 0`. The span was the whole recording offline and
`WindowSeconds = 12` in streaming.

**`CharacterMargin = 0` survives the change untouched, and that is a property of
Tim's ruling rather than luck**: zero is zero under any positive rescaling. The
value chosen because it was "the one value that is not a tuned threshold" is
also the only one that could have crossed this task intact.

**Green baseline: 32 failing of 1601 in the engine, 480 of 481 in the app.** The
instruction states 31 of 1596 and 481 of 481; the difference is three theory
cases from the new captures (all passing) and two known-flaky tests
(`ABroadcastFrequencySurvivesTheSweepWithItsProvenance`,
`ItIsDrawnWhileRefillingAndGoesWhenTextResumes`) which flake in both directions.

### Task 2 — the scale

`σ = P25 / 0.758528`, from the Rayleigh identity, with the derivation in the
doc-comment. The arithmetic checks: the old `P25 × 0.6` is **0.4551 σ**, 2.197×
too small, matching the instruction exactly. Key-up is now a proper Rayleigh
log density carrying its `ln e` term; key-down is the same Gaussian
approximation it always was, now properly normalised, so the difference is a
log-likelihood ratio rather than a difference of two differently-scaled numbers.
Both estimates are taken over a rolling **2.5 s** span on both paths.

**The span barely matters.** At 1.5 s, 2.5 s and 4.0 s every capture's figures
move by a few per cent, with one exception: `012403` loses `KD0UN` at 1.5 s and
keeps it at 2.5 s and 4.0 s. That is the whole sensitivity.

### Task 3 — measured, and the answer is no

Section 3.

### Task 4

Re-ran unit 1.11.3's `TheEmitDecisionTable` against the corrected scale, so the
two units' tables sit side by side in the same format. `ANALYSIS-cw-noise-scale-2026-08-24.md`
carries the ungated reads, the margins and the span sensitivity;
`ANALYSIS-cw-emit-decision-2026-08-24.md` carries the production and locked
columns.

## 2. What Tim should expect

**Read this before running it. The decoder currently reads almost nothing, and
that is this unit's doing.**

Correcting the scale deflated every window ratio by roughly the factor the old
scale had inflated it — five to six times. `Gate = 15` was calibrated in the old
units and is now a bar in units that no longer exist. Through the production
path:

| capture | was | now |
|---|---|---|
| `004507`, the ARRL bulletin | 50 characters, bulletin readable | **nothing** |
| `134712` | 28 characters, `N4L` visible | **nothing** |
| `003016` | 58 characters, readable English | **nothing** |
| `003126` | 53 characters, readable English | **nothing** |
| `013347` | 59 characters, `VA3VRR` | 57 characters, `VA3VRR` |
| `003758` | 53 characters, `AA4MP/4 QNIK` | 34 characters, `AA4MP/4 QNIK` |

**The instruction forbids tuning the guard and forbids removing it, and both
prohibitions are right** — the corpus proves no value works and fitting one here
would be fitting it to the fixture that justified it. So the unit did what it
was told and stopped, and the tree is left in a state where the guard is the
only thing between the operator and the text.

**This is a one-line revert if you want the radio working tonight**: put
`Percentile(sorted, 25) * 0.6` back and the previous unit's behaviour returns
exactly. Everything measured below survives the revert, because it was all taken
with the guard bypassed.

**The silence property held throughout**, on every task that touched the signal
path. Both empty captures emit nothing, asserted rather than inferred. One
consequence is an improvement: `ARecordingWithNoStationInItSaysNothing(014854)`
now **passes** for the first time since unit 002 — the empty band's ratio fell
from 8.0 to 0.65, restoring the margin the Hann swap had narrowed.

**What will look wrong and is not:**

- **49 failing of 1600 in the engine, 481 of 481 in the app**, against a baseline
  of 32. Nineteen moved and three went green. The instruction predicted the
  gate-margin assertions would move and they did; what it did not predict is
  that eleven of the nineteen are ordinary decode assertions on the corpus,
  failing because the guard now silences those recordings.
- **`ItReadsWhatTheReferenceReads` is among them**, and it is the one worth
  naming separately: `tools/reference-decoder/reference_decoder.py` still
  carries the old model, so the port and its reference have diverged. Not
  touched — the instruction does not mention the reference and changing it would
  make the check agree with itself by construction.

## 3. What you should see

**Can a character margin exist that silences both empty captures and keeps all
three adjudicated callsigns, with the guard removed?**

**No.**

| | margin |
|---|---|
| the best character either empty capture produces | **4.50** |
| the weakest character of `VA3VRR` | **not read at all** |
| the weakest character of `N4L` | **not read at all** |
| the weakest character of `AA4MP/4 QNIK` | 16.73 |
| the weakest character of `KD0UN KD0UN K` | **1.75** |

A margin above 4.50 silences the noise. It also cuts **`KD0UN`** at 1.75 —
**this unit's own target, the capture the whole instruction was written
around.**

**And the question is smaller than it looks, which is the more important half.**
`VA3VRR` and `N4L` are **not read at all** once the scale is corrected, so no
margin can keep them: they are gone before any character is judged. A margin
chosen from the callsigns that survive would be chosen from a corpus that had
quietly shrunk by two.

### What the corpus reads with the guard bypassed

This is what the decoder is capable of right now, and it is the best this
repository has recorded:

| capture | window | read |
|---|---|---|
| `004507` | 6.96 | `E JJ AT ARRL DOT NET <BT> EACH STATION HANDLING THIS MESSAG E PE` |
| `003126` | 5.96 | `A OM <BT> E <BT> I WATCH AT L EAST 2 MOVI ES A DAY WID X# WHY NOT E E , `**`WESTERNS`**` , E` |
| `031905` | 4.93 | `TO . PREDICTED 10.7 K NTIMETER `**`FLUX`**` IS 125, 125N` |
| `003016` | 4.55 | `I<BT> HADA KPT15TT ITWAS #K <BT> ESTILL HVE MY ETO 91B TT JUST VFB TUB LIN` |
| `012403` | **1.10** | ` I E E E EE E E E E EEE E ADM UUT UD0 TN DEQ 6Q E `**`SQ DE KD0UN KD0UN K`**` ` |
| `003758` | 10.77 | `K I S QR L TU E EE AN E AN D E `**`AA4MP /4 QNI K`**` E EEEE E …` |

**`WESTERNS` where the old scale read `WESNRNS`. `FLUX` where it read `FLAX`.**
The corrected model reads *better*, character for character, on every capture it
still reads. **`012403` produces `DE KD0UN KD0UN K` — the text this unit was
commissioned to recover — at a window ratio of 1.10 against a guard of 15.**

### The two captures the local span did not fix, and made worse

| capture | before | after |
|---|---|---|
| `cw-2026-08-23-001520` | 1.4 × 10¹⁰ | **1.4 × 10¹⁶** |
| `cw-2026-08-17-013347` | 1.5 × 10⁸ | 1.7 × 10⁷ |

`001520` is 54.1 % exact digital zeros. A 2.5 s window can land entirely inside
that silence, where the quarter point is nought and σ falls to its 1e-9 floor —
so the local estimator makes the pathological case a million times worse, and
what is holding it is an arbitrary floor rather than a model. **No
percentile-based scale estimator can work on audio containing exact zeros**, and
that is a finding this unit produced rather than removed.

## 4. What's blocking us

---

**The outer guard must be re-expressed in the corrected units or removed, and
until it is, the decoder reads almost nothing.**

`Gate = 15` was calibrated against a scale that was 2.2× too small, so the bar is
now roughly five times too high. On the corrected model the captures that read
score between 1.10 and 10.77, and the guard is at 15.

**What the measurement supports.** With the guard bypassed the corpus reads
better than it ever has: `WESTERNS` for `WESNRNS`, `FLUX` for `FLAX`, and
`DE KD0UN KD0UN K` recovered. **What it does not support is a character margin
replacing the guard**, because the best noise character scores 4.50 and KD0UN's
weakest scores 1.75.

**Rejected: tuning the guard to a new number.** The instruction forbids it, and
the corpus still shows the same inversion in the new units — `014854` at 0.65,
`012403` at 1.10, `013622` at 0.20 with 55 emitted characters.

**Rejected: removing the guard on this measurement.** The instruction forbids it
and is right: removing it in the session that measured it is fitting the change
to the fixture that justified it. And silence would then rest on the character
margin, which the numbers say cannot hold it.

**Rejected: reverting the scale to make the tests green.** The corrected model
demonstrably reads better where it reads at all. The fault is the threshold, not
the correction.

**What I would want ruled**: whether to revert the scale for tonight and take
the correction with a replacement guard in the next unit, or to keep the
correction and accept a quiet decoder until the guard is settled. That is a
trade between a working radio this evening and a correct model, and it is Tim's.

---

**Two adjudicated callsigns are lost by the corrected scale, and that has not
been diagnosed.**

`VA3VRR` on `013347` and `N4L` on `134712` are no longer read on the whole-file
path. Both recordings still have pathological ratios — 1.7 × 10⁷ on `013347` —
so the scale estimator is still failing on them for a reason the local span did
not address.

This is the strongest argument against the correction as it stands, and it is
not the same argument as the guard. Named separately because fixing the guard
would not fix it.

---

**`LogLikelihoods` cannot be estimated from percentiles on audio containing
exact digital zeros, and `001520` proves it twice over.**

Whole-file it scored 1.4 × 10¹⁰; over a rolling span it scores 1.4 × 10¹⁶,
because a window can sit entirely inside 54 % digital silence. What stops it
being infinite is a 1e-9 clamp nobody derived.

Parked as `LogLikelihoods` was, but the instruction asked to say so if the local
scale made the honest figure free. **It did not — it made it worse**, and a
scale estimator that needs a floor to avoid dividing by nought needs a different
estimator rather than a better floor.

---

**The port and its reference implementation have diverged.**

`ItReadsWhatTheReferenceReads` exists so the C# port has an implementation to be
checked against rather than a description, and
`tools/reference-decoder/reference_decoder.py` still carries `P25 × 0.6` and the
Gaussian key-up. Not touched: the instruction does not mention it, and updating
it in the same session would make the check agree with itself by construction.

---

**`CLAUDE_CODE.md` changed its report contract without changing its version
line.**

The file went from five sections to four today while both copies read
"Version 1.3". §0 of that file names the version line as the only handle for
detecting drift. Reported because the next session comparing versions will be
misled exactly as this one nearly was.

### Asks still outstanding

Carried forward verbatim per HM-DEC-139 and HM-DEC-140. **Eleven inbound, none
ruled, the oldest open since 2026-08-14. Seven consecutive units have now worked
beside rulings they cannot read.**

1. **The sweep's `invented` column counts substitutions, not invented
   characters.**
2. **Whether the refill guard should apply to the first fill at all.**
3. **`ANNUNCIATOR.md` renamed `PHASE` to `TASK` while HM-DEC-150 makes `PHASE`
   match the version's minor.**
4. **`DECISIONS.md` has no record for HM-DEC-096–133, 136, 141 or 150.**
5. **The tone tracker is a large source of soup** — 22 invented against 0 at a
   fixed pitch.
6. **Whether the integrator ships at 45 Hz or 30 Hz.**
7. **The gate's calibration** — measured anti-correlated with correctness. *(This
   unit measured whether it can be removed. It cannot be replaced by a character
   margin; it now blocks everything.)*
8. **A boxcar's nulls made two of five swept offsets pathological best cases.**
9. **Two stations closer than 125 Hz are not named and the operator is not told
   they are not named.**
10. **The keying witness is correct in 5 of 13 captures.**
11. **HM-OPEN-057** (2026-08-22) and **HM-OPEN-007** (2026-08-14).

Unit 1.11.3's five: **the missing captures** *(closed by this unit's task 1)*;
**the outer guard needing replacement rather than re-tuning** *(measured here;
now the blocking item)*; **the lock helping sometimes and hurting sometimes**;
**the button added against instruction** *(left exactly as it was, per this
instruction)*; **`ElementsSeen` and `ElementsResolved` being one field** *(still
one field; the pair was not trusted here)*.

Plus this unit's five, above.

**Build 1.11.4**, confirmed in `Directory.Build.props`, up from 1.11.3.
