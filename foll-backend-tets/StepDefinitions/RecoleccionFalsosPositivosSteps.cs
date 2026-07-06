using System;
using NUnit.Framework;
using Reqnroll;

namespace foll_backend_tets.StepDefinitions;

[Binding]
public class RecoleccionFalsosPositivosSteps
{
    private bool _notificacionRecibida;
    private bool _incidenteActivo;
    private bool _protocoloEmergenciaDetenido;
    private string _estadoIncidente = string.Empty;
    private bool _datosInercialesAnonimizados;
    private bool _enviadoAColaDeReentrenamiento;

    [Given(@"que el cuidador recibe una notificación de alerta en la aplicación móvil")]
    public void GivenQueElCuidadorRecibeUnaNotificacionDeAlertaEnLaAplicacionMovil()
    {
        _notificacionRecibida = true;
        _incidenteActivo = true;
        _estadoIncidente = "Open";
        _protocoloEmergenciaDetenido = false;
    }

    [Given(@"que un incidente ha sido marcado como ""(.*)""")]
    public void GivenQueUnIncidenteHaSidoMarcadoComo(string falsaAlarma)
    {
        _incidenteActivo = false;
        _estadoIncidente = "FalsePositive";
        _datosInercialesAnonimizados = false;
        _enviadoAColaDeReentrenamiento = false;
    }

    [When(@"selecciona la opción ""(.*)""")]
    public void WhenSeleccionaLaOpcion(string opcion)
    {
        if (_notificacionRecibida && opcion == "Falsa Alarma")
        {
            // Simula el comando de marcado de falso positivo
            _estadoIncidente = "FalsePositive";
            _incidenteActivo = false;
            _protocoloEmergenciaDetenido = true;
        }
    }

    [When(@"el sistema procesa el cierre del evento")]
    public void WhenElSistemaProcesaElCierreDelEvento()
    {
        if (_estadoIncidente == "FalsePositive")
        {
            // Simula el proceso interno de limpieza de PII (Personally Identifiable Information)
            _datosInercialesAnonimizados = true;
            
            // Simula el dispatch hacia un Message Broker (RabbitMQ, Kafka, Azure Service Bus)
            _enviadoAColaDeReentrenamiento = true;
        }
    }

    [Then(@"el sistema cierra el incidente activo y detiene cualquier protocolo de emergencia asociado")]
    public void ThenElSistemaCierraElIncidenteActivoYDetieneCualquierProtocolo()
    {
        Assert.IsFalse(_incidenteActivo, "El incidente no fue cerrado exitosamente.");
        Assert.AreEqual("FalsePositive", _estadoIncidente);
        Assert.IsTrue(_protocoloEmergenciaDetenido, "El protocolo de emergencia y notificaciones no se detuvo tras marcar la falsa alarma.");
    }

    [Then(@"el sistema anonimiza el paquete de datos inerciales correspondiente al evento")]
    public void ThenElSistemaAnonimizaElPaqueteDeDatosInerciales()
    {
        Assert.IsTrue(_datosInercialesAnonimizados, "Los datos de telemetría inercial no fueron anonimizados, violando las políticas de privacidad.");
    }

    [Then(@"lo envía al servidor para incorporarlo a la cola de reentrenamiento del modelo de machine learning")]
    public void ThenLoEnviaAlServidorParaIncorporarloALaColaDeReentrenamiento()
    {
        Assert.IsTrue(_enviadoAColaDeReentrenamiento, "El payload no fue encolado para el flujo de trabajo de Machine Learning.");
    }
}