using UnityEngine;
using TMPro;
using System.Collections;

public class AdvancedTypewriterEffect : MonoBehaviour
{
    [Header("Configuración Principal")]
    [Tooltip("Velocidad de escritura (segundos por caracter)")]
    [SerializeField] private float typingSpeed = 1f; // Valor más lento por defecto
    [Tooltip("Tiempo de espera entre ciclos")]
    [SerializeField] private float delayBetweenLoops = 0.5f; // Tiempo aumentado
    public bool loopInfinitely = true;
    public int loopCount = 3;

    [Header("Efecto Cursor")]
    public bool showCursor = true;
    public string cursorChar = "|";
    [SerializeField] private float cursorBlinkSpeed = 0.5f; // Más lento
    public int endBlinkCount = 1;

    [Header("Efecto de Sonido")]
    public AudioClip typingSound;
    public AudioClip deleteSound;
    [Range(0, 1)] public float soundVolume = 0.7f;
    [SerializeField] private float soundDelay = 0.08f; // Controla frecuencia de sonidos
    private float lastSoundTime;

    [Header("Modo de Borrado")]
    public bool deleteLetterByLetter = true;
    [SerializeField] private float deleteSpeed = 0.3f; // Control independiente para borrado

    private TMP_Text textComponent;
    private string fullText;
    private AudioSource audioSource;
    private int currentLoop = 0;
    private bool isEffectRunning = false;

    void Start()
    {
        textComponent = GetComponent<TMP_Text>();
        fullText = textComponent.text;
        textComponent.text = "";

        InitializeAudio();
        StartEffect();
    }

    void InitializeAudio()
    {
        if (typingSound != null || deleteSound != null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.volume = soundVolume;
            audioSource.playOnAwake = false;
        }
    }

    public void StartEffect()
    {
        if (!isEffectRunning)
        {
            isEffectRunning = true;
            currentLoop = 0;
            StartCoroutine(TypeTextProcess());
        }
    }

    public void StopEffect()
    {
        StopAllCoroutines();
        isEffectRunning = false;
    }

    IEnumerator TypeTextProcess()
    {
        do
        {
            // Fase de escritura
            yield return StartCoroutine(TypeText());

            // Pausa con cursor
            yield return StartCoroutine(HandlePostTypingDelay());

            // Fase de borrado (excepto en último ciclo si no es infinito)
            if (ShouldDeleteText())
            {
                yield return StartCoroutine(DeleteText());
                yield return new WaitForSeconds(delayBetweenLoops * 0.5f);
            }

            currentLoop++;
        }
        while (ShouldContinueLooping());
    }

    IEnumerator TypeText()
    {
        textComponent.text = "";

        foreach (char c in fullText)
        {
            textComponent.text += c;

            if (showCursor)
            {
                textComponent.text += cursorChar;
            }

            TryPlaySound(typingSound);
            yield return new WaitForSeconds(typingSpeed);

            if (showCursor)
            {
                textComponent.text = textComponent.text.Remove(textComponent.text.Length - 1);
            }
        }
    }

    IEnumerator HandlePostTypingDelay()
    {
        if (showCursor)
        {
            yield return StartCoroutine(BlinkCursor(endBlinkCount));
        }
        else
        {
            yield return new WaitForSeconds(delayBetweenLoops);
        }
    }

    IEnumerator DeleteText()
    {
        if (deleteLetterByLetter)
        {
            while (textComponent.text.Length > 0)
            {
                textComponent.text = textComponent.text.Remove(textComponent.text.Length - 1);
                TryPlaySound(deleteSound);
                yield return new WaitForSeconds(deleteSpeed);
            }
        }
        else
        {
            TryPlaySound(deleteSound);
            textComponent.text = "";
            yield return null;
        }
    }

    IEnumerator BlinkCursor(int blinkTimes)
    {
        for (int i = 0; i < blinkTimes; i++)
        {
            textComponent.text += cursorChar;
            TryPlaySound(typingSound);
            yield return new WaitForSeconds(cursorBlinkSpeed);
            textComponent.text = textComponent.text.Remove(textComponent.text.Length - 1);
            yield return new WaitForSeconds(cursorBlinkSpeed);
        }
    }

    void TryPlaySound(AudioClip clip)
    {
        if (clip != null && audioSource != null && Time.time - lastSoundTime > soundDelay)
        {
            audioSource.PlayOneShot(clip);
            lastSoundTime = Time.time;
        }
    }

    bool ShouldDeleteText()
    {
        return loopInfinitely || currentLoop < loopCount - 1;
    }

    bool ShouldContinueLooping()
    {
        return loopInfinitely || currentLoop < loopCount;
    }

    public void RestartEffect()
    {
        StopEffect();
        StartEffect();
    }

    public void ChangeText(string newText)
    {
        fullText = newText;
        RestartEffect();
    }

    // Métodos para ajustar velocidad dinámicamente
    public void SetTypingSpeed(float newSpeed)
    {
        typingSpeed = Mathf.Clamp(newSpeed, 0.01f, 1f);
    }

    public void SetDeleteSpeed(float newSpeed)
    {
        deleteSpeed = Mathf.Clamp(newSpeed, 0.01f, 0.5f);
    }
}