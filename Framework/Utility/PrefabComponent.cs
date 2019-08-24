﻿namespace Macabre2D.Framework {

    using Microsoft.Xna.Framework;
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// A component that contains a <see cref="Prefab"/>.
    /// </summary>
    /// <seealso cref="BaseComponent"/>
    /// <seealso cref="IDrawableComponent"/>
    /// <seealso cref="IAssetComponent{Sprite}"/>
    public sealed class PrefabComponent : BaseComponent, IDrawableComponent, IAssetComponent<Prefab> {
        private Prefab _prefab;

        public PrefabComponent() {
        }

        /// <inheritdoc/>
        public BoundingArea BoundingArea {
            get {
                return this.Prefrab?.Component is IBoundable boundable ? boundable.BoundingArea : new BoundingArea();
            }
        }

        /// <summary>
        /// Gets or sets the prefrab.
        /// </summary>
        /// <value>The prefrab.</value>
        public Prefab Prefrab {
            get {
                return this._prefab;
            }

            set {
                if (this._prefab != value) {
                    this._prefab = value;
                    this.LoadContent();
                }
            }
        }

        /// <inheritdoc/>
        public void Draw(GameTime gameTime, BoundingArea viewBoundingArea) {
            var component = this.Prefrab?.Component;
            if (component is IDrawableComponent drawable) {
                try {
                    component.Parent = this;
                    drawable.Draw(gameTime, viewBoundingArea);
                }
                finally {
                    component.Parent = null;
                }
            }
        }

        /// <inheritdoc/>
        public IEnumerable<Guid> GetOwnedAssetIds() {
            return this.Prefrab != null ? new[] { this.Prefrab.Id } : new Guid[0];
        }

        /// <inheritdoc/>
        public bool HasAsset(Guid id) {
            return this.Prefrab?.Id == id;
        }

        /// <inheritdoc/>
        public override void LoadContent() {
            if (this.Scene.IsInitialized) {
                this.Prefrab?.Component?.LoadContent();
            }

            base.LoadContent();
        }

        /// <inheritdoc/>
        public void RefreshAsset(Prefab newInstance) {
            if (newInstance != null && this.Prefrab?.Id == newInstance.Id) {
                this.Prefrab = newInstance;
            }
        }

        /// <inheritdoc/>
        public bool RemoveAsset(Guid id) {
            var result = this.HasAsset(id);
            if (result) {
                this.Prefrab = null;
            }

            return result;
        }

        /// <inheritdoc/>
        public bool TryGetAsset(Guid id, out Prefab asset) {
            var result = this.Prefrab?.Id == id;
            asset = result ? this.Prefrab : null;
            return result;
        }

        /// <inheritdoc/>
        protected override void Initialize() {
            this.Prefrab?.Component?.Initialize(this.Scene);
        }
    }
}