# ARBITER.md - the seed fixture's stub

`run-phase.bat` refuses to call an arbiter where a root has no `ARBITER.md`, so the
no-seed arm needs this file. The arbiter itself is the fixture's stand-in `claude.bat`,
which writes nothing: the no-seed arm therefore runs the same shipped instruction, one
arbiter call later. That call is what the fixture counts.
