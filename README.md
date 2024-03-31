# Environment variables (ex. is located [here](./Properties/launchSettings.json))
## Required
### Can be kept as not secret
- `PORT` – The number of the port for receiving requests after SSL proxy if you have.
- `DOMAIN` – The domain to send Telegram updates to. SSL is required. The `https://` prefix will be added.
- `TELEGRAM_BOT_AUTHORIZED_USER_IDS` - A list of Telegram user IDs allowed to interact with the bot.
### Recommended to be kept as a secret
- `TELEGRAM_BOT_TOKEN` – Telegram Bot API token.
- `TELEGRAM_BOT_WEBHOOK_SECRET_TOKEN` – A secret token to be sent in a header `X-Telegram-Bot-Api-Secret-Token` in every webhook request, 1-256 characters. Only characters `A-Z`, `a-z`, `0-9`, `_` and `-` are allowed. The header is useful to ensure that the request comes from a webhook set by you.
- `CONNECTION_STRING` – The connection string of the PostgreSQL database to connect to.
## Optional
### Can be kept as not secret
- `LOCALE` – The locale for the text responses from the bot. Only `ru` and `en` are currently available. `ru` by default.
- `DATE_FORMAT` – The format for the date text representation. Ex.: `MM/dd/yyyy` or `dd.MM.yyyy`. `dd.MM.yyyy` by default.
- `DATETIME_FORMAT` – The format for the date and time text representation. Ex.: `HH:mm MM/dd/yyyy` or `dd.MM.yyyy HH:mm`. `dd.MM.yyyy HH:mm` by default.
### Recommended to be kept as a secret
- `SENTRY_DSN` – The Data Source Name of a project in Sentry. Not configured by default.