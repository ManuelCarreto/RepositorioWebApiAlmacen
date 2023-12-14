using Microsoft.VisualStudio.TestTools.UnitTesting;
using MiLibreria.Validators;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiLibreria.Validators.Tests;

[TestClass()]
public class PrecioValidacionTests
{
    [TestMethod()]
    public void PrecioValidacionTest()
    {
        // preparacion
        var precioValidacion = new PrecioValidacion();

        decimal? precioNegativo = -1;

        var valContext = new ValidationContext(new { Precio = precioNegativo });

        // ejecucion
        var badResult = precioValidacion.IsValid(precioNegativo);

        var resultadoMalo = precioValidacion.GetValidationResult(precioNegativo, valContext);

        // Verificación. Assert permite hacer verificaciones
        Assert.AreEqual("El precio no puede ser negativo", resultadoMalo?.ErrorMessage);

        Assert.IsFalse(badResult);
        Assert.IsNotNull(badResult);
    }
    [TestMethod]
    public void PrecioValidacionTest1()
    {
        // preparacion
        var precioValidacion = new PrecioValidacion();

        decimal? precioPositivo = 1;

        var valContext1 = new ValidationContext(new { Precio = precioPositivo });

        // ejecucion
        var goodResult = precioValidacion.IsValid(precioPositivo);

        var resultadoBueno = precioValidacion.GetValidationResult(precioPositivo, valContext1);

        // Verificación. Assert permite hacer verificaciones
        Assert.AreEqual(null, resultadoBueno?.ErrorMessage);

        Assert.IsTrue(goodResult);
        Assert.IsNotNull(goodResult);
    }
}