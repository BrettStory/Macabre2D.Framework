namespace Macabresoft.Macabre2D.UI.Common.Services {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Macabresoft.Macabre2D.UI.Common.Models;
    using ReactiveUI;

    /// <summary>
    /// Interface for a service which handles undo/redo operations.
    /// </summary>
    public interface IUndoService {
        /// <summary>
        /// Gets a value indicating whether or not a redo operation is possible.
        /// </summary>
        bool CanRedo { get; }

        /// <summary>
        /// Gets a value indicating whether or not an undo operation is possible.
        /// </summary>
        bool CanUndo { get; }

        /// <summary>
        /// Clears the queue of undo and redo operations.
        /// </summary>
        void Clear();

        /// <summary>
        /// Clears the queue of undo and redo operations in the specified scope.
        /// </summary>
        /// <param name="scope">The scope.</param>
        void Clear(UndoScope scope);

        /// <summary>
        /// Performs the specified action and makes it available to be undone. When undoing or redoing the action, the property
        /// changed action will also be called.
        /// </summary>
        /// <param name="action">The action that can be undone.</param>
        /// <param name="undoAction">The action that undoes the changes performed in <see cref="action" />.</param>
        /// <param name="scope">The scope of the undo operation.</param>
        /// <param name="propertyChangedAction">An action which will kick off an object's property changed notification.</param>
        void Do(Action action, Action undoAction, UndoScope scope, Action propertyChangedAction);

        /// <summary>
        /// Performs the specified action and makes it available to be undone.
        /// </summary>
        /// <param name="action">The action that can be undone.</param>
        /// <param name="undoAction">The action that undoes the changes performed in <see cref="action" />.</param>
        /// <param name="scope">The scope of the undo operation.</param>
        void Do(Action action, Action undoAction, UndoScope scope);

        /// <summary>
        /// Performs the most recently undone operation.
        /// </summary>
        void Redo();

        /// <summary>
        /// Undoes the most recently performed operation.
        /// </summary>
        void Undo();
    }

    /// <summary>
    /// A service which handles undo/redo operations.
    /// </summary>
    public class UndoService : ReactiveObject, IUndoService {
        private readonly object _lock = new();

        private readonly Stack<UndoCommand> _redoStack = new(50);
        private readonly Stack<UndoCommand> _undoStack = new(50);

        /// <inheritdoc />
        public bool CanRedo {
            get {
                lock (this._lock) {
                    return this._redoStack.Any();
                }
            }
        }

        /// <inheritdoc />
        public bool CanUndo {
            get {
                lock (this._lock) {
                    return this._undoStack.Any();
                }
            }
        }

        /// <inheritdoc />
        public void Clear() {
            lock (this._lock) {
                this._redoStack.Clear();
                this._undoStack.Clear();
                this.RaiseProperties();
            }
        }

        public void Clear(UndoScope scope) {
            lock (this._lock) {
                ClearStack(this._redoStack, scope);
                ClearStack(this._undoStack, scope);
                this.RaiseProperties();
            }
        }

        /// <inheritdoc />
        public void Do(Action action, Action undoAction, UndoScope scope) {
            this.Do(action, undoAction, scope, null);
        }

        /// <inheritdoc />
        public void Do(Action action, Action undoAction, UndoScope scope, Action propertyChangedAction) {
            var undoCommand = new UndoCommand(action, undoAction, scope, propertyChangedAction);
            lock (this._lock) {
                undoCommand.Do();
                this._undoStack.Push(undoCommand);
                this._redoStack.Clear();
                this.RaiseProperties();
            }
        }

        /// <inheritdoc />
        public void Redo() {
            lock (this._lock) {
                if (this.CanRedo) {
                    var command = this._redoStack.Pop();
                    command.Do();
                    this._undoStack.Push(command);
                    this.RaiseProperties();
                }
            }
        }

        /// <inheritdoc />
        public void Undo() {
            lock (this._lock) {
                if (this.CanUndo) {
                    var command = this._undoStack.Pop();
                    command.Undo();
                    this._redoStack.Push(command);
                    this.RaiseProperties();
                }
            }
        }

        private static void ClearStack(Stack<UndoCommand> stack, UndoScope scope) {
            var commands = new List<UndoCommand>();

            while (stack.TryPop(out var command)) {
                if (command.Scope != scope) {
                    commands.Add(command);
                }
            }

            commands.Reverse();

            foreach (var command in commands) {
                stack.Push(command);
            }
        }

        private void RaiseProperties() {
            this.RaisePropertyChanged(nameof(this.CanRedo));
            this.RaisePropertyChanged(nameof(this.CanUndo));
        }
    }
}