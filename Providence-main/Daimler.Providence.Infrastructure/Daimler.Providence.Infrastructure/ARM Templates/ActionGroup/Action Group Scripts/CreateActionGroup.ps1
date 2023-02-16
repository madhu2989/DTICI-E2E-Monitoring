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

    #Logic App Name
    [Parameter(Mandatory=$true)]
    [String] $LogicAppName
)
try
{
    Write-Host "Started running Action Group creation script"
    Write-Host "Printing Variables : "
    Write-Host $ResourceGroupName
    Write-Host $Location
    Write-Host $ActionGroupName
    Write-Host $LogicAppName
    
    $logicAppCallBackUrl = ""
    $logicAppId = ""

    # include common Azure scripts
    #. ([System.IO.Path]::GetFullPath([System.IO.Path]::Combine($PSScriptRoot, "..\..\..\Common\Common-Azure.ps1")))

    
    #check if session exists, if not then prompt for login
    Write-Host "Logging in to Azure"
    #Check-Azure-Session
    Write-Host "Trying to get Azure Context"
	$azureContext = Get-AzContext
	Write-Host "Subscription id : $azureContext.Subscription.Id"

    if($LogicAppName){
        #Get Logic App CallbackUrl
        Write-Host "Trying to get logic app details for : $LogicAppName"
        $logicAppCallBackUrl = Get-AzLogicAppTriggerCallbackUrl -ResourceGroupName $ResourceGroupName -Name $LogicAppName -TriggerName "manual"
        
        #Get Logic App Resource id
        $logicAppInstance = Get-AzLogicApp `
                      -ResourceGroupName $ResourceGroupName `
                      -Name $LogicAppName
        $logicAppId = $logicAppInstance.Id 
        
    }
    else
    {
        Write-Error "Please provide a value for LogicAppName "
    }

    Write-Host "Logic App ID = $logicAppId"
    Write-Host "Logic App callback URL = $logicAppCallBackUrl.Value"

    #Creates a logic app reciever
    Write-Host "Creating Logic App Reciever"

    $logicAppReceiver = New-AzActionGroupReceiver `
       -Name "Logic App Reciever" `
       -LogicAppReceiver `
       -ResourceId $logicAppId `
       -CallbackUrl $logicAppCallBackUrl.Value 

    Write-Host "Logic App Reciever Created"


    # Creates a new or updates an existing action group.
    Write-Host "Creating Action Group with Logic App Reciever"
    $actionGroup = Set-AzActionGroup `
        -Name $ActionGroupName `
        -ShortName "STG-AG01" `
        -ResourceGroupName $ResourceGroupName `
        -Receiver $logicAppReceiver

    
    Write-Host "Successfully craeted Action Group : $ActionGroupName with Logic App Reciever "

}
catch
{
    Write-Error "Error Creating Action Group"
    Write-Erroe $Errors
}