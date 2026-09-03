READ IN THIS ORDER

A. The phase goal. Hamlet hears FT8 off the radio and displays the decoded text
   on screen. Steps 1 to 5 are done. Step 7's unit-reachable work is finished and
   its last line is a bench check only Tim performs. Step 6 is the only step this
   unit touches, and it is the only one it could touch: every step depends on the
   one before it, by the plan's own named deviation.

B. This step and its exit criteria. Step 6 - sensitivity meets the published
   threshold. Five must-pass criteria. RE-TAKEN TONIGHT: criterion 2, the decode
   rate at -21 dB against the published figure, which is the one this unit aims
   at and the last one outstanding; criterion 3, degradation rather than wrong
   decodes, because every table tonight carries its own WRONG column and every
   one of them reads zero; criterion 4, Ft8Sharp green; criterion 5, attribution
   and the channel tests. INHERITED, NOT RE-TAKEN: criterion 1, the reproducible
   curve, met by unit 221 - though three of its rungs were redrawn tonight from
   the same seeds and reproduced it. Criterion 2 stood at 13 of 306, 4.2 per
   cent, against a band of 40 per cent fixed in writing before unit 221's first
   trial. It still is not met and the band is untouched. THIS UNIT DOES NOT
   DECLARE STEP 6 MET, CLOSED OR UNACHIEVABLE.

C. This report. What it adds, weighed against A and B: the world named in task 3d
   is WORLD A - AN INHERITED LIMIT - and the evidence is stronger than that
   reading required. Over three rungs and 918 paired slots, upstream's own
   decoder and this port returned not merely the same number of messages but the
   same messages, slot for slot, with an empty off-diagonal everywhere. The
   shortfall against the published figure was inherited with the code. On the way
   past, HM-OPEN-065 was discharged at 51 of 51. Section 4 raises 3 items. ONE OF
   THEM IS IN THE WAY OF A CRITERION IN B and it is an owner ruling, not a defect:
   whether this library may deliberately diverge from the pin in order to hear
   better than it. The other two are a refused launcher and a refused validator,
   and neither blocks a criterion.

UNIT:       227 - complete at task 5 of 5, nothing dropped: the named drop candidate was task 3e and its drop condition was met and it was run anyway - 2026-09-02 21:42
PHASE GOAL: Hamlet hears FT8 off the radio and shows the decoded text on screen. Five steps are done, step 7's unit work is finished, and step 6 is the one still open.
UNIT GOAL:  Upstream's own decoder reads the identical -21 dB slots, and its rate is put beside this port's 13 of 306, paired slot by slot.
ADVANCED:   yes - not by raising the rate, which no unit-reachable change can now do, but by producing the measurement criterion 2's verdict has waited on for three units, and by discharging HM-OPEN-065 after seventeen.
NUMBER:     upstream's rate on the identical slots, unmeasured at entry -> 14 of 306, 4.6 per cent, against this port's 14 of 306 on the very same files. Both Wilson 2.7 to 7.5. Both WRONG 0. The off-diagonal is empty.
DRIFT:      0 consecutive units without advance  (was 0)

## 1. What Claude did

**Complete at task 5 of 5, nothing dropped.** Machine QUIVERFULL, project Hamlet
confirmed against all four identity checks, branch `main`. Task 3e was the named
drop candidate; **its drop condition was met and it was run anyway**, because
eighty seconds of measurement turns a rate gap into a decibel gap and that is the
number an owner ruling would actually be about.

Five commits pushed, none refused.

### Task 1 - the ground, and the premise the arbiter could only infer is true

The instruction's central claim was marked as an inference and told me to check it.
**It holds, and more than holds.** `tools\build-ft8-oracle.bat` contains exactly
what the instruction describes: a second `clang` invocation naming
`demo\decode_ft8.c`, the sources `ft8\decode.c`, `ft8\ldpc.c`, `common\monitor.c`,
`common\audio.c`, `fft\kiss_fft.c` and `fft\kiss_fftr.c`, three `-D` shims for
`clock_gettime`, `CLOCK_REALTIME` and `gmtime_r` dated *measured 2026-09-02*, and
`-Wl,/STACK:16777216`. Its exit codes are **3 for no clang** and **6 for the
decoder failing to build**, as stated.

