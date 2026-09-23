cd /c/Source/HamLet
sh .run-unit/unit396-clean.sh CwJointCutter.cs
R=.run-unit
P1=$R/unit395-piece-1-2068f868.patch
P2=$R/unit395-piece-2-6fc36a1e.patch
P3=$R/unit395-piece-3-3e84ac74.patch
P5=$R/unit395-piece-5-9de394da.patch
P6=$R/unit395-piece-6-3d4694e5.patch
P7=$R/unit395-piece-7-7fb89d5e.patch
P8=$R/unit395-piece-8-1bf4372d.patch
P9=$R/unit395-piece-9-44cf3fc8.patch
P10=$R/unit396-piece-10-4786c7e7.patch
P11=$R/unit396-piece-11-f2e1db7a.patch
P12=$R/unit396-piece-12-f27174b5.patch
sh $R/unit396-deps.sh $R/unit396-piece-13-386fdb5d.patch "" "$P1" "$P2" "$P3" "$P5" "$P2 $P3" "$P1 $P2 $P3" "$P1 $P2 $P3 $P5 $P12" "$P1 $P2 $P3 $P5 $P6 $P7 $P8 $P9 $P10 $P11 $P12"
