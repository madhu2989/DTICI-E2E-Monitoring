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
	
	#Function App Name
    [Parameter(Mandatory=$true)]
    [String] $FunctionAppName,

    #Function App Name
    [Parameter(Mandatory=$true)]
    [String] $FunctionName,

    #Resource Group Name Of Function App
    [Parameter(Mandatory=$true)]
    [String] $ResourceGroupNameOfFuncApp,

    #Display Name Of ActionGroup 
    [Parameter(Mandatory=$true)]
    [String] $DisplayName
)
try
{
    Write-Host "Started running Action Group creation script"
    Write-Host "Printing Variables : "
    Write-Host $ResourceGroupName
    Write-Host $Location
    Write-Host $ActionGroupName
	Write-Host $FunctionAppName
    Write-Host $FunctionName
    Write-Host $ResourceGroupNameOfFuncApp
  

    # include common Azure scripts
    #. ([System.IO.Path]::GetFullPath([System.IO.Path]::Combine($PSScriptRoot, "..\..\..\Common\Common-Azure.ps1")))

    
    #check if session exists, if not then prompt for login
    Write-Host "Logging in to Azure"
    #Check-Azure-Session
    Write-Host "Trying to get Azure Context"
	$azureContext = Get-AzContext
	Write-Host "Subscription id : $azureContext.Subscription.Id"

    if($FunctionAppName){
        #Get Function App HttpTriggerUrl
        Write-Host "Trying to get Function app trigger Url:"
		
        #Get Function App Resource id		
		$functionAppInstance = Get-AzFunctionApp `
							-ResourceGroupName $ResourceGroupNameOfFuncApp `
							-Name $FunctionAppName
		$functionAppId = $functionAppInstance.Id 
		Write-Host "Received Function App Id: $functionAppId"
		

		$DefaultHostName = $functionAppInstance.DefaultHostName
        Write-Host "HostName has been set: $DefaultHostName"

		$functionKey = (Invoke-AzResourceAction -ResourceId "$functionAppId/functions/$FunctionName" -Action listkeys -Force).default
        Write-Host "Url code/function key has been set: $functionKey"
        #$functionAppURL = "https://" + $DefaultHostName + "/api/" + $FunctionName + " --header 'x-functions-key: $functionKey'"
		$functionAppURL = "https://" + $DefaultHostName + "/api/" + $FunctionName + "?code=" + $FunctionKey        
        Write-Host "Composed Function app Url to : $functionAppURL"
    }
    else
    {
        Write-Error "Please provide a value for FunctionAppName "
    }

    #Creates a Function app reciever
    Write-Host "Creating Function App Reciever"

    $functionAppReceiver = New-AzActionGroupReceiver `
       -Name "Function App Reciever" `
       -AzureFunctionReceiver `
       -FunctionAppResourceId $functionAppId `
	   -HttpTriggerUrl $functionAppURL `
	   -FunctionName $FunctionAppName 

    Write-Host "Function App Reciever Created"

    # Creates a new or updates an existing action group.
    Write-Host "Creating Action Group with Function App Reciever"
    $actionGroup = Set-AzActionGroup `
        -Name $ActionGroupName `
        -ShortName $DisplayName `
        -ResourceGroupName $ResourceGroupName `
        -Receiver $functionAppReceiver

    
    Write-Host "Successfully craeted Action Group : $ActionGroupName with Function App Reciever "

}
catch
{
    Write-Error "Error Creating Action Group"
    Write-Erroe $Errors
}
