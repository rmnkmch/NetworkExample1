using System.Collections.Generic;

namespace LTTDIT.Net
{
    public static class Information
    {
        public enum Commands
        {
            StartCommandFlag,
            EndCommandFlag,
            StartHeaderFlag,
            EndHeaderFlag,
            VersionFlag,
            ApplicationFlag,
            AvailableFlag,
            StartInfoFlag,
            EndInfoFlag,
            IpAddressFlag,
            Divider,
            NicknameFlag,
        }

        public static readonly Dictionary<Commands, string> StringCommands = new Dictionary<Commands, string>()
        {
            [Commands.StartCommandFlag] = "<!>",
            [Commands.EndCommandFlag] = "</!>",
            [Commands.StartHeaderFlag] = "<header>",
            [Commands.EndHeaderFlag] = "</header>",
            [Commands.VersionFlag] = "Version",
            [Commands.ApplicationFlag] = "Application",
            [Commands.AvailableFlag] = "Available",
            [Commands.StartInfoFlag] = "<info>",
            [Commands.EndInfoFlag] = "</info>",
            [Commands.IpAddressFlag] = "IPAddress",
            [Commands.Divider] = "<_!_>",
            [Commands.NicknameFlag] = "Nickname",
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

        public static List<string> GetDividedCommands(string message)
        {
            List<string> retList = new List<string>();
            string currentCommand = string.Empty;
            string dividerCommand = string.Empty;
            int dividerSymbols = 0;
            int symbol_number = GetStringCommand(Commands.StartCommandFlag).Length;
            while (symbol_number < message.Length - GetStringCommand(Commands.EndCommandFlag).Length)
            {
                if (message[symbol_number] == GetStringCommand(Commands.Divider)[dividerSymbols])
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
                        currentCommand += message[symbol_number];
                    }
                    else currentCommand += message[symbol_number];
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
            retList.Add(currentCommand);
            return retList;
        }

        private static string GetStringCommand(Commands command)
        {
            return StringCommands[command];
        }

        public static bool IsCommand(string toCheck)
        {
            if (toCheck.Length < GetStringCommand(Commands.StartCommandFlag).Length + GetStringCommand(Commands.EndCommandFlag).Length) return false;
            if (toCheck.StartsWith(GetStringCommand(Commands.StartCommandFlag)) && toCheck.EndsWith(GetStringCommand(Commands.EndCommandFlag))) return true;
            return false;
        }

        public static bool IsIpAddress(string toCheck)
        {
            return toCheck.StartsWith(SetHeader(GetStringCommand(Commands.IpAddressFlag)) + GetStringCommand(Commands.StartInfoFlag)) &&
                toCheck.EndsWith(GetStringCommand(Commands.EndInfoFlag));
        }

        public static string GetIpAddress(string ip)
        {
            string retIP = string.Empty;
            int ii = SetHeader(GetStringCommand(Commands.IpAddressFlag)).Length + GetStringCommand(Commands.StartInfoFlag).Length;
            while (ii < ip.Length - GetStringCommand(Commands.EndInfoFlag).Length)
            {
                retIP += ip[ii];
                ii++;
            }
            return retIP;
        }

        public static bool IsNickname(string toCheck)
        {
            return toCheck.StartsWith(SetHeader(GetStringCommand(Commands.NicknameFlag)) + GetStringCommand(Commands.StartInfoFlag)) &&
                toCheck.EndsWith(GetStringCommand(Commands.EndInfoFlag));
        }

        public static string GetNickname(string command)
        {
            string ret = string.Empty;
            int symbolNumber = SetHeader(GetStringCommand(Commands.NicknameFlag)).Length + GetStringCommand(Commands.StartInfoFlag).Length;
            while (symbolNumber < command.Length - GetStringCommand(Commands.EndInfoFlag).Length)
            {
                ret += command[symbolNumber];
                symbolNumber++;
            }
            return ret;
        }

        public static bool IsApplication(string toCheck)
        {
            return toCheck.StartsWith(SetHeader(GetStringCommand(Commands.ApplicationFlag)) + GetStringCommand(Commands.StartInfoFlag)) &&
                toCheck.EndsWith(GetStringCommand(Commands.EndInfoFlag));
        }

        public static Applications GetApplication(string command)
        {
            string ret = string.Empty;
            int symbolNumber = SetHeader(GetStringCommand(Commands.ApplicationFlag)).Length + GetStringCommand(Commands.StartInfoFlag).Length;
            while (symbolNumber < command.Length - GetStringCommand(Commands.EndInfoFlag).Length)
            {
                ret += command[symbolNumber];
                symbolNumber++;
            }
            return GetApplicationByString(ret);
        }

        private static Applications GetApplicationByString(string app)
        {
            foreach (Applications ap in StringApplications.Keys)
            {
                if (app == StringApplications[ap]) return ap;
            }
            return Applications.ApplicationError;
        }

        private static string SetStringToCommand(string toCommand)
        {
            return GetStringCommand(Commands.StartCommandFlag) + toCommand + GetStringCommand(Commands.EndCommandFlag);
        }

        private static string SetHeader(string toHeader)
        {
            return GetStringCommand(Commands.StartHeaderFlag) + toHeader + GetStringCommand(Commands.EndHeaderFlag);
        }

        private static string SetInfo(string toInfo)
        {
            return GetStringCommand(Commands.StartInfoFlag) + toInfo + GetStringCommand(Commands.EndInfoFlag);
        }

        public static string SetJoinCommand(string ipAddress, Applications application, string nickname)
        {
            return SetStringToCommand(SetHeader(GetStringCommand(Commands.IpAddressFlag)) + SetInfo(ipAddress) +
                GetDividerCommand() + SetHeader(GetStringCommand(Commands.ApplicationFlag)) + SetInfo(StringApplications[application]) +
                GetDividerCommand() + SetHeader(GetStringCommand(Commands.NicknameFlag)) + SetInfo(nickname));
        }

        public static string GetAvailableCommand()
        {
            return SetStringToCommand(GetStringCommand(Commands.AvailableFlag));
        }

        public static bool IsAvailableCommand(string toCheck)
        {
            return toCheck == GetAvailableCommand();
        }

        public static string GetDividerCommand()
        {
            return GetStringCommand(Commands.Divider);
        }
    }
}
