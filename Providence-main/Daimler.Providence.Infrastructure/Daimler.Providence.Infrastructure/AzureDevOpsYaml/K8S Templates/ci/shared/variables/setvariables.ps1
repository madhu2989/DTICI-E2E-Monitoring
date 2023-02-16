<#
.SYNOPSIS

.DESCRIPTION
This script sets all variables, which are needed for this project as environment variables. See how to use them in Azure DevOps here: https://docs.microsoft.com/en-us/azure/devops/pipelines/process/variables?view=azure-devops&tabs=yaml%2Cbatch
#>

Param(
    [Parameter(Mandatory=$true)]
    [string] $Environment,

    [Parameter(Mandatory = $true)]
    [string] $LocationShortCsg,

    [Parameter(Mandatory=$true)]
    [string] $VersionNumber,

    [Parameter(Mandatory=$true)]
    [string] $WorkingDir
)
#TODO: Need to setup variables as per monitoring
$Environment = $Environment.ToLower()
Write-Host "$Environment"



$LocationShortCsg = $LocationShortCsg.ToLower()

# EXPORT VARS TO ENVIRONMENT
Write-Host "-------------These are the parameters that you can use:-------------"
function WriteVarToHost {
    param (
        [String] $Key,
        $Value,
        [switch] $ToLower,
        [switch] $IsSecret
    )
    if($ToLower) {
        $Value = $Value.ToLower()
    }

    # Boolean and Int Types don't support Trim Operation
    if($Value.getType().Name -ne "Boolean" -And $Value.getType().Name -ne "Int32" -And $Value.getType().Name -ne "Object[]") {
        $Value  = $Value.Trim()
    }
   
    Write-Host "##vso[task.setvariable variable=$Key;isOutput=true]$Value"
    if($IsSecret) {
        $firstChars = $Value.substring(0,3)
        Write-Host "$Key : $firstChars***"
    } else {
        Write-Host "$Key : $Value"
    }
}

function GetMSIClientId($IdentityName, $ResourceGroupName)
{
    Write-Host "Trying to fetch ClientId of MSI $IdentityName in $ResourceGroupName"
    $clientId=$(az identity show -n $IdentityName -g $ResourceGroupName --query 'clientId' -o tsv)
    if (!$clientId) {
        Write-Warning "Could not fetch ClientId. This is okay for the first time executing the infra release as the msi will be created there but shouldn't occur afterwards!"
        return ""
    }
    return $clientId
}

function GetMSIPrincipalId($IdentityName, $ResourceGroupName)
{
    Write-Host "Trying to fetch PrincipalId of MSI $IdentityName in $ResourceGroupName"
    $id=$(az identity show -n $IdentityName -g $ResourceGroupName --query 'principalId' -o tsv)
    if (!$id) {
        Write-Warning "Could not fetch PrincipalId. This is okay for the first time executing the infra release as the msi will be created there but shouldn't occur afterwards!"
        return ""
    }
    return $id
}

function GetAcrLatestImageTag($acrName, $repoName) {
    #return ( az keyvault secret show --name "DBConnectionString" --vault-name $KeyVaultName --query "value")
    $imageTag = az acr repository show-tags -n $acrName --repository $repoName --orderby time_desc --top 1 --output tsv
    # $imageBathUrl = "csgcrewe2emonitoring.azurecr.cn/e2emonitoring-backend:1.2.247031"
    if (($Environment -eq "stg_cne2") -or ($Environment -eq "prod_cne2"))
    {
        $imageUrl = "csgcrewe2emonitoring.azurecr.cn/ " + $repoName + ":" + $imageTag
        return $imageUrl
    } 
    else 
    {
        $imageUrl = "csgcrewe2emonitoring.azurecr.io/" + $repoName + ":" + $imageTag
        return $imageUrl
    }    
}


# MONITORING PARAMETERS

WriteVarToHost -Key 'Environment' -Value $Environment

$E2EMonitoringService = "E2EMonitoringServiceBackend"
WriteVarToHost -Key 'ApplicationName' -Value $E2EMonitoringService

WriteVarToHost -Key 'APP_CONFIGURATION_PREFIX' -Value "/e2e-monitoring-service-backend"

$E2EMonitoringServiceAppName = "e2e-mon-backend"
WriteVarToHost -Key 'E2E_MONITORING_SERVICE_APP_NAME' -Value $E2EMonitoringServiceAppName

$E2EMonitoringServiceAccName = "e2e-mon-backend.com"
WriteVarToHost -Key 'E2E_MONITORING_SERVICE_ACC_NAME' -Value $E2EMonitoringServiceAccName

$COMPONENT_NAME = "e2e-mon-backend"
WriteVarToHost -Key 'COMPONENT_NAME' -Value $COMPONENT_NAME

$E2EMonitoringServiceShort = "e2eMonBackend"
WriteVarToHost -Key 'ApplicationNameShort' -Value $E2EMonitoringServiceShort

#csgcrewe2emonitoring.azurecr.cn

$E2EMonitoringBackendImageName = GetAcrLatestImageTag -acrName "csgcrewe2emonitoring" -repoName "e2emonitoring-backend"

# if (($Environment -eq "stg_cne2") -or ($Environment -eq "prod_cne2"))
# {
#     $E2EMonitoringBackendImageName = "csgcrewe2emonitoring.azurecr.cn/e2emonitoring-backend:1.2.247031"
# }
# else 
# {
#     $E2EMonitoringBackendImageName = "csgcrewe2emonitoring.azurecr.io/e2emonitoring-backend:1.2.250009"
# }

WriteVarToHost -Key 'E2EMonitoringBackendImageName' -Value $E2EMonitoringBackendImageName

#Front End App name and Details
$E2EMonitoringServiceAppNameUI = "e2e-mon-frontend"
WriteVarToHost -Key 'E2E_MONITORING_UI_APP_NAME' -Value $E2EMonitoringServiceAppNameUI

