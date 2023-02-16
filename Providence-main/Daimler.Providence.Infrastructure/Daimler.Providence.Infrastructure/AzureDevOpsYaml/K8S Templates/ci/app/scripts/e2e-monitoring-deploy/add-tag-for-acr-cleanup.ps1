Param(
  [Parameter(Mandatory=$true)]
  [string] $project,

  [Parameter(Mandatory=$true)]
  [string] $stage,

  [Parameter(Mandatory=$true)]
  [string] $locationShort,

  [Parameter(Mandatory=$true)]
  [string] $versionNumber
)


$cs = (az storage account show-connection-string -g ITC_Global_CoBa_Shared_westeurope -n itcglobalsharedweu | convertfrom-json).connectionString

az storage entity insert -t ContainerRegistry --connection-string $cs --if-exists replace --entity PartitionKey=$project RowKey=$stage-$locationShort Stage=$stage Location=$locationshort Tag=$versionNumber