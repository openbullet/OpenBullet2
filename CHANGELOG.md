## 2.0.1 (2026-06-27)
This patch release focuses on stability, security and compatibility fixes after `2.0.0`.

##### Security and Reliability
- Prevented ZIP Slip attacks during plugin installation (by GekySan)
- Fixed plugin installation when plugin directories do not exist yet
- Removed the plugin package size limit
- Fixed an idempotency issue in the web performance monitor service
- Added a fallback for native logging when Serilog configuration is missing from `appsettings.json`

##### Blocks and Automation
- Added a `JwtDecode` block
- Normalized Unix virtual environment paths before initializing Python through `CSnakes`
- Improved constant block logging so only generated values are printed

##### Requests, Proxies and Data
- Added HTTPS proxy support across proxy parsing, block settings and request stacks
- Added random profile options for `curl-impersonate`
- Improved `curl-impersonate` debug logs by showing the actual sent headers
- Fixed header values being split on commas in `curl-impersonate`
- Fixed `curl-impersonate` native loading on macOS arm64 (by meinname)
- Fixed zstd response decoding in the `System.Net` handler (by GekySan)
- Ignored malformed `Set-Cookie` headers instead of failing request processing
- Fixed proxiless requests incorrectly using the proxy connect timeout
- Requested exact HTTP/3 when `3.0` is selected in the `System.Net` HTTP stack

##### OpenBullet (Web)
- Updated changelog/update handling in the web API and client
- Fixed detailed multi-run job view not showing all bots

##### OpenBullet (Native)
- Fixed changelog window timing issues
- Fixed `TimeSpanPicker` binding
- Displayed underscores correctly in multi-run job custom inputs

## 2.0.0 (2026-06-06)
**This is a huge release with lots of improvements. Please discard your old updater and get the latest one, otherwise the update process will break.**

##### Important: New Release Packaging
- Release archives are now platform-specific and self-contained, so you must download the package that matches your OS and CPU architecture
- Packaged releases no longer require a separate `.NET` runtime installation
- You must redownload the updater for this release, because older updater binaries still look for the old package path/layout and will not find the new assets
- Web builds are now published for `Windows`, `Linux` and `macOS` on `x64` and `arm64`
- Native builds are now published for `Windows` on `x64` and `arm64`
- `x86` builds are no longer produced

This is a big staging release focused on new features, quality-of-life improvements and reliability across the app.
It also includes a lot of maintenance work behind the scenes, including the move to `.NET 10`.
If you build from source or run outside the packaged self-contained releases, this version requires `.NET 10`.

##### General
- OpenBullet 2 now runs on `.NET 10`
- Added support for `zstd` compression (by Tarcisio)
- Updated built-in user agents (by RealSadmc)
- Improved overall security in several areas
- Updated many dependencies across the project

##### Parallelization, Jobs and Hits
- Reworked the main parallelizer to be more stable and efficient
- Improved CPM calculation and CPM limiting
- Better queue handling and bot scheduling during runs
- Reduced CPU waste while a job is being speed-limited
- Fixed edge cases when running with `DoP = 1`
- Fixed several timing issues around stopping, aborting and finishing jobs
- Fixed bugs caused by using job progress as the only signal that work had ended
- Fixed skip count not resetting correctly after a job finished all work
- The older thread-based parallelizer is now obsolete in favor of the newer task-based one
- Added more safeguards against corrupted jobs
- Fixed some database-related errors that could happen when multiple actions happened at the same time
- Fixed hit saving concurrency issues, especially in the native client
- Fixed in-memory hit collection concurrency issues
- Hits are ordered correctly again
- Hits no longer show a trailing separator when there is no captured data
- File-system hit output now supports templating in the base directory
- Freed RAM more reliably when deleting jobs and bot data
- Add option to not cache hits in RAM during a multi run job execution

