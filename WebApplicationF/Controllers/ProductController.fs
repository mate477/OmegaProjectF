namespace SunGym.Controllers

open Microsoft.AspNetCore.Mvc
open SunGym.Models
open SunGym
open SunGym.DiscountService
open System.Collections.Generic
open System.IO
open System.Text.Json
open System.Linq

[<ApiController>]
[<Route("api/[controller]")>]
type ProductController () =
    inherit ControllerBase()

    let loadProducts () =
        let path = Path.Combine(Directory.GetCurrentDirectory(), "Data", "products.json")
        if File.Exists(path) then
            let json = File.ReadAllText(path)
            JsonSerializer.Deserialize<List<Product>>(json)
        else
            List<Product>()

    let enrichProduct (product: Product) =
        let discountRate = getDiscountRate product.Id
        let discountedPrice =
            if discountRate > 0.0m then
                Some (System.Math.Round(product.Price * (1.0m - discountRate), 2))
            else
                None
        { product with DiscountedPrice = discountedPrice }

    [<HttpGet>]
    member this.GetAll() : ActionResult<IEnumerable<Product>> =
        let products = loadProducts() |> Seq.map enrichProduct
        ActionResult<IEnumerable<Product>>(this.Ok(products))

    [<HttpGet("{id}")>]
    member this.GetById(id: string) : ActionResult<Product> =
        let products = loadProducts()
        match products |> Seq.tryFind (fun p -> p.Id = id) with
        | Some product -> ActionResult<Product>(this.Ok(enrichProduct product))
        | None -> ActionResult<Product>(this.NotFound())

    [<HttpGet("by-category/{category}")>]
    member this.GetByCategory(category: string) : ActionResult<IEnumerable<Product>> =
        let products = loadProducts()
        let filtered = products |> Seq.filter (fun p -> p.Category.ToLower() = category.ToLower())
                                |> Seq.map enrichProduct
        ActionResult<IEnumerable<Product>>(this.Ok(filtered))

    [<HttpGet("in-stock")>]
    member this.GetInStock() : ActionResult<IEnumerable<Product>> =
        let products = loadProducts()
        let inStock = products |> Seq.filter (fun p -> p.InStock)
                               |> Seq.map enrichProduct
        ActionResult<IEnumerable<Product>>(this.Ok(inStock))

    [<HttpGet("search")>]
    member this.Search([<FromQuery>] query: string) : ActionResult<IEnumerable<Product>> =
        let products = loadProducts()
        let results = products |> Seq.filter (fun p -> p.Name.ToLower().Contains(query.ToLower()))
                               |> Seq.map enrichProduct
        ActionResult<IEnumerable<Product>>(this.Ok(results))