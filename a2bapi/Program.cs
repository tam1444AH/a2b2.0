using System.Text;
using a2bapi.Data;
using a2bapi.Models;
using a2bapi.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;


var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllers();


var frontendUrl = builder.Configuration["FrontendUrl"] ?? "http://localhost";
builder.Services.AddCors(opt =>
{
    opt.AddPolicy("AllowFrontend", policy =>
        policy.WithOrigins(frontendUrl)
        .AllowAnyHeader()
        .AllowAnyMethod());
});


var connectionString = builder.Configuration.GetConnectionString("Default") ?? throw new Exception("Missing DB connection string.");
builder.Services.AddDbContext<AppDbContext>(opt => opt.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IFlightsService, FlightsService>();
builder.Services.AddScoped<IHotelsService, HotelsService>();
builder.Services.AddSingleton<IJwtService, JwtService>();
builder.Services.AddSingleton<IPasswordHasher<User>, PasswordHasher<User>>();

builder.Services.Configure<AviationStackSettings>(builder.Configuration.GetSection("AviationStack"));
builder.Services.AddHttpClient<IAviationStackService, AviationStackService>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["AviationStack:BaseUrl"] ?? "http://api.aviationstack.com/v1/");
});

builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));

var jwt = builder.Configuration.GetSection("Jwt").Get<JwtSettings>() ?? throw new Exception("Missing Jwt config.");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidIssuer = jwt.Issuer,
            ValidAudience = jwt.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key)),
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapGet("/", () => "Welcome to the A2B API!");

app.Run();





//using MySql.Data.MySqlClient;
//using DotNetEnv;
//using a2bapi.Models;
//using Microsoft.AspNetCore.Diagnostics;
//using System.Text.Json;
//using System.Security.Cryptography;
//using System.Text;
//using Microsoft.AspNetCore.Authentication.JwtBearer;
//using Microsoft.IdentityModel.Tokens;
//using System.Security.Claims;
//using System.IdentityModel.Tokens.Jwt;
//using Microsoft.AspNetCore.Authorization;
//using SendGrid;
//using SendGrid.Helpers.Mail;

//var builder = WebApplication.CreateBuilder(args);
//var config = builder.Configuration;
//Env.Load();

////Environment.GetEnvironmentVariable("FRONTEND_URL");

//string frontendUrl = "http://localhost:80/"
//    ?? throw new Exception("FRONTEND_URL not set in environment variables.");

////Environment.GetEnvironmentVariable("BACKEND_URL");

//string backendUrl = "http://localhost:5030"
//    ?? throw new Exception("BACKEND_URL not set in environment variables.");

//var backendUri = new Uri(backendUrl);
//var backendHost = backendUri.Host;
//var backendPort = backendUri.Port;
//string JwtSecretKey = "w2GCyHeP0jS3AB9BP78hwtoGaRhJfp0MQGgGnAC2rDf";


//builder.Services.AddAuthentication( x => {
//    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
//    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
//    x.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
//}).AddJwtBearer(x => 
//{
//    x.TokenValidationParameters = new TokenValidationParameters
//    {
//        ValidIssuer = backendUrl,
//        ValidAudience = frontendUrl,
//        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
//            //Environment.GetEnvironmentVariable("JWT_SECRET") 
//            JwtSecretKey
//            ?? throw new Exception("JWT_SECRET not set in environment variables.")
//        )),
//        ValidateIssuer = true,
//        ValidateAudience = true,
//        ValidateLifetime = true,
//        ValidateIssuerSigningKey = true
//    };
//});

//builder.Services.AddAuthorization();

//string DbServer = "localhost";
//string DbPort = "3306";
//string DbName = "a2b_db";
//string DbUser = "root";
//string DbPassword = "Talha2003.";


//string dbConnectionString = $"Server={Environment.GetEnvironmentVariable("DB_SERVER")};" +
//                            $"Port={Environment.GetEnvironmentVariable("DB_PORT")};" +
//                            $"Database={Environment.GetEnvironmentVariable("DB_NAME")};" +
//                            $"User={Environment.GetEnvironmentVariable("DB_USER")};" +
//                            $"Password={Environment.GetEnvironmentVariable("DB_PASSWORD")}";


//builder.Services.AddScoped(_ => new MySqlConnection(dbConnectionString));
//builder.Services.AddHttpClient();
//builder.Logging.AddConsole();
//builder.Logging.AddDebug();

//var corsPolicyName = "AllowSpecificOrigins";

//builder.Services.AddCors(options =>
//{
//    options.AddPolicy(corsPolicyName, policy =>
//    {
//        policy.WithOrigins(frontendUrl)
//            .AllowAnyHeader()
//            .AllowAnyMethod();
//    });

