# ADR-013: Invoices and Manual Payments

## Status

Accepted.

## Date

2026-11-13

## Context

Phase 5 of eight of the module-selling work. A school's `Subscription` ([ADR-011](ADR-011-Subscriptions-And-Tenant-Modules.md))
holds a price and currency copied from its country's price book. The platform owner now needs to bill it and to record
that the money arrived. There is no card gateway yet: payments are bank transfers, cash, cheques and wallets that the
master tenant confirms by hand. Card payments, when added, follow the PCI DSS skill and need a merchant-model decision.

## Decision

- **`Invoice`** (table `Invoices`, tenant-owned: `SchoolId`, tenant query filter): one subscription period. It copies the
  currency, the subtotal (the subscription's price), the **tax rate** and the computed tax amount and total, so a later
  price or tax change never alters it. Status Open, Paid or Void (lookup table). Issue date, due date (30 days),
  period start and end, paid date. The **number** (`INV-000123`) comes from the database sequence `InvoiceNumbers`: unique
  and readable, not gap-free and shared by all schools.
- **`InvoiceLine`** (table `InvoiceLines`): description and amount before tax. Today an invoice has one line (plan code,
  billing cycle and period dates, no translatable sentence), because the subscription stores one total. More lines come
  when adjustments or itemised add-ons are needed.
- **Tax rate** is a percentage (0 to 100, two decimals) on the country's **price book** (`PriceBooks.TaxRate`, default 0),
  set with `PUT /api/PriceBooks/{id}` (`taxRate`, left out means unchanged). A school can override it: `Schools.TaxRateOverride`
  (nullable, the only other new column on `School`), set with `PUT /api/Schools/{id}/tax-rate` (null clears it, 0 is a real
  rate), audited as `SchoolTaxRateChanged`. The invoice uses the school's rate if set, else the country's. Tax is rounded to two decimals, away from zero.
- **Which period** (`nextPeriod`): false invoices the current period (not allowed on a trial), true the period after the
  current end (a renewal, or the first paid period after a trial). A subscription has at most one open invoice and a
  period is invoiced once unless the first invoice was voided (two partial unique indexes).
- **`Payment`** (table `Payments`, tenant-owned): amount and currency (copied from the invoice), method (BankTransfer,
  Cash, Cheque, MobileWallet, Other; lookup table), the reference typed in, the received date (not in the future) and the
  name of the provider. **A payment covers the whole invoice total**; part payments and refunds are not supported.
- **`IPaymentProvider`** (Application interface, `ChargeAsync`) is the seam for a future gateway. The only implementation
  is `ManualPaymentProvider`, which accepts any payment that has a reference and an amount.
- **Paying activates or extends the subscription** (`Subscription.ApplyPayment`), in the same `SaveChanges` as the payment
  and the paid invoice: an invoice for a period beyond the current end moves the end there and extends every module's
  `ValidTo` (a trial or renewal becomes Active); an invoice for the current period brings a PastDue or Suspended
  subscription back to Active and reactivates its Suspended modules. A cancelled subscription is never changed. The
  school's cached entitlements are dropped afterwards.
- **API** (master tenant administrators only, audited as `InvoiceCreated`, `InvoiceVoided`, `PaymentRecorded`):
  `GET/POST /api/Invoices`, `GET /api/Invoices/{id}`, `POST /api/Invoices/{id}/payments`, `POST /api/Invoices/{id}/void`.
  Schools do not read their invoices yet; the tenant side ("My subscription") comes with phase 6.

## Rationale

- Copying amounts, currency and tax onto the invoice is the same idea as for the subscription: history is stable.
- One save is the transaction: a payment cannot be stored without the invoice turning Paid and the subscription following.
- A provider interface costs one small type now and keeps a gateway out of the billing rules later.
- Whole-total payments keep the first version simple and make "paid" unambiguous.

## Consequences

**Easier:** the lifecycle job (phase 7) only sets PastDue or Suspended and issues renewal invoices; this phase already
brings a subscription back when its invoice is paid.

**Harder / open:**
- A subscription whose period has already ended and is paid for the *current* period is set Active but its module dates
  are not moved forward (they are still expired). Restarting a lapsed period from the payment date is decided in phase 7.
- A renewal invoice uses the subscription's stored price, not the price book's current price.
- Invoice numbers are always unique but can have gaps and are global (maintainer accepted this, 2026-11-13); a jurisdiction that
  requires gap-free numbers per seller would need a different scheme.
- No credit notes, part payments or refunds, and no tax-inclusive pricing.
