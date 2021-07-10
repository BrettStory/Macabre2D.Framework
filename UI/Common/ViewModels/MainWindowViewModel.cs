namespace Macabresoft.Macabre2D.UI.Common.ViewModels {
    using System;
    using System.ComponentModel;
    using System.Reactive.Linq;
    using System.Threading.Tasks;
    using System.Windows.Input;
    using Avalonia;
    using Avalonia.Controls;
    using Avalonia.Platform;
    using Macabresoft.Core;
    using Macabresoft.Macabre2D.UI.Common.Models;
    using Macabresoft.Macabre2D.UI.Common.Services;
    using ReactiveUI;
    using Unity;

    /// <summary>
    /// The view model for the main window.
    /// </summary>
    public class MainWindowViewModel : ViewModelBase {
        private readonly IDialogService _dialogService;
        private readonly IProjectService _projectService;
        private readonly ISceneService _sceneService;
        private readonly IUndoService _undoService;

        /// <summary>
        /// Initializes a new instance of the <see cref="MainWindowViewModel" /> class.
        /// </summary>
        /// <remarks>This constructor only exists for design time XAML.</remarks>
        public MainWindowViewModel() : base() {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MainWindowViewModel" /> class.
        /// </summary>
        /// <param name="dialogService">The dialog service.</param>
        /// <param name="entitySelectionService">The selection service.</param>
        /// <param name="sceneService">The scene service.</param>
        /// <param name="projectService">The project service.</param>
        /// <param name="undoService">The undo service.</param>
        [InjectionConstructor]
        public MainWindowViewModel(
            IDialogService dialogService,
            IEntitySelectionService entitySelectionService,
            IProjectService projectService,
            ISceneService sceneService,
            IUndoService undoService) : base() {
            this._dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
            this.EntitySelectionService = entitySelectionService ?? throw new ArgumentNullException(nameof(entitySelectionService));
            this._projectService = projectService ?? throw new ArgumentNullException(nameof(projectService));
            this._sceneService = sceneService ?? throw new ArgumentNullException(nameof(sceneService));
            this._undoService = undoService ?? throw new ArgumentNullException(nameof(undoService));

            this.ExitCommand = ReactiveCommand.Create<Window>(this.Exit);
            this.RedoCommand = ReactiveCommand.Create(
                undoService.Redo,
                undoService.WhenAnyValue(x => x.CanRedo));

            this.SaveCommand = ReactiveCommand.Create(this.Save, this.WhenAnyValue(x => x.HasChanges));
            this.UndoCommand = ReactiveCommand.Create(
                undoService.Undo,
                undoService.WhenAnyValue(x => x.CanUndo));
            this.ViewSourceCommand = ReactiveCommand.Create(this.ViewSource);

            this._sceneService.PropertyChanged += this.HasChanges_OnPropertyChanged;
            this._projectService.PropertyChanged += this.HasChanges_OnPropertyChanged;
        }

        /// <summary>
        /// Gets the selection service.
        /// </summary>
        public IEntitySelectionService EntitySelectionService { get; }

        /// <summary>
        /// Gets the command to exit the application.
        /// </summary>
        public ICommand ExitCommand { get; }

        /// <summary>
        /// Gets a value indicating whether or not there are changes.
        /// </summary>
        public bool HasChanges => this._sceneService.HasChanges || this._projectService.HasChanges;

        /// <summary>
        /// Gets the command to redo a previously undone operation.
        /// </summary>
        public ICommand RedoCommand { get; }

        /// <summary>
        /// Gets the command to save the current scene.
        /// </summary>
        public ICommand SaveCommand { get; }

        /// <summary>
        /// Gets a value indicating whether or not the non-native menu should be shown. The native menu is for MacOS only.
        /// </summary>
        public bool ShowNonNativeMenu => AvaloniaLocator.Current.GetService<IRuntimePlatform>().GetRuntimeInfo().OperatingSystem != OperatingSystemType.OSX;

        /// <summary>
        /// Gets the command to undo the previous operation.
        /// </summary>
        public ICommand UndoCommand { get; }

        /// <summary>
        /// Gets the command to view the source code.
        /// </summary>
        public ICommand ViewSourceCommand { get; }

        /// <summary>
        /// Gets a value indicating whether or not the window should close.
        /// </summary>
        /// <returns>A value indicating whether or not the window should close.</returns>
        public async Task<YesNoCancelResult> TryClose() {
            var result = YesNoCancelResult.No;
            if (this._projectService.HasChanges || this._sceneService.HasChanges) {
                result = await this._dialogService.ShowYesNoDialog("Unsaved Changes", "Save changes before closing?", true);

                if (result == YesNoCancelResult.Yes) {
                    this._sceneService.SaveScene();
                    this._projectService.SaveProject();
                }
            }

            return result;
        }

        private void Exit(Window window) {
            window?.Close();
        }

        private void HasChanges_OnPropertyChanged(object sender, PropertyChangedEventArgs e) {
            if (e.PropertyName is nameof(this._sceneService.HasChanges) or nameof(this._projectService.HasChanges)) {
                this.RaisePropertyChanged(nameof(this.HasChanges));
            }
        }

        private void Save() {
            this._sceneService.SaveScene();
            this._projectService.SaveProject();
            this._undoService.Clear();
        }

        private void ViewSource() {
            WebHelper.OpenInBrowser("https://github.com/Macabresoft/Macabre2D");
        }
    }
}