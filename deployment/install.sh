#!/bin/bash

set -e

cleanup() {
  docker compose -f ./"$NOW"/docker-compose.yml down -v
  docker compose -f ./"$NOW"/docker-compose.yml rm
  rm -rf ./"$NOW"
}

handle_error() {
  printf " \033[31m %s \n\033[0m" "Something went wrong. Unable to proceed. Status: $?" >&2

  printf " \033[31m %s \n\033[0m" "Cleaning up..."
  cleanup
  printf " \033[31m %s \n\033[0m" "Done."

  exit 1
}

trap 'handle_error' ERR SIGINT SIGQUIT SIGTSTP
NOW=$(date +%Y%m%d%H%M)
mkdir ./"$NOW"

mask() {
  local __MASKED__
  __MASKED__="${1:0:1}$(printf '*%.0s' {1..8})${1:${#1}-1:1}"
  /bin/echo "$__MASKED__"
}

secure_input() {
  local __PASSWORD__=""
  local __CHAR_COUNT__=0
  local __PROMPT__=$1

  while IFS= read -p "$__PROMPT__" -r -s -n 1 __CHAR__
  do
    if [[ $__CHAR__ == $'\0' ]] ; then
      break
    fi
    if [[ $__CHAR__ == $'\177' ]] ; then
      if [ $__CHAR_COUNT__ -gt 0 ] ; then
        __CHAR_COUNT__=$((__CHAR_COUNT__-1))
        __PROMPT__=$'\b \b'
        __PASSWORD__="${__PASSWORD__%?}"
      else
        __PROMPT__=''
      fi
    else
      __CHAR_COUNT__=$((__CHAR_COUNT__+1))
      __PROMPT__='*'
      __PASSWORD__+="$__CHAR__"
    fi
  done

  /bin/echo "$__PASSWORD__"
}

prompt_domain() {
  read -rp "Enter domain (The folder with the same name will be created): " DOMAIN
}

prompt_number_of_replicas() {
  local __IS_NUMBER_REGEX__
  __IS_NUMBER_REGEX__='(^[0-9]+$)|(^$)'
  while true; do
    read -rp "Enter number of replicas (1 by default): " NUMBER_OF_REPLICAS
    if [[ $NUMBER_OF_REPLICAS =~ $__IS_NUMBER_REGEX__ ]]
    then
      if [[ $NUMBER_OF_REPLICAS -eq 0 ]]
      then
        NUMBER_OF_REPLICAS=1
        printf " \033[31m %s \n\033[0m" "value '1' has been set"
      fi
      return 0
    else
      printf " \033[31m %s \n\033[0m" "invalid input"
    fi
  done
}

prompt_confirmation() {
  while true; do
    local __CONFIRMATION__
    read -rp "$1? [y/n]: " __CONFIRMATION__
    case $__CONFIRMATION__ in
      [yY]) return 0 ;;
      [nN]) return 1 ;;
      *) printf " \033[31m %s \n\033[0m" "invalid input"
    esac
  done
}

prompt_setup_db() {
  if prompt_confirmation "Setup database"
  then
    SETUP_DB=1
    return 0
  else
    SETUP_DB=0
    return 1
  fi
}

prompt_db_name() {
  read -rp "Enter database name: " DB_NAME
}

prompt_db_user() {
  read -rp "Enter database user: " DB_USER
}

prompt_db_password() {
  DB_PASSWORD=$(secure_input "Enter database password: ")
  /bin/echo
}

prompt_db_password_confirmation() {
  DB_PASSWORD_CONFIRMATION=$(secure_input "Reply database password: ")
  /bin/echo
}

prompt_db_password_double_checked() {
  while true; do
    prompt_db_password && prompt_db_password_confirmation
    [ "$DB_PASSWORD" = "$DB_PASSWORD_CONFIRMATION" ] && break;
    printf " \033[31m %s \n\033[0m" "passwords don't match, try again"
  done
}

