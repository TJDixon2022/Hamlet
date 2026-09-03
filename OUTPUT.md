READ IN THIS ORDER

A. THE PHASE GOAL. Hamlet hears FT8 off the radio and displays the decoded text
on screen. Steps 1 to 5 are done. Step 6 is blocked on an owner ruling that was
not touched tonight. Step 7 is partial: its five criteria are evidenced against
audio this project fed itself, and its closing line was performed at the radio
on 2026-09-03 and produced nothing on screen.

B. THIS STEP AND ITS EXIT CRITERIA. Step 7's five, named, with which unit met
each. THIS UNIT AIMS AT CRITERION 3 - decodes render on screen - and at the one
segment of the path that no test has ever executed: the conversion of a capture
device's raw bytes into the floats every existing test starts from. Say whether
that segment is now asserted, and whether it was correct.

C. THIS REPORT. Section 4 raises 4 items and none of them is in the way of a
criterion in B: one asks the owner one question about the shack machine that
would settle what tonight could not, one is a ruling request about what the
operator is told when a sound card speaks a format Hamlet cannot read, one
records that the swallowing catch in the capture path leaves no trace of a
dropped buffer, and one records mismatches between the instruction and the tree.
The answer the next unit is authored from, stated here and again in section 3:
through the float path the same three off-air recordings give 47 decodes;
through a device byte buffer they give 47 for 16-bit PCM, 24-bit PCM, 32-bit
PCM, IEEE float 32, Extensible PCM 16 and Extensible PCM 32, 46 for 8-bit PCM,
and for Extensible IEEE float 32 they gave 24 before tonight's fix and 47 after.
The segment is now asserted and it was WRONG. **Whether it is what silenced the
bench check is UNKNOWN and could not be determined from here**: `SHACK_FACTS.md`
FACT-004, added to this tree during this session, says the radio is on a
different machine and that nothing measured about this machine's audio endpoints
says anything about it. The defect is real and repaired; it is neither cleared
nor implicated.

UNIT:       237 - complete at task 7 of 7 - 2026-09-03 16:39
PHASE GOAL: FT8 comes off the antenna, through Hamlet, onto the screen as text
            the operator can read.
UNIT GOAL:  Assert the one segment nobody has ever asserted - a real capture
            device's raw byte buffer becoming the mono floats the tap receives -
            by driving it with the off-air audio already known to make 194 rows,
            and repair it if it is wrong.
ADVANCED:   yes - criterion 3: a known-good off-air recording now produces all
            47 of its messages through 7 of 8 real device byte formats where no
            test had ever executed the conversion at all and an Extensible-float
            buffer produced 24; whether that repair is the silent morning's cause
            is unknown from this machine and is one question for the shack
NUMBER:     device byte formats through which a known-good off-air recording
            produces its full set of decodes: 0 -> 7 (of 8 measured; the eighth,
            8-bit PCM, gives 46 of 47)
DRIFT:      0 consecutive units without advance  (was 5)

## 1. What Claude did

**Complete, at task 7 of 7. Nothing was dropped, including the named drop
candidate.** Machine `C--Source-HamLet`, project confirmed Hamlet by all four
gate checks - `SHACK_FACTS.md` present, `CwProbabilisticDecoder.cs` present,
`CoreHMI.sln` and `MURC.sln` both absent - branch `main`.

### What was traced, built and measured

**Task 1 - the trace.** Walked the device-to-tap path naming every line that
transforms a sample, and established that no test in this repository executes
any of it. Details and line numbers in section 3.

**Task 2 - the seam and the table.** `Downmix` and `ReadSample` are reachable
from `Hamlet.RadioEngine.Tests` now. Fourteen rows built from bytes the test
writes itself, mono and stereo, the two channels always carrying different
values. Thirteen exact. The fourteenth, **32-bit float declared as
`Extensible`, was out by 0.503 of full scale**.

**Task 3 - the repair, watched failing first.** Three tests failed against the
unfixed code and seventeen pass against the fixed one. The discriminator now
reads the format's subformat rather than its top-level tag.

**Task 4 - end to end.** Karlis Goba's three busiest off-air recordings, encoded
into device byte buffers in eight formats at two rates and pushed through the
production conversion into the same `AudioTap` and the same `Ft8SlotWatch`.

