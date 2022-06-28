using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace LTTDIT.Chat
{
    public class ChatRules : MonoBehaviour
    {
        [SerializeField] private ChatMessage chatMessagePrefab;
        [SerializeField] private Net.InviteButton inviteButtonPrefab;

        [SerializeField] private InputField sendableInputField;

        [SerializeField] private GameObject invitePanel;

        [SerializeField] private Transform chatPanelTransform;
        [SerializeField] private Transform invitePanelTransform;

        private List<Net.InviteButton> inviteButtons = new List<Net.InviteButton>();
        private List<Net.InviteButton> buttonsToDelete = new  List<Net.InviteButton>();

        private int messageNumber = 0;

        private void Start()
        {
            messageNumber = 0;
            Net.NetScript1.instance.SetGetManuallySendableMessageDelegate(GetManuallySendableMessage);
            Net.NetScript1.instance.SetCreateChatMessageDelegate(CreateChatMessage);
            Net.NetScript1.instance.SetCreateButtonDelegate(CreateInviteButton);
            if (Net.NetScript1.instance.IsHost()) Net.NetScript1.instance.SendHiMessageChatHost();
            else if (Net.NetScript1.instance.IsClient()) Net.NetScript1.instance.SendHiMessageChatClient();
        }

        private string GetManuallySendableMessage()
        {
            return sendableInputField.textComponent.text;
        }

        public void SendMessageButtonPressed()
        {
            Net.NetScript1.instance.SendMessageAsManually();
        }

        public void InviteButtonPressed()
        {
            invitePanel.SetActive(!invitePanel.activeInHierarchy);
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

        private void Update()
        {
            UpdateInviteButtons();
        }

        private void UpdateInviteButtons()
        {
            foreach (Net.InviteButton inviteButton in inviteButtons)
            {
                inviteButton.AddTime(Time.deltaTime);
                if (inviteButton.IsGonnaDeleted())
                {
                    inviteButton.Delete();
                    buttonsToDelete.Add(inviteButton);
                }
            }
            foreach (Net.InviteButton delInviteButton in buttonsToDelete)
            {
                inviteButtons.Remove(delInviteButton);
            }
            buttonsToDelete.Clear();
        }

        private bool InviteButtonsListContainsAndReliving(string ip, Net.Information.Applications app)
        {
            foreach (Net.InviteButton inviteButton in inviteButtons)
            {
                if ((inviteButton.GetIpAddress() == ip) && (inviteButton.GetApplication() == app))
                {
                    inviteButton.PacketReceived();
                    return true;
                }
            }
            return false;
        }

        private void CreateInviteButton(string ip, string nick, Net.Information.Applications app, Net.NetScript1.CreateButtonDelegate createButtonDelegate)
        {
            if (!InviteButtonsListContainsAndReliving(ip, app))
            {
                Net.InviteButton inviteButton = Instantiate(inviteButtonPrefab, invitePanelTransform);
                inviteButton.SetButton(ip, nick, app);
                inviteButton.SetPressedDelegate(createButtonDelegate);
                inviteButtons.Add(inviteButton);
            }
        }
    }
}
