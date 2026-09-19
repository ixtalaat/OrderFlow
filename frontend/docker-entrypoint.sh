#!/bin/sh
# Renders runtime config from the environment so one image serves any backend.
API_URL="${API_URL:-http://localhost:8080}"
printf '{"apiUrl": "%s"}\n' "$API_URL" > /usr/share/nginx/html/config.json
exec nginx -g 'daemon off;'
