# OmegaProjectF

OmegaProject is an ASP.NET-based web application contained in the WebApplicationF.sln solution file.
The project aims to provide a modern, responsive web interface for managing various business processes.

# Try-live link:

https://omegaproject.azurewebsites.net/

# Screenshots:

[screenshot1.png]

[screenshot2.png]

# Functions

- User management (registration, login, permissions)
- Data management and display from database
- Responsive user interface
- Basic CRUD operations

# Structure:

WebApplicationF,
The core F# web project, responsible for the backend logic and API endpoints. It includes:

- Program.fs: Configures and starts the web application.
- Models.fs & DiscountService.fs: Define domain models and business logic.
- Multiple controllers under Controllers/, such as AuthController.fs, ProductController.fs, and OrderController.fs, implement RESTful endpoints for different modules like authentication, product management, and order processing.
- Sample JSON data under Data/ (e.g. products.json, user.json) used for mock operations or testing.

wwwroot,
This directory contains the static frontend assets:

- index.html: Main HTML page.
- css/style.css: Styling for the user interface.
- js/script.js: Client-side interactivity.
- images/: Product and UI-related illustrations used throughout the site.

# Technology

The application is designed with modularity and clarity in mind, enabling easy extension and maintenance.
It also supports OpenAPI (Swagger) via Swashbuckle, aiding in documentation and testing.
