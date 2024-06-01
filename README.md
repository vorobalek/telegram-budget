# Telegram-Budget

Small profit-loss calculator via serverless telegram bot. Made far away from home in order to control family expenses and shared budgets.

## Try it out
[Telegram Demo Bot](https://t.me/telegram_budget_demo_bot)

## Preview

| Scenario            |                                                                                                                 |                                                                                                                |                                                                                                                |                                                                                                                |
|---------------------|-----------------------------------------------------------------------------------------------------------------|----------------------------------------------------------------------------------------------------------------|----------------------------------------------------------------------------------------------------------------|----------------------------------------------------------------------------------------------------------------|
| First Usage         | <img src="https://github.com/vorobalek/telegram-budget/assets/106157881/6882a7b0-a6d1-47b9-9385-8322cdae3d9f">  | <img src="https://github.com/vorobalek/telegram-budget/assets/106157881/db63db48-ebfa-462c-b28c-fc2de7f4c3aa"> | <img src="https://github.com/vorobalek/telegram-budget/assets/106157881/3ec5a5f5-8ffd-4e05-a497-b461443c9c53"> | <img src="https://github.com/vorobalek/telegram-budget/assets/106157881/5f260908-6f49-47b8-a2a7-643292fae277"> |
| Adding transaction  | <img src="https://github.com/vorobalek/telegram-budget/assets/106157881/40ba7725-5bf5-4bf6-9179-a0d4e1f8f0fd">  | <img src="https://github.com/vorobalek/telegram-budget/assets/106157881/0a100948-a4d9-4029-b632-78afea85d892"> |                                                                                                                |                                                                                                                |
| Editing transaction | <img src="https://github.com/vorobalek/telegram-budget/assets/106157881/b93e8564-3b81-4542-8135-655d7637a257">  | <img src="https://github.com/vorobalek/telegram-budget/assets/106157881/36543792-32f1-4c5a-a521-f81672e02620"> | <img src="https://github.com/vorobalek/telegram-budget/assets/106157881/3690ddd7-261f-4a9a-9098-c609069ae968"> |                                                                                                                |
| Main menu           | <img src="https://github.com/vorobalek/telegram-budget/assets/106157881/b291062a-6734-4a11-b87d-17344fc314aa">  |                                                                                                                |                                                                                                                |                                                                                                                |



## Docker Compose installation
```bash
git clone https://github.com/vorobalek/telegram-budget.git
cd telegram-budget
/bin/bash -c "$(curl -fsSL https://raw.githubusercontent.com/vorobalek/telegram-budget/main/deployment/install-docker-compose.sh)"
```

## Build
1. ```bash
   docker build -t <image nametag> .
   ```
2. ```bash 
   docker run \
     -d \
     -e 'TELEGRAM_BOT_TOKEN=<...>' \
     -e 'TELEGRAM_WEBHOOK_SECRET=<...>' \
     -e 'DB_CONNECTION_STRING=<...>' \
     -e 'PORT=<...>' \
     -e 'DOMAIN=<...>' \ 
     -e 'AUTHORIZED_USER_IDS=<...>' \
     -e 'LOCALE=<...>' \
     -e 'DATE_FORMAT=<...>' 
     -e 'DATETIME_FORMAT=<...>' \
     -e 'SENTRY_DSN=<...>' \
     -p <machine port>:<container port> \
   --restart always \
   --name <container name> \
   <image nametag>
   ```

## Environment variables (ex. is located [here](./Properties/launchSettings.json))

| Key                       | Description                                                                                                                                                                                                                                                        | Required | Secret | Default               |
|---------------------------|--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|----------|--------|-----------------------|
| `TELEGRAM_BOT_TOKEN`      | Telegram Bot API token.                                                                                                                                                                                                                                            | YES      | YES    |                       |
| `TELEGRAM_WEBHOOK_SECRET` | A secret token to be sent in a header `X-Telegram-Bot-Api-Secret-Token` in every webhook request, 1-256 characters. Only characters `A-Z`, `a-z`, `0-9`, `_` and `-` are allowed. The header is useful to ensure that the request comes from a webhook set by you. | YES      | YES    |                       |
| `DB_CONNECTION_STRING`    | The connection string of the PostgreSQL database to connect to.                                                                                                                                                                                                    | YES      | YES    |                       |
| `PORT`                    | The number of the port for receiving requests after SSL proxy if you have.                                                                                                                                                                                         | YES      | NO     | –                     |
| `DOMAIN`                  | The domain to send Telegram updates to. SSL is required. The `https://` prefix will be added.                                                                                                                                                                      | YES      | NO     | –                     |
| `AUTHORIZED_USER_IDS`     | A list of Telegram user IDs allowed to interact with the bot. List of numbers splitted by commas, spaces, or semicolons. Or `*` to authorize everyone.                                                                                                             | YES      | NO     | –                     |
| `LOCALE`                  | The locale for the text responses from the bot. Only `ru` and `en` are currently available.                                                                                                                                                                        | NO       | NO     | `en`                  |
| `DATETIME_FORMAT`         | The format for the date and time text representation. Ex.: `hh:mm tt MM/dd/yyyy` or `dd.MM.yyyy HH:mm`.                                                                                                                                                            | NO       | NO     | `hh:mm tt MM/dd/yyyy` |
| `SENTRY_DSN`              | The Data Source Name of a project in Sentry. Not configured by default.                                                                                                                                                                                            | NO       | YES    | –                     |


## Usage
After launching the bot, you can interact with it through Telegram. Use the following commands to manage your budget:

📌 `/start` - Main page

📌 `/help` - Show help

📌 `/list` - Get a list of available budgets

📌 `/me` - Get your user ID

📌 `/history <budget name>` - Get the transaction history for a budget. The budget name is optional. If no budget name is specified, the active budget will be selected.
Example: `/history` or `/history Vacation`

📌 `/create <budget name>` - Create a new budget. The new budget will automatically become active.
Example: `/create Vacation` or `/create Weekly Budget`

📌 `/switch <budget name>` - Switch the active budget. You can get a list of available budgets using the `/list` command.
Example: `/switch Vacation` or `/switch Weekly Budget`

📌 `/timezone <UTC offset in format 00:00>` - Set your time zone. You need to specify the UTC offset.
Example: `/timezone 03:00` or `/timezone -08:00`

📌 `/grant <user ID> <budget name>` - Grant access to a budget to another user. The budget name is optional. If no budget name is specified, the active budget will be selected.
Example: `/grant 1234567890` or `/grant 1234567890 Weekly Budget`

📌 `/revoke <user ID> <budget name>` - Revoke access to a budget from another user. The budget name is optional. If no budget name is specified, the active budget will be selected.
Example: `/revoke 1234567890` or `/revoke 1234567890 Weekly Budget`

📌 `/delete <budget name>` - Delete a budget. All transactions will be permanently deleted. The budget name is required.
Example: `/delete Vacation` or `/delete Weekly Budget`

### Interaction with the Bot through the Button Interface
You can interact with the bot through the button interface on the main page. Examples of the interfaces:

- **Main Page**:

  <img width="332" alt="Main Page" src="https://github.com/vorobalek/telegram-budget/assets/106157881/d21f8f8f-3a09-4b7d-b09c-6b0ce362acc9">
  
  (The "Share" and "Delete" buttons are still in development – use commands `/grant` and `/delete` for these functions)
  - The main page displays the latest transactions for the current day in the user\`s timezone. The timezone can be set using the `/timezone` command.

- **History**:

  <img width="332" alt="History Page" src="https://github.com/vorobalek/telegram-budget/assets/106157881/37ec3885-9f70-4c0e-b2cf-543dd50b5196">
  
  - Transactions in the history section are ordered from the most recent to the oldest. There is pagination.

- **Switch Budget**:

  <img width="332" alt="Switch Budget" src="https://github.com/vorobalek/telegram-budget/assets/106157881/91fc39f9-3e84-4235-be46-b09ffce02f9d">

  - In the switch budget menu, you can create a new budget. Follow the bot\`s instructions after clicking the "create new" button.

### Adding Transactions
Any message in the format: `<amount> <comment>` (e.g., `-10 for coffee`) will be recorded as a transaction for the specified amount with the given comment in your active budget. The comment is optional.

### Editing Transactions
Any transaction can be edited by modifying the sent message. In this case, all participants of the budget will receive a notification about the change. Editing the amount and comment is allowed. Only the user who created the transaction can edit it.
