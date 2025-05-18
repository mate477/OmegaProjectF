namespace SunGym.Controllers

open Microsoft.AspNetCore.Mvc
open SunGym.Models
open System.Collections.Generic
open System
open System.IO
open System.Text.Json

[<ApiController>]
[<Route("api/[controller]")>]
type UserController () =
    inherit ControllerBase()

    let userFilePath =
        Path.Combine(Directory.GetCurrentDirectory(), "Data", "user.json")

    let loadUsers () =
        if File.Exists(userFilePath) then
            let json = File.ReadAllText(userFilePath)
            JsonSerializer.Deserialize<List<User>>(json)
        else
            List<User>()

    let saveUsers (users: List<User>) =
        let json = JsonSerializer.Serialize(users, JsonSerializerOptions(WriteIndented = true))
        File.WriteAllText(userFilePath, json)

    [<HttpPost("register")>]
    member this.Register([<FromBody>] request: RegisterRequest) : ActionResult<ApiResponse<User>> =
        try
            let users = loadUsers()

            if users |> Seq.exists (fun u -> u.Email.ToLower() = request.Email.ToLower()) then
                let response =
                    { success = false
                      message = "Email is already registered."
                      data = None }
                ActionResult<ApiResponse<User>>(base.Conflict(response))
            else
                let newUser =
                    { Id = Guid.NewGuid().ToString()
                      Name = request.Name
                      Email = request.Email
                      Password = request.Password }

                users.Add(newUser)
                saveUsers users

                let response =
                    { success = true
                      message = "User registered successfully."
                      data = Some newUser }
                ActionResult<ApiResponse<User>>(base.Ok(response))
        with ex ->
            let error =
                { success = false
                  message = "Failed to register user."
                  data = None }
            ActionResult<ApiResponse<User>>(base.StatusCode(500, error))

    [<HttpPost("login")>]
    member this.Login([<FromBody>] request: LoginRequest) : ActionResult<ApiResponse<User>> =
        try
            let users = loadUsers()
            let userOpt =
                users |> Seq.tryFind (fun u -> u.Email = request.Email && u.Password = request.Password)

            match userOpt with
            | Some user ->
                let response =
                    { success = true
                      message = "Login successful."
                      data = Some user }
                ActionResult<ApiResponse<User>>(base.Ok(response))
            | None ->
                let response =
                    { success = false
                      message = "Invalid email or password."
                      data = None }
                ActionResult<ApiResponse<User>>(base.Unauthorized(response))
        with ex ->
            let error =
                { success = false
                  message = "Login failed."
                  data = None }
            ActionResult<ApiResponse<User>>(base.StatusCode(500, error))