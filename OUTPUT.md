READ IN THIS ORDER — A, then B, then C.

A. PHASE — Hamlet hears FT8 off the radio and displays the decoded text on screen. Seven steps.
Step 1 (the library exists and its tables are proven) and step 2 (messages round-trip through 77
bits) are CLOSED, and task 1 re-measured the ground under them tonight rather than inheriting it:
Ft8Sharp 198 total / 197 passed / 0 failed / 1 skipped in 3 s at entry, the library building at 0
warnings and 0 errors on net8.0 with nullable on, warnings as errors and no PackageReference or
ProjectReference, attribution 110 paths from 2828ab6 with not one under any of the four Hamlet
project folders, and all three channels green at 55 and 13 with every named class including
DecisionLogOrderTests. Step 3 (a valid FT8 signal can be produced) is where this unit worked and
this is the FOURTH unit of it. Step 3 ENTERED this unit with all FOUR of its must-pass criteria met
and its THIRD DELIVERABLE — audio synthesis — NEVER BUILT: there was no code anywhere in this tree
that turned a symbol into a sample. IT LEAVES THIS UNIT BUILT. Ft8Waveform is in
src/Ft8Sharp/Encode/ and its own tests pass. Steps 4 (signals are found in noise), 5 (a found signal
becomes a message), 6 (sensitivity meets the published threshold) and 7 (Hamlet displays decoded
FT8) are all NOT STARTED. Step 4 DOES now have fixtures it did not have this morning, and that is
the practical result of the night: its first exit criterion is that a synthesized signal at a known
offset and time is found, and until tonight there was nothing in this tree that could synthesize
one. Every step depends on the one before it by the plan's own named deviation, so step 3 was the
only step this phase could move.
B. STEP 3 — a valid FT8 signal can be produced. FIVE exit criteria, FOUR must-pass. (1) LDPC parity
matches the reference for known payloads, must-pass — RE-DEMONSTRATED: 1431 real messages at seed
20901 across all six kinds, 237546 parity checks over both table readings, zero failures, and the
checker still watched refusing with all 174 single-bit flips caught; the split reading unit 211
settled — payload byte-for-byte against upstream's own bits over all 51, codeword on the syndrome
check and settled rather than pending — stands and is not re-argued here. (2) The symbol sequence is
bit-identical to ft8_lib's, must-pass — RE-DEMONSTRATED: the comparison still ran and still matched,
51 of 51 messages, 4029 symbols, every one identical; and tonight's own comparison checked the tones
on all 51 again as a precondition before any sample was compared, with 0 mismatches. (3) Audio
synthesis produces a signal the reference decoder decodes, nice-to-pass — THIS UNIT'S TARGET.
decode_ft8.exe DOES NOT EXIST on this machine; the clone holds build\gen_ft8.exe at 208896 bytes and
no decoder. CRITERION 3 IS NOT MET ON ITS OWN TERMS. What was taken instead: 51 messages compared
against upstream's WAV; THE MAXIMUM ABSOLUTE SAMPLE DIFFERENCE IS 1 COUNT in int16 terms, at the
message "CQ with a grid", sample 18204; the bound asserted is 2 counts, justified as the smallest
whole number that holds — one above the measured maximum, taken after the measurement was printed
rather than before; 0 messages were identical in every sample, with 251988 of 9180000 samples
differing at all, 2.745 per cent, every one by exactly one count; the alignment came FROM THE SOURCE,
computed from the pin's own timing, and nothing was cross-correlated; and the tone recovery
recovered 4424 of 4424 symbols across all 56 corpus messages, worst frequency error 0.000175 Hz
against a 6.25 Hz tone spacing. (4) Ft8Sharp tests green, must-pass every unit — 222 total, 221
passed, 0 failed, 1 skipped, 3 s of test time and about 7 s of wall clock. The one skip is
Ft8TableGenerationTests.RewriteTheCheckedInTablesFile, the table write gate, which is meant to skip
and is the only skip in the project. (5) Attribution clean from 2828ab6 and the channel tests green,
must-pass every unit — 118 paths, NOT ONE under src/Hamlet.App/, src/Hamlet.RadioEngine/,
tests/Hamlet.App.Tests/ or tests/Hamlet.RadioEngine.Tests/; AudioSeamTests and PrivilegeTests green
at 55, DecisionLogOrderTests, VersionTests, DecisionEmissionTests and VoiceTests green at 13, all
re-run after the version bumps with VersionTests among them.
C. THIS REPORT — the library CAN now turn 79 symbols into samples, and that code is
src/Ft8Sharp/Encode/Ft8Waveform.cs. The waveform stands on THREE legs and all three exist tonight:
its own tests, which run on any machine; the independent second implementation of task 6; and
agreement with upstream's own WAV, which is the strongest and skips everywhere but here. Task 4's
comparison was watched REFUSING all four of its named alterations, every one far outside the bound —
one symbol altered 65460 counts, the base frequency moved one tone spacing 65533, the smoothing
parameter halved 35905, and a changed sample rate refused outright on the length. Task 6 was NOT
dropped: task 4 ran and agreed, so the FIRST branch of its condition licensed dropping it, and it
was kept anyway because task 4's third named alteration cannot be built without an implementation
that takes the smoothing parameter, so dropping it would have cost a required refusal. The shape of
the sample difference is GROWING with time at a constant magnitude — under 1 per cent of samples in
the first fifth rising to over 5 per cent in the last, none at all in the silence, and never more
than one count anywhere — which reads as accumulated last-place rounding in a phase both sides
accumulate one single-precision addition at a time, and NOT as a wrong sample rate or symbol period,
either of which would grow the magnitude rather than only the count. The Ft8Sharp project still
returns in seconds — 3 s of test time for 222 tests — and NO corpus was cut for the clock. There are
8 .obj at the repository root, unchanged, and neither they, nor any WAV, nor the patched binary, nor
anything under tools\ was committed. Section 4 raises 3 items and NONE of them stands in the way of
a criterion named in B.

