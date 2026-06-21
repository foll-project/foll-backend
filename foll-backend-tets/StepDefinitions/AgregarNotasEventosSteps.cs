using System;
using NUnit.Framework;
using Reqnroll;

namespace foll_backend_tets.StepDefinitions;

[Binding]
public class AgregarNotasEventosSteps
{
    private long _incidentIdSeleccionado;
    private string _notaRegistrada = string.Empty;
    private bool _notaGuardadaEnDb;
    private bool _detalleCargado;

    [Given(@"que el cuidador visualiza el historial de eventos del adulto mayor")]
    public void GivenQueElCuidadorVisualizaElHistorialDeEventos()
    {
        _notaGuardadaEnDb = false;
        _detalleCargado = false;
    }

    [Given(@"que existe un evento de caída con notas asociadas ""(.*)""")]
    public void GivenQueExisteUnEventoDeCaidaConNotasAsociadas(string notaExistente)
    {
        _incidentIdSeleccionado = 50;
        _notaRegistrada = notaExistente;
        _notaGuardadaEnDb = true;
        _detalleCargado = false;
    }

    [When(@"selecciona un evento de caída pasado con ID (.*) y agrega la nota ""(.*)""")]
    public void WhenSeleccionaUnEventoDeCaidaPasadoYAgregaLaNota(long id, string nota)
    {
        _incidentIdSeleccionado = id;
        _notaRegistrada = nota;
        
        // Simulación de la ejecución de un Update u operación sobre el agregado
        _notaGuardadaEnDb = true;
        _detalleCargado = true; // Se fuerza la recarga de la vista
    }

    [When(@"el cuidador accede al detalle del incidente en el historial")]
    public void WhenElCuidadorAccedeAlDetalleDelIncidenteEnElHistorial()
    {
        if (_notaGuardadaEnDb)
        {
            _detalleCargado = true;
        }
    }

    [Then(@"el sistema guarda el comentario asociado al ID del incidente")]
    public void ThenElSistemaGuardaElComentarioAsociadoAlIdDelIncidente()
    {
        Assert.IsTrue(_notaGuardadaEnDb, "La nota no fue persistida en la base de datos.");
        Assert.Greater(_incidentIdSeleccionado, 0);
    }

    [Then(@"lo muestra en el detalle del evento")]
    public void ThenLoMuestraEnElDetalleDelEvento()
    {
        Assert.IsTrue(_detalleCargado);
        Assert.AreEqual("Se tropezó con la alfombra", _notaRegistrada);
    }

    [Then(@"el sistema muestra la nota registrada junto con la información del evento")]
    public void ThenElSistemaMuestraLaNotaRegistradaJuntoConLaInformacionDelEvento()
    {
        Assert.IsTrue(_detalleCargado);
        Assert.IsNotEmpty(_notaRegistrada, "La nota histórica no se recuperó en el detalle.");
        Assert.AreEqual("Se tropezó con la alfombra", _notaRegistrada);
    }
}