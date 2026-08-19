# 1. What Claude did

**STATE.** Development computer, `C:\Source\HamLet`, Claude Code surface (§9.5).
**Branch: `main`.** Prompt, `WORK_INSTRUCTIONS.md` and `PROJECT_CARD.md` all say
`PROJECT: Hamlet`; gate passed on all three (§9.6). **No radio was connected**
(HM-DEC-093). Status written at the start and at every phase boundary.

## What actually landed from the previous run

Checked rather than taken from the summary. **`HEAD` and `origin/main` were both
`2dd617e` and the working tree was clean.** All four phases of the previous order
had landed, not three:

| Commit | Phase | What it left |
|---|---|---|
| `e4f8ea6` | 1 | Mode follow will not move him out of Morse while he is working Morse; the data-variant fold that made every trigger write |
| `e70b67b` | 2 | The settled-pass finding with numbers, `CwSettledSilenceTests`, HM-OPEN-048 |
| `5c5c3a7` | 3 | `SendLengthIsLegibleTests` and the length line |
| `2dd617e` | 4 | **The phase the order believed was dropped.** The keep-this control verified, the sidecar's frequency label carrying its age, the decoder speed in the sidecar |

So this run's phase 1 was largely already done, and I say so rather than claiming
it twice. **What it did not do is the half underneath**, which is below.

## Phase 0 — HM-DEC-088's duplicate, renumbered

**The tiebreak came from the history and not from judgment**, as the order
required. Both index rows arrived in the same commit, `49b844c`; within it the
decoder's noise-measurement row is written first, and `DECISIONS.md`'s only
HM-DEC-088 entry is that same ruling. So **the decoder keeps 088** and **the top
strip becoming one row is now HM-DEC-141** — the next free id, since 105 is a
ruling whose entry is missing and 136 is deliberately absent.

Eleven citations re-pointed, each classified by reading the comment it sits in:
the wheel hint retiring, the bands beside the readout, the strip costing one row
rather than a third of the window, the settings flag behind the hint. Everything
else citing 088 is about measuring noise beside the tone and is untouched. Neither
ruling's text changed — an id that was never valid is not a ruling being
overturned.

`DecisionLogOrderTests` no longer permits a repeated id **at all**. The allowance
went with the thing it allowed for rather than staying as a door somebody could
walk back through. HM-OPEN-046 closed.

## Phase 1 — the capture's frequency now has one source

The previous run fixed the sidecar's *wording* — the label carries its age, which
is what HM-DEC-111 was ruled about. **It did not fix the fault underneath, and the
order was right to say verify before building.**

The sidecar read the radio; the telemetry event beside it was handed `FrequencyHz`,
which is Hamlet's own idea of where the dial is. **One capture, two paths to one
fact** — exactly the shape that produced `7025400` against `14028000`. Both now
read one property: the radio's own value where there is one, Hamlet's where there
is not, labelled either way.

The keep-this control needed nothing: on the terminal, gated on the decoder
running rather than on a successful decode, and its copy already names tonight's
case.

## Phase 2 — the settled pass, mechanism found, repair handed back

**This is the phase that decides whether tonight is worth sitting down for, and it
ends in a ruling ask rather than a fix.** Here is the whole chain, measured.

Tallied window by window on `coverage-easy`, which the reference reads at 100%:
**258 windows returned `None`** — read successfully — 63 said `NotYet`, 16 refused
the clock, **and not one character came out.**

The loss is `Emit`'s first line. It asks for the sender's own gap classes and
returns without producing anything when there are none, which is HM-DEC-115 doing
exactly what it says: *no cuts means no transcript, not a guessed one.* There are
**80 gaps** to cluster, far past the ten `CwGapFit` requires. **The fit refuses
anyway, because it requires three non-empty heaps** — element, character and word
— and this message leaves almost no word gaps, so the top class comes back empty
and `Fit` returns null.

**So the settled pass is silent on any transmission without several word gaps.**
A callsign. A contest exchange. A `V` test string. Anything sent without spaces.

**And the brief's lead was already built.** It says Hamlet reads once while the
reference de-glitches again at 0.4 of a dit and re-reads every run. `CwSettledPass`
has done that second de-glitch and refit since HM-DEC-096. That is not the gap,
and a session working from the brief alone would have spent the evening there.

**Why I did not fix it.** Two honest repairs exist and both change what a
transcript asserts about where the words are — cluster two heaps when there is no
third, or say plainly there are no word boundaries here. §12.1 puts that outside a
session's authority without exception, and the order says HM-OPEN-017's labelled
approximation is taken by ruling. The ask is in section four.

## Phase 3 — dropped, and one thing found before dropping it

**Dropped, as the order allows, and I say so rather than half-building it.**

The cheap check first: **the two standing failures are not phase 2's mechanism.**
`TheBulletinDecodesToItsAnswerKey` emits plenty of characters and gets them wrong
— `NLDOTNET■IECHSTAAIONHAND■AHISMESAGEP` against an answer key beginning
`RLDOTNET<BT>EACHSTATIONHANDLING…`. That is character accuracy on a real off-air
Farnsworth capture, and **HM-DEC-115's own text records that same recording being
read correctly**. It has since degraded. Worth its own order; not worth squeezing
into this one.

# 2. What Tim should expect

**Tonight, in the order you will hit it.**

