**PROJECT: Hamlet**

# Analysis for the development computer — where the week actually stands, and the one thing left that changes what the operator sees

Written 2026-08-27 from the capture corpus. Reported findings are measurements;
the design in §4 needs a ruling and the options table is pre-drafted for Tim.

## 1. The scoreboard, stated honestly in both directions

**The operator's verdict is "no real improvements," and on the axis he means,
he is right.** The quality of the characters Hamlet *emits* has not moved all
week. Monday's good captures read `TEN TE C ES 1 T TW TTO` for "Ten-Tec, 100W";
tonight's read `USEDTOUSEAFIRM` and `OUTOFALT`. Same class of error, same
rate: letters right, cuts wrong. **The element-to-character decision has not
been touched since the corpus began, and it is now the only stage that
hasn't.**

What did change is everything around that stage, and the A/B is on tape.
Identical situation — a crowded 7.02x MHz evening, S5–S9, multiple stations:

| | Monday (021629/021825) | Tonight (011112→011617) |
|---|---|---|
| pileup / mush | 63 confident characters of invented text | blocks, then **0 emitted** |
| a station rises | buried in the noise characters | `IVY LEAGUE`, `USE A GUY OUT OF NJ` |
| empty frequency | clock fitted 10 WPM to gaps | `toneHz NOT MEASURED`, duty `not measured`, 0 emitted |
| the screen | two-minute-old soup on top | opens on the current conversation |

And the suppression was audited: the one 0-emitted capture with signal present
(011411, duty 55.9%) contains nothing recoverable — the tracked 580 Hz region
measures ratio 1.86 / duty 75% (overlap), and the fast keying at 520–540 Hz
defeats the independent chain too. **The squelch has not yet silenced anything
readable.** Keep auditing every 0-emitted capture exactly this way.

So: the week built a decoder that is honest about what it cannot read. It did
not make it read better. §4 is the item that does, and nothing else on the
backlog competes with it for operator-visible effect.

## 2. The margin numbers are in, and the computation needs one constraint

Two nights of logged `span/margin/ratio` triples settle it:

- Correct characters: `O:387/1.7  G:266/0.9  L:412/3.4  Z:283/1.6`
- Wrong/pileup characters: `W:3462/1.9  blocks 0.2–48 / 0.0–1.4`

**Margins are 0.1–3.4 on right answers and 0.0–1.9 on wrong ones — no usable
separation.** The cause is almost certainly that "second-best" may re-segment:
shift a boundary, split a dah, borrow an element from the neighbour. There is
always a trivially-different alternative within a whisker, so every margin is
tiny no matter how right the answer is.

**Constraint: second-best = same span, same element boundaries, different
character.** Under that rule a cleanly-fitted K has no close competitor and the
margin opens; a W carved out of two overlapping fists still doesn't. Note that
§4's dynamic program produces exactly this quantity as a by-product, which is
one more reason §4 is the right next build.

Also carried over, still open: `spanLlr` against silence inverts on strong
signals (004808: soup at 8,000–29,000 while real letters sat at 41–437) — do
not gate on it; and the raw scores still need clamping (`6:27306879.3`,
`■:-1876275.2`).

## 3. Small items, each one sentence of work

1. **After a Clear, `textCovers` still claims "everything read since the
   decoder started listening"** while the text visibly starts at the clear.
   Say "since the transcript was cleared at hh:mm:ss". The counters diverging
   from the text is the old `ClearingTheTranscriptLeavesTheDecoderAlone`
   wording leak in a new place.
2. **The keying sweep is at 17 contradictions**, including `no keying` on both
   of tonight's readable captures. Hide it behind a debug flag until rebuilt;
   it is the message on screen at exactly the moments the operator is deciding
   whether to trust the app.
3. **`competing: none found` in every sidecar of the week**, including files
   with eight admitted tones and a station 2.4 dB from the tracked one. Either
   report what the survey saw or drop the field.

## 4. The core work item: joint decoding of the element stream

**This needs a ruling before a session builds it. Options at the end.**

### WHY — the complete evidence, four fixtures, both failure directions

The current cutter thresholds each gap in isolation. Four captures prove that
rule cannot work, from opposite ends:

