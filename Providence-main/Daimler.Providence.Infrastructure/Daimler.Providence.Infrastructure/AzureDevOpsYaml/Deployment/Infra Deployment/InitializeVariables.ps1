param (
    [parameter(Mandatory = $true)]
    [ValidateNotNullOrEmpty()]
    [String] $envName, 

    [parameter(Mandatory = $true)]
    [ValidateNotNullOrEmpty()]
    [String] $resourceType, 

    [parameter(Mandatory = $true)]
    [ValidateNotNullOrEmpty()]
    [String] $location
)
try
{
  Write-Host "envName: "$envName
  Write-Host "resourceType: "$resourceType
  Write-Host "location: "$location

  $resourceTypeList = ""
  $locationCode = ""  
  $SuffixUrlForFuncApp = ".azurewebsites.net"
  $zoneRedundantValue = "true"
  $envNameLower = $envName.ToLower()
  $resourceType = $resourceType.ToLower()
  #$allResourceName = ('msi', 'keyvault', 'loganalytics','nsgwithoutrules', 'nsg','vnetwithoutsubnet', 'vnet', 'storageaccount','storageaccountva', 'appserviceplan', 'functionapp', 'eventhub', 'appinsights', 'sqlserver')
  $allResourceName=('msi','keyvault','loganalytics','nsg','vnet','storageaccount','appserviceplan','functionapp','eventhub','appinsights','sqlserver')

  if($resourceType.ToLower() -eq 'all')
  {
      $resourceTypeList = $allResourceName
  }
  else
  {
      if($allResourceName.Contains($resourceType))
      {
        $resourceTypeList = ($resourceType)
      } 
      else
      {
        Write-Host "Please spell resource type correctly...." 
        Write-Host "Please pass resource type as msi, eventhub, functionapp, sqlserver, storageaccount, loganalytics or appinsights"          
        exit 1;
      }
  }

  #location can be shifted to hash table
  #based on location setting location code
  if($location.ToLower() -eq 'weu')
  {
      $locationCode = 'weu'
      $location = 'WestEurope'
  }
  elseif($location.ToLower() -eq 'neu')
  {
      $locationCode = 'neu'
      $location = 'northeurope'
  }
  elseif($location.ToLower() -eq 'jpe')
  {
      $locationCode = 'jpe'
      $location = 'japaneast'
  }
  elseif($location.ToLower() -eq 'cne2')
  {
      $locationCode = 'cne2'
      $location = 'chinaeast2'
      $SuffixUrlForFuncApp = ".chinacloudsites.cn"
      $zoneRedundantValue = "false"
  }
  elseif($location.ToLower() -eq 'eus2')
  {
      $locationCode = 'eus2'
      $location = 'eastus2'
  }
  else
  {
      Write-Host "Location not found......"
      Write-Host "Available locations are weu, neu, jpe, cne2, eus2"
      exit 1;
  }

  Write-Host "Resource deploying Location - $locationCode"

  Write-Host "Setting resource name"


  function SetVarToOutput {
    param (
        [String] $Key,
        [String] $Value
    )   

    Write-Host "##vso[task.setvariable variable=$Key;isOutput=true]$Value"
   }

   function SetResourceName {
    param (
        [String] $resTag,
        [string] $resName,
        [string] $location
    )   
    if($location) 
    {
        $name = "csg-" + $envNameLower + $resTag + $location
    }
    else 
    {
        $name = "csg-" + $envNameLower + $resTag + $locationCode
    }
    Write-Host "Setting $resName name - $name"
    return $name
   }
  
  $overrideParameters ="overrideParameters";
  $NameOut = "NameOut";  
  
  #msi
  $MsiResourceName = "csg-" + $locationCode  + "-" + $envNameLower  +  "-e2e-msi"
  Write-Host "Setting Msi name - $MsiResourceName"
  $msiParams = "-Msi_Name $MsiResourceName -Location $location -K8s_Namespace `$(K8sNamespace) -MsiResourceGroupName `$(resourceGroupNamemsi)"
  SetVarToOutput -Key ($overrideParameters + "msi") -Value $msiParams
  SetVarToOutput -Key ("msi" + $NameOut) -Value  $MsiResourceName
 
  #keyvault
  $KeyVaultResourceName = SetResourceName -resTag "-e2e-mon-kv-" -resName "keyvault"
  $keyvaultParams = "-keyvault_name $KeyVaultResourceName -Location $location -Tenant_id `$(TenantId) -Pipeline_Object_id `$(objectId)"
  SetVarToOutput -Key ($overrideParameters + "keyvault") -Value $keyvaultParams
  SetVarToOutput -Key ("keyvault" + $NameOut) -Value $KeyVaultResourceName
  SetVarToOutput -Key ("secret" + $NameOut) -Value "test"
  SetVarToOutput -Key "secretValueOut" -Value "test"

  #loganalytics
  $LoganalyticsResourceName = SetResourceName -resTag "-e2e-mon-log-" -resName "Loganalytics"
  $loganalyticsParams = "-LogAnalytics_Name $LoganalyticsResourceName -Location $Location"
  SetVarToOutput -Key ($overrideParameters + "loganalytics") -Value $loganalyticsParams
  SetVarToOutput -Key ("loganalytic" + $NameOut) -Value $LoganalyticsResourceName

  #nsg  
  #nsgwithoutrules
  $NSGResourceName = SetResourceName -resTag "-e2e-mon-mgmt-nsg-" -resName "NSG"
  $nsgParams = "-nsg_name $NSGResourceName -Location $location -SecurityRuleEHName AllowFuncAppToEH -SecurityRuleSAName AllowFuncAppToStorage -LocationPrefix $location"
  $nsgWithoutRulesParams = "-nsg_name $NSGResourceName -Location $location"
  
  if($locationCode.ToLower() -eq 'cne2')
  {
    $nsgResourceNameCnNorth = SetResourceName -resTag "-e2e-mon-mgmt-nsg-" -resName "NSG" -location "cnn3"
    $nsgcnLocation = 'chinanorth3'   
    $nsgParamsCNnorth = "-nsg_name $nsgResourceNameCnNorth -Location $nsgcnLocation -SecurityRuleEHName AllowFuncAppToEH -SecurityRuleSAName AllowFuncAppToStorage -LocationPrefix $location"
    $nsgWithoutRulesParamsCnNorth = "-nsg_name $nsgResourceNameCnNorth -Location $nsgcnLocation"
        
    SetVarToOutput -Key ($overrideParameters + "nsgwithoutrulescnnorth") -Value $nsgWithoutRulesParamsCnNorth
    SetVarToOutput -Key ($overrideParameters + "nsgcnnorth") -Value $nsgParamsCNnorth
    SetVarToOutput -Key ("nsgcnnorth" + $NameOut) -Value $nsgResourceNameCnNorth
  }


  SetVarToOutput -Key ($overrideParameters + "nsgwithoutrules") -Value $nsgWithoutRulesParams
  SetVarToOutput -Key ($overrideParameters + "nsg") -Value $nsgParams
  SetVarToOutput -Key ("nsg" + $NameOut) -Value $NSGResourceName

  #vnet
  #vnetwithoutsubnet
  $VNete2eResourceName = SetResourceName -resTag "-e2e-mon-vnet-" -resName "VNet"
  $SubNete2eResourceName = "BackendSubnet"
  Write-Host "SubNet Name - $SubNete2eResourceName"
  $vnetParams = "-vnet_name $VNete2eResourceName -subnet_name $SubNete2eResourceName -nsg_name $NSGResourceName -Location $location"
  $vnetwithoutsubnetParams = "-vnet_name $VNete2eResourceName -Location $location"

  if($locationCode.ToLower() -eq 'cne2')
  {
    $VNete2eResourceNameCnNorth = SetResourceName -resTag "-e2e-mon-vnet-" -resName "VNet" -location "cnn3"
    $SubNete2eResourceNameCnNorth = "BackendSubnetCnn3"
    Write-Host "SubNet Name - $SubNete2eResourceNameCnNorth"
    $vnetlocationcnn3 = 'chinanorth3'  
    $vnetParamsCnNorth = "-vnet_name $VNete2eResourceNameCnNorth -subnet_name $SubNete2eResourceNameCnNorth -nsg_name $nsgResourceNameCnNorth -Location $vnetlocationcnn3"
    $vnetwithoutsubnetParamsCnNorth = "-vnet_name $VNete2eResourceNameCnNorth -Location $vnetlocationcnn3"
        

    SetVarToOutput -Key ($overrideParameters + "vnetwithoutsubnetcnnorth") -Value $vnetwithoutsubnetParamsCnNorth
    SetVarToOutput -Key ($overrideParameters + "vnetcnnorth") -Value $vnetParamsCnNorth
    SetVarToOutput -Key ("vnet" + $NameOut) -Value $VNete2eResourceNameCnNorth
    SetVarToOutput -Key ("subnet" + $NameOut) -Value $SubNete2eResourceNameCnNorth
  }

  SetVarToOutput -Key ($overrideParameters + "vnetwithoutsubnet") -Value $vnetwithoutsubnetParams
  SetVarToOutput -Key ($overrideParameters + "vnet") -Value $vnetParams
  SetVarToOutput -Key ("vnet" + $NameOut) -Value $VNete2eResourceName
  SetVarToOutput -Key ("subnet" + $NameOut) -Value $SubNete2eResourceName

  #appserviceplan
  $AppServicePlanResourceName = SetResourceName -resTag "-e2e-mon-svcplan-" -resName "Appserviceplan"
  $appserviceplanParams = "-Location $location -App_Service_Plan_Name $AppServicePlanResourceName"
  SetVarToOutput -Key ($overrideParameters + "appserviceplan") -Value $appserviceplanParams

  #functionapp
  $FunctionAppResourceName = SetResourceName -resTag "-e2e-mon-func-" -resName "Functionapp"
  $functionappParams = "-Name $FunctionAppResourceName -Location $location -App_Service_Plan_Name $AppServicePlanResourceName -MSI_Id $MsiResourceName -MSI_ResourceGroup_Name `$(resourceGroupNamemsi) -VNet_Name $VNete2eResourceName -Subnet_Name $SubNete2eResourceName -Suffix_Url $SuffixUrlForFuncApp"
  SetVarToOutput -Key ($overrideParameters + "functionapp") -Value $functionappParams
  SetVarToOutput -Key 'FunctionAppName' -Value $FunctionAppResourceName

  #functionappGrafana
  $GrafanaFunctionAppResourceName = SetResourceName -resTag "-e2e-mon-func-grafana-" -resName "Grafana Functionapp"
  $grafanafunctionappParams = "-Name $GrafanaFunctionAppResourceName -Location $location -App_Service_Plan_Name $AppServicePlanResourceName -MSI_Id $MsiResourceName -MSI_ResourceGroup_Name `$(resourceGroupNamemsi) -VNet_Name $VNete2eResourceName -Subnet_Name $SubNete2eResourceName -Suffix_Url $SuffixUrlForFuncApp"
  SetVarToOutput -Key ($overrideParameters + "grafanafunctionapp") -Value $grafanafunctionappParams
  SetVarToOutput -Key 'GrafanaFuncAppName' -Value $GrafanaFunctionAppResourceName

  #eventhub
  $EventHubResourceName = SetResourceName -resTag "01-eh-hub1-" -resName "Eventhub"
  $EventHubNamespaceName = SetResourceName -resTag "01-eh-" -resName "Eventhub Namespace"
  $eventhubParams = "-EventHub_Name $EventHubResourceName -EventHub_NameSpace_Name $EventHubNamespaceName -Location $Location -Principal_Id `$(RetrieveId.principalIdOut) -Subscription_Id `$(subscriptionId) -zoneRedundant $zoneRedundantValue -VNet_Name_Polaris `$(VNetName) -Subnet_Name_Polaris `$(SubNetName) -Vnet_ResourceGroup_Name_Polaris `$(VNetResourceGroupName) -Vnet_SubscriptionId_Id_Polaris `$(subscriptionId) -VNet_Name_e2e $VNete2eResourceName -Subnet_Name_e2e $SubNete2eResourceName"
  SetVarToOutput -Key ($overrideParameters + "eventhub") -Value $eventhubParams
 
  #appinsights
  $AppInsightsResourceName = SetResourceName -resTag "-e2e-mon-ai-" -resName "Appinsight"
  $appinsightsParams = "-AppInsight_Name $AppInsightsResourceName -Log_Analytics_Name_ResourceId `$(RetrieveResId.resourceIdOut) -Location $location"
  SetVarToOutput -Key ($overrideParameters + "appinsights") -Value $appinsightsParams
  $aiConnectionString = "`$(DatabaseARMOutput.AIConnectionString.value)" 
  SetVarToOutput -Key ("connectionstringai") -Value $aiConnectionString
  


  #storageaccount  
  $StorageAccountResourceName = "csg" + $envNameLower + "e2emonstorage" + $locationCode
  Write-Host "Setting StorageAccount name - $StorageAccountResourceName"
  $storageaccountParams = "-Storage_Account_Name $StorageAccountResourceName -Location $Location -Principal_Id `$(RetrieveId.principalIdOut) -VNet_Name_Polaris `$(VNetName) -Subnet_Name_Polaris `$(SubNetName) -Vnet_ResourceGroup_Name_Polaris `$(VNetResourceGroupName) -Vnet_SubscriptionId_Id_Polaris `$(subscriptionId) -Subscription_Id `$(subscriptionId) -VNet_Name_e2e $VNete2eResourceName -Subnet_Name_e2e $SubNete2eResourceName -EventHubContainer $EventHubResourceName"
  $storageaccountvaParams = "-Storage_Account_Name $StorageAccountResourceName -Location $Location -Subscription_Id `$(subscriptionId) -SqlPrincipal_Id `$(DatabaseARMOutput.sqlIdentity.value)"

  if($locationCode.ToLower() -eq 'cne2')
  {
    $StorageAccountResourceNameCnNorth = "csg" + $envNameLower + "e2emonstoragecnn3" 
    Write-Host "Setting StorageAccount name for chinanorth3 - $StorageAccountResourceNameCnNorth"

    $storagecnlocation = 'chinanorth3'   
    $storageaccountvaParamsCnNorth = "-Storage_Account_Name $StorageAccountResourceNameCnNorth -Location $storagecnlocation -Subscription_Id `$(subscriptionId) -SqlPrincipal_Id `$(DatabaseARMOutput.sqlIdentity.value)"        
    SetVarToOutput -Key ($overrideParameters + "storageaccountvacnnorth") -Value $storageaccountvaParamsCnNorth
  }

  SetVarToOutput -Key ($overrideParameters + "storageaccount") -Value $storageaccountParams
  #for vulnerability assessment
  SetVarToOutput -Key ($overrideParameters + "storageaccountva") -Value $storageaccountvaParams


  #sqlserver
  #sqlserverva
  #$SqlDbResourceName = SetResourceName -resTag "-e2e-mon-sql-" -resName "Sql Db"
  #$SqlDatabaseName =  SetResourceName -resTag "-e2e-mon-sqldb-" -resName "Sql Database"
  if($locationCode.ToLower() -eq 'cne2')
    {
        $SqlDbResourceName = SetResourceName -resTag "-e2e-mon-sql-" -resName "Sql Db" -location "cnn3"
        $SqlDatabaseName =  SetResourceName -resTag "-e2e-mon-sqldb-" -resName "Sql Database" -location "cnn3"
        $sqlcnLocation = 'chinanorth3'   
        # $sqlserverParams = "-IsPublicAccessEnabled Enabled -Location $sqlcnLocation -MinimalTlsVersion 1.2 -RestrictOutboundNetworkAccess Disabled -SQL_Admin_Login `$(SqlServer-Admin-Username) -SQL_Server_Name $SqlDbResourceName -SQL_Database_Name $SqlDatabaseName -Subscription_Id `$(subscriptionId) -SQL_Admin_Password `$(SqlServer-Admin-Password) -MSI_Id $MsiResourceName -MSI_Administrator_Type `$(MSIAdministratorType_sqlserver) -MSI_Principal_Type `$(MSIPrincipalType_sqlserver) -MSI_Login `$(AzureGroupName) -MSI_SID `$(AzureGroupSId) -MSI_Tenant_Id `$(TenantId) -MSI_ResourceGroup_Name `$(resourceGroupNamemsi) -MSI_AzureADOnlyAuthentication false -VNet_Name `$(VNetName) -Subnet_Name `$(SubNetName) -Vnet_ResourceGroup_Name `$(VNetResourceGroupName) -Vnet_SubscriptionId_Id `$(subscriptionId)"
        $sqlserverParams = "-IsPublicAccessEnabled Enabled -Location $sqlcnLocation -MinimalTlsVersion 1.2 -RestrictOutboundNetworkAccess Disabled -SQL_Admin_Login `$(SqlServer-Admin-Username) -SQL_Server_Name $SqlDbResourceName -SQL_Database_Name $SqlDatabaseName -Subscription_Id `$(subscriptionId) -SQL_Admin_Password `$(SqlServer-Admin-Password) -MSI_Id $MsiResourceName -MSI_Administrator_Type `$(MSIAdministratorType_sqlserver) -MSI_Principal_Type `$(MSIPrincipalType_sqlserver) -MSI_Login `$(AzureGroupName) -MSI_SID `$(AzureGroupSId) -MSI_Tenant_Id `$(TenantId) -MSI_ResourceGroup_Name `$(resourceGroupNamemsi) -MSI_AzureADOnlyAuthentication false -VNet_Name $VNete2eResourceNameCnNorth -Subnet_Name $SubNete2eResourceNameCnNorth -Vnet_ResourceGroup_Name `$(resourceGroupName) -Vnet_SubscriptionId_Id `$(subscriptionId)"
        Write-Host "Entered China SQL"
        $sqlservervaParams = "-Location $sqlcnLocation -SQL_Server_Name $SqlDbResourceName -StorageAccount_Name $StorageAccountResourceNameCnNorth -PrincipalId `$(DatabaseARMOutput.sqlIdentity.value) -EmailList `$(EmailListVulnerabilityAssessment) -TenantId `$(TenantId)"
    }
    else {
        $SqlDbResourceName = SetResourceName -resTag "-e2e-mon-sql-" -resName "Sql Db"
        $SqlDatabaseName =  SetResourceName -resTag "-e2e-mon-sqldb-" -resName "Sql Database"
        Write-Host "Entered others "
        $sqlserverParams = "-IsPublicAccessEnabled Enabled -Location $location -MinimalTlsVersion 1.2 -RestrictOutboundNetworkAccess Disabled -SQL_Admin_Login `$(SqlServer-Admin-Username) -SQL_Server_Name $SqlDbResourceName -SQL_Database_Name $SqlDatabaseName -Subscription_Id `$(subscriptionId) -SQL_Admin_Password `$(SqlServer-Admin-Password) -MSI_Id $MsiResourceName -MSI_Administrator_Type `$(MSIAdministratorType_sqlserver) -MSI_Principal_Type `$(MSIPrincipalType_sqlserver) -MSI_Login `$(AzureGroupName) -MSI_SID `$(AzureGroupSId) -MSI_Tenant_Id `$(TenantId) -MSI_ResourceGroup_Name `$(resourceGroupNamemsi) -MSI_AzureADOnlyAuthentication false -VNet_Name `$(VNetName) -Subnet_Name `$(SubNetName) -Vnet_ResourceGroup_Name `$(VNetResourceGroupName) -Vnet_SubscriptionId_Id `$(subscriptionId)"
        $sqlservervaParams = "-Location $location -SQL_Server_Name $SqlDbResourceName -StorageAccount_Name $StorageAccountResourceName -PrincipalId `$(DatabaseARMOutput.sqlIdentity.value) -EmailList `$(EmailListVulnerabilityAssessment) -TenantId `$(TenantId)"
    }
    $databaseConnectionString = "`$(DatabaseARMOutput.DatabaseConnectionString.value)"
    SetVarToOutput -Key ("connectionstringsqlserver") -Value $databaseConnectionString
    SetVarToOutput -Key ($overrideParameters + "sqlserver") -Value $sqlserverParams
    #for vulnerability assessment
    SetVarToOutput -Key ($overrideParameters + "sqlserverva") -Value $sqlservervaParams
    SetVarToOutput -Key ("sqlserver" + $NameOut) -Value $SqlDbResourceName
    SetVarToOutput -Key ("sqlDatabase" + $NameOut) -Value $SqlDatabaseName 
  
}
catch
{
    Write-Host "Exception occured.."
    exit 1;
}