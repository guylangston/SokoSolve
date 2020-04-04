$status = & invoke-Expression "git status" | Out-String
$dirty = $status.Contains("Changes not staged for commit:")

$log = & invoke-Expression "git log --oneline -n 1" | Out-String

$txt = ""
if ($dirty) {$txt += "[DIRTY] "}
$txt += $log
Set-Content -Path "git-label.txt" -Value $txt
echo $txt

