namespace SunGym.Controllers

open Microsoft.AspNetCore.Mvc
open SunGym.Models
open System.Collections.Generic
open System.IO
open System.Text.Json
open System.Linq

[<ApiController>]
[<Route("api/[controller]")>]
type CategoryController () =
    inherit ControllerBase()

    // JSON betöltése
    let loadProducts () =
        let path = Path.Combine(Directory.GetCurrentDirectory(), "Data", "products.json")
        if File.Exists(path) then
            let json = File.ReadAllText(path)
            JsonSerializer.Deserialize<List<Product>>(json)
        else
            List<Product>()

    [<HttpGet>]
    member _.GetCategories() : ActionResult<IEnumerable<string>> =
        let products = loadProducts()
        let categories =
            products
            |> Seq.map (fun p -> p.Category)
            |> Seq.distinct
            |> Seq.toList
        ActionResult<IEnumerable<string>>(categories)