prompt_db_connection_string() {
  if [[ $SETUP_DB == 1 ]]
  then
    DB_CONNECTION_STRING="Server=postgres;Port=5432;Database=$DB_NAME;User Id=$DB_USER;Password=$DB_PASSWORD"
    DB_CONNECTION_STRING_MASKED="Server=postgres;Port=5432;Database=$DB_NAME;User Id=$DB_USER;Password=$(mask "$DB_PASSWORD")"
  else
    DB_CONNECTION_STRING=$(secure_input "Enter database connection string: ") 
    DB_CONNECTION_STRING_MASKED=$(mask DB_CONNECTION_STRING)
  fi
}

prompt_telegram_bot_token() {
  TELEGRAM_BOT_TOKEN=$(secure_input "Enter Telegram Bot token: ")
  /bin/echo
}

prompt_telegram_webhook_secret() {
  TELEGRAM_WEBHOOK_SECRET=$(secure_input "Enter Telegram webhook secret: ")
  /bin/echo
}

prompt_authorized_user_ids() {
  local __IS_NUMBER_SET_REGEX__
  __IS_NUMBER_SET_REGEX__='(^[0-9,; ]+$)|(^\*$)'
  while true; do
    read -rp "Enter authorized used ids (only digits, comma, and semicolon allowed. Or '*' to allow public access): " AUTHORIZED_USER_IDS
    if [[ $AUTHORIZED_USER_IDS =~ $__IS_NUMBER_SET_REGEX__ ]]
    then
      return 0
    else
      printf " \033[31m %s \n\033[0m" "invalid input"
    fi
  done
}

prompt_locale() {
  while true; do
    read -rp "Enter locale (or press enter for default): [en/ru] " LOCALE
    case $LOCALE in
      "en") return 0 ;;
      "ru") return 0 ;;
      "") LOCALE="en" && return 0 ;;
      *) printf " \033[31m %s \n\033[0m" "invalid input"
    esac
  done
}

prompt_datetime_format() {
  read -rp "Enter date/time format (or press enter for default): " DATETIME_FORMAT
  if [[ $DATETIME_FORMAT == "" ]]
  then
    DATETIME_FORMAT="hh:mm tt MM/dd/yyyy"
  fi
}

prompt_sentry_dsn() {
  SENTRY_DSN=$(secure_input "Enter Sentry DSN (or press enter to skip): ")
}

prompt_input() {
  prompt_domain
  prompt_number_of_replicas
  if prompt_setup_db
  then
    prompt_db_name
    prompt_db_user
    prompt_db_password_double_checked
  fi
  prompt_db_connection_string
  prompt_telegram_bot_token
  prompt_telegram_webhook_secret
  prompt_authorized_user_ids
  prompt_locale
  prompt_datetime_format
  prompt_sentry_dsn
}

print_configuration() {
  /bin/echo "--------------------------------------------------------------------------------
Your configuration:
> Domain: $DOMAIN
> Number of replicas: $NUMBER_OF_REPLICAS
> Database settings:"

  if [[ $SETUP_DB = 1 ]]
  then
    /bin/echo ">>> [A new database will be set]
>>> database: $DB_NAME
>>> username: $DB_USER
>>> password: $(mask "$DB_PASSWORD")
>>> connection string: $DB_CONNECTION_STRING_MASKED"
  else
    /bin/echo ">>> [A new database won't be set]
>>> connection string: $DB_CONNECTION_STRING_MASKED"
  fi

  /bin/echo "> Telegram Bot token: $(mask "$TELEGRAM_BOT_TOKEN")
> Telegram Bot webhook secret: $(mask "$TELEGRAM_WEBHOOK_SECRET")
> Authorized user ids: $AUTHORIZED_USER_IDS
> Locale: $LOCALE
> Date/Time format: $DATETIME_FORMAT
> Sentry DSN: $(mask "$SENTRY_DSN")"
}

unset_variables() {
  unset DOMAIN
  unset NUMBER_OF_REPLICAS
  unset SETUP_DB
  unset DB_NAME
  unset DB_USER
  unset DB_PASSWORD
  unset DB_PASSWORD_CONFIRMATION
  unset DB_CONNECTION_STRING
  unset DB_CONNECTION_STRING_MASKED
  unset TELEGRAM_BOT_TOKEN
  unset TELEGRAM_WEBHOOK_SECRET
  unset AUTHORIZED_USER_IDS
  unset LOCALE
  unset DATETIME_FORMAT
  unset SENTRY_DSN
}

