# Work instruction 001 — instrument the decoder, fix one defect, take the baseline

## 1. What Claude did

Claude Code on the development computer, `C:\Source\HamLet`. The prompt claimed
`PROJECT: Hamlet` and so does `WORK_INSTRUCTIONS.md`; the tree confirms it —
`SHACK_FACTS.md` and `src/Hamlet.RadioEngine/Cw/CwProbabilisticDecoder.cs` both
exist, neither `CoreHMI.sln` nor `MURC.sln` does, the solution is `Hamlet.sln`
and `PROJECT_CARD.md` names Hamlet. **Branch `main`**, per §9.5.1. Five tasks,
all five worked, none dropped. Every task committed and pushed to `main` before
the next began; all five pushes succeeded.

**Nothing in this report is evidence about the radio.** No rig was connected and
none of these figures comes from one.

**Nothing was recorded to `DECISIONS.md`.** Everything this unit found bears on
what the display asserts or on a model constant, and both are Tim's without
exception (§12.1).

### Task 1 — the four claims, traced

All four hold, with one correction to the instruction and one larger finding.

1. **Confirmed, and the line number is exact.**
   `CwProbabilisticDecoder.LogLikelihoods` forms its noise scale as
   `Percentile(sorted, 25) * 0.6` at `CwProbabilisticDecoder.cs:427`, with the
   amplitude from the 97th percentile on the line below. The baseline was taken
   over that model unchanged.

2. **Confirmed, every link.** `_refillHops` is declared at
   `CwProbabilisticStream.cs:107` and assigned at `:293`, inside `Restart()` and
   nowhere else. `Restart()` has exactly one caller in the whole tree,
   `CwDecoder.cs:488`, guarded by `ClearOnAStationChange` at `CwDecoder.cs:144`,
   which is `public const bool ... = false`. The guard it feeds is at
   `CwProbabilisticStream.cs:349`. A fresh stream therefore held nought and
   `_envelopeCount < 0` is never true, so the guard had never run on a first
   fill. Task 3 stood.

3. **Confirmed.** `CwProbabilisticStream.Read()` at `:380–384` sets `speed` to
   `measured.WordsPerMinute` when `measured.IsReady` and the value sits between
   `SlowestWpm` and `FastestWpm`, and passes it as `atWordsPerMinute` to
   `Decode`. In `Decode` at `:308–309`, `from` and `to` both take that value, so
   the grid collapses to one hypothesis.

4. **No such ruling exists, and the range the instruction could not read cannot
   be read here either.** The tree's `DECISIONS.md` holds full records for
   HM-DEC-001–095, 134, 135, 137–140 and 142–149. **It has no record at all for
   096–133, nor for 136, 141 or 150.** So the gap is not an artifact of the
   archive the instruction was written from; it is the state of the file. A
   search of `DECISIONS.md` and of `CLAUDE.md`'s index table found nothing
   mandating the measured-unit override or the refill guard's shape. Both were
   session-recorded engineering — the override arrived in `76b295c`, *measure
   the sender's dit instead of searching for it*. **The production default was
   not touched.**

### Mismatches between the instruction and the tree

Reported per the instruction's own requirement, including where the work
succeeded anyway.

- **`CW_REVIEW_BRIEF.md` is not in the tree.** The instruction cites its §1, §5,
  §7 and §10. `CW_CODE_REVIEW.md` is present at the root as stated; the brief is
  not, so none of the figures attributed to it could be checked. Everything this
  unit reports is measured here instead.

- **The HM-DEC-120 wording transcribed into the instruction is not HM-DEC-120.**
  The instruction quotes it as *"Nothing is emitted on audio holding no
  signal"*. `CLAUDE.md`'s index row reads: *"The refusal floor is 14 in the
  decoder's own margin units, superseding the 17 of HM-DEC-117's interim."* The
  full record is inside the missing 096–133 range and could not be read. **The
  property the instruction relies on is real** — the sweep's own numbers, and
  both empty captures staying silent — **but it is HM-DEC-097's and 120's sweep
  result rather than 120's ruling text**, and 120 is being cited for a sentence
  it does not contain. Nothing was done on the strength of the transcription
  alone.

