param (
    [parameter(Mandatory = $true)]
    [ValidateNotNullOrEmpty()]
    [String] $environment
)




#Initialize constants.
Set-Variable development -Option Constant -Value "Development"
Set-Variable staging -Option Constant -Value "Staging"
Set-Variable euProduction -Option Constant -Value "Europe Production"
Set-Variable usProduction -Option Constant -Value "US Production"
Set-Variable jpProduction -Option Constant -Value "Japan Production"
Set-Variable chinaStaging -Option Constant -Value "China Staging"
Set-Variable chinaProduction -Option Constant -Value "China Production"


function WriteVarToPipeline {
  param (
      [String] $Key,
      $Value,
      [switch] $ToLower
  )
  if($ToLower) {
      $Value = $Value.ToLower()
  }

  # Boolean and Int Types don't support Trim Operation
  if($Value.getType().Name -ne "Boolean" -And $Value.getType().Name -ne "Int32" -And $Value.getType().Name -ne "Object[]") {
      $Value  = $Value.Trim()
  }
 
  Write-Host "##vso[task.setvariable variable=$Key;isOutput=true]$Value"

  Write-Host "$Key : $Value"
}


function getLocation {
  param (
    [String] $environment
)
  switch ($environment)
  {
    { ($_ -eq $development) -or ($_ -eq $staging) -or ($_ -eq $euProduction) } { return "weu"; break}
    $usProduction { return "eus2"; break}
    $jpProduction { return "jpe"; break}
    $chinaStaging { return "cne2"; break}
    $chinaProduction { return  "cne2"; break}
    Default {
        Write-Host "No matches"
    }
  }
}


function getEnvironment {
    param (
      [String] $environment
  )
    switch ($environment)
    {
        $development { return "dev"; break}
        { ($_ -eq $staging) -or  ($_ -eq $chinaStaging) } { return "stg"; break}
        { ($_ -eq $jpProduction) -or ($_ -eq $usProduction) -or ($_ -eq $euProduction) -or ($_ -eq $chinaProduction) } { return "prod"; break}
        Default {
            Write-Host "No matches"
        }
    }
}

#set constants

$environmentInputfromUser = $environment
Write-Host "Environment sent is :: " $environment
#Get location details on the basis of input from the pipeline.
$location = getLocation($environment)
WriteVarToPipeline -Key 'location' -Value $location

#set environment value
$environment = getEnvironment($environment)
WriteVarToPipeline -Key 'environment' -Value $environment



#initalize the base variables that is applicable across Deployment in pipelines 
# WriteVarToPipeline -Key 'AzureGroupName' -Value "az_001-csgp_crew-e2e-mon"
# WriteVarToPipeline -Key 'BuiltInRoleID_eventhub' -Value "f526a384-b230-433a-b45c-95f59c4a2dec"
# WriteVarToPipeline -Key 'BuiltInRoleID_storageaccount' -Value "ba92f5b4-2d11-453d-a403-e96b0029c9fe"
# WriteVarToPipeline -Key 'EventHub_SkuTierName' -Value "Standard"
# WriteVarToPipeline -Key 'MSIAdministratorType_sqlserver' -Value "ActiveDirectory"
# WriteVarToPipeline -Key 'MSIPrincipalType_sqlserver' -Value "Group"
# WriteVarToPipeline -Key 'AzureGroupSId' -Value "84be0b2f-e620-4a89-945c-f680c7d77986"
# WriteVarToPipeline -Key 'SubNetName' -Value "polaris-k8s"
WriteVarToPipeline -Key 'TenantId' -Value "9652d7c2-1ccf-4940-8151-4a92bd474ed0"
# WriteVarToPipeline -Key 'EmailListVulnerabilityAssessment' -Value '["sandeep.kulkarni@daimlertruck.com","hina.singh@daimlertruck.com","parna.dutta@daimlertruck.com","madhusudhan.b@daimlertruck.com","mallika.sachan@daimlertruck.com"]'

if($location -eq "cne2") 
{
    #yet to assign
    WriteVarToPipeline -Key 'TenantId' -Value '5b09bf9a-a60b-46f7-a92c-1a72a845c331'
}




