cd /c/Source/HamLet
git diff --stat HEAD -- src/Hamlet.RadioEngine/Cw/CwPitchChoice.cs
echo "choice diff against HEAD after 3way end"
git checkout HEAD -- src/Hamlet.RadioEngine/Cw
C=src/Hamlet.RadioEngine/Cw
R=.run-unit
T=$R/unit396-piece-19-0f2089f3-nochoice.patch
git diff 0f2089f3^ 0f2089f3 -- $C/CwDecodeReport.cs $C/CwDecoder.cs > $T
echo "patch lines $(wc -l < $T)"
P3=$R/unit395-piece-3-3e84ac74.patch
P12R=$R/unit396-piece-12-resolved.patch
P15=$R/unit396-piece-15-4c6e4321-nochoice.patch
P18=$R/unit396-piece-18-b48d1158.patch
sh $R/unit396-deps.sh $T "" "$P3" "$P12R" "$P18" "$P3 $P12R" "$P3 $P12R $P15" "$P3 $P12R $P15 $P18"
