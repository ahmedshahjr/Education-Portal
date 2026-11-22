---
name: standard-fedramp-moderate
description: What FedRAMP 20x Class C (Moderate) requires of this codebase (its Key Security Indicators, mapped to NIST SP 800-53 Rev 5 controls), the values we set for it, and which topic skills implement each control. Use when working on anything security-related or preparing FedRAMP evidence. Part of the mandatory Education Portal compliance rules.
---

# FedRAMP 20x Moderate (Class C) (MUST follow)

Part of the mandatory secure coding and compliance rules. This skill explains what the standard asks of this codebase and points to the topic skills that hold the actual rules. Always follow the [overview](../SKILL.md) too.

**Never claim certification.** Do not write that the product "is FedRAMP authorized" or show its badge or logo in code, the UI, docs, marketing text, commits or PRs until the maintainer has the authorization, report or certificate in hand and says so. Badges and logos are controlled by the owning body and come only after an independent assessment. Use wording like "designed to support FedRAMP 20x Class C (Moderate) key security indicators".

## What it is

FedRAMP is the US federal government's security authorization program for cloud services. FedRAMP 20x is its current model: providers set their own security goals, show that their security decisions work, and give agencies continuous evidence. Class C is the Moderate level. The Class C rules are 46 Key Security Indicators (KSIs) in 10 groups. Each KSI lists the NIST SP 800-53 Rev 5 controls it covers, so the control table below keeps the links to the topic skills. Rev 5 is being retired: FedRAMP's target date for the last new Rev 5 certifications is 11 June 2027, and it expects existing Rev 5 certifications to end no later than about 2029 (NTC-0013). A provider does not pursue Rev 5 and 20x for the same offering. The official timeline ([fedramp.gov/2026/timeline](https://www.fedramp.gov/2026/timeline/)) sets mandatory adoption for 1 January 2027, schedules the 20x Class B and C pipeline for 31 August 2026 (the page does not confirm it is open), and says the rules move to a monthly release cadence after the 2 November 2026 update. Authorization needs a Certification Package Overview and a Security Decision Record, which replace most of the System Security Plan, an independent assessment by a FedRAMP Recognized service (3PAO is the older term), an authorization decision, and continuous monitoring afterwards. The 20x page is in a phased rollout and some of its dates are estimates, so check [fedramp.gov/20x](https://www.fedramp.gov/20x/) for the current phase rather than relying on this text.

## FedRAMP 20x values we set

Under 20x the provider sets these values and records them in its Security Decision Record. FedRAMP removes most of the assigned Rev 5 parameter values (NTC-0013), so the values below are our own.

| Control | Parameter we apply | Rule |
|---|---|---|
| AC-7 | Lock after no more than 3 failed sign-ins in 15 minutes, for at least 30 minutes | SEC-AUTHN-05 |
| AC-11 / AC-12 | 15-minute idle timeout for staff and master users | SEC-AUTHN-07 |
| IA-2(1), IA-2(2) | MFA for privileged and staff accounts; phishing-resistant for master tenant | SEC-AUTHN-04 |
| IA-5(1) | Authenticators per NIST SP 800-63, with phishing-resistant MFA where passwordless is not feasible (20x KSI-IAM-APM; maintainer to review the scope) | SEC-AUTHN-03 |
| SC-8, SC-13 | TLS everywhere; FIPS 140-validated cryptographic modules in deployed environments (20x Class C: SHOULD use validated modules and MUST document them; we keep the stricter rule) | SEC-CRYPTO-02/03 |
| RA-5, SI-2 | Fix vulnerabilities within the 20x window for each finding's Potential Agency Impact rating (PAIN 2 to 5) | SEC-DEP-07 |
| AU-11 | 20x sets no retention period; OMB M-26-14 rescinds M-21-31 (exact period **TODO**) | SEC-LOG-06 |

## FedRAMP-specific engineering duties

- **Authorization boundary:** every component, external service and data flow must be known. Adding a new external service, region, or data store changes the boundary: it needs maintainer approval (SEC-AI-05) and, once authorized, a significant change request.
- **Hosting:** must be FedRAMP-authorized cloud services (e.g. an authorized Azure region and services). Hosting is **TODO**.
- **Inventory:** keep packages and images traceable (Central Package Management, SBOM) for CM-8 and SR controls.
- **Continuous monitoring:** FedRAMP 20x Class C asks for an Ongoing Certification Report every 3 months and a quarterly review (CCM-OCR-AVL, CCM-QTR-MTG). Our monthly vulnerability scans are team practice, and every open finding has a plan of action. Code fixes follow SEC-DEP-07 windows.

## Control mapping (code-level controls)

Controls not listed (for example AT, PE, PS, PL, CP, IR, MA, MP families and most of CA and PM) are organisational; see the [overview](../SKILL.md) section 3. The 20x column lists the Key Security Indicators whose rules name each control (FedRAMP Consolidated Rules 2026, version 2026.11.02.01). A dash means no 20x indicator covers the control (for example PM-5(1), SI-12 enhancements and CA-8); we follow those as good practice.

| Control | Title | Where the rules are | 20x KSI |
|---|---|---|---|
| AC-2 | Account Management | [authorization-tenant-isolation](../authorization-tenant-isolation/SKILL.md) | KSI-IAM-APM, KSI-IAM-JIT, KSI-IAM-SNU, KSI-IAM-SUS |
| AC-3 | Access Enforcement | [authorization-tenant-isolation](../authorization-tenant-isolation/SKILL.md), [student-data-privacy](../student-data-privacy/SKILL.md) | KSI-IAM-APM, KSI-IAM-ELP, KSI-IAM-JIT |
| AC-4 | Information Flow Enforcement | [authorization-tenant-isolation](../authorization-tenant-isolation/SKILL.md) | KSI-IAM-ELP, KSI-IAM-JIT, KSI-IAM-SNU |
| AC-5 | Separation of Duties | [authorization-tenant-isolation](../authorization-tenant-isolation/SKILL.md) | KSI-IAM-JIT, KSI-PIY-RIS, KSI-PIY-RSD |
| AC-6 | Least Privilege | [authorization-tenant-isolation](../authorization-tenant-isolation/SKILL.md), [configuration-environments](../configuration-environments/SKILL.md) | KSI-IAM-ELP, KSI-IAM-JIT |
| AC-6(9) | Log Use of Privileged Functions | [authorization-tenant-isolation](../authorization-tenant-isolation/SKILL.md) | KSI-IAM-JIT, KSI-MLA-LET, KSI-MLA-RVL |
| AC-6(10) | Prohibit Non-privileged Users from Executing Privileged Functions | [authorization-tenant-isolation](../authorization-tenant-isolation/SKILL.md) | KSI-IAM-JIT |
| AC-7 | Unsuccessful Logon Attempts | [authentication](../authentication/SKILL.md) | KSI-IAM-JIT, KSI-IAM-SUS |
| AC-11 | Device Lock | [authentication](../authentication/SKILL.md) | KSI-SVC-SIN |
| AC-12 | Session Termination | [authentication](../authentication/SKILL.md) | KSI-CNA-ULN, KSI-IAM-ELP |
| AC-21 | Information Sharing | [student-data-privacy](../student-data-privacy/SKILL.md) | KSI-IAM-ELP, KSI-IAM-JIT, KSI-SVC-SIN |
| AU-2 | Event Logging | [logging-audit](../logging-audit/SKILL.md) | KSI-CMT-LMC, KSI-MLA-LET, KSI-MLA-OSM, KSI-MLA-RVL |
| AU-3 | Content of Audit Records | [logging-audit](../logging-audit/SKILL.md) | KSI-MLA-OSM |
| AU-3(3) | Limit Personally Identifiable Information Elements | [student-data-privacy](../student-data-privacy/SKILL.md) | KSI-PIY-RSD |
| AU-6 | Audit Record Review, Analysis, and Reporting | [logging-audit](../logging-audit/SKILL.md) | KSI-MLA-RVL |
| AU-8 | Time Stamps | [logging-audit](../logging-audit/SKILL.md) | KSI-MLA-OSM |
| AU-9 | Protection of Audit Information | [logging-audit](../logging-audit/SKILL.md) | KSI-MLA-OSM |
| AU-11 | Audit Record Retention | [logging-audit](../logging-audit/SKILL.md) | KSI-MLA-OSM |
| AU-12 | Audit Record Generation | [logging-audit](../logging-audit/SKILL.md) | KSI-MLA-LET |
| CA-2 | Control Assessments | [security-testing](../security-testing/SKILL.md) | KSI-PIY-RIS |
| CA-8 | Penetration Testing | [security-testing](../security-testing/SKILL.md) | — |
| CM-2 | Baseline Configuration | [configuration-environments](../configuration-environments/SKILL.md) | KSI-CMT-RMV, KSI-CNA-DFP, KSI-CNA-IBP, KSI-MLA-EVC, KSI-SVC-ACM |
| CM-3 | Configuration Change Control | [change-management](../change-management/SKILL.md), [configuration-environments](../configuration-environments/SKILL.md), [overview](../SKILL.md) | KSI-CMT-LMC, KSI-CMT-RMV, KSI-CMT-RVP, KSI-CMT-VTD |
| CM-4 | Impact Analyses | [change-management](../change-management/SKILL.md) | — |
| CM-5 | Access Restrictions for Change | [change-management](../change-management/SKILL.md) | KSI-CMT-RMV, KSI-CMT-RVP, KSI-IAM-JIT |
| CM-6 | Configuration Settings | [configuration-environments](../configuration-environments/SKILL.md), [cryptography-secrets](../cryptography-secrets/SKILL.md), [web-api-hardening](../web-api-hardening/SKILL.md) | KSI-CMT-LMC, KSI-CMT-RMV, KSI-MLA-EVC, KSI-SVC-ACM |
| CM-7 | Least Functionality | [configuration-environments](../configuration-environments/SKILL.md), [web-api-hardening](../web-api-hardening/SKILL.md) | KSI-CMT-RMV, KSI-IAM-JIT |
| CM-8 | System Component Inventory | [dependencies-supply-chain](../dependencies-supply-chain/SKILL.md) | KSI-PIY-GIV |
| IA-2 | Identification and Authentication (Organizational Users) | [authentication](../authentication/SKILL.md) | KSI-IAM-APM, KSI-IAM-ELP |
| IA-2(1) | Multi-factor Authentication to Privileged Accounts | [authentication](../authentication/SKILL.md) | KSI-IAM-APM |
| IA-2(2) | Multi-factor Authentication to Non-privileged Accounts | [authentication](../authentication/SKILL.md) | KSI-IAM-APM |
| IA-5 | Authenticator Management | [authentication](../authentication/SKILL.md) | KSI-IAM-APM |
| IA-5(1) | Password-based Authentication | [authentication](../authentication/SKILL.md) | KSI-IAM-APM |
| IA-5(7) | No Embedded Unencrypted Static Authenticators | [cryptography-secrets](../cryptography-secrets/SKILL.md) | KSI-PIY-RSD |
| IA-8 | Identification and Authentication (Non-organizational Users) | [authentication](../authentication/SKILL.md) | KSI-IAM-APM |
| PM-5(1) | Inventory of Personally Identifiable Information | [student-data-privacy](../student-data-privacy/SKILL.md) | — |
| RA-5 | Vulnerability Monitoring and Scanning | [dependencies-supply-chain](../dependencies-supply-chain/SKILL.md), [security-testing](../security-testing/SKILL.md) | KSI-SCR-MON |
| SA-8 | Security and Privacy Engineering Principles | [overview](../SKILL.md) | KSI-PIY-RSD |
| SA-10 | Developer Configuration Management | [change-management](../change-management/SKILL.md), [configuration-environments](../configuration-environments/SKILL.md), [dependencies-supply-chain](../dependencies-supply-chain/SKILL.md), [overview](../SKILL.md) | KSI-SCR-MIT |
| SA-11 | Developer Testing and Evaluation | [change-management](../change-management/SKILL.md), [overview](../SKILL.md), [security-testing](../security-testing/SKILL.md) | KSI-SCR-MIT |
| SA-11(1) | Static Code Analysis | [security-testing](../security-testing/SKILL.md) | KSI-SCR-MIT |
| SA-22 | Unsupported System Components | [dependencies-supply-chain](../dependencies-supply-chain/SKILL.md) | KSI-SCR-MIT |
| SC-4 | Information in Shared System Resources | [authorization-tenant-isolation](../authorization-tenant-isolation/SKILL.md) | KSI-CNA-ULN, KSI-IAM-ELP, KSI-PIY-RSD, KSI-SVC-PRR, KSI-SVC-SIN |
| SC-5 | Denial-of-service Protection | [web-api-hardening](../web-api-hardening/SKILL.md) | KSI-CNA-RVP |
| SC-7 | Boundary Protection | [web-api-hardening](../web-api-hardening/SKILL.md) | KSI-CNA-ULN, KSI-SVC-EIS |
| SC-8 | Transmission Confidentiality and Integrity | [cryptography-secrets](../cryptography-secrets/SKILL.md), [web-api-hardening](../web-api-hardening/SKILL.md) | KSI-CNA-MAT, KSI-CNA-ULN, KSI-SVC-SIN |
| SC-8(1) | Cryptographic Protection (in transit) | [web-api-hardening](../web-api-hardening/SKILL.md) | KSI-SVC-SIN |
| SC-12 | Cryptographic Key Establishment and Management | [cryptography-secrets](../cryptography-secrets/SKILL.md) | KSI-SVC-ASM |
| SC-13 | Cryptographic Protection | [cryptography-secrets](../cryptography-secrets/SKILL.md) | KSI-SVC-SIN, KSI-SVC-VRI |
| SC-18 | Mobile Code | [input-validation](../input-validation/SKILL.md) | KSI-PIY-RSD, KSI-SCR-MIT |
| SC-23 | Session Authenticity | [authentication](../authentication/SKILL.md), [web-api-hardening](../web-api-hardening/SKILL.md) | KSI-IAM-APM, KSI-IAM-ELP, KSI-IAM-JIT, KSI-SVC-SIN, KSI-SVC-VCM, KSI-SVC-VRI |
| SC-24 | Fail in Known State | [error-handling](../error-handling/SKILL.md) | KSI-CNA-EIS |
| SC-28 | Protection of Information at Rest | [cryptography-secrets](../cryptography-secrets/SKILL.md), [student-data-privacy](../student-data-privacy/SKILL.md) | KSI-SVC-SIN |
| SI-2 | Flaw Remediation | [dependencies-supply-chain](../dependencies-supply-chain/SKILL.md) | KSI-CMT-VTD |
| SI-4 | System Monitoring | [logging-audit](../logging-audit/SKILL.md) | KSI-IAM-SUS, KSI-MLA-RVL, KSI-SVC-EIS |
| SI-7 | Software, Firmware, and Information Integrity | [dependencies-supply-chain](../dependencies-supply-chain/SKILL.md) | KSI-CNA-EIS, KSI-SVC-VRI |
| SI-10 | Information Input Validation | [error-handling](../error-handling/SKILL.md), [input-validation](../input-validation/SKILL.md) | KSI-CNA-MAT, KSI-CNA-RNT, KSI-PIY-RSD |
| SI-11 | Error Handling | [error-handling](../error-handling/SKILL.md), [web-api-hardening](../web-api-hardening/SKILL.md) | KSI-CNA-MAT, KSI-MLA-ALA, KSI-PIY-RSD |
| SI-12 | Information Management and Retention | [student-data-privacy](../student-data-privacy/SKILL.md) | — |
| SI-12(1) | Limit Personally Identifiable Information Elements | [student-data-privacy](../student-data-privacy/SKILL.md) | — |
| SI-12(2) | Minimize PII in Testing, Training, and Research | [student-data-privacy](../student-data-privacy/SKILL.md) | — |
| SI-12(3) | Information Disposal | [student-data-privacy](../student-data-privacy/SKILL.md) | KSI-SVC-RUD |
| SI-15 | Information Output Filtering | [input-validation](../input-validation/SKILL.md) | — |
| SR-3 | Supply Chain Controls and Processes | [dependencies-supply-chain](../dependencies-supply-chain/SKILL.md) | KSI-SCR-MIT, KSI-SCR-MON |
| SR-4 | Provenance | [dependencies-supply-chain](../dependencies-supply-chain/SKILL.md) | — |
| SR-11 | Component Authenticity | [dependencies-supply-chain](../dependencies-supply-chain/SKILL.md) | KSI-SVC-VRI |
| PT family | Personally Identifiable Information Processing and Transparency | [student-data-privacy](../student-data-privacy/SKILL.md) | — |

## FedRAMP 20x Class C Key Security Indicators

The Class C (Moderate) rules as published in the FedRAMP Consolidated Rules 2026 (version 2026.11.02.01). The statements are FedRAMP's own text, quoted unchanged. The last column lists the NIST SP 800-53 Rev 5 controls each indicator names; use it with the control mapping above. Source: [FedRAMP/rules, fedramp-consolidated-rules.json](https://github.com/FedRAMP/rules/blob/main/fedramp-consolidated-rules.json), fetched 2026-11-22.

| Indicator | Group | Name | Statement | Controls |
|---|---|---|---|---|
| KSI-CED-RAT | Cybersecurity Education | Reviewing All Training | The effectiveness of relevant cybersecurity education and training is persistently reviewed, including at least general training for all employees, role-specific training for employees in high risk roles, training for development and engineering staff on secure software delivery, and training for staff involved with incident response or disaster recovery. | AT-2, AT-2(2), AT-2(3), AT-3, AT-3(5), AT-4, CM-11, CP-3, CP-3(1), IR-2, IR-2(1), IR-2(2), IR-2(3), IR-9(2), IR-9(4), PL-4, PL-4(1), PS-6, SR-11(1) |
| KSI-CMT-LMC | Change Management | Logging Changes | Modifications to the cloud service offering are logged and monitored. | AU-2, CA-7(4), CM-3, CM-3(1), CM-3(2), CM-3(6), CM-4(2), CM-6, CM-8(3) |
| KSI-CMT-RMV | Change Management | Redeploying vs Modifying | Changes to machine-based information resources are executed through the redeployment of version controlled resources rather than direct modification wherever reasonable. | CM-2, CM-3, CM-5, CM-6, CM-7, CM-8(1), SI-3 |
| KSI-CMT-RVP | Change Management | Reviewing Change Procedures | The effectiveness of documented change management procedures is persistently reviewed. | CM-3, CM-3(2), CM-3(4), CM-5, CM-7(1), CM-9 |
| KSI-CMT-VTD | Change Management | Validating Throughout Deployment | Persistent testing and validation of changes throughout deployment is automated. | CM-3, CM-3(2), CM-4(2), CM-6(2), IR-9(4), SI-2 |
| KSI-CNA-DFP | Cloud Native Architecture | Defining Functionality and Privileges | The functionality and privileges for infrastructure and services are strictly defined. | CM-2, CM-8(4), CP-10(4), IR-9(4), SI-3 |
| KSI-CNA-EIS | Cloud Native Architecture | Enforcing Intended State |  | CA-2(1), CM-11, CM-6(2), CP-10(4), IR-9(4), SC-24, SC-7(18), SI-4(22), SI-7, SI-7(1), SI-7(5), SI-7(7) |
| KSI-CNA-IBP | Cloud Native Architecture | Implementing Best Practices | The use and configuration of third-party machine-based information resources is persistently compared against the original provider's best practices and guidance. | AC-17(3), CM-2, PL-10, SC-7(18) |
| KSI-CNA-MAT | Cloud Native Architecture | Minimizing Attack Surface | Machine-based information resources are persistently reviewed to ensure they have a minimal attack surface and that lateral movement is minimized if compromised. | AC-17(3), AC-18(1), AC-18(3), AC-20(1), CA-9, SC-10, SC-7(18), SC-7(3), SC-7(4), SC-7(5), SC-7(8), SC-8, SI-10, SI-11, SI-16 |
| KSI-CNA-OFA | Cloud Native Architecture | Optimizing for Availability | Machine-based information resources are persistently reviewed to ensure they are appropriately optimized for high availability and rapid recovery. | CP-2(5), SC-12(1) |
| KSI-CNA-RNT | Cloud Native Architecture | Restricting Network Traffic | Machine-based information resources are persistently reviewed to ensure they are appropriately configured to limit inbound and outbound network traffic. | AC-17(3), CA-9, CM-7(1), SC-7(10), SC-7(18), SC-7(20), SC-7(21), SC-7(5), SI-10, SI-4(11), SI-4(18), SI-4(4), SI-8 |
| KSI-CNA-RVP | Cloud Native Architecture | Reviewing Protections | The effectiveness of protection against denial of service attacks and other unwanted activity for machine-based information resources is persistently reviewed. | IR-4(2), SC-5, SC-7(18), SI-8, SI-8(2) |
| KSI-CNA-ULN | Cloud Native Architecture | Using Logical Networking | Logical networking and related capabilities are used and persistently reviewed to enforce traffic flow controls. | AC-12, AC-17(3), AC-4(21), CA-9, CM-11, SC-10, SC-4, SC-7, SC-7(20), SC-7(21), SC-7(7), SC-8 |
| KSI-IAM-AAM | Identity and Access Management | Automating Account Management | The lifecycle and privileges of all accounts, roles, and groups are securely managed using automation. | AC-2(13), AC-2(2), AC-2(3), AC-6(7), IA-12, IA-12(2), IA-12(3), IA-12(5), IA-4(4), IA-8(4) |
| KSI-IAM-APM | Identity and Access Management | Adopting Passwordless Methods | Secure passwordless methods are used for user authentication and authorization when feasible, otherwise strong passwords with phishing-resistant MFA is used. | AC-2, AC-3, IA-2, IA-2(1), IA-2(2), IA-2(6), IA-2(8), IA-5, IA-5(1), IA-5(2), IA-5(6), IA-5(8), IA-6, IA-8, IA-8(2), IA-8(4), SC-23 |
| KSI-IAM-ELP | Identity and Access Management | Ensuring Least Privilege | Identity and access management measures are used and persistently reviewed to ensure each user or device can only access the resources they need. | AC-12, AC-14, AC-17, AC-17(1), AC-17(2), AC-17(3), AC-18(4), AC-2(11), AC-2(5), AC-2(6), AC-2(7), AC-2(9), AC-20, AC-20(1), AC-21, AC-3, AC-4, AC-6, AC-6(3), AC-6(8), AU-12(3), AU-6(7), CM-2(7), CM-5(1), CM-5(5), CM-7(2), CM-9, IA-11, IA-2, IA-3, IA-4, IA-4(4), IA-5(2), IA-5(6), IR-4(6), IR-9(4), PS-2, PS-3, PS-4, PS-5, PS-6, RA-5(5), SC-2, SC-20, SC-21, SC-22, SC-23, SC-3, SC-39, SC-4, SI-3, SI-4(19) |
| KSI-IAM-JIT | Identity and Access Management | Authorizing Just-in-Time | A least-privileged, role and attribute-based, and just-in-time security authorization model is used and persistently reviewed for all user and non-user accounts and services. | AC-17, AC-17(4), AC-2, AC-2(1), AC-2(11), AC-2(2), AC-2(3), AC-2(4), AC-2(6), AC-2(7), AC-20(1), AC-21, AC-3, AC-4, AC-5, AC-6, AC-6(1), AC-6(10), AC-6(2), AC-6(3), AC-6(5), AC-6(7), AC-6(8), AC-6(9), AC-7, AU-10, AU-12(3), AU-9(4), CA-3(6), CM-5, CM-5(1), CM-5(5), CM-7, CM-7(2), CM-7(5), CM-9, IA-4, IA-4(4), IA-7, IR-4(6), PS-2, PS-3, PS-4, PS-5, PS-6, PS-9, RA-5(5), SC-2, SC-23, SC-3, SC-39 |
| KSI-IAM-SNU | Identity and Access Management | Securing Non-User Authentication | Appropriately secure authentication methods are used and persistently reviewed for non-user accounts and services. | AC-2, AC-2(2), AC-2(9), AC-4, AC-6(5), AU-10, IA-2(5), IA-3, IA-5(2), RA-5(5) |
| KSI-IAM-SUS | Identity and Access Management | Responding to Suspicious Activity | Accounts with privileged access are disabled or otherwise secured in response to suspicious activity. | AC-2, AC-2(1), AC-2(11), AC-2(12), AC-2(13), AC-2(3), AC-7, AU-6(5), CM-5(1), CM-6(2), IR-4(2), IR-4(6), PS-4, PS-8, SI-4, SI-4(1), SI-4(19), SI-4(20), SI-4(22), SI-4(4) |
| KSI-INR-AAR | Incident Response | Generating After Action Reports | Incident after action reports are generated and lessons learned are persistently incorporated. | IR-3, IR-4, IR-4(1), IR-5(1), IR-8, IR-9(3) |
| KSI-INR-RIR | Incident Response | Reviewing Incident Response Procedures | The effectiveness of documented incident response procedures is persistently reviewed. | IR-4, IR-4(1), IR-4(11), IR-6, IR-6(1), IR-6(3), IR-7, IR-7(1), IR-8, IR-8(1), IR-9(3), SI-4(5) |
| KSI-INR-RPI | Incident Response | Reviewing Past Incidents | Past incidents are persistently reviewed for patterns or vulnerabilities that were not previously apparent or identified. | IR-3, IR-4, IR-4(1), IR-4(11), IR-4(4), IR-5, IR-5(1), IR-8, RA-5(8) |
| KSI-MLA-ALA | Monitoring, Logging, and Auditing | Authorizing Log Access |  | AU-12(3), AU-6(7), SI-11 |
| KSI-MLA-EVC | Monitoring, Logging, and Auditing | Evaluating Configurations | The configuration of machine-based information resources, especially infrastructure as code, is persistently evaluated and tested. | AC-19, CA-7, CM-2, CM-6, CM-6(1), RA-3, SI-7(7) |
| KSI-MLA-LET | Monitoring, Logging, and Auditing | Logging Event Types | A list of information resources and event types that will be logged, monitored, and audited is maintained and persistently reviewed to ensure these activities occur. | AC-17(1), AC-2(4), AC-20(1), AC-6(9), AU-10, AU-12, AU-2, AU-7, AU-7(1), CM-5(1), IR-9(4), SI-4(19), SI-4(20), SI-4(4), SI-4(5), SI-7(7) |
| KSI-MLA-OSM | Monitoring, Logging, and Auditing | Operating SIEM Capability | A Security Information and Event Management (SIEM) or similar system(s) is used and persistently reviewed for centralized, tamper-resistant logging of events, activities, and changes. | AC-17(1), AC-20(1), AU-11, AU-12(1), AU-2, AU-3, AU-3(1), AU-4, AU-5, AU-5(1), AU-6(1), AU-6(3), AU-6(4), AU-6(5), AU-7, AU-7(1), AU-8, AU-9, IR-4(1), SI-4(16), SI-4(2), SI-4(4) |
| KSI-MLA-RVL | Monitoring, Logging, and Auditing | Reviewing Logs | Logs are persistently reviewed and audited. | AC-2(4), AC-6(9), AU-2, AU-6, AU-6(1), AU-7, RA-5(8), SI-4, SI-4(19), SI-4(4) |
| KSI-PIY-GIV | Policy and Inventory | Generating Inventories | Authoritative sources are used to automatically generate real-time inventories of all information resources when needed. | CM-12, CM-12(1), CM-2(2), CM-7(5), CM-8, CM-8(1), CM-8(2), CM-8(4), CP-2(8) |
| KSI-PIY-RES | Policy and Inventory | Reviewing Executive Support | Executive support for achieving the provider's security goals is persistently reviewed and demonstrated. |  |
| KSI-PIY-RIS | Policy and Inventory | Reviewing Investments in Security | The effectiveness of the provider's investments in achieving security goals is persistently reviewed. | AC-5, CA-2, CA-2(2), CP-2(1), CP-4(1), IR-3(2), IR-4(11), PM-3, SA-2, SA-3, SR-2(1) |
| KSI-PIY-RSD | Policy and Inventory | Reviewing Security in the SDLC | The effectiveness of building security and privacy considerations into the Software Development Lifecycle and aligning with CISA Secure By Design principles is persistently reviewed. | AC-5, AU-3(3), CM-3(4), IA-5(7), PL-8, PM-7, RA-9, SA-3, SA-8, SC-18, SC-4, SI-10, SI-11, SI-16 |
| KSI-PIY-RVD | Policy and Inventory | Reviewing Vulnerability Disclosures | The effectiveness of the provider's vulnerability disclosure program is persistently reviewed. | RA-5(11) |
| KSI-RPL-ABO | Recovery Planning | Aligning Backups with Objectives | The alignment of machine-based information resource backups with defined recovery objectives is persistently reviewed. | CM-2(3), CP-6, CP-9, CP-10, CP-10(2) |
| KSI-RPL-ARP | Recovery Planning | Aligning Recovery Plan | The alignment of recovery plans with defined recovery objectives is persistently reviewed. | CP-2, CP-2(1), CP-2(3), CP-4(1), CP-6, CP-6(1), CP-6(3), CP-7, CP-7(1), CP-7(2), CP-7(3), CP-8, CP-8(1), CP-8(2), CP-10, CP-10(2) |
| KSI-RPL-RRO | Recovery Planning | Reviewing Recovery Objectives | The desired Recovery Time Objectives (RTO) and Recovery Point Objectives (RPO) are defined and persistently reviewed for alignment with the provider's business needs and capabilities. | CP-2(3), CP-10 |
| KSI-RPL-TRC | Recovery Planning | Testing Recovery Capabilities | The capability to recover from incidents and contingencies aligned with defined recovery objectives is persistently tested. | CP-10, CP-2(1), CP-2(3), CP-4, CP-4(1), CP-6, CP-6(1), CP-9(1), IR-3, IR-3(2), IR-9(3), SC-12(1) |
| KSI-SCR-MIT | Supply Chain Risk | Mitigating Supply Chain Risk | Potential supply chain risks are persistently identified, reviewed, and mitigated. | AC-19, AC-20, CA-7(4), RA-3, RA-3(1), SA-10, SA-11, SA-11(1), SA-11(2), SA-15, SA-15(3), SA-16, SA-17, SA-21, SA-22, SA-4, SA-4(1), SA-4(2), SA-4(5), SA-4(9), SA-9, SA-9(1), SA-9(2), SA-9(5), SC-18, SI-7(1), SI-7(15), SR-2, SR-3, SR-5, SR-6 |
| KSI-SCR-MON | Supply Chain Risk | Monitoring Supply Chain Risk | Third party software information resources are automatically monitored for upstream vulnerabilities using mechanisms that may include contractual notification requirements or active monitoring services. | AC-19, AC-20, CA-2(3), CA-3, IR-6(3), PS-7, RA-3, RA-5, SA-9, SI-5, SR-3, SR-5, SR-6, SR-8 |
| KSI-SVC-ACM | Service Configuration | Automating Configuration Management | The configuration of machine-based information resources is managed using automation and persistently reviewed for drift. | AC-2(4), AC-6(8), CM-2, CM-2(2), CM-2(3), CM-6, CM-7(1), PL-10, PL-9, SA-5, SC-45, SC-45(1), SC-7(18), SI-5, SR-10 |
| KSI-SVC-ASM | Service Configuration | Automating Secret Management | Management, protection, and regular rotation of digital keys, certificates, and other secrets is automated and persistently reviewed. | AC-17(2), IA-5(2), IA-5(6), SC-12, SC-12(1), SC-17 |
| KSI-SVC-EIS | Service Configuration | Evaluating and Improving Security | Information resources are persistently evaluated for opportunities to improve security and those improvements are persistently made. | CA-7(4), CM-12(1), CM-5(5), CM-7(1), IR-9(4), PL-8, RA-3, SC-39, SC-7, SI-2(2), SI-4, SR-10 |
| KSI-SVC-PRR | Service Configuration | Preventing Residual Risk |  | AC-19, IR-9(4), SC-12(1), SC-4, SC-7(18) |
| KSI-SVC-RUD | Service Configuration | Removing Unwanted Data |  | IR-9, SI-12(3), SI-18(4), SI-8, SI-8(2) |
| KSI-SVC-SIN | Service Configuration | Securing Information | Information is encrypted or otherwise secured from unwanted access or modification. | AC-11, AC-11(1), AC-17(2), AC-19(5), AC-21, CP-9(8), IR-9(4), SC-13, SC-20, SC-21, SC-22, SC-23, SC-28, SC-28(1), SC-4, SC-7(18), SC-8, SC-8(1), SI-4(18) |
| KSI-SVC-VCM | Service Configuration | Validating Communications |  | AC-10, AC-17(4), AC-18, AC-19, SC-23, SI-4(22), SI-7(1) |
| KSI-SVC-VRI | Service Configuration | Validating Resource Integrity | Use cryptographic methods to validate the integrity of machine-based information resources. | AC-17(4), AC-18, AC-19, AU-9(3), CM-14, CM-2(2), CM-8(3), SC-13, SC-23, SI-7, SI-7(1), SI-7(15), SR-10, SR-11 |

## Rules for any change

1. Before coding, find which rows in the mapping table below your change touches and load those topic skills.
2. Changes must keep the evidence an assessor will ask for: the PR (with the security checklist), the review approval, the CI run, and tests. Never bypass the PR flow (SEC-CHG-01).
3. Do not invent control parameters, scope or legal applicability. If the standard needs a value the maintainer has not set (marked **TODO**), ask.
4. If the standard updates (the twice-monthly sources review opens an issue), update this skill and the affected topic skills in one PR.

<!-- BEGIN GENERATED: sources (docs/ai/sync-ai-wrappers.py) -->
## Sources and update checks

This skill is written against the versions below, recorded in [sources.json](../sources.json). `.github/workflows/compliance-sources-review.yml` checks every source on the 1st and 16th of each month and opens or updates a `compliance-review` issue. To apply a change: update this skill and any affected topic skills, then `tracked_version`, `last_seen_sha` and `last_reviewed` in sources.json, in one PR into `development`, and run `python3 docs/ai/sync-ai-wrappers.py`. The workflow never edits rules itself.

| Source | Version tracked | How it is checked | Last reviewed |
|---|---|---|---|
| [NIST SP 800-53 Rev 5 catalog (OSCAL)](https://csrc.nist.gov/projects/risk-management/sp800-53-controls/release-search) | SP 800-53 Rev 5.2.0 (released 27 Aug 2025; OSCAL catalog 5.2.0, last changed 2026-10-22) | Automatic: new commits in `usnistgov/oscal-content/nist.gov/SP800-53/rev5` | 2026-11-22 |
| [FedRAMP 20x Class C (Moderate) rules: Key Security Indicators and Consolidated Rules](https://www.fedramp.gov/20x/) | FedRAMP Consolidated Rules 2026, version 2026.11.02.01 (20x Class C) | Automatic: new commits in `FedRAMP/rules/fedramp-consolidated-rules.json` | 2026-11-22 |
| [NIST SP 800-63 Digital Identity Guidelines (Rev 4)](https://pages.nist.gov/800-63-4/) | SP 800-63-4 (Rev 4) | Automatic: new commits in `usnistgov/800-63-4` | 2026-11-22 |
| [OMB M-26-14 logging requirements (rescinds OMB M-21-31; referenced by FedRAMP AU-11)](https://www.whitehouse.gov/omb/information-for-agencies/memoranda/) | OMB M-26-14 (22 May 2026), rescinds OMB M-21-31 (2021) | Manual: listed for a portal check | 2026-11-22 |
<!-- END GENERATED: sources -->
