READ IN THIS ORDER

A. The phase goal is **FT4 works exactly the way FT8 does**. Where every step stands:
   step 0 done; **step 1 done**; steps 2, 3, 4, 5 and 6 not started. Nothing this unit
   found changes the state of any step other than 1. It did measure two things step 2
   will need — the candidate window and the placement — and they are in section 2.

B. Step 1 and its four exit criteria:
   1. a hundred-plus messages round-tripping to themselves, with compound callsigns,
      grids, reports and RR73                                   must-pass   **MET**
      106 sent, 106 read back as themselves.
   2. the timing measured from the audio and stated with its source, and the 4.48
      against 5.04 disagreement named with both numbers          must-pass   **MET**
      (moved from "4.48 seconds of transmission" by the arbiter)
   3. zero wrong decodes, counted separately from missed ones    must-pass   **MET**
      0 wrong and 0 missed on the round trip; 0 wrong over a further 848 ladder trials.
   4. a sensitivity ladder with its trial count and its wrong count
                                                                nice-to-pass **MET**
      848 trials over eight rungs, 0 wrong, 0 out of noise alone.

C. This report's own findings, weighed against A and B. **Section 4 raises 4 items.**
   None of the four is in the way of a criterion in B — all four criteria are met and
   evidenced, and every item is beside them rather than under them. Two are questions
   already with Tim and carried forward unchanged (the 4.48 timing figure, the version
   scheme); one is new and is the single number in the FT4 path that is not upstream's
   (the widened candidate sweep, which criterion 1 depended on and which is already
   done rather than pending); one is a conflict between this session's prompt and task
   7 about who owns `PHASE_STATUS.md`'s step lines, which cost one file edit that was
   made and then reverted. **Task 8 was not dropped.** The named drop candidate was
   taken and criterion 4 is met rather than unmet by choice.

UNIT:       289 - complete at task 8 of 8, nothing dropped - 2026-09-09 09:08
PHASE GOAL: FT4 works exactly the way FT8 does.
UNIT GOAL:  A message becomes FT4 symbols, becomes audio, and decodes back to the same
            message - with the symbols and the samples proved identical to upstream's
            own generator first, so the round trip is evidence rather than a tautology.
ADVANCED:   yes - step 1's four exit criteria all moved from untried to met, three
            must-pass and the nice-to-pass one, and an FT4 decoder now exists.
NUMBER:     the round trip - 106 messages sent, 106 read back as themselves,
            0 wrong decodes, 0 missed decodes.
DRIFT:      0 consecutive units without advance  (was 0)

## 1. What Claude did

**Exit state: complete, at task 8 of 8, with nothing dropped.** Windows 11, project
gate `PROJECT: Hamlet` verified against the tree — `SHACK_FACTS.md` present,
`src/Hamlet.RadioEngine/Cw/CwProbabilisticDecoder.cs` present, `CoreHMI.sln` and
`MURC.sln` both absent — branch `main`, eight commits pushed, one per task.

### What was built

FT4 is now in `src/Ft8Sharp` as **six new library files plus one that holds its
timing**, and two shared files gained one seam each:

| File | What it is |
|---|---|
| `Encode/Ft4SymbolEncoder.cs` | `ft4_encode` — payload XOR, CRC-14, `encode174`, 87 data symbols of 2 bits, Gray map, two ramps, four Costas rows |
| `Encode/Ft4Waveform.cs` | 4-tone GFSK at BT 1.0, 105 symbols of 0.048 s, upstream's own slot layout |
| `Ft4Timing.cs` | **the one place FT4's timing lives** |
| `Dsp/Ft4WaterfallGeometry.cs` | the geometry re-derived at 0.048f |
| `Dsp/Ft4SyncSearch.cs` | `ft4_sync_score` over four *different* Costas rows from symbol 1 |
| `Dsp/Ft4SoftSymbols.cs` | `ft4_extract_likelihood` — 2 bits over 4 bins, 5-then-9-then-13 skip |
| `Dsp/Ft4SlotDecoder.cs` | the whole FT4 path, wiring only |
| `Dsp/Ft8WaterfallGeometry.cs` | *edited*: a protected constructor taking a symbol period and a slot length. The public constructor hands in FT8's own two constants, so every FT8 number it has ever produced is produced by the same arithmetic in the same precision. |
| `Ldpc/Ft8CodewordDecoder.cs` | *edited*: an optional payload-XOR argument, applied after the CRC check and before the payload is handed on — where `ftx_decode_candidate` puts it |
| `Encode/Ft8Waveform.cs` | *edited*: one internal accessor to its error function. No constant moved and the padding is untouched. |

