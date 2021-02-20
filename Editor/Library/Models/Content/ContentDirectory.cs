﻿namespace Macabresoft.Macabre2D.Editor.Library.Models.Content {
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Macabresoft.Core;

    /// <summary>
    /// Interface for a directory content node.
    /// </summary>
    public interface IContentDirectory : IContentNode {
        /// <summary>
        /// Gets the children of this directory.
        /// </summary>
        IReadOnlyCollection<ContentNode> Children { get; }

        /// <summary>
        /// Adds the child node.
        /// </summary>
        /// <param name="node">The child node to add.</param>
        void AddChild(ContentNode node);

        /// <summary>
        /// Removes the child node.
        /// </summary>
        /// <param name="node">The child node to remove.</param>
        void RemoveChild(ContentNode node);

        /// <summary>
        /// Loads the child directories under this node.
        /// </summary>
        void LoadChildDirectories();
    }

    /// <summary>
    /// A directory content node.
    /// </summary>
    public class ContentDirectory : ContentNode, IContentDirectory {
        private readonly ObservableCollectionExtended<ContentNode> _children = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="ContentNode" /> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="parent">The parent content directory.</param>
        public ContentDirectory(string name, IContentDirectory parent) : base(name, parent) {
        }

        /// <inheritdoc />
        public IReadOnlyCollection<ContentNode> Children => this._children;

        /// <inheritdoc />
        public override string NameWithoutExtension => this.Name;

        /// <inheritdoc />
        public void AddChild(ContentNode node) {
            this._children.Add(node);
            node.Initialize(this);
        }

        /// <inheritdoc />
        public void RemoveChild(ContentNode node) {
            this._children.Remove(node);
        }

        /// <inheritdoc />
        public void LoadChildDirectories() {
            var currentDirectoryPath = this.GetFullPath();

            if (Directory.Exists(currentDirectoryPath)) {
                var directories = Directory.GetDirectories(currentDirectoryPath).Where(x => Path.GetDirectoryName(x)?.StartsWith('.') == false);

                foreach (var directory in directories) {
                    this.LoadDirectory(directory);
                }
            }
        }
        
        private void LoadDirectory(string path) {
            var node = new ContentDirectory(path, this);
            this.AddChild(node);
            node.LoadChildDirectories();
        }
    }
}