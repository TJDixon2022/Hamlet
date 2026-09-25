cd /c/Source/HamLet
wmic process where "ProcessId=26364 or ProcessId=45956 or ProcessId=53948" get ProcessId,ParentProcessId,CreationDate,CommandLine 2>&1 | cut -c1-260
git log -3 --format="%h %ci %s" | cut -c1-80
