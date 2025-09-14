# Discord Bot Application

A Discord bot built with .NET that responds to PING with PONG and includes timer functionality with a React frontend for configuration.

## Project Structure

```
Discord-bot-.net/
├── DiscordBot/          # Discord bot application
├── API/                 # .NET API backend (to be created)
├── ReactFrontend/       # React configuration UI (to be created)
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

#### Run the API
```bash
cd API
dotnet restore
dotnet run
```

The API will start on `https://localhost:7001` (or the port shown in console).

#### API Documentation
Once running, visit `https://localhost:7001/swagger` for interactive API documentation.

### 4. Running Both Services Together

For full timer functionality, you need to run both the API and Discord bot:

#### Terminal/Command Prompt Method:
```bash
# Terminal 1 - Start API
cd API
dotnet run

# Terminal 2 - Start Discord Bot (in new terminal)
cd DiscordBot
dotnet run
```

#### VS Code Debug Method:
1. **Start API**: Select "Debug API" and press F5
2. **Start Bot**: Select "Debug Discord Bot" and press F5
3. Both will run simultaneously for full functionality

## API Endpoints

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

## Features Implemented
- [x] Basic Discord bot connection
- [x] PING/PONG command response
- [x] .NET Web API with timer endpoints
- [x] SQLite database integration
- [x] Entity Framework Core with proper schema
- [x] Swagger API documentation
- [x] Timer command in Discord bot (`TIMER <minutes>` or `TIMER <minutes> <message>`)
- [x] API integration in Discord bot
- [x] Timer expiration notifications with user mentions
- [x] Background service for checking expired timers
- [x] Comprehensive logging and error handling
- [ ] React frontend
- [ ] User blocking system

## Architecture Overview

```
Discord Bot (Port varies) ←→ API (Port 7001) ←→ SQLite Database
                                    ↓
                            React Frontend (Port 3000)
```

## Next Steps
1. ✅ Create the .NET API for timer functionality
2. Update Discord bot to integrate with API
3. Implement timer commands and notifications
4. Build React frontend for user management
5. Add authentication system
