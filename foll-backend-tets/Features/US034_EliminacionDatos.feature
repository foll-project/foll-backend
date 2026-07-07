Feature: US034 - Solicitar eliminación definitiva de datos
Como adulto mayor, quiero solicitar la eliminación definitiva de mis datos si dejo de usar el servicio, para ejercer mi derecho a la protección de mis datos personales.

    Scenario: Eliminación definitiva de los datos del usuario
        Given que han transcurrido 30 días desde que el usuario solicitó la eliminación de sus datos
        When el sistema ejecuta el proceso programado de limpieza
        Then el sistema realiza el borrado lógico y físico de los datos del usuario de la base de datos

    Scenario: Cancelación de la solicitud de eliminación durante el período de gracia
        Given que el usuario ha solicitado la eliminación de sus datos y aún no han transcurrido 30 días
        When el usuario cancela la solicitud de eliminación
        Then el sistema revierte el proceso de eliminación y mantiene todos los datos del usuario activos en la plataforma