# READ IN THIS ORDER - A the phase goal, B this step and its exits, C this report

A. THE PHASE GOAL is that Hamlet reads FT8 as well as the best decoder there is and
then reads it further - concretely the 1.5 dB between the `HM-OPEN-067` 50 per cent
crossing near -19.5 dB and the published -21 dB, and step 2 is the step that holds it.
Step 0 is closed and not re-audited (header `partial`, last entry `done`, entries win).
Step 1 is `done` at four of four and is still true tonight: the OSD-off column equals
the port on every rung. Step 2 is `partial` and the number is still moving. Steps 3, 4,
5 and 6 are open and untouched - not one line - though step 4 gained a measured
argument for itself, `HM-OPEN-074`. WHAT THIS UNIT DID TO THE NUMBER: the decode rate
at -21 dB over 306 trials went from 4.2 per cent (13 of 306), 0 wrong, to 10.8 per cent
(33 of 306), 0 wrong, 95 per cent Wilson 7.8 to 14.8. The interpolated 50 per cent
crossing moved from -19.54 dB to -19.81 dB: about 0.27 dB of the 1.5 closed, about
1.2 dB still out there.

B. THIS STEP AND ITS EXIT CRITERIA, one by one. Five of six must-pass met.

| # | Step 2 exit | Verdict |
|---|---|---|
| 1 | 40 per cent at -21 dB on 306 trials | **NOT MET.** Figure reached: **10.8 per cent (33 of 306)**, Wilson 7.8-14.8, 0 wrong |
| 2 | Stays open while the number moves; closes *unachievable* with a figure when it does not | **MET, and it says stay open.** 4.2 to 10.8 per cent is a move outside its own interval |
| 3 | Zero wrong decodes across the whole ladder | **MET.** 0 wrong on all nine rows: 3 rungs x 3 columns x 306 trials |
| 4 | Order and search weight with the cost each buys, measured | **MET.** Orders 0-3 at -21 dB over 51 trials, same seed and noise draw per row |
| 5 | From Fossorier and Lin 1995 and the QEX paper, cited at the point of use, no WSJT-X source read | **MET.** New `src/Ft8Sharp.Deep/porting-notes.md` and XML remarks; neither WSJT-X nor `ft4_ft8_public/` opened |
| 6 | Worst-case time per slot inside 15 s with margin | **MET.** Worst observed slot **110.2 ms** on 14 candidates - a **136x** margin |
| 7 | *nice-to-pass* - decodes per slot on real captures | **NOT MET.** Needs a fixture nobody has (`HM-OPEN-073`, Tim's). Gates nothing |

Exit 1 is the one not met, and what it needs is not a better ordered statistics
decoder. The figure reached is 10.8 per cent; what was tried is orders 0, 1, 2 and 3,
all measured with their costs. Task 1's ceiling - taken against the codeword the ladder
knows it transmitted - admits 7 of 51 trials at order 2, 10 of 51 at order 3 and 16 of
51 at order 4, and order 4 is about two and a half million re-encodings per candidate.
So 40 per cent is not reachable this way at any order that fits in a slot, and what
exit 1 needs is whatever holds the other 1.2 dB.

C. THIS REPORT, weighed against A and B. The thing here is task 1.3's ceiling
distribution, because it says whether the remaining decibel is reachable by any amount
of code searching. It says it is not: the closest candidate carries a median 31
hard-decision errors of 174, about 6 of them inside the 91 most reliable positions -
enough for order 2 to buy something real and nowhere near enough for any tractable
order to reach 40 per cent. And 49 of 51 trials did have a candidate near the signal
and were still not decoded, so it is not in synchronisation either, and unit 222
settled that it is not in the ratios. That bears directly on A and on B's exit 1.
Section 4 raises 2 items, neither of which asks for a ruling and neither of which
stands in the way of an exit criterion in B: the `RULES_AT` mismatch this unit was told
to report once, and a recorded fact about `HM-OPEN-074` that step 4's unit inherits.

```
UNIT:       246 — complete at task 8 of 8 — 2026-09-05 00:11
PHASE GOAL: Hamlet reads FT8 as well as the best decoder there is, and then reads it
            further — concretely, the 1.5 dB between -19.5 and -21.
UNIT GOAL:  Ft8Sharp.Deep reaches codewords belief propagation refused, by re-ordering
            bits by reliability and searching low-weight patterns among the most
            reliable ones, with every codeword accepted or refused by the port's own
            parity and CRC-14 gates and never by OSD's own say-so.
ADVANCED:   yes — the -21 dB rate on the 306-trial ladder moved 4.2 -> 10.8 per cent
            with 0 wrong, and five of step 2's six must-pass exits are met.
NUMBER:     4.2 per cent (13 of 306), 0 wrong -> 10.8 per cent (33 of 306), 0 wrong.
            Suites: Ft8Sharp.Tests 586 passed / 0 failed / 1 skipped in 5 m 23 s;
            Ft8Sharp.Deep.Tests 35 passed / 0 failed / 0 skipped in 997 ms.
DRIFT:      0 consecutive units without advance  (was 0)
```

## 1. What Claude did

**COMPLETE, at task 8 of 8. Nothing was dropped, including the named drop
candidate.** Machine `C:\Source\HamLet`, project confirmed Hamlet by all four gate
checks (`SHACK_FACTS.md` present, `src/Hamlet.RadioEngine/Cw/CwProbabilisticDecoder.cs`
present, no `CoreHMI.sln`, no `MURC.sln`), branch `main`, eight commits pushed.

### The eight tasks

**Task 1 — the trace, and it is the finding this unit owes the phase.**

*1.1 The generator.* Read off the port's own encoder rather than unpacked from
`Ft8Tables.LdpcGenerator`, whose 83 rows are the parity checks in upstream's packing:
row *i* of **G** is `LdpcEncoder.Encode` of the payload with bit *i* set. Checked two
ways — the 91 rows form the identity on columns 0–90, and **500 random payloads,
87 000 codeword bits, agreed bit for bit** with `LdpcEncoder.Encode`. **It did.**

*1.2 The population*, one whole 51-trial block at -21 dB, from `Ft8SlotResult`'s own
five counts: **667 candidates, 3 reaching parity, 3 passing the checksum, 3 becoming
text, 0 duplicates, 3 of 51 trials returning their own message, 0 empty searches.**

*1.3 The ceiling.* See §3. Median closest distance **31 of 174**; median errors inside
the 91 most reliable positions **6**; **2 of 51 trials with no candidate near the
signal at all.**

*1.4 Where the reproduction has to be exact.* `Ft8SlotDecoder.Decode(Ft8Waterfall)`,
`src/Ft8Sharp/Dsp/Ft8SlotDecoder.cs`, stage by stage:

| Line | What |
|---|---|
| `:143` | `_search.Find(waterfall)` — `Ft8SyncSearch.Find(Ft8Waterfall)` at `Ft8SyncSearch.cs:175` |
| `:149` | **one `Ft8CallsignCache` for the slot**, created here, dropped on return |
| `:151`–`:152` | the message list and the `seen` key list |
| `:154`–`:157` | the four running counters; the fifth, `CandidateCount`, is `candidates.Count` at `:216` |
| `:159`–`:160` | the ratio buffer (`Ft8SoftSymbols.RatioCount`) and codeword buffer (`LdpcDecoder.CodewordBits`), allocated once outside the loop |
| `:164` | `Ft8SoftSymbols.Extract(waterfall, candidate, ratios)` |
| `:165` | `Ft8SoftSymbols.Normalise(ratios)` |
| `:167` | `Ft8CodewordDecoder.Decode(ratios, cache, MaxIterations)` |
| `:169`–`:172` | `paritySatisfied++` for **every status that is not `ParityNeverSatisfied`** — so `ChecksumFailed` counts here |
| `:174`–`:177` | `checksumPassed++` for `Decoded` **or** `MessageNotReadable` |
| `:179`–`:182` | anything but `Decoded` stops here |
| `:184` | `becameText++` |
| `:195`–`:196` | **the de-duplication key: `LdpcDecoder.Decode` re-run over the same ratios**, then `codeword[..Ft8Payload.MessageBits]` |
| `:198`–`:202` | `AlreadySeen` → `duplicates++` and `continue` |
| `:204`–`:209` | the `MessageLimit` stop — **`continue`, not `break`, and it counts as nothing**; `becameText` was already incremented |
| `:211`–`:212` | `seen.Add(key)`, `messages.Add(new Ft8SlotMessage(candidate, result))` |

**The dedup key at `:195` is the thing an OSD decode must not do**, and task 4
measured why.

**Task 2 — the reproduced loop, OSD off, identical to the port.** `Ft8DeepSlotDecoder`
stopped delegating and now runs those stages itself through public members only.
**I extended that type rather than adding one beside it**, because the harness's
`Available()` seat and the two existing `Decode` overloads are what the scoreboard
reads; a second type would have left the seat measuring the old path. An `Osd`
setting arrived with it, **null by default, null meaning do exactly what the port
does**. Ruling 4's identity test compares the **whole `Ft8SlotResult`** — five counts
and every message's text, candidate, frequency and dt, in order — over one 51-trial
block at -19 dB, one at -21 dB, the committed capture `ft8-example-244.wav`, and **all
69 reference recordings, 801 messages**. Every one matched.

**I changed a tripwire unit 245 left on purpose, rather than a test breaking.**
`Ft8DeepSlotDecoderTests`'s `Assert.Single(types)` plus its refusal of any type named
`Osd` or `Ordered` was written to force this unit to come here deliberately. It now
asserts the sibling's whole type list, so the next unit that adds one has to come here
too. Each identity test also asserts `deep.Osd is null`.

**Task 3 — the OSD core.** `Ft8DeepOrderedStatistics`, from Fossorier and Lin 1995,
cited in the new `src/Ft8Sharp.Deep/porting-notes.md` and in XML remarks at the point
of use. Reliability ordering; most reliable basis by GF(2) elimination visiting columns
in that order; order 0 re-encode; order λ over every subset of size 1 to λ; ranked by
soft distance. Tests plant errors at known positions with every magnitude equal, so the
basis is positions 0–90 by construction: **λ errors inside the basis recover at order λ
and λ+1 do not, for λ = 0, 1, 2, 3**; twenty errors below the basis are overwritten for
free at order 0; re-encoding counts pinned at **1, 92, 4187, 125 672**. On degenerate
input — all zero, all equal, all infinite, all not-a-number, alternating extremes and
twenty draws of noise — **the elimination returned 91 distinct independent columns every
time and nothing threw**, and what came back was a codeword every time.

**A decision I made for myself, in full.** The "is it a codeword" check goes through
`LdpcEncoder`, not `LdpcDecoder`. The port **refuses the all-zero word outright** —
upstream's *message converged to all-zeros, which is prohibited* — so on all-zero
ratios its decoder reports a perfectly valid codeword as a failure. Re-encoding the
recovered word's own first 91 bits and requiring all 174 back asks the same question
with no such exception. It also means an OSD that returns the all-zero codeword can
never produce a decode, which is a safety property and not a defect.

**Task 4 — the gate.** `Ft8DeepOrderedStatistics.Saturate` makes ±1 ratios, puts them
on upstream's scale through the port's own `Ft8SoftSymbols.Normalise`, and hands them
to `Ft8CodewordDecoder.Decode`. **The port's parity gate and CRC-14 gate are the only
acceptance.** A codeword OSD got *right* came back `Decoded`, `"HAMLET 246"`, one
iteration. A codeword OSD got *wrong* — five basis errors against an order-1 search —
came back **34 bits from the one that was sent, a genuine codeword with 0 unsatisfied
checks**, and the port refused it: `ChecksumFailed`, empty text. And the dedup key:
on a candidate of the shape OSD is offered, belief propagation spent 25 iterations,
left **47 checks unsatisfied**, and its answer disagreed with OSD's codeword in **1 of
the 77 key bits** — which is why the key is taken from OSD's own codeword.

**Task 5 — OSD in the loop.** Run only where the port's per-candidate result is
`ParityNeverSatisfied`. **The stopping rule, both halves:** a *candidate* stops when
the enumeration for its order is exhausted — it always ends with exactly one codeword,
the best by soft distance, and that one is submitted once, with no retry and no second
order; a *slot* stops when the candidate list runs out, with no early exit and no
budget, so the worst case is the candidate limit times the order's re-encoding count
and that is measured rather than believed. `Ft8DeepOsdCounts` carries offered,
produced, accepted and re-encodings.

**Task 6 — the order table.** §3.

**Task 7 — the scoreboard, whole.** §3.

**Task 8 — the write-up. Not dropped.** `docs/unit246-osd.md` carries the ceiling
distribution and the order table so the next unit does not re-measure them.
**`HM-OPEN-074` opened**, because the numbers earned it: two of 51 trials had no
candidate within 60 of the transmitted codeword, so no coding work can reach them and
step 4's baseband re-sync is where they live. **The other half of task 8's condition did
not hold and nothing was opened for it** — the errors do *not* sit outside the most
reliable basis; the median trial carries about 6 of its 31 inside it, which is exactly
why order 2 buys anything.

### Verify this instruction against the tree

**Every line checked. Two mismatches, both trivial, neither repaired.**

- `Ft8Tables` `LdpcM` 83 `:46`, `LdpcN` 174 `:49`, `LdpcKBytes` 12 `:52`,
  `LdpcGenerator` `:76`, `LdpcNm` `:165`, `LdpcMn` `:254`, `LdpcNumRows` `:434` — all
  exact. `LdpcEncoder.Encode` `:73` and `:91` — exact. `LdpcDecoder.Decode` `:136`,
  `LdpcDecodeResult` carrying `UnsatisfiedChecks`/`Iterations`/`ParitySatisfied` —
  exact. `Ft8CodewordDecoder.Decode` `:70` with GATE 1 at `:80` and GATE 2 at `:96` —
  exact. `Ft8Payload` `MessageBits` `:57`, `PayloadBits` `:66`, `Create` `:98`,
  `TryRead` `:165` — exact. `Ft8SoftSymbols` `:117`/`:287`/`:323`/`:351` — exact.
  `Ft8SyncSearch` `DefaultMinimumScore` 10 `:82`, `DefaultCandidateLimit` 140 `:88` —
  exact. `Ft8SlotDecoder.DefaultMessageLimit` 50 `:63` — exact.
- **Mismatch 1.** `Ft8LadderHarness.Available()` is at **`:194`**, not "about `:190`".
  Two entries, as stated.
- **Mismatch 2.** The one-type tripwire assertion is at
  **`Ft8DeepSlotDecoderTests.cs:164`**; `:156` is the `GetTypes()` line above it.
- Versions as stated going in: root `1.12.48`, `Ft8Sharp` **`0.10.7`**,
  `Ft8Sharp.Deep` `0.1.0`. Highest open issue id `HM-OPEN-073`.
- `PROJECT_STATUS.md` `RULES_AT` reads `HM-DEC-153 (2026-09-04)` while `CLAUDE.md` §1
  tops out at `CPS-DEC-0152`. **Still present. Reported once, here, and not
  reconciled.**
- `.run-unit/*` and the three root bookkeeping files were modified and uncommitted.
  **`PHASE_OUTCOME.md`, `PHASE_STATUS.md` and `RUN_LEDGER.md` went in with the first
  commit, as instructed.** `.run-unit/` was left to the launcher.

### The shell

Nothing new. `dotnet build`, `dotnet test` and `git` all ran. `git commit -F -` with a
heredoc was refused by the static analyser, so the message went into a file under
`.run-unit/scratch/` and `git commit -F <file>` ran — a permitted spelling found rather
than a stop. Writing under `.git/` was refused as a sensitive path. Nothing halted.

## 2. What the owner should expect

**`Ft8Sharp.Deep` now hears things the port cannot, and it is off by default.**
`new Ft8DeepSlotDecoder()` still reproduces `Ft8SlotDecoder` exactly; ordered
statistics decoding only runs when a caller passes `Ft8DeepOsdSettings`. The
scoreboard's OSD-on column is the only place it is switched on.

**`Ft8Sharp` is untouched and still `0.10.7`.** Not one file under `src/Ft8Sharp/` was
opened for writing, including its `porting-notes.md`. Root version went `1.12.48` →
`1.12.49`; `Ft8Sharp.Deep` went `0.1.0` → `0.2.0`, because it has a capability of its
own now.

### What will look wrong and is not

- **Step 2 reads `partial` in `PHASE_OUTCOME.md`, not `done`.** That is the correct
  state: the rate exit is not met and the number is still moving, which is exactly the
  condition step 2's own second exit says keeps a step open.
- **`Ft8Sharp.Tests` went 582 → 586 tests but only 5 m 15 s → 5 m 23 s**, despite the
  new scoreboard test alone taking 3 m 15 s. xunit runs test classes in parallel; the
  new work fitted inside the existing wall clock. Nothing was skipped.
- **`ParitySatisfiedCount` does not jump in the OSD-on column.** A codeword the port
  refuses leaves the port's verdict standing, so the five counts stay a report on the
  port's belief propagation. OSD's own three counts carry OSD's story. A rate that moved
  with no OSD activity behind it would therefore be visible as exactly that.
- **The OSD stage produces a codeword for nearly every candidate it is offered and the
  port refuses nearly all of them** — 11 451 offered across the ladder, 100 accepted.
  That is the ordinary case: most of what it is handed is noise, and the gate working is
  what the low acceptance rate looks like.
- **Order 2 is the default even though order 3 decoded more.** Order 3 bought one extra
  decode of 51 for 246 ms a trial, which 51 trials cannot separate from noise. The
  reasoning is on `Ft8DeepOsdSettings.Default` and order 3 is explicitly not ruled out.

## 3. What you should see

### 1. The three-column ladder, whole

```
decoder      requested  delivered  trials  DECODED  MISSED  WRONG    rate   lo 95   hi 95    ms/tr
Ft8Sharp         -19.0    -19.001     306      248      58      0    81.0    76.3    85.0     65.2
Deep OSD off     -19.0    -19.001     306      248      58      0    81.0    76.3    85.0     64.3
Deep OSD on      -19.0    -19.001     306      276      30      0    90.2    86.3    93.0     72.6
Ft8Sharp         -20.0    -20.000     306       73     233      0    23.9    19.4    28.9     64.8
Deep OSD off     -20.0    -20.000     306       73     233      0    23.9    19.4    28.9     64.2
Deep OSD on      -20.0    -20.000     306      125     181      0    40.8    35.5    46.4     72.6
Ft8Sharp         -21.0    -21.001     306       13     293      0     4.2     2.5     7.1     64.1
Deep OSD off     -21.0    -21.001     306       13     293      0     4.2     2.5     7.1     63.7
Deep OSD on      -21.0    -21.001     306       33     273      0    10.8     7.8    14.8     72.5

  Ft8Sharp     at -19.0 / -20.0 / -21.0 dB: NO WRONG DECODES.
  Deep OSD off at -19.0 / -20.0 / -21.0 dB: NO WRONG DECODES.
  Deep OSD on  at -19.0 / -20.0 / -21.0 dB: NO WRONG DECODES.
```

**The OSD-off column equalling the port is what makes the third column mean anything:**
because the sibling reproduces the port's loop decode for decode and miss for miss on
every rung — asserted per rung, not merely printed — a difference between the second and
third columns is attributable to ordered statistics decoding and to nothing else.

**The one number the phase reads: 4.2 per cent (13 of 306), 0 wrong → 10.8 per cent
(33 of 306), 95 per cent Wilson 7.8 to 14.8, 0 wrong. It cost 8.4 ms a trial, thirteen
per cent over the port.**

Interpolating the 50 per cent crossing between the -19 and -20 rungs, which is the
arithmetic `HM-OPEN-067`'s "near -19.5" was read off: **-19.54 dB → -19.81 dB**. About
**0.27 dB** of the 1.5 dB, about **1.2 dB** still out there.

### 2. The ceiling — the finding this unit owes the phase

One whole 51-trial block at -21 dB, seed 220791, against **the codeword the ladder knows
it transmitted**.

Closest candidate's hard-decision distance, all 51 trials, sorted:

```
15 16 20 21 21 25 25 25 26 26 27 27 27 27 28 28 29 29 29 29 30 30 30 31 31 31 31
31 31 32 32 33 34 34 34 35 35 36 38 38 38 39 40 40 41 42 43 44 45 71 81
```

Median about **31** — unit 222's figure — against a code whose iterative recovery
reaches zero at about **17**. That is why belief propagation gives up.

Of those errors, how many fall inside the **91 most reliable positions**:

```
0 1 2 2 2 2 2 3 3 3 4 4 4 4 4 4 5 5 5 5 5 6 6 6 7 7 7 7 7 7 7 7 8 8 8 8 8 8 9 9
10 10 10 10 10 11 11 11 12 41 44
```

| order λ | trials whose basis carries at most λ errors |
|---|---|
| 0 | 1 of 51 |
| 1 | 2 of 51 |
| 2 | **7 of 51** |
| 3 | 10 of 51 |
| 4 | 16 of 51 |

**Trials with no candidate within 60 of the truth at all: 2 of 51** — trial 3 at 81 of
174 and trial 7 at 71 of 174, against a chance distance of 87. `HM-OPEN-074`.

**What order this distribution admits, plainly.** Order 2 admits at most **13.7 per
cent** of trials, order 3 at most **19.6 per cent**, order 4 at most **31.4 per cent**
at about two and a half million re-encodings a candidate. **No tractable order admits
40 per cent.** And two caveats, both in the direction of *worse*: the count is taken
against the leading 91 positions and so is a **lower bound** on the true basis error
count, and reaching the codeword also requires it to **win** the soft-distance ranking
against the single submission allowed. **So step 2's 40 per cent is not reachable by
ordered statistics decoding, and 49 of 51 trials did have a candidate near the signal
and were still not decoded — so the remaining 1.2 dB is not in synchronisation either.**

### 3. The order table, and the time budget

One whole 51-trial block at -21 dB, **every row seeing the same seed and the same noise
draw**, delivered -21.004 dB on every row.

| row | decoded | missed | **wrong** | ms/trial | worst slot ms | offered | accepted | re-encodings |
|---|---|---|---|---|---|---|---|---|
| `Ft8Sharp` | 3 | 48 | **0** | 65.5 | 77.8 | 0 | 0 | 0 |
| Deep OSD off | 3 | 48 | **0** | 64.9 | 74.3 | 0 | 0 | 0 |
| Deep order 0 | 3 | 48 | **0** | 66.3 | 102.1 | 664 | 0 | 664 |
| Deep order 1 | 4 | 47 | **0** | 65.8 | 75.5 | 664 | 1 | 61 088 |
| Deep order 2 | 4 | 47 | **0** | 74.3 | 110.1 | 664 | 1 | 2 780 168 |
| Deep order 3 | 5 | 46 | **0** | 311.4 | 511.6 | 664 | 2 | 83 446 208 |

**Order 0 bought nothing** and is reported as buying nothing. **Order 1 bought one
decode of 51 for no measurable cost. Order 2 bought nothing over order 1 on this block
and cost 8.8 ms a trial. Order 3 bought one more and cost 246 ms a trial.**

**The default is set to order 2, and here is why, from this table.** One decode either
way is well inside the noise at 51 trials, so the gains cannot be separated and the
choice was made on cost against the headroom §3.2 measured: the ceiling admits 7 of 51
at order 2 against 2 of 51 at order 1, and order 2's worst observed slot is 110 ms — a
136-fold margin. Order 3 is **not** ruled out; separating it needs more trials, not a
bigger claim. Nothing was tuned to hit a target: no order in this table reaches 40 per
cent.

**Worst-case time per slot, over the whole 918-trial ladder, worst single slot rather
than the mean:**

| column | worst slot ms | its candidates | margin against 15 s |
|---|---|---|---|
| `Ft8Sharp` | 103.5 | 20 | 145× |
| Deep OSD off | 102.2 | 16 | 147× |
| Deep OSD on | **110.2** | 14 | **136×** |

The OSD stage across all three rungs: **11 451 candidates offered, 100 codewords the
port then accepted, 47 945 337 re-encodings spent.**

### 4. Both suites

| suite | before | after |
|---|---|---|
| `Ft8Sharp.Tests` | 582 passed / 0 failed / 1 skipped / 5 m 15 s | **586 passed / 0 failed / 1 skipped / 5 m 23 s** |
| `Ft8Sharp.Deep.Tests` | 18 passed / 0 failed / 0 skipped / 445 ms | **35 passed / 0 failed / 0 skipped / 997 ms** |

**The baseline matched unit 245's exactly**, which is itself a finding and a clean one.
**No red anywhere, so nothing is outside the expected set.** The one skip is
`Ft8TableGenerationTests.RewriteTheCheckedInTablesFile`, unchanged and not a reference
clone skip — the pinned clone was present and all 69 recordings ran.
`Hamlet.App.Tests` and `Hamlet.RadioEngine.Tests` were not run; nothing here touches
either, and the 51 inherited CW reds and
`CwAdjudicationTests.ASpeedChangeInRealisticAudio` are not in these suites.

### The record

`PHASE_OUTCOME.md` entry appended through
`dotnet build tools/arbiter/outcome-append.proj -p:EntryProps=C:/Source/HamLet/.run-unit/scratch/unit246-outcome.props`
— **`outcome-append exit 0`**, entry written as `## UNIT 4 - STEP 2` and the header's
step 2 state updated to `partial` in the same call. Nothing was written by hand and
nothing above the new entry was touched.

## 4. What's blocking us

**Nothing is blocking. Two items logged, neither asking for a ruling, neither standing
in the way of any exit criterion in B.**

### `RULES_AT` still disagrees with `CLAUDE.md`, reported once as instructed

`PROJECT_STATUS.md` `RULES_AT` reads `HM-DEC-153 (2026-09-04)` while `CLAUDE.md` §1 tops
out at `CPS-DEC-0152`. The work instruction said report once and go no further because
`CLAUDE.md` is the owner's file. **This is that once. It asks for no ruling and nothing
was reconciled.**

### `HM-OPEN-074` is opened and is step 4's to carry, not a question

Two of 51 trials at -21 dB had no candidate within 60 of the transmitted codeword, so
the sync search never returned a place where the signal was and there are no ratios for
any coding work to operate on. That is a floor under step 2 and a measured argument that
step 4's baseband re-sync has something to find. **It is logged, not asked.** It is also
explicitly *not* where the missing 1.2 dB is: two trials cannot account for a decibel,
and 49 of 51 trials did have a candidate near the signal and were still not decoded.

### The one line the arbiter needs

**Step 2 is not closed, and the number is still moving** — 4.2 to 10.8 per cent at
-21 dB with zero wrong, five of six must-pass exits met. **But the ceiling says this
approach cannot reach 40 per cent at any order that fits in a slot**, so the next unit
on step 2 would be taking a different approach, not tuning this one, and the arbiter has
a distribution rather than an opinion on which to decide whether to do that or move to
step 3 or step 4 instead.