- **`CLAUDE_CODE.md` §8 mandates five sections and `SESSION_PROTOCOL.md` §12.2
  mandates three headings.** Named by the instruction, and confirmed. §8's five
  sections are followed, per §0.

- **`ANNUNCIATOR.md` renamed `PHASE` to `TASK`** and says a file still writing
  `PHASE` keeps working. `PROJECT_STATUS.md` was on `PHASE: 10` and is now on
  `TASK: n of m`. **This collides with HM-DEC-150**, which says the status
  file's `PHASE` field and the version's minor are the same number read from one
  place. Under the rename there is no longer a phase field for the minor to be
  the same number as. Raised in section 4.

### Task 2 — the per-character span log-likelihood

Every character now carries the log-likelihood of its own span against
all-key-up over the same span.

Computed in `Spell`, which the cumulative sums already in `DecodeAt` make two
subtractions per mark. **The element gaps inside a character cancel exactly**,
because both hypotheses say the key is up during them, so the quantity reduces
to the marks. The Gaussian length penalty is deliberately excluded: it scores
how well a segment's duration matched the speed hypothesis, which is a statement
about the clock rather than about whether there was a signal there at all.

Carried on `CwProbabilisticCharacter`, threaded through
`CwProbabilisticStream.Character` onto `CwCharacter` as
`SpanLogLikelihoodRatio`, defaulting to `NaN` — which is not the same as nought,
since nought is a character all-key-up explains exactly as well.

**Written to the capture sidecar and to nothing else. No display changed.** Six
tests: real characters score large and positive on a known-text fixture, the
number falls away as the signal does, audio holding no station carries none, the
field reaches the sheet with each character beside it, a word gap is left out
rather than printed as nought, and an empty window says so rather than printing
an empty list.

**The suite was unchanged by this task** — the same 27 engine failures as the
green baseline.

### Task 3 — the refill guard, and what it moved

`_refillHops` is now set in the constructor exactly as `Restart()` computes it.

**It moved the numbers, and the movement is reported rather than tuned away.**
On the generated sweep the first three seconds are no longer read, and what was
in them was soup. At 18 dB, for `CQ DE W1AW K`:

```
before:  E KCTCGQQ N DEDE E WWAJ11AARW W N K
after:   Q N DEDE E WWAJ11AARW W N K
```

Eight characters that were never sent go, and **the message's own `C` goes with
them**, because it was inside the soup. The existing sweep scored the first line
1.00 correct and 0.00 wrong, and scores the second 0.78 and 0.11 — see section 4
for why that scoring is the more interesting half.

Three tests that passed at baseline now fail, all of them knife-edge ratchets on
audio tuned into mid-transmission, which is exactly the case the guard
suppresses the opening of:

| test | was | now |
|---|---|---|
| `CwSensitivityTests.TheDecoderReadsAsFarDownAsItDidBefore` | reached 80 % somewhere | never reaches 80 % |
| `CwAcquisitionWindowTests.AFastFistIsReadWithoutARunUp(25)` | 0.79 against a bar of 0.79 | 0.77 |
| `WhatBandwidthTheDecoderListensThroughTests.HoldingTheWindowLongInTimeReadsMore(003016)` | read more | 56 against 56 |

**And one moved the other way**: `cw-2026-08-20-014854`, a recording an
independent sweep says holds no keying, emitted one character before and emits
none now. That was a defect the suite carried under a ceiling rather than a
behavior it endorsed.

Three tests were added: a stream fed less than `RefillSeconds` reads nothing at
all, one fed more reads as it always did, and the constructor and `Restart()`
now hold back for the same length.

### Task 4 — the baseline table

`ANALYSIS-cw-baseline-2026-08-23.md` is committed at the repository root,
produced by `TheCwBaselineTable`, regenerable with one command printed at the
top of the file. **Every number in it was measured by code committed this
session; nothing is copied forward.** All nine captures are in it, not six — the
instruction said six and the corpus holds nine.

