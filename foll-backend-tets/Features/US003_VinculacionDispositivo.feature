Feature: US003 - Vinculación de dispositivo Foll al perfil del adulto mayor
Como cuidador, quiero vincular el dispositivo físico Foll al perfil del adulto mayor vía Wifi, para activar la recolección de datos.

    Scenario: Conexión correcta del dispositivo Foll
        Given que el dispositivo se encuentra encendido
        When el cuidador escanea el QR o realiza la vinculación normal con ID "FOLL-992"
        Then la aplicación confirma la vinculación y muestra el estado "En línea"

    Scenario: Intento fallido de emparejamiento del dispositivo
        Given que el dispositivo Foll está apagado o fuera de rango
        When el cuidador intenta vincularlo vía Bluetooth o escaneo de QR con ID "FOLL-992"
        Then la aplicación muestra un mensaje de error indicando que no se pudo establecer la conexión
        And sugiere verificar el estado del dispositivo