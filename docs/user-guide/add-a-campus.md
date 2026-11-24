# Add a campus

**Previous:** [Enable modules for a school](enable-modules-for-a-school.md). **Next:** [Set campus codes](set-campus-codes.md).

## What this does

Adds a campus to your school. Campuses are where day-to-day data lives: students, teachers, courses and enrolments belong to a campus. Each campus has contact details and a place on the map.

## Who can do it

A **school administrator** of the school, with the **Campuses** module on. Master tenant administrators do not have school menus, including campuses.

## Before you start

- The **Campuses** module must be on for your school (see [Enable modules for a school](enable-modules-for-a-school.md)).
- Have the campus's address, the state or province and city (chosen from a list), the postal code, at least one phone number, a school number and a short name.
- Have the campus's location: the place on the map. You can also type the latitude and longitude.

## Steps

1. Sign in as the school administrator.
2. In the menu, select **Campuses**.
3. Select **Add campus**. This opens the page **Add a campus**.
4. Type the **Campus name**. Optionally type the **Local name** and choose the gender policy.
5. Type the **Address**.
6. Choose the **State or province**, then the **City**. The lists come from the location data.
7. Type the **Postal code**.
8. Under **Phone numbers (one to five)**, type a **Phone number**. Select **Add a phone number** for more, up to five.
9. Optionally type the **Website**. It must start with `https://` or `http://`.
10. Type the **School number** and the **Short name**.
11. Choose the **Principal** if one is already added under **Users**. Otherwise leave *No principal yet* and add one later.
12. Place the campus on the **Location on the map**: click the map or drag the pin, or type the **Latitude** and **Longitude**.
13. Select **Save**.
14. You see *Campus added.* and the campus in the list.

## What each right allows

Adding a campus needs the school administrator role and the **Campuses** module. The campus is then visible only to people assigned to it, or to administrators, who see every campus.

## If something goes wrong

| What you see | Cause | What to do |
|---|---|---|
| *This field is required.* | A required field is empty. | Fill in the field. |
| *Add at least one phone number.* | No phone number. | Add one. |
| *A campus lists at most five phone numbers.* | More than five. | Remove some. |
| *Use 7 to 15 digits, with an optional leading +, spaces, brackets or dashes.* | A phone number is malformed. | Correct it. |
| *Use letters, digits, spaces or hyphens, up to 10 characters.* | The postal code is malformed. | Correct it. |
| *Enter a full web address that starts with http:// or https://.* | The website is malformed. | Add `https://` in front. |
| *Place the campus on the map.* | No location. | Click the map, or type latitude and longitude. |
| *Latitude must be between -90 and 90.* / *Longitude must be between -180 and 180.* | Coordinates out of range. | Correct them. |
| *No map is set up for this school yet.* | The school has no Google Maps key. | Type the coordinates, or ask the platform team. |
| 403 Forbidden | The module is off, or you are not an administrator. | See [Enable modules for a school](enable-modules-for-a-school.md). |

## Worked example

Ada Example, school administrator of `Example School`, adds *Example Main Campus*. She types the address, chooses the state and city, enters `54000` as postal code, adds `+92 42 0000 0000` as the phone, the school number `EX-001` and short name `EXM`, places the pin on the map, and selects **Save**. The campus appears in the list.

## Flow recap

*Campuses module on, then Campuses, then Add campus with address, contact and map location, then Save.*

**Not verified in the running app:** the layout of the Add a campus page and whether the map shows for a school; the labels above come from the translation file.
