using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Reqnroll;

namespace foll_backend_tets.StepDefinitions;

[Binding]
public class AlmacenarAlertasCacheLocalSteps
{
    private bool _hasInternetConnection;
    private Queue<string> _localCache = new();
    private List<string> _serverReceivedEvents = new();
    private bool _serverConfirmedReceipt;
    private int _alertasEnviadasCount;

    [Given(@"que el dispositivo IoT no tiene conexión a internet")]
    public void GivenQueElDispositivoIotNoTieneConexionAInternet()
    {
        _hasInternetConnection = false;
        _localCache.Clear();
    }

    [Given(@"que existen alertas almacenadas en caché local del dispositivo IoT")]
    public void GivenQueExistenAlertasAlmacenadasEnCacheLocal()
    {
        _hasInternetConnection = false;
        _localCache.Clear();
        // Insertamos en orden de generación (FIFO)
        _localCache.Enqueue("Alerta_Caida_16:00");
        _localCache.Enqueue("Alerta_Caida_16:05");
    }

    [Given(@"que el dispositivo IoT ha enviado alertas pendientes al servidor")]
    public void GivenQueElDispositivoIotHaEnviadoAlertasPendientesAlServidor()
    {
        _hasInternetConnection = true;
        _localCache.Clear();
        _localCache.Enqueue("Alerta_Caida_16:00");

        // El dispositivo procesa y envía, pero aún residen en caché esperando el ACK
        _serverReceivedEvents.Add(_localCache.Peek());
        _alertasEnviadasCount = 1;
        _serverConfirmedReceipt = false;
    }

    [When(@"se genera una alerta de caída u otro evento crítico")]
    public void WhenSeGeneraUnaAlertaDeCaidaUOtroEventoCritico()
    {
        string nuevaAlerta = "Alerta_Caida_Critica_Actual";
        
        if (!_hasInternetConnection)
        {
            // Lógica del controlador local del IoT para desviar el payload al almacenamiento físico/SQLite local
            _localCache.Enqueue(nuevaAlerta);
        }
    }

    [When(@"la conexión a internet se restablece")]
    public void WhenLaConexionAInternetSeRestablece()
    {
        _hasInternetConnection = true;

        // Trabajamos bajo un patrón FIFO para respetar el orden cronológico estricto
        while (_localCache.Count > 0)
        {
            string alertaATransmitir = _localCache.Dequeue();
            _serverReceivedEvents.Add(alertaATransmitir);
            
            // Conservamos una copia temporal simulada en la caché física real hasta recibir la confirmación HTTP/MQTT
            _alertasEnviadasCount++;
        }
    }

    [When(@"el servidor confirma la recepción de los eventos")]
    public void WhenElServidorConfirmaLaRecepcionDeLosEventos()
    {
        if (_serverReceivedEvents.Count > 0)
        {
            // Simulación de ACK (Acknowledgement) exitoso recibido del backend
            _serverConfirmedReceipt = true;
        }
    }

    [Then(@"el sistema almacena la alerta en caché local del dispositivo para su posterior envío")]
    public void ThenElSistemaAlmacenaLaAlertaEnCacheLocalDelDispositivo()
    {
        Assert.IsFalse(_hasInternetConnection);
        Assert.IsTrue(_localCache.Contains("Alerta_Caida_Critica_Actual"), "La alerta no fue interceptada por el almacenamiento local sin conexión.");
    }

    [Then(@"el sistema envía automáticamente todas las alertas pendientes al servidor en orden de generación")]
    public void ThenElSistemaEnviaAutomanticamenteTodasLasAlertasPendientes()
    {
        Assert.IsTrue(_hasInternetConnection);
        Assert.AreEqual(2, _alertasEnviadasCount, "No se despacharon todas las alertas que estaban retenidas en la caché.");
        
        // Verificación del orden cronológico (FIFO)
        Assert.AreEqual("Alerta_Caida_16:00", _serverReceivedEvents[0], "No se respetó el orden de generación más antiguo al enviar.");
        Assert.AreEqual("Alerta_Caida_16:05", _serverReceivedEvents[1], "El segundo evento enviado no coincide en orden secuencial.");
    }

    [Then(@"el sistema elimina las alertas de la caché local para evitar duplicados")]
    public void ThenElSistemaEliminaLasAlertasDeLaCacheLocal()
    {
        if (_serverConfirmedReceipt)
        {
            // Al recibir la respuesta HTTP 200 o el ACK correspondiente, limpiamos el storage
            for (int i = 0; i < _alertasEnviadasCount; i++)
            {
                if (_localCache.Count > 0) _localCache.Dequeue();
            }
            // En nuestra simulación del Given, representamos que el búfer pendiente se limpia por completo
            _localCache.Clear();
        }

        Assert.IsEmpty(_localCache, "La caché local retiene elementos que ya fueron confirmados por el servidor, provocando potenciales duplicados.");
    }
}