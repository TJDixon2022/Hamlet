# tools/repo-listing

`get-listing.bat` produces `repo_listing.txt`: every file in the repository
with its size and modified date, plus the current commit, branch, and how many
files are dirty.

**Run this first.** It is the bootstrap for everything in `CLAUDE.md` §9.

Carried from Tim's simulator project 2026-08-12 with the default repo root
changed to `C:\Source\Hamlet`. Logic is otherwise identical.

---

## Why it comes first

Claude cannot ask for a file it does not know exists, and cannot deliver into
a folder structure it has not read. Without a listing:

- `get-files.bat` blocks are guesses, and a guessed path returns `MISSING`
- scaffolded delivery places files where Claude assumes they go
- a session confidently describes a structure it has never seen

## How Tim uses it

Save to Downloads. Double-click. Upload the `repo_listing.txt` it produces.

Default repo root is `C:\Source\Hamlet`. Override it by passing a path as
the first argument. A trailing backslash is accepted.

## When to re-run it

Whenever files are added, removed or moved. **A stale listing is worse than no
listing** — it produces confident requests for paths that no longer exist,
and the failure looks like a script bug rather than a stale input.

The header carries the commit hash. Compare it against `git rev-parse HEAD`
to tell whether the listing still matches the tree.

## What is excluded

Build output and caches: `bin`, `obj`, `node_modules`, `packages`,
`TestResults`, `coverage`, `dist`, `.git`, `.vs`, `graphify-out`.

Everything else is listed.

**This list is duplicated in `tools\get-files\get-files.template.bat` as the
`XD` variable and the two must agree.** If they diverge, a folder can be
pulled into a zip that was never listed, or listed and silently dropped from
the zip, and the arrival check in §9.4 reports differences that are not real.
Change one, change the other, in the same delivery.

The footer reports `Files excluded` as well as `Files listed`. That count is
the evidence the filters actually fired — a zero there means everything got
through.

## Why the exclusions are not `findstr`

They were, in the parent project, and it was a silent risk.
`findstr /i /c:"\bin\"` cannot be written directly because a backslash before
the closing quote escapes it, and the doubled form that does parse depends on
how the C runtime strips backslashes before `findstr` ever sees the argument.
A filter that looks correct can match nothing and let every build artifact
into the listing, with no visible symptom.

The exclusions are plain substring tests using cmd's own case-insensitive
replacement, against the relative path with a leading backslash prepended so
a folder at the repository root is caught as well as a nested one.

## Traps carried from real use

- A trailing backslash on the argument used to make every path in the listing
  absolute instead of relative. Normalised now.
- `Total bytes` is computed with `set /a` and is 32-bit. Past about 2.1 GB it
  wraps and shows negative.
- Delayed expansion means a file path containing `!` will have its row
  mangled.
