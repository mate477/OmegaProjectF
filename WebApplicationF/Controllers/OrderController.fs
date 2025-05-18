namespace SunGym.Controllers

open Microsoft.AspNetCore.Mvc
open SunGym.Models
open System.Collections.Generic
open System
open System.IO
open System.Text.Json
open System.Linq

[<ApiController>]
[<Route("api/[controller]")>]
type OrderController () as this =
    inherit ControllerBase()

    let orderFilePath =
        Path.Combine(Directory.GetCurrentDirectory(), "Data", "orders.json")

    let loadOrders () =
        if File.Exists(orderFilePath) then
            let json = File.ReadAllText(orderFilePath)
            JsonSerializer.Deserialize<List<Order>>(json)
        else
            List<Order>()

    let saveOrders (orders: List<Order>) =
        let json = JsonSerializer.Serialize(orders, JsonSerializerOptions(WriteIndented = true))
        File.WriteAllText(orderFilePath, json)

    [<HttpPost>]
    member _.PlaceOrder([<FromBody>] newOrder: Order) : ActionResult<ApiResponse<Order>> =
        try
            let orders = loadOrders()
            let orderWithId =
                { newOrder with
                    Id = Guid.NewGuid().ToString()
                    PlacedAt = DateTime.UtcNow
                    Status = OrderStatus.Pending }

            orders.Add(orderWithId)
            saveOrders orders

            let response =
                { success = true
                  message = "Order placed successfully"
                  data = Some orderWithId }

            ActionResult<ApiResponse<Order>>(this.Ok(response))
        with ex ->
            let error =
                { success = false
                  message = "Failed to place order"
                  data = None }
            ActionResult<ApiResponse<Order>>(this.StatusCode(500, error))

    [<HttpGet("{id}")>]
    member _.GetOrderById(id: string) : ActionResult<ApiResponse<Order>> =
        let orders = loadOrders()
        match orders |> Seq.tryFind (fun o -> o.Id = id) with
        | Some ord ->
            let response =
                { success = true
                  message = "Order found"
                  data = Some ord }
            ActionResult<ApiResponse<Order>>(this.Ok(response))
        | None ->
            let notFound =
                { success = false
                  message = "Order not found"
                  data = None }
            ActionResult<ApiResponse<Order>>(this.NotFound(notFound))

    [<HttpGet>]
    member _.GetAllOrders() : ActionResult<ApiResponse<List<Order>>> =
        let orders = loadOrders()
        let response =
            { success = true
              message = sprintf "%d orders found" orders.Count
              data = Some orders }
        ActionResult<ApiResponse<List<Order>>>(this.Ok(response))

    [<HttpPatch("{id}/status")>]
    member _.UpdateOrderStatus(id: string, [<FromBody>] newStatus: OrderStatus) : ActionResult<ApiResponse<Order>> =
        let orders = loadOrders()
        match orders |> Seq.tryFind (fun o -> o.Id = id) with
        | Some order ->
            orders.Remove(order) |> ignore
            let updatedOrder = { order with Status = newStatus }
            orders.Add(updatedOrder)
            saveOrders orders

            let response =
                { success = true
                  message = "Order status updated"
                  data = Some updatedOrder }
            ActionResult<ApiResponse<Order>>(this.Ok(response))
        | None ->
            let error =
                { success = false
                  message = "Order not found"
                  data = None }
            ActionResult<ApiResponse<Order>>(this.NotFound(error))