- **021410, 18.2 WPM, machine-grade fist.** Gap classes at 53 / 221 / 913 ms —
  0.81u / 3.36u / 13.9u, wide dead zones, perfectly separable — and it still
  cut `W` into `A T E` (`ATEEKEND`) and doubled letters. Separable classes,
  wrong cuts: **the rule fails even when the information is perfect.**
- **013637, 30.6 WPM.** Gap clusters at 24 / 28 / 171 ms — element and
  character gaps **four milliseconds apart**. `AB OVE`, `BREE Z E`. **No
  per-gap threshold can work here even in principle.**
- **011447/011514, ~27 WPM (clock not proved).** `USEDTOUSEAFIRM`,
  `OUTOFALT` — the same fault in the missing-cut direction when the clock is
  unproven.
- **The E/T-soup family** (the whole corpus before the squelch): the mark and
  gap classifiers demonstrably not sharing a unit — a forced-unit sweep across
  8–44 WPM cannot reproduce Hamlet's all-E + all-fragment signature with any
  single unit, so the two decisions are structurally independent today.

### WHAT

Replace per-gap thresholding with a joint decode over a short sliding window
of the element stream: choose the segmentation into characters that best
explains **all** the durations together, given the fitted clock.

### HOW

Dynamic program (Viterbi) over element boundaries:

- **State:** position between elements. **Transition:** "emit character C
  spanning elements i..j", allowed only if the mark pattern of i..j is C's
  pattern.
- **Transition cost:** sum of duration-fit terms — each mark against 1u or 3u,
  each internal gap against 1u, the boundary gap against 3u (character) or 7u
  (word) — all as log-likelihoods around the fitted clock with a per-fist
  spread learned from the recent stream. Plus a flat validity term for "is a
  Morse character" and a matching cost for the ■ hypothesis so unreadable
  spans lose to blocks rather than to invented letters.
- **Streaming:** finalise with a lag of ~2 characters; emit on traceback
  agreement, exactly like the current pipeline's cadence.
- **Complexity:** O(elements × max-pattern-length) — trivial at these rates.
- **By-product:** the constrained margin from §2 falls out as best-path minus
  best-path-forced-different-at-this-character. One mechanism replaces the gap
  thresholds, repairs the cuts, and produces the honest confidence number, all
  from the same table.

### The §0.0 guard, stated up front

**No language model. No letter-frequency prior. No dictionary.** The only
"knowledge" admitted is the Morse code table itself and the clock. A decoder
with an English prior will hallucinate plausible words from marginal audio,
which is precisely the confident-lie failure this project exists to avoid. The
validity term must be small against the timing terms, and the acceptance suite
must include a noise fixture (021825) proving the joint decoder still yields
blocks on non-signal — the squelch stays upstream and untouched.

### Done means

- `021410`: `ATEEKEND` → `WEEKEND`, `TTHINKING` → `THINKING`, `FLENX` → `FLEX`.
- `013637`: `AB OVE` → `ABOVE`, `BREE Z E` → `BREEZE`.
- `011447`: `USEDTOUSEAFIRM` → `USED TO USE A FIRM`.
- Every floor in the fixture corpus — the two rag-chew evenings, W1AW seven,
  KD0UN, the synthetic 8 kHz file — reads the same or better, enforced by the
  harness.
- Margins on correct characters separate from margins on pileup characters by
  at least an order of magnitude on the logged corpus (the §2 test).

### NEEDS A RULING (pre-drafted for Tim)

| | option | gets | risks |
|---|---|---|---|
| A | Full joint decoder as specified, replacing the cutter | fixes all four fixture classes; honest margins for free | largest single change to the decode path all project; one full session |
| B | Joint decision for **gaps only** (marks classified as today, DP chooses cut/no-cut/word) | fixes `USEDTOUSEAFIRM`, `AB OVE`, most of `ATEEKEND`; half the change | leaves mark/gap unit split in place; margins still need separate work |
| C | Defer; next session does §2 constraint + §3 small items only | zero risk | operator-visible quality stays exactly where it is, which is the complaint |

Recommendation: **A**, because B rebuilds half the machinery and then rebuilds
it again later, and C is the status quo the operator just sighed at. But A on a
clean day, with the floors green before the first edit and after every phase.
