READ IN THIS ORDER — A, then B, then C.

A. PHASE — Hamlet hears FT8 off the radio and displays the decoded text on screen. Seven steps.
Step 1 (the library exists and its tables are proven) closed. Step 2 (messages round-trip through
77 bits) closed. Step 3 (a valid FT8 signal can be produced) is where this unit worked and it is
NOT closed. Steps 4, 5, 6 and 7 remain unreachable, every step depending on the one before it by
the plan's own named deviation. Task 1 verified step 3's entry afresh rather than inheriting it:
Ft8Sharp at 148 total, 147 passed, 0 failed, 1 skipped in 5.9 seconds so step 2's ground is still
standing; the library building at 0 warnings and 0 errors on net8.0 with nullable on, warnings as
errors and no PackageReference and no ProjectReference; attribution 72 paths from 2828ab6 with not
one under any Hamlet project; and all three channels green at 55 and 13. This is the first unit of
step 3, which PHASE_PLAN.md calls the hinge of the phase, and step 3 did not close.

B. STEP 3 — a valid FT8 signal can be produced. FIVE exit criteria, FOUR must-pass. (1) LDPC
parity matches the reference for known payloads, must-pass — RE-TAKEN tonight from message text
rather than inherited from step 1's basis proof: 1431 real messages, 0 failures over all 83 checks.
The reading stood on is THE WEAKER ONE and is named so it is not assumed: the syndrome check
against the checked-in parity tables computed by the independent LdpcCheck, NOT a byte-for-byte
comparison against ft8_lib's own codeword, which could not be built. MET. (2) The symbol sequence
is bit-identical to ft8_lib's, must-pass — THIS UNIT'S TARGET. The comparison against upstream's
own tones DID NOT RUN. CRITERION 2 IS OPEN. What stopped it: task 2 ended in the second of its
three outcomes, "the pin has one and it would not build". There is no C toolchain on this machine.
Corpus size 14 messages, 0 messages compared against upstream, 0 matching symbol for symbol; the
corpus did include a message carrying a hashed callsign, but it was compared only against this
library's own second implementation. (3) Audio synthesis produces a signal the reference decoder
decodes, nice-to-pass — NOT BUILT TONIGHT, parked to the next unit by the instruction. NOT MET. (4)
Ft8Sharp tests green, must-pass — 171 total, 170 passed, 0 failed, 1 skipped, 4.0 seconds wall
clock. MET. (5) Attribution clean from 2828ab6 and the channel tests green, must-pass — 80 paths,
0 under any Hamlet project; AudioSeamTests and PrivilegeTests 55 green, DecisionLogOrderTests,
VersionTests, DecisionEmissionTests and VoiceTests 13 green, VersionTests re-run after the version
bump. MET.

C. THIS REPORT — the symbol sequence stands on TWO of the three legs and the third does not exist:
provenance against the pin exists (14 items corroborated by machine, none uncorroborated), an
independent second implementation exists (agreeing on 1106 symbols of 14 messages), and
bit-identity against upstream's own output DOES NOT EXIST. Task 2 in the three-way form: the pin
HAS a generator and the Makefile names it as a target, and it WOULD NOT BUILD — nothing resolves on
PATH, and the one cl.exe on the machine has no include folder beside it, no CRT import libraries
and no Windows SDK anywhere, so it cannot compile a program that includes stdio.h. Unit 208's
carried-forward debt is NOT settled: a message carrying a hashed callsign was in the corpus, but
the corpus never met upstream, so the hash still stands on two legs going into step 4. Task 6 was
NOT dropped; the branch that licensed keeping it is the second one, that task 5 was unreachable and
leg B is therefore the only evidence this unit can produce about the sequence. The Ft8Sharp test
project still returns in about four seconds and no corpus was cut for the clock. Section 4 raises
2 items, and the first of them is what stands in the way of criterion 2 in B.

UNIT:       209 — complete at task 8 of 8 — 2026-09-01 14:43
PHASE GOAL: Hamlet decodes FT8 off the operator's own antenna and puts the text on screen, closed
            by Tim seeing it work rather than by any test.
UNIT GOAL:  Make the library produce the 79 channel symbols an FT8 transmission actually sends, and
            settle that sequence against ft8_lib's own tone output rather than against this
            library's own arithmetic.
ADVANCED:   yes — step 3 moved from 0 to 3 of its 4 must-pass criteria, but NOT on the criterion
            this unit was written for; criterion 2 is open and the reason is a missing toolchain.
NUMBER:     step 3 must-pass criteria demonstrated: 0 -> 3 of 4
DRIFT:      0 consecutive units without advance  (was 0 — unit 208 closed step 2)

