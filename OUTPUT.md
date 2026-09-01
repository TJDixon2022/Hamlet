READ IN THIS ORDER — A, then B, then C.

A. PHASE — Hamlet hears FT8 off the radio and displays the decoded text on screen. Seven steps.
Step 1 (the library exists and its tables are proven) and step 2 (messages round-trip through 77
bits) are closed, and task 1 shows their ground still standing: Ft8Sharp opened at 186 tests, 180
passed, 0 failed, 6 skipped in 3 s; the library built at 0 warnings and 0 errors on net8.0 with
nullable on, warnings as errors, no PackageReference and no ProjectReference; attribution from
2828ab6 was 106 paths with not one under any of the four Hamlet project folders; and all three
channels were green at 55 and 13 with every named class green including DecisionLogOrderTests. This
is the THIRD unit of step 3. Step 3 entered this unit at 3 of its 4 must-pass criteria with
criterion 2 open for the third unit running, and it LEAVES this unit at 4 of 4. Steps 4 through 7
(signals found in noise, a found signal becomes a message, sensitivity meets the published
threshold, Hamlet displays decoded FT8) remain unreached, each depending on the one before it by the
plan's own named deviation — but step 3 closing is what unblocks step 4, and that is the first time
this phase has been able to say so.
B. STEP 3 — a valid FT8 signal can be produced. FIVE exit criteria, FOUR must-pass.
(1) LDPC parity matches the reference for known payloads, must-pass — MET. 1431 real messages at
seed 20901 across all six kinds, 237 546 parity checks over both table readings, zero messages
failing any of the 83; and the checker still watched refusing, with all 174 single-bit flips caught
and every one disturbing exactly three checks. WHICH READING: it is now SPLIT and both halves are
named. The 77-bit PAYLOAD is upgraded from the weaker syndrome reading units 209 and 210 both stood
on to a byte-for-byte comparison against upstream's own bits — 51 of 51 packed messages identical.
The 174-bit CODEWORD is NOT upgraded, because upstream's generator prints no codeword under any of
seven labels tried; it stays on the syndrome check against the checked-in parity tables, and that is
now SETTLED rather than pending on a build.
(2) The symbol sequence is bit-identical to ft8_lib's, must-pass — THIS UNIT'S TARGET, and it MET.
The comparison against upstream's own tones RAN. Stack reserve read out of the original image:
1 048 576 bytes, PE32+, at file offset 216 — the linker's 1 MB default, and the owner had not
rebuilt it. A patched copy WAS made, asking for 16 777 216 bytes. The copy differs from the original
in 2 BYTES, AT OFFSETS 218 AND 219, both inside the 8-byte field written and none outside it (two
rather than eight because 1 MB and 16 MB share six of their eight little-endian bytes). The .text
hashes MATCHED, at 143 872 bytes each. The copy's no-argument output was byte-identical to the
original's, both exiting -1. Corpus size 56, of which 51 have a text form upstream can be asked for;
51 messages compared; 51 matching symbol for symbol, 4029 symbols, NO differing symbol position
anywhere. A message carrying a HASHED CALLSIGN was compared — four of them, with the call genuinely
on the wire as a 22-bit hash on both sides. The Gray map direction and the bit-walk continuity are
BOTH now settled against upstream: the map runs the direction the port assumed and the walk IS
continuous across the sync blocks.
(3) Audio synthesis produces a signal the reference decoder decodes, nice-to-pass — parked to the
next unit by the instruction, and not built. The tone sequence it will consume is now settled.
(4) Ft8Sharp tests green, must-pass every unit — MET. 198 total, 197 passed, 0 failed, 1 skipped,
3 s. The single remaining skip is Ft8TableGenerationTests.RewriteTheCheckedInTablesFile, the table
write gate, which is correct and is meant to skip. FIVE of the six skips this unit started with are
now RUNNING, all five having shared the one cause — upstream's generator exiting 0xC00000FD.
(5) Attribution clean from 2828ab6 and the channel tests green, must-pass every unit — MET. 110
paths, no Hamlet path appeared. AudioSeamTests and PrivilegeTests green at 55; DecisionLogOrderTests,
VersionTests, DecisionEmissionTests and VoiceTests green at 13, with VersionTests re-run after the
version bump.
C. THIS REPORT — The symbol sequence now stands on all THREE legs: provenance against the pin
(unchanged from unit 209), the independent second implementation (unchanged, green, now the weaker
of two agreeing legs), and bit-identity against upstream's own output — and it is that third leg
that did not exist yesterday. The patched copy WAS proven to be the same program by all four of task
3's checks; none of them failed. Unit 208's carried-forward debt is SETTLED for the form upstream
can be asked — a callsign really was on the wire as a hash in four compared messages, on both sides
— and honestly short of complete: the non-standard type's 12-bit companion hash cannot be got out of
upstream's generator at all and is NOT counted as covered. Task 6 WAS dropped, under the FIRST
branch of its condition, because task 3 produced a surviving generator and task 4's comparison
actually compared. The Ft8Sharp project still returns in about 3 seconds; no corpus was cut for the
clock — it was widened from 14 to 56. There are 8 .obj at the repository root, and neither they, nor
tools\build-ft8-oracle.bat, nor any patched binary was committed. Section 4 raises 2 items, and
NEITHER stands in the way of a criterion named in B.

