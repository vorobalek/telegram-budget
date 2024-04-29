#!/usr/bin/env bash

mask() {
  local __MASKED__
  __MASKED__="${1:0:1}$(printf '*%.0s' {1..8})${1:${#1}-1:1}"
  echo "$__MASKED__"
}

secure_input() {
  local __PASSWORD__=""
  local __CHAR_COUNT__=0
  local __PROMPT__=$1
  
  stty -echo
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
  
  stty echo
  echo "$__PASSWORD__"
}

prompt_domain() {
  read -rp "Enter domain: " DOMAIN
}

prompt_confirmation() {
  while true; do
    local __CONFIRMATION__
    read -rp "$1? (y/n): " __CONFIRMATION__
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
  echo
}

prompt_db_password_confirmation() {
  DB_PASSWORD_CONFIRMATION=$(secure_input "Reply database password: ")
  echo
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
    read -rp "Enter database connection string: " DB_CONNECTION_STRING
  fi
}

prompt_telegram_bot_token() {
  TELEGRAM_BOT_TOKEN=$(secure_input "Enter Telegram Bot token: ") 
  echo
}

prompt_telegram_webhook_secret() {
  TELEGRAM_WEBHOOK_SECRET=$(secure_input "Enter Telegram webhook secret: ")
  echo
}

prompt_authorized_user_ids() {
  read -rp "Enter authorized used ids: " AUTHORIZED_USER_IDS
}

prompt_locale() {
  while true; do
      read -rp "Enter locale (or press enter for default): (en/ru) " LOCALE
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
  read -rp "Enter Sentry DSN (or press enter to skip): " SENTRY_DSN
}

prompt_input() {
  prompt_domain
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
  echo "--------------------------------------------------------------------------------"
  echo "Your configuration:"
  echo "> Domain: $DOMAIN"
  echo "> Database settings:"

  if [[ $SETUP_DB = 1 ]]
  then
    echo ">>> [A new database will be set]"
    echo ">>> database: $DB_NAME"
    echo ">>> username: $DB_USER"
    echo ">>> password: $(mask "$DB_PASSWORD")"
    echo ">>> connection string: $DB_CONNECTION_STRING_MASKED"
  else
    echo ">>> [A new database won't be set]"
    echo ">>> connection string: $DB_CONNECTION_STRING"
  fi
  
  echo "> Telegram Bot token: $(mask "$TELEGRAM_BOT_TOKEN")"
  echo "> Telegram Bot webhook secret: $(mask "$TELEGRAM_WEBHOOK_SECRET")"
  echo "> Authorized user ids: $AUTHORIZED_USER_IDS"
  echo "> Locale: $LOCALE"
  echo "> Date/Time format: $DATETIME_FORMAT"
  echo "> Sentry DSN: $(mask "$SENTRY_DSN")"
}

unset_variables() {
  unset DOMAIN
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

generate_docker_compose() {
  echo "Generating docker-compose.yml..."
  
  echo "
  networks:
    telegram-budget-lan:
      driver: bridge" > docker-compose.yml
  
  [[ $SETUP_DB == 1 ]] && echo "
    postgres-lan:
      driver: bridge" >> docker-compose.yml
  
  echo "
  services:
  
    nginx:
      image: nginx:latest
      ports:
        - \"80:80\"
        - \"443:443\"
      restart: always
      volumes:
        - ./nginx/conf/:/etc/nginx/conf.d/:ro
        - ./certbot/www/:/var/www/certbot/:ro
      networks:
        - telegram-budget-lan
  
    certbot:
      image: certbot/certbot:latest
      volumes:
        - ./certbot/www/:/var/www/certbot/:rw
        - ./certbot/conf/:/etc/letsencrypt/:rw" > docker-compose.yml
  
  [[ $SETUP_DB == 1 ]] && echo "
    postgres:
      image: postgres:latest
      ports:
        - \"5432\"
      restart: always
      volumes:
        - postgresql_volume:/var/lib/postgresql/data
      environment:
        POSTGRES_DB: \"$DB_NAME\"
        POSTGRES_USER: \"$DB_USER\"
        POSTGRES_PASSWORD: \"$DB_PASSWORD\"
      networks:
        - postgres-lan" >> docker-compose.yml
  
  echo "
    telegram-budget:" >> docker-compose.yml
  
  [[ $SETUP_DB == 1 ]] && echo "
      depends_on:
        - \"postgres\""  >> docker-compose.yml
  
  echo "
      image: telegram-budget:latest
      build:
        context: .
        dockerfile: Dockerfile
      ports:
        - \"80\"
      restart: always
      environment:
        TELEGRAM_BOT_TOKEN: \"$TELEGRAM_BOT_TOKEN
        TELEGRAM_WEBHOOK_SECRET: \"$TELEGRAM_WEBHOOK_SECRET
        CONNECTION_STRING: \"$DB_CONNECTION_STRING\"
        PORT: \"80\"
        DOMAIN: \"$DOMAIN\"
        AUTHORIZED_USER_IDS: \"$AUTHORIZED_USER_IDS\"
        LOCALE: \"$LOCALE\"
        DATETIME_FORMAT: \"$DATETIME_FORMAT\"
        SENTRY_DSN: \"$SENTRY_DSN\"
      networks:
        - telegram-budget-lan"  >> docker-compose.yml
  
  [[ $SETUP_DB == 1 ]] && echo "
        - postgres-lan"  >> docker-compose.yml
  
  echo "
  volumes:"  >> docker-compose.yml
  
  [[ $SETUP_DB == 1 ]] && echo "
    postgresql_volume:" >> docker-compose.yml
}

generate_nginx_configuration() {
  echo "Generating nginx configuration..."
  
  mkdir -p "nginx/conf" || (echo "Unable to create nginx/conf folder" && exit)
  
  echo "
  server {
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
  }
  " > nginx/conf/certbot.conf
}

main() {
  prompt_input_double_checked
  generate_docker_compose
  generate_nginx_configuration
}

main || exit