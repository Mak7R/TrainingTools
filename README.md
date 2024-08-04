# Training Tools

## Description

Training Tools is a web application designed to manage muscle groups, exercises, training plans, and user interactions. Users can log in or register an account, view and manage their profiles, add friends, and track exercise results. Training plans can be created and shared, with the possibility to compare results with friends. Admins have additional privileges to manage exercises, muscle groups, and user roles. The application also supports OAuth 2.0 for signing in with Google and provides an open API for integration with other applications.

## Features

- **User Management:** Register, log in, and manage user profiles.
- **Friends Management:** Add, invite, and manage friends.
- **Exercise Tracking:** Add and compare exercise results with friends.
- **Training Plans:** Create and share training plans.
- **Admin Controls:** Manage exercises, muscle groups, and user roles.
- **OAuth 2.0:** Sign in with Google.
- **Open API:** Integrate with other applications using the provided API.
- **API Documentation:** Available via Swagger at `/api-docs`.

## Technologies

- **.NET 8**
- **ASP.NET Core**
- **MVC**
- **Web API**
- **Entity Framework Core**
- **Microsoft SQL Server**
- **Identity Cookies (MVC)**
- **JWT Tokens (API)**
- **OAuth 2.0 (Google Sign-In)**
- **Serilog**
- **XUnit**
- **Docker**
- **EmailSender (SMTP)**
- **EPPlus**
- **AutoMapper**
- **Swagger**

## Installation

To set up the project, follow these steps:

1. **Prerequisites:**
    - SQL Server
    - .NET 8

2. **Build the Project:**
   ```bash
   dotnet build WebUI

3. **Configure appsettings.json**
    ``` json
   {
     "RootAdmin": {
       "Id": "00000000-0000-0000-0000-000000000001",
       "UserName": "Root-Admin",
       "Email": "root@admin.com",
       "Password": "1111"
     },
     "Logging": {
       "LogLevel": {
         "Default": "Information",
         "Microsoft.AspNetCore": "Warning"
       }
     },
     "AllowedHosts": "*",
     "AllowedOrigins": ["*"],
     "AllowedHeaders": ["*"],
     "AllowedMethods": ["*"],
     "EPPlus": {
       "ExcelPackage": {
         "LicenseContext": "NonCommercial"
       }
     },
     "Jwt": {
       "Issuer": null,
       "Audience": null,
       "EXPIRATION_MINUTES": "240",
       "SecretKey": "Public development key for api authentication (must be longer than 256 bits)"
     },
     "Emails":{
       "Confirmation":{
         "Email": "not@exists@and@invalid@email",
         "Password": "1234",
         "SmtpHost": "smtp.gmail.com",
         "SmtpPort": 587
       }
     },
     "OAuth2": {
       "Google": {
         "ClientId": "id",
         "ClientSecret": "secret"
       }
     },
     "ActiveConnection": "DefaultConnection",
     "ConnectionStrings": {
       "DefaultConnection": "Server=localhost, 7000;Database=training_tools_local_dev;User Id=sa;Password=DbPass20190502;Trust Server Certificate=True;",
       "DockerDatabase": "Server=training_tools_mssql;Database=training_tools_local_dev;User Id=sa;Password=DbPass20190502;Trust Server Certificate=True;"
     },
     "RazorRuntimeCompilation": false
   }

## Usage

To use the application, follow these steps:

1. **Start the Application:**
   ```bash
   dotnet run

2. Once the application is running, you can access the web interface and interact with the available features. The API endpoints will also be available for external applications.

## Endpoints (for API)

Here is a list of available API endpoints with a brief description for each:

### Account Endpoints
- **`DELETE /api/v1.0/Account`**  
  Delete the current account.

- **`GET /api/v1.0/Account/is-email-free`**  
  Check if an email is available for registration.

- **`GET /api/v1.0/Account/is-username-free`**  
  Check if a username is available for registration.

- **`POST /api/v1.0/Account/login`**  
  Log in to the account.

- **`GET /api/v1.0/Account/profile`**  
  Get the profile of the logged-in user.

- **`POST /api/v1.0/Account/profile`**  
  Update the profile of the logged-in user.

- **`POST /api/v1.0/Account/register`**  
  Register a new account.

### Exercises Endpoints
- **`GET /api/v1.0/Exercises`**  
  Get a list of exercises.

- **`POST /api/v1.0/Exercises`**  
  Create a new exercise.

- **`GET /api/v1.0/Exercises/count`**  
  Get the count of exercises.

- **`GET /api/v1.0/Exercises/render-about-preview`**  
  Get a preview of an exercise.

- **`GET /api/v1.0/Exercises/{id}`**  
  Get details of a specific exercise.

- **`PUT /api/v1.0/Exercises/{id}`**  
  Update a specific exercise.

- **`DELETE /api/v1.0/Exercises/{id}`**  
  Delete a specific exercise.

### Friends Endpoints
- **`GET /api/v1.0/Friends`**  
  Get a list of friends.

- **`GET /api/v1.0/Friends/invitations-for`**  
  Get friend invitations for the logged-in user.

- **`GET /api/v1.0/Friends/invitations-of`**  
  Get friend invitations sent by the logged-in user.

- **`PUT /api/v1.0/Friends/{id}/accept`**  
  Accept a friend invitation.

- **`DELETE /api/v1.0/Friends/{id}/cancel`**  
  Cancel a friend invitation.

- **`POST /api/v1.0/Friends/{id}/invite`**  
  Send a friend invitation.

- **`DELETE /api/v1.0/Friends/{id}/refuse`**  
  Refuse a friend invitation.

- **`DELETE /api/v1.0/Friends/{id}/remove`**  
  Remove a friend.

### Groups Endpoints
- **`GET /api/v1.0/Groups`**  
  Get a list of groups.

- **`POST /api/v1.0/Groups`**  
  Create a new group.

- **`GET /api/v1.0/Groups/count`**  
  Get the count of groups.

- **`GET /api/v1.0/Groups/{id}`**  
  Get details of a specific group.

- **`PUT /api/v1.0/Groups/{id}`**  
  Update a specific group.

- **`DELETE /api/v1.0/Groups/{id}`**  
  Delete a specific group.

### Users Endpoints
- **`GET /api/v1.0/Users`**  
  Get a list of users.

- **`GET /api/v1.0/Users/as-csv`**  
  Get a list of users in CSV format.

- **`GET /api/v1.0/Users/count`**  
  Get the count of users.

- **`GET /api/v1.0/Users/{id}`**  
  Get details of a specific user.

- **`PUT /api/v1.0/Users/{id}`**  
  Update a specific user.

- **`DELETE /api/v1.0/Users/{id}`**  
  Delete a specific user.

- **`GET /api/v1.0/account/confirm-email`**  
  Confirm the user's email.

### Exercise Results Endpoints
- **`GET /api/v1.0/exercises/results`**  
  Get a list of exercise results.

- **`GET /api/v1.0/exercises/results/as-exel`**  
  Get exercise results as an Excel file.

- **`GET /api/v1.0/exercises/results/for-exercise/{id}`**  
  Get results for a specific exercise.

- **`PUT /api/v1.0/exercises/results/{id}`**  
  Update exercise results.

- **`POST /api/v1.0/exercises/results`**  
  Add new exercise results.

- **`DELETE /api/v1.0/exercises/results/{id}`**  
  Delete specific exercise results.

### Training Plans Endpoints
- **`GET /api/v1.0/training-plans`**  
  Get a list of training plans.

- **`POST /api/v1.0/training-plans`**  
  Create a new training plan.

- **`GET /api/v1.0/training-plans/count`**  
  Get the count of training plans.

- **`GET /api/v1.0/training-plans/for-user`**  
  Get training plans for a specific user.

- **`GET /api/v1.0/training-plans/my/count`**  
  Get the count of the logged-in user's training plans.

- **`GET /api/v1.0/training-plans/{id}`**  
  Get details of a specific training plan.

- **`PUT /api/v1.0/training-plans/{id}`**  
  Update a specific training plan.

- **`DELETE /api/v1.0/training-plans/{id}`**  
  Delete a specific training plan.

For more information and details about the API, you can view the Swagger documentation at `/api-docs` after running the application.

## Testing

To run the tests for this project, use the following command:

```bash
dotnet test
```

Testing Frameworks: XUnit

## Contributing

We welcome contributions to the project! Here's how you can contribute:

1. **Fork the repository** to your own GitHub account.
2. **Create a new branch** for your changes. You can name it descriptively based on what you are working on.
3. **Make your changes** in the new branch. Ensure your changes are well-documented and tested.
4. **Commit your changes** with a clear and concise commit message.
5. **Push your changes** to your forked repository on GitHub.
6. **Submit a pull request** to the main repository. Provide a detailed description of your changes and why they should be merged.

Feel free to create issues or feature requests if you encounter any bugs or have suggestions for improvements. We appreciate all forms of feedback and contributions.

You can also develop a frontend application using technologies like Angular, React, or any other framework to interact with our open API. This is a great way to extend the functionality of the project or integrate it with other tools and systems.

## License

This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for details.

## Contact Information

For any questions, suggestions, or feedback, please use GitHub issues or contact us through the project's GitHub page. You can find the repository at [GitHub Repository](https://github.com/Mak7R/TrainingTools).

We look forward to your contributions and thank you for being a part of the Training Tools community!