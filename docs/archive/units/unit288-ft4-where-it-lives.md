# Unit 288 - where FT4 lives, decided by reading

Work instruction 288. **Reading only. This unit changes no code at all.**

The question it answers, ruled by Tim on 2026-09-08: *if `ft8_lib` carries FT4, the
port carries FT4; if it does not, FT4 is new work and belongs in `Ft8Sharp.Deep` or a
sibling.* Everything below is read from the tree, with file and line, and nothing in
it is recalled.

---

## Task 1 - what upstream actually carries

### Where the source is

`tools/build-ft8-oracle.bat` line 46 sets `CLONE=C:\Source\ft8_lib`, and that clone
**exists**. It is outside this repository and is not committed, exactly as that
script's own header says. Its `HEAD` reads

```
9fec6ca39886edbf96f4f5e71edc76da5074e871
Sat Aug 23 21:55:25 2025 -0700
non-standard callsigns; special CQ; field type annotation (#47)
```

which is the commit `src/Ft8Sharp/porting-notes.md` line 14 pins the port to. The
clone the oracle builds from and the clone the port was read from are the same clone
at the same revision, so nothing below is a claim about a different upstream.

Both oracle binaries are present and dated 2026-09-02: `C:\Source\ft8_lib\build\gen_ft8.exe`
and `C:\Source\ft8_lib\build\decode_ft8.exe`.

### Does it carry FT4? Yes, encoder and decoder both, and neither is a stub

There is no file called `ft4.c` anywhere in the clone, so the filename test the
instruction warns against would have answered this **wrongly, in the safe direction**.
FT4 lives inside the same files as FT8, behind a protocol enum:

```c
typedef enum
{
    FTX_PROTOCOL_FT4,
    FTX_PROTOCOL_FT8
} ftx_protocol_t;
```
`ft8/constants.h:52-56`

The `FTX_` prefix upstream uses for what the two modes share is the same prefix
`porting-notes.md:309` already records as marking shared ground.

**Encoder.** `ft8/encode.c:127`, `void ft4_encode(const uint8_t* payload, uint8_t* tones)`,
declared at `ft8/encode.h:35`. It is complete: it applies the FT4 pseudorandom XOR to
the 77-bit payload (`encode.c:132-137`), adds the CRC-14 (`encode.c:141`), runs the
same `encode174` LDPC generator FT8 uses (`encode.c:144`), and lays 105 tones out over
the four sync groups and three data blocks (`encode.c:151-193`).

**Decoder.** Four protocol branches, all of them real work:

| What | Where |
|---|---|
| `ft4_sync_score` - scores the four Costas groups | `ft8/decode.c:127-188` |
| Candidate search picks the sync function and the tone count by protocol | `ft8/decode.c:192-193` |
| `ft4_extract_likelihood` - 87 data symbols, 2 bits each | `ft8/decode.c:253-277` |
| `ft4_extract_symbol` - 4 magnitude bins to 2 log-likelihoods | `ft8/decode.c:454-466` |
| `ftx_decode_candidate` dispatches on protocol, and un-XORs the FT4 payload after the CRC check | `ft8/decode.c:330-338`, `369-380` |

**Console apps.** `demo/gen_ft8.c:130` takes `-ft4` as its fourth argument and
`demo/decode_ft8.c:250-253` takes `-ft4` as an option. `README.md:1` reads
"FT8 (and now FT4) library" and `README.md:21` says encoding and decoding work for
both.

### The parameters, as upstream states them

| Parameter | Value | Where |
|---|---|---|
| Slot length | 7.5 s | `ft8/constants.h:15`, `FT4_SLOT_TIME (7.5f)` |
| Symbol period | 0.048 s | `ft8/constants.h:14`, `FT4_SYMBOL_PERIOD (0.048f)` |
| Tones | 4 | `ft8/decode.c:193`, and the Gray map is 4 entries |
| Total channel symbols | 105 | `ft8/constants.h:36`, `FT4_NN` |
| Data symbols | 87, 2 bits each | `ft8/constants.h:34`, `FT4_ND` |
| Ramp symbols | 2, one at each end | `ft8/constants.h:35`, `FT4_NR`; placed at tones 0 and 104, `encode.c:153-156` |
| Sync groups | 4 groups of 4, offset 33 | `ft8/constants.h:37-39` |
| Sync pattern | four *different* 4-symbol Costas arrays | `ft8/constants.c:5-10` |
| Gray map | `{ 0, 1, 3, 2 }` | `ft8/constants.c:14` |
| Payload XOR | 10 bytes, `4A 5E 89 B4 B0 8A 79 55 BE 28` | `ft8/constants.c:16-27` |
| Symbol smoothing BT | 1.0 for FT4, 2.0 for FT8 | `demo/gen_ft8.c:17`, used at `gen_ft8.c:151` |

