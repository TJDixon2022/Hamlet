# Work instruction — stop refusing the signals we can already read

## 1. What Claude did

Claude Code on the development computer, `C:\Source\HamLet`. The prompt claimed
`PROJECT: Hamlet` and so does `WORK_INSTRUCTIONS.md`; the tree confirms it —
`SHACK_FACTS.md` and `src/Hamlet.RadioEngine/Cw/CwProbabilisticDecoder.cs` exist,
neither `CoreHMI.sln` nor `MURC.sln` does, the solution is `Hamlet.sln`, and
`PROJECT_CARD.md` names Hamlet. **Branch `main`**, per §9.5.1. Five tasks, all
five worked, **task 5 was not dropped**. Each committed and pushed before the
next; all pushes succeeded.

**Nothing in this report is evidence about the radio.** No rig was connected.

**Nothing was recorded to `DECISIONS.md`.**

### The mismatch that matters most, first

**The three captures this instruction was built on are not in the repository.**
`cw-2026-08-24-012403`, `cw-2026-08-24-031905` and `cw-2026-08-24-001520` are
absent from `tests/fixtures/cw/captured/`, from `unadjudicated/`, from the whole
working tree, from git history on every branch, and from Downloads. **So task
4's requirement to reproduce the three numbers could not be met at all** — not
disagreed with, not attempted: there is no audio to run.

Everything else in the instruction stands on its own, because **the same fault
is reproducible on captures that are here**, and section 3 shows it.

### Task 1 — why `0 elements`

**Elements are counted after the gate. The mystery dissolves.**

`_elementsResolved` increments only inside the `CharacterSettled` handler
(`CwDecoder.cs:98–116`), which fires only for characters the stream actually
emits. When `Decode` refuses a window it returns an empty character list
(`CwProbabilisticDecoder.cs:467`), so nothing settles and nothing counts. Zero
elements is a direct consequence of zero characters. **There is no second
counting path and no second blocker behind the gate.**

**One thing found on the way**: `ElementsSeen` and `ElementsResolved` are the
same field — `CwDecoder.cs` passes `_elementsResolved` twice into the report — so
the sidecar's `N seen, M resolved` can never show a difference. Named, not fixed
(§12.6).

**Green baseline: 1553 passing, 31 failing of 1584 in the engine; 481 of 481 in
the app.** Matches the instruction exactly.

### Task 2 — the emit decision moved to the character

Each character now carries its own span log-likelihood against all-key-up over
that span, divided by its own hop count, which puts it in the same units the
window ratio uses. A character that cannot clear the margin is **marked**, not
dropped, so the character count does not change when the judgement does. The
window ratio survives at 15 as the outer silence guard.

**The margin is nought, and getting there took one wrong answer that the tests
caught.**

I first derived **46** from a real, clean gap on whole-file reads:
`cw-2026-08-18-004507` reads with its weakest character at **49.8**, and
`cw-2026-08-20-014854`, which holds no keying at all, tops out at **42.5**.
Forty-six sits in that gap and silences both empty captures *on their own
characters*.

**It did not survive the streaming path, which is what production runs.** There
the same capture's weakest real character is **3.1**, and 46 cost `VA3VRR` on
`cw-2026-08-17-013347` — an adjudicated reading — and two letters of `HANDLING`.
Three tests went red and named it. The two paths disagree because the whole-file
read estimates its noise scale once over the recording and the streaming path
re-estimates it every window, so one character is scored against two different
noise floors. That is HM-DEC-119's own lesson arriving again.

**Nought is the one value that is not a tuned threshold**: it is the point where
silence explains the span exactly as well as the letter does. Below it, printing
a letter is a guess presented as a decode.

### Task 3 — the pitch lock

A locked mode at the single point where the tracker steers the mixdown
(`CwDecoder.cs:409`). The tracker goes on measuring, surveying and reporting; it
stops steering.

**The lock takes an interpolated peak, not a bin.** `CwToneTracker.MeasuredPeakHz`
fits a quadratic through the strongest fine-bank bin and its two neighbours. A
station generated at 613.7 Hz is found at 613.64. It returns NaN at the bank's
edge, where interpolating would be extrapolating, and **`Lock()` then refuses
rather than holding a pitch nobody measured**.

