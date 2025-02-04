﻿// Copyright (c) 2021, Adam Congdon <adam.congdon2@gmail.com>
// MIT License
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VeeamHealthCheck.DB
{
    class CModel
    {
        public enum EMyJobTypes
        {
            VMware,
            HV,
            Nutanix,

        }
        public enum EDbJobType
        {
            //Unknown = 666,
            Backup = 0,
            Replica = 1,
            //Copy = 2,
            SureBackup = 3,
            //RestoreVm = 4, RestoreVmFiles = 5, RestoreFiles = 6,
            //Failover = 7,
            //QuickMigration = 8,
            //UndoFailover = 9,
            //FileLevelRestore = 10, LinuxFileLevelRestore = 11,
            //InstantRecovery = 12,
            //RestoreHdd = 13,
            //Failback = 14, PermanentFailover = 15, UndoFailback = 16, CommitFailback = 17,
            //ShellRun = 18,
            //VolumesDiscover = 19,
            //HvCtpRescan = 20,
            //CatCleanup = 21,
            //SanRescan = 22, CreateSanSnapshot = 23, SanMonitor = 30, DeleteSanSnapshot = 31,
            FileTapeBackup = 24,
            //FileTapeRestore = 25, TapeValidate = 26, TapeInventory = 27, 
            VmTapeBackup = 28, 
            //VmTapeRestore = 29, TapeTenantRestore = 500,
            //TapeErase = 32,
            //TapeEject = 33,
            //TapeExport = 34,
            //TapeImport = 35,
            //TapeCatalog = 36,
            //TapeLibrariesDiscover = 37,
            //PowerShellScript = 38,
            //VmReconfig = 39,
            //VmStart = 40,
            //VcdVAppRestore = 41,
            //VcdVmRestore = 42,
            //HierarchyScan = 46,
            //ViVmConsolidation = 47,
            //ApplicationLevelRestore = 48,
            //RemoteReplica = 50,
            BackupSync = 51,
            SqlLogBackup = 52,
            //LicenseAutoUpdate = 53,
            OracleLogBackup = 54,
            //TapeMarkAsFree = 55,
            //TapeDeleteFromLibrary = 56,
            //TapeMoveToMediaPool = 57,
            //TapeCatalogueDecrypted = 58,
            //StorageCopyPolicy = 60,
            //StorageCopyWorker = 61,
            //StorageCopyParentWorker = 62,
            SimpleBackupCopyWorker = 63,
            //QuickMigrationCheck = 64,
            //SimpleBackupCopyPolicy = 65,
            //SimpleSqlBackupCopyWorker = 68,
            //SimpleOracleBackupCopyWorker = 69,
            //SimpleBackupCopyParentWorker = 70,
            //RepoCopyPolicy = 73,
            ConfBackup = 100,
            //ConfRestore = 101,
            //ConfResynchronize = 102,
            //WaGlobalDedupFill = 103,
            //DatabaseMaintenance = 104,
            //RepositoryMaintenance = 105,
            //InfrastructureRescan = 106,
            //DiskInstantRecovery = 110,
            //DiskInstantPublishing = 111,
            //IscsiInstantPublishing = 112,
            //MoutServiceMount = 113,
            //HvLabDeploy = 200,
            //HvLabDelete = 201,
            //FailoverPlan = 202,
            //UndoFailoverPlan = 203,
            //FailoverPlanTask = 204,
            //UndoFailoverPlanTask = 205,
            //PlannedFailover = 206,
            //ViLabDeploy = 207,
            //ViLabDelete = 208,
            //ViLabStart = 209,
            Cloud = 300,
            //CloudApplDeploy = 301,
            //HardwareQuotasProcessing = 302,
            //ReconnectVpn = 303,
            //DisconnectVpn = 304,
            OrchestratedTask = 305,
            //ViReplicaRescan = 306,
            //ExternalRepositoryMaintenance = 307,
            //DeleteBackup = 308,
            //CloudProviderRescan = 309,
            //AzureApplDeploy = 401,
            //EndpointBackup = 4000,
            //EndpointRestore = 4005,
            //BackupCacheSync = 4010,
            //EndpointSqlLogBackup = 4020,
            //EndpointOracleLogBackup = 4021,
            OracleRMANBackup = 4030,
            SapBackintBackup = 4031,
            //OracleRMANRestore = 4032,
            //SapBackintRestore = 4033,
            //OracleRMANBackupCopyWorker = 4035,
            //SapBackintBackupCopyWorker = 4037,
            //PluginBackupCopyPolicy = 4038,
            CloudBackup = 5000,
            //RestoreVirtualDisks = 6000,
            //RestoreAgentVolumes = 6001,
            //InfraItemSave = 7000,
            //InfraItemUpgrade = 7001,
            //InfraItemDelete = 7002,
            //AzureWinProxySave = 7003,
            //FileLevelRestoreByEnterprise = 8000,
            //RepositoryEvacuate = 9000,
            //LogsExport = 10000,
            //InfraStatistic = 10001,
            //AzureVmRestore = 11000,

            EpAgentManagement = 12000,
            //EpAgentDiscoveryObsolete = 12001,   // Use EpAgentDiscovery
            EpAgentPolicy = 12002,              // Managed by agent (parent job type)
            EpAgentBackup = 12003,              // Managed by VBR (parent job type)
            //EpAgentTestCreds = 12004,
            //EpAgentDiscovery = 12005,
            //EpAgentDeletedRetention = 12006,

            //EpAgentOperationBackupNow = 12007,
            //EpAgentOperationActiveFull = 12008,
            //EpAgentOperationStopBackup = 12009,
            //EpAgentOperationPurgeCache = 12010,

            NasBackup = 13000,
            //NasBackupBrowse = 13001,
            //NasRestore = 13002,
            //NasBackupCopy = 13003,
            //NasDownloadMeta = 13004,


            //VmbApiPolicyTempJob = 14000,         // VM Backup Api (N2W) (parent job type)

            //ExternalInfrastructureRescan = 15000,

            //AmazonRestore = 16000,
            //StagedRestore = 17000,

            ArchiveBackup = 18000,
            //ArchiveRehydration = 18001,
            //ArchiveDownload = 18002,
            //ArchiveSync = 18003,
            //ArchiveCopy = 18004,

            //HvStagedRestore = 19000,

            //VbkExport = 20000,

            //GuestScriptingConnect = 21000,

            //ForeignTransform = 22000,

            //AuditZip = 23000,

            //CustomPlatformRestoreVm = 24000,
        }
    }
}
