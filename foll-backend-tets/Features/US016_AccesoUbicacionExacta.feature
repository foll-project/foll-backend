Feature: US016 - Acceso a ubicación exacta desde notificaciones
Como cuidador, quiero abrir la notificación y ver la ubicación GPS exacta del incidente, para saber hacia dónde ir o enviar ayuda.

    Scenario: Visualización de ubicación exacta del incidente
        Given que el cuidador recibe una notificación de incidente y abre la aplicación
        When accede a la vista de mapa del evento con coordenadas iniciales -12.0463 y -77.0427
        Then la aplicación muestra un pin con la ubicación GPS del incidente

    Scenario: Actualización de ubicación en tiempo real del incidente
        Given que un incidente ha sido detectado y está activo
        When el sistema recibe nuevas coordenadas GPS del dispositivo con latitud -12.0468 y longitud -77.0432
        Then la ubicación del pin en el mapa se actualiza en tiempo real para reflejar la posición más reciente del adulto mayor