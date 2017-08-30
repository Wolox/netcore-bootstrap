using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace NetCoreBootstrap.JsonLocalizer.StringLocalizer
{
    public static class LocalizerUtil
    {
        public static string TrimPrefix(string name, string prefix)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));
            if (prefix == null) throw new ArgumentNullException(nameof(prefix));

            if (name.StartsWith(prefix, StringComparison.Ordinal))
            {
                return name.Substring(prefix.Length);
            }
            return name;
        }

        public static IEnumerable<string> ExpandPaths(string name, string baseName)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));
            if (baseName == null) throw new ArgumentNullException(nameof(baseName));

            return ExpandPathIterator(name, baseName);
        }

        private static IEnumerable<string> ExpandPathIterator(string name, string baseName)
        {
            StringBuilder expansion = new StringBuilder();

            // Start replacing periods, starting at the beginning.
            var components = name.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
            for (var i = 0; i < components.Length; i++)
            {
                for (var j = 0; j < components.Length; j++)
                {
                    expansion.Append(components[j]).Append(j < i ? Path.DirectorySeparatorChar : '.');
                }
                // Remove trailing period.
                yield return expansion.Remove(expansion.Length - 1, 1).ToString();
                expansion.Clear();
            }

            // Do the same with the name where baseName prefix is removed.
            var nameWithoutPrefix = TrimPrefix(name, baseName);
            if (nameWithoutPrefix != string.Empty && nameWithoutPrefix != name)
            {
                nameWithoutPrefix = nameWithoutPrefix.Substring(1);
                var componentsWithoutPrefix = nameWithoutPrefix.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
                for (var i = 0; i < componentsWithoutPrefix.Length; i++)
                {
                    for (var j = 0; j < componentsWithoutPrefix.Length; j++)
                    {
                        expansion.Append(componentsWithoutPrefix[j]).Append(j < i ? Path.DirectorySeparatorChar : '.');
                    }
                    // Remove trailing period.
                    yield return expansion.Remove(expansion.Length - 1, 1).ToString();
                    expansion.Clear();
                }
            }
        }
    }
}
