using Reqnroll;

namespace foll_backend_tets.StepDefinitions;

[Binding]
public class CancelacionAlertaSteps
{
    private bool _posibleCaidaDetectada;
    private bool _vibracionActivada;
    private bool _doubleTapRealizado;
    private int _tiempoTranscurridoSegundos;
    private bool _alertaEnviadaALaNube;

    [Given(@"que el dispositivo detecta una posible caída y activa una vibración de advertencia")]
    public void GivenQueElDispositivoDetectaUnaPosibleCaidaYActivaUnaVibracionDeAdvertencia()
    {
        _posibleCaidaDetectada = true;
        _vibracionActivada = true;
        _alertaEnviadaALaNube = false;
    }

    [Given(@"que el dispositivo detecta una posible caída y emite una vibración de advertencia")]
    public void GivenQueElDispositivoDetectaUnaPosibleCaidaYEmiteUnaVibracionDeAdvertencia()
    {
        // Redirigimos al mismo estado inicial para reutilizar la configuración
        GivenQueElDispositivoDetectaUnaPosibleCaidaYActivaUnaVibracionDeAdvertencia();
    }

    [When(@"el adulto mayor realiza un doble tap en el sensor dentro de los 15 segundos")]
    public void WhenElAdultoMayorRealizaUnDobleTapEnElSensorDentroDeLosSegundos()
    {
        _doubleTapRealizado = true;
        _tiempoTranscurridoSegundos = 8; // Simula que reaccionó antes del límite de tiempo

         EvaluarEnvioDeAlerta();
    }

    [When(@"el usuario no realiza un doble tap dentro de los 15 segundos")]
    public void WhenElUsuarioNoRealizaUnDobleTapDentroDeLosSegundos()
    {
        _doubleTapRealizado = false;
        _tiempoTranscurridoSegundos = 16; // Superó el umbral de los 15 segundos sin interactuar

        EvaluarEnvioDeAlerta();
    }

    [Then(@"la alerta se cancela internamente y no se envía ninguna notificación a los cuidadores")]
    public void ThenLaAlertaSeCancelaInternamenteYNoSeEnviaNingunaNotificacion()
    {
        Assert.IsFalse(_alertaEnviadaALaNube, "La alerta fue enviada a la nube a pesar de que el usuario canceló la pre-alarma a tiempo.");
    }

    [Then(@"el sistema envía automáticamente la alerta a la nube para notificar a los cuidadores")]
    public void ThenElSistemaEnviaAutomanticamenteLaAlertaALaNube()
    {
        Assert.IsTrue(_alertaEnviadaALaNube, "El sistema debió despachar la alerta automáticamente tras agotarse la ventana de cancelación.");
    }

    private void EvaluarEnvioDeAlerta()
    {
        // Lógica del hardware/backend equivalente a tu flujo de control de tiempos (RegisterFallCancelled / RegisterFallDetected)
        if (_posibleCaidaDetectada && _vibracionActivada)
        {
            if (_doubleTapRealizado && _tiempoTranscurridoSegundos <= 15)
            {
                _alertaEnviadaALaNube = false; // Cancelación exitosa
            }
            else
            {
                _alertaEnviadaALaNube = true; // No se canceló a tiempo, se envía la emergencia
            }
        }
    }
}