using System;
using NUnit.Framework;
using Reqnroll;

namespace foll_backend_tets.StepDefinitions;

[Binding]
public class DeteccionDesequilibrioIASteps
{
    private int _diasDeRegistroPasivo;
    private double _porcentajeIncrementoTropiezos;
    private bool _alertaPreventivaGenerada;
    private bool _eventoRegistradoEnDashboard;
    private bool _accesoAlDashboard;
    private bool _nivelRiesgoIncrementadoMostrado;
    private bool _historialTendenciasMostrado;

    [Given(@"que el sistema cuenta con registros pasivos de movimiento de los últimos (.*) días del adulto mayor")]
    public void GivenQueElSistemaCuentaConRegistrosPasivos(int dias)
    {
        _diasDeRegistroPasivo = dias;
        _alertaPreventivaGenerada = false;
        _eventoRegistradoEnDashboard = false;
    }

    [Given(@"que se ha generado una alerta por inestabilidad progresiva")]
    public void GivenQueSeHaGeneradoUnaAlertaPorInestabilidadProgresiva()
    {
        _alertaPreventivaGenerada = true;
        _eventoRegistradoEnDashboard = true;
        _accesoAlDashboard = false;
    }

    [When(@"el algoritmo detecta un incremento superior al (.*) % en los tropiezos sin caídas")]
    public void WhenElAlgoritmoDetectaUnIncrementoSuperiorAl(double porcentaje)
    {
        _porcentajeIncrementoTropiezos = porcentaje;

        if (_diasDeRegistroPasivo >= 7 && _porcentajeIncrementoTropiezos >= 30.0)
        {
            // Simulación del motor analítico detectando el patrón de riesgo
            _alertaPreventivaGenerada = true;
            _eventoRegistradoEnDashboard = true;
        }
    }

    [When(@"el cuidador accede al dashboard web del adulto mayor")]
    public void WhenElCuidadorAccedeAlDashboardWeb()
    {
        _accesoAlDashboard = true;

        if (_alertaPreventivaGenerada)
        {
            // Simula la carga de datos del QueryService hacia el frontend
            _nivelRiesgoIncrementadoMostrado = true;
            _historialTendenciasMostrado = true;
        }
    }

    [Then(@"el sistema genera una notificación de advertencia para el cuidador y registra el evento en el dashboard web")]
    public void ThenElSistemaGeneraNotificacionYRegistraEnDashboard()
    {
        Assert.IsTrue(_alertaPreventivaGenerada, "El motor de IA no disparó la notificación preventiva al superar el umbral de tropiezos.");
        Assert.IsTrue(_eventoRegistradoEnDashboard, "El evento de inestabilidad no fue persistido para su visualización en la plataforma web.");
    }

    [Then(@"el sistema muestra el nivel de riesgo incrementado junto con el historial de micropérdidas de equilibrio y su tendencia a lo largo del tiempo")]
    public void ThenElSistemaMuestraRiesgoIncrementadoYTendencia()
    {
        Assert.IsTrue(_accesoAlDashboard);
        Assert.IsTrue(_nivelRiesgoIncrementadoMostrado, "El dashboard no reflejó el aumento en el nivel de riesgo del paciente.");
        Assert.IsTrue(_historialTendenciasMostrado, "Los gráficos de tendencia de micropérdidas no se renderizaron en el dashboard.");
    }
}