//});

//var app = builder.Build();

//// app.Urls.Add($"http://{backendHost}:{backendPort}");
////app.Urls.Add($"http://0.0.0.0:{backendPort}");
//app.UseCors(corsPolicyName);
//app.UseExceptionHandler("/error");
//app.UseAuthentication();
//app.UseAuthorization();

//app.Map("api/error", (HttpContext context) =>
//{
//    var exception = context.Features.Get<IExceptionHandlerFeature>()?.Error;
//    return Results.Problem(exception?.Message ?? "An unknown error occured");
//});

//app.MapGet("api/flights/{from}-{to}", async (string from, string to, IHttpClientFactory httpClientFactory) =>
//{
//var apiKey = Environment.GetEnvironmentVariable("API_ACCESS_KEY");

//if (string.IsNullOrEmpty(apiKey))
//{
//    return Results.Problem("API access key is not set.");
//}

//var client = httpClientFactory.CreateClient();
//var url = "http://api.aviationstack.com/v1/flights";

//var query = new Dictionary<string, string>
//    {
//        { "access_key", apiKey },
//        { "dep_iata", from.Trim().ToUpper() },
//        { "arr_iata", to.Trim().ToUpper() }
//    };

//try
//{
//    var response = await client.GetAsync($"{url}?{string.Join("&", query.Select(kvp => $"{kvp.Key}={kvp.Value}"))}");

//    if (!response.IsSuccessStatusCode)
//    {
//        var error = await response.Content.ReadAsStringAsync();
//        return Results.Problem($"Error fetching flights: {error}");
//    }

//    var responseJson = await response.Content.ReadFromJsonAsync<FlightsResponse>();

//    if (responseJson?.Data == null || responseJson.Data.Count == 0)
//    {
//        return Results.Ok(new { Message = "No flights found for the specified route." });
//    }

//    return Results.Ok(responseJson.Data);
//}
//catch (Exception ex)
//{
//    return Results.Problem($"Error fetching flights: {ex.Message}");
//}

//});

//// Console.WriteLine("Testing email-sending functionality...");

//// var sendGridApiKey = Environment.GetEnvironmentVariable("SENDGRID_API_KEY");

//// if (string.IsNullOrEmpty(sendGridApiKey))
//// {
////     Console.WriteLine("SendGrid API Key is missing. Check your .env file.");
//// }
//// else
//// {
////     var client = new SendGridClient(sendGridApiKey);
////     var from = new EmailAddress("thea2bteam2024@gmail.com", "Example User");
////     var subject = "Test Email from SendGrid";
////     var to = new EmailAddress("mohammedtalha290@gmail.com", "Recipient Name"); // Replace with a valid email
////     var plainTextContent = "This is a plain text test email.";
////     var htmlContent = "<strong>This is an HTML test email.</strong>";
////     var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);

////     try
////     {
////         var response = await client.SendEmailAsync(msg);
////         Console.WriteLine($"Email sent successfully. Status Code: {response.StatusCode}");
////     }
////     catch (Exception ex)
////     {
////         Console.WriteLine($"Failed to send email: {ex.Message}");
////     }
//// }

//app.MapGet("api/test-db", async (MySqlConnection dbConnection) =>
//{
//    try
//    {
//        await dbConnection.OpenAsync();
//        return Results.Ok("Database connection successful!");
//    }
//    catch (Exception ex)
//    {
//        return Results.Problem($"Database connection failed: {ex.Message}");
//    }
//});

//static async Task<string> GetAccessTokenAsync(IHttpClientFactory httpClientFactory)
//{
//    var apiKey = Environment.GetEnvironmentVariable("AMADEUS_API_KEY");
//    var apiSecret = Environment.GetEnvironmentVariable("AMADEUS_API_SECRET");

//    if (string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(apiSecret))
//    {
//        throw new Exception("Amadeus API credentials are missing.");
//    }

//    var client = httpClientFactory.CreateClient();
//    var tokenUrl = "https://test.api.amadeus.com/v1/security/oauth2/token";
//    var authHeader = Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes($"{apiKey}:{apiSecret}"));

//    var content = new FormUrlEncodedContent(new Dictionary<string, string>
//    {
//        { "grant_type", "client_credentials" }
//    });

//    client.DefaultRequestHeaders.Add("Authorization", $"Basic {authHeader}");

//    var response = await client.PostAsync(tokenUrl, content);

//    if (!response.IsSuccessStatusCode)
//    {
//        var error = await response.Content.ReadAsStringAsync();
//        throw new Exception($"Failed to get access token: {error}");
//    }

//    var jsonResponse = await response.Content.ReadAsStringAsync();
//    // Console.WriteLine($"Token Response: {jsonResponse}");