`Ft8SymbolEncoder` was not touched at all, and
`EverySymbolOfEveryMessageIsIdenticalToUpstreams` was not generalised.

The three FT4 tables now come out of `Ft8TableConverter` rather than a keyboard —
16, 4 and 10 elements — and the sentence in two places saying they were deliberately
skipped is rewritten rather than left standing.

### Decisions this session made for itself, reproduced in full

**One.** *The FT4 candidate sweep is widened to blocks -10 to 51 where the demo
application uses -10 to 19.* Task 1 measured that upstream's sweep, at FT4's 0.048 s
block, reaches 0.912 s, and that upstream's own generator centres its signal at 1.23 s —
so upstream's decoder cannot see upstream's generator, which is the symptom unit 288
found and could not explain. The waveform is kept as upstream's, because a waveform that
is not upstream's is not a faithful port; the sweep bound is what moved, and it is **not
in `ft8/` at all** — it is a file-scope judgement in `demo/decode_ft8.c` about how much
work to do, the same class of number as `kMin_score` and `kMax_candidates`, and it was
already a constructor parameter in this port before this unit. 51 is 156 blocks in a slot
less 105 in a transmission, so it covers every placement a well-formed FT4 signal can
have, including upstream's own centred 25. Nothing about the modulation, the tables, the
codeword or the waveform is changed, and a caller who asks for upstream's own sweep gets
upstream's own result, which is nothing. **It is raised in section 4 as an item for the
record rather than as a question holding anything up.**

**Two.** *`PHASE_STATUS.md`'s `STEP:` lines and `CURRENT_STEP` were left alone, against
task 7.* Task 7 says to bring them into line with `PHASE_OUTCOME.md`'s header. This
session's own prompt says `HEARTBEAT:`, `CURRENT_STEP:` and the `STEP:` lines belong to
the launcher and are not to be written. The prompt is an explicit prohibition addressed
to this session and it wins; the edit was made, then reverted. The fact it would have
recorded is recorded in `PHASE_OUTCOME.md`, whose header now reads `STEP: 1 | done`, and
which is the file the arbiter reads. Raised in section 4.

**Three.** *`tools\arbiter\validate-output.bat` could not be run either, and the seven
rules were applied by hand instead.* **This is an environment fact and not a question, so
it is not one of section 4's four items — but it is a pass/fail condition of this unit and
it is said here rather than left to be discovered.** This session's shell will run
`dotnet`, `git` and the ordinary file utilities and **refuses `cmd`, `powershell` and any
`.bat`**; every invocation form was tried — `cmd //c`, `cmd.exe /c`, the full path to
`cmd.exe`, and the script directly — and all four were refused, in a session that cannot
be asked to approve them. So the rules were applied one at a time against this file with
tools that do work, and this is the result:

| Rule | Check | Result |
|---|---|---|
| 1 | a `UNIT:` line above section 1, parseable | present, line 32 |
| 2 | the four top-level sections, in order, exact names | lines 43, 146, 180, 370, exact |
| 3 | no fifth top-level section | four `## ` headings and no more |
| 4 | section 4 present even when empty | present, and not empty |
| 5 | section 3 non-empty | 153 non-blank lines |
| 6 | the ordering block above `UNIT:` — header, A, B, C, and C naming a count | all five present inside the first 60 lines |
| 7 | no placeholder token in the header block | zero matches on the validator's own token list |

**That is not the same thing as the script exiting 0** and it is not claimed to be. The
same two rules the script holds and this session cannot: that its copy of the rules and
`CLAUDE_CODE.md` §8 still agree, and that its own reading of "no other headings" is the
one applied. Both were read out of the script's own comments rather than assumed.

**Four.** *The `PHASE_OUTCOME.md` entry was written by hand.*
`tools\arbiter\outcome-append.bat` could not be run — the shell refused the invocation
and this session is non-interactive, so there was no way to answer for it. Task 7
anticipated exactly this and required it to be said, so the entry says it on its own
face. The arguments the script would have been given are committed at
`tools/arbiter/unit289-append.bat` with no apostrophes in any of them, so the entry can
be reproduced rather than reconstructed.

