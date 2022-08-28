using Epicweb.Alloy.QuickNavExtension.Models.Pages;
using Epicweb.Alloy.QuickNavExtension.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace Epicweb.Alloy.QuickNavExtension.Controllers;

public class SearchPageController : PageControllerBase<SearchPage>
{
    public ViewResult Index(SearchPage currentPage, string q)
    {
        var model = new SearchContentModel(currentPage)
        {
            Hits = Enumerable.Empty<SearchContentModel.SearchHit>(),
            NumberOfHits = 0,
            SearchServiceDisabled = true,
            SearchedQuery = q
        };

        return View(model);
    }
}
