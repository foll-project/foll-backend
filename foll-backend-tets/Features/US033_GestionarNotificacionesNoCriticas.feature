Feature: US033 - Gestionar notificaciones no críticas
Como cuidador, quiero gestionar las notificaciones no críticas como batería baja o desconexiones, para evitar saturación.

    Scenario: Configuración de notificaciones no críticas
        Given que el cuidador accede a la sección de configuración de notificaciones
        When activa o desactiva los toggles de alertas no críticas para "BateriaBaja" como false y "Desconexiones" como true
        Then el sistema actualiza las preferencias y aplica los cambios inmediatamente en la generación de notificaciones