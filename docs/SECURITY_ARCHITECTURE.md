# SECURITY ARCHITECTURE - PUBLIC FORM + STAFF-ONLY SYSTEM

## 🔐 ACCESS CONTROL STRATEGY

### **Two Security Zones:**

**Zone 1: PUBLIC (No Authentication)**
- Application Form: `/apply`
- Housing Preferences Form: `/housing-preferences/{token}`
- Thank You pages

**Zone 2: STAFF ONLY (Authentication Required)**
- Everything else:
  - Dashboard
  - Contacts
  - Deals
  - Properties
  - Showings
  - Reports
  - Admin

---

## 🎯 IMPLEMENTATION APPROACH

### **Frontend Routing:**

```
Public Routes (No Auth):
├── / (homepage with "Apply Now" button)
├── /apply (application form)
├── /application-submitted (thank you page)
└── /housing-preferences/:token (secure form link)

Protected Routes (Auth Required):
├── /dashboard
├── /contacts
├── /deals
├── /properties
├── /showings
├── /reports
└── /admin
```

### **Backend API Security:**

```csharp
// Public endpoints (no [Authorize] attribute)
[HttpPost("/api/applications")]
public async Task<IActionResult> SubmitApplication()
{
    // Anyone can submit
    // Rate limiting: 5 submissions per IP per hour
}

[HttpPost("/api/housing-preferences")]
public async Task<IActionResult> SubmitHousingPreferences([FromQuery] string token)
{
    // Validate JWT token
    // Token identifies the contact
    // Token expires in 30 days
}

// Protected endpoints (require authentication)
[Authorize(Roles = "Staff")]
[HttpGet("/api/contacts")]
public async Task<IActionResult> GetContacts()
{
    // Only authenticated staff can access
}

[Authorize(Roles = "Admin")]
[HttpDelete("/api/contacts/{id}")]
public async Task<IActionResult> DeleteContact(Guid id)
{
    // Only admins can delete
}
```

---

## 🔒 AUTHENTICATION ARCHITECTURE

### **AWS Cognito User Pools:**

**Two User Pools:**

**1. Staff User Pool (Internal)**
- Users: Your 3-5 staff members
- Sign-up: Invitation only (admin creates accounts)
- Login: Email + password
- MFA: Optional (recommended for admins)
- Roles: Admin, Coordinator, BoardMember
- JWT tokens for API access

**2. No User Pool for Applicants**
- Families DON'T create accounts
- They just submit forms (anonymous)
- Exception: Housing preferences form uses JWT token

---

## 🎫 HOUSING PREFERENCES TOKEN SYSTEM

### **Why Tokens:**
- Families shouldn't need to create accounts
- But we need to know who's submitting housing preferences
- Solution: Send them a secure link with embedded token

### **Flow:**

```
1. Family applies → Contact created in system
2. Board approves → Send email:
   "Mazel Tov! Fill out your housing preferences: 
    https://yourapp.com/housing-preferences?token=eyJhbGc..."
3. Token contains: ContactId, expiration (30 days)
4. Family clicks link → Form pre-identifies them
5. They submit preferences → System updates their contact
6. Token expires → They can request new link if needed
```

### **Token Implementation:**

```csharp
// Generate token when sending approval email
public string GenerateHousingPreferencesToken(Guid contactId)
{
    var tokenHandler = new JwtSecurityTokenHandler();
    var key = Encoding.ASCII.GetBytes(_config["JwtSecret"]);
    
    var tokenDescriptor = new SecurityTokenDescriptor
    {
        Subject = new ClaimsIdentity(new[]
        {
            new Claim("contactId", contactId.ToString()),
            new Claim("purpose", "housing-preferences")
        }),
        Expires = DateTime.UtcNow.AddDays(30),
        SigningCredentials = new SigningCredentials(
            new SymmetricSecurityKey(key),
            SecurityAlgorithms.HmacSha256Signature)
    };
    
    var token = tokenHandler.CreateToken(tokenDescriptor);
    return tokenHandler.WriteToken(token);
}

// Validate token when submitting preferences
public Guid ValidateHousingPreferencesToken(string token)
{
    var tokenHandler = new JwtSecurityTokenHandler();
    var key = Encoding.ASCII.GetBytes(_config["JwtSecret"]);
    
    tokenHandler.ValidateToken(token, new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = false,
        ValidateAudience = false,
        ClockSkew = TimeSpan.Zero
    }, out SecurityToken validatedToken);
    
    var jwtToken = (JwtSecurityToken)validatedToken;
    var contactId = Guid.Parse(jwtToken.Claims.First(x => x.Type == "contactId").Value);
    
    return contactId;
}
```

