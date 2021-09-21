using Microsoft.AspNetCore.Mvc;

namespace Web
{
    public class HomeController : Controller
    {
        [Route("home/index")]
        public IActionResult Index()
        {
            return Ok("hi");
        }
    }
}