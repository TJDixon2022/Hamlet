# 1. What Claude did

## Phase 2's finding: they were never written

**`git log -S "id: HM-DEC-096"` across all paths and all history returns nothing.**
The same is true of 100, 113, 120, 129 and 133. Those strings have never existed in
this repository, so there is nothing to recover and no commit to blame for losing
them.

Three independent readings of the history agree:

| Evidence | What it says |
|---|---|
| `git log -S "id: HM-DEC-…"`, all paths, all history | Zero commits for every id sampled across the range |
| `DECISIONS.md` line count at each of its last touches: 5,056 → 5,174 → **5,331 (HM-DEC-095, 08-17)** → **5,369 (HM-DEC-134, 08-18)** → 5,448 → 5,494 → 5,545 → 5,582 | The file has only ever grown. Nothing was deleted |
| Commits touching `DECISIONS.md` | Between `e6d97d2` on 08-17 and `22fdf09` on 08-18 the file was **not touched at all**, and those are exactly the commits carrying 095 and 134 |

**Where it starts, and what those commits were doing.** `d6b4ce2`, 2026-08-17 at
16:26, added the §1 row for HM-DEC-096 alongside `BATCH_BRIEF_AMENDMENT.md`,
`CLAUDE.md` and `SESSION_PROTOCOL.md` — and did not touch `DECISIONS.md`.
`83d58d1` at 16:57 and `c1a76f8` at 21:22 did the same for the rulings around 097
and 113. All three are your commits of chat deliveries, and a chat delivery writes
`CLAUDE.md` and its brief files. **The entry the row pointed at was never in the
zip.** Every ruling from 096 to 133 followed that pattern.

**The rule was already there, in that wording, and nothing enforced it.** I checked
`CLAUDE.md` as it stood immediately before `d6b4ce2`: §1 already said "Detailed
records live in `DECISIONS.md`; this table is the index." §3 already required the
file to hold rulings with `id` and `date`, and §5's definition of done already
required the records updated in the same delivery. So this is not a rule that was
missing. It is **HM-OPEN-044's shape one more time**: a correct rule with nothing
comparing the two artifacts.

**Nothing was repaired, and no ruling text was drafted.** The order's condition for
mechanical restoration was not met, so I stopped exactly there.

## One ruling is worse off than the other thirty-seven

Reconciling every id in `CLAUDE.md` §1 against `DECISIONS.md`:

- **37 ids have a row and no entry**: 096–104 and 106–133.
- **HM-DEC-105 has neither.** No row, no entry, nothing.
- **No entry lacks a row**, and **136 is correctly absent** by your ruling.

HM-DEC-105 was real and was acted on. It is cited by **HM-DEC-112's own row** ("HM-DEC-105 already put half amplitude in the settled pass"), twice by
`DECODER_AND_SCANNER_BRIEF.md`, and by two entries in `OPEN_ISSUES.md`. Everything
anybody knows about it is a clause inside another ruling's summary. Under any of
the three repair shapes it needs its own answer, because there is no row to promote.

## One index row conflicts with the history

**HM-DEC-113's row is dated 2026-08-18 and was added by `c1a76f8` on 2026-08-17 at
21:22.** Every other row I sampled — 096, 097, 098, 104, 129, 133 — agrees with its
commit. Reported and not corrected, as the order directs. It may simply be a
ruling made late in the evening and dated for the next day, which is the same thing
that produced the 08-18/08-19 wobble in the last two orders.

## HM-OPEN-045, and phase 1

**HM-OPEN-045** records the finding with the evidence above and sets out three
repair shapes, ruling none: mark the seam and leave it; recover from the chat
transcripts if you still have those conversations, which is the only route to the
actual text; or promote the index rows as entries that say on their face that they
are index rows and that the reasoning was never recorded. **What a repair may not
be** is a ruling reconstructed from its own one-line summary, which would be a
session writing your reasoning and signing your name to it.

