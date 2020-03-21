﻿namespace Macabre2D.Engine.Windows.Models {

    using System.ComponentModel;

    public interface IHierarchical<TChildren, TParent> : INotifyPropertyChanged, IParent<TChildren> {
        TParent Parent { get; set; }
    }
}