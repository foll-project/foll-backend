using Reqnroll;

namespace foll_backend_tets.StepDefinitions;

[Binding]
public class GraficoTendenciasMovilidadSteps
{
    private int _diasDeDatosDisponibles;
    private bool _seAccedioALaSeccion;
    private List<string> _datasetGrafico = new();
    private bool _isMonitoringActive;

    [Given(@"que existen datos de actividad del adulto mayor correspondientes a los últimos 7 días")]
    public void GivenQueExistenDatosDeActividadDeLosUltimos7Dias()
    {
        _diasDeDatosDisponibles = 7;
        _seAccedioALaSeccion = false;
        _datasetGrafico.Clear();
    }

    [Given(@"que el sistema continúa registrando datos de movimiento del adulto mayor")]
    public void GivenQueElSistemaContinuaRegistrandoDatosDeMovimiento()
    {
        _isMonitoringActive = true;
        _diasDeDatosDisponibles = 7; // Estado base previo
        _datasetGrafico = new List<string> { "Día 1", "Día 2", "Día 3", "Día 4", "Día 5", "Día 6", "Día 7" };
    }

    [When(@"el cuidador accede a la sección de análisis de movilidad")]
    public void WhenElCuidadorAccedeALaSeccionDeAnalisisDeMovilidad()
    {
        _seAccedioALaSeccion = true;
        
        // Simulación de la query que extrae las horas agregadas del backend
        for (int i = 1; i <= _diasDeDatosDisponibles; i++)
        {
            _datasetGrafico.Add($"Día {i}: [Activo: 6h, Inactivo: 18h]");
        }
    }

    [When(@"se completa un nuevo día de monitoreo")]
    public void WhenSeCompletaUnNuevoDiaDeMonitoreo()
    {
        if (_isMonitoringActive)
        {
            _diasDeDatosDisponibles++; // Pasa a 8 días de histórico acumulado
            
            // Simulación del trigger o cron que consolida las horas del nuevo día concluido
            _datasetGrafico.Add($"Día {_diasDeDatosDisponibles}: [Activo: 5h, Inactivo: 19h]");
        }
    }

    [Then(@"el sistema muestra un gráfico de barras con las horas de actividad e inactividad diarias")]
    public void ThenElSistemaMuestraUnGraficoDeBarrasConLasHorasDiarias()
    {
        Assert.IsTrue(_seAccedioALaSeccion);
        Assert.AreEqual(7, _datasetGrafico.Count, "El gráfico no renderizó el bloque completo de 7 días.");
        Assert.IsTrue(_datasetGrafico[0].Contains("Activo") && _datasetGrafico[0].Contains("Inactivo"), "El formato de métricas del gráfico es incorrecto.");
    }

    [Then(@"el gráfico de tendencias se actualiza automáticamente incorporando la nueva información")]
    public void ThenElGraficoDeTendenciasSeActualizaAutomanticamente()
    {
        // Validamos que el dataset ahora cuente con el nuevo nodo agregado automáticamente
        Assert.AreEqual(8, _datasetGrafico.Count, "El gráfico no incorporó el nuevo día de monitoreo.");
        Assert.AreEqual("Día 8: [Activo: 5h, Inactivo: 19h]", _datasetGrafico[7], "Los datos del nuevo día no se guardaron correctamente.");
    }
}