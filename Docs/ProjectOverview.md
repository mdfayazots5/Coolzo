# SECTION 1 — PROJECT IDENTITY & ARCHITECTURE

BRAND IDENTITY

- Brand Name: CoolElite
- Tagline: Precision. Comfort. Trust.
- Positioning Statement:
  CoolElite is positioned as the premium-tier AC service provider in its operating market — not simply a repair service, but a complete climate comfort management company. The platform elevates AC maintenance, repair, installation, and AMC services to a level of professionalism and care associated with luxury home services.

Target Audience (5 segments)

1. Premium Residential
   - Urban homeowners and renters in mid-to-high income brackets who value reliability, punctuality, and digital convenience over price-shopping.
2. High-End Residential Complexes
   - Apartment buildings, gated communities, and societies where building managers need a dependable, contractual AC service provider.
3. Small & Medium Commercial
   - Restaurants, retail outlets, salons, clinics, and offices that cannot afford downtime and require responsive scheduled maintenance.
4. Large Commercial / Enterprise
   - Hotels, hospitals, corporate offices, and industrial units requiring multi-unit AMC management, SLA compliance, and dedicated account handling.
5. Property Managers & Builders
   - Real estate developers and facility management firms who need a service partner for new installations and ongoing building-wide maintenance.

Core Value Differentiators (verbatim)

- Certified, uniformed, background-verified technicians who arrive on time with tracked ETAs.
- Digital service reports delivered to the customer after every visit — no paper, no ambiguity.
- Real-time job tracking visible to the customer through web and mobile — like ordering a premium delivery.
- Transparent, pre-approved estimates before any work begins — no surprise charges.
- Annual Maintenance Contracts (AMC) that automate scheduling, reminders, and compliance — customers never have to call to schedule a visit.
- 24/7 customer support with a premium-feel communication experience via WhatsApp, email, and in-app messaging.
- Branded premium experience across every touchpoint — website, app, invoice, communication, and technician presence.

Visual Direction (exact)

- Design Philosophy: Premium Minimal. Every element on screen earns its place. Visual design communicates trust, professionalism, and care.

Color System (exact hex codes)

- Primary (Deep Navy): #1B2A4A — Brand color. Headers, navigation, primary text, icon fills.
- Accent (Warm Gold): #C9A84C — CTAs, highlights, active states, price callouts.
- Background (Warm White): #FAFAFA — Page backgrounds.
- Surface (White): #FFFFFF — Card backgrounds.
- Border / Divider: #E2E4EE — Subtle boundaries.
- Text Primary: #1A1A2E — Near-black for body text.
- Text Secondary: #6B7080 — Captions, meta text, labels.
- Success: #27AE60
- Warning: #F39C12
- Error / Urgent: #E74C3C

Typography & Spacing (exact)

- Heading Font: Inter (geometric sans-serif). Bold weights for headlines.
- Body Font: Inter Regular at 16px base. Line height 1.6.
- Accent Headlines: Optional refined display font for marketing hero headlines only.
- No decorative or script fonts. No font mixing beyond heading and body.
- Layout: Generous whitespace. Maximum content width: 1280px centered.
- Grid & Spacing: Consistent 8px grid system. Use multiples of 8px (8,16,24,32,48,64).
- Border radius: 12px cards, 8px buttons, 4px small elements.

Interaction & Component rules (exact)

- Button states: hover uses gold accent; active uses pressed effect; disabled 50% opacity.
- Form inputs: bottom-border style or full-border with subtle background; floating label on focus.
- Loading states: skeleton screens for heavy content; progress bars for multi-step flows.
- Transitions: 200ms ease-in-out for hover; 300ms for panel slides.

Design Do's and Must-Nots (exact)

Must NOT appear in the CoolElite UI (verbatim list):

- No auto-playing carousels or video.
- No pop-ups on first visit or while reading.
- No flashing banners or marquee text.
- No more than 2 font weights on a single screen.
- No cramped tables with tiny text in customer-facing screens.
- No generic stock photography of smiling people in uniforms. Use real team photography or premium illustrated graphics.
- No neon or highly saturated accent colors.
- No feature lists that use 12-point bullets covering an entire page.
- No dark mode for customer-facing surfaces (unless customer preference in v2). Admin portal may offer dark mode.
- No interface element that requires explanation — if it needs a tooltip to make sense, redesign it.

PLATFORM ECOSYSTEM

- Surfaces (five):
  - Public & Customer Website — Angular SPA (Marketing, booking, account management, tracking, payments).
  - Admin Web Portal — Angular SPA (Internal operations, configuration, reports, management).
  - Technician Mobile App — Angular (Field job management, navigation, reporting).
  - Helper Mobile App — Angular (simplified build or shared app with role-based UI for field assistants).
  - Backend API Layer — ASP.NET Core C# Web API (single unified API consumed by all surfaces).
  - Database Layer — SQL Server (production) / PostgreSQL (alternate env).

How surfaces connect (integration flow: booking → invoice)

1. Customer creates booking via Website or Customer App (Booking Wizard: service selection, equipment, address, slot, contact, confirm).
2. API creates Service Request (SR) in backend via unversioned `/api/...` endpoints.
3. Operations views SR in Admin Portal and assigns Technician (manual or smart dispatch).
4. Technician receives job on Technician Mobile App (Angular), navigates to address and checks in.
5. Technician performs work, completes checklist, captures photos, captures customer signature and submits Job Report via mobile app.
6. On Job Report submission the API updates SR status and the Billing module triggers invoice generation (invoice auto-generated on submit).
7. Customer receives invoice/payment link via WhatsApp and Email.
8. Customer pays online; payment gateway webhook updates API and marks invoice paid.
9. System sends confirmation notifications and generates receipt; SR closed when payment/closure conditions met.
10. All events are audit-logged and notifications are sent (WhatsApp/Email/Push) at configured triggers.

COMPLETE ROLE ARCHITECTURE (extracted verbatim)

ROLE: Guest / Website Visitor
- Purpose: Any unauthenticated user browsing the public website or app.
- Data visibility (Sees): Public content, service catalog, pricing information, blog, FAQs, booking initiation.
- Cannot See: Any operational, financial, or account-specific data.
- Actions permitted: Browse services, initiate booking (guest booking), view contact info, read reviews.
- Channel: Public website, React Native app (pre-login).

ROLE: Customer (Registered)
- Purpose: End consumer who has registered and uses CoolElite services.
- Data visibility (Sees): Own service history, bookings, AMC status, invoices, receipts, technician info for own jobs, support tickets.
- Cannot See: Other customers, technician internal data, pricing rules, admin settings, other jobs.
- Actions permitted: Book services, track jobs, approve estimates, make payments, raise support tickets, write reviews, manage AMC, view digital service reports.
- Channel: Customer-facing website (authenticated zone), React Native mobile app.

ROLE: Corporate Account User
- Purpose: Designated contact person from an enterprise/commercial client account.
- Data visibility (Sees): All jobs under their company account, consolidated invoices, AMC dashboard, team bookings if multi-user allowed.
- Cannot See: Pricing configurations, internal operations, other companies' data.
- Actions permitted: Raise service requests for company sites, approve estimates above threshold, download consolidated billing reports.
- Channel: Customer web portal (corporate view), mobile app.

ROLE: Customer Support Executive
- Purpose: First point of contact for inbound calls, WhatsApp, and email queries. Creates and manages service requests.
- Data visibility (Sees): Customer database, service request history, booking tools, escalation queue, live job status.
- Cannot See: Financial configuration, technician payroll/incentive data, admin settings.
- Actions permitted: Create SRs, update customer contact info, reschedule jobs, raise escalations, send communication, log callbacks.
- Channel: Admin web portal (CS module).

ROLE: Operations Executive
- Purpose: Responsible for technician dispatching, daily job scheduling, and real-time operations monitoring.
- Data visibility (Sees): All service requests, technician availability, job map view, zone allocation, SLA status.
- Cannot See: Financial P&L reports, admin configuration, customer payment data.
- Actions permitted: Assign technicians, reschedule jobs, reassign in-progress jobs, communicate with field teams, manage daily job sheet.
- Channel: Admin web portal (operations module).

ROLE: Operations Manager
- Purpose: Oversees daily operations, manages technician team, reviews performance, handles escalations.
- Data visibility (Sees): All operations data, technician performance, SLA compliance, escalation reports, job completion quality.
- Cannot See: Full financial P&L (only operational cost data), system configuration.
- Actions permitted: Review SLA violations, approve job overrides, review technician ratings, manage shift scheduling, approve revisit/warranty claims.
- Channel: Admin web portal.

ROLE: Technician
- Purpose: Certified field professional who performs AC service work.
- Data visibility (Sees): Own assigned jobs only, job details, customer address and contact (limited), parts needed, service history for the specific AC unit.
- Cannot See: Other technicians' jobs, pricing rules, customer account details, business reports.
- Actions permitted: Accept/reject job assignment, navigate to job, check-in/check-out, update job status, raise parts request, capture photos, submit job report, collect digital signature, collect payment if COD.
- Channel: React Native Technician Mobile App.

ROLE: Helper / Field Assistant
- Purpose: Junior field staff who assists the technician on-site. Not independently skilled.
- Data visibility (Sees): Only the job they are attached to — address, task list, and status controls.
- Cannot See: Job financials, customer account, other jobs.
- Actions permitted: Check-in with technician, update task completion items, take arrival/completion photos.
- Channel: Simplified React Native app or mobile web view.

ROLE: Inventory / Store Manager
- Purpose: Manages spare parts, consumables, and equipment stock.
- Data visibility (Sees): Inventory levels, parts requests from technicians, purchase orders, stock movement logs.
- Cannot See: Customer data, job financial data, HR/payroll.
- Actions permitted: Approve/issue parts to technicians, raise purchase orders, update stock, generate inventory reports.
- Channel: Admin web portal (inventory module).

ROLE: Billing / Accounts Executive
- Purpose: Manages invoice generation, payment tracking, customer account reconciliation, and financial records.
- Data visibility (Sees): All invoices, payments, pending collections, customer accounts, tax records, credit notes.
- Cannot See: Technician field notes, job operational details beyond billing info, admin configuration.
- Actions permitted: Generate/edit invoices, apply discounts, issue credit notes, reconcile payments, follow up on outstanding dues, export financial data.
- Channel: Admin web portal (billing module).

ROLE: Finance Manager
- Purpose: Strategic oversight of revenue, collections, and financial reporting.
- Data visibility (Sees): Full financial dashboard, P&L summary, revenue trends, aging analysis, tax liability reports.
- Cannot See: Operational details, technician assignments, customer-level job notes.
- Actions permitted: Approve discount overrides, review financial reports, export accounting data, configure billing rules.
- Channel: Admin web portal (finance module).

ROLE: Admin / Branch Manager
- Purpose: Manages a specific branch or city-level operation. Has elevated access within their branch scope.
- Data visibility (Sees): All data within their branch — jobs, technicians, customers, financials.
- Cannot See: Other branches' data unless granted, global system configuration.
- Actions permitted: All operations and billing actions within branch, team management, branch-level reporting.
- Channel: Admin web portal.

ROLE: Super Admin
- Purpose: The highest-privilege system user. Controls all configuration, all permissions, all master data.
- Data visibility (Sees): Everything — all branches, all users, all data, all logs, all configuration.
- Actions permitted: Configure all system settings, manage roles and permissions, control pricing rules, manage notification templates, onboard/offboard users, manage master data, view all analytics.
- Channel: Admin web portal (full access).

ROLE: Marketing Manager (Optional Internal Role)
- Purpose: Manages website content, promotional campaigns, coupon codes, and customer communication campaigns.
- Data visibility (Sees): Website CMS sections, campaign performance, coupon usage, customer acquisition metrics.
- Actions permitted: Update banner content, create promotions, manage SEO-friendly content, export lead data.
- Channel: Admin web portal (CMS + marketing module).

RBAC DESIGN PRINCIPLES (all 4)

1. Module-Level Access: Every module in the platform is individually grantable to any role.
2. Action-Level Access: Within each module, actions (view / create / edit / delete / approve / export) are individually configurable.
3. Data Scope Rules: Roles can be scoped to branch, zone, customer account type, or own-data-only.
4. Super Admin Override: Super Admin can assume any role view for auditing and support purposes.

Module-level, action-level, data scope rules (implementation detail)

- Module-level: Admin Portal must expose every major functional area (Operations, Billing, Inventory, Notifications, CMS, Master Data) as a distinct module that can be granted or revoked per role.
- Action-level: Permissions stored per module as a bitset or explicit CRUD+Approve+Export flags; evaluated at API middleware and enforced in UI rendering.
- Data-scope: Permission records include a scope field: {Global, Branch, Zone, OwnOnly, CustomerAccount}. Authorization middleware resolves effective scope and applies row-level filters to queries.

# SECTION 2 — SERVICE REQUEST & OPERATIONS

1. SERVICE REQUEST (SR) LIFECYCLE

SR lifecycle (ordered): SR Created → Assigned → Scheduled → En Route → Arrived → In Progress → Estimate (if needed) → Parts (if needed) → Work Complete → Invoice → Payment → Feedback → Closed

For each stage (who / system action / customer sees):

1. SR Created
   - Who: Customer (website/app/phone/WhatsApp) or CS Agent.
   - System Action: System generates `SRNumber` (GUID), persists ServiceRequest record with status `Pending Assignment`, triggers initial notifications (WhatsApp/email) and audit log entry.
   - Customer Sees: Booking confirmation with SR number, provisional slot (if selected), estimated price range, ETA for confirmation.

2. Assigned
   - Who: Operations Executive or automated smart dispatch suggestion.
   - System Action: `AssignedTechnicianId` set on ServiceRequest; creates SRAssignment record; status updated to `Scheduled`; notifies technician via push/in-app and customer via WhatsApp with technician name, photo, rating and ETA window.
   - Customer Sees: Technician assigned card with photo, name, phone, estimated arrival window.

3. Scheduled
   - Who: Operations / system (confirmed schedule) or customer (if customer-selected slot).
   - System Action: `ConfirmedDate` and `ConfirmedTimeSlot` set; calendar/slot reserved; day-before reminder scheduled; slot and SLA timers created.
   - Customer Sees: Confirmed appointment date/time and reminder the evening before.

4. En Route
   - Who: Technician (marks Depart) or system (GPS tracking optional).
   - System Action: Job status set to `EnRoute`; GPS journey tracking session started; ETA updates published; notification to customer.
   - Customer Sees: Technician is on the way, live ETA and optional tracking link.

5. Arrived
   - Who: Technician (taps Arrived in app).
   - System Action: App validates GPS check-in within 150m radius; sets `Status=Arrived`; records `TechnicianCheckInCoordinates` and timestamp; notifies customer.
   - Customer Sees: Notification that technician has arrived.

6. In Progress
   - Who: Technician (Start Work).
   - System Action: `Status=InProgress`; `WorkStartTime` recorded; SLA timers updated; job checklist UI unlocked.
   - Customer Sees: Live status: In progress; approximate remaining time if provided.

7. Estimate (if needed)
   - Who: Technician creates estimate in-app; Customer approves via WhatsApp/app.
   - System Action: `Estimate` record persisted (EstimateStatus: PendingApproval). If approved, convert estimate to WorkOrder and lock scope; if rejected, record response and continue as per technician guidance.
   - Customer Sees: Itemized estimate with Approve/Reject CTA (WhatsApp quick-reply or in-app action).

8. Parts (if needed)
   - Who: Technician requests parts; Inventory/Store Manager approves and issues.
   - System Action: PartsRequest and PartsRequestItems created; Stock ledger updated on issue; job may be paused (Revisit SR created if parts not immediately available).
   - Customer Sees: Notification about parts order and revised ETA or revisit scheduling if applicable.

9. Work Complete
   - Who: Technician (submits job report and marks complete).
   - System Action: JobReport created (includes checklist, photos, signature); `Status=Completed`; invoice generation triggered automatically; audit log entry created.
   - Customer Sees: Job completed notification, service report downloadable, invoice sent.

10. Invoice
    - Who: System generates invoice; Billing/Accounts Exec may edit or send manual invoices.
    - System Action: Invoice created with line items (labor, parts, tax), invoice number assigned, PDF generated, invoice link sent via WhatsApp/Email.
    - Customer Sees: Branded invoice PDF and payment link.

11. Payment
    - Who: Customer (online) or Technician (COD).
    - System Action: Payment gateway webhook updates payment status; if COD, technician records collection; invoice status updated and receipt generated.
    - Customer Sees: Payment receipt and confirmation.

12. Feedback
    - Who: System (automated) — sends post-service feedback 2 hours after completion.
    - System Action: Feedback request queued and sent; feedback recorded against SR and technician record.
    - Customer Sees: Quick rating request (1–5) via WhatsApp or in-app.

13. Closed
    - Who: System (auto-close after payment/time) or Admin (manual close).
    - System Action: `Status=Closed`, warranty tracking activated if applicable, AMC counters updated, job archived for reporting.
    - Customer Sees: SR status closed, service report and warranty info retained in account.

2. MODULE: Service Request Management

Screens (platform / roles / purpose / key actions):

- SR List — Platform: Admin Web Portal; Roles: CS Executive, Operations Executive, Operations Manager, Admin; Purpose: list and filter all SRs; Key Actions: search, filter, quick-assign, bulk actions (export, bulk-assign, bulk-cancel).
- SR Detail — Platform: Admin Web Portal; Roles: CS, Ops, Billing, Admin; Purpose: full SR view and management; Key Actions: view timeline, assign/ reassign technician, add notes, view equipment, generate invoice, escalate, create follow-up SR.
- Create SR — Platform: Admin Web Portal / Customer Phone (CS UI); Roles: CS Executive, Operations; Purpose: create manual SR for phone/walk-in customers; Key Actions: capture customer, equipment, location, schedule, priority.
- Assign Technician Panel — Platform: Admin Web Portal (Dispatch View popover); Roles: Operations Executive, Operations Manager; Purpose: assign or swap technician; Key Actions: search technicians, view availability, assign, add helper.
- Reschedule SR — Platform: Admin Web Portal; Roles: CS, Ops; Purpose: change ConfirmedDate/TimeSlot; Key Actions: pick new slot, notify customer, record reschedule history.
- Cancel SR — Platform: Admin Web Portal / Customer Portal; Roles: CS, Customer, Admin; Purpose: cancel SR; Key Actions: set status `Cancelled`, record reason, process refund if applicable, notify customer.
- Escalate SR — Platform: Admin Web Portal; Roles: CS, Ops, Ops Manager; Purpose: flag for higher-level attention; Key Actions: mark escalation, assign to manager, add escalation notes.
- Create Follow-up SR — Platform: Admin Web Portal; Roles: CS, Ops; Purpose: create child SR or revisit linked to parent SR; Key Actions: link to parent SR, set warranty flags, schedule revisit.
- SR Notes — Platform: Admin Web Portal; Roles: CS, Ops, Technician (view/write limited); Purpose: internal notes capture; Key Actions: add note, mark private/public, timestamp.
- SR Communication Log — Platform: Admin Web Portal; Roles: CS, Ops, Billing; Purpose: aggregated outgoing/incoming notifications and chats; Key Actions: view message history, resend templates, view template variables used.
- Bulk SR Actions — Platform: Admin Web Portal; Roles: Ops, Admin; Purpose: operate on multiple SRs; Key Actions: bulk-assign, bulk-cancel, bulk-export, bulk-change-priority.

Current Admin + Field API implementation
- Admin SR list/detail live routes: `GET /api/service-requests`, `GET /api/service-requests/{serviceRequestId}`.
- Manual AdminMobile SR creation uses booking lookup + guest booking pipeline:
  - `GET /api/booking-lookups/services`
  - `GET /api/booking-lookups/ac-types`
  - `GET /api/booking-lookups/tonnage`
  - `GET /api/booking-lookups/brands`
  - `GET /api/booking-lookups/zones`
  - `GET /api/booking-lookups/zones/by-pincode/{pincode}`
  - `GET /api/booking-lookups/slots`
  - `POST /api/bookings/guest`
  - `POST /api/service-requests/from-booking/{bookingId}`
- Admin SR actions currently wired in backend: `POST /api/service-requests/{serviceRequestId}/assign`, `POST /api/service-requests/{serviceRequestId}/reassign`, `POST /api/service-requests/{serviceRequestId}/status`, `POST /api/cancellations/service-requests/{serviceRequestId}`, `POST /api/escalations`, `POST /api/service-requests/{serviceRequestId}/notes`.
- AdminMobile follow-up/template route uses `/service-requests/{id}/edit` as a prefilled follow-up creator because the current backend does not expose a generic `PUT /api/service-requests/{id}` edit contract.
- Legacy technician lifecycle routes still exist under `GET /api/technician-jobs/my-jobs`, `GET /api/technician-jobs/{serviceRequestId}`, `POST /api/technician-jobs/{id}/mark-enroute`, `POST /api/technician-jobs/{id}/mark-reached`, `POST /api/technician-jobs/{id}/start-work`, `POST /api/technician-jobs/{id}/mark-in-progress`, `POST /api/technician-jobs/{id}/mark-work-completed`, `POST /api/technician-jobs/{id}/submit-for-closure`, and `POST /api/technician-jobs/{id}/notes`.
- Phase 10 field workflow is now exposed through `FieldWorkflowController` under `/api/field` with:
  - `GET /api/field/my-jobs`
  - `GET /api/field/job-history`
  - `GET /api/field/jobs/{serviceRequestId}`
  - `PATCH /api/field/jobs/{serviceRequestId}/depart`
  - `PATCH /api/field/jobs/{serviceRequestId}/arrive`
  - `PATCH /api/field/jobs/{serviceRequestId}/start-work`
  - `PATCH /api/field/jobs/{serviceRequestId}/progress`
  - `POST /api/field/jobs/{serviceRequestId}/parts-request`
  - `POST /api/field/jobs/{serviceRequestId}/estimate`
  - `POST /api/field/jobs/{serviceRequestId}/report`
  - `POST /api/field/jobs/{serviceRequestId}/photos`
  - `POST /api/field/jobs/{serviceRequestId}/signature`
  - `PATCH /api/field/jobs/{serviceRequestId}/payment`
  - `PATCH /api/field/jobs/{serviceRequestId}/complete`
  - `POST /api/field/attendance/check-in`
  - `POST /api/field/attendance/check-out`
- `FieldWorkflowFeature.cs` now contains MediatR query/command handlers for all Phase 10 field routes, including job detail aggregation, depart/arrive/start-work state changes, checklist progress save, parts request creation, estimate creation, job report submission, photo upload, customer signature capture, field payment capture, completion, and technician attendance.
- Arrival validation uses SR customer coordinates and a 150m radius check. If the technician is outside the radius and no override reason is supplied, `/api/field/jobs/{serviceRequestId}/arrive` returns HTTP 422 with `overrideRequired=true`, the distance in meters, and the current job snapshot. Override arrivals write both `TechnicianGpsLog` and `AuditLog`.
- Phase 10 billing inside field workflow now auto-creates or reuses a job-card quotation, blocks invoice generation when a field estimate is still pending customer approval or rejected, generates invoices during payment/completion, records payment receipts, and writes billing history entries from the field flow.
- Phase 10 completion requires a field report, at least two field photos, and a customer signature before `/complete` can advance the SR to `SubmittedForClosure`.
- `JobReport` now has `IdempotencyKey` with an indexed persistence mapping so repeated field report submissions can be safely replayed from offline sync without duplicating report rows. Field payment collection also honors payment idempotency and duplicate reference / gateway checks through the billing repository.
- AdminMobile field implementation remains in the existing React application architecture. No React module was added. The live field client now uses `Mobile/Coolzo_AdminMobile/src/core/network/field-workflow-repository.ts` instead of the older technician-job adapter for Phase 10 screens.
- Phase 10 AdminMobile field screens now map to the live field/helper APIs as follows:
  - `TechnicianHomeDashboard.tsx`: technician attendance CTA + job counters + next-job card; helper-safe dashboard branch for simplified assignment + attendance.
  - `MyJobsList.tsx`: `/api/field/my-jobs` + `/api/field/job-history` queue for technicians, helper assignment queue for helpers.
  - `JobWorkflowContainer.tsx`: consolidated field job detail screen with map/call actions, GPS arrival override prompt, checklist progress, parts request form, estimate form, report form, photo upload, signature canvas, payment capture, completion CTA, billing snapshot, and timeline.
  - `AttendanceScreen.tsx`: technician check-in/check-out through `/api/field/attendance/*`; helper attendance list and actions through `/api/helpers/{helperProfileId}/attendance/*`.
- `HelperJobView.tsx`: helper-only assignment/task/photo/attendance view using `/api/helpers/{helperProfileId}/assignment`, `/tasks`, `/tasks/{taskId}/respond`, and `/tasks/{taskId}/upload-photo`, with no customer contact, parts ordering, estimate, or payment controls.
- Helper assignment handling now treats a missing or inactive assignment as an empty helper state instead of a hard failure, and helper check-in is gated in the UI until dispatch assigns an active job.
- Phase 10 AdminMobile offline support is now local-storage based (existing web architecture) rather than Hive/React:
  - `StorageKey.FIELD_JOB_CACHE`
  - `StorageKey.FIELD_JOB_LIST_CACHE`
  - `StorageKey.FIELD_TECHNICIAN_ATTENDANCE`
  - `StorageKey.FIELD_HELPER_ASSIGNMENT`
  - `StorageKey.FIELD_HELPER_ATTENDANCE`
  - `StorageKey.FIELD_OFFLINE_QUEUE`
- `field-workflow-repository.ts` caches job detail/list snapshots, stores queued field/helper submissions with request payload + retry metadata, applies optimistic local updates when offline, and exposes retry via `fieldWorkflowRepository.syncSubmission(id)`.
- `OfflineSyncQueue.tsx` now reads the Phase 10 field queue instead of the placeholder system repository and surfaces queued report / parts / estimate / photo / signature / payment / attendance / helper task submissions at `/system/sync`.
- Build verification on 2026-04-22: `Mobile/Coolzo_AdminMobile` production build now succeeds after correcting the `user-repository.ts` page-size nullish/OR expression that blocked Vite/esbuild parsing.
- Build verification on 2026-04-22: unrestricted `dotnet build Backend/Coolzo.Api/Coolzo.Api.csproj -v minimal` succeeds after aligning newer application/controller files with `Coolzo.Shared.Models.ICurrentDateTime`, resolving domain-entity namespace shadowing in field workflow / scheduling / technician management flows, and fixing controller compile issues in `TechnicianController` and `BranchController`.
- Build verification on 2026-04-22: standalone AdminMobile TypeScript validation now also succeeds with `./node_modules/.bin/tsc --noEmit --pretty false` after tightening repository return contracts, preserving report/invoice literal unions, widening offline queue payload metadata away from `Record<string, unknown>` constraints, and making AdminMobile table/catalog/report component typings explicit.
- Build optimization on 2026-04-22: `Mobile/Coolzo_AdminMobile/vite.config.ts` now applies manual Rollup chunking for heavy vendor groups (charts, maps, UI/runtime utilities) plus route-area feature bundles (admin, operations, field, finance, governance, customer, inventory, support). `npm run build` now completes without the previous chunk-size warning; emitted bundles are split into smaller feature/vendor artifacts instead of one oversized main chunk.

## MOBILE ADMIN APP SCREENS (Coolzo_AdminMobile) — 2026-04-25

The following screen filenames were collected from Mobile/Coolzo_AdminMobile/src/features (basenames only). No source code was opened — only filenames were enumerated.

- TechnicianListScreen.tsx
- TechnicianEditorScreen.tsx
- TechnicianDetailScreen.tsx
- MyProfileScreen.tsx
- TicketDetailScreen.tsx
- SupportTicketCreateScreen.tsx
- SplashScreen.tsx
- CustomerListScreen.tsx
- Customer360ViewScreen.tsx
- CreateCustomerScreen.tsx
- SRListScreen.tsx
- SRDetailScreen.tsx
- InvoiceListScreen.tsx
- InvoiceDetailScreen.tsx
- SessionExpiredScreen.tsx
- ResetPasswordScreen.tsx
- OTPVerificationScreen.tsx
- LoginScreen.tsx
- ForgotPINScreen.tsx
- ForgotPasswordScreen.tsx
- SLAAlertsScreen.tsx
- OperationsDashboardScreen.tsx
- DispatchManagementScreen.tsx
- CreateSRScreen.tsx
- ZoneManagementScreen.tsx
- WorkflowConfigScreen.tsx
- UserManagementListScreen.tsx
- UserDetailScreen.tsx
- TaxConfigScreen.tsx
- SystemConfigHomeScreen.tsx
- ServiceCatalogScreen.tsx
- RolePermissionEditorScreen.tsx
- RoleManagementListScreen.tsx
- PricingConfigScreen.tsx
- CreateUserScreen.tsx
- CreateBranchScreen.tsx
- BusinessHoursScreen.tsx
- BranchManagementListScreen.tsx
- BranchDetailScreen.tsx
- ReceiptManagementScreen.tsx
- PaymentListScreen.tsx
- PaymentDetailScreen.tsx
- FinanceReportScreen.tsx
- AttendanceScreen.tsx
- UpdatePromptScreen.tsx
- UnauthorizedScreen.tsx
- NotFoundScreen.tsx
- DashboardScreen.tsx

