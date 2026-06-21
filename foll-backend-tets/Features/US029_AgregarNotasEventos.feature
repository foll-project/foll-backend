Feature: US029 - Agregar notas a eventos de caídas pasadas
Como cuidador, quiero agregar notas de texto a un evento de caída pasado "Se tropezó con la alfombra", para tener contexto al ir al médico.

    Scenario: Agregar nota a un evento de caída
        Given que el cuidador visualiza el historial de eventos del adulto mayor
        When selecciona un evento de caída pasado con ID 50 y agrega la nota "Se tropezó con la alfombra"
        Then el sistema guarda el comentario asociado al ID del incidente
        And lo muestra en el detalle del evento

    Scenario: Visualización de notas en eventos históricos
        Given que existe un evento de caída con notas asociadas "Se tropezó con la alfombra"
        When el cuidador accede al detalle del incidente en el historial
        Then el sistema muestra la nota registrada junto con la información del evento