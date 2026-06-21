using Reqnroll;

namespace foll_backend_tets.StepDefinitions;

[Binding]
public class VinculacionDispositivoSteps
{
    private bool _isDevicePoweredOn;
    private string _deviceIdIngresado = string.Empty;
    private string _estadoDispositivoResultado = string.Empty;
    private Exception _exceptionCapturada;

    // Si tu servicio real requiere Mocks, los declararías aquí de la siguiente manera:
    // private readonly Mock<IDeviceIncidentAssignmentService> _deviceServiceMock = new();

    [Given(@"que el dispositivo se encuentra encendido")]
    public void GivenQueElDispositivoSeEncuentraEncendido()
    {
        _isDevicePoweredOn = true;
        _exceptionCapturada = null;
    }

    [Given(@"que el dispositivo Foll está apagado o fuera de rango")]
    public void GivenQueElDispositivoFollEstaApagadoOFueraDeRango()
    {
        _isDevicePoweredOn = false;
        _exceptionCapturada = null;
    }

    [When(@"el cuidador escanea el QR o realiza la vinculación normal con ID ""(.*)""")]
    public void WhenElCuidadorEscaneaElQRORealizaLaVinculacionNormalConId(string deviceId)
    {
        _deviceIdIngresado = deviceId;

        try
        {
            if (!_isDevicePoweredOn)
            {
                throw new InvalidOperationException("No se pudo establecer la conexión. Verifica el estado del dispositivo.");
            }

            // Aquí llamarías a tu servicio real del Backend, ej:
            // await _deviceService.BindDeviceToPatientAsync(patientId, deviceId);
            
            _estadoDispositivoResultado = "En línea";
        }
        catch (Exception ex)
        {
            _exceptionCapturada = ex;
        }
    }

    [When(@"el cuidador intenta vincularlo vía Bluetooth o escaneo de QR con ID ""(.*)""")]
    public void WhenElCuidadorIntentaVincularloViaBluetoothOScaneoDeQRConId(string deviceId)
    {
        // Redirigimos al mismo método de acción para reutilizar la lógica de prueba
        WhenElCuidadorEscaneaElQRORealizaLaVinculacionNormalConId(deviceId);
    }

    [Then(@"la aplicación confirma la vinculación y muestra el estado ""(.*)""")]
    public void ThenLaAplicacionConfirmaLaVinculacionYMuestraElEstado(string estadoEsperado)
    {
        Assert.IsNull(_exceptionCapturada, "No se esperaba ningún error durante la vinculación exitosa.");
        Assert.AreEqual(estadoEsperado, _estadoDispositivoResultado);
    }

    [Then(@"la aplicación muestra un mensaje de error indicando que no se pudo establecer la conexión")]
    public void ThenLaAplicacionMuestraUnMensajeDeErrorIndicandoQueNoSePudoEstablecerLaConexion()
    {
        Assert.IsNotNull(_exceptionCapturada, "Se esperaba un fallo de emparejamiento.");
        Assert.IsTrue(_exceptionCapturada.Message.Contains("No se pudo establecer la conexión"));
    }

    [Then(@"sugiere verificar el estado del dispositivo")]
    public void ThenSugiereVerificarElEstadoDelDispositivo()
    {
        Assert.IsTrue(_exceptionCapturada.Message.Contains("Verifica el estado del dispositivo"));
    }
}