**Task 5 - taken, not dropped**, and then its conclusion withdrawn. It was
droppable under its own conditions. `SHACK_FACTS.md` gained FACT-004 during this
session, which says a defect cleared or implicated by enumerating this machine's
capture devices has been measured against the wrong hardware. Section 3 and
section 4 item 1 carry the correction.

**Task 6 - the bench sheet.** Three corrections, each tagged measured or
inherited.

**Task 7 - the gates.** Four, one at a time, every failing set empty.

### The decisions this session made for itself

**1. `Downmix` became `internal static` with a `ref float[]` parameter, which is
more than the access modifier task 2 allowed.** The instruction says the seam
costs an access modifier. It does not: `Downmix` is an instance method reading
the field `_mono`, and the only constructor `WasapiAudioSource` has opens a real
capture device, so an instance is unreachable without a sound card and the whole
point of the night was a verdict that needs no hardware. The scratch array moved
from a field to a `ref` parameter and nothing else changed - the caller passes
its own `_mono`, it is still grown once and reused, and the method still
allocates nothing after the first buffer. **Reported as a decision because it is
one**, and because §7 forbids restructuring `Downmix` for readability; this was
not for readability.

**2. An unrecognised format throws rather than returning `0.0`.** Task 3 left
this to the unit under HM-DEC-009 with one binding constraint - whatever it
returns must not be able to look like quiet audio. `NotSupportedException`,
naming the format and never the device (HM-DEC-018). `OnDataAvailable` drops the
buffer, nothing reaches the tap, and unit 236's slot level writes that down as
*no level at all* - both levels `null`, zero fraction `1` - which is a reading
the operator can act on. Returning zero manufactured a stream of silence that no
reading anywhere could tell from a dead band. **The cost is that this reading is
now shared with an unplugged codec**, which is why the bench sheet gained the
line that tells them apart, and why section 4 raises the ruling.

**3. Task 4 asserts equality for every format except 8-bit PCM, which is
reported and not gated.** Eight bits quantises at 1/128 of full scale, which is
above the noise floor of a quiet band, so a weak decode can be lost in it. It
was measured at 46 against 47 and none of the three recordings matched exactly.
Gating on it would be asserting that eight-bit audio is as good as float, which
is false.

**4. Task 4 plays three recordings, not the twelve
`RealOffAirAudioReachesTheTabTests` uses.** Nine runs per recording per rate is
the cost, and the question - does a format change the answer - is answered as
well by three busy recordings. The twelve-recording run is untouched and still
in the audio gate.

## 2. What the owner should expect

**A defect in the capture path was found, measured and repaired, and whether it
is what happened to you on 2026-09-03 is one press away from being known.** It
cannot be settled from this computer - the radio is on the other one - so section
4 puts one question to you rather than guessing at the answer.

**What is now true.** Hamlet used to decide whether a sound card's bytes were
decimals or whole numbers by asking the wrong question. On a sound card
described the way Windows usually describes one, it got the wrong answer, and
every sample the radio delivered was read as the wrong kind of number - loud,
and completely unintelligible. That is fixed, it is asserted from constructed
bytes for every format a sound card can speak, and real off-air recordings now
decode identically whichever of those formats they arrive in.

**What will look wrong but is not.**

- **The version moved to 1.12.41 and you will see no difference at the radio**
  unless your sound card was one of the affected ones. Nothing was added to any
  screen; three display rulings are still with you.
- **`BENCH_CHECK.md` grew a subsection that ends by telling you nobody knows the
  answer.** That is deliberate and it is FACT-004 being obeyed. The development
  machine's capture devices are not the affected kind; your radio's codec is not
  on the development machine, so that measurement does not carry over. The
  subsection tells you the one row that settles it.
- **A sound card Hamlet cannot read now looks identical to an unplugged one** in
  the log - both levels `null`, every sample zero. That is a deliberate trade
  made under the honesty rule, and the bench sheet says how to tell them apart.
- **Test count went up by 20 in the audio set** and one of the new tests reads
  the development machine's capture device formats. It opens nothing, starts
  nothing and records nothing, and it never reads a device name. Its class
  comment now carries FACT-004 so nobody reads the radio's behaviour out of it.

**What is not claimed.** Nothing tonight bears on step 6. That measurement was
taken on synthesized samples that never went near the capture path, and a
byte-conversion fault could not have touched it.

## 3. What you should see

