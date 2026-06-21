Feature: US022 - Clasificación de "Falsa Alarma" con previa validación
Como cuidador, quiero clasificar la alerta como "Falsa Alarma" después de comunicarme, para detener el protocolo de emergencia.

    Scenario: Cierre de incidente
        Given que el cuidador ha recibido una alerta de emergencia activa
        When selecciona la opción "Falsa alarma" después de verificar la situación con la observación "El usuario tiró el sensor sin querer"
        Then el sistema cierra el incidente y detiene cualquier protocolo de emergencia activo

    Scenario: Registro del evento para entrenamiento
        Given que un incidente ha sido clasificado como "Falsa alarma"
        When el sistema procesa el cierre del evento en la base de datos
        Then se guarda el estado del incidente como "FalsePositive" para su uso en análisis y entrenamiento del modelo de IA