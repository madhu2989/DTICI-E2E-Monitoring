
# **Providence Service Tree**

The Providence service tree is the basis for monitoring the environments.
The service tree is composed of elements which build up a tree with the environment as its root node.

The following diagram shows an example of a service tree:

![Screen_Environment_Tree.jpg](.attachments/Screen_Environment_Tree-b872140a-a6e5-44f6-84ab-d47868ee8e5b.jpg)

The root node is always the environment. An environment contains services, which are composed of actions.
Each action has components, which are the leaf nodes of the service tree.
************
## Alerts
Alerts are triggered in each environment. For events to be visible in the Providence dashboard, a check must be created in the Providence service.

![Screen_Alert.jpg](.attachments/Screen_Alert-20d841fa-18af-4e61-a262-4e7352faf13b.jpg)

************
## State
State in the service tree is propagated bottom-up from the child nodes to their parent node.
![Screen_State.jpg](.attachments/Screen_State-bfa65f67-b817-4a9d-90e2-aa901feab3c6.jpg)

****************************