**No user-visible change in the application.** This unit repairs a defect on the
path between the sound card and everything else and adds no sentence to any
screen. What an operator would see is a difference only where the sound card is
described with the extensible wrapper: there, the Digital tab's table goes from
about half full to full - 24 messages to 47 on the recordings measured tonight.
**Whether the shack machine's card is one of those is the question in section
4.** On the development machine, measured, nothing changes, and under FACT-004
that fact does not carry across to the radio.

### The answer, first

**For each format a capture device can present, the conversion produced the
sample values that went in, to within one quantisation step, and the same
off-air recordings produced 47 decodes through the byte path against 47 through
the float path - except 8-bit PCM at 46, and except 32-bit float declared
`Extensible`, which produced 24 before tonight's fix and 47 after.**

NAudio is at **2.2.1** (`NAudio`, `NAudio.Core`, `NAudio.Wasapi`, all 2.2.1).

The failure text of the new test taken against the unfixed code, verbatim:

```
extensible-float32 at 1 channel(s), declared as Extensible 32-bit: the
conversion is out by 0.502937317 at frame 5 - it produced 0.496086100 where
0.999023438 went in. A device speaking this format delivers audio that is loud
and is not the band.

extensible-float32 at 2 channel(s), declared as Extensible 32-bit: the
conversion is out by 0.199218750 at frame 1 - it produced -0.011718750 where
0.187500000 went in.

AFormatNothingCanReadRefusesInsteadOfReturningQuietAudio
Assert.Throws() Failure: No exception was thrown
Expected: typeof(System.NotSupportedException)
```

### Step 7's five criteria, and which unit met each

| # | Criterion | Where it stands |
|---|---|---|
| 1 | Audio arrives in 15-second slots aligned to the quarter minute | met, unit 225 - `Ft8SlotWatch` against synthesized audio and an injectable clock |
| 2 | The clock offset is measured and shown | met, unit 228 - the readiness line names a clock that is out or unchecked |
| 3 | **Decodes render on screen** | met against this project's own audio by unit 224, strengthened by unit 226 on real off-air recordings - **and contradicted at the radio on 2026-09-03** |
| 4 | `Ft8Sharp` tests green | met, every unit; tonight 523 passed, 0 failed, 1 skipped |
| 5 | Attribution clean from `2828ab6`, channel tests green | met, every unit; tonight 254 paths, 9 of 9 and 38 of 38 |

**Is the segment now asserted, and was it correct?** It is asserted, in two
independent ways - from constructed bytes with known values, and end to end from
real off-air audio to real messages. **It was not correct.**

### Task 1 - the trace, with file and line numbers

All line numbers are in `src/Hamlet.RadioEngine/Audio/WasapiAudioSource.cs`
unless stated.

| Step of the path | Where | Transforms a sample? | Any test executes it? |
|---|---|---|---|
| `new MMDeviceEnumerator()` / `GetDevice` | `:141`, `:142` | no | **no** |
| `new WasapiCapture(endpoint)` | `:144` | no | **no** |
| `SampleRate`, `ChannelCount`, `Encoding` off `_capture.WaveFormat` | `:146`, `:152`, `:153-154` | no | **no** |
| `StartRecording` | `:206` | no | **no** |
| `OnDataAvailable` | `:262` | no - transport only | **no** |
| `Downmix` | `:296` before tonight, `:306` after | **yes** - the channel average at `:323`/`:340` | **no** |
| `ReadSample` | `:330` before, `:351` after | **yes** - every arm | **no** |
| `new AudioChunk(...)`, `SamplesReady` | `:280`, `:282` | no | **no** |
| `AudioTap.Take` and everything after it | `AudioTap.cs` | yes | **yes**, extensively - unit 225 onward |

**The expected answer held: no test reaches `Downmix` or `ReadSample`.** The
only test in the repository that names the type at all is
`tests/Hamlet.RadioEngine.Tests/Audio/AudioSeamTests.cs:150`, which reflects on
the `IsSimulated` property to assert it has no setter and executes no conversion
code.

**There is no `OpenAsync`.** The instruction names one in section 2 item 4 and
in task 1; line 144 is inside the constructor `WasapiAudioSource(AudioDevice)`
at `:132`. Reported, not repaired.

**What NAudio sets `WasapiCapture.WaveFormat` to, read out of the package.** Two
measurements, both hardware-free, both now committed tests:

- `WaveFormat.MarshalFromPtr`, given 40 bytes laid out as Windows lays out a
  `WAVEFORMATEXTENSIBLE` with the IEEE-float subformat, returns a
  `WaveFormatExtensible` whose `Encoding` is **`Extensible`** and whose
  `BitsPerSample` is 32. Printed by the test: *NAudio 2.2.1 read the operating
  system's WAVEFORMATEXTENSIBLE as WaveFormatExtensible, Encoding Extensible,
  32-bit, 2 channels.* The top-level tag is **not** normalised to `IeeeFloat`.
- `WasapiCapture`'s constructors call `MMDevice.get_AudioClient` and
  `AudioClient.get_MixFormat` - read out of the constructors' own IL with a
  proper opcode walk, not a scan for four-byte patterns. So the format handed to
  `ReadSample` is the device's mix format with its tag intact.

**Could `OnDataAvailable`'s `catch (Exception)` be swallowing a conversion
fault, and would anything record it?** Before tonight, no - the old code could
not throw. `ReadSample`'s `_ => 0.0` returned a value for everything, so a fault
was expressed as silence rather than as an exception, and the swallow was not
hiding anything because nothing was thrown. **From tonight it can be**: an
unreadable format throws, `OnDataAvailable` catches it at `:284`, and **nothing
anywhere records that it happened.** No telemetry key, no log line, no counter.
What is visible is the consequence - no chunk is delivered, so the slot level
reads *no level at all*. That is a real signal and it is not the same as being
told why. Section 4 raises it.

### The format x bits table, and which arm each takes

Measured through the production conversion. *Before* is the code as it stood at
1.12.40; *after* is 1.12.41. Error is the worst absolute error across six frames
per row, mono and stereo, the two channels always different.

| Declared | Bits | Real layout | Arm before | Arm after | Error after | Tolerance |
|---|---|---|---|---|---|---|
| `Pcm` | 8 | unsigned 8 | `8` | `8` | 0.006836 (mono), 0.003906 (stereo) | 0.0078125 |
| `Pcm` | 16 | signed 16 | `16` | `16` | 0.000000 | 3.05e-5 |
| `Pcm` | 24 | packed 24 | `24` | `24` | 0.000000 | 1.19e-7 |
| `Pcm` | 32 | signed 32 | `32` | `32` | 0.000000 | 1e-6 |
| `IeeeFloat` | 32 | float32 | float branch | float branch | 0.000000 | 1e-7 |
| **`Extensible`, subformat IEEE float** | **32** | **float32** | **`32` integer - WRONG** | **float branch** | **0.000000** | 1e-7 |
| `Extensible`, subformat PCM | 16 | signed 16 | `16` - right by luck | `16` | 0.000000 | 3.05e-5 |
| `Extensible`, subformat PCM | 32 | signed 32 | `32` - right by luck | `32` | 0.000000 | 1e-6 |
| `IeeeFloat` | 64 | float64 | `_ => 0.0` - **invented silence** | **refuses** | n/a | n/a |
| `Extensible`, any other subformat | any | unknown | integer arm - wrong | **refuses** | n/a | n/a |
| anything else (`Adpcm`, `MpegLayer3`, ...) | any | unknown | integer arm or `0.0` | **refuses** | n/a | n/a |

**The combinations that took an arm not matching their layout** are the last
four rows: extensible IEEE float 32, IEEE float 64, extensible with any other
subformat, and every non-PCM non-float top-level tag. Only the first of those is
a format Windows shared-mode capture commonly presents, and it is the one that
was silently wrong.

**The eight-bit error is quantisation and not a fault.** Five of the six test
values are exact multiples of 1/128 and come back exactly; the sixth is
deliberately near full scale and between codes at every depth.

### The fix, and the before-and-after

`ReadSample` decided a sample was floating point on
`format.Encoding == WaveFormatEncoding.IeeeFloat`. It now asks `Kind(format)`,
which reads `WaveFormatExtensible.SubFormat` against
`AudioMediaSubtypes.MEDIASUBTYPE_IEEE_FLOAT` and `MEDIASUBTYPE_PCM` where the
top-level tag is `Extensible`, and reads the top-level tag otherwise. **One
branch. `Downmix`'s structure is untouched.**

The `_ => 0.0` arm is gone, per the decision in section 1.

**Watched failing first, per task 3.** Against the unfixed code: 3 failed, 14
passed - both `extensible-float32` rows and the refusal row, with the text
quoted above. Against the fixed code: **17 of 17**.

