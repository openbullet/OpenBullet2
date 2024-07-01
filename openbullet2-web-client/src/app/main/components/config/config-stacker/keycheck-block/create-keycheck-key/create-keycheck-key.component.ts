import { Component, EventEmitter, Output } from '@angular/core';
import { SettingInputMode } from 'src/app/main/dtos/config/block-descriptor.dto';
import {
  BlockSettingType,
  BoolComparison,
  BoolKeyDto,
  DictComparison,
  DictionaryKeyDto,
  FloatKeyDto,
  IntKeyDto,
  KeyType,
  KeyTypes,
  ListComparison,
  ListKeyDto,
  NumComparison,
  StrComparison,
  StringKeyDto,
} from 'src/app/main/dtos/config/block-instance.dto';

@Component({
  selector: 'app-create-keycheck-key',
  templateUrl: './create-keycheck-key.component.html',
  styleUrls: ['./create-keycheck-key.component.scss'],
})
export class CreateKeycheckKeyComponent {
  @Output() onSelect: EventEmitter<KeyTypes> = new EventEmitter<KeyTypes>();

  KeyType = KeyType;

  selectKey(type: KeyType) {
    let newKey = null;

    switch (type) {
      case KeyType.Bool:
        newKey = <BoolKeyDto>{
          _polyTypeName: KeyType.Bool,
          left: {
            name: 'left',
            value: false,
            inputVariableName: 'data.SOURCE',
            inputMode: SettingInputMode.Variable,
            type: BlockSettingType.Bool,
          },
          right: {
            name: 'right',
            value: true,
            inputVariableName: '',
            inputMode: SettingInputMode.Fixed,
            type: BlockSettingType.Bool,
          },
          comparison: BoolComparison.Is,
        };
        break;

      case KeyType.Dictionary:
        newKey = <DictionaryKeyDto>{
          _polyTypeName: KeyType.Dictionary,
          left: {
            name: 'left',
            value: {},
            inputVariableName: 'data.HEADERS',
            inputMode: SettingInputMode.Variable,
            type: BlockSettingType.DictionaryOfStrings,
          },
          right: {
            name: 'right',
            value: '',
            inputVariableName: '',
            inputMode: SettingInputMode.Fixed,
            type: BlockSettingType.String,
          },
          comparison: DictComparison.HasKey,
        };
        break;

      case KeyType.List:
        newKey = <ListKeyDto>{
          _polyTypeName: KeyType.List,
          left: {
            name: 'left',
            value: [],
            inputVariableName: '',
            inputMode: SettingInputMode.Variable,
            type: BlockSettingType.ListOfStrings,
          },
          right: {
            name: 'right',
            value: '',
            inputVariableName: '',
            inputMode: SettingInputMode.Fixed,
            type: BlockSettingType.String,
          },
          comparison: ListComparison.Contains,
        };
        break;

      case KeyType.Int:
        newKey = <IntKeyDto>{
          _polyTypeName: KeyType.Int,
          left: {
            name: 'left',
            value: 0,
            inputVariableName: 'data.RESPONSECODE',
            inputMode: SettingInputMode.Variable,
            type: BlockSettingType.Int,
          },
          right: {
            name: 'right',
            value: 0,
            inputVariableName: '',
            inputMode: SettingInputMode.Fixed,
            type: BlockSettingType.Int,
          },
          comparison: NumComparison.EqualTo,
        };
        break;

      case KeyType.Float:
        newKey = <FloatKeyDto>{
          _polyTypeName: KeyType.Float,
          left: {
            name: 'left',
            value: 0,
            inputVariableName: '',
            inputMode: SettingInputMode.Variable,
            type: BlockSettingType.Float,
          },
          right: {
            name: 'right',
            value: 0,
            inputVariableName: '',
            inputMode: SettingInputMode.Fixed,
            type: BlockSettingType.Float,
          },
          comparison: NumComparison.EqualTo,
        };
        break;

      case KeyType.String:
        newKey = <StringKeyDto>{
          _polyTypeName: KeyType.String,
          left: {
            name: 'left',
            value: '',
            inputVariableName: 'data.SOURCE',
            inputMode: SettingInputMode.Variable,
            type: BlockSettingType.String,
          },
          right: {
            name: 'right',
            value: '',
            inputVariableName: '',
            inputMode: SettingInputMode.Fixed,
            type: BlockSettingType.String,
          },
          comparison: StrComparison.Contains,
        };
        break;

      default:
        throw new Error(`Unknown key type: ${type}`);
    }

    this.onSelect.emit(newKey);
  }
}
