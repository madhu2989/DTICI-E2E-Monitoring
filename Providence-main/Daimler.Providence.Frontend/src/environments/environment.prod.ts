export const environment = {
  production: true,

  aadTenant: '#{TenantId}#',
  aadClientId: '#{EnterpriseApplication-AppId}#',

  configEndpoint: '/api/config',
  dataEndpoint: '/api',
  signalREndpoint: '',

  versionBuildNumber: '#{Build.BuildNumber}#',

  appInsights: { instrumentationKey: '#{ApplicationInsightsInstrumentationKey}#' },

  houseKeepingInterval: 1000 * 60,
  historyDuration: 72 * 60 * 60 * 1000
};