### Task 4 - decode counts per format, byte path against float path

Three busiest off-air recordings, stereo, ten-millisecond chunks, through
`WasapiAudioSource`'s own conversion and then the same `AudioTap` and
`Ft8SlotWatch` `RealOffAirAudioReachesTheTabTests` uses. **Identical numbers at
48 000 Hz and at 44 100 Hz.**

| Format | Rows, byte path | Rows, float path | Recordings identical to the float path |
|---|---|---|---|
| float path (baseline) | - | **47** | - |
| `pcm8` | 46 | 47 | 0 of 3 |
| `pcm16` | 47 | 47 | 3 of 3 |
| `pcm24` | 47 | 47 | 3 of 3 |
| `pcm32` | 47 | 47 | 3 of 3 |
| `float32` | 47 | 47 | 3 of 3 |
| **`extensible-float32`, before the fix** | **24** | **47** | **0 of 3** |
| **`extensible-float32`, after the fix** | **47** | **47** | **3 of 3** |
| `extensible-pcm16` | 47 | 47 | 3 of 3 |
| `extensible-pcm32` | 47 | 47 | 3 of 3 |

**The sentence, with real numbers: these recordings produced 47 decodes as
floats, 24 through an Extensible-float device buffer before the fix, and 47
after.**

**And the number is 24, not 0, which matters.** The instruction predicted zero.
The reinterpretation of float bits as integers is destructive but not total -
for samples in one exponent range it is monotonic enough that some structure
survives - so **roughly half the band came through, scrambled.** On its own that
does not account for a table with nothing at all in it. It would account for a
table that filled far too slowly, or one that showed a handful of the strongest
stations.

### Task 5 - what the development machine's endpoints declare, and why that is not the radio

Taken, not dropped. It was droppable under its own conditions - task 2 gave a
definite verdict on every row including Extensible-float, without hardware - and
it was taken because it is the only thing that could say whether the fault was
this machine's. **It is not too close to `ARBITER.md` §6:** §6's owner line is
rulings that change what the project is for, its risk posture, or its cost.
Reading a declared format opens no stream, records nothing, keys nothing and
writes nothing; it is the enumeration `WasapiAudioDevices.List()` already
performs whenever the settings page opens.

**2 active capture endpoints. Both declare `IeeeFloat 32-bit`, 2 channels,
48 000 Hz, with no subformat - the top-level tag is the whole answer.** Both
take the float branch, correctly, and took it correctly before tonight too.
Endpoints are numbered, no name or id was read (HM-DEC-018).

**And that measurement says nothing whatever about the radio, which I did not
know when I took it.** `SHACK_FACTS.md` gained **FACT-004** during this session -
it was not in the tree when the unit started and it is uncommitted in the working
copy as I write. It records that there are two computers: this one, which holds
the repository and has never had a radio attached to it, and the shack machine,
where the IC-7300 is connected and where the bench check was performed. Its
third consequence is exactly this task: *no measurement of the development
machine's audio endpoints says anything about the radio ... a unit that clears or
implicates an audio-path defect by enumerating this machine's capture devices has
measured the wrong hardware and its conclusion does not stand.*

**So the correct reading of task 5 is this.** The defect is real, is measured,
and is repaired. **The two capture endpoints on the development machine declare
`IeeeFloat 32-bit`. The radio's USB codec is not one of them and what it declares
is unknown from this side.** The defect is therefore **neither cleared nor
implicated** as the cause of the silent bench check, and no session can settle
that from this tree. Section 4 turns it into one cheap question for the owner.

**An earlier draft of this report drew the inference FACT-004 forbids** - that
the silent morning is probably not explained by this defect, because this
machine's endpoints are unaffected. That inference is withdrawn. The
`BENCH_CHECK.md` line and the `Directory.Build.props` version note that carried
it have been corrected in the same commit as this correction.

### `BENCH_CHECK.md` - the lines changed

| Line | Change | Measured or inherited |
|---|---|---|
| *Both levels are `null`...* bullet | gains a fourth meaning - a format Hamlet cannot read is now refused, so nothing reaches the tab and the slot reads exactly like an unplugged codec | **measured tonight** |
| *The levels are numbers and the zero fraction is tiny...* bullet | says that before 1.12.41 *the problem is downstream of the sound card* was false, because audio could arrive, be loud, and have been read wrongly | **measured tonight** |
| new subsection *What the sound card said it was speaking* | the `encoding` row of a capture sheet as a lookup table, the 47/24/47 numbers, and the plain statement that nobody knows whether this was his fault until he looks at that row on the shack machine | **measured tonight** for the table and the numbers; the two-machine limit is **FACT-004**, read from `SHACK_FACTS.md` and not measured here |

