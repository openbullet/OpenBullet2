import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { SpinnerComponent } from './components/spinner/spinner.component';
import { TruncatePipe } from './pipes/truncate.pipe';
import { BytesPipe } from './pipes/bytes.pipe';
import { InputTextComponent } from './components/input-text/input-text.component';
import { InputNumberComponent } from './components/input-number/input-number.component';



@NgModule({
  declarations: [
    SpinnerComponent,
    TruncatePipe,
    BytesPipe,
    InputTextComponent,
    InputNumberComponent
  ],
  imports: [
    CommonModule
  ],
  exports: [
    SpinnerComponent,
    TruncatePipe,
    BytesPipe,
    InputTextComponent,
    InputNumberComponent
  ]
})
export class SharedModule { }
