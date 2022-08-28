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
using EditUrlResolver = EPiServer.Cms.Shell.Service.Internal.EditUrlResolver;

namespace Epicweb.Optimizely.QuickNavExtension
{
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
            var appsetting = _configuration.GetSection("QuickNav")?.Value;

            string[] menuitems = new string[] { "admin", "logout" };
            if (!String.IsNullOrEmpty(appsetting))
            {
                menuitems = appsetting.Split(",".ToCharArray());
            }

            var dictionary = new Dictionary<string, QuickNavigatorMenuItem>();

            foreach (var item in menuitems)
            {
                if (!String.IsNullOrEmpty(item))
                {
                    var menu = DoMagic(item, currentContent);
                    if (menu != null)
                        dictionary.Add(item, menu);
                }
            }



            return dictionary;
        }
        private QuickNavigatorMenuItem DoMagic(string item, ContentReference currentContent)
        {

            string keyWord = item;
            string[] arr = new string[0];
            if (item.IndexOf("|") > 0)
            {
                arr = item.Split('|');
                if (arr.Length > 2)
                {
                    if (!string.IsNullOrEmpty(arr[2]))
                    {
                        if (!httpContextAccessor.HttpContext.User.IsInRole(arr[2]))
                            return null;
                    }
                }
                keyWord = arr[0];
            }

            if (keyWord.ToLower() == "imagevault")
            {
                var vaulturl = GetEditUrl() + UriSupport.ResolveUrlFromUIBySettings("../ImageVault.EPiServer.UI/ImageVaultUi");
                return new QuickNavigatorMenuItem("Imagevault", vaulturl, null, "true", null);

            }

            if (keyWord.ToLower() == "find")
            {
                var find = GetEditUrl() + UriSupport.ResolveUrlFromUIBySettings("../find/");
                return new QuickNavigatorMenuItem("Find", find, null, "true", null);
            }


            if (keyWord.ToLower() == "admin")
            {

                if (!httpContextAccessor.HttpContext.User.IsInRole("WebAdmins"))
                    return null;

                var editUrl = GetEditUrl() + EPiServer.Editor.PageEditing.GetEditUrl(currentContent);

                editUrl = editUrl.ReplaceAfter("CMS", "EPiServer.Cms.UI.Admin/default");

                return new QuickNavigatorMenuItem("/shell/cms/menu/admin", editUrl, null, "true", null);
            }

            if (keyWord.ToLower() == "contenttype")
            {
           
                if (!httpContextAccessor.HttpContext.User.IsInRole("WebAdmins"))
                    return null;

                var editUrl = GetEditUrl() + EPiServer.Editor.PageEditing.GetEditUrl(currentContent);

                PageData pd = null;

                if (this._contentLoader.TryGet<PageData>(currentContent, out pd))
                {
                    editUrl = editUrl.Replace("#", "admin/default.aspx?customdefaultpage=admin/EditContentType.aspx?typeId=" + pd.ContentTypeID + "#");
                    var n = LocalizationService.Current.GetString("/addon/quicknav/pagetype", "Admin pagetype") + " " + pd.PageTypeName;
                    return new QuickNavigatorMenuItem(n, editUrl, null, "true", null);
                }
                return null;
            }

            if (keyWord.ToLower() == "logout")
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

            if (arr.Length > 1)
            {
                return new QuickNavigatorMenuItem(LocalizationService.Current.GetString(arr[0], arr[0]), arr[1], null, "true", null);
            }

            //oh no... 
            return new QuickNavigatorMenuItem(LocalizationService.Current.GetString(item, item), "javascript:alert('Wrong config in Appsetting Gosso.QuickNav')", null, "true", null);
        }

        private string GetEditUrl()
        {

            //Url editViewUrl = editUrlResolver.GetFullUrlToEditView(new EditUrlArguments()
            //{
            //    ForceEditHost = true
            //});
            Url editViewUrl = _editUrlResolver.GetFullUrlToEditView(new SiteDefinition()
            {

            });
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
