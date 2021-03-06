namespace Macabresoft.Macabre2D.Framework {
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using System.Runtime.Serialization;
    using Macabresoft.Core;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;

    /// <summary>
    /// Represents a camera into the game world.
    /// </summary>
    [Display(Name = "Camera")]
    public sealed class Camera : Entity, ICamera {
        private readonly ResettableLazy<BoundingArea> _boundingArea;

        [DataMember]
        private readonly AssetReference<Shader> _shaderReference = new();

        private Layers _layersToRender = ~Layers.None;
        private int _renderOrder;
        private SamplerStateType _samplerStateType = SamplerStateType.PointClamp;
        private bool _snapToPixels;
        private float _viewHeight = 10f;

        /// <summary>
        /// Initializes a new instance of the <see cref="Camera" /> class.
        /// </summary>
        public Camera() : base() {
            this._boundingArea = new ResettableLazy<BoundingArea>(this.CreateBoundingArea);
        }

        /// <inheritdoc />
        public BoundingArea BoundingArea => this._boundingArea.Value;

        /// <summary>
        /// Gets the offset settings.
        /// </summary>
        /// <value>The offset settings.</value>
        [DataMember(Name = "Offset Settings")]
        public OffsetSettings OffsetSettings { get; } = new(Vector2.Zero, PixelOffsetType.Center);

        /// <inheritdoc />
        [DataMember(Name = "Layers to Render")]
        public Layers LayersToRender {
            get => this._layersToRender;

            set => this.Set(ref this._layersToRender, value);
        }

        /// <inheritdoc />
        public int RenderOrder {
            get => this._renderOrder;
            set => this.Set(ref this._renderOrder, value, true);
        }

        /// <inheritdoc />
        public SamplerState SamplerState { get; private set; } = SamplerState.PointClamp;

        /// <summary>
        /// Gets or sets the type of the sampler state.
        /// </summary>
        /// <value>The type of the sampler state.</value>
        [DataMember(Name = "Sampler State")]
        public SamplerStateType SamplerStateType {
            get => this._samplerStateType;

            set {
                this.Set(ref this._samplerStateType, value);
                this.SamplerState = this._samplerStateType.ToSamplerState();
                this.RaisePropertyChanged(nameof(this.SamplerState));
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this camera should snap to the pixel ratio
        /// defined in <see cref="IGameSettings" />.
        /// </summary>
        /// <value><c>true</c> if this should snap to pixels; otherwise, <c>false</c>.</value>
        [DataMember(Name = "Snap to Pixels")]
        public bool SnapToPixels {
            get => this._snapToPixels;

            set {
                if (this.Set(ref this._snapToPixels, value)) {
                    this.ResetBoundingArea();
                }
            }
        }

        /// <summary>
        /// Gets the height of the view.
        /// </summary>
        /// <value>The height of the view.</value>
        [DataMember(Name = "View Height")]
        public float ViewHeight {
            get => this._viewHeight;

            set {
                // This is kind of an arbitrary value, but imagine the chaos if we allowed the
                // camera to reach 0.
                if (value <= 0.1f) {
                    value = 0.1f;
                }

                if (this.Set(ref this._viewHeight, value)) {
                    this.ResetBoundingArea();
                }
            }
        }

        /// <inheritdoc />
        public Vector2 ConvertPointFromScreenSpaceToWorldSpace(Point point) {
            var result = Vector2.Zero;

            if (this.OffsetSettings.Size.Y != 0f) {
                var ratio = this.ViewHeight / this.OffsetSettings.Size.Y;
                var pointVector = point.ToVector2();
                var vectorPosition = new Vector2(pointVector.X + this.OffsetSettings.Offset.X, -pointVector.Y + this.OffsetSettings.Size.Y + this.OffsetSettings.Offset.Y) * ratio;
                result = this.GetWorldTransform(vectorPosition).Position;
            }

            return result;
        }

        /// <inheritdoc />
        public Matrix GetViewMatrix() {
            var settings = this.Scene.Game.Project.Settings;
            var pixelsPerUnit = settings.PixelsPerUnit;
            var zoom = 1f / settings.GetPixelAgnosticRatio(this.ViewHeight, (int)this.OffsetSettings.Size.Y);
            var worldTransform = this.Transform;

            return
                Matrix.CreateTranslation(new Vector3(-worldTransform.Position.ToPixelSnappedValue(this.Scene.Game.Project.Settings) * pixelsPerUnit, 0f)) *
                Matrix.CreateScale(zoom, -zoom, 0f) *
                Matrix.CreateTranslation(new Vector3(-this.OffsetSettings.Offset.X, this.OffsetSettings.Size.Y + this.OffsetSettings.Offset.Y, 0f));
        }

        /// <summary>
        /// Gets the width of the view.
        /// </summary>
        /// <returns>The width of the view.</returns>
        public float GetViewWidth() {
            var size = this.OffsetSettings.Size;
            var ratio = size.Y != 0 ? this.ViewHeight / this.OffsetSettings.Size.Y : 0f;
            return size.X * ratio;
        }

        /// <inheritdoc />
        public override void Initialize(IScene scene, IEntity parent) {
            base.Initialize(scene, parent);

            this.OffsetSettings.Initialize(this.CreateSize);
            this.ResetBoundingArea();
            this.SamplerState = this._samplerStateType.ToSamplerState();

            this.Scene.Game.ViewportSizeChanged += this.Game_ViewportSizeChanged;
            this.OffsetSettings.PropertyChanged += this.OffsetSettings_PropertyChanged;

            this.Scene.Assets.ResolveAsset<Shader, Effect>(this._shaderReference);
        }

        /// <inheritdoc />
        public void Render(FrameTime frameTime, SpriteBatch spriteBatch, IEnumerable<IRenderableEntity> entities) {
            spriteBatch.Begin(
                SpriteSortMode.Deferred,
                BlendState.AlphaBlend,
                this.SamplerState,
                null,
                RasterizerState.CullNone,
                this._shaderReference.Asset?.Content,
                this.GetViewMatrix());

            foreach (var entity in entities) {
                entity.Render(frameTime, this.BoundingArea);
            }

            spriteBatch.End();
        }

        /// <summary>
        /// Zooms to a world position.
        /// </summary>
        /// <param name="worldPosition">The world position.</param>
        /// <param name="zoomAmount">The zoom amount.</param>
        public void ZoomTo(Vector2 worldPosition, float zoomAmount) {
            var originalCameraPosition = this.Transform.Position;
            var originalDistanceFromCamera = worldPosition - originalCameraPosition;
            var originalViewHeight = this.ViewHeight;
            this.ViewHeight -= zoomAmount;
            var viewHeightRatio = this.ViewHeight / originalViewHeight;
            this.SetWorldPosition(worldPosition - originalDistanceFromCamera * viewHeightRatio);
        }

        /// <summary>
        /// Zooms to a screen position.
        /// </summary>
        /// <param name="screenPosition">The screen position.</param>
        /// <param name="zoomAmount">The zoom amount.</param>
        public void ZoomTo(Point screenPosition, float zoomAmount) {
            var worldPosition = this.ConvertPointFromScreenSpaceToWorldSpace(screenPosition);
            this.ZoomTo(worldPosition, zoomAmount);
        }

        /// <summary>
        /// Zooms to a boundable entity, fitting it into frame.
        /// </summary>
        /// <param name="boundable">The boundable.</param>
        public void ZoomTo(IBoundable boundable) {
            var area = boundable.BoundingArea;
            var difference = area.Maximum - area.Minimum;
            var origin = area.Minimum + difference * 0.5f;

            this.SetWorldPosition(origin);

            this.ViewHeight = difference.Y;
            var currentViewWidth = this.GetViewWidth();
            if (currentViewWidth < difference.X) {
                this.ViewHeight *= difference.X / currentViewWidth;
            }
        }

        /// <inheritdoc />
        protected override void OnPropertyChanged(object? sender, PropertyChangedEventArgs e) {
            base.OnPropertyChanged(sender, e);

            if (e.PropertyName == nameof(IEntity.Transform)) {
                this.ResetBoundingArea();
            }
        }

        private BoundingArea CreateBoundingArea() {
            var ratio = this.ViewHeight / this.OffsetSettings.Size.Y;
            var width = this.OffsetSettings.Size.X * ratio;
            var height = this.OffsetSettings.Size.Y * ratio;
            var offset = this.OffsetSettings.Offset * ratio;

            var points = new List<Vector2> {
                this.GetWorldTransform(offset).Position,
                this.GetWorldTransform(offset + new Vector2(width, 0f)).Position,
                this.GetWorldTransform(offset + new Vector2(width, height)).Position,
                this.GetWorldTransform(offset + new Vector2(0f, height)).Position
            };

            var minimumX = points.Min(x => x.X);
            var minimumY = points.Min(x => x.Y);
            var maximumX = points.Max(x => x.X);
            var maximumY = points.Max(x => x.Y);

            if (this.SnapToPixels) {
                minimumX = minimumX.ToPixelSnappedValue(this.Scene.Game.Project.Settings);
                minimumY = minimumY.ToPixelSnappedValue(this.Scene.Game.Project.Settings);
                maximumX = maximumX.ToPixelSnappedValue(this.Scene.Game.Project.Settings);
                maximumY = maximumY.ToPixelSnappedValue(this.Scene.Game.Project.Settings);
            }

            return new BoundingArea(new Vector2(minimumX, minimumY), new Vector2(maximumX, maximumY));
        }

        private Vector2 CreateSize() {
            return new(this.Scene.Game.ViewportSize.X, this.Scene.Game.ViewportSize.Y);
        }

        private void Game_ViewportSizeChanged(object? sender, Point e) {
            this.OffsetSettings.InvalidateSize();
            this.ResetBoundingArea();
        }

        private void OffsetSettings_PropertyChanged(object? sender, PropertyChangedEventArgs e) {
            if (e.PropertyName == nameof(this.OffsetSettings.Offset)) {
                this.ResetBoundingArea();
            }
        }

        private void ResetBoundingArea() {
            this._boundingArea.Reset();
        }
    }
}