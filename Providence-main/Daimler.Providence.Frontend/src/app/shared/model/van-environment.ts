import { NodeBase } from "./node-base";
import { VanService } from "./van-service";

export class VanEnvironment extends NodeBase {
    private static readonly childNodesTitle = 'Services';
    private static readonly nodeTitle = 'Environment';
    private static readonly grandChildNodesTitle = 'Actions';
    subscriptionId: string;

    lastHeartBeat: string;
    logSystemState: string;
    isDemo: boolean;

    services: VanService[];


    public constructor(init?: Partial<VanEnvironment>) {
        super();
        Object.assign(this, init);
    }

    getChildNodes(): NodeBase[] {
        return this.services;

    }
    getChildNodesTitle(): string {
        return VanEnvironment.childNodesTitle;
    }
    getNodeTitle(): string {
        return VanEnvironment.nodeTitle;
    }
    getGrandChildNodesTitle(): string {
        return VanEnvironment.grandChildNodesTitle;
    }

    public getUpdatePayload(childElementIdsToAdd: string[] = [], childNodeElementIdsToRemove: string[] = [], checkIdsToAdd: string[] = [], checkIdsToRemove: string[] = []): any {
        const payload = super.getUpdatePayload(childElementIdsToAdd, childNodeElementIdsToRemove, checkIdsToAdd, checkIdsToRemove);
        payload.subscriptionId = this.elementId;
        payload.isDemo = this.isDemo;
        payload.services = this.getChildElementIds(childElementIdsToAdd, childNodeElementIdsToRemove);

        return payload;
    }
}