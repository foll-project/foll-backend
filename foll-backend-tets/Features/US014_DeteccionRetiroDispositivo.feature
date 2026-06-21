Feature: US014 - Detección de retiro del dispositivo del cuerpo
Como cuidador, quiero que el dispositivo me notifique si es removido del cuerpo (si tiene sensor de contacto), para saber si mi familiar no lo lleva puesto.

    Scenario: Detección de dispositivo removido del cuerpo
        Given que el dispositivo Foll está siendo utilizado con sensor de contacto activo
        When se detecta pérdida de temperatura corporal y ausencia de pulso por un tiempo sostenido
        Then el sistema cambia el estado del dispositivo a "No en uso" y notifica al cuidador

    Scenario: Mantener estado activo cuando hay contacto corporal
        Given que el dispositivo Foll está colocado correctamente en el adulto mayor
        When el sensor detecta temperatura corporal y pulso de forma estable
        Then el sistema mantiene el estado del dispositivo como "En uso" sin generar alertas