$E2EMonitoringServiceAccNameUI = "e2e-mon-frontend.com"
WriteVarToHost -Key 'E2E_MONITORING_SERVICE_UI_ACC_NAME' -Value $E2EMonitoringServiceAccNameUI

$COMPONENT_NAME_UI = "e2e-mon-frontend"
WriteVarToHost -Key 'COMPONENT_NAME_UI' -Value $COMPONENT_NAME_UI

$E2EMonitoringFrontEndImageName = GetAcrLatestImageTag -acrName "csgcrewe2emonitoring" -repoName "e2emonitoring-frontend"
# if (($Environment -eq "stg_cne2") -or ($Environment -eq "prod_cne2"))
# {
#     $E2EMonitoringFrontEndImageName = "csgcrewe2emonitoring.azurecr.cn/e2emonitoring-frontend:1.2.247031"    
# } 
# else {
#     $E2EMonitoringFrontEndImageName = "csgcrewe2emonitoring.azurecr.io/e2emonitoring-frontend:1.2.250009"
# }
WriteVarToHost -Key 'E2EMonitoringFrontEndImageName' -Value $E2EMonitoringFrontEndImageName


# COMMON PARAMETERS
$OrgUnitCsg = "csg"

$EnvironmentClusterNameShort = "env"
$SharedResourceGroupName = "Shared"
$ApiManagementNameSuffix = "apim"
$ContainerRegistryName = "itccoba"
$EndpointPort = "9000"
$EndpointProtocol = "https"
$CompanyNameShort = "daimler"
$ACRRepositoryName = "e2e-Monitoring-Service"

$EndpointPortUI = "9050"

WriteVarToHost -Key 'containerRegistryName' -Value $ContainerRegistryName
WriteVarToHost -Key 'serviceName' -Value $E2EMonitoringService
WriteVarToHost -Key 'VersionNumber' -Value $VersionNumber
WriteVarToHost -Key 'ACRRepositoryName' -Value $ACRRepositoryName

WriteVarToHost -Key 'EndpointPort' -Value $EndpointPort
WriteVarToHost -Key 'EndpointProtocol' -Value $EndpointProtocol

WriteVarToHost -Key 'EndpointPortUI' -Value $EndpointPortUI

if ($Env:DOCKERTAG) {
    WriteVarToHost -Key 'imageTag' -Value $Env:DOCKERTAG
}

# DEFAULT CONFIGS
$RESOURCES_REQUESTS_MEMORY = "4Gi"
$RESOURCES_LIMITS_MEMORY = "4Gi"
$RESOURCES_REQUESTS_CPU = "1500m"
$RESOURCES_LIMITS_CPU = "2"
$REPLICAS = 1 # should match the hpa min value

WriteVarToHost -Key 'REPLICAS' -Value $REPLICAS

$infrastructureNugetVersion="2.1.3-20220118-162155"
$azureLocations = @()
$azureLocationsCsg = @()
$csgRegionalSubscriptionObjects = @()
$runAutoRefresh= "true"
$autoRefreshJobIntervalInSeconds = 60
$runAutoReset = 0
$runDeleteExpiredStatetransitions = "true"
$runDeleteExpiredDeployments= 0
$runDeleteExpiredChangelogs= 0
$runDeleteUnassignedComponents= 0
$cutOffTimeRangeInWeeks= 2
$logSqlQuery= 1
$logElapsedTime= 0
$emailNotificationJobIntervalInSeconds= 10
$stateIncreaseJobIntervalInSeconds= 100
$updateDeploymentsJobIntervalInSeconds= 100
$enableEventHubReader= "true"
$autoResetJobIntervalInSeconds = "#{AutoResetJobIntervalInSeconds}#" 
$maxElapsedTimeInMinutes= "#{MaxElapsedTimeInMinutes}#"
$daimlerRelayUsername= "#{DaimlerRelay-Username}#"
$daimlerRelayPassword= "#{DaimlerRelay-Password}#"