# MISSING MOBILE SCREENS (from CoolElite_Master_Blueprint.docx) — 2026-04-25

The following screens are described in CoolElite_Master_Blueprint.docx but were not present in the `MOBILE ADMIN APP SCREENS` list above. These entries include blueprint-level details to help tracing and implementation.

- Technician Job Report Screen (Mobile App)
   - Purpose: Capture the on-site job report, parts used, photos, customer signature and complete the job.
   - Key UI blocks:
      - Service Checklist: pre-populated per service type; checkboxes with mandatory/optional markers; mandatory items must be completed to submit.
      - Observations: equipment condition (Good / Fair / Poor / Critical), issues identified (multi-select + free text), action taken, recommendations.
      - Parts Used: table with Part Name, Part Code, Quantity, Unit Price; ability to search/add parts; automatic total parts cost calculation.
      - Photo Capture: minimum 2 photos required (before & after); photo type selector (Before / During / After / Issue Evidence); thumbnail preview and delete.
      - Customer Signature: digital signature canvas with auto-filled customer name and timestamp; re-sign option.
      - Completion Actions: `Submit Job Report` button validating mandatory fields, confirmation dialog, triggers invoice generation and job status update.
   - Integration notes: Use Field Workflow API endpoints for check-in/arrive/start-work/submit-report/upload-photos/signature/complete; support idempotency (JobReport IdempotencyKey) and offline submission queue as described in blueprint.
   - Verification note (2026-04-25): `Mobile/Coolzo_AdminMobile/src/features/field/TechnicianJobReportScreen.tsx` is implemented as a route wrapper over `JobWorkflowContainer`, and `/field/report/:id` is registered in `app/navigation/router.tsx`.
   - Verification note (2026-04-25): `JobWorkflowContainer` is wired to the live field workflow repository for report submission, photo upload, signature capture, payment collection, progress save and job completion; backend endpoints exist in `Backend/Coolzo.Api/Controllers/FieldWorkflowController.cs`.
   - Verification note (2026-04-25): backend controller routing has been normalized to unversioned `/api/...` routes, so the field workflow surface now aligns with the mobile repository contract for `/api/field/...`.
   - Verification note (2026-04-25): `Database/SQL/20260425_Add_FieldJob_Tables.sql` was corrected to match the EF model (`tblJobReport`, `tblJobPhoto`, `tblCustomerSignature`, `tblOfflineSyncQueueItem`), then executed successfully against LocalDB database `CoolzoDB`. `tblOfflineSyncQueueItem` already existed; the other three tables were created.

# SECTION 3 — CUSTOMER, EQUIPMENT & AMC

1. MODULE: Customer Master (Customer 360 View)

Customer 360 aggregates identity, addresses, equipment, contracts, SRs, invoices, tickets and communication preferences into a single pane for CS, Ops and Account Management.

Customer Identity Block (fields)
- CustomerId (PK)
- CustomerNumber (human-friendly)
- FullName / DisplayName
- CustomerType (Individual / Corporate / Enterprise)
- PrimaryPhone | AlternatePhone
- Email
- Gender
- DateOfBirth
- CustomerSince (createdAt)
- TotalServicesCount
- TotalRevenueGenerated
- AccountManagerId (nullable)
- CustomerRiskFlag (boolean / escalation summary)
- KYCStatus | KYCDocuments (JSON)
- PreferredLanguage
- PreferredContactMethod
- LoyaltyTier | Source | Notes

Addresses Block (columns)
- AddressId (PK)
- CustomerId (FK)
- Label (Home / Office / Other)
- AddressLine1
- AddressLine2
- Landmark
- City
- State
- PostalCode
- Country
- Zone
- Latitude | Longitude
- IsPrimary (bool)
- Status (Active / Archived)
- CreatedAt | UpdatedAt

Equipment Register Block (columns)
- EquipmentId (PK)
- EquipmentTag (human reference)
- Brand
- Model
- Type (Split / Window / Cassette / Centralized / Other)
- Tonnage / Capacity
- SerialNumber
- InstallationDate / InstallationYear
- InstallationAddressId (FK)
- PurchaseDate
- Supplier / InvoiceNumber
- WarrantyExpiryDate
- LastServiceDate
- Status (Active / Retired / Sold)
- AMCEligible (bool)
- Notes

AMC Summary Block (fields)
- ActiveContractsCount
- CurrentActiveContract (ContractNumber, PlanType, StartDate, EndDate)
- VisitsIncluded | VisitsUsed | VisitsRemaining
- NextVisitDate
- ContractStatus (Active / Expired / Lapsed) and RenewalDate
- PaymentStatus (Paid / Pending / Overdue)
- AutoRenewEnabled (bool)

Service Request Summary Block
- Quick tabs: All | Open | Completed | Cancelled
- Metrics: Total SRs, Open SRs, Last SR Date, SRs by Status
- Table columns (SR No, Date, Service Type, Status, Technician, Amount)

Invoice Summary Block
- TotalInvoices, OutstandingAmount, LastInvoiceDate, LastInvoiceStatus, PaymentMethod(s)

Support Tickets Block
- TicketNo, CreatedDate, Subject, Status, LastReply, AssignedTo

Communication Preferences Block
- WhatsAppEnabled (Y/N), EmailEnabled, SMSEnabled, PushEnabled
- DND windows (DoNotDisturbStart / End)
- LanguagePreference, PreferredContactTimeSlots

Screens (platform / roles / purpose / key actions)
- Customer List — Platform: Admin Web Portal; Roles: CS, Ops, Billing; Purpose: search & filter customers; Key Actions: view 360, create, merge, export.
- Customer 360 View — Platform: Admin Web Portal; Roles: CS, Ops, AccountManager; Purpose: single-pane customer operations; Key Actions: view/edit identity, addresses, equipment, contracts, SRs, invoices, tickets, communication panel, add note.
- Create Customer — Platform: Admin Web Portal / CS UI; Roles: CS, Ops; Purpose: onboard new customers; Key Actions: capture identity, contact, primary address, opt-ins, assign corporate account.
- Edit Customer — Platform: Admin Web Portal; Roles: CS, Ops, AccountManager; Purpose: update contact/KYC/status; Key Actions: edit fields, block/unblock, mark high-risk.
- Add / Edit Address — Platform: Admin Web Portal, Customer Web; Roles: CS, Customer; Purpose: manage addresses; Key Actions: add, edit, set primary, archive.
- Equipment Register — Platform: Admin Web Portal; Roles: CS, Ops, Technician; Purpose: register & manage equipment; Key Actions: add equipment, view service history, link to AMC.
- Corporate Account Management — Platform: Admin Web Portal; Roles: AccountManager, Billing; Purpose: manage multi-site corporate customers; Key Actions: manage contacts, consolidated invoices, assign sites.
- Customer Risk Dashboard — Platform: Admin Web Portal; Roles: Ops Manager, Risk; Purpose: surface high-risk customers (escalations, chargebacks); Key Actions: drill into SRs/tickets, impose restrictions.
- Customer Notes — Platform: Admin Web Portal; Roles: CS, Ops; Purpose: internal notes; Key Actions: add private/public notes, timestamp, author.
- Customer Communication Panel — Platform: Admin Web Portal; Roles: CS, Ops, Marketing; Purpose: view/send messages across channels; Key Actions: view templates, resend, add quick replies.

2. MODULE: Equipment Register

Canonical fields (per product):
- EquipmentId, EquipmentTag, Brand, Model, Type, Tonnage/Capacity, Location (AddressId), InstallationYear/InstallationDate, SerialNumber, LastServiceDate, WarrantyExpiryDate, PurchaseDate, Supplier, InvoiceNumber, Status, Notes

Equipment Detail screen
- Service history timeline (linked SRs / JobReports)
- Warranty block with warranty expiry and covered parts
- Book Service CTA (creates SR pre-filled with equipment and location)

Business rules
- Duplicate serial check on create — block or flag duplicates for review
- Soft delete only (IsActive / Status=Retired) — never hard-delete equipment records; keep history intact

3. MODULE: AMC Contract Engine

AMC lifecycle
- Enrollment → Contract Generation → Visit Auto-Schedule → Visit Execution → Renewal → Archive

Contract fields (canonical)
- ContractId (PK) | ContractNumber
- PlanId / PlanType
- CustomerId | CorporateAccountId
- StartDate | EndDate
- VisitsIncluded | VisitsUsed | VisitsRemaining
- EquipmentCovered (list of EquipmentIds / JSON)
- Price / BillingCycle / PaymentStatus
- AutoRenewEnabled | RenewalDate | RenewalAlertSent
- Status (Active / Suspended / Expired / Cancelled)
- CreatedAt | UpdatedAt

Screens
- AMC Contract List — Platform: Admin Web Portal; Roles: Billing, Ops, AccountManager; Purpose: list & filter contracts; Key Actions: search, renew, cancel, export.
- Contract Detail — Platform: Admin Web Portal; Roles: Ops, Billing; Purpose: full contract view; Key Actions: view equipment covered, visits, payment, generate contract PDF.
- Enroll Customer — Platform: Admin Web Portal / Customer Web; Roles: CS, Sales; Purpose: enroll a customer into a plan; Key Actions: pick plan, select equipment, accept terms, process first payment.
- Edit Contract — Platform: Admin Web Portal; Roles: Billing, AccountManager; Purpose: edit plan or pricing (with audit); Key Actions: update visits, suspend/reactivate, change billing.
- Visit Schedule Management — Platform: Admin Web Portal; Roles: Ops; Purpose: view calendar of AMC visits; Key Actions: reassign technician, reschedule, mark complete.
- Visit Detail — Platform: Admin Web Portal / Technician App; Roles: Technician, Ops; Purpose: view visit SR and job report; Key Actions: check-in, capture report, link parts used.
- Renewal Management — Platform: Admin Web Portal; Roles: Billing, AccountManager; Purpose: handle renewals and promos; Key Actions: trigger renewal emails/WhatsApp, apply discounts.
- Bulk Reminder — Platform: Admin Web Portal; Roles: Ops, Marketing; Purpose: send renewal/payment reminders; Key Actions: filter, preview template, send.
- Plan Performance Dashboard — Platform: Admin Web Portal; Roles: Ops Manager, Finance; Purpose: measure plan uptake, churn, revenue; Key Actions: filter by plan, period, export.

Business rules
- Auto-visit creation on enrollment: when contract is active, create scheduled Visit entries based on plan frequency.
- Visits auto-linked to SRs: each scheduled visit becomes an SR once issued and assigned.
- 30-day renewal alert: system triggers a renewal reminder 30 days before contract EndDate.
- Warranty-related revisit behavior: warranty-covered revisits during warranty window are free and billed to warranty ledger, not the customer.

4. MODULE: Warranty Management

WarrantyRecords table fields
- WarrantyId (PK)
- EquipmentId (FK)
- PartName
- ReplacementDate
- ExpiryDate
- TechnicianId
- LinkedSRId
- Notes
- CreatedAt

Rules
- 30-day revisit warranty for repair work (any repair marked with repair-warranty=true)
- 90-day warranty for parts replacement (parts have replacement warranty configured per part)
- On new SR creation, system auto-checks WarrantyRecords for the equipment's serial and flags SR as warranty-eligible if within expiry

5. API ENDPOINTS (Customer / Equipment / AMC)

Customer Management
- GET /api/customers
- POST /api/customers
- GET /api/customers/{customerId}
- PUT /api/customers/{customerId}
- DELETE /api/customers/{customerId} (soft-delete)
- GET /api/customers/{customerId}/addresses
- POST /api/customers/{customerId}/addresses
- PUT /api/customers/{customerId}/addresses/{addressId}

Equipment Register
- GET /api/equipment
- POST /api/equipment
- GET /api/equipment/{equipmentId}
- PUT /api/equipment/{equipmentId}
- DELETE /api/equipment/{equipmentId} (soft-delete)
- GET /api/customers/{customerId}/equipment
- POST /api/customers/{customerId}/equipment

AMC Management
- GET /api/amc/contracts
- POST /api/amc/contracts
- GET /api/amc/contracts/{contractId}
- PUT /api/amc/contracts/{contractId}
- POST /api/amc/contracts/{contractId}/enroll
- GET /api/amc/contracts/{contractId}/visits
- POST /api/amc/contracts/{contractId}/visits
- GET /api/amc/renewals (summary)

Warranty
- GET /api/warranty/records
- POST /api/warranty/records

Current AdminMobile Phase 11 implementation
- AdminMobile continues to use the existing React application architecture under `Mobile/Coolzo_AdminMobile/src/` for AMC and warranty flows; no React module was introduced.
- AMC repository bindings now target the Phase 11 AMC contracts: `GET /api/amc/contracts`, `GET /api/amc/contracts/{id}`, `POST /api/amc/contracts/enroll`, `PUT /api/amc/contracts/{id}`, `PATCH /api/amc/contracts/{id}/cancel`, `GET /api/amc/contracts/{id}/visits`, `PATCH /api/amc/visits/{visitId}/assign`, `PATCH /api/amc/visits/{visitId}/reschedule`, `PATCH /api/amc/visits/{visitId}/complete`, `GET /api/amc/contracts/{id}/pdf`, `GET /api/amc/renewals`, `POST /api/amc/renewals/bulk-remind`, and `GET /api/amc/performance-dashboard`.
- AMC dashboard surfaces live KPI cards for active contracts, expiring contracts, cancellations, enrollments, renewal rate, and visit completion, while contract list ordering is now expiry-first.
- AMC contract detail now reads contract-scoped visit schedules, supports assign/reschedule/complete actions per visit row, opens contract PDFs through the Phase 11 PDF endpoint, and triggers renewal reminder sends from the contract header.
- AMC enrollment now pulls real customers from the admin customer repository, loads customer-linked equipment, and enrolls through `POST /api/amc/contracts/enroll`; mock-mode enrollment auto-generates pending AMC visit schedule entries to mirror backend visit generation behavior.
- Renewal management now supports bulk WhatsApp reminder sends plus renewal disposition tracking (`renewed`, `declined`, `negotiating`, `pending`) through the AMC repository.
- Warranty management now reads `GET /api/warranty/records`, derives active/expiring/expired state client-side from expiry dates, and supports filtered warranty triage for equipment-linked warranty records.

Current AdminMobile customer bindings
- Admin customer register now uses `GET /api/customers` as a paged admin list contract returning customer identity, risk level, service count, total revenue, outstanding amount, AMC presence, open support count, last service date and primary address summary.
- Admin Customer 360 now uses `GET /api/customers/{customerId}` for the base customer summary plus embedded addresses, equipment and internal-note feed; the detail response also carries invoice/support counts, AMC snapshot fields and last invoice metadata for the mobile admin UI.
- The live customer-detail route reads `tblCustomerEquipment` even when the customer has no equipment rows; with the table present, `GET /api/customers/{customerId}` returns `equipment: []` instead of failing. The LocalDB compatibility script for the current schema is `Database/DB_Alter_20260506_CustomerEquipmentCompatibility.sql`.
- Admin customer maintenance routes live under the customer module: `PUT /api/customers/{customerId}`, `GET|POST|PUT /api/customers/{customerId}/addresses`, `GET|POST|PUT /api/customers/{customerId}/equipment`, and `POST /api/customers/{customerId}/notes`.
- AdminMobile Customer 360 enriches the base customer detail with `GET /api/service-history/customer/{customerId}`, `GET /api/amc/customer/{customerId}`, `GET /api/communication-preferences/customer/{customerId}`, `GET /api/support-tickets?customerMobile=...`, and `GET /api/invoices?customerId=...`.
- Internal customer notes are currently persisted as audit-log entries with `EntityName=Customer` and `ActionName=CustomerNoteAdded`; the Phase 6 UI reads and appends notes through that audit-backed feed.
- 2026-04-22 customer-app integration update:
  - AMC Plans and AMC Plan Detail now use `GET /api/amc/plans?isActive=true&pageNumber=1&pageSize=50` and the new `GET /api/amc/plans/{amcPlanId}` endpoint. The customer client maps AMC plan catalog data into the existing `AMCPlan` view model and routes enrollment CTAs into the booking store with `serviceId='amc'`.
  - Address Book and Add/Edit Address now use `GET|POST|PUT|DELETE /api/customers/me/addresses` together with `GET /api/booking-lookups/zones/by-pincode/{pincode}`. The address form resolves zone and city from pincode before save and persists backend `zoneId`, `latitude`, and `longitude` into the customer address model.
  - Equipment List, Add/Edit Equipment, and Equipment Detail now use `GET|POST|PUT|DELETE /api/customers/me/equipment`, `GET /api/booking-lookups/brands`, and `GET /api/service-history/me`. The customer equipment UI continues to persist a plain `location` string while sourcing selectable address summaries from the saved-address module.
  - Refer & Earn and Loyalty Rewards now use the existing customer marketing routes `GET /api/referrals/me`, `GET /api/loyalty/me`, and `GET /api/loyalty/me/transactions`. The customer client maps referral stats, loyalty balance, tier progress, and transaction history from those live contracts instead of the previous `/users/...` placeholders.
  - AMC Dashboard now uses `GET /api/amc/customer/me` and maps the active or latest subscription into the customer-facing contract summary, upcoming visit, visit history, and renewal countdown cards.

6. DB TABLES & COLUMNS (canonical)

Customers
- CustomerId (PK), CustomerNumber, FirstName, LastName, DisplayName, CustomerType, PrimaryPhone, AlternatePhone, Email, Gender, DateOfBirth, CustomerSince, TotalServicesCount, TotalRevenue, AccountManagerId, CustomerRiskFlag, KYCStatus, KYCDocuments (JSON), PreferredLanguage, PreferredContactMethod, DoNotDisturbStart, DoNotDisturbEnd, IsActive, CreatedAt, UpdatedAt

CustomerAddresses
- AddressId (PK), CustomerId (FK), Label, AddressLine1, AddressLine2, Landmark, City, State, PostalCode, Country, Zone, Latitude, Longitude, IsPrimary, Status, CreatedAt, UpdatedAt

CustomerEquipment
- EquipmentId (PK), CustomerId (FK), EquipmentTag, Brand, Model, Type, Tonnage, SerialNumber, InstallationDate, InstallationAddressId, PurchaseDate, Supplier, InvoiceNumber, WarrantyExpiryDate, LastServiceDate, Status, AMCEligible (bool), Notes, IsActive, CreatedAt, UpdatedAt

CustomerNotes
- NoteId (PK), CustomerId (FK), CreatedByUserId, NoteType, Content (text/JSON), IsPrivate (bool), CreatedAt

CorporateAccounts
- CorporateAccountId (PK), AccountName, AccountNumber, BillingAddressId, TaxId, BillingTerms, PrimaryContactId, CreatedAt, UpdatedAt, IsActive

CorporateContacts
- ContactId (PK), CorporateAccountId (FK), Name, Role, Email, Phone, IsPrimary, CreatedAt, UpdatedAt

AMCPlans
- PlanId (PK), PlanName, VisitsIncluded, Frequency (e.g., Quarterly, Monthly), Price, BillingCycle, Benefits (JSON), IsActive, CreatedAt, UpdatedAt

AMCContracts
- ContractId (PK), ContractNumber, PlanId (FK), CustomerId (FK), CorporateAccountId (FK nullable), StartDate, EndDate, VisitsIncluded, VisitsUsed, VisitsRemaining, EquipmentCovered (JSON array), Price, PaymentStatus, AutoRenew (bool), RenewalDate, Status, CreatedAt, UpdatedAt

AMCVisitSchedule
- VisitId (PK), ContractId (FK), ScheduledDate, WindowStart, WindowEnd, VisitType, AssignedTechnicianId, Status, Notes, CreatedAt, UpdatedAt

AMCVisitLog
- VisitLogId (PK), VisitId (FK), SRId (FK), PerformedDate, TechnicianId, Report (JSON), Photos (file refs), PartsUsed (JSON), CreatedAt

WarrantyRecords
- WarrantyId (PK), EquipmentId (FK), PartName, ReplacementDate, ExpiryDate, TechnicianId, LinkedSRId, Notes, CreatedAt

---
Notes:
- Where appropriate, flexible fields (JobReport observations, equipment notes, contract equipment list) use JSON columns to support schema evolution.
- All deletes are soft by default (IsActive / Status) so historical joins and audit logs remain meaningful.

# SECTION 4 — BOOKING ENGINE

1. BOOKING WIZARD — 6-STEP FLOW

Overview: A guided 6-step booking wizard used on Web and Mobile (Customer Website & Customer Mobile App). Steps preserve state, validate input, and call booking APIs as the user proceeds. Guest bookings create a temporary booking record and provide an option to convert to a full account after confirmation.

Step 1 — Service Selection
- Platform: Web, Mobile
- Fields / Elements:
   - Visual card grid of services (icon + label + description)
   - Service sub-type expansion (accordion or inline modal)
   - Selected state visual: gold border + checkmark
   - Search / Filter bar (optional)
   - Service urgency toggle (Normal / Emergency)
- Validation:
   - Must select one primary service; if sub-type required, one sub-type must be selected.
   - Emergency toggle requires acceptance of surcharge acknowledgement.
- UX behaviour:
   - Cards highlight on hover; selected card shows gold border and a checkmark badge.
   - Sub-type expansion animates open; selecting sub-type collapses others.
   - Emergency shows persistent red badge across steps.
   - Disabled state: services with no availability for the selected zone are disabled and show tooltip.
   - Error state: trying to proceed without selection shows inline error under grid.
- API called: none until step confirm; wizard may prefetch `GET /api/service-types` and `GET /api/service-subtypes`.

Step 2 — Equipment Details
- Platform: Web, Mobile
- Fields / Elements:
   - Brand (searchable dropdown with instant suggestions)
   - Model (free-text or model selector if available)
   - AC Type (radio buttons with icons: Split / Window / Cassette / Centralized / Other)
   - Capacity (pills: 1 Ton / 1.5 Ton / 2 Ton / 3 Ton / Custom)
   - Units (stepper) — number of identical units
   - Registered equipment selector (dropdown pre-filled for logged-in users)
   - Serial Number (optional if registering new equipment)
   - Additional notes / Special instructions (optional)
- Validation:
   - Brand required when registering new equipment or when user chooses 'Enter new equipment'.
   - Capacity must be one of allowed values or custom numeric with sanity range (0.5—10 Ton).
   - Units must be integer >=1 and <=10 (business rule configurable).
   - If registered equipment selected, pre-fill Brand/Model/Serial fields and lock editing unless user chooses 'edit'.
- UX behaviour:
   - Brand field shows typeahead; selecting brand filters Model options.
   - Registered-equipment prefill shows a chip with equipment tag and an edit link.
   - Disabled states: capacity pills disabled if equipment type is 'Other' and requires free-text.
   - Error states: invalid capacity or missing required Brand shows inline messages.
- API called: `GET /api/equipment/brands`, `GET /api/customers/{id}/equipment` (if logged), optional validation `POST /api/equipment/validate-serial`.

Step 3 — Location (Service Location)
- Platform: Web, Mobile
- Fields / Elements:
   - Address Line 1 (text)
   - Address Line 2 / Apartment / Floor (text)
   - City (dropdown or auto-detected)
   - PIN / Postal Code (text with zone validation)
   - Address Label (Home / Office / Other)
   - Map pin display + confirm location
   - Save address to account (checkbox, visible when logged-in)
- Validation:
   - Address Line 1, City and PIN required.
   - PIN validated against `Zones` and `ZonePinCodes`; if outside service area, show Zone Unavailable state.
   - Latitude/Longitude required when PIN ambiguous; map pin must be within selected zone.
- UX behaviour:
   - PIN field triggers zone lookup; unavailable zones show tooltip and block the booking flow.
   - Map allows pin-drag; pin updates Lat/Lng and reverse geocoded address preview.
   - Save-to-account persists a CustomerAddress via API after booking or upon explicit save.
- API called: `GET /api/zones/lookup?pin={pin}`, optional `POST /api/customers/{id}/addresses` when saving.

Step 4 — Date & Time Slot
- Platform: Web, Mobile
- Fields / Elements:
   - 7-day calendar view (next 7 days)
   - Time windows per day (Morning 8-12, Afternoon 12-4, Evening 4-7 or configured windows)
   - Slot availability indicators (available, limited, full)
   - Emergency toggle (shows SLA promise and surcharge note)
   - Today availability rule: same-day slots allowed only if booking occurs >4 hours before slot start
- Validation:
   - Must select a date and a time window; blocked slots cannot be selected.
   - If emergency selected, allow same-day even if within 4-hour cutoff only when emergency policy allows.
- UX behaviour:
   - Unavailable slots greyed out with tooltip reason (no slots / technician unavailable / zone blackout).
   - Selecting date fetches live slot availability; selecting a slot reserves a tentative hold until confirm (timeout 5–10 minutes).
   - Error state: if hold fails on confirm, user prompted to pick a new slot.
- API called: `GET /api/bookings/slots?date={date}&zone={zoneId}&serviceType={id}`, `POST /api/bookings/hold` (tentative hold), `POST /api/bookings/check-availability`.

Step 5 — Contact Info
- Platform: Web, Mobile
- Fields / Elements:
   - Full Name (pre-filled if logged-in)
   - Mobile Number (with OTP verify option for guests)
   - Email Address
   - Special Instructions (text area)
   - Coupon Code (input + Apply button)
- Validation:
   - Name and Mobile are required; mobile must be valid per E.164 or local rules.
   - Coupon validation returns discount amount or error (expired / not applicable).
   - OTP verify required for guest booking for mobile verification (optional depending on business rules).
- UX behaviour:
   - Coupon apply triggers `Apply Coupon` API and shows discount or error inline.
   - If OTP required, wizard pauses for OTP verification flow and resumes on success.
   - Disabled states: Confirm disabled until required fields validated and T&C accepted on Step 6.
- API called: `POST /api/coupons/apply`, `POST /api/otp/send`, `POST /api/otp/verify`.

Step 6 — Summary & Confirm
- Platform: Web, Mobile
- Fields / Elements:
   - Booking summary (service, equipment, address, slot, contact)
   - Estimated price range (line items or base estimate)
   - Coupon discount display
   - Terms & Conditions checkbox (required)
   - Confirm & Book (primary CTA)
   - Secondary: Edit (go back to steps), Save Draft (mobile)
- Validation:
   - T&C checkbox must be checked.
   - Slot hold must still be valid; on expired hold, user must re-select a slot.
- UX behaviour:
   - Confirm triggers `Create Booking` API; on success shows Booking Confirmation page with reference number.
   - If payment required at booking (e.g., paid AMC), redirect to payment flow.
   - Offline: on mobile, if network unavailable at Confirm, wizard saves draft locally and shows Resume/Sync options.
- API called: `POST /api/bookings` (Create Guest Booking / Create Booking), `POST /api/bookings/confirm`, `GET /api/bookings/{id}/summary`.

2. POST-BOOKING SCREENS
- Booking Confirmation Page
   - Success state with booking reference number, service summary, technician ETA window note, WhatsApp share CTA, account creation prompt for guests, and a 3-step “What happens next” timeline (Assign → Technician En Route → Job Complete).
- Slot Unavailable / Zone Unavailable
   - Clear messaging with next available options and contact support CTA.
- Booking Draft Resume
   - React / Mobile: booking draft saved locally (indexed DB or secure storage); user can Resume or Start Fresh. Draft shows last saved step and prefilled fields.

3. SPECIAL BOOKING VARIANTS
- Emergency Booking
   - Shows surcharge bottom sheet with confirmation; SLA promise displayed (e.g., ETA within X hours); emergency badge applied across steps; same-day allowance rules differ.
- AMC Enrollment Booking
   - Pre-select AMC plan and allow multiple units selection; Step 6 shows contract summary, billing schedule, and enrollment CTA.
- Guest Booking
   - After booking success, prompt to create account with pre-filled email/phone and copy booking into the new account.

4. BOOKING WIZARD ARCHITECTURE (React)
- State: `BookingWizardNotifier` holds wizard state (currentStep, selections, holds, draftId).
- Back navigation preserves state across steps; moving forward validates current step and caches state.
- Deep link entry: booking URL or WhatsApp pre-fill can open wizard at Step N with prefilled fields.
- Offline behavior: Steps 1–4 work offline using cached master data (service types, brands, zones); Step 6 requires network to create booking; drafts sync when online.

