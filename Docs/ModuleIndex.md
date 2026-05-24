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
- Keywords: SR, service-request, dispatch, admin, operations, timeline

ProjectOverview reference: SECTION 2 — SERVICE REQUEST & OPERATIONS in `Docs/ProjectOverview.md`

API names: Create SR, Create SR from Booking, Get SR, Add SR Note, Update SR Status, Assign Technician, Get SR Timeline, Reschedule SR, Cancel SR, Get SRs by Filters, Get SRs Dashboard Summary

DB tables: ServiceRequests, SRStatusHistory, SRAssignments, SRNotes, SRCommunicationLog, JobReports, JobPhotos, JobChecklists, CustomerSignatures

Screens: SR List, SR Detail, Create SR, Assign Technician Panel, Reschedule SR, Cancel SR, Escalate SR, Create Follow-up SR, SR Notes, SR Communication Log, Bulk SR Actions

Dependencies: Technician Management, Scheduling, Inventory, Billing, Notifications, Customers

## Module: Field Workflow

- Section: SECTION 2
- Keywords: field, technician, mobile, checklist, gps, photos, job-report

ProjectOverview reference: SECTION 2 — SERVICE REQUEST & OPERATIONS (Field Workflow) in `Docs/ProjectOverview.md`

API names: Field My Jobs, Field Job Detail, Field Arrival Check-In, Submit Job Report, Upload Job Photos, Technician Check-In, Technician Check-Out, Request Parts, Submit Estimate, Capture Signature, Record Field Payment, Complete Field Job, Helper Assignment, Helper Attendance, Helper Task Photo Upload

DB tables: JobReports, JobPhotos, JobChecklists, CustomerSignatures, PartsRequests

Screens: Today / Jobs List, SR Detail (Mobile), Navigation / Map View, Arrival Check-in, Service Checklist, Observations & Diagnosis, Parts Used / Parts Request, Photo Capture, Estimate Creation, Customer Signature, Job Report Submission, Payment Collection / Receipt, Revisit Scheduling, Offline Queue / Sync Status, Technician Profile & Availability, Parts Issue / Receive, Notifications & Messages

Dependencies: Service Request Management, Inventory, Notifications, Offline Sync Engine, File/Attachment Storage

## Module: Customer Master (Customer 360)

- Section: SECTION 3
- Keywords: customer, customer-360, customer-management, addresses, equipment, amc, invoices, tickets

ProjectOverview reference: SECTION 3 — CUSTOMER, EQUIPMENT & AMC in `Docs/ProjectOverview.md`

API names: Get Customers, Create Customer, Update Customer, Delete Customer (soft), Get Customer Addresses, Add Address, Update Address, Delete Address, Get My Addresses, Add My Address, Update My Address, Delete My Address, Add Customer Note

DB tables: Customers, CustomerAddresses, CustomerEquipment, CustomerNotes, CorporateAccounts, CorporateContacts

Screens: Customer List, Customer 360 View, Create Customer, Edit Customer, Add/Edit Address, Equipment Register, Corporate Account Management, Customer Risk Dashboard, Customer Notes, Customer Communication Panel

Dependencies: Service Request Management, Billing, Notifications, AMC Engine

## Module: Equipment Register

- Section: SECTION 3
- Keywords: equipment, equipment-register, serial, warranty, installation, service-history

ProjectOverview reference: SECTION 3 — CUSTOMER, EQUIPMENT & AMC in `Docs/ProjectOverview.md`

API names: Get Equipment, Create Equipment, Update Equipment, SoftDelete Equipment, Get Equipment History, Get My Equipment, Add My Equipment, Update My Equipment, Delete My Equipment, Get Booking Brands, Get My Service History

DB tables: CustomerEquipment, JobReports, WarrantyRecords, PartsRequests

Screens: Equipment List, Equipment Detail, Register Equipment, Link to Customer, Book Service CTA

Dependencies: Customer Master, Inventory, Warranty Management, Service Request Management

## Module: AMC Contract Engine

- Section: SECTION 3
- Keywords: amc, contract, enrollment, visits, renewal, plan

ProjectOverview reference: SECTION 3 — CUSTOMER, EQUIPMENT & AMC in `Docs/ProjectOverview.md`

API names: Get AMC Contracts, Create Contract, Update Contract, Enroll Customer, Get Contract Visits, Create Contract Visit, Renew Contract, Get AMC Plans, Get AMC Plan By Id, Get Customer AMC, Get My Customer AMC

DB tables: AMCPlans, AMCContracts, AMCVisitSchedule, AMCVisitLog

Screens: AMC Contract List, Contract Detail, Enroll Customer, Edit Contract, Visit Schedule Management, Visit Detail, Renewal Management, Bulk Reminder, Plan Performance Dashboard

Dependencies: Customer Master, Scheduling, Billing, Notifications

## Module: Warranty Management

- Section: SECTION 3
- Keywords: warranty, replacement, warranty-records, revisit

ProjectOverview reference: SECTION 3 — CUSTOMER, EQUIPMENT & AMC in `Docs/ProjectOverview.md`

API names: Get Warranty Records, Create Warranty Record, Check Warranty Eligibility

DB tables: WarrantyRecords

Screens: Warranty List, Warranty Detail, Link Warranty to SR, Warranty Claims

Dependencies: Equipment Register, Service Request Management, Parts/Inventory

## Module: Booking Engine

- Section: SECTION 4
- Keywords: booking, slots, calendar, guest-booking, coupon, otp, draft

ProjectOverview reference: SECTION 4 — BOOKING ENGINE in `Docs/ProjectOverview.md`

API names: Get Service Types, Get Slots, Hold Slot, Create Booking, Confirm Booking, Get Booking Summary, Apply Coupon, Send OTP, Verify OTP

