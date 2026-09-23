cd /c/Source/HamLet
# Usage: sh .run-unit/unit396-drop.sh <rev to restore from> <file under Cw> "<message>"
git checkout $1 -- src/Hamlet.RadioEngine/Cw/$2
echo "restore rc $?"
git diff --stat HEAD -- src
sh .run-unit/unit396-commit.sh "$3" "$(cat .run-unit/unit396-task.txt)" "task 2: dropped a hunk already in the tree, rebuilding" src/Hamlet.RadioEngine/Cw/$2