The four sync arrays are

```
{ 0, 1, 3, 2 }
{ 1, 0, 2, 3 }
{ 2, 3, 1, 0 }
{ 3, 2, 0, 1 }
```
`ft8/constants.c:5-10`

and they sit at tone indices 1-4, 34-37, 67-70 and 100-103 (`encode.c:157-172`), which
is the `R Sa D29 Sb D29 Sc D29 Sd R` structure `constants.h:29-32` draws in its comment.

### Shared or separate: overwhelmingly shared

Separate for FT4, and only these: the sync pattern, the Gray map, the XOR sequence,
`ft4_encode`, `ft4_sync_score`, `ft4_extract_likelihood`, `ft4_extract_symbol`, and the
five symbol-count constants.

**Shared, byte for byte, with no protocol branch in them at all** - `grep -rn -i ft4`
over the clone returns no hit in any of these files:

- `ft8/message.c` and `ft8/message.h` - the whole 77-bit message layer, every field
  type, callsign packing, hashing, non-standard calls, grids, reports.
- `ft8/ldpc.c` and `ft8/ldpc.h` - `encode174`, `bp_decode`, `ldpc_decode`, the
  (174,91) code.
- `ft8/crc.c` and `ft8/crc.h` - CRC-14, whose header comments say "FT8/FT4" in three
  places (`crc.h:12`, `crc.h:17`, `crc.h:22`).
- `ft8/text.c`, `ft8/text.h`.
- The LDPC generator, `Nm` and `Mn` tables in `ft8/constants.c`, all `FTX_`-prefixed.

The two places outside `ft8/` that do branch are the monitor's DSP sizing
(`common/monitor.c:57-58`, which picks slot time and symbol period) and the two demo
programs' argument handling.

### One mismatch against the work instruction, reported and not repaired

The instruction says FT4 is "7.5-second slots, four tones, **4.48 seconds of
transmission**". Upstream's own constants do not produce 4.48 s: 105 symbols at
`FT4_SYMBOL_PERIOD` 0.048 s is **5.04 s**, and `demo/gen_ft8.c:171-174` computes the
signal length from exactly that product, padding the remainder of the 7.5 s slot with
silence at both ends. A 0.048 s symbol also puts the tone spacing at 20.833 Hz rather
than the 23.4375 Hz that a 4.48 s / 105-symbol transmission implies.

**This unit does not say which figure is right**, because settling it needs the
published description - the QEX paper `README.md:47` cites - and no copy of it is in
this tree or the clone. What is certain is that these two numbers cannot both be true,
and that the number the port would inherit is upstream's 0.048 s. It is raised in
`output.md` section 4 as a thing to check against the citation before any FT4 audio is
synthesized or cut, because a slot cutter built to 4.48 s and a decoder built to 5.04 s
would disagree by more than half a second and every symptom of that lands on the
decoder.

---

## Task 2 - what `Ft8Sharp` already shares

`src/Ft8Sharp` is **33 C# files, 7,926 lines** excluding `obj/` and `bin/`. Of those,
**five files and 1,766 lines carry an assumption FT4 breaks**. The other **28 files and
6,160 lines are protocol-neutral today** and would be used by FT4 unchanged - not
adaptable, unchanged, because nothing in them mentions a tone count, a symbol count, a
slot length or a Costas array at all.

That is measured rather than estimated: `grep -rn` for `Costas`, `GrayMap`, `= 79`,
`= 58`, `ToneCount`, `SymbolCount`, `0.160`, `15.0`, `SymbolPeriod` and `SlotSeconds`
over `src/Ft8Sharp/**/*.cs` returns hits in exactly five files.

### Used unchanged - the whole message layer, the code, and the checksum

