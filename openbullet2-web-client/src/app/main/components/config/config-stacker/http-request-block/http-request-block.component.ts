import { Component, EventEmitter, Input, OnChanges, Output, SimpleChanges } from '@angular/core';
import { faPlus, faTimes } from '@fortawesome/free-solid-svg-icons';
import { BlockDescriptorDto, SettingInputMode } from 'src/app/main/dtos/config/block-descriptor.dto';
import {
  BasicAuthRequestParamsDto,
  BlockSettingType,
  HttpRequestBlockInstanceDto,
  MultipartContentType,
  MultipartRequestParamsDto,
  RawRequestParamsDto,
  RequestParamsType,
  StandardRequestParamsDto,
} from 'src/app/main/dtos/config/block-instance.dto';
import { ConfigStackerComponent } from '../config-stacker.component';

@Component({
  selector: 'app-http-request-block',
  templateUrl: './http-request-block.component.html',
  styleUrls: ['./http-request-block.component.scss'],
})
export class HttpRequestBlockComponent implements OnChanges {
  @Input() block!: HttpRequestBlockInstanceDto;
  @Input() descriptor!: BlockDescriptorDto;
  @Input() stacker!: ConfigStackerComponent;

  @Output() onChange: EventEmitter<void> = new EventEmitter<void>();

  faPlus = faPlus;
  faTimes = faTimes;
  RequestParamsType = RequestParamsType;
  MultipartContentType = MultipartContentType;
  currentParamsType: RequestParamsType = RequestParamsType.Standard;
  useCustomCipherSuites = false;

  standardRequestParams: StandardRequestParamsDto = {
    _polyTypeName: RequestParamsType.Standard,
    content: {
      name: 'content',
      value: '',
      inputVariableName: '',
      inputMode: SettingInputMode.Interpolated,
      type: BlockSettingType.String,
    },
    contentType: {
      name: 'contentType',
      value: 'application/x-www-form-urlencoded',
      inputVariableName: '',
      inputMode: SettingInputMode.Fixed,
      type: BlockSettingType.String,
    },
  };

  rawRequestParams: RawRequestParamsDto = {
    _polyTypeName: RequestParamsType.Raw,
    content: {
      name: 'content',
      value: '',
      inputVariableName: '',
      inputMode: SettingInputMode.Fixed,
      type: BlockSettingType.ByteArray,
    },
    contentType: {
      name: 'contentType',
      value: 'application/octet-stream',
      inputVariableName: '',
      inputMode: SettingInputMode.Fixed,
      type: BlockSettingType.String,
    },
  };

  basicAuthRequestParams: BasicAuthRequestParamsDto = {
    _polyTypeName: RequestParamsType.BasicAuth,
    username: {
      name: 'username',
      value: '',
      inputVariableName: '',
      inputMode: SettingInputMode.Fixed,
      type: BlockSettingType.String,
    },
    password: {
      name: 'password',
      value: '',
      inputVariableName: '',
      inputMode: SettingInputMode.Fixed,
      type: BlockSettingType.String,
    },
  };

  multipartRequestParams: MultipartRequestParamsDto = {
    _polyTypeName: RequestParamsType.Multipart,
    contents: [],
    boundary: {
      name: 'boundary',
      value: '',
      inputVariableName: '',
      inputMode: SettingInputMode.Fixed,
      type: BlockSettingType.String,
    },
  };

  ngOnChanges(changes: SimpleChanges): void {
    // Set the correct request params type
    if (this.block.requestParams._polyTypeName === RequestParamsType.Standard) {
      this.standardRequestParams = this.block.requestParams;
      this.currentParamsType = RequestParamsType.Standard;
    } else if (this.block.requestParams._polyTypeName === RequestParamsType.Raw) {
      this.rawRequestParams = this.block.requestParams;
      this.currentParamsType = RequestParamsType.Raw;
    } else if (this.block.requestParams._polyTypeName === RequestParamsType.BasicAuth) {
      this.basicAuthRequestParams = this.block.requestParams;
      this.currentParamsType = RequestParamsType.BasicAuth;
    } else if (this.block.requestParams._polyTypeName === RequestParamsType.Multipart) {
      this.multipartRequestParams = this.block.requestParams;
      this.currentParamsType = RequestParamsType.Multipart;
    }

    this.useCustomCipherSuites = this.block.settings['useCustomCipherSuites'].value;
  }

