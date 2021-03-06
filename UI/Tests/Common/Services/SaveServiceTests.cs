﻿namespace Macabresoft.Macabre2D.Tests.UI.Common.Services {
    using System;
    using FluentAssertions;
    using FluentAssertions.Execution;
    using Macabresoft.Macabre2D.Framework;
    using Macabresoft.Macabre2D.UI.Common.Models.Content;
    using Macabresoft.Macabre2D.UI.Common.Services;
    using NSubstitute;
    using NUnit.Framework;

    [TestFixture]
    public class SaveServiceTests {
        [Test]
        [Category("Unit Tests")]
        public void HasChanges_IsTrue_WhenUndoServiceHasNewChange() {
            var undoService = new UndoService();
            var saveService = new SaveService(
                Substitute.For<IProjectService>(),
                Substitute.For<ISceneService>(),
                undoService);
            
            undoService.Do(() => { }, () => { });

            using (new AssertionScope()) {
                saveService.HasChanges.Should().BeTrue();
            }
        }
        
        [Test]
        [Category("Unit Tests")]
        public void HasChanges_IsFalse_AfterSaving() {
            var undoService = new UndoService();
            var saveService = new SaveService(
                Substitute.For<IProjectService>(),
                Substitute.For<ISceneService>(),
                undoService);
            
            undoService.Do(() => { }, () => { });
            saveService.Save();

            using (new AssertionScope()) {
                saveService.HasChanges.Should().BeFalse();
            }
        }
        
        [Test]
        [Category("Unit Tests")]
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        [TestCase(5)]
        [TestCase(8)]
        [TestCase(13)]
        [TestCase(50)]
        public void HasChanges_IsFalse_AfterUndoingChangesSinceSave(int numberOfChanges) {
            var undoService = new UndoService();
            var saveService = new SaveService(
                Substitute.For<IProjectService>(),
                Substitute.For<ISceneService>(),
                undoService);
            
            undoService.Do(() => { }, () => { });
            saveService.Save();

            for (var i = 0; i < numberOfChanges; i++) {
                undoService.Do(() => { }, () => { });
            }
            
            for (var i = 0; i < numberOfChanges; i++) {
                undoService.Undo();
            }

            using (new AssertionScope()) {
                saveService.HasChanges.Should().BeFalse();
            }
        }
        
        [Test]
        [Category("Unit Tests")]
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        [TestCase(5)]
        [TestCase(8)]
        [TestCase(13)]
        [TestCase(50)]
        public void HasChanges_IsFalse_AfterUndoingChanges(int numberOfChanges) {
            var undoService = new UndoService();
            var saveService = new SaveService(
                Substitute.For<IProjectService>(),
                Substitute.For<ISceneService>(),
                undoService);

            for (var i = 0; i < numberOfChanges; i++) {
                undoService.Do(() => { }, () => { });
            }
            
            for (var i = 0; i < numberOfChanges; i++) {
                undoService.Undo();
            }

            using (new AssertionScope()) {
                saveService.HasChanges.Should().BeFalse(); 
            }
        }
        
        [Test]
        [Category("Unit Tests")]
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        [TestCase(5)]
        [TestCase(8)]
        [TestCase(13)]
        [TestCase(50)]
        public void HasChanges_IsTrue_AfterChanges(int numberOfChanges) {
            var undoService = new UndoService();
            var saveService = new SaveService(
                Substitute.For<IProjectService>(),
                Substitute.For<ISceneService>(),
                undoService);

            for (var i = 0; i < numberOfChanges; i++) {
                undoService.Do(() => { }, () => { });
            }

            using (new AssertionScope()) {
                saveService.HasChanges.Should().BeTrue();
            }
        }
    }
}