# ASP.NET Robust JWT Authentication with BCrypt and SQLite

This project demonstrates a secure authentication system built with
**ASP.NET**, using **JWT (JSON Web Tokens)** for authorization,
**BCrypt** for password hashing, and **SQLite** as the database.

## Features

-   Secure user registration
-   Password hashing using BCrypt
-   JWT-based authentication
-   Protected endpoints
-   Lightweight SQLite database

------------------------------------------------------------------------

## API Endpoints

### `POST /register`

Registers a new user.

-   Stores the user in the SQLite database
-   Hashes the password using BCrypt before saving
-   Ensures secure password storage

------------------------------------------------------------------------

### `POST /login`

Authenticates a user.

-   Accepts username and password
-   Verifies the password against the stored BCrypt hash
-   Returns a JWT token if authentication is successful

------------------------------------------------------------------------

### `GET /vip`

Protected endpoint.

-   Requires a valid JWT token
-   Token must be included in the `Authorization` header as:

```{=html}
<!-- -->
```
    Authorization: Bearer <your_token>

-   Accessible only if the token was issued by the backend and is valid

------------------------------------------------------------------------

## Security Overview

-   Passwords are never stored in plain text.
-   JWT tokens are required to access protected routes.
-   Authentication and authorization are handled securely using industry
    best practices.

------------------------------------------------------------------------

## Tech Stack

-   ASP.NET
-   JWT Authentication
-   BCrypt
-   SQLite
