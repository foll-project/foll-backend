Feature: US020 - Envío automático de SMS con ubicación a contactos sin aplicación
Como cuidador principal, quiero poder agregar contactos de emergencia que no usan smartphones o que no tienen la aplicación instalada, para maximizar la red de apoyo.

    Scenario: Envío de alerta por SMS a un contacto sin la aplicación
        Given que el sistema ha confirmado una emergencia del adulto mayor
        And que el contacto de emergencia no tiene un token de notificación válido
        When se dispara la notificación de emergencia
        Then el sistema envía un SMS con un mensaje de auxilio y un enlace web temporal con la ubicación del incidente

    Scenario: Envío de notificación a un contacto con la aplicación instalada
        Given que el sistema ha confirmado una emergencia del adulto mayor
        And que el contacto de emergencia tiene un token de notificación push válido
        When se dispara la notificación de emergencia
        Then el sistema envía una notificación push estándar a la aplicación con los detalles del incidente