# ENVIRONMENT VARS
if ($Environment -eq "dev") {
    $RESOURCES_REQUESTS_CPU = "500m"
    $StageCsg = "dev"
    $SubscriptionIdCsg = "8900235a-4634-414a-9ca6-0b391bac3eef"
    $azureLocationsCsg = @("westeurope")
    $csgRegionalSubscriptionObjects = @(@{LocationShort = 'weu'; Subscription = $SubscriptionIdCsg})
    $DnsUrlStageWithDots = ".dev."
    $E2EMonitoringServiceDisplayName = "E2E-Monitoring-Service"    
    $EnvInfix = "dev"
    $MinReplicas = 2
    $MaxReplicas = 3
    $CosmosDbPreferredLocation = "West Europe"
    $CompanyNameShortDnsCsg = "daimler-truck"
    $ManagedIdentityName = "csg-weu-dev-e2e-msi"
    $ManagedIdentityResourceGroup = "csg-weu-dev-e2e-mon-msi"

    #Common Env Variables
    $AadTenantId = "0471b588-7419-4f7e-8333-7ad2726afc86"
    $E2EAADApplicationId = "4cab2c07-0b42-4778-9562-24c711957006"

    #Backend Env Variables
    $envShort= "dev"
    $regionShort= "eu"  
    $eventHubName= "providencedev"
    $storageUrlPath= "https://csgdevmonstorageweu.blob.core.windows.net"
    $eventHubQualifiedNameSpace= "csgprovidencedev.servicebus.windows.net"
    $authBaseUrl = "https://login.microsoftonline.com/"
    
    #Frontend Env Variables
    $versionReleaseName= "Release-1" 
    $versionBuildNumber= "10.0.1"
    $houseKeepingInterval= 60000
    $historyDuration= 259200000
    $instance = "https://login.microsoftonline.com/"
} elseif ($Environment -eq "stg") {
    $StageCsg = "stg"
    $SubscriptionIdCsg = "c1f14484-3fe1-4d20-9f9c-031d6ebf80e1"
    $azureLocationsCsg = @("westeurope")
    $csgRegionalSubscriptionObjects = @(@{LocationShort = 'weu'; Subscription = $SubscriptionIdCsg})
    $DnsUrlStageWithDots = ".staging."
    $E2EMonitoringServiceDisplayName = "E2E-Monitoring-Service"    
    $EnvInfix = "staging"
    $MinReplicas = 3
    $MaxReplicas = 5
    $CompanyNameShortDnsCsg = "daimler-truck"
    $ManagedIdentityName = "csg-weu-stg-e2e-msi"
    $ManagedIdentityResourceGroup = "csg-weu-stg-e2e-mon-msi"

    #Common Env Variables
    $AadTenantId = "0471b588-7419-4f7e-8333-7ad2726afc86"
    $E2EAADApplicationId = "4cab2c07-0b42-4778-9562-24c711957006"

    #Backend Env Variables
    $envShort ="staging"
    $regionShort ="eu"
    $eventHubName= "csg-stg-eh-mon-hub1-weu"
    $storageUrlPath= "https://csgstgmonstorageweu.blob.core.windows.net"
    $eventHubQualifiedNameSpace= "csg-stg-eh-mon-weu.servicebus.windows.net"
    $authBaseUrl = "https://login.microsoftonline.com/"
    
    #Frontend Env Variables
    $versionReleaseName= "Release-1" 
    $versionBuildNumber= "10.0.1"
    $houseKeepingInterval= 60000
    $historyDuration= 259200000 
    $instance = "https://login.microsoftonline.com/"
} elseif ($Environment -eq "stg_cne2") {
    $StageCsg = "stg"
    #TODO: Get Subscription ID for CN
    $SubscriptionIdCsg = "e89715f7-2fe8-48fd-a034-3f99ed93da5a"
    $azureLocationsCsg = @("chinaeast2")
    $csgRegionalSubscriptionObjects = @(@{LocationShort = 'cne2'; Subscription = $SubscriptionIdCsg})
    $DnsUrlStageWithDots = ".staging."
    $E2EMonitoringServiceDisplayName = "E2E-Monitoring-Service"
    #TODO: Populate AAD Tenant ID for CN
    $AadTenantId = "5b09bf9a-a60b-46f7-a92c-1a72a845c331"
    $EnvInfix = "staging"
    $MinReplicas = 3
    $MaxReplicas = 5
    $CosmosDbPreferredLocation = "China East 2"
    $CompanyNameShortDnsCsg = "connectivity.fotondaimler"
    $E2EAADApplicationId = "94077a58-ab3f-43f6-b877-a65f5882f867"
    #TODO: Populate Instrumentation Key for CN
    $ApplicationInsightsInstrumentationKeyCsg = "37cbc263-7791-c2ce-a315-648c69ddf0b1"
    $DatabaseConnectionString = "Server=tcp:csg-stg-e2e-mon-sql-cnn3.database.chinacloudapi.cn; Authentication=Active Directory Managed Identity; User Id=ef198e34-e8f4-4ce5-8aba-e61cde52cac2; Initial Catalog=csg-stg-e2e-mon-sqldb-cnn3; Encrypt=True;MultipleActiveResultSets=true;Max Pool Size=200;"
    # $PodIdentityBindingName = "e2ebackend-csg-cne2-stg-e2e-mon-msi-binding"
    $ManagedIdentityName = "csg-cne2-stg-e2e-msi"
    $ManagedIdentityResourceGroup = "csg-cne2-stg-e2e-mon-msi"
    
    #Common Env Variables
    $AadTenantId = "5b09bf9a-a60b-46f7-a92c-1a72a845c331"
    $E2EAADApplicationId = "94077a58-ab3f-43f6-b877-a65f5882f867"

    #Backend Env Variables
    $envShort ="staging"
    $regionShort ="cn"
    $eventHubName= "csg-stg01-eh-hub1-cne2"
    $storageUrlPath= "https://csgstge2emonstoragecne2.blob.core.chinacloudapi.cn"
    $eventHubQualifiedNameSpace= "csg-stg01-eh-cne2.servicebus.chinacloudapi.cn"
    $appInsightsConnectionString = "InstrumentationKey=37cbc263-7791-c2ce-a315-648c69ddf0b1;EndpointSuffix=applicationinsights.azure.cn;IngestionEndpoint=https://chinaeast2-0.in.applicationinsights.azure.cn/;AADAudience=https://monitor.azure.cn/"
    $authBaseUrl = "https://login.partner.microsoftonline.cn/"
    #$databaseConnectionString= ""
    
    #Frontend Env Variables
    $versionReleaseName= "Release-1" 
    $versionBuildNumber= "10.0.1"
    $houseKeepingInterval= 60000
    $historyDuration= 259200000
    $instance = "https://login.partner.microsoftonline.cn/"
} elseif ($Environment -eq "prod_weu") {
    $StageCsg = "prod"
    #TODO: Get Subscription ID
    $SubscriptionIdCsg = "f363028c-0c18-4fc1-b3da-addc4415a6dd"
    $azureLocationsCsg = @("westeurope","eastus2", "japaneast")
    $csgRegionalSubscriptionObjects = @(
        @{LocationShort = 'weu'; Subscription = $SubscriptionIdCsg},
        @{LocationShort = 'eus2'; Subscription = '74515af6-f941-4a14-a5d9-c8974ce6d8a3'},
        @{LocationShort = 'jpe'; Subscription = '423d04e0-9b86-4d55-9674-2efa1a04b202'}
        )
    $DnsUrlStageWithDots = "."
    $E2EMonitoringServiceDisplayName = "E2E-Monitoring-Service"
    $EnvInfix = ""
    $MinReplicas = 3
    $MaxReplicas = 5
    $CosmosDbPreferredLocation = "West Europe"
    $CompanyNameShortDnsCsg = "daimler-truck"
    $ManagedIdentityName = "csg-weu-prod-e2e-msi"
    $ManagedIdentityResourceGroup = "csg-weu-prod-e2e-mon-msi"

    #Common Env Variables
    $AadTenantId = "9652d7c2-1ccf-4940-8151-4a92bd474ed0"
    $E2EAADApplicationId = "322162e6-c8ed-45ec-8d2f-a76ac909b5f6"

    #Backend Env Variables
    $envShort ="prod"
    $regionShort ="eu"
    $eventHubName= "csg-prod01-eh-hub1-weu"
    $storageUrlPath= "https://csgprode2emonstorageweu.blob.core.windows.net"
    $eventHubQualifiedNameSpace= "csg-prod01-eh-weu.servicebus.windows.net"
    $authBaseUrl = "https://login.microsoftonline.com/"
    
    #Frontend Env Variables
    $versionReleaseName= "Release-1" 
    $versionBuildNumber= "10.0.1"
    $houseKeepingInterval= 60000
    $historyDuration= 259200000    
    $instance = "https://login.microsoftonline.com/"
} elseif ($Environment -eq "prod_eus2") {
    
    $StageCsg = "prod"
    #TODO: Get Subscription ID
    $SubscriptionIdCsg = "74515af6-f941-4a14-a5d9-c8974ce6d8a3"
    $azureLocationsCsg = @("westeurope","eastus2", "japaneast")
    $csgRegionalSubscriptionObjects = @(
        @{LocationShort = 'weu'; Subscription = 'f363028c-0c18-4fc1-b3da-addc4415a6dd'},
        @{LocationShort = 'eus2'; Subscription = $SubscriptionIdCsg},
        @{LocationShort = 'jpe'; Subscription = '423d04e0-9b86-4d55-9674-2efa1a04b202'}
        )
    $DnsUrlStageWithDots = "."
    $E2EMonitoringServiceDisplayName = "E2E-Monitoring-Service"
    $EnvInfix = ""
    $MinReplicas = 3
    $MaxReplicas = 5
    $CosmosDbPreferredLocation = "East US 2"
    $CompanyNameShortDnsCsg = "daimler-truck"
    $ManagedIdentityName = "csg-eus2-prod-e2e-msi"
    $ManagedIdentityResourceGroup = "csg-eus2-prod-e2e-mon-msi"

    #Common Env Variables
    $AadTenantId = "9652d7c2-1ccf-4940-8151-4a92bd474ed0"
    $E2EAADApplicationId = "62d16354-a575-4ffb-bdcd-971474aacc5e"

    #Backend Env Variables
    $envShort ="prod"
    $regionShort ="us"
    $eventHubName= "csg-prod01-eh-hub1-eus2"
    $storageUrlPath= "https://csgprode2emonstorageeus2.blob.core.windows.net"
    $eventHubQualifiedNameSpace= "csg-prod01-eh-eus2.servicebus.windows.net"
    $authBaseUrl = "https://login.microsoftonline.com/"
    
    #Frontend Env Variables
    $versionReleaseName= "Release-1" 
    $versionBuildNumber= "10.0.1"
    $houseKeepingInterval= 60000
    $historyDuration= 259200000
    $instance = "https://login.microsoftonline.com/"
} elseif ($Environment -eq "prod_jpe") {
    $StageCsg = "prod"
    #TODO: Get Subscription ID
    $SubscriptionIdCsg = "423d04e0-9b86-4d55-9674-2efa1a04b202"
    $azureLocationsCsg = @("westeurope","eastus2", "japaneast")
    $csgRegionalSubscriptionObjects = @(
        @{LocationShort = 'weu'; Subscription = 'f363028c-0c18-4fc1-b3da-addc4415a6dd'},
        @{LocationShort = 'eus2'; Subscription = '74515af6-f941-4a14-a5d9-c8974ce6d8a3'},
        @{LocationShort = 'jpe'; Subscription = $SubscriptionIdCsg}
        )
    $DnsUrlStageWithDots = "."
    $E2EMonitoringServiceDisplayName = "E2E-Monitoring-Service"
    $EnvInfix = ""
    $MinReplicas = 3
    $MaxReplicas = 5
    $CosmosDbPreferredLocation = "Japan East"
    $CompanyNameShortDnsCsg = "daimler-truck"
    $ManagedIdentityName = "csg-jpe-prod-e2e-msi"
    $ManagedIdentityResourceGroup = "csg-jpe-prod-e2e-mon-msi"

    #Common Env Variables
    $AadTenantId = "9652d7c2-1ccf-4940-8151-4a92bd474ed0"
    $E2EAADApplicationId = "9e965cea-37dc-4121-baaf-63adbea21540"

    #Backend Env Variables
    $envShort ="prod"
    $regionShort ="jp"
    $runAutoRefresh= "true"
    $eventHubName= "csg-prod01-eh-hub1-jpe"
    $storageUrlPath= "https://csgprode2emonstoragejpe.blob.core.windows.net"
    $eventHubQualifiedNameSpace= "csg-prod01-eh-jpe.servicebus.windows.net"
    $authBaseUrl = "https://login.microsoftonline.com/"
    
    #Frontend Env Variables
    $versionReleaseName= "Release-1" 
    $versionBuildNumber= "10.0.1"
    $houseKeepingInterval= 60000
    $historyDuration= 259200000
    $instance = "https://login.microsoftonline.com/"
} elseif ($Environment -eq "prod_cne2") {
    $StageCsg = "prod"
    #TODO: Get Subscription ID
    $SubscriptionIdCsg = "c6ed4c5e-1e9d-4378-8945-30bb2b08cf63"
    $azureLocationsCsg = @("chinaeast2")
    $csgRegionalSubscriptionObjects = @(@{LocationShort = 'cne2'; Subscription = $SubscriptionIdCsg})
    $DnsUrlStageWithDots = ".prod."
    $E2EMonitoringServiceDisplayName = "E2E-Monitoring-Service"
    $EnvInfix = "prod"
    $MinReplicas = 3
    $MaxReplicas = 5
    $CosmosDbPreferredLocation = "China East 2"
    $CompanyNameShortDnsCsg = "connectivity.fotondaimler"
    $ManagedIdentityName = ""
    $ManagedIdentityResourceGroup = ""

    #Common Env Variables
    $AadTenantId = "5b09bf9a-a60b-46f7-a92c-1a72a845c331"
    $E2EAADApplicationId = "94077a58-ab3f-43f6-b877-a65f5882f867"

    #Backend Env Variables
    $envShort ="prod"
    $regionShort ="cn"
    $eventHubName= "csg-prod01-eh-hub1-cne2"
    $storageUrlPath= "https://csgprode2emonstoragecne2.blob.core.chinacloudapi.cn"
    $eventHubQualifiedNameSpace= "csg-prod01-eh-cne2.servicebus.chinacloudapi.cn"
    #$databaseConnectionString= ""
    $authBaseUrl = "https://login.partner.microsoftonline.cn/"
    
    #Frontend Env Variables
    $versionReleaseName= "Release-1" 
    $versionBuildNumber= "10.0.1"
    $houseKeepingInterval= 60000
    $historyDuration= 259200000
    $instance = "https://login.partner.microsoftonline.cn/"
}

