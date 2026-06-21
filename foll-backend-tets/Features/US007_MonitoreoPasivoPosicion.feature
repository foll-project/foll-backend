Feature: US007 - Monitoreo pasivo de posición del usuario desde el aplicativo
Como adulto mayor, quiero que el sensor monitoree mi posición pasivamente, para no tener que interactuar con él.

    Scenario: Monitoreo pasivo automático
        Given que el adulto mayor tiene el dispositivo encendido y vinculado a su perfil
        When el dispositivo se encuentra en uso durante el día
        Then el sistema captura datos de giroscopio y acelerómetro automáticamente sin requerir interacción del usuario

    Scenario: Continuidad del monitoreo
        Given que el dispositivo está correctamente configurado y funcionando
        And el adulto mayor no interactúa con el dispositivo
        When transcurre el tiempo de uso normal
        Then el dispositivo continúa recolectando datos de movimiento de forma pasiva y constante