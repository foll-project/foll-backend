Feature: US030 - Generar reporte PDF mensual del paciente
Como cuidador, quiero generar un reporte PDF del último mes, para enviarlo por WhatsApp o correo al médico tratante.

    Scenario: Descarga o envío del reporte generado
        Given que el sistema ha generado correctamente el reporte PDF mensual
        When el cuidador selecciona una opción para compartir el reporte por WhatsApp o correo electrónico
        Then el sistema permite enviar o descargar el archivo PDF con la información del paciente