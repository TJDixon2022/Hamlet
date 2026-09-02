READ IN THIS ORDER — A the phase, B the step and its exit criteria, C this report against both.

A. PHASE — Hamlet hears FT8 off the radio and displays the decoded text on screen. Seven steps.
Steps 1 and 2 closed. Step 3 closed on its four must-pass criteria, its nice-to-pass one recorded
as HM-OPEN-065. Step 4 closed by unit 214. **Step 5 is this unit's and its third**, entered at
2 of its 3 subject criteria with criterion 3 partial at 760 of 1298, and **leaves at 760 of 1298,
still partial** — but no longer unexplained. Steps 6 and 7 not started and cannot start, because
every step of this plan depends on the one before it, the plan's own named deviation. **Step 5 was
the only step this phase could move**, and criterion 3 its only outstanding must-pass criterion.

B. STEP 5 — a found signal becomes a message. Five exit criteria.
1. Corrupted codeword recovered / beyond-power failing honestly — met by unit 215. Tonight bears
   on it: the agreement histogram says where recovery actually stops on real air, and it is a
   slope, not a cliff.
2. A candidate failing CRC is never returned — met by unit 216 at four counts of zero. **Task 6's
   second pass was measured and NOT built, so nothing touched this criterion**; `Ft8SlotDecoder`
   is byte-for-byte what unit 216 left.
3. `ft8_lib`'s reference WAVs decode against its expected decode lists — **entered at 760 of 1298,
   leaves at 760 of 1298, against a ceiling measured tonight at 1157. PARTIAL, not met.** The
   number did not move and this report says so in those words.
4. `Ft8Sharp` green — entry 470/469/0/1, exit 485/484/0/1, the one skip the table write gate.
5. Attribution clean and channels green — 174 paths from `2828ab6`, 0 under `src/Hamlet.` or
   `tests/Hamlet.`, channels 55 and 9 re-run after both version bumps.

C. THIS REPORT — 8 findings, and the lead is the census. **470 validated codewords never became
text and every one now has a name: `UnsupportedType` 0 occurrences and 0 distinct,
`UnresolvedCallsign` 277 and 109, `MalformedField` 193 and 68** — summing to 470, against
2733 − 2263 = 470. **The ceiling is 1157 of 1298** and its whole shortfall is the 141 lines the
list itself lost to a hash. **The buckets: H1 69, H2 380, H3 82, H4 7, summing to 538.** The match
rate did not move. **Section 4 raises 2 items and ONE IS IN THE WAY OF CRITERION 3** — the price
of fix C, 82 messages found and thrown away, which is the owner's under CLAUDE.md §12.1.
**Task 7's drop branch applied and the census number that decided it is 0** distinct expected
messages against a bar of 20.

UNIT:       217 — complete at task 8 of 8 — 2026-09-02 12:00
PHASE GOAL: Hamlet hears FT8 off the radio and shows the decoded text on screen.
UNIT GOAL:  Give every one of criterion 3's 538 misses exactly one named cause, reconcile the 470
            validated codewords that never became text against them, and close what needs no ruling.
ADVANCED:   partly — the accounting is complete and the number did not move. No criterion newly met;
            criterion 3 stays partial at 760 of 1298 and is now fully explained rather than a
            percentage nobody could account for.
NUMBER:     760 of 1298 -> 760 of 1298 — unchanged, and unchanged on purpose: nothing was tuned,
            nothing was built, and the two fixes on offer were measured at zero and refused.
DRIFT:      0 consecutive units without advance (was 0) — unit 216 advanced, and this unit advanced
            the criterion's legibility rather than its value.

## 1. What Claude did

**Complete, at task 8 of 8. Nothing dropped by the clock; task 7 dropped by its own named
condition, which the census decided.** Machine `C:\Source\HamLet`, project Hamlet confirmed against
all four checks in the instruction's gate, branch `main`, HEAD `d990f95` at exit.

**No library file changed tonight.** `src/Ft8Sharp/` gained a version bump and a `porting-notes.md`
section and nothing else. Every number below came out of tests added under
`tests/Ft8Sharp.Tests/`.

### Task 1 — the ground, and the one run everything rests on

`Ft8Sharp` at entry **470 total, 469 passed, 0 failed, 1 skipped** in 37 s; the skip is
`Ft8TableGenerationTests.RewriteTheCheckedInTablesFile`, the table write gate, which is meant to
skip. Library builds **0 warnings, 0 errors**. **174** paths from `2828ab6` at exit (168 at entry)
and the `src/Hamlet.`/`tests/Hamlet.` filter returns **0**. HEAD at entry `96b34b9`,
`git status --short` **31** lines at entry and **37** at exit, versions **1.12.23** and **0.10.0**
at entry, **24** divergences, **8** `.obj` at the root.

**And the trace this unit is built on reproduced exactly**: 7803 candidates, 2733 parity, 2733
checksum, 2263 text, 783 unique, **760 matched of 1298**, 538 missed, 23 extra. Not one total
moved on its own, so the accounting stands on a run that repeats.

### Task 2 — what upstream does at the stage this library refuses at

Six inventory tests over the pin, read through the test process because the sandbox refuses a
session direct access to the clone. Each answer pinned by an assertion.

### Task 3 — the refusal census, the centre of the night

