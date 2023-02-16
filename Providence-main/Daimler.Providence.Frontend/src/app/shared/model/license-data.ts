export class LicenseData {
    package: string;
    version: string;
    license: string;
    

    public constructor(init?: Partial<LicenseData>) {
        Object.assign(this, init);
    }
}