| What | Where | Note |
|---|---|---|
| 77-bit payload container | `Message/Ft8Payload.cs` (216) | FT4 and FT8 carry the identical 77 bits; `ft8/message.c` has no `ft4` hit at all |
| Standard messages | `Message/Ft8StandardMessage.cs` (294) | |
| Non-standard messages | `Message/Ft8NonstandardMessage.cs` (460) | the `58` in it is the 58-bit callsign field, not FT8's data-symbol count |
| Callsign field, hash, cache | `Message/Ft8CallsignField.cs` (672), `Ft8CallsignHash.cs` (206), `Ft8CallsignCache.cs` (382) | |
| Grid field | `Message/Ft8GridField.cs` (273) | |
| Free text and telemetry | `Message/Ft8FreeText.cs` (274) | |
| Message type dispatch and decode | `Message/Ft8MessageTypes.cs` (167), `Ft8MessageDecoder.cs` (193) | |
| Character tables | `Message/Ft8Text.cs` (356) | |
| **CRC-14** | `Message/Crc14.cs` (131) | upstream's own header calls it the FT8/FT4 CRC polynomial, `ft8/crc.h:12` |
| **LDPC(174,91) encode** | `Ldpc/LdpcEncoder.cs` (175) | `ft4_encode` calls the same `encode174`, `ft8/encode.c:144` |
| **LDPC belief propagation** | `Ldpc/LdpcDecoder.cs` (355), `LdpcDecodeResult.cs` (62) | `ftx_decode_candidate` runs one `bp_decode` for both protocols, `ft8/decode.c:342` |
| Codeword gate | `Ldpc/Ft8CodewordDecoder.cs` (206) | see the caveat below |
| FFT | `Dsp/Ft8Fft.cs` (336), `Ft8RealFft.cs` (182) | |
| Waterfall store | `Dsp/Ft8Waterfall.cs` (120) | shaped entirely by the geometry handed to it |
| Spectrogram build | `Dsp/Ft8Monitor.cs` (234) | every extent comes from `Geometry`, `Ft8Monitor.cs:48-70` |
| Candidate record | `Dsp/Ft8Candidate.cs` (108) | |
| Slot orchestration | `Dsp/Ft8SlotDecoder.cs` (292) | see the caveat below |
| LDPC generator, `Nm`, `Mn`, `NumRows` | `Tables/Ft8Tables.g.cs:76-443` | all `FTX_`-prefixed upstream, which is upstream's own mark for what the two modes share |

**One caveat on the last two.** `Ft8CodewordDecoder.Decode` and `Ft8SlotDecoder` are
protocol-neutral in their arithmetic and not yet in their interface:
`Ft8CodewordDecoder` takes 174 ratios and knows nothing about tones, but it does not
apply FT4's payload XOR, which upstream does inside `ftx_decode_candidate`
(`decode.c:369-380`) - after the CRC check and before the payload is handed on.
Whoever adds FT4 must put that XOR somewhere, and `Ft8CodewordDecoder` is where
upstream puts it.

### Could not be used - five files, each with the assumption named

| File | Lines | The assumption |
|---|---|---|
| `Encode/Ft8SymbolEncoder.cs` | 289 | `SymbolCount = 79` (`:58`), `DataSymbolCount = 58` (`:61`), `SyncBlockLength = 7` (`:64`), `SyncBlockCount = 3` (`:67`), `SyncBlockOffset = 36` (`:79`), `BitsPerSymbol = 3` (`:82`), `ToneCount = 1 << BitsPerSymbol` (`:88`). Every one of the seven differs for FT4, and there is no ramp symbol and no payload XOR anywhere in it. |
| `Encode/Ft8Waveform.cs` | 451 | `SymbolPeriodSeconds = 0.160f` (`:66`), `SlotSeconds = 15.0f` (`:69`), `ToneSpacingHz = 1/0.160` (`:72`), `SymbolCount` taken from the encoder (`:53`). Its GFSK shaping is written for FT8's BT of 2.0, where upstream uses 1.0 for FT4 (`demo/gen_ft8.c:17`). |
| `Dsp/Ft8WaterfallGeometry.cs` | 284 | `SymbolPeriodSeconds = 0.160f` (`:49`), `SlotSeconds = 15.0f` (`:52`). Block size, bin extents, `MaxBlocks`, tone spacing and both the frequency and the time mapping are computed from those two (`:137`, `:169-171`, `:234`, `:248`, `:264`, `:274`). **This file also carries a deliberate float-truncation match to upstream** (`:13-21`) which is `0.160f` specifically, so an FT4 geometry is not this class with a different number handed in - that truncation has to be re-derived at 0.048f. |
| `Dsp/Ft8SyncSearch.cs` | 376 | `ToneCount = 8` (`:62`), the seven-symbol sync group (`:52`), and the whole score written against `Ft8Tables.Ft8CostasPattern` (`:306`) as one pattern repeated three times. FT4 has **four different** patterns, one per group (`ft8/constants.c:5-10`). That is a structural difference and not a constant. |
| `Dsp/Ft8SoftSymbols.cs` | 366 | `Ft8SymbolEncoder.ToneCount` and `SymbolCount` throughout (`:134`, `:158`, `:162`, `:184`, `:215`, `:237`), three bits per symbol, and the 7-then-14 sync skip. FT4's skip is 5-then-9-then-13 (`ft8/decode.c:261`). |

### The three FT4 tables, and the one decision that stops them existing

`src/Ft8Sharp/Tables/Ft8Tables.g.cs` is machine-generated from
`C:\Source\ft8_lib\ft8\constants.c` by `Ft8Sharp.Tests.TableGen.Ft8TableConverter`, at
the same pin. **The converter already reads the file the FT4 tables are in and
deliberately steps over them**, and says so in the generated header and in its own
remarks:

> The three FT4-only tables in the same source -- `kFT4_Costas_pattern`,
> `kFT4_Gray_map` and `kFT4_XOR_sequence` -- are deliberately not converted. FT4 is
> parked, and an unused table in a published library is a liability.

`Ft8Tables.g.cs:21-24`, written by `Ft8TableConverter.cs:226-227`, with the decision
recorded at `Ft8TableConverter.cs:42-44`.

So the tables are not a transcription job. `Ft8TableConverter.Manifest`
(`Ft8TableConverter.cs:67-99`) holds six `TableSpec` entries; FT4 costs **three more
entries** and a regeneration, and the parser already handles a two-dimensional
initialiser because `kFTX_LDPC_generator` is `[83][12]`. Nothing is retyped by hand and
`Ft8TableGenerationTests.CheckedInTablesAreWhatTheConverterProduces` keeps proving it.

### The port is closed, and that is measured rather than recalled

`Ft8CodewordResult`'s only constructor is `private` (`Ldpc/Ft8CodewordDecoder.cs:176`)
and its three factories are `internal` (`:198`, `:201`, `:204`). No `InternalsVisibleTo`
names `Ft8Sharp.Deep` - `grep -rn InternalsVisibleTo src/` finds them only on
`Hamlet.App` and `Hamlet.RadioEngine`, each pointing at its own test project. Unit 245's
finding holds exactly as stated.

`Ft8Sharp.Deep` (0.8.0, GPL-3.0) works around that by **reproducing the port's
per-candidate loop through public members** rather than by opening anything -
`Ft8DeepSlotDecoder.cs:10-42` states this and names it route A of
`docs/unit245-deep-seam.md` section 4. It has exactly one `ProjectReference`, to
`..\Ft8Sharp\Ft8Sharp.csproj` (`Ft8Sharp.Deep.csproj:30`).

### The 51 fidelity tests, identified

They are `Ft8SymbolBitIdentityTests.EverySymbolOfEveryMessageIsIdenticalToUpstreams`
(`tests/Ft8Sharp.Tests/Encode/Ft8SymbolBitIdentityTests.cs:41-42`), running over
`EncodeCorpus.Build()`. The corpus holds 56 entries of which **51 have a text form**,
and it is those 51 that go to `Ft8Oracle.Generate` and come back as tones compared
symbol for symbol; `docs/unit254-transmit-survey.md:309-330` counts them from source
and says in as many words that this is the 51 the phrase refers to.

`Ft8Oracle.Generate` invokes `build\gen_ft8.exe` with `(messageText, wavPath)` and no
protocol flag (`Ft8Oracle.cs:99-107`), so it generates FT8 today. Upstream takes `-ft4`
as the **fourth positional argument** (`demo/gen_ft8.c:130`), which means the oracle
wrapper needs one optional argument and not a second wrapper.

---

## Task 3 - the decision, with its reason

### The decision: FT4 goes in the port, `src/Ft8Sharp`

The rule Tim ruled on 2026-09-08 is conditional and task 1 satisfied the condition.
`ft8_lib` carries FT4 - `ft4_encode` at `ft8/encode.c:127`, four decoder branches at
`ft8/decode.c:127`, `192`, `253` and `454`, three FT4 tables at `ft8/constants.c:5-27`,
and `-ft4` on both console programs. So **the port carries FT4 and the fidelity tests
extend to cover it.** The rule decided this; this unit only read.

Three things follow that are worth writing down, because each of them would have been
an argument if the reading had gone the other way.

**The licence story does not move.** `ft8_lib` is MIT (`C:\Source\ft8_lib\LICENSE:1`,
"MIT License, Copyright (c) 2018 Kārlis Goba") and `Ft8Sharp` is MIT because of it.
FT4 in the port is a port of MIT code into an MIT library. FT4 in `Ft8Sharp.Deep` would
have been GPL-3.0 code re-deriving something that already exists under MIT, which is
work done twice and licensed worse.

**The instrument exists only because upstream carries FT4.** The reason `Ft8Sharp` is
worth its constraint is that `gen_ft8.exe` will say what upstream's tones are for any
message, and 51 of 51 already match symbol for symbol. That binary takes `-ft4`, and
**it was run in this unit rather than assumed to work** - see the measurement below.
A sibling implementation of FT4 would have no such reference at all: it would be proved
against itself.

**A sibling would have had to rewrite the five files anyway.** Task 2 named them. Not
one of the five is reachable for extension from outside: `Ft8SymbolEncoder`'s seven
constants are `public const`, but `Ft8SyncSearch`, `Ft8SoftSymbols` and
`Ft8WaterfallGeometry` compute from them internally, and `Ft8CodewordResult` cannot be
constructed outside the assembly at all (`Ft8CodewordDecoder.cs:176`, `:198-204`). A
sibling would reproduce the port's loop the way `Ft8DeepSlotDecoder` already does, then
reimplement five files, and end with no upstream reference to hold them to.

