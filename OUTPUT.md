# 1. What Claude did

**STATE.** Development computer, `C:\Source\HamLet`, Claude Code surface (§9.5).
**Branch: `main`.** Prompt, `WORK_INSTRUCTIONS.md` and `PROJECT_CARD.md` all say
`PROJECT: Hamlet`; gate passed on all three (§9.6). **No radio was connected**
(HM-DEC-093). Status written at the start and at every phase boundary.

**Phases 1 to 4 worked. Phase 5 dropped, checked first.** Three commits, and
**nothing in `src/` changed** — both repairs were measured and reverted.

## Phase 1 — it was a defect, and the numbers settle it

**You were right to make me measure before asking.** The last session handed the
`coverage-easy` question back as a ruling on the reasoning that the classes might
be element and character-or-longer. That reasoning was wrong. The eighty gaps are:

| Duration | How many | What it is |
|---|---|---|
| 110 ms | 61 | element, one dit at twelve words a minute |
| 310 ms | **17** | **character**, three dits |
| 710 ms | **2** | **word**, seven dits |

**All three heaps are present and textbook spaced.** There was never a question
about what the transcript should assert. The fit refuses because the three seeds
land on 110, 110 and 310 — the quarter, three-quarter and nineteen-twentieth marks
— and with element gaps three quarters of everything a sender produces, **the
middle seed starts inside the element heap** and its cluster empties.

**Two repairs were tried and each traded one green test for another.** Moving a
collapsed seed to the first value standing clear took `coverage-easy` from **0
settled characters to 14**, with all three classes fitted and both word gaps found
— and cost the clean recordings their word space: `CQDE W1AW K` for
`CQ DE W1AW K`, on two fixtures plus the training-radio and sample-rate tests.
Re-seeding at the point farthest from both neighbours fixed those and broke the
two-class gate instead, because with genuinely two heaps it manufactures a third.

**Both reverted.** What is needed is one rule that rescues a collapsed middle seed
*only where a third heap exists*, which is the fit's own job rather than a patch at
the seeds. Not a ruling ask — work, with every fixture adjudicated.

## Phase 2 — the mechanism, and the line

**A dit is created, not moved.** Every settled character of the bulletin capture
was dumped with its pattern. `STATION` comes out `S T A A I O N`, and the second
T's pattern is literally `.-` — a dit **before** the dah. `THIS` the same. `<BT>`
comes out `.-...-`. Element counts: fifteen where fourteen were sent, seventeen
where sixteen were. **Exactly one extra element each time and never a missing one**,
so nothing crosses a boundary — a mark is being split.

**The fragments, measured across the whole recording**, in ten-millisecond buckets:

| ms | 20 | 30 | 40 | 50 | **60** | 70 | 90–150 | **160** | 170 |
|---|---|---|---|---|---|---|---|---|---|
| count | 475 | 275 | 325 | 425 | **6,841** | 1,774 | ~175 | **4,199** | 574 |

The dit is 60 and the dah 160. **1,075 marks sit between 20 and 50 ms** — a seventh
of everything — and every one is classified as a dit, because `ClassifyMark` cuts
at their geometric mean, about 98.

**The line is `CwSettledPass.Deglitch`:**

```
var width = Math.Max(1, (int)Math.Round(shortestSeconds / _hopSeconds));
```

It is a median filter, and **a median filter removes runs shorter than half its
window.** With a ten-millisecond hop, twenty milliseconds gives a three-hop window
and removes ten; four tenths of a dit — twenty-four milliseconds at twenty words a
minute — also rounds to three hops and also removes ten. **Both passes remove ten
milliseconds whatever they are asked for**, so a 25-to-50 ms fragment survives both
and becomes a dit.

## Phase 3 — the repair is one line, and it is a trade

Widening the window to `2n + 1`, so the filter removes what it is asked to:

| Capture | Before | After |
|---|---|---|
| ARRL bulletin | 36 of 47 — `NL DOT NET ■I ECH STAAION HAND■ AHIS MESAGE P` | **37 of 47** — `NL DOT NET ■E ECH STATION HANDNG AHIS MESAGE P` |
| `cw-2026-08-17-013347` | the callsign and the characters before it | `■■■■VA3VRR` — callsign intact, **four characters ahead of it become placeholders** |

**`STAAION` becomes `STATION`. `HAND■` becomes `HANDNG`.** `AHIS` survives, so one
of the two substitutions has a second cause.

The cost fails `TheSettledPassNoLongerStopsShortOfTheCallsign`, a ratchet an
earlier phase set deliberately on a real capture. **Placeholders are honest where a
wrong letter is not** (§0.0), so this is not obviously a loss — which is exactly
why it is not mine to call. It is a trade between two real off-air recordings,
§12.1 clause 3. **Reverted; the tree is unchanged.**

## Phase 4 — HM-DEC-115's cited fact, recorded not amended

That ruling says the bulletin read every character correctly after acquisition. The
test written the next day showed 36 of 47, the same-day re-measurement showed 36
unmoved, and today shows 36. **Three days, one number**, and the reading in the
ruling has not been seen since it was written. HM-OPEN-050 records it with the
numbers. **The ruling's text is untouched** and its reasoning is not in question.

