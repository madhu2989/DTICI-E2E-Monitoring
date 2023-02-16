[CmdletBinding()]
Param(
    # Define ACR Name
    [Parameter (Mandatory=$true)]
    [ValidateNotNullOrEmpty()]
    [String] $AzureRegistryName
)

Function deleteUntaggedImages {

    Param([string]$AzureRegistryName, [string]$RepositoryName, [string]$enableDelete) 
        
    $RepositoryManifests = @(az acr repository show-manifests --name $AzureRegistryName --repository $RepositoryName --query "[?tags[0]==null].[digest]" --output tsv --orderby time_desc)
    write-host "# Total untagged images for "$RepositoryName"# found :" $RepositoryManifests.length
    $imagesDeleted = 0
    for($index=0; $index -lt $RepositoryManifests.length; $index++){
        $repoDigest = $RepositoryManifests[$index]
        write-host "Untagged Manifest :  "$RepositoryName"@"$repoDigest
    
        if($enableDelete -eq "yes"){
                        
                az acr repository delete --name $AzureRegistryName --image $RepositoryName@$repoDigest --yes
                write-host "# Deleted :  "$RepositoryName@$repoDigest
                $imagesDeleted++    
        }
    
    }
    write-host "# Total untagged deleted images for "$RepositoryName"#  :" $imagesDeleted
}

write-host "# Clean Up activity for Azure Registry  "$AzureRegistryName"" 

$RepositoryList = @(az acr repository list -n $AzureRegistryName --output tsv)

for($index=0; $index -lt $RepositoryList.length; $index++){
    $repo = $RepositoryList[$index]
    Write-Host "$repo"
    deleteUntaggedImages $AzureRegistryName $RepositoryList[$index] "yes"

}