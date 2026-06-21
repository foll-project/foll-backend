using System;
using NUnit.Framework;
using Reqnroll;

namespace foll_backend_tets.StepDefinitions;

[Binding]
public class FiltradoFalsosPositivosSteps
{
    private bool _impactoDetectado;
    private double _cambioAltitudMetros;
    private bool _aceleracionEstabilizada;
    private bool _eventoDescartadoSilenciosamente;
    private bool _notificacionEnviadaALaRed;

    [Given(@"que se detecta un impacto")]
    public void GivenQueSeDetectaUnImpacto()
    {
        _impactoDetectado = true;
        _eventoDescartadoSilenciosamente = false;
        _notificacionEnviadaALaRed = false;
    }

    [When(@"el algoritmo verifica que no hay un cambio de altitud crítico hacia el nivel del piso y la aceleración posterior se estabiliza de forma natural")]
    public void WhenElAlgoritmoVerificaCriteriosDeDescarte()
    {
        // Simulamos valores que representan un movimiento brusco cotidiano (ej. sentarse rápido)
        _cambioAltitudMetros = 0.2; // Un cambio leve, no crítico como una caída al piso (> 1.0m)
        _aceleracionEstabilizada = true; // El usuario siguió moviéndose normalmente después del impacto

        // Lógica del filtro de falsos positivos (equivalente a las reglas de tu servicio de telemetría/incidentes)
        if (_impactoDetectado && _cambioAltitudMetros < 0.8 && _aceleracionEstabilizada)
        {
            _eventoDescartadoSilenciosamente = true;
            _notificacionEnviadaALaRed = false;
        }
        else
        {
            _eventoDescartadoSilenciosamente = false;
            _notificacionEnviadaALaRed = true;
        }
    }

    [Then(@"el evento se descarta silenciosamente en el hardware sin notificar a la red")]
    public void ThenElEventoSeDescartaSilenciosamenteSinNotificarALaRed()
    {
        Assert.IsTrue(_eventoDescartadoSilenciosamente, "El algoritmo debió clasificar el movimiento como un falso positivo y descartarlo.");
        Assert.IsFalse(_notificacionEnviadaALaRed, "Se envió una notificación de alerta errónea a la red familiar, generando pánico innecesario.");
    }
}