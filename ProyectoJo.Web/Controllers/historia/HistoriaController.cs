using Microsoft.AspNetCore.Mvc;

namespace ProyectoJo.Web.Controllers
{
	public class HistoriaController : Controller
	{
		public IActionResult Historia()
		{
			return View();
		}
	}
}