using UnityEngine;
using UnityEngine.UI;

namespace LTTDIT.Chat
{
    public class ChatRules : MonoBehaviour
    {
        [SerializeField] private ChatMessage chatMessagePrefab;

        [SerializeField] private InputField sendableInputField;

        [SerializeField] private Transform chatPanelTransform;

        private int messageNumber = 0;

        private void Start()
        {
            messageNumber = 0;
            Net.NetScript1.instance.SetGetManuallySendableMessageDelegate(GetManuallySendableMessage);
            Net.NetScript1.instance.SetCreateChatMessageDelegate(CreateChatMessage);
        }

        private string GetManuallySendableMessage()
        {
            return sendableInputField.textComponent.text;
        }

        public void SendMessageButtonPressed()
        {
            Net.NetScript1.instance.SendMessageAsManually();
        }

        public void ExitRoom()
        {
            Net.NetScript1.instance.Exitt();
        }

        private void CreateChatMessage(string message)
        {
            ChatMessage chatMessagel = Instantiate(chatMessagePrefab, chatPanelTransform);
            chatMessagel.SetText(messageNumber.ToString() + ") " + message);
            messageNumber++;
        }
    }
}
