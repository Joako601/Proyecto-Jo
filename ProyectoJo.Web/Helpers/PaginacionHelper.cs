namespace ProyectoJo.Web.Helpers
{
	public static class PaginacionHelper
	{
		public static int NormalizarPaginaMinima(int pagina) => pagina < 1 ? 1 : pagina;

		public static int CalcularTotalPaginas(int totalItems, int porPagina) =>
			(int)Math.Ceiling(totalItems / (double)porPagina);
	}
}