##### Blocks and Automation
- Added a new DNS block
- Added a new `GenerateGuid` block
- Added `Playwright` as a browser automation engine
- Reworked the old Puppeteer-only browser blocks into generic browser blocks that work across browser engines
- Added `Ghost Cursor` mouse automation with configurable movement, click and scroll behavior
- Added config and global settings to choose the browser engine and Playwright browser source/family
- Added real CPython support in script blocks through the `Python` interpreter, using either a local `.venv` or an auto-downloaded redistributable runtime
- Improved block categories with a better hierarchical category tree
- Reworked the Scrypt block and added a new key-derivation variant
- Added quantifier mask support to the `RandomString` block
- Improved `RandomString` performance
- Added support for `long` and `double` values in variables, numeric conversions and block settings without changing existing `int`/`float` behavior
- Added interpolated string support to `LOG`, `CLOG` and `SET` statements
- Added a timeout option to the shell command block
- Added an option to ignore certificate validation in the HTTP Request block
- Added an option to only use known mail servers in SMTP, IMAP and POP3 blocks
- Added blocks to get the current URL in Selenium and Puppeteer
- Added click-at-coordinates blocks for Selenium and Puppeteer
- Added support for authenticated proxies in Selenium
- Added better step-by-step debugging, including variables shown between steps
- Added support for step-by-step debugging in legacy LoliScript configs
- Added an action to set the skip count from the job monitor
- Triggered actions can now target the current job more easily when no target job is set
- Triggered actions now require at least one trigger and one action
- Added support for escaping angle brackets in interpolated LoliCode strings by doubling them
- Cleaner saved LoliCode output by skipping default interpolated settings
- Fixed issues when transpiling empty LoliCode to C#
- Fixed the problem where Node.js modules were sometimes not detected
- Fixed the `TcpSendReadHttp` block
- Fixed the keycheck header being logged multiple times
- Fixed an issue with XPath in Puppeteer
- Fixed empty-response handling in Puppeteer wait-for-response
- Fixed Selenium screenshots on Linux

##### Requests, Proxies and Data
- Improved cookie handling to make requests more reliable
- Improved HTTP stream decoding
- Improved multipart request handling
- Multipart form data no longer forces an empty `Content-Type` header when you leave it blank
- Allowed longer timeouts in the `System.Net` HTTP client
- Added `curl-impersonate` as an HTTP Request library for browser-like TLS and HTTP fingerprinting
- Added support for URI-style proxies such as `http://host:port` and `socks5://username:password@host:port`
- Proxy check jobs can now use proxy judges to detect anonymity quality
- Added proxy judge URL settings
- Proxies now track anonymity quality (`Unknown`, `Transparent`, `Anonymous`, `Elite`)
- Added proxy quality filters and bulk deletion of low-quality proxies
- Fixed plain HTTP requests over HTTP proxies
- Prefer IPv4 over IPv6 when resolving hosts
- Improved proxy handling when many bots use the same pool at once
- Improved SOCKS proxy reliability
- Removed duplicate proxies across sources when reloading
- Fixed the total proxy count when only untested proxies are shown
- Improved proxy comparisons
- Improved JSON parsing and the JSON viewer
- Updated the Thunderbird mail autoconfig source
- Cached captcha balance checks briefly to avoid rate limits
- Fixed non-working proxies being marked incorrectly and getting unbanned again
- Wordlists can now handle line counts above the 32-bit integer limit

##### Logging and Diagnostics
- The native client now writes its own application logs to files
- Startup scripts, job status changes and job errors are now logged more clearly
- Inner exceptions are now included in the exception log
- Memory usage reporting is now based on private working set for more accurate monitoring
- Reloading configs is now safer when other actions are happening at the same time
- LoliCode parsing errors now include clearer line and column details, including in custom/raw blocks and enum/list/dictionary settings

##### Config Editing
- Improved the web LoliCode editor with better Monaco font handling, block suggestions and snippet previews
- Added resizable splitters to the web config editor pages, including Stacker and code editors
- Synced the selected block between the Stacker and the LoliCode editor
- Added a test section for data rules in the web client
- Added validation for data rules
- Added suggestions for slice names in data rules
- You can now paste a config image directly from the clipboard
- Avoided crashes caused by invalid config metadata images

##### OpenBullet (Web)
- Added a button to collapse the sidebar
- Added a clear button for date filters in the hits page
- Added a config name filter in the hits page
- Sorted config names alphabetically in hits filters
- Remembered the configs and wordlists table state across page reloads
- Added MCP server support at `/mcp`, including tools for configs, settings, environment, debugging and block reference
- Fixed add/upload wordlist dialogs always using the default wordlist type
- Improved markdown rendering
- Fixed some editor font measurement issues
- Improved the way job logs are shown in the web client
- Large bot log entries are now collapsed by default
- Improved the Stacker layout on high-resolution screens
- Improved the UX when starting the config debugger
- Added warning output when testing data rules fails
- Fixed a config debugger issue where the SignalR websocket could hang
- Added browser automation, Ghost Cursor and Playwright settings to the web editor and settings pages

