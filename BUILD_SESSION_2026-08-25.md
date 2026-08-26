**PROJECT: Hamlet**

# One build session. Success = readable CW on the operator's screen tonight.

Reported per §12.2: `RECORDED` / `NEEDS A RULING` / `STATE`, written to
`OUTPUT.md`, overwriting it. Standing instruction: record rulings needed in the
options-table form and continue; skip and name any phase that cannot proceed.
Anything touching §0.0/§0.0.1 is Tim's. **No transmit work. No scanner work.**
Captures are read-only (HM-DEC-091).

**Scope discipline for this session:** the decoder core is working — it read two
full QSOs last night and `WB8SC … SKSK … KE8P` tonight — and the remaining
losses are (1) it decodes when no single station is present, (2) it sometimes
parks on the wrong pitch, (3) the display buries good copy under stale soup.
Fix those three, log one measurement for tomorrow, stop. **Do not** redesign the
character cutter, retune gap clusters, or refactor anything on the way past.

---

## Phase 0 — bank tonight's three captures (15 minutes, do it first)

Add `cw-2026-08-26-004808`, `-004900`, `-004952` with sidecars to
`tests/fixtures/cw/captured/`, harness floors at today's numbers. Their roles:

- **004808** — duty 55%, overlap; the file that proves the spanLlr inversion
  (phase 4's fixture).
- **004900** — the control: duty 42.3%, tone 405, and Hamlet correctly read
  `WB8SC`, `SKSK`, `KE8P`. **Nothing this session ships may lose those three
  tokens from this file.**
- **004952** — S2, 40 WPM at the top of the search, 58 of 106 unsure; honest
  behaviour that must stay honest.

## Phase 1 — the emission squelch (the biggest win tonight)

**WHAT.** Stop emitting characters when the recent element stream does not look
like one station sending Morse. When it doesn't, emit nothing (or a single `■`
per stretch); when it recovers, resume.

**WHY.** Every failure of the last two nights sorts on one axis, measured
across sixteen captures at identical input level with the tone locked:

```
duty 18–24%  ->  invented text        (021825: 8 s of KC1UEK in 30 s of soup)
duty 36–47%  ->  readable             (ten rag-chew captures, 0–8 unsure)
duty 55%+    ->  more than one station -> soup   (004808)
```

Plain-text Morse cannot exceed ~44% key-down (PARIS arithmetic: 22 of 50
units), and a real station rarely sits under ~30% while actually sending. The
decoder currently decodes everything, including silence and pile-ups, and the
output for both is E/I soup. **Do NOT build this gate on `spanLlr` as it stands
— phase 4 shows why with numbers.**

**HOW.** A rolling test over the last ~3 seconds at the tracked pitch, built
entirely from quantities the decoder already computes:

1. **Local duty** at the tracked bin in `[25%, 55%]`. Below: silence — hold
   emission. Above: overlap — hold emission and mark the stretch `■`.
2. **Fist sanity** on the recent resolved elements: dah/dit ratio of the last
   ~12 marks in `[2.2, 4.0]`, and dit in `[25, 160] ms` (48 down to 7.5 WPM).
   Noise fails the ratio test (measured 4.6–5.8 on tonight's mush, 12.9 on one
   bin); real fists measured 2.6–3.4 all week.
3. Hysteresis on the state itself — require ~2 s of passing before resuming, so
   the gate doesn't flap at word boundaries. A word gap at 20 WPM is 420 ms;
   2 s cannot false-trigger on one.

Characters already resolved before the gate closed stay; the gate is
forward-acting only.

**PROOF IT'S RIGHT, before merging:** on 021825 the output shrinks from 63
characters to roughly the 8-second `K` call window; on 021629 the
`559 559 IN MI MI` survives; on 004900 `WB8SC`/`SKSK`/`KE8P` survive; on
013520/013303 (last night's floors) the output is byte-identical, because those
files never leave the pass band of the test. If any floor case changes, the
window is wrong — widen it, don't ship.

## Phase 2 — pick the pitch by fist quality, not energy

**WHAT.** When the survey chooses the bin to decode, score candidates by
whether their keying looks like one Morse fist, and only then by strength.

**WHY.** Both remaining tone failures were energy-led:

- **012823** (last night): decoder sat at 450 Hz, station at 499.8 → the one
  soup capture of a good hour.
- **004952** (tonight): decoder at 400 Hz where the envelope fits a 190 ms
  "dit" — 6 WPM noise — while 510 Hz held the only coherent structure.

And the discriminator is already proven on real data: sweeping 021629,
485–540 Hz scores ratio 2.7–3.0 / duty 24–31% (one station) while 545–620 Hz
scores ratio 4+ / duty 62–76% (mush). The same test as phase 1, applied per
candidate bin.

**HOW.** For each candidate the survey admits: threshold its envelope, take the
last few seconds of runs, compute (ratio, dit, duty). A bin passing the phase-1
window beats any bin that fails it, regardless of energy; among passers, take
the strongest; add ±1-bin hysteresis so the choice doesn't hop between
adjacent bins on a steady signal (the 013402→013637 captures drifted 525↔540
on one station). Log the chosen bin's (ratio, dit, duty) into the sidecar next
to `toneHz` so a bad choice is diagnosable afterwards (§0.0.1).

## Phase 3 — the display stops burying good copy (small, but it is why
tonight felt hopeless)

