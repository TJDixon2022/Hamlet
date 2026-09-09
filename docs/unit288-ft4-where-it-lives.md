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