No new claim goes on a screen. The sheet is a file he opens deliberately.

### The gates

Every count read from `ResultSummary.Counters` in a TRX logger, never a console
count. One invocation at a time, never two at once.

**Gates 2, 3a and 3b were run twice, and the figures below are the second run.**
FACT-004 arrived after the first pass and the corrections it forced touched
`Directory.Build.props`, `BENCH_CHECK.md` and one test file's class comment - and
the channel set is by definition the classes that open `Directory.Build.props` at
run time. A gate run against a tree that was then edited has not gated the tree.
Both runs agree exactly on every count. Gate 1 was not re-run: nothing under
`src/Ft8Sharp/` was touched in either pass.

| # | Gate | Result | Failing set |
|---|---|---|---|
| 1 | `Ft8Sharp.Tests`, whole project | **523 passed, 0 failed, 1 skipped**, 524 total, 5 m 19 s - exactly the expected figure, so nothing under `src/Ft8Sharp/` moved | **empty** |
| 2 | `Hamlet.RadioEngine.Tests`, filter `~Hamlet.RadioEngine.Tests.Audio` | **117 passed, 0 failed, 0 skipped**, 59 s and 58 s. This is where tonight's change lives. 20 of the 117 are new. | **empty** |
| 3a | `Hamlet.App.Tests`, channel filter | **9 passed, 0 failed**, 459 ms and 551 ms - `DecisionLogOrderTests`, `VersionTests`, `EveryResourceKeyResolvesTests`, `ViewTestsActThroughControlsTests`, green at the new version | **empty** |
| 3b | `Hamlet.RadioEngine.Tests`, channel filter | **38 passed, 0 failed**, 13 m 43 s and 13 m 51 s - the expected 38 of 38. Its own record says 7 m 38 s; it took nearly twice that here on both runs, which is a machine fact and not a result | **empty** |
| 4 | Attribution, `git diff --name-only 2828ab6..HEAD` | 254 paths, 41 under `src/Hamlet.*` or `tests/Hamlet.*` | n/a |

**No test skipped for want of the pinned clone.** `C:\Source\ft8_lib` is on this
machine, so every `[RequiresOffAirRecordingsFact]` ran rather than skipping.

**Attribution, and the reduction is not claimed.** 254 paths changed since
`2828ab6`, 41 of them under Hamlet's own folders. **The attribution reduction
does not apply to step 7 and is not claimed here** - step 7 is by construction
the step that reaches Hamlet's code. The honest substitute, per unit 225: the
Hamlet paths this unit added or touched are

- `src/Hamlet.RadioEngine/Audio/WasapiAudioSource.cs`
- `tests/Hamlet.RadioEngine.Tests/Audio/DeviceBytesBecomeTheFloatsTheTapSeesTests.cs`
- `tests/Hamlet.RadioEngine.Tests/Audio/OffAirAudioThroughADeviceByteBufferTests.cs`
- `tests/Hamlet.RadioEngine.Tests/Audio/WhatThisMachinesCaptureEndpointsDeclareTests.cs`
- `tests/Hamlet.RadioEngine.Tests/Audio/RealOffAirAudioReachesTheTabTests.cs`

and outside them, `Directory.Build.props`, `BENCH_CHECK.md`, `PROJECT_STATUS.md`,
`PHASE_STATUS.md` and this file. **Gate 2 runs the tests over all of that
changed code.**

**`Hamlet.App.Tests` invocations: 2**, gate 3a and its re-run after FACT-004
forced an edit to `Directory.Build.props`. The channel filter keeps both out of
the `Views` namespace where HM-OPEN-069's stall lives; they returned in 459 ms
and 551 ms. No other route in this unit touched that project. **The second
invocation was a choice**: reporting a gate that ran against a superseded tree
would have been the cheaper answer and the wrong one.

### Versions

Root `1.12.40` -> **`1.12.41`** under HM-DEC-150, a patch because this is a
defect repair and not a new capability. `Ft8Sharp` **stays at `0.10.7`** under
HM-DEC-152 - no file under `src/Ft8Sharp/` changed.

