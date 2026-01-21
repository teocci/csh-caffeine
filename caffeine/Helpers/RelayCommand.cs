using System.Windows.Input;

namespace caffeine.Helpers;

/// <summary>
/// A command implementation for MVVM pattern that relays its functionality to other objects.
/// </summary>
public class RelayCommand : ICommand
{
    private readonly Action _execute;
    private readonly Func<bool>? _canExecute;

    /// <summary>
    /// Initializes a new instance of the RelayCommand class.
    /// </summary>
    /// <param name="execute">The execution logic.</param>
    /// <param name="canExecute">The execution status logic.</param>
    public RelayCommand(Action execute, Func<bool>? canExecute = null)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _canExecute = canExecute;
    }

    /// <summary>
    /// Occurs when changes occur that affect whether the command should execute.
    /// </summary>
    public event EventHandler? CanExecuteChanged
    {
        add => CommandManager.RequerySuggested += value;
        remove => CommandManager.RequerySuggested -= value;
    }

    /// <summary>
    /// Determines whether the command can execute in its current state.
    /// </summary>
    /// <param name="parameter">Data used by the command (not used).</param>
    /// <returns>true if the command can be executed; otherwise, false.</returns>
    public bool CanExecute(object? parameter)
    {
        return _canExecute == null || _canExecute();
    }

    /// <summary>
    /// Executes the command logic.
    /// </summary>
    /// <param name="parameter">Data used by the command (not used).</param>
    public void Execute(object? parameter)
    {
        _execute();
    }

    /// <summary>
    /// Raises the CanExecuteChanged event.
    /// </summary>
    public void RaiseCanExecuteChanged()
    {
        CommandManager.InvalidateRequerySuggested();
    }
}

/// <summary>
/// A generic command implementation for MVVM pattern that relays its functionality to other objects.
/// </summary>
/// <typeparam name="T">The type of the command parameter.</typeparam>
public class RelayCommand<T> : ICommand
{
    private readonly Action<T?> _execute;
    private readonly Func<T?, bool>? _canExecute;

    /// <summary>
    /// Initializes a new instance of the RelayCommand class.
    /// </summary>
    /// <param name="execute">The execution logic.</param>
    /// <param name="canExecute">The execution status logic.</param>
    public RelayCommand(Action<T?> execute, Func<T?, bool>? canExecute = null)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _canExecute = canExecute;
    }

    /// <summary>
    /// Occurs when changes occur that affect whether the command should execute.
    /// </summary>
    public event EventHandler? CanExecuteChanged
    {
        add => CommandManager.RequerySuggested += value;
        remove => CommandManager.RequerySuggested -= value;
    }

    /// <summary>
    /// Determines whether the command can execute in its current state.
    /// </summary>
    /// <param name="parameter">Data used by the command.</param>
    /// <returns>true if the command can be executed; otherwise, false.</returns>
    public bool CanExecute(object? parameter)
    {
        if (parameter is null && typeof(T).IsValueType && Nullable.GetUnderlyingType(typeof(T)) == null)
            return false;

        return _canExecute == null || _canExecute((T?)parameter);
    }

    /// <summary>
    /// Executes the command logic.
    /// </summary>
    /// <param name="parameter">Data used by the command.</param>
    public void Execute(object? parameter)
    {
        _execute((T?)parameter);
    }

    /// <summary>
    /// Raises the CanExecuteChanged event.
    /// </summary>
    public void RaiseCanExecuteChanged()
    {
        CommandManager.InvalidateRequerySuggested();
    }
}
