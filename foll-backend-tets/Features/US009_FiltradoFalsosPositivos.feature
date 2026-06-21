Feature: US009 - Filtrado de falsos positivos por movimientos bruscos
Como adulto mayor, quiero realizar mis actividades diarias sin miedo a activar falsas alarmas por movimientos bruscos para no generar pánico innecesario en mi familia.

    Scenario: Falsos positivos filtrados
        Given que se detecta un impacto
        When el algoritmo verifica que no hay un cambio de altitud crítico hacia el nivel del piso y la aceleración posterior se estabiliza de forma natural
        Then el evento se descarta silenciosamente en el hardware sin notificar a la red