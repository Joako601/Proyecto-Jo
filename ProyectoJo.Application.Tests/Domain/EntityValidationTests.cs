using System.ComponentModel.DataAnnotations;
using ProyectoJo.Domain.Entities;
using Xunit;

namespace ProyectoJo.Application.Tests.Domain
{
	internal static class ValidacionHelper
	{
		public static List<ValidationResult> Validar<T>(T entidad) where T : notnull
		{
			var resultados = new List<ValidationResult>();
			Validator.TryValidateObject(entidad, new ValidationContext(entidad), resultados, validateAllProperties: true);
			return resultados;
		}
	}

	public class ItemValidationTests
	{
		private static Item ItemValido() => new()
		{
			Platillo = "Tacos al Pastor",
			Categoria = "Platillos",
			Precio = 120m,
			Ingredientes = "",
			Descripcion = "",
			Base = ""
		};

		[Fact]
		public void Item_ConDatosValidos_NoTieneErrores()
		{
			var resultados = ValidacionHelper.Validar(ItemValido());

			Assert.Empty(resultados);
		}

		[Fact]
		public void Item_ConPrecioCero_EsInvalido()
		{
			var item = ItemValido();
			item.Precio = 0m;

			var resultados = ValidacionHelper.Validar(item);

			Assert.Contains(resultados, r => r.MemberNames.Contains(nameof(Item.Precio)));
		}

		[Fact]
		public void Item_ConPrecioNegativo_EsInvalido()
		{
			var item = ItemValido();
			item.Precio = -10m;

			var resultados = ValidacionHelper.Validar(item);

			Assert.Contains(resultados, r => r.MemberNames.Contains(nameof(Item.Precio)));
		}

		[Fact]
		public void Item_SinPlatillo_EsInvalido()
		{
			var item = ItemValido();
			item.Platillo = "";

			var resultados = ValidacionHelper.Validar(item);

			Assert.Contains(resultados, r => r.MemberNames.Contains(nameof(Item.Platillo)));
		}

		[Fact]
		public void Item_SinCategoria_EsInvalido()
		{
			var item = ItemValido();
			item.Categoria = "";

			var resultados = ValidacionHelper.Validar(item);

			Assert.Contains(resultados, r => r.MemberNames.Contains(nameof(Item.Categoria)));
		}
	}

	public class FinanzaValidationTests
	{
		private static Finanza FinanzaValida() => new()
		{
			Monto = 500m,
			Tipo = TipoMovimiento.Ingreso,
			Categoria = "Ventas",
			Descripcion = ""
		};

		[Fact]
		public void Finanza_ConMontoValido_NoTieneErrores()
		{
			var resultados = ValidacionHelper.Validar(FinanzaValida());

			Assert.Empty(resultados);
		}

		[Fact]
		public void Finanza_ConMontoCero_EsInvalido()
		{
			var finanza = FinanzaValida();
			finanza.Monto = 0m;

			var resultados = ValidacionHelper.Validar(finanza);

			Assert.Contains(resultados, r => r.MemberNames.Contains(nameof(Finanza.Monto)));
		}

		[Fact]
		public void Finanza_ConMontoNegativo_EsInvalido()
		{
			var finanza = FinanzaValida();
			finanza.Monto = -100m;

			var resultados = ValidacionHelper.Validar(finanza);

			Assert.Contains(resultados, r => r.MemberNames.Contains(nameof(Finanza.Monto)));
		}
	}

	public class InsumoValidationTests
	{
		private static Insumo InsumoValido() => new()
		{
			Nombre = "Harina",
			Unidad = UnidadIngrediente.Kilogramo,
			StockActual = 10m,
			StockMinimo = 2m
		};

		[Fact]
		public void Insumo_ConStockValido_NoTieneErrores()
		{
			var resultados = ValidacionHelper.Validar(InsumoValido());

			Assert.Empty(resultados);
		}

		[Fact]
		public void Insumo_ConStockActualCero_EsValido()
		{
			var insumo = InsumoValido();
			insumo.StockActual = 0m;

			var resultados = ValidacionHelper.Validar(insumo);

			Assert.Empty(resultados);
		}

		[Fact]
		public void Insumo_ConStockActualNegativo_EsInvalido()
		{
			var insumo = InsumoValido();
			insumo.StockActual = -1m;

			var resultados = ValidacionHelper.Validar(insumo);

			Assert.Contains(resultados, r => r.MemberNames.Contains(nameof(Insumo.StockActual)));
		}

		[Fact]
		public void Insumo_ConStockMinimoNegativo_EsInvalido()
		{
			var insumo = InsumoValido();
			insumo.StockMinimo = -1m;

			var resultados = ValidacionHelper.Validar(insumo);

			Assert.Contains(resultados, r => r.MemberNames.Contains(nameof(Insumo.StockMinimo)));
		}
	}

	public class IngredienteRecetaValidationTests
	{
		private static IngredienteReceta IngredienteValido() => new()
		{
			InsumoId = 1,
			Nombre = "Harina",
			Cantidad = 1.5m,
			Unidad = UnidadIngrediente.Kilogramo,
			CostoUnitario = 20m
		};

