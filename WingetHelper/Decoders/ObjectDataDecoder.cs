using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;

namespace WingetHelper.Decoders
{
    internal static class ObjectDataDecoder
    {
        private const string LineSplitRegex = @"^\s*(?<key>[a-zA-z|\d].+?)(\:{1}){1}\s*(?<value>.*?)$";
        private const int IndentSize = 2;

        internal static TObject DeserializeObject<TObject>(IEnumerable<string> dataLines) where TObject : class
        {
            return DeserializeObject(typeof(TObject), dataLines, 0, out var _) as TObject;
        }

        private static object DeserializeObject(Type targetType, IEnumerable<string> dataLines, int nestingLevel, out int consumedLines)
        {
            var retVal = Activator.CreateInstance(targetType);
            var expectedIndentation = IndentSize * nestingLevel;
            var i = 0;
            var regex = new Regex(LineSplitRegex);

            for (i = 0; i < dataLines.Count(); i++)
            {
                string line = dataLines.ElementAt(i);
                var lineIndent = line.Length - line.TrimStart().Length;
                var match = regex.Match(line);

                // Reached a line containing a property
                if (match.Success)
                {
                    var currentKeyword = match.Groups["key"].Value;
                    var currentValueText = match.Groups["value"].Value;

                    // Reached a line which contains a differently indented property
                    if (lineIndent != expectedIndentation)
                    {
                        break;
                    }

                    var currentProperty = GetPropertyForName(targetType, currentKeyword);
                    if (currentProperty != default)
                    {
                        if (currentProperty.PropertyType == typeof(string))
                        {
                            var content = dataLines.Skip(i + 1).ToList();
                            // Append the current line without the property identifier
                            content.Insert(0, currentValueText);
                            currentProperty.SetValue(retVal, DeserializeString(
                                content, nestingLevel + 1, out var internalConsumed));
                            i += internalConsumed;
                        }
                        else
                        {
                            currentProperty.SetValue(retVal, DeserializeObject(
                                currentProperty.PropertyType, dataLines.Skip(i + 1).ToList(),
                                nestingLevel + 1, out var internalConsumed));
                            i += internalConsumed;
                        }
                    }
                }
            }

            consumedLines = i;
            return retVal;
        }

        private static object DeserializeString(IEnumerable<string> dataLines, int nestingLevel, out int consumedLines)
        {
            var accumulator = new List<string>();
            var expectedIndentation = IndentSize * nestingLevel;
            var i = 0;
            accumulator.Add(dataLines.ElementAt(0));
            for (i = 1; i < dataLines.Count(); i++)
            {
                string line = dataLines.ElementAt(i);
                var lineIndent = line.Length - line.TrimStart().Length;

                // Reached the end of indented value
                if (lineIndent < expectedIndentation)
                {
                    break;
                }

                accumulator.Add(line);
            }
            consumedLines = i - 1;
            return string.Join(Environment.NewLine, accumulator);
        }

        private static PropertyInfo GetPropertyForName(Type type, string name)
        {
            foreach (var prop in type.GetProperties())
            {
                if (prop.Name == name)
                {
                    return prop;
                }

                var dataMemberAttribute = prop.GetCustomAttribute<DataMemberAttribute>();
                if (dataMemberAttribute != default && dataMemberAttribute.Name == name)
                {
                    return prop;
                }
            }
            return default;
        }
    }
}
