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

using FiftyOne.Common;
using FiftyOne.Common.TestHelpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace FiftyOne.IpIntelligence.TestHelpers
{
    public static class Utils
    {

        /// <summary>
        /// The folder that contains the C++ and therefore device data folder.
        /// </summary>
        private const string OnPremiseDirectory =
            "FiftyOne.IpIntelligence.Engine.OnPremise";

        /// <summary>
        /// Cache of file names to file infos to speed up tests.
        /// </summary>
        private static readonly ConcurrentDictionary<string, FileInfo> Cache =
            new ConcurrentDictionary<string, FileInfo>();

        public static FileInfo GetFilePath(string filename)
        {
            return Cache.GetOrAdd(
                filename,
                (f) =>
                {
                    return TestUtils.GetFilePath(filename, GetOnPremiseDirectory());
                });
        }

        /// <summary>
        /// Finds the on premise directory where the test data files are
        /// expected to be located.
        /// </summary>
        /// <returns></returns>
        private static DirectoryInfo GetOnPremiseDirectory()
        {
            var current = new DirectoryInfo(Environment.CurrentDirectory);
            while (current != null)
            {
                var onPremise = current.GetDirectories(
                    OnPremiseDirectory,
                    SearchOption.TopDirectoryOnly);
                if (onPremise.Length == 1)
                {
                    return onPremise[0];
                }
                current = current.Parent;
            }
            throw new DirectoryNotFoundException(OnPremiseDirectory);
        }

        public static string FindFile(string filename, DirectoryInfo dir)
        {
            return FileUtils.FindFile(filename, dir);
        }
    }
}
