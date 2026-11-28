import { RoutesService, eLayoutType } from '@abp/ng.core';
import { inject, provideAppInitializer } from '@angular/core';

export const APP_ROUTE_PROVIDER = [
  provideAppInitializer(() => {
    configureRoutes();
  }),
];

function configureRoutes() {
  const routes = inject(RoutesService);
  routes.add([
    {
      path: '/',
      name: '::Menu:Home',
      iconClass: 'fas fa-home',
      order: 1,
      layout: eLayoutType.application,
    },
    {
      path: '/books',
      name: '::Menu:Books',
      iconClass: 'fas fa-book',
      layout: eLayoutType.application,
      requiredPolicy: 'SmartPantry.Books',
    },
    {
      path: '/authors',
      name: '::Menu:Authors',
      iconClass: 'fas fa-user-edit',
      layout: eLayoutType.application,
      requiredPolicy: 'SmartPantry.Authors',
    },
    // 👇 Agrega esta nueva entrada para tu despensa
    {
      path: '/pantry',
      name: '::Menu:Pantry',
      iconClass: 'fas fa-utensils', // Puedes cambiar el ícono si prefieres otro de FontAwesome
      order: 3,
      layout: eLayoutType.application,
      requiredPolicy: '', // Si tu backend exige una política de permisos para la despensa, ponla aquí
    },
    {
      path: '/account/manage',
      name: '::Menu:Profile',
      iconClass: 'fas fa-user-circle',
      order: 4,
      layout: eLayoutType.application,
    },
    {
      path: '/preferences',
      name: '::Menu:Preferences',
      iconClass: 'fas fa-sliders-h',
      order: 5,
      layout: eLayoutType.application,
    },
    {
      path: '/profile/photo',
      name: '::Menu:ProfilePhoto',
      iconClass: 'fas fa-camera',
      order: 5,
      layout: eLayoutType.application,
    },
    {
      path: '/catalog/lookup',
      name: '::Menu:CatalogLookup',
      iconClass: 'fas fa-barcode',
      order: 5,
      layout: eLayoutType.application,
    },
    {
      path: '/identity/users',
      name: '::Menu:UserAdministration',
      iconClass: 'fas fa-users-cog',
      order: 6,
      layout: eLayoutType.application,
      requiredPolicy: 'AbpIdentity.Users',
    },
    {
      path: '/identity/roles',
      name: '::Menu:RoleAdministration',
      iconClass: 'fas fa-user-shield',
      order: 7,
      layout: eLayoutType.application,
      requiredPolicy: 'AbpIdentity.Roles',
    },
    {
      path: '/administration/metrics',
      name: '::Menu:Metrics',
      iconClass: 'fas fa-chart-bar',
      order: 8,
      layout: eLayoutType.application,
      requiredPolicy: 'SmartPantry.Administration',
    },
  ]);
}
