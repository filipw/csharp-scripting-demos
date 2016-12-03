using Microsoft.AspNetCore.Mvc;

namespace ScriptingInWebAppDemo.Controllers
{
    public class HomeController : Controller
    {

        public IActionResult Index()
        {
            return View();
        }
    }
}
