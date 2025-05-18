namespace SunGym.Models

open System
open System.Text.Json.Serialization

type Category =
    {
        Id: string
        Name: string
    }

type Product =
    {
        [<JsonPropertyName("id")>]
        Id: string

        [<JsonPropertyName("name")>]
        Name: string

        [<JsonPropertyName("description")>]
        Description: string

        [<JsonPropertyName("price")>]
        Price: decimal

        [<JsonPropertyName("imageUrl")>]
        ImageUrl: string

        [<JsonPropertyName("category")>]
        Category: string

        [<JsonPropertyName("inStock")>]
        InStock: bool

        [<JsonPropertyName("discountedPrice")>] 
        DiscountedPrice: decimal option
    }

type CartItem =
    {
        ProductId: string
        mutable Quantity: int
    }

type OrderItem =
    {
        ProductId: string
        Quantity: int
    }

type OrderStatus =
    | Pending
    | Processing
    | Shipped
    | Delivered
    | Cancelled

type Order =
    {
        Id: string
        Items: OrderItem list
        Total: decimal
        CustomerName: string
        CustomerEmail: string
        DeliveryAddress: string
        PlacedAt: DateTime
        Status: OrderStatus 
    }

type CartSummary =
    {
        Items: CartItem list
        Total: decimal
    }

type ServerCart =
    {
        UserId: string
        Items: CartItem list
        SavedAt: DateTime
    }

type ApiResponse<'T> =
    {
        success: bool
        message: string
        data: 'T option
    }

type User =
    {
        Id: string
        Name: string
        Email: string
        Password: string
    }

type LoginRequest =
    {
        Email: string
        Password: string
    }

type RegisterRequest =
    {
        Name: string
        Email: string
        Password: string
    }

type GymPass =
    {
        [<JsonPropertyName("id")>]
        Id: string

        [<JsonPropertyName("name")>]
        Name: string

        [<JsonPropertyName("description")>]
        Description: string

        [<JsonPropertyName("price")>]
        Price: decimal

        [<JsonPropertyName("durationDays")>]
        DurationDays: int
    }