  valueChanged() {
    this.onChange.emit();
  }

  paramsTypeChanged(newParamsType: RequestParamsType) {
    switch (newParamsType) {
      case RequestParamsType.Standard:
        this.block.requestParams = this.standardRequestParams;
        break;

      case RequestParamsType.Raw:
        this.block.requestParams = this.rawRequestParams;
        break;

      case RequestParamsType.BasicAuth:
        this.block.requestParams = this.basicAuthRequestParams;
        break;

      case RequestParamsType.Multipart:
        this.block.requestParams = this.multipartRequestParams;
        break;
    }
    this.currentParamsType = newParamsType;
    this.valueChanged();
  }

  useCustomCipherSuitesChanged() {
    this.useCustomCipherSuites = this.block.settings['useCustomCipherSuites'].value;
    this.valueChanged();
  }

  addMultipartString() {
    this.multipartRequestParams.contents = [
      ...this.multipartRequestParams.contents,
      {
        _polyTypeName: MultipartContentType.String,
        name: {
          name: 'name',
          value: '',
          inputVariableName: '',
          inputMode: SettingInputMode.Fixed,
          type: BlockSettingType.String,
        },
        contentType: {
          name: 'contentType',
          value: 'text/plain',
          inputVariableName: '',
          inputMode: SettingInputMode.Fixed,
          type: BlockSettingType.String,
        },
        data: {
          name: 'data',
          value: '',
          inputVariableName: '',
          inputMode: SettingInputMode.Fixed,
          type: BlockSettingType.String,
        },
      },
    ];
    this.valueChanged();
  }

  addMultipartRaw() {
    this.multipartRequestParams.contents = [
      ...this.multipartRequestParams.contents,
      {
        _polyTypeName: MultipartContentType.Raw,
        name: {
          name: 'name',
          value: '',
          inputVariableName: '',
          inputMode: SettingInputMode.Fixed,
          type: BlockSettingType.String,
        },
        contentType: {
          name: 'contentType',
          value: 'application/octet-stream',
          inputVariableName: '',
          inputMode: SettingInputMode.Fixed,
          type: BlockSettingType.String,
        },
        data: {
          name: 'data',
          value: '',
          inputVariableName: '',
          inputMode: SettingInputMode.Fixed,
          type: BlockSettingType.ByteArray,
        },
      },
    ];
    this.valueChanged();
  }

  addMultipartFile() {
    this.multipartRequestParams.contents = [
      ...this.multipartRequestParams.contents,
      {
        _polyTypeName: MultipartContentType.File,
        name: {
          name: 'name',
          value: '',
          inputVariableName: '',
          inputMode: SettingInputMode.Fixed,
          type: BlockSettingType.String,
        },
        contentType: {
          name: 'contentType',
          value: 'application/octet-stream',
          inputVariableName: '',
          inputMode: SettingInputMode.Fixed,
          type: BlockSettingType.String,
        },
        fileName: {
          name: 'fileName',
          value: '',
          inputVariableName: '',
          inputMode: SettingInputMode.Fixed,
          type: BlockSettingType.String,
        },
      },
    ];
    this.valueChanged();
  }

  removeMultipart(index: number) {
    this.multipartRequestParams.contents = [
      ...this.multipartRequestParams.contents.slice(0, index),
      ...this.multipartRequestParams.contents.slice(index + 1),
    ];
    this.valueChanged();
  }
}
