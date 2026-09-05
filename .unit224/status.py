"""Rewrite PROJECT_STATUS.md's header for unit 224.

Usage: python .unit224/status.py <state> <task-n-of-m> <ball> <demote-key-or-"-"> <note-file>

The current NOTE line is demoted to <demote-key> (kept as history the way every
prior unit's notes are kept in this file) and the new NOTE is read from the file
so a long line does not have to survive a shell.  UPDATED is read from the clock
and never composed.
"""
import io
import sys
from datetime import datetime, timezone

state, task, ball, demote, note_file = sys.argv[1:6]

path = "PROJECT_STATUS.md"
lines = io.open(path, encoding="utf-8").read().split("\n")

note = " ".join(io.open(note_file, encoding="utf-8").read().split())

out = []
for line in lines:
    if line.startswith("NOTE: ") and demote != "-":
        out.append(demote + ": " + line[6:])
    elif line.startswith("NOTE: "):
        pass
    else:
        out.append(line)
lines = out


def setf(key, value):
    for i, line in enumerate(lines):
        if line.startswith(key + ": "):
            lines[i] = key + ": " + value
            return
    raise SystemExit("missing field " + key)


setf("STATE", state)
setf("TASK", task)
setf("WORK_INSTRUCTION", "224")
setf("BALL", ball)
setf("NEXT_PASTE", "none" if ball == "code" else "output.md -> Claude Web")
setf("UPDATED", datetime.now(timezone.utc).astimezone().isoformat(timespec="seconds"))

for i, line in enumerate(lines):
    if line.startswith("PRIOR_"):
        lines.insert(i, "NOTE: " + note)
        break

io.open(path, "w", encoding="utf-8", newline="\n").write("\n".join(lines))
print("\n".join(lines[:9]))
