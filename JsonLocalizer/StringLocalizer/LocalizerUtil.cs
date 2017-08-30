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
    }
}
