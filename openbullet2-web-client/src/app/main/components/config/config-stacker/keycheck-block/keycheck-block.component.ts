import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { faArrowDown, faArrowUp, faPlus, faTimes, faX } from '@fortawesome/free-solid-svg-icons';
import { BlockDescriptorDto, SettingInputMode } from 'src/app/main/dtos/config/block-descriptor.dto';
import { BlockSettingType, KeyType, KeychainDto, KeychainMode, KeycheckBlockInstanceDto, StrComparison } from 'src/app/main/dtos/config/block-instance.dto';
import { EnvironmentSettingsDto } from 'src/app/main/dtos/settings/environment-settings.dto';

@Component({
  selector: 'app-keycheck-block',
  templateUrl: './keycheck-block.component.html',
  styleUrls: ['./keycheck-block.component.scss']
})
export class KeycheckBlockComponent implements OnInit {
  @Input() block!: KeycheckBlockInstanceDto;
  @Input() descriptor!: BlockDescriptorDto;
  @Input() envSettings!: EnvironmentSettingsDto;

  @Output() onChange: EventEmitter<void> = new EventEmitter<void>();

  faPlus = faPlus;
  faX = faX;
  faTimes = faTimes;
  faArrowUp = faArrowUp;
  faArrowDown = faArrowDown;
  keychainStatuses: string[] = [
    'SUCCESS',
    'FAIL',
    'BAN',
    'RETRY',
    'NONE'
  ];
  keychainModes: KeychainMode[] = [
    KeychainMode.Or,
    KeychainMode.And
  ];

  ngOnInit(): void {
    this.keychainStatuses = [
      ...this.keychainStatuses,
      ...this.envSettings.customStatuses.map(s => s.name)
    ];
  }

  valueChanged() {
    this.onChange.emit();
  }

  keychainStatusChanged(keychain: KeychainDto, status: string) {
    keychain.resultStatus = status;
    this.valueChanged();
  }

  keychainModeChanged(keychain: KeychainDto, mode: KeychainMode) {
    keychain.mode = mode;
    this.valueChanged();
  }

  addKeychain() {
    this.block.keychains = [
      ...this.block.keychains,
      {
        keys: [],
        mode: KeychainMode.Or,
        resultStatus: 'SUCCESS'
      }
    ];
    this.valueChanged();
  }

  removeKeychain(index: number) {
    this.block.keychains = this.block.keychains.filter((_, i) => i !== index);
    this.valueChanged();
  }

  moveKeychainUp(index: number) {
    if (index === 0) {
      return;
    }

    const keychain = this.block.keychains[index];
    this.block.keychains = [
      ...this.block.keychains.slice(0, index - 1),
      keychain,
      this.block.keychains[index - 1],
      ...this.block.keychains.slice(index + 1)
    ];
    this.valueChanged();
  }

  moveKeychainDown(index: number) {
    if (index === this.block.keychains.length - 1) {
      return;
    }

    const keychain = this.block.keychains[index];
    this.block.keychains = [
      ...this.block.keychains.slice(0, index),
      this.block.keychains[index + 1],
      keychain,
      ...this.block.keychains.slice(index + 2)
    ];
    this.valueChanged();
  }

  addKey(keychain: KeychainDto) {
    // FIXME: This doesn't work
    keychain.keys = [
      ...keychain.keys,
      {
        type: KeyType.String,
        left: {
          name: "left",
          value: "",
          inputVariableName: "data.SOURCE",
          inputMode: SettingInputMode.Variable,
          type: BlockSettingType.String
        },
        right: {
          name: "right",
          value: "",
          inputVariableName: "",
          inputMode: SettingInputMode.Fixed,
          type: BlockSettingType.String
        },
        comparison: StrComparison.EqualTo
      }
    ];
    this.valueChanged();
  }

  getColor(keychain: KeychainDto): string {
    let color = 'var(--fg-custom)';

    switch (keychain.resultStatus) {
      case 'SUCCESS':
        color = 'var(--fg-good)';
        break;

      case 'FAIL':
        color = 'var(--fg-bad)';
        break;

      case 'BAN':
        color = 'var(--fg-banned)';
        break;

      case 'RETRY':
        color = 'var(--fg-retry)';
        break;

      case 'NONE':
        color = 'var(--fg-tocheck)';
        break;

      default:
        // Get the first custom status that matches
        const customStatus = this.envSettings.customStatuses.find(
          s => s.name === keychain.resultStatus);

        if (customStatus !== undefined) {
          color = customStatus.color;
        }
        break;
    }

    return color;
  }

  displayEnumValue(value: string): string {
    return value.toUpperCase();
  }
}
