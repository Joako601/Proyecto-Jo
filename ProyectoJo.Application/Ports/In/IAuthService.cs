using System;
using System.Collections.Generic;
using System.Text;

namespace ProyectoJo.Application.Ports.In
{
	public interface IAuthService
	{
		bool ValidarCredenciales(string usuario, string contrasena);
	}
}