prompt_input_double_checked() {
  while true; do
    unset_variables
    prompt_input
    print_configuration
    if prompt_confirmation "Is everything correct"
    then
      break;
    fi
  done
}

generate_postgres_environment_file() {
  /bin/echo "Generating postgres environment files..."

  [[ $SETUP_DB != 1 ]] && /bin/echo "Skipped."

  /bin/echo -n "POSTGRES_DB=$DB_NAME
POSTGRES_USER='$DB_USER'
POSTGRES_PASSWORD='$DB_PASSWORD'" > ./"$NOW"/postgres.env

  /bin/echo "Done."
}

generate_backend_environment_file() {
  /bin/echo "Generating backend environment files..."

  /bin/echo -n "DOMAIN='$DOMAIN'
DB_CONNECTION_STRING='$DB_CONNECTION_STRING'
TELEGRAM_BOT_TOKEN='$TELEGRAM_BOT_TOKEN'
TELEGRAM_WEBHOOK_SECRET='$TELEGRAM_WEBHOOK_SECRET'
AUTHORIZED_USER_IDS='$AUTHORIZED_USER_IDS'
LOCALE='$LOCALE'
DATETIME_FORMAT='$DATETIME_FORMAT'
SENTRY_DSN='$SENTRY_DSN'" > ./"$NOW"/backend.env

  /bin/echo "Done."
}

generate_docker_compose() {
  /bin/echo "Generating docker-compose.yml..."

  /bin/echo -n "networks:
  backend-lan:
    driver: bridge" > ./"$NOW"/docker-compose.yml

  [[ $SETUP_DB == 1 ]] && /bin/echo -n "
  postgres-lan:
    driver: bridge" >> ./"$NOW"/docker-compose.yml

  /bin/echo >> ./"$NOW"/docker-compose.yml

  /bin/echo -n "
services:
  nginx:
    depends_on:" >> ./"$NOW"/docker-compose.yml

  REPLICA_NUMBER=1
  while [ "$REPLICA_NUMBER" -le $NUMBER_OF_REPLICAS ]; do
    /bin/echo -n "
    - \"backend$REPLICA_NUMBER\"" >> ./"$NOW"/docker-compose.yml
    REPLICA_NUMBER=$(( REPLICA_NUMBER + 1 ))
  done

  /bin/echo -n "
    image: nginx:latest
    ports:
      - \"80:80\"
      - \"443:443\"
    restart: always
    volumes:
      - ./nginx/conf/:/etc/nginx/conf.d/:ro
      - ./certbot/conf/:/etc/nginx/ssl/:ro
      - ./certbot/www/:/var/www/certbot/:ro
    networks:
      - backend-lan

  certbot:
    depends_on:
      - \"nginx\"
    image: certbot/certbot:latest
    volumes:
      - ./certbot/www/:/var/www/certbot/:rw
      - ./certbot/conf/:/etc/letsencrypt/:rw" >> ./"$NOW"/docker-compose.yml

  /bin/echo >> ./"$NOW"/docker-compose.yml

  [[ $SETUP_DB == 1 ]] && /bin/echo -n "
  postgres:
    image: postgres:latest
    ports:
      - \"5432\"
    restart: always
    volumes:
      - postgresql_volume:/var/lib/postgresql/data
    env_file:
      - path: postgres.env
        required: true
    networks:
      - postgres-lan" >> ./"$NOW"/docker-compose.yml && /bin/echo >> ./"$NOW"/docker-compose.yml

  REPLICA_NUMBER=1
  while [ "$REPLICA_NUMBER" -le $NUMBER_OF_REPLICAS ]; do
    /bin/echo -n "
  backend$REPLICA_NUMBER:" >> ./"$NOW"/docker-compose.yml

    [[ $SETUP_DB == 1 ]] && /bin/echo -n "
    depends_on:
      - \"postgres\""  >> ./"$NOW"/docker-compose.yml

    /bin/echo -n "
    image: vorobalek/telegram-budget:latest
    ports:
      - \"80\"
    restart: always
    env_file:
      - path: backend.env
        required: true
    environment:
      PORT: \"80\"
    networks:
      - backend-lan"  >> ./"$NOW"/docker-compose.yml

    [[ $SETUP_DB == 1 ]] && /bin/echo -n "
      - postgres-lan"  >> ./"$NOW"/docker-compose.yml

    /bin/echo >> ./"$NOW"/docker-compose.yml
    REPLICA_NUMBER=$(( REPLICA_NUMBER + 1 ))
  done

  [[ $SETUP_DB == 1 ]] && /bin/echo -n "
volumes:
  postgresql_volume:" >> ./"$NOW"/docker-compose.yml

  /bin/echo "Done."
}

