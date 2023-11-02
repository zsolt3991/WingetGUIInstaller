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
            // Find header
            var separatorIndex = output.ToList().FindLastIndex(line =>
                !string.IsNullOrWhiteSpace(line) && line.Trim().All(c => c == '-'));

            // Detect column names from line above separator
            List<ColumnSpec> columns = DetectColumns(output.Skip(separatorIndex - 1).FirstOrDefault());

            if (columns != default)
            {
                var csvBuilder = new StringBuilder();

                //Add header to csv string
                csvBuilder.AppendLine(string.Join(',', columns.Select(column => column.Name)));

                //Parse table data ignoring any malformed lines after the separator
                foreach (var dataLine in output.Skip(separatorIndex + 1))
                {
                    try
                    {
                        csvBuilder.AppendLine(string.Join(',', ParseDataLine(dataLine, columns)));
                    }
                    catch
                    {
                        //Skip malformed lines
                    }
                }

                return csvBuilder.ToString();
            }

            return string.Empty;
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
