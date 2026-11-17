# Authentication & Authorization Requirements

## Project Overview

Implement authentication and authorization for the mobile application using Microsoft Entra External ID as the identity provider.

The solution must support self-service user registration, social login providers, secure API access, and future extensibility for additional identity providers and role-based authorization.

---

# Goals

- Allow users to create their own accounts without administrator involvement.
- Support login via Google accounts.
- Support login via Microsoft accounts.
- Support login via email/password accounts.
- Secure all backend API endpoints using JWT access tokens.
- Maintain user sessions across application restarts.
- Support future additions such as Apple Sign-In, MFA, subscriptions, and role-based access control.

---

# System Architecture

```text
Flutter Mobile App
        |
        | OpenID Connect / OAuth 2.0
        |
Microsoft Entra External ID
        |
        | JWT Access Token
        |
ASP.NET Core Web API
        |
        | EF Core
        |
Database
```

---

# Functional Requirements

## FR-001 User Registration

Users shall be able to create an account directly from the mobile application.

Supported registration methods:

- Google Account
- Microsoft Account
- Email Address

No manual user creation by administrators shall be required.

---

## FR-002 User Authentication

Users shall be able to authenticate using:

- Google Sign-In
- Microsoft Sign-In
- Email Sign-In

Successful authentication shall return valid access tokens for use with protected APIs.

---

## FR-003 User Session Persistence

The mobile application shall maintain authenticated sessions across application restarts.

Authentication state shall be restored automatically when valid tokens are available.

---

## FR-004 Password Reset

Email-based users shall be able to:

- Request password reset
- Complete password reset
- Regain access without administrator intervention

---

## FR-005 User Logout

Users shall be able to sign out from the application.

Logging out shall:

- Remove locally stored authentication tokens
- Remove application authentication state
- Redirect users to the login screen

---

## FR-006 API Authorization

Protected API endpoints shall require valid access tokens.

Unauthenticated requests shall receive HTTP 401 Unauthorized responses.

---

## FR-007 User Provisioning

The backend shall automatically create an application user record the first time a successfully authenticated user accesses the system.

User records shall include:

- External Identity Identifier
- Email Address
- Display Name
- Identity Provider
- Creation Timestamp

---

## FR-008 Account Management

The system shall support future implementation of:

- Profile editing
- Account deletion
- Account recovery

---

# Non-Functional Requirements

## Security Requirements

- All traffic shall use HTTPS.
- JWT tokens shall be validated by the API.
- Authentication tokens shall never be stored in plain text.
- Authentication tokens shall be stored using platform-secure storage mechanisms.
- User passwords shall never be handled directly by the mobile application.

---

## Availability Requirements

Authentication services shall remain available while Microsoft Entra External ID services are available.

---

## Scalability Requirements

The architecture shall support:

- Thousands of users
- Multiple client applications
- Future web application support
- Future desktop application support

---

## Maintainability Requirements

Authentication functionality shall be isolated into dedicated modules.

Flutter authentication code shall be separated from application business logic.

.NET authentication configuration shall be centralized and reusable.

---

# Identity Provider Requirements

## Microsoft Entra External ID

The solution shall use Microsoft Entra External ID as the primary identity provider.

The configuration shall support:

- Self-service registration
- Social login federation
- Local email accounts
- User attribute collection
- Password reset

---

## Google Sign-In

Users shall be able to authenticate using Google accounts.

Google identities shall be federated through Microsoft Entra External ID.

---

## Microsoft Sign-In

Users shall be able to authenticate using Microsoft accounts.

Microsoft identities shall be federated through Microsoft Entra External ID.

---

## Email Sign-In

Users shall be able to register and authenticate using email credentials managed by Microsoft Entra External ID.

---

# Mobile Application Requirements

## Authentication Screens

The application shall provide:

- Login Screen
- Registration Screen
- Forgot Password Screen
- Logout Functionality

---

## Secure Token Storage

Authentication tokens shall be stored using:

- iOS Keychain
- Android Keystore

Flutter Secure Storage shall be used as the client storage mechanism.

---

## Route Protection

Unauthenticated users shall not have access to protected screens.

Protected screens shall require a valid authenticated session.

---

# Backend API Requirements

## Authentication

The API shall validate:

- Token issuer
- Audience
- Signature
- Expiration

---

## Authorization

Phase 1:

- Authenticated users only

Phase 2:

- Role-based authorization

Potential future roles:

- User
- PremiumUser
- Administrator

---

## User Context

Authenticated request context shall expose:

- User Identifier
- Email Address
- Display Name
- Identity Provider

---

# Database Requirements

## Users Table

The application shall maintain application-specific user records.

Suggested fields:

- Id
- ExternalUserId
- Email
- DisplayName
- IdentityProvider
- CreatedUtc

Authentication data shall remain managed by Microsoft Entra External ID.

---

# Out of Scope

The following features are not included in the initial implementation:

- Subscription billing
- MFA
- Apple Sign-In
- User roles
- Administrative portal
- Device management
- Social account linking

These may be implemented in future phases.