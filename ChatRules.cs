using UnityEngine;
using UnityEngine.UI;

namespace LTTDIT.Chat
{
    public class ChatRules : MonoBehaviour
    {
        [SerializeField] private ChatMessage chatMessagePrefab;

        [SerializeField] private InputField sendableInputField;

        [SerializeField] private Transform chatPanelTransform;

        public void ExitRoom()
        {
            Net.NetScript1.instance.Exitt();
        }

        private void CreateChatMessage(string message)
        {
            ChatMessage chatMessagel = Instantiate(chatMessagePrefab, chatPanelTransform);
            chatMessagel.SetText(message);
        }
    }
}