Instrumented by walking the same public parts `Ft8SlotDecoder` composes, candidate by candidate,
with one cache per slot — **and proving the walk faithful per file rather than on totals: 240
stage-count comparisons against the untold path, all equal.**

### Task 4 — the ceiling

Every one of the 1298 expected lines through this library's own packers, into one of four buckets
summing to 1298.

### Task 5 — the per-miss accounting

Every miss into exactly one of H1–H4, with the matched control and the chance figure beside it.
**The sharpest prohibition in the instruction was kept and is stated in the test file in those
words**: the search ran untold on all sixty recordings and its candidate list was read, never
filtered, re-ordered or re-scored; the expected text built a comparison codeword and nothing else;
the expected frequency chose which already-found candidate to look at, after the fact. **No
expected text, frequency, count or list reached `Ft8SlotDecoder`, `Ft8SoftSymbols`,
`Ft8SyncSearch`, `Ft8CodewordDecoder` or any signature they can see.**

### Task 6 — fix A, measured and refused

Measured at 1 of 109, worth 0 matches. **Not built.**

### Task 7 — fix B, dropped by its own condition

Census named 0 types against a bar of 20. **Not built.** `HM-OPEN-064` updated.

### Task 8 — criterion 3 re-taken, the record, the versions

Re-run through the untold path, identical to unit 216 on every total. `porting-notes.md` gains its
unit-217 section with **one divergence added, numbered 25**. Versions bumped and everything re-run.

### Decisions this session made for itself

1. **Not to build the second pass**, on its own measurement of 1 recovered of 109, none on any
   expected list. The instruction licensed building it if it paid; it did not pay.
2. **To measure the price of the hashed-callsign refusal with a rendering computed in the test
   project.** Producing the number section 4 needs meant knowing what a `<...>` line would have
   said. That rendering exists in `RefusalCensus.UpstreamWouldPrint`, is called only by a report,
   and never reaches a caller or the library. **The library still refuses the whole message.** If
   the owner reads that as too close to fix C, the number stands and the helper can go.
3. **To assign the 141 hashed misses on the candidate test alone**, since no true codeword can be
   built for them, and to print that count in the buckets table rather than let it pass as
   measured evidence.
4. **To leave a tripwire on task 6's zero** — a test that reds if the second pass ever becomes
   worth building — rather than recording a stale decision in a file nobody reads.

## 2. What the owner should expect

**The radio does not do anything new tonight.** This library returns exactly what it returned
yesterday, on exactly the same recordings: 760 of upstream's 1298 expected messages. **That is on
purpose.** Two changes were on the table, both were measured before being built, and both measured
at zero — so neither was made. A number that rises because something was adjusted until it rose is
not evidence about anything, and step 6 measures this receiver against a published sensitivity
threshold and would be worthless if the path had been tuned to sixty recordings.

**What is new is that the shortfall is no longer a mystery.** Yesterday the honest summary was
*760 of 1298 and nobody can say why*. Tonight it is: 141 of the missing can never be matched while
this decoder refuses to name a station it cannot identify; 82 of those were actually found,
repaired and checked twice before being thrown away; 380 were real transmissions too faint for the
error-correcting code; 69 were never found at all; 7 are duplicate lines in upstream's own list
that any de-duplicating decoder must miss; and exactly **one** is a case where this library had
the signal cleanly and failed anyway.

### What will look wrong but is not

- **`ADVANCED: partly` and an unchanged number.** The unit was told in terms it would not be judged
  on whether criterion 3 closed. A complete accounting with no movement is the outcome the
  instruction named as a full success.
- **`MalformedField` at 193 occurrences sounds like a defect and is not.** All 68 distinct ones are
  the grid-or-report field, and upstream refuses at exactly that place with `ERROR_GRID`. A test
  reds if one ever lands anywhere other than upstream's own three refusal points.
- **`UnsupportedType` at zero looks like the census failed to run.** It did run. Upstream's own
  message layer decodes exactly the four types this library builds, so there was never a type on
  these recordings that upstream reads and this does not.
- **`Ft8Sharp` went from 470 tests to 485, not to 488.** Three of the added tests were a temporary
  probe onto the clone, emptied to a comment before finishing so that what is on disk and what is
  committed compile to the same tests.
- **The version went to 0.10.1 and not 0.11.0.** Nothing the library does changed.
- **The App channel count reads 9 tonight where prior units reported 13.** See the mismatches
  below; the exact filter earlier units used is not recorded anywhere in the tree.

## 3. What you should see

**No visible change in the application. Nothing reaches a screen in this phase yet.** What this
unit produces is a decision the next one can make from evidence: the 538 messages Hamlet did not
show you off upstream's recordings now each have a named reason, and one of those reasons is a
question for you rather than a bug for anybody.

### The census — the answer this unit was commissioned for

```
Over all sixty of ft8_lib's off-air recordings:

  candidates                                   7803
  of those, satisfied all 83 parity checks     2733
  of those, carried their own CRC-14           2733
  of those, became text                        2263
  VALIDATED AND NEVER BECAME TEXT              2733 - 2263 = 470

refusal                 occurrences  distinct  on a list
UnsupportedType                   0         0          0
UnresolvedCallsign              277       109          0
MalformedField                  193        68          0
TOTAL                           470       177          0

  0 + 277 + 193 = 470, and 2733 - 2263 = 470.
```

