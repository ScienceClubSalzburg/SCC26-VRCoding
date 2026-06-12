using TMPro;
using UnityEngine;

[ExecuteAlways]
public class PaperPileBuilder : MonoBehaviour
{
    private const string GeneratedPrefix = "Generated_";

    [Header("Pile")]
    [SerializeField] private bool rebuild;
    [SerializeField] private bool collidersEnabled;
    [SerializeField] private float paperThickness = 0.006f;

    [Header("Letters")]
    [SerializeField] private string letters = "DPHOLH";
    [SerializeField] private float letterFontSize = 0.22f;
    [SerializeField] private Color letterColor = new Color(0.08f, 0.06f, 0.04f, 1f);

    private Material[] paperMaterials;

    private readonly Color[] paperColors =
    {
        new Color(0.98f, 0.88f, 0.58f, 1f),
        new Color(0.98f, 0.62f, 0.64f, 1f),
        new Color(0.58f, 0.78f, 1f, 1f),
        new Color(0.62f, 0.92f, 0.62f, 1f),
        new Color(0.86f, 0.68f, 1f, 1f),
        new Color(1f, 0.96f, 0.82f, 1f),
    };

    private readonly ScrapData[] scraps =
    {
        new ScrapData(new Vector3(-0.26f, 0.003f, -0.10f), new Vector3(0.16f, 1f, 0.07f), new Vector3(0f, 28f, 0f), 0),
        new ScrapData(new Vector3(-0.16f, 0.006f, 0.08f), new Vector3(0.21f, 1f, 0.055f), new Vector3(0f, -18f, 0f), 1),
        new ScrapData(new Vector3(-0.04f, 0.004f, -0.17f), new Vector3(0.13f, 1f, 0.09f), new Vector3(0f, 62f, 0f), 2),
        new ScrapData(new Vector3(0.12f, 0.006f, -0.08f), new Vector3(0.19f, 1f, 0.06f), new Vector3(0f, -46f, 0f), 3),
        new ScrapData(new Vector3(0.24f, 0.004f, 0.06f), new Vector3(0.14f, 1f, 0.08f), new Vector3(0f, 16f, 0f), 4),
        new ScrapData(new Vector3(0.03f, 0.008f, 0.18f), new Vector3(0.24f, 1f, 0.05f), new Vector3(0f, 76f, 0f), 5),
        new ScrapData(new Vector3(-0.31f, 0.005f, 0.15f), new Vector3(0.11f, 1f, 0.045f), new Vector3(0f, -72f, 0f), 2),
        new ScrapData(new Vector3(0.31f, 0.006f, -0.17f), new Vector3(0.12f, 1f, 0.05f), new Vector3(0f, 38f, 0f), 1),
        new ScrapData(new Vector3(-0.10f, 0.009f, -0.01f), new Vector3(0.34f, 1f, 0.035f), new Vector3(0f, 7f, 0f), 4),
        new ScrapData(new Vector3(0.13f, 0.010f, 0.10f), new Vector3(0.31f, 1f, 0.035f), new Vector3(0f, -31f, 0f), 0),
        new ScrapData(new Vector3(-0.24f, 0.011f, -0.22f), new Vector3(0.22f, 1f, 0.032f), new Vector3(0f, 13f, 0f), 3),
        new ScrapData(new Vector3(0.22f, 0.012f, 0.22f), new Vector3(0.25f, 1f, 0.032f), new Vector3(0f, -10f, 0f), 5),
    };

    private readonly ScrapData[] letterScraps =
    {
        new ScrapData(new Vector3(-0.22f, 0.018f, 0.00f), new Vector3(0.18f, 1f, 0.16f), new Vector3(0f, -20f, 0f), 5),
        new ScrapData(new Vector3(-0.08f, 0.020f, 0.13f), new Vector3(0.18f, 1f, 0.16f), new Vector3(0f, 18f, 0f), 0),
        new ScrapData(new Vector3(0.08f, 0.019f, -0.02f), new Vector3(0.18f, 1f, 0.16f), new Vector3(0f, -7f, 0f), 1),
        new ScrapData(new Vector3(0.22f, 0.021f, 0.10f), new Vector3(0.18f, 1f, 0.16f), new Vector3(0f, 31f, 0f), 2),
        new ScrapData(new Vector3(-0.02f, 0.022f, -0.20f), new Vector3(0.18f, 1f, 0.16f), new Vector3(0f, 44f, 0f), 3),
        new ScrapData(new Vector3(0.26f, 0.023f, -0.14f), new Vector3(0.18f, 1f, 0.16f), new Vector3(0f, -34f, 0f), 4),
    };

