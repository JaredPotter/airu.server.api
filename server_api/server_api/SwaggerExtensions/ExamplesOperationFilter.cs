using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Swashbuckle.Swagger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http.Description;

namespace server_api.SwaggerExtensions
{
    public class ExamplesOperationFilter //: IOperationFilter
    {
        /*
        public void Apply(Operation operation, SchemaRegistry schemaRegistry, ApiDescription apiDescription)
        {
        //    var responseAttributes = apiDescription.GetControllerAndActionAttributes<server_api.SwaggerConfig.SwaggerResponseExamplesAttribute>();

        //    foreach (var attr in responseAttributes)
        //    {
        //        var schema = schemaRegistry.GetOrRegister(attr.ResponseType);

        //        var response = new Response();

        //        foreach (var item in operation.responses)
        //        {
        //            if (item.Value.schema.type == schema.type && item.Value.schema.@ref == schema.@ref)
        //            {
        //                response = item.Value;
        //                break;
        //            }
        //        }

        //        //var response = operation.responses.FirstOrDefault(x => x.Value.schema.type == schema.type && x.Value.schema.@ref == schema.@ref).Value;


        //        if (response != null)
        //        {
        //            var provider = (server_api.SwaggerConfig.IProvideExamples)Activator.CreateInstance(attr.ExamplesType);
        //            response.examples = FormatAsJson(provider);
        //        }
        //    }
        //}

        //private static object FormatAsJson(server_api.SwaggerConfig.IProvideExamples provider)
        //{
        //    var examples = new Dictionary<string, object>()
        //        {
        //            {
        //                "application/json", provider.GetExamples()
        //            }
        //        };

        //    return ConvertToCamelCase(examples);
        //}

        //private static object ConvertToCamelCase(Dictionary<string, object> examples)
        //{
        //    var jsonString = JsonConvert.SerializeObject(examples, new JsonSerializerSettings() { ContractResolver = new CamelCasePropertyNamesContractResolver() });
        //    return JsonConvert.DeserializeObject(jsonString);
        }
        */
    }
}