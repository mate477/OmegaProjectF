namespace SunGym.Controllers

open Microsoft.AspNetCore.Mvc
open SunGym.Models
open System
open System.Collections.Generic
open System.IO
open System.Text.Json
open System.Linq

[<ApiController>]
[<Route("api/[controller]")>]
type AuthController () as this =
    inherit ControllerBase()

    let userFilePath =
        Path.Combine(Directory.GetCurrentDirectory(), "Data", "users.json")

    let loadUsers () =
        if File.Exists(userFilePath) then
            let json = File.ReadAllText(userFilePath)
            JsonSerializer.Deserialize<List<User>>(json)
        else
            List<User>()

    let saveUsers (users: List<User>) =
        let json = JsonSerializer.Serialize(users, JsonSerializerOptions(WriteIndented = true))
        File.WriteAllText(userFilePath, json)

    /// POST: /api/auth/register
    [<HttpPost("register")>]
    member _.Register([<FromBody>] request: RegisterRequest) : ActionResult<ApiResponse<User>> =
        let users = loadUsers()
        let exists = users |> Seq.exists (fun u -> u.Email.ToLower() = request.Email.ToLower())

        if exists then
            let response =
                { success = false
                  message = "Email already registered."
                  data = None }
            ActionResult<ApiResponse<User>>(this.Conflict(response))
        else
            let newUser =
                { Id = Guid.NewGuid().ToString()
                  Name = request.Name
                  Email = request.Email
                  Password = request.Password }

            users.Add(newUser)
            saveUsers(users)

            let response =
                { success = true
                  message = "User registered successfully."
                  data = Some newUser }

            ActionResult<ApiResponse<User>>(this.Ok(response))

    /// POST: /api/auth/login
    [<HttpPost("login")>]
    member _.Login([<FromBody>] request: LoginRequest) : ActionResult<ApiResponse<User>> =
        let users = loadUsers()
        let found =
            users
            |> Seq.tryFind (fun u ->
                u.Email.ToLower() = request.Email.ToLower()
                && u.Password = request.Password)

        match found with
        | Some user ->
            let response =
                { success = true
                  message = "Login successful."
                  data = Some user }
            ActionResult<ApiResponse<User>>(this.Ok(response))

        | None ->
            let response =
                { success = false
                  message = "Invalid email or password."
                  data = None }
            ActionResult<ApiResponse<User>>(this.Unauthorized(response))