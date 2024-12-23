using MySql.Data.MySqlClient;
using DotNetEnv;
using a2bapi.Models;
using Microsoft.AspNetCore.Diagnostics;
using System.Text.Json;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;


var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;
Env.Load();

builder.Services.AddAuthentication( x => {
    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(x => 
{
    x.TokenValidationParameters = new TokenValidationParameters
    {
        ValidIssuer = "http://localhost:5030",
        ValidAudience = "http://localhost:5173",
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable("JWT_SECRET_KEY") ?? "YourDevSecretKey")),
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true
    };
});

builder.Services.AddAuthorization();


string dbConnectionString = $"Server={Environment.GetEnvironmentVariable("DB_SERVER")};" +
                            $"Port={Environment.GetEnvironmentVariable("DB_PORT")};" +
                            $"Database={Environment.GetEnvironmentVariable("DB_NAME")};" +
                            $"User={Environment.GetEnvironmentVariable("DB_USER")};" +
                            $"Password={Environment.GetEnvironmentVariable("DB_PASSWORD")}";


builder.Services.AddSingleton<MySqlConnection>(_ => new MySqlConnection(dbConnectionString));
builder.Services.AddHttpClient();
builder.Logging.AddConsole();
builder.Logging.AddDebug();


var corsPolicyName = "AllowSpecificOrigins";

