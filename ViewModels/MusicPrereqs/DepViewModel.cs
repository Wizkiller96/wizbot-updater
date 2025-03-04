using System;
using System.Collections;
using System.Threading.Tasks;
using upeko.ViewModels;

namespace upeko.ViewModels
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
        /// State of the dependency
        /// </summary>
        public DepState State { get; private set; }
        #endregion

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="name">Name of the dependency</param>
        public DepViewModel(string name)
        {
            Name = name;
            State = DepState.Checking;
        }

        #region Methods

        /// <summary>
        /// Called to install the dependency.
        /// It will only work if <see cref="State"/> is <see cref="DepState.NotInstalled"/> or <see cref="DepState.UpdateNeeded"/>
        /// </summary>
        public async Task InstallAsync()
        {
            if (State == DepState.NotInstalled || State == DepState.UpdateNeeded)
            {
                State = DepState.Checking;

                var success = await InternalInstallAsync();
                if (!success)
                    State = DepState.NotInstalled;
                else
                    State = DepState.Installed;

                return;
            }

            throw new InvalidOperationException("You can only install the dependency if it is in NotInstalled or UpdateNeeded state.");
        }

        /// <summary>
        /// Called to check whether the dependency is installed. It will update <see cref="State"/>
        /// </summary>
        public async Task CheckAsync()
        {
            State = DepState.Checking;
            foreach (DictionaryEntry var in Environment.GetEnvironmentVariables(EnvironmentVariableTarget.User))
            {
                if (!(var.Key is null) && !(var.Value is null))
                    Environment.SetEnvironmentVariable(var.Key.ToString(), var.Value.ToString(), EnvironmentVariableTarget.Process);
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
    UpdateNeeded,
}