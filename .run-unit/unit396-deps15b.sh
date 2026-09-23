cd /c/Source/HamLet
C=src/Hamlet.RadioEngine/Cw
R=.run-unit
P12R=$R/unit396-piece-12-resolved.patch
git diff 56a90616 14155613 -- $C > $P12R
echo "piece 12 as committed, lines $(wc -l < $P12R)"
git diff --stat 56a90616 14155613 -- $C
P3=$R/unit395-piece-3-3e84ac74.patch
sh $R/unit396-deps.sh $R/unit396-piece-15-4c6e4321-nochoice.patch "$P12R" "$P3 $P12R" "$P12R $P3"
