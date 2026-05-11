# Kanban Board — Feature: Tour Search & Filter

Workflow phases: **Wireframe → Implement → Review → Test**

Owners are mapped to roles rather than people since this is a solo project — each role represents a hat I'm wearing for that step.

- **Design** — wireframing, layout decisions
- **Dev** — implementation (frontend + backend wiring)
- **Reviewer** — self-review, design sign-off
- **QA** — testing, edge cases

---

## Task 1: Create wireframes and annotations for Tour Search & Filter

- **Priority:** High
- **Owner:** Design
- **Column:** Done
- **Description:** Sketch two low-fidelity variants of the Tour Search & Filter screen (speed-optimised vs clarity-optimised) and add three annotations per variant explaining the key layout decisions.
- **Refers to:** `wireframe-variant-a.svg`, `wireframe-variant-b.svg`, `annotations.md`

---

## Task 2: Decide layout variant

- **Priority:** Medium
- **Owner:** Reviewer
- **Column:** Todo
- **Description:** Compare both wireframes against the actual TourPlanner user (mostly power users planning recurring tours). Pick one variant — or document a hybrid — and note the reasoning in this issue's comments.

---

## Task 3: Implement TourSearchBar component

- **Priority:** High
- **Owner:** Dev
- **Column:** Todo
- **Description:** Build a reusable search input placed above the tour list. Plays nicely with the existing tour-list component.
- **Acceptance criterion:** *The user can type into the search bar and the tour list re-filters live with a 300ms debounce, matching tour names case-insensitively.*

---

## Task 4: Implement filter UI (panel or chips)

- **Priority:** High
- **Owner:** Dev
- **Column:** Todo
- **Description:** Build the filter UI based on the chosen variant — sidebar with full controls (Variant A) or chip row with "More Filters" modal (Variant B). Filters needed: Distance range, Difficulty (multi-select), Date range, Min Rating.
- **Refers to:** `wireframe-variant-a.svg`, `wireframe-variant-b.svg`

---

## Task 5: Wire up filter UI to backend

- **Priority:** Medium
- **Owner:** Dev
- **Column:** Todo
- **Description:** Extend the existing `GET /api/tours` endpoint to accept query parameters for search and filters. Wire the frontend service to send the params and update the displayed tour list on change.

---

## Task 6: Test empty states, reset, and edge cases

- **Priority:** Medium
- **Owner:** QA
- **Column:** Todo
- **Description:** Verify the screen handles edge cases — no matching results shows a clear "No tours match your filters" message, reset button clears all filters and reloads the full list, very long tour names don't break the layout, and filter combinations produce the correct results. Compare visually against the chosen wireframe variant.

---

## How to add these to the GitHub Projects board

See `README.md` in this folder for step-by-step setup.
