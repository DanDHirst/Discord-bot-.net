# Discord Bot Application

A Discord bot built with .NET that responds to PING with PONG and includes timer functionality with a React frontend for configuration.

## Project Structure

```
Discord-bot-.net/
├── DiscordBot/          # Discord bot application
├── API/                 # .NET API backend
├── API.Tests/           # Unit tests for API project
├── ReactFrontend/       # React configuration UI
└── README.md
```

## Getting Started

### 1. Discord Bot Setup

#### Prerequisites
- .NET 8.0 SDK
- Discord Developer Account

#### Create a Discord Bot
1. Go to [Discord Developer Portal](https://discord.com/developers/applications)
2. Click "New Application" and give it a name
3. Go to the "Bot" section
4. Click "Add Bot"
5. Under "Token", click "Copy" to copy your bot token
6. Under "Privileged Gateway Intents", enable "Message Content Intent"

#### Configure the Bot
1. Navigate to the DiscordBot folder
2. Open `appsettings.json`
3. Replace `YOUR_DISCORD_BOT_TOKEN_HERE` with your actual bot token
4. Set the `API:ApiKey` to match your API configuration (must match the value in API/appsettings.json)

**Example DiscordBot/appsettings.json:**
```json
{
  "Discord": {
    "Token": "your-actual-discord-bot-token-here"
  },
  "API": {
    "BaseUrl": "https://localhost:7001",
    "ApiKey": "your-api-secret-key-change-this"
  }
}
```

#### Invite Bot to Server
1. In Discord Developer Portal, go to "OAuth2" → "URL Generator"
2. Select scopes: `bot`
3. Select bot permissions: `Send Messages`, `Read Messages`, `Read Message History`
4. Copy the generated URL and open it in your browser
5. Select your server and authorize the bot

#### Run the Bot
```bash
cd DiscordBot
dotnet restore
dotnet run
```

### 2. Test Bot Commands
Once the bot is running and in your Discord server:

#### PING/PONG Command
1. Type `PING` in any channel where the bot has access
2. The bot should respond with `PONG`

#### Timer Commands
1. **Basic Timer**: Type `TIMER 5` to set a 5-minute timer
2. **Timer with Message**: Type `TIMER 10 Check the oven` to set a 10-minute timer with a custom reminder
3. The bot will:
   - Confirm the timer is set with expiration time
   - Notify you when the timer expires with a ping
   - Include your custom message in the notification

### 3. API Setup

#### Configure API Security
1. Navigate to the API folder
2. The `appsettings.json` file is included in this repository for easy setup
3. For a new setup, you can customize the security configuration:

**Required Configuration (API/appsettings.json):**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=timers.db"
  },
  "Jwt": {
    "SecretKey": "your-super-secret-key-at-least-32-characters-long-for-security",
    "Issuer": "DiscordBotAPI",
    "Audience": "DiscordBot",
    "ExpiryMinutes": 1440
  },
  "ApiKey": "your-api-secret-key-change-this",
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

⚠️ **Security Notes:**
- For this proof of concept, `appsettings.json` files are included in git for easy setup
- In production, you should exclude `appsettings.json` from git and use secure configuration
- Change `SecretKey` to a strong, random 32+ character string for production use
- Change `ApiKey` to a strong, random string for production use
- Use the same `ApiKey` in both API and DiscordBot appsettings.json

#### Run the API
```bash
cd API
dotnet restore
dotnet run
```

The API will start on `https://localhost:7001` (or the port shown in console).

#### API Documentation
Once running, visit `https://localhost:7001/swagger` for interactive API documentation.

#### API Authentication
The API is protected with JWT (JSON Web Token) authentication. All endpoints except the authentication endpoint require a valid JWT token.

**Getting a Token:**
```bash
# POST /api/auth/token
curl -X POST https://localhost:7001/api/auth/token \
  -H "Content-Type: application/json" \
  -d '{"apiKey": "your-api-secret-key-change-this"}'
```

