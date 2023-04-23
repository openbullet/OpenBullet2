import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { SpinnerComponent } from './components/spinner/spinner.component';
import { TruncatePipe } from './pipes/truncate.pipe';
import { BytesPipe } from './pipes/bytes.pipe';



@NgModule({
  declarations: [
    SpinnerComponent,
    TruncatePipe,
    BytesPipe
  ],
  imports: [
    CommonModule
  ],
  exports: [
    SpinnerComponent,
    TruncatePipe,
    BytesPipe
  ]
})
export class SharedModule { }
