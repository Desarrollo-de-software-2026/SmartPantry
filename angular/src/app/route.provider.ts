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
      name: 'Mi despensa',
      iconClass: 'fas fa-utensils', // Puedes cambiar el ícono si prefieres otro de FontAwesome
      order: 3,
      layout: eLayoutType.application,
      requiredPolicy: '', // Si tu backend exige una política de permisos para la despensa, ponla aquí
    },
  ]);
}
