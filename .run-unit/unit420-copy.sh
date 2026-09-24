#!/bin/sh
# unit 420 - copy unit 419's run scripts, renamed.
cd /c/Source/HamLet/.run-unit || exit 1
for s in cf build rig round types rigsum run alone commit srcdiff transmit
do
  if [ -f unit419-$s.sh ]
  then
    sed "s/unit419/unit420/g; s/unit 419/unit 420/g" unit419-$s.sh > unit420-$s.sh
    echo "copied $s"
  fi
done
