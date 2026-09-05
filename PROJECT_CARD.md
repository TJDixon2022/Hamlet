PROJECT: Hamlet
ONE_LINE: A C# MVVM desktop application controlling an Icom IC-7300 over one USB cable — CW send and receive, scanning, digital-mode decoding — Tim's own project, headed for GPL-3.0 release
REPO_PATH: C:\Source\HamLet
REMOTE: https://github.com/TJDixon2022/Hamlet.git
TRUNK: main
PHASE: Everything this project has built reaches the operator's screen, and the decoder is taken as far as it will go
PHASE_SET: 2026-09-05
TEST_CMD: dotnet test

---

## What this file is

Standing facts about Hamlet, read by the panel that shows every project on one
screen. Eight lines, and the reader takes the leading run of `KEY: value` lines and
stops at the `---` above, so nothing below here is read by anything.

`ONE_LINE` is what tells two similarly-named projects apart at a glance, and it is
taken from this project's own `CLAUDE.md` header rather than composed. `REPO_PATH`,
`REMOTE` and `TRUNK` are measured from git. `TRUNK` is what makes the off-trunk
check possible: without it the panel can see the branch but cannot say whether it
is the wrong one.

**This file is changed only by ruling** (§13.3). It holds standing facts, so a
value that has gone wrong is a ruling rather than an edit, and it is never touched
during a work order.

The rules it answers to are `ANNUNCIATOR.md`, summarized inline in `CLAUDE.md` §13
so that a session reading only `CLAUDE.md` still knows them (HM-DEC-132).
