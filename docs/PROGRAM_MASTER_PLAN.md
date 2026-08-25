# Kolpojontro Foundation — Program Master Plan (Phase 0)

> **Scope of this document.** This is the *program-level* plan that gets the organisation from
> "a half-converted React app plus a scaffold backend" to **a real, publicly hosted website**
> with documented repositories — before any backend dependency exists.
>
> It covers three workstreams only:
> 1. **Workstream A** — Re-implement and *verify* that the legacy HTML site is fully rebuilt in React.
> 2. **Workstream B** — Proper READMEs for both repositories.
> 3. **Workstream C** — Ship a **standalone static frontend** that can be hosted on static hosting today,
>    architected so backend/dynamic features can be layered in later without a rewrite.
>
> Everything after this document is covered by:
> - [`FRONTEND_MASTER_PLAN.md`](FRONTEND_MASTER_PLAN.md) — single source of truth for frontend development.
> - [`BACKEND_MASTER_PLAN.md`](BACKEND_MASTER_PLAN.md) — single source of truth for backend development.
>
> This plan is **Phase 0** in the shared phase numbering used by all three documents.

| | |
|---|---|
| **Organisation** | Kolpojontro Foundation (কল্পযন্ত্র ফাউন্ডেশন) — volunteer-run social research & welfare non-profit, Dhaka, Bangladesh |
| **Repositories** | `KJWebsite.FrontEnd` (github.com/DevCastBD/KJWebsite.FrontEnd) · `KJWebsite.Backend` (github.com/DevCastBD/KJWebsite.Backend) |
| **Document status** | Draft v1 — awaiting sign-off from core members |
| **Owner** | Engineering lead |
| **Phase** | Phase 0 — Static Launch |

---

## Table of contents

