using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
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
        private readonly ConcurrentDictionary<string, Lazy<JObject>> _ResourceObjectCache;
        private readonly string _BaseName;
        private readonly string _ApplicationName;
        private readonly ILogger _Logger;
        private readonly string _ResourceFileLocation;
        private readonly char _JsonSplitter;

        public JsonStringLocalizer(string BaseName, string ApplicationName, ILogger Logger)
        {
            if (String.IsNullOrEmpty(BaseName)) throw new ArgumentNullException(nameof(BaseName));
            if (String.IsNullOrEmpty(ApplicationName)) throw new ArgumentNullException(nameof(ApplicationName));
            if (Logger == null) throw new ArgumentNullException(nameof(Logger));
            this._ResourceObjectCache = new ConcurrentDictionary<string, Lazy<JObject>>();
            this._BaseName = BaseName;
            this._ApplicationName = ApplicationName;
            this._Logger = Logger;
            this._JsonSplitter = ':';
            _ResourceFileLocation = LocalizerUtil.TrimPrefix(BaseName, ApplicationName).Trim('.');
            _Logger.LogTrace($"Resource file location base path: {_ResourceFileLocation}");
        }

        public virtual LocalizedString this[string Name]
        {
            get
            {
                if (String.IsNullOrEmpty(Name)) throw new ArgumentNullException(nameof(Name));
                var Value = GetLocalizedString(Name, CultureInfo.CurrentUICulture);
                return new LocalizedString(Name, Value ?? Name, resourceNotFound: Value == null);
            }
        }

        public virtual LocalizedString this[string Name, params object[] Arguments]
        {
            get
            {
                if (String.IsNullOrEmpty(Name)) throw new ArgumentNullException(nameof(Name));
                var Format = GetLocalizedString(Name, CultureInfo.CurrentUICulture);
                var Value = string.Format(Format ?? Name, Arguments);
                return new LocalizedString(Name, Value, resourceNotFound: Format == null);
            }
        }

        protected string GetLocalizedString(string Name, CultureInfo Culture)
        {
            if (String.IsNullOrEmpty(Name)) throw new ArgumentNullException(nameof(Name));
            var CurrentCulture = CultureInfo.CurrentCulture;
            CultureInfo PreviousCulture = null;
            do
            {
                var ResourceObject = GetResourceObject(CurrentCulture);
                if (ResourceObject == null) Logger.LogInformation($"No resource file found or error occurred for base name {BaseName}, culture {CurrentCulture} and key '{Name}'");
                else
                {
                    JToken Value = TryGetValue(ResourceObject, Name);
                    if(Value != null) return Value.ToString();
                }
                PreviousCulture = CurrentCulture;
                CurrentCulture = CurrentCulture.Parent;
                Logger.LogTrace($"Switching to parent culture {CurrentCulture} for key '{Name}'.");
            } while (PreviousCulture != CurrentCulture);
            Logger.LogInformation($"Could not find key '{Name}' in resource file for base name {BaseName} and culture {CultureInfo.CurrentCulture}");
            return null;
        }

        private JObject GetResourceObject(CultureInfo CurrentCulture)
        {
            if (CurrentCulture == null) throw new ArgumentNullException(nameof(CurrentCulture));
            Logger.LogTrace($"Attempt to get resource object for culture {CurrentCulture}");
            var CultureSuffix = CurrentCulture.Name;
            CultureSuffix = CultureSuffix == "." ? "" : CultureSuffix;

            var LazyJObjectGetter = new Lazy<JObject>(() =>
            {
                // First attempt to find a resource file location that exists.
                string ResourcePath = ResourceFileLocation + Path.DirectorySeparatorChar + CultureSuffix + ".json";
                if(!ResourceExists(ResourcePath, CultureSuffix)) return null;
                // Found a resource file path: attempt to parse it into a JObject.
                try
                {
                    var ResourceFileStream = new FileStream(ResourcePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, FileOptions.Asynchronous | FileOptions.SequentialScan);
                    using (ResourceFileStream)
                    {
                        var ResourceReader = new JsonTextReader(new StreamReader(ResourceFileStream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true));
                        using (ResourceReader)
                        {
                            return JObject.Load(ResourceReader);
                        }
                    }
                }
                catch (Exception e)
                {
                    Logger.LogError($"Error occurred attempting to read JSON resource file {ResourcePath}: {e}");
                    return null;
                }

            }, LazyThreadSafetyMode.ExecutionAndPublication);
            return ResourceObjectCache.GetOrAdd(CultureSuffix, LazyJObjectGetter).Value;
        }

        private JToken TryGetValue(JObject Resource, string Name)
        {
            JToken JTokenValue = null;
            string[] Keys = Name.Split(JsonSplitter);
            JTokenValue = Resource[Keys[0]];            
            for(var i = 1; i < Keys.Length; i++)
            {
                JTokenValue = JTokenValue[Keys[i]];
            }
            return JTokenValue;
        }

        private bool ResourceExists(string Path, string CultureSuffix)
        {
            if (File.Exists(Path))
            {
                Logger.LogInformation($"Resource file location {Path} found");
                return true;
            }
            Logger.LogTrace($"Resource file location {Path} does not exist");
            Logger.LogTrace($"No resource file found for suffix {CultureSuffix}");
            return false;
        }

        public string ResourceFileLocation
        {
            get { return _ResourceFileLocation; }
        }

        public string BaseName
        {
            get { return _BaseName; }
        }

        public char JsonSplitter 
        {
            get { return _JsonSplitter; }
        }

        public ILogger Logger
        {
            get { return _Logger; }
        }

        public ConcurrentDictionary<string, Lazy<JObject>> ResourceObjectCache
        {
            get { return _ResourceObjectCache; }
        }

        #region InterfaceNotImplementedMethods
        public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures) => throw new NotImplementedException();
        public IStringLocalizer WithCulture(CultureInfo culture) => throw new NotImplementedException();
        #endregion
    }
}
