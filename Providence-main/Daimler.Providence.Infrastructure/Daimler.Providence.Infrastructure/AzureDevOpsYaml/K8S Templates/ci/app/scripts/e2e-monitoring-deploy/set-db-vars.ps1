#!/usr/bin/env pwsh

Param(
    [Parameter(Mandatory=$true)]
    [string] $SmsCosmosDbResourceGroup,

    [Parameter(Mandatory=$true)]
    [string] $SmsCosmosAccountName,

    [Parameter(Mandatory=$true)]
    [string] $SmsName,

    [Parameter(Mandatory=$true)]
    [string] $SmsCosmosDbLocation
)

function WriteVarToHost {
    param (
        [String] $Key,
        $Value,
        [switch] $HideOutput
    )
    
    $Value  = $Value.Trim()

    Write-Host "##vso[task.setvariable variable=$Key;isOutput=true]$Value"

    if (-Not $HideOutput) {
        Write-Host "$Key : $Value"
    } else {
        Write-Host "$Key : ***"
    }
}

#################################
# Set CosmosDb variables
#################################
$rg = $SmsCosmosDbResourceGroup
$smsDB = ConvertFrom-Json -InputObject $(az cosmosdb show --name $SmsCosmosAccountName --resource-group $rg | Out-String)
WriteVarToHost -Key 'SmsCosmosDbName' -Value $SmsName
WriteVarToHost -Key 'SmsCosmosDbLocation' -Value $SmsCosmosDbLocation
WriteVarToHost -Key 'SmsCosmosDbEndpoint' -Value $smsDB.DocumentEndpoint

$keys = ConvertFrom-Json -InputObject $(az cosmosdb keys list --name $SmsCosmosAccountName --resource-group $rg --type keys | Out-String)
WriteVarToHost -Key 'SmsCosmosDbPrimaryKey' -Value $keys.PrimaryMasterKey -HideOutput