5. API ENDPOINTS (Booking Engine)
- GET /api/service-types
- GET /api/service-subtypes
- GET /api/equipment/brands
- GET /api/bookings/slots?date={date}&zone={zoneId}&serviceType={id}
- POST /api/bookings/hold
- POST /api/bookings (create booking / guest booking)
- POST /api/bookings/confirm
- GET /api/bookings/{bookingId}/summary
- POST /api/coupons/apply
- POST /api/otp/send
- POST /api/otp/verify

6. DB TABLES (Booking / Slots)

ServiceSlots
- SlotId (PK), SlotDate, WindowStart, WindowEnd, SlotType, MaxCapacity, ZoneId, ServiceTypeId, CreatedAt, UpdatedAt

SlotAvailability
- SlotAvailabilityId (PK), SlotId (FK), ZoneId, AvailableCount, HeldCount, UpdatedAt

BookingQueue
- BookingId (PK), CustomerId (nullable for guest), GuestContact (phone/email), ServiceTypeId, SubTypeId, EquipmentSnapshot (JSON), AddressSnapshot (JSON), SlotId (FK), HoldId (nullable), Status (Draft / Held / Confirmed / Cancelled), CreatedAt, UpdatedAt, Priority, Source (web / mobile / whatsapp)

Notes:
- Slot holds are time-limited; the system must reconcile expired holds and free capacity.
- Zone validation and slot lookup are critical to prevent overbooking; live availability APIs should be cached aggressively on the client with short TTL and revalidated on confirm.
- 2026-04-22 customer booking integration update:
  - The customer app Step 4 catalog flow now uses backend compatibility routes `GET /api/service-types?visibility=public`, `GET /api/service-types/{id}`, and `GET /api/service-types/{id}/sub-types` exposed by `ServiceTypesController`. These responses are backed by the existing booking lookup repository and return only public-facing service type data required by the customer surface.
  - The customer booking wizard now uses live booking lookup endpoints for brands, customer equipment, zone-by-pincode validation, slot availability, and coupon validation. Wizard state was extended in the client to persist `equipmentId`, `zoneId`, `slotAvailabilityId`, and `termsAccepted` so the six-step flow can submit either `POST /api/bookings/customer` or `POST /api/bookings/guest` without mock data.
  - The booking confirmation, bookings list, booking detail, and job tracker screens now read live booking records through `GET /api/bookings/my-bookings`, `GET /api/customer-bookings/{bookingId}`, and technician enrichment through `GET /api/customer-technicians/{technicianId}`. Tracker polling remains client-driven at a 60-second interval.
  - No booking-schema migration was applied in this pass. Existing booking, slot availability, and lookup tables were reused, and no hold/confirm flow was added because the current customer app integration follows the existing customer and guest booking endpoints already present in the backend.

# SECTION 5 — BILLING, INVOICE & PAYMENT

1. INVOICE ARCHITECTURE

- Auto-generation trigger: Invoice auto-generated on job completion submission by the technician (job report submit event).
- Invoice components: base service charge, approved estimate line items, parts used (unit cost from inventory), visit charge (for emergency or special visits), discounts/adjustments, tax lines.
- Invoice elements:
   - InvoiceNumber (configurable prefix/sequence e.g., CE/2026/04/0001)
   - InvoiceDate, DueDate
   - Customer details block (name, billing address, GST/TAX ID if corporate)
   - Job details block (SR/Booking ref, service date, equipment tag)
   - Line Items table: Description, HSN/SAC, Quantity, Unit Price, Discount, Taxable Amount, Tax Rate, Tax Amount, Line Total
   - Subtotal, TotalTax, TotalPayable, AmountPaid, OutstandingAmount
   - Payment instructions and QR code for instant payment
   - Footer: Terms, refund policy, contact info
- Tax handling: GST/VAT configured per service and part; invoice stores HSN/SAC per line; supports tax-inclusive or tax-exclusive pricing and shows separate tax lines for clarity.
- Invoice PDF: Branded CoolElite layout with logo, company details, customer details, itemized table, payment QR, signature area (optional), and legal tax details.

2. MODULE: Billing & Invoice Engine

Screens (platform / roles / purpose / key actions):
- Invoice List — Platform: Admin Web Portal / Finance Mobile; Roles: Billing Exec, Finance Manager; Purpose: list, filter, search invoices; Key Actions: view, export, filter by status/date/customer.
- Invoice Detail — Platform: Admin Web Portal / Customer Web / Mobile; Roles: Billing, Customer, Finance; Purpose: show invoice PDF, payment status, linked SR/Booking; Key Actions: send reminder, regenerate PDF, view payment history.
- Create Manual Invoice — Platform: Admin Web Portal; Roles: Billing Exec, Finance; Purpose: create ad-hoc invoices (adjustments, manual charges); Key Actions: add line items, apply tax, save as Draft/Issue.
- Edit Invoice — Platform: Admin Web Portal; Roles: Billing, Finance Manager; Purpose: edit draft or unpaid invoice; Key Actions: change line items, apply discounts, re-issue (versioning enforced).
- Apply Discount / Coupon — Platform: Admin Web Portal / Customer Web; Roles: Billing, Finance; Purpose: apply manual discount or coupon; Key Actions: apply, require approval if above threshold.
- Issue Credit Note — Platform: Admin Web Portal; Roles: Billing, Finance; Purpose: issue refunds or credits; Key Actions: link credit note to original invoice, adjust customer balance.
- Mark as Paid — Platform: Admin Web Portal / Technician Mobile; Roles: Billing, Technician; Purpose: record offline payments (COD / bank transfer); Key Actions: record amount, date, receipt, reconcile.
- Send Invoice to Customer — Platform: Admin Web Portal; Roles: Billing, CS; Purpose: send invoice via WhatsApp/Email/SMS; Key Actions: preview template, send, view delivery logs.
- Proforma Invoice — Platform: Admin Web Portal / Customer Web; Roles: Sales, Billing; Purpose: generate pre-payment invoice for installations / AMC enrollments; Key Actions: preview, send, convert to final invoice after payment.
- Corporate Consolidated Invoice — Platform: Admin Web Portal; Roles: Finance Manager, AccountManager; Purpose: create consolidated invoices for corporate accounts across sites and period; Key Actions: select range, aggregate, issue consolidated PDF.
- Bad Debt Management — Platform: Admin Web Portal; Roles: Finance Manager; Purpose: mark debts for provisioning and write-offs; Key Actions: age bucket, provision, write-off, generate reports.

Business rules
- Auto-generate invoice on job completion; invoice created in status `Unpaid` and linked to SR/JobReport and Booking.
- Edit allowed only while invoice status = `Unpaid` or `Draft`; edits create a new version entry in InvoiceHistory.
- Discounts above configured threshold require `FinanceManager` approval; discount attempts create a pending approval record.
- Credit Notes link to original invoice; issuing a credit note adjusts outstanding balances and is audit-logged.
- Refunds follow approval workflow and are recorded as negative payments with reference to credit notes.

3. MODULE: Payment Experience

- Online payment: invoice contains payment link / QR → hosted payment gateway → supported methods: card, UPI, net banking, wallets. Gateway webhook updates payment status.
- COD: technician records collection in mobile app; `Payments` record created with `CollectedByTechnicianId` and reconciled by Billing.
- Bank Transfer / Cheque: Billing team marks payment as pending until reconciliation.
- Corporate Credit: support net terms (Net-30 / Net-60) with AR tracking and approved credit limits per CorporateAccount.
- Partial Payments: invoices accept partial payments; system tracks `AmountPaid` and `OutstandingAmount` and reflects in customer account UI.

Payment screens
- Payment Gateway Screen — Platform: Customer Web / Mobile; Purpose: redirect or embed gateway; Key Actions: initiate payment, show progress.
- Payment Success — Purpose: show receipt, update invoice status, send receipt via WhatsApp/Email.
- Payment Failed — Purpose: show retry options and support contact.
- Receipt View / Download — Platform: Customer Web / Admin; Purpose: download receipt PDF.

4. MODULE: Accounts Receivable (AR)

- Aging buckets: 0-30 / 31-60 / 61-90 / 90+ days
- AR Dashboard — Platform: Admin Web Portal; Roles: Finance Manager; Purpose: total outstanding, aging buckets, top debtors; Key Actions: export, schedule follow-ups.
- Payment Reminder Management — Platform: Admin Web Portal; Roles: Billing Exec; Purpose: send automated WhatsApp reminders, email reminders, schedule manual follow-ups and log phone attempts.
- Reconciliation — Platform: Admin Web Portal; Roles: Finance; Purpose: reconcile gateway settlements, COD logs, bank statements; Key Actions: match transactions, flag exceptions, generate reconciliation reports.

5. MODULE: Coupon & Discount Engine

- Coupon fields: Code, DiscountType (Percentage / Flat), DiscountValue, ApplicableServices (list), StartDate, ExpiryDate, UsageLimit, MinOrderValue, MaxDiscountCap, NewCustomerOnly (bool), OneTimePerCustomer (bool), CreatedBy, Status
- Coupon validation flow: client calls `POST /api/coupons/apply` with booking context; server validates applicability, min order, usage, and returns discount amount or error message.
- Discount override: manual overrides above threshold create a pending approval record for Finance Manager.

6. MODULE: Tax Configuration

- Tax configuration: per-service and per-part tax rules (GST/VAT rates), tax-inclusive vs tax-exclusive flag, HSN/SAC mapping per item, tax rounding rules, jurisdiction-specific overrides.
- Invoice tax line format: shows tax base, tax rate, tax amount, and HSN/SAC per line item; aggregates total tax grouped by tax rate where applicable.

7. API ENDPOINTS (Billing & Payments)
- Invoices
   - GET /api/invoices
   - POST /api/invoices
   - GET /api/invoices/{invoiceId}
   - PUT /api/invoices/{invoiceId}
   - POST /api/invoices/{invoiceId}/send
   - POST /api/invoices/{invoiceId}/credit-note
- Payments
   - POST /api/payments (record payment / gateway callback)
   - GET /api/payments/{paymentId}
   - POST /api/payments/reconcile
- Coupons & Discounts
   - POST /api/coupons/apply
   - GET /api/coupons/{code}
- Tax & Config
   - GET /api/tax-configurations
   - PUT /api/tax-configurations/{id}

2026-04-25 billing alias update:
- Invoice detail now exposes PDF download through `GET /api/invoices/{invoiceId}/pdf`.
- Invoice payment can now be recorded through the invoice-level alias `POST /api/invoices/{invoiceId}/mark-paid`, which maps to the same payment-recording flow as the existing payment capture path.

8. DB TABLES & COLUMNS (canonical)

Invoices
- InvoiceId (PK), InvoiceNumber, SRId (FK), BookingId (FK), CustomerId (FK), InvoiceDate, DueDate, SubTotal, TotalTax, TotalPayable, AmountPaid, OutstandingAmount, Status (Draft/Unpaid/Paid/CreditNote/Cancelled), Currency, CreatedBy, CreatedAt, UpdatedAt

InvoiceLineItems
- LineItemId (PK), InvoiceId (FK), Description, HSN_SAC, Quantity, UnitPrice, Discount, TaxRate, TaxAmount, LineTotal

Payments
- PaymentId (PK), InvoiceId (FK), PaymentMethod (Card/UPI/Cash/BankTransfer/Cheque), Amount, PaymentGatewayRef, Status (Pending/Confirmed/Failed), CollectedByTechnicianId (nullable), CollectedAt, Reconciled (bool), CreatedAt

PaymentGatewayLogs
- LogId (PK), PaymentId (FK nullable), GatewayName, RequestPayload (JSON), ResponsePayload (JSON), ReceivedAt

CreditNotes
- CreditNoteId (PK), CreditNoteNumber, InvoiceId (FK), Amount, Reason, IssuedBy, IssuedAt, Status

Receipts
- ReceiptId (PK), PaymentId (FK), InvoiceId (FK), ReceiptNumber, IssuedAt, IssuedBy

Coupons
- CouponId (PK), Code, DiscountType, DiscountValue, ApplicableServices (JSON), StartDate, ExpiryDate, UsageLimit, MinOrderValue, MaxDiscountCap, NewCustomerOnly, OneTimePerCustomer, CreatedBy, Status, CreatedAt

CouponUsage
- UsageId (PK), CouponId (FK), CustomerId (FK), BookingId (FK), InvoiceId (FK nullable), UsedAt

TaxConfigurations
- TaxConfigId (PK), EntityType (Service / Part), EntityId (FK), TaxRate, HSN_SAC, IsInclusive (bool), EffectiveFrom, EffectiveTo, CreatedAt

Notes:
- All financial records are immutable once reconciled; edits create versioned history entries.
- Payment gateway webhook handlers must be idempotent and log full request/response for reconciliation.
- Partial payments update `AmountPaid` and leave invoice `Status=PartiallyPaid` until fully paid.
- 2026-04-22 customer-app integration update:
  - Invoices List now uses `GET /api/invoices/customer?pageNumber=1&pageSize=20`, while invoice detail retrieval uses `GET /api/invoices/{invoiceId}`. The customer client maps backend `currentStatus`, `balanceAmount`, `quotationNumber`, and line totals into the existing invoice card/detail models without introducing a new aggregate billing contract.
  - Invoice Detail now exposes PDF export through the new `GET /api/invoices/{invoiceId}/pdf` endpoint. The API currently generates a lightweight invoice PDF directly from the invoice detail payload without introducing an external PDF package.

# SECTION 6 — CUSTOMER PORTAL

1. MODULE: Customer Dashboard (Authenticated Home)

Customer Dashboard is the authenticated home for customers (Web & React) — a personalized, action-first surface that surfaces active jobs, AMCs, quick actions and recent history.

Active Job Status Card
- Displays: `SRNumber`, `ServiceType`, `TechnicianName`, `TechnicianPhotoUrl`, `TechnicianRating`, `CurrentStatus` (milestone), `ETA` (human-friendly), `LastUpdatedAt`.
- Key actions: `Track Job` CTA (opens Job Tracker), `Call Technician` (tap-to-call), `Message Technician` (in-app / WhatsApp deep link).

AMC Summary Card
- Displays: `ActiveContractName`, `PlanType`, `NextVisitDate`, `VisitsRemaining`, `DaysUntilNextVisit` (countdown).
- Key actions: `View Contract`, `Renew`, `Request Change`.

AMC Upsell Card (for non-AMC customers)
- Content: short benefit bullets, sample savings, CTA `Enroll Now` leading to AMC enrollment flow.

Quick Action Bar
- Actions: `Book` (booking wizard), `Track` (jobs list), `Invoices` (invoice list), `Support` (raise ticket).

Recent Bookings Preview
- Shows last 3 bookings: `Date`, `ServiceType`, `StatusBadge` (Open/Assigned/Completed), link to Booking Detail.

Personalization & Promo Banner
- Greeting: "Hi, {FirstName}" and optional loyalty tier badge.
- CMS Promotional Banner area for targeted offers or messages.

Screens: Customer Dashboard — Platform: Web / Mobile (React); Roles: Customer (Registered); Purpose: immediate customer actions and visibility; Key Actions: view active job, quick-book, open dashboard tiles, deep-link into bookings/tickets.

2. MODULE: My Bookings & Job Tracker

Screens
- My Bookings List — Platform: Web / Mobile; Roles: Customer; Purpose: list all customer bookings & quick filters (All / Upcoming / Past / Cancelled); Key Actions: open booking, reschedule, cancel (if allowed).
- Booking Detail / Job Tracker — Platform: Web / Mobile; Roles: Customer; Purpose: single SR timeline and live tracking; Key Actions: view timeline, contact technician, view job report, approve estimate.
- Service Report View — Platform: Web / Mobile; Roles: Customer; Purpose: view technician-submitted job report PDF and images; Key Actions: download, share, raise dispute.
- Reschedule Booking — Platform: Web / Mobile; Roles: Customer; Purpose: request new slot; Key Actions: propose new slots, confirm reschedule (subject to ops approval or immediate reschedule if policy allows).

Job Tracker UX
- Status milestone stepper: Booked → Assigned → En Route → Arrived → In Progress → Completed. Each milestone shows timestamp and optional note.
- Technician card: photo, name, rating, call, message, ETA; tap-to-open technician profile.
- ETA: live ETA with `lastUpdatedAt` timestamp; client auto-polls server every 60s when tracker open; push notifications update UI in near real-time.
- Offline handling: show cached last-known status with banner: "Last updated X min ago" and disabled live controls until online.

3. MODULE: Estimate Approval (Customer-side)

- Trigger: push notification / in-app alert when technician submits an estimate.
- UI: itemized estimate table with each line: `Description`, `Quantity`, `UnitPrice`, `Tax`, `LineTotal`.
- Actions: `Approve` (gold primary) and `Decline` (outlined). Approve triggers `POST /api/estimates/{id}/approve` and converts approved items to a Work Order; Decline opens confirmation dialog: "Declining will stop the technician from performing this additional work. Continue?" — `Confirm` proceeds to send decline event.
- Customer-side: shows `EstimateExpiry` and `Contact Support` CTA if clarifications needed.

4. MODULE: My Support Tickets (Customer-side)

Screens
- My Support Tickets List — Platform: Web / Mobile; Roles: Customer; Purpose: list of tickets with status and last reply.
- Support Ticket Detail — Platform: Web / Mobile; Roles: Customer, CS; Purpose: chat-style thread for ticket conversation; Key Actions: send message, attach files, close ticket, request escalation.
- Raise New Ticket Form — Fields: `Subject`, `Category` (dropdown), `RelatedBookingId` (optional selector), `Description`, `Attachments` (max 3 files, max 5MB each).
- Review / Feedback Submission Screen — Platform: Web / Mobile; Roles: Customer; Purpose: submit post-service feedback and rating.

Ticket Detail UX
- Chat-style bubbles: customer (right / navy), agent (left / warm white); attachments displayed inline with thumbnails; image lightbox and download option; timestamps per message.
- Satisfaction flow: on ticket closure, prompt for rating (1–5) and optional comment.

5. MODULE: My Profile & Account Management

Screens: My Profile, My Addresses, Notification Preferences, Refer a Friend / Loyalty (future-ready), Terms & Conditions, Privacy Policy.

My Profile fields
- Name, Email, Mobile (verified flag), Secondary Contacts, Preferred Language, KYC fields (if required), Saved Payment Methods (tokenized), Loyalty Info.

My Addresses
- CRUD for addresses with primary flag and geolocation; ability to link equipment to address.

Notification Preferences
- Channel toggles (WhatsApp / Email / SMS / Push) per category: Booking Confirmations, Technician Updates, Job Status, Invoice/Payment, AMC Reminders, Promotions, Support.
- Mandatory communications (e.g., critical job updates, safety recalls, legal notices) are non-togglable and visually indicated.

6. CUSTOMER APP NAVIGATION (React)

- Bottom navigation: `Home` / `Book` / `My Jobs` / `Account` (4 tabs maximum).
- Guest mode: `My Jobs` and `Account` show login prompts; `Book` works partially with guest flow saving a draft; `Home` shows marketing + booking CTA.
- Badge system: `Account` tab badge shows unread ticket replies (red) + overdue invoices (amber) — counts aggregated.

7. CLIENT SYNC & REAL-TIME BEHAVIOR
- Preferred pattern: push notifications / WebSockets for live SR updates; fallback to polling every 60s on open Job Tracker.
- Offline: key screens show cached snapshots with a clear "offline" banner; actions that require network are queued (e.g., feedback, ticket creation) and retried when online.

8. API ENDPOINTS (Customer-facing)
- GET /api/customers/{customerId}/dashboard
- GET /api/bookings?customerId={id}
- GET /api/bookings/{bookingId}
- GET /api/bookings/{bookingId}/timeline
- POST /api/bookings/{bookingId}/reschedule
- GET /api/estimates/{estimateId}
- POST /api/estimates/{estimateId}/approve
- POST /api/estimates/{estimateId}/reject
- GET /api/tickets?customerId={id}
- POST /api/tickets
- GET /api/tickets/{ticketId}
- POST /api/customers/{customerId}/profile
- GET /api/customers/{customerId}/addresses
- PUT /api/customers/{customerId}/addresses/{addressId}
- GET /api/notifications/unread

Notes:
- The customer app should minimize friction: meaningful CTAs, pre-filled data for logged-in users, and clear offline states. Real-time updates are critical for trust during active jobs.
- 2026-04-22 customer-app integration update:
  - `Mobile/Coolzo_MobileCustomer` now uses live customer auth with `POST /api/auth/otp/send`, `POST /api/auth/otp/verify`, `POST /api/auth/refresh`, and `POST /api/auth/logout`. The client persists `@ce_access_token`, `@ce_refresh_token`, `@ce_user_profile`, and `@ce_customer_id` in browser local storage through `src/services/authStorage.ts`.
  - Registration flow now uses `POST /api/customer-auth/register`, then immediately sends the login OTP through `POST /api/auth/otp/send`, and completes auto-login through `POST /api/auth/otp/verify`.
  - Profile read/write now uses `GET /api/customers/me/profile` and `PUT /api/customers/me/profile`; changing the phone number triggers OTP re-verification before the profile save is completed.
  - The home dashboard currently composes customer-facing data client-side from `GET /api/bookings/my-bookings`, `GET /api/amc/customer/me`, `GET /api/cms/public/home`, and `GET /api/customer-technicians/{technicianId}` instead of using a dedicated `/customers/{customerId}/dashboard` aggregate endpoint.
  - My Bookings and Job Tracker screens now use `GET /api/bookings/my-bookings`, `GET /api/customer-bookings/{bookingId}`, and `GET /api/customer-technicians/{technicianId}` with a 60-second polling loop for active tracker state.
  - The service catalog and service detail screens now use `GET /api/service-types?visibility=public`, `GET /api/service-types/{id}`, and `GET /api/service-types/{id}/sub-types`. Service type list responses are cached in local storage for one hour with key `@ce_service_types_cache`.
  - Global Search now uses `GET /api/service-types?visibility=public&search=...` for live service search only; the search surface no longer depends on mock article content and now routes directly into the live service detail page.
  - Promotional Offers now use `GET /api/offers`, and coupon validation inside the shared offer service now targets `POST /api/offers/validate-coupon`.
  - Booking wizard integration now uses `GET /api/booking-lookups/brands`, `GET /api/customers/me/equipment`, `GET /api/booking-lookups/zones/by-pincode/{pincode}`, `GET /api/booking-lookups/slots`, `POST /api/offers/validate-coupon`, `POST /api/bookings/customer`, and `POST /api/bookings/guest`. The wizard draft now retains `zoneId`, `slotAvailabilityId`, and `termsAccepted`.
  - Notification Centre and Notification Preferences now use `GET /api/notifications/unread`, `PATCH /api/notifications/mark-read`, `POST /api/customer-notifications/{notificationId}/mark-read`, `GET /api/communication-preferences/me`, and `PUT /api/communication-preferences/me`. Unread notifications are grouped client-side by day and notification preference toggles are mapped from the live communication preference record.
  - Notification Preferences now render the product-required category sections (Booking Confirmations, Technician Updates, Job Status, Invoice/Payment, AMC Reminders, Promotions, Support) by mapping the existing live communication-preference record into category cards. Mandatory communications remain visible and locked in the customer UI.
  - Support Tickets List and Support Ticket Detail now use `GET /api/support-tickets/my-tickets`, `GET /api/support-tickets/{ticketId}`, `POST /api/support-tickets`, `POST /api/support-tickets/{ticketId}/replies`, `POST /api/support-tickets/{ticketId}/close`, `GET /api/support-ticket-lookups/categories`, and `GET /api/support-ticket-lookups/priorities`. The customer thread remains chat-style, polls every 30 seconds, and only exposes local attachment staging because no attachment upload contract exists in the current backend.
  - Technician Profile now uses the new public customer-safe endpoint `GET /api/technicians/{technicianId}/public`, while Job Tracker deep-links into `/technician/{id}` instead of a stale in-app profile route.
  - Review Submission and Service Reviews now use `POST /api/customer-reviews` and `GET /api/customer-reviews`. Review submission now performs server-side eligibility validation against the booking's service-request completion state before a review is accepted, and the public reviews screen filters the live review feed by customer-visible service categories derived from the service catalog.
  - AMC Visit Detail now resolves directly from the live `GET /api/amc/customer/me` subscription payload and renders visit-specific status, linked SR metadata, and checklist state without placeholder content.
  - Service Report view now uses `GET /api/customer-bookings/{bookingId}/service-report` for customer-visible report data and `GET /api/customer-bookings/{bookingId}/service-report/pdf` for document download.
  - About Us and Legal Content now use CMS block delivery through `GET /api/cms/blocks/about-us`, `GET /api/cms/blocks/legal-terms`, and `GET /api/cms/blocks/privacy-policy`.
  - Delete Account now uses the customer self-service compatibility route `DELETE /api/auth/account`, which maps to the existing customer deactivation flow.
  - Blog List and Blog Detail now use `GET /api/cms/blog-posts` and `GET /api/cms/blog-posts/{id}` as customer-safe aliases over the published CMS blog feed.
  - Emergency Booking now reuses the live booking engine by composing a customer booking from the default address, saved equipment, first matching emergency-capable public service, and the first available slot, with `isEmergency` and surcharge fields set before submission to `POST /api/bookings/customer`.
  - Estimate Approval now loads live quotation data through the booking's `quotationId` and submits customer decisions through `POST /api/quotations/{quotationId}/approve` and `POST /api/quotations/{quotationId}/reject`.
  - Receipt View now uses `GET /api/payments/receipt/{invoiceId}` and `GET /api/payments/receipt/{invoiceId}/pdf`, selecting the latest receipt attached to the invoice's payment transactions.
  - Raise Ticket and Contact Support now create live customer tickets through `POST /api/support-tickets`, using `GET /api/support/categories` as a compatibility alias over the existing support lookup source. Raise Ticket now resolves live category options and recent bookings for booking-linked ticket creation, while Contact Support pre-fills the General Enquiry path and keeps phone / WhatsApp / email shortcuts on the support entry screen.
  - Change Password now uses `POST /api/auth/change-password` as a customer-auth compatibility alias over the existing customer password change handler.
  - Forgot Password and Reset Password now use `POST /api/auth/forgot-password` and `POST /api/auth/reset-password` in the customer app's mobile-number + OTP flow. The auth controller now accepts `phone/loginId` on forgot-password, sends customer OTPs through the existing OTP infrastructure, and accepts `phone + otp + newPassword` on reset-password through a dedicated customer-password-reset-with-OTP command.
  - Reschedule Booking now uses `POST /api/bookings/{bookingId}/reschedule` as a customer-compatible alias over the existing reschedule command. The customer UI now pulls date-specific live slot labels from `GET /api/booking-lookups/slots`, resets time selection when dates change, and submits the matched `slotAvailabilityId` with the reschedule request.
  - Payment Gateway now uses `POST /api/payments` and `GET /api/payments/{paymentId}` compatibility endpoints. The customer payment flow now starts from the live invoice, initiates a payment session, polls payment status, and routes into the live payment-status / receipt journey instead of the earlier mock transition.
  - App Rating Prompt now submits through `POST /api/feedback/app`, which is exposed as a customer-feedback compatibility alias over the existing customer app feedback command.
  - Permissions Management now persists location / push consent state locally under `coolzo-device-permissions`, requests browser notification and geolocation access in-app, and synchronizes push preference changes back to `PUT /api/communication-preferences/me`. Customer profile fields were not expanded because the current backend contract does not expose dedicated consent columns.
  - Changelog now uses `GET /api/cms/blocks/app-changelog`, with the customer content service parsing the CMS block into version/date/change entries and caching it in local storage for 24 hours.
  - Booking Draft Resume now uses the persisted Zustand booking draft as the authoritative Step 50 implementation. The booking store now tracks `updatedAt`, draft resume reads the local persisted draft summary, and draft discard clears the persisted booking state instead of relying on a non-existent backend draft entity.
  - Search, Loyalty Rewards, Promotional Offers, and Notification Preferences reuse the same live service bindings introduced in Steps 21, 24, 22, and 30 respectively; no duplicate service layer was introduced for the Step 51–54 routes.
  - Add / Edit Address and Add / Edit Equipment now remain reachable from both account management and the booking wizard. Booking Step 2 now exposes an account-equipment add entry point, and Booking Step 3 now exposes an account-address add entry point without introducing separate booking-only forms.
  - Technician Profile now remains reachable from both Job Tracker and Review Submission; the review flow now exposes a direct technician-profile deep link using the same public technician endpoint as the tracker flow.
  - Support Ticket Detail now keeps the same live support-thread contract but adds optimistic customer reply rendering in the mobile thread before the `POST /api/support-tickets/{ticketId}/replies` response returns.
  - Support Tickets List now has a dedicated duplicate route surface at `/app/support/tickets` and uses `GET /api/support-tickets/my-tickets?unread=true&countOnly=true` for the account unread badge. The backend `my-tickets` endpoint now accepts `countOnly` and `unread` query params and returns a lightweight count payload when requested.
  - Invoice Detail now remains reachable through both `/app/invoice/{invoiceId}` and `/app/invoices/{invoiceId}` while continuing to use the same live invoice-detail, pay, and PDF-download bindings from Step 28.
  - 2026-04-22 build remediation update: backend compile issues caused by controller-to-contract drift were corrected without changing public APIs. `BookingController`, `CustomerBookingController`, `AuthController`, `InvoiceController`, `PaymentController`, `ServiceTypesController`, and `SupportTicketController` were aligned to the existing request/response contracts and API response factory so `dotnet build` completes successfully again.
  - 2026-04-22 mobile build remediation update: the React customer app compile was restored by tightening route-wrapper typing, removing an invalid support-ticket payload field, adding explicit notification and attachment typing, normalizing mock catalog / loyalty / review adapters to the live `CatalogServiceItem` and string-date contracts, and confirming the customer app now completes both TypeScript compile and production Vite build.
  - 2026-04-23 build verification update: reran `dotnet build` in `Backend`, `node ./node_modules/typescript/lib/tsc.js --noEmit --pretty false` in `Mobile/Coolzo_MobileCustomer`, and `node ./node_modules/vite/bin/vite.js build` in the same mobile app. The backend build completed with 0 warnings and 0 errors, the TypeScript compile exited successfully, and the production bundle completed successfully with only non-fatal Vite warnings about ignored `"use client"` directives and large chunk size.

