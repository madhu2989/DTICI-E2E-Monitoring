import { NodeBase } from "./node-base";
import { VanComponent } from "./van-component";

export class VanAction extends NodeBase {
    private static readonly childNodesTitle = 'Components';
    private static readonly nodeTitle = 'Action';
    private static readonly grandChildNodesTitle = null;
    components: VanComponent[];

    public constructor(init?: Partial<VanAction>) {
        super();
        Object.assign(this, init);
    }


    getChildNodes(): NodeBase[] {
        return this.components;
    }
    getChildNodesTitle(): string {
        return VanAction.childNodesTitle;
    }
    getNodeTitle(): string {
        return VanAction.nodeTitle;
    }
    getGrandChildNodesTitle(): string {
        return VanAction.grandChildNodesTitle;
    }

    public getUpdatePayload(childElementIdsToAdd: string[] = [], childNodeElementIdsToRemove: string[] = [], checkIdsToAdd: string[] = [], checkIdsToRemove: string[] = []): any {
        const payload = super.getUpdatePayload(childElementIdsToAdd, childNodeElementIdsToRemove, checkIdsToAdd, checkIdsToRemove);
        payload.components = this.getChildElementIds(childElementIdsToAdd, childNodeElementIdsToRemove);

        return payload;
    }
}