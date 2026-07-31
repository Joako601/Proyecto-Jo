using System.ComponentModel.DataAnnotations;
using ProyectoJo.Domain.Entities;
using Xunit;

namespace ProyectoJo.Application.Tests.Domain
{
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

		private static List<ValidationResult> Validar(Item item)
		{
			var resultados = new List<ValidationResult>();
			Validator.TryValidateObject(item, new ValidationContext(item), resultados, validateAllProperties: true);
			return resultados;
		}

		[Fact]
		public void Item_ConDatosValidos_NoTieneErrores()
		{
			var resultados = Validar(ItemValido());

			Assert.Empty(resultados);
		}

		[Fact]
		public void Item_ConPrecioCero_EsInvalido()
		{
			var item = ItemValido();
			item.Precio = 0m;

			var resultados = Validar(item);

			Assert.Contains(resultados, r => r.MemberNames.Contains(nameof(Item.Precio)));
		}

		[Fact]
		public void Item_ConPrecioNegativo_EsInvalido()
		{
			var item = ItemValido();
			item.Precio = -10m;

			var resultados = Validar(item);

			Assert.Contains(resultados, r => r.MemberNames.Contains(nameof(Item.Precio)));
		}

		[Fact]
		public void Item_SinPlatillo_EsInvalido()
		{
			var item = ItemValido();
			item.Platillo = "";

			var resultados = Validar(item);

			Assert.Contains(resultados, r => r.MemberNames.Contains(nameof(Item.Platillo)));
		}

