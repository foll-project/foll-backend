Feature: US011 - Recolección de falsos positivos para mejora del modelo
Como cuidador de adultos mayores, quiero poder marcar una notificación como "Falsa Alarma", para detener el protocolo de emergencia y ayudar a que el dispositivo aprenda las rutinas de mi familiar.

    Scenario: Marcar una notificación como falsa alarma
        Given que el cuidador recibe una notificación de alerta en la aplicación móvil
        When selecciona la opción "Falsa Alarma"
        Then el sistema cierra el incidente activo y detiene cualquier protocolo de emergencia asociado

    Scenario: Envío de datos para reentrenamiento del modelo
        Given que un incidente ha sido marcado como "Falsa Alarma"
        When el sistema procesa el cierre del evento
        Then el sistema anonimiza el paquete de datos inerciales correspondiente al evento
        And lo envía al servidor para incorporarlo a la cola de reentrenamiento del modelo de machine learning