**Response:**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiresAt": "2024-09-15T16:52:00Z",
  "tokenType": "Bearer"
}
```

**Using the Token:**
Include the token in the `Authorization` header for all API requests:
```bash
Authorization: Bearer <your-jwt-token>
```

**Example Protected API Call:**
```bash
curl -X GET https://localhost:7001/api/timer/expired \
  -H "Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
```

**Configuration:**
Update your `appsettings.json` with secure values:
```json
{
  "Jwt": {
    "SecretKey": "your-super-secret-key-that-is-at-least-32-characters-long",
    "Issuer": "DiscordBotAPI",
    "Audience": "DiscordBot",
    "ExpiryMinutes": 1440
  },
  "ApiKey": "your-api-secret-key-change-this"
}
```

**Security Notes:**
- Change the default `SecretKey` and `ApiKey` values before deploying
- Tokens expire after 24 hours (1440 minutes) by default
- The `appsettings.json` file is ignored by git for security
- Use the `appsettings.json.template` file as a reference

### 4. React Frontend Setup

#### Configure API Authentication
1. Navigate to ReactFrontend folder
2. Update the API key in the `.env` file to match your backend configuration:

```bash
# In ReactFrontend/.env
REACT_APP_API_KEY=your-api-secret-key-change-this
```

**Note:** The API key in `.env` must match the `ApiKey` value in `API/appsettings.json`

#### Install Dependencies
```bash
cd ReactFrontend
npm install
```

#### Start Development Server
```bash
npm start
```

The React app will start on `http://localhost:3000`

**Authentication Flow:**
- The React app automatically handles JWT authentication
- It requests tokens using the configured API key
- Tokens are automatically refreshed when they expire
- All API calls include the Bearer token in headers

#### Login Credentials (Demo Only)
- **Username**: `admin`
- **Password**: `discord123`

⚠️ **Demo Authentication**: Hardcoded credentials for demonstration only.

**Production Alternatives:**
- Microsoft Azure AD (Enterprise SSO)
- Discord OAuth2 (User-friendly for Discord users)
- Custom user database with ASP.NET Core Identity
- Enhanced API key system with database storage

#### Features
- 📊 **Dashboard**: View bot statistics and status
- 🚫 **Blocked Users**: Manage user blocking/unblocking
- 🔐 **Authentication**: Secure access to admin interface
- 📱 **Responsive Design**: Works on desktop and mobile

### 5. Running All Services Together

**⚠️ Important: Configure API Keys First!**
Before running the services, ensure all three applications use the same API key:

1. **API/appsettings.json** - Set `"ApiKey": "your-strong-secret-key"`
2. **DiscordBot/appsettings.json** - Set `"API:ApiKey": "your-strong-secret-key"` (same value)
3. **ReactFrontend/.env** - Set `REACT_APP_API_KEY=your-strong-secret-key` (same value)

For full functionality, run all three services:

#### Terminal/Command Prompt Method:
```bash
# Terminal 1 - Start API
cd API
dotnet run

# Terminal 2 - Start Discord Bot (in new terminal)
cd DiscordBot
dotnet run

# Terminal 3 - Start React Frontend (in new terminal)
cd ReactFrontend
npm start
```

#### VS Code Debug Method:
1. **Start API**: Select "Debug API" and press F5
2. **Start Bot**: Select "Debug Discord Bot" and press F5
3. **Start React**: Open terminal and run `cd ReactFrontend && npm start`

## API Endpoints