## 1. What Claude did

**Exit state: complete, at task 8 of 8. Nothing was dropped.** Machine: this one, Windows 11,
project claimed and verified as Hamlet, branch `main`. Every task committed and pushed as it
finished; six pushes, all succeeded, none refused.

**The gate passed against the tree, not against the prompt.** `SHACK_FACTS.md` present,
`src/Hamlet.RadioEngine/Cw/CwProbabilisticDecoder.cs` present, `CoreHMI.sln` and `MURC.sln` both
absent, `Hamlet.sln` the only solution.

**Task 1, the trace.** HEAD `9068064` on `main` as expected. `git status --short` printed **33**
lines against the arbiter's 32 — one more, not investigated, and known item 9 says report the count
and do not commit them. Ft8Sharp 148 / 147 / 0 / 1 in 5.9 seconds. Library build 0 and 0; `net8.0`,
nullable enabled, warnings as errors, no `PackageReference`, no `ProjectReference`, all confirmed
from the `.csproj`. Attribution 72 paths, 0 under any Hamlet project. Channels: `AudioSeamTests`
and `PrivilegeTests` 55 green; `DecisionLogOrderTests`, `VersionTests`, `DecisionEmissionTests` and
`VoiceTests` 13 green — every named class green. Known items 4 and 8 both confirmed in one line
each and neither touched.

**Task 2, the oracle, and it is the decision the night turned on.** Ended in 8 minutes of its
40-minute box, because the blocker was not a makefile fighting me — it was an absent toolchain, and
that is measured rather than waited out.

**Task 3, the sanctioned read**, through the gated emitter extended by name to reach `ft8/encode.c`
and `ft8/encode.h`, still off unless its environment variable is set. All four questions settled.

**Task 4**, `src/Ft8Sharp/Encode/Ft8SymbolEncoder.cs`, built through the existing payload and LDPC
types with no second CRC, packer or encoder, reading both tables from the generated file.

**Task 5 was not reachable** and is recorded as such rather than worked around. One line, per the
instruction, and on to task 6.

**Tasks 6, 7 and 8** all ran. Task 6 was not droppable and was not dropped.

**Decisions I made for myself, reproduced in full.**

1. **I did not route the build of upstream's generator through a test process.** The instruction
   says the building route is a shell command against a path outside the repository, and that if
   the harness refuses it I report the refusal and do not work around it. The harness declined
   invoking `cl.exe` outside the repository root, and a bash `ls` of the Visual Studio folder was
   blocked outright. Running the compiler from a checked-in test would have dodged that refusal,
   and I judged it a workaround rather than the sanctioned reading route, so I did not do it.
   **This decision did not change the outcome**, and I want that on the record: the toolchain is
   independently measured as incomplete — no `include` folder beside the compiler, no CRT import
   libraries, no Windows SDK — so the build would have failed by that route too. The finding does
   not rest on the refusal.

2. **I fixed two defects in my own new tests rather than in the tree.** A provenance regex matched
   the pre-loop initialization and so read upstream's bit walk as reset by a sync block; and a
   corroboration tally was written as 6 where three sync blocks plus four shapes is 7. Both were my
   errors in code written tonight, not mismatches with the tree, so repairing them is not the thing
   known item "report, do not repair" forbids.

3. **I raised task 7's corpus generation counts rather than lowering its floor.** The corpus first
   came in at 978 messages against my own "more than 1000" assertion, because generated callsigns
   are drawn from all shapes and many are refused by the standard message type. Loosening the
   assertion to match the number is forbidden and would have been wrong anyway; raising the attempt
   counts to 1431 packed messages is a corpus size change, which is permitted and is reported here.

## 2. What the owner should expect

**The library can now produce the tones a transmission is made of, and it has never been held
against anybody else's.** That is the whole shape of tonight. `Ft8SymbolEncoder` turns a packed
message into 79 channel symbols; every assertion about those symbols is this library agreeing with
itself or with a second implementation written an hour later by the same session.

**What will look wrong but is not:**

- **`ADVANCED: yes` next to an open criterion 2.** Three of step 3's four must-pass criteria moved
  and the unit's own target did not. Both are true; the header says so on the same line.
- **A version bump to 0.5.0 for a capability that is not proven against upstream.** The minor is
  for the capability, which is real. The props file's own note now says in terms what the minor
  does not claim.
- **`git status --short` at 36 lines and climbing.** That is the loop's own uncommitted machinery,
  known item 9, counted and not committed.
