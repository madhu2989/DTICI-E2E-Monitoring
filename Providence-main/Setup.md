How-To: Setup Providence Monitoring

This document provides step-by-step instructions on how to set up needed resources and the build and release pipelines with the goal of making the Providence monitoring tool available within an Azure subscription.

## 1. Create User Groups for the Providence Service
The first thing that needs to be created is the user groups for the Providence service. Here exactly 2 groups are needed: One for **"Contributors"** and one for **"Administrators"**. 
The groups can be created under the following link: https://dwgm.e.corpintra.net/DWGM_DE/page.axd?RuntimeFormID=4b48b595-0ae1-45e7-b070-06221839710b&ContextInstanceID=e4335dd1-941e-462b-bf68-cc018864cb45.
After the groups have been created, you can assign users to them who will later have access to the Providence service.

## 2. Create Azure AD Application
In order to grant access to the Providence service to the user groups created in the previous step, an Azure Active Directory (AAD) application must be created and configured.
The creation of such an application is done via the so called **"WAMS"** tool. Link: https://wams.e.corpintra.net/wkam/index.htm
Configuring this application is also done via WAMS. For this, a web ticket is required for the following changes:	
1. Change the type of the application to type 3 (user signin allowed)	
2. Add the needed API permissions
   1. Azure Active Dircetory Graph -> User.Read | Delegated | Sign in and read user profile | -
   2. Microsoft Graph -> GroupMember.Read.All   | Delegated | Read group memberships        | yes
3. Change the enterprise application manifest
   1. Set Reply URLs:
      1. https://**CustomPrefix**-monitoringservice-**EnvironmentName**.azurewebsites.net/*
      2. https://**CustomPrefix**-monitoringservice-**EnvironmentName**.azurewebsites.net 
   2. Add application roles
      1. Monitoring_contributor
      2. Monitoring_admin
   3. Set SignIn URL
     1. https://**CustomPrefix**-monitoringservice-**EnvironmentName**.azurewebsites.net
	
NOTE: You need to remember the **CustomPrefix**, the **EnvironmentName** and the **Application Id** of the created AAD application because you will need those properties again later.

## 3. Connect the Providence Repository to Azure DevOps
Now that all necessary configurations have been made in the AAD, the preparation for the release of the Providence service can be tackled. First of all, a connection between the Azure DevOps project and Daimler GitHub is required.
This requires a service connection within your own Azure DevOps project. The following section explains how such a service connection can be created.
1. On the bottom left of you project page click on **“Project settings”**
2. Search for **“Service connections”** on the left task bar and click on it
3. On this page click on **“New service connection”** on the top right and create a new service connection with the type **“GitHub Enterprise Server”**.
Set following values:
   1. Server URL: https://git.daimler.com/
   2. Personal access token: look at https://docs.github.com/en/github/authenticating-to-github/creating-a-personal-access-token to see how to create a GitHub access token 
      **Remember: The user performing this step needs contributor rights for the Providence repository**
   3. Service connection name: Choose a name for your service connection
   4. Description: If you want you can add a custom description here

After filling in all the necessary fields, click on **"Verify and save"** to complete the service connection creation. 

## 4. Create the Providence Build pipeline
After connecting to the Providence repository, the **"appservice-build.yml"** file stored in the repository can now be used to create a build pipeline in the Azure DevOps project. The following is a rough description of how to create such a build pipeline.
1. Go to your DevOps project and click on **“Pipelines”** in the toolbar on the left
2. On the next page you click on the button that says **“New pipeline”**
3. Choose **“GitHub Enterprise Server (YAML)”** on the **“Connection”** tab
4. On the next tab you select the previously created service connection to the Providence GitHub repository and then select the **“i3-community/Providence”** repository.
5. On the **“Configure”** tab you then have to search for **“Existing Azure Pipelines YAML file”**
Selecting this option will open a dialog where you have to select the yaml file for the new pipeline. Select following properties and click on **“Continue”**:
   1. Branch: main
   2. Path: “/Daimler.Providence.Infrastructure/Daimler.Providence.Infrastructure/AzureDevOpsYaml/Build/appservice-build.yml”
6. On the final **“Review”** tab you can check that you have selected the right pipeline and then click on save.

After saving the last step you have successfully integrated the Providence build pipeline into your Azure DevOps project. 

## 5. Create the Providence Release pipelines 
Now that the build pipeline has been created, the next step is to create the release pipelines for infrastructure and the app service.
For this, the yaml templates in the Providence repository must be used. Unlike the build pipeline, however, a separate Azure DevOps repository is required here in order to be able to make project-specific configurations and to use the provided Providence templates.

