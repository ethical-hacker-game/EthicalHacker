using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class TextPrinter : MonoBehaviour
{
    [SerializeField] private float printDelay = 0.1f;

    [Space]
    [SerializeField] private TextMeshProUGUI bgTextComponent;
    [SerializeField] private TextMeshProUGUI mainTextComponent;
    
    private bool initialized;
    private string fullText;
    private int currentIndex;
    private float timer;
    private UnityAction onPrintingFinished;

    public void PrintText(string text, UnityAction printingFinishedAction)
    {
        if (initialized)
            return;

        fullText = text;
        onPrintingFinished = printingFinishedAction;

        currentIndex = 0;
        bgTextComponent.text = fullText;
        mainTextComponent.text = "";
        timer = 0f;

        initialized = true;
        this.enabled = true;

        if(string.IsNullOrEmpty(fullText))
        {
            initialized = false;
            this.enabled = false;
            onPrintingFinished?.Invoke();
        }
    }

    private void Update()
    {
        if(string.IsNullOrEmpty(fullText))
        {
            initialized = false;
            this.enabled = false;
            return;
        }

        if(currentIndex >= fullText.Length || initialized == false)
        {
            initialized = false;
            this.enabled = false;
            return;
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            mainTextComponent.text = fullText;
            onPrintingFinished?.Invoke();
            initialized = false;
            this.enabled = false;
            return;
        }

        timer += Time.deltaTime;

        if (timer < printDelay)
            return;

        string textToAdd = fullText[currentIndex].ToString();

        if(textToAdd == "<")
        {
            int closingTagIndex = fullText.IndexOf('>', currentIndex);
        
            if (closingTagIndex != -1)
                textToAdd = fullText.Substring(currentIndex, closingTagIndex - currentIndex + 1);
        }

        mainTextComponent.text += textToAdd;
        currentIndex += textToAdd.Length;
 
        timer = 0f;

        if (currentIndex >= fullText.Length)
            onPrintingFinished?.Invoke();
    }
}
