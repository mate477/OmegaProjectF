namespace SunGym.Controllers

open Microsoft.AspNetCore.Mvc
open SunGym.Models
open System.Collections.Generic
open System.IO
open System.Text.Json

[<ApiController>]
[<Route("api/[controller]")>]
type PassController () =
    inherit ControllerBase()

    let loadPasses () =
        let path = Path.Combine(Directory.GetCurrentDirectory(), "Data", "passes.json")
        if File.Exists(path) then
            let json = File.ReadAllText(path)
            JsonSerializer.Deserialize<List<GymPass>>(json)
        else
            List<GymPass>()

    [<HttpGet>]
    member _.GetAll() : ActionResult<IEnumerable<GymPass>> =
        ActionResult<IEnumerable<GymPass>>(loadPasses())