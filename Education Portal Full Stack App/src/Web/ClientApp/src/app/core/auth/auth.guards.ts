import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { BrandingService } from '../branding/branding.service';
import { ModulesService } from '../modules/modules.service';
import { AuthService } from './auth.service';

/**
 * Sends visitors who are not signed in to the login page, and back here afterwards. An account that still has its
 * one-time password goes to the change-password page first (the server refuses everything else anyway).
 */
export const authGuard: CanActivateFn = (_route, state) => {
  const auth = inject(AuthService);
  if (!auth.isAuthenticated()) {
    return inject(Router).createUrlTree(['/login'], { queryParams: { returnUrl: state.url } });
  }
  return auth.mustChangePassword() ? inject(Router).createUrlTree(['/change-password']) : true;
};

/** Signed-in users only, including one that must still replace a one-time password (the change-password page). */
export const signedInGuard: CanActivateFn = (_route, state) => {
  return inject(AuthService).isAuthenticated()
    ? true
    : inject(Router).createUrlTree(['/login'], { queryParams: { returnUrl: state.url } });
};

/** School administrators only. The server checks the role again on every request. */
export const administratorGuard: CanActivateFn = () => {
  return inject(AuthService).isAdministrator() ? true : inject(Router).createUrlTree(['/']);
};

/** Master tenant administrators only (the master portal). The server checks this again on every request. */
export const masterAdministratorGuard: CanActivateFn = () => {
  const isMasterAdministrator = inject(AuthService).isAdministrator() && inject(BrandingService).isMaster();
  return isMasterAdministrator ? true : inject(Router).createUrlTree(['/']);
};

/** Only when the module is switched on for the school (use on a module's routes). The server checks it again on every request. */
export const moduleGuard =
  (moduleKey: string): CanActivateFn =>
  () =>
    inject(ModulesService).isEnabled(moduleKey) ? true : inject(Router).createUrlTree(['/']);
