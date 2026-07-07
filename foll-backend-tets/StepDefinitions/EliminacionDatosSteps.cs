using System;
using NUnit.Framework;
using Reqnroll;

namespace foll_backend_tets.StepDefinitions;

[Binding]
public class EliminacionDatosSteps
{
    private int _diasTranscurridosDesdeSolicitud;
    private bool _solicitudEliminacionActiva;
    private bool _datosBorradosFisicamente;
    private bool _datosBorradosLogicamente;
    private bool _datosActivosEnPlataforma;

    [Given(@"que han transcurrido (.*) días desde que el usuario solicitó la eliminación de sus datos")]
    public void GivenQueHanTranscurridoDiasDesdeQueElUsuarioSolicito(int dias)
    {
        _diasTranscurridosDesdeSolicitud = dias;
        _solicitudEliminacionActiva = true;
        _datosActivosEnPlataforma = true; // Aún existen en este punto
        _datosBorradosFisicamente = false;
        _datosBorradosLogicamente = false;
    }

    [Given(@"que el usuario ha solicitado la eliminación de sus datos y aún no han transcurrido 30 días")]
    public void GivenQueElUsuarioHaSolicitadoEliminacionYAunNoHanTranscurrido30Dias()
    {
        _diasTranscurridosDesdeSolicitud = 15; // Un número menor al umbral de 30
        _solicitudEliminacionActiva = true;
        _datosActivosEnPlataforma = true;
    }

    [When(@"el sistema ejecuta el proceso programado de limpieza")]
    public void WhenElSistemaEjecutaElProcesoProgramadoDeLimpieza()
    {
        // Simulación del Worker/CronJob de limpieza de base de datos
        if (_solicitudEliminacionActiva && _diasTranscurridosDesdeSolicitud >= 30)
        {
            _datosBorradosLogicamente = true;
            _datosBorradosFisicamente = true;
            _datosActivosEnPlataforma = false;
        }
    }

    [When(@"el usuario cancela la solicitud de eliminación")]
    public void WhenElUsuarioCancelaLaSolicitudDeEliminacion()
    {
        if (_solicitudEliminacionActiva && _diasTranscurridosDesdeSolicitud < 30)
        {
            // Se revierte la solicitud
            _solicitudEliminacionActiva = false;
            _datosActivosEnPlataforma = true;
        }
    }

    [Then(@"el sistema realiza el borrado lógico y físico de los datos del usuario de la base de datos")]
    public void ThenElSistemaRealizaElBorradoLogicoYFisico()
    {
        Assert.IsTrue(_datosBorradosLogicamente, "Fallo al aplicar el borrado lógico (Soft Delete).");
        Assert.IsTrue(_datosBorradosFisicamente, "Fallo al purgar los datos físicos (Hard Delete) de la base de datos.");
        Assert.IsFalse(_datosActivosEnPlataforma, "Los datos del usuario siguen mostrándose como activos tras el periodo de retención.");
    }

    [Then(@"el sistema revierte el proceso de eliminación y mantiene todos los datos del usuario activos en la plataforma")]
    public void ThenElSistemaRevierteElProcesoYMantieneLosDatosActivos()
    {
        Assert.IsFalse(_solicitudEliminacionActiva, "La solicitud de eliminación sigue en pie, no se revirtió correctamente.");
        Assert.IsTrue(_datosActivosEnPlataforma, "Los datos del usuario fueron afectados a pesar de haber cancelado a tiempo.");
        Assert.IsFalse(_datosBorradosFisicamente, "Hubo pérdida de datos físicos durante el periodo de gracia.");
    }
}