UNIT:       211 — complete at task 8 of 8 — 2026-09-01 18:17
PHASE GOAL: Hamlet hears FT8 off the air and puts the decoded text on the screen.
UNIT GOAL:  Give a working copy of upstream's already-correct generator the stack the platform it
            was written for would have given it, prove the copy is the same program, and then hold
            this library's 79 channel symbols against the tones it produces.
ADVANCED:   yes — criterion 2, open for three units, is met: 51 of 51 messages bit-identical to
            upstream's own tones, on a copy proven identical to the original in every byte that does
            any work.
NUMBER:     step 3 must-pass criteria demonstrated: 3 -> 4 of 4
DRIFT:      0 consecutive units without advance  (was 1 — unit 210 held step 3 at 3 of 4)

## 1. What Claude did

**Exit state: complete, at task 8 of 8.** All eight tasks were reached. Task 6 was dropped by name
under the first branch of its own drop condition, which the instruction licenses explicitly; that is
the drop candidate the instruction named, and nothing else was left undone.

Machine: this one, `C:\Source\HamLet`, branch `main`. Project confirmed by the gate at the head of
the work instruction — `SHACK_FACTS.md` and `src/Hamlet.RadioEngine/Cw/CwProbabilisticDecoder.cs`
both tracked, `Hamlet.sln` the only solution, neither `CoreHMI.sln` nor `MURC.sln` present.

**Task 1 — the trace.** HEAD `210c8ee` on `main`, as the instruction says. Ft8Sharp 186/180/0/6 in
3 s. Library build 0 warnings, 0 errors; `net8.0`, nullable enabled, warnings as errors, no
`PackageReference`, no `ProjectReference`. Attribution 106 paths, none under a Hamlet project folder.
Channels green at 55 and 13. Eight `.obj` at the root. `tools\build-ft8-oracle.bat` present,
untracked, unchanged at `16:02:11` — expected, and not a finding. Known items 6 and 10 confirmed and
neither touched.

**Task 2 — the oracle re-measured rather than inherited.** Every claim carried forward was re-taken.

**Task 3 — the working copy.** Copied out to a folder of its own under the temp path, 16 MB written
into `SizeOfStackReserve`, four proofs asserted, and the copy deleted when the run ends. `Ft8Oracle`
now resolves once per run to the patched copy where one was **proven** and to the original
otherwise, with the choice visible in the usability probe's detail string. An unproven copy is never
offered, so nothing in the project can be compared against one by accident.

**Task 4 — criterion 2.** The comparison ran and passed on 51 of 51 messages.

**Task 5 — criterion 1 and the corpus.** Re-taken on the same numbers; the reading split and both
halves named; the corpus widened from 14 to 56.

**Task 6 — dropped**, first branch.

**Task 7 — leg B re-run green and the record written.**

**Task 8 — both versions bumped, channels re-run, final numbers taken.**

### Decisions made for myself, reproduced in full

