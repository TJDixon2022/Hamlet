# Work instruction 288 — where FT4 lives, decided by reading

**READ IN THIS ORDER.** This unit built nothing, so the answers are the product
and they are in section 3.

A. **The phase goal — FT4 works exactly the way FT8 does.** Anything FT8 does that
   FT4 does not is a gap to be named, and section 3 names ten of them.

B. **Step 0 and its exit criteria** — what `ft8_lib` carries with file and line,
   what `Ft8Sharp` already shares, the decision with its reason, and what the 51
   fidelity tests become. All four are must-pass and all four are met, which is
   what moved step 0 to `done` in `PHASE_OUTCOME.md`.

C. **The report last, and section 4 raises 2 items.** Neither blocks the next unit
   from starting; the first blocks step 1's *exit* and the second is a version
   ruling that has drifted from the tree.

```
UNIT:       288 — complete at task 6 of 6, none dropped — 2026-09-09 08:22
PHASE GOAL: FT4 works exactly the way FT8 does.
UNIT GOAL:  Answer from the tree, not from memory, where the FT4 decoder goes, and
            count what is already shared so nothing gets written twice.
ADVANCED:   yes — step 0 closed with all four of its must-pass exits met, and the
            question the whole phase was gated on is answered with file and line.
NUMBER:     how much of FT4 already exists in this tree
            28 of 33 port files, 6,160 of 7,926 lines — protocol-neutral today,
                                                        used by FT4 UNCHANGED
             5 of 33 port files, 1,766 of 7,926 lines — FT8-specific, would be new
            and upstream carries a working answer for every one of the five.
DRIFT:      0 consecutive units without advance  (was 10, carried from unit 287)
```

---

## 1. What Claude did

**Complete. Six tasks of six, none dropped, nothing left for a later unit to pick
up.** Development machine, prompt claimed `PROJECT: Hamlet`, branch `main`, six
commits, all pushed. Root version 1.12.214 to 1.12.215. `Ft8Sharp` stays at 0.10.7
and `Ft8Sharp.Deep` at 0.8.0; no file under `src/Ft8Sharp/` was touched.

The four repository checks in the gate all held: `SHACK_FACTS.md` present,
`src/Hamlet.RadioEngine/Cw/CwProbabilisticDecoder.cs` present, no `CoreHMI.sln`,
no `MURC.sln`.

**No test suite was run.** No test was constructed and none needed to be, so
nothing was run under a filter either. One `dotnet build` of `Hamlet.App`,
foregrounded with a 500-second timeout, finished in 9.05 s with 0 warnings and 0
errors. Nothing was backgrounded and nothing was polled.

The whole of the reading is in `docs/unit288-ft4-where-it-lives.md`, which is the
unit's deliverable. What follows is what it establishes.

**Task 1 — upstream carries FT4, and no filename says so.** The pinned clone is at
`C:\Source\ft8_lib` where `tools/build-ft8-oracle.bat:46` says it is, at commit
`9fec6ca39886edbf96f4f5e71edc76da5074e871`, which is the commit
`src/Ft8Sharp/porting-notes.md:14` pins the port to. **There is no file called
`ft4.c`.** FT4 lives inside the FT8 files behind `ftx_protocol_t`
(`ft8/constants.h:52-56`): `ft4_encode` at `ft8/encode.c:127`, and four decoder
branches — `ft4_sync_score` at `decode.c:127`, the protocol dispatch at `:192`,
`ft4_extract_likelihood` at `:253`, `ft4_extract_symbol` at `:454`, plus the
payload un-XOR at `:369`. Seven and a half second slots, four tones, 105 symbols,
87 data symbols of two bits, two ramp symbols, four *different* Costas groups at
`constants.c:5-10`.

**Task 2 — the port is already three-quarters of the way there.** Grepping the
port for every FT8 channel constant returns hits in exactly five files. The whole
message layer, the LDPC(174,91) code and the CRC-14 have no protocol branch in
them at all — which is what upstream's own `FTX_` prefix marks, and what
`porting-notes.md:309` already records. The three FT4 tables are in the same
generated file's source and the converter **deliberately steps over them**, saying
so in its own remarks (`Ft8TableConverter.cs:42-44`); they cost three manifest
entries and a regeneration.