UNIT:       212 — complete at task 7 of 7 — 2026-09-01 20:06
PHASE GOAL: Hamlet listens to the radio, finds FT8 transmissions in the audio, and puts the words
            they carry on the screen.
UNIT GOAL:  Make the seventy-nine settled tones into the actual audio an FT8 transmission is, in the
            library rather than in a test, and hold every sample of it against the file upstream's
            own program writes for the same message.
ADVANCED:   yes — step 3's third and last named deliverable, audio synthesis, went from not existing
            to existing and agreeing with upstream to one count, and step 4 has fixtures it did not
            have this morning.
NUMBER:     step 3 deliverables built: 2 -> 3 of 3, and criterion 3: not met
DRIFT:      0 consecutive units without advance  (was 0 — unit 211 closed criterion 2)

# 1. What was asked, and what happened

Seven tasks were asked. **All seven ran; none was dropped and none was left unreachable.** Task 6
was the named drop candidate and is discussed in section 3 — the branch that licensed dropping it
was taken and the task was kept anyway, for a reason the drop condition does not cover.

The unit's target was task 4, and it came out. **Nothing is left over and nothing is deferred.**

Two refusals are reported as refusals rather than routed around, per the standing rule:

- **The agent's own file tools were refused `C:\Source\ft8_lib`**, exactly as the arbiter's were for
  units 209 through 212. Everything read from the clone tonight was read by the test process, which
  is the sanctioned route and the one this project already uses. **No route around the refusal was
  attempted.**
- **The system temp folder was refused** when I tried to confirm that no WAV was left behind after
  the run. The deletion is in a `finally` block per message rather than at the end, and the design is
  described in section 3, but **I could not verify the folder is empty and I am not claiming that I
  did.**

`tools\arbiter\validate-output.bat` was attempted in the listed spellings; the outcome is in
section 3.

# 2. What is now true that was not

**This library can make a signal.** Before tonight `src/Ft8Sharp/` held a message packer, a CRC, an
LDPC encoder and a symbol encoder — four things that compute numbers. It now holds a synthesizer
that turns those numbers into the fifteen seconds of audio an FT8 transmission actually is, and
that audio has been held sample for sample against the audio Goba's own program writes for the same
message.

**They agree to one count.** Over fifty-one messages and nine million one hundred and eighty thousand
samples, the largest disagreement anywhere is a single count of a sixteen-bit sample. So what this
library produces is not merely something we believe is FT8: it is, to within the last place of the
arithmetic, the same waveform every FT8 decoder in the world already decodes.

**Step 4 can start.** Its first exit criterion is that a synthesized signal at a known offset and
time is found. Until this morning there was nothing in this tree that could synthesize one.

