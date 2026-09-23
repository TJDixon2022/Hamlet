cd /c/Source/HamLet
# Usage: sh .run-unit/unit397-piece.sh <n> <hash>
# Makes the piece's patch, restricted to src/Hamlet.RadioEngine/Cw minus the eleven transmit files, and applies it.
N=$1
H=$2
C=src/Hamlet.RadioEngine/Cw
P=.run-unit/unit397-piece-$N-$H.patch
sh tools/status.sh EXECUTING "$(cat .run-unit/unit397-task.txt)" code none "task 1: piece $N of 45, $H, applying its Cw diff"
git diff $H^ $H -- $C ":(exclude)$C/CwTransmitter.cs" ":(exclude)$C/KeyerCwSender.cs" ":(exclude)$C/TransmitChain.cs" ":(exclude)$C/AutoCall.cs" ":(exclude)$C/AutoCallAnswers.cs" ":(exclude)$C/CwTransmitGuard.cs" ":(exclude)$C/TransmissionWatch.cs" ":(exclude)$C/TransmitReadiness.cs" ":(exclude)$C/TransmitPrivileges.cs" ":(exclude)$C/TransmitNotes.cs" ":(exclude)$C/ICwSender.cs" > $P
echo "patch lines $(wc -l < $P)"
git diff --stat $H^ $H -- $C
git apply --check $P
echo "check rc $?"
git apply $P
RC=$?
echo "apply rc $RC"
if [ $RC -ne 0 ]; then
  git apply --3way $P
  echo "3way rc $?"
fi
git status --short -- src
