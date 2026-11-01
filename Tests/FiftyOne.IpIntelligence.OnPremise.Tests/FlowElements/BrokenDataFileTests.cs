/* *********************************************************************
 * This Original Work is copyright of 51 Degrees Mobile Experts Limited.
 * Copyright 2026 51 Degrees Mobile Experts Limited, Davidson House,
 * Forbury Square, Reading, Berkshire, United Kingdom RG1 3EU.
 *
 * This Original Work is licensed under the European Union Public Licence
 * (EUPL) v.1.2 and is subject to its terms as set out below.
 *
 * If a copy of the EUPL was not distributed with this file, You can obtain
 * one at https://opensource.org/licenses/EUPL-1.2.
 *
 * The 'Compatible Licences' set out in the Appendix to the EUPL (as may be
 * amended by the European Commission) shall be deemed incompatible for
 * the purposes of the Work and the provisions of the compatibility
 * clause in Article 5 of the EUPL shall not apply.
 *
 * If using the Work as, or as part of, a network application, by
 * including the attribution notice(s) required under Article 5 of the EUPL
 * in the end user terms of the application under an appropriate heading,
 * such notice(s) shall fulfill the requirements of that article.
 * ********************************************************************* */

using FiftyOne.IpIntelligence.Engine.OnPremise.FlowElements;
using FiftyOne.Pipeline.Core.FlowElements;
using FiftyOne.Pipeline.Engines;
using FiftyOne.Pipeline.Engines.Services;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;

namespace FiftyOne.IpIntelligence.OnPremise.Tests.FlowElements;

/// <summary>
/// The real engine and data update service, with a data file that cannot be
/// loaded put in place while the service runs. The engine must keep
/// answering from the data it has, and the service must apply a later good
/// file.
/// </summary>
/// <remarks>
/// The broken file is the ASN data file with one more component in its
/// header than the file holds, which the native code reports only through
/// its exception. Each change is one write of the header bytes in place, so
/// the file watcher sees each change once and never a partial file.
/// </remarks>
[TestClass]
public class BrokenDataFileTests
{
    private const string DataFileName = "51Degrees-IPIV4AsnIpiV41.ipi";

    // The published date's year, then the components' collection header,
    // whose count is at byte 102 as collection positions are 64 bit.
    private const int PublishedYearOffset = 66;
    private const int ComponentCountOffset = 102;
    private const int HeaderEnd = ComponentCountOffset + sizeof(uint);

    private const string ClientIp = "1.1.1.1";

    private static readonly TimeSpan Wait = TimeSpan.FromSeconds(30);

    // Longer than the steps file write times move in.
    private static readonly TimeSpan WriteTimeStep = TimeSpan.FromMilliseconds(50);

