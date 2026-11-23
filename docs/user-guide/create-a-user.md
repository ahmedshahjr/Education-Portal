# Create a user

**Previous:** [Create a role](create-a-role.md). **Next:** [Whole-school items per campus](whole-school-items-per-campus.md).

## What this does

Adds a person to your school: a principal, a teacher, other staff or a student. You can also create their **sign-in account** with an email and a one-time password, and then give them campuses and roles.

## Who can do it

A **school administrator**, with the **Staff** module on (for adding people) and **Users & Roles** on (for roles and campuses).

## Before you start

- The school has its campuses (see [Add a campus](add-a-campus.md)) and its roles (see [Create a role](create-a-role.md)).
- Decide the person's **type**: *Principal*, *Teacher* or *Staff*.
- For a sign-in account: the person's **email** and a **one-time password** of at least 15 characters. Pass it on privately; the person must choose their own password on first sign-in.

## Steps

1. Sign in as the school administrator.
2. In the menu, select **Users**.
3. Select **Add user**. This opens the page **Add a user**.
4. Type the **First name** and **Last name**. Optionally type the **Middle name**.
5. Choose the **Type**: *Principal*, *Teacher* or *Staff*.
6. Type the **Phone**: 7 to 15 digits, with an optional leading `+`.
7. For a sign-in account, type the **Email** and a **One-time password**, or leave them empty to add the person without an account.
8. Under **Campuses**, choose the campuses the person works in. Administrators see every campus; other users see only their assigned campuses.
9. Under **Roles**, tick the roles the person has. Their rights combine.
10. Select **Save user**.
11. You see *User added.* and the person in the list.
12. Give the person their email and one-time password privately. They sign in (see [Sign in](sign-in.md)) and choose their own password.

To change a person later, open them in the list and select **Save user**. To delete, select **Delete** and confirm: the person is hidden from the school. A principal of a campus must be replaced there first.

## What each right allows

Adding a user needs the school administrator role and the **Staff** module. Giving campuses and roles needs **Users & Roles**. A user only acts on data in their assigned campuses, and only with the rights their roles give.

## If something goes wrong

| What you see | Cause | What to do |
|---|---|---|
| *This field is required.* | A required name is empty. | Fill it in. |
| *Enter a valid email address.* | The email is malformed. | Correct it. |
| *The email is already in use.* | Another account has this email. | Use a different email, or find the existing account. |
| *Use 15 to 128 characters.* | The one-time password is too short or too long. | Use a password of 15 to 128 characters. A weak password is refused for both the person and the account. |
| *Use digits with an optional leading +, spaces, hyphens or brackets: 7 to 15 digits.* | The phone number is malformed. | Correct it. |
| *A principal of a campus must be replaced there first.* | You are deleting the principal of a campus. | Choose another principal for the campus first (see [Add a campus](add-a-campus.md)). |
| 403 Forbidden | You are not an administrator, the **Staff** module is off, or you are acting on a campus you do not belong to. | See [Enable modules for a school](enable-modules-for-a-school.md). |

## Worked example

The school administrator adds *Ada Example*, type *Teacher*, phone `+92 300 0000000`, email `ada.example@example.test` with a one-time password. She gives Ada the *Example Main Campus* and the *Teacher* role, and selects **Save user**. Ada signs in on the school's domain and chooses her own password.

## Flow recap

*Campuses and roles exist, then Users, then Add user with name, type, phone, optional account, campuses and roles, then Save user; the person signs in and changes the one-time password.*

**Not verified in the running app:** the exact wording of the messages for duplicate emails, and whether a user without an account can be added by a school administrator (the code allows it).
