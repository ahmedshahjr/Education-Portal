# Pseudocode: core logic for mission msn_cb3148a74d68cbf6ccc3e5d2

Task `t2`. Covers every acceptance criterion of `mission-msn-cb3148a7-ui-entities.md` (AC1.1 to AC5.3). Open questions OQ1 to OQ5 are unresolved; each step that depends on one says which assumption it uses.

## 1. Entity list screens (AC1.1 to AC1.4)

```
ENTITY_SCREENS = [ every core entity with a backend list endpoint ]   # OQ1: assumed School, Campus, UserDetails only

function buildMenu(user, school, enabledModules):
    items = []
    for screen in ENTITY_SCREENS:                                      # AC1.1
        if user.isMasterAdministrator and screen.masterOnly or
           user.isSchoolAdministrator and not screen.masterOnly:
            if screen.module is null or enabledModules contains screen.module:   # AC1.4 (client mirror)
                items.add(screen)
    return items

function loadRows(screen, query):                                      # AC1.2, AC1.3
    state = LOADING
    response = api.list(screen.endpoint, query)
    if response is error:
        state = ERROR(firstProblemMessage(response))                   # AC1.3
    else if response.items is empty:
        state = EMPTY                                                  # AC1.2
    else:
        state = READY(response.items)
```

Server side (AC1.4): every new use case declares `[Authorize(Module = ...)]`. The request rules (`RequestRules`) refuse a disabled module with `MODULE_NOT_ENABLED`; the client menu only hides it.

## 2. Trial banner (AC2.1 to AC2.4)

```
function trialDaysLeft(trialEnd, now):                                 # AC2.3
    if now >= trialEnd: return 0
    return floor((trialEnd - now) in days)                             # rounded down, never negative

function shouldShowTrialBanner(user, subscription, now):               # AC2.1, AC2.2, AC2.4
    if user.isMasterTenant: return false                               # AC2.4
    if subscription is null: return false                              # AC2.2
    if not subscription.isTrial: return false                          # AC2.2
    return true                                                        # AC2.1

layout renders:
    if shouldShowTrialBanner(...):
        render <app-trial-banner days=trialDaysLeft(...)> above <router-outlet>
```

## 3. Toasts (AC3.1 to AC3.3)

```
function notifySuccess(messageKey):                                    # AC3.1
    snackBar.open(translate(messageKey), undefined, { duration: 3000 })

function notifyError(problem):                                         # AC3.2
    snackBar.open(firstProblemMessage(problem), undefined, { duration: 3000 })

# every create, update, delete:
on success -> notifySuccess(screen.successKey)
on error   -> notifyError(error)
# AC3.3: every messageKey and every problem message is a translated string in en, ar, ur (checked by translations.spec.ts)
```

## 4. Campus filter (AC4.1 to AC4.4)

```
function campusFilterVisible(campuses):                                # AC4.1
    return count(campuses) > 1

function applyCampusFilter(rows, selectedCampusId):                    # AC4.2
    if selectedCampusId is null: return rows                           # "All campuses"
    return rows where row.campusId == selectedCampusId

function campusChoices(school):                                        # AC4.3
    return api.get("/api/Campuses")                                    # server returns only campuses the user may act on (OQ5)

on campus chosen(campusId):                                            # AC4.4
    if screen.isCampusScoped:
        navigate("/campuses/" + campusId + screen.path)
    else:
        set in-memory filter only
```

Server side (AC4.3): the campus check is applied on every request by the server, not by the client filter.

## 5. Cross-cutting (AC5.1 to AC5.3)

```
# AC5.1: CI runs dotnet build (warnings as errors) and npm run build; a failing step fails the change.
# AC5.2: for every new use case, a functional test that:
#        - denies another school's data (tenant),
#        - denies a campus the user does not belong to (campus),
#        - denies a disabled module (MODULE_NOT_ENABLED).
# AC5.3: no existing test is removed or skipped to pass the suite. A failing test is fixed or reported, never deleted.
```

## 6. Order of work

1. Campus filter and the campus list endpoint assumption (OQ5), because the list screens depend on it.
2. Toast wrapper, then migrate the features to it (AC3).
3. Trial banner placement and `trialDaysLeft` unit tests (AC2).
4. Entity screens after OQ1 is answered (AC1).
