#!/bin/sh
# unit 429 - copy unit 428's run scripts, renamed; the source diff against entry f94886a1.
cd /c/Source/HamLet/.run-unit || exit 1
for s in build cf round run commit alone transmit srcdiff fullbuild types cwtypes entry rows cmprows cmptrace same
do
  sed "s/unit428/unit429/g; s/unit 428/unit 429/g" unit428-$s.sh > unit429-$s.sh
  echo "copied $s"
done
sed -i "s/9db61107/f94886a1/g" unit429-srcdiff.sh
