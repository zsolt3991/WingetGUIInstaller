using CsvHelper;
using CsvHelper.Configuration;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using WingetHelper.Models;
using YamlDotNet.Serialization;

namespace WingetHelper.Utils
{
    internal static class ResponseDecoder
    {
        private const string LineSplitRegex = @"^\s*(?<key>.*?)(\:{1}){1}\s*(?<value>.*?)$";
        private const string FoundResultRegex = @"^Found\s*?(?<packageName>.*)\s*?\[(?<packageId>.*)\]$";

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

        internal static WingetPackageDetails ParseDetailsResponse(List<string> output)
        {
            var result = default(WingetPackageDetails);
            var foundIndex = -1;
            var regex = new Regex(FoundResultRegex);

            var foundPackageName = string.Empty;
            var foundPackageId = string.Empty;

            output = output.Where(line => !string.IsNullOrEmpty(line)).ToList();

            for (int i = 1; i < output.Count; i++)
            {
                var currentLine = output[i];
                var match = regex.Match(currentLine);

                if (match.Success)
                {
                    foundIndex = i;
                    foundPackageName = match.Groups["packageName"].Value.Trim();
                    foundPackageId = match.Groups["packageId"].Value.Trim();
                    break;
                }
                else
                {
                    if (currentLine.StartsWith("no", StringComparison.InvariantCultureIgnoreCase))
                    {
                        break;
                    }
                }
            }
            if (foundIndex != -1)
            {
                result = DeserializeDetails<WingetPackageDetails>(output.Skip(foundIndex + 1).ToList());
                result.Name = foundPackageName;
                result.Id = foundPackageId;
            }
            return result;
        }

        private static TObject DeserializeDetails<TObject>(List<string> list) where TObject : class
        {
            return DeserializeDetails(typeof(TObject), list, 0, out var _) as TObject;
        }

        private static object DeserializeDetails(Type targetType, List<string> list, int nestingLevel, out int consumedLines)
        {
            var retVal = Activator.CreateInstance(targetType);
            List<string> keywords = GetPropertyNames(targetType);

            var indentLength = 2 * nestingLevel;
            var fieldValue = new List<string>();
            var currentProperty = default(PropertyInfo);
            var i = 0;
            var regex = new Regex(LineSplitRegex);

            for (i = 0; i < list.Count; i++)
            {
                string line = list[i];

                var lineIndent = line.Length - line.TrimStart().Length;

                if (lineIndent != indentLength)
                {
                    break;
                }

                var match = regex.Match(line);

                if (match.Success)
                {
                    var currentKeyword = match.Groups["key"].Value;
                    var currentValueText = match.Groups["value"].Value;

                    if (currentProperty != default)
                    {
                        currentProperty.SetValue(retVal, string.Join(Environment.NewLine, fieldValue));
                        currentProperty = GetPropertyForName(targetType, currentKeyword);
                        if (currentProperty != default)
                        {
                            if (currentProperty.PropertyType == typeof(string))
                            {
                                fieldValue = new List<string>();
                                if (!string.IsNullOrEmpty(currentValueText))
                                {
                                    fieldValue.Add(currentValueText);
                                }
                            }
                            else
                            {
                                currentProperty.SetValue(retVal,
                                    DeserializeDetails(currentProperty.PropertyType, list.Skip(i + 1).ToList(),
                                    nestingLevel + 1, out var internalConsumed));
                                i += internalConsumed;
                                currentProperty = default;
                            }
                        }
                    }
                    else
                    {
                        currentProperty = GetPropertyForName(targetType, currentKeyword);

                        if (currentProperty != default)
                        {
                            if (currentProperty.PropertyType == typeof(string))
                            {
                                fieldValue = new List<string>();
                                if (!string.IsNullOrEmpty(currentValueText))
                                {
                                    fieldValue.Add(currentValueText);
                                }
                            }
                            else
                            {
                                currentProperty.SetValue(retVal,
                                    DeserializeDetails(currentProperty.PropertyType, list.Skip(i + 1).ToList(),
                                    nestingLevel + 1, out var internalConsumed));
                                i += internalConsumed;
                                currentProperty = default;
                            }
                        }
                    }
                }
                else
                {
                    if (!string.IsNullOrWhiteSpace(line))
                    {
                        fieldValue.Add(line);
                    }
                }
            }

            if (currentProperty != default && fieldValue.Count > 0)
            {
                currentProperty.SetValue(retVal, string.Join(Environment.NewLine, fieldValue));
            }

            consumedLines = i;
            return retVal;
        }

        private static List<string> GetPropertyNames(Type type)
        {
            var result = new List<string>();
            foreach (var prop in type.GetProperties())
            {
                var nameAttribute = prop.GetCustomAttribute<DeserializerNameAttribute>();
                if (nameAttribute != default)
                {
                    result.Add(nameAttribute.Name);
                }
                else
                {
                    result.Add(prop.Name);
                }
            }
            return result;
        }

        private static PropertyInfo GetPropertyForName(Type type, string name)
        {
            foreach (var prop in type.GetProperties())
            {
                var nameAttribute = prop.GetCustomAttribute<DeserializerNameAttribute>();
                if (nameAttribute != default)
                {
                    if (nameAttribute.Name == name)
                    {
                        return prop;
                    }
                }
                else
                {
                    if (prop.Name == name)
                    {
                        return prop;
                    }
                }
            }
            return default;
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
