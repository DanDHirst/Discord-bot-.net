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

### 2. Test PING/PONG
Once the bot is running and in your Discord server:
1. Type `PING` in any channel where the bot has access
2. The bot should respond with `PONG`

## Features Implemented
- [x] Basic Discord bot connection
- [x] PING/PONG command response
- [ ] Timer functionality
- [ ] API integration
- [ ] React frontend
- [ ] User blocking system

## Next Steps
1. Create the .NET API for timer functionality
2. Implement timer commands in the bot
3. Build React frontend for user management
4. Add authentication system
