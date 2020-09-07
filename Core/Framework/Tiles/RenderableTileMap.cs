﻿namespace Macabresoft.MonoGame.Core {

    using System.ComponentModel;

    /// <summary>
    /// A base renderable tile map.
    /// </summary>
    public abstract class RenderableTileMap : TileableComponent, IGameRenderableComponent {
        private bool _isVisible = true;
        private int _renderOrder;

        /// <inheritdoc />
        public bool IsVisible {
            get {
                return this._isVisible && this.Entity.IsEnabled;
            }

            set {
                this.Set(ref this._isVisible, value, this.Entity.IsEnabled);
            }
        }

        /// <inheritdoc />
        public int RenderOrder {
            get {
                return this._renderOrder;
            }

            set {
                this.Set(ref this._renderOrder, value);
            }
        }

        /// <inheritdoc />
        public abstract void Render(FrameTime frameTime, BoundingArea viewBoundingArea);

        /// <inheritdoc />
        protected override void OnEntityPropertyChanged(PropertyChangedEventArgs e) {
            base.OnEntityPropertyChanged(e);

            if (e.PropertyName == nameof(IGameEntity.IsEnabled) && this._isVisible) {
                this.RaisePropertyChanged(nameof(this.IsVisible));
            }
        }
    }
}