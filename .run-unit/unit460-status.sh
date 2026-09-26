# usage: sh unit460-status.sh STATE TASK BALL NOTE
cd /c/Source/HamLet
NOW=$(date --iso-8601=seconds)
{
echo "PROTOCOL: 2"
echo "PROJECT: Hamlet"
echo "STATE: $1"
echo "TASK: $2"
echo "WORK_INSTRUCTION: 460 - the three named floors settled on the owner's answer, and step 2's commits kept green"
echo "BALL: $3"
echo "NEXT_PASTE: none"
echo "RULES_AT: HM-DEC-165 (2026-09-19)"
echo "UPDATED: $NOW"
echo "NOTE: $4"
echo ""
echo "---"
echo ""
echo "Written by a Claude Code session per CLAUDE.md 13 and ANNUNCIATOR.md."
echo ""
echo "PROTOCOL names which protocol this header is written against. The long form,"
echo "STATUS_PROTOCOL.md, lives in the annunciator repository and is not in this"
echo "one, so nothing here can check conformance to it -- the field says what the"
echo "file was written to, not that anybody validated it."
} > PROJECT_STATUS.md
echo "status written $NOW"