//    var tokenResponse = JsonSerializer.Deserialize<AccessTokenResponse>(jsonResponse);

//    return tokenResponse?.AccessToken ?? throw new Exception("Access token not found in response.");
//}

//static string GenerateJwt(string email) 
//{
//    var secretKey = Environment.GetEnvironmentVariable("JWT_SECRET") ?? "YourDevSecretKey";
//    var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
//    var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);
//    string frontendUrl = Environment.GetEnvironmentVariable("FRONTEND_URL") ?? "http://localhost:5173";
//    string backendUrl = Environment.GetEnvironmentVariable("BACKEND_URL") ?? "http://localhost:5030";


//    var claims = new[] {
//        new Claim(JwtRegisteredClaimNames.Sub, email),
//        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
//        new Claim(ClaimTypes.Email, email)
//    };

//    var token = new JwtSecurityToken(
//        issuer: backendUrl,
//        audience: frontendUrl,
//        claims: claims,
//        expires: DateTime.UtcNow.AddHours(1),
//        signingCredentials: credentials
//    );

//    return new JwtSecurityTokenHandler().WriteToken(token);

//}


//app.MapGet("api/hotels/{to}-{dist}-{stars}", [Authorize] async (string to, string dist, string stars, IHttpClientFactory httpClientFactory) =>
//{

//    try {

//        to = to.ToUpper();

//        // Console.WriteLine("Starting GetAccessTokenAsync...");
//        var accessToken = await GetAccessTokenAsync(httpClientFactory);
//        // Console.WriteLine($"Retrieved Access Token: {accessToken}");

//        var client = httpClientFactory.CreateClient();
//        var hotelApiUrl = "https://test.api.amadeus.com/v1/reference-data/locations/hotels/by-city";

//        var queryParameters = new Dictionary<string, string>
//        {
//            { "cityCode", to },
//            { "radius", dist },
//            { "radiusUnit", "MILE" },
//            { "ratings", stars },
//            { "hotelSource", "ALL" }
//        };

//        var queryString = string.Join("&", queryParameters.Select(kvp => $"{kvp.Key}={Uri.EscapeDataString(kvp.Value)}"));
//        var requestUrl = $"{hotelApiUrl}?{queryString}";
//        // Console.WriteLine($"Request URL: {requestUrl}");

//        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");
//        var response = await client.GetAsync(requestUrl);

//        if (!response.IsSuccessStatusCode) 
//        {

//            var error = await response.Content.ReadAsStringAsync();
//            Console.WriteLine($"Error Response: {error}");
//            return Results.Problem($"Error fetching hotels: {error}");

//        }

//        var jsonString = await response.Content.ReadAsStringAsync();
//        // Console.WriteLine($"Hotel API Response: {jsonString}");

//        var hotelResponse = JsonSerializer.Deserialize<a2bapi.Models.Hotels.HotelApiResponse>(jsonString);


//        if (hotelResponse?.Data == null)
//        {
//            return Results.Problem("No hotels found.");
//        }

//        return Results.Ok(hotelResponse.Data);

//    }
//    catch (Exception ex) {

//        Console.WriteLine($"Error: {ex.Message}");
//        return Results.Problem($"Error: {ex.Message}");

//    }
//});


//app.MapPost("api/signup", async (HttpContext context, MySqlConnection dbConnection) => 
//{

//    try 
//    {
//        var body = await context.Request.ReadFromJsonAsync<User>();
//        if (body == null || string.IsNullOrWhiteSpace(body.Email) || string.IsNullOrWhiteSpace(body.Password)) 
//        {
//            return Results.BadRequest("Email and password are required.");
//        }

//        var checkUserCmd = new MySqlCommand("SELECT id FROM users_net WHERE email = @Email", dbConnection);
//        checkUserCmd.Parameters.AddWithValue("@Email", body.Email);
//        await dbConnection.OpenAsync();
//        var existingUser = await checkUserCmd.ExecuteScalarAsync();

//        if (existingUser != null) 
//        {
//            await dbConnection.CloseAsync();
//            return Results.Conflict("Email already in use.");
//        }

//        string hashedPassword = HashPassword(body.Password);

//        var insertUserCmd = new MySqlCommand("INSERT INTO users_net (email, password) VALUES (@Email, @Password)", dbConnection);
//        insertUserCmd.Parameters.AddWithValue("@Email", body.Email);
//        insertUserCmd.Parameters.AddWithValue("@Password", hashedPassword);

//        await insertUserCmd.ExecuteNonQueryAsync();
//        await dbConnection.CloseAsync();

//        var jwt = GenerateJwt(body.Email);

//        return Results.Ok(new { message = "User registered successfully.", token = jwt });

//    }
//    catch (Exception ex) 
//    {