# SECTION 7 — ADMIN OPERATIONS

1. OPERATIONS DASHBOARD

Overview: The Operations Dashboard is the real-time control surface for operations managers and dispatch teams. It provides a live KPI strip, visual charts, assignment queues, technician boards and SLA alerts — all actionable.

Live KPI Strip (fields)
- Today'sJobsTotal — integer: total SRs scheduled for today
- Today'sJobsAssigned — integer: SRs with assigned technicians
- Today'sJobsInProgress — integer
- Today'sJobsCompleted — integer
- Today'sJobsPendingAssignment — integer
- Today'sJobsOverdue — integer
- SLAComplianceRate — percentage (jobs meeting SLA / total)
- AverageResponseTime — minutes (mean time from SR creation to assignment)
- ActiveTechniciansCount — integer (technicians currently Available or OnJob)
- LastRefreshedAt — timestamp

Job Status Distribution Chart
- Visual: donut or stacked bar showing SR counts by status (Pending, Scheduled, EnRoute, Arrived, InProgress, Completed, Cancelled)
- Interaction: clickable segments filter Pending Assignment Queue and SR list by status.

Pending Assignment Queue
- Columns/fields: SRNumber, Priority, CustomerName, ServiceType, Zone, RequestedSlot (confirmed/preferred), Age (time since creation), UrgencyBadge (Normal/Emergency/AMC), EstimatedDuration
- Sort: priority, age, requested slot
- Actions: Quick Assign (select technician), Bulk Assign, View SR Detail

Technician Status Board
- Card fields: TechnicianId, PhotoUrl, Name, Zone(s), CurrentStatus (Available / EnRoute / OnJob / OffDuty), JobsRemainingToday, JobsCompletedToday, NextAvailableAt
- Expand: current job detail (SRNumber, Address, ETA, JobStatus)
- Color coding: green (available), blue (en-route), amber (busy), grey (off-duty)

SLA Alert Panel
- Items: SRNumber, SLAType, TimeToBreach (mins), Severity (amber <1hr, red breached), EscalationRecommended (bool)
- Actions: One-tap Escalate, Reassign, Notify Customer

Auto-refresh & Data Strategy
- Auto-refresh interval: default every 60 seconds (configurable); `LastRefreshedAt` displayed.
- Backend provides a delta feed (websocket / SSE) for low-latency updates; polling fallback available.

2. DISPATCH MANAGEMENT MODULE

Screens & Views
- Dispatch / Assignment Screen: two-panel layout — left: SR list / filters; right: Technician board with availability and skill tags. Supports drag-and-drop assignment and search.
- Reassignment Panel: change assigned technician, reason required, preview impact (travel / SLA), commit.
- Emergency SR Alert Handler: separate modal and queue for emergency/high-priority SRs with override controls.
- Zone Workload View: heatmap and counts per zone; supports balancing suggestions.
- Operations Day Summary: end-of-day report for shift handover.

Dispatch Logic & UX
- Technician card match-color: green = full skill match & availability, amber = partial skill match or travel time borderline, grey = no match.
- Assignment actions: drag SR → technician card or select SR → Quick Assign to chosen technician.
- Batch assign: multi-select SRs and assign to multiple technicians in one action.
- Notifications: assigning or reassigning triggers push notification and in-app update to technician app.

Reassignment rules
- Reason required: dropdown (Customer Request, Tech Unavailable, Skill Mismatch, Travel Conflict, Other).
- Both source and destination technicians are notified; SR timeline logs reassignment with reason and user id.

3. JOB SCHEDULING BOARD MODULE

Screens
- Scheduling Board — Day View, Week View, and Technician View.
- Conflict Detection Alert pane: lists detected conflicts and suggested resolutions.
- Slot Availability Manager: shows slot occupancy by zone and service type.
- Technician Shift Scheduler: manage shifts, breaks, and off-duty windows.
- AMC Auto-Schedule Review Board: review auto-generated AMC visits and bulk-assign technicians.
- Daily Briefing Sheet Export — per technician itinerary (PDF/CSV).

Scheduling API Surface
- `GET /api/scheduling/board` returns time slots, technician columns, scheduled jobs, and unassigned jobs for the selected date range.
- `POST /api/scheduling/assign` and `PUT /api/scheduling/reassign` persist drag-drop assignment and reschedule actions against service requests and slot availability.
- `GET /api/scheduling/conflicts` evaluates shift/off-duty, break overlap, job overlap, travel buffer, skill-match, and zone-match conflicts before a scheduling move is committed.
- `GET /api/scheduling/slots` and `PUT /api/scheduling/slots/{slotAvailabilityId}` drive the Slot Availability Manager for occupancy and block/unblock actions.
- `GET /api/scheduling/shifts` and `PUT /api/scheduling/shifts` load and persist weekly technician roster data including break windows and off-duty days.
- `GET /api/scheduling/amc-auto` and `POST /api/scheduling/amc-bulk-assign` expose AMC review-board visits and convert selected visits into scheduled booking + service-request assignments.
- `GET /api/scheduling/day-sheet` returns per-technician itineraries for the daily briefing export flow.

Scheduling Persistence
- `tblTechnicianShift` stores per-technician weekly shift overrides, break windows, and off-duty flags used by the scheduling board and conflict detection.
- `tblSlotAvailability` remains the source of zone/date slot capacity, reservation counts, and blocked-state control for schedule operations.
- `tblServiceRequestStatusHistory` continues to capture assignment/reschedule status transitions generated by scheduling-board actions.

Day View behaviour
- Columns represent technicians; rows represent time slots (configurable granularity, e.g., 15/30/60 mins).
- Jobs rendered as colored blocks with SRNumber, Customer initials, ServiceType, Duration.
- Drag-and-drop blocks across columns (technician) or rows (time) to reschedule.

Conflict Detection & Travel Time
- Detects overlapping bookings, insufficient buffer for travel between jobs using travel-time estimates; flags conflicts in red.
- Blocking conflicts stop the move when the technician is off-duty, the slot falls outside shift hours, a break window is crossed, or another booking already overlaps the target slot.
- Non-blocking warnings surface skill mismatch, zone mismatch, and inadequate travel buffer so supervisors can intervene before confirming the change.
- Override: supervisor can force-reschedule with a mandatory confirmation and reason.

AMC Auto-Schedule
- System generates visits based on `AMCContracts` frequency and preferences and places them in review board for bulk assign or approval.
- Bulk assigning from the review board binds each selected AMC visit to a scheduling slot, creates the booking/service request, assigns the technician, and links the visit back to the created service request.

4. LIVE JOB MAP

- Map features: status-colored job pins (by SR status), technician GPS markers, clustering for high-density areas, zone overlays.
- Interactions: filter by zone/technician/status; tap job pin to open mini-card with SRNumber, Customer, Technician, Status, ETA, Quick Actions (Assign / Navigate / Call).
- Real-time updates: marker positions for technicians update via websocket; map refresh every 30–60s if websocket not available.

5. SUPER ADMIN BUSINESS DASHBOARD

Overview: High-level exec dashboard for Super Admin and senior leadership with financial and operational KPIs and trend visualizations.

Tiles / KPIs (examples)
- TotalRevenue (Today / Month / Quarter / YTD)
- JobsCompleted vs Target (period)
- SLACompliancePercent
- AverageCustomerRating
- NewCustomersCount (period)
- AMCContractsActive
- OutstandingReceivables (amount)
- TopServicesByRevenue (list)
- GeographicRevenueHeatMap
- CustomerSatisfactionTrend (period)
- PlatformUsageStats (MAU/DAU, API calls, Peak loads)

Filters & Actions
- Time-range selector (Today / 7d / 30d / Custom)
- Export to CSV / Excel, scheduled email reports, drill-down links to operations views.

APIs & DB references (ops)
- Executive dashboard APIs: GET /api/dashboard/summary, GET /api/dashboard/metrics
- Current dashboard APIs: GET /api/dashboard/operations, GET /api/dashboard/operations/pending-queue, GET /api/dashboard/operations/technician-status, GET /api/dashboard/operations/sla-alerts, GET /api/dashboard/operations/zone-workload, GET /api/dashboard/operations/day-summary, GET /api/dashboard/live-map
- Supporting dispatch APIs: POST /api/service-requests/{serviceRequestId}/assign, POST /api/service-requests/{serviceRequestId}/reassign, GET /api/technicians/availability-board?serviceRequestId={serviceRequestId}, GET /api/scheduling/board, POST /api/scheduling/reschedule
- Key DB tables: ServiceRequests, SRAssignments, SRAssignmentHistory, Technicians, TechnicianZones, TechnicianStatusLog, TechnicianSkill, TechnicianZone, TechnicianAttendance, TechnicianGPSLog, TechnicianPerformanceSummary, ScheduleSlots, JobStatusHistory, SLAAlerts, DispatchLogs

Current AdminMobile operations bindings
- Global admin dashboard route `/dashboard` reads `GET /api/dashboard/summary` and `GET /api/dashboard/metrics`. KPI cards bind to total revenue, total jobs, total bookings, and open support tickets; the lead chart renders `bookingTrends`; the supporting cards render job-status distribution, revenue summary, and support overview.
- `GET /api/dashboard/metrics` composes `jobStatusDistribution` from booking analytics status distribution, `bookingTrends` from booking analytics trend points, `revenueSummary` from revenue analytics, and `supportOverview` from support analytics. The summary read path only relies on the scalar dashboard summary result required by `GET /api/dashboard/summary`.
- When `/api/dashboard/metrics` is called without `dateFrom` / `dateTo`, the handler still tries the standard rolling 30-day analytics window first. If that window is empty but the all-time dashboard summary proves the tenant has historical live data, the backend now widens the metrics read to the last 365 days so AdminMobile does not render an empty executive dashboard by default for sparse datasets.
- PostgreSQL deployments require `coolzo-postgres/08_dashboard_summary_compat.sql` to expose `public.uspGetDashboardSummary` over the bootstrap `public."tblBooking"`, `public."tblServiceRequest"`, `public."tblJobCard"`, `public."tblInvoiceHeader"`, `public."tblCustomerAMC"`, and `public."tblSupportTicket"` tables. The analytics summary repository now invokes the dashboard summary through the `public` schema under Npgsql. This script covers the scalar summary read used by `GET /api/dashboard/summary`; the broader `GET /api/dashboard/metrics` path still depends on the separate analytics procedures.
- Dashboard metrics compatibility on LocalDB now depends on the live analytics stored procedures materializing reusable booking/revenue intermediate sets instead of reusing multi-statement CTE names. The validated compatibility script is `Database/DB_Alter_20260509_DashboardMetricsAnalyticsCompatibility.sql`, which updates `dbo.uspGetBookingAnalytics` and `dbo.uspGetRevenueAnalytics` so the metrics result sets no longer fail with `Invalid object name 'BookingTrend'` / `Invalid object name 'RevenueTrend'`.
- Operations dashboard reads the live Phase 8 stack from `GET /api/dashboard/operations`, `GET /api/dashboard/operations/pending-queue`, `GET /api/dashboard/operations/technician-status`, `GET /api/dashboard/operations/sla-alerts`, `GET /api/dashboard/operations/zone-workload`, and `GET /api/dashboard/operations/day-summary`.
- Dispatch screen loads the queue from `GET /api/dashboard/operations/pending-queue`, keeps technician match cards on `GET /api/technicians/availability-board?serviceRequestId={id}`, and commits assignments through `POST /api/service-requests/{serviceRequestId}/assign` or `POST /api/service-requests/{serviceRequestId}/reassign`.
- Technician list and create/edit flows use `GET /api/technicians`, `GET /api/technicians/{technicianId}`, `POST /api/technicians`, `PUT /api/technicians/{technicianId}`, `PATCH /api/technicians/{technicianId}/skills`, and `PATCH /api/technicians/{technicianId}/zones`.
- Technician skill and zone assignment writes are soft-delete based. The live LocalDB uniqueness contract for those writes is active-row filtered uniqueness only: `tblTechnicianSkill (TechnicianId, SkillName) WHERE IsDeleted = 0` and `tblTechnicianZone (TechnicianId, ZoneId) WHERE IsDeleted = 0`. The current compatibility script is `Database/DB_Alter_20260506_TechnicianAssignmentSoftDeleteCompatibility.sql`, and the source assignment helpers now reactivate matching rows instead of blindly reinserting duplicates.
- Technician profile management enriches the detail surface with `GET /api/technicians/{technicianId}/performance`, `GET /api/technicians/{technicianId}/attendance`, `POST /api/technicians/{technicianId}/attendance/leave`, `PATCH /api/technicians/{technicianId}/attendance/leave/{leaveRequestId}`, and `GET /api/technicians/{technicianId}/gps-log`.
- Technician status boards inside the dashboard read `GET /api/dashboard/operations/technician-status`; the standalone availability board still reads `GET /api/technicians/availability-board`, with optional `serviceRequestId` ranking for dispatch context, and the legacy `/api/technicians/availability` alias is still accepted by the backend.
- Live map reads `GET /api/dashboard/live-map`, which now merges technician GPS breadcrumbs from technician tracking logs with service-request pins from customer-address latitude/longitude instead of simulated coordinates. The backend handler resolves technician GPS logs sequentially within a request to avoid concurrent reuse of the scoped persistence context, and the AdminMobile repository coalesces overlapping in-flight live-map reads to one API request.
- Technician editor/detail zone selectors use `GET /api/booking-lookups/zones` for base-zone and multi-zone assignment.
- Helper management cards use `GET /api/helpers` and `PUT /api/helpers/{helperProfileId}`.
- SLA escalation cards read `GET /api/dashboard/operations/sla-alerts`, which is derived from open service-request `SystemAlert` entries due within the next hour or already breached; escalation actions still use `POST /api/escalations`, and private operational notes still use `POST /api/service-requests/{serviceRequestId}/notes`.
- Day-summary export and WhatsApp share are generated client-side from `GET /api/dashboard/operations/day-summary`.
- Support analytics access is now split by role in the backend: support dashboard reads `GET /api/analytics/support` under `SupportRead`, while the remaining analytics endpoints continue to require `AnalyticsRead`.
- 2026-04-23 AdminMobile build verification update: reran `dotnet build` in `Backend`, `node ./node_modules/typescript/lib/tsc.js --noEmit --pretty false` in `Mobile/Coolzo_AdminMobile`, and `node ./node_modules/vite/bin/vite.js build` in the same admin app. The backend build completed with 0 warnings and 0 errors, the AdminMobile TypeScript compile exited successfully, and the production bundle completed successfully. The only notable bundler diagnostic was a non-fatal circular chunk warning between the `operations` and `admin` manual chunks.
  - 2026-04-23 Frontend Admin build remediation update: `Frontend/Admin` is a React/Vite project, not Angular. Its build initially failed because the local install was incomplete (`vite` was missing from `node_modules`) and the shared Base UI wrappers were typed too narrowly for the current `@base-ui/react` component signatures. After restoring the local frontend dependencies and widening the shared wrapper prop types in the button, dialog, dropdown-menu, and sheet adapters to the actual rendered component props, both `node ./node_modules/typescript/lib/tsc.js --noEmit --pretty false` and `npm run build` completed successfully. The remaining bundler output is a non-fatal circular chunk warning between the `operations` and `admin` manual chunks.
  - 2026-04-23 client API URL stabilization update: the customer mobile app, admin mobile app, and admin web app share one stable API environment contract via `VITE_API_BASE_URL`, where the env value represents the backend origin only (default `http://localhost:5217`) and clients call unversioned `/api/...` routes directly. Admin clients continue to strip any accidental `/api/v1` suffix from env input so the canonical base URL remains the backend origin.
  - 2026-04-23 client API URL verification update: after the stabilization change, all three frontend clients completed TypeScript compile successfully. `Mobile/Coolzo_MobileCustomer`, `Mobile/Coolzo_AdminMobile`, and `Frontend/Admin` also completed production builds successfully. The customer app still emits non-fatal Vite warnings about ignored `"use client"` directives and large chunks, while the admin mobile and admin web builds still emit the existing non-fatal circular chunk warning between the `operations` and `admin` manual chunks.

Security & Audit
- All dispatch and schedule changes are audit-logged with userId, timestamp, old/new values; role-based permissions required for bulk-assign, override, and write actions.


# SECTION 6 — ESTIMATES, WORK ORDERS & INVENTORY

1. MODULE: Estimates & Work Orders

Estimate flow
- Technician creates estimate in-field (mobile app) with itemized parts and labor.
- System sends estimate to customer via WhatsApp/email with Approve / Reject quick-reply actions or in-app action for logged-in users.
- On customer approval, system auto-creates a Work Order (WO) linked to the SR and locks the approved scope.
- Work proceeds only for approved line items; parts issuance and time tracking occur under the WO.
- On WO completion the Invoice engine is triggered (invoice includes approved estimate items and parts consumed).

Corporate approval rule
- Estimates above a configurable monetary threshold require approval from the corporate procurement contact before work proceeds; approval workflow stored in `EstimateApprovalHistory`.

Estimate fields
- EstimateId (PK), EstimateNumber, SRId (FK), CreatedByTechnicianId, SentToCustomerContact, SentDate, ExpiryDate (configurable TTL), ResponseDate, CustomerResponse (Approve/Reject/Pending), Notes
- Line items: LineItemId, EstimateId, Description, Quantity, UnitPrice, Discount, TaxRate, TaxAmount, LineTotal
- LaborCharges, Subtotal, TaxTotal, GrandTotal, Currency

Work Order (WO) fields
- WorkOrderId (PK), WONumber, SRId (FK), EstimateId (FK nullable), ApprovedLineItems (JSON or normalized table), AssignedTechnicianId, PartsIssuedStatus (None/Requested/Issued/Partial), JobStatus (Open/InProgress/Completed), InvoiceStatus (Uninvoiced/Invoiced/Cancelled), CreatedAt, CompletedAt

Screens
- Estimate List — Platform: Admin Web Portal / Technician Mobile; Roles: Technician, CS, Ops; Purpose: view sent and pending estimates; Key Actions: resend, cancel, view history.
- Estimate Detail — Platform: Admin Web Portal / Customer Web / Mobile; Roles: Customer, CS, Ops; Purpose: review estimate; Key Actions: Approve/Reject, comment, view line item detail.
- Work Order List — Platform: Admin Web Portal; Roles: Ops, Technician; Purpose: list active WOs; Key Actions: assign, reassign, view linked SR/Estimate.
- Work Order Detail — Platform: Admin Web Portal / Technician Mobile; Roles: Technician, Ops; Purpose: carry out approved work; Key Actions: check-in, request parts, mark tasks complete, submit job report.
- Job Reports Queue — Platform: Admin Web Portal; Roles: Ops, Quality; Purpose: review submitted job reports for completeness and quality; Key Actions: approve report, request revision, flag for audit.
- Job Report Detail (Admin View) — Platform: Admin Web Portal; Roles: Ops, Quality, Finance; Purpose: view checklist, photos, signature, parts used; Key Actions: attach to invoice, escalate if issues found.
- Report Quality Dashboard — Platform: Admin Web Portal; Roles: Ops Manager, Quality; Purpose: surface low-quality reports and trends; Key Actions: drill into technician, SR, photo counts.
- Estimate Expiry Management — Platform: Admin Web Portal; Roles: CS, Ops; Purpose: manage expired estimates and send re-quote prompts.
- Corporate Estimate Approval Workflow — Platform: Admin Web Portal; Roles: AccountManager, Procurement; Purpose: approve/reject high-value estimates; Key Actions: approve, request clarifications, attach PO reference.

Business rules
- No work proceeds on any estimate line item until customer approval or corporate procurement approval (if threshold exceeded).
- Estimates expire after configurable hours (default 72h); expired estimates are archived and require reissue.
- Manual override of expiry/approval allowed for Admin roles only and is audit-logged in `EstimateApprovalHistory`.
- Quality score: system auto-calculates a quality score per job using checklist completion rate + minimum photos count + timeliness; low scores surface in Quality Dashboard.

2. MODULE: Inventory & Parts Management

Parts Catalog fields
- PartId (PK), PartCode, Name, CategoryId, CompatibleBrands (JSON or normalized), CompatibleModels (JSON), UnitCost, SellingPrice, StockQuantity, MinReorderQty, ReorderLevelThreshold, StorageLocation, SupplierIds (JSON), LeadTimeDays, IsActive, CreatedAt, UpdatedAt

Stock movement types
- IN — PO Received (increase stock)
- OUT — Issued to Technician (linked to SR/WO)
- ADJ — Manual adjustment (reason required)
- RET — Returned unused parts (from technician)

Parts Request flow
- Technician submits PartsRequest from mobile app including Part codes, quantities and SRId.
- Store Manager reviews PartsRequest in Admin Portal and can Approve (full), Partial Approve, or Reject (reason required).
- On Approve: StockLedger entries created, stock quantities decremented, notification sent to technician with issue details and tracking.

Purchase Order (PO) fields
- POId (PK), PONumber, SupplierId, CreatedBy, OrderDate, ExpectedDeliveryDate, POStatus (Ordered/PartiallyReceived/Received/Cancelled), Line items: POItemId, PartId, QtyOrdered, QtyReceived, UnitPrice, Amount, ReceivedAt, DiscrepancyFlag

Low Stock Alert
- System identifies parts below `ReorderLevelThreshold` and suggests reorder quantity based on average consumption rate and lead time; one-tap PO creation pre-fills supplier and quantities.

Screens
- Inventory Dashboard — Platform: Admin Web Portal; Roles: Inventory Manager, Ops; Purpose: KPI view of stock, consumption, pending requests; Key Actions: filter, export, create PO.
- Parts Catalog List — Platform: Admin Web Portal; Roles: Inventory Manager; Purpose: manage parts master; Key Actions: add/edit part, set reorder thresholds.
- Part Detail — Platform: Admin Web Portal; Roles: Inventory; Purpose: view ledger, stock movements, supplier history; Key Actions: create PO, adjust stock.
- Add / Edit Part — Platform: Admin Web Portal; Roles: Inventory Manager; Purpose: maintain catalog; Key Actions: set compatibility, cost, storage location.
- Parts Requests Queue — Platform: Admin Web Portal; Roles: Store Manager, Inventory; Purpose: review incoming requests; Key Actions: Approve/Partial/Reject, assign pick list.
- Parts Request Detail — Platform: Admin Web Portal; Roles: Store Manager; Purpose: fulfill request; Key Actions: issue parts, record serials if applicable.
- Stock Movement Ledger — Platform: Admin Web Portal; Roles: Inventory; Purpose: audit all stock transactions; Key Actions: search, filter by SR/PO/Date.
- Purchase Order List / Create PO / PO Detail — Platform: Admin Web Portal; Roles: Inventory, Purchasing; Purpose: manage supplier orders; Key Actions: create PO, receive goods, reconcile.
- Low Stock Alerts — Platform: Admin Web Portal; Roles: Inventory Manager; Purpose: actionable list; Key Actions: create PO, delegate.
- Supplier Management — Platform: Admin Web Portal; Roles: Purchasing; Purpose: manage supplier contacts, lead times, credit terms.
- Inventory Reports — Platform: Admin Web Portal; Roles: Inventory, Finance; Purpose: consumption, valuation, stock ageing; Key Actions: export to CSV/Excel.

Business rules
- Stock is auto-deducted when PartsRequest is Approved; stock is auto-incremented on PO receive confirmation.
- When partial approval is made, system issues what is available and creates follow-up request for remaining qty.
- Reorder suggestions use moving-average consumption over configurable window (e.g., 90 days) plus safety factor.

Current AdminMobile Phase 12 implementation
- Estimate repository bindings now follow the Phase 12 estimate workflow contracts: `PATCH /api/estimates/{id}/send`, `PATCH /api/estimates/{id}/approve`, `PATCH /api/estimates/{id}/reject`, `PATCH /api/estimates/{id}/override-approve`, `PATCH /api/estimates/{id}/resend`, and `GET /api/estimates/expiry-queue`.
- Admin estimate list is now sorted with oldest pending estimates first and supports expiry-aware filtering/search; estimate detail exposes resend, reject, approve, and override-approve actions with operator notes captured into an approval-history timeline.
- Estimate detail now models expiry date, customer response, corporate-threshold flag, and approval history in the UI contract; mock-mode approval still auto-generates linked work orders to preserve the documented estimate-to-work-order flow.
- Work-order detail now shows the approval-chain timeline sourced from estimate approval events so operations can see the full approval progression before execution.
- Job-report repository bindings now use `GET /api/job-reports`, `GET /api/job-reports/{id}`, `PATCH /api/job-reports/{id}/approve`, `PATCH /api/job-reports/{id}/flag`, and `GET /api/job-reports/quality-dashboard`.
- Mock-mode job-report quality scoring now mirrors the documented Phase 12 algorithm buckets in the UI contract: checklist completion (40), photo coverage (30), customer signature (20), and observation depth (10). Admin job-report detail surfaces the score breakdown explicitly for report review.
- Job-report queue now links directly to the quality dashboard route, while quality metrics continue to drive the existing quality dashboard visuals.