**Task 3 — the decision, and one thing it stops proving.** Recorded below in
section 3. On the way to it, the oracle was run rather than assumed to work, and
that produced the unit's sharpest finding — also in section 3.

**Task 4 — the button was pressed in the running application**, not read out of
the source. Reported in section 3.

**Task 5 — ten gaps named, none fixed.** Section 3 lists them.

**Task 6 — bookkeeping.** `install-phase.bat` had been run: `PHASE_STATUS.md` and
`PHASE_PLAN.md` both carry the FT4 phase, set 2026-09-08. So `PROJECT_CARD.md`
took `PHASE` and `PHASE_SET` from that header, and because §13.3 permits that only
by ruling, the ruling was written: **HM-DEC-160**, Tim's approval of
`PHASE_PLAN.md` on 2026-09-08, appended to `DECISIONS.md` above HM-DEC-159 and
indexed in `CLAUDE.md` §1 immediately below the separator.
`tools\arbiter\outcome-append.bat` ran clean, appended unit 288's entry and moved
step 0 to `done` in `PHASE_OUTCOME.md`'s header — **the shell did not refuse, so
no file-editing fallback was used.**

**One decision made for itself, and it is small.** The work instruction's task 6
named `DECISIONS.md` and did not name `CLAUDE.md` §1. Every ruling from HM-DEC-159
back is in both, §1 says in its own words that it is the index of every ruling, and
`DecisionLogOrderTests` polices its ordering — so the row was added rather than
left out, which is HM-DEC-160 indexed and not a second ruling. Reproduced in full
so it can be overridden: **the §1 row is the same ruling as the `DECISIONS.md`
entry, restated in the table's own compressed form, dated 2026-09-08, inserted
immediately below the `|---|` separator per §1's own placement rule.**

**Two mismatches against the work instruction, reported and not repaired.**

1. The instruction describes FT4 as **"4.48 seconds of transmission"**, and so does
   `PHASE_PLAN.md` step 1, as a *must-pass*. Upstream's `FT4_SYMBOL_PERIOD` is
   `0.048f` (`ft8/constants.h:14`) and 105 symbols at that is **5.04 s**. The
   string `4.48` appears nowhere in the clone. This is the first ask in section 4.
2. The instruction says to bump the **patch**. HM-DEC-150 says a new phase bumps
   the **minor** and resets the patch to zero, which would make this 1.13.0. The
   instruction was followed. This is the second ask in section 4.

## 2. What the owner should expect

**Nothing in the application changed.** No decoder, no threshold, no panel, no
copy. `Hamlet.App` builds clean and behaves exactly as it did before this unit; the
FT4 button still tunes and still does nothing else.

**What is now true of the phase:** step 0 is `done`, the question the other six
steps were gated on is answered, and **the first build unit has a smaller job than
anyone was carrying in their head.** FT4's decoder is five files in the port plus
three generated table entries, with upstream's own working code as the answer for
every one, and upstream's own binary as the reference to hold them to. The message
layer, the code, the checksum and the whole FFT and waterfall stack are already
protocol-neutral and get used unchanged.

**What will look wrong and is not.**

- **`PHASE_OUTCOME.md` still reads `*None yet. This phase has not started.*` above
  unit 288's entry.** That prose line sits under `## Entries` and
  `outcome-append.bat` appends below it by design, never touching what is above the
  new entry. It is cosmetically stale and was deliberately left rather than
  hand-edited around a script that owns the file.
- **`PHASE_STATUS.md` still shows `STEP: 0 | not started`.** `outcome-append.bat`
  updates `PHASE_OUTCOME.md`'s header and not that one, and task 6 did not name it.
  `PHASE_OUTCOME.md` is the file whose header the script keeps current.
- **The operator's `settings.json` was touched and put back.** Pressing the FT4
  chip persists `LastDigitalSubMode`, which went `FT8` to `FT4`; it was backed up
  before the run and restored to `FT8` after, and it is `FT8` now.