1. **My own first assertion in task 3 was wrong, and I corrected it rather than the tree.** The
   instruction says the number of differing bytes "must be exactly the width of the field you
   wrote — 8 for PE32+". It is 2. That is not a weaker result than expected but a different and
   slightly stronger one: 1 MB and 16 MB share six of their eight little-endian bytes, so only two
   move. Demanding eight would have failed a perfectly good patch; demanding a *count* of eight
   would have accepted six stray bytes elsewhere. **The assertion is containment — every byte that
   moved lies inside the field written, no byte outside it moved, and the file is the same length.**
   This is reported as a mismatch against the instruction in section 3 and was not repaired in the
   instruction file.

2. **Two corpus entries were removed from the tone comparison, and this is the decision most worth
   checking.** In both cases the two sides were being asked *different questions*, not giving
   different answers, and I established that by comparing the **packed message bytes and the message
   type** rather than the tones — see section 3. Neither is an assertion loosened, and in both cases
   the coverage was replaced by something stronger. I want the reasoning visible because "the
   comparison stopped comparing that one" is exactly the shape a laundered failure takes:
   - the non-standard hashed-companion entry now has **no text form**, exactly as telemetry has
     none, because no string makes upstream produce that wire format. It is named as NOT COVERED in
     the test's own output and in the record, and is not counted as covered.
   - one free-text string I added tonight (`K1ABC RR73 X`) is read by upstream's own type selector
     as a standard message. I replaced it with an unambiguous one and recorded the finding, which is
     worth more than the entry was.

3. **The corpus was widened rather than cut.** The instruction permits cutting for the clock; the
   clock did not require it. It went from 14 to 56, and the whole project still returns in 3 s.

4. **I did not treat "the generator survives" as "the generator can be read".** The copy exited 0 and
   the comparison still skipped, because the tone parser did not recognise upstream's format. I added
   a test that reports the *form* of each output line — every character replaced by its character
   class, so the format is visible and no tone value is. That is how the run-together tone format was
   found, and the test stays in the tree.

### What was not done, and what was refused

- **No compiler was invoked, by any route.** No batch file was run. `tools\build-ft8-oracle.bat` was
  not edited, run, or committed.
- **Nothing inside `C:\Source\ft8_lib` was written to, patched or deleted**, including
  `build\gen_ft8.exe`. The original image was opened for reading only.
- **The sandbox again refused shell access outside the tree** — `ls /c/Source/ft8_lib/build/` was
  declined, as it was for the arbiters of units 209, 210 and 211. **Reported as a refusal and not
  routed around:** everything read from the clone was read by the test process, which is the
  sanctioned route and the one this project already uses for reference material.
- **The `dotnet test` shell pipeline was never piped through `grep` on a run whose result mattered**
  (known item 12). The one `grep` used was on a re-run whose full output I had already seen.
- **`tools\arbiter\validate-output.bat` could not be run** — the harness refuses a batch file, which
  is the same refusal section 4 item 2 records. **Reported as a refusal and not routed around.** The
  ordering block was checked by hand instead: `A.`, `B.` and `C.` start at the beginning of lines 3,
  16 and 49, all inside the first 60, and the count in C is written as a digit.
- **Known item 9 confirmed by observation.** Writing `output.md` landed on the tracked `OUTPUT.md`,
  as the instruction says it would on this filesystem. Not renamed.

## 2. What the owner should expect

**Step 3 is done on its four must-pass criteria.** The encoder is no longer only self-consistent;
it produces the same tones Goba's own program produces, for every message either side can be asked
about. Eleven units of message-layer work — the CRC, the packer, the alphabets, the field layouts,
the callsign hash, the LDPC codeword — settle at once against the world rather than against
themselves, because a wrong bit anywhere in that chain moves a tone and none moved.

**What will look wrong but is not:**

- **A test that skips on every machine but this one.** Criterion 2's evidence invokes upstream's
  binary at run time and skips when the clone or the build is absent. On a fresh clone the project
  is green with one skip more than here, and that is correct and deliberate — it is the same
  standing the plan already gives the reference-WAV criterion. What makes it worth something is that
  it *ran here*.
- **A temporary copy of somebody else's executable, with two bytes changed.** It lives under the
  system temp folder, it is deleted when the run ends, it is never committed, and its equality with
  the original is asserted on every run rather than believed. See section 4, item 1 — this is a
  decision you may overrule.