**And one thing is now true that is worth more than the pass.** The port computes the phase in single
precision because upstream does, and the second implementation showed what that is worth: a version
of this synthesizer that held the phase in double — more accurate, and the obvious thing to write —
disagrees with upstream by up to a hundred and seventeen counts. The agreement to one count is not
luck. It is the consequence of reproducing upstream's arithmetic instead of improving on it, and
that is now written into `porting-notes.md` where the next person to tidy the code will find it.

# 3. Findings

## The comparison, in one block, before any prose

```
decode_ft8.exe exists                : NO — the clone holds gen_ft8.exe (208896 bytes) and no decoder
generator answered                   : YES, exit 0
    from which image                 : unit 211's PROVEN PATCHED COPY, not the original
    proofs re-asserted on this run   : whole-file 2 bytes differ, all inside the field written;
                                       .text hashes equal; no-argument behaviour identical;
                                       real message exit 0x00000000

sample rate                          : 12000 Hz
bit depth                            : 16 bits
channels                             : 1
header length                        : 44 bytes
total samples                        : 180000
file agreed with the source          : YES, on every one of those

where the signal starts              : sample 14160, after 14160 samples of silence
    signal length                    : 151680 samples
    trailing silence                 : 14160 samples
    how that was arrived at          : READ from the pin's own timing. NOT a search.
                                       Nothing was cross-correlated.
    silence asserted to be silent    : YES, at both ends, on upstream's file and on ours

messages compared                    : 51
MAXIMUM ABSOLUTE SAMPLE DIFFERENCE   : 1 count
    at                               : "CQ with a grid", sample 18204
samples differing at all             : 251988 of 9180000
    as a fraction                    : 2.745 %
messages identical in every sample   : 0
BOUND ASSERTED                       : 2 counts
    justification                    : the smallest whole number that holds, one above the
                                       measured maximum, chosen AFTER the measurement was
                                       printed rather than before it

the four alterations, each refused:
    one symbol altered (position 40) : REFUSED — 65460 counts, 56629 of 180000 samples differ
    base frequency + one tone spacing: REFUSED — 65533 counts, 151656 of 180000 samples differ
    smoothing parameter halved       : REFUSED — 35905 counts, 145889 of 180000 samples differ
    sample rate changed to 48000     : REFUSED on the length — 720000 samples against 180000,
                                       refused outright rather than compared over a prefix

tone recovery, which survives with no clone:
    messages                         : 56 of 56
    symbols recovered                : 4424 of 4424
    worst frequency error            : 0.000175 Hz against a 6.25 Hz tone spacing
```

## The shape of the difference, read rather than left at a number

```
fifth 1 of the signal : 10635 of 1547136 differ  (0.687 %)
fifth 2               : 34193 of 1547136 differ  (2.210 %)
fifth 3               : 58452 of 1547136 differ  (3.778 %)
fifth 4               : 64705 of 1547136 differ  (4.182 %)
fifth 5               : 84003 of 1547136 differ  (5.430 %)
in the silence        :     0 of 1444320 differ
```

**The count grows through the transmission and the magnitude does not.** Never more than one count,
anywhere, at any point. That is the signature of **accumulated last-place rounding** in a phase both
sides accumulate one single-precision addition at a time.

It is explicitly **not** the other readings the instruction names. A wrong sample rate, symbol period
or accumulated phase *error* would grow the **magnitude** and not merely the count of affected
samples. A difference confined to the ends would be the padding or the ramp — there is none in the
silence at all. A difference steady inside every symbol would be the pulse shape or the smoothing
parameter. A difference at symbol boundaries only would be phase continuity. A difference in one
message and not the others would be the symbols rather than the synthesis, and criterion 2 would be
back open — it is not: the tones were checked on all fifty-one before any sample was compared.

## Whether the two sides were being asked the same question

**Checked before a single sample was compared, on every message.** This is unit 211's most portable
lesson applied rather than restated: our API names a message type, upstream's generator is handed a
string and chooses the type itself, and where they choose differently the tones differ for a reason
that has nothing to do with either encoder. So the comparison reads upstream's packed bytes and its
tone line first and refuses to compare samples where either disagrees.

**0 packed-data mismatches and 0 tone mismatches over all 51.** Had the sample comparison found a
difference, this is what would have told us it was ours.

## What task 2 read out of the pin, as shapes and numbers

