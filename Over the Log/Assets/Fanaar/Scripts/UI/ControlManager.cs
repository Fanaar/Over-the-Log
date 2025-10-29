using UnityEngine;
using TMPro;

public class ControlManager : MonoBehaviour
{
    public static ControlManager Instance;

    public GameObject controlsPanel;
    public TextMeshProUGUI[] controlTexts;
    public float fadeSpeed = 2f;

    private bool[] unlockedControls;
    private float[] targetAlphas;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        unlockedControls = new bool[controlTexts.Length];
        targetAlphas = new float[controlTexts.Length];

        controlsPanel.SetActive(false);

        for (int i = 0; i < controlTexts.Length; i++)
        {
            controlTexts[i].gameObject.SetActive(true); // moet actief zijn voor alpha
            controlTexts[i].alpha = 0;
            targetAlphas[i] = 0;
        }
    }

    private void Update()
    {
        // fade alle controls
        for (int i = 0; i < controlTexts.Length; i++)
        {
            float newAlpha = Mathf.Lerp(controlTexts[i].alpha, targetAlphas[i], Time.deltaTime * fadeSpeed);
            controlTexts[i].alpha = newAlpha;
        }

        // panel aan/uit
        bool anyVisible = false;
        for (int i = 0; i < controlTexts.Length; i++)
        {
            if (controlTexts[i].alpha > 0.01f)
            {
                anyVisible = true;
                break;
            }
        }
        controlsPanel.SetActive(anyVisible);

        // Escape toggling
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ToggleControlsPanel();
        }
    }

    public void ShowControl(int index)
    {
        if (index < 0 || index >= controlTexts.Length) return;

        unlockedControls[index] = true;
        targetAlphas[index] = 1f;
    }

    public void HideControl(int index)
    {
        if (index < 0 || index >= controlTexts.Length) return;

        targetAlphas[index] = 0f;
    }

    public void ToggleControlsPanel()
    {
        if (controlsPanel.activeSelf)
        {
            // fade out alles
            for (int i = 0; i < controlTexts.Length; i++)
                targetAlphas[i] = 0f;
        }
        else
        {
            // fade in alleen unlocked
            for (int i = 0; i < controlTexts.Length; i++)
                targetAlphas[i] = unlockedControls[i] ? 1f : 0f;

            // zorg dat panel actief is zodat alpha zichtbaar wordt
            controlsPanel.SetActive(true);
        }
    }
}
