# Deployment API

The Deployment API of the Providence Service supports operations for creating, updating and deleting deployment windows. 
Using the "repeatInformation" property of a Deployment you can configure recurring deployments. On this way you for example can create a deployment widnow for each friday at 10PM.

## API Documentation
<code>
{{HOST}} = https://spp-monitoringservice-X.azurewebsites.net</code> where X stands for the environment the request is made to.

### Create Deployment

This endpoint is accessible via: **POST: {{HOST}}/api/deployments**

Following payload is used to create a deployment.
**Notice:** 
- A deployment for an environment can only be created if there isn't already an ongoing deployment. 
- Also the property startDate is optional. If no value is provided or the provided value points to the future, the current date and time will be set as value.

```
{
    "environmentsubscriptionId": "",                        
    "description": "", 
    "shortDescription": "",
    "startDate": "",   // StartDate of the Deployment Window
    "endDate" : "",    // EndDateDate of the Deployment Window
    "closeReason": "",
    "elementIds": ["",""],
    "repeatInformation": 
    {
        "repeatType": 0,   // 0 = Daily, 1 = Weekly, 2 = Monthly
        "repeatInterval": 0, // Repeat every X days
        "repeatOnSameWeekDayCount": false,  // true if for example: Every 2nd Monday of a Month
        "repeatUntil": ""  // EndDateDate of the recurring Deployment Window
    }
}
```

The response of this endpoint call can be one of the following:
- **HttpStatusCode.Created** when the deployment was created successfully.
- **HttpStatusCode.BadRequest** when the payload/url contains invalid parameter.
- **HttpStatusCode.NotFound** when the created deployment was not found in the database.
- **HttpStatusCode.InternalServerError** when something went wrong during the creation process.

### Update Deployment

This endpoint is accessible via: **PUT: {{HOST}}/api/deployments/{environmentsubscriptionid}/{id}**

Following payload is used to update an existing deployment.

```
{
    "environmentsubscriptionId": "",                        
    "description": "", 
    "shortDescription": "",
    "startDate": "",   // StartDate of the Deployment Window
    "endDate" : "",    // EndDateDate of the Deployment Window
    "closeReason": "",
    "elementIds": ["",""],
    "repeatInformation": 
    {
        "repeatType": 0,   // 0 = Daily, 1 = Weekly, 2 = Monthly
        "repeatInterval": 0, // Repeat every X days
        "repeatOnSameWeekDayCount": false,  // true if for example: Every 2nd Monday of a Month
        "repeatUntil": ""  // EndDateDate of the recurring Deployment Window
    }
}
```
The response of this endpoint call can be one of the following:
- **HttpStatusCode.NoContent** when the deployment was updated successfully.
- **HttpStatusCode.BadRequest** when the payload/url contains invalid parameter.
- **HttpStatusCode.NotFound** when the deployment to update was not found in the database.
- **HttpStatusCode.InternalServerError** when something went wrong during the update process.

### Delete Deployment

This endpoint is accessible via: **DELETE: {{HOST}}/api/deployments/{environmentsubscriptionid}/{id}**

The response of this endpoint call can be one of the following:
- **HttpStatusCode.Accepted** when the deployment was deleted successfully.
- **HttpStatusCode.BadRequest** when the payload/url contains invalid parameter.
- **HttpStatusCode.NotFound** when the deployment to delete was not found in the database.
- **HttpStatusCode.InternalServerError** when something went wrong during the deletion process.

### Get All Deployments

This endpoint is accessible via: **GET: {{HOST}}/api/deployments**

The response of this endpoint call can be one of the following:
- **HttpStatusCode.Ok** when the searched deployment was retrieved successfully.
- **HttpStatusCode.InternalServerError** when something went wrong during the get process.

### Get Deployment History for an Environment

This endpoint is accesible via: **GET: {{HOST}}/api/deployments/{environmentName}?startDate={startDate}&endDate={endDate}**

The response of this endpoint call can be one of the following:
- **HttpStatusCode.Ok** when the searched deployment were retrieved successfully.
- **HttpStatusCode.BadRequest** when the payload/url contains invalid parameter.
- **HttpStatusCode.NotFound** when the  searched deployment were not found in the database.
- **HttpStatusCode.InternalServerError** when something went wrong during the get process.



