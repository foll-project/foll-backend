Feature: US031 - Detección de patrones de desequilibrio y alerta preventiva con IA
Como cuidador, quiero recibir advertencias si mi familiar muestra signos progresivos de inestabilidad al caminar, para poder llevarlo a una consulta médica preventiva antes de que ocurra una caída.

    Scenario: Generación de alerta por inestabilidad progresiva
        Given que el sistema cuenta con registros pasivos de movimiento de los últimos 7 días del adulto mayor
        When el algoritmo detecta un incremento superior al 30 % en los tropiezos sin caídas
        Then el sistema genera una notificación de advertencia para el cuidador y registra el evento en el dashboard web

    Scenario: Visualización del riesgo en el dashboard analítico
        Given que se ha generado una alerta por inestabilidad progresiva
        When el cuidador accede al dashboard web del adulto mayor
        Then el sistema muestra el nivel de riesgo incrementado junto con el historial de micropérdidas de equilibrio y su tendencia a lo largo del tiempo