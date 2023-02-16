param (
    [parameter(Mandatory = $true)]
    [ValidateNotNullOrEmpty()]
    [String] $VaultName
)
try{ 

  function Get-RandomPassword {
      Param
      (
          [Parameter(Mandatory = $true)]
          [ValidateRange(1, 2048)]
          [int]$PasswordLength      
      )
      $FirstChar = 'abcdefghijkmnpqrstuvwxyzABCEFGHJKLMNPQRSTUVWXYZ'
      $InputStrings = @('abcdefghijkmnpqrstuvwxyz', 'ABCEFGHJKLMNPQRSTUVWXYZ', '1234567890', '!$%()')
    
      # Create char arrays containing groups of possible chars
      [char[][]]$CharGroups = $InputStrings
  	
      # Create char array containing all chars
      $AllChars = $CharGroups | ForEach-Object {[Char[]]$_}
  
      $Password = @{}
      $Password.Add(0, $FirstChar[((Get-Seed) % $FirstChar.Length)])
      # Randomize one char from each group
      Foreach ($Group in $CharGroups) {
          if ($Password.Count -lt $PasswordLength) {
              $Index = Get-Seed
              While ($Password.ContainsKey($Index)) {
                  $Index = Get-Seed            
              }
              $Password.Add($Index, $Group[((Get-Seed) % $Group.Count)])
          }
      }
      # Fill out with chars from $AllChars
      for ($i = $Password.Count; $i -lt $PasswordLength; $i++) {
          $Index = Get-Seed
          While ($Password.ContainsKey($Index)) {
              $Index = Get-Seed          
          }
          $Password.Add($Index, $AllChars[((Get-Seed) % $AllChars.Count)])
      }
      Write-Output -InputObject $( -join ($Password.GetEnumerator() | Sort-Object -Property Name | Select-Object -ExpandProperty Value))
  }
  
  function Get-Seed {
      # Generate a seed for randomization
      $RandomBytes = New-Object -TypeName 'System.Byte[]' 4
      $Random = New-Object -TypeName 'System.Security.Cryptography.RNGCryptoServiceProvider'
      $Random.GetBytes($RandomBytes)
      [BitConverter]::ToUInt32($RandomBytes, 0)
  }
  
  # Check if SqlServer-Admin-Username already exists within the KeyVault and add it if not
  $sqlAdminNameSecret = Get-AzKeyVaultSecret -VaultName $VaultName -Name "SqlServer-Admin-Username"
  if(!$sqlAdminNameSecret)
  {
    $sqlAdminNameSecret = ConvertTo-SecureString -String "monitoringAdmin" -AsPlainText -Force   
    Set-AzKeyVaultSecret -VaultName $VaultName -Name "SqlServer-Admin-Username" -SecretValue $sqlAdminNameSecret
  }
  
  # Check if SqlServer-Admin-Password already exists within the KeyVault and add it if not
  $sqlAdminPasswordSecret = Get-AzKeyVaultSecret -VaultName $VaultName -Name "SqlServer-Admin-Password"
  if(!$sqlAdminPasswordSecret)
  {
    # Generate random password
    #$randomSecret = Get-RandomPassword -PasswordLength 40
    $sqlAdminPasswordSecret = ConvertTo-SecureString -String "e2emonitoring@123" -AsPlainText -Force   
    Set-AzKeyVaultSecret -VaultName $VaultName -Name "SqlServer-Admin-Password" -SecretValue $sqlAdminPasswordSecret
  }
  
  # Check if SqlDb-Service-Username already exists within the KeyVault and add it if not
  $sqlDbServiceNameSecret = Get-AzKeyVaultSecret -VaultName $VaultName -Name "SqlDb-Service-Username"
  if(!$sqlDbServiceNameSecret)
  {
    $sqlDbServiceNameSecret = ConvertTo-SecureString -String "monitoringService" -AsPlainText -Force   
    Set-AzKeyVaultSecret -VaultName $VaultName -Name "SqlDb-Service-Username" -SecretValue $sqlDbServiceNameSecret
  }
  
  # Check if SqlDb-Service-Password already exists within the KeyVault and add it if not
  $sqlDbServicePasswordSecret = Get-AzKeyVaultSecret -VaultName $VaultName -Name "SqlDb-Service-Password"
  if(!$sqlDbServicePasswordSecret)
  {
    # Generate random password
    $randomSecret = Get-RandomPassword -PasswordLength 40
    $sqlDbServicePasswordSecret = ConvertTo-SecureString -String $randomSecret -AsPlainText -Force   
     Set-AzKeyVaultSecret -VaultName $VaultName -Name "SqlDb-Service-Password" -SecretValue $sqlDbServicePasswordSecret
  }
}
catch{
    Write-Host "Exception Occured while setting secret"
    exit 1;
}