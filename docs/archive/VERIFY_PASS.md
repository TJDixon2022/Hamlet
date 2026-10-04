# The verification pass — run before anything chains

**A guard is not a guard until it has refused something.** Every one of these
depends on paths and a test command that changed when this delivery landed.
`PHASE_UPLIFT.md` §1 is a list of defects that were invisible in the source and
appeared only when something ran.

Run from `C:\Source\HamLet` in **cmd.exe**, not PowerShell. Check the exit code
after each with `echo %ERRORLEVEL%`.

## The lock

```
tools\arbiter\lock.bat status
tools\arbiter\lock.bat take
tools\arbiter\lock.bat take
tools\arbiter\lock.bat release
```

Expected: `0` free, `0` taken, **non-zero on the second take — this is the
refusal**, `0` released. A second take that succeeds means two units could run at
once.

## The readers

```
tools\arbiter\rules-at.bat
tools\arbiter\watchdog.bat C:\Source\HamLet
tools\arbiter\validate-output.bat C:\Source\HamLet\output.md
tools\arbiter\outcome-read.bat C:\Source\HamLet
```

Expected: `0` matches or `1` behind; `0` fresh, `1` stale, `2` unreadable; **`0`
— your real report must pass**; `0` read.

## The refusals

```
ren PHASE_OUTCOME.md PHASE_OUTCOME.hold
tools\arbiter\outcome-read.bat C:\Source\HamLet
ren PHASE_OUTCOME.hold PHASE_OUTCOME.md

tools\arbiter\run-unit.bat 001 %TEMP%

tools\arbiter\lock.bat take
tools\arbiter\run-phase.bat C:\Source\HamLet
tools\arbiter\lock.bat release
```

Expected: **`2` not `1`** for the absent outcome file — a phase that has not
started is not a malformed one. **`7`** for a non-git directory, returned before
the lock is taken. **`3`** for a held lock, refused without launching.

**A check for merely non-zero passes two of these while proving neither.** Read
the actual number.

## The suite

Run `dotnet test` before and after. The phase layer touches no application code,
so the count must not move. If there is a baseline of known failures, write the
number down first.

## What to report back

The exit code from each line, and the pass/fail count before and after. **If any
guard returns `0` where a refusal was expected, stop** — that guard is not
guarding, and the loop must not chain behind it.
