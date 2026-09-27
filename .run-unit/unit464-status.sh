#!/bin/sh
# usage: sh .run-unit/unit464-status.sh STATE "n of m" BALL "note"
f=/c/Source/HamLet/PROJECT_STATUS.md
now=$(date +%Y-%m-%dT%H:%M:%S%:z)
cat > "$f" <<EOF
PROTOCOL: 2
PROJECT: Hamlet
STATE: $1
TASK: $2
WORK_INSTRUCTION: 464 - a number on every letter: both decoders give each character a confidence p, calibration measured per condition on the keyed corpus
BALL: $3
NEXT_PASTE: none
RULES_AT: HM-DEC-165 (2026-09-19)
UPDATED: $now
NOTE: $4

---

Written by a Claude Code session per CLAUDE.md 13 and ANNUNCIATOR.md.

PROTOCOL names which protocol this header is written against. The long form,
STATUS_PROTOCOL.md, lives in the annunciator repository and is not in this
one, so nothing here can check conformance to it -- the field says what the
file was written to, not that anybody validated it.
EOF
echo "status $now"
