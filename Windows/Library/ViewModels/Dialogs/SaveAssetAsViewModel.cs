﻿namespace Macabre2D.Engine.Windows.ViewModels.Dialogs {

    using Macabre2D.Engine.Windows.Models;
    using Macabre2D.Engine.Windows.Models.Validation;

    public sealed class SaveAssetAsViewModel : OKCancelDialogViewModel {
        private string _name;
        private Asset _selectedAsset;

        public SaveAssetAsViewModel(Project project) {
            this.Project = project;
        }

        [RequiredValidation(FieldName = "Name")]
        [FileNameValidation]
        public string Name {
            get {
                return this._name;
            }

            set {
                if (this.Set(ref this._name, value)) {
                    this._okCommand.RaiseCanExecuteChanged();
                }
            }
        }

        public Project Project { get; }

        [RequiredValidation]
        public Asset SelectedAsset {
            get {
                return this._selectedAsset;
            }

            set {
                this.Set(ref this._selectedAsset, value);
            }
        }
    }
}