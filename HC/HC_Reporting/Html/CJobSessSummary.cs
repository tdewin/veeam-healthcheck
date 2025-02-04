﻿// Copyright (c) 2021, Adam Congdon <adam.congdon2@gmail.com>
// MIT License
using HC_Reporting;
using HC_Reporting.CsvHandlers;
using HC_Reporting.DataTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using VeeamHealthCheck.CsvHandlers;
using VeeamHealthCheck.DataTypes;
using VeeamHealthCheck.Logging;

namespace VeeamHealthCheck.Html
{
    class CJobSessSummary
    {
        private Dictionary<string, List<TimeSpan>> _waits = new();
        private CLogger log = MainWindow.log;

        public CJobSessSummary(string xmlFile, CLogger log, bool scrub, bool checkLogs, Scrubber.CXmlHandler scrubber, CDataTypesParser dp)
        {
            if (checkLogs)
                PopulateWaits();
            JobSessionSummaryToXml(xmlFile, log, scrub, scrubber, dp);
        }

        private void PopulateWaits()
        {
            try
            {
                FilesParser.CLogParser lp = new();
                _waits = lp.GetWaitsFromFiles();
            }
            catch (Exception e)
            {
                log.Error("Error checking log files:");
                log.Error(e.Message);
            }

        }
        private XElement ReccomendationText()
        {
            var xml = new XElement("rh",
                new XElement("title", "Recommendations"),
                new XElement("ln", "Waiting for Resources / job session length issues"),
                new XElement("bl", "•	Stagger your backup jobs and replication jobs at different time slots so that a job isn’t waiting on resources. (Ex. Instead of scheduling your jobs to all start at 8:00PM, start one job at 8:00, another at 8:30, and another at 9:00.)"),
                new XElement("bl", "•	Increase the number of concurrent tasks allowed on your proxies. See How to set max concurrent tasks. "),
                new XElement("bl", "o	If your backup proxy is a virtual machine, you should increase the amount of CPU and RAM available to the proxy.",
                new XAttribute("ind", "sIndent")),

                new XElement("bl", "•	Deploy additional backup proxies from within “Backup Infrastructure->Backup Proxies”"),
                new XElement("bl", "•	Make sure that your backup job or replication job is selecting the correct proxies by viewing the job session statistics in the VBR Console."),
                new XElement("bl", "•	Investigate backup job performance. If specific jobs are taking longer to process than normal, check for warnings, compare the bottleneck statistics to previous jobs sessions, and try to isolate the problem to a specific proxy, repository, host, or datastore."),
                new XElement("bl", "o	Move larger VMs / servers to their own job and schedule so a conflict does not occur with faster completing jobs Expected backup window exceeded (schedule before all other jobs or after all other jobs, for example)",
                new XAttribute("ind", "sIndent")),
                new XElement("bl", "•	Separate NAS Proxies, Cache Repos, and Repositories from VM proxies and VM and Agent Repository"),
                new XElement("bl", "•	Use Static Gateways and Mount Servers if possible to offload resources consumption required for synthetic operations, SOBR offload processing, backup copy jobs, and other tasks. -  https://helpcenter.veeam.com/docs/backup/vsphere/gateway_server.html?ver=110#gateway-servers-deployment"),
                new XElement("bl", "•	If appropriate, review Architecture Guidelines for deduplicating storage systems"),
                new XElement("bl", "o	https://www.veeam.com/kb2660",
                new XAttribute("ind", "sIndent")
                ));

            return xml;
        }
        private void JobSessionSummaryToXml(string xmlFile, CLogger log, bool scrub, Scrubber.CXmlHandler scrubber, CDataTypesParser d)
        {
            log.Info("converting job session summary to xml");
            List<CJobSessionInfo> jobSessionsCsv = d.JobSessions;
            jobSessionsCsv = jobSessionsCsv.OrderBy(x => x.Name).ToList();
            //csv = csv.OrderBy(x => x.CreationTime).ToList();

            XDocument doc = XDocument.Load(xmlFile);

            XElement extElement = new XElement("jobSessionsSummary");
            doc.Root.Add(extElement);

            List<string> jobNameList = jobSessionsCsv.Select(x => x.Name).ToList();
            List<CJobSummaryTypes> outList = new();

            List<double> avgRates = new();
            List<double> avgDataSizes = new();
            List<double> avgBackupSizes = new();
            List<double> maxBackupSize = new();
            List<double> maxDataSizes = new();
            List<int> successRates = new();

            int totatlProtectedInstances = 0;
            foreach (var j in jobNameList.Distinct())
            {
                CJobSummaryTypes info = new();

                try
                {
                    CCsvParser csv = new();
                    var waitList = csv.WaitsCsvReader();

                    List<TimeSpan> tList = new();

                    foreach (var w in waitList)
                    {
                        string fixedName = j.Replace(" ", "_");
                        
                        if (w.JobName == j || w.JobName == fixedName)
                        {
                            DateTime.TryParse(w.StartTime, out DateTime startTime);
                            DateTime.TryParse(w.EndTime, out DateTime endTime);
                            DateTime now = DateTime.Now;
                            double startDiff = (now - startTime).TotalDays;
                            double endDiff = (now - endTime).TotalDays;

                            if (endDiff < 7 || startDiff < 7)
                            {
                                TimeSpan.TryParse(w.Duration, out TimeSpan duration);
                                tList.Add(duration);
                            }
                        }
                    }
                    string mWait = tList.Max().ToString(@"dd\.hh\:mm\:ss");
                    info.maxWait = mWait;

                    var avg = tList.Average(x => x.Ticks);
                    long longAvg = Convert.ToInt64(avg);
                    TimeSpan t = new TimeSpan(longAvg);
                    info.avgwait = t.ToString(@"dd\.hh\:mm\:ss");
                    info.waitCount = tList.Count();

                    //if (_waits.Count != 0)
                    //{
                    //    _waits.TryGetValue(j, out List<TimeSpan> r);
                    //    if (r == null)
                    //    {
                    //        string fixedName = j.Replace(" ", "_");
                    //        _waits.TryGetValue(fixedName, out r);
                    //    }

                    //    //if (r.Count !=0)
                    //    //{
                    //    //    //do
                    //    //}
                    //    //var v = r.Max();
                        

                    //}

                }
                catch (Exception e) { }
                double sessionCount = 0;
                double fails = 0;
                List<TimeSpan> durations = new();
                List<string> vmNames = new();
                List<double> dataSize = new();
                List<double> backupSize = new();
                string type = "";

                foreach (var c in jobSessionsCsv)
                {
                    try
                    {
                        DateTime now = DateTime.Now;
                        double diff = (now - c.CreationTime).TotalDays;
                        ////if (session.CreationTime.Day == now.Day)
                        ////{

                        ////}
                        //if (diff < days)
                        if (j == c.Name && diff < 7)
                        {
                            sessionCount++;
                            if (c.Status == "Failed")
                                fails++;

                            TimeSpan.TryParse(c.JobDuration, out TimeSpan jDur);

                            durations.Add(jDur); //need to parse this to TimeSpan
                            vmNames.Add(c.VmName);

                            dataSize.Add(c.DataSize);
                            backupSize.Add(c.BackupSize);
                            type = c.JobType;
                        }
                    }
                    catch(Exception e) { }
                   
                }
                List<TimeSpan> nonZeros = new();
                foreach (var du in durations)
                {
                    if (durations.Count == 0)
                        nonZeros.Add(du);
                    if (du.Ticks != 0)
                        nonZeros.Add(du);
                }
                //return:
                // , avg duration, min/max, 
                try
                {
                    info.sessionCount = (int)sessionCount;
                    if(sessionCount != 0)
                    {
                        double percent = ((sessionCount - fails) / sessionCount) * 100;
                        info.SuccessRate = (int)Math.Round(percent, 0, MidpointRounding.ToEven);
                    }

                    successRates.Add(info.SuccessRate);
                    info.JobName = j;

                    if (nonZeros.Count != 0)
                    {
                        info.MinJobTime = nonZeros.Min().ToString(@"hh\:mm\:ss");
                        info.MaxJobTime = nonZeros.Max().ToString(@"hh\:mm\:ss");
                        var s = new TimeSpan(Convert.ToInt64(nonZeros.Average(ts => ts.Ticks)));
                        info.AvgJobTime = s.ToString(@"hh\:mm\:ss");
                    }
                    else
                    {
                        info.MinJobTime = "";
                        info.MaxJobTime = "";
                        info.AvgJobTime = "";
                    }


                    info.ItemCount = vmNames.Distinct().Count();
                    totatlProtectedInstances = totatlProtectedInstances + vmNames.Distinct().Count();

                    info = SetBackupDataSizes(info, dataSize, backupSize);

                    avgDataSizes.Add(info.AvgDataSize);
                    avgBackupSizes.Add(info.AvgBackupSize);
                    maxDataSizes.Add(info.MaxDataSize);
                    maxBackupSize.Add(info.MaxBackupSize);

                    info.JobType = type;

                    if (info.AvgBackupSize != 0 && info.AvgDataSize != 0)
                    {
                        info.AvgChangeRate = Math.Round(((info.AvgBackupSize / info.AvgDataSize) * 100), 0);
                        avgRates.Add(info.AvgChangeRate);
                    }
                    else
                    {
                        info.AvgBackupSize = 0;
                        avgRates.Add(0);

                    }


                    outList.Add(info);
                }
                catch (Exception e)
                {

                }
            }

            foreach (var o in outList)
            {
                try
                {
                    string jname = o.JobName;
                    if (scrub)
                        jname = scrubber.ScrubItem(o.JobName, "job");

                    string wait = o.waitCount.ToString();
                    if (o.waitCount == 0)
                        wait = "";


                    var xml = new XElement("session",
                new XElement("name", jname),
                new XElement("items", o.ItemCount),
                new XElement("avgtime", o.AvgJobTime),
                new XElement("maxtime", o.MaxJobTime),
                new XElement("mintime", o.MinJobTime),
                new XElement("sessions", o.sessionCount),
                new XElement("successrate", o.SuccessRate),
                new XElement("avgBackupSize", Math.Round(o.AvgBackupSize, 2)),
                new XElement("maxBackupSize", Math.Round(o.MaxBackupSize, 2)),
                new XElement("avgDataSize", Math.Round(o.AvgDataSize, 2)),
                new XElement("changerate", Math.Round(o.AvgChangeRate, 2)),
                new XElement("maxDataSize", Math.Round(o.MaxDataSize, 2)),
                new XElement("waitCount", wait),
                new XElement("avgWait", o.avgwait),
                new XElement("maxWait", o.maxWait),
                new XElement("type", o.JobType)
    );

                    extElement.Add(xml);
                }
                catch(Exception e) { }
            }

            var summaryXml = new XElement("session",
                new XElement("name", "Total"),
                new XElement("items", totatlProtectedInstances),
                new XElement("avgtime", ""),
                new XElement("maxtime", ""),
                new XElement("mintime", ""),
                new XElement("sessions", ""),
                new XElement("successrate", Math.Round(successRates.Average(), 0)),
                new XElement("avgBackupSize", Math.Round(avgBackupSizes.Average(), 0)),
                new XElement("maxBackupSize", Math.Round(maxBackupSize.Sum(), 0)),
                new XElement("avgDataSize", Math.Round(avgDataSizes.Average(), 0)),
                new XElement("changerate", Math.Round(avgRates.Average(), 0)),
                new XElement("maxDataSize", Math.Round(maxDataSizes.Sum(), 2))
                );
            extElement.Add(summaryXml);

            //extElement.Add(ReccomendationText());

            doc.Save(xmlFile);
            log.Info("converting job session summary to xml..done!");
        }

