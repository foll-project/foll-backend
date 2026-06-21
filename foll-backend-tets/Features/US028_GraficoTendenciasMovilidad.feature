Feature: US028 - Gráfico de tendencias de movilidad
Como cuidador, quiero visualizar un gráfico de tendencias de movilidad "actividad vs inactividad", para detectar si mi padre se mueve menos.

    Scenario: Visualización de gráfico de tendencias de movilidad
        Given que existen datos de actividad del adulto mayor correspondientes a los últimos 7 días
        When el cuidador accede a la sección de análisis de movilidad
        Then el sistema muestra un gráfico de barras con las horas de actividad e inactividad diarias

    Scenario: Continuidad del monitoreo
        Given que el sistema continúa registrando datos de movimiento del adulto mayor
        When se completa un nuevo día de monitoreo
        Then el gráfico de tendencias se actualiza automáticamente incorporando la nueva información