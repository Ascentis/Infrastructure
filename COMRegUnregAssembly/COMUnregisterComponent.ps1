# for this to work you need to run the following command with admin rights: 
# Set-ExecutionPolicy -ExecutionPolicy Unrestricted -Scope CurrentUser

$comCatalog = New-Object -ComObject COMAdmin.COMAdminCatalog
$appColl = $comCatalog.GetCollection("Applications")
$appColl.Populate()

$app = $appColl | where {$_.Name -eq $args[0]}

$compColl = $appColl.GetCollection("Components", $app.Key)
$compColl.Populate()

$index = 0
foreach($component in $compColl) {
    if ($component.Name -eq $args[1]) {
        $compColl.Remove($index)
        $compColl.SaveChanges()
    }
    $index++
}