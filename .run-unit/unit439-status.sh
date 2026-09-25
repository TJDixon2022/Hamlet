# usage: sh .run-unit/unit439-status.sh STATE TASK BALL NEXT NOTE
NOW=$(powershell -NoProfile -Command "Get-Date -Format 'yyyy-MM-ddTHH:mm:sszzz'" | tr -d '\r')
{
echo "PROTOCOL: 2"
echo "PROJECT: Hamlet"
echo "STATE: $1"
echo "TASK: $2"
echo "WORK_INSTRUCTION: 439 - trace the tests, then build the metrics (by hand, outside the loop)"
echo "BALL: $3"
echo "NEXT_PASTE: $4"
echo "RULES_AT: HM-DEC-183 (2026-09-25)"
echo "UPDATED: $NOW"
echo "NOTE: $5"
echo
echo "---"
echo
echo "Written by a Claude Code session per CLAUDE.md 13 and ANNUNCIATOR.md."
echo
echo "PROTOCOL names which protocol this header is written against. The long form,"
echo "STATUS_PROTOCOL.md, lives in the annunciator repository and is not in this"
echo "one, so nothing here can check conformance to it -- the field says what the"
echo "file was written to, not that anybody validated it."
} > PROJECT_STATUS.md
echo "$NOW"
