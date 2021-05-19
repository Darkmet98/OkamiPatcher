using Microsoft.AspNetCore.Mvc;

namespace OkamiPatcher.Controllers
{
    public class CreditsController : Controller
    {
        public ActionResult Index()
        {
            return View(this);
        }
    }
}
