# Customer Service Contract

## Ownership

Customer Service owns customer profiles, KYC data, authorization rules, and its database. It must not query another service's database.

## Public API

- Local URL: `http://localhost:5180`
- Gateway prefix: `/customers`
- Health endpoint: `GET /health`
- Development Swagger UI: `/swagger`

Service routes use the `/api/{resource}` convention. Error responses use RFC 7807 `application/problem+json` with `status`, `title`, and `detail`.

## Authorization

Validate Identity Service JWTs locally with issuer `InsurancePlatform.Identity` and audience `InsurancePlatform.Clients`.

| Permission | Capability |
| --- | --- |
| `Customer.Read` | Read customer records |
| `Customer.Write` | Create or update customer records |

## KYC Upload Configuration

KYC document uploads are scanned before encryption and storage by default. Demo deployments without a malware scanner can explicitly set `Kyc__SkipMalwareScan=true`. This bypass is not suitable for production or real identity documents.

Set `Kyc__Storage__Provider=Database` to store encrypted document bytes in the Customer Service PostgreSQL database. This avoids a separate storage account for small demo workloads. In this mode, only `Kyc__EncryptionKeyBase64` is required; storage endpoint credentials are not used. The default provider remains object storage, which requires `Kyc__Storage__Endpoint`, `Kyc__Storage__AccessKey`, and `Kyc__Storage__SecretKey`.

## Events

Customer Service consumes `identity.user.registered.v1` and reserves these events for Policy Service and future consumers:

- `customer.created.v1`
- `customer.updated.v1`
- `customer.deactivated.v1`

## Change Rules

Review the OpenAPI diff with API consumers before changing public endpoints. Keep secrets, database connection strings, and deployed service URLs outside source control.