Examples of the two required files **"appservice-release.yml"** and **"infrastructure-release.yml"** can be found in the Providence repository under **"Daimler.Providence.Infrastructure\Daimler.Providence.Infrastructure\AzureDevOpsYaml\Sample"**.
Note: You have to adjust the sample templates by performing following steps:
1. Add the name of your service connection which shall be used to deploy to Azure
   Replace the **'Your_Service_Connection_Name'** token in the sample file
2. Create a variable group within your Azure DevOps project which contains the project specific configurations for the Providence service
   Replace the **'Your_Variable_Group_Name'** token in the sample file
This variable group must contain following properties:
   1. AILocation: Location where the Application Insights instance shall be created
   2. AppServicePlanSku: The scaling for the Providence web service
   3. AutoRefreshJobIntervalInSeconds: The interval in seconds after which the AutoRefresh job should be executed
   4. AutoResetJobIntervalInSeconds: The interval in seconds after which the AutoReset job should be executed
   5. AzureSubscriptionId: The unique Id of the Subscription where the Providence service shall be deployed to
   6. CutOffTimeRangeInWeeks: Time in weeks after which data shall be cleaned up from the database
   7. DatabaseEdition: Edition of the Providence database
   8. DatabaseSku: Scaling of the Providence database
   9. EmailNotificationJobIntervalInSeconds: The interval in seconds after which the email notification job should be executed
   10. EnableEventHubReader: Flag that determines whether messages shall be read from the EventHub or not
   11. EnterpriseApplication-AppId: The unique id of your application you created in Step 2 of this documentation
   12. EnvironmentName: Your custom environment name used to create all the infrastructure resource (you have to use the same one as in step 2 in this documentation) 
   13. Location: Location where all resources besides Application Insights shall be created
   14. LogElapsedTime: Flag to activate logging of long running processes
   15. LogSqlQuery: Flag to activate database level logs
   16. MaxElapsedTimeInMinutes: Time in minutes that determines a “long running process”
   17. CustomPrefix: Your custom prefix used to create all the infrastructure resource (you have to use the same one as in step 2 in this documentation)
   18. RunAutoRefresh: Flag that determines whether the AutoRefresh job shall be activated or not
   19. RunAutoReset: Flag that determines whether the AutoReset job shall be activated or not
   20. RunDeleteExpiredChangeLogs: Flag that determines whether old changelogs shall be deleted after a specific time or not
   21. RunDeleteExpiredDeployments: Flag that determines whether old deployments shall be deleted after a specific time or not
   22. RunDeleteExpiredInternalJobs: Flag that determines whether old internal job shall be deleted after a specific time or not
   23. RunDeleteExpiredStatetransitions Flag that determines whether old statetransitions shall be deleted after a specific time or not
   24. RunDeleteUnassignedComponents: Flag that determines whether unassigned components shall be deleted after a specific time or not
   25. ServicePrincipalObjectId: The unique object Id of you service Principal which is used to deploy the Providence service to your subscription
   26. StateIncreaseJobIntervalInSeconds:
   27. TenantId: The unique Id of the Tenant where the Providence service shall be deployed to
   28. UpdateDeploymentsJobIntervalInSeconds: The interval in seconds after which the UpdateDeplomynets job should be executed
3. Set your Azure DevOps project id and the id of the previously created build pipeline
   Replace the **'Your_Devops_Project_Id'** and the **'Your_Build_Pipeline_Id'** token in the sample file

After the two yaml files have been created in the project-specific repository, they must be added as pipelines in AzureDevOps - just like the build pipeline before.
Afterwards you should have two new pipelines:
1. Providence.Infrastructure.Release
2. Providence.Service.Release

## 6. Deploy the Providence Infrastructure 
To create the necessary infrastructure components now within a resource group, only the previously created **"Providence.Infrastructure.Release"** pipeline has to be executed. This way, components such as the database, the event hub or the web service are created.

## 7. Build and deploy the Providence App Service

7.1 Build the Providence Service
Now that the infrastructure has been successfully created, the Providence service can be deployed. For this, the build pipeline must first be triggered.
1. Go to your build pipeline and click on **„Run pipeline”**
2. In the dialog select the branch you want to build and click on **“Run”**
3. On the next page, the **"buildId"** must now be copied from the URL, as this is required for the next step.
https://dev.azure.com/vanstelematics/ScaledPilotPlatform/_build/results?buildId=342778&view=results

7.2 Deploy the Providence Service
The last step to make the Providence service available now is to trigger the second release pipeline.
1. Go to your release pipeline and click on **„Run pipeline”**
2. In the dialog select the branch of you yaml file and add the previously copied **"buildId"** as pipeline variable then click on **“Run”**

