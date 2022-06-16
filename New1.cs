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
        };

        private static string GetStringCommand(Commands command)
        {
            return StringCommands[command];
        }

        public static bool IsCommand(string toCheck)
        {
            if (toCheck.Length < GetStringCommand(Commands.StartCommandFlag).Length + GetStringCommand(Commands.EndCommandFlag).Length) return false;
            if (toCheck.StartsWith(GetStringCommand(Commands.StartCommandFlag)) && toCheck.EndsWith(GetStringCommand(Commands.EndCommandFlag))) return true;
            return false;
            //bool isCommand = true;
            //for (int i = 0; i < GetStringCommand(Commands.StartCommandFlag).Length; i++)
            //{
            //    if (toCheck[i] != GetStringCommand(Commands.StartCommandFlag)[i])
            //    {
            //        isCommand = false;
            //        break;
            //    }
            //}
            //for (int i = 0; i < GetStringCommand(Commands.EndCommandFlag).Length; i++)
            //{
            //    if (toCheck[toCheck.Length - i - 1] != GetStringCommand(Commands.EndCommandFlag)[GetStringCommand(Commands.EndCommandFlag).Length - i - 1])
            //    {
            //        isCommand = false;
            //        break;
            //    }
            //}
            //return isCommand;
        }

        public static bool IsIpAddress(string toCheck)
        {
            return toCheck.Contains(GetStringCommand(Commands.IpAddressFlag)) && IsCommand(toCheck);
        }

        public static string GetIpAddress(string ip)
        {
            string retIP = string.Empty;
            int ii = GetStringCommand(Commands.StartCommandFlag).Length + GetStringCommand(Commands.StartHeaderFlag).Length +
                GetStringCommand(Commands.IpAddressFlag).Length + GetStringCommand(Commands.EndHeaderFlag).Length + GetStringCommand(Commands.StartInfoFlag).Length;
            while (ii < ip.Length)
            {
                if (ip[ii] == '<') break;
                retIP += ip[ii];
                ii++;
            }
            return retIP;
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

        public static string SetIpAddressToCommand(string toCommand)
        {
            return SetStringToCommand(SetHeader(GetStringCommand(Commands.IpAddressFlag)) + SetInfo(toCommand));
        }

        public static string GetAvailableCommand()
        {
            return SetStringToCommand(GetStringCommand(Commands.AvailableFlag));
        }

        public static bool IsAvailableCommand(string toCheck)
        {
            return toCheck == GetAvailableCommand();
        }
    }
}