- **Mode follow will not take you out of CW any more.** With the terminal decoding
  or the dial inside a CW segment, the map is ignored. The sixty-six seconds of
  `not_in_morse` on 2026-08-18 had two causes and both are fixed: that override,
  and a mode write that folded only the mode into the model so a USB-with-data
  target could never read back satisfied and **wrote again on every trigger**.
- **The terminal reads live CW as well as it did this morning, and no better.**
  The leading edge is what you see arriving character by character, and on the
  proved fixtures it is perfect. **What is broken is the settled transcript**, and
  it is broken in a way that will show tonight: on a callsign or an exchange with
  few spaces, the settled text will be empty while the live text is right. That is
  HM-OPEN-048 and it needs your ruling, not more code.
- **When something defeats the decoder, press "Keep this audio"** on the terminal.
  It works with no decode at all — that is the most valuable kind — and it now
  writes the frequency from one source, so the file and the record cannot disagree
  the way they did on the capture that read 7.025 in one and 14.028 in the other.
  The sidecar says how old the frequency reading was and what speed the decoder
  was tracking.
- **When you compose a reply longer than about thirty characters**, the send panel
  now tells you *before* you press: how many characters, that it goes out as two
  sends, roughly how many seconds of keying at your keyer speed, and that **nobody
  has measured how long the gap in the middle is.** That last part is the honest
  half — the single send does split, and what HM-DEC-130 refused to ship was a
  split whose pause nobody had listened to.
- **Nothing was done toward auto-CQ.** HM-DEC-098 is unruled and dummy-load only.

**The suite.** 1,992 tests, **3 failing, and the red count is not two any more.**
The third is mine and it is deliberate: `CwSettledSilenceTests.APassThatReadSomethingEmitsSomething`, red because a fixture the
reference reads at 100% produces no settled characters, which HM-DEC-114 says is a
defect rather than a ratchet. The other two are the standing decode baseline. If
you see four, something new is wrong.

# 3. What we should do next

- **Rule HM-OPEN-048.** It is one decision and it unblocks the transcript half of
  the terminal, which is the difference between reading a contact live and keeping
  a record of it.
- The bulletin regression: HM-DEC-115 recorded that capture reading correctly and
  it no longer does. That is a decode order of its own.
- The bench evening, whenever it suits: `BENCH_CARD.md` can be followed end to end
  and two queued asks are waiting on it.

# 4. What's blocking us

The settled transcript, on a ruling. Everything else tonight is unblocked.

---
date: 2026-08-19
refs: HM-OPEN-048, HM-DEC-115, HM-DEC-096, HM-OPEN-017, CLAUDE.md §12.1
---

**What the settled pass does when a sender leaves too few word gaps to form a
third heap.**

`CwGapFit` clusters the sender's own gaps into element, character and word, and
refuses when any of the three classes comes back empty. On a message with almost
no spaces that is the ordinary outcome, and `Emit` then produces nothing at all:
258 windows read successfully on `coverage-easy` and emitted zero characters, with
80 gaps available and the clock fitted correctly at 100 ms.

Three ways, and each asserts something different about where the words are:

- **Cluster two heaps when there is no third.** Element and character gaps are
  still the sender's own, so this is not a return to dit multiples. The cost is
  that a genuine word gap — this fixture has two or three — folds into the
  character class and those spaces disappear.
- **Emit with no word boundaries at all** and render the transcript unspaced,
  which is true to what was measured and reads badly.
- **HM-OPEN-017's labelled approximation**, which that item already reserves for
  your ruling.

Rejected as a session's choice: all three. Each changes what a transcript asserts
about where the words are, which §12.1 places outside a session's authority
without exception, and HM-DEC-115 is the ruling that put the current behavior
there deliberately.

What is in place meanwhile: the gap count and whether classes fitted are on the
decoder and in both settled-pass test files, so whichever way this goes the next
session starts from the mechanism rather than from a percentage.

## Asks still outstanding

Five, per HM-DEC-139 and scoped by HM-DEC-140. Carried verbatim until ruled.

| Ask | First made | Waiting on | Where it already sits in the tree |
|---|---|---|---|
| **Whether an attended automatic cycle may reach an antenna** (§0.2, HM-DEC-098) | 2026-08-17 | The bench evening; `BENCH_CARD.md` can be followed end to end | Built and armed, dummy load only |
| **A callsign too long for one keyer send** (HM-DEC-130) | 2026-08-18 | The seam measured at the bench, from the send panel | The cycle refuses; the single send splits. The panel now says so while he types |
| **Whether the star asks for a name at the moment of saving** (HM-DEC-060, HM-DEC-134) | 2026-08-18 | Nothing but the ruling | Favorites are born unnamed |
| **Whether Hamlet may ever ask the radio to send its spectrum** (HM-DEC-062, HM-OPEN-042) | 2026-08-18 | The ruling | Not asked at all; rungs one to five have tests |
| **What the settled pass does with too few word gaps** (HM-OPEN-048) | 2026-08-19 | The ruling; three ways set out above | Silent on any message without several spaces. The leading edge is unaffected |

**Dropped as ruled since it was asked**: HM-DEC-088's duplicate, ruled A and
carried out this session as HM-DEC-141.

---

## Named and left, as the order directs

The four unruled asks above, none built around. No transmit work toward auto-CQ.
**Phase 3 was dropped**, with the check that preceded it reported above.
