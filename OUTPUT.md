READ IN THIS ORDER

A. THE PHASE GOAL. Hamlet reads FT8 as well as the best decoder there is, and
   then reads it further. Five units made the decoder measurably better and none
   of it reached the operator: everything built lives in `Ft8Sharp.Deep` and
   Hamlet called `Ft8Sharp`, the faithful port, exactly as it did before the
   phase began.

B. THIS STEP AND ITS EXIT CRITERIA. The step sequence is paused by Tim's ruling
   of 2026-09-05. This is a single work instruction with no successor, and it
   connects the two libraries and nothing else. It is wiring, not decoding. Its
   exits: the Digital tab decodes through `Ft8Sharp.Deep` with fine sync and
   ordered statistics on; the five-count census still reaches all three surfaces
   with the same meanings; the port's parity and CRC-14 gates stay in the path; a
   capture says which decoder read it; and the comparison exists, off by default.
   All five are met.

C. THIS REPORT, weighed against A and B. The thing here is task 1's finding,
   because it decided the wiring before a line of it was written: `Ft8Reader`
   called `Ft8DeepSlotDecoder.Decode(Ft8Waterfall)`, and that overload hands
   Deep's loop an empty span - it refused all 42 candidates for want of samples.
   The off-grid gain this phase was for is fine sync's, so a Hamlet wired the
   obvious way would have paid 2.1 times the port's cost and got none of it.
   Through the samples entry point fine sync re-synced 42 and accepted 14. That
   bears on A, because it is the difference between the phase's gains reaching
   the operator and not.
   Section 4 raises 1 item. It asks for no ruling and blocks no exit criterion in
   B; it is an observation about a number that is unbounded and, on this machine,
   harmless.

UNIT:       249 - complete at task 5 of 5 - 2026-09-05 13:05
PHASE GOAL: Hamlet reads FT8 as well as the best decoder there is, and then reads it further.
UNIT GOAL:  The Digital tab decodes through Ft8Sharp.Deep with fine sync and ordered
            statistics on, and a capture says which decoder read it.
ADVANCED:   yes - the operator's own screen changes for the first time in six units.
            Hamlet decodes through Deep with both stages on, and every capture from
            tonight names the decoder that read it.
NUMBER:     one slot 261 ms end to end against a 15,000 ms budget, 1.74 per cent used;
            from the same 55 candidates Deep carries 27 through to text where the port
            carries 9; 3 messages each way on the example fixture, every one re-checked
            against the port's gates, 0 wrong. Suites: Ft8Sharp.Tests 609 passed / 0
            failed / 1 skipped; Ft8Sharp.Deep.Tests 69 / 0 / 0.
DRIFT:      0 consecutive units without advance  (was 0)

## 1. What Claude did

**Complete, at task 5 of 5. Nothing was dropped, including the named drop
candidate.** Machine `C:\Source\HamLet`, project confirmed as Hamlet by the
instruction's four filesystem checks - both must-exist files present, both
must-not-exist absent. Branch `main`, which is trunk. Six commits, every one
pushed; the last is `57a2ad2`.

**Root version 1.12.50 to 1.12.51. `Ft8Sharp` did not move and not a line of
`src/Ft8Sharp/` was touched. `Ft8Sharp.Deep` did not move either** - this unit
needed nothing from it that it did not already have.

**No shell call was refused in this session.** The tool rule was ready for the 5
to 28 denials a unit `RUN_LEDGER.md` has recorded for a fortnight; none came.

**Nothing in this report is evidence about the radio** (FACT-004).

### The instruction against the tree

**Every claim held, and one was understated in a way that mattered.**
`Ft8DeepSlotDecoder`'s fine sync and ordered statistics are null by default and
null is the port exactly. `Ft8Reception.cs` holds `Ft8Reader.Read`, cutting slots,
resampling to 12 kHz and calling the port per slot. The five-count census reaches
all three surfaces. `Ft8Sharp` is `0.10.7`, `Ft8Sharp.Deep` `0.3.0`, root was
`1.12.50`. The boundary test guards the port from the port's side and was not
touched.

**What the instruction could not know**, because it was written from unit 248's
report rather than the code: `Ft8DeepSlotDecoder.Decode` returns `Ft8SlotResult`,
**the port's own type**, so `Ft8Reader` needed no adapting at all - the census
counts are the same fields on the same record, and nothing had to be mapped. Deep
also exposes `Geometry`, `CandidateLimit` and `MinimumScore`, which are exactly
the three members the reader reads off its decoder. And **no caller anywhere
passes `Read` an explicit decoder**, so the parameter's type changed without
touching a call site.

