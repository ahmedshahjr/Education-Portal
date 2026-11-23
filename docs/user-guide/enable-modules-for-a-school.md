# Enable modules for a school

**Previous:** [Create a school](create-a-school.md). **Next:** [Add a campus](add-a-campus.md).

## What this does

Decides which parts of the portal a school may use (its **modules**), by its subscription. Core modules, such as *Campuses* and *Users & Roles*, are part of every subscription. Paid modules, such as *Fees* or *Exams*, are on only when the school's subscription includes them.

A school that is switched off for a module is refused with the error `MODULE_NOT_ENABLED`, and the module's data is kept, not deleted.

## Who can do it

Only the **master tenant administrator**. A school administrator sees which modules the school has, but cannot change them. A school administrator who needs a paid module asks for it as an add-on request, which the master tenant approves or rejects.

## Before you start

- The school must exist (see [Create a school](create-a-school.md)).
- Know which plan the school bought (Starter, Standard or Full), or which add-on modules it needs.
- The catalogue of modules is set under **Modules** in the master menu. Core and paid status are set there too.

## Steps

1. Sign in as the master tenant administrator.
2. In the menu, select **Schools**, then open the school.
3. Select the tab **Plan & modules**.
4. Review the school's plan and modules. Change the plan, or add or remove add-on modules, as the school's contract says.
5. Check the **Modules** list on the same tab: each module the school has is on, and each one it does not have is off.
6. For a school's add-on request, open **Add-on requests** in the master menu. Select **Approve** to add the module at once, or **Reject**.

*Not verified in the running app:* the exact controls on the **Plan & modules** tab. The steps follow the page's tab label and the API for the school's subscription and modules.

## What each right allows

Module switches are master tenant only. A school's own roles can only give rights in modules the school has now (see [Create a role](create-a-role.md)). Rights in a module that is switched off are kept, but hidden, and come back when the module is on again.

## If something goes wrong

| What you see | Cause | What to do |
|---|---|---|
| `MODULE_NOT_ENABLED` (403) | The school does not have the module. | Ask the master tenant administrator to add it, or to approve the school's add-on request. |
| A menu item is missing for the school | The module is off for the school. | Same as above. |
| A module cannot be turned on | It requires another module that is off (for example *Fees* requires *Students*). | Turn on the required module first. |

## Worked example

`Example School` buys the Standard plan. The master administrator opens **Schools**, opens `Example School`, opens **Plan & modules**, and checks that *Students*, *Exams* and *Fees* are on as the plan says. Later the school asks for *Library*; the administrator approves the request under **Add-on requests**, and *Library* turns on at once.

## Flow recap

*Master opens the school, then Plan & modules, then checks the plan and add-ons; a school asks for a paid module; the master approves or rejects it.*