    private void OnEnable()
    {
        Rebuild();
    }

    private void OnValidate()
    {
        if (!rebuild)
        {
            return;
        }

        rebuild = false;
        Rebuild();
    }

    private void Rebuild()
    {
        EnsureMaterials();
        ClearGeneratedChildren();

        for (int i = 0; i < scraps.Length; i++)
        {
            CreatePaperScrap($"{GeneratedPrefix}PaperScrap_{i + 1:00}", scraps[i], false, '\0');
        }

        for (int i = 0; i < letterScraps.Length; i++)
        {
            char letter = i < letters.Length ? letters[i] : '?';
            CreatePaperScrap($"{GeneratedPrefix}LetterScrap_{letter}_{i + 1:00}", letterScraps[i], true, letter);
        }
    }

    private void EnsureMaterials()
    {
        if (paperMaterials == null || paperMaterials.Length != paperColors.Length)
        {
            paperMaterials = new Material[paperColors.Length];
        }

        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null)
        {
            shader = Shader.Find("Standard");
        }

        for (int i = 0; i < paperMaterials.Length; i++)
        {
            if (paperMaterials[i] == null)
            {
                paperMaterials[i] = new Material(shader)
                {
                    name = $"PaperPile_Color_{i + 1}"
                };
            }

            paperMaterials[i].color = paperColors[i];
            if (paperMaterials[i].HasProperty("_BaseColor"))
            {
                paperMaterials[i].SetColor("_BaseColor", paperColors[i]);
            }
        }
    }

    private void CreatePaperScrap(string objectName, ScrapData data, bool hasLetter, char letter)
    {
        GameObject scrap = new GameObject(objectName);
        scrap.transform.SetParent(transform, false);
        scrap.transform.localPosition = data.Position;
        scrap.transform.localEulerAngles = data.EulerAngles;

        GameObject paperMesh = GameObject.CreatePrimitive(PrimitiveType.Cube);
        paperMesh.name = $"{GeneratedPrefix}PaperMesh";
        paperMesh.transform.SetParent(scrap.transform, false);
        paperMesh.transform.localPosition = Vector3.zero;
        paperMesh.transform.localRotation = Quaternion.identity;
        paperMesh.transform.localScale = new Vector3(data.Scale.x, paperThickness, data.Scale.z);

        if (paperMesh.TryGetComponent(out Renderer renderer))
        {
            renderer.sharedMaterial = paperMaterials[Mathf.Abs(data.MaterialIndex) % paperMaterials.Length];
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;
        }

        if (paperMesh.TryGetComponent(out Collider collider))
        {
            collider.enabled = collidersEnabled;
        }

        if (hasLetter)
        {
            CreateLetter(scrap.transform, letter);
        }
    }

    private void CreateLetter(Transform parent, char letter)
    {
        GameObject textObject = new GameObject($"{GeneratedPrefix}Letter_{letter}", typeof(RectTransform), typeof(TextMeshPro));
        textObject.transform.SetParent(parent, false);
        textObject.transform.localPosition = new Vector3(0f, paperThickness * 0.8f, 0f);
        textObject.transform.localEulerAngles = new Vector3(90f, 0f, 0f);
        textObject.transform.localScale = Vector3.one;

        RectTransform rect = textObject.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(0.22f, 0.18f);

        TextMeshPro tmp = textObject.GetComponent<TextMeshPro>();
        tmp.text = letter.ToString();
        tmp.font = TMP_Settings.defaultFontAsset;
        tmp.fontSize = letterFontSize;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = letterColor;
        tmp.enableWordWrapping = false;
        tmp.richText = false;
    }

    private void ClearGeneratedChildren()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Transform child = transform.GetChild(i);
            if (!child.name.StartsWith(GeneratedPrefix))
            {
                continue;
            }

            if (Application.isPlaying)
            {
                Destroy(child.gameObject);
            }
            else
            {
                DestroyImmediate(child.gameObject);
            }
        }
    }

    private struct ScrapData
    {
        public ScrapData(Vector3 position, Vector3 scale, Vector3 eulerAngles, int materialIndex)
        {
            Position = position;
            Scale = scale;
            EulerAngles = eulerAngles;
            MaterialIndex = materialIndex;
        }

        public readonly Vector3 Position;
        public readonly Vector3 Scale;
        public readonly Vector3 EulerAngles;
        public readonly int MaterialIndex;
    }
}
