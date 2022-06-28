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
        private Act sendMessageAsManually;
        public delegate void CreateButtonDelegate(string ip, string nick, Information.Applications app);
        public delegate void SetDelegateToButton(string ip, string nick, Information.Applications app,
            CreateButtonDelegate createButtonDelegate);
        private SetDelegateToButton setCreatedButton;
        public delegate void DoSomethingWithStringDelegate(string message);
        private DoSomethingWithStringDelegate createChatMessage;
        private DoSomethingWithStringDelegate showSecondPlayerNicknameAndSetTurn;
        public delegate string GetSomeStringDelegate();
        private GetSomeStringDelegate getManuallySendableMessage;
        private GetSomeStringDelegate messageToUdpBroadcast;
        public delegate void EnemyMadeMoveDelegate(int pos_x, int pos_y);
        private EnemyMadeMoveDelegate enemyMadeMove;
        public delegate void DoSomethingWithTwoStringDelegate(string string1, string string2);
        private DoSomethingWithTwoStringDelegate invitationReceived;

        private TcpListener tcpListener;
        private List<ClientObject> clientObjects = new List<ClientObject>();
        private int maxClientsNumber = 1;
        private TcpClient tcpClient;
        private NetworkStream networkStream;
        private const float availableCooldown = 1f;
        private float availableTime = 0f;

        private readonly IPAddress BroadcastIpAddress = IPAddress.Parse("255.255.255.255");
        private const int BroadcastUDPport = 55558;
        private const int ListenerTCPport = 55559;
        private Dictionary<ClientObject, int> invitedClientsPorts = new Dictionary<ClientObject, int>();

        private string receivedUdpData = string.Empty;
        private bool isUdpReceiving = false;
        private float broadcastUdpTime = 0f;
        public const float BroadcastUdpCooldown = 0.5f;
        private UdpClient udpClient;
        private System.Threading.Thread udpReceiveThread;
        private IPAddress myIPAddress;
        private string myNickname = "myNickname";
        private Information.Applications myApplication = Information.Applications.ApplicationError;
        private Role myRole = Role.RoleError;
        private bool isJoinButtonWillBeCreatedSoon = false;
        private bool isInviteButtonWillBeCreatedSoon = false;

        private string receivedIp = string.Empty;
        private string receivedNickname = string.Empty;
        private Information.Applications receivedApplication = Information.Applications.ApplicationError;
        private string receivedInvitationNickname = string.Empty;
        private Information.Applications receivedInvitationApplication = Information.Applications.ApplicationError;

        private int receivedPosX = 0;
        private int receivedPosY = 0;
        private float currentTimeToShowEnemyMove = 0f;
        private const float TimeToShowEnemyMove = 0.5f;

        public static NetScript1 instance;

        private const int OpeningSceneId = 0;
        private const int ChatSceneId = 1;
        private const int TicTacToeSceneId = 2;
        private int currentSceneId = 0;

        public enum Role
        {
            Host,
            Client,
            RoleError,
        }

        private Role GetRole()
        {
            return myRole;
        }

        public bool IsHost()
        {
            return GetRole() == Role.Host;
        }

        public bool IsClient()
        {
            return GetRole() == Role.Client;
        }

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
            StopClientListen();
            myRole = Role.Client;
            myApplication = app;
            StopUdpProcess();
            StartTcpClientProcess(ip);
            if (app == Information.Applications.Chat)
            {
                LoadChatScene();
                sendMessageAsManually = SendMessageAsClientManually;
            }
            else if (app == Information.Applications.TicTacToe)
            {
                LoadTicTacToeScene();
                SendTcpMessageAsClient(Information.SetNicknameCommand(myNickname));
            }
        }

        private void InviteButtonPressed(string ip, string nick, Information.Applications app)
        {
            try
            {
                int port = GetFreePort();
                tcpClient = new TcpClient(new IPEndPoint(IPAddress.Any, port));
                tcpClient.Connect(IPAddress.Parse(ip), ListenerTCPport);
                networkStream = tcpClient.GetStream();
                ClientObject clientObjectl = new ClientObject(tcpClient, nick);
                AddTcpConnection(clientObjectl, port);
                string helloMessage = string.Format("\"{0}\" invited!", clientObjectl.userName);
                ShowInfo(helloMessage);
                SendBroadcastTcpMessage(helloMessage, clientObjectl.UID);
                SendTcpMessageAsClient(Information.GetNickname(myNickname) + Information.GetDividerCommand() +
                    Information.SetApplicationCommand(app) + Information.GetDividerCommand() + Information.GetInvitationCommand());
                if (ShouldTheRoomBeClosed()) CloseRoomByHost();
            }
            catch (Exception ex)
            {
                if (tcpClient != null)
                {
                    tcpClient.Close();
                    tcpClient = null;
                }
                ShowInfo("InviteButtonPressed - " + ex.Message);
            }
        }

        private int GetFreePort()
        {
            int port = ListenerTCPport + 1;
            while (!IsPortFree(port))
            {
                port++;
            }
            return port;
        }

        private bool IsPortFree(int port)
        {
            foreach (int p in invitedClientsPorts.Values)
            {
                if (port == p) return false;
            }
            return true;
        }

        public void AcceptInvitation()
        {
            SendTcpMessageAsClient(Information.SetNicknameCommand(myNickname) + Information.GetDividerCommand() +
                Information.GetInvitationAcceptedCommand());
            myApplication = receivedInvitationApplication;
            if (myApplication == Information.Applications.Chat)
            {
                LoadChatScene();
                sendMessageAsManually = SendMessageAsClientManually;
            }
            else if (myApplication == Information.Applications.TicTacToe)
            {
                LoadTicTacToeScene();
            }
        }

        public void RefuseInvitation()
        {
            SendTcpMessageAsClient(Information.SetNicknameCommand(myNickname) + Information.GetDividerCommand() +
                Information.GetInvitationRefusedCommand());
            DisconnectTcpClient();
            JoinRoomPressed();
        }

        public void JoinRoomPressed()
        {
            GetMyIp();
            messageToUdpBroadcast = GetInvitationInformation;
            myRole = Role.Client;
            StartUdpProcess();
            StartClientListen();
        }

        public void ChatSelected()
        {
            myRole = Role.Host;
            myApplication = Information.Applications.Chat;
            maxClientsNumber = 10;
            GetMyIp();
            LoadChatScene();
            messageToUdpBroadcast = GetCreatedRoomInformation;
            StartUdpProcess();
            StartTcpHostProcess();
            sendMessageAsManually = SendMessageAsHostManually;
        }

        public void TicTacToeSelected()
        {
            myRole = Role.Host;
            myApplication = Information.Applications.TicTacToe;
            maxClientsNumber = 1;
            GetMyIp();
            LoadTicTacToeScene();
            messageToUdpBroadcast = GetCreatedRoomInformation;
            StartUdpProcess();
            StartTcpHostProcess();
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

        private void AddTcpConnection(ClientObject clientObject)
        {
            clientObjects.Add(clientObject);
        }

        private void AddTcpConnection(ClientObject clientObject, int host_port)
        {
            AddTcpConnection(clientObject);
            invitedClientsPorts.Add(clientObject, host_port);
        }

        private void RemoveTcpConnection(ClientObject clientObject)
        {
            if (clientObject != null)
            {
                if (clientObjects.Contains(clientObject)) clientObjects.Remove(clientObject);
                if (invitedClientsPorts.ContainsKey(clientObject)) invitedClientsPorts.Remove(clientObject);
            }
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
                SendBroadcastTcpMessage(Information.GetAvailableCommand(), string.Empty);
            }
        }

        private void AvailableCheckAsClient()
        {
            availableTime += Time.deltaTime;
            if (availableTime >= availableCooldown)
            {
                SendTcpMessageAsClient(Information.GetAvailableCommand());
            }
        }

        private void SendBroadcastTcpMessage(string message, string id)
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
                RemoveTcpConnection(clientObj);
                clientObj.Close();
                string messagex = string.Format("\"{0}\" left the room!", clientObj.userName);
                ShowInfo(messagex);
                SendBroadcastTcpMessage(messagex, clientObj.UID);
            }
            catch (Exception ex)
            {
                DisconnectTcpHost();
                ShowInfo("SendBroadcastTcpMessage - " + ex.Message);
            }
        }

        private void BroadcastTcpResendAuto()
        {
            try
            {
                for (int i = 0; i < clientObjects.Count; i++)
                {
                    if (clientObjects[i].HasData())
                    {
                        string message = clientObjects[i].GetMessage();
                        if (Information.IsAvailableCommand(message)) return;
                        ProcessTcpCommand(message);
                        SendBroadcastTcpMessage(message, clientObjects[i].UID);
                    }
                }
            }
            catch (Exception ex)
            {
                DisconnectTcpHost();
                ShowInfo("BroadcastTcpResendAuto - " + ex.Message);
            }
        }

        private void ListenTcpAuto()
        {
            try
            {
                if (tcpListener.Pending())
                {
                    TcpClient tcpClientl = tcpListener.AcceptTcpClient();
                    ClientObject clientObjectl = new ClientObject(tcpClientl);
                    AddTcpConnection(clientObjectl);
                    string helloMessage = string.Format("\"{0}\" joined!", clientObjectl.userName);
                    ShowInfo(helloMessage);
                    SendBroadcastTcpMessage(helloMessage, clientObjectl.UID);
                    if (ShouldTheRoomBeClosed()) CloseRoomByHost();
                }
            }
            catch (Exception ex)
            {
                DisconnectTcpHost();
                ShowInfo("ListenTcpAuto - " + ex.Message);
            }
        }

        private void StartTcpHostProcess()
        {
            try
            {
                tcpListener = new TcpListener(IPAddress.Any, ListenerTCPport);
                tcpListener.Start();
                availableCheck = AvailableCheckAsHost;
                StartAvailableCheck();
                SetAct(ListenTcpAuto);
                SetAct(BroadcastTcpResendAuto);
            }
            catch (Exception ex)
            {
                DisconnectTcpHost();
                ShowInfo("StartTcpHostProcess - " + ex.Message);
            }
        }

        private bool ShouldTheRoomBeClosed()
        {
            return clientObjects.Count >= maxClientsNumber;
        }

        private void CloseRoomByHost()
        {
            StopUdpProcess();
            RemAct(ListenTcpAuto);
            if (tcpListener != null)
            {
                tcpListener.Stop();
                tcpListener = null;
            }
            ShowInfo("Room closed!");
        }

        private void DisconnectTcpHost()
        {
            StopAvailableCheck();
            RemAct(ListenTcpAuto);
            RemAct(BroadcastTcpResendAuto);
            createChatMessage = null;
            sendMessageAsManually = null;
            enemyMadeMove = null;
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

        private void StartClientListen()
        {
            try
            {
                tcpListener = new TcpListener(IPAddress.Any, ListenerTCPport);
                tcpListener.Start();
                SetAct(ListenAsClientAuto);
            }
            catch (Exception ex)
            {
                StopClientListen();
                ShowInfo("StartClientListen - " + ex.Message);
            }
        }

        private void ListenAsClientAuto()
        {
            try
            {
                if (tcpListener.Pending())
                {
                    StopUdpProcess();
                    tcpClient = tcpListener.AcceptTcpClient();
                    StopClientListen();
                    networkStream = tcpClient.GetStream();
                    availableCheck = AvailableCheckAsClient;
                    StartAvailableCheck();
                    SetAct(ReceiveTcpMessageAuto);
                }
            }
            catch (Exception ex)
            {
                StopClientListen();
                ShowInfo("ListenAsClientAuto - " + ex.Message);
            }
        }

        private void StopClientListen()
        {
            RemAct(ListenAsClientAuto);
            if (tcpListener != null)
            {
                tcpListener.Stop();
                tcpListener = null;
            }
        }

        private void ReceiveTcpMessageAuto()
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
                    if (!Information.IsAvailableCommand(message)) ProcessTcpCommand(message);
                }
            }
            catch (Exception ex)
            {
                DisconnectTcpClient();
                ShowInfo("ReceiveTcpMessageAuto - " + ex.Message);
            }
        }

        private void SendTcpMessageAsClient(string message)
        {
            try
            {
                availableTime = 0f;
                byte[] data = GetByteArrayFromString(message);
                networkStream.Write(data, 0, data.Length);
            }
            catch (System.IO.IOException)
            {
                ShowInfo("The connection with the host is broken!");
                DisconnectTcpClient();
            }
            catch (Exception ex)
            {
                DisconnectTcpClient();
                ShowInfo("SendTcpMessageAsClient - " + ex.Message);
            }
        }

        private void StartTcpClientProcess(string hostIP)
        {
            try
            {
                tcpClient = new TcpClient(new IPEndPoint(IPAddress.Any, ListenerTCPport));
                tcpClient.Connect(IPAddress.Parse(hostIP), ListenerTCPport);
                networkStream = tcpClient.GetStream();
                string firstMessage = myNickname;
                byte[] data = GetByteArrayFromString(firstMessage);
                networkStream.Write(data, 0, data.Length);
                availableCheck = AvailableCheckAsClient;
                StartAvailableCheck();
                SetAct(ReceiveTcpMessageAuto);
            }
            catch (Exception ex)
            {
                DisconnectTcpClient();
                ShowInfo("StartTcpClientProcess - " + ex.Message);
            }
        }

        private void DisconnectTcpClient()
        {
            StopAvailableCheck();
            RemAct(ReceiveTcpMessageAuto);
            createChatMessage = null;
            sendMessageAsManually = null;
            enemyMadeMove = null;
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

        private void ProcessTcpCommand(string data)
        {
            List<string> divided = Information.GetDividedCommands(data);
            foreach (string dividedCommand in divided)
            {
                if (Information.IsNickname(dividedCommand))
                {
                    receivedNickname = Information.GetNickname(dividedCommand);
                    if (myApplication == Information.Applications.TicTacToe)
                    {
                        showSecondPlayerNicknameAndSetTurn?.Invoke(receivedNickname);
                        showSecondPlayerNicknameAndSetTurn = null;
                    }
                }
                else if (Information.IsApplication(dividedCommand))
                {
                    receivedApplication = Information.GetApplication(dividedCommand);
                }
                else if (Information.IsData(dividedCommand))
                {
                    Information.TypesOfData receivedTypeOfData = Information.GetTypeOfData(dividedCommand);
                    if (receivedTypeOfData == Information.TypesOfData.TicTacToePosX)
                    {
                        receivedPosX = int.Parse(Information.GetData(dividedCommand, receivedTypeOfData));
                    }
                    else if (receivedTypeOfData == Information.TypesOfData.TicTacToePosY)
                    {
                        receivedPosY = int.Parse(Information.GetData(dividedCommand, receivedTypeOfData));
                    }
                    else if (receivedTypeOfData == Information.TypesOfData.TurnWasMade)
                    {
                        SetAct(PositionsReceivedAndWillBeSentSoon);
                    }
                    else if (receivedTypeOfData == Information.TypesOfData.ChatMessage)
                    {
                        ShowInfo(receivedNickname + ": " + Information.GetData(dividedCommand, receivedTypeOfData));
                    }
                }
                else if (Information.IsInvitationAcceptedCommand(dividedCommand))
                {
                    OtherPlayerAcceptInvitation(receivedNickname);
                }
                else if (Information.IsInvitationRefusedCommand(dividedCommand))
                {
                    OtherPlayerRefuseInvitation(receivedNickname);
                }
                else if (Information.IsInvitationCommand(dividedCommand))
                {
                    InvitationReceived(receivedNickname, receivedApplication);
                }
            }
        }

        private void OtherPlayerAcceptInvitation(string other_nickname)
        {
            ShowInfo("\"" + receivedNickname + "\"" + " accept invitation!");
        }

        private void OtherPlayerRefuseInvitation(string other_nickname)
        {
            ShowInfo("\"" + receivedNickname + "\"" + " refuse invitation!");
            ClientObject clientObject = GetClientByNickname(other_nickname);
            clientObject?.Close();
            RemoveTcpConnection(clientObject);
        }

        private void InvitationReceived(string nickname, Information.Applications app)
        {
            receivedInvitationNickname = nickname;
            receivedInvitationApplication = app;
            invitationReceived?.Invoke(Information.StringApplications[app], nickname);
        }

        private ClientObject GetClientByNickname(string nickname)
        {
            foreach (ClientObject client in clientObjects)
            {
                if (client.userName == nickname)
                {
                    return client;
                }
            }
            return null;
        }

        private void PositionsReceivedAndWillBeSentSoon()
        {
            currentTimeToShowEnemyMove += Time.deltaTime;
            if ((currentTimeToShowEnemyMove >= TimeToShowEnemyMove) && (receivedPosX != 0) && (receivedPosY != 0))
            {
                RemAct(PositionsReceivedAndWillBeSentSoon);
                enemyMadeMove?.Invoke(receivedPosX, receivedPosY);
                receivedPosX = 0;
                receivedPosY = 0;
                currentTimeToShowEnemyMove = 0f;
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
            createChatMessage?.Invoke(info);
        }

        public void SendHiMessageChatHost()
        {
            ShowInfo("Room created, waiting for connections...");
            ShowInfo("Ip: " + myIPAddress.ToString() + ",   Nickname: " + myNickname);
            try
            {
                test1();
            }
            catch (Exception ex)
            {
                ShowInfo(ex.Message);
            }
        }

        private void test1()
        {
            foreach (IPAddress iPAddress in Dns.GetHostAddresses(Dns.GetHostName()))
            {
                ShowInfo(iPAddress.ToString() + " - " + iPAddress.AddressFamily.ToString());
            }
        }

        public void SendHiMessageChatClient()
        {
            ShowInfo("Welcome, " + myNickname);
        }

        public void Exitt()
        {
            myRole = Role.RoleError;
            myApplication = Information.Applications.ApplicationError;
            DisconnectTcpHost();
            DisconnectTcpClient();
            StopUdpProcess();
            StopClientListen();
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
            }
        }

        private void GetMyIp()
        {
            foreach (IPAddress iPAddress in Dns.GetHostAddresses(Dns.GetHostName()))
            {
                if (iPAddress.AddressFamily.Equals(AddressFamily.InterNetwork))
                {
                    myIPAddress = iPAddress;
                }
            }
        }

        private void ReceiveUdpMessageAuto()
        {
            if (receivedUdpData.Length > 0)
            {
                ShowInfo(receivedUdpData);
                receivedUdpData = string.Empty;
            }
        }

        private void StartUdpProcess()
        {
            try
            {
                udpClient = new UdpClient(BroadcastUDPport);
                SetAct(ReceiveUdpMessageAuto);
                if (!myIPAddress.ToString().EndsWith(".1"))
                {
                    SetAct(BroadcastUdpAuto);
                }
                isUdpReceiving = true;
                udpReceiveThread = new System.Threading.Thread(new System.Threading.ThreadStart(ReceiveUdp));
                udpReceiveThread.Start();
            }
            catch (Exception ex)
            {
                StopUdpProcess();
                ShowInfo("StartUdpProcess - " + ex.Message);
            }
        }

        private void StopUdpProcess()
        {
            isUdpReceiving = false;
            RemAct(ReceiveUdpMessageAuto);
            RemAct(BroadcastUdpAuto);
            ReceiveUdpMessageAuto();
            messageToUdpBroadcast = null;
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

        private void ReceiveUdp()
        {
            IPEndPoint udpEndPointClient = null;
            while (isUdpReceiving)
            {
                try
                {
                    if (isJoinButtonWillBeCreatedSoon || isInviteButtonWillBeCreatedSoon) continue;
                    byte[] data = udpClient.Receive(ref udpEndPointClient);
                    string message = GetStringFromByteArray(data);
                    if (message == messageToUdpBroadcast.Invoke()) continue;
                    List<string> divided = Information.GetDividedCommands(message);
                    foreach (string command in divided)
                    {
                        if (Information.IsIpAddress(command)) receivedIp = Information.GetIpAddress(command);
                        else if (Information.IsNickname(command)) receivedNickname = Information.GetNickname(command);
                        else if (Information.IsApplication(command)) receivedApplication = Information.GetApplication(command);
                    }
                    if ((receivedIp != string.Empty) && (receivedNickname != string.Empty))
                    {
                        if (receivedApplication != Information.Applications.ApplicationError)
                        {
                            isJoinButtonWillBeCreatedSoon = true;
                            SetAct(CreateJoinButtonFromMainThread);
                            continue;
                        }
                        else if (receivedApplication == Information.Applications.ApplicationError)
                        {
                            isInviteButtonWillBeCreatedSoon = true;
                            SetAct(CreateInviteButtonFromMainThread);
                            continue;
                        }
                    }
                    else receivedUdpData = message;
                }
                catch (Exception ex)
                {
                    receivedUdpData = "ReceiveUdp - " + ex.Message;
                    SetAct(StopReceiveUdpFromMainThread);
                    break;
                }
            }
        }

        private void StopReceiveUdpFromMainThread()
        {
            RemAct(StopReceiveUdpFromMainThread);
            StopUdpProcess();
        }

        private void CreateJoinButtonFromMainThread()
        {
            RemAct(CreateJoinButtonFromMainThread);
            setCreatedButton?.Invoke(receivedIp, receivedNickname, receivedApplication, JoinButtonPressed);
            receivedIp = string.Empty;
            receivedNickname = string.Empty;
            receivedApplication = Information.Applications.ApplicationError;
            isJoinButtonWillBeCreatedSoon = false;
        }

        private void CreateInviteButtonFromMainThread()
        {
            RemAct(CreateInviteButtonFromMainThread);
            //setCreatedButton?.Invoke(receivedIp, receivedNickname, receivedApplication, InviteButtonPressed);
            setCreatedButton?.Invoke(receivedIp, receivedNickname, myApplication, InviteButtonPressed);
            receivedIp = string.Empty;
            receivedNickname = string.Empty;
            receivedApplication = Information.Applications.ApplicationError;
            isInviteButtonWillBeCreatedSoon = false;
        }

        public void SetCreateButtonDelegate(SetDelegateToButton setDelegateToButton)
        {
            setCreatedButton = setDelegateToButton;
        }

        public void SetCreateChatMessageDelegate(DoSomethingWithStringDelegate createChatMessageDelegate)
        {
            createChatMessage = createChatMessageDelegate;
        }

        public void SetShowSecondPlayerNicknameAndSetTurnDelegate(DoSomethingWithStringDelegate _showSecondPlayerNicknameAndSetTurn)
        {
            showSecondPlayerNicknameAndSetTurn = _showSecondPlayerNicknameAndSetTurn;
        }

        public void SetGetManuallySendableMessageDelegate(GetSomeStringDelegate getSomeStringDelegate)
        {
            getManuallySendableMessage = getSomeStringDelegate;
        }

        public void SetEnemyMadeMoveDelegate(EnemyMadeMoveDelegate enemyMadeMoveDelegate)
        {
            enemyMadeMove = enemyMadeMoveDelegate;
        }

        public void SetInvitationReceivedDelegate(DoSomethingWithTwoStringDelegate _invitationReceived)
        {
            invitationReceived = _invitationReceived;
        }

        private void BroadcastUdpAuto()
        {
            broadcastUdpTime += Time.deltaTime;
            if (broadcastUdpTime >= BroadcastUdpCooldown)
            {
                broadcastUdpTime = 0f;
                try
                {
                    string message = messageToUdpBroadcast.Invoke();
                    byte[] data = GetByteArrayFromString(message);
                    udpClient.Send(data, data.Length, new IPEndPoint(BroadcastIpAddress, BroadcastUDPport));
                }
                catch (Exception ex)
                {
                    StopUdpProcess();
                    ShowInfo("BroadcastUdpAuto - " + ex.Message);
                }
            }
        }

        private string GetCreatedRoomInformation()
        {
            return Information.SetJoinCommand(myIPAddress.ToString(), myApplication, myNickname);
        }

        private string GetInvitationInformation()
        {
            return Information.SetInviteMeCommand(myIPAddress.ToString(), myNickname);
        }

        private void SendMessageAsHostManually()
        {
            string message = getManuallySendableMessage?.Invoke();
            ShowInfo(myNickname + "(host): " + message);
            message = Information.SetChatMessageCommand(myNickname + "(host)", message);
            SendBroadcastTcpMessage(message, string.Empty);
        }

        private void SendMessageAsClientManually()
        {
            string message = getManuallySendableMessage?.Invoke();
            ShowInfo(myNickname + ": " + message);
            message = Information.SetChatMessageCommand(myNickname, message);
            SendTcpMessageAsClient(message);
        }

        public void SendMessageAsManually()
        {
            sendMessageAsManually?.Invoke();
        }

        public void SendXODataAsHost(string data)
        {
            SendBroadcastTcpMessage(data, string.Empty);
            ShowInfo(data);
        }

        public void SendXODataAsClient(string data)
        {
            SendTcpMessageAsClient(data);
            ShowInfo(data);
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

            public ClientObject(TcpClient tcpClient, string nickname)
            {
                UID = Guid.NewGuid().ToString();
                userClient = tcpClient;
                UserStream = userClient.GetStream();
                userName = nickname;
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
