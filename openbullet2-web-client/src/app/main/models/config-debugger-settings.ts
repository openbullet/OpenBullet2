export interface ConfigDebuggerSettings {
    testData: string;
    wordlistType: string;
    useProxy: boolean;
    testProxy: string;
    proxyType: string;
    persistLog: boolean;
    stepByStep: boolean
}

export interface BotLoggerEntry {
    message: string;
    color: string;
    canViewAsHtml: boolean;
    timestamp: string
}
