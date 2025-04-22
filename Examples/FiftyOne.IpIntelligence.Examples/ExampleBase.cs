/* *********************************************************************
 * This Original Work is copyright of 51 Degrees Mobile Experts Limited.
 * Copyright 2025 51 Degrees Mobile Experts Limited, Davidson House,
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

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using Microsoft.Extensions.Logging;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

namespace FiftyOne.IpIntelligence.Examples
{
    public class ExampleBase
    {
        /// <summary>
        /// Loops around the IP addresses file returning IP addresses until the
        /// required number have been returned.
        /// </summary>
        /// <param name="ipAddressesFile">Source file of IP addresses where
        /// each IP address is a single line of characters</param>
        /// <param name="count">Number of IP addresses to return</param>
        /// apply</param>
        /// <returns>IP addresses with repetition</returns>
        protected static IEnumerable<string> GetIpAddresses(
            string ipAddressesFile,
            int count)
        {
            var index = 0;
            IList<string> ipAddresses;
            using (var stream = File.OpenRead(ipAddressesFile))
            using (var reader = new StreamReader(stream))
            {
                ipAddresses = GetEvidence(reader)
                    .SelectMany(x => x.Values)
                    .Select(x => x as string)
                    .Where(x => x is object)
                    .ToList();
            }
            for (var i = 0; i < Math.Min(count, ipAddresses.Count); i++)
            {
                yield return ipAddresses[index++];

                // Reset the IpAddresses if all the list has been returned.
                if (index >= ipAddresses.Count)
                {
                    index = 0;
                }
            }
        }

        /// <summary>
        /// Returns IP addresses that have had a number of characters altered.
        /// The selection of digits are random.
        /// </summary>
        /// <param name="ipAddressesFile">Source file of IP addresses where
        /// each IP address is a single line of characters</param>
        /// <param name="count">Number of IP addresses to return</param>
        /// <param name="randomness">Number of character adjustments to 
        /// apply</param>
        /// <returns>IP addresses that are effectively unique</returns>
        protected static IEnumerable<string> GetIpAddresses(
            string ipAddressesFile,
            int count,
            int randomness)
        {
            var random = new Random();
            var iterator = GetIpAddresses(ipAddressesFile, count).GetEnumerator();
            while (iterator.MoveNext())
            {
                var array = iterator.Current.ToCharArray();
                for (var i = 0; i < randomness; i++)
                {
                    var index = random.Next(array.Length - 1);
                    array[index]++;
                }
                yield return new string(array);
            }
        }

        /// <summary>
        /// Read the specified yaml-formatted stream and return evidence collections.
        /// </summary>
        /// <param name="evidenceReader">
        /// A <see cref="TextReader"/> containing the yaml-formatted evidence data to be ingested.
        /// </param>
        /// <param name="logger">
        /// A logger instance. If null is passed then progress messages will not be logged.
        /// </param>
        /// <returns></returns>
        protected static IEnumerable<Dictionary<string, object>> GetEvidence(
            TextReader evidenceReader,
            ILogger logger = null)
        {
            var deserializer = new Deserializer();
            var yamlReader = new Parser(evidenceReader);

            // Consume the stream start event.
            yamlReader.Consume<StreamStart>();
            int records = 0;
            int skipped = 0;
            // Keep going as long as we have more document records.
            while (yamlReader.TryConsume<DocumentStart>(out _))
            {
                // Output progress.
                records++;
                if (logger != null && records % 1000 == 0)
                {
                    logger.LogInformation($"Processed {records} records ({skipped} skipped)");
                }

                // Deserialize the record
                var data = deserializer.Deserialize<Dictionary<string, object>>(yamlReader);
                if (data is null)
                {
                    break;
                }

                // Remove null values
                foreach(var keyWithNullValue in data.Where(kvp => kvp.Value is null).Select(kvp => kvp.Key).ToList())
                {
                    logger.LogWarning($"Document at offset {records-1} contains null value for key: '{keyWithNullValue}'!");
                    data.Remove(keyWithNullValue);
                }

                if (data.Count > 0)
                {
                    yield return data;
                }
                else
                {
                    logger.LogWarning($"Document at offset {records - 1} contains no usable evidence!");
                    ++skipped;
                }

                // Required to move to the start of the next record.
                yamlReader.TryConsume<DocumentEnd>(out _);
            }
        }

        protected static IEnumerable<string> Report(
            List<string> input,
            int count,
            int maxDistinctIAs,
            int marks)
        {
            if (maxDistinctIAs > input.Count) { maxDistinctIAs = input.Count; }
            Random rnd = new Random();
            var current = 0;
            var increment = count / marks;

            while (current < count)
            {
                yield return input.ElementAt(rnd.Next(0, maxDistinctIAs));
                if (current % increment == 0)
                {
                    Console.Write("=");
                }
                current++;
            }
            Console.WriteLine("");
        }



        protected static void OutputException(Exception ex, int depth = 0)
        {
            StringBuilder message = new StringBuilder();
            AddToMessage(message, $"{ex.GetType().Name} - {ex.Message}", depth);
            AddToMessage(message, $"{ex.StackTrace}", depth);
            Console.WriteLine(message);
            if (ex.InnerException != null)
            {
                OutputException(ex.InnerException, depth++);
            }
        }

        private static void AddToMessage(StringBuilder message, string textToAdd, int depth)
        {
            for (int i = 0; i < depth; i++)
            {
                message.Append("   ");
            }
            message.AppendLine(textToAdd);
        }
    }
}
