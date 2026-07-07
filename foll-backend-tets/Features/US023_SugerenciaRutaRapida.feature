Feature: US023 - Sugerencia de ruta rápida hacia el lugar de la caída
Como cuidador, quiero que la aplicación me sugiera la ruta más rápida hacia el lugar de la caída, para llegar lo antes posible y superar las condiciones del tráfico.

    Scenario: Apertura de la navegación hacia el incidente
        Given que el cuidador ha recibido una alerta de caída con la ubicación GPS del incidente
        When presiona el botón "Navegar" en la aplicación
        Then el sistema abre una aplicación de mapas externa con las coordenadas del incidente como destino