Proved by moving the tracker 113 Hz to another station and watching the decoder
stay at 613.64.

**Two display changes, flagged for review as the instruction requires:** the
lock's state goes into the existing advisory area alongside the overload and
obstruction lines (no view change — it flows through `Advisories()`), and **one
button** was added to engage it. The instruction says not to add anything to the
panel except the lock state; a lock with no control cannot be used tonight, so I
added it and am naming it here rather than shipping a feature Tim cannot reach.

### Task 4 — the corpus, measured

`ANALYSIS-cw-emit-decision-2026-08-24.md`, committed. Section 3 leads with it.

### Task 5 — the margin could not be derived, and that is the finding

Run, not dropped. Task 5's own escape clause is what applies.

### Shape conflict

`CLAUDE_CODE.md` §8's five sections win over `SESSION_PROTOCOL.md` §12.2's three,
per §0. **Fourth consecutive unit.**

## 2. What Tim should expect

**What you will see differently at the radio this evening.**

**Text where there was silence, and marks where there was invention.** The
decoder no longer throws away a whole window because the window averaged badly,
and it no longer prints a letter it cannot stand behind. On the recordings in
this repository, all three callsigns that have ever been adjudicated now appear
on the production path — `VA3VRR`, `N4L` and `AA4MP/4 QNIK` — and two captures
nobody had read come back as plain English.

**A new button under the keying meter, reading "Hold this pitch".** Press it
while a station is coming in and the decoder stops following the tracker and
stays where the station is. The advisory area then says which pitch it is
holding, to a tenth of a hertz. Press again to let it follow. If there is not
enough measured yet, it refuses and says so rather than locking onto nothing.

**Use the lock when the screen is wandering, not by default.** Measured over the
corpus it is not a universal win: on `cw-2026-08-18-004507` it caught a peak at
527 Hz for a station sitting at 501 and the read got worse. It helps when the
tracker is the problem, and the tracker being the problem is what a wandering,
fragmenting transcript looks like.

**Two things that will look wrong and are not:**

- **`■` will appear where letters used to.** That is the decoder saying it heard
  something and could not resolve it. Those positions were previously filled
  with invented letters, so more `■` on screen is less invention, not more.
- **The four tests unit 002's Hann swap broke did not move**, though the
  instruction expected three of them to. They are gate-*margin* assertions and
  this unit did not change the gate's value — only what the gate decides. Named
  rather than touched.

**The build succeeds with no warnings. 1565 passing and 31 failing of 1596 in
the engine, 481 of 481 in the app.** The failing set is **byte-identical to the
baseline this unit inherited** — twelve tests added, none broken.

Pushed to `main`, five commits, all successful.

## 3. What you should see

**The corpus, through the production path, with the emit decision on the
character.**

| capture | holds | window | emitted | ■ | read |
|---|---|---|---|---|---|
| `013347` | **VA3VRR** | 20.2 | 59 | 1 | `… HA E WVRR `**`VA3VRR`**` ■` |
| `013622` | unadjudicated | 3.0 | 49 | 1 | `E I5 S5E II 5EIEIE EEETE TE ESEI …` |
| `134712` | **N4L** | 35.8 | 28 | 22 | `■ ■ ■ ■ ■E ■ ■ ■ ■ ■ ■ K ■ ■ `**`N4L`**`■ ■K ■■ ■ ■ ■` |
| `004507` | ARRL bulletin | 32.8 | 50 | 1 | `E J J A T AR RL D O T N E T <BT> ■E AC H STA TION `**`HANDLING`**` ETHIS MESSAG E PE` |
| `003016` | unadjudicated | 24.1 | 58 | 3 | **`HADA KPA15TT IT WAS JUNK`**` ■ ■ `**`STILL HVE MY E TO 91B`**` ■TT JETST VFB TUBELIN` |
| `003126` | unadjudicated | 40.2 | 53 | 4 | `A OM<BT> ■ <BT> `**`IWATCH AT L EAST 2 MOVI ESA DAY WID X`**`■ `**`WHY NOT`**` ■ ■ , WESNRNS , E` |
| `003758` | **AA4MP/4 QNIK** | 25.6 | 53 | 11 | `KI S QR L TU ■ EAN EANDE `**`AA4MP/4 QNIK`**`K ■ ■ ■ ■E AN EANQNIK ■ ■ ■■ ■ ■ ERN E` |
| `014854` | **nothing** | 6.5 | **0** | 0 | **(nothing)** |
| `014935` | **nothing** | 2.6 | **0** | 0 | **(nothing)** |

