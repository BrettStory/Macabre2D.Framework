﻿namespace Macabresoft.MonoGame.Core {

    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    /// Represents a sprite that can be loaded as content.
    /// </summary>
    public sealed class Sprite : BaseIdentifiable, IAsset, IDisposable {
        private bool _disposedValue = false;
        private Point _location;
        private string _name = string.Empty;
        private Point _size;

        /// <summary>
        /// Initializes a new instance of the <see cref="Sprite" /> class.
        /// </summary>
        /// <param name="assetId">The asset identifier.</param>
        public Sprite(Guid assetId) {
            this.AssetId = assetId;
            this.Load();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Sprite" /> class.
        /// </summary>
        /// <param name="assetId">The identifier.</param>
        /// <param name="location">
        /// The location of this specific sprite on the <see cref="Texture2D" />.
        /// </param>
        /// <param name="size">The size of this specific sprite on the <see cref="Texture2D" />.</param>
        public Sprite(Guid assetId, Point location, Point size) {
            this.Id = Guid.NewGuid();
            this.AssetId = assetId;
            this.SetLocationAndSize(location, size);
            this.Load();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Sprite" /> class.
        /// </summary>
        /// <param name="texture">The texture.</param>
        /// <remarks>
        /// This constructor should be used for dynamic sprite creation and will not be properly
        /// saved to a scene as there will be no content path.
        /// </remarks>
        public Sprite(Texture2D texture) : this(texture, Point.Zero, new Point(texture.Width, texture.Height)) {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Sprite" /> class.
        /// </summary>
        /// <param name="texture">The texture.</param>
        /// <param name="location">The location.</param>
        /// <param name="size">The size.</param>
        public Sprite(Texture2D texture, Point location, Point size) {
            this.Texture = texture;
            this.SetLocationAndSize(location, size);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Sprite" /> class.
        /// </summary>
        private Sprite() {
        }

        /// <inheritdoc />
        [DataMember]
        public Guid AssetId { get; set; } = Guid.Empty;

        /// <summary>
        /// Gets the location.
        /// </summary>
        /// <value>The location.</value>
        [DataMember]
        public Point Location {
            get {
                return this._location;
            }

            set {
                this.Set(ref this._location, value);
            }
        }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>The name.</value>
        [DataMember]
        public string Name {
            get {
                return this._name;
            }

            set {
                this.Set(ref this._name, value);
            }
        }

        /// <summary>
        /// Gets the size.
        /// </summary>
        /// <value>The size.</value>
        [DataMember]
        public Point Size {
            get {
                return this._size;
            }

            set {
                this.Set(ref this._size, value);
            }
        }

        /// <summary>
        /// Gets the texture.
        /// </summary>
        /// <value>The texture.</value>
        public Texture2D? Texture { get; set; }

        /// <inheritdoc />
        public void Dispose() {
            this.Dispose(true);
        }

        /// <summary>
        /// Loads the content.
        /// </summary>
        public void Load() {
            if (this.AssetId != Guid.Empty) {
                if (AssetManager.Instance.TryLoad<Texture2D>(this.AssetId, out var texture) && texture != null) {
                    this.Texture = texture;

                    if (this.Size == Point.Zero) {
                        this.SetLocationAndSize(Point.Zero, new Point(this.Texture.Width, this.Texture.Height));
                    }
                }
                else {
                    this.SetErrorTexture(DefaultGame.Instance.SpriteBatch);
                }
            }
        }

        /// <summary>
        /// Sets the error texture.
        /// </summary>
        /// <param name="spriteBatch">The sprite batch.</param>
        public void SetErrorTexture(SpriteBatch? spriteBatch) {
            if (this.Size.X != 0 && this.Size.Y != 0 && spriteBatch != null) {
                var errorTexture = new Texture2D(spriteBatch.GraphicsDevice, this.Size.X, this.Size.Y, false, SurfaceFormat.Color);
                var pixels = new Color[this.Size.X * this.Size.Y];

                for (var i = 0; i < pixels.Length; i++) {
                    pixels[i] = GameSettings.Instance.ErrorSpritesColor;
                }

                errorTexture.SetData(pixels);
                this.Texture = errorTexture;
            }
        }

        private void Dispose(bool disposing) {
            if (!this._disposedValue) {
                if (disposing) {
                    this.Texture?.Dispose();
                }

                this._disposedValue = true;
            }
        }

        private void SetLocationAndSize(Point location, Point size) {
            this.Location = location;
            this.Size = size;
        }
    }
}