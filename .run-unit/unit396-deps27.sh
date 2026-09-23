cd /c/Source/HamLet
git status --short -- src
R=.run-unit
P1=$R/unit395-piece-1-2068f868.patch
P2=$R/unit395-piece-2-6fc36a1e.patch
P5=$R/unit395-piece-5-9de394da.patch
P17=$R/unit396-piece-17-f9c11989.patch
P25=$R/unit396-piece-25-71b4f044.patch
sh $R/unit396-deps.sh $R/unit396-piece-27-a91d8fe7.patch "" "$P25" "$P2 $P25" "$P2 $P5 $P25" "$P1 $P2 $P5 $P17 $P25"