		[Fact]
		public void Item_SinCategoria_EsInvalido()
		{
			var item = ItemValido();
			item.Categoria = "";

			var resultados = Validar(item);

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

		private static List<ValidationResult> Validar(Finanza finanza)
		{
			var resultados = new List<ValidationResult>();
			Validator.TryValidateObject(finanza, new ValidationContext(finanza), resultados, validateAllProperties: true);
			return resultados;
		}

		[Fact]
		public void Finanza_ConMontoValido_NoTieneErrores()
		{
			var resultados = Validar(FinanzaValida());

			Assert.Empty(resultados);
		}

		[Fact]
		public void Finanza_ConMontoCero_EsInvalido()
		{
			var finanza = FinanzaValida();
			finanza.Monto = 0m;

			var resultados = Validar(finanza);

			Assert.Contains(resultados, r => r.MemberNames.Contains(nameof(Finanza.Monto)));
		}

		[Fact]
		public void Finanza_ConMontoNegativo_EsInvalido()
		{
			var finanza = FinanzaValida();
			finanza.Monto = -100m;

			var resultados = Validar(finanza);

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

		private static List<ValidationResult> Validar(Insumo insumo)
		{
			var resultados = new List<ValidationResult>();
			Validator.TryValidateObject(insumo, new ValidationContext(insumo), resultados, validateAllProperties: true);
			return resultados;
		}

		[Fact]
		public void Insumo_ConStockValido_NoTieneErrores()
		{
			var resultados = Validar(InsumoValido());

			Assert.Empty(resultados);
		}

		[Fact]
		public void Insumo_ConStockActualCero_EsValido()
		{
			var insumo = InsumoValido();
			insumo.StockActual = 0m;

			var resultados = Validar(insumo);

			Assert.Empty(resultados);
		}

		[Fact]
		public void Insumo_ConStockActualNegativo_EsInvalido()
		{
			var insumo = InsumoValido();
			insumo.StockActual = -1m;

			var resultados = Validar(insumo);

			Assert.Contains(resultados, r => r.MemberNames.Contains(nameof(Insumo.StockActual)));
		}

		[Fact]
		public void Insumo_ConStockMinimoNegativo_EsInvalido()
		{
			var insumo = InsumoValido();
			insumo.StockMinimo = -1m;

			var resultados = Validar(insumo);

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

		private static List<ValidationResult> Validar(IngredienteReceta ingrediente)
		{
			var resultados = new List<ValidationResult>();
			Validator.TryValidateObject(ingrediente, new ValidationContext(ingrediente), resultados, validateAllProperties: true);
			return resultados;
		}

		[Fact]
		public void Ingrediente_ConDatosValidos_NoTieneErrores()
		{
			var resultados = Validar(IngredienteValido());

			Assert.Empty(resultados);
		}

		[Fact]
		public void Ingrediente_ConCantidadCero_EsInvalido()
		{
			var ingrediente = IngredienteValido();
			ingrediente.Cantidad = 0m;

			var resultados = Validar(ingrediente);

			Assert.Contains(resultados, r => r.MemberNames.Contains(nameof(IngredienteReceta.Cantidad)));
		}

		[Fact]
		public void Ingrediente_ConCantidadNegativa_EsInvalido()
		{
			var ingrediente = IngredienteValido();
			ingrediente.Cantidad = -2m;

			var resultados = Validar(ingrediente);

			Assert.Contains(resultados, r => r.MemberNames.Contains(nameof(IngredienteReceta.Cantidad)));
		}

		[Fact]
		public void Ingrediente_ConCostoUnitarioNegativo_EsInvalido()
		{
			var ingrediente = IngredienteValido();
			ingrediente.CostoUnitario = -5m;

			var resultados = Validar(ingrediente);

			Assert.Contains(resultados, r => r.MemberNames.Contains(nameof(IngredienteReceta.CostoUnitario)));
		}

		[Fact]
		public void Ingrediente_ConCostoUnitarioCero_EsValido()
		{
			var ingrediente = IngredienteValido();
			ingrediente.CostoUnitario = 0m;

			var resultados = Validar(ingrediente);

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

		private static List<ValidationResult> Validar(Receta receta)
		{
			var resultados = new List<ValidationResult>();
			Validator.TryValidateObject(receta, new ValidationContext(receta), resultados, validateAllProperties: true);
			return resultados;
		}

		[Fact]
		public void Receta_ConRendimientoValido_NoTieneErrores()
		{
			var resultados = Validar(RecetaValida());

			Assert.Empty(resultados);
		}

		[Fact]
		public void Receta_ConRendimientoCero_EsInvalido()
		{
			var receta = RecetaValida();
			receta.Rendimiento = 0;

			var resultados = Validar(receta);

			Assert.Contains(resultados, r => r.MemberNames.Contains(nameof(Receta.Rendimiento)));
		}

		[Fact]
		public void Receta_ConRendimientoNegativo_EsInvalido()
		{
			var receta = RecetaValida();
			receta.Rendimiento = -1;

			var resultados = Validar(receta);

			Assert.Contains(resultados, r => r.MemberNames.Contains(nameof(Receta.Rendimiento)));
		}
	}

	public class PedidoValidationTests
	{
		private static Pedido PedidoValido() => new()
		{
			Mesa = "Mesa 5"
		};

		private static List<ValidationResult> Validar(Pedido pedido)
		{
			var resultados = new List<ValidationResult>();
			Validator.TryValidateObject(pedido, new ValidationContext(pedido), resultados, validateAllProperties: true);
			return resultados;
		}

		[Fact]
		public void Pedido_ConMesaValida_NoTieneErrores()
		{
			var resultados = Validar(PedidoValido());

			Assert.Empty(resultados);
		}

		[Fact]
		public void Pedido_SinMesa_EsInvalido()
		{
			var pedido = PedidoValido();
			pedido.Mesa = "";

			var resultados = Validar(pedido);

			Assert.Contains(resultados, r => r.MemberNames.Contains(nameof(Pedido.Mesa)));
		}

		[Fact]
		public void Pedido_ConMesaDeMasDe50Caracteres_EsInvalido()
		{
			var pedido = PedidoValido();
			pedido.Mesa = new string('A', 51);

			var resultados = Validar(pedido);

			Assert.Contains(resultados, r => r.MemberNames.Contains(nameof(Pedido.Mesa)));
		}

		[Fact]
		public void Pedido_ConMesaDeExactamente50Caracteres_EsValido()
		{
			var pedido = PedidoValido();
			pedido.Mesa = new string('A', 50);

			var resultados = Validar(pedido);

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

		private static List<ValidationResult> Validar(Promocion promocion)
		{
			var resultados = new List<ValidationResult>();
			Validator.TryValidateObject(promocion, new ValidationContext(promocion), resultados, validateAllProperties: true);
			return resultados;
		}

		[Fact]
		public void Promocion_SinDescuento_NoTieneErrores()
		{
			var resultados = Validar(PromocionValida());

			Assert.Empty(resultados);
		}

		[Fact]
		public void Promocion_ConDescuentoYSinValorDescuento_EsInvalido()
		{
			var promocion = PromocionValida();
			promocion.TipoDescuento = TipoDescuento.Porcentaje;
			promocion.ValorDescuento = null;

			var resultados = Validar(promocion);

			Assert.Contains(resultados, r => r.MemberNames.Contains(nameof(Promocion.ValorDescuento)));
		}

		[Fact]
		public void Promocion_ConDescuentoYValorNegativoOCero_EsInvalido()
		{
			var promocion = PromocionValida();
			promocion.TipoDescuento = TipoDescuento.MontoFijo;
			promocion.ValorDescuento = 0m;

			var resultados = Validar(promocion);

			Assert.Contains(resultados, r => r.MemberNames.Contains(nameof(Promocion.ValorDescuento)));
		}

		[Fact]
		public void Promocion_ConDescuentoPorcentualMayorA100_EsInvalido()
		{
			var promocion = PromocionValida();
			promocion.TipoDescuento = TipoDescuento.Porcentaje;
			promocion.ValorDescuento = 150m;

			var resultados = Validar(promocion);

			Assert.Contains(resultados, r => r.MemberNames.Contains(nameof(Promocion.ValorDescuento)));
		}

		[Fact]
		public void Promocion_ConDescuentoPorcentualValido_NoTieneErrores()
		{
			var promocion = PromocionValida();
			promocion.TipoDescuento = TipoDescuento.Porcentaje;
			promocion.ValorDescuento = 20m;

			var resultados = Validar(promocion);

			Assert.Empty(resultados);
		}

		[Fact]
		public void Promocion_ConDescuentoDeMontoFijoValido_NoTieneErrores()
		{
			var promocion = PromocionValida();
			promocion.TipoDescuento = TipoDescuento.MontoFijo;
			promocion.ValorDescuento = 50m;

			var resultados = Validar(promocion);

			Assert.Empty(resultados);
		}

		[Fact]
		public void Promocion_ConFechaInicioPosteriorAFechaFin_EsInvalido()
		{
			var promocion = PromocionValida();
			promocion.FechaInicio = new DateTime(2026, 8, 10);
			promocion.FechaFin = new DateTime(2026, 8, 1);

			var resultados = Validar(promocion);

			Assert.Contains(resultados, r => r.MemberNames.Contains(nameof(Promocion.FechaFin)));
		}

		[Fact]
		public void Promocion_ConRangoDeFechasValido_NoTieneErrores()
		{
			var promocion = PromocionValida();
			promocion.FechaInicio = new DateTime(2026, 8, 1);
			promocion.FechaFin = new DateTime(2026, 8, 10);

			var resultados = Validar(promocion);

			Assert.Empty(resultados);
		}
	}
}
