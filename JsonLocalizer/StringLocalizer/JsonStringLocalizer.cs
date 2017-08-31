using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace NetCoreBootstrap.JsonLocalizer.StringLocalizer
{
    public class JsonStringLocalizer : IStringLocalizer
    {
        private readonly ConcurrentDictionary<string, Lazy<JObject>> _resourceObjectCache =
            new ConcurrentDictionary<string, Lazy<JObject>>();

        private readonly string _baseName;
        private readonly string _applicationName;
        private readonly ILogger _logger;
        private readonly string _resourceFileLocation;

        public JsonStringLocalizer(string baseName, string applicationName, ILogger logger)
        {
            if (baseName == null)
            {
                throw new ArgumentNullException(nameof(baseName));
            }
            if (applicationName == null)
            {
                throw new ArgumentNullException(nameof(applicationName));
            }
            if (logger == null)
            {
                throw new ArgumentNullException(nameof(logger));
            }
            
            this._baseName = baseName;
            this._applicationName = applicationName;
            this._logger = logger;

            _resourceFileLocation = LocalizerUtil.TrimPrefix(baseName, applicationName).Trim('.');
            logger.LogTrace($"Resource file location base path: {_resourceFileLocation}");
        }

        public virtual LocalizedString this[string name]
        {
            get
            {
                if (name == null)
                {
                    throw new ArgumentNullException(nameof(name));
                }

                var value = GetLocalizedString(name, CultureInfo.CurrentUICulture);
                return new LocalizedString(name, value ?? name, resourceNotFound: value == null);
            }
        }

        public virtual LocalizedString this[string name, params object[] arguments]
        {
            get
            {
                if (name == null)
                {
                    throw new ArgumentNullException(nameof(name));
                }

                var format = GetLocalizedString(name, CultureInfo.CurrentUICulture);
                var value = string.Format(format ?? name, arguments);
                return new LocalizedString(name, value, resourceNotFound: format == null);
            }
        }

        protected string GetLocalizedString(string name, CultureInfo culture)
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            // Attempt to get resource with the given name from the resource object. if not found, try parent
            // resource object until parent begets himself.
            var currentCulture = CultureInfo.CurrentCulture;
            CultureInfo previousCulture = null;
            do
            {
                var resourceObject = GetResourceObject(currentCulture);
                if (resourceObject == null)
                {
                    _logger.LogInformation($"No resource file found or error occurred for base name {_baseName}, culture {currentCulture} and key '{name}'");
                }
                else
                {
                    JToken value;
                    if (resourceObject.TryGetValue(name, out value))
                    {
                        var localizedString = value.ToString();
                        return localizedString;
                    }
                }

                // Consult parent culture.
                previousCulture = currentCulture;
                currentCulture = currentCulture.Parent;
                _logger.LogTrace($"Switching to parent culture {currentCulture} for key '{name}'.");
            } while (previousCulture != currentCulture);

            _logger.LogInformation($"Could not find key '{name}' in resource file for base name {_baseName} and culture {CultureInfo.CurrentCulture}");
            return null;
        }

        private JObject GetResourceObject(CultureInfo currentCulture)
        {
            if (currentCulture == null)
            {
                throw new ArgumentNullException(nameof(currentCulture));
            }

            _logger.LogTrace($"Attempt to get resource object for culture {currentCulture}");

            var cultureSuffix = currentCulture.Name;
            cultureSuffix = cultureSuffix == "." ? "" : cultureSuffix;

            var lazyJObjectGetter = new Lazy<JObject>(() =>
            {
                // First attempt to find a resource file location that exists.
                string resourcePath = _resourceFileLocation + Path.DirectorySeparatorChar + cultureSuffix + ".json";
                if (File.Exists(resourcePath))
                {
                    _logger.LogInformation($"Resource file location {resourcePath} found");
                }
                else
                {
                    _logger.LogTrace($"Resource file location {resourcePath} does not exist");
                    resourcePath = null;
                }
                
                if (resourcePath == null)
                {
                    _logger.LogTrace($"No resource file found for suffix {cultureSuffix}");
                    return null;
                }

                // Found a resource file path: attempt to parse it into a JObject.
                try
                {
                    var resourceFileStream =
                        new FileStream(resourcePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, FileOptions.Asynchronous | FileOptions.SequentialScan);
                    using (resourceFileStream)
                    {
                        var resourceReader =
                            new JsonTextReader(new StreamReader(resourceFileStream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true));
                        using (resourceReader)
                        {
                            return JObject.Load(resourceReader);
                        }
                    }
                }
                catch (Exception e)
                {
                    _logger.LogError($"Error occurred attempting to read JSON resource file {resourcePath}: {e}");
                    return null;
                }

            }, LazyThreadSafetyMode.ExecutionAndPublication);

            lazyJObjectGetter = _resourceObjectCache.GetOrAdd(cultureSuffix, lazyJObjectGetter);
            var resourceObject = lazyJObjectGetter.Value;
            return resourceObject;
        }

        #region InterfaceNotImplementedMethods
        public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures) => throw new NotImplementedException();
        public IStringLocalizer WithCulture(CultureInfo culture) => throw new NotImplementedException();
        #endregion
    }
}
