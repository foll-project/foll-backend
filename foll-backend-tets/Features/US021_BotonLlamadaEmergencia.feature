Feature: US021 - Botón de llamada directa a emergencias
Como cuidador, quiero tener un botón de acceso directo para llamar a emergencias desde la alerta, para no perder tiempo marcando.

    Scenario: Llamada de emergencia desde la alerta
        Given que el cuidador se encuentra en la pantalla de alerta de emergencia del adulto mayor
        When presiona el botón de llamada de emergencia con el número "911"
        Then la aplicación abre el marcador telefónico con el número de emergencia preconfigurado listo para realizar la llamada

    Scenario: Acceso al botón de llamada desde cualquier estado
        Given que existe una alerta de emergencia activa en la aplicación
        When el cuidador accede a la notificación o al detalle del incidente
        Then el botón de llamada de emergencia permanece disponible y funcional en todo momento dentro del flujo de alerta