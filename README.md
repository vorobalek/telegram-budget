# Try it out
> [@BUDGET_DEMO_BOT](https://t.me/budget_demo_bot)

# How to run

1. ```shell
   docker build -t <image nametag> .
   ```
2. ```shell 
   docker run \
     -d \
     -e 'TELEGRAM_BOT_TOKEN=<...>' \
     -e 'TELEGRAM_WEBHOOK_SECRET=<...>' \
     -e 'CONNECTION_STRING=<...>' \
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
| `CONNECTION_STRING`       | The connection string of the PostgreSQL database to connect to.                                                                                                                                                                                                    | YES      | YES    |                       |
| `PORT`                    | The number of the port for receiving requests after SSL proxy if you have.                                                                                                                                                                                         | YES      | NO     | –                     |
| `DOMAIN`                  | The domain to send Telegram updates to. SSL is required. The `https://` prefix will be added.                                                                                                                                                                      | YES      | NO     | –                     |
| `AUTHORIZED_USER_IDS`     | A list of Telegram user IDs allowed to interact with the bot. List of numbers splitted by commas, spaces, or semicolons. Or `*` to authorize everyone.                                                                                                             | YES      | NO     | –                     |
| `LOCALE`                  | The locale for the text responses from the bot. Only `ru` and `en` are currently available.                                                                                                                                                                        | NO       | NO     | `en`                  |
| `DATETIME_FORMAT`         | The format for the date and time text representation. Ex.: `hh:mm tt MM/dd/yyyy` or `dd.MM.yyyy HH:mm`.                                                                                                                                                            | NO       | NO     | `hh:mm tt MM/dd/yyyy` |
| `SENTRY_DSN`              | The Data Source Name of a project in Sentry. Not configured by default.                                                                                                                                                                                            | NO       | YES    | –                     |