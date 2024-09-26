using Microsoft.AspNetCore.Mvc;

namespace Web
{
    public class HomeController : Controller
    {
        [Route("home/events")]
        public IActionResult Index()
        {
            return Ok("hi");
        }
    }
}