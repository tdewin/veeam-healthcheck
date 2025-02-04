﻿#Requires -Version 4
#Requires -RunAsAdministrator
<#
.Synopsis
    Simple Veeam report to dump server & job configurations
.Notes
    Version: 0.2
    Author: Joe Houghes
    Modified Date: 4-24-2020
.EXAMPLE
    Get-VBRConfig -VBRServer ausveeambr -ReportPath C:\Temp\VBROutput
    Get-VBRconfig -VBRSERVER localhost -ReportPath C:\HealthCheck\output
#>
Write-host("Executing VBR Config Collection...")
function Get-VBRConfig {
  param(
    # VBRServer
    [Parameter(Mandatory)]
    [string]$VBRServer,
    [Parameter(Mandatory)]
    [string]$ReportPath
  )

  begin {

    #Load the Veeam PSSnapin
    if (!(Get-PSSnapin -Name VeeamPSSnapIn -ErrorAction SilentlyContinue)) {
      Add-PSSnapin -Name VeeamPSSnapIn
      Connect-VBRServer -Server $VBRServer
    }

    else {
      Disconnect-VBRServer
      Connect-VBRServer -Server $VBRServer
    }

    if (!(Test-Path $ReportPath)) {
      New-Item -Path $ReportPath -ItemType Directory | Out-Null
    }

    Push-Location -Path $ReportPath
    Write-Verbose ("Changing directory to '$ReportPath'")

  }

  process {

    $Servers = Get-VBRServer
    $Jobs = Get-VBRJob -WarningAction SilentlyContinue #| Where-Object { $_.JobType -eq 'Backup' -OR $_.JobType -eq 'BackupSync' }
    # Agent Jobs
    $AgentJobs = Get-VBRComputerBackupJob -WarningAction SilentlyContinue
    # Replica Jobs
    # CDP Job
    # SureBackup?
    $Proxies = Get-VBRViProxy
    $cdpProxy = Get-VBRCDPProxy                
    $fileProxy = Get-VBRComputerFileProxyServer 
    $hvProxy = Get-VBRHvProxy                 
    $nasProxy =Get-VBRNASProxyServer          
    
    #Agents
    $pg = Get-VBRProtectionGroup
    $pc = Get-VBRDiscoveredComputer

#Capacity extent grab:
$cap = get-vbrbackuprepository -ScaleOut | Get-VBRCapacityExtent
$capOut = $cap | Select-Object Status, @{n='Type';e={$_.Repository.Type}}, @{n='Immute';e={$_.Repository.BackupImmutabilityEnabled}}, @{n='immutabilityperiod';e={$_.Repository.ImmutabilityPeriod}},@{n='SizeLimitEnabled';e={$_.Repository.SizeLimitEnabled}}, @{n='SizeLimit';e={$_.Repository.SizeLimit}}, @{n='RepoId';e={$_.Repository.Id}}, parentid

#Traffic Rules
$trafficRules = Get-VBRNetworkTrafficRule  

#regSettings
        $reg = get-item "HKLM:\SOFTWARE\Veeam\Veeam Backup and Replication"

        [System.Collections.ArrayList]$output = @()
        foreach($r in $reg.Property){

        $regout2 = [PSCustomObject][ordered] @{
            'KeyName' = $r
            'Value' = $reg.GetValue($r)

        }
        $null = $output.Add($regout2)
           # $regout2 += $reg | Select-Object @{n="KeyName";e={$r}}, @{n='value';e={$_.GetValue($r)}}
        }



#JobTypes & conversion
    $catCopy = Get-VBRCatalystCopyJob
    $vaBcj = Get-VBRComputerBackupCopyJob
    $vaBJob = Get-VBRComputerBackupJob
    $configBackup = Get-VBRConfigurationBackupJob
    $epJob =   Get-VBREPJob 
    $sbJob = Get-VSBJob
    $tapeJob = Get-VBRTapeJob
    $nasBackup = Get-VBRNASBackupJob 
    $nasBCJ = Get-VBRNASBackupCopyJob 
    $cdpJob = Get-VBRCDPPolicy
    
    $piJob = Get-VBRPluginJob
    $piJob | Add-Member -MemberType NoteProperty -Name JobType -Value "Plugin Backup"
    #$Jobs += $piJob

    $vcdJob = Get-VBRvCDReplicaJob
    $vcdJob | Add-Member -MemberType NoteProperty -Name JobType -Value "VCD Replica"
    $Jobs += $vcdJob

    #$Jobs += $nasBCJ

    $cdpJob | Add-Member -MemberType NoteProperty -Name JobType -Value "CDP Policy"
    $Jobs += $cdpJob

    $nasBackup | Add-Member -MemberType NoteProperty -Name JobType -Value "NAS Backup"
   # $Jobs += $nasBackup

    $tapeJob | Add-Member -MemberType NoteProperty -Name JobType -Value "Tape Backup"
    $Jobs += $tapeJob

    $catCopy | Add-Member -MemberType NoteProperty -Name JobType -Value "Catalyst Copy"# -InformationVariable "catCopy"
    $Jobs += $catCopy

    
    $vaBcj | Add-Member -MemberType NoteProperty -Name JobType -Value "Physical Backup Copy"
    #$Jobs += $vaBcj //pulled in jobs for now

    try{
    $vaBJob += $epJob }
    catch{write-host("test")}
    
    try{
    $vaBJob | Add-Member -MemberType NoteProperty -Name JobType -Value "Physical Backup" -ErrorAction Ignore}
    catch{write-host("test")}
    #$Jobs += $vaBJob //pulled in jobs for now

    
    #$configBackup #| Add-Member -MemberType NoteProperty -Name JobType -Value "Config Backup"
    #$Jobs += $configBackup

    $Jobs += $sbJob

    
    $Repositories = Get-VBRBackupRepository
    $SOBRs = Get-VBRBackupRepository -ScaleOut
    $wan = Get-VBRWANAccelerator

    [System.Collections.ArrayList]$RepositoryDetails = @()

    foreach ($Repo in $Repositories) {
      $RepoOutput = [pscustomobject][ordered] @{
        'ID'   = $Repo.ID
        'Name' = $Repo.Name
      }
      $null = $RepositoryDetails.Add($RepoOutput)
      Remove-Variable RepoOutput
    }

    foreach ($Repo in $SOBRs) {
      $RepoOutput = [pscustomobject][ordered] @{
        'ID'   = $Repo.ID
        'Name' = $Repo.Name
      }
      $null = $RepositoryDetails.Add($RepoOutput)
      Remove-Variable RepoOutput
    }

    [System.Collections.ArrayList]$AllJobs = @()

    foreach ($Job in $Jobs) {
      $JobDetails = $Job | Select-Object -Property 'Name', 'JobType', 'SheduleEnabledTime', 'ScheduleOptions', @{n = 'RestorePoints'; e = { $Job.Options.BackupStorageOptions.RetainCycles } }, @{n = 'RepoName'; e = { $RepositoryDetails | Where-Object { $_.Id -eq $job.Info.TargetRepositoryId.Guid } | Select-Object -ExpandProperty Name } }, @{n = 'Algorithm'; e = { $Job.Options.BackupTargetOptions.Algorithm } }, @{n = 'FullBackupScheduleKind'; e = { $Job.Options.BackupTargetOptions.FullBackupScheduleKind } }, @{n = 'FullBackupDays'; e = { $Job.Options.BackupTargetOptions.FullBackupDays } }, @{n = 'TransformFullToSyntethic'; e = { $Job.Options.BackupTargetOptions.TransformFullToSyntethic } }, @{n = 'TransformIncrementsToSyntethic'; e = { $Job.Options.BackupTargetOptions.TransformIncrementsToSyntethic } }, @{n = 'TransformToSyntethicDays'; e = { $Job.Options.BackupTargetOptions.TransformToSyntethicDays } }, @{n='PwdKeyId';e={$_.Info.PwdKeyId}}, @{n='OriginalSize';e={$_.Info.IncludedSize}}
      $AllJobs.Add($JobDetails) | Out-Null
    }
    #add other job types...
    #TODO
#    foreach ($AgentJob in $AgentJobs) {
#        $AJobDetails = $AgentJob | Select-Object -Property 'Name',@{n = 'JobType'; e = {$_.Type}}, 'OSPlatform', 'Mode', 'ScheduleEnabled', 'ScheduleOptions', @{n = 'RestorePoints'; e = { $AgentJob.RetentionPolicy } }, @{n = 'RepoName'; e = {$AgentJob.BackupRepository.Name}}
#        $AllJobs.Add($AJobDetails) | Out-Null
#        }
    [System.Collections.ArrayList]$AllSOBRExtents = @()

    foreach ($SOBR in $SOBRs) {
      $Extents = Get-VBRRepositoryExtent -Repository $SOBR

      foreach ($Extent in $Extents) {
        $ExtentDetails = $Extent.Repository | Select-Object *, @{n = 'SOBR_Name'; e = { $SOBR.Name } },@{name='CachedFreeSpace';expression={$_.GetContainer().cachedfreespace.InGigabytes}},@{name='CachedTotalSpace';expression={$_.GetContainer().cachedtotalspace.InGigabytes}}
        $AllSOBRExtents.Add($ExtentDetails) | Out-Null
      }
    }

    $lic = Get-VBRInstalledLicense

$licInfo = $lic |Select-Object "LicensedTo", "Edition","ExpirationDate","Type","SupportId","SupportExpirationDate","AutoUpdateEnabled","FreeAgentInstanceConsumptionEnabled","CloudConnect",
        @{n="LicensedSockets"; e={$_.SocketLicenseSummary.LicensedSocketsNumber}},
        @{n="UsedSockets";e={$_.SocketLicenseSummary.UsedSocketsNumber}},
        @{n="RemainingSockets";e={$_.SocketLicenseSummary.RemainingSocketsNumber}},
        @{n="LicensedInstances";e={$_.InstanceLicenseSummary.LicensedInstancesNumber}},
        @{n="UsedInstances";e={$_.InstanceLicenseSummary.UsedInstancesNumber}},
        @{n="NewInstances";e={$_.InstanceLicenseSummary.NewInstancesNumber}},
        @{n="RentalInstances";e={$_.InstanceLicenseSummary.RentalInstancesNumber}},
        @{n="LicensedCapacityTB";e={$_.CapacityLicenseSummary.LicensedCapacityTb}},
        @{n="UsedCapacityTb";e={$_.CapacityLicenseSummary.UsedCapacityTb}}, "Status"

$repoInfo = $Repositories | Select-Object "Id", "Name","HostId","Description","CreationTime","Path",
        "FullPath","FriendlyPath","ShareCredsId","Type","Status","IsUnavailable","Group","UseNfsOnMountHost",
        "VersionOfCreation","Tag","IsTemporary","TypeDisplay","IsRotatedDriveRepository","EndPointCryptoKeyId",
        "Options","HasBackupChainLengthLimitation", "IsSanSnapshotOnly","IsDedupStorage","SplitStoragesPerVm","IsImmutabilitySupported",
        @{name='Options(maxtasks)';expression={$_.Options.MaxTaskCount}},
        @{name='Options(Unlimited Tasks)';expression={$_.Options.IsTaskCountUnlim}},
        @{name='Options(MaxArchiveTaskCount)';expression={$_.Options.MaxArchiveTaskCount}},
        @{name='Options(CombinedDataRateLimit)';expression={$_.Options.CombinedDataRateLimit}},
        @{name='Options(Uncompress)';expression={$_.Options.Uncompress}},
        @{name='Options(OptimizeBlockAlign)';expression={$_.Options.OptimizeBlockAlign}},
        @{name='Options(RemoteAccessLimitation)';expression={$_.Options.RemoteAccessLimitation}},
        @{name='Options(EpEncryptionEnabled)';expression={$_.Options.EpEncryptionEnabled}},
        @{name='Options(OneBackupFilePerVm)';expression={$_.Options.OneBackupFilePerVm}},
        @{name='Options(IsAutoDetectAffinityProxies)';expression={$_.Options.IsAutoDetectAffinityProxies}},
        @{name='Options(NfsRepositoryEncoding)';expression={$_.Options.NfsRepositoryEncoding}}, 
        @{n='CachedTotalSpace';e={$_.getcontainer().CachedTotalSpace.ingigabytes}}, 
        @{n='CachedFreeSpace';e={$_.getcontainer().CachedFreeSpace.ingigabytes}}

    $SOBROutput = $SOBRs | Select-Object -Property "PolicyType",@{n="Extents";e={$SOBRs.extent.name -as [String]}} ,"UsePerVMBackupFiles","PerformFullWhenExtentOffline","EnableCapacityTier","OperationalRestorePeriod","OverridePolicyEnabled","OverrideSpaceThreshold","OffloadWindowOptions","CapacityExtent","EncryptionEnabled","EncryptionKey","CapacityTierCopyPolicyEnabled","CapacityTierMovePolicyEnabled","ArchiveTierEnabled","ArchiveExtent","ArchivePeriod","CostOptimizedArchiveEnabled","ArchiveFullBackupModeEnabled","PluginBackupsOffloadEnabled","CopyAllPluginBackupsEnabled","CopyAllMachineBackupsEnabled","Id","Name","Description"
    $AllSOBRExtentsOutput = $AllSOBRExtents | Select-Object -property @{name='Host';expression={$_.host.name}} ,"Id","Name","HostId","MountHostId","Description","CreationTime","Path","FullPath","FriendlyPath","ShareCredsId","Type","Status","IsUnavailable","Group","UseNfsOnMountHost","VersionOfCreation","Tag","IsTemporary","TypeDisplay","IsRotatedDriveRepository","EndPointCryptoKeyId","HasBackupChainLengthLimitation","IsSanSnapshotOnly","IsDedupStorage","SplitStoragesPerVm","IsImmutabilitySupported","SOBR_Name", @{name='Options(maxtasks)';expression={$_.Options.MaxTaskCount}},@{name='Options(Unlimited Tasks)';expression={$_.Options.IsTaskCountUnlim}},@{name='Options(MaxArchiveTaskCount)';expression={$_.Options.MaxArchiveTaskCount}},@{name='Options(CombinedDataRateLimit)';expression={$_.Options.CombinedDataRateLimit}},@{name='Options(Uncompress)';expression={$_.Options.Uncompress}},@{name='Options(OptimizeBlockAlign)';expression={$_.Options.OptimizeBlockAlign}},@{name='Options(RemoteAccessLimitation)';expression={$_.Options.RemoteAccessLimitation}},@{name='Options(EpEncryptionEnabled)';expression={$_.Options.EpEncryptionEnabled}},@{name='Options(OneBackupFilePerVm)';expression={$_.Options.OneBackupFilePerVm}},@{name='Options(IsAutoDetectAffinityProxies)';expression={$_.Options.IsAutoDetectAffinityProxies}},@{name='Options(NfsRepositoryEncoding)';expression={$_.Options.NfsRepositoryEncoding}}, "CachedFreeSpace","CachedTotalSpace"
    $Servers = $Servers | Select-Object -Property "Info","ParentId","Id","Uid","Name","Reference","Description","IsUnavailable","Type","ApiVersion","PhysHostId","ProxyServicesCreds", @{name='Cores';expression={$_.GetPhysicalHost().hardwareinfo.CoresCount}},@{name='CPUCount';expression={$_.GetPhysicalHost().hardwareinfo.CPUCount}}, @{name='RAM';expression={$_.GetPhysicalHost().hardwareinfo.PhysicalRamTotal}}
$nasProxyOut = $nasProxy | Select-Object -Property "ConcurrentTaskNumber", @{n="Host";e={$_.Server.Name}}, @{n="HostId";e={$_.Server.Id}}
#$hvProxyOut = $hvProxy | Select-Object -Property "Name", "HostId", @{n=Host}

##Protected Workloads Area
$vmbackups = Get-VBRBackup | ? {$_.TypeToString -eq "VMware Backup" }
$vmNames = $vmbackups.GetLastOibs()
$unprotectedEntityInfo = Find-VBRViEntity | ? {$_.Name -notin $vmNames.Name}
$protectedEntityInfo = Find-VBRViEntity -Name $vmNames.Name
$protectedEntityInfo | select Name,PowerState,ProvisionedSize,UsedSize,Path | sort PoweredOn,Path,Name | Export-Csv -Path $("$ReportPath\$VBRServer" + '_ViProtected.csv') -NoTypeInformation
$unprotectedEntityInfo | select Name,PowerState,ProvisionedSize,UsedSize,Path,Type | sort Type,PoweredOn,Path,Name | Export-Csv -Path $("$ReportPath\$VBRServer" + '_ViUnprotected.csv') -NoTypeInformation

#protected physical Loads
$phys = Get-VBRDiscoveredComputer

$physbackups = Get-VBRBackup | ? {$_.TypeToString -like "*Agent*" }
$pvmNames = $physbackups.GetLastOibs()

$notprotected =$phys | ? {$_.Name -notin $pvmNames.Name}
$protected = $phys | ? {$_.Name -in $pvmNames.Name}

$protected | Export-Csv -Path $("$ReportPath\$VBRServer" + '_PhysProtected.csv') -NoTypeInformation
$notprotected | Export-Csv -Path $("$ReportPath\$VBRServer" + '_PhysNotProtected.csv') -NoTypeInformation

##end protected workloads Area


#GetVbrVersion:
$corePath = Get-ItemProperty -Path "HKLM:\Software\Veeam\Veeam Backup and Replication\" -Name "CorePath"
$dbPath = Get-ItemProperty -Path "HKLM:\Software\Veeam\Veeam Backup and Replication\" -Name "SqlDataBaseName"
$instancePath = Get-ItemProperty -Path "HKLM:\Software\Veeam\Veeam Backup and Replication\" -Name "SqlInstanceName"
$dbServerPath = Get-ItemProperty -Path "HKLM:\Software\Veeam\Veeam Backup and Replication\" -Name "SqlServerName"

$depDLLPath = Join-Path -Path $corePath.CorePath -ChildPath "Packages\VeeamDeploymentDll.dll" -Resolve
$file = Get-Item -Path $depDLLPath
$version = $file.VersionInfo.ProductVersion
$fixes = $file.VersionInfo.Comments

#endGetVbrVersion

#output VBR Versioning
$VbrOutput = [pscustomobject][ordered] @{
        'Version'   = $version
        'Fixes' = $fixes
        'SqlServer' = $dbServerPath.SqlServerName
        'Instance' = $instancePath.SqlInstanceName
      }
$VbrOutput | Export-Csv -Path $("$ReportPath\$VBRServer" + '_vbrinfo.csv') -NoTypeInformation
  }

  end {

    $Servers | Export-Csv -Path $("$ReportPath\$VBRServer" + '_Servers.csv') -NoTypeInformation
    $AllJobs | Export-Csv -Path $("$ReportPath\$VBRServer" + '_Jobs.csv') -NoTypeInformation
    $Proxies | Export-Csv -Path $("$ReportPath\$VBRServer" + '_Proxies.csv') -NoTypeInformation
    $repoInfo | Export-Csv -Path $("$ReportPath\$VBRServer" + '_Repositories.csv') -NoTypeInformation
    $SOBROutput | Export-Csv -Path $("$ReportPath\$VBRServer" + '_SOBRs.csv') -NoTypeInformation
    $AllSOBRExtentsOutput | Export-Csv -Path $("$ReportPath\$VBRServer" + '_SOBRExtents.csv') -NoTypeInformation
    $licInfo | Export-csv -Path $("$ReportPath\$VBRServer" + '_LicInfo.csv') -NoTypeInformation
    $wan | Export-csv -Path $("$ReportPath\$VBRServer" + '_WanAcc.csv') -NoTypeInformation
    $cdpProxy | Export-csv -Path $("$ReportPath\$VBRServer" + '_CdpProxy.csv') -NoTypeInformation
    #$fileProxy| Export-csv -Path $("$ReportPath\$VBRServer" + '_FileProxy.csv') -NoTypeInformation
    $hvProxy | Export-csv -Path $("$ReportPath\$VBRServer" + '_HvProxy.csv') -NoTypeInformation
    $nasProxyOut | Export-csv -Path $("$ReportPath\$VBRServer" + '_NasProxy.csv') -NoTypeInformation
    $piJob | Export-csv -Path $("$ReportPath\$VBRServer" + '_pluginjobs.csv') -NoTypeInformation
    #$pc | Export-csv -Path $("$ReportPath\$VBRServer" + '_protectedComputers.csv') -NoTypeInformation
    #$pg | Export-csv -Path $("$ReportPath\$VBRServer" + '_protectionGroups.csv') -NoTypeInformation
    $output | Export-csv -Path $("$ReportPath\$VBRServer" + '_regkeys.csv') -NoTypeInformation
    $capOut    | Export-csv -Path $("$ReportPath\$VBRServer" + '_capTier.csv') -NoTypeInformation
    $trafficRules | Export-csv -Path $("$ReportPath\$VBRServer" + '_trafficRules.csv') -NoTypeInformation
    $configBackup | Export-Csv -Path $("$ReportPath\$VBRServer" + '_configBackup.csv') -NoTypeInformation

    Disconnect-VBRServer
    Pop-Location
  }
}

function AddTypeInfo ($object, $jobType)
{
    $object | Add-Member -MemberType NoteProperty -Name JobType -Value $jobType

}
Get-VBRConfig -VBRServer localhost -ReportPath 'C:\temp\vHC\Original\Raw_Data'
#Read-host("Complete! Press ENTER to close...")