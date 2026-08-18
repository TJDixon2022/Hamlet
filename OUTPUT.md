# 1. What Claude did

**STATE.** Development computer, `C:\Source\HamLet`, Claude Code surface (§9.5).
**Branch: `main`, and nowhere else** (§9.5.1). The prompt claimed
`PROJECT: Hamlet` and the tree confirms it: `CLAUDE.md`'s header reads
`Project: Hamlet`, the solution is `Hamlet.sln`, the namespaces are `Hamlet.*`.
`git branch -a` showed `main` alone. Gate passed. **Nothing in this report is
evidence about the radio** (HM-DEC-093): every number comes from a fixture, a
generated signal, or one off-air recording decoded here.

**Nothing was recorded under §12.1.** Three questions came up and all three are
in section 4.

**All five phases were worked. Phase 5 was half dropped and I say which below.**

**This session shipped less than it measured, and that is the honest summary.**
Two of the five phases are ratified rulings whose implementations were built,
measured, and reverted because they made things worse. The measurements are the
deliverable in both cases and they are precise.

## Phase 1, the clock fit — one part of three shipped

**Part 3 shipped. Parts 1 and 2 do not, and the reason is a measurement.**

Part 3 is the fitted dit-or-dah boundary: `ClassifyMark` now cuts between the
two measured mark clusters rather than at two dits, fitted per signal, seeded on
percentiles, forgotten on a retune. A textbook fist cuts at 1.73 dits, the
geometric mean of one and three; a tight fist sending dahs at two and a half
dits cuts at 1.58 rather than being judged against a number its sender never
used. **Exactly neutral on the suite: 13 failing before, the same 13 after.**

Parts 1 and 2 were built exactly as the ruling specifies and then measured. **The
premise does not hold for this detector.** Per-mark, against the true lengths:

| Speed | True dit | Gate reads | Half amplitude reads |
|---|---|---|---|
| 12 wpm | 100 ms | 100–110 | 80–90 |
| 25 wpm | 48 ms | 45–50 | 30–35 |
| 30 wpm | 40 ms | 40–45 | 25 |

**The gate's own length is accurate to within a hop at every speed.** It is the
half-amplitude measure that is wrong, and worse as marks shorten — 15 percent at
12 words a minute, 30 at 25, 37 at 30. The shed is a roughly fixed 15–20
milliseconds regardless of speed, which is the analysis window: the Goertzel
window rounds the top of a short mark, so the width at 6 dB below its own apex is
far less than its base.

With all three parts in, `ACleanSignalDecodesExactly(25)` decodes exactly and 25
reads 25 — but 30 words a minute collapses to nothing at all, `fast-easy` reads
empty at 40 wpm, and **the suite goes from 13 failures to 29**, including the
sensitivity floor tests. The acceptance asked for the suite at or below where it
started. It is not.

## Phase 2, the bulletin, re-measured and unmoved

**36 characters against 45, match ratio 0.72, unchanged.** Only part 3 shipped
and part 3 is neutral, so nothing moved. Aligned against the key:

- `JJ` extra and `TARRLD` lost at the start — acquisition.
- `BT` read as a placeholder and an `I`. The prosign is not resolved.
- **`T` read as `A` twice**, in `STATION` and in `THIS`. A dah read as a dit
  followed by a dah is a spurious leading dit: a mark boundary in the wrong
  place, or an edge caught early.
- `A` dropped from `EACH`, `S` from `MESSAGE`, `LING` from `HANDLING`.

Every one is character-level, which is what phase 1 was for. Nothing was tuned
to this recording.

## Phase 3, the floor swept

The floor is now a decoder parameter defaulting to the ruled value, so it can be
measured rather than translated a second time. Nothing outside a measurement
ever sees anything but 17.

```
floor   invents from   correct at 5 dB   at 4 dB   at 3 dB   reads to
   17          never              0.94      0.61      0.11     5.0 dB
   16          never              1.00      0.94      0.67     4.0 dB
   15          never              1.00      1.00      0.94     3.0 dB
   14          never              1.00      1.00      1.00     1.0 dB
   13          never              1.00      1.00      1.00     1.0 dB
   12        -2.0 dB              1.00      1.00      1.00     0.0 dB
   11        -2.0 dB              1.00      1.00      1.00    -1.0 dB
   10        -2.0 dB              1.00      1.00      1.00    -1.0 dB
```