//        Console.WriteLine($"Error: {ex.Message}");
//        return Results.Problem("An error occurred during user registration.");

//    }

//});

//app.MapPost("api/login", async (HttpContext context, MySqlConnection dbConnection) =>
//{
//    try 
//    {
//        var body = await context.Request.ReadFromJsonAsync<User>();
//        if (body == null || string.IsNullOrEmpty(body.Email) || string.IsNullOrEmpty(body.Password))
//        {
//            return Results.BadRequest("Email and password are required.");
//        }

//        var checkUserCmd = new MySqlCommand("SELECT password FROM users_net WHERE email = @Email", dbConnection);
//        checkUserCmd.Parameters.AddWithValue("@Email", body.Email);
//        await dbConnection.OpenAsync();
//        var result = await checkUserCmd.ExecuteScalarAsync();
//        var hashedPassword = result?.ToString();
//        await dbConnection.CloseAsync();

//        if (hashedPassword == null || !VerifyPassword(body.Password, hashedPassword)) 
//        {
//            return Results.Unauthorized();
//        }

//        var jwt = GenerateJwt(body.Email);

//        return Results.Ok(new { message = "Login successful.", token = jwt});
//    }
//    catch (Exception ex) 
//    {
//        Console.WriteLine($"Error: {ex.Message}");
//        return Results.Problem("An error occurred during login.");
//    }

//});

//static bool VerifyPassword(string password, string hashedPassword) 
//{
//    var hashedBytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
//    var computedHash = Convert.ToBase64String(hashedBytes);
//    return computedHash == hashedPassword;
//}

//static string HashPassword(string password) 
//{
//    var hashedBytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
//    return Convert.ToBase64String(hashedBytes);
//}

//app.MapPost("api/save-flight", [Authorize] async (HttpContext context, MySqlConnection dbConnection) =>
//{
//    try
//    {
//        var userEmail = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
//        if (string.IsNullOrEmpty(userEmail))
//        {
//            return Results.Unauthorized();
//        }

//        var body = await context.Request.ReadFromJsonAsync<JsonElement>();
//        if (!body.TryGetProperty("flightName", out var flightNameElement) ||
//            !body.TryGetProperty("departureTime", out var departureTimeElement) ||
//            !body.TryGetProperty("arrivalTime", out var arrivalTimeElement) ||
//            !body.TryGetProperty("flightDate", out var flightDateElement) ||
//            !body.TryGetProperty("departureIata", out var departureIataElement) ||
//            !body.TryGetProperty("arrivalIata", out var arrivalIataElement) ||
//            !body.TryGetProperty("flightPrice", out var flightPriceElement))
//        {
//            return Results.BadRequest("Invalid flight data.");
//        }

//        string flightName = flightNameElement.GetString() ?? string.Empty;
//        string departureTime = departureTimeElement.GetString() ?? string.Empty;
//        string arrivalTime = arrivalTimeElement.GetString() ?? string.Empty;
//        string flightDate = flightDateElement.GetString() ?? string.Empty;
//        string departureIata = departureIataElement.GetString() ?? string.Empty;
//        string arrivalIata = arrivalIataElement.GetString() ?? string.Empty;
//        int flightPrice = flightPriceElement.GetInt32();


//        var getUserCmd = new MySqlCommand("SELECT id FROM users_net WHERE email = @Email", dbConnection);
//        getUserCmd.Parameters.AddWithValue("@Email", userEmail);

//        await dbConnection.OpenAsync();
//        var userIdResult = await getUserCmd.ExecuteScalarAsync();
//        await dbConnection.CloseAsync();

//        if (userIdResult == null)
//        {
//            return Results.NotFound("User not found.");
//        }

//        int userId = Convert.ToInt32(userIdResult);

//        var insertFlightCmd = new MySqlCommand(
//            "INSERT INTO saved_flights_net (flight_name, departure_time, arrival_time, flight_date, departure_iata, arrival_iata, user_id, price) " +
//            "VALUES (@FlightName, @DepartureTime, @ArrivalTime, @FlightDate, @DepartureIata, @ArrivalIata, @UserId, @Price)",
//            dbConnection);

//        insertFlightCmd.Parameters.AddWithValue("@FlightName", flightName);
//        insertFlightCmd.Parameters.AddWithValue("@DepartureTime", departureTime);
//        insertFlightCmd.Parameters.AddWithValue("@ArrivalTime", arrivalTime);
//        insertFlightCmd.Parameters.AddWithValue("@FlightDate", flightDate);
//        insertFlightCmd.Parameters.AddWithValue("@DepartureIata", departureIata);
//        insertFlightCmd.Parameters.AddWithValue("@ArrivalIata", arrivalIata);
//        insertFlightCmd.Parameters.AddWithValue("@UserId", userId);
//        insertFlightCmd.Parameters.AddWithValue("@Price", flightPrice);

