cd /c/Source/HamLet
sh .run-unit/unit397-clean.sh
grep -c "Cut(" src/Hamlet.RadioEngine/Cw/CwUnitEstimator.cs
P=.run-unit
T=$P/unit397-piece-40-ee2cba8d.patch
sh .run-unit/unit397-deps.sh $T "$P/unit396-piece-31-b7147b1f.patch" "$P/unit396-piece-33-4935a4f8.patch" "$P/unit397-piece-34-e6b1ece7.patch" "$P/unit396-piece-31-b7147b1f.patch $P/unit396-piece-33-4935a4f8.patch" "$P/unit396-piece-31-b7147b1f.patch $P/unit397-piece-34-e6b1ece7.patch" "$P/unit396-piece-31-b7147b1f.patch $P/unit396-piece-33-4935a4f8.patch $P/unit397-piece-34-e6b1ece7.patch"