**So: nothing in this decision needs the port to open.** That answers task 3's "what
has to open, minimally" - the question does not arise, and it is worth saying so
plainly, because the alternative was where the opening would have been forced.

### Measured, not assumed: upstream's FT4 round-trips, and it took a shift to see it

Both oracle binaries were already built at `C:\Source\ft8_lib\build\` and dated
2026-09-02, so nothing was compiled in this unit.

```
gen_ft8.exe "CQ KC3QIS FN00" ft4probe.wav 1000 -ft4
  -> Packed data: 00 00 00 24 b1 97 80 0a 0f 08
  -> FSK tones: 105 of them, first 0 and last 0, which are the ramp symbols
  -> a 7.500 s WAV, 90 000 frames at 12 000 Hz

decode_ft8.exe -ft4 ft4probe.wav
  -> Block size = 576, N_FFT = 1152        (576 = 12000 x 0.048, so the flag is read)
  -> DECODED 0 MESSAGES
```

**Upstream's own FT4 decoder read nothing from upstream's own FT4 generator**, which
looks at first like FT4 support being hollow after all. It is not, and the cause is
placement rather than protocol.

`ftx_find_candidates` searches `candidate.time_offset` from **-10 to 19 blocks**
(`ft8/decode.c:205`). A block is one symbol period, so for FT8 that window is -1.6 s to
+3.0 s and for FT4 it is **-0.48 s to +0.91 s**. `demo/gen_ft8.c:173` centres the
transmission in the slot, which for FT4 puts its first symbol at 14 760 samples, or
**1.23 s** - outside the window by a third of a second.

Shifting the identical audio forward by 0.99 s and zero-padding the tail, changing no
sample value:

```
decode_ft8.exe -ft4 ft4shift.wav
  -> Decoded 1 messages, callsign hashtable size 1
  -> 000000 +19.5 +0.29 1000 ~  CQ KC3QIS FN00
```

The message comes back as itself. The FT8 control does the same thing without a shift
(`+16.5 +1.36 1000 ~ CQ KC3QIS FN00`), because FT8's window is four times wider in
seconds and its centred placement lands inside it.

**Two things follow for the first build unit**, and both are the kind of thing that
otherwise gets found by an evening of an FT4 tab showing nothing:

1. **Upstream's FT4 encoder and decoder agree**, over the message layer, the CRC-14,
   the LDPC code, the four Costas groups, the Gray map, the payload XOR and the ramp
   symbols. That is the whole chain, proved end to end, in this unit.
2. **The candidate search window is a quarter as wide in seconds for FT4**, so whatever
   cuts an FT4 slot must place the transmission within about 0.9 s of the slot start.
   Hamlet's `Ft8SlotCutter` cuts on a measured offset for FT8; whether that offset
   satisfies a window this narrow is a measurement the first build unit owes, not an
   assumption it may make.

### What the 51 fidelity tests become

They stay exactly as they are and gain a second protocol beside them.

- **The corpus does not change at all.** `EncodeCorpus.Build()` is a message-layer
  corpus and the message layer is shared, so all 51 texts with a text form are valid
  FT4 messages unchanged. No new messages are invented and no message is dropped.
- **`Ft8Oracle.Generate` gains one optional protocol argument**, appended as
  `gen_ft8`'s fourth positional (`demo/gen_ft8.c:130`). One wrapper, not two.
- **`EverySymbolOfEveryMessageIsIdenticalToUpstreams` gains a protocol dimension**:
  51 comparisons at 79 symbols and 51 at 105, so **102 rather than 51**. The FT8
  assertion is left untouched rather than generalised, so a regression on the FT8 side
  stays attributable to the FT8 side.
- **Two things get compared that FT8 has no analogue for**: the payload XOR
  (`ft8/constants.c:16-27`, applied at `encode.c:132-137`) and the two ramp symbols. A
  symbol-for-symbol comparison covers both without naming them, which is the point of
  comparing symbols rather than intermediate values.
- **`Ft8TableConverter.Manifest` grows by three entries** and
  `CheckedInTablesAreWhatTheConverterProduces` then covers the FT4 tables for free. The
  converter's own remark that the three are deliberately skipped
  (`Ft8TableConverter.cs:42-44`) is what is being reversed, and it should be reversed
  in words as well as in code.

### How upstream parity stays provable - and the thing it stops proving

**Parity is proved against upstream's binary and never against the published
description**, and that arrangement is unchanged. `gen_ft8.exe -ft4` answers for any
message; the comparison is symbol for symbol; a divergence is a red test naming a
position. Nothing about adding FT4 weakens that, and the pin is the same commit the
FT8 side is already held to.

**What does change is that faithfulness and correctness come apart, for the first time
in this project.** Until now the port being byte-identical to upstream and the port
being right were the same statement, because upstream is a well-exercised FT8 decoder
read against real off-air recordings. For FT4 there is a number in upstream that this
tree cannot corroborate: `FT4_SYMBOL_PERIOD` is `0.048f` (`ft8/constants.h:14`), which
puts 105 symbols at **5.04 s** and the tone spacing at **20.833 Hz**, where both
`WORK_INSTRUCTIONS.md` and `PHASE_PLAN.md` step 1 say the transmission is **4.48 s**.
The string `4.48` appears **nowhere in the upstream clone**, and neither does a baud
figure or a bandwidth figure - `grep -rn "4\.48|23\.4375|20\.8|baud|bandwidth"` over
every `.c`, `.h` and `.md` in the clone returns only the two `SYMBOL_BT` lines.

So a faithful port of FT4 can be provably faithful and still unable to hear a real FT4
station, and **no test in this repository could tell the difference**, because every
FT4 test available today is Hamlet's encoder against Hamlet's decoder or against
upstream's, and all three would share the same 0.048 s. This is HM-DEC-091's finding
in a new place: the seven synthetic CW fixtures all passed while the decoder was deaf
on the air.

### What I would need that I do not have

Three things, in the order they block work.

1. **A ruling on which figure step 1 is measured against.** `PHASE_PLAN.md` step 1
   makes "the timing is FT4's: 7.5-second slots, 4.48 seconds of transmission, four
   tones - measured, not asserted" a *must-pass*, and a faithful port of `ft8_lib`
   measures 5.04 s. Those cannot both hold. Either the criterion moves to upstream's
   figure, or the port deliberately diverges from upstream on one constant and says so
   - and a deliberate divergence in `Ft8Sharp` is exactly the thing the ruling in force
   says is not spent casually. **This blocks step 1's exit, not its start.**
2. **The published description.** `ft8_lib`'s `README.md:47` cites
   `https://physics.princeton.edu/pulsar/k1jt/FT4_FT8_QEX.pdf`. No copy of it is in this
   repository or in the clone, and §4's rule is that a figure comes from a citation
   rather than from a model's memory - so this unit states the conflict and does not
   resolve it. A vendored page under `data/vendor/` would settle item 1 without a
   ruling, if the paper's terms allow it.
