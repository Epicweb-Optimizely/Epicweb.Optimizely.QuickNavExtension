using Microsoft.Extensions.DependencyInjection;

namespace Epicweb.Optimizely.QuickNavExtension
{
    public static class ServiceCollectionExtension
    {
        public static IServiceCollection AddQuickNav(this IServiceCollection services, string nameOrLocalizationPath, string urlOrJavascript = null, string role = null)
        {
            QuickNavigator.Rules.Add(new QuickNavRule() { TitleOrLocalisationString = nameOrLocalizationPath, UrlOrJavascript = urlOrJavascript, Role = role });
            return services;
        }
    }
}
