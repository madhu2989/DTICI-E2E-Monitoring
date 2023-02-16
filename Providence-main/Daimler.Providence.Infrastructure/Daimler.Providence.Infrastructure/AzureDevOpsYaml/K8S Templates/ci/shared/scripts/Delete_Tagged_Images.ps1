[CmdletBinding()]
Param(
    # Define ACR Name
    [Parameter (Mandatory=$true)]
    [ValidateNotNullOrEmpty()]
    [String] $AzureRegistryName,

    # Define Azure Subscription Name
    #[Parameter (Mandatory=$false)]
    #[ValidateNotNullOrEmpty()]
    #[String] $SubscriptionName,
  
    # Enable deletion or just run in scan mode
    [Parameter (Mandatory=$false)]
    [ValidateNotNullOrEmpty()]
    [String] $enableDelete = "no",

    # Specify repository to cleanup (if not specified will default to all repositories within the registry)
    [Parameter (Mandatory=$true)]
    [ValidateNotNullOrEmpty()]
    [String] $RepositoryName,
	
	[Parameter (Mandatory=$true)]
    [ValidateNotNullOrEmpty()]
    [String] $tagName
)



#if ($SubscriptionName){
#    Write-Host "Setting subscription to: $SubscriptionName"
#    az account set --subscription $SubscriptionName
#}


$RepoTag="$RepositoryName"+":"+"$tagName"
write-host "Repo tag : " $RepoTag

$tagList=az acr repository show-tags -n $AzureRegistryName --repository $RepositoryName --output tsv


if($tagList -is [system.array] -And $tagList -contains $tagName) {

	write-host "Deleting the tag : " $RepoTag
	az acr repository delete --name $AzureRegistryName --image $RepoTag --$enableDelete
	write-host ""
	Write-Host "ACR $RepoTag cleanup completed"
} elseif ($tagList -eq $tagName) {
	write-host "Deleting the tag : " $RepoTag
	az acr repository delete --name $AzureRegistryName --image $RepoTag --$enableDelete
	write-host ""
	Write-Host "ACR $RepoTag cleanup completed"
} else {

	Write-Host "ACR $RepoTag not found"
}

	




