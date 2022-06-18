using System;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Collections.Generic;
using UnityEngine;

namespace LTTDIT.Net
{
    public class NetScript1 : MonoBehaviour
    {
        private delegate void Act();
        private Act act;
        private Act availableCheck;
        public delegate void CreateJoinButtonDelegate(string ip, string nick, Information.Applications app);
        public delegate void SetDelegateToJoinButton(string ip, string nick, Information.Applications app,
            CreateJoinButtonDelegate createJoinButtonDelegate);
        private SetDelegateToJoinButton setJoinButton;

        private TcpListener tcpListener;
        private List<ClientObject> clientObjects = new List<ClientObject>();
        private TcpClient tcpClient;
        private NetworkStream networkStream;
        private const float availableCooldown = 1f;
        private float availableTime = 0f;

        private readonly IPAddress BroadcastIpAddress = IPAddress.Parse("255.255.255.255");
        private const int BroadcastUDPport = 55558;
        private const int ListenerTCPport = 55559;

        private string receivedUDPData = string.Empty;
        private bool isUDPReceiving = false;
        private float broadcastUDPTime = 0f;
        private const float broadcastUDPCooldown = 1f;
        private UdpClient udpClient;
        private UdpClient udpServer;
        private System.Threading.Thread udpReceiveThread;
        private IPAddress myIPAddress;
        private string myNickname = string.Empty;
        private Information.Applications myApplication = Information.Applications.ApplicationError;

        private string receivedIp = string.Empty;
        private string receivedNickname = string.Empty;
        private Information.Applications receivedApplication = Information.Applications.ApplicationError;

        public static NetScript1 instance;

        private const int OpeningSceneId = 0;
        private const int ChatSceneId = 1;
        private const int TicTacToeSceneId = 2;
        private int currentSceneId = 0;

        public bool SetNickname(string nick_name)
        {
            if (nick_name.Length < 3) return false;
            myNickname = nick_name;
            return true;
        }

        public bool HasNickname()
        {
            return myNickname.Length >= 3;
        }

        public string GetNickname()
        {
            return myNickname;
        }

        private void JoinButtonPressed(string ip, string nick, Information.Applications app)
        {
            StopReceiveUDP();
            StartTCPClientProcess(ip);
            if (app == Information.Applications.Chat)
            {
                LoadChatScene();
            }
            else if (app == Information.Applications.TicTacToe)
            {
                LoadTicTacToeScene();
            }
        }

        public void JoinRoomPressed()
        {
            StartReceiveUDP();
        }

        public void ChatSelected()
        {
            myApplication = Information.Applications.Chat;
            LoadChatScene();
            StartBroadcastUDP();
            StartTCPHostProcess();
        }

        public void TicTacToeSelected()
        {
            myApplication = Information.Applications.TicTacToe;
            LoadTicTacToeScene();
            StartBroadcastUDP();
            StartTCPHostProcess();
        }

        private void ChangeScene(int sceneId)
        {
            if (currentSceneId != sceneId) UnityEngine.SceneManagement.SceneManager.LoadScene(sceneId);
            currentSceneId = sceneId;
        }

        private void LoadChatScene()
        {
            ChangeScene(ChatSceneId);
        }

        private void LoadTicTacToeScene()
        {
            ChangeScene(TicTacToeSceneId);
        }

        private void LoadOpeningScene()
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

        private void StartAvailableCheck()
        {
            SetAct(AvailableCheckAuto);
        }

        private void StopAvailableCheck()
        {
            RemAct(AvailableCheckAuto);
            availableCheck = null;
        }

        private void AvailableCheckAsHost()
        {
            availableTime += Time.deltaTime;
            if (availableTime >= availableCooldown)
            {
                SendBroadcastTCPMessage(Information.GetAvailableCommand(), string.Empty);
            }
        }

