﻿namespace Macabre2D.UI.Library.Views {

    using Macabre2D.UI.Library.Common;
    using Macabre2D.UI.Library.ViewModels;
    using System.Windows.Controls;

    public partial class ProjectView : UserControl {

        public ProjectView() {
            this.DataContext = ViewContainer.Resolve<ProjectViewModel>();
            this.InitializeComponent();
        }
    }
}