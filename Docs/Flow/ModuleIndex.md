## Module: Project Identity

- Section: SECTION 1
- Keywords: brand, roles, RBAC, platform, surfaces, color, design

ProjectOverview reference: SECTION 1 — PROJECT IDENTITY & ARCHITECTURE in `Docs/ProjectOverview.md`

API names: (none — this module documents identity and design guidance only)

DB tables: (none)

Screens: (none)

Dependencies: None

## Module: Service Request Management

- Section: SECTION 2
- Keywords: SR, service-request, dispatch, admin, operations, timeline, cancellation, escalation, no-show

ProjectOverview reference: SECTION 2 — SERVICE REQUEST & OPERATIONS in `Docs/Flow/ProjectOverview.md`

API names:
  ServiceRequestController (/api/service-requests):
    Get SRs List, Get SR Detail, Create SR from Booking, Update SR Status,
    Save SR Note, Get Operations Dashboard Summary
  AssignmentController (/api/service-requests):
    Assign Technician, Reassign Technician, Get Assignment History
  CancellationController (/api/cancellations):
    Create Customer Cancellation, Create Admin Cancellation,
    Get Cancellation Options, Get Cancellations List, Get Cancellation Detail,
    Cancel SR (legacy admin route)
  EscalationController (/api/escalations):
    Create Escalation, Handle No-Show

DB tables: ServiceRequests, SRStatusHistory, SRAssignments, JobExecutionNotes,
           CancellationRecords, CancellationPolicies, SystemAlerts,
           CustomerAbsentRecords, JobReports, JobPhotos, JobChecklists, CustomerSignatures

Screens: SR List (SRListScreen.tsx), SR Detail (SRDetailScreen.tsx),
         Create SR (CreateSRScreen.tsx), Assign Technician Panel,
         Operations Dashboard (OperationsDashboardScreen.tsx),
         Dispatch Management (DispatchManagementScreen.tsx),
         Escalate SR, SR Notes

Dependencies: Technician Management, Scheduling, Inventory, Billing, Notifications, Customers, Booking Engine

## Module: Field Workflow

- Section: SECTION 2
- Keywords: field, technician, mobile, checklist, gps, photos, job-report, attendance, parts-request, estimate, signature, payment, offline-queue, completion-gate
- Updated: 2026-05-24 — Stable Contract Level

ProjectOverview reference: SECTION 2 — MODULE: Field Workflow — inside `Docs/Flow/ProjectOverview.md`
  Heading: "## MODULE: Field Workflow (Technician On-Site Execution)"

Controllers:
  FieldWorkflowController (/api/field, Authorize Roles=Technician) — PRIMARY (17 endpoints)
  FieldExecutionController (/api/technician-jobs, Authorize Roles=Technician) — DEPRECATED (8 endpoints)

API names and routes:
  GET  /api/field/my-jobs                        — Flow 1: Get My Active Jobs
  GET  /api/field/job-history                    — Flow 2: Get Job History
  GET  /api/field/jobs/{id}                      — Flow 3: Get Job Detail
  PATCH /api/field/jobs/{id}/depart              — Flow 4: Mark Departed (En Route)
  PATCH /api/field/jobs/{id}/arrive              — Flow 5: Arrive (GPS 150m gate, override supported)
  PATCH /api/field/jobs/{id}/start-work          — Flow 6: Start Work
  PATCH /api/field/jobs/{id}/progress            — Flow 7: Save Checklist Progress
  POST  /api/field/jobs/{id}/parts-request       — Flow 8: Submit Parts Request
  POST  /api/field/jobs/{id}/estimate            — Flow 9: Create Field Estimate (Quotation)
  POST  /api/field/jobs/{id}/report              — Flow 10: Submit Job Report (IdempotencyKey)
  POST  /api/field/jobs/{id}/photos              — Flow 11: Upload Job Photo (Base64)
  POST  /api/field/jobs/{id}/signature           — Flow 12: Save Customer Signature
  PATCH /api/field/jobs/{id}/payment             — Flow 13: Collect Field Payment (IdempotencyKey)
  PATCH /api/field/jobs/{id}/complete            — Flow 14: Complete Job (gated)
  POST  /api/field/attendance/check-in           — Flow 15: Attendance Check-In
  POST  /api/field/attendance/check-out          — Flow 16: Attendance Check-Out

Deprecated:
  POST  /api/technician-jobs/mark-enroute, mark-reached, start-work, mark-in-progress,
        mark-work-completed, submit-for-closure, notes; GET /api/technician-jobs/timeline

DB tables:
  tblJobReport — job completion reports
  tblJobPhoto — field photos (min 2 required for completion)
  tblCustomerSignature — customer digital signature (required for completion)
  tblOfflineSyncQueueItem — offline queue persistence
  tblPartsRequest, tblPartsRequestItem — field parts requests
  ServiceRequests — status updated at each step
  JobCards — timestamps (departed/arrived/started/completed)
  TechnicianAttendance — daily check-in/check-out

Completion Gate (enforced by /complete):
  1. tblJobReport row exists for SR
  2. tblJobPhoto count ≥ 2 for SR
  3. tblCustomerSignature row exists for SR

Arrival override (enforced by /arrive):
  - If technician GPS > 150m from CustomerAddress → 422 OverrideRequired=true + distanceMeters
  - Client must re-call /arrive with OverrideReason to proceed

IdempotencyKeys:
  - POST /report: prevents duplicate report on offline retry (FieldJobReportRequest.IdempotencyKey)
  - PATCH /payment: prevents duplicate charge (FieldJobPaymentRequest.IdempotencyKey)

Screens: AttendanceScreen.tsx, TechnicianJobReportScreen.tsx (wrapper over JobWorkflowContainer),
  OfflineSyncQueue.tsx (/system/sync)

Offline: FIELD_OFFLINE_QUEUE (StorageKey) → field-workflow-repository.ts → syncSubmission(id)

Dependencies: Service Request Management, Inventory, Notifications, Offline Sync Engine, File/Attachment Storage

## Module: Customer Master (Customer 360)

- Section: SECTION 3
- Keywords: customer, customer-360, customer-management, addresses, equipment, notes, profile, deactivate

ProjectOverview reference: SECTION 3 — CUSTOMER, EQUIPMENT & AMC in `Docs/Flow/ProjectOverview.md`

API names:
  CustomerController (/api/customers) — Admin:
    Get Customers List, Create Customer, Get Customer Detail (360),
    Update Customer, Get Customer Addresses, Create Customer Address, Update Customer Address,
    Get Customer Equipment, Create Customer Equipment, Update Customer Equipment,
    Add Customer Note, Reset Customer Password
  CustomerController (/api/customers) — Customer self-service:
    Get My Profile, Update My Profile, Deactivate My Account
  CustomerAddressController (/api/customers) — Customer self-service:
    Get My Addresses, Create My Address, Update My Address, Delete My Address

DB tables: Customers, CustomerAddresses, CustomerEquipment, CustomerNotes,
           Users, PasswordHistory, RefreshTokens

Screens: Customer List (CustomerListScreen.tsx), Customer 360 View (Customer360ViewScreen.tsx),
         Create Customer (CreateCustomerScreen.tsx), Edit Customer, Add/Edit Address,
         Customer Notes, Customer Communication Panel

Dependencies: Service Request Management, Billing, Notifications, AMC Engine, Authentication

## Module: Equipment Register

- Section: SECTION 3
- Keywords: equipment, equipment-register, serial, warranty, installation, customer-equipment
- Stable Contract: YES — 7 endpoints across 2 controllers (verified 2026-05-24)

ProjectOverview reference: SECTION 3 — MODULE: Equipment Register in `Docs/ProjectOverview.md`

Controllers:
  CustomerController        — /api/customers/{customerId}/equipment   (3 admin endpoints)
  CustomerEquipmentController — /api/customers/me/equipment           (4 customer portal endpoints)

API routes:
  GET    /api/customers/{customerId}/equipment                  — Flow EQP-1 (Policy=UserRead)
  POST   /api/customers/{customerId}/equipment                  — Flow EQP-2 (Policy=UserUpdate)
  PUT    /api/customers/{customerId}/equipment/{equipmentId}    — Flow EQP-3 (Policy=UserUpdate)
  GET    /api/customers/me/equipment                            — Flow EQP-4 (Authorize — JWT)
  POST   /api/customers/me/equipment                            — Flow EQP-5 (Authorize — JWT)
  PUT    /api/customers/me/equipment/{equipmentId}              — Flow EQP-6 (Authorize — JWT)
  DELETE /api/customers/me/equipment/{equipmentId}              — Flow EQP-7 (Authorize — JWT, soft delete)

