namespace SkiaUI.Gtk;

public interface IUICommand<TState>
{
    string Name { get; }
    bool TryExecute(TState state);
}

public class UICommandBind
{
    public required string Key { get; init; }
    public required string CommandName { get; init; }
    public string? Description { get; init; }
}

public class UICommandLambda<TState> : IUICommand<TState>
{
    public required string Name { get; init; }
    public required Func<TState, bool> Action { get; init; }

    public bool TryExecute(TState state) => Action(state);
}

public class UICommandCollection<TState>
{
    Dictionary<string, IUICommand<TState>> commands = new();
    List<UICommandBind> binds = new();


    public UICommandCollection<TState> Bind(UICommandBind bind)
    {
        binds.Add(bind);
        return this;
    }

    public UICommandCollection<TState> Bind(string key, string cmdName, Func<TState, bool> action)
    {
        return Bind(key, new UICommandLambda<TState>()
                {
                    Name = cmdName,
                    Action = action
                });
    }

    public UICommandCollection<TState> Bind(string key, IUICommand<TState> cmd)
    {
        commands[cmd.Name] = cmd;
        return Bind(new UICommandBind() { Key = key, CommandName = cmd.Name });
    }

    public IUICommand<TState> Add(IUICommand<TState> cmd)
    {
        commands.Add(cmd.Name, cmd);
        return cmd;
    }

    public bool TryExecuteWithKey(TState state,string key, out IUICommand<TState>? cmd)
    {
        cmd = null;
        foreach(var bind in binds)
        {
            if (bind.Key == key)
            {
                cmd = commands[bind.CommandName];
                return  cmd.TryExecute(state);
            }
        }
        return false;
    }

    public bool TryExecute(TState state, string name)
    {
        foreach(var cmd in commands)
        {
            if (cmd.Key == name)
            {
                return cmd.Value.TryExecute(state);
            }
        }
        return false;
    }

}


