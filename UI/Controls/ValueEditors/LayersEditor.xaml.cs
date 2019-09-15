﻿namespace Macabre2D.UI.Controls.ValueEditors {

    using Macabre2D.Framework;
    using Macabre2D.UI.Converters;
    using System;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;

    public partial class LayersEditor : NamedValueEditor<Layers> {

        public LayersEditor() {
            this.Loaded += this.LayersEditor_Loaded;
            this.InitializeComponent();
        }

        protected override void OnValueChanged(Layers newValue, Layers oldValue, DependencyObject d) {
            foreach (var item in this._layersItemsControl.Items.OfType<CheckBox>()) {
                // Since we have static bindings we need to update these incase GameSettings has changed.
                item.GetBindingExpression(CheckBox.ContentProperty).UpdateTarget();
            }

            base.OnValueChanged(newValue, oldValue, d);
        }

        private void LayersEditor_Loaded(object sender, RoutedEventArgs e) {
            var layers = Enum.GetValues(typeof(Layers)).Cast<Layers>().OrderBy(x => (ushort)x).ToList();
            layers.Remove(Layers.None);
            layers.Remove(Layers.All);

            foreach (var layer in layers) {
                var checkBox = new CheckBox();
                var contentBinding = new Binding(nameof(CheckBox.Content));
                contentBinding.Source = layer;
                contentBinding.Path = new PropertyPath(string.Empty);
                contentBinding.Converter = new LayerToNameConverter();
                checkBox.SetBinding(CheckBox.ContentProperty, contentBinding);
                var isCheckedBinding = new Binding(nameof(CheckBox.IsChecked));
                isCheckedBinding.Source = this;
                isCheckedBinding.Path = new PropertyPath(nameof(this.Value));
                isCheckedBinding.Converter = new LayersToBoolConverter();
                isCheckedBinding.ConverterParameter = layer;
                checkBox.SetBinding(CheckBox.IsCheckedProperty, isCheckedBinding);
                this._layersItemsControl.Items.Add(checkBox);
            }

            this.Loaded -= this.LayersEditor_Loaded;
        }
    }
}