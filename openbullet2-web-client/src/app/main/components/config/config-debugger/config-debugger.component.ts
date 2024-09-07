import { Component, EventEmitter, Input, OnDestroy, OnInit, Output, ViewChild } from '@angular/core';
import {
  faAlignLeft,
  faBug,
  faCaretRight,
  faPlay,
  faSliders,
  faStop,
  faWindowMaximize,
} from '@fortawesome/free-solid-svg-icons';
import { MessageService } from 'primeng/api';
import { Subscription } from 'rxjs';
import { VariableDto } from 'src/app/main/dtos/config-debugger/messages';
import { ConfigDto } from 'src/app/main/dtos/config/config.dto';
import { EnvironmentSettingsDto } from 'src/app/main/dtos/settings/environment-settings.dto';
import { BotLoggerEntry, ConfigDebuggerSettings } from 'src/app/main/models/config-debugger-settings';
import { ConfigDebuggerSettingsService } from 'src/app/main/services/config-debugger-settings.service';
import { ConfigDebuggerHubService } from 'src/app/main/services/config-debugger.hub.service';
import { ConfigService } from 'src/app/main/services/config.service';
import { UserService } from 'src/app/main/services/user.service';
import { TruncatePipe } from 'src/app/shared/pipes/truncate.pipe';
import { ViewAsHtmlComponent } from './view-as-html/view-as-html.component';

@Component({
  selector: 'app-config-debugger',
  templateUrl: './config-debugger.component.html',
  styleUrls: ['./config-debugger.component.scss'],
  providers: [TruncatePipe],
})
export class ConfigDebuggerComponent implements OnInit, OnDestroy {
  @Input() config!: ConfigDto;
  @Input() envSettings!: EnvironmentSettingsDto;

  @Output() currentWordlistTypeChanged = new EventEmitter<string>();

  @ViewChild('viewAsHtmlComponent')
  htmlViewer: ViewAsHtmlComponent | undefined = undefined;

  settings: ConfigDebuggerSettings | null = null;
  wordlistTypes: string[] = ['Default'];
  proxyTypes: string[] = ['http', 'socks4', 'socks4a', 'socks5'];

  faPlay = faPlay;
  faBug = faBug;
  faStop = faStop;
  faCaretRight = faCaretRight;
  faAlignLeft = faAlignLeft;
  faSliders = faSliders;
  faWindowMaximize = faWindowMaximize;

  logs: BotLoggerEntry[] = [];
  variables: VariableDto[] = [];
  status = 'unknown';

  displayVariables = false;
  viewAsHtmlModalVisible = false;
  html = '';

  stateSubscription: Subscription | null = null;
  logsSubscription: Subscription | null = null;
  variablesSubscription: Subscription | null = null;
  statusSubscription: Subscription | null = null;
  errorSubscription: Subscription | null = null;

  private debuggerHubService = new ConfigDebuggerHubService(this.userService);

  constructor(
    private debuggerSettingsService: ConfigDebuggerSettingsService,
    private configService: ConfigService,
    private messageService: MessageService,
    private truncatePipe: TruncatePipe,
    private userService: UserService,
  ) { }

  ngOnInit() {
    this.wordlistTypes = this.envSettings.wordlistTypes.map((t) => t.name);
    this.settings = this.debuggerSettingsService.loadLocalSettings();

    // If the current wordlist type is null or not in the list,
    // set it to the first one in the list
    if (this.settings?.wordlistType !== null && !this.wordlistTypes.includes(this.settings.wordlistType)) {
      this.settings.wordlistType = this.wordlistTypes[0];
    }

    this.currentWordlistTypeChanged.emit(this.settings.wordlistType);

    this.debuggerHubService.createHubConnection(this.config.id).then((_) => {
      // Request the current state
      this.debuggerHubService.getState();
    });

    // When the state arrives, set current variables
    this.stateSubscription = this.debuggerHubService.state$.subscribe((msg) => {
      if (msg === null || msg === undefined) {
        return;
      }

      this.logs = msg.log;
      this.variables = this.settings?.groupCaptures
        ? [
          ...msg.variables.filter((v) => !v.markedForCapture),
          ...msg.variables.filter((v) => v.markedForCapture)
        ] : msg.variables;
      this.status = msg.status;

      this.onNewState();
    });
  }

  ngOnDestroy(): void {
    this.debuggerHubService.stopHubConnection();

    this.stateSubscription?.unsubscribe();
    this.logsSubscription?.unsubscribe();
    this.variablesSubscription?.unsubscribe();
    this.statusSubscription?.unsubscribe();
    this.errorSubscription?.unsubscribe();
  }

  onNewState() {
    // When we get the debugger state, we can start listening to
    // new messages
    // TODO: Handle the case where other messages arrive before
    // the state message
    this.logsSubscription = this.debuggerHubService.logs$.subscribe((msg) => {
      if (msg === null || msg === undefined) {
        return;
      }

      this.logs = [...this.logs, msg.newMessage];
      this.scrollToBottom();
    });

    this.variablesSubscription = this.debuggerHubService.variables$.subscribe((msg) => {
      if (msg === null || msg === undefined) {
        return;
      }

      this.variables = this.settings?.groupCaptures
        ? [
          ...msg.variables.filter((v) => !v.markedForCapture),
          ...msg.variables.filter((v) => v.markedForCapture)
        ] : msg.variables;
    });

    this.statusSubscription = this.debuggerHubService.status$.subscribe((msg) => {
      if (msg === null || msg === undefined) {
        return;
      }

      this.status = msg.newStatus;

      // Needed because otherwise the scroll is so fast that
      // it happens before the new element is actually rendered
      // to the page.
      setTimeout(() => {
        this.scrollToBottom();
      }, 200);
    });

    this.errorSubscription = this.debuggerHubService.error$.subscribe((msg) => {
      if (msg === null || msg === undefined) {
        return;
      }

      this.messageService.add({
        severity: 'error',
        summary: 'Debugger Error',
        detail: this.truncatePipe.transform(msg.message, 100),
      });

      this.scrollToBottom();
    });
  }

  start() {
    if (!this.settings?.persistLog) {
      this.logs = [];
    }

    // Save the config on the backend but without persisting the changes.
    // We need this in order to properly debug any new changes
    this.configService.saveConfig(this.config, false).subscribe((_) => {
      // Then, send the start message to the debugger
      this.debuggerHubService.start(this.settings!);
    });
  }

  stop() {
    this.debuggerHubService.stop();
  }

  takeStep() {
    this.debuggerHubService.takeStep();
  }

  viewAsHtml(entry: BotLoggerEntry) {
    this.html = entry.message;
    this.viewAsHtmlModalVisible = true;
  }

  scrollToBottom() {
    const elem = document.querySelector('.logs-container');
    elem?.scrollTo(0, elem.scrollHeight);
  }

  localSave() {
    if (this.settings !== null) {
      this.debuggerSettingsService.saveLocalSettings(this.settings);
    }
  }

  formatVariable(variable: VariableDto): string {
    if (variable.value === null) {
      return 'null';
    }

    switch (variable.type) {
      case 'string':
      case 'byteArray':
        return variable.value;

      case 'int':
      case 'float':
      case 'bool':
        return variable.value.toString();

      case 'listOfStrings':
        return `[${variable.value.join(', ')}]`;

      case 'dictionaryOfStrings':
        return `{${Object.keys(variable.value).map((k) => `(${k}, ${variable.value[k]})`).join(', ')}}`;

      default:
        return '[UNKNOWN TYPE]';
    }
  }

  invariantDisplayFunction(x: string) {
    return x;
  }
}
