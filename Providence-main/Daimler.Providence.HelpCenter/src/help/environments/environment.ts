// The file contents for the current environment will overwrite these during build.
// The build system defaults to the dev environment which uses `environment.ts`, but if you do
// `ng build --env=prod` then `environment.prod.ts` will be used instead.
// The list of which env maps to which file can be found in `.angular-cli.json`.

export const environment = {
  production: false,
  templatePath: 'assets/help',

  aadTenant: 'e043fba4-ae72-4eac-843e-227477470d84',
  aadClientId: '7c9a2724-bd3f-4cea-961c-c40df9dedb71',

  configEndpoint: 'http://localhost:32760/api/config',
  dataEndpoint: 'http://localhost:32760/api',
  signalREndpoint: 'http://localhost:32760',

  appInsights: { instrumentationKey: '334c171d-5761-458c-a48c-96e1c681f71b' },

  houseKeepingInterval: 1000 * 60,
  historyDuration: 72 * 60 * 60 * 1000
};

