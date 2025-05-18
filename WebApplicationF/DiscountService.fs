namespace SunGym

module DiscountService =

    open SunGym.Models

    let discountTable : Map<string, decimal> =
        Map.ofList [
            ("prod6", 0.10m) 
            ("prod17", 0.20m) 
        ]

    let getDiscountRate (productId: string) : decimal =
        match discountTable.TryFind productId with
        | Some rate -> rate
        | None -> 0.0m

    let private getPrice (_: CartItem) : decimal =
        100m

    let private calculateItemTotal (item: CartItem) : decimal =
        let price = getPrice item
        let discount = getDiscountRate item.ProductId
        let discountedPrice = price * (1.0m - discount)
        discountedPrice * decimal item.Quantity

    // Kosár végösszeg kiszámítása
    let calculateTotal (items: CartItem list) : decimal =
        items |> List.sumBy calculateItemTotal