﻿using System.Collections.Generic;
using System.IO;
using DataTables.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace DataTables.AspNetCore
{
    /// <summary>
    ///     Represents a response for DataTables.
    /// </summary>
    public class DataTablesResponse : IDataTablesResponse
    {
        /// <summary>
        ///     For internal use only.
        ///     Creates a new response instance.
        /// </summary>
        /// <param name="draw">Draw count from request object.</param>
        /// <param name="errorMessage">Error message.</param>
        protected DataTablesResponse(int draw, string errorMessage)
            : this(draw, errorMessage, null)
        {
        }

        /// <summary>
        ///     For internal use only.
        ///     Creates a new response instance.
        /// </summary>
        /// <param name="draw">Draw count from request object.</param>
        /// <param name="errorMessage">Error message.</param>
        /// <param name="additionalParameters"></param>
        protected DataTablesResponse(int draw, string errorMessage, IDictionary<string, object> additionalParameters)
        {
            Draw = draw;
            Error = errorMessage;
            AdditionalParameters = additionalParameters;
        }

        /// <summary>
        ///     For internal use only.
        ///     Creates a new response instance.
        /// </summary>
        /// <param name="draw">Draw count from request object.</param>
        /// <param name="totalRecords">Total record count (total records available on database).</param>
        /// <param name="totalRecordsFiltered">Filtered record count (total records available after filtering).</param>
        /// <param name="data">Data object (collection).</param>
        protected DataTablesResponse(int draw, int totalRecords, int totalRecordsFiltered, object data)
            : this(draw, totalRecords, totalRecordsFiltered, data, null)
        {
        }

        /// <summary>
        ///     For internal use only.
        ///     Creates a new response instance.
        /// </summary>
        /// <param name="draw">Draw count from request object.</param>
        /// <param name="totalRecords">Total record count (total records available on database).</param>
        /// <param name="totalRecordsFiltered">Filtered record count (total records available after filtering).</param>
        /// <param name="additionalParameters">Aditional parameters for response.</param>
        /// <param name="data">Data object (collection).</param>
        protected DataTablesResponse(int draw, int totalRecords, int totalRecordsFiltered, object data, IDictionary<string, object> additionalParameters)
        {
            Draw = draw;
            TotalRecords = totalRecords;
            TotalRecordsFiltered = totalRecordsFiltered;
            Data = data;

            AdditionalParameters = additionalParameters;
        }

        /// <summary>
        ///     Gets draw count for validation and async ordering.
        /// </summary>
        public int Draw { get; protected set; }

        /// <summary>
        ///     Gets error message, if not successful.
        ///     Should only be available for DataTables 1.10 and above.
        /// </summary>
        public string Error { get; protected set; }

        /// <summary>
        ///     Gets total record count (total records available on database).
        /// </summary>
        public int TotalRecords { get; protected set; }

        /// <summary>
        ///     Gets filtered record count (total records available after filtering).
        /// </summary>
        public int TotalRecordsFiltered { get; protected set; }

        /// <summary>
        ///     Gets data object (collection).
        /// </summary>
        public object Data { get; protected set; }

        /// <summary>
        ///     Gets aditional parameters for response.
        /// </summary>
        public IDictionary<string, object> AdditionalParameters { get; protected set; }

        /// <summary>
        ///     Converts this object to a Json compatible response using global naming convention for parameters.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            using (var stringWriter = new StringWriter())
            using (var jsonWriter = new JsonTextWriter(stringWriter))
            {
                if (IsSuccessResponse())
                {
                    // Start json object.
                    jsonWriter.WriteStartObject();

                    // Draw
                    jsonWriter.WritePropertyName(Configuration.Options.ResponseNameConvention.Draw, true);
                    jsonWriter.WriteValue(Draw);

                    // TotalRecords
                    jsonWriter.WritePropertyName(Configuration.Options.ResponseNameConvention.TotalRecords, true);
                    jsonWriter.WriteValue(TotalRecords);

                    // TotalRecordsFiltered
                    jsonWriter.WritePropertyName(Configuration.Options.ResponseNameConvention.TotalRecordsFiltered, true);
                    jsonWriter.WriteValue(TotalRecordsFiltered);

                    // Data
                    jsonWriter.WritePropertyName(Configuration.Options.ResponseNameConvention.Data, true);
                    jsonWriter.WriteRawValue(SerializeData(Data));

                    // AdditionalParameters
                    if (Configuration.Options.IsResponseAdditionalParametersEnabled && (AdditionalParameters != null))
                        foreach (var keypair in AdditionalParameters)
                        {
                            jsonWriter.WritePropertyName(keypair.Key, true);
                            jsonWriter.WriteValue(keypair.Value);
                        }

                    // End json object
                    jsonWriter.WriteEndObject();
                }
                else
                {
                    // Start json object.
                    jsonWriter.WriteStartObject();

                    // Draw
                    jsonWriter.WritePropertyName(Configuration.Options.ResponseNameConvention.Draw, true);
                    jsonWriter.WriteValue(Draw);

                    // Error
                    jsonWriter.WritePropertyName(Configuration.Options.ResponseNameConvention.Error, true);
                    jsonWriter.WriteValue(Error);

                    // AdditionalParameters
                    if (Configuration.Options.IsResponseAdditionalParametersEnabled && (AdditionalParameters != null))
                        foreach (var keypair in AdditionalParameters)
                        {
                            jsonWriter.WritePropertyName(keypair.Key, true);
                            jsonWriter.WriteValue(keypair.Value);
                        }

                    // End json object
                    jsonWriter.WriteEndObject();
                }

                jsonWriter.Flush();

                return stringWriter.ToString();
            }
        }

        /// <summary>
        ///     For private use only.
        ///     Gets an indicator if this is a success response or an error response.
        /// </summary>
        /// <returns>True if it's a success response, False if it's an error response.</returns>
        private bool IsSuccessResponse()
        {
            return (Data != null) && string.IsNullOrWhiteSpace(Error);
        }

        /// <summary>
        ///     Transforms a data object into a json element using Json.Net library.
        ///     Can be overriten when needed.
        ///     Data will be serialized with camelCase convention by default, since it's a JavaScript standard.
        ///     This should not interfere with DataTables' CamelCase X HungarianNotation issue.
        /// </summary>
        /// <param name="data">Data object to be transformed to json.</param>
        /// <returns>A json representation of your data.</returns>
        public virtual string SerializeData(object data)
        {
            var settings = new JsonSerializerSettings
            {
                ContractResolver = new DefaultContractResolver
                {
                    NamingStrategy = new CamelCaseNamingStrategy(true, false)
                }
            };
            return JsonConvert.SerializeObject(data, settings);
        }

        /// <summary>
        ///     Creates a new response instance.
        /// </summary>
        /// <param name="request">The request object.</param>
        /// <param name="totalRecords">Total record count (total records available on database).</param>
        /// <param name="totalRecordsFiltered">Filtered record count (total records available after filtering).</param>
        /// <param name="data">Data object (collection).</param>
        /// <returns>The response object.</returns>
        public static DataTablesResponse Create(IDataTablesRequest request, int totalRecords, int totalRecordsFiltered, object data)
        {
            return Create(request, totalRecords, totalRecordsFiltered, data, null);
        }

        /// <summary>
        ///     Creates a new response instance.
        /// </summary>
        /// <param name="request">The request object.</param>
        /// <param name="totalRecords">Total record count (total records available on database).</param>
        /// <param name="totalRecordsFiltered">Filtered record count (total records available after filtering).</param>
        /// <param name="data">Data object (collection).</param>
        /// <param name="additionalParameters">Aditional parameters for response.</param>
        /// <returns>The response object.</returns>
        public static DataTablesResponse Create(IDataTablesRequest request, int totalRecords, int totalRecordsFiltered, object data,
            IDictionary<string, object> additionalParameters)
        {
            // When request is null, there should be no response (null response).
            if (request == null) return null;

            if (Configuration.Options.IsDrawValidationEnabled)
                if (request.Draw < 1) return null;

            return new DataTablesResponse(request.Draw, totalRecords, totalRecordsFiltered, data, additionalParameters);
        }

        /// <summary>
        ///     Creates a new response instance.
        /// </summary>
        /// <param name="request">The request object.</param>
        /// <param name="errorMessage">Error message.</param>
        /// <returns>The response object.</returns>
        public static DataTablesResponse Create(IDataTablesRequest request, string errorMessage)
        {
            return Create(request, errorMessage, null);
        }

        /// <summary>
        ///     Creates a new response instance.
        /// </summary>
        /// <param name="request">The request object.</param>
        /// <param name="errorMessage">Error message.</param>
        /// <param name="additionalParameters"></param>
        /// <returns>The response object.</returns>
        public static DataTablesResponse Create(IDataTablesRequest request, string errorMessage, IDictionary<string, object> additionalParameters)
        {
            // When request is null, there should be no response (null response).
            if (request == null) return null;

            if (Configuration.Options.IsDrawValidationEnabled)
                if (request.Draw < 1) return null;

            return new DataTablesResponse(request.Draw, errorMessage, additionalParameters);
        }
    }
}