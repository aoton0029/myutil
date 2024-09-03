

public class CommandResponse
{
    public string Command { get; set; }
    public string Response { get; set; }
    public bool IsError { get; set; }

    public CommandResponse(string command, string response, bool isError = false)
    {
        Command = command;
        Response = response;
        IsError = isError;
    }
}

public class DeviceScpiManager
{
    public string DeviceName { get; private set; }
    private List<CommandResponse> commandResponses;

    public DeviceScpiManager(string deviceName)
    {
        DeviceName = deviceName;
        commandResponses = new List<CommandResponse>();
    }

    public void AddCommandResponse(CommandResponse commandResponse)
    {
        commandResponses.Add(commandResponse);
    }

    public string GetResponse(string command)
    {
        var response = commandResponses.FirstOrDefault(c => c.Command == command);
        return response != null ? response.Response : "Unknown Command";
    }
}

public class CommandMaster
{
    private Dictionary<string, CommandResponse> commandMaster;

    public CommandMaster()
    {
        commandMaster = new Dictionary<string, CommandResponse>();
    }

    // コマンドを追加するメソッド
    public void AddCommand(string command, string response, bool isError = false)
    {
        if (!commandMaster.ContainsKey(command))
        {
            commandMaster[command] = new CommandResponse(command, response, isError);
        }
    }

    // コマンドを取得するメソッド
    public CommandResponse GetCommandResponse(string command)
    {
        return commandMaster.ContainsKey(command) ? commandMaster[command] : null;
    }

    // すべてのコマンドをリストで取得するメソッド
    public List<CommandResponse> GetAllCommands()
    {
        return commandMaster.Values.ToList();
    }
}

public class ErrorMaster
{
    private Dictionary<int, string> errorMaster;

    public ErrorMaster()
    {
        errorMaster = new Dictionary<int, string>();
    }

    // エラーを追加するメソッド
    public void AddError(int errorCode, string errorMessage)
    {
        if (!errorMaster.ContainsKey(errorCode))
        {
            errorMaster[errorCode] = errorMessage;
        }
    }

    // エラーメッセージを取得するメソッド
    public string GetErrorMessage(int errorCode)
    {
        return errorMaster.ContainsKey(errorCode) ? errorMaster[errorCode] : "Unknown error";
    }

    // すべてのエラーをリストで取得するメソッド
    public List<KeyValuePair<int, string>> GetAllErrors()
    {
        return errorMaster.ToList();
    }
}

public class MasterManager
{
    public CommandMaster CommandMaster { get; private set; }
    public ErrorMaster ErrorMaster { get; private set; }

    public MasterManager()
    {
        CommandMaster = new CommandMaster();
        ErrorMaster = new ErrorMaster();
    }

    // 必要に応じて、コマンドとエラーを一度に設定するメソッドなども追加可能
}

public class CommandResponse
{
    public string Command { get; set; }
    public List<string> Responses { get; set; } // 複数の応答をサポート
    public bool IsError { get; set; }
    public Dictionary<string, string> Options { get; set; } // オプションをサポート

    public CommandResponse(string command, bool isError = false)
    {
        Command = command;
        Responses = new List<string>();
        IsError = isError;
        Options = new Dictionary<string, string>();
    }

    public void AddResponse(string response)
    {
        Responses.Add(response);
    }

    public void AddOption(string key, string value)
    {
        Options[key] = value;
    }

    public string GetResponse()
    {
        // 必要に応じて、応答を選択するロジックを追加
        return Responses.FirstOrDefault();
    }
}

public class CommandMaster
{
    private Dictionary<string, CommandResponse> commandMaster;

    public CommandMaster()
    {
        commandMaster = new Dictionary<string, CommandResponse>();
    }

    public void AddCommand(CommandResponse commandResponse)
    {
        if (!commandMaster.ContainsKey(commandResponse.Command))
        {
            commandMaster[commandResponse.Command] = commandResponse;
        }
    }

    public CommandResponse GetCommandResponse(string command)
    {
        return commandMaster.ContainsKey(command) ? commandMaster[command] : null;
    }

    public List<CommandResponse> GetAllCommands()
    {
        return commandMaster.Values.ToList();
    }

    public void SaveToFile(string filePath)
    {
        // ファイルにコマンドデータを保存するロジック（例：JSON形式）
        var json = Newtonsoft.Json.JsonConvert.SerializeObject(commandMaster, Newtonsoft.Json.Formatting.Indented);
        System.IO.File.WriteAllText(filePath, json);
    }

    public void LoadFromFile(string filePath)
    {
        // ファイルからコマンドデータをロードするロジック（例：JSON形式）
        if (System.IO.File.Exists(filePath))
        {
            var json = System.IO.File.ReadAllText(filePath);
            commandMaster = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, CommandResponse>>(json);
        }
    }
}


public class ErrorMaster
{
    private Dictionary<int, string> errorMaster;

    public ErrorMaster()
    {
        errorMaster = new Dictionary<int, string>();
    }

    public void AddError(int errorCode, string errorMessage)
    {
        if (!errorMaster.ContainsKey(errorCode))
        {
            errorMaster[errorCode] = errorMessage;
        }
    }

    public string GetErrorMessage(int errorCode)
    {
        return errorMaster.ContainsKey(errorCode) ? errorMaster[errorCode] : "Unknown error";
    }

    public List<KeyValuePair<int, string>> GetAllErrors()
    {
        return errorMaster.ToList();
    }

    public void SaveToFile(string filePath)
    {
        // ファイルにエラーデータを保存するロジック（例：JSON形式）
        var json = Newtonsoft.Json.JsonConvert.SerializeObject(errorMaster, Newtonsoft.Json.Formatting.Indented);
        System.IO.File.WriteAllText(filePath, json);
    }

    public void LoadFromFile(string filePath)
    {
        // ファイルからエラーデータをロードするロジック（例：JSON形式）
        if (System.IO.File.Exists(filePath))
        {
            var json = System.IO.File.ReadAllText(filePath);
            errorMaster = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<int, string>>(json);
        }
    }
}

public class MasterManager
{
    public CommandMaster CommandMaster { get; private set; }
    public ErrorMaster ErrorMaster { get; private set; }

    public MasterManager()
    {
        CommandMaster = new CommandMaster();
        ErrorMaster = new ErrorMaster();
    }

    public void SaveAllData(string commandFilePath, string errorFilePath)
    {
        CommandMaster.SaveToFile(commandFilePath);
        ErrorMaster.SaveToFile(errorFilePath);
    }

    public void LoadAllData(string commandFilePath, string errorFilePath)
    {
        CommandMaster.LoadFromFile(commandFilePath);
        ErrorMaster.LoadFromFile(errorFilePath);
    }
}