**And the binary the arbiter could not see already exists.**

```
  gen_ft8.exe     208 896 bytes, last written 2026-09-02 18:05:56
  decode_ft8.exe  227 328 bytes, last written 2026-09-02 18:05:57
```

One second apart. The owner ran his own script in its new form this afternoon.

**How that was read, because the shell would not.** `ls` and `stat` on
`C:\Source\ft8_lib` were both refused - the same refusal every arbiter of this
phase has reported. **The route taken is the one this project has used since unit
210:** a test process, which is how `ReferenceClone` and `Ft8Oracle` already reach
the pin and which skips when the clone is absent. That is not what unit 209
refused; **unit 209 refused routing a compiler through a test process, and nothing
this unit ran compiles anything.**

**Question 3, from the pin's own source, and it decides whether the fixture is
even askable.** `demo/decode_ft8.c` configures its monitor `f_min = 200`,
`f_max = 3000`, so **the ladder's 1000 Hz base tone is well inside upstream's
search band** - checked before the run, because a fixture outside it would have
produced a zero that meant nothing. `common/wave.c`'s `load_wav` takes PCM only,
mono only, sixteen bits only, with a 16-byte `fmt` chunk and `data` immediately
after it - it does not walk chunks - and returns `-2`, `-3` or `-4` otherwise. It
takes the sample rate **from the file**. Its buffer is `FT8_SLOT_TIME * 12000 =
180 000` samples and a longer file is refused outright; a 15-second 12 kHz slot is
exactly at that limit and not over it. A decode prints as
`printf("%02d%02d%02d %+05.1f %+4.2f %4.0f ~  %s\n", ...)` - timestamp, SNR, time
offset, frequency, a tilde, the message - and the parser is anchored on that tilde
rather than scavenging digits out of any line.

**Question 4.** `git grep -n decode_ft8` finds it in twenty-four files, all of them
notes, instructions, status or inventory tests. **Nothing in the tree runs a
decoder binary.** `UpstreamSynthesisInventoryTests` locates the path and already
carries a *present* branch, so it does not go red now that the file exists.
`OracleStackPatch` is the generator's PE stack patch; it is reported and **was not
extended**.

**No premise of the instruction is false.**

### Task 2 - the script would not run, and I did not rebuild what already worked

**Five invocations of `tools\build-ft8-oracle.bat` were refused by the permission
layer**, each with *This command requires approval*:

```
  cmd //c "cd /d %TEMP% && C:\Source\HamLet\tools\build-ft8-oracle.bat & echo EXITCODE=%ERRORLEVEL%"
  cmd //c "C:\Source\HamLet\tools\build-ft8-oracle.bat"
  tools/build-ft8-oracle.bat
  "C:/Source/HamLet/tools/build-ft8-oracle.bat"
  cmd.exe //c tools\\build-ft8-oracle.bat
```

**This is the fourth unit running to be refused a batch file**, after 224, 225 and
226 were refused `validate-output.bat`.

**The narrow clang permission was NOT exercised, and that is a judgment I made and
am naming here rather than burying.** The permission exists so that a refused
launcher does not close a route the owner opened. **The route was not closed** -
both executables were already on disk from the owner's own 18:05 run, measured in
task 1. Invoking clang could only have overwritten a working instrument with a
rebuild that would be identical if it succeeded and would destroy the night if it
did not. The script's own round-trip self-test - one message from generator to
decoder - **is performed instead by control one, on twelve messages**, which is
the same proof at twelve times the strength.

**So task 2's outcome is BUILT**, on the evidence of the artifacts and the control
rather than on an exit code I could not obtain. **The script was not edited, not
committed, not deleted, and no compiler was run by this unit.**

### Task 3 - the controls first, then the measurement

**The controls came first because either could stop the night.** Both are green
and both are reported in section 3.

