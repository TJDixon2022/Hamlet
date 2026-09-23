cd /c/Source/HamLet
# Usage: sh .run-unit/unit396-show.sh <rev> - the stat of one commit
git show --stat --format="%h %s" $1 | cut -c1-160
