cd /c/Source/HamLet
# Usage: sh .run-unit/unit395-drop.sh <rev to restore from> <file under Cw> "<message>"
git checkout $1 -- src/Hamlet.RadioEngine/Cw/$2
echo "restore rc $?"
git diff --stat HEAD -- src
sh .run-unit/unit395-commit.sh "$3" "TASK 2 of 4" "task 2: dropped a hunk already in the tree, rebuilding" src/Hamlet.RadioEngine/Cw/$2