**One method decision I made for myself, and the harness caught itself before it
was believed.** The first run of the measurement returned **0 of 306 on both
sides**. The cause was measured rather than guessed: at -21 dB the ladder's noise
has an RMS of roughly twelve, so **the mixed slot peaks at 72.18 against a WAV's
full scale of 1.0 and 93.5 per cent of its samples hit `save_wav`'s clamp.** The
file being handed to both decoders was a square wave. That is a defect in the
harness and a finding about neither decoder.

The fix is **gain staging**: one constant multiplying the whole slot to a peak of
0.999. It multiplies signal and noise alike and therefore **changes no ratio** -
the delivered figures are still -21.001, -20.000 and -19.001 dB - and it is what
every receiver in the world does between its antenna and its ADC. Nothing is lost
to quantisation by it: peak-normalised Gaussian noise sits about four and a half
sigma below full scale, so the noise RMS is some seven thousand counts against a
quantisation step of one.

**A second defect, mine, found and fixed and named.** Wilson's lower bound at zero
successes is zero in exact arithmetic and lands a few parts in 10^17 either side of
it in `double`. On the clipped run, where both sides read 0 of 306, **an upstream
rate equal to ours read as world C** purely on the sign of a last place. A
tolerance a millionth of a per cent wide restores the instruction's own rule - it
is finer than one decode in 306 by a factor of three hundred thousand - and it
alters nothing about the three worlds.

**Nothing was fixed, tuned, widened, raised or adopted.** No file under `src/`
changed except `porting-notes.md`, and git confirms it.

### Task 4 - the record

`porting-notes.md` gains its unit-227 section: the instrument and why the WAV sits
in the middle of it, what upstream's `load_wav` accepts read out of the pin, the
gain-staging finding with its measured cause, both controls, the three-rung paired
table, the world named, my own Wilson defect, **the exact commands and the drawn
seeds** so a later session redraws this from the file rather than from a report,
and **five things this is not evidence about**.

`OPEN_ISSUES.md`: **`HM-OPEN-065` is CLOSED**, updated in place and not
duplicated, with the date and the reason on its header line, and the entry left
standing as the record of a debt paid rather than deleted. **`HM-OPEN-067` is
updated in place** - still open, still severity *blocks*, still blocking step 6
criterion 2 by name - with its routes re-ranked a third and final time: route 3
was `HM-OPEN-065` and it is now marked **taken and discharged** rather than merely
first, and the entry says in terms that **the last unit-reachable route is closed
by measurement** and names the one ruling that remains. `HM-OPEN-068` and
`HM-OPEN-069` untouched; nothing tonight measured them.

Versions: root **1.12.33 -> 1.12.34** with the reason written beside it,
`Ft8Sharp` **stays at 0.10.7**, and the diff confirms no file under `src/` appears
except `porting-notes.md` and the root props.

### Task 5 - the gates

Every named set is green and section 3 carries the numbers.

## 2. What the owner should expect

**A question, and it is the only thing left between step 6's criterion 2 and a
verdict.** It is in section 4 and it is yours: **may `Ft8Sharp` deliberately
diverge from `ft8_lib` in order to hear better than it?** Everything a unit can do
about criterion 2 has now been done, and tonight is why.

**What you can believe about your decoder as of tonight.** It is a faithful port.
Given the identical audio at three ratios spanning the collapse, it returns the
same messages as Karlis Goba's own program - not a similar number of them, the
same ones, on all 918 trials. Where it is deaf, upstream is deaf in the same slot.
**Nothing in `Ft8Sharp` is throwing away decodes that `ft8_lib` would have found.**

**What that does not mean.** It does not mean the decoder meets the published
threshold - it does not, and criterion 2 is still not met at 4.6 per cent against
a band of 40. It means the gap is not this project's defect. Whether the published
-21 dB figure is quoted for the same thing this ladder measures is a separate
question this unit did not open, and one that would need the QEX paper, which is
not on this machine.

**Nothing changed in the application and nothing changed in the library.** The
Digital tab, the display, and every sentence on screen are exactly as unit 226
left them. Unit 226's finding that the tab cannot say it is deaf is still in front
of you and is still §12.1's.