The generator carries its own synthesis in `demo/gen_ft8.c` rather than calling a library one — 4
functions, 190 lines — so there is no file under `ft8/` to point at for the waveform. The WAV writer
is `common/wave.c`, 2 functions. The timing is in `ft8/constants.h`.

Eleven structural facts are now asserted by a checked-in test that skips when the clone is absent:

- the pulse is a **Gaussian-filtered frequency-shift pulse**, spanning **three symbol periods**;
- **phase is accumulated across symbol boundaries and never restarted**;
- the symbol timing and the smoothing factor are **named parameters** of the synthesis, not literals;
- **dummy symbols** repeating the first and last tones extend the pulse past both ends;
- there is an **envelope ramp** over part of the first and last symbol;
- the sample reaches the file as **int16**, converted in the WAV writer;
- the rounding is **a half added before a truncation**, not a rounding function;
- the file is tagged **RIFF**, the form **WAVE**, with a **fmt** chunk and a **data** chunk.

The generator **does** parse its own arguments, so the base frequency can be set from the command
line and the comparison is not confined to the default.

**No constant of upstream's appears in this report, in any commit message, or in
`porting-notes.md`.** The protocol's published facts — 79 symbols, 8 tones, 6.25 symbols per second,
a 15-second slot — are from the QEX paper the NOTICE cites and are free.

## The synthesizer and each of its own tests

`src/Ft8Sharp/Encode/Ft8Waveform.cs`. Takes symbols, a sample rate and a base frequency; returns a
buffer. **No `Random`, no clock, no ambient state, no file, no stream, no device, no
`PackageReference`, no `ProjectReference`.** It needed no new table: the Costas pattern and the Gray
map were already in the generated tables file and already read by `Ft8SymbolEncoder`, and the
synthesizer takes that encoder's output rather than re-deriving a tone.

| # | Test | Result |
|---|---|---|
| 1 | **Length** at two rates | PASS — 1920 samples per symbol and 151680 of signal in 180000 of slot at 12000 Hz; 7680 and 606720 in 720000 at 48000 Hz; silence asserted silent at both ends |
| 2 | **Range**, on the loudest sequence constructible | PASS — every sample within ±1, no sign inversion on any loud sample, and the conversion fed past both ends of its range on purpose and clipping correctly |
| 3 | **Phase continuity** | PASS — largest step at a symbol boundary 0.5347, largest step anywhere else 0.5397, against a derived bound of 1.0930. Boundaries are indistinguishable from the middle of a symbol |
| 4 | **Tone recovery** | PASS — **4424 of 4424 symbols over 56 of 56 messages**, worst frequency error 0.000175 Hz against 6.25 Hz spacing |
| 5 | **Determinism** | PASS — byte-identical buffers over 151680 samples, and demonstrably different at another base frequency |
| 6 | **Watched refusing** | PASS — a symbol count that is not 79 (four lengths), a symbol outside the alphabet (three positions), a non-positive rate (three values), a base frequency putting the top tone at Nyquist or the bottom at DC (five values), and one guard of my own |

**On test 3, the strongest thing available:** the claim that this catches a phase-restarting port is
not left as a claim. Task 6's second implementation builds one deliberately, and the measurement is
watched **catching** it — **0.9995 at a boundary against 0.5397 elsewhere**, where the faithful
waveform gives 0.5348 against 0.5397. That waveform is the right length and every symbol still comes
back out of it, which is exactly why the continuity measurement has to exist.

## Task 5 — criterion 3, in the words it asks for

**`decode_ft8.exe` is not on this machine, so CRITERION 3 IS NOT MET ON ITS OWN TERMS and this unit
does not claim it.**

- **What the criterion asks:** that audio synthesis produce a signal the reference decoder decodes.
- **The reference decoder is not built here.** It is a different program from the generator and a
  materially larger build — it pulls in the FFT — and the owner's script builds the generator only.
- **Building it needs a compiler run for which the permission scope has no rule.** That is
  owner-class under `ARBITER.md` §6 and is already a standing note with the owner from units 210 and
  211. It was not attempted and no widening of the scope is argued for.
- **What was taken instead:** task 4's sample-level agreement with upstream's own WAV, and task 3's
  tone recovery out of our own waveform.
- **The one thing neither of them shows:** **nothing has demodulated this waveform.** Not this
  project, not upstream, not anybody. No candidate search has run over it, no soft symbol has been
  formed from it, no belief propagation has read one. **That is steps 4 and 5 and this unit claims
  none of it.**

