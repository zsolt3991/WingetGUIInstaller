using CsvHelper;
using CsvHelper.Configuration;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WingetHelper.Models;
using YamlDotNet.Serialization;

namespace WingetHelper.Utils
{
    internal static class ResponseDecoder
    {
        internal static bool ParseInstallSuccessResult(List<string> commandResult)
        {
            return commandResult.Any(line => line.Contains("successfully installed", StringComparison.InvariantCultureIgnoreCase));
        }

        internal static object ParseResultsTable<T>(Task<List<string>> commandResult)
        {
            throw new NotImplementedException();
        }

        internal static bool ParseUninstallSuccessResult(List<string> commandResult)
        {
            return commandResult.Any(line => line.Contains("successfully uninstalled", StringComparison.InvariantCultureIgnoreCase));
        }

        internal static WingetPackageDetails ParseDetailsYaml(List<string> output)
        {
            var result = default(WingetPackageDetails);
            var foundIndex = -1;
            output = output.Where(line => !string.IsNullOrEmpty(line)).ToList();

            for (int i = 1; i < output.Count; i++)
            {
                var currentLine = output[i];
                if (currentLine.StartsWith("no", StringComparison.InvariantCultureIgnoreCase))
                {
                    break;
                }
                if (currentLine.StartsWith("found", StringComparison.InvariantCultureIgnoreCase))
                {
                    foundIndex = i;
                    break;
                }
            }
            if (foundIndex != -1)
            {
                try
                {
                    var cleanedOutput = SanitizeOutputAsYaml(output.Skip(foundIndex + 1).ToList());
                    var yamlData = string.Join(Environment.NewLine, cleanedOutput);
                    result = new DeserializerBuilder()
                       .IgnoreUnmatchedProperties()
                       .Build()
                       .Deserialize<WingetPackageDetails>(yamlData);
                }
                catch (Exception ex)
                { }
            }
            return result;
        }

        private static List<string> SanitizeOutputAsYaml(List<string> output)
        {
            var keyvalue = ConvertOutputToKeyValue(output);
            var result = new List<string>();

            foreach (var (key, value) in keyvalue)
            {
                if (value.Count == 1)
                {
                    result.Add(string.Concat(key, ": ", value.FirstOrDefault()));
                }
                else
                {
                    result.Add(string.Concat(key, ": |"));
                    value.ForEach(value => result.Add(string.Concat(" ", value.TrimStart(' '))));
                }
            }
            return result;
        }

        private static Dictionary<string, List<string>> ConvertOutputToKeyValue(List<string> output)
        {
            string[] keywords = typeof(WingetPackageDetails).GetProperties().Select(p => p.Name.Replace("_", " ")).ToArray();
            Dictionary<string, List<string>> keyValuePairs = new Dictionary<string, List<string>>();
            var key = string.Empty;
            var value = default(List<string>);
            for (int i = 0; i < output.Count; i++)
            {
                var currentLine = output[i];
                if (!keywords.Any(keyword => currentLine.StartsWith(keyword)))
                {
                    if (string.IsNullOrEmpty(key))
                    {
                        throw new Exception("Unexpected line in response");
                    }
                    else
                    {
                        value.Add(currentLine);
                    }
                }
                else
                {
                    if (!string.IsNullOrEmpty(key))
                    {
                        keyValuePairs.Add(key, value);
                    }
                    var splitted = currentLine.Split(':', 2);
                    key = splitted.FirstOrDefault();
                    value = new List<string>() { splitted.LastOrDefault() };
                }
            }
            if (!string.IsNullOrEmpty(key))
            {
                keyValuePairs.Add(key, value);
            }
            return keyValuePairs;
        }

        internal static Version ParseResultsVersion(List<string> commandResult)
        {
            var version = default(Version);
            if (commandResult.Count == 1)
            {
                string versionString = commandResult[0].Replace("v", "");
                version = new Version(versionString);
            }
            return version;
        }

        internal static IEnumerable<TResultType> ParseResultsTable<TResultType>(List<string> commandResult)
        {
            IEnumerable<TResultType> result;
            var dataAsCsv = ConvertAsciiTableToCsv(commandResult);

            var configuration = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HeaderValidated = null,
                MissingFieldFound = null
            };

            using (var reader = new CsvReader(new StringReader(dataAsCsv), configuration))
            {
                result = reader.GetRecords<TResultType>().ToList();
            }

            return result;
        }

        private static string ConvertAsciiTableToCsv(List<string> output)
        {
            List<ColumnSpec> columns = default;
            var dataStart = 0;
            var csvBuilder = new StringBuilder();

            // Find header
            for (int i = 1; i < output.Count; i++)
            {
                if (!string.IsNullOrWhiteSpace(output[i]) && output[i].Trim().All(c => c == '-'))
                {
                    columns = DetectColumns(output[i - 1]);
                    dataStart = i + 1;
                    break;
                }
            }

            if (columns != default)
            {
                //Add header to csv string
                csvBuilder.AppendLine(string.Join(',', columns.Select(column => column.Name)));

                //Parse table data ignoring any malformed lines
                for (int i = dataStart; i < output.Count; i++)
                {
                    try
                    {
                        List<string> dataFields = ParseDataLine(output[i], columns);
                        csvBuilder.AppendLine(string.Join(',', dataFields));
                    }
                    catch { }
                }
            }

            return csvBuilder.ToString();
        }

        private static List<string> ParseDataLine(string dataLine, List<ColumnSpec> columns)
        {
            var dataFields = new List<string>();
            var stringInfo = new StringInfo(dataLine);
            var consumedLength = 0;
            foreach (var column in columns)
            {
                if (column.IsLastColumn)
                {
                    if (consumedLength < stringInfo.LengthInTextElements)
                    {
                        dataFields.Add(stringInfo.SubstringByTextElements(consumedLength).Trim());
                    }
                    else
                    {
                        dataFields.Add(string.Empty);
                    }
                }
                else
                {
                    dataFields.Add(stringInfo.SubstringByTextElements(consumedLength, column.MaxLength).Trim());
                    consumedLength += column.MaxLength;
                }
            }
            return dataFields;
        }

        private static List<ColumnSpec> DetectColumns(string headerLine)
        {
            List<ColumnSpec> columns = new List<ColumnSpec>();
            var currentStart = 0;
            var currentLength = 0;
            bool columnDetected = true;

            var iterator = StringInfo.GetTextElementEnumerator(headerLine);
            while (iterator.MoveNext())
            {
                var textElement = iterator.GetTextElement();
                if (char.IsWhiteSpace(textElement, 0) && columnDetected)
                {
                    columnDetected = false;
                }
                if ((!char.IsWhiteSpace(textElement, 0) && !columnDetected))
                {
                    var text = new StringInfo(headerLine).SubstringByTextElements(currentStart, currentLength);
                    columns.Add(new ColumnSpec
                    {
                        MaxLength = currentLength,
                        Name = text.Trim(),
                    });
                    currentStart = iterator.ElementIndex;
                    currentLength = 0;
                    columnDetected = true;
                }
                currentLength++;
            }

            columns.Add(new ColumnSpec
            {
                MaxLength = currentLength,
                Name = new StringInfo(headerLine).SubstringByTextElements(currentStart).Trim(),
                IsLastColumn = true
            });
            return columns;
        }
    }
}