		[Fact]
		public void Ingrediente_ConDatosValidos_NoTieneErrores()
		{
			var resultados = ValidacionHelper.Validar(IngredienteValido());

			Assert.Empty(resultados);
		}

		[Fact]
		public void Ingrediente_ConCantidadCero_EsInvalido()
		{
			var ingrediente = IngredienteValido();
			ingrediente.Cantidad = 0m;

			var resultados = ValidacionHelper.Validar(ingrediente);

			Assert.Contains(resultados, r => r.MemberNames.Contains(nameof(IngredienteReceta.Cantidad)));
		}

		[Fact]
		public void Ingrediente_ConCantidadNegativa_EsInvalido()
		{
			var ingrediente = IngredienteValido();
			ingrediente.Cantidad = -2m;

			var resultados = ValidacionHelper.Validar(ingrediente);

			Assert.Contains(resultados, r => r.MemberNames.Contains(nameof(IngredienteReceta.Cantidad)));
		}

		[Fact]
		public void Ingrediente_ConCostoUnitarioNegativo_EsInvalido()
		{
			var ingrediente = IngredienteValido();
			ingrediente.CostoUnitario = -5m;

			var resultados = ValidacionHelper.Validar(ingrediente);

			Assert.Contains(resultados, r => r.MemberNames.Contains(nameof(IngredienteReceta.CostoUnitario)));
		}

		[Fact]
		public void Ingrediente_ConCostoUnitarioCero_EsValido()
		{
			var ingrediente = IngredienteValido();
			ingrediente.CostoUnitario = 0m;

			var resultados = ValidacionHelper.Validar(ingrediente);

			Assert.Empty(resultados);
		}
	}

	public class RecetaValidationTests
	{
		private static Receta RecetaValida() => new()
		{
			ItemId = 1,
			NombreReceta = "Tacos al Pastor",
			Rendimiento = 4
		};

		[Fact]
		public void Receta_ConRendimientoValido_NoTieneErrores()
		{
			var resultados = ValidacionHelper.Validar(RecetaValida());

			Assert.Empty(resultados);
		}

		[Fact]
		public void Receta_ConRendimientoCero_EsInvalido()
		{
			var receta = RecetaValida();
			receta.Rendimiento = 0;

			var resultados = ValidacionHelper.Validar(receta);

			Assert.Contains(resultados, r => r.MemberNames.Contains(nameof(Receta.Rendimiento)));
		}

		[Fact]
		public void Receta_ConRendimientoNegativo_EsInvalido()
		{
			var receta = RecetaValida();
			receta.Rendimiento = -1;

			var resultados = ValidacionHelper.Validar(receta);

			Assert.Contains(resultados, r => r.MemberNames.Contains(nameof(Receta.Rendimiento)));
		}
	}

	public class PedidoValidationTests
	{
		private static Pedido PedidoValido() => new()
		{
			Mesa = "Mesa 5"
		};

		[Fact]
		public void Pedido_ConMesaValida_NoTieneErrores()
		{
			var resultados = ValidacionHelper.Validar(PedidoValido());

			Assert.Empty(resultados);
		}

		[Fact]
		public void Pedido_SinMesa_EsInvalido()
		{
			var pedido = PedidoValido();
			pedido.Mesa = "";

			var resultados = ValidacionHelper.Validar(pedido);

			Assert.Contains(resultados, r => r.MemberNames.Contains(nameof(Pedido.Mesa)));
		}

		[Fact]
		public void Pedido_ConMesaDeMasDe50Caracteres_EsInvalido()
		{
			var pedido = PedidoValido();
			pedido.Mesa = new string('A', 51);

			var resultados = ValidacionHelper.Validar(pedido);

			Assert.Contains(resultados, r => r.MemberNames.Contains(nameof(Pedido.Mesa)));
		}

		[Fact]
		public void Pedido_ConMesaDeExactamente50Caracteres_EsValido()
		{
			var pedido = PedidoValido();
			pedido.Mesa = new string('A', 50);

			var resultados = ValidacionHelper.Validar(pedido);

			Assert.Empty(resultados);
		}
	}

	public class PromocionValidationTests
	{
		private static Promocion PromocionValida() => new()
		{
			Titulo = "2x1 Tacos",
			TipoDescuento = TipoDescuento.Ninguno
		};

		[Fact]
		public void Promocion_SinDescuento_NoTieneErrores()
		{
			var resultados = ValidacionHelper.Validar(PromocionValida());

			Assert.Empty(resultados);
		}

		[Fact]
		public void Promocion_ConDescuentoYSinValorDescuento_EsInvalido()
		{
			var promocion = PromocionValida();
			promocion.TipoDescuento = TipoDescuento.Porcentaje;
			promocion.ValorDescuento = null;

			var resultados = ValidacionHelper.Validar(promocion);

			Assert.Contains(resultados, r => r.MemberNames.Contains(nameof(Promocion.ValorDescuento)));
		}

