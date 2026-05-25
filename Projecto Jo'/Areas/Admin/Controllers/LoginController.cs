using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;

[Area("Admin")]
public class LoginController : Controller
{
	[HttpGet]
	public IActionResult Login() => View();

	[HttpPost]
	public async Task<IActionResult> Login(string usuario, string password)
	{
		// Usuario y contrasena temporal
		if (usuario == "Joaquin" && password == "Secreto123")
		{
			var claims = new List<Claim> { new Claim(ClaimTypes.Name, usuario) };
			var identity = new ClaimsIdentity(claims, "JoCookieAuth");
			await HttpContext.SignInAsync("JoCookieAuth", new ClaimsPrincipal(identity));

			return RedirectToAction("Index", "Gestion"); // Te manda al panel
		}
		ViewBag.Error = "Datos incorrectos";
		return View();
	}
}