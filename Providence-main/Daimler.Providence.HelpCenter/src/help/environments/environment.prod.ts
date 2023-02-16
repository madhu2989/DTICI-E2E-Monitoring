export const environment = {
  production: true,
  templatePath: 'assets/help',

  aadTenant: '#{TenantId}#',
  aadClientId: '#{EnterpriseApplication-AppId}#',

  configEndpoint: '/api/config',
  dataEndpoint: '/api',
  signalREndpoint: '',


  appInsights: { instrumentationKey: '#{ApplicationInsightsInstrumentationKey}#' },

  houseKeepingInterval: 1000 * 60,
  historyDuration: 72 * 60 * 60 * 1000
};
