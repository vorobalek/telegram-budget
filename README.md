# Try it out

> [@BUDGET_DEMO_BOT](https://t.me/budget_demo_bot)

# Preview

| <img src="https://github.com/vorobalek/telegram-budget/assets/106157881/87c88562-3cc6-491f-86d9-429ac307e067" width="200" /> | <img src="https://github.com/vorobalek/telegram-budget/assets/106157881/73e20e62-90dc-438e-9174-91a7e97f88c3" width="200" /> | <img src="https://github.com/vorobalek/telegram-budget/assets/106157881/60cfd274-c615-4160-bdbe-71e78edbb162" width="200" /> | <img src="https://github.com/vorobalek/telegram-budget/assets/106157881/d5929080-df90-490f-b341-e346f682202b" width="200" /> | <img src="https://github.com/vorobalek/telegram-budget/assets/106157881/93453324-5fc4-4279-b34a-7bbb4afc44ad" width="200" /> | <img src="https://github.com/vorobalek/telegram-budget/assets/106157881/97b8a97d-5ef5-4302-a185-c3bbe1911a32" width="200" /> |
|------------------------------------------------------------------------------------------------------------------------------|------------------------------------------------------------------------------------------------------------------------------|------------------------------------------------------------------------------------------------------------------------------|------------------------------------------------------------------------------------------------------------------------------|------------------------------------------------------------------------------------------------------------------------------|------------------------------------------------------------------------------------------------------------------------------|

# Docker Compose installation

> docker and docker compose required

1. Run
   ```shell
   /bin/bash -c "$(curl -fsSL https://raw.githubusercontent.com/vorobalek/telegram-budget/main/deployment/install-docker-compose.sh)"
   ```
2. Follow the installation script instruction.

# Build

1. ```shell
   docker build -t <image nametag> .
   ```
2. ```shell 
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

# Environment variables (ex. is located [here](./Properties/launchSettings.json))

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