- **`TempEncoderProbe.cs` still tracked and still empty.** Known item 8, seventh session, not
  touched.
- **One skipped test in Ft8Sharp, still exactly one.** The table write gate. This unit added 23
  tests and no second skip.

## 3. What you should see

### CRITERION 2 — the block this report leads with

```
TASK 2 OUTCOME (of the three named)  : "the pin has one and it would not build, here is where"
DID THE COMPARISON RUN?              : NO
CRITERION 2                          : OPEN
corpus size                          : 14 messages
messages compared against upstream   : 0
matching symbol for symbol           : 0
first differing symbol position      : n/a — nothing was compared
hashed-callsign message in corpus?   : YES (one), but never compared against upstream
```

**Where it stopped, precisely.** Not in a makefile. There is no C toolchain on this machine to
drive the makefile with. `gcc`, `clang`, `cc`, `cl`, `cmake`, `make`, `ninja` and `mingw32-make`:
**none of them resolves on `PATH`.** One `cl.exe` exists, under a Visual Studio 18 Insiders install,
and it is a compiler payload without a compiler's surroundings — the toolset folder holds
`Auxiliary`, `bin` and `lib` and **no `include` folder at all**, its `lib` holds only `onecore`, and
`Windows Kits` holds only `NETFXSDK` with **no Windows 10 SDK and no `ucrt` include folder anywhere
under either Program Files**. So there are no C headers and no C runtime import libraries, and that
compiler cannot build a program that includes `stdio.h`, which the generator does. `vcvars64.bat`
exists and has nothing to point at.

**Installing a toolchain is the owner's under `ARBITER.md` §6 and this instruction refuses it in
terms. It was not done and nothing on this machine was changed.**

**Two harness refusals, reported as refusals and not worked around.** A bash `ls` of
`C:\Program Files\Microsoft Visual Studio` was blocked outright; invoking `cl.exe` by absolute path
required approval and was declined. **Neither refusal is what stopped criterion 2** — the toolchain
finding above was measured by a checked-in test reading with the operating system's permissions,
which is the route this phase already uses, and it holds regardless.

### Task 2 — the toolchain and the pin's build system, in the three-way form

**The pin HAS one.** `demo/gen_ft8.c`, 6477 bytes, 190 lines, with a `main`, calling an encode
entry point and mentioning tones. A second demo calls a decode entry point instead. **The build
system names it as a target**: the `Makefile` at the clone root, 1543 bytes and 59 lines, carries
nine rule heads and one of them is the generator's. There is no `CMakeLists.txt`. **The narrow
question is answered yes on both halves.** It is the machine that cannot build it, not the pin that
lacks it.

### The sequence geometry the encoder asserts

- **Length: 79 channel symbols**, every message, every time. 14 messages, all 79.
- **Data symbols: 58**, and 58 × 3 bits is exactly the codeword. The geometry closes on itself and
  is asserted to.
- **Every value inside the tone alphabet**: 1106 symbols checked across the corpus, none outside.
- **The three Costas blocks at the indices task 3 measured — 0, 36 and 72 — each checked
  separately**, in three separate tests rather than one loop, each against the checked-in Costas
  table position by position across the whole corpus. A single loop passes when two of three are
  right and reports one failure for the pair; three tests do not.
- **21 sync positions identical across the corpus and all 58 data positions varying**, which is
  what says the codeword is reaching the symbols rather than the encoder emitting a constant with
  sync blocks in it.
- **The third sync block ends exactly at the end of the transmission**, asserted directly.
- **Encoding is pure**: four sequential rounds in two orders plus a parallel round, all identical.

### The provenance, scalar by scalar, with its anchoring named

**MACRO-ANCHORED — the strong form. Seven, none uncorroborated.**

| Scalar | Role |
|---|---|
| `FT8_NN` | total channel symbols |
| `FT8_ND` | data symbols |
| `FT8_LENGTH_SYNC` | length of each sync group |
| `FT8_NUM_SYNC` | number of sync groups |
| `FT8_SYNC_OFFSET` | offset between sync groups |
| `FTX_LDPC_K_BYTES` | CRC'd payload buffer size |
| `FTX_LDPC_N_BYTES` | codeword buffer size |

**ARRAY-EXTENT-ANCHORED — a declaration; weaker than a macro, stronger than a transcription. Two.**
The tone alphabet size, from the declared extent of upstream's Gray map; the sync group length,
from the declared extent of its Costas pattern.