        private void AvailableCheckAsClient()
        {
            availableTime += Time.deltaTime;
            if (availableTime >= availableCooldown)
            {
                SendTCPMessageAsClient(Information.GetAvailableCommand());
            }
        }

        private void SendBroadcastTCPMessage(string message, string id)
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
                SendBroadcastTCPMessage(messagex, clientObj.UID);
            }
            catch (Exception ex)
            {
                DisconnectTCPHost();
                ShowInfo("SendBroadcastTCPMessage - " + ex.Message);
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
                        if (Information.IsAvailableCommand(message)) return;
                        message = string.Format("{0}: {1}", clientObjects[i].userName, message);
                        ShowInfo(message);
                        SendBroadcastTCPMessage(message, clientObjects[i].UID);
                    }
                }
            }
            catch (Exception ex)
            {
                DisconnectTCPHost();
                ShowInfo("BroadcastTCPResendAuto - " + ex.Message);
            }
        }

        private void ListenTCPAuto()
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
                    SendBroadcastTCPMessage(helloMessage, clientObjectl.UID);
                }
            }
            catch (Exception ex)
            {
                DisconnectTCPHost();
                ShowInfo("ListenTCPAuto - " + ex.Message);
            }
        }

        private void StartTCPHostProcess()
        {
            try
            {
                tcpListener = new TcpListener(IPAddress.Any, ListenerTCPport);
                tcpListener.Start();
                availableCheck = AvailableCheckAsHost;
                StartAvailableCheck();
                SetAct(ListenTCPAuto);
                SetAct(BroadcastTCPResendAuto);
            }
            catch (Exception ex)
            {
                DisconnectTCPHost();
                ShowInfo("StartTCPHostProcess - " + ex.Message);
            }
        }

        private void DisconnectTCPHost()
        {
            StopAvailableCheck();
            RemAct(ListenTCPAuto);
            RemAct(BroadcastTCPResendAuto);
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

        private void ReceiveTCPMessageAuto()
        {
            try
            {
                if (networkStream.DataAvailable)
                {
                    byte[] data = new byte[256];
                    StringBuilder builder = new StringBuilder();
                    int bytes = 0;
                    do
                    {
                        bytes = networkStream.Read(data, 0, data.Length);
                        builder.Append(Encoding.Unicode.GetString(data, 0, bytes));
                    }
                    while (networkStream.DataAvailable);
                    string message = builder.ToString();
                    if (!Information.IsAvailableCommand(message)) ShowInfo(message);
                }
            }
            catch (Exception ex)
            {
                DisconnectTCPClient();
                ShowInfo("ReceiveTCPMessageAuto - " + ex.Message);
            }
        }

        private void SendTCPMessageAsClient(string message)
        {
            try
            {
                availableTime = 0f;
                byte[] data = GetByteArrayFromString(message);
                networkStream.Write(data, 0, data.Length);
            }
            catch (System.IO.IOException)
            {
                DisconnectTCPClient();
                ShowInfo("Связь с хостом разорвана!");
            }
            catch (Exception ex)
            {
                DisconnectTCPClient();
                ShowInfo("SendTCPMessageAsClient - " + ex.Message);
            }
        }

        private void StartTCPClientProcess(string hostIP)
        {
            tcpClient = new TcpClient();
            try
            {
                tcpClient.Connect(IPAddress.Parse(hostIP), ListenerTCPport);
                networkStream = tcpClient.GetStream();
                string firstMessage = myNickname;
                byte[] data = GetByteArrayFromString(firstMessage);
                networkStream.Write(data, 0, data.Length);
                ShowInfo("Welcome, " + myNickname);
                availableCheck = AvailableCheckAsClient;
                StartAvailableCheck();
                SetAct(ReceiveTCPMessageAuto);
            }
            catch (Exception ex)
            {
                DisconnectTCPClient();
                ShowInfo("StartTCPClientProcess - " + ex.Message);
            }
        }

        private void DisconnectTCPClient()
        {
            StopAvailableCheck();
            RemAct(ReceiveTCPMessageAuto);
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

        private void SetAct(Act _act)
        {
            act += _act;
        }

        private void RemAct(Act _act)
        {
            act -= _act;
        }

        private void ShowInfo(string info)
        {
            Debug.Log(info);
        }

        public void Exitt()
        {
            DisconnectTCPHost();
            DisconnectTCPClient();
            StopBroadcastUDP();
            StopReceiveUDP();
            LoadOpeningScene();
        }

        private void Update()
        {
            act?.Invoke();
        }

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(instance);
                GetMyIp();
            }
        }

        private void Start()
        {

        }

        private void GetMyIp()
        {
            foreach (IPAddress iPAddress in Dns.GetHostAddresses(Dns.GetHostName()))
            {
                if (iPAddress.AddressFamily.Equals(AddressFamily.InterNetwork))
                {
                    myIPAddress = iPAddress;
                    ShowInfo(iPAddress.ToString() + " - my IP");
                }
            }
        }

        private void ReceiveUDPMessageAuto()
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
                SendToSolveReceiveBug();
                SetAct(ReceiveUDPMessageAuto);
                isUDPReceiving = true;
                udpReceiveThread = new System.Threading.Thread(new System.Threading.ThreadStart(ReceiveUDP));
                udpReceiveThread.Start();
            }
            catch (Exception ex)
            {
                StopReceiveUDP();
                ShowInfo("StartReceiveUDP - " + ex.Message);
            }
        }

        private void SendToSolveReceiveBug()
        {
            byte[] data = GetByteArrayFromString(Information.GetAvailableCommand());
            udpClient.Send(data, data.Length, new IPEndPoint(BroadcastIpAddress, BroadcastUDPport));
        }

        private void StopReceiveUDP()
        {
            isUDPReceiving = false;
            RemAct(ReceiveUDPMessageAuto);
            ReceiveUDPMessageAuto();
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

        private void ReceiveUDP()
        {
            IPEndPoint udpEndPointClient = null;
            while (isUDPReceiving)
            {
                try
                {
                    byte[] data = udpClient.Receive(ref udpEndPointClient);
                    string message = GetStringFromByteArray(data);
                    List<string> divided = Information.GetDividedCommands(message);
                    receivedIp = string.Empty;
                    receivedNickname = string.Empty;
                    receivedApplication = Information.Applications.ApplicationError;
                    foreach (string command in divided)
                    {
                        if (Information.IsIpAddress(command)) receivedIp = Information.GetIpAddress(command);
                        else if (Information.IsNickname(command)) receivedNickname = Information.GetNickname(command);
                        else if (Information.IsApplication(command)) receivedApplication = Information.GetApplication(command);
                    }
                    if ((receivedIp != string.Empty) && (receivedNickname != string.Empty) &&
                        (receivedApplication != Information.Applications.ApplicationError))
                    {
                        SetAct(CreateJoinButtonFromMainThread);
                        continue;
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

        private void StopReceiveUDPFromMainThread()
        {
            RemAct(StopReceiveUDPFromMainThread);
            StopReceiveUDP();
        }

        private void CreateJoinButtonFromMainThread()
        {
            RemAct(CreateJoinButtonFromMainThread);
            setJoinButton?.Invoke(receivedIp, receivedNickname, receivedApplication, JoinButtonPressed);
        }

        public void SetCreateJoinButtonDelegate(SetDelegateToJoinButton setDelegateToJoinButton)
        {
            setJoinButton = setDelegateToJoinButton;
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
            if (broadcastUDPTime >= broadcastUDPCooldown)
            {
                broadcastUDPTime = 0f;
                try
                {
                    string message = Information.SetJoinCommand(myIPAddress.ToString(), myApplication, myNickname);
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

            protected internal void Close()
            {
                if (UserStream != null) UserStream.Close();
                if (userClient != null) userClient.Close();
            }
        }
    }
}