### Drift, and whether the silent morning now has a mechanical explanation

**Tonight produces a real candidate for it and cannot confirm it, and I spent the
night inside the capture path, so this judgment is worth having on the record.**
The header reads `DRIFT: 0 (was 5)` because the measurement met the rule the
instruction set for an advance, and the last two units read 4 and 5 because step
7's remaining work was judged to be the owner's at the radio. **Tonight does not
overturn that judgment.** A real defect was found on the one unasserted segment,
and on a sound card described the way Windows usually describes one it would
have cost roughly half the decodes - a table that filled far too slowly, or one
showing a handful of the strongest stations. Whether the radio's codec is such a
card is **not knowable from this machine**, under `SHACK_FACTS.md` FACT-004, and
I will not guess it. So the honest position is: **the silent morning now has a
plausible mechanical explanation for the first time, and confirming or killing it
costs the owner one glance at one row on the shack machine.** Two things follow
for whoever authors next. First, the path from the antenna to the tap is measured
end to end rather than taken on trust - that segment is retired as *unasserted*,
and it was the last one. Second, everything still open is on the other computer:
which device Hamlet opened that morning, what format it declared, and what the
shack machine's own log says. **I do not think another unit of code on this
machine is the next thing this phase needs** - the instruments exist and the
measurement is now on the wrong side of a USB cable from this session. That does
not set the next subject; the arbiter reasons from the plan.

### The validator, and the push

`tools\arbiter\validate-output.bat "C:\Source\HamLet\output.md"` - **exit 0,
VALID, all six rules passed**: the `UNIT:` line, the four sections in order, no
fifth, section 4 present, section 3 non-empty at 228 lines, and the ordering
block with its count. Unit 236 measured that the path must be in double quotes;
**the script path must be too.** Bash eats a lone backslash, so
`tools\arbiter\validate-output.bat` runs as `toolsarbitervalidate-output.bat` and
exits 127 - which is what put the file of that name in the root debris. The
spelling that runs is `"tools\arbiter\validate-output.bat"`, both halves quoted.

**Pushed `997963e..6c5feeb` on `main`, seven commits, one per task**, then
`6c5feeb..4b77bc2` carrying the first draft of this report, then
`4b77bc2..ec64834` carrying the correction FACT-004 forced.

**And then the check found that this file was in none of them.** `git status`
after the third push still read `M OUTPUT.md`. **Git tracks this path as
`OUTPUT.md`, uppercase**, and this session wrote and staged `output.md` - which
on Windows is the same file on disk and a different path to git, so `git add
output.md` matched nothing and both report commits carried everything except the
report. A tenth commit adds it under the tracked spelling. **The full range is
`997963e..HEAD` on `main`, and the range is quoted here rather than assumed
because that check is the only reason this was caught** (`CLAUDE_CODE.md` §11).

## 4. What's blocking us

Four items. **None of them is in the way of a criterion in B.**

### 1. One question is asked of the owner, and it is cheap: what does the shack machine's sound card say it is speaking?