**Fourteen and thirteen are the last floors that never invent a character at any
level, and both read the whole message perfectly down to one decibel.** That is
the four decibels seventeen gave away, recovered, with the property the ruling
exists for intact. Twelve and below begin inventing at minus two, which is the
case HM-DEC-097 names at 0.44 invented.

**Reported and not chosen**, as the order required.

## Phase 4, the tip adopting the settled classes — built and reverted

Built exactly as HM-DEC-116 describes: the estimator takes the settled pass's
fitted classes for the current sender, keeps dit multiples until they arrive,
and forgets them on a retune or a lost clock.

**The acceptance's two named tests pass.** `NothingIsInventedAtTheHandover` and
tone-finding on the two-station recording both survive. And it fixes the app's
`ClearingTheTranscriptLeavesTheDecoderAlone`.

**It costs the callsign on a real capture.** On `cw-2026-08-17-013347` the
settled pass goes from `■■■ ■■VA3VRR` to `■■■ ■` — every character a
placeholder, and `TheSettledPassNoLongerStopsShortOfTheCallsign` fails.

There is a feedback loop: the estimator adopting the settled pass's classes
changes state the settled pass then depends on. Three narrowings were tried and
none broke it — suppressing null adoption, removing the explicit drop on a lost
clock, and confining adoption to the boundaries. Disabling adoption entirely
restores the callsign, which is what identifies it as the cause.

**Net it is thirteen failures either way**, one synthetic app test swapped for
one real-capture test. **A real capture outranks a synthetic one** — HM-DEC-091's
own lesson, where two real recordings did in an afternoon what seven synthesized
fixtures could not — so it was reverted.

## Phase 5, half done and half dropped

**Done: HM-DEC-118.** The first spot load runs from the reconnect rather than
from the view model's constructor, in a `finally` so it happens whatever the
radio did. One that answered has set the band from its dial; one that did not
leaves the remembered band, which is the same guess as before and is now the
only guess available rather than a guess made in preference to asking. Asking
the radio from the constructor stays rejected.

**Dropped: the fixture rebuild.** It needs the recipes changed, the WAVs
regenerated, the reference scorer run to satisfy HM-DEC-101's gate, and every
held-out fixture adjudicated one at a time with a recorded reason (§12.5). That
is a work order rather than a phase tail, and half-doing it would leave the
fixture set unadjudicated, which §12.5 forbids by name.

**No transmit work of any kind was done and nothing was built toward auto-CQ.**

# 2. What Tim should expect

- **Build succeeds, no warnings.**
- **1806 tests, 13 failing.** 1378 of 1390 pass in the engine, 415 of 416 in the
  app. 6 tests added.
- **The 13 are the same 13 the session started with.** Nothing regressed and
  nothing was fixed. Five of them are supposed to be red: the four
  `TheEasyTierIsReadWhole` rows and `TheBulletinDecodesToItsAnswerKey`.
- **What is different at the radio.** Almost nothing, deliberately. The
  dit-or-dah boundary is fitted rather than multiplied, which no fixture
  notices, and the happening-now panel is empty for a second or two at startup
  instead of showing the remembered band's stations.
- **What did not change and was expected to.** `ACleanSignalDecodesExactly(25)`
  is still red and the bulletin still reads 36 of 45. Both were phase 1's job
  and phase 1 delivered a third of itself.
- **The floor is still 17.** The sweep says 14 or 13 and the number is yours.
- **Everything is committed and pushed to `main`.** Nothing local, no branches.

# 3. What we should do next

- Rule on the clock, section 4 item one. It is the same blocker as last session
  and it now has a measurement underneath it rather than a theory.
- Rule on the floor, section 4 item two. It is a one-line change once chosen and
  it buys back four decibels.
- Rebuild the six short fixtures as its own work order. It clears most of the
  pre-existing failures and three of the four bar failures, and it is too big to
  be a phase tail.
- Find the coupling behind phase 4 before attempting HM-DEC-116 again. The
  measurement in section 4 item three narrows it to something the settled pass
  reads out of the estimator.