#Assign Backend Environment Variable

WriteVarToHost -Key 'EnvShort' -Value $envShort
WriteVarToHost -Key 'RegionShort' -Value $regionShort
WriteVarToHost -Key 'RunAutoRefresh' -Value $runAutoRefresh
WriteVarToHost -Key 'AutoRefreshJobIntervalInSeconds' -Value $autoRefreshJobIntervalInSeconds
WriteVarToHost -Key 'RunAutoReset' -Value $runAutoReset
WriteVarToHost -Key 'RunDeleteExpiredStatetransitions' -Value $runDeleteExpiredStatetransitions
WriteVarToHost -Key 'RunDeleteExpiredDeployments' -Value $runDeleteExpiredDeployments
WriteVarToHost -Key 'RunDeleteExpiredChangelogs' -Value $runDeleteExpiredChangelogs
WriteVarToHost -Key 'RunDeleteUnassignedComponents' -Value $runDeleteUnassignedComponents
WriteVarToHost -Key 'CutOffTimeRangeInWeeks' -Value $cutOffTimeRangeInWeeks
WriteVarToHost -Key 'LogSqlQuery' -Value $logSqlQuery
WriteVarToHost -Key 'LogElapsedTime' -Value $logElapsedTime
WriteVarToHost -Key 'EmailNotificationJobIntervalInSeconds' -Value $emailNotificationJobIntervalInSeconds
WriteVarToHost -Key 'StateIncreaseJobIntervalInSeconds' -Value $stateIncreaseJobIntervalInSeconds
WriteVarToHost -Key 'UpdateDeploymentsJobIntervalInSeconds' -Value $updateDeploymentsJobIntervalInSeconds
WriteVarToHost -Key 'EnableEventHubReader' -Value $enableEventHubReader
#WriteVarToHost -Key 'ManagedIdentity' -Value $managedIdentity
WriteVarToHost -Key 'EventHubName' -Value $eventHubName
WriteVarToHost -Key 'StorageUrlPath' -Value $storageUrlPath
WriteVarToHost -Key 'EventHubQualifiedNameSpace' -Value $eventHubQualifiedNameSpace
WriteVarToHost -Key 'AutoResetJobIntervalInSeconds' -Value $autoResetJobIntervalInSeconds
WriteVarToHost -Key 'MaxElapsedTimeInMinutes' -Value $maxElapsedTimeInMinutes
WriteVarToHost -Key 'DaimlerRelayUsername' -Value $daimlerRelayUsername
WriteVarToHost -Key 'DaimlerRelayPassword' -Value $daimlerRelayPassword
WriteVarToHost -Key 'AuthBaseUrl' -Value $authBaseUrl


