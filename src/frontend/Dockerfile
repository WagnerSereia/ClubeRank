# Build stage
FROM node:18-alpine AS build
WORKDIR /app

# Copy package files
COPY package*.json ./
RUN npm pkg delete devDependencies.node
RUN npm ci

# Copy source code
COPY . .

# Build the application
RUN node ./node_modules/@angular/cli/bin/ng.js build --configuration production

# Runtime stage
FROM nginx:alpine
COPY --from=build /app/dist/clube-rank-ui/browser /usr/share/nginx/html
COPY nginx.conf /etc/nginx/nginx.conf

EXPOSE 80
