using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(TextMesh))]
public class ControlsTextController : MonoBehaviour
{
    private TextMesh _tm;

    void Start()
    {
        _tm = GetComponent<TextMesh>();
        _tm.characterSize = 0.12f;
        _tm.alignment = TextAlignment.Center;
        _tm.anchor = TextAnchor.MiddleCenter;
    }

    void Update()
    {
        if (_tm == null) return;

        bool isEditor = Application.isEditor;
        Gamepad pad = Gamepad.current;

        if (pad != null)
        {
            // Check if the connected controller is a PlayStation DualShock / DualSense
            string padName = (pad.displayName + " " + pad.name + " " + pad.description.interfaceName).ToLower();
            bool isPlayStation = padName.Contains("dualshock") || padName.Contains("dualsense") || 
                                 padName.Contains("playstation") || padName.Contains("sony") || 
                                 padName.Contains("wireless controller");

            string platformHeader = isEditor ? "[Modo: PC Editor + Mando]" : "[Modo: Android VR + Mando]";

            if (isPlayStation)
            {
                _tm.text = $"{platformHeader}\n" +
                           "Dispositivo: Mando PlayStation\n" +
                           "----------------------------------------\n" +
                           "Caminar: Stick Izquierdo (L3)\n" +
                           "Mirar (VR): Giroscopio / Stick Derecho (R3)\n" +
                           "Girar cuerpo: Stick Derecho (R3)\n" +
                           "Saltar: Botón Cruz (X) o R1\n" +
                           "Correr (Sprint): Cuadrado (□), L1 o L2\n" +
                           "Interactuar Tesoro: Mirar + Círculo (O) / R2";
            }
            else
            {
                _tm.text = $"{platformHeader}\n" +
                           $"Dispositivo: Mando Xbox / {pad.displayName}\n" +
                           "----------------------------------------\n" +
                           "Caminar: Stick Izquierdo\n" +
                           "Mirar (VR): Giroscopio / Stick Derecho\n" +
                           "Girar cuerpo: Stick Derecho\n" +
                           "Saltar: Botón A o RB\n" +
                           "Correr (Sprint): Botón X, LB o LT\n" +
                           "Interactuar Tesoro: Mirar + Botón B / RT";
            }
        }
        else
        {
            // No gamepad connected
            if (isEditor)
            {
                _tm.text = "[Modo: Pruebas en PC (Sin Mando)]\n" +
                           "Dispositivo: Teclado\n" +
                           "----------------------------------------\n" +
                           "Caminar: W, A, S, D\n" +
                           "Mirar alrededor: Flechas (↑, ↓, ←, →)\n" +
                           "Girar cuerpo: Flechas (←, →)\n" +
                           "Saltar: Barra Espaciadora\n" +
                           "Correr (Sprint): Left Shift\n" +
                           "Interactuar Tesoro: Mirar + Tecla E / Enter";
            }
            else
            {
                _tm.text = "[Modo: Android VR (Visor Cardboard)]\n" +
                           "Dispositivo: Giroscopio (Sin Mando conectado)\n" +
                           "----------------------------------------\n" +
                           "Mirar alrededor: Giroscopio (Mover cabeza)\n" +
                           "Interactuar Tesoro: Mirar al objeto\n" +
                           "(Conecta un mando Xbox o PlayStation por Bluetooth para caminar)";
            }
        }
    }
}