**Distinct means one message once per recording**: a payload refused at five candidates in one slot
is one message and four duplicates. The occurrence column is the larger and more flattering one and
is never given alone.

**`UnsupportedType`, broken down by type code, as the instruction requires:**

```
type                      occurrences  distinct
(no rows)

NOT ONE of the 470 was refused for an unsupported type. The census names no type
at all, so task 7's bar of 20 distinct expected messages was not approached.
```

**The other two, by declared type code:**

```
UnresolvedCallsign        occurrences  distinct
NonstandardCallsign                74        24
Standard                          203        85

MalformedField            occurrences  distinct
Standard                          193        68
```

### `MalformedField` — its own paragraph, as required

**All 68 distinct malformed-field refusals are the grid-or-report field**, re-derived from the same
77 bits by calling the field readers directly. Upstream refuses at exactly three places —
`ERROR_CALLSIGN1`, `ERROR_CALLSIGN2`, `ERROR_GRID` — and every one of these lands on the third, so
**this is agreement with upstream and not a port defect.** A codeword that satisfied 83 parity
checks and a 14-bit checksum carrying a grid value the protocol does not define is a transmission
upstream would also print `Error [4] while unpacking!` for. The test asserts none lands outside
upstream's own three, so a real defect there would red rather than pass quietly.

### The join between the two ledgers, with its control

```
distinct refused payloads                        177
of those, matching an expected line on bits        0
expected lines this packer could represent      1157 of 1298

CONTROL — the packer against messages this library ITSELF decoded:
  messages decoded off the sixty recordings      2263
    packed back to THE SAME 77 BITS              2187   (96.6 per cent)
    the packer refused the text                     0
    packed, and to DIFFERENT bits                  76
```

**The zero can be read because the control says so.** The 76 that pack to different bits are
`CQ <non-standard call>` lines, where this library and upstream disagree about which wire format
carries them — unit 211's recorded finding, not a new one. **So none of the 470 corresponds by bits
to an expected line this library can represent**, and the only route from the refusals to the lists
is the hashed one below.

### The ceiling

```
outcome                                         lines    share
REPRESENTABLE - packs and round-trips            1157    89.1%
the LIST lost the callsign to a hash (<...>)      141    10.9%
no shape this library builds accepts it             0     0.0%
packed, and came back as different text             0     0.0%
TOTAL                                            1298   100.0%
```

**The highest score criterion 3 could reach with a perfect receiver, given this library's message
layer as it stands tonight, is 1157 of 1298 — 89.1 per cent.** And last night's result against it,
both readings, neither alone:

```
760 of 1298 expected lines            = 58.6 per cent
760 of a ceiling of 1157              = 65.7 per cent
the gap a receiver can still close    = 397 lines
the gap no receiver can close         = 141 lines
```

**The hashed lines are counted apart on purpose.** A line printed `<...>` has lost the callsign in
the *list*; nobody can re-pack it — not this library, not upstream, not whatever wrote the list.
Folding them into "lines this library refuses" would blame this port for somebody else's missing
information. **A ceiling below 1298 is not an excuse and is not used as one**: it says the
receive-side shortfall is smaller than 58.6 per cent suggests, and that a named part of the rest
sits in a decision the owner holds.

### The buckets, with the matched control and the histogram

```
bucket  count  what it means
H1         69  no candidate on that transmission - none within 4 Hz, or one
               within 4 Hz agreeing below 100 of 174, which is chance
H2        380  the signal was there and too weak for the code to recover
H3         82  RECOVERED, past parity and CRC, AND THE MESSAGE LAYER REFUSED IT
H4          7  decoded and matched nothing anyway - de-duplication or the
               text comparison
TOTAL     538  and the miss total is 538

  of which assigned WITHOUT an agreement figure, because the list itself lost
  the callsign and no true codeword can be built for them:  141
```

```
agreement      misses   matched
0-79                6         3
80-89              22         0
90-99              36         0
100-109            47         0
110-119            61         6
120-129            73         0
130-139            43         1
140-149            68         2
150-159            29        19
160-164             4        21
165-169             1        33
170-173             2        47
174                 5       137

misses with an agreement figure:   397   mean 122.8
matched control, sampled:          269   mean 167.7
CHANCE, on a candidate placed where there is no signal: 84.8 of 174 (expected 87)
```

**It is a slope, not a cliff**, and the two say completely different things about this port. A
cliff would mean a threshold sitting in the wrong place; a slope is what a real channel looks like.
The misses spread right across 100 to 160 while the matched pile up from 160 to 174, with 137 of
269 at 174 exactly. **The chance control landed at 84.8 against the 87 a bit-is-a-bit predicts**,
which is what makes the rest of the column mean anything.

**The indictment set is one row.** Eight misses reached 165 of 174 or better; seven are H4
duplicates rather than failures. The only genuine one:

```
20m_busy/test_34.wav      1344 Hz   within 4 Hz   agreement 165   H2   JG2PQN F1BHB -24
```

**So extraction is not indicted by this night's measurement**, and the next unit does not have that
target. That is a finding worth as much as a fix.

### Criterion 3 re-taken, in unit 216's exact columns