//        await dbConnection.OpenAsync();
//        await insertFlightCmd.ExecuteNonQueryAsync();
//        await dbConnection.CloseAsync();

//        return Results.Ok(new { message = "Flight successfully saved." });
//    }
//    catch (Exception ex)
//    {
//        Console.WriteLine($"Error: {ex.Message}");
//        return Results.Problem("An error occurred while saving the flight.");
//    }
//});


//app.MapPost("api/save-hotel", [Authorize] async (HttpContext context, MySqlConnection dbConnection) =>
//{
//    try
//    {
//        var userEmail = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
//        if (string.IsNullOrEmpty(userEmail))
//        {
//            return Results.Unauthorized();
//        }

//        var body = await context.Request.ReadFromJsonAsync<JsonElement>();
//        if (!body.TryGetProperty("hotelName", out var hotelNameElement) ||
//            !body.TryGetProperty("hotelDistance", out var hotelDistanceElement) ||
//            !body.TryGetProperty("hotelStars", out var hotelStarsElement) ||
//            !body.TryGetProperty("hotelPrice", out var hotelPriceElement) ||
//            !body.TryGetProperty("hotelCountryCode", out var hotelCountryCodeElement) ||
//            !body.TryGetProperty("hotelIataCode", out var hotelIataCodeElement))
//        {
//            return Results.BadRequest("Invalid hotel data.");
//        }

//        string hotelName = hotelNameElement.GetString() ?? string.Empty;
//        float hotelDistance = hotelDistanceElement.GetSingle();
//        int hotelStars = hotelStarsElement.GetInt32();
//        int hotelPrice = hotelPriceElement.GetInt32();
//        string hotelCountryCode = hotelCountryCodeElement.GetString() ?? string.Empty;
//        string hotelIataCode = hotelIataCodeElement.GetString() ?? string.Empty;

//        var getUserCmd = new MySqlCommand("SELECT id FROM users_net WHERE email = @Email", dbConnection);
//        getUserCmd.Parameters.AddWithValue("@Email", userEmail);

//        await dbConnection.OpenAsync();
//        var userIdResult = await getUserCmd.ExecuteScalarAsync();
//        await dbConnection.CloseAsync();

//        if (userIdResult == null)
//        {
//            return Results.NotFound("User not found.");
//        }

//        int userId = Convert.ToInt32(userIdResult);

//        var insertHotelCmd = new MySqlCommand(
//            "INSERT INTO saved_hotels_net (hotel_name, hotel_distance, hotel_rating, user_id, price, country_code, iata_code) VALUES (@Name, @Distance, @Rating, @UserId, @Price, @CountryCode, @IataCode)",
//            dbConnection);

//        insertHotelCmd.Parameters.AddWithValue("@Name", hotelName);
//        insertHotelCmd.Parameters.AddWithValue("@Distance", hotelDistance);
//        insertHotelCmd.Parameters.AddWithValue("@Rating", hotelStars);
//        insertHotelCmd.Parameters.AddWithValue("@UserId", userId);
//        insertHotelCmd.Parameters.AddWithValue("@Price", hotelPrice);
//        insertHotelCmd.Parameters.AddWithValue("@CountryCode", hotelCountryCode);
//        insertHotelCmd.Parameters.AddWithValue("@IataCode", hotelIataCode);

//        await dbConnection.OpenAsync();
//        await insertHotelCmd.ExecuteNonQueryAsync();
//        await dbConnection.CloseAsync();

//        return Results.Ok(new { message = "Hotel successfully saved." });
//    }
//    catch (Exception ex)
//    {
//        Console.WriteLine($"Error: {ex.Message}");
//        return Results.Problem("An error occurred while saving the hotel.");
//    }
//});

//app.MapGet("api/saved-flights", [Authorize] async (HttpContext context, MySqlConnection dbConnection) =>
//{
//    try
//    {
//        var userEmail = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
//        if (string.IsNullOrEmpty(userEmail))
//        {
//            return Results.Unauthorized();
//        }

//        var getUserCmd = new MySqlCommand("SELECT id FROM users_net WHERE email = @Email", dbConnection);
//        getUserCmd.Parameters.AddWithValue("@Email", userEmail);

//        await dbConnection.OpenAsync();
//        var userIdResult = await getUserCmd.ExecuteScalarAsync();
//        await dbConnection.CloseAsync();

//        if (userIdResult == null)
//        {
//            return Results.NotFound("User not found.");
//        }

//        int userId = Convert.ToInt32(userIdResult);

