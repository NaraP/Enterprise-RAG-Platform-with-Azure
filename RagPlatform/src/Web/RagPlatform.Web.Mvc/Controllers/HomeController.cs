using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace RagPlatform.Web.Mvc.Controllers;

public class HomeController : Controller
{
    [AllowAnonymous]
    public IActionResult Index() => RedirectToAction("Index", "Documents");

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error() => View();
}
