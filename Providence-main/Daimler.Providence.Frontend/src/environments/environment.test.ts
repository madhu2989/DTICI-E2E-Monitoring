// The file contents for the current environment will overwrite these during build.
// The build system defaults to the dev environment which uses `environment.ts`, but if you do
// `ng build --env=prod` then `environment.prod.ts` will be used instead.
// The list of which env maps to which file can be found in `.angular-cli.json`.

export const environment = {
  production: false,

  aadTenant: 'cfb54644-9b51-4043-b240-157ac93ddfe6',
  aadClientId: 'bee4f618-3441-4e60-8bcc-cf1faf5c1bda',

  configEndpoint: '/api/config',
  dataEndpoint: 'https://spp-monitoringservice-test.azurewebsites.net/api',
  signalREndpoint: 'https://spp-monitoringservice-test.azurewebsites.net',

  versionReleaseName: 'Release-test-1',
  versionBuildNumber: '10.0.1',

  appInsights: { instrumentationKey: '5b543f4c-b138-4d31-bcfe-d57e234733b9' },
  houseKeepingInterval: 1000 * 15,
  historyDuration: 72 * 60 * 60 * 1000
};