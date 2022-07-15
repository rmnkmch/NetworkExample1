using System.Collections.Generic;

namespace LTTDIT.Net
{
    public static class Information
    {
        public enum CommandFlags
        {
            StartHeader,
            EndHeader,
            Available,
            StartInfo,
            EndInfo,
            Divider,
            Data,
            StartData,
            EndData,
            InvitationAccepted,
            InvitationRefused,
            Invitation,
            Request,
        }

        public static readonly Dictionary<CommandFlags, string> StringCommands = new Dictionary<CommandFlags, string>()
        {
            [CommandFlags.StartHeader] = "<header>",
            [CommandFlags.EndHeader] = "</header>",
            [CommandFlags.Available] = "<Available>",
            [CommandFlags.StartInfo] = "<info>",
            [CommandFlags.EndInfo] = "</info>",
            [CommandFlags.Divider] = "<_!_>",
            [CommandFlags.Data] = "Data",
            [CommandFlags.StartData] = "<data>",
            [CommandFlags.EndData] = "</data>",
            [CommandFlags.InvitationAccepted] = "<InvitationAccepted>",
            [CommandFlags.InvitationRefused] = "<InvitationRefused>",
            [CommandFlags.Invitation] = "<Invitation>",
            [CommandFlags.Request] = "Request",
        };

        public enum Applications
        {
            ApplicationError,
            Chat,
            TicTacToe,
        }

        public static readonly Dictionary<Applications, string> StringApplications = new Dictionary<Applications, string>()
        {
            [Applications.ApplicationError] = "ApplicationError!",
            [Applications.Chat] = "Chat",
            [Applications.TicTacToe] = "TicTacToe",
        };

        public enum TransceiverData
        {
            DataError,
            TicTacToePosX,
            TicTacToePosY,
            TurnWasMade,
            ChatMessage,
            TicTacToeBoardSize,
            TicTacToeWinSize,
            MyNickname,
            OtherNickname,
            MyIpAddress,
            OtherIpAddress,
            MyApplication,
            OtherApplication,
        }

        private static readonly Dictionary<TransceiverData, string> StringTypesOfData = new Dictionary<TransceiverData, string>()
        {
            [TransceiverData.DataError] = "DataError",
            [TransceiverData.TicTacToePosX] = "TicTacToePosX",
            [TransceiverData.TicTacToePosY] = "TicTacToePosY",
            [TransceiverData.TurnWasMade] = "TurnWasMade",
            [TransceiverData.ChatMessage] = "ChatMessage",
            [TransceiverData.TicTacToeBoardSize] = "TicTacToeBoardSize",
            [TransceiverData.TicTacToeWinSize] = "TicTacToeWinSize",
            [TransceiverData.MyNickname] = "MyNickname",
            [TransceiverData.OtherNickname] = "OtherNickname",
            [TransceiverData.MyIpAddress] = "MyIpAddress",
            [TransceiverData.OtherIpAddress] = "OtherIpAddress",
            [TransceiverData.MyApplication] = "MyApplication",
            [TransceiverData.OtherApplication] = "OtherApplication",
        };

        public static List<string> GetDividedCommands(string message)
        {
            List<string> retList = new List<string>();
            string currentCommand = string.Empty;
            string dividerCommand = string.Empty;
            int dividerSymbols = 0;
            int symbol_number = 0;
            while (symbol_number < message.Length)
            {
                if (message[symbol_number] == GetDividerCommand()[dividerSymbols])
                {
                    dividerCommand += message[symbol_number];
                    dividerSymbols++;
                }
                else
                {
                    if (dividerCommand.Length > 0)
                    {
                        currentCommand += dividerCommand;
                        dividerCommand = string.Empty;
                        dividerSymbols = 0;
                    }
                    currentCommand += message[symbol_number];
                }
                symbol_number++;
                if (dividerCommand == GetDividerCommand())
                {
                    retList.Add(currentCommand);
                    dividerCommand = string.Empty;
                    currentCommand = string.Empty;
                    dividerSymbols = 0;
                }
            }
            if (currentCommand != string.Empty) retList.Add(currentCommand);
            return retList;
        }

