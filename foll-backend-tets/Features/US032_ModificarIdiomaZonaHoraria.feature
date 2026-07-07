Feature: US032 - Modificar idioma y zona horaria de la aplicación
Como usuario, quiero modificar el idioma y la zona horaria de mi aplicación, para adaptarla a mis preferencias.

    Scenario: Cambio de idioma y zona horaria
        Given que el usuario se encuentra en la sección de configuración de la aplicación
        When modifica el idioma a "English" y selecciona una nueva zona horaria "UTC-5"
        Then el sistema actualiza el idioma de la interfaz y la hora mostrada de acuerdo con las preferencias seleccionadas

    Scenario: Persistencia de las preferencias del usuario
        Given que el usuario ha guardado un nuevo idioma "English" y una nueva zona horaria "UTC-5"
        When cierra y vuelve a abrir la aplicación
        Then el sistema mantiene las preferencias configuradas previamente sin necesidad de realizar una nueva configuración