# Stage 1: Build the React application
FROM node:22-alpine AS build
WORKDIR /app

# Copy package files first for layer caching
COPY package.json package-lock.json ./

# Install dependencies (ci for deterministic builds)
RUN npm ci

# Copy source code
COPY . .

# Build the production bundle
RUN npm run build

# Stage 2: Serve with nginx (tiny image, ~25MB)
FROM nginx:1.27-alpine AS runtime

# Remove default nginx config
RUN rm /etc/nginx/conf.d/default.conf

# Copy custom nginx config
COPY nginx.conf /etc/nginx/conf.d/default.conf

# Copy built static files from build stage
COPY --from=build /app/dist /usr/share/nginx/html

# Expose port 80
EXPOSE 80

# Health check
HEALTHCHECK --interval=30s --timeout=5s --start-period=5s --retries=3 \
    CMD wget -qO- http://localhost:80/ || exit 1

# nginx runs as root by default to bind to port 80, then drops to nginx user
CMD ["nginx", "-g", "daemon off;"]