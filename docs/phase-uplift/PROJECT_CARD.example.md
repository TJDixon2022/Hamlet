PROTOCOL: 2
PROJECT: ClaudeProjectStatus
ID_PREFIX: CPS
ONE_LINE: One-screen annunciator panel reading PROJECT_STATUS.md from the owner's concurrent projects, and the home of the shared automation and rules every project consumes.
REPO_PATH: C:\Source\ClaudeProjectStatus
REMOTE: https://github.com/TJDixon2022/ClaudeProjectStatus.git
TRUNK: main
PHASE: Automate the Claude Web to Claude Code back to Claude Web loop so that multiple work instructions run without manual intervention
PHASE_SET: 2026-08-28
BRANCH_POLICY: trunk-only
PRIME_DIRECTIVE: The panel never shows a state it did not read. Every lamp traces to a field in a file or a measurement of the disk. A reading that is absent, unparseable, stale, or refused is displayed as unknown — never as healthy, never as blank.
PRIME_TEST: Could the owner, acting on what the panel shows, paste into the wrong window, trust a session that has died, or believe the platform is up when the panel never reached it, because the panel was more confident than its input justified?
GROUND_TRUTH: none
HAZARD: None directly. Indirect and the reason the project exists: a wrong reading here can route a paste into a project that controls megawatt-scale load banks.
RATINGS: none
BUILD_CMD: none — single file, no build step, no dependencies
TEST_CMD: node tools/tests/run.js
RUN_CMD: Open app\PROJECT_ANNUNCIATOR.html in Chrome or Edge
BASELINE_RED: 0 — 1267 checks
PAIRED_WITH: none
LISTING: repo_listing.txt
VENDOR_DOCS: docs\vendor\
UPDATED: 2026-08-28T18:23:27-04:00

---

Standing facts. **Changed only by ruling, recorded in `CLAUDE.md` §1.** A session
does not update this file during a work order; if a fact here is wrong, that is a
decision ask, not an edit.

`PRIME_DIRECTIVE` and `PRIME_TEST` are held verbatim against `CLAUDE.md` §0.0. If
they diverge, `CLAUDE.md` wins and this file is in error.

`UPDATED` carries a midnight time because a chat session cannot read the clock on
the owner's machine and will not assert one it did not measure.
