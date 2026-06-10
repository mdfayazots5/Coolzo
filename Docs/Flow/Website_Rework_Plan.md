# Website Rework & Stabilization Plan (Public Web — Frontend/Web)
**Owner:** Project CEO + Chief Architect | **Status:** APPROVED DIRECTION / PENDING PHASE-0 GO
**Created:** 2026-06-10 | **Tier:** T3 — Strategic | **Goal:** Stable, AC-service-grade public Website with reusable rules so the same screens are never reworked again.

> Precedence: this plan governs the Website effort. Every phase ends with a ProjectOverview update + QA device-matrix gate + CEO sign-off. No phase ships past a failed QA/Security gate.

---

## LOCKED DECISIONS (from CEO clarification 2026-06-10)
1. **Scope (Phase 1 initiative):** Public site + booking flow (guest AND logged-in). Customer portal `/portal/*` deferred to a later initiative.
2. **Visual direction:** Keep CLAUDE.md premium navy/gold design system. Fix execution: content hierarchy (service-first), sizing, spacing, responsiveness.
3. **Master data:** All DB work delivered as **PostgreSQL/Supabase SQL files for review**. Nothing executes automatically — the user runs approved files in Supabase.
4. **Public sections kept:** Home + Services + ServiceDetail + Booking (the AC-service spine).
   - CONFIRMED 2026-06-10: keep **Terms + Privacy** (legal) and a **lightweight Contact** (phone/WhatsApp footer + contact block).
   - **Remove:** Blog. **Defer (hide from nav):** About, WhyCoolzo, Reviews, AMC.
   - Execution: nav/route changes happen in **Phase 2** (rebuild) so `App.tsx`/`Navbar` are touched once, not twice.

---

## ROOT-CAUSE DIAGNOSIS (why it's unstable / doesn't look like an AC service)
- **No stable responsive/layout standard** → every fix is ad-hoc → recurring rework.
- **Booking flow has no single documented contract enforced in code** → guest vs logged-in branching, slot/zone/idempotency issues recur.
- **Content hierarchy is not service-first** → first screen doesn't say "AC repair / service / AMC — book now".
- **Screens not device-correct** → wrong sizes, inconsistent spacing across phone/tablet/desktop.
- **Prod Supabase seeded with test/garbage master data** → real users see junk.
- **Too many half-built sections** (blog/about/etc.) inflate surface area without value.
- **CMS delivery incomplete** → admin can't manage screens/images per page; uploads landed on a single page.

---

## STABLE RULES TO BE ESTABLISHED (kills rework)
Created in Phase 0, enforced by QA on every later phase:
1. **Web Responsive & Layout Standard** (`Docs/Rules/Web_Responsive_Standard.md`): breakpoints, container max-widths, 8px spacing scale, type scale, component sizing, image rules, min 44px touch targets, section rhythm.
2. **Shared layout primitives** (`Frontend/Web/src/components/`): `Container`, `Section`, `Grid`, responsive image via existing `SnapshotImage`. Every page uses these — one system, not per-page CSS.
3. **Definition of Stable (QA device matrix):** phone 360/390, tablet 768, desktop 1280/1440 — all green before any sign-off.
4. **CLAUDE.md updates:** add the Responsive Standard as a governing artifact; strengthen UX Designer + Product Manager charters (the roles observed underperforming).

---

## PHASES (sequenced: rules → stop the bleeding → make it right → on clean data → hand admin the controls)

### PHASE 0 — Foundations & Governance (no UI churn, no data changes)
- Write `Web_Responsive_Standard.md`; update CLAUDE.md (artifact + role charters).
- Build shared layout primitives (Container/Section/Grid/responsive image).
- Define QA device matrix & "Definition of Stable".
- **Gate:** Architecture + QA approve the standard. Deliverable = rules + primitives, zero behavior change.

### PHASE 1 — Booking Flow Stabilization (guest + logged-in) — TOP PRIORITY
- Finalize the **stable booking contract** in ProjectOverview (both journeys: every step, validation, idempotency, failure/recovery, auth vs guest branching).
- Audit & fix `BookingWizard.tsx`: step validation, zone-by-pincode, slot load/selection, X-Idempotency-Key, guest vs customer path, OTP verify, full loading/empty/error/retry states (no dead-ends).
- **Gate (Security + QA veto):** both journeys pass on the full device matrix; recurring issues documented as drift entries so they never recur.

### PHASE 2 — Core Public Pages Rebuild (Home, Services, ServiceDetail)
- Rebuild content hierarchy service-first: hero (what we do + Book CTA), services with pricing, how-it-works, trust signals, AMC teaser (link only while AMC page deferred).
- All pages on shared primitives + responsive standard.
- Remove Blog; hide deferred sections from nav; keep Terms/Privacy + footer Contact (pending confirm).
- **Gate:** UX + QA device-matrix green; renders real masters (depends on Phase 3 data).

### PHASE 3 — Master Data Cleanup (SQL files; user executes in Supabase)
PostgreSQL/Supabase syntax (provider = Supabase Postgres per project memory). Files prepared for review:
- `01_purge_test_masters.sql` — clean test rows (soft-delete pattern)
- `02_seed_service_categories.sql`, `03_seed_services.sql` (pricing + ImageUrl), `04_seed_ac_types.sql`, `05_seed_tonnages.sql`, `06_seed_brands.sql`, `07_seed_zones_pincodes.sql`, `08_seed_slots.sql`
- **Gate:** Database Architect + CEO review; user runs in Supabase. No auto-execution. (Final pricing/zones to be confirmed by user or seeded as realistic Hyderabad placeholders marked for replacement.)
- *Sequenced after Phase 2 so we seed exactly the fields the rebuilt UI needs — seed once, correctly.*

### PHASE 4 — CMS Module Completion (admin manages the Website)
- Finish CMS delivery so admin controls every page/section image (per `page.slot`, not one page), content blocks, theme, publish/version/rollback.
- Admin screens: Theme & Appearance, Screen Image Manager (per slot), Content/Master editors, Publish & Version history.
- Public site already consumes the published snapshot via `ContentContext`/`snapshotService`.
- **Gate:** Architecture + QA; admin can re-skin/re-image the public spine without code.

---

## DELIVERY DISCIPLINE (per phase, every time)
- DB → SQL files first, executed only after approval (Supabase Postgres).
- ProjectOverview.md updated to stable-contract level at phase end.
- QA device matrix green before sign-off.
- Drift logged so recurring issues become permanent fixes.
- CLAUDE.md kept current (rules + role charters).
