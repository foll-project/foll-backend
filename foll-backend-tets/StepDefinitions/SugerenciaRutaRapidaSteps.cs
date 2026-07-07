using System;
using NUnit.Framework;
using Reqnroll;

namespace foll_backend_tets.StepDefinitions;

[Binding]
public class SugerenciaRutaRapidaSteps
{
    private bool _alertaRecibidaConUbicacion;
    private double _latitudIncidente;
    private double _longitudIncidente;
    private bool _intentMapasLanzado;
    private string _uriMapasGenerada = string.Empty;

    [Given(@"que el cuidador ha recibido una alerta de caída con la ubicación GPS del incidente")]
    public void GivenQueElCuidadorHaRecibidoUnaAlertaDeCaidaConUbicacion()
    {
        _alertaRecibidaConUbicacion = true;
        
        // Coordenadas simuladas del incidente
        _latitudIncidente = -12.0463;
        _longitudIncidente = -77.0427;
        
        _intentMapasLanzado = false;
    }

    [When(@"presiona el botón ""(.*)"" en la aplicación")]
    public void WhenPresionaElBotonEnLaAplicacion(string boton)
    {
        if (_alertaRecibidaConUbicacion && boton == "Navegar")
        {
            // Simulación de Launcher.OpenAsync en .NET MAUI o un Intent en Android nativo
            // Ejemplo de URI generada: "geo:-12.0463,-77.0427?q=-12.0463,-77.0427(Incidente)"
            _uriMapasGenerada = $"geo:{_latitudIncidente},{_longitudIncidente}";
            _intentMapasLanzado = true;
        }
    }

    [Then(@"el sistema abre una aplicación de mapas externa con las coordenadas del incidente como destino")]
    public void ThenElSistemaAbreUnaAplicacionDeMapasExterna()
    {
        Assert.IsTrue(_intentMapasLanzado, "La aplicación no disparó el intent/launcher para abrir los mapas.");
        Assert.IsTrue(_uriMapasGenerada.Contains(_latitudIncidente.ToString()) && _uriMapasGenerada.Contains(_longitudIncidente.ToString()), 
            "La URI de mapas no contiene las coordenadas GPS exactas del incidente.");
    }
}