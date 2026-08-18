PROTOCOL: 2
PROJECT: Hamlet
ID_PREFIX: HM-DEC- for rulings, HM-OPEN- for questions
ONE_LINE: A C# MVVM desktop application that controls an Icom IC-7300 over its single USB connection — CW send and receive, frequency control and scanning, digital-mode decoding, and a waterfall UI — Tim's own project, own time, own money, intended for public release under GPL-3.0.
REPO_PATH: C:\Source\HamLet
REMOTE: https://github.com/TJDixon2022/Hamlet.git
TRUNK: main
BRANCH_POLICY: trunk-only (main). No session creates or works on a branch without Tim's ruling; a work order needing isolation says so and stops (§9.5.1, HM-DEC-113).
PRIME_DIRECTIVE: Never present a guess as a decode. What Hamlet shows on screen is what was actually on the air. Uncertainty is displayed as uncertainty — a low-confidence character is marked, a failed decode is silence, and a mode identification below threshold says "unknown", not its best guess dressed as an answer.
PRIME_TEST: Practical test: could the operator, acting on what the screen says, be wrong because the app was more confident than its input justified? If yes, the display is wrong regardless of how much cleaner it looks.
GROUND_TRUTH: SHACK_FACTS.md — standing facts about the operator's own station. A fact there outranks any inference a session draws from its own reading (HM-DEC-093).
HAZARD: Keys an Icom IC-7300 HF transmitter (100 W) via CI-V, including an unattended repeating cycle — dummy load only until §0.2's first sentence is amended by separate ruling (HM-DEC-098, HM-DEC-008). Also commands the operator's VFO under §0.2.1.
RATINGS: IC-7300, 100 W HF/50 MHz. CW pitch 300–900 Hz in 5 Hz steps (14 09). Keyer message ≤30 characters (17). CI-V 115200, address 94h. IF filter 50 Hz–3.6 kHz. Every figure per Full Manual A7292-4EX-6; a page number is a page in that edition and no other.
BUILD_CMD: dotnet build
TEST_CMD: dotnet test
RUN_CMD: dotnet run --project src/Hamlet.App
BASELINE_RED: 2 — ClearingTheTranscriptLeavesTheDecoderAlone (would pass with HM-DEC-116, which HM-DEC-128 superseded) and TheBulletinDecodesToItsAnswerKey (the standing bar on a real off-air recording, 30 correct of 44). Measured 2026-08-18.
PAIRED_WITH: none
LISTING: none committed. Generated on demand by tools/repo-listing/get-listing.bat for chat sessions (§9.3); a Claude Code session reads the tree directly (§9.0), and a stale listing is worse than none.
VENDOR_DOCS: data/vendor/ (pinned snapshots of cited outside documents, §4) and data/privileges/ (cited Part 97 privileges, HM-DEC-029). The IC-7300 Full Manual is cited by page and never committed — Icom permits individual use and prohibits redistribution (§2.1, HM-DEC-049).
UPDATED: 2026-08-18T18:45:18+00:00

---

## What this file is

Standing facts about Hamlet: the ones that are true between sessions and do not
change when work does. A session reads this to learn where it is, what the
project can do to the physical world, and what it may never infer, without
reading the whole of `CLAUDE.md` first.

**It is not a second copy of the rules.** Everything here is either measured from
the tree or quoted from `CLAUDE.md`, and where the two ever disagree `CLAUDE.md`
wins and the disagreement is an open issue rather than a judgement call. The
prime directive and its practical test are quoted verbatim from §0.0 rather than
paraphrased, for the same reason a quotation that has been tidied is no longer a
quotation.

**`HAZARD` and `RATINGS` are the two fields worth reading twice.** This project
keys a hundred-watt transmitter, and as of 2026-08-18 it does so on an unattended
repeating cycle. §0.2 is absolute, HM-DEC-098 confines that cycle to a dummy
load, and whether it ever reaches an antenna is a separate ruling Tim takes after
watching every interlock fire into the load. Nothing in this file amends that and
nothing in this file may be read as amending it.

`RATINGS` carries its edition because a page number is only as good as the
printing it came from: this table used to span three of them and that seam
produced two defects (HM-DEC-071). A figure the manual does not state is an
explicit known-unknown and is never filled in with a plausible number (§4, §12.4).

## The protocol it names has not arrived

`PROTOCOL: 2` refers to `STATUS_PROTOCOL.md`, which **is not in this repository
yet.** It is being revised elsewhere and will be delivered here later.

So the header is a statement of which protocol this file is written against, and
**not a claim of conformance that anybody here can currently check.** Read it that
way until the protocol lands, and when it does, this file and
`PROJECT_STATUS.md` are checked against it rather than assumed to already agree.
Saying so plainly costs a paragraph; a header nobody can verify, presented as
though somebody had, is the same fault as a decode with no signal behind it.