**WHAT.** Three changes to the CW terminal, none to the decoder:

1. When the phase-1 gate has held for more than ~10 s, insert a visible
   separator (a timestamped rule) into the transcript instead of nothing.
2. Render everything before the most recent separator dimmed, so the eye lands
   on current copy. Keep it selectable; delete nothing.
3. **Retire the `no keying here` advice block whenever the tone panel is
   showing** — two panels currently assert, simultaneously, that a clear tone
   is present and that nothing is there, 50 Hz apart, and the advice sends the
   operator to the radio for a decoder condition. §0.0 applies to the screen.

**WHY.** Tonight's "this is hopeless" was a screen whose first hundred
characters were decoded two minutes earlier at 55% duty; the current capture
had read three callsign tokens correctly. The decoder was fine. The screen
lied by accumulation.

## Phase 4 — measure the margin; do not gate on it yet

**WHAT.** For every emitted character, also compute and log
`marginLlr = LLR(best character) − LLR(second-best character)` over the same
span, alongside the existing `spanLlr`. Sidecar and jsonl only. No behaviour
change. Also clamp both scores to sane bounds.

**WHY.** The existing `spanLlr` nulls against *silence* ("the key having been
up throughout its own span"), and 004808 proves that null inverts exactly when
it matters: the E-soup scored `8003, 15042, 28266, 29261` while the plausible
`K H I H D A N Y` tail scored `41–437` — the garbage out-scored the signal
**100:1**, because on a near-continuous tone everything beats silence. A gate
built on it would have kept the soup and discarded the text. Against a
*second-best-character* null, an E carved out of a continuous tone scores near
zero (many characters explain the span equally) while a cleanly-fitted K does
not. Tonight's job is only to log it so tomorrow's session can set thresholds
from real distributions instead of guesses — and to bound the numbers, because
`6:27306879.3` and `■:-1876275.2` are the `fit 14774028201.6` overflow family
again.

## Phase 5 — only if time remains: take the keying sweep off the screen

Wrong on 14 of 20 captures against independent measurement, including
`no keying` on files where the decoder beside it was reading callsigns. Hide
the panel behind a debug flag until it is rebuilt against the fixture corpus.
Removing a lying instrument is one line; rebuilding it is not tonight's work.

---

## The operator's half — how tonight proves it (for Tim, not the session)

1. **After the build, point at W1AW, 7.0475.** It is a weekday: your own
   Friday captures prove the late-evening CW slot (you recorded the bulletin at
   23:18 Eastern). The bulletin is 18 WPM machine keying — the exact profile
   the decoder reads best — and the schedule is at arrl.org/w1aw-operating-schedule
   if you want the precise slot. This is the known-good target; if the screen
   doesn't read W1AW after this build, the session missed.
2. **Before that slot, find one CQ** the way you found KD0UN and K1ZJA — a
   single station calling, any speed. Watch specifically for: no E-soup while
   the frequency is quiet (phase 1), the transcript separator appearing during
   dead air (phase 3), and current copy bright with old copy dimmed.
3. **Press capture on both good and bad.** The bad ones are tomorrow's
   margin-threshold data (phase 4).
4. Skip 7.055 tonight — straight-key ops and a crowded segment; it's a fine
   target next week, not for judging this build.

## What not to do tonight, so it counts

No joint character-cutter (it needs a design ruling and a day, not an
evening). No gap-cluster retuning (013637 proves the clusters merge at
30 WPM; tuning cannot fix that). No gating on the silence-nulled `spanLlr`
(004808). No touching the tone interpolation that started working, beyond
phase 2's *selection* logic. And floors only rise: 013520, 013303, 004900 read
tonight exactly what they read this morning, or the change that broke them
does not ship.
