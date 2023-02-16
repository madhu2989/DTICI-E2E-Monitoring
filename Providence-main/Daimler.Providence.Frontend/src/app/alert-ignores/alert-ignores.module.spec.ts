import { AlertIgnoresModule } from './alert-ignores.module';

describe('AlertIgnoresModule', () => {
  let alertIgnoresModule: AlertIgnoresModule;

  beforeEach(() => {
    alertIgnoresModule = new AlertIgnoresModule();
  });

  it('should create an instance', () => {
    expect(alertIgnoresModule).toBeTruthy();
  });
});
