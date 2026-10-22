using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class ObjectController : MonoBehaviour
{
    public Material InactiveMaterial;
    public Material GazedAtMaterial;

    // Global counter shared across all 3 treasure shapes
    public static int CollectedCount = 0;
    private static LineRenderer _guideTrail;
    private static Coroutine _trailCoroutine;
    private static TextMesh _hudCounterText;

    private Renderer _myRenderer;
    private bool _isGazedAt = false;

    public void Start()
    {
        _myRenderer = GetComponent<Renderer>();
        SetMaterial(false);

        CreateHUDCounterIfNeeded();

        if (gameObject.activeSelf && CollectedCount == 0)
        {
            SnapParentAboveTerrain(transform.parent.position, 1.5f);
        }
    }

    private void CreateHUDCounterIfNeeded()
    {
        if (_hudCounterText != null || Camera.main == null) return;

        GameObject hudObj = new GameObject("TreasureCounterHUD");
        hudObj.transform.SetParent(Camera.main.transform, false);
        hudObj.transform.localPosition = new Vector3(0f, 0.32f, 1.5f);
        hudObj.transform.localRotation = Quaternion.identity;

        _hudCounterText = hudObj.AddComponent<TextMesh>();
        _hudCounterText.characterSize = 0.045f;
        _hudCounterText.fontSize = 32;
        _hudCounterText.alignment = TextAlignment.Center;
        _hudCounterText.anchor = TextAnchor.UpperCenter;
        _hudCounterText.color = new Color(1f, 0.9f, 0.2f);
        UpdateHUDText();
    }

    private void UpdateHUDText()
    {
        if (_hudCounterText != null)
        {
            _hudCounterText.text = $"★ Tesoros: {CollectedCount}";
        }
    }

    public void Update()
    {
        transform.Rotate(Vector3.up * 45f * Time.deltaTime, Space.Self);

        if (_isGazedAt)
        {
            bool interactPressed = false;

            if (Keyboard.current != null && (Keyboard.current.eKey.wasPressedThisFrame || Keyboard.current.enterKey.wasPressedThisFrame))
            {
                interactPressed = true;
            }

            if (Gamepad.current != null && (Gamepad.current.buttonEast.wasPressedThisFrame || Gamepad.current.rightTrigger.wasPressedThisFrame))
            {
                interactPressed = true;
            }

            if (interactPressed)
            {
                OnPointerClick();
            }
        }
    }

    public static float SamplePureTerrainHeight(Vector3 worldPos, out bool isOnValidTerrain)
    {
        Terrain[] terrains = Terrain.activeTerrains;
        foreach (Terrain t in terrains)
        {
            Vector3 tPos = t.transform.position;
            Vector3 tSize = t.terrainData.size;

            if (worldPos.x >= tPos.x && worldPos.x <= tPos.x + tSize.x &&
                worldPos.z >= tPos.z && worldPos.z <= tPos.z + tSize.z)
            {
                isOnValidTerrain = true;
                return t.SampleHeight(worldPos) + tPos.y;
            }
        }

        if (Terrain.activeTerrain != null)
        {
            isOnValidTerrain = true;
            return Terrain.activeTerrain.SampleHeight(worldPos) + Terrain.activeTerrain.transform.position.y;
        }

        isOnValidTerrain = false;
        return 0f;
    }

    private bool IsBlockedByTree(Vector3 candidatePos, float terrainY)
    {
        Collider[] overlaps = Physics.OverlapSphere(candidatePos, 2.2f);
        foreach (Collider col in overlaps)
        {
            if (!(col is TerrainCollider) && col.transform.parent != transform.parent)
            {
                return true;
            }
        }

        Ray ray = new Ray(new Vector3(candidatePos.x, terrainY + 30f, candidatePos.z), Vector3.down);
        if (Physics.Raycast(ray, out RaycastHit hit, 35f))
        {
            if (hit.point.y > terrainY + 1.0f)
            {
                return true;
            }
        }

        return false;
    }

    private void SnapParentAboveTerrain(Vector3 targetXZ, float heightOffset)
    {
        float groundY = SamplePureTerrainHeight(targetXZ, out _);
        transform.parent.position = new Vector3(targetXZ.x, groundY + heightOffset, targetXZ.z);
    }

    public void TeleportRandomly()
    {
        Vector3 oldPosition = transform.parent.position;

        CollectedCount++;
        UpdateHUDText();

        Vector3 finalNewPosition = oldPosition;
        Vector3 searchCenter = Camera.main != null ? Camera.main.transform.position : oldPosition;

        for (int attempt = 0; attempt < 25; attempt++)
        {
            float angleRad = Random.Range(0f, Mathf.PI * 2f);
            float distance = Random.Range(10f, 24f);

            Vector3 candidateXZ = new Vector3(
                searchCenter.x + Mathf.Cos(angleRad) * distance,
                0f,
                searchCenter.z + Mathf.Sin(angleRad) * distance
            );

            float groundY = SamplePureTerrainHeight(candidateXZ, out bool onTerrain);
            if (!onTerrain) continue;

            float hoverHeight = Random.Range(1.3f, 2.0f);
            Vector3 candidatePos = new Vector3(candidateXZ.x, groundY + hoverHeight, candidateXZ.z);

            if (!IsBlockedByTree(candidatePos, groundY))
            {
                finalNewPosition = candidatePos;
                break;
            }

            finalNewPosition = candidatePos;
        }

        transform.parent.position = finalNewPosition;

        ShowDirectionalTrail(oldPosition, finalNewPosition);

        int sibIdx = transform.GetSiblingIndex();
        int numSibs = transform.parent.childCount;
        sibIdx = (sibIdx + Random.Range(1, numSibs)) % numSibs;
        GameObject randomSib = transform.parent.GetChild(sibIdx).gameObject;

        _isGazedAt = false;
        SetMaterial(false);
        randomSib.SetActive(true);

        if (randomSib != gameObject)
        {
            StartCoroutine(DeactivateAfterFrame());
        }
    }

    private IEnumerator DeactivateAfterFrame()
    {
        yield return null;
        gameObject.SetActive(false);
    }

    private void ShowDirectionalTrail(Vector3 fromPos, Vector3 toPos)
    {
        if (_guideTrail == null)
        {
            GameObject trailObj = new GameObject("TreasureGuideTrail");
            _guideTrail = trailObj.AddComponent<LineRenderer>();
            _guideTrail.material = new Material(Shader.Find("Sprites/Default"));
            _guideTrail.startWidth = 0.28f;
            _guideTrail.endWidth = 0.02f;
            _guideTrail.numCapVertices = 4;
        }

        MonoBehaviour runner = transform.parent.GetComponent<MonoBehaviour>();
        if (runner == null) runner = this;

        float guideFraction = 0.45f;
        Vector3 partialEndXZ = Vector3.Lerp(fromPos, toPos, guideFraction);

        int segments = 16;
        _guideTrail.positionCount = segments;

        for (int i = 0; i < segments; i++)
        {
            float t = i / (float)(segments - 1);
            Vector3 pointXZ = Vector3.Lerp(fromPos, partialEndXZ, t);
            float groundY = SamplePureTerrainHeight(pointXZ, out _);

            float arcBoost = Mathf.Sin(t * Mathf.PI) * 1.2f;
            float pointY = Mathf.Max(groundY + 0.8f + arcBoost, Mathf.Lerp(fromPos.y, groundY + 1.0f, t));

            _guideTrail.SetPosition(i, new Vector3(pointXZ.x, pointY, pointXZ.z));
        }

        _guideTrail.enabled = true;
        _guideTrail.startColor = new Color(1f, 0.85f, 0.2f, 0.9f);
        _guideTrail.endColor = new Color(0.2f, 0.9f, 1f, 0.0f); 
    }

    public void OnPointerEnter()
    {
        _isGazedAt = true;
        SetMaterial(true);
    }

    public void OnPointerExit()
    {
        _isGazedAt = false;
        SetMaterial(false);
    }

    public void OnPointerClick()
    {
        TeleportRandomly();
    }

    private void SetMaterial(bool gazedAt)
    {
        if (_myRenderer != null && InactiveMaterial != null && GazedAtMaterial != null)
        {
            _myRenderer.material = gazedAt ? GazedAtMaterial : InactiveMaterial;
        }
    }
}