#Assign Frontend Environment Variable

WriteVarToHost -Key 'VersionReleaseName' -Value $versionReleaseName
WriteVarToHost -Key 'VersionBuildNumber' -Value $versionBuildNumber
WriteVarToHost -Key 'HouseKeepingInterval' -Value $houseKeepingInterval
WriteVarToHost -Key 'HistoryDuration' -Value $historyDuration
WriteVarToHost -Key 'Instance' -Value $instance


#Assign MI Pod Identity values

WriteVarToHost -Key 'ManagedIdentityName' -Value $ManagedIdentityName
WriteVarToHost -Key 'ManagedIdentityResourceGroup' -Value $ManagedIdentityResourceGroup

##### MANAGED IDENTITY #####
# PrincipalID
$MsiPrincipalId = GetMSIPrincipalId -IdentityName $ManagedIdentityName -ResourceGroupName $ManagedIdentityResourceGroup
WriteVarToHost -Key 'MSI_PRINCIPAL_ID' -Value $MsiPrincipalId

# ApplicationID
$MsiClientId = GetMSIClientId -IdentityName $ManagedIdentityName -ResourceGroupName $ManagedIdentityResourceGroup
WriteVarToHost -Key 'MSI_CLIENT_ID' -Value $MsiClientId

WriteVarToHost -Key 'PodIdentityBindingName' -Value "e2ebackend-$MSI_NAME-binding"


WriteVarToHost -Key 'RESOURCES_REQUESTS_MEMORY' -Value $RESOURCES_REQUESTS_MEMORY # please also adapt thresholds in the dashboard if you change something here
WriteVarToHost -Key 'RESOURCES_REQUESTS_CPU' -Value $RESOURCES_REQUESTS_CPU # please also adapt thresholds in the dashboard if you change something here
WriteVarToHost -Key 'RESOURCES_LIMITS_MEMORY' -Value $RESOURCES_LIMITS_MEMORY
WriteVarToHost -Key 'RESOURCES_LIMITS_CPU' -Value $RESOURCES_LIMITS_CPU
WriteVarToHost -Key 'StageCsg' -Value $StageCsg
WriteVarToHost -Key 'SubscriptionIdCsg' -Value $SubscriptionIdCsg
WriteVarToHost -Key 'azureLocationsCsg' -Value (ConvertTo-Json $azureLocationsCsg -Compress)
#WriteVarToHost -Key 'DevGroupObjectId' -Value '1f177605-5c14-4066-86a3-088aff33fb1f' # az_001-csgp_crew-das-1
WriteVarToHost -Key 'MinReplicas' -Value $MinReplicas
WriteVarToHost -Key 'MaxReplicas' -Value $MaxReplicas

