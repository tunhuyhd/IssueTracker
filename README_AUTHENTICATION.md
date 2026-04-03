# 📚 Complete Documentation Index - IssueTracker Authentication

## 🎯 Dành Cho Frontend Developers

### 📖 Documentation Files

| File | Description | Use Case |
|------|-------------|----------|
| **FRONTEND_QUICK_REFERENCE.md** ⭐ | Copy-paste code snippets | Quick implementation |
| **FRONTEND_INTEGRATION_GUIDE.md** | Complete React examples | Full integration guide |
| **API_DOCUMENTATION.md** | All API endpoints details | API reference |

### 🛠️ Tools & Helpers

| File | Description | How to Use |
|------|-------------|------------|
| **swagger-google-oauth-tester.html** | Get Google ID Token | Open in browser → Sign in |
| **test-auth.html** | Test all auth flows | Open in browser → Test features |
| **IssueTracker-Auth-API.postman_collection.json** | Postman collection | Import into Postman |

---

## 🎯 Dành Cho Backend Developers / Testers

### 📖 Documentation Files

| File | Description | Use Case |
|------|-------------|----------|
| **TEST_SWAGGER_GOOGLE_OAUTH.md** | Swagger testing guide | Test with Swagger UI |
| **SWAGGER_QUICK_START.md** | 3-step quick test | Fast testing |
| **GOOGLE_OAUTH_SETUP.md** | Google Cloud setup | Configure Google OAuth |
| **GOOGLE_OAUTH_IMPLEMENTATION.md** | Implementation details | Understand backend code |

### 🛠️ Database & Scripts

| File | Description | How to Use |
|------|-------------|------------|
| **run-google-oauth-migration.sql** | Add Google OAuth columns | Run in PostgreSQL |
| **verify-google-users.sql** | Check Google users | Verify data |
| **test-google-auth.ps1** | PowerShell test script | Auto testing |

---

## 🚀 Quick Start Guide

### For Frontend Developers

1. **Read:** `FRONTEND_QUICK_REFERENCE.md` (5 minutes)
2. **Copy:** Code snippets into your project
3. **Setup:** Google OAuth Provider
4. **Test:** Login with username/password and Google

### For Backend Developers / Testers

1. **Run:** Migration SQL (`run-google-oauth-migration.sql`)
2. **Start:** Backend (F5 in Visual Studio)
3. **Open:** `swagger-google-oauth-tester.html`
4. **Test:** All endpoints in Swagger

---

## 📋 Features Implemented

### ✅ Authentication Methods

- [x] Username/Password Registration
- [x] Username/Password Login
- [x] Google OAuth Login/Register
- [x] Auto Account Linking (Google + existing email)
- [x] JWT Access Token (60 min)
- [x] Refresh Token (7 days, HTTPOnly cookie)
- [x] Auto Token Refresh
- [x] Logout

### ✅ User Management

- [x] Get Current User Info
- [x] Update Profile (FullName + Avatar)
- [x] Image Upload (Cloudinary/Local storage)
- [x] Thumbnail Generation (150x150, 50x50)
- [x] Get User Projects
- [x] Get User Role in Project

### ✅ Security

- [x] HTTPOnly Cookies for Refresh Token
- [x] Secure Cookies (HTTPS only)
- [x] SameSite=Strict (CSRF protection)
- [x] Google Token Verification
- [x] Email Verification (Google)
- [x] Password Hashing (SHA256 + Salt)

---

## 🎨 Frontend Flow Overview

```
┌──────────────────────────────────────────┐
│          Registration Screen             │
│  ┌────────────────────────────────────┐  │
│  │ Username: [________]               │  │
│  │ Email:    [________]               │  │
│  │ Password: [________]               │  │
│  │ [Register Button]                  │  │
│  └────────────────────────────────────┘  │
└──────────────────────────────────────────┘
                  ↓
┌──────────────────────────────────────────┐
│            Login Screen                  │
│  ┌────────────────────────────────────┐  │
│  │ Username: [________]               │  │
│  │ Password: [________]               │  │
│  │ [Login Button]                     │  │
│  │                                    │  │
│  │ ────── OR ──────                   │  │
│  │                                    │  │
│  │ [🔵 Sign in with Google]           │  │
│  └────────────────────────────────────┘  │
└──────────────────────────────────────────┘
                  ↓
┌──────────────────────────────────────────┐
│          Dashboard (Protected)           │
│  ┌────────────────────────────────────┐  │
│  │ 👤 John Doe        [Logout]        │  │
│  │                                    │  │
│  │ Your Projects:                     │  │
│  │  - Project 1                       │  │
│  │  - Project 2                       │  │
│  │                                    │  │
│  │ [Update Profile]                   │  │
│  └────────────────────────────────────┘  │
└──────────────────────────────────────────┘
```

