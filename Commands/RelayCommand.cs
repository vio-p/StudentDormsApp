using System;

namespace StudentDormsApp.Commands;

public class RelayCommand : CommandBase
{
    private readonly Action? _commandTask;
    private readonly Action<object> _parameterizedCommandTask;
    private readonly Predicate<object> _canExecuteTask;

    public RelayCommand(Action commandTask, Predicate<object> canExecuteTask)
    {
        _commandTask = commandTask;
        _canExecuteTask = canExecuteTask;
    }

    public RelayCommand(Action<object> commandTask, Predicate<object> canExecuteTask)
    {
        _parameterizedCommandTask = commandTask;
        _canExecuteTask = canExecuteTask;
    }

    private static bool DefaultCanExecute(object parameter)
    {
        return true;
    }

    public RelayCommand(Action commandTask)
        : this(commandTask, DefaultCanExecute)
    {
        _commandTask = commandTask;
    }

    public RelayCommand(Action<object> commandTask)
        : this(commandTask, DefaultCanExecute)
    {
        _parameterizedCommandTask = commandTask;
    }

    public override bool CanExecute(object? parameter)
    {
        return _canExecuteTask != null && _canExecuteTask(parameter!);
    }

    public override void Execute(object? parameter)
    {
        if (_commandTask != null)
        {
            _commandTask();
        }
        else
        {
            _parameterizedCommandTask(parameter!);
        }
    }
}
