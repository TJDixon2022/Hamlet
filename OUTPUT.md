# 1. What Claude did

**STATE.** Development computer, `C:\Source\HamLet`, Claude Code surface (§9.5).
**Branch: `main`.** Prompt, `WORK_INSTRUCTIONS.md` and `PROJECT_CARD.md` all say
`PROJECT: Hamlet`; gate passed on all three (§9.6). **No radio was connected**
(HM-DEC-093). Status written at the start and at every phase boundary.

**Phases 1 to 4 worked. Phase 5 dropped, and checked before dropping.** Four
commits.

## Phase 1 — HM-DEC-142 recorded

Written verbatim to `DECISIONS.md` at the head, index row at the true head of §1,
and `DecisionLogOrderTests` confirms the placement rather than me asserting it.

## Phase 2 — built

`CwGapFit` no longer refuses when only the word class is empty. An empty element
or character class is still a refusal — those are the measurement genuinely
failing — but an empty word class is the sender rather than the fit. **The word
boundary is put out of reach rather than invented**: the character cut goes to
infinity, so no gap can be classified as a word break and the transcript comes out
unspaced, asserting exactly what was measured.

The transcript says the condition, through `VoiceTests`, where he is looking:

> The letters below are what Hamlet heard, run together. Whoever is sending has
> not left a gap long enough to call a word break, which is ordinary in a callsign
> or an exchange, so the letters are measured and the spaces between words are not
> shown at all rather than being put where they might have gone.

`decode_quality` carries `wordSpacingUnmeasured`, so an unspaced transcript and an
empty one are different facts in the record.

## Phase 3 — the gate, measured, and it passes

**Measured on the sender's own gaps at three speeds**, which is where the decision
is actually made:

| Dit | Element class | Character class | Word | Boundary |
|---|---|---|---|---|
| 100 ms | 24 gaps at 100 | 10 at 300 | 0 | **173 ms** |
| 60 ms | 24 at 60 | 10 at 180 | 0 | **104 ms** |
| 48 ms | 24 at 48 | 10 at 144 | 0 | **83 ms** |

Every boundary lands above every element gap and below every character gap, so
**a callsign comes apart into characters rather than running together**. Three
heaps are untouched — a sender who leaves word gaps still gets spaces — and one
heap is still refused. The ruling's condition for shipping is met.

**And the honest half: it does not move either named fixture.**

- **`exchange-easy` now fits classes where it fitted none** — element 12 gaps at
  110 ms, character 4 at 310, word 0, boundary 185. The new path is exercised and
  the separation is good. Its transcript is still nearly empty, because it is
  blocked further up by a refused clock and a keying verdict that is false at the
  end of the recording. That is the second fault recorded in HM-OPEN-048 and it is
  not what this ruling was about.
- **`coverage-easy` is not covered by the ruling as written.** Its empty class is
  the **middle** one, not the word class: 80 gaps split 61 into the element class
  and 0 into the character class. HM-DEC-142 says in terms that an empty character
  class stays a refusal, so it stays refused. The two heaps it does have are
  element and everything-longer, which is arguably the same situation wearing a
  different label, and deciding that is not mine. It is in section four.

**So `APassThatReadSomethingEmitsSomething` does not go green and the standing red
does not return to two.** It is three, and the third is that test, on
`coverage-easy`, for the reason above.

## Phase 4 — the bulletin never degraded

**Answered from the history rather than by bisecting, because the history answers
it directly.**

| When | Recorded | Reading |
|---|---|---|
| 2026-08-17, `2ec922f`, the test is written | "36 characters against 47" | red from birth |
| 2026-08-17, `95de0a3` | "unmoved: 36 against 45" | T read as A twice, dropped letters |
| 2026-08-18, `d033e7c` | "30 of 44 correct" | **a different metric** — aligned rather than counted |
| 2026-08-19, today | **36 characters against 47** | `NL DOT NET ■I ECH STAAION HAND■ AHIS MESAGE P` |

**The count today is the count on the day the test was introduced.** The numbers in
between appeared to move because one of them was measured a different way. There
is no regression to find and a bisect would have spent the evening landing nowhere.

**What actually disagrees is HM-DEC-115 and the test, one day apart.** That ruling
says the same audio read every character correctly after acquisition; the test
written the next day already showed 36 of 47. That is a measurement never
reproduced, not something rotting.

**The mechanism is element-level, not spacing.** `STAAION` for `STATION` and `AHIS`
for `THIS` are both T read as A — a lone dah gaining a leading dit, so either a
mark is being split or a character boundary is missed and a preceding dit joins the
dah. HM-DEC-142 cannot touch that. Naming the line needs an order aimed at it with
the audio in front of it, and this project has twice been burned by a diagnosis
that named a suspect without a mechanism. HM-OPEN-049.

## Phase 5 — dropped, after the check the order asked for