3. **A real off-air FT4 recording**, which this tree does not have. `ft8_lib`'s
   `test/wav/` is FT8 throughout and `tests/fixtures/` has no FT4 audio. Until one
   exists, every FT4 decode this project can demonstrate is self-consistent by
   construction, and item 1 is unfalsifiable from inside. One recording answers items 1
   and 3 together and outranks every synthetic fixture (HM-DEC-091).

---

## Task 4 - what the FT4 button does today

**Pressed in the running application**, not read out of the source. `Hamlet.App` was
built (`dotnet build`, 9.05 s, 0 warnings, 0 errors), launched, and the FT4 chip was
found and invoked through Windows UI Automation. The whole visible text of the window
was captured before the press and after it and the two were differenced, so what
follows is what changed on screen and not what a reading predicts would change.

**The operator's settings were backed up before the run and restored after it.**
Pressing the chip persists `LastDigitalSubMode`, which went `FT8` to `FT4`; it is back
at `FT8`.

### Finding the chip took two attempts, and that is itself a finding

The four chips are `Button`s whose whole content is a `Panel`
(`MainWindow.axaml:2985-2991`), so UI Automation reports their names as
`Avalonia.Controls.Panel` and a search for a button called `FT4` finds nothing. The
label lives one level down, on a `TextBlock` inside. Once matched that way all four
are there, enabled, on screen, and correctly ordered:

```
'FT8'    enabled=True  offscreen=False  rect=445,769,45,23
'FT4'    enabled=True  offscreen=False  rect=496,770,42,21
'PSK31'  enabled=True  offscreen=False  rect=544,770,57,21
'WSPR'   enabled=True  offscreen=False  rect=607,770,55,21
```

They support `Invoke`, so they are live controls and not labels.

### It tunes, and the display waits for the radio

Starting state: training radio, 40 m, **7.030 MHz**, FT8 the chosen mode. After the
press:

| | Before | After |
|---|---|---|
| Frequency readout | `7.030 MHz · yours to use` | `7.047 MHz · yours to use` |
| The press line | *not shown* | `FT4 on 40 m — the radio confirmed 7.047000 MHz.` |
| Licence line | `Your General license covers digital modes here. Call away.` | `Your General license covers digital modes here.` |