**EXPRESSION-ANCHORED, inside `ft8_encode`'s own body — the weak form. Seven.** Where each of the
three sync blocks sits and how its Costas index is rebased (three items); **which direction the
Gray map runs** — the three codeword bits are the index and the map's element is the tone; the bit
order within the group, first taken most significant; the codeword walked most significant bit
first; and **that the bit walk is continuous across the sync blocks**, a sync symbol consuming no
codeword bit. The function body is extracted by brace-matching from its definition at column zero,
so none of these can have matched against `ft4_encode`, which sits in the same file with the same
skeleton and different numbers in it.

**Two of those seven are the ones a plausible reading gets wrong and neither is catchable from this
side.** A port that ran the Gray map backwards, or that restarted the walk at each sync block, would
produce a sequence of the right length, with every value inside the alphabet, with the sync blocks
in exactly the right places. **Every assertion in section 3 above would still pass.** Only leg C
would catch it, and leg C did not run.

**One thing checked that could have been bad news and was not:** upstream's generator adds the CRC
into a 12-byte buffer and encodes that, which is `Ft8Payload.Create` followed by `LdpcEncoder.Encode`
with nothing between them. **Step 2's payload assembly agrees with upstream's. No step 2 defect
surfaced.**

### The independent second implementation — leg B

**Not dropped. The branch that licensed keeping it is the second one: task 5 was unreachable, so
leg B is the only evidence this unit can produce about the sequence.**

`SymbolCheck`, in the test project, calling nothing under `src/Ft8Sharp/Encode/`. Its arithmetic is
deliberately the opposite shape: the encoder walks the codeword with a mask and a byte index,
deciding at each of the 79 positions what kind of symbol it is on, which is upstream's shape; the
checker flattens the codeword to 174 separate bits, folds them into 58 data tones with no notion of
position at all, and splices the sync blocks in afterwards by index — rebasing them with a modulo
where the encoder subtracts a start.

```
corpus                        : 14 messages, 6 kinds
agreeing symbol for symbol    : 14 of 14
symbols compared              : 1106
sync positions, both methods  : 21, identical sets
watched catching a difference : flipping the first codeword bit moves exactly 1 symbol,
                                and it is symbol 7 — the first data symbol after the
                                opening sync block — with no sync symbol moved at all
```

**This is consistency, not correctness.** Two implementations written by the same session an hour
apart share whatever the session misunderstood.

### Criterion 1 — parity from message text, all 83 checks

```
seed                                             : 20901
real messages through pack, CRC, payload, encode : 1431
    standard, CQ                                 : 435
    standard, exchange                           : 354
    free text                                    : 200
    telemetry                                    : 200
    non-standard, call in full                   : 124
    non-standard, hashed companion               : 118
parity checks run (both table readings)          : 237 546
messages failing any of the 83 checks            : 0
```

**Which reading of "matches the reference" this stands on, named so it is not assumed: the WEAKER
one.** The syndrome check against the checked-in parity tables, computed by the independent
`LdpcCheck`, which shares no code with `LdpcEncoder`. **Not** a byte-for-byte comparison of the
codeword against upstream's — that needs the reference built and it could not be.

**The check is watched refusing.** All 174 single-bit flips of a codeword are caught, and every one
disturbs **exactly 3** checks, which is the column weight the code declares. Sharp, not merely
non-zero.

**Step 1's proof is untouched.** `Ft8LdpcParityTests`, `BasisProof` and `Payloads` are exactly as
they were; this is added beside them.

### What refuses, and that no partial sequence is ever returned

| What | Refused with |
|---|---|
| A message of the wrong length | `ArgumentException`, param `message` |
| A message with bits set past its 77th | `ArgumentException` from `Ft8Payload.Create`, **uncaught** |
| A symbol buffer of the wrong length | `ArgumentException`, param `symbols` |
| A payload with its spare bits set | `LdpcEncoder`'s own refusal, **uncaught** |
| A sync block index outside the three | `ArgumentOutOfRangeException` |
| A message that will not pack | Never reaches the encoder; the packer refuses and writes nothing |
| A table value outside the tone alphabet | `InvalidOperationException`, new tonight |

**No partial sequence, no zero-filled tail, no tone outside the alphabet.** The whole assembly
happens in stack buffers and the caller's span is written once at the end, so a call that threw
leaves the caller's buffer byte-for-byte as it arrived — asserted directly, against a buffer
pre-filled with a marker.

### Divergences from upstream, numbered on from the thirteen

**14. The sync block positions are derived from the offset macro rather than written as literals.**
Upstream writes each block's range as literal numbers in its guards, so the macro stating the offset
is declared and then not used by the code that places them. Deriving them means the two cannot
drift apart, and the provenance test checks the derivation against upstream's literals so both
readings still have to agree.

