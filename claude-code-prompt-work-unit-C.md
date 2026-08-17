PROJECT: CoreHMI

# CoreHMI Work Unit C — accept the 144-address map, consume nothing new

**Project gate.** CoreHMI (C:\Source\SharedRepo) only. In any other
repository, stop and say so; execute nothing.

**Repository:** SharedRepo, branch `feature/corehmi-modbus-machine-link`.
**Authority:** this prompt, `CLAUDE.md`, the project rules.

The simulator's SIM-DEC-108 added a read-only application-state block at
holding register 100 — 27 quantities (mode, alarms, cycle, GLS, recording,
load-on timer). The re-vendored map now carries 144 addresses. This unit
makes CoreHMI's map handling accept and read that block. It deliberately
does NOT publish any of it to the client-facing snapshot: what CoreHMI
consumes from the block is governed by Work Unit A's open rulings 2 and 5,
which remain open, and by DEC-061's ownership question, which is Tim's and
Bill's. Loader accepts, poll reads, diagnostics show, snapshot unchanged.

Decisions already made — do not reopen:

- **The client-facing contract does not change.** `ReCapLoadBank.Contracts/`
  and `openapi.json` stay byte-identical to their current state; verify with
  `git diff` at the end and say so in the report.
- **`BrokeredState` is not touched.** Moving any of its tenants onto the
  wire is exactly what the open rulings decide; pre-empting them here is
  forbidden even where the new block makes it easy.
- The new block is **read-only, enforced by the simulator with Modbus
  exception 02**. CoreHMI never writes at or above base 100, and the
  existing setpoint write (four registers from base 0) must be shown by
  test to not straddle into it.
- Commits on the existing feature branch, `Refs: GAP-05`.

## 0. Preflight

1. Branch `feature/corehmi-modbus-machine-link`, working tree clean of
   product-code changes (name and leave anything of Tim's in flight, as
   Work Unit B did).
2. `data/r256874-1.modbus-map.json` must be the re-vendored copy: it must
   parse, and its document block and entries must describe 144 addresses
   including a holding-register block based at 100. If the map still holds
   117, **stop** — the re-vendor has not happened; say so.
3. Read the map's own document block for the new block's convention (word
   sizes, order, bit-order source for alarm words). Generate from the rule,
   never transcribe rows.
4. If you need a file you do not have, say so and stop.

## Phases — ordered, each ends green: builds, all tests pass, one commit

**Phase 1 — loader and constants accept 144.** Extend the map loader's
validation to the new convention (counts, the block at 100, its read-only
marking) and regenerate the constants file from the map. A 117-address map
must now fail validation loudly as stale, exactly as a corrupted one does.
Tests updated accordingly.

**Phase 2 — poll reads the block; diagnostics show it; snapshot does not.**
Add the application-state block to the poll cycle as one additional read
transaction, decoded per the map, surfaced only on CoreHMI's diagnostics
path (raw quantities with names). The published snapshot is byte-for-byte
unaffected — prove it with a test that asserts the snapshot shape and
content are unchanged with the block present, absent, and unreadable.
An unreadable block (exception or gap) must not degrade the machine
snapshot: it is diagnostics that go dark, nothing else.

**Phase 3 — write-safety proof and report.** A test proving the setpoint
FC16 never straddles base 100; a test proving CoreHMI issues no write at or
above 100 under any command path; full report to output.md per standing
rule, including the git-diff statement on the contract.

**If you run out of room, drop Phase 2's diagnostics surface (keep the
read-path decode and its tests) — say plainly that you dropped it. Never
drop Phase 1.**

## Report — output.md, verbatim, unsummarized

End with both headings. RECORDED — "nothing; sessions do not write records
in this repository." NEEDS A RULING — decision-log format. Expected
candidates: anything in the block's 27 quantities the map's document block
does not fully specify; anywhere the new read changes poll-cycle timing
enough to matter against the 250 ms cadence.

## What not to do

- Do not publish any block quantity into the client-facing snapshot, the
  contract, or `BrokeredState`.
- Do not write at or above holding register 100, ever, including in tests
  against the in-process double.
- Do not hand-edit the vendored map. Stale or wrong means stop and report.
- Do not edit, build, or commit in the simulator repository.
- Do not merge the feature branch.
- Do not write to any `.Bill.md` file or to any decision log.
- If you finish every phase, stop and report. Do not start the next work
  unit.