**Phase 1: HM-DEC-140 written verbatim** to `DECISIONS.md` at the head, its index
row at the true head of §1 (the head now reads 140, 139, 135, 138, 137, 134, and
the out-of-order pair is untouched), and `CLAUDE.md` §12.2 gained one sentence
stating the boundary so a session reading only that file does not have to infer it.

**On the date:** `ISSUED: 2026-08-18` against a clock now reading 2026-08-19. The
order is current, nothing is stale, and nothing stopped.

# 2. What Tim should expect

- **The hole is not recoverable from this repository.** If those chat conversations
  still exist, that is the only place the reasoning survives.
- **Nothing is broken by it.** All 38 rulings are in force and indexed; what is
  missing is the reasoning, which is exactly what a session two months from now
  will want when it is deciding whether one still applies.
- **Two commits, pushed to `main`. No source file changed** — `git status` on
  `src/` and `tests/` was empty at both.
- **The suite is untouched**: 1,969 tests, 2 failing, the standing decode baseline.
- **`DECISIONS.md` still reads 001–095 then 134 onwards.** I did not put a marker at
  the seam, because which marker is one of the three shapes and that is yours.
- **The queue is five now.** HM-OPEN-045 joins it.

# 3. What we should do next

- Rule HM-OPEN-045, and say in particular what happens to HM-DEC-105.
- If the answer is recovery, the chat transcripts for 2026-08-17 and 2026-08-18 are
  the only source; if it is promotion, it is mechanical and I can do it in one pass
  without writing a word of reasoning.
- HM-OPEN-036 whenever §1 is opened deliberately, which the promotion option would
  be a natural moment for.

# 4. What's blocking us

Nothing is blocked; every affected ruling is in force. One new ask, in the queue
below.

## Asks still outstanding

Five, per HM-DEC-139 and scoped by HM-DEC-140. Carried verbatim until ruled.

| Ask | First made | Waiting on | Where it already sits in the tree |
|---|---|---|---|
| **Whether an attended automatic cycle may reach an antenna** (§0.2, HM-DEC-098) | 2026-08-17 | Every interlock watched to fire into the dummy load, per `BENCH_CARD.md`, including the link pulled mid-cycle | Built and armed: `AutoCaller`, `AutoCallAnswers`, the widget on the making-contacts preset. Dummy load only until this is ruled |
| **A callsign too long for one keyer send** (HM-DEC-130) | 2026-08-18 | Five minutes at the bench measuring the gap between two sends into the load | Refused, not split. `CwMessage.Split` exists and is unused for this |
| **Whether the star asks for a name at the moment of saving** (HM-DEC-060, HM-DEC-134) | 2026-08-18 | Nothing but the ruling; handed back under §12.1 clause 3 as a trade-off | Favorites are born unnamed from places the operator was. The manage window renames them afterwards |
| **Whether Hamlet may ever ask the radio to send its spectrum, and if so when** (HM-DEC-062, HM-DEC-092, HM-OPEN-042) | 2026-08-18 | The ruling. Three ways were put: leave the switch to the operator, ask once on a button, or ask automatically once the counters show the stream is not eating the link | **Not asked at all.** The automatic `27 11` was removed on 2026-08-18 and HM-DEC-062 restored; the reads stay |
| **What repair the 096-to-133 hole gets, and what happens to HM-DEC-105** (HM-OPEN-045) | 2026-08-19 | The ruling, and whether the 2026-08-17 and 2026-08-18 chat transcripts still exist | `DECISIONS.md` runs 001–095 then 134 onwards, with no marker at the seam. 37 ids have an index row and no entry; HM-DEC-105 has neither |

Nothing was dropped this session: no ask in the queue was ruled since the last
report.

---

## Named and left, as the order directs

Not started: HM-OPEN-036; the record sweep for rulings resting on a write outcome;
HM-OPEN-042's remaining rungs; mode follow, favorites and the recent list.
