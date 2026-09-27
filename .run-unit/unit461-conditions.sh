#!/bin/sh
# unit 461 - task 3's printout: the point, pooled and verdict lines of TheChannelConditionsAreReadFact.
cd /c/Source/HamLet || exit 1
{
  echo "UNIT 461 TASK 3 - ONE READING OF HM-REQ-013, 040 AND 041 ON THE CH-* CONDITIONS"
  echo "Printed by TheChannelConditionsAreReadFact from .run-unit/unit461-conditions-t3.txt."
  echo "TX-ITU 20 wpm, SyntheticCq.Text, pitch 600 Hz, seeds 461400 + 10 x profile row + 0..2."
  echo "SNR in the 2500 Hz reference. One point each, not a sweep: no floor is claimed (V-05)."
  echo
  grep -a -E "^\s*(point|pooled|verdict) \|" .run-unit/unit461-conditions-t3.txt | sed -E "s/^\s+//"
  echo
  echo "V-04 TRACE OF THE CH-AWGN +15 dB OPENING (seed 461490), ours beside the fldigi port, from .run-unit/unit461-awgn-trace-t3.txt:"
  grep -a -E "^\s*trace \|" .run-unit/unit461-awgn-trace-t3.txt | sed -E "s/^\s+//"
} > .run-unit/unit461-conditions.txt
cat .run-unit/unit461-conditions.txt
