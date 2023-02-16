
param (
    #Alert Rule Names
    [Parameter(Mandatory=$true)]
    [String] $AlertRuleName,

    #Alert Rule Descriptions
    [Parameter(Mandatory=$true)]
    [String] $AlertRuleDescription,

    #Alert Rule Condition Name
    [Parameter(Mandatory=$true)]
    [String] $MeticRuleName,

    #Metric Rule Time Aggregation
    [Parameter(Mandatory=$true)]
    [String] $TimeAggregation,

    #Metric Rule Operators
    [Parameter(Mandatory=$true)]
    [String] $Operator,

    #Metric Rule Threshold
    [Parameter(Mandatory=$true)]
    [String] $Threshold,

    #Alert Rule Target Resource Name
    [Parameter(Mandatory=$true)]
    [String] $TargetResource,

    #Alert Rule Target Resource Location
    [Parameter(Mandatory=$true)]
    [string] $TargetResourceGroupName,

    #Alert Rule Target Resource Type
    [Parameter(Mandatory=$true)]
    [string] $TargetResourceType,

    #Alert Rule Severity
    [Parameter(Mandatory=$true)]
    [String] $Severity,

    #Alert Rule WindowSize
    [Parameter(Mandatory=$true)]
    [String] $WindowSize,

    #Alert Rule Frequency
    [Parameter(Mandatory=$true)]
    [String] $Frequency,

    #Resource Group Name
    [Parameter(Mandatory=$true)]
    [String] $ResourceGroupName,

    #Location
    [Parameter(Mandatory=$true)]
    [String] $Location,

    #ActionGroup Name
    [Parameter(Mandatory=$true)]
    [String] $ActionGroupName,

    #ActionGroup Resource Group Name
    [Parameter(Mandatory=$true)]
    [String] $ActionGroupResourceGroupName,
    
    #Environment
    [Parameter(Mandatory=$true)]
    [String] $Environment,

    #ComponentId
    [Parameter(Mandatory=$true)]
    [String] $ComponentId,

    #CheckId
    [Parameter(Mandatory=$true)]
    [String] $CheckId,
    
    [Hashtable] $AlertOverwriteTemplateParameters,
    
    [string] $AlertTemplateFile = 'MetricRule.json',
    
    [string] $AlertTemplateParametersFile = 'MetricRule.parameters.json'
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

    $AlertTemplateFile = [System.IO.Path]::GetFullPath([System.IO.Path]::Combine($PSScriptRoot, "..\MetricAlert\", $AlertTemplateFile))
    $AlertTemplateParametersFile = [System.IO.Path]::GetFullPath([System.IO.Path]::Combine($PSScriptRoot, "..\MetricAlert\", $AlertTemplateParametersFile))

    Write-Host "Getting the Path of TemplateFile: $AlertTemplateFile"
    Write-Host "Getting the Path of Parameter TemplateFile: $AlertTemplateParametersFile"

    Write-Host "Appending values to : $AlertOverwriteTemplateParameters"
    $AlertOverwriteTemplateParameters.Add("AlertName", $AlertRuleName)
    $AlertOverwriteTemplateParameters.Add("AlertDescription", $AlertRuleDescription)
    $AlertOverwriteTemplateParameters.Add("Scope", $targetResourceId)
    $AlertOverwriteTemplateParameters.Add("ActionGroup", $actiongroupid)
    $AlertOverwriteTemplateParameters.Add("MetricName", $MeticRuleName)
    $AlertOverwriteTemplateParameters.Add("Location", $Location)
    $AlertOverwriteTemplateParameters.Add("Frequency", $Frequency)
    $AlertOverwriteTemplateParameters.Add("Severity", $Severity)
    $AlertOverwriteTemplateParameters.Add("ResourceType", $TargetResourceType)
    $AlertOverwriteTemplateParameters.Add("WindowSize", $WindowSize)
    $AlertOverwriteTemplateParameters.Add("Operator", $Operator)
    $AlertOverwriteTemplateParameters.Add("TimeAggregation", $TimeAggregation)
    $AlertOverwriteTemplateParameters.Add("Threshold", $Threshold)
     $AlertOverwriteTemplateParameters.Add("Environment", $Environment)
    $AlertOverwriteTemplateParameters.Add("CheckId", $CheckId)
    $AlertOverwriteTemplateParameters.Add("ComponentId", $ComponentId)

    foreach($key in $AlertOverwriteTemplateParameters.keys) {
	Write-Host "$($key), $($AlertOverwriteTemplateParameters[$key])"
    }

    Write-Host "Creating Metric Alert"

    Write-Host " Template file path $AlertTemplateFile"
    Write-Host " Template parameters file path $AlertTemplateParametersFile"

    $jsonOutput = New-AzResourceGroupDeployment -Name $AlertRuleName `
                                                -ResourceGroupName $ResourceGroupName `
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




