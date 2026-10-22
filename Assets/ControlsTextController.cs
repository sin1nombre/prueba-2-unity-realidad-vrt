using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(TextMesh))]
public class ControlsTextController : MonoBehaviour
{
    [Header("Terrain Placement Settings")]
    public float heightAboveGround = 4.5f;
    public float characterSize = 0.45f; // Keeps the large readable size from the Editor

    private TextMesh _tm;

    void Start()
    {
        _tm = GetComponent<TextMesh>();
        _tm.characterSize = characterSize;
        _tm.alignment = TextAlignment.Center;
        _tm.anchor = TextAnchor.UpperCenter;
        AdjustHeightToTerrain();
    }

    void AdjustHeightToTerrain()
    {
        Vector3 pos = transform.position;
        float groundY = GetGroundHeight(pos);
        transform.position = new Vector3(pos.x, groundY + heightAboveGround, pos.z);
    }

    public static float GetGroundHeight(Vector3 worldPos)
    {
        Ray ray = new Ray(new Vector3(worldPos.x, 500f, worldPos.z), Vector3.down);
        RaycastHit[] hits = Physics.RaycastAll(ray, 1000f);

        float highestTerrainY = 0f;
        bool foundTerrain = false;

        foreach (RaycastHit hit in hits)
        {
            if (hit.collider is TerrainCollider)
            {
                if (!foundTerrain || hit.point.y > highestTerrainY)
                {
                    highestTerrainY = hit.point.y;
                    foundTerrain = true;
                }
            }
        }

        if (foundTerrain) return highestTerrainY;
        if (Terrain.activeTerrain != null) return Terrain.activeTerrain.SampleHeight(worldPos);
        return 0f;
    }

    void Update()
    {
        if (_tm == null) return;

        bool isEditor = Application.isEditor;
        Gamepad pad = Gamepad.current;
        string scoreLine = $"★ Tesoros Recolectados: {ObjectController.CollectedCount} ★\n";

        if (pad != null)
        {
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
                           $"Interactuar Tesoro: Mirar + Círculo (O) / R2";
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
                           $"Interactuar Tesoro: Mirar + Botón B / RT";
            }
        }
        else
        {
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
                           $"Interactuar Tesoro: Mirar + Tecla E / Enter";
            }
            else
            {
                _tm.text = "[Modo: Android VR (Visor Cardboard)]\n" +
                           "Dispositivo: Giroscopio (Sin Mando conectado)\n" +
                           "----------------------------------------\n" +
                           "Mirar alrededor: Giroscopio (Mover cabeza)\n" +
                           $"Interactuar Tesoro: Mirar al objeto\n" +
                           "(Conecta un mando Bluetooth para caminar)";
            }
        }
    }
}