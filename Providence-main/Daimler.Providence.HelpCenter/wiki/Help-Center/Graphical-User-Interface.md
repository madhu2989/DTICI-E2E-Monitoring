# Graphical User Interface

## Dashboard 

The dashboard includes information about the current status of the environment. 
- Timeline of the environment
- Overview of all services monitored: : 
  - Service name 
  - Status icon (OK, Warning, Error) 
  - Service timeline shows the status in color
  - Quantity of actions 
  - Quantity of components

![Dashboard.png](.attachments/Dashboard-1af6d265-37f0-4260-9e54-16875d8ae331.png)
***
## Operational Menu

The Operational menu includes the following areas:
- Dashboard
- SLA Report
  - SLA Thresholds
- Deployments
- Alert ignores
- Checks
- Notification Rules
- State Increase Rules
- Changelog History

![Menu.png](.attachments/Menu-7340b5f6-c219-4473-82a5-5aacabb69da0.png)


***
## Timeline
The timeline shows the status of the environemt. The status is highlighted in color.
The timeline shows also the deployment slot. The deployment window is marked in blue color, means in that time a deployment of a service is running.

![Timeline.png](.attachments/Timeline-7f749daa-3f4d-47a2-825f-7d3bb39fa724.png)


***
## Toolbar
In the upper right corner you can see the Toolbar

![Toolbar.png](.attachments/Toolbar-35a1d810-2a62-4e2a-8cf6-18c274c67605.png)

The following icons/buttons are displayed:

- Receiving (real time data from monitoring backend)

- Refresh (manual refresh) 

- Help (Linkout to the Helpfunction)

- Setting (The settings reference to the dashboard and the timeline)

  ![Settings.png](.attachments/Settings-472fb5ae-a626-4b82-b81c-ad4700ba80d3.png)

  The following settings are available:
  - Show subcomponents in view
    This setting contains the quantity and graphical display of all actions and components for the respective service.
    ![SubcomView.png](.attachments/SubcomView-af21c46e-1b81-4c2a-9847-86fd938fbd71.png)

  - Ignore missing environment heartbeats
    This setting ignore missing heartbeats for all visible environments which means that no "No Data available" message will be shown for those environments.
   ![NoHaerdbeatView.png](.attachments/NoHaerdbeatView-597c0c72-be71-4e50-9552-f3434d05e453.png)
   ![HeartbeatsView.png](.attachments/HeartbeatsView-cb30a0b0-5189-41a8-bbcb-7760fb480519.png)

  - Ignore elements with state "OK"
    This setting shows only elements with warnings or error. All elements with the status "OK" are not listed in this overview.
    ![StateOKView.png](.attachments/StateOKView-d5b49b80-0bde-4ea7-a9bd-f8f3145c86ef.png)

  - Timerange setting
    The timerange setting shows the timeline and all elements in the defined time period.
    ![TimerangeView1.png](.attachments/TimerangeView1-d34a4395-924f-499a-b947-b03e29839b57.png)

- Login / Logout User (User Profile)
  Click on User Profile and "Logout" button if you want to logout.

  ![UserProfile.png](.attachments/UserProfile-29f74966-9d55-48f9-9c9e-f97b141c4215.png)
  ![Logout.png](.attachments/Logout-d02e6293-f92d-4497-9512-6c2ca2638be8.png)

***
## Search an Element 

The search function can be used to search for elements in the environment to step directly to this element.
![search.png](.attachments/search-b06f5484-8fc6-437f-901a-a03f20409e7a.png)

***
## Administration

In the lower right corner you can see the toogle 'Edit mode', it is only visible for the administrator. 
He can create, modify and delete environments, services, actions and components.

![Administration.png](.attachments/Administration-9f55bfc5-881c-47ce-b3ae-7a5247a2d168.png)

***