### Task 1 - the seam, measured

`Decode(Ft8Waterfall)` cannot run fine sync, and Deep says so in its own remark:
that overload hands the loop an empty span, and "a waterfall has no phase in it
and no samples behind it and there is nothing in one to re-sync from."

| one slot, decode only | cost | fine sync |
|---|---|---|
| port | 23.6 ms | - |
| Deep via waterfall | 50.4 ms | refused 42 of 42 for want of samples |
| Deep via samples | 209.8 ms | re-synced 42, accepted 14 |

Committed as `90a786d`.

### Task 2 - Hamlet decodes through Deep

`Ft8Reader.Read` defaults to `Ft8DeepSlotDecoder` with both stages on at their
own `Default` settings, and calls the samples entry point. The waterfall is still
built and still used: `places` is read off it for the top Costas scores, which
the result type cannot give for a slot that decoded nothing.

**The port's gates stay in the path and the test re-checks rather than assuming
it** - every message the reader returns is packed back through the port's own
message layer and must survive.

Committed as `03762fb`.

### Task 3 - a capture says which decoder read it

`Ft8DecoderIdentity` carries the name and both stage flags and sits on
`Ft8SlotCensus` as an init property, on the "added rather than substituted"
precedent units 233 and 236 both set on that record.

**Putting it on the per-slot census is why no signature changed.** The sidecar and
the telemetry both take `IReadOnlyList<Ft8SlotCensus>` rather than the whole
reception, so it reaches all three surfaces without a new parameter threaded
through three call chains - and it is the honest granularity, since the
instruction asks which decoder produced the slot.

**An unrecorded decoder says so rather than naming the port.** The port was the
only one for a year, so defaulting to it would look harmless and would put a false
attribution in the one record that exists to settle attribution.

Committed as `b4e2bac`.

### Task 4 - the comparison, off by default

`CompareWithThePort` in `AppSettings`, off, persisted. When on, every slot is also
decoded through the port and its counts recorded beside Deep's. **The messages are
identical with it on and off, and that is asserted**: the port's numbers go to the
record and stay there. Null is "nobody asked" and never a zero, which would read
as "the port found nothing" - the opposite fact.

The engine takes it as a parameter rather than reading a setting, because the
engine references no settings type and §0.1 keeps it that way.

Committed as `b57a312`.

### Task 5 - the placeholder that reached a report

**Yes, and cheaply.** `validate-output.bat` already runs from `run-unit.bat` and
already holds six shape rules; this is rule 7, and it is the only one that checks
the shape was filled in rather than that it is a shape.

**The first cut could not catch the case it was written for.** It read the first
60 lines, the window rule 6 uses, and unit 248's token sits at line 71 - that
report's header runs long because its `NUMBER:` and `TESTS:` lines wrap. The
boundary is now everything before the `## 1.` heading. Watched failing on 248's
own report at exit 1, then passing at exit 0 on the same file with the header's
token replaced and section 3 still quoting it.

Committed as `57a2ad2`.

### Decisions made for itself

**None.** Every conclusion here is a measurement.

## 2. What the owner should expect

**What is now true.** Hamlet decodes FT8 through `Ft8Sharp.Deep` with fine sync
and ordered statistics on. The panel is untouched - same columns, same tooltips,
same sort, same `snr` dash, same trim; this unit added no display work. Every
capture sidecar and every `ft8_slot` telemetry row from tonight names the decoder
that read the slot and which stages were on.

**Build:** clean, 0 errors, 0 warnings, all projects.

| suite | |
|---|---|
| `Ft8Sharp.Tests` | 609 passed / 0 failed / 1 skipped |
| `Ft8Sharp.Deep.Tests` | 69 / 0 / 0 |
| Engine, audio and FT8 channels | 252 of 253 |
| `Hamlet.App.Tests`, not-Views | 572 of 572 |
| `Hamlet.App.Tests`, Views | 66 of 66 |

**What will look wrong but is not:**

- **`CwAdjudicationTests.ASpeedChangeInRealisticAudio` is red**, named
  pre-existing by this instruction. Not touched.
- **The `Ft8Sharp.Deep.Tests` type-list tripwire did not fire**, because this unit
  added no type to Deep. Its 69 tests all pass.
- **The 51 inherited CW reds** were not run and remain parked.
- **The engine project has no total**, for the fourth report running. The audio
  and FT8 channels were run whole, twice.
