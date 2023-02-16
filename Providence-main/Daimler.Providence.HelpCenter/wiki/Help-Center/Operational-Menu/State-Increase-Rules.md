# State Increase Rules
The State Increase Rule overview of the Providence Service supports operations for creating, updating and deleting specific rules. 

State Increase Rules can be used to increase the state of specific alerts after a specified period of time.

The state increase job checks in a specific period of time (configurable in app settings) if a check is in the state "WARNING" for a specific duration. 
If so the job increases the state to "ERROR" to give this component more attention within the dashboard.

**State Increase Rule overview:**

![StateOverview1.png](.attachments/StateOverview1-92173645-a35f-4185-86f8-03a4f3c11fc0.png)

**Add State Increase Rule:**

Click on "Add" icon to add an new rule.

![AddStateRule1.png](.attachments/AddStateRule1-a128ea00-dddc-4e87-9c41-cae5b04217c9.png)

The following fields must be filled:
- Title
- Description
- Select the environment
- Select the Component Id
- Select the Check Id
- Set the Alert Name
- Set "Activate this Rule" (yes/no)
- Set Trigger time (from 1 - 720 minutes)

![AddStateRule.png](.attachments/AddStateRule-e8363f20-3b43-47a8-9e0d-f0bc063faa0e.png)

**Modify State Increase Rule:**
- Double-click on the rule **or** select the rule and click on the "Edit" icon

![EditStateRule.png](.attachments/EditStateRule-aeede1d4-f340-4336-9586-5550a3dc6c27.png) 

- Make changes and save to complete.

![EditStateRule1.png](.attachments/EditStateRule1-4b47d6ed-8b81-47a6-b514-aeae51a40eaa.png)

**Delete State Increase Rule:**

Select the rule and click on the "Delete" icon

![DeleteStateRule.png](.attachments/DeleteStateRule-57713095-e273-465b-8b43-9ddb8fa110c3.png)

![DeleteStateRule1.png](.attachments/DeleteStateRule1-747e3c60-5155-44ba-9638-6b7e4adbc8ea.png)