        private static CJobSummaryTypes SetBackupDataSizes(CJobSummaryTypes info, List<double> dataSize, List<double> backupSize)
        {
            if(backupSize.Count != 0)
            {
                info.MinBackupSize = backupSize.Min() / 1024;
                info.MaxBackupSize = backupSize.Max() / 1024;
                info.AvgBackupSize = Math.Round(backupSize.Average() / 1024, 2);
            }
            else
            {
                info.MinBackupSize = 0;
                info.MaxBackupSize = 0;
                info.AvgBackupSize = 0;
            }
            
            if(dataSize.Count != 0)
            {
                info.MinDataSize = dataSize.Min() / 1024;
                info.MaxDataSize = dataSize.Max() / 1024;
                info.AvgDataSize = Math.Round(dataSize.Average() / 1024, 2);
            }
            else
            {
                info.MinDataSize = 0;
                info.MaxDataSize = 0;
                info.AvgDataSize = 0;
            }
            return info;   
        }

        private int SplitDurationToMinutes(string duration)
        {
            try
            {
                //log.Info("splitting duration..");
                int i = 0;
                //00:01:59.4060000
                string[] split = duration.Split(':');
                int hours = 0;
                int minutes = 0;
                int seconds = 0;

                if (split[0] != "00")
                {
                    int.TryParse(split[0], out int h);
                    hours = h;
                }
                if (split[1] != "00")
                {
                    int.TryParse(split[1], out int m);
                    minutes = m;
                }
                minutes = minutes + (hours * 60);

                //log.Info("splitting duration..done!");
                return minutes;
            }
            catch (Exception e) { return 0; }
        }
    }
}
