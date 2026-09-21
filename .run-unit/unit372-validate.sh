#!/bin/sh
# Unit 372 task 4: run the report validator against this unit's output.md.
# //c rather than /c: Git Bash rewrites a lone /c into a path.
MSYS_NO_PATHCONV=1 cmd //c "tools\\arbiter\\validate-output.bat" "C:\\Source\\HamLet\\output.md"
