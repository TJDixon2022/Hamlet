PROJECT: Hamlet
ISSUED: 2026-08-19

## Asks still outstanding (inbound, per HM-DEC-139)

| Ask | First made | Waiting on |
|---|---|---|
| **Whether an attended automatic cycle may reach an antenna** (§0.2, HM-DEC-098) | 2026-08-17 | The bench evening |
| **A callsign too long for one keyer send** (HM-DEC-130) | 2026-08-18 | The seam measured at the bench |
| **Whether the star asks for a name at the moment of saving** (HM-DEC-060, HM-DEC-134) | 2026-08-18 | Nothing but the ruling |
| **Whether Hamlet may ever ask the radio to send its spectrum** (HM-DEC-062, HM-OPEN-042) | 2026-08-18 | The ruling |

**HM-OPEN-049 is ruled A and is phase 1.** Dropped from the queue.

---

# Work order — the text he watches arrive, and the signals that never arrive at all

**Five phases. Phase 5 is the one to drop.**

Gate first (HM-DEC-099). Write `PROJECT_STATUS.md` now, at every phase boundary,
and at the finish.

## Read this before scoping anything

**The decoder has moved one character in three days.** The ARRL bulletin read 36
of 47 on 2026-08-17 and reads 36 today. Sessions have been improving the *settled
transcript* — the record kept afterwards — while every report has stated that the
**leading edge is what the operator watches and is already good on proved
fixtures.**

**This order goes where he is looking, and then earlier in the chain.** The chain
is: find the tone, decide someone is keying, fit the clock, extract elements,
emit. Recent work lived entirely in the last step. **The signals he has never
decoded fail before it.**

---

## Phase 1 — Ship the de-glitch fix (ruled A on 2026-08-19)

`CwSettledPass.Deglitch` sizes a median filter as
`Math.Max(1, (int)Math.Round(shortestSeconds / _hopSeconds))`. A median filter
removes runs shorter than **half** its window, so both passes removed ten
milliseconds whatever they were asked for, and 25-to-50 ms fragments survived to
become dits.

- **`width = 2n + 1`.** One line, already measured: bulletin 36 → 37 of 47,
  `STAAION` → `STATION`, `HAND■` → `HANDNG`.
- **Adjudicate `TheSettledPassNoLongerStopsShortOfTheCallsign`.** It goes red: four
  characters ahead of `VA3VRR` become placeholders. Tim ruled that a placeholder is
  honest where a wrong letter is not (§0.0), and that a ratchet is a regression
  guard rather than a statement of what is correct (HM-DEC-114). Re-baseline it
  with the reason recorded.
- **The four lost characters are not written off.** They are the likeliest sign of a
  *second* defect the wider filter has exposed — real dits being eaten because the
  dit estimate is high. Record what they were and what they became. Do not chase it
  in this phase.

## Phase 2 — Does the live path have the same fault? (this is the phase that matters most)

**Nobody has checked.** The de-glitch fault was found in the settled pass and fixed
there. The streaming pass is what the operator reads at the radio, character by
character, in real time.

- **Find every de-glitch, median filter or run-length threshold in the streaming
  path** and check each for the same halving. Same question, same arithmetic: does
  it remove what it is asked to remove?
- On the same bulletin capture, dump the streaming pass's element widths in
  ten-millisecond buckets, as phase 2 of the last order did for the settled pass.
  **1,075 marks between 20 and 50 ms on that recording were all classified as
  dits.** If the streaming path sees the same fragments, the fix in phase 1 is
  worth far more than one character and is worth it where he is looking.
- Report the streaming pass's character accuracy on the bulletin and on
  `cw-2026-08-17-013347`, before and after. **Those numbers have never been
  published**; every accuracy figure this week has been the settled pass's.

## Phase 3 — Why `exchange-easy` never gets as far as spacing

A fixture the reference reads at **100%**, and the last session found it *blocked
further up by a refused clock and a keying verdict that is false at the end of the
recording.* That was named and left, twice, in favour of work on transcript
spacing.

- **The false keying verdict first.** A stage that decides nobody is keying, on
  audio where somebody plainly is, ends the decode before anything else runs. Find
  what it measures at the end of that recording and why it flips.
- **Then the refused clock.** Which floor, on which measurement, with what values.
- Report both as mechanisms with numbers. **Repair only what is unambiguous**; if a
  repair changes what the display asserts about whether a signal is there, that is
  §0.0 and it is Tim's.

## Phase 4 — The floor that was ruled and never built (HM-DEC-097)

**There is no SNR floor in the decoder at all.** The streaming pass gates on
coherence and a plausible speed, the settled pass on six decibels of contrast, and
neither is what HM-DEC-097 describes. At −2 dB the decoder emits a full message of
which **44% is invented** — which is `STAAION` a hundred times over, and is the
largest §0.0 failure in the application.

It cannot simply be added: HM-DEC-097's decibels are the broadband ratio the
fixture was generated at, and the decoder measures inside a narrow tone filter,
reading about seventeen decibels higher for the same audio.

- **Do not pick the number.** What the decoder's own margin corresponds to nought
  decibels broadband decides what the display asserts, and §12.1 puts that outside
  a session's authority.
- **Do produce the table that lets Tim pick it in one sentence**: the decoder's own
  measured margin against generated SNR across the sweep, and what the transcript
  looks like at each — how much is right, how much is invented, how much is
  placeholder. The existing sweep already has the audio.
- Say plainly what a floor would cost in signals refused that are currently decoded
  correctly. That is the other half of the trade and it is the half nobody has
  measured.

## Phase 5 — The gap-fit seeding (DROP THIS ONE IF SHORT)

`coverage-easy`: eighty gaps at 110, 310 and 710 ms — element, character and word,
textbook — and the fit refuses because three seeds land at 110, 110 and 310 and
the middle cluster empties. Two repairs were tried and each traded one green test
for another.

What is wanted is one rule that rescues a collapsed middle seed **only where a
third heap exists**, in the fit rather than at the seeds, with every fixture
adjudicated.

**Drop this whole if short and say you dropped it.** It improves the record kept
afterwards, not the text he watches arrive.

## Named and left (§12.6)

The four unruled asks above. **No transmit work toward auto-CQ.** **No records
work of any kind** beyond recording a ruling a phase actually needs — no index
tidying, no open-item sweeps, no numbering.

## Reporting

`OUTPUT.md`, four sections (HM-DEC-106), section four carrying the asks queue.

**Section two opens with two numbers and nothing else first**: the streaming
pass's character accuracy on the bulletin before and after. That is the text he
reads at the radio and it has never been measured.

**If you finish every phase, stop and report.**