DB tables: ServiceSlots, SlotAvailability, BookingQueue

Screens: Booking Wizard Step 1..6, Booking Confirmation, Slot Unavailable, Booking Draft Resume

Dependencies: Customers, Service Request Management, Scheduling, Notifications, Coupons

## Module: Billing & Payments

- Section: SECTION 5
- Keywords: billing, invoice, payments, ar, coupons, tax

ProjectOverview reference: SECTION 5 — BILLING, INVOICE & PAYMENT in `Docs/ProjectOverview.md`

API names: Create Invoice, Get Invoices, Get Invoice Detail, Download Invoice PDF, Send Invoice, Mark Invoice Paid, Record Payment, Payment Reconcile, Get Accounts Receivable Dashboard, Apply Coupon, Get Tax Configurations

DB tables: Invoices, InvoiceLineItems, Payments, PaymentGatewayLogs, CreditNotes, Receipts, Coupons, CouponUsage, TaxConfigurations

Screens: Invoice List, Invoice Detail, Create Manual Invoice, Edit Invoice, Apply Discount/Coupon, Issue Credit Note, Mark as Paid, Send Invoice, Proforma Invoice, Corporate Consolidated Invoice, AR Dashboard

Dependencies: Service Request Management, Accounting Integrations, Notifications, Customers, Inventory

## Module: Estimates & Work Orders

- Section: SECTION 6
- Keywords: estimates, work-orders, approvals, technician-estimate, corporate-approval

ProjectOverview reference: SECTION 6 — ESTIMATES, WORK ORDERS & INVENTORY in `Docs/ProjectOverview.md`

API names: Create Estimate, Get Estimates, Send Estimate, Approve Estimate, Reject Estimate, Override Estimate Approval, Create Work Order, Get Work Orders, Issue Parts, Get Job Reports, Approve Job Report, Flag Job Report

DB tables: Estimates, EstimateLineItems, WorkOrders, EstimateApprovalHistory

Screens: Estimate List, Estimate Detail, Work Order List, Work Order Detail, Job Reports Queue, Report Quality Dashboard, Corporate Approval Workflow

Dependencies: Service Request Management, Billing, Inventory, Notifications

## Module: Inventory & Parts Management

- Section: SECTION 6
- Keywords: inventory, parts, stock, purchase-order, parts-request

ProjectOverview reference: SECTION 6 — ESTIMATES, WORK ORDERS & INVENTORY in `Docs/ProjectOverview.md`

API names: Get Parts, Create Part, Parts Request, Approve Parts Request, Partial Approve Parts Request, Stock Adjust, Create PO, Receive PO, Get Low Stock Alerts, Get Suppliers, Create Supplier

DB tables: PartsCategories, PartsCatalog, StockLedger, PartsRequests, PartsRequestItems, PurchaseOrders, POItems

Screens: Inventory Dashboard, Parts Catalog List, Part Detail, Add/Edit Part, Parts Requests Queue, Parts Request Detail, Stock Movement Ledger, Purchase Order List, PO Detail, Low Stock Alerts, Supplier Management

Dependencies: Procurement, Service Request Management, Work Orders, Finance

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

- Section: SECTION 8
- Keywords: technician-management, team, skills, zones, attendance, availability-board, helper, gps

ProjectOverview reference: SECTION 8 — ADMIN OPERATIONS in `Docs/ProjectOverview.md`

API names: Get Technicians, Create Technician, Update Technician, Get Technician Performance, Get Technician Attendance, Request Technician Leave, Review Technician Leave, Get Availability Board, Get Technician GPS Log, Update Technician Skills, Update Technician Zones, Get Helpers, Update Helper, Get Technician Public Profile

DB tables: Technicians, TechnicianStatusLog, TechnicianSkill, TechnicianZone, TechnicianAttendance, TechnicianGPSLog, TechnicianPerformanceSummary

Screens: Technician List, Technician Detail, Technician Editor, Availability Board, Helper Editor

Dependencies: Dispatch Management, Scheduling, Master Data & Configuration, Field Workflow

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
- Keywords: auth, jwt, otp, 2fa, sessions, rbac, password-policy

ProjectOverview reference: SECTION 10 — AUTH, SECURITY & API ARCHITECTURE in `Docs/ProjectOverview.md`

API names: Send OTP, Verify OTP, Login (Email/Password), Technician Login (EmployeeID+PIN), Refresh Token, Logout, Forgot Password, Reset Password, Change Password

DB tables: Users, UserSessions, RefreshTokens, OtpVerifications, UserDevices, PasswordHistory

Screens: Login Screen, OTP Verification, Session Management, Force Logout Console, Password Policy Manager

Dependencies: Users, Notifications, Admin Portal, Audit Logs

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

## Module: Customer Portal (Web)

- Section: SECTION 7
- Keywords: customer-portal, dashboard, bookings, invoices, job-tracker, feedback

ProjectOverview reference: SECTION 7 — CUSTOMER PORTAL in `Docs/ProjectOverview.md`

API names: Get Dashboard, Get Bookings, Get Booking Timeline, Reschedule Booking, Get Estimates, Approve Estimate, Get Customer Reviews, Create Customer Review, Get Feedback, Get Feedback Detail, Respond to Feedback, Publish Feedback, Flag Feedback

DB tables: Customers, CustomerAddresses, ServiceRequests, Estimates, Tickets, Notifications, CustomerReviews

Screens: Customer Dashboard (Web), My Bookings List, Booking Detail, Service Report View, Billing / Invoice View, Feedback List, Feedback Detail

Dependencies: Booking Engine, Notifications, Billing, Support

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