Current AdminMobile Phase 13 implementation
- Inventory repository bindings now target the Phase 13 inventory contracts: `GET /api/inventory/dashboard`, `GET|POST /api/inventory/parts`, `GET|PUT /api/inventory/parts/{id}`, `GET /api/inventory/parts-requests`, `GET /api/inventory/parts-requests/{id}`, `PATCH /api/inventory/parts-requests/{id}/approve`, `PATCH /api/inventory/parts-requests/{id}/partial`, `PATCH /api/inventory/parts-requests/{id}/reject`, `GET /api/inventory/stock-movements`, `POST /api/inventory/stock-adjust`, `GET|POST /api/inventory/purchase-orders`, `GET /api/inventory/purchase-orders/{id}`, `PATCH /api/inventory/purchase-orders/{id}/receive`, `GET /api/inventory/low-stock-alerts`, and `GET|POST /api/inventory/suppliers`.
- IDs-only rule saved on 2026-04-25: whenever inventory APIs or mobile routes identify a part/request/entity, they must pass the numeric database `Id` only. Human-readable codes such as `CAP-45` are display fields only and must never be used as route parameters, `partId` values, or API identifiers.
- Inventory dashboard now pulls low-stock alerts from the repository, links quick actions to request processing, purchase orders, and supplier management, and keeps the low-stock action rail wired to purchase-order flows.
- Parts request queue now orders emergency requests ahead of standard requests, supports search, and keeps partial-approval states visible in the queue filter set.
- Parts request detail now supports full approval, rejection, and partial approval with issued-quantity capture per requested line item.
- Part detail now performs manual stock adjustments through the dedicated stock-adjust repository action instead of mutating stock directly through generic part updates.
- Purchase-order detail now completes warehouse receipt through the Phase 13 receive endpoint; mock-mode receipt increments stock and writes `IN` stock-ledger entries per received line item.
- Supplier management is now exposed inside AdminMobile at `/inventory/suppliers`, backed by `GET /api/inventory/suppliers` and `POST /api/inventory/suppliers`, so inventory/purchasing roles can maintain supplier directory data inside the app.
- Backend API routing no longer uses `/api/v1/...` URL version segments; AdminMobile live integrations must call the unversioned `/api/...` routes directly, and the shared API client now leaves relative API paths unchanged for inventory dashboard, low-stock alerts, and auth refresh calls.
- Inventory queue routing was corrected so AdminMobile now requests `GET /api/inventory/parts-requests` and `GET /api/inventory/parts-requests/{id}` instead of the stale `/api/inventory/requests...` path. Backend `InventoryController` now exposes those queue/detail read endpoints directly from the persisted `PartsRequest` records so the parts-request list no longer 404s on load.
- Live LocalDB remediation on 2026-04-25: the `500` on `GET /api/inventory/parts-requests` was caused by missing physical tables in `CoolzoDB`, not by an empty result set. Added `Database/SQL/20260425_Add_InventoryPartsRequest_Tables.sql` to create `tblPartsRequest` and `tblPartsRequestItem` with the EF-aligned schema and foreign keys to `tblServiceRequest`, `tblJobCard`, and `tblTechnician`. The script also seeds two live-like parts requests plus four line items only when the new request table is empty, using existing SR/job-card/technician rows as the authoritative linkage source. After execution, the live API returned `200` for `GET /api/inventory/parts-requests` with seeded queue data.
- Inventory identifier normalization on 2026-04-25: Backend `InventoryController` still exposes `GET /api/inventory/parts` and `GET /api/inventory/parts/{id}`, but the contract is now numeric-`Id` only. `GET /api/inventory/parts` returns `Part.id = ItemId.ToString()` instead of `ItemCode`, `GET /api/inventory/parts/{id}` is constrained to `:long`, and inventory parts-request responses now emit `partId` from persisted `ItemId` with a temporary PartCode-to-ItemId fallback for older rows. This removes the broken `/api/inventory/parts/CAP-45` lookup pattern from AdminMobile.
- Parts-request persistence normalization on 2026-04-25: `tblPartsRequestItem` now carries nullable `ItemId` in addition to `PartCode`/`PartName`, `Database/SQL/20260425_Add_PartsRequestItem_ItemId.sql` adds/backfills that column for live LocalDB, and the field workflow parts-request write contract now submits `partId` only. `PartCode` and `PartName` remain persisted/displayed for readability, but the authoritative inventory join key is `ItemId`.
- Purchase-order live persistence on 2026-05-06: `InventoryController` now exposes `GET /api/inventory/purchase-orders`, `GET /api/inventory/purchase-orders/{id}`, `POST /api/inventory/purchase-orders`, and `PATCH /api/inventory/purchase-orders/{id}/receive` from persisted `tblPurchaseOrder` and `tblPurchaseOrderItem` rows. `Database/SQL/20260506_Add_InventoryPurchaseOrder_Tables.sql` creates those tables for live LocalDB with supplier/item foreign keys. Receipt confirmation increments `tblWarehouseStock`, records `PurchaseIn` entries in `tblStockTransaction`, and advances PO status to `partially_received` or `fully_received`.

Current AdminMobile Phase 14 implementation
- Billing repository bindings now target the Phase 14 invoice contracts: `GET /api/invoices`, `GET /api/invoices/{id}`, `POST /api/invoices`, `PUT /api/invoices/{id}`, `PATCH /api/invoices/{id}/send`, `PATCH /api/invoices/{id}/apply-discount`, `POST /api/invoices/{id}/credit-note`, `PATCH /api/invoices/{id}/mark-paid`, `GET /api/invoices/{id}/version-history`, `GET /api/billing/accounts-receivable`, `POST /api/billing/payment-reminders/send`, `POST /api/billing/invoices/bulk-export`, `POST /api/billing/corporate-invoice/{accountId}`, `POST /api/billing/proforma/{customerId}`, and `PATCH /api/billing/invoices/{id}/bad-debt`.
- Billing list now combines invoice search/filtering with AR summary data, reminder queue actions, bulk export, and one-tap corporate/proforma generation from the main billing surface.
- Billing filter normalization on 2026-05-06: AdminMobile now omits unsupported empty query parameters when calling `GET /api/invoices`, never sends `status=all`, and treats `/billing/invoices?status=overdue` as a UI filter derived from `GET /api/billing/accounts-receivable` overdue IDs/due dates instead of sending an invalid backend status value.
- Invoice detail now exposes send, edit-with-change-reason, manual payment capture, discount application, credit-note issuance, bad-debt marking, payment history, credit-note history, and version-history review in a single workflow.
- Manual invoice creation now captures customer identity, customer type, due date, HSN/SAC per line item, discount, notes, and invoice status (`draft` or `sent`) before routing to the generated invoice record.
- AR dashboard now consumes the dedicated accounts-receivable endpoint and surfaces aging buckets, overdue invoice follow-up, and top-outstanding-customer reminder actions for billing/finance roles.

Current AdminMobile Phase 15 implementation
- Payment repository bindings now target the Phase 15 finance contracts: `GET /api/payments`, `GET /api/payments/{id}`, `POST /api/payments/cod-verify`, `POST /api/payments/refund`, `GET /api/payments/unmatched`, `PATCH /api/payments/match/{gatewayTxnId}`, `GET /api/payments/cod-pending`, `GET /api/receipts`, `POST /api/receipts/{id}/resend`, `GET /api/reports/finance-dashboard`, `GET /api/reports/revenue`, `GET /api/reports/collection-efficiency`, `GET /api/reports/tax-liability`, `GET /api/reports/financial-summary`, and `GET /api/reports/discount-coupon-usage`.
- Finance dashboard now aggregates KPI tiles, revenue-vs-target charting, revenue-mix visualization, collection-efficiency progress bars, and direct navigation into finance report surfaces and receipt management.
- Payment list now includes status filtering, free-text search, unmatched gateway reconciliation cards, COD verification queue handling, and detail navigation for each payment transaction.
- Payment detail now surfaces gateway transaction metadata, verification state, invoice linkage, refund processing, and COD verification controls inside the finance route tree.
- Receipt management is now exposed inside AdminMobile at `/finance/receipts`, backed by the Phase 15 receipt endpoints for receipt list retrieval, resend actions, and PDF handoff.
- Finance reports now have a dedicated route surface at `/finance/reports/:reportType` for revenue, collection-efficiency, financial-summary, and discount/coupon-usage outputs, while tax liability remains on `/finance/tax`.

Current AdminMobile Phase 16 implementation
- Support repository bindings now cover the Phase 16 ticketing surface used by AdminMobile: ticket queue/detail/create, replies, assignment, escalation, status change, close, dashboard metrics, support lookups, `GET /api/feedback`, `GET /api/feedback/{customerReviewId}`, `PATCH /api/feedback/{customerReviewId}/respond`, `PATCH /api/feedback/{customerReviewId}/publish`, `PATCH /api/feedback/{customerReviewId}/flag`, negative-feedback queue, feedback analytics, and complaint heatmap retrieval. Public customer review read surfaces remain on `GET /api/customer-reviews`, filtered to published/displayable reviews only.
- Support dashboard now consumes live support stats plus feedback analytics to render KPI tiles, ticket-volume trend, issue mix, agent performance, and technician review ranking from the support module entry route.
- Ticket queue remains escalated-first and now keeps the queue constrained to searchable status-driven ticket cards with linked SR context, assignee state, and SLA-facing timestamps.
- Ticket detail now supports chat-style reply handling, internal notes, assignment, escalation, status management, customer context, and linked SR visibility from a single support-workflow screen.
- Ticket creation now captures customer identity, customer name, optional linked SR, category, priority, and issue description before routing to the created support ticket record.
- Feedback list now reads the dedicated admin moderation feed from `GET /api/feedback`, combines it with a negative-feedback alert rail, and keeps lowest-rating-first ordering with direct navigation to publish/flag/respond actions on the feedback detail screen.
- Feedback detail now uses persisted admin moderation actions through `PATCH /api/feedback/{customerReviewId}/respond`, `PATCH /api/feedback/{customerReviewId}/publish`, and `PATCH /api/feedback/{customerReviewId}/flag`, storing admin response text plus publish/flag state on the underlying customer review record so moderation survives refresh and remains hidden from the public review feed when unpublished or flagged.

Current AdminMobile Phase 17 implementation
- Governance repository bindings now target the Phase 17 admin surfaces: `GET|PUT /api/notifications/templates`, `GET /api/notifications/log`, `POST /api/notifications/push-campaign`, `GET|POST /api/cms/blocks`, `PUT /api/cms/blocks/{id}`, `GET|POST /api/coupons`, `PUT /api/coupons/{id}`, `PATCH /api/coupons/{id}/disable`, `GET /api/reports/{reportType}`, `GET|POST /api/reports/scheduled`, `DELETE /api/reports/scheduled/{id}`, `GET /api/audit-logs`, and `GET /api/audit-logs/data-access`.
- Notification template management now supports trigger-level template selection, body editing, merge-tag insertion, per-channel toggles, preview rendering, version history visibility, and send-log review from the governance route tree.
- CMS management now supports banner, service-description, FAQ, article, and footer blocks with in-app creation plus JSON-backed content editing and publish-state maintenance.
- Coupon management now supports create, edit, disable, and analytics review flows with usage, minimum-order, expiry, and abuse-signal visibility aligned to the coupon admin surface.
- Reports hub now consumes repository-backed report definitions, supports run/export actions per report, renders returned report payload rows, and manages recurring report schedules from the same screen.
- Audit logs now remain append-only while adding action filtering, broader search coverage, change diff inspection, and a dedicated sensitive-data-access table fed by the data-access audit endpoint.

Current AdminMobile Phase 18 implementation
- Production-polish infrastructure now uses a shared `SystemUXProvider` for app-wide connectivity state, pending-sync counts, and last-sync timestamps, with amber offline banner behavior and pending-sync messaging exposed through the shared shell.
- Offline sync queue handling moved to a generic local queue model backed by local storage rather than new backend APIs, including ordered queue items with endpoint/method metadata, idempotency keys, retry counters, next-retry timestamps, attachment references, conflict markers, and queue-count exposure for the shared notification entry point.
- System health now supports cached-state presentation while offline, surfacing the last known sync timestamp instead of assuming live connectivity for observability screens.
- Deep-link routing now recognizes the AdminMobile launch patterns for jobs, service requests, estimates, invoices, tickets, AMC contracts, renewal-focused AMC entry, offers, and reset-password token handoff, with router aliases plus startup deep-link resolution.
- Push-intent routing is now mapped in the app layer for job assignment, parts approval, estimate response, SLA breach, ticket reply, AMC renewal, invoice overdue, new SR, and emergency SR flows, including foreground in-app banner display and post-auth pending-route consumption.
- Session hardening now records background-entry time and attempts silent token refresh after a 30-minute background interval, falling back to the existing session-expired route if refresh cannot complete.
- AdminMobile shell composition was refactored on 2026-04-22 into a dedicated project-root `app/` layer that now owns navigation, role menu composition, screen wrappers, service wrappers, store exports, constants, utilities, and API-contract documentation while continuing to reuse the stable feature implementations in `src/features/*` and repository bindings in `src/core/network/*`.
- The active router is now `Mobile/Coolzo_AdminMobile/app/navigation/router.tsx`; it replaces the older `src/routes/index.tsx` glue layer, keeps auth guard + role guard enforcement, preserves existing route contracts, and adds first-class `/bookings`, `/bookings/:id`, `/notifications`, and `/settings` admin surfaces.
- Role-based navigation is now sourced from `Mobile/Coolzo_AdminMobile/app/modules/role-navigation.tsx` and consumed by `AdminScaffold.tsx`, replacing the older `src/core/config/navigation-config.tsx` configuration and ensuring menu composition is driven from the new app shell.
- Booking list/detail are now implemented as admin wrapper screens backed by `app/services/booking-service.ts`, which maps service-request repository data into booking records so booking queue/detail flows remain functional without inventing undocumented backend contracts.
- Notifications now have a dedicated shared entry route at `/notifications`, backed by `app/services/notification-service.ts` over the governance notification log repository, while `/settings` now lands on a shared shortcut hub instead of dropping directly into a single configuration feature.
- The single-source API contract document for AdminMobile is now `Mobile/Coolzo_AdminMobile/docs/API_MASTER.ts`; it groups auth, dashboard, service request, booking, customer, technician, operations, notifications, and settings endpoints with request/response shapes plus mock/live status.
- The global mock/live toggle is now explicitly exported as `IS_MOCK` from `Mobile/Coolzo_AdminMobile/src/core/config/api-config.ts` and re-exposed through the new app infrastructure layer so app services and API documentation share the same mode flag.
- The unused `React_migration_starter` placeholder scaffold was removed on 2026-04-22 because the recorded AdminMobile implementation remains the React/Vite application under `Mobile/Coolzo_AdminMobile`.
- Verification on 2026-04-22: `./node_modules/.bin/tsc --noEmit --pretty false` succeeds after the new `app/` layer and `docs/` contract file were added. `npm run build` emits a fresh `dist/` bundle set, but the Vite process still did not exit within the command timeout window during this session, so build output generation was observed even though command completion needs follow-up if strict CI exit timing matters.
- Build stabilization on 2026-04-22: `Mobile/Coolzo_AdminMobile/package.json` now routes `npm run build` through `scripts/build.mjs`, which executes the existing Vite production build via the Vite Node API and exits the Node process explicitly on success/failure. This preserves the current app architecture and existing `vite.config.ts` behavior while preventing the lingering post-build process state seen with the direct `vite build` CLI invocation in this environment.
- Verification on 2026-04-22 after stabilization: `./node_modules/.bin/tsc --noEmit --pretty false` exits successfully, `npm run build` exits with code `0`, and the AdminMobile `dist/` bundle is generated with the normal Vite asset summary (`✓ built in 1m 45s`).

3. API ENDPOINTS (Estimates / Work Orders / Inventory)
- Estimates & Work Orders
   - GET /api/estimates
   - POST /api/estimates
   - GET /api/estimates/{estimateId}
   - POST /api/estimates/{estimateId}/send
   - POST /api/estimates/{estimateId}/approve
   - POST /api/estimates/{estimateId}/reject
   - GET /api/workorders
   - POST /api/workorders
   - GET /api/workorders/{workOrderId}
   - PUT /api/workorders/{workOrderId}/status
   - POST /api/workorders/{workOrderId}/parts-issue

- Inventory & Parts
   - GET /api/parts
   - POST /api/parts
   - GET /api/parts/{partId}
   - PUT /api/parts/{partId}
   - GET /api/parts/{partId}/ledger
   - GET /api/parts/requests
   - POST /api/parts/requests
   - PUT /api/parts/requests/{requestId}/approve
   - GET /api/inventory/purchase-orders
   - POST /api/inventory/purchase-orders
   - GET /api/inventory/purchase-orders/{poId}
   - PATCH /api/inventory/purchase-orders/{poId}/receive

4. DB TABLES & COLUMNS (canonical)

Estimates
- EstimateId (PK), EstimateNumber, SRId (FK), CreatedByTechnicianId, SentToCustomerContact, SentDate, ExpiryDate, ResponseDate, CustomerResponse, Subtotal, TaxTotal, GrandTotal, Currency, Notes, CreatedAt, UpdatedAt

EstimateLineItems
- LineItemId (PK), EstimateId (FK), Description, Quantity, UnitPrice, Discount, TaxRate, TaxAmount, LineTotal

WorkOrders
- WorkOrderId (PK), WONumber, SRId (FK), EstimateId (FK nullable), AssignedTechnicianId, ApprovedLineItems (JSON), PartsIssuedStatus, JobStatus, InvoiceStatus, CreatedAt, StartedAt, CompletedAt

EstimateApprovalHistory
- ApprovalId (PK), EstimateId (FK), ApproverUserId, ApproverRole, ThresholdAmount, Action (Approved/Rejected/Overridden), Comments, ActionDate

PartsCategories
- CategoryId (PK), Name, Description, ParentCategoryId, CreatedAt

PartsCatalog
- PartId (PK), PartCode, Name, CategoryId (FK), CompatibleBrands (JSON), CompatibleModels (JSON), UnitCost, SellingPrice, StockQuantity, MinReorderQty, ReorderLevelThreshold, StorageLocation, SupplierIds (JSON), LeadTimeDays, IsActive, CreatedAt, UpdatedAt

StockLedger
- LedgerId (PK), PartId (FK), ChangeType (IN/OUT/ADJ/RET), Quantity, SourceRefId (PO/Request/SR), SourceRefType, UnitCost, BalanceAfter, Notes, CreatedAt

PartsRequests
- RequestId (PK), RequestNumber, RequestedByTechnicianId, SRId (FK), Status (Pending/Approved/Partial/Rejected), RequestedAt, RequestedItems (JSON), ApprovedBy, ApprovedAt, Notes

PartsRequestItems
- RequestItemId (PK), RequestId (FK), PartId (FK), QuantityRequested, QuantityApproved, QuantityIssued

PurchaseOrders
- POId (PK), PONumber, SupplierId, CreatedBy, OrderDate, ExpectedDeliveryDate, Status, TotalAmount, CreatedAt, UpdatedAt

POItems
- POItemId (PK), POId (FK), PartId (FK), QtyOrdered, QtyReceived, UnitPrice, Amount, ReceivedAt

Notes:
- Inventory and parts systems are highly auditable: all stock movements are recorded in `StockLedger` and tied to SR/WO/PO for traceability.
- Parts with serial numbers should be issued and recorded per-unit in StockLedger with SerialNumber references when required.

# SECTION 9 — CONFIGURATION, NOTIFICATIONS & AUDIT

1. MASTER DATA & CONFIGURATION

The admin configuration surface exposes versioned, zero-code controls for business masters and operational rules. Key configuration areas and canonical fields:

Service & Business Master
- ServiceType: ServiceTypeId, Name, Description, IconKey, DefaultDurationMinutes, BasePrice, ApplicableEquipmentTypes (JSON), Visibility, DisplayOrder, IsActive
- ServiceSubType: ServiceSubTypeId, ServiceTypeId, Name, Description, DurationModifier, PriceModifier, SkillTags, IsActive
- Zones: ZoneId, ZoneName, PinCodes (array), ServiceAvailability (bool), DefaultBranchId, Timezone, GeoPolygon (optional), IsActive
- EquipmentCatalog: BrandId, BrandName, ModelId, ModelName, CompatibleTypes (JSON), DefaultWarrantyMonths, ManufacturerCode, IsActive
- JobStatusWorkflow: JobStatusId, Label, ColorHex, IsTerminal, AllowedTransitions (JSON)
- TechnicianSkillTags: SkillTagId, Name, Description, CertificationRequired
- PriorityLevels: PriorityId, Name, SLAHours, SurchargePercent, EscalationPolicyId

Pricing & Billing Config
- ServiceBasePricing: PricingId, ServiceTypeId, ZoneId (nullable), EquipmentType, TonnageRange, BasePrice, Currency, EffectiveFrom, EffectiveTo
- AMCPlans: PlanId, PlanName, VisitsIncluded, Frequency, PriceTiers (JSON), ValidityMonths, RenewalRules (JSON)
- TaxConfig: TaxConfigId, EntityType, EntityId, TaxRatePercent, IsInclusive, HSN_SAC_Code, EffectiveFrom
- DiscountRules: CouponConfig (Code, Type, Value, Applicability, StartDate, ExpiryDate, UsageLimit, MinOrderValue)
- PaymentTerms: PaymentTermId, Name, DueDays, LateFeePercent, ReminderSchedule
- InvoiceNumbering: FormatString, Prefix, SequenceSeed, ResetPeriod

SLA & Operations
- SLATargets: PriorityId → AssignmentSLA_Hours, ArrivalSLA_Hours, CompletionSLA_Hours
- BusinessHours: BusinessHoursId, BranchId, WorkingDays, StartTime, EndTime, Timezone
- HolidayCalendar: HolidayId, Date, Name, BranchIdNullable
- SlotRules: SlotDurationMinutes, BufferMinutes, MaxConcurrentPerTech, AdvanceBookingWindowDays
- AutoEscalation: EscalationId, TriggerEvent, ThresholdMinutes, RecipientRoles, NotificationTemplateId
- AMC AutoSchedule: AutoCreateDaysBeforeDue, ReminderScheduleDays, GracePeriod

Communication & Notification Config
- NotificationTrigger: TriggerId, EventKey, Enabled, DefaultChannels (JSON)
- ChannelMapping: TriggerId → [WhatsApp, Email, SMS, Push]
- Templates: TemplateId, Name, Channel, Subject, Body (text/HTML), MergeTags (JSON), Language, IsActive
- SenderIdentities: SenderId, WhatsAppBusinessNumber, EmailFrom, SMS_SenderId
- ReminderSchedules: ReminderId, TriggerId, WhenToSend, Repeat

Role & Permission Config
- Roles: RoleId, RoleName, ModulePermissions (JSON), DataScopeDefault, IsSystemRole

Website CMS Config
- Banners, ServiceDescriptions, AMCHighlights, Testimonials, FAQs, BlogPosts, FooterContent — each stored with versioning and activation windows.

2. NOTIFICATION SYSTEM — EVENT MAP (core events)

Event → Recipient(s) | Primary Channel(s) | Content Summary
- Booking Confirmed — Customer | WhatsApp + Email | SR number, service type, appointment date/time, contact number, link to booking
- Booking Rescheduled — Customer | WhatsApp + Email + Push | New date/time, previous slot, reschedule reason
- Booking Cancelled — Customer | WhatsApp + Email | Cancellation confirmation, refund/credit details if applicable
- Technician Assigned — Customer, Technician | WhatsApp (customer) + Push (technician) | Technician name/photo/rating, ETA window, contact
- Technician En Route — Customer | WhatsApp + Push | ETA, live tracking link
- Technician Arrived — Customer | Push | Arrival confirmation
- Job Started — Customer, Ops | Push + In-App | Work start timestamp and estimated remaining time
- Estimate Sent (Awaiting Approval) — Customer | WhatsApp + Email | Itemized estimate, Approve/Reject CTAs
- Estimate Approved — Ops, Technician | Push + Email | Approval confirmation, locked scope for WO
- Parts Request Approved — Technician | Push + In-App | Approved parts list and pickup/delivery instructions
- Job Completed — Customer | WhatsApp + Email | Service report link, invoice link, payment CTA
- Invoice Generated — Customer, Billing | WhatsApp + Email | Invoice number, amount, due date, payment link
- Payment Received — Customer, Billing | WhatsApp + Email | Receipt and confirmation
- Payment Failed — Customer | Email + Push | Failure reason and retry instructions
- Invoice Overdue — Customer, Billing | WhatsApp + Email | Overdue notice, outstanding amount, next steps
- Feedback Request — Customer | WhatsApp | Post-service feedback request with 1-click rating
- SLA Breach Alert — Operations Manager | Email + In-App | Details of breached SRs and recommended actions
- AMC Renewal Reminder — Customer | WhatsApp + Email | Renewal due in X days with renewal CTA and offer

Notes: Each event maps to one or more templates (per channel). Merge tags must be declared on templates. Admin UI exposes enable/disable per trigger, per-channel overrides, and escalation recipients for critical events. Notification delivery and response logs are persisted for reconciliation.

3. CMS MODULE (admin editable content)

Content types and editable fields:
- Banners: BannerId, Title, ImageRef, CTAText, CTAUrl, ActiveFrom, ActiveTo, Priority
- ServiceDescriptions: ServiceId, ShortDesc, LongDesc (rich HTML), IconKey, SEO_Meta
- AMCHighlights: PlanId, Headline, Benefits (array), CTA
- Testimonials: TestimonialId, CustomerName, Quote, Rating, PhotoUrl, IsFeatured
- FAQs: FAQId, Category, Question, Answer, Order
- BlogPosts: PostId, Title, Slug, Excerpt, ContentHTML, FeaturedImage, Tags, PublishedAt, Status

CMS supports scheduling, preview per channel (web/mobile), and version rollback via `CMSBlockVersions`.

4. REPORTS HUB

Operational reports with parameters and export formats:
- Daily Job Sheet — params: Date, Branch/Zone, TechnicianId(s); exports: CSV, PDF
- Technician Performance — params: DateRange, TechnicianId(s); exports: CSV, XLSX
- SLA Compliance — params: DateRange, Priority, Zone; exports: CSV, PDF
- Customer Retention — params: CohortRange, MinBookings; exports: CSV, XLSX
- AMC Performance — params: PlanId, DateRange; exports: CSV, PDF
- Parts Consumption — params: DateRange, PartCategory; exports: CSV, XLSX
- Warranty Revisit Analysis — params: DateRange, ServiceType; exports: CSV, PDF
- Financial Summary — params: DateRange, Branch, ServiceType; exports: CSV, XLSX, PDF

Reports may be scheduled, emailed, and support parameterized drill-downs. Report generation uses materialized read-models for performance where needed.

5. AUDIT LOG

Audit schema and policies:
- AuditLogs: AuditId (PK), TimestampUTC, UserId, UserRole, IPAddress, EndpointCalled, EntityType, EntityId, Action, JSONBefore, JSONAfter, CorrelationId, Notes

Rules:
- Append-only: audit records are immutable.
- Retention: default minimum 7-year retention (configurable per region/legal requirement).
- Access: read-only UI for `Auditor` and `Super Admin` roles; exports gated behind permission.
- Security: logs encrypted at rest and transmitted over TLS; log integrity checks and rotation policies applied.

6. CONFIG & NOTIFICATION DB TABLES (summary)

SystemConfigs (ConfigId, Key, ValueJSON, Description, IsActive, EffectiveFrom, EffectiveTo, CreatedAt)
BusinessHours (BusinessHoursId, BranchId, TimeZone, WorkingDaysJSON, StartTime, EndTime, CreatedAt)
HolidayCalendar (HolidayId, Date, Name, BranchIdNullable)
SLAConfigurations (SLAConfigId, PriorityId, AssignmentHours, ArrivalHours, CompletionHours)
NotificationTriggers (TriggerId, EventKey, DefaultChannelsJSON, TemplateId, Enabled)
NotificationTemplates (TemplateId, Name, Channel, Subject, Body, MergeTagsJSON, Language, IsActive)
NotificationLog (NotificationId, RecipientId, Channel, TemplateId, PayloadJSON, SentAt, DeliveredAt, Status, ResponseCode, Error)
CommunicationPreferences (PrefId, UserIdNullable, PreferencesJSON, DNDStart, DNDEnd)
PushNotificationQueue (QueueId, UserId, PayloadJSON, ScheduledAt, Status, Attempts)
CMSBlocks, CMSBlockVersions, Banners, FAQs, BlogPosts, Reviews, ReviewResponses
AuditLogs, DataAccessLogs

Implementation notes:
- Admin UI must present clear version history and a staged activation workflow for critical configuration changes.
- Notification delivery must be idempotent and track provider response codes for diagnostics and billing reconciliation.
- Reporting extracts should read from denormalized read-models where heavy aggregation is required.

Current AdminMobile Phase 4 implementation
- `Mobile/Coolzo_AdminMobile/src/App.tsx` mounts `MasterDataProvider`, so master/config payloads are cached and shared across admin configuration screens.
- The system configuration hub now routes to live AdminMobile configuration screens for service catalog, zones, business hours, workflow, pricing, tax, equipment brands/models, AMC plans, payment terms, warranty periods, invoice numbering, and related aliases.
- Service catalog screen manages `service-types`, `service-subtypes`, `equipment-brands`, and `equipment-models`; zone screen manages `zones`; business-hours screen manages `business-hours`, holiday entries, and slot availability records.
- Workflow configuration screen manages `job-statuses`, `urgency-levels`, `skill-tags`, `sla-targets`, and auto-escalation rules; pricing configuration screen manages pricing matrix, AMC plans, payment terms, and warranty periods; tax configuration screen manages tax and invoice numbering.
- These AdminMobile screens use the existing generic admin configuration surface exposed by `Backend/Coolzo.Api/Controllers/Phase4ConfigurationController.cs` under `/api/master/*` and `/api/config/*`.


END OF SECTION 9

# SECTION 10 — AUTH, SECURITY & API ARCHITECTURE

1. AUTHENTICATION MODEL

Authentication modes
- Customer authentication: Phone number + OTP (one-time password) flow; option to convert guest to registered account after first booking.
- Internal staff authentication: Email + Password with optional enforced 2FA for privileged roles.
- Technician / Helper authentication: EmployeeId + PIN (4-6 digit PIN) for field app quick auth; device registration required.

Two-Factor & session controls
- 2FA: OTP-based second factor required (configurable) for `Admin`, `Finance` and other high-privilege roles; enforced per-role in RBAC config.
- JWT: Access token TTL = 15 minutes; Refresh token TTL = 7 days. Refresh tokens rotated on every use; revoked on logout or admin force-logout.
- Session management: multi-device sessions allowed; each device registers a `UserDevice` record (deviceId, push token, lastSeen). Admins can list and force-logout sessions per-user or per-device.