That statement is a checked-in test rather than a line in a report — it asserts nothing about the
outcome and passes either way, so the decoder arriving on a later machine closes the criterion
rather than turning the test red.

## Task 6 and which branch of its drop condition

**NOT DROPPED. The branch that licensed dropping it is the FIRST one — task 4 ran and it agreed — and
it was kept anyway.**

The reason is one the drop condition does not cover, and it is worth stating precisely. **Task 4 is
required to be watched refusing a waveform built with the smoothing parameter moved.** The library
holds that parameter fixed at the modulation's own value, which is correct for a library that only
ever does FT8. There is therefore no altered waveform to refuse without a second implementation that
takes the parameter — so **dropping leg B would have left one of task 4's four required refusals
unexercised**, and a comparison that has never refused is not a comparison.

Having built it, it earned its keep twice over:

- **It found the night's most useful secondary result.** Leg B holds the phase in double and differs
  from the library by **117 counts** at worst, against **1 count** for upstream. The divergence is
  drift, not disagreement, and the test measures that rather than assuming it: **1 count of
  difference in the first symbol and 103 in the last.** The cause is that a half-radian phase step in
  single precision is off by a fixed fraction of its last place **in the same direction at every one
  of 151680 samples**, so it accumulates instead of cancelling. **A port that computed the phase in
  double would have been more accurate and would have disagreed with upstream by about a hundred
  counts.** The agreement to one count is a consequence of reproducing upstream's arithmetic, and
  that is now recorded where the next person to tidy the precision will find it.
- **It provided the phase-restarting waveform** that turns test 3 above from a claim into a
  demonstration.

Leg B is genuinely independent: the pulse comes from numerical integration of the Gaussian where the
library evaluates a series for the error function, the phase is a running total in double where the
library accumulates in single with a remainder every sample, and it shares no term with the port. Its
bound is **128 counts** and was written down after the measurement.

## Criteria 1, 2, 4 and 5, re-demonstrated

- **Criterion 1** — 1431 real messages at seed 20901 across all six kinds, 237546 parity checks over
  both table readings, zero failures; the checker still watched refusing with all 174 single-bit
  flips caught and each disturbing exactly three checks. The split reading unit 211 settled is
  untouched and is not re-argued.
- **Criterion 2** — the comparison against upstream's own tones still ran and still matched: **51 of
  51 messages, 4029 symbols, every one identical.** Tonight's comparison independently re-checked the
  tones on all 51 as a precondition, with 0 mismatches. Leg B for the symbols is green at 3 of 3 over
  56 messages and 4424 symbols, untouched and not weakened.
- **Criterion 4** — **222 total, 221 passed, 0 failed, 1 skipped**, 3 s of test time. **24 tests
  added tonight.** The one skip is `Ft8TableGenerationTests.RewriteTheCheckedInTablesFile`, the table
  write gate, which is meant to skip; it is the only skip in the project and no test was disabled,
  quarantined, renamed or weakened to reach that number.
- **Criterion 5** — attribution **118 paths** from 2828ab6 with **not one** under any of the four
  Hamlet project folders. Channels green at **55** and **13**, every named class including
  `DecisionLogOrderTests`, all re-run after the version bumps with `VersionTests` among them.

## Divergences

**One added, numbered 16 on from fifteen.** A sample rate at which the signal's two lengths disagree
is refused. Upstream reaches that length twice by two routes — from the transmission's duration,
which sizes the slot, and as the symbol count times the samples per symbol, which is what the
synthesis writes. They agree at the rate FT8 is used at and part company elsewhere, where upstream
would run past the end of its own stack buffer. Nothing here runs past anything, but a signal of one
length laid into a slot sized for the other puts every sample after the join at the wrong offset, so
it is refused with the reason. **This is a divergence in behaviour at rates upstream never uses, and
not in the waveform.** The count now stands at sixteen. The fifteen inherited ones were not re-argued
and none was removed.

## The versions

- **`src/Ft8Sharp/Directory.Build.props`: 0.5.2 → 0.6.0.** The library can now make the audio a
  transmission is — a capability it did not have rather than a correction to one it did. The comment
  says what the minor does not claim: nothing here decodes, there is no demodulator, no program has
  been run against this waveform to see whether it can be read back, and it is still not a 1.x. It
  can now speak and it still cannot hear.