```
file                     rate   secs  samples  cand  par  crc  txt  uniq  exp  match  miss  extra
191111_110115.wav       12000  15.00   180000    24    0    0    0     0    1      0     1      0
191111_110130.wav       12000  15.00   180000    40    9    9    9     4    5      4     1      0
191111_110145.wav       12000  15.00   180000    32    5    5    3     1    2      1     1      0
191111_110200.wav       12000  15.00   180000    37   11   11   11     4    5      4     1      0
191111_110215.wav       12000  15.00   180000    44    9    9    6     2    4      2     2      0
191111_110615.wav       12000  15.00   180000   140   54   54   52    16   22     16     6      0
191111_110630.wav       12000  15.00   180000   140   39   39   39    12   15     11     4      1
191111_110645.wav       12000  15.00   180000   140   48   48   44    15   20     15     5      0
191111_110700.wav       12000  15.00   180000   140   46   46   44    13   16     13     3      0
20m_busy/test_01.wav    12000  15.00   180000   140   50   50   39    15   24     13    11      2
20m_busy/test_02.wav    12000  15.00   180000   140   56   56   36    13   24     13    11      0
20m_busy/test_03.wav    12000  15.00   180000   140   33   33   33    12   19     12     7      0
20m_busy/test_04.wav    12000  15.00   180000   140   49   49   45    14   20     13     7      1
20m_busy/test_05.wav    12000  15.00   180000   140   54   54   52    19   32     19    13      0
20m_busy/test_06.wav    12000  15.00   180000   140   49   49   41    17   27     17    10      0
20m_busy/test_07.wav    12000  15.00   180000   140   56   56   43    15   31     15    16      0
20m_busy/test_08.wav    12000  15.00   180000   140   53   53   45    15   19     14     5      1
20m_busy/test_09.wav    12000  15.00   180000   140   49   49   42    16   27     16    11      0
20m_busy/test_10.wav    12000  15.00   180000   140   54   54   48    16   20     14     6      2
20m_busy/test_11.wav    12000  15.00   180000   140   52   52   45    16   31     16    15      0
20m_busy/test_12.wav    12000  15.00   180000   140   47   47   41    12   18     12     6      0
20m_busy/test_13.wav    12000  15.00   180000   140   56   56   44    16   26     16    10      0
20m_busy/test_14.wav    12000  15.00   180000   140   44   44   27    10   17     10     7      0
20m_busy/test_15.wav    12000  15.00   180000   140   63   63   50    16   28     16    12      0
20m_busy/test_16.wav    12000  15.00   180000   140   44   44   41    15   16     14     2      1
20m_busy/test_17.wav    12000  15.00   180000   140   53   53   44    15   26     15    11      0
20m_busy/test_18.wav    12000  15.00   180000   140   47   47   31    11   20     11     9      0
20m_busy/test_19.wav    12000  15.00   180000   140   64   64   49    17   30     17    13      0
20m_busy/test_20.wav    12000  15.00   180000   140   51   51   35    12   20     11     9      1
20m_busy/test_21.wav    12000  15.00   180000   140   52   52   40    15   34     15    19      0
20m_busy/test_22.wav    12000  15.00   180000   140   58   58   36    12   23     12    11      0
20m_busy/test_23.wav    12000  15.00   180000   140   51   51   35    13   26     11    15      2
20m_busy/test_24.wav    12000  15.00   180000   140   52   52   33    12   22     11    11      1
20m_busy/test_25.wav    12000  15.00   180000   140   55   55   47    17   28     17    11      0
20m_busy/test_26.wav    12000  15.00   180000   140   49   49   34    12   23     12    11      0
20m_busy/test_27.wav    12000  15.00   180000   140   53   53   42    15   29     15    14      0
20m_busy/test_28.wav    12000  15.00   180000   140   43   43   28    11   25     11    14      0
20m_busy/test_29.wav    12000  15.00   180000   140   50   50   38    14   23     12    11      2
20m_busy/test_30.wav    12000  15.00   180000   140   56   56   47    15   27     15    12      0
20m_busy/test_31.wav    12000  15.00   180000   140   53   53   40    14   24     12    12      2
20m_busy/test_32.wav    12000  15.00   180000   140   57   57   49    19   25     17     8      2
20m_busy/test_33.wav    12000  15.00   180000   140   56   56   45    14   28     14    14      0
20m_busy/test_34.wav    12000  15.00   180000   140   48   48   34    12   25     12    13      0
20m_busy/test_35.wav    12000  15.00   180000   140   60   60   43    13   32     13    19      0
20m_busy/test_36.wav    12000  15.00   180000   140   45   45   32    11   24     11    13      0
20m_busy/test_37.wav    12000  15.00   180000   140   54   54   47    13   24     13    11      0
20m_busy/test_38.wav    12000  15.00   180000   140   40   40   35    11   19     11     8      0
websdr_test1.wav        12000  15.00   180000   140   35   35   35    13   18     13     5      0
websdr_test10.wav       12000  15.00   180000   113   30   30   30    12   15     12     3      0
websdr_test11.wav       12000  15.00   180000   140   44   44   32    10   23     10    13      0
websdr_test12.wav       12000  15.00   180000    99   12   12   12     7   14      6     8      1
websdr_test13.wav       12000  15.00   180000   140   36   36   35    12   13     10     3      2
websdr_test2.wav        12000  15.00   180000   140   50   50   46    18   21     18     3      0
websdr_test3.wav        12000  15.00   180000   134   28   28   27     8   11      8     3      0
websdr_test4.wav        12000  15.00   180000   140   61   61   55    19   23     18     5      1
websdr_test5.wav        12000  15.00   180000   140   49   49   41    15   27     15    12      0
websdr_test6.wav        12000  15.00   180000   140   57   57   57    20   30     20    10      0
websdr_test7.wav        12000  15.00   180000   140   49   49   47    17   27     16    11      1
websdr_test8.wav        12000  15.00   180000   140   55   55   53    17   26     17     9      0
websdr_test9.wav        12000  15.00   180000   140   50   50   49    13   24     13    11      0
TOTAL                                          7803 2733 2733 2263   783 1298    760   538     23

THE CHANGE ON EVERY TOTAL, against unit 216:
  candidates  7803 -> 7803   (0)      unique    783 ->  783   (0)
  parity      2733 -> 2733   (0)      expected 1298 -> 1298   (0)
  checksum    2733 -> 2733   (0)      MATCHED   760 ->  760   (0)
  text        2263 -> 2263   (0)      missed    538 ->  538   (0)
                                      EXTRA      23 ->   23   (0)
```