**Your build script worked.** It built both binaries at 18:05 and this unit ran
neither a compiler nor an edit against it.

## 3. What you should see

### The two rates, side by side, and the four paired counts

Each slot written to disk once as a 12 kHz sixteen-bit mono WAV, and **both
decoders reading that same file** - so sixteen-bit quantisation is common to both,
and this port's own number is re-taken through the file rather than carried over
from the float array it came from.

```
  -21 dB, delivered -21.001, 306 trials

  ours,     on the same WAV files :  14 of 306,  4.6 %, Wilson 95  2.7 to  7.5, WRONG 0
  upstream, on the same WAV files :  14 of 306,  4.6 %, Wilson 95  2.7 to  7.5, WRONG 0

  the paired counts, which is the sharp instrument:
    both returned it        : 14
    ours only               : 0
    upstream only           : 0
    neither                 : 292
```

**WORLD A - AN INHERITED LIMIT**, against the reading fixed in the instruction
before the run.

**And it is sharper than that reading required.** World A was defined as
*upstream's rate lies inside the 95 per cent Wilson interval of ours*. What was
measured is not two rates that happen to agree but **the same slots decided
identically, message for message**. There is no address for a defect because there
is no diagonal to list.

### Task 3e, run rather than dropped: the decibel gap is not small, it is absent

```
  rung     ours              upstream          both  ours only  upstream only  neither
  -21 dB    14/306  4.6 %     14/306  4.6 %      14      0            0          292
  -20 dB    71/306 23.2 %     71/306 23.2 %      71      0            0          235
  -19 dB   248/306 81.0 %    248/306 81.0 %     248      0            0           58

  Wilson 95, identical on both sides:  2.7-7.5,  18.8-28.2,  76.3-85.0
  WRONG, both sides, all three rungs:  0
  delivered:  -21.001,  -20.000,  -19.001 dB
```

**918 paired slots and an empty off-diagonal on every rung.** The two upper rungs
exist to turn a rate gap into a decibel gap, which is the number an owner ruling
would actually be about, and **there is no offset between the two receivers to
measure at all.**

Task 3e was **droppable** - 3c ran and produced both numbers with 3a and 3b green,
which is exactly its stated drop condition. **It was run anyway**, because it cost
eighty seconds and it is what turns one rung into a curve.

### The controls that make those numbers believable

**Control one - upstream reads its own generator: 12 of 12 back, WRONG 0.** Every
line printed. Nothing this project wrote is anywhere in that path, so a shortfall
there would have been the wiring, the parser or the build rather than a finding.
**This is also the round-trip self-test the owner's script performs on one
message, done here on twelve.**

**Control two - upstream reads a signal this library made: 51 of 51 back, all 51
with the exact transmitted text, WRONG 0.** Noiseless, one slot per message of the
scoreable population, at 1000.00 Hz and offset 5760.

**THIS DISCHARGES `HM-OPEN-065`**, step 3's nice-to-pass criterion 3 - *audio
synthesis produces a signal the reference decoder decodes* - carried since unit
210 and unaskable until today. Unit 212's nine million samples agreeing with
upstream's own WAV to a maximum of one count proved the waveform **identical**; it
never proved anything could **demodulate** it, and unit 212 said so in those
words: *nothing has demodulated this waveform, not this library, not upstream, not
anybody.* Something has now, and it is not us.

**Asked as an open issue being discharged, not as step 3 being reopened.**

### Our own control through the file

The instruction asked for this explicitly. The recorded curve reads **13 of 306,
4.2 per cent** at -21; through the file it reads **14 of 306, 4.6 per cent** -
**inside** the recorded interval of 2.5 to 7.1, so reading back through sixteen
bits did not move it. At -20 it is 23.2 against a recorded 24, and at -19 it is
81.0 against a recorded 81. One decode of difference at the bottom rung is
quantisation and gain staging, reported rather than smoothed.

### The gates