- **Root `Directory.Build.props`: 1.12.18 → 1.12.19** under HM-DEC-150.

Both re-run afterwards; `VersionTests`, which is what catches a mistyped bump, is green.

## The .obj count, and what was not committed

**8 `*.obj` at the repository root**, the same eight, untouched and uncommitted. Every file this unit
committed is a source file under `src/Ft8Sharp/` or `tests/Ft8Sharp.Tests/Encode/`, plus
`porting-notes.md`, the two `Directory.Build.props`, `PROJECT_STATUS.md`, `PHASE_STATUS.md` and this
report — **eleven paths, listed and checked.** No `.obj`, no WAV, no sample, no tone, no payload, no
patched executable, nothing under `tools\`, nothing from `C:\Source\ft8_lib`, and nothing upstream's
generator emitted.

`tools\build-ft8-oracle.bat` is present, untracked and still stamped **16:02:11** — unchanged, as
expected, and not edited or run. Known items 6 and 10 both confirmed and neither touched:
`PHASE_OUTCOME.md` has no entry for unit 211, and `TempEncoderProbe.cs` is still on disk and still
tracked.

## Mismatches against the instruction, reported and not repaired

1. **`git status --short` printed 27 lines at entry, where the instruction says 26.** Not
   investigated further; the loop's own uncommitted files are beyond counting them. It reads 29 now,
   which is this report and the untracked files the run produced.
2. **The attribution count is 118, where the instruction says 110.** That is not a mismatch in the
   instruction — it was 110 when measured at entry, exactly as stated, and the eight added paths are
   this unit's own commits. Recorded so the next unit's figure is not read as drift.
3. **`decode_ft8.exe` is absent, which is what the arbiter expected.** Recorded as a confirmation
   rather than a mismatch.

# 4. What needs a decision, or is carried forward

**3 items. None of them stands in the way of a criterion named in section B.**

1. **The reference decoder is still not built, and criterion 3 cannot be closed by any unit until it
   is.** This is the third unit to leave it with the owner and the shape has not changed: building
   `decode_ft8` needs a compiler run, the permission scope has no rule for one, and that is
   owner-class under `ARBITER.md` §6. **It does not block step 3** — criterion 3 is nice-to-pass and
   the step's four must-pass criteria are met — and it does not block step 4, which needs fixtures
   and now has them. It is the only route to the criterion as literally written. **Nothing was
   attempted and no widening is argued for.**
2. **The validator did not run, and two other refusals.** `tools\arbiter\validate-output.bat` was
   attempted in **all five** spellings `tools\arbiter\run-unit-tools.txt` lists. **None of them
   executed it.** The two `//c` forms lose the backslashes before `cmd` sees them and report
   `'toolsarbitervalidate-output.bat' is not recognized`; the two `/c` forms and the bare form open
   an interactive `cmd` that prints its banner and a path and never runs the batch file. This is
   reported as a failure to invoke rather than routed around — **the file was not edited, no
   alternative path was tried, and the ordering block was checked by hand instead**, as known item 15
   directs. Separately, **the harness refused me `C:\Source\ft8_lib`** (everything was read through
   the test process instead, which is the sanctioned route) and **refused me the system temp folder**
   when I tried to confirm no WAV was left behind. The files are deleted per message in a `finally`
   block rather than at the end and the run completed normally, so none should remain — but **I could
   not check, and I am not claiming I did.**
3. **`PHASE_OUTCOME.md` and `PHASE_STATUS.md` are still stale and still disagree**, and
   `PHASE_OUTCOME.md` still has no entry for unit 211. Both are the loop's and neither was
   hand-edited. `PHASE_STATUS.md`'s `WORK_INSTRUCTION:` line was set to this unit's, which is the one
   line of it that is mine.

**Nothing here is a request to change a ruling, and nothing waits on Tim before step 4 can begin.**

---

*Validator: `tools\arbiter\validate-output.bat` was attempted in all five spellings the tools file
lists and **none of them executed it** — see section 4, item 2. **The ordering block was therefore
checked by hand** against the stated requirements, and it holds: `A.` is at line 3, `B.` at line 21
and `C.` at line 48, each beginning a line with no indentation; all three fall inside the first 60
lines; all three sit above the `UNIT:` line at line 68; and the count in C is written as a digit —
`raises 3 items`.*
