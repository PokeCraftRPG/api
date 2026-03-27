using Microsoft.OpenApi;
using PokeGame.Middlewares;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace PokeGame.Extensions;

internal class AddHeaderParameters : IOperationFilter
{
  public void Apply(OpenApiOperation operation, OperationFilterContext context)
  {
    operation.Parameters?.Add(new OpenApiParameter
    {
      In = ParameterLocation.Header,
      Name = ResolveWorld.Header,
      Description = "Enter your world ID or key in the input below:"
    });
  }
}