7.047000 MHz is `data/bands/us-neighborhoods.json:293-295`, the 40 m FT4 row's
`jumpHz`. Nothing was invented and the wording is the read-back wording, so
`TuneToDigitalModeAsync`'s promise that the display moves on the read-back and never on
the command is being kept (`MainWindowViewModel.cs:1016-1022`).

**So: yes, it tunes.** That half works exactly as unit 251 built it.

### It changes nothing else, and one of those nothings is a wrong sentence

Everything else on the window was identical before and after. In particular, **with the
dial on 7.047 and FT4 chosen, the decoded panel still reads**:

> nothing on this frequency yet. **FT8 runs in fifteen second slots**, so give it a
> slot or two before deciding the band is empty.

That sentence is now false about the frequency the application just tuned to, and it is
the application telling the operator to wait for something that will not arrive. FT4's
slot is 7.5 s (`ft8/constants.h:15`), and the thing actually cutting slots is
`Ft8Slots.SlotSeconds`, `public const double SlotSeconds = 15`
(`src/Hamlet.RadioEngine/Audio/Ft8Slots.cs:126`) - a constant with no mode in it.
`DigitalWaterfallSummary` prints `"15 s slots"` from a literal at
`MainWindowViewModel.cs:939`, likewise unconditional.

**This is the §0.0 half of the finding and it is worth separating from the missing
decoder.** A decoder that has not been written is an absence and the screen can say so.
A sentence that asserts a fifteen-second slot grid on an FT4 frequency is a claim, it is
wrong, and the operator acting on it waits for a decode that no amount of waiting
produces.

### What silently does nothing

`ChosenDigitalMode` is read by exactly two things in the whole application -
`grep -rn ChosenDigitalMode src/Hamlet.App` returns three hits and one of them is the
assignment:

- `MainWindowViewModel.cs:324-328`, which persists it to `settings.json`;
- `MainWindowViewModel.cs:982`, which hands it to `DigitalModeChip.For` so the chip
  draws as chosen.

**Nothing in the decode path reads it at all.** `Ft8SlotWatch`
(`MainWindowViewModel.cs:190`), `Ft8Slots.SlotStart`, `Ft8Reception` and the decode
timer are all unconditional FT8. So after the press the application sits on an FT4
calling frequency cutting fifteen-second FT8 slots and running the FT8 decoder over
them, and it will do that indefinitely without an error, a warning or an empty-looking
failure - it looks exactly like a quiet band.

### Nothing broke

No exception, no dialog, no unresponsive control, and the window closed cleanly with
exit code 0. `Invoke()` returned without throwing. There is nothing to repair in what
the button does; there is only the far larger thing it does not do yet.

---

## Task 5 - the gaps, named

Tim's ruling is **exactly the way FT8 does**, so every one of these is a thing FT8 has
and FT4 would not. **None is fixed here** and none is a scope decision this unit is
making; they are named so the phase's later steps have a list rather than a memory.

The shape of the problem in one sentence: `Ft8Slots` holds two `const double`s -
`SlotSeconds = 15` (`:126`) and `TransmissionSeconds = 12.64` (`:135`) - and **eight
files across the engine and the application compute from them with no mode in the
question.**

### 1. The slot clock, and the eight things that read it

| File | What it does with the fifteen |
|---|---|
| `Audio/Ft8SlotWatch.cs` | decides when a slot has closed, and cuts `SlotSeconds * rate` samples |
| `Audio/Ft8SlotCutter.cs` | cuts a slot's audio, and refuses one that cannot hold `TransmissionSeconds` |
| `Audio/Ft8Turn.cs` | the turn ring - how long is left, and whose turn it is |
| `Audio/DigitalCaptureSheet.cs` | writes the sidecar's boundary list and its "12.64 s transmission" line |
| `Contacts/Ft8ContactState.cs` | how long a contact may stand before it goes stale |
| `Contacts/Ft8ContactLedger.cs` | counts slots between two moments |
| `Transmit/Ft8TransmitSequence.cs` | `TransmissionFits` - whether 12.64 s of tones still lands inside the slot |
| `ViewModels/MainWindowViewModel.cs` | rides the decode tick, and prints the grid |

FT4's figures are 7.5 s (`ft8/constants.h:15`) and, on upstream's constant,
105 x 0.048 = 5.04 s - which is the number task 3 says needs a ruling before it is
written anywhere.

### 2. Two sentences on screen that would be wrong rather than absent

Task 4 measured both of these live.

- **`nothing on this frequency yet. FT8 runs in fifteen second slots, so give it a
  slot or two before deciding the band is empty.`** Still shown, word for word, with
  the dial on 7.047 and FT4 chosen.