##### OpenBullet (Native)
- Added the job monitor to the native client
- Added save toasts in the native client
- Added the multipart HTTP request settings editor
- Made it easier to change the number of bots
- Kept custom inputs when editing a multi-run job
- Prevented editing a running job
- Added a warning when selecting a dangerous config, with an option to disable it in settings
- Fixed issues when copying hits
- Fixed Telegram chat ID limits
- Fixed empty plugin popup warnings
- Fixed the debugger getting stuck after a failed proxy parse
- Fixed block cloning issues in the native config stacker
- Added the missing options button to the proxy check job viewer
- Added clearer save feedback in several native editor pages
- Added browser automation, Ghost Cursor and Playwright settings to the native editor and settings pages
- Added proxy judge settings, proxy quality filtering and low-quality proxy cleanup tools

##### Console and Updater
- Added a single-run debug mode to the console app
- Added a final variables report table to the console app
- Fixed the console app trying to run with 0 bots
- Release packages are now split by platform and architecture and include a self-contained runtime
- The updater now selects the correct platform-specific package, including `win-arm64` and `osx` web builds
- The updater now downloads to a temporary file instead of memory
- The updater now installs OpenBullet 2 in the correct executable directory
- The updater is safer against malformed archive contents
- Improved updater reliability on clean installs and folder-based installs
- Updater-related memory usage and cleanup behavior were improved
- You can now choose whether update alerts follow the release channel, staging channel or are disabled

##### Jobs and Monitoring
- Added last-run outcome tracking for jobs
- The play button in the job manager now resumes a paused job instead of always starting a new run
- Improved how job progress is displayed in the UI
- Custom input answers are now saved with job options
- Improved the start-condition UX when starting jobs immediately

## 0.3.2 (2024-09-07)
This release contains some bugfixes and improvements.
Most notably, `CaptchaSharp` has been upgraded to version `2.1.0` and **many new captcha-related blocks and services have been added**.
Check them out in the "RL Settings" and Stacker!

Other changes:

##### RuriLib
- Added support for parsing comma-separated cookies from the `Set-Cookie` and `Set-Cookie2` headers in the `RuriLib.Http` client. *This breaks support for cookie values that contain commas, but [they are disallowed](https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/Set-Cookie) by the standard anyway* 
- Removed auth constraint from SMTP blocks that send mails, allowing to send mails as an anonymous user on servers that allow it
- Fixed/Added support for the `ListOfStrings` and `DictionaryOfStrings` output variables when using Node.js in the Script block
- Removed support for the CapSolver service upon request from the service owner
- Added missing timeouts to `NoProxyClient`'s `ProxySettings`
- Added elliptic curve support for JWT signature (by GekySan)

##### OpenBullet (Web)
- Fixed missing suggestions in variable-mode input fields
- Removed double labels from some boolean parameters for a cleaner UI
- Added try/catch in `ReadNetworkUsage` due to a macOS issue that caused the program to crash

##### OpenBullet (Native)
- Fixed annoying auto word selection in Windows Forms' RichTextBoxes

## 0.3.1 (2024-07-10)
This release is mostly a bugfix release. The main changes are:

##### RuriLib
- Changed the script block for NodeJS to write the script to a string in the resulting C# code instead of using an external file (by rudyrdx)
- Added workaround to fix PuppeteerSwitchToTab block
- Added more IMAP blocks, mainly to switch folders (e.g. if the mail is in the spam folder instead of the inbox)
- Fixed `ReadResponseContent = false` of the HTTP Request block not working when using the `System.Net` library
- Implemented RSA signing for JWT (by GekySan)

##### OpenBullet (Web)
- Fixed incorrect function calls when trying to edit or clone a proxy check job
- Added warning upon disabling require admin login
- Fixed API Key generation in the Sharing section
- Added version to console output when the program starts
- Fixed wordlist type not being kept in the config debugger
- Added autocomplete for custom input answers and prefilled the default answers
- Fixed the date filter for hits (it was not taking the timezone into account)

##### OpenBullet (Native)
- Fixed multi-line text boxes in RL Settings (global ban keys, global retry keys and custom user agents were not being saved correctly)

## 0.3.0 (2024-07-01)
The **new web client** is now available! The main features include:
- A completely redesigned UI built with Angular
- An API to interact with the backend, that can also be used by third party applications via an API key
- Several fixes and improvements to the backend
- A brand new updater
- OpenBullet 2 now runs on .NET 8

