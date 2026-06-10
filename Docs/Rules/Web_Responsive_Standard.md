# Web Responsive & Layout Standard (Public Web — Frontend/Web)
**Authority:** Governing artifact for all public-Website UI. **Tier:** binds T1+ Website work.
**Created:** 2026-06-10 (Phase 0 of Website Rework). **Stack:** React 19 + Vite + Tailwind CSS v4 (`@import "tailwindcss"`, CSS `@theme`).

> Purpose: one fixed set of layout/responsive rules so screens are built right once and never reworked.
> QA enforces this on every Website change. Any new public page MUST use the shared primitives below.

---

## 1. SOURCE-OF-TRUTH TOKENS (implemented theme — `src/index.css @theme`)
| Token | Value | Tailwind class |
|---|---|---|
| Navy (primary) | `#0A192F` | `brand-navy` |
| Gold (accent) | `#D4AF37` | `brand-gold` |
| Cream (background) | `#FDFCFB` | `brand-cream` |
| Black (deep text) | `#050505` | `brand-black` |
| Serif (headings) | Cormorant Garamond | `font-serif` |
| Sans (body/UI) | Inter | `font-sans` |

> ⚠ Drift note: the CLAUDE.md design-system table lists different hexes (navy `#1B2A4A`, gold `#C9A84C`, Inter-only). Decision 2026-06-10: **keep the implemented Web theme** above as source of truth for the public site. Do not change colors/fonts during the rework.

---

## 2. BREAKPOINTS (Tailwind v4 defaults — do not redefine)
| Name | Min width | Target device |
|---|---|---|
| (base) | 0 | phones (360–430) |
| `sm` | 640px | large phones / small tablets portrait |
| `md` | 768px | tablets |
| `lg` | 1024px | small laptops |
| `xl` | 1280px | desktops |
| `2xl` | 1536px | large desktops |

Rule: **mobile-first.** Author base styles for phone, add `sm:`/`md:`/`lg:` upward. Never author desktop-first then patch mobile.

Root font scales automatically (index.css): 16px base → 17px ≥640 → 18px ≥1024. Use `rem`-based Tailwind utilities so type/space scale with it.

---

## 3. PAGE WIDTH & GUTTERS
- **Max content width:** 1280px, centered (`Container` default → `max-w-7xl`). Wide screens get padding, never stretch.
- **Horizontal gutters (every page):** `px-5` (20px) base → `sm:px-6` (24px) → `lg:px-8` (32px). Provided by `Container`. Never hard-code page padding per screen.
- **Reading-heavy blocks** (legal, blog body): `Container width="narrow"` → `max-w-3xl`.

---

## 4. SPACING SCALE (8px base — no off-scale values)
Allowed steps only: `2`=8 · `4`=16 · `6`=24 · `8`=32 · `12`=48 · `16`=64 (px). `1`/`3` (4/12px) allowed for fine icon/text gaps. **No arbitrary `p-[13px]`-style values** except documented exceptions.

**Section vertical rhythm** (via `Section` primitive):
| spacing | classes | use |
|---|---|---|
| `compact` | `py-10 md:py-14` | dense utility sections |
| `default` | `py-16 md:py-24` | standard marketing section |
| `loose` | `py-20 md:py-32` | hero / feature spotlight |

---

## 5. TYPE SCALE (defined in index.css `@layer base` — reuse, don't override)
- `h1` → `text-4xl md:text-5xl lg:text-6xl font-serif`
- `h2` → `text-3xl md:text-4xl lg:text-5xl font-serif`
- `h3` → `text-2xl md:text-3xl lg:text-4xl font-serif`
- body `p` → `text-sm md:text-base leading-relaxed`
- Eyebrow/label: `text-[10px] uppercase tracking-[0.3em] font-bold` (existing idiom)
- Max **2 font weights per screen** (CLAUDE.md design law).

---

## 6. GRID & COLUMNS (via `Grid` primitive)
Cards/lists collapse predictably:
- `cols={2}` → `grid-cols-1 sm:grid-cols-2`
- `cols={3}` → `grid-cols-1 sm:grid-cols-2 lg:grid-cols-3`
- `cols={4}` → `grid-cols-1 sm:grid-cols-2 lg:grid-cols-4`
- Gap: `default` = `gap-6 lg:gap-8`. Never a single fixed-width grid that overflows on phones.

---

## 7. TOUCH & INTERACTION
- **Minimum touch target 44×44px** on all interactive elements (`min-h-[44px] min-w-[44px]`). Already the idiom — make it universal.
- Visible focus ring on every interactive element: `focus-visible:ring-2 focus-visible:ring-brand-gold/60`.
- Fixed bottom bars (mobile) must use `safe-area-pb` utility (iOS notch).

---

## 8. IMAGES (prevent layout shift & oversize)
- Wrap in a fixed aspect ratio box (`aspect-video` / `aspect-square` / `aspect-[4/3]`) + `object-cover`. No raw `<img>` that dictates its own height.
- Use the existing `SnapshotImage` component for CMS/screen images (resolves responsive `variants`).
- Always `loading="lazy"` below the fold; set explicit dimensions where possible to avoid CLS.
- No autoplay video, no carousels (CLAUDE.md forbidden-UI law).

---

## 9. CARDS & ELEVATION
- Radius: `rounded-xl` cards (12px), `rounded-lg` buttons (8px), `rounded` small (4px).
- Elevation: `shadow-sm` rest → `hover:shadow-xl` (no heavy borders). Border accents: `border border-brand-navy/5`.

---

## 10. MANDATORY ASYNC STATES (every data-driven screen)
Every screen that loads data MUST render all four: **loading**, **empty**, **error + retry**, **success**. No dead-ends, no infinite spinners. This is a QA gate item (booking flow especially).

---

## 11. DEFINITION OF STABLE — QA DEVICE MATRIX
A Website change is "stable" only when verified green at **all** of:
| Class | Widths | Notes |
|---|---|---|
| Phone | 360, 390, 430 | primary — most traffic |
| Tablet | 768, 820 | portrait |
| Desktop | 1280, 1440 | content capped at 1280 |

Per width, verify: no horizontal scroll, no overlap/clipping, touch targets ≥44px, readable type, images not distorted, all four async states reachable. Failing any width blocks sign-off (QA veto).

---

## 12. ENFORCEMENT
- New/edited public pages MUST use `Container` / `Section` / `Grid` primitives (`src/components/`). No bespoke page-level max-width/padding/grid.
- Off-standard spacing/type/color requires a documented exception in the PR/notes.
- This standard is referenced by CLAUDE.md DESIGN SYSTEM and is permanent until explicitly revised.
