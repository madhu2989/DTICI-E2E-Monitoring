import { VanStateTransition } from "./van-statetransition";
import { VanChecks } from "./van-checks";

export abstract class NodeBase {
    id: number;
    name: string;
    state: VanStateTransition;
    description: string;
    elementId: string;
    isSLA:boolean=false;
    isDashboardAvailable:boolean = false;

    checks: VanChecks[];
    abstract getChildNodes(): NodeBase[];
    abstract getChildNodesTitle(): string;
    abstract getNodeTitle(): string;
    abstract getGrandChildNodesTitle(): string;

    public getUpdatePayload(childElementIdsToAdd: string[] = [], childNodeElementIdsToRemove: string[] = [], checkIdsToAdd: string[] = [], checkIdsToRemove: string[] = []): any {
        return {
            name: this.name,
            description: this.description,
            elementId: this.elementId,
            checks: this.getCheckElementIds(checkIdsToAdd, checkIdsToRemove)
        };
    }

    protected getChildElementIds(childElementIdsToAdd: string[] = [], childElementIdsToRemove: string[] = []): string[] {
        if (this.getChildNodes()) {
            const elementIds: string[] = this.getChildNodes().map(childNode => childNode.elementId)
                .filter(elemendId => !childElementIdsToRemove.includes(elemendId));

            childElementIdsToAdd.forEach(elementId => {
                if (!elementIds.includes(elementId)) {
                    elementIds.push(elementId);
                }
            });

            return elementIds;
        }

        return [];
    }

    private getCheckElementIds(checkIdsToAdd: string[] = [], checkIdsToRemove: string[] = []): string[] {
        if (this.checks) {
            const checkIds: string[] = this.checks.map(check => check.elementId)
                .filter(checkId => !checkIdsToRemove.includes(checkId));

            checkIdsToAdd.forEach(checkId => {
                if (!checkIds.includes(checkId)) {
                    checkIds.push(checkId);
                }
            });

            return checkIds;
        }

        return [];
    }
}