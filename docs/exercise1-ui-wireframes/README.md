# Exercise 1 — Wireframes & Kanban Board

Deliverables for **Tasks 1 (graded)** — UI Work exercise.

## Contents

| File | What it is |
|---|---|
| `wireframe-variant-a.svg` | Speed-optimised layout (sidebar + table) |
| `wireframe-variant-b.svg` | Clarity-optimised layout (search bar + chips + cards) |
| `annotations.md` | Design rationale (3 annotations per variant) |
| `kanban-tasks.md` | 6 tasks for the GitHub Projects board |
| `README.md` | This file — overview + setup steps |

## Feature: Tour Search & Filter

I chose **Tour Search & Filter** as the small UI feature for this exercise. The current `tourplanner-frontend` has a flat tour list without any search or filter UI, so this is a real gap, and the topic offers genuine UX trade-offs to compare (sidebar vs chips, live search vs submit, empty-state handling, reset behaviour).

The two wireframe variants share the same content (header, breadcrumb, 6–7 tours, action buttons) but apply different layout strategies — one optimised for repeat power users, one for first-time browsers.

## GitHub Projects Board — Setup

1. Open https://github.com/Dashaev-Amin/SWEN-Tourplaner
2. Click the **Projects** tab → **New project** → choose the **Board** template
3. Name it `Exercise 1 — Tour Search & Filter`, create
4. The default columns (**Todo / In Progress / Done**) are fine
5. For each task in `kanban-tasks.md`:
   - Click **+ Add item** → **+ Create new issue**
   - Title and body: copy from the task in `kanban-tasks.md`
   - Add a label for priority: `priority:high`, `priority:medium`, or `priority:low`
   - For tasks marked **Refers to:** include a link in the issue body, e.g.:
     ```
     [Variant A](../blob/main/docs/exercise1-ui-wireframes/wireframe-variant-a.svg)
     ```
6. Drag the **"Create wireframes and annotations"** task to **Done** (it's already complete)
7. Leave the rest in **Todo**

## Quick check before submission

- [ ] Both SVG files render correctly when opened in browser
- [ ] At least one task on the board has an acceptance criterion (Task 3 does)
- [ ] At least one issue is linked to the wireframe files
- [ ] Board is visible to the course assessor (set project visibility accordingly)
