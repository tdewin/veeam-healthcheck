﻿// Copyright (c) 2021, Adam Congdon <adam.congdon2@gmail.com>
// MIT License
using System;
using CsvHelper.Configuration.Attributes;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HC_Reporting
{
    class CSobrExtentCsvInfos
    {
        //"Host","Id","Name","HostId","MountHostId","Description","CreationTime","Path","FullPath","FriendlyPath","ShareCredsId","Type","Status","IsUnavailable","Group","UseNfsOnMountHost","VersionOfCreation","Tag","IsTemporary","TypeDisplay","IsRotatedDriveRepository","EndPointCryptoKeyId","HasBackupChainLengthLimitation","IsSanSnapshotOnly","IsDedupStorage","SplitStoragesPerVm","IsImmutabilitySupported","SOBR_Name","Options(maxtasks)","Options(Unlimited Tasks)","Options(MaxArchiveTaskCount)","Options(CombinedDataRateLimit)","Options(Uncompress)","Options(OptimizeBlockAlign)","Options(RemoteAccessLimitation)","Options(EpEncryptionEnabled)","Options(OneBackupFilePerVm)","Options(IsAutoDetectAffinityProxies)","Options(NfsRepositoryEncoding)"
        [Index(0)]
        public string HostName { get; set; }
        [Index(1)]
        public string Id { get; set; }
        [Index(2)]
        public string Name { get; set; }
        [Index(3)]
        public string HostId { get; set; }
        [Index(4)]
        public string MountHostId { get; set; }
        [Index(5)]
        public string Description { get; set; }
        [Index(6)]
        public string CreationTime { get; set; }
        [Index(7)]
        public string Path { get; set; }
        [Index(8)]
        public string FullPath { get; set; }
        [Index(9)]
        public string FriendlyPath { get; set; }
        [Index(10)]
        public string ShareCredsId { get; set; }
        [Index(11)]
        public string Type { get; set; }
        [Index(12)]
        public string Status { get; set; }
        [Index(13)]
        public string IsUnavailable { get; set; }
        [Index(14)]
        public string Group { get; set; }
        [Index(15)]
        public string UseNfsOnMountHost { get; set; }
        [Index(16)]
        public string VersionOfCreation { get; set; }
        [Index(17)]
        public string Tag { get; set; }
        [Index(18)]
        public string IsTemprorary { get; set; }
        [Index(19)]

        public string TypeDisplay { get; set; }
        [Index(20)]
        public string IsRotatedDriveRepository { get; set; }
        [Index(21)]
        public string EndPointCryptoKeyId { get; set; }
        [Index(22)]
        public string HasBackupChainLengthLimitation { get; set; }
        [Index(23)]

        public string IsSanSnapshotOnly { get; set; }
        [Index(24)]
        public string IsDedupStorage { get; set; }
        [Index(25)]
        public string SplitStoragesPerVm { get; set; }
        [Index(26)]
        public string IsImmutabilitySupported { get; set; }
        [Index(27)]

        public string SOBR_Name { get; set; }
        [Index(28)]

        public string MaxTasks { get; set; }
        [Index(29)]

        public string UnlimitedTasks { get; set; }
        [Index(30)]
        public string MaxArchiveTaskCount { get; set; }
        [Index(31)]
        public string CombinedDataRateLimit { get; set; }
        [Index(32)]
        public string UnCompress { get; set; }
        [Index(33)]
        public string OptimizeBlockAlign { get; set; }
        [Index(34)]
        public string RemoteAccessLimitation { get; set; }
        [Index(35)]
        public string EpEncryptionEnabled { get; set; }
        [Index(36)]
        public string OneBackupFilePerVm { get; set; }
        [Index(37)]
        public string IsAutoDetectAffinityProxies { get; set; }
        [Index(38)]
        public string NfsRepositoryEncoding { get; set; }
        [Index(39)]
        public string FreeSpace { get; set; }
        [Index(40)]
        public string TotalSpace { get; set; }

    }
}