//        var getFlightsCmd = new MySqlCommand(
//            "SELECT id, flight_name, departure_time, arrival_time, flight_date, departure_iata, arrival_iata, price " +
//            "FROM saved_flights_net WHERE user_id = @UserId",
//            dbConnection);

//        getFlightsCmd.Parameters.AddWithValue("@UserId", userId);

//        await dbConnection.OpenAsync();
//        var reader = await getFlightsCmd.ExecuteReaderAsync();

//        var savedFlights = new List<object>();
//        while (await reader.ReadAsync())
//        {
//            savedFlights.Add(new
//            {
//                Id = Convert.ToInt32(reader["id"]),
//                FlightName = reader["flight_name"].ToString(),
//                DepartureTime = reader["departure_time"].ToString(),
//                ArrivalTime = reader["arrival_time"].ToString(),
//                FlightDate = reader["flight_date"].ToString(),
//                DepartureIata = reader["departure_iata"].ToString(),
//                ArrivalIata = reader["arrival_iata"].ToString(),
//                Price = Convert.ToInt32(reader["price"])
//            });
//        }

//        await dbConnection.CloseAsync();

//        return Results.Ok(savedFlights);
//    }
//    catch (Exception ex)
//    {
//        Console.WriteLine($"Error: {ex.Message}");
//        return Results.Problem("An error occurred while fetching saved flights.");
//    }
//});


//app.MapGet("api/saved-hotels", [Authorize] async (HttpContext context, MySqlConnection dbConnection) =>
//{
//    try
//    {
//        var userEmail = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
//        if (string.IsNullOrEmpty(userEmail))
//        {
//            return Results.Unauthorized();
//        }

//        var getUserCmd = new MySqlCommand("SELECT id FROM users_net WHERE email = @Email", dbConnection);
//        getUserCmd.Parameters.AddWithValue("@Email", userEmail);

//        await dbConnection.OpenAsync();
//        var userIdResult = await getUserCmd.ExecuteScalarAsync();
//        await dbConnection.CloseAsync();

//        if (userIdResult == null)
//        {
//            return Results.NotFound("User not found.");
//        }

//        int userId = Convert.ToInt32(userIdResult);

//        var getHotelsCmd = new MySqlCommand(
//            "SELECT id, hotel_name, hotel_distance, hotel_rating, price, country_code, iata_code " +
//            "FROM saved_hotels_net WHERE user_id = @UserId",
//            dbConnection);

//        getHotelsCmd.Parameters.AddWithValue("@UserId", userId);

//        await dbConnection.OpenAsync();
//        var reader = await getHotelsCmd.ExecuteReaderAsync();

//        var savedHotels = new List<object>();
//        while (await reader.ReadAsync())
//        {
//            savedHotels.Add(new
//            {
//                Id = Convert.ToInt32(reader["id"]),
//                HotelName = reader["hotel_name"].ToString(),
//                HotelDistance = Convert.ToDecimal(reader["hotel_distance"]),
//                HotelRating = Convert.ToInt32(reader["hotel_rating"]),
//                HotelCountryCode = reader["country_code"].ToString(),
//                HotelIataCode = reader["iata_code"].ToString(),
//                HotelPrice = Convert.ToInt32(reader["price"])
//            });
//        }

//        await dbConnection.CloseAsync();

//        return Results.Ok(savedHotels);
//    }
//    catch (Exception ex)
//    {
//        Console.WriteLine($"Error: {ex.Message}");
//        return Results.Problem("An error occurred while fetching saved hotels.");
//    }
//});

//app.MapDelete("api/delete-flight/{id}", [Authorize] async (int id, HttpContext context, MySqlConnection dbConnection) =>
//{
//    try
//    {
//        var userEmail = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
//        if (string.IsNullOrEmpty(userEmail))
//        {
//            return Results.Unauthorized();
//        }

//        var getUserCmd = new MySqlCommand("SELECT id FROM users_net WHERE email = @Email", dbConnection);
//        getUserCmd.Parameters.AddWithValue("@Email", userEmail);

//        await dbConnection.OpenAsync();
//        var userIdResult = await getUserCmd.ExecuteScalarAsync();
//        await dbConnection.CloseAsync();

//        if (userIdResult == null)
//        {
//            return Results.NotFound("User not found.");
//        }

//        int userId = Convert.ToInt32(userIdResult);

//        var deleteCmd = new MySqlCommand(
//            "DELETE FROM saved_flights_net WHERE id = @Id AND user_id = @UserId",
//            dbConnection);
//        deleteCmd.Parameters.AddWithValue("@Id", id);
//        deleteCmd.Parameters.AddWithValue("@UserId", userId);

//        await dbConnection.OpenAsync();
//        int rowsAffected = await deleteCmd.ExecuteNonQueryAsync();
//        await dbConnection.CloseAsync();

