param([string] $version)
$ErrorActionPreference = "Stop"

# Set location to the Solution directory
(Get-Item $PSScriptRoot).Parent.FullName | Push-Location
[xml] $versionFile = Get-Content "./src/lunchin.Optimizely.Cloud.Extensions/lunchin.Optimizely.Cloud.Extensions.csproj"

$azureNode = $versionFile.SelectSingleNode("Project/ItemGroup/PackageReference[@Include='EPiServer.Azure']")
$azureVersion = $azureNode.Attributes["Version"].Value
$azureParts = $azureVersion.Split(".")
$azureMajor = [int]::Parse($azureParts[0]) + 1
$azureNextMajorVersion = ($azureMajor.ToString() + ".0.0") 

$uiNode = $versionFile.SelectSingleNode("Project/ItemGroup/PackageReference[@Include='EPiServer.CMS.UI.Core']")
$uiVersion = $uiNode.Attributes["Version"].Value
$uiParts = $uiVersion.Split(".")
$uiMajor = [int]::Parse($uiParts[0]) + 1
$uiNextMajorVersion = ($uiMajor.ToString() + ".0.0") 

$agilityNode = $versionFile.SelectSingleNode("Project/ItemGroup/PackageReference[@Include='HtmlAgilityPack']")
$agilityVersion = $agilityNode.Attributes["Version"].Value
$agilityParts = $agilityVersion.Split(".")
$agilityMajor = [int]::Parse($agilityParts[0]) + 1
$agilityNextMajorVersion = ($agilityMajor.ToString() + ".0.0") 

$helpersNode = $versionFile.SelectSingleNode("Project/ItemGroup/PackageReference[@Include='EPiServer.CMS.AspNetCore.HtmlHelpers']")
$helpersVersion = $helpersNode.Attributes["Version"].Value
$helpersParts = $helpersVersion.Split(".")
$helpersMajor = [int]::Parse($helpersParts[0]) + 1
$helpersNextMajorVersion = ($helpersMajor.ToString() + ".0.0") 

[xml] $commerceVersionFile = Get-Content "./src/lunchin.Optimizely.Cloud.Extensions.Commerce/lunchin.Optimizely.Cloud.Extensions.Commerce.csproj"
$commerceNode = $commerceVersionFile.SelectSingleNode("Project/ItemGroup/PackageReference[@Include='EPiServer.Commerce.Core']")
$commerceVersion = $commerceNode.Attributes["Version"].Value
$commerceParts = $commerceVersion.Split(".")
$commerceMajor = [int]::Parse($commerceParts[0]) + 1
$commerceNextMajorVersion = ($commerceMajor.ToString() + ".0.0") 

$couponNode = $commerceVersionFile.SelectSingleNode("Project/ItemGroup/PackageReference[@Include='Powell.CouponCode']")
$couponVersion = $couponNode.Attributes["Version"].Value
$couponParts = $couponVersion.Split(".")
$couponMajor = [int]::Parse($couponParts[0]) + 1
$couponNextMajorVersion = ($couponMajor.ToString() + ".0.0") 

Remove-Item -Path ./zipoutput -Recurse -Force -Confirm:$false -ErrorAction Ignore

Copy-Item "./src/lunchin.Optimizely.Cloud.Extensions/clientResources" -Destination "./zipoutput/lunchin.Optimizely.Cloud.Extensions/clientResources" -Recurse

[xml] $moduleFile = Get-Content "./src/lunchin.Optimizely.Cloud.Extensions/module.config"
$module = $moduleFile.SelectSingleNode("module")
$module.Attributes["clientResourceRelativePath"].Value = $version
$moduleFile.Save("./zipoutput/lunchin.Optimizely.Cloud.Extensions/module.config")

New-Item -Path "./zipoutput/lunchin.Optimizely.Cloud.Extensions" -Name "$version" -ItemType "directory"
Move-Item -Path "./zipoutput/lunchin.Optimizely.Cloud.Extensions/clientResources" -Destination "./zipoutput/lunchin.Optimizely.Cloud.Extensions/$version/clientResources"

$compress = @{
  Path = "./zipoutput/lunchin.Optimizely.Cloud.Extensions/*"
  CompressionLevel = "Optimal"
  DestinationPath = "./zipoutput/lunchin.Optimizely.Cloud.Extensions.zip"
}

Compress-Archive @compress

Copy-Item "./src/lunchin.Optimizely.Cloud.Extensions.Commerce/clientResources" -Destination "./zipoutput/lunchin.Optimizely.Cloud.Extensions.Commerce/clientResources" -Recurse

[xml] $moduleFile = Get-Content "./src/lunchin.Optimizely.Cloud.Extensions.Commerce/module.config"
$module = $moduleFile.SelectSingleNode("module")
$module.Attributes["clientResourceRelativePath"].Value = $version
$moduleFile.Save("./zipoutput/lunchin.Optimizely.Cloud.Extensions.Commerce/module.config")

New-Item -Path "./zipoutput/lunchin.Optimizely.Cloud.Extensions.Commerce" -Name "$version" -ItemType "directory"
Move-Item -Path "./zipoutput/lunchin.Optimizely.Cloud.Extensions.Commerce/clientResources" -Destination "./zipoutput/lunchin.Optimizely.Cloud.Extensions.Commerce/$version/clientResources"

$compress = @{
  Path = "./zipoutput/lunchin.Optimizely.Cloud.Extensions.Commerce/*"
  CompressionLevel = "Optimal"
  DestinationPath = "./zipoutput/lunchin.Optimizely.Cloud.Extensions.Commerce.zip"
}

Compress-Archive @compress

dotnet pack --no-restore --no-build -c Release /p:PackageVersion=$version /p:AzureVersion=$azureVersion /p:AzureNextMajorVersion=$azureNextMajorVersion /p:UiVersion=$uiVersion /p:UiNextMajorVersion=$uiNextMajorVersion /p:AgilityVersion=$agilityVersion /p:AgilityNextMajorVersion=$agilityNextMajorVersion /p:CommerceVersion=$commerceVersion /p:CommerceNextMajorVersion=$commerceNextMajorVersion /p:HelpersVersion=$helpersVersion /p:HelpersNextMajorVersion=$helpersNextMajorVersion /p:CouponVersion=$couponVersion /p:CouponNextMajorVersion=$couponNextMajorVersion lunchin.Optimizely.Cloud.Extensions.sln

Pop-Location