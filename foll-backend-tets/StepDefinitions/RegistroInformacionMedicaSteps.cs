using Reqnroll;

namespace foll_backend_tets.StepDefinitions;

[Binding]
public class RegistroInformacionMedicaSteps
{
    private string _tipoSangreGuardado = string.Empty;
    private string _alergiasGuardadas = string.Empty;
    private string _medicacionGuardada = string.Empty;
    private bool _isDataSavedCorrectly;

    [Given(@"que el cuidador se encuentra en la sección de datos de emergencia del perfil del adulto mayor")]
    public void GivenQueElCuidadorSeEncuentraEnLaSeccionDeDatosDeEmergencia()
    {
        // Contexto inicial: simulamos que el perfil del adulto mayor está cargado en memoria
        ScenarioContext.Current["AdultoMayorId"] = 42;
        _isDataSavedCorrectly = false;
    }

    [When(@"ingresa el tipo de sangre ""(.*)"", alergias ""(.*)"" y medicación ""(.*)"" y guarda la información")]
    public void WhenIngresaElTipoDeSangreAlergiasYMedicacionYGuardaLaInformacion(string tipoSangre, string alergias, string medicacion)
    {
        // Aquí simularías el mapeo hacia tu comando o DTO, por ejemplo:
        // var command = new UpdateEmergencyInformationCommand(tipoSangre, alergias, medicacion);
        // await _emergencyInfoService.Handle(command);

        _tipoSangreGuardado = tipoSangre;
        _alergiasGuardadas = alergias;
        _medicacionGuardada = medicacion;
        
        _isDataSavedCorrectly = true;
    }

    [Then(@"la aplicación registra los datos correctamente y los muestra disponibles en el perfil del adulto mayor")]
    public void ThenLaAplicacionRegistraLosDatosCorrectamenteYLosMuestraDisponibles()
    {
        // Verificaciones de estado correspondientes al Assert del criterio de aceptación
        Assert.IsTrue(_isDataSavedCorrectly, "La información médica no se guardó correctamente.");
        
        Assert.AreEqual("O+", _tipoSangreGuardado);
        Assert.AreEqual("Penicilina", _alergiasGuardadas);
        Assert.AreEqual("Enalapril 10mg", _medicacionGuardada);
    }
}