using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace LTTDIT.Net
{
    public class OpeningScript : MonoBehaviour
    {
        [SerializeField] private ChooseIP JoinButtonPrefab;

        [SerializeField] private Transform joinButtonsTransform;

        [SerializeField] private Text nicknameButtonText;
        [SerializeField] private InputField nicknameInputField;
        [SerializeField] private Text invitationText;

        [SerializeField] private GameObject nicknamePanel;
        [SerializeField] private GameObject applicationsPanel;
        [SerializeField] private GameObject joinToPanel;
        [SerializeField] private GameObject invitationPanel;

        [SerializeField] private TicTacToe.TicTacToeSettings ticTacToeSettings;

        [SerializeField] private Button ticTacToeAppButton;

        private List<ChooseIP> joinButtons = new List<ChooseIP>();
        private List<ChooseIP> buttonsToDelete = new List<ChooseIP>();

        private void Start()
        {
            NetScript1.instance.SetCreateButtonDelegate(CreateJoinButton);
            NetScript1.instance.SetInvitationReceivedDelegate(InvitationReceived);
            if (!NetScript1.instance.HasNickname()) ChangeNicknameButtonPressed();
            else nicknameButtonText.text = NetScript1.instance.GetNickname();
            ticTacToeAppButton.onClick.AddListener(ticTacToeSettings.ActivatePanel);
        }

        private void Update()
        {
            UpdateJoinButtons();
        }

        private void UpdateJoinButtons()
        {
            foreach (ChooseIP joinButton in joinButtons)
            {
                joinButton.AddTime(Time.deltaTime);
                if (joinButton.IsGonnaDeleted())
                {
                    joinButton.Delete();
                    buttonsToDelete.Add(joinButton);
                }
            }
            foreach (ChooseIP deletableButton in buttonsToDelete)
            {
                joinButtons.Remove(deletableButton);
            }
            buttonsToDelete.Clear();
        }

        public void ConfirmNicknameButtonPressed()
        {
            if (NetScript1.instance.SetNickname(nicknameInputField.textComponent.text))
            {
                nicknameButtonText.text = nicknameInputField.textComponent.text;
                nicknamePanel.SetActive(false);
            }
        }

        public void ChangeNicknameButtonPressed()
        {
            nicknamePanel.SetActive(true);
        }

        public void CreateButtonPressed()
        {
            applicationsPanel.SetActive(true);
        }

        public void BackFromCreatePanelButtonPressed()
        {
            applicationsPanel.SetActive(false);
        }

        public void JoinButtonPressed()
        {
            joinToPanel.SetActive(true);
            NetScript1.instance.JoinRoomPressed();
        }

        public void BackFromJoinPanelButtonPressed()
        {
            Net.NetScript1.instance.Exitt();
            joinToPanel.SetActive(false);
        }

        public void ChatButtonPressed()
        {
            NetScript1.instance.ChatSelected();
        }

        public void QiutButtonPressed()
        {
            NetScript1.instance.Exitt();
            Application.Quit();
        }

        private bool JoinButtonsListContainsAndReliving(string ip, Information.Applications app)
        {
            foreach (ChooseIP joinButton in joinButtons)
            {
                if ((joinButton.GetIpAddress() == ip) && (joinButton.GetApplication() == app))
                {
                    joinButton.PacketReceived();
                    return true;
                }
            }
            return false;
        }

        private void CreateJoinButton(string ip, string nick, Information.Applications app, NetScript1.CreateButtonDelegate createButtonDelegate)
        {
            if (!JoinButtonsListContainsAndReliving(ip, app))
            {
                ChooseIP joinButton = Instantiate(JoinButtonPrefab, joinButtonsTransform);
                joinButton.SetJoinButton(ip, nick, app);
                joinButton.SetDelegateWithData(createButtonDelegate);
                joinButtons.Add(joinButton);
            }
        }

        private void InvitationReceived(string application, string nickname)
        {
            invitationPanel.SetActive(true);
            invitationText.text = "App: " + application + ",  Nickname: " + nickname;
        }

        public void AcceptInvitation()
        {
            invitationPanel.SetActive(false);
            NetScript1.instance.AcceptInvitation();
        }

        public void RefuseInvitation()
        {
            invitationPanel.SetActive(false);
            NetScript1.instance.RefuseInvitation();
        }
    }
}
