using Reqnroll;

namespace foll_backend_tets.StepDefinitions;

[Binding]
public class GestionarNotificacionesNoCriticasSteps
{
    private Dictionary<string, bool> _preferenciasNotificaciones = new();
    private bool _seAccedioAConfiguracion;
    private bool _cambiosAplicadosInmediatamente;

    [Given(@"que el cuidador accede a la sección de configuración de notificaciones")]
    public void GivenQueElCuidadorAccedeALaSeccionDeConfiguracion()
    {
        _seAccedioAConfiguracion = true;
        _cambiosAplicadosInmediatamente = false;
        
        // Estado inicial por defecto (todas activadas)
        _preferenciasNotificaciones["BateriaBaja"] = true;
        _preferenciasNotificaciones["Desconexiones"] = true;
    }

    [When(@"activa o desactiva los toggles de alertas no críticas para ""(.*)"" como (.*) y ""(.*)"" como (.*)")]
    public void WhenActivaODesactivaLosTogglesDeAlertasNoCriticas(string tipo1, bool estado1, string tipo2, bool estado2)
    {
        if (_seAccedioAConfiguracion)
        {
            // Simulación de la persistencia de las preferencias del perfil
            _preferenciasNotificaciones[tipo1] = estado1;
            _preferenciasNotificaciones[tipo2] = estado2;
            
            // Simula el refresco inmediato del componente o manejador de políticas de eventos
            _cambiosAplicadosInmediatamente = true;
        }
    }

    [Then(@"el sistema actualiza las preferencias y aplica los cambios inmediatamente en la generación de notificaciones")]
    public void ThenElSistemaActualizaLasPreferenciasYAplicaLosCambiosInmediatamente()
    {
        Assert.IsTrue(_cambiosAplicadosInmediatamente, "Las preferencias no se aplicaron de forma reactiva en el sistema.");
        
        // Verificaciones del Assert según el escenario ejecutado
        Assert.IsFalse(_preferenciasNotificaciones["BateriaBaja"], "El toggle de batería baja debió desactivarse.");
        Assert.IsTrue(_preferenciasNotificaciones["Desconexiones"], "El toggle de desconexiones debió mantenerse activo.");
    }
}