- **`.oa-287.bat` at the repository root is not loose and is not this unit's.**
  The git snapshot this session opened with showed it untracked, which was already
  out of date: `install-phase.bat`'s own commit `048a158` had committed it along
  with the FT4 phase files. It is tracked and clean and was not touched.
- **The version is 1.12.215 and not 1.13.0.** See the second ask in section 4.

## 3. What you should see

**No visible change in the application.** This unit only reads, and it is worth
what it cost only if the four answers below save the build units from getting it
wrong. Here they are.

### 1. Does `ft8_lib` carry FT4? Yes, and completely

`ft4_encode` at `ft8/encode.c:127`, declared at `encode.h:35`. Four decoder
branches at `ft8/decode.c:127`, `192`, `253` and `454`, plus the payload un-XOR at
`:369`. Three FT4 tables at `ft8/constants.c:5`, `:14` and `:16`. `-ft4` on both
console programs (`gen_ft8.c:130`, `decode_ft8.c:250`). `README.md:1` reads
"FT8 (and now FT4) library". **There is no file called `ft4.c`**, so a filename
test would have answered this wrongly and in the expensive direction.

**And it was run, not read.** Upstream generated `CQ KC3QIS FN00` as 105 FT4 tones
into a 7.5 s WAV — and **upstream's own decoder then read 0 messages out of it**,
which looks exactly like hollow support. It is placement.
`ftx_find_candidates` searches `time_offset` from **-10 to 19 blocks**
(`decode.c:205`), which at FT8's 0.16 s block is -1.6 s to +3.0 s and at FT4's
0.048 s block is only **-0.48 s to +0.91 s** — while `gen_ft8.c:173` centres the
transmission, putting FT4's first symbol at **1.23 s**. Shifting the identical
audio forward 0.99 s and changing no sample value:

```
Decoded 1 messages
000000 +19.5 +0.29 1000 ~  CQ KC3QIS FN00
```

**So the whole upstream chain agrees end to end**, and the first build unit
inherits a candidate window a quarter as wide in seconds as FT8's. Whatever cuts an
FT4 slot has to land the transmission within about 0.9 s of the slot start, and
whether `Ft8SlotCutter`'s measured offset does that is a measurement owed, not an
assumption available.

### 2. The decision: FT4 goes in the port, `src/Ft8Sharp`

The rule Tim ruled on 2026-09-08 is conditional, and finding 1 satisfied the
condition. Three things follow that would have been arguments had the reading gone
the other way:

- **The licence story does not move.** `ft8_lib` is MIT; FT4 in the port is a port
  of MIT code into an MIT library. FT4 in `Ft8Sharp.Deep` would be GPL-3.0 code
  re-deriving something that already exists under MIT.
- **The instrument exists only because upstream carries FT4.** `gen_ft8.exe -ft4`
  answers for any message, so FT4 in the port can be held to the same byte-fidelity
  bar FT8 already clears. A sibling would be proved against itself.
- **Nothing has to open.** Unit 245's finding was confirmed —
  `Ft8CodewordResult`'s constructor is `private` and its factories `internal`, and
  no `InternalsVisibleTo` names `Ft8Sharp.Deep`. A sibling would have had to rewrite
  the same five files anyway *and* reproduce the port's loop. Because FT4 goes in
  the port, **the minimal opening is none.**

