# Create a school

**Previous:** [Sign in](sign-in.md). **Next:** [Enable modules for a school](enable-modules-for-a-school.md).

## What this does

Adds a school to the platform, with its domain, its country and its first administrator. This is done by the platform's master tenant. Schools can also request to join, and the master tenant approves the request.

## Who can do it

Only a **master tenant administrator**. A school administrator cannot create another school.

## Before you start

- The school's **domain**, for example `school1.com` (no `http://`, no path). For local testing, a name ending in `.localhost`.
- The school's **country**. It must have a price book, because the country sets the school's languages, time zone, currency and price.
- The **email** of the school's first administrator.
- A **one-time password** of at least 15 characters. Pass it on privately. The administrator must change it on first sign-in.
- The master tenant is never limited by modules, so no module needs to be on for you to add a school.

## Steps

1. Sign in on the platform's own domain as the master tenant administrator.
2. In the menu, select **Schools**.
3. Select **Add school**.
4. Under *School*, type the **School name**.
5. Type the **Domain**, for example `school1.com`. The hint says to use a name ending in `.localhost` locally.
6. Choose the **Country**. The hint says it sets languages, time zone, currency and price book. Choose *None (English only, UTC)* only when the school has no country.
7. Optionally choose a **Data region**. The default is *Not decided yet*.
8. Under *First administrator*, type the **Administrator email**. The school's administrator signs in with it on the school's domain.
9. Type a **One-time password**, or select **Generate**. Copy it and pass it on privately.
10. Select **Add school**.
11. The school appears in the **Schools** list. Give the administrator their email and one-time password.

The administrator then signs in (see [Sign in](sign-in.md)) and must choose a new password first.

## What each right allows

This page is for the master tenant administrator only. The school's administrator does not create schools; they manage only their own school's campuses, users and roles.

## If something goes wrong

| What you see | Cause | What to do |
|---|---|---|
| *Enter the school's name.* | The name is empty. | Type the school's name. |
| *Enter a host name such as school1.com, without http:// or a path.* | The domain has `http://`, a path, or is not a host name. | Type only the host, for example `school1.com`. |
| *Enter a valid email address.* | The administrator email is malformed. | Correct it. |
| *Use 15 to 128 characters.* | The one-time password is too short or too long. | Select **Generate** or type a password of 15 to 128 characters. |
| The school is refused at sign-in (404) | The school is inactive, or its domain is not set. | Check the school in **Schools**. An unknown host is never served as a default school. |

## Worked example

The master administrator adds `Example School`, domain `school1.example.test`, country *Pakistan*, administrator `ada.example@example.test`, and a generated one-time password. Ada gets the password from a private message and signs in on `school1.example.test`.

*Note: the example domain is invented. Use your own domain in real work.*

## Flow recap

*Master signs in, then Schools, then Add school with domain, country and first administrator, then the administrator signs in and changes the one-time password.*

**Not verified in the running app:** the exact wording and layout of the Add school form were taken from the translation file and the form code; check the screen once before relying on the exact labels.
