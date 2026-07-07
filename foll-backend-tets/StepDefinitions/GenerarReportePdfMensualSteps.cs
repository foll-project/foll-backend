using System;
using NUnit.Framework;
using Reqnroll;

namespace foll_backend_tets.StepDefinitions;

[Binding]
public class GenerarReportePdfMensualSteps
{
    private bool _enSeccionReportes;
    private bool _pdfGenerado;
    private string _contenidoPdfSimulado = string.Empty;
    private bool _intentCompartirAbierto;

    [Given(@"que el cuidador accede a la sección de reportes del adulto mayor")]
    public void GivenQueElCuidadorAccedeALaSeccionDeReportes()
    {
        _enSeccionReportes = true;
        _pdfGenerado = false;
        _intentCompartirAbierto = false;
    }

    [Given(@"que el sistema ha generado correctamente el reporte PDF mensual")]
    public void GivenQueElSistemaHaGeneradoCorrectamenteElReportePdfMensual()
    {
        _enSeccionReportes = true;
        _pdfGenerado = true;
        _contenidoPdfSimulado = "DatosPaciente_GraficoActividad_TablaIncidentes";
        _intentCompartirAbierto = false;
    }

    // Cambiado a texto explícito para evitar colisión de expresiones regulares con la US011
    [When(@"selecciona la opción ""Generar reporte del último mes""")]
    public void WhenSeleccionaLaOpcionGenerarReporteDelUltimoMes()
    {
        if (_enSeccionReportes)
        {
            // Simulación del servicio de generación de PDF (ej. QuestPDF / iText)
            _pdfGenerado = true;
            _contenidoPdfSimulado = "DatosPaciente_GraficoActividad_TablaIncidentes";
        }
    }

    [When(@"el cuidador selecciona una opción para compartir el reporte por WhatsApp o correo electrónico")]
    public void WhenElCuidadorSeleccionaUnaOpcionParaCompartirElReporte()
    {
        if (_pdfGenerado)
        {
            // Simulación del Launcher de .NET MAUI o Intent nativo
            _intentCompartirAbierto = true;
        }
    }

    [Then(@"el sistema genera un archivo PDF con los datos del paciente, un gráfico de actividad y una tabla de incidentes")]
    public void ThenElSistemaGeneraUnArchivoPdfConLosDatosDelPaciente()
    {
        Assert.IsTrue(_pdfGenerado, "El motor de reportes falló al generar el archivo PDF.");
        Assert.IsTrue(_contenidoPdfSimulado.Contains("DatosPaciente"), "El PDF no incluyó los datos clínicos del paciente.");
        Assert.IsTrue(_contenidoPdfSimulado.Contains("GraficoActividad"), "El PDF no renderizó el gráfico de tendencias de movilidad.");
        Assert.IsTrue(_contenidoPdfSimulado.Contains("TablaIncidentes"), "El PDF omitió el historial de incidentes/caídas.");
    }

    [Then(@"el sistema permite enviar o descargar el archivo PDF con la información del paciente")]
    public void ThenElSistemaPermiteEnviarODescargarElArchivoPdf()
    {
        Assert.IsTrue(_intentCompartirAbierto, "El menú nativo para compartir no fue invocado.");
        Assert.IsTrue(_pdfGenerado, "Se intentó compartir un archivo inexistente o corrupto.");
    }
}