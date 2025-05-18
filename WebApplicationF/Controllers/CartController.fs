namespace SunGym.Controllers

open Microsoft.AspNetCore.Mvc
open SunGym.Models
open System.Collections.Generic
open System.IO
open System.Text.Json
open System

[<ApiController>]
[<Route("api/[controller]")>]
type CartController () as this =
    inherit ControllerBase()
    static let cart = ResizeArray<CartItem>()

    // Elérési útvonalak
    let dataDir = Path.Combine(Directory.GetCurrentDirectory(), "Data")
    let cartFilePath userId = Path.Combine(dataDir, $"cart_{userId}.json")
    let productFilePath = Path.Combine(dataDir, "products.json")

    // Termékek betöltése fájlból
    let loadProducts () =
        if File.Exists(productFilePath) then
            let json = File.ReadAllText(productFilePath)
            JsonSerializer.Deserialize<List<Product>>(json)
        else
            List<Product>()

    // Kosár mentése fájlba
    let saveCartToFile (userId: string) (items: List<CartItem>) =
        let cartData = { UserId = userId; Items = List.ofSeq items; SavedAt = DateTime.UtcNow }
        let json = JsonSerializer.Serialize(cartData, JsonSerializerOptions(WriteIndented = true))
        File.WriteAllText(cartFilePath userId, json)

    // Kosár betöltése fájlból
    let loadCartFromFile (userId: string) =
        let path = cartFilePath userId
        if File.Exists(path) then
            let json = File.ReadAllText(path)
            JsonSerializer.Deserialize<ServerCart>(json)
        else
            { UserId = userId; Items = []; SavedAt = DateTime.MinValue }

    [<HttpGet>]
    member _.GetCart() : ActionResult<IEnumerable<CartItem>> =
        ActionResult<IEnumerable<CartItem>>(cart)

    [<HttpGet("summary")>]
    member _.GetSummary() : ActionResult<CartSummary> =
        let products = loadProducts()
        let total =
            cart
            |> Seq.sumBy (fun item ->
                match products |> Seq.tryFind (fun p -> p.Id = item.ProductId) with
                | Some p -> decimal item.Quantity * p.Price
                | None -> 0.0m)
        let summary = { Items = List.ofSeq cart; Total = total }
        ActionResult<CartSummary>(summary)

    [<HttpPost("add")>]
    member _.AddToCart([<FromBody>] item: CartItem) : IActionResult =
        let products = loadProducts()
        match products |> Seq.tryFind (fun p -> p.Id = item.ProductId) with
        | None -> this.BadRequest("Product does not exist.") :> IActionResult
        | Some _ ->
            let existing = cart |> Seq.tryFind (fun i -> i.ProductId = item.ProductId)
            match existing with
            | Some i -> i.Quantity <- i.Quantity + item.Quantity
            | None -> cart.Add(item)
            this.Ok() :> IActionResult

    [<HttpPost("remove")>]
    member _.RemoveFromCart([<FromBody>] productId: string) : IActionResult =
        let toRemove = cart |> Seq.tryFind (fun i -> i.ProductId = productId)
        match toRemove with
        | Some item -> cart.Remove(item) |> ignore; this.Ok() :> IActionResult
        | None -> this.NotFound() :> IActionResult

    [<HttpPost("clear")>]
    member _.ClearCart() : IActionResult =
        cart.Clear()
        this.Ok() :> IActionResult

    [<HttpPost("save")>]
    member _.SaveCart([<FromQuery>] userId: string) : IActionResult =
        try
            saveCartToFile userId cart
            this.Ok("Cart saved.") :> IActionResult
        with ex ->
            this.StatusCode(500, "Failed to save cart.") :> IActionResult

    [<HttpGet("load/{userId}")>]
    member _.LoadCart(userId: string) : ActionResult<ServerCart> =
        try
            let cartData = loadCartFromFile userId
            ActionResult<ServerCart>(cartData)
        with ex ->
            ActionResult<ServerCart>(this.StatusCode(500, "Failed to load cart."))