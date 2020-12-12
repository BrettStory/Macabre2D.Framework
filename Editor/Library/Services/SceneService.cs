﻿namespace Macabresoft.Macabre2D.Editor.Library.Services {

    using Macabresoft.Macabre2D.Framework;
    using ReactiveUI;
    using System.ComponentModel;

    /// <summary>
    /// Interface for a service which handles the <see cref="IGameScene" /> open in the editor.
    /// </summary>
    public interface ISceneService : INotifyPropertyChanged {

        /// <summary>
        /// Gets the current scene.
        /// </summary>
        /// <value>The current scene.</value>
        public IGameScene CurrentScene { get; }

        /// <summary>
        /// Creates the new scene and assigns it to <see cref="CurrentScene" />.
        /// </summary>
        /// <typeparam name="T">The type of scene.</typeparam>
        /// <returns>The newly created scene.</returns>
        T CreateNewScene<T>() where T : IGameScene, new();
    }

    /// <summary>
    /// A service which handles the <see cref="IGameScene" /> open in the editor.
    /// </summary>
    public sealed class SceneService : ReactiveObject, ISceneService {
        private IGameScene _currentScene;

        /// <inheritdoc />
        public IGameScene CurrentScene {
            get {
                return this._currentScene;
            }

            private set {
                this.RaiseAndSetIfChanged(ref this._currentScene, value);
            }
        }

        /// <inheritdoc />
        public T CreateNewScene<T>() where T : IGameScene, new() {
            var scene = new T();
            this.CurrentScene = scene;
            return scene;
        }
    }
}