Two things the harness had to be corrected on before its numbers meant anything.

- **The witness split needs three rows, not two.** `KeyingVerdict` has a third
  state, `Listening`, which is the meter before it has formed a verdict at all —
  its first six seconds, and any stretch where it has not seen enough. Folding
  that into "no keying" reports an absence of evidence as evidence of absence.
  On `cw-2026-08-17-013347` it is 60 characters of 128.

- **The span LLR is comparable within a recording and not across recordings.**
  The per-hop difference works out at roughly the squared ratio of the signal
  amplitude to the noise scale, both estimated from the recording's own
  envelope, so a quiet recording produces enormous numbers rather than confident
  ones. That is said in the file rather than left for a reader to discover.

### Task 5 — the streaming gate *(the drop candidate, not dropped)*

Run, and appended to the analysis file. Split by what the witness said at each
read's own moment rather than once per file, because the question the gate has
to answer is whether *this window* holds keying.

### Version

`Directory.Build.props` read `1.10.13` and now reads `1.11.1`, as the
instruction specified. The stale semantic-versioning comment beside it was left
alone and named in `OPEN_ISSUES.md`.

### Parked items named rather than touched

Both the instruction named at packaging now have entries, neither previously
present: **HM-OPEN-058**, `FastestWpm`'s remarks argue for forty and the
constant is `32` (`CwProbabilisticDecoder.cs:153`); **HM-OPEN-059**, the stale
semver comment in `Directory.Build.props`, the same staleness in
`CHANGELOG.md`'s opening paragraph, and the changelog's newest entry being
`1.9.0` while the tree is at `1.11.1`.

Nothing else on the parked list was touched. `LogLikelihoods`, `Gate`,
`ClearOnAStationChange`, per-character gating, the `Skip()` splice wall,
`CwToneSurvey`'s valve and `CwUnitEstimator.Runs` are all as they were.

## 2. What Tim should expect

**In the application: nothing.** This unit changed what the decoder records, not
what it decodes, with one exception — the refill guard now runs, so the first
three seconds after listening starts produce no text where they used to produce
some. What they used to produce was soup, so the visible effect is a slightly
later first character and slightly less garbage before the real text.

A capture sidecar now carries one new field, `spanLlr`, listing each recent
character with its own evidence. Nothing else on the sheet moved.

**The build succeeds with no warnings.**

**The suite: 1536 passing and 31 failing of 1567 in the engine, 481 passing and
0 failing of 481 in the app.** The green baseline before this unit was 1533 and
27 of 1560 in the engine, 477 and 1 of 478 in the app.

**What will look wrong and is not:**

- **Three of those 31 are this unit's doing and are named above.** They are not
  regressions in what the decoder reads; they are ratchets that sat within two
  to five points of their bars, falling below them because the first three
  seconds of soup no longer count toward the score.

- **One of the 31 is flaky, not a failure.**
  `BroadcastWhileBusyTests.ABroadcastDoesNotAnswerTheCommandInFlight` failed in
  the full run and passes three times out of three in isolation. It is
  timing-sensitive under load. `TheFollowedSentenceReachesTheScreenTests.ItIsDrawnWhileRefillingAndGoesWhenTextResumes`
  is the same class — it failed in the baseline run and has passed in every run
  since, with nothing between them that touches it. Both are named and left
  (§12.6).

- **The remaining 27 are the baseline this unit inherited.** They were failing
  before a line was written and none of them was touched.

- **`ANALYSIS-cw-baseline-2026-08-23.md` reads badly, and that is the point.**
  It is a measurement of what the decoder does today, not a target. The
  transcripts in it are soup, and the whole reason for taking it is that nobody
  has been able to say how much soup, where it comes from, or whether a change
  helps.

Pushed to `main`, five commits, all pushed successfully.

## 3. What you should see

**The number this unit was commissioned to produce: the E-share inside and
outside the witness verdict, per capture.**

