# Hand-run of the six rules validate-output.bat holds, using the same
# PowerShell expressions the script itself uses. Unit 234 scratch, untracked.
$f = 'C:\Source\HamLet\output.md'
$t = Get-Content -LiteralPath $f

$u = $t | Select-Object -First 60 | Where-Object { $_.TrimStart([char]0xFEFF) -like 'UNIT:*' } | Select-Object -First 1
if ($u) { "rule 1  ok      $($u.TrimStart([char]0xFEFF))" } else { "rule 1  FAILED  no UNIT: line" }

$secs = ((Select-String -Path $f -Pattern '^## ' | ForEach-Object { $_.Line.Substring(3).Trim() }) -join ' ~ ')
$want = "1. What Claude did ~ 2. What the owner should expect ~ 3. What you should see ~ 4. What's blocking us"
if ($secs -eq $want) { "rule 2  ok      four top-level sections, in order, exact names"; "rule 3  ok      no fifth top-level section" }
else { "rule 2/3  FAILED"; "  expected : $want"; "  found    : $secs" }

$four = $t | Where-Object { $_ -like "## 4. What's blocking us*" }
if ($four) { "rule 4  ok      section 4 present" } else { "rule 4  FAILED  section 4 absent" }

$a = ($t | Select-String -Pattern '^## 3\. ' | Select-Object -First 1).LineNumber
$b = ($t | Select-String -Pattern '^## 4\. ' | Select-Object -First 1).LineNumber
$n = ($t[$a..($b-2)] | Where-Object { $_.Trim() -ne '' }).Count
if ($n -gt 0) { "rule 5  ok      section 3 has $n non-blank lines" } else { "rule 5  FAILED  section 3 empty" }

$h = ($t | Select-Object -First 60 | Where-Object { $_ -match 'READ IN THIS ORDER' } | Measure-Object).Count
$oa = ($t | Select-Object -First 60 | Where-Object { $_ -match '^A\.' } | Measure-Object).Count
$ob = ($t | Select-Object -First 60 | Where-Object { $_ -match '^B\.' } | Measure-Object).Count
$oc = ($t | Select-Object -First 60 | Where-Object { $_ -match '^C\.' } | Measure-Object).Count
$cn = ($t | Select-Object -First 60 | Where-Object { $_ -match 'raises \d+ item' } | Measure-Object).Count
if ($h -ge 1 -and $oa -ge 1 -and $ob -ge 1 -and $oc -ge 1 -and $cn -ge 1) { "rule 6  ok      ordering block present, A B C, and C names a count" }
else { "rule 6  FAILED  h=$h a=$oa b=$ob c=$oc count=$cn" }