**The match rate did not move, and it did not move because nothing was changed.** Both fixes on
offer were measured first and both measured at zero. **The extras did not rise**, which is the
number that mattered most tonight: every measurement here was aimed at the message layer, and the
one thing this project refuses is a message on Tim's screen that nobody sent. The 23 are the same
23 unit 216 printed, unchanged, and no fix raised the count because no fix was made.

### The 538 against the 531 — reconciled, and 538 is right

```
expected lines in all sixty lists:                     1298
lines that repeat another line in the SAME list:          9
of those, repeats of a message this library RETURNED:     7

  191111_110615.wav   x3  RETURNED         PA3EPP SP8NFO KN09
  191111_110645.wav   x2  RETURNED         PA3EPP SP8NFO R+01
  websdr_test11.wav   x3  never came back  K4VBM HA8EK RR73
  websdr_test4.wav    x2  RETURNED         SM2EKA UT7IS KN98
  websdr_test6.wav    x2  RETURNED         SM2EKA UT7IS -06
  websdr_test9.wav    x3  RETURNED         K4VBM HA8EK -15

the totals row, a MULTISET comparison:      538 missed   <- the criterion's own
the diagnostic, a CONTAINMENT comparison:   531 missed
the difference:                               7
repeats of a message that DID come back:      7
```

**538 is right.** The criterion's table compares as a multiset, so a list carrying a message twice
is not satisfied by one decode. The diagnostic asks only whether the text came back at all, so it
scores every copy of a repeated line as found. The other two repeats never came back at all, so
both comparisons agree about them. **None of the seven is a lost message**: each was returned and
de-duplicated to a single decode by upstream's own payload rule. They belong to the ceiling, not
the shortfall, and the accounting puts them in H4.

### Task 2's findings, with the anchoring split

**STRONG — declarations in `ft8/message.h`:** `ftx_message_type_t` with eleven enumerators;
`ftx_message_rc_t` with six values, **not one naming an unresolved hash**; and
`ftx_callsign_hash_interface_t`, two function pointers and nothing else, so the library owns no
hash storage at all and the application supplies it.

**WEAK — expressions inside static function bodies:** `lookup_callsign`'s placeholder write; the
switch inside `ftx_message_decode`; the one-pass shape of the demo's `decode` helper; and the hash
table's storage and lifetime.

**WEAKEST — one `snprintf` in the demo:** `"Error [%d] while unpacking!"`.

1. **What upstream does with a hashed callsign it cannot resolve.** `lookup_callsign` writes the
   literal `"<...>"` into the callsign buffer and **returns the miss to its caller** — and neither
   caller looks. `unpack28` calls it and then unconditionally `return 0; // Success`;
   `ftx_message_decode_nonstd` calls it as a bare statement and returns `FTX_MESSAGE_RC_OK`. **So
   upstream prints a message naming a station it cannot name, and reports the decode as clean.** A
   resolved hash is bracketed by `add_brackets`, so `<CALL>` is upstream's form too and the two
   sides agree there.
2. **Which types upstream actually prints.** Exactly four: `FREE_TEXT` (0.0), `TELEMETRY` (0.5),
   `STANDARD` (1 and 2) and `NONSTD_CALL` (4). Its default branch reads `// not handled yet` and
   returns `FTX_MESSAGE_RC_ERROR_TYPE`. Declared and never decoded: `DXPEDITION` (0.1), `EU_VHF`
   (0.2), `ARRL_FD` (0.3/0.4), `CONTESTING` (0.6), `ARRL_RTTY` (3), `WWROF` (5). **These are
   exactly the six `HM-OPEN-064` records, so upstream does not build them either.** And the demo
   does not drop a line it cannot read — it prints `Error [n] while unpacking!` as the text. **No
   expected list in the pin carries that string**, one more independent confirmation that these
   lists were not written by the pinned decoder.
3. **Does upstream re-offer a refused payload? No — stated plainly.** One `ftx_find_candidates`,
   one loop over the candidates in score order, one `ftx_decode_candidate` each, and **exactly one
   `ftx_message_decode` call site in the whole application**, inside the branch that has just
   entered a new payload in the duplicate table. **So fix A would have been an addition and a
   numbered divergence, not a port.**