		[Fact]
		public void Promocion_ConDescuentoYValorNegativoOCero_EsInvalido()
		{
			var promocion = PromocionValida();
			promocion.TipoDescuento = TipoDescuento.MontoFijo;
			promocion.ValorDescuento = 0m;

			var resultados = ValidacionHelper.Validar(promocion);

			Assert.Contains(resultados, r => r.MemberNames.Contains(nameof(Promocion.ValorDescuento)));
		}

		[Fact]
		public void Promocion_ConDescuentoPorcentualMayorA100_EsInvalido()
		{
			var promocion = PromocionValida();
			promocion.TipoDescuento = TipoDescuento.Porcentaje;
			promocion.ValorDescuento = 150m;

			var resultados = ValidacionHelper.Validar(promocion);

			Assert.Contains(resultados, r => r.MemberNames.Contains(nameof(Promocion.ValorDescuento)));
		}

		[Fact]
		public void Promocion_ConDescuentoPorcentualValido_NoTieneErrores()
		{
			var promocion = PromocionValida();
			promocion.TipoDescuento = TipoDescuento.Porcentaje;
			promocion.ValorDescuento = 20m;

			var resultados = ValidacionHelper.Validar(promocion);

			Assert.Empty(resultados);
		}

		[Fact]
		public void Promocion_ConDescuentoDeMontoFijoValido_NoTieneErrores()
		{
			var promocion = PromocionValida();
			promocion.TipoDescuento = TipoDescuento.MontoFijo;
			promocion.ValorDescuento = 50m;

			var resultados = ValidacionHelper.Validar(promocion);

			Assert.Empty(resultados);
		}

		[Fact]
		public void Promocion_ConFechaInicioPosteriorAFechaFin_EsInvalido()
		{
			var promocion = PromocionValida();
			promocion.FechaInicio = new DateTime(2026, 8, 10);
			promocion.FechaFin = new DateTime(2026, 8, 1);

			var resultados = ValidacionHelper.Validar(promocion);

			Assert.Contains(resultados, r => r.MemberNames.Contains(nameof(Promocion.FechaFin)));
		}

		[Fact]
		public void Promocion_ConRangoDeFechasValido_NoTieneErrores()
		{
			var promocion = PromocionValida();
			promocion.FechaInicio = new DateTime(2026, 8, 1);
			promocion.FechaFin = new DateTime(2026, 8, 10);

			var resultados = ValidacionHelper.Validar(promocion);

			Assert.Empty(resultados);
		}
	}

	public class OpinionClienteValidationTests
	{
		private static OpinionCliente OpinionValida() => new()
		{
			Comentario = "Muy buena atención.",
			Calificacion = 4.5m
		};

		[Fact]
		public void Opinion_ConDatosValidos_NoTieneErrores()
		{
			var resultados = ValidacionHelper.Validar(OpinionValida());

			Assert.Empty(resultados);
		}

		[Fact]
		public void Opinion_SinComentario_EsInvalido()
		{
			var opinion = OpinionValida();
			opinion.Comentario = "";

			var resultados = ValidacionHelper.Validar(opinion);

			Assert.Contains(resultados, r => r.MemberNames.Contains(nameof(OpinionCliente.Comentario)));
		}

		[Fact]
		public void Opinion_ConComentarioDeMasDe1000Caracteres_EsInvalido()
		{
			var opinion = OpinionValida();
			opinion.Comentario = new string('A', 1001);

			var resultados = ValidacionHelper.Validar(opinion);

			Assert.Contains(resultados, r => r.MemberNames.Contains(nameof(OpinionCliente.Comentario)));
		}

		[Fact]
		public void Opinion_ConCalificacionMenorA1_EsInvalido()
		{
			var opinion = OpinionValida();
			opinion.Calificacion = 0.5m;

			var resultados = ValidacionHelper.Validar(opinion);

			Assert.Contains(resultados, r => r.MemberNames.Contains(nameof(OpinionCliente.Calificacion)));
		}

		[Fact]
		public void Opinion_ConCalificacionMayorA5_EsInvalido()
		{
			var opinion = OpinionValida();
			opinion.Calificacion = 5.5m;

			var resultados = ValidacionHelper.Validar(opinion);

			Assert.Contains(resultados, r => r.MemberNames.Contains(nameof(OpinionCliente.Calificacion)));
		}

		[Fact]
		public void Opinion_ConCalificacionFueraDePasoDeMediaEstrella_EsInvalido()
		{
			var opinion = OpinionValida();
			opinion.Calificacion = 3.2m;

			var resultados = ValidacionHelper.Validar(opinion);

			Assert.Contains(resultados, r => r.MemberNames.Contains(nameof(OpinionCliente.Calificacion)));
		}

		[Fact]
		public void Opinion_ConNombreClienteDeMasDe200Caracteres_EsInvalido()
		{
			var opinion = OpinionValida();
			opinion.NombreCliente = new string('A', 201);

			var resultados = ValidacionHelper.Validar(opinion);

			Assert.Contains(resultados, r => r.MemberNames.Contains(nameof(OpinionCliente.NombreCliente)));
		}
	}
}
