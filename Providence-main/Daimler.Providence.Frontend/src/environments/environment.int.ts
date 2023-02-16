// The file contents for the current environment will overwrite these during build.
// The build system defaults to the dev environment which uses `environment.ts`, but if you do
// `ng build --env=prod` then `environment.prod.ts` will be used instead.
// The list of which env maps to which file can be found in `.angular-cli.json`.

export const environment = {
  production: false,

  aadTenant: '67369289-5193-4251-849f-cb28035dd3c7',
  aadClientId: '07cc2e6a-6cc2-4e86-aeb6-272e23b9b64d',

  configEndpoint: '/api/config',
  dataEndpoint: 'https://spp-monitoringservice-int.azurewebsites.net/api',
  signalREndpoint: 'https://spp-monitoringservice-int.azurewebsites.net',

  versionBuildNumber: '10.0.1',

  appInsights: { instrumentationKey: '15f3dbd9-adc3-4c5d-81ba-ba3ca6181796' },
  houseKeepingInterval: 1000 * 60,
  historyDuration: 72 * 60 * 60 * 1000
};