- **`portComparison    not run` in every sidecar** is the comparison being off,
  which is its ruled default - not a failure to run it.
- **A slot decodes in 261 ms where it used to take about 30.** That is Deep doing
  more work, inside a 15,000 ms budget.

## 3. What you should see

**The answer to what this unit was commissioned to ask: the phase's gains now
reach the screen.** For five units the answer to *what changes for the operator*
was *nothing*. This time, on a marginal evening, **more stations appear in the
decoded table** - and on a strong evening nothing changes, because Deep is a
superset and where the port could already read a signal both read it.

The size of that, from unit 248's ladder rather than from this unit: at -21 dB
over 306 trials, **13 of 306 through the port against 33 through Deep, 0 wrong
either way**. At the centre of a waterfall cell - where a real station lands,
because nothing on 14.074 arranges itself on Hamlet's analysis grid - **0 of 306
against 3 of 306**.

**The three things this section was asked to lead with.**

**1. What one slot costs.**

| | |
|---|---|
| port | 23.6 ms |
| Deep, both stages, via samples | 209.8 ms, 8.9 times the port |
| through `Ft8Reader` end to end | **261 ms** |
| budget | 15,000 ms |
| **margin left** | **14,739 ms, 1.74 per cent used** |

With the port comparison also on: 327 ms, 2.2 per cent used.

**2. The same reference recording before and after.** Both read **3 messages**,
and the same three: `CQ K1ABC FN42`, `CQ W9XYZ`, `K1ABC W9XYZ -11`. **Every one
passed the port's gates**, re-checked by packing each back through the port's own
message layer. **0 wrong.**

The fixture is the synthesised example, not an off-air capture -
`tests/fixtures/ft8/captured/` exists and holds only a README recording that the
radio is on another computer. It is a clean synthetic the port already reads
whole, so **the gain is invisible on it**, and that is the honest reading rather
than a disappointment. What the comparison does show on that slot is the
mechanism: from the same 55 candidates, **Deep carries 27 through to text where
the port carries 9.**

**3. What a sidecar now says about which decoder read it**, quoted:

```
decoder    Ft8Sharp.Deep with fine sync and ordered statistics
```

And on a slot that gave up nothing readable, the census line on screen now ends
`, read by Ft8Sharp.Deep with fine sync and ordered statistics` - which is exactly
the moment he wants to know what had already been tried on it.

## 4. What's blocking us

**Nothing is blocking.** The step sequence is paused by Tim's ruling and no
successor unit was authored.

One item, which asks for no ruling:

> **Ordered statistics re-encodes 192,602 times on one slot of clean synthetic
> audio, and nothing bounds that number.**
>
> Measured in task 1: `Ft8DeepOsdCounts { Offered = 46, Produced = 46,
> Accepted = 4, Reencodings = 192602 }`. It is well inside the budget here, 210 ms
> of 15,000, and it is the one figure in this unit not obviously bounded by
> anything. The order is 2 and the count is combinatorial in the order; what
> varies is how many candidates fail the cheap path, which is exactly what a
> crowded evening on 14.074 changes.
>
> The alternatives: leave it, on the grounds that 1.74 per cent of the budget has
> room for a lot of multiplication; cap the re-encodings per slot, which would
> make the decoder's reach depend on how busy the band is and do so silently; or
> measure it on a real off-air capture before deciding, which needs a capture
> fixture nobody has.
>
> **It is not treated as a defect** - no measurement says it is one, and
> `Ft8Sharp.Deep` is parked for this unit in any case. It is raised because the
> shack machine is a different computer and this is the number most likely to
> behave differently there.

### Asks still outstanding

1. **Ordered statistics' unbounded re-encoding count** - raised above,
   2026-09-05. **New.**
2. **The decoded panel's family colour** - unit 241, 2026-09-04. The markup says
   `Lavender`; the sender field is green.
3. **The waterfall's dropped window and its late first row** - unit 240,
   2026-09-03.
4. **`ReusableWindow`'s borrowed buffer** - unit 239, 2026-09-03.
5. **`ProcessDelayForTests` as a hook or a seam** - unit 238, 2026-09-03.
6. **The tap's owner** - parked since work instruction 238.
7. **Unit 237's Extensible-format conclusion** - the fix stands, the exoneration
   does not (FACT-004).
8. **Work instruction 231's four tree items.**
9. **`validate-output.bat`'s permitted-spellings bug** - parked by this
   instruction and not touched. Rule 7 was added beside it without disturbing it.