DB tables (confirmed):
  tblCustomerEquipment

REMOVED from old docs (wrong/phantom):
  - "SoftDelete Equipment" (admin) → no admin DELETE endpoint; only customer portal DELETE
  - "Get Equipment History" → no dedicated endpoint; equipment in CustomerController detail response
  - "Get Booking Brands", "Get My Service History" → not in equipment controllers
  - JobReports, WarrantyRecords, PartsRequests → NOT equipment module tables
  - EquipmentTag, Model (separate), AddressId FK, InstallationYear, WarrantyExpiryDate, Supplier, InvoiceNumber, Notes → NOT in actual DTO

Key fields in CustomerEquipmentResponse:
  CustomerEquipmentId, CustomerId, Name, Type, Brand, Capacity (string), Location (string),
  PurchaseDate (DateOnly?), LastServiceDate (DateOnly?), SerialNumber, IsActive, DateCreated, LastUpdated?

Key notes:
  Location = free-text string (NOT FK to CustomerAddress)
  Capacity = string e.g. "1.5 Ton" (NOT a numeric tonnage field)
  WarrantyExpiryDate NOT on equipment — see Module 11 (Warranty Management)
  Admin deactivates equipment via PUT (set IsActive=false); Customer uses DELETE (soft delete)

Screens: Customer 360 Equipment Tab, My Equipment (Customer Portal), Add/Edit Appliance

Dependencies: Customer Master, Warranty Management (Module 11) for warranty expiry tracking

## Module: AMC Contract Engine

- Section: SECTION 3
- Keywords: amc, contract, enrollment, visits, renewal, plan, generate-visits, customer-amc
- Stable Contract: YES — 8 endpoints (verified 2026-05-24)

ProjectOverview reference: SECTION 3 — MODULE: AMC Contract Engine in `Docs/ProjectOverview.md`

Controller: AmcController — /api/amc

API routes:
  POST   /api/amc/plans                                   — Flow AMC-1 (Policy=AmcCreate)
  PUT    /api/amc/plans/{amcPlanId}                        — Flow AMC-2 (Policy=AmcCreate)
  GET    /api/amc/plans                                    — Flow AMC-3 (Authorize)
  GET    /api/amc/plans/{amcPlanId}                        — Flow AMC-4 (Authorize)
  POST   /api/amc/assign                                   — Flow AMC-5 (Policy=AmcAssign)
  POST   /api/amc/customer/{customerAmcId}/generate-visits — Flow AMC-6 (Policy=AmcAssign)
  GET    /api/amc/customer/{customerId}                    — Flow AMC-7 (Authorize)
  GET    /api/amc/customer/me                              — Flow AMC-8 (Authorize — customer JWT)

DB tables (confirmed):
  tblAmcPlan, tblCustomerAmc, tblAmcVisitSchedule

REMOVED from old docs (phantom/wrong):
  - AMCPlans → tblAmcPlan
  - AMCContracts → tblCustomerAmc (no ContractNumber, no EquipmentCovered, no BillingCycle)
  - AMCVisitSchedule → tblAmcVisitSchedule
  - AMCVisitLog → NOT a separate table; tracking via tblAmcVisitSchedule.CompletedDateUtc
  - "Create Contract Visit", "Get Contract Visits", "Renew Contract" → phantom endpoints
  - AutoRenewEnabled, RenewalDate, RenewalAlertSent → NOT in DTOs

Key: AMC enrollment = POST /api/amc/assign (requires JobCardId + InvoiceId mandatory)
     Visit generation = POST /api/amc/customer/{id}/generate-visits (spreads visits over duration)
     Customer self-service = GET /api/amc/customer/me (JWT-scoped)

Screens: AMC Contract List, Contract Detail, Enroll Customer (admin), My AMC (customer portal),
  Visit Schedule View

Dependencies: Customer Master, JobCard (required on enroll), Invoice (required on enroll),
  Scheduling (visits become SRs), Billing, Notifications

## Module: Warranty Management

- Section: SECTION 3
- Keywords: warranty, warranty-claim, warranty-rule, revisit, coverage, eligibility, invoice-warranty
- Stable Contract: YES — 2 endpoints (verified 2026-05-24)

ProjectOverview reference: SECTION 3 — MODULE: Warranty Management in `Docs/ProjectOverview.md`

Controller: WarrantyController — /api/warranty

API routes:
  POST   /api/warranty/claim                   — Flow WRN-1 (Authorize — any authenticated)
  GET    /api/warranty/invoice/{invoiceId}      — Flow WRN-2 (Authorize — any authenticated)

DB tables (confirmed from DTO shapes):
  tblWarrantyClaim, tblWarrantyRule

CRITICAL MODEL DRIFT CORRECTED:
  Old docs: warranty = equipment-centric (EquipmentId FK, serial check on SR creation)
  Actual:   warranty = INVOICE-CENTRIC (InvoiceId FK — claim raised against a completed job)
  Old "WarrantyRecords" table → actual: tblWarrantyClaim + tblWarrantyRule
  Old routes "GET/POST /api/warranty/records" → phantom; actual routes listed above

Key fields:
  WarrantyClaimResponse: WarrantyClaimId, InvoiceId, InvoiceNumber, CustomerId, CustomerName,
    WarrantyRuleId?, WarrantyRuleName?, CoverageStartDateUtc, CoverageEndDateUtc, IsEligible,
    CurrentStatus, ServiceName, ClaimRemarks, ClaimDateUtc, RevisitRequestId?
  WarrantyStatusResponse: InvoiceId, InvoiceNumber, IsWarrantyAvailable, IsEligible,
    EligibilityMessage, CoverageStartDateUtc?, CoverageEndDateUtc?, WarrantyRuleName?, Claims[]

Key rules:
  WarrantyRule is configurable per service type (30-day repair / 90-day parts = default configs)
  IsEligible = claim within CoverageStartDateUtc–CoverageEndDateUtc window
  On approved claim: revisit SR created and linked via RevisitRequestId

Screens: Invoice Detail → Warranty tab, Booking Detail → Warranty (customer portal),
  Raise Warranty Claim, Warranty Status

Dependencies: Billing (InvoiceId required), Service Request Management (revisit SR creation),
  Configuration (WarrantyRule setup)

## Module: Booking Engine

- Section: SECTION 4
- Keywords: booking, slots, calendar, guest-booking, customer-booking, reschedule, service-types, service-report

ProjectOverview reference: SECTION 4 — BOOKING ENGINE in `Docs/Flow/ProjectOverview.md`

API names:
  BookingLookupController (/api/booking-lookups) — all AllowAnonymous:
    Get Service Categories, Get Services, Get AC Types, Get Tonnages, Get Brands,
    Get Zones, Get Zone by Pincode, Get Available Slots
  ServiceTypesController (/api/service-types) — all AllowAnonymous:
    Get Service Types (list), Get Service Type Detail, Get Service Sub-Types
  BookingController (/api/bookings):
    Create Guest Booking, Create Customer Booking, Get Booking Detail (admin),
    Search Bookings (admin list), Get My Bookings, Reschedule Booking
  CustomerBookingController (/api/customer-bookings):
    Get Customer Booking Detail, Get Customer Bookings List,
    Reschedule Customer Booking, Get Service Report, Download Service Report PDF

DB tables: Bookings, BookingStatusHistory, BookingLines,
           ServiceSlots, SlotAvailability,
           Services, ServiceCategories, AcTypes, Tonnages, Brands, Zones

Screens: Booking Wizard Steps 1–6, Booking Confirmation, Slot Unavailable,
         My Bookings List, Booking Detail / Job Tracker, Service Report View,
         Reschedule Booking

Dependencies: Customers, Service Request Management, Scheduling,
              Notifications, Coupons & Discounts, Auth (OTP for guest verify)

## Module: Billing & Payments

- Section: SECTION 5
- Keywords: billing, invoice, payments, ar, quotation, estimate, refund, receipt, pdf, mark-paid
- Updated: 2026-05-24 — Stable Contract Level

