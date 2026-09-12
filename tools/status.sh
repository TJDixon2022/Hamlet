#!/bin/sh
# Writes PROJECT_STATUS.md whole, per CLAUDE.md 13.1. UPDATED is read from the clock.
# Usage: tools/status.sh STATE "TASK n of m" BALL "NEXT_PASTE" "NOTE"
set -e
cat > PROJECT_STATUS.md <<EOF
PROTOCOL: 2
PROJECT: Hamlet
STATE: $1
TASK: $2
WORK_INSTRUCTION: $(sed -n "s/^WORK_INSTRUCTION: //p" PHASE_STATUS.md)
BALL: $3
NEXT_PASTE: $4
RULES_AT: HM-DEC-161 (2026-09-11)
UPDATED: $(date "+%Y-%m-%dT%H:%M:%S%:z")
NOTE: $5

---

Written by a Claude Code session per CLAUDE.md 13 and ANNUNCIATOR.md.

PROTOCOL names which protocol this header is written against. The long form,
STATUS_PROTOCOL.md, lives in the annunciator repository and is not in this
one, so nothing here can check conformance to it -- the field says what the
file was written to, not that anybody validated it.
EOF