WriteVarToHost -Key 'ServiceLoglevel' -Value 'info'
$AzureAppConfigAzureUriAzureGlobal = ".azconfig.io"
$AzureAppConfigAzureUriAzureChina = ".azconfig.azure.cn"
$ContainerRegistryLoginServerAzureGlobal = "csgcrewe2emonitoring.azurecr.io"
$ContainerRegistryLoginServerAzureChina = "csgcrewdas1.azurecr.cn"
$AadAuthenticationEndpointAzureGlobal = "https://login.microsoftonline.com"
$AadAuthenticationEndpointAzureChina = "https://login.partner.microsoftonline.cn"
$AadIssuerUrlAzureGlobal = "https://sts.windows.net/$AadTenantId/"
$AadIssuerUrlAzureChina = "https://sts.chinacloudapi.cn/$AadTenantId/"
$BuildAgentObjectIdGlobal = '0df0e39a-77fe-4f2c-9cf0-4d4d1273bee0' # DAI-GB9_csgp-deployment_crew-das-1_715-3
$BuildAgentObjectIdChina = "3e0370fc-f634-4534-b590-97ee6dcb0b4a" # DAI-GB9_csgp-deployment_crew-das-1
 

# REGIONAL VARS CSG
if ($LocationShortCsg -eq "weu") {
    $LocationCsg = "westeurope"
    $LocationShortCsg = "weu"
    $LocationInfixDnsCsg = "eu"
    $LocationInfixCsg = "eu"
    $DnsUrlRegionWithDots = ".eu"    
    $APP_CONFIG_AZURE_URI = $AzureAppConfigAzureUriAzureGlobal
    $ContainerRegistryLoginServer = $ContainerRegistryLoginServerAzureGlobal
    $AadAuthenticationEndpoint = $AadAuthenticationEndpointAzureGlobal
    $AadIssuerUrl = $AadIssuerUrlAzureGlobal
    $BuildAgentObjectId = $BuildAgentObjectIdGlobal
}elseif ($LocationShortCsg -eq "wus") {
    $LocationCsg = "westus"
    $LocationShortCsg = "wus"
    $LocationInfixDnsCsg = "us"
    $LocationInfixCsg = "us"
    $DnsUrlRegionWithDots = ".us"
    $APP_CONFIG_AZURE_URI = $AzureAppConfigAzureUriAzureGlobal
    $ContainerRegistryLoginServer = $ContainerRegistryLoginServerAzureGlobal
    $AadAuthenticationEndpoint = $AadAuthenticationEndpointAzureGlobal
    $AadIssuerUrl = $AadIssuerUrlAzureGlobal
    $BuildAgentObjectId = $BuildAgentObjectIdGlobal
}
elseif ($LocationShortCsg -eq "eus2") {
    $LocationCsg = "eastus2"
    $LocationShortCsg = "eus2"
    $LocationInfixDnsCsg = "us"
    $LocationInfixCsg = "us"
    $DnsUrlRegionWithDots = ".us"
    $APP_CONFIG_AZURE_URI = $AzureAppConfigAzureUriAzureGlobal
    $ContainerRegistryLoginServer = $ContainerRegistryLoginServerAzureGlobal
    $AadAuthenticationEndpoint = $AadAuthenticationEndpointAzureGlobal
    $AadIssuerUrl = $AadIssuerUrlAzureGlobal
    $BuildAgentObjectId = $BuildAgentObjectIdGlobal
}
elseif ($LocationShortCsg -eq "jpe") {
    $LocationCsg = "japaneast"
    $LocationShortCsg = "jpe"
    $LocationInfixDnsCsg = "jp"
    $LocationInfixCsg = "jp"
    $DnsUrlRegionWithDots = ".jp"
    $APP_CONFIG_AZURE_URI = $AzureAppConfigAzureUriAzureGlobal
    $ContainerRegistryLoginServer = $ContainerRegistryLoginServerAzureGlobal
    $AadAuthenticationEndpoint = $AadAuthenticationEndpointAzureGlobal
    $AadIssuerUrl = $AadIssuerUrlAzureGlobal
    $BuildAgentObjectId = $BuildAgentObjectIdGlobal
} elseif ($LocationShortCsg -eq "cne2") {
    $LocationCsg = "chinaeast2"
    $LocationShortCsg = "cne2"
    $LocationInfixDnsCsg = ""
    $LocationInfixCsg = "cn"
    $DnsUrlRegionWithDots = ""
    $APP_CONFIG_AZURE_URI = $AzureAppConfigAzureUriAzureChina
    $ContainerRegistryLoginServer = $ContainerRegistryLoginServerAzureChina
    $AadAuthenticationEndpoint = $AadAuthenticationEndpointAzureChina
    $AadIssuerUrl = $AadIssuerUrlAzureChina
    $BuildAgentObjectId = $BuildAgentObjectIdChina
}

WriteVarToHost -Key 'BuildAgentObjectId' -Value $BuildAgentObjectId
WriteVarToHost -Key 'LocationCsg' -Value $LocationCsg
WriteVarToHost -Key 'LocationShortCsg' -Value $LocationShortCsg
WriteVarToHost -Key 'ContainerRegistryLoginServer' -Value $ContainerRegistryLoginServer
WriteVarToHost -Key 'LocationInfixCsg' -Value $LocationInfixCsg
WriteVarToHost -Key 'AadIssuerUrl' -Value $AadIssuerUrl

######## RESOURCE GROUP ########

$computedResourceGroupName = $OrgUnitCsg + "-" + $LocationShortCsg + "-" + $StageCsg + "-" + $E2EMonitoringServiceShort
WriteVarToHost -Key 'ComputedResourceGroupName' -Value $computedResourceGroupName

