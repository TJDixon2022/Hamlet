cd /c/Source/HamLet
grep -n -e "^| # |" -e "^| 51 " -e "^| Type | File added" -e "^| .ThePitchCanBeHeldTests. | 2026" -e "^| Run | Entry" -e "^| Eleven transmit" docs/phase-cw/unit394-reds.md
git diff --quiet HEAD -- output.md
echo "output.md dirty rc $?"
date "+%Y-%m-%d %H:%M"
