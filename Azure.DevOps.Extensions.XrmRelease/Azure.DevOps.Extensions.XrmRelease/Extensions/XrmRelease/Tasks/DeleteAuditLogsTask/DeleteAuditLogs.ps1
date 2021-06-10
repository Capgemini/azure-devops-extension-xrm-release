#
# DeleteAuditLogs.ps1
#
[CmdletBinding()]
param(
    [string][Parameter(Mandatory=$true)]$clientId,
    [string][Parameter(Mandatory=$true)]$clientSecret,
    [string][Parameter(Mandatory=$true)]$url,
    [string][Parameter(Mandatory=$true)]$auditLogCreatedBeforeDays
) 

$connectionString = "AuthType=ClientSecret;ClientId=" + $clientId + ";ClientSecret=" + $clientSecret + ";Url=" + $url + ";"

Write-Host "Installing XRM Tooling module"
if (Get-Module -ListAvailable -Name Microsoft.Xrm.Tooling.CrmConnector.PowerShell) {
} 
else {
    Install-Module -Name Microsoft.Xrm.Tooling.CrmConnector.PowerShell -Force -Scope CurrentUser
}

Write-Host "Deletion of Audit Logs Started"
Write-Host "Audit log created before days :" $auditLogCreatedBeforeDays 
$ErrorActionPreference = "Stop"
try {
    $conn = Get-CrmConnection -ConnectionString $connectionString 
    Write-Host "Organization Name :" $conn.ConnectedOrgFriendlyName 
    $headers = New-Object "System.Collections.Generic.Dictionary[[String],[String]]"
    $headers.Add("Authorization", $("Bearer " + $conn.CurrentAccessToken))
    $headers.Add("Content-Type", "application/json") 
    $enddate = (Get-date).AddDays(-$auditLogCreatedBeforeDays).ToString("yyyy-MM-dd")
    $body = $('{"EndDate":' + '"' + $enddate + '"}')
    Write-Host "End Date :" $enddate

    $response = Invoke-RestMethod  $($conn.ConnectedOrgPublishedEndpoints['WebApplication'] + 'api/data/v9.2/DeleteAuditData') -Method 'POST' -Headers $headers -Body $body
    $response | ConvertTo-Json
    Write-Host "Audit log Deleted Successfully"

}
catch {
    foreach ($er in $Error) {
	      
        if ($_ | Where-Object { $er.ErrorDetails -match "0x80040216" }) {
            Write-Host "##vso[task.logissue type=warning]$er"
            Write-Host "##vso[task.complete result=SucceededWithIssues;]Task completed - No data to delete"
            exit 1             
        }
        else {
            Write-Host "##vso[task.logissue type=error]$er"
            Write-Host "##vso[task.complete result=Failed;]Task failed - see error/s"
        }
    }
}