- **`Ft8Sharp` at 0.5.2 while the root is at 1.12.18.** Two version numbers, deliberately, under
  HM-DEC-152. Not drift.
- **Eight `.obj` files at the repository root.** Yours, from your build at 15:37–15:39. Not
  committed, not deleted, counted at 8.
- **`git status --short` printing 24 lines.** The loop's own uncommitted files, the `.obj`, and the
  oracle script. Not committed.

## 3. What you should see

### CRITERION 2 — the block, before any prose

```
original image stack reserve      : 1 048 576 bytes (1.00 MB), PE32+, field at file offset 216
had the owner already raised it?  : NO — still the linker's 1 MB default
patched copy made?                : YES — 16 777 216 bytes (16.00 MB) written
bytes differing, copy v. original : 2, AT OFFSETS 218 AND 219
                                    both inside the 8-byte field written; no byte outside it moved;
                                    both files 208 896 bytes
.text hashes equal?               : YES — 143 872 bytes each, at file offset 1024 in both
no-argument output identical?     : YES — byte-identical stdout and stderr, both images exit -1
copy survives a real message?     : YES — exit 0x00000000, WAV written, 360 044 bytes
all four proofs?                  : PROVEN
comparison ran?                   : YES
corpus size                       : 56 messages (51 with a text form upstream can be asked for)
messages compared                 : 51
matching symbol for symbol        : 51  (4029 symbols)
first differing symbol position   : NONE — no message differed at any position
hashed-callsign message compared? : YES — 4 of them, call on the wire as a 22-bit hash on both sides
Gray map direction settled?       : YES — the map runs the direction the port assumed
bit-walk continuity settled?      : YES — the walk IS continuous across the sync blocks
```

Both of the two ways unit 209 said this port could still be wrong — a Gray map run backwards, a
codeword bit-walk restarted at each sync block — are dead. Either would leave the sequence the right
length, every value inside the alphabet, and the sync blocks in the right places, and either would
move data symbols. Every data symbol of every compared message agrees with upstream's.

### The oracle as task 2 measured it

The pinned clone is reachable and `build\gen_ft8.exe` is present at 208 896 bytes. Its PE32+ optional
header asks Windows for **1 048 576 bytes**, at file offset 216 — **the owner had NOT rebuilt it**,
so the linker-flag route was still open and task 3 was needed. Given a real message the original
still exits **0xC00000FD (STATUS_STACK_OVERFLOW)** and writes no WAV. Given no arguments it prints
194 characters over 7 lines and exits -1, cleanly — a sound build meeting a platform limit, exactly
as unit 210 diagnosed. Every claim carried into this unit came back confirmed.

Two independent readings of the header field agree: the reader unit 210 wrote inside
`Ft8OracleDiagnosisTests`, and the one added beside it to find the offset to write to. Both land on
216. That is asserted, because a writer aiming somewhere the reader is not looking would make the
whole-file comparison check the wrong bytes.

### Task 3's four proofs, each with its number

1. **Whole-file:** both images 208 896 bytes; **2 bytes differ, at offsets 218 and 219**; the field
   written spans 216–223; every differing offset is inside it and none outside it moved. No checksum
   was recomputed and nothing was re-signed.
2. **`.text`:** equal SHA-256, 143 872 bytes each at file offset 1024 in both images. No instruction
   moved.
3. **Behaviour where it already worked:** both exit -1 with no arguments and print byte-identical
   stdout and stderr. A patched image printing its usage unchanged is direct evidence its code path
   is untouched.
4. **A real message:** exit 0x00000000, WAV written, 360 044 bytes. The thing the original could not
   do.

**MISMATCH AGAINST THE INSTRUCTION, reported and not repaired.** Task 3.3 says the number of
differing bytes "must be exactly the width of the field you wrote — 8 for PE32+". It is **2**, and
that is correct rather than suspicious: 1 MB and 16 MB share six of their eight little-endian bytes.
The proof that actually bears weight is **containment** — no byte outside the field moved — not a
count, and the test asserts containment. The work succeeded anyway; the instruction's expectation is
the thing that is wrong.

### The comparison watched refusing — and now also passing

Both together, which is what makes the pass mean something:

| what was fed to it | what it said |
|---|---|
| one symbol altered at position 7 | names position 7, the 1st data symbol, carrying codeword bits 0–2 |
| one symbol altered at position 38 | names position 38, inside sync block 1 (symbols 36–42) |
| two sequences of different lengths | refused outright, 0 compared, not compared over the prefix |
| an unaltered sequence, whole corpus | agrees, 79 of 79, every message |
| 51 real messages against upstream | agrees, 79 of 79, every message |

The tone parser is watched refusing prose, an empty string, the right count with one value outside
the alphabet, and the right values one short. **The run-together form added tonight is held to the
same standard** and is watched refusing a run one short, one long, one carrying an 8, and a
same-length run of nines. The hex reader gained its own refusal test for the same reason.

### The two corpus entries that were asking a different question

This is the subtlest thing the unit found and it would have read as a defect in the port.

Our API names a message type — the caller picks the packer. Upstream's generator is handed a string
and picks the type **itself**. Where the two pick differently, the tones differ for a reason that has
nothing to do with either encoder.

| entry | our type | upstream's type | verdict |
|---|---|---|---|
| non-standard, hashed companion | i3 = 4 | i3 = 1 | different wire formats — not comparable |
| free text `K1ABC RR73 X` | i3 = 0 | i3 = 1 | upstream reads it as a standard message |

Both were caught by comparing the **packed bytes and the message type**, not the tones. That
instrument is now a permanent test, and it prints both types on every difference, because it
separates *the two sides disagree* — a defect — from *the two sides were asked different things*,
which is not.

### Leg B, re-run

Green at 3 of 3, over 56 messages and 4424 symbols, **untouched and not weakened**. It is now **the
weaker of two agreeing legs** rather than the only implementation-level evidence there is. It is
kept for two reasons beyond that: it covers the 5 messages leg C cannot reach — the four telemetry
entries and the non-standard hashed-companion entry — and it is the only symbol-level evidence that
survives on a machine with no clone, which is every machine but this one. Its own closing line said
"criterion 2 is open"; that line is now true instead.

### Criterion 1, and which reading it stands on

1431 real messages at seed 20901 across all six kinds, **237 546 parity checks** over both table
readings, **zero** messages failing any of the 83. Watched refusing: all 174 single-bit flips caught,
each disturbing exactly three checks, which is the column weight the code declares.

**The reading is split and both halves are named.** The **payload** is upgraded to the stronger,
byte-for-byte reading — 51 of 51 packed messages identical to upstream's own bits. The **codeword**
is not, and cannot be: the question that had been skipping since unit 210 now answers, and the
answer is that upstream prints a 10-byte packed message and **no codeword under any of seven labels
tried** (a 174-bit codeword would be 22 bytes). So the codeword half stays on the syndrome check
against the checked-in tables — **settled, not pending on a build**. One stale sentence in that
test, inherited from unit 209 and saying the reference could not be built here, was corrected.

### The corpus as widened

From unit 209's **14** to **56**, across the same six kinds: 39 standard (grids at both corners and
the middle, reports at both ends and zero, the roger forms, the extra-field tokens, ten callsign
shapes, the lettered and numbered CQ forms and QRZ), 8 free text (both length extremes, digits,
punctuation, spaces), 4 telemetry, 2 non-standard with the call in full, 1 non-standard with a hashed
companion, 4 standard with a callsign hashed onto the wire. **Nothing was cut for the clock.**

### Divergences, and corrections to the port

**No divergence from upstream was added; the count stands at fifteen.** **No correction was made to
the port** — task 4's bounded permission was not needed and was not used. Nothing under
`src/Ft8Sharp/` changed except `Directory.Build.props` and `porting-notes.md`. Every line of
machinery this unit built is in the test project.

### The numbers

| | before | after |
|---|---|---|
| Ft8Sharp total | 186 | 198 |
| passed | 180 | 197 |
| failed | 0 | 0 |
| skipped | 6 | 1 |
| wall clock | 3 s | 3 s |

**12 tests added.** **Five of the six starting skips are now running** — all five shared the one
cause, upstream's generator exiting 0xC00000FD, and the patched copy removed it. **The one remaining
skip is `Ft8TableGenerationTests.RewriteTheCheckedInTablesFile`**, the table write gate, which is
correct and is meant to skip. No test was repaired, quarantined, deleted, renamed or disabled to make
a count come out.