- Still outstanding and untouched: `cw-2026-08-18-003758` is not on the machine
  (HM-OPEN-026), `prosigns-easy` reads `IR` for `AR`, and the 400 Hz tracker
  will not hold a pitch it finds.

# 4. What's blocking us

---
date: 2026-08-18
refs: CLAUDE.md §0.0, §12.1, §12.5; HM-DEC-112
---

**The clock fit's bias is measured on this detector before any correction is
applied to it, and HM-DEC-112's remaining two parts wait for that measurement.**

HM-DEC-112 says the gate catches the rising and falling skirts, so every mark
measures long and every gap short by the detector's own fall time. **Measured on
this detector, that is not what happens.** The gate reads 100–110 ms for a true
100, 45–50 for a true 48, and 40–45 for a true 40: accurate to within one hop at
every speed. The half-amplitude correction reads 80–90, 30–35 and 25 for the same
three, which is wrong by 15, 30 and 37 percent and worse the shorter the mark.

The shed is a fixed 15–20 milliseconds whatever the speed, which is the analysis
window rather than the transmitter's fall time. A Goertzel window rounds the top
of a short mark, so the width at six decibels below its own apex is far less than
its base. **The ruling's measurement was made with a different analysis window
from the one Hamlet decodes through**, and the correction it prescribes is
carrying that window's shape into the clock.

What is wanted before trying again is one number: for a synthesized dit of known
length, what does this detector's envelope actually do at the edges, at 12, 25
and 30 words a minute. That is a morning's work with the harness that already
exists, and it would say whether the fix is a shorter edge window, sub-hop
interpolation, or nothing at all.

Rejected: shipping the mark correction alone, which leaves the speed readout 16
percent high, and the speed is what a beginner uses to decide whether he could
have copied something. Rejected: shipping all three as ruled, which takes the
suite from 13 failures to 29 and silences 30 words a minute entirely.

---
date: 2026-08-18
refs: CLAUDE.md §0.0, §12.1; HM-DEC-097; HM-DEC-117
---

**The refusal floor is 14.**

The sweep is in section 1 and it is unambiguous. Fourteen and thirteen are the
last floors that never invent a character at any level, and both read the whole
message perfectly down to one decibel — the four decibels seventeen gave away,
recovered, with the property HM-DEC-097 exists for intact.

**Fourteen rather than thirteen** because it is the further of the two from the
first floor that does invent, and they are otherwise identical on every number
in the table. Twelve begins inventing at minus two decibels, which is the exact
case HM-DEC-097 names at 0.44 of the message invented, so thirteen sits one step
from the cliff and fourteen sits two.

Rejected: keeping seventeen. It costs four decibels of reach and buys nothing —
every floor from seventeen down to thirteen has the same worst invented share,
which is none.

Rejected: choosing it here. §12.1 puts a number that decides what the display
asserts with you without exception, and this one has been guessed wrong once
already, which is why it was measured this time.

---
date: 2026-08-18
refs: CLAUDE.md §0.0; HM-DEC-116; HM-DEC-091
---

**HM-DEC-116 waits until the coupling between the two passes is identified.**

The implementation is straightforward and the ruling's own acceptance is met:
`NothingIsInventedAtTheHandover` passes, tone-finding on the two-station
recording passes, and the app's transcript test starts passing. **What it costs
is the callsign on capture `cw-2026-08-17-013347`**, where the settled pass goes
from `■■■ ■■VA3VRR` to `■■■ ■`.

That is a feedback loop rather than a tuning problem: the estimator adopting the
settled pass's classes changes state the settled pass then reads back. Three
narrowings were tried and none broke it. Disabling adoption restores the
callsign, which is what identifies the cause but not the path.

The path is what is needed. The settled pass takes one thing from the estimator
— the dit hint — and if that is the whole coupling then the fix is to hand the
classes forward without letting them touch anything the dit is derived from.
Finding out is an afternoon with a trace on both passes.

Rejected: shipping it and accepting the swap. A real off-air capture outranks a
synthesized app test, which is this project's own repeated lesson: two real
recordings did in an afternoon what seven synthesized fixtures could not
(HM-DEC-091). Rejected: leaving HM-DEC-116 unattempted and silent, which is why
this is here with the measurement.
