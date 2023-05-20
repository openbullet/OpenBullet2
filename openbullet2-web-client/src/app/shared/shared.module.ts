import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { SpinnerComponent } from './components/spinner/spinner.component';
import { TruncatePipe } from './pipes/truncate.pipe';
import { BytesPipe } from './pipes/bytes.pipe';
import { InputTextComponent } from './components/input-text/input-text.component';
import { InputNumberComponent } from './components/input-number/input-number.component';
import { InputDropdownComponent } from './components/input-dropdown/input-dropdown.component';
import { DropdownModule } from 'primeng/dropdown';


@NgModule({
  declarations: [
    SpinnerComponent,
    TruncatePipe,
    BytesPipe,
    InputTextComponent,
    InputNumberComponent,
    InputDropdownComponent
  ],
  imports: [
    CommonModule,
    DropdownModule
  ],
  exports: [
    SpinnerComponent,
    TruncatePipe,
    BytesPipe,
    InputTextComponent,
    InputNumberComponent,
    InputDropdownComponent
  ]
})
export class SharedModule { }
