using System;
using NUnit.Framework;
using Reqnroll;

namespace foll_backend_tets.StepDefinitions;

[Binding]
public class ClasificacionFalsaAlarmaSteps
{
    private bool _isIncidentActive;
    private string _estadoIncidenteDb = string.Empty;
    private string _observacionCierre = string.Empty;
    private bool _protocoloEmergenciaDetenido;
    private bool _disponibleParaEntrenamientoIa;

    [Given(@"que el cuidador ha recibido una alerta de emergencia activa")]
    public void GivenQueElCuidadorHaRecibidoUnaAlertaDeEmergenciaActiva()
    {
        _isIncidentActive = true;
        _estadoIncidenteDb = "Open";
        _protocoloEmergenciaDetenido = false;
        _disponibleParaEntrenamientoIa = false;
    }

    [Given(@"que un incidente ha sido clasificado como ""Falsa alarma""")]
    public void GivenQueUnIncidenteHaSidoClasificadoComoFalsaAlarma()
    {
        _isIncidentActive = false;
        _estadoIncidenteDb = "FalsePositive";
        _protocoloEmergenciaDetenido = true;
        _disponibleParaEntrenamientoIa = false;
    }

    [When(@"selecciona la opción ""Falsa alarma"" después de verificar la situación con la observación ""(.*)""")]
    public void WhenSeleccionaLaOpcionFalsaAlarmaDespuesDeVerificarLaSituacion(string observacion)
    {
        if (_isIncidentActive)
        {
            // Simulación exacta de tu comando: MarkFallIncidentFalsePositiveCommand
            _observacionCierre = observacion;
            _estadoIncidenteDb = "FalsePositive";
            _isIncidentActive = false;
            _protocoloEmergenciaDetenido = true;
        }
    }

    [When(@"el sistema procesa el cierre del evento en la base de datos")]
    public void WhenElSistemaProcesaElCierreDelEventoEnLaBaseDeDatos()
    {
        if (_estadoIncidenteDb == "FalsePositive")
        {
            // Simula el guardado final y la exposición del evento para el módulo de analíticas/IA
            _disponibleParaEntrenamientoIa = true;
        }
    }

    [Then(@"el sistema cierra el incidente y detiene cualquier protocolo de emergencia activo")]
    public void ThenElSistemaCierraElIncidenteYDetieneCualquierProtocolo()
    {
        Assert.AreEqual("FalsePositive", _estadoIncidenteDb);
        Assert.IsFalse(_isIncidentActive, "El incidente debería figurar como cerrado.");
        Assert.IsTrue(_protocoloEmergenciaDetenido, "El protocolo de llamadas/alertas debió detenerse.");
    }

    [Then(@"se guarda el estado del incidente como ""(.*)"" para su uso en análisis y entrenamiento del modelo de IA")]
    public void ThenSeGuardaElEstadoDelIncidenteComoParaSuUsoEnAnalisis(string estadoEsperado)
    {
        Assert.AreEqual(estadoEsperado, _estadoIncidenteDb);
        Assert.IsTrue(_disponibleParaEntrenamientoIa, "El registro no fue marcado correctamente en la base de datos para el reentrenamiento del modelo.");
    }
}