**15. A table value outside the tone alphabet refuses instead of reaching the sequence.** Upstream
indexes its tables and uses what comes back. HM-DEC-009: a regenerated table gone wrong could
otherwise put a value on the air that is not a tone, and the failure would surface as a waveform
rather than as an error.

### The Ft8Sharp totals, before and after

```
before (task 1) : 148 total, 147 passed, 0 failed, 1 skipped, 5.9 s
after  (task 8) : 171 total, 170 passed, 0 failed, 1 skipped, 4.0 s
tests added     : 23
skips           : still exactly one, the table write gate; no second skip added
```

The project still returns in about four seconds. **No corpus was cut for the clock.**

### Attribution and the three channel verdicts

```
git diff --name-only 2828ab6..HEAD  : 80 paths
under src/Hamlet.App/, src/Hamlet.RadioEngine/,
tests/Hamlet.App.Tests/, tests/Hamlet.RadioEngine.Tests/ : 0

AudioSeamTests + PrivilegeTests                      : 55 green
DecisionLogOrderTests                                : green
VersionTests            (RE-RUN AFTER THE BUMP)      : green
DecisionEmissionTests                                : green
VoiceTests                                           : green
                                                       13 green in that project
```

No new shared artifact was added, so the channel list is unchanged.

### What the inventories found in the clone, as names and shapes only

`Makefile` at the root, 1543 bytes, 59 lines, nine rule heads — `all`, `clean`, `run_tests`,
`install`, the generator, the decoder, the test program, `lib` and the static library. No
`CMakeLists.txt`. No C sources at the clone root. A `demo/` folder with two programs, 6477 and
13839 bytes, 190 and 394 lines, both with a `main`; one calls an encode entry point and mentions
tones, the other calls a decode entry point. `ft8/encode.c` and `ft8/encode.h` present and legible.
`ft8/constants.h` yields 18 integer macros. `ft4_ft8_public/` was not read, enumerated or
referenced.

### The two version numbers as they now stand

```
src/Ft8Sharp/Directory.Build.props : 0.4.0 -> 0.5.0   (HM-DEC-152)
Directory.Build.props (root)       : 1.12.15 -> 1.12.16 (HM-DEC-150)
```

## 4. What's blocking us

**Two items. The first is what stands in the way of criterion 2 in section B; the second does not
block anything.**

---

**1. There is no C toolchain on this machine, and step 3's second must-pass criterion cannot be
taken without one.**

**Ruling requested:** whether to install a C build toolchain on this machine so that `ft8_lib`'s
generator can be built and the symbol sequence compared against it.

**Reasoning.** Criterion 2 — *the symbol sequence is bit-identical to `ft8_lib`'s for the same
message* — is the only criterion in this phase that cannot be satisfied by a port that is
internally coherent and wrong in the same way at both ends. Units 207 and 208 both named it as
where their debt gets settled. It needs upstream's own generator running on this machine, and the
machine cannot compile C: nothing on `PATH`, and the one `cl.exe` present has no headers, no CRT
import libraries, and no Windows SDK. The pin is not the problem — it ships the generator and names
it as a Makefile target. `ARBITER.md` §6 puts installing software with the owner, so this is
genuinely yours and not something a unit may do.

**What was rejected and why.** *Declaring criterion 2 met on legs A and B* — rejected outright;
it is exactly what the instruction forbids and what the criterion was written to prevent. *Patching
upstream to build* — rejected; the instruction forbids it and a modified pin makes every provenance
test in the tree a lie. *Reaching for WSJT-X as a second oracle* — rejected; the plan leaves it
unruled and reserves it for you, and it is not needed before step 5.

**What it costs to leave open.** Steps 4, 5 and 6 all generate their fixtures from the encoder built
tonight. If the Gray map runs the wrong way, every one of those fixtures is self-consistently wrong,
every test over them passes, and the first thing that tells anybody is a blank screen at the radio
in step 7. The two ways this port could plausibly be wrong — the map's direction and the continuity
of the bit walk — are both invisible from inside.

---

**2. `PHASE_STATUS.md` still disagrees with `PHASE_OUTCOME.md`, and `git status --short` is now at
36 lines.** Known items 4 and 9. **This is not a ruling request and is not asking you to decide
anything** — it is confirmed and untouched exactly as instructed, and both belong to the loop rather
than to a unit. Recorded only so the next reader does not rediscover it.

---

**Nothing else is blocking.** The encoder, its assertions, the provenance, leg B, criterion 1 and
both version bumps are all done and green, and no question about any of them is outstanding.
