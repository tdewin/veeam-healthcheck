﻿// Copyright (c) 2021, Adam Congdon <adam.congdon2@gmail.com>
// MIT License
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HC_Reporting.DataTypes
{
    class CSobrTypeInfos
    {
        public string PolicyType { get; set; }
        public string Extents { get; set; }
        public bool UsePerVMBackupFiles { get; set; }
        public bool PerformFullWhenExtentOffline { get; set; }
        public bool EnableCapacityTier { get; set; }
        public int OperationalRestorePeriod { get; set; }
        public bool OverridePolicyEnabled { get; set; }
        public int OverrideSpaceThreshold { get; set; }
        public string OffloadWindowOptions { get; set; }
        public string CapacityExtent { get; set; }
        public bool EncryptionEnabled { get; set; }
        public string EncryptionKey { get; set; }
        public string CapacityTierCopyPolicyEnabled { get; set; }
        public string CapacityTierMovePolicyEnabled { get; set; }
        public bool ArchiveTierEnabled { get; set; }
        public string ArchiveExtent { get; set; }
        public string ArchivePeriod { get; set; }
        public bool CostOptimizedArchiveEnabled { get; set; }
        public bool ArchiveFullBackupModeEnabled { get; set; }
        public bool PluginBackupsOffloadEnabled { get; set; }
        public bool CopyAllPluginBackupsEnabled { get; set; }
        public bool CopyAllMachineBackupsEnabled { get; set; }
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        public string CapTierName { get; set; }
        public string CapTierType { get; set; }
        public string ImmuteEnabled { get; set; }
        public string ImmutePeriod { get; set; }
        public string SizeLimitEnabled { get; set; }
        public string SizeLimit { get; set; }

        public int ExtentCount { get; set; }

        public CSobrTypeInfos()
        {

        }
    }
}
