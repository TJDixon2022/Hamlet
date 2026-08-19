# 1. What Claude did

**STATE.** Development computer, `C:\Source\HamLet`, Claude Code surface (§9.5).
**Branch: `main`.** Prompt, `WORK_INSTRUCTIONS.md` and `PROJECT_CARD.md` all say
`PROJECT: Hamlet`; gate passed on all three (§9.6). **No radio was connected**
(HM-DEC-093). Status written at the start and at every phase boundary.

**Phases 1 to 4 worked. Phase 5 dropped.** Five commits.

## Phase 1 — the de-glitch fix is shipped

`width = 2n + 1`. The bulletin goes **36 → 37 of 47** on the existing metric:
`STAAION` becomes `STATION`, `HAND■` becomes `HANDNG`. `AHIS` survives, so that
substitution has a second cause.

**And the earlier report of its cost was wrong, which I should say plainly.** I
wrote last session that four characters ahead of `VA3VRR` become placeholders.
Measured properly this time, before and after the one line, on the same capture:

- before: `■■■ ■■VA3VRR`
- after: `■■ ■■VA3VRR`

**Every letter is identical and one unreadable marker is gone.** Nothing was lost;
the count fell below the ratchet because a placeholder stopped being emitted. I had
inferred "four characters" from a failing length assertion instead of reading the
two strings. The ratchet is re-baselined by that one glyph, with the reason
recorded, and its substantive assertions — reaches the prefix, carries the whole
callsign — are untouched.

## Phase 2 — the leading edge, measured for the first time, and it inverts the assumption

**Every accuracy figure this project has published was the settled pass's.** On the
ARRL bulletin capture, aligned against the answer key by longest common
subsequence:

| Pass | Before phase 1 | After phase 1 |
|---|---|---|
| **Leading edge** — what he watches arrive | **13 of 43**, 19 emitted | **13 of 43**, 19 emitted |
| Settled — the record kept afterwards | 31 of 43, 36 emitted | **33 of 43**, 37 emitted |

The leading edge reads `O T ■T  T ■T ■ O   ■  ISE SSRG E ■`. The settled pass
reads `N L D O T NET ■E ECH STATION HANDNG AHIS MESAGE P`.

**"The leading edge is already good" was true of synthesized fixtures and is false
on real off-air audio.** Every report this week has repeated it, including mine.

**The streaming path does not have the same bug and does have the same exposure.**
`CwGate.FollowSpeed` sizes its vote window in **hops**, not milliseconds —
`ShortestVote` is 5 — so it is not halving anything. But five hops removes about
twenty-five milliseconds, and the split fragments on that recording run to fifty,
so the same marks survive there too. That is the likeliest reason the leading edge
reads 13, and it is the highest-value thing left in the decoder.

**My first metric was wrong and I caught it before publishing.** A greedy in-order
walk reported 3 of 43 for a reading plainly carrying `DOT NET`, `STATION` and
`MESSAGE`; it mis-anchors on a decode that starts mid-acquisition. Replaced with a
proper alignment.

## Phase 3 — the keying verdict expires while he is still sending

`CwDecoder.RunSettledPass` emits nothing unless `_tracker.KeyingRecently` is true.
That gate is HM-DEC-095's and it is right — a carrier once produced two hundred
characters of confident nonsense.

**`KeyingRecently` is a six-survey counter and a survey runs every half second, so
it is false three seconds after the survey last saw keying.** Measured:

| | `exchange-easy` | `coverage-easy` |
|---|---|---|
| audio | 32.0 s | 40.4 s |
| last character read by the leading edge | 31.3 s | 39.7 s |
| trailing silence | **0.7 s** | 0.7 s |
| keying protected when the pass drained | **false** | true |

**The trailing silence is identical, so the ending is not the cause.** The
protection expired *while the station was still sending*. The verdict comes from
the keying-structure detector, which needs enough marks in three seconds to see two
clusters; `exchange-easy` is twenty-seven characters over thirty-two seconds, so a
three-second window often holds two or three. **A slow sender with big gaps is
exactly who a newcomer works.**

Not repaired: every candidate changes what the display asserts about whether a
signal is there (§0.0, §12.1). The clock refusal on the same fixture is recorded as
measured — `Clock`, fitted dit of zero — and **explicitly not proved**, because
sparsity is a plausible shared cause and this project has been burned by naming a
suspect without a mechanism. HM-OPEN-051.

## Phase 4 — the table, and HM-DEC-097's premise does not hold

| Generated | Decoder's own margin | Correct | **Invented** | Emitted |
|---|---|---|---|---|
| +18 to +4 dB | 36.8 → 23.2 dB | 100% | **0%** | 9 |
| +2 dB | 21.5 dB | 97% | **0%** | 8 |
| **0 dB** | **19.8 dB** | 14% | **0%** | 1 |
| −2 dB | 19.5 dB | 3% | **0%** | 0 |
| −4, −6 dB | 17.2, 16.8 dB | 0% | **0%** | 0 |

**Nought decibels broadband is 19.8 on the decoder's own scale** — the number that
ruling needed and never had.

**And the decoder invents nothing, at any level.** HM-DEC-097 says that at minus
two decibels it emits a full message of which forty-four per cent is invented.
Today it emits **nothing at all** there. The existing gates already produce silence
rather than confident nonsense, which is what the ruling wanted, reached another
way.