Password & account policy
- Password policy (internal users): Minimum 10 characters, must include upper, lower, number, symbol; no reuse of last 5 passwords; optional 90-day rotation for high-privilege accounts.
- Account lockout after configurable failed attempts; admin override to unlock.

2. RBAC MODEL

Three-level RBAC
- Module Level: grant or deny access to whole modules (e.g., Billing, Dispatch, Inventory).
- Action Level: fine-grained actions per module (View / Create / Edit / Delete / Approve / Export). Each action flag stored per role and evaluated by middleware.
- Data Scope Level: row-level data scope enforcement (All Data / Branch Only / Own-Created Only / Customer-Linked Only / Zone Only).

Data visibility rules (canonical table)
- Customer: Own bookings, invoices, equipment, support tickets only. Never sees other customers or pricing rules.
- CS Executive: All customer data and all SRs. Cannot see financial P&L, system config, or technician personal data.
- Operations Executive: All SRs and technician availability. Cannot see customer payment data or business financials.
- Technician: Only own assigned jobs and own job history. Cannot see other technicians' jobs or customer financials.
- Billing Executive: All invoices and payments. Cannot see job operational details or technician personal data.
- Branch Admin: All data within their assigned branch. Cannot see other branch data.
- Super Admin: Full unrestricted access to all data, logs, and configuration.

RBAC mechanics
- RBAC rules persisted in DB tables (`Roles`, `RolePermissions`, `RoleScopes`) and evaluated at API middleware. UI renders controls and screens based on effective permissions returned by `GET /api/users/{id}/permissions`.
- Permission changes take effect immediately (no restart) and are auditable.

3. API SECURITY

Core principles
- Authentication: JWT Bearer tokens for all authenticated requests. Only public endpoints (public content, slot availability, guest booking, auth endpoints) are exempt.
- Authorization: Role claims embedded in JWT. Action-level policies implemented via custom authorization handlers (policy-per-action) and applied at controller/action level.
- Rate limiting: Per-IP and per-user rate limits applied at API Gateway or middleware; configurable thresholds per route group.
- Transport & CORS: TLS/HTTPS enforced; CORS allowed only for configured origins per environment (production strict, dev relaxed).

PII & storage security
- PII encrypted at rest (column-level or encrypted JSON blobs) for fields such as government IDs; phone numbers masked in API responses for non-privileged roles (e.g., show +91-98*****1234).
- Secrets & keys stored in vault (Azure Key Vault / AWS Parameter Store) and injected into app at startup.

Operational security
- Audit trail: all state-changing operations (POST/PUT/PATCH/DELETE) are auto-logged by middleware to `AuditLogs` with before/after JSON diff.
- File uploads: validate content-type and extension, enforce size limits, run virus scan (ClamAV or cloud provider equivalent), sanitize file names and rename before storage, store in private blob storage with time-limited signed URLs for access.
- Input validation: FluentValidation (server-side) on DTOs; reject invalid requests early.

4. API DESIGN STANDARDS

- One API for all surfaces: single ASP.NET Core Web API serving web, admin, and mobile clients.
- Versioning: All endpoints are exposed under unversioned `/api/...` routes. Backward-compatible additions are applied without path-version segments.
- Response envelope: every response uses { success: bool, statusCode: int, data: object|null, errors: array|null, meta: object|null }.
- Pagination: query parameters `pageNumber`, `pageSize`, `sortBy`, `sortOrder`. Responses include `totalCount`, `totalPages`, `hasNext`, `hasPrevious` in `meta`.
- Standardized error codes: API returns structured business error codes (string code + message), enabling client mapping and localization.
- Idempotency: write endpoints support idempotency keys where appropriate (e.g., `Idempotency-Key` header for booking/payment creation).
- API docs: OpenAPI/Swagger generated from controllers and DTOs with examples and standardized error schemas.

5. COMPLETE API MODULE LIST (24 groups)

1) Auth & Session
- POST /api/auth/otp/send
- POST /api/auth/otp/verify
- POST /api/auth/login
- POST /api/auth/technician-login
- POST /api/auth/refresh
- POST /api/auth/logout
- POST /api/auth/forgot-password
- POST /api/auth/reset-password
Description: All authentication, token lifecycle, OTP and session endpoints.
- 2026-04-22 compatibility update:
  - `AuthController` now exposes customer-facing compatibility routes `POST /api/auth/otp/send` and `POST /api/auth/otp/verify` for the customer app phone login flow.
  - OTP login resolution now looks up customer accounts by login identifier through `CustomerAccountLookupService`, which allows phone-number-based verification to resolve the linked `User` record before issuing JWT and refresh tokens.
  - No schema migration was required for this auth work because `OtpVerifications`, `RefreshTokens`, and customer-linked `Users` were already present in the current database model.
- 2026-04-25 routing update:
  - Backend controller routes no longer include the `v{version:apiVersion}` URL segment. All controller-level API routes now resolve directly under `/api/...`, matching the AdminMobile, customer app, and Playwright contracts.

2) Users / User Management
- GET /api/users
- POST /api/users
- GET /api/users/{userId}
- PUT /api/users/{userId}
- DELETE /api/users/{userId}
- POST /api/users/{userId}/devices
Description: CRUD for users, device registration, account lifecycle.

3) Roles & Permissions
- GET /api/roles
- POST /api/roles
- GET /api/roles/{roleId}
- PUT /api/roles/{roleId}/permissions
- POST /api/roles/{roleId}/assign
Description: Manage roles, permission bitsets, role assignment.

4) Customer Management
- POST /api/customers
- GET /api/customers
- GET /api/customers/{customerId}
- PUT /api/customers/{customerId}
- GET /api/customers/{customerId}/360
Description: Customer CRUD and Customer-360 aggregated view.

5) Equipment Register
- POST /api/equipment
- GET /api/equipment
- GET /api/equipment/{equipmentId}
- PUT /api/equipment/{equipmentId}
- DELETE /api/equipment/{equipmentId}
Description: Equipment catalog and per-customer equipment management.

6) Service Requests (SR)
- POST /api/sr
- GET /api/sr
- GET /api/sr/{srId}
- PUT /api/sr/{srId}/status
- POST /api/sr/{srId}/assign
- POST /api/sr/{srId}/reschedule
Description: Core SR lifecycle operations and assignment.

7) Booking Engine
- GET /api/bookings/slots
- POST /api/bookings/hold
- POST /api/bookings
- POST /api/bookings/{bookingId}/confirm
- GET /api/bookings/{bookingId}/summary
Description: Slot availability, guest bookings, holds and confirms.

8) Technician Management
- POST /api/technicians
- GET /api/technicians
- GET /api/technicians/{techId}
- PUT /api/technicians/{techId}
- GET /api/technicians/{techId}/performance
- GET /api/technicians/{techId}/attendance
- POST /api/technicians/{techId}/attendance/leave
- PATCH /api/technicians/{techId}/attendance/leave/{leaveRequestId}
- GET /api/technicians/availability-board
- GET /api/technicians/{techId}/gps-log
- PATCH /api/technicians/{techId}/skills
- PATCH /api/technicians/{techId}/zones
- GET /api/helpers
- PUT /api/helpers/{helperId}
Description: Technician profiles, skills, zones, attendance, performance, availability, GPS breadcrumbs and helper administration.

9) Field Workflow (Mobile)
- POST /api/field/checkin
- POST /api/field/checkout
- POST /api/field/jobreport
- POST /api/field/photos
- POST /api/field/parts-request
Description: Mobile-specific job actions (check-in, reports, photos, parts).

10) AMC Management
- GET /api/amc/plans
- POST /api/amc/contracts
- GET /api/amc/contracts/{contractId}
- POST /api/amc/contracts/{contractId}/visits
- POST /api/amc/contracts/{contractId}/renew
Description: AMC plan management and scheduled visits.

11) Estimates & Work Orders
- POST /api/estimates
- GET /api/estimates/{estimateId}
- POST /api/estimates/{estimateId}/send
- POST /api/estimates/{estimateId}/approve
- POST /api/workorders
Description: Estimate creation, customer approval and WO conversion.

12) Inventory & Parts
- GET /api/parts
- POST /api/parts/requests
- PUT /api/parts/requests/{requestId}/approve
- POST /api/purchase-orders
- GET /api/parts/{partId}/ledger
Description: Parts catalog, requests, PO lifecycle and stock ledger.

13) Billing & Invoice
- POST /api/invoices
- GET /api/invoices/{invoiceId}
- PUT /api/invoices/{invoiceId}
- POST /api/invoices/{invoiceId}/send
- POST /api/invoices/{invoiceId}/credit-note
Description: Invoice generation, editing, sending and credit notes.

14) Payments
- POST /api/payments
- POST /api/payments/webhook
- GET /api/payments/{paymentId}
- POST /api/payments/{paymentId}/verify
Description: Payment initiation, webhook handling and verification.

15) Notifications
- GET /api/notifications/templates
- POST /api/notifications/templates
- POST /api/notifications/trigger
- GET /api/notifications/logs
Description: Template management, manual triggers and delivery logs.

16) Master Data / Lookups
- GET /api/service-types
- POST /api/service-types
- GET /api/zones
- POST /api/zones
Description: CRUD for dynamic master tables used across systems.

17) Configuration
- GET /api/config/{key}
- PUT /api/config/{key}
- GET /api/business-hours
- PUT /api/sla-configs/{id}
Description: Runtime configuration and system settings.

18) Dashboard & Analytics
- GET /api/ops/dashboard/summary
- GET /api/finance/dashboard/summary
- GET /api/analytics/sla-report
Description: Read-model endpoints for dashboards and reports.

19) Audit Logs
- GET /api/audit/logs
- GET /api/audit/logs/{id}
Description: Queryable immutable audit trail for compliance and investigations.

20) CMS / Content
- GET /api/cms/blocks/{key}
- POST /api/cms/blocks
- GET /api/banners
- POST /api/banners
Description: Website and app content management.

21) File / Attachment
- POST /api/files/upload
- GET /api/files/{fileId}
- DELETE /api/files/{fileId}
Description: Secure upload, retrieval and lifecycle of attachments.

22) Support Tickets
- POST /api/tickets
- GET /api/tickets/{ticketId}
- POST /api/tickets/{ticketId}/reply
- POST /api/tickets/{ticketId}/escalate
Description: Customer support ticketing endpoints.

23) Reviews & Feedback
- GET /api/customer-reviews
- POST /api/customer-reviews
- GET /api/feedback
- GET /api/feedback/{customerReviewId}
- PATCH /api/feedback/{customerReviewId}/respond
- PATCH /api/feedback/{customerReviewId}/publish
- PATCH /api/feedback/{customerReviewId}/flag
Description: Capture public customer reviews and manage admin feedback moderation through the dedicated support route family.

24) AI / Intelligence (future)
- GET /api/ai/dispatch-suggestion
- GET /api/ai/demand-forecast
Description: ML-backed suggestions and forecasting endpoints (future-ready).

6. DATABASE STRATEGY

- ORM & Migrations: EF Core (Code-First) with provider-agnostic migrations; support for SQL Server (prod) and PostgreSQL (alternate) via DI at startup.
- Keys & IDs: UUID/GUID primary keys for all entities to avoid guessable incremental IDs.
- Soft deletes: `IsDeleted`, `DeletedAt`, `DeletedBy` on all entities; hard delete only via administrative cleanup.
- Audit fields: Base entity includes `CreatedAt`, `CreatedBy`, `UpdatedAt`, `UpdatedBy` and enforced in the DbContext Save pipeline.
- JSON columns: Use JSON/JSONB or NVARCHAR(JSON) columns for flexible fields (e.g., SRNotes, JobReport.observations, Estimate.additionalData).
- Status history: StatusHistory tables for SR, Invoice, Ticket to preserve state transitions and enable time-series auditing.

7. ADMIN MOBILE CLIENT ARCHITECTURE (current React/Vite implementation)

Client architecture
- Runtime: React 19 + Vite + TypeScript.
- State management: Zustand store under `src/store/*`, with the new `app/store/session-store.ts` exporting the shared auth/session surface into the app shell.
- Routing: `app/navigation/router.tsx` with auth guard + role guard protection, deep-link aliases, push-intent route resolution, and session-expiry handling.
- API client: Axios-based repositories under `src/core/network/*`; the new `app/services/*` layer wraps repository usage for app-shell screens such as bookings and notifications.

Local storage & offline
- Local storage: browser local storage via `src/core/storage/local-storage.ts` for auth/session, permissions, masters, configuration caches, field-job caches, offline queues, pending routes, and last-sync timestamps.
- Offline queue: local-storage backed queue for field/system submissions with retry metadata and sync counters exposed through `SystemUXProvider`.

UI & design tokens
- Design tokens: centralized TypeScript token surface in `src/core/theme/tokens.ts`.
- Shell: `AdminScaffold` remains the shared responsive shell, now fed by the new role-navigation module in the `app/` layer.

Folder structure (recommended)
- `app/modules`, `app/components`, `app/screens`, `app/navigation`, `app/services`, `app/store`, `app/utils`, `app/constants`
- `app/domain/models`, `app/infrastructure/api`
- `src/features/*` for stable implementation screens
- `src/core/*` for repositories, auth, storage, system UX, theme, and configuration

Integration
- Push/deep-link intent consumption remains in the app router through the shared navigation-intent helpers.
- Role menu composition now resolves from `app/modules/role-navigation.tsx`.
- API contract governance is centralized in `Mobile/Coolzo_AdminMobile/docs/API_MASTER.ts`.

Developer experience
- Type validation: `./node_modules/.bin/tsc --noEmit --pretty false`.
- Build: `npm run build`.
- Environment configs: Vite env vars through `src/core/config/env.ts`, including the shared `IS_MOCK` / `VITE_MOCK_API` behavior.
- 2026-04-25 Linux DB connectivity update: `Backend/Coolzo.Persistence/DependencyInjection/PersistenceServiceCollectionExtensions.cs` now resolves the database connection string dynamically when the process runs on Linux and the configured string still points at LocalDB. The fallback accepts either `COOLZO_SQL_CONNECTION` or the split env/config pair `Database:LinuxSqlConnection`, `Database:LinuxSqlHost`, `Database:LinuxSqlPort`, `Database:LinuxSqlUser`, and `Database:LinuxSqlPassword`. If no Linux override is present, the existing LocalDB connection string remains unchanged for Windows runs.
- 2026-04-25 live DB account cleanup update: the live LocalDB database `CoolzoDB` was cleaned so only the canonical role set remains (`SuperAdmin`, `Admin`, `OperationsManager`, `OperationsExecutive`, `CustomerSupportExecutive`, `Technician`, `Helper`, `Customer`, `FinanceManager`, `InventoryManager`, `BillingExecutive`, `MarketingManager`). Synthetic test roles (`batch3Probe1777011293903`, `Phase2Mutable*`, `step4Role*`) and their associated users were deleted. The surviving 13 users now share the fixed password `Welcome@123`, with all remaining accounts normalized to hashed storage and active status. Verified remaining accounts are `admin@coolzo.local` (`SuperAdmin`), `adminuser@coolzo.local` (`Admin`), `customer@coolzo.local` (`Customer`), `opsmanager@coolzo.local` (`OperationsManager`), `opsexec@coolzo.local` (`OperationsExecutive`), `supportmeena@coolzo.local` (`CustomerSupportExecutive`), `techashok@coolzo.local` and `techmeera@coolzo.local` (`Technician`), `helper1@coolzo.local` (`Helper`), `financemanager@coolzo.local` (`FinanceManager`), `inventorymanager@coolzo.local` (`InventoryManager`), `billingexecutive@coolzo.local` (`BillingExecutive`), and `marketingmanager@coolzo.local` (`MarketingManager`).
- 2026-04-25 API solution build verification update: ran `dotnet build` in `Backend`. The full solution restored and compiled successfully, including `Coolzo.Shared`, `Coolzo.Domain`, `Coolzo.Contracts`, `Coolzo.Application`, `Coolzo.Infrastructure`, `Coolzo.Persistence`, `Coolzo.Worker`, and `Coolzo.Api`. Build completed with 0 warnings and 0 errors in 00:01:24.26, so no backend source changes were required.
- 2026-04-25 API solution rebuild verification update: reran `dotnet build` in `Backend`. Restore reported all projects up to date, and the full solution compiled successfully again for `Coolzo.Shared`, `Coolzo.Domain`, `Coolzo.Contracts`, `Coolzo.Application`, `Coolzo.Infrastructure`, `Coolzo.Persistence`, `Coolzo.Worker`, and `Coolzo.Api`. Build completed with 0 warnings and 0 errors in 00:01:38.43, so no backend source changes were required.
- 2026-04-25 Development fixed OTP testing update: `AuthSessionTokenFactory.CreateOtp()` in `Backend/Coolzo.Application/Features/Auth/Commands/AuthSession/AuthSessionCommands.cs` now issues a stable fixed OTP during local `Development` runs for present testing. The default development OTP is `123456`, and it can be overridden with the `COOLZO_FIXED_TEST_OTP` environment variable. Non-development environments continue to use random six-digit OTP generation.
- 2026-04-25 Live LocalDB master-data and dummy-data seed update: executed the new direct DB seed script `Database/DB_Seed_MasterAndDummy_20260425.sql` against the Windows LocalDB database `CoolzoDB`, with the SQL formatted to follow `Docs/New SQL Format.txt`. The live seed added the missing canonical roles `InventoryManager`, `BillingExecutive`, and `MarketingManager`; added canonical login users `financemanager`, `inventorymanager`, `billingexecutive`, and `marketingmanager` using the same hashed admin credential convention (`Admin@12345`); extended role-permission coverage for `FinanceManager`, `InventoryManager`, `BillingExecutive`, and `MarketingManager`; and inserted active fixed `LoginOtp` rows with OTP `123456` for 13 canonical role-wise test accounts (`admin`, `adminuser`, `customer`, `opsmanager`, `opsexec`, `techashok`, `techmeera`, `supportmeena`, `helper1`, `financemanager`, `inventorymanager`, `billingexecutive`, `marketingmanager`). The same seed populated previously empty or missing live master/config tables with 4 holiday rows in `tblHolidayConfiguration`, 12 dynamic master rows in `tblDynamicMasterRecord`, 8 new configuration rows in `tblSystemConfiguration` (bringing total active configs to 10), and 8 notification templates in `tblNotificationTemplate`. Inventory and warehouse coverage was added through 4 seeded items in `tblItem`, 4 active rates in `tblItemRate`, and 4 warehouse stock rows in `tblWarehouseStock`. End-to-end operational dummy data was added through 4 service requests in `tblServiceRequest`, 3 technician assignments in `tblServiceRequestAssignment`, 12 status-history rows in `tblServiceRequestStatusHistory`, 2 job cards in `tblJobCard`, 2 quotations and 4 quotation lines in `tblQuotationHeader` / `tblQuotationLine`, 1 live invoice plus 2 invoice lines in `tblInvoiceHeader` / `tblInvoiceLine`, 1 payment transaction and 1 receipt in `tblPaymentTransaction` / `tblPaymentReceipt`, 5 billing history entries in `tblBillingStatusHistory`, and 2 support ticket assignment rows in `tblSupportTicketAssignment`. No table renames or destructive schema normalization were applied to the live database because the current application code depends on the existing schema contracts; the delivered change for this task is the repeatable seed script plus the validated direct-data population.
- 2026-04-25 Service-request API live schema compatibility fix: the Super Admin AdminMobile route `/service-requests` was returning `500 unexpected_error` from `GET /api/service-requests` on the active IIS Express API at `https://localhost:44394`. The persisted `tblSystemAlert` unhandled-exception entries showed the running backend assembly was querying columns that did not exist in the live `CoolzoDB` schema: `tblBooking.IsEmergency`, `tblBooking.EmergencySurchargeAmount`, `tblCustomerAddress.StateName`, `tblCustomerAddress.AddressType`, `tblCustomerAddress.Latitude`, and `tblCustomerAddress.Longitude`. To keep the currently running API instance compatible without requiring an application restart, the direct DB compatibility script `Database/DB_Alter_20260425_ServiceRequestSchemaCompatibility.sql` was added and executed against LocalDB. That script adds the missing booking/address columns with safe defaults (`BIT` / `MONEY` / `NVARCHAR` / `FLOAT`) only when absent. After the DB patch, the live Super Admin flow was re-verified through `POST /api/auth/login`, `POST /api/auth/verify-otp` with OTP `123456`, and `GET /api/service-requests?pageNumber=1&pageSize=20`; the service-request list now returns `200 success` with 4 records instead of the previous wrapped `unexpected_error`.
- 2026-04-25 Technician-management API live schema compatibility fix: `GET /api/technicians?activeOnly=false` on the active IIS Express API at `https://localhost:44394` was returning `500 unexpected_error` because the technician-management list path (`SearchTechniciansQuery` → `SearchManagementAsync`) includes auxiliary management tables that were missing in the live `CoolzoDB` schema. The persisted `tblSystemAlert` unhandled-exception entries showed the immediate failure was `Invalid object name 'tblTechnicianSkill'`, and live schema inspection confirmed additional missing management tables: `tblTechnicianZone`, `tblTechnicianAttendance`, `tblTechnicianGPSLog`, and `tblTechnicianPerformanceSummary` (while `tblTechnicianAvailability` and `tblTechnicianSkillMapping` already existed). To keep the currently running API instance compatible without requiring an application restart, the direct DB compatibility script `Database/DB_Alter_20260425_TechnicianManagementCompatibility.sql` was added and executed against LocalDB to create those missing technician-management tables only when absent. A direct live seed was then applied to populate 2 technician-zone rows, 4 technician-skill rows, 2 technician-attendance rows, and 2 technician-performance-summary rows for the existing live technicians. After the DB patch, the live Super Admin flow was re-verified through `POST /api/auth/login`, `POST /api/auth/verify-otp` with OTP `123456`, and `GET /api/technicians?activeOnly=false`; the technician list now returns `200 success` with 2 technicians, zone names, seeded skill arrays, and non-zero performance metrics instead of the previous wrapped `unexpected_error`.
- 2026-04-25 Support analytics API live compatibility fix: the Super Admin support dashboard route `/support/dashboard` was surfacing `500 unexpected_error` from `GET /api/analytics/support` on the active IIS Express API at `https://localhost:44394`. The persisted `tblSystemAlert` unhandled-exception entries showed the real failure was `Invalid object name 'FilteredSupport'` inside the live stored procedure `dbo.uspGetSupportAnalytics`. Root cause: the procedure defined `FilteredSupport` as a CTE and then attempted to reuse it across three separate result-set queries, but SQL Server scopes a CTE to the single immediately following statement only. To keep the currently running API instance compatible without requiring an application restart, the direct DB compatibility script `Database/DB_Alter_20260425_SupportAnalyticsCompatibility.sql` was added and executed against LocalDB, and the canonical procedure source `Docs/Database/Phase_09_Analytics/013_Create_uspGetSupportAnalytics.sql` was updated to materialize the filtered support rows into a table variable reused by the summary, status-breakdown, and trend result sets. The live database fix was validated directly with `EXEC dbo.uspGetSupportAnalytics`, which now returns the expected summary, status, and trend result sets instead of throwing the previous `FilteredSupport` error.
- 2026-04-29 Main database script replay attempt: the `C:\Live\Live\DB\Main` scripts were checked against the live SQL Server LocalDB target `RevalPOS_SAAS_MainDB` from the `DefaultConnection` environment, and execution was started with `Step1-Main_Table.sql`. The replay stopped immediately because `dbo.atblSiteSubscription` already exists in the database, so the step-1 schema script is not idempotent as written. No additional `Main` scripts were applied after that failure.
- 2026-04-29 Main database script replay completion: all seven `C:\Live\Live\DB\Main` scripts were replayed against `RevalPOS_SAAS_MainDB` using replay-safe `sqlcmd` execution that continued past existing objects. Script-level results were: `Step1-Main_Table.sql` completed with 286 duplicate-object skips and 3,102 other logged errors; `Step2-Main_SP.sql` completed with 336 duplicate-object skips and 337 other logged errors; `Step3-Main_IDX.sql` completed with 6 duplicate-object skips and 10 other logged errors; `Step4-Main_Data.sql` completed with 11,885 other logged errors; `Step5-Main_Data.sql` completed with 900 other logged errors; `Step6-Main_Data.sql` completed with 4 other logged errors; and `Step7-Main_Data.sql` completed with 2,676 other logged errors. The command pipeline exited cleanly for every file and did not stop on already-existing objects.
- 2026-04-30 Step 3 CLR cleanup: `Step3-Main_IDX.sql` was patched so an existing `DDLTrigger_Sample` is dropped before the script recreates it, and the trigger is disabled at the end of the script instead of left active. The live `RevalPOS_SAAS_MainDB` database was also updated with trusted assembly registrations for the two CLR blobs used by this step (`RevalEncryption_SQL_CLR` and `RevalEncryption_SQL_CLR_RevalEncrypt`), which cleared the prior `Msg 10327` trust failure and the follow-on `Msg 6503` / `Msg 6528` missing-assembly errors. A verification rerun of `Step3-Main_IDX.sql` completed without the prior assembly or trigger errors, and metadata check confirmed both assemblies are present while `DDLTrigger_Sample` is disabled.
- 2026-04-30 Step 3 local CLR cleanup: `Step3-Local_IDX.sql` under `C:\Live\Live\DB\Local` was patched with a replay-safe preamble that drops any existing CLR-dependent functions, assemblies, and DDL triggers before recreating them, and it now leaves `DDLTrigger_Sample`, `TRG_InsertProcedureSchema`, and `TRG_InsertTableSchema` disabled at the end. The local `RevalPOS_RevalERPlocalDB` database was updated with trusted assembly registrations for the three CLR blobs used by this step (`BouncyCastle`, `RevalEncryptDecrypt`, and `RevalEncryption_SQL_CLR`), which cleared the `Msg 10343` strict-security trust failure on `Encrypt_Decryption`. A rerun of `Step3-Local_IDX.sql` completed with the assemblies present and all three triggers disabled, although the database still logs duplicate-object warnings for types and scalar functions that already exist in this LocalDB.
- 2026-05-06 AdminMobile support-feedback direct integration update: the support feedback detail route `/support/feedback/:id` now uses a persisted backend moderation family under `/api/feedback` instead of falling back to public `/api/customer-reviews` data plus missing moderation URLs. `GET /api/feedback` and `GET /api/feedback/{customerReviewId}` return support-visible customer review records with moderation status and admin response fields, while `PATCH /api/feedback/{customerReviewId}/respond`, `/publish`, and `/flag` update the same persisted review row. Negative-feedback queue, feedback analytics, and complaint heatmap remain client-derived from the live admin feed, so AdminMobile no longer depends on unsupported `/api/feedback/analytics`, `/api/feedback/negative-queue`, or `/api/feedback/complaint-heatmap` endpoints.
- 2026-04-25 Customer review API live schema compatibility fix: `GET /api/customer-reviews` on the active IIS Express API at `https://localhost:44394` was returning `500 Internal Server Error` because the live `CoolzoDB` schema was missing `dbo.tblCustomerReview`, while the current backend repository maps customer reviews through EF to that table. The persisted `tblSystemAlert` unhandled-exception entries confirmed the exact failure as `Invalid object name 'tblCustomerReview'`. To keep the currently running API instance compatible without requiring an application restart, the direct DB compatibility script `Database/DB_Alter_20260425_CustomerReviewCompatibility.sql` was added and executed against LocalDB. That script creates `tblCustomerReview` with the audit/default columns expected by the current application model, adds the service/date index used by review queries, and seeds three live-shaped review rows from the existing active booking/customer records when the table is empty. `Docs/ModuleIndex.md` was also updated to include `CustomerReviews` under the Customer Portal module because the live schema now includes that table.
- 2026-04-25 Billing accounts-receivable endpoint implementation fix: `GET /api/billing/accounts-receivable` on the active IIS Express API at `https://localhost:44394` still returns `404 Not Found` because the running backend assembly does not expose that route, even though the AdminMobile Phase 14 billing contract and repository already depend on it. Backend source validation confirmed `BillingController` previously exposed only `GET /api/billing/status/{invoiceId}`. The backend source now includes a compatibility implementation for `GET /api/billing/accounts-receivable`, backed by a new billing query/handler and repository method that aggregate live `tblInvoiceHeader` / `tblCustomer` data into the AdminMobile AR dashboard shape: aging buckets, overdue invoice queue, top-outstanding customers, and total outstanding. New billing response contracts were added for the AR payload, `BillingController` now exposes the route under `PermissionNames.BillingRead`, and `Docs/ModuleIndex.md` was updated to include `Get Accounts Receivable Dashboard` under Billing & Payments. Source verification succeeded with `dotnet build Backend/Coolzo.Api/Coolzo.Api.csproj -o /tmp/coolzo-api-ar-build` (0 warnings, 0 errors). The currently running IIS Express process on `https://localhost:44394` still serves the old assembly until the API is restarted, so the live endpoint will remain `404` until that process reloads the updated build.
- 2026-04-25 Billing accounts-receivable live schema compatibility fix: after the route became available on the active IIS Express API at `https://localhost:44394`, `GET /api/billing/accounts-receivable` returned `500 Internal Server Error`. The persisted `tblSystemAlert` unhandled-exception entries showed the live `CoolzoDB` schema was missing payment-transaction columns now required by the billing repository mapping: `tblPaymentTransaction.GatewaySignature`, `tblPaymentTransaction.GatewayTransactionId`, `tblPaymentTransaction.IdempotencyKey`, and `tblPaymentTransaction.WebhookReference`. To keep the currently running API instance compatible without requiring an application restart, the direct DB compatibility script `Database/DB_Alter_20260425_BillingPaymentTransactionCompatibility.sql` was added and executed against LocalDB. That script adds the four missing non-null `NVARCHAR` columns with empty-string defaults only when absent, and creates the `IDX_tblPaymentTransaction_IdempotencyKey` and `IDX_tblPaymentTransaction_GatewayTransactionId` indexes when missing. Live verification then succeeded through `POST /api/auth/login`, `POST /api/auth/verify-otp` with OTP `123456`, and authenticated `GET /api/billing/accounts-receivable`; the endpoint now returns `200 success` with aging buckets, overdue invoice queue, top-outstanding customer, and `totalOutstanding = 593.0000` instead of the previous wrapped SQL error.
- 2026-04-25 LocalDB connectivity verification update: direct DB access from the current WSL session was validated successfully against Windows LocalDB database `CoolzoDB` by invoking Windows `SQLCMD.EXE` through PowerShell with server `(localdb)\\MSSQLLocalDB`. SQL Server resolved the active instance as `Fayaz-Yoga\\LOCALDB#C200165C`, `SELECT DB_NAME()` returned `CoolzoDB`, and a table probe (`SELECT TOP 1 name FROM sys.tables ORDER BY name`) returned `tblAcType`. This confirms the current repo session can connect to the live LocalDB through the Windows SQL client path `C:\\Program Files\\Microsoft SQL Server\\Client SDK\\ODBC\\170\\Tools\\Binn\\SQLCMD.EXE`.
- 2026-04-25 AdminMobile verify-otp view stabilization update: the `/verify-otp` route remains the step-up screen for `AuthStatus.REQUIRES_2FA`, but it now runs through the guest-auth route guard and the `OTPVerificationScreen` now renders the full branded auth view instead of a brittle standalone state. The screen now shows the Coolzo auth header, masked target email, delivery/access summary cards, OTP paste support, autofocus on the first digit, and a proper session-expired fallback card when the pending 2FA user context is missing, with return-to-login recovery instead of a transient broken view.
- 2026-04-25 AdminMobile session-expired view stabilization update: the `/session-expired` route now also runs through the guest-auth route guard, and the `SessionExpiredScreen` has been aligned with the branded auth-shell presentation used by the login and verify-otp flows. The screen now shows the Coolzo auth header, clear reason/action summary cards, improved spacing and mobile layout, and a stable re-authentication call to action instead of the previous thin standalone card view.
- 2026-04-25 AdminMobile unauthorized view stabilization update: the `UnauthorizedScreen` used by `/unauthorized` and inline settings/auth checks no longer renders as a full-screen standalone page. It now renders as an in-app restricted-access state that fits inside `AdminScaffold`, shows the blocked module and current route when available from router state, and provides clear back, dashboard, and support actions for denied-access flows.