    [TestMethod]
    [DataRow(PerformanceProfiles.LowMemory)]
    [DataRow(PerformanceProfiles.MaxPerformance)]
    [DataRow(PerformanceProfiles.HighPerformance)]
    [DataRow(PerformanceProfiles.Balanced)]
    [DataRow(PerformanceProfiles.BalancedTemp)]
    public void Watcher_BrokenFile_KeepsAnsweringAndAppliesLaterFile(
        PerformanceProfiles profile)
    {
        var source = TestHelpers.Utils.GetFilePath(DataFileName);
        var directory = Directory.CreateDirectory(Path.Combine(
            Path.GetTempPath(),
            Path.GetRandomFileName())).FullName;
        var master = Path.Combine(directory, DataFileName);
        var good = File.ReadAllBytes(source.FullName);
        File.WriteAllBytes(master, good);

        var broken = (byte[])good.Clone();
        BitConverter.GetBytes(
            BitConverter.ToUInt32(good, ComponentCountOffset) + 1)
            .CopyTo(broken, ComponentCountOffset);
        var later = (byte[])good.Clone();
        var year = BitConverter.ToInt16(good, PublishedYearOffset);
        BitConverter.GetBytes((short)(year + 1))
            .CopyTo(later, PublishedYearOffset);

        var loggerFactory = new LoggerFactory();
        var service = new DataUpdateService(
            loggerFactory.CreateLogger<DataUpdateService>(),
            new HttpClient());
        var reports = new BlockingCollection<AutoUpdateStatus>();
        service.CheckForUpdateComplete += (sender, e) => reports.Add(e.Status);
        var builder = new IpiOnPremiseEngineBuilder(loggerFactory, service);
        builder.SetPerformanceProfile(profile);
        builder.SetTempDirPath(Directory.CreateDirectory(
            Path.Combine(directory, "temp")).FullName);
        builder.SetDataFileSystemWatcher(true);
        builder.SetDataUpdateOnStartup(false);
        // Automatic updates must be on for the watcher. The polling timer
        // is left for a day so the test never downloads anything.
        builder.SetAutoUpdate(true);
        builder.SetDataUpdateLicenseKey("not-used");
        builder.SetUpdatePollingInterval(TimeSpan.FromDays(1));
        builder.SetUpdateRandomisationMax(TimeSpan.Zero);

        using (var engine = builder.Build(master, true))
        using (var pipeline = new PipelineBuilder(loggerFactory)
            .AddFlowElement(engine)
            .Build())
        {
            var dataFile = engine.DataFiles.Single();
            var before = Lookup(pipeline, engine);

            WriteHeader(master, broken);
            Assert.IsTrue(
                WaitFor(reports, AutoUpdateStatus.AUTO_UPDATE_REFRESH_FAILED),
                "The update service did not report the failed refresh.");
            Assert.AreEqual(
                before,
                Lookup(pipeline, engine),
                "The engine does not answer from the data it had.");

            WriteHeader(master, later);
            Assert.IsTrue(
                WaitFor(reports, AutoUpdateStatus.AUTO_UPDATE_SUCCESS),
                "The update service did not apply the later good file.");
            Assert.AreEqual(year + 1, dataFile.DataPublishedDateTime.Year);
            Assert.AreEqual(before, Lookup(pipeline, engine));
        }

        try
        {
            Directory.Delete(directory, true);
        }
        catch (IOException)
        {
            // The native code can still hold a temporary copy for a moment.
        }
    }

    // Every value of every available property, as one string to compare.
    private static string Lookup(IPipeline pipeline, IpiOnPremiseEngine engine)
    {
        using (var data = pipeline.CreateFlowData())
        {
            data.AddEvidence("query.client-ip", ClientIp);
            data.Process();
            var ipData = data.GetFromElement(engine);
            return string.Join(";", engine.Properties
                .Where(p => p.Available)
                .OrderBy(p => p.Name, StringComparer.Ordinal)
                .Select(p =>
                {
                    var values = ipData.GetValues(p.Name);
                    return p.Name + "=" + (values.HasValue
                        ? string.Join("|", values.Value)
                        : "(none)");
                }));
        }
    }

    // Writes the header bytes from the data in place with one write. The
    // service only refreshes for a later write time, so the write waits
    // until the clock is clear of the last one, and it tries again while
    // the service has the file open to check it.
    private static void WriteHeader(string path, byte[] data)
    {
        var last = File.GetLastWriteTimeUtc(path);
        while (DateTime.UtcNow - last < WriteTimeStep)
        {
            Thread.Sleep(10);
        }
        var deadline = DateTime.UtcNow + Wait;
        while (true)
        {
            try
            {
                using (var stream = new FileStream(
                    path, FileMode.Open, FileAccess.Write, FileShare.ReadWrite))
                {
                    stream.Position = PublishedYearOffset;
                    stream.Write(
                        data,
                        PublishedYearOffset,
                        HeaderEnd - PublishedYearOffset);
                }
                return;
            }
            catch (IOException) when (DateTime.UtcNow < deadline)
            {
                Thread.Sleep(10);
            }
        }
    }

    // Takes reports until one has the status, ignoring the others that the
    // watcher raises for the same change.
    private static bool WaitFor(
        BlockingCollection<AutoUpdateStatus> reports,
        AutoUpdateStatus status)
    {
        var deadline = DateTime.UtcNow + Wait;
        while (true)
        {
            var remaining = deadline - DateTime.UtcNow;
            if (remaining < TimeSpan.Zero ||
                reports.TryTake(out var report, remaining) == false)
            {
                return false;
            }
            if (report == status)
            {
                return true;
            }
        }
    }
}