        private static string GetStringCommand(CommandFlags command)
        {
            return StringCommands[command];
        }

        public static Applications GetApplicationByString(string app)
        {
            foreach (Applications ap in StringApplications.Keys)
            {
                if (app == StringApplications[ap]) return ap;
            }
            return Applications.ApplicationError;
        }

        public static string SetDataCommand(TransceiverData typeOfData, string data)
        {
            return SetHeader(GetStringCommand(CommandFlags.Data)) + SetTypeOfData(typeOfData) + SetData(data) + GetDividerCommand();
        }

        public static bool IsData(string toCheck)
        {
            return toCheck.StartsWith(SetHeader(GetStringCommand(CommandFlags.Data)) + GetStringCommand(CommandFlags.StartInfo)) &&
                toCheck.EndsWith(GetStringCommand(CommandFlags.EndData));
        }

        public static TransceiverData GetTypeOfData(string command)
        {
            string currentTypeOfData = string.Empty;
            string endInfoFlag = string.Empty;
            int endInfoFlagSymbols = 0;
            int symbol_number = (SetHeader(GetStringCommand(CommandFlags.Data)) + GetStringCommand(CommandFlags.StartInfo)).Length;
            while (symbol_number < command.Length)
            {
                if (command[symbol_number] == GetStringCommand(CommandFlags.EndInfo)[endInfoFlagSymbols])
                {
                    endInfoFlag += command[symbol_number];
                    endInfoFlagSymbols++;
                }
                else
                {
                    if (endInfoFlag.Length > 0)
                    {
                        currentTypeOfData += endInfoFlag;
                        endInfoFlag = string.Empty;
                        endInfoFlagSymbols = 0;
                    }
                    currentTypeOfData += command[symbol_number];
                }
                symbol_number++;
                if (endInfoFlag == GetStringCommand(CommandFlags.EndInfo))
                {
                    return GetTransceiverDataFromString(currentTypeOfData);
                }
            }
            return TransceiverData.DataError;
        }

        public static string GetData(string command, TransceiverData typeOfData)
        {
            string ret = string.Empty;
            int symbolNumber = (SetHeader(GetStringCommand(CommandFlags.Data)) + SetTypeOfData(typeOfData) +
                GetStringCommand(CommandFlags.StartData)).Length;
            while (symbolNumber < command.Length - GetStringCommand(CommandFlags.EndData).Length)
            {
                ret += command[symbolNumber];
                symbolNumber++;
            }
            return ret;
        }

        private static string SetHeader(string toHeader)
        {
            return GetStringCommand(CommandFlags.StartHeader) + toHeader + GetStringCommand(CommandFlags.EndHeader);
        }

        private static string SetInfo(string toInfo)
        {
            return GetStringCommand(CommandFlags.StartInfo) + toInfo + GetStringCommand(CommandFlags.EndInfo);
        }

        private static string SetData(string toData)
        {
            return GetStringCommand(CommandFlags.StartData) + toData + GetStringCommand(CommandFlags.EndData);
        }

        private static string SetTypeOfData(TransceiverData typeOfData)
        {
            return SetInfo(StringTypesOfData[typeOfData]);
        }

        public static string SetJoinCommand(string ipAddress, Applications application, string nickname)
        {
            return SetDataCommand(TransceiverData.OtherIpAddress, ipAddress) + SetDataCommand(TransceiverData.OtherApplication,
                StringApplications[application]) + SetDataCommand(TransceiverData.MyNickname, nickname);
        }

        public static string SetInviteMeCommand(string ipAddress, string nickname)
        {
            return SetDataCommand(TransceiverData.OtherIpAddress, ipAddress) + SetDataCommand(TransceiverData.MyNickname, nickname);
        }

        public static string SetChatMessageCommand(string nickname, string message)
        {
            return SetDataCommand(TransceiverData.MyNickname, nickname) + SetDataCommand(TransceiverData.ChatMessage, message);
        }