Cost of a floor: at 20 it refuses four levels, the best reading 14%; at 18, two
levels reading 0%; at 16 and below, none. **Nearly free, and it buys nothing
measurable.** And the invention that does exist — `STAAION` — is at strong signal
and is the split-mark fault, so a noise floor would not have prevented it.
HM-OPEN-052.

## Phase 5 — dropped

The gap-fit seeding improves the record kept afterwards, not the text he watches
arrive, and the order names it as the drop.

# 2. What Tim should expect

**13 of 43 before, 13 of 43 after.** That is the leading edge on the ARRL bulletin
— the text that appears character by character while you listen — and it is
unchanged by today's fix, because today's fix was in the settled pass.

**The second number is 31 of 43 before, 33 after**, and that is the transcript kept
afterwards.

**What that means at the radio tonight:**

- **The live text is the weak half, and now we know by how much.** It reads a third
  of what the transcript reads on real off-air audio. Nothing this week improved
  it, and nobody had measured it, so every report has been calling it "already
  good" on the strength of synthesized fixtures.
- **The transcript is two characters better and reads `STATION` where it read
  `STAAION`.** If you are keeping a record of a contact, it is better than this
  morning.
- **If the transcript stops mid-contact on a slow sender**, that is HM-OPEN-051 and
  it is the keying verdict timing out three seconds after the last burst it
  recognised. It is not you and it is not the radio.
- **Keep the audio.** Everything in phases 2 to 4 came from two captures you kept.
  The next thing worth fixing — the streaming path's own de-glitch — will be found
  the same way.

**The suite: 2,001 tests, 3 failing**, the same three as this morning. If you see
four, something new is wrong.

# 3. What we should do next

- **The streaming de-glitch.** Phase 2 says the fragments survive there too and
  that path is what he reads. It is the same class of change as phase 1 and the
  measurement to judge it by now exists.
- HM-OPEN-051, the keying verdict, which needs your ruling first.
- HM-OPEN-052: whether HM-DEC-097 is satisfied by the silence the existing gates
  produce.

# 4. What's blocking us

Two rulings, below, and neither blocks tonight.

---
date: 2026-08-19
refs: HM-OPEN-051, HM-DEC-095, §0.0
---

**Whether the settled pass may emit when the keying verdict has expired.**

The verdict is false three seconds after the survey last saw keying, and on a
sparse sender it expires mid-message: `exchange-easy` has its last character 0.7 s
from the end and is already unprotected. Everything the pass reads from then on is
discarded.

Three ways, each with a real cost:

- **Lengthen the protection.** Weakens the carrier guard by exactly the amount it
  is lengthened, and that guard exists because a carrier produced two hundred
  characters of nonsense.
- **Exempt the final drain.** Keeps the guard for live decoding, not for the end of
  a recording, which is where a callsign usually is.
- **Give the settled pass its own keying evidence** rather than the survey's.
  Largest change, most principled, and it is a different measurement of the same
  question.

Rejected as a session's choice: all three. Each changes what the display asserts
about whether a signal is there.

---
date: 2026-08-19
refs: HM-OPEN-052, HM-DEC-097
---

**Whether HM-DEC-097 is satisfied by the silence the existing gates already
produce.**

That ruling has been carried for two days as the largest outstanding §0.0 failure,
on the strength of a measurement — 44% invented at −2 dB — that does not reproduce.
The sweep now shows nothing invented at any level, and emission falling to nothing
below 0 dB on its own.

Either the ruling is satisfied and the open item closes, or an explicit floor goes
in anyway as a guard against a case this sweep does not cover. **If it goes in the
number is 19.8** on the decoder's own scale, and the cost is one level that reads
14%.

Rejected as a session's choice: setting the number. It decides what the display
asserts.

## Asks still outstanding

Six, per HM-DEC-139 and scoped by HM-DEC-140.

| Ask | First made | Waiting on | Where it sits |
|---|---|---|---|
| **Whether an attended automatic cycle may reach an antenna** (§0.2, HM-DEC-098) | 2026-08-17 | The bench evening | Built and armed, dummy load only |
| **A callsign too long for one keyer send** (HM-DEC-130) | 2026-08-18 | The seam measured at the bench | The panel says it will split while he types |
| **Whether the star asks for a name at the moment of saving** (HM-DEC-060, HM-DEC-134) | 2026-08-18 | Nothing but the ruling | Favorites are born unnamed |
| **Whether Hamlet may ever ask the radio to send its spectrum** (HM-DEC-062, HM-OPEN-042) | 2026-08-18 | The ruling | Not asked at all |
| **Whether the settled pass may emit with the keying verdict expired** (HM-OPEN-051) | 2026-08-19 | The ruling; three ways above | The transcript stops mid-contact on sparse sending |
| **Whether HM-DEC-097 is satisfied by existing silence** (HM-OPEN-052) | 2026-08-19 | The ruling; the number is 19.8 if it goes in | No floor exists; nothing is invented in the sweep |

**Dropped since it was asked**: whether the de-glitch is widened — ruled A and
shipped in phase 1.

---

## Named and left, as the order directs

The four older asks, none built around. No transmit work toward auto-CQ. **No
records work**: nothing was tidied, swept or renumbered, and the only entries
written are the three findings these phases produced. **Phase 5 dropped.**