//        return rowsAffected > 0 ? Results.Ok("Flight deleted successfully.") : Results.NotFound("Flight not found.");
//    }
//    catch (Exception ex)
//    {
//        Console.WriteLine($"Error: {ex.Message}");
//        return Results.Problem("An error occurred while deleting the flight.");
//    }
//});

//app.MapDelete("api/delete-hotel/{id}", [Authorize] async (int id, HttpContext context, MySqlConnection dbConnection) =>
//{
//    try
//    {
//        var userEmail = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
//        if (string.IsNullOrEmpty(userEmail))
//        {
//            return Results.Unauthorized();
//        }

//        var getUserCmd = new MySqlCommand("SELECT id FROM users_net WHERE email = @Email", dbConnection);
//        getUserCmd.Parameters.AddWithValue("@Email", userEmail);

//        await dbConnection.OpenAsync();
//        var userIdResult = await getUserCmd.ExecuteScalarAsync();
//        await dbConnection.CloseAsync();

//        if (userIdResult == null)
//        {
//            return Results.NotFound("User not found.");
//        }

//        int userId = Convert.ToInt32(userIdResult);

//        var deleteCmd = new MySqlCommand(
//            "DELETE FROM saved_hotels_net WHERE id = @Id AND user_id = @UserId",
//            dbConnection);
//        deleteCmd.Parameters.AddWithValue("@Id", id);
//        deleteCmd.Parameters.AddWithValue("@UserId", userId);

//        await dbConnection.OpenAsync();
//        int rowsAffected = await deleteCmd.ExecuteNonQueryAsync();
//        await dbConnection.CloseAsync();

//        return rowsAffected > 0 ? Results.Ok("Hotel deleted successfully.") : Results.NotFound("Hotel not found.");
//    }
//    catch (Exception ex)
//    {
//        Console.WriteLine($"Error: {ex.Message}");
//        return Results.Problem("An error occurred while deleting the hotel.");
//    }
//});


//app.MapPost("api/book-flight", [Authorize] async (HttpContext context, HttpClient client, MySqlConnection dbConnection) =>
//{

//    var sendGridApiKey = Environment.GetEnvironmentVariable("SENDGRID_API_KEY");

//    if (string.IsNullOrEmpty(sendGridApiKey))
//    {
//        return Results.BadRequest("SendGrid API Key is missing.");
//    }

//    var flightRequest = await context.Request.ReadFromJsonAsync<FlightBookingRequest>();

//    if (flightRequest == null)
//    {
//        return Results.BadRequest("Invalid booking details.");
//    }

//    var userEmail = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

//    if (string.IsNullOrEmpty(userEmail))
//    {
//        return Results.BadRequest("Unable to retrieve user email.");
//    }


//    var subject = $"Booking Confirmation for Flight: {flightRequest.FlightName}";
//    var plainTextContent = $@"
//        Dear fellow traveler,

//        Thank you for booking your flight with A2B Travel!

//        Here are your booking details:
//        Flight: {flightRequest.FlightName}
//        Date: {flightRequest.FlightDate}
//        Departure: {flightRequest.DepartureIata} at {flightRequest.DepartureTime}
//        Arrival: {flightRequest.ArrivalIata} at {flightRequest.ArrivalTime}
//        Tickets: {flightRequest.NumTickets}
//        Total Cost: ${flightRequest.TotalCost}

//        Safe travels,
//        The A2B Team
//    ";

//    var htmlContent = $@"
//        <strong>Dear fellow traveler,</strong><br/><br/>
//        Thank you for booking your flight with <strong>A2B Travel</strong>!<br/><br/>
//        <strong>Here are your booking details:</strong><br/>
//        <ul>
//            <li>Flight: {flightRequest.FlightName}</li>
//            <li>Date: {flightRequest.FlightDate}</li>
//            <li>Departure: {flightRequest.DepartureIata} at {flightRequest.DepartureTime}</li>
//            <li>Arrival: {flightRequest.ArrivalIata} at {flightRequest.ArrivalTime}</li>
//            <li>Tickets: {flightRequest.NumTickets}</li>
//            <li>Total Cost: <strong>${flightRequest.TotalCost}</strong></li>
//        </ul>
//        <br/>
//        Safe travels,<br/>
//        <strong>The A2B Team</strong>
//    ";

//    var from = new EmailAddress("thea2bteam2024@gmail.com", "A2B Travel");
//    var to = new EmailAddress(userEmail);
//    var message = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);

//    try
//    {
//        var sgClient = new SendGridClient(sendGridApiKey);
//        var response = await sgClient.SendEmailAsync(message);