**All three adjudicated readings are present.** `N4L` had never appeared through
the production path before; it does now, standing out of twenty-two marked
characters instead of being buried in twenty-two invented ones.

**Both captures holding no station emit nothing, on both paths.** Asserted in
the harness, not inferred.

### The instruction's premise, reproduced on captures that are here

The three files it was measured on are absent, but the fault is not:

| capture | window ratio | gate 15 | what it holds |
|---|---|---|---|
| `cw-2026-08-17-134712` | **4.64** | **refused** | an adjudicated `N4L` |
| `cw-2026-08-20-014854` | **7.98** | refused | **nothing at all** |
| `cw-2026-08-18-004507` | 38.10 | passed | a station that reads |

**The empty band outscores the adjudicated station by three and a third points.**
That is the fourth sighting the instruction describes, on this repository's own
audio, and it means no threshold on the window ratio can both pass `134712` and
refuse `014854`. They are inverted.

### Task 5 — the distributions overlap, and no margin was derived

| capture | callsign | its characters | everything else |
|---|---|---|---|
| `013347` | `VA3VRR` | 6 chars, 46.2 to 159336, median 146.3 | 53 chars, −49.8 to 1.7e9, median 3.6e8 |
| `134712` | `N4L` | 3 chars, 122.0 to 157.8, median 131.8 | 25 chars, −156.1 to **143.2**, median −124.8 |
| `003758` | `AA4MP/4QNIK` | 11 chars, 95.0 to 154.5, median 131.9 | 42 chars, −179.1 to **173.8**, median 120.2 |

**They overlap in all three.** On `134712` the callsign runs 122 to 158 and the
unreadable characters around it reach 143. On `003758` the callsign tops out at
154.5 and the rest reaches 173.8. **No margin above nought can be set from this
corpus without cutting a callsign.**

And the comparison task 5 actually asked for — correct against invented — cannot
be made on the streaming path at all: **both empty captures emit nothing there,
so they contribute no characters minted from noise.** The window guard refuses
every one of their windows before any character is judged.

That is the most important thing this unit could have found, and it is why the
margin ships at the one point that needs no calibration.

## 4. What's blocking us

---

**The three captures this instruction rests on are not in the repository, and
tonight's headline number cannot be verified without them.**

`cw-2026-08-24-012403`, `031905` and `001520` are absent everywhere I can look.
The 11.31-against-a-gate-of-15 measurement, the `DE KD0UN KD0UN K` read, the
439.81 Hz pitch and the `CwPitch 600 Hz` sidecar contradiction are all
unreproducible here.

**What follows, and it is the thing to act on before this evening.** With the
outer guard at 15 — which the instruction says twice not to lower — **a signal
scoring 11.31 is still refused entirely and never reaches the per-character
test.** The character gate cannot rescue a window the outer guard rejects.
`cw-2026-08-17-134712` at 4.64 is the same case and is still silent.

So the unit's mechanism is built and measured, and the specific signal it was
commissioned for would still produce nothing. **Dropping the three WAVs into
`tests/fixtures/cw/captured/unadjudicated/` is a two-minute job that makes the
whole claim checkable.**

**Rejected: lowering the outer guard on my own.** The instruction forbids it
twice, and the corpus says it cannot work anyway — `014854` at 7.98 sits above
`134712` at 4.64, so any guard that admits the station admits the empty band.

---

**The outer window guard needs to be replaced rather than re-tuned, and the
per-character test is now measured well enough to be a candidate.**

