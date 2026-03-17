# Etapa 1 - Build
FROM node:22-alpine AS builder
WORKDIR /app

COPY package*.json ./
RUN npm ci --legacy-peer-deps

COPY . .

# Build de produção Angular
RUN node_modules/.bin/ng build --configuration production

# Etapa 2 - Runtime com Nginx
FROM nginx:alpine AS runner

# Remove config padrão do Nginx
RUN rm /etc/nginx/conf.d/default.conf

# Copia config customizada
COPY nginx.conf /etc/nginx/conf.d/app.conf

# Copia o build para o Nginx servir
COPY --from=builder /app/dist/*/browser /usr/share/nginx/html

EXPOSE 80

CMD ["nginx", "-g", "daemon off;"]
