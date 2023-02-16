param (
    [parameter(Mandatory = $true)]
    [String] $envName,

    [parameter(Mandatory = $true)]
    [String] $FuncAppType
)
try
{    
    function SetVarToOutput {
        param (
            [String] $ResGroupName,
            [String] $FuncAppName,
            [String] $StorageAccName,
            [String] $AppSettings
        ) 
        Write-Host "##vso[task.setvariable variable=ResGroupName;isOutput=true]$ResGroupName"      
        Write-Host "##vso[task.setvariable variable=FuncAppName;isOutput=true]$FuncAppName" 
        Write-Host "##vso[task.setvariable variable=StorageAccName;isOutput=true]$StorageAccName"
        Write-Host "##vso[task.setvariable variable=AppSettings;isOutput=true]$AppSettings"
    }

    $AppSettingsValue = ''
    $StorageAccNameValue = ''
    $FunctionAppNameValue = ''

    if($envName -eq 'DEV-EU')
    {
        $AppSettingsValue = '-AzureWebJobsStorage DefaultEndpointsProtocol=https;AccountName=csgdevmonstorageweu;AccountKey=4XvBqHR1UxU9ZSg9xYK/Mk6fqLKhAEJxiLqNo2AxpWP/TCKgdE4ia7Cb3cSvR9ZqJpKE35ozNgBvghnESfntqQ==;EndpointSuffix=core.windows.net -FUNCTIONS_EXTENSION_VERSION ~4 -EVENTHUB_NAME providencedev -WEBSITE_ENABLE_SYNC_UPDATE_SITE true -Instrumentation_key e3ec21a6-f3e6-4658-bdfc-201bb623539c -CLIENT_ID 2706c202-b9f3-4901-8294-d473e835e758 -EVENTHUB_HOSTNAME csgprovidencedev.servicebus.windows.net -WEBSITE_RUN_FROM_PACKAGE 1'
        $StorageAccNameValue = 'csgdevmonstorageweu'
        if($FuncAppType -eq 'ProvidenceFuncApp')
        {
            $FunctionAppNameValue = 'csg-dev-func-e2e-weu'
        }
        else
        {
            $FunctionAppNameValue = 'csg-dev-e2e-mon-func-grafana-weu'
        }
        SetVarToOutput -ResGroupName 'csg-weu-dev-e2e-mon' -FuncAppName $FunctionAppNameValue -StorageAccName $StorageAccNameValue -AppSettings $AppSettingsValue
    }
    elseif ($envName -eq 'STG-EU') {
        
        $AppSettingsValue = '-AzureWebJobsStorage DefaultEndpointsProtocol=https;AccountName=csgstgmonstorageweu;AccountKey=l8tkJctKjWdVg0m8qF0sAgbCsBiskkBD59mDxNL2ubzRE5sPtdbKQR6hj2izJXQf0nLfMSlNBgs3Cm93PkQqsg==;EndpointSuffix=core.windows.net -FUNCTIONS_EXTENSION_VERSION ~4 -EVENTHUB_NAME csg-stg-eh-mon-hub1-weu -WEBSITE_ENABLE_SYNC_UPDATE_SITE true -WEBSITE_RUN_FROM_PACKAGE 1 -Instrumentation_key 7d7c117f-e567-4238-96e7-0cf059b1177f -EVENTHUB_HOSTNAME csg-stg-eh-mon-weu.servicebus.windows.net -CLIENT_ID 2b256eb9-65d6-4836-9756-671e863348bf'
        $StorageAccNameValue = 'csgstgmonstorageweu'
        if($FuncAppType -eq 'ProvidenceFuncApp')
        {
            $FunctionAppNameValue = 'csg-stg-func-e2e-weu'
        }
        else
        {
            $FunctionAppNameValue = 'csg-stg-e2e-mon-func-grafana-weu'
        }
        SetVarToOutput -ResGroupName 'csg-weu-stg-e2e-mon' -FuncAppName $FunctionAppNameValue -StorageAccName $StorageAccNameValue -AppSettings $AppSettingsValue
    }
    elseif ($envName -eq 'PROD-EU') {            
        
        $AppSettingsValue = '-AzureWebJobsStorage DefaultEndpointsProtocol=https;EndpointSuffix=core.windows.net;AccountName=csgprode2emonstorageweu;AccountKey=Vxv9Z0Ug11Kpgza3yxWXAmQTm5u9SfDq6TCvcjo8/Ipzv7BvQE+vViFZ2yvsj0LIkmVn2wZlmRJy+AStAGzBDA==; -FUNCTIONS_EXTENSION_VERSION ~4 -EVENTHUB_NAME csg-prod01-eh-hub1-weu -WEBSITE_ENABLE_SYNC_UPDATE_SITE true -WEBSITE_RUN_FROM_PACKAGE 1 -Instrumentation_key 2b3974e9-930d-476a-b489-942231024879 -EVENTHUB_HOSTNAME csg-prod01-eh-eus2.servicebus.windows.net -CLIENT_ID 18454b67-f6f5-4f99-991c-89c11ca7bb77'
        $StorageAccNameValue = 'csgprode2emonstorageweu'
        if($FuncAppType -eq 'ProvidenceFuncApp')
        {
            $FunctionAppNameValue = 'csg-prod-e2e-mon-func-weu'
        }
        else
        {
            $FunctionAppNameValue = 'csg-prod-e2e-mon-func-grafana-weu'
        }
        SetVarToOutput -ResGroupName 'csg-weu-prod-e2e-mon' -FuncAppName $FunctionAppNameValue -StorageAccName $StorageAccNameValue -AppSettings $AppSettingsValue
    }
    elseif ($envName -eq 'PROD-US') {         
        
        $AppSettingsValue = '-AzureWebJobsStorage DefaultEndpointsProtocol=https;EndpointSuffix=core.windows.net;AccountName=csgprode2emonstorageeus2;AccountKey=VbwrmeyZh23qfODyWyt7PyfoPits8BSpZOmtfbORW9VPMOV1dVKPXETA9PZhFckfXodrKXsMhxYW+ASt5PpWfg==; -FUNCTIONS_EXTENSION_VERSION ~4 -EVENTHUB_NAME csg-prod01-eh-hub1-eus2 -WEBSITE_ENABLE_SYNC_UPDATE_SITE true -WEBSITE_RUN_FROM_PACKAGE 1 -Instrumentation_key 2b3974e9-930d-476a-b489-942231024879 -EVENTHUB_HOSTNAME csg-prod01-eh-eus2.servicebus.windows.net -CLIENT_ID c2405693-2ecb-4eec-b2a9-e1cd65e0a2e7'
        $StorageAccNameValue = 'csgprode2emonstorageeus2'
        if($FuncAppType -eq 'ProvidenceFuncApp')
        {
            $FunctionAppNameValue = 'csg-prod-e2e-mon-func-eus2'
        }
        else
        {
            $FunctionAppNameValue = 'csg-prod-e2e-mon-func-grafana-eus2'
        }
        SetVarToOutput -ResGroupName 'csg-eus2-prod-e2e-mon' -FuncAppName $FunctionAppNameValue -StorageAccName $StorageAccNameValue -AppSettings $AppSettingsValue
    }
    elseif ($envName -eq 'PROD-JP') {        
        
        $AppSettingsValue = '-AzureWebJobsStorage  DefaultEndpointsProtocol=https;EndpointSuffix=core.windows.net;AccountName=csgprode2emonstoragejpe;AccountKey=3ZPAJGwrDAoAe04h0Sbo9HgTOFuFlHw1jmXHwp9R6r1n3Y9hla5jSQjTX9KcME7yj7EJKAmrXJHM+AStygwk/Q==; -FUNCTIONS_EXTENSION_VERSION ~4 -EVENTHUB_NAME csg-prod01-eh-hub1-jpe -WEBSITE_ENABLE_SYNC_UPDATE_SITE true -WEBSITE_RUN_FROM_PACKAGE 1 -Instrumentation_key 9db44488-7f81-426f-bc68-a829613f8c1c -EVENTHUB_HOSTNAME csg-prod01-eh-jpe.servicebus.windows.net -CLIENT_ID d7aa0e3a-bf0d-431b-8221-890b8229abb7'  
        $StorageAccNameValue = 'csgprode2emonstoragejpe'
        if($FuncAppType -eq 'ProvidenceFuncApp')
        {
            $FunctionAppNameValue = 'csg-prod-e2e-mon-func-jpe'
        }
        else
        {
            $FunctionAppNameValue = 'csg-prod-e2e-mon-func-grafana-jpe'
        }
        SetVarToOutput -ResGroupName 'csg-jpe-prod-e2e-mon' -FuncAppName $FunctionAppNameValue -StorageAccName $StorageAccNameValue -AppSettings $AppSettingsValue
    }
    elseif ($envName -eq 'STG-CN') {    
        
        $AppSettingsValue = '-AzureWebJobsStorage DefaultEndpointsProtocol=https;AccountName=csgstge2emonstoragecne2;AccountKey=9onAKsrfeE3LK7//aOy8rzEPcMOgNmfdMM/vxbaEcr+dj6vNhA4bklHKSuiKJyEdHyO9imWN4np1+ASt6fQ98g==;EndpointSuffix=core.chinacloudapi.cn -FUNCTIONS_EXTENSION_VERSION ~4 -EVENTHUB_NAME csg-stg01-eh-hub1-cne2 -WEBSITE_ENABLE_SYNC_UPDATE_SITE true -WEBSITE_RUN_FROM_PACKAGE 1 -Instrumentation_key 37cbc263-7791-c2ce-a315-648c69ddf0b1 -EVENTHUB_HOSTNAME csg-stg01-eh-cne2.servicebus.chinacloudapi.cn -CLIENT_ID f8ea446f-851e-4b8a-b4bb-9dabe91a6cf8 -APPLICATIONINSIGHTS_CONNECTION_STRING InstrumentationKey=37cbc263-7791-c2ce-a315-648c69ddf0b1;EndpointSuffix=applicationinsights.azure.cn;IngestionEndpoint=https://chinaeast2-0.in.applicationinsights.azure.cn/;AADAudience=https://monitor.azure.cn/'
        $StorageAccNameValue = 'csgstge2emonstoragecne2'
        if($FuncAppType -eq 'ProvidenceFuncApp')
        {
            $FunctionAppNameValue = 'csg-stg-e2e-mon-func-cne2'
        }
        else
        {
            $FunctionAppNameValue = 'csg-stg-e2e-mon-func-grafana-cne2'
        }
        SetVarToOutput -ResGroupName 'csg-cne2-stg-e2e-mon' -FuncAppName $FunctionAppNameValue -StorageAccName $StorageAccNameValue -AppSettings $AppSettingsValue
    }        
    elseif ($envName -eq 'PROD-CN') {    
        
        $AppSettingsValue = '-FUNCTIONS_EXTENSION_VERSION ~4  -WEBSITE_RUN_FROM_PACKAGE 1'
        $StorageAccNameValue = ''
        if($FuncAppType -eq 'ProvidenceFuncApp')
        {
            $FunctionAppNameValue = 'csg-prod-e2e-mon-func-cne2'
        }
        else
        {
            $FunctionAppNameValue = 'csg-prod-e2e-mon-func-grafana-cne2'
        }
        SetVarToOutput -ResGroupName 'csg-cne2-prod-e2e-mon' -FuncAppName $FunctionAppNameValue -StorageAccName $StorageAccNameValue -AppSettings $AppSettingsValue
    }
}
catch
{
    Write-Host "Exception occured.."
    exit 1;
} 