---

## 🔐 Authentication Architecture

```
┌─────────────┐
│   Frontend  │
└──────┬──────┘
       │
       │ 1. Login Request
       ↓
┌─────────────┐     2. Verify        ┌──────────────┐
│   Backend   │ ←──────────────────→ │    Google    │
│  (API)      │   3. User Info       │   OAuth      │
└──────┬──────┘                      └──────────────┘
       │
       │ 4. Generate JWT
       ↓
┌─────────────┐
│  Database   │
│ (PostgreSQL)│
└─────────────┘
       │
       │ 5. Return Tokens
       ↓
┌─────────────┐
│  Frontend   │ ← Access Token (localStorage)
│             │ ← Refresh Token (HTTPOnly Cookie)
└─────────────┘
```

---

## 📊 API Endpoint Summary

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/auth/register` | Register new user |
| POST | `/api/auth/login` | Login with credentials |
| POST | `/api/auth/google` | Google OAuth login/register |
| POST | `/api/auth/refresh` | Refresh access token |
| POST | `/api/auth/logout` | Logout user |
| GET | `/api/user/me` | Get current user info |
| PUT | `/api/user/update-me` | Update profile + avatar |
| GET | `/api/user/me/projects` | Get user's projects |
| GET | `/api/user/projects/{id}/role` | Get role in project |

---

## 🧪 Testing Workflow

### Option 1: Swagger UI (Recommended for API Testing)
```
1. Start backend (F5)
2. Open: https://localhost:7001/swagger
3. Open: swagger-google-oauth-tester.html
4. Get Google ID Token
5. Test in Swagger
```

### Option 2: Postman (Recommended for Automation)
```
1. Import: IssueTracker-Auth-API.postman_collection.json
2. Set environment: baseUrl = https://localhost:7001
3. Test endpoints
```

### Option 3: HTML Tool (Recommended for Manual E2E)
```
1. Open: test-auth.html
2. Test all features visually
```

---

## 🔧 Troubleshooting

### Common Issues

| Issue | Solution | Reference |
|-------|----------|-----------|
| Google OAuth error | Configure Google Cloud Console | `GOOGLE_OAUTH_SETUP.md` |
| CORS error | Check `Program.cs` CORS config | `API_DOCUMENTATION.md` |
| Token expired | Use refresh token endpoint | `FRONTEND_QUICK_REFERENCE.md` |
| Cookie not set | Use `credentials: 'include'` | `FRONTEND_INTEGRATION_GUIDE.md` |

---

## 📞 Support & Resources

### Documentation
- **Full Guide:** `FRONTEND_INTEGRATION_GUIDE.md`
- **Quick Start:** `FRONTEND_QUICK_REFERENCE.md`
- **API Docs:** `API_DOCUMENTATION.md`

### Testing Tools
- **Swagger:** https://localhost:7001/swagger
- **Google Token:** `swagger-google-oauth-tester.html`
- **E2E Test:** `test-auth.html`

### Database
- **Migration:** `run-google-oauth-migration.sql`
- **Verification:** `verify-google-users.sql`

---

## ✅ Implementation Checklist

### Backend ✅ (Completed)
- [x] User entity với Google OAuth fields
- [x] Google OAuth service implementation
- [x] Auth endpoints (register, login, google, refresh, logout)
- [x] User endpoints (get, update, projects)
- [x] JWT token generation & validation
- [x] Refresh token with HTTPOnly cookie
- [x] Image upload (avatar)
- [x] Database migration

### Frontend 🔄 (Need to Implement)
- [ ] Install dependencies (`@react-oauth/google`, `axios`)
- [ ] Setup Google OAuth Provider
- [ ] Create Auth Context
- [ ] Create Login Page
- [ ] Create Register Page
- [ ] Create Protected Route Component
- [ ] Implement auto token refresh
- [ ] Create Profile Page
- [ ] Test all flows

---

## 🎓 Learning Path

### For Junior Developers
1. Start with: `FRONTEND_QUICK_REFERENCE.md`
2. Follow: `FRONTEND_INTEGRATION_GUIDE.md`
3. Test with: `test-auth.html`

### For Senior Developers
1. Review: `API_DOCUMENTATION.md`
2. Understand: `GOOGLE_OAUTH_IMPLEMENTATION.md`
3. Test with: Postman collection

---

## 📈 Next Steps

### Enhancements (Future)
- [ ] Email verification for password users
- [ ] Password reset functionality
- [ ] Two-factor authentication (2FA)
- [ ] Multiple OAuth providers (Facebook, GitHub)
- [ ] Account linking UI
- [ ] Session management
- [ ] Audit logs

---

**Version:** 1.0  
**Last Updated:** 2024  
**Backend:** .NET 10  
**Frontend:** React + TypeScript  
**Database:** PostgreSQL