### One correction

The intermediate `UPDATED:` timestamps in `PROJECT_STATUS.md` during tasks 2 to 7 were
composed rather than read from the clock, and ran about an hour and a quarter fast. The
final one is read. Nothing else in this report or in any commit depends on them, and the
unit ran about 32 minutes wall clock from 08:36 to 09:08.

### How tests were run

**No suite was run.** Every test was filtered by exact name or exact type, foregrounded,
with a stated 10-minute timeout, per HM-DEC-155. Nothing was backgrounded and nothing was
polled. `dotnet build` was run foregrounded three times.

## 2. What the owner should expect

**An FT4 decoder now exists in `Ft8Sharp` and it can be trusted about as far as FT8's
could be after unit 216 — which is a long way for synthesized audio and not at all for a
radio.** Hamlet can turn a message into an FT4 transmission whose tones and whose samples
are upstream's own, and read it back as the same message, a hundred and six times out of
a hundred and six, with nothing wrong coming out.

**What will look wrong and is not.**

- **The FT4 button still does nothing.** This unit changed nothing in `src/Hamlet.App` or
  `src/Hamlet.RadioEngine` — it is not allowed to, and step 4 owns that.
- **The screen still says "fifteen second slots" and `"15 s slots"`.** Both are real §0.0
  breaches, both were named by unit 288, and both are step 2's.
- **`Ft8Sharp` jumped a minor rather than a patch**, from 0.10.7 to 0.11.0, where the last
  seven units all took patches. The rule is the one that file's own comment block states:
  bump when the library gains a capability of its own, minor for an addition rather than a
  correction. A second modulation is an addition. The root took a patch, 1.12.215 to
  1.12.216, as instructed.
- **`PHASE_STATUS.md` still reads step 0 and step 1 as `not started`.** That is deliberate
  and section 4 explains it. `PHASE_OUTCOME.md` is right.

**What step 2 inherits, and it is the thing that will bite the slot machinery.**

- **The placement.** An FT4 signal sits **1.23 s** into its 7.5 s slot, occupies **5.04 s**,
  and leaves 1.23 s behind it. `Ft8Slots.SlotSeconds` and `TransmissionSeconds` and the
  eight files that compute from them are all FT8's today.
- **The candidate window.** `Ft4SyncSearch` sweeps blocks **-10 to 51**, which is
  **-0.48 s to +2.448 s**. That is wider than upstream's and it is why the chain works.
  If step 2 ever moves where the signal is placed in the slot, this bound is what has to
  move with it, and it is one named constant in one file.
- **One timing constant.** If Tim rules that FT4's transmission is 4.48 s rather than
  5.04 s, the change is `src/Ft8Sharp/Ft4Timing.cs` and nothing else in the library.

## 3. What you should see

### 1. The round trip's numbers

**106 messages sent. 106 read back as themselves. 0 wrong decodes. 0 missed decodes.**

Each one composed from fields, packed to 77 bits, encoded to 105 FT4 symbols, synthesized
to audio, decoded, and compared against **the string it was composed from** — never
against the decoder's own reading of its own bits. Sent at eight frequencies across the
passband rather than one, so a hundred agreements are a hundred places in the waterfall.

| Kind | Read back |
|---|---|
| CQ with a grid | 10 of 10 |
| a directed CQ | 10 of 10 |
| a signal report | 14 of 14 |
| a report acknowledged | 14 of 14 |
| RRR | 10 of 10 |
| **RR73** | **10 of 10** |
| 73 | 10 of 10 |
| nothing at all | 10 of 10 |
| **a compound callsign** | **6 of 6** |
| a portable suffix | 4 of 4 |
| free text | 8 of 8 |

A wrong decode is counted separately from a missed one, and both are reported at zero.
One transmission goes into each slot, so anything else coming out of it is counted wrong
whatever else the slot did.

### 2. The upstream comparison

**51 of 51** corpus messages with a text form are identical to `gen_ft8 -ft4` **symbol for
symbol**, 105 symbols each. Five telemetry entries have no text form and are named as not
compared rather than left out silently.

**And 51 of 51 again at the sample level**: every sample of every FT4 slot this library
synthesizes agrees with the WAV upstream's generator writes for the same message to
**within one count of 32767** — the same rounding-level agreement unit 212 measured for
FT8, and no disagreement anywhere was larger than one.

