import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { FontAwesomeModule } from '@fortawesome/angular-fontawesome';
import { MarkdownModule } from 'ngx-markdown';
import { NgxSpinnerModule } from 'ngx-spinner';
import { AccordionModule } from 'primeng/accordion';
import { AutoCompleteModule } from 'primeng/autocomplete';
import { AvatarModule } from 'primeng/avatar';
import { ButtonModule } from 'primeng/button';
import { CalendarModule } from 'primeng/calendar';
import { CardModule } from 'primeng/card';
import { CheckboxModule } from 'primeng/checkbox';
import { ChipModule } from 'primeng/chip';
import { DialogModule } from 'primeng/dialog';
import { DropdownModule } from 'primeng/dropdown';
import { FileUploadModule } from 'primeng/fileupload';
import { InputTextModule } from 'primeng/inputtext';
import { MenuModule } from 'primeng/menu';
import { MenubarModule } from 'primeng/menubar';
import { MultiSelectModule } from 'primeng/multiselect';
import { PickListModule } from 'primeng/picklist';
import { ProgressBarModule } from 'primeng/progressbar';
import { RadioButtonModule } from 'primeng/radiobutton';
import { TableModule } from 'primeng/table';
import { ToggleButtonModule } from 'primeng/togglebutton';
import { TooltipModule } from 'primeng/tooltip';
import { SharedModule } from '../shared/shared.module';
import { ChangelogComponent } from './components/changelog/changelog.component';
import { ConfigCsharpComponent } from './components/config/config-csharp/config-csharp.component';
import { ConfigDebuggerComponent } from './components/config/config-debugger/config-debugger.component';
import { ViewAsHtmlComponent } from './components/config/config-debugger/view-as-html/view-as-html.component';
import { ConfigLolicodeComponent } from './components/config/config-lolicode/config-lolicode.component';
import { ConfigLoliscriptComponent } from './components/config/config-loliscript/config-loliscript.component';
import { ConfigMetadataComponent } from './components/config/config-metadata/config-metadata.component';
import { ConfigReadmeComponent } from './components/config/config-readme/config-readme.component';
import { ConfigSettingsComponent } from './components/config/config-settings/config-settings.component';
import { AddBlockComponent } from './components/config/config-stacker/add-block/add-block.component';
import { BlockInfoComponent } from './components/config/config-stacker/block-info/block-info.component';
import { BoolSettingComponent } from './components/config/config-stacker/block-info/bool-setting/bool-setting.component';
import { ByteArraySettingComponent } from './components/config/config-stacker/block-info/byte-array-setting/byte-array-setting.component';
import { DictionaryOfStringsSettingComponent } from './components/config/config-stacker/block-info/dictionary-of-strings-setting/dictionary-of-strings-setting.component';
import { EnumSettingComponent } from './components/config/config-stacker/block-info/enum-setting/enum-setting.component';
import { FloatSettingComponent } from './components/config/config-stacker/block-info/float-setting/float-setting.component';
import { IntSettingComponent } from './components/config/config-stacker/block-info/int-setting/int-setting.component';
import { ListOfStringsSettingComponent } from './components/config/config-stacker/block-info/list-of-strings-setting/list-of-strings-setting.component';
import { SettingInputModeComponent } from './components/config/config-stacker/block-info/setting-input-mode/setting-input-mode.component';
import { SettingInputVariableComponent } from './components/config/config-stacker/block-info/setting-input-variable/setting-input-variable.component';
import { StringSettingComponent } from './components/config/config-stacker/block-info/string-setting/string-setting.component';
import { ConfigStackerComponent } from './components/config/config-stacker/config-stacker.component';
import { HttpRequestBlockComponent } from './components/config/config-stacker/http-request-block/http-request-block.component';
import { CreateKeycheckKeyComponent } from './components/config/config-stacker/keycheck-block/create-keycheck-key/create-keycheck-key.component';
import { KeycheckBlockComponent } from './components/config/config-stacker/keycheck-block/keycheck-block.component';
import { LolicodeBlockComponent } from './components/config/config-stacker/lolicode-block/lolicode-block.component';
import { ParseBlockComponent } from './components/config/config-stacker/parse-block/parse-block.component';
import { ScriptBlockComponent } from './components/config/config-stacker/script-block/script-block.component';
import { EditConfigImageComponent } from './components/config/edit-config-image/edit-config-image.component';
import { ConfigsComponent } from './components/configs/configs.component';
import { UploadConfigsComponent } from './components/configs/upload-configs/upload-configs.component';
import { CreateGuestComponent } from './components/guests/create-guest/create-guest.component';
import { GuestsComponent } from './components/guests/guests.component';
import { UpdateGuestInfoComponent } from './components/guests/update-guest-info/update-guest-info.component';
import { UpdateGuestPasswordComponent } from './components/guests/update-guest-password/update-guest-password.component';
import { HitsComponent } from './components/hits/hits.component';
import { UpdateHitComponent } from './components/hits/update-hit/update-hit.component';
import { HomeComponent } from './components/home/home.component';
import { RecentHitsChartComponent } from './components/home/recent-hits-chart/recent-hits-chart.component';
import { SysperfCardsComponent } from './components/home/sysperf-cards/sysperf-cards.component';
import { ContributorComponent } from './components/info/contributor/contributor.component';
import { InfoComponent } from './components/info/info.component';
import { AddActionComponent } from './components/job-monitor/edit-triggered-action/add-action/add-action.component';
import { AddTriggerComponent } from './components/job-monitor/edit-triggered-action/add-trigger/add-trigger.component';
import { EditTriggeredActionComponent } from './components/job-monitor/edit-triggered-action/edit-triggered-action.component';
import { JobMonitorComponent } from './components/job-monitor/job-monitor.component';
import { CreateJobComponent } from './components/jobs/create-job/create-job.component';
import { ConfigureCustomWebhookComponent } from './components/jobs/edit-multi-run-job/configure-custom-webhook/configure-custom-webhook.component';
import { ConfigureDiscordComponent } from './components/jobs/edit-multi-run-job/configure-discord/configure-discord.component';
import { ConfigureTelegramComponent } from './components/jobs/edit-multi-run-job/configure-telegram/configure-telegram.component';
import { EditMultiRunJobComponent } from './components/jobs/edit-multi-run-job/edit-multi-run-job.component';
import { EditProxyCheckJobComponent } from './components/jobs/edit-proxy-check-job/edit-proxy-check-job.component';
import { JobsComponent } from './components/jobs/jobs.component';
import { HitLogComponent } from './components/jobs/multi-run-job/hit-log/hit-log.component';
import { MultiRunJobComponent } from './components/jobs/multi-run-job/multi-run-job.component';
import { ProxyCheckJobComponent } from './components/jobs/proxy-check-job/proxy-check-job.component';
import { SelectConfigComponent } from './components/jobs/select-config/select-config.component';
import { SelectWordlistComponent } from './components/jobs/select-wordlist/select-wordlist.component';
import { MenuItemComponent } from './components/menu/menu-item/menu-item.component';
import { MenuComponent } from './components/menu/menu.component';
import { NotFoundComponent } from './components/not-found/not-found.component';
import { AddThemeComponent } from './components/ob-settings/add-theme/add-theme.component';
import { ChangeAdminApiKeyComponent } from './components/ob-settings/change-admin-api-key/change-admin-api-key.component';
import { ChangeAdminPasswordComponent } from './components/ob-settings/change-admin-password/change-admin-password.component';
import { CreateCustomSnippetComponent } from './components/ob-settings/create-custom-snippet/create-custom-snippet.component';
import { CreateProxyCheckTargetComponent } from './components/ob-settings/create-proxy-check-target/create-proxy-check-target.component';
import { CreateRemoteConfigsEndpointComponent } from './components/ob-settings/create-remote-configs-endpoint/create-remote-configs-endpoint.component';
import { OBSettingsComponent } from './components/ob-settings/ob-settings.component';
import { UpdateCustomSnippetComponent } from './components/ob-settings/update-custom-snippet/update-custom-snippet.component';
import { UpdateProxyCheckTargetComponent } from './components/ob-settings/update-proxy-check-target/update-proxy-check-target.component';
import { UpdateRemoteConfigsEndpointComponent } from './components/ob-settings/update-remote-configs-endpoint/update-remote-configs-endpoint.component';
import { AddPluginComponent } from './components/plugins/add-plugin/add-plugin.component';
import { PluginsComponent } from './components/plugins/plugins.component';
import { CreateProxyGroupComponent } from './components/proxies/create-proxy-group/create-proxy-group.component';
import { DeleteSlowProxiesComponent } from './components/proxies/delete-slow-proxies/delete-slow-proxies.component';
import { ImportProxiesFromFileComponent } from './components/proxies/import-proxies-from-file/import-proxies-from-file.component';
import { ImportProxiesFromRemoteComponent } from './components/proxies/import-proxies-from-remote/import-proxies-from-remote.component';
import { ImportProxiesFromTextComponent } from './components/proxies/import-proxies-from-text/import-proxies-from-text.component';
import { ProxiesComponent } from './components/proxies/proxies.component';
import { ProxySyntaxInfoComponent } from './components/proxies/proxy-syntax-info/proxy-syntax-info.component';
import { UpdateProxyGroupComponent } from './components/proxies/update-proxy-group/update-proxy-group.component';
import { RlSettingsComponent } from './components/rl-settings/rl-settings.component';
import { CreateEndpointComponent } from './components/sharing/create-endpoint/create-endpoint.component';
import { SharingComponent } from './components/sharing/sharing.component';
import { AddWordlistComponent } from './components/wordlists/add-wordlist/add-wordlist.component';
import { UpdateWordlistInfoComponent } from './components/wordlists/update-wordlist-info/update-wordlist-info.component';
import { UploadWordlistComponent } from './components/wordlists/upload-wordlist/upload-wordlist.component';
import { WordlistsComponent } from './components/wordlists/wordlists.component';
import { ErrorDetailsComponent } from './components/error-details/error-details.component';
import { MainRoutingModule } from './main-routing.module';
import { MainComponent } from './main.component';
import { CustomInputsComponent } from './components/jobs/multi-run-job/custom-inputs/custom-inputs.component';
import { BaseChartDirective } from 'ng2-charts';