4. **Is upstream's hash table per-slot, per-file or per-process? Per-process, and aged.**
   `callsign_hashtable` is a file-scope `static struct` array in `demo/decode_ft8.c` with a
   `static int callsign_hashtable_size` beside it; `hashtable_init()` is called **once**, in
   `main`, before the slot loop; and `hashtable_cleanup(10)` runs at the **end of every** `decode()`,
   ageing entries in the top byte of their hash word and evicting those older than ten slots. **So
   upstream can name a station from a callsign heard up to two and a half minutes earlier.** Unit
   208's per-slot ruling stands and the lifetime was not changed; **the difference is recorded as
   divergence 25.**

### Task 6 — fix A measured, and refused

```
distinct payloads refused for an unresolved callsign:  109
  resolved when re-offered at the end of the slot:       1
  still refused, because the slot never heard the call: 108
  of those resolved, on an expected list:                0

  20m_busy/test_29.wav   not on a list  (already returned)  <LZ365BM> US5IQI KN87
```

**It would be worth zero matches.** The one payload it resolves is a message the same slot had
already returned, so de-duplication would drop it — and that text is one of unit 216's 23 extras,
so the only thing building it could have done is risk the extras count. **108 of 109 belong to
stations whose callsign was never spelled out anywhere in the same fifteen seconds**, so waiting
cannot help them. **The hypothesis that decode order costs this library messages is removed
permanently.** No determinism re-run was needed because nothing changed; `Ft8SlotDecoder` is
byte-for-byte what unit 216 left, which is also why criterion 2 cannot have been disturbed.

**A tripwire is left behind**: the test asserts the zero, so a later re-pin or a better receiver
that makes the second pass worth building reds this test rather than leaving a stale decision.

### Every refusal watched refusing

- **The one that must still refuse after task 6**, watched without the clone on a message built in
  memory: a hashed companion is refused **cold**; refused **again** against a warm cache holding
  three other calls; and **decoded** only once the slot has actually heard its owner, coming back
  as `<PJ4/K1ABC> W9XYZ/R RR73`. **So waiting never weakens the gate.** HM-DEC-009 untouched, no
  placeholder written, nothing invented.
- **The unsupported-type remainder**, still watched: 15 type combinations enumerated, 5 built and
  round-tripping, **10 refused as `UnsupportedType`**, and not one returning a wrong decode. Step
  2's must-pass clause holds.
- **The malformed-field refusals**, asserted to land only on upstream's own three refusal points.
- **The packer's own refusals**, named rather than lumped: `HashedCallsignLostInTheList` 141,
  `NoShapeThisLibraryBuildsAcceptsIt` 0, `PackedButDidNotRoundTrip` 0.

### Task 7's branch, and the number that decided it

**The drop branch applied.** The census named **0** types against a bar of **20** distinct expected
messages — not below the bar, not near it. Nothing was built. `HM-OPEN-064` is updated with the
measurement, kept open, kept `owner: claude`, and now also records the correction that its stated
field layouts are **not** in the pin's `ft8/message.c`: only the comment table of field widths in
`message.h`, so building those six would be protocol work against the QEX paper rather than
porting, with no upstream oracle for a round trip.

### The price of fix C, as a number

```
UPPER BOUND - expected lines printed <...>:   141 of 1298  (10.9 per cent)
REALISED    - of those, whose codeword this library RECOVERED and then
              refused, so a placeholder would have made them matches:  82
```

**82 transmissions were found, corrected, checked against a 14-bit checksum, and thrown away** —
6.3 percentage points of criterion 3, or 7.1 per cent of the 1157-line ceiling. The rendering that
measured this lives in the test project, is called only by a report, and never reaches the library.
**The library still refuses the whole message.** This goes to section 4.

### The extras — every one printed, count unchanged at 23

```
191111_110630.wav      JH1AJT W4FGA EM83
20m_busy/test_01.wav   OE3MLC G3ZQQ 73
20m_busy/test_01.wav   JO1COV PA0CAH JO21
20m_busy/test_04.wav   CQ MM0IMC IO75
20m_busy/test_08.wav   RW6PA UA3NFG 73
20m_busy/test_10.wav   CQ LZ365BM
20m_busy/test_10.wav   SP9LKP F4VTS 73
20m_busy/test_16.wav   UR7HN UA3NFG LO28
20m_busy/test_20.wav   CQ 2E0LDW IO70
20m_busy/test_23.wav   YC6RMT IZ7NLM -22
20m_busy/test_23.wav   7Z1AL DF2FE JO51
20m_busy/test_24.wav   CQ G0OSK IO91
20m_busy/test_29.wav   DM2DLG UR7HN -13
20m_busy/test_29.wav   <LZ365BM> US5IQI KN87
20m_busy/test_31.wav   DM2DLG UR7HN -13
20m_busy/test_31.wav   RA3TPE BD8NBG -17
20m_busy/test_32.wav   DH1NAS UA3NFG LO28
20m_busy/test_32.wav   E75C RA9UJP NO25
websdr_test12.wav      CT7AIX WG5D EM62
websdr_test13.wav      CQ 2E0PKK IO90
websdr_test13.wav      CQ N2BJ EN61
websdr_test4.wav       UT7IS SV8EUB -12
websdr_test7.wav       SQ7MRR ON7AN JO20
```