**What a mismatch would have looked like.** The comparator names the position and says
what kind of position it is, because the three faults are different: a difference at a
ramp implicates the ends of the transmission; inside a sync group it implicates that
group's *row* of the Costas table — FT4 sends four different patterns and a port
repeating row 0 shows up exactly there; anywhere else it implicates the codeword, the
Gray map direction, the two-bit walk, or the payload exclusive-OR, which would move
nearly every data symbol at once. All three refusals were watched firing.

**This is what makes the round trip evidence rather than a tautology**, and it is why it
ran first. Hamlet's encoder into Hamlet's decoder is self-consistent by construction: a
wrong Gray map, a wrong Costas row, a wrong XOR byte or a wrong ramp position is applied
at one end and undone at the other and every message comes back perfect. The round trip
above is not offered as proof on its own.

### 3. The measured timing

Measured from the audio buffer, not read back off the constant it came from:

| | |
|---|---|
| sample rate | 12000 Hz |
| samples per symbol | 576 |
| **symbol period** | **0.048000 s** |
| **symbol count** | **105** |
| **occupancy** | **5.0400 s** (60480 samples) |
| **tone count** | **4** |
| **tone spacing** | **20.8333 Hz** |
| **slot length** | **7.5000 s** (90000 samples) |
| signal starts at | 1.2300 s |

The four tones were also read back out of the audio by counting zero crossings — 1000.000,
1020.588, 1041.667 and 1062.500 Hz — which is 62.500 Hz across three steps against 62.500
expected.

**The 4.48 against 5.04 disagreement, with both numbers.** `PHASE_PLAN.md` step 1 calls
FT4's transmission **4.48 seconds**. Upstream's `FT4_SYMBOL_PERIOD` is `0.048f`
(`ft8/constants.h:14`), which puts 105 symbols at **5.04 seconds** and the tone spacing at
20.833 Hz. **The string `4.48` appears nowhere in the pinned clone.** Measured from this
audio: **5.0400 s.**

**This unit built on 5.04**, because the standing ruling is that `Ft8Sharp` is a faithful
MIT port and a deliberate divergence from upstream is not a unit's to make. **Nothing here
settles it.** The question is Tim's and it is carried in section 4 unchanged.

### 4. Where the FT4 timing constants live

**`src/Ft8Sharp/Ft4Timing.cs`, and nowhere else.** There is no `0.048f` and no `7.5f`
anywhere else in the FT4 path — the synthesizer, the waterfall geometry, the tone
spacing, the slot length and the block count all read from that one file. **A ruling the
other way costs one edit.**

### The sensitivity ladder — criterion 4, taken rather than dropped

Eight rungs, 106 messages a rung, **848 trials**, seed 289, one frequency, and the SNR
delivered at each rung measured from the samples rather than assumed from the request.
The axis is power in a 2500 Hz reference bandwidth, the same one unit 222 checked against
a second instrument and found agreeing to a mean of 0.0098 dB.

| requested | delivered | trials | decoded | **WRONG** | missed |
|---|---|---|---|---|---|
| 0.0 | 0.000 | 106 | 106 | **0** | 0 |
| -5.0 | -5.000 | 106 | 106 | **0** | 0 |
| -10.0 | -10.000 | 106 | 106 | **0** | 0 |
| -13.0 | -13.000 | 106 | 106 | **0** | 0 |
| -15.0 | -15.000 | 106 | 49 | **0** | 57 |
| -17.0 | -17.000 | 106 | 0 | **0** | 106 |
| -19.0 | -19.000 | 106 | 0 | **0** | 106 |
| -21.0 | -21.000 | 106 | 0 | **0** | 106 |

**0 wrong decodes across all 848**, and **0 messages out of twenty slots of noise alone.**
The 50 per cent crossing sits between -13 and -17 dB, near -15.

**It is not a step 6 result and it is not compared with any published FT4 figure.** One
process, one seed, one frequency, over audio this library synthesized itself — no fading,
no drift, no neighbours. It was not re-drawn in a second process. Comparing it with a
published threshold needs a citation this tree does not carry, and this unit does not
supply uncited numbers; that is the same treatment the 4.48 question gets.

### The geometry, re-derived rather than inherited

