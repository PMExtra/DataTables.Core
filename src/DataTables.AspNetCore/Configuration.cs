using System;
using System.Collections.Generic;
using DataTables.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.DependencyInjection;
// ReSharper disable once RedundantUsingDirective
// For .NET Core
using System.Reflection;

namespace DataTables.AspNetCore
{
    /// <summary>
    ///     Handles DataTables.AspNet registration and holds default (global) configuration options.
    /// </summary>
    public static class Configuration
    {
        /// <summary>
        ///     Static constructor.
        ///     Set's default configuration for DataTables.AspNet.
        /// </summary>
        static Configuration()
        {
            Options = new Options();
        }

        /// <summary>
        ///     Get's DataTables.AspNet runtime options for server-side processing.
        /// </summary>
        public static IOptions Options { get; private set; }

        /// <summary>
        ///     Provides DataTables.AspNet registration for AspNet5 projects.
        /// </summary>
        /// <param name="services">Service collection for dependency injection.</param>
        public static void RegisterDataTables(this IServiceCollection services)
        {
            services.RegisterDataTables(new Options());
        }

        /// <summary>
        ///     Provides DataTables.AspNet registration for AspNet5 projects.
        /// </summary>
        /// <param name="services">Service collection for dependency injection.</param>
        /// <param name="options">DataTables.AspNet options.</param>
        public static void RegisterDataTables(this IServiceCollection services, IOptions options)
        {
            services.RegisterDataTables(options, new ModelBinder());
        }

        /// <summary>
        ///     Provides DataTables.AspNet registration for AspNet5 projects.
        /// </summary>
        /// <param name="services">Service collection for dependency injection.</param>
        /// <param name="requestModelBinder">Request model binder to use when resolving 'IDataTablesRequest' models.</param>
        public static void RegisterDataTables(this IServiceCollection services, ModelBinder requestModelBinder)
        {
            services.RegisterDataTables(new Options(), requestModelBinder);
        }

        /// <summary>
        ///     Provides DataTables.AspNet registration for AspNet5 projects.
        /// </summary>
        /// <param name="services">Service collection for dependency injection.</param>
        /// <param name="parseRequestAdditionalParameters">
        ///     Function to evaluante and parse aditional parameters sent within the
        ///     request (user-defined parameters).
        /// </param>
        /// <param name="parseResponseAdditionalParameters">
        ///     Indicates whether response aditional parameters parsing is enabled or
        ///     not.
        /// </param>
        public static void RegisterDataTables(this IServiceCollection services, Func<ModelBindingContext, IDictionary<string, object>> parseRequestAdditionalParameters,
            bool parseResponseAdditionalParameters)
        {
            services.RegisterDataTables(new Options(), new ModelBinder(), parseRequestAdditionalParameters, parseResponseAdditionalParameters);
        }

        /// <summary>
        ///     Provides DataTables.AspNet registration for AspNet5 projects.
        /// </summary>
        /// <param name="services"></param>
        /// <param name="options">DataTables.AspNet options.</param>
        /// <param name="requestModelBinder">Model binder to use when resolving 'IDataTablesRequest' model.</param>
        public static void RegisterDataTables(this IServiceCollection services, IOptions options, ModelBinder requestModelBinder)
        {
            services.RegisterDataTables(options, requestModelBinder, null, false);
        }

        /// <summary>
        ///     Provides DataTables.AspNet registration for AspNet5 projects.
        /// </summary>
        /// <param name="services">Service collection for dependency injection.</param>
        /// <param name="options">DataTables.AspNet options.</param>
        /// <param name="requestModelBinder">Request model binder to use when resolving 'IDataTablesRequest' models.</param>
        /// <param name="parseRequestAdditionalParameters">
        ///     Function to evaluate and parse aditional parameters sent within the
        ///     request (user-defined parameters).
        /// </param>
        /// <param name="enableResponseAdditionalParameters">
        ///     Indicates whether response aditional parameters parsing is enabled or
        ///     not.
        /// </param>
        public static void RegisterDataTables(this IServiceCollection services, IOptions options, ModelBinder requestModelBinder,
            Func<ModelBindingContext, IDictionary<string, object>> parseRequestAdditionalParameters, bool enableResponseAdditionalParameters)
        {
            if (options == null) throw new ArgumentNullException(nameof(options), "Options for DataTables.Core cannot be null.");
            if (requestModelBinder == null) throw new ArgumentNullException(nameof(requestModelBinder), "Request model binder for DataTables.Core cannot be null.");

            Options = options;

            if (parseRequestAdditionalParameters != null)
            {
                Options.EnableRequestAdditionalParameters();
                requestModelBinder.ParseAdditionalParameters = parseRequestAdditionalParameters;
            }

            if (enableResponseAdditionalParameters)
                Options.EnableResponseAdditionalParameters();

            // ReSharper disable once InconsistentNaming
            services.Configure<MvcOptions>(_options =>
            {
                // Should be inserted into first position because there is a generic binder which could end up resolving/binding model incorrectly.
                _options.ModelBinderProviders.Insert(0, new ModelBinderProvider(requestModelBinder));
            });
        }

        internal class ModelBinderProvider : IModelBinderProvider
        {
            public ModelBinderProvider()
            {
            }

            public ModelBinderProvider(IModelBinder modelBinder)
            {
                ModelBinder = modelBinder;
            }

            public IModelBinder ModelBinder { get; private set; }

            public IModelBinder GetBinder(ModelBinderProviderContext context)
            {
                if (IsBindable(context.Metadata.ModelType))
                    return ModelBinder ?? (ModelBinder = new ModelBinder());
                return null;
            }

            private static bool IsBindable(Type type)
            {
                return type.IsAssignableFrom(typeof(IDataTablesRequest));
            }
        }
    }
}