**No fix raised this count, because no fix was made.**

### Divergences, versions, counts, and what was committed

**One divergence added and numbered 25** — the callsign cache lives for one slot where upstream's
lives for the process and is aged over ten. It has been true since unit 208 and had never been
written down; **recording it is the change, not the behaviour.**

**Versions.** `src/Ft8Sharp/Directory.Build.props` **0.10.0 → 0.10.1** under HM-DEC-152, a patch
because the night was measurement only and the library gains evidence rather than a capability —
unit 211's arbiter's precedent. Root `Directory.Build.props` **1.12.23 → 1.12.24** under HM-DEC-150.

**Re-run after both bumps.** `Ft8Sharp` **485 total, 484 passed, 0 failed, 1 skipped** in 38 s.
**The one skip is `Ft8TableGenerationTests.RewriteTheCheckedInTablesFile`**, the table write gate,
whose reason names the environment variable that would run it — it is meant to skip. Library builds
**0 warnings, 0 errors**. Channels **55** (RadioEngine) and **9** (App), both green with
`VersionTests` among them. **174** paths from `2828ab6`, and the `src/Hamlet.`/`tests/Hamlet.`
filter returns **0**. **8** `.obj` at the repository root, counted at the end and untouched.
`git status --short` prints **35** at exit, the four added since entry being this session's own
untracked scratch: `Unit217Probe.cs`, `tools/unit217/`, `unit217-status.py`, and `OUTPUT.md` itself
awaiting its commit.

**No new shared artifact was added, so no new channel was needed.**

**Committed:** `PROJECT_STATUS.md`, `OPEN_ISSUES.md`, both `Directory.Build.props`,
`src/Ft8Sharp/porting-notes.md`, and five test files under `tests/Ft8Sharp.Tests/` —
`Message/UpstreamMessageLayerInventoryTests.cs`, `Message/ExpectedMessagePacker.cs`,
`Dsp/Ft8RefusalCensusTests.cs`, `Dsp/Ft8CriterionCeilingTests.cs`, `Dsp/Ft8MissAccountingTests.cs`
and `Dsp/Ft8SecondPassMeasurementTests.cs`. **Nine commits — one per task and one for this report —
each pushed before the next task started; every push accepted first time, no refusals from GitHub.**

**Left alone:** the 8 `.obj` at the root; `tools/build-ft8-oracle.bat`, which is present, untracked
and was not run; `PHASE_OUTCOME.md`; the loop's uncommitted files — modified
`tools/arbiter/run-phase.bat` and `run-unit.bat`, and untracked `ARBITER.md`, `MANIFEST.txt`,
`PHASE_PLAN.md`, `RUN_LEDGER.md`, `VERIFY_PASS.md`, `SCRUB_SELFTEST.bat`, `SESSION.lock`,
`.run-unit/` and `docs/phase-uplift/`; the six probe and scratch files known item 10 names; and
`src/Ft8Sharp/Tables/Ft8Tables.g.cs`, read for declarations and not edited.

**`PHASE_STATUS.md` was edited on one line only** — `WORK_INSTRUCTION:` set to
`217 - where the other 538 die, and every one of them given a name`, as the session prompt directs.
It was not committed, `HEARTBEAT:`, `CURRENT_STEP:` and the `STEP:` lines were not touched, and
nothing below the `---` was changed. **The work instruction's known item 14 says not to hand-edit
that file at all**; the two directions conflict and the session followed the prompt's explicit,
narrower one. Reported here rather than resolved.

### Mismatches against the instruction — reported, not repaired

1. **Task 7's field layouts are not where the instruction says.** It directs that they *come from
   the pin's `ft8/message.c`, read the way units 207 and 208 read it.* **They are not in
   `ft8/message.c`.** `ftx_message_decode` has four cases and a default commented `// not handled
   yet`; the only description of those six types anywhere in the pin is the comment table of field
   widths in `ft8/message.h`. Recorded in `HM-OPEN-064`.
2. **The 141-hashed-lines figure is confirmed**, exactly as unit 216 measured it — the arbiter
   flagged it as a likely mismatch and it is not one.
3. **`git status --short` printed 31 at entry, not the 30 the instruction states.** The extra line
   is `SESSION.lock`. **35 at exit**, the four added being this session's own untracked scratch —
   `Unit217Probe.cs`, `tools/unit217/`, `unit217-status.py` — and `OUTPUT.md` awaiting its commit.
4. **Attribution is 174 paths at exit, not the 168 the instruction cites from unit 216's report.**
   The difference is this unit's own six added files.
5. **The App channel count is 9 under a reconstructed filter, where prior units reported 13.** The
   exact filter earlier units used **is not recorded anywhere in the tree** — the plan names the
   three channels and `DecisionLogOrderTests` but no command. The RadioEngine half reconstructs
   exactly: `AudioSeamTests` plus `PrivilegeTests`, the two tests reading `CLAUDE.md`/`DECISIONS.md`
   in that project, gives **55**, unit 215's and 216's own figure. The App half was reconstructed
   from the same principle — `DecisionLogOrderTests` for the `CLAUDE.md` §1 rows, `VersionTests` for
   the root version, `EveryResourceKeyResolvesTests` and `ViewTestsActThroughControlsTests` for
   `Hamlet.sln` membership — and gives 9. **Both are green and neither reds.** Whether 13 counted
   something else cannot be established from the tree, and a channel list nobody can reconstruct is
   the failure the plan's own line warns about.
