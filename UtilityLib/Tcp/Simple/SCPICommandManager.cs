using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilityLib.Tcp.Simple
{
    public class SCPICommandManager
    {
        // コマンドリストを格納する辞書
        private Dictionary<string, string> commandList;
        private string delimiter;
        private string terminator;

        public SCPICommandManager(string delimiter = " ", string terminator = "\n")
        {
            this.commandList = new Dictionary<string, string>();
            this.delimiter = delimiter;
            this.terminator = terminator;
        }

        // コマンドを登録するメソッド
        public void RegisterCommand(string commandName, string commandFormat)
        {
            if (!commandList.ContainsKey(commandName))
            {
                commandList.Add(commandName, commandFormat);
            }
            else
            {
                commandList[commandName] = commandFormat;
            }
        }

        // 設定コマンドを生成するメソッド
        public string CreateSetCommand(string commandName, string value, string option = "")
        {
            if (commandList.ContainsKey(commandName))
            {
                var commandFormat = commandList[commandName];
                StringBuilder commandBuilder = new StringBuilder();
                commandBuilder.Append(commandName)
                              .Append(delimiter)
                              .Append(value);

                if (!string.IsNullOrEmpty(option))
                {
                    commandBuilder.Append(delimiter).Append(option);
                }

                commandBuilder.Append(terminator);

                return commandBuilder.ToString();
            }
            else
            {
                throw new ArgumentException("Command not found.");
            }
        }

        // クエリコマンドを生成するメソッド
        public string CreateQueryCommand(string commandName)
        {
            if (commandList.ContainsKey(commandName))
            {
                StringBuilder commandBuilder = new StringBuilder();
                commandBuilder.Append(commandName)
                              .Append("?")
                              .Append(terminator);

                return commandBuilder.ToString();
            }
            else
            {
                throw new ArgumentException("Command not found.");
            }
        }

        // 登録されているすべてのコマンドを取得するメソッド
        public Dictionary<string, string> GetAllCommands()
        {
            return new Dictionary<string, string>(commandList);
        }
    }

}