**⚠️ Authentication Required**: All endpoints below require a valid JWT token in the `Authorization: Bearer <token>` header. See [API Authentication](#api-authentication) section for details.

### Authentication

#### Get JWT Token
- **POST** `/api/auth/token`
- **Description**: Generate a JWT token for API access
- **Request Body**:
```json
{
  "apiKey": "your-api-secret-key-change-this"
}
```
- **Response**: JWT token with expiration details

### Timer Management

#### Create Timer
- **POST** `/api/timer`
- **Description**: Create a new timer for a Discord user
- **Request Body**:
```json
{
  "userId": "123456789012345678",
  "username": "DiscordUser#1234",
  "channelId": 987654321098765432,
  "durationMinutes": 5,
  "message": "Optional reminder message"
}
```
- **Response**: Timer object with ID and expiration details

#### Get Expired Timers
- **GET** `/api/timer/expired`
- **Description**: Retrieve all expired timers for Discord bot notifications
- **Response**: Array of expired timer objects

#### Complete Timer
- **POST** `/api/timer/{id}/complete`
- **Description**: Mark a timer as completed (prevents duplicate notifications)
- **Response**: 204 No Content on success

#### Get Timer Details
- **GET** `/api/timer/{id}`
- **Description**: Get details of a specific timer
- **Response**: Timer object

#### Get User Timers
- **GET** `/api/timer/user/{userId}?includeCompleted=false`
- **Description**: Get all timers for a specific user
- **Query Parameters**:
  - `includeCompleted` (optional): Include completed timers in results
- **Response**: Array of user's timer objects

### Database

The API uses SQLite database stored in `timers.db` file. The database is automatically created when the API starts.

#### Timer Schema
```sql
Timers (
  Id INTEGER PRIMARY KEY,
  UserId TEXT NOT NULL,
  Username TEXT NOT NULL,
  ChannelId INTEGER NOT NULL,
  DurationMinutes INTEGER NOT NULL,
  CreatedAt DATETIME NOT NULL,
  ExpiresAt DATETIME NOT NULL,
  IsCompleted BOOLEAN DEFAULT FALSE,
  CompletedAt DATETIME NULL,
  Message TEXT NULL
)
```

## Testing

The project includes a comprehensive test suite to ensure code quality and reliability.

### Test Projects

#### API.Tests
Unit tests for the API project covering core functionality:
- **Authentication**: JWT token generation and validation
- **Timer Management**: Creating, retrieving, and managing timers
- **User Blocking**: Blocking and unblocking Discord users
- **Database Operations**: Entity Framework operations with in-memory database

### Running Tests

#### Run All Tests
```bash
# Run all tests in the solution
dotnet test

# Run tests with detailed output
dotnet test --verbosity normal

# Run tests for specific project
dotnet test API.Tests/API.Tests.csproj
```

#### Test Coverage Report
```bash
# Generate coverage report (requires coverlet)
dotnet test --collect:"XPlat Code Coverage"

# View coverage in HTML format
dotnet tool install -g dotnet-reportgenerator-globaltool
reportgenerator -reports:"**/coverage.cobertura.xml" -targetdir:"coveragereport" -reporttypes:Html
```

### Test Structure

#### Current Test Coverage
- ✅ **AuthController Tests**: Token generation with valid/invalid API keys
- ✅ **BlockedUsersController Tests**: User blocking operations with database integration
- ✅ **TimerController Tests**: Timer creation, retrieval, and error handling
- ✅ **Database Integration Tests**: In-memory Entity Framework testing

#### Test Dependencies
- **xUnit**: Testing framework
- **Moq**: Mocking library for dependencies
- **Microsoft.EntityFrameworkCore.InMemory**: In-memory database for testing
- **Microsoft.AspNetCore.Mvc.Testing**: Integration testing support

#### Example Test Structure
```csharp
[Fact]
public async Task TimerController_CreateTimer_ValidRequest_ReturnsCreated()
{
    // Arrange
    var options = new DbContextOptionsBuilder<ApplicationDbContext>()
        .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
        .Options;

    await using var context = new ApplicationDbContext(options);
    var controller = new TimerController(context, mockLogger.Object);

    // Act
    var result = await controller.CreateTimer(request);

    // Assert
    var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
    Assert.Equal(request.UserId, response.UserId);
}
```

### Test Best Practices

#### Mocking
- Use Moq for mocking dependencies like ILogger and IConfiguration
- Create isolated test instances with unique in-memory databases
- Mock external services to ensure tests are deterministic

#### Database Testing
- Use Entity Framework In-Memory provider for database tests
- Each test uses a unique database name to prevent interference
- Tests automatically dispose of database contexts

#### Assertion Patterns
- Use specific assertion types (`Assert.IsType<T>`, `Assert.Equal`)
- Test both success and failure scenarios
- Verify response types and content

### Continuous Integration

Tests are designed to run in CI/CD environments:

```yaml
# Example GitHub Actions workflow
- name: Run Tests
  run: dotnet test --no-build --verbosity normal --collect:"XPlat Code Coverage"
  
- name: Upload Coverage
  uses: codecov/codecov-action@v3
  with:
    files: ./coverage.cobertura.xml
```

### Adding New Tests

When adding new features, include corresponding tests:

1. **Controller Tests**: Test HTTP endpoints and response codes
2. **Service Tests**: Test business logic and data operations
3. **Integration Tests**: Test end-to-end functionality
4. **Unit Tests**: Test individual methods and edge cases

**Test Naming Convention:**
`{ClassName}_{MethodName}_{Scenario}_{ExpectedResult}`

Example: `AuthController_GetToken_InvalidApiKey_ReturnsUnauthorized`

## Features Implemented
- [x] Basic Discord bot connection
- [x] PING/PONG command response
- [x] .NET Web API with timer endpoints
- [x] SQLite database integration
- [x] Entity Framework Core with proper schema
- [x] Swagger API documentation
- [x] JWT authentication for API security
- [x] Timer command in Discord bot (`TIMER <minutes>` or `TIMER <minutes> <message>`)
- [x] API integration in Discord bot
- [x] Timer expiration notifications with user mentions
- [x] Background service for checking expired timers
- [x] Comprehensive logging and error handling
- [x] React frontend with Discord theme
- [x] User blocking system with web interface
- [x] Authentication for admin access
- [x] Blocked user API endpoints (GET, POST, DELETE)
- [x] Real-time blocked user checking in Discord bot
- [x] Comprehensive unit test suite with xUnit and Moq
- [x] API controller testing with in-memory database
- [x] Authentication and JWT token testing
- [x] Database integration testing

## Architecture Overview

```
Discord Bot (Port varies) ←--JWT Auth--→ API (Port 7001) ←→ SQLite Database
                                              ↑
                                          JWT Auth
                                              ↓
                                  React Frontend (Port 3000)
```

**Authentication Flow:**
1. **API Key Authentication**: Both Discord Bot and React Frontend use a shared API key
2. **JWT Token Generation**: API key is exchanged for JWT tokens via `/api/auth/token`
3. **Secure API Access**: All API endpoints require valid JWT tokens in `Authorization: Bearer <token>` header
4. **Automatic Token Refresh**: Clients automatically refresh expired tokens
5. **Token Expiry**: JWT tokens expire after 24 hours for security

## Troubleshooting

### Authentication Issues

**Problem: Discord bot can't access API (401 Unauthorized)**
- Check that `DiscordBot/appsettings.json` has the correct `API:ApiKey`
- Ensure the API key matches `API/appsettings.json` exactly
- Verify the API is running on the correct port (7001)

**Problem: React frontend can't load data**
- Check that `ReactFrontend/.env` has the correct `REACT_APP_API_KEY`
- Restart React dev server after modifying `.env` file (`npm start`)
- Open browser dev tools and check for authentication errors
- Ensure all three applications use the same API key

**Problem: JWT token errors**
- Verify `Jwt:SecretKey` in `API/appsettings.json` is at least 32 characters
- Check that the API service is running and accessible
- Look for token expiration messages in logs (tokens expire after 24 hours)

### Configuration Checklist
- [ ] API/appsettings.json exists and has strong SecretKey and ApiKey
- [ ] DiscordBot/appsettings.json has API:ApiKey matching the API
- [ ] ReactFrontend/.env has REACT_APP_API_KEY matching the API
- [ ] React dev server restarted after creating/modifying .env file
- [ ] All three services can communicate (no firewall/network issues)

## Next Steps
1. ✅ Create the .NET API for timer functionality
2. ✅ Update Discord bot to integrate with API
3. ✅ Implement timer commands and notifications
4. ✅ Build React frontend for user management
5. ✅ Add JWT authentication system
