
param (
    #Resource Group Name
    [Parameter(Mandatory=$true)]
    [String] $ResourceGroupName,

    #Location
    [Parameter(Mandatory=$true)]
    [String] $Location,

    #Action Group Name
    [Parameter(Mandatory=$true)]
    [String] $ActionGroupName,

    #Display Name Of ActionGroup 
    [Parameter(Mandatory=$true)]
    [String] $DisplayName,

    #Email 
    [Parameter(Mandatory=$true)]
    [String] $Email
)
try
{
    Write-Host "Started running Action Group creation script"
    Write-Host "Printing Variables : "
    Write-Host $ResourceGroupName
    Write-Host $Location
    Write-Host $ActionGroupName
  

    # include common Azure scripts
    #. ([System.IO.Path]::GetFullPath([System.IO.Path]::Combine($PSScriptRoot, "..\..\..\Common\Common-Azure.ps1")))

    
    #check if session exists, if not then prompt for login
    Write-Host "Logging in to Azure"
    #Check-Azure-Session
    Write-Host "Trying to get Azure Context"
	$azureContext = Get-AzContext
	Write-Host "Subscription id : $azureContext.Subscription.Id"

    

    #Creates a Email app reciever
    Write-Host "Creating Email App Reciever"

    $emailReceiver = New-AzActionGroupReceiver -Name 'Email Receiver' -EmailReceiver -EmailAddress $Email

    Write-Host "Email App Reciever Created"

    # Creates a new or updates an existing action group.
    Write-Host "Creating Action Group with Email App Reciever"
    $actionGroup = Set-AzActionGroup `
        -Name $ActionGroupName `
        -ShortName $DisplayName `
        -ResourceGroupName $ResourceGroupName `
        -Receiver $emailReceiver

    
    Write-Host "Successfully craeted Action Group : $ActionGroupName with Email App Reciever "

}
catch
{
    Write-Error "Error Creating Action Group"
    Write-Erroe $Errors
}