| capture | witness | characters | E-share | span LLR P10 / median / P90 |
|---|---|---|---|---|
| `013347` **`VA3VRR`** | said keying | 16 | **25 %** | −66 / 10 189 / 108 876 |
| | said no keying | 52 | **35 %** | 8.5e8 / 1.1e10 / 2.1e10 |
| | had not decided | 60 | **43 %** | 5.8e8 / 2.0e9 / 1.7e10 |
| `013622` | said keying | 0 | — | — |
| | said no keying | 38 | 37 % | 10 181 / 5.3e7 / 7.2e9 |
| | had not decided | 59 | 47 % | 2.0e8 / 2.0e9 / 1.1e10 |
| `134712` **`N4L`** | said keying | 18 | **72 %** | **−323 / −270 / 9 035** |
| | said no keying | 0 | — | — |
| `004507` | said keying | 103 | 16 % | 830 / 3 095 / 6 243 |
| | had not decided | 12 | 17 % | 485 / 2 095 / 2 519 |
| `003016` | said keying | 117 | 15 % | 526 / 1 760 / 4 690 |
| | had not decided | 17 | 0 % | 960 / 2 118 / 4 567 |
| `003126` | said keying | 112 | 14 % | 698 / 2 375 / 6 097 |
| | had not decided | 12 | 17 % | −210 / 3 385 / 6 264 |
| `003758` | said keying | 92 | 29 % | −332 / 5 293 / 10 902 |
| | had not decided | 14 | 21 % | −477 / 7 037 / 12 952 |
| `014854` *(holds nothing)* | — | **0** | — | — |
| `014935` *(holds nothing)* | — | **0** | — | — |

**Both empty captures emit nothing.** That is the one property that has never
been traded, and it holds — `014854` improved from one character to none.

**The split works, weakly, and in the expected direction.** On `013347` the
E-share is 25 % where the witness says somebody is keying and 35 % to 43 %
where it does not. On the four captures where the witness says keying for almost
everything, E-share sits at 14 % to 29 %.

**The evidence points the wrong way and that is the finding.** On `013347` the
characters emitted while nobody was keying score a median span LLR of eleven
*billion*, against ten thousand for the characters emitted while somebody was.
The quantity is dominated by the window's own noise estimate, and that estimate
collapses when there is nothing to estimate from. **Both this instrument and the
gate's likelihood ratio rest on `Percentile(sorted, 25) * 0.6`, which is exactly
what unit 002 is scoped to look at.**

**And on `cw-2026-08-17-134712`, the capture carrying the adjudicated `N4L`, the
median span LLR is −270.** Negative means the key never going down explains that
character's span better than the keying the decoder chose. Eighteen characters
came out, 72 % of them `E`, and they read `QQ  ET EKK  E  E E E  E  E E E  E  E
E`. The offline grid decode of the same audio reads nothing at all.

### The sensitivity sweep

| generated | correct | wrong | invented | emitted | invented share of what was read | read |
|---|---|---|---|---|---|---|
| 18 dB | 8.0 | 0.0 | **12.0** | 20.0 | **60 %** | `Q N DEDE E WWAJ11AARW W N K` |
| 11 dB | 8.0 | 0.0 | **12.0** | 20.0 | **60 %** | `Q N DEDE E WWAJ11AARW W N K` |
| 3 dB | 7.5 | 0.3 | **12.8** | 20.5 | **62 %** | `Q N DEDE E WWAJ11AARW W N K` |

The message is `CQ DE W1AW K`, nine characters, at eighteen words a minute at a
comfortable ratio.

**Sixty per cent of what the decoder emits at 18 dB was never sent, and the
existing sweep's `invented` column reads nought.** `CwRefusalFloorTableTests`
counts `CwMatchKind.Wrong` under that heading, which is a substitution at a
position where something *was* sent. `CwMatchKind.Invented` — a character
aligned against nothing at all — exists in `CwAlignment` and is counted nowhere
in the repository. Raised in section 4.

### The streaming gate

