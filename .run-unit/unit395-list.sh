cd /c/Source/HamLet
C=src/Hamlet.RadioEngine/Cw
echo "== LOG"
git log --reverse --date=short --format="@@%h|%ad|%s" --name-only 07f0397a..HEAD --until=2026-09-04 -- $C
echo "== ALLFILES per hash"
for h in $(git log --reverse --format=%h 07f0397a..HEAD --until=2026-09-04 -- $C); do
  echo "## $h"
  git diff-tree --no-commit-id --name-only -r $h | grep -v "^$C/"
  echo "## body"
  git log -1 --format=%b $h | head -8
done
echo "== TRANSMIT LOG"
git log --format="%h %ad %s" --date=short 07f0397a..HEAD --until=2026-09-04 -- $C/CwTransmitter.cs $C/KeyerCwSender.cs $C/TransmitChain.cs $C/AutoCall.cs $C/AutoCallAnswers.cs $C/CwTransmitGuard.cs $C/TransmissionWatch.cs $C/TransmitReadiness.cs $C/TransmitPrivileges.cs $C/TransmitNotes.cs $C/ICwSender.cs
echo "== TRANSMIT LOG 7e209cb4.."
git log --format="%h %ad %s" --date=short 7e209cb4..HEAD --until=2026-09-04 -- $C/CwTransmitter.cs $C/KeyerCwSender.cs $C/TransmitChain.cs $C/AutoCall.cs $C/AutoCallAnswers.cs $C/CwTransmitGuard.cs $C/TransmissionWatch.cs $C/TransmitReadiness.cs $C/TransmitPrivileges.cs $C/TransmitNotes.cs $C/ICwSender.cs
echo "== END"
