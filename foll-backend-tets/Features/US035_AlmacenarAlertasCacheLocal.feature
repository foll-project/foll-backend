Feature: US035 - Almacenar alertas en caché local sin conexión
Como sistema, quiero almacenar las alertas en caché local (dispositivo IoT) si no hay conexión a internet, para transmitirlas en cuanto vuelva la red.

    Scenario: Almacenamiento local de alertas sin conexión
        Given que el dispositivo IoT no tiene conexión a internet
        When se genera una alerta de caída u otro evento crítico
        Then el sistema almacena la alerta en caché local del dispositivo para su posterior envío

    Scenario: Reenvío de alertas cuando se restablece la conexión
        Given que existen alertas almacenadas en caché local del dispositivo IoT
        When la conexión a internet se restablece
        Then el sistema envía automáticamente todas las alertas pendientes al servidor en orden de generación

    Scenario: Confirmación de sincronización exitosa
        Given que el dispositivo IoT ha enviado alertas pendientes al servidor
        When el servidor confirma la recepción de los eventos
        Then el sistema elimina las alertas de la caché local para evitar duplicados