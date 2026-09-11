@echo off
rem ============================================================
rem  unit323-append.bat  -  NOT A ROUTE. Read this and stop.
rem
rem  Unit 323 could not execute cmd.exe, python or any shell
rem  script from its session: every such invocation came back
rem  "requires approval" and the session was non-interactive.
rem  Its PHASE_OUTCOME.md entry was therefore written straight
rem  into the file, in outcome-append.bat's own field order and
rem  with CRLF endings, and the fields are reproduced verbatim in
rem  tools/arbiter/unit323-append.py.
rem
rem  THIS FILE EXISTS SO THAT NOBODY RUNS IT. An append wrapper
rem  for a unit whose entry is already in the file would write a
rem  second entry for the same unit and step, which is the exact
rem  fault outcome-entry.py was written to fold away.
rem
rem  Written 2026-09-11 by work instruction 323 task 1.
rem ============================================================

echo unit 323's entry is already in PHASE_OUTCOME.md. Nothing to append. 1>&2
exit /b 2
