PROJECT: Hamlet
ISSUED: 2026-08-19

## Asks still outstanding (inbound, per HM-DEC-139)

| Ask | First made | Waiting on |
|---|---|---|
| **Whether an attended automatic cycle may reach an antenna** (§0.2, HM-DEC-098) | 2026-08-17 | The bench evening; `BENCH_CARD.md` can be followed end to end |
| **A callsign too long for one keyer send** (HM-DEC-130) | 2026-08-18 | The seam measured at the bench, from the send panel |
| **Whether the star asks for a name at the moment of saving** (HM-DEC-060, HM-DEC-134) | 2026-08-18 | Nothing but the ruling |
| **Whether Hamlet may ever ask the radio to send its spectrum** (HM-DEC-062, HM-OPEN-042) | 2026-08-18 | The ruling |

**HM-OPEN-048 is ruled and is this order.** Dropped from the queue.

---

# Work order — the settled transcript speaks, and says what it did not measure

**Five phases. Phase 5 is the one to drop.**

Gate first (HM-DEC-099). Write `PROJECT_STATUS.md` now, at every phase boundary,
and at the finish.

**The operator is at his radio tonight**, finding live CW Hamlet cannot read and
holding conversations. Phases 1 to 3 are what he needs. Phase 4 is a regression
that will cost him accuracy on real off-air copy. Judge everything against
whether he can read it, answer it, or keep it.

---

## Phase 1 — Record the ruling

Write to `DECISIONS.md` at the head, verbatim. Next free id is **142** — 141 was
taken by the top-strip renumber this morning, 136 is deliberately absent, and 105
is a ruling whose entry is missing.

```
---
id: HM-DEC-142
date: 2026-08-19
refs: src/Hamlet.RadioEngine/Cw/CwGapFit.cs, src/Hamlet.RadioEngine/Cw/CwSettledPass.cs, HM-OPEN-048, HM-DEC-115, HM-DEC-114, HM-OPEN-017
---

**When the sender leaves too few word gaps to form a third class, the settled pass
emits the characters it read, unspaced, and says on the transcript that word
spacing was not measured.** Closes HM-OPEN-048. **Narrows HM-DEC-115 to the case it
was ruled for** and overturns none of it.

WHAT IS THERE IS NOT A GUESS. HM-DEC-115 says no cuts means no transcript rather
than a guessed one, and that is right and stays. **This is not that case.** On
`coverage-easy` there are eighty gaps, clustered from the sender's own keying, and
the clock fits at a hundred milliseconds. Two of the three classes come back
populated. What is missing is the word class, and it is missing because the
operator sent a callsign without spaces — which is a fact about his sending and not
a failure to measure it. A ruling written against having nothing was reaching into
a case where we have almost everything.

AND THE CURRENT BEHAVIOUR IS THE ONE THAT FAILS §0.0. Two hundred and fifty-eight
windows read successfully on a fixture the reference reads at a hundred per cent,
and the transcript is empty. **An empty box says nothing was sent.** That is a
belief formed from the screen that is not true, and it is today's behaviour rather
than a risk of changing it. A ham reads `CQCQDEW4AWHK` without difficulty. Nobody
reads a blank.

THE SPACING IS NOT INVENTED, AND THE TRANSCRIPT SAYS SO. Emitting unspaced asserts
no word boundary anywhere, which is exactly what was measured. **Clustering two
heaps and calling the wider class a word gap was rejected**: this fixture has two
or three genuine word gaps, and folding them into the character class would place
spaces that were never measured, which is the guess HM-DEC-115 forbids. The
sentence on screen is the load-bearing part and not a caveat — it is the
difference between an odd-looking transcript and a stated condition.

MEASURED BEFORE IT SHIPS. Two classes must still separate element gaps from
character gaps reliably, or the transcript runs a callsign together and reads as
confident nonsense, which is worse than the silence it replaces. **If the
measurement says it does not separate them, none of this ships and the finding
comes back.** HM-OPEN-017's labelled approximation stays reserved and unused.

THE LEADING EDGE IS UNTOUCHED. It was always right on these fixtures and it is what
the operator watches arrive. This ruling is about the record he keeps afterwards.
```

Index row at the true head of `CLAUDE.md` §1 — which now reads newest-first and has
a test on it, so put it at the top and let `DecisionLogOrderTests` confirm.

## Phase 2 — Build it

- `CwGapFit` stops refusing when the word class is empty **and the other two are
  not.** An empty element or character class is still a refusal; those are the
  measurement genuinely failing.
- `Emit` produces the characters it read, with no word boundaries.
- **The transcript states the condition, in the operator's own terms, through
  `VoiceTests`.** Not a log line, not a tooltip. He is reading the transcript; the
  sentence belongs where he is looking. Something to the effect that the sender left
  no spaces long enough to measure, so the letters are right and the word breaks are
  not shown.
- Distinguish this in the record from a genuine refusal, so `decode_quality` can
  tell an unspaced emission from an empty one.

## Phase 3 — The gate: measure it before it ships

**This is not a test-writing phase. It is the condition on phase 2 shipping at
all.**

- `coverage-easy` and `exchange-easy` against the reference, which reads both at
  100%. Report character accuracy, not a pass or fail.
- **Specifically: does a callsign run together?** Two classes must separate element
  from character gaps. Take a fixture with a callsign in it and read the output
  character by character against the key.
- `CwSettledSilenceTests.APassThatReadSomethingEmitsSomething` — the third red the
  last session added deliberately — goes green. **Standing red returns to exactly
  two**, and if it does not, say which and why.
- **If accuracy is poor, revert phase 2 and report.** A transcript that reads
  `W4AWH` as `W4AW H` or `WHAWH` is HM-DEC-114's defect, not an improvement, and the
  ruling above says in terms that it does not ship.

## Phase 4 — The bulletin regression

`TheBulletinDecodesToItsAnswerKey` emits `NLDOTNET■IECHSTAAIONHAND■AHISMESAGEP`
against a key beginning `RLDOTNET<BT>EACHSTATIONHANDLING…`. **HM-DEC-115's own text
records that recording being read correctly.** It has since degraded, on a real
off-air Farnsworth capture, which is exactly the kind of signal tonight is about.

- Find when. `git bisect` against that test if it runs at older commits.
- **Name the change and the mechanism**, not just the commit. This project has been
  burned twice this week by a diagnosis that named a suspect without naming the
  line.
- Repair if the repair is clear. If it needs a ruling, hand it back.

## Phase 5 — `ClearingTheTranscriptLeavesTheDecoderAlone` (DROP THIS ONE IF SHORT)

The remaining standing failure, left red by HM-DEC-114. Phase 2 or 4 may move it;
check before doing separate work.

**Drop this whole if short and say you dropped it.**

## Named and left (§12.6)

The four unruled asks above. **No transmit work toward auto-CQ** — HM-DEC-098 is
unruled and dummy-load only.

## Reporting

`OUTPUT.md`, four sections (HM-DEC-106), section four carrying the asks queue.

**Section two is written for a man at his radio tonight**, and leads with one
sentence: whether the settled transcript now shows him what he heard. Then what
the letters are worth — the accuracy number from phase 3, plainly, because he is
going to trust that text or not based on it.

**If you finish every phase, stop and report.**
