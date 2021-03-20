﻿namespace Macabresoft.Macabre2D.Tests.Editor.Library.Services {
    using System;
    using System.IO;
    using FluentAssertions;
    using FluentAssertions.Execution;
    using Macabresoft.Macabre2D.Editor.Library.Services;
    using Macabresoft.Macabre2D.Framework;
    using NSubstitute;
    using NUnit.Framework;

    [TestFixture]
    public class ProjectServiceTests {
        [Test]
        [Category("Unit Tests")]
        public void CreateProject_ShouldCreateProject_WhenFileDoesNotExist() {
            var fileSystem = Substitute.For<IFileSystemService>();
            var sceneService = Substitute.For<ISceneService>();
            var serializer = Substitute.For<ISerializer>();
            var undoService = Substitute.For<IUndoService>();
            var projectService = new ProjectService(fileSystem, sceneService, serializer, undoService);
            var sceneAsset = new SceneAsset();
            sceneService.CreateNewScene(Arg.Any<string>(), Arg.Any<string>()).Returns(sceneAsset);
            
            var projectDirectoryPath = Path.Combine(Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), Guid.NewGuid().ToString());
            var projectFilePath = Path.Combine(projectDirectoryPath, GameProject.ProjectFileExtension);
            fileSystem.DoesFileExist(projectFilePath).Returns(false);
            serializer.When(x => x.Serialize(Arg.Any<GameProject>(), Arg.Any<string>()))
                .Do(x => {
                    if (x[1] is string path) {
                        fileSystem.DoesFileExist(path).Returns(true);
                        serializer.Deserialize<GameProject>(path).Returns(new GameProject());
                    }
                });
            
            
            var project = projectService.CreateProject(projectDirectoryPath);

            using (new AssertionScope()) {
                serializer.Received().Serialize(Arg.Any<GameProject>(), projectFilePath);
                project.Should().NotBeNull();
                projectService.CurrentProject.Should().NotBeNull();
                projectService.CurrentProject.Should().Be(project);
            }
        }

        [Test]
        [Category("Unit Tests")]
        public void CreateProject_ShouldThrow_WhenFileExists() {
            var fileSystem = Substitute.For<IFileSystemService>();
            var sceneService = Substitute.For<ISceneService>();
            var serializer = Substitute.For<ISerializer>();
            var undoService = Substitute.For<IUndoService>();
            var projectService = new ProjectService(fileSystem, sceneService, serializer, undoService);

            var projectDirectoryPath = Path.Combine(Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), Guid.NewGuid().ToString());
            var projectFilePath = Path.Combine(projectDirectoryPath, GameProject.ProjectFileExtension);
            fileSystem.DoesFileExist(projectFilePath).Returns(true);

            using (new AssertionScope()) {
                projectService.Invoking(x => x.CreateProject(projectDirectoryPath)).Should().Throw<NotSupportedException>();
                serializer.DidNotReceive().Serialize(Arg.Any<GameProject>(), projectFilePath);
                projectService.CurrentProject.Should().BeNull();
            }
        }

        [Test]
        [Category("Unit Tests")]
        public void LoadProject_ShouldLoad_WhenFileExists() {
            var fileSystem = Substitute.For<IFileSystemService>();
            var sceneService = Substitute.For<ISceneService>();
            var serializer = Substitute.For<ISerializer>();
            var undoService = Substitute.For<IUndoService>();
            var projectService = new ProjectService(fileSystem, sceneService, serializer, undoService);

            var projectDirectoryPath = Path.Combine(Guid.NewGuid().ToString(), Guid.NewGuid().ToString());
            var projectFilePath = Path.Combine(projectDirectoryPath, GameProject.ProjectFileExtension);
            fileSystem.DoesFileExist(projectFilePath).Returns(true);
            var project = new GameProject();
            serializer.Deserialize<GameProject>(projectFilePath).Returns(project);

            var loadedProject = projectService.LoadProject(projectFilePath);

            using (new AssertionScope()) {
                serializer.Received().Deserialize<GameProject>(projectFilePath);
                loadedProject.Should().Be(project);
                projectService.CurrentProject.Should().Be(project);
            }
        }

        [Test]
        [Category("Unit Tests")]
        public void LoadProject_ShouldNotLoad_WhenFileDoesNotExist() {
            var fileSystem = Substitute.For<IFileSystemService>();
            var sceneService = Substitute.For<ISceneService>();
            var serializer = Substitute.For<ISerializer>();
            var undoService = Substitute.For<IUndoService>();
            var projectService = new ProjectService(fileSystem, sceneService, serializer, undoService);
            
            var projectFilePath = Path.Combine(Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), GameProject.ProjectFileExtension);
            fileSystem.DoesFileExist(projectFilePath).Returns(false);


            using (new AssertionScope()) {
                projectService.Invoking(x => x.LoadProject(projectFilePath)).Should().Throw<NotSupportedException>();
                serializer.DidNotReceive().Deserialize<GameProject>(projectFilePath);
                projectService.CurrentProject.Should().BeNull();
            }
        }

        [Test]
        [Category("Unit Tests")]
        public void SaveProject_ShouldNotSave_WhenProjectDoesNotExist() {
            var fileSystem = Substitute.For<IFileSystemService>();
            var sceneService = Substitute.For<ISceneService>();
            var serializer = Substitute.For<ISerializer>();
            var undoService = Substitute.For<IUndoService>();
            var projectService = new ProjectService(fileSystem, sceneService, serializer, undoService);

            var projectFilePath = Path.Combine(Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), GameProject.ProjectFileExtension);
            projectService.SaveProject();

            using (new AssertionScope()) {
                serializer.DidNotReceive().Serialize(Arg.Any<IGameProject>(), projectFilePath);
            }
        }

        [Test]
        [Category("Unit Tests")]
        public void SaveProject_ShouldSave_WhenProjectExists() {
            var fileSystem = Substitute.For<IFileSystemService>();
            var sceneService = Substitute.For<ISceneService>();
            var serializer = Substitute.For<ISerializer>();
            var undoService = Substitute.For<IUndoService>();
            var projectService = new ProjectService(fileSystem, sceneService, serializer, undoService);

            var projectFilePath = Path.Combine(Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), GameProject.ProjectFileExtension);
            fileSystem.DoesFileExist(projectFilePath).Returns(true);
            var project = new GameProject();
            serializer.Deserialize<GameProject>(projectFilePath).Returns(project);
            projectService.LoadProject(projectFilePath);
            projectService.SaveProject();

            using (new AssertionScope()) {
                serializer.Received().Serialize(project, projectFilePath);
            }
        }
    }
}