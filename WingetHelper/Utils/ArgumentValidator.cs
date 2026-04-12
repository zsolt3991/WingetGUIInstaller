using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace WingetHelper.Utils
{
    internal static class ArgumentValidator
    {
        private static readonly char[] DangerousChars = new[] {'&', '|', '>', '<', ';', '`', '\n', '\r'};

        public static void ValidateMany(IEnumerable<string> arguments)
        {
            if (arguments == null)
            {
                return;
            }

            foreach (var arg in arguments)
            {
                Validate(arg);
            }
        }

        public static void Validate(string argument)
        {
            if (argument == null)
            {
                throw new ArgumentNullException(nameof(argument));
            }

            if (string.IsNullOrWhiteSpace(argument))
            {
                throw new ArgumentException("Argument cannot be empty or whitespace", nameof(argument));
            }

            if (argument.IndexOfAny(DangerousChars) >= 0)
            {
                throw new ArgumentException($"Argument contains disallowed shell characters: {argument}", nameof(argument));
            }

            if (argument.Contains("&&") || argument.Contains("||"))
            {
                throw new ArgumentException($"Argument contains disallowed operator sequence: {argument}", nameof(argument));
            }

            // Disallow ASCII control characters
            if (argument.Any(c => char.IsControl(c)))
            {
                throw new ArgumentException($"Argument contains control characters", nameof(argument));
            }
        }
    }
}