**What the 51 fidelity tests become: 102.** The corpus does not change — it is a
message-layer corpus and the message layer is shared, so all 51 texts are valid FT4
messages unchanged. `Ft8Oracle.Generate` gains one optional protocol argument
(`gen_ft8`'s fourth positional), and
`Ft8SymbolBitIdentityTests.EverySymbolOfEveryMessageIsIdenticalToUpstreams` gains a
protocol dimension: 51 comparisons at 79 symbols and 51 at 105. The existing FT8
assertion is left alone rather than generalised, so an FT8 regression stays
attributable. `Ft8TableConverter.Manifest` grows by three and the regeneration test
then covers the FT4 tables for free.

**And the thing this stops proving, which is the finding worth arguing with.**
Parity is proved against upstream's binary, never against the published
description, and that arrangement is unchanged. But **faithfulness and correctness
come apart here for the first time in this project**: `FT4_SYMBOL_PERIOD` is a
number this tree cannot corroborate, so a faithful port can be provably faithful and
still unable to hear a real FT4 station, and **no test in this repository could tell
the difference** — every FT4 test available today is Hamlet's encoder against
Hamlet's or upstream's decoder, all three sharing the same constant. That is
HM-DEC-091's finding in a new place.

### 3. What pressing FT4 does today: it tunes, and that is all

Measured in the running application through UI Automation, with the whole visible
text of the window captured before and after and differenced.

| | Before | After |
|---|---|---|
| Frequency | `7.030 MHz · yours to use` | `7.047 MHz · yours to use` |
| Press line | *not shown* | `FT4 on 40 m — the radio confirmed 7.047000 MHz.` |

7.047000 is the 40 m FT4 row's `jumpHz` in `data/bands/us-neighborhoods.json`, and
the wording is the read-back wording, so unit 251's promise that the display moves
on the read-back and never on the command is being kept. All four chips are live,
enabled and invokable. **Nothing broke** — no exception, no dialog, clean exit 0.

**Everything else was identical, and one of those identicals is a wrong sentence.**
With the dial on 7.047 and FT4 chosen, the decoded panel still reads:

> nothing on this frequency yet. **FT8 runs in fifteen second slots**, so give it a
> slot or two before deciding the band is empty.

That is not an absence the screen is honest about; it is a claim, it is false about
the frequency the application just tuned to, and it tells the operator to wait for
something that will never come.

**And what silently does nothing:** `ChosenDigitalMode` is read by exactly two
things in the whole application — the settings save at
`MainWindowViewModel.cs:324`, and how the chip draws at `:982`. **Nothing in the
decode path reads it.** So after the press Hamlet sits on an FT4 calling frequency
cutting fifteen-second FT8 slots and running the FT8 decoder over them, forever,
with no error and no warning. It looks exactly like a quiet band.

### 4. The gaps, listed, none fixed

1. **The slot clock.** `Ft8Slots.SlotSeconds = 15` and `TransmissionSeconds = 12.64`
   are `const double`, and **eight files compute from them with no mode in the
   question**: `Ft8SlotWatch`, `Ft8SlotCutter`, `Ft8Turn`, `DigitalCaptureSheet`,
   `Ft8ContactState`, `Ft8ContactLedger`, `Ft8TransmitSequence`,
   `MainWindowViewModel`.
2. **Two sentences that would be wrong rather than absent** — the fifteen-second
   line above, and `DigitalWaterfallSummary`'s literal `"15 s slots"`
   (`MainWindowViewModel.cs:939`). The cheapest items here, and the only two that
   are §0.0 breaches today.
3. **The transmit chain is FT8 end to end.** `Ft8Composer` calls
   `Ft8SymbolEncoder.Encode` and `Ft8Waveform.SynthesizeSlot`, both FT8-assuming,
   and there is no seam where an FT4 path would go.
4. **The hashed-callsign read-back gate has no FT4 equivalent.**
   `Ft8ReadBack.WouldReachAnybody` is unit 272's whole answer to the two live
   transmissions of 2026-09-07, and an FT4 send path built without one reopens a
   defect this project has already put on an antenna.
5. **The log has no `SUBMODE`.** Unit 287's finding, confirmed in the code at
   `AchievementsViewModel.cs:311-315`. `MODE=FT4` is invalid ADIF. Phase step 3.
6. **The achievements FT4 row cannot be earned by construction**, and the copy that
   honestly says so today becomes wrong the day either half lands.
7. **The frequency table covers five bands of seven** — no FT4 row on 30 m or 17 m.
   The application already says so rather than inventing a number; whether the rows
   are genuinely absent or merely unsourced needs a citation.
8. **The signal-to-noise column would read five decibels optimistic.**
   `Ft8DeepSignalToNoise` carries a per-bin ratio through
   `10 log10(2500 / 6.25) = 26.0206 dB`, and the 6.25 is the reciprocal of *FT8's*
   symbol period — its own remark at `:103` says so. On FT4's 0.048 s bin the same
   arithmetic gives `10 log10(2500 / 20.833) = 20.79 dB`, so reusing the constant
   is **5.2 dB out** against an estimate unit 251 accepted at 0.26 dB mean absolute
   error. **This is the one to watch**, because it fails quietly: an FT4 message
   five decibels optimistic looks exactly like an FT4 message.
9. **Telemetry emits `ft8_slot`** (`AppEvents.cs:940`, `:1010`), and HM-DEC-077
   makes a reason token stable on purpose.
10. **The capture sidecar names one transmission length**, 12.64 s, and a sidecar is
    what turns a wrong decode into a regression test.

## 4. What's blocking us

Nothing blocks the next unit from starting. Two questions block a *step* each, and
the first blocks step 1's exit rather than its beginning, so step 1 can be written
and begun today.

**The work instruction parks the asks queue carried since unit 271, so it is not
restated here.** These two are new tonight.

---

**FT4's symbol period is upstream's 0.048 s, and `PHASE_PLAN.md` step 1's
"4.48 seconds of transmission" is amended to match — or the port diverges from
upstream on one constant and records it as a deliberate divergence.**

Upstream states `FT4_SYMBOL_PERIOD (0.048f)` at `ft8/constants.h:14` and nothing
else. 105 symbols at 0.048 s is **5.04 seconds** and the tone spacing is
**20.833 Hz**. `WORK_INSTRUCTIONS.md` and `PHASE_PLAN.md` step 1 both say the
transmission is **4.48 seconds**, which implies 23.4375 symbols per second and a
23.4375 Hz spacing. `grep` over every `.c`, `.h` and `.md` in the pinned clone for
`4.48`, `23.4375`, `20.8`, `baud` and `bandwidth` returns **only the two
`SYMBOL_BT` lines** — the figure is not upstream's and this tree cannot say where
it came from.

They cannot both hold, and step 1 makes the 4.48 figure a *must-pass measured, not
asserted*. As written, **a faithful port of `ft8_lib` fails step 1's own exit
criterion**, which is a defect in the plan rather than in the work — the same shape
HM-DEC-158 found when four steps sat `partial` for eight units on criteria no bench
could satisfy.

What was rejected. **Settling it from a model's memory of the published FT4
parameters**, which is precisely what §4 forbids and what this unit was written to
avoid; the number is stated in the QEX paper `ft8_lib`'s `README.md:47` cites, and
no copy of it is in this repository or the clone. **Quietly writing 5.04 into the
slot machinery and moving on**, which would leave a *must-pass* criterion in the
plan that the code contradicts, and the next session would read the plan and
believe it. **Diverging from upstream on the constant without a ruling**, because
`Ft8Sharp`'s byte-fidelity is the instrument every measurement of the sensitivity
phase leaned on and it is not spent by a session on its own.

**What would settle it cheaply and needs no ruling at all: one real off-air FT4
recording.** With one, the constant is a measurement rather than an argument, and
HM-DEC-091's precedent says a real recording outranks every synthetic fixture.
Vendoring the cited QEX page under `data/vendor/` would also settle it, if the
paper's terms permit.

---

**The version scheme is what `Directory.Build.props` has actually been doing — a
patch per work unit, with the minor moving rarely — and HM-DEC-150's "the minor is
the phase" is superseded to match, or 1.12.215 is corrected to 1.13.0.**

HM-DEC-150 says a phase bumps the minor and resets the patch to zero, and that
`PROJECT_STATUS.md`'s `PHASE` field and the minor are the same number read from one
place. This work instruction says to bump the patch, and the instruction was
followed: 1.12.214 to **1.12.215**.

The instruction matches practice and the ruling does not. `1.12` has now spanned
the sensitivity phase, the send phase, the on-air phase and the start of the FT4
phase — well over sixty work units and at least four phases — so **the minor has not
tracked the phase for a long time and nothing noticed**, which is the same class of
failure HM-DEC-113 recorded about the branch nobody had ruled on.

What was rejected. **Writing 1.13.0 on this unit's own authority**, which is a
version decision under a ruling that is not this session's to supersede.
**Reporting the number without the conflict**, which would leave a ruling and a tree
disagreeing for another sixty units. **Treating the instruction as defective**,
because it is the ruling that has drifted from the tree and not the other way round,
and the instruction is the more recent statement of intent.
