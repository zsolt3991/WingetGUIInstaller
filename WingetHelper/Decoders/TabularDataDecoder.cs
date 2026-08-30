using CsvHelper;
using CsvHelper.Configuration;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using WingetHelper.Models;

namespace WingetHelper.Decoders
{
    internal static class TabularDataDecoder
    {
        internal static IEnumerable<TResultType> ParseResultsTable<TResultType>(IEnumerable<string> commandResult)
        {
            var dataAsCsv = ConvertAsciiTableToCsv(commandResult);
            var configuration = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HeaderValidated = null,
                MissingFieldFound = null
            };

            using (var reader = new CsvReader(new StringReader(dataAsCsv), configuration))
            {
                return reader.GetRecords<TResultType>().ToList();
            }
        }

        private static string ConvertAsciiTableToCsv(IEnumerable<string> output)
        {
            var outputLines = output.ToList();

            // Find header
            var separatorIndex = outputLines.FindLastIndex(line =>
                !string.IsNullOrWhiteSpace(line) && line.Trim().All(c => c == '-'));

            if (separatorIndex <= 0)
            {
                return string.Empty;
            }

            // Detect column names from line above separator
            List<ColumnSpec> columns = DetectColumns(outputLines[separatorIndex - 1]);

            if (columns != default)
            {
                var csvBuilder = new StringBuilder();

                //Add header to csv string
                csvBuilder.AppendLine(string.Join(',', columns.Select(column => column.Name)));

                //Parse table data ignoring any malformed lines after the separator
                var tableStarted = false;
                foreach (var dataLine in outputLines.Skip(separatorIndex + 1))
                {
                    if (string.IsNullOrWhiteSpace(dataLine))
                    {
                        if (tableStarted)
                        {
                            break;
                        }

                        continue;
                    }

                    if (!IsLikelyDataRow(dataLine, columns))
                    {
                        if (tableStarted)
                        {
                            break;
                        }

                        continue;
                    }

                    tableStarted = true;
                    try
                    {
                        csvBuilder.AppendLine(string.Join(',', ParseDataLine(dataLine, columns)));
                    }
                    catch
                    {
                        break;
                    }
                }

                return csvBuilder.ToString();
            }

            return string.Empty;
        }

        private static bool IsLikelyDataRow(string dataLine, List<ColumnSpec> columns)
        {
            var minimumWidth = columns
                .Where(column => !column.IsLastColumn)
                .Sum(column => column.MaxLength);

            return dataLine.Length >= minimumWidth;
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

            var headerStringInfo = new StringInfo(headerLine);
            var iterator = StringInfo.GetTextElementEnumerator(headerLine);

            while (iterator.MoveNext())
            {
                var textElement = iterator.GetTextElement();

                // Reached whitespace characters in column header
                if (char.IsWhiteSpace(textElement, 0) && columnDetected)
                {
                    columnDetected = false;
                }

                // Reached a non whitespace character which is the beginning of the next header
                if ((!char.IsWhiteSpace(textElement, 0) && !columnDetected))
                {
                    var text = headerStringInfo.SubstringByTextElements(currentStart, currentLength);
                    columns.Add(new ColumnSpec(text.Trim(), currentLength));
                    currentStart = iterator.ElementIndex;
                    currentLength = 0;
                    columnDetected = true;
                }
                currentLength++;
            }

            // Add the remaining data as the last column
            columns.Add(new ColumnSpec(headerStringInfo.SubstringByTextElements(currentStart).Trim(), currentLength, true));
            return columns;
        }
    }
}
