using InterviewPrepSystem.AI;
using InterviewPrepSystem.Data;
using InterviewPrepSystem.Helpers;
using InterviewPrepSystem.Middleware;
using InterviewPrepSystem.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ================================================================
//  DATABASE
// ================================================================
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ================================================================
//  JWT AUTHENTICATION
// ================================================================
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey   = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not configured");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme    = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey         = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
        ValidateIssuer           = true,
        ValidIssuer              = jwtSettings["Issuer"],
        ValidateAudience         = true,
        ValidAudience            = jwtSettings["Audience"],
        ValidateLifetime         = true,
        ClockSkew                = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();

// ================================================================
//  AI SERVICE — swappable: OpenAI or Gemini
// ================================================================
var aiProvider = builder.Configuration["AISettings:Provider"] ?? "Groq";

if (aiProvider == "Gemini")
    builder.Services.AddHttpClient<IAIService, GeminiService>();
else if (aiProvider == "OpenAI")
    builder.Services.AddHttpClient<IAIService, OpenAIService>();
else
    builder.Services.AddHttpClient<IAIService, GroqService>(); // Default = Groq

// ================================================================
//  APPLICATION SERVICES
// ================================================================
builder.Services.AddScoped<IJwtHelper,        JwtHelper>();
builder.Services.AddScoped<IAuthService,      AuthService>();
builder.Services.AddScoped<ICategoryService,  CategoryService>();
builder.Services.AddScoped<IQuestionService,  QuestionService>();
builder.Services.AddScoped<IMockTestService,  MockTestService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();

// ================================================================
//  CORS (for React/Angular frontend)
// ================================================================
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod());
});

// ================================================================
//  SWAGGER WITH JWT SUPPORT
// ================================================================
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title       = "AI Interview Prep System API",
        Version     = "v1",
        Description = "Complete REST API for AI-powered interview preparation",
        Contact = new OpenApiContact { Name = "Adarsh Gupta", Email = "adarshgupta02027@gmail.com" }
    });

    // JWT Auth in Swagger UI
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name         = "Authorization",
        Type         = SecuritySchemeType.ApiKey,
        Scheme       = "Bearer",
        BearerFormat = "JWT",
        In           = ParameterLocation.Header,
        Description  = "Enter: Bearer {dsjnfjsnjfndsjnfjdsknfjdsnjknasjdfff4498s468h1g4h894846g}"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });

    // Include XML comments
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath)) c.IncludeXmlComments(xmlPath);
});

builder.Services.AddControllers();

// ================================================================
//  BUILD APP
// ================================================================
var app = builder.Build();

// ----------------------------------------------------------------
//  Auto-apply migrations on startup (dev convenience)
// ----------------------------------------------------------------
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();
}

// ----------------------------------------------------------------
//  MIDDLEWARE PIPELINE
// ----------------------------------------------------------------
app.UseGlobalExceptionHandler();   // Custom exception handler

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Interview Prep API v1");
        c.RoutePrefix = string.Empty; // Serve at root
    });
}

app.UseHttpsRedirection();
app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