builder.Services.AddCors(options =>
{
    options.AddPolicy(corsPolicyName, policy =>
    {
        policy.WithOrigins("http://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
    
});

var app = builder.Build();

app.UseCors(corsPolicyName);
app.UseExceptionHandler("/error");
app.UseAuthentication();
app.UseAuthorization();
app.Map("/error", (HttpContext context) =>
{
    var exception = context.Features.Get<IExceptionHandlerFeature>()?.Error;
    return Results.Problem(exception?.Message ?? "An unknown error occured");
});

app.MapGet("/flights/{from}-{to}", async (string from, string to, IHttpClientFactory httpClientFactory) => 
{
    var apiKey = Environment.GetEnvironmentVariable("API_ACCESS_KEY");

    if (string.IsNullOrEmpty(apiKey)) {
        return Results.Problem("API access key is not set.");
    }

    var client = httpClientFactory.CreateClient();
    var url = "http://api.aviationstack.com/v1/flights";

    var query = new Dictionary<string, string>
    {
        { "access_key", apiKey },
        { "dep_iata", from.Trim().ToUpper() },
        { "arr_iata", to.Trim().ToUpper() }
    };

    try {
        var response = await client.GetAsync($"{url}?{string.Join("&", query.Select(kvp => $"{kvp.Key}={kvp.Value}"))}");

        if (!response.IsSuccessStatusCode) {
            var error = await response.Content.ReadAsStringAsync();
            return Results.Problem($"Error fetching flights: {error}");
        }

        var responseJson = await response.Content.ReadFromJsonAsync<FlightsResponse>();

        if (responseJson?.Data == null || responseJson.Data.Count == 0) {
            return Results.Ok(new { Message = "No flights found for the specified route."});
        }

        return Results.Ok(responseJson.Data);
    }
    catch (Exception ex) {
        return Results.Problem($"Error fetching flights: {ex.Message}");
    }

});

app.MapGet("/test-db", async (MySqlConnection dbConnection) =>
{
    try
    {
        await dbConnection.OpenAsync();
        return Results.Ok("Database connection successful!");
    }
    catch (Exception ex)
    {
        return Results.Problem($"Database connection failed: {ex.Message}");
    }
});

static async Task<string> GetAccessTokenAsync(IHttpClientFactory httpClientFactory)
{
    var apiKey = Environment.GetEnvironmentVariable("AMADEUS_API_KEY");
    var apiSecret = Environment.GetEnvironmentVariable("AMADEUS_API_SECRET");

    if (string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(apiSecret))
    {
        throw new Exception("Amadeus API credentials are missing.");
    }

    var client = httpClientFactory.CreateClient();
    var tokenUrl = "https://test.api.amadeus.com/v1/security/oauth2/token";
    var authHeader = Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes($"{apiKey}:{apiSecret}"));

    var content = new FormUrlEncodedContent(new Dictionary<string, string>
    {
        { "grant_type", "client_credentials" }
    });

    client.DefaultRequestHeaders.Add("Authorization", $"Basic {authHeader}");

    var response = await client.PostAsync(tokenUrl, content);

    if (!response.IsSuccessStatusCode)
    {
        var error = await response.Content.ReadAsStringAsync();
        throw new Exception($"Failed to get access token: {error}");
    }

    var jsonResponse = await response.Content.ReadAsStringAsync();
    Console.WriteLine($"Token Response: {jsonResponse}");

    var tokenResponse = JsonSerializer.Deserialize<AccessTokenResponse>(jsonResponse);

    return tokenResponse?.AccessToken ?? throw new Exception("Access token not found in response.");
}

static string GenerateJwt(string email) 
{
    var secretKey = Environment.GetEnvironmentVariable("JWT_SECRET_KEY") ?? "YourDevSecretKey";
    var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
    var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

    var claims = new[] {
        new Claim(JwtRegisteredClaimNames.Sub, email),
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
    };

    var token = new JwtSecurityToken(
        issuer: "http://localhost:5030",
        audience: "http://localhost:5173",
        claims: claims,
        expires: DateTime.UtcNow.AddHours(1),
        signingCredentials: credentials
    );

    return new JwtSecurityTokenHandler().WriteToken(token);

}


app.MapGet("/hotels/{to}-{dist}-{stars}", [Authorize] async (string to, string dist, string stars, IHttpClientFactory httpClientFactory) =>
{
    
    try {

        to = to.ToUpper();

        Console.WriteLine("Starting GetAccessTokenAsync...");
        var accessToken = await GetAccessTokenAsync(httpClientFactory);
        Console.WriteLine($"Retrieved Access Token: {accessToken}");

        var client = httpClientFactory.CreateClient();
        var hotelApiUrl = "https://test.api.amadeus.com/v1/reference-data/locations/hotels/by-city";

        var queryParameters = new Dictionary<string, string>
        {
            { "cityCode", to },
            { "radius", dist },
            { "radiusUnit", "MILE" },
            { "ratings", stars },
            { "hotelSource", "ALL" }
        };

        var queryString = string.Join("&", queryParameters.Select(kvp => $"{kvp.Key}={Uri.EscapeDataString(kvp.Value)}"));
        var requestUrl = $"{hotelApiUrl}?{queryString}";
        Console.WriteLine($"Request URL: {requestUrl}");

        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");
        var response = await client.GetAsync(requestUrl);

        if (!response.IsSuccessStatusCode) 
        {

            var error = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Error Response: {error}");
            return Results.Problem($"Error fetching hotels: {error}");

        }

        var jsonString = await response.Content.ReadAsStringAsync();
        Console.WriteLine($"Hotel API Response: {jsonString}");
        
        var hotelResponse = JsonSerializer.Deserialize<a2bapi.Models.Hotels.HotelApiResponse>(jsonString);
        

        if (hotelResponse?.Data == null)
        {
            return Results.Problem("No hotels found.");
        }

        return Results.Ok(hotelResponse.Data);

    }
    catch (Exception ex) {

        Console.WriteLine($"Error: {ex.Message}");
        return Results.Problem($"Error: {ex.Message}");
        
    }
});


app.MapPost("/signup", async(HttpContext context, MySqlConnection dbConnection) => 
{

    try 
    {
        var body = await context.Request.ReadFromJsonAsync<User>();
        if (body == null || string.IsNullOrWhiteSpace(body.Email) || string.IsNullOrWhiteSpace(body.Password)) 
        {
            return Results.BadRequest("Email and password are required.");
        }

        var checkUserCmd = new MySqlCommand("SELECT id FROM users_net WHERE email = @Email", dbConnection);
        checkUserCmd.Parameters.AddWithValue("@Email", body.Email);
        await dbConnection.OpenAsync();
        var existingUser = await checkUserCmd.ExecuteScalarAsync();

        if (existingUser != null) 
        {
            await dbConnection.CloseAsync();
            return Results.Conflict("Email already in use.");
        }

        string hashedPassword = HashPassword(body.Password);

        var insertUserCmd = new MySqlCommand("INSERT INTO users_net (email, password) VALUES (@Email, @Password)", dbConnection);
        insertUserCmd.Parameters.AddWithValue("@Email", body.Email);
        insertUserCmd.Parameters.AddWithValue("@Password", hashedPassword);

        await insertUserCmd.ExecuteNonQueryAsync();
        await dbConnection.CloseAsync();

        var jwt = GenerateJwt(body.Email);

        return Results.Ok(new { Message = "User registered successfully.", Token = jwt });

    }
    catch (Exception ex) 
    {

        Console.WriteLine($"Error: {ex.Message}");
        return Results.Problem("An error occurred during user registration.");

    }

});

app.MapPost("/login", async (HttpContext context, MySqlConnection dbConnection) =>
{
    try 
    {
        var body = await context.Request.ReadFromJsonAsync<User>();
        if (body == null || string.IsNullOrEmpty(body.Email) || string.IsNullOrEmpty(body.Password))
        {
            return Results.BadRequest("Email and password are required.");
        }

        var checkUserCmd = new MySqlCommand("SELECT password FROM users_net WHERE email = @Email", dbConnection);
        checkUserCmd.Parameters.AddWithValue("@Email", body.Email);
        await dbConnection.OpenAsync();
        var result = await checkUserCmd.ExecuteScalarAsync();
        var hashedPassword = result?.ToString();
        await dbConnection.CloseAsync();

        if (hashedPassword == null || !VerifyPassword(body.Password, hashedPassword)) 
        {
            return Results.Unauthorized();
        }

        var jwt = GenerateJwt(body.Email);

        return Results.Ok(new { Message = "Login successful.", Token = jwt});
    }
    catch (Exception ex) 
    {
        Console.WriteLine($"Error: {ex.Message}");
        return Results.Problem("An error occurred during login.");
    }

});

static bool VerifyPassword(string password, string hashedPassword) 
{
    var hashedBytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
    var computedHash = Convert.ToBase64String(hashedBytes);
    return computedHash == hashedPassword;
}

static string HashPassword(string password) 
{
    var hashedBytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
    return Convert.ToBase64String(hashedBytes);
}

app.MapGet("/", () => "Hello World!");

app.Run();
