# tools/get-files

`get-files.template.bat` is the **canonical** script Claude uses to ask Tim
for files. `CLAUDE.md` §9.4 requires it to be copied verbatim.

It lives here rather than only in project knowledge because project knowledge
goes stale and a Claude Code session reads this repository from disk.

Carried from Tim's simulator project 2026-08-12 with the default repo root
changed to `C:\Source\HamManager` and the file-list block reset. Subroutines,
staging paths, output name, zip mechanism and `XD` exclusion list are
unchanged.

---

## How Claude uses it

1. Copy `get-files.template.bat` verbatim.
2. Replace **only** the block marked `<<< REPLACE THIS BLOCK EVERY TIME >>>`.
3. Fill in the `Generated <date> for: <row>` line in the header.
4. Deliver it as `get-files.bat`.

Nothing else changes. Not the subroutines, not the staging paths, not the
output name, not the zip mechanism, not the `XD` exclusion list.

**Check the paths against `repo_listing.txt` before writing the block.** A
request for a file that does not exist costs a round trip and teaches nobody
anything.

## How Tim uses it

Save to Downloads. Double-click. Upload the `for_claude.zip` it produces.

Default repo root is `C:\Source\HamManager`. Override it by passing a path as
the first argument. A trailing backslash is accepted.

It is not run from the repository root and not run from a PowerShell prompt.
Both have been tried and both fail: the script is not in the repo, and
PowerShell does not execute from the current directory without `.\`.

## Rules for the file-list block

- `:adddir` takes a folder, recursive. `:add` takes one file.
- Paths are relative to the repo root and use backslashes.
- **Prefer `:adddir`.** §9.4 requires whole folders when the work touches a
  subsystem — a partial mirror has produced delivered defects repeatedly.

## Exclusion parity — do not let these drift

The `XD` variable holds the folders robocopy skips. It is deliberately
identical to the exclusion list in `tools\repo-listing\get-listing.bat`.

They must agree. If they do not, a folder can be pulled into the zip that was
never in the listing, or listed and silently dropped from the zip — and the
arrival check in §9.4, which compares the zip against `repo_listing.txt`,
then reports differences that are not real. A check that cries wolf is worse
than no check.

**Change one, change the other, in the same delivery.**

## Traps that have already cost rounds (in the parent project)

- Zip with `Compress-Archive`, never `tar -a -c -f ... -C dir .` — tar
  prefixes every entry `./` and Explorer renders the result as an empty
  folder.
- In `:add`, the `for`/`if` creating the parent directory must be
  parenthesised across lines. On one line, cmd mis-parses it for a root-level
  path and reports "cannot find the batch label specified - add".
- `robocopy` exit codes below 8 are success. Only `if errorlevel 8` is a real
  failure.
- Quote every path. Escape `(`, `)` and `>` in `echo` with `^`.
- A `.ps1` will not run on this machine — unsigned scripts are blocked by
  execution policy. Batch, always. The inline `powershell -Command` used for
  zipping is not a script file and is not affected.
- A wrong default repo root makes every double-click fail before doing
  anything. This copy's default was set to `C:\Source\HamManager` at port
  time — if the repo ever moves, fix it here and in `get-listing.bat` in the
  same delivery.

## On arrival

Extract, count files, check sizes against `repo_listing.txt`. A `MISSING`
line is loud on purpose — do not build past it.
