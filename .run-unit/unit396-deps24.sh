cd /c/Source/HamLet
R=.run-unit
P1=$R/unit395-piece-1-2068f868.patch
P2=$R/unit395-piece-2-6fc36a1e.patch
P5=$R/unit395-piece-5-9de394da.patch
P13=$R/unit396-piece-13-386fdb5d.patch
P17=$R/unit396-piece-17-f9c11989.patch
sh $R/unit396-deps.sh $R/unit396-piece-24-fc1ee77f.patch "" "$P2" "$P17" "$P2 $P17" "$P2 $P5 $P13 $P17" "$P1 $P2 $P5 $P13 $P17"
