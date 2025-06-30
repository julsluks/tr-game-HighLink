using UnityEngine;
using TMPro;

public class TextPulseAnimation : MonoBehaviour
{
    public float pulseSpeed = 1.5f;
    public float sizeVariation = 0.5f;
    private TMP_Text textComponent;
    private float baseSize;

    void Start()
    {
        textComponent = GetComponent<TMP_Text>();
        baseSize = textComponent.fontSize;
    }

    void Update()
    {
        // Crea un efecto de pulso cambiando el tamaño
        textComponent.fontSize = baseSize + Mathf.Sin(Time.time * pulseSpeed) * sizeVariation;
    }
}