---

## 🛡️ SECURITY MEASURES

### **Public Form Protection:**

**1. Rate Limiting:**
```csharp
// Limit to 5 submissions per IP per hour
services.AddRateLimiting(options =>
{
    options.AddPolicy("PublicFormPolicy", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString(),
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 5,
                Window = TimeSpan.FromHours(1)
            }));
});
```

**2. CAPTCHA (Google reCAPTCHA v3):**
- Invisible to users
- Prevents bot submissions
- Free tier: 1M assessments/month

**3. Input Validation:**
- All fields validated server-side
- XSS prevention (sanitize HTML)
- SQL injection prevention (EF Core parameterizes)
- Email validation (regex)
- Phone validation (format)

**4. CSRF Protection:**
- Anti-forgery tokens on forms
- Built-in to ASP.NET Core

**5. HTTPS Only:**
- Enforce HTTPS everywhere
- HSTS headers
- Secure cookies

---

## 🎭 ROLE-BASED ACCESS CONTROL

### **Three Roles:**

**1. Admin**
- Full system access
- User management
- System configuration
- Delete data
- Export data

**2. Coordinator**
- Manage contacts
- Manage deals
- Manage properties
- Schedule showings
- View reports
- CANNOT: Delete data, manage users, system config

**3. Board Member**
- View contacts (read-only)
- View deals (read-only)
- Approve/reject applications
- View reports
- CANNOT: Edit contacts, create properties, delete anything

### **Implementation:**

```csharp
// In controllers
[Authorize(Roles = "Admin,Coordinator")]
[HttpPost("/api/contacts")]
public async Task<IActionResult> CreateContact() { }

[Authorize(Roles = "Admin")]
[HttpDelete("/api/contacts/{id}")]
public async Task<IActionResult> DeleteContact() { }

[Authorize(Roles = "Admin,Coordinator,BoardMember")]
[HttpGet("/api/contacts/{id}")]
public async Task<IActionResult> GetContact() { }

// In application layer
public class UpdateContactCommandHandler
{
    public async Task<Result> Handle(UpdateContactCommand request)
    {
        // Check role-specific permissions
        if (request.UserRole == "BoardMember")
        {
            return Result.Fail("Board members cannot edit contacts");
        }
        
        // Proceed with update
    }
}
```

---

## 📱 FRONTEND ROUTING PROTECTION

### **React Router Example:**

```typescript
// Public routes (no authentication check)
<Route path="/" element={<HomePage />} />
<Route path="/apply" element={<ApplicationForm />} />
<Route path="/application-submitted" element={<ThankYou />} />
<Route path="/housing-preferences" element={<HousingPreferencesForm />} />

// Protected routes (redirect to login if not authenticated)
<Route path="/dashboard" element={
  <ProtectedRoute>
    <Dashboard />
  </ProtectedRoute>
} />

<Route path="/contacts" element={
  <ProtectedRoute>
    <ContactList />
  </ProtectedRoute>
} />

// Admin-only routes
<Route path="/admin" element={
  <ProtectedRoute requiredRole="Admin">
    <AdminPanel />
  </ProtectedRoute>
} />

// ProtectedRoute component
function ProtectedRoute({ children, requiredRole }) {
  const { isAuthenticated, user } = useAuth();
  
  if (!isAuthenticated) {
    return <Navigate to="/login" />;
  }
  
  if (requiredRole && user.role !== requiredRole) {
    return <Navigate to="/unauthorized" />;
  }
  
  return children;
}
```

---

## 🌐 DOMAIN STRUCTURE

### **Recommended Setup:**

**Option A: Subdomain**
- Public site: `apply.kehilasunion.org`
- Staff portal: `admin.kehilasunion.org`
- Clearer separation
- Different CORS policies

