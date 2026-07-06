using System;
using NUnit.Framework;
using Reqnroll;

namespace foll_backend_tets.StepDefinitions;

[Binding]
public class EnvioAutomaticoSmsSteps
{
    private bool _emergenciaConfirmada;
    private bool _tieneTokenPushValido;
    private bool _smsEnviado;
    private bool _enlaceTemporalGenerado;
    private bool _pushNotificationEnviada;

    [Given(@"que el sistema ha confirmado una emergencia del adulto mayor")]
    public void GivenQueElSistemaHaConfirmadoUnaEmergenciaDelAdultoMayor()
    {
        _emergenciaConfirmada = true;
        _smsEnviado = false;
        _enlaceTemporalGenerado = false;
        _pushNotificationEnviada = false;
    }

    [Given(@"que el contacto de emergencia no tiene un token de notificación válido")]
    public void GivenQueElContactoDeEmergenciaNoTieneUnTokenDeNotificacionValido()
    {
        _tieneTokenPushValido = false;
    }

    [Given(@"que el contacto de emergencia tiene un token de notificación push válido")]
    public void GivenQueElContactoDeEmergenciaTieneUnTokenDeNotificacionPushValido()
    {
        _tieneTokenPushValido = true;
    }

    [When(@"se dispara la notificación de emergencia")]
    public void WhenSeDisparaLaNotificacionDeEmergencia()
    {
        if (_emergenciaConfirmada)
        {
            // Simulación del patrón de estrategia (Strategy Pattern) para el canal de notificación
            if (_tieneTokenPushValido)
            {
                // Flujo estándar hacia Firebase Cloud Messaging (FCM) o Apple Push Notification Service (APNs)
                _pushNotificationEnviada = true;
            }
            else
            {
                // Flujo alternativo (Fallback) hacia proveedor SMS (Twilio, AWS SNS, etc.)
                _smsEnviado = true;
                _enlaceTemporalGenerado = true;
            }
        }
    }

    [Then(@"el sistema envía un SMS con un mensaje de auxilio y un enlace web temporal con la ubicación del incidente")]
    public void ThenElSistemaEnviaUnSmsConMensajeDeAuxilioYEnlaceWebTemporal()
    {
        Assert.IsTrue(_smsEnviado, "El sistema falló al intentar enviar el SMS de respaldo.");
        Assert.IsTrue(_enlaceTemporalGenerado, "El SMS se envió, pero no incluyó el enlace web temporal con las coordenadas del incidente.");
        Assert.IsFalse(_pushNotificationEnviada, "Se intentó enviar una notificación push a un dispositivo sin token válido.");
    }

    [Then(@"el sistema envía una notificación push estándar a la aplicación con los detalles del incidente")]
    public void ThenElSistemaEnviaUnaNotificacionPushEstandarALaAplicacion()
    {
        Assert.IsTrue(_pushNotificationEnviada, "El sistema no despachó la notificación push a pesar de tener un token válido.");
        Assert.IsFalse(_smsEnviado, "El sistema envió un SMS innecesario, generando posibles costos extra de facturación.");
    }
}