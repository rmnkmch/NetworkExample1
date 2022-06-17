using System;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace LTTDIT.Net
{
    public class NetScript1 : MonoBehaviour
    {
        [SerializeField] private Chat.ChatMessage chatMessagePrefab;
        [SerializeField] private ChooseIP joinButtonPrefab;
        [SerializeField] private RectTransform joinButtonTransform;

        [SerializeField] private InputField nicknameInput;
        [SerializeField] private GameObject nicknamePanel;
        [SerializeField] private GameObject chooseAppsPanel;
        [SerializeField] private GameObject chooseToJoinPanel;

        [SerializeField] private Text nickNameButton;

        private Text textSend;
        private RectTransform chatMessageTransform;
        private int messageNumber = 0;

        private delegate void Act();
        private Act act;
        private Act sendMessageAs;
        private Act availableCheck;

        private TcpListener tcpListener;
        private List<ClientObject> clientObjects = new List<ClientObject>();
        private TcpClient tcpClient;
        private NetworkStream networkStream;
        private const float availableCooldown = 3f;
        private float availableTime = 0f;

        private readonly IPAddress BroadcastIpAddress = IPAddress.Parse("255.255.255.255");
        private const int BroadcastUDPport = 55555;
        private const int ListenerTCPport = 55556;

        private string receivedUDPData = string.Empty;
        private float broadcastUDPTime = 0f;
        private const float broadcastUDPCooldown = 2f;
        private UdpClient udpClient;
        private UdpClient udpServer;
        private System.Threading.Thread udpReceiveThread;
        private IPAddress myIPAddress;
        private string myNickname = string.Empty;
        private Information.Applications application = Information.Applications.ApplicationError;
        private string receivedIp = string.Empty;
        private string receivedNickname = string.Empty;
        private Information.Applications receivedApplication = Information.Applications.ApplicationError;

        public static NetScript1 instance;

        private const int OpeningSceneId = 0;
        private const int ChatSceneId = 1;
        private const int TicTacToeSceneId = 2;

        public void SetNickname()
        {
            if (nicknameInput.textComponent.text.Length < 3) return;
            myNickname = nicknameInput.textComponent.text;
            nickNameButton.text = myNickname;
            nicknamePanel.SetActive(false);
        }

        public void ShowJoinButton(string ip, string nick, Information.Applications app)
        {
            ChooseIP joinButton = Instantiate(joinButtonPrefab, joinButtonTransform);
            joinButton.SetJoinButton(ip, nick, app);
        }

        public void ChangeNickname()
        {
            nicknamePanel.SetActive(true);
        }

        public void Quit()
        {
            Application.Quit();
        }

        public void Join()
        {
            chooseToJoinPanel.SetActive(true);
            StartReceiveUDP();
        }

        public void BackFromJoin()
        {
            chooseToJoinPanel.SetActive(false);
            Exitt();
        }

        public void Create()
        {
            chooseAppsPanel.SetActive(true);
        }

        public void ChatSelected()
        {
            LoadChatScene();
            StartTCPHostProcess();
            StartBroadcastUDP();
        }

        public void TicTacToeSelected()
        {
            LoadTicTacToeScene();
        }

        public void BackFromCreate()
        {
            chooseAppsPanel.SetActive(false);
            Exitt();
        }

        private void ChangeScene(int sceneId)
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(sceneId);
        }

        public void LoadChatScene()
        {
            ChangeScene(ChatSceneId);
        }

        public void LoadTicTacToeScene()
        {
            ChangeScene(TicTacToeSceneId);
        }

        public void LoadOpeningScene()
        {
            ChangeScene(OpeningSceneId);
        }

        private void AddTCPConnection(ClientObject clientObject)
        {
            clientObjects.Add(clientObject);
        }

        private void RemoveTCPConnection(ClientObject clientObject)
        {
            if ((clientObject != null) && clientObjects.Contains(clientObject)) clientObjects.Remove(clientObject);
        }

        private void AvailableCheckAuto()
        {
            availableCheck?.Invoke();
        }

        private void AvailableCheckServer()
        {
            availableTime += Time.deltaTime;
            if (availableTime >= availableCooldown)
            {
                BroadcastTCPMessage(Information.GetAvailableCommand(), string.Empty);
            }
        }

        private void AvailableCheckClient()
        {
            availableTime += Time.deltaTime;
            if (availableTime >= availableCooldown)
            {
                SendMessageAsClient(Information.GetAvailableCommand());
            }
        }

        private void BroadcastTCPMessage(string message, string id)
        {
            availableTime = 0f;
            ClientObject clientObj = null;
            try
            {
                byte[] data = GetByteArrayFromString(message);
                for (int i = 0; i < clientObjects.Count; i++)
                {
                    clientObj = clientObjects[i];
                    if (clientObjects[i].UID != id) clientObjects[i].UserStream.Write(data, 0, data.Length);
                }
            }
            catch (System.IO.IOException)
            {
                RemoveTCPConnection(clientObj);
                clientObj.Close();
                string messagex = string.Format("\"{0}\" left the room!", clientObj.userName);
                ShowInfo(messagex);
                BroadcastTCPMessage(messagex, clientObj.UID);
            }
            catch (Exception ex)
            {
                s_Disconnect();
                ShowInfo("BroadcastTCPMessage - " + ex.Message);
            }
        }

        private void BroadcastTCPResendAuto()
        {
            try
            {
                for (int i = 0; i < clientObjects.Count; i++)
                {
                    if (clientObjects[i].HasData())
                    {
                        string message = clientObjects[i].GetMessage();
                        if (Information.IsCommand(message)) return;
                        message = string.Format("{0}: {1}", clientObjects[i].userName, message);
                        ShowInfo(message);
                        BroadcastTCPMessage(message, clientObjects[i].UID);
                    }
                }
            }
            catch (Exception ex)
            {
                s_Disconnect();
                ShowInfo("BroadcastTCPResendAuto - " + ex.Message);
            }
        }

        private void s_Disconnect()
        {
            RemAct(TCPListenAuto);
            RemAct(BroadcastTCPResendAuto);
            RemAct(AvailableCheckAuto);
            sendMessageAs = null;
            availableCheck = null;
            if (tcpListener != null)
            {
                tcpListener.Stop();
                tcpListener = null;
            }
            for (int i = 0; i < clientObjects.Count; i++)
            {
                clientObjects[i].Close();
            }
            clientObjects.Clear();
        }

        private void TCPListenAuto()
        {
            try
            {
                if (tcpListener.Pending())
                {
                    TcpClient tcpClientl = tcpListener.AcceptTcpClient();
                    ClientObject clientObjectl = new ClientObject(tcpClientl);
                    AddTCPConnection(clientObjectl);
                    string helloMessage = string.Format("\"{0}\" joined!", clientObjectl.userName);
                    ShowInfo(helloMessage);
                    BroadcastTCPMessage(helloMessage, clientObjectl.UID);
                }
            }
            catch (Exception ex)
            {
                s_Disconnect();
                ShowInfo("TCPListenAuto - " + ex.Message);
            }
        }

        private void ShowInfo(string info)
        {
            Debug.Log(info);
            if (chatMessageTransform != null)
            {
                Instantiate(chatMessagePrefab, chatMessageTransform).SetText(messageNumber.ToString() + ") " + info);
                messageNumber++;
            }
        }

        public void Exitt()
        {
            s_Disconnect();
            c_Disconnect();
            StopBroadcastUDP();
            StopReceiveUDP();
        }

        private void Update()
        {
            act?.Invoke();
        }

        private void Start()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(instance);
            }
            ShowMyIp();
            if (myNickname.Length < 3) ChangeNickname();
            else nickNameButton.text = myNickname;
        }

        private void ShowMyIp()
        {
            foreach (IPAddress iPAddress in Dns.GetHostAddresses(Dns.GetHostName()))
            {
                if (iPAddress.AddressFamily.Equals(AddressFamily.InterNetwork))
                {
                    myIPAddress = iPAddress;
                    //ShowInfo(iPAddress.ToString() + " - my IP");
                }
            }
        }

        private void SetAct(Act _act)
        {
            act += _act;
        }

        private void RemAct(Act _act)
        {
            act -= _act;
        }

        private void ClearAct()
        {
            act = null;
        }

        private void StartTCPHostProcess()
        {
            try
            {
                tcpListener = new TcpListener(IPAddress.Any, ListenerTCPport);
                tcpListener.Start();
                sendMessageAs = SendMessageAsHost;
                availableCheck = AvailableCheckServer;
                SetAct(AvailableCheckAuto);
                SetAct(TCPListenAuto);
                SetAct(BroadcastTCPResendAuto);
            }
            catch (Exception ex)
            {
                s_Disconnect();
                ShowInfo("StartTCPHostProcess - " + ex.Message);
            }
        }

        public void SendTCPMessage()
        {
            sendMessageAs?.Invoke();
        }

        private void SendMessageAsClient()
        {
            string message = textSend.text;
            SendMessageAsClient(message);
            ShowInfo("me : " + message);
        }

        private void SendMessageAsClient(string message)
        {
            try
            {
                availableTime = 0f;
                byte[] data = GetByteArrayFromString(message);
                networkStream.Write(data, 0, data.Length);
            }
            catch (System.IO.IOException)
            {
                c_Disconnect();
                ShowInfo("Связь с хостом разорвана!");
            }
            catch (Exception ex)
            {
                c_Disconnect();
                ShowInfo("SendMessageAsClient - " + ex.Message);
            }
        }

        private void SendMessageAsHost()
        {
            string message = textSend.text;
            ShowInfo("me(host) : " + message);
            message = string.Format("{0}(host): {1}", myNickname, message);
            BroadcastTCPMessage(message, string.Empty);
        }

        private void ReceiveTCPMessageAuto()
        {
            try
            {
                if (networkStream.DataAvailable)
                {
                    byte[] data = new byte[64];
                    StringBuilder builder = new StringBuilder();
                    int bytes = 0;
                    do
                    {
                        bytes = networkStream.Read(data, 0, data.Length);
                        builder.Append(Encoding.Unicode.GetString(data, 0, bytes));
                    }
                    while (networkStream.DataAvailable);
                    string message = builder.ToString();
                    if (!Information.IsCommand(message)) ShowInfo(message);
                }
            }
            catch (Exception ex)
            {
                c_Disconnect();
                ShowInfo("ReceiveTCPMessageAuto - " + ex.Message);
            }
        }

        private void c_Disconnect()
        {
            RemAct(ReceiveTCPMessageAuto);
            RemAct(AvailableCheckAuto);
            sendMessageAs = null;
            availableCheck = null;
            if (networkStream != null)
            {
                networkStream.Close();
                networkStream = null;
            }
            if (tcpClient != null)
            {
                tcpClient.Close();
                tcpClient = null;
            }
        }

        public void StartTCPClientProcess(string hostIP)
        {
            tcpClient = new TcpClient();
            try
            {
                tcpClient.Connect(IPAddress.Parse(hostIP), ListenerTCPport);
                networkStream = tcpClient.GetStream();
                SetAct(ReceiveTCPMessageAuto);
                sendMessageAs = SendMessageAsClient;
                string firstMessage = myNickname;
                byte[] data = GetByteArrayFromString(firstMessage);
                networkStream.Write(data, 0, data.Length);
                ShowInfo("Welcome, " + myNickname);
                SetAct(AvailableCheckAuto);
                availableCheck = AvailableCheckClient;
            }
            catch (Exception ex)
            {
                c_Disconnect();
                ShowInfo("StartTCPClientProcess - " + ex.Message);
            }
        }

        private void ReceiveUDPMessageA()
        {
            if (receivedUDPData.Length > 0)
            {
                ShowInfo(receivedUDPData);
                receivedUDPData = string.Empty;
            }
        }

        private void StartReceiveUDP()
        {
            try
            {
                udpClient = new UdpClient(BroadcastUDPport);
                ReceiveBugSolve();
                SetAct(ReceiveUDPMessageA);
                udpReceiveThread = new System.Threading.Thread(new System.Threading.ThreadStart(ReceiveUDP));
                udpReceiveThread.Start();
            }
            catch (Exception ex)
            {
                StopReceiveUDP();
                ShowInfo("StartReceiveUDP - " + ex.Message);
            }
        }

        private void ReceiveBugSolve()
        {
            byte[] data = GetByteArrayFromString(Information.GetAvailableCommand());
            udpClient.Send(data, data.Length, new IPEndPoint(BroadcastIpAddress, BroadcastUDPport));
        }

        private void StopReceiveUDP()
        {
            RemAct(ReceiveUDPMessageA);
            ReceiveUDPMessageA();
            if (udpClient != null)
            {
                udpClient.Close();
                udpClient = null;
            }
            if ((udpReceiveThread != null) && udpReceiveThread.IsAlive)
            {
                udpReceiveThread.Interrupt();
                udpReceiveThread = null;
            }
        }

        private void StopReceiveUDPFromMainThread()
        {
            RemAct(StopReceiveUDPFromMainThread);
            StopReceiveUDP();
        }

        private void ReceiveUDP()
        {
            IPEndPoint udpEndPointClient = null;
            while (true)
            {
                try
                {
                    byte[] data = udpClient.Receive(ref udpEndPointClient);
                    string message = GetStringFromByteArray(data);
                    List<string> divided = Information.GetDividedCommands(message);
                    receivedNickname = string.Empty;
                    receivedIp = string.Empty;
                    receivedApplication = Information.Applications.TicTacToe;
                    foreach (string command in divided)
                    {
                        if (Information.IsIpAddress(command)) receivedIp = Information.GetIpAddress(command);
                        else if (Information.IsNickname(command)) receivedNickname = Information.GetNickname(command);
                        else if (Information.IsApplication(command)) receivedApplication = Information.GetApplication(command);
                    }
                    if ((receivedNickname != string.Empty) && (receivedIp != string.Empty) && (receivedApplication != Information.Applications.TicTacToe))
                    {
                        SetAct(ConnectFromMainThread);
                        SetAct(StopReceiveUDPFromMainThread);
                        break;
                    }
                    else receivedUDPData = message;
                }
                catch (Exception ex)
                {
                    receivedUDPData = "ReceiveUDP - " + ex.Message;
                    SetAct(StopReceiveUDPFromMainThread);
                    break;
                }
            }
        }

        private void ConnectFromMainThread()
        {
            RemAct(ConnectFromMainThread);
            ShowJoinButton(receivedIp, receivedNickname, receivedApplication);
        }

        private void StartBroadcastUDP()
        {
            try
            {
                udpServer = new UdpClient();
                udpServer.Connect(BroadcastIpAddress, BroadcastUDPport);
                SetAct(BroadcastUDP);
            }
            catch (Exception ex)
            {
                StopBroadcastUDP();
                ShowInfo("StartBroadcastUDP - " + ex.Message);
            }
        }

        private void StopBroadcastUDP()
        {
            RemAct(BroadcastUDP);
            if (udpServer != null)
            {
                udpServer.Close();
                udpServer = null;
            }
        }

        private void BroadcastUDP()
        {
            broadcastUDPTime += Time.deltaTime;
            if (broadcastUDPTime > broadcastUDPCooldown)
            {
                broadcastUDPTime = 0f;
                try
                {
                    string message = Information.SetJoinCommand(myIPAddress.ToString(), application, myNickname);
                    byte[] data = GetByteArrayFromString(message);
                    udpServer.Send(data, data.Length);
                }
                catch (Exception ex)
                {
                    StopBroadcastUDP();
                    ShowInfo("BroadcastUDP - " + ex.Message);
                }
            }
        }

        private static byte[] GetByteArrayFromString(string infoToEncode)
        {
            return Encoding.Unicode.GetBytes(infoToEncode);
        }

        private static string GetStringFromByteArray(byte[] infoToEncode)
        {
            return Encoding.Unicode.GetString(infoToEncode);
        }


        public class ClientObject
        {
            protected internal string UID { get; private set; }
            protected internal NetworkStream UserStream { get; private set; }
            protected internal readonly string userName;
            private readonly TcpClient userClient;

            public ClientObject(TcpClient tcpClient)
            {
                UID = Guid.NewGuid().ToString();
                userClient = tcpClient;
                UserStream = userClient.GetStream();
                userName = GetMessage();
            }

            protected internal bool HasData()
            {
                return UserStream.DataAvailable;
            }

            protected internal string GetMessage()
            {
                if (HasData())
                {
                    byte[] data = new byte[64];
                    StringBuilder builder = new StringBuilder();
                    do
                    {
                        int bytes = UserStream.Read(data, 0, data.Length);
                        builder.Append(Encoding.Unicode.GetString(data, 0, bytes));
                    }
                    while (HasData());
                    return builder.ToString();
                }
                return string.Empty;
            }

            protected internal void Close()
            {
                if (UserStream != null) UserStream.Close();
                if (userClient != null) userClient.Close();
            }
        }
    }
}
