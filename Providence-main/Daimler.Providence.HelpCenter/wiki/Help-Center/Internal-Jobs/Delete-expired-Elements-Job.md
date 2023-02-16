## Delete expired Elements Job

This job is used to delete expired elements stored in the Providence database after a specific time period (minimum 1 week) in order to reduce the data sice within the state transition table and to increase the overall perfomance of the system.
The settings is environment specific and is configured within the app settings of the Providence service application.

Elements that are deleted by this job are:
- Deployments
- StateTransitions
- ChangeLog Entries
- Unassigned Components

