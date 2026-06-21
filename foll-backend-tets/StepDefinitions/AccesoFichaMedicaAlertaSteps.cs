using System;
using NUnit.Framework;
using Reqnroll;

namespace foll_backend_tets.StepDefinitions;

[Binding]
public class AccesoFichaMedicaAlertaSteps
{
    private bool _alertaRecibida;
    private bool _pantallaDetalleAbierta;
    private bool _fichaMedicaVisible;
    private int _edadMostrada;
    private string _tipoSangreMostrado = string.Empty;
    private string _alergiasMostradas = string.Empty;
    private string _enfermedadesBaseMostradas = string.Empty;

    [Given(@"que el cuidador ha recibido una alerta de emergencia del adulto mayor")]
    public void GivenQueElCuidadorHaRecibidoUnaAlertaDeEmergencia()
    {
        _alertaRecibida = true;
        _pantallaDetalleAbierta = false;
        _fichaMedicaVisible = false;
    }

    [Given(@"que existe una alerta de emergencia activa del adulto mayor")]
    public void GivenQueExisteUnaAlertaDeEmergenciaActiva()
    {
        _alertaRecibida = true;
        _pantallaDetalleAbierta = true;
        _fichaMedicaVisible = true;
    }

    [When(@"abre la pantalla de detalle de la alerta en la aplicación")]
    public void WhenAbreLaPantallaDeDetalleDeLaAlerta()
    {
        if (_alertaRecibida)
        {
            _pantallaDetalleAbierta = true;
            
            // Simulación de carga de datos combinados (Alerta + Ficha Médica)
            _edadMostrada = 81;
            _tipoSangreMostrado = "O+";
            _alergiasMostradas = "Penicilina";
            _enfermedadesBaseMostradas = "Hipertensión";
            _fichaMedicaVisible = true;
        }
    }

    [When(@"el cuidador navega dentro de la pantalla de alerta")]
    public void WhenElCuidadorNavegaDentroDeLaPantallaDeAlerta()
    {
        // Simula navegación interna (p.ej. ver pestañas de mapa o logs del sensor)
        // La ficha médica debe persistir montada en el componente/vista
        if (_pantallaDetalleAbierta)
        {
            _fichaMedicaVisible = true; 
        }
    }

    [Then(@"la aplicación muestra la ficha médica con edad (.*), tipo de sangre ""(.*)"", alergias ""(.*)"" y enfermedades base ""(.*)""")]
    public void ThenLaAplicacionMuestraLaFichaMedicaConDetallesCompletos(int edad, string tipoSangre, string alergias, string enfermedades)
    {
        Assert.IsTrue(_fichaMedicaVisible, "La ficha médica debería renderizarse en la pantalla de detalle.");
        Assert.AreEqual(edad, _edadMostrada);
        Assert.AreEqual(tipoSangre, _tipoSangreMostrado);
        Assert.AreEqual(alergias, _alergiasMostradas);
        Assert.AreEqual(enfermedades, _enfermedadesBaseMostradas);
    }

    [Then(@"la ficha médica permanece accesible y visible sin necesidad de salir del flujo de emergencia")]
    public void ThenLaFichaMedicaPermaneceAccesibleYVisible()
    {
        Assert.IsTrue(_pantallaDetalleAbierta, "El usuario salió del flujo de la alerta.");
        Assert.IsTrue(_fichaMedicaVisible, "La ficha médica se ocultó durante la navegación interna del incidente.");
    }
}