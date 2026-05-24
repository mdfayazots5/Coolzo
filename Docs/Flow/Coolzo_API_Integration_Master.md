# Coolzo API Integration Master

Source of truth: backend controllers, request/response contracts, ProjectOverview module slices, and consuming customer/admin/mobile service layers verified on 2026-04-19.

## 1. Authentication APIs

| Module | API Group / Controller | API Name | HTTP | Route | Purpose / Description | Used By | Authentication Requirement | Query Parameters | Path Parameters | Headers | Request Body / Field Definition | Response Structure / Field Definition | Success / Error Example | Validation / Business Rule Notes | Dependency / Linked APIs | Integration Notes |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| Access & Security Foundation | AuthController | Login | POST | /api/auth/login | Authenticate a user and issue access + refresh tokens. | Both | Anonymous | None | None | None | [`LoginRequest`](#schema-loginrequest)<br>Field-by-field: schema registry entry. | Envelope: `ApiResponse<AuthTokenResponse>`<br>Data schema: [`AuthTokenResponse`](#schema-authtokenresponse)<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Anonymous Request body fields are defined in [`LoginRequest`](#schema-loginrequest). | `POST /auth/login` -> `POST /auth/refresh` -> `GET /auth/me` sequence. | Persist `accessToken` + `refreshToken`; send bearer token on every secured request. |
| Access & Security Foundation | AuthController | Refresh | POST | /api/auth/refresh | Refresh an access token using the current refresh token. | Both | Anonymous | None | None | None | [`RefreshTokenRequest`](#schema-refreshtokenrequest)<br>Field-by-field: schema registry entry. | Envelope: `ApiResponse<AuthTokenResponse>`<br>Data schema: [`AuthTokenResponse`](#schema-authtokenresponse)<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Anonymous Request body fields are defined in [`RefreshTokenRequest`](#schema-refreshtokenrequest). | `POST /auth/login` -> `POST /auth/refresh` -> `GET /auth/me` sequence. | Persist `accessToken` + `refreshToken`; send bearer token on every secured request. |
| Access & Security Foundation | AuthController | GetCurrentUser | GET | /api/auth/me | Return the authenticated user profile, roles, and permissions. | Both | Authorize | None | None | None | None | Envelope: `ApiResponse<CurrentUserResponse>`<br>Data schema: [`CurrentUserResponse`](#schema-currentuserresponse)<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Authorize | `POST /auth/login` -> `POST /auth/refresh` -> `GET /auth/me` sequence. | Persist `accessToken` + `refreshToken`; send bearer token on every secured request. |
| Access & Security Foundation | CustomerAuthController | Register | POST | /api/customer-auth/register | Register a new customer account. | Customer | Anonymous | None | None | None | [`RegisterCustomerRequest`](#schema-registercustomerrequest)<br>Field-by-field: schema registry entry. | Envelope: `ApiResponse<CustomerAccountResponse>`<br>Data schema: [`CustomerAccountResponse`](#schema-customeraccountresponse)<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Anonymous Request body fields are defined in [`RegisterCustomerRequest`](#schema-registercustomerrequest). | See referenced request/response schemas and related route families in the same module. | Customer web/mobile can use `/auth/login` for session bootstrap after registration/reset flows. |
| Access & Security Foundation | CustomerAuthController | ForgotPassword | POST | /api/customer-auth/forgot-password | Start customer password reset using login identifier input. | Customer | Anonymous | None | None | None | [`ForgotCustomerPasswordRequest`](#schema-forgotcustomerpasswordrequest)<br>Field-by-field: schema registry entry. | Envelope: `ApiResponse<CustomerPasswordOperationResponse>`<br>Data schema: [`CustomerPasswordOperationResponse`](#schema-customerpasswordoperationresponse)<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Anonymous Request body fields are defined in [`ForgotCustomerPasswordRequest`](#schema-forgotcustomerpasswordrequest). | See referenced request/response schemas and related route families in the same module. | Customer web/mobile can use `/auth/login` for session bootstrap after registration/reset flows. |
| Access & Security Foundation | CustomerAuthController | ResetPassword | POST | /api/customer-auth/reset-password | Reset customer password using the same contract currently wired for forgot-password. | Customer | Anonymous | None | None | None | [`ForgotCustomerPasswordRequest`](#schema-forgotcustomerpasswordrequest)<br>Field-by-field: schema registry entry. | Envelope: `ApiResponse<CustomerPasswordOperationResponse>`<br>Data schema: [`CustomerPasswordOperationResponse`](#schema-customerpasswordoperationresponse)<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Anonymous Request body fields are defined in [`ForgotCustomerPasswordRequest`](#schema-forgotcustomerpasswordrequest). | See referenced request/response schemas and related route families in the same module. | Customer web/mobile can use `/auth/login` for session bootstrap after registration/reset flows. |
| Access & Security Foundation | CustomerAuthController | ChangePassword | POST | /api/customer-auth/change-password | Change the authenticated customer password. | Customer | Authorize(Roles = RoleNames.Customer) | None | None | None | [`ChangeCustomerPasswordRequest`](#schema-changecustomerpasswordrequest)<br>Field-by-field: schema registry entry. | Envelope: `ApiResponse<CustomerPasswordOperationResponse>`<br>Data schema: [`CustomerPasswordOperationResponse`](#schema-customerpasswordoperationresponse)<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Authorize(Roles = RoleNames.Customer) Request body fields are defined in [`ChangeCustomerPasswordRequest`](#schema-changecustomerpasswordrequest). | See referenced request/response schemas and related route families in the same module. | Customer web/mobile can use `/auth/login` for session bootstrap after registration/reset flows. |

## 2. User / Profile / Session APIs

| Module | API Group / Controller | API Name | HTTP | Route | Purpose / Description | Used By | Authentication Requirement | Query Parameters | Path Parameters | Headers | Request Body / Field Definition | Response Structure / Field Definition | Success / Error Example | Validation / Business Rule Notes | Dependency / Linked APIs | Integration Notes |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| Customer Account Portal / Access & Security Foundation | CustomerController | Create | POST | /api/customers | Execute `Create` for `/api/customers`. | Admin | Authorize(Policy = PermissionNames.UserCreate) | None | None | None | [`CreateCustomerAccountRequest`](#schema-createcustomeraccountrequest)<br>Field-by-field: schema registry entry. | Envelope: `ApiResponse<CustomerAccountResponse>`<br>Data schema: [`CustomerAccountResponse`](#schema-customeraccountresponse)<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Authorize(Policy = PermissionNames.UserCreate) Request body fields are defined in [`CreateCustomerAccountRequest`](#schema-createcustomeraccountrequest). | See referenced request/response schemas and related route families in the same module. | Use JSON camelCase field names exactly as defined in the schema registry below. |
| Customer Account Portal / Access & Security Foundation | CustomerController | ResetPassword | POST | /api/customers/{customerId:long}/reset-password | Reset customer password using the same contract currently wired for forgot-password. | Admin | Authorize(Policy = PermissionNames.UserUpdate) | None | `customerId` `integer(int64)` Required | None | [`ResetCustomerPasswordRequest`](#schema-resetcustomerpasswordrequest)<br>Field-by-field: schema registry entry. | Envelope: `ApiResponse<CustomerPasswordOperationResponse>`<br>Data schema: [`CustomerPasswordOperationResponse`](#schema-customerpasswordoperationresponse)<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Authorize(Policy = PermissionNames.UserUpdate) Request body fields are defined in [`ResetCustomerPasswordRequest`](#schema-resetcustomerpasswordrequest). | See referenced request/response schemas and related route families in the same module. | Use JSON camelCase field names exactly as defined in the schema registry below. |
| Customer Account Portal / Access & Security Foundation | CustomerController | GetMyProfile | GET | /api/customers/me/profile | Return authenticated customer profile details. | Customer | Authorize(Roles = RoleNames.Customer) | None | None | None | None | Envelope: `ApiResponse<CustomerProfileResponse>`<br>Data schema: [`CustomerProfileResponse`](#schema-customerprofileresponse)<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Authorize(Roles = RoleNames.Customer) | `GET /auth/me` bootstraps session; this route returns customer-profile fields. | Server derives the customer from the bearer token; clients do not send `customerId`. |
| Customer Account Portal / Access & Security Foundation | CustomerController | UpdateMyProfile | PUT | /api/customers/me/profile | Update authenticated customer profile details. | Customer | Authorize(Roles = RoleNames.Customer) | None | None | None | [`UpdateCustomerProfileRequest`](#schema-updatecustomerprofilerequest)<br>Field-by-field: schema registry entry. | Envelope: `ApiResponse<CustomerProfileResponse>`<br>Data schema: [`CustomerProfileResponse`](#schema-customerprofileresponse)<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Authorize(Roles = RoleNames.Customer) Request body fields are defined in [`UpdateCustomerProfileRequest`](#schema-updatecustomerprofilerequest). | Usually follows `GET /customers/me/profile` and may be followed by `GET /auth/me` refresh. | Use JSON camelCase field names exactly as defined in the schema registry below. |
| Customer Account Portal / Access & Security Foundation | CustomerController | DeactivateMyAccount | POST | /api/customers/me/deactivate | Deactivate the authenticated customer account. | Customer | Authorize(Roles = RoleNames.Customer) | None | None | None | [`DeleteCustomerAccountRequest`](#schema-deletecustomeraccountrequest)<br>Field-by-field: schema registry entry. | Envelope: `ApiResponse<CustomerAccountDeletionResponse>`<br>Data schema: [`CustomerAccountDeletionResponse`](#schema-customeraccountdeletionresponse)<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Authorize(Roles = RoleNames.Customer) Request body fields are defined in [`DeleteCustomerAccountRequest`](#schema-deletecustomeraccountrequest). | Client should clear local auth state after a successful deactivation. | This is a customer self-service route; admin deletion/deactivation is not exposed here. |
| Access & Security Foundation | UserController | Get | GET | /api/users | Execute `Get` for `/api/users`. | Admin | Authorize(Policy = PermissionNames.UserRead) | `pageNumber` `integer(int32)` Required<br>`pageSize` `integer(int32)` Required | None | None | None | Envelope: `ApiResponse<PagedResult<UserResponse>>`<br>Data schema: `PagedResult` of [`UserResponse`](#schema-userresponse) with `items`, `totalCount`, `pageNumber`, `pageSize`<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Authorize(Policy = PermissionNames.UserRead) Omit null/empty query values; client utilities in web/mobile already serialize only populated keys. | See referenced request/response schemas and related route families in the same module. | Use JSON camelCase field names exactly as defined in the schema registry below. |
| Access & Security Foundation | UserController | Create | POST | /api/users | Execute `Create` for `/api/users`. | Admin | Authorize(Policy = PermissionNames.UserCreate) | None | None | None | [`CreateUserRequest`](#schema-createuserrequest)<br>Field-by-field: schema registry entry. | Envelope: `ApiResponse<UserResponse>`<br>Data schema: [`UserResponse`](#schema-userresponse)<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Authorize(Policy = PermissionNames.UserCreate) Request body fields are defined in [`CreateUserRequest`](#schema-createuserrequest). | See referenced request/response schemas and related route families in the same module. | Use JSON camelCase field names exactly as defined in the schema registry below. |
| Access & Security Foundation | UserController | Update | PUT | /api/users/{userId:long} | Execute `Update` for `/api/users/{userId:long}`. | Admin | Authorize(Policy = PermissionNames.UserUpdate) | None | `userId` `integer(int64)` Required | None | [`UpdateUserRequest`](#schema-updateuserrequest)<br>Field-by-field: schema registry entry. | Envelope: `ApiResponse<UserResponse>`<br>Data schema: [`UserResponse`](#schema-userresponse)<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Authorize(Policy = PermissionNames.UserUpdate) Request body fields are defined in [`UpdateUserRequest`](#schema-updateuserrequest). | See referenced request/response schemas and related route families in the same module. | Use JSON camelCase field names exactly as defined in the schema registry below. |

## 3. Lookup / Master APIs

| Module | API Group / Controller | API Name | HTTP | Route | Purpose / Description | Used By | Authentication Requirement | Query Parameters | Path Parameters | Headers | Request Body / Field Definition | Response Structure / Field Definition | Success / Error Example | Validation / Business Rule Notes | Dependency / Linked APIs | Integration Notes |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| Service Booking Engine | BookingLookupController | GetServiceCategories | GET | /api/booking-lookups/service-categories | Return booking service category lookup values. | Both | Anonymous | `search` `string?` Optional | None | None | None | Envelope: `ApiResponse<IReadOnlyCollection<ServiceCategoryLookupResponse>>`<br>Data schema: Array of [`ServiceCategoryLookupResponse`](#schema-servicecategorylookupresponse)<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Anonymous Omit null/empty query values; client utilities in web/mobile already serialize only populated keys. | See referenced request/response schemas and related route families in the same module. | Use JSON camelCase field names exactly as defined in the schema registry below. |
| Service Booking Engine | BookingLookupController | GetServices | GET | /api/booking-lookups/services | Return booking service lookup values filtered by category/search. | Both | Anonymous | `serviceCategoryId` `integer(int64)?` Optional<br>`search` `string?` Optional | None | None | None | Envelope: `ApiResponse<IReadOnlyCollection<ServiceLookupResponse>>`<br>Data schema: Array of [`ServiceLookupResponse`](#schema-servicelookupresponse)<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Anonymous Omit null/empty query values; client utilities in web/mobile already serialize only populated keys. | See referenced request/response schemas and related route families in the same module. | Use JSON camelCase field names exactly as defined in the schema registry below. |
| Service Booking Engine | BookingLookupController | GetAcTypes | GET | /api/booking-lookups/ac-types | Fetch ac types data for `/api/booking-lookups/ac-types`. | Both | Anonymous | `search` `string?` Optional | None | None | None | Envelope: `ApiResponse<IReadOnlyCollection<AcTypeLookupResponse>>`<br>Data schema: Array of [`AcTypeLookupResponse`](#schema-actypelookupresponse)<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Anonymous Omit null/empty query values; client utilities in web/mobile already serialize only populated keys. | See referenced request/response schemas and related route families in the same module. | Use JSON camelCase field names exactly as defined in the schema registry below. |
| Service Booking Engine | BookingLookupController | GetTonnages | GET | /api/booking-lookups/tonnage | Fetch tonnages data for `/api/booking-lookups/tonnage`. | Both | Anonymous | `search` `string?` Optional | None | None | None | Envelope: `ApiResponse<IReadOnlyCollection<TonnageLookupResponse>>`<br>Data schema: Array of [`TonnageLookupResponse`](#schema-tonnagelookupresponse)<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Anonymous Omit null/empty query values; client utilities in web/mobile already serialize only populated keys. | See referenced request/response schemas and related route families in the same module. | Use JSON camelCase field names exactly as defined in the schema registry below. |
| Service Booking Engine | BookingLookupController | GetBrands | GET | /api/booking-lookups/brands | Fetch brands data for `/api/booking-lookups/brands`. | Both | Anonymous | `search` `string?` Optional | None | None | None | Envelope: `ApiResponse<IReadOnlyCollection<BrandLookupResponse>>`<br>Data schema: Array of [`BrandLookupResponse`](#schema-brandlookupresponse)<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Anonymous Omit null/empty query values; client utilities in web/mobile already serialize only populated keys. | See referenced request/response schemas and related route families in the same module. | Use JSON camelCase field names exactly as defined in the schema registry below. |
| Service Booking Engine | BookingLookupController | GetZoneByPincode | GET | /api/booking-lookups/zones/by-pincode/{pincode} | Resolve zone details from the supplied pincode. | Both | Anonymous | None | `pincode` `string` Required | None | None | Envelope: `ApiResponse<ZoneLookupResponse>`<br>Data schema: [`ZoneLookupResponse`](#schema-zonelookupresponse)<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Anonymous | See referenced request/response schemas and related route families in the same module. | Use JSON camelCase field names exactly as defined in the schema registry below. |
| Service Booking Engine | BookingLookupController | GetSlots | GET | /api/booking-lookups/slots | Return slot availability for a zone/date pair. | Both | Anonymous | `zoneId` `integer(int64)` Required<br>`slotDate` `date` Required | None | None | None | Envelope: `ApiResponse<IReadOnlyCollection<SlotAvailabilityResponse>>`<br>Data schema: Array of [`SlotAvailabilityResponse`](#schema-slotavailabilityresponse)<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Anonymous Omit null/empty query values; client utilities in web/mobile already serialize only populated keys. | See referenced request/response schemas and related route families in the same module. | Use JSON camelCase field names exactly as defined in the schema registry below. |
| Configuration & Master Data | ConfigurationController | GetSystemSettings | GET | /api/configuration/settings | Fetch system settings data for `/api/configuration/settings`. | Admin | Authorize(Policy = PermissionNames.ConfigurationRead) | None | None | None | None | Envelope: `ApiResponse<IReadOnlyCollection<SystemSettingResponse>>`<br>Data schema: Array of [`SystemSettingResponse`](#schema-systemsettingresponse)<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Authorize(Policy = PermissionNames.ConfigurationRead) | See referenced request/response schemas and related route families in the same module. | Use JSON camelCase field names exactly as defined in the schema registry below. |
| Configuration & Master Data | LookupController | Get | GET | /api/lookups/{lookupType} | Execute `Get` for `/api/lookups/{lookupType}`. | Admin | Authorize(Policy = PermissionNames.LookupRead) | None | `lookupType` `string` Required | None | None | Envelope: `ApiResponse<IReadOnlyCollection<LookupItemResponse>>`<br>Data schema: Array of [`LookupItemResponse`](#schema-lookupitemresponse)<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Authorize(Policy = PermissionNames.LookupRead) | See referenced request/response schemas and related route families in the same module. | Use JSON camelCase field names exactly as defined in the schema registry below. |
| Configuration & Master Data | MasterDataAdminController | Get | GET | /api/admin-masters | Execute `Get` for `/api/admin-masters`. | Admin | Authorize(Policy = PermissionNames.LookupRead) | `masterType` `string?` Optional<br>`search` `string?` Optional<br>`isActive` `boolean?` Optional | None | None | None | Envelope: `ApiResponse<IReadOnlyCollection<DynamicMasterRecordResponse>>`<br>Data schema: Array of [`DynamicMasterRecordResponse`](#schema-dynamicmasterrecordresponse)<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Authorize(Policy = PermissionNames.LookupRead) Omit null/empty query values; client utilities in web/mobile already serialize only populated keys. | See referenced request/response schemas and related route families in the same module. | Use JSON camelCase field names exactly as defined in the schema registry below. |
| Configuration & Master Data | MasterDataAdminController | GetByType | GET | /api/admin-masters/{type} | Fetch by type data for `/api/admin-masters/{type}`. | Admin | Authorize(Policy = PermissionNames.LookupRead) | `search` `string?` Optional<br>`isActive` `boolean?` Optional | `type` `string` Required | None | None | Envelope: `ApiResponse<IReadOnlyCollection<DynamicMasterRecordResponse>>`<br>Data schema: Array of [`DynamicMasterRecordResponse`](#schema-dynamicmasterrecordresponse)<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Authorize(Policy = PermissionNames.LookupRead) Omit null/empty query values; client utilities in web/mobile already serialize only populated keys. | See referenced request/response schemas and related route families in the same module. | Use JSON camelCase field names exactly as defined in the schema registry below. |
| Configuration & Master Data | MasterDataAdminController | Create | POST | /api/admin-masters | Execute `Create` for `/api/admin-masters`. | Admin | Authorize(Policy = PermissionNames.LookupManage) | None | None | None | [`DynamicMasterRecordUpsertRequest`](#schema-dynamicmasterrecordupsertrequest)<br>Field-by-field: schema registry entry. | Envelope: `ApiResponse<DynamicMasterRecordResponse>`<br>Data schema: [`DynamicMasterRecordResponse`](#schema-dynamicmasterrecordresponse)<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Authorize(Policy = PermissionNames.LookupManage) Request body fields are defined in [`DynamicMasterRecordUpsertRequest`](#schema-dynamicmasterrecordupsertrequest). | See referenced request/response schemas and related route families in the same module. | Use JSON camelCase field names exactly as defined in the schema registry below. |
| Configuration & Master Data | MasterDataAdminController | Update | PUT | /api/admin-masters/{dynamicMasterRecordId:long} | Execute `Update` for `/api/admin-masters/{dynamicMasterRecordId:long}`. | Admin | Authorize(Policy = PermissionNames.LookupManage) | None | `dynamicMasterRecordId` `integer(int64)` Required | None | [`DynamicMasterRecordUpsertRequest`](#schema-dynamicmasterrecordupsertrequest)<br>Field-by-field: schema registry entry. | Envelope: `ApiResponse<DynamicMasterRecordResponse>`<br>Data schema: [`DynamicMasterRecordResponse`](#schema-dynamicmasterrecordresponse)<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Authorize(Policy = PermissionNames.LookupManage) Request body fields are defined in [`DynamicMasterRecordUpsertRequest`](#schema-dynamicmasterrecordupsertrequest). | See referenced request/response schemas and related route families in the same module. | Use JSON camelCase field names exactly as defined in the schema registry below. |
| Configuration & Master Data | SystemConfigurationController | CreateSystemConfiguration | POST | /api/system-configurations | Create system configuration for `/api/system-configurations`. | Admin | Authorize(Policy = PermissionNames.ConfigurationManage) | None | None | None | [`SystemConfigurationUpsertRequest`](#schema-systemconfigurationupsertrequest)<br>Field-by-field: schema registry entry. | Envelope: `ApiResponse<SystemConfigurationResponse>`<br>Data schema: [`SystemConfigurationResponse`](#schema-systemconfigurationresponse)<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Authorize(Policy = PermissionNames.ConfigurationManage) Request body fields are defined in [`SystemConfigurationUpsertRequest`](#schema-systemconfigurationupsertrequest). | See referenced request/response schemas and related route families in the same module. | Use JSON camelCase field names exactly as defined in the schema registry below. |
| Configuration & Master Data | SystemConfigurationController | UpdateSystemConfiguration | PUT | /api/system-configurations/{systemConfigurationId:long} | Update system configuration for `/api/system-configurations/{systemConfigurationId:long}`. | Admin | Authorize(Policy = PermissionNames.ConfigurationManage) | None | `systemConfigurationId` `integer(int64)` Required | None | [`SystemConfigurationUpsertRequest`](#schema-systemconfigurationupsertrequest)<br>Field-by-field: schema registry entry. | Envelope: `ApiResponse<SystemConfigurationResponse>`<br>Data schema: [`SystemConfigurationResponse`](#schema-systemconfigurationresponse)<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Authorize(Policy = PermissionNames.ConfigurationManage) Request body fields are defined in [`SystemConfigurationUpsertRequest`](#schema-systemconfigurationupsertrequest). | See referenced request/response schemas and related route families in the same module. | Use JSON camelCase field names exactly as defined in the schema registry below. |
| Configuration & Master Data | SystemConfigurationController | GetSystemConfigurations | GET | /api/system-configurations | Fetch system configurations data for `/api/system-configurations`. | Admin | Authorize(Policy = PermissionNames.ConfigurationRead) | `configurationGroup` `string?` Optional<br>`configurationKey` `string?` Optional<br>`valueType` `string?` Optional<br>`isActive` `boolean?` Optional | None | None | None | Envelope: `ApiResponse<IReadOnlyCollection<SystemConfigurationResponse>>`<br>Data schema: Array of [`SystemConfigurationResponse`](#schema-systemconfigurationresponse)<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Authorize(Policy = PermissionNames.ConfigurationRead) Omit null/empty query values; client utilities in web/mobile already serialize only populated keys. | See referenced request/response schemas and related route families in the same module. | Use JSON camelCase field names exactly as defined in the schema registry below. |
| Configuration & Master Data | SystemConfigurationController | GetSystemConfigurationDetail | GET | /api/system-configurations/{systemConfigurationId:long} | Fetch system configuration detail data for `/api/system-configurations/{systemConfigurationId:long}`. | Admin | Authorize(Policy = PermissionNames.ConfigurationRead) | None | `systemConfigurationId` `integer(int64)` Required | None | None | Envelope: `ApiResponse<SystemConfigurationResponse>`<br>Data schema: [`SystemConfigurationResponse`](#schema-systemconfigurationresponse)<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Authorize(Policy = PermissionNames.ConfigurationRead) | See referenced request/response schemas and related route families in the same module. | Use JSON camelCase field names exactly as defined in the schema registry below. |
| Configuration & Master Data | SystemConfigurationController | GetBusinessHours | GET | /api/business-hours | Fetch business hours data for `/api/business-hours`. | Admin | Authorize(Policy = PermissionNames.ConfigurationRead) | None | None | None | None | Envelope: `ApiResponse<IReadOnlyCollection<BusinessHourConfigurationResponse>>`<br>Data schema: Array of [`BusinessHourConfigurationResponse`](#schema-businesshourconfigurationresponse)<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Authorize(Policy = PermissionNames.ConfigurationRead) | See referenced request/response schemas and related route families in the same module. | Use JSON camelCase field names exactly as defined in the schema registry below. |
| Configuration & Master Data | SystemConfigurationController | SaveBusinessHours | POST | /api/business-hours | Persist submitted details for `/api/business-hours`. | Admin | Authorize(Policy = PermissionNames.ConfigurationManage) | None | None | None | [`SaveBusinessHoursRequest`](#schema-savebusinesshoursrequest)<br>Field-by-field: schema registry entry. | Envelope: `ApiResponse<IReadOnlyCollection<BusinessHourConfigurationResponse>>`<br>Data schema: Array of [`BusinessHourConfigurationResponse`](#schema-businesshourconfigurationresponse)<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Authorize(Policy = PermissionNames.ConfigurationManage) Request body fields are defined in [`SaveBusinessHoursRequest`](#schema-savebusinesshoursrequest). | See referenced request/response schemas and related route families in the same module. | Use JSON camelCase field names exactly as defined in the schema registry below. |
| Configuration & Master Data | SystemConfigurationController | GetHolidays | GET | /api/holidays | Fetch holidays data for `/api/holidays`. | Admin | Authorize(Policy = PermissionNames.ConfigurationRead) | `year` `integer(int32)?` Optional<br>`isActive` `boolean?` Optional | None | None | None | Envelope: `ApiResponse<IReadOnlyCollection<HolidayConfigurationResponse>>`<br>Data schema: Array of [`HolidayConfigurationResponse`](#schema-holidayconfigurationresponse)<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Authorize(Policy = PermissionNames.ConfigurationRead) Omit null/empty query values; client utilities in web/mobile already serialize only populated keys. | See referenced request/response schemas and related route families in the same module. | Use JSON camelCase field names exactly as defined in the schema registry below. |
| Configuration & Master Data | SystemConfigurationController | CreateHoliday | POST | /api/holidays | Create holiday for `/api/holidays`. | Admin | Authorize(Policy = PermissionNames.ConfigurationManage) | None | None | None | [`CreateHolidayConfigurationRequest`](#schema-createholidayconfigurationrequest)<br>Field-by-field: schema registry entry. | Envelope: `ApiResponse<HolidayConfigurationResponse>`<br>Data schema: [`HolidayConfigurationResponse`](#schema-holidayconfigurationresponse)<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Authorize(Policy = PermissionNames.ConfigurationManage) Request body fields are defined in [`CreateHolidayConfigurationRequest`](#schema-createholidayconfigurationrequest). | See referenced request/response schemas and related route families in the same module. | Use JSON camelCase field names exactly as defined in the schema registry below. |

## 4. Booking APIs

| Module | API Group / Controller | API Name | HTTP | Route | Purpose / Description | Used By | Authentication Requirement | Query Parameters | Path Parameters | Headers | Request Body / Field Definition | Response Structure / Field Definition | Success / Error Example | Validation / Business Rule Notes | Dependency / Linked APIs | Integration Notes |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| Service Booking Engine | BookingController | CreateGuestBooking | POST | /api/bookings/guest | Create a guest booking without a logged-in customer session. | Customer | Anonymous | None | None | `X-Idempotency-Key` `string?` Optional | [`GuestBookingCreateRequest`](#schema-guestbookingcreaterequest)<br>Field-by-field: schema registry entry. | Envelope: `ApiResponse<BookingSummaryResponse>`<br>Data schema: [`BookingSummaryResponse`](#schema-bookingsummaryresponse)<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Anonymous Request body fields are defined in [`GuestBookingCreateRequest`](#schema-guestbookingcreaterequest). | `GET /booking-lookups/*` feeds booking create; bookings may later link to `/service-requests/from-booking/{bookingId}`. | Frontend should send `X-Idempotency-Key` for create retries; request body uses JSON camelCase. |
| Service Booking Engine | BookingController | CreateCustomerBooking | POST | /api/bookings/customer | Create a booking for the authenticated customer context. | Customer | Authorize | None | None | `X-Idempotency-Key` `string?` Optional | [`CustomerBookingCreateRequest`](#schema-customerbookingcreaterequest)<br>Field-by-field: schema registry entry. | Envelope: `ApiResponse<BookingSummaryResponse>`<br>Data schema: [`BookingSummaryResponse`](#schema-bookingsummaryresponse)<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Authorize Request body fields are defined in [`CustomerBookingCreateRequest`](#schema-customerbookingcreaterequest). | `GET /booking-lookups/*` feeds booking create; bookings may later link to `/service-requests/from-booking/{bookingId}`. | Frontend should send `X-Idempotency-Key` for create retries; request body uses JSON camelCase. |
| Service Booking Engine | BookingController | GetBookingById | GET | /api/bookings/{bookingId:long} | Return booking detail for the supplied booking id. | Admin | Authorize(Policy = PermissionNames.BookingRead) | None | `bookingId` `integer(int64)` Required | None | None | Envelope: `ApiResponse<BookingDetailResponse>`<br>Data schema: [`BookingDetailResponse`](#schema-bookingdetailresponse)<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Authorize(Policy = PermissionNames.BookingRead) | `GET /booking-lookups/*` feeds booking create; bookings may later link to `/service-requests/from-booking/{bookingId}`. | Use JSON camelCase field names exactly as defined in the schema registry below. |
| Service Booking Engine | BookingController | GetMyBookings | GET | /api/bookings/my-bookings | Return the authenticated customer booking list. | Customer | Authorize | `pageNumber` `integer(int32)` Required<br>`pageSize` `integer(int32)` Required | None | None | None | Envelope: `ApiResponse<PagedResult<BookingListItemResponse>>`<br>Data schema: `PagedResult` of [`BookingListItemResponse`](#schema-bookinglistitemresponse) with `items`, `totalCount`, `pageNumber`, `pageSize`<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Authorize Omit null/empty query values; client utilities in web/mobile already serialize only populated keys. | `GET /booking-lookups/*` feeds booking create; bookings may later link to `/service-requests/from-booking/{bookingId}`. | Use JSON camelCase field names exactly as defined in the schema registry below. |
| Service Booking Engine | BookingController | Search | GET | /api/bookings | Search records using the supplied filter set for this route. | Admin | Authorize(Policy = PermissionNames.BookingRead) | `bookingReference` `string?` Optional<br>`customerMobile` `string?` Optional<br>`bookingDate` `date?` Optional<br>`serviceId` `integer(int64)?` Optional<br>`pageNumber` `integer(int32)` Required<br>`pageSize` `integer(int32)` Required | None | None | None | Envelope: `ApiResponse<PagedResult<BookingListItemResponse>>`<br>Data schema: `PagedResult` of [`BookingListItemResponse`](#schema-bookinglistitemresponse) with `items`, `totalCount`, `pageNumber`, `pageSize`<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Authorize(Policy = PermissionNames.BookingRead) Omit null/empty query values; client utilities in web/mobile already serialize only populated keys. | `GET /booking-lookups/*` feeds booking create; bookings may later link to `/service-requests/from-booking/{bookingId}`. | Use JSON camelCase field names exactly as defined in the schema registry below. |
| Customer Account Portal / Service Booking Engine | CancellationController | CreateCustomerCancellation | POST | /api/cancellations/customer | Create customer cancellation for `/api/cancellations/customer`. | Customer | Authorize | None | None | None | [`CreateCustomerCancellationRequest`](#schema-createcustomercancellationrequest)<br>Field-by-field: schema registry entry. | Envelope: `ApiResponse<CancellationDetailResponse>`<br>Data schema: [`CancellationDetailResponse`](#schema-cancellationdetailresponse)<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Authorize Request body fields are defined in [`CreateCustomerCancellationRequest`](#schema-createcustomercancellationrequest). | Often depends on booking/service-request detail and can link to `/refunds/{id}`. | Use JSON camelCase field names exactly as defined in the schema registry below. |
| Customer Account Portal / Service Booking Engine | CancellationController | CreateAdminCancellation | POST | /api/cancellations/admin | Create admin cancellation for `/api/cancellations/admin`. | Admin | Authorize(Policy = PermissionNames.ServiceRequestUpdate) | None | None | None | [`CreateAdminCancellationRequest`](#schema-createadmincancellationrequest)<br>Field-by-field: schema registry entry. | Envelope: `ApiResponse<CancellationDetailResponse>`<br>Data schema: [`CancellationDetailResponse`](#schema-cancellationdetailresponse)<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Authorize(Policy = PermissionNames.ServiceRequestUpdate) Request body fields are defined in [`CreateAdminCancellationRequest`](#schema-createadmincancellationrequest). | Often depends on booking/service-request detail and can link to `/refunds/{id}`. | Use JSON camelCase field names exactly as defined in the schema registry below. |
| Customer Account Portal / Service Booking Engine | CancellationController | GetCancellationOptions | GET | /api/cancellations/options/{serviceRequestId:long} | Fetch cancellation options data for `/api/cancellations/options/{serviceRequestId:long}`. | Both | Authorize | `bookingId` `integer(int64)?` Optional | `serviceRequestId` `integer(int64)` Required | None | None | Envelope: `ApiResponse<CancellationOptionsResponse>`<br>Data schema: [`CancellationOptionsResponse`](#schema-cancellationoptionsresponse)<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Authorize Omit null/empty query values; client utilities in web/mobile already serialize only populated keys. | Often depends on booking/service-request detail and can link to `/refunds/{id}`. | Use JSON camelCase field names exactly as defined in the schema registry below. |
| Customer Account Portal / Service Booking Engine | CancellationController | GetCancellations | GET | /api/cancellations | Fetch cancellations data for `/api/cancellations`. | Both | Authorize | `bookingId` `integer(int64)?` Optional<br>`serviceRequestId` `integer(int64)?` Optional<br>`cancellationStatus` `string?` Optional<br>`cancellationSource` `string?` Optional<br>`cancellationReasonCode` `string?` Optional<br>`branchId` `integer(int32)?` Optional<br>`fromDateUtc` `datetime?` Optional<br>`toDateUtc` `datetime?` Optional | None | None | None | Envelope: `ApiResponse<IReadOnlyCollection<CancellationListItemResponse>>`<br>Data schema: Array of [`CancellationListItemResponse`](#schema-cancellationlistitemresponse)<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Authorize Omit null/empty query values; client utilities in web/mobile already serialize only populated keys. | Often depends on booking/service-request detail and can link to `/refunds/{id}`. | Use JSON camelCase field names exactly as defined in the schema registry below. |
| Customer Account Portal / Service Booking Engine | CancellationController | GetCancellationById | GET | /api/cancellations/{id:long} | Fetch cancellation by id data for `/api/cancellations/{id:long}`. | Both | Authorize | None | `id` `integer(int64)` Required | None | None | Envelope: `ApiResponse<CancellationDetailResponse>`<br>Data schema: [`CancellationDetailResponse`](#schema-cancellationdetailresponse)<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Authorize | Often depends on booking/service-request detail and can link to `/refunds/{id}`. | Use JSON camelCase field names exactly as defined in the schema registry below. |
| Customer Account Portal / Service Booking Engine | CancellationController | CancelServiceRequest | POST | /api/cancellations/service-requests/{serviceRequestId:long} | Execute `CancelServiceRequest` for `/api/cancellations/service-requests/{serviceRequestId:long}`. | Admin | Authorize(Policy = PermissionNames.ServiceRequestUpdate) | None | `serviceRequestId` `integer(int64)` Required | None | [`CancelServiceRequestRequest`](#schema-cancelservicerequestrequest)<br>Field-by-field: schema registry entry. | Envelope: `ApiResponse<CancellationRecordResponse>`<br>Data schema: [`CancellationRecordResponse`](#schema-cancellationrecordresponse)<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Authorize(Policy = PermissionNames.ServiceRequestUpdate) Request body fields are defined in [`CancelServiceRequestRequest`](#schema-cancelservicerequestrequest). | Often depends on booking/service-request detail and can link to `/refunds/{id}`. | Use JSON camelCase field names exactly as defined in the schema registry below. |
| Customer Account Portal / Service Booking Engine | CustomerBookingController | GetCustomerBookingById | GET | /api/customer-bookings/{bookingId:long} | Return booking detail limited to the authenticated customer context. | Customer | Authorize | None | `bookingId` `integer(int64)` Required | None | None | Envelope: `ApiResponse<BookingDetailResponse>`<br>Data schema: [`BookingDetailResponse`](#schema-bookingdetailresponse)<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Authorize | Usually paired with `/cancellations/options/{serviceRequestId}` and `/support-tickets` from customer surfaces. | Use JSON camelCase field names exactly as defined in the schema registry below. |
| Customer Account Portal / Service Booking Engine | CustomerBookingController | GetCustomerBookings | GET | /api/customer-bookings | Return customer booking history for the authenticated customer context. | Customer | Authorize | `pageNumber` `integer(int32)` Required<br>`pageSize` `integer(int32)` Required | None | None | None | Envelope: `ApiResponse<PagedResult<BookingListItemResponse>>`<br>Data schema: `PagedResult` of [`BookingListItemResponse`](#schema-bookinglistitemresponse) with `items`, `totalCount`, `pageNumber`, `pageSize`<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Authorize Omit null/empty query values; client utilities in web/mobile already serialize only populated keys. | Usually paired with `/cancellations/options/{serviceRequestId}` and `/support-tickets` from customer surfaces. | Use JSON camelCase field names exactly as defined in the schema registry below. |
| Customer Account Portal / Service Booking Engine | CustomerBookingController | RescheduleCustomerBooking | POST | /api/customer-bookings/{bookingId:long}/reschedule | Reschedule a booking for the authenticated customer context. | Customer | Authorize | None | `bookingId` `integer(int64)` Required | None | [`RescheduleCustomerBookingRequest`](#schema-reschedulecustomerbookingrequest)<br>Field-by-field: schema registry entry. | Envelope: `ApiResponse<BookingDetailResponse>`<br>Data schema: [`BookingDetailResponse`](#schema-bookingdetailresponse)<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Authorize Request body fields are defined in [`RescheduleCustomerBookingRequest`](#schema-reschedulecustomerbookingrequest). | Validate slot availability through `/booking-lookups/slots` before submission. | Server enforces customer ownership for the target booking. |
| Customer Account Portal / Service Booking Engine | CustomerBookingController | GetServiceReport | GET | /api/customer-bookings/{bookingId:long}/service-report | Return the customer-visible service report for a booking. | Customer | Authorize | None | `bookingId` `integer(int64)` Required | None | None | Envelope: `ApiResponse<BookingDetailResponse>`<br>Data schema: [`BookingDetailResponse`](#schema-bookingdetailresponse)<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Authorize | Usually follows completed booking/job flows and reuses booking detail payload shape. | Server enforces customer ownership for the target booking. |

## 5. Service Request APIs

| Module | API Group / Controller | API Name | HTTP | Route | Purpose / Description | Used By | Authentication Requirement | Query Parameters | Path Parameters | Headers | Request Body / Field Definition | Response Structure / Field Definition | Success / Error Example | Validation / Business Rule Notes | Dependency / Linked APIs | Integration Notes |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| Dispatch, Scheduling & Work Orders | AssignmentController | Assign | POST | /api/service-requests/{serviceRequestId:long}/assign | Execute `Assign` for `/api/service-requests/{serviceRequestId:long}/assign`. | Admin | Authorize(Policy = PermissionNames.AssignmentManage) | None | `serviceRequestId` `integer(int64)` Required | None | [`AssignTechnicianRequest`](#schema-assigntechnicianrequest)<br>Field-by-field: schema registry entry. | Envelope: `ApiResponse<ServiceRequestDetailResponse>`<br>Data schema: [`ServiceRequestDetailResponse`](#schema-servicerequestdetailresponse)<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Authorize(Policy = PermissionNames.AssignmentManage) Request body fields are defined in [`AssignTechnicianRequest`](#schema-assigntechnicianrequest). | Typically chained with `/technicians/availability`, assignment history, technician execution APIs, and billing/quotation APIs. | Use JSON camelCase field names exactly as defined in the schema registry below. |
| Dispatch, Scheduling & Work Orders | AssignmentController | Reassign | POST | /api/service-requests/{serviceRequestId:long}/reassign | Execute `Reassign` for `/api/service-requests/{serviceRequestId:long}/reassign`. | Admin | Authorize(Policy = PermissionNames.AssignmentManage) | None | `serviceRequestId` `integer(int64)` Required | None | [`ReassignTechnicianRequest`](#schema-reassigntechnicianrequest)<br>Field-by-field: schema registry entry. | Envelope: `ApiResponse<ServiceRequestDetailResponse>`<br>Data schema: [`ServiceRequestDetailResponse`](#schema-servicerequestdetailresponse)<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Authorize(Policy = PermissionNames.AssignmentManage) Request body fields are defined in [`ReassignTechnicianRequest`](#schema-reassigntechnicianrequest). | Typically chained with `/technicians/availability`, assignment history, technician execution APIs, and billing/quotation APIs. | Use JSON camelCase field names exactly as defined in the schema registry below. |
| Dispatch, Scheduling & Work Orders | AssignmentController | GetAssignmentHistory | GET | /api/service-requests/{serviceRequestId:long}/assignment-history | Fetch assignment history data for `/api/service-requests/{serviceRequestId:long}/assignment-history`. | Admin | Authorize(Policy = PermissionNames.ServiceRequestRead) | None | `serviceRequestId` `integer(int64)` Required | None | None | Envelope: `ApiResponse<IReadOnlyCollection<AssignmentHistoryItemResponse>>`<br>Data schema: Array of [`AssignmentHistoryItemResponse`](#schema-assignmenthistoryitemresponse)<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Authorize(Policy = PermissionNames.ServiceRequestRead) | Typically chained with `/technicians/availability`, assignment history, technician execution APIs, and billing/quotation APIs. | Use JSON camelCase field names exactly as defined in the schema registry below. |
| Dispatch, Scheduling & Work Orders / Technician Mobile Workflow | CustomerAbsentController | Mark | POST | /api/customer-absent/{serviceRequestId:long}/mark | Execute `Mark` for `/api/customer-absent/{serviceRequestId:long}/mark`. | Internal | Authorize(Roles = RoleNames.Technician) | None | `serviceRequestId` `integer(int64)` Required | None | [`MarkCustomerAbsentRequest`](#schema-markcustomerabsentrequest)<br>Field-by-field: schema registry entry. | Envelope: `ApiResponse<CustomerAbsentDetailResponse>`<br>Data schema: [`CustomerAbsentDetailResponse`](#schema-customerabsentdetailresponse)<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Authorize(Roles = RoleNames.Technician) Request body fields are defined in [`MarkCustomerAbsentRequest`](#schema-markcustomerabsentrequest). | See referenced request/response schemas and related route families in the same module. | Use JSON camelCase field names exactly as defined in the schema registry below. |
| Dispatch, Scheduling & Work Orders / Technician Mobile Workflow | CustomerAbsentController | Reschedule | POST | /api/customer-absent/{serviceRequestId:long}/reschedule | Execute `Reschedule` for `/api/customer-absent/{serviceRequestId:long}/reschedule`. | Admin | Authorize(Policy = PermissionNames.ServiceRequestUpdate) | None | `serviceRequestId` `integer(int64)` Required | None | [`ResolveCustomerAbsentRequest`](#schema-resolvecustomerabsentrequest)<br>Field-by-field: schema registry entry. | Envelope: `ApiResponse<CustomerAbsentDetailResponse>`<br>Data schema: [`CustomerAbsentDetailResponse`](#schema-customerabsentdetailresponse)<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Authorize(Policy = PermissionNames.ServiceRequestUpdate) Request body fields are defined in [`ResolveCustomerAbsentRequest`](#schema-resolvecustomerabsentrequest). | See referenced request/response schemas and related route families in the same module. | Use JSON camelCase field names exactly as defined in the schema registry below. |
| Dispatch, Scheduling & Work Orders / Technician Mobile Workflow | CustomerAbsentController | Cancel | POST | /api/customer-absent/{serviceRequestId:long}/cancel | Execute `Cancel` for `/api/customer-absent/{serviceRequestId:long}/cancel`. | Admin | Authorize(Policy = PermissionNames.ServiceRequestUpdate) | None | `serviceRequestId` `integer(int64)` Required | None | [`CancelCustomerAbsentRequest`](#schema-cancelcustomerabsentrequest)<br>Field-by-field: schema registry entry. | Envelope: `ApiResponse<CancellationDetailResponse>`<br>Data schema: [`CancellationDetailResponse`](#schema-cancellationdetailresponse)<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Authorize(Policy = PermissionNames.ServiceRequestUpdate) Request body fields are defined in [`CancelCustomerAbsentRequest`](#schema-cancelcustomerabsentrequest). | See referenced request/response schemas and related route families in the same module. | Use JSON camelCase field names exactly as defined in the schema registry below. |
| Dispatch, Scheduling & Work Orders / Technician Mobile Workflow | CustomerAbsentController | GetByServiceRequestId | GET | /api/customer-absent/{serviceRequestId:long} | Fetch by service request id data for `/api/customer-absent/{serviceRequestId:long}`. | Both | Authorize | None | `serviceRequestId` `integer(int64)` Required | None | None | Envelope: `ApiResponse<CustomerAbsentDetailResponse>`<br>Data schema: [`CustomerAbsentDetailResponse`](#schema-customerabsentdetailresponse)<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Authorize | See referenced request/response schemas and related route families in the same module. | Use JSON camelCase field names exactly as defined in the schema registry below. |
| Customer Support & Feedback / Service Request Management | EscalationController | Create | POST | /api/escalations | Execute `Create` for `/api/escalations`. | Admin | Authorize(Policy = PermissionNames.SupportManage) | None | None | None | [`CreateEscalationRequest`](#schema-createescalationrequest)<br>Field-by-field: schema registry entry. | Envelope: `ApiResponse<EscalationResponse>`<br>Data schema: [`EscalationResponse`](#schema-escalationresponse)<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Authorize(Policy = PermissionNames.SupportManage) Request body fields are defined in [`CreateEscalationRequest`](#schema-createescalationrequest). | See referenced request/response schemas and related route families in the same module. | Use JSON camelCase field names exactly as defined in the schema registry below. |
| Customer Support & Feedback / Service Request Management | EscalationController | HandleNoShow | POST | /api/escalations/service-requests/{serviceRequestId:long}/no-show | Execute `HandleNoShow` for `/api/escalations/service-requests/{serviceRequestId:long}/no-show`. | Admin | Authorize(Policy = PermissionNames.SupportManage) | None | `serviceRequestId` `integer(int64)` Required | None | [`HandleNoShowRequest`](#schema-handlenoshowrequest)<br>Field-by-field: schema registry entry. | Envelope: `ApiResponse<ServiceRequestDetailResponse>`<br>Data schema: [`ServiceRequestDetailResponse`](#schema-servicerequestdetailresponse)<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Authorize(Policy = PermissionNames.SupportManage) Request body fields are defined in [`HandleNoShowRequest`](#schema-handlenoshowrequest). | See referenced request/response schemas and related route families in the same module. | Use JSON camelCase field names exactly as defined in the schema registry below. |
| Service Request Management | ServiceRequestController | CreateFromBooking | POST | /api/service-requests/from-booking/{bookingId:long} | Create from booking for `/api/service-requests/from-booking/{bookingId:long}`. | Admin | Authorize(Policy = PermissionNames.ServiceRequestCreate) | None | `bookingId` `integer(int64)` Required | None | None | Envelope: `ApiResponse<ServiceRequestDetailResponse>`<br>Data schema: [`ServiceRequestDetailResponse`](#schema-servicerequestdetailresponse)<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Authorize(Policy = PermissionNames.ServiceRequestCreate) | Typically chained with `/technicians/availability`, assignment history, technician execution APIs, and billing/quotation APIs. | Use JSON camelCase field names exactly as defined in the schema registry below. |
| Service Request Management | ServiceRequestController | GetServiceRequests | GET | /api/service-requests | Fetch service requests data for `/api/service-requests`. | Admin | Authorize(Policy = PermissionNames.ServiceRequestRead) | `bookingId` `integer(int64)?` Optional<br>`serviceId` `integer(int64)?` Optional<br>`status` `string?` Optional<br>`slotDate` `date?` Optional<br>`pageNumber` `integer(int32)` Required<br>`pageSize` `integer(int32)` Required | None | None | None | Envelope: `ApiResponse<PagedResult<ServiceRequestListItemResponse>>`<br>Data schema: `PagedResult` of [`ServiceRequestListItemResponse`](#schema-servicerequestlistitemresponse) with `items`, `totalCount`, `pageNumber`, `pageSize`<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Authorize(Policy = PermissionNames.ServiceRequestRead) Omit null/empty query values; client utilities in web/mobile already serialize only populated keys. | Typically chained with `/technicians/availability`, assignment history, technician execution APIs, and billing/quotation APIs. | Use JSON camelCase field names exactly as defined in the schema registry below. |
| Service Request Management | ServiceRequestController | GetServiceRequestById | GET | /api/service-requests/{serviceRequestId:long} | Fetch service request by id data for `/api/service-requests/{serviceRequestId:long}`. | Admin | Authorize(Policy = PermissionNames.ServiceRequestRead) | None | `serviceRequestId` `integer(int64)` Required | None | None | Envelope: `ApiResponse<ServiceRequestDetailResponse>`<br>Data schema: [`ServiceRequestDetailResponse`](#schema-servicerequestdetailresponse)<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Authorize(Policy = PermissionNames.ServiceRequestRead) | Typically chained with `/technicians/availability`, assignment history, technician execution APIs, and billing/quotation APIs. | Use JSON camelCase field names exactly as defined in the schema registry below. |
| Service Request Management | ServiceRequestController | UpdateStatus | POST | /api/service-requests/{serviceRequestId:long}/status | Update status for `/api/service-requests/{serviceRequestId:long}/status`. | Admin | Authorize(Policy = PermissionNames.ServiceRequestUpdate) | None | `serviceRequestId` `integer(int64)` Required | None | [`UpdateServiceRequestStatusRequest`](#schema-updateservicerequeststatusrequest)<br>Field-by-field: schema registry entry. | Envelope: `ApiResponse<ServiceRequestDetailResponse>`<br>Data schema: [`ServiceRequestDetailResponse`](#schema-servicerequestdetailresponse)<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Authorize(Policy = PermissionNames.ServiceRequestUpdate) Request body fields are defined in [`UpdateServiceRequestStatusRequest`](#schema-updateservicerequeststatusrequest). | Typically chained with `/technicians/availability`, assignment history, technician execution APIs, and billing/quotation APIs. | Use JSON camelCase field names exactly as defined in the schema registry below. |

## 6. Customer Management APIs

| Module | API Group / Controller | API Name | HTTP | Route | Purpose / Description | Used By | Authentication Requirement | Query Parameters | Path Parameters | Headers | Request Body / Field Definition | Response Structure / Field Definition | Success / Error Example | Validation / Business Rule Notes | Dependency / Linked APIs | Integration Notes |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| Customer Account Portal | CustomerAddressController | GetMyAddresses | GET | /api/customers/me/addresses | Return saved addresses for the authenticated customer. | Customer | Authorize | None | None | None | None | Envelope: `ApiResponse<IReadOnlyCollection<CustomerAddressResponse>>`<br>Data schema: Array of [`CustomerAddressResponse`](#schema-customeraddressresponse)<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Authorize | Address records are used by customer booking and profile flows. | Server derives the customer from the bearer token; clients do not send `customerId`. |
| Customer Account Portal | CustomerAddressController | CreateAddress | POST | /api/customers/me/addresses | Create a saved address for the authenticated customer. | Customer | Authorize | None | None | None | [`CreateCustomerAddressRequest`](#schema-createcustomeraddressrequest)<br>Field-by-field: schema registry entry. | Envelope: `ApiResponse<CustomerAddressResponse>`<br>Data schema: [`CustomerAddressResponse`](#schema-customeraddressresponse)<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Authorize Request body fields are defined in [`CreateCustomerAddressRequest`](#schema-createcustomeraddressrequest). | Use `/booking-lookups/zones/by-pincode/{pincode}` when resolving service zones. | Use JSON camelCase field names exactly as defined in the schema registry below. |
| Customer Account Portal | CustomerAddressController | UpdateAddress | PUT | /api/customers/me/addresses/{addressId:long} | Update a saved address for the authenticated customer. | Customer | Authorize | None | `addressId` `integer(int64)` Required | None | [`UpdateCustomerAddressRequest`](#schema-updatecustomeraddressrequest)<br>Field-by-field: schema registry entry. | Envelope: `ApiResponse<CustomerAddressResponse>`<br>Data schema: [`CustomerAddressResponse`](#schema-customeraddressresponse)<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Authorize Request body fields are defined in [`UpdateCustomerAddressRequest`](#schema-updatecustomeraddressrequest). | Address ownership is validated server-side. | Route `addressId` is the authoritative target identifier. |
| Customer Account Portal | CustomerAddressController | DeleteAddress | DELETE | /api/customers/me/addresses/{addressId:long} | Delete a saved address for the authenticated customer. | Customer | Authorize | None | `addressId` `integer(int64)` Required | None | None | Envelope: `ApiResponse<object>`<br>Data fields: `addressId` `integer(int64)`. | Success: standard envelope + listed data fields.<br>Error: standard error envelope in Section 16. | Authorize | Address ownership is validated server-side. | Client should remove the address locally only after success. |
| Booking / Technician | CustomerTechnicianController | GetTechnician | GET | /api/customer-technicians/{technicianId:long} | Return customer-visible technician profile details. | Customer | Authorize | None | `technicianId` `integer(int64)` Required | None | None | Envelope: `ApiResponse<CustomerVisibleTechnicianResponse>`<br>Data schema: [`CustomerVisibleTechnicianResponse`](#schema-customervisibletechnicianresponse)<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Authorize | Usually linked from booking/job tracking surfaces. | Only customer-safe technician fields are exposed. |
| Customer, Equipment & Warranty | ServiceHistoryController | GetByCustomer | GET | /api/service-history/customer/{customerId:long} | Fetch by customer data for `/api/service-history/customer/{customerId:long}`. | Both | Authorize | None | `customerId` `integer(int64)` Required | None | None | Envelope: `ApiResponse<IReadOnlyCollection<ServiceHistoryItemResponse>>`<br>Data schema: Array of [`ServiceHistoryItemResponse`](#schema-servicehistoryitemresponse)<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Authorize | See referenced request/response schemas and related route families in the same module. | Use JSON camelCase field names exactly as defined in the schema registry below. |
| Customer, Equipment & Warranty | ServiceHistoryController | GetForCurrentCustomer | GET | /api/service-history/me | Fetch for current customer data for `/api/service-history/me`. | Customer | Authorize | None | None | None | None | Envelope: `ApiResponse<IReadOnlyCollection<ServiceHistoryItemResponse>>`<br>Data schema: Array of [`ServiceHistoryItemResponse`](#schema-servicehistoryitemresponse)<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Authorize | See referenced request/response schemas and related route families in the same module. | Use JSON camelCase field names exactly as defined in the schema registry below. |

## 7. Equipment / AMC APIs

| Module | API Group / Controller | API Name | HTTP | Route | Purpose / Description | Used By | Authentication Requirement | Query Parameters | Path Parameters | Headers | Request Body / Field Definition | Response Structure / Field Definition | Success / Error Example | Validation / Business Rule Notes | Dependency / Linked APIs | Integration Notes |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| Customer, Equipment & Warranty | CustomerEquipmentController | GetMyEquipment | GET | /api/customers/me/equipment | Return registered equipment for the authenticated customer. | Customer | Authorize | None | None | None | None | Envelope: `ApiResponse<IReadOnlyCollection<CustomerEquipmentResponse>>`<br>Data schema: Array of [`CustomerEquipmentResponse`](#schema-customerequipmentresponse)<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Authorize | Equipment records are used by AMC, warranty, and booking/service-history flows. | Server derives the customer from the bearer token; clients do not send `customerId`. |
| Customer, Equipment & Warranty | CustomerEquipmentController | CreateEquipment | POST | /api/customers/me/equipment | Register customer equipment. | Customer | Authorize | None | None | None | [`CreateCustomerEquipmentRequest`](#schema-createcustomerequipmentrequest)<br>Field-by-field: schema registry entry. | Envelope: `ApiResponse<CustomerEquipmentResponse>`<br>Data schema: [`CustomerEquipmentResponse`](#schema-customerequipmentresponse)<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Authorize Request body fields are defined in [`CreateCustomerEquipmentRequest`](#schema-createcustomerequipmentrequest). | Equipment may later be linked to service history, warranty, and AMC screens. | Use JSON camelCase field names exactly as defined in the schema registry below. |
| Customer, Equipment & Warranty | CustomerEquipmentController | UpdateEquipment | PUT | /api/customers/me/equipment/{equipmentId:long} | Update customer equipment. | Customer | Authorize | None | `equipmentId` `integer(int64)` Required | None | [`UpdateCustomerEquipmentRequest`](#schema-updatecustomerequipmentrequest)<br>Field-by-field: schema registry entry. | Envelope: `ApiResponse<CustomerEquipmentResponse>`<br>Data schema: [`CustomerEquipmentResponse`](#schema-customerequipmentresponse)<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Authorize Request body fields are defined in [`UpdateCustomerEquipmentRequest`](#schema-updatecustomerequipmentrequest). | Equipment ownership is validated server-side. | Route `equipmentId` is the authoritative target identifier. |
| Customer, Equipment & Warranty | CustomerEquipmentController | DeleteEquipment | DELETE | /api/customers/me/equipment/{equipmentId:long} | Delete customer equipment. | Customer | Authorize | None | `equipmentId` `integer(int64)` Required | None | None | Envelope: `ApiResponse<object>`<br>Data fields: `equipmentId` `integer(int64)`. | Success: standard envelope + listed data fields.<br>Error: standard error envelope in Section 16. | Authorize | Equipment ownership is validated server-side. | Client should remove the equipment locally only after success. |
| AMC Contract & Portal | AmcController | CreatePlan | POST | /api/amc/plans | Create plan for `/api/amc/plans`. | Admin | Authorize(Policy = PermissionNames.AmcCreate) | None | None | None | [`CreateAmcPlanRequest`](#schema-createamcplanrequest)<br>Field-by-field: schema registry entry. | Envelope: `ApiResponse<AmcPlanResponse>`<br>Data schema: [`AmcPlanResponse`](#schema-amcplanresponse)<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Authorize(Policy = PermissionNames.AmcCreate) Request body fields are defined in [`CreateAmcPlanRequest`](#schema-createamcplanrequest). | AMC detail commonly pairs with `/service-history/me`, `/warranty/*`, and `/revisit/*`. | Use JSON camelCase field names exactly as defined in the schema registry below. |
| AMC Contract & Portal | AmcController | UpdatePlan | PUT | /api/amc/plans/{amcPlanId:long} | Update plan for `/api/amc/plans/{amcPlanId:long}`. | Admin | Authorize(Policy = PermissionNames.AmcCreate) | None | `amcPlanId` `integer(int64)` Required | None | [`UpdateAmcPlanRequest`](#schema-updateamcplanrequest)<br>Field-by-field: schema registry entry. | Envelope: `ApiResponse<AmcPlanResponse>`<br>Data schema: [`AmcPlanResponse`](#schema-amcplanresponse)<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Authorize(Policy = PermissionNames.AmcCreate) Request body fields are defined in [`UpdateAmcPlanRequest`](#schema-updateamcplanrequest). | AMC detail commonly pairs with `/service-history/me`, `/warranty/*`, and `/revisit/*`. | Use JSON camelCase field names exactly as defined in the schema registry below. |
| AMC Contract & Portal | AmcController | GetPlans | GET | /api/amc/plans | Fetch plans data for `/api/amc/plans`. | Both | Authorize | `isActive` `boolean?` Optional<br>`pageNumber` `integer(int32)` Required<br>`pageSize` `integer(int32)` Required | None | None | None | Envelope: `ApiResponse<PagedResult<AmcPlanResponse>>`<br>Data schema: `PagedResult` of [`AmcPlanResponse`](#schema-amcplanresponse) with `items`, `totalCount`, `pageNumber`, `pageSize`<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Authorize Omit null/empty query values; client utilities in web/mobile already serialize only populated keys. | AMC detail commonly pairs with `/service-history/me`, `/warranty/*`, and `/revisit/*`. | Use JSON camelCase field names exactly as defined in the schema registry below. |
| AMC Contract & Portal | AmcController | Assign | POST | /api/amc/assign | Execute `Assign` for `/api/amc/assign`. | Admin | Authorize(Policy = PermissionNames.AmcAssign) | None | None | None | [`AssignAmcToCustomerRequest`](#schema-assignamctocustomerrequest)<br>Field-by-field: schema registry entry. | Envelope: `ApiResponse<CustomerAmcResponse>`<br>Data schema: [`CustomerAmcResponse`](#schema-customeramcresponse)<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Authorize(Policy = PermissionNames.AmcAssign) Request body fields are defined in [`AssignAmcToCustomerRequest`](#schema-assignamctocustomerrequest). | AMC detail commonly pairs with `/service-history/me`, `/warranty/*`, and `/revisit/*`. | Use JSON camelCase field names exactly as defined in the schema registry below. |
| AMC Contract & Portal | AmcController | GenerateVisits | POST | /api/amc/customer/{customerAmcId:long}/generate-visits | Execute `GenerateVisits` for `/api/amc/customer/{customerAmcId:long}/generate-visits`. | Admin | Authorize(Policy = PermissionNames.AmcAssign) | None | `customerAmcId` `integer(int64)` Required | None | None | Envelope: `ApiResponse<CustomerAmcResponse>`<br>Data schema: [`CustomerAmcResponse`](#schema-customeramcresponse)<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Authorize(Policy = PermissionNames.AmcAssign) | AMC detail commonly pairs with `/service-history/me`, `/warranty/*`, and `/revisit/*`. | Use JSON camelCase field names exactly as defined in the schema registry below. |
| AMC Contract & Portal | AmcController | GetCustomerSubscriptions | GET | /api/amc/customer/{customerId:long} | Fetch customer subscriptions data for `/api/amc/customer/{customerId:long}`. | Admin | Authorize | None | `customerId` `integer(int64)` Required | None | None | Envelope: `ApiResponse<IReadOnlyCollection<CustomerAmcResponse>>`<br>Data schema: Array of [`CustomerAmcResponse`](#schema-customeramcresponse)<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Authorize | AMC detail commonly pairs with `/service-history/me`, `/warranty/*`, and `/revisit/*`. | Use JSON camelCase field names exactly as defined in the schema registry below. |
| AMC Contract & Portal | AmcController | GetCurrentCustomerSubscriptions | GET | /api/amc/customer/me | Fetch current customer subscriptions data for `/api/amc/customer/me`. | Customer | Authorize | None | None | None | None | Envelope: `ApiResponse<IReadOnlyCollection<CustomerAmcResponse>>`<br>Data schema: Array of [`CustomerAmcResponse`](#schema-customeramcresponse)<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Authorize | AMC detail commonly pairs with `/service-history/me`, `/warranty/*`, and `/revisit/*`. | Use JSON camelCase field names exactly as defined in the schema registry below. |
| Installation Lifecycle | InstallationController | CreateInstallation | POST | /api/installations | Create installation for `/api/installations`. | Customer | Anonymous | None | None | None | [`CreateInstallationRequest`](#schema-createinstallationrequest)<br>Field-by-field: schema registry entry. | Envelope: `ApiResponse<InstallationSummaryResponse>`<br>Data schema: [`InstallationSummaryResponse`](#schema-installationsummaryresponse)<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Anonymous Request body fields are defined in [`CreateInstallationRequest`](#schema-createinstallationrequest). | See referenced request/response schemas and related route families in the same module. | Use JSON camelCase field names exactly as defined in the schema registry below. |
| Installation Lifecycle | InstallationController | GetInstallations | GET | /api/installations | Fetch installations data for `/api/installations`. | Both | Authorize | `searchTerm` `string?` Optional<br>`installationStatus` `string?` Optional<br>`approvalStatus` `string?` Optional<br>`pageNumber` `integer(int32)` Required<br>`pageSize` `integer(int32)` Required | None | None | None | Envelope: `ApiResponse<PagedResult<InstallationListItemResponse>>`<br>Data schema: `PagedResult` of [`InstallationListItemResponse`](#schema-installationlistitemresponse) with `items`, `totalCount`, `pageNumber`, `pageSize`<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Authorize Omit null/empty query values; client utilities in web/mobile already serialize only populated keys. | See referenced request/response schemas and related route families in the same module. | Use JSON camelCase field names exactly as defined in the schema registry below. |
| Installation Lifecycle | InstallationController | GetInstallationDetail | GET | /api/installations/{installationId:long} | Fetch installation detail data for `/api/installations/{installationId:long}`. | Both | Authorize | None | `installationId` `integer(int64)` Required | None | None | Envelope: `ApiResponse<InstallationDetailResponse>`<br>Data schema: [`InstallationDetailResponse`](#schema-installationdetailresponse)<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Authorize | See referenced request/response schemas and related route families in the same module. | Use JSON camelCase field names exactly as defined in the schema registry below. |
| Installation Lifecycle | InstallationController | CreateInstallationOrder | POST | /api/installations/orders | Create installation order for `/api/installations/orders`. | Both | Authorize(Policy = PermissionNames.ServiceRequestUpdate) | None | None | None | [`CreateInstallationOrderRequest`](#schema-createinstallationorderrequest)<br>Field-by-field: schema registry entry. | Envelope: `ApiResponse<InstallationOrderResponse>`<br>Data schema: [`InstallationOrderResponse`](#schema-installationorderresponse)<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Authorize(Policy = PermissionNames.ServiceRequestUpdate) Request body fields are defined in [`CreateInstallationOrderRequest`](#schema-createinstallationorderrequest). | See referenced request/response schemas and related route families in the same module. | Use JSON camelCase field names exactly as defined in the schema registry below. |
| Installation Lifecycle | InstallationController | SubmitSurveyReport | POST | /api/installations/orders/{installationOrderId:long}/survey-report | Execute `SubmitSurveyReport` for `/api/installations/orders/{installationOrderId:long}/survey-report`. | Both | No explicit controller/method authorize attribute. | None | `installationOrderId` `integer(int64)` Required | None | [`SubmitSurveyReportRequest`](#schema-submitsurveyreportrequest)<br>Field-by-field: schema registry entry. | Envelope: `ApiResponse<InstallationOrderResponse>`<br>Data schema: [`InstallationOrderResponse`](#schema-installationorderresponse)<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | No explicit controller/method authorize attribute. Request body fields are defined in [`SubmitSurveyReportRequest`](#schema-submitsurveyreportrequest). | See referenced request/response schemas and related route families in the same module. | Use JSON camelCase field names exactly as defined in the schema registry below. |
| Installation Lifecycle | InstallationController | CreateCommissioningCertificate | POST | /api/installations/orders/{installationOrderId:long}/commissioning-certificate | Create commissioning certificate for `/api/installations/orders/{installationOrderId:long}/commissioning-certificate`. | Both | No explicit controller/method authorize attribute. | None | `installationOrderId` `integer(int64)` Required | None | [`CreateCommissioningCertificateRequest`](#schema-createcommissioningcertificaterequest)<br>Field-by-field: schema registry entry. | Envelope: `ApiResponse<CommissioningCertificateResponse>`<br>Data schema: [`CommissioningCertificateResponse`](#schema-commissioningcertificateresponse)<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | No explicit controller/method authorize attribute. Request body fields are defined in [`CreateCommissioningCertificateRequest`](#schema-createcommissioningcertificaterequest). | See referenced request/response schemas and related route families in the same module. | Use JSON camelCase field names exactly as defined in the schema registry below. |
| Installation Lifecycle | InstallationExecutionController | CreateOrder | POST | /api/installations/{installationId:long}/create-order | Create order for `/api/installations/{installationId:long}/create-order`. | Internal | Authorize | None | `installationId` `integer(int64)` Required | None | [`CreateInstallationExecutionOrderRequest`](#schema-createinstallationexecutionorderrequest)<br>Field-by-field: schema registry entry. | Envelope: `ApiResponse<InstallationSummaryResponse>`<br>Data schema: [`InstallationSummaryResponse`](#schema-installationsummaryresponse)<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Authorize Request body fields are defined in [`CreateInstallationExecutionOrderRequest`](#schema-createinstallationexecutionorderrequest). | See referenced request/response schemas and related route families in the same module. | Use JSON camelCase field names exactly as defined in the schema registry below. |
| Installation Lifecycle | InstallationExecutionController | StartInstallation | POST | /api/installations/{installationId:long}/start | Execute `StartInstallation` for `/api/installations/{installationId:long}/start`. | Internal | Authorize | None | `installationId` `integer(int64)` Required | None | [`StartInstallationRequest`](#schema-startinstallationrequest)<br>Field-by-field: schema registry entry. | Envelope: `ApiResponse<InstallationSummaryResponse>`<br>Data schema: [`InstallationSummaryResponse`](#schema-installationsummaryresponse)<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Authorize Request body fields are defined in [`StartInstallationRequest`](#schema-startinstallationrequest). | See referenced request/response schemas and related route families in the same module. | Use JSON camelCase field names exactly as defined in the schema registry below. |
| Installation Lifecycle | InstallationExecutionController | CompleteInstallation | POST | /api/installations/{installationId:long}/complete | Execute `CompleteInstallation` for `/api/installations/{installationId:long}/complete`. | Internal | Authorize | None | `installationId` `integer(int64)` Required | None | [`CompleteInstallationRequest`](#schema-completeinstallationrequest)<br>Field-by-field: schema registry entry. | Envelope: `ApiResponse<InstallationSummaryResponse>`<br>Data schema: [`InstallationSummaryResponse`](#schema-installationsummaryresponse)<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Authorize Request body fields are defined in [`CompleteInstallationRequest`](#schema-completeinstallationrequest). | See referenced request/response schemas and related route families in the same module. | Use JSON camelCase field names exactly as defined in the schema registry below. |
| Installation Lifecycle | InstallationExecutionController | SaveChecklist | POST | /api/installations/{installationId:long}/checklist | Persist submitted details for `/api/installations/{installationId:long}/checklist`. | Internal | Authorize | None | `installationId` `integer(int64)` Required | None | [`SaveInstallationChecklistRequest`](#schema-saveinstallationchecklistrequest)<br>Field-by-field: schema registry entry. | Envelope: `ApiResponse<InstallationSummaryResponse>`<br>Data schema: [`InstallationSummaryResponse`](#schema-installationsummaryresponse)<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Authorize Request body fields are defined in [`SaveInstallationChecklistRequest`](#schema-saveinstallationchecklistrequest). | See referenced request/response schemas and related route families in the same module. | Use JSON camelCase field names exactly as defined in the schema registry below. |
| Installation Lifecycle | InstallationExecutionController | Commission | POST | /api/installations/{installationId:long}/commission | Execute `Commission` for `/api/installations/{installationId:long}/commission`. | Internal | Authorize | None | `installationId` `integer(int64)` Required | None | [`GenerateInstallationCommissioningRequest`](#schema-generateinstallationcommissioningrequest)<br>Field-by-field: schema registry entry. | Envelope: `ApiResponse<InstallationSummaryResponse>`<br>Data schema: [`InstallationSummaryResponse`](#schema-installationsummaryresponse)<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Authorize Request body fields are defined in [`GenerateInstallationCommissioningRequest`](#schema-generateinstallationcommissioningrequest). | See referenced request/response schemas and related route families in the same module. | Use JSON camelCase field names exactly as defined in the schema registry below. |
| Installation Lifecycle | InstallationProposalController | CreateProposal | POST | /api/installations/{installationId:long}/proposal | Create proposal for `/api/installations/{installationId:long}/proposal`. | Both | Authorize | None | `installationId` `integer(int64)` Required | None | [`CreateInstallationProposalRequest`](#schema-createinstallationproposalrequest)<br>Field-by-field: schema registry entry. | Envelope: `ApiResponse<InstallationSummaryResponse>`<br>Data schema: [`InstallationSummaryResponse`](#schema-installationsummaryresponse)<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Authorize Request body fields are defined in [`CreateInstallationProposalRequest`](#schema-createinstallationproposalrequest). | See referenced request/response schemas and related route families in the same module. | Use JSON camelCase field names exactly as defined in the schema registry below. |
| Installation Lifecycle | InstallationProposalController | ApproveProposal | POST | /api/installations/{installationId:long}/proposal/approve | Approve the target workflow for `/api/installations/{installationId:long}/proposal/approve`. | Customer | Authorize | None | `installationId` `integer(int64)` Required | None | [`ApproveInstallationProposalRequest`](#schema-approveinstallationproposalrequest)<br>Field-by-field: schema registry entry. | Envelope: `ApiResponse<InstallationSummaryResponse>`<br>Data schema: [`InstallationSummaryResponse`](#schema-installationsummaryresponse)<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Authorize Request body fields are defined in [`ApproveInstallationProposalRequest`](#schema-approveinstallationproposalrequest). | See referenced request/response schemas and related route families in the same module. | Use JSON camelCase field names exactly as defined in the schema registry below. |
| Installation Lifecycle | InstallationProposalController | RejectProposal | POST | /api/installations/{installationId:long}/proposal/reject | Reject the target workflow for `/api/installations/{installationId:long}/proposal/reject`. | Customer | Authorize | None | `installationId` `integer(int64)` Required | None | [`RejectInstallationProposalRequest`](#schema-rejectinstallationproposalrequest)<br>Field-by-field: schema registry entry. | Envelope: `ApiResponse<InstallationSummaryResponse>`<br>Data schema: [`InstallationSummaryResponse`](#schema-installationsummaryresponse)<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Authorize Request body fields are defined in [`RejectInstallationProposalRequest`](#schema-rejectinstallationproposalrequest). | See referenced request/response schemas and related route families in the same module. | Use JSON camelCase field names exactly as defined in the schema registry below. |
| Installation Lifecycle | InstallationSurveyController | ScheduleSurvey | POST | /api/installations/{installationId:long}/schedule-survey | Execute `ScheduleSurvey` for `/api/installations/{installationId:long}/schedule-survey`. | Internal | Authorize | None | `installationId` `integer(int64)` Required | None | [`ScheduleInstallationSurveyRequest`](#schema-scheduleinstallationsurveyrequest)<br>Field-by-field: schema registry entry. | Envelope: `ApiResponse<InstallationSummaryResponse>`<br>Data schema: [`InstallationSummaryResponse`](#schema-installationsummaryresponse)<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Authorize Request body fields are defined in [`ScheduleInstallationSurveyRequest`](#schema-scheduleinstallationsurveyrequest). | See referenced request/response schemas and related route families in the same module. | Use JSON camelCase field names exactly as defined in the schema registry below. |
| Installation Lifecycle | InstallationSurveyController | SubmitSurvey | POST | /api/installations/{installationId:long}/submit-survey | Execute `SubmitSurvey` for `/api/installations/{installationId:long}/submit-survey`. | Internal | Authorize | None | `installationId` `integer(int64)` Required | None | [`SubmitInstallationSurveyRequest`](#schema-submitinstallationsurveyrequest)<br>Field-by-field: schema registry entry. | Envelope: `ApiResponse<InstallationSummaryResponse>`<br>Data schema: [`InstallationSummaryResponse`](#schema-installationsummaryresponse)<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Authorize Request body fields are defined in [`SubmitInstallationSurveyRequest`](#schema-submitinstallationsurveyrequest). | See referenced request/response schemas and related route families in the same module. | Use JSON camelCase field names exactly as defined in the schema registry below. |
| AMC Contract & Portal / Customer, Equipment & Warranty | RevisitController | CreateRequest | POST | /api/revisit/request | Create request for `/api/revisit/request`. | Customer | Authorize | None | None | None | [`RevisitRequestCreateRequest`](#schema-revisitrequestcreaterequest)<br>Field-by-field: schema registry entry. | Envelope: `ApiResponse<RevisitRequestResponse>`<br>Data schema: [`RevisitRequestResponse`](#schema-revisitrequestresponse)<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Authorize Request body fields are defined in [`RevisitRequestCreateRequest`](#schema-revisitrequestcreaterequest). | See referenced request/response schemas and related route families in the same module. | Use JSON camelCase field names exactly as defined in the schema registry below. |
| AMC Contract & Portal / Customer, Equipment & Warranty | RevisitController | GetByBooking | GET | /api/revisit/booking/{bookingId:long} | Fetch by booking data for `/api/revisit/booking/{bookingId:long}`. | Customer | Authorize | None | `bookingId` `integer(int64)` Required | None | None | Envelope: `ApiResponse<IReadOnlyCollection<RevisitRequestResponse>>`<br>Data schema: Array of [`RevisitRequestResponse`](#schema-revisitrequestresponse)<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Authorize | See referenced request/response schemas and related route families in the same module. | Use JSON camelCase field names exactly as defined in the schema registry below. |
| Customer, Equipment & Warranty | WarrantyController | CreateClaim | POST | /api/warranty/claim | Create claim for `/api/warranty/claim`. | Customer | Authorize | None | None | None | [`CreateWarrantyClaimRequest`](#schema-createwarrantyclaimrequest)<br>Field-by-field: schema registry entry. | Envelope: `ApiResponse<WarrantyClaimResponse>`<br>Data schema: [`WarrantyClaimResponse`](#schema-warrantyclaimresponse)<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Authorize Request body fields are defined in [`CreateWarrantyClaimRequest`](#schema-createwarrantyclaimrequest). | See referenced request/response schemas and related route families in the same module. | Use JSON camelCase field names exactly as defined in the schema registry below. |
| Customer, Equipment & Warranty | WarrantyController | GetByInvoice | GET | /api/warranty/invoice/{invoiceId:long} | Fetch by invoice data for `/api/warranty/invoice/{invoiceId:long}`. | Customer | Authorize | None | `invoiceId` `integer(int64)` Required | None | None | Envelope: `ApiResponse<WarrantyStatusResponse>`<br>Data schema: [`WarrantyStatusResponse`](#schema-warrantystatusresponse)<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Authorize | See referenced request/response schemas and related route families in the same module. | Use JSON camelCase field names exactly as defined in the schema registry below. |

## 8. Invoice / Payment APIs

| Module | API Group / Controller | API Name | HTTP | Route | Purpose / Description | Used By | Authentication Requirement | Query Parameters | Path Parameters | Headers | Request Body / Field Definition | Response Structure / Field Definition | Success / Error Example | Validation / Business Rule Notes | Dependency / Linked APIs | Integration Notes |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| Billing, Payments & Receivables | BillingController | GetStatus | GET | /api/billing/status/{invoiceId:long} | Fetch status data for `/api/billing/status/{invoiceId:long}`. | Both | Authorize | None | `invoiceId` `integer(int64)` Required | None | None | Envelope: `ApiResponse<BillingStatusResponse>`<br>Data schema: [`BillingStatusResponse`](#schema-billingstatusresponse)<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Authorize | See referenced request/response schemas and related route families in the same module. | Use JSON camelCase field names exactly as defined in the schema registry below. |
| Billing, Payments & Receivables | InvoiceController | CreateFromQuotation | POST | /api/invoices/from-quotation/{quotationId:long} | Create from quotation for `/api/invoices/from-quotation/{quotationId:long}`. | Admin | Authorize(Policy = PermissionNames.InvoiceCreate) | None | `quotationId` `integer(int64)` Required | `X-Idempotency-Key` `string?` Optional | None | Envelope: `ApiResponse<InvoiceDetailResponse>`<br>Data schema: [`InvoiceDetailResponse`](#schema-invoicedetailresponse)<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Authorize(Policy = PermissionNames.InvoiceCreate) | Invoice detail commonly pairs with `/payments/invoice/{invoiceId}` and `/billing/status/{invoiceId}`. | Send `X-Idempotency-Key` on invoice generation retries to avoid duplicate invoices. |
| Billing, Payments & Receivables | InvoiceController | Search | GET | /api/invoices | Search records using the supplied filter set for this route. | Admin | Authorize(Policy = PermissionNames.InvoiceRead) | `status` `string?` Optional<br>`pageNumber` `integer(int32)` Required<br>`pageSize` `integer(int32)` Required | None | None | None | Envelope: `ApiResponse<PagedResult<InvoiceListItemResponse>>`<br>Data schema: `PagedResult` of [`InvoiceListItemResponse`](#schema-invoicelistitemresponse) with `items`, `totalCount`, `pageNumber`, `pageSize`<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Authorize(Policy = PermissionNames.InvoiceRead) Omit null/empty query values; client utilities in web/mobile already serialize only populated keys. | Invoice detail commonly pairs with `/payments/invoice/{invoiceId}` and `/billing/status/{invoiceId}`. | Use JSON camelCase field names exactly as defined in the schema registry below. |
| Billing, Payments & Receivables | InvoiceController | GetById | GET | /api/invoices/{id:long} | Fetch by id data for `/api/invoices/{id:long}`. | Both | Authorize | None | `id` `integer(int64)` Required | None | None | Envelope: `ApiResponse<InvoiceDetailResponse>`<br>Data schema: [`InvoiceDetailResponse`](#schema-invoicedetailresponse)<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Authorize | Invoice detail commonly pairs with `/payments/invoice/{invoiceId}` and `/billing/status/{invoiceId}`. | Use JSON camelCase field names exactly as defined in the schema registry below. |
| Billing, Payments & Receivables | InvoiceController | GetCustomerInvoices | GET | /api/invoices/customer | Fetch customer invoices data for `/api/invoices/customer`. | Customer | Authorize | `pageNumber` `integer(int32)` Required<br>`pageSize` `integer(int32)` Required | None | None | None | Envelope: `ApiResponse<PagedResult<InvoiceListItemResponse>>`<br>Data schema: `PagedResult` of [`InvoiceListItemResponse`](#schema-invoicelistitemresponse) with `items`, `totalCount`, `pageNumber`, `pageSize`<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Authorize Omit null/empty query values; client utilities in web/mobile already serialize only populated keys. | Invoice detail commonly pairs with `/payments/invoice/{invoiceId}` and `/billing/status/{invoiceId}`. | Use JSON camelCase field names exactly as defined in the schema registry below. |
| Billing, Payments & Receivables | PaymentController | Collect | POST | /api/payments/collect | Record a payment transaction against an invoice. | Both | Authorize | None | None | None | [`RecordPaymentRequest`](#schema-recordpaymentrequest)<br>Field-by-field: schema registry entry. | Envelope: `ApiResponse<PaymentTransactionResponse>`<br>Data schema: [`PaymentTransactionResponse`](#schema-paymenttransactionresponse)<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Authorize Request body fields are defined in [`RecordPaymentRequest`](#schema-recordpaymentrequest). | Payment collection updates invoice/billing status and can feed refund flows. | Customer web and admin web both consume this route; align `paymentMethod` and gateway fields before merge. |
| Billing, Payments & Receivables | PaymentController | GetByInvoice | GET | /api/payments/invoice/{invoiceId:long} | Fetch by invoice data for `/api/payments/invoice/{invoiceId:long}`. | Both | Authorize | None | `invoiceId` `integer(int64)` Required | None | None | Envelope: `ApiResponse<IReadOnlyCollection<PaymentTransactionResponse>>`<br>Data schema: Array of [`PaymentTransactionResponse`](#schema-paymenttransactionresponse)<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Authorize | Payment collection updates invoice/billing status and can feed refund flows. | Customer web and admin web both consume this route; align `paymentMethod` and gateway fields before merge. |
| Billing, Payments & Receivables / Inventory, Parts & Estimates | QuotationController | CreateFromJob | POST | /api/quotations/from-job/{jobCardId:long} | Create from job for `/api/quotations/from-job/{jobCardId:long}`. | Internal | Authorize(Roles = RoleNames.Technician) | None | `jobCardId` `integer(int64)` Required | None | [`CreateQuotationFromJobRequest`](#schema-createquotationfromjobrequest)<br>Field-by-field: schema registry entry. | Envelope: `ApiResponse<QuotationDetailResponse>`<br>Data schema: [`QuotationDetailResponse`](#schema-quotationdetailresponse)<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Authorize(Roles = RoleNames.Technician) Request body fields are defined in [`CreateQuotationFromJobRequest`](#schema-createquotationfromjobrequest). | Quotation approval/rejection commonly precedes `/invoices/from-quotation/{quotationId}`. | Use JSON camelCase field names exactly as defined in the schema registry below. |
| Billing, Payments & Receivables / Inventory, Parts & Estimates | QuotationController | Search | GET | /api/quotations | Search records using the supplied filter set for this route. | Admin | Authorize(Policy = PermissionNames.QuotationRead) | `status` `string?` Optional<br>`pageNumber` `integer(int32)` Required<br>`pageSize` `integer(int32)` Required | None | None | None | Envelope: `ApiResponse<PagedResult<QuotationListItemResponse>>`<br>Data schema: `PagedResult` of [`QuotationListItemResponse`](#schema-quotationlistitemresponse) with `items`, `totalCount`, `pageNumber`, `pageSize`<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Authorize(Policy = PermissionNames.QuotationRead) Omit null/empty query values; client utilities in web/mobile already serialize only populated keys. | Quotation approval/rejection commonly precedes `/invoices/from-quotation/{quotationId}`. | Use JSON camelCase field names exactly as defined in the schema registry below. |
| Billing, Payments & Receivables / Inventory, Parts & Estimates | QuotationController | GetById | GET | /api/quotations/{id:long} | Fetch by id data for `/api/quotations/{id:long}`. | Both | Authorize | None | `id` `integer(int64)` Required | None | None | Envelope: `ApiResponse<QuotationDetailResponse>`<br>Data schema: [`QuotationDetailResponse`](#schema-quotationdetailresponse)<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Authorize | Quotation approval/rejection commonly precedes `/invoices/from-quotation/{quotationId}`. | Use JSON camelCase field names exactly as defined in the schema registry below. |
| Billing, Payments & Receivables / Inventory, Parts & Estimates | QuotationController | GetByJobCard | GET | /api/quotations/job/{jobCardId:long} | Fetch by job card data for `/api/quotations/job/{jobCardId:long}`. | Both | Authorize | None | `jobCardId` `integer(int64)` Required | None | None | Envelope: `ApiResponse<QuotationDetailResponse>`<br>Data schema: [`QuotationDetailResponse`](#schema-quotationdetailresponse)<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Authorize | Quotation approval/rejection commonly precedes `/invoices/from-quotation/{quotationId}`. | Use JSON camelCase field names exactly as defined in the schema registry below. |
| Billing, Payments & Receivables / Inventory, Parts & Estimates | QuotationController | Approve | POST | /api/quotations/{id:long}/approve | Execute `Approve` for `/api/quotations/{id:long}/approve`. | Both | Authorize | None | `id` `integer(int64)` Required | None | [`QuotationDecisionRequest`](#schema-quotationdecisionrequest)<br>Field-by-field: schema registry entry. | Envelope: `ApiResponse<QuotationDetailResponse>`<br>Data schema: [`QuotationDetailResponse`](#schema-quotationdetailresponse)<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Authorize Request body fields are defined in [`QuotationDecisionRequest`](#schema-quotationdecisionrequest). | Quotation approval/rejection commonly precedes `/invoices/from-quotation/{quotationId}`. | Use JSON camelCase field names exactly as defined in the schema registry below. |
| Billing, Payments & Receivables / Inventory, Parts & Estimates | QuotationController | Reject | POST | /api/quotations/{id:long}/reject | Execute `Reject` for `/api/quotations/{id:long}/reject`. | Both | Authorize | None | `id` `integer(int64)` Required | None | [`QuotationDecisionRequest`](#schema-quotationdecisionrequest)<br>Field-by-field: schema registry entry. | Envelope: `ApiResponse<QuotationDetailResponse>`<br>Data schema: [`QuotationDetailResponse`](#schema-quotationdetailresponse)<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Authorize Request body fields are defined in [`QuotationDecisionRequest`](#schema-quotationdecisionrequest). | Quotation approval/rejection commonly precedes `/invoices/from-quotation/{quotationId}`. | Use JSON camelCase field names exactly as defined in the schema registry below. |
| Billing, Payments & Receivables | RefundController | CreateRefundRequest | POST | /api/refunds/request | Create refund request for `/api/refunds/request`. | Admin | Authorize(Policy = PermissionNames.PaymentCollect) | None | None | None | [`CreateRefundRequestCommandRequest`](#schema-createrefundrequestcommandrequest)<br>Field-by-field: schema registry entry. | Envelope: `ApiResponse<RefundDetailResponse>`<br>Data schema: [`RefundDetailResponse`](#schema-refunddetailresponse)<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Authorize(Policy = PermissionNames.PaymentCollect) Request body fields are defined in [`CreateRefundRequestCommandRequest`](#schema-createrefundrequestcommandrequest). | See referenced request/response schemas and related route families in the same module. | Use JSON camelCase field names exactly as defined in the schema registry below. |
| Billing, Payments & Receivables | RefundController | GetRefunds | GET | /api/refunds | Fetch refunds data for `/api/refunds`. | Admin | Authorize(Policy = PermissionNames.PaymentRead) | `refundStatus` `string?` Optional<br>`customerId` `integer(int64)?` Optional<br>`branchId` `integer(int32)?` Optional<br>`fromDateUtc` `datetime?` Optional<br>`toDateUtc` `datetime?` Optional | None | None | None | Envelope: `ApiResponse<IReadOnlyCollection<RefundListItemResponse>>`<br>Data schema: Array of [`RefundListItemResponse`](#schema-refundlistitemresponse)<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Authorize(Policy = PermissionNames.PaymentRead) Omit null/empty query values; client utilities in web/mobile already serialize only populated keys. | See referenced request/response schemas and related route families in the same module. | Use JSON camelCase field names exactly as defined in the schema registry below. |
| Billing, Payments & Receivables | RefundController | GetRefundById | GET | /api/refunds/{id:long} | Fetch refund by id data for `/api/refunds/{id:long}`. | Both | Authorize | None | `id` `integer(int64)` Required | None | None | Envelope: `ApiResponse<RefundDetailResponse>`<br>Data schema: [`RefundDetailResponse`](#schema-refunddetailresponse)<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Authorize | See referenced request/response schemas and related route families in the same module. | Use JSON camelCase field names exactly as defined in the schema registry below. |
| Billing, Payments & Receivables | RefundController | GetCustomerRefundStatus | GET | /api/refunds/customer/{customerId:long} | Fetch customer refund status data for `/api/refunds/customer/{customerId:long}`. | Both | Authorize | None | `customerId` `integer(int64)` Required | None | None | Envelope: `ApiResponse<IReadOnlyCollection<CustomerRefundStatusResponse>>`<br>Data schema: Array of [`CustomerRefundStatusResponse`](#schema-customerrefundstatusresponse)<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Authorize | See referenced request/response schemas and related route families in the same module. | Use JSON camelCase field names exactly as defined in the schema registry below. |
| Billing, Payments & Receivables | RefundController | InitiateRefund | POST | /api/refunds | Execute `InitiateRefund` for `/api/refunds`. | Admin | Authorize(Policy = PermissionNames.PaymentCollect) | None | None | None | [`InitiateRefundRequest`](#schema-initiaterefundrequest)<br>Field-by-field: schema registry entry. | Envelope: `ApiResponse<RefundRequestResponse>`<br>Data schema: [`RefundRequestResponse`](#schema-refundrequestresponse)<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Authorize(Policy = PermissionNames.PaymentCollect) Request body fields are defined in [`InitiateRefundRequest`](#schema-initiaterefundrequest). | See referenced request/response schemas and related route families in the same module. | Use JSON camelCase field names exactly as defined in the schema registry below. |
| Billing, Payments & Receivables | RefundController | ApproveRefund | POST | /api/refunds/{refundRequestId:long}/approve | Approve the target workflow for `/api/refunds/{refundRequestId:long}/approve`. | Admin | Authorize(Policy = PermissionNames.ConfigurationManage) | None | `refundRequestId` `integer(int64)` Required | None | [`ApproveRefundRequestDecisionRequest`](#schema-approverefundrequestdecisionrequest)<br>Field-by-field: schema registry entry. | Envelope: `ApiResponse<RefundDetailResponse>`<br>Data schema: [`RefundDetailResponse`](#schema-refunddetailresponse)<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Authorize(Policy = PermissionNames.ConfigurationManage) Request body fields are defined in [`ApproveRefundRequestDecisionRequest`](#schema-approverefundrequestdecisionrequest). | See referenced request/response schemas and related route families in the same module. | Use JSON camelCase field names exactly as defined in the schema registry below. |
| Billing, Payments & Receivables | RefundController | RejectRefund | POST | /api/refunds/{refundRequestId:long}/reject | Reject the target workflow for `/api/refunds/{refundRequestId:long}/reject`. | Admin | Authorize(Policy = PermissionNames.ConfigurationManage) | None | `refundRequestId` `integer(int64)` Required | None | [`RejectRefundRequestDecisionRequest`](#schema-rejectrefundrequestdecisionrequest)<br>Field-by-field: schema registry entry. | Envelope: `ApiResponse<RefundDetailResponse>`<br>Data schema: [`RefundDetailResponse`](#schema-refunddetailresponse)<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Authorize(Policy = PermissionNames.ConfigurationManage) Request body fields are defined in [`RejectRefundRequestDecisionRequest`](#schema-rejectrefundrequestdecisionrequest). | See referenced request/response schemas and related route families in the same module. | Use JSON camelCase field names exactly as defined in the schema registry below. |
| Billing, Payments & Receivables | RefundController | UpdateRefundStatus | POST | /api/refunds/{refundRequestId:long}/status | Update refund status for `/api/refunds/{refundRequestId:long}/status`. | Admin | Authorize(Policy = PermissionNames.ConfigurationManage) | None | `refundRequestId` `integer(int64)` Required | None | [`UpdateRefundStatusRequest`](#schema-updaterefundstatusrequest)<br>Field-by-field: schema registry entry. | Envelope: `ApiResponse<RefundDetailResponse>`<br>Data schema: [`RefundDetailResponse`](#schema-refunddetailresponse)<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Authorize(Policy = PermissionNames.ConfigurationManage) Request body fields are defined in [`UpdateRefundStatusRequest`](#schema-updaterefundstatusrequest). | See referenced request/response schemas and related route families in the same module. | Use JSON camelCase field names exactly as defined in the schema registry below. |

## 9. Support Ticket APIs

| Module | API Group / Controller | API Name | HTTP | Route | Purpose / Description | Used By | Authentication Requirement | Query Parameters | Path Parameters | Headers | Request Body / Field Definition | Response Structure / Field Definition | Success / Error Example | Validation / Business Rule Notes | Dependency / Linked APIs | Integration Notes |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| Customer Support & Feedback | CustomerReviewController | GetReviews | GET | /api/customer-reviews | Return customer reviews, optionally filtered by service. | Customer | Anonymous | `serviceId` `integer(int64)?` Optional | None | None | None | Envelope: `ApiResponse<IReadOnlyCollection<CustomerReviewResponse>>`<br>Data schema: Array of [`CustomerReviewResponse`](#schema-customerreviewresponse)<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Anonymous Omit null/empty query values; client utilities in web/mobile already serialize only populated keys. | Often shown on public service and customer-app screens. | Do not require bearer token for read-only public review display. |
| Customer Support & Feedback | CustomerReviewController | CreateReview | POST | /api/customer-reviews | Submit a customer review. | Customer | Authorize | None | None | None | [`CreateCustomerReviewRequest`](#schema-createcustomerreviewrequest)<br>Field-by-field: schema registry entry. | Envelope: `ApiResponse<CustomerReviewResponse>`<br>Data schema: [`CustomerReviewResponse`](#schema-customerreviewresponse)<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Authorize Request body fields are defined in [`CreateCustomerReviewRequest`](#schema-createcustomerreviewrequest). | Usually linked from completed booking/service-history flows. | Server validates authenticated customer context and target booking/service ownership where applicable. |
| Customer Support & Feedback | FeedbackController | Get | GET | /api/feedback | Return the admin support-feedback moderation feed, optionally filtered by service. | Admin | Authorize(Policy = PermissionNames.SupportRead) | `serviceId` `integer(int64)?` Optional | None | None | None | Envelope: `ApiResponse<IReadOnlyCollection<SupportFeedbackResponse>>`<br>Data schema: Array of [`SupportFeedbackResponse`](#schema-supportfeedbackresponse)<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Authorize(Policy = PermissionNames.SupportRead) Omit null/empty query values; admin clients can derive negative queue and analytics from this live feed. | Pair with `/api/feedback/{customerReviewId}`, `/respond`, `/publish`, and `/flag`. | This feed returns unpublished/flagged moderation state; public customer review display remains on `/api/customer-reviews`. |
| Customer Support & Feedback | FeedbackController | GetById | GET | /api/feedback/{customerReviewId:long} | Return support-visible detail for a single feedback record. | Admin | Authorize(Policy = PermissionNames.SupportRead) | None | `customerReviewId` `integer(int64)` Required | None | None | Envelope: `ApiResponse<SupportFeedbackResponse>`<br>Data schema: [`SupportFeedbackResponse`](#schema-supportfeedbackresponse)<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Authorize(Policy = PermissionNames.SupportRead) | Pair with `/respond`, `/publish`, and `/flag` for in-place moderation. | Use this route for feedback detail screens instead of reading from the public review feed. |
| Customer Support & Feedback | FeedbackController | Respond | PATCH | /api/feedback/{customerReviewId:long}/respond | Save or clear the admin response attached to a feedback record. | Admin | Authorize(Policy = PermissionNames.SupportManage) | None | `customerReviewId` `integer(int64)` Required | None | [`RespondFeedbackRequest`](#schema-respondfeedbackrequest)<br>Field-by-field: schema registry entry. | Envelope: `ApiResponse<SupportFeedbackResponse>`<br>Data schema: [`SupportFeedbackResponse`](#schema-supportfeedbackresponse)<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Authorize(Policy = PermissionNames.SupportManage) Request body fields are defined in [`RespondFeedbackRequest`](#schema-respondfeedbackrequest). | Pair with `GET /api/feedback/{customerReviewId}` for moderation detail refresh. | Empty/blank response text clears the persisted admin response. |
| Customer Support & Feedback | FeedbackController | Publish | PATCH | /api/feedback/{customerReviewId:long}/publish | Publish or unpublish a feedback record for public display. | Admin | Authorize(Policy = PermissionNames.SupportManage) | None | `customerReviewId` `integer(int64)` Required | None | [`PublishFeedbackRequest`](#schema-publishfeedbackrequest)<br>Field-by-field: schema registry entry. | Envelope: `ApiResponse<SupportFeedbackResponse>`<br>Data schema: [`SupportFeedbackResponse`](#schema-supportfeedbackresponse)<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Authorize(Policy = PermissionNames.SupportManage) Request body fields are defined in [`PublishFeedbackRequest`](#schema-publishfeedbackrequest). | Pair with `GET /api/customer-reviews` when verifying public visibility behaviour. | Publishing restores public visibility; unpublishing removes the record from the public review feed. |
| Customer Support & Feedback | FeedbackController | Flag | PATCH | /api/feedback/{customerReviewId:long}/flag | Flag a feedback record and remove it from public display. | Admin | Authorize(Policy = PermissionNames.SupportManage) | None | `customerReviewId` `integer(int64)` Required | None | [`FlagFeedbackRequest`](#schema-flagfeedbackrequest)<br>Field-by-field: schema registry entry. | Envelope: `ApiResponse<SupportFeedbackResponse>`<br>Data schema: [`SupportFeedbackResponse`](#schema-supportfeedbackresponse)<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Authorize(Policy = PermissionNames.SupportManage) Request body fields are defined in [`FlagFeedbackRequest`](#schema-flagfeedbackrequest). | Pair with `GET /api/feedback` negative-follow-up flows. | Flagging stores the moderation reason and forces the record out of the public review feed until it is explicitly published again. |
| Customer Support & Feedback | SupportTicketController | Create | POST | /api/support-tickets | Execute `Create` for `/api/support-tickets`. | Both | Authorize | None | None | None | [`CreateSupportTicketRequest`](#schema-createsupportticketrequest)<br>Field-by-field: schema registry entry. | Envelope: `ApiResponse<SupportTicketDetailResponse>`<br>Data schema: [`SupportTicketDetailResponse`](#schema-supportticketdetailresponse)<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Authorize Request body fields are defined in [`CreateSupportTicketRequest`](#schema-createsupportticketrequest). | Pair with `/support-tickets/{supportTicketId}/replies`, `/escalations`, and support lookup APIs. | Customer surfaces must force `isInternalOnly=false`; admin surfaces may use internal replies. |
| Customer Support & Feedback | SupportTicketController | Search | GET | /api/support-tickets | Search records using the supplied filter set for this route. | Admin | Authorize(Policy = PermissionNames.SupportRead) | `ticketNumber` `string?` Optional<br>`customerMobile` `string?` Optional<br>`categoryId` `integer(int64)?` Optional<br>`priorityId` `integer(int64)?` Optional<br>`status` `string?` Optional<br>`dateFrom` `date?` Optional<br>`dateTo` `date?` Optional<br>`linkedEntityType` `string?` Optional<br>`pageNumber` `integer(int32)` Required<br>`pageSize` `integer(int32)` Required | None | None | None | Envelope: `ApiResponse<PagedResult<SupportTicketListItemResponse>>`<br>Data schema: `PagedResult` of [`SupportTicketListItemResponse`](#schema-supportticketlistitemresponse) with `items`, `totalCount`, `pageNumber`, `pageSize`<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Authorize(Policy = PermissionNames.SupportRead) Omit null/empty query values; client utilities in web/mobile already serialize only populated keys. | Pair with `/support-tickets/{supportTicketId}/replies`, `/escalations`, and support lookup APIs. | Customer surfaces must force `isInternalOnly=false`; admin surfaces may use internal replies. |
| Customer Support & Feedback | SupportTicketController | GetById | GET | /api/support-tickets/{supportTicketId:long} | Fetch by id data for `/api/support-tickets/{supportTicketId:long}`. | Both | Authorize | None | `supportTicketId` `integer(int64)` Required | None | None | Envelope: `ApiResponse<SupportTicketDetailResponse>`<br>Data schema: [`SupportTicketDetailResponse`](#schema-supportticketdetailresponse)<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Authorize | Pair with `/support-tickets/{supportTicketId}/replies`, `/escalations`, and support lookup APIs. | Customer surfaces must force `isInternalOnly=false`; admin surfaces may use internal replies. |
| Customer Support & Feedback | SupportTicketController | GetMyTickets | GET | /api/support-tickets/my-tickets | Fetch my tickets data for `/api/support-tickets/my-tickets`. | Both | Authorize | `pageNumber` `integer(int32)` Required<br>`pageSize` `integer(int32)` Required | None | None | None | Envelope: `ApiResponse<PagedResult<SupportTicketListItemResponse>>`<br>Data schema: `PagedResult` of [`SupportTicketListItemResponse`](#schema-supportticketlistitemresponse) with `items`, `totalCount`, `pageNumber`, `pageSize`<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Authorize Omit null/empty query values; client utilities in web/mobile already serialize only populated keys. | Pair with `/support-tickets/{supportTicketId}/replies`, `/escalations`, and support lookup APIs. | Customer surfaces must force `isInternalOnly=false`; admin surfaces may use internal replies. |
| Customer Support & Feedback | SupportTicketController | Assign | POST | /api/support-tickets/{supportTicketId:long}/assign | Execute `Assign` for `/api/support-tickets/{supportTicketId:long}/assign`. | Admin | Authorize(Policy = PermissionNames.SupportManage) | None | `supportTicketId` `integer(int64)` Required | None | [`AssignSupportTicketRequest`](#schema-assignsupportticketrequest)<br>Field-by-field: schema registry entry. | Envelope: `ApiResponse<SupportTicketDetailResponse>`<br>Data schema: [`SupportTicketDetailResponse`](#schema-supportticketdetailresponse)<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Authorize(Policy = PermissionNames.SupportManage) Request body fields are defined in [`AssignSupportTicketRequest`](#schema-assignsupportticketrequest). | Pair with `/support-tickets/{supportTicketId}/replies`, `/escalations`, and support lookup APIs. | Customer surfaces must force `isInternalOnly=false`; admin surfaces may use internal replies. |
| Customer Support & Feedback | SupportTicketController | ChangeStatus | POST | /api/support-tickets/{supportTicketId:long}/change-status | Execute `ChangeStatus` for `/api/support-tickets/{supportTicketId:long}/change-status`. | Admin | Authorize(Policy = PermissionNames.SupportManage) | None | `supportTicketId` `integer(int64)` Required | None | [`ChangeSupportTicketStatusRequest`](#schema-changesupportticketstatusrequest)<br>Field-by-field: schema registry entry. | Envelope: `ApiResponse<SupportTicketDetailResponse>`<br>Data schema: [`SupportTicketDetailResponse`](#schema-supportticketdetailresponse)<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Authorize(Policy = PermissionNames.SupportManage) Request body fields are defined in [`ChangeSupportTicketStatusRequest`](#schema-changesupportticketstatusrequest). | Pair with `/support-tickets/{supportTicketId}/replies`, `/escalations`, and support lookup APIs. | Customer surfaces must force `isInternalOnly=false`; admin surfaces may use internal replies. |
| Customer Support & Feedback | SupportTicketController | ChangePriority | POST | /api/support-tickets/{supportTicketId:long}/change-priority | Execute `ChangePriority` for `/api/support-tickets/{supportTicketId:long}/change-priority`. | Admin | Authorize(Policy = PermissionNames.SupportManage) | None | `supportTicketId` `integer(int64)` Required | None | [`ChangeSupportTicketPriorityRequest`](#schema-changesupportticketpriorityrequest)<br>Field-by-field: schema registry entry. | Envelope: `ApiResponse<SupportTicketDetailResponse>`<br>Data schema: [`SupportTicketDetailResponse`](#schema-supportticketdetailresponse)<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Authorize(Policy = PermissionNames.SupportManage) Request body fields are defined in [`ChangeSupportTicketPriorityRequest`](#schema-changesupportticketpriorityrequest). | Pair with `/support-tickets/{supportTicketId}/replies`, `/escalations`, and support lookup APIs. | Customer surfaces must force `isInternalOnly=false`; admin surfaces may use internal replies. |
| Customer Support & Feedback | SupportTicketController | Close | POST | /api/support-tickets/{supportTicketId:long}/close | Execute `Close` for `/api/support-tickets/{supportTicketId:long}/close`. | Both | Authorize | None | `supportTicketId` `integer(int64)` Required | None | [`SupportTicketActionRequest`](#schema-supportticketactionrequest)<br>Field-by-field: schema registry entry. | Envelope: `ApiResponse<SupportTicketDetailResponse>`<br>Data schema: [`SupportTicketDetailResponse`](#schema-supportticketdetailresponse)<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Authorize Request body fields are defined in [`SupportTicketActionRequest`](#schema-supportticketactionrequest). | Pair with `/support-tickets/{supportTicketId}/replies`, `/escalations`, and support lookup APIs. | Customer surfaces must force `isInternalOnly=false`; admin surfaces may use internal replies. |
| Customer Support & Feedback | SupportTicketController | Reopen | POST | /api/support-tickets/{supportTicketId:long}/reopen | Execute `Reopen` for `/api/support-tickets/{supportTicketId:long}/reopen`. | Both | Authorize | None | `supportTicketId` `integer(int64)` Required | None | [`SupportTicketActionRequest`](#schema-supportticketactionrequest)<br>Field-by-field: schema registry entry. | Envelope: `ApiResponse<SupportTicketDetailResponse>`<br>Data schema: [`SupportTicketDetailResponse`](#schema-supportticketdetailresponse)<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Authorize Request body fields are defined in [`SupportTicketActionRequest`](#schema-supportticketactionrequest). | Pair with `/support-tickets/{supportTicketId}/replies`, `/escalations`, and support lookup APIs. | Customer surfaces must force `isInternalOnly=false`; admin surfaces may use internal replies. |
| Customer Support & Feedback | SupportTicketEscalationController | Get | GET | /api/support-tickets/{supportTicketId:long}/escalations | Execute `Get` for `/api/support-tickets/{supportTicketId:long}/escalations`. | Admin | Authorize | None | `supportTicketId` `integer(int64)` Required | None | None | Envelope: `ApiResponse<IReadOnlyCollection<SupportTicketEscalationResponse>>`<br>Data schema: Array of [`SupportTicketEscalationResponse`](#schema-supportticketescalationresponse)<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Authorize | See referenced request/response schemas and related route families in the same module. | Use JSON camelCase field names exactly as defined in the schema registry below. |
| Customer Support & Feedback | SupportTicketEscalationController | Escalate | POST | /api/support-tickets/{supportTicketId:long}/escalate | Execute `Escalate` for `/api/support-tickets/{supportTicketId:long}/escalate`. | Admin | Authorize(Policy = PermissionNames.SupportManage) | None | `supportTicketId` `integer(int64)` Required | None | [`EscalateSupportTicketRequest`](#schema-escalatesupportticketrequest)<br>Field-by-field: schema registry entry. | Envelope: `ApiResponse<SupportTicketDetailResponse>`<br>Data schema: [`SupportTicketDetailResponse`](#schema-supportticketdetailresponse)<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Authorize(Policy = PermissionNames.SupportManage) Request body fields are defined in [`EscalateSupportTicketRequest`](#schema-escalatesupportticketrequest). | See referenced request/response schemas and related route families in the same module. | Use JSON camelCase field names exactly as defined in the schema registry below. |
| Customer Support & Feedback | SupportTicketLookupController | GetCategories | GET | /api/support-ticket-lookups/categories | Fetch categories data for `/api/support-ticket-lookups/categories`. | Both | Authorize | None | None | None | None | Envelope: `ApiResponse<IReadOnlyCollection<LookupItemResponse>>`<br>Data schema: Array of [`LookupItemResponse`](#schema-lookupitemresponse)<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Authorize | See referenced request/response schemas and related route families in the same module. | Use JSON camelCase field names exactly as defined in the schema registry below. |
| Customer Support & Feedback | SupportTicketLookupController | GetPriorities | GET | /api/support-ticket-lookups/priorities | Fetch priorities data for `/api/support-ticket-lookups/priorities`. | Both | Authorize | None | None | None | None | Envelope: `ApiResponse<IReadOnlyCollection<LookupItemResponse>>`<br>Data schema: Array of [`LookupItemResponse`](#schema-lookupitemresponse)<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Authorize | See referenced request/response schemas and related route families in the same module. | Use JSON camelCase field names exactly as defined in the schema registry below. |
| Customer Support & Feedback | SupportTicketLookupController | GetStatuses | GET | /api/support-ticket-lookups/statuses | Fetch statuses data for `/api/support-ticket-lookups/statuses`. | Both | Authorize | None | None | None | None | Envelope: `ApiResponse<IReadOnlyCollection<LookupItemResponse>>`<br>Data schema: Array of [`LookupItemResponse`](#schema-lookupitemresponse)<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Authorize | See referenced request/response schemas and related route families in the same module. | Use JSON camelCase field names exactly as defined in the schema registry below. |
| Customer Support & Feedback | SupportTicketReplyController | Get | GET | /api/support-tickets/{supportTicketId:long}/replies | Execute `Get` for `/api/support-tickets/{supportTicketId:long}/replies`. | Both | Authorize | None | `supportTicketId` `integer(int64)` Required | None | None | Envelope: `ApiResponse<IReadOnlyCollection<SupportTicketReplyResponse>>`<br>Data schema: Array of [`SupportTicketReplyResponse`](#schema-supportticketreplyresponse)<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Authorize | See referenced request/response schemas and related route families in the same module. | Customer surfaces must force `isInternalOnly=false`; admin surfaces may use internal replies. |
| Customer Support & Feedback | SupportTicketReplyController | Create | POST | /api/support-tickets/{supportTicketId:long}/replies | Execute `Create` for `/api/support-tickets/{supportTicketId:long}/replies`. | Both | Authorize | None | `supportTicketId` `integer(int64)` Required | None | [`AddSupportTicketReplyRequest`](#schema-addsupportticketreplyrequest)<br>Field-by-field: schema registry entry. | Envelope: `ApiResponse<SupportTicketReplyResponse>`<br>Data schema: [`SupportTicketReplyResponse`](#schema-supportticketreplyresponse)<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Authorize Request body fields are defined in [`AddSupportTicketReplyRequest`](#schema-addsupportticketreplyrequest). | See referenced request/response schemas and related route families in the same module. | Customer surfaces must force `isInternalOnly=false`; admin surfaces may use internal replies. |

## 10. Notification / Preferences APIs

| Module | API Group / Controller | API Name | HTTP | Route | Purpose / Description | Used By | Authentication Requirement | Query Parameters | Path Parameters | Headers | Request Body / Field Definition | Response Structure / Field Definition | Success / Error Example | Validation / Business Rule Notes | Dependency / Linked APIs | Integration Notes |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| Notification & Communication | CustomerNotificationController | GetMine | GET | /api/customer-notifications | Return paged notifications for the authenticated customer. | Customer | Authorize | `pageNumber` `integer(int32)` Required<br>`pageSize` `integer(int32)` Required | None | None | None | Envelope: `ApiResponse<PagedResult<CustomerNotificationResponse>>`<br>Data schema: `PagedResult` of [`CustomerNotificationResponse`](#schema-customernotificationresponse) with `items`, `totalCount`, `pageNumber`, `pageSize`<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Authorize Omit null/empty query values; client utilities in web/mobile already serialize only populated keys. | Pairs with `POST /customer-notifications/{notificationId}/mark-read`. | Server derives the customer from the bearer token; clients do not send `customerId`. |
| Notification & Communication | CustomerNotificationController | MarkRead | POST | /api/customer-notifications/{notificationId:long}/mark-read | Mark a customer notification as read. | Customer | Authorize | None | `notificationId` `integer(int64)` Required | None | None | Envelope: `ApiResponse<object>`<br>Data fields: `notificationId` `integer(int64)`. | Success: standard envelope + listed data fields.<br>Error: standard error envelope in Section 16. | Authorize | Notification ownership is validated server-side. | Client should update local read state only after success. |
| Notification & Communication | CommunicationPreferenceController | GetMine | GET | /api/communication-preferences/me | Fetch mine data for `/api/communication-preferences/me`. | Customer | Authorize | None | None | None | None | Envelope: `ApiResponse<CommunicationPreferenceResponse>`<br>Data schema: [`CommunicationPreferenceResponse`](#schema-communicationpreferenceresponse)<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Authorize | See referenced request/response schemas and related route families in the same module. | Customer self-service uses `/me`; admin backoffice uses `/customer/{customerId}`. |
| Notification & Communication | CommunicationPreferenceController | UpdateMine | PUT | /api/communication-preferences/me | Update mine for `/api/communication-preferences/me`. | Customer | Authorize | None | None | None | [`CommunicationPreferenceUpdateRequest`](#schema-communicationpreferenceupdaterequest)<br>Field-by-field: schema registry entry. | Envelope: `ApiResponse<CommunicationPreferenceResponse>`<br>Data schema: [`CommunicationPreferenceResponse`](#schema-communicationpreferenceresponse)<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Authorize Request body fields are defined in [`CommunicationPreferenceUpdateRequest`](#schema-communicationpreferenceupdaterequest). | See referenced request/response schemas and related route families in the same module. | Customer self-service uses `/me`; admin backoffice uses `/customer/{customerId}`. |
| Notification & Communication | CommunicationPreferenceController | GetByCustomer | GET | /api/communication-preferences/customer/{customerId:long} | Fetch by customer data for `/api/communication-preferences/customer/{customerId:long}`. | Admin | Authorize(Policy = PermissionNames.CommunicationPreferenceRead) | None | `customerId` `integer(int64)` Required | None | None | Envelope: `ApiResponse<CommunicationPreferenceResponse>`<br>Data schema: [`CommunicationPreferenceResponse`](#schema-communicationpreferenceresponse)<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Authorize(Policy = PermissionNames.CommunicationPreferenceRead) | See referenced request/response schemas and related route families in the same module. | Customer self-service uses `/me`; admin backoffice uses `/customer/{customerId}`. |
| Notification & Communication | NotificationTemplateController | Get | GET | /api/notification-templates | Execute `Get` for `/api/notification-templates`. | Admin | Authorize(Policy = PermissionNames.NotificationTemplateRead) | `search` `string?` Optional<br>`channel` `string?` Optional<br>`triggerCode` `string?` Optional<br>`isActive` `boolean?` Optional | None | None | None | Envelope: `ApiResponse<IReadOnlyCollection<NotificationTemplateResponse>>`<br>Data schema: Array of [`NotificationTemplateResponse`](#schema-notificationtemplateresponse)<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Authorize(Policy = PermissionNames.NotificationTemplateRead) Omit null/empty query values; client utilities in web/mobile already serialize only populated keys. | See referenced request/response schemas and related route families in the same module. | Use JSON camelCase field names exactly as defined in the schema registry below. |
| Notification & Communication | NotificationTemplateController | GetDetail | GET | /api/notification-templates/{notificationTemplateId:long} | Fetch detail data for `/api/notification-templates/{notificationTemplateId:long}`. | Admin | Authorize(Policy = PermissionNames.NotificationTemplateRead) | None | `notificationTemplateId` `integer(int64)` Required | None | None | Envelope: `ApiResponse<NotificationTemplateResponse>`<br>Data schema: [`NotificationTemplateResponse`](#schema-notificationtemplateresponse)<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Authorize(Policy = PermissionNames.NotificationTemplateRead) | See referenced request/response schemas and related route families in the same module. | Use JSON camelCase field names exactly as defined in the schema registry below. |
| Notification & Communication | NotificationTemplateController | Create | POST | /api/notification-templates | Execute `Create` for `/api/notification-templates`. | Admin | Authorize(Policy = PermissionNames.NotificationTemplateManage) | None | None | None | [`NotificationTemplateUpsertRequest`](#schema-notificationtemplateupsertrequest)<br>Field-by-field: schema registry entry. | Envelope: `ApiResponse<NotificationTemplateResponse>`<br>Data schema: [`NotificationTemplateResponse`](#schema-notificationtemplateresponse)<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Authorize(Policy = PermissionNames.NotificationTemplateManage) Request body fields are defined in [`NotificationTemplateUpsertRequest`](#schema-notificationtemplateupsertrequest). | See referenced request/response schemas and related route families in the same module. | Use JSON camelCase field names exactly as defined in the schema registry below. |
| Notification & Communication | NotificationTemplateController | Update | PUT | /api/notification-templates/{notificationTemplateId:long} | Execute `Update` for `/api/notification-templates/{notificationTemplateId:long}`. | Admin | Authorize(Policy = PermissionNames.NotificationTemplateManage) | None | `notificationTemplateId` `integer(int64)` Required | None | [`NotificationTemplateUpsertRequest`](#schema-notificationtemplateupsertrequest)<br>Field-by-field: schema registry entry. | Envelope: `ApiResponse<NotificationTemplateResponse>`<br>Data schema: [`NotificationTemplateResponse`](#schema-notificationtemplateresponse)<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Authorize(Policy = PermissionNames.NotificationTemplateManage) Request body fields are defined in [`NotificationTemplateUpsertRequest`](#schema-notificationtemplateupsertrequest). | See referenced request/response schemas and related route families in the same module. | Use JSON camelCase field names exactly as defined in the schema registry below. |
| Notification & Communication | NotificationTriggerController | Get | GET | /api/notification-triggers | Execute `Get` for `/api/notification-triggers`. | Admin | Authorize(Policy = PermissionNames.NotificationTriggerRead) | `search` `string?` Optional<br>`isEnabled` `boolean?` Optional | None | None | None | Envelope: `ApiResponse<IReadOnlyCollection<NotificationTriggerConfigurationResponse>>`<br>Data schema: Array of [`NotificationTriggerConfigurationResponse`](#schema-notificationtriggerconfigurationresponse)<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Authorize(Policy = PermissionNames.NotificationTriggerRead) Omit null/empty query values; client utilities in web/mobile already serialize only populated keys. | See referenced request/response schemas and related route families in the same module. | Use JSON camelCase field names exactly as defined in the schema registry below. |
| Notification & Communication | NotificationTriggerController | Create | POST | /api/notification-triggers | Execute `Create` for `/api/notification-triggers`. | Admin | Authorize(Policy = PermissionNames.NotificationTriggerManage) | None | None | None | [`NotificationTriggerUpsertRequest`](#schema-notificationtriggerupsertrequest)<br>Field-by-field: schema registry entry. | Envelope: `ApiResponse<NotificationTriggerConfigurationResponse>`<br>Data schema: [`NotificationTriggerConfigurationResponse`](#schema-notificationtriggerconfigurationresponse)<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Authorize(Policy = PermissionNames.NotificationTriggerManage) Request body fields are defined in [`NotificationTriggerUpsertRequest`](#schema-notificationtriggerupsertrequest). | See referenced request/response schemas and related route families in the same module. | Use JSON camelCase field names exactly as defined in the schema registry below. |
| Notification & Communication | NotificationTriggerController | Update | PUT | /api/notification-triggers/{notificationTriggerConfigurationId:long} | Execute `Update` for `/api/notification-triggers/{notificationTriggerConfigurationId:long}`. | Admin | Authorize(Policy = PermissionNames.NotificationTriggerManage) | None | `notificationTriggerConfigurationId` `integer(int64)` Required | None | [`NotificationTriggerUpsertRequest`](#schema-notificationtriggerupsertrequest)<br>Field-by-field: schema registry entry. | Envelope: `ApiResponse<NotificationTriggerConfigurationResponse>`<br>Data schema: [`NotificationTriggerConfigurationResponse`](#schema-notificationtriggerconfigurationresponse)<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Authorize(Policy = PermissionNames.NotificationTriggerManage) Request body fields are defined in [`NotificationTriggerUpsertRequest`](#schema-notificationtriggerupsertrequest). | See referenced request/response schemas and related route families in the same module. | Use JSON camelCase field names exactly as defined in the schema registry below. |

## 11. CMS / Content APIs

| Module | API Group / Controller | API Name | HTTP | Route | Purpose / Description | Used By | Authentication Requirement | Query Parameters | Path Parameters | Headers | Request Body / Field Definition | Response Structure / Field Definition | Success / Error Example | Validation / Business Rule Notes | Dependency / Linked APIs | Integration Notes |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| Website CMS & Marketing | CMSController | GetPublicHome | GET | /api/cms/public/home | Return aggregated public-home CMS payload. | Customer | Anonymous | None | None | None | None | Envelope: `ApiResponse<PublicHomeCMSContentResponse>`<br>Data schema: [`PublicHomeCMSContentResponse`](#schema-publichomecmscontentresponse)<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Anonymous | See referenced request/response schemas and related route families in the same module. | Public customer web already consumes these endpoints and normalizes nested/stringified data payloads. |
| Website CMS & Marketing | CMSController | GetPublicFaqs | GET | /api/cms/public/faqs | Fetch public faqs data for `/api/cms/public/faqs`. | Customer | Anonymous | None | None | None | None | Envelope: `ApiResponse<IReadOnlyCollection<CMSFaqResponse>>`<br>Data schema: Array of [`CMSFaqResponse`](#schema-cmsfaqresponse)<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Anonymous | See referenced request/response schemas and related route families in the same module. | Public customer web already consumes these endpoints and normalizes nested/stringified data payloads. |
| Website CMS & Marketing | CMSController | GetPublicBanners | GET | /api/cms/public/banners | Fetch public banners data for `/api/cms/public/banners`. | Customer | Anonymous | None | None | None | None | Envelope: `ApiResponse<IReadOnlyCollection<CMSBannerResponse>>`<br>Data schema: Array of [`CMSBannerResponse`](#schema-cmsbannerresponse)<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Anonymous | See referenced request/response schemas and related route families in the same module. | Public customer web already consumes these endpoints and normalizes nested/stringified data payloads. |
| Website CMS & Marketing | CMSController | GetPublicServiceContent | GET | /api/cms/public/service-content/{key} | Fetch public service content data for `/api/cms/public/service-content/{key}`. | Customer | Anonymous | None | `key` `string` Required | None | None | Envelope: `ApiResponse<CMSBlockResponse>`<br>Data schema: [`CMSBlockResponse`](#schema-cmsblockresponse)<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Anonymous | See referenced request/response schemas and related route families in the same module. | Public customer web already consumes these endpoints and normalizes nested/stringified data payloads. |
| Website CMS & Marketing | CustomerContentController | GetBlogs | GET | /api/cms/public/blogs | Return public blog content for customer apps. | Customer | Anonymous | None | None | None | None | Envelope: `ApiResponse<IReadOnlyCollection<BlogContentResponse>>`<br>Data schema: Array of [`BlogContentResponse`](#schema-blogcontentresponse)<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Anonymous | Public content endpoints do not require bearer token. | Use this route for customer-app blog lists rather than admin CMS routes. |
| Website CMS & Marketing | CustomerContentController | GetBlogById | GET | /api/cms/public/blogs/{id} | Return public blog content detail. | Customer | Anonymous | None | `id` `string` Required | None | None | Envelope: `ApiResponse<BlogContentResponse?>`<br>Data schema: [`BlogContentResponse`](#schema-blogcontentresponse)<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Anonymous | Public content endpoints do not require bearer token. | `id` is a content identifier/string key, not a constrained numeric route. |
| Website CMS & Marketing | CustomerContentController | GetChangelog | GET | /api/cms/public/changelog | Return public customer-app changelog content. | Customer | Anonymous | None | None | None | None | Envelope: `ApiResponse<IReadOnlyCollection<ChangelogItemResponse>>`<br>Data schema: Array of [`ChangelogItemResponse`](#schema-changelogitemresponse)<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Anonymous | Public content endpoints do not require bearer token. | Use for customer app release/update notes. |
| Website CMS & Marketing | CustomerContentController | SubmitFeedback | POST | /api/customer-app/feedback | Submit authenticated customer app feedback. | Customer | Authorize | None | None | None | [`SubmitAppFeedbackRequest`](#schema-submitappfeedbackrequest)<br>Field-by-field: schema registry entry. | Envelope: `ApiResponse<CustomerAppFeedbackResponse>`<br>Data schema: [`CustomerAppFeedbackResponse`](#schema-customerappfeedbackresponse)<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Authorize Request body fields are defined in [`SubmitAppFeedbackRequest`](#schema-submitappfeedbackrequest). | Feedback is tied to the authenticated customer account. | Use JSON camelCase field names exactly as defined in the schema registry below. |
| Website CMS & Marketing | CMSController | GetBlocks | GET | /api/cms/admin/blocks | List CMS blocks for admin management. | Admin | Authorize(Policy = PermissionNames.CmsRead) | `search` `string?` Optional<br>`isActive` `boolean?` Optional<br>`isPublished` `boolean?` Optional | None | None | None | Envelope: `ApiResponse<IReadOnlyCollection<CMSBlockResponse>>`<br>Data schema: Array of [`CMSBlockResponse`](#schema-cmsblockresponse)<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Authorize(Policy = PermissionNames.CmsRead) Omit null/empty query values; client utilities in web/mobile already serialize only populated keys. | See referenced request/response schemas and related route families in the same module. | Use JSON camelCase field names exactly as defined in the schema registry below. |
| Website CMS & Marketing | CMSController | CreateBlock | POST | /api/cms/admin/blocks | Create block for `/api/cms/admin/blocks`. | Admin | Authorize(Policy = PermissionNames.CmsManage) | None | None | None | [`CMSBlockUpsertRequest`](#schema-cmsblockupsertrequest)<br>Field-by-field: schema registry entry. | Envelope: `ApiResponse<CMSBlockResponse>`<br>Data schema: [`CMSBlockResponse`](#schema-cmsblockresponse)<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Authorize(Policy = PermissionNames.CmsManage) Request body fields are defined in [`CMSBlockUpsertRequest`](#schema-cmsblockupsertrequest). | See referenced request/response schemas and related route families in the same module. | Use JSON camelCase field names exactly as defined in the schema registry below. |
| Website CMS & Marketing | CMSController | UpdateBlock | PUT | /api/cms/admin/blocks/{cmsBlockId:long} | Update block for `/api/cms/admin/blocks/{cmsBlockId:long}`. | Admin | Authorize(Policy = PermissionNames.CmsManage) | None | `cmsBlockId` `integer(int64)` Required | None | [`CMSBlockUpsertRequest`](#schema-cmsblockupsertrequest)<br>Field-by-field: schema registry entry. | Envelope: `ApiResponse<CMSBlockResponse>`<br>Data schema: [`CMSBlockResponse`](#schema-cmsblockresponse)<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Authorize(Policy = PermissionNames.CmsManage) Request body fields are defined in [`CMSBlockUpsertRequest`](#schema-cmsblockupsertrequest). | See referenced request/response schemas and related route families in the same module. | Use JSON camelCase field names exactly as defined in the schema registry below. |
| Website CMS & Marketing | CMSController | GetBanners | GET | /api/cms/admin/banners | List CMS banners for admin/public consumption. | Admin | Authorize(Policy = PermissionNames.CmsRead) | `search` `string?` Optional<br>`isActive` `boolean?` Optional<br>`isPublished` `boolean?` Optional | None | None | None | Envelope: `ApiResponse<IReadOnlyCollection<CMSBannerResponse>>`<br>Data schema: Array of [`CMSBannerResponse`](#schema-cmsbannerresponse)<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Authorize(Policy = PermissionNames.CmsRead) Omit null/empty query values; client utilities in web/mobile already serialize only populated keys. | See referenced request/response schemas and related route families in the same module. | Use JSON camelCase field names exactly as defined in the schema registry below. |
| Website CMS & Marketing | CMSController | CreateBanner | POST | /api/cms/admin/banners | Create banner for `/api/cms/admin/banners`. | Admin | Authorize(Policy = PermissionNames.CmsManage) | None | None | None | [`CMSBannerUpsertRequest`](#schema-cmsbannerupsertrequest)<br>Field-by-field: schema registry entry. | Envelope: `ApiResponse<CMSBannerResponse>`<br>Data schema: [`CMSBannerResponse`](#schema-cmsbannerresponse)<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Authorize(Policy = PermissionNames.CmsManage) Request body fields are defined in [`CMSBannerUpsertRequest`](#schema-cmsbannerupsertrequest). | See referenced request/response schemas and related route families in the same module. | Use JSON camelCase field names exactly as defined in the schema registry below. |
| Website CMS & Marketing | CMSController | UpdateBanner | PUT | /api/cms/admin/banners/{cmsBannerId:long} | Update banner for `/api/cms/admin/banners/{cmsBannerId:long}`. | Admin | Authorize(Policy = PermissionNames.CmsManage) | None | `cmsBannerId` `integer(int64)` Required | None | [`CMSBannerUpsertRequest`](#schema-cmsbannerupsertrequest)<br>Field-by-field: schema registry entry. | Envelope: `ApiResponse<CMSBannerResponse>`<br>Data schema: [`CMSBannerResponse`](#schema-cmsbannerresponse)<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Authorize(Policy = PermissionNames.CmsManage) Request body fields are defined in [`CMSBannerUpsertRequest`](#schema-cmsbannerupsertrequest). | See referenced request/response schemas and related route families in the same module. | Use JSON camelCase field names exactly as defined in the schema registry below. |
| Website CMS & Marketing | CMSController | GetFaqs | GET | /api/cms/admin/faqs | List CMS FAQ records. | Admin | Authorize(Policy = PermissionNames.CmsRead) | `category` `string?` Optional<br>`search` `string?` Optional<br>`isActive` `boolean?` Optional<br>`isPublished` `boolean?` Optional | None | None | None | Envelope: `ApiResponse<IReadOnlyCollection<CMSFaqResponse>>`<br>Data schema: Array of [`CMSFaqResponse`](#schema-cmsfaqresponse)<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Authorize(Policy = PermissionNames.CmsRead) Omit null/empty query values; client utilities in web/mobile already serialize only populated keys. | See referenced request/response schemas and related route families in the same module. | Use JSON camelCase field names exactly as defined in the schema registry below. |
| Website CMS & Marketing | CMSController | CreateFaq | POST | /api/cms/admin/faqs | Create faq for `/api/cms/admin/faqs`. | Admin | Authorize(Policy = PermissionNames.CmsManage) | None | None | None | [`CMSFaqUpsertRequest`](#schema-cmsfaqupsertrequest)<br>Field-by-field: schema registry entry. | Envelope: `ApiResponse<CMSFaqResponse>`<br>Data schema: [`CMSFaqResponse`](#schema-cmsfaqresponse)<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Authorize(Policy = PermissionNames.CmsManage) Request body fields are defined in [`CMSFaqUpsertRequest`](#schema-cmsfaqupsertrequest). | See referenced request/response schemas and related route families in the same module. | Use JSON camelCase field names exactly as defined in the schema registry below. |
| Website CMS & Marketing | CMSController | UpdateFaq | PUT | /api/cms/admin/faqs/{cmsFaqId:long} | Update faq for `/api/cms/admin/faqs/{cmsFaqId:long}`. | Admin | Authorize(Policy = PermissionNames.CmsManage) | None | `cmsFaqId` `integer(int64)` Required | None | [`CMSFaqUpsertRequest`](#schema-cmsfaqupsertrequest)<br>Field-by-field: schema registry entry. | Envelope: `ApiResponse<CMSFaqResponse>`<br>Data schema: [`CMSFaqResponse`](#schema-cmsfaqresponse)<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Authorize(Policy = PermissionNames.CmsManage) Request body fields are defined in [`CMSFaqUpsertRequest`](#schema-cmsfaqupsertrequest). | See referenced request/response schemas and related route families in the same module. | Use JSON camelCase field names exactly as defined in the schema registry below. |
| Website CMS & Marketing | CustomerMarketingController | GetOffers | GET | /api/offers | Return active promotional offers. | Customer | Anonymous | None | None | None | None | Envelope: `ApiResponse<IReadOnlyCollection<PromotionalOfferResponse>>`<br>Data schema: Array of [`PromotionalOfferResponse`](#schema-promotionalofferresponse)<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Anonymous | Offer data can feed booking and customer-home surfaces. | Public read route; coupon validation requires authentication. |
| Website CMS & Marketing | CustomerMarketingController | ValidateCoupon | POST | /api/offers/validate-coupon | Validate a coupon code for the authenticated customer. | Customer | Authorize | None | None | None | [`ValidateCouponRequest`](#schema-validatecouponrequest)<br>Field-by-field: schema registry entry. | Envelope: `ApiResponse<PromotionalOfferResponse?>`<br>Data schema: [`PromotionalOfferResponse`](#schema-promotionalofferresponse)<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema or null when invalid.<br>Error: standard error envelope in Section 16. | Authorize Request body fields are defined in [`ValidateCouponRequest`](#schema-validatecouponrequest). | Usually called before booking/payment submission when a coupon code is entered. | Null `data` with a validation message represents an invalid coupon in the current controller contract. |
| Website CMS & Marketing | CustomerMarketingController | GetMyReferralStats | GET | /api/referrals/me | Return referral statistics for the authenticated customer. | Customer | Authorize | None | None | None | None | Envelope: `ApiResponse<ReferralStatsResponse>`<br>Data schema: [`ReferralStatsResponse`](#schema-referralstatsresponse)<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Authorize | Referral stats are customer-scoped. | Server derives the customer from the bearer token; clients do not send `customerId`. |
| Website CMS & Marketing | CustomerMarketingController | GetMyLoyaltyPoints | GET | /api/loyalty/me | Return loyalty point summary for the authenticated customer. | Customer | Authorize | None | None | None | None | Envelope: `ApiResponse<LoyaltyPointsResponse>`<br>Data schema: [`LoyaltyPointsResponse`](#schema-loyaltypointsresponse)<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Authorize | Loyalty summary is customer-scoped. | Pair with `/loyalty/me/transactions` for ledger display. |
| Website CMS & Marketing | CustomerMarketingController | GetMyLoyaltyTransactions | GET | /api/loyalty/me/transactions | Return loyalty point transactions for the authenticated customer. | Customer | Authorize | None | None | None | None | Envelope: `ApiResponse<IReadOnlyCollection<LoyaltyTransactionResponse>>`<br>Data schema: Array of [`LoyaltyTransactionResponse`](#schema-loyaltytransactionresponse)<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Authorize | Loyalty transactions are customer-scoped. | Pair with `/loyalty/me` for summary display. |
| Website CMS & Marketing | CampaignController | Create | POST | /api/campaigns | Execute `Create` for `/api/campaigns`. | Admin | Authorize(Policy = PermissionNames.BookingCreate) | None | None | None | [`CreateCampaignRequest`](#schema-createcampaignrequest)<br>Field-by-field: schema registry entry. | Envelope: `ApiResponse<CampaignResponse>`<br>Data schema: [`CampaignResponse`](#schema-campaignresponse)<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Authorize(Policy = PermissionNames.BookingCreate) Request body fields are defined in [`CreateCampaignRequest`](#schema-createcampaignrequest). | See referenced request/response schemas and related route families in the same module. | Use JSON camelCase field names exactly as defined in the schema registry below. |

## 12. Admin / Operations APIs

| Module | API Group / Controller | API Name | HTTP | Route | Purpose / Description | Used By | Authentication Requirement | Query Parameters | Path Parameters | Headers | Request Body / Field Definition | Response Structure / Field Definition | Success / Error Example | Validation / Business Rule Notes | Dependency / Linked APIs | Integration Notes |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| Analytics & Reporting | AnalyticsController | GetBookings | GET | /api/analytics/bookings | Fetch bookings data for `/api/analytics/bookings`. | Admin | Authorize(Policy = PermissionNames.AnalyticsRead) | `dateFrom` `date?` Optional<br>`dateTo` `date?` Optional<br>`trendBy` `string?` Optional<br>`serviceId` `integer(int64)?` Optional<br>`status` `string?` Optional | None | None | None | Envelope: `ApiResponse<BookingAnalyticsResponse>`<br>Data schema: [`BookingAnalyticsResponse`](#schema-bookinganalyticsresponse)<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Authorize(Policy = PermissionNames.AnalyticsRead) Omit null/empty query values; client utilities in web/mobile already serialize only populated keys. | See referenced request/response schemas and related route families in the same module. | Use JSON camelCase field names exactly as defined in the schema registry below. |
| Analytics & Reporting | AnalyticsController | GetRevenue | GET | /api/analytics/revenue | Fetch revenue data for `/api/analytics/revenue`. | Admin | Authorize(Policy = PermissionNames.AnalyticsRead) | `dateFrom` `date?` Optional<br>`dateTo` `date?` Optional<br>`trendBy` `string?` Optional<br>`serviceId` `integer(int64)?` Optional | None | None | None | Envelope: `ApiResponse<RevenueAnalyticsResponse>`<br>Data schema: [`RevenueAnalyticsResponse`](#schema-revenueanalyticsresponse)<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Authorize(Policy = PermissionNames.AnalyticsRead) Omit null/empty query values; client utilities in web/mobile already serialize only populated keys. | See referenced request/response schemas and related route families in the same module. | Use JSON camelCase field names exactly as defined in the schema registry below. |
| Analytics & Reporting | AnalyticsController | GetTechnicians | GET | /api/analytics/technicians | Fetch technicians data for `/api/analytics/technicians`. | Admin | Authorize(Policy = PermissionNames.AnalyticsRead) | `dateFrom` `date?` Optional<br>`dateTo` `date?` Optional<br>`trendBy` `string?` Optional<br>`technicianId` `integer(int64)?` Optional<br>`status` `string?` Optional | None | None | None | Envelope: `ApiResponse<TechnicianPerformanceResponse>`<br>Data schema: [`TechnicianPerformanceResponse`](#schema-technicianperformanceresponse)<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Authorize(Policy = PermissionNames.AnalyticsRead) Omit null/empty query values; client utilities in web/mobile already serialize only populated keys. | See referenced request/response schemas and related route families in the same module. | Use JSON camelCase field names exactly as defined in the schema registry below. |
| Analytics & Reporting | AnalyticsController | GetCustomers | GET | /api/analytics/customers | Fetch customers data for `/api/analytics/customers`. | Admin | Authorize(Policy = PermissionNames.AnalyticsRead) | `dateFrom` `date?` Optional<br>`dateTo` `date?` Optional<br>`trendBy` `string?` Optional | None | None | None | Envelope: `ApiResponse<CustomerAnalyticsResponse>`<br>Data schema: [`CustomerAnalyticsResponse`](#schema-customeranalyticsresponse)<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Authorize(Policy = PermissionNames.AnalyticsRead) Omit null/empty query values; client utilities in web/mobile already serialize only populated keys. | See referenced request/response schemas and related route families in the same module. | Use JSON camelCase field names exactly as defined in the schema registry below. |
| Analytics & Reporting | AnalyticsController | GetSupport | GET | /api/analytics/support | Fetch support data for `/api/analytics/support`. | Admin | Authorize(Policy = PermissionNames.AnalyticsRead) | `dateFrom` `date?` Optional<br>`dateTo` `date?` Optional<br>`trendBy` `string?` Optional<br>`status` `string?` Optional | None | None | None | Envelope: `ApiResponse<SupportAnalyticsResponse>`<br>Data schema: [`SupportAnalyticsResponse`](#schema-supportanalyticsresponse)<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Authorize(Policy = PermissionNames.AnalyticsRead) Omit null/empty query values; client utilities in web/mobile already serialize only populated keys. | See referenced request/response schemas and related route families in the same module. | Use JSON camelCase field names exactly as defined in the schema registry below. |
| Analytics & Reporting | AnalyticsController | GetInventory | GET | /api/analytics/inventory | Fetch inventory data for `/api/analytics/inventory`. | Admin | Authorize(Policy = PermissionNames.AnalyticsRead) | `dateFrom` `date?` Optional<br>`dateTo` `date?` Optional<br>`trendBy` `string?` Optional | None | None | None | Envelope: `ApiResponse<InventoryAnalyticsResponse>`<br>Data schema: [`InventoryAnalyticsResponse`](#schema-inventoryanalyticsresponse)<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Authorize(Policy = PermissionNames.AnalyticsRead) Omit null/empty query values; client utilities in web/mobile already serialize only populated keys. | See referenced request/response schemas and related route families in the same module. | Use JSON camelCase field names exactly as defined in the schema registry below. |
| Analytics & Reporting | DashboardController | GetSummary | GET | /api/dashboard/summary | Return the executive/admin dashboard summary payload. | Admin | Authorize(Policy = PermissionNames.DashboardRead) | None | None | None | None | Envelope: `ApiResponse<DashboardSummaryResponse>`<br>Data schema: [`DashboardSummaryResponse`](#schema-dashboardsummaryresponse)<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Authorize(Policy = PermissionNames.DashboardRead) | See referenced request/response schemas and related route families in the same module. | Use JSON camelCase field names exactly as defined in the schema registry below. |
| Analytics & Reporting | DashboardController | GetMetrics | GET | /api/dashboard/metrics | Fetch metrics data for `/api/dashboard/metrics`. | Admin | Authorize(Policy = PermissionNames.DashboardRead) | `dateFrom` `date?` Optional<br>`dateTo` `date?` Optional<br>`trendBy` `string?` Optional | None | None | None | Envelope: `ApiResponse<DashboardMetricsResponse>`<br>Data schema: [`DashboardMetricsResponse`](#schema-dashboardmetricsresponse)<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Authorize(Policy = PermissionNames.DashboardRead) Omit null/empty query values; client utilities in web/mobile already serialize only populated keys. | See referenced request/response schemas and related route families in the same module. | Use JSON camelCase field names exactly as defined in the schema registry below. |
| Technician Mobile Workflow | HelperController | Create | POST | /api/helpers | Execute `Create` for `/api/helpers`. | Admin | Authorize(Policy = PermissionNames.UserCreate) | None | None | None | [`CreateHelperProfileRequest`](#schema-createhelperprofilerequest)<br>Field-by-field: schema registry entry. | Envelope: `ApiResponse<HelperDetailResponse>`<br>Data schema: [`HelperDetailResponse`](#schema-helperdetailresponse)<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Authorize(Policy = PermissionNames.UserCreate) Request body fields are defined in [`CreateHelperProfileRequest`](#schema-createhelperprofilerequest). | See referenced request/response schemas and related route families in the same module. | Use JSON camelCase field names exactly as defined in the schema registry below. |
| Technician Mobile Workflow | HelperController | GetList | GET | /api/helpers | Fetch list data for `/api/helpers`. | Admin | Authorize(Policy = PermissionNames.UserRead) | `searchTerm` `string?` Optional<br>`branchId` `integer(int32)?` Optional | None | None | None | Envelope: `ApiResponse<IReadOnlyCollection<HelperListItemResponse>>`<br>Data schema: Array of [`HelperListItemResponse`](#schema-helperlistitemresponse)<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Authorize(Policy = PermissionNames.UserRead) Omit null/empty query values; client utilities in web/mobile already serialize only populated keys. | See referenced request/response schemas and related route families in the same module. | Use JSON camelCase field names exactly as defined in the schema registry below. |
| Technician Mobile Workflow | HelperController | GetDetail | GET | /api/helpers/{helperProfileId:long} | Fetch detail data for `/api/helpers/{helperProfileId:long}`. | Internal | Authorize | None | `helperProfileId` `integer(int64)` Required | None | None | Envelope: `ApiResponse<HelperDetailResponse>`<br>Data schema: [`HelperDetailResponse`](#schema-helperdetailresponse)<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Authorize | See referenced request/response schemas and related route families in the same module. | Use JSON camelCase field names exactly as defined in the schema registry below. |
| Technician Mobile Workflow | HelperController | Assign | POST | /api/helpers/{helperProfileId:long}/assign | Execute `Assign` for `/api/helpers/{helperProfileId:long}/assign`. | Admin | Authorize(Policy = PermissionNames.AssignmentManage) | None | `helperProfileId` `integer(int64)` Required | None | [`AssignHelperToJobRequest`](#schema-assignhelpertojobrequest)<br>Field-by-field: schema registry entry. | Envelope: `ApiResponse<HelperDetailResponse>`<br>Data schema: [`HelperDetailResponse`](#schema-helperdetailresponse)<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Authorize(Policy = PermissionNames.AssignmentManage) Request body fields are defined in [`AssignHelperToJobRequest`](#schema-assignhelpertojobrequest). | See referenced request/response schemas and related route families in the same module. | Use JSON camelCase field names exactly as defined in the schema registry below. |
| Technician Mobile Workflow | HelperController | Release | POST | /api/helpers/{helperProfileId:long}/release | Execute `Release` for `/api/helpers/{helperProfileId:long}/release`. | Admin | Authorize(Policy = PermissionNames.AssignmentManage) | None | `helperProfileId` `integer(int64)` Required | None | [`ReleaseHelperAssignmentRequest`](#schema-releasehelperassignmentrequest)<br>Field-by-field: schema registry entry. | Envelope: `ApiResponse<HelperDetailResponse>`<br>Data schema: [`HelperDetailResponse`](#schema-helperdetailresponse)<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Authorize(Policy = PermissionNames.AssignmentManage) Request body fields are defined in [`ReleaseHelperAssignmentRequest`](#schema-releasehelperassignmentrequest). | See referenced request/response schemas and related route families in the same module. | Use JSON camelCase field names exactly as defined in the schema registry below. |
| Technician Mobile Workflow | HelperController | GetAssignment | GET | /api/helpers/{helperProfileId:long}/assignment | Fetch assignment data for `/api/helpers/{helperProfileId:long}/assignment`. | Internal | Authorize | None | `helperProfileId` `integer(int64)` Required | None | None | Envelope: `ApiResponse<HelperAssignmentDetailResponse>`<br>Data schema: [`HelperAssignmentDetailResponse`](#schema-helperassignmentdetailresponse)<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Authorize | See referenced request/response schemas and related route families in the same module. | Use JSON camelCase field names exactly as defined in the schema registry below. |
| Inventory, Parts & Estimates | ItemController | Create | POST | /api/items | Execute `Create` for `/api/items`. | Admin | Authorize(Policy = PermissionNames.ItemCreate) | None | None | None | [`CreateItemRequest`](#schema-createitemrequest)<br>Field-by-field: schema registry entry. | Envelope: `ApiResponse<ItemResponse>`<br>Data schema: [`ItemResponse`](#schema-itemresponse)<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Authorize(Policy = PermissionNames.ItemCreate) Request body fields are defined in [`CreateItemRequest`](#schema-createitemrequest). | See referenced request/response schemas and related route families in the same module. | Use JSON camelCase field names exactly as defined in the schema registry below. |
| Inventory, Parts & Estimates | ItemController | Update | PUT | /api/items/{id:long} | Execute `Update` for `/api/items/{id:long}`. | Admin | Authorize(Policy = PermissionNames.ItemCreate) | None | `id` `integer(int64)` Required | None | [`UpdateItemRequest`](#schema-updateitemrequest)<br>Field-by-field: schema registry entry. | Envelope: `ApiResponse<ItemResponse>`<br>Data schema: [`ItemResponse`](#schema-itemresponse)<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Authorize(Policy = PermissionNames.ItemCreate) Request body fields are defined in [`UpdateItemRequest`](#schema-updateitemrequest). | See referenced request/response schemas and related route families in the same module. | Use JSON camelCase field names exactly as defined in the schema registry below. |
| Inventory, Parts & Estimates | ItemController | GetItems | GET | /api/items | Fetch items data for `/api/items`. | Admin | Authorize(Policy = PermissionNames.ItemRead) | `searchTerm` `string?` Optional<br>`isActive` `boolean?` Optional<br>`pageNumber` `integer(int32)` Required<br>`pageSize` `integer(int32)` Required | None | None | None | Envelope: `ApiResponse<PagedResult<ItemResponse>>`<br>Data schema: `PagedResult` of [`ItemResponse`](#schema-itemresponse) with `items`, `totalCount`, `pageNumber`, `pageSize`<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Authorize(Policy = PermissionNames.ItemRead) Omit null/empty query values; client utilities in web/mobile already serialize only populated keys. | See referenced request/response schemas and related route families in the same module. | Use JSON camelCase field names exactly as defined in the schema registry below. |
| Inventory, Parts & Estimates | ItemController | GetById | GET | /api/items/{id:long} | Fetch by id data for `/api/items/{id:long}`. | Admin | Authorize(Policy = PermissionNames.ItemRead) | None | `id` `integer(int64)` Required | None | None | Envelope: `ApiResponse<ItemResponse>`<br>Data schema: [`ItemResponse`](#schema-itemresponse)<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Authorize(Policy = PermissionNames.ItemRead) | See referenced request/response schemas and related route families in the same module. | Use JSON camelCase field names exactly as defined in the schema registry below. |
| Website CMS & Marketing / Service Request Management | LeadController | CreateLead | POST | /api/leads | Create lead for `/api/leads`. | Customer | Anonymous | None | None | None | [`CreateLeadRequest`](#schema-createleadrequest)<br>Field-by-field: schema registry entry. | Envelope: `ApiResponse<LeadResponse>`<br>Data schema: [`LeadResponse`](#schema-leadresponse)<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Anonymous Request body fields are defined in [`CreateLeadRequest`](#schema-createleadrequest). | See referenced request/response schemas and related route families in the same module. | Use JSON camelCase field names exactly as defined in the schema registry below. |
| Website CMS & Marketing / Service Request Management | LeadController | GetAnalytics | GET | /api/leads/analytics | Fetch analytics data for `/api/leads/analytics`. | Admin | Authorize(Policy = PermissionNames.ServiceRequestRead) | `fromDate` `date?` Optional<br>`toDate` `date?` Optional | None | None | None | Envelope: `ApiResponse<LeadAnalyticsResponse>`<br>Data schema: [`LeadAnalyticsResponse`](#schema-leadanalyticsresponse)<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Authorize(Policy = PermissionNames.ServiceRequestRead) Omit null/empty query values; client utilities in web/mobile already serialize only populated keys. | See referenced request/response schemas and related route families in the same module. | Use JSON camelCase field names exactly as defined in the schema registry below. |
| Website CMS & Marketing / Service Request Management | LeadController | GetLeads | GET | /api/leads | Fetch leads data for `/api/leads`. | Admin | Authorize(Policy = PermissionNames.ServiceRequestRead) | `searchTerm` `string?` Optional<br>`leadStatus` `string?` Optional<br>`sourceChannel` `string?` Optional<br>`createdFrom` `date?` Optional<br>`createdTo` `date?` Optional<br>`pageNumber` `integer(int32)` Required<br>`pageSize` `integer(int32)` Required | None | None | None | Envelope: `ApiResponse<PagedResult<LeadListItemResponse>>`<br>Data schema: `PagedResult` of [`LeadListItemResponse`](#schema-leadlistitemresponse) with `items`, `totalCount`, `pageNumber`, `pageSize`<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Authorize(Policy = PermissionNames.ServiceRequestRead) Omit null/empty query values; client utilities in web/mobile already serialize only populated keys. | See referenced request/response schemas and related route families in the same module. | Use JSON camelCase field names exactly as defined in the schema registry below. |
| Website CMS & Marketing / Service Request Management | LeadController | GetLeadById | GET | /api/leads/{leadId:long} | Fetch lead by id data for `/api/leads/{leadId:long}`. | Admin | Authorize(Policy = PermissionNames.ServiceRequestRead) | None | `leadId` `integer(int64)` Required | None | None | Envelope: `ApiResponse<LeadDetailResponse>`<br>Data schema: [`LeadDetailResponse`](#schema-leaddetailresponse)<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Authorize(Policy = PermissionNames.ServiceRequestRead) | See referenced request/response schemas and related route families in the same module. | Use JSON camelCase field names exactly as defined in the schema registry below. |
| Website CMS & Marketing / Service Request Management | LeadController | AssignLead | PUT | /api/leads/{leadId:long}/assign | Assign the target record for `/api/leads/{leadId:long}/assign`. | Admin | Authorize(Policy = PermissionNames.ServiceRequestUpdate) | None | `leadId` `integer(int64)` Required | None | [`AssignLeadRequest`](#schema-assignleadrequest)<br>Field-by-field: schema registry entry. | Envelope: `ApiResponse<LeadResponse>`<br>Data schema: [`LeadResponse`](#schema-leadresponse)<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Authorize(Policy = PermissionNames.ServiceRequestUpdate) Request body fields are defined in [`AssignLeadRequest`](#schema-assignleadrequest). | See referenced request/response schemas and related route families in the same module. | Use JSON camelCase field names exactly as defined in the schema registry below. |
| Website CMS & Marketing / Service Request Management | LeadController | UpdateLeadStatus | PUT | /api/leads/{leadId:long}/status | Update lead status for `/api/leads/{leadId:long}/status`. | Admin | Authorize(Policy = PermissionNames.ServiceRequestUpdate) | None | `leadId` `integer(int64)` Required | None | [`UpdateLeadStatusRequest`](#schema-updateleadstatusrequest)<br>Field-by-field: schema registry entry. | Envelope: `ApiResponse<LeadResponse>`<br>Data schema: [`LeadResponse`](#schema-leadresponse)<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Authorize(Policy = PermissionNames.ServiceRequestUpdate) Request body fields are defined in [`UpdateLeadStatusRequest`](#schema-updateleadstatusrequest). | See referenced request/response schemas and related route families in the same module. | Use JSON camelCase field names exactly as defined in the schema registry below. |
| Website CMS & Marketing / Service Request Management | LeadController | ConvertToBooking | POST | /api/leads/{leadId:long}/convert-to-booking | Execute `ConvertToBooking` for `/api/leads/{leadId:long}/convert-to-booking`. | Admin | Authorize(Policy = PermissionNames.BookingCreate) | None | `leadId` `integer(int64)` Required | None | [`ConvertLeadToBookingRequest`](#schema-convertleadtobookingrequest)<br>Field-by-field: schema registry entry. | Envelope: `ApiResponse<LeadResponse>`<br>Data schema: [`LeadResponse`](#schema-leadresponse)<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Authorize(Policy = PermissionNames.BookingCreate) Request body fields are defined in [`ConvertLeadToBookingRequest`](#schema-convertleadtobookingrequest). | See referenced request/response schemas and related route families in the same module. | Use JSON camelCase field names exactly as defined in the schema registry below. |
| Website CMS & Marketing / Service Request Management | LeadController | ConvertToServiceRequest | POST | /api/leads/{leadId:long}/convert-to-sr | Execute `ConvertToServiceRequest` for `/api/leads/{leadId:long}/convert-to-sr`. | Admin | Authorize(Policy = PermissionNames.ServiceRequestCreate) | None | `leadId` `integer(int64)` Required | None | [`ConvertLeadToServiceRequestRequest`](#schema-convertleadtoservicerequestrequest)<br>Field-by-field: schema registry entry. | Envelope: `ApiResponse<LeadResponse>`<br>Data schema: [`LeadResponse`](#schema-leadresponse)<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Authorize(Policy = PermissionNames.ServiceRequestCreate) Request body fields are defined in [`ConvertLeadToServiceRequestRequest`](#schema-convertleadtoservicerequestrequest). | See referenced request/response schemas and related route families in the same module. | Use JSON camelCase field names exactly as defined in the schema registry below. |
| Website CMS & Marketing / Service Request Management | LeadController | AddNote | POST | /api/leads/{leadId:long}/notes | Execute `AddNote` for `/api/leads/{leadId:long}/notes`. | Admin | Authorize(Policy = PermissionNames.ServiceRequestUpdate) | None | `leadId` `integer(int64)` Required | None | [`AddLeadNoteRequest`](#schema-addleadnoterequest)<br>Field-by-field: schema registry entry. | Envelope: `ApiResponse<LeadResponse>`<br>Data schema: [`LeadResponse`](#schema-leadresponse)<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Authorize(Policy = PermissionNames.ServiceRequestUpdate) Request body fields are defined in [`AddLeadNoteRequest`](#schema-addleadnoterequest). | See referenced request/response schemas and related route families in the same module. | Use JSON camelCase field names exactly as defined in the schema registry below. |
| Dispatch, Scheduling & Work Orders | OperationsDashboardController | GetDashboardSummary | GET | /api/operations/dashboard-summary | Return the operational dashboard summary payload. | Admin | Authorize(Policy = PermissionNames.OperationsDashboardRead) | None | None | None | None | Envelope: `ApiResponse<OperationsDashboardSummaryResponse>`<br>Data schema: [`OperationsDashboardSummaryResponse`](#schema-operationsdashboardsummaryresponse)<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Authorize(Policy = PermissionNames.OperationsDashboardRead) | See referenced request/response schemas and related route families in the same module. | Use JSON camelCase field names exactly as defined in the schema registry below. |
| Inventory, Parts & Estimates | PartsReturnController | Create | POST | /api/parts-returns | Execute `Create` for `/api/parts-returns`. | Admin | Authorize(Policy = PermissionNames.StockManage) | None | None | None | [`CreatePartsReturnRequest`](#schema-createpartsreturnrequest)<br>Field-by-field: schema registry entry. | Envelope: `ApiResponse<PartsReturnResponse>`<br>Data schema: [`PartsReturnResponse`](#schema-partsreturnresponse)<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Authorize(Policy = PermissionNames.StockManage) Request body fields are defined in [`CreatePartsReturnRequest`](#schema-createpartsreturnrequest). | See referenced request/response schemas and related route families in the same module. | Use JSON camelCase field names exactly as defined in the schema registry below. |
| Inventory, Parts & Estimates | PartsReturnController | Approve | POST | /api/parts-returns/{partsReturnId:long}/approve | Execute `Approve` for `/api/parts-returns/{partsReturnId:long}/approve`. | Admin | Authorize(Policy = PermissionNames.StockManage) | None | `partsReturnId` `integer(int64)` Required | None | [`ApprovePartsReturnRequest`](#schema-approvepartsreturnrequest)<br>Field-by-field: schema registry entry. | Envelope: `ApiResponse<PartsReturnResponse>`<br>Data schema: [`PartsReturnResponse`](#schema-partsreturnresponse)<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Authorize(Policy = PermissionNames.StockManage) Request body fields are defined in [`ApprovePartsReturnRequest`](#schema-approvepartsreturnrequest). | See referenced request/response schemas and related route families in the same module. | Use JSON camelCase field names exactly as defined in the schema registry below. |
| Access & Security Foundation | PermissionController | Get | GET | /api/permissions | Execute `Get` for `/api/permissions`. | Admin | Authorize(Policy = PermissionNames.PermissionRead) | `pageNumber` `integer(int32)` Required<br>`pageSize` `integer(int32)` Required | None | None | None | Envelope: `ApiResponse<PagedResult<PermissionResponse>>`<br>Data schema: `PagedResult` of [`PermissionResponse`](#schema-permissionresponse) with `items`, `totalCount`, `pageNumber`, `pageSize`<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Authorize(Policy = PermissionNames.PermissionRead) Omit null/empty query values; client utilities in web/mobile already serialize only populated keys. | See referenced request/response schemas and related route families in the same module. | Use JSON camelCase field names exactly as defined in the schema registry below. |
| Analytics & Reporting | ReportController | GetDateRange | GET | /api/reports/date-range | Fetch date range data for `/api/reports/date-range`. | Admin | Authorize(Policy = PermissionNames.ReportRead) | `dateFrom` `date?` Optional<br>`dateTo` `date?` Optional<br>`trendBy` `string?` Optional | None | None | None | Envelope: `ApiResponse<DateRangeReportResponse>`<br>Data schema: [`DateRangeReportResponse`](#schema-daterangereportresponse)<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Authorize(Policy = PermissionNames.ReportRead) Omit null/empty query values; client utilities in web/mobile already serialize only populated keys. | See referenced request/response schemas and related route families in the same module. | Use JSON camelCase field names exactly as defined in the schema registry below. |
| Analytics & Reporting | ReportController | Export | GET | /api/reports/export | Execute `Export` for `/api/reports/export`. | Admin | Authorize(Policy = PermissionNames.ReportRead) | `dateFrom` `date?` Optional<br>`dateTo` `date?` Optional<br>`trendBy` `string?` Optional<br>`format` `string?` Optional | None | None | None | Envelope: `ApiResponse<ReportExportResponse>`<br>Data schema: [`ReportExportResponse`](#schema-reportexportresponse)<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Authorize(Policy = PermissionNames.ReportRead) Omit null/empty query values; client utilities in web/mobile already serialize only populated keys. | See referenced request/response schemas and related route families in the same module. | Use JSON camelCase field names exactly as defined in the schema registry below. |
| Access & Security Foundation | RoleController | Get | GET | /api/roles | Execute `Get` for `/api/roles`. | Admin | Authorize(Policy = PermissionNames.RoleRead) | `pageNumber` `integer(int32)` Required<br>`pageSize` `integer(int32)` Required | None | None | None | Envelope: `ApiResponse<PagedResult<RoleResponse>>`<br>Data schema: `PagedResult` of [`RoleResponse`](#schema-roleresponse) with `items`, `totalCount`, `pageNumber`, `pageSize`<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Authorize(Policy = PermissionNames.RoleRead) Omit null/empty query values; client utilities in web/mobile already serialize only populated keys. | See referenced request/response schemas and related route families in the same module. | Use JSON camelCase field names exactly as defined in the schema registry below. |
| Access & Security Foundation | RoleController | Create | POST | /api/roles | Execute `Create` for `/api/roles`. | Admin | Authorize(Policy = PermissionNames.RoleCreate) | None | None | None | [`CreateRoleRequest`](#schema-createrolerequest)<br>Field-by-field: schema registry entry. | Envelope: `ApiResponse<RoleResponse>`<br>Data schema: [`RoleResponse`](#schema-roleresponse)<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Authorize(Policy = PermissionNames.RoleCreate) Request body fields are defined in [`CreateRoleRequest`](#schema-createrolerequest). | See referenced request/response schemas and related route families in the same module. | Use JSON camelCase field names exactly as defined in the schema registry below. |
| Access & Security Foundation | RoleController | Update | PUT | /api/roles/{roleId:long} | Execute `Update` for `/api/roles/{roleId:long}`. | Admin | Authorize(Policy = PermissionNames.RoleUpdate) | None | `roleId` `integer(int64)` Required | None | [`UpdateRoleRequest`](#schema-updaterolerequest)<br>Field-by-field: schema registry entry. | Envelope: `ApiResponse<RoleResponse>`<br>Data schema: [`RoleResponse`](#schema-roleresponse)<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Authorize(Policy = PermissionNames.RoleUpdate) Request body fields are defined in [`UpdateRoleRequest`](#schema-updaterolerequest). | See referenced request/response schemas and related route families in the same module. | Use JSON camelCase field names exactly as defined in the schema registry below. |
| Technician Mobile Workflow | SkillAssessmentController | Create | POST | /api/technicians/{technicianId:long}/skill-assessments | Execute `Create` for `/api/technicians/{technicianId:long}/skill-assessments`. | Admin | Authorize(Policy = PermissionNames.UserUpdate) | None | `technicianId` `integer(int64)` Required | None | [`CreateSkillAssessmentRequest`](#schema-createskillassessmentrequest)<br>Field-by-field: schema registry entry. | Envelope: `ApiResponse<IReadOnlyCollection<SkillAssessmentDetailResponse>>`<br>Data schema: Array of [`SkillAssessmentDetailResponse`](#schema-skillassessmentdetailresponse)<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Authorize(Policy = PermissionNames.UserUpdate) Request body fields are defined in [`CreateSkillAssessmentRequest`](#schema-createskillassessmentrequest). | See referenced request/response schemas and related route families in the same module. | Use JSON camelCase field names exactly as defined in the schema registry below. |
| Technician Mobile Workflow | SkillAssessmentController | GetList | GET | /api/technicians/{technicianId:long}/skill-assessments | Fetch list data for `/api/technicians/{technicianId:long}/skill-assessments`. | Admin | Authorize(Policy = PermissionNames.TechnicianRead) | None | `technicianId` `integer(int64)` Required | None | None | Envelope: `ApiResponse<IReadOnlyCollection<SkillAssessmentDetailResponse>>`<br>Data schema: Array of [`SkillAssessmentDetailResponse`](#schema-skillassessmentdetailresponse)<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Authorize(Policy = PermissionNames.TechnicianRead) | See referenced request/response schemas and related route families in the same module. | Use JSON camelCase field names exactly as defined in the schema registry below. |
| Technician Mobile Workflow | SkillAssessmentController | SubmitResult | POST | /api/technicians/{technicianId:long}/skill-assessments/{assessmentId:long}/submit-result | Execute `SubmitResult` for `/api/technicians/{technicianId:long}/skill-assessments/{assessmentId:long}/submit-result`. | Admin | Authorize(Policy = PermissionNames.UserUpdate) | None | `technicianId` `integer(int64)` Required<br>`assessmentId` `integer(int64)` Required | None | [`SubmitSkillAssessmentResultRequest`](#schema-submitskillassessmentresultrequest)<br>Field-by-field: schema registry entry. | Envelope: `ApiResponse<IReadOnlyCollection<SkillAssessmentDetailResponse>>`<br>Data schema: Array of [`SkillAssessmentDetailResponse`](#schema-skillassessmentdetailresponse)<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Authorize(Policy = PermissionNames.UserUpdate) Request body fields are defined in [`SubmitSkillAssessmentResultRequest`](#schema-submitskillassessmentresultrequest). | See referenced request/response schemas and related route families in the same module. | Use JSON camelCase field names exactly as defined in the schema registry below. |
| Inventory, Parts & Estimates | StockController | RecordTransaction | POST | /api/stock/transaction | Execute `RecordTransaction` for `/api/stock/transaction`. | Admin | Authorize(Policy = PermissionNames.StockManage) | None | None | None | [`RecordStockTransactionRequest`](#schema-recordstocktransactionrequest)<br>Field-by-field: schema registry entry. | Envelope: `ApiResponse<StockTransactionResponse>`<br>Data schema: [`StockTransactionResponse`](#schema-stocktransactionresponse)<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Authorize(Policy = PermissionNames.StockManage) Request body fields are defined in [`RecordStockTransactionRequest`](#schema-recordstocktransactionrequest). | See referenced request/response schemas and related route families in the same module. | Use JSON camelCase field names exactly as defined in the schema registry below. |
| Inventory, Parts & Estimates | StockController | Transfer | POST | /api/stock/transfer | Execute `Transfer` for `/api/stock/transfer`. | Admin | Authorize(Policy = PermissionNames.StockManage) | None | None | None | [`TransferStockRequest`](#schema-transferstockrequest)<br>Field-by-field: schema registry entry. | Envelope: `ApiResponse<IReadOnlyCollection<StockTransactionResponse>>`<br>Data schema: Array of [`StockTransactionResponse`](#schema-stocktransactionresponse)<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Authorize(Policy = PermissionNames.StockManage) Request body fields are defined in [`TransferStockRequest`](#schema-transferstockrequest). | See referenced request/response schemas and related route families in the same module. | Use JSON camelCase field names exactly as defined in the schema registry below. |
| Inventory, Parts & Estimates | StockController | GetTransactions | GET | /api/stock/transactions | Fetch transactions data for `/api/stock/transactions`. | Admin | Authorize(Policy = PermissionNames.StockRead) | `transactionType` `string?` Optional<br>`itemId` `integer(int64)?` Optional<br>`warehouseId` `integer(int64)?` Optional<br>`technicianId` `integer(int64)?` Optional<br>`jobCardId` `integer(int64)?` Optional<br>`fromDateUtc` `datetime?` Optional<br>`toDateUtc` `datetime?` Optional<br>`pageNumber` `integer(int32)` Required<br>`pageSize` `integer(int32)` Required | None | None | None | Envelope: `ApiResponse<PagedResult<StockTransactionResponse>>`<br>Data schema: `PagedResult` of [`StockTransactionResponse`](#schema-stocktransactionresponse) with `items`, `totalCount`, `pageNumber`, `pageSize`<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Authorize(Policy = PermissionNames.StockRead) Omit null/empty query values; client utilities in web/mobile already serialize only populated keys. | See referenced request/response schemas and related route families in the same module. | Use JSON camelCase field names exactly as defined in the schema registry below. |
| Inventory, Parts & Estimates | SupplierController | CreateClaim | POST | /api/suppliers/claims | Create claim for `/api/suppliers/claims`. | Admin | Authorize(Policy = PermissionNames.StockManage) | None | None | None | [`CreateSupplierClaimRequest`](#schema-createsupplierclaimrequest)<br>Field-by-field: schema registry entry. | Envelope: `ApiResponse<SupplierClaimResponse>`<br>Data schema: [`SupplierClaimResponse`](#schema-supplierclaimresponse)<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Authorize(Policy = PermissionNames.StockManage) Request body fields are defined in [`CreateSupplierClaimRequest`](#schema-createsupplierclaimrequest). | See referenced request/response schemas and related route families in the same module. | Use JSON camelCase field names exactly as defined in the schema registry below. |
| Technician Mobile Workflow | TechnicianActivationController | Activate | POST | /api/technicians/{technicianId:long}/activate | Execute `Activate` for `/api/technicians/{technicianId:long}/activate`. | Admin | Authorize(Policy = PermissionNames.UserUpdate) | None | `technicianId` `integer(int64)` Required | None | [`ActivateTechnicianPhaseERequest`](#schema-activatetechnicianphaseerequest)<br>Field-by-field: schema registry entry. | Envelope: `ApiResponse<TechnicianOnboardingDetailResponse>`<br>Data schema: [`TechnicianOnboardingDetailResponse`](#schema-technicianonboardingdetailresponse)<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Authorize(Policy = PermissionNames.UserUpdate) Request body fields are defined in [`ActivateTechnicianPhaseERequest`](#schema-activatetechnicianphaseerequest). | See referenced request/response schemas and related route families in the same module. | Use JSON camelCase field names exactly as defined in the schema registry below. |
| Technician Mobile Workflow | TechnicianActivationController | Deactivate | POST | /api/technicians/{technicianId:long}/deactivate | Execute `Deactivate` for `/api/technicians/{technicianId:long}/deactivate`. | Admin | Authorize(Policy = PermissionNames.UserUpdate) | None | `technicianId` `integer(int64)` Required | None | [`DeactivateTechnicianRequest`](#schema-deactivatetechnicianrequest)<br>Field-by-field: schema registry entry. | Envelope: `ApiResponse<TechnicianOnboardingDetailResponse>`<br>Data schema: [`TechnicianOnboardingDetailResponse`](#schema-technicianonboardingdetailresponse)<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Authorize(Policy = PermissionNames.UserUpdate) Request body fields are defined in [`DeactivateTechnicianRequest`](#schema-deactivatetechnicianrequest). | See referenced request/response schemas and related route families in the same module. | Use JSON camelCase field names exactly as defined in the schema registry below. |
| Technician Mobile Workflow | TechnicianActivationController | GetHistory | GET | /api/technicians/{technicianId:long}/activation-history | Fetch history data for `/api/technicians/{technicianId:long}/activation-history`. | Admin | Authorize(Policy = PermissionNames.TechnicianRead) | None | `technicianId` `integer(int64)` Required | None | None | Envelope: `ApiResponse<IReadOnlyCollection<TechnicianActivationLogResponse>>`<br>Data schema: Array of [`TechnicianActivationLogResponse`](#schema-technicianactivationlogresponse)<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Authorize(Policy = PermissionNames.TechnicianRead) | See referenced request/response schemas and related route families in the same module. | Use JSON camelCase field names exactly as defined in the schema registry below. |
| Dispatch, Scheduling & Work Orders | TechnicianController | GetTechnicians | GET | /api/technicians | Fetch technicians data for `/api/technicians`. | Admin | Authorize(Policy = PermissionNames.TechnicianRead) | `searchTerm` `string?` Optional<br>`activeOnly` `boolean` Required | None | None | None | Envelope: `ApiResponse<IReadOnlyCollection<TechnicianListItemResponse>>`<br>Data schema: Array of [`TechnicianListItemResponse`](#schema-technicianlistitemresponse)<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Authorize(Policy = PermissionNames.TechnicianRead) Omit null/empty query values; client utilities in web/mobile already serialize only populated keys. | See referenced request/response schemas and related route families in the same module. | Use JSON camelCase field names exactly as defined in the schema registry below. |
| Dispatch, Scheduling & Work Orders | TechnicianController | GetTechnicianAvailability | GET | /api/technicians/availability | Fetch technician availability data for `/api/technicians/availability`. | Admin | Authorize(Policy = PermissionNames.TechnicianRead) | `serviceRequestId` `integer(int64)` Required | None | None | None | Envelope: `ApiResponse<IReadOnlyCollection<TechnicianAvailabilityResponse>>`<br>Data schema: Array of [`TechnicianAvailabilityResponse`](#schema-technicianavailabilityresponse)<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Authorize(Policy = PermissionNames.TechnicianRead) Omit null/empty query values; client utilities in web/mobile already serialize only populated keys. | See referenced request/response schemas and related route families in the same module. | Use JSON camelCase field names exactly as defined in the schema registry below. |
| Technician Mobile Workflow | TechnicianDocumentController | Upload | POST | /api/technicians/{technicianId:long}/documents | Execute `Upload` for `/api/technicians/{technicianId:long}/documents`. | Admin | Authorize(Policy = PermissionNames.UserUpdate) | None | `technicianId` `integer(int64)` Required | None | [`UploadTechnicianDocumentsRequest`](#schema-uploadtechniciandocumentsrequest)<br>Field-by-field: schema registry entry. | Envelope: `ApiResponse<TechnicianOnboardingDetailResponse>`<br>Data schema: [`TechnicianOnboardingDetailResponse`](#schema-technicianonboardingdetailresponse)<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Authorize(Policy = PermissionNames.UserUpdate) Request body fields are defined in [`UploadTechnicianDocumentsRequest`](#schema-uploadtechniciandocumentsrequest). | See referenced request/response schemas and related route families in the same module. | Use JSON camelCase field names exactly as defined in the schema registry below. |
| Technician Mobile Workflow | TechnicianDocumentController | GetList | GET | /api/technicians/{technicianId:long}/documents | Fetch list data for `/api/technicians/{technicianId:long}/documents`. | Admin | Authorize(Policy = PermissionNames.TechnicianRead) | None | `technicianId` `integer(int64)` Required | None | None | Envelope: `ApiResponse<IReadOnlyCollection<TechnicianDocumentDetailResponse>>`<br>Data schema: Array of [`TechnicianDocumentDetailResponse`](#schema-techniciandocumentdetailresponse)<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Authorize(Policy = PermissionNames.TechnicianRead) | See referenced request/response schemas and related route families in the same module. | Use JSON camelCase field names exactly as defined in the schema registry below. |
| Technician Mobile Workflow | TechnicianDocumentController | Verify | POST | /api/technicians/{technicianId:long}/documents/{documentId:long}/verify | Execute `Verify` for `/api/technicians/{technicianId:long}/documents/{documentId:long}/verify`. | Admin | Authorize(Policy = PermissionNames.UserUpdate) | None | `technicianId` `integer(int64)` Required<br>`documentId` `integer(int64)` Required | None | [`VerifyTechnicianDocumentRequest`](#schema-verifytechniciandocumentrequest)<br>Field-by-field: schema registry entry. | Envelope: `ApiResponse<IReadOnlyCollection<TechnicianDocumentDetailResponse>>`<br>Data schema: Array of [`TechnicianDocumentDetailResponse`](#schema-techniciandocumentdetailresponse)<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Authorize(Policy = PermissionNames.UserUpdate) Request body fields are defined in [`VerifyTechnicianDocumentRequest`](#schema-verifytechniciandocumentrequest). | See referenced request/response schemas and related route families in the same module. | Use JSON camelCase field names exactly as defined in the schema registry below. |
| Technician Mobile Workflow | TechnicianDocumentController | Reject | POST | /api/technicians/{technicianId:long}/documents/{documentId:long}/reject | Execute `Reject` for `/api/technicians/{technicianId:long}/documents/{documentId:long}/reject`. | Admin | Authorize(Policy = PermissionNames.UserUpdate) | None | `technicianId` `integer(int64)` Required<br>`documentId` `integer(int64)` Required | None | [`RejectTechnicianDocumentRequest`](#schema-rejecttechniciandocumentrequest)<br>Field-by-field: schema registry entry. | Envelope: `ApiResponse<IReadOnlyCollection<TechnicianDocumentDetailResponse>>`<br>Data schema: Array of [`TechnicianDocumentDetailResponse`](#schema-techniciandocumentdetailresponse)<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Authorize(Policy = PermissionNames.UserUpdate) Request body fields are defined in [`RejectTechnicianDocumentRequest`](#schema-rejecttechniciandocumentrequest). | See referenced request/response schemas and related route families in the same module. | Use JSON camelCase field names exactly as defined in the schema registry below. |
| Technician Mobile Workflow | TechnicianOnboardingController | CreateDraft | POST | /api/technician-onboarding/draft | Create draft for `/api/technician-onboarding/draft`. | Admin | Authorize(Policy = PermissionNames.UserCreate) | None | None | None | [`CreateTechnicianDraftRequest`](#schema-createtechniciandraftrequest)<br>Field-by-field: schema registry entry. | Envelope: `ApiResponse<TechnicianOnboardingResponse>`<br>Data schema: [`TechnicianOnboardingResponse`](#schema-technicianonboardingresponse)<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Authorize(Policy = PermissionNames.UserCreate) Request body fields are defined in [`CreateTechnicianDraftRequest`](#schema-createtechniciandraftrequest). | See referenced request/response schemas and related route families in the same module. | Use JSON camelCase field names exactly as defined in the schema registry below. |
| Technician Mobile Workflow | TechnicianOnboardingController | GetList | GET | /api/technician-onboarding | Fetch list data for `/api/technician-onboarding`. | Admin | Authorize(Policy = PermissionNames.TechnicianRead) | `searchTerm` `string?` Optional<br>`status` `string?` Optional<br>`branchId` `integer(int32)?` Optional | None | None | None | Envelope: `ApiResponse<IReadOnlyCollection<TechnicianOnboardingListItemResponse>>`<br>Data schema: Array of [`TechnicianOnboardingListItemResponse`](#schema-technicianonboardinglistitemresponse)<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Authorize(Policy = PermissionNames.TechnicianRead) Omit null/empty query values; client utilities in web/mobile already serialize only populated keys. | See referenced request/response schemas and related route families in the same module. | Use JSON camelCase field names exactly as defined in the schema registry below. |
| Technician Mobile Workflow | TechnicianOnboardingController | GetDetail | GET | /api/technician-onboarding/{technicianId:long} | Fetch detail data for `/api/technician-onboarding/{technicianId:long}`. | Admin | Authorize(Policy = PermissionNames.TechnicianRead) | None | `technicianId` `integer(int64)` Required | None | None | Envelope: `ApiResponse<TechnicianOnboardingDetailResponse>`<br>Data schema: [`TechnicianOnboardingDetailResponse`](#schema-technicianonboardingdetailresponse)<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Authorize(Policy = PermissionNames.TechnicianRead) | See referenced request/response schemas and related route families in the same module. | Use JSON camelCase field names exactly as defined in the schema registry below. |
| Technician Mobile Workflow | TechnicianOnboardingController | UploadDocuments | POST | /api/technician-onboarding/{technicianId:long}/documents | Execute `UploadDocuments` for `/api/technician-onboarding/{technicianId:long}/documents`. | Admin | Authorize(Policy = PermissionNames.UserUpdate) | None | `technicianId` `integer(int64)` Required | None | [`UploadTechnicianDocumentsRequest`](#schema-uploadtechniciandocumentsrequest)<br>Field-by-field: schema registry entry. | Envelope: `ApiResponse<TechnicianOnboardingResponse>`<br>Data schema: [`TechnicianOnboardingResponse`](#schema-technicianonboardingresponse)<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Authorize(Policy = PermissionNames.UserUpdate) Request body fields are defined in [`UploadTechnicianDocumentsRequest`](#schema-uploadtechniciandocumentsrequest). | See referenced request/response schemas and related route families in the same module. | Use JSON camelCase field names exactly as defined in the schema registry below. |
| Technician Mobile Workflow | TechnicianOnboardingController | Activate | POST | /api/technician-onboarding/{technicianId:long}/activate | Execute `Activate` for `/api/technician-onboarding/{technicianId:long}/activate`. | Admin | Authorize(Policy = PermissionNames.UserUpdate) | None | `technicianId` `integer(int64)` Required | None | [`ActivateTechnicianRequest`](#schema-activatetechnicianrequest)<br>Field-by-field: schema registry entry. | Envelope: `ApiResponse<TechnicianOnboardingResponse>`<br>Data schema: [`TechnicianOnboardingResponse`](#schema-technicianonboardingresponse)<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Authorize(Policy = PermissionNames.UserUpdate) Request body fields are defined in [`ActivateTechnicianRequest`](#schema-activatetechnicianrequest). | See referenced request/response schemas and related route families in the same module. | Use JSON camelCase field names exactly as defined in the schema registry below. |
| Inventory, Parts & Estimates / Technician Mobile Workflow | TechnicianStockController | Assign | POST | /api/technicians/{id:long}/stock-assign | Execute `Assign` for `/api/technicians/{id:long}/stock-assign`. | Internal | Authorize(Policy = PermissionNames.StockManage) | None | `id` `integer(int64)` Required | None | [`AssignStockToTechnicianRequest`](#schema-assignstocktotechnicianrequest)<br>Field-by-field: schema registry entry. | Envelope: `ApiResponse<IReadOnlyCollection<StockTransactionResponse>>`<br>Data schema: Array of [`StockTransactionResponse`](#schema-stocktransactionresponse)<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Authorize(Policy = PermissionNames.StockManage) Request body fields are defined in [`AssignStockToTechnicianRequest`](#schema-assignstocktotechnicianrequest). | See referenced request/response schemas and related route families in the same module. | Use JSON camelCase field names exactly as defined in the schema registry below. |
| Inventory, Parts & Estimates / Technician Mobile Workflow | TechnicianStockController | GetTechnicianStock | GET | /api/technicians/{id:long}/stock | Fetch technician stock data for `/api/technicians/{id:long}/stock`. | Internal | Authorize(Policy = PermissionNames.StockRead) | None | `id` `integer(int64)` Required | None | None | Envelope: `ApiResponse<TechnicianStockResponse>`<br>Data schema: [`TechnicianStockResponse`](#schema-technicianstockresponse)<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Authorize(Policy = PermissionNames.StockRead) | See referenced request/response schemas and related route families in the same module. | Use JSON camelCase field names exactly as defined in the schema registry below. |
| Technician Mobile Workflow | TrainingController | Create | POST | /api/technicians/{technicianId:long}/training-records | Execute `Create` for `/api/technicians/{technicianId:long}/training-records`. | Admin | Authorize(Policy = PermissionNames.UserUpdate) | None | `technicianId` `integer(int64)` Required | None | [`CreateTrainingRecordRequest`](#schema-createtrainingrecordrequest)<br>Field-by-field: schema registry entry. | Envelope: `ApiResponse<IReadOnlyCollection<TrainingRecordDetailResponse>>`<br>Data schema: Array of [`TrainingRecordDetailResponse`](#schema-trainingrecorddetailresponse)<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Authorize(Policy = PermissionNames.UserUpdate) Request body fields are defined in [`CreateTrainingRecordRequest`](#schema-createtrainingrecordrequest). | See referenced request/response schemas and related route families in the same module. | Use JSON camelCase field names exactly as defined in the schema registry below. |
| Technician Mobile Workflow | TrainingController | GetList | GET | /api/technicians/{technicianId:long}/training-records | Fetch list data for `/api/technicians/{technicianId:long}/training-records`. | Admin | Authorize(Policy = PermissionNames.TechnicianRead) | None | `technicianId` `integer(int64)` Required | None | None | Envelope: `ApiResponse<IReadOnlyCollection<TrainingRecordDetailResponse>>`<br>Data schema: Array of [`TrainingRecordDetailResponse`](#schema-trainingrecorddetailresponse)<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Authorize(Policy = PermissionNames.TechnicianRead) | See referenced request/response schemas and related route families in the same module. | Use JSON camelCase field names exactly as defined in the schema registry below. |
| Technician Mobile Workflow | TrainingController | Complete | POST | /api/technicians/{technicianId:long}/training-records/{trainingRecordId:long}/complete | Execute `Complete` for `/api/technicians/{technicianId:long}/training-records/{trainingRecordId:long}/complete`. | Admin | Authorize(Policy = PermissionNames.UserUpdate) | None | `technicianId` `integer(int64)` Required<br>`trainingRecordId` `integer(int64)` Required | None | [`CompleteTrainingRecordRequest`](#schema-completetrainingrecordrequest)<br>Field-by-field: schema registry entry. | Envelope: `ApiResponse<IReadOnlyCollection<TrainingRecordDetailResponse>>`<br>Data schema: Array of [`TrainingRecordDetailResponse`](#schema-trainingrecorddetailresponse)<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Authorize(Policy = PermissionNames.UserUpdate) Request body fields are defined in [`CompleteTrainingRecordRequest`](#schema-completetrainingrecordrequest). | See referenced request/response schemas and related route families in the same module. | Use JSON camelCase field names exactly as defined in the schema registry below. |
| Inventory, Parts & Estimates | WarehouseController | Create | POST | /api/warehouses | Execute `Create` for `/api/warehouses`. | Admin | Authorize(Policy = PermissionNames.WarehouseCreate) | None | None | None | [`CreateWarehouseRequest`](#schema-createwarehouserequest)<br>Field-by-field: schema registry entry. | Envelope: `ApiResponse<WarehouseResponse>`<br>Data schema: [`WarehouseResponse`](#schema-warehouseresponse)<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Authorize(Policy = PermissionNames.WarehouseCreate) Request body fields are defined in [`CreateWarehouseRequest`](#schema-createwarehouserequest). | See referenced request/response schemas and related route families in the same module. | Use JSON camelCase field names exactly as defined in the schema registry below. |
| Inventory, Parts & Estimates | WarehouseController | GetWarehouses | GET | /api/warehouses | Fetch warehouses data for `/api/warehouses`. | Admin | Authorize(Policy = PermissionNames.WarehouseRead) | `searchTerm` `string?` Optional<br>`isActive` `boolean?` Optional | None | None | None | Envelope: `ApiResponse<IReadOnlyCollection<WarehouseResponse>>`<br>Data schema: Array of [`WarehouseResponse`](#schema-warehouseresponse)<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Authorize(Policy = PermissionNames.WarehouseRead) Omit null/empty query values; client utilities in web/mobile already serialize only populated keys. | See referenced request/response schemas and related route families in the same module. | Use JSON camelCase field names exactly as defined in the schema registry below. |
| Inventory, Parts & Estimates | WarehouseController | GetWarehouseStock | GET | /api/warehouses/{id:long}/stock | Fetch warehouse stock data for `/api/warehouses/{id:long}/stock`. | Admin | Authorize(Policy = PermissionNames.WarehouseRead) | None | `id` `integer(int64)` Required | None | None | Envelope: `ApiResponse<WarehouseStockResponse>`<br>Data schema: [`WarehouseStockResponse`](#schema-warehousestockresponse)<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Authorize(Policy = PermissionNames.WarehouseRead) | See referenced request/response schemas and related route families in the same module. | Use JSON camelCase field names exactly as defined in the schema registry below. |

## 13. Shared / Common APIs

| Module | API Group / Controller | API Name | HTTP | Route | Purpose / Description | Used By | Authentication Requirement | Query Parameters | Path Parameters | Headers | Request Body / Field Definition | Response Structure / Field Definition | Success / Error Example | Validation / Business Rule Notes | Dependency / Linked APIs | Integration Notes |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| Access & Security Foundation | HealthController | Get | GET | /api/health | Execute `Get` for `/api/health`. | Both | Anonymous | None | None | None | None | Envelope: `ApiResponse<object>`<br>Data schema: [`object`](#schema-object)<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Anonymous | See referenced request/response schemas and related route families in the same module. | Use JSON camelCase field names exactly as defined in the schema registry below. |

## 14. Internal / System APIs

| Module | API Group / Controller | API Name | HTTP | Route | Purpose / Description | Used By | Authentication Requirement | Query Parameters | Path Parameters | Headers | Request Body / Field Definition | Response Structure / Field Definition | Success / Error Example | Validation / Business Rule Notes | Dependency / Linked APIs | Integration Notes |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| Technician Mobile Workflow | DiagnosisController | GetIssueLookups | GET | /api/diagnosis/lookups/issues | Fetch issue lookups data for `/api/diagnosis/lookups/issues`. | Internal | Authorize(Roles = RoleNames.Technician) | `search` `string?` Optional | None | None | None | Envelope: `ApiResponse<IReadOnlyCollection<DiagnosisLookupItemResponse>>`<br>Data schema: Array of [`DiagnosisLookupItemResponse`](#schema-diagnosislookupitemresponse)<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Authorize(Roles = RoleNames.Technician) Omit null/empty query values; client utilities in web/mobile already serialize only populated keys. | Technician execution flow uses these routes together for job detail, status, diagnosis, checklist, attachments, notes, timeline, and parts consumption. | Partner app should treat path `{id}` as the technician-visible job/service-request identifier used throughout the execution flow. |
| Technician Mobile Workflow | DiagnosisController | GetResultLookups | GET | /api/diagnosis/lookups/results | Fetch result lookups data for `/api/diagnosis/lookups/results`. | Internal | Authorize(Roles = RoleNames.Technician) | `search` `string?` Optional | None | None | None | Envelope: `ApiResponse<IReadOnlyCollection<DiagnosisLookupItemResponse>>`<br>Data schema: Array of [`DiagnosisLookupItemResponse`](#schema-diagnosislookupitemresponse)<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Authorize(Roles = RoleNames.Technician) Omit null/empty query values; client utilities in web/mobile already serialize only populated keys. | Technician execution flow uses these routes together for job detail, status, diagnosis, checklist, attachments, notes, timeline, and parts consumption. | Partner app should treat path `{id}` as the technician-visible job/service-request identifier used throughout the execution flow. |
| Technician Mobile Workflow | DiagnosisController | SaveDiagnosis | POST | /api/technician-jobs/{id:long}/diagnosis | Persist submitted details for `/api/technician-jobs/{id:long}/diagnosis`. | Internal | Authorize(Roles = RoleNames.Technician) | None | `id` `integer(int64)` Required | None | [`SaveJobDiagnosisRequest`](#schema-savejobdiagnosisrequest)<br>Field-by-field: schema registry entry. | Envelope: `ApiResponse<JobDiagnosisSummaryResponse>`<br>Data schema: [`JobDiagnosisSummaryResponse`](#schema-jobdiagnosissummaryresponse)<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Authorize(Roles = RoleNames.Technician) Request body fields are defined in [`SaveJobDiagnosisRequest`](#schema-savejobdiagnosisrequest). | Technician execution flow uses these routes together for job detail, status, diagnosis, checklist, attachments, notes, timeline, and parts consumption. | Partner app should treat path `{id}` as the technician-visible job/service-request identifier used throughout the execution flow. |
| Technician Mobile Workflow / Job Tracking Module | FieldExecutionController | MarkEnRoute | POST | /api/technician-jobs/{id:long}/mark-enroute | Apply the target execution state for `/api/technician-jobs/{id:long}/mark-enroute`. | Internal | Authorize(Roles = RoleNames.Technician) | None | `id` `integer(int64)` Required | None | [`UpdateTechnicianJobStatusRequest`](#schema-updatetechnicianjobstatusrequest)<br>Field-by-field: schema registry entry. | Envelope: `ApiResponse<TechnicianJobDetailResponse>`<br>Data schema: [`TechnicianJobDetailResponse`](#schema-technicianjobdetailresponse)<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Authorize(Roles = RoleNames.Technician) Request body fields are defined in [`UpdateTechnicianJobStatusRequest`](#schema-updatetechnicianjobstatusrequest). | Technician execution flow uses these routes together for job detail, status, diagnosis, checklist, attachments, notes, timeline, and parts consumption. | Partner app should treat path `{id}` as the technician-visible job/service-request identifier used throughout the execution flow. |
| Technician Mobile Workflow / Job Tracking Module | FieldExecutionController | MarkReached | POST | /api/technician-jobs/{id:long}/mark-reached | Apply the target execution state for `/api/technician-jobs/{id:long}/mark-reached`. | Internal | Authorize(Roles = RoleNames.Technician) | None | `id` `integer(int64)` Required | None | [`UpdateTechnicianJobStatusRequest`](#schema-updatetechnicianjobstatusrequest)<br>Field-by-field: schema registry entry. | Envelope: `ApiResponse<TechnicianJobDetailResponse>`<br>Data schema: [`TechnicianJobDetailResponse`](#schema-technicianjobdetailresponse)<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Authorize(Roles = RoleNames.Technician) Request body fields are defined in [`UpdateTechnicianJobStatusRequest`](#schema-updatetechnicianjobstatusrequest). | Technician execution flow uses these routes together for job detail, status, diagnosis, checklist, attachments, notes, timeline, and parts consumption. | Partner app should treat path `{id}` as the technician-visible job/service-request identifier used throughout the execution flow. |
| Technician Mobile Workflow / Job Tracking Module | FieldExecutionController | StartWork | POST | /api/technician-jobs/{id:long}/start-work | Execute `StartWork` for `/api/technician-jobs/{id:long}/start-work`. | Internal | Authorize(Roles = RoleNames.Technician) | None | `id` `integer(int64)` Required | None | [`UpdateTechnicianJobStatusRequest`](#schema-updatetechnicianjobstatusrequest)<br>Field-by-field: schema registry entry. | Envelope: `ApiResponse<TechnicianJobDetailResponse>`<br>Data schema: [`TechnicianJobDetailResponse`](#schema-technicianjobdetailresponse)<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Authorize(Roles = RoleNames.Technician) Request body fields are defined in [`UpdateTechnicianJobStatusRequest`](#schema-updatetechnicianjobstatusrequest). | Technician execution flow uses these routes together for job detail, status, diagnosis, checklist, attachments, notes, timeline, and parts consumption. | Partner app should treat path `{id}` as the technician-visible job/service-request identifier used throughout the execution flow. |
| Technician Mobile Workflow / Job Tracking Module | FieldExecutionController | MarkInProgress | POST | /api/technician-jobs/{id:long}/mark-in-progress | Apply the target execution state for `/api/technician-jobs/{id:long}/mark-in-progress`. | Internal | Authorize(Roles = RoleNames.Technician) | None | `id` `integer(int64)` Required | None | [`UpdateTechnicianJobStatusRequest`](#schema-updatetechnicianjobstatusrequest)<br>Field-by-field: schema registry entry. | Envelope: `ApiResponse<TechnicianJobDetailResponse>`<br>Data schema: [`TechnicianJobDetailResponse`](#schema-technicianjobdetailresponse)<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Authorize(Roles = RoleNames.Technician) Request body fields are defined in [`UpdateTechnicianJobStatusRequest`](#schema-updatetechnicianjobstatusrequest). | Technician execution flow uses these routes together for job detail, status, diagnosis, checklist, attachments, notes, timeline, and parts consumption. | Partner app should treat path `{id}` as the technician-visible job/service-request identifier used throughout the execution flow. |
| Technician Mobile Workflow / Job Tracking Module | FieldExecutionController | MarkWorkCompleted | POST | /api/technician-jobs/{id:long}/mark-work-completed | Apply the target execution state for `/api/technician-jobs/{id:long}/mark-work-completed`. | Internal | Authorize(Roles = RoleNames.Technician) | None | `id` `integer(int64)` Required | None | [`UpdateTechnicianJobStatusRequest`](#schema-updatetechnicianjobstatusrequest)<br>Field-by-field: schema registry entry. | Envelope: `ApiResponse<TechnicianJobDetailResponse>`<br>Data schema: [`TechnicianJobDetailResponse`](#schema-technicianjobdetailresponse)<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Authorize(Roles = RoleNames.Technician) Request body fields are defined in [`UpdateTechnicianJobStatusRequest`](#schema-updatetechnicianjobstatusrequest). | Technician execution flow uses these routes together for job detail, status, diagnosis, checklist, attachments, notes, timeline, and parts consumption. | Partner app should treat path `{id}` as the technician-visible job/service-request identifier used throughout the execution flow. |
| Technician Mobile Workflow / Job Tracking Module | FieldExecutionController | SubmitForClosure | POST | /api/technician-jobs/{id:long}/submit-for-closure | Execute `SubmitForClosure` for `/api/technician-jobs/{id:long}/submit-for-closure`. | Internal | Authorize(Roles = RoleNames.Technician) | None | `id` `integer(int64)` Required | None | [`UpdateTechnicianJobStatusRequest`](#schema-updatetechnicianjobstatusrequest)<br>Field-by-field: schema registry entry. | Envelope: `ApiResponse<TechnicianJobDetailResponse>`<br>Data schema: [`TechnicianJobDetailResponse`](#schema-technicianjobdetailresponse)<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Authorize(Roles = RoleNames.Technician) Request body fields are defined in [`UpdateTechnicianJobStatusRequest`](#schema-updatetechnicianjobstatusrequest). | Technician execution flow uses these routes together for job detail, status, diagnosis, checklist, attachments, notes, timeline, and parts consumption. | Partner app should treat path `{id}` as the technician-visible job/service-request identifier used throughout the execution flow. |
| Technician Mobile Workflow / Job Tracking Module | FieldExecutionController | SaveNote | POST | /api/technician-jobs/{id:long}/notes | Persist submitted details for `/api/technician-jobs/{id:long}/notes`. | Internal | Authorize(Roles = RoleNames.Technician) | None | `id` `integer(int64)` Required | None | [`SaveJobExecutionNoteRequest`](#schema-savejobexecutionnoterequest)<br>Field-by-field: schema registry entry. | Envelope: `ApiResponse<JobExecutionNoteResponse>`<br>Data schema: [`JobExecutionNoteResponse`](#schema-jobexecutionnoteresponse)<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Authorize(Roles = RoleNames.Technician) Request body fields are defined in [`SaveJobExecutionNoteRequest`](#schema-savejobexecutionnoterequest). | Technician execution flow uses these routes together for job detail, status, diagnosis, checklist, attachments, notes, timeline, and parts consumption. | Partner app should treat path `{id}` as the technician-visible job/service-request identifier used throughout the execution flow. |
| Technician Mobile Workflow / Job Tracking Module | FieldExecutionController | GetTimeline | GET | /api/technician-jobs/{id:long}/timeline | Fetch timeline data for `/api/technician-jobs/{id:long}/timeline`. | Internal | Authorize(Roles = RoleNames.Technician) | None | `id` `integer(int64)` Required | None | None | Envelope: `ApiResponse<IReadOnlyCollection<JobExecutionTimelineItemResponse>>`<br>Data schema: Array of [`JobExecutionTimelineItemResponse`](#schema-jobexecutiontimelineitemresponse)<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Authorize(Roles = RoleNames.Technician) | Technician execution flow uses these routes together for job detail, status, diagnosis, checklist, attachments, notes, timeline, and parts consumption. | Partner app should treat path `{id}` as the technician-visible job/service-request identifier used throughout the execution flow. |
| Technician Mobile Workflow | HelperAttendanceController | CheckIn | POST | /api/helpers/{helperProfileId:long}/attendance/check-in | Execute `CheckIn` for `/api/helpers/{helperProfileId:long}/attendance/check-in`. | Internal | Authorize | None | `helperProfileId` `integer(int64)` Required | None | [`CheckInHelperAttendanceRequest`](#schema-checkinhelperattendancerequest)<br>Field-by-field: schema registry entry. | Envelope: `ApiResponse<IReadOnlyCollection<HelperAttendanceResponse>>`<br>Data schema: Array of [`HelperAttendanceResponse`](#schema-helperattendanceresponse)<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Authorize Request body fields are defined in [`CheckInHelperAttendanceRequest`](#schema-checkinhelperattendancerequest). | See referenced request/response schemas and related route families in the same module. | Use JSON camelCase field names exactly as defined in the schema registry below. |
| Technician Mobile Workflow | HelperAttendanceController | CheckOut | POST | /api/helpers/{helperProfileId:long}/attendance/check-out | Execute `CheckOut` for `/api/helpers/{helperProfileId:long}/attendance/check-out`. | Internal | Authorize | None | `helperProfileId` `integer(int64)` Required | None | [`CheckOutHelperAttendanceRequest`](#schema-checkouthelperattendancerequest)<br>Field-by-field: schema registry entry. | Envelope: `ApiResponse<IReadOnlyCollection<HelperAttendanceResponse>>`<br>Data schema: Array of [`HelperAttendanceResponse`](#schema-helperattendanceresponse)<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Authorize Request body fields are defined in [`CheckOutHelperAttendanceRequest`](#schema-checkouthelperattendancerequest). | See referenced request/response schemas and related route families in the same module. | Use JSON camelCase field names exactly as defined in the schema registry below. |
| Technician Mobile Workflow | HelperAttendanceController | GetList | GET | /api/helpers/{helperProfileId:long}/attendance | Fetch list data for `/api/helpers/{helperProfileId:long}/attendance`. | Internal | Authorize | None | `helperProfileId` `integer(int64)` Required | None | None | Envelope: `ApiResponse<IReadOnlyCollection<HelperAttendanceResponse>>`<br>Data schema: Array of [`HelperAttendanceResponse`](#schema-helperattendanceresponse)<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Authorize | See referenced request/response schemas and related route families in the same module. | Use JSON camelCase field names exactly as defined in the schema registry below. |
| Technician Mobile Workflow | HelperTaskController | GetList | GET | /api/helpers/{helperProfileId:long}/tasks | Fetch list data for `/api/helpers/{helperProfileId:long}/tasks`. | Internal | Authorize | None | `helperProfileId` `integer(int64)` Required | None | None | Envelope: `ApiResponse<IReadOnlyCollection<HelperTaskChecklistResponse>>`<br>Data schema: Array of [`HelperTaskChecklistResponse`](#schema-helpertaskchecklistresponse)<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Authorize | See referenced request/response schemas and related route families in the same module. | Use JSON camelCase field names exactly as defined in the schema registry below. |
| Technician Mobile Workflow | HelperTaskController | Respond | POST | /api/helpers/{helperProfileId:long}/tasks/{taskId:long}/respond | Execute `Respond` for `/api/helpers/{helperProfileId:long}/tasks/{taskId:long}/respond`. | Internal | Authorize | None | `helperProfileId` `integer(int64)` Required<br>`taskId` `integer(int64)` Required | None | [`SaveHelperTaskResponseRequest`](#schema-savehelpertaskresponserequest)<br>Field-by-field: schema registry entry. | Envelope: `ApiResponse<IReadOnlyCollection<HelperTaskChecklistResponse>>`<br>Data schema: Array of [`HelperTaskChecklistResponse`](#schema-helpertaskchecklistresponse)<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Authorize Request body fields are defined in [`SaveHelperTaskResponseRequest`](#schema-savehelpertaskresponserequest). | See referenced request/response schemas and related route families in the same module. | Use JSON camelCase field names exactly as defined in the schema registry below. |
| Technician Mobile Workflow | HelperTaskController | UploadPhoto | POST | /api/helpers/{helperProfileId:long}/tasks/{taskId:long}/upload-photo | Execute `UploadPhoto` for `/api/helpers/{helperProfileId:long}/tasks/{taskId:long}/upload-photo`. | Internal | Authorize | None | `helperProfileId` `integer(int64)` Required<br>`taskId` `integer(int64)` Required | None | [`UploadHelperTaskPhotoRequest`](#schema-uploadhelpertaskphotorequest)<br>Field-by-field: schema registry entry. | Envelope: `ApiResponse<IReadOnlyCollection<HelperTaskChecklistResponse>>`<br>Data schema: Array of [`HelperTaskChecklistResponse`](#schema-helpertaskchecklistresponse)<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Authorize Request body fields are defined in [`UploadHelperTaskPhotoRequest`](#schema-uploadhelpertaskphotorequest). | See referenced request/response schemas and related route families in the same module. | Use JSON camelCase field names exactly as defined in the schema registry below. |
| Technician Mobile Workflow | JobAttachmentController | SaveAttachment | POST | /api/technician-jobs/{id:long}/attachments | Persist submitted details for `/api/technician-jobs/{id:long}/attachments`. | Internal | Authorize(Roles = RoleNames.Technician) | None | `id` `integer(int64)` Required | None | [`SaveJobAttachmentRequest`](#schema-savejobattachmentrequest)<br>Field-by-field: schema registry entry. | Envelope: `ApiResponse<JobAttachmentResponse>`<br>Data schema: [`JobAttachmentResponse`](#schema-jobattachmentresponse)<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Authorize(Roles = RoleNames.Technician) Request body fields are defined in [`SaveJobAttachmentRequest`](#schema-savejobattachmentrequest). | Technician execution flow uses these routes together for job detail, status, diagnosis, checklist, attachments, notes, timeline, and parts consumption. | Partner app should treat path `{id}` as the technician-visible job/service-request identifier used throughout the execution flow. |
| Technician Mobile Workflow | JobAttachmentController | GetAttachments | GET | /api/technician-jobs/{id:long}/attachments | Fetch attachments data for `/api/technician-jobs/{id:long}/attachments`. | Internal | Authorize(Roles = RoleNames.Technician) | None | `id` `integer(int64)` Required | None | None | Envelope: `ApiResponse<IReadOnlyCollection<JobAttachmentResponse>>`<br>Data schema: Array of [`JobAttachmentResponse`](#schema-jobattachmentresponse)<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Authorize(Roles = RoleNames.Technician) | Technician execution flow uses these routes together for job detail, status, diagnosis, checklist, attachments, notes, timeline, and parts consumption. | Partner app should treat path `{id}` as the technician-visible job/service-request identifier used throughout the execution flow. |
| Technician Mobile Workflow | JobChecklistController | GetChecklist | GET | /api/technician-jobs/{id:long}/checklist | Fetch checklist data for `/api/technician-jobs/{id:long}/checklist`. | Internal | Authorize(Roles = RoleNames.Technician) | None | `id` `integer(int64)` Required | None | None | Envelope: `ApiResponse<IReadOnlyCollection<JobChecklistItemResponse>>`<br>Data schema: Array of [`JobChecklistItemResponse`](#schema-jobchecklistitemresponse)<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Authorize(Roles = RoleNames.Technician) | Technician execution flow uses these routes together for job detail, status, diagnosis, checklist, attachments, notes, timeline, and parts consumption. | Partner app should treat path `{id}` as the technician-visible job/service-request identifier used throughout the execution flow. |
| Technician Mobile Workflow | JobChecklistController | SaveChecklist | POST | /api/technician-jobs/{id:long}/checklist | Persist submitted details for `/api/technician-jobs/{id:long}/checklist`. | Internal | Authorize(Roles = RoleNames.Technician) | None | `id` `integer(int64)` Required | None | [`SaveJobChecklistResponseRequest`](#schema-savejobchecklistresponserequest)<br>Field-by-field: schema registry entry. | Envelope: `ApiResponse<IReadOnlyCollection<JobChecklistItemResponse>>`<br>Data schema: Array of [`JobChecklistItemResponse`](#schema-jobchecklistitemresponse)<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Authorize(Roles = RoleNames.Technician) Request body fields are defined in [`SaveJobChecklistResponseRequest`](#schema-savejobchecklistresponserequest). | Technician execution flow uses these routes together for job detail, status, diagnosis, checklist, attachments, notes, timeline, and parts consumption. | Partner app should treat path `{id}` as the technician-visible job/service-request identifier used throughout the execution flow. |
| Inventory, Parts & Estimates / Technician Mobile Workflow | JobConsumptionController | ConsumeParts | POST | /api/jobs/{jobCardId:long}/consume-parts | Execute `ConsumeParts` for `/api/jobs/{jobCardId:long}/consume-parts`. | Internal | Authorize(Policy = PermissionNames.JobConsumptionCreate) | None | `jobCardId` `integer(int64)` Required | None | [`ConsumeJobPartsRequest`](#schema-consumejobpartsrequest)<br>Field-by-field: schema registry entry. | Envelope: `ApiResponse<JobPartConsumptionSummaryResponse>`<br>Data schema: [`JobPartConsumptionSummaryResponse`](#schema-jobpartconsumptionsummaryresponse)<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Authorize(Policy = PermissionNames.JobConsumptionCreate) Request body fields are defined in [`ConsumeJobPartsRequest`](#schema-consumejobpartsrequest). | Technician execution flow uses these routes together for job detail, status, diagnosis, checklist, attachments, notes, timeline, and parts consumption. | Partner app should treat path `{id}` as the technician-visible job/service-request identifier used throughout the execution flow. |
| Inventory, Parts & Estimates / Technician Mobile Workflow | JobConsumptionController | GetConsumption | GET | /api/jobs/{jobCardId:long}/consumption | Fetch consumption data for `/api/jobs/{jobCardId:long}/consumption`. | Internal | Authorize(Policy = PermissionNames.JobConsumptionRead) | None | `jobCardId` `integer(int64)` Required | None | None | Envelope: `ApiResponse<JobPartConsumptionSummaryResponse>`<br>Data schema: [`JobPartConsumptionSummaryResponse`](#schema-jobpartconsumptionsummaryresponse)<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Authorize(Policy = PermissionNames.JobConsumptionRead) | Technician execution flow uses these routes together for job detail, status, diagnosis, checklist, attachments, notes, timeline, and parts consumption. | Partner app should treat path `{id}` as the technician-visible job/service-request identifier used throughout the execution flow. |
| Access & Security Foundation / Internal System Monitoring | SystemHealthController | Get | GET | /api/system-health | Execute `Get` for `/api/system-health`. | Internal/System | Authorize(Policy = PermissionNames.HealthRead) | None | None | None | None | Envelope: `ApiResponse<SystemHealthResponse>`<br>Data schema: [`SystemHealthResponse`](#schema-systemhealthresponse)<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Authorize(Policy = PermissionNames.HealthRead) | See referenced request/response schemas and related route families in the same module. | Use JSON camelCase field names exactly as defined in the schema registry below. |
| Technician Mobile Workflow | TechnicianJobController | GetTechnicianJobs | GET | /api/technician-jobs | Fetch technician jobs data for `/api/technician-jobs`. | Internal | Authorize(Roles = RoleNames.Technician) | `status` `string?` Optional<br>`slotDate` `date?` Optional<br>`pageNumber` `integer(int32)` Required<br>`pageSize` `integer(int32)` Required | None | None | None | Envelope: `ApiResponse<PagedResult<TechnicianJobListItemResponse>>`<br>Data schema: `PagedResult` of [`TechnicianJobListItemResponse`](#schema-technicianjoblistitemresponse) with `items`, `totalCount`, `pageNumber`, `pageSize`<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Authorize(Roles = RoleNames.Technician) Omit null/empty query values; client utilities in web/mobile already serialize only populated keys. | Technician execution flow uses these routes together for job detail, status, diagnosis, checklist, attachments, notes, timeline, and parts consumption. | Partner app should treat path `{id}` as the technician-visible job/service-request identifier used throughout the execution flow. |
| Technician Mobile Workflow | TechnicianJobController | GetTechnicianJobById | GET | /api/technician-jobs/{id:long} | Fetch technician job by id data for `/api/technician-jobs/{id:long}`. | Internal | Authorize(Roles = RoleNames.Technician) | None | `id` `integer(int64)` Required | None | None | Envelope: `ApiResponse<TechnicianJobDetailResponse>`<br>Data schema: [`TechnicianJobDetailResponse`](#schema-technicianjobdetailresponse)<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Authorize(Roles = RoleNames.Technician) | Technician execution flow uses these routes together for job detail, status, diagnosis, checklist, attachments, notes, timeline, and parts consumption. | Partner app should treat path `{id}` as the technician-visible job/service-request identifier used throughout the execution flow. |
| Technician Mobile Workflow | TechnicianJobController | GetMyJobs | GET | /api/technician-jobs/my-jobs | Fetch my jobs data for `/api/technician-jobs/my-jobs`. | Internal | Authorize(Roles = RoleNames.Technician) | `status` `string?` Optional<br>`slotDate` `date?` Optional<br>`pageNumber` `integer(int32)` Required<br>`pageSize` `integer(int32)` Required | None | None | None | Envelope: `ApiResponse<PagedResult<TechnicianJobListItemResponse>>`<br>Data schema: `PagedResult` of [`TechnicianJobListItemResponse`](#schema-technicianjoblistitemresponse) with `items`, `totalCount`, `pageNumber`, `pageSize`<br>Field-by-field: schema registry entry. | Success: standard envelope + listed `data` schema.<br>Error: standard error envelope in Section 16. | Authorize(Roles = RoleNames.Technician) Omit null/empty query values; client utilities in web/mobile already serialize only populated keys. | Technician execution flow uses these routes together for job detail, status, diagnosis, checklist, attachments, notes, timeline, and parts consumption. | Partner app should treat path `{id}` as the technician-visible job/service-request identifier used throughout the execution flow. |

## 15. API Naming Consistency Issues or Field Naming Conflict Notes

- Shared authentication is split across `/auth/*` and `/customer-auth/*`, but customer-web/mobile also use `/auth/login`; merge planning must decide whether customer login remains shared or becomes customer-specific.
- `/customer-auth/forgot-password` and `/customer-auth/reset-password` currently use the same request contract (`ForgotCustomerPasswordRequest`) and the same command path, so the API contract does not distinguish initiation vs final reset state.
- Idempotency is inconsistent: booking create and invoice generation use header `X-Idempotency-Key`, while payment collection carries `idempotencyKey` inside the JSON body.
- Booking read surfaces are split between `/bookings/my-bookings` and `/customer-bookings`; admin surfaces also read `/bookings/{bookingId}`. Consumer teams should not assume a single booking namespace.
- `GET /booking-lookups/tonnage` is singular while sibling routes are pluralized.
- Installation lifecycle routes are spread across `/installations`, `/installations/{installationId}`, and `/installations/orders/*`, so consumers must not assume one controller owns the full installation flow.

## 16. Reusable Common Response Format

All APIs return the standard JSON envelope below; payloads are camelCase on the wire.

| Field | Type | Description |
| --- | --- | --- |
| `isSuccess` | `boolean` | True for successful calls, false for failures. |
| `code` | `string` | Machine-readable success/error code. |
| `message` | `string` | Human-readable result message. |
| `data` | `T or null` | Endpoint-specific payload object, array, or paged result. |
| `errors` | `ApiError[]` | List of error items when validation/business failures occur. |
| `traceId` | `string` | Server trace identifier for diagnostics and log correlation. |
| `timestampUtc` | `datetime` | UTC timestamp when the response envelope was created. |

Standard success example structure:

```json
{
  "isSuccess": true,
  "code": "success",
  "message": "Request completed successfully.",
  "data": { "...": "endpoint-specific payload" },
  "errors": [],
  "traceId": "...",
  "timestampUtc": "2026-04-11T00:00:00Z"
}
```

Standard error example structure:

```json
{
  "isSuccess": false,
  "code": "validation_error",
  "message": "Request failed.",
  "data": null,
  "errors": [
    { "code": "field_error", "message": "Field-level detail." }
  ],
  "traceId": "...",
  "timestampUtc": "2026-04-11T00:00:00Z"
}
```

`ApiError` fields:

| Field | Type | Description |
| --- | --- | --- |
| `code` | `string` | Machine-readable error code. |
| `message` | `string` | Human-readable error description. |

## 17. Common Request / Response Conventions

- Base URL: `/api/`
- JSON naming: camelCase for all request/response body fields.
- Authentication: bearer token in `Authorization: Bearer <token>` for secured routes.
- Content type: `application/json` for request bodies.
- Pagination payload: `PagedResult<T>` with `items`, `totalCount`, `pageNumber`, `pageSize`.
- Date serialization: `DateOnly` fields are date strings; `DateTime` / `DateTimeOffset` fields are UTC datetime strings.
- Query serialization: client helpers omit null/undefined/empty-string query values.
- Path parameter naming follows controller parameter names and is already camelCase in routes.
- Some write routes support idempotency; where present, honor the exact header/body placement documented in the endpoint catalog.

## 18. Enum / Status Value Reference Section

- Booking master values are retrieved from `/booking-lookups/service-categories`, `/booking-lookups/services`, `/booking-lookups/ac-types`, `/booking-lookups/tonnage`, `/booking-lookups/brands`, and `/booking-lookups/zones/by-pincode/{pincode}`.
- Support ticket categories, priorities, and statuses are resolved from `/support-ticket-lookups/categories`, `/support-ticket-lookups/priorities`, and `/support-ticket-lookups/statuses`.
- Dynamic master / lookup-governed values are available from `/lookups/{lookupType}` and `/admin-masters`.
- Many operational filters are string-based with no compile-time enum contract in source: `status`, `paymentMethod`, `trendBy`, `channel`, `triggerCode`, `linkedEntityType`, `transactionType`, `sourceChannel`, `refundStatus`, `installationStatus`, `approvalStatus`, `revisitType`. Treat these as backend-governed values until explicitly standardized.
- RBAC values come from database-driven roles/permissions; customer/mobile/admin clients should not hardcode permissions beyond display gating already aligned with backend claims.

## 19. Field Reuse Matrix

| Reused Field | Appears Across | Integration Relevance |
| --- | --- | --- |
| `bookingId` | booking create/detail/list, cancellations, revisit, service-request creation from booking | Primary handoff from booking to service lifecycle. |
| `serviceRequestId` | service requests, assignment, customer absent, cancellation options, technician execution, operations detail | Core operational identifier after booking conversion. |
| `jobCardId` | technician execution, quotations, job consumption, revisit context | Bridges field work, quotation, and parts consumption. |
| `invoiceId` | billing status, invoice detail, payment collection/history, warranty claim | Finance and post-service lifecycle anchor. |
| `quotationId` | booking detail, quotation detail, invoice generation | Estimate-to-invoice linkage. |
| `customerId` | customer management, AMC assignment, support tickets, refunds, communication preferences | Customer-ownership and admin targeting key. |
| `technicianId` | assignment, availability, onboarding, documents, stock, training, skill assessments | Technician profile and operational routing key. |
| `status` | bookings, service requests, support tickets, quotations, invoices, refunds, installations, analytics filters | String-based status families require central mapping in clients. |
| `remarks` | assignment, status updates, payments, refunds, support actions, field execution, helper tasks | Common free-text audit/comment field; keep UX copy consistent. |
| `pageNumber` / `pageSize` | paged list endpoints across bookings, invoices, quotations, users, support, analytics-adjacent lists | Common pagination contract using `PagedResult<T>`. |

## 20. Final Integration Gap Notes

- Source defines exact route and schema contracts, but many business enums remain DB-driven or lookup-driven rather than strongly typed in code; merge consumers should source values from lookup APIs or confirm seed data before hardcoding.
- Ownership/security checks for customer-scoped routes are enforced in handlers/queries and are not fully visible from controller signatures alone; customer apps should assume server-side authorization on every detail/list route.
- No logout endpoint is present in the current controller surface; clients clear local session state only.
- `POST /payments/collect` mixes manual payment capture, gateway metadata, and webhook metadata in one contract; frontend teams should align on which fields are actually populated per payment channel before release.
- Installation lifecycle and Phase A/C/D style APIs are implemented in source, but broader project-brain notes still mark parts of those workflows as not fully source-verified; treat those flows as implemented but still requiring end-to-end validation.
- Customer password reset/finalization semantics are not separated in the public contract today because forgot/reset share the same request payload and command path.

## Appendix A. Schema Registry

### Schema: PagedResult<T>
<a id="schema-pagedresult-t"></a>

| Field | Wire Type | Required | Description |
| --- | --- | --- | --- |
| `items` | `T[]` | `Required` | Paged collection payload. |
| `totalCount` | `integer(int32)` | `Required` | Total records available for the current filter. |
| `pageNumber` | `integer(int32)` | `Required` | Current page number. |
| `pageSize` | `integer(int32)` | `Required` | Requested page size. |

### Schema: AcTypeLookupResponse
<a id="schema-actypelookupresponse"></a>

- Source file: `Backend/Coolzo.Contracts/Responses/Booking/AcTypeLookupResponse.cs`
- Shape: `record-positional`

| Field | Wire Type | Required | Description |
| --- | --- | --- | --- |
| `acTypeId` | `integer(int64)` | `Required` | Unique ac type identifier. |
| `acTypeName` | `string` | `Required` | Display name for ac type. |
| `description` | `string` | `Required` | Mapped description value. |

### Schema: ActivateTechnicianPhaseERequest
<a id="schema-activatetechnicianphaseerequest"></a>

- Source file: `Backend/Coolzo.Contracts/Requests/GapPhaseE/TechnicianOnboardingPhaseERequests.cs`
- Shape: `record-positional`

| Field | Wire Type | Required | Description |
| --- | --- | --- | --- |
| `activationReason` | `string` | `Required` | Mapped activation reason value. |

### Schema: ActivateTechnicianRequest
<a id="schema-activatetechnicianrequest"></a>

- Source file: `Backend/Coolzo.Contracts/Requests/GapPhaseA/TechnicianOnboardingRequests.cs`
- Shape: `record-positional`

| Field | Wire Type | Required | Description |
| --- | --- | --- | --- |
| `assessmentCode` | `string` | `Required` | Code value for assessment. |
| `scorePercentage` | `number(decimal)` | `Required` | Mapped score percentage value. |
| `trainingName` | `string` | `Required` | Display name for training. |
| `certificationNumber` | `string?` | `Optional` | Business number/reference for certification. |
| `trainingScorePercentage` | `number(decimal)` | `Required` | Mapped training score percentage value. |
| `remarks` | `string?` | `Optional` | Free-text remarks for the action. |

### Schema: AddLeadNoteRequest
<a id="schema-addleadnoterequest"></a>

- Source file: `Backend/Coolzo.Contracts/Requests/GapPhaseA/LeadManagementRequests.cs`
- Shape: `record-positional`

| Field | Wire Type | Required | Description |
| --- | --- | --- | --- |
| `noteText` | `string` | `Required` | Mapped note text value. |
| `isInternal` | `boolean` | `Required` | Boolean flag indicating whether internal is true. |

### Schema: AddSupportTicketReplyRequest
<a id="schema-addsupportticketreplyrequest"></a>

- Source file: `Backend/Coolzo.Contracts/Requests/Support/AddSupportTicketReplyRequest.cs`
- Shape: `record-positional`

| Field | Wire Type | Required | Description |
| --- | --- | --- | --- |
| `replyText` | `string` | `Required` | Mapped reply text value. |
| `isInternalOnly` | `boolean` | `Required` | Boolean flag indicating whether internal only is true. |

### Schema: AmcPlanResponse
<a id="schema-amcplanresponse"></a>

- Source file: `Backend/Coolzo.Contracts/Responses/Amc/AmcPlanResponse.cs`
- Shape: `record-positional`

| Field | Wire Type | Required | Description |
| --- | --- | --- | --- |
| `amcPlanId` | `integer(int64)` | `Required` | Unique amc plan identifier. |
| `planName` | `string` | `Required` | Display name for plan. |
| `planDescription` | `string` | `Required` | Mapped plan description value. |
| `durationInMonths` | `integer(int32)` | `Required` | Mapped duration in months value. |
| `visitCount` | `integer(int32)` | `Required` | Mapped visit count value. |
| `priceAmount` | `number(decimal)` | `Required` | Monetary amount for price. |
| `isActive` | `boolean` | `Required` | Boolean flag indicating whether active is true. |
| `termsAndConditions` | `string` | `Required` | Mapped terms and conditions value. |

### Schema: ApproveInstallationProposalRequest
<a id="schema-approveinstallationproposalrequest"></a>

- Source file: `Backend/Coolzo.Contracts/Requests/GapPhaseC/InstallationLifecycleRequests.cs`
- Shape: `record-positional`

| Field | Wire Type | Required | Description |
| --- | --- | --- | --- |
| `customerRemarks` | `string?` | `Optional` | Free-text remarks for customer. |

### Schema: ApprovePartsReturnRequest
<a id="schema-approvepartsreturnrequest"></a>

- Source file: `Backend/Coolzo.Contracts/Requests/GapPhaseA/PartsReturnRequests.cs`
- Shape: `record-positional`

| Field | Wire Type | Required | Description |
| --- | --- | --- | --- |
| `remarks` | `string?` | `Optional` | Free-text remarks for the action. |

### Schema: ApproveRefundRequestDecisionRequest
<a id="schema-approverefundrequestdecisionrequest"></a>

- Source file: `Backend/Coolzo.Contracts/Requests/GapPhaseD/CancellationRefundRequests.cs`
- Shape: `record-positional`

| Field | Wire Type | Required | Description |
| --- | --- | --- | --- |
| `approvedAmount` | `number(decimal)?` | `Optional` | Monetary amount for approved. |
| `remarks` | `string` | `Required` | Free-text remarks for the action. |

### Schema: AssignAmcToCustomerRequest
<a id="schema-assignamctocustomerrequest"></a>

- Source file: `Backend/Coolzo.Contracts/Requests/Amc/AssignAmcToCustomerRequest.cs`
- Shape: `record-positional`

| Field | Wire Type | Required | Description |
| --- | --- | --- | --- |
| `customerId` | `integer(int64)` | `Required` | Unique customer identifier. |
| `amcPlanId` | `integer(int64)` | `Required` | Unique amc plan identifier. |
| `jobCardId` | `integer(int64)` | `Required` | Unique job card identifier. |
| `invoiceId` | `integer(int64)` | `Required` | Unique invoice identifier. |
| `startDateUtc` | `datetime?` | `Optional` | UTC timestamp for start date. |
| `remarks` | `string?` | `Optional` | Free-text remarks for the action. |

### Schema: AssignHelperToJobRequest
<a id="schema-assignhelpertojobrequest"></a>

- Source file: `Backend/Coolzo.Contracts/Requests/GapPhaseE/HelperPhaseERequests.cs`
- Shape: `record-positional`

| Field | Wire Type | Required | Description |
| --- | --- | --- | --- |
| `technicianId` | `integer(int64)` | `Required` | Unique technician identifier. |
| `serviceRequestId` | `integer(int64)` | `Required` | Unique service request identifier. |
| `jobCardId` | `integer(int64)?` | `Optional` | Unique job card identifier. |
| `assignmentRemarks` | `string?` | `Optional` | Free-text remarks for assignment. |

### Schema: AssignLeadRequest
<a id="schema-assignleadrequest"></a>

- Source file: `Backend/Coolzo.Contracts/Requests/GapPhaseA/LeadManagementRequests.cs`
- Shape: `record-positional`

| Field | Wire Type | Required | Description |
| --- | --- | --- | --- |
| `assignedUserId` | `integer(int64)` | `Required` | Unique assigned user identifier. |
| `remarks` | `string?` | `Optional` | Free-text remarks for the action. |

### Schema: AssignStockToTechnicianRequest
<a id="schema-assignstocktotechnicianrequest"></a>

- Source file: `Backend/Coolzo.Contracts/Requests/Inventory/AssignStockToTechnicianRequest.cs`
- Shape: `record-positional`

| Field | Wire Type | Required | Description |
| --- | --- | --- | --- |
| `sourceWarehouseId` | `integer(int64)` | `Required` | Unique source warehouse identifier. |
| `itemId` | `integer(int64)` | `Required` | Unique item identifier. |
| `quantity` | `number(decimal)` | `Required` | Mapped quantity value. |
| `unitCost` | `number(decimal)` | `Required` | Mapped unit cost value. |
| `referenceNumber` | `string?` | `Optional` | Business number/reference for reference. |
| `remarks` | `string?` | `Optional` | Free-text remarks for the action. |

### Schema: AssignSupportTicketRequest
<a id="schema-assignsupportticketrequest"></a>

- Source file: `Backend/Coolzo.Contracts/Requests/Support/AssignSupportTicketRequest.cs`
- Shape: `record-positional`

| Field | Wire Type | Required | Description |
| --- | --- | --- | --- |
| `assignedUserId` | `integer(int64)` | `Required` | Unique assigned user identifier. |
| `remarks` | `string` | `Required` | Free-text remarks for the action. |

### Schema: AssignTechnicianRequest
<a id="schema-assigntechnicianrequest"></a>

- Source file: `Backend/Coolzo.Contracts/Requests/Operations/AssignTechnicianRequest.cs`
- Shape: `record-positional`

| Field | Wire Type | Required | Description |
| --- | --- | --- | --- |
| `technicianId` | `integer(int64)?` | `Optional` | Unique technician identifier. |
| `remarks` | `string?` | `Optional` | Free-text remarks for the action. |

### Schema: AssignmentHistoryItemResponse
<a id="schema-assignmenthistoryitemresponse"></a>

- Source file: `Backend/Coolzo.Contracts/Responses/Operations/AssignmentHistoryItemResponse.cs`
- Shape: `record-positional`

| Field | Wire Type | Required | Description |
| --- | --- | --- | --- |
| `actionName` | `string` | `Required` | Display name for action. |
| `previousTechnicianName` | `string?` | `Optional` | Display name for previous technician. |
| `currentTechnicianName` | `string` | `Required` | Display name for current technician. |
| `remarks` | `string` | `Required` | Free-text remarks for the action. |
| `actionDateUtc` | `datetime` | `Required` | UTC timestamp for action date. |

### Schema: AuthTokenResponse
<a id="schema-authtokenresponse"></a>

- Source file: `Backend/Coolzo.Contracts/Responses/Auth/AuthTokenResponse.cs`
- Shape: `record-positional`

| Field | Wire Type | Required | Description |
| --- | --- | --- | --- |
| `accessToken` | `string` | `Required` | Mapped access token value. |
| `refreshToken` | `string` | `Required` | Mapped refresh token value. |
| `expiresAtUtc` | `datetime` | `Required` | UTC timestamp for expires at. |
| `currentUser` | `CurrentUserResponse` | `Required` | Mapped current user value. |

### Schema: BillingStatusResponse
<a id="schema-billingstatusresponse"></a>

- Source file: `Backend/Coolzo.Contracts/Responses/Billing/BillingStatusResponse.cs`
- Shape: `record-positional`

| Field | Wire Type | Required | Description |
| --- | --- | --- | --- |
| `invoiceId` | `integer(int64)` | `Required` | Unique invoice identifier. |
| `invoiceNumber` | `string` | `Required` | Business number/reference for invoice. |
| `invoiceStatus` | `string` | `Required` | Current status value for invoice. |
| `grandTotalAmount` | `number(decimal)` | `Required` | Monetary amount for grand total. |
| `paidAmount` | `number(decimal)` | `Required` | Monetary amount for paid. |
| `balanceAmount` | `number(decimal)` | `Required` | Monetary amount for balance. |
| `timeline` | `BillingStatusHistoryResponse[]` | `Required` | Collection of `BillingStatusHistoryResponse` items. |

### Schema: BookingAnalyticsResponse
<a id="schema-bookinganalyticsresponse"></a>

- Source file: `Backend/Coolzo.Contracts/Responses/Analytics/AnalyticsResponses.cs`
- Shape: `record-positional`

| Field | Wire Type | Required | Description |
| --- | --- | --- | --- |
| `totalBookings` | `integer(int64)` | `Required` | Mapped total bookings value. |
| `pendingBookings` | `integer(int64)` | `Required` | Mapped pending bookings value. |
| `confirmedBookings` | `integer(int64)` | `Required` | Mapped confirmed bookings value. |
| `cancelledBookings` | `integer(int64)` | `Required` | Boolean permission flag indicating whether celled bookings is allowed. |
| `averageBookingsPerPeriod` | `number(decimal)` | `Required` | Mapped average bookings per period value. |
| `bookingTrends` | `AnalyticsTrendPointResponse[]` | `Required` | Collection of `AnalyticsTrendPointResponse` items. |
| `statusDistribution` | `AnalyticsBreakdownItemResponse[]` | `Required` | Collection of `AnalyticsBreakdownItemResponse` items. |
| `serviceDistribution` | `AnalyticsBreakdownItemResponse[]` | `Required` | Collection of `AnalyticsBreakdownItemResponse` items. |

### Schema: BookingDetailResponse
<a id="schema-bookingdetailresponse"></a>

- Source file: `Backend/Coolzo.Contracts/Responses/Booking/BookingDetailResponse.cs`
- Shape: `record-positional`

| Field | Wire Type | Required | Description |
| --- | --- | --- | --- |
| `bookingId` | `integer(int64)` | `Required` | Unique booking identifier. |
| `bookingReference` | `string` | `Required` | Reference value for booking. |
| `status` | `string` | `Required` | Current status value for the record. |
| `sourceChannel` | `string` | `Required` | Mapped source channel value. |
| `isGuestBooking` | `boolean` | `Required` | Boolean flag indicating whether guest booking is true. |
| `bookingDateUtc` | `datetime` | `Required` | UTC timestamp for booking date. |
| `serviceName` | `string` | `Required` | Display name for service. |
| `customerName` | `string` | `Required` | Display name for customer. |
| `mobileNumber` | `string` | `Required` | Business number/reference for mobile. |
| `emailAddress` | `string` | `Required` | Mapped email address value. |
| `addressSummary` | `string` | `Required` | Summary text for address. |
| `zoneName` | `string` | `Required` | Display name for zone. |
| `slotDate` | `date` | `Required` | Date value for slot. |
| `slotLabel` | `string` | `Required` | Mapped slot label value. |
| `estimatedPrice` | `number(decimal)` | `Required` | Mapped estimated price value. |
| `serviceRequestId` | `integer(int64)?` | `Optional` | Unique service request identifier. |
| `serviceRequestNumber` | `string?` | `Optional` | Business number/reference for service request. |
| `operationalStatus` | `string?` | `Optional` | Current status value for operational. |
| `assignedTechnicianId` | `integer(int64)?` | `Optional` | Unique assigned technician identifier. |
| `assignedTechnicianName` | `string?` | `Optional` | Display name for assigned technician. |
| `jobCardId` | `integer(int64)?` | `Optional` | Unique job card identifier. |
| `jobCardNumber` | `string?` | `Optional` | Business number/reference for job card. |
| `quotationId` | `integer(int64)?` | `Optional` | Unique quotation identifier. |
| `quotationNumber` | `string?` | `Optional` | Business number/reference for quotation. |
| `quotationStatus` | `string?` | `Optional` | Current status value for quotation. |
| `invoiceId` | `integer(int64)?` | `Optional` | Unique invoice identifier. |
| `invoiceNumber` | `string?` | `Optional` | Business number/reference for invoice. |
| `invoiceStatus` | `string?` | `Optional` | Current status value for invoice. |
| `invoiceGrandTotalAmount` | `number(decimal)?` | `Optional` | Monetary amount for invoice grand total. |
| `invoiceBalanceAmount` | `number(decimal)?` | `Optional` | Monetary amount for invoice balance. |
| `completionSummary` | `string?` | `Optional` | Summary text for completion. |
| `fieldTimeline` | `JobExecutionTimelineItemResponse[]` | `Required` | Collection of `JobExecutionTimelineItemResponse` items. |
| `customerVisibleNotes` | `JobExecutionNoteResponse[]` | `Required` | Collection of `JobExecutionNoteResponse` items. |
| `lines` | `BookingLineResponse[]` | `Required` | Collection of `BookingLineResponse` items. |
| `statusHistory` | `BookingStatusHistoryResponse[]` | `Required` | Collection of `BookingStatusHistoryResponse` items. |

### Schema: BookingListItemResponse
<a id="schema-bookinglistitemresponse"></a>

- Source file: `Backend/Coolzo.Contracts/Responses/Booking/BookingListItemResponse.cs`
- Shape: `record-positional`

| Field | Wire Type | Required | Description |
| --- | --- | --- | --- |
| `bookingId` | `integer(int64)` | `Required` | Unique booking identifier. |
| `bookingReference` | `string` | `Required` | Reference value for booking. |
| `status` | `string` | `Required` | Current status value for the record. |
| `serviceName` | `string` | `Required` | Display name for service. |
| `customerName` | `string` | `Required` | Display name for customer. |
| `mobileNumber` | `string` | `Required` | Business number/reference for mobile. |
| `slotDate` | `date` | `Required` | Date value for slot. |
| `slotLabel` | `string` | `Required` | Mapped slot label value. |
| `sourceChannel` | `string` | `Required` | Mapped source channel value. |
| `bookingDateUtc` | `datetime` | `Required` | UTC timestamp for booking date. |
| `operationalStatus` | `string?` | `Optional` | Current status value for operational. |
| `assignedTechnicianName` | `string?` | `Optional` | Display name for assigned technician. |

### Schema: BookingSummaryResponse
<a id="schema-bookingsummaryresponse"></a>

- Source file: `Backend/Coolzo.Contracts/Responses/Booking/BookingSummaryResponse.cs`
- Shape: `record-positional`

| Field | Wire Type | Required | Description |
| --- | --- | --- | --- |
| `bookingId` | `integer(int64)` | `Required` | Unique booking identifier. |
| `bookingReference` | `string` | `Required` | Reference value for booking. |
| `status` | `string` | `Required` | Current status value for the record. |
| `serviceName` | `string` | `Required` | Display name for service. |
| `customerName` | `string` | `Required` | Display name for customer. |
| `mobileNumber` | `string` | `Required` | Business number/reference for mobile. |
| `slotDate` | `date` | `Required` | Date value for slot. |
| `slotLabel` | `string` | `Required` | Mapped slot label value. |
| `addressSummary` | `string` | `Required` | Summary text for address. |
| `estimatedPrice` | `number(decimal)` | `Required` | Mapped estimated price value. |

### Schema: BrandLookupResponse
<a id="schema-brandlookupresponse"></a>

- Source file: `Backend/Coolzo.Contracts/Responses/Booking/BrandLookupResponse.cs`
- Shape: `record-positional`

| Field | Wire Type | Required | Description |
| --- | --- | --- | --- |
| `brandId` | `integer(int64)` | `Required` | Unique brand identifier. |
| `brandName` | `string` | `Required` | Display name for brand. |
| `description` | `string` | `Required` | Mapped description value. |

### Schema: BusinessHourConfigurationResponse
<a id="schema-businesshourconfigurationresponse"></a>

- Source file: `Backend/Coolzo.Contracts/Responses/Admin/AdminResponses.cs`
- Shape: `record-positional`

| Field | Wire Type | Required | Description |
| --- | --- | --- | --- |
| `businessHourConfigurationId` | `integer(int64)` | `Required` | Unique business hour configuration identifier. |
| `dayOfWeekNumber` | `integer(int32)` | `Required` | Business number/reference for day of week. |
| `dayName` | `string` | `Required` | Display name for day. |
| `startTimeLocal` | `string?` | `Optional` | Mapped start time local value. |
| `endTimeLocal` | `string?` | `Optional` | Mapped end time local value. |
| `isClosed` | `boolean` | `Required` | Boolean flag indicating whether closed is true. |

### Schema: CMSBannerResponse
<a id="schema-cmsbannerresponse"></a>

- Source file: `Backend/Coolzo.Contracts/Responses/Admin/AdminResponses.cs`
- Shape: `record-positional`

| Field | Wire Type | Required | Description |
| --- | --- | --- | --- |
| `cMSBannerId` | `integer(int64)` | `Required` | Unique cmsbanner identifier. |
| `bannerTitle` | `string` | `Required` | Mapped banner title value. |
| `bannerSubtitle` | `string` | `Required` | Mapped banner subtitle value. |
| `imageUrl` | `string` | `Required` | Mapped image url value. |
| `redirectUrl` | `string` | `Required` | Mapped redirect url value. |
| `displayArea` | `string` | `Required` | Mapped display area value. |
| `activeFromDate` | `string?` | `Optional` | Date value for active from. |
| `activeToDate` | `string?` | `Optional` | Date value for active to. |
| `isActive` | `boolean` | `Required` | Boolean flag indicating whether active is true. |
| `isPublished` | `boolean` | `Required` | Boolean flag indicating whether published is true. |
| `sortOrder` | `integer(int32)` | `Required` | Mapped sort order value. |

### Schema: CMSBannerUpsertRequest
<a id="schema-cmsbannerupsertrequest"></a>

- Source file: `Backend/Coolzo.Contracts/Requests/Admin/AdminRequests.cs`
- Shape: `record-positional`

| Field | Wire Type | Required | Description |
| --- | --- | --- | --- |
| `bannerTitle` | `string` | `Required` | Mapped banner title value. |
| `bannerSubtitle` | `string?` | `Optional` | Mapped banner subtitle value. |
| `imageUrl` | `string?` | `Optional` | Mapped image url value. |
| `redirectUrl` | `string?` | `Optional` | Mapped redirect url value. |
| `displayArea` | `string?` | `Optional` | Mapped display area value. |
| `activeFromDate` | `date?` | `Optional` | Date value for active from. |
| `activeToDate` | `date?` | `Optional` | Date value for active to. |
| `isActive` | `boolean` | `Required` | Boolean flag indicating whether active is true. |
| `isPublished` | `boolean` | `Required` | Boolean flag indicating whether published is true. |
| `sortOrder` | `integer(int32)` | `Required` | Mapped sort order value. |

### Schema: CMSBlockResponse
<a id="schema-cmsblockresponse"></a>

- Source file: `Backend/Coolzo.Contracts/Responses/Admin/AdminResponses.cs`
- Shape: `record-positional`

| Field | Wire Type | Required | Description |
| --- | --- | --- | --- |
| `cMSBlockId` | `integer(int64)` | `Required` | Unique cmsblock identifier. |
| `blockKey` | `string` | `Required` | Mapped block key value. |
| `title` | `string` | `Required` | Mapped title value. |
| `summary` | `string` | `Required` | Summary text for the record. |
| `content` | `string` | `Required` | Mapped content value. |
| `previewImageUrl` | `string` | `Required` | Mapped preview image url value. |
| `isActive` | `boolean` | `Required` | Boolean flag indicating whether active is true. |
| `isPublished` | `boolean` | `Required` | Boolean flag indicating whether published is true. |
| `sortOrder` | `integer(int32)` | `Required` | Mapped sort order value. |
| `versionNumber` | `integer(int32)` | `Required` | Business number/reference for version. |
| `dateCreated` | `datetime` | `Required` | Mapped date created value. |
| `lastUpdated` | `datetime?` | `Optional` | Mapped last updated value. |

### Schema: CMSBlockUpsertRequest
<a id="schema-cmsblockupsertrequest"></a>

- Source file: `Backend/Coolzo.Contracts/Requests/Admin/AdminRequests.cs`
- Shape: `record-positional`

| Field | Wire Type | Required | Description |
| --- | --- | --- | --- |
| `blockKey` | `string` | `Required` | Mapped block key value. |
| `title` | `string` | `Required` | Mapped title value. |
| `summary` | `string?` | `Optional` | Summary text for the record. |
| `content` | `string` | `Required` | Mapped content value. |
| `previewImageUrl` | `string?` | `Optional` | Mapped preview image url value. |
| `isActive` | `boolean` | `Required` | Boolean flag indicating whether active is true. |
| `isPublished` | `boolean` | `Required` | Boolean flag indicating whether published is true. |
| `sortOrder` | `integer(int32)` | `Required` | Mapped sort order value. |

### Schema: CMSFaqResponse
<a id="schema-cmsfaqresponse"></a>

- Source file: `Backend/Coolzo.Contracts/Responses/Admin/AdminResponses.cs`
- Shape: `record-positional`

| Field | Wire Type | Required | Description |
| --- | --- | --- | --- |
| `cMSFaqId` | `integer(int64)` | `Required` | Unique cmsfaq identifier. |
| `category` | `string` | `Required` | Mapped category value. |
| `question` | `string` | `Required` | Mapped question value. |
| `answer` | `string` | `Required` | Mapped answer value. |
| `isActive` | `boolean` | `Required` | Boolean flag indicating whether active is true. |
| `isPublished` | `boolean` | `Required` | Boolean flag indicating whether published is true. |
| `sortOrder` | `integer(int32)` | `Required` | Mapped sort order value. |

### Schema: CMSFaqUpsertRequest
<a id="schema-cmsfaqupsertrequest"></a>

- Source file: `Backend/Coolzo.Contracts/Requests/Admin/AdminRequests.cs`
- Shape: `record-positional`

| Field | Wire Type | Required | Description |
| --- | --- | --- | --- |
| `category` | `string` | `Required` | Mapped category value. |
| `question` | `string` | `Required` | Mapped question value. |
| `answer` | `string` | `Required` | Mapped answer value. |
| `isActive` | `boolean` | `Required` | Boolean flag indicating whether active is true. |
| `isPublished` | `boolean` | `Required` | Boolean flag indicating whether published is true. |
| `sortOrder` | `integer(int32)` | `Required` | Mapped sort order value. |

### Schema: CampaignResponse
<a id="schema-campaignresponse"></a>

- Source file: `Backend/Coolzo.Contracts/Responses/GapPhaseA/GapPhaseAResponses.cs`
- Shape: `record-positional`

| Field | Wire Type | Required | Description |
| --- | --- | --- | --- |
| `campaignId` | `integer(int64)` | `Required` | Unique campaign identifier. |
| `campaignCode` | `string` | `Required` | Code value for campaign. |
| `campaignName` | `string` | `Required` | Display name for campaign. |
| `campaignStatus` | `string` | `Required` | Current status value for campaign. |
| `plannedBookingCount` | `integer(int32)` | `Required` | Mapped planned booking count value. |
| `allocatedBookingCount` | `integer(int32)` | `Required` | Mapped allocated booking count value. |
| `slotAvailabilityId` | `integer(int64)` | `Required` | Unique slot availability identifier. |

### Schema: CancelCustomerAbsentRequest
<a id="schema-cancelcustomerabsentrequest"></a>

- Source file: `Backend/Coolzo.Contracts/Requests/GapPhaseD/CancellationRefundRequests.cs`
- Shape: `record-positional`

| Field | Wire Type | Required | Description |
| --- | --- | --- | --- |
| `cancellationReasonCode` | `string` | `Required` | Code value for cancellation reason. |
| `cancellationReasonText` | `string` | `Required` | Boolean permission flag indicating whether cellation reason text is allowed. |
| `remarks` | `string` | `Required` | Free-text remarks for the action. |

### Schema: CancelServiceRequestRequest
<a id="schema-cancelservicerequestrequest"></a>

- Source file: `Backend/Coolzo.Contracts/Requests/GapPhaseA/CancellationRefundRequests.cs`
- Shape: `record-positional`

| Field | Wire Type | Required | Description |
| --- | --- | --- | --- |
| `reasonCode` | `string` | `Required` | Code value for reason. |
| `reasonDescription` | `string` | `Required` | Mapped reason description value. |
| `requiresApproval` | `boolean` | `Required` | Mapped requires approval value. |

### Schema: CancellationDetailResponse
<a id="schema-cancellationdetailresponse"></a>

- Source file: `Backend/Coolzo.Contracts/Responses/GapPhaseD/CancellationRefundResponses.cs`
- Shape: `record-positional`

| Field | Wire Type | Required | Description |
| --- | --- | --- | --- |
| `cancellationRecordId` | `integer(int64)` | `Required` | Unique cancellation record identifier. |
| `bookingId` | `integer(int64)?` | `Optional` | Unique booking identifier. |
| `serviceRequestId` | `integer(int64)?` | `Optional` | Unique service request identifier. |
| `cancelledByUserId` | `integer(int64)?` | `Optional` | Unique cancelled by user identifier. |
| `cancelledByRole` | `string` | `Required` | Boolean permission flag indicating whether celled by role is allowed. |
| `cancellationSource` | `string` | `Required` | Boolean permission flag indicating whether cellation source is allowed. |
| `cancellationReasonCode` | `string` | `Required` | Code value for cancellation reason. |
| `cancellationReasonText` | `string` | `Required` | Boolean permission flag indicating whether cellation reason text is allowed. |
| `timeToSlotMinutes` | `integer(int32)` | `Required` | Mapped time to slot minutes value. |
| `cancellationFee` | `number(decimal)` | `Required` | Boolean permission flag indicating whether cellation fee is allowed. |
| `refundEligibleAmount` | `number(decimal)` | `Required` | Monetary amount for refund eligible. |
| `cancellationStatus` | `string` | `Required` | Current status value for cancellation. |
| `policyCode` | `string` | `Required` | Code value for policy. |
| `policyDescription` | `string` | `Required` | Mapped policy description value. |
| `approvalRequired` | `boolean` | `Required` | Mapped approval required value. |
| `dateCreated` | `datetime` | `Required` | Mapped date created value. |
| `refundRequestId` | `integer(int64)?` | `Optional` | Unique refund request identifier. |
| `refundStatus` | `string?` | `Optional` | Current status value for refund. |

### Schema: CancellationListItemResponse
<a id="schema-cancellationlistitemresponse"></a>

- Source file: `Backend/Coolzo.Contracts/Responses/GapPhaseD/CancellationRefundResponses.cs`
- Shape: `record-positional`

| Field | Wire Type | Required | Description |
| --- | --- | --- | --- |
| `cancellationRecordId` | `integer(int64)` | `Required` | Unique cancellation record identifier. |
| `bookingId` | `integer(int64)?` | `Optional` | Unique booking identifier. |
| `serviceRequestId` | `integer(int64)?` | `Optional` | Unique service request identifier. |
| `referenceNumber` | `string` | `Required` | Business number/reference for reference. |
| `cancellationStatus` | `string` | `Required` | Current status value for cancellation. |
| `cancellationSource` | `string` | `Required` | Boolean permission flag indicating whether cellation source is allowed. |
| `cancellationReasonCode` | `string` | `Required` | Code value for cancellation reason. |
| `cancellationFee` | `number(decimal)` | `Required` | Boolean permission flag indicating whether cellation fee is allowed. |
| `refundEligibleAmount` | `number(decimal)` | `Required` | Monetary amount for refund eligible. |
| `cancelledByRole` | `string` | `Required` | Boolean permission flag indicating whether celled by role is allowed. |
| `dateCreated` | `datetime` | `Required` | Mapped date created value. |
| `refundRequestId` | `integer(int64)?` | `Optional` | Unique refund request identifier. |
| `refundStatus` | `string?` | `Optional` | Current status value for refund. |

### Schema: CancellationOptionsResponse
<a id="schema-cancellationoptionsresponse"></a>

- Source file: `Backend/Coolzo.Contracts/Responses/GapPhaseD/CancellationRefundResponses.cs`
- Shape: `record-positional`

| Field | Wire Type | Required | Description |
| --- | --- | --- | --- |
| `bookingId` | `integer(int64)?` | `Optional` | Unique booking identifier. |
| `serviceRequestId` | `integer(int64)?` | `Optional` | Unique service request identifier. |
| `policyCode` | `string` | `Required` | Code value for policy. |
| `policyName` | `string` | `Required` | Display name for policy. |
| `policyDescription` | `string` | `Required` | Mapped policy description value. |
| `timeToSlotMinutes` | `integer(int32)` | `Required` | Mapped time to slot minutes value. |
| `paidAmount` | `number(decimal)` | `Required` | Monetary amount for paid. |
| `cancellationFee` | `number(decimal)` | `Required` | Boolean permission flag indicating whether cellation fee is allowed. |
| `refundEligibleAmount` | `number(decimal)` | `Required` | Monetary amount for refund eligible. |
| `approvalRequired` | `boolean` | `Required` | Mapped approval required value. |
| `canCustomerCancel` | `boolean` | `Required` | Boolean permission flag indicating whether customer cancel is allowed. |
| `customerDenialReason` | `string` | `Required` | Mapped customer denial reason value. |
| `scheduledStartUtc` | `datetime` | `Required` | Mapped scheduled start utc value. |
| `isTechnicianDispatched` | `boolean` | `Required` | Boolean flag indicating whether technician dispatched is true. |

### Schema: CancellationRecordResponse
<a id="schema-cancellationrecordresponse"></a>

- Source file: `Backend/Coolzo.Contracts/Responses/GapPhaseA/GapPhaseAResponses.cs`
- Shape: `record-positional`

| Field | Wire Type | Required | Description |
| --- | --- | --- | --- |
| `cancellationRecordId` | `integer(int64)` | `Required` | Unique cancellation record identifier. |
| `serviceRequestId` | `integer(int64)` | `Required` | Unique service request identifier. |
| `cancellationStatus` | `string` | `Required` | Current status value for cancellation. |
| `cancellationFeeAmount` | `number(decimal)` | `Required` | Boolean permission flag indicating whether cellation fee amount is allowed. |
| `refundEligibleAmount` | `number(decimal)` | `Required` | Monetary amount for refund eligible. |
| `requiresApproval` | `boolean` | `Required` | Mapped requires approval value. |

### Schema: ChangeCustomerPasswordRequest
<a id="schema-changecustomerpasswordrequest"></a>

- Source file: `Backend/Coolzo.Contracts/Requests/CustomerAuth/ChangeCustomerPasswordRequest.cs`
- Shape: `record-positional`

| Field | Wire Type | Required | Description |
| --- | --- | --- | --- |
| `currentPassword` | `string` | `Required` | Mapped current password value. |
| `newPassword` | `string` | `Required` | Mapped new password value. |

### Schema: ChangeSupportTicketPriorityRequest
<a id="schema-changesupportticketpriorityrequest"></a>

- Source file: `Backend/Coolzo.Contracts/Requests/Support/ChangeSupportTicketPriorityRequest.cs`
- Shape: `record-positional`

| Field | Wire Type | Required | Description |
| --- | --- | --- | --- |
| `priorityId` | `integer(int64)` | `Required` | Unique priority identifier. |
| `remarks` | `string` | `Required` | Free-text remarks for the action. |

### Schema: ChangeSupportTicketStatusRequest
<a id="schema-changesupportticketstatusrequest"></a>

- Source file: `Backend/Coolzo.Contracts/Requests/Support/ChangeSupportTicketStatusRequest.cs`
- Shape: `record-positional`

| Field | Wire Type | Required | Description |
| --- | --- | --- | --- |
| `status` | `string` | `Required` | Current status value for the record. |
| `remarks` | `string` | `Required` | Free-text remarks for the action. |

### Schema: CheckInHelperAttendanceRequest
<a id="schema-checkinhelperattendancerequest"></a>

- Source file: `Backend/Coolzo.Contracts/Requests/GapPhaseE/HelperPhaseERequests.cs`
- Shape: `record-positional`

| Field | Wire Type | Required | Description |
| --- | --- | --- | --- |
| `locationText` | `string?` | `Optional` | Mapped location text value. |

### Schema: CheckOutHelperAttendanceRequest
<a id="schema-checkouthelperattendancerequest"></a>

- Source file: `Backend/Coolzo.Contracts/Requests/GapPhaseE/HelperPhaseERequests.cs`
- Shape: `record-positional`

| Field | Wire Type | Required | Description |
| --- | --- | --- | --- |
| `locationText` | `string?` | `Optional` | Mapped location text value. |

### Schema: CommissioningCertificateResponse
<a id="schema-commissioningcertificateresponse"></a>

- Source file: `Backend/Coolzo.Contracts/Responses/GapPhaseA/GapPhaseAResponses.cs`
- Shape: `record-positional`

| Field | Wire Type | Required | Description |
| --- | --- | --- | --- |
| `commissioningCertificateId` | `integer(int64)` | `Required` | Unique commissioning certificate identifier. |
| `installationOrderId` | `integer(int64)` | `Required` | Unique installation order identifier. |
| `certificateNumber` | `string` | `Required` | Business number/reference for certificate. |
| `commissioningDateUtc` | `datetime` | `Required` | UTC timestamp for commissioning date. |
| `isAccepted` | `boolean` | `Required` | Boolean flag indicating whether accepted is true. |

### Schema: CommunicationPreferenceResponse
<a id="schema-communicationpreferenceresponse"></a>

- Source file: `Backend/Coolzo.Contracts/Responses/Admin/AdminResponses.cs`
- Shape: `record-positional`

| Field | Wire Type | Required | Description |
| --- | --- | --- | --- |
| `communicationPreferenceId` | `integer(int64)` | `Required` | Unique communication preference identifier. |
| `customerId` | `integer(int64)` | `Required` | Unique customer identifier. |
| `emailAddress` | `string` | `Required` | Mapped email address value. |
| `mobileNumber` | `string` | `Required` | Business number/reference for mobile. |
| `emailEnabled` | `boolean` | `Required` | Mapped email enabled value. |
| `smsEnabled` | `boolean` | `Required` | Mapped sms enabled value. |
| `whatsAppEnabled` | `boolean` | `Required` | Mapped whats app enabled value. |
| `pushEnabled` | `boolean` | `Required` | Mapped push enabled value. |
| `allowPromotionalContent` | `boolean` | `Required` | Mapped allow promotional content value. |
| `lastUpdated` | `datetime?` | `Optional` | Mapped last updated value. |

### Schema: CommunicationPreferenceUpdateRequest
<a id="schema-communicationpreferenceupdaterequest"></a>

- Source file: `Backend/Coolzo.Contracts/Requests/Admin/AdminRequests.cs`
- Shape: `record-positional`

| Field | Wire Type | Required | Description |
| --- | --- | --- | --- |
| `emailEnabled` | `boolean` | `Required` | Mapped email enabled value. |
| `smsEnabled` | `boolean` | `Required` | Mapped sms enabled value. |
| `whatsAppEnabled` | `boolean` | `Required` | Mapped whats app enabled value. |
| `pushEnabled` | `boolean` | `Required` | Mapped push enabled value. |
| `allowPromotionalContent` | `boolean` | `Required` | Mapped allow promotional content value. |
| `emailAddress` | `string?` | `Optional` | Mapped email address value. |
| `mobileNumber` | `string?` | `Optional` | Business number/reference for mobile. |

### Schema: CompleteInstallationRequest
<a id="schema-completeinstallationrequest"></a>

- Source file: `Backend/Coolzo.Contracts/Requests/GapPhaseC/InstallationLifecycleRequests.cs`
- Shape: `record-positional`

| Field | Wire Type | Required | Description |
| --- | --- | --- | --- |
| `workSummary` | `string` | `Required` | Summary text for work. |

### Schema: CompleteTrainingRecordRequest
<a id="schema-completetrainingrecordrequest"></a>

- Source file: `Backend/Coolzo.Contracts/Requests/GapPhaseE/TechnicianOnboardingPhaseERequests.cs`
- Shape: `record-positional`

| Field | Wire Type | Required | Description |
| --- | --- | --- | --- |
| `certificationNumber` | `string?` | `Optional` | Business number/reference for certification. |
| `scorePercentage` | `number(decimal)?` | `Optional` | Mapped score percentage value. |
| `certificateUrl` | `string?` | `Optional` | Mapped certificate url value. |
| `remarks` | `string?` | `Optional` | Free-text remarks for the action. |

### Schema: ConsumeJobPartsRequest
<a id="schema-consumejobpartsrequest"></a>

- Source file: `Backend/Coolzo.Contracts/Requests/Inventory/ConsumeJobPartsRequest.cs`
- Shape: `record-positional`

| Field | Wire Type | Required | Description |
| --- | --- | --- | --- |
| `items` | `ConsumeJobPartItemRequest[]` | `Required` | Collection of `ConsumeJobPartItemRequest` items. |

### Schema: ConvertLeadToBookingRequest
<a id="schema-convertleadtobookingrequest"></a>

- Source file: `Backend/Coolzo.Contracts/Requests/GapPhaseA/LeadManagementRequests.cs`
- Shape: `record-positional`

| Field | Wire Type | Required | Description |
| --- | --- | --- | --- |
| `serviceId` | `integer(int64)?` | `Optional` | Unique service identifier. |
| `acTypeId` | `integer(int64)?` | `Optional` | Unique ac type identifier. |
| `tonnageId` | `integer(int64)?` | `Optional` | Unique tonnage identifier. |
| `brandId` | `integer(int64)?` | `Optional` | Unique brand identifier. |
| `slotAvailabilityId` | `integer(int64)?` | `Optional` | Unique slot availability identifier. |
| `addressLine1` | `string?` | `Optional` | Mapped address line1 value. |
| `addressLine2` | `string?` | `Optional` | Mapped address line2 value. |
| `cityName` | `string?` | `Optional` | Display name for city. |
| `pincode` | `string?` | `Optional` | Code value for pin. |
| `inquiryNotes` | `string?` | `Optional` | Mapped inquiry notes value. |

### Schema: ConvertLeadToServiceRequestRequest
<a id="schema-convertleadtoservicerequestrequest"></a>

- Source file: `Backend/Coolzo.Contracts/Requests/GapPhaseA/LeadManagementRequests.cs`
- Shape: `record-positional`

| Field | Wire Type | Required | Description |
| --- | --- | --- | --- |
| `serviceId` | `integer(int64)?` | `Optional` | Unique service identifier. |
| `acTypeId` | `integer(int64)?` | `Optional` | Unique ac type identifier. |
| `tonnageId` | `integer(int64)?` | `Optional` | Unique tonnage identifier. |
| `brandId` | `integer(int64)?` | `Optional` | Unique brand identifier. |
| `slotAvailabilityId` | `integer(int64)?` | `Optional` | Unique slot availability identifier. |
| `addressLine1` | `string?` | `Optional` | Mapped address line1 value. |
| `addressLine2` | `string?` | `Optional` | Mapped address line2 value. |
| `cityName` | `string?` | `Optional` | Display name for city. |
| `pincode` | `string?` | `Optional` | Code value for pin. |
| `inquiryNotes` | `string?` | `Optional` | Mapped inquiry notes value. |

### Schema: CreateAdminCancellationRequest
<a id="schema-createadmincancellationrequest"></a>

- Source file: `Backend/Coolzo.Contracts/Requests/GapPhaseD/CancellationRefundRequests.cs`
- Shape: `record-positional`

| Field | Wire Type | Required | Description |
| --- | --- | --- | --- |
| `bookingId` | `integer(int64)?` | `Optional` | Unique booking identifier. |
| `serviceRequestId` | `integer(int64)?` | `Optional` | Unique service request identifier. |
| `cancellationSource` | `string` | `Required` | Boolean permission flag indicating whether cellation source is allowed. |
| `cancellationReasonCode` | `string` | `Required` | Code value for cancellation reason. |
| `cancellationReasonText` | `string` | `Required` | Boolean permission flag indicating whether cellation reason text is allowed. |
| `forceOverride` | `boolean` | `Required` | Mapped force override value. |
| `overrideReason` | `string?` | `Optional` | Mapped override reason value. |

### Schema: CreateAmcPlanRequest
<a id="schema-createamcplanrequest"></a>

- Source file: `Backend/Coolzo.Contracts/Requests/Amc/CreateAmcPlanRequest.cs`
- Shape: `record-positional`

| Field | Wire Type | Required | Description |
| --- | --- | --- | --- |
| `planName` | `string` | `Required` | Display name for plan. |
| `planDescription` | `string?` | `Optional` | Mapped plan description value. |
| `durationInMonths` | `integer(int32)` | `Required` | Mapped duration in months value. |
| `visitCount` | `integer(int32)` | `Required` | Mapped visit count value. |
| `priceAmount` | `number(decimal)` | `Required` | Monetary amount for price. |
| `isActive` | `boolean` | `Required` | Boolean flag indicating whether active is true. |
| `termsAndConditions` | `string?` | `Optional` | Mapped terms and conditions value. |

### Schema: CreateCampaignRequest
<a id="schema-createcampaignrequest"></a>

- Source file: `Backend/Coolzo.Contracts/Requests/GapPhaseA/CampaignRequests.cs`
- Shape: `record-positional`

| Field | Wire Type | Required | Description |
| --- | --- | --- | --- |
| `campaignName` | `string` | `Required` | Display name for campaign. |
| `serviceId` | `integer(int64)` | `Required` | Unique service identifier. |
| `zoneId` | `integer(int64)` | `Required` | Unique zone identifier. |
| `slotAvailabilityId` | `integer(int64)` | `Required` | Unique slot availability identifier. |
| `plannedBookingCount` | `integer(int32)` | `Required` | Mapped planned booking count value. |
| `startDateUtc` | `datetime` | `Required` | UTC timestamp for start date. |
| `endDateUtc` | `datetime` | `Required` | UTC timestamp for end date. |
| `notes` | `string?` | `Optional` | Mapped notes value. |

### Schema: CreateCommissioningCertificateRequest
<a id="schema-createcommissioningcertificaterequest"></a>

- Source file: `Backend/Coolzo.Contracts/Requests/GapPhaseA/InstallationRequests.cs`
- Shape: `record-positional`

| Field | Wire Type | Required | Description |
| --- | --- | --- | --- |
| `customerConfirmationName` | `string` | `Required` | Display name for customer confirmation. |
| `checklistJson` | `string?` | `Optional` | JSON-serialized payload for checklist. |
| `remarks` | `string?` | `Optional` | Free-text remarks for the action. |
| `isAccepted` | `boolean` | `Required` | Boolean flag indicating whether accepted is true. |

### Schema: CreateCustomerAccountRequest
<a id="schema-createcustomeraccountrequest"></a>

- Source file: `Backend/Coolzo.Contracts/Requests/Customer/CreateCustomerAccountRequest.cs`
- Shape: `record-positional`

| Field | Wire Type | Required | Description |
| --- | --- | --- | --- |
| `customerName` | `string` | `Required` | Display name for customer. |
| `mobileNumber` | `string` | `Required` | Business number/reference for mobile. |
| `emailAddress` | `string` | `Required` | Mapped email address value. |

### Schema: CreateCustomerCancellationRequest
<a id="schema-createcustomercancellationrequest"></a>

- Source file: `Backend/Coolzo.Contracts/Requests/GapPhaseD/CancellationRefundRequests.cs`
- Shape: `record-positional`

| Field | Wire Type | Required | Description |
| --- | --- | --- | --- |
| `bookingId` | `integer(int64)?` | `Optional` | Unique booking identifier. |
| `serviceRequestId` | `integer(int64)?` | `Optional` | Unique service request identifier. |
| `cancellationReasonCode` | `string` | `Required` | Code value for cancellation reason. |
| `cancellationReasonText` | `string` | `Required` | Boolean permission flag indicating whether cellation reason text is allowed. |

### Schema: CreateEscalationRequest
<a id="schema-createescalationrequest"></a>

- Source file: `Backend/Coolzo.Contracts/Requests/GapPhaseA/EscalationRequests.cs`
- Shape: `record-positional`

| Field | Wire Type | Required | Description |
| --- | --- | --- | --- |
| `alertType` | `string` | `Required` | Mapped alert type value. |
| `relatedEntityName` | `string` | `Required` | Display name for related entity. |
| `relatedEntityId` | `string` | `Required` | Unique related entity identifier. |
| `severity` | `string` | `Required` | Mapped severity value. |
| `escalationLevel` | `integer(int32)` | `Required` | Mapped escalation level value. |
| `slaMinutes` | `integer(int32)` | `Required` | Mapped sla minutes value. |
| `notificationChain` | `string?` | `Optional` | Mapped notification chain value. |
| `message` | `string` | `Required` | Mapped message value. |

### Schema: CreateHelperProfileRequest
<a id="schema-createhelperprofilerequest"></a>

- Source file: `Backend/Coolzo.Contracts/Requests/GapPhaseE/HelperPhaseERequests.cs`
- Shape: `record-positional`

| Field | Wire Type | Required | Description |
| --- | --- | --- | --- |
| `userId` | `integer(int64)` | `Required` | Unique user identifier. |
| `helperCode` | `string` | `Required` | Code value for helper. |
| `helperName` | `string` | `Required` | Display name for helper. |
| `mobileNo` | `string` | `Required` | Mapped mobile no value. |
| `activeFlag` | `boolean` | `Required` | Mapped active flag value. |

### Schema: CreateHolidayConfigurationRequest
<a id="schema-createholidayconfigurationrequest"></a>

- Source file: `Backend/Coolzo.Contracts/Requests/Admin/AdminRequests.cs`
- Shape: `record-positional`

| Field | Wire Type | Required | Description |
| --- | --- | --- | --- |
| `holidayDate` | `date` | `Required` | Date value for holiday. |
| `holidayName` | `string` | `Required` | Display name for holiday. |
| `isRecurringAnnually` | `boolean` | `Required` | Boolean flag indicating whether recurring annually is true. |
| `isActive` | `boolean` | `Required` | Boolean flag indicating whether active is true. |

### Schema: CreateInstallationExecutionOrderRequest
<a id="schema-createinstallationexecutionorderrequest"></a>

- Source file: `Backend/Coolzo.Contracts/Requests/GapPhaseC/InstallationLifecycleRequests.cs`
- Shape: `record-positional`

| Field | Wire Type | Required | Description |
| --- | --- | --- | --- |
| `technicianId` | `integer(int64)?` | `Optional` | Unique technician identifier. |
| `scheduledInstallationDateUtc` | `datetime?` | `Optional` | UTC timestamp for scheduled installation date. |
| `helperCount` | `integer(int32)` | `Required` | Mapped helper count value. |
| `executionRemarks` | `string?` | `Optional` | Free-text remarks for execution. |

### Schema: CreateInstallationOrderRequest
<a id="schema-createinstallationorderrequest"></a>

- Source file: `Backend/Coolzo.Contracts/Requests/GapPhaseA/InstallationRequests.cs`
- Shape: `record-positional`

| Field | Wire Type | Required | Description |
| --- | --- | --- | --- |
| `leadId` | `integer(int64)?` | `Optional` | Unique lead identifier. |
| `serviceRequestId` | `integer(int64)?` | `Optional` | Unique service request identifier. |
| `customerId` | `integer(int64)` | `Required` | Unique customer identifier. |
| `customerAddressId` | `integer(int64)` | `Required` | Unique customer address identifier. |
| `technicianId` | `integer(int64)?` | `Optional` | Unique technician identifier. |
| `scheduledInstallationDateUtc` | `datetime?` | `Optional` | UTC timestamp for scheduled installation date. |
| `installationChecklistJson` | `string?` | `Optional` | JSON-serialized payload for installation checklist. |

### Schema: CreateInstallationProposalRequest
<a id="schema-createinstallationproposalrequest"></a>

- Source file: `Backend/Coolzo.Contracts/Requests/GapPhaseC/InstallationLifecycleRequests.cs`
- Shape: `record-positional`

| Field | Wire Type | Required | Description |
| --- | --- | --- | --- |
| `proposalRemarks` | `string?` | `Optional` | Free-text remarks for proposal. |
| `lines` | `InstallationProposalLineRequest[]` | `Required` | Collection of `InstallationProposalLineRequest` items. |

### Schema: CreateInstallationRequest
<a id="schema-createinstallationrequest"></a>

- Source file: `Backend/Coolzo.Contracts/Requests/GapPhaseC/InstallationLifecycleRequests.cs`
- Shape: `record-positional`

| Field | Wire Type | Required | Description |
| --- | --- | --- | --- |
| `leadId` | `integer(int64)?` | `Optional` | Unique lead identifier. |
| `customerName` | `string` | `Required` | Display name for customer. |
| `mobileNumber` | `string` | `Required` | Business number/reference for mobile. |
| `emailAddress` | `string?` | `Optional` | Mapped email address value. |
| `sourceChannel` | `string` | `Required` | Mapped source channel value. |
| `addressLine1` | `string` | `Required` | Mapped address line1 value. |
| `addressLine2` | `string?` | `Optional` | Mapped address line2 value. |
| `cityName` | `string` | `Required` | Display name for city. |
| `pincode` | `string` | `Required` | Code value for pin. |
| `installationType` | `string` | `Required` | Mapped installation type value. |
| `numberOfUnits` | `integer(int32)` | `Required` | Mapped number of units value. |
| `siteNotes` | `string?` | `Optional` | Mapped site notes value. |
| `preferredSurveyDateUtc` | `datetime?` | `Optional` | UTC timestamp for preferred survey date. |

### Schema: CreateItemRequest
<a id="schema-createitemrequest"></a>

- Source file: `Backend/Coolzo.Contracts/Requests/Inventory/CreateItemRequest.cs`
- Shape: `record-positional`

| Field | Wire Type | Required | Description |
| --- | --- | --- | --- |
| `categoryCode` | `string` | `Required` | Code value for category. |
| `categoryName` | `string` | `Required` | Display name for category. |
| `unitOfMeasureCode` | `string` | `Required` | Code value for unit of measure. |
| `unitOfMeasureName` | `string` | `Required` | Display name for unit of measure. |
| `supplierCode` | `string?` | `Optional` | Code value for supplier. |
| `supplierName` | `string?` | `Optional` | Display name for supplier. |
| `itemCode` | `string` | `Required` | Code value for item. |
| `itemName` | `string` | `Required` | Display name for item. |
| `itemDescription` | `string?` | `Optional` | Mapped item description value. |
| `purchasePrice` | `number(decimal)` | `Required` | Mapped purchase price value. |
| `sellingPrice` | `number(decimal)` | `Required` | Mapped selling price value. |
| `taxPercentage` | `number(decimal)` | `Required` | Mapped tax percentage value. |
| `warrantyDays` | `integer(int32)` | `Required` | Mapped warranty days value. |
| `reorderLevel` | `number(decimal)` | `Required` | Mapped reorder level value. |
| `isActive` | `boolean` | `Required` | Boolean flag indicating whether active is true. |

### Schema: CreateLeadRequest
<a id="schema-createleadrequest"></a>

- Source file: `Backend/Coolzo.Contracts/Requests/GapPhaseA/LeadManagementRequests.cs`
- Shape: `record-positional`

| Field | Wire Type | Required | Description |
| --- | --- | --- | --- |
| `customerName` | `string` | `Required` | Display name for customer. |
| `mobileNumber` | `string` | `Required` | Business number/reference for mobile. |
| `emailAddress` | `string?` | `Optional` | Mapped email address value. |
| `sourceChannel` | `string` | `Required` | Mapped source channel value. |
| `addressLine1` | `string?` | `Optional` | Mapped address line1 value. |
| `addressLine2` | `string?` | `Optional` | Mapped address line2 value. |
| `cityName` | `string?` | `Optional` | Display name for city. |
| `pincode` | `string?` | `Optional` | Code value for pin. |
| `serviceId` | `integer(int64)?` | `Optional` | Unique service identifier. |
| `acTypeId` | `integer(int64)?` | `Optional` | Unique ac type identifier. |
| `tonnageId` | `integer(int64)?` | `Optional` | Unique tonnage identifier. |
| `brandId` | `integer(int64)?` | `Optional` | Unique brand identifier. |
| `slotAvailabilityId` | `integer(int64)?` | `Optional` | Unique slot availability identifier. |
| `inquiryNotes` | `string?` | `Optional` | Mapped inquiry notes value. |

### Schema: CreatePartsReturnRequest
<a id="schema-createpartsreturnrequest"></a>

- Source file: `Backend/Coolzo.Contracts/Requests/GapPhaseA/PartsReturnRequests.cs`
- Shape: `record-positional`

| Field | Wire Type | Required | Description |
| --- | --- | --- | --- |
| `itemId` | `integer(int64)` | `Required` | Unique item identifier. |
| `quantity` | `number(decimal)` | `Required` | Mapped quantity value. |
| `reasonCode` | `string` | `Required` | Code value for reason. |
| `defectDescription` | `string` | `Required` | Mapped defect description value. |
| `technicianId` | `integer(int64)?` | `Optional` | Unique technician identifier. |
| `jobCardId` | `integer(int64)?` | `Optional` | Unique job card identifier. |

### Schema: CreateQuotationFromJobRequest
<a id="schema-createquotationfromjobrequest"></a>

- Source file: `Backend/Coolzo.Contracts/Requests/Billing/CreateQuotationFromJobRequest.cs`
- Shape: `record-positional`

| Field | Wire Type | Required | Description |
| --- | --- | --- | --- |
| `lines` | `QuotationLineRequest[]` | `Required` | Collection of `QuotationLineRequest` items. |
| `discountAmount` | `number(decimal)` | `Required` | Monetary amount for discount. |
| `taxPercentage` | `number(decimal)` | `Required` | Mapped tax percentage value. |
| `remarks` | `string?` | `Optional` | Free-text remarks for the action. |

### Schema: CreateRefundRequestCommandRequest
<a id="schema-createrefundrequestcommandrequest"></a>

- Source file: `Backend/Coolzo.Contracts/Requests/GapPhaseD/CancellationRefundRequests.cs`
- Shape: `record-positional`

| Field | Wire Type | Required | Description |
| --- | --- | --- | --- |
| `cancellationRecordId` | `integer(int64)` | `Required` | Unique cancellation record identifier. |
| `invoiceId` | `integer(int64)?` | `Optional` | Unique invoice identifier. |
| `refundAmount` | `number(decimal)` | `Required` | Monetary amount for refund. |
| `refundMethod` | `string` | `Required` | Mapped refund method value. |
| `refundReason` | `string` | `Required` | Mapped refund reason value. |

### Schema: CreateRoleRequest
<a id="schema-createrolerequest"></a>

- Source file: `Backend/Coolzo.Contracts/Requests/Role/CreateRoleRequest.cs`
- Shape: `record-positional`

| Field | Wire Type | Required | Description |
| --- | --- | --- | --- |
| `roleName` | `string` | `Required` | Display name for role. |
| `displayName` | `string` | `Required` | Display name for display. |
| `description` | `string` | `Required` | Mapped description value. |
| `isActive` | `boolean` | `Required` | Boolean flag indicating whether active is true. |
| `permissionIds` | `long[]` | `Required` | List of permission identifiers. |

### Schema: CreateSkillAssessmentRequest
<a id="schema-createskillassessmentrequest"></a>

- Source file: `Backend/Coolzo.Contracts/Requests/GapPhaseE/TechnicianOnboardingPhaseERequests.cs`
- Shape: `record-positional`

| Field | Wire Type | Required | Description |
| --- | --- | --- | --- |
| `skillTagId` | `integer(int64)?` | `Optional` | Unique skill tag identifier. |
| `assessmentCode` | `string` | `Required` | Code value for assessment. |
| `assessmentName` | `string` | `Required` | Display name for assessment. |
| `remarks` | `string?` | `Optional` | Free-text remarks for the action. |

### Schema: CreateSupplierClaimRequest
<a id="schema-createsupplierclaimrequest"></a>

- Source file: `Backend/Coolzo.Contracts/Requests/GapPhaseA/PartsReturnRequests.cs`
- Shape: `record-positional`

| Field | Wire Type | Required | Description |
| --- | --- | --- | --- |
| `partsReturnId` | `integer(int64)` | `Required` | Unique parts return identifier. |
| `supplierClaimReference` | `string` | `Required` | Reference value for supplier claim. |
| `remarks` | `string?` | `Optional` | Free-text remarks for the action. |

### Schema: CreateSupportTicketRequest
<a id="schema-createsupportticketrequest"></a>

- Source file: `Backend/Coolzo.Contracts/Requests/Support/CreateSupportTicketRequest.cs`
- Shape: `record-positional`

| Field | Wire Type | Required | Description |
| --- | --- | --- | --- |
| `customerId` | `integer(int64)?` | `Optional` | Unique customer identifier. |
| `subject` | `string` | `Required` | Mapped subject value. |
| `categoryId` | `integer(int64)` | `Required` | Unique category identifier. |
| `priorityId` | `integer(int64)` | `Required` | Unique priority identifier. |
| `description` | `string` | `Required` | Mapped description value. |
| `links` | `CreateSupportTicketLinkRequest[]` | `Required` | Collection of `CreateSupportTicketLinkRequest` items. |

### Schema: CreateTechnicianDraftRequest
<a id="schema-createtechniciandraftrequest"></a>

- Source file: `Backend/Coolzo.Contracts/Requests/GapPhaseA/TechnicianOnboardingRequests.cs`
- Shape: `record-positional`

| Field | Wire Type | Required | Description |
| --- | --- | --- | --- |
| `technicianName` | `string` | `Required` | Display name for technician. |
| `mobileNumber` | `string` | `Required` | Business number/reference for mobile. |
| `emailAddress` | `string?` | `Optional` | Mapped email address value. |
| `baseZoneId` | `integer(int64)?` | `Optional` | Unique base zone identifier. |
| `maxDailyAssignments` | `integer(int32)` | `Required` | Mapped max daily assignments value. |

### Schema: CreateTrainingRecordRequest
<a id="schema-createtrainingrecordrequest"></a>

- Source file: `Backend/Coolzo.Contracts/Requests/GapPhaseE/TechnicianOnboardingPhaseERequests.cs`
- Shape: `record-positional`

| Field | Wire Type | Required | Description |
| --- | --- | --- | --- |
| `trainingTitle` | `string` | `Required` | Mapped training title value. |
| `trainingType` | `string` | `Required` | Mapped training type value. |
| `remarks` | `string?` | `Optional` | Free-text remarks for the action. |

### Schema: CreateUserRequest
<a id="schema-createuserrequest"></a>

- Source file: `Backend/Coolzo.Contracts/Requests/User/CreateUserRequest.cs`
- Shape: `record-positional`

| Field | Wire Type | Required | Description |
| --- | --- | --- | --- |
| `userName` | `string` | `Required` | Display name for user. |
| `email` | `string` | `Required` | Mapped email value. |
| `fullName` | `string` | `Required` | Display name for full. |
| `password` | `string` | `Required` | Mapped password value. |
| `isActive` | `boolean` | `Required` | Boolean flag indicating whether active is true. |
| `roleIds` | `long[]` | `Required` | List of role identifiers. |

### Schema: CreateWarehouseRequest
<a id="schema-createwarehouserequest"></a>

- Source file: `Backend/Coolzo.Contracts/Requests/Inventory/CreateWarehouseRequest.cs`
- Shape: `record-positional`

| Field | Wire Type | Required | Description |
| --- | --- | --- | --- |
| `warehouseCode` | `string` | `Required` | Code value for warehouse. |
| `warehouseName` | `string` | `Required` | Display name for warehouse. |
| `contactPerson` | `string?` | `Optional` | Mapped contact person value. |
| `mobileNumber` | `string?` | `Optional` | Business number/reference for mobile. |
| `emailAddress` | `string?` | `Optional` | Mapped email address value. |
| `addressLine1` | `string?` | `Optional` | Mapped address line1 value. |
| `addressLine2` | `string?` | `Optional` | Mapped address line2 value. |
| `landmark` | `string?` | `Optional` | Mapped landmark value. |
| `cityName` | `string?` | `Optional` | Display name for city. |
| `pincode` | `string?` | `Optional` | Code value for pin. |
| `isActive` | `boolean` | `Required` | Boolean flag indicating whether active is true. |

### Schema: CreateWarrantyClaimRequest
<a id="schema-createwarrantyclaimrequest"></a>

- Source file: `Backend/Coolzo.Contracts/Requests/Warranty/CreateWarrantyClaimRequest.cs`
- Shape: `record-positional`

| Field | Wire Type | Required | Description |
| --- | --- | --- | --- |
| `invoiceId` | `integer(int64)` | `Required` | Unique invoice identifier. |
| `claimRemarks` | `string?` | `Optional` | Free-text remarks for claim. |

### Schema: CurrentUserResponse
<a id="schema-currentuserresponse"></a>

- Source file: `Backend/Coolzo.Contracts/Responses/Auth/CurrentUserResponse.cs`
- Shape: `record-positional`

| Field | Wire Type | Required | Description |
| --- | --- | --- | --- |
| `userId` | `integer(int64)` | `Required` | Unique user identifier. |
| `userName` | `string` | `Required` | Display name for user. |
| `email` | `string` | `Required` | Mapped email value. |
| `fullName` | `string` | `Required` | Display name for full. |
| `technicianId` | `integer(int64)?` | `Optional` | Unique technician identifier. |
| `helperProfileId` | `integer(int64)?` | `Optional` | Unique helper profile identifier. |
| `roles` | `string[]` | `Required` | Collection of `string` items. |
| `permissions` | `string[]` | `Required` | Collection of `string` items. |
| `customerId` | `integer(int64)?` | `Optional` | Unique customer identifier. |
| `mustChangePassword` | `boolean` | `Required` | Mapped must change password value. |
| `isTemporaryPassword` | `boolean` | `Required` | Boolean flag indicating whether temporary password is true. |
| `passwordExpiryOnUtc` | `datetime?` | `Optional` | Mapped password expiry on utc value. |

### Schema: CustomerAbsentDetailResponse
<a id="schema-customerabsentdetailresponse"></a>

- Source file: `Backend/Coolzo.Contracts/Responses/GapPhaseD/CancellationRefundResponses.cs`
- Shape: `record-positional`

| Field | Wire Type | Required | Description |
| --- | --- | --- | --- |
| `customerAbsentRecordId` | `integer(int64)` | `Required` | Unique customer absent record identifier. |
| `serviceRequestId` | `integer(int64)` | `Required` | Unique service request identifier. |
| `serviceRequestNumber` | `string` | `Required` | Business number/reference for service request. |
| `bookingId` | `integer(int64)` | `Required` | Unique booking identifier. |
| `bookingReference` | `string` | `Required` | Reference value for booking. |
| `technicianId` | `integer(int64)` | `Required` | Unique technician identifier. |
| `technicianName` | `string` | `Required` | Display name for technician. |
| `markedOn` | `datetime` | `Required` | Mapped marked on value. |
| `attemptCount` | `integer(int32)` | `Required` | Mapped attempt count value. |
| `contactAttemptLog` | `string` | `Required` | Mapped contact attempt log value. |
| `absentReasonCode` | `string` | `Required` | Code value for absent reason. |
| `absentReasonText` | `string` | `Required` | Mapped absent reason text value. |
| `customerAbsentStatus` | `string` | `Required` | Current status value for customer absent. |
| `serviceRequestStatus` | `string` | `Required` | Current status value for service request. |
| `resolutionRemarks` | `string` | `Required` | Free-text remarks for resolution. |
| `resolvedOn` | `datetime?` | `Optional` | Mapped resolved on value. |

### Schema: CustomerAccountResponse
<a id="schema-customeraccountresponse"></a>

- Source file: `Backend/Coolzo.Contracts/Responses/CustomerAuth/CustomerAccountResponse.cs`
- Shape: `record-positional`

| Field | Wire Type | Required | Description |
| --- | --- | --- | --- |
| `customerId` | `integer(int64)` | `Required` | Unique customer identifier. |
| `userId` | `integer(int64)` | `Required` | Unique user identifier. |
| `customerName` | `string` | `Required` | Display name for customer. |
| `mobileNumber` | `string` | `Required` | Business number/reference for mobile. |
| `emailAddress` | `string` | `Required` | Mapped email address value. |
| `passwordGenerated` | `boolean` | `Required` | Mapped password generated value. |
| `requiresPasswordDelivery` | `boolean` | `Required` | Mapped requires password delivery value. |
| `mustChangePassword` | `boolean` | `Required` | Mapped must change password value. |
| `isTemporaryPassword` | `boolean` | `Required` | Boolean flag indicating whether temporary password is true. |
| `passwordExpiryOnUtc` | `datetime?` | `Optional` | Mapped password expiry on utc value. |

### Schema: CustomerAmcResponse
<a id="schema-customeramcresponse"></a>

- Source file: `Backend/Coolzo.Contracts/Responses/Amc/CustomerAmcResponse.cs`
- Shape: `record-positional`

| Field | Wire Type | Required | Description |
| --- | --- | --- | --- |
| `customerAmcId` | `integer(int64)` | `Required` | Unique customer amc identifier. |
| `customerId` | `integer(int64)` | `Required` | Unique customer identifier. |
| `customerName` | `string` | `Required` | Display name for customer. |
| `amcPlanId` | `integer(int64)` | `Required` | Unique amc plan identifier. |
| `planName` | `string` | `Required` | Display name for plan. |
| `jobCardId` | `integer(int64)` | `Required` | Unique job card identifier. |
| `jobCardNumber` | `string` | `Required` | Business number/reference for job card. |
| `invoiceId` | `integer(int64)` | `Required` | Unique invoice identifier. |
| `invoiceNumber` | `string` | `Required` | Business number/reference for invoice. |
| `currentStatus` | `string` | `Required` | Current status value for current. |
| `startDateUtc` | `datetime` | `Required` | UTC timestamp for start date. |
| `endDateUtc` | `datetime` | `Required` | UTC timestamp for end date. |
| `totalVisitCount` | `integer(int32)` | `Required` | Mapped total visit count value. |
| `consumedVisitCount` | `integer(int32)` | `Required` | Mapped consumed visit count value. |
| `priceAmount` | `number(decimal)` | `Required` | Monetary amount for price. |
| `visits` | `AmcVisitScheduleResponse[]` | `Required` | Collection of `AmcVisitScheduleResponse` items. |

### Schema: CustomerAnalyticsResponse
<a id="schema-customeranalyticsresponse"></a>

- Source file: `Backend/Coolzo.Contracts/Responses/Analytics/AnalyticsResponses.cs`
- Shape: `record-positional`

| Field | Wire Type | Required | Description |
| --- | --- | --- | --- |
| `totalCustomers` | `integer(int64)` | `Required` | Mapped total customers value. |
| `newCustomers` | `integer(int64)` | `Required` | Mapped new customers value. |
| `returningCustomers` | `integer(int64)` | `Required` | Mapped returning customers value. |
| `repeatCustomers` | `integer(int64)` | `Required` | Mapped repeat customers value. |
| `amcCustomers` | `integer(int64)` | `Required` | Mapped amc customers value. |
| `nonAmcCustomers` | `integer(int64)` | `Required` | Mapped non amc customers value. |
| `repeatRatePercentage` | `number(decimal)` | `Required` | Mapped repeat rate percentage value. |
| `segmentDistribution` | `AnalyticsBreakdownItemResponse[]` | `Required` | Collection of `AnalyticsBreakdownItemResponse` items. |
| `customerTrends` | `CustomerTrendPointResponse[]` | `Required` | Collection of `CustomerTrendPointResponse` items. |

### Schema: CustomerBookingCreateRequest
<a id="schema-customerbookingcreaterequest"></a>

- Source file: `Backend/Coolzo.Contracts/Requests/Booking/CustomerBookingCreateRequest.cs`
- Shape: `record-positional`

| Field | Wire Type | Required | Description |
| --- | --- | --- | --- |
| `serviceId` | `integer(int64)` | `Required` | Unique service identifier. |
| `acTypeId` | `integer(int64)` | `Required` | Unique ac type identifier. |
| `tonnageId` | `integer(int64)` | `Required` | Unique tonnage identifier. |
| `brandId` | `integer(int64)` | `Required` | Unique brand identifier. |
| `slotAvailabilityId` | `integer(int64)` | `Required` | Unique slot availability identifier. |
| `customerName` | `string` | `Required` | Display name for customer. |
| `mobileNumber` | `string` | `Required` | Business number/reference for mobile. |
| `emailAddress` | `string?` | `Optional` | Mapped email address value. |
| `addressLine1` | `string` | `Required` | Mapped address line1 value. |
| `addressLine2` | `string?` | `Optional` | Mapped address line2 value. |
| `landmark` | `string?` | `Optional` | Mapped landmark value. |
| `cityName` | `string` | `Required` | Display name for city. |
| `pincode` | `string` | `Required` | Code value for pin. |
| `addressLabel` | `string?` | `Optional` | Mapped address label value. |
| `modelName` | `string?` | `Optional` | Display name for model. |
| `issueNotes` | `string?` | `Optional` | Boolean flag indicating whether sue notes is true. |
| `sourceChannel` | `string` | `Required` | Mapped source channel value. |

### Schema: CustomerPasswordOperationResponse
<a id="schema-customerpasswordoperationresponse"></a>

- Source file: `Backend/Coolzo.Contracts/Responses/CustomerAuth/CustomerPasswordOperationResponse.cs`
- Shape: `record-positional`

| Field | Wire Type | Required | Description |
| --- | --- | --- | --- |
| `passwordUpdated` | `boolean` | `Required` | Mapped password updated value. |
| `passwordGenerated` | `boolean` | `Required` | Mapped password generated value. |
| `requiresPasswordDelivery` | `boolean` | `Required` | Mapped requires password delivery value. |
| `mustChangePassword` | `boolean` | `Required` | Mapped must change password value. |
| `isTemporaryPassword` | `boolean` | `Required` | Boolean flag indicating whether temporary password is true. |
| `passwordExpiryOnUtc` | `datetime?` | `Optional` | Mapped password expiry on utc value. |

### Schema: CustomerRefundStatusResponse
<a id="schema-customerrefundstatusresponse"></a>

- Source file: `Backend/Coolzo.Contracts/Responses/GapPhaseD/CancellationRefundResponses.cs`
- Shape: `record-positional`

| Field | Wire Type | Required | Description |
| --- | --- | --- | --- |
| `refundRequestId` | `integer(int64)` | `Required` | Unique refund request identifier. |
| `refundRequestNo` | `string` | `Required` | Mapped refund request no value. |
| `refundAmount` | `number(decimal)` | `Required` | Monetary amount for refund. |
| `refundMethod` | `string` | `Required` | Mapped refund method value. |
| `refundStatus` | `string` | `Required` | Current status value for refund. |
| `dateCreated` | `datetime` | `Required` | Mapped date created value. |
| `processedOn` | `datetime?` | `Optional` | Mapped processed on value. |

### Schema: DashboardMetricsResponse
<a id="schema-dashboardmetricsresponse"></a>

- Source file: `Backend/Coolzo.Contracts/Responses/Analytics/AnalyticsResponses.cs`
- Shape: `record-positional`

| Field | Wire Type | Required | Description |
| --- | --- | --- | --- |
| `bookingTrends` | `AnalyticsTrendPointResponse[]` | `Required` | Collection of `AnalyticsTrendPointResponse` items. |
| `jobStatusDistribution` | `AnalyticsBreakdownItemResponse[]` | `Required` | Collection of `AnalyticsBreakdownItemResponse` items. |
| `revenueSummary` | `RevenueSummarySnapshotResponse` | `Required` | Summary text for revenue. |
| `supportOverview` | `SupportOverviewSnapshotResponse` | `Required` | Mapped support overview value. |

### Schema: DashboardSummaryResponse
<a id="schema-dashboardsummaryresponse"></a>

- Source file: `Backend/Coolzo.Contracts/Responses/Analytics/AnalyticsResponses.cs`
- Shape: `record-positional`

| Field | Wire Type | Required | Description |
| --- | --- | --- | --- |
| `totalBookings` | `integer(int64)` | `Required` | Mapped total bookings value. |
| `totalServiceRequests` | `integer(int64)` | `Required` | Mapped total service requests value. |
| `totalJobs` | `integer(int64)` | `Required` | Mapped total jobs value. |
| `totalRevenue` | `number(decimal)` | `Required` | Mapped total revenue value. |
| `totalAmcCustomers` | `integer(int64)` | `Required` | Mapped total amc customers value. |
| `totalSupportTickets` | `integer(int64)` | `Required` | Mapped total support tickets value. |

### Schema: DateRangeReportResponse
<a id="schema-daterangereportresponse"></a>

- Source file: `Backend/Coolzo.Contracts/Responses/Analytics/AnalyticsResponses.cs`
- Shape: `record-positional`

| Field | Wire Type | Required | Description |
| --- | --- | --- | --- |
| `dateFrom` | `string` | `Required` | Mapped date from value. |
| `dateTo` | `string` | `Required` | Mapped date to value. |
| `totalBookings` | `integer(int64)` | `Required` | Mapped total bookings value. |
| `totalRevenue` | `number(decimal)` | `Required` | Mapped total revenue value. |
| `completedJobs` | `integer(int64)` | `Required` | Mapped completed jobs value. |
| `totalSupportTickets` | `integer(int64)` | `Required` | Mapped total support tickets value. |
| `activeTechnicians` | `integer(int64)` | `Required` | Mapped active technicians value. |
| `newCustomers` | `integer(int64)` | `Required` | Mapped new customers value. |
| `bookingTrends` | `AnalyticsTrendPointResponse[]` | `Required` | Collection of `AnalyticsTrendPointResponse` items. |
| `revenueTrends` | `AnalyticsTrendPointResponse[]` | `Required` | Collection of `AnalyticsTrendPointResponse` items. |
| `supportStatusDistribution` | `AnalyticsBreakdownItemResponse[]` | `Required` | Collection of `AnalyticsBreakdownItemResponse` items. |

### Schema: DeactivateTechnicianRequest
<a id="schema-deactivatetechnicianrequest"></a>

- Source file: `Backend/Coolzo.Contracts/Requests/GapPhaseE/TechnicianOnboardingPhaseERequests.cs`
- Shape: `record-positional`

| Field | Wire Type | Required | Description |
| --- | --- | --- | --- |
| `activationReason` | `string` | `Required` | Mapped activation reason value. |

### Schema: DiagnosisLookupItemResponse
<a id="schema-diagnosislookupitemresponse"></a>

- Source file: `Backend/Coolzo.Contracts/Responses/FieldExecution/DiagnosisLookupItemResponse.cs`
- Shape: `record-positional`

| Field | Wire Type | Required | Description |
| --- | --- | --- | --- |
| `id` | `integer(int64)` | `Required` | Unique record identifier. |
| `name` | `string` | `Required` | Display name for the record. |
| `description` | `string` | `Required` | Mapped description value. |

### Schema: DynamicMasterRecordResponse
<a id="schema-dynamicmasterrecordresponse"></a>

- Source file: `Backend/Coolzo.Contracts/Responses/Admin/AdminResponses.cs`
- Shape: `record-positional`

| Field | Wire Type | Required | Description |
| --- | --- | --- | --- |
| `dynamicMasterRecordId` | `integer(int64)` | `Required` | Unique dynamic master record identifier. |
| `masterType` | `string` | `Required` | Mapped master type value. |
| `masterCode` | `string` | `Required` | Code value for master. |
| `masterLabel` | `string` | `Required` | Mapped master label value. |
| `masterValue` | `string` | `Required` | Mapped master value value. |
| `description` | `string` | `Required` | Mapped description value. |
| `isActive` | `boolean` | `Required` | Boolean flag indicating whether active is true. |
| `isPublished` | `boolean` | `Required` | Boolean flag indicating whether published is true. |
| `sortOrder` | `integer(int32)` | `Required` | Mapped sort order value. |
| `dateCreated` | `datetime` | `Required` | Mapped date created value. |
| `lastUpdated` | `datetime?` | `Optional` | Mapped last updated value. |

### Schema: DynamicMasterRecordUpsertRequest
<a id="schema-dynamicmasterrecordupsertrequest"></a>

- Source file: `Backend/Coolzo.Contracts/Requests/Admin/AdminRequests.cs`
- Shape: `record-positional`

| Field | Wire Type | Required | Description |
| --- | --- | --- | --- |
| `masterType` | `string` | `Required` | Mapped master type value. |
| `masterCode` | `string` | `Required` | Code value for master. |
| `masterLabel` | `string` | `Required` | Mapped master label value. |
| `masterValue` | `string` | `Required` | Mapped master value value. |
| `description` | `string?` | `Optional` | Mapped description value. |
| `isActive` | `boolean` | `Required` | Boolean flag indicating whether active is true. |
| `isPublished` | `boolean` | `Required` | Boolean flag indicating whether published is true. |
| `sortOrder` | `integer(int32)` | `Required` | Mapped sort order value. |

### Schema: EscalateSupportTicketRequest
<a id="schema-escalatesupportticketrequest"></a>

- Source file: `Backend/Coolzo.Contracts/Requests/Support/EscalateSupportTicketRequest.cs`
- Shape: `record-positional`

| Field | Wire Type | Required | Description |
| --- | --- | --- | --- |
| `escalationTarget` | `string` | `Required` | Mapped escalation target value. |
| `escalationRemarks` | `string` | `Required` | Free-text remarks for escalation. |

### Schema: EscalationResponse
<a id="schema-escalationresponse"></a>

- Source file: `Backend/Coolzo.Contracts/Responses/GapPhaseA/GapPhaseAResponses.cs`
- Shape: `record-positional`

| Field | Wire Type | Required | Description |
| --- | --- | --- | --- |
| `systemAlertId` | `integer(int64)` | `Required` | Unique system alert identifier. |
| `alertCode` | `string` | `Required` | Code value for alert. |
| `alertType` | `string` | `Required` | Mapped alert type value. |
| `relatedEntityName` | `string` | `Required` | Display name for related entity. |
| `relatedEntityId` | `string` | `Required` | Unique related entity identifier. |
| `severity` | `string` | `Required` | Mapped severity value. |
| `alertStatus` | `string` | `Required` | Current status value for alert. |
| `escalationLevel` | `integer(int32)` | `Required` | Mapped escalation level value. |

### Schema: ForgotCustomerPasswordRequest
<a id="schema-forgotcustomerpasswordrequest"></a>

- Source file: `Backend/Coolzo.Contracts/Requests/CustomerAuth/ForgotCustomerPasswordRequest.cs`
- Shape: `record-positional`

| Field | Wire Type | Required | Description |
| --- | --- | --- | --- |
| `loginId` | `string` | `Required` | Unique login identifier. |

### Schema: GenerateInstallationCommissioningRequest
<a id="schema-generateinstallationcommissioningrequest"></a>

- Source file: `Backend/Coolzo.Contracts/Requests/GapPhaseC/InstallationLifecycleRequests.cs`
- Shape: `record-positional`

| Field | Wire Type | Required | Description |
| --- | --- | --- | --- |
| `customerConfirmationName` | `string` | `Required` | Display name for customer confirmation. |
| `customerSignatureName` | `string` | `Required` | Display name for customer signature. |
| `checklistJson` | `string?` | `Optional` | JSON-serialized payload for checklist. |
| `remarks` | `string?` | `Optional` | Free-text remarks for the action. |
| `isAccepted` | `boolean` | `Required` | Boolean flag indicating whether accepted is true. |

### Schema: GuestBookingCreateRequest
<a id="schema-guestbookingcreaterequest"></a>

- Source file: `Backend/Coolzo.Contracts/Requests/Booking/GuestBookingCreateRequest.cs`
- Shape: `record-positional`

| Field | Wire Type | Required | Description |
| --- | --- | --- | --- |
| `serviceId` | `integer(int64)` | `Required` | Unique service identifier. |
| `acTypeId` | `integer(int64)` | `Required` | Unique ac type identifier. |
| `tonnageId` | `integer(int64)` | `Required` | Unique tonnage identifier. |
| `brandId` | `integer(int64)` | `Required` | Unique brand identifier. |
| `slotAvailabilityId` | `integer(int64)` | `Required` | Unique slot availability identifier. |
| `customerName` | `string` | `Required` | Display name for customer. |
| `mobileNumber` | `string` | `Required` | Business number/reference for mobile. |
| `emailAddress` | `string?` | `Optional` | Mapped email address value. |
| `addressLine1` | `string` | `Required` | Mapped address line1 value. |
| `addressLine2` | `string?` | `Optional` | Mapped address line2 value. |
| `landmark` | `string?` | `Optional` | Mapped landmark value. |
| `cityName` | `string` | `Required` | Display name for city. |
| `pincode` | `string` | `Required` | Code value for pin. |
| `addressLabel` | `string?` | `Optional` | Mapped address label value. |
| `modelName` | `string?` | `Optional` | Display name for model. |
| `issueNotes` | `string?` | `Optional` | Boolean flag indicating whether sue notes is true. |
| `sourceChannel` | `string` | `Required` | Mapped source channel value. |

### Schema: HandleNoShowRequest
<a id="schema-handlenoshowrequest"></a>

- Source file: `Backend/Coolzo.Contracts/Requests/GapPhaseA/EscalationRequests.cs`
- Shape: `record-positional`

| Field | Wire Type | Required | Description |
| --- | --- | --- | --- |
| `reason` | `string` | `Required` | Mapped reason value. |
| `preferredTechnicianId` | `integer(int64)?` | `Optional` | Unique preferred technician identifier. |

### Schema: HelperAssignmentDetailResponse
<a id="schema-helperassignmentdetailresponse"></a>

- Source file: `Backend/Coolzo.Contracts/Responses/GapPhaseE/HelperPhaseEResponses.cs`
- Shape: `record-positional`

| Field | Wire Type | Required | Description |
| --- | --- | --- | --- |
| `helperAssignmentId` | `integer(int64)?` | `Optional` | Unique helper assignment identifier. |
| `assignmentStatus` | `string` | `Required` | Current status value for assignment. |
| `technicianId` | `integer(int64)?` | `Optional` | Unique technician identifier. |
| `technicianName` | `string?` | `Optional` | Display name for technician. |
| `serviceRequestId` | `integer(int64)?` | `Optional` | Unique service request identifier. |
| `serviceRequestNumber` | `string?` | `Optional` | Business number/reference for service request. |
| `jobCardId` | `integer(int64)?` | `Optional` | Unique job card identifier. |
| `jobCardNumber` | `string?` | `Optional` | Business number/reference for job card. |
| `customerName` | `string?` | `Optional` | Display name for customer. |
| `serviceName` | `string?` | `Optional` | Display name for service. |
| `addressSummary` | `string?` | `Optional` | Summary text for address. |
| `assignmentRemarks` | `string` | `Required` | Free-text remarks for assignment. |
| `assignedOnUtc` | `datetime?` | `Optional` | Mapped assigned on utc value. |
| `releasedOnUtc` | `datetime?` | `Optional` | Mapped released on utc value. |

### Schema: HelperAttendanceResponse
<a id="schema-helperattendanceresponse"></a>

- Source file: `Backend/Coolzo.Contracts/Responses/GapPhaseE/HelperPhaseEResponses.cs`
- Shape: `record-positional`

| Field | Wire Type | Required | Description |
| --- | --- | --- | --- |
| `helperAttendanceId` | `integer(int64)` | `Required` | Unique helper attendance identifier. |
| `attendanceDate` | `date` | `Required` | Date value for attendance. |
| `checkInOnUtc` | `datetime?` | `Optional` | Mapped check in on utc value. |
| `checkOutOnUtc` | `datetime?` | `Optional` | Mapped check out on utc value. |
| `attendanceStatus` | `string` | `Required` | Current status value for attendance. |
| `locationText` | `string` | `Required` | Mapped location text value. |

### Schema: HelperDetailResponse
<a id="schema-helperdetailresponse"></a>

- Source file: `Backend/Coolzo.Contracts/Responses/GapPhaseE/HelperPhaseEResponses.cs`
- Shape: `record-positional`

| Field | Wire Type | Required | Description |
| --- | --- | --- | --- |
| `helperProfileId` | `integer(int64)` | `Required` | Unique helper profile identifier. |
| `userId` | `integer(int64)` | `Required` | Unique user identifier. |
| `helperCode` | `string` | `Required` | Code value for helper. |
| `helperName` | `string` | `Required` | Display name for helper. |
| `mobileNo` | `string` | `Required` | Mapped mobile no value. |
| `activeFlag` | `boolean` | `Required` | Mapped active flag value. |
| `currentAssignment` | `HelperAssignmentDetailResponse` | `Required` | Mapped current assignment value. |
| `attendanceHistory` | `HelperAttendanceResponse[]` | `Required` | Collection of `HelperAttendanceResponse` items. |
| `taskResponses` | `HelperTaskChecklistResponse[]` | `Required` | Collection of `HelperTaskChecklistResponse` items. |

### Schema: HelperListItemResponse
<a id="schema-helperlistitemresponse"></a>

- Source file: `Backend/Coolzo.Contracts/Responses/GapPhaseE/HelperPhaseEResponses.cs`
- Shape: `record-positional`

| Field | Wire Type | Required | Description |
| --- | --- | --- | --- |
| `helperProfileId` | `integer(int64)` | `Required` | Unique helper profile identifier. |
| `helperCode` | `string` | `Required` | Code value for helper. |
| `helperName` | `string` | `Required` | Display name for helper. |
| `mobileNo` | `string` | `Required` | Mapped mobile no value. |
| `activeFlag` | `boolean` | `Required` | Mapped active flag value. |
| `currentAssignmentStatus` | `string?` | `Optional` | Current status value for current assignment. |
| `pairedTechnicianName` | `string?` | `Optional` | Display name for paired technician. |
| `serviceRequestNumber` | `string?` | `Optional` | Business number/reference for service request. |

### Schema: HelperTaskChecklistResponse
<a id="schema-helpertaskchecklistresponse"></a>

- Source file: `Backend/Coolzo.Contracts/Responses/GapPhaseE/HelperPhaseEResponses.cs`
- Shape: `record-positional`

| Field | Wire Type | Required | Description |
| --- | --- | --- | --- |
| `helperTaskChecklistId` | `integer(int64)` | `Required` | Unique helper task checklist identifier. |
| `taskName` | `string` | `Required` | Display name for task. |
| `taskDescription` | `string` | `Required` | Mapped task description value. |
| `mandatoryFlag` | `boolean` | `Required` | Mapped mandatory flag value. |
| `sortOrder` | `integer(int32)` | `Required` | Mapped sort order value. |
| `responseStatus` | `string` | `Required` | Current status value for response. |
| `responseRemarks` | `string` | `Required` | Free-text remarks for response. |
| `responsePhotoUrl` | `string` | `Required` | Mapped response photo url value. |
| `respondedOnUtc` | `datetime?` | `Optional` | Mapped responded on utc value. |

### Schema: HolidayConfigurationResponse
<a id="schema-holidayconfigurationresponse"></a>

- Source file: `Backend/Coolzo.Contracts/Responses/Admin/AdminResponses.cs`
- Shape: `record-positional`

| Field | Wire Type | Required | Description |
| --- | --- | --- | --- |
| `holidayConfigurationId` | `integer(int64)` | `Required` | Unique holiday configuration identifier. |
| `holidayDate` | `string` | `Required` | Date value for holiday. |
| `holidayName` | `string` | `Required` | Display name for holiday. |
| `isRecurringAnnually` | `boolean` | `Required` | Boolean flag indicating whether recurring annually is true. |
| `isActive` | `boolean` | `Required` | Boolean flag indicating whether active is true. |

### Schema: InitiateRefundRequest
<a id="schema-initiaterefundrequest"></a>

- Source file: `Backend/Coolzo.Contracts/Requests/GapPhaseA/CancellationRefundRequests.cs`
- Shape: `record-positional`

| Field | Wire Type | Required | Description |
| --- | --- | --- | --- |
| `cancellationRecordId` | `integer(int64)` | `Required` | Unique cancellation record identifier. |
| `invoiceId` | `integer(int64)` | `Required` | Unique invoice identifier. |
| `requestedAmount` | `number(decimal)` | `Required` | Monetary amount for requested. |
| `reason` | `string` | `Required` | Mapped reason value. |

### Schema: InstallationDetailResponse
<a id="schema-installationdetailresponse"></a>

- Source file: `Backend/Coolzo.Contracts/Responses/GapPhaseC/InstallationLifecycleResponses.cs`
- Shape: `record-positional`

| Field | Wire Type | Required | Description |
| --- | --- | --- | --- |
| `installationId` | `integer(int64)` | `Required` | Unique installation identifier. |
| `installationNumber` | `string` | `Required` | Business number/reference for installation. |
| `leadId` | `integer(int64)?` | `Optional` | Unique lead identifier. |
| `leadNumber` | `string?` | `Optional` | Business number/reference for lead. |
| `customerId` | `integer(int64)` | `Required` | Unique customer identifier. |
| `customerName` | `string` | `Required` | Display name for customer. |
| `mobileNumber` | `string` | `Required` | Business number/reference for mobile. |
| `emailAddress` | `string?` | `Optional` | Mapped email address value. |
| `customerAddressId` | `integer(int64)` | `Required` | Unique customer address identifier. |
| `addressLine1` | `string` | `Required` | Mapped address line1 value. |
| `addressLine2` | `string` | `Required` | Mapped address line2 value. |
| `cityName` | `string` | `Required` | Display name for city. |
| `pincode` | `string` | `Required` | Code value for pin. |
| `installationType` | `string` | `Required` | Mapped installation type value. |
| `numberOfUnits` | `integer(int32)` | `Required` | Mapped number of units value. |
| `siteNotes` | `string` | `Required` | Mapped site notes value. |
| `installationStatus` | `string` | `Required` | Current status value for installation. |
| `approvalStatus` | `string` | `Required` | Current status value for approval. |
| `assignedTechnicianId` | `integer(int64)?` | `Optional` | Unique assigned technician identifier. |
| `assignedTechnicianName` | `string?` | `Optional` | Display name for assigned technician. |
| `surveyDateUtc` | `datetime?` | `Optional` | UTC timestamp for survey date. |
| `proposalApprovedDateUtc` | `datetime?` | `Optional` | UTC timestamp for proposal approved date. |
| `scheduledInstallationDateUtc` | `datetime?` | `Optional` | UTC timestamp for scheduled installation date. |
| `installationStartedDateUtc` | `datetime?` | `Optional` | UTC timestamp for installation started date. |
| `installationCompletedDateUtc` | `datetime?` | `Optional` | UTC timestamp for installation completed date. |
| `commissionedDateUtc` | `datetime?` | `Optional` | UTC timestamp for commissioned date. |
| `surveys` | `InstallationSurveyResponse[]` | `Required` | Collection of `InstallationSurveyResponse` items. |
| `proposals` | `InstallationProposalResponse[]` | `Required` | Collection of `InstallationProposalResponse` items. |
| `checklistItems` | `InstallationChecklistItemResponse[]` | `Required` | Collection of `InstallationChecklistItemResponse` items. |
| `orders` | `InstallationExecutionOrderResponse[]` | `Required` | Collection of `InstallationExecutionOrderResponse` items. |
| `commissioningCertificates` | `InstallationCommissioningResponse[]` | `Required` | Collection of `InstallationCommissioningResponse` items. |
| `statusTimeline` | `InstallationStatusHistoryResponse[]` | `Required` | Collection of `InstallationStatusHistoryResponse` items. |

### Schema: InstallationListItemResponse
<a id="schema-installationlistitemresponse"></a>

- Source file: `Backend/Coolzo.Contracts/Responses/GapPhaseC/InstallationLifecycleResponses.cs`
- Shape: `record-positional`

| Field | Wire Type | Required | Description |
| --- | --- | --- | --- |
| `installationId` | `integer(int64)` | `Required` | Unique installation identifier. |
| `installationNumber` | `string` | `Required` | Business number/reference for installation. |
| `customerName` | `string` | `Required` | Display name for customer. |
| `mobileNumber` | `string` | `Required` | Business number/reference for mobile. |
| `addressSummary` | `string` | `Required` | Summary text for address. |
| `installationType` | `string` | `Required` | Mapped installation type value. |
| `numberOfUnits` | `integer(int32)` | `Required` | Mapped number of units value. |
| `installationStatus` | `string` | `Required` | Current status value for installation. |
| `approvalStatus` | `string` | `Required` | Current status value for approval. |
| `surveyDateUtc` | `datetime?` | `Optional` | UTC timestamp for survey date. |
| `scheduledInstallationDateUtc` | `datetime?` | `Optional` | UTC timestamp for scheduled installation date. |
| `assignedTechnicianName` | `string?` | `Optional` | Display name for assigned technician. |
| `proposalNumber` | `string?` | `Optional` | Business number/reference for proposal. |
| `proposalTotalAmount` | `number(decimal)?` | `Optional` | Monetary amount for proposal total. |
| `installationOrderNumber` | `string?` | `Optional` | Business number/reference for installation order. |

### Schema: InstallationOrderResponse
<a id="schema-installationorderresponse"></a>

- Source file: `Backend/Coolzo.Contracts/Responses/GapPhaseA/GapPhaseAResponses.cs`
- Shape: `record-positional`

| Field | Wire Type | Required | Description |
| --- | --- | --- | --- |
| `installationOrderId` | `integer(int64)` | `Required` | Unique installation order identifier. |
| `installationOrderNumber` | `string` | `Required` | Business number/reference for installation order. |
| `currentStatus` | `string` | `Required` | Current status value for current. |
| `customerId` | `integer(int64)` | `Required` | Unique customer identifier. |
| `serviceRequestId` | `integer(int64)?` | `Optional` | Unique service request identifier. |
| `scheduledInstallationDateUtc` | `datetime?` | `Optional` | UTC timestamp for scheduled installation date. |
| `surveyReportCount` | `integer(int32)` | `Required` | Mapped survey report count value. |

### Schema: InstallationSummaryResponse
<a id="schema-installationsummaryresponse"></a>

- Source file: `Backend/Coolzo.Contracts/Responses/GapPhaseC/InstallationLifecycleResponses.cs`
- Shape: `record-positional`

| Field | Wire Type | Required | Description |
| --- | --- | --- | --- |
| `installationId` | `integer(int64)` | `Required` | Unique installation identifier. |
| `installationNumber` | `string` | `Required` | Business number/reference for installation. |
| `leadId` | `integer(int64)?` | `Optional` | Unique lead identifier. |
| `installationStatus` | `string` | `Required` | Current status value for installation. |
| `approvalStatus` | `string` | `Required` | Current status value for approval. |
| `customerName` | `string` | `Required` | Display name for customer. |
| `mobileNumber` | `string` | `Required` | Business number/reference for mobile. |
| `installationType` | `string` | `Required` | Mapped installation type value. |
| `numberOfUnits` | `integer(int32)` | `Required` | Mapped number of units value. |
| `surveyDateUtc` | `datetime?` | `Optional` | UTC timestamp for survey date. |
| `scheduledInstallationDateUtc` | `datetime?` | `Optional` | UTC timestamp for scheduled installation date. |
| `proposalNumber` | `string?` | `Optional` | Business number/reference for proposal. |
| `proposalTotalAmount` | `number(decimal)?` | `Optional` | Monetary amount for proposal total. |
| `installationOrderNumber` | `string?` | `Optional` | Business number/reference for installation order. |
| `warrantyRegistrationNumber` | `string?` | `Optional` | Business number/reference for warranty registration. |

### Schema: InventoryAnalyticsResponse
<a id="schema-inventoryanalyticsresponse"></a>

- Source file: `Backend/Coolzo.Contracts/Responses/Analytics/AnalyticsResponses.cs`
- Shape: `record-positional`

| Field | Wire Type | Required | Description |
| --- | --- | --- | --- |
| `totalItems` | `integer(int64)` | `Required` | Mapped total items value. |
| `lowStockItems` | `integer(int64)` | `Required` | Mapped low stock items value. |
| `totalOnHandQuantity` | `number(decimal)` | `Required` | Mapped total on hand quantity value. |
| `consumedQuantity` | `number(decimal)` | `Required` | Mapped consumed quantity value. |
| `lowStockSummaries` | `LowStockInventoryItemResponse[]` | `Required` | Collection of `LowStockInventoryItemResponse` items. |
| `consumptionTrends` | `InventoryConsumptionTrendPointResponse[]` | `Required` | Collection of `InventoryConsumptionTrendPointResponse` items. |

### Schema: InvoiceDetailResponse
<a id="schema-invoicedetailresponse"></a>

- Source file: `Backend/Coolzo.Contracts/Responses/Billing/InvoiceDetailResponse.cs`
- Shape: `record-positional`

| Field | Wire Type | Required | Description |
| --- | --- | --- | --- |
| `invoiceId` | `integer(int64)` | `Required` | Unique invoice identifier. |
| `invoiceNumber` | `string` | `Required` | Business number/reference for invoice. |
| `quotationId` | `integer(int64)` | `Required` | Unique quotation identifier. |
| `quotationNumber` | `string` | `Required` | Business number/reference for quotation. |
| `customerId` | `integer(int64)` | `Required` | Unique customer identifier. |
| `customerName` | `string` | `Required` | Display name for customer. |
| `mobileNumber` | `string` | `Required` | Business number/reference for mobile. |
| `addressSummary` | `string` | `Required` | Summary text for address. |
| `serviceName` | `string` | `Required` | Display name for service. |
| `currentStatus` | `string` | `Required` | Current status value for current. |
| `invoiceDateUtc` | `datetime` | `Required` | UTC timestamp for invoice date. |
| `subTotalAmount` | `number(decimal)` | `Required` | Monetary amount for sub total. |
| `discountAmount` | `number(decimal)` | `Required` | Monetary amount for discount. |
| `taxPercentage` | `number(decimal)` | `Required` | Mapped tax percentage value. |
| `taxAmount` | `number(decimal)` | `Required` | Monetary amount for tax. |
| `grandTotalAmount` | `number(decimal)` | `Required` | Monetary amount for grand total. |
| `paidAmount` | `number(decimal)` | `Required` | Monetary amount for paid. |
| `balanceAmount` | `number(decimal)` | `Required` | Monetary amount for balance. |
| `lastPaymentDateUtc` | `datetime?` | `Optional` | UTC timestamp for last payment date. |
| `lines` | `InvoiceLineResponse[]` | `Required` | Collection of `InvoiceLineResponse` items. |
| `payments` | `PaymentTransactionResponse[]` | `Required` | Collection of `PaymentTransactionResponse` items. |
| `billingHistory` | `BillingStatusHistoryResponse[]` | `Required` | Collection of `BillingStatusHistoryResponse` items. |

### Schema: InvoiceListItemResponse
<a id="schema-invoicelistitemresponse"></a>

- Source file: `Backend/Coolzo.Contracts/Responses/Billing/InvoiceListItemResponse.cs`
- Shape: `record-positional`

| Field | Wire Type | Required | Description |
| --- | --- | --- | --- |
| `invoiceId` | `integer(int64)` | `Required` | Unique invoice identifier. |
| `invoiceNumber` | `string` | `Required` | Business number/reference for invoice. |
| `quotationId` | `integer(int64)` | `Required` | Unique quotation identifier. |
| `quotationNumber` | `string` | `Required` | Business number/reference for quotation. |
| `customerName` | `string` | `Required` | Display name for customer. |
| `currentStatus` | `string` | `Required` | Current status value for current. |
| `grandTotalAmount` | `number(decimal)` | `Required` | Monetary amount for grand total. |
| `paidAmount` | `number(decimal)` | `Required` | Monetary amount for paid. |
| `balanceAmount` | `number(decimal)` | `Required` | Monetary amount for balance. |
| `invoiceDateUtc` | `datetime` | `Required` | UTC timestamp for invoice date. |

### Schema: ItemResponse
<a id="schema-itemresponse"></a>

- Source file: `Backend/Coolzo.Contracts/Responses/Inventory/ItemResponse.cs`
- Shape: `record-positional`

| Field | Wire Type | Required | Description |
| --- | --- | --- | --- |
| `itemId` | `integer(int64)` | `Required` | Unique item identifier. |
| `itemCode` | `string` | `Required` | Code value for item. |
| `itemName` | `string` | `Required` | Display name for item. |
| `categoryCode` | `string` | `Required` | Code value for category. |
| `categoryName` | `string` | `Required` | Display name for category. |
| `unitOfMeasureCode` | `string` | `Required` | Code value for unit of measure. |
| `unitOfMeasureName` | `string` | `Required` | Display name for unit of measure. |
| `supplierId` | `integer(int64)?` | `Optional` | Unique supplier identifier. |
| `supplierCode` | `string?` | `Optional` | Code value for supplier. |
| `supplierName` | `string?` | `Optional` | Display name for supplier. |
| `itemDescription` | `string` | `Required` | Mapped item description value. |
| `purchasePrice` | `number(decimal)` | `Required` | Mapped purchase price value. |
| `sellingPrice` | `number(decimal)` | `Required` | Mapped selling price value. |
| `taxPercentage` | `number(decimal)` | `Required` | Mapped tax percentage value. |
| `warrantyDays` | `integer(int32)` | `Required` | Mapped warranty days value. |
| `reorderLevel` | `number(decimal)` | `Required` | Mapped reorder level value. |
| `isActive` | `boolean` | `Required` | Boolean flag indicating whether active is true. |

### Schema: JobAttachmentResponse
<a id="schema-jobattachmentresponse"></a>

- Source file: `Backend/Coolzo.Contracts/Responses/FieldExecution/JobAttachmentResponse.cs`
- Shape: `record-positional`

| Field | Wire Type | Required | Description |
| --- | --- | --- | --- |
| `jobAttachmentId` | `integer(int64)` | `Required` | Unique job attachment identifier. |
| `attachmentType` | `string` | `Required` | Mapped attachment type value. |
| `fileName` | `string` | `Required` | Display name for file. |
| `contentType` | `string` | `Required` | Mapped content type value. |
| `fileSizeInBytes` | `integer(int64)` | `Required` | Mapped file size in bytes value. |
| `fileUrl` | `string` | `Required` | Mapped file url value. |
| `attachmentRemarks` | `string` | `Required` | Free-text remarks for attachment. |
| `uploadedDateUtc` | `datetime` | `Required` | UTC timestamp for uploaded date. |

### Schema: JobChecklistItemResponse
<a id="schema-jobchecklistitemresponse"></a>

- Source file: `Backend/Coolzo.Contracts/Responses/FieldExecution/JobChecklistItemResponse.cs`
- Shape: `record-positional`

| Field | Wire Type | Required | Description |
| --- | --- | --- | --- |
| `serviceChecklistMasterId` | `integer(int64)` | `Required` | Unique service checklist master identifier. |
| `checklistTitle` | `string` | `Required` | Mapped checklist title value. |
| `checklistDescription` | `string` | `Required` | Mapped checklist description value. |
| `isMandatory` | `boolean` | `Required` | Boolean flag indicating whether mandatory is true. |
| `isChecked` | `boolean?` | `Optional` | Boolean flag indicating whether checked is true. |
| `responseRemarks` | `string` | `Required` | Free-text remarks for response. |
| `responseDateUtc` | `datetime?` | `Optional` | UTC timestamp for response date. |

### Schema: JobDiagnosisSummaryResponse
<a id="schema-jobdiagnosissummaryresponse"></a>

- Source file: `Backend/Coolzo.Contracts/Responses/FieldExecution/JobDiagnosisSummaryResponse.cs`
- Shape: `record-positional`

| Field | Wire Type | Required | Description |
| --- | --- | --- | --- |
| `jobDiagnosisId` | `integer(int64)?` | `Optional` | Unique job diagnosis identifier. |
| `complaintIssueMasterId` | `integer(int64)?` | `Optional` | Unique complaint issue master identifier. |
| `complaintIssueName` | `string?` | `Optional` | Display name for complaint issue. |
| `diagnosisResultMasterId` | `integer(int64)?` | `Optional` | Unique diagnosis result master identifier. |
| `diagnosisResultName` | `string?` | `Optional` | Display name for diagnosis result. |
| `diagnosisRemarks` | `string?` | `Optional` | Free-text remarks for diagnosis. |
| `diagnosisDateUtc` | `datetime?` | `Optional` | UTC timestamp for diagnosis date. |

### Schema: JobExecutionNoteResponse
<a id="schema-jobexecutionnoteresponse"></a>

- Source file: `Backend/Coolzo.Contracts/Responses/FieldExecution/JobExecutionNoteResponse.cs`
- Shape: `record-positional`

| Field | Wire Type | Required | Description |
| --- | --- | --- | --- |
| `jobExecutionNoteId` | `integer(int64)` | `Required` | Unique job execution note identifier. |
| `noteText` | `string` | `Required` | Mapped note text value. |
| `isCustomerVisible` | `boolean` | `Required` | Boolean flag indicating whether customer visible is true. |
| `createdBy` | `string` | `Required` | Mapped created by value. |
| `noteDateUtc` | `datetime` | `Required` | UTC timestamp for note date. |

### Schema: JobExecutionTimelineItemResponse
<a id="schema-jobexecutiontimelineitemresponse"></a>

- Source file: `Backend/Coolzo.Contracts/Responses/FieldExecution/JobExecutionTimelineItemResponse.cs`
- Shape: `record-positional`

| Field | Wire Type | Required | Description |
| --- | --- | --- | --- |
| `eventType` | `string` | `Required` | Mapped event type value. |
| `eventTitle` | `string` | `Required` | Mapped event title value. |
| `status` | `string` | `Required` | Current status value for the record. |
| `remarks` | `string` | `Required` | Free-text remarks for the action. |
| `eventDateUtc` | `datetime` | `Required` | UTC timestamp for event date. |

### Schema: JobPartConsumptionSummaryResponse
<a id="schema-jobpartconsumptionsummaryresponse"></a>

- Source file: `Backend/Coolzo.Contracts/Responses/Inventory/JobPartConsumptionSummaryResponse.cs`
- Shape: `record-positional`

| Field | Wire Type | Required | Description |
| --- | --- | --- | --- |
| `jobCardId` | `integer(int64)` | `Required` | Unique job card identifier. |
| `jobCardNumber` | `string` | `Required` | Business number/reference for job card. |
| `technicianId` | `integer(int64)?` | `Optional` | Unique technician identifier. |
| `technicianName` | `string?` | `Optional` | Display name for technician. |
| `totalLines` | `integer(int32)` | `Required` | Mapped total lines value. |
| `totalQuantityUsed` | `number(decimal)` | `Required` | Mapped total quantity used value. |
| `totalAmount` | `number(decimal)` | `Required` | Monetary amount for total. |
| `items` | `JobPartConsumptionResponse[]` | `Required` | Collection of `JobPartConsumptionResponse` items. |

### Schema: LeadAnalyticsResponse
<a id="schema-leadanalyticsresponse"></a>

- Source file: `Backend/Coolzo.Contracts/Responses/GapPhaseA/GapPhaseAResponses.cs`
- Shape: `record-positional`

| Field | Wire Type | Required | Description |
| --- | --- | --- | --- |
| `fromDate` | `date` | `Required` | Date value for from. |
| `toDate` | `date` | `Required` | Date value for to. |
| `totalLeads` | `integer(int32)` | `Required` | Mapped total leads value. |
| `contactedLeads` | `integer(int32)` | `Required` | Mapped contacted leads value. |
| `qualifiedLeads` | `integer(int32)` | `Required` | Mapped qualified leads value. |
| `convertedLeads` | `integer(int32)` | `Required` | Mapped converted leads value. |
| `lostLeads` | `integer(int32)` | `Required` | Mapped lost leads value. |
| `closedLeads` | `integer(int32)` | `Required` | Mapped closed leads value. |
| `conversionRate` | `number(decimal)` | `Required` | Mapped conversion rate value. |
| `leadsBySource` | `LeadSourceAnalyticsResponse[]` | `Required` | Collection of `LeadSourceAnalyticsResponse` items. |
| `dailyLeadCount` | `LeadDailyCountResponse[]` | `Required` | Collection of `LeadDailyCountResponse` items. |

### Schema: LeadDetailResponse
<a id="schema-leaddetailresponse"></a>

- Source file: `Backend/Coolzo.Contracts/Responses/GapPhaseA/GapPhaseAResponses.cs`
- Shape: `record-positional`

| Field | Wire Type | Required | Description |
| --- | --- | --- | --- |
| `leadId` | `integer(int64)` | `Required` | Unique lead identifier. |
| `leadNumber` | `string` | `Required` | Business number/reference for lead. |
| `customerName` | `string` | `Required` | Display name for customer. |
| `mobileNumber` | `string` | `Required` | Business number/reference for mobile. |
| `emailAddress` | `string?` | `Optional` | Mapped email address value. |
| `sourceChannel` | `string` | `Required` | Mapped source channel value. |
| `leadStatus` | `string` | `Required` | Current status value for lead. |
| `assignedUserId` | `integer(int64)?` | `Optional` | Unique assigned user identifier. |
| `assignedUserName` | `string?` | `Optional` | Display name for assigned user. |
| `serviceId` | `integer(int64)?` | `Optional` | Unique service identifier. |
| `acTypeId` | `integer(int64)?` | `Optional` | Unique ac type identifier. |
| `tonnageId` | `integer(int64)?` | `Optional` | Unique tonnage identifier. |
| `brandId` | `integer(int64)?` | `Optional` | Unique brand identifier. |
| `slotAvailabilityId` | `integer(int64)?` | `Optional` | Unique slot availability identifier. |
| `convertedBookingId` | `integer(int64)?` | `Optional` | Unique converted booking identifier. |
| `convertedServiceRequestId` | `integer(int64)?` | `Optional` | Unique converted service request identifier. |
| `addressLine1` | `string?` | `Optional` | Mapped address line1 value. |
| `addressLine2` | `string?` | `Optional` | Mapped address line2 value. |
| `cityName` | `string?` | `Optional` | Display name for city. |
| `pincode` | `string?` | `Optional` | Code value for pin. |
| `inquiryNotes` | `string?` | `Optional` | Mapped inquiry notes value. |
| `lostReason` | `string?` | `Optional` | Mapped lost reason value. |
| `dateCreated` | `datetime` | `Required` | Mapped date created value. |
| `lastContactedDateUtc` | `datetime?` | `Optional` | UTC timestamp for last contacted date. |
| `convertedDateUtc` | `datetime?` | `Optional` | UTC timestamp for converted date. |
| `closedDateUtc` | `datetime?` | `Optional` | UTC timestamp for closed date. |
| `statusTimeline` | `LeadStatusHistoryResponse[]` | `Required` | Collection of `LeadStatusHistoryResponse` items. |
| `assignments` | `LeadAssignmentResponse[]` | `Required` | Collection of `LeadAssignmentResponse` items. |
| `notes` | `LeadNoteResponse[]` | `Required` | Collection of `LeadNoteResponse` items. |
| `conversions` | `LeadConversionResponse[]` | `Required` | Collection of `LeadConversionResponse` items. |

### Schema: LeadListItemResponse
<a id="schema-leadlistitemresponse"></a>

- Source file: `Backend/Coolzo.Contracts/Responses/GapPhaseA/GapPhaseAResponses.cs`
- Shape: `record-positional`

| Field | Wire Type | Required | Description |
| --- | --- | --- | --- |
| `leadId` | `integer(int64)` | `Required` | Unique lead identifier. |
| `leadNumber` | `string` | `Required` | Business number/reference for lead. |
| `customerName` | `string` | `Required` | Display name for customer. |
| `mobileNumber` | `string` | `Required` | Business number/reference for mobile. |
| `emailAddress` | `string?` | `Optional` | Mapped email address value. |
| `sourceChannel` | `string` | `Required` | Mapped source channel value. |
| `leadStatus` | `string` | `Required` | Current status value for lead. |
| `assignedUserId` | `integer(int64)?` | `Optional` | Unique assigned user identifier. |
| `assignedUserName` | `string?` | `Optional` | Display name for assigned user. |
| `dateCreated` | `datetime` | `Required` | Mapped date created value. |
| `lastContactedDateUtc` | `datetime?` | `Optional` | UTC timestamp for last contacted date. |
| `convertedDateUtc` | `datetime?` | `Optional` | UTC timestamp for converted date. |
| `lostReason` | `string?` | `Optional` | Mapped lost reason value. |

### Schema: LeadResponse
<a id="schema-leadresponse"></a>

- Source file: `Backend/Coolzo.Contracts/Responses/GapPhaseA/GapPhaseAResponses.cs`
- Shape: `record-positional`

| Field | Wire Type | Required | Description |
| --- | --- | --- | --- |
| `leadId` | `integer(int64)` | `Required` | Unique lead identifier. |
| `leadNumber` | `string` | `Required` | Business number/reference for lead. |
| `customerName` | `string` | `Required` | Display name for customer. |
| `mobileNumber` | `string` | `Required` | Business number/reference for mobile. |
| `leadStatus` | `string` | `Required` | Current status value for lead. |
| `convertedBookingId` | `integer(int64)?` | `Optional` | Unique converted booking identifier. |
| `convertedServiceRequestId` | `integer(int64)?` | `Optional` | Unique converted service request identifier. |

### Schema: LoginRequest
<a id="schema-loginrequest"></a>

- Source file: `Backend/Coolzo.Contracts/Requests/Auth/LoginRequest.cs`
- Shape: `record-positional`

| Field | Wire Type | Required | Description |
| --- | --- | --- | --- |
| `userNameOrEmail` | `string` | `Required` | Mapped user name or email value. |
| `password` | `string` | `Required` | Mapped password value. |

### Schema: LookupItemResponse
<a id="schema-lookupitemresponse"></a>

- Source file: `Backend/Coolzo.Contracts/Common/LookupItemResponse.cs`
- Shape: `record-positional`

| Field | Wire Type | Required | Description |
| --- | --- | --- | --- |
| `value` | `integer(int64)` | `Required` | Mapped value value. |
| `label` | `string` | `Required` | Mapped label value. |

### Schema: MarkCustomerAbsentRequest
<a id="schema-markcustomerabsentrequest"></a>

- Source file: `Backend/Coolzo.Contracts/Requests/GapPhaseD/CancellationRefundRequests.cs`
- Shape: `record-positional`

| Field | Wire Type | Required | Description |
| --- | --- | --- | --- |
| `absentReasonCode` | `string` | `Required` | Code value for absent reason. |
| `absentReasonText` | `string` | `Required` | Mapped absent reason text value. |
| `attemptCount` | `integer(int32)` | `Required` | Mapped attempt count value. |
| `contactAttemptLog` | `string` | `Required` | Mapped contact attempt log value. |

### Schema: NotificationTemplateResponse
<a id="schema-notificationtemplateresponse"></a>

- Source file: `Backend/Coolzo.Contracts/Responses/Admin/AdminResponses.cs`
- Shape: `record-positional`

| Field | Wire Type | Required | Description |
| --- | --- | --- | --- |
| `notificationTemplateId` | `integer(int64)` | `Required` | Unique notification template identifier. |
| `templateCode` | `string` | `Required` | Code value for template. |
| `templateName` | `string` | `Required` | Display name for template. |
| `triggerCode` | `string` | `Required` | Code value for trigger. |
| `channel` | `string` | `Required` | Mapped channel value. |
| `subjectTemplate` | `string` | `Required` | Mapped subject template value. |
| `bodyTemplate` | `string` | `Required` | Mapped body template value. |
| `allowedMergeTags` | `string[]` | `Required` | Collection of `string` items. |
| `isActive` | `boolean` | `Required` | Boolean flag indicating whether active is true. |
| `dateCreated` | `datetime` | `Required` | Mapped date created value. |
| `lastUpdated` | `datetime?` | `Optional` | Mapped last updated value. |

### Schema: NotificationTemplateUpsertRequest
<a id="schema-notificationtemplateupsertrequest"></a>

- Source file: `Backend/Coolzo.Contracts/Requests/Admin/AdminRequests.cs`
- Shape: `record-positional`

| Field | Wire Type | Required | Description |
| --- | --- | --- | --- |
| `templateCode` | `string` | `Required` | Code value for template. |
| `templateName` | `string` | `Required` | Display name for template. |
| `triggerCode` | `string` | `Required` | Code value for trigger. |
| `channel` | `string` | `Required` | Mapped channel value. |
| `subjectTemplate` | `string?` | `Optional` | Mapped subject template value. |
| `bodyTemplate` | `string` | `Required` | Mapped body template value. |
| `allowedMergeTags` | `string[]` | `Required` | Collection of `string` items. |
| `isActive` | `boolean` | `Required` | Boolean flag indicating whether active is true. |

### Schema: NotificationTriggerConfigurationResponse
<a id="schema-notificationtriggerconfigurationresponse"></a>

- Source file: `Backend/Coolzo.Contracts/Responses/Admin/AdminResponses.cs`
- Shape: `record-positional`

| Field | Wire Type | Required | Description |
| --- | --- | --- | --- |
| `notificationTriggerConfigurationId` | `integer(int64)` | `Required` | Unique notification trigger configuration identifier. |
| `triggerCode` | `string` | `Required` | Code value for trigger. |
| `triggerName` | `string` | `Required` | Display name for trigger. |
| `description` | `string` | `Required` | Mapped description value. |
| `isEnabled` | `boolean` | `Required` | Boolean flag indicating whether enabled is true. |
| `emailEnabled` | `boolean` | `Required` | Mapped email enabled value. |
| `smsEnabled` | `boolean` | `Required` | Mapped sms enabled value. |
| `whatsAppEnabled` | `boolean` | `Required` | Mapped whats app enabled value. |
| `pushEnabled` | `boolean` | `Required` | Mapped push enabled value. |
| `reminderLeadMinutes` | `integer(int32)` | `Required` | Mapped reminder lead minutes value. |
| `delayMinutes` | `integer(int32)` | `Required` | Mapped delay minutes value. |
| `dateCreated` | `datetime` | `Required` | Mapped date created value. |
| `lastUpdated` | `datetime?` | `Optional` | Mapped last updated value. |

### Schema: NotificationTriggerUpsertRequest
<a id="schema-notificationtriggerupsertrequest"></a>

- Source file: `Backend/Coolzo.Contracts/Requests/Admin/AdminRequests.cs`
- Shape: `record-positional`

| Field | Wire Type | Required | Description |
| --- | --- | --- | --- |
| `triggerCode` | `string` | `Required` | Code value for trigger. |
| `triggerName` | `string` | `Required` | Display name for trigger. |
| `description` | `string?` | `Optional` | Mapped description value. |
| `isEnabled` | `boolean` | `Required` | Boolean flag indicating whether enabled is true. |
| `emailEnabled` | `boolean` | `Required` | Mapped email enabled value. |
| `smsEnabled` | `boolean` | `Required` | Mapped sms enabled value. |
| `whatsAppEnabled` | `boolean` | `Required` | Mapped whats app enabled value. |
| `pushEnabled` | `boolean` | `Required` | Mapped push enabled value. |
| `reminderLeadMinutes` | `integer(int32)` | `Required` | Mapped reminder lead minutes value. |
| `delayMinutes` | `integer(int32)` | `Required` | Mapped delay minutes value. |

### Schema: OperationsDashboardSummaryResponse
<a id="schema-operationsdashboardsummaryresponse"></a>

- Source file: `Backend/Coolzo.Contracts/Responses/Operations/OperationsDashboardSummaryResponse.cs`
- Shape: `record-positional`

| Field | Wire Type | Required | Description |
| --- | --- | --- | --- |
| `totalBookings` | `integer(int32)` | `Required` | Mapped total bookings value. |
| `totalServiceRequests` | `integer(int32)` | `Required` | Mapped total service requests value. |
| `assignedServiceRequests` | `integer(int32)` | `Required` | Mapped assigned service requests value. |
| `unassignedServiceRequests` | `integer(int32)` | `Required` | Mapped unassigned service requests value. |
| `enRouteCount` | `integer(int32)` | `Required` | Mapped en route count value. |
| `reachedCount` | `integer(int32)` | `Required` | Mapped reached count value. |
| `workStartedCount` | `integer(int32)` | `Required` | Mapped work started count value. |
| `workInProgressCount` | `integer(int32)` | `Required` | Mapped work in progress count value. |
| `submittedForClosureCount` | `integer(int32)` | `Required` | Mapped submitted for closure count value. |
| `activeTechnicianCount` | `integer(int32)` | `Required` | Mapped active technician count value. |
| `technicianMonitoring` | `TechnicianMonitoringItemResponse[]` | `Required` | Collection of `TechnicianMonitoringItemResponse` items. |

### Schema: PartsReturnResponse
<a id="schema-partsreturnresponse"></a>

- Source file: `Backend/Coolzo.Contracts/Responses/GapPhaseA/GapPhaseAResponses.cs`
- Shape: `record-positional`

| Field | Wire Type | Required | Description |
| --- | --- | --- | --- |
| `partsReturnId` | `integer(int64)` | `Required` | Unique parts return identifier. |
| `partsReturnNumber` | `string` | `Required` | Business number/reference for parts return. |
| `partsReturnStatus` | `string` | `Required` | Current status value for parts return. |
| `quantity` | `number(decimal)` | `Required` | Mapped quantity value. |
| `supplierClaimReference` | `string` | `Required` | Reference value for supplier claim. |

### Schema: PaymentTransactionResponse
<a id="schema-paymenttransactionresponse"></a>

- Source file: `Backend/Coolzo.Contracts/Responses/Billing/PaymentTransactionResponse.cs`
- Shape: `record-positional`

| Field | Wire Type | Required | Description |
| --- | --- | --- | --- |
| `paymentTransactionId` | `integer(int64)` | `Required` | Unique payment transaction identifier. |
| `invoiceId` | `integer(int64)` | `Required` | Unique invoice identifier. |
| `paymentMethod` | `string` | `Required` | Mapped payment method value. |
| `referenceNumber` | `string` | `Required` | Business number/reference for reference. |
| `paidAmount` | `number(decimal)` | `Required` | Monetary amount for paid. |
| `paymentDateUtc` | `datetime` | `Required` | UTC timestamp for payment date. |
| `transactionRemarks` | `string` | `Required` | Free-text remarks for transaction. |
| `receipt` | `PaymentReceiptResponse?` | `Optional` | Mapped receipt value. |

### Schema: PermissionResponse
<a id="schema-permissionresponse"></a>

- Source file: `Backend/Coolzo.Contracts/Responses/Permission/PermissionResponse.cs`
- Shape: `record-positional`

| Field | Wire Type | Required | Description |
| --- | --- | --- | --- |
| `permissionId` | `integer(int64)` | `Required` | Unique permission identifier. |
| `permissionName` | `string` | `Required` | Display name for permission. |
| `displayName` | `string` | `Required` | Display name for display. |
| `moduleName` | `string` | `Required` | Display name for module. |
| `actionName` | `string` | `Required` | Display name for action. |
| `isActive` | `boolean` | `Required` | Boolean flag indicating whether active is true. |

### Schema: PublicHomeCMSContentResponse
<a id="schema-publichomecmscontentresponse"></a>

- Source file: `Backend/Coolzo.Contracts/Responses/Admin/AdminResponses.cs`
- Shape: `record-positional`

| Field | Wire Type | Required | Description |
| --- | --- | --- | --- |
| `blocks` | `CMSBlockResponse[]` | `Required` | Collection of `CMSBlockResponse` items. |
| `banners` | `CMSBannerResponse[]` | `Required` | Collection of `CMSBannerResponse` items. |
| `faqs` | `CMSFaqResponse[]` | `Required` | Collection of `CMSFaqResponse` items. |
| `displaySettings` | `DisplayContentSettingResponse[]` | `Required` | Collection of `DisplayContentSettingResponse` items. |

### Schema: QuotationDecisionRequest
<a id="schema-quotationdecisionrequest"></a>

- Source file: `Backend/Coolzo.Contracts/Requests/Billing/QuotationDecisionRequest.cs`
- Shape: `record-positional`

| Field | Wire Type | Required | Description |
| --- | --- | --- | --- |
| `remarks` | `string?` | `Optional` | Free-text remarks for the action. |

### Schema: QuotationDetailResponse
<a id="schema-quotationdetailresponse"></a>

- Source file: `Backend/Coolzo.Contracts/Responses/Billing/QuotationDetailResponse.cs`
- Shape: `record-positional`

| Field | Wire Type | Required | Description |
| --- | --- | --- | --- |
| `quotationId` | `integer(int64)` | `Required` | Unique quotation identifier. |
| `quotationNumber` | `string` | `Required` | Business number/reference for quotation. |
| `jobCardId` | `integer(int64)` | `Required` | Unique job card identifier. |
| `jobCardNumber` | `string` | `Required` | Business number/reference for job card. |
| `serviceRequestId` | `integer(int64)` | `Required` | Unique service request identifier. |
| `serviceRequestNumber` | `string` | `Required` | Business number/reference for service request. |
| `bookingId` | `integer(int64)` | `Required` | Unique booking identifier. |
| `bookingReference` | `string` | `Required` | Reference value for booking. |
| `customerId` | `integer(int64)` | `Required` | Unique customer identifier. |
| `customerName` | `string` | `Required` | Display name for customer. |
| `mobileNumber` | `string` | `Required` | Business number/reference for mobile. |
| `addressSummary` | `string` | `Required` | Summary text for address. |
| `serviceName` | `string` | `Required` | Display name for service. |
| `currentStatus` | `string` | `Required` | Current status value for current. |
| `quotationDateUtc` | `datetime` | `Required` | UTC timestamp for quotation date. |
| `subTotalAmount` | `number(decimal)` | `Required` | Monetary amount for sub total. |
| `discountAmount` | `number(decimal)` | `Required` | Monetary amount for discount. |
| `taxPercentage` | `number(decimal)` | `Required` | Mapped tax percentage value. |
| `taxAmount` | `number(decimal)` | `Required` | Monetary amount for tax. |
| `grandTotalAmount` | `number(decimal)` | `Required` | Monetary amount for grand total. |
| `customerDecisionRemarks` | `string` | `Required` | Free-text remarks for customer decision. |
| `approvedDateUtc` | `datetime?` | `Optional` | UTC timestamp for approved date. |
| `rejectedDateUtc` | `datetime?` | `Optional` | UTC timestamp for rejected date. |
| `invoiceId` | `integer(int64)?` | `Optional` | Unique invoice identifier. |
| `invoiceNumber` | `string?` | `Optional` | Business number/reference for invoice. |
| `invoiceStatus` | `string?` | `Optional` | Current status value for invoice. |
| `lines` | `QuotationLineResponse[]` | `Required` | Collection of `QuotationLineResponse` items. |
| `billingHistory` | `BillingStatusHistoryResponse[]` | `Required` | Collection of `BillingStatusHistoryResponse` items. |

### Schema: QuotationListItemResponse
<a id="schema-quotationlistitemresponse"></a>

- Source file: `Backend/Coolzo.Contracts/Responses/Billing/QuotationListItemResponse.cs`
- Shape: `record-positional`

| Field | Wire Type | Required | Description |
| --- | --- | --- | --- |
| `quotationId` | `integer(int64)` | `Required` | Unique quotation identifier. |
| `quotationNumber` | `string` | `Required` | Business number/reference for quotation. |
| `jobCardId` | `integer(int64)` | `Required` | Unique job card identifier. |
| `jobCardNumber` | `string` | `Required` | Business number/reference for job card. |
| `serviceRequestId` | `integer(int64)` | `Required` | Unique service request identifier. |
| `serviceRequestNumber` | `string` | `Required` | Business number/reference for service request. |
| `customerName` | `string` | `Required` | Display name for customer. |
| `currentStatus` | `string` | `Required` | Current status value for current. |
| `grandTotalAmount` | `number(decimal)` | `Required` | Monetary amount for grand total. |
| `quotationDateUtc` | `datetime` | `Required` | UTC timestamp for quotation date. |

### Schema: ReassignTechnicianRequest
<a id="schema-reassigntechnicianrequest"></a>

- Source file: `Backend/Coolzo.Contracts/Requests/Operations/ReassignTechnicianRequest.cs`
- Shape: `record-positional`

| Field | Wire Type | Required | Description |
| --- | --- | --- | --- |
| `technicianId` | `integer(int64)` | `Required` | Unique technician identifier. |
| `remarks` | `string?` | `Optional` | Free-text remarks for the action. |

### Schema: RecordPaymentRequest
<a id="schema-recordpaymentrequest"></a>

- Source file: `Backend/Coolzo.Contracts/Requests/Billing/RecordPaymentRequest.cs`
- Shape: `record-positional`

| Field | Wire Type | Required | Description |
| --- | --- | --- | --- |
| `invoiceId` | `integer(int64)` | `Required` | Unique invoice identifier. |
| `paidAmount` | `number(decimal)` | `Required` | Monetary amount for paid. |
| `paymentMethod` | `string` | `Required` | Mapped payment method value. |
| `referenceNumber` | `string?` | `Optional` | Business number/reference for reference. |
| `remarks` | `string?` | `Optional` | Free-text remarks for the action. |
| `idempotencyKey` | `string?` | `Optional` | Mapped idempotency key value. |
| `gatewayTransactionId` | `string?` | `Optional` | Unique gateway transaction identifier. |
| `signature` | `string?` | `Optional` | Mapped signature value. |
| `expectedInvoiceAmount` | `number(decimal)?` | `Optional` | Monetary amount for expected invoice. |
| `isWebhookEvent` | `boolean` | `Required` | Boolean flag indicating whether webhook event is true. |
| `webhookReference` | `string?` | `Optional` | Reference value for webhook. |

### Schema: RecordStockTransactionRequest
<a id="schema-recordstocktransactionrequest"></a>

- Source file: `Backend/Coolzo.Contracts/Requests/Inventory/RecordStockTransactionRequest.cs`
- Shape: `record-positional`

| Field | Wire Type | Required | Description |
| --- | --- | --- | --- |
| `warehouseId` | `integer(int64)` | `Required` | Unique warehouse identifier. |
| `itemId` | `integer(int64)` | `Required` | Unique item identifier. |
| `transactionType` | `string` | `Required` | Mapped transaction type value. |
| `quantity` | `number(decimal)` | `Required` | Mapped quantity value. |
| `unitCost` | `number(decimal)` | `Required` | Mapped unit cost value. |
| `supplierId` | `integer(int64)?` | `Optional` | Unique supplier identifier. |
| `referenceNumber` | `string?` | `Optional` | Business number/reference for reference. |
| `remarks` | `string?` | `Optional` | Free-text remarks for the action. |

### Schema: RefreshTokenRequest
<a id="schema-refreshtokenrequest"></a>

- Source file: `Backend/Coolzo.Contracts/Requests/Auth/RefreshTokenRequest.cs`
- Shape: `record-positional`

| Field | Wire Type | Required | Description |
| --- | --- | --- | --- |
| `accessToken` | `string` | `Required` | Mapped access token value. |
| `refreshToken` | `string` | `Required` | Mapped refresh token value. |

### Schema: RefundDetailResponse
<a id="schema-refunddetailresponse"></a>

- Source file: `Backend/Coolzo.Contracts/Responses/GapPhaseD/CancellationRefundResponses.cs`
- Shape: `record-positional`

| Field | Wire Type | Required | Description |
| --- | --- | --- | --- |
| `refundRequestId` | `integer(int64)` | `Required` | Unique refund request identifier. |
| `refundRequestNo` | `string` | `Required` | Mapped refund request no value. |
| `cancellationRecordId` | `integer(int64)?` | `Optional` | Unique cancellation record identifier. |
| `invoiceId` | `integer(int64)?` | `Optional` | Unique invoice identifier. |
| `paymentTransactionId` | `integer(int64)?` | `Optional` | Unique payment transaction identifier. |
| `refundAmount` | `number(decimal)` | `Required` | Monetary amount for refund. |
| `requestedAmount` | `number(decimal)` | `Required` | Monetary amount for requested. |
| `approvedAmount` | `number(decimal)` | `Required` | Monetary amount for approved. |
| `maxAllowedAmount` | `number(decimal)` | `Required` | Monetary amount for max allowed. |
| `refundMethod` | `string` | `Required` | Mapped refund method value. |
| `refundReason` | `string` | `Required` | Mapped refund reason value. |
| `refundStatus` | `string` | `Required` | Current status value for refund. |
| `approvalRequired` | `boolean` | `Required` | Mapped approval required value. |
| `approvedByUserId` | `integer(int64)?` | `Optional` | Unique approved by user identifier. |
| `approvedDateUtc` | `datetime?` | `Optional` | UTC timestamp for approved date. |
| `processedOn` | `datetime?` | `Optional` | Mapped processed on value. |
| `approvals` | `RefundApprovalHistoryResponse[]` | `Required` | Collection of `RefundApprovalHistoryResponse` items. |
| `statusHistory` | `RefundStatusHistoryResponse[]` | `Required` | Collection of `RefundStatusHistoryResponse` items. |

### Schema: RefundListItemResponse
<a id="schema-refundlistitemresponse"></a>

- Source file: `Backend/Coolzo.Contracts/Responses/GapPhaseD/CancellationRefundResponses.cs`
- Shape: `record-positional`

| Field | Wire Type | Required | Description |
| --- | --- | --- | --- |
| `refundRequestId` | `integer(int64)` | `Required` | Unique refund request identifier. |
| `refundRequestNo` | `string` | `Required` | Mapped refund request no value. |
| `cancellationRecordId` | `integer(int64)?` | `Optional` | Unique cancellation record identifier. |
| `invoiceId` | `integer(int64)?` | `Optional` | Unique invoice identifier. |
| `paymentTransactionId` | `integer(int64)?` | `Optional` | Unique payment transaction identifier. |
| `refundAmount` | `number(decimal)` | `Required` | Monetary amount for refund. |
| `refundMethod` | `string` | `Required` | Mapped refund method value. |
| `refundStatus` | `string` | `Required` | Current status value for refund. |
| `approvalRequired` | `boolean` | `Required` | Mapped approval required value. |
| `dateCreated` | `datetime` | `Required` | Mapped date created value. |
| `processedOn` | `datetime?` | `Optional` | Mapped processed on value. |

### Schema: RefundRequestResponse
<a id="schema-refundrequestresponse"></a>

- Source file: `Backend/Coolzo.Contracts/Responses/GapPhaseA/GapPhaseAResponses.cs`
- Shape: `record-positional`

| Field | Wire Type | Required | Description |
| --- | --- | --- | --- |
| `refundRequestId` | `integer(int64)` | `Required` | Unique refund request identifier. |
| `cancellationRecordId` | `integer(int64)` | `Required` | Unique cancellation record identifier. |
| `invoiceId` | `integer(int64)` | `Required` | Unique invoice identifier. |
| `refundStatus` | `string` | `Required` | Current status value for refund. |
| `requestedAmount` | `number(decimal)` | `Required` | Monetary amount for requested. |
| `approvedAmount` | `number(decimal)` | `Required` | Monetary amount for approved. |

### Schema: RegisterCustomerRequest
<a id="schema-registercustomerrequest"></a>

- Source file: `Backend/Coolzo.Contracts/Requests/CustomerAuth/RegisterCustomerRequest.cs`
- Shape: `record-positional`

| Field | Wire Type | Required | Description |
| --- | --- | --- | --- |
| `customerName` | `string` | `Required` | Display name for customer. |
| `mobileNumber` | `string` | `Required` | Business number/reference for mobile. |
| `emailAddress` | `string` | `Required` | Mapped email address value. |
| `password` | `string?` | `Optional` | Mapped password value. |

### Schema: RejectInstallationProposalRequest
<a id="schema-rejectinstallationproposalrequest"></a>

- Source file: `Backend/Coolzo.Contracts/Requests/GapPhaseC/InstallationLifecycleRequests.cs`
- Shape: `record-positional`

| Field | Wire Type | Required | Description |
| --- | --- | --- | --- |
| `customerRemarks` | `string?` | `Optional` | Free-text remarks for customer. |

### Schema: RejectRefundRequestDecisionRequest
<a id="schema-rejectrefundrequestdecisionrequest"></a>

- Source file: `Backend/Coolzo.Contracts/Requests/GapPhaseD/CancellationRefundRequests.cs`
- Shape: `record-positional`

| Field | Wire Type | Required | Description |
| --- | --- | --- | --- |
| `remarks` | `string` | `Required` | Free-text remarks for the action. |

### Schema: RejectTechnicianDocumentRequest
<a id="schema-rejecttechniciandocumentrequest"></a>

- Source file: `Backend/Coolzo.Contracts/Requests/GapPhaseE/TechnicianOnboardingPhaseERequests.cs`
- Shape: `record-positional`

| Field | Wire Type | Required | Description |
| --- | --- | --- | --- |
| `remarks` | `string` | `Required` | Free-text remarks for the action. |

### Schema: ReleaseHelperAssignmentRequest
<a id="schema-releasehelperassignmentrequest"></a>

- Source file: `Backend/Coolzo.Contracts/Requests/GapPhaseE/HelperPhaseERequests.cs`
- Shape: `record-positional`

| Field | Wire Type | Required | Description |
| --- | --- | --- | --- |
| `remarks` | `string` | `Required` | Free-text remarks for the action. |

### Schema: ReportExportResponse
<a id="schema-reportexportresponse"></a>

- Source file: `Backend/Coolzo.Contracts/Responses/Analytics/AnalyticsResponses.cs`
- Shape: `record-positional`

| Field | Wire Type | Required | Description |
| --- | --- | --- | --- |
| `format` | `string` | `Required` | Mapped format value. |
| `fileName` | `string` | `Required` | Display name for file. |
| `contentType` | `string` | `Required` | Mapped content type value. |
| `content` | `string` | `Required` | Mapped content value. |
| `generatedAtUtc` | `string` | `Required` | UTC timestamp for generated at. |

### Schema: ResetCustomerPasswordRequest
<a id="schema-resetcustomerpasswordrequest"></a>

- Source file: `Backend/Coolzo.Contracts/Requests/Customer/ResetCustomerPasswordRequest.cs`
- Shape: `record-positional`

| Field | Wire Type | Required | Description |
| --- | --- | --- | --- |
| `reason` | `string?` | `Optional` | Mapped reason value. |

### Schema: ResolveCustomerAbsentRequest
<a id="schema-resolvecustomerabsentrequest"></a>

- Source file: `Backend/Coolzo.Contracts/Requests/GapPhaseD/CancellationRefundRequests.cs`
- Shape: `record-positional`

| Field | Wire Type | Required | Description |
| --- | --- | --- | --- |
| `remarks` | `string` | `Required` | Free-text remarks for the action. |

### Schema: RevenueAnalyticsResponse
<a id="schema-revenueanalyticsresponse"></a>

- Source file: `Backend/Coolzo.Contracts/Responses/Analytics/AnalyticsResponses.cs`
- Shape: `record-positional`

| Field | Wire Type | Required | Description |
| --- | --- | --- | --- |
| `totalRevenue` | `number(decimal)` | `Required` | Mapped total revenue value. |
| `paidRevenue` | `number(decimal)` | `Required` | Mapped paid revenue value. |
| `outstandingRevenue` | `number(decimal)` | `Required` | Mapped outstanding revenue value. |
| `invoiceCount` | `integer(int64)` | `Required` | Mapped invoice count value. |
| `averageInvoiceValue` | `number(decimal)` | `Required` | Mapped average invoice value value. |
| `revenueTrends` | `AnalyticsTrendPointResponse[]` | `Required` | Collection of `AnalyticsTrendPointResponse` items. |
| `revenueByService` | `AnalyticsBreakdownItemResponse[]` | `Required` | Collection of `AnalyticsBreakdownItemResponse` items. |
| `revenueByCustomerSegment` | `AnalyticsBreakdownItemResponse[]` | `Required` | Collection of `AnalyticsBreakdownItemResponse` items. |

### Schema: RevisitRequestCreateRequest
<a id="schema-revisitrequestcreaterequest"></a>

- Source file: `Backend/Coolzo.Contracts/Requests/Revisit/RevisitRequestCreateRequest.cs`
- Shape: `record-positional`

| Field | Wire Type | Required | Description |
| --- | --- | --- | --- |
| `originalJobCardId` | `integer(int64)` | `Required` | Unique original job card identifier. |
| `revisitType` | `string` | `Required` | Mapped revisit type value. |
| `preferredVisitDateUtc` | `datetime?` | `Optional` | UTC timestamp for preferred visit date. |
| `issueSummary` | `string` | `Required` | Boolean flag indicating whether sue summary is true. |
| `requestRemarks` | `string?` | `Optional` | Free-text remarks for request. |
| `customerAmcId` | `integer(int64)?` | `Optional` | Unique customer amc identifier. |
| `warrantyClaimId` | `integer(int64)?` | `Optional` | Unique warranty claim identifier. |
| `chargeAmount` | `number(decimal)?` | `Optional` | Monetary amount for charge. |

### Schema: RevisitRequestResponse
<a id="schema-revisitrequestresponse"></a>

- Source file: `Backend/Coolzo.Contracts/Responses/Revisit/RevisitRequestResponse.cs`
- Shape: `record-positional`

| Field | Wire Type | Required | Description |
| --- | --- | --- | --- |
| `revisitRequestId` | `integer(int64)` | `Required` | Unique revisit request identifier. |
| `bookingId` | `integer(int64)` | `Required` | Unique booking identifier. |
| `bookingReference` | `string` | `Required` | Reference value for booking. |
| `customerId` | `integer(int64)` | `Required` | Unique customer identifier. |
| `customerName` | `string` | `Required` | Display name for customer. |
| `originalJobCardId` | `integer(int64)` | `Required` | Unique original job card identifier. |
| `originalJobCardNumber` | `string` | `Required` | Business number/reference for original job card. |
| `revisitType` | `string` | `Required` | Mapped revisit type value. |
| `currentStatus` | `string` | `Required` | Current status value for current. |
| `requestedDateUtc` | `datetime` | `Required` | UTC timestamp for requested date. |
| `preferredVisitDateUtc` | `datetime?` | `Optional` | UTC timestamp for preferred visit date. |
| `issueSummary` | `string` | `Required` | Boolean flag indicating whether sue summary is true. |
| `requestRemarks` | `string` | `Required` | Free-text remarks for request. |
| `chargeAmount` | `number(decimal)` | `Required` | Monetary amount for charge. |
| `customerAmcId` | `integer(int64)?` | `Optional` | Unique customer amc identifier. |
| `warrantyClaimId` | `integer(int64)?` | `Optional` | Unique warranty claim identifier. |

### Schema: RoleResponse
<a id="schema-roleresponse"></a>

- Source file: `Backend/Coolzo.Contracts/Responses/Role/RoleResponse.cs`
- Shape: `record-positional`

| Field | Wire Type | Required | Description |
| --- | --- | --- | --- |
| `roleId` | `integer(int64)` | `Required` | Unique role identifier. |
| `roleName` | `string` | `Required` | Display name for role. |
| `displayName` | `string` | `Required` | Display name for display. |
| `description` | `string` | `Required` | Mapped description value. |
| `isActive` | `boolean` | `Required` | Boolean flag indicating whether active is true. |
| `permissionIds` | `long[]` | `Required` | List of permission identifiers. |
| `permissions` | `string[]` | `Required` | Collection of `string` items. |

### Schema: SaveBusinessHoursRequest
<a id="schema-savebusinesshoursrequest"></a>

- Source file: `Backend/Coolzo.Contracts/Requests/Admin/AdminRequests.cs`
- Shape: `record-positional`

| Field | Wire Type | Required | Description |
| --- | --- | --- | --- |
| `businessHours` | `BusinessHourItemRequest[]` | `Required` | Collection of `BusinessHourItemRequest` items. |

### Schema: SaveHelperTaskResponseRequest
<a id="schema-savehelpertaskresponserequest"></a>

- Source file: `Backend/Coolzo.Contracts/Requests/GapPhaseE/HelperPhaseERequests.cs`
- Shape: `record-positional`

| Field | Wire Type | Required | Description |
| --- | --- | --- | --- |
| `responseStatus` | `string` | `Required` | Current status value for response. |
| `responseRemarks` | `string?` | `Optional` | Free-text remarks for response. |

### Schema: SaveInstallationChecklistRequest
<a id="schema-saveinstallationchecklistrequest"></a>

- Source file: `Backend/Coolzo.Contracts/Requests/GapPhaseC/InstallationLifecycleRequests.cs`
- Shape: `record-positional`

| Field | Wire Type | Required | Description |
| --- | --- | --- | --- |
| `items` | `InstallationChecklistItemRequest[]` | `Required` | Collection of `InstallationChecklistItemRequest` items. |

### Schema: SaveJobAttachmentRequest
<a id="schema-savejobattachmentrequest"></a>

- Source file: `Backend/Coolzo.Contracts/Requests/FieldExecution/SaveJobAttachmentRequest.cs`
- Shape: `record-positional`

| Field | Wire Type | Required | Description |
| --- | --- | --- | --- |
| `attachmentType` | `string` | `Required` | Mapped attachment type value. |
| `fileName` | `string` | `Required` | Display name for file. |
| `contentType` | `string` | `Required` | Mapped content type value. |
| `base64Content` | `string` | `Required` | Mapped base64 content value. |
| `attachmentRemarks` | `string?` | `Optional` | Free-text remarks for attachment. |

### Schema: SaveJobChecklistResponseRequest
<a id="schema-savejobchecklistresponserequest"></a>

- Source file: `Backend/Coolzo.Contracts/Requests/FieldExecution/SaveJobChecklistResponseRequest.cs`
- Shape: `record-positional`

| Field | Wire Type | Required | Description |
| --- | --- | --- | --- |
| `items` | `SaveJobChecklistResponseItemRequest[]` | `Required` | Collection of `SaveJobChecklistResponseItemRequest` items. |

### Schema: SaveJobDiagnosisRequest
<a id="schema-savejobdiagnosisrequest"></a>

- Source file: `Backend/Coolzo.Contracts/Requests/FieldExecution/SaveJobDiagnosisRequest.cs`
- Shape: `record-positional`

| Field | Wire Type | Required | Description |
| --- | --- | --- | --- |
| `complaintIssueMasterId` | `integer(int64)?` | `Optional` | Unique complaint issue master identifier. |
| `diagnosisResultMasterId` | `integer(int64)?` | `Optional` | Unique diagnosis result master identifier. |
| `diagnosisRemarks` | `string?` | `Optional` | Free-text remarks for diagnosis. |

### Schema: SaveJobExecutionNoteRequest
<a id="schema-savejobexecutionnoterequest"></a>

- Source file: `Backend/Coolzo.Contracts/Requests/FieldExecution/SaveJobExecutionNoteRequest.cs`
- Shape: `record-positional`

| Field | Wire Type | Required | Description |
| --- | --- | --- | --- |
| `noteText` | `string` | `Required` | Mapped note text value. |
| `isCustomerVisible` | `boolean` | `Required` | Boolean flag indicating whether customer visible is true. |

### Schema: ScheduleInstallationSurveyRequest
<a id="schema-scheduleinstallationsurveyrequest"></a>

- Source file: `Backend/Coolzo.Contracts/Requests/GapPhaseC/InstallationLifecycleRequests.cs`
- Shape: `record-positional`

| Field | Wire Type | Required | Description |
| --- | --- | --- | --- |
| `surveyDateUtc` | `datetime` | `Required` | UTC timestamp for survey date. |
| `technicianId` | `integer(int64)?` | `Optional` | Unique technician identifier. |
| `remarks` | `string?` | `Optional` | Free-text remarks for the action. |

### Schema: ServiceCategoryLookupResponse
<a id="schema-servicecategorylookupresponse"></a>

- Source file: `Backend/Coolzo.Contracts/Responses/Booking/ServiceCategoryLookupResponse.cs`
- Shape: `record-positional`

| Field | Wire Type | Required | Description |
| --- | --- | --- | --- |
| `serviceCategoryId` | `integer(int64)` | `Required` | Unique service category identifier. |
| `categoryName` | `string` | `Required` | Display name for category. |
| `description` | `string` | `Required` | Mapped description value. |

### Schema: ServiceHistoryItemResponse
<a id="schema-servicehistoryitemresponse"></a>

- Source file: `Backend/Coolzo.Contracts/Responses/ServiceHistory/ServiceHistoryItemResponse.cs`
- Shape: `record-positional`

| Field | Wire Type | Required | Description |
| --- | --- | --- | --- |
| `historyType` | `string` | `Required` | Mapped history type value. |
| `referenceNumber` | `string` | `Required` | Business number/reference for reference. |
| `title` | `string` | `Required` | Mapped title value. |
| `status` | `string` | `Required` | Current status value for the record. |
| `eventDateUtc` | `datetime` | `Required` | UTC timestamp for event date. |
| `detail` | `string` | `Required` | Mapped detail value. |
| `amount` | `number(decimal)?` | `Optional` | Monetary amount for the record. |
| `bookingId` | `integer(int64)?` | `Optional` | Unique booking identifier. |
| `serviceRequestId` | `integer(int64)?` | `Optional` | Unique service request identifier. |
| `jobCardId` | `integer(int64)?` | `Optional` | Unique job card identifier. |
| `invoiceId` | `integer(int64)?` | `Optional` | Unique invoice identifier. |
| `customerAmcId` | `integer(int64)?` | `Optional` | Unique customer amc identifier. |
| `revisitRequestId` | `integer(int64)?` | `Optional` | Unique revisit request identifier. |

### Schema: ServiceLookupResponse
<a id="schema-servicelookupresponse"></a>

- Source file: `Backend/Coolzo.Contracts/Responses/Booking/ServiceLookupResponse.cs`
- Shape: `record-positional`

| Field | Wire Type | Required | Description |
| --- | --- | --- | --- |
| `serviceId` | `integer(int64)` | `Required` | Unique service identifier. |
| `serviceCategoryId` | `integer(int64)` | `Required` | Unique service category identifier. |
| `serviceName` | `string` | `Required` | Display name for service. |
| `summary` | `string` | `Required` | Summary text for the record. |
| `basePrice` | `number(decimal)` | `Required` | Mapped base price value. |
| `pricingModelName` | `string` | `Required` | Display name for pricing model. |

### Schema: ServiceRequestDetailResponse
<a id="schema-servicerequestdetailresponse"></a>

- Source file: `Backend/Coolzo.Contracts/Responses/Operations/ServiceRequestDetailResponse.cs`
- Shape: `record-positional`

| Field | Wire Type | Required | Description |
| --- | --- | --- | --- |
| `serviceRequestId` | `integer(int64)` | `Required` | Unique service request identifier. |
| `serviceRequestNumber` | `string` | `Required` | Business number/reference for service request. |
| `bookingId` | `integer(int64)` | `Required` | Unique booking identifier. |
| `bookingReference` | `string` | `Required` | Reference value for booking. |
| `bookingStatus` | `string` | `Required` | Current status value for booking. |
| `currentStatus` | `string` | `Required` | Current status value for current. |
| `serviceRequestDateUtc` | `datetime` | `Required` | UTC timestamp for service request date. |
| `sourceChannel` | `string` | `Required` | Mapped source channel value. |
| `customerName` | `string` | `Required` | Display name for customer. |
| `mobileNumber` | `string` | `Required` | Business number/reference for mobile. |
| `emailAddress` | `string` | `Required` | Mapped email address value. |
| `addressSummary` | `string` | `Required` | Summary text for address. |
| `zoneName` | `string` | `Required` | Display name for zone. |
| `slotDate` | `date` | `Required` | Date value for slot. |
| `slotLabel` | `string` | `Required` | Mapped slot label value. |
| `serviceName` | `string` | `Required` | Display name for service. |
| `acTypeName` | `string` | `Required` | Display name for ac type. |
| `tonnageName` | `string` | `Required` | Display name for tonnage. |
| `brandName` | `string` | `Required` | Display name for brand. |
| `modelName` | `string` | `Required` | Display name for model. |
| `issueNotes` | `string` | `Required` | Boolean flag indicating whether sue notes is true. |
| `estimatedPrice` | `number(decimal)` | `Required` | Mapped estimated price value. |
| `technicianId` | `integer(int64)?` | `Optional` | Unique technician identifier. |
| `technicianCode` | `string?` | `Optional` | Code value for technician. |
| `technicianName` | `string?` | `Optional` | Display name for technician. |
| `technicianMobileNumber` | `string?` | `Optional` | Business number/reference for technician mobile. |
| `assignmentRemarks` | `string?` | `Optional` | Free-text remarks for assignment. |
| `jobCard` | `JobCardSummaryResponse` | `Required` | Mapped job card value. |
| `quotationId` | `integer(int64)?` | `Optional` | Unique quotation identifier. |
| `quotationNumber` | `string?` | `Optional` | Business number/reference for quotation. |
| `quotationStatus` | `string?` | `Optional` | Current status value for quotation. |
| `invoiceId` | `integer(int64)?` | `Optional` | Unique invoice identifier. |
| `invoiceNumber` | `string?` | `Optional` | Business number/reference for invoice. |
| `invoiceStatus` | `string?` | `Optional` | Current status value for invoice. |
| `invoiceGrandTotalAmount` | `number(decimal)?` | `Optional` | Monetary amount for invoice grand total. |
| `invoiceBalanceAmount` | `number(decimal)?` | `Optional` | Monetary amount for invoice balance. |
| `diagnosisSummary` | `JobDiagnosisSummaryResponse` | `Required` | Summary text for diagnosis. |
| `checklistSummary` | `JobChecklistSummaryResponse` | `Required` | Summary text for checklist. |
| `executionNotes` | `JobExecutionNoteResponse[]` | `Required` | Collection of `JobExecutionNoteResponse` items. |
| `attachments` | `JobAttachmentResponse[]` | `Required` | Collection of `JobAttachmentResponse` items. |
| `executionTimeline` | `JobExecutionTimelineItemResponse[]` | `Required` | Collection of `JobExecutionTimelineItemResponse` items. |
| `statusTimeline` | `ServiceRequestStatusHistoryResponse[]` | `Required` | Collection of `ServiceRequestStatusHistoryResponse` items. |
| `assignmentHistory` | `AssignmentHistoryItemResponse[]` | `Required` | Collection of `AssignmentHistoryItemResponse` items. |

### Schema: ServiceRequestListItemResponse
<a id="schema-servicerequestlistitemresponse"></a>

- Source file: `Backend/Coolzo.Contracts/Responses/Operations/ServiceRequestListItemResponse.cs`
- Shape: `record-positional`

| Field | Wire Type | Required | Description |
| --- | --- | --- | --- |
| `serviceRequestId` | `integer(int64)` | `Required` | Unique service request identifier. |
| `serviceRequestNumber` | `string` | `Required` | Business number/reference for service request. |
| `bookingId` | `integer(int64)` | `Required` | Unique booking identifier. |
| `bookingReference` | `string` | `Required` | Reference value for booking. |
| `customerName` | `string` | `Required` | Display name for customer. |
| `serviceName` | `string` | `Required` | Display name for service. |
| `currentStatus` | `string` | `Required` | Current status value for current. |
| `technicianName` | `string?` | `Optional` | Display name for technician. |
| `slotDate` | `date` | `Required` | Date value for slot. |
| `slotLabel` | `string` | `Required` | Mapped slot label value. |
| `serviceRequestDateUtc` | `datetime` | `Required` | UTC timestamp for service request date. |

### Schema: SkillAssessmentDetailResponse
<a id="schema-skillassessmentdetailresponse"></a>

- Source file: `Backend/Coolzo.Contracts/Responses/GapPhaseE/TechnicianOnboardingPhaseEResponses.cs`
- Shape: `record-positional`

| Field | Wire Type | Required | Description |
| --- | --- | --- | --- |
| `skillAssessmentId` | `integer(int64)` | `Required` | Unique skill assessment identifier. |
| `skillTagId` | `integer(int64)?` | `Optional` | Unique skill tag identifier. |
| `assessmentCode` | `string` | `Required` | Code value for assessment. |
| `assessmentName` | `string` | `Required` | Display name for assessment. |
| `assessmentStatus` | `string` | `Required` | Current status value for assessment. |
| `scorePercentage` | `number(decimal)` | `Required` | Mapped score percentage value. |
| `assessmentResult` | `string` | `Required` | Mapped assessment result value. |
| `passFlag` | `boolean` | `Required` | Mapped pass flag value. |
| `assessedByUserId` | `integer(int64)?` | `Optional` | Unique assessed by user identifier. |
| `assessedOnUtc` | `datetime?` | `Optional` | Mapped assessed on utc value. |
| `remarks` | `string` | `Required` | Free-text remarks for the action. |

### Schema: SlotAvailabilityResponse
<a id="schema-slotavailabilityresponse"></a>

- Source file: `Backend/Coolzo.Contracts/Responses/Booking/SlotAvailabilityResponse.cs`
- Shape: `record-positional`

| Field | Wire Type | Required | Description |
| --- | --- | --- | --- |
| `slotAvailabilityId` | `integer(int64)` | `Required` | Unique slot availability identifier. |
| `zoneId` | `integer(int64)` | `Required` | Unique zone identifier. |
| `slotDate` | `date` | `Required` | Date value for slot. |
| `slotLabel` | `string` | `Required` | Mapped slot label value. |
| `startTime` | `string` | `Required` | Mapped start time value. |
| `endTime` | `string` | `Required` | Mapped end time value. |
| `availableCapacity` | `integer(int32)` | `Required` | Mapped available capacity value. |
| `reservedCapacity` | `integer(int32)` | `Required` | Mapped reserved capacity value. |
| `isAvailable` | `boolean` | `Required` | Boolean flag indicating whether available is true. |

### Schema: StartInstallationRequest
<a id="schema-startinstallationrequest"></a>

- Source file: `Backend/Coolzo.Contracts/Requests/GapPhaseC/InstallationLifecycleRequests.cs`
- Shape: `record-positional`

| Field | Wire Type | Required | Description |
| --- | --- | --- | --- |
| `remarks` | `string?` | `Optional` | Free-text remarks for the action. |

### Schema: StockTransactionResponse
<a id="schema-stocktransactionresponse"></a>

- Source file: `Backend/Coolzo.Contracts/Responses/Inventory/StockTransactionResponse.cs`
- Shape: `record-positional`

| Field | Wire Type | Required | Description |
| --- | --- | --- | --- |
| `stockTransactionId` | `integer(int64)` | `Required` | Unique stock transaction identifier. |
| `itemId` | `integer(int64)` | `Required` | Unique item identifier. |
| `itemCode` | `string` | `Required` | Code value for item. |
| `itemName` | `string` | `Required` | Display name for item. |
| `transactionType` | `string` | `Required` | Mapped transaction type value. |
| `warehouseId` | `integer(int64)?` | `Optional` | Unique warehouse identifier. |
| `warehouseName` | `string?` | `Optional` | Display name for warehouse. |
| `technicianId` | `integer(int64)?` | `Optional` | Unique technician identifier. |
| `technicianName` | `string?` | `Optional` | Display name for technician. |
| `jobCardId` | `integer(int64)?` | `Optional` | Unique job card identifier. |
| `jobCardNumber` | `string?` | `Optional` | Business number/reference for job card. |
| `quantity` | `number(decimal)` | `Required` | Mapped quantity value. |
| `unitCost` | `number(decimal)` | `Required` | Mapped unit cost value. |
| `amount` | `number(decimal)` | `Required` | Monetary amount for the record. |
| `balanceAfterTransaction` | `number(decimal)` | `Required` | Mapped balance after transaction value. |
| `referenceNumber` | `string` | `Required` | Business number/reference for reference. |
| `transactionGroupCode` | `string` | `Required` | Code value for transaction group. |
| `transactionDateUtc` | `datetime` | `Required` | UTC timestamp for transaction date. |
| `remarks` | `string` | `Required` | Free-text remarks for the action. |
| `supplierName` | `string?` | `Optional` | Display name for supplier. |
| `createdBy` | `string` | `Required` | Mapped created by value. |

### Schema: SubmitInstallationSurveyRequest
<a id="schema-submitinstallationsurveyrequest"></a>

- Source file: `Backend/Coolzo.Contracts/Requests/GapPhaseC/InstallationLifecycleRequests.cs`
- Shape: `record-positional`

| Field | Wire Type | Required | Description |
| --- | --- | --- | --- |
| `siteConditionSummary` | `string` | `Required` | Summary text for site condition. |
| `electricalReadiness` | `boolean` | `Required` | Mapped electrical readiness value. |
| `accessReadiness` | `boolean` | `Required` | Mapped access readiness value. |
| `safetyRiskNotes` | `string?` | `Optional` | Mapped safety risk notes value. |
| `recommendedAction` | `string?` | `Optional` | Mapped recommended action value. |
| `estimatedMaterialCost` | `number(decimal)` | `Required` | Mapped estimated material cost value. |
| `measurementsJson` | `string?` | `Optional` | JSON-serialized payload for measurements. |
| `photoUrlsJson` | `string?` | `Optional` | JSON-serialized payload for photo urls. |
| `items` | `InstallationSurveyItemRequest[]` | `Required` | Collection of `InstallationSurveyItemRequest` items. |

### Schema: SubmitSkillAssessmentResultRequest
<a id="schema-submitskillassessmentresultrequest"></a>

- Source file: `Backend/Coolzo.Contracts/Requests/GapPhaseE/TechnicianOnboardingPhaseERequests.cs`
- Shape: `record-positional`

| Field | Wire Type | Required | Description |
| --- | --- | --- | --- |
| `scorePercentage` | `number(decimal)` | `Required` | Mapped score percentage value. |
| `passFlag` | `boolean` | `Required` | Mapped pass flag value. |
| `remarks` | `string?` | `Optional` | Free-text remarks for the action. |

### Schema: SubmitSurveyReportRequest
<a id="schema-submitsurveyreportrequest"></a>

- Source file: `Backend/Coolzo.Contracts/Requests/GapPhaseA/InstallationRequests.cs`
- Shape: `record-positional`

| Field | Wire Type | Required | Description |
| --- | --- | --- | --- |
| `surveyDecision` | `string` | `Required` | Mapped survey decision value. |
| `siteConditionSummary` | `string` | `Required` | Summary text for site condition. |
| `electricalReadiness` | `boolean` | `Required` | Mapped electrical readiness value. |
| `accessReadiness` | `boolean` | `Required` | Mapped access readiness value. |
| `safetyRiskNotes` | `string?` | `Optional` | Mapped safety risk notes value. |
| `recommendedAction` | `string?` | `Optional` | Mapped recommended action value. |
| `estimatedMaterialCost` | `number(decimal)` | `Required` | Mapped estimated material cost value. |
| `syncDeviceReference` | `string?` | `Optional` | Reference value for sync device. |
| `syncReference` | `string?` | `Optional` | Reference value for sync. |

### Schema: SupplierClaimResponse
<a id="schema-supplierclaimresponse"></a>

- Source file: `Backend/Coolzo.Contracts/Responses/GapPhaseA/GapPhaseAResponses.cs`
- Shape: `record-positional`

| Field | Wire Type | Required | Description |
| --- | --- | --- | --- |
| `partsReturnId` | `integer(int64)` | `Required` | Unique parts return identifier. |
| `partsReturnNumber` | `string` | `Required` | Business number/reference for parts return. |
| `partsReturnStatus` | `string` | `Required` | Current status value for parts return. |
| `supplierClaimReference` | `string` | `Required` | Reference value for supplier claim. |

### Schema: SupportAnalyticsResponse
<a id="schema-supportanalyticsresponse"></a>

- Source file: `Backend/Coolzo.Contracts/Responses/Analytics/AnalyticsResponses.cs`
- Shape: `record-positional`

| Field | Wire Type | Required | Description |
| --- | --- | --- | --- |
| `totalTickets` | `integer(int64)` | `Required` | Mapped total tickets value. |
| `openTickets` | `integer(int64)` | `Required` | Mapped open tickets value. |
| `resolvedTickets` | `integer(int64)` | `Required` | Mapped resolved tickets value. |
| `escalationCount` | `integer(int64)` | `Required` | Mapped escalation count value. |
| `averageResolutionHours` | `number(decimal)` | `Required` | Mapped average resolution hours value. |
| `statusDistribution` | `AnalyticsBreakdownItemResponse[]` | `Required` | Collection of `AnalyticsBreakdownItemResponse` items. |
| `resolutionTrends` | `SupportResolutionTrendPointResponse[]` | `Required` | Collection of `SupportResolutionTrendPointResponse` items. |

### Schema: SupportTicketActionRequest
<a id="schema-supportticketactionrequest"></a>

- Source file: `Backend/Coolzo.Contracts/Requests/Support/SupportTicketActionRequest.cs`
- Shape: `record-positional`

| Field | Wire Type | Required | Description |
| --- | --- | --- | --- |
| `remarks` | `string` | `Required` | Free-text remarks for the action. |

### Schema: SupportFeedbackResponse
<a id="schema-supportfeedbackresponse"></a>

- Source file: `Backend/Coolzo.Contracts/Responses/Support/SupportFeedbackResponse.cs`
- Shape: `record-positional`

| Field | Wire Type | Required | Description |
| --- | --- | --- | --- |
| `customerReviewId` | `integer(int64)` | `Required` | Unique customer review identifier. |
| `customerId` | `integer(int64)` | `Required` | Unique customer identifier. |
| `customerName` | `string` | `Required` | Customer display name snapshot. |
| `customerPhotoUrl` | `string` | `Required` | Customer photo URL snapshot. |
| `rating` | `integer(int32)` | `Required` | Numeric review rating. |
| `comment` | `string` | `Required` | Review comment text. |
| `bookingId` | `integer(int64)?` | `Optional` | Linked booking identifier. |
| `serviceId` | `integer(int64)?` | `Optional` | Linked service identifier. |
| `createdAt` | `datetime` | `Required` | Created timestamp. |
| `feedbackStatus` | `string` | `Required` | Moderation status value: `published`, `unpublished`, or `flagged`. |
| `adminResponse` | `string?` | `Optional` | Persisted admin response text. |
| `flagReason` | `string?` | `Optional` | Stored flag reason when the record is flagged. |
| `moderatedAt` | `datetime?` | `Optional` | Latest moderation timestamp. |
| `moderatedBy` | `string?` | `Optional` | Latest moderation actor display/login name. |

### Schema: SupportTicketDetailResponse
<a id="schema-supportticketdetailresponse"></a>

- Source file: `Backend/Coolzo.Contracts/Responses/Support/SupportTicketDetailResponse.cs`
- Shape: `record-positional`

| Field | Wire Type | Required | Description |
| --- | --- | --- | --- |
| `supportTicketId` | `integer(int64)` | `Required` | Unique support ticket identifier. |
| `ticketNumber` | `string` | `Required` | Business number/reference for ticket. |
| `subject` | `string` | `Required` | Mapped subject value. |
| `description` | `string` | `Required` | Mapped description value. |
| `customerId` | `integer(int64)` | `Required` | Unique customer identifier. |
| `customerName` | `string` | `Required` | Display name for customer. |
| `customerMobile` | `string` | `Required` | Mapped customer mobile value. |
| `customerEmail` | `string` | `Required` | Mapped customer email value. |
| `supportTicketCategoryId` | `integer(int64)` | `Required` | Unique support ticket category identifier. |
| `categoryName` | `string` | `Required` | Display name for category. |
| `supportTicketPriorityId` | `integer(int64)` | `Required` | Unique support ticket priority identifier. |
| `priorityName` | `string` | `Required` | Display name for priority. |
| `status` | `string` | `Required` | Current status value for the record. |
| `assignedUserId` | `integer(int64)?` | `Optional` | Unique assigned user identifier. |
| `assignedOwnerName` | `string?` | `Optional` | Display name for assigned owner. |

| `dateCreated` | `datetime` | `Required` | Mapped date created value. |
| `lastUpdated` | `datetime?` | `Optional` | Mapped last updated value. |
| `canCustomerClose` | `boolean` | `Required` | Boolean permission flag indicating whether customer close is allowed. |
| `canManage` | `boolean` | `Required` | Boolean permission flag indicating whether manage is allowed. |
| `links` | `SupportTicketLinkResponse[]` | `Required` | Collection of `SupportTicketLinkResponse` items. |
| `assignments` | `SupportTicketAssignmentResponse[]` | `Required` | Collection of `SupportTicketAssignmentResponse` items. |
| `replies` | `SupportTicketReplyResponse[]` | `Required` | Collection of `SupportTicketReplyResponse` items. |
| `escalations` | `SupportTicketEscalationResponse[]` | `Required` | Collection of `SupportTicketEscalationResponse` items. |
| `statusHistory` | `SupportTicketStatusHistoryResponse[]` | `Required` | Collection of `SupportTicketStatusHistoryResponse` items. |

### Schema: SupportTicketEscalationResponse
<a id="schema-supportticketescalationresponse"></a>

- Source file: `Backend/Coolzo.Contracts/Responses/Support/SupportTicketEscalationResponse.cs`
- Shape: `record-positional`

| Field | Wire Type | Required | Description |
| --- | --- | --- | --- |
| `supportTicketEscalationId` | `integer(int64)` | `Required` | Unique support ticket escalation identifier. |
| `escalationTarget` | `string` | `Required` | Mapped escalation target value. |
| `escalationRemarks` | `string` | `Required` | Free-text remarks for escalation. |
| `escalatedBy` | `string` | `Required` | Mapped escalated by value. |
| `escalatedDateUtc` | `datetime` | `Required` | UTC timestamp for escalated date. |

### Schema: SupportTicketListItemResponse
<a id="schema-supportticketlistitemresponse"></a>

- Source file: `Backend/Coolzo.Contracts/Responses/Support/SupportTicketListItemResponse.cs`
- Shape: `record-positional`

| Field | Wire Type | Required | Description |
| --- | --- | --- | --- |
| `supportTicketId` | `integer(int64)` | `Required` | Unique support ticket identifier. |
| `ticketNumber` | `string` | `Required` | Business number/reference for ticket. |
| `subject` | `string` | `Required` | Mapped subject value. |
| `customerName` | `string` | `Required` | Display name for customer. |
| `customerMobile` | `string` | `Required` | Mapped customer mobile value. |
| `linkedEntityType` | `string?` | `Optional` | Mapped linked entity type value. |
| `linkedEntitySummary` | `string` | `Required` | Summary text for linked entity. |
| `categoryName` | `string` | `Required` | Display name for category. |
| `priorityName` | `string` | `Required` | Display name for priority. |
| `status` | `string` | `Required` | Current status value for the record. |
| `assignedUserId` | `integer(int64)?` | `Optional` | Unique assigned user identifier. |
| `assignedOwnerName` | `string?` | `Optional` | Display name for assigned owner. |
| `dateCreated` | `datetime` | `Required` | Mapped date created value. |
| `lastUpdated` | `datetime?` | `Optional` | Mapped last updated value. |

### Schema: SupportTicketReplyResponse
<a id="schema-supportticketreplyresponse"></a>

- Source file: `Backend/Coolzo.Contracts/Responses/Support/SupportTicketReplyResponse.cs`
- Shape: `record-positional`

| Field | Wire Type | Required | Description |
| --- | --- | --- | --- |
| `supportTicketReplyId` | `integer(int64)` | `Required` | Unique support ticket reply identifier. |
| `replyText` | `string` | `Required` | Mapped reply text value. |
| `isInternalOnly` | `boolean` | `Required` | Boolean flag indicating whether internal only is true. |
| `isFromCustomer` | `boolean` | `Required` | Boolean flag indicating whether from customer is true. |
| `createdBy` | `string` | `Required` | Mapped created by value. |
| `replyDateUtc` | `datetime` | `Required` | UTC timestamp for reply date. |

### Schema: SystemConfigurationResponse
<a id="schema-systemconfigurationresponse"></a>

- Source file: `Backend/Coolzo.Contracts/Responses/Admin/AdminResponses.cs`
- Shape: `record-positional`

| Field | Wire Type | Required | Description |
| --- | --- | --- | --- |
| `systemConfigurationId` | `integer(int64)` | `Required` | Unique system configuration identifier. |
| `configurationGroup` | `string` | `Required` | Mapped configuration group value. |
| `configurationKey` | `string` | `Required` | Mapped configuration key value. |
| `configurationValue` | `string` | `Required` | Mapped configuration value value. |
| `valueType` | `string` | `Required` | Mapped value type value. |
| `description` | `string` | `Required` | Mapped description value. |
| `isSensitive` | `boolean` | `Required` | Boolean flag indicating whether sensitive is true. |
| `isActive` | `boolean` | `Required` | Boolean flag indicating whether active is true. |
| `dateCreated` | `datetime` | `Required` | Mapped date created value. |
| `lastUpdated` | `datetime?` | `Optional` | Mapped last updated value. |

### Schema: SystemConfigurationUpsertRequest
<a id="schema-systemconfigurationupsertrequest"></a>

- Source file: `Backend/Coolzo.Contracts/Requests/Admin/AdminRequests.cs`
- Shape: `record-positional`

| Field | Wire Type | Required | Description |
| --- | --- | --- | --- |
| `configurationGroup` | `string` | `Required` | Mapped configuration group value. |
| `configurationKey` | `string` | `Required` | Mapped configuration key value. |
| `configurationValue` | `string` | `Required` | Mapped configuration value value. |
| `valueType` | `string` | `Required` | Mapped value type value. |
| `description` | `string?` | `Optional` | Mapped description value. |
| `isSensitive` | `boolean` | `Required` | Boolean flag indicating whether sensitive is true. |
| `isActive` | `boolean` | `Required` | Boolean flag indicating whether active is true. |

### Schema: SystemHealthResponse
<a id="schema-systemhealthresponse"></a>

- Source file: `Backend/Coolzo.Contracts/Responses/GapPhaseA/GapPhaseAResponses.cs`
- Shape: `record-positional`

| Field | Wire Type | Required | Description |
| --- | --- | --- | --- |
| `status` | `string` | `Required` | Current status value for the record. |
| `openAlertCount` | `integer(int32)` | `Required` | Mapped open alert count value. |
| `pendingOfflineSyncCount` | `integer(int32)` | `Required` | Mapped pending offline sync count value. |
| `pendingWebhookRetryCount` | `integer(int32)` | `Required` | Mapped pending webhook retry count value. |
| `enabledFeatureFlagCount` | `integer(int32)` | `Required` | Mapped enabled feature flag count value. |
| `criticalTriggerCodes` | `string[]` | `Required` | Collection of `string` items. |

### Schema: SystemSettingResponse
<a id="schema-systemsettingresponse"></a>

- Source file: `Backend/Coolzo.Contracts/Responses/Configuration/SystemSettingResponse.cs`
- Shape: `record-positional`

| Field | Wire Type | Required | Description |
| --- | --- | --- | --- |
| `systemSettingId` | `integer(int64)` | `Required` | Unique system setting identifier. |
| `settingKey` | `string` | `Required` | Mapped setting key value. |
| `settingValue` | `string` | `Required` | Mapped setting value value. |
| `dataType` | `string` | `Required` | Mapped data type value. |
| `isSensitive` | `boolean` | `Required` | Boolean flag indicating whether sensitive is true. |

### Schema: TechnicianActivationLogResponse
<a id="schema-technicianactivationlogresponse"></a>

- Source file: `Backend/Coolzo.Contracts/Responses/GapPhaseE/TechnicianOnboardingPhaseEResponses.cs`
- Shape: `record-positional`

| Field | Wire Type | Required | Description |
| --- | --- | --- | --- |
| `technicianActivationLogId` | `integer(int64)` | `Required` | Unique technician activation log identifier. |
| `activationAction` | `string` | `Required` | Mapped activation action value. |
| `activationReason` | `string` | `Required` | Mapped activation reason value. |
| `activatedByUserId` | `integer(int64)?` | `Optional` | Unique activated by user identifier. |
| `activatedOnUtc` | `datetime` | `Required` | Mapped activated on utc value. |
| `eligibilitySnapshot` | `string` | `Required` | Mapped eligibility snapshot value. |

### Schema: TechnicianAvailabilityResponse
<a id="schema-technicianavailabilityresponse"></a>

- Source file: `Backend/Coolzo.Contracts/Responses/Operations/TechnicianAvailabilityResponse.cs`
- Shape: `record-positional`

| Field | Wire Type | Required | Description |
| --- | --- | --- | --- |
| `technicianId` | `integer(int64)` | `Required` | Unique technician identifier. |
| `technicianCode` | `string` | `Required` | Code value for technician. |
| `technicianName` | `string` | `Required` | Display name for technician. |
| `mobileNumber` | `string` | `Required` | Business number/reference for mobile. |
| `emailAddress` | `string` | `Required` | Mapped email address value. |
| `baseZoneName` | `string?` | `Optional` | Display name for base zone. |
| `availableDate` | `date` | `Required` | Date value for available. |
| `availableSlotCount` | `integer(int32)` | `Required` | Mapped available slot count value. |
| `bookedAssignmentCount` | `integer(int32)` | `Required` | Mapped booked assignment count value. |
| `remainingCapacity` | `integer(int32)` | `Required` | Mapped remaining capacity value. |
| `isAvailable` | `boolean` | `Required` | Boolean flag indicating whether available is true. |
| `isSkillMatched` | `boolean` | `Required` | Boolean flag indicating whether skill matched is true. |
| `availabilityMessage` | `string` | `Required` | Mapped availability message value. |

### Schema: TechnicianDocumentDetailResponse
<a id="schema-techniciandocumentdetailresponse"></a>

- Source file: `Backend/Coolzo.Contracts/Responses/GapPhaseE/TechnicianOnboardingPhaseEResponses.cs`
- Shape: `record-positional`

| Field | Wire Type | Required | Description |
| --- | --- | --- | --- |
| `technicianDocumentId` | `integer(int64)` | `Required` | Unique technician document identifier. |
| `documentType` | `string` | `Required` | Mapped document type value. |
| `documentNumber` | `string` | `Required` | Business number/reference for document. |
| `storageUrl` | `string` | `Required` | Mapped storage url value. |
| `verificationStatus` | `string` | `Required` | Current status value for verification. |
| `verificationRemarks` | `string` | `Required` | Free-text remarks for verification. |
| `expiryDateUtc` | `datetime?` | `Optional` | UTC timestamp for expiry date. |
| `verifiedByUserId` | `integer(int64)?` | `Optional` | Unique verified by user identifier. |
| `verifiedOnUtc` | `datetime?` | `Optional` | Mapped verified on utc value. |

### Schema: TechnicianJobDetailResponse
<a id="schema-technicianjobdetailresponse"></a>

- Source file: `Backend/Coolzo.Contracts/Responses/TechnicianJobs/TechnicianJobDetailResponse.cs`
- Shape: `record-positional`

| Field | Wire Type | Required | Description |
| --- | --- | --- | --- |
| `serviceRequestId` | `integer(int64)` | `Required` | Unique service request identifier. |
| `serviceRequestNumber` | `string` | `Required` | Business number/reference for service request. |
| `lifecycleType` | `string` | `Required` | Mapped lifecycle type value. |
| `lifecycleLabel` | `string` | `Required` | Mapped lifecycle label value. |
| `bookingId` | `integer(int64)` | `Required` | Unique booking identifier. |
| `bookingReference` | `string` | `Required` | Reference value for booking. |
| `currentStatus` | `string` | `Required` | Current status value for current. |
| `customerName` | `string` | `Required` | Display name for customer. |
| `mobileNumber` | `string` | `Required` | Business number/reference for mobile. |
| `addressSummary` | `string` | `Required` | Summary text for address. |
| `zoneName` | `string` | `Required` | Display name for zone. |
| `serviceName` | `string` | `Required` | Display name for service. |
| `acTypeName` | `string` | `Required` | Display name for ac type. |
| `tonnageName` | `string` | `Required` | Display name for tonnage. |
| `brandName` | `string` | `Required` | Display name for brand. |
| `modelName` | `string` | `Required` | Display name for model. |
| `issueNotes` | `string` | `Required` | Boolean flag indicating whether sue notes is true. |
| `slotDate` | `date` | `Required` | Date value for slot. |
| `slotLabel` | `string` | `Required` | Mapped slot label value. |
| `assignmentRemarks` | `string?` | `Optional` | Free-text remarks for assignment. |
| `jobCard` | `JobCardSummaryResponse` | `Required` | Mapped job card value. |
| `quotationId` | `integer(int64)?` | `Optional` | Unique quotation identifier. |
| `quotationNumber` | `string?` | `Optional` | Business number/reference for quotation. |
| `quotationStatus` | `string?` | `Optional` | Current status value for quotation. |
| `diagnosis` | `JobDiagnosisSummaryResponse` | `Required` | Mapped diagnosis value. |
| `checklistSummary` | `JobChecklistSummaryResponse` | `Required` | Summary text for checklist. |
| `checklistItems` | `JobChecklistItemResponse[]` | `Required` | Collection of `JobChecklistItemResponse` items. |
| `notes` | `JobExecutionNoteResponse[]` | `Required` | Collection of `JobExecutionNoteResponse` items. |
| `attachments` | `JobAttachmentResponse[]` | `Required` | Collection of `JobAttachmentResponse` items. |
| `timeline` | `JobExecutionTimelineItemResponse[]` | `Required` | Collection of `JobExecutionTimelineItemResponse` items. |
| `supportAlert` | `SupportTicketJobAlertResponse` | `Required` | Mapped support alert value. |
| `allowedActions` | `string[]` | `Required` | Collection of `string` items. |

### Schema: TechnicianJobListItemResponse
<a id="schema-technicianjoblistitemresponse"></a>

- Source file: `Backend/Coolzo.Contracts/Responses/TechnicianJobs/TechnicianJobListItemResponse.cs`
- Shape: `record-positional`

| Field | Wire Type | Required | Description |
| --- | --- | --- | --- |
| `serviceRequestId` | `integer(int64)` | `Required` | Unique service request identifier. |
| `jobCardId` | `integer(int64)?` | `Optional` | Unique job card identifier. |
| `serviceRequestNumber` | `string` | `Required` | Business number/reference for service request. |
| `jobCardNumber` | `string?` | `Optional` | Business number/reference for job card. |
| `lifecycleType` | `string` | `Required` | Mapped lifecycle type value. |
| `lifecycleLabel` | `string` | `Required` | Mapped lifecycle label value. |
| `bookingReference` | `string` | `Required` | Reference value for booking. |
| `customerName` | `string` | `Required` | Display name for customer. |
| `mobileNumber` | `string` | `Required` | Business number/reference for mobile. |
| `addressSummary` | `string` | `Required` | Summary text for address. |
| `serviceName` | `string` | `Required` | Display name for service. |
| `currentStatus` | `string` | `Required` | Current status value for current. |
| `slotDate` | `date` | `Required` | Date value for slot. |
| `slotLabel` | `string` | `Required` | Mapped slot label value. |

### Schema: TechnicianListItemResponse
<a id="schema-technicianlistitemresponse"></a>

- Source file: `Backend/Coolzo.Contracts/Responses/Operations/TechnicianListItemResponse.cs`
- Shape: `record-positional`

| Field | Wire Type | Required | Description |
| --- | --- | --- | --- |
| `technicianId` | `integer(int64)` | `Required` | Unique technician identifier. |
| `technicianCode` | `string` | `Required` | Code value for technician. |
| `technicianName` | `string` | `Required` | Display name for technician. |
| `mobileNumber` | `string` | `Required` | Business number/reference for mobile. |
| `emailAddress` | `string` | `Required` | Mapped email address value. |
| `baseZoneName` | `string?` | `Optional` | Display name for base zone. |
| `isActive` | `boolean` | `Required` | Boolean flag indicating whether active is true. |
| `maxDailyAssignments` | `integer(int32)` | `Required` | Mapped max daily assignments value. |
| `activeAssignments` | `integer(int32)` | `Required` | Mapped active assignments value. |

### Schema: TechnicianOnboardingDetailResponse
<a id="schema-technicianonboardingdetailresponse"></a>

- Source file: `Backend/Coolzo.Contracts/Responses/GapPhaseE/TechnicianOnboardingPhaseEResponses.cs`
- Shape: `record-positional`

| Field | Wire Type | Required | Description |
| --- | --- | --- | --- |
| `technicianId` | `integer(int64)` | `Required` | Unique technician identifier. |
| `technicianCode` | `string` | `Required` | Code value for technician. |
| `technicianName` | `string` | `Required` | Display name for technician. |
| `mobileNumber` | `string` | `Required` | Business number/reference for mobile. |
| `emailAddress` | `string` | `Required` | Mapped email address value. |
| `baseZoneId` | `integer(int64)?` | `Optional` | Unique base zone identifier. |
| `maxDailyAssignments` | `integer(int32)` | `Required` | Mapped max daily assignments value. |
| `isActive` | `boolean` | `Required` | Boolean flag indicating whether active is true. |
| `onboardingStatus` | `string` | `Required` | Current status value for onboarding. |
| `isActivationEligible` | `boolean` | `Required` | Boolean flag indicating whether activation eligible is true. |
| `pendingEligibilityItems` | `string[]` | `Required` | Collection of `string` items. |
| `documents` | `TechnicianDocumentDetailResponse[]` | `Required` | Collection of `TechnicianDocumentDetailResponse` items. |
| `skillAssessments` | `SkillAssessmentDetailResponse[]` | `Required` | Collection of `SkillAssessmentDetailResponse` items. |
| `trainingRecords` | `TrainingRecordDetailResponse[]` | `Required` | Collection of `TrainingRecordDetailResponse` items. |
| `activationHistory` | `TechnicianActivationLogResponse[]` | `Required` | Collection of `TechnicianActivationLogResponse` items. |

### Schema: TechnicianOnboardingListItemResponse
<a id="schema-technicianonboardinglistitemresponse"></a>

- Source file: `Backend/Coolzo.Contracts/Responses/GapPhaseE/TechnicianOnboardingPhaseEResponses.cs`
- Shape: `record-positional`

| Field | Wire Type | Required | Description |
| --- | --- | --- | --- |
| `technicianId` | `integer(int64)` | `Required` | Unique technician identifier. |
| `technicianCode` | `string` | `Required` | Code value for technician. |
| `technicianName` | `string` | `Required` | Display name for technician. |
| `mobileNumber` | `string` | `Required` | Business number/reference for mobile. |
| `emailAddress` | `string` | `Required` | Mapped email address value. |
| `isActive` | `boolean` | `Required` | Boolean flag indicating whether active is true. |
| `onboardingStatus` | `string` | `Required` | Current status value for onboarding. |
| `uploadedDocumentCount` | `integer(int32)` | `Required` | Mapped uploaded document count value. |
| `verifiedDocumentCount` | `integer(int32)` | `Required` | Mapped verified document count value. |
| `latestAssessmentResult` | `string` | `Required` | Mapped latest assessment result value. |
| `completedTrainingCount` | `integer(int32)` | `Required` | Mapped completed training count value. |
| `isActivationEligible` | `boolean` | `Required` | Boolean flag indicating whether activation eligible is true. |

### Schema: TechnicianOnboardingResponse
<a id="schema-technicianonboardingresponse"></a>

- Source file: `Backend/Coolzo.Contracts/Responses/GapPhaseA/GapPhaseAResponses.cs`
- Shape: `record-positional`

| Field | Wire Type | Required | Description |
| --- | --- | --- | --- |
| `technicianId` | `integer(int64)` | `Required` | Unique technician identifier. |
| `technicianCode` | `string` | `Required` | Code value for technician. |
| `technicianName` | `string` | `Required` | Display name for technician. |
| `isActive` | `boolean` | `Required` | Boolean flag indicating whether active is true. |
| `documentCount` | `integer(int32)` | `Required` | Mapped document count value. |
| `latestAssessmentResult` | `string` | `Required` | Mapped latest assessment result value. |
| `completedTrainingCount` | `integer(int32)` | `Required` | Mapped completed training count value. |

### Schema: TechnicianPerformanceResponse
<a id="schema-technicianperformanceresponse"></a>

- Source file: `Backend/Coolzo.Contracts/Responses/Analytics/AnalyticsResponses.cs`
- Shape: `record-positional`

| Field | Wire Type | Required | Description |
| --- | --- | --- | --- |
| `totalTechnicians` | `integer(int64)` | `Required` | Mapped total technicians value. |
| `activeTechnicians` | `integer(int64)` | `Required` | Mapped active technicians value. |
| `totalAssignedJobs` | `integer(int64)` | `Required` | Mapped total assigned jobs value. |
| `totalCompletedJobs` | `integer(int64)` | `Required` | Mapped total completed jobs value. |
| `averageCompletionHours` | `number(decimal)` | `Required` | Mapped average completion hours value. |
| `technicians` | `TechnicianPerformanceItemResponse[]` | `Required` | Collection of `TechnicianPerformanceItemResponse` items. |

### Schema: TechnicianStockResponse
<a id="schema-technicianstockresponse"></a>

- Source file: `Backend/Coolzo.Contracts/Responses/Inventory/TechnicianStockResponse.cs`
- Shape: `record-positional`

| Field | Wire Type | Required | Description |
| --- | --- | --- | --- |
| `technicianId` | `integer(int64)` | `Required` | Unique technician identifier. |
| `technicianCode` | `string` | `Required` | Code value for technician. |
| `technicianName` | `string` | `Required` | Display name for technician. |
| `totalSkuCount` | `integer(int32)` | `Required` | Mapped total sku count value. |
| `totalQuantityOnHand` | `number(decimal)` | `Required` | Mapped total quantity on hand value. |
| `items` | `TechnicianStockItemResponse[]` | `Required` | Collection of `TechnicianStockItemResponse` items. |

### Schema: TonnageLookupResponse
<a id="schema-tonnagelookupresponse"></a>

- Source file: `Backend/Coolzo.Contracts/Responses/Booking/TonnageLookupResponse.cs`
- Shape: `record-positional`

| Field | Wire Type | Required | Description |
| --- | --- | --- | --- |
| `tonnageId` | `integer(int64)` | `Required` | Unique tonnage identifier. |
| `tonnageName` | `string` | `Required` | Display name for tonnage. |
| `description` | `string` | `Required` | Mapped description value. |

### Schema: TrainingRecordDetailResponse
<a id="schema-trainingrecorddetailresponse"></a>

- Source file: `Backend/Coolzo.Contracts/Responses/GapPhaseE/TechnicianOnboardingPhaseEResponses.cs`
- Shape: `record-positional`

| Field | Wire Type | Required | Description |
| --- | --- | --- | --- |
| `trainingRecordId` | `integer(int64)` | `Required` | Unique training record identifier. |
| `trainingTitle` | `string` | `Required` | Mapped training title value. |
| `trainingType` | `string` | `Required` | Mapped training type value. |
| `trainingStatus` | `string` | `Required` | Current status value for training. |
| `certificationNumber` | `string` | `Required` | Business number/reference for certification. |
| `scorePercentage` | `number(decimal)` | `Required` | Mapped score percentage value. |
| `isCompleted` | `boolean` | `Required` | Boolean flag indicating whether completed is true. |
| `trainingCompletionDateUtc` | `datetime?` | `Optional` | UTC timestamp for training completion date. |
| `trainerUserId` | `integer(int64)?` | `Optional` | Unique trainer user identifier. |
| `certificateUrl` | `string` | `Required` | Mapped certificate url value. |
| `remarks` | `string` | `Required` | Free-text remarks for the action. |

### Schema: TransferStockRequest
<a id="schema-transferstockrequest"></a>

- Source file: `Backend/Coolzo.Contracts/Requests/Inventory/TransferStockRequest.cs`
- Shape: `record-positional`

| Field | Wire Type | Required | Description |
| --- | --- | --- | --- |
| `sourceWarehouseId` | `integer(int64)` | `Required` | Unique source warehouse identifier. |
| `destinationWarehouseId` | `integer(int64)` | `Required` | Unique destination warehouse identifier. |
| `itemId` | `integer(int64)` | `Required` | Unique item identifier. |
| `quantity` | `number(decimal)` | `Required` | Mapped quantity value. |
| `unitCost` | `number(decimal)` | `Required` | Mapped unit cost value. |
| `referenceNumber` | `string?` | `Optional` | Business number/reference for reference. |
| `remarks` | `string?` | `Optional` | Free-text remarks for the action. |

### Schema: UpdateAmcPlanRequest
<a id="schema-updateamcplanrequest"></a>

- Source file: `Backend/Coolzo.Contracts/Requests/Amc/UpdateAmcPlanRequest.cs`
- Shape: `record-positional`

| Field | Wire Type | Required | Description |
| --- | --- | --- | --- |
| `planName` | `string` | `Required` | Display name for plan. |
| `planDescription` | `string?` | `Optional` | Mapped plan description value. |
| `durationInMonths` | `integer(int32)` | `Required` | Mapped duration in months value. |
| `visitCount` | `integer(int32)` | `Required` | Mapped visit count value. |
| `priceAmount` | `number(decimal)` | `Required` | Monetary amount for price. |
| `isActive` | `boolean` | `Required` | Boolean flag indicating whether active is true. |
| `termsAndConditions` | `string?` | `Optional` | Mapped terms and conditions value. |

### Schema: UpdateItemRequest
<a id="schema-updateitemrequest"></a>

- Source file: `Backend/Coolzo.Contracts/Requests/Inventory/UpdateItemRequest.cs`
- Shape: `record-positional`

| Field | Wire Type | Required | Description |
| --- | --- | --- | --- |
| `categoryCode` | `string` | `Required` | Code value for category. |
| `categoryName` | `string` | `Required` | Display name for category. |
| `unitOfMeasureCode` | `string` | `Required` | Code value for unit of measure. |
| `unitOfMeasureName` | `string` | `Required` | Display name for unit of measure. |
| `supplierCode` | `string?` | `Optional` | Code value for supplier. |
| `supplierName` | `string?` | `Optional` | Display name for supplier. |
| `itemCode` | `string` | `Required` | Code value for item. |
| `itemName` | `string` | `Required` | Display name for item. |
| `itemDescription` | `string?` | `Optional` | Mapped item description value. |
| `purchasePrice` | `number(decimal)` | `Required` | Mapped purchase price value. |
| `sellingPrice` | `number(decimal)` | `Required` | Mapped selling price value. |
| `taxPercentage` | `number(decimal)` | `Required` | Mapped tax percentage value. |
| `warrantyDays` | `integer(int32)` | `Required` | Mapped warranty days value. |
| `reorderLevel` | `number(decimal)` | `Required` | Mapped reorder level value. |
| `isActive` | `boolean` | `Required` | Boolean flag indicating whether active is true. |

### Schema: UpdateLeadStatusRequest
<a id="schema-updateleadstatusrequest"></a>

- Source file: `Backend/Coolzo.Contracts/Requests/GapPhaseA/LeadManagementRequests.cs`
- Shape: `record-positional`

| Field | Wire Type | Required | Description |
| --- | --- | --- | --- |
| `leadStatus` | `string` | `Required` | Current status value for lead. |
| `remarks` | `string?` | `Optional` | Free-text remarks for the action. |
| `lostReason` | `string?` | `Optional` | Mapped lost reason value. |

### Schema: UpdateRefundStatusRequest
<a id="schema-updaterefundstatusrequest"></a>

- Source file: `Backend/Coolzo.Contracts/Requests/GapPhaseD/CancellationRefundRequests.cs`
- Shape: `record-positional`

| Field | Wire Type | Required | Description |
| --- | --- | --- | --- |
| `refundStatus` | `string` | `Required` | Current status value for refund. |
| `remarks` | `string` | `Required` | Free-text remarks for the action. |

### Schema: UpdateRoleRequest
<a id="schema-updaterolerequest"></a>

- Source file: `Backend/Coolzo.Contracts/Requests/Role/UpdateRoleRequest.cs`
- Shape: `record-positional`

| Field | Wire Type | Required | Description |
| --- | --- | --- | --- |
| `roleId` | `integer(int64)` | `Required` | Unique role identifier. |
| `displayName` | `string` | `Required` | Display name for display. |
| `description` | `string` | `Required` | Mapped description value. |
| `isActive` | `boolean` | `Required` | Boolean flag indicating whether active is true. |
| `permissionIds` | `long[]` | `Required` | List of permission identifiers. |

### Schema: UpdateServiceRequestStatusRequest
<a id="schema-updateservicerequeststatusrequest"></a>

- Source file: `Backend/Coolzo.Contracts/Requests/Operations/UpdateServiceRequestStatusRequest.cs`
- Shape: `record-positional`

| Field | Wire Type | Required | Description |
| --- | --- | --- | --- |
| `status` | `string` | `Required` | Current status value for the record. |
| `remarks` | `string?` | `Optional` | Free-text remarks for the action. |

### Schema: UpdateTechnicianJobStatusRequest
<a id="schema-updatetechnicianjobstatusrequest"></a>

- Source file: `Backend/Coolzo.Contracts/Requests/FieldExecution/UpdateTechnicianJobStatusRequest.cs`
- Shape: `record-positional`

| Field | Wire Type | Required | Description |
| --- | --- | --- | --- |
| `remarks` | `string?` | `Optional` | Free-text remarks for the action. |
| `workSummary` | `string?` | `Optional` | Summary text for work. |

### Schema: UpdateUserRequest
<a id="schema-updateuserrequest"></a>

- Source file: `Backend/Coolzo.Contracts/Requests/User/UpdateUserRequest.cs`
- Shape: `record-positional`

| Field | Wire Type | Required | Description |
| --- | --- | --- | --- |
| `userId` | `integer(int64)` | `Required` | Unique user identifier. |
| `email` | `string` | `Required` | Mapped email value. |
| `fullName` | `string` | `Required` | Display name for full. |
| `isActive` | `boolean` | `Required` | Boolean flag indicating whether active is true. |
| `roleIds` | `long[]` | `Required` | List of role identifiers. |

### Schema: UploadHelperTaskPhotoRequest
<a id="schema-uploadhelpertaskphotorequest"></a>

- Source file: `Backend/Coolzo.Contracts/Requests/GapPhaseE/HelperPhaseERequests.cs`
- Shape: `record-positional`

| Field | Wire Type | Required | Description |
| --- | --- | --- | --- |
| `fileName` | `string` | `Required` | Display name for file. |
| `contentType` | `string` | `Required` | Mapped content type value. |
| `base64Content` | `string` | `Required` | Mapped base64 content value. |
| `responseRemarks` | `string?` | `Optional` | Free-text remarks for response. |

### Schema: UploadTechnicianDocumentsRequest
<a id="schema-uploadtechniciandocumentsrequest"></a>

- Source file: `Backend/Coolzo.Contracts/Requests/GapPhaseA/TechnicianOnboardingRequests.cs`
- Shape: `record-positional`

| Field | Wire Type | Required | Description |
| --- | --- | --- | --- |
| `documents` | `TechnicianDocumentInput[]` | `Required` | Collection of `TechnicianDocumentInput` items. |

### Schema: UserResponse
<a id="schema-userresponse"></a>

- Source file: `Backend/Coolzo.Contracts/Responses/User/UserResponse.cs`
- Shape: `record-positional`

| Field | Wire Type | Required | Description |
| --- | --- | --- | --- |
| `userId` | `integer(int64)` | `Required` | Unique user identifier. |
| `userName` | `string` | `Required` | Display name for user. |
| `email` | `string` | `Required` | Mapped email value. |
| `fullName` | `string` | `Required` | Display name for full. |
| `isActive` | `boolean` | `Required` | Boolean flag indicating whether active is true. |
| `roleIds` | `long[]` | `Required` | List of role identifiers. |
| `roles` | `string[]` | `Required` | Collection of `string` items. |
| `dateCreated` | `datetime` | `Required` | Mapped date created value. |

### Schema: VerifyTechnicianDocumentRequest
<a id="schema-verifytechniciandocumentrequest"></a>

- Source file: `Backend/Coolzo.Contracts/Requests/GapPhaseE/TechnicianOnboardingPhaseERequests.cs`
- Shape: `record-positional`

| Field | Wire Type | Required | Description |
| --- | --- | --- | --- |
| `remarks` | `string?` | `Optional` | Free-text remarks for the action. |

### Schema: WarehouseResponse
<a id="schema-warehouseresponse"></a>

- Source file: `Backend/Coolzo.Contracts/Responses/Inventory/WarehouseResponse.cs`
- Shape: `record-positional`

| Field | Wire Type | Required | Description |
| --- | --- | --- | --- |
| `warehouseId` | `integer(int64)` | `Required` | Unique warehouse identifier. |
| `warehouseCode` | `string` | `Required` | Code value for warehouse. |
| `warehouseName` | `string` | `Required` | Display name for warehouse. |
| `contactPerson` | `string` | `Required` | Mapped contact person value. |
| `mobileNumber` | `string` | `Required` | Business number/reference for mobile. |
| `emailAddress` | `string` | `Required` | Mapped email address value. |
| `cityName` | `string` | `Required` | Display name for city. |
| `addressSummary` | `string` | `Required` | Summary text for address. |
| `isActive` | `boolean` | `Required` | Boolean flag indicating whether active is true. |
| `stockItemCount` | `integer(int32)` | `Required` | Mapped stock item count value. |
| `totalQuantityOnHand` | `number(decimal)` | `Required` | Mapped total quantity on hand value. |

### Schema: WarehouseStockResponse
<a id="schema-warehousestockresponse"></a>

- Source file: `Backend/Coolzo.Contracts/Responses/Inventory/WarehouseStockResponse.cs`
- Shape: `record-positional`

| Field | Wire Type | Required | Description |
| --- | --- | --- | --- |
| `warehouseId` | `integer(int64)` | `Required` | Unique warehouse identifier. |
| `warehouseCode` | `string` | `Required` | Code value for warehouse. |
| `warehouseName` | `string` | `Required` | Display name for warehouse. |
| `totalSkuCount` | `integer(int32)` | `Required` | Mapped total sku count value. |
| `totalQuantityOnHand` | `number(decimal)` | `Required` | Mapped total quantity on hand value. |
| `items` | `WarehouseStockItemResponse[]` | `Required` | Collection of `WarehouseStockItemResponse` items. |

### Schema: WarrantyClaimResponse
<a id="schema-warrantyclaimresponse"></a>

- Source file: `Backend/Coolzo.Contracts/Responses/Warranty/WarrantyClaimResponse.cs`
- Shape: `record-positional`

| Field | Wire Type | Required | Description |
| --- | --- | --- | --- |
| `warrantyClaimId` | `integer(int64)` | `Required` | Unique warranty claim identifier. |
| `invoiceId` | `integer(int64)` | `Required` | Unique invoice identifier. |
| `invoiceNumber` | `string` | `Required` | Business number/reference for invoice. |
| `customerId` | `integer(int64)` | `Required` | Unique customer identifier. |
| `customerName` | `string` | `Required` | Display name for customer. |
| `warrantyRuleId` | `integer(int64)?` | `Optional` | Unique warranty rule identifier. |
| `warrantyRuleName` | `string?` | `Optional` | Display name for warranty rule. |
| `coverageStartDateUtc` | `datetime` | `Required` | UTC timestamp for coverage start date. |
| `coverageEndDateUtc` | `datetime` | `Required` | UTC timestamp for coverage end date. |
| `isEligible` | `boolean` | `Required` | Boolean flag indicating whether eligible is true. |
| `currentStatus` | `string` | `Required` | Current status value for current. |
| `serviceName` | `string` | `Required` | Display name for service. |
| `claimRemarks` | `string` | `Required` | Free-text remarks for claim. |
| `claimDateUtc` | `datetime` | `Required` | UTC timestamp for claim date. |
| `revisitRequestId` | `integer(int64)?` | `Optional` | Unique revisit request identifier. |

### Schema: WarrantyStatusResponse
<a id="schema-warrantystatusresponse"></a>

- Source file: `Backend/Coolzo.Contracts/Responses/Warranty/WarrantyStatusResponse.cs`
- Shape: `record-positional`

| Field | Wire Type | Required | Description |
| --- | --- | --- | --- |
| `invoiceId` | `integer(int64)` | `Required` | Unique invoice identifier. |
| `invoiceNumber` | `string` | `Required` | Business number/reference for invoice. |
| `customerId` | `integer(int64)` | `Required` | Unique customer identifier. |
| `customerName` | `string` | `Required` | Display name for customer. |
| `serviceName` | `string` | `Required` | Display name for service. |
| `isWarrantyAvailable` | `boolean` | `Required` | Boolean flag indicating whether warranty available is true. |
| `isEligible` | `boolean` | `Required` | Boolean flag indicating whether eligible is true. |
| `eligibilityMessage` | `string` | `Required` | Mapped eligibility message value. |
| `coverageStartDateUtc` | `datetime?` | `Optional` | UTC timestamp for coverage start date. |
| `coverageEndDateUtc` | `datetime?` | `Optional` | UTC timestamp for coverage end date. |
| `warrantyRuleName` | `string?` | `Optional` | Display name for warranty rule. |
| `claims` | `WarrantyClaimResponse[]` | `Required` | Collection of `WarrantyClaimResponse` items. |

### Schema: ZoneLookupResponse
<a id="schema-zonelookupresponse"></a>

- Source file: `Backend/Coolzo.Contracts/Responses/Booking/ZoneLookupResponse.cs`
- Shape: `record-positional`

| Field | Wire Type | Required | Description |
| --- | --- | --- | --- |
| `zoneId` | `integer(int64)` | `Required` | Unique zone identifier. |
| `zoneName` | `string` | `Required` | Display name for zone. |
| `cityName` | `string` | `Required` | Display name for city. |
| `pincode` | `string` | `Required` | Code value for pin. |

### Schema: BlogContentResponse
<a id="schema-blogcontentresponse"></a>

- Source file: `Backend/Coolzo.Contracts/Responses/CMS/BlogContentResponse.cs`
- Shape: `record-positional`

| Field | Wire Type | Required | Description |
| --- | --- | --- | --- |
| `id` | `string` | `Required` | Content identifier/string key. |
| `title` | `string` | `Required` | Display title. |
| `excerpt` | `string` | `Required` | Short content summary. |
| `content` | `string` | `Required` | Full content body. |
| `author` | `string` | `Required` | Display author name. |
| `date` | `datetime` | `Required` | Published/content date. |
| `image` | `string` | `Required` | Image URL or asset key. |
| `category` | `string` | `Required` | Content category. |

### Schema: ChangelogItemResponse
<a id="schema-changelogitemresponse"></a>

- Source file: `Backend/Coolzo.Contracts/Responses/CMS/BlogContentResponse.cs`
- Shape: `record-positional`

| Field | Wire Type | Required | Description |
| --- | --- | --- | --- |
| `version` | `string` | `Required` | App/content version label. |
| `date` | `datetime` | `Required` | Release/change date. |
| `changes` | `string[]` | `Required` | Collection of change descriptions. |

### Schema: CreateCustomerAddressRequest
<a id="schema-createcustomeraddressrequest"></a>

- Source file: `Backend/Coolzo.Contracts/Requests/Customer/CreateCustomerAddressRequest.cs`
- Shape: `record-positional`

| Field | Wire Type | Required | Description |
| --- | --- | --- | --- |
| `addressLabel` | `string` | `Required` | Display label for the address. |
| `addressLine1` | `string` | `Required` | Primary address line. |
| `addressLine2` | `string` | `Required` | Secondary address line. |
| `landmark` | `string` | `Required` | Nearby landmark. |
| `cityName` | `string` | `Required` | Display city name. |
| `pincode` | `string` | `Required` | Postal/pincode value. |
| `zoneId` | `integer(int64)?` | `Optional` | Service zone identifier. |
| `latitude` | `number(double)?` | `Optional` | Latitude coordinate. |
| `longitude` | `number(double)?` | `Optional` | Longitude coordinate. |
| `isDefault` | `boolean` | `Required` | Boolean flag indicating whether this is the default address. |
| `stateName` | `string?` | `Optional` | Display state name. |
| `addressType` | `string?` | `Optional` | Address type/category. |

### Schema: CreateCustomerEquipmentRequest
<a id="schema-createcustomerequipmentrequest"></a>

- Source file: `Backend/Coolzo.Contracts/Requests/Customer/CustomerAppRequests.cs`
- Shape: `record-positional`

| Field | Wire Type | Required | Description |
| --- | --- | --- | --- |
| `name` | `string` | `Required` | Equipment display name. |
| `type` | `string` | `Required` | Equipment type. |
| `brand` | `string` | `Required` | Equipment brand. |
| `capacity` | `string` | `Required` | Capacity value/label. |
| `location` | `string` | `Required` | Installed/customer location. |
| `purchaseDate` | `date?` | `Optional` | Purchase date. |
| `lastServiceDate` | `date?` | `Optional` | Last service date. |
| `serialNumber` | `string?` | `Optional` | Equipment serial number. |

### Schema: CreateCustomerReviewRequest
<a id="schema-createcustomerreviewrequest"></a>

- Source file: `Backend/Coolzo.Contracts/Requests/Customer/CustomerAppRequests.cs`
- Shape: `record-positional`

| Field | Wire Type | Required | Description |
| --- | --- | --- | --- |
| `rating` | `integer(int32)` | `Required` | Numeric review rating. |
| `comment` | `string` | `Required` | Review comment text. |
| `bookingId` | `integer(int64)?` | `Optional` | Linked booking identifier. |
| `serviceId` | `integer(int64)?` | `Optional` | Linked service identifier. |
| `customerPhotoUrl` | `string?` | `Optional` | Customer photo URL. |

### Schema: CustomerAccountDeletionResponse
<a id="schema-customeraccountdeletionresponse"></a>

- Source file: `Backend/Coolzo.Contracts/Responses/Customer/CustomerAppResponses.cs`
- Shape: `record-positional`

| Field | Wire Type | Required | Description |
| --- | --- | --- | --- |
| `customerId` | `integer(int64)` | `Required` | Unique customer identifier. |
| `isActive` | `boolean` | `Required` | Boolean flag indicating whether active is true. |
| `deactivatedAtUtc` | `datetime` | `Required` | UTC timestamp for account deactivation. |
| `reason` | `string` | `Required` | Deactivation reason. |

### Schema: CustomerAddressResponse
<a id="schema-customeraddressresponse"></a>

- Source file: `Backend/Coolzo.Contracts/Responses/Customer/CustomerAddressResponse.cs`
- Shape: `record-positional`

| Field | Wire Type | Required | Description |
| --- | --- | --- | --- |
| `customerAddressId` | `integer(int64)` | `Required` | Unique customer address identifier. |
| `customerId` | `integer(int64)` | `Required` | Unique customer identifier. |
| `addressLabel` | `string` | `Required` | Display label for the address. |
| `addressLine1` | `string` | `Required` | Primary address line. |
| `addressLine2` | `string` | `Required` | Secondary address line. |
| `landmark` | `string` | `Required` | Nearby landmark. |
| `cityName` | `string` | `Required` | Display city name. |
| `stateName` | `string` | `Required` | Display state name. |
| `pincode` | `string` | `Required` | Postal/pincode value. |
| `addressType` | `string` | `Required` | Address type/category. |
| `zoneId` | `integer(int64)?` | `Optional` | Service zone identifier. |
| `latitude` | `number(double)?` | `Optional` | Latitude coordinate. |
| `longitude` | `number(double)?` | `Optional` | Longitude coordinate. |
| `isDefault` | `boolean` | `Required` | Boolean flag indicating whether default is true. |
| `isActive` | `boolean` | `Required` | Boolean flag indicating whether active is true. |
| `dateCreated` | `datetime` | `Required` | Created timestamp. |
| `lastUpdated` | `datetime?` | `Optional` | Last updated timestamp. |

### Schema: CustomerAppFeedbackResponse
<a id="schema-customerappfeedbackresponse"></a>

- Source file: `Backend/Coolzo.Contracts/Responses/Customer/CustomerAppResponses.cs`
- Shape: `record-positional`

| Field | Wire Type | Required | Description |
| --- | --- | --- | --- |
| `customerAppFeedbackId` | `integer(int64)` | `Required` | Unique customer app feedback identifier. |
| `customerId` | `integer(int64)` | `Required` | Unique customer identifier. |
| `feedbackType` | `string` | `Required` | Feedback type/category. |
| `message` | `string` | `Required` | Feedback message. |
| `rating` | `integer(int32)?` | `Optional` | Optional app/customer rating. |
| `appVersion` | `string` | `Required` | Customer app version. |
| `deviceInfo` | `string` | `Required` | Device information. |
| `feedbackStatus` | `string` | `Required` | Feedback workflow status. |
| `createdAt` | `datetime` | `Required` | Created timestamp. |

### Schema: CustomerEquipmentResponse
<a id="schema-customerequipmentresponse"></a>

- Source file: `Backend/Coolzo.Contracts/Responses/Customer/CustomerAppResponses.cs`
- Shape: `record-positional`

| Field | Wire Type | Required | Description |
| --- | --- | --- | --- |
| `customerEquipmentId` | `integer(int64)` | `Required` | Unique customer equipment identifier. |
| `customerId` | `integer(int64)` | `Required` | Unique customer identifier. |
| `name` | `string` | `Required` | Equipment display name. |
| `type` | `string` | `Required` | Equipment type. |
| `brand` | `string` | `Required` | Equipment brand. |
| `capacity` | `string` | `Required` | Capacity value/label. |
| `location` | `string` | `Required` | Installed/customer location. |
| `purchaseDate` | `date?` | `Optional` | Purchase date. |
| `lastServiceDate` | `date?` | `Optional` | Last service date. |
| `serialNumber` | `string` | `Required` | Equipment serial number. |
| `isActive` | `boolean` | `Required` | Boolean flag indicating whether active is true. |
| `dateCreated` | `datetime` | `Required` | Created timestamp. |
| `lastUpdated` | `datetime?` | `Optional` | Last updated timestamp. |

### Schema: CustomerNotificationResponse
<a id="schema-customernotificationresponse"></a>

- Source file: `Backend/Coolzo.Contracts/Responses/Customer/CustomerAppResponses.cs`
- Shape: `record-positional`

| Field | Wire Type | Required | Description |
| --- | --- | --- | --- |
| `customerNotificationId` | `integer(int64)` | `Required` | Unique customer notification identifier. |
| `customerId` | `integer(int64)` | `Required` | Unique customer identifier. |
| `title` | `string` | `Required` | Notification title. |
| `message` | `string` | `Required` | Notification message. |
| `type` | `string` | `Required` | Notification type/category. |
| `isRead` | `boolean` | `Required` | Boolean flag indicating whether read is true. |
| `createdAt` | `datetime` | `Required` | Created timestamp. |
| `link` | `string` | `Required` | Notification target link. |

### Schema: CustomerProfileResponse
<a id="schema-customerprofileresponse"></a>

- Source file: `Backend/Coolzo.Contracts/Responses/Customer/CustomerAppResponses.cs`
- Shape: `record-positional`

| Field | Wire Type | Required | Description |
| --- | --- | --- | --- |
| `customerId` | `integer(int64)` | `Required` | Unique customer identifier. |
| `userId` | `integer(int64)?` | `Optional` | Linked user identifier. |
| `customerName` | `string` | `Required` | Display name for customer. |
| `mobileNumber` | `string` | `Required` | Customer mobile number. |
| `emailAddress` | `string` | `Required` | Customer email address. |
| `photoUrl` | `string` | `Required` | Profile photo URL. |
| `membershipStatus` | `string` | `Required` | Customer membership status. |
| `isActive` | `boolean` | `Required` | Boolean flag indicating whether active is true. |
| `dateCreated` | `datetime` | `Required` | Created timestamp. |
| `lastUpdated` | `datetime?` | `Optional` | Last updated timestamp. |

### Schema: CustomerReviewResponse
<a id="schema-customerreviewresponse"></a>

- Source file: `Backend/Coolzo.Contracts/Responses/Customer/CustomerAppResponses.cs`
- Shape: `record-positional`

| Field | Wire Type | Required | Description |
| --- | --- | --- | --- |
| `customerReviewId` | `integer(int64)` | `Required` | Unique customer review identifier. |
| `customerId` | `integer(int64)` | `Required` | Unique customer identifier. |
| `userName` | `string` | `Required` | Customer/user display name. |
| `userPhoto` | `string` | `Required` | Customer/user photo URL. |
| `rating` | `integer(int32)` | `Required` | Numeric review rating. |
| `comment` | `string` | `Required` | Review comment text. |
| `bookingId` | `integer(int64)?` | `Optional` | Linked booking identifier. |
| `serviceId` | `integer(int64)?` | `Optional` | Linked service identifier. |
| `createdAt` | `datetime` | `Required` | Created timestamp. |

### Schema: FlagFeedbackRequest
<a id="schema-flagfeedbackrequest"></a>

- Source file: `Backend/Coolzo.Contracts/Requests/Support/FlagFeedbackRequest.cs`
- Shape: `record-positional`

| Field | Wire Type | Required | Description |
| --- | --- | --- | --- |
| `reason` | `string` | `Required` | Support moderation reason captured when the review is flagged. |

### Schema: CustomerVisibleTechnicianResponse
<a id="schema-customervisibletechnicianresponse"></a>

- Source file: `Backend/Coolzo.Contracts/Responses/Customer/CustomerAppResponses.cs`
- Shape: `record-positional`

| Field | Wire Type | Required | Description |
| --- | --- | --- | --- |
| `technicianId` | `integer(int64)` | `Required` | Unique technician identifier. |
| `name` | `string` | `Required` | Technician display name. |
| `photoUrl` | `string` | `Required` | Technician photo URL. |
| `rating` | `number(decimal)` | `Required` | Customer-visible technician rating. |
| `totalJobs` | `integer(int32)` | `Required` | Total completed/displayed jobs. |
| `experience` | `string` | `Required` | Experience summary. |
| `specialization` | `string[]` | `Required` | Collection of specialization labels. |
| `languages` | `string[]` | `Required` | Collection of language labels. |
| `verified` | `boolean` | `Required` | Boolean flag indicating whether verified is true. |

### Schema: DeleteCustomerAccountRequest
<a id="schema-deletecustomeraccountrequest"></a>

- Source file: `Backend/Coolzo.Contracts/Requests/Customer/CustomerAppRequests.cs`
- Shape: `record-positional`

| Field | Wire Type | Required | Description |
| --- | --- | --- | --- |
| `reason` | `string?` | `Optional` | Customer-provided deactivation reason. |

### Schema: LoyaltyPointsResponse
<a id="schema-loyaltypointsresponse"></a>

- Source file: `Backend/Coolzo.Contracts/Responses/Customer/CustomerAppResponses.cs`
- Shape: `record-positional`

| Field | Wire Type | Required | Description |
| --- | --- | --- | --- |
| `balance` | `integer(int32)` | `Required` | Current points balance. |
| `tier` | `string` | `Required` | Current loyalty tier. |
| `nextTierPoints` | `integer(int32)` | `Required` | Points required for next tier. |

### Schema: LoyaltyTransactionResponse
<a id="schema-loyaltytransactionresponse"></a>

- Source file: `Backend/Coolzo.Contracts/Responses/Customer/CustomerAppResponses.cs`
- Shape: `record-positional`

| Field | Wire Type | Required | Description |
| --- | --- | --- | --- |
| `customerLoyaltyTransactionId` | `integer(int64)` | `Required` | Unique loyalty transaction identifier. |
| `type` | `string` | `Required` | Transaction type. |
| `points` | `integer(int32)` | `Required` | Points credited or debited. |
| `description` | `string` | `Required` | Transaction description. |
| `createdAt` | `datetime` | `Required` | Created timestamp. |

### Schema: PromotionalOfferResponse
<a id="schema-promotionalofferresponse"></a>

- Source file: `Backend/Coolzo.Contracts/Responses/Customer/CustomerAppResponses.cs`
- Shape: `record-positional`

| Field | Wire Type | Required | Description |
| --- | --- | --- | --- |
| `promotionalOfferId` | `integer(int64)` | `Required` | Unique promotional offer identifier. |
| `code` | `string` | `Required` | Offer/coupon code. |
| `title` | `string` | `Required` | Offer title. |
| `description` | `string` | `Required` | Offer description. |
| `discountType` | `string` | `Required` | Discount type. |
| `discountValue` | `number(decimal)` | `Required` | Discount value. |
| `minOrderValue` | `number(decimal)` | `Required` | Minimum order value. |
| `expiryDate` | `date?` | `Optional` | Offer expiry date. |
| `category` | `string` | `Required` | Offer category. |

### Schema: PublishFeedbackRequest
<a id="schema-publishfeedbackrequest"></a>

- Source file: `Backend/Coolzo.Contracts/Requests/Support/PublishFeedbackRequest.cs`
- Shape: `record-positional`

| Field | Wire Type | Required | Description |
| --- | --- | --- | --- |
| `publish` | `boolean` | `Required` | Boolean flag indicating whether the review should be publicly published. |

### Schema: ReferralResponse
<a id="schema-referralresponse"></a>

- Source file: `Backend/Coolzo.Contracts/Responses/Customer/CustomerAppResponses.cs`
- Shape: `record-positional`

| Field | Wire Type | Required | Description |
| --- | --- | --- | --- |
| `customerReferralId` | `integer(int64)` | `Required` | Unique customer referral identifier. |
| `name` | `string` | `Required` | Referral display name. |
| `status` | `string` | `Required` | Referral status. |
| `reward` | `number(decimal)` | `Required` | Referral reward amount. |
| `date` | `date` | `Required` | Referral date. |

### Schema: ReferralStatsResponse
<a id="schema-referralstatsresponse"></a>

- Source file: `Backend/Coolzo.Contracts/Responses/Customer/CustomerAppResponses.cs`
- Shape: `record-positional`

| Field | Wire Type | Required | Description |
| --- | --- | --- | --- |
| `referralCode` | `string` | `Required` | Customer referral code. |
| `totalReferrals` | `integer(int32)` | `Required` | Total referral count. |
| `totalEarnings` | `number(decimal)` | `Required` | Total referral earnings. |
| `pendingReferrals` | `integer(int32)` | `Required` | Pending referral count. |
| `referrals` | `ReferralResponse[]` | `Required` | Collection of [`ReferralResponse`](#schema-referralresponse) items. |

### Schema: RescheduleCustomerBookingRequest
<a id="schema-reschedulecustomerbookingrequest"></a>

- Source file: `Backend/Coolzo.Contracts/Requests/Customer/CustomerAppRequests.cs`
- Shape: `record-positional`

| Field | Wire Type | Required | Description |
| --- | --- | --- | --- |
| `slotAvailabilityId` | `integer(int64)` | `Required` | Target slot availability identifier. |
| `remarks` | `string?` | `Optional` | Free-text reschedule remarks. |

### Schema: SubmitAppFeedbackRequest
<a id="schema-submitappfeedbackrequest"></a>

- Source file: `Backend/Coolzo.Contracts/Requests/Customer/CustomerAppRequests.cs`
- Shape: `record-positional`

| Field | Wire Type | Required | Description |
| --- | --- | --- | --- |
| `feedbackType` | `string?` | `Optional` | Feedback type/category. |
| `message` | `string` | `Required` | Feedback message. |
| `rating` | `integer(int32)?` | `Optional` | Optional app/customer rating. |
| `appVersion` | `string?` | `Optional` | Customer app version. |
| `deviceInfo` | `string?` | `Optional` | Device information. |

### Schema: RespondFeedbackRequest
<a id="schema-respondfeedbackrequest"></a>

- Source file: `Backend/Coolzo.Contracts/Requests/Support/RespondFeedbackRequest.cs`
- Shape: `record-positional`

| Field | Wire Type | Required | Description |
| --- | --- | --- | --- |
| `response` | `string` | `Required` | Admin response text stored against the feedback record; blank clears the response. |

### Schema: UpdateCustomerAddressRequest
<a id="schema-updatecustomeraddressrequest"></a>

- Source file: `Backend/Coolzo.Contracts/Requests/Customer/UpdateCustomerAddressRequest.cs`
- Shape: `record-positional`

| Field | Wire Type | Required | Description |
| --- | --- | --- | --- |
| `customerAddressId` | `integer(int64)` | `Required` | Customer address identifier included in the DTO. |
| `addressLabel` | `string` | `Required` | Display label for the address. |
| `addressLine1` | `string` | `Required` | Primary address line. |
| `addressLine2` | `string` | `Required` | Secondary address line. |
| `landmark` | `string` | `Required` | Nearby landmark. |
| `cityName` | `string` | `Required` | Display city name. |
| `pincode` | `string` | `Required` | Postal/pincode value. |
| `zoneId` | `integer(int64)?` | `Optional` | Service zone identifier. |
| `latitude` | `number(double)?` | `Optional` | Latitude coordinate. |
| `longitude` | `number(double)?` | `Optional` | Longitude coordinate. |
| `isDefault` | `boolean` | `Required` | Boolean flag indicating whether this is the default address. |
| `stateName` | `string?` | `Optional` | Display state name. |
| `addressType` | `string?` | `Optional` | Address type/category. |

### Schema: UpdateCustomerEquipmentRequest
<a id="schema-updatecustomerequipmentrequest"></a>

- Source file: `Backend/Coolzo.Contracts/Requests/Customer/CustomerAppRequests.cs`
- Shape: `record-positional`

| Field | Wire Type | Required | Description |
| --- | --- | --- | --- |
| `customerEquipmentId` | `integer(int64)` | `Required` | Customer equipment identifier included in the DTO. |
| `name` | `string` | `Required` | Equipment display name. |
| `type` | `string` | `Required` | Equipment type. |
| `brand` | `string` | `Required` | Equipment brand. |
| `capacity` | `string` | `Required` | Capacity value/label. |
| `location` | `string` | `Required` | Installed/customer location. |
| `purchaseDate` | `date?` | `Optional` | Purchase date. |
| `lastServiceDate` | `date?` | `Optional` | Last service date. |
| `serialNumber` | `string?` | `Optional` | Equipment serial number. |

### Schema: UpdateCustomerProfileRequest
<a id="schema-updatecustomerprofilerequest"></a>

- Source file: `Backend/Coolzo.Contracts/Requests/Customer/CustomerAppRequests.cs`
- Shape: `record-positional`

| Field | Wire Type | Required | Description |
| --- | --- | --- | --- |
| `customerName` | `string` | `Required` | Display name for customer. |
| `mobileNumber` | `string` | `Required` | Customer mobile number. |
| `emailAddress` | `string` | `Required` | Customer email address. |
| `photoUrl` | `string?` | `Optional` | Profile photo URL. |
| `membershipStatus` | `string?` | `Optional` | Customer membership status. |

### Schema: ValidateCouponRequest
<a id="schema-validatecouponrequest"></a>

- Source file: `Backend/Coolzo.Contracts/Requests/Customer/CustomerAppRequests.cs`
- Shape: `record-positional`

| Field | Wire Type | Required | Description |
| --- | --- | --- | --- |
| `code` | `string` | `Required` | Coupon/offer code to validate. |

### Schema: object
<a id="schema-object"></a>

- Source file: Runtime/anonymous payload
- Shape: `object`

| Field | Wire Type | Required | Description |
| --- | --- | --- | --- |
| `value` | `object` | `Optional` | Endpoint-specific object payload; see the endpoint row for concrete fields when applicable. |