ProjectOverview reference: SECTION 5 — BILLING, INVOICE & PAYMENT in `Docs/Flow/ProjectOverview.md`
  Headings: "## MODULE: Invoice Engine", "## MODULE: Payment Engine",
            "## MODULE: Billing Dashboard & AR", "## MODULE: Quotation / Estimate Engine",
            "## MODULE: Refund Engine"

Controllers:
  InvoiceController (/api/invoices) — 6 endpoints
  PaymentController (/api/payments) — 6 endpoints
  BillingController (/api/billing) — 3 endpoints
  QuotationController (/api/quotations) — 6 endpoints [was ENTIRELY MISSING]
  RefundController (/api/refunds) — 8 endpoints [was ENTIRELY MISSING]

API routes:
  InvoiceController (/api/invoices):
    POST  from-quotation/{quotationId}   — Flow 1: Generate Invoice from Quotation (X-Idempotency-Key)
    GET                                  — Flow 2: Search Invoices (status?, customerId?, page)
    GET   {id}                           — Flow 3: Get Invoice Detail
    POST  {id}/mark-paid                 — Flow 4: Mark Invoice Paid (InvoiceMarkPaidRequest)
    GET   {id}/pdf                       — Flow 5: Download Invoice PDF (text-based, no external lib)
    GET   customer                       — Flow 6: Get Customer Invoices (JWT-filtered)

  PaymentController (/api/payments):
    POST                                 — Flow 7: Initiate Payment Gateway Session
    POST  collect                        — Flow 8: Collect/Record Payment (IsWebhookEvent support)
    GET   invoice/{invoiceId}            — Flow 9: Get Payments by Invoice
    GET   {paymentId}                    — Flow 10: Get Payment/Gateway Status
    GET   receipt/{invoiceId}            — Flow 11: Get Payment Receipt
    GET   receipt/{invoiceId}/pdf        — Flow 12: Download Receipt PDF

  BillingController (/api/billing):
    GET   accounts-receivable            — Flow 13: AR Dashboard (aging + overdue + top debtors)
    GET   status/{invoiceId}             — Flow 14: Get Billing Status
    POST  payment-reminders/send         — Flow 15: Send Payment Reminder (queued)

  QuotationController (/api/quotations):
    POST  from-job/{jobCardId}           — Flow 16: Create Quotation from Job (Technician)
    GET                                  — Flow 17: Search Quotations
    GET   {id}                           — Flow 18: Get Quotation by ID
    GET   job/{jobCardId}                — Flow 19: Get Quotation by Job Card
    POST  {id}/approve                   — Flow 20: Approve Quotation
    POST  {id}/reject                    — Flow 21: Reject Quotation

  RefundController (/api/refunds):
    POST  request                        — Flow 22: Create Refund Request
    GET                                  — Flow 23: List Refund Requests
    GET   {id}                           — Flow 24: Get Refund Detail
    GET   customer/{customerId}          — Flow 25: Customer Refund Status
    POST                                 — Flow 26: Initiate Refund (simplified)
    POST  {refundRequestId}/approve      — Flow 27: Approve Refund
    POST  {refundRequestId}/reject       — Flow 28: Reject Refund
    POST  {refundRequestId}/status       — Flow 29: Update Refund Status

DB tables:
  Invoices, InvoiceLines, BillingStatusHistory
  PaymentTransactions, PaymentReceipts
  Quotations, QuotationLines
  RefundRequests, RefundApprovalHistory, RefundStatusHistory

PHANTOM ENDPOINTS REMOVED:
  PUT /api/invoices/{id} — never existed
  POST /api/invoices/{id}/send — never existed
  POST /api/invoices/{id}/credit-note — never existed
  POST /api/payments/reconcile — never existed
  POST /api/coupons/apply — in CustomerMarketingController, not Billing
  GET /api/coupons/{code} — never existed
  GET /api/tax-configurations — never existed

Coupon validation: POST /api/customer-marketing/offers/validate-coupon (see Customer Portal module)

Screens: Invoice List, Invoice Detail, AR Dashboard, Quotation List, Refund Management, Mark as Paid

Dependencies: Service Request Management, Field Workflow, Customer Master, Notifications, Cancellation Engine

## Module: Estimates & Work Orders

- Section: SECTION 6
- Keywords: estimates, quotation, work-orders, job-attachment, job-checklist, diagnosis, parts-consumption, job-execution
- Updated: 2026-05-24 — Stable Contract Level

ProjectOverview reference: SECTION 6 — MODULE: Estimates & Work Orders (Job Execution Support)
  Heading: "## MODULE: Estimates & Work Orders (Job Execution Support)"

