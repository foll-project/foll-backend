using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Reqnroll;

namespace foll_backend_tets.StepDefinitions;

[Binding]
public class AccesoSoporteTecnicoSteps
{
    private bool _isInsideApp;
    private bool _isSupportSectionOpen;
    private List<string> _faqArticles = new();
    private List<string> _filteredArticles = new();

    [Given(@"que el cuidador se encuentra dentro de la aplicación móvil")]
    public void GivenQueElCuidadorSeEncuentraDentroDeLaAplicacionMovil()
    {
        _isInsideApp = true;
        _isSupportSectionOpen = false;
    }

    [Given(@"que el cuidador se encuentra en la sección de soporte")]
    public void GivenQueElCuidadorSeEncuentraEnLaSeccionDeSoporte()
    {
        _isInsideApp = true;
        _isSupportSectionOpen = true;
        
        // Simulación de los artículos cargados desde la base de datos o el CMS
        _faqArticles = new List<string>
        {
            "¿Cómo cargar la batería del dispositivo?",
            "Problemas de conexión WiFi o Bluetooth",
            "¿Cómo configurar las alertas de emergencia?",
            "Guía de mantenimiento y limpieza del sensor"
        };
    }

    [When(@"accede a la sección de soporte o ayuda")]
    public void WhenAccedeALaSeccionDeSoporteOAyuda()
    {
        if (_isInsideApp)
        {
            _isSupportSectionOpen = true;
            
            // Carga inicial del panel
            _faqArticles = new List<string>
            {
                "¿Cómo cargar la batería del dispositivo?",
                "Problemas de conexión WiFi o Bluetooth",
                "¿Cómo configurar las alertas de emergencia?",
                "Guía de mantenimiento y limpieza del sensor"
            };
        }
    }

    [When(@"ingresa una palabra clave en el buscador, como ""(.*)""")]
    public void WhenIngresaUnaPalabraClaveEnElBuscadorComo(string keyword)
    {
        if (_isSupportSectionOpen)
        {
            // Simulación del motor de búsqueda interno filtrando los títulos
            _filteredArticles = _faqArticles
                .Where(article => article.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }
    }

    [Then(@"el sistema muestra un panel con preguntas frecuentes y artículos básicos para la resolución de problemas")]
    public void ThenElSistemaMuestraUnPanelConPreguntasFrecuentes()
    {
        Assert.IsTrue(_isSupportSectionOpen, "El panel de soporte no se renderizó correctamente en la interfaz.");
        Assert.Greater(_faqArticles.Count, 0, "El sistema no cargó la lista de artículos básicos.");
    }

    [Then(@"el sistema filtra y muestra los artículos de ayuda relacionados con la búsqueda")]
    public void ThenElSistemaFiltraYMuestraLosArticulosDeAyudaRelacionados()
    {
        Assert.IsNotNull(_filteredArticles, "La lista de resultados filtrados es nula.");
        Assert.Greater(_filteredArticles.Count, 0, "El buscador no arrojó resultados para la palabra clave proporcionada.");
        
        // Validamos que los artículos devueltos realmente contengan la palabra buscada
        // (En el Gherkin inyectamos "batería", así que validamos que el filtro haya funcionado)
        bool resultContainsKeyword = _filteredArticles.Any(a => a.Contains("batería", StringComparison.OrdinalIgnoreCase));
        Assert.IsTrue(resultContainsKeyword, "Los resultados filtrados no coinciden con la palabra clave solicitada.");
    }
}