| recording | witness | reads | ratio P10 / median / P90 |
|---|---|---|---|
| `013347` | said keying | 11 | 17.7 / **34.1** / 251.6 |
| | said no keying | 24 | 6.0e6 / **6.3e7** / 1.1e8 |
| `013622` | said no keying | 35 | 3.0 / 6.2 / 2.9e7 |
| `134712` **`N4L`** | said keying | 18 | **1.7 / 1.8 / 2.1** |
| | said no keying | 17 | 2.2 / 2.3 / 2.4 |
| `004507` | said keying | 49 | 29.3 / 33.5 / 41.3 |
| `003016` | said keying | 49 | 21.0 / 26.2 / 28.9 |
| `003126` | said keying | 49 | 23.8 / 27.8 / 35.2 |
| `003758` | said keying | 49 | 21.3 / 50.0 / 61.7 |
| `014854` *(nothing)* | said no keying | 35 | 5.5 / 6.5 / 7.3 |
| `014935` *(nothing)* | said no keying | 35 | 2.7 / 3.3 / 3.8 |

**`Gate = 15` separates nothing on the instrument that actually gates.** The
highest ratios in the whole corpus — sixty-three million — come from windows the
witness says hold no keying. The lowest — 1.7 to 1.8 — come from the recording
carrying an adjudicated callsign, while somebody is keying, so a gate at fifteen
would refuse `N4L` outright. The 3-to-6 against 24-to-39 separation the gate was
set from was measured by the offline reference on whole files, and the streaming
windower does not reproduce it.

## 4. What's blocking us

Nothing blocks the next unit. Four questions want a ruling, most-blocking first.

---

**The sweep's `invented` column counts substitutions, not invented characters,
and the figure HM-DEC-120 was ruled on is therefore not a measurement of
invention.**

`CwRefusalFloorTableTests.Measure` accumulates its `invented` figure from
`m.Kind == CwMatchKind.Wrong`, which `CwAlignment` defines as *a different
character was sent here*. `CwMatchKind.Invented` — *nothing was sent here at
all* — is defined in the same file, produced by the same alignment, and read by
no test in the repository. So a transcript that is entirely characters which
were never on the air scores nought invented, and the sweep printed exactly that
for months: `E KCTCGQQ N DEDE E WWAJ11AARW W N K` for `CQ DE W1AW K`, scored
1.00 correct and 0.00 invented. Counted properly, twelve of the twenty
characters at 18 dB were never sent.

**What that does and does not overturn.** It does not overturn the property
itself — both empty captures are silent and always have been, which is a
different measurement and an honest one. It overturns the *sweep's* invention
figures, which are the ones a floor was chosen against, and the choice between
seventeen, fifteen, fourteen and thirteen rested on all of them having the same
worst invented share, which is none. Under a column that counts invention, that
comparison has not been made.

**Rejected: fixing the column inside this unit.** It changes what every
sensitivity number in the repository means, which is a ruling and not an edit,
and this unit's whole value is that the review's diagnosis and its own numbers
describe the same code.

**Rejected: reporting it only as an open issue.** A floor was ruled on the
strength of the figure, so the correction belongs where the ruling can be
revisited.

---

**Whether the refill guard should apply to the first fill at all, or only to a
refill after the window has been emptied.**

Task 3 did what it was ordered to do and the mechanism is exactly as the
instruction described. But the field's own documentation scopes it narrowly:
*"How much audio the window has to hold again after being emptied before
anything is read from it"*, and the comment at the guard site reasons entirely
about a station change. So initializing it in the constructor is arguably a
widening of the guard rather than a repair of it, and it costs three tests.

**What is measured either way.** With it, the first three seconds of every
session produce nothing; the soup they used to produce goes, and one real
character of `CQ` goes with it, and `cw-2026-08-20-014854` stops emitting the
one character it should never have emitted. Without it, that soup comes back and
the three ratchets go green.

**Rejected: tuning `RefillSeconds` down until the ratchets pass.** The
instruction forbids it and it would be choosing a constant to make a number look
right, which is how the fixture faults in §12.5 happened.

