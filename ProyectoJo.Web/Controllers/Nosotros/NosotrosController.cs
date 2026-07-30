using Microsoft.AspNetCore.Mvc;

namespace ProyectoJo.Web.Controllers;

public class NosotrosController : Controller
{
	public IActionResult Index() => View();
}