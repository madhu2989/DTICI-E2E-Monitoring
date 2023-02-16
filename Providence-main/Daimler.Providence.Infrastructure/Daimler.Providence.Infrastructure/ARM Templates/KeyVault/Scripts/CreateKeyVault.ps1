param(
    [parameter(Mandatory = $true)]
    [ValidateNotNullOrEmpty()]
    [String]$VaultName,

    [parameter(Mandatory = $true)]
    [ValidateNotNullOrEmpty()]
    [String]$ResourceGroupName,   

    [parameter(Mandatory = $true)]
    [ValidateNotNullOrEmpty()]
    [String]$Location,

    [Hashtable] $OverwriteTemplateParameters,

    [String]$ServicePrincipalObjectId
)$OverwriteTemplateParameters
try {
    $existingKeyVault = Get-AzKeyVault -VaultName $VaultName -ResourceGroupName $ResourceGroupName
    if ($existingKeyVault -eq $null) {
    
		#Write-Host "Creating/updating resource group in $($ResourceGroupName) in region $($Location)."			
        #New-AzResourceGroup -Name $ResourceGroupName -Location $Location -Force | Out-Null


    if(!$OverwriteTemplateParameters) {
	    $OverwriteTemplateParameters = New-Object -TypeName Hashtable
    }

    Write-Host "Appending values to : $OverwriteTemplateParameters"
    #set the relevant template parameters to overwrite 
    $OverwriteTemplateParameters.Add("Availability", "A2")
    $OverwriteTemplateParameters.Add("Confidentiality", "C2.2")
    $OverwriteTemplateParameters.Add("Export", "E2")
    $OverwriteTemplateParameters.Add("Integrity", "I2")
    $OverwriteTemplateParameters.Add("Recovery", "R2")
    $OverwriteTemplateParameters.Add("Project", "E2Emon")


        Write-Host "Creating new key vault '$($VaultName)' in '$($ResourceGroupName)'."
        $newKeyVault = New-AzKeyVault `
            -VaultName $VaultName `
            -ResourceGroupName $ResourceGroupName `
            -Location $Location `
            -EnabledForDeployment `
            -EnabledForTemplateDeployment `
            -EnablePurgeProtection `
            -Sku 'Standard' `
            -Tag $OverwriteTemplateParameters `
            -SoftDeleteRetentionInDays 90

        # Access for ServicePrincipal to copy/update/create secret within the key vault
        #Set-AzKeyVaultAccessPolicy -ResourceGroupName $ResourceGroupName -VaultName $VaultName -ObjectId $ServicePrincipalObjectId -PermissionsToSecrets set,get,list -PermissionsToKeys get,wrapKey,unwrapKey -BypassObjectIdValidation     

        if ($newKeyVault) {
           Write-Host "Key Vault creation succeeded" $VaultName
        }
    }
    else {
        Write-Host "Key vault '$($VaultName)' already exists in '$($ResourceGroupName)'."  
    }
}
catch {
    Write-Error "Key Vault creation failed unexpected: $($_.Exception.Message)"
}