######## App Insights ########
<# function GetAppInsightsInstrumentationKey($AppInsightsName, $AppInsightsResourceGroupName) {
    Write-Host "Adding application insights extension"
    az extension add -n application-insights
    Write-Host "Getting AppInsights Instrumentation Key for AppInsightsName= $AppInsightsName in ResourceGroup= $AppInsightsResourceGroupName"
    return (az monitor app-insights component show --app "$AppInsightsName"  -g "$AppInsightsResourceGroupName" --query "instrumentationKey").Trim('"')
} #>

function GetSecret($KeyVaultName, $SecretName) {
    #return ( az keyvault secret show --name "DBConnectionString" --vault-name $KeyVaultName --query "value")
    return ( az keyvault secret show --name $SecretName --vault-name $KeyVaultName --query "value")

}



<# if ($Environment -eq "dev") {
    $appInsightsNameCsg ="csgprovidencedev"
    $appInsightsConnectionString = "InstrumentationKey=e3ec21a6-f3e6-4658-bdfc-201bb623539c;IngestionEndpoint=https://westeurope-5.in.applicationinsights.azure.com/;LiveEndpoint=https://westeurope.livediagnostics.monitor.azure.com/"
} elseif($Environment -eq "stg") {
    $appInsightsNameCsg ="csg-stg-mon-ai-weu"
    $appInsightsConnectionString = "InstrumentationKey=7d7c117f-e567-4238-96e7-0cf059b1177f;IngestionEndpoint=https://westeurope-5.in.applicationinsights.azure.com/;LiveEndpoint=https://westeurope.livediagnostics.monitor.azure.com/"
}elseif($Environment -eq "stg_cne2") {
    $appInsightsNameCsg ="csg-stg-e2e-mon-ai-cne2"
    $appInsightsConnectionString = "InstrumentationKey=37cbc263-7791-c2ce-a315-648c69ddf0b1;EndpointSuffix=applicationinsights.azure.cn;IngestionEndpoint=https://chinaeast2-0.in.applicationinsights.azure.cn/;AADAudience=https://monitor.azure.cn/"
}elseif($Environment -eq "prod_weu") {
    #TODO: Populate app insights name
    $appInsightsNameCsg ="csg-stg-mon-ai-weu"
    $appInsightsConnectionString = "InstrumentationKey=295d244e-f251-46db-87ab-c61998a7baef;IngestionEndpoint=https://westeurope-5.in.applicationinsights.azure.com/;LiveEndpoint=https://westeurope.livediagnostics.monitor.azure.com/"
}elseif($Environment -eq "prod_eus2") {
    #TODO: Populate app insights name
    $appInsightsNameCsg ="csg-stg-mon-ai-weu"
    $appInsightsConnectionString = "InstrumentationKey=853682c3-08a9-4794-8b62-227a84092033;IngestionEndpoint=https://eastus2-3.in.applicationinsights.azure.com/;LiveEndpoint=https://eastus2.livediagnostics.monitor.azure.com/"
}elseif($Environment -eq "prod_jpe") {
    #TODO: Populate app insights name
    $appInsightsNameCsg ="csg-stg-mon-ai-weu"
    $appInsightsConnectionString = "InstrumentationKey=9db44488-7f81-426f-bc68-a829613f8c1c;IngestionEndpoint=https://japaneast-1.in.applicationinsights.azure.com/;LiveEndpoint=https://japaneast.livediagnostics.monitor.azure.com/"
}elseif($Environment -eq "prod_cne2") {
    #TODO: Populate app insights name
    $appInsightsNameCsg ="csg-stg-mon-ai-weu"
    $appInsightsConnectionString = ""
} #>


#WriteVarToHost -Key 'AppInsightsNameCsg' -Value $appInsightsNameCsg

# csg-weu-dev-e2e-mon


$appInsightsResourceGroupNameCsg = $OrgUnitCsg + "-" + $LocationShortCsg + "-" + $StageCsg + "-e2e-mon"
WriteVarToHost -Key 'AppInsightsResourceGroupNameCsg' -Value $appInsightsResourceGroupNameCsg

#$ApplicationInsightsInstrumentationKeyCsg = GetAppInsightsInstrumentationKey $appInsightsNameCsg $appInsightsResourceGroupNameCsg

<# Write-Host "AppInsights Key is $ApplicationInsightsInstrumentationKeyCsg"
WriteVarToHost -Key 'ApplicationInsightsInstrumentationKeyCsg' -Value $ApplicationInsightsInstrumentationKeyCsg
 #>
 



######## KeyVault ########
# csg-weu-dev-stream-kv
$computedKeyVaultNameCsg = $OrgUnitCsg + "-" + $StageCsg + "-e2e-mon-kv-" + $LocationShortCsg
WriteVarToHost -Key 'ComputedKeyVaultNameCsg' -Value $computedKeyVaultNameCsg  -ToLower

#Getting app insights connection string from Key vault
$appInsightsConnectionString = GetSecret $computedKeyVaultNameCsg "AIConnectionString"
WriteVarToHost -Key 'AppInsightsConnectionString' -Value $appInsightsConnectionString

#Getting database connection string from Key vault
$DatabaseConnectionString = GetSecret $computedKeyVaultNameCsg "DBConnectionString"
WriteVarToHost -Key 'DatabaseConnectionString' -Value $DatabaseConnectionString


######## Nuget Stuff ########
WriteVarToHost -Key 'infrastructureNugetVersion' -Value $infrastructureNugetVersion
$NugetRootFolder = "Daimler.Connectivity.CoBa.Infrastructure." + $infrastructureNugetVersion
WriteVarToHost -Key 'nugetRootFolder' -Value $NugetRootFolder
$NugetIntallPath = $WorkingDir + "/cobaps"
WriteVarToHost -Key 'nugetInstallPath' -Value $NugetIntallPath
$NugetCallPath = $NugetIntallPath + "/" + $NugetRootFolder + "/content/Infra"
WriteVarToHost -Key 'nugetCallPath' -Value $NugetCallPath


