import { authGuard, permissionGuard } from '@abp/ng.core';
import { Routes } from '@angular/router';

export const APP_ROUTES: Routes = [
  {
    path: '',
    pathMatch: 'full',
    loadComponent: () => import('./home/home.component').then(c => c.HomeComponent),
  },
  {
    path: 'account',
    loadChildren: () => import('@abp/ng.account').then(c => c.createRoutes()),
  },
  {
    path: 'identity',
    loadChildren: () => import('@abp/ng.identity').then(c => c.createRoutes()),
  },
  {
    path: 'setting-management',
    loadChildren: () => import('@abp/ng.setting-management').then(c => c.createRoutes()),
  },
  {
    path: 'books',
    loadComponent: () => import('./book/book.component').then(c => c.BookComponent),
    canActivate: [authGuard, permissionGuard],
  },
  {
    path: 'authors',
    loadComponent: () => import('./author/author.component').then(c => c.AuthorComponent),
    canActivate: [authGuard, permissionGuard],
  },
  {
    path: 'pantry',
    loadComponent: () => import('./pantry/pantry').then(c => c.Pantry),
    canActivate: [authGuard],
  },
  {
    path: 'preferences',
    loadComponent: () => import('./settings/preferences.component').then(c => c.PreferencesComponent),
    canActivate: [authGuard],
  },
  {
    path: 'profile/photo',
    loadComponent: () => import('./settings/profile-picture.component').then(c => c.ProfilePictureComponent),
    canActivate: [authGuard],
  },
  {
    path: 'catalog/lookup',
    loadComponent: () => import('./catalog/lookup.component').then(c => c.LookupComponent),
    canActivate: [authGuard],
  },
  {
    path: 'administration/metrics',
    loadComponent: () => import('./administration/metrics.component').then(c => c.MetricsComponent),
    canActivate: [authGuard, permissionGuard],
    data: { requiredPolicy: 'SmartPantry.Administration' },
  },
];