Architecture:
  - Estimate/Quotation engine is in Section 5 → QuotationController (/api/quotations), Flows 16–21
  - Work Orders: NOT IMPLEMENTED — no WorkOrderController exists
  - Job Reports admin review: NOT IMPLEMENTED — /api/job-reports/* are phantom routes

Controllers:
  JobAttachmentController (/api/technician-jobs, Roles=Technician) — 2 endpoints
  JobChecklistController (/api/technician-jobs, Roles=Technician) — 2 endpoints
  DiagnosisController (/api and /api/technician-jobs, Roles=Technician) — 3 endpoints
  JobConsumptionController (/api/jobs, policy-based) — 2 endpoints

API routes:
  POST  /api/technician-jobs/{id}/attachments   — Flow 1: Save Job Attachment
  GET   /api/technician-jobs/{id}/attachments   — Flow 2: Get Job Attachments
  GET   /api/technician-jobs/{id}/checklist     — Flow 3: Get Job Checklist
  POST  /api/technician-jobs/{id}/checklist     — Flow 4: Save Checklist Responses
  GET   /api/diagnosis/lookups/issues           — Flow 5: Get Diagnosis Issue Lookups
  GET   /api/diagnosis/lookups/results          — Flow 6: Get Diagnosis Result Lookups
  POST  /api/technician-jobs/{id}/diagnosis     — Flow 7: Save Job Diagnosis
  POST  /api/jobs/{jobCardId}/consume-parts     — Flow 8: Record Job Parts Consumption (Policy=JobConsumptionCreate)
  GET   /api/jobs/{jobCardId}/consumption       — Flow 9: Get Job Parts Consumption (Policy=JobConsumptionRead)

PHANTOM ROUTES REMOVED:
  GET /api/estimates, POST /api/estimates, GET /api/estimates/{id}
  PATCH /api/estimates/{id}/send, approve, reject, override-approve, resend
  GET /api/estimates/expiry-queue
  GET /api/workorders, POST /api/workorders, GET /api/workorders/{id}
  GET /api/job-reports, GET /api/job-reports/{id}
  PATCH /api/job-reports/{id}/approve, flag
  GET /api/job-reports/quality-dashboard

DB tables:
  JobAttachments
  JobChecklistResponses, ServiceChecklistMaster
  JobDiagnosis, ComplaintIssueMaster, DiagnosisResultMaster
  JobPartConsumptions, WarehouseStock, StockTransactions

PHANTOM DB tables removed (not confirmed in source):
  Estimates, EstimateLineItems, WorkOrders, EstimateApprovalHistory

IDs-only rule: ItemId (numeric) is the only valid join key for parts/inventory.
  ItemCode (e.g. "CAP-45") is display-only — never use as route param or request field.

Screens: Job Detail (Attachment/Checklist/Diagnosis tabs), Parts Consumed tab

Dependencies: Service Request Management, Field Workflow, Inventory, Billing (Quotation via Section 5)

## Module: Inventory & Parts Management

- Section: SECTION 6
- Keywords: inventory, parts, stock, purchase-order, parts-request, warehouse, supplier, van-stock, parts-return, supplier-claim
- Stable Contract: YES — 34 endpoints across 7 controllers (verified 2026-05-24)

ProjectOverview reference: SECTION 6 — MODULE: Inventory & Parts Management in `Docs/ProjectOverview.md`

Controllers:
  InventoryController  — /api/inventory       (19 endpoints)
  ItemController       — /api/items            (4 endpoints)
  WarehouseController  — /api/warehouses       (3 endpoints)
  StockController      — /api/stock            (3 endpoints)
  PartsReturnController — /api/parts-returns   (2 endpoints)
  SupplierController   — /api/suppliers        (1 endpoint — claims only)
  TechnicianStockController — /api/technicians/{id}/stock* (2 endpoints)

API routes:
  GET    /api/inventory/dashboard                         — Flow INV-1 (Policy=ItemRead)
  GET    /api/inventory/parts                             — Flow INV-2 (Policy=ItemRead)
  GET    /api/inventory/parts/{id}                        — Flow INV-3 (Policy=ItemRead)
  POST   /api/inventory/parts                             — Flow INV-4 (Policy=ItemCreate)
  PUT    /api/inventory/parts/{id}                        — Flow INV-5 (Policy=ItemCreate)
  GET    /api/inventory/parts-requests                    — Flow INV-6 (Policy=ItemRead)
  GET    /api/inventory/parts-requests/{id}               — Flow INV-7 (Policy=ItemRead)
  PATCH  /api/inventory/parts-requests/{id}/approve       — Flow INV-8 (Policy=StockManage)
  PATCH  /api/inventory/parts-requests/{id}/partial       — Flow INV-9 (Policy=StockManage)
  PATCH  /api/inventory/parts-requests/{id}/reject        — Flow INV-10 (Policy=StockManage)
  GET    /api/inventory/low-stock-alerts                  — Flow INV-11 (Policy=ItemRead)
  GET    /api/inventory/stock-movements                   — Flow INV-12 (Policy=StockRead)
  POST   /api/inventory/stock-adjust                      — Flow INV-13 (Policy=StockManage)
  GET    /api/inventory/purchase-orders                   — Flow INV-14 (Policy=ItemRead)
  GET    /api/inventory/purchase-orders/{id}              — Flow INV-15 (Policy=ItemRead)
  POST   /api/inventory/purchase-orders                   — Flow INV-16 (Policy=StockManage)
  PATCH  /api/inventory/purchase-orders/{id}/receive      — Flow INV-17 (Policy=StockManage)
  GET    /api/inventory/suppliers                         — Flow INV-18 (Policy=ItemRead)
  POST   /api/inventory/suppliers                         — Flow INV-19 (Policy=ItemCreate)
  POST   /api/items                                       — Flow ITM-1 (Policy=ItemCreate)
  PUT    /api/items/{id}                                  — Flow ITM-2 (Policy=ItemCreate)
  GET    /api/items                                       — Flow ITM-3 (Policy=ItemRead)
  GET    /api/items/{id}                                  — Flow ITM-4 (Policy=ItemRead)
  POST   /api/warehouses                                  — Flow WH-1 (Policy=WarehouseCreate)
  GET    /api/warehouses                                  — Flow WH-2 (Policy=WarehouseRead)
  GET    /api/warehouses/{id}/stock                       — Flow WH-3 (Policy=WarehouseRead)
  POST   /api/stock/transaction                           — Flow STK-1 (Policy=StockManage)
  POST   /api/stock/transfer                              — Flow STK-2 (Policy=StockManage)
  GET    /api/stock/transactions                          — Flow STK-3 (Policy=StockRead)
  POST   /api/parts-returns                               — Flow RTN-1 (Policy=StockManage)
  POST   /api/parts-returns/{partsReturnId}/approve       — Flow RTN-2 (Policy=StockManage)
  POST   /api/suppliers/claims                            — Flow SUP-1 (Policy=StockManage)
  POST   /api/technicians/{id}/stock-assign               — Flow TKS-1 (Policy=StockManage)
  GET    /api/technicians/{id}/stock                      — Flow TKS-2 (Policy=StockRead)

DB tables (confirmed from EF Core DbContext):
  tblItem, tblItemCategory, tblItemRate, tblWarehouse, tblWarehouseStock,
  tblStockTransaction, tblPartsRequest, tblPartsRequestItem,
  tblPurchaseOrder, tblPurchaseOrderItem, tblSupplier, tblTechnicianVanStock,
  tblPartsReturn

REMOVED from old docs (phantom/wrong):
  - PartsCategories → tblItemCategory
  - PartsCatalog → tblItem
  - StockLedger → tblStockTransaction
  - PartsRequests → tblPartsRequest
  - PartsRequestItems → tblPartsRequestItem
  - PurchaseOrders → tblPurchaseOrder
  - POItems → tblPurchaseOrderItem
  - CompatibleBrands/CompatibleModels JSON on item → not in schema
  - SupplierIds (JSON) on item → single SupplierId FK

Key rules:
  IDs-only: PartId in all requests = ItemId.ToString() (numeric string); PartCode is display-only.
  PO status enum: Draft | Submitted | Confirmed | PartiallyReceived | FullyReceived | Cancelled
  Stock types: PurchaseIn/TransferIn/ReturnIn → IN; JobConsumption/TransferOut/ReturnOut → OUT; AdjustmentIn/Out → ADJ
  Parts approval deducts tblWarehouseStock immediately; creates StockTransaction(JobConsumption).
  PO receipt increments tblWarehouseStock; creates StockTransaction(PurchaseIn).

Screens: Inventory Dashboard, Parts Catalog List, Part Detail, Add/Edit Part, Parts Requests Queue,
  Parts Request Detail, Stock Movement Ledger, Purchase Order List, PO Detail, Low Stock Alerts,
  Supplier Management, Warehouse Stock, Technician Van Stock

Dependencies: Service Request Management, Field Workflow (PartsRequest from mobile → INV-8/9/10),
  Billing (JobConsumption feeds into Quotation lines), Technician Management (van stock)

## Module: Customer Portal

- Section: SECTION 7
- Keywords: customer-portal, dashboard, bookings, job-tracker, support, profile

ProjectOverview reference: SECTION 7 — CUSTOMER PORTAL in `Docs/ProjectOverview.md`

API names: Get Dashboard, Get Bookings, Get Booking Timeline, Reschedule Booking, Get Estimates, Approve Estimate, Tickets CRUD, Update Profile, Search Service Types, Get Promotional Offers, Validate Coupon, Get Referral Stats, Get Loyalty Points, Get Loyalty Transactions, Get Customer Reviews, Create Customer Review, Get Customer Invoices, Get Invoice Detail, Get Unread Notifications, Mark Notification Read, Mark All Notifications Read, Get My Communication Preferences, Update My Communication Preferences, Get My Support Tickets, Get My Support Ticket Count, Get Support Ticket Detail, Add Support Ticket Reply, Close Support Ticket, Get Support Ticket Categories, Get Support Ticket Priorities, Get Public Technician Profile, Get CMS Block By Key, Get Blog Posts (Customer), Get Blog Post Detail (Customer), Delete My Customer Account, Download Customer Service Report PDF, Get Customer Receipt, Download Customer Receipt PDF, Submit App Feedback

DB tables: Customers, CustomerAddresses, ServiceRequests, Estimates, Tickets, Notifications, JobReports, CustomerReviews

Screens: Customer Dashboard, My Bookings List, Booking Detail / Job Tracker, Service Report View, Reschedule Booking, Estimate Approval, My Support Tickets, My Profile, Notification Preferences

Dependencies: Booking Engine, Service Request Management, AMC, Billing, Notifications

## Module: Admin Operations

- Section: SECTION 7
- Keywords: operations-dashboard, dispatch, scheduling, technician-board, sla, map

ProjectOverview reference: SECTION 7 — ADMIN OPERATIONS in `Docs/ProjectOverview.md`

API names: Get Operations Summary, Get Pending Queue, Get Technician Status, Get SLA Alerts, Get Zone Workload, Get Operations Day Summary, Get Live Map, Assign Technician, Get Technician Availability, Get Scheduling Board, Reschedule Job

DB tables: ServiceRequests, SRAssignments, TechnicianStatusLog, ScheduleSlots, JobStatusHistory, SLAAlerts, DispatchLogs

Screens: Operations Dashboard, Dispatch / Assignment Screen, Reassignment Panel, Scheduling Board (Day/Week), Technician Shift Scheduler, Live Job Map, Super Admin Business Dashboard

Dependencies: Technicians, Service Request Management, Booking Engine, Notifications, Geo / Mapping Services

## Module: Dispatch Management

- Section: SECTION 7
- Keywords: dispatch, assign, reassign, emergency, workload

ProjectOverview reference: SECTION 7 — ADMIN OPERATIONS in `Docs/ProjectOverview.md`

API names: Get Unassigned SRs, Assign Technician, Reassign SR, Batch Assign

DB tables: SRAssignments, DispatchLogs, TechnicianZones

Screens: Dispatch/Assignment Screen, Emergency SR Alert Handler, Zone Workload View, Operations Day Summary

Dependencies: Technician Profiles, Scheduling, Notifications

## Module: Technician Management

- Section: SECTION 7 (ADMIN OPERATIONS)
- Keywords: technician, skills, zones, attendance, leave, availability-board, gps-log, helper, onboarding, documents, activation, technician-jobs
- Stable Contract: YES — 40 endpoints across 9 controllers (verified 2026-05-24)

ProjectOverview reference: SECTION 7 — MODULE: Technician Management in `Docs/ProjectOverview.md`

Controllers:
  TechnicianController          — /api/technicians               (12 endpoints)
  TechnicianPublicController    — /api/technicians/{id}/public    (1 endpoint, AllowAnonymous)
  TechnicianActivationController— /api/technicians/{id}          (3 endpoints)
  TechnicianDocumentController  — /api/technicians/{id}/documents (4 endpoints)
  TechnicianOnboardingController— /api/technician-onboarding      (5 endpoints — legacy onboarding)
  TechnicianJobController       — /api/technician-jobs            (3 endpoints, Roles=Technician)
  HelperController              — /api/helpers                    (7 endpoints)
  HelperAttendanceController    — /api/helpers/{id}/attendance    (3 endpoints)
  HelperTaskController          — /api/helpers/{id}/tasks         (3 endpoints)
  TechnicianStockController     — /api/technicians/{id}/stock*    (2 endpoints — see Module 8)

Key routes:
  GET    /api/technicians                               — TCH-1 (Policy=TechnicianRead)
  GET    /api/technicians/{id}                          — TCH-2 (Policy=TechnicianRead)
  POST   /api/technicians                               — TCH-3 (Policy=UserCreate)
  PUT    /api/technicians/{id}                          — TCH-4 (Policy=UserUpdate)
  GET    /api/technicians/{id}/performance              — TCH-5 (Policy=TechnicianRead)
  GET    /api/technicians/{id}/attendance               — TCH-6 (Policy=TechnicianRead)
  POST   /api/technicians/{id}/attendance/leave         — TCH-7 (Policy=UserUpdate)
  PATCH  /api/technicians/{id}/attendance/leave/{lid}   — TCH-8 (Policy=UserUpdate)
  GET    /api/technicians/availability-board            — TCH-9 (Policy=TechnicianRead)
  GET    /api/technicians/availability                  — TCH-9 alias (same handler)
  GET    /api/technicians/{id}/gps-log                  — TCH-10 (Policy=TechnicianRead)
  PATCH  /api/technicians/{id}/skills                   — TCH-11 (Policy=UserUpdate)
  PATCH  /api/technicians/{id}/zones                    — TCH-12 (Policy=UserUpdate)
  GET    /api/technicians/{id}/public                   — TCH-13 (AllowAnonymous)
  POST   /api/technicians/{id}/activate                 — TCH-14 (Policy=UserUpdate)
  POST   /api/technicians/{id}/deactivate               — TCH-15 (Policy=UserUpdate)
  GET    /api/technicians/{id}/activation-history       — TCH-16 (Policy=TechnicianRead)
  POST   /api/technicians/{id}/documents                — TCH-17 (Policy=UserUpdate)
  GET    /api/technicians/{id}/documents                — TCH-18 (Policy=TechnicianRead)
  POST   /api/technicians/{id}/documents/{did}/verify   — TCH-19 (Policy=UserUpdate)
  POST   /api/technicians/{id}/documents/{did}/reject   — TCH-20 (Policy=UserUpdate)
  POST   /api/technician-onboarding/draft               — TCH-21 (Policy=UserCreate)
  GET    /api/technician-onboarding                     — TCH-22 (Policy=TechnicianRead)
  GET    /api/technician-onboarding/{id}                — TCH-23 (Policy=TechnicianRead)
  POST   /api/technician-onboarding/{id}/documents      — TCH-24 (Policy=UserUpdate)
  POST   /api/technician-onboarding/{id}/activate       — TCH-25 (Policy=UserUpdate — legacy)
  GET    /api/technician-jobs                           — TCH-26 (Roles=Technician)
  GET    /api/technician-jobs/{id}                      — TCH-27 (Roles=Technician)
  GET    /api/technician-jobs/my-jobs                   — TCH-28 (Roles=Technician)
  POST   /api/helpers                                   — HLP-1
  GET    /api/helpers                                   — HLP-2
  GET    /api/helpers/{id}                              — HLP-3
  PUT    /api/helpers/{id}                              — HLP-4
  POST   /api/helpers/{id}/assign                       — HLP-5
  POST   /api/helpers/{id}/release                      — HLP-6
  GET    /api/helpers/{id}/assignment                   — HLP-7
  POST   /api/helpers/{id}/attendance/check-in          — HLP-8
  POST   /api/helpers/{id}/attendance/check-out         — HLP-9
  GET    /api/helpers/{id}/attendance                   — HLP-10
  GET    /api/helpers/{id}/tasks                        — HLP-11
  POST   /api/helpers/{id}/tasks/{tid}/respond          — HLP-12
  POST   /api/helpers/{id}/tasks/{tid}/upload-photo     — HLP-13

DB tables (confirmed):
  tblTechnician, tblTechnicianSkill, tblTechnicianZone, tblTechnicianAttendance,
  tblTechnicianGpsLog, tblTechnicianDocument, tblSkillAssessment, tblTrainingRecord,
  tblTechnicianActivationLog, tblHelperProfile, tblHelperAssignment, tblHelperAttendance

REMOVED from old docs:
  - TechnicianStatusLog → NOT a table; AvailabilityStatus derived from job state
  - TechnicianPerformanceSummary → NOT a persistent table; computed on query

Key rules:
  Skill/Zone updates use soft-delete + reactivation (NOT duplicate insert)
  Unique filtered indexes: (TechnicianId, SkillName|ZoneId) WHERE IsDeleted=0
  Onboarding path: POST /api/technician-onboarding/draft → upload docs → activate (legacy)
  Modern path: POST /api/technicians → PATCH skills/zones → POST activate
  Helper DTOs (HelperDetailResponse etc.) → [VERIFY full shape from GapPhaseE]

Dependencies: Dispatch Management, Scheduling Board, Field Workflow (Technician mobile jobs),
  Inventory (van stock), Master Data (zones, skills), Customer Portal (public profile)

## Module: Scheduling Board

- Section: SECTION 8
- Keywords: scheduling, calendar, drag-and-drop, conflict-detection, shifts

ProjectOverview reference: SECTION 8 — ADMIN OPERATIONS in `Docs/ProjectOverview.md`

API names: GET /api/scheduling/board, POST /api/scheduling/assign, PUT /api/scheduling/reassign, GET /api/scheduling/conflicts, GET /api/scheduling/slots, PUT /api/scheduling/slots/{slotAvailabilityId}, GET /api/scheduling/shifts, PUT /api/scheduling/shifts, GET /api/scheduling/amc-auto, POST /api/scheduling/amc-bulk-assign, GET /api/scheduling/day-sheet

DB tables: tblSlotAvailability, tblTechnicianShift, tblServiceRequestStatusHistory

Screens: Scheduling Board Day View, Week View, Technician View, Conflict Detection Alert, Slot Availability Manager, Technician Shift Scheduler, AMC Auto-Schedule Review Board, Daily Briefing Sheet Export

 Dependencies: AMC Engine, Dispatch, Geo / Travel Time Service

## Module: Master Data & Configuration

- Section: SECTION 9
- Keywords: master-data, configuration, zones, pricing, sla, role-permissions, cms

ProjectOverview reference: SECTION 9 — CONFIGURATION, NOTIFICATIONS & AUDIT in `Docs/ProjectOverview.md`

API names: Get Configs, Update Config, Get Zones, Get Service Types, Get Pricing, Get SLA Configs, Get Holiday Calendar, Upload CMS Block

DB tables: SystemConfigs, Zones, EquipmentCatalog, JobStatusWorkflow, SLAConfigurations, BusinessHours, HolidayCalendar

Screens: Master Data List, Service Type Editor, Zone Manager, Pricing Matrix, SLA Config Editor, Role & Permission Manager

Dependencies: Super Admin, Admin Portal, Billing, Scheduling, Notifications

## Module: Notifications & Communication

- Section: SECTION 9
- Keywords: notifications, templates, whatsapp, sms, email, push, delivery-logs

ProjectOverview reference: SECTION 9 — CONFIGURATION, NOTIFICATIONS & AUDIT in `Docs/ProjectOverview.md`

API names: Get Notification Templates, Create/Update Template, Get Triggers, Update Trigger Channels, Send Test Notification, Get Notification Logs

DB tables: NotificationTemplates, NotificationTriggers, NotificationLog, UserNotificationPreferences, PushNotificationQueue

Screens: Notification Triggers Map, Template Editor, Channel Mapping, Send Test Message, Delivery Logs, Communication Preferences

Dependencies: Customers, Bookings, Billing, Technicians, CMS

## Module: CMS & Content

- Section: SECTION 9
- Keywords: cms, banners, pages, blog, faqs, scheduling, content-versioning

ProjectOverview reference: SECTION 9 — CONFIGURATION, NOTIFICATIONS & AUDIT in `Docs/ProjectOverview.md`

API names: Get CMS Blocks, Create/Update CMS Block, Get Banners, Create Banner, Get Blog Posts, Publish Post

DB tables: CMSBlocks, CMSBlockVersions, Banners, FAQs, BlogPosts, Reviews

Screens: CMS Dashboard, Banner Manager, Page Editor, Blog Manager, FAQ Manager, Content Preview

Dependencies: Marketing, Website, Mobile Apps, Notifications

## Module: Reports & Audit

- Section: SECTION 9
- Keywords: reports, analytics, audit, dashboards, exports, compliance, executive-dashboard, admin-dashboard

ProjectOverview reference: SECTION 9 — CONFIGURATION, NOTIFICATIONS & AUDIT in `Docs/ProjectOverview.md`

API names: Get Dashboard Summary, Get Dashboard Metrics, Get Reports List, Generate Report, Schedule Report, Get Audit Logs, Export Audit

DB tables: AuditLogs, DataAccessLogs, ReportSchedules, MaterializedReadModels (naming per report)

Screens: Reports Hub, Report Scheduler, Report Viewer, Audit Log Viewer, Export/Compliance Tools

Dependencies: DB Read Models, Super Admin, Finance, Ops

## Module: Authentication & Security

- Section: SECTION 10
- Keywords: auth, jwt, otp, 2fa, sessions, rbac, password-policy, customer-auth, register

ProjectOverview reference: SECTION 10 — AUTH, SECURITY & API ARCHITECTURE in `Docs/Flow/ProjectOverview.md`

API names:
  AuthController (/api/auth):
    Send OTP, Verify Customer OTP (phone), Login (Email/Password), Login Field (EmployeeId+PIN),
    Login OTP (internal 2FA), Verify OTP (email-based), Refresh Token (/refresh + /refresh-token alias),
    Forgot Password, Reset Password, Change Password (Customer), Logout,
    Delete Customer Account, Get Current User (me), Get Permission Snapshot (me/permissions),
    Force Logout User (SuperAdmin)
  CustomerAuthController (/api/customer-auth):
    Register Customer, Customer Forgot Password, Customer Reset Password, Customer Change Password

DB tables: Users, RefreshTokens, OtpVerifications, UserDevices, PasswordHistory, Customers

Screens: Login Screen, OTP Verification Screen, Verify OTP Screen (/verify-otp),
         Session Expired Screen, Force Logout Console, Password Policy Manager,
         Customer Registration Screen, Customer Forgot/Reset Password Screen

Dependencies: Users, Notifications, Admin Portal, Audit Logs, Customers

## Module: API Architecture

- Section: SECTION 10
- Keywords: api-versioning, response-envelope, pagination, error-codes, rate-limiting, openapi

ProjectOverview reference: SECTION 10 — AUTH, SECURITY & API ARCHITECTURE in `Docs/ProjectOverview.md`

API names: Get OpenAPI Spec, Get API Status, Get RateLimitStatus, Get API Usage Metrics

DB tables: APIUsageLogs, RateLimitCounters, ApiKeys, SystemConfigs

Screens: API Docs (internal), API Status Dashboard, Rate Limit Monitor

Dependencies: System Config, Audit Logs, Dashboard & Analytics

## Module: Database Architecture

- Section: SECTION 10
- Keywords: ef-core, migrations, soft-delete, audit-fields, json-columns, uuid, partitioning

ProjectOverview reference: SECTION 10 — AUTH, SECURITY & API ARCHITECTURE in `Docs/ProjectOverview.md`

API names: Get Migration Status, Trigger Migration (admin), Get DB Health

DB tables: (applies across all modules) Users, Customers, ServiceRequests, Invoices, AuditLogs, FileAttachments

Screens: DB Migrations Console (internal), DB Health Dashboard

Dependencies: All modules, DevOps

## Module: Payment Management

- Section: SECTION 5
- Keywords: payments, gateway, reconciliation, refunds, receipts, webhook

ProjectOverview reference: SECTION 5 — BILLING, INVOICE & PAYMENT in `Docs/ProjectOverview.md`

API names: Initiate Payment, Verify Payment, Handle Payment Webhook, Get Payment Status, Generate Receipt, Get Receipt By Invoice, Download Receipt PDF

DB tables: Payments, PaymentGatewayLogs, Receipts

Screens: Payment Gateway Screen, Payment Success, Payment Failed, Receipt View, Payment Reconciliation

Dependencies: Billing & Invoice, Customers, Accounting Integrations, Notifications

## Module: Accounts Receivable

- Section: SECTION 5
- Keywords: ar, aging, collections, reminders, write-off, reconciliations

ProjectOverview reference: SECTION 5 — BILLING, INVOICE & PAYMENT in `Docs/ProjectOverview.md`

API names: Get AR Aging, Send Payment Reminder, Record Payment, Generate AR Report, Mark Write-Off

DB tables: Invoices, Payments, Receipts, CreditNotes

Screens: AR Dashboard, Aging Report, Reminder Manager, Write-Off Console

Dependencies: Billing & Invoice, Payments, Notifications, Finance

## Module: Coupon & Discount Engine

- Section: SECTION 5
- Keywords: coupons, discounts, promo, validation, usage, thresholds

ProjectOverview reference: SECTION 5 — BILLING, INVOICE & PAYMENT in `Docs/ProjectOverview.md`

API names: Apply Coupon, Create Coupon, Get Coupons, Validate Coupon, Get Coupon Usage

DB tables: Coupons, CouponUsage

Screens: Coupon Manager, Apply Coupon UI, Coupon Usage Report

Dependencies: Booking Engine, Billing, Promotions, Notifications

## Module: Tax Configuration

- Section: SECTION 5
- Keywords: tax, gst, vat, hsn, tax-rates, jurisdiction

ProjectOverview reference: SECTION 5 — BILLING, INVOICE & PAYMENT in `Docs/ProjectOverview.md`

API names: Get Tax Configurations, Update Tax Configuration, Map HSN/SAC, Get Tax Liability Summary

DB tables: TaxConfigurations

Screens: Tax Config Editor, Tax Rate List, Tax Liability Report

Dependencies: Billing, Finance, Invoicing

## Module: Customer Web Portal

- Section: SECTION 6A
- Keywords: customer-portal, web-portal, dashboard, bookings, invoices, job-tracker, equipment, amc, notifications, addresses, profile, support-tickets, referral, feedback
- Stable Contract: YES — verified 2026-05-26
- Codebase: Frontend/Web/src (React + Vite + TypeScript + Tailwind)

ProjectOverview reference: SECTION 6A — CUSTOMER WEB PORTAL in `Docs/Flow/ProjectOverview.md`

Controllers (backend):
  CustomerNotificationController (/api/customer-notifications)
  CustomerEquipmentController    (/api/customers/me/equipment)
  CustomerAddressController      (/api/customers/me/addresses)
  CustomerController             (/api/customers/me/profile)
  BookingController              (/api/bookings/my-bookings, /api/bookings/customer, /api/bookings/guest)
  CustomerBookingController      (/api/customer-bookings)
  AmcController                  (/api/amc/customer/me, /api/amc/plans)
  InvoiceController              (/api/invoices/customer)
  SupportTicketController        (/api/support-tickets/my-tickets)
  AuthController                 (/api/auth/*)
  CustomerAuthController         (/api/customer-auth/*)

DB tables (confirmed):
  tblCustomer, tblCustomerAddress, tblCustomerEquipment,
  tblCustomerNotification [⚠ created 2026-05-26 via 13_customer_app_tables.sql],
  tblPromotionalOffer     [⚠ created 2026-05-26 via 13_customer_app_tables.sql],
  tblCustomerReferral     [⚠ created 2026-05-26 via 13_customer_app_tables.sql],
  tblCustomerLoyaltyTransaction [⚠ created 2026-05-26 via 13_customer_app_tables.sql],
  tblCustomerAppFeedback  [⚠ created 2026-05-26 via 13_customer_app_tables.sql],
  tblCustomerReview, tblBooking, tblServiceRequest,
  tblInvoiceHeader, tblSupportTicket, tblCommunicationPreference,
  tblCustomerAMC, tblAMCVisitSchedule

Portal Routes (React Router):
  /portal             → Dashboard.tsx
  /portal/bookings    → BookingsList.tsx
  /portal/bookings/:id → BookingDetail.tsx
  /portal/amc         → AMCDashboard.tsx
  /portal/equipment   → EquipmentList.tsx
  /portal/equipment/:id → EquipmentDetail.tsx
  /portal/invoices    → InvoicesList.tsx
  /portal/invoices/:id → InvoiceDetail.tsx
  /portal/support     → TicketsList.tsx
  /portal/support/new → NewTicket.tsx
  /portal/support/:id → TicketDetail.tsx
  /portal/notifications → Notifications.tsx
  /portal/addresses   → Addresses.tsx
  /portal/profile     → Profile.tsx
  /portal/feedback    → Feedback.tsx
  /portal/referral    → Referral.tsx

Dependencies: Auth, Booking Engine, Service Request Management, Billing, AMC Contract Engine, Support Tickets, Notifications, Customer Master, Equipment Register

## Module: Customer Portal (Mobile)

- Section: SECTION 7
- Keywords: mobile, offline, job-tracker, push, quick-book, draft

ProjectOverview reference: SECTION 7 — CUSTOMER PORTAL in `Docs/ProjectOverview.md`

API names: Get Mobile Dashboard, Sync Draft Booking, Get Bookings (mobile), Get Notifications, Submit Feedback

DB tables: Customers, Notifications, BookingQueue, JobReports

Screens: Mobile Home, Quick Book, Job Tracker (mobile), My Bookings (mobile), Notifications

Dependencies: Push Notifications, Offline Sync Engine, Booking Engine

## Module: Technician Mobile App

- Section: SECTION 2
- Keywords: technician-app, checkin, job-report, photos, parts-request, routes

ProjectOverview reference: SECTION 2 — SERVICE REQUEST & OPERATIONS in `Docs/ProjectOverview.md`

API names: Technician Check-In, Update Job Status, Submit Job Report, Upload Job Photos, Request Parts, Collect Payment (COD)

DB tables: Technicians, TechnicianStatusLog, JobReports, JobPhotos, PartsRequests, TechnicianGPSLog

Screens: Today / Jobs List (Tech), SR Detail (Mobile), Arrival Check-in, Service Checklist, Parts Request, Job Report Submission

Dependencies: Service Request Management, Inventory, Notifications, Maps

## Module: Lead Management

- Section: SECTION 8
- Keywords: lead, inquiry, crm, convert, assign, lost, qualified, source-channel, lead-note
- Stable Contract: YES — 9 endpoints (verified 2026-05-25)

ProjectOverview reference: SECTION 8 — MODULE: Lead Management in `Docs/Flow/ProjectOverview.md`

Controller: LeadController — /api/leads

API routes:
  POST   /api/leads                              — Create Lead (AllowAnonymous)
  GET    /api/leads                              — Search Leads (Policy=ServiceRequestRead)
  GET    /api/leads/analytics                    — Lead Analytics (Policy=ServiceRequestRead)
  GET    /api/leads/{leadId}                     — Get Lead Detail (Policy=ServiceRequestRead)
  PUT    /api/leads/{leadId}/assign              — Assign Lead (Policy=ServiceRequestUpdate)
  PUT    /api/leads/{leadId}/status              — Update Lead Status (Policy=ServiceRequestUpdate)
  POST   /api/leads/{leadId}/convert-to-booking  — Convert → Booking (Policy=BookingCreate)
  POST   /api/leads/{leadId}/convert-to-sr       — Convert → Service Request (Policy=ServiceRequestCreate)
  POST   /api/leads/{leadId}/notes               — Add Note (Policy=ServiceRequestUpdate)

DB tables: Lead, LeadAssignment, LeadConversion, LeadNote, LeadSource, LeadStatusHistory

Screens: Lead List, Lead Detail, Create Lead (public inquiry form), Assign Lead, Convert Lead

Dependencies: Booking Engine, Service Request Management, Notifications

## Module: Installation Management

- Section: SECTION 8
- Keywords: installation, survey, proposal, commissioning, installation-order, site-survey, checklist-execution
- Stable Contract: YES — 16 endpoints across 4 controllers (verified 2026-05-25)

ProjectOverview reference: SECTION 8 — MODULE: Installation Management in `Docs/Flow/ProjectOverview.md`

Controllers:
  InstallationController        — /api/installations              (6 endpoints)
  InstallationSurveyController  — /api/installations/{id}         (2 endpoints)
  InstallationProposalController — /api/installations/{id}        (3 endpoints)
  InstallationExecutionController — /api/installations/{id}       (5 endpoints)

API routes:
  POST   /api/installations                                         — Create Installation (AllowAnonymous)
  GET    /api/installations                                         — List Installations (Authorize)
  GET    /api/installations/{id}                                    — Get Detail (Authorize)
  POST   /api/installations/orders                                  — Create Installation Order (Policy=ServiceRequestUpdate)
  POST   /api/installations/orders/{orderId}/survey-report          — Submit Survey Report
  POST   /api/installations/orders/{orderId}/commissioning-certificate — Create Commissioning Certificate
  POST   /api/installations/{id}/schedule-survey                   — Schedule Survey
  POST   /api/installations/{id}/submit-survey                     — Submit Survey
  POST   /api/installations/{id}/proposal                          — Create Proposal
  POST   /api/installations/{id}/proposal/approve                  — Approve Proposal
  POST   /api/installations/{id}/proposal/reject                   — Reject Proposal
  POST   /api/installations/{id}/create-order                      — Create Execution Order
  POST   /api/installations/{id}/start                             — Start Installation
  POST   /api/installations/{id}/checklist                         — Save Checklist
  POST   /api/installations/{id}/complete                          — Complete Installation
  POST   /api/installations/{id}/commission                        — Commission (sign-off)

DB tables:
  InstallationLead, InstallationOrder, InstallationProposal, InstallationProposalLine,
  InstallationStatusHistory, InstallationSurvey, InstallationSurveyItem,
  InstallationChecklist, InstallationChecklistResponse,
  CommissioningCertificate, SiteSurveyReport

Screens: Installation List, Installation Detail, Schedule Survey, Survey Report, Proposal View, Execution Tracking, Commissioning Sign-Off

Dependencies: Lead Management, Service Request Management, Technician Management, Billing

## Module: Branch Management

- Section: SECTION 8
- Keywords: branch, office, location, manager, zone-assignment, technician-branch
- Stable Contract: YES — 4 endpoints (verified 2026-05-25)

ProjectOverview reference: SECTION 8 — MODULE: Branch Management in `Docs/Flow/ProjectOverview.md`

Controller: BranchController — /api/branches

API routes:
  GET    /api/branches              — List / Search Branches (Policy=UserRead)
  GET    /api/branches/{branchId}   — Get Branch by ID (Policy=UserRead)
  POST   /api/branches              — Create Branch (Policy=UserCreate)
  PUT    /api/branches/{branchId}   — Update Branch (Policy=UserUpdate)

DB tables (storage): DynamicMasterRecord (MasterType="Branch") — not a dedicated table
  Users.BranchId FK, ServiceRequests.BranchId FK — live counts computed on each GET

Screens: Branch List, Branch Detail, Create/Edit Branch

Dependencies: RBAC / Users & Roles, Technician Management, Service Request Management

## Module: Campaign Management

- Section: SECTION 8
- Keywords: campaign, marketing, batch-booking, zone-campaign, service-promotion
- Stable Contract: PARTIAL — 1 endpoint (create only) (verified 2026-05-25)

ProjectOverview reference: SECTION 8 — MODULE: Campaign Management in `Docs/Flow/ProjectOverview.md`

Controller: CampaignController — /api/campaigns

API routes:
  POST   /api/campaigns             — Create Campaign (Policy=BookingCreate)

DB tables: Campaign

Screens: Campaign List (admin), Create Campaign

Dependencies: Booking Engine, Zones, Service Types, Slot Availability

## Module: Support Tickets

- Section: SECTION 8
- Keywords: support, ticket, helpdesk, escalation, reply, category, priority, close, reopen
- Stable Contract: YES — 16 endpoints across 4 controllers (verified 2026-05-25)

ProjectOverview reference: SECTION 8 — MODULE: Support Tickets in `Docs/Flow/ProjectOverview.md`

Controllers:
  SupportTicketController         — /api/support-tickets                    (9 endpoints)
  SupportTicketEscalationController — /api/support-tickets/{id}             (2 endpoints)
  SupportTicketReplyController    — /api/support-tickets/{id}/replies       (2 endpoints)
  SupportTicketLookupController   — /api/support-ticket-lookups             (3 endpoints + 1 alias)

API routes:
  POST   /api/support-tickets                              — Create Ticket (Authorize)
  GET    /api/support-tickets                              — Search Tickets (Policy=SupportRead)
  GET    /api/support-tickets/{id}                         — Get Detail (Authorize)
  GET    /api/support-tickets/my-tickets                   — My Tickets (JWT-scoped, countOnly supported)
  POST   /api/support-tickets/{id}/assign                  — Assign (Policy=SupportManage)
  POST   /api/support-tickets/{id}/change-status           — Change Status (Policy=SupportManage)
  POST   /api/support-tickets/{id}/change-priority         — Change Priority (Policy=SupportManage)
  POST   /api/support-tickets/{id}/close                   — Close (Authorize)
  POST   /api/support-tickets/{id}/reopen                  — Reopen (Authorize)
  GET    /api/support-tickets/{id}/escalations             — Get Escalations (Authorize)
  POST   /api/support-tickets/{id}/escalate                — Escalate (Policy=SupportManage)
  GET    /api/support-tickets/{id}/replies                 — Get Replies (Authorize)
  POST   /api/support-tickets/{id}/replies                 — Add Reply (Authorize)
  GET    /api/support-ticket-lookups/categories            — Get Categories
  GET    /api/support-ticket-lookups/priorities            — Get Priorities
  GET    /api/support-ticket-lookups/statuses              — Get Statuses

DB tables:
  SupportTicket, SupportTicketAssignment, SupportTicketReply, SupportTicketEscalation,
  SupportTicketCategory, SupportTicketPriority, SupportTicketLink, SupportTicketStatusHistory

Screens: Support Queue (admin), My Tickets (customer portal), Ticket Detail, Escalation View, Reply Thread

Dependencies: Customer Master, Service Request Management, Notifications

## Module: Feedback Management

- Section: SECTION 8
- Keywords: feedback, reviews, respond, publish, flag, customer-review
- Stable Contract: YES — 4 endpoints (verified 2026-05-25)

ProjectOverview reference: SECTION 8 — MODULE: Feedback Management in `Docs/Flow/ProjectOverview.md`

Controller: FeedbackController — /api/feedback

API routes:
  GET    /api/feedback                              — List Feedback (Policy=SupportRead)
  GET    /api/feedback/{customerReviewId}           — Get Detail (Policy=SupportRead)
  PATCH  /api/feedback/{customerReviewId}/respond   — Respond (Policy=SupportManage)
  PATCH  /api/feedback/{customerReviewId}/publish   — Publish / Unpublish (Policy=SupportManage)
  PATCH  /api/feedback/{customerReviewId}/flag      — Flag (Policy=SupportManage)

DB tables: CustomerReview

Screens: Feedback Queue (admin), Feedback Detail, Respond/Publish/Flag actions

Dependencies: Customer Portal (review submission via CustomerReviewController), Support Tickets

## Module: Revisit Management

- Section: SECTION 8
- Keywords: revisit, warranty-revisit, amc-revisit, callback, complaint-revisit, job-revisit
- Stable Contract: YES — 2 endpoints (verified 2026-05-25)

ProjectOverview reference: SECTION 8 — MODULE: Revisit Management in `Docs/Flow/ProjectOverview.md`

Controller: RevisitController — /api/revisit

API routes:
  POST   /api/revisit/request                  — Create Revisit Request (Authorize)
  GET    /api/revisit/booking/{bookingId}       — Get Revisits by Booking (Authorize)

DB tables: RevisitRequest

Screens: Raise Revisit (Invoice/SR detail), Revisit History (Booking detail)

Dependencies: Warranty Management (WarrantyClaimId), AMC Contract Engine (CustomerAmcId), Service Request Management (new SR on approval)

## Module: Analytics & Dashboard

- Section: SECTION 8
- Keywords: analytics, dashboard, kpi, revenue, booking-trends, technician-performance, customer-growth, inventory-analytics, support-analytics
- Stable Contract: YES — 8 endpoints across 2 controllers (verified 2026-05-25)

ProjectOverview reference: SECTION 8 — MODULE: Analytics & Dashboard in `Docs/Flow/ProjectOverview.md`

Controllers:
  AnalyticsController  — /api/analytics   (6 endpoints, Policy=AnalyticsRead / SupportRead)
  DashboardController  — /api/dashboard   (2 endpoints, Policy=DashboardRead)

API routes:
  GET   /api/analytics/bookings       — Booking Analytics (Policy=AnalyticsRead)
  GET   /api/analytics/revenue        — Revenue Analytics (Policy=AnalyticsRead)
  GET   /api/analytics/technicians    — Technician Performance (Policy=AnalyticsRead)
  GET   /api/analytics/customers      — Customer Analytics (Policy=AnalyticsRead)
  GET   /api/analytics/support        — Support Analytics (Policy=SupportRead)
  GET   /api/analytics/inventory      — Inventory Analytics (Policy=AnalyticsRead)
  GET   /api/dashboard/summary        — Dashboard Summary (Policy=DashboardRead)
  GET   /api/dashboard/metrics        — Dashboard Metrics (Policy=DashboardRead)

DB tables: (computed queries — no dedicated analytics table)
  Source: Bookings, ServiceRequests, InvoiceHeaders, PaymentTransactions, Technicians,
          Customers, SupportTickets, JobCards, WarehouseStock, StockTransactions, CustomerReviews

Screens: Analytics Hub, Revenue Dashboard, Technician Performance, Customer Insights, Support Analytics, Inventory Analytics, Admin Dashboard Home

Dependencies: Billing, Service Request Management, Technician Management, Customer Master, Support Tickets, Inventory

## Module: RBAC — Users, Roles & Permissions

- Section: SECTION 8
- Keywords: rbac, users, roles, permissions, access-control, user-create, role-assign, permission-matrix, deactivate-user, reset-password, reset-pin
- Stable Contract: YES — 14 endpoints across 3 controllers (verified 2026-05-25)

ProjectOverview reference: SECTION 8 — MODULE: RBAC — Users, Roles & Permissions in `Docs/Flow/ProjectOverview.md`

Controllers:
  UserController       — /api/users        (8 endpoints)
  RoleController       — /api/roles        (5 endpoints)
  PermissionController — /api/permissions  (1 endpoint)

API routes:
  GET    /api/users                          — List Users (Policy=UserRead)
  GET    /api/users/{userId}                 — Get User Detail (Policy=UserRead)
  POST   /api/users                          — Create User (Policy=UserCreate)
  PUT    /api/users/{userId}                 — Update User (Policy=UserUpdate)
  POST   /api/users/{userId}/deactivate      — Deactivate User (Policy=UserUpdate)
  POST   /api/users/{userId}/reactivate      — Reactivate User (Policy=UserUpdate)
  POST   /api/users/{userId}/reset-password  — Reset Password (Policy=UserUpdate)
  POST   /api/users/{userId}/reset-pin       — Reset PIN (Policy=UserUpdate)
  GET    /api/roles                          — List Roles (Policy=RoleRead)
  POST   /api/roles                          — Create Role (Policy=RoleCreate)
  PUT    /api/roles/{roleId}                 — Update Role (Policy=RoleUpdate)
  GET    /api/roles/{roleId}/permissions     — Get Role Permission Snapshot (Policy=RoleRead)
  PUT    /api/roles/{roleId}/permissions     — Update Role Permissions (Policy=RoleUpdate)
  GET    /api/permissions                    — List Permissions (Policy=PermissionRead)

DB tables: User, Role, UserRole, Permission, RolePermission, UserPasswordHistory, UserSession

Screens: User List, User Detail, Create/Edit User, Role List, Role Detail, Permission Editor (module matrix), Deactivate User

Dependencies: Authentication (AuthController shares User/RefreshToken tables), Branch Management (BranchId FK), all modules (policy enforcement)
