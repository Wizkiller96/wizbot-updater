using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using ReactiveUI;

namespace wizbotupdater.ViewModels
{
    /// <summary>
    /// Base Viewmodel that all bot dependencies inherit from
    /// </summary>
    public abstract class DepViewModel : ViewModelBase
    {
        #region Properties

        /// <summary>
        /// Name of the dependency
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Command to install the dependency
        /// </summary>
        public IAsyncRelayCommand InstallCommand { get; }

        /// <summary>
        /// State of the dependency
        /// </summary>
        private DepState _state;

        public DepState State
        {
            get => _state;
            set
            {
                var oldValue = _state;
                var newValue = this.RaiseAndSetIfChanged(ref _state, value);
                if (oldValue != newValue)
                {
                    // Notify that the derived properties have also changed
                    this.RaisePropertyChanged(nameof(IsChecking));
                    this.RaisePropertyChanged(nameof(IsNotInstalled));
                    this.RaisePropertyChanged(nameof(IsInstalled));
                }
            }
        }

        /// <summary>
        /// Gets whether the dependency is currently being checked
        /// </summary>
        public bool IsChecking
            => State == DepState.Checking;

        /// <summary>
        /// Gets whether the dependency is not installed
        /// </summary>
        public bool IsNotInstalled
            => State == DepState.NotInstalled;

        /// <summary>
        /// Gets whether the dependency is installed
        /// </summary>
        public bool IsInstalled
            => State == DepState.Installed;

        #endregion

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="name">Name of the dependency</param>
        public DepViewModel(string name)
        {
            Name = name;
            InstallCommand = new AsyncRelayCommand(InstallAsync);
            State = DepState.Checking;
        }

        #region Methods

        /// <summary>
        /// Called to install the dependency.
        /// It will only work if <see cref="State"/> is <see cref="DepState.NotInstalled"/> or <see cref="DepState.UpdateNeeded"/>
        /// </summary>
        public async Task InstallAsync()
        {
            if (State == DepState.NotInstalled)
            {
                State = DepState.Checking;

                var success = await InternalInstallAsync();
                if (!success)
                    State = DepState.NotInstalled;
                else
                    State = DepState.Installed;

                return;
            }

            throw new InvalidOperationException(
                "You can only install the dependency if it is in NotInstalled or UpdateNeeded state.");
        }

        /// <summary>
        /// Called to check whether the dependency is installed. It will update <see cref="State"/>
        /// </summary>
        public async Task CheckAsync()
        {
            State = DepState.Checking;
            try
            {
                // foreach (DictionaryEntry var in Environment.GetEnvironmentVariables(EnvironmentVariableTarget.User))
                // {
                //     if (!(var.Key is null) && !(var.Value is null))
                //         Environment.SetEnvironmentVariable(var.Key.ToString(),
                //             var.Value.ToString(),
                //             EnvironmentVariableTarget.Process);
                // }
            }
            catch
            {
                // todo: Error message box
            }

            var newState = await InternalCheckAsync();
            State = newState;
        }

        /// <summary>
        /// Needs to be overriden by inheriting classes to provide state checking mechanism
        /// </summary>
        /// <returns></returns>
        protected abstract Task<DepState> InternalCheckAsync();

        /// <summary>
        /// Needs to be overriden by inheriting classes to provide installation mechanism
        /// </summary>
        /// <returns></returns>
        protected abstract Task<bool> InternalInstallAsync();

        #endregion
    }
}

public enum DepState
{
    Checking,
    NotInstalled,
    Installed,
}