- **`DigitalWaterfallSummary` prints `"15 s slots"`** from a literal at
  `MainWindowViewModel.cs:939`.

These two are different in kind from everything else on this list: **a missing FT4
decoder is an absence and the screen can say so, but these are claims** (§0.0,
HM-DEC-092). They are also the cheapest items here.

### 3. The transmit chain is FT8 end to end

`Ft8Composer.Compose` calls `Ft8SymbolEncoder.Encode` and then
`Ft8Waveform.SynthesizeSlot` or `Synthesize` (`Ft8Composer.cs:383-386`), and task 2
named both of those as FT8-assuming. `Ft8Composer.cs:181-182` says in its own remarks
that there is no Costas array in it, which is exactly why the whole thing follows the
port. There is no FT4 path and there is no seam where one would go.

`Ft8ReadBack.WouldReachAnybody` (`Ft8ReadBack.cs:63-69`) refuses a send whose callsign
would travel as a hash, by decoding it back through `Ft8SlotDecoder`. That gate is
unit 272's whole answer to the two live transmissions of 2026-09-07, and **it has no
FT4 equivalent**, so an FT4 send path built without one would reopen a defect this
project has already put on an antenna.

### 4. The log has no room for the mode - already found, still true

Unit 287's finding holds and this unit confirms it in the code:
`AchievementsViewModel.cs:311-315` says so in its own comment, and matches on
`mode.Matches(c.Mode, null)` because `AdifContact` has no `SUBMODE` field. FT4 is
`MODE=MFSK, SUBMODE=FT4` in ADIF, so **`MODE=FT4` is invalid** and there is nowhere
valid to write it. This is step 3 of the phase and is parked; it is listed because it
is on the "exactly the way FT8 does" ledger.

### 5. The achievements row cannot be earned, by construction

`AchievementsViewModel.cs:374-378` already tells the operator this in as many words -
that Hamlet can tune him to FT4 and cannot work it, and that the record has no room for
the mode either. That copy is honest today and becomes wrong the day either half lands,
so it is a gap with a second edge: **the screen has to stop saying it at the right
moment.**

### 6. The frequency table covers five bands of seven

Counted from `data/bands/us-neighborhoods.json`:

| Band | FT8 | FT4 |
|---|---|---|
| 80 m | 3.573 | 3.575 |
| 40 m | 7.074 | 7.047 |
| **30 m** | 10.136 | **none** |
| 20 m | 14.074 | 14.080 |
| **17 m** | 18.100 | **none** |
| 15 m | 21.074 | 21.140 |
| 10 m | 28.074 | 28.180 |

`MainWindowViewModel.cs:1060-1063` already knows and says so on screen rather than
inventing a number. Whether 30 m and 17 m genuinely have no FT4 convention or the rows
are merely unsourced is **not** something this unit can settle - it needs a citation,
which is §0's rule and not a preference.

### 7. The signal-to-noise column would read the wrong number

`Ft8DeepSignalToNoise` carries a per-bin ratio to the 2500 Hz reference through
`10 log10(2500 / 6.25) = 26.0206 dB` (`Ft8DeepSignalToNoise.cs:114-131`). The 6.25 is
**the reciprocal of FT8's symbol period** and its own remark at `:103` says so. On
FT4's symbol period the bin is 1/0.048 = 20.833 Hz and the constant would be
`10 log10(2500 / 20.833) = 20.79 dB` - **5.2 dB out** if the FT8 constant were reused.
Unit 251 measured that estimate to a mean absolute error of 0.26 dB, so reusing the
constant would put a number in the `snr` column that is wrong by twenty times the
error it was accepted at, on all three surfaces it reaches: the panel cell, the
per-slot telemetry line and the capture sidecar.

**This one is worth flagging above the others** because it fails quietly and
plausibly - a decoded FT4 message with an SNR five decibels optimistic looks exactly
like a decoded FT4 message.

### 8. Telemetry says `ft8_slot` and means it

`AppEvents.cs:940` and `:1010` emit `ft8_slot`. HM-DEC-077 makes a reason token stable
on purpose, so this is not a rename to be done casually: either FT4 slots carry a
protocol field on the same event, or they get their own token, and either way every
comparison across sessions has to survive it.

### 9. The capture sidecar names one transmission length

`DigitalCaptureSheet` writes the `12.64 s transmission` line and the slot boundary list
from `Ft8Slots`. A sidecar is the thing that makes a wrong decode a regression test
(§0.0.1), so a sidecar that says 12.64 next to FT4 audio is worse than one that says
nothing.

### 10. The decoder itself

Named last because it is the one thing everybody already knows is missing, and because
tasks 1 to 3 established that it is the *smallest* of the ten in terms of what has to be
newly reasoned: five files in the port, three table entries, and upstream's own answer
for every one of them.