#generate variables on the basis of the environment.
# $env:MYSECRET
function generateVariablesForEnvironment ($inputenv,$inputlocation,$objectId,$serviceConnection,$subscriptionId) {
        

    $k8snamespace = $inputenv + "-" + $inputlocation + '-e2e-mon'
    WriteVarToPipeline -Key 'K8sNamespace' -Value $k8snamespace

    WriteVarToPipeline -Key 'objectId' -Value $objectId

    #csg-weu-dev-e2e-mon
    $resourceGroupName = 'csg-' + $inputlocation + '-' + $inputenv + '-e2e-mon'
    WriteVarToPipeline -Key 'resourceGroupName' -Value $resourceGroupName

    #csg-weu-dev-e2e-mon-msi
    $resourceGroupNamemsi = 'csg-' + $inputlocation + '-' + $inputenv + '-e2e-mon-msi'
    WriteVarToPipeline -Key 'resourceGroupNamemsi' -Value $resourceGroupNamemsi

    #csg-azure-dev-eu-e2e-monitoring
    WriteVarToPipeline -Key 'serviceConnection' -Value $serviceConnection

    #subcriptionId
    WriteVarToPipeline -Key 'subscriptionId' -Value $subscriptionId
    #vnet
    #vnet-weu-dev
    $vnet = "vnet-" + $inputlocation + "-" + $inputenv
    WriteVarToPipeline -Key 'VNetName' -Value $vnet
    #csg-weu-dev-network
    $vnetResourceGroup = "csg-" + $inputlocation + "-" + $inputenv +"-network"
    WriteVarToPipeline -Key 'VNetResourceGroupName' -Value $vnetResourceGroup

}


switch ($environmentInputfromUser) {
    $development {  
        generateVariablesForEnvironment -inputenv $environment -inputlocation $location -objectId '8327b169-1a6d-4bf4-b6bd-21c179f37ed3' -serviceConnection 'csg-azure-dev-eu-e2e-monitoring' -subscriptionId '8900235a-4634-414a-9ca6-0b391bac3eef'
        break
    }
    $staging {  
        generateVariablesForEnvironment -inputenv $environment -inputlocation $location -objectId '8327b169-1a6d-4bf4-b6bd-21c179f37ed3' -serviceConnection 'csg-azure-stg-eu-e2e-monitoring' -subscriptionId 'c1f14484-3fe1-4d20-9f9c-031d6ebf80e1'
        Break
    }
    $euProduction {  
        generateVariablesForEnvironment -inputenv $environment -inputlocation $location -objectId '8327b169-1a6d-4bf4-b6bd-21c179f37ed3' -serviceConnection 'csg-azure-prod-eu-e2e-monitoring' -subscriptionId 'f363028c-0c18-4fc1-b3da-addc4415a6dd'
        break
    }
    $usProduction {  
        generateVariablesForEnvironment -inputenv $environment -inputlocation $location -objectId '8327b169-1a6d-4bf4-b6bd-21c179f37ed3' -serviceConnection ' csg-azure-prod-us-e2e-monitoring' -subscriptionId '74515af6-f941-4a14-a5d9-c8974ce6d8a3'
        break
    }
    $jpProduction {  
        generateVariablesForEnvironment -inputenv $environment -inputlocation $location -objectId '8327b169-1a6d-4bf4-b6bd-21c179f37ed3' -serviceConnection 'csg-azure-prod-jp-e2e-monitoring' -subscriptionId '423d04e0-9b86-4d55-9674-2efa1a04b202'
        break
    }
    $chinaStaging {  
        generateVariablesForEnvironment -inputenv $environment -inputlocation $location -objectId '5ff8a0eb-f8d7-47d8-854b-6a470b7cd383' -serviceConnection 'csg-azure-cn-staging-e2e-monitoring' -subscriptionId 'e89715f7-2fe8-48fd-a034-3f99ed93da5a'
        break
    }
    $chinaProduction {  
        generateVariablesForEnvironment -inputenv $environment -inputlocation $location -objectId '5ff8a0eb-f8d7-47d8-854b-6a470b7cd383' -serviceConnection 'csg-azure-cn-prod-e2e-monitoring' -subscriptionId 'c6ed4c5e-1e9d-4378-8945-30bb2b08cf63'
        break
    }
    Default {
        Write-Host 'Nothing matched for generating variables'
    }
}