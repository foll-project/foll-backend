Feature: US010 - Cancelación de alerta mediante interacción del usuario
Como adulto mayor, quiero que el dispositivo vibre brevemente antes de enviar una alerta, para poder cancelarla si fue un tropiezo leve.

    Scenario: Cancelación de alerta
        Given que el dispositivo detecta una posible caída y activa una vibración de advertencia
        When el adulto mayor realiza un doble tap en el sensor dentro de los 15 segundos
        Then la alerta se cancela internamente y no se envía ninguna notificación a los cuidadores

    Scenario: Envío de alerta si no hay cancelación
        Given que el dispositivo detecta una posible caída y emite una vibración de advertencia
        When el usuario no realiza un doble tap dentro de los 15 segundos
        Then el sistema envía automáticamente la alerta a la nube para notificar a los cuidadores