generate_nginx_configuration() {
  /bin/echo "Generating nginx configuration..."

  mkdir -p "./$NOW/nginx/conf" || (/bin/echo "Unable to create ./$NOW/nginx/conf folder" && exit)

  /bin/echo -n "server {
  listen 80;
  listen [::]:80;
  server_name $DOMAIN;
  server_tokens off;
  location /.well-known/acme-challenge/ {
    root /var/www/certbot;
  }
  location / {
    return 301 https://$DOMAIN\$request_uri;
  }
}" > ./"$NOW"/nginx/conf/"$DOMAIN".conf

  /bin/echo "Done."
}

generate_certificates() {
  /bin/echo "Issuing certificates..."

  docker compose -f ./"$NOW"/docker-compose.yml run --rm certbot certonly --webroot --webroot-path /var/www/certbot/ --dry-run -d "$DOMAIN" || handle_error
  docker compose -f ./"$NOW"/docker-compose.yml run --rm certbot certonly --webroot --webroot-path /var/www/certbot/ -d "$DOMAIN" || handle_error

  /bin/echo "Done."
}

update_nginx_configuration() {
  /bin/echo "Updating nginx configuration..."

  /bin/echo -n "

upstream backends {" >> ./"$NOW"/nginx/conf/"$DOMAIN".conf

  REPLICA_NUMBER=1
  while [ "$REPLICA_NUMBER" -le $NUMBER_OF_REPLICAS ]; do
    /bin/echo -n "
  server backend$REPLICA_NUMBER:80;" >> ./"$NOW"/nginx/conf/"$DOMAIN".conf
    REPLICA_NUMBER=$(( REPLICA_NUMBER + 1 ))
  done

  /bin/echo "
}" >> ./"$NOW"/nginx/conf/"$DOMAIN".conf

  /bin/echo -n "
server {
  listen 443 default_server ssl http2;
  listen [::]:443 ssl http2;
  server_name $DOMAIN;
  ssl_certificate /etc/nginx/ssl/live/$DOMAIN/fullchain.pem;
  ssl_certificate_key /etc/nginx/ssl/live/$DOMAIN/privkey.pem;
  location /health {
    proxy_pass http://backends;
  }
  location /bot {
    proxy_pass http://backends;
  }
  location / {
    return 302 https://youtu.be/dQw4w9WgXcQ?si=YgqFHwW3_gusiTFf&t=43;

    add_header Content-Type \"application/json\";
    add_header Location \"https://youtu.be/dQw4w9WgXcQ?si=YgqFHwW3_gusiTFf&t=43\";
    add_header Reason \"You've been rick-rolled.\";

    default_type application/json;
    return 302 \"{\\\"message\\\": \\\"You've been rick-rolled.\\\"}\";
  }
}" >> ./"$NOW"/nginx/conf/"$DOMAIN".conf

  /bin/echo "Done."
}

copy_installed() {
  cp -r ./"$NOW"/* ./"$DOMAIN"
}

main() {
  prompt_input_double_checked
  generate_backend_environment_file
  generate_postgres_environment_file
  generate_docker_compose
  generate_nginx_configuration
  generate_certificates
  update_nginx_configuration
  copy_installed
  cleanup
}

main || exit