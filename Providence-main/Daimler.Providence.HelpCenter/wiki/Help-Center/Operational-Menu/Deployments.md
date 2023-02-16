## Deployments

The Deployment notifies the UI when in the UI timeline a deployment window should be shown.
The sence for this behaviour is that the Operations-team can consider the deployments if a state of a element is in state "ERROR".
Screenshot of the timeline showing the deployment slot.
The deployment window is marked in blue color, means in that time a deployment of a service is running.

![Deployment_1.png](.attachments/Deployment_1-9ca2ac1e-1b89-4361-8712-5c758d64ae08.png)

**Deployment Details:**
With double click on the deployment line, following deployments details are displayed

![Deployment_View.png](.attachments/Deployment_View-80e47733-abb0-4d90-9b81-ea80cff42844.png)
![Deployments_details.png](.attachments/Deployments_details-7663fb3b-115c-4358-91a5-c2a309c87570.png)

- Environment Name
- Subscription ID
- Start Date & Time
- End Date & Time
-  Detail of individual Deployment:
   - Deployment ID
   - Description
   - Start Date & Time
   - End Date & Time
   - ElementID´s


**Deployment Overview:**

![Deployment_Overview.png](.attachments/Deployment_Overview-1641907f-0f02-4bf6-9ad2-2125a731505e.png)

**Add new Deployment:**

Click on "Add" icon to add an new deployment.

![DeploymentAdd.png](.attachments/DeploymentAdd-c304ee44-9281-4f31-b880-68bacbb5d533.png)

It is possible to either create one single deployment or to create a set of reccuring deployments. If "Once" is chosen the following fields must be filled:
- Description
- Short Description
- Select the environment that is affected
- Select or Set the affected ElementId(s)
- Set the option "Once" deployment
- Set the start date or choose a date
- Set the start time
- Set the end date or choose a date
- Set the end time
- Set Reason for close

![Deployment_2.png](.attachments/Deployment_2-693876c0-fc00-4feb-ac17-9b339b00afb6.png)

**Add Deployment (recurring):**
If "Serie" is chosen, to create a whole deployment set, tho following fields must be filled:
- Description
- Short Description
- Select the environment that is affected
- Select or Set the affected ElementId(s)
- Set the option "Serie" deployment
- Set the start time
- Set the end time
- Set Reason for close
- Set the start date
- Choose the pattern of rescheduling ( Daily, Weekly, Monthly)
  (e.g. "Daily" and "every 2 days" for a deployment that will reoccur every two days)
  - Daily     --> Choose day interval
  - Weekly    --> Choose weeks interval (Mo - Su)
  - Monthly   --> Choose monthly interval on the ... day of month (Mo - Su)
- Choose the duration date 
- Choose "No Enddate"(default + 1 year) or the "End date"
  Optional: choose an end date (otherwise the deployment will reoccur for one year)

![Deployment_Serie.png](.attachments/Deployment_Serie-67ba20f7-5da3-4abf-ae12-a0cc93174353.png)

**Modify Deployment:**
Double-click on the deployment **or** select the deployment and click on the "Edit" icon

![ModiDeployment.png](.attachments/ModiDeployment-d9b63810-e937-4e42-a2c2-89df80da6faa.png)

**Deployment "Once":**

![Deployment_Once.png](.attachments/Deployment_Once-8920fba7-af1d-4219-9b28-3d66b0aa712b.png)

**Deployment "Serie":**

![Deployment_Serie.png](.attachments/Deployment_Serie-aad01cac-1e95-4cc8-9e2a-351dbeb6975f.png)

**Delete Deployment (Once/Serie):**
It is not possible to delete a serial deployment as a whole, because these are created as individual ones per day of the week. Deletion must be done manually and individually.

Select the deployment and click on the "Delete" Icon

![Deployment_Overview2.png](.attachments/Deployment_Overview2-6c284536-a46c-46e8-98d6-192453a8cc3c.png)

![Delete.png](.attachments/Delete-50bee6cb-d9d5-417f-9469-b5894fbe10f4.png)
***