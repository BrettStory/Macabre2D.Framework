namespace Macabresoft.Macabre2D.Framework {
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;
    using System.Runtime.Serialization;

    /// <summary>
    /// A body to be used by the physics engine.
    /// </summary>
    [Display(Name = "Simple Physics Body")]
    public class SimplePhysicsBody : PhysicsBody {
        private Collider _collider = new CircleCollider();

        /// <inheritdoc />
        public override BoundingArea BoundingArea => this.Collider.BoundingArea;

        /// <inheritdoc />
        public override bool HasCollider => true;

        /// <summary>
        /// Gets the colliders.
        /// </summary>
        /// <value>The colliders.</value>
        [DataMember(Order = 0)]
        [Category("Collider")]
        public Collider Collider {
            get => this._collider;

            set {
                if (this.Set(ref this._collider, value)) {
                    this._collider.Initialize(this);
                }
            }
        }

        /// <inheritdoc />
        public override IEnumerable<Collider> GetColliders() {
            return new[] { this.Collider };
        }

        public override void Initialize(IScene scene, IEntity parent) {
            base.Initialize(scene, parent);
            this._collider.Initialize(this);
        }

        protected override void OnPropertyChanged(object? sender, PropertyChangedEventArgs e) {
            base.OnPropertyChanged(sender, e);

            if (e.PropertyName == nameof(IEntity.Transform)) {
                this._collider.Reset();
            }
        }
    }
}