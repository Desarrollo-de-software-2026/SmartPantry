import { Injectable, inject } from '@angular/core';
import { Rest, RestService } from '@abp/ng.core';
import type { ProfilePictureDto } from './models';

@Injectable({ providedIn: 'root' })
export class ProfilePictureService {
  private readonly rest = inject(RestService);
  get(config?: Partial<Rest.Config>) { return this.rest.request<any, ProfilePictureDto | null>({ method: 'GET', url: '/api/app/profile-picture' }, { apiName: 'Default', ...config }); }
  save(body: ProfilePictureDto, config?: Partial<Rest.Config>) { return this.rest.request<ProfilePictureDto, void>({ method: 'POST', url: '/api/app/profile-picture', body }, { apiName: 'Default', ...config }); }
  delete(config?: Partial<Rest.Config>) { return this.rest.request<any, void>({ method: 'DELETE', url: '/api/app/profile-picture' }, { apiName: 'Default', ...config }); }
}
