cd /c/Source/HamLet
git checkout HEAD -- src/Hamlet.RadioEngine/Cw
git status --short -- src
C=src/Hamlet.RadioEngine/Cw
T=.run-unit/unit396-piece-15-4c6e4321-nochoice.patch
git diff 4c6e4321^ 4c6e4321 -- $C/CwDecodeReport.cs $C/CwDecoder.cs $C/CwToneTracker.cs > $T
echo "patch lines $(wc -l < $T)"
R=.run-unit
P1=$R/unit395-piece-1-2068f868.patch
P2=$R/unit395-piece-2-6fc36a1e.patch
P3=$R/unit395-piece-3-3e84ac74.patch
P5=$R/unit395-piece-5-9de394da.patch
P7=$R/unit395-piece-7-7fb89d5e.patch
P9=$R/unit395-piece-9-44cf3fc8.patch
P10=$R/unit396-piece-10-4786c7e7.patch
P11=$R/unit396-piece-11-f2e1db7a.patch
P12=$R/unit396-piece-12-f27174b5.patch
P13=$R/unit396-piece-13-386fdb5d.patch
sh $R/unit396-deps.sh $T "" "$P1" "$P3" "$P5" "$P7" "$P3 $P12" "$P7 $P9" "$P7 $P9 $P11" "$P1 $P2 $P3 $P5 $P7 $P9 $P10 $P11"