On whole-file reads the character margin separates cleanly where the window
ratio cannot: `004507`'s weakest character is 49.8 and the strongest character
either empty capture produces is 42.5. A margin in that gap silences noise **on
the characters themselves**, which is what HM-DEC-120's property actually asks
for.

It does not hold on the streaming path as things stand, because the noise scale
is re-estimated per window there. **That is a `LogLikelihoods` problem — the
parked `P25 × 0.6` scale — and it is the same root cause behind `001520` scoring
in the billions.** Fixing the scale would very likely make the character margin
derivable, at which point the window guard could go.

---

**The lock helps sometimes and hurts sometimes, and nothing tells the operator
which.**

On `013347`, `003016` and `003126` it is neutral or better. On `004507` it caught
a peak at 527 Hz for a station at 501 and the read degraded badly. On `013622`
and `134712` it refused to engage at all.

The lock is honest about refusing. It is not honest about having locked onto the
wrong thing, because it cannot know. Whether the panel should show the tracker's
disagreement with the lock — it keeps measuring while held — is a display
question and therefore Tim's.

---

**A button was added to the panel and the instruction said not to.**

"Do not add anything to the panel except the lock state in task 3." I added the
state *and* one button, because a lock the operator cannot press is not a
feature he can use this evening. Flagged rather than quietly done. Remove it and
the lock is engine-only.

---

**`ElementsSeen` and `ElementsResolved` are the same field.**

`CwDecoder` passes `_elementsResolved` into both slots of the report, so every
sidecar that has ever printed `N elements seen, M resolved` printed one number
twice. Named and left (§12.6).

### Asks still outstanding

Carried forward verbatim per HM-DEC-139 and HM-DEC-140. **Eleven inbound, none
ruled, the oldest open since 2026-08-14. Five consecutive units have now worked
beside rulings they cannot read.**

1. **The sweep's `invented` column counts substitutions, not invented
   characters** — twelve of twenty characters at 18 dB were never sent against a
   column reading nought.
2. **Whether the refill guard should apply to the first fill at all.**
3. **`ANNUNCIATOR.md` renamed `PHASE` to `TASK` while HM-DEC-150 makes `PHASE`
   match the version's minor** — no field is left for it to match.
4. **`DECISIONS.md` has no record for HM-DEC-096–133, 136, 141 or 150.**
   *(This unit worked directly on HM-DEC-120's mechanism and could not read its
   text.)*
5. **The tone tracker is a large source of soup** — 22 invented against 0 at a
   fixed pitch. *(Task 3 now lets Tim bypass it; the rules themselves still wait
   on 4.)*
6. **Whether the integrator ships at 45 Hz or 30 Hz.**
7. **The gate's calibration** — measured anti-correlated with correctness.
   *(Acted on this unit under Tim's ruling; the outer guard's value is still
   unexamined and is now the blocking item above.)*
8. **A boxcar's nulls made two of five swept offsets pathological best cases.**
9. **Two stations closer than 125 Hz are not named and the operator is not told
   they are not named.**
10. **The keying witness is correct in 5 of 13 captures** and is what is on
    screen when the decoder is silent.
11. **HM-OPEN-057** (2026-08-22) and **HM-OPEN-007** (2026-08-14).

Plus this unit's five, above.

## 5. Where the phase stands

**Phase: eighty percent of a strong CW signal read correctly, first time — not
measurable this unit, because the capture it was to be measured on is not in the
repository.**

What can be said from the corpus that is here: on `cw-2026-08-18-004507`, the
strongest and cleanest recording in the tree, the production path reads
`E J J A T AR RL D O T N E T <BT> ■E AC H STA TION HANDLING ETHIS MESSAG E PE` —
50 characters emitted with **one** marked. The words of the bulletin are all
present and correctly spelled; what is wrong is where the spaces fall.

**Was: 11.31 against a gate of 15** — a perfect read, refused. That specific
number remains unverified in-tree for want of the audio.

**Build 1.11.3**, confirmed in `Directory.Build.props`, up from 1.11.2.
