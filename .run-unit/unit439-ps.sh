#!/bin/sh
# Lists live processes that could be an earlier unit's chain. Kills nothing.
powershell -NoProfile -Command 'Get-CimInstance Win32_Process | Where-Object { $_.Name -match "dotnet|testhost|claude|bash|^sh" } | Select-Object ProcessId,ParentProcessId,Name,CreationDate,CommandLine | Format-List'
