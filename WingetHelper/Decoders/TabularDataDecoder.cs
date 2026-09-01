using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using WingetHelper.Models;

namespace WingetHelper.Decoders
{
    internal static class TabularDataDecoder
    {
        internal static IEnumerable<TResultType> ParseResultsTable<TResultType>(IEnumerable<string> commandResult)
        {
            var outputLines = commandResult.ToList();
            var separatorIndex = outputLines.FindLastIndex(line =>
                !string.IsNullOrWhiteSpace(line) && line.Trim().All(c => c == '-'));

            if (separatorIndex <= 0)
            {
                return Enumerable.Empty<TResultType>();
            }

            List<ColumnSpec> columns = DetectColumns(outputLines[separatorIndex - 1]);
            var properties = typeof(TResultType)
                .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Where(property => property.CanWrite)
                .ToDictionary(property => property.Name, StringComparer.OrdinalIgnoreCase);
            var results = new List<TResultType>();
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
                var values = ParseDataLine(dataLine, columns);
                results.Add(CreateResult<TResultType>(columns, values, properties));
            }

            return results;
        }

        private static TResultType CreateResult<TResultType>(
            IReadOnlyList<ColumnSpec> columns,
            IReadOnlyList<string> values,
            IReadOnlyDictionary<string, PropertyInfo> properties)
        {
            var result = (TResultType)Activator.CreateInstance(typeof(TResultType));

            for (var index = 0; index < columns.Count; index++)
            {
                if (properties.TryGetValue(columns[index].Name, out var property))
                {
                    property.SetValue(result, values[index]);
                }
            }

            return result;
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
            var consumedWidth = 0;
            foreach (var column in columns)
            {
                if (column.IsLastColumn)
                {
                    dataFields.Add(SubstringByDisplayWidth(dataLine, consumedWidth).Trim());
                }
                else
                {
                    dataFields.Add(SubstringByDisplayWidth(dataLine, consumedWidth, column.MaxLength).Trim());
                    consumedWidth += column.MaxLength;
                }
            }
            return dataFields;
        }

        private static List<ColumnSpec> DetectColumns(string headerLine)
        {
            List<ColumnSpec> columns = new List<ColumnSpec>();
            var currentStart = 0;
            var currentTextElementLength = 0;
            var currentDisplayWidth = 0;
            var textElementIndex = 0;
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
                    var text = headerStringInfo.SubstringByTextElements(currentStart, currentTextElementLength);
                    columns.Add(new ColumnSpec(text.Trim(), currentDisplayWidth));
                    currentStart = textElementIndex;
                    currentTextElementLength = 0;
                    currentDisplayWidth = 0;
                    columnDetected = true;
                }
                currentTextElementLength++;
                currentDisplayWidth += GetDisplayWidth(textElement);
                textElementIndex++;
            }

            // Add the remaining data as the last column
            columns.Add(new ColumnSpec(headerStringInfo.SubstringByTextElements(currentStart).Trim(), currentDisplayWidth, true));
            return columns;
        }

        private static string SubstringByDisplayWidth(string value, int startWidth, int maximumWidth = int.MaxValue)
        {
            var textElements = StringInfo.GetTextElementEnumerator(value);
            var displayWidth = 0;
            var result = new StringBuilder();

            while (textElements.MoveNext())
            {
                var textElement = textElements.GetTextElement();
                var textElementWidth = GetDisplayWidth(textElement);

                if (displayWidth >= startWidth
                    && (maximumWidth == int.MaxValue || displayWidth + textElementWidth <= startWidth + maximumWidth))
                {
                    result.Append(textElement);
                }

                displayWidth += textElementWidth;
            }

            return result.ToString();
        }

        private static int GetDisplayWidth(string textElement)
        {
            var codePoint = char.ConvertToUtf32(textElement, 0);
            return IsDoubleWidth(codePoint) ? 2 : 1;
        }

        private static bool IsDoubleWidth(int codePoint)
        {
            // Hangul Jamo.
            return (codePoint >= 0x1100 && codePoint <= 0x115F)
                // Angle brackets.
                || codePoint == 0x2329
                || codePoint == 0x232A
                // CJK radicals, ideographs, and Yi syllables.
                || (codePoint >= 0x2E80 && codePoint <= 0xA4CF)
                // Hangul syllables.
                || (codePoint >= 0xAC00 && codePoint <= 0xD7A3)
                // CJK compatibility ideographs.
                || (codePoint >= 0xF900 && codePoint <= 0xFAFF)
                // Vertical, compatibility, and small form variants.
                || (codePoint >= 0xFE10 && codePoint <= 0xFE6F)
                // Full-width ASCII variants.
                || (codePoint >= 0xFF00 && codePoint <= 0xFF60)
                // Full-width symbol variants.
                || (codePoint >= 0xFFE0 && codePoint <= 0xFFE6)
                // Emoji and pictographs.
                || (codePoint >= 0x1F300 && codePoint <= 0x1FAFF);
        }
    }
}
