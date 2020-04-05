$status = & invoke-Expression "git status" | Out-String
$dirty = $status.Contains("Changes not staged for commit:")

$log = & invoke-Expression "git log --oneline -n 1" | Out-String
$log = $log.Replace("`n", "").Replace("`r", "")

$rev = & invoke-Expression "git rev-list --count HEAD" | Out-String
$rev = $rev.Replace("`n", "").Replace("`r", "")

$txt = ""
if ($dirty) {$txt += "[DIRTY] "}
$txt += $log
$txt += ', rev:' + $rev
Set-Content -Path "git-label.txt" -Value $txt
echo $txt

