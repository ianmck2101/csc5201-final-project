# Stage 1: Build the React app
FROM node:18-alpine AS build

# Set working directory
WORKDIR /app

# Install dependencies
COPY package.json package-lock.json ./
RUN npm install

# Copy the rest of the application code
COPY . .

# Build the React app
RUN npm run build

# Stage 2: Serve the app using a simpler HTTP server (e.g., serve)
FROM node:18-alpine

# Install the serve package globally
RUN npm install -g serve

# Set the working directory to the build output folder
WORKDIR /app

# Copy the build output from the build stage
COPY --from=build /app/dist /app

# Expose the port the app will run on
EXPOSE 5000

# Start the server to serve the React app
CMD ["serve", "-s", ".", "-l", "5000"]
