Feature: US026 - Aviso de estado de situación
Como cuidador que ya llegó al lugar del incidente, quiero tener la opción de avisar a mis familiares mediante un estado, para informar que la situación está bajo control sin tener que llamarlos uno por uno.

    Scenario: Notificación de situación bajo control tras atender la emergencia
        Given que se presenta una emergencia en la red familiar [cite: 1]
        When el familiar a cargo acude al lugar del incidente [cite: 1]
        And una vez controlada la situación presiona el botón de "Todo bajo control" en la aplicación [cite: 1]
        Then el sistema actualiza el estado del incidente a "Bajo Control" [cite: 1]
        And notifica a los demás familiares que la situación está bajo control [cite: 1]