Param(
  [Parameter(Mandatory=$true)]
  [string] $AlertName,

  [Parameter(Mandatory=$true)]
  [string] $AlertDescription,

  [Parameter(Mandatory=$true)]
  [string] $AlertSeverity,

  [Parameter(Mandatory=$true)]
  [string] $AlertResourceGroupName,

  [Parameter(Mandatory=$false)]
  [string] $AlertStatus,

  [Parameter(Mandatory=$false)]
  [string] $Query,

  [Parameter(Mandatory=$false)]
  [string] $QueryFile,

  [Parameter(Mandatory=$true)]
  [string] $Operator,

  [Parameter(Mandatory=$true)]
  [string] $Threshold,

  [Parameter(Mandatory=$true)]
  [string] $ActiongroupName,

  [Parameter(Mandatory=$true)]
  [string] $ActionGroupResourceGroupName,

  #Alert Rule Target Resource Name
  [Parameter(Mandatory=$true)]
  [String] $TargetResource,

  #Alert Rule Target Resource Location
  [Parameter(Mandatory=$true)]
  [string] $TargetResourceGroupName,

  #Alert Rule Target Resource Type
  [Parameter(Mandatory=$true)]
  [string] $TargetResourceType,

  [Parameter(Mandatory=$true)]
  [string] $Location,

  [Parameter(Mandatory=$true)]
  [string] $Frequency,

  [Parameter(Mandatory=$true)]
  [string] $WindowSize,

  [Parameter(Mandatory=$true)]
  [string] $TimeAggregation,

  [Parameter(Mandatory=$true)]
  [string] $Environment,

  [Parameter(Mandatory=$true)]
  [string] $CheckId,

  [Parameter(Mandatory=$true)]
  [string] $ComponentId,

  [Hashtable] $AlertOverwriteTemplateParameters,

  [string] $AlertTemplateFile = 'ScheduledQueryRule.json',

  [string] $AlertTemplateParametersFile = 'ScheduledQueryRule.parameters.json'
)

try{

    #Import-Module Az.Resources
    Write-Host "Uninstalling Azure RM module"
    Uninstall-AzureRm
    Write-Host 'Import Az modules'
    Import-Module Az.Resources
    Write-Host "Trying to get Azure Context"
	$azureContext = Get-AzContext
	Write-Host "Subscription id :"
    Write-Host $azureContext.Subscription.Id
    #Get-AzResourceGroup
    $actiongroupid = ""

    $Alert = Get-AzActivityLogAlert -ResourceGroupName $AlertResourceGroupName -Name $AlertName -ErrorAction SilentlyContinue
    
    if ($Alert) {
      Remove-AzActivityLogAlert -ResourceGroupName $AlertResourceGroupName -Name $AlertName
      Write-Host "Existing alert removed"
    }
    
    if($ActionGroupName){
        #Get Action Group Id
        Write-Host "Getting Action Group Id"
        $Actiongroup = Get-AzActionGroup -ResourceGroup $ActionGroupResourceGroupName -Name $ActionGroupName
        $actiongroupid = $Actiongroup.Id
        Write-Host "Action Group Id = $actiongroupid"
    }
    else
    {
        Write-Error "Please provide a value for Action Group Name "
    }

   Write-Host "Getting the target scope"

    $targetResourceObject = Get-AzResource -Name $TargetResource `
                                           -ResourceGroupName $TargetResourceGroupName `
                                           -ResourceType $TargetResourceType 

    $targetResourceId = $targetResourceObject.ResourceId

    
    # if no template parameters to overwrite are already passed to this script, create the hashtable
    if(!$AlertOverwriteTemplateParameters) {
	    $AlertOverwriteTemplateParameters = New-Object -TypeName Hashtable
    }

    if (!$Query -and !$QueryFile) {
      Write-Host 
      Write-Error "Either -query or -queryFile must be specified."    
      exit
    }

    $AlertTemplateFile = [System.IO.Path]::GetFullPath([System.IO.Path]::Combine($PSScriptRoot, "..\ScheduledQuery\", $AlertTemplateFile))
    $AlertTemplateParametersFile = [System.IO.Path]::GetFullPath([System.IO.Path]::Combine($PSScriptRoot, "..\ScheduledQuery\", $AlertTemplateParametersFile))

    Write-Host "Getting the Path of TemplateFile: $AlertTemplateFile"
    Write-Host "Getting the Path of Parameter TemplateFile: $AlertTemplateParametersFile"
    Write-Host "QueryFile : $QueryFile"
    Write-Host "Query : $Query"
    if ($QueryFile){
    Write-Host "QueryFile : $QueryFile"
      $QueryFile = [System.IO.Path]::GetFullPath([System.IO.Path]::Combine($PSScriptRoot, ".\AlertRules\", $QueryFile))
      # Read From File and set the Query Variable
      Write-Host "QueryFile : $QueryFile"
      $Query = Get-Content -Path $QueryFile
      Write-Host "**************************************** $Query"
      Write-Host $Query
    }

    Write-Host "Appending values to : $AlertOverwriteTemplateParameters"
    $AlertOverwriteTemplateParameters.Add("AlertName", $AlertName)
    $AlertOverwriteTemplateParameters.Add("AlertDescription", $AlertDescription)
    $AlertOverwriteTemplateParameters.Add("Scope", $targetResourceId)
    $AlertOverwriteTemplateParameters.Add("ActionGroup", $actiongroupid)
    $AlertOverwriteTemplateParameters.Add("Location", $Location)
    $AlertOverwriteTemplateParameters.Add("Frequency", $Frequency)
    $AlertOverwriteTemplateParameters.Add("Severity", $AlertSeverity)
    $AlertOverwriteTemplateParameters.Add("ResourceType", $TargetResourceType)
    $AlertOverwriteTemplateParameters.Add("WindowSize", $WindowSize)
    $AlertOverwriteTemplateParameters.Add("Operator", $Operator)
    $AlertOverwriteTemplateParameters.Add("Query", $Query)
    $AlertOverwriteTemplateParameters.Add("TimeAggregation", $TimeAggregation)
    $AlertOverwriteTemplateParameters.Add("Threshold", $Threshold)
     $AlertOverwriteTemplateParameters.Add("Environment", $Environment)
    $AlertOverwriteTemplateParameters.Add("CheckId", $CheckId)
    $AlertOverwriteTemplateParameters.Add("ComponentId", $ComponentId)

    foreach($key in $AlertOverwriteTemplateParameters.keys) {
	Write-Host "$($key), $($AlertOverwriteTemplateParameters[$key])"
    }

    Write-Host "Creating Scheduled Alert"

    Write-Host " Template file path $AlertTemplateFile"
    Write-Host " Template parameters file path $AlertTemplateParametersFile"

    $jsonOutput = New-AzResourceGroupDeployment -Name $AlertName `
                                                -ResourceGroupName $AlertResourceGroupName `
                                                -TemplateParameterObject $AlertOverwriteTemplateParameters `
                                                -TemplateFile $AlertTemplateFile `
                                                -Force -Verbose `
                                                -ErrorVariable ErrorMessages -DeploymentDebugLogLevel All
    Write-Host "Resource Created"
    Write-Host $jsonOutput
    #if (ErrorMessages) {
    #  Write-Output '', 'Creating the Alert returned the following errors:', @(@(ErrorMessages) | ForEach-Object { $_.Exception.Message.TrimEnd("`r`n") })		
    #}

}
catch{
    Write-Error "Error Creating Alert Rule $ErrorMessages"
}