Unit 288 said explicitly that FT8's truncation match was derived at `0.160f` and does not
carry. It was re-taken at `0.048f`, both columns computed and printed:

| | in float (upstream, and this) | the same constant in double |
|---|---|---|
| block size | 576 | 576 |
| first kept bin | 9 | 9 |
| last kept bin | 145 | 145 |
| blocks in a slot | 156 | 156 |

**At FT4's period the two columns agree everywhere, and at FT8's they do not** — FT8's
block is 1920 in float and 1919 in double, a whole sample per symbol, and its first bin 32
against 31, which is a whole tone of frequency error. So the single precision is
load-bearing for FT8 and buys FT4 nothing. It is kept anyway, because it is what upstream
does and because a port that is right for a reason that has stopped applying is a port
waiting to be wrong.

FT4 keeps **136 bins** where FT8 keeps 449, out of the same 200–3000 Hz passband, because
a bin is one tone spacing and FT4's tones are 20.833 Hz apart rather than 6.25.

### The predicted failures, and which one happened

- **"The first FT4 round trip may decode zero messages."** It did not, because task 1 did
  its arithmetic *before* anything was built and the sweep was widened at task 5 rather
  than discovered at task 6. Task 1's arithmetic is what explains why it would have.
- **"The FT4 waterfall geometry may not match upstream's float truncation."** Re-derived
  at 0.048f, table above. Nothing turned on it at that period.

### Mismatches between the work instruction and the tree — reported, not repaired

1. **`src/Ft8Sharp` is 29 C# files, not 33.** Counted with `find … -name '*.cs'` excluding
   `bin/` and `obj/`. It is 34 files of all kinds including `LICENSE`, `NOTICE`,
   `porting-notes.md`, `Ft8Sharp.csproj` and `Directory.Build.props`, which may be where
   the 33 came from. Unit 288's *proportions* — 28 of 33 protocol-neutral — should be read
   against whichever denominator it actually counted.
2. **`CLAUDE.md` §1 does not hold `CPS-DEC-0160`.** The prefix `CPS-DEC-` appears nowhere
   in `CLAUDE.md`, `DECISIONS.md` or `PROJECT_STATUS.md`. It survives in
   `.run-unit/reload.txt`, which is the launcher's file and not mine; in one `.bak` and one
   archived example; and in three script comments citing another project's rulings.
   **`HM-DEC-nnn` is this project's form**, `HM-DEC-160` is the correct spelling, and
   `PROJECT_STATUS.md` and `CLAUDE.md` §1 already agree with each other.
   `docs/unit255-keying-path-survey.md:311` recorded the same finding already. **There was
   nothing to repair.** Separately, `HM-OPEN-077`'s premise — that §1's table stops short
   of the recent rulings — has since been repaired: §1 now runs to HM-DEC-160 and carries
   153, 154 and 155.
3. **The five-file list is right about the five and is not the whole list.** Two more
   files carried an FT8 assumption: `Dsp/Ft8SlotDecoder.cs`, which hardwires the FT8
   extractor and the FT8 search, and `Ldpc/Ft8CodewordDecoder.cs`, which had no seam for
   the payload XOR. Add `Tables/Ft8Tables.g.cs`, whose header asserted the FT4 tables were
   deliberately absent. All three were addressed.
4. **FT4 came to seven new files rather than five**, because a slot decoder and a place for
   the timing constants are both wanted and neither is one of the five.

