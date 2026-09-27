# PR Review — AI Requirement Phase 1

Date: 2026-09-27  
PR: #1 — `feature/ai-requirement-phase1` → `main`

## Scope reviewed

- AI requirement endpoint and DTO contract.
- Development mock implementation.
- Real OpenAI implementation boundary.
- Dependency injection / environment behavior.
- Existing requirement save, OTP, image upload and admin-review flow impact.
- Frontend contract compatibility with `StructuredRequirement`.
- Security and billing behavior.

## Review result

### Safe for current local Phase 1 testing

- `POST /api/ai/requirements/generate` returns the same shape expected by the frontend.
- No database schema or EF migration is required.
- Existing requirement persistence is not changed.
- Existing OTP, image upload and admin-review workflow is not changed.
- Country and City remain required editable fields.
- Budget remains editable and optional.
- Development/mock response preserves the original buyer text in `requirementDetails`.
- AC test case supports quantity constraints such as "more than 40" and room size such as "12x12".

### Safety change made during review

Real paid AI is now **opt-in**, not environment-driven.

Default behavior:
- `OpenAI:Enabled=false` or missing → `DevelopmentAiRequirementService`.
- `OpenAI:Enabled=true` → `OpenAiRequirementService`.

This prevents an accidental production deployment from immediately consuming paid API usage before billing, keys and abuse controls are intentionally configured.

## Before enabling real paid AI

Do not set `OpenAI:Enabled=true` in production until all of these are complete:

- Configure the API key through secrets/environment configuration; never commit it.
- Confirm the production model name supported by the configured OpenAI account.
- Add request throttling/rate limiting to the generation endpoint.
- Decide whether anonymous generation remains allowed. The current UX generates before login, so changing this requires a product-flow decision.
- Add provider timeout/retry handling and map provider failures to an appropriate server error instead of a buyer validation error.
- Add telemetry for request failures without logging API keys or sensitive buyer data.
- Add integration tests for malformed/empty provider output.

## Manual smoke test

1. Keep `OpenAI:Enabled` unset/false.
2. Start API.
3. Start frontend with the AI Phase 1 frontend changes.
4. Enter: `Mujhe ek achi company ka AC chahiye, quantity 40 se zyada ho aur room 12x12 hai.`
5. Verify:
   - title is AC-specific;
   - quantity is `More than 40`;
   - room size is `12x12`;
   - Country and City are visible and editable;
   - Budget is visible and editable;
   - form can still be manually edited;
   - image selection works;
   - submit continues through the existing auth/OTP/create/image/admin-review flow.

## Known limitation

The development service is intentionally a deterministic mock and uses limited parsing. It is **not** a substitute for testing semantic AI quality. Its purpose is to validate the complete application integration without API billing.

## Merge decision

Approved for Phase 1 mock/local integration after the safety change above. Real paid AI remains disabled by default.
