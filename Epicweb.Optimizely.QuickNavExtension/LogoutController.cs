using EPiServer.Cms.Shell.UI.Attributes;
using EPiServer.Framework.Localization;
using EPiServer.Framework.Modules;
using EPiServer.Globalization;
using EPiServer.Security;
using EPiServer.Shell.Security;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using System.Globalization;

namespace Epicweb.Optimizely.QuickNavExtension
{

    public static class Constants
    {
        public const string LogoutUrl = "/epicweb/logout";
    }

    [Route(Constants.LogoutUrl)]
    public class LogoutController : Controller
    {
        private UISignInManager _signInManager;
        private LocalizationService _localizationService;
        private IPrincipalAccessor _principalAccessor;
        private IModuleResourceResolver _moduleResourceResolver;
        private const string ReturnUrlKey = "ReturnUrl";

        public LogoutController(
          UISignInManager signInManager,
          LocalizationService localizationService,
          IPrincipalAccessor principalAccessor,
          IModuleResourceResolver moduleResourceResolver)
        {
            this._signInManager = signInManager;
            this._localizationService = localizationService;
            this._principalAccessor = principalAccessor;
            this._moduleResourceResolver = moduleResourceResolver;
        }

        [AcceptAntiforgeryTokenFromQuery]
        [ValidateAntiForgeryReleaseToken]
        public async Task<IActionResult> Logout()
        {
            await this._signInManager.SignOutAsync();
            return Redirect(this.Request.Query[ReturnUrlKey]);
        }

        private bool HasAdminAccess => this._principalAccessor.Principal.IsInRole("Administrators") || this._principalAccessor.Principal.IsInRole("CmsAdmins");

        private bool HasEditAccess => this._principalAccessor.Principal.IsInRole("CmsEditors");
    }
}
