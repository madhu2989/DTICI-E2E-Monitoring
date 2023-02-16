import { NodeBase } from "./node-base";
import { VanAction } from "./van-action";

export class VanService extends NodeBase {
    private static readonly childNodesTitle = 'Actions';
    private static readonly nodeTitle = 'Service';
    private static readonly grandChildNodesTitle = 'Components';
    actions: VanAction[];

    public constructor(init?: Partial<VanService>) {
        super();
        Object.assign(this, init);
    }


    getChildNodes(): NodeBase[] {
        return this.actions;
    }

    getChildNodesTitle(): string {
        return VanService.childNodesTitle;
    }
    getNodeTitle(): string {
        return VanService.nodeTitle;
    }
    getGrandChildNodesTitle(): string {
        return VanService.grandChildNodesTitle;
    }

    public getUpdatePayload(childElementIdsToAdd: string[] = [], childNodeElementIdsToRemove: string[] = [], checkIdsToAdd: string[] = [], checkIdsToRemove: string[] = []): any {
        const payload = super.getUpdatePayload(childElementIdsToAdd, childNodeElementIdsToRemove, checkIdsToAdd, checkIdsToRemove);
        payload.actions = this.getChildElementIds(childElementIdsToAdd, childNodeElementIdsToRemove);

        return payload;
    }
}

