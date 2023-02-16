param (
    [Parameter(Mandatory=$true)]
	[String] $SecretName,

	[Parameter(Mandatory=$true)]
	[String] $SecretValue,

	[Parameter(Mandatory=$true)]
	[String] $VaultName,

    [Parameter(Mandatory=$true)]
    [String] $ResourceGroupName
)

try
{
    $existingKeyVault = Get-AzKeyVault -VaultName $VaultName -ResourceGroupName $ResourceGroupName
    Write-Host "1: -"$existingKeyVault
    if(!$existingKeyVault) 
    {
        throw "Error getting the key vault"
    } 

    #Write-Host "Setting access for objectId in keyvault to deploy secrets inside it"
    #Set-AzKeyVaultAccessPolicy -ResourceGroupName $ResourceGroupName -VaultName $VaultName -ObjectId $ObjectId -PermissionsToSecrets set,get,list -PermissionsToKeys get,wrapKey,unwrapKey -BypassObjectIdValidation 
    
    #Check if secret already exists
    $secretValueStr = ConvertTo-SecureString -String $SecretValue -AsPlainText -Force
    $targetVaultSecret = Get-AzKeyVaultSecret -VaultName $VaultName -Name $SecretName
    
    if(!$targetVaultSecret)
    {	  
    	Write-Host "Adding secret with name: $SecretName to target KeyVault."
    	Set-AzKeyVaultSecret -VaultName $VaultName -Name $SecretName -SecretValue $secretValueStr
    }
    else
    {
    	Write-Host "Secret with name: $SecretName already exists in key vault with same value."
    }	
}
catch
{
	Write-Error "Key vault $VaultName does not exist in resource group $ResourceGroupName. $Errors"
}