**Rejected: keeping the fix and lowering the three bars.** A bar lowered to
accommodate a change is not a bar.

---

**`ANNUNCIATOR.md` renamed the status file's `PHASE` field to `TASK`, and
HM-DEC-150 makes `PHASE` the same number as the version's minor.**

`ANNUNCIATOR.md` line 70 says the field used to be called `PHASE`, that `PHASE`
still reads, and that a session should write `TASK`, meaning the task within
this prompt. HM-DEC-150 says the minor version *is* the phase number and that
`PROJECT_STATUS.md`'s `PHASE` field and the minor are the same number read from
one place, so they cannot drift.

Under the rename there is no field for the minor to be the same number as, and
`TASK: 5 of 5` is a different quantity entirely. This session wrote `TASK` per
the annunciator and the prompt, and the version is `1.11.1` per the instruction,
so the two happen to agree by hand rather than by construction — which is the
drift HM-DEC-150 exists to prevent.

**Rejected: writing both fields.** Two numbers claiming to be the phase is the
second copy §0 forbids, and the panel would show whichever it was built for
without saying which.

---

**`DECISIONS.md` has no record for HM-DEC-096 to 133, 136, 141 or 150, and
`CLAUDE.md`'s index has rows for all of them.**

Forty-one rulings exist as one-line index entries with no record behind them.
That includes HM-DEC-120, which this unit was instructed not to re-argue and
whose text could not be read; HM-DEC-114, which sets the bar a loud clean signal
must clear; and HM-DEC-150, which governs the version this unit bumped.

The index rows are substantial — several hundred words each — so the loss is the
`DECISIONS.md` fields, the supersession chain and the rejected alternatives,
rather than the rulings themselves. **A session cannot tell a ruling it is
acting against from one that does not exist**, which is the failure §9.5 names.

**Rejected: reconstructing the records from the index rows.** A ruling is never
edited (§1), and composing a record from a summary and dating it is manufacturing
a source.

### Asks still outstanding

Carried forward verbatim until ruled, per HM-DEC-139 and HM-DEC-140.

The four asks above are this session's and are made for the first time here.

**No queue was carried inbound.** `WORK_INSTRUCTIONS.md` does not carry an
`Asks still outstanding` list, which §9.6 makes a defect in the order rather
than in the session. Per HM-DEC-139 the queue was reconstructed from
`OPEN_ISSUES.md` and the previous report:

- **2026-08-22 — the E-dominance outside the keying verdict** (`HM-OPEN-057`,
  owner `tim`, `severity: slows`). Waiting on a ruling about whether the
  fragmenting signature can be read at all. **This unit measured it again with a
  per-character instrument and the answer is in section 3**: the split holds and
  the evidence behind it points the wrong way. The entry stays open because
  nothing was ruled.

- **2026-08-14 — `HM-OPEN-007`** has been open and unruled since that date. It
  is named here because HM-DEC-140 rests on `OPEN_ISSUES.md` genuinely being
  swept, and an item nine days old is the evidence that it is not.

`OUTPUT.md` from the previous session was overwritten by this one, so any ask it
carried that is not in `OPEN_ISSUES.md` is gone. That is the failure HM-DEC-139
was written for, and it has happened once more here.

## 5. Where the phase stands

**Phase: E-share in single figures across the corpus — not reached, and now
measured for the first time.** E-share among emitted letters runs 13 % to 43 %
across the nine captures, and 14 % to 72 % inside the witness's keying verdict.
Single-character-word share runs 35 % to 79 %. The three adjudicated readings
are not intact: `VA3VRR` survives inside a hundred and twenty-eight characters of
soup, `N4L` does not appear at all, and `AA4MP/4 QNIK`'s recording is
unobtainable (HM-DEC-126). Both empty captures are silent. Invention above 3 dB
on the sensitivity sweep is 12 characters out of 20 emitted, not nought.

**Was: unmeasurable — no per-character instrument existed.**

**Build 1.11.1**, confirmed in `Directory.Build.props`, up from 1.10.13.
