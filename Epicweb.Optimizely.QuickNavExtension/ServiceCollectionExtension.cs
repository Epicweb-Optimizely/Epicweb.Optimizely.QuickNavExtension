using Microsoft.Extensions.DependencyInjection;

namespace Epicweb.Optimizely.QuickNavExtension
{
    public static class ServiceCollectionExtension
    {
        /// <summary>
        /// Add a link to Optimizely Quick Navigation on public site, when logged in. 
        /// </summary>
        /// <param name="services"></param>
        /// <param name="nameOrLocalizationPath">Title or a localization path eg: "/shell/cms/menu/admin"</param>
        /// <param name="urlOrJavascript">anything that can be inside a href</param>
        /// <param name="role">if not null evaluate User.IsInRole(role)</param>
        /// <returns></returns>
        public static IServiceCollection AddQuickNav(this IServiceCollection services, string nameOrLocalizationPath, string urlOrJavascript = null, string role = null)
        {
            QuickNavigator.Rules.Add(new QuickNavRule() { TitleOrLocalisationString = nameOrLocalizationPath, UrlOrJavascript = urlOrJavascript, Role = role });
            return services;
        }
    }
}
