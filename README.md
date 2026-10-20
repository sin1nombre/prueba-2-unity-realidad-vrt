# Entorno 3D VR en Android con Google Cardboard y Gamepad

Proyecto de realidad virtual desarrollado en **Unity 6 (`6000.3.21f1 LTS`)** utilizando **Universal Render Pipeline (URP)** y el plugin oficial **Google Cardboard XR**. Permite explorar un entorno natural abierto (terreno con relieve, vegetación low-poly e iluminación diurna) en dispositivos Android utilizando el giroscopio del teléfono para la vista estereoscópica y un mando Bluetooth (Xbox o PlayStation) para el desplazamiento.

---

## Características Principales

* **Renderizado Estereoscópico VR:** Integración con Google Cardboard XR Plugin (`1.36.0`) bajo la API gráfica OpenGLES3.
* **Entorno 3D Abierto:** Terreno esculpido con capas de texturas (`TerrainLayers`), árboles 3D optimizados para móviles y cielo procedural soleado (`Skybox`).
* **Sistema de Movimiento Híbrido (`UnityEngine.InputSystem`):**
  * **En Android (VR):** Rotación libre de cámara en 360° mediante el giroscopio del dispositivo (`Tracked Pose Driver`) combinada con locomoción y giro mediante mando Bluetooth.
  * **En PC (Modo Editor):** Soporte completo de teclado (`WASD` + Flechas) y mando para pruebas rápidas sin necesidad de compilar en cada cambio.
* **Físicas con `CharacterController`:** Detección continua de suelo con gravedad personalizada, salto consistente en terrenos irregulares y sprint.

---

## Controles

El script `VRPlayerController.cs` utiliza la interfaz universal `Gamepad.current` del nuevo **Input System**, por lo que es compatible de forma nativa con mandos de **Xbox** y **PlayStation** sin configuración adicional.

| Acción | Mando Xbox | Mando PlayStation | Teclado (Pruebas en PC) |
| :--- | :--- | :--- | :--- |
| **Caminar / Desplazarse** | Stick Izquierdo | Stick Izquierdo (L3) | `W`, `A`, `S`, `D` |
| **Mirar alrededor (VR)** | Giroscopio | Giroscopio | Flechas (`↑`, `↓`, `←`, `→`) |
| **Girar cuerpo (Cámara H)** | Stick Derecho | Stick Derecho (R3) | Flechas (`←`, `→`) |
| **Saltar** | Botón **A** o **RB** | Botón **Cruz (X)** o **R1** | `Barra Espaciadora` |
| **Correr (Sprint)** | Botón **X**, **LB** o **LT** | **Cuadrado (□)**, **L1** o **L2** | `Left Shift` |

---

## Requisitos Técnicos y Dependencias

* **Motor:** Unity `6000.3.21f1 LTS` (con módulos *Android Build Support*, *OpenJDK* y *Android SDK & NDK Tools*).
* **Pipeline Gráfico:** Universal Render Pipeline (URP `17.3.0`).
* **Paquetes Principales:**
  * `com.google.xr.cardboard` (`1.36.0`) - Vía Git URL: `https://github.com/googlevr/cardboard-xr-plugin.git`
  * `com.unity.xr.management` (`4.5.4`)
  * `com.unity.inputsystem` (`1.20.1`)
* **Assets Visuales:**
  * *Low-Poly Simple Nature Pack*
  * *Terrain Sample Assets* (Texturas y capas de terreno)

---

## Estructura del Jugador en la Escena

```text
Player (CardboardStartup.cs)
 └── Capsule (CharacterController + VRPlayerController.cs)
      └── Main Camera (Camera + Tracked Pose Driver [Input System])
           └── CardboardReticlePointer
