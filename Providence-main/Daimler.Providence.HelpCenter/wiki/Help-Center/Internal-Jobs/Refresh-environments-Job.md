## Refresh environments Job

This job is used to clean up the state managers hold by the Providence backend. If necessary the job disposes and rebuilds the state managers within the backend. This is for example needed if changes were made directly on the database and need to be transfered into the state manager of a deployed instance.
The settings is environment specific and is configured within the app settings of the Providence service application.


