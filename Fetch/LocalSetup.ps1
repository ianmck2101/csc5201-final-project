$networkName = "fetch_network"

# Remove the existing network
Write-Host "Removing existing Docker network: $networkName"
docker compose down

# Recreate the network
Write-Host "Creating Docker network: $networkName"
docker compose up -d --build