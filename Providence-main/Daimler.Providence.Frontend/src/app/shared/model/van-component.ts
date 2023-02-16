import { NodeBase } from "./node-base";

export class VanComponent extends NodeBase {
    private static readonly childNodesTitle = null;
    private static readonly nodeTitle = 'Component';
    private static readonly grandChildNodesTitle = null;
    type: string;
    componentType: string;

    public constructor(init?: Partial<VanComponent>) {
        super();
        Object.assign(this, init);
    }

    getChildNodes(): NodeBase[] {
        return null;
    }

    getChildNodesTitle(): string {
        return VanComponent.childNodesTitle;
    }

    getNodeTitle(): string {
        return VanComponent.nodeTitle;
    }
    getGrandChildNodesTitle(): string {
        return VanComponent.grandChildNodesTitle;
    }

    public getUpdatePayload(childElementIdsToAdd: string[] = [], childNodeElementIdsToRemove: string[] = [], checkIdsToAdd: string[] = [], checkIdsToRemove: string[] = []): any {
        const payload = super.getUpdatePayload(childElementIdsToAdd, childNodeElementIdsToRemove, checkIdsToAdd, checkIdsToRemove);
        payload.componentType = this.componentType;

        return payload;
    }

}