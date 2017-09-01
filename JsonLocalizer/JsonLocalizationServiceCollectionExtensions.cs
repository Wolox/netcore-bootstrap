using System;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Localization;

namespace Microsoft.Extensions.DependencyInjection
{
    using global::NetCoreBootstrap.JsonLocalizer;
    using global::NetCoreBootstrap.JsonLocalizer.StringLocalizer;

    public static class JsonLocalizationServiceCollectionExtensions
    {
        public static IServiceCollection AddJsonLocalization(this IServiceCollection Services)
        {
            if(Services == null) throw new ArgumentNullException(nameof(Services));
            return AddJsonLocalization(Services, SetupAction: null);
        }

        public static IServiceCollection AddJsonLocalization(this IServiceCollection Services, Action<JsonLocalizationOptions> SetupAction)
        {
            if (Services == null) throw new ArgumentNullException(nameof(Services));
            Services.TryAdd(new ServiceDescriptor(typeof(IStringLocalizerFactory), 
                                                  typeof(JsonStringLocalizerFactory), 
                                                  ServiceLifetime.Singleton));
            Services.TryAdd(new ServiceDescriptor(typeof(IStringLocalizer), 
                                                  typeof(JsonStringLocalizer), 
                                                  ServiceLifetime.Singleton));
            if (SetupAction != null) Services.Configure(SetupAction);
            return Services;
        }
    }
}
