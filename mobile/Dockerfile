# Static web build of the Expo app, served by nginx. A real release ships through app stores;
# this is here so the storefront has a one-command, clickable demo alongside the APIs.
FROM node:20-alpine AS build
WORKDIR /app
COPY package.json package-lock.json ./
RUN npm ci
COPY . .
# EXPO_PUBLIC_* variables are inlined into the bundle at build time, so the gateway's address
# must be the one the *browser* can reach, not a Docker-internal hostname.
ARG EXPO_PUBLIC_API_URL
ENV EXPO_PUBLIC_API_URL=${EXPO_PUBLIC_API_URL} \
    CI=1
RUN npx expo export --platform web

FROM nginx:1.27-alpine AS runtime
COPY --from=build /app/dist /usr/share/nginx/html
COPY nginx.conf /etc/nginx/conf.d/default.conf
EXPOSE 80
