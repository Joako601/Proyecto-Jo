using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Projecto_Jo_.Areas.Admin.Controllers
{
	[Area("Admin")]
	[Authorize(AuthenticationSchemes = "JoCookieAuth")] 
	public class GestionController : Controller
	{
		public IActionResult Index()
		{
	
			return View();
		}
	}
}