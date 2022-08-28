using EPiServer;
using EPiServer.Core;
using EPiServer.Framework.Localization;
using EPiServer.ServiceLocation;
using EPiServer.Web;
using EPiServer.Web.Routing;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using EditUrlResolver = EPiServer.Cms.Shell.Service.Internal.EditUrlResolver;

namespace Epicweb.Optimizely.QuickNavExtension
{
    public static class QuickNavigator
    {
        public static List<QuickNavRule> Rules = new List<QuickNavRule>();
        public const string Admin = "admin";
        public const string ContentType = "contenttype";
        public const string FIND = "find";
        public const string LogOut = "logout";
    }

    public class QuickNavRule
    {
        public string TitleOrLocalisationString { get; set; }
        public string UrlOrJavascript { get; set; }
        public string Role { get; set; }
    }

    [ServiceConfiguration(typeof(IQuickNavigatorItemProvider))]
    public class QuickNavigatorItemProvider : IQuickNavigatorItemProvider
    {
        private readonly IContentLoader _contentLoader;
        private readonly EditUrlResolver _editUrlResolver;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor httpContextAccessor;
        public QuickNavigatorItemProvider(IContentLoader contentLoader, EditUrlResolver editUrlResolver, IConfiguration config, IHttpContextAccessor context)
        {
            httpContextAccessor = context;
            _contentLoader = contentLoader;
            _editUrlResolver = editUrlResolver;
            _configuration = config;

        }

        public IDictionary<string, QuickNavigatorMenuItem> GetMenuItems(ContentReference currentContent)
        {
            if (!QuickNavigator.Rules.Any())
            {
                var appsetting = _configuration.GetSection("Epicweb.QuickNav")?.Value;

                string[] menuitems = new string[] { "admin", "logout" };
                if (!String.IsNullOrEmpty(appsetting))
                {
                    menuitems = appsetting.Split(",".ToCharArray());
                }

                foreach (var item in menuitems)
                {
                    if (!String.IsNullOrEmpty(item))
                    {
                        var rule = CreateRule(item);
                        if (rule != null)
                            QuickNavigator.Rules.Add(rule);
                    }
                }
            }

            var dictionary = new Dictionary<string, QuickNavigatorMenuItem>();

            foreach (var rule in QuickNavigator.Rules)
            {
                if (!String.IsNullOrEmpty(rule?.TitleOrLocalisationString))
                {
                    var menu = EvaluateRule(rule, currentContent);
                    if (menu != null)
                        dictionary.Add(rule.TitleOrLocalisationString, menu);
                }
            }

            return dictionary;
        }

        private QuickNavRule CreateRule(string item)
        {
            QuickNavRule rule = new QuickNavRule();
            rule.TitleOrLocalisationString = item;
            string[] arr = new string[0];
            if (item.IndexOf("|") > 0)
            {
                arr = item.Split('|');
                if (arr.Length > 1 && !string.IsNullOrEmpty(arr[2]))
                {
                        rule.Role = arr[2];
                }
                rule.TitleOrLocalisationString = arr[0];
                rule.UrlOrJavascript = arr[1];
            }
            return rule;
        }

        /// <summary>
        /// Evalutates the "Rule" at run time
        /// </summary>
        /// <param name="rule"></param>
        /// <param name="currentContent"></param>
        /// <returns></returns>
        private QuickNavigatorMenuItem EvaluateRule(QuickNavRule rule, ContentReference currentContent)
        {

            string keyWord = rule.TitleOrLocalisationString;

            if (rule.Role != null && (!httpContextAccessor.HttpContext.User.IsInRole(rule.Role)))
            {
                    return null;
            }

            if (keyWord.ToLower() == QuickNavigator.FIND)
            {
                var findurl = GetEditUrl() + UIPathResolver.Instance.CombineWithUI("../find/");
                var findTitle = LocalizationService.Current.GetString("/addon/quicknav/find", "Search & Navigation");
                return new QuickNavigatorMenuItem(findTitle, findurl, null, "true", null);
            }

            if (keyWord.ToLower() == QuickNavigator.Admin)
            {

                if (!httpContextAccessor.HttpContext.User.IsInRole("WebAdmins"))
                    return null;

                var editUrl = GetAdminUrl(currentContent);

                return new QuickNavigatorMenuItem("/shell/cms/menu/admin", editUrl, null, "true", null);
            }

            if (keyWord.ToLower() == QuickNavigator.ContentType)
            {
           
                if (!httpContextAccessor.HttpContext.User.IsInRole("WebAdmins"))
                    return null;

                var editUrl = GetAdminUrl(currentContent);

                PageData pd = null;

                if (this._contentLoader.TryGet<PageData>(currentContent, out pd))
                {
                    editUrl = editUrl + "#/ContentTypes/edit-content-type/" + pd.ContentTypeID;
                    var n = LocalizationService.Current.GetString("/addon/quicknav/pagetype", "Admin pagetype") + " " + pd.PageTypeName;
                    return new QuickNavigatorMenuItem(n, editUrl, null, "true", null);
                }
                return null;
            }

            if (keyWord.ToLower() == QuickNavigator.LogOut)
            {
                var urlBuilder = new UrlBuilder(Constants.LogoutUrl);

                if (this.IsPageData(currentContent))
                {

                    string url = UrlResolver.Current.GetUrl(currentContent);
                    urlBuilder.QueryCollection.Add("ReturnUrl", url);
                }

                if (httpContextAccessor.HttpContext != null)
                {
                    IAntiforgery service1 = httpContextAccessor.HttpContext.RequestServices.GetService<IAntiforgery>();
                    IOptions<AntiforgeryOptions> service2 = httpContextAccessor.HttpContext.RequestServices.GetService<IOptions<AntiforgeryOptions>>();
                    if (service1 != null && service2?.Value != null)
                        urlBuilder.QueryCollection.Add(service2.Value.HeaderName, service1.GetAndStoreTokens(httpContextAccessor.HttpContext).RequestToken);
                }

                return new QuickNavigatorMenuItem("/shell/cms/menu/logout", urlBuilder.ToString(), null, "true", null);
            }

            if (!string.IsNullOrEmpty(rule.UrlOrJavascript))
            {
                return new QuickNavigatorMenuItem(LocalizationService.Current.GetString(rule.TitleOrLocalisationString, rule.TitleOrLocalisationString), rule.UrlOrJavascript, null, "true", null);
            }

            //oh no... 
            return new QuickNavigatorMenuItem(LocalizationService.Current.GetString(rule.TitleOrLocalisationString, rule.TitleOrLocalisationString), "javascript:alert('Wrong config in Appsetting QuickNav')", null, "true", null);
        }

        private string GetAdminUrl(ContentReference currentContent)
        {
            var url = GetEditUrl() + EPiServer.Editor.PageEditing.GetEditUrl(currentContent);
            return url.ReplaceAfter("CMS", "EPiServer.Cms.UI.Admin/default");
        }

        private string GetEditUrl()
        {
            Url editViewUrl = _editUrlResolver.GetFullUrlToEditView(new SiteDefinition());
            return editViewUrl?.Uri.ToString().Replace(editViewUrl.Path, "");//Just want the HOST to *EDIT*
        }

        private bool IsPageData(ContentReference currentContentLink)
        {
            PageData pd = null;
            if (this._contentLoader.TryGet<PageData>(currentContentLink, out pd))
                return true;
            else
                return false;
        }
        public int SortOrder
        {
            get
            {
                return 150;
            }
        }
    }
}
