PHASE: FT4 works exactly the way FT8 does
PHASE_SET: 2026-09-08
STEP: 0 | done | where FT4 lives, decided by reading
STEP: 1 | not started | FT4 decodes a signal Hamlet made
STEP: 2 | not started | the slot machinery is FT4's
STEP: 3 | not started | the log can say FT4
STEP: 4 | not started | the FT4 button works
STEP: 5 | not started | Tim hears FT4
STEP: 6 | not started | Tim works a station on FT4

---

## What this file is

The phase's memory. `output.md` is overwritten every unit and cannot carry what was
tried; this file survives the unit and records, per unit, the approach taken and what
it hit.

**The header above is a cursor over the entries below, and the entries win.**

Appended by `tools\arbiter\outcome-append.bat`. Five state words and no others:
`not started`, `in progress`, `partial`, `blocked`, `done`.

The send phase's entries are archived at `docs/phase-send-run/PHASE_OUTCOME.md`. What
it built and what is not to be rebuilt: the abort proved from six states, the FT8
composer, the sound card route, the stop that took 8.4 seconds of audio off the air
mid transmission, the loopback reading 3 of 3 messages back as themselves, the
transmit level, the contact ledger and its four row states, the right-click menu, the
ADIF log, the achievements screen, the belt, and the marks. **Tim transmitted on a
live antenna on 2026-09-07 and logged his first contacts.**

## Entries

*None yet. This phase has not started.*

## UNIT 288 - STEP 0

STEP: 0
APPROACH: Read the pinned ft8_lib clone and the port itself rather than deciding from memory, then stood the application up and pressed the FT4 chip through UI Automation.
HIT: There is no file called ft4.c, so the filename test the instruction warns against would have answered wrongly; upstream FT4 lives inside the FT8 files behind a protocol enum. And upstream decode_ft8 -ft4 read 0 messages from upstream gen_ft8 -ft4, which looks like hollow support and is placement: the candidate search runs -10 to 19 blocks, which at 0.048 s is -0.48 to 0.91 s, and the generator centres the transmission at 1.23 s.
MOVE: The port carries FT4. Ten gaps named beyond the decoder and none fixed.
WHY: ft8_lib carries FT4 complete - ft4_encode at ft8/encode.c:127 and four decoder branches at ft8/decode.c:127, 192, 253 and 454 - so the rule Tim ruled on 2026-09-08 decided it and this unit only read. Shifting the identical audio forward 0.99 s returned CQ KC3QIS FN00 at plus 19.5, so the whole upstream chain agrees.
DECIDED: Nothing on the arbiter's authority. The decision was Tim's own rule applied to evidence, and the two questions it raised were handed back rather than settled.
LICENCE: Work instruction 288 tasks 1 to 3, under Tim's ruling of 2026-09-08 that where FT4 lives follows what upstream does.
COST: one session, six tasks, no test suite run, one dotnet build at 9.05 s
ACCOMPLISHED: Where the FT4 decoder goes is answered from the tree with file and line, and what is already shared is counted so nothing is written twice: 28 of 33 port files and 6160 of 7926 lines are protocol-neutral today.
FATE: executed
STATE_AFTER: done
STATE_WHY: All four of step 0's must-pass exits are met: what upstream carries with file and line, what the port already shares, the decision with its reason, and what the 51 fidelity tests would have to become.