`ClearingTheTranscriptLeavesTheDecoderAlone` still reads `■ DE W1AW K` against
`CQ DE W1AW K`. **The placeholder is the same element-level fault as the
bulletin's**, not a spacing one, so neither phase 2 nor phase 4 moved it and
separate work on it belongs with HM-OPEN-049 rather than with a phase of its own.

# 2. What Tim should expect

**Does the settled transcript now show you what you heard? On a signal where the
sender leaves no word gaps, yes — and only there. On the two fixtures we have
measured, no, for two different reasons that are not this ruling's.**

**What the letters are worth, plainly**, because you are going to trust that text
or not:

- **The boundary between one character and the next is sound.** Measured at three
  speeds: 173 ms between element gaps of 100 and character gaps of 300, and the
  same shape at 60 and 48. A callsign comes apart into letters. **It will not run
  `W4AWH` together and it will not split it.**
- **What is not measured is where the words break**, and the transcript says so in
  those words rather than leaving you to infer it from an odd-looking line.
- **The leading edge is untouched and is still what you watch arrive.** It reads
  the proved fixtures perfectly. Nothing in this order changed it.
- **The settled transcript may still be empty on a real signal**, and if it is,
  that is `coverage-easy`'s case or `exchange-easy`'s, both recorded. **Press "Keep
  this audio"** — it works with no decode, and a recording of the thing that
  defeated it is worth more than a memory of it.

**The suite: 1,998 tests, 3 failing.** Not two. The three are
`APassThatReadSomethingEmitsSomething` (the `coverage-easy` case above),
`TheBulletinDecodesToItsAnswerKey` and `ClearingTheTranscriptLeavesTheDecoderAlone`.
If you see four, something new is wrong.

# 3. What we should do next

- **Rule the `coverage-easy` question in section four.** It is the difference
  between HM-DEC-142 helping on one fixture and helping on both.
- **HM-OPEN-049**: an order aimed at the T-read-as-A substitution, with the audio
  open. It is the same fault behind two of the three standing failures.
- The bench evening, whenever it suits.

# 4. What's blocking us

Nothing tonight. One ruling wanted, below.

---
date: 2026-08-19
refs: HM-DEC-142, HM-OPEN-048, src/Hamlet.RadioEngine/Cw/CwGapClasses.cs
---

**Whether an empty middle class is the same case HM-DEC-142 just ruled on.**

That ruling says an empty word class is the sender and an empty element or
character class is the measurement failing. `coverage-easy` has neither shape
exactly: **its 80 gaps split 61 into the element class and none into the
character class**, with the rest above. Two heaps exist — element, and
everything longer — and the three-way seeding empties the middle one because the
seed lands on the boundary between them.

By the letter of HM-DEC-142 that is a refusal, and it is what ships today.

Two readings, and only you can pick:

- **It is the same two-heap case.** The classes are named by position and the
  content here is element and character-or-longer, so relabelling and putting the
  word boundary out of reach gives the transcript the ruling wanted.
- **It is genuinely different.** The gaps above the element class may contain both
  character and word gaps, and treating them as one class would place no spaces
  where two or three were actually sent — which is what HM-DEC-142 rejected for
  the other case.

Rejected as a session's choice: both. Each changes what the transcript asserts
about where the words are, which §12.1 places outside a session's authority, and
HM-DEC-142 is fresh enough that guessing at its edges would be reading my own
intent into your ruling.

## Asks still outstanding

Five, per HM-DEC-139 and scoped by HM-DEC-140. Carried verbatim until ruled.

| Ask | First made | Waiting on | Where it already sits in the tree |
|---|---|---|---|
| **Whether an attended automatic cycle may reach an antenna** (§0.2, HM-DEC-098) | 2026-08-17 | The bench evening; `BENCH_CARD.md` can be followed end to end | Built and armed, dummy load only |
| **A callsign too long for one keyer send** (HM-DEC-130) | 2026-08-18 | The seam measured at the bench, from the send panel | The cycle refuses; the single send splits, and the panel says so while he types |
| **Whether the star asks for a name at the moment of saving** (HM-DEC-060, HM-DEC-134) | 2026-08-18 | Nothing but the ruling | Favorites are born unnamed |
| **Whether Hamlet may ever ask the radio to send its spectrum** (HM-DEC-062, HM-OPEN-042) | 2026-08-18 | The ruling | Not asked at all; rungs one to five have tests |
| **Whether an empty middle class is HM-DEC-142's case** (HM-OPEN-048's remainder) | 2026-08-19 | The ruling; two readings set out above | `coverage-easy` stays refused and its settled transcript stays empty |

**Dropped as ruled since it was asked**: what the settled pass does with too few
word gaps, ruled as HM-DEC-142 and built this session.

---

## Named and left, as the order directs

The four unruled asks above, none built around. No transmit work toward auto-CQ.
**Phase 5 dropped**, with the check reported in section one. HM-OPEN-049 is
recorded and not worked.
