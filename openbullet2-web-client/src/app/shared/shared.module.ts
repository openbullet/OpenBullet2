import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { SpinnerComponent } from './components/spinner/spinner.component';
import { TruncatePipe } from './pipes/truncate.pipe';



@NgModule({
  declarations: [
    SpinnerComponent,
    TruncatePipe
  ],
  imports: [
    CommonModule
  ],
  exports: [
    SpinnerComponent,
    TruncatePipe
  ]
})
export class SharedModule { }