@NgModule({
  declarations: [
    MenuComponent,
    MainComponent,
    HomeComponent,
    MenuItemComponent,
    NotFoundComponent,
    SysperfCardsComponent,
    InfoComponent,
    ContributorComponent,
    GuestsComponent,
    CreateGuestComponent,
    UpdateGuestInfoComponent,
    UpdateGuestPasswordComponent,
    PluginsComponent,
    AddPluginComponent,
    SharingComponent,
    CreateEndpointComponent,
    OBSettingsComponent,
    CreateProxyCheckTargetComponent,
    UpdateProxyCheckTargetComponent,
    CreateCustomSnippetComponent,
    UpdateCustomSnippetComponent,
    ChangeAdminPasswordComponent,
    CreateRemoteConfigsEndpointComponent,
    UpdateRemoteConfigsEndpointComponent,
    RlSettingsComponent,
    ProxiesComponent,
    CreateProxyGroupComponent,
    UpdateProxyGroupComponent,
    DeleteSlowProxiesComponent,
    ImportProxiesFromTextComponent,
    ProxySyntaxInfoComponent,
    ImportProxiesFromRemoteComponent,
    ImportProxiesFromFileComponent,
    HitsComponent,
    UpdateHitComponent,
    WordlistsComponent,
    ErrorDetailsComponent,
    UpdateWordlistInfoComponent,
    UploadWordlistComponent,
    AddWordlistComponent,
    ConfigsComponent,
    UploadConfigsComponent,
    ConfigMetadataComponent,
    EditConfigImageComponent,
    ConfigReadmeComponent,
    ConfigSettingsComponent,
    ConfigLolicodeComponent,
    ConfigCsharpComponent,
    ConfigDebuggerComponent,
    ViewAsHtmlComponent,
    JobsComponent,
    CreateJobComponent,
    EditProxyCheckJobComponent,
    ProxyCheckJobComponent,
    EditMultiRunJobComponent,
    MultiRunJobComponent,
    AddThemeComponent,
    SelectConfigComponent,
    ConfigureDiscordComponent,
    ConfigureTelegramComponent,
    ConfigureCustomWebhookComponent,
    SelectWordlistComponent,
    JobMonitorComponent,
    EditTriggeredActionComponent,
    AddTriggerComponent,
    AddActionComponent,
    HitLogComponent,
    ConfigStackerComponent,
    AddBlockComponent,
    BlockInfoComponent,
    BoolSettingComponent,
    SettingInputModeComponent,
    SettingInputVariableComponent,
    StringSettingComponent,
    IntSettingComponent,
    FloatSettingComponent,
    EnumSettingComponent,
    ByteArraySettingComponent,
    ListOfStringsSettingComponent,
    DictionaryOfStringsSettingComponent,
    ParseBlockComponent,
    LolicodeBlockComponent,
    ScriptBlockComponent,
    KeycheckBlockComponent,
    HttpRequestBlockComponent,
    CreateKeycheckKeyComponent,
    RecentHitsChartComponent,
    ChangelogComponent,
    ConfigLoliscriptComponent,
    ChangeAdminApiKeyComponent,
    CustomInputsComponent,
  ],
  imports: [
    CommonModule,
    MainRoutingModule,
    FontAwesomeModule,
    FormsModule,
    ReactiveFormsModule,
    CardModule,
    ButtonModule,
    AutoCompleteModule,
    AvatarModule,
    TooltipModule,
    InputTextModule,
    ToggleButtonModule,
    RadioButtonModule,
    MenuModule,
    ProgressBarModule,
    DropdownModule,
    MenubarModule,
    MultiSelectModule,
    PickListModule,
    AccordionModule,
    CheckboxModule,
    CalendarModule,
    FileUploadModule,
    DialogModule,
    TableModule,
    ChipModule,
    BaseChartDirective,
    MarkdownModule.forRoot(),
    SharedModule,
    NgxSpinnerModule,
  ],
  providers: [],
})
export class MainModule { }
