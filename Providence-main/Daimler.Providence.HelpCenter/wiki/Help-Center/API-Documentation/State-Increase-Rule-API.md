# State Increase Rule API
The State Increase Rule API of the Providence Service supports operations for creating, updating and deleting specific rules.

State increase rules can be used to increase the state of specifc alerts after a specified period of time. 
- For example a "warning" can be configuered to be processed as an "error" within the Providence Service if the state "warning" does not change after X minutes.

## API Documentation
<code>
{{HOST}} = https://spp-monitoringservice-X.azurewebsites.net where</code> X stands for the environment the request is made to.

### Create State Increase Rule

This endpoint is accessible via: **POST: {{HOST}}/api/stateIncreaseRules**

Following payload is used to create a state increase rule.
**Notice:** Only the value of **alertName** is optional. All other values are **mandatory**

```
{
    "name": "",
    "description": "",
    "environmentSubscriptionId": "",
    "checkId": "",  
    "alertName": "",
    "componentId": "",
    "triggerTime": int,
    "isActive": bool
}
```

The response of this endpoint call can be one of the following:
-  **HttpStatusCode.Created** when the State Increase Rule was created successfully.
- **HttpStatusCode.BadRequest** when the payload/url contains invalid parameter.
- **HttpStatusCode.NotFound** when the specified environment was not found in the database.
- **HttpStatusCode.InternalServerError** when something went wrong during the creation process.

### Update State Increase Rule

This endpoint is accessible via: **PUT: {{HOST}}/api/stateIncreaseRules/{id}**

Following payload is used to update a state increase rule.

```
{
    "name": "",
    "description": "",
    "environmentSubscriptionId": "",
    "checkId": "",  
    "alertName": "",
    "componentId": "",
    "triggerTime": int,
    "isActive": bool
}
```

The response of this endpoint call can be one of the following:
- **HttpStatusCode.NoContent** when the State Increase Rule was updated successfully.
- **HttpStatusCode.BadRequest** when the payload/url contains invalid parameter.
- **HttpStatusCode.NotFound** when the State Increase Rule to update was not found in the database.
- **HttpStatusCode.InternalServerError** when something went wrong during the update process.

### Delete State Increase Rule

This endpoint is accessible via: **DELETE: {{HOST}}/api/stateIncreaseRules/{id}**

The response of this endpoint call can be one of the following:
- **HttpStatusCode.Accepted** when the State Increase Rule was deleted successfully.
- **HttpStatusCode.BadRequest** when the payload/url contains invalid parameter.
- **HttpStatusCode.NotFound** when the State Increase Rule to delete was not found in the database.
- **HttpStatusCode.InternalServerError** when something went wrong during the deletion process.

### Get State Increase Rule

This endpoint is accessible via: **GET: {{HOST}}/api/stateIncreaseRules/{id}**

The response of this endpoint call can be one of the following:
-  **HttpStatusCode.Ok** when the searched State Increase Rule was retrieved successfully.
- **HttpStatusCode.BadRequest** when the payload/url contains invalid parameter.
- **HttpStatusCode.NotFound** when the searched State Increase Rule was not found in the database.
- **HttpStatusCode.InternalServerError** when something went wrong during the get process.

### Get State Increase Rules

This endpoint is accessible via: **GET: {{HOST}}/api/stateIncreaseRules**

The response of this endpoint call can be one of the following:
-  **HttpStatusCode.Ok** when the searched State Increase Rule  were retrieved successfully.
- **HttpStatusCode.BadRequest** when the payload/url contains invalid parameter.
- **HttpStatusCode.NotFound** when the searched State Increase Rules were not found in the database.
- **HttpStatusCode.InternalServerError** when something went wrong during the get process.