###### APIM VARIABLES ######
$EnvInfixOptionalDot = ""
if($EnvInfix) {
    $EnvInfixOptionalDot = $EnvInfix + "."
}
if($LocationInfixDnsCsg) {
    $LocationInfixDnsCsgOptionalDot = $LocationInfixDnsCsg + "."
}
$backendServiceUrl = "https://e2emonitoringapi." + $LocationInfixDnsCsgOptionalDot + $EnvInfixOptionalDot + "private.csg." + $CompanyNameShortDnsCsg + ".com" # tbd check for each new stage if naming pattern fits

#### AAD Variables ####
function GetApplicationIdByDisplayName ($azureApplicationDisplayName) {
    if($azureApplicationDisplayName) {
        return (az ad sp list --display-name $azureApplicationDisplayName | ConvertFrom-Json).appId
    }
    Write-Warning "No azureApplicationDisplayName $azureApplicationDisplayName provided. Returning empty string."
    return ""
}


WriteVarToHost -Key 'E2EAADApplicationId' -Value $E2EAADApplicationId

WriteVarToHost -Key 'AadTenantId' -Value $AadTenantId

WriteVarToHost -Key 'AadAuthenticationEndpoint' -Value $AadAuthenticationEndpoint

WriteVarToHost -Key 'CoBaAADClientIdKeyName' -Value 'COBAPARTNERAADCLIENTID'
WriteVarToHost -Key 'CoBaAADClientSecretKeyName' -Value 'COBAPARTNERAADCLIENTSECRET'

if ($Environment -eq "dev") {
    $CoBaTenantSuffix = "-COBA-TENANT"
} else {
    $CoBaTenantSuffix = ""
}
$CoBaAADClientIdKeyNameCsg = "AAD-partner-coba-clientId" + $CoBaTenantSuffix # AAD-partner-coba-clientId-COBA-TENANT
WriteVarToHost -Key 'CoBaAADClientIdKeyNameCsg' -Value $CoBaAADClientIdKeyNameCsg
$CoBaAADClientSecretNameCsg = "AAD-partner-coba-clientSecret" + $CoBaTenantSuffix # AAD-partner-coba-clientSecret-COBA-TENANT
WriteVarToHost -Key 'CoBaAADClientSecretKeyNameCsg' -Value $CoBaAADClientSecretNameCsg

###### KUBERNETES #####
# "Azure MSI resources are linked by tag k8snamespace, you can specify multiple namespaces with comma" https://docs.ops.csg.daimler-truck.com/UsersHandbook/kubernetes/best-practises/pod-identity.html

if ($Environment -eq "dev") {
    $k8sNamespaceE2E  ="dev-e2e-mon"
} else {
    $k8sNamespaceE2E  ="stg-e2e-mon"
}

if ($Environment -eq "dev") {
    $k8sNamespaceE2E  ="dev-e2e-mon"
} elseif($Environment -eq "stg") {
    $k8sNamespaceE2E  ="stg-e2e-mon"
}elseif($Environment -eq "stg_cne2") {
    $k8sNamespaceE2E  ="stg-e2e-mon"    
} elseif($Environment -eq "prod_weu") {
    $k8sNamespaceE2E  ="prod-weu-e2e-mon"    
}elseif($Environment -eq "prod_eus2") {
    $k8sNamespaceE2E  ="prod-eus2-e2e-mon"    
}elseif($Environment -eq "prod_jpe") {
    $k8sNamespaceE2E  ="prod-jpe-e2e-mon"    
}elseif($Environment -eq "prod_cne2") {
    $k8sNamespaceE2E  ="prod-cne2-e2e-mon"    
}
WriteVarToHost -Key 'K8S_NAMESPACES_E2E' -Value $k8sNamespaceE2E

# Ingress Host
#e2emonitoringservice.eu.staging.csg.daimler-truck.com - for Staging
$IngressHost = "e2emonitoringservice." + $LocationInfixDnsCsgOptionalDot + $EnvInfixOptionalDot + "csg." + $CompanyNameShortDnsCsg + ".com"
WriteVarToHost -Key "IngressHost" -Value $IngressHost

$E2EBackendConfigEndPoint = "https://"+ $IngressHost + "/api/config"
$E2EBackendDataEndPoint = "https://"+ $IngressHost + "/api"
$E2EBackendSignalREndPoint = "https://"+ $IngressHost

# Ingress Host Front End
#e2emonitoring.eu.staging.csg.daimler-truck.com - for Staging
$IngressHostFrontEnd = "e2emonitoring." + $LocationInfixDnsCsgOptionalDot + $EnvInfixOptionalDot + "csg." + $CompanyNameShortDnsCsg + ".com"
WriteVarToHost -Key "IngressHostFrontEnd" -Value $IngressHostFrontEnd 


WriteVarToHost -Key "E2EBackendConfigEndPoint" -Value $E2EBackendConfigEndPoint 
WriteVarToHost -Key "E2EBackendDataEndPoint" -Value $E2EBackendDataEndPoint 
WriteVarToHost -Key "E2EBackendSignalREndPoint" -Value $E2EBackendSignalREndPoint 

##### NETWORK #####

$AppSubnetResourceIds = @()

foreach ($regionalObject in $csgRegionalSubscriptionObjects) {
    # csg-weu-dev-network
    if($StageCsg -eq 'stg'){
        $networkStageCsg = 'staging'
    } else {
        $networkStageCsg = $StageCsg
    }
    $NetworkResourceGroupCsg = "$OrgUnitCsg-$($regionalObject.LocationShort)-$networkStageCsg-network"

    #vnet-weu-dev
    $NetworkVnetNameCsg = "vnet-$($regionalObject.LocationShort)-$StageCsg"

    $AppSubnetResourceIds += "/subscriptions/$($regionalObject.Subscription)/resourceGroups/$NetworkResourceGroupCsg/providers/Microsoft.Network/virtualNetworks/$NetworkVnetNameCsg/subnets/polaris-k8s"

}

WriteVarToHost -Key 'AppSubnetResourceIds' -Value (ConvertTo-Json $AppSubnetResourceIds -Compress)

