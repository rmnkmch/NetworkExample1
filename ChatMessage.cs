using UnityEngine;
using UnityEngine.UI;

public class ChatMessage : MonoBehaviour
{
    [SerializeField] private Text chText;

    public void SetText(string newtext)
    {
        chText.text = newtext;
    }
}
