namespace WebApplicationF
#nowarn "20"
open System
open System.Collections.Generic
open System.IO
open System.Linq
open System.Threading.Tasks
open Microsoft.AspNetCore
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.AspNetCore.HttpsPolicy
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.Logging
open Microsoft.OpenApi.Models
open Swashbuckle.AspNetCore.Swagger
open Swashbuckle.AspNetCore.SwaggerGen
open Swashbuckle.AspNetCore.SwaggerUI

module Program =
    let exitCode = 0

    [<EntryPoint>]
    let main args =

        let builder = WebApplication.CreateBuilder(args)

        builder.Services.AddControllers()
        builder.Services.AddEndpointsApiExplorer()
        builder.Services.AddSwaggerGen(fun options -> options.SwaggerDoc("v1", OpenApiInfo(Title = "SunGym", Version = "v1")))
        let app = builder.Build()
        // Configure the HTTP request pipeline

        app.UseSwagger()
        app.UseSwaggerUI(fun options ->options.SwaggerEndpoint("/swagger/v1/swagger.json", "SunGym v1"))

        app.UseDefaultFiles()
        app.UseStaticFiles()

        app.UseAuthorization()
        app.MapControllers()

        app.Run()

        exitCode