# Token Revocation Implementation

## Problem

When you revoke a refresh token, the access token can still be used until it expires (60 minutes). This is because JWT tokens are stateless and validated independently.

## Solution Implemented

Added a **token blacklist** mechanism that stores revoked access tokens in the database and checks against it during authentication.

## Changes Made

### 1. New Entity: `RevokedToken`

Located: `src/Domain/Entities/RevokedToken.cs`

- Stores revoked access tokens with expiration time
- Includes revocation reason for audit purposes

### 2. Updated `ApplicationDbContext`

Added: `DbSet<RevokedToken> RevokedTokens`

### 3. Custom JWT Bearer Handler

Located: `src/Infrastructure/Authentication/CustomJwtBearerHandler.cs`

- Extends default JWT validation
- Checks if token exists in RevokedTokens table
- Rejects authentication if token is blacklisted

### 4. Updated `RevokeToken` Endpoint

Now stores BOTH:

- Refresh token revocation (existing)
- Access token in blacklist (new)

### 5. Database Configuration

Located: `src/Infrastructure/Data/Configurations/RevokedTokenConfiguration.cs`

- Proper indexes for fast token lookup
- Cleanup indexes for expired tokens

## Migration Steps

### Create Migration:

```powershell
# Navigate to project root
cd c:\Users\user\OneDrive\Documents\Immigration_Support_Platform\new_template_sample\MigratingAssistant

# Create migration
dotnet ef migrations add AddRevokedTokensTable -s src/Web -p src/Infrastructure

# Apply migration (if using Docker)
docker-compose down
docker-compose build --no-cache api
docker-compose up -d

# Or apply migration directly (if running locally)
dotnet ef database update -s src/Web -p src/Infrastructure
```

## How It Works Now

1. **Login**: User gets access token (valid 60 min) + refresh token (valid 7 days)
2. **API Calls**: Access token is checked against:
   - JWT signature ✓
   - Expiration ✓
   - Issuer/Audience ✓
   - **Blacklist** ✓ (NEW)
3. **Revoke**: Both tokens are invalidated:
   - Refresh token: marked as revoked in RefreshTokens table
   - Access token: added to RevokedTokens blacklist
4. **Subsequent API Calls**: Access token lookup finds it in blacklist → 401 Unauthorized

## Performance Considerations

### Optimization 1: Database Indexes

- Index on `Token` column for fast O(1) lookup
- Index on `ExpiresAt` for cleanup queries

### Optimization 2: Automatic Cleanup

Recommended: Add a background job to remove expired tokens from RevokedTokens table

```csharp
// Cleanup job (add to a hosted service)
var expiredTokens = await dbContext.RevokedTokens
    .Where(rt => rt.ExpiresAt < DateTime.UtcNow)
    .ToListAsync();

dbContext.RevokedTokens.RemoveRange(expiredTokens);
await dbContext.SaveChangesAsync();
```

### Optimization 3: Caching (Future Enhancement)

For high-traffic scenarios, cache revoked tokens in Redis/Memory Cache:

- Check cache first before database lookup
- Cache invalidation when new tokens are revoked
- TTL matches token expiration

## Testing

1. **Login** to get tokens
2. **Call API** with access token → should work ✓
3. **Revoke token**
4. **Call API again** with same access token → should get 401 Unauthorized ✓
5. **Try refresh** with revoked refresh token → should get 401 Unauthorized ✓

## Cleanup Strategy

Run this cleanup job daily/weekly:

```sql
DELETE FROM RevokedTokens WHERE ExpiresAt < NOW();
```

## Alternative Approaches Considered

1. **Short-lived tokens**: Reduce access token to 5-15 min

   - Pro: Less data in blacklist
   - Con: More frequent refresh calls

2. **Token versioning**: Add version number to user record

   - Pro: Revoke all user tokens at once
   - Con: Requires schema changes in User table

3. **Stateful sessions**: Store all tokens in database
   - Pro: Full control over sessions
   - Con: Loses JWT stateless benefits
