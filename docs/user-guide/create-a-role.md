# Create a role

**Previous:** [Set campus codes](set-campus-codes.md). **Next:** [Create a user](create-a-user.md).

## What this does

Creates a role for your school and sets what the role may do in each module: **View**, **Create**, **Edit**, **Delete**, **Approve** and **Export**. Roles are given to users, who can hold several roles. Their rights combine.

## Who can do it

A **school administrator**, with the **Users & Roles** module on.

## Before you start

- Know which modules the school has (see [Enable modules for a school](enable-modules-for-a-school.md)). You can only give rights in modules the school has now.
- Decide which campuses the role applies to, if the school has more than one campus.

## Steps

1. Sign in as the school administrator.
2. In the menu, select **Roles**, then **Add role**. This opens the page **Add a role**.
3. Type the **Role name**.
4. Under **Rights per module**, find each module and tick the rights the role needs. Every right needs **View**; tick **View** first.
5. If the school has more than one campus, use the **Campuses** column: tick **All campuses**, or choose the campuses the rights apply to.
6. Select **Add role**.
7. You see *Role added.* and the role in the list.

To change a role, open it in the list. To delete a role, select **Delete** and confirm: the staff who had it lose its rights at once.

The built-in roles *Principal*, *Teacher*, *Student* and *Parent* can have their rights and description changed, but keep their name and cannot be deleted.

## What each right allows

| Right | Meaning |
|---|---|
| View | See the records in the module. Needed for every other right. |
| Create | Add new records in the module. |
| Edit | Change records in the module. |
| Delete | Remove records in the module. |
| Approve | Approve a request in the module, where the module has one. |
| Export | Export records from the module, where the module supports it. |

A teacher only grades their own courses, and parents only see their own children, whatever their rights say. Students and parents have read-only access.

## If something goes wrong

| What you see | Cause | What to do |
|---|---|---|
| *This field is required.* | No role name. | Type a name. |
| A right is greyed out | The module is off for the school, or you need View first. | Tick **View**, or ask the master tenant administrator. |
| Rights in a module disappear from the list | The module was switched off. Its rights are kept and come back when it is on again. | Ask the master tenant administrator to turn the module on. |
| 403 Forbidden | You are not an administrator, or the **Users & Roles** module is off. | See [Enable modules for a school](enable-modules-for-a-school.md). |

## Worked example

The school administrator adds the role *Front Office*. Under **Rights per module**, for **Students** they tick **View**, **Create** and **Edit**, and for **Users & Roles** only **View**. The role applies to *All campuses*. They select **Add role**. A new user with this role can add and change student records, and see the users list, but cannot delete students.

## Flow recap

*Module on for the school, then Roles, then Add role with name and rights per module (View first), then Add role; a user is given the role in Create a user.*

**Not verified in the running app:** the exact layout of the rights table and the campus column, which shows only when the school has more than one campus.