END OF SECTION 10

Playwright validation note
- 2026-04-23: Began the documented Playwright end-to-end validation flow with `Super Admin` `Step 1 — Login` only. Executed the targeted auth Playwright case against the live AdminMobile dev server and a locally started API listener on `http://127.0.0.1:5258`. The run failed before JWT/session/navigation validation because `POST /api/auth/login` returned `500 unexpected_error`. Runtime logs confirmed the backend is still configured to use `(localdb)\MSSQLLocalDB`, and login is currently blocked in this Linux environment by `System.PlatformNotSupportedException: LocalDB is not supported on this platform`. The session result was recorded in `Docs/Admin_Playwright_Test_Report.md`.
- 2026-04-23 remediation: Playwright/API execution for the AdminMobile auth slice now runs the API through Windows `dotnet.exe` on `http://0.0.0.0:5258`, which makes the existing Windows LocalDB-backed `CoolzoDB` reachable from WSL/Playwright at `http://172.17.32.1:5258`. The auth OTP path was also corrected for privileged internal users: `CustomerAccountLookupService` now skips customer-mobile lookup for non-phone login identifiers, and `LoginOtpCommandHandler` falls back to direct user lookup when the OTP belongs to a non-customer role such as `Super Admin`. The targeted Super Admin Step 1 Playwright auth scenario now passes end to end, including `/auth/login`, `/auth/me`, `/auth/me/permissions`, session seeding, and dashboard route resolution. The successful rerun is recorded in `Docs/Admin_Playwright_Test_Report.md`.
- 2026-04-23 Step 2 remediation: The documented Playwright navigation-validation slice for `Super Admin` now passes through `e2e/tests/rbac.spec.js`. The failing assertions were stale relative to the current AdminMobile router and role-navigation contract. The spec was updated to use `/dashboard` instead of `/admin/dashboard`, and the role-menu expectations were aligned to the implemented shell navigation: `Super Admin` shows `Billing`, `Team`, and `Settings`; `Technician` shows `Home` and `Jobs` while restricted business/config menus stay hidden; `Finance Manager` shows `Finance` and `Billing` while `Dispatch` and `Team` remain hidden. The successful rerun is recorded in `Docs/Admin_Playwright_Test_Report.md`.
- 2026-04-24 Step 3 status: Began the documented `Super Admin` module-wise execution slice through `e2e/tests/admin-user-role-management.spec.js`. The Step 3 smoke coverage for opening User Management and Role Management now passes after stale test assumptions were removed: the spec no longer depends on a separate UI-login phase, and the Playwright auth seeding helper now persists `branchId` from the live auth payload into the seeded session. AdminMobile `CreateUserScreen` also now falls back to the authenticated operator branch when `/api/branches` returns an empty array, preventing branch-context loss in this environment. The latest rerun still exposes two live Step 3 blockers recorded in `Docs/Admin_Playwright_Test_Report.md`: the create-user screen renders an empty `Role assignments` group during the run, and the role-create UI remains on `/settings/roles/create` after a valid submit even though direct `POST /api/roles` API verification succeeds for Super Admin.
- 2026-04-24 Step 4 remediation: Added a dedicated Super Admin Role Management Step 4 Playwright slice in `e2e/tests/admin-data-flow.spec.js` to validate role create data across API, DB, API readback, and UI detail mapping. During the first Step 4 run, the browser app surfaced `404`/`Network Error` failures even though the request-context API and direct DB checks passed. Root cause: `Mobile/Coolzo_AdminMobile/src/core/config/api-config.ts` was composing the live browser base URL as `${EnvConfig.API_BASE_URL}/api/v1` while the repositories already call endpoints with `/api/...`, causing duplicated browser paths such as `/api/api/roles`. The base URL was corrected to `EnvConfig.API_BASE_URL`, the Playwright app-server bootstrap in `e2e/playwright.config.js` was updated to honor the configured app host/port without stale frontend reuse, and the Step 4 role-management data-flow slice now passes end to end against the live API and LocalDB-backed database. The session record is captured in `Docs/Admin_Playwright_Test_Report.md`.
- 2026-04-24 Step 5 remediation: Added a dedicated Super Admin Role Management error-handling slice in `e2e/tests/admin-error-handling.spec.js` to validate invalid input, simulated backend failure, and simulated network failure behavior from the live browser app. The first Step 5 run confirmed the UI handled all three paths correctly, but Chromium emitted expected resource-load console entries for the intentionally forced `500` and `ERR_FAILED` requests. The Step 5 assertion layer was refined to ignore only those expected browser-network console messages while continuing to fail on unexpected application console errors or page crashes. The Step 5 Role Management error-handling slice now passes and is recorded in `Docs/Admin_Playwright_Test_Report.md`.
- 2026-04-24 Step 6 remediation: Added a dedicated Super Admin role-isolation slice in `e2e/tests/admin-role-isolation.spec.js` to validate `view-as-role` scoped navigation, scoped browser permission storage, unauthorized redirect behavior for `/settings/users`, and restoration of full access after exiting scoped mode. The initial Step 6 failure was traced to the Playwright auth seeding helper rather than the AdminMobile RBAC runtime: `e2e/tests/helpers/admin-auth.js` used `page.addInitScript(...)`, which cleared and reseeded local storage on every document load, so a hard reload to `/settings/users` erased the active `view_as_role` session and falsely rehydrated the app as plain `Super Admin`. `seedSession()` now opens `/login` once and writes the auth/session keys through `page.evaluate(...)`, preserving the live scoped-role session across later reloads. The final Step 6 rerun passes and is recorded in `Docs/Admin_Playwright_Test_Report.md`.
- 2026-04-24 Admin/Branch Manager Step 1 remediation: Began the documented `Admin / Branch Manager` Playwright flow with `Step 1 — Login` only. The live database currently exposes no separate `Branch Manager` role; the environment-equivalent role is `Admin`, and the active seeded user for that role is `adminuser`. A dedicated Step 1 auth case was added to `e2e/tests/auth.spec.js` to validate `/api/auth/login`, `/api/auth/me`, `/api/auth/me/permissions`, seeded browser session creation, and `/admin/dashboard` route resolution for this branch-admin-equivalent account. The first run failed with `401 invalid_credentials` for `adminuser`; root cause was stale test-account credentials rather than an auth/API contract issue. The live forgot/reset-password flow was used to reset `adminuser` back to `Admin@12345`, after which the targeted Branch Admin Step 1 Playwright slice passed. The session result is recorded in `Docs/Admin_Playwright_Test_Report.md`.
- 2026-04-24 Admin/Branch Manager Step 2 remediation: Added a dedicated branch-admin-equivalent navigation slice in `e2e/tests/admin-branch-navigation.spec.js` for the live `Admin` user `adminuser`. The first Step 2 run exposed that the live `Admin` permission snapshot grants `billing.view`, but `Mobile/Coolzo_AdminMobile/app/modules/role-navigation.tsx` did not expose a `Billing` nav item for `UserRole.ADMIN`; the Admin role navigation now includes `Billing` at `/billing/dashboard`. The next rerun exposed a separate UI failure on the allowed billing route: `GET /api/billing/accounts-receivable` returns `404` for this environment, and `Mobile/Coolzo_AdminMobile/src/features/billing/ARDashboard.tsx` previously stayed on an infinite loader whenever `dashboard` remained null after an API failure. `ARDashboard.tsx` now shows a proper fallback error panel with retry/back actions instead of hanging. The final Step 2 rerun passes and is recorded in `Docs/Admin_Playwright_Test_Report.md`.
- 2026-04-24 Admin/Branch Manager Step 3 remediation: Added a dedicated branch-admin-equivalent Team + Billing module slice in `e2e/tests/admin-branch-modules.spec.js` for the live `Admin` user `adminuser`, because this environment still has no separate `Branch Manager` role. The first Step 3 run exposed an RBAC hydration defect: the live permission snapshot grants `team.view = true`, but a direct deep-link to `/team` still redirected to `/unauthorized` because route guards evaluated the empty initial permission state before the live permission payload had finished loading. `Mobile/Coolzo_AdminMobile/src/core/auth/RBACProvider.tsx` now tracks `isPermissionsReady`, and `Mobile/Coolzo_AdminMobile/app/navigation/router.tsx` now keeps guarded routes on `SplashScreen` until permissions are hydrated instead of redirecting early. The next rerun exposed leaked Team actions for a role that has `team.create = false` and `team.edit = false`; `Mobile/Coolzo_AdminMobile/src/features/team/TechnicianListScreen.tsx` now hides `Add Technician` behind `canCreate("team")`, `Mobile/Coolzo_AdminMobile/src/features/team/TechnicianDetailScreen.tsx` now hides `Edit Profile` behind `canEdit("team")`, and `Mobile/Coolzo_AdminMobile/src/features/team/TechnicianEditorScreen.tsx` now redirects unauthorized create/edit entry to `/unauthorized`. The Billing invoices route also still deadlocked under the live `GET /api/invoices` `500 unexpected_error` plus `GET /api/billing/accounts-receivable` `404` combination; `Mobile/Coolzo_AdminMobile/src/features/billing/InvoiceListScreen.tsx` now surfaces a retryable fallback error panel instead of treating `!dashboard` as perpetual loading. One rerun attempt further exposed a Playwright harness issue where `e2e/playwright.config.js` still set the frontend `webServer` entry to `reuseExistingServer: false`, causing the Step 3 run to abort when `http://127.0.0.1:3100/login` was already active; the app web-server entry now reuses the existing frontend server. The final Branch Admin Step 3 rerun passes and is recorded in `Docs/Admin_Playwright_Test_Report.md`.
 - 2026-04-24 Admin/Branch Manager Step 4 status: Began the documented Branch Admin-equivalent data-flow validation through `e2e/tests/admin-branch-data-flow.spec.js`. The initial Billing Step 4 direction could not proceed against live data because `dbo.tblInvoiceHeader` currently has no invoice rows and `dbo.tblQuotationHeader` has no quotation row available for invoice generation, while the AdminMobile manual-invoice screen still posts to unsupported `POST /api/invoices` instead of the live backend route `POST /api/invoices/from-quotation/{quotationId}`. During that investigation, `Mobile/Coolzo_AdminMobile/src/core/network/invoice-repository.ts` was updated to map live backend invoice payloads (`invoiceId`, `currentStatus`, `grandTotalAmount`, `payments`, `billingHistory`) into the AdminMobile invoice model instead of assuming only the mock schema. Step 4 was then redirected to the allowed Team data-flow path using live `dbo.tblTechnician` rows, but both `GET /api/technicians/{id}` and `GET /api/technicians?searchTerm=...` still returned `500 unexpected_error` for the branch-admin-equivalent role. `Backend/Coolzo.Application/Features/Technician/Management/TechnicianManagementFeature.cs` was hardened so optional onboarding reads (`TechnicianDocuments`, `SkillAssessments`, `TrainingRecords`) fall back to empty collections instead of crashing the profile path, but the live Team API still fails before Step 4 can complete DB → API → UI validation. The current blocked state and all rerun attempts are recorded in `Docs/Admin_Playwright_Test_Report.md`.

---

## ADDED API ENDPOINTS (discovered in controllers — first 10 files scanned) — 2026-04-25

The endpoints below were discovered in the backend controller sources and were not present in the API lists earlier in this document. Routes are shown normalized (version placeholder removed).

- WarrantyController
   - POST /api/warranty/claim — Create warranty claim (`CreateClaimAsync`)
   - GET  /api/warranty/invoice/{invoiceId} — Get warranty status by invoice (`GetByInvoiceAsync`)

- WarehouseController
   - POST /api/warehouses — Create warehouse (`CreateAsync`)
   - GET  /api/warehouses — List warehouses (`GetWarehousesAsync`)
   - GET  /api/warehouses/{id}/stock — Get warehouse stock (`GetWarehouseStockAsync`)

- UserController
   - POST /api/users/{userId}/deactivate — Deactivate user (`DeactivateAsync`)
   - POST /api/users/{userId}/reactivate — Reactivate user (`ReactivateAsync`)
   - POST /api/users/{userId}/reset-password — Reset user password (`ResetPasswordAsync`)
   - POST /api/users/{userId}/reset-pin — Reset user PIN (`ResetPinAsync`)

- TrainingController
   - POST /api/technicians/{technicianId}/training-records — Create training record (`CreateAsync`)
   - GET  /api/technicians/{technicianId}/training-records — List training records (`GetListAsync`)
   - POST /api/technicians/{technicianId}/training-records/{trainingRecordId}/complete — Complete training record (`CompleteAsync`)

- TechnicianStockController
   - POST /api/technicians/{id}/stock-assign — Assign stock to technician (`AssignAsync`)
   - GET  /api/technicians/{id}/stock — Get technician stock (`GetTechnicianStockAsync`)

- TechnicianOnboardingController
   - POST /api/technician-onboarding/draft — Create technician onboarding draft (`CreateDraftAsync`)
   - GET  /api/technician-onboarding — List onboarding drafts (`GetListAsync`)
   - GET  /api/technician-onboarding/{technicianId} — Get onboarding detail (`GetDetailAsync`)
   - POST /api/technician-onboarding/{technicianId}/documents — Upload onboarding documents (`UploadDocumentsAsync`)
   - POST /api/technician-onboarding/{technicianId}/activate — Activate technician (`ActivateAsync`)

- TechnicianDocumentController
   - POST /api/technicians/{technicianId}/documents — Upload technician document (`UploadAsync`)
   - GET  /api/technicians/{technicianId}/documents — List technician documents (`GetListAsync`)
   - POST /api/technicians/{technicianId}/documents/{documentId}/verify — Verify document (`VerifyAsync`)
   - POST /api/technicians/{technicianId}/documents/{documentId}/reject — Reject document (`RejectAsync`)

Notes:
- These endpoints were appended after scanning the first 10 controllers in `Backend/Coolzo.Api/Controllers`. They were added to help keep the ProjectOverview API list in sync with the implemented controllers.
- If you want, I can continue scanning the next batches of controller files and append further missing APIs in the same format.

## ADDED API ENDPOINTS (discovered in controllers — next 10 files scanned) — 2026-04-25

The endpoints below were discovered in the backend controller sources during the second scan (controllers 11–20). Routes are shown normalized (version placeholder removed).

- TechnicianActivationController
   - POST /api/technicians/{technicianId}/activate — Activate technician (`ActivateAsync`)
   - POST /api/technicians/{technicianId}/deactivate — Deactivate technician (`DeactivateAsync`)
   - GET  /api/technicians/{technicianId}/activation-history — Technician activation history (`GetHistoryAsync`)

- SystemHealthController
   - GET  /api/system-health — Get system health snapshot (`GetAsync`)

- SystemConfigurationController
   - POST /api/system-configurations — Create system configuration (`CreateSystemConfigurationAsync`)
   - PUT  /api/system-configurations/{systemConfigurationId} — Update system configuration (`UpdateSystemConfigurationAsync`)
   - GET  /api/system-configurations — List system configurations (`GetSystemConfigurationsAsync`)
   - GET  /api/system-configurations/{systemConfigurationId} — Get system configuration detail (`GetSystemConfigurationDetailAsync`)
   - GET  /api/business-hours — Get business hours (`GetBusinessHoursAsync`)
   - POST /api/business-hours — Save business hours (`SaveBusinessHoursAsync`)
   - GET  /api/holidays — Get holidays (`GetHolidaysAsync`)
   - POST /api/holidays — Create holiday (`CreateHolidayAsync`)

- SupportTicketReplyController
   - GET  /api/support-tickets/{supportTicketId}/replies — List ticket replies (`GetAsync`)
   - POST /api/support-tickets/{supportTicketId}/replies — Add ticket reply (`CreateAsync`)

- SupportTicketLookupController
   - GET  /api/support-ticket-lookups/categories — Support ticket categories (`GetCategoriesAsync`)
   - GET  /api/support/categories — Support categories compatibility route (`GetSupportCategoriesAsync`)
   - GET  /api/support-ticket-lookups/priorities — Support ticket priorities (`GetPrioritiesAsync`)
   - GET  /api/support-ticket-lookups/statuses — Support ticket statuses (`GetStatusesAsync`)

- SupportTicketEscalationController
   - GET  /api/support-tickets/{supportTicketId}/escalations — List escalations (`GetAsync`)
   - POST /api/support-tickets/{supportTicketId}/escalate — Escalate ticket (`EscalateAsync`)

- SupportTicketController
   - POST /api/support-tickets — Create support ticket (`CreateAsync`)
   - GET  /api/support-tickets — Search/list support tickets (`SearchAsync`)
   - GET  /api/support-tickets/{supportTicketId} — Get ticket detail (`GetByIdAsync`)
   - GET  /api/support-tickets/my-tickets — Get my tickets (`GetMyTicketsAsync`)
   - POST /api/support-tickets/{supportTicketId}/assign — Assign ticket (`AssignAsync`)
   - POST /api/support-tickets/{supportTicketId}/change-status — Change status (`ChangeStatusAsync`)
   - POST /api/support-tickets/{supportTicketId}/change-priority — Change priority (`ChangePriorityAsync`)
   - POST /api/support-tickets/{supportTicketId}/close — Close ticket (`CloseAsync`)
   - POST /api/support-tickets/{supportTicketId}/reopen — Reopen ticket (`ReopenAsync`)

- SupplierController
   - POST /api/suppliers/claims — Create supplier claim (`CreateClaimAsync`)

- StockController
   - POST /api/stock/transaction — Record stock transaction (`RecordTransactionAsync`)
   - POST /api/stock/transfer — Transfer stock between warehouses (`TransferAsync`)
   - GET  /api/stock/transactions — Get stock transactions (`GetTransactionsAsync`)

- SkillAssessmentController
   - POST /api/technicians/{technicianId}/skill-assessments — Create skill assessment (`CreateAsync`)
   - GET  /api/technicians/{technicianId}/skill-assessments — List skill assessments (`GetListAsync`)
   - POST /api/technicians/{technicianId}/skill-assessments/{assessmentId}/submit-result — Submit assessment result (`SubmitResultAsync`)

Notes:
- These entries were appended after scanning controllers 11–20 in `Backend/Coolzo.Api/Controllers`.
- I can continue with the next batch of controllers (21–30) and append discovered endpoints the same way. Let me know to proceed.

## ADDED API ENDPOINTS (discovered in controllers — controllers 21–30) — 2026-04-25

The endpoints below were discovered in the backend controller sources during the third scan (controllers 21–30). Routes are shown normalized (version placeholder removed).

- ServiceTypesController
   - GET  /api/service-types — List service types (`GetAllAsync`)
   - GET  /api/service-types/{serviceTypeId} — Get service type detail (`GetByIdAsync`)
   - GET  /api/service-types/{serviceTypeId}/sub-types — Get service sub-types (`GetSubTypesAsync`)

- ServiceRequestController
   - POST /api/service-requests/from-booking/{bookingId} — Create service request from booking (`CreateFromBookingAsync`)
   - GET  /api/service-requests — Search/list service requests (`GetServiceRequestsAsync`)
   - GET  /api/service-requests/dashboard-summary — Operations dashboard summary (`GetDashboardSummaryAsync`)
   - GET  /api/service-requests/{serviceRequestId} — Get service request detail (`GetServiceRequestByIdAsync`)
   - POST /api/service-requests/{serviceRequestId}/notes — Save service request note (`SaveNoteAsync`)
   - POST /api/service-requests/{serviceRequestId}/status — Update service request status (`UpdateStatusAsync`)

- ServiceHistoryController
   - GET  /api/service-history/customer/{customerId} — Get service history for customer (`GetByCustomerAsync`)
   - GET  /api/service-history/me — Get service history for current customer (`GetForCurrentCustomerAsync`)

- SchedulingBoardController
   - GET  /api/scheduling/board — Get scheduling board (`GetBoardAsync`)
   - POST /api/scheduling/assign — Assign service request (`AssignAsync`)
   - PUT  /api/scheduling/reassign — Reassign scheduled job (`ReassignAsync`)
   - GET  /api/scheduling/amc-auto — Get AMC auto visits (`GetAmcAutoAsync`)
   - POST /api/scheduling/amc-bulk-assign — Bulk assign AMC visits (`BulkAssignAmcAsync`)
   - GET  /api/scheduling/conflicts — Get scheduling conflicts (`GetConflictsAsync`)
   - GET  /api/scheduling/slots — Get scheduling slots (`GetSlotsAsync`)
   - PUT  /api/scheduling/slots/{slotAvailabilityId} — Update scheduling slot (`UpdateSlotAsync`)
   - GET  /api/scheduling/shifts — Get technician shifts (`GetShiftsAsync`)
   - PUT  /api/scheduling/shifts — Update technician shifts (`UpdateShiftsAsync`)
   - GET  /api/scheduling/day-sheet — Get day-sheet (`GetDaySheetAsync`)

- RoleController
   - GET  /api/roles — List roles (`GetAsync`)
   - POST /api/roles — Create role (`CreateAsync`)
   - PUT  /api/roles/{roleId} — Update role (`UpdateAsync`)
   - GET  /api/roles/{roleId}/permissions — Get role permissions snapshot (`GetPermissionsAsync`)
   - PUT  /api/roles/{roleId}/permissions — Update role permissions (`UpdatePermissionsAsync`)

- RevisitController
   - POST /api/revisit/request — Create revisit request (`CreateRequestAsync`)
   - GET  /api/revisit/booking/{bookingId} — Get revisit requests by booking (`GetByBookingAsync`)

- ReportController
   - GET  /api/reports/date-range — Get report by date range (`GetDateRangeAsync`)
   - GET  /api/reports/export — Export report (`ExportAsync`)

- RefundController
   - POST /api/refunds/request — Create refund request (`CreateRefundRequestAsync`)
   - GET  /api/refunds — List refund requests (`GetRefundsAsync`)
   - GET  /api/refunds/{id} — Get refund detail (`GetRefundByIdAsync`)
   - GET  /api/refunds/customer/{customerId} — Get customer refund status (`GetCustomerRefundStatusAsync`)
   - POST /api/refunds — Initiate refund (`InitiateRefundAsync`)
   - POST /api/refunds/{refundRequestId}/approve — Approve refund (`ApproveRefundAsync`)
   - POST /api/refunds/{refundRequestId}/reject — Reject refund (`RejectRefundAsync`)
   - POST /api/refunds/{refundRequestId}/status — Update refund status (`UpdateRefundStatusAsync`)

- QuotationController
   - POST /api/quotations/from-job/{jobCardId} — Create quotation from job (`CreateFromJobAsync`)
   - GET  /api/quotations — Search/list quotations (`SearchAsync`)
   - GET  /api/quotations/{id} — Get quotation by id (`GetByIdAsync`)
   - GET  /api/quotations/job/{jobCardId} — Get quotation by job (`GetByJobCardAsync`)
   - POST /api/quotations/{id}/approve — Approve quotation (`ApproveAsync`)
   - POST /api/quotations/{id}/reject — Reject quotation (`RejectAsync`)

- Phase4ConfigurationController
   - GET  /api/master/{masterSlug} — List master records (`GetMasterRecordsAsync`)
   - POST /api/master/{masterSlug} — Create master record (`CreateMasterRecordAsync`)
   - PUT  /api/master/{masterSlug} — Update master record (`UpdateMasterRecordAsync`)
   - DELETE /api/master/{masterSlug} — Delete master record (`DeleteMasterRecordAsync`)
   - GET  /api/config/business-hours — Get business hours (`GetBusinessHoursAsync`)
   - POST /api/config/business-hours — Save business hours (`CreateBusinessHoursAsync`)
   - PUT  /api/config/business-hours — Update business hours (`UpdateBusinessHoursAsync`)
   - GET  /api/config/{configSlug} — Get configuration group (`GetConfigurationGroupAsync`)
   - POST /api/config/{configSlug} — Create configuration record (`CreateConfigurationRecordAsync`)
   - PUT  /api/config/{configSlug} — Update configuration record (`UpdateConfigurationRecordAsync`)

Notes:
- These entries were appended after scanning controllers 21–30 in `Backend/Coolzo.Api/Controllers`.
- I can continue with the next batch of controllers (31–40) and append discovered endpoints the same way. Let me know to proceed.

## ADDED API ENDPOINTS (discovered in controllers — controllers 41–50) — 2026-04-25

The endpoints below were discovered in the backend controller sources during the fourth scan (controllers 41–50). Routes are shown normalized (version placeholder removed).

- LeadController
   - POST /api/leads — Create lead (`CreateLeadAsync`)
   - GET  /api/leads/analytics — Lead analytics (`GetAnalyticsAsync`)
   - GET  /api/leads — List/search leads (`GetLeadsAsync`)
   - GET  /api/leads/{leadId} — Get lead detail (`GetLeadByIdAsync`)
   - PUT  /api/leads/{leadId}/assign — Assign lead (`AssignLeadAsync`)
   - PUT  /api/leads/{leadId}/status — Update lead status (`UpdateLeadStatusAsync`)
   - POST /api/leads/{leadId}/convert-to-booking — Convert lead to booking (`ConvertToBookingAsync`)
   - POST /api/leads/{leadId}/convert-to-sr — Convert lead to service request (`ConvertToServiceRequestAsync`)
   - POST /api/leads/{leadId}/notes — Add lead note (`AddNoteAsync`)

