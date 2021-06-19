﻿namespace Macabresoft.Macabre2D.UI.SceneEditor {
    using Macabresoft.Macabre2D.UI.Library.Mappers;
    using Macabresoft.Macabre2D.UI.Library.Services;
    using Macabresoft.Macabre2D.UI.SceneEditor.Mappers;
    using Macabresoft.Macabre2D.UI.SceneEditor.Services;
    using Unity;
    using Unity.Lifetime;

    /// <summary>
    /// Registers required types to the <see cref="IUnityContainer"/>.
    /// </summary>
    public static class Registrar {
        /// <summary>
        /// Registers the required types.
        /// </summary>
        /// <param name="container">The container.</param>
        /// <returns>The container.</returns>
        public static IUnityContainer RegisterMappers(this IUnityContainer container) {
            return container.RegisterType<IValueEditorTypeMapper, ValueEditorTypeMapper>(new SingletonLifetimeManager());
        }

        /// <summary>
        /// Registers services to the container.
        /// </summary>
        /// <param name="container">The container.</param>
        /// <returns>The container.</returns>
        public static IUnityContainer RegisterServices(this IUnityContainer container) {
            return container.RegisterType<IDialogService, DialogService>(new SingletonLifetimeManager());
        }
    }
}