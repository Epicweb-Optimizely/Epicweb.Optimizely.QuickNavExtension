using EPiServer.Cms.Shell.UI.Attributes;
using EPiServer.Shell.Security;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

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
        private const string ReturnUrlKey = "ReturnUrl";

        public LogoutController(
          UISignInManager signInManager)
        {
            _signInManager = signInManager;
        }

        [AcceptAntiforgeryTokenFromQuery]
        [ValidateAntiForgeryReleaseToken]
        public async Task<IActionResult> Logout()
        {
            await this._signInManager.SignOutAsync();
            return Redirect(this.Request.Query[ReturnUrlKey]);
        }
    }
}