It matters because that claim was cited this week as evidence the capture had
*degraded*, which sent a session hunting a regression that does not exist.

## Phase 5 — dropped, after the check

`ClearingTheTranscriptLeavesTheDecoderAlone` uses synthesized 12 wpm audio, not a
capture, and **it still failed with the phase-3 repair in place**. So neither phase
would have taken it, and separate work on it is its own job.

# 2. What Tim should expect

**The accuracy number, before and after, on the bulletin: 36 of 47 today, 37 of 47
with the one-line repair — and the repair is not in the build.** That is the whole
of it. `STATION` is the character that comes back; `AHIS` does not.

**What that means at the radio tonight:**

- **The transcript is exactly as accurate as it was this morning.** Nothing in the
  decode path changed. If it read a contact for you this morning it will tonight,
  and if it garbled one it still will.
- **The garbling has a name now.** When you see a T come out as an A — `STAAION`,
  `AHIS` — that is a dah being split at its leading edge and the fragment surviving
  as a dit. It is one line and one number, and it is measured, not suspected.
- **The leading edge is untouched and is still what you watch arrive.**
- **Keep the audio when something defeats it.** Everything above came from one
  thirty-second capture you kept on 2026-08-18. That is what makes a mechanism
  findable at all.

**The suite: 1,998 tests, 3 failing**, the same three as this morning:
`APassThatReadSomethingEmitsSomething`, `TheBulletinDecodesToItsAnswerKey` and
`ClearingTheTranscriptLeavesTheDecoderAlone`. If you see four, something new is
wrong.

# 3. What we should do next

- **Rule the phase-3 trade.** One sentence: is a right letter on the bulletin worth
  four placeholders on the VA3VRR capture? If yes it ships tonight — it is one
  line and it is written.
- The seeding rule from phase 1, as its own work with every fixture adjudicated.
  That one is worth an evening and it is what makes the settled transcript appear
  at all on short exchanges.
- HM-OPEN-050, whenever you next look at HM-DEC-115.

# 4. What's blocking us

One trade, below. Nothing else.

---
date: 2026-08-19
refs: HM-OPEN-049, HM-DEC-114, §0.0, §12.1
---

**Whether the de-glitch is widened, buying a right letter on one real capture and
paying four placeholders on another.**

`CwSettledPass.Deglitch` is a median filter sized so that it removes half of what
it is asked to remove. Correcting that is one line, `width = 2n + 1`.

Measured on both real captures:

- **ARRL bulletin: 36 of 47 becomes 37 of 47.** `STAAION` becomes `STATION` and
  `HAND■` becomes `HANDNG`.
- **`cw-2026-08-17-013347`: four characters ahead of `VA3VRR` become
  placeholders.** The callsign itself is intact either way.

For it: a wrong letter is the thing §0.0 exists to prevent, and a placeholder is
the honest answer. Trading `STAAION` for `STATION` removes a confident error.

Against it: `TheSettledPassNoLongerStopsShortOfTheCallsign` is a ratchet a previous
phase set deliberately on real audio, and this makes it red. Four characters lost
to placeholders on a capture the operator will recognise is a real cost on real
off-air copy.

Rejected as a session's choice: both directions. It weighs two costs against each
other on two real recordings, which is §12.1 clause 3 without ambiguity.

The change is one line and it is written up in HM-OPEN-049 with both readings, so
whichever way it goes it is minutes of work.

## Asks still outstanding

Five, per HM-DEC-139 and scoped by HM-DEC-140. Carried verbatim until ruled.

| Ask | First made | Waiting on | Where it already sits in the tree |
|---|---|---|---|
| **Whether an attended automatic cycle may reach an antenna** (§0.2, HM-DEC-098) | 2026-08-17 | The bench evening; `BENCH_CARD.md` can be followed end to end | Built and armed, dummy load only |
| **A callsign too long for one keyer send** (HM-DEC-130) | 2026-08-18 | The seam measured at the bench, from the send panel | The cycle refuses; the single send splits, and the panel says so while he types |
| **Whether the star asks for a name at the moment of saving** (HM-DEC-060, HM-DEC-134) | 2026-08-18 | Nothing but the ruling | Favorites are born unnamed |
| **Whether Hamlet may ever ask the radio to send its spectrum** (HM-DEC-062, HM-OPEN-042) | 2026-08-18 | The ruling | Not asked at all; rungs one to five have tests |
| **Whether the de-glitch is widened** (HM-OPEN-049) | 2026-08-19 | The ruling; both readings measured above | One line, written up, not in the build |

**Dropped since it was asked**: whether an empty middle class is HM-DEC-142's case
— **not a ruling at all.** Phase 1 measured it and it is a seeding defect, recorded
in HM-OPEN-048.

---

## Named and left, as the order directs

The four unruled asks above, none built around. No transmit work toward auto-CQ.
**Phase 5 dropped**, with the check reported in section one. The phase-1 seeding
rule is recorded in HM-OPEN-048 and not attempted further.
