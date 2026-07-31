namespace ProyectoJo.Domain.Entities
{
	public interface IEntidadConId
	{
		int Id { get; set; }
	}

	public static class EntidadConIdExtensions
	{
		public static void DescartarId(this IEntidadConId entidad) => entidad.Id = 0;
	}
}
