FROM node:18-alpine as build

WORKDIR /usr/src/app
COPY package.json package-lock.json ./

RUN npm install
COPY . .
RUN npm run build

FROM nginx:latest
COPY nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=build /usr/src/app/dist /html

EXPOSE 9050

# CMD ["nginx", "-g", "daemon off;"]