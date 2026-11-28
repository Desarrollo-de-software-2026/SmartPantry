import { ChangeDetectionStrategy, Component, OnInit, inject, signal } from '@angular/core';
import { LocalizationPipe } from '@abp/ng.core';
import { ProfilePictureService } from '../proxy/perfil/profile-picture.service';

@Component({
  selector: 'app-profile-picture', standalone: true, imports: [LocalizationPipe],
  template: `<main class="container py-4"><h1 class="h2">{{ '::ProfilePhoto:Title' | abpLocalization }}</h1><p class="text-secondary">{{ '::ProfilePhoto:Help' | abpLocalization }}</p>
    @if (photo()) { <img class="profile-preview mb-3" [src]="photo()" alt="{{ '::ProfilePhoto:Preview' | abpLocalization }}" /> }
    <div class="d-flex gap-2 flex-wrap"><input class="form-control photo-input" type="file" accept="image/png,image/jpeg,image/webp" (change)="select($event)" />
    <button class="btn btn-primary" type="button" [disabled]="!pending() || busy()" (click)="save()">{{ '::ProfilePhoto:Save' | abpLocalization }}</button>
    <button class="btn btn-outline-danger" type="button" [disabled]="!photo() || busy()" (click)="remove()">{{ '::ProfilePhoto:Remove' | abpLocalization }}</button></div>
    @if (message()) { <div class="alert alert-info mt-3" role="status">{{ message() | abpLocalization }}</div> }
  </main>`,
  styles: [`.profile-preview{width:144px;height:144px;border-radius:50%;object-fit:cover;border:1px solid #dee2e6}.photo-input{max-width:360px}`],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ProfilePictureComponent implements OnInit {
  private readonly service = inject(ProfilePictureService);
  readonly photo = signal<string | null>(null); readonly pending = signal<{contentType:string;base64:string}|null>(null);
  readonly message = signal(''); readonly busy = signal(false);
  ngOnInit(): void { this.service.get({ skipHandleError: true }).subscribe({ next: result => { if(result) this.photo.set(`data:${result.contentType};base64,${result.base64}`); } }); }
  select(event: Event): void {
    const file = (event.target as HTMLInputElement).files?.[0]; if (!file) return;
    if (!['image/png','image/jpeg','image/webp'].includes(file.type) || file.size > 1024*1024) { this.message.set('::ProfilePhoto:Invalid'); this.pending.set(null); return; }
    const reader = new FileReader(); reader.onload = () => { const data = String(reader.result); const base64 = data.split(',')[1] ?? ''; this.pending.set({contentType:file.type,base64}); this.photo.set(data); this.message.set(''); }; reader.readAsDataURL(file);
  }
  save(): void { const value=this.pending(); if(!value || this.busy()) return; this.busy.set(true); this.service.save(value,{skipHandleError:true}).subscribe({next:()=>{this.pending.set(null);this.message.set('::ProfilePhoto:Saved');this.busy.set(false);},error:()=>{this.message.set('::ProfilePhoto:Error');this.busy.set(false);}}); }
  remove(): void { this.busy.set(true); this.service.delete({skipHandleError:true}).subscribe({next:()=>{this.photo.set(null);this.pending.set(null);this.message.set('::ProfilePhoto:Removed');this.busy.set(false);},error:()=>{this.message.set('::ProfilePhoto:Error');this.busy.set(false);}}); }
}
