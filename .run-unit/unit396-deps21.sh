cd /c/Source/HamLet
R=.run-unit
P1=$R/unit395-piece-1-2068f868.patch
P3=$R/unit395-piece-3-3e84ac74.patch
P12R=$R/unit396-piece-12-resolved.patch
sh $R/unit396-deps.sh $R/unit396-piece-21-62262b94.patch "" "$P1" "$P3" "$P12R" "$P1 $P3" "$P3 $P12R" "$P1 $P3 $P12R"
echo "== names the piece uses, in HEAD"
grep -c "_reReadAt\|_lastMeasuredForReRead" src/Hamlet.RadioEngine/Cw/CwDecoder.cs
grep -c "void Restart" src/Hamlet.RadioEngine/Cw/CwProbabilisticStream.cs
