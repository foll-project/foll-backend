Feature: US036 - Acceder al panel de soporte técnico en la aplicación
Como cuidador, quiero acceder a un panel de soporte técnico o de preguntas frecuentes (FAQ) dentro de la aplicación, para resolver dudas sobre el uso del dispositivo.

    Scenario: Acceso al panel de soporte técnico
        Given que el cuidador se encuentra dentro de la aplicación móvil
        When accede a la sección de soporte o ayuda
        Then el sistema muestra un panel con preguntas frecuentes y artículos básicos para la resolución de problemas

    Scenario: Búsqueda dentro del centro de ayuda
        Given que el cuidador se encuentra en la sección de soporte
        When ingresa una palabra clave en el buscador, como "batería"
        Then el sistema filtra y muestra los artículos de ayuda relacionados con la búsqueda