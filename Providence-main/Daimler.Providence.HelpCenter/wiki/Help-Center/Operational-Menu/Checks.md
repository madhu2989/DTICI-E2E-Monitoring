# Checks
Checks are used as a mechanism to avoid unknown data to be processed by the Providence service backend. Only alerts with a valid and known "CheckId" property are processed by the service. If a valid "CheckId" was found the alert changes the state of all elements which are linked to the Check within the environment.Depending on the state defined in the alert the state then changes to "ERROR", "WARNING" or "OK".

Besides the functionality as a "Validation Gateway" for alerts checks are also used to configure other different features like for example the "Reset to Green" feature. You can read more about this feature within the "Internal Jobs" section.

![Screen_Check.jpg](.attachments/Screen_Check-cccf6ad8-87c8-46c8-8a1e-3bcfa4e9a4e0.jpg)

**Check Overview:**

![CheckOverview.png](.attachments/CheckOverview-11d3103b-a439-492a-a23a-86b301f3b22d.png)

**Add Check:**

Click on "Add" icon to add a service check.

![SelectCheck.png](.attachments/SelectCheck-ab3f1959-898d-4efc-a389-a1386c109d7b.png)

The following fields must be filled in:
- Check Id
- Select the environment that is affected
- Name
- Description
- VSTS Link
- Select "Specify a reset frequency"
- Set the frequency in second (10 - 1800 seconds)
- Save the settings

![AddCheck.png](.attachments/AddCheck-03217c28-cc43-43c5-b387-45bafdfdeb1a.png)

**Modify Check:**

Double-click on the service check **or** Select the service check and click on the "Edit" icon

![SelectCheck.png](.attachments/SelectCheck-ab3f1959-898d-4efc-a389-a1386c109d7b.png)
- Make changes and save to complete.

![EditCheck.png](.attachments/EditCheck-d60c936a-8c9a-48a4-8f51-cc62e390ca41.png)

**Delete Check:**

Select the service check and click on the "Delete" icon

![DeleteCheck.png](.attachments/DeleteCheck-894f330d-1242-4a5b-bc23-c95c8c3fd452.png)

![DeleteCheck2.png](.attachments/DeleteCheck2-c5f823b9-bb8c-424d-9cd5-22c7e15b5a4d.png)