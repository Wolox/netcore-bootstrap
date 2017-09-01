using System;
using System.Collections.Concurrent;
using System.Linq;
using System.IO;
using System.Reflection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace NetCoreBootstrap.JsonLocalizer.StringLocalizer
{
    public class JsonStringLocalizerFactory : IStringLocalizerFactory
    {
        private static readonly string[] KnownViewExtensions = new[] { ".cshtml" };
        private readonly ConcurrentDictionary<string, JsonStringLocalizer> _LocalizerCache;        
        private readonly IHostingEnvironment _ApplicationEnvironment;
        private readonly ILogger<JsonStringLocalizerFactory> _Logger;
        private string _ResourcesRelativePath;

        public JsonStringLocalizerFactory(IHostingEnvironment ApplicationEnvironment, IOptions<JsonLocalizationOptions> LocalizationOptions,
                                          ILogger<JsonStringLocalizerFactory> Logger)
        {
            if (ApplicationEnvironment == null) throw new ArgumentNullException(nameof(ApplicationEnvironment));
            if (LocalizationOptions == null) throw new ArgumentNullException(nameof(LocalizationOptions));
            if (Logger == null) throw new ArgumentNullException(nameof(Logger));
            this._ApplicationEnvironment = ApplicationEnvironment;
            this._Logger = Logger;
            this._LocalizerCache = new ConcurrentDictionary<string, JsonStringLocalizer>();            
            this._ResourcesRelativePath = LocalizationOptions.Value.ResourcesPath ?? String.Empty;
            if (!String.IsNullOrEmpty(_ResourcesRelativePath))
            {
                _ResourcesRelativePath = _ResourcesRelativePath
                    .Replace(Path.AltDirectorySeparatorChar, '.')
                    .Replace(Path.DirectorySeparatorChar, '.') + ".";
            }
            Logger.LogTrace($"Created {nameof(JsonStringLocalizerFactory)} with:{Environment.NewLine}" +
                $"    (application name: {_ApplicationEnvironment.ApplicationName}{Environment.NewLine}" +
                $"    (resources relative path: {_ResourcesRelativePath})");
        }

        public IStringLocalizer Create(Type ResourceSource)
        {
            if (ResourceSource == null) throw new ArgumentNullException(nameof(ResourceSource));          
            Logger.LogTrace($"Getting localizer for type {ResourceSource}");
            if(String.IsNullOrEmpty(ResourcesRelativePath)) throw new ArgumentNullException(nameof(ResourcesRelativePath));
            var ResourceBaseName = ApplicationEnvironment.ApplicationName + "." + ResourcesRelativePath;
            return LocalizerCache.GetOrAdd(ResourceBaseName, new JsonStringLocalizer(ResourceBaseName, ApplicationEnvironment.ApplicationName, Logger));
        }

        public IStringLocalizer Create(string BaseName, string Location)
        {
            if(String.IsNullOrEmpty(BaseName)) throw new ArgumentNullException(nameof(BaseName));            
            Logger.LogTrace($"Getting localizer for baseName {BaseName} and location {Location}");
            Location = Location ?? ApplicationEnvironment.ApplicationName;
            var ResourceBaseName = Location + "." + ResourcesRelativePath;
            var viewExtension = KnownViewExtensions.FirstOrDefault(extension => ResourceBaseName.EndsWith(extension));
            if (viewExtension != null) ResourceBaseName = ResourceBaseName.Substring(0, ResourceBaseName.Length - viewExtension.Length);
            Logger.LogTrace($"Localizer basename: {ResourceBaseName}");
            return LocalizerCache.GetOrAdd(ResourceBaseName, new JsonStringLocalizer(ResourceBaseName, ApplicationEnvironment.ApplicationName, Logger));
        }

        public ILogger Logger
        {
            get { return _Logger; }
        }

        public string ResourcesRelativePath
        {
            get { return _ResourcesRelativePath; }
        }

        public ConcurrentDictionary<string, JsonStringLocalizer> LocalizerCache
        {
            get { return _LocalizerCache; }
        }

        public IHostingEnvironment ApplicationEnvironment
        {
            get { return _ApplicationEnvironment; }
        }
    }
}
