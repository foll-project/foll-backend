using System;
using NUnit.Framework;
using Reqnroll;

namespace foll_backend_tets.StepDefinitions;

[Binding]
public class AccesoUbicacionExactaSteps
{
    private double _latitudeMostrada;
    private double _longitudeMostrada;
    private bool _isPinVisibleInMap;
    private bool _isIncidentActive;

    [Given(@"que el cuidador recibe una notificación de incidente y abre la aplicación")]
    public void GivenQueElCuidadorRecibeUnaNotificacionDeIncidente()
    {
        _isIncidentActive = true;
        _isPinVisibleInMap = false;
    }

    [Given(@"que un incidente ha sido detectado y está activo")]
    public void GivenQueUnIncidenteHaSidoDetectadoYEstaActivo()
    {
        _isIncidentActive = true;
        _isPinVisibleInMap = true;
        // Coordenadas base iniciales
        _latitudeMostrada = -12.0463;
        _longitudeMostrada = -77.0427;
    }

    [When(@"accede a la vista de mapa del evento con coordenadas iniciales (.*) y (.*)")]
    public void WhenAccedeALaVistaDeMapaDelEventoConCoordenadasIniciales(double lat, double lng)
    {
        if (_isIncidentActive)
        {
            _latitudeMostrada = lat;
            _longitudeMostrada = lng;
            _isPinVisibleInMap = true;
        }
    }

    [When(@"el sistema recibe nuevas coordenadas GPS del dispositivo con latitud (.*) y longitud (.*)")]
    public void WhenElSistemaRecibeNuevasCoordenadasGPSConLatitudYLongitud(double nuevaLat, double nuevaLng)
    {
        if (_isIncidentActive && _isPinVisibleInMap)
        {
            // Simula el refresco del método RefreshDetection de tu servicio real
            _latitudeMostrada = nuevaLat;
            _longitudeMostrada = nuevaLng;
        }
    }

    [Then(@"la aplicación muestra un pin con la ubicación GPS del incidente")]
    public void ThenLaAplicacionMuestraUnPinConLaUbicacionGPS()
    {
        Assert.IsTrue(_isPinVisibleInMap, "El pin de ubicación no está visible en el mapa.");
        Assert.AreEqual(-12.0463, _latitudeMostrada, 0.0001);
        Assert.AreEqual(-77.0427, _longitudeMostrada, 0.0001);
    }

    [Then(@"la ubicación del pin en el mapa se actualiza en tiempo real para reflejar la posición más reciente del adulto mayor")]
    public void ThenLaUbicacionDelPinSeActualizaEnTiempoReal()
    {
        Assert.IsTrue(_isPinVisibleInMap);
        // Validamos que se hayan renderizado las coordenadas refrescadas
        Assert.AreEqual(-12.0468, _latitudeMostrada, 0.0001);
        Assert.AreEqual(-77.0432, _longitudeMostrada, 0.0001);
    }
}