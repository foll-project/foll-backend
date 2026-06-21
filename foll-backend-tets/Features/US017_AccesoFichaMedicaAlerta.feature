Feature: US017 - Acceso a ficha médica en alerta de emergencia
Como cuidador, quiero visualizar en la misma pantalla de alerta la Ficha Médica del adulto mayor, para dictársela al servicio de emergencias.

    Scenario: Visualización de ficha médica
        Given que el cuidador ha recibido una alerta de emergencia del adulto mayor
        When abre la pantalla de detalle de la alerta en la aplicación
        Then la aplicación muestra la ficha médica con edad 81, tipo de sangre "O+", alergias "Penicilina" y enfermedades base "Hipertensión"

    Scenario: Acceso a ficha médica durante incidente
        Given que existe una alerta de emergencia activa del adulto mayor
        When el cuidador navega dentro de la pantalla de alerta
        Then la ficha médica permanece accesible y visible sin necesidad de salir del flujo de emergencia