using Microsoft.OpenApi;
using PokeGame.Api.Constants;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace PokeGame.Api.Extensions;

internal class AddHeaderParameters : IOperationFilter
{
  public void Apply(OpenApiOperation operation, OperationFilterContext context)
  {
    operation.Parameters?.Add(new OpenApiParameter
    {
      In = ParameterLocation.Header,
      Name = Headers.World,
      Description = "Enter your world ID or key in the input below:"
    });
  }
}
