# Quick Start - CircleCI → Digital Ocean Deployment

## Setup in 3 Steps

### Step 1: Add Environment Variables to CircleCI (5 minutes)

Go to: **https://app.circleci.com/** → Your project → **Project Settings** → **Environment Variables**

Add these 3 variables:

```
Name: DO_ACCESS_TOKEN
Value: your_digitalocean_access_token_here

Name: DO_REGISTRY_NAME
Value: migratingassistant

Name: DO_APP_ID
Value: (get this in step 2 - can add later)
```

---

### Step 2: Create Digital Ocean App (5 minutes)

**Option A: Web Console (Easiest)**

1. Go to: **https://cloud.digitalocean.com/apps**
2. Click **Create App**
3. Select **Container Registry**
4. Choose registry: **migratingassistant**
5. Choose repository: **migratingassistant-api**
6. Click **Next**
7. Configure:
   - Name: `migratingassistant-api-staging`
   - Region: NYC3 (or closest to you)
   - Plan: Basic ($5/month)
8. Add these environment variables (in app settings):
   ```
   ASPNETCORE_ENVIRONMENT=Production
   JwtSettings__Secret=<your-secret-32-chars-min>
   JwtSettings__Issuer=MigratingAssistant
   JwtSettings__Audience=MigratingAssistantUsers
   ConnectionStrings__DefaultConnection=<your-mysql-connection>
   ```
9. Click **Create Resources**
10. **Copy the App ID** from the URL or app details page

**Option B: Using CLI**

```bash
# Install doctl (skip if already installed)
# Windows: choco install doctl
# Mac: brew install doctl

# Authenticate
doctl auth init --access-token your_digitalocean_access_token_here

# List apps (if you already have one)
doctl apps list

# The App ID is in the first column
```

---

### Step 3: Add App ID & Deploy (2 minutes)

1. Go back to CircleCI → Environment Variables
2. Update `DO_APP_ID` with your App ID from Step 2
3. Push to dev branch:

```bash
git checkout dev
git add .
git commit -m "Setup CircleCI deployment to Digital Ocean"
git push origin dev
```

**Watch it deploy!**
- CircleCI: https://app.circleci.com/
- Digital Ocean: https://cloud.digitalocean.com/apps

---

## What Gets Deployed

**Your Images:**
```
registry.digitalocean.com/migratingassistant/migratingassistant-api:latest
registry.digitalocean.com/migratingassistant/migratingassistant-api:<commit-sha>
registry.digitalocean.com/migratingassistant/migratingassistant-api:dev
```

**Automatic Deployments:**
- Push to `dev` → Auto-deploy to staging
- Push to `master` → Wait for approval → Deploy to production

---

## Verify Deployment

```bash
# Check app status
doctl apps list

# View logs
doctl apps logs <your-app-id> --follow

# Test API
curl https://your-app.ondigitalocean.app/health
```

---

## Troubleshooting

### CircleCI Build Fails

**"DO_ACCESS_TOKEN not set"**
→ Add it to CircleCI Project Settings → Environment Variables

**"DO_REGISTRY_NAME not set"**
→ Add it to CircleCI: Value must be `migratingassistant`

**"DO_APP_ID not set"**
→ Create app first, then add App ID to CircleCI

### Docker Push Fails

**"Authentication failed"**
```bash
# Test authentication locally
doctl auth init --access-token your_digitalocean_access_token_here
doctl registry login
```

### Deployment Fails

**Check logs:**
```bash
doctl apps logs <app-id> --type=deploy --follow
```

**Common issues:**
- Image not found → Verify push was successful in CircleCI
- Health check failing → Verify `/health` endpoint works
- Environment variables missing → Add in Digital Ocean app settings

---

## Your Configuration

- **Registry**: `registry.digitalocean.com/migratingassistant`
- **Account**: `achalakavinda95@gmail.com`
- **Access Token**: Stored securely in CircleCI environment variables

---

## URLs to Bookmark

- **CircleCI Dashboard**: https://app.circleci.com/
- **Digital Ocean Apps**: https://cloud.digitalocean.com/apps
- **Digital Ocean Registry**: https://cloud.digitalocean.com/registry
- **Digital Ocean API Tokens**: https://cloud.digitalocean.com/account/api/tokens

---

## Costs

- **Container Registry**: $5/month (basic)
- **App Platform**: $5/month per app (basic)
- **CircleCI**: Free tier (6,000 build minutes/month)

**Total**: ~$10-15/month for staging + production

---

## Security Reminder

⚠️ Your access token is visible in this file and in CircleCI.

**After setup, consider:**
1. Rotating the token (create new, delete old)
2. Using separate tokens for staging/production
3. Never committing tokens to git

Rotate at: https://cloud.digitalocean.com/account/api/tokens

---

## Next Steps

✅ 1. Add 3 environment variables to CircleCI
✅ 2. Create Digital Ocean app
✅ 3. Push to dev branch to deploy
✅ 4. Verify deployment works
✅ 5. Set up production app (repeat step 2 for master branch)

**Ready to deploy!** 🚀