        public static string GetAvailableCommand()
        {
            return GetStringCommand(CommandFlags.Available) + GetDividerCommand();
        }

        public static string SetInvitationAcceptedCommand(string nickname)
        {
            return SetDataCommand(TransceiverData.MyNickname, nickname) + GetInvitationAcceptedCommand();
        }

        private static string GetInvitationAcceptedCommand()
        {
            return GetStringCommand(CommandFlags.InvitationAccepted) + GetDividerCommand();
        }

        public static bool IsInvitationAcceptedCommand(string toCheck)
        {
            return toCheck == GetStringCommand(CommandFlags.InvitationAccepted);
        }

        public static string SetInvitationRefusedCommand(string nickname)
        {
            return SetDataCommand(TransceiverData.MyNickname, nickname) + GetInvitationRefusedCommand();
        }

        private static string GetInvitationRefusedCommand()
        {
            return GetStringCommand(CommandFlags.InvitationRefused) + GetDividerCommand();
        }

        public static bool IsInvitationRefusedCommand(string toCheck)
        {
            return toCheck == GetStringCommand(CommandFlags.InvitationRefused);
        }

        public static string SetInvitationCommand(string nickname, Applications application)
        {
            return SetDataCommand(TransceiverData.MyNickname, nickname) +
                SetDataCommand(TransceiverData.OtherApplication, StringApplications[application]) + GetInvitationCommand();
        }

        private static string GetInvitationCommand()
        {
            return GetStringCommand(CommandFlags.Invitation) + GetDividerCommand();
        }

        public static bool IsInvitationCommand(string toCheck)
        {
            return toCheck == GetStringCommand(CommandFlags.Invitation);
        }

        private static string GetDividerCommand()
        {
            return GetStringCommand(CommandFlags.Divider);
        }

        public static string SetTicTacToeTurnCommand(string posX, string posY)
        {
            return SetDataCommand(TransceiverData.TicTacToePosX, posX) + SetDataCommand(TransceiverData.TicTacToePosY, posY) +
                SetDataCommand(TransceiverData.TurnWasMade, "?");
        }

        public static string SetRequestCommand(TransceiverData[] transceiverDatas)
        {
            string datas = string.Empty;
            for (int i = 0; i < transceiverDatas.Length; i++)
            {
                datas += SetHeader(GetStringCommand(CommandFlags.Request)) + SetTypeOfData(transceiverDatas[i]) + GetDividerCommand();
            }
            return datas;
        }

        public static bool IsRequestCommand(string toCheck)
        {
            return toCheck.StartsWith(SetHeader(GetStringCommand(CommandFlags.Request)) + GetStringCommand(CommandFlags.StartInfo)) &&
                toCheck.EndsWith(GetStringCommand(CommandFlags.EndInfo));
        }

        public static TransceiverData GetTransceiverDataFromRequestCommand(string command)
        {
            string ret = string.Empty;
            int symbolNumber = (SetHeader(GetStringCommand(CommandFlags.Request)) + GetStringCommand(CommandFlags.StartInfo)).Length;
            while (symbolNumber < command.Length - GetStringCommand(CommandFlags.EndInfo).Length)
            {
                ret += command[symbolNumber];
                symbolNumber++;
            }
            return GetTransceiverDataFromString(ret);
        }

        private static TransceiverData GetTransceiverDataFromString(string _transceiverData)
        {
            foreach (TransceiverData transceiverData in StringTypesOfData.Keys)
            {
                if (_transceiverData == StringTypesOfData[transceiverData]) return transceiverData;
            }
            return TransceiverData.DataError;
        }

        public class Data
        {
            private readonly TransceiverData transceiverData;
            private readonly object data;

            public Data(TransceiverData _transceiverData, object _data)
            {
                transceiverData = _transceiverData;
                data = _data;
            }

            public TransceiverData GetTransceiverData()
            {
                return transceiverData;
            }

            public bool Is(TransceiverData _transceiverData)
            {
                return transceiverData.Equals(_transceiverData);
            }

            public object GetData()
            {
                return data;
            }
        }
    }
}