### Attribution and the three channels

**110 paths** from `2828ab6`, and **not one** under `src/Hamlet.App/`, `src/Hamlet.RadioEngine/`,
`tests/Hamlet.App.Tests/` or `tests/Hamlet.RadioEngine.Tests/`. Channels, re-run **after** the
version bump: `AudioSeamTests` and `PrivilegeTests` green at **55**; `DecisionLogOrderTests`,
`VersionTests`, `DecisionEmissionTests` and `VoiceTests` green at **13**. Every named class green.
No new shared artifact, so the channel list is unchanged.

### The root, and what was not committed

**8 `.obj` at the repository root**, unchanged and untouched. **Not committed:** the eight `.obj`,
`tools\build-ft8-oracle.bat`, the patched binary or any trace of it, any WAV, any tone sequence, any
payload, any codeword, and the loop's own files. `git status --short` prints **24**. Every commit
used explicit paths; `git add -A` was not used anywhere. Versions now stand at **`Ft8Sharp` 0.5.2**
and **root 1.12.18**.

### Mismatches against the instruction, reported and not repaired

1. `git status --short` printed **24** at task 1 and **24** at the end, where the instruction says
   **23** — one more, both times.
2. `git diff --name-only 2828ab6..HEAD` gave **106** at task 1, matching, and **110** at the end
   after this unit's own commits.
3. Task 3.3's expectation that the copy differs by exactly the field width — it differs by 2 of 8,
   described above.

## 4. What's blocking us

**Nothing is blocking.** Both items below are notes, and **neither is asking Tim to decide anything
before work continues.** Neither stands in the way of a criterion named in section B.

### 1. The arbiter licensed patching a working copy of upstream's binary, and you may overrule it

**This is a note, not a ruling request, and nothing is stopped waiting for you.**

Unit 210 considered editing the built executable's PE header to widen its stack and rejected it, on
the ground that "an oracle I have modified is a weaker oracle." The arbiter overruled that for this
unit — not with an argument but with a requirement: the copy had to be *proven* the same program
before it could answer anything.

It was. Every run asserts it, and here are tonight's numbers:

- both images **208 896 bytes**; **2 bytes differ, at offsets 218 and 219**, both inside the 8-byte
  `SizeOfStackReserve` field, and **no byte outside that field moved**;
- `.text` hashes **equal**, 143 872 bytes each;
- no-argument output **byte-identical**, both exiting -1;
- and the copy exits **0** on a real message where the original exits **0xC00000FD**.

The field written is read by Windows at process creation to size an address-space reservation. It is
not code, not data the program reads, and not an input to any tone the generator computes. **The
original was never opened for writing; the copy lives under the temp folder and is deleted when the
run ends; nothing patched was committed.**

**If you would rather the oracle came only from your own script, the stack-size flag on the link line
in `tools\build-ft8-oracle.bat` is still the route, and you can overrule this.** That question
remains open with you — the script is unchanged at `16:02:11`, before unit 210 even ran, and this
unit neither edited it, ran it, nor chased the answer. Tonight's route makes your answer **optional
rather than blocking**, and the machinery is built to stand down on its own: the test checks the
image's reserve first and **makes no copy at all** if it already asks for 8 MB or more. The day the
flag lands, this quietly stops running.

### 2. The permission scope has no rule under which a unit can run a batch file or a compiler

**Recorded, owner-class under `ARBITER.md` §6, and no longer holding anything up.**

`tools\arbiter\run-unit-tools.txt` grants `Read`, `Write`, `Edit`, seven `git` verbs, `dotnet test`,
`dotnet build`, `dotnet restore` and the report validator. There is no rule under which a unit can
execute a batch file or a compiler, which is why the build route to the oracle was not attempted
tonight. Unit 210 raised this; it is a note, it is yours, and **tonight's route means the phase is no
longer waiting on it.** I did not edit that file and am not arguing for it to be widened.

**Separately, and reported as a refusal rather than routed around:** the harness again declined shell
access outside the repository root, so nothing about `C:\Source\ft8_lib` in this report was read by a
shell. Everything was measured by the test process, which is the sanctioned route.
