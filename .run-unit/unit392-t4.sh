cd /c/Source/HamLet
C=src/Hamlet.RadioEngine/Cw
TX="$C/CwTransmitter.cs $C/KeyerCwSender.cs $C/TransmitChain.cs $C/AutoCall.cs $C/AutoCallAnswers.cs $C/CwTransmitGuard.cs $C/TransmissionWatch.cs $C/TransmitReadiness.cs $C/TransmitPrivileges.cs $C/TransmitNotes.cs $C/ICwSender.cs"
echo "== app diff stat 10512248..HEAD"
git diff --stat 10512248 HEAD -- src/Hamlet.App
echo "== app hunks"
git diff -U0 10512248 HEAD -- src/Hamlet.App | grep "^@@"
echo "== transmit 7e209cb4..HEAD"
git diff --stat 7e209cb4 HEAD -- $TX
echo "(end transmit)"
echo "== Cw diff stat 7e209cb4..HEAD"
git diff --stat 7e209cb4 HEAD -- $C
echo "== Cw hunks"
git diff -U0 7e209cb4 HEAD -- $C | grep -E "^(\+\+\+|@@)"
echo "== all of src and tests vs entry"
git diff --stat 10512248 HEAD -- src tests Hamlet.sln | tail -3
