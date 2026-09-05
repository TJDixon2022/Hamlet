PROTOCOL: 2
PROJECT: ClaudeProjectStatus
STATE: COMPLETED
TASK: 7 of 7
WORK_INSTRUCTION: 054 - is the loop turning
PROMPT: 4
BALL: tim
NEXT_PASTE: output.md -> Claude Web
DROP_CANDIDATE: 7 - the launcher writes the beat, DROPPED whole
OPEN_QUESTIONS: 2
OPEN_ITEMS: 10
RULES_AT: CPS-DEC-068 (2026-08-31)
TESTS: 1267 pass / 0 fail
ASSUMPTIONS: 0
SESSION_SURFACE: code
UPDATED: 2026-08-31 12:09
NOTE: six of seven tasks done and pushed; the loop reading is on the card and proved, and the launcher's write is dropped whole because run-phase.bat is the file cmd.exe is executing and %TEMP% is outside this session's reach.

---

Governed by `STATUS_PROTOCOL.md` §7. The header above is the whole file as far as
any reader is concerned.

**`UPDATED` was read from the machine clock at every write.** This session's
values: `11:53`, `11:54`, `11:56`, `12:01`, `12:02`, `12:04`, `12:09`. Each was
read immediately before it was written and none was estimated from how much work
had been done. 052 composed seven consecutive values and shipped a file two hours
ahead; that is the failure this note exists against. **One estimate was made and
corrected before delivery:** `output.md`'s `UNIT:` line was first written
`12:06` without reading the clock, and was replaced with `12:09` read from it.

**Step 4's entry criterion was checked, not inherited.** `PHASE_CONTROL.md` §2
says a step verifies its own ground, so step 3's report and its `STATE_AFTER:
done` were not evidence. The suite was run before anything was touched and read
**1183 pass / 0 fail**, exactly what the card claimed, and `phaserender.test.js`'s
78 literal `eq(` calls reconcile one-for-one against the suite's own tally of 78.

**The suite reads 1267 pass / 0 fail at the end**, run twice with identical
results, with 84 new assertions in `tools\tests\heartbeat.test.js`.

**No assertion had to be moved when task 2 changed the phase file.** The
instruction predicted some would go red; none did.

**`OPEN_QUESTIONS: 2`** — `output.md` §4 items 1 and 2, both wanted before a
later unit attempts the launcher's write: that a file `cmd.exe` holds open while
a unit runs is edited outside a loop run, and that there is a named permitted
location for read-back scratch proofs.

**Task 7 is DROPPED, not partially delivered.** The reading is on the card and
proved; nothing writes a beat. The card correctly reads `loop stopped`.

**The `.run-unit\` artifacts, the three modified root files and the two
`scratch-052` directories are found state and were left exactly as found.**

This state is terminal because `output.md` is on disk and was written before this
line.
