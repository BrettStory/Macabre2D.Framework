namespace Macabresoft.Macabre2D.UI.ProjectEditor.Controls {
    using Avalonia.Controls;
    using Avalonia.Markup.Xaml;

    public class DropDownButton : ContentControl {
        public DropDownButton() {
            this.InitializeComponent();
        }

        private void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);
        }
    }
}