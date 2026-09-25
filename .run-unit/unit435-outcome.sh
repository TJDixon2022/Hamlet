#!/bin/sh
# unit 435 - append the decision block at the foot of WORK_INSTRUCTIONS.md to PHASE_OUTCOME.md, CRLF as the file is.
cd /c/Source/HamLet || exit 1
printf '\r\n## UNIT 435 - STEP 7\r\n\r\n' >> PHASE_OUTCOME.md
sed -n '/^ARBITER-DECISION/,/^END-ARBITER-DECISION/p' WORK_INSTRUCTIONS.md | tr -d '\r' | grep -v -E "ARBITER-DECISION" | sed 's/$/\r/' >> PHASE_OUTCOME.md
tail -n 12 PHASE_OUTCOME.md | cut -c1-110