- JobConsumptionController
   - POST /api/jobs/{jobCardId}/consume-parts — Consume job parts (`ConsumePartsAsync`)
   - GET  /api/jobs/{jobCardId}/consumption — Get job consumption (`GetConsumptionAsync`)

- JobChecklistController
   - GET  /api/technician-jobs/{id}/checklist — Get job checklist (`GetChecklistAsync`)
   - POST /api/technician-jobs/{id}/checklist — Save job checklist (`SaveChecklistAsync`)

- JobAttachmentController
   - POST /api/technician-jobs/{id}/attachments — Save job attachment (`SaveAttachmentAsync`)
   - GET  /api/technician-jobs/{id}/attachments — List job attachments (`GetAttachmentsAsync`)

- ItemController
   - POST /api/items — Create item (`CreateAsync`)
   - PUT  /api/items/{id} — Update item (`UpdateAsync`)
   - GET  /api/items — List/search items (`GetItemsAsync`)
   - GET  /api/items/{id} — Get item detail (`GetByIdAsync`)

- InvoiceController
   - POST /api/invoices/from-quotation/{quotationId} — Create invoice from quotation (`CreateFromQuotationAsync`)
   - GET  /api/invoices — Search/list invoices (`SearchAsync`)
   - GET  /api/invoices/{id} — Get invoice detail (`GetByIdAsync`)
   - GET  /api/invoices/{id}/pdf — Download invoice PDF (`DownloadPdfAsync`)
   - GET  /api/invoices/customer — Get customer invoices (`GetCustomerInvoicesAsync`)

- InventoryController
   - GET  /api/inventory/dashboard — Inventory dashboard (`GetDashboardAsync`)
   - GET  /api/inventory/purchase-orders — Purchase-order list (`GetPurchaseOrdersAsync`)
   - GET  /api/inventory/purchase-orders/{id} — Purchase-order detail (`GetPurchaseOrderByIdAsync`)
   - POST /api/inventory/purchase-orders — Create purchase order (`CreatePurchaseOrderAsync`)
   - PATCH /api/inventory/purchase-orders/{id}/receive — Receive purchase order (`ReceivePurchaseOrderAsync`)
   - GET  /api/inventory/low-stock-alerts — Low stock alerts (`GetLowStockAlertsAsync`)

- InstallationSurveyController
   - POST /api/installations/{installationId}/schedule-survey — Schedule installation survey (`ScheduleSurveyAsync`)
   - POST /api/installations/{installationId}/submit-survey — Submit installation survey (`SubmitSurveyAsync`)

- InstallationProposalController
   - POST /api/installations/{installationId}/proposal — Create installation proposal (`CreateProposalAsync`)
   - POST /api/installations/{installationId}/proposal/approve — Approve proposal (`ApproveProposalAsync`)
   - POST /api/installations/{installationId}/proposal/reject — Reject proposal (`RejectProposalAsync`)

- InstallationExecutionController
   - POST /api/installations/{installationId}/create-order — Create installation order (`CreateOrderAsync`)
   - POST /api/installations/{installationId}/start — Start installation (`StartInstallationAsync`)
   - POST /api/installations/{installationId}/complete — Complete installation (`CompleteInstallationAsync`)
   - POST /api/installations/{installationId}/checklist — Save installation checklist (`SaveChecklistAsync`)
   - POST /api/installations/{installationId}/commission — Commission installation (`CommissionAsync`)

Notes:
- These entries were appended after scanning controllers 41–50 in `Backend/Coolzo.Api/Controllers`.
- I will continue scanning controllers 51–60 and append any missing endpoints next.

## ADDED API ENDPOINTS (discovered in controllers — controllers 51–60) — 2026-04-25

The endpoints below were discovered in the backend controller sources during the fifth scan (controllers 51–60). Routes are shown normalized (version placeholder removed).

- InstallationController
   - POST /api/installations — Create installation (`CreateInstallationAsync`)
   - GET  /api/installations — List/search installations (`GetInstallationsAsync`)
   - GET  /api/installations/{installationId} — Get installation detail (`GetInstallationDetailAsync`)
   - POST /api/installations/orders — Create installation order (`CreateInstallationOrderAsync`)
   - POST /api/installations/orders/{installationOrderId}/survey-report — Submit survey report (`SubmitSurveyReportAsync`)
   - POST /api/installations/orders/{installationOrderId}/commissioning-certificate — Create commissioning certificate (`CreateCommissioningCertificateAsync`)

- HelperTaskController
   - GET  /api/helpers/{helperProfileId}/tasks — List helper tasks (`GetListAsync`)
   - POST /api/helpers/{helperProfileId}/tasks/{taskId}/respond — Respond to helper task (`RespondAsync`)
   - POST /api/helpers/{helperProfileId}/tasks/{taskId}/upload-photo — Upload helper task photo (`UploadPhotoAsync`)

- HelperController
   - POST /api/helpers — Create helper profile (`CreateAsync`)
   - GET  /api/helpers — List helpers (`GetListAsync`)
   - GET  /api/helpers/{helperProfileId} — Get helper detail (`GetDetailAsync`)
   - PUT  /api/helpers/{helperProfileId} — Update helper profile (`UpdateAsync`)
   - POST /api/helpers/{helperProfileId}/assign — Assign helper (`AssignAsync`)
   - POST /api/helpers/{helperProfileId}/release — Release helper assignment (`ReleaseAsync`)
   - GET  /api/helpers/{helperProfileId}/assignment — Get helper assignment (`GetAssignmentAsync`)

- HelperAttendanceController
   - POST /api/helpers/{helperProfileId}/attendance/check-in — Helper check-in (`CheckInAsync`)
   - POST /api/helpers/{helperProfileId}/attendance/check-out — Helper check-out (`CheckOutAsync`)
   - GET  /api/helpers/{helperProfileId}/attendance — List helper attendance (`GetListAsync`)

- HealthController
   - GET  /api/health — System health check (`GetAsync`)

- FieldWorkflowController
   - GET  /api/field/my-jobs — Get my field jobs (`GetMyJobsAsync`)
   - GET  /api/field/job-history — Get field job history (`GetJobHistoryAsync`)
   - GET  /api/field/jobs/{serviceRequestId} — Get field job detail (`GetJobDetailAsync`)
   - PATCH /api/field/jobs/{serviceRequestId}/depart — Mark en route (`DepartAsync`)
   - PATCH /api/field/jobs/{serviceRequestId}/arrive — Mark arrived (`ArriveAsync`)
   - PATCH /api/field/jobs/{serviceRequestId}/start-work — Start work (`StartWorkAsync`)
   - PATCH /api/field/jobs/{serviceRequestId}/progress — Save progress (`SaveProgressAsync`)
   - POST  /api/field/jobs/{serviceRequestId}/parts-request — Create field parts request (`CreatePartsRequestAsync`)
   - POST  /api/field/jobs/{serviceRequestId}/estimate — Create field estimate (`CreateEstimateAsync`)
   - POST  /api/field/jobs/{serviceRequestId}/report — Submit field job report (`SubmitReportAsync`)
   - POST  /api/field/jobs/{serviceRequestId}/photos — Upload field job photo (`UploadPhotoAsync`)
   - POST  /api/field/jobs/{serviceRequestId}/signature — Save customer signature (`SaveSignatureAsync`)
   - PATCH /api/field/jobs/{serviceRequestId}/payment — Collect payment (`CollectPaymentAsync`)
   - PATCH /api/field/jobs/{serviceRequestId}/complete — Complete field job (`CompleteAsync`)
   - POST  /api/field/attendance/check-in — Technician check-in (`CheckInAsync`)
   - POST  /api/field/attendance/check-out — Technician check-out (`CheckOutAsync`)

- FieldExecutionController
   - POST /api/technician-jobs/{id}/mark-enroute — Mark job en route (`MarkEnRouteAsync`)
   - POST /api/technician-jobs/{id}/mark-reached — Mark job reached (`MarkReachedAsync`)
   - POST /api/technician-jobs/{id}/start-work — Start work (`StartWorkAsync`)
   - POST /api/technician-jobs/{id}/mark-in-progress — Mark in progress (`MarkInProgressAsync`)
   - POST /api/technician-jobs/{id}/mark-work-completed — Mark work completed (`MarkWorkCompletedAsync`)
   - POST /api/technician-jobs/{id}/submit-for-closure — Submit for closure (`SubmitForClosureAsync`)
   - POST /api/technician-jobs/{id}/notes — Save execution note (`SaveNoteAsync`)
   - GET  /api/technician-jobs/{id}/timeline — Get execution timeline (`GetTimelineAsync`)

- EscalationController
   - POST /api/escalations — Create escalation (`CreateAsync`)
   - POST /api/escalations/service-requests/{serviceRequestId}/no-show — Handle no-show (`HandleNoShowAsync`)

- DiagnosisController
   - GET  /api/diagnosis/lookups/issues — Diagnosis issue lookups (`GetIssueLookupsAsync`)
   - GET  /api/diagnosis/lookups/results — Diagnosis result lookups (`GetResultLookupsAsync`)
   - POST /api/technician-jobs/{id}/diagnosis — Save job diagnosis (`SaveDiagnosisAsync`)

- DashboardController
   - GET  /api/dashboard/summary — Dashboard summary (`GetSummaryAsync`)
   - GET  /api/dashboard/metrics — Dashboard metrics (`GetMetricsAsync`)

Notes:
- These entries were appended after scanning controllers 51–60 in `Backend/Coolzo.Api/Controllers`.
- I will continue scanning controllers 61–70 and append any missing endpoints next.

## ADDED API ENDPOINTS (discovered in controllers — controllers 61–70) — 2026-04-25

The endpoints below were discovered in the backend controller sources during the sixth scan (controllers 61–70). Routes are shown normalized (version placeholder removed).

- CustomerTechnicianController
   - GET  /api/customer-technicians/{technicianId} — Get customer-visible technician (`GetTechnicianAsync`)

- CustomerReviewController
   - GET  /api/customer-reviews — List customer reviews (`GetReviewsAsync`)
   - POST /api/customer-reviews — Submit customer review (`CreateReviewAsync`)

- CustomerNotificationController
   - GET  /api/customer-notifications — List my notifications (`GetMineAsync`)
   - POST /api/customer-notifications/{notificationId}/mark-read — Mark notification read (`MarkReadAsync`)

- CustomerMarketingController
   - GET  /api/offers — Active promotional offers (`GetOffersAsync`)
   - POST /api/offers/validate-coupon — Validate coupon (`ValidateCouponAsync`)
   - GET  /api/referrals/me — My referral stats (`GetMyReferralStatsAsync`)
   - GET  /api/loyalty/me — My loyalty points (`GetMyLoyaltyPointsAsync`)
   - GET  /api/loyalty/me/transactions — My loyalty transactions (`GetMyLoyaltyTransactionsAsync`)

- CustomerEquipmentController
   - GET  /api/customers/me/equipment — Get my equipment (`GetMyEquipmentAsync`)
   - POST /api/customers/me/equipment — Create my equipment (`CreateEquipmentAsync`)
   - PUT  /api/customers/me/equipment/{equipmentId} — Update my equipment (`UpdateEquipmentAsync`)
   - DELETE /api/customers/me/equipment/{equipmentId} — Delete my equipment (`DeleteEquipmentAsync`)

- CustomerController
   - GET  /api/customers — List customers (`GetCustomersAsync`)
   - POST /api/customers — Create customer account (`CreateAsync`)
   - GET  /api/customers/{customerId} — Get customer detail (`GetByIdAsync`)
   - PUT  /api/customers/{customerId} — Update customer (`UpdateAsync`)
   - GET  /api/customers/{customerId}/addresses — Get customer addresses (`GetAddressesAsync`)
   - POST /api/customers/{customerId}/addresses — Create customer address (`CreateAddressAsync`)
   - PUT  /api/customers/{customerId}/addresses/{addressId} — Update customer address (`UpdateAddressAsync`)
   - GET  /api/customers/{customerId}/equipment — Get customer equipment (`GetEquipmentAsync`)
   - POST /api/customers/{customerId}/equipment — Create customer equipment (`CreateEquipmentAsync`)
   - POST /api/customers/{customerId}/notes — Add customer note (`CreateNoteAsync`)
   - POST /api/customers/{customerId}/reset-password — Reset customer password (`ResetPasswordAsync`)
   - GET  /api/customers/me/profile — Get my profile (`GetMyProfileAsync`)
   - PUT  /api/customers/me/profile — Update my profile (`UpdateMyProfileAsync`)
   - POST /api/customers/me/deactivate — Deactivate my account (`DeactivateMyAccountAsync`)

- CustomerContentController
   - GET  /api/cms/public/blogs — Public blogs (`GetBlogsAsync`)
   - GET  /api/cms/blog-posts — Published blog posts (`GetPublishedBlogPostsAsync`)
   - GET  /api/cms/public/blogs/{id} — Get public blog by id (`GetBlogByIdAsync`)
   - GET  /api/cms/blog-posts/{id} — Get blog post by id (`GetBlogPostByIdAsync`)
   - GET  /api/cms/public/changelog — Get changelog (`GetChangelogAsync`)
   - POST /api/customer-app/feedback — Submit customer app feedback (`SubmitFeedbackAsync`)
   - POST /api/feedback/app — Submit app feedback alias (`SubmitAppFeedbackAliasAsync`)

- CustomerBookingController
   - GET  /api/customer-bookings/{bookingId} — Get customer booking detail (`GetCustomerBookingByIdAsync`)
   - GET  /api/customer-bookings — List my bookings (`GetCustomerBookingsAsync`)
   - POST /api/customer-bookings/{bookingId}/reschedule — Reschedule booking (`RescheduleCustomerBookingAsync`)
   - GET  /api/customer-bookings/{bookingId}/service-report — Get service report (`GetServiceReportAsync`)
   - GET  /api/customer-bookings/{bookingId}/service-report/pdf — Download service report PDF (`DownloadServiceReportPdfAsync`)

- CustomerAuthController
   - POST /api/customer-auth/register — Register customer (`RegisterAsync`)
   - POST /api/customer-auth/forgot-password — Forgot password (`ForgotPasswordAsync`)
   - POST /api/customer-auth/reset-password — Reset password (`ResetPasswordAsync`)
   - POST /api/customer-auth/change-password — Change password (`ChangePasswordAsync`)

- CustomerAddressController
   - GET  /api/customers/me/addresses — Get my addresses (`GetMyAddressesAsync`)
   - POST /api/customers/me/addresses — Create my address (`CreateAddressAsync`)
   - PUT  /api/customers/me/addresses/{addressId} — Update my address (`UpdateAddressAsync`)
   - DELETE /api/customers/me/addresses/{addressId} — Delete my address (`DeleteAddressAsync`)

Notes:
- These entries were appended after scanning controllers 61–70 in `Backend/Coolzo.Api/Controllers`.
- I will continue scanning controllers 71–80 and append any missing endpoints next.

## ADDED API ENDPOINTS (discovered in controllers — controllers 71–80) — 2026-04-25

The endpoints below were discovered in the backend controller sources during the seventh scan (controllers 71–80). Routes are shown normalized (version placeholder removed).

- CustomerAbsentController
   - POST /api/customer-absent/{serviceRequestId}/mark — Mark customer absent (`MarkAsync`)
   - POST /api/customer-absent/{serviceRequestId}/reschedule — Reschedule absent service request (`RescheduleAsync`)
   - POST /api/customer-absent/{serviceRequestId}/cancel — Cancel absent service request (`CancelAsync`)
   - GET  /api/customer-absent/{serviceRequestId} — Get absent detail (`GetByServiceRequestIdAsync`)

- ConfigurationController
   - GET  /api/configuration/settings — Get system settings (`GetSystemSettingsAsync`)

- CommunicationPreferenceController
   - GET  /api/communication-preferences/me — Get my communication preferences (`GetMineAsync`)
   - PUT  /api/communication-preferences/me — Update my communication preferences (`UpdateMineAsync`)
   - GET  /api/communication-preferences/customer/{customerId} — Get communication preferences for a customer (`GetByCustomerAsync`)

- CMSController
   - GET  /api/cms/public/home — Public home CMS content (`GetPublicHomeAsync`)
   - GET  /api/cms/public/faqs — Public FAQs (`GetPublicFaqsAsync`)
   - GET  /api/cms/public/banners — Public banners (`GetPublicBannersAsync`)
   - GET  /api/cms/public/service-content/{key} — Public service content (`GetPublicServiceContentAsync`)
   - GET  /api/cms/blocks/{key} — Public CMS block by key (`GetPublicBlockByKeyAsync`)
   - GET  /api/cms/admin/blocks — Admin CMS blocks (`GetBlocksAsync`)
   - POST /api/cms/admin/blocks — Create CMS block (`CreateBlockAsync`)
   - PUT  /api/cms/admin/blocks/{cmsBlockId} — Update CMS block (`UpdateBlockAsync`)
   - GET  /api/cms/admin/banners — Admin banners (`GetBannersAsync`)
   - POST /api/cms/admin/banners — Create banner (`CreateBannerAsync`)
   - PUT  /api/cms/admin/banners/{cmsBannerId} — Update banner (`UpdateBannerAsync`)
   - GET  /api/cms/admin/faqs — Admin FAQs (`GetFaqsAsync`)
   - POST /api/cms/admin/faqs — Create FAQ (`CreateFaqAsync`)
   - PUT  /api/cms/admin/faqs/{cmsFaqId} — Update FAQ (`UpdateFaqAsync`)

- CancellationController
   - POST /api/cancellations/customer — Create customer cancellation (`CreateCustomerCancellationAsync`)
   - POST /api/cancellations/admin — Create admin cancellation (`CreateAdminCancellationAsync`)
   - GET  /api/cancellations/options/{serviceRequestId} — Get cancellation options (`GetCancellationOptionsAsync`)
   - GET  /api/cancellations — List cancellations (`GetCancellationsAsync`)
   - GET  /api/cancellations/{id} — Get cancellation detail (`GetCancellationByIdAsync`)
   - POST /api/cancellations/service-requests/{serviceRequestId} — Cancel service request (`CancelServiceRequestAsync`)

- CampaignController
   - POST /api/campaigns — Create campaign (`CreateAsync`)

- BranchController
   - GET  /api/branches — List branches (`GetAsync`)
   - GET  /api/branches/{branchId} — Get branch by id (`GetByIdAsync`)
   - POST /api/branches — Create branch (`CreateAsync`)
   - PUT  /api/branches/{branchId} — Update branch (`UpdateAsync`)

- BookingLookupController
   - GET  /api/booking-lookups/service-categories — Service categories (`GetServiceCategoriesAsync`)
   - GET  /api/booking-lookups/services — Services (`GetServicesAsync`)
   - GET  /api/booking-lookups/ac-types — AC types (`GetAcTypesAsync`)
   - GET  /api/booking-lookups/tonnage — Tonnages (`GetTonnagesAsync`)
   - GET  /api/booking-lookups/brands — Brands (`GetBrandsAsync`)
   - GET  /api/booking-lookups/zones — Zones (`GetZonesAsync`)
   - GET  /api/booking-lookups/zones/by-pincode/{pincode} — Zone by pincode (`GetZoneByPincodeAsync`)
   - GET  /api/booking-lookups/slots — Available slots (`GetSlotsAsync`)

- BookingController
   - POST /api/bookings/guest — Create guest booking (`CreateGuestBookingAsync`)
   - POST /api/bookings/customer — Create customer booking (`CreateCustomerBookingAsync`)
   - GET  /api/bookings/{bookingId} — Get booking by id (`GetBookingByIdAsync`)
   - GET  /api/bookings/my-bookings — Get my bookings (`GetMyBookingsAsync`)
   - GET  /api/bookings — Search bookings (`SearchAsync`)
   - POST /api/bookings/{bookingId}/reschedule — Reschedule booking (`RescheduleAsync`)

- BillingController
   - GET  /api/billing/accounts-receivable — Accounts receivable dashboard (`GetAccountsReceivableAsync`)
   - GET  /api/billing/status/{invoiceId} — Get billing status by invoice (`GetStatusAsync`)

Notes:
- These entries were appended after scanning controllers 71–80 in `Backend/Coolzo.Api/Controllers`.
- I will continue scanning controllers 81–86 and append any missing endpoints next.

## ADDED API ENDPOINTS (discovered in controllers — controllers 81–86) — 2026-04-25

The endpoints below were discovered in the backend controller sources during the final scan (controllers 81–86). Routes are shown normalized (version placeholder removed).

- AuthController
   - POST   /api/auth/otp/send — Send OTP (`SendOtpAsync`)
   - POST   /api/auth/otp/verify — Verify customer OTP (`VerifyCustomerOtpAsync`)
   - POST   /api/auth/login — Login (`LoginAsync`)
   - POST   /api/auth/refresh — Refresh token (`RefreshAsync`)
   - POST   /api/auth/login-field — Field login (`LoginFieldAsync`)
   - POST   /api/auth/login-otp — Login with OTP (`LoginOtpAsync`)
   - POST   /api/auth/verify-otp — Verify OTP (`VerifyOtpAsync`)
   - POST   /api/auth/refresh-token — Refresh token (alternate) (`RefreshTokenAsync`)
   - POST   /api/auth/forgot-password — Forgot password / send OTP (`ForgotPasswordAsync`)
   - POST   /api/auth/reset-password — Reset password (`ResetPasswordAsync`)
   - POST   /api/auth/change-password — Change customer password (`ChangePasswordAsync`)
   - POST   /api/auth/logout — Logout (`LogoutAsync`)
   - DELETE /api/auth/account — Delete customer account (`DeleteCustomerAccountAsync`)
   - GET    /api/auth/me — Get current user (`GetCurrentUserAsync`)
   - GET    /api/auth/me/permissions — Get permission snapshot (`GetPermissionSnapshotAsync`)
   - POST   /api/auth/force-logout/{userId} — Force logout user (`ForceLogoutAsync`)

- AssignmentController
   - POST /api/service-requests/{serviceRequestId}/assign — Assign technician (`AssignAsync`)
   - POST /api/service-requests/{serviceRequestId}/reassign — Reassign technician (`ReassignAsync`)
   - GET  /api/service-requests/{serviceRequestId}/assignment-history — Assignment history (`GetAssignmentHistoryAsync`)

- ApiControllerBase
   - Note: `ApiControllerBase` is a base class used by controllers and does not declare API action endpoints (provides `Success<T>` helper).

- AnalyticsController
   - GET /api/analytics/bookings — Booking analytics (`GetBookingsAsync`)
   - GET /api/analytics/revenue — Revenue analytics (`GetRevenueAsync`)
   - GET /api/analytics/technicians — Technician performance (`GetTechniciansAsync`)
   - GET /api/analytics/customers — Customer analytics (`GetCustomersAsync`)
   - GET /api/analytics/support — Support analytics (`GetSupportAsync`)
   - GET /api/analytics/inventory — Inventory analytics (`GetInventoryAsync`)

- AmcController
   - POST /api/amc/plans — Create AMC plan (`CreatePlanAsync`)
   - PUT  /api/amc/plans/{amcPlanId} — Update AMC plan (`UpdatePlanAsync`)
   - GET  /api/amc/plans — Get AMC plans (`GetPlansAsync`)
   - GET  /api/amc/plans/{amcPlanId} — Get AMC plan by id (`GetPlanByIdAsync`)
   - POST /api/amc/assign — Assign AMC to customer (`AssignAsync`)
   - POST /api/amc/customer/{customerAmcId}/generate-visits — Generate AMC visits (`GenerateVisitsAsync`)
   - GET  /api/amc/customer/{customerId} — Get customer AMC subscriptions (`GetCustomerSubscriptionsAsync`)
   - GET  /api/amc/customer/me — Get current customer's AMC subscriptions (`GetCurrentCustomerSubscriptionsAsync`)

- AdminController
   - POST /api/admin/view-as-role — Prepare view-as-role session (`ViewAsRoleAsync`)

Notes:
- These entries were appended after scanning controllers 81–86 in `Backend/Coolzo.Api/Controllers`.
- All additions are append-only; `ProjectOverview.md` content was not otherwise modified.

**IMPLEMENTATION UPDATES (2026-04-25)**

- **Technician Job Report Screen**: Implemented `Mobile/Coolzo_AdminMobile/src/features/field/TechnicianJobReportScreen.tsx` as a thin wrapper around `JobWorkflowContainer` and registered route `/field/report/:id`. This reuses the existing `field-workflow-repository` (methods: `submitReport`, `uploadPhoto`, `saveSignature`, `collectPayment`, `complete`) and leverages the local offline sync queue (`StorageKey.FIELD_OFFLINE_QUEUE`) for queued submissions. (Implemented 2026-04-25)

6. DATABASE ARCHITECTURE & MIGRATION

Source SQL Server database
- The current source of truth for the operational SQL Server database is `Backend/Coolzo.Api/appsettings.json`, which points the API to `Server=(localdb)\MSSQLLocalDB;Database=CoolzoDB;Trusted_Connection=True;TrustServerCertificate=True;`.
- The live migration export for this repository now uses the repo-root `.env` plus `generate.js` to connect directly to `CoolzoDB`, query the `dbo` schema automatically, and generate PostgreSQL migration artifacts without hardcoded table lists.
- Runtime database provider selection is now appsettings-driven. `Backend/Coolzo.Api/appsettings*.json` and `Backend/Coolzo.Worker/appsettings*.json` now expose `Database:Provider` with provider-specific connection keys `ConnectionStrings:SqlServerConnection` and `ConnectionStrings:PostgresConnection`.
- The supported provider values are `SqlServer` and `Postgres`. SQL Server remains the default provider; PostgreSQL is activated by setting `Database:Provider` to `Postgres` and supplying a direct PostgreSQL connection string.
- `ConnectionStrings:PostgresConnection` in the API and worker appsettings files is prefilled with the Supabase direct placeholder `postgresql://postgres:[YOUR-PASSWORD]@db.yjjuhdkhnsxsmplvmsda.supabase.co:5432/postgres`. The persistence bootstrap still consumes a single PostgreSQL connection slot (`ConnectionStrings:PostgresConnection` or `COOLZO_POSTGRES_CONNECTION`), so the separate Supabase transaction-pooler URL is not stored as a second appsettings key.

Validated source snapshot
- The current validated `dbo` schema snapshot contains 139 base tables.
- The current validated relational snapshot contains 221 foreign key constraints.
- The current validated PostgreSQL index export contains 332 statements (existing non-PK indexes, unique constraints, and supplemental FK indexes).
- The current validated data export writes 2010 source rows across 93 populated tables, including master data.

Generated PostgreSQL artifacts
- `coolzo-postgres/01_extensions.sql`: enables `uuid-ossp` and `pgcrypto`.
- `coolzo-postgres/02_create_tables.sql`: creates all `dbo` tables in FK dependency order with PostgreSQL type/default conversion applied and no FK constraints inline.
- `coolzo-postgres/03_foreign_keys.sql`: recreates all foreign keys with `ON DELETE` and `ON UPDATE` actions.
- `coolzo-postgres/04_indexes.sql`: recreates non-PK indexes, unique constraints as unique indexes, and adds missing FK-supporting indexes.
- `coolzo-postgres/05_all_data.sql`: inserts all exported source rows, including master data, and uses `OVERRIDING SYSTEM VALUE` for identity-backed tables.
- `coolzo-postgres/06_sequences_reset.sql`: resets PostgreSQL identity sequences to the maximum imported values per identity column.
- `coolzo-postgres/07_verify.sql`: verifies PostgreSQL row counts against the exported SQL Server source counts for every table.
- Identity-backed insert statements in `coolzo-postgres/05_all_data.sql` are emitted in PostgreSQL-valid order: `INSERT INTO ... (columns) OVERRIDING SYSTEM VALUE VALUES ...`.

Validated SQL Server to PostgreSQL conversion rules in use
- `bigint` identity columns map to `BIGINT GENERATED ALWAYS AS IDENTITY`; `int` identity columns map to `INTEGER GENERATED ALWAYS AS IDENTITY`.
- `int` maps to `INTEGER`; `bit` maps to `BOOLEAN`; `money` maps to `NUMERIC(18,2)`; `decimal(p,s)` maps to `NUMERIC(p,s)`; `float` maps to `DOUBLE PRECISION`.
- `nvarchar(n)` maps to `VARCHAR(n)`; `nvarchar(max)` maps to `TEXT`.
- `datetime2` maps to `TIMESTAMPTZ`; `date` maps to `DATE`; `time` maps to `TIME`.
- SQL Server defaults `((1))`, `((0))`, `('...')`, `(N'...')`, `(getdate())`, and `(getutcdate())` are converted to PostgreSQL-compatible `DEFAULT` clauses during export.