**The question.** On the **shack machine** - the one with the IC-7300 on it -
open Hamlet on the Digital tab, press *keep the last 30 seconds*, and open the
`.txt` file that appears beside the WAV in
`%AppData%\Hamlet\captures\digital\`. **What does its `encoding` row say?**

- If it says **`Extensible 32-bit`**, this unit found the cause of the silent
  morning and 1.12.41 fixes it.
- If it says **`IeeeFloat 32-bit`** or any `Pcm` value, this unit found a real
  defect that was not your defect, and the empty table has another cause.

**Why it is asked rather than measured.** `SHACK_FACTS.md` FACT-004, added to
this tree during this session, says no measurement of this machine's audio
endpoints says anything about the radio, and that a unit clearing or implicating
an audio-path defect by enumerating them has measured the wrong hardware. That
is precisely what task 5 did, so its conclusion is withdrawn and the question is
put to the only person who can answer it. **It is one press and one file.**

**What was rejected and why.** Inferring it from this machine's two endpoints -
forbidden by FACT-004, and it is the mistake FACT-004 was written to stop.
Reading device names to work out whether the codec is present - HM-DEC-018.
Guessing - `CLAUDE.md` §0.0.

**Not blocking.** The repair stands on its own measurement either way, and step
7's criterion 3 does not wait on the answer.

### 2. A ruling is asked for: what does Hamlet tell the operator when a sound card speaks a format it cannot read?

**The ruling wanted.** A capture device presenting a format `ReadSample` cannot
read now produces nothing at all on the tab, and the log reads exactly as an
unplugged codec reads - both levels `null`, every sample zero. **Should Hamlet
say, somewhere the operator will see it, that the sound card is speaking
something it does not understand?**

**The reasoning.** HM-DEC-009 made returning `0.0` unacceptable, and refusing is
the honest alternative, but refusal costs the operator the distinction between
*nothing is plugged in* and *this device speaks something I cannot read*. Those
have different fixes. The sheet now carries the distinction, but the sheet
requires him to press *keep the last 30 seconds* first.

**What was rejected and why.** Adding a branch to `DigitalReadiness`, or a
sentence to the status bar - `CLAUDE.md` §12.1 makes what Hamlet asserts to the
operator the owner's, and three display rulings from units 227, 233 and 236 are
already in front of him unanswered. Authoring a fourth around them would be
exactly the drift §12.1 exists to stop. Adding a telemetry key - §7 of tonight's
instruction forbids it and units 233 to 236 already built the record.

**This is genuinely a fourth display question and it should probably be answered
together with the other three.** It is not blocking.

### 3. Recorded, not asked: the swallowing catch leaves no trace

`OnDataAvailable`'s `catch (Exception)` at `WasapiAudioSource.cs:284` drops the
buffer and records nothing anywhere - no telemetry line, no counter, no log.
Before tonight nothing could throw there, so it was hiding nothing. From
tonight an unreadable format throws every buffer, and the only evidence is the
absence of audio. **Recording it would need a telemetry key, which this unit was
forbidden to add**, so it is written down here rather than built. It is coupled
to item 2 and would be settled by the same answer.

### 4. Recorded, not asked: mismatches between the instruction and the tree, and the inherited debris

Per §5 of the instruction - reported, not repaired.

- **`OpenAsync` does not exist.** Section 2 item 4 and task 1 both name it. Line
  144 is inside the constructor at `:132`. Everything else in section 2 checked
  out exactly, including `git grep Extensible` under `src/` returning nothing.
- **The seam does not cost only an access modifier.** Section 1's last paragraph
  says it does. `Downmix` is an instance method on a class whose only
  constructor opens a device. Handled as a named decision in section 1.
- **`AudioMediaSubtypes` is in `NAudio.Dmo`, not `NAudio.CoreAudioApi`**, which
  cost one compile. Noted for the next unit that reaches for it.
- **The instruction's task 5 could not have produced the evidence it was written
  to produce.** Its drop conditions turn on whether task 2 settled the Extensible
  row, but under FACT-004 the corroboration it describes - what a real device
  presents - is not available on this machine at all. That is not the arbiter's
  error: FACT-004 did not exist when the instruction was written. It is recorded
  so the next instruction does not ask for it again.
- **`SHACK_FACTS.md` is modified and uncommitted in the working tree**, holding
  FACT-004. I read it and obeyed it; I did not commit it, because it is the
  owner's file and staging somebody else's uncommitted work is not mine to do.
  **It should be committed.**
- **The report file is tracked as `OUTPUT.md` and every session writes
  `output.md`.** On Windows those are one file on disk and two paths to git, so
  `git add output.md` stages nothing and the report is silently left out of the
  commit. This unit caught it only by reading `git status` after pushing. **It
  has been happening for at least one unit before this one**: `OUTPUT.md`'s last
  commit is `8745b11`, unit 234, and it was already dirty in the working tree
  when this unit started - so unit 236's report, and possibly 235's, exist on
  disk and are not in the repository. This unit's is committed under the tracked
  spelling. **Recorded rather than repaired**: renaming the tracked path is a
  change to the loop's own plumbing and every future session's habit, which is
  not a unit's call to make on its way out of the door.

And the inherited items the instruction named as known and not mine, all still
present and all untouched: `PHASE_OUTCOME.md`'s header disagreeing with its
entries on steps 1 and 3; `PROJECT_STATUS.md` and `CLAUDE.md` §1 disagreeing on
the ruling id; and the uncommitted root debris, which stands at 25 items
including eight `.obj` files and several scratch scripts. **One more for the
list**: `Directory.Build.props` has version-log entries up to `1.12.36` and none
for `.37` through `.40`; tonight's `1.12.41` has one, following the file's own
convention, and the four missing entries were left alone.