1. [Current state assessment](#1-current-state-assessment)
2. [Guiding principles and locked decisions](#2-guiding-principles-and-locked-decisions)
3. [Workstream A — Legacy → React parity and verification](#3-workstream-a--legacy--react-parity-and-verification)
4. [Workstream B — Repository READMEs](#4-workstream-b--repository-readmes)
5. [Workstream C — Standalone static site launch](#5-workstream-c--standalone-static-site-launch)
6. [Milestones and sequencing](#6-milestones-and-sequencing)
7. [Definition of Done for Phase 0](#7-definition-of-done-for-phase-0)
8. [Risks and mitigations](#8-risks-and-mitigations)
9. [Open questions for the organisation](#9-open-questions-for-the-organisation)

---

## 1. Current state assessment

### 1.1 Frontend — `KJWebsite.FrontEnd`

| Aspect | Finding |
|---|---|
| Stack | Vite 8, React 19.2, `react-router-dom` 7.15, plain JS (no TypeScript), ESLint 9 + Prettier |
| Routing | SPA with `BrowserRouter`; routes for `/`, `/about`, `/projects`, `/project/:id`, `/news/:id`, `/blog`, `/contact`, `/registration`, `/success`, `/events` (placeholder), legacy `.html`/`.php` redirect routes, catch-all → `/` |
| Styling | Original charity theme CSS retained verbatim in `public/css/`, pulled in by `src/styles.css` via `@import url('/css/style.css')`. Bootstrap 3, Font Awesome 4, owl-carousel, fancybox, animate.css all still imported through `style.css` even though the carousels/lightbox were re-implemented in React |
| i18n | Hand-rolled `LanguageContext` (bn/en) with `localStorage`; **translation strings are hard-coded inline in components** — `src/pages/Home.jsx` alone is ~300 lines, most of it copy |
| Content | `src/services/contentService.js` loads from `VITE_CONTENT_API_URL` and falls back to `src/data/siteData.js` / `src/data/newsData.js`. Expected shape is `{ projects: { bn, en }, news: { bn, en } }` |
| Forms | `src/services/ctaService.js` posts to `VITE_CTA_API_URL`; **throws immediately when the variable is unset** |
| Assets | `public/` is ~11 MB — 9.2 MB of unoptimised JPG/PNG, 1.2 MB of fonts (Font Awesome 4 + flaticon + glyphicons), 428 KB CSS |
| Quality gates | No tests, no CI, no type checking, no bundle/perf budget |
| Hosting readiness | No SPA rewrite config, no `favicon`, no meta/OG/Twitter tags, no `robots.txt`, no `sitemap.xml`, no analytics, no error boundary, no real 404 page |

### 1.2 Backend — `KJWebsite.Backend`

| Aspect | Finding |
|---|---|
| Stack | .NET 10 (`net10.0`), ASP.NET Core Minimal APIs, `.slnx` solution, YARP gateway, EF Core 10, SQLite, Scalar API reference |
| Services | `ApiGateway` (:7000), `ContentService` (:7001), `CtaSubmissionService` (:7002), `AuthIdentityService` (:7003), plus an empty `KJWebsite.BuildingBlocks` (`Class1.cs`) |
| Contract | `openapi.v1.yaml` (638 lines) documents gateway-level v1 routes |
| Persistence | Auth + CTA on SQLite with EF migrations; **ContentService is in-memory only** — all content is lost on restart |
| Security | Passwords stored **in plain text**; access tokens are opaque GUIDs held in an **in-process dictionary** (lost on restart, breaks under >1 replica); ContentService admin auth is the hard-coded string `dev-admin-token`; no CORS policy, no rate limiting, no HTTPS enforcement |
| Gateway | Routes and destinations hard-coded in `Program.cs` as `localhost` addresses; not configuration-driven |
| Missing | Tests, Docker, CI/CD, structured logging/observability, email delivery, file/media storage, background jobs, payments, PostgreSQL provider |
| Legacy references | `kj-registration/` and `kj-registration-AwaitingUserFeatures/` — ASP.NET Identity apps with an `AwaitingUser` approval workflow (`Awaiting / Approved / Rejected / Postponed / Terminated`), roles `SuperAdmin / Admin / Moderator / Member`, and MySQL dumps. **Read-only historical references** — but they define the membership domain the new backend must reproduce |

### 1.3 Legacy site — `KJWebsite.FrontEnd/legacy-html/`

Six files: `index.html`, `about.html`, `contact.html`, `registration.html`, `success.php`, `email.html`.

**Critical finding:** a large share of the legacy markup is *unused theme filler that is already commented out in the source* — fact counters ("Total Volunteer 2200"), "Meet Our Volunteers" with placeholder names (Muhibbur Rashid, Rashed Kabir…), "Testimonials" by "Roberto Carlos", the clients/sponsors logo carousel, and the donate pie-charts. These are **not** Kolpojontro content and must **not** be ported verbatim. The genuine legacy site is much smaller than the file sizes suggest.

`email.html` is not a website page at all — it is a **transactional email template** ("Congratulations on being selected as a Member… Projonmo Boron orientation programme"). It belongs to the backend notification system, not the React app.

---

## 2. Guiding principles and locked decisions

| # | Decision | Rationale |
|---|---|---|
| D1 | **Static-first.** The site must build to plain files and deploy to static hosting with zero backend running. | The organisation needs a live site now; the backend is months away from production. |
| D2 | **Every dynamic capability is feature-flagged.** Backend-dependent UI is behind a runtime capability flag that defaults to *off*. | Lets frontend and backend teams work in parallel without blocking releases. |
| D3 | **One anti-corruption layer for all API access.** All network access goes through `src/api/*`; components never call `fetch` directly. | The current `contentService` shape and the live `ContentService` API already disagree; an adapter absorbs that. |
| D4 | **Legacy theme CSS is kept for Phase 0**, then retired incrementally in later phases. | Rewriting the design system now would delay launch and risk visual regressions. |
| D5 | **Bengali is the primary language, English is the secondary.** | The legacy production site was Bengali-first; the React app currently defaults to English — this is a regression to fix. |
| D6 | **No fabricated content.** Placeholder theme content is deleted, not translated. Team, testimonials, statistics, and sponsor logos appear only when the organisation supplies real data. | A non-profit's credibility depends on it. |
| D7 | **Content lives in versioned JSON/Markdown until the CMS exists**, in exactly the shape the future API will return. | Migration to the CMS becomes a transport swap, not a data model rewrite. |
| D8 | **Accessibility and performance are launch criteria, not follow-ups.** | Target audience includes low-bandwidth mobile users in Bangladesh. |

### 2.1 Shared phase numbering

All three plans use the same phase numbers so frontend and backend tracks can ship in lock-step:

| Phase | Theme | Frontend | Backend |
|---|---|---|---|
| **P0** | Static launch | Parity + static site live | Repo hygiene, Docker, security baseline |
| **P1** | Foundations & contracts | API client layer, feature flags | PostgreSQL, JWT, real content persistence, CORS |
| **P2** | Content & back-office shell | Admin shell, content editors | Content domain, RBAC, media, audit log |
| **P3** | Membership & volunteers | Member portal, application form | Membership lifecycle, approvals, directory |
| **P4** | Donations & finance | Donation UI, finance back office | Payments, receipts, ledger, expenses |
| **P5** | Events & volunteer ops | Event pages, RSVP, volunteer portal | Events, attendance, hours, teams |
| **P6** | Communications & community | Press/media pages, newsletter UI | Email, newsletter, press coverage, inbox |
| **P7** | Analytics & transparency | Dashboards, impact & annual reports | Reporting, aggregation, exports |
| **P8** | Scale & hardening | PWA, perf, deep i18n | Observability, security audit, DR |

---

## 3. Workstream A — Legacy → React parity and verification

### 3.1 Objective

Prove, item by item, that every piece of *real* legacy content and behaviour exists in the React app — and fix what does not.

### 3.2 Parity matrix

Legend: ✅ ported · ⚠️ ported with gaps · ❌ missing/regressed · ⛔ deliberately dropped (theme filler)

#### `index.html` → `/`

| Legacy element | React counterpart | Status | Action |
|---|---|---|---|
| Top bar: support email + social icons | `components/TopBar.jsx` | ✅ | Language switch added — an improvement, keep |
| Header: logo, email box, phone box | `components/Header.jsx` | ✅ | — |
| Header search box | — | ⛔ | Legacy search was non-functional theme markup. Re-introduce properly in P2 (site search) |
| Main menu (হোম / কল্পযন্ত্র সম্পর্কে / প্রকল্প / ইভেন্ট / ব্লগ / যোগাযোগ) | `data/siteData.js` → `Header.jsx` | ⚠️ | `/events` renders `Placeholder.jsx` — a dead nav item on a public site. **A-1** |
| Revolution-slider hero, 3 slides | `components/home/HomeHero.jsx` | ✅ | jQuery revslider replaced by a React slider — verify autoplay, arrows, dots, and pause-on-hover |
| 3 call-to-action boxes (আর্থিক অনুদান / প্রজন্ম / সহযোগিতা) | `components/home/HomeCallouts.jsx` | ✅ | Links now route to `/contact?cta=…` and `/registration?cta=join` — an improvement |
| Donate pie-charts inside CTA boxes | — | ⛔ | Commented out in legacy source |
| সাম্প্রতিক কার্যক্রম (recent activities, 3 items) | `components/home/HomeActivitiesFeatured.jsx` | ✅ | Content still hard-coded in `Home.jsx`. **A-6** |
| উল্লেখযোগ্য প্রকল্প — Pathshala feature + "Help us by share" | `HomeActivitiesFeatured.jsx` | ⚠️ | Share icons link to the org's own profiles, not share intents. **A-2** |
| প্রকল্পসমূহ carousel | `components/home/HomeProjectsCarousel.jsx` | ✅ | — |
| Parallax promo banner (ঝরে পড়া রোধে এগিয়ে আসুন) | `components/PromoteProject.jsx` | ✅ | Verify parallax/background behaviour on mobile |
| ফটো গ্যালারি + fancybox lightbox | `components/GallerySection.jsx` | ✅ | Custom lightbox; verify keyboard (Esc/←/→) and focus trap. **A-7** |
| Fact counters (Total Volunteer / Total Projects) | — | ⛔ | Commented out; numbers were theme dummies |
| "Meet Our Volunteers" carousel | — | ⛔ | Placeholder people. Real team page is a P2/P3 feature |
| "Testimonials" (Roberto Carlos ×12) | — | ⛔ | Theme filler. Real testimonials are a P6 feature |
| Clients/sponsors logo carousel | — | ⛔ | Theme filler. Real partners are a P6 feature |
| সর্বশেষ সংবাদ (latest news, 3 cards) | `components/home/HomeLatestNews.jsx` | ✅ | — |
| Footer: logo, quick links, latest posts, contact form, socials, copyright | `components/Footer.jsx` | ⚠️ | Copyright reads "2016-2017". **A-3** |

#### `about.html` → `/about`

| Legacy element | React counterpart | Status | Action |
|---|---|---|---|
| Inner page header (আমাদের পরিচয়) | `components/PageHeader.jsx` | ✅ | — |
| Intro statement + আমাদের উদ্দেশ্য + photo | `pages/About.jsx` | ✅ | — |
| আমাদের কর্মপদ্ধতি (two columns) | `pages/About.jsx` | ✅ | — |
| আমাদের সাথে কাজের সুযোগ accordion (3 real panels) | `components/WorkWithUs.jsx` | ✅ | 4th legacy panel was lorem ipsum — correctly dropped |
| CTA boxes / fact counters / volunteers / testimonials / clients | — | ⛔ | All commented out in legacy |

#### `contact.html` → `/contact`

| Legacy element | React counterpart | Status | Action |
|---|---|---|---|
| Contact form (নাম / ইমেইল / বার্তা) | `pages/Contact.jsx` | ✅ | Adds `subject` + CTA-type awareness — an improvement |
| Address / phone / email blocks | `pages/Contact.jsx` + `data/siteData.js` | ✅ | Legacy `success.php` had a typo'd address email (`kolpojantro.org`) — do not propagate |
| Google Maps embed (`maps.google.com` script) | — | ❌ | Never rendered a map in legacy (script only, no container). Add a real, privacy-respecting map. **A-4** |

#### `registration.html` → `/registration`

| Legacy field | React field | Status |
|---|---|---|
| Name (required) | `name` | ✅ |
| Email (required) | `email` | ✅ |
| Phone (required) | `phone` (optional) | ⚠️ |
| Blood group (select, required) | — | ❌ |
| Current address (required) | `address` (optional) | ⚠️ |
| Recent academic institute (required) | — | ❌ |
| Occupation (select, required) | — | ❌ |
| Why do you want to join? (textarea) | folded into `message` | ⚠️ |
| Volunteering experience (textarea) | — | ❌ |
| Present organisation | — | ❌ |
| How did you hear about Kolpojontro? | — | ❌ |

**This is the single biggest functional regression in the conversion.** The legacy form captured 11 fields that map directly onto the legacy `AwaitingUser` / `ApplicationUser` model in `kj-registration` and onto the current `CtaSubmissionEntity` columns (`FirstName`, `LastName`, `Gender`, `ReasonForJoining`, `PresentOrganization`, `VolunteeingExperience`, `DateOfBirth`, `CityOfResidence`, `CountryOfResidence`). Restoring it is **A-5** and is a prerequisite for the P3 membership pipeline.

#### `success.php` → `/success`

| Legacy element | React counterpart | Status |
|---|---|---|
| Confirmation page after submit | `pages/Success.jsx` | ✅ — now CTA-type aware, an improvement |

#### `email.html`

Not a web page. **Move to the backend** as the seed for the member-acceptance / orientation email template (see `BACKEND_MASTER_PLAN.md`, P6 notification templates). Record it, then remove it from `legacy-html/` so nobody mistakes it for a missing route.

### 3.3 Parity backlog

| ID | Task | Priority | Est. |
|---|---|---|---|
| **A-1** | Decide the fate of `/events`: either build a static events page from real past events, or remove it from navigation until P5. No placeholder pages in production. | P0 blocker | 0.5 d |
| **A-2** | Replace social icons on the featured project with real share intents (Facebook, X, LinkedIn, WhatsApp, copy-link) using the current page URL. | High | 0.5 d |
| **A-3** | Fix footer copyright to a dynamic `2016–{currentYear}`; correct the stale contact email typo everywhere. | High | 0.25 d |
| **A-4** | Add a real location block to `/contact` — static map image + "Open in Google Maps / OpenStreetMap" link (no third-party script, no consent banner needed). | Medium | 0.5 d |
| **A-5** | Restore the full legacy registration form (11 fields, correct required-flags, bn/en labels, client-side validation, inline error messages). Field names must match the backend `AwaitingUser` contract. | **P0 blocker** | 2 d |
| **A-6** | Extract all hard-coded bn/en copy out of components into `src/content/*.json`, keyed by locale, in the shape the future Content API will return. | High | 2 d |
| **A-7** | Accessibility pass over the re-implemented interactive widgets: hero slider, project carousel, activity carousel, gallery lightbox — keyboard operation, focus management, `aria-live`, reduced-motion, visible focus rings. | High | 1.5 d |
| **A-8** | Visual regression sweep — legacy page vs React page, side by side, at 360 / 768 / 1024 / 1440 px, in both languages. Record findings in `docs/parity-report.md` with screenshots. | High | 1 d |
| **A-9** | Verify the Bengali font stack renders correctly (legacy relies on Poppins/Raleway from Google Fonts, neither of which covers Bengali) and add a proper Bengali webfont with `font-display: swap`. | High | 1 d |
| **A-10** | Remove `legacy-html/` from the deployed bundle; keep it in-repo under `docs/legacy/` for reference only, and confirm it is excluded from the build output. | Medium | 0.25 d |

### 3.4 Verification method

1. **Static inventory** — the matrix in §3.2, re-checked after each change.
2. **Side-by-side visual diff** — serve `legacy-html/` on a local static server, open the React route alongside, capture both at four breakpoints × two languages.
3. **Link and asset audit** — crawl the built `dist/` for broken internal links, missing images, and 404 assets.
4. **Content diff** — extract visible text from each legacy page and each React route, and diff, to catch dropped paragraphs.
5. **Sign-off** — `docs/parity-report.md` with an explicit per-row verdict, reviewed by a core member who knows the original site.

---

## 4. Workstream B — Repository READMEs

Two READMEs, written to the same template so contributors can move between repositories without re-learning the layout.

### 4.1 Shared structure

```
# <Project name>
<one-line description>
<badge row>

## Overview            — what this is, who it serves, how it fits the platform
## Screenshots / API preview
## Tech stack          — table: concern → technology → version
## Architecture        — diagram + short explanation
## Getting started     — prerequisites, clone, install, configure, run, verify
## Configuration       — every environment variable: name, required?, default, purpose
## Project structure   — annotated tree
## Development workflow— branching, commit convention, lint/format/test, PR checklist
## Testing
## Building for production
## Deployment          — per target environment, with rollback
## Troubleshooting     — symptom → cause → fix
## Roadmap             — links to the master plans
## Contributing
## License & contact
```

### 4.2 Badges

| Badge | Frontend | Backend |
|---|---|---|
| Build / CI status | GitHub Actions workflow badge | GitHub Actions workflow badge |
| Deployment status | Hosting provider badge | — (until P1 hosting exists) |
| Runtime version | `Node 22` | `.NET 10` |
| Framework | `React 19` · `Vite 8` | `ASP.NET Core` · `EF Core 10` |
| Code quality | ESLint · Prettier | `dotnet format` |
| Tests / coverage | Vitest coverage | xUnit coverage |
| License | License badge | License badge |
| Docs | Link to `FRONTEND_MASTER_PLAN.md` | Link to `openapi.v1.yaml` + `BACKEND_MASTER_PLAN.md` |

Badges must reflect reality. Do not add a coverage badge before tests exist — add the badge in the same PR that adds the workflow.

### 4.3 Frontend README — required specifics

- **Overview** — bilingual (bn/en) public site for Kolpojontro Foundation; static-first, progressively enhanced by the backend.
- **Prerequisites** — Node 22 LTS, npm 10.
- **Local development**

  ```bash
  npm install
  cp .env.example .env
  npm run dev
  ```

- **Environment variables** — full table for `VITE_CONTENT_API_URL`, `VITE_CTA_API_URL`, `VITE_EMAIL_TRACKING_API_URL`, `VITE_ANALYTICS_API_URL`, plus the new `VITE_ENABLE_*` capability flags. Must state explicitly: **all are optional; with none set, the site runs fully static.**
- **Static-only mode** — a dedicated section explaining exactly what works without a backend and what degrades (see §5.3).
- **Content editing** — how a non-developer edits `src/content/*.json` and news/projects data, and how a PR triggers a rebuild.
- **Deployment** — one subsection per supported target with the exact SPA-rewrite configuration (see §5.4).
- **Troubleshooting** — blank page after deploy (missing SPA rewrite / wrong `base`), Bengali text rendering as boxes (missing webfont), form submits failing (CTA endpoint unset in a build that expects it).

### 4.4 Backend README — required specifics

- **Overview** — .NET 10 microservices behind a YARP gateway, serving the public site and the back office.
- **Prerequisites** — .NET SDK 10.0.300, Docker Desktop (P1+), `dotnet-ef` tool.
- **Service map table** — service · port · responsibility · database · public/internal.
- **Run locally**

  ```bash
  dotnet restore KJWebsite.Backend.slnx
  dotnet build KJWebsite.Backend.slnx -c Debug
  ```

  Plus how to run all four services together, and the Scalar API reference URLs.
- **Cross-platform note** — replace the Windows-only `"C:\Program Files\dotnet\dotnet.exe"` guidance with platform-neutral commands and a short Windows footnote.
- **Database & migrations** — connection strings, `dotnet ef migrations add`, `dotnet ef database update`, and the SQLite → PostgreSQL plan.
- **Security notice** — a prominent, honest block stating that the current dev implementation uses plain-text passwords, in-memory access tokens, and a hard-coded `dev-admin-token`, that it is **not production-safe**, and that hardening is P1. Include the dev seed credentials and a warning never to reuse them.
- **API contract** — `openapi.v1.yaml` as the source of truth, how to regenerate, how the frontend consumes it.
- **Legacy folders** — `kj-registration/` and `kj-registration-AwaitingUserFeatures/` are read-only historical snapshots imported via git subtree; explain what to mine from them and what never to run.
- **Deployment** — container build, environment configuration, health checks, migration-on-deploy strategy, rollback.

### 4.5 README backlog

| ID | Task | Est. |
|---|---|---|
| **B-1** | Agree the shared README template and add it to `docs/`. | 0.25 d |
| **B-2** | Write the frontend README against the template. | 1 d |
| **B-3** | Write the backend README against the template, folding in the useful parts of `PROJECT_CONTEXT.md`. | 1 d |
| **B-4** | Decide the license (MIT / Apache-2.0 / proprietary) with the core members and add `LICENSE` to both repositories. | 0.25 d |
| **B-5** | Add `CONTRIBUTING.md`, `CODE_OF_CONDUCT.md`, and PR/issue templates to both repositories. | 0.5 d |
| **B-6** | Add architecture diagrams (Mermaid, rendered inline by GitHub) to both READMEs. | 0.5 d |
| **B-7** | Reconcile `PROJECT_CONTEXT.md` with the new README and `BACKEND_MASTER_PLAN.md` — keep it as an AI/onboarding context file, remove duplicated and now-stale content. | 0.25 d |

---

## 5. Workstream C — Standalone static site launch

### 5.1 Objective

A production URL, serving the full public site, built from `npm run build`, hosted on static hosting, with **no backend running** — and with the seams already cut so the backend can be plugged in later without refactoring.

### 5.2 Launch blockers

| ID | Blocker | Why it blocks | Fix |
|---|---|---|---|
| **C-1** | `ctaService.submitCtaForm` throws when `VITE_CTA_API_URL` is unset — every form on the site fails. | Registration, contact, footer contact, and all CTA journeys are dead on a static deploy. | Add a **static submission strategy**: pluggable transport with `api` \| `formspree`\|`static-mailto` \| `noop` modes, selected by env. Default to a hosted form endpoint (or `mailto:` composition) so submissions actually reach the organisation. |
| **C-2** | No SPA rewrite configuration. | Any deep link (`/about`, `/project/pathshala`) returns 404 on refresh. | Add host-specific rewrite config (§5.4) and a `public/404.html` fallback for hosts that need it. |
| **C-3** | Catch-all route redirects everything to `/`. | Broken/mistyped URLs silently land on the home page; bad for users and for SEO. | Add a real bilingual `NotFound` page with a 404-appropriate `<meta name="robots">`. |
| **C-4** | No `<head>` metadata at all — no favicon, description, canonical, OG/Twitter tags, or `lang` switching. | Unshareable on social media, unindexable, no browser tab icon. | Add a head-management layer with per-route metadata, plus favicon set and `og:image`. |
| **C-5** | 9.2 MB of unoptimised images ship to every visitor. | Unusable on mobile data in Bangladesh; fails any performance budget. | Convert to WebP/AVIF with responsive `srcset`, compress, lazy-load below-the-fold, and preload the hero. |
| **C-6** | `/events` is a placeholder page in the main navigation. | Ships a visibly unfinished site. | Resolve via **A-1**. |
| **C-7** | Registration form has lost 6 of 11 legacy fields. | The primary volunteer-recruitment funnel collects insufficient data. | Resolve via **A-5**. |
| **C-8** | No build verification of any kind. | A broken build can reach production unnoticed. | Add CI: install → lint → format check → build → link check → Lighthouse budget. |

### 5.3 Static-mode capability contract

This table goes verbatim into the frontend README and defines what "static" means.

| Capability | Static (P0) | With backend (P1+) |
|---|---|---|
| Home, About, Projects, Project detail | Build-time JSON | Content API |
| News/Blog list and detail | Build-time JSON | Content API + pagination |
| Gallery | Build-time asset manifest | Media library |
| Language switching (bn/en) | Full | Full |
| Contact / footer-contact form | Hosted form endpoint or mailto composition | CTA API + auto-acknowledgement email |
| Volunteer registration | Hosted form endpoint, full 11-field payload | Membership API + approval workflow |
| Donation | Informational page: bank details, bKash/Nagad numbers, and instructions | Online payment + receipt |
| Events | Static list of past/upcoming events | Events API + RSVP |
| Member login / portal | Hidden (flag off) | Enabled |
| Admin / back office | Not present | Separate authenticated area |
| Search | Client-side over the static index | Server-side search |
| Analytics | Client-side (GA4/Plausible) | Plus server-side events |

### 5.4 Hosting

**Recommendation: Cloudflare Pages.** Free tier, global CDN with good South Asia latency, native SPA rewrites, preview deployments per PR, easy custom domain + automatic TLS, and Cloudflare Workers/Functions available later for lightweight backend-adjacent needs. Netlify and Vercel are equivalent alternatives; GitHub Pages is viable but needs the `404.html` SPA hack and lacks preview deploys.

Required configuration per target:

| Host | Config |
|---|---|
| Cloudflare Pages | `public/_redirects` → `/*  /index.html  200`; build `npm run build`; output `dist` |
| Netlify | `netlify.toml` with a `[[redirects]]` block, `from = "/*"`, `to = "/index.html"`, `status = 200` |
| Vercel | `vercel.json` with `{ "rewrites": [{ "source": "/(.*)", "destination": "/index.html" }] }` |
| GitHub Pages | Copy `dist/index.html` → `dist/404.html`; set Vite `base` if served from a sub-path |
| Any static host / S3+CloudFront | Configure the "error document" to `index.html` with a 200 rewrite |

Also required: custom domain (`kolpojontro.org`), HTTPS enforced, `www` ↔ apex canonicalisation, HSTS, and security headers (`Content-Security-Policy`, `X-Content-Type-Options`, `Referrer-Policy`, `Permissions-Policy`).

### 5.5 Static-launch backlog

| ID | Task | Priority | Est. |
|---|---|---|---|
| **C-10** | Introduce `src/config/features.js` — a single runtime capability map read from `import.meta.env`, with every backend-dependent flag defaulting to `false`. | P0 blocker | 0.5 d |
| **C-11** | Refactor `ctaService` into a transport-strategy module (`api` / `formspree` / `mailto` / `noop`) with graceful degradation and no throw when unconfigured. Add success/error UI states to every form. | P0 blocker | 1.5 d |
| **C-12** | Introduce `src/api/` as the single network boundary; move `contentService` behind it and add an adapter that maps the live backend shape (`{ items: [...] }` per `?lang=`) to the app's `{ projects: { bn, en } }` shape. Document the mismatch. | High | 1 d |
| **C-13** | Add SPA rewrite configs for the chosen host + a real bilingual 404 page. | P0 blocker | 0.5 d |
| **C-14** | Add head/SEO management: per-route `<title>`, description, canonical, `hreflang` for bn/en, OG/Twitter cards, `favicon` set, `site.webmanifest`, `robots.txt`, generated `sitemap.xml`, and JSON-LD `NGO` + `NewsArticle` structured data. | P0 blocker | 1.5 d |
| **C-15** | Image pipeline: convert `public/img` to WebP/AVIF, generate responsive sizes, add `width`/`height` to prevent layout shift, lazy-load below the fold, preload the hero image. Target: home page ≤ 1.2 MB total transfer. | P0 blocker | 2 d |
| **C-16** | Font strategy: self-host Latin fonts, add a Bengali webfont, subset aggressively, `font-display: swap`, drop unused icon fonts (audit Font Awesome + flaticon + glyphicons usage — three icon fonts for one site is unjustifiable). | High | 1 d |
| **C-17** | Route-level code splitting with `React.lazy` + `Suspense`; add an app-level `ErrorBoundary` with a bilingual fallback. | High | 0.5 d |
| **C-18** | Set up CI (GitHub Actions): install → `lint` → `format:check` → `build` → broken-link check → Lighthouse CI against a budget (Performance ≥ 90, Accessibility ≥ 95, Best Practices ≥ 95, SEO ≥ 95 on mobile). | P0 blocker | 1 d |
| **C-19** | Set up hosting: connect the repository, configure production + preview environments, attach the custom domain, enforce HTTPS, add security headers. | P0 blocker | 0.5 d |
| **C-20** | Add privacy-respecting analytics (Plausible or GA4 with anonymised IP) behind a flag, plus a cookie/consent notice only if a cookie-setting provider is chosen. | Medium | 0.5 d |
| **C-21** | Add a static, informative **Donate** page (bank account, bKash/Nagad, mailing address, what donations fund) so the donate CTA leads somewhere useful before payments exist. | High | 1 d |
| **C-22** | Add a minimal test harness: Vitest + React Testing Library, with smoke tests for every route, the language switch, and each form's submit path. | High | 1.5 d |
| **C-23** | Default the site language to Bengali with English available (reverses decision D5 regression); persist choice; set `<html lang>` and `hreflang` correctly. | High | 0.5 d |
| **C-24** | Write `docs/RUNBOOK.md`: how to deploy, how to roll back, how to add a news post or project without a developer. | Medium | 0.5 d |

---

## 6. Milestones and sequencing

Estimates assume **one full-time frontend developer** plus part-time review. Two developers can run A and C largely in parallel.

| Milestone | Contents | Exit criteria | Est. |
|---|---|---|---|
| **M0 — Groundwork** (week 1) | C-10, C-12, C-11, B-1 | Site builds and runs with zero env vars set; no form throws | 4 d |
| **M1 — Parity restored** (weeks 1–2) | A-1, A-2, A-3, A-5, A-6, A-9 | Parity matrix has no ❌; registration form complete; copy externalised | 7 d |
| **M2 — Hostable** (week 3) | C-13, C-14, C-17, C-19 | Deep links work on the deployed preview URL; social share preview renders | 3 d |
| **M3 — Fast and accessible** (weeks 3–4) | C-15, C-16, A-7, A-4 | Lighthouse mobile ≥ 90/95/95/95; keyboard-only walkthrough passes | 5 d |
| **M4 — Verified** (week 4) | A-8, A-10, C-22 | `docs/parity-report.md` signed off; smoke tests green | 3 d |
| **M5 — Documented and shipped** (week 5) | B-2, B-3, B-4, B-5, B-6, B-7, C-18, C-20, C-21, C-23, C-24 | Custom domain live; CI enforcing gates; both READMEs merged | 5 d |

**Total: ~5 weeks for one developer, ~3 weeks for two.**

Backend work runs in parallel throughout — see `BACKEND_MASTER_PLAN.md` P0, which has no frontend dependency.

---

## 7. Definition of Done for Phase 0

Phase 0 is complete when **all** of the following hold:

- [ ] `https://kolpojontro.org` (or the agreed domain) serves the React site from static hosting.
- [ ] Every route deep-links and refreshes correctly; unknown URLs render a real 404 page.
- [ ] No backend is required for any user-facing journey; every form reaches a real inbox.
- [ ] The parity report shows zero unexplained regressions against the legacy site, signed off by a core member.
- [ ] The registration form captures the full legacy field set, with field names matching the backend contract.
- [ ] Lighthouse mobile: Performance ≥ 90, Accessibility ≥ 95, Best Practices ≥ 95, SEO ≥ 95.
- [ ] Home page total transfer ≤ 1.2 MB; largest single image ≤ 200 KB.
- [ ] Keyboard-only and screen-reader walkthrough of the home page, registration, and contact passes.
- [ ] Both languages render correctly, including Bengali typography, with Bengali as the default.
- [ ] CI blocks merges on lint, format, build, link check, and the Lighthouse budget.
- [ ] Both repositories have complete, accurate READMEs with working badges, plus `LICENSE` and `CONTRIBUTING.md`.
- [ ] `docs/RUNBOOK.md` lets a non-author deploy and roll back.
- [ ] Feature flags exist for every planned backend-dependent capability and are all `false` in production.

---

## 8. Risks and mitigations

| Risk | Impact | Likelihood | Mitigation |
|---|---|---|---|
| The legacy theme CSS (Bootstrap 3, jQuery-era) fights modern responsive work and blocks the accessibility target | High | High | Accept for P0 (D4); scope a design-system replacement as a dedicated P2/P8 track with a component-by-component migration, never a big-bang rewrite |
| Content owners cannot supply real team/testimonial/statistics content, tempting the team to re-introduce placeholder data | Medium | Medium | D6 is non-negotiable — sections stay absent until content exists |
| No one on the team can verify what the original Bengali site said | High | Medium | Content diff (§3.4 step 4) is automated; escalate ambiguous passages to a core member before changing them |
| Frontend and backend content contracts drift further apart | High | High | The adapter in C-12 plus `openapi.v1.yaml` as the single contract; contract tests from P1 |
| Static form provider becomes a spam target or hits free-tier limits | Medium | Medium | Honeypot + rate limiting at the provider; migrate to the CTA API at P1 |
| 11 MB of assets makes the repository and every clone slow | Low | High | Optimise in C-15; consider Git LFS or moving originals out of the repository |
| Solo-developer bus factor | High | Medium | Everything is written down in these three plans; enforce PR review and keep the runbook current |

---

## 9. Open questions for the organisation

These need answers from core members; each blocks a specific task.

1. **Domain** — Is `kolpojontro.org` under the organisation's control, and who holds DNS? *(blocks C-19)*
2. **Events page** — Do real events exist to publish now, or should `/events` be removed from navigation until P5? *(blocks A-1, C-6)*
3. **Donation details** — What bank account, bKash/Nagad numbers, and instructions should the static donate page show? *(blocks C-21)*
4. **Legal status** — Registration number, governing-body details, and audited-accounts availability, for the transparency page and donor trust. *(blocks P7)*
5. **License** — Public open-source or proprietary? *(blocks B-4)*
6. **Contact routing** — Which inbox receives contact-form and registration submissions, and who is responsible for responding? *(blocks C-11)*
7. **Team and testimonials** — Can real member profiles and partner logos be supplied, and with what consent? *(blocks P6 planning)*
8. **Content freshness** — The newest content on the site is from 2017. Is there 2018–2026 activity to publish at launch? *(materially affects credibility at launch)*
9. **Back-office users** — How many core members will use the admin dashboard, and what roles do they need? *(shapes P2 RBAC design)*
10. **Payments** — Which payment providers are realistically obtainable (bKash Merchant, Nagad, SSLCommerz, Stripe, PayPal)? *(shapes P4)*