Please read more on the [official announcements](https://discourse.openbullet.dev/c/announcements/6).

## 0.2.5 (2024-07-01)
A **new feature** called *startup LoliCode* is available! In the LoliCode editor you will find a new toggleable section where you can write some LoliCode that will be executed once when the job starts. You can use it for example to set global variables, like a session cookie, or read the lines of a file to consume later or anything else. You can use blocks in here but be aware that the variables you set (unless they are set via e.g. `globals.myVariable = ...`) will be cleared once the startup phase is complete.

##### RuriLib
- Added TCP blocks to work with bytes
- Added ssl options support to SystemNet with http proxies and proxiless
- Added BlockParam attribute for pretty names and description tooltips
- Fixed NRE on timer tick if proxyPool is null
- Added support for auto casting dynamics when passing them to blocks
- Added default values for a couple keychain keys
- Added info about data pool in WordlistId and WordlistName fields of hits
- Do not read response content on HEAD requests
- Fixed Selenium not closing properly
- Support for scripts as proxy file source (by AmyBergqvist)
- Added `asyncLocker` when using `ProxyPool.ReloadAll()` (by AmyBergqvist)
- Performance improvements (by AmyBergqvist)
- Fixed HttpClient from System.Net not reading the Set-Cookie header
- Fixed assignment of lists from jint to C#
- Fixed some issues with starting and stopping jobs

#### CaptchaSharp
- Fixed `captchas.io`
- Added `capsolver.com`

##### OpenBullet (Core)
- Fixed FileDataPool not accepting double quoted paths on Windows

##### OpenBullet (Web)
- Fixed regex helper crashing on dash in supported symbols
- Added auto start for JobManagerService on boot

##### OpenBullet (Native)
- Sorted jobs by their ID in job manager
- Fixed bug with send to recheck
- Added timestamp to job log
- Added search panel to script editors, can be opened with CTRL+F
- Added auto start for JobManagerService on boot

## 0.2.4 (2022-04-28)
##### RuriLib
- Add backoff logic to proxy reload in `ProxyPool`
- Automatically decompress brotli streams in `HttpClient`
- Added support for `BOTNUM` variable
- Added SAFE mode to `Parse` and `HttpRequest` blocks
- Added `extraCmdLineArgs` to `Puppeteer/Selenium OpenBrowser` blocks
- Added `CharAt` block

##### OpenBullet (Core)
- 

##### OpenBullet (Web)
- Added word wrap setting for text editors under OB Settings > Customization 

##### OpenBullet (Native)
- Added word wrap setting for text editors under OB Settings > Customization

## 0.2.3 (2022-03-13)
##### RuriLib
- Added log after writing variables in blocks with output variable(s)

##### OpenBullet (Core)
- 

##### OpenBullet (Web)
- Added step by step debugger mode
- Fixed bug when cloning blocks that have an enum parameter
- Updated sqlite package to support Mac M1

##### OpenBullet (Native)
- Added step by step debugger mode
- Fixed bug when cloning blocks that have an enum parameter

## 0.2.2 (2022-03-09)
##### RuriLib
- Fixed time format in log
- Added support for Chrome extensions in Puppeteer
- Added support for logging any kind of object with `LOG` and `CLOG` in LoliCode (not just strings)
- Added `MultiLine` attribute support to string parameters (will be displayed as a text area instead of a string).
- Added `MultiLine` to some existing blocks (you can use it in plugins too!)
- Added support for using the `Description` attribute in enums (you can use it in plugins too!)

##### OpenBullet (Core)
- Added failsafe when deserializing `triggeredActions.json`

##### OpenBullet (Web)
- Fixed problem with editing wordlist type not being persisted without reloading the wordlist in the job

##### OpenBullet (Native)
- Fixed bug with keychains reordering
- Added prompt if jobs are running when quitting
- Fixed bug where deleting a group would delete all proxies

## 0.2.1 (2022-02-15)
##### RuriLib
- Added AWS4 signature block (by tcortega)
- Added option to automatically decode HTML in Http Request Block (by tcortega)
- Improved plugin system (added `BlockAction` and `BlockImage` attributes)

##### OpenBullet (Core)
- 

##### OpenBullet (Web)
- Update skip count on different wordlist selection (by tcortega)
- Added setting to print all lines to job log with their status (by tcortega)
- Added support for loading `.loli` configs directly with the Upload button
- Added suggestion to switch to interpolated mode when using `<` and `>` in fixed mode

##### OpenBullet (Native)
- Update skip count on different wordlist selection (by tcortega)
- Added setting to print all lines to job log with their status (by tcortega)
- Enhanced data tester in Config > Settings > Data section (by tcortega)
- Fixed author field in Config > Metadata not being saved properly (by tcortega)
- Removed the 'is capture' box if the block has no return value
- Added variable suggestions when block settings are in VAR mode
- Fixed a memory leak related to the job log
- Added suggestion to switch to interpolated mode when using `<` and `>` in fixed mode
- Added license info

#### Docker
- Added puppeteer support
- Removed verbose logs on startup

## 0.2.0 (2022-01-27)
OpenBullet 2 now runs on .NET 6. If you don't have it yet, please download the **ASP.NET Core Runtime 6.x** for the web client or the **.NET Desktop Runtime 6.x** for your OS from [this page](https://dotnet.microsoft.com/en-us/download/dotnet/6.0)

*Note that old compiled configs and plugins are not guaranteed to work with .NET 6 so please upgrade them if needed.*

##### RuriLib
- Converted to .NET 6
- Added Parallel.ForEach based parallelizer for testing purposes
- Added HttpLibrary setting to Http Request block. Now you will be able to choose between RuriLib.Http and System.Net to send your HTTP requests. With .NET 6, System.Net supports SOCKS proxies as well.
- Added HTTP/2.0 support (only for the System.Net library)
- Fixed AesEncrypt block and added AesEncryptString and AesDecryptString blocks to make it easier to work with strings
- Added BCryptHash, BCryptHashGenSalt and BCryptVerify blocks (you can avoid using bcryptjs from now on)

##### OpenBullet (Core)
- 

##### OpenBullet (Web)
- Minor fixes

##### OpenBullet (Native)
- Changed the font in all textboxes to Consolas

## 0.1.28 (2022-01-04)
##### RuriLib
- Fixed issue with multiple select by text in puppeteer
- Fixed `PuppeteerGetCookies`
- Added timeout to ws read block
- Added `FtpGetLog` block
- Fixed opening new tab and switching tab in Puppeteer
- Added `CreateListOfNumbers` block
- Prevented starting a job more than once at a time
- Added `WsSendRaw` block
- Added support for reading binary ws messages
- Fixed selenium browser being disposed on debugger end

##### CaptchaSharp
- Added `CustomAntiCaptcha` and `AnyCaptcha` services
- Fixed report incorrect captcha for `AntiCaptcha`
- Added capy support to `TwoCaptcha`

##### OpenBullet (Core)
- 

##### OpenBullet (Web)
- Fixed problem with absolute start condition not being set
- Added bounds to session lifetime
- Fixed problems with JobMonitor save
- Fixed visual bug when quitting edit modals
- Fixed config submenu disappearing on login
- Added support for selecting multiple blocks with SHIFT
- Added Download All buttons to configs section
- Changed proxy group id to name in MultiRunJobViewer
- Added new application icons

##### OpenBullet (Native)
- Disregard task exceptions (no more annoying popups while running)
- Clear debugger log when loading another config in native client
- Fixed problem with wait not being displayed
- Added support for selecting multiple blocks with SHIFT
- Added custom inputs display to MultiRunJobViewer

## 0.1.27 (2021-11-25)
**Support for debugging and running legacy .loli configs from OB1** has been added. To import your configs, place them in the `UserData/Configs` folder and hit the Reload button in the configs section. OB2 will automatically repackage them into .opk files so make sure you have a backup before doing this! Also note that not every single function is supported, if you find something that is missing and would like us to implement, please open an issue on the official github repository.

Other fixes:

##### RuriLib
- Added selenium blocks to use selenium in OB2 configs as well
- Added custom headers parameter to Websocket Connect (PR by Mostafa-Mahdi)
- Fixed and changed the signature of RSAPkcs1Pad2 block
- Allowed the `Connection: Keep-Alive` header in the HttpRequest block
- Updated to latest CaptchaSharp (fixed AZCaptcha service)
- Fixed error on utf-8 value for `Content-Encoding` header
- Fixed important error in LRParser with empty delims

##### OpenBullet (Core)
- 

##### OpenBullet (Web)
- Added LoliScript section to edit the code of legacy configs
- Updated the about page

##### OpenBullet (Native)
- Added LoliScript section to edit the code of legacy configs
- Updated the about page

## 0.1.26 (2021-10-13)
##### RuriLib
- Fixed an Exception in some edge cases
- **IMPORTANT** Fix for HTTP proxies
- Added ImapReadMailRaw block

##### OpenBullet (Core)
- 

##### OpenBullet (Web)
- Fixed crash when deleting wordlists not found
- Fixed a bug when importing proxies from big files
- Removed auto-formatting when writing in dictionary fields

##### OpenBullet (Native)
- Fixed visual bugs

## 0.1.25 (2021-09-20)
##### RuriLib
- Added `SET PROXY`, `SET USEPROXY`, `MARK` and `UNMARK` statements
- URLEncode setting in Parse block
- Support for 9kw.eu captcha provider and enterprise reCaptcha v2/v3
- Bug fix for http proxy authentication by ColdHue

##### OpenBullet (Core)
- Set first wordlist type in Environment.ini as default allowed type for new configs
- Fixed a bug when obtaining hits

##### OpenBullet (Web)
- Disabled file system access to wordlists for guests
- Added stats in Proxies section
- Fixed date filter in grids
- Button to delete wordlists not found
- Custom snippets for LoliCode
- ComboBox in Custom Input selection
- Removed js animations in setup screen

##### OpenBullet (Native)
- Fixed interpolated strings highlight
- Added stats in Proxies section
- Saved sort preference between config list views
- Fixed MultiRunJobOptionsDialog size
- Custom snippets for LoliCode
- ComboBox in Custom Input selection

## 0.1.24 (2021-09-14)
##### RuriLib
- Added option to report the last captcha as bad upon RETRY status (in Config Settings)
- Removed trailing semicolon and space after the last cookie in Cookie header

##### OpenBullet (Core)
- 

##### OpenBullet (Web)
- 

##### OpenBullet (Native)
- Added ability to test data rules from the config settings
- Show bot log as normal window instead of dialog
- Added search in debugger log
- Fixed an issue with copying to clipboard
- Improved the HTML viewer

## 0.1.23 (2021-09-08)
##### RuriLib
- Fixed NRE when using AsString() in null ByteArrayVariable

##### OpenBullet (Core)
- 

##### OpenBullet (Web)
- 

##### OpenBullet (Native)
- Fixed color of progress text in progress bar
- Fixed ugly grey button color on mouse hover
- Fixed bug when deleting proxies
- Fixed some unhandled task exceptions
- Fixed slow proxy/hit deletion
- Fixed issue that did not bind new viewmodel after editing job

## 0.1.22 (2021-09-07)
##### RuriLib
- Added TakeMaxInt, TakeMinInt, TakeMaxFloat and TakeMinFloat blocks
- Better exception when disconnecting from a websocket (by its5Q)
- Added multiline option for regex in parse block (by its5Q)
- Fixed LoliCode parser when encountering negative numbers
- Fixed issues with the skip
- Fixed event propagation in the job
- Other minor fixes and improvements

##### OpenBullet (Core)
- Big refactor of the codebase
- Improvements to counter some database errors

##### OpenBullet (Web)
- Added partial vietnamese localization by PITVN

##### OpenBullet (Native)
- Introduced the new Native UI

## 0.1.21 (2021-07-25)
- Added support for configurable bot limit
- Fixed FtpConnect block not throwing exception on connection failed
- Fixed important issues with email blocks
- Order jobs by id in manager
- Job options should be saved correctly now after being changed in the job viewer
- Fixed yet another issue with ApplicationDbContext
- Fixed issues where some entities wouldn't properly update after being written to the db
- Progress % instead of just progress trigger
- Changed execution info to STOPPED when a bot is stopping (in detailed view)
- Added timeoutMilliseconds to email login blocks
- Added ImapGetLog and Pop3GetLog blocks (for raw protocol logs)
- Fixed data.ERROR not being cleared on retry
- Improved dispose of puppeteer
- Fixed WordlistType not updating after wordlist edit
- Better exception when puppeteer element not found
- Added StringToBytes and BytesToString blocks

## 0.1.20 (2021-07-10)
- Quality of life improvements
- Better syntax highlighting for LoliCode
- Autocompletions for LoliCode snippets and blocks
- Autocompletion for variable names in blocks
- Added support for .ico files as config icon
- Fixed issue where the job viewer would take long to open in case of many hits
- Added SshAuthenticateWithPK block
- Added encoding parameter to file blocks
- Added OverrideHostHeader checkbox to CustomTwoCaptcha provider
- Added POP3 blocks
- Added FTP blocks
- Added timeoutMilliseconds to some TCP blocks

## 0.1.19 (2021-07-02)
- Fixed some issues in IMAP block
- Added SMTP blocks
- Fixed LockRecursionException in file blocks
- Fixed issues with ApplicationDbContext when storing hits
- Fixed issue with solvecaptcha.com

## 0.1.18 (2021-06-23)
- Fixed high CPU usage with HTTP proxies
- Improved exception message in safe mode (now includes the exception type)
- Fixed errors in DataPoolFactoryService when files are missing (now falls back to infinite data pool)
- Added custom formats to Zip block
- Added blocks for SSH
- Added blocks for IMAP

This update is fixing a game breaking bug introduced in 0.1.17 so I had to push it ASAP.

## 0.1.17 (2021-06-21)
- Fixed screenshot blocks in puppeteer
- Fixed some memory leaks
- Improved output of LoliCodeParserException
- Added ability to block URLs on puppeteer (in Config Settings)
- Added XOR and XORStrings blocks
- Added ability to move proxies between groups
- Fixed some issues with Cloudflare bot detection when getting proxies from remote (added User-Agent header to requests)
- Fixed the maximum redirects setting in Http Request block
- Added safe mode to auto blocks (see below)
- Added SvgToPng block (to solve svg captchas using captcha services)

**Safe mode** will catch exceptions and store their message in the string variable `data.ERROR` so you don't have to manually deal with try/catch anymore, and you can easily perform a keycheck on the message.

## 0.1.16 (2021-06-10)
- Implemented async locks. You can find a sample use case [here](https://discourse.openbullet.dev/t/global-session-token-that-refreshes/2854)
- Fixed game breaking bug in auto-blocks that use dictionaries
- Removed error on invalid cookies sent from the server

This is a very small update but it's fixing a game breaking bug introduced in 0.1.15 so I had to push it ASAP.

## 0.1.15 (2021-06-03)
- Fixed default value detection for LoliCode settings (ListOfStrings, DictionaryOfStrings, ByteArray)
- Added missing integer comparison when using INTKEYs
- Fixed issue with proxies in captcha blocks
- Improved the "add block" menu
- Added support for resources. You can find a small guide [here](https://discourse.openbullet.dev/t/using-global-variables-to-take-lines-from-a-file-in-order/48)
- Improved ZipLists block
- Added "view as HTML" button to single line sources in the bot log
- Added PuppeteerSelectByIndex and PuppeteerSelectByText blocks
- Fixed issue in DataPoolSelector
- Added "Delete Filtered" button to proxy manager
- Automatically cast to int in REPEAT statement

## 0.1.14 (2021-05-11)
- Fixed visual issue in request log
- Fixed issue with file lock
- Fixed issue with ApplicationDbContext (when multiple jobs are storing hits at the same time)
- Fixed issue with hits + fails not matching the amount of tested data
- Fixed issue where progress was stuck at 99.99% in jobs
- Added XPath mode to parse block
- Added XPath support to puppeteer blocks
- Improved the hits section

---
About the hits section improvements: users can now download hits as a .txt file, execute actions on all filtered hits (and not only selected) and choose custom export formats from `Environment.ini`

## 0.1.13 (2021-05-01)
- Save grid query status per user instead of globally
- Removed scientific notation when displaying floats
- Added ListToDictionary block
- Fixed small bug in Regex Helper
- Allow `Connection: Upgrade` header to upgrade from http(s) to ws(s)
- Fixed vulnerability where guests could access all proxies
- Added `?h` in RandomString block
- Added RemoveAllFromList block
- Fixed IsSubPathOf for relative paths and unix paths
- Fixed admin not being logged in automatically during setup
- Fixed serious issues with file operations
- Added PuppeteerWaitForNavigation block
- Fixed the width of modals on mobile
- Fixed output variable types in Script block
- Fixed errors when getting HTTP responses from some servers (thanks Rydj)
- **BREAKING CHANGE:** Fixed AES blocks to work with byte arrays instead of strings

## 0.1.12 (2021-04-14)
- Changed websocket library to a better one
- Fixed AddToList and RemoveFromList blocks with negative index
- Fixed a memory leak
- Small improvement in the task-based parallelizer code
- Fixed elapsed time not stopping after a job finished
- Added option to automatically dismiss dialogs in puppeteer
- Fixed "operation canceled" error for some HTTP requests (by Rydj)

## 0.1.11 (2021-04-09)
- Fixed FolderDelete block not deleting folders which are not empty
- Added the Custom Webhook hit output. More info [here](https://discourse.openbullet.dev/t/how-to-use-the-custom-webhook-output/1965)
- Added ability to send only hits to webhooks (and not custom / to check)
- Added ability to export proxies to a file
- PuppeteerNavigateTo now also fills the data.ADDRESS field
- Added ability to provide multiple proxy files when importing proxies
- Fixed some UI issue in Job Manager
- Brightened the color of the debugger log
- Added PuppeteerWaitForResponse block
- Added ability to skip reading the response content in HttpRequest block
- Fixed the response's content-related headers not being added to data.HEADERS
- Added extended sorting to configs grid
- Fixed SortList block in numeric mode
- Added SET VAR and SET CAP LoliCode statements. More info in the docs inside the program.

## 0.1.10 (2021-03-28)
- Fixed more bugs with ApplicationDbContext
- Fixed French translation
- Added support for cookie-based culture in appsettings.json
- Fixed issue with RandomUA provider (opposite behaviour in debugger)
- Fixed indentation problems in script block (critical for IronPython)
- Fixed problem with no input variables in IronPython script
- Fixed elapsed time not stopping on pause
- Fill data.SOURCE and data.RAWSOURCE on PuppeteerNavigateTo block
- Added FolderDelete block
- Accept underscored variables in NodeJS script
- Configurable polling interval for captcha solvers
- Added LOCK statement in LoliCode

## 0.1.9 (2021-03-26)
- Display which guest owns wordlists and proxy groups
- Fixed themes being deleted on update (redownload the updater for this to work)
- Abort while pausing
- Fixed issue with Random User Agent provider in the Debugger
- Added support for Arabic Language (translation still pending)
- Added ability to only load document and script with puppeteer (e.g. do not download images) for speed
- Configurable resources in appsettings.json
- Fixed reading response source when server doesn't send Content-Length and Transfer-Encoding

## 0.1.8 (2021-03-20)
- Added Abort Job action
- Fixed issues with the skip
- Added useUtc parameter to CurrentUnixTime block
- Fixes for the InvalidOperationException on ApplicationDbContext
- Fixed NRE when getting the user's IP and OB2 is behind a reverse proxy
- Fixed NRE in BotLoggerViewer
- Fixed bug when changing the bots after a job has been stopped

## 0.1.7 (2021-03-19)
- Fixed issues when using IP addresses as hostnames in Http Request block
- Fixed a memory leak
- Optimized CPM calculation
- Added GetHWID block
- Added PuppeteerExists block
- Added info about the owner of a job in the JobManager admin view
- Fixed changing the bots while a job is paused
- Fixed issues in the parallelizer
- Fixed remote configs not being reassigned to jobs upon OB2 restart

## 0.1.6 (2021-03-16)
- Persian translation by LilToba
- Added support for custom codepage encodings like windows-1251 in Http Request block
- Added button to clone a config
- Added support for async in Script block with NodeJS interpreter
- Added ability to change page size in ConfigSelector and WordlistSelector
- Fixed NRE in ProxyCheckOptions
- Added keySize parameter to AESEncrypt and AESDecrypt
- Fixed plugins not being extracted correctly sometimes
- Fixed updater not creating new directories
- Added support for uploading multiple files with puppeteer (disruptive change, removed previous block)
- Added GetFilesInFolder block
- Added support for big combination datapools
- Fixed editor in Script block not updating correctly

## 0.1.5 (2021-03-14)
- Fixed bug that was breaking Raw, BasicAuth and Multipart requests
- Fixed Translate block
- Allow the use of --urls parameter to change the port

## 0.1.4 (2021-03-14)
- Fixed AESEncrypt and AESDecrypt
- Performance improvements when getting proxies and building HTTP responses
- Fixed bug for 204 No Content HTTP responses
- Added Verbose Mode toggle in RL Settings
- Added PuppeteerFileUpload block
- Added UrlEncodeContent setting to HTTP Request block
- Fixed exception when Content-Length is not set by the server

## 0.1.3 (2021-03-13)
- Reverted game-breaking change which was affecting interpolated strings
- Performance improvements for HTTP by Rydj
- Added support for persian language, the actual translation will come soon
- Tried to fix config submenu disappearing again
- Accept arbitrary expression in REPEAT statement
- Removed blockSize parameter from AESEncrypt and AESDecrypt (useless)

## 0.1.2 (2021-03-12)
- Fixed issue with proxy check targets
- Fixed exception when referencing a deleted or remote config
- Added escaping for < and > in interpolated mode (use << and >>)
- Added support for SOCKS4/5 with auth in puppeteer
- Added MergeByteArrays block
- Added PuppeteerExecuteJs block
- Print DOM to debug log in PuppeteerGetDOM block
- Fixed browser not staying open after debugger ends
- Add error prompt when copying to clipboard fails
- Other fixes and improvements

## 0.1.1 (2021-03-10)
- Fixed PT translation (partially)
- Fixed errors while getting network statistics on some machines
- Fixed errors on malformed proxies from file or remote API
- Added Stop and Abort buttons when a job is paused
- Fixed config submenu randomly disappearing
- Fixed synchronization exceptions when using blocks that work with files
- Show the language flag only if logged in as admin

