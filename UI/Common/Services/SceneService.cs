namespace Macabresoft.Macabre2D.UI.Common.Services {
    using System;
    using System.ComponentModel;
    using System.IO;
    using System.Linq;
    using Macabresoft.Macabre2D.Framework;
    using Macabresoft.Macabre2D.UI.Common.Models.Content;
    using ReactiveUI;

    /// <summary>
    /// Interface for a service which handles the <see cref="IScene" /> open in the editor.
    /// </summary>
    public interface ISceneService : INotifyPropertyChanged {
        /// <summary>
        /// Gets the current scene.
        /// </summary>
        /// <value>The current scene.</value>
        IScene CurrentScene { get; }

        /// <summary>
        /// Gets the current scene metadata.
        /// </summary>
        ContentMetadata CurrentSceneMetadata { get; }

        /// <summary>
        /// Creates the new scene and serializes it.
        /// </summary>
        /// <param name="parent">The parent content directory.</param>
        /// <param name="sceneName">The name of the scene without its file extension.</param>
        /// <returns>The newly created scene wrapped in a <see cref="SceneAsset" />.</returns>
        SceneAsset CreateNewScene(IContentDirectory parent, string sceneName);

        /// <summary>
        /// Saves the current scene.
        /// </summary>
        void SaveScene();

        /// <summary>
        /// Tries to load a scene.
        /// </summary>
        /// <param name="contentId">The content identifier of the scene.</param>
        /// <param name="sceneAsset">The scene asset.</param>
        /// <returns>A value indicating whether or not the scene was loaded.</returns>
        bool TryLoadScene(Guid contentId, out SceneAsset sceneAsset);
    }

    /// <summary>
    /// A service which handles the <see cref="IScene" /> open in the editor.
    /// </summary>
    public sealed class SceneService : ReactiveObject, ISceneService {
        private readonly IAssetManager _assetManager;
        private readonly IFileSystemService _fileSystem;
        private readonly IPathService _pathService;
        private readonly ISerializer _serializer;
        private ContentMetadata _currentSceneMetadata;

        /// <summary>
        /// Initializes a new instance of the <see cref="SceneService" /> class.
        /// </summary>
        /// <param name="assetManager">The asset manager.</param>
        /// <param name="fileSystem">The file system service.</param>
        /// <param name="pathService">The path service.</param>
        /// <param name="serializer">The serializer.</param>
        public SceneService(
            IAssetManager assetManager,
            IFileSystemService fileSystem,
            IPathService pathService,
            ISerializer serializer) {
            this._assetManager = assetManager;
            this._fileSystem = fileSystem;
            this._pathService = pathService;
            this._serializer = serializer;
        }

        /// <inheritdoc />
        public IScene CurrentScene => (this._currentSceneMetadata?.Asset as SceneAsset)?.Content;

        /// <inheritdoc />
        public ContentMetadata CurrentSceneMetadata {
            get => this._currentSceneMetadata;
            private set {
                this.RaiseAndSetIfChanged(ref this._currentSceneMetadata, value);
                this.RaisePropertyChanged(nameof(this.CurrentScene));
            }
        }

        /// <inheritdoc />
        public SceneAsset CreateNewScene(IContentDirectory parent, string sceneName) {
            if (this.CurrentScene != null) {
                // TODO: ask user if they want to save or cancel changes. This should actually be asked somewhere that SaveService is available.
                this.SaveScene();
            }

            var contentDirectoryPath = parent.GetFullPath();
            if (!this._fileSystem.DoesDirectoryExist(contentDirectoryPath)) {
                throw new DirectoryNotFoundException();
            }

            var filePath = Path.Combine(contentDirectoryPath, $"{sceneName}{SceneAsset.FileExtension}");
            if (this._fileSystem.DoesFileExist(filePath)) {
                // TODO: Overwrite warning.
            }

            var scene = new Scene {
                BackgroundColor = DefinedColors.MacabresoftPurple,
                Name = sceneName
            };

            scene.AddSystem<UpdateSystem>();
            scene.AddSystem<RenderSystem>();
            scene.AddChild<Camera>();

            var sceneAsset = new SceneAsset {
                Name = scene.Name
            };

            sceneAsset.LoadContent(scene);
            var contentPath = Path.Combine(parent.GetContentPath(), sceneName);
            var metadata = new ContentMetadata(
                sceneAsset,
                contentPath.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar, StringSplitOptions.RemoveEmptyEntries).ToList(),
                SceneAsset.FileExtension);

            this.CurrentSceneMetadata = metadata;

            this.SaveScene();

            this._assetManager.RegisterMetadata(metadata);
            this.CreateContentFile(parent, metadata);
            return sceneAsset;
        }

        /// <inheritdoc />
        public void SaveScene() {
            if (this.CurrentSceneMetadata != null && this.CurrentScene != null) {
                var metadataPath = this._pathService.GetMetadataFilePath(this.CurrentSceneMetadata.ContentId);
                this._serializer.Serialize(this.CurrentSceneMetadata, metadataPath);

                var filePath = Path.Combine(this._pathService.ContentDirectoryPath, this.CurrentSceneMetadata.GetContentPath());
                this._serializer.Serialize(this.CurrentScene, filePath);
            }
        }

        /// <inheritdoc />
        public bool TryLoadScene(Guid contentId, out SceneAsset sceneAsset) {
            if (contentId == Guid.Empty) {
                sceneAsset = null;
            }
            else {
                var metadataFilePath = this._pathService.GetMetadataFilePath(contentId);
                if (this._fileSystem.DoesFileExist(metadataFilePath)) {
                    var metadata = this._serializer.Deserialize<ContentMetadata>(metadataFilePath);
                    sceneAsset = metadata?.Asset as SceneAsset;

                    if (metadata != null && sceneAsset != null) {
                        var contentPath = Path.Combine(this._pathService.ContentDirectoryPath, metadata.GetContentPath());

                        if (this._fileSystem.DoesFileExist(contentPath)) {
                            var scene = this._serializer.Deserialize<Scene>(contentPath);
                            if (scene != null) {
                                sceneAsset.LoadContent(scene);
                                this.CurrentSceneMetadata = metadata;
                            }
                        }
                    }
                }
                else {
                    sceneAsset = null;
                }
            }

            return sceneAsset != null;
        }

        private ContentFile CreateContentFile(IContentDirectory parent, ContentMetadata metadata) {
            return new(parent, metadata);
        }
    }
}