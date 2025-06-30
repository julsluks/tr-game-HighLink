using UnityEngine;
using TMPro;

public class TextColorAnimation : MonoBehaviour
{
    public Color[] colors;
    public float colorChangeSpeed = 1f;
    private TMP_Text textComponent;
    private int currentColorIndex = 0;
    private float t = 0f;

    void Start()
    {
        textComponent = GetComponent<TMP_Text>();
        if (colors.Length > 0) textComponent.color = colors[0];
    }

    void Update()
    {
        if (colors.Length < 2) return;
        
        t += Time.deltaTime * colorChangeSpeed;
        textComponent.color = Color.Lerp(colors[currentColorIndex], 
            colors[(currentColorIndex + 1) % colors.Length], 
            t);
        
        if (t >= 1f)
        {
            t = 0f;
            currentColorIndex = (currentColorIndex + 1) % colors.Length;
        }
    }
}