6. **A directory was created under `tools/` in error at the start of the unit** —
   `tools/unit217/status.py`, a status-writing helper, before the session re-read known item 14's
   prohibition. **The sandbox refused every attempt to delete the file and the directory**, in four
   spellings. It is emptied to a comment naming the mistake, it is untracked, and it was not
   committed. The helper that replaced it is `unit217-status.py` at the repository root, also
   untracked and also not committed. **Reported as a refusal and not routed around.**
7. **`tests/Ft8Sharp.Tests/Dsp/Unit217Probe.cs` is left behind, emptied to a comment**, the same
   way units 214, 215 and 216 left theirs. No deletion was attempted; known item 10 says not to try.
8. **`PHASE_STATUS.md` is stale and `PHASE_OUTCOME.md`'s header disagrees with its own entries.**
   Both confirmed and neither touched beyond the one line above.

### The validator, refused for the seventh unit running

**All five spellings `tools\arbiter\run-unit-tools.txt` lists were tried and none ran the script.**
`cmd /c ...`, `cmd.exe /c ...` and `cmd.exe //c ...` were **denied outright** by the permission
layer. `cmd //c tools\arbiter\validate-output.bat output.md` and
`tools\arbiter\validate-output.bat output.md` were **permitted and still failed**, for a reason
worth recording because it is not the one units 211 to 216 hit: the Bash tool is Git Bash, a POSIX
shell, and it **strips the backslashes** before `cmd` ever sees the path — so the batch file is
invoked as `toolsarbitervalidate-output.bat` and cmd reports it does not exist. Quoting the path
would fix the shell and breaks the permission match, and that form was denied too.

**Reported as a refusal and not routed around.** No alternative interpreter was used, the script was
not copied, and nothing was renamed to make a permitted spelling resolve. **If one line is added to
`tools\arbiter\run-unit-tools.txt` for the next unit, the useful one is a quoted spelling** — the
listed forms cannot work from a shell that eats backslashes.

**The six rules were therefore checked by hand against the script's own source, and all six pass:**

```
rule 1  UNIT: line present, parseable, above section 1     line 35, section 1 at line 47   ok
rule 2  four top-level sections, in order, exact names     lines 47, 124, 159, 667         ok
rule 3  no fifth top-level section                         `grep -c "^## "` returns 4      ok
rule 4  section 4 present                                  line 667, ASCII apostrophe      ok
rule 5  section 3 non-empty                                lines 160 to 666                ok
rule 6  ordering block above UNIT:, in the first 60 lines   READ IN THIS ORDER  line 1
        A. line 3, B. line 11, C. line 25, and C names a
        count as a digit - "raises 2 items" at line 30                                     ok
```

## 4. What's blocking us

**Two items. One is in the way of criterion 3 and it is genuinely the owner's.**

### 1 — May a decoder ever tell you *a station I cannot name*? Worth 82 messages tonight.

**The ruling requested.** Whether `Ft8Sharp` may return a message in which a callsign it could not
resolve is rendered as a placeholder — `<...>`, the way `ft8_lib` does — rather than refusing the
whole message.

**The reasoning, with the number in front of it.** This library refuses. Upstream does not:
`lookup_callsign` writes the literal `"<...>"` and both its callers discard the miss and report
success, so upstream's own lists carry those lines. **141 of the 1298 expected lines are of that
shape — 10.9 per cent of criterion 3 — and no improvement to this receiver can ever match one of
them while the refusal stands, because matching would mean printing the placeholder.** Of those
141, **82 were transmissions this library found, corrected, and validated against a 14-bit checksum
before discarding.** They are 6.3 points of criterion 3 and 7.1 per cent of the 1157 that are
reachable at all. **A warm cache does not recover them**: task 6 measured re-offering every
unresolved payload at the end of its slot and got 1 of 109, none on any list.

**What was rejected, and why it was not decided here.** The change itself was forbidden tonight and
correctly so. The refusal is a deliberate divergence, number 8 in `porting-notes.md`, taken under
HM-DEC-009 and tested since step 2, and `CLAUDE.md` §12.1 puts what the display asserts to Tim with
Tim. **Neither this session nor an arbiter may rule on it.** No recommendation is offered here.
What can be said factually is what the two answers cost: refusing costs 82 real messages on these
sixty recordings; printing costs a line naming a station the receiver cannot identify, in a project
whose prime directive is that a decode nobody transmitted is worse than a blank screen.

### 2 — Carried forward, not a ruling request: where the residue actually lives

**380 of the 538 misses are real transmissions too weak for the code to recover**, at a mean
hard-decision agreement of 122.8 of 174 against 167.7 for the messages that came back. That is not
a defect with an address; it is a sensitivity statement, and **the measurement that would judge it
is step 6's**, against the published threshold, which this unit is forbidden to invoke. **The
extraction indictment the instruction anticipated did not materialise**: exactly one miss had the
signal cleanly present at 165 of 174 and still failed. The next unit does not have that target and
should not be sent looking for it.

**Nothing else needs a decision.** The reference decoder binary is not re-raised: it is
`HM-OPEN-065`, a standing owner-side item, and tonight's question did not need it.
