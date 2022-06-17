using UnityEngine;
using UnityEngine.UI;

namespace LTTDIT.Chat
{
    public class ChatMessage : MonoBehaviour
    {
        [SerializeField] private Text message;

        public void SetText(string new_text)
        {
            message.text = new_text;
        }
    }
}
