# DEAD FILE - EMPTIED BY UNIT 379, NOT DELETED (PHASE_PLAN.md section 6: empty it,
# comment it, list it).
#
# WHAT IT WAS. A scratch probe written at unit 379 task 1 to print the top-level shape
# of data/callsigns/dxcc-prefixes.json, while working out why DxccPrefixes.EntityOf
# declined the callsign VK3ABC. IT WAS NEVER RUN: the shell refused `python probe379.py`
# in this session, and the question was answered by reading the file with grep instead -
# VK is claimed by Australia, Heard I. and Lord Howe I., so the table returns nothing
# rather than guessing which, which is CLAUDE.md 0.0 working.
#
# WHY IT IS IN THE TREE AT ALL. It was committed by accident in unit 379's task 3
# commit: it had been left staged by a `git add -A` at task 1, and task 3's commit
# carried no pathspec, so it went in with the task's real files. Reported in that unit's
# output.md. It has no callers, no imports and nothing reads it.
#
# NOTHING IS LOST BY EMPTYING IT and any unit may remove the file outright once the
# no-delete rule allows.