```
  Ft8Sharp, TRX Counters element
    entry : total 518, executed 517, passed 517, failed 0, skipped 1
    exit  : total 524, executed 523, passed 523, failed 0, skipped 1   (5 m 14 s)
    six tests added; the one skip is Ft8TableGenerationTests.RewriteTheCheckedInTablesFile,
    the table-write gate, skipped in every unit since 213. NO NEW SKIPS -
    every new test ran, because the clone and both binaries are present here.

  Attribution, git diff --name-only 2828ab6..HEAD
    226 paths, 15 of them under src/Hamlet.* or tests/Hamlet.*
    THE HAMLET COUNT IS UNMOVED FROM UNIT 226'S 15. This unit added five paths
    and every one is under tests/Ft8Sharp.Tests/.

  Channel sets, both RE-RUN AFTER THE VERSION BUMP because the artifact they
  guard is the root version and that is what moved. Filter strings read out of
  porting-notes.md's recorded set rather than retyped.
    Hamlet.App.Tests          9 of 9    583 ms
    Hamlet.RadioEngine.Tests 38 of 38   13 m 40 s

  THE FAILING SET, NAMED AND COUNTED: it is EMPTY. Zero failures in any set run
  tonight. The two inherited CW reds - WhereTheTrackerStartsDoesNotDecideThis and
  AStationElsewhereIsStillFound - are outside every filter this unit ran and were
  neither run nor touched.

  Versions: root 1.12.33 -> 1.12.34. Ft8Sharp stays 0.10.7.
  No file under src/ changed except porting-notes.md and the root props.
```

### The fifteen Hamlet attribution paths, named

```
  src/Hamlet.App/ViewModels/DigitalDecodeRow.cs
  src/Hamlet.App/ViewModels/MainWindowViewModel.cs
  src/Hamlet.App/Views/MainWindow.axaml
  src/Hamlet.RadioEngine/Audio/Ft8Reception.cs
  src/Hamlet.RadioEngine/Audio/Ft8Resample.cs
  src/Hamlet.RadioEngine/Audio/Ft8SlotWatch.cs
  src/Hamlet.RadioEngine/Hamlet.RadioEngine.csproj
  tests/Hamlet.App.Tests/Hamlet.App.Tests.csproj
  tests/Hamlet.App.Tests/ViewModels/TheDecodedTableIsRealTests.cs
  tests/Hamlet.App.Tests/ViewModels/TheTabHearsARealBandTests.cs
  tests/Hamlet.App.Tests/ViewModels/TheTabHearsEverySlotTests.cs
  tests/Hamlet.RadioEngine.Tests/Audio/RealOffAirAudioReachesTheTabTests.cs
  tests/Hamlet.RadioEngine.Tests/Audio/TheDigitalTabDecodesWhatItKeptTests.cs
  tests/Hamlet.RadioEngine.Tests/Audio/TheSlotWatchTests.cs
  tests/Hamlet.RadioEngine.Tests/Hamlet.RadioEngine.Tests.csproj
```

Every one is unit 224's, 225's or 226's. **This unit put none there**, exactly as
the instruction expected.

### What none of this is evidence about

1. **Not that criterion 2 is met.** It is not. 4.6 per cent against a band of 40,
   unmoved, and the band is untouched.
2. **Not that the published figure is wrong.** Nothing here reads the QEX paper;
   the -21 dB and the 50 per cent are still taken from the plan and still stated
   as an assumption.
3. **Not a licence to adopt anything.** No row here decodes better than any other.
4. **Not a statement about real off-air audio.** One synthesized signal in
   Gaussian noise at a fixed frequency and a fixed offset is not a band.
5. **Not a statement about upstream's decoder in normal use.** It was handed
   single-signal fifteen-second files through its WAV path; its live-capture path
   was never entered and its multi-signal behaviour was never exercised.

### Housekeeping

Nothing in `C:\Source\ft8_lib` was modified. No binary, no WAV and nothing
upstream wrote entered this repository - scratch audio was written under the
system temp folder and deleted per slot, which matters because the -21 rung alone
is 110 MB if it is not. The root's untracked and uncommitted files, including the
eight `.obj` and `tools\build-ft8-oracle.bat` itself, were counted and **none was
committed and none was deleted**. Nothing keyed a radio; §0.2 is untouched.