**Option B: Path-based**
- Public form: `kehilasunion.org/apply`
- Staff portal: `kehilasunion.org/dashboard`
- Simpler DNS
- Single SSL certificate
- **Recommended for your use case**

---

## 🔐 AUTHENTICATION FLOW

### **Staff Login Flow:**

```
1. User goes to /login
2. Enters email + password
3. Frontend calls AWS Cognito
4. Cognito validates credentials
5. Returns JWT access token + refresh token
6. Frontend stores tokens (httpOnly cookies)
7. All API calls include token in Authorization header
8. API validates token with Cognito
9. API checks user role from token claims
10. API allows/denies based on role + endpoint
```

### **Cognito Configuration:**

```csharp
services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = "https://cognito-idp.{region}.amazonaws.com/{userPoolId}";
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidAudience = "{clientId}",
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

services.AddAuthorization(options =>
{
    options.AddPolicy("RequireStaffRole", policy =>
        policy.RequireRole("Admin", "Coordinator", "BoardMember"));
    
    options.AddPolicy("RequireAdminRole", policy =>
        policy.RequireRole("Admin"));
});
```

---

## 🎯 SECURITY CHECKLIST

**For Public Form:**
- [ ] HTTPS enforced
- [ ] Rate limiting (5 per hour per IP)
- [ ] reCAPTCHA v3 implemented
- [ ] Input validation (server-side)
- [ ] CSRF tokens
- [ ] No sensitive data exposure
- [ ] Email confirmation only (no sensitive info in email)

**For Staff Portal:**
- [ ] AWS Cognito authentication
- [ ] Role-based authorization
- [ ] JWT token validation on every request
- [ ] Tokens expire (1 hour access, 7 days refresh)
- [ ] HTTPS only
- [ ] XSS prevention
- [ ] SQL injection prevention (EF Core)
- [ ] Audit logging (who did what when)
- [ ] Session timeout (15 minutes idle)
- [ ] Logout functionality

**For Housing Preferences Form:**
- [ ] Token-based authentication
- [ ] Token expires in 30 days
- [ ] Token can only be used for specific contact
- [ ] Token can only be used once (mark as used)
- [ ] Rate limiting
- [ ] Input validation

---

## 🚨 ATTACK PREVENTION

**Common Attacks & Mitigations:**

**1. SQL Injection**
- ✅ Mitigated: EF Core parameterizes all queries

**2. XSS (Cross-Site Scripting)**
- ✅ Mitigated: React escapes HTML by default
- ✅ Mitigated: Sanitize user input (DOMPurify library)

**3. CSRF (Cross-Site Request Forgery)**
- ✅ Mitigated: Anti-forgery tokens on forms
- ✅ Mitigated: SameSite cookies

**4. Brute Force Login**
- ✅ Mitigated: AWS Cognito rate limiting
- ✅ Mitigated: Account lockout after 5 failed attempts
- ✅ Mitigated: CAPTCHA on login form

**5. DDoS on Public Form**
- ✅ Mitigated: AWS WAF (optional, costs money)
- ✅ Mitigated: Rate limiting
- ✅ Mitigated: CloudFront CDN

**6. Data Leakage**
- ✅ Mitigated: API returns only authorized data
- ✅ Mitigated: No sensitive data in URLs or logs
- ✅ Mitigated: Audit logging for all access

---

## 📊 SECURITY IMPLEMENTATION PRIORITY

**P0 (Must Have - MVP):**
- ✅ Public form with rate limiting
- ✅ Staff authentication (Cognito)
- ✅ Role-based authorization
- ✅ HTTPS enforcement
- ✅ Input validation
- ✅ CSRF protection

**P1 (Should Have - Phase 2):**
- ✅ reCAPTCHA on public form
- ✅ Audit logging
- ✅ Session timeout
- ✅ Housing preferences token system

**P2 (Could Have - Phase 3):**
- ✅ MFA for admins
- ✅ AWS WAF for DDoS protection
- ✅ Advanced threat detection
- ✅ Security headers (CSP, etc.)

---

**Your setup will be SECURE from day 1!** 🔒

Public can apply, staff is protected, families don't need accounts. Perfect architecture!
