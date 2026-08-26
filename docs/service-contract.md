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

## Events

Customer Service consumes `identity.user.registered.v1` and reserves these events for Policy Service and future consumers:

- `customer.created.v1`
- `customer.updated.v1`
- `customer.deactivated.v1`

## Change Rules

Review the OpenAPI diff with API consumers before changing public endpoints. Keep secrets, database connection strings, and deployed service URLs outside source control.