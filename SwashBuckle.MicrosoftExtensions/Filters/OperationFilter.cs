﻿using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Controllers;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;
using SwashBuckle.MicrosoftExtensions.Attributes;
using SwashBuckle.MicrosoftExtensions.Extensions;

namespace SwashBuckle.MicrosoftExtensions.Filters
{
    public class OperationFilter : IOperationFilter
    {
        public void Apply(Operation operation, OperationFilterContext context)
        {
            if(operation is null || context is null)
                return;
            
            var metadataAttribute = context.ApiDescription.ActionAttributes().OfType<MetadataAttribute>().SingleOrDefault();

            operation.Extensions.AddRange(metadataAttribute.GetMetadataExtensions());
            
            ApplyPropertiesMetadata(operation.Parameters, context.ApiDescription.ActionDescriptor.Parameters);
        }

        private static void ApplyPropertiesMetadata
        (
            IEnumerable<IParameter> parameters,
            IList<ParameterDescriptor> parameterDescriptions
        )
        {
            if(parameters is null) 
                return;
            
            foreach (var operationParameter in parameters)
            {
                var parameterDescription =
                    parameterDescriptions.FirstOrDefault(x =>
                        x.Name == operationParameter.Name);
                switch (parameterDescription)
                {
                    case ControllerParameterDescriptor controllerParameterDescriptor:
                        AddMetadataProperties(operationParameter, controllerParameterDescriptor.ParameterInfo);
                        break;
                    case ControllerBoundPropertyDescriptor controllerBoundPropertyDescriptor:
                        AddMetadataProperties(operationParameter, controllerBoundPropertyDescriptor.PropertyInfo);
                        break;
                    default: continue;
                }
            }
        }

        private static void AddMetadataProperties(IParameter parameter, ICustomAttributeProvider attributeProvider)
        {
            var attribute = attributeProvider.GetCustomAttributes(typeof(MetadataAttribute), true).SingleOrDefault() as MetadataAttribute;
            var extensions = attribute.GetMetadataExtensions();
            parameter.Extensions.AddRange(extensions);
        }
    }
}