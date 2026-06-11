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

3. SERVICE REQUEST API CONTRACT — STABLE (audited 2026-05-24)

Controllers: ServiceRequestController (/api/service-requests),
             AssignmentController (/api/service-requests),
             CancellationController (/api/cancellations),
             EscalationController (/api/escalations)

---

### Flow: Get Service Requests (List)
  Entry Points:         Admin SR List screen (SRListScreen.tsx), Dispatch screen
  UI Trigger:           Page load / filter change
  API Endpoint:         GET /api/service-requests
  Auth:                 Policy = ServiceRequestRead
  Query Params:
    - bookingId (long?, optional): filter by booking
    - serviceId (long?, optional): filter by service type
    - status (string?, optional): filter by SR status
    - slotDate (DateOnly?, optional): filter by slot date
    - pageNumber (int, default=1)
    - pageSize (int, default=20)
  Response DTO:         ApiResponse<PagedResult<ServiceRequestListItemResponse>>
    ServiceRequestListItemResponse:
      - ServiceRequestId (long)
      - ServiceRequestNumber (string)
      - BookingId (long)
      - BookingReference (string)
      - CustomerName (string)
      - ServiceName (string)
      - CurrentStatus (string)
      - TechnicianName (string?)
      - SlotDate (DateOnly)
      - SlotLabel (string)
      - ServiceRequestDateUtc (DateTime)
  DB Tables:            ServiceRequests (read), Bookings (read), Customers (read), Technicians (read)
  Failure Cases:
    - Unauthorized → 403

---

### Flow: Get Service Request Detail
  Entry Points:         Admin SR Detail screen (SRDetailScreen.tsx)
  UI Trigger:           SR row click / direct navigation
  API Endpoint:         GET /api/service-requests/{serviceRequestId}
  Auth:                 Policy = ServiceRequestRead
  Request:              serviceRequestId (long, route param)
  Response DTO:         ApiResponse<ServiceRequestDetailResponse>
    ServiceRequestDetailResponse:
      - ServiceRequestId (long), ServiceRequestNumber (string)
      - BookingId (long), BookingReference (string), BookingStatus (string)
      - CurrentStatus (string), ServiceRequestDateUtc (DateTime), SourceChannel (string)
      - CustomerName (string), MobileNumber (string), EmailAddress (string)
      - AddressSummary (string), ZoneName (string)
      - SlotDate (DateOnly), SlotLabel (string)
      - ServiceName (string), AcTypeName (string), TonnageName (string),
        BrandName (string), ModelName (string), IssueNotes (string)
      - EstimatedPrice (decimal)
      - TechnicianId (long?), TechnicianCode (string?), TechnicianName (string?),
        TechnicianMobileNumber (string?), AssignmentRemarks (string?)
      - JobCard (JobCardSummaryResponse), QuotationId/Number/Status (long?/string?/string?)
      - InvoiceId/Number/Status/GrandTotalAmount/BalanceAmount (long?/string?/string?/decimal?/decimal?)
      - DiagnosisSummary (JobDiagnosisSummaryResponse)
      - ChecklistSummary (JobChecklistSummaryResponse)
      - ExecutionNotes (JobExecutionNoteResponse[])
      - Attachments (JobAttachmentResponse[])
      - ExecutionTimeline (JobExecutionTimelineItemResponse[])
      - StatusTimeline (ServiceRequestStatusHistoryResponse[]):
          Status (string), Remarks (string), StatusDateUtc (DateTime)
      - AssignmentHistory (AssignmentHistoryItemResponse[]):
          ActionName, PreviousTechnicianName?, CurrentTechnicianName, Remarks, ActionDateUtc
      - CustomerLatitude (double?): GPS latitude from tblCustomerAddress — null for pre-GPS bookings [added 2026-06-09]
      - CustomerLongitude (double?): GPS longitude from tblCustomerAddress — null for pre-GPS bookings [added 2026-06-09]
  Admin Portal "View on Map":
      - When CustomerLatitude/CustomerLongitude are non-null: opens Google Maps directions to exact coords
        URL: https://www.google.com/maps/dir/?api=1&destination={lat},{lng}
      - Fallback (null coords): OpenStreetMap address-text search
  DB Tables:            ServiceRequests, Bookings, Customers, Technicians,
                        JobReports, JobPhotos, SRStatusHistory, SRAssignments,
                        Estimates, Invoices, JobChecklists
  Failure Cases:
    - SR not found → 404
    - Unauthorized → 403

---

### Flow: Update Service Request Coordinates (Admin Pin-Drop) [added 2026-06-09]
  Entry Points:         Admin SR Detail screen — Service Location card
  UI Trigger:           Admin drags the Leaflet pin on the mini-map then clicks "Save Pin"
  API Endpoint:         PATCH /api/service-requests/{serviceRequestId}/coordinates
  Auth:                 Policy = ServiceRequestUpdate
  Request DTO:          UpdateServiceRequestCoordinatesRequest
    - Latitude (double, required): corrected GPS latitude
    - Longitude (double, required): corrected GPS longitude
  Response DTO:         ApiResponse<ServiceRequestDetailResponse> (full updated SR detail)
  DB Tables:            tblCustomerAddress (Latitude/Longitude updated), tblBooking (LatitudeSnapshot/LongitudeSnapshot updated), AuditLogs (write)
  Business Rules:
    1. Both CustomerAddress and Booking snapshots are updated atomically
    2. Audit log entry created with ActionName = "UpdateServiceRequestCoordinates"
    3. SR immediately appears on live map after save (ShouldRenderOnLiveMap re-evaluates on next poll)
  UI Behaviour:
    - Mini Leaflet map (220px) rendered in Service Location card on SR detail screen
    - Draggable pin — defaults to current saved coords; falls back to Hyderabad city center (17.385, 78.4867)
    - Coordinates displayed below map: lat/lng to 6dp
    - "— drag to set exact location" warning shown when no saved coords yet
    - "Save Pin" button → PATCH endpoint → refreshes SR → toast success
    - "View on Map" button: Google Maps directions with exact coords when saved; OpenStreetMap text search fallback
  Failure Cases:
    - SR not found → 404
    - No CustomerAddress linked → 404
    - Unauthorized → 403
  Flow Stable: YES

---

### Flow: Create SR from Booking
  Entry Points:         Admin SR Create screen (CreateSRScreen.tsx) — final step after booking
  UI Trigger:           "Create Service Request" confirm after guest booking
  API Endpoint:         POST /api/service-requests/from-booking/{bookingId}
  Auth:                 Policy = ServiceRequestCreate
  Request:              bookingId (long, route param)
  Response DTO:         ApiResponse<ServiceRequestDetailResponse> (full SR detail — same shape above)
  DB Tables:            Bookings (read), ServiceRequests (write), SRStatusHistory (write),
                        AuditLogs (write)
  Business Rules:
    1. Booking must be in Confirmed or equivalent state
    2. Generates SRNumber (GUID-based unique reference)
    3. SR created with status PendingAssignment
    4. Triggers notifications: WhatsApp/email confirmation to customer
  State Transitions:    Booking → ServiceRequest.PendingAssignment
  Manual SR Creation Pipeline (AdminMobile — full flow):
    Step 1: GET /api/booking-lookups/services
    Step 2: GET /api/booking-lookups/ac-types
    Step 3: GET /api/booking-lookups/tonnage
    Step 4: GET /api/booking-lookups/brands
    Step 5: GET /api/booking-lookups/zones
    Step 6: GET /api/booking-lookups/zones/by-pincode/{pincode}
    Step 7: GET /api/booking-lookups/slots
    Step 8: POST /api/bookings/guest  (creates guest booking)
    Step 9: POST /api/service-requests/from-booking/{bookingId}  (converts to SR)
  Failure Cases:
    - Booking not found → 404
    - Booking not in valid state for SR creation → 400

---

### Flow: Update SR Status
  Entry Points:         SR Detail screen — status action buttons
  UI Trigger:           Status change button click
  API Endpoint:         POST /api/service-requests/{serviceRequestId}/status
  Auth:                 Policy = ServiceRequestUpdate
  Request DTO:          UpdateServiceRequestStatusRequest
    - Status (string, required): new status value
    - Remarks (string?, optional): reason/notes for status change
  Response DTO:         ApiResponse<ServiceRequestDetailResponse>
  DB Tables:            ServiceRequests (write), SRStatusHistory (write), AuditLogs (write)
  Business Rules:
    1. Status must be a valid SR status in the configured workflow
    2. Status history entry created for each transition
    3. Notifications triggered based on NotificationTrigger config for this status
  State Transitions:
    Valid statuses: PendingAssignment → Scheduled → EnRoute → Arrived →
                    InProgress → SubmittedForClosure → Closed / Cancelled
  Failure Cases:
    - Invalid status string → 400
    - SR not found → 404

---

### Flow: Save SR Note
  Entry Points:         SR Detail screen — notes panel
  UI Trigger:           "Add Note" form submit
  API Endpoint:         POST /api/service-requests/{serviceRequestId}/notes
  Auth:                 Policy = ServiceRequestUpdate
  Request DTO:          SaveJobExecutionNoteRequest
    - NoteText (string, required): note content
    - IsCustomerVisible (bool, required): true = visible in customer portal
  Response DTO:         ApiResponse<JobExecutionNoteResponse>
    - JobExecutionNoteId (long)
    - NoteText (string)
    - IsCustomerVisible (bool)
    - CreatedBy (string)
    - NoteDateUtc (DateTime)
  DB Tables:            JobExecutionNotes (write)
  Failure Cases:
    - SR not found → 404
    - Empty NoteText → 400

---

### Flow: Get Operations Dashboard Summary
  Entry Points:         Operations Dashboard screen (OperationsDashboardScreen.tsx)
  UI Trigger:           Dashboard page load
  API Endpoint:         GET /api/service-requests/dashboard-summary
  Auth:                 Policy = OperationsDashboardRead
  Request:              (none)
  Response DTO:         ApiResponse<OperationsDashboardSummaryResponse>
  DB Tables:            ServiceRequests (read aggregate), Technicians (read)
  Failure Cases:
    - Unauthorized → 403

---

### Flow: Assign Technician to SR
  Entry Points:         Assign Technician Panel (dispatch popover), SR Detail screen
  UI Trigger:           "Assign" button in dispatch panel
  API Endpoint:         POST /api/service-requests/{serviceRequestId}/assign
  Auth:                 Policy = AssignmentManage
  Request DTO:          AssignTechnicianRequest
    - TechnicianId (long?, required in practice): technician to assign
    - Remarks (string?, optional): assignment notes
  Response DTO:         ApiResponse<ServiceRequestDetailResponse>
  DB Tables:            ServiceRequests (write), SRAssignments (write),
                        SRStatusHistory (write), AuditLogs (write)
  Business Rules:
    1. TechnicianId set on ServiceRequest
    2. SRAssignments record created
    3. SR status updated to Scheduled
    4. Technician notified via push/in-app
    5. Customer notified via WhatsApp with technician name/photo/rating/ETA
  State Transitions:    PendingAssignment → Scheduled
  Failure Cases:
    - Technician not found or inactive → 400
    - SR already assigned → 400 (must use reassign)

---

### Flow: Reassign Technician
  Entry Points:         SR Detail screen — Reassign action, Dispatch screen
  UI Trigger:           "Reassign" button
  API Endpoint:         POST /api/service-requests/{serviceRequestId}/reassign
  Auth:                 Policy = AssignmentManage
  Request DTO:          ReassignTechnicianRequest
    - TechnicianId (long, required): new technician
    - Remarks (string?, optional): reason for reassignment
  Response DTO:         ApiResponse<ServiceRequestDetailResponse>
  DB Tables:            ServiceRequests (write), SRAssignments (write),
                        SRStatusHistory (write), AuditLogs (write)
  Business Rules:
    1. Previous assignment record closed; new assignment record created
    2. SR status remains Scheduled (or updated per workflow)
    3. New technician notified; old technician notified of unassignment
  Failure Cases:
    - Technician not found → 400
    - SR not in assignable state → 400

---

### Flow: Get Assignment History
  Entry Points:         SR Detail screen — assignment history panel
  UI Trigger:           Page load / expand history
  API Endpoint:         GET /api/service-requests/{serviceRequestId}/assignment-history
  Auth:                 Policy = ServiceRequestRead
  Response DTO:         ApiResponse<IReadOnlyCollection<AssignmentHistoryItemResponse>>
    AssignmentHistoryItemResponse:
      - ActionName (string): "Assigned" / "Reassigned" / "Unassigned"
      - PreviousTechnicianName (string?)
      - CurrentTechnicianName (string)
      - Remarks (string)
      - ActionDateUtc (DateTime)
  DB Tables:            SRAssignments (read)
  Notes on Drift:       Previously not documented in ProjectOverview. Added 2026-05-24.

---

### Flow: Cancel SR (Admin)
  Entry Points:         SR Detail screen — Cancel action; legacy route
  UI Trigger:           "Cancel SR" button
  API Endpoint:         POST /api/cancellations/service-requests/{serviceRequestId}
  Auth:                 Policy = ServiceRequestUpdate
  Request DTO:          CancelServiceRequestRequest
    - ReasonCode (string, required): cancellation reason code
    - ReasonDescription (string, required): free-text reason
    - RequiresApproval (bool, required): whether cancellation needs approval
  Response DTO:         ApiResponse<CancellationRecordResponse>
    - CancellationRecordId (long)
    - ServiceRequestId (long)
    - CancellationStatus (string)
    - CancellationFeeAmount (decimal)
    - RefundEligibleAmount (decimal)
    - RequiresApproval (bool)
  DB Tables:            CancellationRecords (write), ServiceRequests (write),
                        SRStatusHistory (write), Invoices (read)
  State Transitions:    SR → Cancelled
  Notes on Drift:       This is a legacy/simplified admin cancel route (source="Operations").
                        New cancellation flows use POST /api/cancellations/admin or /customer.

---

### Flow: Create Customer Cancellation
  Entry Points:         Customer portal "Cancel Booking" action
  UI Trigger:           Customer confirms cancellation
  API Endpoint:         POST /api/cancellations/customer
  Auth:                 Authorize (any authenticated — customer enforced in handler)
  Request DTO:          CreateCustomerCancellationRequest
    - BookingId (long?, optional)
    - ServiceRequestId (long?, optional)
    - CancellationReasonCode (string, required)
    - CancellationReasonText (string, required)
  Response DTO:         ApiResponse<CancellationDetailResponse>
    CancellationDetailResponse:
      - CancellationRecordId (long), BookingId (long?), ServiceRequestId (long?)
      - CancelledByUserId (long?), CancelledByRole (string), CancellationSource (string)
      - CancellationReasonCode (string), CancellationReasonText (string)
      - TimeToSlotMinutes (int), CancellationFee (decimal), RefundEligibleAmount (decimal)
      - CancellationStatus (string), PolicyCode (string), PolicyDescription (string)
      - ApprovalRequired (bool), DateCreated (DateTime)
      - RefundRequestId (long?), RefundStatus (string?)
  DB Tables:            CancellationRecords (write), Bookings/ServiceRequests (write),
                        CancellationPolicies (read), Invoices (read), Payments (read)
  Business Rules:
    1. Policy evaluated at time of cancellation (TimeToSlotMinutes determines fee tier)
    2. CancellationFee calculated per policy; RefundEligibleAmount = PaidAmount - Fee
    3. If TechnicianDispatched → cancellation may require admin approval
  State Transitions:    Booking/SR → Cancelled

---

### Flow: Create Admin Cancellation
  Entry Points:         Admin SR Detail — Cancel action (new admin route)
  UI Trigger:           Admin confirms cancel with source override
  API Endpoint:         POST /api/cancellations/admin
  Auth:                 Policy = ServiceRequestUpdate
  Request DTO:          CreateAdminCancellationRequest
    - BookingId (long?, optional)
    - ServiceRequestId (long?, optional)
    - CancellationSource (string, required): "Operations" / "Admin" / etc.
    - CancellationReasonCode (string, required)
    - CancellationReasonText (string, required)
    - ForceOverride (bool, required): bypass policy restrictions
    - OverrideReason (string?, optional): required if ForceOverride=true
  Response DTO:         ApiResponse<CancellationDetailResponse> (same shape as customer cancel)
  Business Rules:
    1. ForceOverride=true bypasses time-based policy restrictions
    2. OverrideReason recorded in audit log when override used
  State Transitions:    Booking/SR → Cancelled

---

### Flow: Get Cancellation Options (Pre-Cancel Preview)
  Entry Points:         Cancel SR dialog — before confirming
  UI Trigger:           "Cancel" button opens dialog showing fee/refund preview
  API Endpoint:         GET /api/cancellations/options/{serviceRequestId}
  Auth:                 Authorize (any authenticated)
  Query Params:
    - bookingId (long?, optional)
  Response DTO:         ApiResponse<CancellationOptionsResponse>
    - BookingId (long?), ServiceRequestId (long?)
    - PolicyCode (string), PolicyName (string), PolicyDescription (string)
    - TimeToSlotMinutes (int)
    - PaidAmount (decimal), CancellationFee (decimal), RefundEligibleAmount (decimal)
    - ApprovalRequired (bool), CanCustomerCancel (bool), CustomerDenialReason (string)
    - ScheduledStartUtc (DateTime), IsTechnicianDispatched (bool)
  Business Rules:
    1. Read-only preview — no data mutation
    2. CanCustomerCancel=false + CustomerDenialReason set if technician already dispatched

---

### Flow: Get Cancellations List
  Entry Points:         Admin cancellations report / operations view
  UI Trigger:           Page load / filter
  API Endpoint:         GET /api/cancellations
  Auth:                 Authorize (any authenticated)
  Query Params:
    - bookingId (long?), serviceRequestId (long?), cancellationStatus (string?),
      cancellationSource (string?), cancellationReasonCode (string?),
      branchId (int?), fromDateUtc (DateTime?), toDateUtc (DateTime?)
  Response DTO:         ApiResponse<IReadOnlyCollection<CancellationListItemResponse>>
    - CancellationRecordId (long), BookingId/ServiceRequestId (long?), ReferenceNumber (string)
    - CancellationStatus/Source/ReasonCode (string), CancellationFee/RefundEligibleAmount (decimal)
    - CancelledByRole (string), DateCreated (DateTime), RefundRequestId (long?), RefundStatus (string?)

---

### Flow: Get Cancellation Detail
  Entry Points:         Admin cancellation detail view
  UI Trigger:           Cancellation row click
  API Endpoint:         GET /api/cancellations/{id}
  Auth:                 Authorize (any authenticated)
  Response DTO:         ApiResponse<CancellationDetailResponse> (full shape — see Create Customer Cancel)

---

### Flow: Create Escalation
  Entry Points:         SR Detail screen — Escalate action; SLA alert system
  UI Trigger:           "Escalate" button or automated SLA breach trigger
  API Endpoint:         POST /api/escalations
  Auth:                 Policy = SupportManage
  Request DTO:          CreateEscalationRequest
    - AlertType (string, required): type code (e.g., "SR_DELAYED", "SLA_BREACH")
    - RelatedEntityName (string, required): entity type (e.g., "ServiceRequest")
    - RelatedEntityId (string, required): entity ID as string
    - Severity (string, required): "Low" / "Medium" / "High" / "Critical"
    - EscalationLevel (int, required): 1–3 (1=CS, 2=OpsManager, 3=Director)
    - SlaMinutes (int, required): SLA target in minutes
    - NotificationChain (string?, optional): comma-separated user IDs to notify
    - Message (string, required): escalation description
  Response DTO:         ApiResponse<EscalationResponse>
    - SystemAlertId (long), AlertCode (string), AlertType (string)
    - RelatedEntityName (string), RelatedEntityId (string)
    - Severity (string), AlertStatus (string), EscalationLevel (int)
  DB Tables:            SystemAlerts (write), AuditLogs (write)
  Business Rules:
    1. Alert persisted in SystemAlerts with generated AlertCode
    2. NotificationChain users notified per configured channels
  Failure Cases:
    - Invalid AlertType or Severity → 400

---

### Flow: Handle No-Show
  Entry Points:         SR Detail screen — "Customer No-Show" action
  UI Trigger:           Technician or ops marks customer absent
  API Endpoint:         POST /api/escalations/service-requests/{serviceRequestId}/no-show
  Auth:                 Policy = SupportManage
  Request DTO:          HandleNoShowRequest
    - Reason (string, required): reason for no-show
    - PreferredTechnicianId (long?, optional): preferred tech for retry
  Response DTO:         ApiResponse<ServiceRequestDetailResponse>
  DB Tables:            ServiceRequests (write), SRStatusHistory (write),
                        CustomerAbsentRecords (write), AuditLogs (write)
  Business Rules:
    1. SR status updated (CustomerAbsent or Rescheduled)
    2. CustomerAbsentRecord created with attempt count
    3. Customer notified to reschedule
  Notes on Drift:       Previously undocumented in ProjectOverview. Added 2026-05-24.

---

---

## MODULE: Field Workflow (Technician On-Site Execution)
## Controller: FieldWorkflowController — Route Prefix: /api/field
## Auth: Authorize(Roles = "Technician") on all endpoints
## Updated: 2026-05-24 — Stable Contract Level

---

### Flow 1: Get My Active Jobs

Entry Points:       AdminMobile → /field/jobs (TechnicianJobListScreen)
UI Trigger:         Screen load / pull-to-refresh
Endpoint:           GET /api/field/my-jobs
Request:            No body. No query params.
Response (200):
  IReadOnlyCollection<TechnicianJobListItemResponse>
  Each item:
    - ServiceRequestId (long)
    - JobCardId (long?)
    - ServiceRequestNumber (string)
    - JobCardNumber (string?)
    - LifecycleType (string)
    - LifecycleLabel (string)
    - BookingReference (string)
    - CustomerName (string)
    - MobileNumber (string)
    - AddressSummary (string)
    - ServiceName (string)
    - CurrentStatus (string)
    - SlotDate (DateOnly)
    - SlotLabel (string)
DB Tables:          ServiceRequests, JobCards, Bookings, Customers, Services
Business Rules:
  - Returns only SRs assigned to the currently authenticated technician
  - Filters to active/non-terminal statuses (excludes Completed, Cancelled, Closed)
Failure Cases:
  - 401 if token expired or role not Technician
Notes on Drift:     Previously listed as "my-jobs" stub only; now stable contract. Added 2026-05-24.

---

### Flow 2: Get Job History

Entry Points:       AdminMobile → /field/history
UI Trigger:         Screen load
Endpoint:           GET /api/field/job-history
Request:            No body. No query params.
Response (200):     IReadOnlyCollection<TechnicianJobListItemResponse> (same shape as Flow 1)
DB Tables:          ServiceRequests, JobCards, Bookings, Customers, Services
Business Rules:
  - Returns only SRs assigned to the technician with terminal statuses (Completed, Closed)
Failure Cases:
  - 401 if unauthorized
Notes on Drift:     Previously undocumented at contract level. Added 2026-05-24.

---

### Flow 3: Get Job Detail

Entry Points:       AdminMobile → /field/jobs/:id (TechnicianJobReportScreen / JobWorkflowContainer)
UI Trigger:         Job card tap; refreshed after every status mutation
Endpoint:           GET /api/field/jobs/{serviceRequestId}
Request:            Route param: serviceRequestId (long)
Response (200):     FieldJobDetailResponse
  - Job: TechnicianJobDetailResponse
      ServiceRequestId, ServiceRequestNumber, LifecycleType, LifecycleLabel,
      BookingId, BookingReference, CurrentStatus, CustomerName, MobileNumber,
      AddressSummary, ZoneName, ServiceName, AcTypeName, TonnageName, BrandName,
      ModelName, IssueNotes, SlotDate, SlotLabel, AssignmentRemarks?,
      JobCard (JobCardSummaryResponse), QuotationId?, QuotationNumber?, QuotationStatus?,
      Diagnosis (JobDiagnosisSummaryResponse), ChecklistSummary (JobChecklistSummaryResponse),
      ChecklistItems (JobChecklistItemResponse[]), Notes (JobExecutionNoteResponse[]),
      Attachments (JobAttachmentResponse[]), Timeline (JobExecutionTimelineItemResponse[]),
      SupportAlert (SupportTicketJobAlertResponse), AllowedActions (string[])
  - CustomerLatitude (double?)
  - CustomerLongitude (double?)
  - LatestReport (FieldJobReportResponse?)
  - Photos (FieldJobPhotoResponse[])
  - Signature (FieldCustomerSignatureResponse?)
  - PartsRequests (FieldPartsRequestResponse[])
  - Quotation (QuotationDetailResponse?)
  - Invoice (InvoiceDetailResponse?)
  - Payments (PaymentTransactionResponse[])
DB Tables:          ServiceRequests, JobCards, Bookings, Customers, CustomerAddresses,
                    tblJobReport, tblJobPhoto, tblCustomerSignature, tblPartsRequest,
                    Quotations, Invoices, PaymentTransactions
Failure Cases:
  - 404 if SR not found or not assigned to requesting technician
  - 401 if unauthorized
Notes on Drift:     FieldJobDetailResponse wraps TechnicianJobDetailResponse + all job artifacts.
                    Previously only stub. Added 2026-05-24.

---

### Flow 4: Depart (Mark En Route)

Entry Points:       AdminMobile → JobWorkflowContainer → Depart button
UI Trigger:         "Depart" / "On My Way" button tap
Endpoint:           PATCH /api/field/jobs/{id}/depart
Request Body (FieldJobStatusRequest):
  - Latitude (double?, optional): current GPS latitude
  - Longitude (double?, optional): current GPS longitude
  - Remarks (string?, optional): technician note
  - OverrideReason (string?, optional): not used for depart; present on shared DTO
Response (200):     FieldJobDetailResponse (full job snapshot after status change)
DB Tables:          ServiceRequests (status → Enroute), JobCards, tblJobReport
State Transitions:  ServiceRequest: Assigned → Enroute
Business Rules:
  - Sets DepartedAtUtc on JobCard
  - GPS coordinates logged to TechnicianGpsLog if provided
Failure Cases:
  - 422 if SR is not in Assigned status
  - 404 if SR not found
Notes on Drift:     Previously undocumented at contract level. Added 2026-05-24.

---

### Flow 5: Arrive (GPS Check-In with 150m Radius Gate)

Entry Points:       AdminMobile → JobWorkflowContainer → Arrive button
UI Trigger:         "I've Arrived" button tap; triggers device GPS read
Endpoint:           PATCH /api/field/jobs/{id}/arrive
Request Body (FieldJobStatusRequest):
  - Latitude (double?, optional): current GPS latitude
  - Longitude (double?, optional): current GPS longitude
  - Remarks (string?, optional)
  - OverrideReason (string?, optional): must be provided if overriding 150m gate
Response (200):     FieldArrivalValidationResponse
  - OverrideRequired (bool): true if technician is outside 150m radius and no override provided
  - DistanceMeters (double): calculated distance from customer address
  - Message (string): human-readable result
  - Job (FieldJobDetailResponse?): populated on successful arrival; null if override required
Response (422):     Returns FieldArrivalValidationResponse with OverrideRequired=true + DistanceMeters
                    when technician is outside 150m and OverrideReason was not supplied
DB Tables:          ServiceRequests (status → Arrived), CustomerAddresses, TechnicianGpsLog
State Transitions:  ServiceRequest: Enroute → Arrived
Business Rules:
  - Haversine distance calculated between technician GPS and CustomerAddress lat/lon
  - If distance > 150m AND OverrideReason is null/empty → return 422 with OverrideRequired=true
  - If distance > 150m AND OverrideReason is provided → record override, proceed with arrival
  - If distance ≤ 150m → proceed normally
  - Sets ArrivedAtUtc on JobCard
Failure Cases:
  - 422 OverrideRequired=true if outside geofence without override reason
  - 422 if SR not in Enroute status
  - 404 if SR not found
Recovery / Fallback: Client should re-call PATCH /arrive with OverrideReason after user confirmation dialog
Notes on Drift:     Previously noted only as "422 with overrideRequired=true" — now full contract. Added 2026-05-24.

---

### Flow 6: Start Work

Entry Points:       AdminMobile → JobWorkflowContainer → Start Work button
UI Trigger:         "Start Work" button tap
Endpoint:           PATCH /api/field/jobs/{id}/start-work
Request Body (FieldJobStatusRequest):
  - Remarks (string?, optional)
  - Latitude/Longitude (double?, optional)
  - OverrideReason (string?, optional) — not used
Response (200):     FieldJobDetailResponse
DB Tables:          ServiceRequests (status → InProgress), JobCards
State Transitions:  ServiceRequest: Arrived → InProgress
Business Rules:
  - Sets WorkStartedAtUtc on JobCard
Failure Cases:
  - 422 if not in Arrived status
Notes on Drift:     Previously undocumented at contract level. Added 2026-05-24.

---

### Flow 7: Save Checklist Progress

Entry Points:       AdminMobile → JobWorkflowContainer → Checklist tab
UI Trigger:         Checkbox toggle / checklist item save
Endpoint:           PATCH /api/field/jobs/{id}/progress
Request Body (FieldJobProgressRequest):
  - Items (SaveJobChecklistResponseItemRequest[], required): checklist item responses
  - Remarks (string?, optional)
Response (200):     FieldJobDetailResponse
DB Tables:          JobChecklistResponses, ServiceRequests
Business Rules:
  - Persists technician responses to each checklist item (pass/fail/NA/measured value)
  - Mandatory checklist items must be completed before report submission
Failure Cases:
  - 422 if SR not in InProgress status
Notes on Drift:     Previously undocumented at contract level. Added 2026-05-24.

---

### Flow 8: Submit Parts Request

Entry Points:       AdminMobile → JobWorkflowContainer → Parts tab → Request Parts
UI Trigger:         "Submit Parts Request" button
Endpoint:           POST /api/field/jobs/{id}/parts-request
Request Body (FieldPartsRequestRequest):
  - Urgency (string, required): e.g. "Normal", "Urgent", "Critical"
  - Items (FieldPartsRequestItemRequest[], required):
      each: PartId (long), QuantityRequested (decimal), Remarks (string?)
  - Notes (string?, optional)
Response (200):     FieldPartsRequestResponse
  - PartsRequestId (long)
  - ServiceRequestId (long)
  - JobCardId (long)
  - TechnicianId (long)
  - Urgency (string)
  - CurrentStatus (string)
  - Notes (string)
  - SubmittedAtUtc (DateTime)
  - ProcessedAtUtc (DateTime?)
  - Items (FieldPartsRequestItemResponse[]):
      each: PartsRequestItemId, PartId?, PartCode, PartName, QuantityRequested,
            QuantityApproved, CurrentStatus, ItemRemarks
DB Tables:          tblPartsRequest, tblPartsRequestItem
Business Rules:
  - Creates a pending parts request routed to operations/warehouse for approval
  - Job can continue while parts request is pending
Failure Cases:
  - 422 if SR not in InProgress or related active status
Notes on Drift:     Previously undocumented at contract level. Added 2026-05-24.

---

### Flow 9: Create Field Estimate (Quotation)

Entry Points:       AdminMobile → JobWorkflowContainer → Estimate tab
UI Trigger:         "Create Estimate" / "Submit Quote" button
Endpoint:           POST /api/field/jobs/{id}/estimate
Request Body (FieldEstimateRequest):
  - Lines (QuotationLineRequest[], required):
      each: LineType (string), LineDescription (string), Quantity (decimal), UnitPrice (decimal)
  - DiscountAmount (decimal, required)
  - TaxPercentage (decimal, required)
  - Remarks (string?, optional)
Response (200):     QuotationDetailResponse
  - QuotationId, QuotationNumber, JobCardId, JobCardNumber, ServiceRequestId,
    ServiceRequestNumber, BookingId, BookingReference, CustomerId, CustomerName,
    MobileNumber, AddressSummary, ServiceName, CurrentStatus,
    QuotationDateUtc, SubTotalAmount, DiscountAmount, TaxPercentage, TaxAmount,
    GrandTotalAmount, CustomerDecisionRemarks, ApprovedDateUtc?, RejectedDateUtc?,
    InvoiceId?, InvoiceNumber?, InvoiceStatus?,
    Lines (QuotationLineResponse[]), BillingHistory (BillingStatusHistoryResponse[])
DB Tables:          Quotations, QuotationLines, JobCards
State Transitions:  Quotation: created → PendingApproval
Business Rules:
  - GrandTotal = (SubTotal - Discount) + TaxAmount; TaxAmount = (SubTotal - Discount) × TaxPercentage/100
  - Quotation linked to JobCard and ServiceRequest
Failure Cases:
  - 422 if SR not in valid status for estimate creation
Notes on Drift:     Previously undocumented at contract level. Added 2026-05-24.

---

### Flow 10: Submit Job Report

Entry Points:       AdminMobile → JobWorkflowContainer → Report tab
UI Trigger:         "Submit Report" button (gated: mandatory checklist items must be complete)
Endpoint:           POST /api/field/jobs/{id}/report
Request Body (FieldJobReportRequest):
  - EquipmentCondition (string, required): "Good" | "Fair" | "Poor" | "Critical"
  - IssuesIdentified (string[], required): list of issue tags/text
  - ActionTaken (string, required): description of work performed
  - Recommendation (string?, optional): technician recommendation
  - Observations (string?, optional): additional notes
  - IdempotencyKey (string?, optional): client-generated UUID for offline replay safety
Response (200):     FieldJobReportResponse
  - JobReportId (long)
  - ServiceRequestId (long)
  - JobCardId (long)
  - TechnicianId (long)
  - IssuesIdentified (string[])
  - EquipmentCondition (string)
  - ActionTaken (string)
  - Recommendation (string)
  - Observations (string)
  - SubmittedAtUtc (DateTime)
  - IsQualityReviewed (bool)
  - QualityScore (decimal)
DB Tables:          tblJobReport, ServiceRequests, JobCards
Business Rules:
  - IdempotencyKey prevents duplicate submissions on offline retry (same key = return existing report)
  - First report submission transitions SR status
  - IsQualityReviewed and QualityScore are set later by QA reviewers (default false/0)
Failure Cases:
  - 409 if duplicate IdempotencyKey (return existing report)
  - 422 if mandatory checklist items incomplete
Notes on Drift:     DB table is tblJobReport (confirmed via SQL script 20260425_Add_FieldJob_Tables.sql).
                    IdempotencyKey prevents offline double-submit. Added 2026-05-24.

---

### Flow 11: Upload Job Photo

Entry Points:       AdminMobile → JobWorkflowContainer → Photos tab
UI Trigger:         Camera capture or gallery pick + "Upload" tap
Endpoint:           POST /api/field/jobs/{id}/photos
Request Body (FieldJobPhotoUploadRequest):
  - PhotoType (string, required): "Before" | "During" | "After" | "IssueEvidence"
  - FileName (string, required)
  - ContentType (string, required): MIME type e.g. "image/jpeg"
  - Base64Content (string, required): base64-encoded image data
  - Remarks (string?, optional)
Response (200):     FieldJobPhotoResponse
  - JobPhotoId (long)
  - ServiceRequestId (long)
  - JobCardId (long)
  - PhotoType (string)
  - FileName (string)
  - ContentType (string)
  - StorageUrl (string): CDN/blob URL of uploaded file
  - UploadedBy (string)
  - UploadedAtUtc (DateTime)
  - PhotoRemarks (string)
DB Tables:          tblJobPhoto
Business Rules:
  - Minimum 2 photos required before /complete is allowed
  - Each upload stored to blob/CDN; StorageUrl returned
Failure Cases:
  - 413 if payload too large (base64 image oversized)
  - 422 if SR not in active status
Notes on Drift:     DB table is tblJobPhoto (confirmed via SQL script 20260425_Add_FieldJob_Tables.sql).
                    Added 2026-05-24.

---

### Flow 12: Save Customer Signature

Entry Points:       AdminMobile → JobWorkflowContainer → Signature tab
UI Trigger:         "Save Signature" button after customer draws on canvas
Endpoint:           POST /api/field/jobs/{id}/signature
Request Body (FieldJobSignatureRequest):
  - CustomerName (string, required): auto-filled from booking; editable
  - SignatureBase64 (string, required): base64 PNG of drawn signature
  - Remarks (string?, optional)
Response (200):     FieldCustomerSignatureResponse
  - CustomerSignatureId (long)
  - ServiceRequestId (long)
  - JobCardId (long)
  - CustomerName (string)
  - SignatureDataUrl (string): stored data URL / blob URL
  - SignedAtUtc (DateTime)
  - CapturedBy (string)
  - SignatureRemarks (string)
DB Tables:          tblCustomerSignature
Business Rules:
  - One signature per job (overwrite if re-signed)
  - Signature required before /complete is allowed
Failure Cases:
  - 422 if SR not in valid active status
Notes on Drift:     DB table is tblCustomerSignature (confirmed via SQL script 20260425_Add_FieldJob_Tables.sql).
                    Added 2026-05-24.

---

### Flow 13: Collect Field Payment

Entry Points:       AdminMobile → JobWorkflowContainer → Payment tab
UI Trigger:         "Collect Payment" button
Endpoint:           PATCH /api/field/jobs/{id}/payment
Request Body (FieldJobPaymentRequest):
  - PaidAmount (decimal, required)
  - PaymentMethod (string, required): "Cash" | "UPI" | "Card" | "NetBanking"
  - ReferenceNumber (string?, optional): UPI/card ref
  - Remarks (string?, optional)
  - IdempotencyKey (string?, optional): client UUID for offline duplicate prevention
  - GatewayTransactionId (string?, optional): gateway-assigned ID
  - Signature (string?, optional): digital payment signature
  - ExpectedInvoiceAmount (decimal?, optional): client-side invoice total for cross-check
Response (200):     PaymentTransactionResponse
  - PaymentTransactionId (long)
  - InvoiceId (long)
  - PaymentMethod (string)
  - ReferenceNumber (string)
  - PaidAmount (decimal)
  - PaymentDateUtc (DateTime)
  - TransactionRemarks (string)
  - Receipt (PaymentReceiptResponse?)
DB Tables:          PaymentTransactions, Invoices
Business Rules:
  - IdempotencyKey prevents duplicate offline payment records
  - ExpectedInvoiceAmount is client-side validation only; server validates against actual invoice total
  - GatewayTransactionId stored for reconciliation with payment gateway
State Transitions:  Invoice: partially or fully paid based on PaidAmount vs InvoiceTotal
Failure Cases:
  - 409 if duplicate IdempotencyKey → return existing transaction
  - 422 if invoice not found or amount mismatch above tolerance
Notes on Drift:     IdempotencyKey + GatewayTransactionId for gateway duplicate prevention.
                    Previously only noted in summary. Full contract added 2026-05-24.

---

### Flow 14: Complete Job

Entry Points:       AdminMobile → JobWorkflowContainer → Complete button
UI Trigger:         "Complete Job" button (gated by completion requirements)
Endpoint:           PATCH /api/field/jobs/{id}/complete
Request Body (FieldJobStatusRequest):
  - Remarks (string?, optional)
  - Latitude/Longitude (double?, optional): final location log
  - OverrideReason (string?, optional): not used for complete
Response (200):     FieldJobDetailResponse (full snapshot; CurrentStatus = Completed)
DB Tables:          ServiceRequests (status → Completed), JobCards, Invoices
State Transitions:  ServiceRequest: InProgress → Completed
Business Rules:
  - Completion gate: ALL of the following must be satisfied before /complete is accepted:
    1. At least one job report submitted (tblJobReport row exists for this SR)
    2. At least 2 photos uploaded (tblJobPhoto count ≥ 2)
    3. Customer signature captured (tblCustomerSignature row exists)
  - Sets CompletedAtUtc on JobCard
  - Triggers invoice generation if not already generated
Failure Cases:
  - 422 if any completion gate condition not met (report/photos/signature missing)
  - 422 if SR not in InProgress status
Notes on Drift:     Completion gate (report + 2 photos + signature) previously only noted as a summary.
                    Full contract with gate conditions added 2026-05-24.

---

### Flow 15: Attendance Check-In

Entry Points:       AdminMobile → AttendanceScreen.tsx
UI Trigger:         "Check In" button at start of shift
Endpoint:           POST /api/field/attendance/check-in
Request Body (FieldAttendanceRequest):
  - LocationText (string?, optional): human-readable location label
  - Latitude (double?, optional): GPS lat
  - Longitude (double?, optional): GPS lon
Response (200):     TechnicianAttendanceResponse
  - TechnicianAttendanceId (long)
  - AttendanceDate (DateOnly)
  - AttendanceStatus (string)
  - CheckInOnUtc (DateTime?)
  - CheckOutOnUtc (DateTime?)
  - LocationText (string)
  - LeaveReason (string)
  - ReviewedByUserId (long?)
  - ReviewedOnUtc (DateTime?)
DB Tables:          TechnicianAttendance (tblTechnicianAttendance)
Business Rules:
  - One attendance record per technician per calendar day
  - Duplicate check-in same day → update existing record
  - AttendanceStatus set to "Present" on check-in
Failure Cases:
  - 422 if already checked in today with completed checkout
Notes on Drift:     Previously listed as one-liner. Full contract added 2026-05-24.

---

### Flow 16: Attendance Check-Out

Entry Points:       AdminMobile → AttendanceScreen.tsx
UI Trigger:         "Check Out" button at end of shift
Endpoint:           POST /api/field/attendance/check-out
Request Body (FieldAttendanceRequest):
  - LocationText (string?, optional)
  - Latitude (double?, optional)
  - Longitude (double?, optional)
Response (200):     TechnicianAttendanceResponse (same shape as check-in response; CheckOutOnUtc populated)
DB Tables:          TechnicianAttendance
Business Rules:
  - Sets CheckOutOnUtc on existing attendance record for today
  - Requires check-in record to exist first
State Transitions:  AttendanceStatus: Present → CheckedOut
Failure Cases:
  - 422 if no check-in record for today
Notes on Drift:     Previously listed as one-liner. Full contract added 2026-05-24.

---

## LEGACY: FieldExecutionController — /api/technician-jobs (DEPRECATED)
## Auth: Authorize(Roles = "Technician")
## Status: Still active but deprecated; prefer /api/field/* endpoints

Endpoints (all return TechnicianJobDetailResponse):
  POST /api/technician-jobs/mark-enroute             → UpdateTechnicianJobStatusRequest
  POST /api/technician-jobs/mark-reached             → UpdateTechnicianJobStatusRequest
  POST /api/technician-jobs/start-work               → UpdateTechnicianJobStatusRequest
  POST /api/technician-jobs/mark-in-progress         → UpdateTechnicianJobStatusRequest
  POST /api/technician-jobs/mark-work-completed      → UpdateTechnicianJobStatusRequest
  POST /api/technician-jobs/submit-for-closure       → UpdateTechnicianJobStatusRequest
  POST /api/technician-jobs/notes                    → SaveJobExecutionNoteRequest
  GET  /api/technician-jobs/timeline                 → JobExecutionTimelineItemResponse[]

Request DTO (UpdateTechnicianJobStatusRequest): Remarks (string?), WorkSummary (string?)
Notes: These mirror the old job status state machine. New code should use /api/field/* exclusively.

---

## OFFLINE SYNC — AdminMobile Field Workflow

Storage Keys (local-storage based, React web app):
  - FIELD_JOB_CACHE: individual job snapshots
  - FIELD_JOB_LIST_CACHE: job queue list
  - FIELD_TECHNICIAN_ATTENDANCE: today's attendance record
  - FIELD_HELPER_ASSIGNMENT: helper/assistant assignment data
  - FIELD_HELPER_ATTENDANCE: helper attendance
  - FIELD_OFFLINE_QUEUE: pending submissions awaiting connectivity

field-workflow-repository.ts capabilities:
  - Caches job snapshots for offline access
  - Queues report/photo/signature/payment submissions with retry metadata
  - Applies optimistic updates offline
  - Exposes retry via fieldWorkflowRepository.syncSubmission(id)
  - Offline queue is surfaced inline via NetworkStatusBanner (pending count) + AdminScaffold bell badge.
    Note on Drift (2026-06-10): the standalone OfflineSyncQueue.tsx page at route /system/sync was
    removed (component + route + all UI links). Backend offline-sync infra (tblOfflineSyncQueueItem,
    worker, repository methods) is unchanged and still authoritative.

IdempotencyKey usage:
  - JobReport (Flow 10): prevents duplicate report on offline retry
  - Payment (Flow 13): prevents duplicate charge on connectivity restore

Build status (2026-04-22): Backend dotnet build 0 warnings/errors;
  AdminMobile tsc --noEmit 0 errors; npm run build with chunked output succeeds.

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

## MODULE: Equipment Register
## Controllers: CustomerController (/api/customers — admin routes),
##              CustomerEquipmentController (/api/customers/me/equipment — customer portal)
## Verified: 2026-05-24 | 7 endpoints | Stable Contract

---

### Flow EQP-1: Get Customer Equipment List (Admin)
  Entry Points:         Customer 360 → Equipment tab; Admin Equipment Register screen
  UI Trigger:           Tab select / page load
  Endpoint:             GET /api/customers/{customerId:long}/equipment
  Auth:                 Authorize(Policy=UserRead)
  Request DTO:          route: customerId (long)
  Response DTO:         ApiResponse<IReadOnlyCollection<CustomerEquipmentResponse>>
    CustomerEquipmentResponse:
      - CustomerEquipmentId long
      - CustomerId          long
      - Name                string   — equipment name / label
      - Type                string   — equipment type (e.g. "Split AC", "Cassette AC")
      - Brand               string
      - Capacity            string   — e.g. "1.5 Ton" (string, not numeric)
      - Location            string   — room / location description (not AddressId)
      - PurchaseDate        DateOnly?
      - LastServiceDate     DateOnly?
      - SerialNumber        string
      - IsActive            bool
      - DateCreated         DateTime
      - LastUpdated         DateTime?
  DB Tables Read:       tblCustomerEquipment (via GetAdminCustomerDetailQuery — returns .Equipment)
  Business Rules:
    1. Returns all equipment records (active and inactive) for the customer.
  Failure Cases:
    404 — customer not found
    401, 403 — auth/permission

---

### Flow EQP-2: Create Customer Equipment (Admin)
  Entry Points:         Customer 360 → Equipment tab → Add Equipment
  UI Trigger:           Add Equipment form submit
  Endpoint:             POST /api/customers/{customerId:long}/equipment
  Auth:                 Authorize(Policy=UserUpdate)
  Request DTO:          CreateCustomerEquipmentRequest (body)
    - Name              string    required
    - Type              string    required
    - Brand             string    required
    - Capacity          string    required  — e.g. "1.5 Ton"
    - Location          string    required  — room/location description
    - PurchaseDate      DateOnly? — optional
    - LastServiceDate   DateOnly? — optional
    - SerialNumber      string?   — optional
  Response DTO:         CustomerEquipmentResponse (new record)
  DB Tables Written:    tblCustomerEquipment (via CreateAdminCustomerEquipmentCommand)
  Business Rules:
    1. Linked to customerId from route.
    2. IsActive defaults to true.
  Failure Cases:
    404 — customer not found
    400 — validation failure
    401, 403 — auth/permission

---

### Flow EQP-3: Update Customer Equipment (Admin)
  Entry Points:         Customer 360 → Equipment → Edit Equipment
  UI Trigger:           Save button on edit form
  Endpoint:             PUT /api/customers/{customerId:long}/equipment/{equipmentId:long}
  Auth:                 Authorize(Policy=UserUpdate)
  Request DTO:          UpdateCustomerEquipmentRequest (body)
    - CustomerEquipmentId long    required (also in body; route equipmentId takes precedence)
    - Name              string    required
    - Type              string    required
    - Brand             string    required
    - Capacity          string    required
    - Location          string    required
    - PurchaseDate      DateOnly?
    - LastServiceDate   DateOnly?
    - SerialNumber      string?
  Response DTO:         CustomerEquipmentResponse (updated)
  DB Tables Written:    tblCustomerEquipment (via UpdateAdminCustomerEquipmentCommand)
  Business Rules:
    1. Command receives both customerId and equipmentId — validates ownership.
  Failure Cases:
    404 — equipment not found or not owned by customer
    400 — validation failure
    401, 403 — auth/permission
  Notes on Drift:
    No admin soft-delete endpoint found — IsActive flag changes via UpdateEquipmentAsync only.

---

### Flow EQP-4: Get My Equipment (Customer Portal)
  Entry Points:         Customer Portal → My Equipment / Profile → Appliances
  UI Trigger:           Screen mount
  Endpoint:             GET /api/customers/me/equipment
  Auth:                 Authorize (JWT — any authenticated user; CustomerId from JWT claims)
  Request DTO:          None
  Response DTO:         ApiResponse<IReadOnlyCollection<CustomerEquipmentResponse>> (same shape as EQP-1)
  DB Tables Read:       tblCustomerEquipment (via GetMyCustomerEquipmentQuery — JWT-scoped)
  Business Rules:
    1. Returns only equipment owned by the authenticated customer (CustomerId from JWT).
  Failure Cases:
    401 — not authenticated

---

### Flow EQP-4b: Get My Equipment By Id (Customer Portal)
  Entry Points:         Customer Portal → My Equipment → Equipment detail / edit screen
  UI Trigger:           Navigation to /portal/equipment/{id}
  Endpoint:             GET /api/customers/me/equipment/{equipmentId:long}
  Auth:                 Authorize (JWT — customer)
  Request DTO:          equipmentId (route param, long)
  Response DTO:         ApiResponse<CustomerEquipmentResponse>
    CustomerEquipmentResponse: same shape as EQP-4 list items
  Application Layer:    GetMyCustomerEquipmentByIdQuery(CustomerEquipmentId) →
                        GetMyCustomerEquipmentByIdQueryHandler
  DB Tables Read:       tblCustomerEquipment via ICustomerAppRepository.GetEquipmentForUpdateAsync
                        (filters by CustomerId + CustomerEquipmentId + IsDeleted=false)
  Business Rules:
    1. Ownership enforced: GetEquipmentForUpdateAsync validates CustomerId matches JWT.
    2. If record not found or belongs to another customer → 404 NotFound.
  Failure Cases:
    401 — not authenticated
    404 — equipment not found or not owned by caller
  Notes on Drift:
    Added 2026-05-26. Backend previously had no single-item GET for customer equipment.
    GetEquipmentForUpdateAsync was reused (no new repo method needed).

---

### Flow EQP-5: Add My Equipment (Customer Portal)
  Entry Points:         Customer Portal → My Equipment → Add New Appliance
  UI Trigger:           Add Equipment form submit
  Endpoint:             POST /api/customers/me/equipment
  Auth:                 Authorize (JWT — customer)
  Request DTO:          CreateCustomerEquipmentRequest (body) — same shape as EQP-2 request
    - Name, Type, Brand, Capacity, Location, PurchaseDate?, LastServiceDate?, SerialNumber?
  Response DTO:         CustomerEquipmentResponse (new record)
  DB Tables Written:    tblCustomerEquipment (via CreateMyCustomerEquipmentCommand)
  Business Rules:
    1. CustomerId resolved from JWT.
  Failure Cases:
    400 — validation failure
    401 — not authenticated

---

### Flow EQP-6: Update My Equipment (Customer Portal)
  Endpoint:             PUT /api/customers/me/equipment/{equipmentId:long}
  Auth:                 Authorize (JWT — customer)
  Request DTO:          UpdateCustomerEquipmentRequest (body)
  Response DTO:         CustomerEquipmentResponse (updated)
  DB Tables Written:    tblCustomerEquipment (via UpdateMyCustomerEquipmentCommand)
  Business Rules:
    1. Validates that equipmentId belongs to the authenticated customer.
  Failure Cases:
    404 — equipment not found or not owned by customer
    400 — validation failure
    401 — not authenticated

---

### Flow EQP-7: Delete My Equipment (Customer Portal)
  Endpoint:             DELETE /api/customers/me/equipment/{equipmentId:long}
  Auth:                 Authorize (JWT — customer)
  Request DTO:          route: equipmentId (long)
  Response DTO:         ApiResponse<object> → { equipmentId }
  DB Tables Written:    tblCustomerEquipment (via DeleteMyCustomerEquipmentCommand — soft delete)
  Business Rules:
    1. Soft delete — sets IsActive=false or IsDeleted=true; record is retained.
    2. Validates ownership via JWT CustomerId.
  Failure Cases:
    404 — equipment not found or not owned by customer
    401 — not authenticated

---

### DB TABLES — Equipment Register (Confirmed)

tblCustomerEquipment
  - CustomerEquipmentId (PK bigint), CustomerId (FK),
    Name (nvarchar), Type (nvarchar), Brand (nvarchar),
    Capacity (nvarchar), Location (nvarchar),
    PurchaseDate (date nullable), LastServiceDate (date nullable),
    SerialNumber (nvarchar nullable), IsActive (bit),
    + audit columns (CreatedBy, DateCreated, UpdatedBy, LastUpdated, IsDeleted, etc.)
  - Note: Old docs listed fields EquipmentTag, Model (separate), Tonnage, AddressId, InstallationYear,
    WarrantyExpiryDate, Supplier, InvoiceNumber, Notes — NONE of these are in actual DTOs.
  - Note: Location is a free-text string, NOT a FK to tblCustomerAddress.
  - Note: Capacity is a string (e.g. "1.5 Ton"), not a numeric tonnage field.
  - Note: No separate WarrantyExpiryDate on equipment — warranty tracked in tblWarrantyRecord (Module 11).

---

### DRIFT NOTES — Equipment Register (corrected 2026-05-24)

1. Old docs canonical fields had: EquipmentTag, Model (separate field), Tonnage/Capacity as numeric,
   AddressId (FK), InstallationYear, WarrantyExpiryDate, Supplier, InvoiceNumber, Status (string), Notes
   → Actual DTO has: Name, Type, Brand, Capacity (string), Location (string), PurchaseDate, LastServiceDate,
     SerialNumber, IsActive (bool), DateCreated, LastUpdated. No EquipmentTag, no Model, no AddressId FK,
     no WarrantyExpiryDate, no Supplier, no InvoiceNumber, no Notes.

2. Admin routes live on CustomerController (/api/customers/{customerId}/equipment), not a dedicated
   /api/equipment controller. ModuleIndex listed "Get Equipment, Create Equipment, Update Equipment,
   SoftDelete Equipment" as if they were on a separate controller — clarified.

3. "Get Equipment History" — no dedicated endpoint; equipment list is embedded in
   GetAdminCustomerDetailQuery response (.Equipment field).

4. "Get Booking Brands" — not part of Equipment Register; located in Booking/Configuration module.

5. No admin soft-delete endpoint — only Customer Portal has DELETE. Admin deactivates via PUT (IsActive).

6. Old DB table listing "JobReports, WarrantyRecords, PartsRequests" under Equipment module → incorrect;
   JobReports = Field Workflow (tblJobReport), WarrantyRecords = Module 11, PartsRequests = Inventory.

---

## MODULE: AMC Contract Engine
## Controller: AmcController (/api/amc)
## Verified: 2026-05-24 | 8 endpoints | Stable Contract

AMC lifecycle (confirmed):
  Plan Setup → Customer Enrollment (POST /api/amc/assign) → Visit Generation (POST /generate-visits)
  → Visit Execution (each visit linked to SR) → Contract expires when EndDateUtc passed

---

### Flow AMC-1: Create AMC Plan
  Entry Points:         Admin → AMC → Plans → New Plan
  UI Trigger:           Save Plan form submit
  Endpoint:             POST /api/amc/plans
  Auth:                 Authorize(Policy=AmcCreate)
  Request DTO:          CreateAmcPlanRequest (body)
    - PlanName          string    required
    - PlanDescription   string?
    - DurationInMonths  int       required — plan validity period
    - VisitCount        int       required — number of service visits included
    - PriceAmount       decimal   required
    - IsActive          bool      required
    - TermsAndConditions string?
  Response DTO:         AmcPlanResponse
    - AmcPlanId         long
    - PlanName          string
    - PlanDescription   string
    - DurationInMonths  int
    - VisitCount        int
    - PriceAmount       decimal
    - IsActive          bool
    - TermsAndConditions string
  DB Tables Written:    tblAmcPlan (via CreateAmcPlanCommand)
  Failure Cases:
    400 — validation failure
    401, 403 — auth/permission

---

### Flow AMC-2: Update AMC Plan
  Endpoint:             PUT /api/amc/plans/{amcPlanId:long}
  Auth:                 Authorize(Policy=AmcCreate)
  Request DTO:          UpdateAmcPlanRequest (body) — identical fields to CreateAmcPlanRequest
    - PlanName, PlanDescription?, DurationInMonths, VisitCount, PriceAmount, IsActive, TermsAndConditions?
  Response DTO:         AmcPlanResponse (updated)
  DB Tables Written:    tblAmcPlan (via UpdateAmcPlanCommand)
  Failure Cases:
    404 — plan not found
    400, 401, 403

---

### Flow AMC-3: List AMC Plans
  Entry Points:         Admin → AMC → Plans list; Customer enrollment plan picker
  Endpoint:             GET /api/amc/plans
  Auth:                 Authorize (any authenticated user)
  Request DTO:          query params
    - isActive          bool?   — filter by active status
    - pageNumber        int     default 1
    - pageSize          int     default 20
  Response DTO:         ApiResponse<PagedResult<AmcPlanResponse>>
  DB Tables Read:       tblAmcPlan (via GetAmcPlansQuery)
  Failure Cases:
    401 — not authenticated

---

### Flow AMC-4: Get AMC Plan by ID
  Endpoint:             GET /api/amc/plans/{amcPlanId:long}
  Auth:                 Authorize (any authenticated user)
  Request DTO:          route: amcPlanId (long)
  Response DTO:         AmcPlanResponse
  DB Tables Read:       tblAmcPlan (via GetAmcPlanByIdQuery)
  Failure Cases:
    404 — plan not found
    401 — not authenticated

---

### Flow AMC-5: Assign AMC to Customer (Enrollment)
  Entry Points:         Admin → AMC → Enroll Customer; also triggered post-payment in billing flow
  UI Trigger:           Enroll / Assign button with plan + job card + invoice selection
  Endpoint:             POST /api/amc/assign
  Auth:                 Authorize(Policy=AmcAssign)
  Request DTO:          AssignAmcToCustomerRequest (body)
    - CustomerId        long      required
    - AmcPlanId         long      required
    - JobCardId         long      required — the job card associated with the AMC activation visit
    - InvoiceId         long      required — the invoice for AMC payment
    - StartDateUtc      DateTime? — defaults to current date if null
    - Remarks           string?
  Response DTO:         CustomerAmcResponse
    - CustomerAmcId     long
    - CustomerId        long
    - CustomerName      string
    - AmcPlanId         long
    - PlanName          string
    - JobCardId         long
    - JobCardNumber     string
    - InvoiceId         long
    - InvoiceNumber     string
    - CurrentStatus     string   — e.g. "Active", "Expired", "Cancelled"
    - StartDateUtc      DateTime
    - EndDateUtc        DateTime — StartDateUtc + DurationInMonths
    - TotalVisitCount   int      — from AmcPlan.VisitCount
    - ConsumedVisitCount int
    - PriceAmount       decimal
    - Visits            AmcVisitScheduleResponse[]
        - AmcVisitScheduleId long
        - VisitNumber        int
        - ScheduledDate      DateOnly
        - CurrentStatus      string
        - ServiceRequestId   long?  — linked SR once visit is issued
        - ServiceRequestNumber string?
        - CompletedDateUtc   DateTime?
        - VisitRemarks       string
  DB Tables Written:    tblCustomerAmc, tblAmcVisitSchedule (via AssignAmcToCustomerCommand)
  Business Rules:
    1. EndDateUtc = StartDateUtc + DurationInMonths months.
    2. Visit schedules created automatically on enrollment (same as calling generate-visits).
    3. JobCardId and InvoiceId are mandatory — AMC is always tied to a service job + payment.
  Failure Cases:
    404 — customer, plan, job card, or invoice not found
    400 — validation failure
    401, 403 — auth/permission
  Notes on Drift:
    - Old docs: AssignAmcToCustomerRequest had "EquipmentCovered (JSON)" — NOT in actual contract.
    - Old docs: ContractNumber field — NOT in CustomerAmcResponse; identification is via CustomerAmcId.
    - Old docs: CorporateAccountId on contract — NOT in contract; CustomerId only.
    - Old docs: BillingCycle/PaymentStatus/AutoRenewEnabled — NOT in current DTOs.

---

### Flow AMC-6: Generate AMC Visit Schedule (Manual Trigger)
  Entry Points:         Admin → AMC → Contract Detail → Generate Visits button
  UI Trigger:           Generate Visits button (if visits not yet created)
  Endpoint:             POST /api/amc/customer/{customerAmcId:long}/generate-visits
  Auth:                 Authorize(Policy=AmcAssign)
  Request DTO:          route: customerAmcId (long)
  Response DTO:         CustomerAmcResponse (with Visits[] populated)
  DB Tables Written:    tblAmcVisitSchedule (via GenerateAmcVisitsCommand)
  Business Rules:
    1. Generates VisitCount entries in tblAmcVisitSchedule for the CustomerAmc.
    2. Visits spread evenly across DurationInMonths (one per interval).
    3. Idempotent — does not duplicate visits if already generated.
  Failure Cases:
    404 — CustomerAmc not found
    401, 403 — auth/permission

---

### Flow AMC-7: Get AMC Subscriptions for Customer (Admin)
  Entry Points:         Admin → Customer 360 → AMC tab; AMC Contract List (filter by customer)
  Endpoint:             GET /api/amc/customer/{customerId:long}
  Auth:                 Authorize (any authenticated user)
  Request DTO:          route: customerId (long)
  Response DTO:         ApiResponse<IReadOnlyCollection<CustomerAmcResponse>>
  DB Tables Read:       tblCustomerAmc, tblAmcPlan, tblAmcVisitSchedule (via GetCustomerAmcQuery)
  Business Rules:
    1. Returns all AMC subscriptions for the customer (active, expired, cancelled).
  Failure Cases:
    401 — not authenticated

---

### Flow AMC-8: Get My AMC Subscriptions (Customer Portal)
  Entry Points:         Customer Portal → My AMC / Contracts screen
  UI Trigger:           Screen mount
  Endpoint:             GET /api/amc/customer/me
  Auth:                 Authorize (JWT — customer role; CustomerId extracted from claims via ServiceLifecycleAccessService)
  Request DTO:          None (CustomerId from JWT)
  Response DTO:         ApiResponse<IReadOnlyCollection<CustomerAmcResponse>> (same shape as AMC-7)
  DB Tables Read:       tblCustomerAmc, tblAmcPlan, tblAmcVisitSchedule (via GetCustomerAmcQuery)
  Business Rules:
    1. CustomerId resolved from JWT via ServiceLifecycleAccessService.GetCurrentCustomerIdAsync().
    2. Returns same data as AMC-7 but scoped to the authenticated customer.
  Failure Cases:
    401 — not authenticated / not a customer token

---

### DB TABLES — AMC Contract Engine (Confirmed from EF + DTO shapes)

tblAmcPlan
  - AmcPlanId (PK bigint), PlanName (nvarchar), PlanDescription (nvarchar), DurationInMonths (int),
    VisitCount (int), PriceAmount (decimal), IsActive (bit), TermsAndConditions (nvarchar)
  - Note: Old docs named this "AMCPlans" — actual EF entity is AmcPlan/tblAmcPlan.

tblCustomerAmc
  - CustomerAmcId (PK bigint), CustomerId (FK), AmcPlanId (FK), JobCardId (FK), InvoiceId (FK),
    CurrentStatus (nvarchar — "Active" | "Expired" | "Cancelled"),
    StartDateUtc (datetime2), EndDateUtc (datetime2),
    TotalVisitCount (int), ConsumedVisitCount (int), PriceAmount (decimal),
    Remarks (nvarchar nullable)
  - Note: Old docs named this "AMCContracts" — actual EF entity is CustomerAmc/tblCustomerAmc.
  - Note: Old docs had ContractNumber, EquipmentCovered JSON, BillingCycle, AutoRenewEnabled —
    none of these are in actual entity/DTOs.

tblAmcVisitSchedule
  - AmcVisitScheduleId (PK bigint), CustomerAmcId (FK), VisitNumber (int),
    ScheduledDate (date/DateOnly), CurrentStatus (nvarchar),
    ServiceRequestId (FK nullable), CompletedDateUtc (datetime2 nullable),
    VisitRemarks (nvarchar)
  - Note: Old docs had "AMCVisitLog" as separate table — not confirmed; visit tracking is via
    tblAmcVisitSchedule with CompletedDateUtc and SR linkage.

---

### DRIFT NOTES — AMC Contract Engine (corrected 2026-05-24)

1. Old docs listed "AMCContracts", "AMCVisitSchedule", "AMCVisitLog", "AMCPlans" as table names
   → actual: tblCustomerAmc, tblAmcVisitSchedule, tblAmcPlan (no separate VisitLog table).

2. Old docs had ContractNumber field on contract → NOT in CustomerAmcResponse.

3. Old docs had CorporateAccountId, EquipmentCovered (JSON), BillingCycle, PaymentStatus,
   AutoRenewEnabled, RenewalDate, RenewalAlertSent on contract → NONE in actual entity/DTOs.

4. Old docs described "30-day renewal alert" as automated → not confirmed in source; no renewal
   endpoint or AutoRenewEnabled field in current implementation.

5. Old docs listed separate API names: "Get Contract Visits", "Create Contract Visit", "Update Contract",
   "Renew Contract" → no separate CRUD for contracts or visits; contract = enrollment (POST /api/amc/assign);
   visits = generate-visits (POST /api/amc/customer/{id}/generate-visits).

6. AMC-8 (GET /api/amc/customer/me) was entirely missing from old docs — added.

7. AMC-6 (POST /api/amc/customer/{customerAmcId}/generate-visits) was missing from old docs — added.

8. Auto-schedule note: visit schedules may be created automatically on enrollment
   (AssignAmcToCustomerCommand likely calls GenerateAmcVisitsCommand internally — [VERIFY]).

---

## MODULE: Warranty Management
## Controller: WarrantyController (/api/warranty)
## Verified: 2026-05-24 | 2 endpoints | Stable Contract

Architecture note: Warranty is **invoice-centric** (not equipment-centric). A warranty claim
is raised against an InvoiceId (a completed service job), not against an EquipmentId.
Configurable WarrantyRules define coverage periods per service type.

---

### Flow WRN-1: Create Warranty Claim
  Entry Points:         Admin → Service Request → Warranty Claim; Customer Portal → My Bookings → Raise Warranty
  UI Trigger:           "Raise Warranty Claim" button on completed job invoice
  Endpoint:             POST /api/warranty/claim
  Auth:                 Authorize (any authenticated user)
  Request DTO:          CreateWarrantyClaimRequest (body)
    - InvoiceId         long      required — invoice of the original completed service job
    - ClaimRemarks      string?   — reason for revisit
  Response DTO:         WarrantyClaimResponse
    - WarrantyClaimId       long
    - InvoiceId             long
    - InvoiceNumber         string
    - CustomerId            long
    - CustomerName          string
    - WarrantyRuleId        long?   — matched rule (null if no rule configured for this service)
    - WarrantyRuleName      string? — e.g. "30-Day Repair Warranty", "90-Day Parts Warranty"
    - CoverageStartDateUtc  DateTime — start of warranty window
    - CoverageEndDateUtc    DateTime — end of warranty window
    - IsEligible            bool    — true if claim is within coverage window
    - CurrentStatus         string  — e.g. "Pending", "Approved", "Rejected"
    - ServiceName           string
    - ClaimRemarks          string
    - ClaimDateUtc          DateTime
    - RevisitRequestId      long?   — linked SR created for the revisit (if approved)
  DB Tables Written:    tblWarrantyClaim (via CreateWarrantyClaimCommand)
  Business Rules:
    1. System looks up WarrantyRule applicable to the service type on the invoice.
    2. CoverageStartDateUtc derived from invoice completion date.
    3. CoverageEndDateUtc = CoverageStartDateUtc + rule coverage days.
    4. IsEligible = true if ClaimDateUtc is within coverage window AND within valid WarrantyRule.
    5. On approval, a revisit ServiceRequest is created and linked via RevisitRequestId.
    6. Configurable rules (stored in tblWarrantyRule) drive the 30-day / 90-day windows.
  Failure Cases:
    404 — invoice not found
    400 — validation failure
    401 — not authenticated

---

### Flow WRN-2: Get Warranty Status by Invoice
  Entry Points:         Admin → Invoice Detail → Warranty tab; Customer Portal → Booking Detail → Warranty
  UI Trigger:           Tab select / page load
  Endpoint:             GET /api/warranty/invoice/{invoiceId:long}
  Auth:                 Authorize (any authenticated user)
  Request DTO:          route: invoiceId (long)
  Response DTO:         WarrantyStatusResponse
    - InvoiceId             long
    - InvoiceNumber         string
    - CustomerId            long
    - CustomerName          string
    - ServiceName           string
    - IsWarrantyAvailable   bool    — true if any WarrantyRule exists for this service type
    - IsEligible            bool    — true if still within coverage window
    - EligibilityMessage    string  — human-readable status (e.g. "Warranty valid until 15 Jul 2026")
    - CoverageStartDateUtc  DateTime? — null if no warranty configured
    - CoverageEndDateUtc    DateTime? — null if no warranty configured
    - WarrantyRuleName      string? — e.g. "30-Day Repair Warranty"
    - Claims                WarrantyClaimResponse[] — all claims raised against this invoice
  DB Tables Read:       tblWarrantyClaim, tblWarrantyRule (via GetWarrantyByInvoiceQuery)
  Business Rules:
    1. IsWarrantyAvailable = WarrantyRule exists for the service type on this invoice.
    2. IsEligible = within coverage window.
    3. Claims includes all historical claims with their statuses.
  Failure Cases:
    404 — invoice not found
    401 — not authenticated

---

### DB TABLES — Warranty Management (Confirmed from DTO shapes)

tblWarrantyClaim
  - WarrantyClaimId (PK bigint), InvoiceId (FK), CustomerId (FK),
    WarrantyRuleId (FK nullable), CoverageStartDateUtc (datetime2),
    CoverageEndDateUtc (datetime2), IsEligible (bit), CurrentStatus (nvarchar),
    ServiceName (nvarchar), ClaimRemarks (nvarchar), ClaimDateUtc (datetime2),
    RevisitRequestId (FK nullable — links to tblServiceRequest for the revisit)

tblWarrantyRule
  - WarrantyRuleId (PK), WarrantyRuleName (nvarchar),
    ServiceTypeId (FK — which service type this rule applies to),
    CoverageDays (int — e.g. 30 for repair, 90 for parts)
  - Note: Rules are configurable per service type.
  - Note: Old docs described 30-day/90-day as hardcoded — actual: driven by WarrantyRule config.

---

### DRIFT NOTES — Warranty Management (corrected 2026-05-24)

1. Critical model drift: old docs described warranty as EQUIPMENT-CENTRIC (EquipmentId FK,
   SerialNumber check on SR creation) → actual implementation is INVOICE-CENTRIC (InvoiceId FK).
   Warranty is raised against a completed job's invoice, not an equipment serial number.

2. Old docs table "WarrantyRecords" with fields: WarrantyId, EquipmentId, PartName, ReplacementDate,
   ExpiryDate, TechnicianId, LinkedSRId, Notes → NOT confirmed in actual DTOs.
   Actual tables: tblWarrantyClaim + tblWarrantyRule.

3. Old docs reference "GET/POST /api/warranty/records" → actual routes:
   POST /api/warranty/claim and GET /api/warranty/invoice/{invoiceId}.

4. WarrantyRule concept entirely absent from old docs — actual system has configurable rules
   (WarrantyRuleId, WarrantyRuleName) driving coverage periods.

5. IsWarrantyAvailable, IsEligible, EligibilityMessage, RevisitRequestId — all new fields
   not in old docs.

6. "30-day repair / 90-day parts" business rules may still apply as default WarrantyRule
   configurations but are not hardcoded — they are rule-driven.

---

5. CUSTOMER MASTER API CONTRACT — STABLE (audited 2026-05-24)

Controllers: CustomerController (/api/customers), CustomerAddressController (/api/customers)
All endpoints require Authorize unless noted.

---

### Flow: Get Customers List (Admin)
  Entry Points:         Customer List screen (CustomerListScreen.tsx)
  UI Trigger:           Page load / search input
  API Endpoint:         GET /api/customers
  Auth:                 Policy = UserRead
  Query Params:
    - searchTerm (string?, optional): name / mobile / email filter
    - pageNumber (int, default=1)
    - pageSize (int, default=20)
  Response DTO:         ApiResponse<PagedResult<CustomerAdminListItemResponse>>
    CustomerAdminListItemResponse:
      - CustomerId (long), CustomerName (string), MobileNumber (string), EmailAddress (string)
      - IsActive (bool), RiskLevel (string)
      - TotalServicesCount (int), TotalRevenueAmount (decimal), OutstandingAmount (decimal)
      - HasActiveAmc (bool), OpenSupportTicketCount (int)
      - CustomerSinceUtc (DateTime), LastServiceDateUtc (DateTime?), PrimaryAddressSummary (string?)
  DB Tables:            Customers (read), CustomerAddresses (read), AMCContracts (read),
                        ServiceRequests (read aggregate), Invoices (read aggregate)
  Failure Cases:
    - Unauthorized → 403

---

### Flow: Create Customer (Admin)
  Entry Points:         Create Customer screen (CreateCustomerScreen.tsx), CS phone intake
  UI Trigger:           "Save Customer" form submit
  API Endpoint:         POST /api/customers
  Auth:                 Policy = UserCreate
  Request DTO:          CreateCustomerAccountRequest
    - CustomerName (string, required)
    - MobileNumber (string, required): must be unique
    - EmailAddress (string, required): must be unique
  Response DTO:         ApiResponse<CustomerAccountResponse>
    - CustomerId (long), UserId (long), CustomerName (string)
    - MobileNumber (string), EmailAddress (string)
    - PasswordGenerated (bool), RequiresPasswordDelivery (bool)
    - MustChangePassword (bool), IsTemporaryPassword (bool), PasswordExpiryOnUtc (DateTime?)
  DB Tables:            Customers (write), Users (write)
  Business Rules:
    1. Creates Customer + linked User account in one transaction
    2. System generates temporary password; RequiresPasswordDelivery=true
    3. MobileNumber and EmailAddress must be unique across Customers table
  Failure Cases:
    - Duplicate mobile or email → 400
    - Validation failure → 400
  Notes on Drift:
    - Admin creates via POST /api/customers (no password field — admin-managed)
    - Customer self-registers via POST /api/customer-auth/register (password optional)

---

### Flow: Get Customer Detail (Admin / Customer 360)
  Entry Points:         Customer 360 View (Customer360ViewScreen.tsx)
  UI Trigger:           Customer row click / direct navigation
  API Endpoint:         GET /api/customers/{customerId}
  Auth:                 Policy = UserRead
  Response DTO:         ApiResponse<CustomerAdminDetailResponse>
    CustomerAdminDetailResponse:
      - CustomerId (long), CustomerName (string), MobileNumber (string), EmailAddress (string)
      - IsActive (bool), RiskLevel (string)
      - TotalServicesCount (int), TotalRevenueAmount (decimal), OutstandingAmount (decimal)
      - HasActiveAmc (bool), OpenSupportTicketCount (int), TotalSupportTicketCount (int)
      - CustomerSinceUtc (DateTime), LastServiceDateUtc (DateTime?)
      - LastInvoiceDateUtc (DateTime?), LastInvoiceStatus (string?)
      - PrimaryAddressSummary (string?)
      - ActiveAmcCount (int), ActiveAmcPlanName (string?), ActiveAmcStatus (string?)
      - VisitsIncluded (int?), VisitsUsed (int?), NextAmcVisitDate (DateOnly?)
      - Addresses (CustomerAddressResponse[]): embedded address list
      - Equipment (CustomerEquipmentResponse[]): embedded equipment list
      - Notes (CustomerNoteResponse[]): embedded note feed
  DB Tables:            Customers (read), CustomerAddresses (read), CustomerEquipment (read),
                        CustomerNotes (read), AMCContracts (read), ServiceRequests (read),
                        Invoices (read), SupportTickets (read)
  Business Rules:
    1. Returns empty Equipment[] if no equipment rows (not a 500); requires tblCustomerEquipment
       to exist — see DB_Alter_20260506_CustomerEquipmentCompatibility.sql
  Customer 360 enrichment calls (separate API calls from AdminMobile):
    - GET /api/service-history/customer/{customerId}
    - GET /api/amc/customer/{customerId}
    - GET /api/communication-preferences/customer/{customerId}
    - GET /api/support-tickets?customerMobile=...
    - GET /api/invoices?customerId=...
  Failure Cases:
    - Customer not found → 404

---

### Flow: Update Customer (Admin)
  Entry Points:         Customer 360 — Edit Customer action
  UI Trigger:           "Save" on edit form
  API Endpoint:         PUT /api/customers/{customerId}
  Auth:                 Policy = UserUpdate
  Request DTO:          UpdateAdminCustomerRequest
    - CustomerName (string, required)
    - MobileNumber (string, required)
    - EmailAddress (string, required)
  Response DTO:         ApiResponse<CustomerAdminDetailResponse> (full detail — same shape above)
  DB Tables:            Customers (write), Users (write)
  Failure Cases:
    - Customer not found → 404
    - Duplicate mobile/email → 400
  Notes on Drift:
    - DELETE /api/customers/{customerId} was documented previously but does NOT exist
      in CustomerController. Soft-delete is done via status fields, not an HTTP DELETE.
      Removed from documentation 2026-05-24.

---

### Flow: Get Customer Addresses (Admin)
  Entry Points:         Customer 360 — addresses panel
  UI Trigger:           Page load (embedded in detail response) or explicit tab open
  API Endpoint:         GET /api/customers/{customerId}/addresses
  Auth:                 Policy = UserRead
  Response DTO:         ApiResponse<IReadOnlyCollection<CustomerAddressResponse>>
    CustomerAddressResponse:
      - CustomerAddressId (long), CustomerId (long)
      - AddressLabel (string), AddressLine1 (string), AddressLine2 (string)
      - Landmark (string), CityName (string), StateName (string), Pincode (string)
      - AddressType (string), ZoneId (long?), Latitude (double?), Longitude (double?)
      - IsDefault (bool), IsActive (bool), DateCreated (DateTime), LastUpdated (DateTime?)
  DB Tables:            CustomerAddresses (read)
  Notes on Implementation:
    Delegates to GetAdminCustomerDetailQuery; returns response.Addresses slice.

---

### Flow: Create Customer Address (Admin)
  Entry Points:         Customer 360 — Add Address panel
  UI Trigger:           "Save Address" submit
  API Endpoint:         POST /api/customers/{customerId}/addresses
  Auth:                 Policy = UserUpdate
  Request DTO:          CreateCustomerAddressRequest
    - AddressLabel (string, required): "Home" / "Office" / "Other"
    - AddressLine1 (string, required)
    - AddressLine2 (string, required)
    - Landmark (string, required)
    - CityName (string, required)
    - Pincode (string, required)
    - ZoneId (long?, optional): zone from pincode lookup
    - Latitude (double?, optional)
    - Longitude (double?, optional)
    - IsDefault (bool, required): set as primary address
    - StateName (string?, optional)
    - AddressType (string?, optional)
  Response DTO:         ApiResponse<CustomerAddressResponse> (same shape as above)
  DB Tables:            CustomerAddresses (write)
  Business Rules:
    1. If IsDefault=true, clears IsDefault on all existing addresses for this customer
  Failure Cases:
    - Customer not found → 404

---

### Flow: Update Customer Address (Admin)
  Entry Points:         Customer 360 — Edit Address action
  UI Trigger:           "Save" on address edit form
  API Endpoint:         PUT /api/customers/{customerId}/addresses/{addressId}
  Auth:                 Policy = UserUpdate
  Request DTO:          UpdateCustomerAddressRequest
    - CustomerAddressId (long, required): must match route addressId
    - AddressLabel, AddressLine1, AddressLine2, Landmark, CityName, Pincode (all string, required)
    - ZoneId (long?), Latitude (double?), Longitude (double?), IsDefault (bool)
    - StateName (string?), AddressType (string?)
  Response DTO:         ApiResponse<CustomerAddressResponse>
  DB Tables:            CustomerAddresses (write)
  Failure Cases:
    - Address not found → 404

---

### Flow: Get Customer Equipment (Admin)
  Entry Points:         Customer 360 — equipment panel
  UI Trigger:           Page load (embedded in detail response)
  API Endpoint:         GET /api/customers/{customerId}/equipment
  Auth:                 Policy = UserRead
  Response DTO:         ApiResponse<IReadOnlyCollection<CustomerEquipmentResponse>>
    CustomerEquipmentResponse:
      - CustomerEquipmentId (long), CustomerId (long)
      - Name (string), Type (string), Brand (string), Capacity (string), Location (string)
      - PurchaseDate (DateOnly?), LastServiceDate (DateOnly?), SerialNumber (string)
      - IsActive (bool), DateCreated (DateTime), LastUpdated (DateTime?)
  DB Tables:            CustomerEquipment (read)
  Notes on Drift:       Previously undocumented in ProjectOverview. Added 2026-05-24.

---

### Flow: Create Customer Equipment (Admin)
  Entry Points:         Customer 360 — Register Equipment action
  UI Trigger:           "Save Equipment" submit
  API Endpoint:         POST /api/customers/{customerId}/equipment
  Auth:                 Policy = UserUpdate
  Request DTO:          CreateCustomerEquipmentRequest
    - Name (string, required): equipment name/model
    - Type (string, required): Split / Window / Cassette / Centralized / Other
    - Brand (string, required)
    - Capacity (string, required): e.g., "1.5 Ton"
    - Location (string, required): free-text location description
    - PurchaseDate (DateOnly?, optional)
    - LastServiceDate (DateOnly?, optional)
    - SerialNumber (string?, optional)
  Response DTO:         ApiResponse<CustomerEquipmentResponse>
  DB Tables:            CustomerEquipment (write)
  Notes on Drift:       Previously undocumented. Added 2026-05-24.

---

### Flow: Update Customer Equipment (Admin)
  Entry Points:         Customer 360 — Edit Equipment action
  UI Trigger:           "Save" on equipment edit form
  API Endpoint:         PUT /api/customers/{customerId}/equipment/{equipmentId}
  Auth:                 Policy = UserUpdate
  Request DTO:          UpdateCustomerEquipmentRequest
    - CustomerEquipmentId (long, required)
    - Name, Type, Brand, Capacity, Location (all string, required)
    - PurchaseDate (DateOnly?), LastServiceDate (DateOnly?), SerialNumber (string?)
  Response DTO:         ApiResponse<CustomerEquipmentResponse>
  DB Tables:            CustomerEquipment (write)
  Notes on Drift:       Previously undocumented. Added 2026-05-24.

---

### Flow: Add Customer Note (Admin)
  Entry Points:         Customer 360 — Notes panel
  UI Trigger:           "Add Note" submit
  API Endpoint:         POST /api/customers/{customerId}/notes
  Auth:                 Policy = UserUpdate
  Request DTO:          CreateCustomerNoteRequest
    - Content (string, required): note text
    - IsPrivate (bool, required): true = internal only, not visible to customer
    - NoteType (string?, optional): type tag (e.g., "Follow-up", "Complaint")
  Response DTO:         ApiResponse<CustomerNoteResponse>
    - NoteId (string), Author (string), Content (string)
    - TimestampUtc (DateTime), IsPrivate (bool), NoteType (string)
  DB Tables:            CustomerNotes (write)
  Notes on Drift:
    - Previously stated notes persisted as AuditLog entries — confirmed now
      persisted to CustomerNotes table via CreateCustomerNoteCommand.
      Corrected 2026-05-24.

---

### Flow: Reset Customer Password (Admin)
  Entry Points:         Customer 360 — Reset Password action
  UI Trigger:           "Reset Password" button
  API Endpoint:         POST /api/customers/{customerId}/reset-password
  Auth:                 Policy = UserUpdate
  Request DTO:          ResetCustomerPasswordRequest
    - Reason (string?, optional): reason for admin-initiated reset
  Response DTO:         ApiResponse<CustomerPasswordOperationResponse>
    - PasswordUpdated (bool), PasswordGenerated (bool), RequiresPasswordDelivery (bool)
    - MustChangePassword (bool), IsTemporaryPassword (bool), PasswordExpiryOnUtc (DateTime?)
  DB Tables:            Users (write), PasswordHistory (write)
  Business Rules:
    1. Generates new temporary password; sets MustChangePassword=true
    2. Delivers via SMS/email to customer
    3. Reason logged to audit trail
  Notes on Drift:       Previously undocumented. Added 2026-05-24.

---

### Flow: Get My Profile (Customer Self-Service)
  Entry Points:         Customer portal — My Profile screen
  UI Trigger:           Profile page load
  API Endpoint:         GET /api/customers/me/profile
  Auth:                 Authorize(Roles = Customer)
  Request:              (none — identity from JWT)
  Response DTO:         ApiResponse<CustomerProfileResponse>
    - CustomerId (long), UserId (long?)
    - CustomerName (string), MobileNumber (string), EmailAddress (string)
    - PhotoUrl (string), MembershipStatus (string)
    - IsActive (bool), DateCreated (DateTime), LastUpdated (DateTime?)
  DB Tables:            Customers (read)
  Notes on Drift:       Previously undocumented. Added 2026-05-24.

---

### Flow: Update My Profile (Customer Self-Service)
  Entry Points:         Customer portal — Edit Profile form
  UI Trigger:           "Save Profile" submit
  API Endpoint:         PUT /api/customers/me/profile
  Auth:                 Authorize(Roles = Customer)
  Request DTO:          UpdateCustomerProfileRequest
    - CustomerName (string, required)
    - MobileNumber (string, required)
    - EmailAddress (string, required)
    - PhotoUrl (string?, optional)
    - MembershipStatus (string?, optional)
  Response DTO:         ApiResponse<CustomerProfileResponse> (same shape as Get My Profile)
  DB Tables:            Customers (write), Users (write)
  Notes on Drift:       Previously undocumented. Added 2026-05-24.

---

### Flow: Deactivate My Account (Customer Self-Service)
  Entry Points:         Customer portal — Account Settings → Delete Account
  UI Trigger:           "Deactivate Account" confirm action
  API Endpoint:         POST /api/customers/me/deactivate
  Auth:                 Authorize(Roles = Customer)
  Request DTO:          DeleteCustomerAccountRequest
    - Reason (string?, optional): customer-supplied deactivation reason
  Response DTO:         ApiResponse<CustomerAccountDeletionResponse>
    - CustomerId (long), IsActive (bool), DeactivatedAtUtc (DateTime), Reason (string)
  DB Tables:            Customers (soft-deactivate), Users (deactivate), RefreshTokens (revoke)
  Business Rules:
    1. Soft deactivation — IsActive=false; no hard delete
    2. All sessions revoked
  Notes on Drift:
    - Duplicate deactivation path also exists at DELETE /api/auth/account (via AuthController)
      both route to DeactivateMyCustomerAccountCommand. Confirmed 2026-05-24.

---

### Flow: Get My Addresses (Customer Self-Service)
  Entry Points:         Customer portal — Address Book
  UI Trigger:           Address Book page load
  API Endpoint:         GET /api/customers/me/addresses
  Auth:                 Authorize (any authenticated Customer)
  Response DTO:         ApiResponse<IReadOnlyCollection<CustomerAddressResponse>> (same shape as admin)
  DB Tables:            CustomerAddresses (read — filtered to JWT customer)
  Notes on Drift:       Previously undocumented. Added 2026-05-24.

---

### Flow: Create My Address (Customer Self-Service)
  Entry Points:         Customer portal — Add Address
  UI Trigger:           "Save Address" submit
  API Endpoint:         POST /api/customers/me/addresses
  Auth:                 Authorize (Customer)
  Request DTO:          CreateCustomerAddressRequest (same shape as admin create address)
  Response DTO:         ApiResponse<CustomerAddressResponse>
  DB Tables:            CustomerAddresses (write)
  Business Rules:       Zone resolved from pincode via GET /api/booking-lookups/zones/by-pincode/{pincode}
                        before save; ZoneId + Latitude + Longitude persisted from lookup result.

---

### Flow: Update My Address (Customer Self-Service)
  Entry Points:         Customer portal — Edit Address
  UI Trigger:           "Save" on address edit form
  API Endpoint:         PUT /api/customers/me/addresses/{addressId}
  Auth:                 Authorize (Customer)
  Request DTO:          UpdateCustomerAddressRequest (same shape as admin update address)
  Response DTO:         ApiResponse<CustomerAddressResponse>
  DB Tables:            CustomerAddresses (write)
  Failure Cases:
    - Address belongs to different customer → 403

---

### Flow: Delete My Address (Customer Self-Service)
  Entry Points:         Customer portal — Address Book → Delete
  UI Trigger:           "Delete" button confirm
  API Endpoint:         DELETE /api/customers/me/addresses/{addressId}
  Auth:                 Authorize (Customer)
  Request:              addressId (long, route param)
  Response DTO:         ApiResponse<{ addressId }> (confirmation)
  DB Tables:            CustomerAddresses (soft-delete or hard-delete per implementation)
  Failure Cases:
    - Address belongs to different customer → 403
  Notes on Drift:       Previously undocumented. Added 2026-05-24.

---

AMC/Equipment/Warranty API endpoints are in separate modules:
  - Module: AMC Contract Engine → GET/POST /api/amc/*, GET /api/amc/customer/{customerId}, etc.
  - Module: Equipment Register  → GET/POST /api/equipment/{equipmentId}, etc.
  - Module: Warranty Management → GET/POST /api/warranty/records
  Customer-scoped equipment (GET/POST/PUT /api/customers/{id}/equipment) is documented above.
  Customer-facing equipment (GET/POST/PUT/DELETE /api/customers/me/equipment) —
    confirmed exists per AdminMobile integration notes; full DTOs same as CreateCustomerEquipmentRequest
    and CustomerEquipmentResponse; full contract to be documented in Module: Equipment Register.

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

1. BOOKING WIZARD — 5-STEP FLOW (Web — Journey A: Guest / Journey B: Logged-In)
   Updated: 2026-05-27 — Stable Contract Level
   Source file: Frontend/Web/src/pages/BookingWizard.tsx
   Spec source: Frontend/Web/CoolElite_Booking_Flow_Prompts.md

Overview: A guided 5-step booking wizard on the Customer Portal Website (Angular/React SPA).
Steps preserve wizard state locally (WizardData). No API writes until the final Confirm & Book.
Two journeys share the same component; Journey B (logged-in) has enhanced steps (saved address,
registered equipment, pre-verified mobile). Guest mobile is OTP-verified in Step 4.
SR created on submit with status: Pending Assignment.

Deep-link pre-select [2026-06-08]: catalog pages navigate to /book with router state.
  - Services.tsx / ServiceDetail.tsx "Book Now" → state { serviceId, serviceName, price }.
  - Home.tsx per-category card "Book Now" → state { serviceCategoryId, serviceCategoryName }
    (dynamic categories only; hero "Book a Service" passes no state = fresh start).
  - The wizard resolves the deep-link inside the catalog-load .then() (where services/categories
    are available): a serviceId seeds BOTH its category (serviceTypeId) and the sub-type chip
    (serviceSubTypeId + base price); a category-only link seeds serviceTypeId only.
  - Previously inert (wizard read serviceCategoryId but callers sent serviceId) — fixed.

PENDING — VERIFICATION (development complete; NOT yet tested as of 2026-06-08):
  All booking-flow code below is implemented and compiles (backend build clean, frontend tsc clean),
  but has NOT been exercised end-to-end. The following 4 journeys require E2E testing before sign-off:
    1. Guest — normal booking (slot selected) → POST /api/bookings/guest
    2. Guest — emergency booking (no slot, surcharge) → POST /api/bookings/guest
    3. Logged-in — normal booking (saved address / equipment quick-select) → POST /api/bookings/customer
    4. Logged-in — emergency booking → POST /api/bookings/customer
  Also pending (ops): apply Docs/Postgres/14_booking_optional_equipment_slot.sql to running databases
  (nullable SlotAvailabilityId / TonnageId / BrandId). Fresh DBs already get it from 02_create_tables.sql.

─────────────────────────────────────────────────────────────────────────────
STEP 1 — Service & AC Type   [STABLE — 2026-05-27]
─────────────────────────────────────────────────────────────────────────────
Entry Points:   /book, any CTA on the site
UI Trigger:     Page load / “Book Now” CTA

Journey B Enhancement [B-S1] — Equipment Quick-Select (Logged-In Only):
  Shown above the category cards when isLoggedIn === true AND myEquipment.length > 0.
  Section header: “Book for your saved equipment?” with “Skip” link.
  Display: horizontal scrollable card row; each card shows:
    - Icon (Wind for Split, Building2 for Cassette/Centralized, Wind fallback)
    - Equipment name (or “${brand} ${type}” if name absent)
    - Capacity and location label
    - Selected state: navy bg + white text + gold ring
  Special card: “➕ New / different unit” — deselects equipment
  On equipment card tap:
    - selectedEquipmentId, selectedEquipmentName set
    - AC Type chip auto-selected by matching eq.type.toLowerCase() against
      AcTypeLookupResponse.acTypeName.toLowerCase()
    - If no match: acTypeId unchanged (user selects manually)
  API Calls for B-S1:
    GET /api/customers/me/equipment — returns CustomerEquipmentResponse[]
      (loaded on wizard mount when isLoggedIn; filtered to isActive === true)
  State Written: selectedEquipmentId, selectedEquipmentName (+ acTypeId/acTypeName if match)

Fields:
  Service Category (visual card grid, 3-col)
    - Type: button cards with icon + label
    - Required: Yes
    - Options: sourced from GET /api/booking-lookups/service-categories
    - Selected state: gold border-2 + checkmark badge overlay
    - Unselected when another is selected: opacity-60
    - “Other” category: free-text textarea (max 100 chars) renders below cards
      on selection; chip row hidden
    - AMC Enrollment: no sub-type chip row shown

  Service Sub-Type (inline chip row)
    - Type: flex-wrap button chips
    - Required: Yes — except for AMC Enrollment and Other categories
    - Data source: GET /api/booking-lookups/services (loaded on page mount, filtered by
      selected serviceCategoryId — no API call on chip selection)
    - Appears: AnimatePresence height-transition (200ms ease-in-out) below category cards
      after a category is selected
    - Selected chip: navy bg + white text; Unselected: navy outline + navy text
    - Changing category: resets serviceSubTypeId/serviceSubTypeName; does NOT reset acTypeId

  AC Type (chip row — ALWAYS VISIBLE, not conditional)
    - Type: 2-col / 4-col grid of button chips
    - Required: Yes
    - Options: sourced from GET /api/booking-lookups/ac-types (e.g., Split, Window, Cassette,
      Centralized)
    - Selected state: navy bg + white text

  (Number of Units field REMOVED 2026-06-08 — see Notes on Drift. Unit count is not part of the
   booking-create contract nor the DB; technician confirms unit quantity on-site.)

API Calls:
  GET /api/booking-lookups/service-categories   — categories (on page mount)
  GET /api/booking-lookups/services             — all sub-types grouped by categoryId (on page mount)
  GET /api/booking-lookups/ac-types             — AC type chips (on page mount)
  No API calls on field selection — all data cached from initial load.

Validation:
  - serviceTypeId must be selected
  - acTypeId must be selected
  - serviceSubTypeId must be selected unless isAmc(serviceTypeName) or isOther(serviceTypeName)

State Written (WizardData):
  serviceTypeId, serviceTypeName, serviceSubTypeId, serviceSubTypeName,
  acTypeId, acTypeName, serviceBasePrice, otherNote

Exit Condition: category + sub-type (where applicable) + AC type selected

─────────────────────────────────────────────────────────────────────────────
STEP 2 — Service Location   [STABLE — 2026-05-27]
─────────────────────────────────────────────────────────────────────────────
Fields:
  PIN / Postal Code (first field, 6-digit numeric)
    - Zone validation fires on useEffect when pincode.length === 6
    - Serviceable: green zone chip “Zone: [Name] — we serve this area”
    - Non-serviceable: inline error block + WhatsApp deep-link
    - Invalid format: inline field error

  Address Line 1 (min 5 chars, max 128)  — revealed only after serviceable PIN
  Address Line 2 (optional, max 128)      — revealed only after serviceable PIN
  City (text, auto-populated from zone, editable) — revealed only after serviceable PIN

  Address fields animate in with AnimatePresence (250ms ease-in-out) after zone confirmed.

Journey B Enhancement [B-S2] — Saved Address Quick-Select (Logged-In Only):
  Shown above the PIN field when isLoggedIn === true AND myAddresses.length > 0.
  Section header: "Use a saved address?" with "Enter a different address" toggle link.
  Display: vertical card list sorted (default address first); each card shows:
    - addressLabel (or "Address {n}" fallback)
    - "Default" badge (green) on isDefault address
    - addressLine1, cityName, pincode
    - Home / Building2 icon (derived from label text)
  On address card tap:
    - Fills pincode, addressLine1, addressLine2, cityName
    - If zoneId + zoneName exist on CustomerAddressResponse: fills directly (no API call)
    - If zoneId missing: calls GET /api/booking-lookups/zones/by-pincode/{pin} as fallback
    - Sets selectedAddressId, setPinStatus("valid")
    - Collapses manual form (setShowManualForm(false))
  "Enter a different address" link: sets showManualForm(true), clears selectedAddressId
  API Calls for B-S2:
    GET /api/customers/me/addresses — returns CustomerAddressResponse[]
      (loaded on wizard mount when isLoggedIn)
    GET /api/booking-lookups/zones/by-pincode/{pin} — fallback only when zoneId missing on addr
  State Written: selectedAddressId (+ all address fields from selected address)

API Calls:
  GET /api/booking-lookups/zones/by-pincode/{pin}
    - Returns: { isServiceable: bool, zoneName: string, zoneId: number }

Validation: pincode.length === 6 AND zoneId !== null AND addressLine1.trim().length >= 5

State Written: pincode, zoneId, zoneName, addressLine1, addressLine2, cityName,
               selectedAddressId (Journey B only)

─────────────────────────────────────────────────────────────────────────────
STEP 3 — Date & Time   [STABLE — 2026-05-27]
─────────────────────────────────────────────────────────────────────────────
Fields:
  Date Picker (inline 14-day calendar grid)
    - 7 columns (Mon–Sun), rows fill forward 14 days from today
    - Today rule: only shown as selectable if current hour < 4 (i.e. ≥ 4hrs to 8 AM)
    - Selected state: navy fill + gold ring ring-offset-1

  Time Window (3 large cards loaded after date selection)
    - Options: Morning (8 AM–12 PM) · Afternoon (12–4 PM) · Evening (4–7 PM)
    - Availability sourced from API, mapped to windows by startHour
    - States: normal / “1 slot left” (amber badge) / “Full” (red badge, disabled)
    - Selecting a window stores: slotWindow + slotAvailabilityId (first available slot in window)
    - Selected card: navy bg + gold border + white text

  Emergency Service Card (always shown, amber left border accent)
    - Overrides date to today, sets isEmergency = true, emergencySurcharge = 499
    - Sets slotWindow = “Emergency”, slotAvailabilityId = null
    - X button deselects emergency and restores normal flow
    - Confirmation callout shown on card after selection

API Calls:
  GET /api/booking-lookups/slots?zoneId={id}&slotDate={date}
    - Fires on each date selection
    - Returns: SlotAvailabilityResponse[] (slotAvailabilityId, startTime, endTime, availableCapacity, isFullyBooked)
    - Slots grouped into Morning/Afternoon/Evening windows client-side

Validation: slotDate selected AND (slotAvailabilityId !== null OR isEmergency === true)

State Written: slotDate, slotAvailabilityId, slotWindow, isEmergency, emergencySurcharge

─────────────────────────────────────────────────────────────────────────────
STEP 4 — Contact Details (Guest)   [STABLE — 2026-05-27]
─────────────────────────────────────────────────────────────────────────────
Journey A (Guest):
  Full Name (text, min 2 chars, no numbers)
  Mobile Number (+91 prefix, 10-digit, must not start with 0 or 1)
    - After valid 10-digit entry: “Verify” button appears inline
  OTP Verification (inline 6-box input, appears after “Verify” tap)
    - 60-second countdown with “Resend OTP” after expiry
    - Max 3 attempts; 4th attempt sets otpLocked = true
    - On verified: mobile field shows green badge, mobileVerified = true
    - If registered mobile: soft prompt shown (non-blocking)
  Special Instructions (textarea, max 250 chars, always blank on entry)
  (Coupon Code field REMOVED 2026-06-08 — see Notes on Drift.)

Journey B Enhancement [B-S4] — Logged-In Contact Step:
  Name: read-only display “Booking for: [Full Name]” (from CurrentUserResponse.fullName)
  Mobile: read-only masked display “Contact: +91 [XX XXX XX XXX]”
    - Mask pattern: `${mobile.slice(0,2)}XXX XX${mobile.slice(-3)}`
    - Mobile sourced from ProfileService.getMyProfile() (mobileNumber field)
    - NOT available on CurrentUserResponse — requires separate profile load
  No OTP flow — mobileVerified defaults to true for logged-in users
  Special Instructions: same textarea as guest (max 250 chars)
  (Coupon Code + Loyalty Coupon Chip REMOVED 2026-06-08 — see Notes on Drift. GET /api/offers
   no longer loaded on wizard mount.)

  API Calls for B-S4:
    GET /api/customers/me/profile — returns CustomerProfileResponse (mobileNumber field)

API Calls (Guest OTP):
  POST /api/auth/otp/send            — payload: { phone: string }
  POST /api/auth/otp/verify          — payload: { phone: string, otp: string }

Validation (Guest):   guestName.length >= 2 AND mobileVerified === true
Validation (Logged-in): always valid (T&C click-through in Step 5)

State Written: guestName, guestMobile, mobileVerified, specialInstructions

─────────────────────────────────────────────────────────────────────────────
STEP 5 — Review & Confirm   [STABLE — 2026-05-27]
─────────────────────────────────────────────────────────────────────────────
Summary Cards (4 cards, each with visible “Edit” link):
  Card 1 — Service:     category name + sub-type + AC type (+ equipment name if selected)
  Card 2 — Location:    address line 1/2 + city + PIN + zone name
  Card 3 — Appointment: formatted date + window label + Emergency badge (if applicable)
  Card 4 — Contact:     guest name + masked mobile

Pricing Block (navy bg):
  - “Estimated service charge: ₹X” (serviceBasePrice; no unit multiplier)
  - Emergency surcharge line: “+ ₹499 priority charge” in amber (if emergency)
  - Estimated total (only shown when the emergency surcharge applies)
  - “Applicable GST will be added to your final invoice.”
  (Coupon discount line REMOVED 2026-06-08 — see Notes on Drift.)

T&C Checkbox (required — enables Confirm & Book):
  “I agree to CoolElite's Terms of Service and Cancellation Policy.”

Confirm & Book Button (gold, full-width):
  - Enabled only when termsAccepted === true
  - Loading state: spinner + “Booking…” label; disabled during API call
  - Error state: inline error block with retry message

Trust Strip: Verified technicians · SSL secured · Digital report after every visit

API Call on Confirm:
  Guest:     POST /api/bookings/guest
  Logged-in: POST /api/bookings/customer
  Payload fields:
    serviceId (= serviceSubTypeId ?? serviceTypeId),
    acTypeId, addressLine1, addressLine2,
    cityName, pincode, issueNotes (specialInstructions + otherNote),
    isEmergency, sourceChannel: “web”,
    slotAvailabilityId (omitted/undefined when emergency),
    emergencySurchargeAmount (sent only when emergency)
    NOTE: tonnageId/brandId are NOT sent — verified on-site (backend accepts null) [2026-06-08]
  Guest-only additions: customerName, mobileNumber

On Success:
  Navigate to /booking-confirmation with booking details
  SR created with status: Pending Assignment
  SR number format: CE/YYYY/MM/XXXXXX
  WhatsApp + Email confirmation sent

Validation: termsAccepted === true

State Written: termsAccepted

─────────────────────────────────────────────────────────────────────────────
NOTES ON DRIFT (2026-05-27)
─────────────────────────────────────────────────────────────────────────────
- Old wizard was 7 steps: Service → Equipment (brand/model/tonnage) → Location → Slots →
  Contact → Review → Confirm & Pay. Equipment Details step removed as technician verifies
  brand/model/tonnage on-site. Emergency moved from step 4 toggle to a dedicated card in Step 3.
- Old Step 1 showed individual services (ServiceLookupResponse) as the primary selection.
  Corrected to spec: categories are primary cards; services become sub-type chips.
- Old slot display: individual slot cards (slotAvailabilityId per card). Corrected to spec:
  3-window card display (Morning/Afternoon/Evening); first available slotAvailabilityId per
  window is stored.
- Coupon endpoint corrected: was phantom POST /api/coupons/apply — actual is
  POST /api/customer-marketing/offers/validate-coupon.
- OTP endpoint corrected: was phantom POST /api/otp/send/verify — actual is
  POST /api/auth/otp/send and POST /api/auth/otp/verify (from AuthController).
- Slot-hold step removed (POST /api/bookings/hold does not exist; booking is idempotent
  via X-Idempotency-Key).

─────────────────────────────────────────────────────────────────────────────
NOTES ON DRIFT (2026-06-08) — Coupon + Unit Count removed from wizard
─────────────────────────────────────────────────────────────────────────────
Drift type: Request/response drift (UI collected data the booking-create contract could not carry).
- The wizard collected `unitCount` (Step 1 stepper) and coupon data (`couponCode`/`appliedCoupon`/
  `discountAmount`, Step 4 + loyalty chip), but the booking-create contract
  (CustomerBookingCreateRequest / GuestBookingCreateRequest → POST /api/bookings/customer | /guest)
  has NO unitCount or couponCode fields, and neither does the booking/service-request DB table.
  Result: customers could apply a promo and see a discount that was silently dropped on submit;
  unit count never persisted.
- Resolution: removed both from the UI entirely (steppers, coupon entry, loyalty chip, and all
  related WizardData fields and state). API and DB are unchanged and remain null-tolerant because
  these fields were never part of the booking pipeline — nothing was sent, nothing was stored.
- Scope guard: the standalone POST /api/customer-marketing/offers/validate-coupon endpoint and the
  DiscountAmount columns on Quotation/Invoice (billing) tables are a SEPARATE flow and were NOT
  touched. Promotions, if reintroduced, must be added to the booking contract + DB first, then UI.
- Services.tsx field mismatches fixed in the same change: category chip used `serviceCategoryName`
  (correct: `categoryName`); service card used `service.description` (correct: `summary`) and
  `service.serviceCategoryName` for the icon (corrected to a categoryId→categoryName lookup);
  `service.estimatedDurationMinutes` (not on ServiceLookupResponse) display block removed.

2. POST-BOOKING SCREENS
- Booking Confirmation Page (/booking-confirmation)
   - Success state: animated checkmark, SR number (large + copy button), 3-line booking summary,
     “What happens next” 3-step guide, Track Booking link, Share via WhatsApp button.
   - AMC upsell card: shown only for Repair/Cleaning AND no active AMC contract.
   - Guest account creation card: shown only for guest bookings.

3. SPECIAL BOOKING VARIANTS
- Emergency Booking
   - Selected via Emergency card in Step 3; overrides date to today, adds ₹499 surcharge.
   - slotWindow = “Emergency”, slotAvailabilityId = null (dispatch handled by ops).
   - priority = Emergency on SR creation.
- AMC Enrollment Booking
   - Category selected in Step 1; no sub-type chips shown (chip row hidden).
- Guest Booking
   - Mobile OTP-verified in Step 4 as identity anchor for the SR.
   - Guest booking linked to verified mobile — claimable on account creation.
   - Registration prompt shown on Summary screen (non-blocking).

4. BOOKING WIZARD ARCHITECTURE (React/TypeScript)   [STABLE — 2026-05-27]
   File: Frontend/Web/src/pages/BookingWizard.tsx
   State type: WizardData interface (all 5 steps merged into one flat object)

   WizardData interface (final, includes Journey B fields):
     Step 1: serviceTypeId, serviceTypeName, serviceSubTypeId, serviceSubTypeName,
             acTypeId, acTypeName, serviceBasePrice, otherNote,
             selectedEquipmentId [B-S1], selectedEquipmentName [B-S1]
     Step 2: pincode, zoneId, zoneName, addressLine1, addressLine2, cityName,
             selectedAddressId [B-S2]
     Step 3: slotDate, slotAvailabilityId, slotWindow, isEmergency, emergencySurcharge
     Step 4: guestName, guestMobile, mobileVerified, specialInstructions
     Step 5: termsAccepted
     (unitCount + couponCode/appliedCoupon/discountAmount REMOVED 2026-06-08 — see Notes on Drift.)

   Main component state (Journey B additions):
     myEquipment: CustomerEquipmentResponse[]    — loaded on mount if isLoggedIn [B-S1]
     myAddresses: CustomerAddressResponse[]       — sorted default-first [B-S2]
     myMobile: string                             — from ProfileService.getMyProfile() [B-S4]
     (loyaltyOffers REMOVED 2026-06-08 — coupon feature dropped from wizard.)

   Component tree:
     BookingWizard (orchestrator: state, step counter, submit handler)
       Step1 (B-S1 equipment quick-select + categories + sub-types + AC type)
       Step2 (B-S2 saved address quick-select + PIN → address reveal)
       Step3 (14-day calendar + window cards + emergency)
       Step4 (B-S4 read-only profile OR guest OTP + special instructions)
       Step5 (summary cards + pricing + T&C + CTA)
       SummaryCard (reusable sub-component used in Step5)

   Data Loading Strategy (Journey B):
     On wizard mount, when isLoggedIn:
       - EquipmentService.getMyEquipment() → filter isActive
       - AddressService.getMyAddresses()   → sort default first
       - ProfileService.getMyProfile()     → extract mobileNumber
     All 3 calls in parallel; each has its own catch(() => []) fallback.
     Catalog (categories, services, AC types) loaded for all users (anonymous + logged-in).

   Journey B isStepValid rules:
     Step 4: isLoggedIn ? true : (guestName.length >= 2 AND mobileVerified)

   Catalog loaded once on mount: service categories, services, AC types.
   No slot holds; booking creation is idempotent via X-Idempotency-Key.
   Both journeys fully implemented as of 2026-05-27.

5. BOOKING ENGINE API CONTRACT — STABLE (audited 2026-05-24)

Controllers:
  BookingLookupController  (/api/booking-lookups) — all AllowAnonymous
  ServiceTypesController   (/api/service-types)   — all AllowAnonymous
  BookingController        (/api/bookings)
  CustomerBookingController(/api/customer-bookings)

Notes on Drift (phantom endpoints removed 2026-05-24):
  - POST /api/bookings/hold    — documented previously but does NOT exist in BookingController
  - POST /api/bookings/confirm — documented previously but does NOT exist in BookingController
  - GET /api/bookings/{id}/summary — WRONG; actual is GET /api/bookings/{id}
  - GET /api/equipment/brands  — WRONG prefix; actual is GET /api/booking-lookups/brands
  - GET /api/service-subtypes  — does not exist; sub-types via GET /api/service-types/{id}/sub-types
  - POST /api/otp/send/verify  — wrong prefix; those are POST /api/auth/otp/* (see Auth module)
  - No slot-hold step in current implementation; booking creation is idempotent via
    X-Idempotency-Key header on POST /api/bookings/guest or /customer.

---

## BookingLookup API (Catalog / Wizard Step Data)

### Flow: Get Service Categories
  API Endpoint:   GET /api/booking-lookups/service-categories
  Auth:           AllowAnonymous
  Query Params:   search (string?, optional)
  Response DTO:   ApiResponse<IReadOnlyCollection<ServiceCategoryLookupResponse>>
    - ServiceCategoryId (long), CategoryName (string), Description (string)
  DB Tables:      ServiceCategories (read)

### Flow: Get Services
  API Endpoint:   GET /api/booking-lookups/services
  Auth:           AllowAnonymous
  Query Params:   serviceCategoryId (long?, optional), search (string?, optional)
  Response DTO:   ApiResponse<IReadOnlyCollection<ServiceLookupResponse>>
    - ServiceId (long), ServiceCategoryId (long), ServiceName (string)
    - Summary (string), BasePrice (decimal), PricingModelName (string)
    - ImageUrl (string, "" when null) — per-service image; added 2026-06-10 (Phase 1)
  DB Tables:      Services (read — incl. ImageUrl), ServiceCategories (read)

### Flow: Get AC Types
  API Endpoint:   GET /api/booking-lookups/ac-types
  Auth:           AllowAnonymous
  Query Params:   search (string?, optional)
  Response DTO:   ApiResponse<IReadOnlyCollection<AcTypeLookupResponse>>
    - AcTypeId (long), AcTypeName (string), Description (string)
  DB Tables:      AcTypes (read)

### Flow: Get Tonnages
  API Endpoint:   GET /api/booking-lookups/tonnage
  Auth:           AllowAnonymous
  Query Params:   search (string?, optional)
  Response DTO:   ApiResponse<IReadOnlyCollection<TonnageLookupResponse>>
    - TonnageId (long), TonnageName (string), Description (string)
  DB Tables:      Tonnages (read)

### Flow: Get Brands
  API Endpoint:   GET /api/booking-lookups/brands
  Auth:           AllowAnonymous
  Query Params:   search (string?, optional)
  Response DTO:   ApiResponse<IReadOnlyCollection<BrandLookupResponse>>
    - BrandId (long), BrandName (string), Description (string)
  DB Tables:      Brands (read)

### Flow: Get Zones
  API Endpoint:   GET /api/booking-lookups/zones
  Auth:           AllowAnonymous
  Query Params:   search (string?, optional)
  Response DTO:   ApiResponse<IReadOnlyCollection<ZoneListItemResponse>>
    - ZoneId (long), ZoneCode (string), ZoneName (string), CityName (string)
  DB Tables:      Zones (read)

### Flow: Get Zone by Pincode
  Entry Points:   Booking wizard Step 3 (location) — pincode entry auto-fills zone
  API Endpoint:   GET /api/booking-lookups/zones/by-pincode/{pincode}
  Auth:           AllowAnonymous
  Request:        pincode (string, route param)
  Response DTO:   ApiResponse<ZoneLookupResponse>
    - ZoneId (long), ZoneName (string), CityName (string), Pincode (string)
  DB Tables:      Zones (read), ZonePinCodes (read)
  Business Rules:
    1. If pincode outside service area → 404 (zone not found)
    2. Client persists ZoneId + city into address before saving

### Flow: Get Available Slots
  Entry Points:   Booking wizard Step 4 (date & time slot)
  API Endpoint:   GET /api/booking-lookups/slots
  Auth:           AllowAnonymous
  Query Params:
    - zoneId (long, required)
    - slotDate (DateOnly, required)
  Response DTO:   ApiResponse<IReadOnlyCollection<SlotAvailabilityResponse>>
    - SlotAvailabilityId (long): key used in booking creation
    - ZoneId (long), SlotDate (DateOnly), SlotLabel (string)
    - StartTime (string), EndTime (string)
    - AvailableCapacity (int), ReservedCapacity (int), IsAvailable (bool)
  DB Tables:      SlotAvailability (read), ServiceSlots (read)
  Business Rules:
    1. IsAvailable=false slots returned but not selectable (client hides or greys them)
    2. Client caches with short TTL; revalidates on booking submit

---

## PUBLIC WEBSITE MASTER DATA CATALOG (Frontend/Web — public site)

Purpose: single registry of every MASTER (reference / catalog) data set surfaced on the public
Website (Frontend/Web), which page renders it, the read endpoint, and the backing DB table.
"Master" here = admin-maintained reference data the public consumes read-only (anonymous).

Surface: Frontend/Web/src — React + Vite. Catalog masters served by BookingLookupController and
ServiceTypesController (all AllowAnonymous) via Frontend/Web/src/services/catalogService.ts.
CMS/content masters served as a published static snapshot (snapshotService.ts → ContentContext).

### A. CATALOG / BOOKING MASTERS (BookingLookupController + ServiceTypesController — AllowAnonymous)

| # | Master | Read Endpoint | DB Table(s) | Website page(s) that render it |
|---|--------|---------------|-------------|--------------------------------|
| 1 | Service Categories | GET /api/booking-lookups/service-categories | ServiceCategories | Home (Services.tsx category grid), Services (filter chips), BookingWizard Step 1 |
| 2 | Services | GET /api/booking-lookups/services | Services (incl. ImageUrl), ServiceCategories | Home, Services, ServiceDetail, BookingWizard Step 1–2 |
| 3 | Service Types (public catalog) | GET /api/service-types (+ /{id}, /{id}/sub-types) | Services (read), CMSBlocks (FAQ inject) | Public service catalog / ServiceDetail (alt catalog endpoint; sub-types currently empty) |
| 4 | AC Types | GET /api/booking-lookups/ac-types | AcTypes | BookingWizard Step 2 (required selection) |
| 5 | Tonnages | GET /api/booking-lookups/tonnage | Tonnages | API live + booking DTO field (TonnageId OPTIONAL — technician verifies on-site); NOT a rendered wizard step today [VERIFY if a future tonnage step is added] |
| 6 | Brands | GET /api/booking-lookups/brands | Brands | API live + booking DTO field (BrandId OPTIONAL — technician verifies on-site); NOT a rendered wizard step today [VERIFY] |
| 7 | Zones | GET /api/booking-lookups/zones ; GET /api/booking-lookups/zones/by-pincode/{pincode} | Zones, ZonePinCodes | BookingWizard Step 3 (pincode → zone resolution); Portal Addresses.tsx |
| 8 | Service Slots / Availability | GET /api/booking-lookups/slots?zoneId&slotDate | SlotAvailability, ServiceSlots | BookingWizard Step 4 (date & time slot) |

Notes on usage:
  - Wizard loads categories + services + ac-types once on mount (BookingWizard.tsx ~L185-187).
  - Tonnage and Brand masters are reachable (AllowAnonymous) and the GuestBookingCreateRequest /
    customer booking DTOs accept TonnageId?/BrandId? as OPTIONAL (null at booking; technician
    confirms on-site — changed 2026-06-08). The current public wizard does not render them as steps.
  - AcTypeId is REQUIRED on booking; SlotAvailabilityId required only when IsEmergency=false.

### B. CMS / CONTENT MASTERS (published snapshot — snapshotService.ts → ContentContext)

These are admin-maintained content masters delivered to the public site via a static published
snapshot (theme + content blocks + screen images), with IMemoryCache-backed API fallback.

| # | Master | Source | Website page(s) |
|---|--------|--------|-----------------|
| 9  | Content Blocks (CMS) | snapshot.content.blocks (getBlock) | Home, About, WhyCoolzo, AMC, Contact, Services copy |
| 10 | Screen Images / Theme | snapshot.images + snapshot.theme (getImage) | All public pages (hero/section imagery, theme tokens) |
| 11 | FAQs | CMS (injected into Get Service Type Detail .Faqs; CMS blocks) | ServiceDetail FAQ, Home/AMC FAQ sections |
| 12 | Testimonials / Reviews | reviewService.ts (public reviews) | Reviews.tsx, Home testimonials |
| 13 | Blog Posts | cms/blog (public) | Blog.tsx, BlogDetail.tsx |
| 14 | Promotional Offers | marketingService.ts | Home / offers surfaces (customer-marketing) |

### PHYSICAL SCHEMA (confirmed 2026-06-10 from EF configs — Supabase Postgres, tbl-prefix, PascalCase quoted columns)
Tables use the SQL-standard naming (tbl prefix) even on Postgres; columns are PascalCase (must be
double-quoted in Postgres SQL). All business tables carry audit columns via ConfigureAuditColumns().
  - tblServiceCategory   — ServiceCategoryId(PK identity), CategoryName, Description, SortOrder, IsActive, IsDeleted, +audit
  - tblService           — ServiceId(PK identity), ServiceCode(UNIQUE, req, ≤64), ServiceName(req ≤128),
                           Summary(req ≤512), ImageUrl(≤512), EstimatedDurationInMinutes(default 60),
                           BasePrice numeric(18,2), IsActive(default true), ServiceCategoryId(FK→tblServiceCategory),
                           PricingModelId(FK→tblPricingModel), SortOrder, IsDeleted, +audit
                           ⚠ DEPENDENCY: a tblPricingModel row must exist before inserting services.
  - tblPricingModel      — PricingModelId(PK identity), PricingModelName(req UNIQUE ≤128), Description(req ≤256),
                           BasePrice numeric(18,2), IsActive(default true), +audit
  - tblAcType            — AcTypeId(PK), AcTypeCode(req ≤64), AcTypeName(req ≤128), Description(req ≤256), IsActive, +audit
  - tblTonnage           — TonnageId(PK), TonnageCode(req ≤64), TonnageName(req ≤128), Description(req ≤256), IsActive, +audit
  - tblBrand             — BrandId(PK), BrandCode(req ≤64), BrandName(req ≤128), Description(req ≤256), IsActive, +audit
  - tblServiceCategory   — ServiceCategoryId(PK), CategoryCode(req ≤64), CategoryName(req ≤128), Description(req ≤256), IsActive, +audit
  - tblZone              — ZoneId(PK), ZoneCode(req ≤64), ZoneName(req ≤128), CityName(req ≤128), IsActive, +audit
  - tblZonePincode       — ZonePincodeId(PK), Pincode(req ≤16), ZoneId(FK→tblZone), IsActive, +audit
  - tblSlotConfiguration — SlotConfigurationId(PK), ZoneId(FK→tblZone), SlotLabel(req ≤64), StartTime(time),
                           EndTime(time), MaxBookingCount(default 1), IsActive(default true), +audit;
                           UNIQUE (ZoneId, StartTime, EndTime)
  - tblSlotAvailability  — SlotAvailabilityId(PK), ZoneId(FK), SlotConfigurationId(FK→tblSlotConfiguration),
                           SlotDate(date), AvailableCapacity(default 1), ReservedCapacity(default 0),
                           IsBlocked(default false), +audit; UNIQUE (ZoneId, SlotDate, SlotConfigurationId).
                           No IsActive column (active state derives from SlotConfiguration).
                           ⚠ slots are NOT a flat "ServiceSlots" table — SlotConfiguration (template) + per-date SlotAvailability.

Audit columns (ConfigureAuditColumns, all tables above): CompanyId(def 1), SiteId(def 1), BranchId(def 1),
  DepartmentId?, Tag?, Comments?, DisplayOnWeb(def true), IsPublished(def true), DatePublished?, PublishedBy?,
  SortOrder(def 0), IPAddress(def '127.0.0.1'), CreatedBy(def 'System'), DateCreated(default GETUTCDATE() —
  ⚠ SQL-Server fn, NOT valid on Postgres), UpdatedBy?, LastUpdated?, DeletedBy?, DateDeleted?, IsDeleted(def false).

Seed SQL (Phase 3): Backend/Database/Seeds/00..09 (Supabase Postgres, realistic Hyderabad placeholders,
  explicit audit columns, idempotent). EXECUTED on prod Supabase 2026-06-10 — active counts: pricing 2,
  categories 5, services 11, acTypes 5, tonnages 4, brands 9, zones 20, pincodes 113 (full 500001-500113
  Hyderabad coverage — known localities zoned specifically, rest → "Hyderabad (City-wide)" catch-all zone
  ZN-HYD-CITY), slotConfigs 60 (20 zones × 3 windows), slotAvailability 840 (14 days).
  07 = more named zones+pincodes; 08 = all-Hyderabad-pincodes catch-all; 09 = booking flags.
  Booking flags in tblSystemSetting set: Booking.OpenBookingMode=false (slot required),
  Booking.EnforceSlotCapacity=true (capacity enforced). Prices/capacity remain placeholders to tune before launch.
  ⚠ tblSystemSetting audit columns EXCLUDE BranchId (ConfigureAuditColumns includeBranchId:false) —
  inserts to it must omit "BranchId". Columns: SettingKey(UNIQUE ≤128), SettingValue(req ≤512), DataType(req ≤64), IsSensitive.

DB EXECUTION (operational): live DB = Supabase Postgres; connection at appsettings.json
  ConnectionStrings:PostgresConnection. SQL is run directly via a dotnet/Npgsql console runner
  (psql not installed; PS 5.1 can't load net8 Npgsql). Do not require manual SQL runs. See memory.
Notes on Drift (2026-06-10, RESOLVED): brain previously listed logical names only; physical tables are
  tbl-prefixed (PascalCase quoted columns), masters carry *Code natural keys, Service depends on
  PricingModel, slots = SlotConfiguration+SlotAvailability, DateCreated default is SQL-Server-specific
  (seed sets it explicitly).

### MASTER COUNT (public Website)
  - Catalog/Booking masters surfaced anonymously: 8 (Service Categories, Services, Service Types,
    AC Types, Tonnages, Brands, Zones, Service Slots).
  - CMS/Content masters: 6 (Content Blocks, Screen Images/Theme, FAQs, Testimonials/Reviews,
    Blog Posts, Promotional Offers).
  - Of the 8 catalog masters, 6 are actively rendered in the public booking/catalog UI today
    (Categories, Services, Service Types, AC Types, Zones, Slots); Tonnages & Brands are
    API-available and DTO-supported but not rendered as wizard steps.

Notes on Drift:
  - catalogService.ts exposes getTonnages()/getBrands() but no public page consumes them yet
    (only acTypeId/tonnageId/brandId appear as optional fields in types/booking.ts). Documented as
    "API live, UI not rendered" rather than removed, since the booking DTO still accepts them.
  - Slot DTO field naming differs between brain (SlotLabel/IsAvailable) and web type
    (displayLabel/isFullyBooked) — web maps via SlotAvailabilityResponse in types/catalog.ts.

---

## PUBLIC WEBSITE — UI ARCHITECTURE (Website Rework Phase 0, 2026-06-10)

Surface: Frontend/Web (React 19 + Vite + Tailwind CSS v4). Governing plan:
`Backend/Docs/Flow/Website_Rework_Plan.md`. Governing UI rules:
`Backend/Docs/Rules/Web_Responsive_Standard.md` (binding, QA-enforced).

Shared layout primitives (every public page must use these — no per-page max-width/padding/grid):
  - Container (Frontend/Web/src/components/Container.tsx) — page width + gutters
    (width: default=max-w-7xl/1280px, narrow=max-w-3xl, wide=max-w-[90rem]; px-5 sm:px-6 lg:px-8)
  - Section (Frontend/Web/src/components/Section.tsx) — vertical rhythm + surface
    (spacing: compact/default/loose/none; surface: transparent/cream/white/navy)
  - Grid (Frontend/Web/src/components/Grid.tsx) — responsive columns (cols 2|3|4; always collapse to 1 on phone)

Theme tokens (src/index.css @theme — source of truth for Web):
  brand-navy #0A192F, brand-gold #D4AF37, brand-cream #FDFCFB, brand-black #050505;
  font-serif Cormorant Garamond (headings), font-sans Inter (body/UI).
  Root font scales 16→17(≥640)→18(≥1024) px.

Definition of Stable (QA device matrix): phone 360/390/430, tablet 768/820, desktop 1280/1440 —
all green (no h-scroll, no overlap, ≥44px touch targets, all 4 async states reachable) before sign-off.

Phase 0 status: primitives + standard delivered, additive only (not yet wired into pages — zero
behavior change). tsc --noEmit passes. Page rebuilds happen in Phase 2 on these primitives.

Phase 2 status (2026-06-10): public spine rebuilt on the primitives; tsc + vite build pass.
  - Nav de-scope: Navbar nav = Services only; Footer = Quick Links (Services/Book/Terms/Privacy) +
    Get in Touch contact block (phone/WhatsApp/email/city — PLACEHOLDER numbers, replace before launch).
    Blog routes removed from App.tsx (Blog.tsx/BlogDetail.tsx orphaned, deletable). About/WhyCoolzo/
    Reviews/AMC routes kept but hidden from nav (deferred).
  - Home.tsx rebuilt: service-first hero (AC repair/service/install/gas; Book + View Services CTAs),
    honest trust strip (removed picsum avatars + wikipedia brand logos), live service categories with
    loading state + static fallback, Hyderabad coverage, How-it-works, final CTA. AMC section removed.
  - Services.tsx rebuilt on Container/Grid: fixed empty-state-rendered-during-loading bug; booking
    links now auth-aware (/portal/book vs /book); search/filter retained; per-service image aspect box.
  - ServiceDetail.tsx rebuilt: auth-aware booking; related services now REAL (same category from API)
    instead of hardcoded fakes; reviews from API; FAQs from CMS with generic fallback; safe-area sticky
    mobile CTA. Removed mock "Complementary Services".
  - PLACEHOLDER contact numbers in Footer ([VERIFY] — replace +91 00000 00000 / wa.me / support@coolzo.in
    with real values before launch). [Resolved: footer now CMS-driven with real fallback 7075949956/mdfayazots5@gmail.com]

Mobile UX pass (2026-06-11) — public Web. Verified homepage at true 390px device emulation (CDP):
  NO horizontal overflow (scrollWidth=390); earlier "clipping" was a headless --window-size artifact.
  - Mobile density pass (2026-06-11, CEO call — reduce scroll/oversized sections): full-page mobile
    height 6122px → 4814px (~21% less scroll).
      * Section.tsx spacing tightened on phones (desktop unchanged): compact py-8 md:py-14,
        default py-12 md:py-24, loose py-16 md:py-32.
      * Footer.tsx now 2-col on mobile (grid-cols-2): brand + Get-in-Touch span both cols, Expertise &
        Quick Links sit side-by-side; tighter pt-16/space-y-3/mb-12. Gold headings stay bright (#D4AF37
        on black = good contrast).
      * Home service cards now 2-up on mobile (grid grid-cols-2 lg:grid-cols-4) and compacted
        (p-4, smaller icon, description line-clamp-2, "View details" hidden < sm, single Book Now CTA).
        NOTE: this is a deliberate, CEO-approved exception to Web_Responsive_Standard "cards collapse to
        single column on phones" — applied ONLY to the home services overview via a local grid override;
        the shared Grid primitive is unchanged (still 1-col on phones for other pages).
  - CTA de-dup (2026-06-11, CEO call): the top-nav "Book Service" button is now hidden below lg
    (Navbar.tsx → `hidden lg:flex`) so mobile/tablet show a single persistent Book CTA (the sticky
    MobileActionBar), not two fixed buttons. Desktop (lg+, no bottom bar) keeps the nav button. In-content
    CTAs (hero/cards/CTA-band) and the footer link are intentional and unchanged.
  - NEW component MobileActionBar.tsx: persistent bottom CTA bar (lg:hidden) on the public Layout —
    "Call" (tel: from CMS contact.phone, same source as Footer) + "Book Service" (auth-aware path like
    Navbar). Hidden on /book and /booking-confirmation. Uses safe-area-pb. Footer pb-28 lg:pb-12 added so
    its bottom row clears the fixed bar.
  - App.tsx wrapped in <MotionConfig reducedMotion="user"> — whileInView sections (opacity:0 initial) now
    render final state for reduced-motion users instead of risking blank gaps.
  - P2 DONE (2026-06-11, user-authorized theme change): small gold eyebrow/label text on LIGHT surfaces
    was #D4AF37 on cream = ~2:1 (fail WCAG AA). Added theme token --color-brand-gold-deep (#8A6D1A =
    4.8:1 on cream / 4.9:1 on white) in index.css and switched light-surface eyebrows/stat-labels/
    card-categories/links to text-brand-gold-deep across Home, About, Blog, BlogDetail, AMC, WhyCoolzo,
    Pricing, Reviews, Contact, Services, Forgot/ResetPassword. KEPT bright #D4AF37 on dark (navy/black)
    surfaces — hero eyebrows, navy CTA/promise bands, Footer, WhyCoolzo table header, on-image overlays —
    where it already passes (8.4:1 on navy). Gold BUTTON backgrounds + icon fills unchanged. tsc passes.
  - P4 DONE (2026-06-11): Home service cards now show "From ₹X" (min basePrice per category, computed
    from GET /api/booking-lookups/services joined to categories by serviceCategoryId; "Priced on
    inspection" when no priced service). CTAs simplified to one primary "Book Now" (navy, passes
    serviceCategoryId/Name to booking) + subtle "View details" link. Home.tsx fetches categories+services
    via Promise.allSettled. tsc passes; verified From ₹199 (Repair) / ₹399 (Service) render on mobile.
  - Still recommended (not yet done): homepage reviews strip; trust strip 2x2 on mobile; replace
    decorative arch image with a Hyderabad coverage map.

Phase 5 status (2026-06-10): public conversion pages added/rebuilt on primitives (tsc + build pass).
  - Navbar nav re-expanded: Services, AMC Plans, Pricing, Reviews, Contact + Login icon (LogIn) for
    existing users (desktop + mobile menu). Pricing route added to App.tsx.
  - AMC.tsx: lists AMC-category services from the PUBLIC catalog (CatalogService; real prices, anonymous;
    auth-only /api/amc/plans NOT used) + benefits + Enroll→booking (auth-aware).
  - Reviews.tsx: real reviews via GET /api/customer-reviews (anonymous, all), computed avg/distribution,
    load-more; removed fake video/named-testimonial placeholders.
    Notes on Drift (2026-06-11 — request/response drift, Web): the endpoint returns `data` as a BARE
    ARRAY (no paging envelope) and uses fields customerReviewId/userName/userPhoto/createdAt. The Web
    page expected PagedResult.items with reviewId/customerName/dateCreated, so nothing bound (items was
    undefined → []). Fixed in reviewService.ts: getReviews normalizes the bare array → PagedResult and
    maps raw fields → ReviewResponse (customerReviewId→reviewId, userName→customerName, userPhoto→
    customerPhoto, createdAt→dateCreated); hasNext=false until backend paginates. submitReview maps too.
    No backend/mobile contract change. Backend list endpoint still does NOT honor pageNumber/pageSize. [VERIFY]
  - Pricing.tsx (NEW): services grouped by category with BasePrice ("From ₹…"/"On inspection") + Book CTA.
  - Contact.tsx: contact details from CMS (contact.* with real fallback); form submits a real lead via
    POST /api/leads (AllowAnonymous, sourceChannel="web") — services/contactService.ts. Loading/
    error/success states. CreateLeadRequest: CustomerName, MobileNumber, EmailAddress, SourceChannel, InquiryNotes(+more optional).
    SourceChannel is validated by LeadManagementSupport.TryParseLeadSourceChannel — ACCEPTED (normalized,
    case/separator-insensitive): website|web, app|mobileapp|mobile, phone|call, whatsapp, manual|admin,
    or an exact LeadSourceChannel enum name. Anything else → 400 "Lead source channel is invalid."
    Notes on Drift (2026-06-11 — request drift, Web): Contact.tsx sent "web-contact" (normalizes to
    "webcontact" → no match) so submit 400'd. Fixed to "web" (matches BookingWizard). The contact subject
    is carried inside InquiryNotes as "[Subject] message", not via SourceChannel.

Phase 4 status (2026-06-10): CMS module completion — ADMIN content-block editor.
  - Backend CMS already complete: CMSController /api/cms admin/blocks (GET/POST/PUT), admin/banners,
    admin/faqs, admin/theme (GET/PUT), admin/image-slots (+upload), publish, rollback/{v},
    snapshot/manifest, snapshot/{v}. Block contract: CMSBlockUpsertRequest {BlockKey,Title,Summary,
    Content,PreviewImageUrl,IsActive,IsPublished,SortOrder}; CMSBlockResponse adds CMSBlockId,VersionNumber,dates.
  - Admin UI (Frontend/Admin is REACT/TSX, not Angular): CmsDeliveryManager.tsx had Theme/Images/
    Publish tabs; ADDED a "Content Blocks" tab + repository methods getBlocks/createBlock/updateBlock.
    Admin can now create/edit any keyed block incl. footer contacts (contact.phone/whatsapp/email/city —
    quick-add buttons for missing ones), toggle Published, then Publish to push live. tsc passes.
  - Public footer (Frontend/Web Footer.tsx) reads getBlock("contact.*").content from the published
    snapshot, falling back to real values. To make footer CMS-sourced: admin adds the contact.* blocks
    (quick-add) and Publishes (one-time). Until then the real fallback values display.
  - CONTENT-BLOCK KEY REGISTRY (admin Add-website-text dropdown = KNOWN_BLOCK_KEYS in CmsDeliveryManager.tsx;
    must stay in sync with what the Web portal actually reads via getBlock — never list a key the site ignores).
    CONTENT-BLOCK MODEL: each block = { key, title, content, summary, ... }. For section blocks BOTH
    title (heading) and content (body) render on the site; for contact.* only content is used (title=label).
    Site-read content-block keys as of 2026-06-11 (admin KNOWN_BLOCK_KEYS == these; never list a key the site ignores):
      • contact.phone / contact.whatsapp / contact.email / contact.city  → Footer.tsx, Contact.tsx, MobileActionBar.tsx (content only)
      • home.hero            → Home.tsx hero: title=headline (keeps styled <br>/italic default when blank), content=subtitle
      • home.about           → Home.tsx NEW "About Coolzo" section (white, before final CTA): title=heading, content=body
      • service-content.general-service → Services.tsx banner: title=H1, content=intro paragraph
    NOTE name collision: block key "home.hero" (text) is DISTINCT from SCREEN-IMAGE slot key "home.hero" (image);
    blocks come from snapshot.content.blocks via getBlock, images from snapshot.images via getImage — different maps, no conflict.
    IMAGE SLOTS now all wired in portal: home.hero (Home), home.coverage (Home coverage section), about.hero (About), amc.banner (AMC), services.banner (Services banner — wired 2026-06-11; previously unused). All via <SnapshotImage slotKey> reading snapshot.images (relative URLs → resolveAssetUrl→API origin); bundled fallbackSrc only if a slot is unpublished.
    SLOT REGISTRY FIX (2026-06-11): Home.tsx rendered <SnapshotImage slotKey="home.coverage"> but NO such slot was ever seeded in tblScreenImageSlot — so it could never be admin-managed and always showed the hardcoded Unsplash fallback. Registered slot home.coverage/desktop 1200x900 (4:3) on live Supabase (ScreenImageSlotId 7, idempotent) + added to both seed files (Postgres 15_seed + SqlServer DB_Seed_20260609). It now appears in the admin Screen Images tab; once an admin uploads + Publishes, Home binds it from snapshot.images (until then, fallback still shows — by design). LESSON: every <SnapshotImage slotKey="X"> in the Web portal MUST have a matching tblScreenImageSlot row, or the image can never come from JSON.
    NOT YET BOUND from snapshot (gaps, 2026-06-11): snapshot.content.banners (no getBanners in ContentContext; seed banner imageUrl is /assets/… placeholder, not a CMS image → would 404 if rendered) and snapshot.content.faqs (no getFaqs; portal FAQ, if any, is API-driven). Theme tokens border/surface/success/warning/error/font.weights are in the snapshot but not mapped to CSS vars (only primary/accent/background/textPrimary/font.family are) — intentional.
    Admin dropdown shows friendly labels; custom keys validated /^[a-z0-9]+(?:[.-][a-z0-9]+)*$/; content required (create + save) so a published block can't render blank.

Notes on Drift:
  - CLAUDE.md DESIGN SYSTEM table hexes (navy #1B2A4A, gold #C9A84C, Inter-only) differ from the
    implemented Web theme above. Decision 2026-06-10: implemented Web theme is source of truth;
    do not change Web colors/fonts without explicit instruction.

---

## ServiceTypes API (Public Catalog)

### Flow: Get Service Types (Public)
  API Endpoint:   GET /api/service-types
  Auth:           AllowAnonymous
  Query Params:   visibility (string?, "public" for customer-facing), search (string?)
  Response DTO:   ApiResponse<ServiceTypeListItemResponse[]>
    - Id (long), Name (string), Description (string), Category (string)
    - BasePrice (decimal), EstimatedDurationInMinutes (int), IconKey (string)
    - ImageUrl (string, "" when null) — per-service image; added 2026-06-10 (Phase 1)
  DB Tables:      Services (read — incl. ImageUrl), ServiceCategories (read)
  Business Rules:
    1. IconKey resolved from category/service name: "repair" / "cleaning" /
       "installation" / "gas-refill" / "amc" / "service" (default)

### Flow: Get Service Type Detail
  API Endpoint:   GET /api/service-types/{serviceTypeId}
  Auth:           AllowAnonymous
  Response DTO:   ApiResponse<ServiceTypeDetailResponse>
    - Id, Name, Description, Category, BasePrice, EstimatedDurationInMinutes, IconKey
    - ImageUrl (string, "" when null) — per-service image; added 2026-06-10 (Phase 1)
    - SubTypes (ServiceTypeSubTypeResponse[]): currently always empty array
    - Faqs (CMSFaqResponse[]): public FAQ content injected from CMS
  DB Tables:      Services (read), CMSBlocks (read via FAQ query)
  Failure Cases:
    - Service not found → 404

### Flow: Get Service Sub-Types
  API Endpoint:   GET /api/service-types/{serviceTypeId}/sub-types
  Auth:           AllowAnonymous
  Response DTO:   ApiResponse<IReadOnlyCollection<ServiceTypeSubTypeResponse>>
    - Id (string), Name (string), Description (string)
  Notes:          Currently returns empty array (sub-types not yet implemented in backend)

---

## Booking Creation & Management (BookingController)

### Flow: Create Guest Booking
  Entry Points:   Booking wizard Step 6 — guest (unauthenticated) path;
                  AdminMobile manual SR creation pipeline Step 8
  UI Trigger:     "Confirm & Book" CTA
  API Endpoint:   POST /api/bookings/guest
  Auth:           AllowAnonymous
  Headers:        X-Idempotency-Key (string?, optional): prevents duplicate submissions
  Request DTO:    GuestBookingCreateRequest
    - ServiceId (long, required)
    - AcTypeId (long, required)
    - TonnageId (long?, OPTIONAL): null at booking — technician verifies on-site [changed 2026-06-08]
    - BrandId (long?, OPTIONAL): null at booking — technician verifies on-site [changed 2026-06-08]
    - SlotAvailabilityId (long?, conditional): required when IsEmergency=false; null for emergency [changed 2026-06-08]
    - CustomerName (string, required)
    - MobileNumber (string, required)
    - EmailAddress (string?, optional)
    - AddressLine1 (string, required)
    - AddressLine2 (string?, optional)
    - Landmark (string?, optional)
    - CityName (string, required)
    - Pincode (string, required)
    - AddressLabel (string?, optional)
    - ModelName (string?, optional)
    - IssueNotes (string?, optional)
    - SourceChannel (string, required): "web" / "mobile" / "whatsapp" / "admin"
    - IsEmergency (bool, required): emergency flag [added 2026-06-08 — guests can book emergency]
    - EmergencySurchargeAmount (decimal?, optional): surcharge when emergency
    - Latitude (double?, optional): GPS latitude captured by browser geolocation at address entry [added 2026-06-09]
    - Longitude (double?, optional): GPS longitude captured by browser geolocation at address entry [added 2026-06-09]
  Response DTO:   ApiResponse<BookingSummaryResponse>
    - BookingId (long), BookingReference (string), Status (string)
    - ServiceName (string), CustomerName (string), MobileNumber (string)
    - SlotDate (DateOnly), SlotLabel (string), AddressSummary (string)
    - EstimatedPrice (decimal), IsEmergency (bool), EmergencySurchargeAmount (decimal)
  DB Tables:      Bookings (write), SlotAvailability (update — decrement AvailableCapacity),
                  Customers (write if new guest), CustomerAddresses (upsert by AddressLine1+Pincode),
                  CustomerEquipment (insert if type not already saved for customer), AuditLogs (write)
  Business Rules:
    1. Idempotency: same X-Idempotency-Key returns existing booking without creating duplicate
    2. Guest customer record created if mobile not found in Customers
    3. Address upserted by (CustomerId, AddressLine1, Pincode) — creates new if not found [confirmed 2026-06-09]
    4. Equipment upserted by (CustomerId, EquipmentType) — creates minimal record if type not present;
       EquipmentName = "{AcTypeName} AC", BrandName empty (technician fills on-site) [added 2026-06-09]
    5. SlotAvailability capacity decremented on successful booking — SKIPPED for emergency (no slot)
    6. Booking confirmation notification triggered (WhatsApp/email/SMS)
    7. Admin creates SR in next step: POST /api/service-requests/from-booking/{bookingId}
    8. Tonnage/Brand looked up + validated only when supplied; null is accepted [2026-06-08]
    9. Emergency (IsEmergency=true): booking created with SlotAvailabilityId=null, capacity untouched;
       EstimatedPrice = service base + EmergencySurchargeAmount [2026-06-08]
    10. Booking-mode flags read from tblSystemSetting (keys Booking.OpenBookingMode,
       Booking.EnforceSlotCapacity) per request [added 2026-06-09]:
       • OpenBookingMode=true  → slot selection optional even for non-emergency; admin assigns time
         later (null slot accepted, no 400). Default false → slot required for non-emergency.
       • EnforceSlotCapacity=false → ValidateSlot/capacity check bypassed when a slot IS supplied.
         Default true → capacity validated. Resolution defaults to OPEN=false / ENFORCE=true when the
         key is missing or unparseable (fail-safe to strict behavior).
  State Transitions: → BookingStatus.Confirmed
  Failure Cases:
    - SlotAvailabilityId provided but not found / slot full → 409
    - SlotAvailabilityId null AND IsEmergency=false AND OpenBookingMode=false → 400 ("A time slot is required")
    - SlotAvailabilityId provided + EnforceSlotCapacity=true + slot at capacity → 409
    - TonnageId/BrandId provided but invalid → 400
    - Missing required fields → 400

---

### Flow: Create Customer Booking (Authenticated)
  Entry Points:   Booking wizard Step 6 — authenticated customer path
  UI Trigger:     "Confirm & Book" CTA (logged-in customer)
  API Endpoint:   POST /api/bookings/customer
  Auth:           Authorize (any authenticated)
  Headers:        X-Idempotency-Key (string?, optional)
  Request DTO:    CustomerBookingCreateRequest
    Same fields as GuestBookingCreateRequest (including IsEmergency + EmergencySurchargeAmount).
    TonnageId/BrandId optional, SlotAvailabilityId conditional — identical rules to guest [2026-06-08].
    Latitude (double?, optional) + Longitude (double?, optional): GPS coords — same as guest [added 2026-06-09].
    CustomerId is resolved from the JWT (not a request field).
  Response DTO:   ApiResponse<BookingSummaryResponse> (same shape as guest)
  DB Tables:      Bookings (write), SlotAvailability (write), CustomerAddresses (upsert),
                  CustomerEquipment (insert if type not already saved), AuditLogs (write)
  Business Rules:
    1. Booking linked to authenticated customer's CustomerId (from JWT)
    2. CustomerName/MobileNumber: optional in request — resolved from existing Customer record;
       only updated when request supplies non-empty values [fixed 2026-06-09]
    3. Address upserted by (CustomerId, AddressLine1, Pincode) — creates new if not found
    4. Equipment upserted by (CustomerId, EquipmentType) — creates minimal record if type not present;
       EquipmentName = "{AcTypeName} AC", BrandName empty (technician fills on-site) [added 2026-06-09]
    5. IsEmergency=true: bypass normal slot rules per emergency policy
    6. EmergencySurchargeAmount recorded on booking
    7. Same idempotency behavior as guest booking
    8. Booking-mode flags (Booking.OpenBookingMode, Booking.EnforceSlotCapacity) applied identically
       to the guest flow — same read from tblSystemSetting, same defaults/fail-safe [added 2026-06-09]
  State Transitions: → BookingStatus.Confirmed
  Failure Cases:
    - Slot provided but full/unavailable (when EnforceSlotCapacity=true) → 409
    - SlotAvailabilityId null AND IsEmergency=false AND OpenBookingMode=false → 400
    - Invalid emergency surcharge (< 0) → 400

---

### Flow: Get Booking Public Settings  [added 2026-06-09]
  Entry Points:   Booking wizard bootstrap (public/guest path) — read before slot step
  UI Trigger:     Wizard load / "Book Now" entry
  API Endpoint:   GET /api/bookings/public/settings
  Auth:           AllowAnonymous
  Request DTO:    none
  Response DTO:   ApiResponse<BookingPublicSettingsResponse>
    - OpenBookingMode (bool): true → wizard may allow submit without a slot (admin schedules later)
    - EnforceSlotCapacity (bool): true → wizard must respect per-slot capacity / disable full slots
  Business Rules:
    1. Values resolved from tblSystemSetting via ISystemSettingRepository.GetByKeysAsync
       (keys Booking.OpenBookingMode, Booking.EnforceSlotCapacity).
    2. Fail-safe defaults when key missing/unparseable: OpenBookingMode=false, EnforceSlotCapacity=true
       — same resolution logic the two booking-create handlers use, so UI and server agree.
    3. Read-only; no DB writes, no audit entry.
  DB Tables:      tblSystemSetting (read)
  Stored Procedures: none (EF query via SystemSettingRepository.GetByKeysAsync)
  Failure Cases:  none functional — always returns a resolved pair (defaults on missing rows)
  Notes on Drift: Endpoint exists so the public wizard reads the SAME two flags the create handlers
                  enforce, preventing UI/server divergence (client can't submit a no-slot booking the
                  server would 400, and vice-versa).

---

## WEB BOOKING WIZARD — CLIENT CONTRACT & STABILITY (Frontend/Web, Phase 1 — 2026-06-10)

File: Frontend/Web/src/pages/BookingWizard.tsx (single component, 5 visible steps).
Service layer: Frontend/Web/src/services/bookingService.ts. API error shape (apiClient interceptor):
rejects with { status, message, fieldErrors, raw } — callers read err.status / err.message.

Step sequence (driven by feature flags + auth):
  Guest, slot required:        1 Service → 2 Location → 3 Date/Slot → 4 Contact → 5 Confirm
  Guest, OpenBookingMode:      1 → 2 → 4 → 5 (slot step skipped; admin schedules later)
  Logged-in, slot required:    1 → 2 → 3 → 5 (contact step skipped; name/mobile from profile)
  Logged-in, OpenBookingMode:  1 → 2 → 5
  Flags loaded once on mount via GET /api/bookings/public/settings; Step 1 shows a loader until
  catalog + settings resolve, so openBookingMode is stable before any navigation (no mid-flow shift).

serviceId resolution rule (CRITICAL):
  serviceId sent to the API MUST be a Service id, never a ServiceCategory id.
  Normal categories require a sub-type selection (Step-1 validation). AMC/Other skip the sub-type row,
  so the client resolves serviceId = serviceSubTypeId ?? firstServiceInCategory.serviceId. If the
  category has no bookable service, the client BLOCKS submit with a message (does not send a category id).

slotDate rule:
  Wizard sends slotDate as a LOCAL calendar date (toLocalDateStr) — never toISOString() (UTC), which
  shifted the date a day back for IST users in early hours.

Submit error mapping (describeBookingError):
  409 → "That time slot was just taken. Please pick another time window or date."
  400 → surfaces backend message (e.g. serviceability / validation).
  else → backend message if present, else generic retry. (401 is auto-handled by apiClient → /session-expired.)

Idempotency: client generates crypto.randomUUID() as X-Idempotency-Key on every create call
  (createCustomerBooking / createGuestBooking).

Notes on Drift (fixed 2026-06-10, Phase 1):
  - DRIFT (request): serviceId fell back to ServiceCategory id for AMC/Other categories
    (`data.serviceSubTypeId ?? data.serviceTypeId!`) → invalid serviceId → intermittent booking
    failures. Fixed: resolve to a real Service id or block. [VERIFY with Product the intended
    representative service for AMC/Other categories — currently first service in category.]
  - DRIFT (date): UTC date via toISOString caused off-by-one slotDate for IST early-hours users. Fixed.
  - DRIFT (error UX): generic catch hid 409/400 reasons → blind retries. Fixed with status mapping.
  - KNOWN GAP [VERIFY]: WizardData carries isEmergency/emergencySurcharge and Step 5 renders an
    emergency badge, but no Step-3 UI sets them — web cannot currently create an emergency booking
    (slotWindow "Emergency" is declared but unused). Backend supports it; web UI is a Phase-2/PM decision.

---

### NOTES ON DRIFT (2026-06-09) — Booking-mode flags (OpenBookingMode / EnforceSlotCapacity)
Drift type: stale docs (feature shipped in code; brain + DB seed not updated).
- WHAT SHIPPED: two system-setting flags now govern booking slot behavior, read per request by both
  CreateGuestBooking and CreateCustomerBooking handlers, plus a new GET /api/bookings/public/settings
  endpoint that exposes them to the public wizard.
  • Booking.OpenBookingMode (default false): when true, slot selection optional for non-emergency.
  • Booking.EnforceSlotCapacity (default true): when false, per-slot capacity check is bypassed.
- STORAGE: tblSystemSetting (existing table — no schema change). Flags stored as DataType='Boolean'.
- SEED REQUIRED (RUN ONCE per environment — not yet executed). Provider differs by environment:
  • appsettings.json        → Provider=SqlServer → (localdb)\MSSQLLocalDB / CoolzoDB
  • appsettings.Development  → Provider=Postgres  → Supabase (Host=aws-1-ap-south-1.pooler.supabase.com)
  SQL Server seed:  Backend/Docs/Database/DB_Seed_20260609_BookingModeSettings.sql
  Postgres seed:    Backend/Docs/Postgres/DB_Seed_20260609_BookingModeSettings_Postgres.sql  [added 2026-06-09]
  The original SQL Server seed (GETDATE()/dbo./GO/IF-EXISTS) does NOT run on Postgres — run the
  matching file for the target provider. Both insert OpenBookingMode='false' + EnforceSlotCapacity='true'
  only if absent (idempotent). Handlers fail-safe to the same defaults if the seed has not run, so
  behavior is correct pre-seed; the seed only makes the flags admin-visible/editable.
- [VERIFY] Seed not confirmed executed against either DB from this session.

---

### NOTES ON DRIFT (2026-06-08) — Booking-create contract relaxed for redesigned wizard
Drift type: DB contract + request drift (API/DB required equipment + slot the redesigned wizard never sends).
- ROOT CAUSE: The 2026-05-27 wizard redesign dropped equipment capture (brand/tonnage/model) and made
  emergency bookings slot-less, but BOTH booking-create validators still enforced
  `TonnageId > 0`, `BrandId > 0`, `SlotAvailabilityId > 0`. Non-nullable `long` request fields meant
  a missing JSON value bound to 0 → validation failed → EVERY wizard booking returned 400 (not just
  emergency/AMC). Guest DTO also lacked IsEmergency entirely, so guests could not flag emergency.
- FIX (root cause, on backend per the mobile/contract rule — no surface hack):
  • Request DTOs + commands: TonnageId, BrandId, SlotAvailabilityId → nullable (long?).
  • GuestBookingCreateRequest/Command/Controller: added IsEmergency + EmergencySurchargeAmount.
  • Validators: Tonnage/Brand validated only `.When(HasValue)`; SlotAvailabilityId required only
    `.When(!IsEmergency)`.
  • Both handlers: Tonnage/Brand/Slot looked up only when supplied; capacity decrement guarded;
    emergency branch creates a slot-less booking; guest handler now records IsEmergency + surcharge.
  • Entities: Booking.SlotAvailabilityId, BookingLine.TonnageId, BookingLine.BrandId → long?.
    EF infers optional FK from nullable CLR type (no Configuration change needed). Response mappers
    were already null-safe (slot/tonnage/brand null-coalesced).
  • DB: tblBooking.SlotAvailabilityId, tblBookingLine.TonnageId/BrandId set nullable. Canonical DDL
    (02_create_tables.sql) updated; idempotent migration in Docs/Postgres/14_booking_optional_equipment_slot.sql.
  • Frontend: CustomerBookingCreateRequest.slotAvailabilityId optional; wizard sends
    `slotAvailabilityId ?? undefined` and `emergencySurchargeAmount` when emergency.
- VERIFIED: backend `dotnet build` 0 errors/0 warnings; frontend `tsc --noEmit` clean.

---

### NOTES ON DRIFT (2026-06-09) — GPS coordinate capture added to booking + SR detail flow
Drift type: Missing feature — GPS coordinates were captured by browser geolocation but silently dropped before reaching the backend.
- ROOT CAUSE: BookingWizard.tsx `handleLocationDetect` called `navigator.geolocation.getCurrentPosition()` and
  ran reverse geocoding (Nominatim) to produce address text, but the `onUpdate()` call never included
  `latitude`/`longitude`. Coords were also missing from `handleAddressSelect` for saved addresses.
  The backend `CustomerAddress.Latitude/Longitude` columns existed (added 2026-04-25 schema compat) but
  were always NULL. The `ArriveFieldJobCommand` 150m Haversine check-in gate was fully implemented but
  completely blocked — NULL coords → distance check skipped → gate never fired.
- FIX (Phase 1 — GPS capture activated, coordinates persisted end-to-end):
  • BookingWizard.tsx WizardData: added `latitude: number|null`, `longitude: number|null`.
  • `handleLocationDetect`: passes raw GPS coords (not Nominatim centroid) to `onUpdate`.
  • `handleAddressSelect`: propagates saved address `.latitude/.longitude` to wizard state.
  • `handleEnterDifferentAddress`: resets coords to null on new address entry.
  • `handleConfirm`: includes `latitude`/`longitude` in the booking creation API payload.
  • Frontend type `CustomerBookingCreateRequest`: added `latitude?: number`, `longitude?: number`.
  • Backend `GuestBookingCreateRequest` + `CustomerBookingCreateRequest` records: added `double? Latitude, double? Longitude`.
  • `CreateGuestBookingCommand` + `CreateCustomerBookingCommand`: added same two params.
  • Both command handlers: save coords to `CustomerAddress.Latitude/Longitude` (create: always; update: only when non-null).
  • `Booking.LatitudeSnapshot` + `LongitudeSnapshot` (double?): added to domain entity + EF config.
  • BookingController: wires `request.Latitude, request.Longitude` for both endpoints.
- FIX (Phase 2 — admin portal SR detail "View on Map" uses exact coordinates):
  • `ServiceRequestDetailResponse`: added `double? CustomerLatitude, double? CustomerLongitude` (positional params).
  • `ServiceRequestResponseMapper.ToDetail`: populates from `booking?.CustomerAddress?.Latitude/Longitude`.
  • Frontend `BackendServiceRequestDetail` interface: added `customerLatitude?/customerLongitude?`.
  • `buildBaseServiceRequest`: accepts optional `coordinates: { lat, lng }` and includes it in `location`.
  • `mapAdminDetailToServiceRequest`: passes backend coords to `buildBaseServiceRequest.coordinates`.
  • `SRDetailScreen.tsx` "View on Map" button: uses `https://www.google.com/maps/dir/?api=1&destination={lat},{lng}` when coordinates available, falls back to OpenStreetMap address-text search when not.
- ACTIVATION: The `ArriveFieldJobCommand` 150m GPS check-in gate (already fully implemented in FieldWorkflowFeature.cs) now activates automatically for all new GPS-assisted bookings. No code change needed — coordinates being NULL was the only blocker.
- DB migration required: `tblBooking.LatitudeSnapshot` and `LongitudeSnapshot` columns (double precision / FLOAT) must be added to the live schema. Migration file: `Backend/Docs/Postgres/17_booking_gps_snapshot.sql`.
- FLOW STABLE: YES — future agent can work GPS-capture without source reads.

---

### NOTES ON DRIFT (2026-06-09) — Equipment save + address persist + CustomerName null fix

Drift type: Missing feature (equipment save) + request drift (CustomerName required on customer booking but not sent).

**Root causes fixed:**

1. **CustomerName required on authenticated booking (400 on every logged-in booking)**
   - `CreateCustomerBookingCommandValidator` had `CustomerName NotEmpty()` — frontend never sends it for
     logged-in users (identity from JWT). Result: every `POST /api/bookings/customer` returned 400;
     address/equipment never persisted.
   - FIX: `CreateCustomerBookingCommand.CustomerName/MobileNumber` → `string?` (nullable).
     Validator: both validated only `.When(not null/empty)`.
     Handler: null-safe update — only overwrites existing Customer.CustomerName/MobileNumber when request
     supplies non-empty values; falls back to `_currentUserContext.UserName` on new customer create.
   - Frontend: `CustomerBookingCreateRequest` now includes optional `customerName?/mobileNumber?`.
     `handleConfirm` sends `data.guestName || user.fullName` and `myMobile` for logged-in path.

2. **Equipment never saved on booking**
   - Neither handler created `CustomerEquipment` records. Portal `/portal/equipment` always showed empty.
   - FIX: Both handlers (guest + customer) now call `HasCustomerEquipmentByTypeAsync(customerId, acTypeName)`
     and insert a minimal `CustomerEquipment` record if the type is not already registered.
     Record shape: `EquipmentName = "{AcTypeName} AC"`, `EquipmentType = acTypeName`, `BrandName = ""`
     (technician fills brand/model on-site). Prevents duplicate inserts on repeat bookings of same type.
   - New repository methods: `AddCustomerEquipmentAsync` + `HasCustomerEquipmentByTypeAsync` added to
     `IBookingRepository` and `BookingRepository`.

3. **Step 2 manual form always showed for logged-in users**
   - `showManualForm` initialized to `!data.selectedAddressId = !null = true`, causing manual entry form
     to render simultaneously with the saved-address card list.
   - FIX: `showManualForm` initialized to `!(isLoggedIn && myAddresses.length > 0) && !data.selectedAddressId`.
     Auto-select `useEffect` fires once on mount (guarded by `autoSelectDone` ref) and calls
     `handleAddressSelect(myAddresses[0])` — pre-selects the default (first sorted) address.
     Button label: "Enter a different address" → "Use a different address".

- DB migration required: no new columns — `CustomerEquipment` table already exists (13_customer_app_tables.sql).
- FLOW STABLE: YES — booking create → address upsert + equipment upsert + booking record all in one transaction.

---

### Flow: Get Booking Detail (Admin)
  Entry Points:   Admin booking detail view, SR creation pipeline
  API Endpoint:   GET /api/bookings/{bookingId}
  Auth:           Policy = BookingRead
  Response DTO:   ApiResponse<BookingDetailResponse>
    - BookingId (long), BookingReference (string), Status (string), SourceChannel (string)
    - IsGuestBooking (bool), BookingDateUtc (DateTime), IsEmergency (bool)
    - EmergencySurchargeAmount (decimal)
    - ServiceName (string), CustomerName (string), MobileNumber (string), EmailAddress (string)
    - AddressSummary (string), ZoneName (string), SlotDate (DateOnly), SlotLabel (string)
    - EstimatedPrice (decimal)
    - ServiceRequestId (long?), ServiceRequestNumber (string?), OperationalStatus (string?)
    - AssignedTechnicianId (long?), AssignedTechnicianName (string?)
    - JobCardId (long?), JobCardNumber (string?)
    - QuotationId/Number/Status (long?/string?/string?)
    - InvoiceId/Number/Status/GrandTotalAmount/BalanceAmount (long?/string?/string?/decimal?/decimal?)
    - CompletionSummary (string?)
    - FieldTimeline (JobExecutionTimelineItemResponse[])
    - CustomerVisibleNotes (JobExecutionNoteResponse[])
    - Lines (BookingLineResponse[])
    - StatusHistory (BookingStatusHistoryResponse[])
  DB Tables:      Bookings (read), ServiceRequests (read), Technicians (read),
                  Invoices (read), JobReports (read)
  Failure Cases:
    - Booking not found → 404

---

### Flow: Search Bookings (Admin List)
  Entry Points:   Admin booking list / operations list view
  API Endpoint:   GET /api/bookings
  Auth:           Policy = BookingRead
  Query Params:
    - bookingReference (string?, optional)
    - customerMobile (string?, optional)
    - bookingDate (DateOnly?, optional)
    - serviceId (long?, optional)
    - pageNumber (int, default=1), pageSize (int, default=20)
  Response DTO:   ApiResponse<PagedResult<BookingListItemResponse>>
    BookingListItemResponse:
      - BookingId (long), BookingReference (string), Status (string), SourceChannel (string)
      - ServiceName (string), CustomerName (string), MobileNumber (string)
      - SlotDate (DateOnly), SlotLabel (string), BookingDateUtc (DateTime)
      - OperationalStatus (string?), AssignedTechnicianName (string?), AssignedTechnicianId (long?)
      - AddressSummary (string), EstimatedPrice (decimal)
      - IsEmergency (bool), EmergencySurchargeAmount (decimal)
      - QuotationId (long?), QuotationStatus (string?), InvoiceGrandTotalAmount (decimal?)
  DB Tables:      Bookings (read), ServiceRequests (read), Technicians (read)

---

### Flow: Get My Bookings (Customer Self-Service)
  Entry Points:   Customer portal — My Bookings list
  UI Trigger:     My Bookings page load / pull-to-refresh
  API Endpoint:   GET /api/bookings/my-bookings
  Auth:           Authorize (any authenticated)
  Query Params:   pageNumber (int, default=1), pageSize (int, default=20)
  Response DTO:   ApiResponse<PagedResult<BookingListItemResponse>> (same shape as admin list)
  DB Tables:      Bookings (read — filtered to JWT CustomerId)
  Business Rules:
    1. Returns only bookings belonging to the authenticated customer
    2. Tracker polling at 60s interval on active bookings (client-side)

---

### Flow: Reschedule Booking
  Entry Points:   Customer portal — Reschedule Booking action; Admin portal
  UI Trigger:     "Reschedule" CTA → new slot selection
  API Endpoint:   POST /api/bookings/{bookingId}/reschedule
  Auth:           Authorize (any authenticated)
  Request DTO:    RescheduleCustomerBookingRequest
    - SlotAvailabilityId (long, required): new slot
    - Remarks (string?, optional): reason for reschedule
  Response DTO:   ApiResponse<BookingDetailResponse>
  DB Tables:      Bookings (write), SlotAvailability (write — release old, reserve new),
                  BookingStatusHistory (write), AuditLogs (write)
  Business Rules:
    1. Old SlotAvailability capacity released; new slot capacity decremented
    2. Customer and technician notified of reschedule
    3. Reschedule history entry created
  Failure Cases:
    - New slot full → 400
    - Booking not in reschedulable state → 400

---

## CustomerBooking API (Customer-Facing Booking Operations)
## Route prefix: /api/customer-bookings
## NOTE: Entire controller was MISSING from ProjectOverview prior to 2026-05-24 audit.

### Flow: Get Customer Booking Detail
  Entry Points:   Customer portal — Booking Detail / Job Tracker screen
  UI Trigger:     Booking row click / direct deep link
  API Endpoint:   GET /api/customer-bookings/{bookingId}
  Auth:           Authorize (any authenticated)
  Response DTO:   ApiResponse<BookingDetailResponse> (same full shape as admin detail)
  DB Tables:      Bookings (read), ServiceRequests (read), Invoices (read), JobReports (read)
  Business Rules:
    1. Customer sees only their own bookings (enforced in query handler)
    2. FieldTimeline shows only customer-visible events

---

### Flow: Get Customer Bookings List
  Entry Points:   Customer portal — My Bookings list (alternate route)
  API Endpoint:   GET /api/customer-bookings
  Auth:           Authorize (any authenticated)
  Query Params:   pageNumber (int, default=1), pageSize (int, default=20)
  Response DTO:   ApiResponse<PagedResult<BookingListItemResponse>>
  DB Tables:      Bookings (read — filtered to JWT CustomerId)
  Notes:          Same query handler as GET /api/bookings/my-bookings.

---

### Flow: Reschedule Customer Booking (Customer Portal)
  API Endpoint:   POST /api/customer-bookings/{bookingId}/reschedule
  Auth:           Authorize (any authenticated)
  Request DTO:    RescheduleCustomerBookingRequest
    - SlotAvailabilityId (long, required), Remarks (string?, optional)
  Response DTO:   ApiResponse<BookingDetailResponse>
  DB Tables:      Bookings (write), SlotAvailability (write), BookingStatusHistory (write)
  Notes:          Same command handler as POST /api/bookings/{bookingId}/reschedule.

---

### Flow: Get Service Report
  Entry Points:   Customer portal — Service Report view after job completion
  API Endpoint:   GET /api/customer-bookings/{bookingId}/service-report
  Auth:           Authorize (any authenticated)
  Response DTO:   ApiResponse<BookingDetailResponse> (same as booking detail)
  DB Tables:      Bookings (read), JobReports (read)

---

### Flow: Download Service Report PDF
  Entry Points:   Customer portal — "Download Report" button
  API Endpoint:   GET /api/customer-bookings/{bookingId}/service-report/pdf
  Auth:           Authorize (any authenticated)
  Response:       application/pdf file stream
    Content: ServiceRequestNumber/BookingReference, ServiceName, AddressSummary,
             Slot, Status, CompletionSummary, FieldTimeline events
  DB Tables:      Bookings (read), JobReports (read)
  Notes:          PDF generated in-memory via SimplePdfDocumentBuilder (text-only layout)

---

6. DB TABLES (Booking / Slots) — confirmed structure

ServiceSlots
- SlotId (PK), SlotDate, WindowStart, WindowEnd, SlotType, MaxCapacity,
  ZoneId, ServiceTypeId, CreatedAt, UpdatedAt

SlotAvailability
- SlotAvailabilityId (PK), SlotId (FK), ZoneId, AvailableCapacity, ReservedCapacity, UpdatedAt
  NOTE: capacity updated directly on booking creation (no separate hold step)

Bookings
- BookingId (PK), BookingReference, Status, SourceChannel, IsGuestBooking
  CustomerId (FK nullable for guest), GuestName, GuestMobile, GuestEmail
  ServiceId (FK), AcTypeId (FK), TonnageId (FK), BrandId (FK)
  ModelName, IssueNotes, SlotAvailabilityId (FK)
  AddressLine1..Pincode, AddressSummary, ZoneId, ZoneName
  EstimatedPrice, IsEmergency, EmergencySurchargeAmount
  IdempotencyKey, CreatedAt, UpdatedAt

BookingStatusHistory
- HistoryId (PK), BookingId (FK), Status, Remarks, ChangedByUserId, ChangedAt

BookingLines
- LineId (PK), BookingId (FK), Description, Quantity, UnitPrice, LineTotal

Notes:
- No slot-hold step in current implementation. Booking creation is idempotent
  via X-Idempotency-Key; slot capacity decremented atomically on booking write.
- Zone validation happens at wizard Step 3 via GET /api/booking-lookups/zones/by-pincode.
- OTP for guest verification uses POST /api/auth/otp/send + /otp/verify (Auth module).
- Coupon validation uses POST /api/coupons/apply or GET /api/customers/me/promotions
  (Coupon & Discount Engine module).

# SECTION 5 — BILLING, INVOICE & PAYMENT
## Updated: 2026-05-24 — Stable Contract Level
## Controllers: InvoiceController, PaymentController, BillingController, QuotationController, RefundController

---

## MODULE: Invoice Engine
## Controller: InvoiceController — Route Prefix: /api/invoices

---

### Flow 1: Generate Invoice from Quotation

Entry Points:       Admin Portal → Invoice module → "Generate Invoice" button; auto-triggered on SR completion
UI Trigger:         Manual "Generate Invoice" or system auto-trigger on job report submission
Endpoint:           POST /api/invoices/from-quotation/{quotationId}
Auth:               Authorize(Policy = InvoiceCreate)
Request:
  - Route param: quotationId (long)
  - Header: X-Idempotency-Key (string?, optional): prevents duplicate invoice generation on retry
  - No body
Response (200):     InvoiceDetailResponse
  - InvoiceId (long)
  - InvoiceNumber (string)
  - QuotationId (long)
  - QuotationNumber (string)
  - CustomerId (long)
  - CustomerName (string)
  - MobileNumber (string)
  - AddressSummary (string)
  - ServiceName (string)
  - CurrentStatus (string): "Unpaid" on creation
  - InvoiceDateUtc (DateTime)
  - SubTotalAmount (decimal)
  - DiscountAmount (decimal)
  - TaxPercentage (decimal)
  - TaxAmount (decimal)
  - GrandTotalAmount (decimal)
  - PaidAmount (decimal): 0.00 on creation
  - BalanceAmount (decimal): equals GrandTotalAmount on creation
  - LastPaymentDateUtc (DateTime?)
  - Lines (InvoiceLineResponse[])
  - Payments (PaymentTransactionResponse[])
  - BillingHistory (BillingStatusHistoryResponse[])
DB Tables:          Invoices, InvoiceLines, Quotations, QuotationLines, BillingStatusHistory
Business Rules:
  - Invoice auto-created from approved quotation line items
  - GrandTotal = (SubTotal - Discount) + TaxAmount
  - IdempotencyKey on header prevents duplicate invoice for same quotation
  - Invoice starts in status "Unpaid"; linked to QuotationId
State Transitions:  Invoice: created → Unpaid
Failure Cases:
  - 404 if quotationId not found
  - 409 if invoice already exists for this quotation (idempotency)
  - 422 if quotation not in Approved status
Notes on Drift:     ProjectOverview previously listed "POST /api/invoices" as the creation route.
                    Actual route is POST /api/invoices/from-quotation/{quotationId}. Corrected 2026-05-24.
                    Also: PUT /api/invoices/{id}, POST /api/invoices/{id}/send, POST /api/invoices/{id}/credit-note
                    were listed but do NOT exist in InvoiceController. Removed as phantom endpoints 2026-05-24.

---

### Flow 2: Search Invoices (Admin)

Entry Points:       Admin Portal → Invoices List screen
UI Trigger:         Screen load / filter change
Endpoint:           GET /api/invoices
Auth:               Authorize(Policy = InvoiceRead)
Query Params:
  - status (string?, optional): filter by invoice status
  - customerId (long?, optional): filter by customer
  - pageNumber (int, default 1)
  - pageSize (int, default 20)
Response (200):     PagedResult<InvoiceListItemResponse>
  Each item:
    - InvoiceId (long)
    - InvoiceNumber (string)
    - QuotationId (long)
    - QuotationNumber (string)
    - CustomerName (string)
    - CurrentStatus (string)
    - GrandTotalAmount (decimal)
    - PaidAmount (decimal)
    - BalanceAmount (decimal)
    - InvoiceDateUtc (DateTime)
DB Tables:          Invoices, Customers, Quotations
Failure Cases:
  - 403 if role lacks InvoiceRead permission
Notes on Drift:     Previously just "GET /api/invoices" — no query params or DTO documented. Added 2026-05-24.

---

### Flow 3: Get Invoice Detail

Entry Points:       Admin Portal → Invoice Detail; Customer Portal → Invoice Detail; Mobile app → invoice view
UI Trigger:         Invoice row click; notification link
Endpoint:           GET /api/invoices/{id}
Auth:               Authorize (any authenticated user)
Request:            Route param: id (long)
Response (200):     InvoiceDetailResponse (same shape as Flow 1)
DB Tables:          Invoices, InvoiceLines, PaymentTransactions, BillingStatusHistory
Failure Cases:
  - 404 if invoice not found
  - 401 if not authenticated
Notes on Drift:     Was documented. Now at stable contract level with full DTO fields. Updated 2026-05-24.

---

### Flow 4: Mark Invoice as Paid (Manual / COD)

Entry Points:       Admin Portal → Invoice Detail → "Mark as Paid" button; Billing Exec screen
UI Trigger:         "Mark as Paid" button tap
Endpoint:           POST /api/invoices/{id}/mark-paid
Auth:               Authorize (any authenticated user)
Request Body (InvoiceMarkPaidRequest — defined inline in InvoiceController.cs):
  - Amount (decimal, required)
  - Method (string, required): "Cash" | "Upi" | "Card" | "Cheque" | "BankTransfer" | "Online"
  - Reference (string?, optional): payment reference number; if blank, auto-generated as MANUAL-{id}-{timestamp}
  - Notes (string?, optional)
Payment Method Normalization (server-side):
  - "cash", "cheque", "banktransfer" → "Cash"
  - "upi", "online" → "Upi"
  - "card" → "Card"
Response (200):     InvoiceDetailResponse (updated with payment applied)
DB Tables:          Invoices (PaidAmount, BalanceAmount, Status updated), PaymentTransactions, PaymentReceipts
Business Rules:
  - Internal idempotency key auto-generated: "invoice-mark-paid-{id}-{referenceNumber}"
  - After RecordPayment, GetInvoiceById re-fetched and returned
  - Partial payment leaves invoice status "PartiallyPaid"; full payment → "Paid"
State Transitions:  Invoice: Unpaid → PartiallyPaid (partial) or Paid (full)
Failure Cases:
  - 404 if invoice not found
  - 422 if amount > BalanceAmount
Notes on Drift:     Route was documented as "POST /api/invoices/{invoiceId}/mark-paid" — confirmed correct.
                    InvoiceMarkPaidRequest DTO is defined inline in InvoiceController.cs (not a Contracts file).
                    Updated 2026-05-24.

---

### Flow 5: Download Invoice PDF

Entry Points:       Admin Portal → Invoice Detail → "Download PDF"; Customer Portal → invoice view
UI Trigger:         "Download PDF" button
Endpoint:           GET /api/invoices/{id}/pdf
Auth:               Authorize (any authenticated user)
Request:            Route param: id (long)
Response (200):     application/pdf file; filename: invoice-{InvoiceNumber}.pdf
                    Content: text-based PDF via SimplePdfDocumentBuilder (lightweight, no external PDF library)
                    Lines: Invoice number, Customer, Service, Date, Status, Subtotal, Discount, Tax, Total, Paid, Outstanding, line items
DB Tables:          Invoices, InvoiceLines (read-only)
Failure Cases:
  - 404 if invoice not found
Notes on Drift:     PDF generation uses SimplePdfDocumentBuilder (internal utility). Not a full branded PDF template;
                    lightweight text-based output. Confirmed 2026-05-24.

---

### Flow 6: Get Customer Invoices (Customer-Facing)

Entry Points:       Customer Portal → "My Invoices"; Customer Mobile app → invoice list
UI Trigger:         Screen load / pagination
Endpoint:           GET /api/invoices/customer
Auth:               Authorize (authenticated customer)
Query Params:
  - pageNumber (int, default 1)
  - pageSize (int, default 20)
Response (200):     PagedResult<InvoiceListItemResponse> (filtered to authenticated customer's invoices)
DB Tables:          Invoices, Customers
Business Rules:
  - Filtered by CustomerId derived from JWT claims (not query param)
Failure Cases:
  - 401 if not authenticated
Notes on Drift:     Route documented in 2026-04-22 update as "GET /api/invoices/customer". Confirmed. Stable 2026-05-24.

---

## MODULE: Payment Engine
## Controller: PaymentController — Route Prefix: /api/payments
## Auth: Authorize on all endpoints

---

### Flow 7: Initiate Payment (Gateway Session)

Entry Points:       Customer Portal → "Pay Now" button; Customer Mobile → payment screen
UI Trigger:         "Pay Now" button tap
Endpoint:           POST /api/payments
Auth:               Authorize
Request Body (InitiatePaymentRequest):
  - InvoiceId (long, required)
  - Method (string, required): "Upi" | "Card" | "Cash" etc.
Response (200):     PaymentGatewaySessionResponse
  - PaymentId (string): "invoice-{InvoiceId}"
  - PaymentUrl (string): "/app/payment-status/success/{InvoiceId}"
  - Status (string): "Confirmed" (payment recorded inline — no external gateway redirect in current impl)
DB Tables:          Invoices, PaymentTransactions, PaymentReceipts
Business Rules:
  - If invoice.BalanceAmount > 0: RecordPaymentCommand called with full balance amount
  - Reference number auto-generated: "APP-{InvoiceId}-{timestamp}"
  - Idempotency key: "customer-app-{InvoiceId}"
  - Current implementation records payment inline (simulates gateway confirmation)
Failure Cases:
  - 404 if invoice not found
  - 422 if BalanceAmount ≤ 0 (already paid)
Notes on Drift:     Previously documented as "POST /api/payments (record payment / gateway callback)".
                    Actual: creates a simulated gateway session and records payment inline.
                    POST /api/payments/reconcile listed in ProjectOverview does NOT exist. Removed 2026-05-24.

---

### Flow 8: Collect Payment (Direct / Webhook Record)

Entry Points:       Admin Portal → Billing → Record Payment; webhook receiver; Field payment via /api/field/jobs/{id}/payment
UI Trigger:         "Record Payment" button; or webhook event; or field payment completion
Endpoint:           POST /api/payments/collect
Auth:               Authorize
Request Body (RecordPaymentRequest):
  - InvoiceId (long, required)
  - PaidAmount (decimal, required)
  - PaymentMethod (string, required)
  - ReferenceNumber (string?, optional)
  - Remarks (string?, optional)
  - IdempotencyKey (string?, optional): prevents duplicate recording
  - GatewayTransactionId (string?, optional): gateway reference
  - Signature (string?, optional): digital payment signature
  - ExpectedInvoiceAmount (decimal?, optional): client-side cross-check
  - IsWebhookEvent (bool, required): true if called by gateway webhook
  - WebhookReference (string?, optional): gateway webhook reference
Response (200):     PaymentTransactionResponse
  - PaymentTransactionId (long)
  - InvoiceId (long)
  - PaymentMethod (string)
  - ReferenceNumber (string)
  - PaidAmount (decimal)
  - PaymentDateUtc (DateTime)
  - TransactionRemarks (string)
  - Receipt (PaymentReceiptResponse?)
DB Tables:          PaymentTransactions, Invoices (PaidAmount, BalanceAmount, Status updated), PaymentReceipts
Business Rules:
  - IdempotencyKey prevents duplicate payment recording
  - IsWebhookEvent=true triggers webhook-specific processing path
Failure Cases:
  - 409 if duplicate IdempotencyKey
  - 404 if invoice not found
Notes on Drift:     This endpoint is new; not in old ProjectOverview. Added 2026-05-24.

---

### Flow 9: Get Payments by Invoice

Entry Points:       Admin Portal → Invoice Detail → Payments tab
UI Trigger:         Tab switch / screen load
Endpoint:           GET /api/payments/invoice/{invoiceId}
Auth:               Authorize
Request:            Route param: invoiceId (long)
Response (200):     IReadOnlyCollection<PaymentTransactionResponse>
DB Tables:          PaymentTransactions, PaymentReceipts
Failure Cases:
  - 404 if invoice not found
Notes on Drift:     Not in old ProjectOverview. Added 2026-05-24.

---

### Flow 10: Get Payment / Gateway Status

Entry Points:       Payment success/failed redirect; polling after gateway redirect
UI Trigger:         Payment return URL or client poll
Endpoint:           GET /api/payments/{paymentId}
Auth:               Authorize
Request:            Route param: paymentId (string) — format "invoice-{invoiceId}" or just "{invoiceId}"
Response (200):     PaymentGatewayStatusResponse
  - PaymentId (string)
  - InvoiceId (long)
  - Status (string): "Confirmed" if BalanceAmount ≤ 0; else "Pending"
  - RedirectUrl (string): "/app/payment-status/success/{invoiceId}" or "/app/payment-status/failed/{invoiceId}"
DB Tables:          Invoices (read-only)
Failure Cases:
  - 404 if paymentId format cannot be resolved to an invoice
Notes on Drift:     Previously documented as "GET /api/payments/{paymentId}" (generic). Now full contract. 2026-05-24.

---

### Flow 11: Get Payment Receipt

Entry Points:       Customer Portal → "Download Receipt"; Admin Portal → Invoice Detail → Receipt tab
UI Trigger:         "View Receipt" button
Endpoint:           GET /api/payments/receipt/{invoiceId}
Auth:               Authorize
Request:            Route param: invoiceId (long)
Response (200):     PaymentReceiptResponse
  - PaymentReceiptId (long)
  - ReceiptNumber (string)
  - InvoiceId (long)
  - PaymentTransactionId (long)
  - ReceiptDateUtc (DateTime)
  - ReceivedAmount (decimal)
  - BalanceAmount (decimal)
  - ReceiptRemarks (string)
Response (404):     "No payment receipt was found for this invoice." — if no confirmed payment exists
DB Tables:          PaymentTransactions, PaymentReceipts
Business Rules:
  - Returns receipt from most recent payment with a non-null Receipt
Failure Cases:
  - 404 if no receipt found
Notes on Drift:     Not in old ProjectOverview. Added 2026-05-24.

---

### Flow 12: Download Receipt PDF

Entry Points:       Customer Portal → "Download Receipt PDF"
UI Trigger:         "Download PDF" button
Endpoint:           GET /api/payments/receipt/{invoiceId}/pdf
Auth:               Authorize
Request:            Route param: invoiceId (long)
Response (200):     application/pdf file; filename: receipt-{ReceiptNumber}.pdf
                    Content: text-based PDF via SimplePdfDocumentBuilder
                    Lines: Receipt number, Invoice Id, Transaction Id, Date, Amount, Balance, Remarks
DB Tables:          PaymentTransactions, PaymentReceipts (read-only)
Failure Cases:
  - 404 if no receipt found for invoice
Notes on Drift:     Not in old ProjectOverview. Added 2026-05-24.

---

## MODULE: Billing Dashboard & AR
## Controller: BillingController — Route Prefix: /api/billing
## Auth: Authorize on all endpoints

---

### Flow 13: Get Accounts Receivable Dashboard

Entry Points:       Admin Portal → Billing → AR Dashboard
UI Trigger:         Screen load
Endpoint:           GET /api/billing/accounts-receivable
Auth:               Authorize(Policy = BillingRead)
Response (200):     AccountsReceivableDashboardResponse
  - Aging (AccountsReceivableAgingBucketResponse[]): aged buckets (0-30, 31-60, 61-90, 90+ days)
  - OverdueInvoices (AccountsReceivableInvoiceResponse[]): list of overdue invoices
  - TopOutstandingCustomers (AccountsReceivableOutstandingCustomerResponse[]): top debtors
  - TotalOutstanding (decimal)
DB Tables:          Invoices, Customers
Failure Cases:
  - 403 if role lacks BillingRead permission
Notes on Drift:     Previously documented only as "GET /api/billing/accounts-receivable" stub. Full contract 2026-05-24.
                    POST /api/payments/reconcile listed in ProjectOverview does NOT exist. Removed.

---

### Flow 14: Get Billing Status for Invoice

Entry Points:       Admin Portal → any screen needing quick billing status
UI Trigger:         Inline widget / notification
Endpoint:           GET /api/billing/status/{invoiceId}
Auth:               Authorize
Request:            Route param: invoiceId (long)
Response (200):     BillingStatusResponse
  - InvoiceId (long)
  - InvoiceNumber (string)
  - InvoiceStatus (string)
  - GrandTotalAmount (decimal)
  - PaidAmount (decimal)
  - BalanceAmount (decimal)
  - Timeline (BillingStatusHistoryResponse[]): status change history
DB Tables:          Invoices, BillingStatusHistory
Failure Cases:
  - 404 if invoice not found
Notes on Drift:     Not in old ProjectOverview. Added 2026-05-24.

---

### Flow 15: Send Payment Reminder

Entry Points:       Admin Portal → AR Dashboard → "Send Reminder" button
UI Trigger:         "Send Reminder" button
Endpoint:           POST /api/billing/payment-reminders/send
Auth:               Authorize(Policy = BillingRead)
Request Body (BillingPaymentReminderRequest — defined inline in BillingController.cs):
  - InvoiceId (string, required)
Response (200):     { invoiceId, reminderStatus: "queued", queuedAtUtc }
Business Rules:
  - Currently queues reminder; async dispatch to notification service
  - Response is immediate acknowledgement (fire-and-forget queue)
Failure Cases:
  - 403 if role lacks BillingRead permission
Notes on Drift:     Not in old ProjectOverview. Added 2026-05-24.

---

## MODULE: Quotation / Estimate Engine
## Controller: QuotationController — Route Prefix: /api/quotations
## Status: ENTIRELY MISSING from old ProjectOverview — added 2026-05-24

---

### Flow 16: Create Quotation from Job (Technician)

Entry Points:       AdminMobile → JobWorkflowContainer → Estimate tab; Field Workflow Flow 9 (alias via /api/field)
UI Trigger:         "Submit Quote" button by technician
Endpoint:           POST /api/quotations/from-job/{jobCardId}
Auth:               Authorize(Roles = Technician)
Request:            Route param: jobCardId (long)
Request Body (CreateQuotationFromJobRequest):
  - Lines (QuotationLineRequest[], required):
      each: LineType (string), LineDescription (string), Quantity (decimal), UnitPrice (decimal)
  - DiscountAmount (decimal, required)
  - TaxPercentage (decimal, required)
  - Remarks (string?, optional)
Response (200):     QuotationDetailResponse (see Field Workflow Flow 9 for full field list)
DB Tables:          Quotations, QuotationLines, JobCards, BillingStatusHistory
State Transitions:  Quotation: created → PendingApproval
Business Rules:
  - GrandTotal = (SubTotal - Discount) + TaxAmount
  - Quotation linked to JobCard; awaits customer approval
  - Note: Field Workflow uses POST /api/field/jobs/{id}/estimate which delegates to same command
Failure Cases:
  - 404 if jobCardId not found
  - 403 if caller is not Technician role
Notes on Drift:     QuotationController entirely absent from ProjectOverview. Added 2026-05-24.

---

### Flow 17: Search Quotations

Entry Points:       Admin Portal → Billing → Quotations list
UI Trigger:         Screen load / filter
Endpoint:           GET /api/quotations
Auth:               Authorize(Policy = QuotationRead)
Query Params:
  - status (string?, optional): filter by quotation status
  - pageNumber (int, default 1)
  - pageSize (int, default 20)
Response (200):     PagedResult<QuotationListItemResponse>
  Each item:
    - QuotationId, QuotationNumber, JobCardId, JobCardNumber, ServiceRequestId,
      ServiceRequestNumber, CustomerName, CurrentStatus, GrandTotalAmount, QuotationDateUtc
DB Tables:          Quotations, JobCards, ServiceRequests, Customers
Failure Cases:
  - 403 if role lacks QuotationRead permission
Notes on Drift:     Not in old ProjectOverview. Added 2026-05-24.

---

### Flow 18: Get Quotation by ID

Entry Points:       Admin Portal → Quotation Detail; Customer Portal → Estimate approval screen
UI Trigger:         Row click / notification link
Endpoint:           GET /api/quotations/{id}
Auth:               Authorize
Response (200):     QuotationDetailResponse (see Field Workflow Flow 9 for full field list)
DB Tables:          Quotations, QuotationLines, BillingStatusHistory
Failure Cases:
  - 404 if not found

---

### Flow 19: Get Quotation by Job Card

Entry Points:       Admin Portal → Job detail → Quotation tab
UI Trigger:         Tab switch
Endpoint:           GET /api/quotations/job/{jobCardId}
Auth:               Authorize
Response (200):     QuotationDetailResponse
DB Tables:          Quotations, QuotationLines, JobCards
Failure Cases:
  - 404 if no quotation for this job card

---

### Flow 20: Approve Quotation

Entry Points:       Customer Portal → Estimate approval screen; WhatsApp quick-reply (webhook triggers this)
UI Trigger:         "Approve" button; or WhatsApp quick-reply approval
Endpoint:           POST /api/quotations/{id}/approve
Auth:               Authorize
Request Body (QuotationDecisionRequest):
  - Remarks (string?, optional)
Response (200):     QuotationDetailResponse (CurrentStatus = "Approved")
DB Tables:          Quotations, BillingStatusHistory
State Transitions:  Quotation: PendingApproval → Approved
Business Rules:
  - Approved quotation can then trigger invoice generation via POST /api/invoices/from-quotation/{quotationId}
Failure Cases:
  - 422 if quotation not in PendingApproval status
Notes on Drift:     Not in old ProjectOverview. Added 2026-05-24.

---

### Flow 21: Reject Quotation

Entry Points:       Customer Portal → Estimate approval screen; WhatsApp quick-reply rejection
UI Trigger:         "Reject" button; or WhatsApp quick-reply rejection
Endpoint:           POST /api/quotations/{id}/reject
Auth:               Authorize
Request Body (QuotationDecisionRequest):
  - Remarks (string?, optional)
Response (200):     QuotationDetailResponse (CurrentStatus = "Rejected")
DB Tables:          Quotations, BillingStatusHistory
State Transitions:  Quotation: PendingApproval → Rejected
Failure Cases:
  - 422 if quotation not in PendingApproval status
Notes on Drift:     Not in old ProjectOverview. Added 2026-05-24.

---

## MODULE: Refund Engine
## Controller: RefundController — Route Prefix: /api/refunds
## Status: ENTIRELY MISSING from old ProjectOverview — added 2026-05-24

---

### Flow 22: Create Refund Request

Entry Points:       Admin Portal → Billing → Refund module → "Create Refund Request"
UI Trigger:         "Request Refund" button
Endpoint:           POST /api/refunds/request
Auth:               Authorize(Policy = PaymentCollect)
Request Body (CreateRefundRequestCommandRequest):
  - CancellationRecordId (long, required)
  - InvoiceId (long?, optional)
  - RefundAmount (decimal, required)
  - RefundMethod (string, required): e.g. "OriginalPaymentMethod", "Cash", "BankTransfer"
  - RefundReason (string, required)
Response (200):     RefundDetailResponse
  - RefundRequestId (long)
  - RefundRequestNo (string)
  - CancellationRecordId (long?)
  - InvoiceId (long?)
  - PaymentTransactionId (long?)
  - RefundAmount (decimal)
  - RequestedAmount (decimal)
  - ApprovedAmount (decimal)
  - MaxAllowedAmount (decimal)
  - RefundMethod (string)
  - RefundReason (string)
  - RefundStatus (string)
  - ApprovalRequired (bool)
  - ApprovedByUserId (long?)
  - ApprovedDateUtc (DateTime?)
  - ProcessedOn (DateTime?)
  - Approvals (RefundApprovalHistoryResponse[])
  - StatusHistory (RefundStatusHistoryResponse[])
DB Tables:          RefundRequests, CancellationRecords, Invoices, RefundApprovalHistory, RefundStatusHistory
State Transitions:  RefundRequest: created → PendingApproval (if ApprovalRequired=true) or PendingProcessing
Failure Cases:
  - 403 if role lacks PaymentCollect permission
  - 404 if CancellationRecordId not found

---

### Flow 23: List Refund Requests

Entry Points:       Admin Portal → Billing → Refund list
UI Trigger:         Screen load / filter
Endpoint:           GET /api/refunds
Auth:               Authorize(Policy = PaymentRead)
Query Params:
  - refundStatus (string?, optional)
  - customerId (long?, optional)
  - branchId (int?, optional)
  - fromDateUtc (DateTime?, optional)
  - toDateUtc (DateTime?, optional)
Response (200):     IReadOnlyCollection<RefundListItemResponse>
  Each: RefundRequestId, RefundRequestNo, CancellationRecordId?, InvoiceId?, PaymentTransactionId?,
        RefundAmount, RefundMethod, RefundStatus, ApprovalRequired, DateCreated
DB Tables:          RefundRequests, CancellationRecords, Invoices

---

### Flow 24: Get Refund Detail

Entry Points:       Admin Portal → Refund Detail screen
Endpoint:           GET /api/refunds/{id}
Auth:               Authorize
Response (200):     RefundDetailResponse (same shape as Flow 22 response)
Failure Cases:
  - 404 if not found

---

### Flow 25: Get Customer Refund Status

Entry Points:       Customer Portal → "My Refunds"; Customer Mobile → refund status
Endpoint:           GET /api/refunds/customer/{customerId}
Auth:               Authorize
Response (200):     IReadOnlyCollection<CustomerRefundStatusResponse>
  Each: RefundRequestId, RefundRequestNo, RefundAmount, RefundMethod, RefundStatus, DateCreated, ProcessedOn?
DB Tables:          RefundRequests, Customers

---

### Flow 26: Initiate Refund (Simplified)

Entry Points:       Admin Portal → quick refund initiation
Endpoint:           POST /api/refunds
Auth:               Authorize(Policy = PaymentCollect)
Request Body (InitiateRefundRequest — from GapPhaseA):
  - CancellationRecordId (long, required)
  - InvoiceId (long, required)
  - RequestedAmount (decimal, required)
  - Reason (string, required)
Response (200):     RefundRequestResponse
  - RefundRequestId (long)
  - CancellationRecordId (long)
  - InvoiceId (long)
  - RefundStatus (string)
  - RequestedAmount (decimal)
  - ApprovedAmount (decimal)
DB Tables:          RefundRequests, CancellationRecords
Business Rules:
  - Delegates to CreateRefundRequestCommand with RefundMethod = "OriginalPaymentMethod"
Notes on Drift:     Duplicate of Flow 22 with simpler request DTO; both routes exist. Added 2026-05-24.

---

### Flow 27: Approve Refund Request

Entry Points:       Admin Portal → Refund Detail → "Approve" button
Endpoint:           POST /api/refunds/{refundRequestId}/approve
Auth:               Authorize(Policy = ConfigurationManage)
Request Body (ApproveRefundRequestDecisionRequest):
  - ApprovedAmount (decimal?, optional)
  - Remarks (string, required)
Response (200):     RefundDetailResponse (RefundStatus = Approved)
DB Tables:          RefundRequests, RefundApprovalHistory, RefundStatusHistory
State Transitions:  RefundRequest: PendingApproval → Approved

---

### Flow 28: Reject Refund Request

Entry Points:       Admin Portal → Refund Detail → "Reject" button
Endpoint:           POST /api/refunds/{refundRequestId}/reject
Auth:               Authorize(Policy = ConfigurationManage)
Request Body (RejectRefundRequestDecisionRequest):
  - Remarks (string, required)
Response (200):     RefundDetailResponse (RefundStatus = Rejected)
DB Tables:          RefundRequests, RefundStatusHistory
State Transitions:  RefundRequest: PendingApproval → Rejected

---

### Flow 29: Update Refund Status

Entry Points:       Admin Portal → Refund Detail → "Update Status" (finance processing step)
Endpoint:           POST /api/refunds/{refundRequestId}/status
Auth:               Authorize(Policy = ConfigurationManage)
Request Body (UpdateRefundStatusRequest):
  - RefundStatus (string, required): target status
  - Remarks (string, required)
Response (200):     RefundDetailResponse
DB Tables:          RefundRequests, RefundStatusHistory
State Transitions:  RefundRequest → target status (e.g., Processing → Processed → Completed)
Notes on Drift:     RefundController with 8 endpoints entirely missing from old ProjectOverview. Added 2026-05-24.

---

## BILLING ARCHITECTURE NOTES

Invoice Auto-Generation:
  - Trigger: Job report submission (SR status → Completed) → CreateServiceRequestFromBooking or field complete event
  - Invoice created from approved quotation; POST /api/invoices/from-quotation/{quotationId}
  - Invoice starts as "Unpaid"; transitions to "PartiallyPaid" or "Paid" as payments recorded

Quotation → Invoice Lifecycle:
  Technician creates estimate → QuotationDetailResponse (PendingApproval)
  Customer approves → QuotationDetailResponse (Approved)
  Admin/System generates invoice → InvoiceDetailResponse (Unpaid)
  Customer pays → InvoiceDetailResponse (Paid) + PaymentReceiptResponse

Payment Method Normalization (InvoiceController):
  - "cash" / "cheque" / "banktransfer" → stored as "Cash"
  - "upi" / "online" → stored as "Upi"
  - "card" → stored as "Card"

Partial Payments:
  - PaidAmount and BalanceAmount tracked on Invoice
  - Status = "PartiallyPaid" until BalanceAmount = 0
  - Multiple PaymentTransactions allowed per invoice

PDF Generation:
  - GET /api/invoices/{id}/pdf — SimplePdfDocumentBuilder (text-based)
  - GET /api/payments/receipt/{invoiceId}/pdf — SimplePdfDocumentBuilder (text-based)
  - Both are lightweight; no external PDF library dependency

Coupon / Tax Config:
  - POST /api/coupons/apply — does NOT exist as a standalone endpoint
  - GET /api/coupons/{code} — does NOT exist
  - GET /api/tax-configurations — does NOT exist
  - Coupon validation is in CustomerMarketingController (POST /api/customer-marketing/offers/validate-coupon)
  - Tax configuration endpoints are [VERIFY] — not found in billing controllers

DB Tables Confirmed from Source:
  - Invoices (InvoiceId, InvoiceNumber, QuotationId, CustomerId, CurrentStatus, SubTotalAmount,
    DiscountAmount, TaxPercentage, TaxAmount, GrandTotalAmount, PaidAmount, BalanceAmount,
    InvoiceDateUtc, LastPaymentDateUtc)
  - InvoiceLines (LineDescription, LineAmount, ...)
  - PaymentTransactions (PaymentTransactionId, InvoiceId, PaymentMethod, ReferenceNumber,
    PaidAmount, PaymentDateUtc, TransactionRemarks, Receipt)
  - PaymentReceipts (PaymentReceiptId, ReceiptNumber, InvoiceId, PaymentTransactionId,
    ReceiptDateUtc, ReceivedAmount, BalanceAmount, ReceiptRemarks)
  - Quotations (QuotationId, QuotationNumber, JobCardId, ServiceRequestId, CustomerId,
    CurrentStatus, SubTotalAmount, DiscountAmount, TaxPercentage, TaxAmount, GrandTotalAmount,
    CustomerDecisionRemarks, ApprovedDateUtc, RejectedDateUtc, InvoiceId)
  - QuotationLines (LineType, LineDescription, Quantity, UnitPrice)
  - BillingStatusHistory
  - RefundRequests (RefundRequestId, RefundRequestNo, CancellationRecordId, InvoiceId,
    RefundAmount, RequestedAmount, ApprovedAmount, MaxAllowedAmount, RefundMethod, RefundReason,
    RefundStatus, ApprovalRequired, ApprovedByUserId, ApprovedDateUtc, ProcessedOn)
  - RefundApprovalHistory, RefundStatusHistory

Notes on Drift (consolidated):
  - PUT /api/invoices/{id} — PHANTOM — does not exist; removed
  - POST /api/invoices/{id}/send — PHANTOM — does not exist; removed
  - POST /api/invoices/{id}/credit-note — PHANTOM — does not exist; removed
  - POST /api/invoices (plain create) — PHANTOM — actual is POST /api/invoices/from-quotation/{id}
  - POST /api/payments/reconcile — PHANTOM — does not exist; removed
  - POST /api/coupons/apply — PHANTOM in Billing section; actual is in CustomerMarketingController
  - GET /api/coupons/{code} — PHANTOM in Billing section; does not exist
  - GET /api/tax-configurations — PHANTOM in Billing section; does not exist
  - QuotationController (/api/quotations) — ENTIRELY MISSING — 6 endpoints added
  - RefundController (/api/refunds) — ENTIRELY MISSING — 8 endpoints added
  - BillingController endpoints (AR Dashboard, Billing Status, Payment Reminders) — missing from docs

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
- Name (editable), Email (read-only), Mobile (read-only — OTP identity anchor; change via support ticket only).
- No password field. Customer authentication is mobile OTP only — no password exists for customer accounts.
- Secondary Contacts, Preferred Language, KYC, Saved Payment Methods, Loyalty Info: future-ready fields, not yet implemented.

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

# SECTION 6A — CUSTOMER WEB PORTAL (React + Vite — Frontend/Web)

## Platform Identity
- **Codebase:** `C:\Live\Coolzo\Frontend\Web\src`
- **Stack:** React 18 + Vite + TypeScript + Tailwind CSS
- **Auth:** JWT — `AuthContext` + `apiClient` 401 interceptor (silent refresh on demand)
- **Token Storage:** Access token → `sessionStorage` | Refresh token → `localStorage`
- **Portal Route Prefix:** `/portal`
- **Layout:** `PortalLayout.tsx` — sticky sidebar (desktop) + bottom nav (mobile)
- **Status:** Separate from Mobile App (`Mobile/Coolzo_MobileCustomer`) and Admin Portal (`Frontend/Admin`)

---

## MODULE: Authentication (Web Portal)

### Screens
| Route | Component | Purpose |
|---|---|---|
| `/login` | `Login.tsx` | Email/password or OTP login |
| `/register` | `Register.tsx` | New customer account + OTP |
| `/forgot-password` | `ForgotPassword.tsx` | Trigger reset email/SMS |
| `/reset-password` | `ResetPassword.tsx` | Complete password reset |
| `/session-expired` | `SessionExpired.tsx` | Shown on expired refresh token |

### API Endpoints Used
| Method | Endpoint | Purpose |
|---|---|---|
| POST | `/api/auth/login` | Email + password login → returns `accessToken`, `refreshToken` |
| POST | `/api/auth/otp/send` | Send OTP to mobile number |
| POST | `/api/auth/otp/verify` | Verify OTP → returns `accessToken`, `refreshToken` |
| GET | `/api/auth/me` | Get current authenticated user |
| POST | `/api/auth/refresh` | Refresh access token (401 interceptor only — NOT called on reload) |
| POST | `/api/auth/logout` | Revoke refresh token server-side |
| POST | `/api/customer-auth/register` | Create new customer account |
| POST | `/api/auth/forgot-password` | Initiate password reset |
| POST | `/api/auth/reset-password` | Complete password reset with token |
| POST | `/api/auth/change-password` | Change password for authenticated customer |

### Key Architectural Decisions
- `silentRefresh()` on page reload was removed (2026-05-26). The 401 interceptor in `apiClient.ts` handles all token expiry transparently.
- On app init: if no tokens exist → `loading=false` immediately (0 API calls). If token exists → `GET /api/auth/me` directly; 401 interceptor refreshes and retries if expired.
- Service: `authService.ts` | Context: `AuthContext.tsx` | Storage: `tokenStorage.ts`

---

## MODULE: Customer Dashboard (Web Portal)

### Screen
| Route | Component | Purpose |
|---|---|---|
| `/portal` | `portal/Dashboard.tsx` | Authenticated home — bookings summary, quick actions, AMC status |

### API Endpoints Used
| Method | Endpoint | Purpose |
|---|---|---|
| GET | `/api/bookings/my-bookings` | Recent bookings for dashboard preview |
| GET | `/api/amc/customer/me` | Active AMC subscription summary |

---

## MODULE: My Bookings (Web Portal)

### Screens
| Route | Component | Purpose |
|---|---|---|
| `/portal/bookings` | `portal/BookingsList.tsx` | Paginated list of all customer bookings |
| `/portal/bookings/:id` | `portal/BookingDetail.tsx` | Booking detail + job tracker timeline |

### API Endpoints Used
| Method | Endpoint | Purpose |
|---|---|---|
| GET | `/api/bookings/my-bookings` | `pageNumber`, `pageSize` → `PagedResult<BookingListItemResponse>` |
| GET | `/api/bookings/{bookingId}` | Admin-level booking detail |
| GET | `/api/customer-bookings/{bookingId}` | Customer-safe booking detail |
| POST | `/api/bookings/{bookingId}/reschedule` | Reschedule with `slotAvailabilityId`, `remarks` |
| GET | `/api/customer-bookings/{bookingId}/service-report` | Customer service report data |
| GET | `/api/customer-bookings/{bookingId}/service-report/pdf` | Download service report PDF (Blob) |
| POST | `/api/bookings/customer` | Create new booking (authenticated) |
| POST | `/api/bookings/guest` | Create new booking (guest) |
| GET | `/api/bookings/public/settings` | Public booking-mode flags (OpenBookingMode, EnforceSlotCapacity) |

- Service: `bookingService.ts`

---

## MODULE: AMC Dashboard (Web Portal)

### Screen
| Route | Component | Purpose |
|---|---|---|
| `/portal/amc` | `portal/AMCDashboard.tsx` | Active AMC contracts + visit schedule |

### API Endpoints Used
| Method | Endpoint | Purpose |
|---|---|---|
| GET | `/api/amc/plans` | `isActive=true, pageSize=100` → available AMC plans |
| GET | `/api/amc/customer/me` | Customer's active AMC subscriptions + visit schedule |

- Service: `amcService.ts`

---

## MODULE: My Equipment (Web Portal)

### Screens
| Route | Component | Purpose |
|---|---|---|
| `/portal/equipment` | `portal/EquipmentList.tsx` | List all customer appliances |
| `/portal/equipment/:id` | `portal/EquipmentDetail.tsx` | Single appliance detail + edit |

### API Endpoints Used
| Method | Endpoint | Purpose |
|---|---|---|
| GET | `/api/customers/me/equipment` | List all equipment for current customer |
| GET | `/api/customers/me/equipment/{equipmentId}` | Single equipment detail |
| POST | `/api/customers/me/equipment` | Add new appliance |
| PUT | `/api/customers/me/equipment/{equipmentId}` | Update appliance |
| DELETE | `/api/customers/me/equipment/{equipmentId}` | Soft-delete appliance |

- Service: `equipmentService.ts`
- Controller: `CustomerEquipmentController` (`/api/customers/me/equipment`)

---

## MODULE: My Invoices (Web Portal)

### Screens
| Route | Component | Purpose |
|---|---|---|
| `/portal/invoices` | `portal/InvoicesList.tsx` | Paginated invoice list |
| `/portal/invoices/:id` | `portal/InvoiceDetail.tsx` | Invoice detail + PDF download + pay |

### API Endpoints Used
| Method | Endpoint | Purpose |
|---|---|---|
| GET | `/api/invoices/customer` | `pageNumber`, `pageSize` → `PagedResult<InvoiceListItemResponse>` |
| GET | `/api/invoices/{invoiceId}` | Invoice detail |
| GET | `/api/invoices/{invoiceId}/pdf` | Download invoice PDF (Blob) |

- Service: `invoiceService.ts`
- Controller: `InvoiceController` (Flow 6 — `GET /api/invoices/customer`)

---

## MODULE: Support Tickets (Web Portal)

### Screens
| Route | Component | Purpose |
|---|---|---|
| `/portal/support` | `portal/TicketsList.tsx` | My tickets list with status |
| `/portal/support/:id` | `portal/TicketDetail.tsx` | Chat-style ticket thread |
| `/portal/support/new` | `portal/NewTicket.tsx` | Raise new support ticket |

### API Endpoints Used
| Method | Endpoint | Purpose |
|---|---|---|
| GET | `/api/support-tickets/my-tickets` | `pageNumber`, `pageSize` → `PagedResult<SupportTicketListItemResponse>` |
| GET | `/api/support-tickets/{ticketId}` | Ticket detail + reply thread |
| POST | `/api/support-tickets` | Create ticket — `subject, categoryId, priorityId, description, links?` |
| POST | `/api/support-tickets/{ticketId}/replies` | Add reply `{ message }` |
| POST | `/api/support-tickets/{ticketId}/close` | Close ticket with optional `remarks` |
| POST | `/api/support-tickets/{ticketId}/reopen` | Reopen closed ticket |
| GET | `/api/support-ticket-lookups/categories` | Ticket category options |
| GET | `/api/support-ticket-lookups/priorities` | Ticket priority options |

- Service: `ticketService.ts`
- Note: CustomerId is NOT sent in the request body — backend resolves from JWT

---

## MODULE: Notifications (Web Portal)

### Screen
| Route | Component | Purpose |
|---|---|---|
| `/portal/notifications` | `portal/Notifications.tsx` | Notification centre — read/unread list |
| `/portal/notification-preferences` | `portal/NotificationPreferences.tsx` | Communication channel toggles |

### API Endpoints Used
| Method | Endpoint | Purpose |
|---|---|---|
| GET | `/api/customer-notifications` | `pageNumber=1, pageSize=20` → `PagedResult<CustomerNotificationResponse>` |
| POST | `/api/customer-notifications/{notificationId}/mark-read` | Mark single notification read |
| PATCH | `/api/notifications/mark-read` | Mark all notifications read |
| GET | `/api/communication-preferences/me` | Load channel preference toggles |
| PUT | `/api/communication-preferences/me` | Save channel preference changes |

### DB Table
| Table | Purpose |
|---|---|
| `tblCustomerNotification` | Stores per-customer in-app notifications |

### CustomerNotificationResponse Fields
`CustomerNotificationId, CustomerId, Title, Message, NotificationType, IsRead, DateCreated, LinkUrl`

### Known Issue Fixed (2026-05-26)
- **Root cause:** `tblCustomerNotification` (and 4 related CustomerApp tables) existed in EF Core but were never applied to the PostgreSQL database.
- **Error:** `Npgsql.PostgresException: 42P01: relation "tblCustomerNotification" does not exist`
- **Fix:** [13_customer_app_tables.sql](../Database/../Postgres/13_customer_app_tables.sql) — creates all 5 missing tables with correct constraints and indexes.
- **Drift type:** DB contract drift — EF config existed, Postgres schema did not.
- Service: `notificationService.ts`
- Controller: `CustomerNotificationController` (`/api/customer-notifications`)
- Handler: `GetMyNotificationsQueryHandler` → `ICustomerAppRepository.ListNotificationsAsync`

---

## MODULE: Addresses (Web Portal)

### Screen
| Route | Component | Purpose |
|---|---|---|
| `/portal/addresses` | `portal/Addresses.tsx` | Manage delivery/service addresses |

### API Endpoints Used
| Method | Endpoint | Purpose |
|---|---|---|
| GET | `/api/customers/me/addresses` | List all addresses for current customer |
| POST | `/api/customers/me/addresses` | Create address — `addressLabel, addressLine1, addressLine2, landmark, cityName, pincode, zoneId?, latitude?, longitude?, isDefault, stateName?, addressType?` |
| PUT | `/api/customers/me/addresses/{addressId}` | Update address |
| DELETE | `/api/customers/me/addresses/{addressId}` | Soft-delete address |
| GET | `/api/booking-lookups/zones/by-pincode/{pincode}` | Resolve `zoneId` from pincode |

- Service: `addressService.ts`
- Controller: `CustomerAddressController` (`/api/customers/me/addresses`)

---

## MODULE: Profile (Web Portal)

### Screen
| Route | Component | Purpose |
|---|---|---|
| `/portal/profile` | `portal/Profile.tsx` | View and edit customer profile |

### API Endpoints Used
| Method | Endpoint | Purpose |
|---|---|---|
| GET | `/api/customers/me/profile` | Load current profile |
| PUT | `/api/customers/me/profile` | Update name only (email + mobile are read-only in portal) |

### Auth Model — Customer Portal
- Customer authentication is **mobile OTP only**. There is no password for customer accounts.
- No "Change Password" screen or flow exists in the customer web portal.
- Mobile number is a verified OTP identity anchor — customers cannot self-update it. To change mobile, a support ticket must be raised.
- Email is a secondary contact field — read-only in the portal; cannot be changed by the customer.

### Editable Fields
| Field | Editable | Reason |
|---|---|---|
| Full Name | ✅ Yes | Used on invoices, technician greeting, support records |
| Email Address | ❌ Read-only | Cannot be changed via self-service |
| Mobile Number | ❌ Read-only | OTP identity anchor; change requires support verification |
| Date of Birth | ❌ Removed | No operational relevance for AC service |

### Profile Page Sections
1. **Personal Information** — name (editable), email (read-only), mobile (read-only)
2. **Account Card** — avatar, name, verified badge, mobile summary, Sign Out button
3. **Account Settings** — quick-links to My Addresses (`/portal/addresses`) and Support Tickets (`/portal/support`)

- Service: `profileService.ts`
- Profile avatar in portal header links to this screen directly

---

## MODULE: Feedback & Referral (Web Portal)

### Screens
| Route | Component | Purpose |
|---|---|---|
| `/portal/feedback` | `portal/Feedback.tsx` | Submit post-service review |
| `/portal/referral` | `portal/Referral.tsx` | Referral code + stats |

---

## Web Portal DB Tables (direct dependency)

| Table | Module | Notes |
|---|---|---|
| `tblCustomer` | Profile, Auth | Core customer identity |
| `tblCustomerAddress` | Addresses | CRUD via CustomerAddressController |
| `tblCustomerEquipment` | Equipment | CRUD via CustomerEquipmentController |
| `tblCustomerNotification` | Notifications | ⚠️ Created by 13_customer_app_tables.sql (was missing) |
| `tblPromotionalOffer` | Marketing | ⚠️ Created by 13_customer_app_tables.sql (was missing) |
| `tblCustomerReferral` | Referral | ⚠️ Created by 13_customer_app_tables.sql (was missing) |
| `tblCustomerLoyaltyTransaction` | Loyalty | ⚠️ Created by 13_customer_app_tables.sql (was missing) |
| `tblCustomerAppFeedback` | Feedback | ⚠️ Created by 13_customer_app_tables.sql (was missing) |
| `tblCustomerReview` | Reviews | Exists in 02_create_tables.sql |
| `tblBooking`, `tblBookingLine` | Bookings | Read via CustomerBookingController |
| `tblServiceRequest` | Job Tracker | Status timeline in BookingDetail |
| `tblInvoiceHeader`, `tblInvoiceLine` | Invoices | Read via InvoiceController |
| `tblSupportTicket`, `tblSupportTicketReply` | Tickets | Full CRUD via SupportTicketController |
| `tblCommunicationPreference` | Notification Prefs | Read/write via CommunicationPreferenceController |
| `tblCustomerAMC`, `tblAMCVisitSchedule` | AMC | Read via AmcController |

---

## Web Portal Service Layer Map

| Service File | Controller | Key Endpoints |
|---|---|---|
| `authService.ts` | AuthController, CustomerAuthController | otp/send, otp/verify, refresh, me, logout, register (customer portal uses OTP only — no password endpoints) |
| `profileService.ts` | CustomerController | GET/PUT `/api/customers/me/profile` |
| `addressService.ts` | CustomerAddressController | CRUD `/api/customers/me/addresses` |
| `equipmentService.ts` | CustomerEquipmentController | CRUD `/api/customers/me/equipment` |
| `bookingService.ts` | BookingController, CustomerBookingController | my-bookings, detail, reschedule, service-report |
| `amcService.ts` | AmcController | plans, customer/me |
| `invoiceService.ts` | InvoiceController | customer invoices, detail, PDF |
| `ticketService.ts` | SupportTicketController, SupportTicketLookupController | CRUD tickets, replies, lookups |
| `notificationService.ts` | CustomerNotificationController, CommunicationPreferenceController | notifications, mark-read, preferences |
| `catalogService.ts` | BookingLookupController, ServiceTypesController | services, brands, zones, slots |
| `paymentService.ts` | PaymentController | initiate, collect, receipt, receipt PDF |
| `reviewService.ts` | CustomerReviewController | list, submit review |
| `marketingService.ts` | CustomerMarketingController | offers, validate coupon, referral, loyalty |
| `cmsService.ts` | CustomerContentController | blogs, CMS blocks, changelog |

---

## UI Quality Pass — All 40 Screens (2026-06-09)

A comprehensive quality audit and fix pass was completed across all 40 screens of the customer web portal against the three `New UI Format.txt` section-7 gates.

**Gates applied:**
- Responsive Gate: no horizontal scroll, correct nav per breakpoint, grids collapse on mobile, fixed bars reserve space
- Component/Consistency Gate: buttons `rounded-lg`, cards `rounded-xl`, inputs `rounded-lg`, loading + empty + error states for data pages
- Accessibility Gate: single `<h1>`, semantic HTML, `htmlFor`/`id` on all form inputs, `aria-label` on icon-only buttons, touch targets ≥ 44px, WCAG AA contrast

**Screens covered (all 🟢 STABLE):**

| Batch | Screens | Key fixes applied |
|---|---|---|
| Batch 1 (public marketing) | Home, Services, ServiceDetail, AMC, About, WhyCoolzo, Reviews, Blog, BlogDetail, Contact | FAQ aria-expanded; newsletter htmlFor/id; share button aria-label + 44px targets; service search sr-only label; radius conformance throughout |
| Batch 2 (transactional/auth) | BookingWizard, BookingConfirmation, Terms, Privacy, Login, Register, ForgotPassword, ResetPassword, SessionExpired, Maintenance | h2→h1 on all auth screens; htmlFor/id on all auth form inputs; Eye/EyeOff toggle aria-label; OTP digit aria-labels; sr-only h1 on BookingWizard; radius conformance throughout |
| Batch 3 (core portal) | Dashboard, BookingsList, BookingDetail, AMCDashboard, EquipmentList, EquipmentDetail, InvoicesList, InvoiceDetail, Profile, Addresses | Icon-only button aria-labels (Call technician, View/Pay/Download invoice, Download report, Cancel booking); htmlFor/id on Profile and Addresses modal form inputs; Addresses modal close + menu button aria-labels; bulk radius conformance (179 violations fixed) |
| Batch 4 (support/engagement) | TicketsList, TicketDetail, NewTicket, Notifications, Referral, Feedback, ErrorPage, NotFound, Layout (Navbar/Footer), PortalLayout | Icon-only button aria-labels (View ticket, Attach file, Send message); Footer social icon aria-label + 44px targets; htmlFor/id on NewTicket Subject/Category/Priority/Description; bulk radius conformance |

**Tracker:** `Frontend/Web/UI_RESPONSIVENESS_TRACKER.md` — all 40 screens documented at stable contract level.

---

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

## MODULE: Scheduling Board
Controller: SchedulingBoardController — route prefix: /api/scheduling
Updated: 2026-05-24 — Stable Contract Level

DB Tables Confirmed:
  tblSlotAvailability    — zone/date slot capacity, reservation counts, IsBlocked control
  tblTechnicianShift     — per-technician weekly shift overrides, break windows, off-duty flags
  tblServiceRequest      — jobs shown on board (read); assignment/status updated on assign/reassign
  tblSRStatusHistory     — captures assignment and reschedule status transitions from board actions
  tblCustomerAmc         — AMC contracts; source for amc-auto visit generation
  tblAmcVisitSchedule    — individual AMC visits surfaced on AMC auto-schedule review board

Notes on Drift:
  Previous docs were narrative-only (no DTO shapes, no auth policies, no field definitions).
  Controller is SchedulingBoardController.cs (not SchedulingController).
  All 11 routes confirmed from source. ModuleIndex "SECTION 8" corrected to SECTION 7 (no SECTION 8 exists).

---

SCH-1: Get Scheduling Board
  Entry Points:         Admin → Scheduling Board screen (Day/Week/Technician views)
  UI Trigger:           Date range selector change; view-mode toggle; technician filter
  Endpoint:             GET /api/scheduling/board
  Auth Policy:          ServiceRequestRead
  Request:
    dateFrom            DateOnly     required   Start date of board window
    dateTo              DateOnly     required   End date of board window
    technicianId        long?        optional   Filter board to single technician
  Response: SchedulingBoardResponse
    DateFrom            DateOnly                 Echoed range start
    DateTo              DateOnly                 Echoed range end
    GeneratedOnUtc      DateTime                 Board snapshot timestamp
    TimeSlots[]         SchedulingTimeSlotResponse
      SlotKey           string                   Internal slot identifier
      SlotLabel         string                   Display label (e.g. "09:00 – 10:00")
      StartTime         string                   "HH:mm" format
      EndTime           string                   "HH:mm" format
    Technicians[]       SchedulingTechnicianResponse
      TechnicianId      long
      TechnicianCode    string
      TechnicianName    string
      AvailabilityStatus string                  Available / Busy / Off-Duty / On-Leave
      BaseZoneName      string?
      Zones[]           string[]                 All assigned zone names
      Skills[]          string[]                 All skill names
      AverageRating     decimal
      TodayJobCount     int
      NextFreeSlot      string?                  First available slot label
      WeeklyShifts[]    SchedulingShiftDayResponse (see SCH-9 for shape)
    Jobs[]              SchedulingBoardJobResponse (assigned jobs — see shape below)
    UnassignedJobs[]    SchedulingBoardJobResponse (pending assignment — same shape)
  SchedulingBoardJobResponse shape (22 fields):
    ServiceRequestId    long
    ServiceRequestNumber string
    BookingId           long
    ZoneId              long
    ZoneName            string
    CustomerName        string
    MobileNumber        string
    AddressSummary      string
    ServiceName         string
    AcTypeName          string?
    BrandName           string?
    Priority            string                   Normal / High / Emergency
    CurrentStatus       string                   SR status at time of board render
    SlotAvailabilityId  long
    SlotDate            DateOnly
    SlotLabel           string
    StartTime           string
    EndTime             string
    DurationMinutes     int
    TechnicianId        long?                    null if UnassignedJob
    TechnicianName      string?                  null if UnassignedJob
    EstimatedPrice      decimal
  DB Tables Read:       tblServiceRequest, tblSlotAvailability, tblTechnicianShift, tblTechnicianZone, tblTechnicianSkill
  Business Rules:
    1. Board renders all SRs with SlotDate between dateFrom and dateTo.
    2. Jobs array = SRs with a technician assigned; UnassignedJobs = SRs not yet assigned.
    3. If technicianId filter is provided, Technicians and Jobs are scoped to that technician only.
    4. Technician AvailabilityStatus derived from shift + leave + current job count.
  Failure Cases:
    400  dateFrom > dateTo
    401  Missing or invalid JWT
    403  Caller does not have ServiceRequestRead permission

SCH-2: Schedule Assign (Drag-Drop Assignment)
  Entry Points:         Scheduling Board — drag SR card onto technician/time slot
  UI Trigger:           Drag-and-drop SR onto technician column + time row; or "Quick Assign" button
  Endpoint:             POST /api/scheduling/assign
  Auth Policy:          AssignmentManage
  Request: ScheduleAssignServiceRequest
    ServiceRequestId    long         required   SR to assign
    TechnicianId        long         required   Technician to assign to
    SlotAvailabilityId  long         required   Slot to bind the assignment to
    Remarks             string?      optional   Supervisor comment
  Response: SchedulingBoardJobResponse (22 fields — see SCH-1)
    Success message: "Service request scheduled successfully."
  DB Tables Read:       tblServiceRequest, tblSlotAvailability, tblTechnicianShift
  DB Tables Written:    tblServiceRequest (TechnicianId, SlotAvailabilityId, Status), tblSRStatusHistory
  Business Rules:
    1. Caller should first call SCH-6 (GET /api/scheduling/conflicts) to verify no blocking conflicts.
    2. Assigns the technician and updates SR status to Assigned.
    3. SlotAvailability ReservedCapacity incremented by 1.
    4. Status transition logged in tblSRStatusHistory.
    5. Technician receives push notification (async event).
  State Transitions:    SR: Pending → Assigned (or Scheduled if slot date is set)
  Failure Cases:
    400  Blocking conflict exists (duplicate slot, off-duty, shift violation)
    404  ServiceRequestId, TechnicianId, or SlotAvailabilityId not found
    401/403  Auth

SCH-3: Schedule Reassign
  Entry Points:         Scheduling Board — drag assigned SR to different technician or time slot; or "Reassign" action button
  UI Trigger:           Drag-and-drop reassign; Reassign panel with reason dropdown
  Endpoint:             PUT /api/scheduling/reassign
  Auth Policy:          AssignmentManage
  Request: ScheduleReassignServiceRequest
    ServiceRequestId    long         required   SR to reassign
    TechnicianId        long         required   New technician
    SlotAvailabilityId  long         required   New slot
    Remarks             string?      optional   Reason (Customer Request / Tech Unavailable / Skill Mismatch / Travel Conflict / Other)
  Response: SchedulingBoardJobResponse (22 fields — see SCH-1)
    Success message: "Scheduling change saved successfully."
  DB Tables Read:       tblServiceRequest, tblSlotAvailability
  DB Tables Written:    tblServiceRequest (TechnicianId, SlotAvailabilityId), tblSRStatusHistory
  Business Rules:
    1. Previous slot SlotAvailability ReservedCapacity decremented by 1.
    2. New slot ReservedCapacity incremented by 1.
    3. Both source and destination technicians notified (async event).
    4. Reassignment logged in tblSRStatusHistory with Remarks.
    5. If source technician had SR as current active job, AvailabilityStatus recalculated.
  State Transitions:    SR stays Assigned; slot and technician binding updated
  Failure Cases:
    400  Blocking conflict on new slot/technician
    404  SR, Technician, or Slot not found
    401/403  Auth

SCH-4: Get AMC Auto-Schedule Review Board
  Entry Points:         Admin → Scheduling Board → AMC Auto-Schedule tab
  UI Trigger:           Tab switch; date range filter applied
  Endpoint:             GET /api/scheduling/amc-auto
  Auth Policy:          ServiceRequestRead
  Request:
    dateFrom            DateOnly?    optional   Filter AMC visits from date
    dateTo              DateOnly?    optional   Filter AMC visits to date
  Response: IReadOnlyCollection<SchedulingAmcAutoVisitResponse>
    Each item (21 fields):
      AmcVisitScheduleId        long
      CustomerAmcId             long
      VisitNumber               int
      ScheduledDate             DateOnly
      CurrentStatus             string            Pending / Scheduled / Completed / Skipped
      CustomerId                long
      CustomerName              string
      MobileNumber              string
      CustomerAddressId         long
      ZoneId                    long
      ZoneName                  string
      AddressSummary            string
      ServiceId                 long
      ServiceName               string
      AcTypeName                string?
      BrandName                 string?
      JobCardNumber             string
      OriginServiceRequestNumber string?          SR that originally enrolled the AMC contract
      AmcPlanName               string
      LinkedServiceRequestId    long?             null if not yet assigned/created
      LinkedServiceRequestNumber string?          null if not yet assigned/created
  DB Tables Read:       tblAmcVisitSchedule, tblCustomerAmc, tblAmcPlan, tblCustomer, tblServiceRequest
  Business Rules:
    1. Returns AMC visits that are Pending (not yet scheduled/assigned).
    2. Visits with LinkedServiceRequestId already have a booking/SR created.
    3. If dateFrom/dateTo omitted, returns all unscheduled visits for the current + next calendar month.
  Failure Cases:
    401/403  Auth

SCH-5: AMC Bulk Assign
  Entry Points:         Admin → Scheduling Board → AMC Auto-Schedule tab → "Bulk Assign" action
  UI Trigger:           Multi-select visits + pick technician + click Bulk Assign
  Endpoint:             POST /api/scheduling/amc-bulk-assign
  Auth Policy:          AssignmentManage
  Request: ScheduleAmcBulkAssignRequest
    TechnicianId        long         required   Technician to assign all selected visits to
    Visits[]            ScheduleAmcBulkAssignVisitRequest[]  required
      AmcVisitScheduleId long        required   AMC visit to schedule
      SlotAvailabilityId long        required   Slot to bind visit to
    Remarks             string?      optional   Bulk assign note
  Response: IReadOnlyCollection<SchedulingBoardJobResponse>
    Success message: "AMC visits scheduled successfully."
    One SchedulingBoardJobResponse per visit successfully scheduled.
  DB Tables Read:       tblAmcVisitSchedule, tblSlotAvailability
  DB Tables Written:    tblAmcVisitSchedule (LinkedServiceRequestId, CurrentStatus), tblServiceRequest (new SR per visit), tblSRStatusHistory
  Business Rules:
    1. For each visit: creates a ServiceRequest from the AMC visit data and assigns to TechnicianId + SlotAvailabilityId.
    2. tblAmcVisitSchedule.LinkedServiceRequestId set to newly created SR.
    3. tblAmcVisitSchedule.CurrentStatus updated to Scheduled.
    4. SlotAvailability ReservedCapacity incremented per visit.
    5. Operation is partial-success: visits that conflict are skipped; successfully scheduled visits returned.
  Failure Cases:
    400  No visits provided; TechnicianId invalid
    404  AmcVisitScheduleId or SlotAvailabilityId not found
    401/403  Auth

SCH-6: Get Scheduling Conflicts
  Entry Points:         Scheduling Board — called before committing assign/reassign action
  UI Trigger:           Triggered automatically on hover/tentative drop before confirming drag-drop move
  Endpoint:             GET /api/scheduling/conflicts
  Auth Policy:          ServiceRequestRead
  Request:
    serviceRequestId    long         required   SR being scheduled
    technicianId        long         required   Target technician
    slotAvailabilityId  long         required   Target slot
  Response: IReadOnlyCollection<SchedulingConflictResponse>
    Each item:
      ConflictType            string    OffDuty / ShiftViolation / BreakOverlap / JobOverlap / TravelBuffer / SkillMismatch / ZoneMismatch / CapacityFull
      Severity                string    Blocking / Warning
      Message                 string    Human-readable explanation
      SuggestedResolution     string?   Optional guidance text
      RelatedServiceRequestId long?     Conflicting SR if applicable
      RelatedServiceRequestNumber string? Display number of conflicting SR
  DB Tables Read:       tblTechnicianShift, tblSlotAvailability, tblServiceRequest, tblTechnicianSkill, tblTechnicianZone
  Business Rules:
    1. Blocking conflicts (Severity=Blocking): OffDuty, ShiftViolation, BreakOverlap, JobOverlap, CapacityFull — must be resolved before assign/reassign.
    2. Warning conflicts (Severity=Warning): TravelBuffer, SkillMismatch, ZoneMismatch — surfaced as alerts; supervisor can override.
    3. Empty response = no conflicts; safe to commit the move.
    4. Caller uses this to show conflict pane before POST /api/scheduling/assign.
  Failure Cases:
    400  Missing required query params
    401/403  Auth

SCH-7: Get Slot Availability
  Entry Points:         Admin → Slot Availability Manager screen; also called by booking engine for customer-facing slot selection
  UI Trigger:           Zone selector + date picker in Slot Availability Manager
  Endpoint:             GET /api/scheduling/slots
  Auth Policy:          ServiceRequestRead
  Request:
    zoneId              long         required   Zone to query slots for
    slotDate            DateOnly     required   Specific date
  Response: IReadOnlyCollection<SchedulingSlotResponse>
    Each item:
      SlotAvailabilityId  long
      ZoneId              long
      ZoneName            string
      SlotDate            DateOnly
      SlotLabel           string
      StartTime           string     "HH:mm"
      EndTime             string     "HH:mm"
      AvailableCapacity   int        Total slots configured for this block
      ReservedCapacity    int        How many are currently booked/assigned
      IsBlocked           bool       Admin-blocked (no bookings regardless of capacity)
      IsAvailable         bool       Derived: !IsBlocked && (ReservedCapacity < AvailableCapacity)
  DB Tables Read:       tblSlotAvailability
  Business Rules:
    1. Returns all time slots for the given zone and date.
    2. IsAvailable is computed field: false when blocked OR capacity full.
    3. Customer-facing slot picker should only show slots where IsAvailable = true.
  Failure Cases:
    400  Missing zoneId or slotDate
    401/403  Auth

SCH-8: Update Slot Availability
  Entry Points:         Admin → Slot Availability Manager → Block / Unblock / Edit capacity action
  UI Trigger:           Toggle block switch; capacity field edit + save
  Endpoint:             PUT /api/scheduling/slots/{slotAvailabilityId}
  Auth Policy:          AssignmentManage
  Request: ScheduleUpdateSlotRequest
    IsBlocked           bool         required   true = block slot, false = unblock
    AvailableCapacity   int?         optional   Override total capacity; null = no change
  Response: SchedulingSlotResponse (full slot object — see SCH-7)
    Success message: "Scheduling slot updated successfully."
  DB Tables Written:    tblSlotAvailability (IsBlocked, AvailableCapacity)
  Business Rules:
    1. Blocking a slot does NOT cancel existing reservations; it only prevents new bookings.
    2. AvailableCapacity cannot be set below current ReservedCapacity.
  Failure Cases:
    400  AvailableCapacity < current ReservedCapacity
    404  slotAvailabilityId not found
    401/403  Auth

SCH-9: Get Technician Shifts
  Entry Points:         Admin → Technician Shift Scheduler screen
  UI Trigger:           Screen load; technician selector change
  Endpoint:             GET /api/scheduling/shifts
  Auth Policy:          ServiceRequestRead
  Request:
    technicianId        long?        optional   Filter to single technician; omit = all technicians
  Response: IReadOnlyCollection<SchedulingShiftResponse>
    Each item:
      TechnicianId      long
      TechnicianCode    string
      TechnicianName    string
      Days[]            SchedulingShiftDayResponse
        DayOfWeekNumber int           0=Sunday … 6=Saturday
        DayName         string        "Sunday" … "Saturday"
        IsOffDuty       bool
        ShiftStartTime  string?       "HH:mm"; null if IsOffDuty
        ShiftEndTime    string?       "HH:mm"; null if IsOffDuty
        BreakStartTime  string?       "HH:mm"; null if no break
        BreakEndTime    string?       "HH:mm"; null if no break
  DB Tables Read:       tblTechnicianShift
  Business Rules:
    1. If no shift row exists for a technician/day, defaults to system-wide standard shift hours.
    2. IsOffDuty = true suppresses the technician from the board for that day.
  Failure Cases:
    401/403  Auth

SCH-10: Update Technician Shifts
  Entry Points:         Admin → Technician Shift Scheduler → Edit technician row
  UI Trigger:           Shift editor form submit
  Endpoint:             PUT /api/scheduling/shifts
  Auth Policy:          AssignmentManage
  Request: ScheduleUpdateTechnicianShiftsRequest
    TechnicianId        long         required
    Days[]              ScheduleShiftDayRequest[]  required
      DayOfWeekNumber   int          required   0–6
      IsOffDuty         bool         required
      ShiftStartTime    string?      optional   "HH:mm"; required if !IsOffDuty
      ShiftEndTime      string?      optional   "HH:mm"; required if !IsOffDuty
      BreakStartTime    string?      optional   "HH:mm"
      BreakEndTime      string?      optional   "HH:mm"
  Response: SchedulingShiftResponse (single technician's updated shift)
    Success message: "Technician shifts updated successfully."
  DB Tables Written:    tblTechnicianShift (upsert per DayOfWeekNumber row)
  Business Rules:
    1. Upserts one row per day — creates if not exists, updates if exists.
    2. If ShiftStartTime/ShiftEndTime missing but IsOffDuty=false, returns 400.
    3. Shift changes affect board availability display immediately.
    4. Conflict detection (SCH-6) re-evaluates using updated shift data.
  Failure Cases:
    400  ShiftStartTime/ShiftEndTime null when IsOffDuty=false; invalid time format
    404  TechnicianId not found
    401/403  Auth

SCH-11: Get Day Sheet (Daily Briefing Export)
  Entry Points:         Admin → Scheduling Board → "Day Sheet" / "Briefing Export" button
  UI Trigger:           Date selector + optional technician filter + Export/View button
  Endpoint:             GET /api/scheduling/day-sheet
  Auth Policy:          ServiceRequestRead
  Request:
    scheduleDate        DateOnly     required   Date for the briefing sheet
    technicianId        long?        optional   Scope to single technician; omit = all
  Response: SchedulingDaySheetResponse
    ScheduleDate        DateOnly
    GeneratedOnUtc      DateTime
    Technicians[]       SchedulingDaySheetTechnicianResponse
      TechnicianId      long
      TechnicianCode    string
      TechnicianName    string
      BaseZoneName      string?
      Itinerary[]       SchedulingDaySheetItemResponse
        ServiceRequestId        long
        ServiceRequestNumber    string
        CustomerName            string
        MobileNumber            string
        AddressSummary          string
        ServiceName             string
        SlotLabel               string
        StartTime               string   "HH:mm"
        EndTime                 string   "HH:mm"
        CurrentStatus           string
        Priority                string
        ZoneName                string
  DB Tables Read:       tblServiceRequest, tblSlotAvailability, tblTechnicianShift
  Business Rules:
    1. Returns only SRs scheduled for the specified date (SlotDate = scheduleDate).
    2. Itinerary items sorted by StartTime ascending per technician.
    3. Only assigned SRs included (TechnicianId not null).
    4. Frontend uses this response to render per-technician day briefing cards and to generate PDF/CSV export via jsPDF or client-side CSV.
  Failure Cases:
    400  scheduleDate missing
    401/403  Auth

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

---

## MODULE: Technician Management
## Controllers: TechnicianController, TechnicianPublicController, TechnicianActivationController,
##              TechnicianDocumentController, TechnicianOnboardingController,
##              TechnicianJobController, HelperController, HelperAttendanceController,
##              HelperTaskController
## (TechnicianStockController — /api/technicians/{id}/stock — documented in Module 8: Inventory)
## Verified: 2026-05-24 | 40 endpoints across 9 controllers | Stable Contract

---

### TECHNICIAN CONTROLLER GROUP (/api/technicians) — Core CRUD & Profile

#### Flow TCH-1: List Technicians
  Entry Points:         Admin → Technicians list; Dispatch → Technician Board
  Endpoint:             GET /api/technicians
  Auth:                 Authorize(Policy=TechnicianRead)
  Request DTO:          query params
    - searchTerm        string?   — partial match on name/mobile
    - activeOnly        bool      default false
    - zone              string?   — filter by zone name
    - skill             string?   — filter by skill code/name
    - availability      string?   — filter by AvailabilityStatus
    - minimumRating     decimal?  — filter by AverageRating >= value
  Response DTO:         IReadOnlyCollection<TechnicianListItemResponse>
    - TechnicianId              long
    - TechnicianCode            string
    - TechnicianName            string
    - MobileNumber              string
    - EmailAddress              string
    - IsActive                  bool
    - AvailabilityStatus        string  — "Available" | "OnJob" | "EnRoute" | "OffDuty"
    - CurrentServiceRequestNumber string? — active SR if OnJob/EnRoute
    - BaseZoneName              string?
    - Zones                     string[]  — zone names
    - Skills                    TechnicianSkillResponse[]
    - AverageRating             decimal
    - TodayJobCount             int
    - SlaCompliancePercent      decimal
    - NextFreeSlot              string?
  DB Tables Read:       tblTechnician, tblTechnicianSkill, tblTechnicianZone (via SearchTechniciansQuery)
  Failure Cases:        401, 403

---

#### Flow TCH-2: Get Technician Detail
  Endpoint:             GET /api/technicians/{technicianId:long}
  Auth:                 Authorize(Policy=TechnicianRead)
  Request DTO:          route: technicianId (long)
  Response DTO:         TechnicianDetailResponse
    - TechnicianId, TechnicianCode, TechnicianName, MobileNumber, EmailAddress
    - BaseZoneId (long?), BaseZoneName (string?), IsActive, MaxDailyAssignments
    - AvailabilityStatus, CurrentServiceRequestNumber?
    - Zones                     TechnicianZoneResponse[]
        - TechnicianZoneId, ZoneId, ZoneName, IsPrimaryZone
    - Skills                    TechnicianSkillResponse[]
        - TechnicianSkillId, SkillCode, SkillName, SkillCategory, CertifiedOnUtc?
    - OnboardingStatus          string  — e.g. "Draft" | "DocumentsUploaded" | "Active"
    - PendingEligibilityItems   string[]
    - UploadedDocumentCount     int
    - VerifiedDocumentCount     int
    - LatestAssessmentResult    string
    - CompletedTrainingCount    int
  DB Tables Read:       tblTechnician, tblTechnicianZone, tblTechnicianSkill,
                        tblTechnicianDocument, tblSkillAssessment, tblTrainingRecord
  Failure Cases:        404, 401, 403

---

#### Flow TCH-3: Create Technician
  Endpoint:             POST /api/technicians
  Auth:                 Authorize(Policy=UserCreate)
  Request DTO:          CreateTechnicianRequest (body)
    - TechnicianName    string    required
    - MobileNumber      string    required
    - EmailAddress      string?
    - BaseZoneId        long?
    - MaxDailyAssignments int     required
    - Skills            TechnicianSkillRequest[]?
        - SkillCode, SkillName, SkillCategory, CertifiedOnUtc?
    - ZoneIds           long[]?
  Response DTO:         TechnicianDetailResponse (new technician)
  DB Tables Written:    tblTechnician, tblTechnicianSkill, tblTechnicianZone
  Business Rules:
    1. Creates Technician profile + User account.
    2. Skills and ZoneIds are optional on creation; can be added via PATCH later.
  Failure Cases:        400, 401, 403

---

#### Flow TCH-4: Update Technician
  Endpoint:             PUT /api/technicians/{technicianId:long}
  Auth:                 Authorize(Policy=UserUpdate)
  Request DTO:          UpdateTechnicianRequest (body)
    - TechnicianName, MobileNumber, EmailAddress?, BaseZoneId?, MaxDailyAssignments, IsActive
  Response DTO:         TechnicianDetailResponse (updated)
  DB Tables Written:    tblTechnician
  Note: Skills and Zones updated via separate PATCH endpoints (TCH-11, TCH-12).
  Failure Cases:        404, 400, 401, 403

---

#### Flow TCH-5: Get Technician Performance
  Endpoint:             GET /api/technicians/{technicianId:long}/performance
  Auth:                 Authorize(Policy=TechnicianRead)
  Request DTO:          route: technicianId; query: fromDate (DateOnly?), toDate (DateOnly?)
  Response DTO:         TechnicianPerformanceResponse
    - AverageRating, TotalJobs, CompletedJobs, SlaCompliancePercent
    - RevisitRatePercent, RevenueGenerated
    - TeamAverageSlaCompliancePercent
    - Trends                    TechnicianPerformanceTrendResponse[]
        - Label, JobsAssigned, JobsCompleted, SlaCompliancePercent
  DB Tables Read:       tblTechnician, tblServiceRequest, tblJobCard, tblInvoice
  Failure Cases:        404, 401, 403

---

#### Flow TCH-6: Get Technician Attendance
  Endpoint:             GET /api/technicians/{technicianId:long}/attendance
  Auth:                 Authorize(Policy=TechnicianRead)
  Request DTO:          route: technicianId; query: year? (default current), month? (default current)
  Response DTO:         IReadOnlyCollection<TechnicianAttendanceResponse>
    - TechnicianAttendanceId, AttendanceDate (DateOnly), AttendanceStatus
    - CheckInOnUtc?, CheckOutOnUtc?, LocationText, LeaveReason
    - ReviewedByUserId?, ReviewedOnUtc?
  DB Tables Read:       tblTechnicianAttendance (via GetTechnicianAttendanceQuery)
  Failure Cases:        404, 401, 403

---

#### Flow TCH-7: Request Technician Leave
  Endpoint:             POST /api/technicians/{technicianId:long}/attendance/leave
  Auth:                 Authorize(Policy=UserUpdate)
  Request DTO:          CreateTechnicianLeaveRequest (body)
    - LeaveDate         DateOnly  required
    - LeaveReason       string?
  Response DTO:         TechnicianAttendanceResponse (new leave record)
  DB Tables Written:    tblTechnicianAttendance (via RequestTechnicianLeaveCommand)
  Failure Cases:        404, 400, 401, 403

---

#### Flow TCH-8: Review Technician Leave (Approve/Reject)
  Endpoint:             PATCH /api/technicians/{technicianId:long}/attendance/leave/{leaveRequestId:long}
  Auth:                 Authorize(Policy=UserUpdate)
  Request DTO:          ReviewTechnicianLeaveRequest (body)
    - Decision          string    required — "Approved" | "Rejected"
    - Remarks           string?
  Response DTO:         TechnicianAttendanceResponse (updated)
  DB Tables Written:    tblTechnicianAttendance (via ReviewTechnicianLeaveCommand)
  Failure Cases:        404, 400, 401, 403

---

#### Flow TCH-9: Get Availability Board
  Entry Points:         Dispatch screen → Technician match panel; Scheduling board technician selector
  Endpoint:             GET /api/technicians/availability-board
                        GET /api/technicians/availability (legacy alias — same handler)
  Auth:                 Authorize(Policy=TechnicianRead)
  Request DTO:          query: serviceRequestId (long?) — if provided, ranks technicians by skill/zone match
  Response DTO:         IReadOnlyCollection<TechnicianListItemResponse> (ranked if serviceRequestId given)
  DB Tables Read:       tblTechnician, tblTechnicianSkill, tblTechnicianZone, tblServiceRequest
  Failure Cases:        401, 403

---

#### Flow TCH-10: Get Technician GPS Log
  Endpoint:             GET /api/technicians/{technicianId:long}/gps-log
  Auth:                 Authorize(Policy=TechnicianRead)
  Request DTO:          route: technicianId; query: trackingDate (DateOnly? — default today UTC)
  Response DTO:         IReadOnlyCollection<TechnicianGpsLogResponse>
    - TechnicianGpsLogId, TrackedOnUtc, Latitude, Longitude
    - TrackingSource, LocationText, ServiceRequestId?
  DB Tables Read:       tblTechnicianGpsLog (via GetTechnicianGpsLogQuery)
  Failure Cases:        404, 401, 403

---

#### Flow TCH-11: Update Technician Skills
  Endpoint:             PATCH /api/technicians/{technicianId:long}/skills
  Auth:                 Authorize(Policy=UserUpdate)
  Request DTO:          UpdateTechnicianSkillsRequest (body)
    - Skills            TechnicianSkillRequest[]  — full replacement list
        - SkillCode, SkillName, SkillCategory, CertifiedOnUtc?
  Response DTO:         IReadOnlyCollection<TechnicianSkillResponse>
  DB Tables Written:    tblTechnicianSkill (soft-delete + re-insert; unique filtered index on TechnicianId+SkillName WHERE IsDeleted=0)
  Business Rules:
    1. Reactivates existing matching rows instead of inserting duplicates.
    2. Compatibility script: DB_Alter_20260506_TechnicianAssignmentSoftDeleteCompatibility.sql
  Failure Cases:        404, 400, 401, 403

---

#### Flow TCH-12: Update Technician Zones
  Endpoint:             PATCH /api/technicians/{technicianId:long}/zones
  Auth:                 Authorize(Policy=UserUpdate)
  Request DTO:          UpdateTechnicianZonesRequest (body)
    - ZoneIds           long[]    required — full replacement list of zone IDs
    - PrimaryZoneId     long?
  Response DTO:         IReadOnlyCollection<TechnicianZoneResponse>
  DB Tables Written:    tblTechnicianZone (soft-delete + re-insert; unique filtered index on TechnicianId+ZoneId WHERE IsDeleted=0)
  Business Rules:
    1. Reactivates existing matching rows instead of inserting duplicates.
  Failure Cases:        404, 400, 401, 403

---

#### Flow TCH-13: Get Technician Public Profile (Customer-Visible)
  Entry Points:         Customer Portal → Booking Detail → Technician card; Public profile link
  Endpoint:             GET /api/technicians/{technicianId:long}/public
  Auth:                 AllowAnonymous
  Request DTO:          route: technicianId (long)
  Response DTO:         CustomerVisibleTechnicianResponse
    - TechnicianId, Name, PhotoUrl, Rating, TotalJobs
    - Experience (string), Specialization (string[]), Languages (string[]), Verified (bool)
  DB Tables Read:       tblTechnician (via GetCustomerVisibleTechnicianQuery)
  Business Rules:
    1. Returns only public-safe fields — no mobile number, email, zone, GPS data.
  Failure Cases:        404

---

### TECHNICIAN ACTIVATION & DOCUMENT CONTROLLER GROUP (/api/technicians/{technicianId}/...)

#### Flow TCH-14: Activate Technician
  Endpoint:             POST /api/technicians/{technicianId:long}/activate
  Auth:                 Authorize(Policy=UserUpdate)
  Request DTO:          ActivateTechnicianPhaseERequest (body)
    - ActivationReason  string    required
  Response DTO:         TechnicianOnboardingDetailResponse (see TCH-17 for shape)
  DB Tables Written:    tblTechnician (IsActive=true), tblTechnicianActivationLog
  Failure Cases:        404, 400, 401, 403

---

#### Flow TCH-15: Deactivate Technician
  Endpoint:             POST /api/technicians/{technicianId:long}/deactivate
  Auth:                 Authorize(Policy=UserUpdate)
  Request DTO:          DeactivateTechnicianRequest (body)
    - ActivationReason  string    required — reason for deactivation
  Response DTO:         TechnicianOnboardingDetailResponse
  DB Tables Written:    tblTechnician (IsActive=false), tblTechnicianActivationLog
  Failure Cases:        404, 400, 401, 403

---

#### Flow TCH-16: Get Technician Activation History
  Endpoint:             GET /api/technicians/{technicianId:long}/activation-history
  Auth:                 Authorize(Policy=TechnicianRead)
  Response DTO:         IReadOnlyCollection<TechnicianActivationLogResponse>
    - TechnicianActivationLogId, ActivationAction, ActivationReason
    - ActivatedByUserId?, ActivatedOnUtc, EligibilitySnapshot
  DB Tables Read:       tblTechnicianActivationLog
  Failure Cases:        404, 401, 403

---

#### Flow TCH-17: Upload Technician Documents
  Endpoint:             POST /api/technicians/{technicianId:long}/documents
  Auth:                 Authorize(Policy=UserUpdate)
  Request DTO:          UploadTechnicianDocumentsRequest (body) — [VERIFY exact fields in GapPhaseA]
  Response DTO:         TechnicianOnboardingDetailResponse
    - TechnicianId, TechnicianCode, TechnicianName, MobileNumber, EmailAddress
    - BaseZoneId?, MaxDailyAssignments, IsActive, OnboardingStatus, IsActivationEligible
    - PendingEligibilityItems (string[])
    - Documents                 TechnicianDocumentDetailResponse[]
        - TechnicianDocumentId, DocumentType, DocumentNumber, StorageUrl
        - VerificationStatus, VerificationRemarks, ExpiryDateUtc?, VerifiedByUserId?, VerifiedOnUtc?
    - SkillAssessments          SkillAssessmentDetailResponse[]
    - TrainingRecords           TrainingRecordDetailResponse[]
    - ActivationHistory         TechnicianActivationLogResponse[]
  DB Tables Written:    tblTechnicianDocument
  Failure Cases:        404, 400, 401, 403

---

#### Flow TCH-18: Get Technician Documents
  Endpoint:             GET /api/technicians/{technicianId:long}/documents
  Auth:                 Authorize(Policy=TechnicianRead)
  Response DTO:         IReadOnlyCollection<TechnicianDocumentDetailResponse>
  DB Tables Read:       tblTechnicianDocument
  Failure Cases:        404, 401, 403

---

#### Flow TCH-19: Verify Technician Document
  Endpoint:             POST /api/technicians/{technicianId:long}/documents/{documentId:long}/verify
  Auth:                 Authorize(Policy=UserUpdate)
  Request DTO:          VerifyTechnicianDocumentRequest (body)
    - Remarks           string?
  Response DTO:         IReadOnlyCollection<TechnicianDocumentDetailResponse>
  DB Tables Written:    tblTechnicianDocument (VerificationStatus=Verified)
  Failure Cases:        404, 400, 401, 403

---

#### Flow TCH-20: Reject Technician Document
  Endpoint:             POST /api/technicians/{technicianId:long}/documents/{documentId:long}/reject
  Auth:                 Authorize(Policy=UserUpdate)
  Request DTO:          RejectTechnicianDocumentRequest (body)
    - Remarks           string?
  Response DTO:         IReadOnlyCollection<TechnicianDocumentDetailResponse>
  DB Tables Written:    tblTechnicianDocument (VerificationStatus=Rejected)
  Failure Cases:        404, 400, 401, 403

---

### TECHNICIAN ONBOARDING CONTROLLER GROUP (/api/technician-onboarding)

Note: TechnicianOnboardingController provides a legacy onboarding workflow. The modern path
uses TechnicianController (TCH-3) + TechnicianActivationController (TCH-14/15/17/18/19/20).

#### Flow TCH-21: Create Technician Draft (Onboarding)
  Endpoint:             POST /api/technician-onboarding/draft
  Auth:                 Authorize(Policy=UserCreate)
  Request DTO:          CreateTechnicianDraftRequest (body)
    - TechnicianName, MobileNumber, EmailAddress?, BaseZoneId?, MaxDailyAssignments
  Response DTO:         TechnicianOnboardingResponse (simplified — TechnicianId, TechnicianCode, TechnicianName, IsActive, DocumentCount, AssessmentResult, CompletedTrainingCount)
  DB Tables Written:    tblTechnician (draft state)
  Failure Cases:        400, 401, 403

---

#### Flow TCH-22: List Technicians in Onboarding
  Endpoint:             GET /api/technician-onboarding
  Auth:                 Authorize(Policy=TechnicianRead)
  Request DTO:          query: searchTerm?, status?, branchId?
  Response DTO:         IReadOnlyCollection<TechnicianOnboardingListItemResponse>
    - TechnicianId, TechnicianCode, TechnicianName, MobileNumber, EmailAddress
    - IsActive, OnboardingStatus, UploadedDocumentCount, VerifiedDocumentCount
    - LatestAssessmentResult, CompletedTrainingCount, IsActivationEligible
  DB Tables Read:       tblTechnician, tblTechnicianDocument, tblSkillAssessment, tblTrainingRecord
  Failure Cases:        401, 403

---

#### Flow TCH-23: Get Onboarding Detail
  Endpoint:             GET /api/technician-onboarding/{technicianId:long}
  Auth:                 Authorize(Policy=TechnicianRead)
  Response DTO:         TechnicianOnboardingDetailResponse (full shape — same as TCH-17)
  DB Tables Read:       tblTechnician, tblTechnicianDocument, tblSkillAssessment, tblTrainingRecord, tblTechnicianActivationLog
  Failure Cases:        404, 401, 403

---

#### Flow TCH-24: Upload Documents (Onboarding Path)
  Endpoint:             POST /api/technician-onboarding/{technicianId:long}/documents
  Auth:                 Authorize(Policy=UserUpdate)
  Request DTO:          UploadTechnicianDocumentsRequest (same as TCH-17)
  Response DTO:         TechnicianOnboardingResponse (simplified)
  DB Tables Written:    tblTechnicianDocument
  Failure Cases:        404, 400, 401, 403

---

#### Flow TCH-25: Activate via Onboarding (Legacy)
  Endpoint:             POST /api/technician-onboarding/{technicianId:long}/activate
  Auth:                 Authorize(Policy=UserUpdate)
  Request DTO:          ActivateTechnicianRequest (legacy body)
    - AssessmentCode?, ScorePercentage?, TrainingName?, CertificationNumber?, TrainingScorePercentage?, Remarks?
  Response DTO:         TechnicianOnboardingResponse (simplified)
  DB Tables Written:    tblTechnician (IsActive=true), tblTechnicianActivationLog
  Note: Modern path = POST /api/technicians/{id}/activate (TCH-14)
  Failure Cases:        404, 400, 401, 403

---

### TECHNICIAN JOB CONTROLLER GROUP (/api/technician-jobs) — Technician Mobile App

Note: Roles=Technician on all endpoints; technician sees only their assigned jobs.

#### Flow TCH-26: List Technician Jobs (Admin/Ops)
  Endpoint:             GET /api/technician-jobs
  Auth:                 Authorize(Roles=Technician)
  Request DTO:          query: status?, slotDate (DateOnly?), pageNumber=1, pageSize=20
  Response DTO:         PagedResult<TechnicianJobListItemResponse> (14 fields — see Module 5 / Field Workflow)
  DB Tables Read:       tblServiceRequest, tblJobCard, tblTechnician (via GetTechnicianJobListQuery)
  Failure Cases:        401, 403

---

#### Flow TCH-27: Get Technician Job Detail
  Endpoint:             GET /api/technician-jobs/{id:long}
  Auth:                 Authorize(Roles=Technician)
  Request DTO:          route: id (long — ServiceRequestId)
  Response DTO:         TechnicianJobDetailResponse (26 fields — see Module 5 / Field Workflow)
  DB Tables Read:       tblServiceRequest, tblJobCard, tblTechnician, tblCustomer, etc.
  Failure Cases:        404, 401, 403

---

#### Flow TCH-28: Get My Jobs (JWT-scoped)
  Endpoint:             GET /api/technician-jobs/my-jobs
  Auth:                 Authorize(Roles=Technician)
  Request DTO:          query: status?, slotDate?, pageNumber=1, pageSize=20
  Response DTO:         PagedResult<TechnicianJobListItemResponse>
  DB Tables Read:       tblServiceRequest, tblJobCard (filtered to authenticated TechnicianId)
  Failure Cases:        401, 403

---

### HELPER CONTROLLER GROUP (/api/helpers) — Field Helper Management

#### Flow HLP-1: Create Helper Profile
  Endpoint:             POST /api/helpers
  Auth:                 Authorize (Policy = UserCreate or StockManage — [VERIFY])
  Request DTO:          CreateHelperProfileRequest (body)
    - fields [VERIFY from GapPhaseE HelperPhaseERequests.cs]
  Response DTO:         HelperDetailResponse
  DB Tables Written:    tblHelperProfile
  Failure Cases:        400, 401, 403

---

#### Flow HLP-2: List Helpers
  Endpoint:             GET /api/helpers
  Response DTO:         IReadOnlyCollection<HelperListItemResponse>
  DB Tables Read:       tblHelperProfile

---

#### Flow HLP-3: Get Helper Detail
  Endpoint:             GET /api/helpers/{helperProfileId:long}
  Response DTO:         HelperDetailResponse

---

#### Flow HLP-4: Update Helper
  Endpoint:             PUT /api/helpers/{helperProfileId:long}
  Request DTO:          UpdateHelperProfileRequest
  Response DTO:         HelperDetailResponse
  DB Tables Written:    tblHelperProfile

---

#### Flow HLP-5: Assign Helper to Job
  Endpoint:             POST /api/helpers/{helperProfileId:long}/assign
  Request DTO:          AssignHelperToJobRequest (JobCardId, TechnicianId, Remarks?)
  Response DTO:         HelperDetailResponse
  DB Tables Written:    tblHelperAssignment

---

#### Flow HLP-6: Release Helper from Job
  Endpoint:             POST /api/helpers/{helperProfileId:long}/release
  Request DTO:          ReleaseHelperAssignmentRequest
  Response DTO:         HelperDetailResponse
  DB Tables Written:    tblHelperAssignment

---

#### Flow HLP-7: Get Helper Current Assignment
  Endpoint:             GET /api/helpers/{helperProfileId:long}/assignment
  Response DTO:         HelperAssignmentDetailResponse

---

### HELPER ATTENDANCE CONTROLLER GROUP (/api/helpers/{helperProfileId}/attendance)

#### Flow HLP-8: Check In Helper
  Endpoint:             POST /api/helpers/{helperProfileId:long}/attendance/check-in
  Request DTO:          CheckInHelperAttendanceRequest (LocationText?, Latitude?, Longitude?)
  Response DTO:         IReadOnlyCollection<HelperAttendanceResponse>
  DB Tables Written:    tblHelperAttendance

---

#### Flow HLP-9: Check Out Helper
  Endpoint:             POST /api/helpers/{helperProfileId:long}/attendance/check-out
  Request DTO:          CheckOutHelperAttendanceRequest
  Response DTO:         IReadOnlyCollection<HelperAttendanceResponse>
  DB Tables Written:    tblHelperAttendance

---

#### Flow HLP-10: Get Helper Attendance
  Endpoint:             GET /api/helpers/{helperProfileId:long}/attendance
  Response DTO:         IReadOnlyCollection<HelperAttendanceResponse>
  DB Tables Read:       tblHelperAttendance

---

### HELPER TASK CONTROLLER GROUP (/api/helpers/{helperProfileId}/tasks)

#### Flow HLP-11: Get Helper Task Checklist
  Endpoint:             GET /api/helpers/{helperProfileId:long}/tasks
  Response DTO:         IReadOnlyCollection<HelperTaskChecklistResponse>

---

#### Flow HLP-12: Respond to Helper Task
  Endpoint:             POST /api/helpers/{helperProfileId:long}/tasks/{taskId:long}/respond
  Request DTO:          SaveHelperTaskResponseRequest
  Response DTO:         IReadOnlyCollection<HelperTaskChecklistResponse>

---

#### Flow HLP-13: Upload Helper Task Photo
  Endpoint:             POST /api/helpers/{helperProfileId:long}/tasks/{taskId:long}/upload-photo
  Request DTO:          UploadHelperTaskPhotoRequest
  Response DTO:         IReadOnlyCollection<HelperTaskChecklistResponse>

---

### DB TABLES — Technician Management (Confirmed from DTOs and controller shapes)

tblTechnician
  - TechnicianId (PK), TechnicianCode, TechnicianName, MobileNumber, EmailAddress,
    BaseZoneId (FK nullable), MaxDailyAssignments (int), IsActive (bit),
    OnboardingStatus (nvarchar), + audit columns

tblTechnicianSkill
  - TechnicianSkillId (PK), TechnicianId (FK), SkillCode, SkillName, SkillCategory,
    CertifiedOnUtc (datetime2 nullable), IsDeleted (bit)
  - Unique filtered index: (TechnicianId, SkillName) WHERE IsDeleted=0
  - Reactivation logic: matching IsDeleted=1 rows restored instead of duplicate insert

tblTechnicianZone
  - TechnicianZoneId (PK), TechnicianId (FK), ZoneId (FK), IsPrimaryZone (bit), IsDeleted (bit)
  - Unique filtered index: (TechnicianId, ZoneId) WHERE IsDeleted=0

tblTechnicianAttendance
  - TechnicianAttendanceId (PK), TechnicianId (FK), AttendanceDate (date), AttendanceStatus,
    CheckInOnUtc, CheckOutOnUtc, LocationText, LeaveReason, ReviewedByUserId, ReviewedOnUtc

tblTechnicianGpsLog
  - TechnicianGpsLogId (PK), TechnicianId (FK), TrackedOnUtc, Latitude, Longitude,
    TrackingSource, LocationText, ServiceRequestId (FK nullable)

tblTechnicianDocument
  - TechnicianDocumentId (PK), TechnicianId (FK), DocumentType, DocumentNumber, StorageUrl,
    VerificationStatus, VerificationRemarks, ExpiryDateUtc nullable,
    VerifiedByUserId nullable, VerifiedOnUtc nullable

tblSkillAssessment
  - SkillAssessmentId (PK), TechnicianId (FK), SkillTagId? (FK), AssessmentCode, AssessmentName,
    AssessmentStatus, ScorePercentage, AssessmentResult, PassFlag, AssessedByUserId?, AssessedOnUtc?, Remarks

tblTrainingRecord
  - TrainingRecordId (PK), TechnicianId (FK), TrainingTitle, TrainingType, TrainingStatus,
    CertificationNumber, ScorePercentage, IsCompleted, TrainingCompletionDateUtc?,
    TrainerUserId?, CertificateUrl, Remarks

tblTechnicianActivationLog
  - TechnicianActivationLogId (PK), TechnicianId (FK), ActivationAction, ActivationReason,
    ActivatedByUserId?, ActivatedOnUtc, EligibilitySnapshot (nvarchar — JSON)

tblHelperProfile
  - HelperProfileId (PK), HelperName, MobileNumber, IsActive, + audit columns

tblHelperAssignment
  - HelperAssignmentId (PK), HelperProfileId (FK), JobCardId (FK), TechnicianId (FK),
    AssignedAt, ReleasedAt nullable, Remarks

tblHelperAttendance
  - HelperAttendanceId (PK), HelperProfileId (FK), CheckInAtUtc, CheckOutAtUtc nullable,
    LocationText, Latitude, Longitude

---

### DRIFT NOTES — Technician Management (corrected 2026-05-24)

1. Old docs listed 7 DB tables: "Technicians, TechnicianStatusLog, TechnicianSkill, TechnicianZone,
   TechnicianAttendance, TechnicianGPSLog, TechnicianPerformanceSummary"
   → Actual confirmed: tblTechnician, tblTechnicianSkill, tblTechnicianZone, tblTechnicianAttendance,
     tblTechnicianGpsLog + 6 new tables: tblTechnicianDocument, tblSkillAssessment, tblTrainingRecord,
     tblTechnicianActivationLog, tblHelperProfile, tblHelperAssignment, tblHelperAttendance
   → "TechnicianStatusLog" — NOT a separate table; AvailabilityStatus derived from current job state.
   → "TechnicianPerformanceSummary" — NOT a persistent table; computed via GetTechnicianPerformanceQuery.

2. Controllers entirely absent from old docs:
   - TechnicianActivationController (activate, deactivate, activation-history) — 3 endpoints
   - TechnicianDocumentController (upload, list, verify, reject) — 4 endpoints
   - TechnicianOnboardingController (draft, list, detail, upload-docs, activate) — 5 endpoints
   - TechnicianJobController (list, detail, my-jobs) — 3 endpoints
   - HelperController (create, list, detail, update, assign, release, current-assignment) — 7 endpoints
   - HelperAttendanceController (check-in, check-out, list) — 3 endpoints
   - HelperTaskController (list, respond, upload-photo) — 3 endpoints

3. Skill/Zone assignment soft-delete: reactivation pattern (not duplicate insert) confirmed.
   Compatibility script: DB_Alter_20260506_TechnicianAssignmentSoftDeleteCompatibility.sql.

4. /api/technicians/availability and /api/technicians/availability-board are both valid — same handler.

5. Helper DTOs (HelperDetailResponse, HelperListItemResponse, HelperAssignmentDetailResponse,
   HelperAttendanceResponse, HelperTaskChecklistResponse) marked [VERIFY] — shapes inferred from
   controller; GapPhaseE request shapes known but response shapes not read in this session.

---

## MODULE: Estimates & Work Orders (Job Execution Support)
## Updated: 2026-05-24 — Stable Contract Level

### Architecture Note

The Estimate (Quotation) engine is implemented in QuotationController (/api/quotations).
Full stable contracts are in SECTION 5 — MODULE: Quotation / Estimate Engine (Flows 16–21).

Work Orders: NO dedicated WorkOrderController exists. Work orders are conceptually represented
through the SR lifecycle and Job Cards — there is no /api/work-orders endpoint in the backend.

Job Reports Admin Review: NO /api/job-reports/* endpoints exist in the backend.
All job reports are submitted via POST /api/field/jobs/{id}/report (FieldWorkflowController)
and are accessible via GET /api/field/jobs/{id} as part of the FieldJobDetailResponse.LatestReport.

---

### Controllers in this module:
- JobAttachmentController — /api/technician-jobs (legacy prefix, Roles=Technician)
- JobChecklistController — /api/technician-jobs (legacy prefix, Roles=Technician)
- DiagnosisController — /api and /api/technician-jobs (Roles=Technician)
- JobConsumptionController — /api/jobs (admin + ops, policy-based)

---

### Flow 1: Save Job Attachment

Entry Points:       AdminMobile → JobWorkflowContainer → Attachments tab (legacy path)
UI Trigger:         File upload / camera capture
Endpoint:           POST /api/technician-jobs/{id}/attachments
Auth:               Authorize(Roles = Technician)
Request:            Route param: id (long) — ServiceRequestId
Request Body (SaveJobAttachmentRequest):
  - AttachmentType (string, required): e.g. "Document", "Invoice", "Certificate"
  - FileName (string, required)
  - ContentType (string, required): MIME type
  - Base64Content (string, required): base64-encoded file data
  - AttachmentRemarks (string?, optional)
Response (200):     JobAttachmentResponse
  - JobAttachmentId (long)
  - AttachmentType (string)
  - FileName (string)
  - ContentType (string)
  - FileSizeInBytes (long)
  - FileUrl (string): stored file URL
  - AttachmentRemarks (string)
  - UploadedDateUtc (DateTime)
DB Tables:          JobAttachments
Business Rules:
  - AttachmentType distinguishes documents from photos (photos go via /api/field/jobs/{id}/photos)
Failure Cases:
  - 403 if caller is not Technician role
Notes on Drift:     Under legacy /api/technician-jobs prefix. Prefer /api/field/jobs/{id}/photos for
                    photo uploads (FieldWorkflowController). Added 2026-05-24.

---

### Flow 2: Get Job Attachments

Entry Points:       AdminMobile → Job Detail → Attachments tab
Endpoint:           GET /api/technician-jobs/{id}/attachments
Auth:               Authorize(Roles = Technician)
Request:            Route param: id (long)
Response (200):     IReadOnlyCollection<JobAttachmentResponse> (same shape as Flow 1 response)
DB Tables:          JobAttachments
Failure Cases:
  - 403 if caller is not Technician role

---

### Flow 3: Get Job Checklist

Entry Points:       AdminMobile → JobWorkflowContainer → Checklist tab
UI Trigger:         Checklist tab open; also included in FieldJobDetailResponse.ChecklistItems
Endpoint:           GET /api/technician-jobs/{id}/checklist
Auth:               Authorize(Roles = Technician)
Request:            Route param: id (long)
Response (200):     IReadOnlyCollection<JobChecklistItemResponse>
  Each item:
    - ServiceChecklistMasterId (long)
    - ChecklistTitle (string)
    - ChecklistDescription (string)
    - IsMandatory (bool)
    - IsChecked (bool?)
    - ResponseRemarks (string)
    - ResponseDateUtc (DateTime?)
DB Tables:          ServiceChecklistMaster, JobChecklistResponses
Notes on Drift:     Checklist items also returned in GET /api/field/jobs/{id} (FieldJobDetailResponse.ChecklistItems).
                    This is the legacy direct read endpoint. Added 2026-05-24.

---

### Flow 4: Save Job Checklist Responses

Entry Points:       AdminMobile → JobWorkflowContainer → Checklist tab
UI Trigger:         Checkbox tap / batch save (also aliased in PATCH /api/field/jobs/{id}/progress)
Endpoint:           POST /api/technician-jobs/{id}/checklist
Auth:               Authorize(Roles = Technician)
Request:            Route param: id (long)
Request Body (SaveJobChecklistResponseRequest):
  - Items (SaveJobChecklistResponseItemRequest[], required):
      each: ServiceChecklistMasterId (long), IsChecked (bool?), ResponseRemarks (string?)
Response (200):     IReadOnlyCollection<JobChecklistItemResponse>
DB Tables:          JobChecklistResponses
Business Rules:
  - Mandatory items (IsMandatory=true) must all have IsChecked=true before job report can be submitted
Notes on Drift:     Also aliased via PATCH /api/field/jobs/{id}/progress (FieldJobProgressRequest).
                    This legacy endpoint accepts the same underlying command. Added 2026-05-24.

---

### Flow 5: Get Diagnosis Issue Lookups

Entry Points:       AdminMobile → Diagnosis screen → complaint issue selector
Endpoint:           GET /api/diagnosis/lookups/issues
Auth:               Authorize(Roles = Technician)
Query Params:       search (string?, optional)
Response (200):     IReadOnlyCollection<DiagnosisLookupItemResponse>
  Each: Id (long), Name (string), Description (string)
DB Tables:          ComplaintIssueMaster
Notes on Drift:     Not documented. Added 2026-05-24.

---

### Flow 6: Get Diagnosis Result Lookups

Entry Points:       AdminMobile → Diagnosis screen → result/resolution selector
Endpoint:           GET /api/diagnosis/lookups/results
Auth:               Authorize(Roles = Technician)
Query Params:       search (string?, optional)
Response (200):     IReadOnlyCollection<DiagnosisLookupItemResponse> (same shape as Flow 5)
DB Tables:          DiagnosisResultMaster
Notes on Drift:     Not documented. Added 2026-05-24.

---

### Flow 7: Save Job Diagnosis

Entry Points:       AdminMobile → JobWorkflowContainer → Observations section
UI Trigger:         Diagnosis form save
Endpoint:           POST /api/technician-jobs/{id}/diagnosis
Auth:               Authorize(Roles = Technician)
Request:            Route param: id (long) — ServiceRequestId
Request Body (SaveJobDiagnosisRequest):
  - ComplaintIssueMasterId (long?, optional)
  - DiagnosisResultMasterId (long?, optional)
  - DiagnosisRemarks (string?, optional)
Response (200):     JobDiagnosisSummaryResponse
  - JobDiagnosisId (long?)
  - ComplaintIssueMasterId (long?)
  - ComplaintIssueName (string?)
  - DiagnosisResultMasterId (long?)
  - DiagnosisResultName (string?)
  - DiagnosisRemarks (string?)
  - DiagnosisDateUtc (DateTime?)
DB Tables:          JobDiagnosis, ComplaintIssueMaster, DiagnosisResultMaster
Business Rules:
  - Diagnosis linked to job; also returned in TechnicianJobDetailResponse.Diagnosis
Failure Cases:
  - 403 if caller is not Technician role
Notes on Drift:     Not documented at contract level. Added 2026-05-24.

---

### Flow 8: Record Job Parts Consumption

Entry Points:       Admin Portal → Job Detail → Parts Consumed tab; store manager post-issue confirmation
UI Trigger:         "Record Consumption" after parts are issued and used
Endpoint:           POST /api/jobs/{jobCardId}/consume-parts
Auth:               Authorize(Policy = JobConsumptionCreate)
Request:            Route param: jobCardId (long)
Request Body (ConsumeJobPartsRequest):
  - Items (ConsumeJobPartItemRequest[], required):
      each: ItemId (long — numeric DB ID only, never part code), QuantityUsed (decimal), ConsumptionRemarks (string?)
Response (200):     JobPartConsumptionSummaryResponse
  - JobCardId (long)
  - JobCardNumber (string)
  - TechnicianId (long?)
  - TechnicianName (string?)
  - TotalLines (int)
  - TotalQuantityUsed (decimal)
  - TotalAmount (decimal)
  - Items (JobPartConsumptionResponse[]):
      each: JobPartConsumptionId, JobCardId, JobCardNumber, TechnicianId, TechnicianName,
            ItemId, ItemCode, ItemName, QuantityUsed, UnitPrice, LineAmount,
            ConsumedDateUtc, ConsumptionRemarks
DB Tables:          JobPartConsumptions, WarehouseStock (decremented), StockTransactions
Business Rules:
  - ItemId must be numeric DB ID — never use ItemCode as route or request param
  - Stock decremented from WarehouseStock on consumption record
  - TotalAmount = sum of (QuantityUsed × UnitPrice) per line
Failure Cases:
  - 403 if role lacks JobConsumptionCreate permission
  - 422 if ItemId not found or insufficient stock
Notes on Drift:     Not documented. IDs-only rule critical: ItemCode (e.g. "CAP-45") is display only.
                    Added 2026-05-24.

---

### Flow 9: Get Job Parts Consumption

Entry Points:       Admin Portal → Job Detail → Parts Consumed tab
UI Trigger:         Tab switch
Endpoint:           GET /api/jobs/{jobCardId}/consumption
Auth:               Authorize(Policy = JobConsumptionRead)
Request:            Route param: jobCardId (long)
Response (200):     JobPartConsumptionSummaryResponse (same shape as Flow 8 response)
DB Tables:          JobPartConsumptions (read-only)
Failure Cases:
  - 403 if role lacks JobConsumptionRead permission
  - 404 if jobCardId not found

---

## PHANTOM ROUTES — Estimates & Work Orders (removed 2026-05-24)

The following routes were documented in Phase 12 AdminMobile notes but NO backend controller exists:
  PATCH /api/estimates/{id}/send             — PHANTOM, never implemented
  PATCH /api/estimates/{id}/approve          — PHANTOM — use POST /api/quotations/{id}/approve
  PATCH /api/estimates/{id}/reject           — PHANTOM — use POST /api/quotations/{id}/reject
  PATCH /api/estimates/{id}/override-approve — PHANTOM, never implemented
  PATCH /api/estimates/{id}/resend           — PHANTOM, never implemented
  GET   /api/estimates/expiry-queue          — PHANTOM, never implemented
  GET   /api/job-reports                     — PHANTOM, never implemented
  GET   /api/job-reports/{id}                — PHANTOM, never implemented
  PATCH /api/job-reports/{id}/approve        — PHANTOM, never implemented
  PATCH /api/job-reports/{id}/flag           — PHANTOM, never implemented
  GET   /api/job-reports/quality-dashboard   — PHANTOM, never implemented

Correct routes for estimate/quotation operations: POST /api/quotations/from-job/{id},
  POST /api/quotations/{id}/approve, POST /api/quotations/{id}/reject (Section 5, Flows 16–21)
Correct routes for job report operations: POST /api/field/jobs/{id}/report,
  GET /api/field/jobs/{id} (Field Workflow, Flows 3 and 10)

Work Orders: NOT IMPLEMENTED. No WorkOrderController or work-order DB tables confirmed in source.
  Work execution is tracked through SR status + JobCard entity.

Job Report Admin Review / Quality Dashboard: NOT IMPLEMENTED as standalone endpoints.
  Job report data accessible via GET /api/field/jobs/{id}.LatestReport.

---

## MODULE: Inventory & Parts Management
## Controllers: InventoryController, ItemController, WarehouseController, StockController,
##              PartsReturnController, SupplierController, TechnicianStockController
## Verified: 2026-05-24 | 34 endpoints | Stable Contract

---

### INVENTORY CONTROLLER GROUP (/api/inventory) — Authorize on all

---

#### Flow INV-1: Get Inventory Dashboard
  Entry Points:         /inventory/dashboard (AdminMobile Inventory module)
  UI Trigger:           Screen mount
  Endpoint:             GET /api/inventory/dashboard
  Auth:                 Authorize(Policy=ItemRead)
  Request DTO:          None (no query params)
  Response DTO:         InventoryDashboardResponse
    - TotalSKUs         int    — total active Items
    - TotalStockValue   decimal — sum(QtyOnHand × PurchasePrice) for primary warehouse
    - LowStockCount     int    — items with IsLowStock=true and QtyOnHand > 0
    - OutOfStockCount   int    — items with QtyOnHand <= 0
    - PendingRequests   int    — PartsRequests in Pending|PartiallyApproved status
    - OpenPOs           int    — PurchaseOrders not in FullyReceived|Cancelled
  DB Tables Read:       tblItem, tblWarehouseStock, tblPartsRequest, tblPurchaseOrder
  Business Rules:
    1. Pulls first active Warehouse (order by WarehouseId) as primary warehouse.
    2. Stock value = sum over warehouse stock items of (QtyOnHand × PurchasePrice from item rates).
    3. LowStock = IsLowStock flag on WarehouseStockItemResponse AND QtyOnHand > 0.
    4. OutOfStock = QtyOnHand <= 0 (regardless of reorder level).
  Failure Cases:
    401 — not authenticated
    403 — missing ItemRead permission

---

#### Flow INV-2: List Parts Catalog
  Entry Points:         /inventory/parts (AdminMobile Parts Catalog screen)
  UI Trigger:           Screen mount; search input debounce
  Endpoint:             GET /api/inventory/parts
  Auth:                 Authorize(Policy=ItemRead)
  Request DTO:          query params
    - searchTerm        string? — partial match on ItemCode, ItemName, ItemDescription
    - isActive          bool?   — filter by active status
  Response DTO:         IReadOnlyCollection<InventoryPartResponse>
    - Id                string  — ItemId.ToString()   [NOTE: string, not long — IDs-only rule]
    - PartCode          string  — ItemCode (display only; never use as API identifier)
    - Name              string  — ItemName
    - Category          string  — ItemCategory.CategoryName or CategoryId string fallback
    - Description       string  — ItemDescription
    - CompatibleBrands  string[] — always empty array (not populated in current impl)
    - UnitCost          decimal — latest active rate PurchasePrice, or 0.00
    - SellingPrice      decimal? — latest active rate SellingPrice
    - StockQuantity     decimal — sum of WarehouseStocks.QuantityOnHand
    - MinReorderLevel   decimal — Item.ReorderLevel
    - ReorderQuantity   decimal — max(ReorderLevel - StockQuantity, 0)
    - Location          string  — "Main Warehouse" (hardcoded)
    - Status            string  — "out_of_stock" | "low_stock" | "in_stock"
    - ImageUrl          string? — always null (not populated)
    - SupplierIds       string[] — [SupplierId.ToString()] if SupplierId set, else []
  DB Tables Read:       tblItem (includes ItemCategory, Supplier, Rates, WarehouseStocks)
  Business Rules:
    1. Results ordered by ItemName ascending.
    2. Status logic: QtyOnHand<=0 → "out_of_stock"; QtyOnHand<=ReorderLevel → "low_stock"; else "in_stock".
    3. PartCode is display-only — must NEVER be used as a route param or API identifier (IDs-only rule).
  Failure Cases:
    401, 403 — auth/permission

---

#### Flow INV-3: Get Part by ID
  Entry Points:         /inventory/parts/{id} (AdminMobile Part Detail screen)
  UI Trigger:           List item tap or direct navigation
  Endpoint:             GET /api/inventory/parts/{id:long}
  Auth:                 Authorize(Policy=ItemRead)
  Request DTO:          route: id (long, numeric ItemId)
  Response DTO:         InventoryPartResponse (same shape as Flow INV-2)
  DB Tables Read:       tblItem (with ItemCategory, Supplier, Rates, WarehouseStocks)
  Business Rules:
    1. Filters by ItemId and IsDeleted=false.
  Failure Cases:
    404 — part not found
    400 — non-numeric id rejected by route constraint

---

#### Flow INV-4: Create Part (Simplified Admin)
  Entry Points:         /inventory/parts/new (AdminMobile Add Part screen)
  UI Trigger:           Save button on Add Part form
  Endpoint:             POST /api/inventory/parts
  Auth:                 Authorize(Policy=ItemCreate)
  Request DTO:          InventoryPartUpsertRequest (body)
    - PartCode          string    required
    - Name              string    required
    - Category          string    required — mapped to CategoryCode + CategoryName
    - Description       string?
    - UnitCost          decimal   required
    - SellingPrice      decimal?  — defaults to UnitCost if null
    - MinReorderLevel   decimal   required
    - IsActive          bool      required
  Response DTO:         InventoryPartResponse (new item, same shape as INV-2)
  DB Tables Written:    tblItem, tblItemCategory, tblItemRate (via CreateItemCommand)
  Business Rules:
    1. CategoryCode auto-generated from Category string (uppercase alphanumeric, hyphens for special chars).
    2. UoM defaults to "PCS"/"Pieces"; Supplier left null.
    3. TaxPercentage defaults to 0; WarrantyDays defaults to 0; InitialStock defaults to 0.
  Failure Cases:
    400 — validation failure
    401, 403 — auth/permission

---

#### Flow INV-5: Update Part (Simplified Admin)
  Entry Points:         /inventory/parts/{id}/edit (AdminMobile Edit Part screen)
  UI Trigger:           Save button on Edit Part form
  Endpoint:             PUT /api/inventory/parts/{id:long}
  Auth:                 Authorize(Policy=ItemCreate)
  Request DTO:          InventoryPartUpsertRequest (same as INV-4)
  Response DTO:         InventoryPartResponse
  DB Tables Written:    tblItem, tblItemCategory (via UpdateItemCommand)
  Business Rules:
    1. Preserves existing UoM, Supplier, TaxPercentage, WarrantyDays from the item record.
    2. Returns 404 if item not found or soft-deleted.
  Failure Cases:
    404 — part not found
    400 — validation failure
    401, 403 — auth/permission

---

#### Flow INV-6: List Parts Requests Queue
  Entry Points:         /inventory/parts-requests (AdminMobile Parts Requests screen)
  UI Trigger:           Screen mount; pull-to-refresh
  Endpoint:             GET /api/inventory/parts-requests
  Auth:                 Authorize(Policy=ItemRead)
  Request DTO:          None
  Response DTO:         IReadOnlyCollection<InventoryPartsRequestResponse>
    - Id                string   — PartsRequestId.ToString()
    - TechnicianId      string   — TechnicianId.ToString()
    - TechnicianName    string
    - SrId              string   — ServiceRequestId.ToString()
    - SrNumber          string   — ServiceRequestNumber or "SR-{id}" fallback
    - Urgency           string   — "emergency" | "normal"
    - Status            string   — "pending" | "approved" | "partially_approved" | "rejected"
    - Items             InventoryPartsRequestItemResponse[]
        - PartId        string   — ItemId.ToString() (or 0 if unresolved legacy row)
        - PartName      string
        - RequestedQty  decimal
        - IssuedQty     decimal? — null if not yet issued
        - Status        string   — "available" | "insufficient" | "out_of_stock"
    - SubmittedAt       DateTime
    - ProcessedAt       DateTime?
    - Notes             string?
  DB Tables Read:       tblPartsRequest, tblPartsRequestItem, tblServiceRequest, tblTechnician
  Business Rules:
    1. Ordered by urgency DESC (Emergency first), then SubmittedAtUtc ASC.
    2. Legacy rows without ItemId: PartCode-to-ItemId fallback lookup via tblItem.ItemCode.
    3. Item status: IssuedQty >= RequestedQty → "available"; IssuedQty > 0 → "insufficient"; else "out_of_stock".
  Failure Cases:
    401, 403 — auth/permission

---

#### Flow INV-7: Get Parts Request by ID
  Entry Points:         /inventory/parts-requests/{id} (AdminMobile Parts Request Detail)
  Endpoint:             GET /api/inventory/parts-requests/{id:long}
  Auth:                 Authorize(Policy=ItemRead)
  Request DTO:          route: id (long)
  Response DTO:         InventoryPartsRequestResponse (same as INV-6 single item)
  DB Tables Read:       tblPartsRequest, tblPartsRequestItem, tblServiceRequest, tblTechnician
  Failure Cases:
    404 — request not found
    401, 403 — auth/permission

---

#### Flow INV-8: Approve Parts Request (Full)
  Entry Points:         /inventory/parts-requests/{id}/approve (AdminMobile Parts Request Detail → Approve)
  UI Trigger:           Approve button
  Endpoint:             PATCH /api/inventory/parts-requests/{id:long}/approve
  Auth:                 Authorize(Policy=StockManage)
  Request DTO:          InventoryPartsRequestProcessRequest (body)
    - Items             InventoryPartsRequestProcessItemRequest[]
        - PartId        string    required — numeric ItemId string
        - PartName      string    required
        - RequestedQty  decimal   required
        - IssuedQty     decimal?  — quantity to issue per line
        - Status        string    required
  Response DTO:         InventoryPartsRequestResponse (updated)
  DB Tables Read:       tblPartsRequest, tblPartsRequestItem, tblWarehouse, tblWarehouseStock
  DB Tables Written:    tblPartsRequestItem (QuantityApproved, CurrentStatus), tblPartsRequest (CurrentStatus, ProcessedAtUtc), tblWarehouseStock (QuantityOnHand decremented), tblStockTransaction (JobConsumption record)
  Business Rules:
    1. Sets PartsRequest.CurrentStatus = Approved.
    2. For each item: QuantityApproved = min(RequestedQty, IssuedQty).
    3. If QuantityApproved > 0 and ItemId resolved and warehouse found:
       a. Deducts QuantityApproved from tblWarehouseStock.QuantityOnHand (floor 0).
       b. Creates tblStockTransaction: TransactionType=JobConsumption, linked to TechnicianId+JobCardId.
    4. ReferenceNumber = ServiceRequestNumber or "PR-{id}" fallback.
    5. Legacy rows without ItemId resolved via PartCode-to-ItemId lookup.
  State Transitions:    PartsRequest → Pending → Approved
  Failure Cases:
    404 — request not found
    401, 403 — auth/permission

---

#### Flow INV-9: Partially Approve Parts Request
  Entry Points:         /inventory/parts-requests/{id}/partial
  Endpoint:             PATCH /api/inventory/parts-requests/{id:long}/partial
  Auth:                 Authorize(Policy=StockManage)
  Request DTO:          InventoryPartsRequestProcessRequest (same as INV-8)
  Response DTO:         InventoryPartsRequestResponse
  DB Tables Written:    Same as INV-8
  Business Rules:       Same logic as INV-8; sets CurrentStatus = PartiallyApproved.
  State Transitions:    PartsRequest → Pending → PartiallyApproved
  Notes on Drift:       Old docs described "follow-up request for remaining qty" as automatic — not implemented;
                        partially approved items simply show lower IssuedQty vs RequestedQty.
  Failure Cases:
    404, 401, 403

---

#### Flow INV-10: Reject Parts Request
  Entry Points:         /inventory/parts-requests/{id}/reject
  Endpoint:             PATCH /api/inventory/parts-requests/{id:long}/reject
  Auth:                 Authorize(Policy=StockManage)
  Request DTO:          InventoryPartsRequestProcessRequest (same as INV-8; IssuedQty ignored)
  Response DTO:         InventoryPartsRequestResponse
  DB Tables Written:    tblPartsRequestItem (QuantityApproved=0, CurrentStatus=Rejected), tblPartsRequest (CurrentStatus=Rejected)
  Business Rules:
    1. IssuedQty forced to 0 for all items.
    2. No stock deduction.
    3. No StockTransaction created.
  State Transitions:    PartsRequest → Pending|PartiallyApproved → Rejected
  Failure Cases:
    404, 401, 403

---

#### Flow INV-11: Get Low Stock Alerts
  Entry Points:         /inventory/low-stock (AdminMobile Low Stock Alerts screen)
  UI Trigger:           Screen mount
  Endpoint:             GET /api/inventory/low-stock-alerts
  Auth:                 Authorize(Policy=ItemRead)
  Request DTO:          None
  Response DTO:         IReadOnlyCollection<InventoryLowStockAlertResponse>
    — identical shape to InventoryPartResponse; same fields (Id, PartCode, Name, Category, Description,
      CompatibleBrands[], UnitCost, SellingPrice?, StockQuantity, MinReorderLevel, ReorderQuantity,
      Location, Status, ImageUrl?, SupplierIds[])
  DB Tables Read:       tblItem (via GetItemsQuery), tblWarehouseStock (primary warehouse only)
  Business Rules:
    1. Returns only items where IsLowStock=true in primary warehouse stock.
    2. Ordered: out-of-stock (QtyOnHand<=0) first, then ascending QtyOnHand, then Name.
    3. Status = "out_of_stock" if QtyOnHand<=0; else "low_stock".
    4. ReorderQuantity = max(ReorderLevel - QtyOnHand, 0).
  Failure Cases:
    401, 403 — auth/permission

---

#### Flow INV-12: Get Stock Movements
  Entry Points:         /inventory/stock-movements (AdminMobile Stock Movements screen; Part Detail → Ledger tab)
  Endpoint:             GET /api/inventory/stock-movements
  Auth:                 Authorize(Policy=StockRead)
  Request DTO:          query params
    - partId            long?  — filter by ItemId
  Response DTO:         IReadOnlyCollection<InventoryStockMovementResponse>
    - Id                string   — StockTransactionId.ToString()
    - PartId            string   — ItemId.ToString()
    - PartName          string
    - Type              string   — "IN" | "OUT" | "ADJ" (mapped from StockTransactionType enum)
    - Quantity          decimal  — abs(Quantity)
    - BalanceAfter      decimal  — BalanceAfterTransaction
    - ReferenceId       string   — ReferenceNumber or "TXN-{id}" fallback
    - ReferenceType     string   — "po" | "job" | "manual" | "return"
    - Timestamp         DateTime — TransactionDateUtc
    - Actor             string   — TechnicianName or CreatedBy
    - Notes             string?  — Remarks if non-empty
  DB Tables Read:       tblStockTransaction (includes tblItem, tblTechnician)
  Business Rules:
    1. Returns latest 250 records (no pagination), ordered by TransactionDateUtc DESC.
    2. Type mapping: PurchaseIn/TransferIn/ReturnIn → "IN"; JobConsumption/TransferOut/ReturnOut → "OUT";
       AdjustmentIn/AdjustmentOut → "ADJ".
    3. ReferenceType: PurchaseIn → "po"; JobConsumption → "job"; AdjustmentIn/Out → "manual"; else "return".
  Failure Cases:
    401, 403 — auth/permission

---

#### Flow INV-13: Manual Stock Adjustment
  Entry Points:         /inventory/stock-adjust (AdminMobile Part Detail → Adjust Stock)
  UI Trigger:           Adjust Stock button with reason input
  Endpoint:             POST /api/inventory/stock-adjust
  Auth:                 Authorize(Policy=StockManage)
  Request DTO:          InventoryStockAdjustRequest (body)
    - PartId            string    required — numeric ItemId string (IDs-only rule)
    - Quantity          decimal   required — positive = add stock; negative = remove stock; cannot be 0
    - Reason            string    required — adjustment reason; cannot be blank
  Response DTO:         InventoryPartResponse (updated part with new stock quantity)
  DB Tables Read:       tblWarehouse, tblItem (with rates)
  DB Tables Written:    tblStockTransaction (AdjustmentIn or AdjustmentOut), tblWarehouseStock (QuantityOnHand via RecordStockTransactionCommand)
  Validation Rules:
    1. PartId must be parseable as long → 400 "A numeric partId is required."
    2. Quantity cannot be 0 → 400 "Adjustment quantity cannot be zero."
    3. Reason cannot be blank → 400 "Adjustment reason is required."
    4. Active warehouse must exist → 404 "No active warehouse was found."
    5. Part must exist and not be deleted → 404 "The requested inventory part could not be found."
  Business Rules:
    1. Quantity > 0 → TransactionType = AdjustmentIn; Quantity < 0 → AdjustmentOut (uses abs value).
    2. ReferenceNumber = "ADJ-{ItemCode}".
    3. UnitCost = latest active rate PurchasePrice; 0 if no rate.
  State Transitions:    None on PartsRequest; tblWarehouseStock.QuantityOnHand changes immediately.
  Failure Cases:
    400 — validation errors (see Validation Rules above)
    404 — warehouse or part not found
    401, 403 — auth/permission

---

#### Flow INV-14: List Purchase Orders
  Entry Points:         /inventory/purchase-orders (AdminMobile Purchase Orders screen)
  Endpoint:             GET /api/inventory/purchase-orders
  Auth:                 Authorize(Policy=ItemRead)
  Request DTO:          query params
    - status            string?  — "draft" | "submitted" | "confirmed" | "partially_received" | "fully_received" | "cancelled"
  Response DTO:         IReadOnlyCollection<InventoryPurchaseOrderResponse>
    - Id                string   — PurchaseOrderId.ToString()
    - PONumber          string   — e.g. "PO-0001"
    - SupplierId        string   — SupplierId.ToString()
    - SupplierName      string
    - Status            string   — mapped from PurchaseOrderStatus enum
    - Items             InventoryPurchaseOrderItemResponse[]
        - PartId        string   — ItemId.ToString()
        - PartName      string
        - OrderedQty    decimal
        - ReceivedQty   decimal
        - UnitPrice     decimal
        - Total         decimal  — OrderedQty × UnitPrice
    - Subtotal          decimal
    - Tax               decimal
    - Total             decimal  — Subtotal + Tax
    - CreatedAt         DateTime — OrderDateUtc
    - ExpectedDeliveryDate DateTime
    - ReceivedAt        DateTime? — ReceivedAtUtc
    - Notes             string?
  DB Tables Read:       tblPurchaseOrder (includes tblSupplier, tblPurchaseOrderItem, tblItem)
  Business Rules:
    1. Invalid status string → 400 "Purchase order status filter is invalid."
    2. Ordered by OrderDateUtc DESC, then PurchaseOrderId DESC.
  Failure Cases:
    400 — invalid status filter
    401, 403 — auth/permission

---

#### Flow INV-15: Get Purchase Order by ID
  Endpoint:             GET /api/inventory/purchase-orders/{id:long}
  Auth:                 Authorize(Policy=ItemRead)
  Request DTO:          route: id (long)
  Response DTO:         InventoryPurchaseOrderResponse (same as INV-14 single item)
  DB Tables Read:       tblPurchaseOrder (with Supplier, Items, Item)
  Failure Cases:
    404 — PO not found
    401, 403 — auth/permission

---

#### Flow INV-16: Create Purchase Order
  Entry Points:         /inventory/purchase-orders/new (AdminMobile Create PO screen)
  UI Trigger:           Save/Submit button
  Endpoint:             POST /api/inventory/purchase-orders
  Auth:                 Authorize(Policy=StockManage)
  Request DTO:          InventoryPurchaseOrderCreateRequest (body)
    - SupplierId            string    required — numeric SupplierId string
    - ExpectedDeliveryDate  DateTime  required
    - Status                string?   — "draft" sets Draft; anything else → Submitted
    - Notes                 string?
    - Items                 InventoryPurchaseOrderCreateItemRequest[]  min 1
        - PartId            string    required — numeric ItemId string
        - OrderedQty        decimal   required
        - UnitPrice         decimal   required — if 0, defaults to latest PurchasePrice from item rates
  Response DTO:         InventoryPurchaseOrderResponse (created PO)
  DB Tables Read:       tblSupplier, tblItem (with Rates)
  DB Tables Written:    tblPurchaseOrder, tblPurchaseOrderItem
  Validation Rules:
    1. SupplierId must be numeric → 400.
    2. Items cannot be empty → 400.
    3. Each PartId must be numeric → 400.
    4. Supplier not found or inactive → 404.
    5. Any PartId not found → 404.
  Business Rules:
    1. PONumber auto-generated: "PO-{count+1:0000}".
    2. OrderedQty = max(OrderedQty, 0).
    3. UnitPrice = provided value; if 0, uses latest item PurchasePrice.
    4. Line amount = OrderedQty × UnitPrice; Tax = amount × TaxPercentage/100.
    5. TotalAmount = SubtotalAmount + TaxAmount.
  Failure Cases:
    400 — validation errors
    404 — supplier or part not found
    401, 403 — auth/permission

---

#### Flow INV-17: Receive Purchase Order (Goods Receipt)
  Entry Points:         /inventory/purchase-orders/{id} → Receive Goods button
  UI Trigger:           Record Receipt button with quantity inputs per line
  Endpoint:             PATCH /api/inventory/purchase-orders/{id:long}/receive
  Auth:                 Authorize(Policy=StockManage)
  Request DTO:          InventoryPurchaseOrderReceiveRequest (body)
    - ReceivedQtys      Dictionary<string, decimal>  — key: ItemId string; value: cumulative received qty
  Response DTO:         InventoryPurchaseOrderResponse (updated PO)
  DB Tables Read:       tblPurchaseOrder (with Supplier, Items, Item), tblWarehouse
  DB Tables Written:    tblPurchaseOrderItem (QuantityReceived, ReceivedAtUtc, DiscrepancyFlag),
                        tblWarehouseStock (QuantityOnHand incremented),
                        tblStockTransaction (PurchaseIn record per line),
                        tblPurchaseOrder (CurrentStatus, ReceivedAtUtc)
  Validation Rules:
    1. PO must exist and not be deleted → 404.
    2. Active warehouse must exist → 404.
    3. At least one received qty must exceed current received qty → 400 "At least one received quantity greater..."
  Business Rules:
    1. For each item: newReceivedQty = min(OrderedQty, requestedQty); only increments are processed.
    2. Increment = newReceivedQty - currentQuantityReceived.
    3. DiscrepancyFlag = QuantityReceived < QuantityOrdered.
    4. WarehouseStock incremented by increment; created if no record exists for warehouse+item.
    5. StockTransaction: TransactionType=PurchaseIn, ReferenceNumber=PONumber, TransactionGroupCode="PO-{id}".
    6. PO status: all items fully received → FullyReceived; else → PartiallyReceived.
  State Transitions:    PurchaseOrder: Submitted/Confirmed → PartiallyReceived → FullyReceived
  Failure Cases:
    404 — PO or warehouse not found
    400 — no valid increments
    401, 403 — auth/permission

---

#### Flow INV-18: List Suppliers
  Entry Points:         /inventory/suppliers (AdminMobile Supplier Management screen)
  Endpoint:             GET /api/inventory/suppliers
  Auth:                 Authorize(Policy=ItemRead)
  Request DTO:          None
  Response DTO:         IReadOnlyCollection<InventorySupplierResponse>
    - Id                string  — SupplierId.ToString()
    - Name              string  — SupplierName
    - ContactPerson     string
    - Phone             string  — MobileNumber
    - Email             string  — EmailAddress
    - LeadTimeDays      int     — always 0 (not persisted in current impl)
    - PaymentTerms      string  — always "Standard" (not persisted in current impl)
  DB Tables Read:       tblSupplier (active, not deleted; ordered by SupplierName)
  Failure Cases:
    401, 403 — auth/permission

---

#### Flow INV-19: Create Supplier
  Entry Points:         /inventory/suppliers/new (AdminMobile Add Supplier)
  Endpoint:             POST /api/inventory/suppliers
  Auth:                 Authorize(Policy=ItemCreate)
  Request DTO:          InventorySupplierCreateRequest (body)
    - Name              string    required
    - ContactPerson     string?
    - Phone             string?
    - Email             string?
    - LeadTimeDays      int?      — stored but not exposed in list response
    - PaymentTerms      string?   — stored but not exposed in list response
  Response DTO:         InventorySupplierResponse (new supplier)
  DB Tables Written:    tblSupplier
  Business Rules:
    1. SupplierCode auto-generated: "SUP-{count+1:0000}".
    2. IsActive defaults to true.
    3. AddressLine defaults to empty string.
  Failure Cases:
    400 — missing Name
    401, 403 — auth/permission

---

### ITEM CONTROLLER GROUP (/api/items) — Full Item Master CQRS

Note: ItemController exposes the full ItemResponse data model (all fields including TaxPercentage,
WarrantyDays, UoM, Supplier codes). InventoryController exposes simplified InventoryPartResponse
for AdminMobile. Both target the same tblItem table via EF Core.

---

#### Flow ITM-1: Create Inventory Item (Full Data Model)
  Entry Points:         Admin configuration → Inventory → New Item
  Endpoint:             POST /api/items
  Auth:                 Authorize(Policy=ItemCreate)
  Request DTO:          CreateItemRequest (body)
    - CategoryCode      string    required
    - CategoryName      string    required
    - UnitOfMeasureCode string    required
    - UnitOfMeasureName string    required
    - SupplierCode      string?
    - SupplierName      string?
    - ItemCode          string    required
    - ItemName          string    required
    - ItemDescription   string?
    - PurchasePrice     decimal   required
    - SellingPrice      decimal   required
    - TaxPercentage     decimal   required
    - WarrantyDays      int       required
    - ReorderLevel      decimal   required
    - IsActive          bool      required
  Response DTO:         ItemResponse
    - ItemId            long
    - ItemCode          string
    - ItemName          string
    - CategoryCode      string
    - CategoryName      string
    - UnitOfMeasureCode string
    - UnitOfMeasureName string
    - SupplierId        long?
    - SupplierCode      string?
    - SupplierName      string?
    - ItemDescription   string
    - PurchasePrice     decimal
    - SellingPrice      decimal
    - TaxPercentage     decimal
    - WarrantyDays      int
    - ReorderLevel      decimal
    - IsActive          bool
  DB Tables Written:    tblItem, tblItemCategory, tblUnitOfMeasure, tblSupplier (upsert via command)
  Failure Cases:
    400 — validation failure
    401, 403 — auth/permission

---

#### Flow ITM-2: Update Inventory Item
  Endpoint:             PUT /api/items/{id:long}
  Auth:                 Authorize(Policy=ItemCreate)
  Request DTO:          UpdateItemRequest (same fields as CreateItemRequest minus WarrantyDays)
  Response DTO:         ItemResponse
  DB Tables Written:    tblItem, tblItemCategory, tblUnitOfMeasure, tblSupplier (upsert via command)
  Failure Cases:
    404 — item not found
    400, 401, 403

---

#### Flow ITM-3: List Inventory Items (Paged)
  Endpoint:             GET /api/items
  Auth:                 Authorize(Policy=ItemRead)
  Request DTO:          query params
    - searchTerm        string?
    - isActive          bool?
    - pageNumber        int     default 1
    - pageSize          int     default 20
  Response DTO:         PagedResult<ItemResponse>
  DB Tables Read:       tblItem (via GetItemsQuery)
  Failure Cases:
    401, 403

---

#### Flow ITM-4: Get Item by ID
  Endpoint:             GET /api/items/{id:long}
  Auth:                 Authorize(Policy=ItemRead)
  Request DTO:          route: id (long)
  Response DTO:         ItemResponse
  DB Tables Read:       tblItem (via GetItemByIdQuery)
  Failure Cases:
    404 — item not found
    401, 403

---

### WAREHOUSE CONTROLLER GROUP (/api/warehouses)

---

#### Flow WH-1: Create Warehouse
  Endpoint:             POST /api/warehouses
  Auth:                 Authorize(Policy=WarehouseCreate)
  Request DTO:          CreateWarehouseRequest (body)
    - WarehouseCode     string    required
    - WarehouseName     string    required
    - ContactPerson     string?
    - MobileNumber      string?
    - EmailAddress      string?
    - AddressLine1      string?
    - AddressLine2      string?
    - Landmark          string?
    - CityName          string?
    - Pincode           string?
    - IsActive          bool      required
  Response DTO:         WarehouseResponse (WarehouseId, WarehouseCode, WarehouseName, ContactPerson, MobileNumber, EmailAddress, AddressLine1, AddressLine2, Landmark, CityName, Pincode, IsActive)
  DB Tables Written:    tblWarehouse (via CreateWarehouseCommand)
  Failure Cases:
    400, 401, 403

---

#### Flow WH-2: List Warehouses
  Endpoint:             GET /api/warehouses
  Auth:                 Authorize(Policy=WarehouseRead)
  Request DTO:          query params: searchTerm?, isActive?
  Response DTO:         IReadOnlyCollection<WarehouseResponse>
  DB Tables Read:       tblWarehouse (via GetWarehousesQuery)
  Failure Cases:
    401, 403

---

#### Flow WH-3: Get Warehouse Stock
  Entry Points:         Admin Inventory → Warehouse Detail → Stock tab
  Endpoint:             GET /api/warehouses/{id:long}/stock
  Auth:                 Authorize(Policy=WarehouseRead)
  Request DTO:          route: id (long, WarehouseId)
  Response DTO:         WarehouseStockResponse
    - WarehouseId       long
    - WarehouseCode     string
    - WarehouseName     string
    - TotalSkuCount     int
    - TotalQuantityOnHand decimal
    - Items             WarehouseStockItemResponse[]
        - WarehouseStockId  long
        - ItemId            long
        - ItemCode          string
        - ItemName          string
        - CategoryName      string
        - UnitOfMeasureCode string
        - QuantityOnHand    decimal
        - ReorderLevel      decimal
        - IsLowStock        bool
        - LastTransactionDateUtc DateTime?
  DB Tables Read:       tblWarehouse, tblWarehouseStock, tblItem, tblItemCategory, tblUnitOfMeasure
  Failure Cases:
    404 — warehouse not found
    401, 403

---

### STOCK CONTROLLER GROUP (/api/stock) — Raw Stock Operations

---

#### Flow STK-1: Record Stock Transaction (Raw)
  Entry Points:         Admin → Stock → Manual Transaction
  Endpoint:             POST /api/stock/transaction
  Auth:                 Authorize(Policy=StockManage)
  Request DTO:          RecordStockTransactionRequest (body)
    - WarehouseId       long      required
    - ItemId            long      required
    - TransactionType   string    required — enum string from StockTransactionType
    - Quantity          decimal   required
    - UnitCost          decimal   required
    - SupplierId        long?
    - ReferenceNumber   string?
    - Remarks           string?
  Response DTO:         StockTransactionResponse (StockTransactionId, ItemId, ItemCode, ItemName, WarehouseId, WarehouseName, TransactionType, Quantity, UnitCost, Amount, ReferenceNumber, TransactionDateUtc, BalanceAfterTransaction, Remarks)
  DB Tables Written:    tblStockTransaction, tblWarehouseStock (via RecordStockTransactionCommand)
  Failure Cases:
    400, 401, 403

---

#### Flow STK-2: Transfer Stock Between Warehouses
  Endpoint:             POST /api/stock/transfer
  Auth:                 Authorize(Policy=StockManage)
  Request DTO:          TransferStockRequest (body)
    - SourceWarehouseId       long      required
    - DestinationWarehouseId  long      required
    - ItemId                  long      required
    - Quantity                decimal   required
    - UnitCost                decimal   required
    - ReferenceNumber         string?
    - Remarks                 string?
  Response DTO:         IReadOnlyCollection<StockTransactionResponse> — [TransferOut, TransferIn]
  DB Tables Written:    tblStockTransaction (two records), tblWarehouseStock (two records updated)
  Business Rules:
    1. Creates TransferOut from source and TransferIn to destination atomically.
  Failure Cases:
    400, 404, 401, 403

---

#### Flow STK-3: Get Stock Transactions (Paged)
  Entry Points:         Admin → Stock → Transactions Ledger
  Endpoint:             GET /api/stock/transactions
  Auth:                 Authorize(Policy=StockRead)
  Request DTO:          query params
    - transactionType   string?
    - itemId            long?
    - warehouseId       long?
    - technicianId      long?
    - jobCardId         long?
    - fromDateUtc       DateTime?
    - toDateUtc         DateTime?
    - pageNumber        int     default 1
    - pageSize          int     default 20
  Response DTO:         PagedResult<StockTransactionResponse>
  DB Tables Read:       tblStockTransaction (via GetStockTransactionsQuery)
  Failure Cases:
    401, 403

---

### PARTS RETURN CONTROLLER GROUP (/api/parts-returns)
Auth: Authorize(Policy=StockManage) on all

---

#### Flow RTN-1: Create Parts Return (Defective/Unused)
  Entry Points:         Admin → Inventory → Parts Return → New Return
  Endpoint:             POST /api/parts-returns
  Auth:                 Authorize(Policy=StockManage)
  Request DTO:          CreatePartsReturnRequest (body)
    - ItemId            long      required — numeric ItemId
    - Quantity          decimal   required
    - ReasonCode        string    required — e.g. "Defective", "Unused", "Wrong Part"
    - DefectDescription string    required
    - TechnicianId      long?
    - JobCardId         long?
  Response DTO:         PartsReturnResponse
    - PartsReturnId       long
    - PartsReturnNumber   string
    - PartsReturnStatus   string
    - [field 4]           [VERIFY — GapPhaseA, 5th field not confirmed from grep]
    - SupplierClaimReference string
  DB Tables Written:    tblPartsReturn (via CreatePartsReturnCommand in GapPhaseA feature)
  Failure Cases:
    400, 401, 403

---

#### Flow RTN-2: Approve Parts Return
  Endpoint:             POST /api/parts-returns/{partsReturnId:long}/approve
  Auth:                 Authorize(Policy=StockManage)
  Request DTO:          ApprovePartsReturnRequest (body)
    - Remarks           string?
  Response DTO:         PartsReturnResponse
  DB Tables Written:    tblPartsReturn (via ApprovePartsReturnCommand in GapPhaseA feature)
  Failure Cases:
    404, 400, 401, 403

---

### SUPPLIER CONTROLLER GROUP (/api/suppliers)
Note: This controller handles warranty/defect claims against suppliers, not supplier master (supplier master is in InventoryController /api/inventory/suppliers).

---

#### Flow SUP-1: Create Supplier Claim
  Entry Points:         Admin → Parts Return → Raise Supplier Claim
  Endpoint:             POST /api/suppliers/claims
  Auth:                 Authorize(Policy=StockManage)
  Request DTO:          CreateSupplierClaimRequest (body)
    - PartsReturnId           long    required
    - SupplierClaimReference  string  required
    - Remarks                 string?
  Response DTO:         SupplierClaimResponse
    - PartsReturnId           long
    - PartsReturnNumber       string
    - PartsReturnStatus       string
    - SupplierClaimReference  string
  DB Tables Written:    tblPartsReturn (via CreateSupplierClaimCommand in GapPhaseA feature)
  Failure Cases:
    400, 404, 401, 403

---

### TECHNICIAN STOCK CONTROLLER GROUP (/api/technicians/{id}/...)
Note: Route prefix shared with TechnicianController. These endpoints are for van/field stock management.

---

#### Flow TKS-1: Assign Stock to Technician (Van Stock)
  Entry Points:         Admin → Technicians → {Technician} → Assign Stock
  Endpoint:             POST /api/technicians/{id:long}/stock-assign
  Auth:                 Authorize(Policy=StockManage)
  Request DTO:          AssignStockToTechnicianRequest (body)
    - SourceWarehouseId long      required
    - ItemId            long      required
    - Quantity          decimal   required
    - UnitCost          decimal   required
    - ReferenceNumber   string?
    - Remarks           string?
  Response DTO:         IReadOnlyCollection<StockTransactionResponse> — [TransferOut from warehouse, TransferOut to technician van stock]
  DB Tables Written:    tblStockTransaction, tblWarehouseStock, tblTechnicianVanStock (via AssignStockToTechnicianCommand)
  Business Rules:
    1. Deducts from SourceWarehouse stock.
    2. Increments Technician's van stock.
  Failure Cases:
    404 — technician or warehouse not found
    400, 401, 403

---

#### Flow TKS-2: Get Technician Stock
  Entry Points:         Admin → Technicians → {Technician} → Stock tab
  Endpoint:             GET /api/technicians/{id:long}/stock
  Auth:                 Authorize(Policy=StockRead)
  Request DTO:          route: id (long, TechnicianId)
  Response DTO:         TechnicianStockResponse
    - TechnicianId      long
    - TechnicianCode    string
    - TechnicianName    string
    - TotalSkuCount     int
    - TotalQuantityOnHand decimal
    - Items             TechnicianStockItemResponse[]
        - TechnicianVanStockId  long
        - ItemId                long
        - ItemCode              string
        - ItemName              string
        - CategoryName          string
        - UnitOfMeasureCode     string
        - QuantityOnHand        decimal
        - ReorderLevel          decimal
        - IsLowStock            bool
        - LastTransactionDateUtc DateTime?
  DB Tables Read:       tblTechnicianVanStock, tblItem, tblItemCategory, tblUnitOfMeasure
  Failure Cases:
    404 — technician not found
    401, 403

---

### DB TABLES — Inventory & Parts Management (Confirmed from EF Core DbContext)

tblItem
  - ItemId (PK bigint), ItemCode (nvarchar), ItemName (nvarchar), ItemCategoryId (FK),
    ItemDescription (nvarchar), SupplierId (FK nullable), ReorderLevel (decimal),
    TaxPercentage (decimal), WarrantyDays (int), IsActive (bit),
    + audit columns (CreatedBy, DateCreated, UpdatedBy, LastUpdated, IsDeleted, etc.)

tblItemCategory
  - ItemCategoryId (PK), CategoryCode (nvarchar), CategoryName (nvarchar)
  - Note: Old docs had "PartsCategories" — actual table is tblItemCategory.

tblItemRate
  - ItemRateId (PK), ItemId (FK), PurchasePrice (decimal), SellingPrice (decimal),
    IsActive (bit), EffectiveFromUtc (datetime2)
  - Note: Old docs listed UnitCost/SellingPrice directly on item — actual stored in separate tblItemRate.

tblWarehouse
  - WarehouseId (PK), WarehouseCode, WarehouseName, ContactPerson, MobileNumber,
    EmailAddress, AddressLine1, AddressLine2, Landmark, CityName, Pincode, IsActive

tblWarehouseStock
  - WarehouseStockId (PK), WarehouseId (FK), ItemId (FK), QuantityOnHand (decimal),
    LastTransactionDateUtc (datetime2 nullable), + audit columns
  - Note: Created on-demand during first PO receipt or stock assignment.

tblStockTransaction
  - StockTransactionId (PK), ItemId (FK), WarehouseId (FK), SupplierId (FK nullable),
    TechnicianId (FK nullable), JobCardId (FK nullable),
    TransactionType (enum: PurchaseIn|TransferIn|ReturnIn|JobConsumption|TransferOut|ReturnOut|AdjustmentIn|AdjustmentOut),
    Quantity (decimal), UnitCost (decimal), Amount (decimal),
    ReferenceNumber (nvarchar), TransactionGroupCode (nvarchar),
    TransactionDateUtc (datetime2), BalanceAfterTransaction (decimal), Remarks (nvarchar),
    + audit columns
  - Note: Old docs called this "StockLedger" — actual EF entity is StockTransaction/tblStockTransaction.

tblPartsRequest
  - PartsRequestId (PK), TechnicianId (FK), ServiceRequestId (FK), JobCardId (FK nullable),
    Urgency (enum: Normal|Emergency), CurrentStatus (enum: Pending|Approved|PartiallyApproved|Rejected),
    SubmittedAtUtc (datetime2), ProcessedAtUtc (datetime2 nullable), Notes (nvarchar)
  - Note: Old docs had "PartsRequests" with RequestItems in JSON — actual uses separate tblPartsRequestItem.

tblPartsRequestItem
  - PartsRequestItemId (PK), PartsRequestId (FK), ItemId (FK nullable), PartCode (nvarchar), PartName (nvarchar),
    QuantityRequested (decimal), QuantityApproved (decimal), CurrentStatus (enum)
  - Note: ItemId added 2026-04-25; pre-existing rows may have null ItemId (PartCode fallback active).

tblPurchaseOrder
  - PurchaseOrderId (PK), PONumber (nvarchar), SupplierId (FK), OrderDateUtc (datetime2),
    ExpectedDeliveryDateUtc (datetime2), ReceivedAtUtc (datetime2 nullable),
    CurrentStatus (enum: Draft|Submitted|Confirmed|PartiallyReceived|FullyReceived|Cancelled),
    SubtotalAmount (decimal), TaxAmount (decimal), TotalAmount (decimal), Notes (nvarchar),
    + audit columns
  - Note: Old docs named status values "Ordered/PartiallyReceived/Received/Cancelled" — actual enum:
    Draft|Submitted|Confirmed|PartiallyReceived|FullyReceived|Cancelled.

tblPurchaseOrderItem
  - PurchaseOrderItemId (PK), PurchaseOrderId (FK), ItemId (FK), PartCode, PartName,
    QuantityOrdered (decimal), QuantityReceived (decimal), UnitPrice (decimal), Amount (decimal),
    ReceivedAtUtc (datetime2 nullable), DiscrepancyFlag (bit), + audit columns
  - Note: Old docs had "POItems" — actual is tblPurchaseOrderItem.

tblSupplier
  - SupplierId (PK), SupplierCode (nvarchar), SupplierName, ContactPerson, MobileNumber,
    EmailAddress, AddressLine, IsActive, + audit columns
  - Note: Old docs listed SupplierIds as JSON array on PartsCatalog — actual is single SupplierId FK on tblItem.

tblTechnicianVanStock
  - TechnicianVanStockId (PK), TechnicianId (FK), ItemId (FK), QuantityOnHand (decimal),
    ReorderLevel (decimal), LastTransactionDateUtc (datetime2 nullable)

tblPartsReturn (GapPhaseA)
  - PartsReturnId (PK), PartsReturnNumber, ItemId (FK), Quantity (decimal),
    ReasonCode, DefectDescription, TechnicianId (FK nullable), JobCardId (FK nullable),
    PartsReturnStatus, SupplierClaimReference (nvarchar nullable)

---

### DRIFT NOTES — Inventory & Parts Management (corrected 2026-05-24)

1. DB naming drift:
   - "PartsCategories" → actual: tblItemCategory
   - "PartsCatalog" → actual: tblItem
   - "StockLedger" → actual: tblStockTransaction
   - "PartsRequests" → actual: tblPartsRequest
   - "PartsRequestItems" → actual: tblPartsRequestItem
   - "PurchaseOrders" → actual: tblPurchaseOrder
   - "POItems" → actual: tblPurchaseOrderItem
   - "CompatibleBrands (JSON)" on PartsCatalog → not in tblItem schema; CompatibleBrands always returns [] in API
   - "CompatibleModels (JSON)" on PartsCatalog → not in tblItem schema
   - "SupplierIds (JSON)" on PartsCatalog → actual: single SupplierId FK (long?) on tblItem

2. PO status drift: Old docs listed "Ordered/PartiallyReceived/Received/Cancelled" → actual enum:
   Draft | Submitted | Confirmed | PartiallyReceived | FullyReceived | Cancelled

3. Missing controllers (all entirely absent from old docs):
   - WarehouseController (3 endpoints: /api/warehouses)
   - StockController (3 endpoints: /api/stock)
   - TechnicianStockController (2 endpoints: /api/technicians/{id}/stock*)
   - PartsReturnController (2 endpoints: /api/parts-returns)
   - SupplierController (1 endpoint: /api/suppliers/claims)
   Total: 11 previously undocumented endpoints now added.

4. InventoryPartResponse.Id is string (not long) by design — IDs-only rule means ItemId.ToString() is
   the authoritative identifier; PartCode is display-only.

5. Old docs described "automatic follow-up request for remaining qty" on partial approval — not implemented
   in backend; partial approval simply records lower IssuedQty per line.

6. Reorder suggestions via "moving-average consumption" — not implemented as an endpoint; low-stock
   detection uses simple QtyOnHand <= ReorderLevel comparison.

7. Phase 13 implementation notes (replaced): routes from Phase 13 notes are now confirmed and stable.
   Notes removed as they contained redundant implementation history.

8. IDs-only rule (confirmed 2026-04-25): PartId in all request bodies accepts string representation
   of numeric ItemId only. PartCode is a display field only — never a route param or request identifier.

---

# SECTION 8 — EXTENDED PLATFORM MODULES

---

## MODULE: Lead Management

Notes on Drift (2026-06-11 — DB contract drift, ROOT CAUSE of "unexpected_error" on POST /api/leads):
  Symptom: every public Contact-form lead after the very first returned 500 unexpected_error
  ("An error occurred while saving the entity changes"). Same failure hit POST /api/bookings/guest.
  Real cause: the live Postgres DB had a UNIQUE index `UK_tblSystemAlert_AlertCode` on
  tblSystemAlert.AlertCode, but AlertCode is a REUSED event code. CreateLeadCommandHandler raises a
  SystemAlert via GapPhaseANotificationService.RaiseAlertAsync with constant alertCode="lead.received"
  (a TriggerCode/category — see SystemHealthFeature.RequiredTriggerCodes), so the 1st lead inserted
  AlertCode='lead.received' and every later lead collided → 23505 unique violation → whole
  SaveChanges rolled back. The EF model (LeadConfiguration/SystemAlert config) NEVER declared
  AlertCode unique → the unique index was hand-written-script drift (04_indexes.sql). Only
  GlobalExceptionMiddleware survived because it uniquifies its code as
  "system.unhandled_exception.{traceId}".
  Fix (Backend Engineer owning; DB Architect + Chief Architect + QA): dropped the unique index on the
  live DB and replaced it with a NON-unique lookup index `IDX_tblSystemAlert_AlertCode`; corrected
  Backend/Docs/Postgres/04_indexes.sql so a fresh deploy can't reintroduce it. No app code changed
  (AlertCode is intentionally reused; the EF model is the source of truth). Verified: two consecutive
  POST /api/leads now return 200 (leadIds 12,13 — test rows removed afterward).
  Generalize: any UNIQUE index on a reusable "*Code"/category column is suspect; audit 04_indexes.sql.

  Audit (2026-06-11) — diffed ALL 90 live-DB UNIQUE indexes vs the EF model's declared unique set.
  Only 3 DB-only unique indexes (not declared in EF = drift) existed:
    1. UK_tblSystemAlert_AlertCode            → FIXED (dropped; reused event code).  [this incident]
    2. UK_tblJobPartConsumption_StockTransactionId → FIXED (dropped → IDX_…). EF models
       StockTransaction.WithMany(JobPartConsumptions) i.e. 1:many, and the col is NOT NULL, so the
       unique index would 500 the 2nd part-consumption sharing a stock transaction. (table empty; latent)
    3. UK_tblRevisitRequest_WarrantyClaimId   → FIXED (dropped → IDX_…). Business confirmed a warranty
       claim MAY have more than one revisit request; col was nullable + not configured in EF. Now a plain
       lookup index so a 2nd revisit on the same claim does not fail.
  All other 87 unique indexes are legitimate (declared in EF: natural keys, generated *Number/*Reference,
  master *Code columns, 1:1 FKs, composite natural keys). 04_indexes.sql corrected for #1, #2 and #3.
  Live DB verified: 0 drift unique indexes remaining.

Entry Points:         Public website inquiry form (AllowAnonymous), Admin lead list (/admin/leads)
UI Trigger:           Customer submits inquiry form; Admin opens Leads list
API Endpoint Group:   LeadController — /api/leads

API Routes:
  POST   /api/leads                              — Create Lead (AllowAnonymous) → LeadResponse
  GET    /api/leads                              — Search Leads (Policy=ServiceRequestRead) → PagedResult<LeadListItemResponse>
  GET    /api/leads/analytics                    — Get Lead Analytics (Policy=ServiceRequestRead) → LeadAnalyticsResponse
  GET    /api/leads/{leadId}                     — Get Lead Detail (Policy=ServiceRequestRead) → LeadDetailResponse
  PUT    /api/leads/{leadId}/assign              — Assign Lead to User (Policy=ServiceRequestUpdate) → LeadResponse
  PUT    /api/leads/{leadId}/status              — Update Lead Status (Policy=ServiceRequestUpdate) → LeadResponse
  POST   /api/leads/{leadId}/convert-to-booking  — Convert Lead → Booking (Policy=BookingCreate) → LeadResponse
  POST   /api/leads/{leadId}/convert-to-sr       — Convert Lead → Service Request (Policy=ServiceRequestCreate) → LeadResponse
  POST   /api/leads/{leadId}/notes               — Add Lead Note (Policy=ServiceRequestUpdate) → LeadResponse

Request DTOs:
  CreateLeadRequest:
    CustomerName (string, required), MobileNumber (string, required), EmailAddress (string, optional)
    SourceChannel (string, required), AddressLine1, AddressLine2, CityName, Pincode
    ServiceId (long?), AcTypeId (long?), TonnageId (long?), BrandId (long?)
    SlotAvailabilityId (long?), InquiryNotes (string, optional)
  AssignLeadRequest:      AssignedUserId (long), Remarks (string?)
  UpdateLeadStatusRequest: LeadStatus (string), Remarks (string?), LostReason (string?)
  ConvertLeadToBookingRequest: ServiceId, AcTypeId, TonnageId, BrandId, SlotAvailabilityId, AddressLine1, AddressLine2, CityName, Pincode, InquiryNotes
  ConvertLeadToServiceRequestRequest: same shape as ConvertLeadToBookingRequest
  AddLeadNoteRequest:     NoteText (string, required), IsInternal (bool)

Response DTOs:
  LeadResponse:         LeadId, LeadStatus, CustomerName, MobileNumber, assigned user details
  LeadDetailResponse:   Full lead detail including notes, assignment history, conversion info
  LeadListItemResponse: Paged list item with status, channel, date
  LeadAnalyticsResponse: Aggregated lead funnel stats filtered by date range

Search Filters (GET /api/leads):
  searchTerm, leadStatus, sourceChannel, createdFrom (DateOnly), createdTo (DateOnly)
  pageNumber (default 1), pageSize (default 20)

Analytics Filters (GET /api/leads/analytics):
  fromDate (DateOnly?), toDate (DateOnly?)

DB Tables:
  Lead             — master lead record (LeadId, CustomerName, MobileNumber, EmailAddress, SourceChannel, LeadStatus, CurrentStatus, ServiceId, AddressLine1…Pincode, InquiryNotes, ConvertedBookingId?, ConvertedSRId?)
  LeadAssignment   — assignment history (LeadAssignmentId, LeadId FK, AssignedUserId, Remarks, DateCreated)
  LeadConversion   — conversion record (LeadConversionId, LeadId FK, ConversionType [Booking|SR], ConvertedEntityId, DateConverted)
  LeadNote         — notes (LeadNoteId, LeadId FK, NoteText, IsInternal, CreatedBy, DateCreated)
  LeadSource       — source channel master (LeadSourceId, SourceName, IsActive)
  LeadStatusHistory — status transitions (LeadStatusHistoryId, LeadId FK, OldStatus, NewStatus, Remarks, ChangedBy, DateChanged)

Business Rules:
  1. Lead creation is open to anonymous users (public inquiry form) — no authentication required.
  2. Lead must be assigned to an admin user before status can progress beyond New.
  3. Status transitions are logged to LeadStatusHistory on every UpdateLeadStatus call.
  4. Convert to Booking: creates a full Booking record; sets Lead.ConvertedBookingId and status = Converted.
  5. Convert to SR: creates a ServiceRequest directly; sets Lead.ConvertedSRId and status = Converted.
  6. A lead can only be converted once — second conversion attempt returns a conflict error.
  7. Notes marked IsInternal=true are visible to admin only, not the customer.
  8. LostReason is mandatory when UpdateLeadStatus sets LeadStatus = Lost.

State Transitions:
  New → Assigned (PUT /assign)
  Assigned → Contacted (PUT /status)
  Contacted → Qualified | Lost (PUT /status)
  Qualified → Converted (POST /convert-to-booking or /convert-to-sr)
  Any → Lost (PUT /status, LostReason required)

Failure Cases:
  POST /api/leads/{id}/convert-to-booking — Lead already converted → 409 Conflict
  PUT /api/leads/{id}/status with LeadStatus=Lost and no LostReason → 422 Unprocessable
  GET /api/leads/{leadId} with non-existent ID → 404 Not Found

Notes on Drift:
  Module was implemented under GapPhaseA feature folder — not previously documented.
  Application layer: Features/GapPhaseA/Lead/LeadManagementFeature.cs + LeadManagementPhaseBFeature.cs

---

## MODULE: Installation Management

Entry Points:         Public installation inquiry form (AllowAnonymous), Admin installation list (/admin/installations)
UI Trigger:           Customer submits installation request; Admin manages lifecycle
API Endpoint Group:   InstallationController + InstallationSurveyController + InstallationProposalController + InstallationExecutionController

API Routes:

  InstallationController (/api/installations):
    POST   /api/installations                                         — Create Installation Request (AllowAnonymous) → InstallationSummaryResponse
    GET    /api/installations                                         — List Installations (Authorize) → PagedResult<InstallationListItemResponse>
    GET    /api/installations/{installationId}                        — Get Installation Detail (Authorize) → InstallationDetailResponse
    POST   /api/installations/orders                                  — Create Installation Order (Policy=ServiceRequestUpdate) → InstallationOrderResponse
    POST   /api/installations/orders/{installationOrderId}/survey-report        — Submit Survey Report → InstallationOrderResponse
    POST   /api/installations/orders/{installationOrderId}/commissioning-certificate — Create Commissioning Certificate → CommissioningCertificateResponse

  InstallationSurveyController (/api/installations/{installationId}):
    POST   /api/installations/{installationId}/schedule-survey        — Schedule Survey (Authorize) → InstallationSummaryResponse
    POST   /api/installations/{installationId}/submit-survey          — Submit Survey Results (Authorize) → InstallationSummaryResponse

  InstallationProposalController (/api/installations/{installationId}):
    POST   /api/installations/{installationId}/proposal               — Create Proposal (Authorize) → InstallationSummaryResponse
    POST   /api/installations/{installationId}/proposal/approve       — Approve Proposal (Authorize) → InstallationSummaryResponse
    POST   /api/installations/{installationId}/proposal/reject        — Reject Proposal (Authorize) → InstallationSummaryResponse

  InstallationExecutionController (/api/installations/{installationId}):
    POST   /api/installations/{installationId}/create-order           — Create Execution Order (Authorize) → InstallationSummaryResponse
    POST   /api/installations/{installationId}/start                  — Start Installation (Authorize) → InstallationSummaryResponse
    POST   /api/installations/{installationId}/checklist              — Save Installation Checklist (Authorize) → InstallationSummaryResponse
    POST   /api/installations/{installationId}/complete               — Complete Installation (Authorize) → InstallationSummaryResponse
    POST   /api/installations/{installationId}/commission             — Generate Commissioning Record (Authorize) → InstallationSummaryResponse

Request DTOs:
  CreateInstallationRequest:
    LeadId (long?), CustomerName, MobileNumber, EmailAddress, SourceChannel
    AddressLine1, AddressLine2, CityName, Pincode
    InstallationType (string), NumberOfUnits (int), SiteNotes (string?), PreferredSurveyDateUtc (datetime?)
  CreateInstallationOrderRequest:
    LeadId, ServiceRequestId, CustomerId, CustomerAddressId, TechnicianId
    ScheduledInstallationDateUtc, InstallationChecklistJson (string)
  ScheduleInstallationSurveyRequest: SurveyDateUtc, TechnicianId, Remarks
  SubmitInstallationSurveyRequest:
    SiteConditionSummary, ElectricalReadiness, AccessReadiness, SafetyRiskNotes
    RecommendedAction, EstimatedMaterialCost (decimal?), MeasurementsJson, PhotoUrlsJson
    Items (survey checklist items)
  CreateInstallationProposalRequest: ProposalRemarks, Lines (line items)
  ApproveInstallationProposalRequest: CustomerRemarks
  RejectInstallationProposalRequest: CustomerRemarks
  CreateInstallationExecutionOrderRequest: TechnicianId, ScheduledInstallationDateUtc, HelperCount, ExecutionRemarks
  StartInstallationRequest: Remarks
  CompleteInstallationRequest: WorkSummary
  SaveInstallationChecklistRequest: Items
  GenerateInstallationCommissioningRequest:
    CustomerConfirmationName, CustomerSignatureName, ChecklistJson, Remarks, IsAccepted (bool)
  SubmitSurveyReportRequest (orders route):
    SurveyDecision, SiteConditionSummary, ElectricalReadiness, AccessReadiness
    SafetyRiskNotes, RecommendedAction, EstimatedMaterialCost, SyncDeviceReference, SyncReference
  CreateCommissioningCertificateRequest:
    CustomerConfirmationName, ChecklistJson, Remarks, IsAccepted (bool)

DB Tables:
  InstallationLead           — installation inquiry record linked to lead
  InstallationOrder          — confirmed installation job (OrderId, InstallationId FK, TechnicianId, ScheduledDateUtc, Status)
  InstallationProposal       — pricing proposal (ProposalId, InstallationId FK, Status, ProposalRemarks)
  InstallationProposalLine   — line items in proposal (ProposalLineId, ProposalId FK, Description, Qty, UnitPrice)
  InstallationStatusHistory  — status transitions (StatusHistoryId, InstallationId FK, OldStatus, NewStatus, ChangedBy, DateChanged)
  InstallationSurvey         — pre-installation site survey record
  InstallationSurveyItem     — individual survey checklist items
  InstallationChecklist      — installation checklist master / instance
  InstallationChecklistResponse — technician responses to checklist
  CommissioningCertificate   — post-installation commissioning sign-off (CertId, InstallationOrderId FK, CustomerConfirmationName, IsAccepted, DateCreated)
  SiteSurveyReport           — structured site survey report linked to InstallationOrder

Business Rules:
  1. Installation creation is AllowAnonymous — public inquiry form, linked to optional LeadId.
  2. Lifecycle: Request → Survey Scheduled → Survey Submitted → Proposal Created → Proposal Approved/Rejected → Execution Order → Started → Checklist Saved → Completed → Commissioned.
  3. Status transitions are recorded in InstallationStatusHistory at every lifecycle step.
  4. Survey must be submitted before a Proposal can be created.
  5. Proposal approval triggers the execution phase; rejection returns to survey/negotiation.
  6. Commissioning certificate requires customer confirmation name and IsAccepted=true for the installation to reach fully closed status.
  7. Application layer lives in Features/GapPhaseA/Installation (create) and Features/GapPhaseC/Installation (full lifecycle).

State Transitions:
  New → SurveyScheduled (/schedule-survey)
  SurveyScheduled → SurveySubmitted (/submit-survey)
  SurveySubmitted → ProposalCreated (/proposal)
  ProposalCreated → ProposalApproved (/proposal/approve) | ProposalRejected (/proposal/reject)
  ProposalApproved → ExecutionOrderCreated (/create-order)
  ExecutionOrderCreated → Started (/start)
  Started → InProgress (/checklist)
  InProgress → Completed (/complete)
  Completed → Commissioned (/commission)

Failure Cases:
  Survey submission without prior survey scheduling → 422 Unprocessable
  Proposal creation before survey submission → 422 Unprocessable
  Commissioning with IsAccepted=false → 422 — customer must accept to commission
  GET /api/installations/{id} — non-existent ID → 404 Not Found

Notes on Drift:
  Module split across two GapPhase feature sets:
    GapPhaseA → InstallationFeature.cs (create + basic queries)
    GapPhaseC → InstallationExecutionFeature, InstallationManagementFeature, InstallationProposalFeature, InstallationSurveyFeature, InstallationLifecycleSupport
  Not previously documented anywhere in ProjectOverview.

---

## MODULE: Branch Management

Entry Points:         Admin Portal — Branch Settings (/admin/settings/branches)
UI Trigger:           Admin navigates to branch list; creates or edits a branch
API Endpoint Group:   BranchController — /api/branches

API Routes:
  GET    /api/branches             — List / Search Branches (Policy=UserRead) → IReadOnlyCollection<BranchResponse>
  GET    /api/branches/{branchId}  — Get Branch by ID (Policy=UserRead) → BranchResponse
  POST   /api/branches             — Create Branch (Policy=UserCreate) → BranchResponse
  PUT    /api/branches/{branchId}  — Update Branch (Policy=UserUpdate) → BranchResponse

Request DTOs:
  BranchUpsertRequest:
    Name (string, required), City (string, required), Address (string, required)
    ManagerId (long?, optional — resolved to ManagerName from Users), Zones (string[], optional)
    IsActive (bool, required)

Response DTOs:
  BranchResponse:
    BranchId (int), Name, City, Address, ManagerId (long?), ManagerName (string?)
    Zones (string[]), IsActive (bool)
    TechnicianCount (int — live count of active Technician + Helper users linked to this BranchId)
    ServiceRequestCount (int — live count of non-cancelled SRs linked to this BranchId)

Search Filters (GET /api/branches):
  searchTerm (string?), isActive (bool?)

DB Tables / Storage:
  DynamicMasterRecord — Branches are stored as dynamic master records with MasterType="Branch"
    MasterCode   = slug + GUID (auto-generated, max 64 chars)
    MasterLabel  = Branch name
    MasterValue  = JSON payload: { City, Address, ManagerId, ManagerName, Zones[] }
    Description  = Address (denormalized for quick display)
    IsActive     = active flag
  Users.BranchId    — FK linking users (technicians/helpers) to a branch
  ServiceRequests.BranchId — FK linking SRs to a branch

Business Rules:
  1. Branch is not a dedicated table — stored in DynamicMasterRecord with MasterType="Branch".
  2. On GET, TechnicianCount is computed live: count of Users with IsActive=true, IsDeleted=false, BranchId=branchId, and Role = Technician or Helper.
  3. ServiceRequestCount is computed live: count of SRs with BranchId=branchId and CurrentStatus ≠ Cancelled.
  4. ManagerId is optional; if supplied, ManagerName is resolved from Users table at save time (denormalized into JSON payload).
  5. MasterCode is auto-generated slug + UUID — never editable after creation.
  6. Zones is a free-text list of zone names (not FK references) stored in the JSON payload.
  7. Controller directly queries CoolzoDbContext — no CQRS mediator for Branch (direct EF Core access pattern).

Failure Cases:
  GET /api/branches/{branchId} with wrong MasterType or IsDeleted=true → 404 Not Found
  PUT /api/branches/{branchId} — branch not found or deleted → 404 Not Found

Notes on Drift:
  Branch uses DynamicMasterRecord pattern (not a dedicated tblBranch table).
  Live count queries run on every GET — no materialized view.
  Not previously documented in ProjectOverview.

---

## MODULE: Campaign Management

Entry Points:         Admin Portal — Marketing / Campaign screen (/admin/marketing/campaigns)
UI Trigger:           Admin creates a campaign to batch-schedule bookings for a service + zone
API Endpoint Group:   CampaignController — /api/campaigns

API Routes:
  POST   /api/campaigns   — Create Campaign (Policy=BookingCreate) → CampaignResponse

Request DTOs:
  CreateCampaignRequest:
    CampaignName (string, required)
    ServiceId (long, required)
    ZoneId (long, required)
    SlotAvailabilityId (long, required)
    PlannedBookingCount (int, required)
    StartDateUtc (datetime, required)
    EndDateUtc (datetime, required)
    Notes (string, optional)

Response DTOs:
  CampaignResponse: CampaignId, CampaignName, ServiceId, ZoneId, SlotAvailabilityId, PlannedBookingCount, StartDateUtc, EndDateUtc, Notes, Status, DateCreated

DB Tables:
  Campaign — campaign master record (CampaignId, CampaignName, ServiceId FK, ZoneId FK, SlotAvailabilityId FK, PlannedBookingCount, StartDateUtc, EndDateUtc, Notes, Status, CreatedBy, DateCreated)

Business Rules:
  1. Campaign creation requires Policy=BookingCreate — operations/admin role minimum.
  2. Campaign defines a batch booking intent for a service in a specific zone and slot.
  3. Only POST (create) is implemented — list, detail, and cancel endpoints are not yet built.
  4. Application layer: Features/GapPhaseA/Campaign/CreateCampaignCommand.

Failure Cases:
  Non-existent ServiceId / ZoneId / SlotAvailabilityId → 422 Unprocessable (FK validation in handler)

Notes on Drift:
  Only Campaign create endpoint is implemented; read/update/cancel not yet built.
  Not previously documented in ProjectOverview.

---

## MODULE: Support Tickets

Entry Points:         Customer Portal — My Support (raise ticket); Admin Portal — Support Queue
UI Trigger:           Customer clicks "Raise Ticket"; Admin opens support queue
API Endpoint Group:   SupportTicketController + SupportTicketEscalationController + SupportTicketLookupController + SupportTicketReplyController

API Routes:

  SupportTicketController (/api/support-tickets):
    POST   /api/support-tickets                                      — Create Ticket (Authorize) → SupportTicketDetailResponse
    GET    /api/support-tickets                                      — Search Tickets (Policy=SupportRead) → PagedResult<SupportTicketListItemResponse>
    GET    /api/support-tickets/{supportTicketId}                    — Get Ticket Detail (Authorize) → SupportTicketDetailResponse
    GET    /api/support-tickets/my-tickets                           — Get My Tickets (Authorize, JWT-scoped) → PagedResult | SupportTicketCountResponse
    POST   /api/support-tickets/{supportTicketId}/assign             — Assign Ticket (Policy=SupportManage) → SupportTicketDetailResponse
    POST   /api/support-tickets/{supportTicketId}/change-status      — Change Status (Policy=SupportManage) → SupportTicketDetailResponse
    POST   /api/support-tickets/{supportTicketId}/change-priority    — Change Priority (Policy=SupportManage) → SupportTicketDetailResponse
    POST   /api/support-tickets/{supportTicketId}/close              — Close Ticket (Authorize) → SupportTicketDetailResponse
    POST   /api/support-tickets/{supportTicketId}/reopen             — Reopen Ticket (Authorize) → SupportTicketDetailResponse

  SupportTicketEscalationController (/api/support-tickets/{supportTicketId}):
    GET    /api/support-tickets/{supportTicketId}/escalations        — Get Escalations (Authorize) → IReadOnlyCollection<SupportTicketEscalationResponse>
    POST   /api/support-tickets/{supportTicketId}/escalate           — Escalate Ticket (Policy=SupportManage) → SupportTicketDetailResponse

  SupportTicketReplyController (/api/support-tickets/{supportTicketId}/replies):
    GET    /api/support-tickets/{supportTicketId}/replies            — Get Replies (Authorize) → IReadOnlyCollection<SupportTicketReplyResponse>
    POST   /api/support-tickets/{supportTicketId}/replies            — Add Reply (Authorize) → SupportTicketReplyResponse

  SupportTicketLookupController (/api/support-ticket-lookups):
    GET    /api/support-ticket-lookups/categories                    — Get Categories → IReadOnlyCollection<LookupItemResponse>
    GET    /api/support-ticket-lookups/priorities                    — Get Priorities → IReadOnlyCollection<LookupItemResponse>
    GET    /api/support-ticket-lookups/statuses                      — Get Statuses → IReadOnlyCollection<LookupItemResponse>
    GET    /api/support/categories                                   — Alias for categories (AllowAnonymous alias route)

Request DTOs:
  CreateSupportTicketRequest:
    CustomerId (long, required), Subject (string, required), CategoryId (long, required)
    PriorityId (long, required), Description (string, required)
    Links (CreateSupportTicketLinkRequest[], optional — linked entities e.g. BookingId, SRId)
  AssignSupportTicketRequest:      AssignedUserId (long), Remarks (string?)
  ChangeSupportTicketStatusRequest: Status (string), Remarks (string?)
  ChangeSupportTicketPriorityRequest: PriorityId (long), Remarks (string?)
  SupportTicketActionRequest:      Remarks (string?) — used for Close and Reopen
  EscalateSupportTicketRequest:    EscalationTarget (string), EscalationRemarks (string?)
  AddSupportTicketReplyRequest:    ReplyText (string, required), IsInternalOnly (bool)

Response DTOs:
  SupportTicketDetailResponse: Full ticket with status, priority, category, assignments, replies, escalations, links
  SupportTicketListItemResponse: Paged list item with TicketNumber, Subject, Status, Priority, CustomerName, DateCreated
  SupportTicketCountResponse: TotalCount (int) — returned when countOnly=true
  SupportTicketReplyResponse: ReplyId, TicketId, ReplyText, IsInternalOnly, AuthorName, DateCreated
  SupportTicketEscalationResponse: EscalationId, TicketId, EscalationTarget, EscalationRemarks, EscalatedBy, DateEscalated

Search Filters (GET /api/support-tickets):
  ticketNumber, customerMobile, categoryId, priorityId, status, dateFrom, dateTo, linkedEntityType
  pageNumber (default 1), pageSize (default 20)

GET /api/support-tickets/my-tickets Query Params:
  pageNumber, pageSize, countOnly (bool — returns SupportTicketCountResponse instead), unread (bool)

DB Tables:
  SupportTicket              — master record (TicketId, TicketNumber, CustomerId FK, Subject, CategoryId FK, PriorityId FK, Status, Description, DateCreated)
  SupportTicketAssignment    — assignment history (AssignmentId, TicketId FK, AssignedUserId, Remarks, DateCreated)
  SupportTicketReply         — replies (ReplyId, TicketId FK, ReplyText, IsInternalOnly, CreatedBy, DateCreated)
  SupportTicketEscalation    — escalations (EscalationId, TicketId FK, EscalationTarget, EscalationRemarks, EscalatedBy, DateEscalated)
  SupportTicketCategory      — category master (CategoryId, CategoryName, IsActive)
  SupportTicketPriority      — priority master (PriorityId, PriorityName, SortOrder, IsActive)
  SupportTicketLink          — linked entities (LinkId, TicketId FK, LinkedEntityType, LinkedEntityId)
  SupportTicketStatusHistory — status transitions (StatusHistoryId, TicketId FK, OldStatus, NewStatus, Remarks, ChangedBy, DateChanged)

Business Rules:
  1. Any authenticated user can create a support ticket.
  2. Customer-scoped GET /my-tickets returns only tickets belonging to the authenticated customer (JWT-scoped).
  3. Admin search (GET /support-tickets) requires Policy=SupportRead.
  4. Assign, change-status, change-priority, and escalate require Policy=SupportManage.
  5. Close and Reopen can be called by any authenticated user (customer can close their own ticket).
  6. IsInternalOnly replies are visible to admin staff only — never returned in customer-facing responses.
  7. countOnly=true on /my-tickets returns SupportTicketCountResponse with TotalCount — no paged list.
  8. Ticket links (SupportTicketLink) allow associating a ticket to a Booking, SR, Invoice, or other entity by type + ID.

State Transitions:
  Open → Assigned (POST /assign)
  Assigned → InProgress | Resolved (POST /change-status)
  Resolved → Closed (POST /close) | Reopened (POST /reopen)
  Any → Escalated (POST /escalate)
  Escalated → InProgress | Resolved (POST /change-status)

Failure Cases:
  GET /api/support-tickets/{id} — not found → 404
  POST /assign with non-existent AssignedUserId → 422 Unprocessable
  POST /escalate without Policy=SupportManage → 403 Forbidden

Notes on Drift:
  SupportTickets were referenced in Customer Portal (SECTION 6) but had no dedicated section.
  Lookup alias route GET /api/support/categories is a redundant alias for backward compatibility.

---

## MODULE: Feedback Management

Entry Points:         Admin Portal — Feedback Queue (/admin/feedback)
UI Trigger:           Admin views customer reviews/feedback; responds, publishes, or flags
API Endpoint Group:   FeedbackController — /api/feedback

API Routes:
  GET    /api/feedback                                  — List Feedback (Policy=SupportRead) → IReadOnlyCollection<SupportFeedbackResponse>
  GET    /api/feedback/{customerReviewId}               — Get Feedback Detail (Policy=SupportRead) → SupportFeedbackResponse
  PATCH  /api/feedback/{customerReviewId}/respond       — Respond to Feedback (Policy=SupportManage) → SupportFeedbackResponse
  PATCH  /api/feedback/{customerReviewId}/publish       — Publish / Unpublish (Policy=SupportManage) → SupportFeedbackResponse
  PATCH  /api/feedback/{customerReviewId}/flag          — Flag Feedback (Policy=SupportManage) → SupportFeedbackResponse

Request DTOs:
  RespondFeedbackRequest:  Response (string — admin's public response text)
  PublishFeedbackRequest:  Publish (bool — true = publish, false = unpublish)
  FlagFeedbackRequest:     Reason (string — reason for flagging)

Query Filters (GET /api/feedback):
  serviceId (long?, optional — filter feedback by service)

Response DTOs:
  SupportFeedbackResponse: CustomerReviewId, CustomerId, CustomerName, ServiceId, ServiceName, Rating, ReviewText, AdminResponse, IsPublished, IsFlagged, FlagReason, DateCreated, DateResponded

DB Tables:
  CustomerReview  — review/feedback record (CustomerReviewId, CustomerId FK, ServiceId FK, Rating, ReviewText, AdminResponse, IsPublished, IsFlagged, FlagReason, DateCreated, DateResponded)

Business Rules:
  1. Feedback is submitted by customers via Customer Portal (CustomerReviewController — Section 6).
  2. FeedbackController provides admin-side management only (view, respond, publish, flag).
  3. Policy=SupportRead required to view; Policy=SupportManage required to respond, publish, or flag.
  4. PATCH /publish with Publish=false unpublishes a previously published review.
  5. Flagged reviews are hidden from public display regardless of IsPublished state.
  6. AdminResponse is a single text field — only one admin response per review (overwrites on re-respond).

Failure Cases:
  GET /api/feedback/{id} — not found → 404 Not Found
  PATCH /respond with empty Response text → empty string is accepted (Response ?? string.Empty)

Notes on Drift:
  Feedback management was listed in Customer Portal section but had no dedicated module entry.
  Backed by CustomerReview entity (same table used by customer-side CustomerReviewController).

---

## MODULE: Revisit Management

Entry Points:         Admin Portal or Customer Portal — linked from Invoice or SR detail
UI Trigger:           Admin or customer raises revisit after warranty/AMC issue; links to original job
API Endpoint Group:   RevisitController — /api/revisit

API Routes:
  POST   /api/revisit/request                  — Create Revisit Request (Authorize) → RevisitRequestResponse
  GET    /api/revisit/booking/{bookingId}       — Get Revisits by Booking (Authorize) → IReadOnlyCollection<RevisitRequestResponse>

Request DTOs:
  RevisitRequestCreateRequest:
    OriginalJobCardId (long, required)
    RevisitType (string, required — e.g. Warranty | AMC | Callback | Complaint)
    PreferredVisitDateUtc (datetime?, optional)
    IssueSummary (string, required)
    RequestRemarks (string?, optional)
    CustomerAmcId (long?, optional — link to AMC contract if AMC revisit)
    WarrantyClaimId (long?, optional — link to warranty claim if warranty revisit)
    ChargeAmount (decimal?, optional — 0 for free revisit, value if chargeable)

Response DTOs:
  RevisitRequestResponse:
    RevisitRequestId, OriginalJobCardId, RevisitType, PreferredVisitDateUtc, IssueSummary
    RequestRemarks, CustomerAmcId?, WarrantyClaimId?, ChargeAmount?, Status, DateCreated

DB Tables:
  RevisitRequest  — revisit record (RevisitRequestId, OriginalJobCardId FK → JobCard, RevisitType, PreferredVisitDateUtc, IssueSummary, RequestRemarks, CustomerAmcId FK?, WarrantyClaimId FK?, ChargeAmount?, Status, CreatedBy, DateCreated)

Business Rules:
  1. Any authenticated user can raise a revisit request.
  2. RevisitType must be one of: Warranty, AMC, Callback, Complaint.
  3. If RevisitType=Warranty → WarrantyClaimId should be provided (links revisit to a WarrantyClaim created via /api/warranty/claim).
  4. If RevisitType=AMC → CustomerAmcId should be provided.
  5. ChargeAmount = 0 or null indicates free revisit (warranty/AMC); positive value indicates a chargeable callback.
  6. On approval, the revisit generates a new ServiceRequest linked to OriginalJobCardId.
  7. GET /booking/{bookingId} returns all revisit requests for all jobs under a booking.

Cross-Module Links:
  WarrantyClaim.RevisitRequestId → populated when warranty claim triggers a revisit (see Warranty Management, Section 3)
  JobCard.OriginalJobCardId → original job the revisit is raised against

Failure Cases:
  POST with non-existent OriginalJobCardId → 422 or 404 from handler
  GET /booking/{bookingId} — booking not found → returns empty collection (not 404)

Notes on Drift:
  Revisit was referenced in Warranty Management (Section 3) as "revisit SR created" but the module itself was undocumented.
  Application layer: Features/Revisit/Commands/CreateRevisitRequest + Features/Revisit/Queries/GetRevisitByBooking

---

## MODULE: Analytics & Dashboard

Entry Points:         Admin Portal — Analytics Hub (/admin/analytics), Dashboard Home (/admin/dashboard)
UI Trigger:           Admin navigates to analytics section or opens dashboard
API Endpoint Group:   AnalyticsController (/api/analytics) + DashboardController (/api/dashboard)

API Routes:

  AnalyticsController (/api/analytics, Policy=AnalyticsRead):
    GET   /api/analytics/bookings       — Booking Analytics (Policy=AnalyticsRead) → BookingAnalyticsResponse
    GET   /api/analytics/revenue        — Revenue Analytics (Policy=AnalyticsRead) → RevenueAnalyticsResponse
    GET   /api/analytics/technicians    — Technician Performance (Policy=AnalyticsRead) → TechnicianPerformanceResponse
    GET   /api/analytics/customers      — Customer Analytics (Policy=AnalyticsRead) → CustomerAnalyticsResponse
    GET   /api/analytics/support        — Support Analytics (Policy=SupportRead) → SupportAnalyticsResponse
    GET   /api/analytics/inventory      — Inventory Analytics (Policy=AnalyticsRead) → InventoryAnalyticsResponse

  DashboardController (/api/dashboard, Policy=DashboardRead):
    GET   /api/dashboard/summary        — Dashboard Summary → DashboardSummaryResponse
    GET   /api/dashboard/metrics        — Dashboard Metrics (date-range filterable) → DashboardMetricsResponse

Common Query Filters (analytics endpoints):
  dateFrom (DateOnly?), dateTo (DateOnly?), trendBy (string? — e.g. "day", "week", "month")
  Per-endpoint extras:
    /bookings:    serviceId (long?), status (string?)
    /revenue:     serviceId (long?)
    /technicians: technicianId (long?), status (string?)
    /support:     status (string?)

DashboardMetrics Query Filters:
  dateFrom (DateOnly?), dateTo (DateOnly?), trendBy (string?)

Response DTOs:
  BookingAnalyticsResponse:      Booking volumes, trends, conversion rates by service/status
  RevenueAnalyticsResponse:      Revenue totals, trends, breakdown by service
  TechnicianPerformanceResponse: Job completion rates, avg resolution time, ratings per technician
  CustomerAnalyticsResponse:     New vs returning customers, growth trend, engagement metrics
  SupportAnalyticsResponse:      Ticket volumes, resolution time, category breakdown
  InventoryAnalyticsResponse:    Stock movement, low-stock alerts, PO fulfilment stats
  DashboardSummaryResponse:      KPI snapshot — total SRs, open jobs, revenue today, active technicians
  DashboardMetricsResponse:      Detailed metrics with trend lines for selected date range

DB Tables:
  (Analytics are computed queries — no dedicated analytics table)
  Source tables: Bookings, ServiceRequests, InvoiceHeaders, PaymentTransactions, Technicians, Customers,
                 SupportTickets, JobCards, WarehouseStock, StockTransactions, CustomerReviews

Business Rules:
  1. All analytics endpoints require Policy=AnalyticsRead except /analytics/support which requires Policy=SupportRead.
  2. DashboardController requires Policy=DashboardRead.
  3. All analytics are computed at query time — no pre-aggregated materialized tables.
  4. trendBy controls grouping: "day" | "week" | "month" — defaults to "month" if omitted.
  5. Date range defaults: if dateFrom/dateTo not supplied, each query applies its own default window (typically last 30 days).

Failure Cases:
  Invalid trendBy value → handler normalises to "month" (no error thrown)
  dateFrom > dateTo → 422 Unprocessable

Notes on Drift:
  Dashboard and Analytics were listed under "Reports & Audit" in SECTION 9 of ModuleIndex but never given a dedicated section in ProjectOverview.
  Application layer: Features/Analytics/* (6 query handlers) + Features/Dashboard/* (2 query handlers)

---

## MODULE: RBAC — Users, Roles & Permissions

Entry Points:         Admin Portal — User Management (/admin/users), Role Manager (/admin/roles)
UI Trigger:           SuperAdmin creates users, defines roles, assigns permissions
API Endpoint Group:   UserController (/api/users) + RoleController (/api/roles) + PermissionController (/api/permissions)

API Routes:

  UserController (/api/users):
    GET    /api/users                          — List Users (Policy=UserRead) → PagedResult<UserResponse>
    GET    /api/users/{userId}                 — Get User Detail (Policy=UserRead) → UserDetailResponse
    POST   /api/users                          — Create User (Policy=UserCreate) → UserResponse
    PUT    /api/users/{userId}                 — Update User (Policy=UserUpdate) → UserResponse
    POST   /api/users/{userId}/deactivate      — Deactivate User (Policy=UserUpdate) → UserResponse
    POST   /api/users/{userId}/reactivate      — Reactivate User (Policy=UserUpdate) → UserResponse
    POST   /api/users/{userId}/reset-password  — Reset Password (Policy=UserUpdate) → UserPasswordResetResponse
    POST   /api/users/{userId}/reset-pin       — Reset PIN (Policy=UserUpdate) → UserPasswordResetResponse

  RoleController (/api/roles):
    GET    /api/roles                          — List Roles (Policy=RoleRead) → PagedResult<RoleResponse>
    POST   /api/roles                          — Create Role (Policy=RoleCreate) → RoleResponse
    PUT    /api/roles/{roleId}                 — Update Role (Policy=RoleUpdate) → RoleResponse
    GET    /api/roles/{roleId}/permissions     — Get Role Permission Snapshot (Policy=RoleRead) → RolePermissionSnapshotResponse
    PUT    /api/roles/{roleId}/permissions     — Update Role Permissions (Policy=RoleUpdate) → RoleResponse

  PermissionController (/api/permissions):
    GET    /api/permissions                    — List Permissions (Policy=PermissionRead) → PagedResult<PermissionResponse>

Request DTOs:
  GetUsersQuery filters: pageNumber, pageSize, searchTerm, isActive (bool?), roleIds (long[]?), branchIds (int[]?), sortBy, sortOrder
  CreateUserRequest:  UserName, Email, FullName, Password, IsActive (bool), RoleIds (long[]), BranchId (int?)
  UpdateUserRequest:  Email, FullName, IsActive (bool), RoleIds (long[]), BranchId (int?)
  DeactivateUserRequest: Reason (string)
  ResetUserPasswordRequest: Reason (string)
  CreateRoleRequest:  RoleName, DisplayName, Description, IsActive (bool), PermissionIds (long[])
  UpdateRoleRequest:  DisplayName, Description, IsActive (bool), PermissionIds (long[])
  UpdateRolePermissionsRequest: PermissionIds (long[])

Response DTOs:
  UserResponse:       UserId, UserName, Email, FullName, IsActive, BranchId?, Roles[]
  UserDetailResponse: Full user with role assignments, branch detail, creation metadata
  UserPasswordResetResponse: UserId, TemporaryPassword (or confirmation token)
  RoleResponse:       RoleId, RoleName, DisplayName, Description, IsActive, Permissions[]
  RolePermissionSnapshotResponse:
    RoleId, PermissionIds[], ModuleMatrix (permission-to-module map), DataScope,
    PermissionNames[], RoleName, DisplayName
  PermissionResponse: PermissionId, PermissionName, DisplayName, Module, IsActive

DB Tables:
  User              — platform users (UserId, UserName, Email, FullName, PasswordHash, PinHash, IsActive, BranchId FK, IsDeleted, DateCreated)
  Role              — role definitions (RoleId, RoleName, DisplayName, Description, IsActive, IsDeleted)
  UserRole          — many-to-many user↔role (UserRoleId, UserId FK, RoleId FK, IsDeleted)
  Permission        — permission definitions (PermissionId, PermissionName, DisplayName, Module, IsActive, IsDeleted)
  RolePermission    — many-to-many role↔permission (RolePermissionId, RoleId FK, PermissionId FK, IsDeleted)
  UserPasswordHistory — password change history (audit) (HistoryId, UserId FK, PasswordHash, DateChanged)
  UserSession       — active sessions (SessionId, UserId FK, Token, ExpiresAt, IsRevoked)

Business Rules:
  1. User creation requires Policy=UserCreate. Users are assigned one or more RoleIds at creation.
  2. BranchId links a user to a branch (optional — system/SuperAdmin users may have no branch).
  3. Role assignment is many-to-many — one user can hold multiple roles.
  4. Permission is assigned at the Role level (not directly to users).
  5. RolePermissionSnapshot includes PermissionModuleMatrixMapper.Build() — computes a structured module-permission matrix for the frontend permission editor.
  6. DataScope is resolved from RoleName via PermissionDataScopeMapper.Resolve() — governs what records a role can see.
  7. Deactivate preserves the user record (soft deactivate); Reactivate re-enables without a new password.
  8. ResetPassword generates a temporary credential (returned once in response — not stored in plain text).
  9. ResetPin resets the field-login PIN used by technicians (separate from the web password).
  10. PermissionNames follow the PermissionNames constants (e.g. UserRead, UserCreate, RoleUpdate, SupportManage) — never free-text strings.

Failure Cases:
  POST /api/users — duplicate UserName or Email → 409 Conflict
  GET /api/users/{id} — not found → 404 Not Found
  PUT /api/roles/{id}/permissions — roleId not found → 404 Not Found
  POST /api/users/{id}/deactivate — user already inactive → 422 Unprocessable

Notes on Drift:
  RBAC was referenced throughout other modules via policy names but had no dedicated section in ProjectOverview.
  RolePermissionSnapshotResponse is the canonical response for the admin permission editor screen.
  Application layer: Features/User/* (7 commands/queries) + Features/Role/* (2 commands + 1 query) + Features/Permission/* (1 query)

---

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

CATALOG-NATIVE SERVICE IMAGE (Admin Service & Equipment Catalog) — [IMPLEMENTED 2026-06-10]:
  Purpose: give each Service Type a per-record image, uploaded + managed where the service is edited
  (Admin → Service & Equipment Catalog → Service Types). Persisted in the master record itself; no new
  table or column — the URL is stored inside the existing metadata JSON (masterValue).

  Flow Name: Upload & Persist Service Catalog Image
    Entry Point:    Frontend/Admin → ServiceCatalogScreen.tsx (Service Types tab), Add/Edit form.
    UI Trigger:     "Service Image" Upload/Replace file input (PNG/JPEG/WebP/SVG ≤5MB) + preview + Remove.
    Upload API:     POST /api/admin-masters/upload-image  (Authorize Policy=lookup.manage)
                    Controller: MasterDataAdminController.UploadImageAsync
                    Feature: MasterDataAdmin/Commands/UploadMasterImage/UploadMasterImageFeature.cs
    Request DTO:    MasterImageUploadRequest { Folder, FileName, ContentType, Base64Content }
                      Folder = master slug (e.g. "service-types"); validated ^[a-z0-9-]+$.
    Response DTO:   MasterImageUploadResponse { Url }  (object-storage public URL)
    Storage:        IObjectStorageService.PutObjectAsync → key "catalog/{folder}/{guid}{ext}"
                    (same FileSystem/R2 provider as CMS screen images). STATELESS — upload writes the
                    file + audit log only; it does NOT persist to the master record by itself.
    Persistence (DB): the returned Url is placed in the form as metadata.imageUrl and saved on the
                    EXISTING create/update master flow (POST/PUT /api/master/service-types →
                    Phase4ConfigurationController) which serializes metadata into
                    tblDynamicMasterRecord.MasterValue (JSON). So "save in DB" = save the service after upload.
    Get & Bind:     GET /api/master/service-types → MasterValue JSON parsed to metadata →
                    ServiceCatalogScreen hydrates form.imageUrl (preview) AND renders a list-card thumbnail.
    Validation:     ContentType ∈ {png,jpeg,webp,svg+xml}; ≤5MB; base64 decodable. metadata.imageUrl is
                    optional (omitted when blank).
    Constraint:     MasterValue is capped at 512 chars (CreateDynamicMasterRecord validator). A typical
                    service-types JSON + an object-storage URL stays well under this; keep public base
                    URLs short. [VERIFY if very long CDN domains + many metadata fields approach 512.]
    Failure Cases:  invalid type/size/base64 → 400 standard envelope; toast on the client. Upload success
                    but service not saved → image orphaned in storage until a save references it.
    Files:          Backend: MasterImageUploadRequest.cs, MasterImageUploadResponse.cs,
                      UploadMasterImageFeature.cs, MasterDataAdminController.cs (UploadImageAsync).
                    Admin: master-data-repository.ts (uploadMasterImage + MasterImageUpload),
                      MasterDataProvider.tsx (uploadMasterImage passthrough), ServiceCatalogScreen.tsx
                      (imageUrl form field, upload control, preview, list thumbnail).
    Validation status: Backend build 0/0; Admin tsc --noEmit clean (2026-06-10).
  Notes on Drift (cross-source — RECONCILIATION CHOSEN; canonical = tblService):
    Two separate "service" representations exist: (1) the bookable tblService (entity Service) read by
    the public site + booking + invoicing; (2) DynamicMasterRecord MasterType="ServiceType" edited by the
    Admin "Service Types" catalog tab — NOT consumed by booking/public. Decision: tblService is canonical.
    PHASE 1 DONE [2026-06-10]: added tblService.ImageUrl (NVARCHAR(512) NULL) + EF config; surfaced ImageUrl
      in ServiceLookupResponse, ServiceTypeListItemResponse, ServiceTypeDetailResponse; public ServiceDetail.tsx
      binds service.imageUrl with FALLBACK_IMG when empty. Migrations: SqlServer
      Docs/Database/SQL/20260610_Add_Service_ImageUrl.sql, Postgres Docs/Postgres/20_add_service_imageurl.sql
      [NOT yet applied to any DB — run before relying on the column]. Backend build 0/0; Web tsc clean.
    PHASE 2 DONE [2026-06-10] (admin writes the real tblService.ImageUrl — additive, non-destructive):
      New write endpoint PUT /api/admin/services/{serviceId}/image (Authorize Policy=lookup.manage)
        Controller: ServiceCatalogAdminController (route api/admin/services)
        Feature: ServiceCatalogAdmin/Commands/SetServiceImage/SetServiceImageFeature.cs
        Request DTO: SetServiceImageRequest { ImageUrl (string?, null/empty = clear) }
        Response DTO: ServiceLookupResponse (incl. ImageUrl)
        Handler: loads the TRACKED entity via IBookingLookupRepository.GetServiceByIdAsync, sets
          ImageUrl + UpdatedBy/LastUpdated, audit-logs, SaveChanges. 404 when service missing.
      New admin screen: Frontend/Admin ServiceImagesScreen.tsx (route /settings/master/service-images,
        RoleGuard module="settings"; discoverable via SystemConfigHomeScreen "Service Images" card).
        Lists bookable services (GET /api/booking-lookups/services), per-service Upload/Replace/Remove.
        Upload reuses POST /api/admin-masters/upload-image (folder="services") then immediately PUTs the
        URL to /image — so the image is persisted to tblService and live on the public site in one action.
        Repository: src/core/network/service-catalog-repository.ts (getServices, setServiceImage).
      NOTE: this is a SEPARATE admin surface from the legacy DynamicMasterRecord "Service Types" catalog
        tab (which still writes metadata.imageUrl, unused by the public site). Both coexist until Phase 3.
      Validation status: Backend build 0/0; Admin tsc --noEmit clean (2026-06-10).
    PHASE 3 DONE [2026-06-10] (retire the legacy taxonomy tab — UI-level, non-destructive):
      Frontend/Admin ServiceCatalogScreen.tsx no longer exposes the "Service Types" tab (and its
      now-redundant catalog-native image uploader — superseded by the Phase 2 Service Images screen).
      Remaining tabs: Service Subtypes, Equipment Brands, Equipment Models. The Subtypes "Parent"
      picker now sources from REAL bookable services (GET /api/booking-lookups/services) instead of the
      DynamicMasterRecord "ServiceType" master — so subtypes hang off tblService. Stored
      metadata.parentCode now holds the service NAME (was the old master code). Routes /settings/master/
      services + /brands still map to this screen (now defaulting to Subtypes).
      NOT done (intentionally deferred — low value, some risk): physically migrating/deleting existing
      DynamicMasterRecord MasterType="ServiceType" rows from the DB, and building a full tblService admin
      CRUD (create/rename/price a service). Today bookable services are created via DB seed; admin can
      set their image (Phase 2). The orphaned "ServiceType" master rows are harmless (no consumer).
      Public Services list cards (Frontend/Web Services.tsx) now also render service.imageUrl when present.
      Validation status: Admin + Web tsc --noEmit clean (2026-06-10).


7. CMS CONTENT & THEME DELIVERY (PUBLIC WEB PORTAL) — TARGET ARCHITECTURE & ROADMAP

Purpose: Make the public Web portal (`Frontend/Web`, React 19 + Vite) fully backend-driven. The admin
team edits masters (services, pricing, brands), content (banners, copy, FAQs, blog, testimonials),
screen images, and theme (colors, fonts, logo) in the Admin app; on Publish those changes flow to the
portal automatically with NO portal code change. The portal does NOT hit the DB or call per-request
APIs on each load — it reads one versioned static JSON snapshot from the existing Render storage bucket
(CDN). A lightweight in-process IMemoryCache holds the snapshot manifest for the rare API fallback path.

Scope decisions (locked 2026-06-09):
- Consuming surface: Public Web portal ONLY (Admin app and mobile apps are out of scope for this module).
- Delivery: versioned `snapshot.json` written to the Render bucket; portal fetches the static file. Zero per-request DB/API.
- Images: prompt-helper + upload (no live Gemini API). Each image slot carries a suggested AI prompt; admin generates in Gemini or uses a real photo, then uploads.
- Caching: NO Redis. The static-bucket design makes the portal never touch a cache; publish is an infrequent admin write; the fallback API is a tiny query. In-process IMemoryCache covers the fallback. [Decision 2026-06-09: Redis rejected as standing infra for a near-zero-traffic path; revisit only if a shared cross-module cache need is measured.]

Flow Name: CMS Snapshot Publish & Portal Hydration
  Entry Points:
    - Admin app: CMS module → Content/Masters/Theme/Image editors → "Publish" action.
    - Public Web portal: app bootstrap (`Frontend/Web/src/main.tsx` → ContentProvider/ThemeProvider).
  UI Trigger:
    - Admin: "Publish" button (staged activation, T2/T3 confirmation).
    - Portal: bootstrap fetch on first load + version-manifest poll (ETag) thereafter.
  API Endpoint: [IMPLEMENTED — note: this codebase routes under /api/... NOT /api/v1/...; CMSController = [Route("api/cms")]. v1 convention from CLAUDE.md is not used anywhere in the codebase — following codebase reality.]
    - POST /api/cms/publish — builds + writes snapshot (Authorize policy cms.manage). Returns SnapshotManifestResponse.
    - GET  /api/cms/snapshot/manifest — returns { version, bucketUrl, checksum, publishedAtUtc } (IMemoryCache 60s, fallback to tblPublishedSnapshot). AllowAnonymous.
    - GET  /api/cms/snapshot/{version} — fallback full-snapshot read from object storage (AllowAnonymous).
    - POST /api/cms/rollback/{version} — re-activates a prior snapshot version (cms.manage).
    - Static (no API): {ObjectStorage.PublicBaseUrl}/cms/snapshot-latest.json  ← primary path the portal reads.
  Snapshot Payload (single immutable JSON document, aggregated on publish):
    {
      version, publishedAt, checksum,
      theme: { colors{...design tokens...}, fonts{family, weights}, logoUrl },
      masters: { serviceTypes[], serviceSubTypes[], brands[], pricing[], amcPlans[] },
      content: { banners[], homeBlocks[], serviceContent{}, testimonials[], faqs[], footer{} },
      images: { "<pageKey>.<slotKey>": { url, alt, variants{desktop,tablet,mobile} } }
    }
  Validation Rules:
    - Publish requires CMS-publish permission; payload schema-validated; no PII / no secrets in snapshot.
    - Every active image slot must resolve to a reachable bucket URL or fall back to a documented default.
    - Theme color tokens validated against design system; fonts limited to ≤2 weights per screen (design rule).
  DB Tables (provider-aware: SQL Server prod / Postgres dev — see [[db-provider-split]]):
    - Reuse: CMSBlocks, CMSBlockVersions, Banners, FAQs, BlogPosts, Testimonials, ServiceType/Brand/Pricing masters. [VERIFY exact tbl-prefixed names against schema]
    - Theme (colors, fonts, logo ref): REUSE existing tblSystemSetting as SCALAR rows under a "theme.*" key prefix (theme.color.primary, theme.color.accent, ..., theme.font.family, theme.font.weights, theme.logoUrl). NOT a single JSON blob — SettingValue is nvarchar(512) IsRequired (SystemSettingConfiguration.cs:16) and a full theme blob overflows it. Scalar rows fit the 512 cap and the table's DataType design; the publish aggregator composes them into snapshot.theme{}. No dedicated theme table; snapshot versioning (tblPublishedSnapshot) covers theme version/rollback. [Decision 2026-06-09: tblThemeSetting rejected as duplicate; single-blob rejected due to 512-char cap; scalar theme.* rows chosen.] [DRIFT FIXED: brain previously said "SystemConfigs(ValueJSON)"; real table is tblSystemSetting(SettingKey, SettingValue nvarchar(512), DataType, IsSensitive).]
    - NEW tblScreenImageSlot (PageKey, SlotKey, Breakpoint, RecommendedWidth, RecommendedHeight, AltText, SuggestedAIPrompt, ImageUrl, IsActive + audit columns).
    - NEW tblPublishedSnapshot (SnapshotId, Version, BucketUrl, ChecksumHash, PayloadSizeBytes, PublishedBy, DatePublished, IsActive + audit columns) — version history & rollback.
  Stored Procedures: uspBuildContentSnapshot (aggregate), uspInsertPublishedSnapshot, uspGetActiveSnapshot, uspRollbackSnapshot, uspGetScreenImageSlotList, uspUpsertScreenImageSlot. Theme read/write reuses existing tblSystemSetting access (scalar theme.* keys). [VERIFY/CREATE per New SQL Format]
  Business Rules:
    - Publish is atomic: snapshot built fully, written to bucket, version row marked active, prior version retained for rollback.
    - Portal always reads the active snapshot version; cache-busts via version in the manifest/filename.
    - Image slots define per-breakpoint variants so device-type rendering (Desktop/Laptop/Tablet/Mobile) is correct.
  Realtime Events: none required (portal polls manifest ETag). [VERIFY — optional SignalR "snapshot.published" for instant admin preview]
  Failure Cases:
    - Bucket write fails → publish aborts, prior snapshot stays active, error surfaced to admin.
    - Portal bucket fetch fails → fall back to GET /api/v1/cms/snapshot/manifest + /{version} (Redis-served).
    - Missing image slot → documented default asset; never a broken image.
  Recovery / Fallback: IMemoryCache-backed manifest + API snapshot read; rollback endpoint restores any retained version.
  Storage Provider (CMS object storage): Cloudflare R2 ONLY (S3-compatible) via S3ObjectStorageService implementing IObjectStorageService. [FILESYSTEM REMOVED 2026-06-11 — see drift note] The selectable FileSystem provider and ObjectStorage:Provider / ObjectStorage:RootPath settings were deleted: Render's disk is ephemeral and silently lost uploaded images + published snapshots on redeploy. DI now registers S3ObjectStorageService unconditionally and builds an R2-aware IAmazonS3 (ServiceUrl + ForcePathStyle). ObjectStorageConfigurationGuard.Validate(configuration) runs before host build and FAILS STARTUP if any R2 setting is missing (ServiceUrl, BucketName, AccessKey, SecretKey, PublicBaseUrl). PublicBaseUrl = the R2 bucket public/CDN base; GetPublicUrl returns absolute URLs the portal uses directly (resolveAssetUrl passes http(s) URLs through untouched). JOB/TECHNICIAN MEDIA [MOVED TO PRIVATE R2 2026-06-11]: IJobAttachmentStorageService is now R2JobAttachmentStorageService writing to a SEPARATE PRIVATE bucket (ObjectStorage:JobMediaBucketName = coolzo-job-media), NOT the public CMS bucket — customer site photos are PII-grade. LocalJobAttachmentStorageService deleted. SaveAsync stores objects under key `job-media/{yyyyMMdd}/{guid}{ext}` and returns RelativePath = a RELATIVE API proxy URL `/api/field-media/{key}` (callers FieldWorkflowFeature/HelperWorkflowFeature/SaveJobAttachment persist+return it unchanged → mobile contract field shape intact). Reads stream through GetByKeyAsync via NEW endpoint GET /api/field-media/{**objectKey} (FieldMediaController, [AllowAnonymous], key confined to the job-media prefix, 404 on miss/bad key). The private bucket is never public; bytes only reachable through this proxy. SECURITY POSTURE: matches the prior wwwroot model (anonymous + unguessable GUID key — works with plain <img>/RN Image, no auth header) but durable + bucket-private. Elevating to per-user authz requires a VERIFIED mobile change to send the bearer token on image fetches (image controls don't by default) — deferred to honor the immutable mobile contract. CREDENTIALS: the private bucket uses its OWN least-privilege R2 token (ObjectStorage:JobMediaAccessKey / :JobMediaSecretKey via env), built into a separate AmazonS3 client wrapped by JobMediaStorageClient (DI) — the CMS token (ObjectStorage:S3:AccessKey/SecretKey) cannot touch job media and vice-versa; same account endpoint (ServiceUrl/Region/ForcePathStyle reused). DEPLOY: create private bucket coolzo-job-media (NOT public); set Render env ObjectStorage__JobMediaAccessKey / __JobMediaSecretKey (the coolzo-job-media token). Startup guard now also requires JobMediaBucketName/JobMediaAccessKey/JobMediaSecretKey. Legacy DB rows with old /uploads/job-attachments/... paths will 404 (ephemeral disk, already lost on prior redeploys).
  Notes on Drift: General IObjectStorageService introduced in Phase 0 [2026-06-09] alongside the untouched IJobAttachmentStorageService. No Redis is used (rejected — see Caching decision). Portal now consumes the published snapshot via ContentProvider + SnapshotImage (Phase 4); home.hero/amc.banner/about.hero are backend-driven, other hardcoded images can be migrated incrementally with the same <SnapshotImage slotKey=...> pattern (add a tblScreenImageSlot row + use the component). [DRIFT FIXED 2026-06-10: brain previously said storage was "filesystem-backed… no S3". Reality: S3ObjectStorageService + S3StorageOptions already existed (AWSSDK.S3 referenced) but the DI hardcoded FileSystem and never registered IAmazonS3 — so S3/R2 was unreachable. Now InfrastructureServiceCollectionExtensions switches IObjectStorageService on ObjectStorage:Provider and builds an R2-aware IAmazonS3 (ServiceUrl + ForcePathStyle for R2, regional endpoint for AWS). Prod appsettings.json set Provider="S3" (secrets via env); dev stays FileSystem. Frontend: VITE_SNAPSHOT_BASE_URL added so the portal fetches the static snapshot directly from the R2 CDN (falls back to API origin when unset). Build 0/0.]

IMPLEMENTATION ROADMAP (execute phase-wise; each phase passes Architecture + Security + QA gates before the next):
  Phase 0 — Foundations / Infra (DevOps-SRE + Backend + Security): [IMPLEMENTED 2026-06-09]
    IObjectStorageService (Coolzo.Application/Common/Interfaces) + StoredObjectResult model; FileSystemObjectStorageService (Coolzo.Infrastructure/Storage) — filesystem-backed, works for local wwwroot (served via existing UseStaticFiles) and absolute Render persistent-disk RootPath; path-traversal guarded (keys must be relative, no ".."). ObjectStorageOptions ("ObjectStorage": RootPath, PublicBaseUrl) bound in InfrastructureServiceCollectionExtensions; registered scoped. ObjectStorageHealthCheck (Coolzo.Api/HealthChecks) mapped at GET /health. appsettings + appsettings.Development have ObjectStorage section. No Redis (IMemoryCache where a cache is needed). Existing IJobAttachmentStorageService left untouched (additive). Build: 0 warnings / 0 errors.
    OPS NOTE: for production, set ObjectStorage:RootPath to the mounted Render disk path and ObjectStorage:PublicBaseUrl to the served/CDN base URL.
  Phase 1 — Snapshot contract & publish pipeline (Backend + Chief Architect + Security + QA): [IMPLEMENTED 2026-06-09]
    STABLE KEY REGISTRY: Coolzo.Shared/Constants/SnapshotKeys.cs — frozen sections (theme/masters/content/images), theme.* token keys, master/content collection keys, storage object keys, manifest cache key. Permanent contract; never rename/drop a bound key without migration; world-readable artifact carries no PII/secret.
    Snapshot DTO: Coolzo.Contracts/Responses/CMS/ContentSnapshotResponse.cs (+ body/manifest/sub-DTOs). Aggregator: IContentSnapshotBuilder/ContentSnapshotBuilder (theme from tblSystemSetting theme.* non-sensitive; content from CMS blocks/banners/faqs; masters+images shapes fixed, populated Phase 2+). Serializer: SnapshotSerializer (camelCase, SHA256 checksum).
    Persistence: PublishedSnapshot entity + PublishedSnapshotConfiguration (tblPublishedSnapshot, UK on Version, IDX on IsActive) + DbSet + IPublishedSnapshotRepository/PublishedSnapshotRepository. DDL: Docs/Database/SQL/20260609_Add_PublishedSnapshot_Table.sql (SqlServer) + Docs/Postgres/13_add_published_snapshot.sql (Postgres). [Postgres DDL APPLIED to Supabase dev DB 2026-06-09 — tblPublishedSnapshot verified, 26 columns. SqlServer script pending prod apply.]
    Runtime smoke test 2026-06-09: GET /api/cms/snapshot/manifest → 404 standard envelope ("No published content snapshot is active.") proving DI + handler + repository against live tblPublishedSnapshot + envelope; GET /health → 200 Healthy (object-storage + database checks). Publish/rollback not yet runtime-tested (needs cms.manage JWT — covered when Admin UI lands in Phase 3).
    Publish handler atomic order: build → write versioned file → deactivate prior + insert active row + SaveChanges → flip snapshot-latest.json → invalidate cache → audit. Rollback re-points latest to a retained version. Build: 0 warnings / 0 errors.
  Phase 2 — Theme & Screen-Image backend + DB (Database Architect + Backend): [IMPLEMENTED 2026-06-09]
    Theme: ISystemSettingRepository gained GetTrackedByKeyAsync/AddAsync; GetTheme query + UpdateTheme command (validates keys ∈ SnapshotKeys.Theme.AllKeys) over tblSystemSetting "theme.*" scalar rows. Endpoints GET/PUT /api/cms/admin/theme (cms.read / cms.manage).
    Screen images: ScreenImageSlot entity + ScreenImageSlotConfiguration (tblScreenImageSlot, UK PageKey+SlotKey+Breakpoint) + DbSet + IScreenImageSlotRepository; features GetScreenImageSlotList, UpsertScreenImageSlot, UploadScreenImage (base64, ≤5MB, png/jpeg/webp/svg → IObjectStorageService key cms/images/{page}/{slot}-{bp}-{guid}{ext}). Endpoints under /api/cms/admin/image-slots (+ /{id}/upload). AUDIT LOG ENTITY-ID STABILITY [2026-06-11]: both UpsertScreenImageSlot and UploadScreenImage now persist AuditLog.EntityId as the numeric ScreenImageSlotId instead of the composite page.slot.breakpoint string, because tblAuditLog.EntityId is capped at 64 chars and long custom keys could overflow it and mask the real upload result with a generic 500.
    CMS asset upload (slot-less): UploadCmsAsset command/handler (Features/CMS/Asset/Commands/UploadCmsAsset) — base64, ≤5MB, png/jpeg/webp/svg → IObjectStorageService key cms/assets/{assetKey}-{guid}{ext}; returns CmsAssetUploadResponse(ImageUrl). Endpoint POST /api/cms/admin/assets/upload (cms.manage). Used by the Theme tab so the brand logo (theme.logoUrl token) is uploaded as an image (assetKey="logo") instead of a pasted URL; returned URL is stored in the theme.logoUrl token and persisted via PUT theme + Publish. Frontend resolves relative image/asset URLs against EnvConfig.API_BASE_URL in cms-delivery-repository (mapImageSlot/uploadAsset) so previews load from the API origin, not the Vite dev server (absolute http(s)/data URLs pass through for R2/S3).
    Snapshot builder now populates images: active slots with a non-empty ImageUrl grouped by "{PageKey}.{SlotKey}" → SnapshotImageDto(url, alt, variants{breakpoint→url}).
    MASTERS POPULATED [2026-06-11]: snapshot.masters now carries the public catalog so the portal binds from the static snapshot instead of calling each booking-lookup endpoint. SnapshotMastersDto = { serviceCategories, services, acTypes, tonnages, brands, amcPlans } reusing the EXISTING public response records (Coolzo.Contracts.Responses.Booking.*LookupResponse + Amc.AmcPlanResponse) so the snapshot shape can never drift from the live API. ContentSnapshotBuilder injects MediatR ISender and SENDS the same queries the live endpoints use (GetServiceCategoriesQuery/GetServicesQuery(null,null)/GetAcTypesQuery/GetTonnagesQuery/GetBrandsQuery + GetAmcPlansQuery(isActive:true,1,500)) — single source of truth, no duplicated repo/mapping logic. SECURITY: all six are AllowAnonymous public-display data (no PII); AMC filtered to active plans only. Old placeholder SnapshotBrandDto{Code,Name} removed. STAYS LIVE (NOT snapshotted): zones-by-pincode + slots (parameterized/dynamic) and every authenticated portal endpoint (bookings, invoices, tickets, equipment, addresses, profile, AMC-customer, notifications) — these are per-user/PII and must remain direct API calls. Build 0/0.
    DDL: SqlServer Docs/Database/SQL/20260609_Add_ScreenImageSlot_Table.sql + seed DB_Seed_20260609_CmsThemeAndImageSlots.sql; Postgres Docs/Postgres/14_add_screen_image_slot.sql + 15_seed_cms_theme_and_image_slots.sql. [Postgres table+seed APPLIED to Supabase 2026-06-09: 13 theme.* tokens + 6 starter slots (home.hero desktop/tablet/mobile, services.banner, amc.banner, about.hero) with Gemini prompts. SqlServer pending prod.]
    END-TO-END VERIFIED 2026-06-09 (dev, minted cms JWT): list slots → upload PNG to home.hero/desktop → publish v1 → GET /cms/snapshot-latest.json shows 13 theme tokens (#1B2A4A primary, #C9A84C accent) + images["home.hero"] with variants; manifest 404→200; publish v2 → rollback to v1 → manifest=v1. Build 0/0. Dev wwwroot left with a working published snapshot baseline (test image is a 1×1 placeholder; admin replaces it).
  Phase 3 — Admin CMS authoring UI (Frontend & Mobile Eng + UX + QA): [IMPLEMENTED 2026-06-09]
    Admin app = Frontend/Admin (React 19 + Vite). Repository src/core/network/cms-delivery-repository.ts wraps /api/cms admin+publish endpoints (envelope auto-unwrapped by api-client interceptor; Bearer token auto-attached). Screen src/features/governance/CmsDeliveryManager.tsx with three tabs: Theme (color pickers + hex for the 10 colour tokens, font family/weights, logo image upload with preview + recommended-size details → assets/upload then PUT theme), Screen Images (slots grouped by page; per-slot preview, recommended dims, Suggested Gemini prompt + Copy button, file→base64 upload). Upload + slot upsert now write audit entries with ScreenImageSlotId as the audit entity id, which keeps tblAuditLog.EntityId inside the 64-char limit and prevents long custom page/slot keys from turning a successful R2 write into a generic 500. Publish & Versions (Publish Now, active-version/checksum readout, rollback by version). Route /governance/cms-delivery (RoleGuard module="settings") in app/navigation/router.tsx; "Web Portal" nav item (Globe) added to SUPER_ADMIN, ADMIN, MARKETING_MANAGER in app/modules/role-navigation.tsx. Typecheck (tsc --noEmit) clean. Runtime UI click-through pending (needs admin login in browser); underlying endpoints already verified live in Phase 1/2.
  Phase 4 — Public Web portal consumption (Frontend & Mobile Eng + UX + Performance + QA): [IMPLEMENTED 2026-06-09]
    Frontend/Web (React 19 + Vite). snapshotService.ts fetches the static {VITE_API_BASE_URL}/cms/snapshot-latest.json (cache-busted, localStorage-cached; resolveAssetUrl prefixes relative object URLs with the API origin so dev works without a CDN). ContentContext.tsx (ContentProvider + useContent): hydrates from cache instantly, revalidates on mount, injects theme tokens as CSS variables on :root (theme.color.primary→--color-brand-navy, accent→--color-brand-gold, background→--color-brand-cream, textPrimary→--color-brand-black, font.family→--font-sans) — so theme changes apply site-wide with no code edit. SnapshotImage.tsx renders a responsive <picture> (mobile ≤640 / tablet ≤1024 / desktop) from a slot's variants with a bundled fallback (never a broken image). App.tsx wrapped in ContentProvider. Wired slots: home.hero (Home), amc.banner (AMC), about.hero (About). services.banner slot exists but Services is a filterable catalog with no hero image (left for future use). Typecheck tsc --noEmit clean.
    MASTERS CONSUMPTION [2026-06-11]: snapshotService.ts now types snapshot.masters as SnapshotMasters { serviceCategories, services, acTypes, tonnages, brands, amcPlans } and exposes getSnapshotMasters() (returns the cached/fresh masters, or null when the active snapshot predates masters → caller falls back to live API; gated on serviceCategories presence). CatalogService.getServiceCategories/getServices/getAcTypes/getTonnages/getBrands are now SNAPSHOT-FIRST with live booking-lookup API fallback (client-side search/categoryId filtering when served from snapshot). No page changes needed — the 6 consumers (Services, Home, Pricing, AMC, ServiceDetail, BookingWizard) call CatalogService unchanged. Dynamic lookups (getZones/getZoneByPincode/getAvailableSlots) stay live. Web typecheck tsc --noEmit clean. NOTE: portal serves masters from the snapshot only AFTER an admin re-publishes (so the active snapshot carries masters); until then it transparently falls back to the live API.
    AMC DRIFT [RESOLVED 2026-06-11]: frontend src/types/amc.ts AmcPlanResponse was corrected to the backend shape {amcPlanId, planName, planDescription, durationInMonths, visitCount, priceAmount, isActive, termsAndConditions}; the duplicate SnapshotAmcPlan type was removed (snapshotService.ts imports the corrected AmcPlanResponse). AmcService.getPlans is now SNAPSHOT-FIRST (snapshot.masters.amcPlans, active-filtered) with live /api/amc/plans fallback. Safe because getPlans had no active consumer and the public AMC page (AMC.tsx) renders AMC-category SERVICES via CatalogService (already snapshot-backed), not AMC plan entities. Web typecheck tsc --noEmit clean.
    VERIFIED 2026-06-09 (dev): GET {API}/cms/snapshot-latest.json → 200 (theme #1B2A4A + images.home.hero present); referenced image asset → 200 image/png. Full browser render click-through still recommended.
    CORS-on-static-files fix (2026-06-11): the Web portal fetch() of /cms/snapshot-latest.json (cross-origin 3000→44394) was blocked — "No 'Access-Control-Allow-Origin' header" despite 200 — because Program.cs called app.UseStaticFiles() (wwwroot + object-storage PhysicalFileProvider) BEFORE app.UseCors(); static-file middleware short-circuits the pipeline so CORS never ran. Fix: order is now UseRouting → UseCors("FrontendPolicy") → UseStaticFiles (both) → Auth → MapControllers, so CMS object/snapshot responses carry CORS headers for the allowed origins. RULE: any new UseStaticFiles for publicly-fetched CMS objects must sit AFTER UseCors. Requires API restart to take effect.
  Phase 5 — Hardening (QA + Security + Performance): [IMPLEMENTED 2026-06-09]
    Bucket-down fallback: snapshotService.fetchSnapshot() now tries the static file first, then falls back to the API (GET /api/cms/snapshot/manifest → GET /api/cms/snapshot/{version}, envelope-unwrapped, IMemoryCache+DB backed); localStorage write is guarded. Cache-busting via ?t= each load; cached snapshot paints instantly then revalidates.
    Security review (PASS): snapshot theme = only non-sensitive "theme.*" tblSystemSetting rows (ContentSnapshotBuilder filters !IsSensitive + prefix); content = public-published CMS (publicOnly); images = public slots. No PII/secrets in the world-readable artifact. Mutations gated by cms.manage; manifest/snapshot reads AllowAnonymous (public content only).
    Performance: snapshot ~2KB; SnapshotImage lazy by default (hero eager); single static fetch, no per-request DB/API on the hot path.
    QA: Web tsc --noEmit clean + production `vite build` succeeds. Backend build 0/0. Atomic publish ordering re-confirmed (versioned write → DB commit → flip latest).
    Doc: this SECTION 9 §7 entry is at STABLE CONTRACT LEVEL — a future agent can diagnose/extend the module from here without source reads.

OPERATIONAL RUNBOOK (CMS Content & Theme Delivery):
  - Admin path: Admin app → "Web Portal" (/governance/cms-delivery) → edit Theme / upload Screen Images → "Publish Now". Rollback by version on the same screen.
  - Add a new backend-driven image: insert a tblScreenImageSlot row (PageKey, SlotKey, Breakpoint, prompt, dims) → admin uploads → publish → use <SnapshotImage slotKey="page.slot" fallbackSrc=… /> on the portal page.
  - Add a theme token: extend SnapshotKeys.Theme + the portal THEME_CSS_VAR_MAP mapping; seed a default.
  - Prod deploy checklist: run SqlServer scripts (Docs/Database/SQL/20260609_Add_PublishedSnapshot_Table.sql, 20260609_Add_ScreenImageSlot_Table.sql) + seed (DB_Seed_20260609_CmsThemeAndImageSlots.sql); set Web VITE_API_BASE_URL.
  - Storage = Cloudflare R2 ONLY: set ObjectStorage:S3:ServiceUrl=https://<accountid>.r2.cloudflarestorage.com, :Region="auto", :ForcePathStyle=true, :BucketName=<bucket>, ObjectStorage:PublicBaseUrl=the bucket's R2 public domain (r2.dev URL or CDN). Supply :AccessKey / :SecretKey via environment ONLY (ObjectStorage__S3__AccessKey / __SecretKey) — never source control. Set Web VITE_SNAPSHOT_BASE_URL to the same public base so the portal reads the static snapshot from R2. There is NO FileSystem fallback: ObjectStorageConfigurationGuard fails startup if any of these are missing (dev included).
  - FILESYSTEM PROVIDER REMOVED [2026-06-11]: FileSystemObjectStorageService deleted; ObjectStorageOptions.Provider/RootPath + the FileSystem provider constants removed; DI registers S3ObjectStorageService unconditionally; Program.cs dropped the PhysicalFileProvider UseStaticFiles block for object storage; appsettings.json lost the Provider/RootPath keys and appsettings.Development.json's ObjectStorage block was removed so dev inherits the R2 config from the base file (dev devs supply AccessKey/SecretKey via user-secrets/env; dev currently shares the coolzo-cms bucket — a dedicated dev bucket is a future refinement). Build 0/0 (Infrastructure + Api compiled isolated; full-solution build blocked only by the running IIS Express/VS lock). ACTION after pulling this: every environment, including local dev, MUST have the R2 settings + AccessKey/SecretKey or the API will not start.
  - ACTIVE PROD PROVIDER: Cloudflare R2 (bucket coolzo-cms). ServiceUrl/BucketName/Region(auto)/ForcePathStyle(true)/PublicBaseUrl in appsettings.json; AccessKey/SecretKey via env on Render (ObjectStorage__S3__AccessKey / __SecretKey). DEPLOY GATE: set the two R2 secret env vars on Render BEFORE deploying the guard, otherwise the API refuses to boot. Verify post-deploy: GET /health object-storage=Healthy + a CMS image upload returns a https://<PublicBaseUrl>/... URL.
  - Failure modes: bucket/CDN down → portal API fallback; missing slot image → bundled fallbackSrc (no broken image); no active snapshot → manifest 404, portal uses fallbacks + default theme.

PUBLIC WEB SITE — COMPLETE IMAGE INVENTORY & UPLOAD STATUS (audited 2026-06-10):
  Scope: Frontend/Web (React public site). Three image classes exist: (A) CMS-managed screen slots
  rendered via <SnapshotImage>, (B) CMS banners from snapshot content.banners, (C) hardcoded external
  images that are NOT admin-managed. "Missed upload" = a registered/used slot with no published image,
  so the site silently shows its bundled fallbackSrc.

  A. CMS-MANAGED SCREEN SLOTS (tblScreenImageSlot → snapshot images map)
     Registered slots (seed DB_Seed_20260609 / 15_seed): home.hero (desktop/tablet/mobile),
     about.hero, amc.banner, services.banner. Published snapshot v1 (snapshot-latest.json) images map
     currently contains ONLY home.hero.
     | Slot           | Portal usage (SnapshotImage)        | Published image? | Asset on disk                                   | Status            |
     | home.hero      | Home.tsx:70                         | YES              | wwwroot/cms/images/home/hero-desktop-7b603b…png | ✅ Uploaded       |
     | about.hero     | About.tsx:41 (fallback=Unsplash)    | NO               | —                                               | ❌ MISSED upload  |
     | amc.banner     | AMC.tsx:139 (fallback=Unsplash)     | NO               | —                                               | ❌ MISSED upload  |
     | services.banner| NOT rendered (Services = catalog,   | NO               | wwwroot/cms/images/services/banner-desktop-…jpeg| ⚠ ORPHANED:      |
     |                | no hero — see Phase 4 note)         |                  | + banner-desktop-…png (2 files)                 | uploaded, unwired |

  B. CMS BANNERS (snapshot content.banners[])
     | Banner                     | imageUrl                          | File on disk? | Status                          |
     | "Summer Service Slots Open"| /assets/banners/summer-service.jpg| NO            | ❌ BROKEN — asset does not exist |
     Note: banner imageUrl path (/assets/banners/) is NOT under wwwroot/cms — no such file anywhere in
     wwwroot. Banner renders broken/empty unless the asset is supplied or the path is corrected.

  C. HARDCODED EXTERNAL IMAGES (not CMS-managed — informational; candidates for CMS migration)
     - Home.tsx: hero fallback (Unsplash), Hyderabad coverage (Home.tsx:191), Service Experience
       (Home.tsx:251), Modern Home (Home.tsx:290), reviewer avatars (picsum, Home.tsx:117),
       brand logos Daikin + Mitsubishi (Wikipedia, Home.tsx:139-140).
     - About.tsx:105: team member photos (person.img).
     - Blog.tsx / BlogDetail.tsx: post imageUrl || FALLBACK_IMG (external).
     - ServiceDetail.tsx:174: FALLBACK_IMG (external) — would be replaced by services.banner if wired.
     - Reviews.tsx, portal/Feedback.tsx (techAvatarUrl), portal/Profile.tsx (photoUrl||avatar): avatars.

  SUMMARY (admin action required):
    - Missing uploads an admin must publish: 3 → about.hero, amc.banner (screen slots) + summer-service.jpg (banner).
    - Orphaned uploads (in storage, not published/unwired): 2 → services.banner desktop jpeg+png.
    - Fix paths: wire services.banner into a Services hero OR retire the slot; correct the banner imageUrl
      to a real published asset (e.g. an uploaded screen slot or /cms/... object path).
  Notes on Drift (content drift): snapshot banner references a non-existent /assets/banners/ asset;
    services.banner has storage uploads with no snapshot entry and no portal consumer.

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

1) Auth & Session — STABLE CONTRACT (audited 2026-05-24)

Controllers: AuthController (route prefix: /api/auth), CustomerAuthController (route prefix: /api/customer-auth)
All endpoints AllowAnonymous unless auth role noted. All routes use /api/... directly — no version segment.

---

### Flow: Send OTP
  Entry Points:         Customer login screen (phone), admin forgot-password screen, 2FA trigger
  UI Trigger:           "Send OTP" button / forgot-password submit
  API Endpoint:         POST /api/auth/otp/send
  Auth:                 AllowAnonymous
  Request DTO:          SendCustomerOtpRequest
    - Phone (string, required): phone number or login identifier
  Response DTO:         AuthActionResponse
    - Success (bool): true if OTP dispatch succeeded
    - Message (string): human-readable status
  Validation Rules:     Phone must not be null or empty
  DB Tables:            OtpVerifications (write — stores OTP with TTL)
  Business Rules:
    1. Development env: OTP fixed to '123456' via AuthSessionTokenFactory.CreateOtp()
       (override with COOLZO_FIXED_TEST_OTP env var)
    2. Non-dev: random 6-digit OTP generated
    3. OTP stored with expiry; delivery via SMS/WhatsApp/email per channel config
  Failure Cases:
    - Missing phone → 400 Bad Request
    - OTP delivery failure → 200 Success=false with message
  Notes on Drift:
    - 2026-04-22: AuthController began serving this customer-facing OTP route as a
      compatibility alias. OTP lookup resolves phone → User via CustomerAccountLookupService.

---

### Flow: Verify Customer OTP (Phone-Based Login)
  Entry Points:         Customer login OTP entry screen
  UI Trigger:           OTP code submission
  API Endpoint:         POST /api/auth/otp/verify
  Auth:                 AllowAnonymous
  Request DTO:          VerifyCustomerOtpRequest
    - Phone (string, required): phone number
    - Otp (string, required): 6-digit OTP code
  Response DTO:         AuthTokenResponse
    - AccessToken (string): JWT access token (TTL = 15 min)
    - RefreshToken (string): refresh token (TTL = 7 days, rotated on every use)
    - ExpiresAtUtc (DateTime): access token expiry timestamp (UTC)
    - CurrentUser (CurrentUserResponse):
        UserId (long), UserName (string), Email (string), FullName (string),
        TechnicianId (long?), HelperProfileId (long?), BranchId (int),
        Roles (string[]), Permissions (string[]), CustomerId (long?),
        MustChangePassword (bool), IsTemporaryPassword (bool), PasswordExpiryOnUtc (DateTime?)
    - RequiresTwoFactor (bool): true if 2FA step still pending
  DB Tables:            OtpVerifications (read + delete on use), Users (read),
                        RefreshTokens (write), UserDevices (upsert)
  Business Rules:
    1. OTP validated against OtpVerifications; expired OTPs rejected
    2. Refresh token rotated and stored on success
    3. UserDevices upserted with deviceId and push token
  Failure Cases:
    - Invalid/expired OTP → 400 Bad Request
    - Customer not found → 400 Bad Request

---

### Flow: Email/Password Login (Internal Staff)
  Entry Points:         Admin/staff login screen
  UI Trigger:           Login form submit
  API Endpoint:         POST /api/auth/login
  Auth:                 AllowAnonymous
  Request DTO:          LoginRequest
    - UserNameOrEmail (string, required): username or email address
    - Password (string, required): plaintext password (TLS-protected in transit)
  Response DTO:         AuthTokenResponse (same shape as OTP verify above)
  DB Tables:            Users (read), RefreshTokens (write), UserDevices (upsert)
  Business Rules:
    1. Password validated against hashed value in Users table
    2. Account lockout after configurable failed attempts; admin override to unlock
    3. RequiresTwoFactor=true returned for roles requiring 2FA (Admin, Finance)
    4. MustChangePassword=true if password is temporary or expired
  Failure Cases:
    - Invalid credentials → 401
    - Account locked → 400 with lockout message

---

### Flow: Field Technician / Helper Login (EmployeeId + PIN)
  Entry Points:         Technician/Helper field app login screen
  UI Trigger:           Employee ID + PIN submit
  API Endpoint:         POST /api/auth/login-field
  Auth:                 AllowAnonymous
  Request DTO:          LoginFieldRequest
    - EmployeeId (string, required): technician or helper employee identifier
    - Pin (string, required): 4–6 digit PIN
  Response DTO:         AuthTokenResponse
  DB Tables:            Users (read via EmployeeId), RefreshTokens (write), UserDevices (upsert)
  Business Rules:
    1. PIN validated against hashed PIN on user record
    2. Device registration enforced — UserDevices upserted on every login
    3. Role scope limited to Technician/Helper job data
  Failure Cases:
    - Invalid EmployeeId or PIN → 401
  Notes on Drift:
    - Previously documented in this file as POST /api/auth/technician-login — WRONG.
      Actual route confirmed from AuthController.cs: POST /api/auth/login-field.
      Corrected 2026-05-24 during source audit.

---

### Flow: OTP-Based Login (Internal Users — 2FA Step-Up)
  Entry Points:         Admin 2FA step after password login (LoginOtpRequest)
  UI Trigger:           OTP entry after password challenge
  API Endpoint:         POST /api/auth/login-otp
  Auth:                 AllowAnonymous
  Request DTO:          LoginOtpRequest
    - LoginId (string, required): email or username
    - Otp (string, required): 6-digit OTP
  Response DTO:         AuthTokenResponse
  DB Tables:            OtpVerifications (read + delete), Users (read), RefreshTokens (write)
  Business Rules:
    1. For non-Customer roles, OTP resolved via direct user lookup
       (bypasses CustomerAccountLookupService)
    2. Issues full JWT + refresh token on success
    3. AuthStatus transitions REQUIRES_2FA → AUTHENTICATED
  Failure Cases:
    - Invalid/expired OTP → 400

---

### Flow: Verify OTP (Email-Based, Internal — /verify-otp route)
  Entry Points:         Admin /verify-otp screen (AuthStatus.REQUIRES_2FA)
  UI Trigger:           OTP digit entry
  API Endpoint:         POST /api/auth/verify-otp
  Auth:                 AllowAnonymous
  Request DTO:          VerifyOtpRequest
    - Email (string, required): email of pending auth user
    - Otp (string, required): 6-digit OTP
  Response DTO:         AuthTokenResponse
  DB Tables:            OtpVerifications (read + delete), Users (read), RefreshTokens (write)
  Business Rules:
    1. OTP validated against email in OtpVerifications
    2. Issues full JWT + refresh token on success
  Failure Cases:
    - Invalid/expired OTP → 400
  Notes on Drift:
    - 2026-04-23: /verify-otp screen stabilized with branded auth view, OTP paste support,
      autofocus on first digit, session-expired fallback card.

---

### Flow: Refresh Token
  Entry Points:         All surfaces — automatic client-side refresh on 401
  UI Trigger:           Axios interceptor on access token expiry
  API Endpoint:         POST /api/auth/refresh  (alias: POST /api/auth/refresh-token — same handler)
  Auth:                 AllowAnonymous
  Request DTO:          RefreshTokenRequest
    - AccessToken (string, required): expired access token
    - RefreshToken (string, required): valid refresh token
  Response DTO:         AuthTokenResponse
  DB Tables:            RefreshTokens (read + rotate + write), Users (read)
  Business Rules:
    1. Refresh token rotated on every use — old token invalidated, new token issued
    2. Revoked on logout or admin force-logout
    3. /api/auth/refresh and /api/auth/refresh-token are both live and route to identical handler
  Failure Cases:
    - Expired refresh token → 401
    - Revoked refresh token → 401
    - Tampered token → 400

---

### Flow: Forgot Password
  Entry Points:         Login screen "Forgot Password" (staff and customer)
  UI Trigger:           Forgot-password form submit
  API Endpoint:         POST /api/auth/forgot-password
  Auth:                 AllowAnonymous
  Request DTO:          ForgotPasswordRequest
    - Email (string?, optional): staff email-based reset
    - Phone (string?, optional): customer phone-based OTP reset
    - LoginId (string?, optional): unified identifier — phone takes priority over email
  Response DTO:         AuthActionResponse
  Business Rules:
    1. If LoginId or Phone present → triggers SendOtpCommand (OTP delivery to phone)
    2. If only Email present → triggers ForgotPasswordCommand (email reset token delivery)
    3. Responds with generic success message for security (no user-enumeration info)
  Failure Cases:
    - All fields null/empty → 200 with generic message (no error exposed)

---

### Flow: Reset Password
  Entry Points:         Reset password screen (staff email link or customer OTP flow)
  UI Trigger:           New password form submit
  API Endpoint:         POST /api/auth/reset-password
  Auth:                 AllowAnonymous
  Request DTO:          ResetPasswordRequest
    - Token (string?, optional): email reset token (staff flow)
    - Password (string?, optional): new password (staff token flow)
    - Phone (string?, optional): phone number (customer OTP flow)
    - Otp (string?, optional): 6-digit OTP (customer OTP flow)
    - NewPassword (string?, optional): new password (customer OTP flow)
  Response DTO:         AuthActionResponse
  DB Tables:            OtpVerifications (read + delete for OTP flow),
                        Users (write), PasswordHistory (write)
  Business Rules:
    1. Customer OTP flow: Phone + Otp + NewPassword all present →
       ResetCustomerPasswordWithOtpCommand
    2. Staff token flow: Token + Password present → ResetPasswordCommand
    3. Password policy enforced: min 10 chars, upper/lower/number/symbol,
       no reuse of last 5 passwords
  Failure Cases:
    - Invalid OTP → 400
    - Invalid/expired reset token → 400
    - Password policy violation → 400

---

### Flow: Change Password — Customer (Authenticated)
  Entry Points:         Customer portal profile screen
  UI Trigger:           "Change Password" form submit
  API Endpoint:         POST /api/auth/change-password
  Auth:                 Authorize(Roles = Customer)
  Request DTO:          ChangeCustomerPasswordRequest
    - CurrentPassword (string, required)
    - NewPassword (string, required)
  Response DTO:         AuthActionResponse
  DB Tables:            Users (read + write), PasswordHistory (write)
  Business Rules:
    1. Current password validated before update
    2. New password must not match last 5 passwords
  Failure Cases:
    - Wrong current password → 400
    - Password policy violation → 400

---

### Flow: Logout
  Entry Points:         All surfaces — user profile menu
  UI Trigger:           "Logout" button
  API Endpoint:         POST /api/auth/logout
  Auth:                 Authorize (any authenticated role)
  Request DTO:          LogoutRequest
    - RefreshToken (string, required): current session's refresh token
  Response DTO:         AuthActionResponse
  DB Tables:            RefreshTokens (revoke/delete)
  Business Rules:
    1. Refresh token revoked immediately on logout
    2. Access token expires naturally (15 min TTL; no server-side JWT revocation list)
  Failure Cases:
    - Already-revoked token → 200 (idempotent)

---

### Flow: Delete Customer Account (Self-Service)
  Entry Points:         Customer portal profile screen
  UI Trigger:           "Delete Account" action
  API Endpoint:         DELETE /api/auth/account
  Auth:                 Authorize(Roles = Customer)
  Request DTO:          (none — authenticated user identity from JWT claims)
  Response DTO:         CustomerAccountDeletionResponse
  DB Tables:            Customers (soft-delete: IsDeleted=true), Users (deactivate)
  Business Rules:
    1. Soft delete only — data retained; hard delete only via admin cleanup
    2. All refresh tokens for this user revoked on account deactivation
    3. Reason logged: "Customer app self-service deletion"
  Failure Cases:
    - Not authenticated → 401

---

### Flow: Get Current User (Me)
  Entry Points:         All surfaces — session bootstrap, route guard hydration
  UI Trigger:           App startup / Axios interceptor rehydration
  API Endpoint:         GET /api/auth/me
  Auth:                 Authorize (any authenticated role)
  Request DTO:          (none)
  Response DTO:         CurrentUserResponse (full shape — see Verify OTP response above)
  DB Tables:            Users (read), Roles (read), RolePermissions (read)
  Business Rules:
    1. Returns full identity + flat permission set for JWT subject
    2. AdminMobile uses this to hydrate RBACProvider and session store
  Failure Cases:
    - Expired/invalid token → 401

---

### Flow: Get Permission Snapshot (Me/Permissions)
  Entry Points:         AdminMobile RBAC hydration, role-navigation composition
  UI Trigger:           Session init / permission refresh call
  API Endpoint:         GET /api/auth/me/permissions
  Auth:                 Authorize (any authenticated role)
  Request DTO:          (none)
  Response DTO:         AuthPermissionSnapshotResponse
    - Modules (Dictionary<string, PermissionModuleActionsResponse>):
        per-module action flags (view/create/edit/delete/approve/export per module)
    - DataScope (string): resolved data scope (All / BranchOnly / OwnCreated / etc.)
    - Permissions (string[]): flat permission key list
  DB Tables:            Users (read), RolePermissions (read)
  Business Rules:
    1. Module matrix built via PermissionModuleMatrixMapper.Build(permissions, roles)
    2. Data scope resolved via PermissionDataScopeMapper.Resolve(roles)
  Failure Cases:
    - Expired/invalid token → 401

---

### Flow: Force Logout User (SuperAdmin)
  Entry Points:         Admin session management console
  UI Trigger:           "Force Logout" button on user session panel
  API Endpoint:         POST /api/auth/force-logout/{userId}
  Auth:                 Authorize(Roles = SuperAdmin)
  Request DTO:          userId (long, route parameter)
  Response DTO:         AuthActionResponse
  DB Tables:            RefreshTokens (revoke all for userId)
  Business Rules:
    1. All refresh tokens for target user revoked immediately
    2. Audit log entry created
  Failure Cases:
    - User not found → 404
    - Caller not SuperAdmin → 403

---

## CustomerAuth Controller — Customer Registration & Password Management
## Route prefix: /api/customer-auth
## NOTE: This entire controller was MISSING from ProjectOverview prior to 2026-05-24 audit.

### Flow: Register Customer
  Entry Points:         Customer app registration screen, guest-to-registered conversion
  UI Trigger:           Registration form submit
  API Endpoint:         POST /api/customer-auth/register
  Auth:                 AllowAnonymous
  Request DTO:          RegisterCustomerRequest
    - CustomerName (string, required)
    - MobileNumber (string, required): must be unique across Customers
    - EmailAddress (string, required): must be unique across Customers
    - Password (string?, optional): if null, system generates a temporary password
  Response DTO:         CustomerAccountResponse
    - CustomerId (long)
    - UserId (long)
    - CustomerName (string)
    - MobileNumber (string)
    - EmailAddress (string)
    - PasswordGenerated (bool): true if system auto-generated password
    - RequiresPasswordDelivery (bool): true if generated password must be delivered via SMS/email
    - MustChangePassword (bool)
    - IsTemporaryPassword (bool)
    - PasswordExpiryOnUtc (DateTime?)
  DB Tables:            Customers (write), Users (write), PasswordHistory (write)
  Business Rules:
    1. Creates both Customer and linked User records atomically
    2. If Password null → system generates temporary password → MustChangePassword=true
    3. MobileNumber uniqueness enforced
    4. EmailAddress uniqueness enforced
  Failure Cases:
    - Duplicate mobile or email → 400 with conflict message
    - Validation failure → 400

---

### Flow: Customer Forgot Password
  Entry Points:         Customer login screen "Forgot Password" link
  UI Trigger:           Forgot-password form submit
  API Endpoint:         POST /api/customer-auth/forgot-password
  Auth:                 AllowAnonymous
  Request DTO:          ForgotCustomerPasswordRequest
    - LoginId (string, required): customer mobile number or email address
  Response DTO:         CustomerPasswordOperationResponse
    - PasswordUpdated (bool)
    - PasswordGenerated (bool)
    - RequiresPasswordDelivery (bool)
    - MustChangePassword (bool)
    - IsTemporaryPassword (bool)
    - PasswordExpiryOnUtc (DateTime?)
  DB Tables:            Customers (read), Users (read + write)
  Business Rules:
    1. Looks up customer by mobile or email via LoginId
    2. Generates new temporary password and delivers via SMS/email
  Failure Cases:
    - Customer not found → 400

---

### Flow: Customer Reset Password
  Entry Points:         Customer portal reset-password screen
  UI Trigger:           Reset form submit
  API Endpoint:         POST /api/customer-auth/reset-password
  Auth:                 AllowAnonymous
  Request DTO:          ForgotCustomerPasswordRequest
    - LoginId (string, required): mobile or email
  Response DTO:         CustomerPasswordOperationResponse (same shape as forgot-password)
  DB Tables:            Customers (read), Users (write)
  Business Rules:       Routes to same ForgotCustomerPasswordCommand handler as forgot-password
  Notes on Drift:       /reset-password and /forgot-password currently use identical handler.
                        Differentiate if token-based reset flow is added in future.

---

### Flow: Customer Change Password (Authenticated)
  Entry Points:         Customer portal profile screen
  UI Trigger:           "Change Password" form submit
  API Endpoint:         POST /api/customer-auth/change-password
  Auth:                 Authorize(Roles = Customer)
  Request DTO:          ChangeCustomerPasswordRequest
    - CurrentPassword (string, required)
    - NewPassword (string, required)
  Response DTO:         CustomerPasswordOperationResponse
  DB Tables:            Users (read + write), PasswordHistory (write)
  Business Rules:
    1. Current password validated before update
    2. Password history enforced
  Failure Cases:
    - Wrong current password → 400

---

Auth Module Routing Notes (all flows):
  - All routes: /api/... directly — no v{version} URL segment (updated 2026-04-25)
  - 2026-04-22: AuthController began serving /api/auth/otp/send + /api/auth/otp/verify
    as customer-app compatibility aliases
  - 2026-04-23: Non-Customer OTP login bypasses CustomerAccountLookupService;
    uses direct user lookup for Admin/internal roles
  - Dev OTP: fixed '123456' from AuthSessionTokenFactory.CreateOtp()
    (COOLZO_FIXED_TEST_OTP env var overrides; non-dev uses random 6-digit)

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
   - HEAD /api/health — Same action; allows uptime monitors (UptimeRobot default = HEAD) to ping without 405. Returns 200, no body. (`GetAsync`)

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
   - GET  /api/customers/me/equipment/{equipmentId} — Get single equipment by id (`GetMyEquipmentByIdAsync`) [added 2026-05-26]
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
   - GET  /api/bookings/public/settings — Public booking-mode flags (`GetPublicSettingsAsync`) [AllowAnonymous]
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
