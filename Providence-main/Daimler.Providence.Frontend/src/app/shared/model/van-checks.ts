import { NodeBase } from "./node-base";

export class VanChecks extends NodeBase {
  private static readonly childNodesTitle = null;
  private static readonly nodeTitle = null;
  private static readonly grandChildNodesTitle = null;
  checkId: string;
  vstsLink: string;
  frequency: number;
  environmentSubscriptionId: string;

  public constructor(init?: Partial<VanChecks>) {
    super();
    Object.assign(this, init);
  }

  getChildNodes() {
    return null;
  }
  getChildNodesTitle(): string {
    return VanChecks.childNodesTitle;
  }
  getNodeTitle(): string {
    return VanChecks.nodeTitle;
  }
  getGrandChildNodesTitle(): string {
    return VanChecks.grandChildNodesTitle;
  }

}