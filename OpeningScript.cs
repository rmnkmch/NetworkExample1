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

        [SerializeField] private GameObject nicknamePanel;
        [SerializeField] private GameObject applicationsPanel;
        [SerializeField] private GameObject joinToPanel;

        private List<ChooseIP> joinButtons = new List<ChooseIP>();
        private List<ChooseIP> buttonsToDelete = new List<ChooseIP>();

        private void Start()
        {
            NetScript1.instance.SetCreateJoinButtonDelegate(CreateJoinButton);
            if (!NetScript1.instance.HasNickname()) ChangeNicknameButtonPressed();
            else nicknameButtonText.text = NetScript1.instance.GetNickname();
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
            joinToPanel.SetActive(false);
        }

        public void ChatButtonPressed()
        {
            NetScript1.instance.ChatSelected();
        }

        public void TicTacToeButtonPressed()
        {
            NetScript1.instance.TicTacToeSelected();
        }

        public void QiutButtonPressed()
        {
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

        private void CreateJoinButton(string ip, string nick, Information.Applications app, NetScript1.CreateJoinButtonDelegate createJoinButtonDelegate)
        {
            if (!JoinButtonsListContainsAndReliving(ip, app))
            {
                ChooseIP joinButton = Instantiate(JoinButtonPrefab, joinButtonsTransform);
                joinButton.SetJoinButton(ip, nick, app);
                joinButton.SetDelegateWithData(createJoinButtonDelegate);
                joinButtons.Add(joinButton);
            }
        }
    }
}
