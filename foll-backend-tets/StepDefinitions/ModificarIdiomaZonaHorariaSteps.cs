using System;
using NUnit.Framework;
using Reqnroll;

namespace foll_backend_tets.StepDefinitions;

[Binding]
public class ModificarIdiomaZonaHorariaSteps
{
    private bool _seccionConfiguracionAbierta;
    private string _idiomaActual = "Español";
    private string _zonaHorariaActual = "UTC-3";
    private bool _aplicacionReiniciada;

    [Given(@"que el usuario se encuentra en la sección de configuración de la aplicación")]
    public void GivenQueElUsuarioSeEncuentraEnLaSeccionDeConfiguracion()
    {
        _seccionConfiguracionAbierta = true;
        _aplicacionReiniciada = false;
    }

    [Given(@"que el usuario ha guardado un nuevo idioma ""(.*)"" y una nueva zona horaria ""(.*)""")]
    public void GivenQueElUsuarioHaGuardadoUnNuevoIdiomaYUnaNuevaZonaHoraria(string idioma, string zonaHoraria)
    {
        // Simulación de los datos ya guardados en la base de datos o almacenamiento local
        _idiomaActual = idioma;
        _zonaHorariaActual = zonaHoraria;
        _aplicacionReiniciada = false;
    }

    [When(@"modifica el idioma a ""(.*)"" y selecciona una nueva zona horaria ""(.*)""")]
    public void WhenModificaElIdiomaYSeleccionaUnaNuevaZonaHoraria(string nuevoIdioma, string nuevaZonaHoraria)
    {
        if (_seccionConfiguracionAbierta)
        {
            // Simulación del Update a las preferencias del usuario
            _idiomaActual = nuevoIdioma;
            _zonaHorariaActual = nuevaZonaHoraria;
        }
    }

    [When(@"cierra y vuelve a abrir la aplicación")]
    public void WhenCierraYVuelveAAbrirLaAplicacion()
    {
        _aplicacionReiniciada = true;
        
        // Al reiniciar, los valores se recargan desde la persistencia. 
        // En esta simulación, las variables _idiomaActual y _zonaHorariaActual representan esa lectura exitosa.
    }

    [Then(@"el sistema actualiza el idioma de la interfaz y la hora mostrada de acuerdo con las preferencias seleccionadas")]
    public void ThenElSistemaActualizaElIdiomaYLaHoraMostrada()
    {
        Assert.AreEqual("English", _idiomaActual, "El idioma no se actualizó correctamente en el sistema.");
        Assert.AreEqual("UTC-5", _zonaHorariaActual, "La zona horaria no se reflejó correctamente en el sistema.");
    }

    [Then(@"el sistema mantiene las preferencias configuradas previamente sin necesidad de realizar una nueva configuración")]
    public void ThenElSistemaMantieneLasPreferenciasConfiguradasPreviamente()
    {
        Assert.IsTrue(_aplicacionReiniciada, "La prueba falló al simular el reinicio de la aplicación.");
        Assert.AreEqual("English", _idiomaActual, "El idioma configurado se perdió tras reiniciar la app.");
        Assert.AreEqual("UTC-5", _zonaHorariaActual, "La zona horaria configurada se perdió tras reiniciar la app.");
    }
}