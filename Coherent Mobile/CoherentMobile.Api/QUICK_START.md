# Quick Start Guide - Coherent Mobile Health API

## ⚡ Get Started in 5 Minutes

### 1. Prerequisites Check

Ensure you have installed:
- [x] .NET 8 SDK
- [x] SQL Server (or SQL Server Express)
- [x] Your favorite IDE (VS 2022, VS Code, or Rider)

### 2. Setup Database

Open SQL Server Management Studio (SSMS) or Azure Data Studio and run:

```bash
# Navigate to database folder
cd "c:\Users\DELL\Desktop\Coheret\Coherent Mobile\Coherent Mobile\Database"

# Execute the SQL script
# Open CreateDatabase.sql in SSMS and execute it
```

Or use command line:
```bash
sqlcmd -S localhost -i Database/CreateDatabase.sql
```

### 3. Update Configuration

Edit `src/CoherentMobile.API/appsettings.json`:

**Update your connection string:**
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost;Database=CoherentHealthDb;User Id=sa;Password=YourPassword;TrustServerCertificate=True;"
}
```

**Update JWT secret (for production):**
```json
"Jwt": {
  "Secret": "YourSuperSecretKeyForJWTTokenGeneration32CharactersMinimum!!"
}
```

### 4. Restore and Build

```bash
# Navigate to solution folder
cd "c:\Users\DELL\Desktop\Coheret\Coherent Mobile\Coherent Mobile"

# Restore packages
dotnet restore CoherentMobile.sln

# Build solution
dotnet build CoherentMobile.sln
```

### 5. Run the Application

```bash
# Navigate to API project
cd src/CoherentMobile.API

# Run the application
dotnet run
```

The API will start on:
- HTTPS: `https://localhost:5001`
- HTTP: `http://localhost:5000`

### 6. Access Swagger UI

Open your browser and go to:
```
https://localhost:5001
```

You should see the Swagger UI with all available endpoints!

## 🧪 Test the API

### Register a New User

In Swagger UI or using curl:

```bash
curl -X POST https://localhost:5001/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "email": "test@example.com",
    "password": "Test@1234",
    "firstName": "Test",
    "lastName": "User",
    "phoneNumber": "+1234567890",
    "gender": "Male"
  }'
```

### Login

```bash
curl -X POST https://localhost:5001/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "test@example.com",
    "password": "Test@1234"
  }'
```

Copy the `token` from the response.

### Get User Profile (Authenticated)

```bash
curl -X GET https://localhost:5001/api/user/profile \
  -H "Authorization: Bearer YOUR_TOKEN_HERE"
```

### Create Health Record

```bash
curl -X POST https://localhost:5001/api/healthrecord \
  -H "Authorization: Bearer YOUR_TOKEN_HERE" \
  -H "Content-Type: application/json" \
  -d '{
    "recordType": "BloodPressure",
    "value": "120/80",
    "unit": "mmHg",
    "recordedAt": "2024-12-01T10:00:00Z",
    "notes": "Morning reading"
  }'
```

## 🔐 Using Swagger with Authentication

1. Click **"Authorize"** button in Swagger UI
2. Enter: `Bearer YOUR_TOKEN_HERE` (replace with actual token from login)
3. Click **"Authorize"**
4. Now you can test protected endpoints!

## 📱 Test SignalR Hub

### JavaScript Client

```html
<!DOCTYPE html>
<html>
<head>
    <script src="https://cdn.jsdelivr.net/npm/@microsoft/signalr@latest/dist/browser/signalr.min.js"></script>
</head>
<body>
    <script>
        const token = "YOUR_JWT_TOKEN_HERE";
        
        const connection = new signalR.HubConnectionBuilder()
            .withUrl("https://localhost:5001/hubs/healthdata", {
                accessTokenFactory: () => token
            })
            .build();

        connection.on("ReceiveHealthDataUpdate", (data) => {
            console.log("Health Update:", data);
        });

        connection.on("ReceiveHealthAlert", (alert) => {
            console.log("Alert:", alert);
        });

        connection.start()
            .then(() => console.log("Connected to SignalR Hub"))
            .catch(err => console.error(err));
    </script>
</body>
</html>
```

## 🛠️ Common Issues

### Issue: Connection to SQL Server failed

**Solution**: Check your connection string and ensure SQL Server is running.

```bash
# Check SQL Server service
net start MSSQLSERVER
```

### Issue: Port already in use

**Solution**: Change the port in `launchSettings.json` or stop the process using that port.

### Issue: JWT token validation fails

**Solution**: Ensure the JWT secret is the same in both token generation and validation. Check `appsettings.json`.

### Issue: CORS error in browser

**Solution**: Add your frontend URL to the allowed origins in `appsettings.json`:

```json
"Cors": {
  "AllowedOrigins": [
    "http://localhost:3000",
    "YOUR_FRONTEND_URL"
  ]
}
```

## 📊 Next Steps

- ✅ Explore all endpoints in Swagger
- ✅ Create more users and health records
- ✅ Test SignalR real-time features
- ✅ Review the architecture documentation
- ✅ Customize entities and add features
- ✅ Set up your mobile app to consume the API
- ✅ Configure external API integrations

## 🚀 Production Deployment

Before deploying to production:

1. **Security**:
   - [ ] Generate strong JWT secret (minimum 32 characters)
   - [ ] Enable HTTPS only
   - [ ] Use secure password hashing (BCrypt)
   - [ ] Update CORS to specific domains only

2. **Database**:
   - [ ] Use production connection string
   - [ ] Enable connection pooling
   - [ ] Set up database backups

3. **Logging**:
   - [ ] Configure production logging (Application Insights, etc.)
   - [ ] Set appropriate log levels
   - [ ] Configure log retention

4. **Configuration**:
   - [ ] Use environment variables for secrets
   - [ ] Enable health checks
   - [ ] Configure monitoring

## 📚 Documentation

- **README.md** - Complete project documentation
- **ARCHITECTURE.md** - Detailed architecture explanation
- **Database/CreateDatabase.sql** - Database schema

## 💡 Tips

- Use **Postman** to save and organize your API requests
- Check the `logs/` folder for application logs
- Use **SQL Server Profiler** to debug database queries
- Enable detailed errors in Development environment only

---

**You're all set! Happy coding! 🎉**