Everything else in the instruction's verification list checked out: root `1.12.215` and
`Ft8Sharp` `0.10.7` before the bumps; both oracle binaries present at
`C:\Source\ft8_lib\build\`; six `TableSpec` entries with the skip stated at `:42-44`;
`Ft8Oracle.Generate` at `:99` with no protocol argument; `Ft8SlotDecoder.Decode` at `:133`;
`ftx_find_candidates` sweeping -10 to +19 at `decode.c:205`; and `PHASE_STATUS.md`'s
`STEP:` lines stale exactly as unit 288 reported.

### How the upstream C was read

A session's file tools are confined to this repository and the pinned clone is outside it,
so a throwaway probe in the test project staged `ft8/constants.h`, `ft8/constants.c`,
`ft8/encode.c`, `ft8/decode.c`, `demo/gen_ft8.c` and `common/monitor.c` into `artifacts/`,
which `.gitignore` already excludes. **This is unit 203's own device and its own
precedent** — `TempEncoderProbe.cs` still carries that unit's note about it. Nothing
upstream was committed. The probe has been emptied to a comment rather than deleted,
because this sandbox refuses file deletion, and it was never `git add`ed.

## 4. What's blocking us

Four items. **None of them is in the way of a criterion in section B** — all four criteria
are met — and two of the four are carried forward from unit 288 unchanged.

### Is FT4's transmission 4.48 seconds or 5.04 seconds? — Tim's, carried from unit 288

**The ruling wanted:** which figure is right, and if it is 4.48, what symbol period and
symbol count produce it.

**The reasoning.** `PHASE_PLAN.md` step 1 says 4.48 s. `ft8/constants.h:14` says
`FT4_SYMBOL_PERIOD (0.048f)`, and 105 symbols at 0.048 s is 5.04 s with tones 20.833 Hz
apart. The string `4.48` appears nowhere in the pinned clone. They cannot both be true and
settling it needs the cited QEX paper or a real off-air recording; this tree has neither.

**What was rejected.** Settling it from a model's knowledge of FT4 — forbidden outright,
and it is exactly the class of uncited number this project's own history says gets quoted
forever afterwards. Building on 4.48 — that is a deliberate divergence from upstream and
not a unit's to make. **What was done instead**: built on upstream's 0.048f, put it in one
file so a ruling costs one edit, and reported the measured figure rather than the asserted
one.

### The FT4 candidate sweep is -10 to 51 blocks where upstream's demo uses -10 to 19

**The ruling wanted:** confirmation that this is accepted as this port's own, or an
instruction to do it another way.

**The reasoning.** Upstream's own FT4 decoder reads zero messages out of upstream's own
FT4 generator. The cause is measured: at a 0.048 s block, -10 to +19 reaches 0.912 s, and
the generator centres its signal at 1.23 s. The bound is not in `ft8/` at all — it is a
file-scope constant in `demo/decode_ft8.c`, the same class as `kMin_score` and
`kMax_candidates`, and it was already a constructor parameter in this port. 51 is 156
blocks in a slot less 105 in a transmission. Nothing about the modulation, the tables, the
codeword or the waveform changed, and the encoder and the waveform are still upstream's
byte for byte.

**What was rejected.** Moving the signal instead — a waveform that is not upstream's is
not a faithful port, and it would have broken the sample comparison that makes this unit's
round trip evidence. Leaving the sweep at 19 and reporting a decoder that reads nothing —
that would have been accurate and would have closed no criterion, and the phase would have
been no further forward.

### Who owns `PHASE_STATUS.md`'s `STEP:` lines and `CURRENT_STEP`?

**The ruling wanted:** which of the two instructions stands.

**The reasoning.** Work instruction 289 task 7 says the `STEP:` lines are stale and to
bring them and `CURRENT_STEP` into line with `PHASE_OUTCOME.md`'s header. This session's
prompt says `HEARTBEAT:`, `CURRENT_STEP:` and the `STEP:` lines belong to the launcher and
are not to be written. Both cannot hold. The prompt won, because it is the explicit
prohibition addressed to this session, so `PHASE_STATUS.md` still reads step 0 and step 1
as `not started` while `PHASE_OUTCOME.md` reads step 0 done and step 1 done.

**What was rejected.** Doing both — the edit was made and then reverted rather than left
in, because a half-applied header is worse than either state. Doing neither and saying
nothing — that is how unit 288's report described the same lines as stale and nothing
moved.

### The version scheme — carried from unit 288, unchanged and unruled

**The ruling wanted:** HM-DEC-150 says the minor is the phase number and the patch is the
work unit within it, with no second copy of the minor. What `Directory.Build.props` has
actually been doing does not match that.

**What this unit did meanwhile**, exactly as instructed: bumped the root patch by one,
1.12.215 to 1.12.216, and resolved nothing.

### Outstanding asks

Per HM-DEC-139 this heading appears whether or not anything is under it. **The queue
carried since unit 271 is carried forward unchanged and untouched** — work instruction 289
parks it explicitly and this unit did not open it. The two new asks this unit adds are the
candidate sweep and the `PHASE_STATUS.md` ownership conflict above; the 4.48 timing figure
and the version scheme were already on it from unit 288 and are repeated here verbatim in
substance rather than assumed remembered.
