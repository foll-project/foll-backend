Feature: US018 - Botón "Voy en camino" para coordinar cuidadores
Como cuidador, quiero presionar un botón de "Voy en camino", para informar al resto de familiares que ya me hice cargo.

    Scenario: Actualización de estado al presionar el botón
        Given que existe una alerta activa del adulto mayor compartida entre varios cuidadores
        When el cuidador "A" presiona el botón "Voy en camino"
        Then el sistema actualiza el estado del incidente a "Atendido por A"
        And notifica a los demás cuidadores

    Scenario: Notificación a otros cuidadores
        Given que una alerta de emergencia está activa y visible para múltiples cuidadores
        When el cuidador "A" se marca como responsable del incidente
        Then los cuidadores "B" y "C" reciben una actualización en tiempo real indicando que el incidente está siendo atendido por A