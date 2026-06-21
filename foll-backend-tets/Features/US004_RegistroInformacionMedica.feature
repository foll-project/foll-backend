Feature: US004 - Registro de información médica de emergencia con acceso offline
Como cuidador, quiero registrar datos de emergencia (tipo de sangre, alergias, medicación) del adulto mayor, para tenerlos disponibles ante un accidente.

    Scenario: Actualización de datos de salud
        Given que el cuidador se encuentra en la sección de datos de emergencia del perfil del adulto mayor
        When ingresa el tipo de sangre "O+", alergias "Penicilina" y medicación "Enalapril 10mg" y guarda la información
        Then la aplicación registra los datos correctamente y los muestra disponibles en el perfil del adulto mayor