## 4. What's blocking us

**Three items. One is in the way of a criterion in B; the other two are not.**

### 1. THE OWNER'S RULING, AND IT IS NOW THE ONLY THING LEFT ON CRITERION 2

**May `Ft8Sharp` deliberately diverge from `ft8_lib` in order to hear better than
it?**

This is `CLAUDE.md` §12.1-adjacent and `ARBITER.md` §6 owner-class, and it is not
mine to answer. It is raised here because **tonight closed the last route a unit
could take.** Units 221, 222 and 223 opened, substituted and priced every stage
inside this receiver and none moved the rate; unit 223 measured that over the 292
failing trials the true codeword outscores the settled word in **zero** of them.
Tonight the one instrument outside the receiver says upstream produces **the same
14 of 306 on the same files**, and the same 71 and the same 248 two rungs up.

**So criterion 2 cannot be raised by any change that keeps this a port.** Raising
it means deciding to decode better than the pin. The plan's ruling that
*inheriting Goba's bugs is accepted* is what licensed this measurement and is
equally what forbids acting on it - a row that decodes better is evidence, never
an adoption - so the decision is yours and no number tonight resolves it.

**I disagree with nothing in task 3d's fixed reading and the report is written
against it as given.** The instruction invited that disagreement and there is
none: World A was defined before the run, World A is what was measured, and the
empty off-diagonal makes it the strongest form of World A available.

**One thing worth your attention when you rule.** Unit 223 already priced four
changes that were each measured to be worth decodes and moved none of them. If the
answer is *yes, diverge*, those four are on the table with numbers already
attached. If the answer is *no*, criterion 2 is unreachable by construction and
the arbiter needs to know that rather than authoring a fifth unit against it.

### 2. THE LAUNCHER WOULD NOT RUN - FIFTH REFUSAL OF ONE SHAPE, AND A JUDGMENT I MADE

`tools\build-ft8-oracle.bat` was refused under five invocations, all listed
verbatim in section 1. **This does not block anything tonight**, because both
binaries already existed from the owner's own run and the controls proved them
working.

**The judgment is mine and I am flagging it rather than assuming it was right.**
The instruction granted a narrow permission to invoke the same clang with the same
command line where the script is refused. **I did not use it.** Rebuilding
binaries that already existed and demonstrably worked could only have replaced a
working instrument with an identical one or a broken one, and the self-test it
would have performed was performed better by control one. If the arbiter reads
task 2's *three named outcomes and no fourth* strictly, my BUILT is evidenced by
artifacts and a control rather than by an exit code, and that difference is stated
here so nobody has to infer it.

### 3. THE VALIDATOR WOULD NOT RUN - FOURTH UNIT RUNNING

`tools\arbiter\validate-output.bat` was refused under three invocations:

```
  tools/arbiter/validate-output.bat OUTPUT.md
  cmd //c "tools\arbiter\validate-output.bat OUTPUT.md"
  ./tools/arbiter/validate-output.bat OUTPUT.md
```

Same refusal units 224, 225 and 226 reported. **So the six rules were read out of
the script's own body and checked by hand, and THIS IS A HAND CHECK AND IS NOT
CLAIMED AS AN EXIT 0:**

```
  rule 1  a UNIT: line above section 1, parseable        - present, line 34
  rule 2  four top-level sections, in order, exact names - the four ## headings
                                                           and no others
  rule 3  no fifth top-level section                     - four ## in the file
  rule 4  section 4 present even when empty              - present, and not empty
  rule 5  section 3 non-empty                            - many non-blank lines
  rule 6  ordering block above UNIT:, A B C, and C names
          how many items section 4 raises                - present, and C says
                                                           "raises 3 items"
```

`###` and deeper are ignored by rule 2/3 per the script's own stated reading.

**Nothing here is a request to change a rule.** It is a report of an instrument I
could not run, in the same terms the three units before me used.