//        if (response.StatusCode == System.Net.HttpStatusCode.Accepted)
//        {
//            return Results.Ok(new { message = "Confirmation email sent successfully!" });
//        }
//        else
//        {
//            return Results.Json(new { error = "Failed to send confirmation email." }, statusCode: (int)response.StatusCode);
//        }
//    }
//    catch (Exception ex)
//    {
//        return Results.Json(new { error = $"Failed to send confirmation email: {ex.Message}" });
//    }
//});

//app.MapPost("api/book-hotel", [Authorize] async (HttpContext context, HttpClient client, MySqlConnection dbConnection) => 
//{
//    var sendGridApiKey = Environment.GetEnvironmentVariable("SENDGRID_API_KEY");


//    if (string.IsNullOrEmpty(sendGridApiKey))
//    {
//        return Results.BadRequest("SendGrid API Key is missing.");
//    }

//    var hotelRequest = await context.Request.ReadFromJsonAsync<HotelBookingRequest>();

//    if (hotelRequest == null)
//    {
//        return Results.BadRequest("Invalid booking details.");
//    }

//    var userEmail = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

//    if (string.IsNullOrEmpty(userEmail))
//    {
//        return Results.BadRequest("Unable to retrieve user email.");
//    }

//    var subject = $"Booking Confirmation for Hotel: {hotelRequest.HotelName}";
//    var plainTextContent = $@"
//        Dear fellow traveler,

//        Thank you for booking your hotel with A2B Travel!

//        Here are your booking details:

//        Hotel: {hotelRequest.HotelName}
//        Distance: {hotelRequest.HotelDistance} MI
//        IATA Code: {hotelRequest.HotelIataCode}
//        Country Code: {hotelRequest.HotelCountryCode}
//        Rating: {hotelRequest.HotelRating} stars
//        Number of nights: {hotelRequest.NumNights}
//        Total Cost: {hotelRequest.TotalCost}

//        Safe travels,

//        The A2B Team";

//    var htmlContent = $@"
//        <strong>Dear fellow traveler,</strong><br/><br/>
//        Thank you for booking your hotel with <strong>A2B Travel</strong>!<br/><br/>
//        <strong>Here are your booking details:</strong><br/>
//        <ul>
//            <li>Hotel: {hotelRequest.HotelName}</li>
//            <li>Distance: {hotelRequest.HotelDistance} miles</li>
//            <li>IATA Code: {hotelRequest.HotelIataCode}</li>
//            <li>Country Code: {hotelRequest.HotelCountryCode}</li>
//            <li>Rating: {hotelRequest.HotelRating} stars</li>
//            <li>Number of nights: {hotelRequest.NumNights}</li>
//            <li>Total Cost: <strong>${hotelRequest.TotalCost}<strong/></li>
//        </ul>
//        <br/>
//        Safe travels,<br/>
//        <strong>The A2B Team</strong>
//        ";



//    var from = new EmailAddress("thea2bteam2024@gmail.com", "A2B Travel");
//    var to = new EmailAddress(userEmail);
//    var message = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);


//    try
//    {
//        var sgClient = new SendGridClient(sendGridApiKey);
//        var response = await sgClient.SendEmailAsync(message);

//        if (response.StatusCode == System.Net.HttpStatusCode.Accepted)
//        {
//            return Results.Ok(new { message = "Confirmation email sent successfully!" });
//        }
//        else
//        {
//            return Results.Json(new { error = "Failed to send confirmation email." }, statusCode: (int)response.StatusCode);
//        }
//    }
//    catch (Exception ex)
//    {
//        return Results.Json(new { error = $"Failed to send confirmation email: {ex.Message}" });
//    }

//});

//app.MapDelete("api/delete-account", [Authorize] async (HttpContext context, MySqlConnection dbConnection) =>
//{
//    try 
//    {
//        var email = context.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;

//        if (string.IsNullOrEmpty(email))
//        {
//            return Results.BadRequest("User email not found in token.");
//        }

//        var deleteUserCmd = new MySqlCommand("DELETE FROM users_net WHERE email = @Email", dbConnection);
//        deleteUserCmd.Parameters.AddWithValue("@Email", email);

//        await dbConnection.OpenAsync();
//        var rowsAffected = await deleteUserCmd.ExecuteNonQueryAsync();
//        await dbConnection.CloseAsync();

//        if (rowsAffected > 0)
//        {
//            return Results.Ok("User account deleted successfully.");
//        }
//        else
//        {
//            return Results.BadRequest("Account not found or failed to delete.");
//        }

//    }
//    catch (Exception ex) {
//        Console.WriteLine($"Error: {ex.Message}");
//        return Results.Problem("An error occurred during account deletion.");
//    }

//});

//app.MapGet("api/", () => "Hello World!");

//app.Run();
