# Alert Ignore API
The Alert Ignore API of the Providence Service supports operations for creating, updating and deleting "Alert Ignore Rules".

Alert ignore rules can be used to mute specifc alerts. For example all alerts with a specific CheckId can be muted to avoid StateTransitions resulting from their occurrence in the Providence Service.

## API Documentation
<code>
{{HOST}} = https://spp-monitoringservice-X.azurewebsites.net</code> where X stands for the environment the request is made to.

### Create Alert Ignore Rule

This endpoint is accessible via: **POST: {{HOST}}/api/alertIgnores**

Following payload is used to create an alert ignore rule.
**Notice:** The value of **environmentSubscriptionId** is mandatory and if **subscriptionId** is set in the **ignoreCondition** property those two values have to be identical.

```json
{
    "name": "",                        
    "environmentSubscriptionId": "", 
    "creationDate": "",
    "expirationDate": "",
    "ignoreCondition": 
    {
       "checkId" : "",
       "componentId" : "",
       "subscriptionId" : "",
       "description" : "",
       "state": "",                                               
       "customField1": "",
       "customField2": "",
       "customField3": "",
       "customField4": "",
       "customField5": "",
       "alertName": ""
    }
}
```

The response of this endpoint call can be one of the following:
- **HttpStatusCode.Created** when the alert ignore was created successfully.
- **HttpStatusCode.BadRequest** when the payload/url contains invalid parameter.
- **HttpStatusCode.NotFound** when the created alert ignore was not found in the database.
- **HttpStatusCode.InternalServerError** when something went wrong during the creation process.

### Update Alert Ignore Rule

This endpoint is accessible via: **PUT: {{HOST}}/api/alertIgnores/{id}**

Following payload is used to create an alert ignore rule.

```json
{
    "name": "",
    "environmentSubscriptionId": "", 
    "creationDate": "",
    "expirationDate": "",
    "ignoreCondition": 
    {
       "checkId" : "",
       "componentId" : "",
       "description" : "",
       "state": "",
       "customField1": "",
       "customField2": "",
       "customField3": "",
       "customField4": "",
       "customField5": "",
       "alertName": ""
    }
}
```
The response of this endpoint call can be one of the following:
- **HttpStatusCode.NoContent** when the alert ignore was updated successfully.
- **HttpStatusCode.BadRequest** when the payload/url contains invalid parameter.
- **HttpStatusCode.NotFound** when the alert ignore to update was not found in the database.
- **HttpStatusCode.InternalServerError** when something went wrong during the update process.

### Delete Alert Ignore Rule

This endpoint is accessible via: **DELETE: {{HOST}}/api/alertIgnores/{id}**

The response of this endpoint call can be one of the following:
- **HttpStatusCode.Accepted** when the alert ignore was deleted successfully.
- **HttpStatusCode.BadRequest** when the payload/url contains invalid parameter.
- **HttpStatusCode.NotFound** when the alert ignore to delete was not found in the database.
- **HttpStatusCode.InternalServerError** when something went wrong during the deletion process.

### Get Alert Ignore Rule

This endpoint is accessible via: **GET: {{HOST}}/api/alertIgnores/{id}**

The response of this endpoint call can be one of the following:
- **HttpStatusCode.Ok** when the searched alert ignore was retrieved successfully.
- **HttpStatusCode.BadRequest** when the payload/url contains invalid parameter.
- **HttpStatusCode.NotFound** when the searched alert ignore was not found in the database.
- **HttpStatusCode.InternalServerError** when something went wrong during the get process.

### Get Alert Ignore Rules

This endpoint is accessible via: **GET: {{HOST}}/api/alertIgnores**

The response of this endpoint call can be one of the following:
- **HttpStatusCode.Ok** when the searched alert ignore  were retrieved successfully.
- **HttpStatusCode.BadRequest** when the payload/url contains invalid parameter.
- **HttpStatusCode.NotFound** when the  searched alert ignores were not found in the database.
- **HttpStatusCode.InternalServerError** when something went wrong during the get process.

