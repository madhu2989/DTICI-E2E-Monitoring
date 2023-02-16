#
# General_Azure.ps1
#

function Check-Azure-Session () {
	Write-Host "Checking the Azure Session"
	$azureError = $null
	try{
		Write-Host "Trying to get Azure Context"
		$azureContext = Get-AzureRmContext -ErrorVariable $azureError
		Write-Error $azureError
		Write-Host "Subscription id : "
		Write-Host $azureContext.Subscription.Id
		if($azureError -or ($azureContext.Account.Id.Length -eq 0) -or ($azureContext.Subscription.Id.Length -eq 0) ) {
			# only works locally, not in the deployment pipeline
			#Login-AzureRmAccount
			#silencing the error for the build pipeline
			Write-Host "You are not logged in. Run Login-AzureRmAccount"
		}
	}
	catch{
		Write-Error $azureError
	}
	

}